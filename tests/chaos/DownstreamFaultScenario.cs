using System.Diagnostics.Metrics;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.ChaosTests;

/// <summary>
/// W-0042 / P6-3 §8, ARCH-05 §1 (Order Core row) and §4. Sales is unreachable while IVR holds a
/// finished call result. The dangerous outcome is not the outage — it is IVR deciding on its own
/// that the order is confirmed, or dropping the result so nothing downstream ever learns of it.
/// </summary>
[Collection(ChaosTestGroup.Name)]
public sealed class DownstreamFaultScenario(ChaosEnvironment chaos)
{
    [Fact]
    [Trait("TestId", "CHAOS-DOWNSTREAM-01")]
    public async Task WhenSalesIsDownTheResultIsHeldForBoundedRetryAndNeverReportedAsConfirmed()
    {
        string suffix = $"chaos01{Guid.NewGuid():N}"[..14];
        ResultCallbackEntity queued = await SeedReadyCallbackAsync(suffix);

        var observed = new List<(string Instrument, string Outcome)>();
        using MeterListener listener = ListenForCallbackMetrics(observed);

        var options = Options.Create(new CallbackDeliveryOptions
        {
            Enabled = true,
            Provider = Ivr.Contracts.Sales.SalesProviderNames.FakeTargetV1,
            BatchSize = 8,
            MaxRetries = 3,
        });
        var breaker = new CallbackCircuitBreaker(TimeProvider.System, options);
        var dispatcher = new CallbackDispatcher(
            chaos.Services.GetRequiredService<ICallbackOutboxRepository>(),
            new UnreachableSalesTransport(),
            new UnreachableSalesTransport(),
            breaker,
            options,
            TimeProvider.System);

        IReadOnlyList<CallbackDispatchResult> results = await dispatcher.RunBatchAsync();
        CallbackDispatchResult attempt = Assert.Single(results);

        // Bounded retry, and the row still says the order is NOT confirmed. D-04 lets IVR retry a
        // timeout or a 5xx with the same idempotency key; it never lets IVR decide the outcome.
        Assert.Equal("RETRY_PENDING", attempt.DeliveryStatus);
        Assert.True(attempt.Persisted);
        Assert.Equal(1, attempt.RetryCount);

        await using IvrDbContext context = await chaos.DbContextFactory.CreateDbContextAsync();
        ResultCallbackEntity stored = await context.ResultCallbacks
            .SingleAsync(row => row.CallbackId == queued.CallbackId);

        Assert.Equal("RETRY_PENDING", stored.DeliveryStatus);
        Assert.Null(stored.AcknowledgedAt);          // nothing acknowledged it
        Assert.NotNull(stored.NextRetryAt);          // and it is still owed a retry, not dropped
        Assert.Equal("PENDING_CORE_REVALIDATION", stored.ResultState);

        // The signal reached the metric the P6-2 alert reads. The alert firing on this shape is
        // proven separately by IT-SLO-ALERT-01 against the real rule evaluator; what this scenario
        // proves is the half that promtool cannot -- that a real outage moves the real counter.
        Assert.Contains(
            observed,
            entry => entry.Instrument == "ivr_result_callbacks_total"
                && entry.Outcome == "RETRY_PENDING");

        // Running the batch again immediately delivers NOTHING. The backoff is what stops IVR
        // hammering a downstream that is already down -- and it turns out to do so before the
        // circuit breaker is ever consulted, because a message in RETRY_PENDING with a future
        // NextRetryAt is not dequeued at all. Worth asserting explicitly: the first draft of this
        // scenario assumed repeated batches would keep retrying, and it was the system that was
        // right.
        Assert.Empty(await dispatcher.RunBatchAsync());

        // The breaker is the second line, for a burst of distinct deliveries all failing at once.
        // Held, never bypassed: an open breaker defers, it never turns into "assume confirmed".
        for (int extra = 0; extra < options.Value.CircuitFailureThreshold; extra++)
        {
            await SeedReadyCallbackAsync($"{suffix}b{extra}");
        }

        await dispatcher.RunBatchAsync();
        CallbackCircuitState circuit = breaker.Snapshot();
        Assert.True(
            circuit.IsOpen,
            $"The breaker stayed closed after {circuit.ConsecutiveTransientFailures} consecutive "
            + "transient failures against a downstream that answered none of them.");

        // Everything the breaker deferred is still owed a delivery. Nothing was marked confirmed
        // and nothing was dropped on the floor.
        await using IvrDbContext afterBurst = await chaos.DbContextFactory.CreateDbContextAsync();
        Assert.Empty(await afterBurst.ResultCallbacks
            .Where(row => row.CallbackId.StartsWith($"CALLBACK-{suffix}"))
            .Where(row => row.DeliveryStatus == "DELIVERED_ACCEPTED" || row.AcknowledgedAt != null)
            .ToListAsync());
    }

    private static MeterListener ListenForCallbackMetrics(List<(string, string)> observed)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter.Name == IvrTelemetry.ServiceName)
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((instrument, _, tags, _) =>
        {
            foreach (KeyValuePair<string, object?> tag in tags)
            {
                if (tag.Key == TelemetryTags.Outcome)
                {
                    lock (observed)
                    {
                        observed.Add((instrument.Name, tag.Value?.ToString() ?? string.Empty));
                    }
                }
            }
        });
        listener.Start();
        return listener;
    }

    private async Task<ResultCallbackEntity> SeedReadyCallbackAsync(string suffix)
    {
        ConfirmationTaskEntity task = ChaosFixtures.ReadCanonicalTask(suffix);
        CallJobEntity job = ChaosFixtures.CreateJob(
            task,
            task.MaxAttempts,
            task.AttemptOffsetsSecondsJson);
        CallResultEntity result = ChaosFixtures.CreateResult(task, job);

        await using (IvrDbContext context = await chaos.DbContextFactory.CreateDbContextAsync())
        {
            context.AddRange(task, job, result);
            await context.SaveChangesAsync();
        }

        string payload = $"{{\"task_id\":\"{task.TaskId}\",\"result_type\":\"IVR_CONFIRMED\"}}";
        var callback = new ResultCallbackEntity
        {
            CallbackId = $"CALLBACK-{suffix}",
            IvrCallResultId = result.IvrCallResultId,
            TaskId = task.TaskId,
            OfficialOrderId = task.OfficialOrderId,
            IdempotencyKey = $"callback-idem-{suffix}",
            ResultStatus = "IVR_CONFIRMED",
            ResultState = "PENDING_CORE_REVALIDATION",
            DeliveryStatus = "READY",
            RequiresCoreRevalidation = true,
            PayloadJson = payload,
            PayloadSha256 = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(payload))),
            CreatedAt = task.CreatedAt,
        };
        await chaos.Services
            .GetRequiredService<ICallbackOutboxRepository>()
            .EnqueueAsync(callback);
        return callback;
    }

    /// <summary>Sales is there but answering nothing: the shape of a downstream outage.</summary>
    private sealed class UnreachableSalesTransport
        : ITargetV1CallbackTransport, ICurrentGoldenHourCallbackTransport
    {
        public Task<CallbackTransportResult> SendAsync(
            CallbackOutboxMessage message,
            CancellationToken cancellationToken = default) =>
            throw new HttpRequestException("Simulated Sales outage (P6-3 fault injection).");
    }
}

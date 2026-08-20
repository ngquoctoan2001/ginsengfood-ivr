using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.ChaosTests;

/// <summary>
/// W-0042 / P6-3 §8. Recovery is the half that is usually skipped, and it is where duplicates come
/// from: work that looked lost during the outage gets redone afterwards. The question is whether
/// the backlog drains exactly once.
/// </summary>
[Collection(ChaosTestGroup.Name)]
public sealed class RecoveryScenario(ChaosEnvironment chaos)
{
    [Fact]
    [Trait("TestId", "CHAOS-RECOVERY-04")]
    public async Task TheBacklogDrainsExactlyOnceAfterTheLinkReturns()
    {
        string suffix = $"rec{Guid.NewGuid():N}"[..12];
        ResultCallbackEntity queued = await ChaosFixtures.SeedReadyCallbackAsync(chaos, suffix);

        var options = Options.Create(new CallbackDeliveryOptions
        {
            Enabled = true,
            Provider = Ivr.Contracts.Sales.SalesProviderNames.FakeTargetV1,
            BatchSize = 8,
            MaxRetries = 3,
        });
        var transport = new CountingTransport();
        CallbackDispatcher Dispatcher() => new(
            chaos.Services.GetRequiredService<ICallbackOutboxRepository>(),
            transport,
            transport,
            new CallbackCircuitBreaker(TimeProvider.System, options),
            options,
            TimeProvider.System);

        // ---- fault: the database link goes away while work is queued ----
        await chaos.CutDatabaseLinkAsync();

        await Assert.ThrowsAnyAsync<Exception>(() => Dispatcher().RunBatchAsync());

        // Nothing was sent while the store was unreachable. Sending first and recording later is
        // exactly how a duplicate is born: the send survives the outage, the record does not.
        Assert.Equal(0, transport.Sends);

        // ---- recovery ----
        await chaos.RestoreDatabaseLinkAsync();
        Assert.True(await EventuallyQueryableAsync(), "The store never came back.");

        IReadOnlyList<CallbackDispatchResult> drained = await Dispatcher().RunBatchAsync();
        CallbackDispatchResult delivered = Assert.Single(drained);
        Assert.Equal("DELIVERED_ACCEPTED", delivered.DeliveryStatus);
        Assert.Equal(1, transport.Sends);

        // Draining again sends nothing more. The backlog is empty because the work is finished,
        // not because the second run happened to find the row leased.
        Assert.Empty(await Dispatcher().RunBatchAsync());
        Assert.Equal(1, transport.Sends);

        await using IvrDbContext context = await chaos.DbContextFactory.CreateDbContextAsync();
        ResultCallbackEntity stored = await context.ResultCallbacks
            .SingleAsync(row => row.CallbackId == queued.CallbackId);
        Assert.Equal("DELIVERED_ACCEPTED", stored.DeliveryStatus);
        Assert.NotNull(stored.AcknowledgedAt);

        // The idempotency key never changed across the outage, so a redelivery Sales did receive
        // would be recognised as the same result rather than as a second confirmation (D-04).
        Assert.Equal($"callback-idem-{suffix}", stored.IdempotencyKey);
        Assert.Equal(1, await context.ResultCallbacks
            .CountAsync(row => row.IdempotencyKey == $"callback-idem-{suffix}"));
    }

    private async Task<bool> EventuallyQueryableAsync()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                await using IvrDbContext probe = await chaos.DbContextFactory.CreateDbContextAsync();
                await probe.ResultCallbacks.CountAsync();
                return true;
            }
            catch (Exception)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        return false;
    }

    /// <summary>Sales is healthy and counts what it received, so a duplicate cannot hide.</summary>
    private sealed class CountingTransport
        : ITargetV1CallbackTransport, ICurrentGoldenHourCallbackTransport
    {
        private int sends;

        public int Sends => Volatile.Read(ref sends);

        public Task<CallbackTransportResult> SendAsync(
            CallbackOutboxMessage message,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref sends);
            return Task.FromResult(new CallbackTransportResult(
                CallbackTransportOutcome.Accepted,
                202,
                "ACCEPTED",
                "ACCEPTED"));
        }
    }
}

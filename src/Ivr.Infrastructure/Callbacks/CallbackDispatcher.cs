using System.Security.Cryptography;
using System.Text;
using Ivr.Contracts.Sales;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Callbacks;

public sealed record CallbackDispatchResult(
    string CallbackId,
    string DeliveryStatus,
    string ResponseCode,
    int RetryCount,
    bool Persisted);

public sealed class CallbackDispatcher(
    ICallbackOutboxRepository outbox,
    ITargetV1CallbackTransport targetTransport,
    ICurrentGoldenHourCallbackTransport currentTransport,
    CallbackCircuitBreaker circuitBreaker,
    IOptions<CallbackDeliveryOptions> options,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<CallbackDispatchResult>> RunBatchAsync(
        CancellationToken cancellationToken = default)
    {
        CallbackDeliveryOptions settings = options.Value;
        if (!settings.Enabled)
        {
            return [];
        }

        if (!SalesProviderNames.TryParse(settings.Provider, out SalesProviderKind provider))
        {
            throw new InvalidOperationException("Callback provider is unsupported.");
        }

        IReadOnlyList<CallbackOutboxMessage> messages = await outbox.DequeueReadyAsync(
            settings.BatchSize,
            TimeSpan.FromSeconds(settings.LeaseDurationSeconds),
            cancellationToken);
        List<CallbackDispatchResult> results = new(messages.Count);
        foreach (CallbackOutboxMessage message in messages)
        {
            // W-0040 / P6-1 §6.2. One span per delivery, carrying the correlation id the task
            // arrived with, so an investigation can follow a single order across the boundary
            // instead of guessing which of a batch's log lines belong together.
            TraceContextSnapshot? traceContext = TraceContextSnapshot.FromPersisted(
                message.TraceParent,
                message.TraceState);
            using System.Diagnostics.Activity? span = IvrTelemetry.StartWorkflowSpan(
                "ivr.callback.deliver",
                System.Diagnostics.ActivityKind.Consumer,
                traceContext,
                linkCurrent: false,
                (TelemetryTags.CorrelationId, message.CorrelationId),
                (TelemetryTags.TaskId, message.TaskId),
                (TelemetryTags.CallbackId, message.CallbackId),
                (TelemetryTags.Program, message.ProgramCode));
            long startedTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            CallbackTransportResult transportResult;
            if (!circuitBreaker.TryEnter())
            {
                transportResult = new CallbackTransportResult(
                    CallbackTransportOutcome.TransientFailure,
                    null,
                    "CALLBACK_CIRCUIT_OPEN",
                    "CALLBACK_CIRCUIT_OPEN");
                CallbackDeliveryUpdate deferred = CreateCircuitOpenUpdate(message, settings);
                bool persisted = await outbox.CompleteDeliveryAsync(
                    message.CallbackId,
                    message.LeaseToken,
                    deferred,
                    cancellationToken);
                results.Add(new CallbackDispatchResult(
                    message.CallbackId,
                    deferred.DeliveryStatus,
                    transportResult.Code,
                    deferred.RetryCount,
                    persisted));
                span?.SetTag(TelemetryTags.Outcome, deferred.DeliveryStatus);
                span?.SetStatus(System.Diagnostics.ActivityStatusCode.Error);
                continue;
            }

            try
            {
                SalesCallbackContractKind contract = SalesCallbackContractSelector.Select(
                    provider,
                    message.ProgramCode);
                transportResult = contract switch
                {
                    SalesCallbackContractKind.TargetV1 => await targetTransport.SendAsync(
                        message,
                        cancellationToken),
                    SalesCallbackContractKind.CurrentGoldenHourCompat =>
                        await currentTransport.SendAsync(message, cancellationToken),
                    _ => throw new InvalidOperationException("Callback contract is unsupported."),
                };
            }
            catch (InvalidOperationException)
            {
                transportResult = new CallbackTransportResult(
                    CallbackTransportOutcome.Invalid,
                    null,
                    "CALLBACK_ADAPTER_SELECTION_REJECTED",
                    "CALLBACK_ADAPTER_SELECTION_REJECTED");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                circuitBreaker.RecordProbeAborted();
                throw;
            }
            catch (Exception)
            {
                transportResult = new CallbackTransportResult(
                    CallbackTransportOutcome.TransientFailure,
                    null,
                    "CALLBACK_TRANSPORT_UNEXPECTED_FAILURE",
                    "CALLBACK_TRANSPORT_UNEXPECTED_FAILURE");
            }

            if (transportResult.Outcome == CallbackTransportOutcome.TransientFailure)
            {
                circuitBreaker.RecordTransientFailure();
            }
            else
            {
                // The breaker represents transient reachability only. Any terminal result,
                // including a local validation failure without an HTTP status, releases a
                // half-open probe and resets the transient failure streak.
                circuitBreaker.RecordSuccess();
            }

            CallbackDeliveryUpdate update = CreateUpdate(message, transportResult, settings);
            bool saved = await outbox.CompleteDeliveryAsync(
                message.CallbackId,
                message.LeaseToken,
                update,
                cancellationToken);
            results.Add(new CallbackDispatchResult(
                message.CallbackId,
                update.DeliveryStatus,
                transportResult.Code,
                update.RetryCount,
                saved));

            // Measured from the real elapsed time of this delivery, not estimated: P6-1 §11
            // forbids reporting a KPI that was inferred rather than observed.
            IvrTelemetry.RecordCallback(
                System.Diagnostics.Stopwatch.GetElapsedTime(startedTicks).TotalSeconds,
                (TelemetryTags.Program, message.ProgramCode),
                (TelemetryTags.Outcome, update.DeliveryStatus),
                (TelemetryTags.AckCode, transportResult.Code ?? "NONE"),
                (TelemetryTags.HttpStatus, transportResult.HttpStatus ?? 0));
            span?.SetTag(TelemetryTags.Outcome, update.DeliveryStatus);
            if (transportResult.Outcome is CallbackTransportOutcome.TransientFailure
                or CallbackTransportOutcome.Invalid
                or CallbackTransportOutcome.AuthRejected)
            {
                span?.SetStatus(System.Diagnostics.ActivityStatusCode.Error);
            }
        }

        return results;
    }

    private CallbackDeliveryUpdate CreateCircuitOpenUpdate(
        CallbackOutboxMessage message,
        CallbackDeliveryOptions settings) => new(
            "RETRY_PENDING",
            null,
            "CALLBACK_CIRCUIT_OPEN",
            "CALLBACK_CIRCUIT_OPEN",
            message.RetryCount,
            timeProvider.GetUtcNow().AddSeconds(settings.CircuitOpenSeconds),
            false,
            false);

    private CallbackDeliveryUpdate CreateUpdate(
        CallbackOutboxMessage message,
        CallbackTransportResult result,
        CallbackDeliveryOptions settings)
    {
        bool acknowledged = result.HttpStatus is not null;
        return result.Outcome switch
        {
            CallbackTransportOutcome.Accepted or CallbackTransportOutcome.DuplicateAccepted =>
                Terminal("DELIVERED_ACCEPTED", result, message.RetryCount, acknowledged, false),
            CallbackTransportOutcome.BlockedByCore =>
                Terminal("DELIVERED_BLOCKED", result, message.RetryCount, true, true),
            CallbackTransportOutcome.ReviewRequired =>
                Terminal("DELIVERED_REVIEW", result, message.RetryCount, true, true),
            CallbackTransportOutcome.RejectedStale =>
                Terminal("REJECTED_STALE", result, message.RetryCount, true, true),
            CallbackTransportOutcome.IdempotencyConflict =>
                Terminal("IDEMPOTENCY_CONFLICT", result, message.RetryCount, true, true),
            CallbackTransportOutcome.Invalid =>
                Terminal("INVALID_DEAD_LETTER", result, message.RetryCount, acknowledged, true),
            CallbackTransportOutcome.AuthRejected =>
                Terminal("AUTH_REJECTED", result, message.RetryCount, acknowledged, true),
            CallbackTransportOutcome.TransientFailure => Retry(message, result, settings),
            _ => throw new InvalidOperationException("Callback outcome is unsupported."),
        };
    }

    private CallbackDeliveryUpdate Retry(
        CallbackOutboxMessage message,
        CallbackTransportResult result,
        CallbackDeliveryOptions settings)
    {
        int nextRetryCount = checked(message.RetryCount + 1);
        if (nextRetryCount > settings.MaxRetries)
        {
            return Terminal(
                "RETRY_EXHAUSTED",
                result,
                message.RetryCount,
                false,
                true);
        }

        TimeSpan retryDelay = ComputeRetryDelay(
            message.CallbackId,
            nextRetryCount,
            settings);
        if (result.RetryAfter is { } serverDelay && serverDelay > retryDelay)
        {
            retryDelay = serverDelay;
        }

        return new CallbackDeliveryUpdate(
            "RETRY_PENDING",
            result.HttpStatus,
            result.Code,
            result.SafeError,
            nextRetryCount,
            timeProvider.GetUtcNow().Add(retryDelay),
            false,
            false);
    }

    private static CallbackDeliveryUpdate Terminal(
        string status,
        CallbackTransportResult result,
        int retryCount,
        bool acknowledged,
        bool review) => new(
            status,
            result.HttpStatus,
            result.Code,
            result.SafeError,
            retryCount,
            null,
            acknowledged,
            review);

    internal static TimeSpan ComputeRetryDelay(
        string callbackId,
        int retryNumber,
        CallbackDeliveryOptions settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callbackId);
        ArgumentOutOfRangeException.ThrowIfLessThan(retryNumber, 1);

        long multiplier = 1L << Math.Min(retryNumber - 1, 20);
        long exponential = Math.Min(
            checked((long)settings.BaseRetryDelayMilliseconds * multiplier),
            settings.MaxRetryDelayMilliseconds);
        int jitter = 0;
        if (settings.MaxJitterMilliseconds > 0)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(string.Concat(
                callbackId,
                ":",
                retryNumber.ToString(System.Globalization.CultureInfo.InvariantCulture))));
            uint sample = BitConverter.ToUInt32(hash, 0);
            jitter = (int)(sample % (uint)(settings.MaxJitterMilliseconds + 1));
        }

        return TimeSpan.FromMilliseconds(Math.Min(
            exponential + jitter,
            settings.MaxRetryDelayMilliseconds));
    }
}

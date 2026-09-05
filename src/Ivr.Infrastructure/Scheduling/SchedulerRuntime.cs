using Microsoft.Extensions.Options;
using Ivr.Infrastructure.Observability;

namespace Ivr.Infrastructure.Scheduling;

/// <param name="CallingWindowOpen">
/// W-0197. Whether the hour of day permits a call. Reported separately from
/// <paramref name="DispatchGatewayReady"/> on purpose: "the gateway is not ready" and "it is
/// half past three in the morning" are different facts, and collapsing them would let a
/// perfectly healthy night look like a broken telephony stack.
/// </param>
public sealed record SchedulerRunResult(
    bool Enabled,
    bool DispatchGatewayReady,
    int QuarantinedLeases,
    int ClosedMissedDeadlines,
    bool DispatchClaimed,
    bool CallingWindowOpen = true);

public interface ISchedulerDispatchGateway
{
    public bool IsReady { get; }

    public Task DispatchAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default);
}

public sealed class UnavailableSchedulerDispatchGateway : ISchedulerDispatchGateway
{
    public bool IsReady => false;

    public Task DispatchAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        cancellationToken.ThrowIfCancellationRequested();
        throw new InvalidOperationException(
            "Scheduler dispatch gateway is unavailable until P2-4 supplies a safe adapter.");
    }
}

public interface ISchedulerRuntime
{
    public Task<SchedulerRunResult> RunOnceAsync(
        string workerId,
        CancellationToken cancellationToken = default);
}

public sealed class SchedulerRuntime(
    IPostgresSchedulerStore store,
    ISchedulerDispatchGateway dispatchGateway,
    IOptions<SchedulerOptions> options,
    SchedulerExecutionContext executionContext,
    CallingWindow callingWindow,
    TimeProvider timeProvider) : ISchedulerRuntime
{
    public async Task<SchedulerRunResult> RunOnceAsync(
        string workerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        SchedulerOptions snapshot = options.Value;
        if (!snapshot.Enabled)
        {
            return new SchedulerRunResult(false, dispatchGateway.IsReady, 0, 0, false);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        int quarantined = await store.QuarantineExpiredLeasesAsync(
            now,
            TimeSpan.FromSeconds(snapshot.RecoveryQuarantineSeconds),
            snapshot.ClaimBatchSize,
            cancellationToken).ConfigureAwait(false);
        int closed = await store.CloseMissedDeadlinesAsync(
            now,
            snapshot.ClaimBatchSize,
            cancellationToken).ConfigureAwait(false);
        if (!dispatchGateway.IsReady)
        {
            return new SchedulerRunResult(true, false, quarantined, closed, false);
        }

        // W-0197 / OD-V1-16. The hour gate sits AFTER lease recovery and missed-deadline closing
        // and BEFORE claiming a dial, and that order is the design.
        //
        // A window that closed at nine in the evening must not also stop the scheduler noticing
        // that a lease died or that a confirmation window expired overnight - those are
        // bookkeeping, they wake nobody, and suspending them would mean every morning started
        // with a backlog of jobs that had silently missed their deadline hours earlier. Only
        // dialling stops.
        CallingWindowDecision window = callingWindow.Evaluate(now);
        if (!window.Open)
        {
            return new SchedulerRunResult(true, true, quarantined, closed, false, false);
        }

        SchedulerDispatchLease? lease = await store.TryClaimDueDispatchAsync(
            workerId,
            executionContext.ExecutionMode,
            TimeSpan.FromSeconds(snapshot.LeaseDurationSeconds),
            cancellationToken).ConfigureAwait(false);
        if (lease is null)
        {
            return new SchedulerRunResult(true, true, quarantined, closed, false);
        }

        TraceContextSnapshot? traceContext = TraceContextSnapshot.FromPersisted(
            lease.TraceParent,
            lease.TraceState);
        using System.Diagnostics.Activity? span = IvrTelemetry.StartWorkflowSpan(
            "ivr.scheduler.dispatch",
            System.Diagnostics.ActivityKind.Consumer,
            traceContext,
            linkCurrent: false,
            (TelemetryTags.CorrelationId, lease.CorrelationId),
            (TelemetryTags.TaskId, lease.TaskId),
            (TelemetryTags.JobId, lease.JobId),
            (TelemetryTags.AttemptId, lease.AttemptId),
            (TelemetryTags.AttemptNumber, lease.AttemptNumber),
            (TelemetryTags.SimProvider, lease.ProviderName));
        try
        {
            await dispatchGateway.DispatchAsync(lease, cancellationToken).ConfigureAwait(false);
            span?.SetTag(TelemetryTags.Outcome, "DISPATCHED");
        }
        catch
        {
            span?.SetStatus(System.Diagnostics.ActivityStatusCode.Error);
            throw;
        }

        return new SchedulerRunResult(true, true, quarantined, closed, true);
    }
}

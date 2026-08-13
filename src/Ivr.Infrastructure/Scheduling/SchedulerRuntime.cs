using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Scheduling;

public sealed record SchedulerRunResult(
    bool Enabled,
    bool DispatchGatewayReady,
    int QuarantinedLeases,
    int ClosedMissedDeadlines,
    bool DispatchClaimed);

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

        SchedulerDispatchLease? lease = await store.TryClaimDueDispatchAsync(
            workerId,
            executionContext.ExecutionMode,
            TimeSpan.FromSeconds(snapshot.LeaseDurationSeconds),
            cancellationToken).ConfigureAwait(false);
        if (lease is null)
        {
            return new SchedulerRunResult(true, true, quarantined, closed, false);
        }

        await dispatchGateway.DispatchAsync(lease, cancellationToken).ConfigureAwait(false);
        return new SchedulerRunResult(true, true, quarantined, closed, true);
    }
}

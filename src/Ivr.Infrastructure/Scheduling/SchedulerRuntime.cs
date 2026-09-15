using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Scheduling;

/// <param name="CallingWindowOpen">
/// W-0198. Whether the hour of day permits a call. Reported separately from
/// <paramref name="DispatchGatewayReady"/> on purpose: "the gateway is not ready" and "it is
/// half past three in the morning" are different facts, and collapsing them would let a
/// perfectly healthy night look like a broken telephony stack.
/// </param>
/// <param name="CallingWindowOpensAt">
/// W-0214. When the window next opens, carried out only because nothing inside the runtime can
/// say it. A closed window used to produce no output at all: the three counters are zero, the
/// host logged nothing, and the only visible symptom was jobs expiring hours later with no
/// attempt against them. An operator could read that as a broken dialler for a whole night.
/// Null whenever the window is open, or when the run never reached the hour gate.
/// </param>
/// <param name="DispatchClaimed">
/// Whether this pass claimed at least one lease. Kept as a bool because that is what the host
/// logs and what four tests assert; <paramref name="DispatchesStarted"/> carries the count.
/// </param>
/// <param name="DispatchesStarted">
/// SIP-05. Calls this pass started. Started, not finished: a pass now returns while the calls it
/// began are still ringing, which is the whole point of the change.
/// </param>
/// <param name="ActiveDispatches">
/// SIP-05. Calls this worker was holding when the pass ended, including ones started by earlier
/// passes. This is the number that must stay at or under the configured ceiling.
/// </param>
/// <param name="DispatchFailures">
/// SIP-05. Failures from calls started by earlier passes, reported once and then forgotten. Before
/// the change a failing dispatch threw out of the pass and the loop logged it; a call that outlives
/// its pass has no stack to throw up, so it is carried here instead.
/// </param>
/// <param name="DispatchSheddingUntil">
/// SIP-05. When new calls will be admitted again after consecutive dispatch failures, or null
/// when they are being admitted now. Distinct from <paramref name="CallingWindowOpen"/>, which is
/// also a reason nothing dials: one is the hour of day and the other is a route that keeps
/// failing, and an operator needs to be told which.
/// </param>
public sealed record SchedulerRunResult(
    bool Enabled,
    bool DispatchGatewayReady,
    int QuarantinedLeases,
    int ClosedMissedDeadlines,
    bool DispatchClaimed,
    bool CallingWindowOpen = true,
    DateTimeOffset? CallingWindowOpensAt = null,
    int DispatchesStarted = 0,
    int ActiveDispatches = 0,
    IReadOnlyList<SchedulerDispatchFailure>? DispatchFailures = null,
    DateTimeOffset? DispatchSheddingUntil = null);

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

/// <summary>
/// One pass of the scheduler: recover, close, then start as many calls as the ceiling and the
/// start rate allow.
/// <para>
/// SIP-05 changed what a pass is. It used to end by awaiting the call it had just claimed, which
/// made the pass as long as the call - up to audio 120s plus ring 30s plus DTMF 15s - and had two
/// consequences neither of which was about capacity. A worker could hold exactly one call however
/// many channels were free, and <c>WorkerLiveness</c>, which ticks only when a pass returns and
/// calls a loop stale after max(3 x poll, 30s), reported a wedged scheduler during every ordinary
/// long call. A pass now returns once the calls are started.
/// </para>
/// </summary>
public sealed class SchedulerRuntime(
    IPostgresSchedulerStore store,
    ISchedulerDispatchGateway dispatchGateway,
    SchedulerDispatchPump pump,
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
            // Reported even here. A scheduler switched off while calls were up still has those
            // calls, and a run result that said zero would be read as "nothing is running".
            return new SchedulerRunResult(
                false,
                dispatchGateway.IsReady,
                0,
                0,
                false,
                ActiveDispatches: pump.Active,
                DispatchFailures: pump.TakeFailures(),
                DispatchSheddingUntil: pump.SheddingUntil);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        int quarantined = await store.QuarantineExpiredLeasesAsync(
            now,
            TimeSpan.FromSeconds(snapshot.RecoveryQuarantineSeconds),
            snapshot.ClaimBatchSize,
            cancellationToken);
        int closed = await store.CloseMissedDeadlinesAsync(
            now,
            snapshot.ClaimBatchSize,
            cancellationToken);

        // Read once, here, so that every return below reports them. Taken after the maintenance
        // work rather than before it because that is the order an operator reads the result in:
        // what the pass recovered, then what the calls it started earlier did.
        IReadOnlyList<SchedulerDispatchFailure> failures = pump.TakeFailures();
        if (!dispatchGateway.IsReady)
        {
            return new SchedulerRunResult(
                true,
                false,
                quarantined,
                closed,
                false,
                ActiveDispatches: pump.Active,
                DispatchFailures: failures,
                DispatchSheddingUntil: pump.SheddingUntil);
        }

        // W-0198 / OD-V1-16. The hour gate sits AFTER lease recovery and missed-deadline closing
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
            return new SchedulerRunResult(
                true,
                true,
                quarantined,
                closed,
                false,
                false,
                window.OpensAt,
                ActiveDispatches: pump.Active,
                DispatchFailures: failures,
                DispatchSheddingUntil: pump.SheddingUntil);
        }

        // Reserve, then claim. Never the other way round: a lease claimed with nowhere to run it
        // leaves an ivr_sim_channels row RESERVED and a fencing generation spent, and nothing puts
        // it back until QuarantineExpiredLeasesAsync notices ten minutes later - on a job that was
        // due the moment it was claimed.
        int started = 0;
        TimeSpan leaseDuration = TimeSpan.FromSeconds(snapshot.LeaseDurationSeconds);
        while (!cancellationToken.IsCancellationRequested && pump.TryReserve())
        {
            bool dispatchStarted = false;
            try
            {
                SchedulerDispatchLease? lease = await store.TryClaimDueDispatchAsync(
                    workerId,
                    executionContext.ExecutionMode,
                    leaseDuration,
                    cancellationToken);

                // Nothing due, or the shared channel pool is empty. Either way this worker stops
                // asking until the next pass; the pool is the answer that binds across workers,
                // and spinning on it here would only add load to the database that is refusing.
                if (lease is null)
                {
                    break;
                }

                pump.Start(lease, dispatchGateway.DispatchAsync, cancellationToken);
                dispatchStarted = true;
                started++;
            }
            finally
            {
                // The reservation is either handed to a call or given back. A claim that throws
                // must not leave a slot held by nothing: the worker would shrink its own ceiling
                // by one on every database blip, permanently, with no path back up.
                if (!dispatchStarted)
                {
                    pump.Release();
                }
            }
        }

        return new SchedulerRunResult(
            true,
            true,
            quarantined,
            closed,
            started > 0,
            true,
            null,
            started,
            pump.Active,
            failures,
            pump.SheddingUntil);
    }
}

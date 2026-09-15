using System.Collections.Concurrent;
using System.Diagnostics;
using Ivr.Infrastructure.Observability;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Scheduling;

/// <summary>
/// A dispatch that threw, held until the next scheduler pass reports it.
/// <para>
/// Until SIP-05 a failing dispatch threw out of <c>SchedulerRuntime.RunOnceAsync</c>, up through
/// <c>PollingJobHost</c>, and was logged there with a failure streak. Once a call outlives the pass
/// that started it there is no longer a stack to throw up, so the failure is carried out instead of
/// lost: an unobserved <see cref="Task"/> exception is the silent version of this, and silence is
/// what the plan rules out when it asks for dispatch tasks to be watched and their errors kept.
/// </para>
/// </summary>
public sealed record SchedulerDispatchFailure(
    string JobId,
    string AttemptId,
    Exception Exception);

/// <summary>
/// Holds the calls a worker has started but not yet finished, and decides whether it may start
/// another. See SIP-05 in
/// <c>plan/ivr-orther/mobile-sip-trunk-production-32-channels-plan-2026-09-15.md</c>.
/// <para>
/// <b>This ceiling is process-local and is not the system-wide bound.</b> The bound that actually
/// holds across workers is the channel pool in <c>ivr_sim_channels</c>: every claim in
/// <see cref="PostgresSchedulerStore.TryClaimDueDispatchAsync"/> reserves one IDLE row under
/// <c>FOR UPDATE ... SKIP LOCKED</c> and returns null when none is free, so two workers cannot
/// both take the same channel however they are configured. What this class adds is a per-process
/// limit on how much of that pool one worker will hold at once, plus a start rate. Written down
/// because a green concurrency test proves the local limit and says nothing about the shared one.
/// </para>
/// <para>
/// A counter under a lock rather than a <see cref="SemaphoreSlim"/>, because the ceiling has to be
/// able to fall. Lowering <see cref="SchedulerOptions.MaxConcurrentDispatches"/> from 32 to 8 must
/// stop new admissions until the active count has drained below the new figure, and must not cut
/// off calls that are already connected to a customer; a semaphore cannot shrink, and draining its
/// permits would mean deciding which live call to drop.
/// </para>
/// </summary>
public sealed class SchedulerDispatchPump(
    IOptions<SchedulerOptions> options,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan RateWindow = TimeSpan.FromSeconds(1);

    private readonly Lock gate = new();
    private readonly ConcurrentDictionary<long, Task> inFlight = new();
    private readonly ConcurrentQueue<SchedulerDispatchFailure> failures = new();

    private long nextDispatchId;
    private int active;
    private DateTimeOffset rateWindowStartedAt = DateTimeOffset.MinValue;
    private int startsInRateWindow;

    /// <summary>Calls started and not yet finished.</summary>
    public int Active
    {
        get
        {
            lock (gate)
            {
                return active;
            }
        }
    }

    /// <summary>
    /// Takes one concurrency slot and one start-rate token together, or refuses.
    /// <para>
    /// Both are taken before the caller claims from the database and not after, because a lease
    /// claimed with nowhere to run it is worse than a lease left alone: the row is already
    /// RESERVED with a fencing generation burnt, and nothing releases it until
    /// <c>QuarantineExpiredLeasesAsync</c> notices, which is RecoveryQuarantineSeconds - ten
    /// minutes by default - after a job that was due now.
    /// </para>
    /// </summary>
    public bool TryReserve()
    {
        SchedulerOptions snapshot = options.Value;
        DateTimeOffset now = timeProvider.GetUtcNow();
        lock (gate)
        {
            if (active >= snapshot.MaxConcurrentDispatches)
            {
                return false;
            }

            // A fixed window, not a sliding one: the boundary lets up to twice the configured rate
            // fall inside one arbitrary second. Named here rather than smoothed away, because the
            // number this protects is a carrier calls-per-second allowance and the honest reading
            // is "close to the limit", not "provably under it". A sliding window belongs with the
            // trunk pool work, where the allowance becomes a contractual figure.
            if (now - rateWindowStartedAt >= RateWindow)
            {
                rateWindowStartedAt = now;
                startsInRateWindow = 0;
            }

            if (startsInRateWindow >= snapshot.MaxCallStartsPerSecond)
            {
                return false;
            }

            active++;
            startsInRateWindow++;
            return true;
        }
    }

    /// <summary>
    /// Gives back a reservation that found no work to run.
    /// <para>
    /// The concurrency slot comes back; the start-rate token does not. Refunding it would need the
    /// window it was taken from to still be the current one, and getting that wrong hands out more
    /// starts per second than configured. Not refunding it can only cost one start in a second
    /// where the queue was empty anyway, which is the direction that cannot breach an allowance.
    /// </para>
    /// </summary>
    public void Release()
    {
        lock (gate)
        {
            if (active > 0)
            {
                active--;
            }
        }
    }

    /// <summary>
    /// Starts a dispatch on a reservation already taken by <see cref="TryReserve"/>, and returns
    /// without waiting for the call. The slot is released when the call ends, not when this
    /// returns.
    /// </summary>
    public void Start(
        SchedulerDispatchLease lease,
        Func<SchedulerDispatchLease, CancellationToken, Task> dispatch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(dispatch);
        long id = Interlocked.Increment(ref nextDispatchId);
        Task task = RunAsync(lease, dispatch, cancellationToken);

        // Registered first, then de-registered by a continuation on the registered task. Removing
        // from inside RunAsync would race its own registration: a dispatch that completed
        // synchronously would remove an entry that had not been added yet, and then leak it.
        inFlight[id] = task;
        _ = task.ContinueWith(
            _ => inFlight.TryRemove(id, out Task? _),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    /// <summary>
    /// Hands over the failures recorded since the last call and forgets them, so one pass reports
    /// each failure exactly once.
    /// </summary>
    public IReadOnlyList<SchedulerDispatchFailure> TakeFailures()
    {
        if (failures.IsEmpty)
        {
            return [];
        }

        List<SchedulerDispatchFailure> taken = [];
        while (failures.TryDequeue(out SchedulerDispatchFailure? failure))
        {
            taken.Add(failure);
        }

        return taken;
    }

    /// <summary>
    /// Waits for the calls already started, up to <paramref name="timeout"/>. False means the
    /// timeout won and calls are still running.
    /// <para>
    /// Waiting only. Ending a call that is still up is termination, which has its own contract and
    /// its own measured latency; a drain that quietly hung up on a customer mid-sentence would be
    /// a different behaviour wearing this method name.
    /// </para>
    /// </summary>
    public async Task<bool> DrainAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        Task[] pending = [.. inFlight.Values];
        if (pending.Length == 0)
        {
            return true;
        }

        try
        {
            await Task.WhenAll(pending).WaitAsync(timeout, timeProvider, cancellationToken);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    private async Task RunAsync(
        SchedulerDispatchLease lease,
        Func<SchedulerDispatchLease, CancellationToken, Task> dispatch,
        CancellationToken cancellationToken)
    {
        TraceContextSnapshot? traceContext = TraceContextSnapshot.FromPersisted(
            lease.TraceParent,
            lease.TraceState);

        // The span moved here with the call it measures. Left in RunOnceAsync it would have closed
        // the moment the pass returned, reporting every dial as lasting microseconds.
        using Activity? span = IvrTelemetry.StartWorkflowSpan(
            "ivr.scheduler.dispatch",
            ActivityKind.Consumer,
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
            await dispatch(lease, cancellationToken);
            span?.SetTag(TelemetryTags.Outcome, "DISPATCHED");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutdown, not a fault. Recording it would put a line in the log for every call in
            // flight on every deploy, which is the noise that hides the one real failure.
            span?.SetTag(TelemetryTags.Outcome, "CANCELLED");
        }
#pragma warning disable CA1031 // A failed call must not take the worker or the other calls down.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            span?.SetStatus(ActivityStatusCode.Error);
            failures.Enqueue(
                new SchedulerDispatchFailure(lease.JobId, lease.AttemptId, exception));
        }
        finally
        {
            Release();
        }
    }
}

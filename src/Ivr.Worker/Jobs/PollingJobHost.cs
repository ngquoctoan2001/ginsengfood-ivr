using Ivr.Infrastructure.Resilience;

namespace Ivr.Worker.Jobs;

/// <summary>
/// The loop every polling worker runs, written once.
/// <para>
/// Four hosts carried their own copy of it — analytics, callback delivery, normalisation and the
/// scheduler — including one comment repeated word for word in three of them. The copies had
/// already drifted twice by the time this was written: one used <c>do/while</c> where the others
/// used <c>while</c>, and only one passed <see cref="TimeProvider"/> to its
/// <see cref="PeriodicTimer"/>, which left the other three untestable against a fake clock until
/// <c>W-0256</c> fixed them one at a time.
/// </para>
/// <para>
/// What is shared is not the line count — that saving is small — but the answers to questions the
/// copies could each answer differently: when a failure backs off, whether a tick is recorded
/// before or after the backoff, which exceptions end the loop rather than being reported, and what
/// a cancelled wait means. Those live here now, so a fifth host inherits them instead of being
/// reviewed for them.
/// </para>
/// <para>
/// <b>Not for a one-shot host.</b> <c>RetentionJobHost</c> runs a single pass and returns; it has
/// no timer, no liveness registration and no failure loop, and forcing it through this base would
/// mean inventing a period it does not have.
/// </para>
/// </summary>
internal abstract class PollingJobHost(WorkerLiveness liveness, TimeProvider timeProvider)
    : BackgroundService
{
    /// <summary>The name this loop is registered and reported under. Must be stable.</summary>
    protected abstract string LoopName { get; }

    /// <summary>Whether configuration has this loop turned on.</summary>
    protected abstract bool IsEnabled { get; }

    /// <summary>How long between passes.</summary>
    protected abstract TimeSpan Period { get; }

    /// <summary>
    /// Whether a disabled loop should return instead of turning.
    /// <para>
    /// True for every host but the scheduler, whose enable gate lives inside
    /// <c>SchedulerRuntime.RunOnceAsync</c> rather than out here: with the scheduler off its loop
    /// still turns and does nothing on each pass, so returning early would skip the recovery and
    /// deadline work that runs regardless of whether dialling is allowed.
    /// </para>
    /// </summary>
    protected virtual bool StopWhenDisabled => true;

    /// <summary>One pass. Anything it throws is reported and retried after a backoff.</summary>
    protected abstract Task RunOnceAsync(CancellationToken cancellationToken);

    /// <summary>Called once when configuration has the loop turned off.</summary>
    protected abstract void OnDisabled();

    /// <summary>
    /// Called for every failed pass, with the length of the current failure streak.
    /// <para>
    /// A hook rather than a shared log call because each host owns its own
    /// <c>[LoggerMessage]</c> with its own event id, and an event id shared across four loops
    /// would make them indistinguishable in exactly the incident where telling them apart matters.
    /// </para>
    /// </summary>
    protected abstract void OnFailure(Exception exception, int consecutiveFailures);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool enabled = IsEnabled;
        if (!enabled)
        {
            OnDisabled();

            // Registered even though it will not run, so the health report can tell a loop that
            // was turned OFF from a loop that was never wired: the first is a decision, the second
            // is a defect, and only one of them is worth a restart.
            liveness.RegisterDisabled(LoopName);
            if (StopWhenDisabled)
            {
                return;
            }
        }

        TimeSpan period = Period;
        using var timer = new PeriodicTimer(period, timeProvider);
        if (enabled)
        {
            // Registered explicitly. A loop that forgot to register would be silently exempt from
            // the liveness check, and the loops worth watching are exactly the ones somebody added
            // without thinking about health.
            liveness.Register(LoopName, period);
        }

        var backoff = new LoopBackoff(period, timeProvider);
        while (!stoppingToken.IsCancellationRequested)
        {
            bool failed = false;
            try
            {
                await RunOnceAsync(stoppingToken);
                liveness.Tick(LoopName);
                backoff.RecordSuccess();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
#pragma warning disable CA1031 // A failing pass must not take the worker down with it.
            catch (Exception exception)
#pragma warning restore CA1031
            {
                OnFailure(exception, backoff.ConsecutiveFailures + 1);
                liveness.Fault(LoopName, exception);
                failed = true;
            }

            // A failed pass waits before the next one. Retrying at the poll interval regardless of
            // what is wrong turns a dependency outage into a log flood and a connection storm, and
            // the storm is part of why the dependency stays down. The scheduler polls every 100 ms
            // under the LocalMockE2E profile, which is where that arithmetic gets ugly.
            if (failed
                && !await backoff.DelayAfterFailureAsync(stoppingToken))
            {
                break;
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }
}

using Ivr.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

internal sealed partial class SchedulerJobHost(
    ISchedulerRuntime scheduler,
    IOptions<SchedulerOptions> options,
    WorkerLiveness liveness,
    TimeProvider timeProvider,
    ILogger<SchedulerJobHost> logger) : PollingJobHost(liveness, timeProvider)
{
    private readonly string workerId =
        string.Concat("ivr-scheduler-", Guid.NewGuid().ToString("N"));

    // W-0214. Only the transitions are logged, not the state. This loop turns every
    // PollIntervalMilliseconds -- 100ms under the LocalMockE2E profile -- so a line per pass would
    // bury the night it is meant to explain under six hundred identical lines a minute. Null until
    // the first run answers, so the first closed window still announces itself.
    private bool? callingWindowOpen;

    protected override string LoopName => "scheduler";

    protected override bool IsEnabled => options.Value.Enabled;

    protected override TimeSpan Period =>
        TimeSpan.FromMilliseconds(options.Value.PollIntervalMilliseconds);

    /// <summary>
    /// The scheduler keeps turning when it is disabled, unlike every other loop.
    /// <para>
    /// Its enable gate lives inside <c>SchedulerRuntime.RunOnceAsync</c>, so with the scheduler
    /// off the pass still runs and simply places no call — and the recovery and deadline-closing
    /// work in that same pass has to keep happening either way. Returning early here would stop
    /// those too. The liveness registration still reports DISABLED, because reporting a healthy
    /// loop that cannot dispatch is the exact shape of comfort that registration exists to remove.
    /// </para>
    /// </summary>
    protected override bool StopWhenDisabled => false;

    protected override async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        SchedulerRunResult result = await scheduler.RunOnceAsync(
            workerId,
            cancellationToken);
        if (result.QuarantinedLeases > 0
            || result.ClosedMissedDeadlines > 0
            || result.DispatchClaimed)
        {
            LogRun(
                logger,
                result.QuarantinedLeases,
                result.ClosedMissedDeadlines,
                result.DispatchClaimed);
        }

        if (callingWindowOpen != result.CallingWindowOpen)
        {
            if (result.CallingWindowOpen)
            {
                LogCallingWindowOpened(logger);
            }
            else
            {
                LogCallingWindowClosed(logger, result.CallingWindowOpensAt);
            }

            callingWindowOpen = result.CallingWindowOpen;
        }
    }

    /// <summary>
    /// Nothing to say. The scheduler being off is already reported by the liveness registration,
    /// and unlike the other loops it keeps running, so a "disabled; not running" line would be
    /// false.
    /// </summary>
    protected override void OnDisabled()
    {
    }

    protected override void OnFailure(Exception exception, int consecutiveFailures) =>
        LogFailure(logger, exception, consecutiveFailures);

    [LoggerMessage(
        EventId = 2310,
        Level = LogLevel.Information,
        Message = "Scheduler run completed: quarantined={Quarantined}, deadlineClosed={Closed}, dispatchClaimed={Claimed}.")]
    private static partial void LogRun(
        ILogger logger,
        int quarantined,
        int closed,
        bool claimed);

    [LoggerMessage(
        EventId = 2311,
        Level = LogLevel.Error,
        Message = "Scheduler run failed closed; consecutive failures={ConsecutiveFailures}.")]
    private static partial void LogFailure(
        ILogger logger,
        Exception exception,
        int consecutiveFailures);

    [LoggerMessage(
        EventId = 2312,
        Level = LogLevel.Information,
        Message = "Calling window closed; no dial will be claimed until {OpensAt}. "
            + "Recovery and deadline closing keep running.")]
    private static partial void LogCallingWindowClosed(ILogger logger, DateTimeOffset? opensAt);

    [LoggerMessage(
        EventId = 2313,
        Level = LogLevel.Information,
        Message = "Calling window open; dialling resumes.")]
    private static partial void LogCallingWindowOpened(ILogger logger);
}

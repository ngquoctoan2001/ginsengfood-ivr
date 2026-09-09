using Ivr.Infrastructure.Resilience;
using Ivr.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

public sealed partial class SchedulerJobHost(
    ISchedulerRuntime scheduler,
    IOptions<SchedulerOptions> options,
    WorkerLiveness liveness,
    TimeProvider timeProvider,
    ILogger<SchedulerJobHost> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string workerId = string.Concat("ivr-scheduler-", Guid.NewGuid().ToString("N"));
        var period = TimeSpan.FromMilliseconds(options.Value.PollIntervalMilliseconds);
        using var timer = new PeriodicTimer(period, timeProvider);
        // Registered explicitly. A loop that forgot to register would be silently exempt
        // from the liveness check, and the loops worth watching are exactly the ones
        // somebody added without thinking about health.
        //
        // The scheduler differs from the other two loops: its enable gate lives inside
        // SchedulerRuntime.RunOnceAsync, so with the scheduler off this loop still turns and does
        // nothing on every pass. Registering it as ENABLED then would report a healthy loop that
        // cannot dispatch, which is the exact shape of comfort this whole class exists to remove.
        if (options.Value.Enabled)
        {
            liveness.Register("scheduler", period);
        }
        else
        {
            liveness.RegisterDisabled("scheduler");
        }

        var backoff = new LoopBackoff(period, timeProvider);
        // W-0214. Only the transitions are logged, not the state. This loop turns every
        // PollIntervalMilliseconds -- 100ms under the LocalMockE2E profile -- so a line per pass
        // would bury the night it is meant to explain under six hundred identical lines a minute.
        // Null until the first run answers, so the first closed window still announces itself.
        bool? callingWindowOpen = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            bool failed = false;
            try
            {
                SchedulerRunResult result = await scheduler.RunOnceAsync(
                    workerId,
                    stoppingToken).ConfigureAwait(false);
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

                liveness.Tick("scheduler");
                backoff.RecordSuccess();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogFailure(logger, exception, backoff.ConsecutiveFailures + 1);
                liveness.Fault("scheduler", exception);
                failed = true;
            }

            // A failed pass waits before the next one. This loop turns every 100 ms under the
            // LocalMockE2E profile, so retrying at the poll interval regardless of what is wrong
            // meant ten stack traces a second per replica for as long as an outage lasted — and
            // the connection storm that came with them is part of why it lasted.
            if (failed
                && !await backoff.DelayAfterFailureAsync(stoppingToken).ConfigureAwait(false))
            {
                break;
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                break;
            }
        }
    }

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

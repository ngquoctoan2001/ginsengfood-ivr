using Ivr.Infrastructure.Scheduling;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

public sealed partial class SchedulerJobHost(
    ISchedulerRuntime scheduler,
    IOptions<SchedulerOptions> options,
    ILogger<SchedulerJobHost> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string workerId = string.Concat("ivr-scheduler-", Guid.NewGuid().ToString("N"));
        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(options.Value.PollIntervalMilliseconds));
        while (!stoppingToken.IsCancellationRequested)
        {
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
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogFailure(logger, exception);
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
        Message = "Scheduler run failed closed.")]
    private static partial void LogFailure(ILogger logger, Exception exception);
}

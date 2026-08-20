namespace Ivr.Worker;

/// <summary>
/// Reports whether the worker's background loops are still turning (<c>W-0043</c> §2).
/// <para>
/// This used to log a line every thirty seconds saying the worker existed, which was honest while
/// there was no background processing to report on and became misleading once there was: a line
/// that says "heartbeat" reads as "everything is fine", and it kept saying that while a job loop
/// was failing on every pass or hanging inside one.
/// </para>
/// <para>
/// So it now reads <see cref="WorkerLiveness"/> and says which loop stopped. A wedged loop names
/// itself at WARNING; a loop that is turning but failing names itself too, at a different level,
/// because those two want different responses from whoever reads the log.
/// </para>
/// </summary>
public sealed partial class IvrHeartbeat(
    WorkerLiveness liveness,
    ILogger<IvrHeartbeat> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            WorkerLivenessReport report = liveness.Read();
            if (report.Status == WorkerLivenessStatus.Stalled)
            {
                LogStalled(
                    logger,
                    report.StaleLoops.Count > 0
                        ? string.Join(", ", report.StaleLoops)
                        : "(none registered)");
            }
            else if (report.Status == WorkerLivenessStatus.Idle)
            {
                LogIdle(logger, report.Loops.Count);
            }
            else
            {
                WorkerLoopHealth[] faulting = [.. report.Loops.Where(loop => loop.ConsecutiveFaults > 0)];
                if (faulting.Length > 0)
                {
                    // Turning but failing. Not a restart signal -- restarting does not repair a
                    // dependency -- so it is reported at a level that does not read as one.
                    LogFaulting(
                        logger,
                        string.Join(
                            ", ",
                            faulting.Select(loop =>
                                $"{loop.Loop}x{loop.ConsecutiveFaults}({loop.LastFaultKind})")));
                }
                else
                {
                    LogHealthy(logger, report.Loops.Count);
                }
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "IVR worker heartbeat: {LoopCount} background loops turning.")]
    private static partial void LogHealthy(ILogger logger, int loopCount);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "IVR worker loops have stopped ticking: {StaleLoops}. The process is alive and these loops are not.")]
    private static partial void LogStalled(ILogger logger, string staleLoops);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "IVR worker is idle: all {LoopCount} loops are configured off. Nothing is being processed, and that is the configuration rather than a fault.")]
    private static partial void LogIdle(ILogger logger, int loopCount);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "IVR worker loops are turning but failing: {FaultingLoops}. A restart will not repair a dependency.")]
    private static partial void LogFaulting(ILogger logger, string faultingLoops);
}

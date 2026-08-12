namespace Ivr.Worker;

/// <summary>
/// Emits a low-frequency heartbeat until real background processing is added.
/// </summary>
public sealed partial class IvrHeartbeat(ILogger<IvrHeartbeat> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            LogHeartbeat(logger, DateTimeOffset.UtcNow);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "IVR worker heartbeat at {HeartbeatTimeUtc}; execution remains MOCK")]
    private static partial void LogHeartbeat(ILogger logger, DateTimeOffset heartbeatTimeUtc);
}

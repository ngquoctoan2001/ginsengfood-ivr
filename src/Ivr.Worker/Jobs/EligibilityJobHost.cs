using Ivr.Infrastructure.Eligibility;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

internal sealed partial class EligibilityJobHost(
    EligibilityPollingRuntime runtime,
    IOptions<EligibilityPollingOptions> options,
    WorkerLiveness liveness,
    TimeProvider timeProvider,
    ILogger<EligibilityJobHost> logger) : PollingJobHost(liveness, timeProvider)
{
    protected override string LoopName => "eligibility";
    protected override bool IsEnabled => options.Value.Enabled;
    protected override TimeSpan Period => TimeSpan.FromMilliseconds(options.Value.PollIntervalMilliseconds);
    protected override async Task RunOnceAsync(CancellationToken cancellationToken) =>
        await runtime.RunOnceAsync(cancellationToken);
    protected override void OnDisabled() => LogDisabled(logger);
    protected override void OnFailure(Exception exception, int consecutiveFailures) =>
        LogFailure(logger, consecutiveFailures);

    [LoggerMessage(EventId = 2360, Level = LogLevel.Information,
        Message = "Eligibility polling is disabled by configuration.")]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(EventId = 2361, Level = LogLevel.Error,
        Message = "Eligibility polling failed; pending work retained; consecutive failures={ConsecutiveFailures}.")]
    private static partial void LogFailure(ILogger logger, int consecutiveFailures);
}

using Ivr.Infrastructure.Repositories;
using Ivr.Worker.Normalization;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

internal sealed partial class NormalizationJobHost(
    ResultNormalizer normalizer,
    IOptions<NormalizationOptions> options,
    WorkerLiveness liveness,
    TimeProvider timeProvider,
    ILogger<NormalizationJobHost> logger) : PollingJobHost(liveness, timeProvider)
{
    private readonly string workerId =
        string.Concat("ivr-normalizer-", Guid.NewGuid().ToString("N"));

    protected override string LoopName => "normalization";

    protected override bool IsEnabled => options.Value.Enabled;

    protected override TimeSpan Period =>
        TimeSpan.FromMilliseconds(options.Value.PollIntervalMilliseconds);

    protected override async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<NormalizationPersistenceResult> results = await normalizer.RunBatchAsync(
            workerId,
            options.Value.BatchSize,
            cancellationToken).ConfigureAwait(false);
        if (results.Count > 0)
        {
            LogBatch(logger, results.Count);
        }
    }

    protected override void OnDisabled() => LogDisabled(logger);

    protected override void OnFailure(Exception exception, int consecutiveFailures) =>
        LogFailure(logger, exception, consecutiveFailures);

    [LoggerMessage(
        EventId = 2320,
        Level = LogLevel.Information,
        Message = "Result normalizer is disabled by configuration.")]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(
        EventId = 2321,
        Level = LogLevel.Information,
        Message = "Result normalizer persisted {Count} result(s).")]
    private static partial void LogBatch(ILogger logger, int count);

    [LoggerMessage(
        EventId = 2322,
        Level = LogLevel.Error,
        Message = "Result normalizer failed closed; consecutive failures={ConsecutiveFailures}.")]
    private static partial void LogFailure(
        ILogger logger,
        Exception exception,
        int consecutiveFailures);
}

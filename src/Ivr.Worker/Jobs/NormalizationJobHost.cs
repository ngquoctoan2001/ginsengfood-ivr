using Ivr.Infrastructure.Repositories;
using Ivr.Worker.Normalization;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

public sealed partial class NormalizationJobHost(
    ResultNormalizer normalizer,
    IOptions<NormalizationOptions> options,
    ILogger<NormalizationJobHost> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        NormalizationOptions snapshot = options.Value;
        if (!snapshot.Enabled)
        {
            LogDisabled(logger);
            return;
        }

        string workerId = string.Concat("ivr-normalizer-", Guid.NewGuid().ToString("N"));
        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(snapshot.PollIntervalMilliseconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyList<NormalizationPersistenceResult> results =
                    await normalizer.RunBatchAsync(
                        workerId,
                        snapshot.BatchSize,
                        stoppingToken).ConfigureAwait(false);
                if (results.Count > 0)
                {
                    LogBatch(logger, results.Count);
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
        Message = "Result normalizer failed closed.")]
    private static partial void LogFailure(ILogger logger, Exception exception);
}

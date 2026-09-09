using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Resilience;
using Ivr.Worker.Normalization;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

public sealed partial class NormalizationJobHost(
    ResultNormalizer normalizer,
    IOptions<NormalizationOptions> options,
    WorkerLiveness liveness,
    TimeProvider timeProvider,
    ILogger<NormalizationJobHost> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        NormalizationOptions snapshot = options.Value;
        if (!snapshot.Enabled)
        {
            LogDisabled(logger);
            // Registered even though it will not run, so the report can tell a loop that
            // was turned OFF from a loop that was never wired: the first is a decision,
            // the second is a defect, and only one of them is worth a restart.
            liveness.RegisterDisabled("normalization");
            return;
        }

        string workerId = string.Concat("ivr-normalizer-", Guid.NewGuid().ToString("N"));
        var period = TimeSpan.FromMilliseconds(snapshot.PollIntervalMilliseconds);
        using var timer = new PeriodicTimer(period, timeProvider);
        // Registered explicitly. A loop that forgot to register would be silently exempt
        // from the liveness check, and the loops worth watching are exactly the ones
        // somebody added without thinking about health.
        liveness.Register("normalization", period);
        var backoff = new LoopBackoff(period, timeProvider);
        while (!stoppingToken.IsCancellationRequested)
        {
            bool failed = false;
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

                liveness.Tick("normalization");
                backoff.RecordSuccess();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogFailure(logger, exception, backoff.ConsecutiveFailures + 1);
                liveness.Fault("normalization", exception);
                failed = true;
            }

            // A failed pass waits before the next one. Retrying at the poll interval regardless of
            // what is wrong turns a dependency outage into a log flood and a connection storm, and
            // the storm is part of why the dependency stays down.
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

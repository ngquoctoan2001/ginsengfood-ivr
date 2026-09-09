using Ivr.Infrastructure.Analytics;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

/// <summary>
/// Runs the P10-4 pipeline on an interval (<c>W-0055</c>).
///
/// <para>A run that throws does not stop the host. The pipeline is derived, read-only reporting:
/// taking the worker down with it would trade a stale dashboard for a stopped scheduler, and the
/// scheduler is the part that places calls. The failure is logged at warning and the next tick
/// retries — the anti-join means a failed run leaves nothing half-applied to clean up.</para>
///
/// <para>This loop has the longest interval in the worker, so its liveness grace is the widest —
/// three of its own intervals rather than the thirty-second floor the fast loops land on. A slow
/// reporting loop that stops still matters: the dashboard goes quietly stale rather than visibly
/// empty, which is the harder kind of wrong to notice.</para>
/// </summary>
internal sealed partial class AnalyticsEtlJobHost(
    IAnalyticsEtlJob etlJob,
    IOptions<AnalyticsEtlOptions> options,
    TimeProvider timeProvider,
    WorkerLiveness liveness,
    ILogger<AnalyticsEtlJobHost> logger) : PollingJobHost(liveness, timeProvider)
{
    protected override string LoopName => "analytics";

    protected override bool IsEnabled => options.Value.Enabled;

    protected override TimeSpan Period => TimeSpan.FromSeconds(options.Value.IntervalSeconds);

    protected override async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        AnalyticsEtlOptions configured = options.Value;
        AnalyticsEtlRunReport report = await etlJob.RunAsync(
            new AnalyticsEtlRunOptions
            {
                BatchSize = configured.BatchSize,
                RebuildAggregates = configured.RebuildAggregates,
            },
            cancellationToken).ConfigureAwait(false);

        LogCompleted(
            logger,
            report.LoadedRows,
            report.RejectedRows,
            report.JobRowsInserted + report.JobRowsRefreshed,
            report.BucketsRecomputed,
            report.ReconcileStatus,
            report.DurationMs);

        if (report.RejectedRows > 0)
        {
            // Separate from the completion line on purpose: a rejected row means the privacy
            // filter fired, and that must be findable without reading a field inside an
            // informational message.
            LogRejected(logger, report.RejectedRows);
        }
    }

    protected override void OnDisabled() => LogDisabled(logger);

    protected override void OnFailure(Exception exception, int consecutiveFailures) =>
        LogFailed(logger, exception, consecutiveFailures);

    [LoggerMessage(
        EventId = 1200,
        Level = LogLevel.Information,
        Message = "Analytics ETL disabled; no facts were loaded")]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(
        EventId = 1201,
        Level = LogLevel.Information,
        Message = "Analytics ETL loaded {Loaded} results, rejected {Rejected}, touched {JobRows} job rows, recomputed {Buckets} buckets; reconcile={ReconcileStatus}; durationMs={DurationMs}")]
    private static partial void LogCompleted(
        ILogger logger,
        int loaded,
        int rejected,
        int jobRows,
        int buckets,
        string reconcileStatus,
        long durationMs);

    [LoggerMessage(
        EventId = 1202,
        Level = LogLevel.Warning,
        Message = "Analytics privacy filter rejected {Rejected} source rows; they were not loaded")]
    private static partial void LogRejected(ILogger logger, int rejected);

    [LoggerMessage(
        EventId = 1203,
        Level = LogLevel.Warning,
        Message = "Analytics ETL run failed; the next tick retries; consecutive failures={ConsecutiveFailures}")]
    private static partial void LogFailed(
        ILogger logger,
        Exception exception,
        int consecutiveFailures);
}

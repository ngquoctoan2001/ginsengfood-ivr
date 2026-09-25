namespace Ivr.Infrastructure.Analytics;

/// <summary>Reconciliation verdict recorded on the checkpoint after every run.</summary>
public static class AnalyticsReconcileStatus
{
    public const string NotRun = "NOT_RUN";

    /// <summary>Fact count equals source count. The pipeline is complete as of this run.</summary>
    public const string Complete = "COMPLETE";

    /// <summary>The batch cap was reached; more source rows remain. Expected, not a fault.</summary>
    public const string Backlog = "BACKLOG";

    /// <summary>
    /// Counts diverge with no backlog and no rejections to explain it, or a job-grain
    /// fact still disagrees with its source job after the run refreshed it. This is
    /// the state a time-watermark pipeline would reach silently; here it is a status a
    /// data-quality check can fail on, and <c>IvrAnalyticsReconcileMismatch</c> does.
    /// </summary>
    public const string Mismatch = "MISMATCH";
}

/// <summary>
/// The <c>ivr.outcome</c> values on <c>ivr_analytics_etl_runs_total</c> that are not a
/// reconcile verdict (<c>W-0055</c>).
///
/// <para>Kept out of <see cref="AnalyticsReconcileStatus"/> on purpose. That set is
/// persisted on the checkpoint and served by the reporting API, and a run that threw
/// writes no checkpoint at all, so a verdict of "failed" would be a value the API could
/// never show and the console could never have been built to read.</para>
/// </summary>
public static class AnalyticsEtlRunOutcome
{
    /// <summary>The run threw before it wrote a verdict. Cancellation at shutdown is not counted.</summary>
    public const string Failed = "FAILED";
}

public sealed record AnalyticsEtlRunOptions
{
    /// <summary>Rows loaded per run. Bounds the transaction, not correctness.</summary>
    public int BatchSize { get; init; } = 5_000;

    /// <summary>
    /// Recompute every KPI bucket rather than only those the batch touched. Used
    /// after a backfill, and by the data-quality path to prove a recompute is a
    /// no-op when nothing changed.
    /// </summary>
    public bool RebuildAggregates { get; init; }

    public DateTimeOffset? Now { get; init; }
}

/// <param name="SourceJobCount">Call jobs in the source, read in the same snapshot as the job facts.</param>
/// <param name="JobFactCount">Job-grain facts after this run's inserts.</param>
/// <param name="JobRowsRejected">
/// Source jobs the privacy filter refused this run. Like a rejected result, a rejected job is
/// re-read and re-refused on every run, so this is the number currently refused, not a total.
/// </param>
/// <param name="JobFactsDrifted">
/// Job facts that still disagree with their source job after the refresh, in the same snapshot.
/// Zero on a healthy run by construction; anything else is the refresh failing to do its job.
/// </param>
/// <param name="Skipped">
/// W-0355. Another worker held the run lock, so this run read nothing, wrote nothing -- not even a
/// checkpoint -- and is not counted on <c>ivr_analytics_etl_runs_total</c>. Its status stays
/// <see cref="AnalyticsReconcileStatus.NotRun"/>, which is a value the reporting API already knows.
/// </param>
public sealed record AnalyticsEtlRunReport(
    int LoadedRows,
    int RejectedRows,
    int BucketsRecomputed,
    int SourceRowCount,
    int FactRowCount,
    int OrphanSourceRows,
    string ReconcileStatus,
    long DurationMs,
    int JobRowsInserted = 0,
    int JobRowsRefreshed = 0,
    int SourceJobCount = 0,
    int JobFactCount = 0,
    int JobRowsRejected = 0,
    int JobFactsDrifted = 0,
    bool Skipped = false)
{
    /// <summary>True when the batch cap stopped the run before the source was exhausted.</summary>
    public bool HasBacklog =>
        string.Equals(ReconcileStatus, AnalyticsReconcileStatus.Backlog, StringComparison.Ordinal);
}

public interface IAnalyticsEtlJob
{
    public Task<AnalyticsEtlRunReport> RunAsync(
        AnalyticsEtlRunOptions options,
        CancellationToken cancellationToken);
}

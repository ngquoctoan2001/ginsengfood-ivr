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
    /// Counts diverge with no backlog and no rejections to explain it. This is the
    /// state a time-watermark pipeline would reach silently; here it is a status a
    /// data-quality check can fail on.
    /// </summary>
    public const string Mismatch = "MISMATCH";
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
    int JobRowsRefreshed = 0)
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

namespace Ivr.Infrastructure.Analytics;

/// <summary>
/// Worker-side schedule for the P10-4 pipeline.
///
/// <para><b>Enabled by default</b>, unlike the retention CronJob that <c>W-0044</c>
/// deliberately ships off. The two look similar and are not: the CronJob could not
/// complete without a code change, so shipping it on would have left a permanently
/// failing object in every namespace. This job has no external dependency — same
/// process, same database, read-only against the operational tables — so the
/// failure mode of shipping it off is the one that actually bites: an empty
/// warehouse, a reporting API quietly serving operational reads, and nothing
/// anywhere saying the pipeline never ran.</para>
/// </summary>
public sealed class AnalyticsEtlOptions
{
    public const string SectionName = "Ivr:Analytics";

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Rows per run. The cap bounds one transaction, not completeness — a run that
    /// hits it reports <c>BACKLOG</c> and the next run continues from the anti-join.
    /// </summary>
    public int BatchSize { get; set; } = 5_000;

    /// <summary>
    /// Seconds between runs. Five minutes sits under the fifteen-minute freshness
    /// budget the reporting console already advertises, so a warehouse that is
    /// keeping up never shows as stale.
    /// </summary>
    public int IntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Recompute every KPI bucket on each run instead of only the touched dates.
    /// Off by default: it is a backfill tool, and leaving it on would make every
    /// run scale with history rather than with new data.
    /// </summary>
    public bool RebuildAggregates { get; set; }
}

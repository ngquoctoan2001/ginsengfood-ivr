namespace Ivr.Infrastructure.Analytics;

/// <summary>
/// Star-schema tables for the P10-4 analytics pipeline (<c>W-0055</c>).
///
/// <para><b>Where this lives, and why it is not a separate warehouse.</b> These
/// tables sit in a dedicated <c>analytics</c> PostgreSQL schema inside the same
/// database as the operational tables. That is a deliberate, stated limit, not a
/// claim: no warehouse cluster exists yet (<c>W-0063</c>), and inventing a second
/// datastore that nobody can provision would produce a pipeline that has never
/// run. A separate schema is the part that is real today — it gives the grant
/// boundary a BI tool needs (<c>SELECT</c> on <c>analytics</c> only, never on the
/// operational tables) without pretending the rest exists.</para>
///
/// <para><b>The privacy boundary.</b> Two kinds of identifier are treated
/// differently, and the difference is the whole point:</para>
/// <list type="bullet">
///   <item><description><b>IVR-internal ids</b> (<c>ivr_call_result_id</c>,
///   <c>ivr_call_job_id</c>) are carried as-is. They identify IVR work, not a
///   person, and they are already visible in the admin console. Hashing one while
///   the other stays readable would look like protection and provide none.</description></item>
///   <item><description><b>The Sales order id</b> is carried only as
///   <c>order_ref_hash</c>. It is a business key that resolves to a customer in a
///   system IVR does not own, so it is the one field where a join back is a real
///   re-identification path. The hash still supports distinct-order counting,
///   which is the only thing the KPIs need it for.</description></item>
/// </list>
///
/// <para>Never present, in any table here: phone in any form, dial token, order
/// code, customer id, trust status, evidence or audit refs, SIM channel, provider
/// call id, or any free-text field. <c>AnalyticsColumnPolicy</c> enforces that by
/// reflection so a column added tomorrow fails a test rather than shipping.</para>
/// </summary>
public sealed class AnalyticsFactCallOutcomeEntity
{
    /// <summary>Natural key. The ETL anti-joins on this, which is what makes a replay exactly-once.</summary>
    public string IvrCallResultId { get; set; } = string.Empty;

    public string IvrCallJobId { get; set; } = string.Empty;

    /// <summary>SHA-256 of the Sales order id, lowercase hex. Never the order id itself.</summary>
    public string OrderRefHash { get; set; } = string.Empty;

    public string ProgramKey { get; set; } = string.Empty;
    public string ScriptVariantKey { get; set; } = string.Empty;
    public string ResultTypeKey { get; set; } = string.Empty;
    public string FinalResultStatus { get; set; } = string.Empty;

    /// <summary>Single DTMF digit, or null. Bounded domain, no free text.</summary>
    public string? DtmfKey { get; set; }

    public bool IsFinal { get; set; }
    public bool IsCountedCustomerAttempt { get; set; }

    /// <summary>Counted customer attempts on the job at load time. Technical retries excluded (DT-02).</summary>
    public int CountedAttemptNumber { get; set; }

    public DateTimeOffset EventAt { get; set; }
    public DateOnly EventDate { get; set; }
    public int EventHour { get; set; }

    /// <summary>Seconds from job T0 to this result, or null when the source ordering is inconsistent.</summary>
    public int? SecondsToResult { get; set; }

    /// <summary>Ingest time. Distinct from <see cref="EventAt"/> — a late row has both.</summary>
    public DateTimeOffset LoadedAt { get; set; }
}

/// <summary>
/// Job-grain fact. The result-grain fact cannot answer job-level questions —
/// how many jobs ran, how many were eligible, how many needed a second attempt —
/// because a job with no result has no row there at all.
///
/// <para>Carrying both grains is what lets the reporting API say
/// <c>warehouse_backed=true</c> without an asterisk. The alternative considered
/// and rejected was to serve the job-level numbers from the operational tables
/// while claiming the warehouse: the payload would have been half true, and the
/// half that was not would have been the half nobody checks.</para>
/// </summary>
public sealed class AnalyticsFactCallJobEntity
{
    public string IvrCallJobId { get; set; } = string.Empty;
    public string OrderRefHash { get; set; } = string.Empty;
    public string ProgramKey { get; set; } = string.Empty;
    public string ScriptVariantKey { get; set; } = string.Empty;

    /// <summary>Eligibility decision reduced to a boolean; the reason is not carried.</summary>
    public bool Eligible { get; set; }

    /// <summary>Counted customer attempts. Technical retries excluded (DT-02).</summary>
    public int CountedAttemptCount { get; set; }

    /// <summary>
    /// False while the job can still change. The ETL refreshes open jobs on every
    /// run and leaves closed ones alone, which is the only reason a mutable source
    /// row can be projected into an append-shaped store without going stale.
    /// </summary>
    public bool Closed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateOnly CreatedDate { get; set; }
    public DateTimeOffset LoadedAt { get; set; }
}

/// <summary>Program dimension. Upserted by the ETL; carries no data of its own.</summary>
public sealed class AnalyticsDimProgramEntity
{
    public string ProgramKey { get; set; } = string.Empty;
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public int FactRowCount { get; set; }
}

/// <summary>Script-variant dimension — the A/B axis P2-7 versions produce.</summary>
public sealed class AnalyticsDimScriptVariantEntity
{
    public string ScriptVariantKey { get; set; } = string.Empty;
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public int FactRowCount { get; set; }
}

/// <summary>Result-taxonomy dimension (DT-02).</summary>
public sealed class AnalyticsDimResultTypeEntity
{
    public string ResultTypeKey { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public int FactRowCount { get; set; }
}

/// <summary>
/// Daily KPI aggregate. Every bucket is recomputed in full from the facts it
/// covers, never incremented — a recompute is idempotent by construction, and an
/// increment is the shape that double-counts on replay.
/// </summary>
public sealed class AnalyticsKpiDailyEntity
{
    public DateOnly BucketDate { get; set; }
    public string ProgramKey { get; set; } = string.Empty;
    public string ScriptVariantKey { get; set; } = string.Empty;

    public int TotalResults { get; set; }
    public int FinalResults { get; set; }
    public int DistinctOrders { get; set; }
    public int ConfirmedCount { get; set; }
    public int CancelledCount { get; set; }
    public int NoAnswerCount { get; set; }
    public int InvalidPhoneCount { get; set; }
    public int TechnicalCount { get; set; }
    public int OperationalBlockedCount { get; set; }
    public int SecondAttemptResults { get; set; }

    /// <summary>Sum and count are stored rather than the mean, so buckets stay addable.</summary>
    public long SecondsToResultSum { get; set; }
    public int SecondsToResultCount { get; set; }

    public DateTimeOffset ComputedAt { get; set; }
}

/// <summary>
/// One row per pipeline. Observability and reconciliation only — deliberately
/// <b>not</b> a correctness input: the ETL selects by anti-join, so losing this
/// row costs a slower run, never a missing fact.
/// </summary>
public sealed class AnalyticsEtlCheckpointEntity
{
    public string PipelineName { get; set; } = string.Empty;
    public DateTimeOffset? LastRunAt { get; set; }
    public int LastRunLoadedRows { get; set; }
    public int LastRunRejectedRows { get; set; }
    public long LastRunDurationMs { get; set; }
    public long TotalLoadedRows { get; set; }
    public long TotalRejectedRows { get; set; }
    public DateTimeOffset? HighWaterEventAt { get; set; }
    public DateTimeOffset? LastReconciledAt { get; set; }
    public int SourceRowCount { get; set; }
    public int FactRowCount { get; set; }
    public string ReconcileStatus { get; set; } = "NOT_RUN";
}

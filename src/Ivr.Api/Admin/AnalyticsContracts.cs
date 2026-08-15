using System.Text.Json.Serialization;

namespace Ivr.Api.Admin;

/// <summary>
/// Read-only, aggregate-only analytics projections (W-0098, consumed by P3-4).
///
/// Nothing here carries a customer identifier. Every payload is a count, a rate
/// or a bucket label, and every bucket smaller than
/// <c>min_bucket_size</c> is dropped before it leaves the service (D-05).
/// </summary>
public sealed record AnalyticsDataQualityView(
    [property: JsonPropertyName("generated_at")] DateTimeOffset GeneratedAt,
    /// `OPERATIONAL_READ_MODEL` until the P10-4 warehouse exists. The console
    /// must state which one it is rather than implying a BI pipeline that has
    /// not been built.
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("warehouse_backed")] bool WarehouseBacked,
    /// The work that replaces this service with the real pipeline.
    [property: JsonPropertyName("pipeline_work_id")] string PipelineWorkId,
    [property: JsonPropertyName("latest_event_at")] DateTimeOffset? LatestEventAt,
    [property: JsonPropertyName("freshness_seconds")] long? FreshnessSeconds,
    /// One of `FRESH`, `STALE`, `NO_DATA`.
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("min_bucket_size")] int MinBucketSize,
    [property: JsonPropertyName("suppressed_bucket_count")] int SuppressedBucketCount,
    [property: JsonPropertyName("scanned_rows")] int ScannedRows,
    /// True when the scan cap was reached, so the numbers below are a partial
    /// view. Reported rather than silently truncated.
    [property: JsonPropertyName("truncated")] bool Truncated);

public sealed record AnalyticsFilterView(
    [property: JsonPropertyName("program")] string? Program,
    [property: JsonPropertyName("result_type")] string? ResultType,
    [property: JsonPropertyName("script_variant")] string? ScriptVariant,
    [property: JsonPropertyName("bucket")] string Bucket,
    [property: JsonPropertyName("from")] DateTimeOffset? From,
    [property: JsonPropertyName("to")] DateTimeOffset? To);

public sealed record AnalyticsKpiView(
    [property: JsonPropertyName("total_results")] int TotalResults,
    [property: JsonPropertyName("total_final_results")] int TotalFinalResults,
    [property: JsonPropertyName("total_call_jobs")] int TotalCallJobs,
    [property: JsonPropertyName("total_eligible_tasks")] int TotalEligibleTasks,
    [property: JsonPropertyName("confirm_rate")] double ConfirmRate,
    [property: JsonPropertyName("cancel_rate")] double CancelRate,
    [property: JsonPropertyName("no_answer_rate")] double NoAnswerRate,
    [property: JsonPropertyName("invalid_phone_rate")] double InvalidPhoneRate,
    [property: JsonPropertyName("technical_rate")] double TechnicalRate,
    [property: JsonPropertyName("operational_blocked_rate")] double OperationalBlockedRate,
    /// Share of in-scope call jobs that consumed a second counted customer
    /// attempt. Technical retries never count (DT-02).
    [property: JsonPropertyName("attempt_2_rate")] double AttemptTwoRate,
    [property: JsonPropertyName("avg_seconds_to_final")] double? AvgSecondsToFinal);

public sealed record AnalyticsTrendBucketView(
    [property: JsonPropertyName("bucket_start")] DateTimeOffset BucketStart,
    [property: JsonPropertyName("program")] string Program,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("confirmed")] int Confirmed,
    [property: JsonPropertyName("cancelled")] int Cancelled,
    [property: JsonPropertyName("no_answer")] int NoAnswer,
    [property: JsonPropertyName("invalid_phone")] int InvalidPhone,
    [property: JsonPropertyName("technical")] int Technical,
    [property: JsonPropertyName("operational_blocked")] int OperationalBlocked,
    [property: JsonPropertyName("confirm_rate")] double ConfirmRate);

public sealed record AnalyticsBreakdownRowView(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("confirmed")] int Confirmed,
    [property: JsonPropertyName("confirm_rate")] double ConfirmRate,
    [property: JsonPropertyName("share")] double Share);

public sealed record AnalyticsSummaryApiResult(
    [property: JsonPropertyName("filter")] AnalyticsFilterView Filter,
    [property: JsonPropertyName("execution_mode")] string ExecutionMode,
    [property: JsonPropertyName("kpi")] AnalyticsKpiView Kpi,
    [property: JsonPropertyName("result_taxonomy")] IReadOnlyList<AnalyticsBreakdownRowView> ResultTaxonomy,
    [property: JsonPropertyName("data_quality")] AnalyticsDataQualityView DataQuality);

public sealed record AnalyticsTrendApiResult(
    [property: JsonPropertyName("filter")] AnalyticsFilterView Filter,
    [property: JsonPropertyName("buckets")] IReadOnlyList<AnalyticsTrendBucketView> Buckets,
    [property: JsonPropertyName("data_quality")] AnalyticsDataQualityView DataQuality);

public sealed record AnalyticsBreakdownApiResult(
    [property: JsonPropertyName("filter")] AnalyticsFilterView Filter,
    /// One of `RESULT_TYPE`, `SCRIPT_VARIANT`, `PROGRAM`.
    [property: JsonPropertyName("dimension")] string Dimension,
    [property: JsonPropertyName("rows")] IReadOnlyList<AnalyticsBreakdownRowView> Rows,
    [property: JsonPropertyName("data_quality")] AnalyticsDataQualityView DataQuality);

/// <summary>
/// A sanitized, column-oriented aggregate extract. `rows` holds formatted
/// strings only — there is no object shape a caller could smuggle a customer
/// field through, and every row is already above the k-anonymity threshold.
/// </summary>
public sealed record AnalyticsExportApiResult(
    [property: JsonPropertyName("filter")] AnalyticsFilterView Filter,
    [property: JsonPropertyName("dimension")] string Dimension,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("actor_id")] string ActorId,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("audit_ref")] string AuditRef,
    [property: JsonPropertyName("columns")] IReadOnlyList<string> Columns,
    [property: JsonPropertyName("rows")] IReadOnlyList<IReadOnlyList<string>> Rows,
    [property: JsonPropertyName("suppressed_row_count")] int SuppressedRowCount,
    [property: JsonPropertyName("data_quality")] AnalyticsDataQualityView DataQuality);

/// <summary>Query shape shared by the four analytics operations.</summary>
public sealed record AnalyticsFilter(
    string? Program,
    string? ResultType,
    string? ScriptVariant,
    string? Bucket,
    DateTimeOffset? From,
    DateTimeOffset? To);

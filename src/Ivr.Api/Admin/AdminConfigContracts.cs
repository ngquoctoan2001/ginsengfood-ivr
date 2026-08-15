using System.Text.Json.Serialization;

namespace Ivr.Api.Admin;

/// <summary>
/// Read-only back-office projections for the admin console (W-0096, consumed by
/// P3-3). Nothing here mutates: script lifecycle transitions, seed loading and
/// permission assignment are all deliberately absent.
/// </summary>
public sealed record ScriptApprovalView(
    [property: JsonPropertyName("approval_type")] string ApprovalType,
    [property: JsonPropertyName("actor_id")] string ActorId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("approved_at")] DateTimeOffset ApprovedAt);

public sealed record ScriptVersionView(
    [property: JsonPropertyName("template_id")] string TemplateId,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("template_hash")] string TemplateHash,
    [property: JsonPropertyName("allowed_input_fields")] IReadOnlyList<string> AllowedInputFields,
    [property: JsonPropertyName("approvals")] IReadOnlyList<ScriptApprovalView> Approvals,
    [property: JsonPropertyName("missing_approvals")] IReadOnlyList<string> MissingApprovals,
    /// False when the stored template no longer satisfies the Target V1 whitelist.
    /// Surfaced rather than thrown: a non-conforming version must be visible to an
    /// operator, not crash the catalogue.
    [property: JsonPropertyName("template_valid")] bool TemplateValid,
    [property: JsonPropertyName("uses_production_decision_fields")] bool UsesProductionDecisionFields,
    [property: JsonPropertyName("created_by")] string CreatedBy,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("submitted_by")] string? SubmittedBy,
    [property: JsonPropertyName("submitted_at")] DateTimeOffset? SubmittedAt,
    [property: JsonPropertyName("retired_by")] string? RetiredBy,
    [property: JsonPropertyName("retired_at")] DateTimeOffset? RetiredAt);

public sealed record DtmfKeyView(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("meaning")] string Meaning,
    [property: JsonPropertyName("enabled")] bool Enabled);

public sealed record ScriptCatalogApiResult(
    [property: JsonPropertyName("generated_at")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("execution_mode")] string ExecutionMode,
    /// OD-V1-15. While this is false the Target V1 field set is not production approved,
    /// whatever the CONTENT and PRIVACY_LEGAL approvals say.
    [property: JsonPropertyName("production_target_v1_fields_approved")] bool ProductionTargetV1FieldsApproved,
    [property: JsonPropertyName("allowed_input_fields")] IReadOnlyList<string> AllowedInputFields,
    [property: JsonPropertyName("prohibited_variables")] IReadOnlyList<string> ProhibitedVariables,
    [property: JsonPropertyName("dtmf_map")] IReadOnlyList<DtmfKeyView> DtmfMap,
    [property: JsonPropertyName("required_approval_types")] IReadOnlyList<string> RequiredApprovalTypes,
    [property: JsonPropertyName("versions")] IReadOnlyList<ScriptVersionView> Versions);

public sealed record DependencyStatusView(
    [property: JsonPropertyName("dependency")] string Dependency,
    /// One of `UP`, `DOWN`, `READY_503`, `NOT_WIRED`.
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("detail")] string Detail,
    [property: JsonPropertyName("fail_closed_effect")] string FailClosedEffect,
    [property: JsonPropertyName("observed")] bool Observed,
    [property: JsonPropertyName("captured_at")] DateTimeOffset? CapturedAt);

public sealed record FailClosedEventView(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("reference")] string Reference,
    [property: JsonPropertyName("effect")] string Effect,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("occurred_at")] DateTimeOffset OccurredAt);

public sealed record IntegrationStatusApiResult(
    [property: JsonPropertyName("generated_at")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("execution_mode")] string ExecutionMode,
    [property: JsonPropertyName("sales_provider")] string SalesProvider,
    [property: JsonPropertyName("sim_provider")] string SimProvider,
    [property: JsonPropertyName("real_customer_call_allowed")] bool RealCustomerCallAllowed,
    [property: JsonPropertyName("global_dial_kill_switch")] bool GlobalDialKillSwitch,
    [property: JsonPropertyName("attempt_policy_version")] string AttemptPolicyVersion,
    [property: JsonPropertyName("flag_revision")] long FlagRevision,
    /// False until P6-1 (W-0040) delivers real dependency probing. While false the
    /// console must not present any dependency card as verified fail-closed.
    [property: JsonPropertyName("dependency_probing_available")] bool DependencyProbingAvailable,
    [property: JsonPropertyName("dependencies")] IReadOnlyList<DependencyStatusView> Dependencies,
    [property: JsonPropertyName("recent_fail_closed_events")] IReadOnlyList<FailClosedEventView> RecentFailClosedEvents);

public sealed record ReviewQueueItemView(
    [property: JsonPropertyName("review_item_id")] string ReviewItemId,
    [property: JsonPropertyName("source_type")] string SourceType,
    [property: JsonPropertyName("source_id")] string SourceId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("resolution")] string? Resolution,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("ivr_call_job_id")] string? IvrCallJobId,
    [property: JsonPropertyName("order_code_short")] string? OrderCodeShort,
    [property: JsonPropertyName("result_type")] string? ResultType,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("resolved_at")] DateTimeOffset? ResolvedAt);

public sealed record ReviewQueueApiResult(
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("page_size")] int PageSize,
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("items")] IReadOnlyList<ReviewQueueItemView> Items);

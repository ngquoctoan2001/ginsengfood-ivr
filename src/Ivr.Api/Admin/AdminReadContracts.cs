using System.Text.Json.Serialization;

namespace Ivr.Api.Admin;

/// <summary>
/// Read-side projections for the admin console (W-0095, consumed by P3-2).
///
/// Every shape here is masked by construction: no raw phone, no dial token, no
/// full address, no recording reference, no order state mutation (D-02/D-05).
/// The order code is exposed only in its short, script-approved form.
/// </summary>
public sealed record CallJobFilter(
    string? Program,
    string? Status,
    string? QueueStatus,
    string? ResultType,
    string? OrderCode,
    string? CorrelationId,
    bool NearExpiryOnly,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize);

public sealed record CallJobListItem(
    [property: JsonPropertyName("ivr_call_job_id")] string IvrCallJobId,
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("order_code_short")] string OrderCodeShort,
    [property: JsonPropertyName("phone_masked")] string PhoneMasked,
    [property: JsonPropertyName("program_type")] string ProgramType,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("queue_status")] string QueueStatus,
    [property: JsonPropertyName("attempt_count")] int AttemptCount,
    [property: JsonPropertyName("max_attempts")] int MaxAttempts,
    [property: JsonPropertyName("result_type")] string? ResultType,
    [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("closed_at")] DateTimeOffset? ClosedAt,
    [property: JsonPropertyName("near_expiry")] bool NearExpiry);

public sealed record CallJobPageApiResult(
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("page_size")] int PageSize,
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("items")] IReadOnlyList<CallJobListItem> Items);

public sealed record CallAttemptDetail(
    [property: JsonPropertyName("ivr_call_attempt_id")] string IvrCallAttemptId,
    [property: JsonPropertyName("attempt_number")] int AttemptNumber,
    [property: JsonPropertyName("scheduled_at")] DateTimeOffset ScheduledAt,
    [property: JsonPropertyName("started_at")] DateTimeOffset? StartedAt,
    [property: JsonPropertyName("ended_at")] DateTimeOffset? EndedAt,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("result_status")] string? ResultStatus,
    [property: JsonPropertyName("disposition")] string? Disposition,
    [property: JsonPropertyName("dtmf_key")] string? DtmfKey,
    [property: JsonPropertyName("is_counted_customer_attempt")] bool IsCountedCustomerAttempt,
    [property: JsonPropertyName("technical_retry_count")] int TechnicalRetryCount,
    [property: JsonPropertyName("technical_exception_type")] string? TechnicalExceptionType,
    [property: JsonPropertyName("sim_channel_id")] string? SimChannelId,
    [property: JsonPropertyName("blocked_reason")] string? BlockedReason,
    [property: JsonPropertyName("policy_version")] string PolicyVersion,
    [property: JsonPropertyName("script_version")] string ScriptVersion);

public sealed record CallResultDetail(
    [property: JsonPropertyName("ivr_call_result_id")] string IvrCallResultId,
    [property: JsonPropertyName("result_type")] string ResultType,
    [property: JsonPropertyName("result_reason")] string? ResultReason,
    [property: JsonPropertyName("dtmf_key")] string? DtmfKey,
    [property: JsonPropertyName("is_counted_customer_attempt")] bool IsCountedCustomerAttempt,
    [property: JsonPropertyName("is_final_for_ivr")] bool IsFinalForIvr,
    [property: JsonPropertyName("recommended_core_action")] string RecommendedCoreAction,
    [property: JsonPropertyName("human_review_required")] bool HumanReviewRequired,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

public sealed record ResultCallbackDetail(
    [property: JsonPropertyName("callback_id")] string CallbackId,
    [property: JsonPropertyName("ivr_call_result_id")] string IvrCallResultId,
    [property: JsonPropertyName("result_state")] string ResultState,
    [property: JsonPropertyName("delivery_status")] string DeliveryStatus,
    [property: JsonPropertyName("core_http_status")] int? CoreHttpStatus,
    [property: JsonPropertyName("core_response_code")] string? CoreResponseCode,
    [property: JsonPropertyName("retry_count")] int RetryCount,
    [property: JsonPropertyName("requires_core_revalidation")] bool RequiresCoreRevalidation,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("sent_at")] DateTimeOffset? SentAt,
    [property: JsonPropertyName("acknowledged_at")] DateTimeOffset? AcknowledgedAt);

public sealed record TechnicalExceptionDetail(
    [property: JsonPropertyName("technical_exception_id")] string TechnicalExceptionId,
    [property: JsonPropertyName("ivr_call_attempt_id")] string IvrCallAttemptId,
    [property: JsonPropertyName("exception_type")] string ExceptionType,
    [property: JsonPropertyName("customer_attempt_counted")] bool CustomerAttemptCounted,
    [property: JsonPropertyName("technical_retry_allowed")] bool TechnicalRetryAllowed,
    [property: JsonPropertyName("technical_retry_count")] int TechnicalRetryCount,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

public sealed record ReviewItemDetail(
    [property: JsonPropertyName("review_item_id")] string ReviewItemId,
    [property: JsonPropertyName("source_type")] string SourceType,
    [property: JsonPropertyName("source_id")] string SourceId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("resolution")] string? Resolution,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("resolved_at")] DateTimeOffset? ResolvedAt);

public sealed record CallJobDetailApiResult(
    [property: JsonPropertyName("ivr_call_job_id")] string IvrCallJobId,
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("order_code_short")] string OrderCodeShort,
    [property: JsonPropertyName("phone_masked")] string PhoneMasked,
    [property: JsonPropertyName("program_type")] string ProgramType,
    /// Opaque enum owned by Order Core. Displayed, never derived, never changed (D-02).
    [property: JsonPropertyName("order_state")] string OrderState,
    [property: JsonPropertyName("order_version_snapshot")] string OrderVersionSnapshot,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("queue_status")] string QueueStatus,
    [property: JsonPropertyName("eligible")] bool Eligible,
    [property: JsonPropertyName("eligibility_decision")] string EligibilityDecision,
    [property: JsonPropertyName("blocked_reasons")] IReadOnlyList<string> BlockedReasons,
    [property: JsonPropertyName("call_restriction")] bool CallRestriction,
    [property: JsonPropertyName("sellable_captured_at")] DateTimeOffset? SellableCapturedAt,
    [property: JsonPropertyName("max_attempts")] int MaxAttempts,
    [property: JsonPropertyName("attempt_policy_code")] string AttemptPolicyCode,
    [property: JsonPropertyName("script_version")] string ScriptVersion,
    [property: JsonPropertyName("privacy_policy_version")] string PrivacyPolicyVersion,
    [property: JsonPropertyName("t0_at")] DateTimeOffset T0At,
    [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("closed_at")] DateTimeOffset? ClosedAt,
    [property: JsonPropertyName("closed_reason")] string? ClosedReason,
    [property: JsonPropertyName("attempts")] IReadOnlyList<CallAttemptDetail> Attempts,
    [property: JsonPropertyName("results")] IReadOnlyList<CallResultDetail> Results,
    [property: JsonPropertyName("callbacks")] IReadOnlyList<ResultCallbackDetail> Callbacks,
    [property: JsonPropertyName("technical_exceptions")] IReadOnlyList<TechnicalExceptionDetail> TechnicalExceptions,
    [property: JsonPropertyName("review_items")] IReadOnlyList<ReviewItemDetail> ReviewItems,
    [property: JsonPropertyName("evidence_refs")] IReadOnlyList<string> EvidenceRefs,
    [property: JsonPropertyName("audit_refs")] IReadOnlyList<string> AuditRefs,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("input_signal_only")] bool InputSignalOnly,
    [property: JsonPropertyName("no_direct_order_update")] bool NoDirectOrderUpdate);

public sealed record DashboardQueuePanel(
    [property: JsonPropertyName("paused")] bool Paused,
    [property: JsonPropertyName("queued")] int Queued,
    [property: JsonPropertyName("held_mock")] int HeldMock,
    [property: JsonPropertyName("held_admin_review")] int HeldAdminReview,
    [property: JsonPropertyName("dispatching")] int Dispatching,
    [property: JsonPropertyName("open_total")] int OpenTotal,
    [property: JsonPropertyName("closed_total")] int ClosedTotal,
    [property: JsonPropertyName("near_expiry")] int NearExpiry);

public sealed record DashboardResultPanel(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("by_result_type")] IReadOnlyDictionary<string, int> ByResultType,
    [property: JsonPropertyName("confirm_rate")] double ConfirmRate,
    [property: JsonPropertyName("cancel_rate")] double CancelRate,
    [property: JsonPropertyName("no_answer_rate")] double NoAnswerRate,
    [property: JsonPropertyName("technical_exception_rate")] double TechnicalExceptionRate);

public sealed record DashboardAttemptPanel(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("counted_customer_attempts")] int CountedCustomerAttempts,
    [property: JsonPropertyName("technical_retries")] int TechnicalRetries,
    [property: JsonPropertyName("active")] int Active);

public sealed record DashboardSimPanel(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("enabled")] int Enabled,
    [property: JsonPropertyName("idle")] int Idle,
    [property: JsonPropertyName("active")] int Active,
    [property: JsonPropertyName("disabled")] int Disabled,
    [property: JsonPropertyName("health_failed")] int HealthFailed,
    [property: JsonPropertyName("quarantined")] int Quarantined,
    [property: JsonPropertyName("adapter_mode")] string AdapterMode);

public sealed record CapacityIncidentSummary(
    [property: JsonPropertyName("capacity_incident_id")] string CapacityIncidentId,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("hold_new_calls")] bool HoldNewCalls,
    [property: JsonPropertyName("shortage_reason")] string? ShortageReason,
    [property: JsonPropertyName("missed_deadline_count")] int MissedDeadlineCount,
    [property: JsonPropertyName("opened_at")] DateTimeOffset OpenedAt);

public sealed record DashboardApiResult(
    [property: JsonPropertyName("generated_at")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("execution_mode")] string ExecutionMode,
    [property: JsonPropertyName("sim_provider")] string SimProvider,
    [property: JsonPropertyName("real_customer_call_allowed")] bool RealCustomerCallAllowed,
    [property: JsonPropertyName("program_filter")] string? ProgramFilter,
    [property: JsonPropertyName("from")] DateTimeOffset? From,
    [property: JsonPropertyName("to")] DateTimeOffset? To,
    [property: JsonPropertyName("queue")] DashboardQueuePanel Queue,
    [property: JsonPropertyName("results")] DashboardResultPanel Results,
    [property: JsonPropertyName("attempts")] DashboardAttemptPanel Attempts,
    [property: JsonPropertyName("sim")] DashboardSimPanel Sim,
    [property: JsonPropertyName("open_incidents")] IReadOnlyList<CapacityIncidentSummary> OpenIncidents,
    [property: JsonPropertyName("missed_deadline_count")] int MissedDeadlineCount);

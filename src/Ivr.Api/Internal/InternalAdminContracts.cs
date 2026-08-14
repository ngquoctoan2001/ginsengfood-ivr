using System.Text.Json.Serialization;

namespace Ivr.Api.Internal;

public sealed record EligibilityLifecycleRequest(
    [property: JsonPropertyName("task_id")] string TaskId);

public sealed record CallJobLifecycleRequest(
    [property: JsonPropertyName("ivr_call_job_id")] string IvrCallJobId,
    [property: JsonPropertyName("task_id")] string TaskId);

public sealed record CallAttemptLifecycleRequest(
    [property: JsonPropertyName("ivr_call_attempt_id")] string IvrCallAttemptId,
    [property: JsonPropertyName("ivr_call_job_id")] string IvrCallJobId);

public sealed record CallResultLifecycleRequest(
    [property: JsonPropertyName("ivr_call_result_id")] string IvrCallResultId,
    [property: JsonPropertyName("ivr_call_job_id")] string IvrCallJobId);

public sealed record ResultCallbackLifecycleRequest(
    [property: JsonPropertyName("callback_id")] string CallbackId,
    [property: JsonPropertyName("ivr_call_result_id")] string IvrCallResultId);

public sealed record AdminMutationRequest(
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("evidence_ref")] string? EvidenceRef = null);

public sealed record TechnicalRetryRequest(
    [property: JsonPropertyName("technical_exception_id")] string TechnicalExceptionId,
    [property: JsonPropertyName("target_attempt_id")] string TargetAttemptId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("evidence_ref")] string? EvidenceRef = null);

public sealed record AdminReviewRequest(
    [property: JsonPropertyName("review_item_id")] string ReviewItemId,
    [property: JsonPropertyName("resolution")] string Resolution,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("evidence_ref")] string? EvidenceRef = null);

public sealed record EligibilityApiResult(
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("blocked_reasons")] IReadOnlyCollection<string> BlockedReasons,
    [property: JsonPropertyName("evidence_ref")] string EvidenceRef);

public sealed record CallJobApiResult(
    [property: JsonPropertyName("ivr_call_job_id")] string IvrCallJobId,
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("program_type")] string ProgramType,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("queue_status")] string QueueStatus,
    [property: JsonPropertyName("eligible")] bool Eligible,
    [property: JsonPropertyName("max_attempts")] int MaxAttempts,
    [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt,
    [property: JsonPropertyName("input_signal_only")] bool InputSignalOnly,
    [property: JsonPropertyName("no_direct_order_update")] bool NoDirectOrderUpdate);

public sealed record CallAttemptApiResult(
    [property: JsonPropertyName("ivr_call_attempt_id")] string IvrCallAttemptId,
    [property: JsonPropertyName("ivr_call_job_id")] string IvrCallJobId,
    [property: JsonPropertyName("attempt_number")] int AttemptNumber,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("result_status")] string? ResultStatus,
    [property: JsonPropertyName("is_counted_customer_attempt")] bool IsCountedCustomerAttempt,
    [property: JsonPropertyName("technical_retry_count")] int TechnicalRetryCount,
    [property: JsonPropertyName("sim_channel_id")] string? SimChannelId);

public sealed record CallResultApiResult(
    [property: JsonPropertyName("ivr_call_result_id")] string IvrCallResultId,
    [property: JsonPropertyName("ivr_call_job_id")] string IvrCallJobId,
    [property: JsonPropertyName("result_type")] string ResultType,
    [property: JsonPropertyName("result_reason")] string? ResultReason,
    [property: JsonPropertyName("is_counted_customer_attempt")] bool IsCountedCustomerAttempt,
    [property: JsonPropertyName("is_final_for_ivr")] bool IsFinalForIvr,
    [property: JsonPropertyName("recommended_core_action")] string RecommendedCoreAction,
    [property: JsonPropertyName("input_signal_only")] bool InputSignalOnly,
    [property: JsonPropertyName("no_direct_order_update")] bool NoDirectOrderUpdate,
    [property: JsonPropertyName("no_payment_or_revenue_effect")] bool NoPaymentOrRevenueEffect);

public sealed record ResultCallbackApiResult(
    [property: JsonPropertyName("callback_id")] string CallbackId,
    [property: JsonPropertyName("ivr_call_result_id")] string IvrCallResultId,
    [property: JsonPropertyName("result_state")] string ResultState,
    [property: JsonPropertyName("delivery_status")] string DeliveryStatus,
    [property: JsonPropertyName("retry_count")] int RetryCount,
    [property: JsonPropertyName("requires_core_revalidation")] bool RequiresCoreRevalidation);

public sealed record QueueProjectionApiResult(
    [property: JsonPropertyName("paused")] bool Paused,
    [property: JsonPropertyName("pending_jobs")] int PendingJobs,
    [property: JsonPropertyName("active_attempts")] int ActiveAttempts,
    [property: JsonPropertyName("enabled_channels")] int EnabledChannels,
    [property: JsonPropertyName("open_hold_incidents")] int OpenHoldIncidents,
    [property: JsonPropertyName("projected_at")] DateTimeOffset ProjectedAt);

public sealed record AdminActionApiResult(
    [property: JsonPropertyName("admin_action_id")] string AdminActionId,
    [property: JsonPropertyName("action_type")] string ActionType,
    [property: JsonPropertyName("target_type")] string TargetType,
    [property: JsonPropertyName("target_id")] string TargetId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("no_policy_bypass")] bool NoPolicyBypass);

public sealed record TechnicalRetryApiResult(
    [property: JsonPropertyName("admin_action_id")] string AdminActionId,
    [property: JsonPropertyName("technical_exception_id")] string TechnicalExceptionId,
    [property: JsonPropertyName("target_attempt_id")] string TargetAttemptId,
    [property: JsonPropertyName("technical_retry_count")] int TechnicalRetryCount,
    [property: JsonPropertyName("customer_attempt_counted")] bool CustomerAttemptCounted,
    [property: JsonPropertyName("queue_status")] string QueueStatus,
    [property: JsonPropertyName("no_policy_bypass")] bool NoPolicyBypass);

public sealed record AdminReviewApiResult(
    [property: JsonPropertyName("admin_action_id")] string AdminActionId,
    [property: JsonPropertyName("review_item_id")] string ReviewItemId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("resolution")] string Resolution,
    [property: JsonPropertyName("result_unchanged")] bool ResultUnchanged,
    [property: JsonPropertyName("no_policy_bypass")] bool NoPolicyBypass);

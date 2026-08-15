/**
 * Hand-written mirror of the operations the admin UI consumes from
 * `specs/api/openapi/ivr-order-confirmation.v1.yaml`.
 *
 * The generated .NET DTOs (`src/Ivr.Contracts/Generated/**`, P1-1) are the
 * server-side contract source; there is no committed TypeScript generator yet.
 * `tests/unit/openapi-contract-drift.test.ts` therefore reads the YAML directly
 * and fails if a required property is added, removed or renamed here.
 */

/** API-06 §1c — stable error code catalogue. */
export const IVR_ERROR_CODES = [
  "IVR_UNAUTHENTICATED",
  "IVR_FORBIDDEN_CALLER",
  "IVR_MALFORMED_REQUEST",
  "IVR_MISSING_TRACE",
  "IVR_IDEMPOTENCY_CONFLICT",
  "IVR_VERSION_CONFLICT",
  "IVR_NOT_OFFICIAL_ORDER",
  "IVR_STATE_NOT_CALLABLE",
  "IVR_POLICY_MISMATCH",
  "IVR_CONTACT_INVALID",
  "IVR_SCRIPT_NOT_APPROVED",
  "IVR_PII_POLICY_VIOLATION",
  "IVR_OPERATIONAL_BLOCKED",
  "IVR_NOT_FOUND",
  "IVR_RATE_LIMITED",
  "IVR_INTERNAL_ERROR",
] as const;

export type IvrErrorCode = (typeof IVR_ERROR_CODES)[number];

const ERROR_CODE_LOOKUP: ReadonlySet<string> = new Set(IVR_ERROR_CODES);

export function isIvrErrorCode(value: unknown): value is IvrErrorCode {
  return typeof value === "string" && ERROR_CODE_LOOKUP.has(value);
}

/** OpenAPI `ErrorEnvelope`. */
export interface IvrErrorEnvelope {
  readonly error: {
    readonly code: string;
    readonly message: string;
    readonly correlationId: string;
    readonly details?: Readonly<Record<string, string>>;
  };
}

/** OpenAPI `IvrQueueProjection` — masked queue/capacity projection. */
export interface IvrQueueProjection {
  readonly paused: boolean;
  readonly pending_jobs: number;
  readonly active_attempts: number;
  readonly enabled_channels: number;
  readonly open_hold_incidents: number;
  readonly projected_at: string;
}

/** OpenAPI `IvrDashboardQueuePanel`. */
export interface IvrDashboardQueuePanel {
  readonly paused: boolean;
  readonly queued: number;
  readonly held_mock: number;
  readonly held_admin_review: number;
  readonly dispatching: number;
  readonly open_total: number;
  readonly closed_total: number;
  readonly near_expiry: number;
}

/** OpenAPI `IvrDashboardResultPanel` — rates are computed by the API, never here. */
export interface IvrDashboardResultPanel {
  readonly total: number;
  readonly by_result_type: Readonly<Record<string, number>>;
  readonly confirm_rate: number;
  readonly cancel_rate: number;
  readonly no_answer_rate: number;
  readonly technical_exception_rate: number;
}

/** OpenAPI `IvrDashboardAttemptPanel`. */
export interface IvrDashboardAttemptPanel {
  readonly total: number;
  readonly counted_customer_attempts: number;
  readonly technical_retries: number;
  readonly active: number;
}

/** OpenAPI `IvrDashboardSimPanel`. */
export interface IvrDashboardSimPanel {
  readonly total: number;
  readonly enabled: number;
  readonly idle: number;
  readonly active: number;
  readonly disabled: number;
  readonly health_failed: number;
  readonly quarantined: number;
  readonly adapter_mode: string;
}

/** OpenAPI `IvrCapacityIncidentSummary`. */
export interface IvrCapacityIncidentSummary {
  readonly capacity_incident_id: string;
  readonly scope: string;
  readonly status: string;
  readonly hold_new_calls: boolean;
  readonly shortage_reason?: string;
  readonly missed_deadline_count: number;
  readonly opened_at: string;
}

/** OpenAPI `IvrDashboardProjection`. */
export interface IvrDashboardProjection {
  readonly generated_at: string;
  readonly execution_mode: string;
  readonly sim_provider: string;
  readonly real_customer_call_allowed: boolean;
  readonly program_filter?: string;
  readonly from?: string;
  readonly to?: string;
  readonly queue: IvrDashboardQueuePanel;
  readonly results: IvrDashboardResultPanel;
  readonly attempts: IvrDashboardAttemptPanel;
  readonly sim: IvrDashboardSimPanel;
  readonly open_incidents: readonly IvrCapacityIncidentSummary[];
  readonly missed_deadline_count: number;
}

/** OpenAPI `IvrCallJobListItem`. */
export interface IvrCallJobListItem {
  readonly ivr_call_job_id: string;
  readonly task_id: string;
  readonly order_code_short: string;
  readonly phone_masked: string;
  readonly program_type: string;
  readonly status: string;
  readonly queue_status: string;
  readonly attempt_count: number;
  readonly max_attempts: number;
  readonly result_type?: string;
  readonly expires_at: string;
  readonly created_at: string;
  readonly closed_at?: string;
  readonly near_expiry: boolean;
}

/** OpenAPI `IvrCallJobPage`. */
export interface IvrCallJobPage {
  readonly page: number;
  readonly page_size: number;
  readonly total_count: number;
  readonly items: readonly IvrCallJobListItem[];
}

/** OpenAPI `IvrCallAttemptDetail`. */
export interface IvrCallAttemptDetail {
  readonly ivr_call_attempt_id: string;
  readonly attempt_number: number;
  readonly scheduled_at: string;
  readonly started_at?: string;
  readonly ended_at?: string;
  readonly status: string;
  readonly result_status?: string;
  readonly disposition?: string;
  readonly dtmf_key?: string;
  readonly is_counted_customer_attempt: boolean;
  readonly technical_retry_count: number;
  readonly technical_exception_type?: string;
  readonly sim_channel_id?: string;
  readonly blocked_reason?: string;
  readonly policy_version: string;
  readonly script_version: string;
}

/** OpenAPI `IvrCallResultDetail`. */
export interface IvrCallResultDetail {
  readonly ivr_call_result_id: string;
  readonly result_type: string;
  readonly result_reason?: string;
  readonly dtmf_key?: string;
  readonly is_counted_customer_attempt: boolean;
  readonly is_final_for_ivr: boolean;
  readonly recommended_core_action: string;
  readonly human_review_required: boolean;
  readonly created_at: string;
}

/** OpenAPI `IvrResultCallbackDetail`. */
export interface IvrResultCallbackDetail {
  readonly callback_id: string;
  readonly ivr_call_result_id: string;
  readonly result_state: string;
  readonly delivery_status: string;
  readonly core_http_status?: number;
  readonly core_response_code?: string;
  readonly retry_count: number;
  readonly requires_core_revalidation: boolean;
  readonly created_at: string;
  readonly sent_at?: string;
  readonly acknowledged_at?: string;
}

/** OpenAPI `IvrTechnicalExceptionDetail`. */
export interface IvrTechnicalExceptionDetail {
  readonly technical_exception_id: string;
  readonly ivr_call_attempt_id: string;
  readonly exception_type: string;
  readonly customer_attempt_counted: boolean;
  readonly technical_retry_allowed: boolean;
  readonly technical_retry_count: number;
  readonly created_at: string;
}

/** OpenAPI `IvrReviewItemDetail`. */
export interface IvrReviewItemDetail {
  readonly review_item_id: string;
  readonly source_type: string;
  readonly source_id: string;
  readonly reason: string;
  readonly status: string;
  readonly resolution?: string;
  readonly created_at: string;
  readonly resolved_at?: string;
}

/** OpenAPI `IvrCallJobDetail`. */
export interface IvrCallJobDetail {
  readonly ivr_call_job_id: string;
  readonly task_id: string;
  readonly order_code_short: string;
  readonly phone_masked: string;
  readonly program_type: string;
  /** Opaque Core enum. Rendered as text; the console never derives or changes it (D-02). */
  readonly order_state: string;
  readonly order_version_snapshot: string;
  readonly status: string;
  readonly queue_status: string;
  readonly eligible: boolean;
  readonly eligibility_decision: string;
  readonly blocked_reasons: readonly string[];
  readonly call_restriction: boolean;
  readonly sellable_captured_at?: string;
  readonly max_attempts: number;
  readonly attempt_policy_code: string;
  readonly script_version: string;
  readonly privacy_policy_version: string;
  readonly t0_at: string;
  readonly expires_at: string;
  readonly created_at: string;
  readonly closed_at?: string;
  readonly closed_reason?: string;
  readonly attempts: readonly IvrCallAttemptDetail[];
  readonly results: readonly IvrCallResultDetail[];
  readonly callbacks: readonly IvrResultCallbackDetail[];
  readonly technical_exceptions: readonly IvrTechnicalExceptionDetail[];
  readonly review_items: readonly IvrReviewItemDetail[];
  readonly evidence_refs: readonly string[];
  readonly audit_refs: readonly string[];
  readonly correlation_id: string;
  readonly input_signal_only: boolean;
  readonly no_direct_order_update: boolean;
}

/** OpenAPI `IvrScriptApproval`. */
export interface IvrScriptApproval {
  readonly approval_type: string;
  readonly actor_id: string;
  readonly reason: string;
  readonly correlation_id: string;
  readonly approved_at: string;
}

/** OpenAPI `IvrScriptVersion`. */
export interface IvrScriptVersion {
  readonly template_id: string;
  readonly version: string;
  readonly status: string;
  readonly template_hash: string;
  readonly allowed_input_fields: readonly string[];
  readonly approvals: readonly IvrScriptApproval[];
  readonly missing_approvals: readonly string[];
  readonly template_valid: boolean;
  readonly uses_production_decision_fields: boolean;
  readonly created_by: string;
  readonly created_at: string;
  readonly submitted_by?: string;
  readonly submitted_at?: string;
  readonly retired_by?: string;
  readonly retired_at?: string;
}

/** OpenAPI `IvrDtmfKey`. */
export interface IvrDtmfKey {
  readonly key: string;
  readonly meaning: string;
  readonly enabled: boolean;
}

/** OpenAPI `IvrScriptCatalog`. */
export interface IvrScriptCatalog {
  readonly generated_at: string;
  readonly execution_mode: string;
  /** OD-V1-15 lock. False means the Target V1 field set is not production approved. */
  readonly production_target_v1_fields_approved: boolean;
  readonly allowed_input_fields: readonly string[];
  readonly prohibited_variables: readonly string[];
  readonly dtmf_map: readonly IvrDtmfKey[];
  readonly required_approval_types: readonly string[];
  readonly versions: readonly IvrScriptVersion[];
}

/** OpenAPI `IvrDependencyStatus`. */
export interface IvrDependencyStatus {
  readonly dependency: string;
  readonly state: "UP" | "DOWN" | "READY_503" | "NOT_WIRED";
  readonly detail: string;
  readonly fail_closed_effect: string;
  readonly observed: boolean;
  readonly captured_at?: string;
}

/** OpenAPI `IvrFailClosedEvent`. */
export interface IvrFailClosedEvent {
  readonly source: string;
  readonly reference: string;
  readonly effect: string;
  readonly correlation_id: string;
  readonly occurred_at: string;
}

/** OpenAPI `IvrIntegrationStatus`. */
export interface IvrIntegrationStatus {
  readonly generated_at: string;
  readonly execution_mode: string;
  readonly sales_provider: string;
  readonly sim_provider: string;
  readonly real_customer_call_allowed: boolean;
  readonly global_dial_kill_switch: boolean;
  readonly attempt_policy_version: string;
  readonly flag_revision: number;
  /** False until P6-1 (W-0040); no card may be read as verified fail-closed. */
  readonly dependency_probing_available: boolean;
  readonly dependencies: readonly IvrDependencyStatus[];
  readonly recent_fail_closed_events: readonly IvrFailClosedEvent[];
}

/** OpenAPI `IvrReviewQueueItem`. */
export interface IvrReviewQueueItem {
  readonly review_item_id: string;
  readonly source_type: string;
  readonly source_id: string;
  readonly reason: string;
  readonly status: string;
  readonly resolution?: string;
  readonly correlation_id: string;
  readonly ivr_call_job_id?: string;
  readonly order_code_short?: string;
  readonly result_type?: string;
  readonly created_at: string;
  readonly resolved_at?: string;
}

/** OpenAPI `IvrReviewQueue`. */
export interface IvrReviewQueue {
  readonly page: number;
  readonly page_size: number;
  readonly total_count: number;
  readonly items: readonly IvrReviewQueueItem[];
}

/** OpenAPI `AdminMutationRequest` — every admin mutation carries a reason. */
export interface AdminMutationRequest {
  readonly reason: string;
  readonly evidence_ref?: string;
}

/** OpenAPI `IvrAdminActionResult`. */
export interface IvrAdminActionResult {
  readonly admin_action_id: string;
  readonly action_type: string;
  readonly target_type: string;
  readonly target_id: string;
  readonly status: string;
  readonly correlation_id: string;
  readonly no_policy_bypass: boolean;
}

/** OpenAPI `TechnicalRetryRequest`. */
export interface TechnicalRetryRequest {
  readonly technical_exception_id: string;
  readonly target_attempt_id: string;
  readonly reason: string;
  readonly evidence_ref?: string;
}

/** OpenAPI `IvrTechnicalRetryResult`. */
export interface IvrTechnicalRetryResult {
  readonly admin_action_id: string;
  readonly technical_exception_id: string;
  readonly target_attempt_id: string;
  readonly technical_retry_count: number;
  readonly customer_attempt_counted: boolean;
  readonly queue_status: string;
  readonly no_policy_bypass: boolean;
}

/** OpenAPI `AdminReviewRequest`. */
export interface AdminReviewRequest {
  readonly review_item_id: string;
  readonly resolution: string;
  readonly reason: string;
  readonly evidence_ref?: string;
}

/** OpenAPI `IvrAdminReviewResult`. */
export interface IvrAdminReviewResult {
  readonly admin_action_id: string;
  readonly review_item_id: string;
  readonly status: string;
  readonly resolution: string;
  readonly result_unchanged: boolean;
  readonly no_policy_bypass: boolean;
}

# OpenAPI Contract Baseline and Human-Readable Diff

Contract state: `TARGET_CONTRACT_V1=DRAFT`.

> Generated deterministically from the pinned manifest. A changed hash or report fails CI. Regeneration never upgrades DRAFT or accepts upstream drift by itself.

Generator: `NSwag.ConsoleCore 14.7.1`.
Verified current Sales source: `ginsengfood-business-platform@a3aad246d986fbc273cf41aaa93eec6659669656`.

## ivr-owned-target-v1-draft

- Role/status: IVR server DTO source / `TARGET_DRAFT`
- Source: `specs/api/openapi/ivr-order-confirmation.v1.yaml`
- SHA-256: `4dd221befe0e2cd8b5bc090ec0179ca3581caa928abf268ba865fd30c31316d4`
- Title/version: IVR Order Confirmation — Internal/Admin API (Target V1 Draft) / `1.0.0`
- Generated: `src/Ivr.Contracts/Generated/IvrServer/V1/IvrServerModels.g.cs`
- Operations (17): POST /tasks (intakeTask); POST /eligibility-checks (recordEligibility); POST /call-jobs (createCallJob); GET /call-jobs/{ivrCallJobId} (getCallJob); POST /call-attempts (recordAttempt); POST /call-results (recordResult); POST /result-callbacks (recordResultCallback); GET /queue (getQueue); POST /queue:pause (pauseQueue); POST /queue:resume (resumeQueue); POST /sim-channels/{simChannelId}:disable (disableSim); POST /sim-channels/{simChannelId}:enable (enableSim); POST /technical-retries (technicalRetry); POST /admin-reviews (adminReview); GET /feature-flags/{environment} (getFeatureFlags); POST /feature-flags/{environment} (mutateFeatureFlags); GET /feature-flags/{environment}/kill-switch (verifyFeatureFlagKillSwitch)
- Schemas (37): AdminMutationRequest, AdminReviewRequest, CallAttemptLifecycleRequest, CallJobLifecycleRequest, CallResultLifecycleRequest, CallbackCoreResponseTarget, EligibilityLifecycleRequest, ErrorCode, ErrorEnvelope, FeatureFlagChangeSet, FeatureFlagMutationRequest, FeatureFlagMutationResult, FeatureFlagReadResult, FeatureFlagSnapshot, IvrAdminAction, IvrAdminActionResult, IvrAdminReviewResult, IvrCallAttempt, IvrCallJob, IvrCallResult, IvrConfirmationResultCallbackTargetV1, IvrConfirmationTaskV1, IvrEligibilityDecision, IvrQueueProjection, IvrResultCallbackLifecycle, IvrTaskIntakeResult, IvrTechnicalException, IvrTechnicalRetryResult, KillSwitchVerification, OrderSpeechItem, PrivacySafeOrderSummary, ProgramCode, RecommendedCoreAction, ResultCallbackLifecycleRequest, ResultType, SellableStatusLine, TechnicalRetryRequest
- Primary required fields (22): contract_version, task_id, order_id, order_code, order_version, order_state, payment_method_snapshot, program_code, ivr_confirmation_required, confirmation_window_started_at, confirmation_window_expires_at, attempt_policy_version, max_customer_attempts, attempt_offsets_seconds, phone_ref, phone_masked, dial_token, dial_token_expires_at, privacy_safe_order_summary, call_restriction, eligibility_snapshot, evidence_ref
- Notes: servers=/v1/ivr/order-confirmation; security=bearerAuth

## sales-callback-target-v1-draft

- Role/status: Sales callback client source / `TARGET_DRAFT`
- Source: `specs/api/openapi/order-core-ivr-callback.target-v1.yaml`
- SHA-256: `1677d490eea5484e449ace3310e26e3c59acbb8011c7c1736e3f981afffa96ee`
- Title/version: Sales Order Core — IVR Result Callback Target V1 / `1.0.0-draft`
- Generated: `src/Ivr.Contracts/Generated/SalesTarget/V1/SalesTargetV1Client.g.cs`
- Operations (1): POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks (submitIvrOrderConfirmationResult)
- Schemas (6): CallbackAck200, CallbackAck409, ErrorResponse, IvrResultCallbackV1, RecommendedCoreAction, ResultType
- Primary required fields (13): contract_version, callback_id, task_id, order_id, order_version_seen_by_ivr, result_type, is_counted_customer_attempt, is_final_for_ivr, attempt_number, occurred_at, recommended_core_action, evidence_ref, audit_ref
- Notes: servers=https://sales-platform.invalid; security=serviceJwt

## sales-current-golden-hour-compat-a3aad246

- Role/status: Verified current compatibility fixture / `CURRENT_COMPAT_VERIFIED_AT_PINNED_SHA`
- Source: `specs/api/compat/current-golden-hour-callback.a3aad246.schema.json`
- SHA-256: `ad2f655070b14d0cdfb0540893f7d7ea83354dda56c4b403ae47f56a3f6a494d`
- Title/version: CurrentGoldenHourCallbackRequest / `pinned-source`
- Generated: hand-written isolated compatibility DTO
- Operations (1): POST /api/v1/internal/ivr/golden-hour/callbacks
- Schemas (2): CurrentGoldenHourCallbackRequest, CurrentGoldenHourCallbackResponse
- Primary required fields (6): callId, reservationId, orderId, customerId, result, idempotencyKey
- Notes: auth=X-Internal-Token transitional shared secret; unsupported Target fields=contract_version, callback_id, task_id, order_version_seen_by_ivr, attempt_number, is_counted_customer_attempt, is_final_for_ivr, recommended_core_action, evidence_ref, audit_ref, semantic_ack

## Review boundary

- Target and current DTOs remain unrelated types and use different clients.
- `https://sales-platform.invalid` and `https://identity.invalid` are deliberate placeholders, never production configuration.
- Any required-field, enum, path, auth, ACK, or privacy change needs human review and a new pinned hash; the command requires the explicit `--accept-reviewed-draft` flag.
- Real Sales approval, sandbox, auth and CDC evidence remain `BLOCKED_EXTERNAL`.

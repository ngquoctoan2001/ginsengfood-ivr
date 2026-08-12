# DB-02 — Tables

Trạng thái: `TARGET_V1_DRAFT` · Policy values are versioned/configurable; exact candidate timings are not database invariants.
Cột: `type semantic · required · index · note`. Tên bảng đề xuất; giữ semantic.

## 1. `ivr_confirmation_tasks`
| Column | Type | Req | Index | Note |
| --- | --- | --- | --- | --- |
| `id` | uuid | ✓ | PK | internal |
| `task_id` | string | ✓ | Unique | contract id |
| `version` | string | ✓ | | contract version, e.g. `ivr-order-confirmation.v1` |
| `idempotency_key` | string | ✓ | Unique(scoped) | chống duplicate task |
| `correlation_id` | string | ✓ | idx | trace |
| `official_order_id` | string | ✓ | idx | **không** source-of-truth |
| `order_code` | string | ○ | | display/audit |
| `order_version` | string | ✓ | idx | Target V1 race snapshot; current-compat may use isolated nullable DTO, not this target table invariant |
| `order_state` | string | ✓ | idx | opaque snapshot; Sales owns callable states |
| `payment_method_snapshot` | string | ✓ | idx | `ONLINE` for GH or `COD` for 24/7; IVR does not process payment |
| `ivr_confirmation_required` | bool | ✓ | idx | must be true |
| `customer_id` | string | ○ | idx | không full profile |
| `customer_trust_status` | string | ✓ | | D-12 |
| `trusted_skip_allowed` | bool | ✓ | | D-12 |
| `risk_flags_json` | json | ✓ | | boolean source-backed (D-13) |
| `program_type` | string | ✓ | idx | `GOLDEN_HOUR`/`TWENTY_FOUR_SEVEN` |
| `attempt_policy_version` | string | ✓ | idx | owner-approved in production; candidate allowed only MOCK/LAB |
| `max_attempts` | int | ✓ | | bounded by app policy, not fixed to 2 |
| `attempt_offsets_seconds_json` | json | ✓ | | ordered offsets including 0 |
| `confirmation_window_started_at` | datetime | ✓ | idx | source timestamp |
| `confirmation_window_expires_at` | datetime | ✓ | idx | source deadline |
| `official_contact_id` | string | ✓ | idx | contact duyệt |
| `phone_ref` | string | ✓ | | secure ref (D-05) — **không raw** |
| `phone_masked` | string | ✓ | | admin-safe |
| `phone_validation_status` | string | ✓ | idx | không unknown khi dispatch |
| `dial_token` | string/encrypted | ✓ | | opaque token only; never raw phone |
| `dial_token_expires_at` | datetime | ✓ | idx | must not exceed call window |
| `privacy_safe_order_summary_json` | json | ✓ | | short name/code, public items+qty, total, short area; schema validated |
| `eligibility_decision` | string | ✓ | idx | enum |
| `blocked_reasons_json` | json | ✓ | | danh sách block |
| `sellable_status_json` | json | ✓ | | **per-line SKU/batch snapshot** (DO-02): `[{sku_id,batch_id?,decision,recall_hold,sale_lock,quality_hold,stock_available,batch_released,trace_ready,captured_at}]` |
| `sellable_captured_at` | datetime | ○ | | max độ tươi snapshot |
| `call_restriction` | bool | ○ | | ✅ do-not-call từ CRM (DC-01); nullable tới IR-CRM-01 |
| `not_for_quote_cart_draft` | bool | ✓ | | must true |
| `no_direct_order_update` | bool | ✓ | | must true |
| `created_at` | datetime | ✓ | idx | server time |
| `expires_at` | datetime | ✓ | idx | = `t0_at + window` |
| `accepted_at`/`rejected_at` | datetime | ○ | | |
| `reject_reason` | string | ○ | | machine-readable |
| `evidence_refs_json`/`audit_refs_json` | json | ○ | | |

**Constraints:** Unique(`task_id`); Unique(`idempotency_key` scope intake); CHECK `max_attempts BETWEEN 1 AND 10`; CHECK offsets nonnegative/strictly increasing and before expiry in application/domain validation; CHECK program/payment matrix (GH+ONLINE or 24/7+COD); CHECK `ivr_confirmation_required=true`. Exact 2/300/150/900/450 candidate values are configuration, not DB constraints.

## 2. `ivr_call_jobs`
| Column | Type | Req | Index | Note |
| --- | --- | --- | --- | --- |
| `id`/`ivr_call_job_id` | uuid/string | ✓ | PK/Unique | |
| `task_id` | string | ✓ | FK/idx | |
| `official_order_id` | string | ✓ | idx | snapshot ref |
| `order_version_snapshot` | string | ✓ | idx | Target V1 race guard |
| `program_type` | string | ✓ | idx | |
| `attempt_policy_code` | string | ✓ | | policy version |
| `status` | string | ✓ | idx | `ivr-call-job-status` |
| `max_attempts` | int | ✓ | | policy snapshot, bounded 1..10 |
| `attempt_offsets_seconds_json` | json | ✓ | | policy snapshot |
| `confirmation_window_seconds` | int | ✓ | | derived from task start/expiry |
| `attempt_schedule_json` | json | ✓ | | offsets từ `t0_at` |
| `t0_at` | datetime | ✓ | idx | |
| `eligible` | bool | ✓ | idx | |
| `eligibility_decision` | string | ✓ | idx | |
| `queue_status` | string | ✓ | idx | active/paused/held |
| `capacity_incident_id` | string | ○ | idx | |
| `script_version`/`privacy_policy_version` | string | ✓ | | |
| `input_signal_only`/`no_direct_order_update` | bool | ✓ | | must true |
| `created_at`/`closed_at` | datetime | ✓/○ | idx | |
| `closed_reason` | string | ○ | | |
| `evidence_refs_json`/`audit_refs_json` | json | ○ | | |

**Indexes:** `(status, expires_at)` (scheduler deadline), `(program_type,status)`, `(official_order_id,status)`.

## 3. `ivr_call_attempts`
| Column | Type | Req | Index | Note |
| --- | --- | --- | --- | --- |
| `id`/`ivr_call_attempt_id` | uuid/string | ✓ | PK/Unique | |
| `ivr_call_job_id` | string | ✓ | FK/idx | |
| `task_id` | string | ✓ | idx | |
| `attempt_number` | int | ✓ | idx | 1..2 |
| `scheduled_at`/`scheduled_window_expires_at` | datetime | ✓ | idx | |
| `started_at`/`ended_at` | datetime | ○ | | |
| `status` | string | ✓ | idx | `ivr-call-attempt-status` |
| `result_status` | string | ○ | idx | |
| `dtmf_key` | string | ○ | | `1`/`0`/invalid/null |
| `disposition` | string | ○ | idx | raw→DT-02 |
| `is_counted_customer_attempt` | bool | ✓ | idx | false cho technical |
| `technical_retry_allowed`/`technical_retry_count` | bool/int | ✓ | | |
| `no_answer`/`invalid_phone` | bool | ✓ | | |
| `technical_exception_type` | string | ○ | idx | |
| `sim_channel_id` | string | ○ | idx | |
| `provider_call_id` | string | ○ | idx | nếu có |
| `raw_call_event_id` | string | ○ | FK | link raw event |
| `blocked_reason` | string | ○ | | |
| `policy_version`/`script_version` | string | ✓ | | |
| `evidence_refs_json`/`audit_refs_json` | json | ○ | | |

**Constraints:** Unique(`ivr_call_job_id`,`attempt_number`) cho customer-counted; CHECK `attempt_number ≤ 2` (D-10); `is_counted_customer_attempt=false` khi `technical_exception_type` not null.

## 4. `ivr_raw_call_event` (MỚI — OD-DR-03)
| Column | Type | Req | Index | Note |
| --- | --- | --- | --- | --- |
| `id`/`raw_event_id` | uuid/string | ✓ | PK/Unique | |
| `ivr_call_attempt_id` | string | ✓ | FK/idx | |
| `ivr_call_job_id` | string | ✓ | idx | |
| `provider_internal_payload_ref` | string | ○ | | **sanitized** ref, không PII thô |
| `raw_call_status` | string | ✓ | | disposition thô (DT-02) |
| `raw_dtmf` | string | ○ | | |
| `audio_status` | string | ○ | | |
| `technical_error_code` | string | ○ | idx | |
| `recording_ref` | string | ○ | | **null mặc định** (recording OFF DT-05) |
| `received_at` | datetime | ✓ | idx | |

## 5. `ivr_call_results`
| Column | Type | Req | Index | Note |
| --- | --- | --- | --- | --- |
| `id`/`ivr_call_result_id` | uuid/string | ✓ | PK/Unique | |
| `ivr_call_job_id` | string | ✓ | FK/idx | |
| `task_id`/`official_order_id` | string | ✓ | idx | |
| `order_version_snapshot` | string | ○ | idx(target) | order version từ task nếu Core cung cấp; current Core stale guard bằng state/COD/sellable recheck (DS-04) |
| `order_version_seen_by_ivr` | string | ✓ | idx | required Target V1 race guard; compatibility records use explicit legacy shape/provider |
| `final_result_status`/`result_type` | string | ✓ | idx | |
| `result_reason`/`dtmf_key` | string | ○ | | |
| `is_counted_customer_attempt`/`is_final_for_ivr` | bool | ✓ | idx(final) | |
| `recommended_core_action` | string | ✓ | | advisory |
| `core_order_handoff_required`/`human_review_required` | bool | ✓ | idx(review) | |
| `input_signal_only`/`no_direct_order_update`/`no_payment_or_revenue_effect` | bool | ✓ | | must true |
| `technical_error_code` | string | ○ | | |
| `created_at` | datetime | ✓ | idx | |
| `evidence_refs_json`/`audit_refs_json` | json | ○ | | |

## 6. `ivr_result_callbacks`
| Column | Type | Req | Index | Note |
| --- | --- | --- | --- | --- |
| `callback_id` | string | ✓ | PK/Unique | |
| `ivr_call_result_id` | string | ✓ | FK/idx | |
| `task_id`/`official_order_id` | string | ✓ | idx | |
| `idempotency_key` | string | ✓ | Unique(scoped) | |
| `result_status`/`result_state` | string | ✓ | idx | state machine phase-8/07 |
| `requires_core_revalidation` | bool | ✓ | | must true |
| `sent_at`/`acknowledged_at` | datetime | ○ | idx | |
| `core_http_status` | int | ○ | idx | current Core response: `200` accept / `422` invalid state-COD (DS-03) |
| `core_response_code` | string | ○ | idx(target) | target semantic code: accepted/stale/blocked/review/retry (IR-SALES-OC2) |
| `retry_count` | int | ✓ | | bounded |
| `last_retry_at`/`next_retry_at` | datetime | ○ | idx(next) | |
| `last_error` | string | ○ | | sanitized |

## 7. Bảng vận hành phụ trợ
| Bảng | Field chính |
| --- | --- |
| `ivr_sim_channels` | `sim_channel_id`(PK), `sim_number_ref`, `enabled`, `status` (IDLE/RESERVED/ACTIVE_CALL/DISABLED/HEALTH_FAILED), `active_call_job_id`, `fail_count`, `last_health_check_at`, `cooldown_until`, `disabled_reason`, **`adapter_mode`** (MOCK/REAL — DT-01) |
| `ivr_capacity_incidents` | `capacity_incident_id`(PK), `session_id`, `program_code`, `status`, `scope`, `hold_new_calls`, `active_sim_count`, `pending_call_jobs`, `expired_call_jobs`, `missed_deadline_count`, `shortage_reason`, `opened_at`, `resolved_at`, `reason` |
| `ivr_technical_exceptions` | `technical_exception_id`(PK), `ivr_call_attempt_id`, `exception_type`, `customer_attempt_counted=false`, `technical_retry_allowed`, `technical_retry_count`, `retry_reason`, `correlation_id`, `created_at` |
| `ivr_admin_actions` | `admin_action_id`(PK), `action_type`, `permission`, `actor_id`, `target_type`, `target_id`, `reason`, `before_state`, `after_state`, `correlation_id`, `evidence_ref`, `no_policy_bypass=true`, `created_at` |
| `ivr_evidence_links` | `owner_table`, `owner_id`, `evidence_ref`, `audit_ref` |

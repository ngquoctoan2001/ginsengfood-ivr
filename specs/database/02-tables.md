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
| `customer_trust_status` | string | ○ | | `LEGACY_READ`, deprecated/ignored bởi active eligibility; giữ cột cho history/rolling rollback |
| `trusted_skip_allowed` | bool | ○ | | `LEGACY_READ`, deprecated/ignored bởi active eligibility; giữ cột cho history/rolling rollback |
| `risk_flags_json` | json | ○ | | Sales-supplied optional; chỉ dùng audit/scheduler priority, không quyết định call/skip (`OD-18`) |
| `program_type` | string | ✓ | idx | `GOLDEN_HOUR`/`TWENTY_FOUR_SEVEN` |
| `attempt_policy_version` | string | ✓ | idx | owner-approved in production; candidate allowed only MOCK/LAB |
| `max_attempts` | int | ✓ | | bounded by app policy, not fixed to 2 |
| `attempt_offsets_seconds_json` | json | ✓ | | ordered offsets including 0 |
| `confirmation_window_started_at` | datetime | ✓ | idx | source timestamp |
| `confirmation_window_expires_at` | datetime | ✓ | idx | source deadline |
| `official_contact_id` | string | ○ | idx | **IVR-derived** từ `phone_ref`; không có trên task contract, nullable tới khi resolver cung cấp |
| `phone_ref` | string | ✓ | | secure ref (D-05) — **không raw** |
| `phone_masked` | string | ✓ | | admin-safe |
| `phone_validation_status` | string | ○ | idx | **Sales-supplied optional**; null/`unknown` ⇒ **không dispatch** (fail-closed) |
| `dial_token` | string/encrypted | ✓ | | opaque token only; never raw phone |
| `dial_token_expires_at` | datetime | ✓ | idx | must not exceed call window |
| `privacy_safe_order_summary_json` | json | ✓ | | short name/code, public items+qty, total, short area; schema validated |
| `call_script_template_id` / `call_script_version` | string | ✓ | | exact approved script snapshot resolved by IVR at intake; immutable |
| `evidence_policy_version` / `privacy_policy_version` | string | ✓ | | IVR-resolved policy snapshots; MOCK uses explicit synthetic defaults, non-MOCK must supply/resolve real versions |
| `eligibility_decision` | string | ○ | idx | **IVR-derived** (P2-2 ghi sau intake); null tới khi eligibility chạy |
| `blocked_reasons_json` | json | ○ | | **IVR-derived**; null tới khi eligibility chạy |
| `call_restriction` | bool | ✓ | | **Sales-supplied**, required trên wire (OpenAPI) và NOT NULL ở đây; `true`/`unknown` → fail-closed (DC-01) |
| `not_for_quote_cart_draft` | bool | ✓ | | **IVR-derived invariant**, server-default `true`; không nhận từ wire |
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

## 2.1. `ivr_task_intake_outbox` (W-0018)

| Column | Type | Req | Index | Note |
| --- | --- | --- | --- | --- |
| `outbox_id` | uuid | ✓ | PK | server-generated |
| `task_id` | string | ✓ | Unique/FK | exactly one intake event per accepted task |
| `ivr_call_job_id` | string | ✓ | Unique/FK | exactly one intake event per created/dry-run job |
| `event_type` | string | ✓ | | `IVR_TASK_DRY_RUN_RECORDED` or `IVR_TASK_READY_FOR_ELIGIBILITY` |
| `status` | string | ✓ | `(status,created_at)` | `HELD_MOCK`, `READY_FOR_ELIGIBILITY`, `PUBLISHED` |
| `correlation_id` | string | ✓ | idx | privacy-safe trace only |
| `payload_sha256` | string | ✓ | CHECK | uppercase SHA-256 of canonical request JSON; never stores request body |
| `created_at` / `published_at` | datetime | ✓/○ | idx | MOCK remains held and is never dispatched to real telephony |

Task, call job, intake outbox, idempotency response snapshot and audit record are committed in one PostgreSQL transaction. Rejected/held requests persist only the idempotency decision and privacy-safe audit; they create zero task/job/outbox rows. Outbox identity/payload fields are immutable by trigger; only lifecycle status/published time and retention fields may advance.

## 3. `ivr_call_attempts`
| Column | Type | Req | Index | Note |
| --- | --- | --- | --- | --- |
| `id`/`ivr_call_attempt_id` | uuid/string | ✓ | PK/Unique | |
| `ivr_call_job_id` | string | ✓ | FK/idx | |
| `task_id` | string | ✓ | idx | |
| `attempt_number` | int | ✓ | idx | `1..max_attempts_snapshot` (cùng hàng); KHÔNG hằng số |
| `max_attempts_snapshot` | int | ✓ | | **Persisted snapshot** copy từ `ivr_call_jobs.max_attempts` lúc INSERT. Tồn tại để CHECK là **same-row** — PostgreSQL không cho CHECK tham chiếu bảng khác. Immutable sau khi tạo. |
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

**Constraints:** Unique(`ivr_call_job_id`,`attempt_number`) cho customer-counted; **same-row** CHECK `attempt_number >= 1 AND attempt_number <= max_attempts_snapshot` (**không** CHECK hằng số `2`, xem DB-04 §4); `is_counted_customer_attempt=false` khi `technical_exception_type` not null.

> **PostgreSQL:** `CHECK` chỉ đánh giá được trên **một hàng của chính bảng đó** — không tham chiếu được `ivr_call_jobs`. Vì vậy bound policy được denormalize thành `max_attempts_snapshot`. Tính nhất quán `max_attempts_snapshot == ivr_call_jobs.max_attempts` được bảo đảm bằng **trigger `BEFORE INSERT`** (copy giá trị) hoặc bằng application invariant, **không** bằng CHECK. Xem DB-04 §4.

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
| `order_version_snapshot` | string | ○ | idx(target) | order version từ task nếu Core cung cấp; current Core stale guard bằng state/COD recheck (DS-04) |
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
| `ivr_sim_channels` | `sim_channel_id`(PK), `sim_number_ref`, `enabled`, `status` (IDLE/RESERVED/ACTIVE_CALL/QUARANTINED/DISABLED/HEALTH_FAILED), `active_call_job_id`, `fail_count`, `last_health_check_at`, `cooldown_until`, `quarantine_until`, `disabled_reason`, **`adapter_mode`** (MOCK/REAL — DT-01), **lease/fencing:** `lease_token`, `lease_fencing_generation`, `leased_by_worker_id`, `lease_acquired_at`, `lease_expires_at` (xem DB-04 §5) |
| `ivr_capacity_incidents` | `capacity_incident_id`(PK), `session_id`, `program_code`, `status`, `scope`, `hold_new_calls`, `active_sim_count`, `pending_call_jobs`, `expired_call_jobs`, `missed_deadline_count`, `shortage_reason`, `opened_at`, `resolved_at`, `reason` |
| `ivr_technical_exceptions` | `technical_exception_id`(PK), `ivr_call_attempt_id`, `exception_type`, `customer_attempt_counted=false`, `technical_retry_allowed`, `technical_retry_count`, `retry_reason`, `correlation_id`, `created_at` |
| `ivr_admin_actions` | `admin_action_id`(PK), `action_type`, `permission`, `actor_id`, `target_type`, `target_id`, `reason`, `before_state`, `after_state`, `correlation_id`, `evidence_ref`, `no_policy_bypass=true`, `created_at` |
| `ivr_evidence_links` | `owner_table`, `owner_id`, `evidence_ref`, `audit_ref` |

## 8. Bảng foundation / platform (định nghĩa entity ở P0-3/P0-4, migration ở P1-2)

> Các bảng này trước đây chỉ tồn tại trong prompt P0-3/P0-4/P4-6 mà không có trong DB spec, nên P1-2 (nguồn = `specs/database/*`) sẽ không tạo chúng. Bổ sung 2026-08-12 (W-0062).

| Bảng | Field chính | Owner prompt |
| --- | --- | --- |
| `ivr_idempotency_keys` | `key`(Unique scoped), `scope`, `payload_hash`, `response_snapshot_json`, `created_at`, `expires_at` | P0-3 |
| `ivr_audit_log` | `audit_id`(PK), `actor_id`, `actor_type`, `action`, `target_type`, `target_id`, `reason`, `before_state`, `after_state`, `correlation_id`, `created_at` — **append-only**, không UPDATE/DELETE | P0-3 |
| `ivr_evidence` | `evidence_ref`(PK), `kind`, `correlation_id`, `work_id`, `payload_ref` (đường dẫn `docs/evidence/<W-XXXX>/`), `created_at` | P0-3 |
| `ivr_feature_flags` | `key`, `env`, `enabled`, `value_json`, `updated_by`, `updated_at`, `reason` — Unique(`key`,`env`); mọi thay đổi ghi `ivr_audit_log` | P0-4 |
| `ivr_review_items` | `review_item_id`(PK), `source_type`, `source_id`, `reason`, `status`, `assigned_to`, `resolution`, `correlation_id`, `created_at`, `resolved_at` | P4-6 |

**Retention:** mỗi bảng trên phải khai báo data class và retention period trong `specs/database/05-retention-and-privacy.md`; job purge do `IRetentionJob` thực thi (xem prompt P1-5).

## 8.1. Script/content lifecycle (W-0024)

| Bảng | Field chính | Invariant |
| --- | --- | --- |
| `ivr_script_versions` | `id`(PK), `template_id` + `version`(Unique), `status`, `template_text`, `template_hash`, `allowed_input_fields_json`, create/submit/retire actor+reason+time | `DRAFT/IN_REVIEW/APPROVED/RETIRED`; content/identity immutable từ lúc approve; retire thay delete |
| `ivr_script_approvals` | `id`(PK), `script_version_id`(FK), `approval_type`, `actor_id`, `reason`, `correlation_id`, `approved_at` | Unique(version,type); `MOCK_TEST/LAB/CONTENT/PRIVACY_LEGAL`; append-only trigger |

Migration W-0024 seed duy nhất `SCRIPT-ORDER-CONFIRM:v1-test-approved` với `MOCK_TEST`; seed không cấp LAB/PROD. Bảng không chứa customer input, rendered speech, raw phone, full address hay recording.


(3–64 ký tự), display name tối đa 128, role `Admin|Operator`, status
`ACTIVE|DISABLED|DELETED`, built-in marker, PBKDF2 verifier, durable lockout,
login/password timestamps, optimistic version và retention/legal-hold columns.
Không có plaintext password hoặc cơ chế “lấy lại” password; Admin chỉ đặt mật
khẩu mới.

create/expiry/revoke metadata và retention/legal-hold columns. Token raw chỉ trả
một lần cho client và không được ghi DB/log/audit. Session TTL là 8 giờ; đổi
role/status, reset password hoặc soft-delete account phải revoke mọi session của
account đó.

Username unique trên toàn bảng, kể cả record đã soft-delete. FK session → account
dùng `RESTRICT`; retention xoá session child trước account đã soft-delete.

## 9. Phân loại nguồn cột (chống DB↔OpenAPI inversion)

Mỗi cột thuộc đúng một loại. `P1-2` và `P1-3` phải giữ phân loại này khi sinh entity/migration:

| Loại | Nghĩa | Quy tắc NOT NULL |
| --- | --- | --- |
| **Sales-supplied required** | có trong `required` của `IvrConfirmationTaskV1` | NOT NULL |
| **Sales-supplied optional** | có trong `properties` nhưng không `required` | NULLABLE; null phải có hành vi fail-closed ghi rõ |
| **IVR-derived** | IVR tính/ghi sau intake | NULLABLE lúc insert, hoặc server-default |
| **Persisted snapshot** | copy bất biến của policy/script/eligibility tại thời điểm intake | NOT NULL sau khi job tạo, immutable |
| **Internal-only** | không bao giờ xuất hiện trên wire contract | NOT NULL với server-default |

**Kiểm tra bắt buộc ở P1-2:** canonical fixture `seed/sales-target-v1.sample.json` phải INSERT được vào schema mà không vi phạm NOT NULL. Nếu vi phạm → hoặc cột sai loại, hoặc fixture thiếu field; sửa cả hai phía trong cùng work item.

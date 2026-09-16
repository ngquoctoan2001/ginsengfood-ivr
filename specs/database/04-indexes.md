# DB-04 — Indexes & Constraints

Trạng thái: `TARGET_V1_DRAFT` · Nguồn: `phase-8/12` §4-8,§12; DF-04, D-02; **TV1-02** (attempt policy versioned/configurable).

> **Realigned 2026-08-12 (W-0062).** §4 trước đây ghi `max_attempts = 2`, `window=300 ∧ spacing=150`, `window=900 ∧ spacing=450` và `attempt_number ≤ 2` như constraint P0. Điều này mâu thuẫn với `plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md` (“Không hard-code candidate vào database constraint hoặc domain constant”), `specs/database/02-tables.md` §header, `specs/functional/03-scheduler-attempt-policy.md` (“Database không CHECK exact `2/300/150/900/450`”) và cả hai OpenAPI (`minimum: 1, maximum: 10`). Các giá trị candidate đã được chuyển sang **config/policy registry**; DB chỉ enforce bounds/invariant.

## 1. Unique (idempotency / trace)
| Bảng | Unique | Mục đích |
| --- | --- | --- |
| `ivr_confirmation_tasks` | `task_id` | 1 task 1 record |
| `ivr_confirmation_tasks` | `idempotency_key` (scope intake) | chống duplicate task (DF-04) |
| `ivr_task_intake_outbox` | `task_id`; `ivr_call_job_id` | 1 atomic intake event cho mỗi task/job accepted |
| `ivr_call_jobs` | `ivr_call_job_id` | |
| `ivr_call_attempts` | `ivr_call_attempt_id`; `(ivr_call_job_id, attempt_number)` | 1 attempt-number/job |
| `ivr_call_results` | `ivr_call_result_id` | |
| `ivr_result_callbacks` | `callback_id`; `idempotency_key` (scope callback) | chống duplicate transition |
| `ivr_raw_call_events` | `raw_event_id` | |
| `ivr_idempotency_keys` | `key` (scope) | replay/conflict detection (P0-3) |
| `ivr_feature_flags` | `(key, env)` | 1 giá trị/flag/env (P0-4) |
| `ivr_sim_channels` | `sim_channel_id`; `lease_token` (partial, khi not null) | 1 lease sống/channel |

## 2. Index scheduler (deadline-aware — phase-8/12 §5)
- `ivr_call_jobs (status, expires_at)` — query job sắp hết window (rolling queue).
- `ivr_call_jobs (t0_at)` / `ivr_call_attempts (scheduled_at)` / `(scheduled_window_expires_at)`.
- `ivr_call_jobs (program_type, status)`; `(official_order_id, status)`.
- `ivr_sim_channels (status, enabled, cooldown_until)` — chọn SIM rảnh.
- `ivr_sim_channels (lease_expires_at)` — reclaim lease hết hạn sau worker crash.
- `ivr_result_callbacks (delivery_status, next_retry_at)` — outbox dequeue.
- `ivr_idempotency_keys (created_at)` — retention/purge scan.
- `ivr_task_intake_outbox (status, created_at)`, `(published_at)`, `(correlation_id)` — lifecycle/trace scan; MOCK rows stay `HELD_MOCK`.

## 3. Index race guard / lookup
- Current lookup: `order_state`, `program_type`, `official_order_id`, `task_id`, `correlation_id`; stale guard hiện chạy bằng Core recheck state/COD.
- Target race guard: required/indexed `order_version_snapshot` / `order_version_seen_by_ivr`; current-compat records are distinguishable and cannot weaken target validation.
- `correlation_id` trên tasks/results/technical_exceptions/admin_actions (trace).
- `official_order_id`, `task_id` trên các bảng con.
- `result_type`, `is_final_for_ivr`, `human_review_required` (query review).
- `core_http_status` (current), `core_response_code` (target), `next_retry_at` trên callbacks (retry scan).

## 4. Constraint P0 — **bounds và invariant only**

Attempt policy là **data/config gắn `attempt_policy_version`**, không phải schema invariant. DB chỉ được enforce:

| Constraint | Biểu thức | Ghi chú |
| --- | --- | --- |
| attempt bound | `max_attempts BETWEEN 1 AND 10` | safety bound, không phải policy value |
| attempt number | **same-row** `attempt_number >= 1 AND attempt_number <= max_attempts_snapshot` trên `ivr_call_attempts` | không so với hằng số; `max_attempts_snapshot` là cột denormalize (xem ghi chú dưới bảng) |
| offsets nonnegative | mọi phần tử `attempt_offsets_seconds_json >= 0` | application/domain validation |
| offsets strictly increasing | `offsets[i] < offsets[i+1]` | application/domain validation |
| offsets trong window | `max(offsets) < (confirmation_window_expires_at - confirmation_window_started_at)` | application/domain validation |
| offsets cardinality | `len(offsets) == max_attempts` | application/domain validation |
| snapshot consistency | `ivr_call_attempts.max_attempts_snapshot == ivr_call_jobs.max_attempts` | **trigger `BEFORE INSERT`** hoặc application invariant — **không** phải CHECK |
| policy snapshot immutable | `attempt_policy_version`, `max_attempts`, `attempt_offsets_seconds_json` trên `ivr_call_jobs` không được UPDATE sau khi job được tạo | job giữ snapshot policy tại thời điểm intake (`FR-IVR-SCH-009`) |
| intake snapshot immutable | script template/version + evidence/privacy policy trên task và outbox identity/payload không được UPDATE | PostgreSQL trigger; replay reads stored response instead of recreating rows |
| intake payload hash | `payload_sha256 ~ '^[A-F0-9]{64}$'` | canonical JSON hash only; request body/PII is not copied to outbox |
| technical ≠ no-answer | `is_counted_customer_attempt=false` khi `technical_exception_type` không null | P0-IVR-004 |
| program/payment matrix | `(GOLDEN_HOUR ∧ ONLINE) ∨ (TWENTY_FOUR_SEVEN ∧ COD)` | Target V1; xem `open-decisions-register` `OD-V1-01` |
| required flag | `ivr_confirmation_required = true` | |
| signal-only invariants | `not_for_quote_cart_draft`, `no_direct_order_update`, `input_signal_only`, `no_payment_or_revenue_effect` = true | |
| PII | không cột nào bắt buộc lưu **full phone**, **full address** hay **raw recording** (D-05/DT-05) | |
| lease exclusivity | tối đa 1 hàng `ivr_sim_channels` có `status='ACTIVE_CALL'` cho mỗi `sim_channel_id`; `active_call_job_id` không null ⇒ `lease_token` không null | one active call per channel (`FR-IVR-SCH-003`) |

> **Giới hạn của `CHECK` trong PostgreSQL.** `CHECK` chỉ đánh giá biểu thức trên **một hàng của chính bảng chứa nó**; nó **không** đọc được bảng khác (subquery trong CHECK bị cấm, và kể cả hàm `STABLE` cũng không an toàn vì CHECK không được re-evaluate khi bảng kia đổi). Do đó mọi ràng buộc liên bảng ở đây phải hiện thực bằng **(a)** cột snapshot denormalize + same-row CHECK, **(b)** trigger, hoặc **(c)** application/domain validation. `P1-2` phải chọn đúng cơ chế cho từng dòng trong bảng trên và ghi rõ trong migration.

**CẤM tuyệt đối trong migration:** `CHECK max_attempts = 2`, `CHECK attempt_number <= 2`, hoặc bất kỳ CHECK nào chứa literal `300`/`150`/`900`/`450`. Candidate `mock-lab-v1` (2 attempts; GH `[0,150]` trong 300s; 24/7 `[0,450]` trong 900s) là **fixture/config cho MOCK/LAB**, không phải schema. Xem `OD-V1-08` và delta business source ở `specs/_review/open-decisions-register.md`.

## 5. Lease / fencing (chống double-call)

| Cột trên `ivr_sim_channels` | Ý nghĩa |
| --- | --- |
| `lease_token` (uuid, null khi rảnh) | token của lease hiện tại |
| `lease_fencing_generation` (bigint, monotonic++) | fencing token; mọi dial/hangup gửi kèm, gateway/adapter từ chối generation cũ |
| `leased_by_worker_id` (string) | worker instance đang giữ |
| `lease_acquired_at` / `lease_expires_at` (datetime) | TTL lease |
| `quarantine_until` (datetime, null) | trạng thái `QUARANTINED` sau fail liên tiếp |

- Acquire: `UPDATE … SET lease_token=…, lease_fencing_generation=lease_fencing_generation+1 WHERE sim_channel_id=… AND (lease_token IS NULL OR lease_expires_at < now())` — chỉ thành công khi rảnh hoặc lease đã hết hạn.
- Reclaim: worker riêng quét `lease_expires_at < now()`, chuyển channel về `IDLE` **sau khi** reconcile với gateway; nếu không reconcile được → `QUARANTINED` + admin review, **không** tự coi là rảnh (fail-closed).
- Fencing: adapter gửi `lease_fencing_generation` theo mọi lệnh; lệnh mang generation nhỏ hơn giá trị hiện tại bị từ chối (chống orphan call sau crash).

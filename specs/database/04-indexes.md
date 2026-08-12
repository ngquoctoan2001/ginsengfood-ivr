# DB-04 — Indexes & Constraints

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p07` · Nguồn: `phase-8/12` §4-8,§12; DF-04, D-10, D-02.

## 1. Unique (idempotency / trace)
| Bảng | Unique | Mục đích |
| --- | --- | --- |
| `ivr_confirmation_tasks` | `task_id` | 1 task 1 record |
| `ivr_confirmation_tasks` | `idempotency_key` (scope intake) | chống duplicate task (DF-04) |
| `ivr_call_jobs` | `ivr_call_job_id` | |
| `ivr_call_attempts` | `ivr_call_attempt_id`; `(ivr_call_job_id, attempt_number)` | 1 attempt-number/job (D-10) |
| `ivr_call_results` | `ivr_call_result_id` | |
| `ivr_result_callbacks` | `callback_id`; `idempotency_key` (scope callback) | chống duplicate transition |
| `ivr_raw_call_event` | `raw_event_id` | |

## 2. Index scheduler (deadline-aware — phase-8/12 §5)
- `ivr_call_jobs (status, expires_at)` — query job sắp hết window (rolling queue).
- `ivr_call_jobs (t0_at)` / `ivr_call_attempts (scheduled_at)` / `(scheduled_window_expires_at)`.
- `ivr_call_jobs (program_type, status)`; `(official_order_id, status)`.
- `ivr_sim_channels (status, enabled, cooldown_until)` — chọn SIM rảnh.

## 3. Index race guard / lookup
- Current lookup: `order_state`, `program_type`, `official_order_id`, `task_id`, `correlation_id`; stale guard hiện chạy bằng Core recheck state/COD/sellable.
- Target race guard: required/indexed `order_version_snapshot` / `order_version_seen_by_ivr`; current-compat records are distinguishable and cannot weaken target validation.
- `correlation_id` trên tasks/results/technical_exceptions/admin_actions (trace).
- `official_order_id`, `task_id` trên các bảng con.
- `result_type`, `is_final_for_ivr`, `human_review_required` (query review).
- `core_http_status` (current), `core_response_code` (target), `next_retry_at` trên callbacks (retry scan).

## 4. Constraint P0 (application guard nếu DB không hỗ trợ CHECK phức)
- `max_attempts = 2` mọi program (D-10).
- `program_type=GOLDEN_HOUR → window=300 ∧ spacing=150`; `TWENTY_FOUR_SEVEN → window=900 ∧ spacing=450` (D-10).
- `attempt_number ≤ 2`; không tạo attempt vượt max.
- `is_counted_customer_attempt=false` khi `technical_exception_type` không null (technical ≠ no-answer, P0-IVR-004).
- `not_for_quote_cart_draft`, `no_direct_order_update`, `input_signal_only`, `no_payment_or_revenue_effect` = true (invariants).
- Không cột nào bắt buộc lưu **full phone** hay **raw recording** (D-05/DT-05).

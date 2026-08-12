# API-02 — Internal API

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/11` §4; `/04`, `/07`, `/12`; D-01..D-06, DO-02.
Base path `/v1/ivr/order-confirmation/*`. Tất cả internal, auth service-token (DF-06), correlation bắt buộc.

## 1. Danh sách endpoint
| Endpoint | Method | Producer → Consumer | Contract | Idempotency |
| --- | --- | --- | --- | --- |
| `/tasks` | POST | Order Core → IVR API | `IvrConfirmationTaskV1` | **Có** |
| `/eligibility-checks` | POST | IVR/Order Core → IVR API | `IvrEligibilityDecision` | Có |
| `/call-jobs` | POST | IVR Runtime → IVR API | `IvrCallJob` | Có |
| `/call-jobs/{ivrCallJobId}` | GET | Admin/Internal → IVR API | `IvrCallJob` | — |
| `/call-attempts` | POST | Scheduler/SIM Adapter → IVR API | `IvrCallAttempt` | Có |
| `/call-results` | POST | Result Normalizer → IVR API | `IvrCallResult` | Có |
| `/result-callbacks` | POST | IVR Runtime → IVR API (bản ghi) | `IvrResultCallback` | Có |

> Cuộc gọi callback **đi ra Order Core** là một outbound call riêng: `POST {orderCore}/v1/orders/{order_id}/ivr-result-callbacks` (D-04) — xem [05-order-core-contracts.md](05-order-core-contracts.md) & [08-external-api-needs.md](08-external-api-needs.md). Endpoint `/result-callbacks` ở đây chỉ để **ghi & theo dõi** vòng đời callback (khớp bảng `ivr_result_callbacks`).

## 2. `POST /tasks` — Task intake
- Mục tiêu: nhận `IvrConfirmationTaskV1` từ Order Core; validate; tạo/trả CallJob.
- Validation (fail → mã HTTP; xem [06](06-error-codes.md)):

| Check | Fail |
| --- | --- |
| Caller là Order Core (allowlist `X-Source-System`+token) | `403` |
| Có `Idempotency-Key` + `X-Correlation-Id` | `422` |
| Entity là **Official Order** (`order_id`, `order_code`) ở `order_state=CONFIRMING` + `payment_method_snapshot=COD`; `is_ivr_callable` nếu có chỉ là cờ derive — D-01/D-02/DS-01 | `422` (`TASK_REJECTED_NOT_OFFICIAL_ORDER`/`STATE_NOT_CALLABLE`) |
| `program_code ∈ {GOLDEN_HOUR, TWENTY_FOUR_SEVEN}`; `max_attempts=2`; schedule khớp D-10 | `409` (`POLICY_MISMATCH`) |
| Official contact hợp lệ (`phone_ref`/`dial_token`, `phone_validation_status=PASS`) — D-05 | `422` (`CONTACT_INVALID`) |
| Blocker: mảng `sellable_status[]` per-line không có `Decision∈{NOT_SELLABLE,BLOCKED}`/`RecallHold`/`SaleLock` (DO-02); do-not-call=false (✅ DC-01, fail-closed nếu không xác định) | `409`/`TASK_BLOCKED_OPERATIONAL` |
| Evidence/privacy policy version có | `422`/hold |
| Release flag chưa cho real call | Accept **dry-run** (`TASK_ACCEPTED_DRY_RUN_ONLY`), không dispatch SIM thật |

- Kết quả intake (taxonomy): `TASK_ACCEPTED_CALL_JOB_CREATED` · `TASK_ACCEPTED_DRY_RUN_ONLY` · `TASK_SKIPPED_TRUSTED_CUSTOMER` (D-12) · `TASK_REJECTED_NOT_OFFICIAL_ORDER` · `TASK_REJECTED_STATE_NOT_CALLABLE` · `TASK_REJECTED_POLICY_MISMATCH` · `TASK_REJECTED_CONTACT_INVALID` · `TASK_REJECTED_SCRIPT_NOT_APPROVED` · `TASK_REJECTED_INVALID_TRACE` · `TASK_BLOCKED_OPERATIONAL` · `TASK_HELD_ADMIN_REVIEW` · `TASK_HELD_POLICY_MISSING`.

## 3. `POST /eligibility-checks`
- Ghi quyết định eligibility (trust/contact/blocker/window/capacity). Input: order/program/contact refs + `sellable_status[]` snapshot. Output: `IvrEligibilityDecision{ decision, blocked_reasons[], skip_trusted?, evidence_ref }`.
- Trusted skip (D-12): chỉ khi `customer_trust_status=TRUSTED` + `trusted_skip_allowed=true` + contact ổn định + không blocker + không risk flag → `SKIP_TRUSTED_CUSTOMER`.

## 4. `POST /call-jobs` · `GET /call-jobs/{id}`
- Tạo/đọc CallJob. Trường chính (khớp `ivr_call_jobs`): `ivr_call_job_id`, `task_id`, `official_order_id`, `order_state`, `payment_method_snapshot=COD`, `order_version_snapshot` (target/nullable IR-SALES-OC1), `program_type`, `attempt_policy_code`, `status`, `max_attempts=2`, `attempt_spacing_seconds` (GH 150s / 24-7 450s — D-10), `confirmation_window_seconds` (GH 300 / 24-7 900 — D-10), `attempt_schedule_json`, `queue_status`, `input_signal_only=true`, `no_direct_order_update=true`.
- GET dùng cho admin/monitor; response **masked** (không raw phone).

## 5. `POST /call-attempts`
- Ghi từng attempt (khớp `ivr_call_attempts`): `attempt_number` (1..2), `scheduled_at/window_expires_at`, `status`, `result_status`, `dtmf_key`, `is_counted_customer_attempt`, `technical_retry_*`, `sim_channel_id`, `disposition` (DT-02). 
- Constraint: `attempt_number ≤ 2` (D-10); `is_counted_customer_attempt=false` khi `technical_exception_type` không null (DT-02).

## 6. `POST /call-results`
- Ghi result normalized (khớp `ivr_call_results`): `final_result_status`, `result_type`, `is_counted_customer_attempt`, `is_final_for_ivr`, `recommended_core_action` (advisory), `input_signal_only=true`, `no_direct_order_update=true`, `no_payment_or_revenue_effect=true`. Không để raw provider event vào Core (chuẩn hóa trước).

## 7. `POST /result-callbacks`
- Ghi vòng đời callback (khớp `ivr_result_callbacks`): `callback_id`, `ivr_call_result_id`, `task_id`, `official_order_id`, `idempotency_key`, `result_status`, `result_state` (state machine phase-8/07 §7), `requires_core_revalidation=true`, `core_http_status` (current), `core_response_code` (target), `retry_count`, `next_retry_at`.
- Sau khi ghi READY → thực hiện outbound call tới Order Core (D-04). Current response của Core: HTTP `200`/`422`; semantic `CALLBACK_*` response là target IR-SALES-OC2.

## Báo cáo (internal)
- **7 endpoint internal** (6 POST + 1 GET). Không endpoint nào update order state (D-02). Callback thật đi ra Order Core (D-04) — mô tả ở `05/08`.

# DB-03 — Enums and Status

Trạng thái: `TARGET_V1_DRAFT` · Đối soát code: `2026-09-04` (`W-0171`).

> **Nguồn sự thật là DB CHECK constraint, không phải file này.** Mọi danh sách dưới đây được
> trích từ `src/Ivr.Infrastructure/Persistence/Migrations/IvrDbContextModelSnapshot.cs` và
> đóng lại bởi `W-0115` (`20260824021636_W0115ClosedEnumChecks`). Cột "Constraint" cho biết
> chỗ kiểm tra lại. Ghi một giá trị không có trong danh sách sẽ bị PostgreSQL từ chối, nên
> khi file này lệch code thì **code đúng** — sửa file này, đừng sửa constraint.

## 1. Program và mode

| Trường | Giá trị | Constraint |
| --- | --- | --- |
| `program_type` | `GOLDEN_HOUR`, `TWENTY_FOUR_SEVEN` (`24_7` chỉ là compatibility input) | `ck_ivr_confirmation_tasks_matrix` |
| `payment_method_snapshot` | `ONLINE`, `COD` | `ck_ivr_confirmation_tasks_matrix` |
| `execution_mode` | `MOCK`, `LAB_REAL_SIM`, `PRODUCTION_REAL` | `ck_ivr_sim_channels_mode` |

Ma trận bắt buộc: `GOLDEN_HOUR`+`ONLINE` **hoặc** `TWENTY_FOUR_SEVEN`+`COD`. Không có tổ hợp thứ ba.

## 2. `ivr_call_jobs`

### 2.1 `status` — 30 giá trị (`ck_ivr_call_jobs_status`)

| Nhóm | Giá trị |
| --- | --- |
| Khởi tạo | `CREATED`, `DRY_RUN`, `OPEN` |
| Chờ lập lịch | `QUEUED`, `READY_FOR_SCHEDULER` |
| Lease/dispatch | `LEASED`, `LEASED_PENDING_DISPATCH`, `DISPATCH_LEASED` |
| Đang gọi | `DIALING`, `ACTIVE_CALL` |
| Chờ chuẩn hóa | `DISPOSITION_PENDING_NORMALIZATION`, `PROVIDER_EVENT_PENDING_NORMALIZATION` |
| Chờ callback | `RESULT_READY_FOR_CALLBACK` |
| Retry kỹ thuật | `TECHNICAL_RETRY_QUEUED` |
| Hold | `HELD_MOCK`, `HELD_ADMIN_REVIEW`, `HELD_ELIGIBILITY`, `HELD_CAPACITY`, `HELD_CALLBACK`, `HELD_TECHNICAL_REVIEW`, `HELD_NORMALIZATION`, `HELD_LEASE_RECOVERY` |
| Capacity | `CAPACITY_HELD`, `CAPACITY_MISSED`, `CLOSED_CAPACITY` |
| Kết thúc | `WINDOW_EXPIRED`, `RECOVERY_REQUIRED`, `BLOCKED`, `SKIPPED`, `CLOSED` |

### 2.2 `queue_status` — 14 giá trị (`ck_ivr_call_jobs_queue_status`)

`QUEUED`, `HELD_MOCK`, `HELD_ELIGIBILITY`, `LEASED`, `HELD_LEASE_RECOVERY`, `HELD_NORMALIZATION`,
`HELD_CALLBACK`, `HELD_TECHNICAL_REVIEW`, `HELD_CAPACITY`, `HELD_ADMIN_REVIEW`, `SKIPPED`,
`BLOCKED`, `CLOSED_CAPACITY`, `CLOSED_WINDOW_EXPIRED`.

Đây là trục riêng cho hàng đợi vận hành, **không** phải tập con của `status`: `CLOSED_WINDOW_EXPIRED`
chỉ tồn tại ở đây, còn `status` dùng `WINDOW_EXPIRED`.

### 2.3 `eligibility_decision` — 6 giá trị (`ck_ivr_call_jobs_eligibility_decision`)

`PENDING_ELIGIBILITY`, `ELIGIBLE_FOR_IVR`, `TASK_BLOCKED_OPERATIONAL`, `TASK_HELD_ADMIN_REVIEW`,
`TASK_SKIPPED_TRUSTED_CUSTOMER`, `IVR_CAPACITY_EXCEPTION`.

Cùng danh sách được `ck_ivr_confirmation_tasks_eligibility_decision` áp cho `ivr_confirmation_tasks`
(ở đó cho phép `NULL`). `TASK_SKIPPED_TRUSTED_CUSTOMER` là **tên đã persist**, giữ lại sau `OD-18`;
runtime không còn tự quyết định trusted-skip — xem `specs/workflows/07-trusted-skip.md`.

## 3. `ivr_call_attempts`

### 3.1 `status` — 10 giá trị (`ck_ivr_call_attempts_status`)

`LEASED_PENDING_DISPATCH`, `DIALING`, `ACTIVE_CALL`, `PROVIDER_EVENT_PENDING_NORMALIZATION`,
`NORMALIZED_ATTEMPT_COMPLETE`, `NORMALIZED_FINAL`, `NORMALIZED_TECHNICAL_RETRY`,
`NORMALIZED_REVIEW_REQUIRED`, `TECHNICAL_RETRY_QUEUED`, `RECOVERY_REQUIRED`.

> Đây là vòng đời **lease → dispatch → chuẩn hóa**, không phải vòng đời tín hiệu telephony.
> Các trạng thái `RINGING`/`ANSWERED`/`CAPTURING_DTMF` thuộc về gateway và không được persist
> thành `status`; kết quả cuộc gọi đi vào `result_status` (§3.2) sau khi normalize.

### 3.2 `result_status` — 11 giá trị (`ck_ivr_call_attempts_result_status`)

Cùng tập với result type ở §4.

### 3.3 `voice_region` — 3 giá trị (`ck_ivr_call_attempts_voice_region`)

`North`, `Central`, `South`. **Chú ý:** PascalCase, không phải SCREAMING_SNAKE như mọi enum khác.

## 4. Result type — 11 giá trị

`IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_ATTEMPT`, `IVR_NO_ANSWER_FINAL`,
`IVR_CONFIRMATION_WINDOW_EXPIRED`, `IVR_INVALID_PHONE_FINAL`, `IVR_WRONG_INPUT`,
`IVR_TECHNICAL_EXCEPTION`, `IVR_CAPACITY_EXCEPTION`, `IVR_OPERATIONAL_BLOCKED`, `IVR_POLICY_BLOCKED`.

Bốn nơi phải khớp nhau và đang khớp: enum `IvrResultType` trong
`src/Ivr.Domain/Confirmation/CallResult.cs`, `ck_ivr_call_results_result_type`,
`ck_ivr_result_callbacks_result_status`, và OpenAPI `specs/api/openapi/ivr-order-confirmation.v1.yaml`.

## 5. Callback

| Trường | Giá trị | Constraint |
| --- | --- | --- |
| `delivery_status` (11) | `READY`, `SENDING`, `RETRY_PENDING`, `RETRY_EXHAUSTED`, `DELIVERED_ACCEPTED`, `DELIVERED_BLOCKED`, `DELIVERED_REVIEW`, `REJECTED_STALE`, `IDEMPOTENCY_CONFLICT`, `INVALID_DEAD_LETTER`, `AUTH_REJECTED` | `ck_ivr_result_callbacks_delivery_status` |
| Sales ACK (6) | `ACCEPTED`, `DUPLICATE_ACCEPTED`, `BLOCKED_BY_CORE`, `REVIEW_REQUIRED`, `REJECTED_STALE`, `IDEMPOTENCY_CONFLICT` | enum `CallbackAcknowledgementCode`; OpenAPI `order-core-ivr-callback.target-v1.yaml` |
| `recommended_core_action` (7) | `REVALIDATE_AND_CONFIRM_ORDER`, `REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST`, `NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`, `REVALIDATE_AND_EXPIRE_CONFIRMATION`, `REVALIDATE_AND_HOLD_ADMIN_REVIEW`, `IGNORE_STALE_CALLBACK`, `BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT` | `ck_ivr_call_results_recommended_core_action` |

`AUTH_REJECTED` là trạng thái delivery riêng: callback bị Sales từ chối vì credential, không phải
vì nội dung — nó không được retry như `RETRY_PENDING`.

## 6. SIM channel — `status` 8 giá trị (`ck_ivr_sim_channels_status`)

`IDLE`, `RESERVED`, `LEASED`, `DIALING`, `ACTIVE_CALL`, `DISABLED`, `QUARANTINED`, `HEALTH_FAILED`.

Lease/fencing của channel xem `specs/database/04-indexes.md` §5.

## 7. Các enum còn lại

| Bảng / trường | Giá trị | Constraint |
| --- | --- | --- |
| `ivr_review_items.status` | `OPEN`, `RESOLVED`, `PENDING_CRM`, `ACCEPTED_BY_CRM` | `ck_ivr_review_items_status` |
| `ivr_review_items.source_type` | `IVR_CALL_RESULT`, `IVR_RESULT_CALLBACK`, `ELIGIBILITY_DECISION`, `IVR_OPTOUT_PROPOSAL` | `ck_ivr_review_items_source_type` |
| `ivr_capacity_incidents.scope` | `ADMIN_QUEUE_PAUSE`, `ELIGIBILITY_DEADLINE`, `SCHEDULER_DEADLINE` | `ck_ivr_capacity_incidents_scope` |
| `ivr_capacity_incidents.status` | `OPEN`, `RESOLVED` | `ck_ivr_capacity_incidents_status` |
| `ivr_script_versions.status` | `DRAFT`, `IN_REVIEW`, `APPROVED`, `RETIRED` | `ck_ivr_script_versions_status` |
| `ivr_script_approvals.approval_type` | `MOCK_TEST`, `LAB`, `CONTENT`, `PRIVACY_LEGAL` | `ck_ivr_script_approvals_type` |
| `ivr_task_intake_outbox.status` | `HELD_MOCK`, `READY_FOR_ELIGIBILITY`, `PUBLISHED` | `ck_ivr_task_intake_outbox_status` |

## 8. Cố ý KHÔNG phải enum

- **`exception_type` / `technical_exception_type`** là `string` tự do ở cả DB lẫn OpenAPI. Xem
  `specs/functional/06-technical-exception-capacity.md` §Taxonomy — taxonomy 7 giá trị trong tài
  liệu Owner **chưa được thực thi**; runtime ghi mã gốc của gateway (ví dụ `ASTERISK_DIAL_TIMEOUT`).
- **Attempt policy** timing/max là data/config gắn với `attempt_policy_version`, không phải enum.
  Ràng buộc duy nhất là `ck_ivr_confirmation_tasks_attempt_bounds`: `max_attempts BETWEEN 1 AND 10`.

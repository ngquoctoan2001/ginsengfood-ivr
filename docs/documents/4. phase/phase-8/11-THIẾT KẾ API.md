# IVR-11 - API Design

Trạng thái: `SDS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Vai trò: Thiết kế API triển khai cho Phase 8 dựa trên contract `openapi/business-platform/ivr-order-confirmation.v1.yaml`.

## 1. Mục tiêu

Tài liệu này mô tả API nội bộ/admin cần triển khai cho IVR. API Phase 8 không phải public consumer API. Tất cả endpoint phải nằm dưới `/v1/ivr/order-confirmation/*`, dùng auth nội bộ hoặc admin RBAC, có idempotency cho lệnh rủi ro, và ghi audit/evidence.

## 2. Nguyên tắc API

| Nguyên tắc | Yêu cầu |
| --- | --- |
| Version | `v1`, không phá vỡ contract hiện có nếu chưa có compatibility note. |
| Boundary | Internal/admin only, không public consumer API. |
| Envelope | Response dùng envelope chuẩn nếu implementation đã có common envelope. |
| Idempotency | Bắt buộc cho task intake, callback, admin action, technical retry. |
| Correlation | Bắt buộc qua `correlation_id` hoặc header tương đương. |
| Auth | Service-to-service hoặc admin RBAC, không anonymous. |
| Evidence | Lệnh rủi ro phải có evidence/audit refs hoặc tạo audit record. |
| Fail-safe | Dependency owner unavailable thì không dispatch call thật. |

## 3. Header chuẩn

| Header | Required | Áp dụng | Ghi chú |
| --- | --- | --- | --- |
| `Authorization` | Có | Tất cả | Service token hoặc admin session. |
| `X-Correlation-Id` | Có | Tất cả | Trace xuyên hệ thống. |
| `Idempotency-Key` | Có với POST rủi ro | Task, callback, admin, retry | Phải map vào idempotency store. |
| `X-Actor-Id` | Có với admin | Admin actions | Không tin client-only; backend xác thực lại. |
| `X-Source-System` | Có với internal | Order Core, IVR Worker | Chỉ allowlisted systems. |

## 4. Endpoint nhóm internal

| Endpoint | Method | Producer | Consumer | Contract | Chức năng |
| --- | --- | --- | --- | --- | --- |
| `/v1/ivr/order-confirmation/tasks` | POST | Order Core | IVR API | `IvrConfirmationTaskV1` | Nhận task xác nhận đơn. |
| `/v1/ivr/order-confirmation/eligibility-checks` | POST | IVR/Order Core | IVR API | `IvrEligibilityDecision` | Ghi quyết định eligibility/trusted/phone/block. |
| `/v1/ivr/order-confirmation/call-jobs` | POST | IVR Runtime | IVR API | `IvrCallJob` | Tạo hoặc ghi CallJob. |
| `/v1/ivr/order-confirmation/call-jobs/{ivrCallJobId}` | GET | Admin/Internal | IVR API | `IvrCallJob` | Đọc trạng thái CallJob. |
| `/v1/ivr/order-confirmation/call-attempts` | POST | Scheduler/SIM Adapter | IVR API | `IvrCallAttempt` | Ghi Attempt. |
| `/v1/ivr/order-confirmation/call-results` | POST | Result Normalizer | IVR API | `IvrCallResult` | Ghi result normalized. |
| `/v1/ivr/order-confirmation/result-callbacks` | POST | IVR Runtime | Order Core boundary | `IvrResultCallback` | Gửi signal về Order Core. |

## 5. Endpoint nhóm admin

| Endpoint | Method | Permission | Contract | Chức năng |
| --- | --- | --- | --- | --- |
| `/v1/ivr/order-confirmation/queue` | GET | `IVR_QUEUE_VIEW` | Queue status projection | Xem queue/capacity. |
| `/v1/ivr/order-confirmation/queue:pause` | POST | `IVR_QUEUE_PAUSE` | `IvrAdminAction` | Pause queue có reason/evidence. |
| `/v1/ivr/order-confirmation/queue:resume` | POST | `IVR_QUEUE_RESUME` | `IvrAdminAction` | Resume queue sau review. |
| `/v1/ivr/order-confirmation/sim-channels/{simChannelId}:disable` | POST | `IVR_SIM_DISABLE` | `IvrAdminAction` | Disable SIM channel. |
| `/v1/ivr/order-confirmation/sim-channels/{simChannelId}:enable` | POST | `IVR_SIM_ENABLE` | `IvrAdminAction` | Enable SIM channel sau health check. |
| `/v1/ivr/order-confirmation/technical-retries` | POST | `IVR_MANUAL_RETRY` | `IvrTechnicalException` | Request technical retry. |
| `/v1/ivr/order-confirmation/admin-reviews` | POST | `IVR_RESULT_REVIEW` | `IvrAdminAction` | Ghi review/annotation. |

## 6. Task intake API

Endpoint: `POST /v1/ivr/order-confirmation/tasks`

Mục tiêu:

- Nhận task từ Order Core.
- Validate idempotency/correlation/official order/program/contact/policy.
- Tạo hoặc trả lại CallJob tương ứng.

Validation bắt buộc:

| Check | Reject/Hold khi fail |
| --- | --- |
| Caller là Order Core hoặc service được ủy quyền | `403 Forbidden` |
| Có idempotency key | `422 UnprocessableEntity` |
| Entity là Official Order | `422 UnprocessableEntity` |
| Program là `GOLDEN_HOUR` hoặc `TWENTY_FOUR_SEVEN` | `422 UnprocessableEntity` |
| `max_attempts` khớp program | `409 Conflict` |
| Official contact/phone projection hợp lệ | `422 UnprocessableEntity` |
| Sale Lock/Recall/Suppression/opt-out active | `409 Conflict` hoặc task blocked |
| Release flag chưa cho phép real call | Task accepted ở dry-run/internal mode, không dispatch real SIM |

## 7. Result callback API

Endpoint: `POST /v1/ivr/order-confirmation/result-callbacks`

Mục tiêu:

- Gửi result normalized từ IVR về Order Core.
- Order Core revalidate trước khi quyết định order transition.

Order Core phải kiểm:

- Idempotency key chưa conflict.
- `task_id`, `order_id`, `order_version_seen_by_ivr`.
- Order hiện còn nhận IVR result không.
- Sale Lock/Recall/Suppression/opt-out/payment issue hiện tại.
- Evidence/audit refs tồn tại.
- Result type có hợp với attempt policy không.

Callback response semantic:

| Response result | Ý nghĩa |
| --- | --- |
| `CALLBACK_ACCEPTED_FOR_REVALIDATION` | Order Core nhận signal và bắt đầu/đã xử lý revalidation. |
| `CALLBACK_REJECTED_STALE` | Task/order version stale, không transition. |
| `CALLBACK_BLOCKED_BY_CORE` | Revalidation thấy blocker. |
| `CALLBACK_NEEDS_ADMIN_REVIEW` | Owner/admin review required. |

## 8. Admin action API

Mọi admin POST phải có:

- Authenticated actor.
- Permission server-side.
- `reason`.
- `target_type`, `target_id`.
- Audit record.
- Evidence ref nếu action ảnh hưởng queue/SIM/retry/result.
- `no_policy_bypass = true`.

Admin không được:

- Gọi khách ngoài attempt policy.
- Reset customer attempt count.
- Force confirm/cancel order.
- Enable SIM khi health check đang fail.
- Resume queue khi capacity incident chưa được xử lý.

## 9. Error mapping

| HTTP | Dùng khi |
| --- | --- |
| `400` | Request syntax hoặc field format sai. |
| `401` | Thiếu auth. |
| `403` | Caller không có quyền hoặc không thuộc allowlist. |
| `404` | Resource không tồn tại. |
| `409` | Conflict idempotency, order version, policy, state, capacity. |
| `422` | Request hợp lệ cú pháp nhưng vi phạm business contract. |
| `429` | Queue/admin API rate limit nếu implementation hỗ trợ. |
| `500` | Lỗi hệ thống ngoài dự kiến; không dùng để che business reject. |

## 10. API release gates

API chỉ được coi là sẵn sàng khi:

- OpenAPI `3.1.0` parse hygiene pass.
- Contract validator pass.
- Unit/integration tests pass.
- Auth/RBAC enforced server-side.
- Idempotency store hoạt động cho POST rủi ro.
- Admin action ghi audit/evidence.
- Real customer call vẫn bị feature flag chặn.

## 11. Acceptance criteria

- API design bao phủ task, eligibility, call job, attempt, result, callback, queue, SIM, technical retry, admin review.
- Không endpoint nào cho phép IVR update order state trực tiếp.
- Có rule auth/idempotency/correlation/error rõ.
- Có mapping permission cho admin.
- Có response semantic để Order Core xử lý race condition.

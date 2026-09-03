# FR — Result Normalization and Sales Callback

Trạng thái: `TARGET_V1_DRAFT`.

## Canonical results

Contract dùng chung giữ **11 mã**. Runtime IVR hiện có producer path cho **9 mã**:

- final và có callback: `IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_FINAL`,
  `IVR_CONFIRMATION_WINDOW_EXPIRED`, `IVR_INVALID_PHONE_FINAL`, `IVR_CAPACITY_EXCEPTION`;
- non-final, được persist nhưng không vào callback outbox: `IVR_NO_ANSWER_ATTEMPT`,
  `IVR_WRONG_INPUT`, `IVR_TECHNICAL_EXCEPTION`.

Hai mã còn lại được giữ để tương thích nhưng **không phải call result do IVR phát**:

- `IVR_OPERATIONAL_BLOCKED` và `IVR_POLICY_BLOCKED` là quyết định trước cuộc gọi; không tạo
  call result và không callback.
- Nếu Sales revalidate sau cuộc gọi rồi chặn vì Sale Lock/Recall, Sales trả ACK
  `BLOCKED_BY_CORE`; kết quả quan sát (`IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, ...) không bị
  viết lại.

`IVR_CONFIRMATION_WINDOW_EXPIRED` do scheduler IVR tạo khi cửa sổ hết trước final result. Sweep
không tính thêm customer attempt. Nếu đã có counted attempt, advisory là revalidate rồi expire;
nếu chưa từng có counted attempt, advisory là revalidate rồi hold admin review. Sales/Order Core
vẫn là bên duy nhất được đổi order state.
Technical/capacity/window-sweep exceptions are not customer attempts. IVR never transitions the
order. Quyết định chi tiết và nguồn KPI nằm ở
[DT-06](../decisions/DT-06-blocked-result-semantics.md); gói ký hiện hành nằm ở
[M8-05/W-0145](../../plan/ivr-orther/m8-05-program-result-contract-signoff-2026-09-03.md).

## Target callback

`POST {sales}/api/v1/internal/orders/{orderId}/ivr-result-callbacks` with auth, `Idempotency-Key`, `X-Correlation-Id` and body fields defined in `specs/api/05-order-core-contracts.md`.

| HTTP | Code | Terminal/retry behavior |
| --- | --- | --- |
| 200 | `ACCEPTED`, `DUPLICATE_ACCEPTED`, `BLOCKED_BY_CORE`, `REVIEW_REQUIRED` | delivery terminal; record semantic outcome |
| 409 | `REJECTED_STALE`, `IDEMPOTENCY_CONFLICT` | no automatic transport retry; review by policy |
| 422 | invalid schema/outcome | dead-letter/review |
| 429/5xx/timeout | retryable transport | bounded retry, same key/payload |

Current Golden Hour endpoint is an isolated compatibility adapter, not Target V1.

## No-answer

`IVR_NO_ANSWER_FINAL` recommends `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`; Sales timeout worker may expire only after revalidation. IVR does not cancel and does not send notification.

## Requirements

| ID | Yêu cầu |
| --- | --- |
| `FR-IVR-RES-001` | Normalize raw provider events into canonical result + evidence |
| `FR-IVR-RES-002` | Target payload includes callback/task/order/version/result/attempt/time/action/evidence/audit |
| `FR-IVR-RES-003` | Persist outbox before delivery; replay same key and immutable payload |
| `FR-IVR-RES-004` | Map ACK by HTTP+semantic code; do not retry terminal business outcomes |
| `FR-IVR-RES-005` | Version/state/blocker race belongs to Sales revalidation; IVR displays ACK truth |
| `FR-IVR-RES-006` | Auth/downstream outage fail safely; no duplicate result/attempt |
| `FR-IVR-RES-007` | V1 notification path is disabled/no-op and tested |
| `FR-IVR-RES-008` | Chỉ final result vào callback outbox; hai blocked code phải bị outbound mapper từ chối |

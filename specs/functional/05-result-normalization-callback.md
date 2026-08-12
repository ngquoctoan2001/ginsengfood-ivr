# FR — Result Normalization and Sales Callback

Trạng thái: `TARGET_V1_DRAFT`.

## Canonical results

`IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_ATTEMPT`, `IVR_NO_ANSWER_FINAL`, `IVR_CONFIRMATION_WINDOW_EXPIRED`, `IVR_INVALID_PHONE_FINAL`, `IVR_WRONG_INPUT`, `IVR_TECHNICAL_EXCEPTION`, `IVR_CAPACITY_EXCEPTION`, `IVR_OPERATIONAL_BLOCKED`, `IVR_POLICY_BLOCKED`.

Technical/capacity/policy exceptions are not customer attempts. IVR never transitions the order.

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

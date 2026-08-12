# API-05 — Sales Platform Contracts

Trạng thái: `TARGET_V1_DRAFT` · Owner: Sales Platform/Order Core.

## 1. Sales → IVR task

Endpoint IVR-owned: `POST {ivr}/v1/ivr/order-confirmation/tasks`.

Required headers: service bearer token, `Idempotency-Key`, `X-Correlation-Id`.

Required body groups:

- identity/version: `contract_version`, `task_id`, `order_id`, `order_code`, `order_version`;
- eligibility: `order_state`, `program_code`, `payment_method_snapshot`, `ivr_confirmation_required=true`, `call_restriction`, `eligibility_snapshot`, `evidence_ref`;
- time/policy: start/expiry, `attempt_policy_version`, max customer attempts, offsets;
- dialing: `phone_ref`, `phone_masked`, `dial_token`, expiry;
- speech: `privacy_safe_order_summary` with short name/code, public item name+qty, total VND, short area, program, locale;
- script/policy versions.

Program invariant:

| `program_code` | `payment_method_snapshot` |
| --- | --- |
| `GOLDEN_HOUR` | `ONLINE` |
| `TWENTY_FOUR_SEVEN` | `COD` |

Other combinations reject. `24_7` is a current-compat input alias only.

## 2. IVR → Sales callback Target V1

Endpoint Sales-owned: `POST {sales}/api/v1/internal/orders/{orderId}/ivr-result-callbacks`.

Body: `contract_version`, callback/task/order IDs, `order_version_seen_by_ivr`, result, counted/final flags, attempt number, occurred time, advisory action, evidence and audit refs.

The path `orderId` must equal body `order_id`. Sales revalidates idempotency, version/state/program/payment, blockers and evidence before any transition.

## 3. ACK mapping

| HTTP | `code` | IVR action |
| --- | --- | --- |
| 200 | `ACCEPTED`, `DUPLICATE_ACCEPTED` | mark delivered |
| 200 | `BLOCKED_BY_CORE`, `REVIEW_REQUIRED` | mark delivered + expose semantic outcome |
| 409 | `REJECTED_STALE`, `IDEMPOTENCY_CONFLICT` | no transport retry; audit/review |
| 422 | invalid schema/outcome | DLQ/review |
| 429/5xx/timeout | retryable | bounded retry with identical key/body |

ACK `ACCEPTED` means Sales accepted the signal for its decision path, not necessarily that order is confirmed.

## 4. No-answer

For `IVR_NO_ANSWER_FINAL`, use `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`. IVR never cancels the order. Sales timeout worker decides expiry after revalidation.

## 5. Current compatibility

Current source exposes `POST /api/v1/internal/ivr/golden-hour/callbacks`. It is supported only by an isolated feature-flagged adapter and current-specific tests. It does not prove generic Target V1 support, must not silently receive 24/7 results, and must have a sunset/disable path.

Machine-readable proposal: `specs/api/openapi/order-core-ivr-callback.target-v1.yaml`.

# IVR Integration Guide — Order Core, Ops and CRM

Status: `TARGET_CONTRACT_V1=DRAFT` · Environment: non-production only · Real customer calls: `NO`.

## Read this first

There are three separate contract boundaries:

| Boundary | Machine source | Runtime status |
| --- | --- | --- |
| Sales/Order Core → IVR task and IVR-owned admin/internal API | `specs/api/openapi/ivr-order-confirmation.v1.yaml` | Target draft; MOCK fixtures only |
| IVR → Sales result callback | `specs/api/openapi/order-core-ivr-callback.target-v1.yaml` | Target draft; Sales approval/base URL/auth pending |
| IVR → current Golden Hour callback | pinned JSON Schema at Sales commit `a3aad246…` | compatibility-only; no approved IVR runtime mode |

Do not send 24/7 COD outcomes to the current Golden Hour endpoint and do not
interpret a rendered Target page as proof that Sales has implemented it.

## Target task intake

Proposed endpoint:

```text
POST {ivr-base}/v1/ivr/order-confirmation/tasks
```

Required transport metadata is defined by OpenAPI and includes service bearer
identity, `Idempotency-Key`, `X-Correlation-Id` and the Order Core source
profile. Use a new idempotency key for a new semantic command; replay the exact
same key and body only when retrying the same command.

The body includes immutable order/version/policy snapshots, an opaque dialing
reference and a privacy-safe speech summary. Never paste a real phone, full
street address, credential or real dialing token into a request example,
ticket, log or evidence file. Safe documentation uses values such as
`09****1234`, `Quận 7`, `order_demo_01` and `opaque-nonprod-token`.

Supported program/payment rows are exactly:

| Program | Payment |
| --- | --- |
| `GOLDEN_HOUR` | `ONLINE` |
| `TWENTY_FOUR_SEVEN` | `COD` |

Every other combination is rejected. The authoritative fields, formats,
required list and response schemas are in the generated IVR API reference.

## Target result callback

Proposed endpoint owned by Sales:

```text
POST {sales-base}/api/v1/internal/orders/{orderId}/ivr-result-callbacks
```

`orderId` in the path must equal `order_id` in the body. IVR submits a signal;
Sales revalidates version, state, program/payment, blockers and evidence before
performing any transition.

Semantic success codes are `ACCEPTED`, `DUPLICATE_ACCEPTED`,
`BLOCKED_BY_CORE` and `REVIEW_REQUIRED`. HTTP 200 therefore means delivery was
processed, not necessarily that the order became confirmed. HTTP 409 is a
stale/idempotency decision and must not be blindly transport-retried; 429,
5xx and timeout use bounded retry with the identical key and body.

For `IVR_NO_ANSWER_FINAL`, send
`CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`. IVR does not cancel or expire the
order; the Sales timeout worker owns that decision after revalidation.

## Current Golden Hour compatibility

Verified current endpoint:

```text
POST /api/v1/internal/ivr/golden-hour/callbacks
```

It uses transitional `X-Internal-Token`, a current-specific DTO and the result
values `CONFIRMED`, `REJECTED`, `NO_ANSWER`, `FAILED`. It has no semantic Target
ACK, order-version field, attempt metadata or evidence/audit references. It is
isolated behind its own adapter and must have a reviewed sunset/disable path.

## Errors and traceability

- IVR errors use `{error:{code,message,details,correlationId}}`.
- `code` is the stable enum rendered from OpenAPI; do not parse human messages.
- Propagate `X-Correlation-Id` end to end and record it in safe evidence.
- Never log raw phone, full address, token, authorization header or secret.
- Authentication, allowlist, idempotency and policy failures are fail-closed.

## Team handoff checklist

### Order Core / Sales

- approve exact Target OpenAPI and SemVer;
- supply sandbox/base URL and service-auth profile;
- implement/review task producer, callback consumer and semantic ACKs;
- prove idempotency, stale-version handling, blocker revalidation and no-answer timeout behavior.

### Ops

- revalidate inventory, recall and sale-lock truth for Order Core at callback time (D-06);
- return stable fail-closed codes; no direct order transition is owned by IVR.

### CRM

- own customer communication/template channels; IVR does not send SMS or notification;
- provide only privacy-safe speech inputs approved for the call script.

### IVR

- keep MOCK and real-customer permission fail-closed;
- accept only the exact task contract and publish only normalized result signals;
- retain audit/evidence references without raw PII;
- enable current or Target provider only through reviewed configuration and release gates.

## Local developer commands

```powershell
npm --prefix deploy/ci ci
npm --prefix deploy/ci run openapi:lint
npm --prefix deploy/ci run openapi:validate
npm --prefix deploy/ci run openapi:drift
npm --prefix deploy/ci run docs:build
npm --prefix deploy/ci run test:docs
```

The final hosted URL is intentionally absent until GitLab Pages Access Control
and the non-prod publication gate have evidence.

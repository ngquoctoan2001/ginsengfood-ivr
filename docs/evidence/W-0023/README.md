# W-0023 / P2-6 — Sales callback outbox and Golden Hour compatibility

Date: 2026-08-14

Implementation baseline: `68d5fef6ffa39f8a52409adf281876e0f0d1734f`

Implementation commit: `2412cf65615c1079e2c96f5f9ab5fc5b6eeb149c`

Execution boundary: local/MOCK, `REAL_CUSTOMER_CALL_ALLOWED=NO`

## Delivered behavior

P2-6 turns each final IVR result into one immutable callback snapshot and delivers
that snapshot through an explicitly selected Sales provider. The Target V1 path is
the canonical domain contract. The existing Golden Hour endpoint is isolated behind
a compatibility adapter and cannot become the default provider.

The result normalizer now stores the following records in one PostgreSQL transaction:

- the final `ivr_call_results` row;
- the immutable `ivr_callback_outbox` body, payload hash, stable idempotency key and
  correlation ID;
- the existing evidence, audit and job-state changes from P2-5.

The scheduler's final `IVR_CAPACITY_EXCEPTION` path creates the same callback snapshot
in its existing atomic transaction. Non-final no-answer, wrong-input and technical
results never create a callback. A redelivery always uses the stored body,
`Idempotency-Key` and `X-Correlation-Id`; no live task/order data is re-read to rebuild
the request.

## Target V1 delivery contract

The Target transport sends:

```text
POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks
Authorization: Bearer <service token>
Idempotency-Key: ivr-result:<resultId>
X-Correlation-Id: <task correlation id>
```

Before egress it verifies the stored SHA-256 hash, callback/task/order identity,
path/body order match, finality, and the no-answer action. The dispatcher classifies
responses without mutating an order:

| Response | Delivery state | Retry | Review |
| --- | --- | ---: | ---: |
| `200 ACCEPTED` / `DUPLICATE_ACCEPTED` | `DELIVERED_ACCEPTED` | no | no |
| `200 BLOCKED_BY_CORE` | `DELIVERED_BLOCKED` | no | yes |
| `200 REVIEW_REQUIRED` | `DELIVERED_REVIEW` | no | yes |
| `409 REJECTED_STALE` | `REJECTED_STALE` | no | yes |
| `409 IDEMPOTENCY_CONFLICT` | `IDEMPOTENCY_CONFLICT` | no | yes |
| `422` or invalid immutable payload | `INVALID_DEAD_LETTER` | no | yes |
| `401` / `403` | `AUTH_REJECTED` | no | yes |
| `429`, `5xx`, timeout or transport failure | `RETRY_PENDING` then `RETRY_EXHAUSTED` | bounded | on exhaustion |

`IVR_NO_ANSWER_FINAL` is accepted only with
`CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`. Neither that result nor any callback ACK
causes IVR to cancel/confirm an order or send a notification. Sales/Core remains the
owner of business revalidation and order-state transitions.

## Current Golden Hour compatibility boundary

The compatibility adapter is independently configured and fail-closed:

- provider must be `CURRENT_GOLDEN_HOUR_COMPAT`;
- `CurrentGoldenHourCompatibilityEnabled=true` is required in addition to the main
  callback enable flag;
- only `GOLDEN_HOUR` tasks are accepted; 24/7 tasks are rejected locally;
- it calls the verified current path
  `/api/v1/internal/ivr/golden-hour/callbacks` with `X-Internal-Token`;
- numeric current-system `callId`, `reservationId`, `orderId` and `customerId` must
  come from an explicit compatibility identity map;
- only the current DTO/result vocabulary is emitted; Target-only fields never leak
  into that request.

A current endpoint HTTP 200 means only that delivery was accepted. It is not proof
that an order was confirmed, cancelled or otherwise transitioned.

## Runtime controls and observability

- `Ivr:Callbacks:Enabled=false` by default;
- `CurrentGoldenHourCompatibilityEnabled=false` by default;
- real `TARGET_V1` selection fails validation until the real Sales contract/auth
  gate is wired; local testing uses `FAKE_TARGET_V1` and a refreshing mock token;
- timeout, bounded exponential backoff with jitter, retry budget and circuit-open
  period are configuration-bound;
- expired `SENDING` leases are recoverable and every completion is lease-fenced;
- terminal/retry state, safe code/error and review item are persisted for admin
  visibility; response bodies and credentials are never stored in audit/error text;
- readiness exposes disabled, ready and circuit-open states.

## Named behavior evidence

Focused callback unit suite (`33/33 PASS`) covers:

- every Target 200 semantic ACK, 409 subtype, 422, 429, 500/503, 401/403 and timeout;
- exact request path/body/auth/idempotency/correlation and byte-identical retry;
- path/body mismatch, changed payload hash and no-answer action rejection;
- foundation correlation propagation, token refresh, circuit open/half-open recovery,
  terminal local-result probe release and retry exhaustion;
- current Golden Hour exact path/header/body, explicit selection and forbidden 24/7;
- fail-closed options and stable atomic snapshot/hash construction.

Focused PostgreSQL evidence:

- result normalization `6/6 PASS`: final snapshot is atomic and idempotent, non-final
  results have no callback, completion is lease-fenced, and terminal review/audit is
  privacy-safe;
- scheduler persistence `8/8 PASS`: capacity-deadline closure persists a final,
  non-counted Target callback with `IVR_CAPACITY_EXCEPTION`.

## Verification evidence

| Gate | Result |
| --- | --- |
| Release analyzer build | PASS — 0 warnings, 0 errors |
| Contract tests | PASS — 21/21 |
| Unit tests | PASS — 157/157 |
| PostgreSQL integration tests | PASS — 65/65 |
| Total regression | PASS — 243/243 |
| Fresh aggregate line coverage | PASS — 94.13% (21,729/23,084), 3 reports, threshold 60% |
| `dotnet format --verify-no-changes` | PASS |
| EF pending model changes | PASS — none; P1-2 already owns the outbox schema |
| Target/current contract, OpenAPI, drift and negative gates | PASS |
| API documentation and local-link gates | PASS |
| GitLab CI configuration self-tests | PASS |
| Admin UI lint/build/npm High audit | PASS — 0 vulnerabilities |
| NuGet High vulnerability policy | PASS |
| Docker Compose MOCK profile | PASS |
| Gitleaks 8.30.0 working-tree scan | PASS — no leaks found |
| Locale-stable PII self-test + evidence scan | PASS — 26 text files, 2 binary files skipped |
| Official Markdown map | PASS — 417 files, 375 resolved links, 0 unresolved |
| GitNexus staged change scope | CRITICAL (expected) — 21 files, 189 symbols, 36 callback/normalization/scheduler flows |

The Linux security wrapper cannot execute its downloaded Linux Gitleaks binary
directly from Windows (`Exec format error 126`). Its NuGet/npm gates passed; the exact
pinned Gitleaks 8.30.0 scan was therefore run through Docker, as documented by the
project's Windows verification runbook.

## Safety and residual gates

- No real Sales endpoint, token issuer, current Core callback, SIM/eSIM, modem,
  customer destination or notification channel was invoked.
- Callback processing remains disabled by default. Enabling fake delivery is a local
  MOCK action only and does not authorize a real call.
- `W-0005` remains `OWNER_DATA_REQUIRED`: the current Golden Hour compatibility route
  needs the Sales-owned numeric identity mapping and approved legacy lifecycle.
- `W-0006` remains `OWNER_DATA_REQUIRED`: Target V1 endpoint/auth/scope, final payload
  approval and hosted contract evidence are not supplied.
- The Target contract remains `DRAFT`; production retry values, real token refresh,
  Sales/Core semantics, deployment and owner acceptance remain external/not run.
- There is deliberately no SMS/customer-message implementation in P2-6.
- One-SIM lab and future 32-eSIM provisioning remain later hardware/provider gates.

This evidence supports `TESTS_PASS` for end-to-end local/MOCK delivery only. It does
not support `ACCEPTED`, real-integration readiness or production readiness.

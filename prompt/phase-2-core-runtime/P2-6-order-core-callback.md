# PROMPT P2-6 — Sales Result Callback, Outbox and Compatibility Adapter

## 0. Meta

Work `W-0023` · prereq P2-5 · mode `MOCK`.

## 1. Outcome

Implement outbound callback delivery as immutable signal: Target generic Sales client is primary domain path; Golden Hour current endpoint is isolated compatibility provider. Complete E2E against WireMock/fake Sales now.

## 2. Build

1. Persist normalized result + callback outbox atomically with stable idempotency/payload hash/correlation.
2. Target client POSTs `/api/v1/internal/orders/{orderId}/ivr-result-callbacks` with service auth and required version/evidence fields.
3. Map 200 `ACCEPTED/DUPLICATE_ACCEPTED/BLOCKED_BY_CORE/REVIEW_REQUIRED`; 409 stale/conflict; 422 DLQ; 429/5xx/timeout bounded retry with same body/key.
4. `NO_ANSWER_FINAL` always sends `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`; no direct cancellation/notification.
5. Current GH adapter uses its verified schema/path/auth, only for GH under explicit config; rejects 24/7 route and cannot leak legacy behavior into domain.
6. Add token refresh, timeouts, jitter/backoff, circuit/readiness and admin-visible delivery states.

## 3. Tests/evidence

WireMock/contract tests for all Target ACK/statuses, duplicate/conflict, retry identity, timeout, auth failure, path/body mismatch, no-answer semantics, adapter selection and forbidden 24/7→GH compat. Update W-0023; W-0005/W-0006 stay blocked until real Sales/auth evidence.

## 4. Forbidden/DoD

No treating callback 200 as order confirmed; no retry 409/422 blindly; no current endpoint as Target; no notification. Completion means end-to-end MOCK only.

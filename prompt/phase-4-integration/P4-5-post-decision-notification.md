# PROMPT P4-5 — V1 Notification Disabled Boundary

## 0. Meta

Work `W-0033` · status `DEFERRED_TARGET` · V1 implementation is a no-op boundary, not a delivery integration.

## 1. Outcome

Prove IVR cannot send SMS, email, Zalo or any customer notification in V1. Keep a future extension interface only if it prevents coupling; it must have no provider credential/egress and be disabled in every mode.

## 2. Build/tests

1. Remove/avoid CRM notification client and customer-message templates from V1 runtime.
2. If an interface already exists, bind `DisabledNotificationSink` that writes privacy-safe audit only and cannot enqueue external delivery.
3. Add config validation that rejects enabling notification; no notification secret/environment variable.
4. Tests assert confirm/cancel/no-answer/timeout/technical results produce zero notification calls/outbox rows/network egress.
5. UI/docs display `V1_NOTIFICATION=DISABLED`, not “pending failure”.
6. Update W-0033 with grep/config/test/evidence; leave future CRM work outside V1 until a separately approved contract.

## 3. Forbidden/DoD

Do not implement a hidden flag that can turn delivery on, do not build a consumer “for later”, and do not treat notification as a release dependency. Done when no-op/no-egress evidence passes.

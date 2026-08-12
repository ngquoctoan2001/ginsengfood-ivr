# API-08 — External API Needs

Trạng thái: `OPEN_TARGET_V1` · Chi tiết owner/acceptance: `integration-requirements/*`.

## Sales Platform — P0

- task producer cho Golden Hour ONLINE và 24/7 COD;
- `order_version`, window/policy/eligibility evidence;
- `privacy_safe_order_summary`;
- dial-token issue/resolve;
- generic callback endpoint + semantic ACK + revalidation;
- timeout/no-answer behavior;
- production auth profile, OpenAPI, sandbox URL và test credentials.

Current Golden Hour callback is `CURRENT_COMPAT`, not contract closure.

## Telephony — P0 for lab/production

- adapter protocol/SDK/auth, DTMF/disposition/health;
- 1 SIM thật + allowlist cho lab;
- 32 eSIM channel provisioning/capacity/failover/caller-ID cho production.

## Foundation/Legal — gated

JWT/mTLS decision, admin RBAC, audit/retention, script/privacy approval và release sign-off. V1 notification API is not required because notification is disabled.

All dependencies have mocks so IVR code can complete, but their gates remain `BLOCKED_EXTERNAL` until real evidence exists.

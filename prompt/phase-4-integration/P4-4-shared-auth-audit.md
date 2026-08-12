# PROMPT P4-4 — Target Service JWT, Optional mTLS and Audit

## 0. Meta

Work `W-0032`; prereq P0-3; real path blocked on W-0006. Use mock JWT first.

## 1. Outcome

Implement one auth abstraction that is fully testable locally and can bind an owner-approved short-lived service-account JWT profile. mTLS support is optional until Security/Platform decides it. Legacy internal token remains current-compat only.

## 2. Build

1. Local mock OIDC/JWT issuer and deterministic keys for Compose/CI.
2. Ingress validates signature, issuer, audience, expiry/not-before, scopes and service identity; source header is metadata, never authentication alone.
3. Egress token provider uses client credentials, caches before expiry, refreshes safely and never logs secrets/tokens.
4. Optional mTLS handler/cert rotation hooks without making unapproved assumptions.
5. Current compatibility auth isolated and disabled outside explicit provider profile; sunset tracked.
6. Admin RBAC/audit/correlation remain separate from service auth.
7. Fail closed on identity/JWKS/token provider outage; expose safe readiness reason.

## 3. Tests/evidence

Valid token; wrong issuer/audience/scope/signature; expired/not-yet-valid; JWKS rotation/cache; refresh race; auth outage; token/log secret scan; optional mTLS profile validation; compat isolation. Record mock evidence in W-0032 and real profile/sandbox evidence under W-0006 before closing.

## 4. Forbidden

No `X-Source-System`/`X-Internal-Token` as Target authentication, no direct Ops/CRM service credentials, no secret in source/log/evidence, no claim real auth from mock JWT.

## 5. Definition of Done
- [ ] Mock JWT suite (issuer/audience/scope/expiry/nbf/alg/kid/JWKS-failure) xanh → **`TESTS_PASS`** (mock-only).
- [ ] Production auth profile vẫn `BLOCKED_EXTERNAL` (`W-0006`/`OD-V1-07`); **không** suy ra từ mock.
- [ ] Cập nhật Work ID `W-0032` với artifacts/commands/evidence/residual gate; chỉ reviewer/owner chuyển `ACCEPTED`.

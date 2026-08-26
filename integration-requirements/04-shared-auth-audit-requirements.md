# IR-04 — Shared Auth, Audit and Release Requirements

Trạng thái: `TARGET_V1_DRAFT` · Cập nhật: `2026-08-26`
Owner: **Security / Platform** (auth, RBAC, audit) và **Legal / Privacy** (PII, retention, release).

> `IR-FND-AUTH-01/02/03` là mặt Security của `IR-SALES-AUTH-01` — cùng một quyết định, hai bên cùng cần. Chi tiết integrator ở [IR-06 §7](06-module-3-api-handover.md); câu hỏi mở ở [IR-05](05-open-contract-questions.md) `OQ-AUTH-01`.

| ID | Yêu cầu | IVR build ngay | External/owner closure |
| --- | --- | --- | --- |
| `IR-FND-AUTH-01` | Dev mock JWT; validate issuer/audience/expiry/scope/service identity | auth abstraction + negative tests | Sales/Security chốt metadata production |
| `IR-FND-AUTH-02` | Production short-lived service-account JWT; không log token | client credentials/token cache/rotation hooks | issuer/JWKS/audience/scope/TTL |
| `IR-FND-AUTH-03` | Quyết định mTLS và certificate rotation | optional transport hook | Security/Platform owner |
| `IR-FND-AUTH-04` | `X-Internal-Token` chỉ `CURRENT_COMPAT`, không phải target auth | isolated compat handler/flag | sunset date |
| `IR-FND-RBAC-01` | Admin RBAC server-side cho queue/SIM/retry/review/config | policies/tests | map platform roles |
| `IR-FND-AUDIT-01` | append-only audit, idempotency, correlation và evidence refs | persistence/outbox/tests | retention/WORM policy |
| `IR-FND-PII-01` | không raw phone/full address/recording; masked UI/log | redaction and leak tests | privacy/script approval |
| `IR-FND-REL-01` | `REAL_CUSTOMER_CALL_ALLOWED=NO`; separate MOCK/LAB/PROD gates | config validator + kill switch | release sign-off |
| `IR-FND-RET-01` | retention duration per data class | configurable purge/legal hold | Legal/owner values |

## Auth acceptance

- valid service token succeeds; wrong issuer/audience/scope, expired token and unsigned token fail;
- token refresh/clock-skew behavior is tested;
- ingress and egress identity are separate;
- production secret never appears in repository, logs or evidence;
- auth failure is fail-closed and never dispatches a real call.

## Notification boundary

V1 notification is disabled (`TV1-07`). Foundation must not provision a notification credential to IVR. P4-5 proves no-op behavior only.

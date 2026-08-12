# ARCH-03 — Integration Architecture

Trạng thái: `TARGET_V1_DRAFT`.

| Integration | Direction | Transport | Auth/status |
| --- | --- | --- | --- |
| Target task | Sales → IVR | `POST /v1/ivr/order-confirmation/tasks` | service JWT; mock now |
| Target result | IVR → Sales | `POST /api/v1/internal/orders/{id}/ivr-result-callbacks` + outbox | service JWT; mTLS pending |
| Current compatibility | IVR → Sales | `/api/v1/internal/ivr/golden-hour/callbacks` | isolated/feature-flagged |
| Eligibility/blockers | Ops/CRM → Sales internally | Sales aggregates task + revalidates callback | IVR has no direct credentials |
| Telephony | IVR → gateway | provider port | mock / 1 SIM lab / 32 eSIM target |
| Admin | Next.js → IVR | internal REST | RBAC/service auth |
| Audit/evidence | IVR → approved sink | internal writer/export | no self-acceptance |

All commands carry idempotency/correlation. External failure is fail-closed. V1 has no customer-notification integration or generic event publication. `X-Internal-Token` belongs only to current compatibility; target uses short-lived JWT.

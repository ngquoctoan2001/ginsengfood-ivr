# API SRS — Index

Trạng thái: `TARGET_V1_DRAFT` · Cập nhật: `2026-08-12`.

| File | Nội dung |
| --- | --- |
| [01-conventions.md](01-conventions.md) | version/headers/envelope/auth/fail-safe |
| [02-internal-api.md](02-internal-api.md) | API do IVR .NET sở hữu |
| [03-admin-api.md](03-admin-api.md) | admin endpoints/RBAC |
| [04-sim-adapter-contract.md](04-sim-adapter-contract.md) | telephony port; mock/lab/real |
| [05-order-core-contracts.md](05-order-core-contracts.md) | Sales task/callback Target V1 + current compat |
| [06-error-codes.md](06-error-codes.md) | IVR error taxonomy |
| [07-idempotency-and-correlation.md](07-idempotency-and-correlation.md) | replay/conflict/trace rules |
| [08-external-api-needs.md](08-external-api-needs.md) | external dependencies |
| `openapi/ivr-order-confirmation.v1.yaml` | IVR-owned internal/admin API Target V1 draft |
| `openapi/order-core-ivr-callback.target-v1.yaml` | Sales-owned callback proposal; chưa implemented/locked |

## Invariants

- IVR không update order và không gửi SMS/notification.
- Task supports Golden Hour ONLINE và 24/7 COD only theo flag.
- Raw phone/full address không đi vào persistence/log/script; dial bằng token.
- Every command uses auth, idempotency and correlation.
- Three modes: MOCK, LAB_REAL_SIM allowlist, PRODUCTION_REAL gated.
- Attempt numbers are policy-versioned; candidate values are not schema-hardcoded.

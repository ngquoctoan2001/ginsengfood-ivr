# Testing SRS — Index

Trạng thái: `TARGET_V1_DRAFT`.

| File | Scope |
| --- | --- |
| [01-strategy.md](01-strategy.md) | test pyramid, mocks vs real evidence |
| [02-unit-test-plan.md](02-unit-test-plan.md) | domain/property/privacy |
| [03-integration-test-plan.md](03-integration-test-plan.md) | PostgreSQL + fake Sales/mock SIM/JWT |
| [04-contract-test-plan.md](04-contract-test-plan.md) | two OpenAPI + current compatibility |
| [05-e2e-test-plan.md](05-e2e-test-plan.md) | end-to-end business/runtime modes |
| [06-performance-test-plan.md](06-performance-test-plan.md) | 1/32-channel simulations and measured gate |
| [07-security-privacy-test-plan.md](07-security-privacy-test-plan.md) | auth/RBAC/PII/egress/mode guards |
| [08-acceptance-criteria.md](08-acceptance-criteria.md) | four outcome gates |
| [09-smoke-matrix.md](09-smoke-matrix.md) | Target V1 smoke set |

P0 tests: both program/payment rows; speech/token/privacy; versioned policy; technical≠no-answer; target callback semantics/idempotency; no-answer timeout; no order transition/notification; MOCK no egress; LAB allowlist; missing dependencies fail closed. Fake pass never counts as real integration/lab/production evidence.

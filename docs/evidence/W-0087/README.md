# W-0087 — Phase 1/2 runtime continuity remediation

Status: `TESTS_PASS` (local MOCK + disposable PostgreSQL)

## Findings addressed

- `E-01` was valid: operational MOCK intake used an in-memory store while the
  scheduler used PostgreSQL.
- `E-02` was valid: a fresh database had no source-owned SIM provisioning path.

Operational API/Worker registration now uses `PostgresTaskIntakeStore`,
`PostgresEligibilityRepository` and `PostgresSchedulerStore` for every normal
runtime mode. In-memory stores remain available only through the explicit
`useInMemoryTestDoubles` test-fixture switch, which rejects non-MOCK modes.

`MockSimChannelProvisioner` idempotently creates synthetic channel
`SIM-MOCK-001` only in MOCK. `ON CONFLICT DO NOTHING` preserves disable,
quarantine and lease decisions across restarts.

## Executable proof

`IT-MOCK-BOOT-01` boots the operational MOCK registrations against PostgreSQL,
asserts that intake and scheduler resolve persistent stores, runs provisioning
twice, proves exactly one synthetic channel exists, disables it, runs the
provisioner again and proves the admin state was not overwritten.

Focused result: `1/1 PASS`. Final regression: contract `21`, unit `168`,
integration `92` = `281/281 PASS`; Release build `0 warnings / 0 errors`.

## Boundary

This closes the source-owned continuity/provisioning defects. It does not prove
physical SIM/eSIM, external Sales, carrier, LAB, staging or production. No real
destination or customer call was used; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

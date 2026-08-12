# PROMPT P1-2 — PostgreSQL/EF Core Persistence for Target V1

## 0. Meta

Work `W-0015` · prereq P0-3 · mode `MOCK`.

## 1. Role/outcome

Bạn là Senior .NET/PostgreSQL Engineer. Tạo migrations, EF mappings và repository/outbox primitives đủ cho task/job/attempt/result/callback/channel/audit, với policy version và speech snapshot; không hard-code candidate D-10.

## 2. Read first

Governance/tracker · Target V1 draft · `specs/database/*` · `specs/data/*` · both OpenAPI schemas.

## 3. Build

1. Implement tables/mappings from DB specs, including contract/order version, program/payment/required flag, window, policy version/max/offsets, opaque dial-token expiry and privacy-safe speech JSON.
2. Add outbox/callback delivery state, immutable payload/hash/idempotency key and retry scheduling.
3. Add channel pool/lease/fencing/health/quarantine with provider/mode metadata; channel count is data/config.
4. Constraints: unique scoped idempotency/task/callback; max attempts bounds 1..10; task program/payment matrix; required flag; token/window bounds; technical not counted. Exact candidate timings are not DB CHECK constraints.
5. Add indexes for scheduler deadline, lease, callback outbox, order/task lookup and policy/version audit.
6. Add redaction/encryption hooks and retention columns; never raw phone/full address/recording.
7. Add forward migration, clean-db migration test and rollback/data-loss notes.

## 4. Tests/evidence

Testcontainers PostgreSQL: migrate empty/recreate, uniqueness/replay conflict, exact matrix, policy bounds/offset validation, concurrency lease/fencing, outbox dequeue, PII forbidden fixture, candidate policy stored as data. Record migration SQL/report in W-0015.

## 5. Forbidden/DoD

No shared Sales DB/entity; no exact `max=2`/300/150/900/450 constraint; no raw phone/address. `TESTS_PASS` requires real PostgreSQL tests, not EF InMemory.

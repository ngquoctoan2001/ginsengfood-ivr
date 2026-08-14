# W-0015 / P1-2 PostgreSQL persistence evidence

Date: 2026-08-12
Baseline: `5d2301e` (`P1-1`)
Status: `TESTS_PASS`; local persistence and historical hosted GitLab quality proof pass

> Evidence correction — 2026-08-14: historical hosted passes do not prove the
> current remediation tree. Current GitLab `main` has
> `Allowed to push and merge: No one`; the setting is `PASS_SETTING_CURRENT`
> under W-0061 and a fresh rejected-push probe is `NOT_RUN`.

## Delivered scope

- EF Core PostgreSQL model and initial migration for 17 IVR-owned tables:
  confirmation tasks, attempt policies, call jobs, attempts, raw events, results,
  callback outbox, SIM channels, capacity incidents, technical exceptions,
  admin actions, evidence links, idempotency keys, audit log, evidence, review
  items, and feature flags.
- Versioned attempt-policy and privacy-safe speech snapshots. Database checks
  enforce only invariant bounds (`1..10`); candidate timings remain data in the
  MOCK/LAB policy registry.
- Protected opaque dial-token storage hook. The default non-MOCK protector is
  fail-closed until a production key provider is selected.
- PostgreSQL callback outbox with immutable business payload/hash/idempotency
  fields and `FOR UPDATE SKIP LOCKED` dequeue leasing.
- Config-driven SIM channel pool with health/quarantine filters, atomic lease
  acquisition, lease token, and monotonic fencing generation. No fixed 32-SIM
  schema limit exists.
- Persistent idempotency, append-only audit, evidence storage, and PostgreSQL
  feature-flag mutation. Feature-flag rows, audit row, and command-idempotency
  response commit in the same serializable transaction.
- P0-4 local persistence gap is closed. This does not approve real calls or
  production rollout.

## Migration artifacts

- `docs/evidence/W-0015/migration-up.sql`: generated EF `Up` script; 17 tables,
  94 indexes, six database triggers.
- `docs/evidence/W-0015/migration-down.sql`: generated EF `Down` script; drops
  all 17 IVR tables.
- `dotnet ef migrations has-pending-model-changes`: no pending model changes.
- Forbidden candidate constraint scan: zero matches for exact attempt count or
  candidate timing literals in database checks.

The `Down` script is destructive and causes total IVR persistence loss. It is
evidence for clean-database rollback/recreate testing only. It must not be used
against staging or production without an approved backup, restore verification,
maintenance window, and explicit release-owner authorization.

## PostgreSQL integration proof

The dedicated Testcontainers suite ran against `postgres:16-alpine` on Docker
Engine 29.6.2 and passed 6/6:

| Test ID | Assertion |
| --- | --- |
| `IT-DB-MIGRATE-01` | empty database migrate, `Down` to zero, recreate, 17 tables and 45 safe feature-flag seeds |
| `IT-DB-TASK-02` | canonical Target V1 seed insert, dynamic three-attempt policy, uniqueness, raw-phone/forbidden-PII rejection, invalid-offset rejection, no candidate DB constants |
| `IT-DB-ATTEMPT-03` | attempt snapshot copied from job, immutable numbering, technical retries not counted and not blocked by the customer-attempt unique index |
| `IT-DB-FLAG-04` | feature flags + append-only audit + idempotency commit atomically; replay/conflict and rollback verified |
| `IT-DB-LEASE-05` | concurrent channel acquire leases once; stale release rejected; fencing generation increases |
| `IT-DB-OUTBOX-06` | concurrent dequeue leases callback once; payload hash retained; database trigger rejects payload mutation |

## Full local gates

- locked restore: PASS;
- Release build: PASS, 0 warnings / 0 errors;
- tests: PASS, 61/61 (`19` contract, `22` unit, `20` integration);
- aggregate line coverage: `90.64%` (`6765/7464`, three reports), threshold
  `60%` PASS;
- `dotnet format --verify-no-changes`: PASS after normalizing the EF-generated
  migration to repository LF/UTF-8 rules;
- UI ESLint and Next.js production build: PASS;
- OpenAPI lint, parse/schema validation, pinned drift and negative self-test:
  PASS;
- GitLab config self-test: PASS, including pinned Testcontainers DIND service;
- Docker Compose MOCK config: PASS;
- NuGet and npm High/Critical audit: PASS, zero reported vulnerabilities;
- Gitleaks 8.30.0: PASS, no leaks. Ten EF-generated
  `v1NotificationEnabled` seed findings were reviewed as non-secret property
  names and suppressed by exact file/rule/line fingerprint only;
- locale-stable PII self-test and current evidence scan: PASS;
- official Markdown map: 397 files, 369 resolved links, 0 unresolved links.
- GitNexus staged review: `MEDIUM`, 40 files, 35 indexed symbols, four
  feature-flag read flows, zero circular imports. New persistence symbols are
  not yet in the index, so symbol/process counts are a documented lower bound;
  direct source review plus PostgreSQL/full regression tests are authoritative.

## Explicit residual gates

- `W-0061` / `G-GITLAB`: `BLOCKED_EXTERNAL`. Historical runner, DinD,
  hosted-MR, Registry, Pages and variable evidence remains valid; current
  no-direct-push configuration passes, while independent approval remains
  unavailable.
- `DF-07`: retention periods and deletion/legal-hold policy remain owner/legal
  data. Rows default to `LEGAL_DECISION_PENDING`; no purge job is armed.
- Production encryption/KMS provider, key rotation, backup/restore drill,
  staging migration and production rollback approval: `NOT_RUN`.
- `TARGET_CONTRACT_V1` remains `DRAFT`; Sales endpoint/auth/CDC owner approvals
  remain external. No Sales API, SIM/eSIM device, SMS, or real customer call was
  executed.

## Reproduction commands

```powershell
dotnet restore Ivr.sln --locked-mode
dotnet tool restore
dotnet ef migrations has-pending-model-changes --project src/Ivr.Infrastructure/Ivr.Infrastructure.csproj --no-build --configuration Release
dotnet format Ivr.sln --no-restore --verify-no-changes
dotnet build Ivr.sln --configuration Release --no-restore
dotnet test Ivr.sln --configuration Release --no-build --collect:"XPlat Code Coverage"
npm --prefix deploy/ci run test:config
docker compose -f docker-compose.dev.yml --profile mocks config --quiet
```

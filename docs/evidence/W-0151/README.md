# W-0151 — Attempt-policy production audit evidence

Status:
`EVIDENCE_SUBMITTED / EXTERNAL_SIGNATURES_REQUIRED / CODE_NOT_AUTHORIZED`

Baseline IVR: `main@b21ec676e4906ab6886adb442fb596c35be67c66`

M3 snapshot reviewed: `C:\Projects\ginsengfood-business-platform`,
`PhucApu@a3aad246d986fbc273cf41aaa93eec6659669656`.

## Scope

Read-only audit of candidate/business numbers, task wire, registry/write path, persistence
invariants, intake mismatch behavior, scheduler/counting/deadline, technical retry, runtime config,
seed/test coverage and the current M3 snapshot. The only W-0151 mutations are documentation,
decision handoff, ledger/status mirrors and generated documentation memory.

No source, OpenAPI, migration, seed, scheduler, registry, config, deployment or secret change is
authorized or included.

## Verified findings

| Area | Evidence | Result |
| --- | --- | --- |
| Candidate numbers | `src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs:10-30` | `mock-lab-v1`: GH `2/[0,150]/300s`; 24/7 `2/[0,450]/900s`; candidate only |
| Business conflict | `docs/documents/4. phase/phase-8/10-KIẾN TRÚC TRIỂN KHAI.md:125-126`; `16-YÊU CẦU PHI CHỨC NĂNG.md:26-27` | GH `2/[0,300]/600s`; 24/7 `3/[0,300,600]/900s` |
| Dev/lab seed | `deploy/docker/dev-seed/seed.sql:27-37`; `deploy/lab/seed.sql:6-12` | dev `mock-lab-v1` only MOCK; lab uses separate `lab-softphone-v1` |
| Registry resolution | `AttemptPolicyRegistries.cs:33-89` | exact version/program/mode; production requires allowed mode + owner-approved row |
| Intake mismatch | `TaskIntakeService.cs:125-165,372-382` | unknown/disallowed held; max/offset/window mismatch fails with `IVR_POLICY_MISMATCH` |
| Immutable job snapshot | `TaskIntakeService.cs:668-753`; `PersistenceInvariantValidator.cs:70-86` | accepted task/job retain version/max/offset/window/schedule snapshot |
| Scheduler counting | `PostgresSchedulerStore.cs:90-140` | only counted attempts advance slot; no claim after expiry/final/active attempt |
| Technical retry | `DispositionMapper.cs:193-212`; `ResultRepository.cs:324-383`; `SchedulerCapacity.cs:25-40` | uncounted; default limit 1; requeue/admin-review behavior separate from versioned policy |
| Registry governance | `AttemptPolicyRegistryWriter.cs:22-112`; entity/model/migration | immutable per row + audit hash, but no signed refs/four-eyes/effective-retire/bundle atomicity |
| Runtime flag | `FeatureFlagGuardrails.cs:89-108`; `DispatchGate.cs:15-68` | known candidate literals blocked; no registry lookup or equality with job policy |
| Wire shape | `specs/api/openapi/ivr-order-confirmation.v1.yaml:1143-1188` | version + snapshot required; structural bounds do not express all cross-field invariants |
| M3 producer | exact scan on pinned snapshot | target producer fields/code not found; only high-level business doc references |

## Claims corrected

1. T-09 previously said no rule existed when version and parameters conflicted. Current intake does
   have exact comparison and `409 IVR_POLICY_MISMATCH`; the document was corrected.
2. Docs broadly said `mock-lab-v1` runs MOCK/LAB. Code/dev loader allows both, but default dev seed
   allows only MOCK and lab seed uses `lab-softphone-v1`; docs now state the distinction.
3. A blanket “production fails startup if policy is unapproved” was too strong. Current evidence is
   intake resolution plus pre-dial feature-flag validation; no registry-wide startup activation
   validation was found.

## Decision artifact

- [M8-11 decision pack](../../../plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md)
- [T-09 closure ticket](../../contracts/target-v1-closure-pack/T-09-attempt-policy.md)

## Validation record

| Gate | Kết quả 03/09/2026 |
| --- | --- |
| Focused Unit — attempt policy/intake/normalization/deadline/feature flag/fail gates | **PASS `67/67`** |
| Contract suite | **PASS `24/24`** |
| Focused PostgreSQL — immutable intake, final-attempt scheduling, policy writer/production reject, technical/invalid counting | **PASS_LOCAL_POSTGRES `6/6`** — W-0161 full integration `236/236`, 0 fail/skip; gồm `IT-INTAKE-DB-01`, `IT-SCH-FINAL-04`, `IT-POLICY-AUDIT-05`, `IT-POLICY-PROD-06`, `IT-NORM-TECH-02`, `IT-NORM-INVALID-04` |
| API docs | **PASS** — `14` generated artifact; boundary/link/topology checks xanh |
| OpenAPI | **PASS** — invalid spec rejected; parse `2` file; `9` task fixture; `12` schema-negative; `13` domain-negative; `1` compatibility; `3` pinned hashes current |
| Test traceability | **PASS `476`** |
| Tracker/readiness mirror | **PASS** — `11` gate, `149` work item, `23` open decision; no rung claimed; production flag `false` |
| Official Markdown map | **PASS** — `633` Markdown file; M8-11 pack, T-09 và W-0151 evidence đều `0` unresolved link |
| `git diff --check` | **PASS** — chỉ có line-ending conversion warnings của shared worktree |

Ghi chú lịch sử: lần chạy W-0151 ban đầu dừng ở fixture. W-0161 đã chạy assertion thật qua local
Docker/Testcontainers; xem [evidence W-0161](../W-0161/README.md). Local checks không thay external
signatures, M3 producer hoặc production acceptance.

## Residual gates

- `ATP-01..ATP-15`: `NOT_SIGNED`.
- Production policy bundle/version/hash: `NOT_RECEIVED`.
- M3 producer OpenAPI/schema/CDC/sandbox: `NOT_RECEIVED / NOT_RUN`.
- Registry lifecycle/four-eyes and runtime policy coherence design: `NOT_APPROVED`.
- Shared E2E, cutover/rollback, capacity/token recalibration: `NOT_RUN`.
- Production attempt policy: `NOT_APPROVED`.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## Next step

Route ATP-01..ATP-15 to Product, Order Core and M3. Do not open scheduler/registry implementation
until the signed bundle and producer artifacts are attached to this evidence folder.

# W-0013 — P0-4 Feature-Flag Platform Evidence

Date: 2026-08-12

Baseline: `main@1c08cf0` (dedicated P0-3 commit)

Execution mode: `MOCK`

Real customer calls: `NO`

Final status: `TESTS_PASS`. The source, local/container quality gates, MOCK
store, isolated API tests and later hosted GitLab quality pipelines are
complete. OD-V1-20 approval and later-phase production gates are not implied by
this evidence.

## Scope implemented

- Canonical environment and flag catalog for `dev`, `staging`, `lab`, `pilot`,
  and `prod`, with `MOCK`, `LAB_REAL_SIM`, and `PRODUCTION_REAL` execution modes.
- Safe per-environment seed snapshots: real-customer permission, notification,
  and recording are false; destination sets are empty; the global dial kill
  switch is on.
- `IFeatureFlags`, `IDynamicConfig`, refresh, cache, store, `IKillSwitch`, and
  centralized `IDispatchGate` interfaces. Dispatch and kill checks force a fresh
  read, so an old permissive cache cannot authorize a call.
- A MOCK in-memory store with optimistic revision checks and an atomic
  audit-before-change transaction. A PostgreSQL read adapter and EF model are
  present, but persistent mutation deliberately fails closed until P1-2.
- `ivr_feature_flags` entity mapping with the canonical fields, composite key,
  unique index, and 45 safe seed rows. No migration was created because DB-02
  section 8 assigns physical migrations to P1-2.
- Admin GET, kill verification, and mutation endpoints plus matching OpenAPI
  schemas. Mutation requires exact permissions, an authenticated actor matching
  the actor header, reason, idempotency key, and append-only audit.
- Opaque four-eyes approval verification. The request cannot declare an
  approver identity, the proposer cannot approve its own request, and an actor
  cannot add its own destination reference.
- Asymmetric safety controls: production risk increases are deployment-only;
  real-customer enable, notification enable, and recording enable are rejected.
  Kill-on, destination narrowing, and real-customer disable are immediate risk
  reductions. Kill-on remains possible even when another stored control was
  already invalid.
- Runtime permission approval defaults to denied because OD-V1-20 is open.
  Tests that exercise the complete workflow replace this with isolated approved
  fakes; production code contains no self-approval flag.
- Both API and Worker receive the centralized feature-flag/dispatch gate DI
  graph. The existing legacy `LAB` configuration spelling was removed in favor
  of `LAB_REAL_SIM`.
- Full-suite stress exposed a pre-existing correlation flake: an ungrouped
  random GUID could accidentally resemble restricted numeric data. Generated
  IDs now use a fixed prefix and four-character groups; the existing PII test
  validates 1,000 generated IDs and five consecutive full-suite runs pass.

## Required P0-4 tests

| Test ID | Result and proof |
| --- | --- |
| `UT-FLAG-DEFAULT-01` | PASS — risky defaults are safe; provider failure clears permissive cache use and makes the kill switch effective |
| `UT-FLAG-GUARD-02` | PASS — admin cannot enable real-customer calling even with test-scoped runtime and approval fakes |
| `UT-FLAG-AUDIT-03` | PASS — actor, reason, before, and after are appended; audit failure leaves the store unchanged |
| `UT-FLAG-AUTHZ-05` | PASS — missing `IVR_RUNTIME_GATE_ADMIN` returns HTTP 403 with the stable forbidden code |
| `UT-FLAG-ALLOWLIST-06` | PASS — reason, independently verified approval, and self-authorization rejection are enforced |
| `IT-FLAG-PRODGUARD-07` | PASS — production allows kill-on and narrowing, rejects kill-off/expansion, and rejects notification/recording enable |
| `IT-FLAG-EMERGENCY-10` | PASS — on-call kill-on succeeds without four-eyes even when a different stored control is already invalid, and the action is audited |
| `IT-FLAG-KILLSWITCH-08` | PASS — kill-on immediately blocks an otherwise valid allowlisted lab dispatch |
| `IT-FLAG-FAILCLOSED-09` | PASS — config or audit health failure blocks dispatch; audit write failure cannot mutate state |
| `IT-FLAG-KILL-04` | PASS — the atomic `LAB_REAL_SIM` to `MOCK` batch updates mode/provider/kill state and the verification endpoint sees the new revision immediately |

The prompt's review prose says four tests, but its test table contains ten IDs.
All ten pass. Three additional P0-4 tests pass: `UT-FLAG-MODEL-11` validates the
EF table/key/45 seeds, `IT-FLAG-OWNERGATE-12` proves default owner-gate denial,
and `IT-FLAG-IDEMP-13` proves replay returns the original response with one
audit entry and one revision increment.

## Final local gate results

| Gate/command | Exact result |
| --- | --- |
| locked restore | PASS — all nine solution projects restore from committed lock files |
| Release build/analyzers | PASS — 0 warnings, 0 errors |
| full test run | PASS — 27/27 implemented tests: 13 unit and 14 integration; repeated five consecutive times after correlation remediation |
| coverage policy | PASS — merged line coverage `87.50%` (`1183/1352`, 3 Cobertura reports), above 60% |
| format | PASS — `dotnet format --verify-no-changes` |
| admin UI | PASS — clean install, ESLint, strict TypeScript, production build, 0 npm vulnerabilities |
| CI configuration | PASS — CT-CI-05/07/08; all fragments reachable and artifact topology intact |
| OpenAPI | PASS — 2/2 parse, Target fixtures and negative validation pass; 10 pre-existing advisory warnings, 0 errors |
| dependency policy | PASS — NuGet High/Critical gate and both npm audits; 0 npm vulnerabilities |
| Compose | PASS — local development configuration including inert mocks renders |
| secrets and privacy | PASS — Gitleaks plus locale-stable PII self-tests/current evidence scan |

## Safety and transaction review

- `IKillSwitch` and `IDispatchGate` never authorize from a cached snapshot. A
  missing/unreadable store is interpreted as kill-on.
- Dispatch evaluates audit-provider health, kill state, the complete
  mode/provider/policy combination, the opaque destination set, and the
  production release gate in one centralized path.
- Admin mutations derive actor identity from authentication. MOCK identity
  headers are rejected outside MOCK alongside the existing permission header.
- Four-eyes verification consumes an opaque server-verifiable reference. It
  does not trust an approver field from the request.
- The in-memory transaction holds the revision lock, writes audit first, then
  replaces the snapshot. If audit fails or the expected revision changed, no
  feature state is committed.
- API command replay snapshots a dedicated JSON-compatible response DTO. The
  replay regression confirms that retry does not duplicate audit or revision.
- Emergency kill-on is a narrowly classified unconditional risk reduction. It
  cannot carry another flag change in the same bypass; all other batches must
  pass full effective-state validation.

## Residual gates and ownership boundaries

- OD-V1-20 is still pending Security/Release owner approval. The production
  `IRuntimeGateAuthorization` therefore returns false. Test fakes do not close
  this owner decision.

> **Superseded 2026-08-22 — `OD-V1-20` approved.** `IVR_FLAG_READ` and `IVR_RUNTIME_GATE_ADMIN` are now granted to the `Admin` role. The statement above records the state at this evidence pack's baseline and is left unchanged; current state lives in `plan/ivr-orther/decisions-log.md` and `specs/ui/08-role-permission-ui.md` §2.

- P1-2 owns the physical `ivr_feature_flags` migration and persistent atomic
  audit/flag/idempotency transaction. The P0-4 PostgreSQL mutation path remains
  deliberately `OPERATIONAL_BLOCKED` until that work is complete.
- W-0061 remains `BLOCKED_EXTERNAL` only for required independent MR approval;
  all other GitLab project/runner/branch/merge-check/Registry/Pages/variable
  evidence is complete.
- Production identity and approval providers remain later work. The current
  non-MOCK path cannot self-enable runtime mutation.
- Sales API/auth, physical SIM/eSIM, customer calls, lab, staging, pilot, and
  production are `NOT_RUN`. No call provider was invoked by P0-4.

## GitNexus change review

The refreshed index contains 37,400 nodes, 38,474 edges, and 62 flows. Final
staged `detect_changes` reports `CRITICAL`: 42 files, 279 symbols, and 49
affected flows. The warning is retained rather than relabelled. This P0-4 slice
adds one complete platform vertical plus API, tests, OpenAPI, evidence, and the
generated Markdown map. Two of the 42 files are GitNexus-generated index-count
updates in `AGENTS.md` and `CLAUDE.md`.

Focused upstream impact is `MEDIUM` for `InMemoryFeatureFlagStore` (11 test
dependants; seven direct) and `LOW` for the admin service, dispatch gate,
feature platform, endpoint, guardrails, EF context, and correlation generator.
Graph queries group the affected flows into the expected P0-4 families:
admin-to-store/audit/stable-error, fresh-read-to-operational-block,
dispatch-to-guard/privacy, and correlation-to-safe-generator. Interface/DI
edges are explicitly lower-bound, so the full API/Worker integration suite is
the authoritative consumer proof. No unexpected external source consumer was
identified and the circular-import check reports zero cycles.

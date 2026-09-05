# W-0191 — P0.2 Development seed/scenario bootstrap

Status: `TESTS_PASS`  
Date: `2026-09-05`  
Baseline: `main@2a6d2902a27d6d41a10d244ba28a5c78da1a86e2` plus preserved shared WIP  
Execution boundary: `Development / MOCK / local PostgreSQL`; no worker, vendor or customer call

## Scope and root cause

- Give Development a safe committed `Ivr:DevTooling:SeedDirectory` and resolve relative values
  from the API content root, not the caller's working directory.
- Fail during options startup validation when a configured directory or any of the three catalog
  files is missing, with the configuration key, missing file and recovery action in the message.
- Provide one documented command that prepares PostgreSQL, starts an isolated API, loads all nine
  task fixtures and dry-runs `SCN-001-confirm` without private environment variables.
- Preserve the do-not-call invariant on repeat loads: the eight admitted fixtures point to their
  existing jobs; the restricted ninth fixture is not mislabeled as loaded because it has no job.

The prior manual path depended on where `dotnet run` was launched, performed no preflight over the
catalog, and required separate database/API/HTTP steps. The repeat-seed assertion also expected the
restricted fixture to have a job even though the real intake path correctly blocks it.

## Implementation evidence

- `appsettings.Development.json` supplies `../../seed`; `ResolveSeedDirectory` anchors it to
  `IHostEnvironment.ContentRootPath`. Empty remains the safe disabled default elsewhere.
- `DevToolingOptionsValidator` checks the directory and the task/scenario/integration-profile files
  before the host accepts traffic. `SeedCatalog.ReadBytesAsync` was not changed.
- `pnpm dev:bootstrap` validates prerequisites/catalog, builds, runs `local:prepare`, chooses a free
  loopback port, pins `MOCK / MOCK / NO`, starts only `Ivr.Api`, loads seed and checks the scenario.
- The command uses only the fake Development write credential already committed and documented; it
  starts no worker and stops exactly the temporary API process it created.

## Verification

| Check | Result |
|---|---|
| GitNexus pre-edit impact: seed-path resolver | `LOW`, 1 caller, 0 process |
| GitNexus pre-edit impact: options validator | `LOW`, 0 caller/process |
| GitNexus pre-edit impact: seed catalog reader | `HIGH`, 11 symbols/3 processes; deliberately not edited |
| Focused options tests | PASS `4/4` |
| Composition-root + DevTooling integration | PASS `20/20` |
| `pnpm dev:bootstrap` | PASS: build 0 warnings/errors; migration current; seed `9/9`; 8 dry-run jobs; 1 restricted/no-job |
| Scenario runtime | PASS: `SCN-001-confirm`, `REPLAYED`, `IVR_CONFIRMED`, `matches=true` |
| Full Unit | PASS `515/515` |
| Full PostgreSQL Integration | PASS `252/252` in `10m38s` |
| Full Contract / Chaos | PASS `24/24` / `8/8` |
| Aggregate .NET | PASS `799/799` by project results |
| Test traceability | regenerated and current, `498` TestId mappings |
| Post-change GitNexus shared checkout | `HIGH`, 18 tracked files, 363 symbols, 9 flows; includes concurrent P0.1/UI/lab WIP |

Audit note: the first aggregate command discovered only stale generated traceability (Unit
`514/515`); Integration `252/252`, Contract `24/24` and Chaos `8/8` were already green. After
regeneration, the complete Unit project passed `515/515`.

The final GitNexus result is an aggregate shared-tree warning, not the scoped P0.2 blast radius.
Pre-edit impacts for the symbols changed by P0.2 were LOW; the HIGH catalog reader was left intact.

## Exit and residual boundary

P0.2 exit is met: one `pnpm dev:bootstrap` command reproducibly returns nine seed outcomes and
replays `SCN-001-confirm`. The proof is local Development/MOCK only. It does not start the worker,
approve a real Sales/Module 3 connection, buy telephony, run a physical SIM, or authorize real calls.
`REAL_CUSTOMER_CALL_ALLOWED=NO` and all external gates remain unchanged.

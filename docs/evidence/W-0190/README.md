# W-0190 — P0.1 feature-flag API remediation

Status: `TESTS_PASS`  
Date: `2026-09-05`  
Baseline: `main@2a6d2902a27d6d41a10d244ba28a5c78da1a86e2` plus preserved shared WIP  
Execution boundary: `Development / MOCK / disposable PostgreSQL`; `REAL_CUSTOMER_CALL_ALLOWED=NO`

## Scope

- Repair the MOCK composition-root registration so all five feature-flag environments are seeded.
- Keep provider failure fail-closed while emitting a privacy-safe internal warning and counter.
- Convert unknown environment path segments from unhandled 500 responses to stable 404 envelopes.
- Add an integration test that starts the shipping `Program` composition root against the normal PostgreSQL fixture.

## Root cause

`InMemoryFeatureFlagStore` has a one-argument constructor that creates all safe-default environment
snapshots and a two-argument constructor that accepts explicit seeds. The previous type registration
allowed Microsoft DI to choose the greediest resolvable constructor. Because
`IEnumerable<FeatureFlagSnapshot>` resolves as an empty sequence, the store started with zero
environments. Every read then threw not-found and `FeatureFlagPlatform` correctly—but silently—fell
back to an unreadable safe default.

## Implementation evidence

- `FeatureFlagServiceCollectionExtensions.AddIvrFeatureFlags` now uses an explicit factory and the
  constructor that seeds `dev`, `staging`, `lab`, `pilot` and `prod`.
- `FeatureFlagPlatform` logs event `2400` with only environment and exception type, and increments
  `ivr.feature_flags.read_fallback`; it never logs exception message/provider detail.
- `FeatureFlagEndpoint` validates all GET/kill-switch/POST environment route values before platform
  access and returns `IVR_NOT_FOUND` for unknown input.
- `FeatureFlagProgramCompositionRootTests` starts `WebApplicationFactory<Program>` in Development,
  uses the disposable PostgreSQL fixture, and verifies both healthy and injected-outage paths.

## Verification

| Check | Result |
|---|---|
| GitNexus impact: `AddIvrFeatureFlags`, `FeatureFlagPlatform` | `LOW`, 0 affected process |
| GitNexus impact: environment guard | `LOW`, 2 direct callers, 1 process |
| Focused unit `FeatureFlagPlatformTests` | PASS `6/6` |
| Focused integration feature/composition-root tests | PASS `22/22` |
| Actual Development host snapshots | `dev/staging/lab/pilot/prod`: HTTP `200`, `ProviderReadable=true` |
| Actual Development host kill switch | all five: HTTP `200`, switch ON, real calls false |
| Unknown `Development`, `MOCK`, `nonesuch` | HTTP `404`, `IVR_NOT_FOUND` |
| Provider-outage snapshot | HTTP `409`, `IVR_OPERATIONAL_BLOCKED`, provider detail absent |
| Provider-outage kill switch | HTTP `200`, unreadable, switch ON, real calls false |
| Release build | PASS, 0 warning, 0 error |
| Scoped format and diff checks | PASS |
| Focused PII scan | PASS, 8 files; `CT-CI-06..06h` controls PASS |
| Tracker/readiness/docs | PASS, 11 gates / 188 work / 23 decisions; 494 test IDs; docs self-test PASS |

Aggregate check: Unit `511/511`, Contract `24/24`, Chaos `8/8` passed. Integration was
`251/252`; the only failure was the concurrently modified, out-of-scope
`DevToolingApiTests.LoadingTwiceNamesEachFixtureAlreadyLoadedAndAddsNothing`, where the restricted
seed has no existing call job and remains an idempotency conflict. It does not execute the feature-
flag path and is not counted as W-0190 evidence.

Post-change GitNexus `detect-changes` on the aggregate shared checkout reported `HIGH`, 17 tracked
files, 125 symbols and 9 flows because it also includes concurrent seed-loader, UI, lab and other
WIP. The pre-edit symbol impacts for this feature-flag scope were `LOW`; the aggregate result must
not be presented as a scoped W-0190 blast radius.

## Residual boundary

- This closes the local P0.1 defect only. It does not approve risk-increasing flag mutation,
  production credentials, staging rollout or real calls.
- External gates and `REAL_CUSTOMER_CALL_ALLOWED=NO` remain unchanged.

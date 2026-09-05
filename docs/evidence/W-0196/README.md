# W-0196 — P0.3 migration expand-contract

Status: `IN_PROGRESS` — local PostgreSQL checks passed; exact-SHA two-binary drill pending.
Baseline: `ba43605` (after the independently completed W-0193..W-0195 merge).

## Scope and reproduction

- Reproduced `progressive-selftest.mjs` failure: W0122 `Up` dropped both console tables.
- Corrected historical W0122 to preserve tables; additive P03 repairs missing compatibility shape.
- Removed the two table-drop exemptions; static and EF-operation gates now inspect raw SQL.
- Two unchanged pre-W0118 SQL migrations are byte-pinned historical baseline, not evidence of
  safe upgrades from arbitrary historical binaries. No W0122 drop exemption remains.
- Current API/worker source has no retired account/session consumer outside migration files.
  External consumers and real rollout observation must be checked before a later contract release.
- [Runbook](../../database/expand-contract.md); [two-binary drill](../../../tools/dev/Test-ExpandContract.ps1).

## Verification in progress

- `node deploy/ci/scripts/progressive-selftest.mjs`: PASS, 22 migrations, 15 negative controls.
- Focused schema unit tests: 16/16 PASS.
- PostgreSQL clone/upgrade/rollback/forward + readiness + complete teardown/recreate: 5/5 PASS.
- Final candidate SHA, executable/dump hashes and runtime rollback results will be added after
  running the clean-commit drill. Do not use this interim document as completion evidence.

## Safety and limits

MOCK / MOCK / NO, synthetic data only, no worker or provider calls. Missing old rows require a
verified pre-drop backup; empty shape repair is not data recovery. Hosted CI and cluster rollback
remain NOT_RUN; real consumer confirmation and cleanup are a later release, not part of expand.

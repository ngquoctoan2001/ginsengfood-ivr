# W-0196 — P0.3 migration expand-contract

Status: `TESTS_PASS` — P0.3 local/PostgreSQL exit achieved on 2026-09-05.
Candidate: `c8dc3c44ee803717d952541f0d90383545230da2` (clean commit during the drill).
Previous: `ba436059cf0094404a58c328025e69ac2c771cf8` (W-0193..W-0195 already merged).
This evidence-only follow-up does not change candidate code, tests, CI or the drill script.

## Scope and reproduction

- Reproduced `progressive-selftest.mjs` failure: W0122 `Up` dropped both console tables.
- Corrected historical W0122 to preserve tables; additive P03 repairs missing compatibility shape.
- Removed the two table-drop exemptions; static and EF-operation gates now inspect raw SQL.
- Two unchanged pre-W0118 SQL migrations are byte-pinned historical baseline, not evidence of
  safe upgrades from arbitrary historical binaries. No W0122 drop exemption remains.
- Current API/worker source has no retired account/session consumer outside migration files.
  External consumers and real rollout observation must be checked before a later contract release.
- [Runbook](../../database/expand-contract.md); [two-binary drill](../../../tools/dev/Test-ExpandContract.ps1).

## Verification on the pinned candidate

- `node deploy/ci/scripts/progressive-selftest.mjs`: PASS, 22 migrations, 15 negative controls.
- Full unit: **528/528 PASS**; schema guard subset **16/16** (included, not added to total).
- PostgreSQL clone/upgrade/rollback/forward + readiness + complete teardown/recreate: **5/5 PASS**, no skips.
- EF pending model changes: none. Scoped `dotnet format --verify-no-changes` and `git diff --check`: PASS.
- `ci-config-selftest.mjs` and `cd-selftest.mjs`: PASS (configuration, not hosted execution).
- GitNexus initially could not resolve the unindexed isolated worktree; after indexing,
  compare against `ba43605`: **22 files / 86 symbols / 0 affected processes / LOW**.

## Actual backup/restore and two-binary rollback

Command: `pwsh -NoProfile -File tools/dev/Test-ExpandContract.ps1 -PreviousRef ba43605`.
Result: **EXPAND_CONTRACT_DRILL_PASS**, PostgreSQL **16.15**, run `9c8c99f6ebda49e5b468e6258e1af6a8`.

1. Previous API seeded nine outcomes/eight jobs on its own migrated database.
2. `pg_dump -Fc` → `pg_restore --exit-on-error` copied data and EF history into a separate database.
3. New API initially returned readiness `503/schema_behind`; after expand it recovered without restart.
4. Both distinct SHA binaries remained active on the same forward schema; both passed ready/seed/confirm.
5. Candidate stopped; restarted previous binary passed the same checks (application rollback, no Down).
6. Restarted candidate passed again (forward recovery); source and copy task fingerprints stayed equal.

The N/N+1 overlap claim means this **actual adjacent-version pair**, not an unbuilt future cleanup release.
The pre-drop legacy account/session data path is covered separately by `IT-SCHEMA-EXPAND-07`.
[Machine evidence](rollback-evidence.json) contains exact SHAs, binary/backup hashes and schema history;
[verification summary](verification-summary.json) records test totals and raw TRX hashes.
Raw synthetic dump/logs/TRX remain in the isolated worktree at `ci-artifacts/expand-contract/<run>/`.
Only the drill's temporary APIs/container were removed; the developer database was not changed.

## Safety and limits

MOCK / MOCK / NO, synthetic data only, no worker or provider calls. Missing old rows require a
verified pre-drop backup; empty shape repair is not data recovery. Hosted CI and cluster rollback
remain NOT_RUN; real consumer confirmation and cleanup are a later release, not part of expand.

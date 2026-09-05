# P0.3 — Retire console storage without breaking rollback

Scope: `W-0196`, PostgreSQL 16, local MOCK. Operator identity remains owned by Module 3.
This release does **not** enable legacy authentication, a worker, telephony or customer calls.

## Release sequence and cleanup gate

| Phase | Required action/evidence | State in this change |
| --- | --- | --- |
| Stop consuming | Remove runtime reads/writes/models of `ivr_console_accounts` / `ivr_console_sessions`; inventory API, worker, scripts, BI, jobs and external DB users | Active `src/` has no references outside migrations; external inventory still required before cleanup |
| Compatible deployment (expand) | Keep physical tables and rows while old/new processes overlap | W0122 keeps its historical ID but `Up`/`Down` are no-ops; P03 additive repair creates only missing shape |
| Observe retirement | Record deployed image/commit digests for **every** replica/job, no legacy SQL during at least the longest job/session lifetime and agreed rollback window | Required on the real deployment; source search alone cannot establish this |
| Close rollback window | Owner accepts restore rehearsal and consumer inventory; no old image remains eligible for rollback; privacy/retention/legal-hold treatment is approved | Not inferred from local tests |
| Contract in a later release | Separate reviewed cleanup migration, backup checksum, restoration instructions and explicit release approval; drop sessions before accounts | **DEFERRED**; no executable cleanup ships in this expand release |

The two-person team can perform the inventory and rehearsal together. External consumers must be
confirmed by their owner; absence of credentials or responses is not a zero-consumer confirmation.
Record the deployment window start/end, observation query/log reference, consumer list, signer,
approved rollback floor and cleanup candidate SHA. A missing field blocks cleanup.

## Existing database cases

1. W0122 not yet applied: the corrected migration preserves both tables and every row.
2. Old W0122 already recorded: EF will not rerun it. `20260905120000_P03PreserveConsoleCompatibility`
   adds missing tables/indexes without changing existing rows. An empty recreated table is **not**
   recovered account/session data. Do not re-enable an old authentication binary on that assumption.
3. Deleted rows needed for rollback: restore a verified **pre-drop** backup into a separate DB first,
   apply the corrected migrations forward and reconcile post-backup writes before switching traffic.
   Do not overwrite a live database or invent password/token rows. No suitable backup => rollback
   to a legacy account-consuming release is **BLOCKED**; run the retired-consumer compatible release.
4. Schema drift/partial restore: verify columns, checks, FK and indexes against W0105 before routing
   an old consumer. `IF NOT EXISTS` intentionally does not rewrite an incompatible existing table.

The retired tables stay outside the runtime EF model. Retention/PII controls still apply to their
stored hashes and backups; do not attach real rows, dumps or connection strings to public evidence.

## Rollback and reproducible local evidence

Run from a **clean committed** candidate with Docker Linux containers and .NET 10 installed:

```powershell
pwsh -NoProfile -File tools/dev/Test-ExpandContract.ps1 -PreviousRef <previous-compatible-commit>
```

The script builds distinct previous/candidate SHAs, creates a disposable loopback PostgreSQL,
loads nine synthetic task outcomes through the previous API, takes `pg_dump -Fc`, restores a copy,
and starts both API binaries. Candidate readiness is `503/schema_behind` until migration completes.
Both then serve/read/write on the forward schema (the adjacent N/N+1 overlap condition). It stops
the candidate, restarts the previous binary (**rollback of N**, no automatic `Down`), then restarts
the candidate (**forward recovery**). Each active phase checks ready, seed `9/8` and confirm replay.
Source/copy task fingerprints must stay equal. Evidence includes commits, DLL hashes, dump hash,
schema history and PostgreSQL version. Arbitrary future releases must rerun this gate on their own pair.

`IT-SCHEMA-EXPAND-07` additionally clones the pre-drop W0118 PostgreSQL schema containing synthetic
account/session rows and checks upgrade, schema rollback and forward recovery. A second case models
the already-applied destructive migration: it proves shape repair, explicitly not recovery of lost rows.
The existing schema readiness and complete teardown/recreate tests remain mandatory.

Artifacts are kept in `ci-artifacts/expand-contract/<run>/`; the script removes only its own API
processes and dedicated Docker container. The previous-source worktree and synthetic dump remain
for inspection. Re-run from the candidate SHA named by evidence, not a later dirty working tree.

## Static and CI enforcement

- Mandatory `progressive_selftest` (`allow_failure: false`, validate stage) scans EF source for
  drops/renames/alterations, required columns without defaults and destructive/dynamic raw SQL.
  Its negative self-test includes comment-obfuscated SQL; .NET `UpOperations` also follows helpers.
- Mandatory .NET tests inspect all discovered migrations, including raw `SqlOperation`; W0122
  table-drop exemptions are removed. `Down` is not an expand operation and is checked by rehearsal.
- Two **pre-baseline** historical SQL migrations remain byte-pinned in
  `deploy/ci/migration-expand-baseline.json`: P2_1 removes intake defaults, P7_1 changes callback
  payload type. The supported legacy schema starts at W0118, after both. These pins are not proof
  those old historical rolling upgrades were safe; changed bytes or new destructive SQL fail.
- The guard is conservative source/operation analysis, not a PostgreSQL parser or a substitute
  for two-binary tests. Contract cleanup needs a separately designed release gate; do not add a
  drop-table exemption to make this expand gate green.

Local evidence: [W-0196](../evidence/W-0196/README.md). Hosted CI, Kubernetes canary/rollback and
real deployment consumer observation remain separate acceptance gates, not established here.

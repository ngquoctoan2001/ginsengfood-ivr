# W-0102 — Phase 3 §10 live evidence capture

| | |
| --- | --- |
| Work ID | `W-0102` · Origin `UNPLANNED` (closes the §10 gap left by `W-0100` §6) |
| Status | `EVIDENCE_SUBMITTED` (owner/reviewer acceptance pending) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

## 1. What was captured and why this form

`§10` of all four Phase 3 prompts asks for screenshots. Owner chose **option A**:
capture from the real stack without adding a browser-automation dependency to
the repo — the same reasoning `W-0097` used to keep a third-party skill out of
the tree (a new dev dependency widens the lockfile, the gitleaks surface and the
PII scan surface for something that is not product code).

So each screen is recorded as its **visible text**: `<script>` blocks and the RSC
flight payload are stripped before writing, which means the file holds what an
operator would see rather than the serialized data behind it. That distinction
matters — the payload repeats every string as data, so a naive dump would
"prove" things the screen never rendered.

| File | Screens |
| --- | --- |
| `docs/evidence/W-0025/live-screens-phase3.txt` | Signed-out redirect, shell + governance banner, not-found state |
| `docs/evidence/W-0026/live-screens-phase3.txt` | Dashboard (2 roles), call log, order-code filter, date filter, call detail (2 roles), error envelope |
| `docs/evidence/W-0027/live-screens-phase3.txt` | Config, integration, review, seed, roles |
| `docs/evidence/W-0028/live-screens-phase3.txt` | Reporting overview, A/B breakdown, suppressed slice, CSV export, both refusals |
| `docs/evidence/W-0099/live-screens-phase3.txt` | SIM roster across all three permission profiles |

## 2. The stack behind the capture

```text
admin-ui  next start          127.0.0.1:3007   NODE_ENV=production
Ivr.Api   dotnet run Release  127.0.0.1:5015   IVR_EXECUTION_MODE=MOCK
database  PostgreSQL 16       127.0.0.1:55433  docker-compose.dev.yml
```

**Port 5015, not 5005.** An older `Ivr.Api` — predating this work, without
`/analytics` or `/sim-channels` — was already holding 5005. It is the owner's
process, so it was left running and this capture used its own port. The first
capture attempt is what exposed it: the reporting screen rendered
`IVR_INTERNAL_ERROR` because a bare `404` from the stale build carries no error
envelope and the client degrades an unrecognised code rather than inventing one.

## 3. The fixture

18 jobs / 14 results, shaped so every screen has something real to show *and* the
k-anonymity threshold is actually exercised rather than asserted:

| Cohort | Rows | Purpose |
| --- | --- | --- |
| Golden Hour, variant `vA`, confirmed | 6 | Above `min_bucket_size=5` — survives |
| Golden Hour, variant `vA`, no-answer | 5 | Exactly at the threshold — survives |
| 24/7, variant `vB`, technical | 3 | **Below** the threshold — must be suppressed |
| Open jobs | 4 | One plain, one awaiting attempt 2, one eligibility-blocked, one near expiry |
| SIM channels | 3 | Idle, busy-with-a-call, disabled-and-quarantined |

Plus one capacity incident, one review item, one technical exception, and one
callback where Order Core answered `422` — so the detail screen has a real
failure to display truthfully rather than a happy path.

Two database constraints corrected the fixture during the run, which is worth
recording as proof they are live: `ck_ivr_confirmation_tasks_matrix` (Golden Hour
is the `ONLINE` programme, 24/7 is `COD`) and `ck_ivr_result_callbacks_hash`
(`^[A-F0-9]{64}$` — uppercase).

## 4. What the capture proves that a test could not

- **The export writes a real audit row.** The CSV response carried
  `x-ivr-audit-ref: 41dbc8b8-e7d6-47ba-a3f4-1a77e4e70738`, and that exact
  `audit_id` is in `ivr_audit_log` with `action=IVR_ANALYTICS_EXPORT`,
  `actor_id=AGT-ADMIN-01`, `reason=phase 3 evidence capture`.
- **Suppression is visible on screen, not just in a field.** The reporting page
  shows `Số nhóm bị ẩn vì dưới ngưỡng ẩn danh: 1 (k=5)` while the KPI band still
  reports `Tỉ lệ lỗi kỹ thuật 21.4%` — the aggregate rate over the whole scope is
  published, the 3-row bucket behind it is not.
- **Both export refusals happen against the real service**: `400
  IVR_MALFORMED_REQUEST` with no reason, `422 IVR_PII_POLICY_VIOLATION` for the
  24/7 slice where nothing survives suppression.
- **RBAC on the SIM controls is real, per row.** `AGT-OPS-01` holds only
  `IVR_SIM_DISABLE`: it sees `Tắt kênh` on both enabled channels and **nothing at
  all** on the disabled one, because enabling it is a permission it does not
  have. `AGT-VIEWER-01` sees the roster and no control anywhere.
- **The `W-0101` tiles carry real numbers**: `Tỷ lệ gọi thành công 42.9%`
  (6 of 14), `Chờ gọi lần 2 1`, `Bị chặn (eligibility) 1`,
  `Tỷ lệ kênh lỗi 33.3%` (1 of 3).
- **Freshness is measured, not stubbed**: `Mới`, lag `7m 55s`, 14 rows scanned,
  and the banner naming `OPERATIONAL_READ_MODEL, W-0055`.

## 5. The PII gate caught the capture twice — both false positives, both fixed

`scan-pii.sh` scans `docs/evidence/` (not the source tree), so console prose only
enters its scope when a capture copies it there. Two console strings used a
Vietnamese word that means both "street" and "path/route"; the address pattern in
`deploy/ci/pii-patterns.txt` matched the second sense.

| String | Change |
| --- | --- |
| `seed.loaderUnavailable` | "…creates no *route* to write data" reworded to "…opens no *way* to write data" |
| `state.notFoundBody` | "The *path* does not exist" reworded to "This URL address does not exist" |

The wording was changed rather than the pattern. `W-0076` chose blunt literal
byte alternations on purpose, for locale independence — narrowing them to admit
the non-address sense would trade a proven property for a cosmetic one.

The reusable rule, now recorded in the `UT-UI-SEED-PROD-03` test: console prose
that can reach an evidence capture must avoid the address vocabulary enumerated
in `deploy/ci/pii-patterns.txt`, even where the word carries another meaning.
This file itself hit the same gate on the draft that spelled those terms out,
which is the clearest possible demonstration that the scanner cannot distinguish
documentation about a pattern from the pattern.

Final run: `PII_SCAN_PASS files=259 skipped_binary=2`.

## 6. Process hygiene

Both processes started for this capture were stopped and verified gone
(`ui3007=000`, `api5015=000`); `dotnet build-server shutdown` was run. The
owner's API on 5005 was never touched and is still healthy (`api5005=200`). The
PostgreSQL container was already running before this work and was left running;
only IVR fixture tables were written.

## 7. Not claimed

- **These are text captures, not images.** That is what option A means. If a
  reviewer requires PNGs, that is option B and needs a decision about adding
  Playwright to the repo.
- The fixture is synthetic and MOCK-only: no real SIM, carrier, Sales endpoint or
  customer. `REAL_CUSTOMER_CALL_ALLOWED=NO` throughout.
- Owner and reviewer acceptance: **pending**.
- Hosted GitLab pipeline evidence: `NOT_RUN`.

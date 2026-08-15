# W-0098 — Analytics read API (`/analytics/summary|trend|breakdown|export`)

| | |
| --- | --- |
| Work ID | `W-0098` · Origin `UNPLANNED` · unblocks `P3-4` / `W-0028` |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

## 1. Why this exists, and what it deliberately is not

P3-4 declares `P10-4` a prerequisite and says the reporting console must not
compute KPI itself — it must read them from an analytics API. That API did not
exist: `W-0055` (P10-4, the BI pipeline) is `NOT_STARTED` and its own
prerequisite `W-0040` (P6-1) is `NOT_STARTED` too. There was no warehouse, no
fact table and no serve layer.

Two honest options existed: block P3-4 outright, or build the bounded serve
layer P3-4 actually consumes. This is the second, following the same precedent
as `W-0095` and `W-0096`, with one condition attached: **the API states its own
provenance on every response.**

```json
"data_quality": {
  "source": "OPERATIONAL_READ_MODEL",
  "warehouse_backed": false,
  "pipeline_work_id": "W-0055"
}
```

This is not the P10-4 pipeline and does not close `W-0055`. There is no ETL, no
fact/dimension schema, no idempotent replay and no warehouse. What exists is a
read-only aggregation over the operational tables, behind the contract the
pipeline can later serve unchanged.

## 2. What was added

| Operation | Permission | Purpose |
| --- | --- | --- |
| `GET /analytics/summary` | `IVR_QUEUE_VIEW` | Business KPI over the filtered scope plus the result-taxonomy split. |
| `GET /analytics/trend` | `IVR_QUEUE_VIEW` | Series per time bucket (`DAY`/`HOUR`) and programme. |
| `GET /analytics/breakdown` | `IVR_QUEUE_VIEW` | Split by `RESULT_TYPE`, `SCRIPT_VARIANT` (A/B) or `PROGRAM`. |
| `GET /analytics/export` | `IVR_QUEUE_VIEW` | Sanitized aggregate extract; `reason` mandatory, audited. |

Source: `src/Ivr.Api/Admin/AnalyticsContracts.cs`,
`src/Ivr.Api/Application/AnalyticsReadService.cs`.

KPI covered: confirm / cancel / no-answer / invalid-phone / technical /
operational-blocked rate (the full `DT-02` taxonomy), attempt-2 rate, average
time-to-final, total results, total final results, total call jobs and total
eligible tasks.

## 3. The three properties that carry the privacy claim

1. **Aggregate only, by construction.** Every field in the contract is a count,
   a rate or a dimension label. No task id, order code, phone, dial token or
   evidence ref is projected, so there is nothing downstream to mask. The export
   goes further: its rows are `string[]`, not objects, so there is no shape a
   customer field could travel in even by accident.

2. **k-anonymity is a server constant.** `MinBucketSize = 5` is a compile-time
   constant, never a request parameter. A caller can narrow a filter but cannot
   lower the threshold. `IT-ANALYTICS-05` passes `min_bucket_size=1` and asserts
   the answer is unchanged.

   A bucket below the threshold is **omitted**, not zeroed. A zeroed row reads as
   "no calls happened here", which is a different and false statement; the count
   of dropped buckets is reported instead as `suppressed_bucket_count`.

3. **A re-identifying export is refused, not emptied.** When data exists but
   nothing survives suppression, the export answers
   `422 IVR_PII_POLICY_VIOLATION`. An empty file would let an operator conclude
   "no data" from what is actually "too few people to publish".

## 4. Why the export is a GET

P3-4 §6.7 asks for an audited export with a mandatory reason. It is wired as a
`GET` with `reason` as a required query parameter rather than a `POST`, because
an export is a read that gets logged, not a state change. That choice keeps the
"no mutation surface" invariant intact: `IT-ANALYTICS-05` asserts all four
reporting routes answer `405` to a `POST`, the same guarantee `W-0096` gives the
back-office routes. The audit entry is written through the existing
`IAuditLogger` under action `IVR_ANALYTICS_EXPORT` with the resolved filter, row
count and suppressed count — IVR's own audit trail, which `D-14` requires, not a
write into Order Core, CRM or evidence.

## 5. Bounded scan, reported honestly

The service reads at most `MaxFactRows = 50_000` fact rows and sets
`data_quality.truncated` when the cap is reached, so a partial answer is never
presented as a complete one. This is a deliberate limit of an operational read
model; the warehouse is what removes it.

## 6. Contract governance

```text
oasdiff changelog draft.2 → current : 10 endpoints added, nothing else
oasdiff breaking  --fail-on WARN    : "No breaking changes to report"
```

Contract version `1.0.0-draft.4` → `1.0.0-draft.5`. Codegen regenerated
(NSwag 14.7.1), `contract-manifest.json` re-pinned, changelog and API-docs
portal rebuilt. `openapi:drift` reports `OPENAPI_HASHES_PINNED=3`,
`OPENAPI_HUMAN_DIFF_CURRENT=YES`. The Sales callback contract is untouched and
`TARGET_CONTRACT_V1=DRAFT` is unchanged.

## 7. Tests

`IT-ANALYTICS-01..05` in `tests/Ivr.IntegrationTests/AnalyticsApiTests.cs`,
against real PostgreSQL — **5/5 pass**.

The fixture is shaped so suppression is actually exercised rather than asserted:
11 Golden Hour results on script variant `vA` (6 confirmed, 5 no-answer) stay
above the threshold, while 2 results on the 24/7 variant `vB` fall below it.

| Test ID | Asserts |
| --- | --- |
| `IT-ANALYTICS-01` | All four routes return `200` for `IVR_QUEUE_VIEW` and `403 IVR_FORBIDDEN_CALLER` for an actor holding only `IVR_MANUAL_RETRY`. |
| `IT-ANALYTICS-02` | KPI match the seeded data exactly; attempt-2 counts only counted customer attempts, never the seeded technical retries (DT-02); `warehouse_backed=false`, `pipeline_work_id=W-0055`, `min_bucket_size=5`, `truncated=false`; the 2-result bucket is suppressed, not listed. |
| `IT-ANALYTICS-03` | The below-threshold bucket is absent from the trend series and from both breakdown dimensions, and each response reports `suppressed_bucket_count=1`. |
| `IT-ANALYTICS-04` | Missing and too-short reasons are rejected `400`; a valid export writes an `IVR_ANALYTICS_EXPORT` audit row with the reason and actor; the payload contains no order code, phone, masked phone, dial token or task id; a slice where nothing survives suppression answers `422 IVR_PII_POLICY_VIOLATION`. |
| `IT-ANALYTICS-05` | Every reporting route answers `405` to a `POST`; a caller-supplied `min_bucket_size=1` changes nothing. |

Full solution suite: **299/299** — 21 contract, 168 unit, 110 integration.
Build: 0 warnings, 0 errors.

Gates run: `openapi:lint` (0 warnings), `openapi:validate`, `openapi:drift`,
`docs:build`, `test:docs` (CT-DOC-01, UT-DOC-PII-03, boundary, links, topology),
`test:config`, `test:openapi-negative`, `scan-pii.sh`
(`PII_SCAN_PASS files=248 skipped_binary=2`).

## 8. A defect found by the tests

The first run returned `500` on every route. Cause: the fact projection filtered
and ordered on an already-constructed projection type, which EF cannot
translate. Fixed by filtering and ordering on the entities and projecting last.
Worth recording because it fails only at runtime — it compiles cleanly.

The fixture also proved the `ck_ivr_confirmation_tasks_matrix` constraint is
real: Golden Hour is the `ONLINE`-payment programme, 24/7 is `COD`, and a seed
that ignores that pairing is rejected by the database.

## 9. Not claimed

- **This is not P10-4.** `W-0055` stays `NOT_STARTED`: no ETL, no warehouse, no
  fact/dimension schema, no idempotent replay, no late-arrival handling. The
  `BI-PII-01`, `BI-KPI-02`, `BI-IDEMP-03` and `BI-QUALITY-04` tests belong to
  that work and were not run.
- Owner and reviewer acceptance: **pending**. `TESTS_PASS`, not `ACCEPTED`.
- No new permission was introduced; reporting reuses `IVR_QUEUE_VIEW` because
  permission management belongs to Permission Core (DF-01).
- Freshness is measured against the newest result in scope, not against a
  pipeline watermark — there is no pipeline to watermark yet.
- Hosted GitLab pipeline evidence: `NOT_RUN`.
- `dotnet format` reports pre-existing `ENDOFLINE` errors across ~50
  git-checked-out files on this Windows working tree (`core.autocrlf=true`); the
  files added here are LF-only and none appear in that list.

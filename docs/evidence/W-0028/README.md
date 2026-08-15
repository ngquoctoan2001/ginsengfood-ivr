# W-0028 / P3-4 — Reporting & analytics console

| | |
| --- | --- |
| Work ID | `W-0028` · Prompt `P3-4` |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Prereq | `W-0026` `TESTS_PASS`; `W-0098` `TESTS_PASS` (see §1 on `W-0055`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `IVR_ADAPTER_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

## 1. The prerequisite that does not exist

P3-4 lists `P10-4` (`W-0055`) as a prerequisite and forbids the console from
computing KPI itself. `W-0055` is `NOT_STARTED`, and so is its own prerequisite
`W-0040`. The analytics API was therefore built first as `W-0098`, a bounded
read-only serve layer behind the contract P10-4 can later fill.

The consequence is visible on the screen, not buried in a document: the
freshness banner states `Nguồn: đọc trực tiếp dữ liệu vận hành — CHƯA có
pipeline P10-4` together with `W-0055`, so nobody can mistake these figures for
BI-pipeline output.

## 2. What was built

| Path | Contents |
| --- | --- |
| `admin-ui/src/app/(console)/reports/page.tsx` | Reporting route, permission-guarded by the shared console shell |
| `admin-ui/src/app/(console)/reports/ReportFilters.tsx` | URL-encoded filter form (programme, result type, script variant, bucket, dimension, date range) |
| `admin-ui/src/app/(console)/reports/export/route.ts` | CSV route handler over the audited export operation |
| `admin-ui/src/components/reports/FreshnessBanner.tsx` | Data-quality / freshness / suppression / truncation banner |
| `admin-ui/src/components/reports/TrendChart.tsx` | Dependency-free trend bars plus the equivalent data table |
| `admin-ui/src/components/reports/BreakdownTable.tsx` | Result-taxonomy and per-dimension breakdown |
| `admin-ui/src/components/reports/ExportForm.tsx` | Export form with mandatory reason |
| `admin-ui/src/lib/analytics/{client,format}.ts` | Analytics API client and the formatting/CSV helpers |
| `admin-ui/src/i18n/vi.json` | 60 Vietnamese reporting strings |

`/reports` was added to the console nav under `IVR_QUEUE_VIEW`. Production
build: **16 routes + Proxy** (was 14).

## 3. Design decisions worth stating

**The console formats; it never derives.** Every rate, count and duration on the
screen comes from `data_quality`/`kpi`/`rows` as computed by the API. The only
client-side arithmetic is the bar width, which is a presentation of `total`.
This is P3-4 §4 taken literally.

**Historical reporting is separated from live operations in the UI, not just in
prose.** A notice at the top of the screen says the page analyses what already
happened and must not be used to decide dispatch, pointing at the P3-2 dashboard
instead. `E2E-UI-REPORT-05` asserts the screen renders no control that could
dispatch a call, pause the queue or change an order.

**No charting dependency.** The trend is CSS bars whose widths are percentages,
with the numbers repeated in a real table. The chart carries `aria-hidden` and
the table is what assistive technology reads, so the visual is additive rather
than load-bearing. The two programmes differ by pattern as well as shade, so the
comparison never rests on colour alone.

**Suppressed buckets are announced.** When the server drops a bucket below the
k-anonymity threshold, the banner says how many and at what `k`. A filtered view
that silently lost rows would read as complete.

**The export is a plain form to a server route.** `reason` is `required` with
`minLength=8` mirroring the server rule, the active filter travels as hidden
fields so the file matches the screen, and the CSV handler renders exactly the
columns and rows the API returned. A cell beginning `=`, `+`, `-` or `@` is
prefixed with an apostrophe — the file is opened in a spreadsheet, where a
leading `=` is a formula rather than a label.

**A refused export stays refused.** The `422 IVR_PII_POLICY_VIOLATION` from a
re-identifying slice is passed through as an error response, never downgraded to
an empty file.

## 4. Tests

| Test ID | File | Asserts |
| --- | --- | --- |
| `UT-UI-REPORT-01` | `tests/component/reports.test.tsx` | KPI cards render the API metric formatted as `%` and as `m/s`; an absent time-to-final shows `—` not `0`; the banner states source, freshness, suppression and truncation; a suppressed trend bucket simply is not rendered. |
| `UT-UI-REPORT-02` | `tests/unit/analytics-client.test.ts` | Every filter field maps to its contract query parameter; a date input widens to the whole day (`T00:00:00Z`/`T23:59:59Z`); empty filters are omitted rather than sent blank; the dimension reaches breakdown and export; no `min_bucket_size` is ever sent; a `422` export surfaces as `IvrApiError`. |
| `UT-UI-REPORT-PII-03` | `tests/component/reports.test.tsx` | The breakdown markup contains no phone / dial-token / order-code / address / payment / member-tier / health token; no reporting **label** names a customer field; the CSV contains only the server's aggregate cells and no 9+ digit run; a formula-shaped cell is neutralised. |
| `UT-UI-REPORT-EXPORT-04` | `tests/component/reports.test.tsx` | The reason field is `required` with the server's minimum; the form targets `/reports/export`; the notice states the audit log and the `k` threshold; the active filter is carried; there is no control that could alter the threshold. |
| `E2E-UI-REPORT-05` | `tests/e2e/reports-screen.test.ts` | Signed-in user sees KPI, trend, breakdown and the freshness banner; filters round-trip through the URL; the CSV download carries `X-Ivr-Audit-Ref` and `X-Ivr-Suppressed-Rows`; a missing reason answers `400` and a re-identifying slice `422`; an API denial renders `IVR_FORBIDDEN_CALLER` instead of numbers; a signed-out visitor is redirected to `/login`; no dispatch/order control is rendered. |

Suite: **16 files / 171 tests pass** (was 146). `eslint --max-warnings 0` clean,
`tsc --noEmit` clean, production build clean.

Note on the RBAC half of `E2E-UI-REPORT-05`: every actor in
`seed/agents.sample.json` holds `IVR_QUEUE_VIEW`, so no seeded identity lacks
it. Authorization is decided by the API in any case, so the denial is driven
where it actually happens — the stub answers `403 IVR_FORBIDDEN_CALLER` for one
actor — and the assertion is that the console shows the refusal and no figures.
The unauthenticated path is covered separately by the `/login` redirect.

## 5. Not claimed

- **`W-0055` is not closed.** The screen reads a bounded operational aggregation
  (`W-0098`), not a BI pipeline, and says so in the banner. `BI-PII-01`,
  `BI-KPI-02`, `BI-IDEMP-03` and `BI-QUALITY-04` were not run.
- Owner and reviewer acceptance: **pending**. `TESTS_PASS`, not `ACCEPTED`.
- Read-only throughout: no dispatch, no order-state control (D-02), no write to
  Order Core, CRM or evidence (D-14). The only write anywhere in the flow is
  IVR's own audit entry for an export.
- Drill-down stops at the aggregate bucket. There is no link from a reporting row
  into a call detail, because no row identifies a call.
- Accessibility and visual QA remain `W-0039` (P5-5); the component library
  choice remains `NEED_CONFIRMATION`.
- Cost-per-confirmed-order is still absent — there is no cost model until
  `W-0054`.
- Hosted GitLab pipeline evidence: `NOT_RUN`.

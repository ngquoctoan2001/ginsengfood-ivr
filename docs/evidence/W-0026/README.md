# W-0026 — P3-2 Dashboard, Call Log & Call Detail

| | |
| --- | --- |
| Work ID | `W-0026` · Prompt `P3-2` · Phase 3 |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| Prereq | `W-0025` (P3-1) `TESTS_PASS`; `W-0095` (read API) `TESTS_PASS` |

## 1. Screens

| Route | Spec | Contents |
| --- | --- | --- |
| `/dashboard` | `specs/ui/01` | Result-rate cards, queue panel, attempt panel, SIM pool panel, open capacity incidents, queue pause/resume. Program and date-range filter. |
| `/calls` | `specs/ui/02` | Masked, paginated call-job table with filters by order code, correlation id, program, queue status, result type and near-expiry. View only. |
| `/calls/[ivrCallJobId]` | `specs/ui/03` | task → eligibility → attempt timeline → result → Core callback, technical exceptions, review items, evidence/audit refs, correlation id. Technical retry and admin review. |

`/queue` from P3-1 now redirects to `/dashboard`: UI-01 owns the queue panel and
the pause/resume controls, and keeping both screens would have shown the same
admin actions in two places. `/` and post-login also land on `/dashboard`.

Source: `admin-ui/src/app/(console)/dashboard/**`,
`admin-ui/src/app/(console)/calls/**`, `admin-ui/src/components/data/**`.

## 2. Constraints as implemented

- **No order control (D-02).** The console has exactly two admin actions:
  technical retry and admin review. There is no confirm, cancel, force,
  reset-attempt or dispatch control anywhere, and `UT-UI-NOORDER-03` asserts the
  complete action set rather than the absence of one button.
  `recommended_core_action` is rendered under an explicit advisory note.
- **Masked only (D-05).** Every phone renders through `MaskedPhone`, which
  redacts anything that is not already masked. The table shows
  `order_code_short`; the full order code is a filter input the API accepts but
  never echoes into a row. DTMF is rendered as business semantics
  (`1 — xác nhận`), never as a raw payload. No recording player exists.
- **Figures come from the API (§9).** `MetricGrid` formats pre-computed values;
  no rate, count or ratio is derived in the browser.
- **RBAC hides, the server decides (DF-01).** Each action is wrapped in
  `RequirePermission`; Ivr.Api independently answers `403 IVR_FORBIDDEN_CALLER`
  to a caller without the permission (`IT-ADMIN-READ-01`, W-0025 evidence §4).
- **Truthful Core outcomes.** `core_http_status` and `core_response_code` are
  displayed as returned, including a `422 REJECTED_STALE`
  (`E2E-UI-DETAIL-02` asserts exactly that case).

## 3. Tests — §8 mapping

`npm --prefix admin-ui test` → **10 files, 83 tests, 83 passed** (10.9 s).

| Test ID | File | Count | Asserts |
| --- | --- | --- | --- |
| `E2E-UI-LOG-01` | `tests/e2e/console-screens.test.ts` | 3 | Real `next start` over HTTP: masked rows render, no raw MSISDN in the page, no `[đã ẩn]` redaction; `order_code` and `queue_status` reach the API and narrow the result while the full order code never appears in a table cell; dashboard figures render and the program filter propagates. |
| `E2E-UI-DETAIL-02` | `tests/e2e/console-screens.test.ts` | 3 | Both attempts in order, DTMF as semantics, result plus advisory framing, Core callback `422 REJECTED_STALE`, evidence and audit refs, correlation id, opaque `CONFIRMING` shown as text; unknown job renders a typed `IVR_NOT_FOUND` envelope instead of crashing; actions hidden from a viewer. |
| `UT-UI-NOORDER-03` | `tests/component/call-detail-actions.test.tsx` | 3 | The fullest role is offered exactly two actions; no control matches confirm/cancel-order, force or reset-attempt; the shipped Vietnamese copy states Order Core owns the state and offers no order transition. |
| `UT-UI-REVIEW-04` | `tests/component/call-detail-actions.test.tsx` | 4 | Review requires both a reason (≤500) and a resolution, carries the audited `review_item_id` and shows the audit notice; hidden entirely without `IVR_RESULT_REVIEW`; nothing rendered for a viewer; a non-retryable exception offers no retry. |

Carried forward from P3-1 and still green: `UT-UI-RBAC-01`, `UT-UI-ERR-02`,
`UT-UI-CORR-03`, `UT-UI-PII-04`, `E2E-UI-AUTH-05`, plus the contract-drift,
session and sign-in suites.

The e2e suites drive a real `next start` server; Ivr.Api is replaced at the HTTP
boundary by a stub speaking the same wire contract, so the front-end job needs no
.NET or PostgreSQL. Contract fidelity is held elsewhere — by `IT-ADMIN-READ-*`
against real PostgreSQL and by `tests/unit/contract-drift.test.ts`, which
compares the UI types against the OpenAPI file itself.

## 4. Commands

```text
npm --prefix admin-ui run lint       exit 0  (eslint --max-warnings 0)
npm --prefix admin-ui run typecheck  exit 0  (tsc --noEmit, strict)
npm --prefix admin-ui test           10 files / 83 tests / 83 pass
npm --prefix admin-ui run build      exit 0  9 routes + Proxy
```

## 5. Evidence — §10 mapping

Captured against the live stack: PostgreSQL on 55433, `Ivr.Api` on
`127.0.0.1:5005` in MOCK, admin-ui on `127.0.0.1:3005`. Fixture rows were seeded
directly into the database (two jobs across both programs, two attempts, a
technical exception, a confirmed result, an acknowledged callback and an open
review item). Transcript: [`live-screens.txt`](live-screens.txt).

| §10 item | Where |
| --- | --- |
| Dashboard | `live-screens.txt` §Dashboard — all four rate cards, queue panel, SIM panel, pause/resume |
| Call log | `live-screens.txt` §Call log — `GF-DEMO-GH` / `GF-DEMO-247`, masked phones, near-expiry badge |
| Call detail | `live-screens.txt` §Call detail — both attempts, `1 — xác nhận`, result, `CORE_REVALIDATE_AND_CONTINUE`, callback `ACCEPTED`, `TECH-DEMO-GH`, `REVIEW-DEMO-GH`, evidence + audit refs, `corr-demo-gh` |
| Masked phone proof | Only `84xxxxx0065` / `84xxxxx0247` appear; the filter capture shows the table cell carrying `GF-DEMO-GH` while the query used `GF-ORDER-DEMO-GH-FULL` |
| Review-action audit record | `docs/evidence/W-0025/` §6 holds a real audited mutation (`ivr_admin_actions` + `ivr_audit_log` with the UI-generated correlation id); the P3-2 actions use the same `AdminActionDialog` and API client |
| Error/empty states | `E2E-UI-DETAIL-02` asserts the `IVR_NOT_FOUND` envelope; `E2E-UI-AUTH-05` asserts `IVR_INTERNAL_ERROR` when Ivr.Api is unreachable; `EmptyState` renders on a zero-row page |

### RBAC observed live

```text
actor          retry-trigger  review-trigger  resolution-input  reviewItemId  technicalExceptionId
AGT-VIEWER-01  0              0               0                 0             0
AGT-OPS-01     1              0               0                 0             1
AGT-ADMIN-01   0              1               1                 1             0
```

Each role receives exactly the action forms its permissions allow, hidden target
ids included, while the read-only timeline stays visible to all three. AdminIM
shows no retry trigger because `seed/agents.sample.json` does not grant it
`IVR_MANUAL_RETRY` — the console reflects the seeded matrix rather than assuming
"admin can do everything".

## 6. Not claimed

- Owner and reviewer acceptance: **pending**. `TESTS_PASS`, not `ACCEPTED`.
- `cost_per_confirmed_order` from `specs/ui/01` is **not** shown. No cost model
  exists (`P10-3` / `W-0054`, "measured data needed") and a fabricated figure on
  an operations dashboard would be worse than an absent one.
- CSV export (`specs/ui/02`, optional) not built.
- Auto-refresh (P3-2 §6.1, "nhẹ") not built: the screens are request-time
  rendered and refresh on navigation. A polling interval is an ops decision that
  should come with the observability work (`P6-1` / `W-0040`).
- Component library still `NEED_CONFIRMATION` — no UI dependency was added.
- Hosted GitLab pipeline evidence: `NOT_RUN`.
- Browser-level accessibility, visual and i18n QA: `NOT_RUN`, owned by `P5-5`
  (`W-0039`). No screenshot images are attached — the preview browser in this
  environment does not composite frames, so rendered output is recorded as text
  transcripts and DOM assertions.
- Screens owned by `P3-3` (`W-0027`) — menu config, integration status,
  seed/mock management, role matrix — are not built here.
- No real customer contacted, no SIM enabled, no order state changed, no Sales
  write, no recording.

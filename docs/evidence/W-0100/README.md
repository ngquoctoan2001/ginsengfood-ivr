# W-0100 — Phase 3 audit hygiene remediation

| | |
| --- | --- |
| Work ID | `W-0100` · Origin `RED_TEAM_REMEDIATION` (Phase 3 audit findings #2–#7, #9) |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

Findings raised by the Phase 3 review of `W-0025..W-0028` and `W-0095..W-0098`.
The behavioural gap (finding #1, the SIM channel surface) is `W-0099`; this is
everything else.

## 1. The contract-drift guard had fallen behind by ten endpoints

`UT-UI-CONTRACT-06` contained an assertion named *"declares every admin path the
UI can reach"* that checked three paths. The console reached twelve — and
`/queue`, one of the three, was no longer reached at all. The guard passed no
matter what `W-0095`, `W-0096` or `W-0098` added.

Rewritten so it cannot fall behind again:

- **Paths are derived from the client source, not hand-listed.** The test scans
  `lib/api/admin.ts` and `lib/analytics/client.ts` for `path:` literals and
  compares them to the declared OpenAPI paths. A template literal is scanned
  with a depth-tracking walker rather than regex-replaced, because
  `${buildQuery({ a, b })}` contains braces of its own; each interpolation
  becomes `*`, a trailing `*` is a query builder and is dropped, an interior one
  is a route parameter and is matched against `{param}`. A new client function is
  covered the moment it is written.
- **The privacy decisions are asserted against the contract.** A new case walks
  the ten schemas the reporting and SIM screens read and fails if any property is
  a customer identity (`phone_ref`, `sim_number_ref`, `dial_token`, `order_code`,
  `payment_method`, lease fields, …). This locks in what `W-0095/0096/0098/0099`
  each decided in prose.
- Added required-key mirrors for `IvrSimChannel` and `IvrAnalyticsDataQuality`.

**The new guard found a real bug in its own first run**, in the opposite
direction: a substring rule flagged `IvrAnalyticsTrendBucket.invalid_phone`,
which is a *count* of results whose type is `IVR_INVALID_PHONE_FINAL` and
contains no number. The rule was changed to exact names, with the reason written
down — a substring rule would have kept flagging a legitimate counter while still
missing a field that spells the identity differently.

## 2. A test whose comment claimed more than its assertion

`UT-UI-ROLE-04` had an `it()` titled *"covers every permission in the screen
mapping"* whose body asserted `expect(typeof permission).toBe("string")`. The
comment said it guarded the mapping's exhaustiveness; exhaustiveness is actually
guarded by `Record<IvrPermission, string>` at compile time, and the assertion
guarded nothing.

Replaced with one that reads the roles page and checks each permission maps to a
screen that exists — including that `IVR_QUEUE_VIEW` names the reporting screen
and that the SIM permissions no longer promise a future screen.

## 3. Dead code and stale copy left by earlier changes

| Removed / fixed | Origin |
| --- | --- |
| `getQueue()` client function — zero callers | `W-0026` folded `/queue` into `/dashboard`; the wrapper stayed. It broke the rule `W-0025` set for itself: no operation ships without a caller and a test. |
| `IvrQueueProjection` type and its drift assertion | Only reachable from the test that asserted it. |
| 10 orphaned i18n strings (`nav.queue`, `queue.title/subtitle/pendingJobs/activeAttempts/enabledChannels/openHoldIncidents/projectedAt`, `detail.timelineTitle`, `detail.attemptStatus`) | Same fold, plus two call-detail keys superseded by `detail.attemptsTitle`. |
| `/reports` missing from the roles screen mapping and from `admin-ui/README.md` | `W-0028`. No test caught either; the new `UT-UI-ROLE-04` now does. |
| `IVR_SIM_ENABLE/DISABLE` mapping text "màn cấu hình kênh, P3-3 sau" | Pointed at a screen that never existed. Now names the real control (`W-0099`). |
| `P3-3` §12 "Kết thúc Phase 3" | False since `P3-4` exists. Corrected with a dated note rather than a silent edit. |

An i18n sweep after the change reports zero dead strings; the five the scanner
still lists are reached through `t(state.messageKey)` and were verified by hand.

## 4. Two capability gaps closed

**The job-status filter had no control.** `GET /call-jobs` accepts `status` and
`queue_status`; the call-log page already read `status` from the URL, passed it
to the API and preserved it across pagination — but `CallLogFilters` rendered
only `queue_status`, so the filter was unreachable and `calls.filterStatus` sat
unused. The control was added; the two are different axes (a job can be `CLOSED`
while its queue status records how it left the queue) and both now have one.

**An admin action gave no confirmation.** `action.succeeded` existed but was
never rendered: `AdminActionDialog` closed itself on success via an effect, so
the operator was left with nothing to quote. The dialog now stays open on
success and shows the admin action id and correlation id — which is what a ticket
or an audit lookup needs — with the primary button replaced by a close button.

## 5. Verification

| Gate | Result |
| --- | --- |
| .NET | **301/301** (21 contract, 168 unit, 112 integration), build 0 warnings |
| admin-ui | **180/180** across 17 files (was 171/16); lint `--max-warnings 0`, `tsc --noEmit` clean; build 16 routes |
| Contract | `openapi:lint` 0 warnings, `validate`, `drift` (`HASHES_PINNED=3`, `HUMAN_DIFF_CURRENT=YES`), oasdiff no breaking change |
| Docs / config / privacy | `test:docs` (CT-DOC-01, UT-DOC-PII-03, boundary, links, topology), `test:config`, `test:openapi-negative`, `scan-pii.sh` (`files=250`) |

The dashboard E2E failed on the first run after `W-0099` added a second read to
that page — the stub API answered `404` and the screen correctly rendered the
error envelope instead of numbers. That is the guard working; the stub was
extended rather than the assertion weakened.

## 6. Not fixed here, and why

- **Screenshot evidence (`§10` of all four Phase 3 prompts) is still missing.**
  No Phase 3 work has produced images: `W-0025` has two text captures, `W-0026`
  one, `W-0027`/`W-0028` none. Capturing them needs the live stack and seeded
  data above the k-anonymity threshold; it is a separate, owner-visible task.
- `UT-UI-SEED-PROD-03` remains a message-level assertion. The prod-guard
  *behaviour* is covered by the `E2E-UI-REVIEW-05` suite, which boots a second
  server with `IVR_ENVIRONMENT_LABEL=production`; duplicating it as a component
  test would add no signal.
- `dotnet format` still reports pre-existing `ENDOFLINE` errors across ~50
  git-checked-out files on this Windows working tree (`core.autocrlf=true`).
  Untouched: every file added or edited here is LF-only.

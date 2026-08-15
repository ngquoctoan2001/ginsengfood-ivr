# W-0101 — Phase 3 spec conformance (UI-01 / UI-02 / UI-03)

| | |
| --- | --- |
| Work ID | `W-0101` · Origin `RED_TEAM_REMEDIATION` (Phase 3 spec sweep) |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

## 1. Why this exists

`W-0099` and `W-0100` closed the findings from reading the Phase 3 *prompts*.
This closes what came out of reading the *UI specs* line by line — `specs/ui/01`,
`02` and `03` each asked for elements that had no field behind them.

The pattern is the same in all three cases: the screen was built against what the
API happened to return, not against what the spec listed.

## 2. UI-01 — four dashboard tiles with nothing behind them

`specs/ui/01` lists eight KPI cards and a five-item queue panel. Three KPI cards
and two queue items had no corresponding field anywhere.

| Spec element | Added as | Definition |
| --- | --- | --- |
| `call_success_rate` | `results.call_success_rate` | Share of results where IVR reached the customer and got an input: `IVR_CONFIRMED + IVR_CUSTOMER_CANCELLED + IVR_WRONG_INPUT`. **A cancel counts — the call succeeded, the answer was no.** |
| `sim_failure_rate` | `sim.failure_rate` | Channels in `HEALTH_FAILED` over the whole pool. |
| queue `blocked` | `queue.blocked` | Open jobs the eligibility gate refused. |
| queue `attempt2-due` | `queue.attempt_two_pending` | Open jobs holding exactly one counted customer attempt with one left. |
| `cost_per_confirmed_order` | — | Still absent. There is no cost model until `W-0054`; inventing one would be worse than the gap. |

Two naming decisions are deliberate:

- **`attempt_two_pending`, not `attempt_two_due`.** Due-ness needs each job's own
  offset schedule (`T0At + offsets[1]`), which means parsing a JSON array per
  open job inside a dashboard aggregate. The tile reports what it can compute
  exactly, under a name that does not overclaim.
- **`call_success_rate` carries its formula in the contract description and is
  flagged pending owner confirmation.** `specs/ui/01` names the tile but never
  defines it; the reading above is stated openly rather than buried in code.

## 3. UI-02 — a filter the API accepted and the screen could not send

`specs/ui/02` lists `date` among the call-log filters. `GET /call-jobs` had
accepted `from`/`to` since `W-0095`, and the page already carried them through
pagination — but `CallJobQuery` did not declare them and the filter form had no
inputs, so the capability was unreachable.

Added: date inputs, client fields, and the day→instant widening that the rest of
the console already uses (`T00:00:00Z` / `T23:59:59Z`), with the range preserved
across pages.

Not added: an `updated_at` column. The spec's table lists it; no such field
exists on the job projection, and the timestamps that do exist (`created_at`,
`expires_at`, `closed_at`) are already shown.

## 4. UI-03 — the sellable snapshot was collapsed to a timestamp

`specs/ui/03` puts a per-line snapshot in the trace —
`sellable_status[] (per-line: sku/decision/recall_hold/sale_lock)`. The detail
projection carried only `sellable_captured_at`, so an operator could see *when*
Order Core decided but never *what* it decided.

The data was already stored (`ConfirmationTaskEntity.SellableStatusJson`) and the
schema already existed in the contract (`SellableStatusLine`, used by the intake
DTO); only the projection was missing. Now parsed and rendered as a table.

Three properties of the parser:

- **Read back exactly as captured.** IVR never re-evaluates sellability (DO-02);
  this is a display of someone else's decision.
- **A malformed or absent snapshot yields an empty list**, not a failed screen —
  the same lesson `W-0096` learned when one invalid script template took down the
  whole catalogue.
- **Three-state flags.** `✓` set, `–` not set, `—` not captured. Collapsing "not
  captured" into "false" would assert something Order Core never said.

Not added: `sale_lock_id` / `recall_case_id`. The spec mentions them among the
evidence refs, but no such field is stored or contracted anywhere in IVR.

## 5. Contract governance

```text
oasdiff changelog draft.2 → current : 11 endpoints added, nothing else
oasdiff breaking  --fail-on WARN    : "No breaking changes to report"
```

`1.0.0-draft.6` → `1.0.0-draft.7`. **No new operation** — five response fields on
three existing ones. Codegen regenerated, manifest re-pinned, changelog and
portal rebuilt; `openapi:drift` reports `OPENAPI_HASHES_PINNED=3`,
`OPENAPI_HUMAN_DIFF_CURRENT=YES`.

## 6. Verification

| Gate | Result |
| --- | --- |
| .NET | **302/302** (21 contract, 168 unit, 113 integration), build 0 warnings |
| admin-ui | **181/181** across 17 files; lint `--max-warnings 0`, `tsc --noEmit` clean; build 16 routes |
| Contract | lint 0 warnings, validate, drift, oasdiff no breaking change |
| Docs / config / privacy | `test:docs`, `test:config`, `test:openapi-negative`, `scan-pii.sh` (`files=252`) |

New `IT-ADMIN-READ-09` asserts all four dashboard tiles are present and in range,
that `call_success_rate ≥ confirm_rate + cancel_rate` (the invariant that follows
from the definition), and that the seeded sellable line round-trips with
`sale_lock=true` and `recall_hold=false`. `E2E-UI-LOG-01` gained a date-range
case asserting the day is widened at both ends; `E2E-UI-DETAIL-02` asserts the
sellable table renders.

## 7. Not claimed

- `call_success_rate`'s formula is **pending owner confirmation**. It is stated
  in the contract description and in this file; if Product means something else
  by "call success", the definition changes and the tests change with it.
- `cost_per_confirmed_order` (`W-0054`), `updated_at` and
  `sale_lock_id`/`recall_case_id` remain absent, with reasons in §2–§4.
- Owner and reviewer acceptance: **pending**. `TESTS_PASS`, not `ACCEPTED`.
- Hosted GitLab pipeline evidence: `NOT_RUN`.
- Screenshot evidence (`§10` of all four Phase 3 prompts) is still outstanding —
  see `docs/evidence/W-0100/` §6.

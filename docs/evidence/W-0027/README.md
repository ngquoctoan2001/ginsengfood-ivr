# W-0027 — P3-3 Config, Integration Status, Review Queue, Seed/Mock & Roles

| | |
| --- | --- |
| Work ID | `W-0027` · Prompt `P3-3` · Phase 3 |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| Prereq | `W-0026` (P3-2) and `W-0096` (back-office read API) both `TESTS_PASS` |

## 1. Screens

| Route | Spec | Contents |
| --- | --- | --- |
| `/config` | `specs/ui/04` | Script versions with approval matrix and missing approvals, template validity, DTMF map with KEY_9 disabled, allowed and prohibited variables, the `OD-V1-15` production lock. Read-only. |
| `/integration` | `specs/ui/05` | Runtime flags, a dependency card per downstream system, recent fail-closed events. View only, no override. |
| `/review` | `specs/ui/06` | Human review queue, each row linking into its call detail where the two actions live. |
| `/seed` | `specs/ui/07` | Adapter mode, REAL lock, available integration-status profiles. Locked outside a known non-production environment. |
| `/roles` | `specs/ui/08` | Role → permission table, permission → screen matrix, current session. Reference only. |

Console now has 14 routes. Every nav entry is gated on `IVR_QUEUE_VIEW`.

## 2. Two places where the prompt and its source specs disagree

**Callback replay (§6.4, `E2E-UI-REPLAY-05`).** The prompt describes a replay
control with the same idempotency key. `specs/ui/06` — which P3-3 §3 lists as a
source spec to read first — defines UI-06 as the review queue with
`POST /admin-reviews` and `POST /technical-retries`, and says nothing about
replay. No replay operation exists anywhere in the API, and callback re-delivery
is owned by the outbox and its circuit breaker (P2-6 / `W-0023`).

Followed the spec. The review queue is built; the console offers **no** resend or
replay control, and says so on screen. `E2E-UI-REPLAY-05` is therefore delivered
as **`E2E-UI-REVIEW-05`**, which asserts the queue works end to end *and* that no
such control is rendered on any of the five screens.

**Role assignment (§6.5, `UT-UI-ROLE-04`).** The prompt asks for assign/revoke
with audit. DF-01 places permission creation and management in Permission Core,
and `specs/ui/08` is a matrix specification. Building a write path here would
create a second source of truth for authorization. `/roles` is a reference matrix;
`UT-UI-ROLE-04` asserts the absence of an assignment control and the presence of
the statement explaining where assignment does live.

Both substitutions are visible in the shipped UI copy, not just in this document.

## 3. Constraints as implemented

- **No REAL mode from the UI.** There is no adapter-mode control at all. The
  screen states the two gates that would have to close first (DT-01 purchase,
  DF-03 release).
- **Seed/mock is production-locked by an allowlist**, not by `label !== "production"`.
  An unfamiliar or misspelled environment label locks the screen. This also fixed
  a bug found while writing the e2e: the first version keyed off
  `NODE_ENV === "production"`, which is true for *every* `next start` — it would
  have locked staging and lab too.
- **No unapproved script can be presented as usable.** Each version shows its
  missing approvals, and `production_target_v1_fields_approved=false` is rendered
  as an explicit lock citing `OD-V1-15`.
- **KEY_9 is read-only NOT_ENABLED** (AS-07), stated both in the DTMF table and
  in a note saying it cannot be enabled from the interface.
- **Fail-closed is labelled, never asserted.** `DOWN` and `READY_503` carry a
  `fail-closed` badge; `NOT_WIRED` carries neither a green state nor a badge, and
  a page-level warning cites `W-0040`. Nothing in this work verifies fail-closed
  behaviour and the evidence says so.

## 4. Tests — §8 mapping

`npm --prefix admin-ui test` → **12 files, 102 tests, 102 passed**.

| Test ID | File | Count | Asserts |
| --- | --- | --- | --- |
| `UT-UI-SCRIPT-01` | `tests/component/back-office.test.tsx` | 3 | Approved and not-approved badges exist; no lifecycle action label is defined at all; the read-only notice cites OD-V1-15; KEY_9 copy says NOT_ENABLED and cannot be enabled, citing AS-07; the production lock names `ProductionTargetV1FieldsApproved=NO`. |
| `UT-UI-HEALTH-02` | `tests/component/back-office.test.tsx` | 4 | `DOWN` and `READY_503` both render a `fail-closed` badge; `UP` does not; `NOT_WIRED` renders neither the badge nor an observed marker; the page warning cites W-0040 and the screen offers no override. |
| `UT-UI-SEED-PROD-03` | `tests/component/back-office.test.tsx` | 2 | Production lock and REAL-mode lock are separate statements citing DT-01 and DF-03; the copy states no seed write path exists from the console. |
| `UT-UI-ROLE-04` | `tests/component/back-office.test.tsx` | 3 | The matrix states Permission Core owns assignment and that no assign/revoke control exists; the permission vocabulary is covered; `IVR_RUNTIME_GATE_ADMIN` is held by no role. **Superseded 2026-08-22 (`OD-V1-20`):** the third case now asserts the label names the gates the permission reaches, since `Admin` holds it. |
| `E2E-UI-REVIEW-05` | `tests/e2e/back-office-screens.test.ts` | 6 | Real `next start` over HTTP against a stub API: review queue lists items and links to `/calls/{id}`; **no replay or resend control** appears in the rendered markup of any of the five screens; script approval state, KEY_9 and the OD-V1-15 lock render; a `READY_503` dependency is labelled fail-closed while `NOT_WIRED` is not; seed/mock is open in `dev` and locked in `production`; roles render as a matrix with no assignment control. |

The production-lock case runs a **second** `next start` server with
`IVR_ENVIRONMENT_LABEL=production`, so the guard is exercised as deployed rather
than mocked.

## 5. Commands

```text
npm --prefix admin-ui run lint       exit 0  (eslint --max-warnings 0)
npm --prefix admin-ui run typecheck  exit 0  (tsc --noEmit, strict)
npm --prefix admin-ui test           12 files / 102 tests / 102 pass
npm --prefix admin-ui run build      exit 0  14 routes + Proxy
```

Backend behind these screens: `docs/evidence/W-0096/` — 5/5 `IT-ADMIN-CONFIG-*`,
full solution 294/294.

## 6. Evidence — §10 mapping

| §10 item | Where |
| --- | --- |
| Four screens | `E2E-UI-REVIEW-05` drives all five routes against a real server and asserts their distinctive content |
| Prod-guard demo | `E2E-UI-REVIEW-05` case 5 — the same page open in `dev` and locked in `production`, from two live servers |
| Health fail-closed badge | `UT-UI-HEALTH-02` (component) and `E2E-UI-REVIEW-05` case 4 (`READY_503` labelled, `NOT_WIRED` not) |
| Role-change audit | **Not applicable.** No role change is possible from this console; see §2 |
| Replay same-key proof | **Not applicable.** No replay control exists; see §2. `E2E-UI-REVIEW-05` case 2 asserts its absence |

No screenshot images: the preview browser in this environment does not composite
frames, so rendered output is recorded as HTTP-level assertions over real server
markup rather than images.

## 7. Not claimed

- Owner and reviewer acceptance: **pending**. `TESTS_PASS`, not `ACCEPTED`.
- **Fail-closed behaviour is not verified.** Dependency probing is `W-0040`.
- Script lifecycle actions, seed loading, scenario dry-run, integration-status
  profile switching and permission assignment are **not implemented** — see
  `docs/evidence/W-0096/` §5 for why each one was left out.
- Component library still `NEED_CONFIRMATION`; no UI dependency added.
- Hosted GitLab pipeline evidence: `NOT_RUN`.
- Accessibility, visual and i18n QA: `NOT_RUN`, owned by `P5-5` (`W-0039`).
- No real customer contacted, no SIM enabled, no order state changed, no adapter
  mode changed, no script approved.

## 8. Phase 3 status

P3-1, P3-2 and P3-3 are `TESTS_PASS`. P3-4 (`W-0028`, privacy-safe reporting)
remains `NOT_STARTED`, so Phase 3 is not complete. The admin console is complete
for the screens these three prompts define, in MOCK, behind read-only APIs for
everything the console does not own.

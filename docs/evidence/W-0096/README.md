# W-0096 — Back-office read API (`/scripts`, `/integration-status`, `/review-items`)

| | |
| --- | --- |
| Work ID | `W-0096` · Origin `UNPLANNED` · unblocks `P3-3` / `W-0027` |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

## 1. Why this exists

P3-3 asks for four back-office screens. Before this work the API had no
operation behind any of them:

- Script config: the lifecycle domain exists (`IScriptContentManager`, P2-7 /
  `W-0024`) but **zero endpoints** expose it.
- Integration status: no health aggregation of any kind.
- Review queue (`specs/ui/06`): the two actions exist from P2-8, but there was
  no way to *list* the items they act on.
- Seed/mock and role assignment: see §5 — those are deliberately not built.

## 2. What was added

| Operation | Permission | Purpose |
| --- | --- | --- |
| `GET /scripts` | `IVR_QUEUE_VIEW` | Script versions, approval matrix, missing approvals, allowed/prohibited variables, DTMF map, the `OD-V1-15` production lock. |
| `GET /integration-status` | `IVR_QUEUE_VIEW` | Runtime flags plus a dependency card per downstream system, with unprobed ones marked `NOT_WIRED`. |
| `GET /review-items` | `IVR_QUEUE_VIEW` | Paginated human review queue, each item resolved back to its call job. |

Source: `src/Ivr.Api/Admin/AdminConfigContracts.cs`,
`src/Ivr.Api/Application/AdminConfigReadService.cs`.

Three design points:

- **Read-only.** No mutation was added. `IT-ADMIN-CONFIG-05` asserts each route
  answers `405` to a POST.
- **Unprobed is not healthy.** `dependency_probing_available` is `false` and the
  four unwired dependencies report `NOT_WIRED` with `observed=false`. The P3-3
  prompt's own header says the `ready=503` signal is not real until P6-1
  (`W-0040`); painting those cards green would have manufactured a fail-closed
  claim that has not been verified.
- **A stored template that no longer validates is reported, not thrown.**
  See §4.

## 3. Contract governance

```text
oasdiff changelog draft.2 → current : 6 endpoints added, nothing else
oasdiff breaking  --fail-on WARN    : "No breaking changes to report"
```

Contract version `1.0.0-draft.3` → `1.0.0-draft.4`. Codegen regenerated,
`contract-manifest.json` re-pinned, changelog and API-docs portal rebuilt;
`openapi:drift` reports `OPENAPI_HASHES_PINNED=3`,
`OPENAPI_HUMAN_DIFF_CURRENT=YES`. The Sales callback contract is untouched.

## 4. A crash found by the tests, fixed in the service

The first run of `IT-ADMIN-CONFIG-01` returned `500` on `/scripts`. Cause:
`TargetV1SpeechPolicy.UsesProductionDecisionFields` calls `ValidateTemplate`,
which **throws** when a template does not satisfy the Target V1 whitelist — so a
single stored draft with an outdated template took down the whole catalogue.

That is a real robustness bug, not just a bad fixture: a draft or retired version
is exactly where a non-conforming template is expected to live, and an operator
needs to see it. Fixed by catching the validation failure and reporting
`template_valid: false` on the row. The fixture now deliberately seeds one valid
and one invalid version, and `IT-ADMIN-CONFIG-02` asserts both render.

## 5. Deliberately not built

| Asked for | Why not |
| --- | --- |
| Script lifecycle mutations (`§6.1`) | Approval is an owner action under `OD-V1-15` (`OWNER_DECISION_REQUIRED`), not a console button. The domain already enforces four-eyes and no-self-approval; exposing it is a separate, owner-sanctioned step. Confirmed read-only with the IVR dev on 2026-08-15. |
| Seed loader / scenario runner / profile switch (`§6.3`) | Would be a brand-new write path from a browser into the database, with no API and no domain service behind it. Confirmed read-only with the IVR dev on 2026-08-15. |
| Permission assignment (`§6.5`) | DF-01 puts permission management in Permission Core. A write path here would create a second source of truth for authorization. |
| Callback replay (`§6.4`) | No such operation exists, and `specs/ui/06` — the source spec P3-3 §3 lists first — defines UI-06 as the review queue, not a replay screen. See `docs/evidence/W-0027/` §2. |

## 6. Tests

`IT-ADMIN-CONFIG-01..05` in `tests/Ivr.IntegrationTests/AdminConfigApiTests.cs`,
against real PostgreSQL — **5/5 pass**.

| Test ID | Asserts |
| --- | --- |
| `IT-ADMIN-CONFIG-01` | All three routes return `200` for `IVR_QUEUE_VIEW` and `403 IVR_FORBIDDEN_CALLER` for an actor holding only `IVR_MANUAL_RETRY`. |
| `IT-ADMIN-CONFIG-02` | `OD-V1-15` lock reported false; KEY_9 present and disabled while 1 and 0 are enabled; prohibited variables listed; an approved version has no missing approvals; a draft lists all four and reports `template_valid=false`. |
| `IT-ADMIN-CONFIG-03` | `dependency_probing_available=false`; the four unwired dependencies are `NOT_WIRED` with `observed=false` and a stated fail-closed effect; the SIM pool IVR does own reports `UP` with `observed=true`. |
| `IT-ADMIN-CONFIG-04` | A review item resolves through its result back to the call job with `order_code_short` and `result_type`; the payload carries no `phone_ref`, `dial_token` or full order code; a status filter narrows correctly. |
| `IT-ADMIN-CONFIG-05` | Every back-office route answers `405` to a POST — no mutation surface exists. |

Full solution suite: **294/294** — 21 contract, 168 unit, 105 integration.
Build: 0 warnings.

## 7. Not claimed

- Owner and reviewer acceptance: **pending**. `TESTS_PASS`, not `ACCEPTED`.
- **No fail-closed behaviour is verified by this work.** The dependency cards
  report configuration and locally observable state only. Real probing is
  `P6-1` / `W-0040`.
- `TARGET_CONTRACT_V1=DRAFT` unchanged; no external gate moved.
- No new permission introduced; `IVR_RUNTIME_GATE_ADMIN` still ungranted.
- Hosted GitLab pipeline evidence: `NOT_RUN`.

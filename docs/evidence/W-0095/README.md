# W-0095 — Admin read API (`GET /dashboard`, `/call-jobs`, `/call-jobs/{id}/detail`)

| | |
| --- | --- |
| Work ID | `W-0095` · Origin `UNPLANNED` · unblocks `P3-2` / `W-0026` |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Baseline | `34340cca66b3d0bf59083f11a382d4f46ebe181b` (`main`) |
| Date | 2026-08-15 |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

## 1. Why this exists

P3-2 needs three screens. The admin surface delivered by P2-8 (`W-0065`) has
exactly **one** read operation — `GET /queue`, five scalar counters — and six
POST mutations. There is no list operation and no aggregate.

Concretely, before this work:

- The dashboard (`specs/ui/01`) asks for eight KPI cards, a per-status queue
  breakdown, a SIM pool panel and an incident list. `GET /queue` supplies none
  of that.
- The call log (`specs/ui/02`) has no backing operation at all.
- The call detail (`specs/ui/03`) could only reach the **service-token-only**
  `GET /call-jobs/{id}`, which returns a single flat job row — and
  `specs/ui/08` §4 forbids the browser from touching internal lifecycle APIs.
- `POST /technical-retries` and `POST /admin-reviews` were unreachable from any
  browser-facing surface, because `technical_exception_id`, `target_attempt_id`
  and `review_item_id` could not be obtained.

P3-2 §5 authorises adding the missing endpoints (`Ivr.Api — bổ sung nếu thiếu`)
and §9 requires the dashboard figures to come from the API rather than be
computed in the client. Both point the same way, so the endpoints were added
rather than mocked.

## 2. What was added

| Operation | Permission | Purpose |
| --- | --- | --- |
| `GET /dashboard` | `IVR_QUEUE_VIEW` | Queue, result-rate, attempt and SIM aggregates plus open incidents. Optional `program`, `from`, `to`. |
| `GET /call-jobs` | `IVR_QUEUE_VIEW` | Masked, paginated list. Filters: program, status, queue_status, result_type, order_code, correlation_id, near_expiry, from, to. |
| `GET /call-jobs/{ivrCallJobId}/detail` | `IVR_QUEUE_VIEW` | task → eligibility → attempts → result → callback trace, plus technical-exception and review-item ids and evidence/audit refs. |

Source: `src/Ivr.Api/Admin/AdminReadContracts.cs`,
`src/Ivr.Api/Application/AdminReadService.cs`,
`src/Ivr.Api/Admin/IvrAdminEndpoints.cs`.

Design points worth stating:

- **The service never writes.** It has no `SaveChanges` path and no mutation
  method; every query is `AsNoTracking`.
- **Aggregates are computed server-side** so no KPI is derived in the browser
  (P3-2 §9). Rates are returned as fractions rounded to four places.
- **`order_code` is a filter input only.** The response carries the
  script-approved `order_code_short`, read out of the persisted privacy-safe
  summary; the full code never leaves the database (D-05, `specs/ui/02`).
- **`order_state` is passed through as opaque text.** It is displayed, never
  derived and never written (D-02).
- **The SIM pool and open incidents ignore the program/time filter** because
  they are machine-wide state, not per-program.
- Every response still passes through `PiiMaskingFilter`, which fails closed on
  a restricted field name or a value that trips `PiiGuard`.

Not added, deliberately: `cost_per_confirmed_order` from `specs/ui/01`. There is
no cost model yet — it is `P10-3` / `W-0054`, marked "measured data needed" — and
inventing one would put a fabricated number on an operations dashboard.

## 3. Contract governance

The contract is IVR-owned and the change is purely additive.

```text
oasdiff changelog draft.2 → current : 3 endpoints added, nothing else
oasdiff breaking  --fail-on WARN    : "No breaking changes to report"
```

- `specs/api/openapi/ivr-order-confirmation.v1.yaml` version `1.0.0-draft.2` →
  `1.0.0-draft.3`. The bump is deliberate: `draft.2` is a frozen baseline
  snapshot, so a contract that differs from it must not keep claiming to be it.
- Regenerated `src/Ivr.Contracts/Generated/IvrServer/V1/IvrServerModels.g.cs`
  via `deploy/ci/scripts/regenerate-openapi.ps1` (+653 lines).
- `contract-manifest.json` hash re-pinned through
  `npm run openapi:accept-reviewed-draft`; `openapi:drift` then reports
  `OPENAPI_HASHES_PINNED=3`, `OPENAPI_HUMAN_DIFF_CURRENT=YES`.
- `docs/api/changelog/ivr-order-confirmation.md` regenerated with the pinned
  `tufin/oasdiff:v1.26.1` image; `docs/api-changelog.md` and the API-docs portal
  updated to match.
- The Sales callback contract is untouched: its hash and changelog are unchanged
  and no external gate moved.

## 4. Tests

`IT-ADMIN-READ-01..08` in `tests/Ivr.IntegrationTests/AdminReadApiTests.cs`,
against real PostgreSQL via Testcontainers — **8/8 pass**.

| Test ID | Asserts |
| --- | --- |
| `IT-ADMIN-READ-01` | All three routes mapped; an actor with only `IVR_MANUAL_RETRY` gets `403 IVR_FORBIDDEN_CALLER` on each. |
| `IT-ADMIN-READ-02` | Queue, result, attempt and SIM aggregates and the incident list all match the seeded graph, including `confirm_rate=0.5` from 1 of 2 results. |
| `IT-ADMIN-READ-03` | `program=GOLDEN_HOUR` narrows jobs and results but leaves the SIM pool whole; an unknown program is `400`. |
| `IT-ADMIN-READ-04` | Paging metadata; masked phone; `order_code_short`; neither full order code appears anywhere in the payload. |
| `IT-ADMIN-READ-05` | Each documented filter — order_code, correlation_id, result_type, queue_status, program, near_expiry — selects the expected single job. |
| `IT-ADMIN-READ-06` | Full trace, both attempts in order, DT-02 holds (technical attempt not counted), and the `technical_exception_id` / `review_item_id` the mutations need are present. |
| `IT-ADMIN-READ-07` | Unknown job → `404 IVR_NOT_FOUND` envelope. |
| `IT-ADMIN-READ-08` | No response contains `dial_token`, `phone_ref`, `recording`, the stored ciphertext or a raw MSISDN. |

Responses are deserialised into the **OpenAPI-generated DTOs**, so a drift
between the runtime projection and the committed schema fails the test rather
than reaching the browser.

Full solution suite after the change: **289/289** — 21 contract, 168 unit,
100 integration. Build: 0 warnings, 0 errors.

## 5. Commands and results

```text
dotnet build Ivr.sln --no-restore                 Build succeeded, 0 Warning(s)
dotnet test  Ivr.sln --no-build                   289/289 pass
npm --prefix deploy/ci run openapi:lint           valid
npm --prefix deploy/ci run openapi:validate       OPENAPI_FILES_VALID=2
npm --prefix deploy/ci run openapi:drift          OPENAPI_HASHES_PINNED=3
npm --prefix deploy/ci run test:openapi-negative  CT-CI-01 PASS
npm --prefix deploy/ci run test:config            CI_CONFIG_SELFTEST_PASS
npm --prefix deploy/ci run test:docs              API_DOCS_SELFTEST_PASS
oasdiff breaking (pinned image)                   no breaking changes
```

## 6. A Windows-only gate failure, fixed on the way

`test:docs` and the changelog-baseline check were failing locally on any Windows
checkout, and would have taken the contract-manifest hash down with them: with
`core.autocrlf=true` Git rewrites these files to CRLF, so tools that hash raw
bytes outside Git see different content than CI does. The manifest hash pinned
from a CRLF working tree would have gone red in CI.

Fixed in `.gitattributes` by pinning `specs/api/openapi/**`, `docs/api/**` and
`docs/api-changelog.md` to `eol=lf`, and normalising the four affected files in
place. Local and CI now hash identical bytes, and `test:docs` passes on Windows
for the first time.

## 7. Not claimed

- Owner and reviewer acceptance: **pending**. `TESTS_PASS`, not `ACCEPTED`.
- Target contract state is unchanged: `TARGET_CONTRACT_V1=DRAFT`. Adding
  IVR-owned read endpoints approves nothing on the Sales side.
- Hosted GitLab pipeline evidence: `NOT_RUN`; all results above are local.
- No new permission was introduced and no role gained one. `IVR_RUNTIME_GATE_ADMIN`
  remains ungranted pending `OD-V1-20`.
- The endpoints read; they cannot transition an order, alter a result, change an
  attempt count or enable a SIM.

# W-0065 — P2-8 internal and admin API evidence

Status: `TESTS_PASS` (MOCK-only)

Baseline: `87457b6f37a89c35a3eaf9e77452049e9c0429d9`

Evidence date: `2026-08-14`

Real-customer-call gate: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## Delivered boundary

- Six IVR-owned lifecycle operations require the dedicated bearer secret from
  `IVR_INTERNAL_SERVICE_TOKEN`, `X-Source-System: ivr-worker|ivr-adapter`, and
  `X-Service-Scope: ivr.internal.write`. Admin identity/permission headers are
  rejected on this surface.
- Lifecycle POST operations are idempotent and reassert the canonical records
  already owned by eligibility, scheduler, normalizer, and callback workers.
  They cannot synthesize arbitrary jobs, attempts, final results, or callbacks.
- Seven admin operations enforce the existing `IVR_*` permission one-to-one and
  bind `X-Actor-Id` to the authenticated subject.
- Every admin mutation commits `ivr_admin_actions` and `ivr_audit_log` in the
  same serializable transaction with reason, actor, before/after state,
  correlation, evidence reference, and `no_policy_bypass=true`.
- Queue pause blocks only new scheduler claims. Existing active calls and leases
  are not cancelled. Resume remains blocked while another open hold exists.
- Direct enable is MOCK-only and is rejected for REAL, quarantined, failed,
  active, or unreconciled channels. Disable preserves an active lease/call.
- Technical retry is non-customer-counting, bounded, and rechecks final state,
  expiry, eligibility, call restriction, queue hold, kill switch, allowlist,
  execution mode, and the real-call gate.
- Admin review resolves/annotates `ivr_review_items` without changing the
  normalized `ivr_call_results` row.
- Responses are typed, masked projections. A fail-closed PII filter scans the
  serialized response; request/audit fields use `PiiGuard`. No request or
  response body is logged by these endpoints.

The IVR does not transition Sales order state, call Sales, send SMS, dial a
customer, enable a real SIM/eSIM adapter, or create a payment/revenue effect in
this work item.

## API surface

| Boundary | Operations | Authentication/authorization |
| --- | ---: | --- |
| Internal lifecycle | 6 | IVR service identity + source allowlist + `ivr.internal.write` |
| Admin | 7 | authenticated admin + exact existing `IVR_*` permission + actor binding |
| Total W-0065 | 13 | all POST operations require idempotency and correlation |

Admin browser call-detail does not receive the internal secret. The future UI
must use an admin backend/BFF that enforces `IVR_QUEUE_VIEW`, then reads the
masked internal projection using its own service identity.

## Required test groups

| Test ID | Cases | Result | Proven behavior |
| --- | ---: | --- | --- |
| `CT-API-OAS-10` | 1 | PASS | exact 13 routes; six internal success bodies deserialize using generated OpenAPI DTOs |
| `IT-API-AUTHZ-01` | 1 | PASS | seven admin routes reject the wrong permission; actor binding is enforced |
| `IT-API-AUTHZ-02` | 1 | PASS | internal service token, source and scope required; admin impersonation rejected |
| `IT-API-IDEMP-03` | 1 | PASS | same key/hash replays one action/audit; changed payload returns 409 |
| `IT-API-AUDIT-04` | 1 | PASS | action/audit pair has before/after/reason/actor; audit UPDATE rejected by DB trigger |
| `IT-API-QUEUE-08` | 1 | PASS | pause blocks a new claim; resume restores claim; active work is untouched |
| `IT-API-SIM-09` | 1 | PASS | disable preserves active lease; unsafe direct enable returns conflict |
| `IT-API-RETRY-06` | 3 | PASS | non-counting bound plus kill-switch and allowlist fail-closed cases |
| `IT-API-REVIEW-07` | 1 | PASS | result snapshot is byte-equivalent before/after review |
| `IT-API-PII-05` | 1 | PASS | raw PII and malformed JSON fail closed; masked queue response has no restricted field |
| **Total** | **12** | **12/12 PASS** | **10 required groups covered** |

Synthetic/redacted evidence samples:

- `docs/evidence/W-0065/authz-403-samples.json`
- `docs/evidence/W-0065/idempotency-409-sample.json`
- `docs/evidence/W-0065/audit-redacted.json`
- `docs/evidence/W-0065/coverage-summary.txt`

These samples contain no production identity, credential, destination, phone,
address, dial token, order, or customer data. The integration assertions are
the executable evidence; the JSON files are privacy-safe review projections.

## Contract evidence

- OpenAPI source: `specs/api/openapi/ivr-order-confirmation.v1.yaml`.
- Reviewed draft SHA-256:
  `4dd221befe0e2cd8b5bc090ec0179ca3581caa928abf268ba865fd30c31316d4`.
- Human-readable manifest/diff:
  `docs/contracts/openapi-contract-diff.md`.
- Generated DTO:
  `src/Ivr.Contracts/Generated/IvrServer/V1/IvrServerModels.g.cs` using pinned
  `NSwag.ConsoleCore 14.7.1`.
- Validation proves two OpenAPI files parse, negative schema fixtures are
  rejected, generated code is current, and the reviewed-draft hash is pinned.
- Self-review corrected a YAML indentation defect that had combined
  `REJECTED_STALE` and `IDEMPOTENCY_CONFLICT`; the final parsed enum contains six
  distinct values and all contract gates were rerun afterward.

## Final local verification

| Gate | Result |
| --- | --- |
| Release analyzer build | PASS — 0 warnings, 0 errors |
| Focused W-0065 integration | PASS — 12/12 |
| Contract tests | PASS — 21/21 |
| Unit tests | PASS — 157/157 |
| PostgreSQL integration tests | PASS — 77/77 |
| Total regression | PASS — 255/255 |
| Fresh aggregate line coverage | PASS — 94.30% (22,760/24,137), 3 reports, threshold 60% |
| `dotnet format --verify-no-changes` | PASS |
| EF pending-model changes | PASS — none; W-0065 reuses the existing schema |
| OpenAPI lint/parse/negative/codegen/drift | PASS — four pre-existing unused-component warnings, no errors |
| API portal build/drift/local links | PASS |
| GitLab CI topology/config self-tests | PASS |
| Admin UI lint/build/npm High audit | PASS — 0 vulnerabilities |
| NuGet High vulnerability policy | PASS |
| Docker Compose MOCK profile | PASS |
| Gitleaks 8.30.0 staged commit-scope scan | PASS — no leaks |
| Locale-stable PII self-test + evidence/artifact scan | PASS |
| Official Markdown map | PASS — 418 files, 375 resolved links, 0 unresolved |
| GitNexus staged change review | CRITICAL (expected) — 33 files, 285 symbols, 141 API/admin/privacy/scheduler flows |

## Samples and safety interpretation

- The 403 matrix proves permission names are not interchangeable. A 403 on the
  internal surface also proves admin headers cannot substitute for service
  identity.
- The 409 sample proves a reused idempotency key cannot alter a completed
  mutation. Exactly one admin action and one audit row remain.
- The audit sample is redacted and synthetic; the PostgreSQL test queries the
  committed rows and then proves the append-only trigger rejects UPDATE.
- OpenAPI remains `TARGET_CONTRACT_V1=DRAFT`. Pinning the reviewed draft is not
  owner approval and is not hosted Sales contract evidence.

## Residual gates

- GitLab hosted pipeline/protected-main evidence: `NOT_RUN` for this commit;
  direct main push is expected to remain rejected by project protection.
- Reviewer/owner acceptance: `NOT_RUN`; status is intentionally capped at
  `TESTS_PASS`.
- Real Sales/internal identity issuer, production service token, sandbox/CDC,
  and Target V1 approval: `BLOCKED_EXTERNAL` / `OWNER_DATA_REQUIRED`.
- Physical one-SIM/eSIM lab, carrier/modem, allowlisted test destination, TTS
  pronunciation, 32-eSIM capacity, and production release evidence: `NOT_RUN`.
- Real SIM enable and real customer calling remain fail-closed. No production
  readiness or integration-verified claim is made.

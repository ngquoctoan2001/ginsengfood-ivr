# W-0014 — P1-1 OpenAPI Codegen and Contract Scaffold Evidence

Date: 2026-08-12

IVR baseline: `main@c78a407466e0f49847c83e0cea665582b80f6b1a`

Verified Sales baseline:
`ginsengfood-business-platform@a3aad246d986fbc273cf41aaa93eec6659669656`

Execution mode: `MOCK` · fake Sales: `FAKE_TARGET_V1` · mock SIM: `MOCK` ·
real-customer calls: `NO`

Final status: `TESTS_PASS`. Both Target V1 contracts remain
`TARGET_CONTRACT_V1=DRAFT`; this evidence does not claim Sales approval,
connectivity, credentials, a real SIM call, or production readiness. Later
hosted GitLab quality pipelines are recorded separately under W-0061.

> Evidence correction — 2026-08-14: the local P1-1 results and later historical
> hosted W-0061 pipelines are separate evidence sets. Current GitLab `main`
> has `Allowed to push and merge: No one`, so the setting is
> `PASS_SETTING_CURRENT`; a fresh rejection probe is `NOT_RUN`. No hosted run of
> the current remediation tree is claimed.

## Scope implemented

- Repository-local `NSwag.ConsoleCore 14.7.1` tool manifest and deterministic
  PowerShell regeneration for committed generated code.
- A generated-code-only `.gitattributes` exception accounts for NSwag's
  trailing spaces without weakening byte-level codegen drift or handwritten
  whitespace checks.
- IVR-owned server DTOs generated into
  `Ivr.Contracts.Generated.IvrServer.V1`; no generated server client.
- Target Sales callback client/DTOs generated into
  `Ivr.Contracts.Generated.SalesTarget.V1` with injected `HttpClient`.
- A manifest pins the two OAS files, the verified current-compat fixture,
  generator version, generated paths, and Sales source commit. A deterministic
  human-readable report inventories operations, schemas and required fields.
- Normal drift validation is read-only. Only the explicit
  `--accept-reviewed-draft` command can refresh reviewed hashes/report, and it
  cannot promote the contract from DRAFT.
- Separate current Golden Hour DTO, result enum, envelope, exception and client.
  They have no inheritance, conversion or shared callback request base with
  Target V1.
- Typed provider selection accepts only `FAKE_TARGET_V1`,
  `CURRENT_GOLDEN_HOUR_COMPAT` or `TARGET_V1`. The current provider rejects
  `TWENTY_FOUR_SEVEN` and currently has no startup-valid runtime mode.
- Startup validates the complete mode/provider/SIM matrix: MOCK uses fake
  Target + mock SIM; lab uses fake Target + vendor SIM; production-real uses
  Target V1 + vendor SIM. Real-customer permission remains fail-closed.
- Fake Sales mappings cover both allowed program/payment rows, all four Target
  semantic success ACKs, stale/conflict, invalid request, rate limit and 5xx,
  plus current-compat accepted/rejected responses.
- OpenAPI validation now proves the exact two-row task matrix and rejects
  cross-combinations, missing version/policy/token fields and unsafe speech
  shapes. Both OAS documents lint with zero warnings.
- GitLab CI restores the pinned tool, regenerates both outputs and fails on
  generated drift; the OpenAPI job separately enforces source hashes and the
  human report.

## Verified current Sales compatibility source

The compatibility contract was read directly from the pinned Java source, not
inferred from the Target draft:

- `GoldenHourIvrCallbackRequest.java`: `callId`, `reservationId`, `orderId`,
  `customerId`, `result`, optional `occurredAt`, `idempotencyKey`;
- `GoldenHourIvrCallbackResult.java`: `CONFIRMED`, `REJECTED`, `NO_ANSWER`,
  `FAILED`;
- `GoldenHourIvrCallbackResponse.java`: current response-data fields;
- `InternalGoldenHourIvrCallbackController.java`: `POST
  /api/v1/internal/ivr/golden-hour/callbacks`, transitional
  `X-Internal-Token`, `ApiResponse<GoldenHourIvrCallbackResponse>` envelope.

The checked fixture is
`specs/api/compat/current-golden-hour-callback.a3aad246.schema.json`. Target-only
version, attempt, evidence, action and semantic-ACK fields are explicitly
unsupported by this current contract.

## Pinned contract and generated hashes

| Artifact | SHA-256 |
| --- | --- |
| IVR-owned Target V1 OAS | `5a7bafa4f69f28d480ee3083ea5ede5be0577dc06734964f0f5ba686e13d111b` |
| Sales callback Target V1 OAS | `1677d490eea5484e449ace3310e26e3c59acbb8011c7c1736e3f981afffa96ee` |
| Current Golden Hour compatibility fixture | `ad2f655070b14d0cdfb0540893f7d7ea83354dda56c4b403ae47f56a3f6a494d` |
| Generated IVR server models | `5821f13fb15c7c77ab5496afbbd295c634f413c5ce9a8ca6e2fc1a2e22e5f114` |
| Generated Target Sales client | `88b84fd1a43e1faa89c523ff112428c2084096de3422d38581574f83cf7594ee` |

Regeneration reported no file changes and both before/after generated hashes
were identical.

## Contract tests

| Test ID | Result and proof |
| --- | --- |
| `CT-CONTRACT-SEPARATION-01` | PASS — internal Target, outbound Target and current DTO types are unrelated/non-assignable |
| `CT-CONTRACT-PROVIDER-02` | PASS — typed selection covers fake/Target for both programs and current only for Golden Hour |
| `CT-CONTRACT-PROVIDER-03` | PASS — current compatibility rejects 24/7 |
| `CT-CONTRACT-TARGET-ACK-04` | PASS — generated client parses `ACCEPTED`, `DUPLICATE_ACCEPTED`, `BLOCKED_BY_CORE`, `REVIEW_REQUIRED` |
| `CT-CONTRACT-TARGET-ERROR-05` | PASS — typed exceptions cover both 409 meanings plus 422/429/500/503 |
| `CT-CONTRACT-CURRENT-06` | PASS — exact current route/header/wire DTO and uppercase result are observed |
| `CT-CONTRACT-WIREMOCK-07` | PASS — fixture catalog contains both matrix rows, all response classes and current scenarios |

OpenAPI fixture validation additionally reported:
`OPENAPI_FILES_VALID=2`, `TARGET_TASKS_SCHEMA_VALID=9`,
`SCHEMA_NEGATIVE_REJECTED=10`, `DOMAIN_NEGATIVE_SCHEMA_VALID=10`,
`CURRENT_COMPAT_SCHEMA_VALID=1`, and
`CURRENT_COMPAT_TARGET_FIELD_REJECTED=1`.

## Final local gate results

| Gate/command | Exact result |
| --- | --- |
| locked restore | PASS — all solution projects restore from committed lock files |
| NSwag regeneration | PASS — tool `14.7.1`; both generated hashes stable |
| Release build/analyzers | PASS — 0 warnings, 0 errors |
| full solution tests | PASS — 55/55: 19 contract, 22 unit, 14 integration |
| coverage policy | PASS — merged line coverage `75.57%` (`1404/1858`, 3 reports), above 60% |
| format | PASS — 0/108 files changed by `dotnet format --verify-no-changes` |
| CI configuration | PASS — CT-CI-05/07/08 plus `OPENAPI_CODEGEN_GATE_PASS` |
| OpenAPI | PASS — Redocly 0 warnings; 2/2 parse/ref; hash/report drift and negative self-test PASS |
| admin UI | PASS — clean install, ESLint, TypeScript/Next.js production build |
| dependencies | PASS — NuGet High/Critical policy and both npm audits; npm reports 0 vulnerabilities |
| Compose | PASS — MOCK profile renders with Postgres, fake Sales, mock SIM and mock JWT |
| secrets/privacy | PASS — Gitleaks found no leaks; locale-stable PII self-tests and 60 evidence/artifact files pass |

During the final artifact scan, Cobertura method metadata named after the
`dial_token` contract property produced a false positive. The token pattern now
requires an actual `:` or `=` assignment and tests JSON/YAML/env-like values
while explicitly accepting the harmless Cobertura method name. This closes the
false-positive without weakening value detection; CT-CI-06g and the full
artifact rescan pass.

## GitNexus and impact review

The refreshed graph contains 37,854 nodes, 39,085 edges and 66 flows. Focused
upstream impact is LOW for the generated Target client, current compatibility
client, typed selector and startup validator; consumers are limited to the
expected configuration and contract/unit tests. The integration-test fixture
adjustment is MEDIUM with five direct test callers, one test module and zero
production flows. Final staged `detect_changes` is MEDIUM across 42 files and
452 symbols with four generated Target-client flows; all four are covered by
contract tests. The circular-import check reports zero cycles.

## Residual gates and ownership boundaries

- `W-0002`, `W-0005` and `W-0006` remain `BLOCKED_EXTERNAL`: Sales has not
  approved Target V1, supplied sandbox/base URL/auth, or completed CDC.
- `W-0061` remains `BLOCKED_EXTERNAL`: historical hosted runner/MR/Registry/
  Pages proof remains valid and the no-direct-push setting is current; required
  independent MR approval remains unavailable.
- Current Golden Hour compatibility is source-verified but runtime-disabled;
  it is not treated as the generic Target endpoint.
- Sales API, identity issuer, vendor SIM/eSIM, customer calls, lab, staging,
  pilot and production are `NOT_RUN`.
- No SMS/notification or Sales order-transition endpoint was added.

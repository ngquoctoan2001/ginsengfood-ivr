# W-0016 / P1-3 — Domain, DTO mapping, provider ports and privacy guards

Date: 2026-08-13

Baseline: `38eaecad2b4ce99aa14b12f708f5db1dd5fda5e9` (`P1-2`)

Mode: `MOCK`

Status: `TESTS_PASS`; local domain/provider proof and historical hosted GitLab quality pipelines pass.

> Evidence correction — 2026-08-14: historical hosted passes do not prove the
> current remediation tree. Current GitLab `main` has
> `Allowed to push and merge: No one`; the setting is `PASS_SETTING_CURRENT`
> under W-0061 and a fresh rejected-push probe is `NOT_RUN`.

## Implemented scope

- Immutable value objects for task/order/callback/attempt/job/correlation IDs, order/policy versions, evidence/audit references and redacted dial-token references.
- `ProgramPaymentPolicy` accepts only `GOLDEN_HOUR + ONLINE + required=true` and `TWENTY_FOUR_SEVEN + COD + required=true`.
- Versioned `AttemptPolicySnapshot` and `AttemptOffsets` validate max attempts, zero-based strictly ordered offsets and confirmation-window bounds.
- Candidate policies are allowed only in `MOCK`/`LAB_REAL_SIM`; `PRODUCTION_REAL` requires owner-approved policy data.
- Privacy-safe Vietnamese speech model supports bounded item names/quantity/unit, VND total, short administrative delivery area, `vi-VN` and bounded pronunciation hints.
- Semantic full-address detector rejects leading house numbers, slash-form house numbers and street/address markers. Raw phone values are rejected when creating dial references or gateway authorizations.
- Result model contains the full 11-result taxonomy. No-answer can only recommend `CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`; technical/capacity/operational/policy results cannot count as customer attempts.
- Semantic callback ACK model is independent of legacy HTTP status details.
- Provider ports: policy registry, dial-token resolver, speech renderer, SIM gateway, Order Core callback client, service-token provider, clock, ID generator, audit and evidence sinks.
- Every port has a deterministic fake or in-memory implementation under `Ivr.Infrastructure.Providers.Fakes`.
- Target V1 task/callback/ACK mapping and current Golden Hour compatibility mapping are separate anti-corruption layers. The current mapper rejects 24/7 and target-only result values rather than applying a lossy mapping.
- SHA-256 hashes use normalized, length-independent canonical field encoding; collections are copied into immutable ordered representations.

## Tests executed

The P1-3 tests executed as part of the 54/54 unit-test pass and cover:

- exact program/payment/required matrix;
- attempt bounds, ordered offsets and candidate-environment gate;
- Vietnamese Unicode, item/unit, VND total, short delivery area and configurable limits;
- raw phone, dial-token-looking text and full-address rejection;
- no-answer advisory and technical/capacity/policy non-counting rules;
- Target V1/current-compat mapping separation and semantic ACK mapping;
- deterministic immutable snapshot hashes;
- deterministic fake coverage for every provider port.

## Review evidence

- GitNexus pre-edit anchors: `PiiGuard` LOW/0 caller; `SalesCallbackContractSelector` LOW/4 direct imports; no HIGH/CRITICAL pre-existing symbol change was made.
- GitNexus final re-analysis parsed the reviewed files and refreshed the graph to 38,700 nodes, 40,925 edges and 113 flows.
- Focused post-index impacts: `AttemptPolicySnapshot` LOW/8; `TargetV1TaskMapper` LOW/1; `ConfirmationTaskSnapshot` LOW/4; `CurrentGoldenHourCompatMapper` LOW/0. `AttemptOffsets` is HIGH/10 across three policy flows; `CallResultSnapshot` is HIGH/8 with one callback creation flow and three affected modules. Neither HIGH symbol may change again without full policy/mapper/callback regression.
- Staged `detect_changes(compare, main)` reports HIGH breadth: 292 changed
  symbols, 22 indexed files, 12 affected speech/privacy/policy/task-mapping
  flows. This is the expected blast radius of the new domain and
  anti-corruption boundary; full unit/contract/integration regression and
  privacy gates passed before handoff.
- All 13 new C# files have balanced braces, parentheses and brackets.
- All generated Target V1 enum members referenced by the mapper exist in the generated clients/models.
- Direct privacy-surface scan found no HTTP-status branching, notification dependency or raw phone/full-address property in the domain model.

## Full local gates

- locked restore: PASS;
- Release build: PASS, 0 warnings / 0 errors;
- tests: PASS, 93/93 (`19` contract, `54` unit, `20` integration); the
  integration lane included PostgreSQL Testcontainers on Docker Engine 29.6.2;
- aggregate line coverage: `90.99%` (`7476/8216`, three reports), threshold
  `60%` PASS;
- `dotnet format --verify-no-changes`: PASS, 0/138 files changed;
- UI ESLint and Next.js production build: PASS;
- OpenAPI lint, parse/schema validation, pinned drift and negative self-test:
  PASS;
- GitLab config self-test and Docker Compose MOCK config: PASS;
- NuGet and both npm High/Critical audits: PASS, zero reported
  vulnerabilities after `W-0078` pinned transitive `SSH.NET` to patched version
  `2026.0.0`;
- Gitleaks 8.30.0 directory and Git-history scans: PASS, no leaks;
- locale-stable PII self-test and evidence/artifact scan: PASS, 67 files;
- official Markdown map: 398 files, 369 resolved links, 0 unresolved links.

## Security-gate remediation discovered during closure

`Testcontainers 4.13.0` transitively selected `SSH.NET 2025.1.0`. The NuGet
audit began failing with High advisory `GHSA-q939-rpr3-3284`, whose affected
range is `<=2025.1.0` and patched version is `2026.0.0`. Testcontainers 4.13.0
was still the latest release on 2026-08-13, so `W-0078` records the unplanned
direct patched-version pin. Locked restore, the vulnerability audit, full build
and all 20 integration tests passed after the pin.

## Explicit residual gates

- `W-0061` / `G-GITLAB`: `BLOCKED_EXTERNAL`; historical hosted MR/runner/
  Registry/Pages evidence passes and current no-direct-push configuration
  passes, while independent approval remains unavailable.
- `TARGET_CONTRACT_V1` remains `DRAFT`; Sales endpoint/auth/CDC and approved
  production attempt policy remain external.
- Real Sales API, SIM/eSIM hardware, SMS and real customer calls were not used.
  P1-3 proves the local MOCK/domain boundary only.

## Reproduction commands

```powershell
dotnet restore Ivr.sln --locked-mode
dotnet build Ivr.sln -c Release --no-restore
dotnet format Ivr.sln --verify-no-changes --no-restore
dotnet test Ivr.sln -c Release --no-build
dotnet test Ivr.sln -c Release --no-build --collect:"XPlat Code Coverage"
dotnet run --project deploy/ci/tools/Ivr.CiPolicy -- coverage ci-artifacts/dotnet/coverage-w0016 60
npm --prefix deploy/ci run test:config
npm --prefix deploy/ci run openapi:lint
npm --prefix deploy/ci run openapi:validate
npm --prefix deploy/ci run openapi:drift
npm --prefix deploy/ci run test:openapi-negative
docker compose -f docker-compose.dev.yml --profile mocks config --quiet
node .gitnexus/run.cjs analyze
```

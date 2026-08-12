# W-0012 — P0-3 Cross-Cutting Foundation Evidence

Date: 2026-08-12

Baseline: `main@0c2f692` (dedicated P0-2 commit)

Execution mode: `MOCK`

Real customer calls: `NO`

Final local status: `TESTS_PASS`. Source, local/container quality gates, and
MOCK integration tests are complete. Hosted GitLab execution remains
`NOT_RUN`/`BLOCKED_EXTERNAL` under W-0061 and is not implied by this evidence.

## Scope implemented

- Fail-fast typed configuration with safe execution-mode validation and a hard
  rejection when real-customer calling is enabled before its release gate.
- `X-Correlation-Id` inbound preservation/generation, logging scope,
  `ICorrelationContext`, response header, and automatic outbound `HttpClient`
  propagation.
- Central error factory/catalog containing exactly the 15 API-06 codes and a
  privacy-safe envelope with deterministic HTTP mapping.
- Seven fixed DF-01 permissions, server-side policy evaluation, MOCK-only
  `X-Permissions` claim adapter, and fail-closed non-MOCK behavior.
- Reusable Order Core command-route allowlist requiring exact source identity
  and a constant-time service-token comparison. The token is environment or
  secret-provider input only and is absent from tracked appsettings files.
- Interface-first idempotency, append-only audit, and evidence registries with
  in-memory MOCK implementations. PostgreSQL mappings/migrations remain owned
  by P1-2.
- Required administrative reason, correlation-bearing audit entries, approved
  phone mask shape, and field/value guards that reject raw PII. Correlation
  input resembling PII is replaced before response, logger scope, or outbound
  propagation; mixed-case Unicode address variants are rejected.
- Middleware order is correlation, authentication/authorization, allowlist,
  then error envelope, exactly as required by P0-3.

## Required P0-3 tests

| Test ID | Result and proof |
| --- | --- |
| `UT-FND-IDEMP-01` | PASS — same key/hash replayed the stored response without a second factory execution; a different hash returned the stable conflict code |
| `UT-FND-CORR-02` | PASS — inbound `corr-inbound-1` was returned and captured on outbound HTTP; a missing header generated one and propagated the same value |
| `UT-FND-RBAC-03` | PASS — missing permission produced HTTP 403 plus the stable forbidden code; the fixed permission passed |
| `UT-FND-RBAC-08` | PASS — non-MOCK rejected the mock header; explicitly registering the mock provider in LAB threw during service registration |
| `UT-FND-ALLOW-04` | PASS — missing credential, wrong source, and wrong token were rejected; exact source plus valid token passed |
| `UT-FND-ERR-05` | PASS — stable 409 and safe 500 envelopes were asserted; exception type, stack, and raw test PII were absent from the response |
| `UT-FND-AUDIT-06` | PASS — only append exists on the interface; admin reason and correlation are required; rejected data never entered the store |
| `UT-FND-PII-07` | PASS — mask output is `09****1234`; restricted field/value guards reject raw PII |

The prompt says “7 tests” in its review/DoD prose, but its test table contains
eight required IDs. All eight were implemented. Three additional tests also
pass: `UT-FND-CONFIG-09` for fail-fast unsafe/missing configuration,
`UT-FND-EVID-10` for evidence uniqueness/append-only/PII safety, and
`UT-FND-ERRCAT-11` for immutable validated error details.

## Normalized safe envelope samples

Correlation values below are normalized labels. The integration test validates
the actual generated/propagated values.

```json
{"error":{"code":"IVR_FORBIDDEN_CALLER","message":"The caller is not permitted.","details":{},"correlationId":"corr-rbac"}}
{"error":{"code":"IVR_IDEMPOTENCY_CONFLICT","message":"The idempotency key was already used with a different payload.","details":{},"correlationId":"corr-conflict"}}
{"error":{"code":"IVR_INTERNAL_ERROR","message":"An internal error occurred.","details":{},"correlationId":"corr-failure"}}
```

No raw customer data, exception message, stack trace, recording reference, or
real credential is present in these samples or this evidence pack.

## Final local gate results

| Gate/command | Exact result |
| --- | --- |
| locked restore | PASS — all nine solution projects restore with committed lock files |
| Release build | PASS — 0 warnings, 0 errors |
| full test run | PASS — 14/14 implemented tests: 8 unit and 6 integration; 11 are P0-3 tests |
| coverage policy | PASS — merged line coverage `91.99%` (`563/612`, 3 Cobertura reports), threshold 60% |
| format/analyzers | PASS — no formatting or analyzer changes required |
| admin UI | PASS — clean install, ESLint, strict TypeScript, Next.js production build; npm audit reports 0 vulnerabilities |
| CI/OpenAPI | PASS — CI self-tests CT-CI-05/07/08; 2/2 OpenAPI parse; 9 valid target fixtures; negative tests pass; 10 pre-existing advisory warnings and 0 errors |
| dependency policy | PASS — NuGet High/Critical gate; both npm audits report 0 vulnerabilities |
| Compose | PASS — development Compose including inert mock profile renders successfully |
| Gitleaks | PASS — repository directory scan reports no leaks |
| PII | PASS — locale self-tests plus a separate current evidence/artifact scan |

## GitNexus staged change review

The final staged analysis reports `CRITICAL`: 56 files, 228 symbols, and 25
affected execution flows. This warning is retained rather than relabelled. The
scope creates the foundation primitives and their tests in one prompt, so all
reported flows are the new correlation, error-writing, authorization,
idempotency, audit, evidence, and privacy paths reviewed above.

Focused impact analysis found the PII guard at `HIGH` (four direct dependants,
two affected store processes). That triggered the remediation recorded here:
mixed-case Unicode coverage, complete audit/evidence metadata guarding,
PII-shaped correlation replacement, idempotency snapshot guarding, and
immutable error detail snapshots. Its regression suite passes. The API
registration/pipeline entrypoints and individual middleware/store operations
are `LOW` or `MEDIUM`; the graph reports zero circular imports. No process
outside the P0-3 foundation/test scope was identified.

The contract-test assembly currently contains no discovered test. Contract
generation belongs to P1-1; this is visible and is not counted among the 13
implemented tests above.

## Append-only and privacy review

- `IAuditLogger` exposes `AppendAsync` only; reflection regression coverage
  confirms there is no update/delete method, and the in-memory implementation
  only enqueues immutable entries.
- `IEvidenceStore` exposes `AppendAsync` only; duplicate evidence references
  fail closed and rejected data is not persisted.
- Actor/action/entity/reason/correlation/data metadata and every evidence field
  pass the same PII guard; the regression suite includes mixed-case Unicode and
  a PII-shaped inbound correlation value.
- Error-code literals occur only in the central error catalog; permission
  constants are held in their own fixed catalog.
- `X-Permissions` exists only in MOCK implementation/test code and does not
  appear in either OpenAPI contract.
- The Order Core service token key is documented, but no token value exists in
  source configuration, logs, or evidence.

## Residual gates and ownership boundaries

- W-0061/G-GITLAB remains `BLOCKED_EXTERNAL`: no GitLab project/remote, runner,
  hosted MR pipeline, protected-branch settings, approval enforcement, or
  registry proof exists yet.
- P4-4 owns production JWT/service-auth federation. The current non-MOCK scheme
  intentionally fails closed; it is not a production authentication claim.
- P1-2 owns PostgreSQL entity mapping and migrations for idempotency, audit, and
  evidence. P0-3 deliberately adds no migration or shared Sales database access.
- Sales API/auth, real SIM/eSIM, customer calls, lab, staging, and production are
  `NOT_RUN`. No real call path was enabled or exercised.

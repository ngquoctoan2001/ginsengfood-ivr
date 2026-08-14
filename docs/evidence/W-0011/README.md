# W-0011 — P0-2 GitLab CI Baseline Evidence

Date: 2026-08-12

Baseline: `main@85cefa7` (P0-1 accepted commit)

Execution mode: `MOCK`

Real customer calls: `NO`

Final status: `TESTS_PASS`. Local/config, hosted pipelines, runner identity,
protected branch, `Pipelines must succeed`, Registry, Pages access control and
protected-variable evidence are complete. Required independent MR approval
enforcement remains `BLOCKED_EXTERNAL` under W-0061 because the current GitLab
Free project has one member and exposes approvals only as optional.

## Scope implemented

- Root GitLab entrypoint with MR, default-branch, and deliberate web routing;
  feature-branch pushes without an MR do not create duplicate pipelines.
- Seven non-optional jobs: one CI configuration self-test plus the six required
  build/test, .NET lint, UI, OpenAPI, security, and PII quality gates.
- Locked NuGet/npm dependencies, JUnit/Cobertura reports, 60% aggregate line
  coverage, OpenAPI 3.1 parse/ref/schema validation, High/Critical dependency
  policy, Gitleaks, and a separate locale-stable PII scanner.
- Centralized PII artifact topology: every artifact-producing upstream job is a
  `needs` dependency with `artifacts: true`.
- PII violations report the file location with `[REDACTED]`; the matching phone,
  address, or opaque credential is never echoed into CI logs.
- GitLab MR traceability template, review routing, local runbook, and honest
  hosted-settings checklist.
- No active GitHub Actions workflow or remote GitLab include.

## Positive gate results

| Gate/command | Exact result |
| --- | --- |
| `npm --prefix deploy/ci run test:config` | PASS: CT-CI-05, CT-CI-07, CT-CI-08; required jobs and `allow_failure: false`; .NET image matches `global.json` |
| Redocly + OpenAPI validator | PASS: 2/2 OpenAPI files parse; 9 target tasks schema-valid; 7/7 schema negatives rejected; 10/10 domain negatives reach the domain layer; 10 advisory lint warnings, 0 lint errors |
| `dotnet restore Ivr.sln --locked-mode` | PASS: all nine solution projects restored from committed lock files |
| `dotnet build Ivr.sln --configuration Release --no-restore` | PASS: 0 warnings, 0 errors |
| `dotnet test ... --collect:"XPlat Code Coverage"` | PASS: 3/3 implemented tests; JUnit files for all three test assemblies and 3 Cobertura reports produced |
| coverage policy | PASS: merged `95.77%`, 68/71 unique lines across 3 reports, threshold 60% |
| `dotnet format ... --verify-no-changes` | PASS: formatted 0/43 files; analyzer pass |
| admin UI | PASS: clean `npm ci`, ESLint, strict TypeScript, Next.js 16.3.0 production build |
| dependency audits | PASS: NuGet High/Critical policy; admin UI npm 0 vulnerabilities; CI tools npm 0 vulnerabilities |
| exact Linux `security_scan` script | PASS in `mcr.microsoft.com/dotnet/sdk:10.0.201`; checksum-verified Gitleaks 8.30.0; full Git history clean |
| PII self-test and scan | PASS: 25/25 malicious fixtures rejected identically in `C`, `C.UTF-8`, and `POSIX`; safe fixtures accepted; downloaded-artifact simulation rejected; 20 current text artifacts/evidence files clean |
| Gitleaks directory scan | PASS: about 18.49 MB including uncommitted P0-2 files, no leaks |
| `docker compose ... --profile mocks config --quiet` | PASS |
| official `markdown-doc-reader` mapper | PASS: 391 Markdown files, 369 links resolved, 0 unresolved |

The OpenAPI warnings concern draft-document metadata, tag descriptions, missing
4xx responses, and unused compatibility schemas. They are visible but are not
parser/ref/schema errors; P1-1 owns generated-contract hardening.

## Negative self-tests

| Test ID | Result |
| --- | --- |
| CT-CI-01 | deliberately unresolved OpenAPI `$ref` rejected |
| CT-CI-02 | deliberately failing xUnit fixture returned exit 1 |
| CT-CI-03 | 50% Cobertura fixture returned exit 1; 60% boundary fixture passed; two complementary 50% reports merged to 100% unique-line coverage |
| CT-CI-04 | fake GitHub PAT caused Gitleaks exit 42; scanner then passed repository history |
| CT-CI-05 | MR/default-branch/web routes enabled; feature push and unsupported sources disabled |
| CT-CI-06/06b/06d/06e/06f | phone, Vietnamese uppercase/mixed-case, accented/unaccented address, and opaque credential fixtures rejected consistently across three locales; reviewed safe values accepted |
| CT-CI-06c | simulated PII in an upstream downloaded artifact rejected |
| CT-CI-07 | every `deploy/ci/*.gitlab-ci.yml` fragment reachable from root |
| CT-CI-08 | every job with artifacts is present in centralized PII `needs` with download enabled |

## Review finding fixed during verification

Coverage reports are merged by unique `package + class + line` identity, with a
line counted covered if any test suite covers it. This prevents unit and
integration reports from inflating or depressing the aggregate by counting the
same source line more than once.

The first exact security-container run exposed an SDK drift: floating image
`mcr.microsoft.com/dotnet/sdk:10.0` resolved to SDK 10.0.400 while `global.json`
requires 10.0.201. All .NET jobs are now pinned to `10.0.201`, and the config
self-test prevents future image/global.json mismatch. The complete security job
then exited 0 in the corrected image.

## Hosted evidence and production residual gates

- GitLab project/remote and separate GitHub mirror: PASS.
- Hosted MR pipelines `#2756409438` and `#2756495155`: PASS, 9/9 quality jobs
  and 98 tests; both merged only after pipeline success.
- Protected `main`, current `Allowed to push and merge: No one`, and
  **Pipelines must succeed**: PASS_SETTING_CURRENT. The explicit direct-push
  rejection is historical; a fresh post-remediation probe is `NOT_RUN`.
- Self-hosted runner `#55115499`, Docker-in-Docker and protected-variable
  metadata: PASS.
- GitLab Container Registry push/pull: PASS in job `15872915564`.
- Private GitLab Pages/access control: PASS in pipeline `#2756517379`; anonymous
  request redirects to GitLab authentication.
- Required independent MR approval/CODEOWNERS enforcement: `BLOCKED_EXTERNAL`;
  GitLab Free reports approvals as optional and the project has one member.
- Sales API/auth, real SIM/eSIM, customer calls, lab, staging, and production:
  `NOT_RUN`; P0-2 does not authorize or exercise them.

W-0061/G-GITLAB remains `BLOCKED_EXTERNAL` only for the required approval rule
and independent reviewer proof. Hosted CI/platform success must not be described
as production IVR readiness.

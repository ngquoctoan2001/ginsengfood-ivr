# GitLab CI baseline

This directory implements P0-2/W-0011. The root `.gitlab-ci.yml` is the only
pipeline entrypoint and includes `ci.gitlab-ci.yml` locally. No GitHub Actions or
unpinned remote include is active.

## Pipeline routing

`workflow:rules` creates a pipeline only for:

- Merge Request events;
- pushes to the default branch;
- deliberate manual `web` pipelines.

A feature-branch push without an MR does not create a second branch pipeline.
All quality jobs set `allow_failure: false` explicitly.
The .NET jobs pin `mcr.microsoft.com/dotnet/sdk:10.0.201` to the SDK selected by
`global.json`; the config self-test fails if those values drift apart.

## Gates

| Job | Gate |
| --- | --- |
| `ci_config_selftest` | workflow routing, local-fragment reachability, artifact topology, no active GitHub Actions |
| `openapi_lint` | Redocly lint, OpenAPI 3.1 parse/local refs, Target/current fixture schema validation, pinned contract drift |
| `build_test_dotnet` | locked restore, warnings-as-errors build, xUnit/JUnit/Cobertura, aggregate line coverage ≥ 60% |
| `lint_dotnet` | locked restore, pinned NSwag regeneration/drift check, analyzers, `dotnet format --verify-no-changes` |
| `build_lint_ui` | lockfile-based `npm ci`, ESLint, optional UI test script, production build |
| `security_scan` | NuGet High/Critical policy, npm High/Critical policy, checksum-verified Gitleaks 8.30.0 |
| `pii_scan` | raw-phone/address/dial-token scan under explicit `LC_ALL=C.UTF-8` |

Foundation coverage starts at 60%. It may only rise or receive a documented
generated-code exclusion; lowering the threshold or excluding handwritten
source requires a separately reviewed decision. Core slices target at least 80%
by P5.

NuGet and npm scans fail for High/Critical vulnerabilities. Lower severities
remain visible for triage. Gitleaks is a separate secret scanner and does not
replace the PII job.

`.gitleaksignore` contains four line-specific false-positive fingerprints from
documentation prose. No complete source file is exempted; moving or changing a
line invalidates its exception and forces a fresh review.

## PII artifact topology

The pipeline uses option A: a centralized `pii_scan`. GitLab jobs are isolated,
so it declares `needs` with `artifacts: true` for every artifact producer:

- `build_test_dotnet`;
- `build_lint_ui`;
- `openapi_lint`.

`CT-CI-08` fails if any job gains `artifacts:` without being added to this list.
`scan-pii.sh` scans text evidence and downloaded artifacts; binary screenshots
are deliberately skipped because byte-oriented grep cannot classify image
content. Match logs expose only file paths and `[REDACTED]`, never the matched
value. Evidence authors must keep PII out before creating any screenshot.

Patterns live in `pii-patterns.txt`, one ERE per line. Vietnamese case handling
uses literal per-character alternation rather than `grep -i` or multibyte
bracket expressions. `selftest-pii.sh` proves identical results under `C`,
`C.UTF-8`, and `POSIX`, including accented, unaccented, uppercase, and mixed-case
fixtures. ASCII word boundaries prevent the unaccented `ap` variant from
matching ordinary words such as `bootstrap` or `OpenAPI`.

## Negative pipeline switches

Use only on a manual test branch/MR. Each switch deliberately makes its quality
job fail:

| Variable | Expected failed job / test ID |
| --- | --- |
| `CI_OPENAPI_SELFTEST_INVALID=1` | `openapi_lint` / CT-CI-01 |
| `CI_DOTNET_SELFTEST_FAIL=1` | `build_test_dotnet` / CT-CI-02 |
| `CI_COVERAGE_SELFTEST_LOW=1` | `build_test_dotnet` / CT-CI-03 |
| `CI_SECRET_SELFTEST=1` | `security_scan` / CT-CI-04 |
| `CI_PII_SELFTEST_ARTIFACT=1` | `pii_scan` after downloading `build_test_dotnet` artifact / CT-CI-06c |

Never configure these variables as persistent project/group variables.

## Local verification

Run from the repository root:

```powershell
dotnet restore Ivr.sln --locked-mode
dotnet build Ivr.sln --configuration Release --no-restore
dotnet test Ivr.sln --configuration Release --no-build
dotnet format Ivr.sln --no-restore --verify-no-changes
npm --prefix admin-ui ci
npm --prefix admin-ui run lint
npm --prefix admin-ui run build
npm --prefix admin-ui audit --audit-level=high
npm --prefix deploy/ci ci
npm --prefix deploy/ci run test:config
npm --prefix deploy/ci run openapi:lint
npm --prefix deploy/ci run openapi:validate
npm --prefix deploy/ci run openapi:drift
npm --prefix deploy/ci run test:openapi-negative
./deploy/ci/scripts/regenerate-openapi.ps1
docker compose -f docker-compose.dev.yml --profile mocks config --quiet
```

`openapi:drift` is read-only and fails when a reviewed source hash changes.
After reviewing an intentional draft change, run
`openapi:accept-reviewed-draft`; this refreshes only the committed manifest and
human-readable report. It never changes `TARGET_CONTRACT_V1=DRAFT` or approves
the external Sales contract. The PowerShell regeneration script uses the
repository-local `NSwag.ConsoleCore` tool pinned in `dotnet-tools.json`; GitLab
runs the equivalent cross-platform `dotnet nswag` commands directly.

Coverage and NuGet policy tool examples:

```powershell
dotnet run --project deploy/ci/tools/Ivr.CiPolicy -- coverage <coverage-directory> 60
dotnet list Ivr.sln package --vulnerable --include-transitive --format json --no-restore > dotnet-vulnerabilities.json
dotnet run --project deploy/ci/tools/Ivr.CiPolicy -- vulnerabilities dotnet-vulnerabilities.json high
```

Run Gitleaks and the exact Linux/grep PII self-test through Docker:

```powershell
docker run --rm -v "${PWD}:/repo" -w /repo zricethezav/gitleaks:v8.30.0 dir . --config .gitleaks.toml --no-banner --redact
docker run --rm -v "${PWD}:/repo" -w /repo -e LC_ALL=C.UTF-8 debian:bookworm-slim sh deploy/ci/scripts/selftest-pii.sh
docker run --rm -v "${PWD}:/repo" -w /repo -e LC_ALL=C.UTF-8 debian:bookworm-slim sh deploy/ci/scripts/scan-pii.sh docs/evidence
```

`gitlab-ci-local` is optional. On Windows it may fail solely because the host has
no `/bin/bash`; that is an environment limitation, not a GitLab pipeline result.
A local render or YAML parse never replaces hosted GitLab evidence.

## GitLab project settings — hosted evidence

All items below are `NOT_RUN` and `BLOCKED_EXTERNAL` by W-0061 until a real
GitLab project and runner exist:

- project/mirror URL and a GitLab remote;
- Linux/amd64 runner identity capable of Docker pulls, outbound NuGet/npm/GitHub
  release downloads, and at least the images pinned in the pipeline;
- protected default branch with no direct push;
- Merge Request approvals and verified CODEOWNERS group paths;
- **Pipelines must succeed** merge check;
- masked/protected CI/CD variables and protected environments;
- Container Registry push/pull proof;
- one green hosted MR plus red negative pipelines from the switches above.

The root `CODEOWNERS` uses planned `@ginsengfood/ivr-*` group paths. Platform must
create or replace and verify those groups as part of W-0061; the file alone does
not prove approval enforcement.

Do not place credentials in YAML, caches, artifacts, logs, or evidence. GitLab
variables containing credentials must be masked, protected, scoped, and rotated
under the platform secret-store decision.

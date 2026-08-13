# W-0017 / P1-4 API documentation portal evidence

Date: 2026-08-13
Baseline: `main@a94b858`
Mode: local `MOCK`; non-production documentation only
Local implementation status: `TESTS_PASS`
Hosted GitLab Pages status: `NOT_RUN` / `BLOCKED_EXTERNAL` under `W-0061`

## Delivered scope

- Deterministic static Redoc pages for both reviewed OpenAPI sources.
- A developer portal home page with an explicit `NON-PRODUCTION ONLY` banner.
- Separate `TARGET_DRAFT` and current Golden Hour compatibility sections.
- Source-linked integration, API versioning and contract changelog guides.
- Pinned oasdiff `v1.26.1` changelog and breaking-change gates.
- Root-included GitLab CI verification, contract-diff and fail-closed Pages jobs.
- Committed rendered output plus a manifest of source hashes and renderer versions.

The portal does not claim that Target V1 is approved. It does not publish a
production portal, connect to Sales, place a call, or contain a real customer
phone number, full delivery address or dial token.

## Primary artifacts

- `docs/api/index.html` and its generated static assets/pages
- `docs/api/portal-manifest.json`
- `docs/api-changelog.md`
- `docs/api-versioning.md`
- `docs/integration-guide.md`
- `docs/api/changelog/*.md`
- `specs/api/openapi/baselines/*.yaml`
- `specs/api/openapi/changelog-baseline.json`
- `deploy/ci/docs.gitlab-ci.yml`
- `deploy/ci/scripts/build-api-docs.mjs`
- `deploy/ci/scripts/docs-selftest.mjs`
- `deploy/ci/scripts/generate-oasdiff-changelog.sh`
- `deploy/ci/scripts/selftest-oasdiff.sh`
- `api-portal-home.png`

## Contract boundaries shown by the portal

| Surface | State | Portal treatment |
| --- | --- | --- |
| IVR internal API | `TARGET_DRAFT` | Rendered from `ivr-order-confirmation.v1.yaml`; not approval evidence |
| Order Core callback | `TARGET_DRAFT` | Rendered from `order-core-ivr-callback.target-v1.yaml`; not the current Sales runtime |
| Golden Hour callback | current compatibility snapshot | Separate page pinned to Sales SHA `a3aad246`; seven-field payload, four-result enum and `X-Internal-Token` are not merged into Target V1 |

## Required P1-4 tests

| Test | Result | Proof |
| --- | --- | --- |
| `CT-DOC-01` generated-doc drift | PASS | A temporary render byte-matches all 11 committed portal files; a deliberate mutation is rejected |
| `CT-DOC-02` breaking contract | PASS | oasdiff rejects the fixture that removes `GET /tasks/{taskId}` when `--fail-on WARN` is armed |
| `UT-DOC-PII-03` no real PII | PASS | Source examples reject raw phone numbers, full street addresses and assigned dial tokens |

Additional portal checks passed: Target/current boundary labels, pinned Sales SHA,
all local generated links, root CI include topology, baseline hashes, renderer
versions and fail-closed non-production Pages rules.

## Commands and observed results

```text
npm --prefix deploy/ci run docs:build
API_DOCS_GENERATED=11

npm --prefix deploy/ci run test:docs
CT-DOC-01 PASS
UT-DOC-PII-03 PASS
DOC_BOUNDARY_PASS
DOC_LINKS_PASS
DOC_CI_TOPOLOGY_PASS
API_DOCS_SELFTEST_PASS

oasdiff version
v1.26.1

sh deploy/ci/scripts/selftest-oasdiff.sh
CT-DOC-02 PASS

oasdiff breaking <each-baseline> <each-current> --fail-on WARN
PASS; no changes detected for both initial baselines

npm --prefix deploy/ci run openapi:lint
PASS; both descriptions valid

npm --prefix deploy/ci run openapi:validate
OPENAPI_FILES_VALID=2
TARGET_TASKS_SCHEMA_VALID=9
SCHEMA_NEGATIVE_REJECTED=10
DOMAIN_NEGATIVE_SCHEMA_VALID=10
CURRENT_COMPAT_SCHEMA_VALID=1
CURRENT_COMPAT_TARGET_FIELD_REJECTED=1

npm --prefix deploy/ci run openapi:drift
OPENAPI_HASHES_PINNED=3
OPENAPI_HUMAN_DIFF_CURRENT=YES

official markdown-doc-reader mapper
MARKDOWN_FILES=405
LINKS_RESOLVED=372
UNRESOLVED_LINKS=0
```

The general byte-oriented PII scanner is intentionally not used to parse the
embedded Redoc schema bundle: it treats a schema property named `dial_token`
followed by `type: string` as if `string` were an assigned token. The P1-4 test
instead scans the documentation sources semantically and fails on actual raw
phone, full-address or assigned-token examples. The repository-wide scanner
still runs unchanged on its evidence/artifact scope.

## Visual check

`api-portal-home.png` records the locally rendered home page. The home page and
one Redoc contract page were opened over a local HTTP server. The expected
non-production banner, two Target draft cards, current compatibility card and
guide links rendered successfully.

## Hosted and runtime evidence not claimed

- GitLab hosted pipeline, runner execution and protected-branch enforcement:
  `NOT_RUN` under `W-0061`.
- GitLab Pages access control and hosted URL: `NOT_RUN`. Publication defaults to
  `API_DOCS_PUBLISH_NONPROD=NO`; Platform may arm `YES` only after access control
  is enabled and verified.
- Sales API/auth, CDC, SIM/eSIM, TTS and real customer call: `NOT_RUN`.
- Staging and production deployment: `NOT_RUN`.

Therefore P1-4 is locally complete at `TESTS_PASS`; this evidence must not be
used to infer Target V1 approval, hosted CI acceptance or production readiness.

Final GitNexus staged review: `LOW` risk, 33 files, 10 indexed documentation
symbols, zero affected IVR execution processes. New generator/CI files are not
yet represented as runtime symbols, so direct source, deterministic render and
CI self-tests remain the authoritative evidence for those files.

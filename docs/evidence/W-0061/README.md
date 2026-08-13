# W-0061 — GitLab platform provisioning evidence

Date: 2026-08-13

Gate: `G-GITLAB`

Status: `BLOCKED_EXTERNAL` — project/remote and the first hosted parser result exist, but a successful hosted pipeline, usable runner, protected default branch, merge enforcement and registry proof are still incomplete.

## Confirmed external progress

- GitLab project: `https://gitlab.com/nqt20102001/ginsengfood-ivr`
- GitLab remote: `origin=https://gitlab.com/nqt20102001/ginsengfood-ivr.git`
- GitHub mirror remote remains separate: `github=https://github.com/ngquoctoan2001/ivr.git`
- GitLab `main` was confirmed at `3c0aa13bf5460cbb44d9eb76e0f64fc31a0d49f1` before this remediation.
- First hosted pipeline: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2755964245`

## First hosted pipeline result

Pipeline `#2755964245` was created for commit `3c0aa13` and failed during configuration validation with zero jobs:

```text
jobs:build_test_dotnet:cache:key:files config has too many items (maximum is 2)
```

The failure is a real hosted GitLab parser result. It is not runner evidence because no job was created or assigned.

## Remediation in this change

- `.dotnet_cache.cache.key.files` now contains two entries: `global.json` and `dotnet-tools.json`.
- The cache remains advisory and continues to store `.nuget/packages/`; locked restore remains the correctness gate for application dependencies.
- `ci-config-selftest.mjs` now rejects any cache definition whose file-based key contains zero entries or more than two entries.
- The self-test also pins the expected `.NET` cache inputs so a later edit cannot silently reintroduce a third item.

GitLab documents `cache:key:files` as accepting at most two file paths or patterns: <https://docs.gitlab.com/ci/yaml/#cachekeyfiles>.

## Local verification

```text
npm --prefix deploy/ci run test:config
CT-CI-05 PASS
CT-CI-07 PASS
CT-CI-08 PASS
CACHE_KEY_FILES_PASS
SDK_IMAGE_PIN_PASS
TESTCONTAINERS_DIND_PASS
OPENAPI_CODEGEN_GATE_PASS
CI_CONFIG_SELFTEST_PASS
```

`npx --yes gitlab-ci-local@latest --list` remains `ENV_BLOCKED` on this Windows host because its renderer attempts to execute `/bin/bash`. This does not invalidate the deterministic Node self-test, but it also does not replace the required hosted GitLab rerun.

## Residual W-0061 checklist

| Requirement | State | Required evidence |
| --- | --- | --- |
| GitLab project and remote | PASS | project URL, `git remote -v`, remote branch SHA |
| CI configuration accepted | NOT_RUN after remediation | new hosted pipeline must create the expected jobs |
| Linux runner executes jobs | NOT_RUN | successful job pages including .NET/Testcontainers and security gates |
| Docker-in-Docker capability | NOT_RUN | `build_test_dotnet` runs the PostgreSQL Testcontainers suite |
| Protected default branch | NOT_RUN | branch rule export/screenshot; direct push disabled as approved |
| Merge-request approvals/CODEOWNERS | NOT_RUN | enforced approval rule evidence |
| Pipelines must succeed | NOT_RUN | GitLab merge-check setting evidence |
| Container Registry push/pull | NOT_RUN | registry path and successful authenticated push/pull evidence |
| Masked/protected variables | NOT_RUN | names/scope/protection only; never record secret values |

W-0011/P0-2 remains `TESTS_PASS` locally. Do not promote it to hosted acceptance until the residual checklist is evidenced.

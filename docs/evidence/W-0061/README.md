# W-0061 — GitLab platform provisioning evidence

Date: 2026-08-13

Gate: `G-GITLAB`

Status: `BLOCKED_EXTERNAL` — project/remote, a successful SaaS-hosted pipeline and an online self-hosted Docker runner now exist. Tagged execution on the new runner, protected default branch, merge enforcement, Pages access control and registry proof are still incomplete.

## Confirmed external progress

- GitLab project: `https://gitlab.com/nqt20102001/ginsengfood-ivr`
- GitLab remote: `origin=https://gitlab.com/nqt20102001/ginsengfood-ivr.git`
- GitHub mirror remote remains separate: `github=https://github.com/ngquoctoan2001/ginsengfood-ivr.git`
- GitLab `main` was confirmed at `3c0aa13bf5460cbb44d9eb76e0f64fc31a0d49f1` before this remediation.
- First hosted pipeline: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2755964245`
- First fully successful hosted pipeline after W-0085: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2756119982`

## First hosted pipeline result

Pipeline `#2755964245` was created for commit `3c0aa13` and failed during configuration validation with zero jobs:

```text
jobs:build_test_dotnet:cache:key:files config has too many items (maximum is 2)
```

The failure is a real hosted GitLab parser result. It is not runner evidence because no job was created or assigned.

## Successful hosted baseline

Pipeline `#2756119982` passed for commit `799501c6` after the W-0085 Linux path-portability remediation:

```text
Status: Passed
Jobs: 9/9
Tests: 98
Duration: 4m42s
Stages: validate, build, test, security, privacy
```

This proves the GitLab configuration and all current gates can pass on the SaaS Linux executor. It does not by itself prove the new self-hosted runner.

## Self-hosted Docker runner provisioning

The Windows development host runs Docker Desktop in Linux-container mode. No Ubuntu distribution is required. GitLab Runner `19.2.0` remains installed as the existing Windows service.

| Project | Runner | Executor | Tag | GitLab state before tagged pipeline |
| --- | --- | --- | --- | --- |
| IVR | `#55115499` / `ivr-docker-winhost` | Docker, privileged | `ginsengfood-docker` | Online |
| Things | `#55115556` / `things-docker-winhost` | Docker, privileged | `ginsengfood-docker` | Online |

The existing `ops-core-win` shell executor was preserved. Host scheduling is `concurrent=3`; every runner has `limit=1` and `request_concurrency=2`, so Ops Core, IVR and Things may each run one job without one project consuming all worker slots. Both Docker runners are project-locked, do not accept untagged jobs and are not marked protected until protected-branch policy is configured.

Runner authentication tokens are stored only in `C:\\GitLab-Runner\\config.toml`; token values must never be copied into repository evidence, logs or screenshots.

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
| CI configuration accepted | PASS | pipeline `#2756119982`, 9/9 jobs and 98 tests passed |
| Self-hosted runner control plane | PASS | runner `#55115499` online, version 19.2.0, project-locked tag `ginsengfood-docker` |
| Tagged Linux-container execution | NOT_RUN | successful job pages naming `ivr-docker-winhost` |
| Docker-in-Docker on self-hosted runner | NOT_RUN | `build_test_dotnet` runs the PostgreSQL Testcontainers suite on runner `#55115499` |
| Protected default branch | NOT_RUN | branch rule export/screenshot; direct push disabled as approved |
| Merge-request approvals/CODEOWNERS | NOT_RUN | enforced approval rule evidence |
| Pipelines must succeed | NOT_RUN | GitLab merge-check setting evidence |
| Container Registry push/pull | NOT_RUN | registry path and successful authenticated push/pull evidence |
| Masked/protected variables | NOT_RUN | names/scope/protection only; never record secret values |

W-0011/P0-2 remains `TESTS_PASS` locally. Do not promote it to hosted acceptance until the residual checklist is evidenced.

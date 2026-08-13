# W-0061 — GitLab platform provisioning evidence

Date: 2026-08-13

Gate: `G-GITLAB`

Status: `BLOCKED_EXTERNAL` — every requested GitLab platform control except required Merge Request approval enforcement is now proven. The remaining blocker is external: the project is on GitLab Free, the UI reports `Approval is optional`, and the project has only one member (`nqt20102001`, Owner). Required approval rules need GitLab Premium/Ultimate and an independent reviewer account.

## Confirmed external progress

- GitLab project: `https://gitlab.com/nqt20102001/ginsengfood-ivr`
- GitLab remote: `origin=https://gitlab.com/nqt20102001/ginsengfood-ivr.git`
- GitHub mirror remote remains separate: `github=https://github.com/ngquoctoan2001/ginsengfood-ivr.git`
- GitLab `main` was confirmed at `3c0aa13bf5460cbb44d9eb76e0f64fc31a0d49f1` before this remediation.
- First hosted pipeline: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2755964245`
- First fully successful hosted pipeline after W-0085: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2756119982`
- First fully successful tagged self-hosted pipeline: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2756183002`
- First protected-branch MR pipeline: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2756409438`
- Final Pages-remediation MR pipeline: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2756495155`
- Final protected `main` pipeline with Pages deploy: `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2756517379`

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

## Successful self-hosted baseline

Pipeline `#2756183002` passed for commit `fba3172` with all jobs routed by tag `ginsengfood-docker` to project runner `#55115499` / `ivr-docker-winhost`:

```text
Status: Passed
Jobs: 9/9
Tests: 98
Merged coverage: 91.5%
Duration: 19m37s
Queued: 3s
Stages: validate, build, test, security, privacy
```

Job `15871330726` (`ci_config_selftest`) and job `15871330732` (`build_test_dotnet`) both named runner `#55115499`. The build job passed the PostgreSQL Testcontainers suite through the privileged Docker executor, proving Docker-in-Docker use on the Windows-hosted Linux-container runner. Security job `15871330733` and the privacy gate also passed on the same tagged runner.

The sibling Things project independently passed pipeline `#2756187683` on runner `#55115556` / `things-docker-winhost`; its Docker-in-Docker job completed in `12m59s` with a `3s` queue. This is supporting host-capacity evidence, not a substitute for IVR acceptance.

## Protected branch and merge enforcement

GitLab project settings were configured and verified as follows:

| Control | Verified state |
| --- | --- |
| Protected branch | `main` protected |
| Allowed to merge | Maintainers |
| Allowed to push and merge | No one |
| Force push | Disabled |
| Merge check | `Pipelines must succeed` enabled |
| Skipped pipelines | Not considered successful |

A direct-push negative test was run after the rule was saved:

```text
git push origin HEAD:main
remote: GitLab: You are not allowed to push code to protected branches on this project.
! [remote rejected] HEAD -> main (pre-receive hook declined)
```

Merge Request `!1` (`codex/w0061-platform-enforcement`) passed pipeline `#2756409438` with 9/9 quality jobs and 98 tests. GitLab displayed `Pipeline must succeed`; auto-merge completed only after the pipeline passed and produced merge commit `b8044096`.

Merge Request `!2` (`codex/w0061-evidence-closure`) passed pipeline `#2756495155` with the same 9/9 quality jobs and 98 tests. It remediated the Pages artifact-root defect described below and merged as `ca10ebb4` only after the pipeline passed.

## Protected variables

The following project variables are stored in GitLab; secret values are intentionally excluded from repository evidence:

| Variable | Protected | Masked | Hidden | Purpose |
| --- | --- | --- | --- | --- |
| `IVR_W0061_PROTECTED_PROBE` | Yes | Yes | Yes | prove secret-variable protection metadata without disclosing the value |
| `API_DOCS_PUBLISH_NONPROD` | Yes | No | No | arm private non-production Pages publication only on protected refs |

No credential value was printed, copied into YAML, committed, or recorded in this evidence.

## Container Registry proof

Manual job `15872915564` in protected-`main` pipeline `#2756451810` passed on runner `#55115499`. The job authenticated with the short-lived `CI_JOB_TOKEN`, pushed a metadata-only image, removed its local copy, pulled it again, and verified the revision label.

GitLab Container Registry shows repository `ginsengfood-ivr/w0061-proof` with one tag under project registry path `/container_registry/12115445`. The job neither used nor exposed a long-lived personal/deploy token.

## Pages access-control and publication proof

The project is private and GitLab Pages access is configured as `Only Project Members`. Protected variable `API_DOCS_PUBLISH_NONPROD` armed the default-branch-only publish job.

The first post-enforcement `main` pipeline `#2756451810` is intentionally retained as failed evidence: all quality jobs, `api_docs_pages` script, and Registry smoke passed, but generated `pages:deploy` failed because the script wrote to `deploy/ci/public` while GitLab expected repository-root `public/`. Merge Request `!2` changed the argument to `--output ../../public` and added a topology regression assertion.

Final pipeline `#2756517379` for commit `ca10ebb4` passed with 12 jobs and 98 tests. Job `15873355825` produced `API_DOCS_GENERATED=11`, resolved `API_DOCS_OUTPUT=public`, uploaded 12 matching files with HTTP 201, and the generated `pages:deploy` job passed.

The deployed private portal is `https://ginsengfood-ivr-0332fa.gitlab.io/`. GitLab Pages lists the deployment as active from root `/public`, 12 files, 53.9 KiB. An authenticated project-member browser loaded the page with title `Ginsengfood IVR Developer Portal`. An anonymous HTTP request returned `302 Found` to `https://projects.gitlab.io/auth?...`, proving the portal is not anonymously readable.

## Approval enforcement blocker

The Merge Request page currently reports `Approval is optional`. GitLab's approval documentation states that approvals on GitLab Free are optional and do not prevent merging; required approval rules are available in Premium/Ultimate. The project currently has one member, the same Owner who authors and merges these changes, so an independent approval cannot be demonstrated even after enabling a required rule.

Authoritative references:

- <https://docs.gitlab.com/user/project/merge_requests/approvals/>
- <https://docs.gitlab.com/user/project/merge_requests/approvals/rules/>

Exact external closure actions:

1. upgrade the namespace/project to GitLab Premium or Ultimate;
2. invite at least one independent reviewer with sufficient project access;
3. create an approval rule for protected branches requiring at least one approval, and configure Code Owner approval if desired;
4. open a small Merge Request, capture the blocked-before-approval state, obtain the independent approval, then capture the merge-after-green-pipeline-and-approval state.

## Self-hosted Docker runner provisioning

The Windows development host runs Docker Desktop in Linux-container mode. No Ubuntu distribution is required. GitLab Runner `19.2.0` remains installed as the existing Windows service.

| Project | Runner | Executor | Tag | Verified state |
| --- | --- | --- | --- | --- |
| IVR | `#55115499` / `ivr-docker-winhost` | Docker, privileged | `ginsengfood-docker` | Online; tagged pipeline PASS |
| Things | `#55115556` / `things-docker-winhost` | Docker, privileged | `ginsengfood-docker` | Online; tagged pipeline PASS |

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

`npx --yes gitlab-ci-local@latest --list` remains `ENV_BLOCKED` on this Windows host because its renderer attempts to execute `/bin/bash`. This does not invalidate the deterministic Node self-test. Hosted GitLab pipelines listed above are the authoritative execution evidence instead.

## Residual W-0061 checklist

| Requirement | State | Required evidence |
| --- | --- | --- |
| GitLab project and remote | PASS | project URL, `git remote -v`, remote branch SHA |
| CI configuration accepted | PASS | pipelines `#2756119982` and `#2756183002`; 9/9 jobs and 98 tests passed |
| Self-hosted runner control plane | PASS | runner `#55115499` online, version 19.2.0, project-locked tag `ginsengfood-docker` |
| Tagged Linux-container execution | PASS | pipeline `#2756183002`; jobs name `#55115499` / `ivr-docker-winhost` |
| Docker-in-Docker on self-hosted runner | PASS | job `15871330732` passed the PostgreSQL Testcontainers suite on runner `#55115499` |
| Protected default branch | PASS | `main`: merge Maintainers, push No one, force push off; direct-push negative test rejected |
| Merge-request approvals/CODEOWNERS | BLOCKED_EXTERNAL | current Free tier shows optional approvals; only one project member; upgrade plus independent reviewer required |
| Pipelines must succeed | PASS | setting enabled; MR `!1`/`!2` merged only after green pipelines |
| Container Registry push/pull | PASS | job `15872915564`; registry repository `ginsengfood-ivr/w0061-proof` |
| Pages access control | PASS | private project, `Only Project Members`; pipeline `#2756517379`, job `15873355825`, anonymous redirect to GitLab auth |
| Masked/protected variables | PASS | `IVR_W0061_PROTECTED_PROBE` protected/masked/hidden; `API_DOCS_PUBLISH_NONPROD` protected; values not recorded |

W-0011/P0-2 now has hosted pipeline, runner, branch protection, merge-check, Registry, Pages and protected-variable evidence. W-0061 remains `BLOCKED_EXTERNAL` solely because required independent MR approval cannot be enforced or demonstrated on the current GitLab Free/single-member project. This status must not be weakened to `ACCEPTED` merely because optional self-approval is available.

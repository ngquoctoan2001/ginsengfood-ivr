# W-0175 — R0 remote-head, hosted-CI and clean-checkout audit

Date: `2026-09-04`
Scope: read-only remote/range audit, detached exact-head verification, and a
repository EOL policy fix. No push, pipeline trigger, runtime change, external
call, production data access, or release-gate promotion was performed.

## 1. Immutable baseline and remote state

| Ref | Exact SHA | Verification |
| --- | --- | --- |
| local `main` / `HEAD` at audit start | `c213bf7663708dfca7184bf443e66d6552e2daea` | `git rev-parse HEAD` |
| GitLab `origin/main` | `c213bf7663708dfca7184bf443e66d6552e2daea` | `git ls-remote origin refs/heads/main` |
| GitHub `github/main` | `c213bf7663708dfca7184bf443e66d6552e2daea` | `git ls-remote github refs/heads/main` |
| last independently clean candidate | `a07780c` | ancestor of `c213bf7`; its historical verdict is not inherited by later commits |

State correction after the detached audit: while W-0175 was running, another
local process committed W-0171 as
`710c81ca6022c3a9c33731385660f2833769c68d`. A fresh `ls-remote` still showed
both GitLab and GitHub at `c213bf7`; therefore local `main` is now one commit
ahead of both remotes. `710c81c` is a docs reconciliation commit and was not
silently treated as part of the detached `c213bf7` result.

`a07780c..c213bf7` contains 11 commits. The only application-facing changes in
that range are the W-0147 `Retry-After` correction and the admin UI type-generation
step; the remaining commits are validators, evidence, documentation, and cleanup.
GitNexus compare was advisory and reported `LOW`, `1,107` changed symbols and
`0` affected execution processes. Git itself is the file-level authority and
reported 173 changed paths for the range.

## 2. Detached exact-head verification

The following results were collected from a detached worktree at exact
`c213bf7663708dfca7184bf443e66d6552e2daea`, not from the shared dirty checkout.

| Lane | Exact result | Verdict |
| --- | --- | --- |
| .NET Contract | `24/24` passed | `PASS` |
| .NET Unit | `497/497` passed | `PASS` |
| .NET Integration | `51` passed; `185` stopped at Testcontainers fixture because `npipe://./pipe/docker_engine` was unavailable | `ENV_BLOCKED / ASSERTIONS_NOT_RUN` for Docker cases |
| .NET Chaos | 7 cases stopped during Docker fixture startup | `ENV_BLOCKED / ASSERTIONS_NOT_RUN` |
| Admin UI | clean `npm ci`; lint, `next typegen && tsc --noEmit`, Vitest `176/176`, and production build passed; npm reported 0 vulnerabilities | `PASS` |
| Capacity intake chain | full self-test passed: valid `1`, mode guard `2`, template guard `1`, receipt guard `7`, receipt verifier `12`, ledger guard `9`, checkpoint guard `13`, refusals `14` | `PASS` |
| API docs / CI config | `API_DOCS_SELFTEST_PASS`; CI config self-test passed after clean deploy/CI `npm ci` | `PASS` |
| W-0164 routing validator | self-test rejected current source pin because M8-12 bytes no longer matched the pinned SHA | `FAIL — PIN_DRIFT` |
| W-0165 response validator | self-test rejected current artifact manifest pin | `FAIL — PIN_DRIFT` |
| Generated traceability | `generate-test-traceability --check` failed; generator rewrote 476 rows | `FAIL — CLEAN_WINDOWS_EOL_DRIFT` |
| Generated gate/readiness mirrors | `gate-status.mjs` failed mirror comparison; generator rewrote 11 gates / 167 work items / 23 decisions | `FAIL — CLEAN_WINDOWS_EOL_DRIFT` |
| Security | Gitleaks `8.30.0`, NuGet HIGH policy and both npm HIGH audits passed; negative fake-PAT control was rejected; 153-commit history scan found no leak | `PASS` |

The Integration/Chaos result is an environment result, not a test-regression
verdict. Docker Desktop processes could be started, but the engine pipe never
became available and the Windows service could not be started from this
non-elevated shell.

## 3. Root cause and bounded remediation

On exact `c213bf7`, `git check-attr text eol` returned `unspecified` for the three
generated release mirrors and the W-0164/W-0165/W-0170 hash-bound JSON/Markdown
artifacts. With `core.autocrlf=true`, a clean Windows checkout materialized CRLF;
the generators wrote canonical LF, so byte comparisons became dirty without a
semantic change.

W-0175 adds narrow `text eol=lf` rules in `.gitattributes` for:

- `docs/traceability-tests.md`, `docs/release/gate-status.yaml`, and
  `docs/release/readiness-board.md`;
- W-0164, W-0165, and W-0170 JSON/text provenance artifacts;
- M8-12 and M8-13, whose bytes are pinned by the external-decision validators.

The shared W-0170 changes rotated the M8-12/M8-13/manifest pins before W-0173
and W-0174 completed. A current-tree regression after those work items finished
found a second, legitimate drift: M8-07 is now
`c4bb79fa8b06c0f06a8b959b698084f9d02444a5cb1a25e14413b87ae74c1aa0`, while
the W-0170 manifest still pins
`72ddb92347fc88fad8607d2f9ceef40546274f828642a041fe021049c6a7e426`.

Current-tree bounded regression:

```text
W0164_SELFTEST_PASS template=1 valid=2 refusals=19
W0165_VALIDATION_FAILED: M8-07 drifted from the artifact manifest
W0170_VALIDATION_FAILED: W-0165 prerequisite validation failed
API_DOCS_SELFTEST_PASS
CI_CONFIG_SELFTEST_PASS
TEST_TRACEABILITY_CURRENT=485
```

This is why W-0175 does not rotate pins while another work item is still
changing a manifest member. The final pin rotation and EOL policy must land in
the same next candidate before the validators can be judged on a clean checkout.

W-0175 bounded verification after the policy change:

```text
git check-attr: text=set / eol=lf for every target path
GATE_STATUS_PASS: 11 gates / 173 work items / 23 open decisions / production=false
PII_SCAN_PASS: W-0175 evidence and M8 worklist, 2/2 files
git diff --check (W-0175 scope): PASS
GitNexus detect-changes (aggregate dirty tree): LOW / 29 files / 81 symbols / 0 processes
```

The GitNexus number is advisory for the shared checkout and is not attributed
solely to W-0175. W-0175 itself changes no runtime symbol.

## 4. Hosted CI evidence boundary

- `glab` and `gh` are not installed in the audit environment.
- Public GitLab project/pipeline API access returned `404` for the private project.
- Reusing the configured Git credential via `PRIVATE-TOKEN`, Bearer, and Basic
  requests returned `401`, `403`, and `404`; no usable `read_api` permission was
  available. No credential value was logged or stored in this evidence.
- GitHub Check Runs for the SHA returned zero runs; this does not prove the GitLab
  pipeline state.

Therefore hosted CI remains `AUTH_BLOCKED / UNKNOWN`. No PASS, FAIL, or NOT_RUN
claim is inferred from the absence of readable pipeline data.

## 5. Decision and next action

Current R0 verdict:

`REMOTE_PAIR_SYNCED / LOCAL_AHEAD_ONE / REMOTE_HEAD_AUDITED / EXACT_HEAD_RELEASE_BLOCKED / HOSTED_CI_AUTH_BLOCKED / REAL_CUSTOMER_CALL_ALLOWED=NO`

Next action, in order:

1. Stabilize and review the existing W-0169..W-0174 shared WIP without mixing
   unrelated files accidentally into R0.
2. After no manifest member is changing, rotate W-0170 once more for the final
   M8-07 bytes. Review `710c81c` plus the remaining WIP, commit a scoped
   candidate containing the accepted set and W-0175 LF rules, regenerate the
   three mirrors, then verify that exact commit from a new detached clean
   worktree.
3. Make Docker Engine available and rerun full Integration + Chaos on that exact
   candidate.
4. Give the release reviewer a GitLab pipeline URL or a token/session with
   `read_api`, then bind the hosted pipeline verdict to the exact candidate SHA.
5. Do not release `c213bf7` and do not enable real-customer calls from this local
   evidence.

# W-0086 — Shallow-clone Gitleaks fingerprint remediation

Date: 2026-08-13

Origin: `UNPLANNED` hosted MR pipeline failure during W-0061 evidence closure

Status: `TESTS_PASS`; hosted rerun pending

## Failure evidence

Merge Request `!3` pipeline `#2756568239` passed validate, build, 98 tests and
continued to privacy, but job `15873689410` failed because Gitleaks found one
`generic-api-key` match in immutable historical commit
`b3a93aac90099169c1bc5df0afa6b216fa50a43c`:

```text
File: prompt/phase-0-foundation/P0-2-ci-baseline-quality-gates.md
Line: 75
Historical text: planning prose listed several expected CI evidence artifacts.
Fingerprint: b3a93aac90099169c1bc5df0afa6b216fa50a43c:prompt/phase-0-foundation/P0-2-ci-baseline-quality-gates.md:generic-api-key:75
```

The match is planning prose, contains no credential, and is byte-identical in
an immutable historical commit. The previous full-history local scan did not
surface this fingerprint, while a fresh `--depth 20` clone reproduced it. This
is a shallow-history boundary effect, not a leaked secret and not a reason to
weaken the Gitleaks rule globally.

## Remediation

`.gitleaksignore` now contains the exact commit/file/rule/line fingerprint. No
file-wide allowlist, regex relaxation, path exclusion or secret-scanner bypass
was added. The existing deliberate fake GitHub PAT negative test remains armed
and must still produce Gitleaks exit 42.

## Verification

- local full-history Gitleaks 8.30.0 scan: PASS, no leaks;
- fresh GitLab branch clone at depth 20: failure reproduced with exactly one
  redacted `generic-api-key` match before remediation;
- the same depth-20 clone with the reviewed `.gitleaksignore`: PASS, 20 commits,
  19.93 MB, no leaks;
- first hosted rerun pipeline `#2756604515`, job `15873949053`: FAILED because
  this evidence file repeated the scanner-triggering planning phrase;
- exact synthetic merge ref `refs/merge-requests/3/merge` reproduced one
  redacted match in this file at depth 20/21 commits; wording was rewritten
  instead of adding another ignore entry;
- second hosted rerun pipeline `#2756636651`, job `15874176742`: FAILED even
  though a fresh clone of its exact synthetic merge ref passed 21 commits,
  19.93 MB and no leaks. The persistent runner worktree retained the orphaned
  pre-amend commit outside the pipeline commit ancestry;
- the history scan now validates and anchors Gitleaks `--log-opts` to
  `${CI_COMMIT_SHA:-HEAD}`. This keeps the complete reachable history in scope
  while excluding stale local refs unrelated to the checked pipeline commit;
- `npm --prefix deploy/ci run test:config`: PASS;
- GitLab MR pipeline rerun: required before promotion to `ACCEPTED`.

This work changes only a line-specific false-positive fingerprint. It does not
change application runtime, Sales integration, SIM/eSIM behavior or customer
calling policy.

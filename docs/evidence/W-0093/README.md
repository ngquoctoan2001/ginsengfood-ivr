# W-0093 — Phase 1/2 governance and evidence truth remediation

Status: `EVIDENCE_SUBMITTED`; platform enforcement remains `BLOCKED_EXTERNAL`

## Live platform correction

Authenticated GitLab settings were read and remediated on 2026-08-14. The first
read proved the Opus finding: Maintainers could push directly. Only that branch
permission was changed. Current facts after the saved change:

- `main` is protected;
- merge is allowed for Maintainers;
- push and merge is allowed for `No one`, so the current setting enforces
  no-direct-push;
- force push is disabled;
- `Pipelines must succeed` is enabled;
- project merge method is `Merge commit`;
- the project has exactly one direct member, `nqt20102001` (`Owner`);
- independent required approval is unavailable with the current single-owner /
  current-tier setup.

Therefore `E-14` was valid at review time: old W-0061 no-direct-push evidence was
not current. Its inference from zero merge commits alone was not sufficient in
general; the live pre-remediation branch rule and merge method provided the
decisive evidence. The setting drift is now closed as `PASS_SETTING_CURRENT`.
W-0061 and the tracker preserve the historical hosted artifacts, the observed
drift and the post-remediation state without claiming a new rejected push.

A new rejected direct-push probe remains `NOT_RUN` because a meaningful probe
needs a disposable divergent commit and must not risk mutating `main` if the
rule regresses. Exact external closure remains: add an independent reviewer,
require at least one approval, then record a blocked-before/merged-after green
MR. No account invitation or service-tier mutation was made without the missing
reviewer/owner data.

## Evidence corrections

- W-0014/W-0015/W-0016 distinguish historical hosted pipeline proof from the
  current tree and current branch controls.
- W-0017 owner acceptance is explicitly not independent MR approval.
- W-0019 now includes sellable, do-not-call and fail-closed block samples.
- W-0023 uses real table `ivr_result_callbacks` and configuration path
  `Ivr:CallbackDelivery:Enabled`.
- W-0064 reports real fixture mode/environment.
- W-0065/W-0066 preserve historical inflated coverage but publish corrected
  `88.80% (10,350/11,656)` with one source-generator class explicitly excluded
  by policy.
- W-0018 tracker columns are aligned.

Local evidence truth is closed and the current no-direct-push setting is
`PASS_SETTING_CURRENT`. No rejected-push execution, production readiness or
independent approval is claimed.

## Final local evidence gates

- official Markdown map: `431` files, `378` resolved links, `0` unresolved;
- PII Linux/locale self-test: PASS; final evidence/artifact scan `225` text
  files, `2` binary files skipped by policy;
- Gitleaks 8.30.0: working tree `66.43 MB` PASS and Git history `43` commits /
  `22.15 MB` PASS;
- NuGet High vulnerability policy and both npm High audits: PASS;
- corrected coverage policy negative control: `50.00%` remains rejected while
  two generated/migration classes are excluded; actual full coverage
  `88.80% (10,350/11,656)` PASS;
- final Release build `0 warnings / 0 errors`; full regression `281/281 PASS`.

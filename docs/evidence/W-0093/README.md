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

## Hosted remediation evidence

Merge Request `!4` carries the W-0087..W-0094 remediation. Three failed hosted
pipelines were diagnosed rather than relabelled as successful: `#2760049871`
found a stale integration-test package lock, `#2760097290` exposed moving
Gitleaks fingerprints at the depth-20 clone boundary, and `#2760190045` proved
that a raw PII fixture had leaked into a downloaded JUnit display name.

After the targeted fixes, pipeline `#2760238052` on commit `001d2f57` passed
all 9 jobs. Hosted evidence is Release `0 warnings / 0 errors`, `281/281` tests
(`21` contract + `168` unit + `92` integration), and corrected coverage
`88.80% (10,350/11,656)`. Security job `15900070454` scanned the full `47`
commit ancestry / `22.44 MB` with no leaks and emitted `SECURITY_SCAN_PASS`.
PII job `15900070455` passed all scanner self-tests and the real downloaded
artifact scan (`65` text files, `2` binary files skipped), then emitted
`PII_SCAN_PASS`.

This closes the hosted code/CI evidence for the remediation branch. It does not
close W-0061: required independent approval remains `BLOCKED_EXTERNAL`, and a
fresh rejected direct-push probe remains `NOT_RUN`.

## Final local evidence gates

- official Markdown map: `431` files, `378` resolved links, `0` unresolved;
- PII Linux/locale self-test: PASS; final evidence/artifact scan `225` text
  files, `2` binary files skipped by policy;
- Gitleaks 8.30.0: working tree `66.45 MB` PASS and Git history `47` commits /
  `22.44 MB` PASS;
- NuGet High vulnerability policy and both npm High audits: PASS;
- corrected coverage policy negative control: `50.00%` remains rejected while
  two generated/migration classes are excluded; actual full coverage
  `88.80% (10,350/11,656)` PASS;
- final Release build `0 warnings / 0 errors`; full regression `281/281 PASS`.

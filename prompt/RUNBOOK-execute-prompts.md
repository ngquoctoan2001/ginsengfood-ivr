# RUNBOOK — Execute IVR Prompts P0–P11

Trạng thái: `LIVING` · Cập nhật: `2026-08-12`.

## Outcome ladder

1. `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS`: P0–P7/P5 evidence, fake Sales/mock SIM, no external truth claimed.
2. `LAB_REAL_SIM_VERIFIED`: P8 with one real SIM, allowlisted test numbers, kill switch; no customers.
3. `REAL_SALES_INTEGRATION_VERIFIED`: producer/speech/dial-token/callback/auth/no-answer verified on sandbox/staging.
4. `PRODUCTION_REAL_ELIGIBLE`: 32 eSIM capacity plus legal/security/release gates accepted.

## Before every prompt

1. Read governance, Target V1 draft, relevant spec/OpenAPI and defaults.
2. Verify repository/current diff and preserve unrelated work.
3. Open canonical tracker; allocate/select Work ID and mark `IN_PROGRESS` with baseline/prereqs.
4. List external inputs as real/mock/open. If missing, create the next Work ID and continue only behind an explicit mock/port.

## During every prompt

- Implement only the prompt scope; add tests/evidence with traceability.
- Append meaningful checkpoints to the tracker Activity Log.
- Any unplanned bug/refactor/API/data/deployment work receives the next global Work ID with `Origin=UNPLANNED`, discovery source, impact, owner, priority and dependency.
- Contract changes update spec + OpenAPI + fake fixtures + tests in the same work item, or remain blocked.

## After every prompt

1. Run required build/lint/unit/integration/contract/E2E checks proportionate to scope.
2. Record exact command/result/artifact/evidence in tracker.
3. Record residual external/live gates separately.
4. Set status truthfully: code complete is not evidence accepted.
5. Reviewer/owner alone moves critical work to `ACCEPTED`.

## Status vocabulary

`PLANNED`, `NOT_STARTED`, `IN_PROGRESS`, `CODE_DONE`, `TESTS_PASS`, `EVIDENCE_SUBMITTED`, `ACCEPTED`, `BLOCKED_INTERNAL`, `BLOCKED_EXTERNAL`, `DEFERRED_TARGET`, `N/A`, `CANCELLED`.

## Execution order

- P0 foundation; P1 contracts/data; P2 runtime with fake Sales/mock SIM; P3 UI and P5 tests can overlap after their prerequisites.
- P4 builds target/current adapters behind mocks first; real provider wiring waits for Sales/auth inputs.
- P6/P7 observability/deployment complete in MOCK.
- P8 first proves one real SIM lab, not real-customer pilot.
- P10/P11 collect privacy/capacity/contracts/release artifacts from day one.
- P9 customer release only after all production gates.

## Stop rules

Stop the affected path and mark tracker when: source priority conflicts; contract/policy is invented; raw phone/full address can leak; IVR could transition order/send notification; MOCK could dial real; LAB could call outside allowlist; provider response is treated as success without semantic validation; required evidence is absent.

## External dependency rule

External missing data never disappears from the plan. Create/maintain tracker work plus `integration-requirements` entry, owner, mock fallback, exact requested schema/sample/credential and closure evidence. Mock completion may unblock downstream code but does not close the external item.

## Docs validation after a batch

Run the Markdown mapper from the installed `markdown-doc-reader` skill; require zero unresolved internal links. Parse both OpenAPI YAML files, validate refs/schemas where tooling exists, then inspect git diff/status. Record the commands/results in tracker.

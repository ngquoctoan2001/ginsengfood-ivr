# IVR Prompt Library — Master Index

Trạng thái: `READY_FOR_IMPLEMENTATION_WITH_MOCKS` · Cập nhật: `2026-08-12`.

Đọc theo thứ tự: [Governance](README-governance.md) → [Runbook](RUNBOOK-execute-prompts.md) → [Target V1](../plan/ivr-orther/target-contract-v1-draft.md) → [Defaults](./_execution/defaults-and-confirmations.md) → [Canonical tracker](./_execution/prompt-execution-tracker.md).

Mục tiêu gần: implementation-complete bằng fake Sales + mock SIM. Sau đó test 1 SIM thật/allowlist. Production cần Sales API/auth/policy/legal và target 32 eSIM được nghiệm thu.

## Execution map

```text
P0 Foundation -> P1 Contracts/Data -> P2 Runtime MOCK -> P3 UI
                                      |              -> P5 Quality
                                      -> P4 provider adapters (mock first; real when available)
                         P6 Observability -> P7 Deploy
External closure P11 runs from day one
P8 = one real SIM lab only -> real Sales/staging -> P9 production gate
P10 privacy/DR/capacity/SLA runs across all phases
```

Every prompt must update the Work ID assigned in the canonical tracker before/during/after execution. Any unplanned work gets the next global Work ID.

## Phase 0 — Foundation

| ID | Prompt | Target V1 addition |
| --- | --- | --- |
| P0-1 | [repo/solution bootstrap](phase-0-foundation/P0-1-repo-and-solution-bootstrap.md) | standalone .NET repo; fake providers |
| P0-2 | [CI quality gates](phase-0-foundation/P0-2-ci-baseline-quality-gates.md) | two OpenAPI drift/lint; tracker/evidence checks |
| P0-3 | [cross-cutting foundation](phase-0-foundation/P0-3-crosscutting-foundation.md) | mock JWT, PII guards, idempotency/correlation |
| P0-4 | [flags/config](phase-0-foundation/P0-4-feature-flag-config-platform.md) | MOCK/LAB/PROD modes, provider flags, kill switches |

## Phase 1 — Contracts and Data

| ID | Prompt | Target V1 addition |
| --- | --- | --- |
| P1-1 | [OpenAPI/codegen](phase-1-contracts-data/P1-1-openapi-codegen-contract-scaffold.md) | task + Sales callback target/current-compat clients |
| P1-2 | [Postgres migrations](phase-1-contracts-data/P1-2-database-migrations-postgres.md) | policy/speech/dial token snapshots; no exact D-10 checks |
| P1-3 | [domain/DTO mapping](phase-1-contracts-data/P1-3-domain-model-dto-mapping.md) | program matrix, policy registry, speech/privacy providers |
| P1-4 | [API docs portal](phase-1-contracts-data/P1-4-api-docs-developer-portal.md) | clear CURRENT_COMPAT vs TARGET_DRAFT |

## Phase 2 — Core runtime in MOCK

| ID | Prompt | Target V1 addition |
| --- | --- | --- |
| P2-1 | [task intake](phase-2-core-runtime/P2-1-task-intake.md) | GH ONLINE + 24/7 COD, required flag, speech/version/token |
| P2-2 | [eligibility/blockers](phase-2-core-runtime/P2-2-eligibility-blockers.md) | Sales snapshot truth; fail-closed |
| P2-3 | [scheduler/policy](phase-2-core-runtime/P2-3-scheduler-attempt-policy.md) | versioned config; candidate only MOCK/LAB |
| P2-4 | [mock SIM adapter](phase-2-core-runtime/P2-4-sim-adapter-mock.md) | speech render, fake dial-token resolver, deterministic calls |
| P2-5 | [DTMF normalizer](phase-2-core-runtime/P2-5-dtmf-normalizer.md) | candidate mapping until real lab |
| P2-6 | [Sales callback](phase-2-core-runtime/P2-6-order-core-callback.md) | generic semantic ACK/outbox + GH compat |
| P2-7 | [script/content](phase-2-core-runtime/P2-7-script-content-management.md) | items/qty/total/short area; privacy approval |

## Phase 3 — Next.js admin

| ID | Prompt |
| --- | --- |
| P3-1 | [UI foundation](phase-3-admin-ui/P3-1-ui-foundation.md) |
| P3-2 | [dashboard/call log/detail](phase-3-admin-ui/P3-2-dashboard-calllog-detail.md) |
| P3-3 | [config/integration/roles](phase-3-admin-ui/P3-3-config-integration-roles.md) |
| P3-4 | [reporting/analytics](phase-3-admin-ui/P3-4-reporting-analytics-ui.md) |

UI must show provider/mode, mock-vs-real evidence, policy version, callback semantic ACK, masked identifiers and external blockers; it cannot override Sales decisions.

## Phase 4 — Integration adapters

| ID | Prompt | State |
| --- | --- | --- |
| P4-1 | [Sales wiring](phase-4-integration/P4-1-order-core-wiring.md) | build behind fakes now; real blocked on Sales contract/auth |
| P4-2 | [sellable contract](phase-4-integration/P4-2-ops-sellable-gate.md) | Sales remains orchestration owner |
| P4-3 | [CRM eligibility](phase-4-integration/P4-3-crm-eligibility-events.md) | mock/provider; fail-closed |
| P4-4 | [shared auth/audit](phase-4-integration/P4-4-shared-auth-audit.md) | mock JWT now; prod JWT/mTLS pending |
| P4-5 | [post-decision notification](phase-4-integration/P4-5-post-decision-notification.md) | `DEFERRED_TARGET`: disabled/no-op, prove no delivery |
| P4-6 | [opt-out feedback](phase-4-integration/P4-6-opt-out-feedback-loop.md) | deferred/manual review; no automatic consent mutation |

## Phase 5–7 — Quality, observability and deployment

| Phase | Prompts |
| --- | --- |
| P5 | [unit/integration](phase-5-quality/P5-1-unit-integration-tests.md), [contract/E2E](phase-5-quality/P5-2-contract-e2e-tests.md), [performance/security](phase-5-quality/P5-3-performance-security-tests.md), [review gate](phase-5-quality/P5-4-code-review-gate.md), [a11y/i18n](phase-5-quality/P5-5-accessibility-i18n-qa.md) |
| P6 | [telemetry](phase-6-observability/P6-1-logging-metrics-tracing.md), [SLO/alerts](phase-6-observability/P6-2-dashboards-slo-alerting.md), [chaos](phase-6-observability/P6-3-chaos-resilience-gamedays.md) |
| P7 | [Docker/Compose](phase-7-deployment/P7-1-docker-images-compose.md), [Helm/K8s](phase-7-deployment/P7-2-kubernetes-helm.md), [CI/CD](phase-7-deployment/P7-3-cicd-pipeline.md), [canary](phase-7-deployment/P7-4-progressive-delivery-canary.md), [secret rotation](phase-7-deployment/P7-5-secret-rotation-key-lifecycle.md) |

P5 must test both programs, speech/PII, policy versions, three modes, target ACKs and notification no-op. Compose must include fake Sales/mock SIM/mock JWT.

## Phase 8–9 — Lab then production

| ID | Prompt | Gate |
| --- | --- | --- |
| P8-1 | [real SIM adapter](phase-8-sim-pilot/P8-1-real-sim-adapter.md) | vendor protocol + 1 real SIM; allowlist only |
| P8-2 | [lab runbook](phase-8-sim-pilot/P8-2-pilot-runbook.md) | no real customers; keep real-customer flag NO |
| P9-1 | [release gate](phase-9-release-ops/P9-1-release-gate-execution.md) | real Sales/auth/policy/32 eSIM/legal/evidence |
| P9-2 | [cutover/ops](phase-9-release-ops/P9-2-cutover-ops-runbook.md) | only after P9-1 acceptance |

## Phase 10–11 — Maturity/external closure

| Phase | Prompts |
| --- | --- |
| P10 | [privacy](phase-10-compliance-maturity/P10-1-pdpa-privacy-compliance.md), [data/DR](phase-10-compliance-maturity/P10-2-data-governance-backup-dr.md), [capacity/cost](phase-10-compliance-maturity/P10-3-capacity-cost-sim-sizing.md), [analytics](phase-10-compliance-maturity/P10-4-analytics-bi-pipeline.md), [SLA/on-call](phase-10-compliance-maturity/P10-5-sla-error-budget-oncall.md) |
| P11 | [telephony closure](phase-11-production-closure/P11-1-telephony-procurement-rfq-lab-acceptance.md), [Sales/auth contract closure](phase-11-production-closure/P11-2-cross-team-contract-closure-pack.md), [legal/sign-off](phase-11-production-closure/P11-3-legal-retention-df03-signoff-pack.md), [readiness command center](phase-11-production-closure/P11-4-production-readiness-command-center.md) |

There are 51 prompts. “All prompts authored” means the library exists; it does not mean implementation, external integration, lab or production is complete.

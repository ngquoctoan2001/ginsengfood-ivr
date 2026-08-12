# Prompt Execution Tracker — IVR P0-P11

Trạng thái: `READY_TO_USE` · Cập nhật khi bắt đầu/chạy/xong từng prompt. File này là bảng điều phối, không thay thế evidence thật.

## Status vocabulary
`NOT_STARTED` · `IN_PROGRESS` · `CODE_DONE` · `TESTS_PASS` · `EVIDENCE_SUBMITTED` · `ACCEPTED` · `BLOCKED_INTERNAL` · `BLOCKED_EXTERNAL` · `DEFERRED_TARGET` · `N/A`

## Global Gates
| Gate | Owner | Status | Evidence | Notes |
| --- | --- | --- | --- | --- |
| Defaults sheet chốt dòng `MUST_DECIDE_BEFORE_P0` | IVR Owner | NOT_STARTED |  | Bắt buộc trước P0-1 |
| Defaults sheet chốt dòng `MUST_DECIDE_BEFORE_P1` | IVR Owner + Tech Lead | NOT_STARTED |  | Bắt buộc trước P1-1 |
| DT-01 SIM procurement/lab | Infra/procurement | BLOCKED_EXTERNAL |  | P11-1 sở hữu |
| DF-07 retention/legal | Owner + Legal | BLOCKED_EXTERNAL |  | P11-3/P10-1/P10-2 sở hữu |
| DF-03 release sign-off | Release Owner + Security/Privacy | BLOCKED_EXTERNAL |  | P9-1/P11-3/P11-4 sở hữu |
| OpenAPI current/target drift check | API Owner | NOT_STARTED |  | P1-1/P5-2 |
| Evidence ledger active | Release Owner | NOT_STARTED |  | P11-4 |

## Prompt Tracker
| ID | Phase | Prompt | Prereq | Status | Owner/Agent | Code/PR | Test Evidence | Acceptance Evidence | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| P0-1 | Foundation | [repo-and-solution-bootstrap](../phase-0-foundation/P0-1-repo-and-solution-bootstrap.md) | - | NOT_STARTED |  |  |  |  |  |
| P0-2 | Foundation | [ci-baseline-quality-gates](../phase-0-foundation/P0-2-ci-baseline-quality-gates.md) | P0-1 | NOT_STARTED |  |  |  |  |  |
| P0-3 | Foundation | [crosscutting-foundation](../phase-0-foundation/P0-3-crosscutting-foundation.md) | P0-1 | NOT_STARTED |  |  |  |  |  |
| P0-4 | Foundation | [feature-flag-config-platform](../phase-0-foundation/P0-4-feature-flag-config-platform.md) | P0-3 | NOT_STARTED |  |  |  |  |  |
| P1-1 | Contracts & Data | [openapi-codegen-contract-scaffold](../phase-1-contracts-data/P1-1-openapi-codegen-contract-scaffold.md) | P0-1..P0-3 | NOT_STARTED |  |  |  |  |  |
| P1-2 | Contracts & Data | [database-migrations-postgres](../phase-1-contracts-data/P1-2-database-migrations-postgres.md) | P0-3 | NOT_STARTED |  |  |  |  |  |
| P1-3 | Contracts & Data | [domain-model-dto-mapping](../phase-1-contracts-data/P1-3-domain-model-dto-mapping.md) | P1-1,P1-2 | NOT_STARTED |  |  |  |  |  |
| P1-4 | Contracts & Data | [api-docs-developer-portal](../phase-1-contracts-data/P1-4-api-docs-developer-portal.md) | P1-1 | NOT_STARTED |  |  |  |  |  |
| P2-1 | Core Runtime | [task-intake](../phase-2-core-runtime/P2-1-task-intake.md) | P1-* | NOT_STARTED |  |  |  |  |  |
| P2-2 | Core Runtime | [eligibility-blockers](../phase-2-core-runtime/P2-2-eligibility-blockers.md) | P2-1 | NOT_STARTED |  |  |  |  |  |
| P2-3 | Core Runtime | [scheduler-attempt-policy](../phase-2-core-runtime/P2-3-scheduler-attempt-policy.md) | P2-2 | NOT_STARTED |  |  |  |  |  |
| P2-4 | Core Runtime | [sim-adapter-mock](../phase-2-core-runtime/P2-4-sim-adapter-mock.md) | P2-3 | NOT_STARTED |  |  |  |  |  |
| P2-5 | Core Runtime | [dtmf-normalizer](../phase-2-core-runtime/P2-5-dtmf-normalizer.md) | P2-4 | NOT_STARTED |  |  |  |  |  |
| P2-6 | Core Runtime | [order-core-callback](../phase-2-core-runtime/P2-6-order-core-callback.md) | P2-5 | NOT_STARTED |  |  |  |  |  |
| P2-7 | Core Runtime | [script-content-management](../phase-2-core-runtime/P2-7-script-content-management.md) | P2-1 | NOT_STARTED |  |  |  |  |  |
| P3-1 | Admin UI | [ui-foundation](../phase-3-admin-ui/P3-1-ui-foundation.md) | P0-3,P2-1 | NOT_STARTED |  |  |  |  |  |
| P3-2 | Admin UI | [dashboard-calllog-detail](../phase-3-admin-ui/P3-2-dashboard-calllog-detail.md) | P3-1 | NOT_STARTED |  |  |  |  |  |
| P3-3 | Admin UI | [config-integration-roles](../phase-3-admin-ui/P3-3-config-integration-roles.md) | P3-2 | NOT_STARTED |  |  |  |  |  |
| P3-4 | Admin UI | [reporting-analytics-ui](../phase-3-admin-ui/P3-4-reporting-analytics-ui.md) | P3-2,P10-4 | NOT_STARTED |  |  |  |  |  |
| P4-1 | Real Integration | [order-core-wiring](../phase-4-integration/P4-1-order-core-wiring.md) | P2-6 | NOT_STARTED |  |  |  |  | Current 200/422 first; OC1/OC2 target flags |
| P4-2 | Real Integration | [ops-sellable-gate](../phase-4-integration/P4-2-ops-sellable-gate.md) | P2-2 | NOT_STARTED |  |  |  |  |  |
| P4-3 | Real Integration | [crm-eligibility-events](../phase-4-integration/P4-3-crm-eligibility-events.md) | P2-2 | NOT_STARTED |  |  |  |  | IR-CRM-01 rich fields may be target |
| P4-4 | Real Integration | [shared-auth-audit](../phase-4-integration/P4-4-shared-auth-audit.md) | P0-3 | NOT_STARTED |  |  |  |  |  |
| P4-5 | Real Integration | [post-decision-notification](../phase-4-integration/P4-5-post-decision-notification.md) | P4-3 | NOT_STARTED |  |  |  |  | DC-05 provider event may be target |
| P4-6 | Real Integration | [opt-out-feedback-loop](../phase-4-integration/P4-6-opt-out-feedback-loop.md) | P4-3 | NOT_STARTED |  |  |  |  |  |
| P5-1 | Quality | [unit-integration-tests](../phase-5-quality/P5-1-unit-integration-tests.md) | P2-* | NOT_STARTED |  |  |  |  |  |
| P5-2 | Quality | [contract-e2e-tests](../phase-5-quality/P5-2-contract-e2e-tests.md) | P2-*,P3-* | NOT_STARTED |  |  |  |  | Target cases pending, not fake pass |
| P5-3 | Quality | [performance-security-tests](../phase-5-quality/P5-3-performance-security-tests.md) | P2-*,P4-* | NOT_STARTED |  |  |  |  |  |
| P5-4 | Quality | [code-review-gate](../phase-5-quality/P5-4-code-review-gate.md) | P0-2 | NOT_STARTED |  |  |  |  |  |
| P5-5 | Quality | [accessibility-i18n-qa](../phase-5-quality/P5-5-accessibility-i18n-qa.md) | P3-* | NOT_STARTED |  |  |  |  |  |
| P6-1 | Observability | [logging-metrics-tracing](../phase-6-observability/P6-1-logging-metrics-tracing.md) | P2-* | NOT_STARTED |  |  |  |  |  |
| P6-2 | Observability | [dashboards-slo-alerting](../phase-6-observability/P6-2-dashboards-slo-alerting.md) | P6-1 | NOT_STARTED |  |  |  |  |  |
| P6-3 | Observability | [chaos-resilience-gamedays](../phase-6-observability/P6-3-chaos-resilience-gamedays.md) | P6-2,P4-* | NOT_STARTED |  |  |  |  |  |
| P7-1 | Deployment | [docker-images-compose](../phase-7-deployment/P7-1-docker-images-compose.md) | P2-*,P3-* | NOT_STARTED |  |  |  |  |  |
| P7-2 | Deployment | [kubernetes-helm](../phase-7-deployment/P7-2-kubernetes-helm.md) | P7-1,P6-2 | NOT_STARTED |  |  |  |  |  |
| P7-3 | Deployment | [cicd-pipeline](../phase-7-deployment/P7-3-cicd-pipeline.md) | P7-2,P5-4 | NOT_STARTED |  |  |  |  |  |
| P7-4 | Deployment | [progressive-delivery-canary](../phase-7-deployment/P7-4-progressive-delivery-canary.md) | P7-3 | NOT_STARTED |  |  |  |  |  |
| P7-5 | Deployment | [secret-rotation-key-lifecycle](../phase-7-deployment/P7-5-secret-rotation-key-lifecycle.md) | P7-2,P4-4 | NOT_STARTED |  |  |  |  |  |
| P8-1 | SIM Pilot | [real-sim-adapter](../phase-8-sim-pilot/P8-1-real-sim-adapter.md) | P2-4, DT-01 | BLOCKED_EXTERNAL |  |  |  |  | Needs SIM gateway/lab |
| P8-2 | SIM Pilot | [pilot-runbook](../phase-8-sim-pilot/P8-2-pilot-runbook.md) | P8-1,P7-3 | BLOCKED_EXTERNAL |  |  |  |  | Needs P8-1 + DF-03 pilot scope |
| P9-1 | Release & Ops | [release-gate-execution](../phase-9-release-ops/P9-1-release-gate-execution.md) | P8-2 | BLOCKED_EXTERNAL |  |  |  |  | Needs P11-3/P11-4 |
| P9-2 | Release & Ops | [cutover-ops-runbook](../phase-9-release-ops/P9-2-cutover-ops-runbook.md) | P9-1 | BLOCKED_EXTERNAL |  |  |  |  |  |
| P10-1 | Compliance | [pdpa-privacy-compliance](../phase-10-compliance-maturity/P10-1-pdpa-privacy-compliance.md) | P0-3,P4-3 | NOT_STARTED |  |  |  |  | Legal input required |
| P10-2 | Compliance | [data-governance-backup-dr](../phase-10-compliance-maturity/P10-2-data-governance-backup-dr.md) | P1-2,P7-2 | NOT_STARTED |  |  |  |  |  |
| P10-3 | Compliance | [capacity-cost-sim-sizing](../phase-10-compliance-maturity/P10-3-capacity-cost-sim-sizing.md) | P6-2,P5-3 | NOT_STARTED |  |  |  |  | Needs volume assumptions |
| P10-4 | Compliance | [analytics-bi-pipeline](../phase-10-compliance-maturity/P10-4-analytics-bi-pipeline.md) | P6-1 | NOT_STARTED |  |  |  |  |  |
| P10-5 | Compliance | [sla-error-budget-oncall](../phase-10-compliance-maturity/P10-5-sla-error-budget-oncall.md) | P6-2,P9-2 | NOT_STARTED |  |  |  |  | Business SLA required |
| P11-1 | External Closure | [telephony-procurement-rfq-lab-acceptance](../phase-11-production-closure/P11-1-telephony-procurement-rfq-lab-acceptance.md) | Start from P0 | NOT_STARTED |  |  |  |  | Start immediately |
| P11-2 | External Closure | [cross-team-contract-closure-pack](../phase-11-production-closure/P11-2-cross-team-contract-closure-pack.md) | P1-1 | NOT_STARTED |  |  |  |  | Start immediately after contract baseline |
| P11-3 | External Closure | [legal-retention-df03-signoff-pack](../phase-11-production-closure/P11-3-legal-retention-df03-signoff-pack.md) | P10-1/P10-2 | NOT_STARTED |  |  |  |  | Start legal thread early |
| P11-4 | External Closure | [production-readiness-command-center](../phase-11-production-closure/P11-4-production-readiness-command-center.md) | Continuous | NOT_STARTED |  |  |  |  | Owns board/ledger/go-no-go |

## Batch Log
| Date | Batch | Prompts touched | Result | Evidence |
| --- | --- | --- | --- | --- |
| 2026-07-06 | Tracker initialized | P0-P11 | Ready for execution control | This file |

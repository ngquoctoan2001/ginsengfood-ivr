# IVR Prompt Library — Master Index (zero → production)

Trạng thái: `LIVING` · Stack: **.NET 10 · PostgreSQL · Next.js · Docker/K8s** (DTS-01..05).
Đọc trước: [`README-governance.md`](README-governance.md) (bất biến governance) · runbook [`RUNBOOK-execute-prompts.md`](RUNBOOK-execute-prompts.md) · tracker [`_execution/prompt-execution-tracker.md`](_execution/prompt-execution-tracker.md) · defaults [`_execution/defaults-and-confirmations.md`](_execution/defaults-and-confirmations.md) · mẫu [`_TEMPLATE.md`](_TEMPLATE.md) · review readiness [`_review/zero-to-production-prompt-readiness-review.md`](_review/zero-to-production-prompt-readiness-review.md) · review spec alignment [`_review/phase-0-11-spec-alignment-review.md`](_review/phase-0-11-spec-alignment-review.md).
Mục tiêu: đưa IVR Order Confirmation từ **repo trống → chạy production**, mỗi prompt thực thi trọn (code + test + review + evidence).
Blocker ngoài code (mua SIM, team khác, legal): nguồn điều phối ở [`../plan/ivr-orther/production-blockers-plan.md`](../plan/ivr-orther/production-blockers-plan.md), đã được prompt hóa ở **Phase 11** để có RFQ/ticket/sign-off/evidence.

> **Trạng thái build:** ✅ = prompt đã viết xong · ⏳ = đang soạn · ⬜ = chưa soạn.
> Prompt cũ (mock M8.2A–H, stack-agnostic) đã archive ở `_legacy-mock/` — dùng làm tham chiếu, KHÔNG dùng trực tiếp.
> **P*-1..3 lõi = ✅**; nhóm maturity (P0-4, P1-4, P2-7, P3-4, P4-5/6, P5-5, P6-3, P7-4/5) + **Phase 10** = mở rộng độ chín.

## Bản đồ phase & cổng governance
```
P0 Foundation ─▶ P1 Contracts&Data ─▶ P2 Core(MOCK) ─▶ P3 Admin UI
     │                                      │
     └──────────────▶ P5 Quality ◀──────────┤ (test song song từ P2)
                                            ▼
                          P4 Real Integration ─▶ P6 Observability ─▶ P7 Deploy(K8s)
                                            │
   [mua SIM DT-01] ─▶ P8 SIM Pilot(REAL, scope hạn chế) ─▶ P9 Release&Ops ─▶ PROD
                                            gate: DF-03 sign-off + Legal DF-07
   P10 Compliance/Data-Governance/Analytics/SLA ── xuyên suốt, gate trước PROD
   P11 External Closure(SIM procurement + cross-team + legal + command-center)
      ├────────────── feeds P8/P9/P10 and blocks PROD if HARD evidence missing
```
`REAL_CUSTOMER_CALL_ALLOWED=NO` từ P0→P7; chỉ mở ở P9 sau DF-03. `IVR_ADAPTER_MODE=MOCK` tới P8.

## Trước khi chạy prompt
1. Chốt các dòng `MUST_DECIDE_BEFORE_P0/P1` trong [`_execution/defaults-and-confirmations.md`](_execution/defaults-and-confirmations.md).
2. Dùng [`RUNBOOK-execute-prompts.md`](RUNBOOK-execute-prompts.md) làm quy trình chạy chính.
3. Cập nhật [`_execution/prompt-execution-tracker.md`](_execution/prompt-execution-tracker.md) sau mỗi prompt/batch; không chuyển `ACCEPTED` khi chưa có evidence đọc được.
4. Khởi động P11-1/P11-2/P11-3/P11-4 sớm để blocker ngoài code không bị dồn về cuối.

## Phase 0 — Foundation & Project Setup
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P0-1 | [repo-and-solution-bootstrap](phase-0-foundation/P0-1-repo-and-solution-bootstrap.md) | Solution .NET 10 (Api/Worker/Domain/Infra/Contracts), Next.js app, Postgres local, layout, .editorconfig/analyzers | — | ✅ |
| P0-2 | [ci-baseline-quality-gates](phase-0-foundation/P0-2-ci-baseline-quality-gates.md) | CI (build/test/lint/coverage/scan/OpenAPI-lint), branch policy, PR traceability template | P0-1 | ✅ |
| P0-3 | [crosscutting-foundation](phase-0-foundation/P0-3-crosscutting-foundation.md) | Config/secrets, RBAC `IVR_*`, audit append-only, idempotency store, correlation middleware, evidence registry, error envelope | P0-1 | ✅ |
| P0-4 | [feature-flag-config-platform](phase-0-foundation/P0-4-feature-flag-config-platform.md) | Hệ feature-flag + config động (flag target vs live: race-guard/richCodes/trustResolver/pilot), audit đổi flag, kill-switch primitive | P0-3 | ✅ |

## Phase 1 — Contracts & Data
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P1-1 | [openapi-codegen-contract-scaffold](phase-1-contracts-data/P1-1-openapi-codegen-contract-scaffold.md) | Sinh server stub + client từ OpenAPI 3.1; contract-test scaffold; `Ivr.Contracts` | P0-1..3 | ✅ |
| P1-2 | [database-migrations-postgres](phase-1-contracts-data/P1-2-database-migrations-postgres.md) | EF Core migrations 11 bảng `ivr_*`, constraint D-10, index, retention job hook | P0-3 | ✅ |
| P1-3 | [domain-model-dto-mapping](phase-1-contracts-data/P1-3-domain-model-dto-mapping.md) | Entities/VO/policies (D-10, taxonomy), DTO mapping, privacy-safe snapshot guard | P1-1,P1-2 | ✅ |
| P1-4 | [api-docs-developer-portal](phase-1-contracts-data/P1-4-api-docs-developer-portal.md) | Tài liệu API sinh từ OpenAPI, changelog contract, developer portal (non-prod), versioning/deprecation policy | P1-1 | ✅ |

## Phase 2 — Core Runtime (.NET, mock SIM)
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P2-1 | [task-intake](phase-2-core-runtime/P2-1-task-intake.md) | `POST /tasks`: allowlist, idempotency, validate CONFIRMING+COD, snapshot, decision taxonomy | P1-* | ✅ |
| P2-2 | [eligibility-blockers](phase-2-core-runtime/P2-2-eligibility-blockers.md) | Consume sellable snapshot, do-not-call (mock/CRM), trust (disabled DC-06), contact/window/capacity | P2-1 | ✅ |
| P2-3 | [scheduler-attempt-policy](phase-2-core-runtime/P2-3-scheduler-attempt-policy.md) | Rolling queue, attempt D-10 (2 lần, window/spacing), no-batch, deadline index | P2-2 | ✅ |
| P2-4 | [sim-adapter-mock](phase-2-core-runtime/P2-4-sim-adapter-mock.md) | `ISimGateway` (dial/play/capture/disposition/health) + Mock đọc seed; one-sim-one-call; token vault boundary | P2-3 | ✅ |
| P2-5 | [dtmf-normalizer](phase-2-core-runtime/P2-5-dtmf-normalizer.md) | Disposition→result taxonomy (DT-02); technical≠no-answer; counted/final flags | P2-4 | ✅ |
| P2-6 | [order-core-callback](phase-2-core-runtime/P2-6-order-core-callback.md) | Callback client (200/422 today, target codes), revalidate contract, retry bounded, evidence link | P2-5 | ✅ |
| P2-7 | [script-content-management](phase-2-core-runtime/P2-7-script-content-management.md) | Store script/template + version + approve workflow + allowed variables; intake chỉ nhận approved; A/B khung (script khác nhau) | P2-1 | ✅ |

## Phase 3 — Admin UI (Next.js)
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P3-1 | [ui-foundation](phase-3-admin-ui/P3-1-ui-foundation.md) | Next.js app, auth/RBAC, layout, API client, i18n vi, error/loading | P0-3,P2-1 | ✅ |
| P3-2 | [dashboard-calllog-detail](phase-3-admin-ui/P3-2-dashboard-calllog-detail.md) | Dashboard + call-log + call-detail (evidence view), PII masked, no bypass Core | P3-1 | ✅ |
| P3-3 | [config-integration-roles](phase-3-admin-ui/P3-3-config-integration-roles.md) | IVR-menu config + integration-status + seed/mock mgmt + role/permission UI | P3-2 | ✅ |
| P3-4 | [reporting-analytics-ui](phase-3-admin-ui/P3-4-reporting-analytics-ui.md) | Báo cáo/analytics UI: success/no-answer/technical trend theo program/thời gian, export, drill-down (privacy-safe) | P3-2,P10-4 | ✅ |

## Phase 4 — Real Integration (fail-closed; một số phụ thuộc team khác)
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P4-1 | [order-core-wiring](phase-4-integration/P4-1-order-core-wiring.md) | Nhận task push thật + callback thật; race-guard khi OC1 sẵn sàng (else state/COD recheck) | P2-6 | ✅ |
| P4-2 | [ops-sellable-gate](phase-4-integration/P4-2-ops-sellable-gate.md) | `availability/check` thật, webhook dedupe, captured_at/ETag, fail-closed | P2-2 | ✅ |
| P4-3 | [crm-eligibility-events](phase-4-integration/P4-3-crm-eligibility-events.md) | `crm-ads-eligibility` (do-not-call), consume event sau Core decision (DC-05), trust resolver (DC-06 khi có) | P2-2 | ✅ |
| P4-4 | [shared-auth-audit](phase-4-integration/P4-4-shared-auth-audit.md) | Service identity allowlist thật, mTLS/JWT, audit federation | P0-3 | ✅ |
| P4-5 | [post-decision-notification](phase-4-integration/P4-5-post-decision-notification.md) | Consume event Core decision → trigger CRM notification (DC-05); IVR không tự gửi (D-14); template/khớp; no-op tới khi Core publish | P4-3 | ✅ |
| P4-6 | [opt-out-feedback-loop](phase-4-integration/P4-6-opt-out-feedback-loop.md) | Tín hiệu rejected/opt-out (DT-02 review-flag) → review → đề xuất do-not-call về CRM; khép vòng suppression | P4-3 | ✅ |

## Phase 5 — Quality Engineering (test code + review)
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P5-1 | [unit-integration-tests](phase-5-quality/P5-1-unit-integration-tests.md) | xUnit unit + integration (Testcontainers Postgres) theo testing/02,03 | P2-* | ✅ |
| P5-2 | [contract-e2e-tests](phase-5-quality/P5-2-contract-e2e-tests.md) | Consumer-driven contract + OpenAPI + E2E (Playwright) theo testing/04,05 | P2-*,P3-* | ✅ |
| P5-3 | [performance-security-tests](phase-5-quality/P5-3-performance-security-tests.md) | Load/soak (SIM concurrency, capacity) + security/privacy (PII, fail-closed) theo testing/06,07 | P2-*,P4-* | ✅ |
| P5-4 | [code-review-gate](phase-5-quality/P5-4-code-review-gate.md) | Static analysis, PR review checklist, coverage gate, security scan — "review" tự động + người | P0-2 | ✅ |
| P5-5 | [accessibility-i18n-qa](phase-5-quality/P5-5-accessibility-i18n-qa.md) | Admin UI: a11y (WCAG), i18n vi QA, cross-browser/responsive, visual regression | P3-* | ✅ |

## Phase 6 — Observability & Reliability
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P6-1 | [logging-metrics-tracing](phase-6-observability/P6-1-logging-metrics-tracing.md) | OpenTelemetry log/metric/trace, correlation propagation, PII-safe | P2-* | ✅ |
| P6-2 | [dashboards-slo-alerting](phase-6-observability/P6-2-dashboards-slo-alerting.md) | Dashboards, SLO/alert, health/readiness (fail-closed 503), capacity monitor | P6-1 | ✅ |
| P6-3 | [chaos-resilience-gamedays](phase-6-observability/P6-3-chaos-resilience-gamedays.md) | Fault injection (downstream/SIM/DB down), chaos game-day, verify fail-closed + recovery dưới sự cố thật | P6-2,P4-* | ✅ |

## Phase 7 — Deployment (Docker + Kubernetes)
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P7-1 | [docker-images-compose](phase-7-deployment/P7-1-docker-images-compose.md) | Dockerfile api/worker/ui (multi-stage), dev docker-compose | P2-*,P3-* | ✅ |
| P7-2 | [kubernetes-helm](phase-7-deployment/P7-2-kubernetes-helm.md) | Helm chart: deployments, HPA (SIM concurrency), config/secret/vault, NetworkPolicy, CronJob retention | P7-1,P6-2 | ✅ |
| P7-3 | [cicd-pipeline](phase-7-deployment/P7-3-cicd-pipeline.md) | CD: build→test→scan→push→deploy staged; governance ladder → env promotion | P7-2,P5-4 | ✅ |
| P7-4 | [progressive-delivery-canary](phase-7-deployment/P7-4-progressive-delivery-canary.md) | Blue-green/canary chi tiết, traffic shifting, automated rollback theo SLO, feature-flag ramp | P7-3 | ✅ |
| P7-5 | [secret-rotation-key-lifecycle](phase-7-deployment/P7-5-secret-rotation-key-lifecycle.md) | Rotation secret/credential/dial-token key, Vault/KMS lifecycle, zero-downtime rotation, runbook | P7-2,P4-4 | ✅ |

## Phase 8 — SIM Pilot (REAL, scope hạn chế)
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P8-1 | [real-sim-adapter](phase-8-sim-pilot/P8-1-real-sim-adapter.md) | Impl `ISimGateway` REAL theo protocol DT-01 (sau khi mua); disposition re-verify harness (DT-02) | P2-4, mua SIM | ✅ |
| P8-2 | [pilot-runbook](phase-8-sim-pilot/P8-2-pilot-runbook.md) | Pilot real-customer scope hạn chế (DF-03), evidence capture, kill-switch, rollback | P8-1,P7-3 | ✅ |

## Phase 9 — Release & Operations
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P9-1 | [release-gate-execution](phase-9-release-ops/P9-1-release-gate-execution.md) | Chạy governance ladder → mở `REAL_CUSTOMER_CALL_ALLOWED`; MASTER-05 evidence acceptance; sign-off DF-03 | P8-2 | ✅ |
| P9-2 | [cutover-ops-runbook](phase-9-release-ops/P9-2-cutover-ops-runbook.md) | Cutover/rollback, ops runbook (on-call, incident, DR/backup), retention/legal DF-07 | P9-1 | ✅ |

## Phase 10 — Compliance, Data Governance & Business Ops (xuyên suốt, gate trước PROD)
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P10-1 | [pdpa-privacy-compliance](phase-10-compliance-maturity/P10-1-pdpa-privacy-compliance.md) | PDPA/consent legal basis (transactional COD call), DSAR, do-not-call registry hợp lệ, privacy impact assessment, DF-07 retention | P0-3,P4-3 | ✅ |
| P10-2 | [data-governance-backup-dr](phase-10-compliance-maturity/P10-2-data-governance-backup-dr.md) | Data classification, crypto at-rest/in-transit, backup encryption, DR topology/RTO-RPO, data lifecycle | P1-2,P7-2 | ✅ |
| P10-3 | [capacity-cost-sim-sizing](phase-10-compliance-maturity/P10-3-capacity-cost-sim-sizing.md) | Mô hình capacity (SIM pool sizing calibrate DT-04), cost model, load forecast theo program, scaling policy | P6-2,P5-3 | ✅ |
| P10-4 | [analytics-bi-pipeline](phase-10-compliance-maturity/P10-4-analytics-bi-pipeline.md) | Data pipeline analytics (call outcomes → warehouse), KPI nghiệp vụ, privacy-safe aggregation (feed P3-4) | P6-1 | ✅ |
| P10-5 | [sla-error-budget-oncall](phase-10-compliance-maturity/P10-5-sla-error-budget-oncall.md) | SLA với business, error-budget policy, on-call maturity (rota/training/drill), incident review loop | P6-2,P9-2 | ✅ |

## Phase 11 — External Production Closure (hard blockers prompt hóa)
| ID | Prompt | Scope | Prereq | TT |
| --- | --- | --- | --- | --- |
| P11-1 | [telephony-procurement-rfq-lab-acceptance](phase-11-production-closure/P11-1-telephony-procurement-rfq-lab-acceptance.md) | RFQ SIM gateway, vendor scorecard, lab acceptance, DT-01/04/06 decision records, handoff cho P8-1 | chạy song song từ P0; xong trước P8-1 | ✅ |
| P11-2 | [cross-team-contract-closure-pack](phase-11-production-closure/P11-2-cross-team-contract-closure-pack.md) | Ticket/contract/pact cho OC1/OC2/OC3, DC-05/06, IR-CRM-01, DO-02; flag target/live | P1-1; feed P4/P5/P9 | ✅ |
| P11-3 | [legal-retention-df03-signoff-pack](phase-11-production-closure/P11-3-legal-retention-df03-signoff-pack.md) | DF-07 retention, PDPA/legal basis, recording OFF, DF-03 sign-off package | P10-1/P10-2; xong trước P9-1 | ✅ |
| P11-4 | [production-readiness-command-center](phase-11-production-closure/P11-4-production-readiness-command-center.md) | Readiness board, evidence ledger, feature flag ledger, go/no-go handoff | chạy xuyên suốt; final trước P9-1 | ✅ |

## Tổng
**51 prompt / 12 phase — ✅ tất cả đã soạn.** Lõi (P*-1..3) = 32; mở rộng maturity + Phase 10 = 15; external closure = 4.
- **Zero→production kỹ thuật:** P0–P9.
- **Zero→production end-to-end:** P0–P11, trong đó P11 đóng hard blocker ngoài code bằng artifacts/evidence/sign-off.
- **Độ chín/tuân thủ:** nhóm maturity + P10 — gate trước PROD (compliance/DR/SLA).
- **Chặn ngoài code:** vẫn cần owner/vendor/legal/team khác thực thi thật; P11 không ký thay nhưng bảo đảm RFQ/ticket/legal/sign-off/evidence không bị bỏ trống.

## Ghi chú tách prompt thô (đang làm — task #16)
Các prompt lõi coarse sẽ tách per-FR để agent dễ thực thi từng bước: **P2-1** (validate / persist+decision / idempotency), **P2-3** (queue-claim / attempt-exec / sim-pool), **P3-2** (dashboard / call-log+detail), **P7-2** (core-manifests / HPA+netpol+secret / retention+PDB). Bản tách sẽ thay row tương ứng, giữ prereq nhất quán.

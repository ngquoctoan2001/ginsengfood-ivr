# Defaults & Confirmations — IVR Prompt Execution

Trạng thái: `LIVING` · Mục tiêu: chốt các lựa chọn `NEED_CONFIRMATION` trước khi chạy prompt để agent không tự đoán sai.

## Cách dùng
- `MUST_DECIDE_BEFORE_P0`: chốt trước khi chạy P0-1.
- `MUST_DECIDE_BEFORE_P1`: chốt trước khi sinh contract/data.
- `MUST_DECIDE_BEFORE_P7`: chốt trước deploy.
- `MUST_DECIDE_BEFORE_P8/P9`: chốt trước pilot/release.
- `DEFAULT_OK`: dùng default nếu owner chưa phản đối.
- `OWNER_DECISION`: không được tự chốt; cần owner/legal/vendor/provider.

## Implementation Defaults
| Item | Default đề xuất | Khi cần chốt | Owner | Status | Decision / Evidence |
| --- | --- | --- | --- | --- | --- |
| Repo/service name | `ivr` / `ginsengfood-ivr` | MUST_DECIDE_BEFORE_P0 | IVR Owner | OPEN |  |
| Root namespace | `Ivr` | MUST_DECIDE_BEFORE_P0 | Tech Lead | OPEN |  |
| Backend stack | .NET 10, ASP.NET Core, Worker Service | DEFAULT_OK | Tech Lead | DEFAULT_OK | DTS-01 |
| Database | PostgreSQL + EF Core migrations | DEFAULT_OK | Tech Lead | DEFAULT_OK | DTS-02 |
| Admin UI | Next.js + TypeScript strict | DEFAULT_OK | Tech Lead | DEFAULT_OK | DTS-03 |
| Container/deploy | Docker + Kubernetes + Helm | DEFAULT_OK | Platform | DEFAULT_OK | DTS-04 |
| Local dev compose | Docker Compose for API/Worker/UI/Postgres/mock services | DEFAULT_OK | Tech Lead | DEFAULT_OK | P7-1 |
| ORM style | EF Core explicit mappings; no in-memory DB for integration tests | DEFAULT_OK | Tech Lead | DEFAULT_OK | P1-2/P5-1 |
| Outbox/job scheduling | Postgres-backed outbox/worker loop first; queue broker only if platform requires | MUST_DECIDE_BEFORE_P2 | Tech Lead | OPEN |  |
| Codegen tool | NSwag default; Kiota only if team standard says so | MUST_DECIDE_BEFORE_P1 | API Owner | OPEN | P1-1 |
| Generated code strategy | Commit generated DTO/client + CI drift-check | MUST_DECIDE_BEFORE_P1 | API Owner | OPEN | P1-1 |
| API docs renderer | Scalar or Redoc static, non-prod only | MUST_DECIDE_BEFORE_P1 | API Owner | OPEN | P1-4 |
| Frontend styling | Tailwind + headless components | MUST_DECIDE_BEFORE_P3 | UI Lead | OPEN | P3-1 |
| Frontend package manager | `pnpm` default if no platform standard | MUST_DECIDE_BEFORE_P3 | UI Lead | OPEN |  |
| Auth provider | Dev mock JWT; prod platform SSO/JWT | MUST_DECIDE_BEFORE_P3 | Security/Platform | OPEN | P3-1/P4-4 |
| CI provider | GitHub Actions default; GitLab/Azure if platform mandates | MUST_DECIDE_BEFORE_P0 | Platform | OPEN | P0-2 |
| Container registry | Platform registry | MUST_DECIDE_BEFORE_P7 | Platform | OPEN | P7-1/P7-3 |
| Secret store dev | local env/user-secrets + K8s Secret in dev | DEFAULT_OK | Platform | DEFAULT_OK | P0-3/P7-2 |
| Secret store prod | Vault/KMS/ExternalSecret, no literal prod secrets | MUST_DECIDE_BEFORE_P7 | Security/Platform | OPEN | P7-2/P7-5 |
| Observability backend | Prometheus + Grafana + Loki/Tempo or platform APM | MUST_DECIDE_BEFORE_P6 | SRE | OPEN | P6-1/P6-2 |
| Canary controller | Argo Rollouts default; Flagger if platform standard | MUST_DECIDE_BEFORE_P7 | Platform/SRE | OPEN | P7-4 |
| BI/warehouse | Postgres analytics schema first; ClickHouse/BigQuery if platform exists | MUST_DECIDE_BEFORE_P10 | Data Owner | OPEN | P10-4 |

## Product / Governance Confirmations
| Item | Default / current truth | Khi cần chốt | Owner | Status | Decision / Evidence |
| --- | --- | --- | --- | --- | --- |
| COD-only scope | Current: only `CONFIRMING` + `payment_method_snapshot=COD` | DEFAULT_OK | Product/Order Core | CONFIRMED | DS-01 |
| Attempt policy | D-10: max 2; GH 300/150; 24-7 900/450 | DEFAULT_OK | Product/Order Core | CONFIRMED | D-10 |
| IVR order transition | IVR never transitions order; Core owns state | DEFAULT_OK | Product/Order Core | CONFIRMED | D-02 |
| Callback current | Current = 200/422, no `order_version_seen_by_ivr` | DEFAULT_OK | Order Core | CONFIRMED_CURRENT | DS-03/DS-04 |
| Callback target OC1/OC2 | Add `order_version_seen_by_ivr` + semantic `CALLBACK_*` | OWNER_DECISION | Order Core | TARGET_OPEN | P11-2 |
| Do-not-call basic | `crm-ads-eligibility` PHONE_CALL usable via `eligible` | DEFAULT_OK | CRM/Identity | CONFIRMED_CURRENT | DC-01 |
| Do-not-call rich fields | `do_not_call/opt_out_scope/reason/effective_at` + Core wiring | OWNER_DECISION | CRM/Identity + Order Core | TARGET_OPEN | IR-CRM-01/P11-2 |
| Trust resolver | Default require-IVR; trusted skip disabled | OWNER_DECISION | CRM/Business | TARGET_OPEN | DC-06 |
| Post-decision event | IVR does not notify directly; waits for Core/CRM event | OWNER_DECISION | Order Core + CRM | TARGET_OPEN | DC-05 |
| KEY_9 human support | NOT_ENABLED initially | OWNER_DECISION | Ops/CSKH | OPEN_NON_BLOCKING | Q-F2 |
| Real customer calls | `REAL_CUSTOMER_CALL_ALLOWED=NO` until P9 gate | OWNER_DECISION | Release Owner + Security/Privacy | HARD_GATE | DF-03 |
| Retention durations | Legal-specific durations still required | OWNER_DECISION | Owner + Legal | HARD_GATE | DF-07/P11-3 |
| Recording | OFF by default; enabling needs consent + legal + retention | OWNER_DECISION | Owner + Legal | DEFAULT_OFF | DT-05 |

## Telephony / SIM Confirmations
| Item | Default / current truth | Khi cần chốt | Owner | Status | Decision / Evidence |
| --- | --- | --- | --- | --- | --- |
| SIM gateway protocol | Adapter port designed; protocol after purchase | MUST_DECIDE_BEFORE_P8 | Infra/procurement | HARD_GATE_OPEN | DT-01/P11-1 |
| DTMF mode | RFC2833 or in-band, depends on gateway | MUST_DECIDE_BEFORE_P8 | Infra/procurement | OPEN | DT-03 |
| SIM pool pilot | Planning assumption 12 SIM | MUST_DECIDE_BEFORE_P8 | Infra/procurement | OPEN | DT-04/P10-3 |
| SIM pool launch | Planning assumption 24-32 SIM; model must calibrate | MUST_DECIDE_BEFORE_P9 | Infra/procurement + Business | OPEN | DT-04/P10-3 |
| Cooldown/fail disable | cooldown 5s; fail_count >=3/10m disables + alert | DEFAULT_OK | Infra/SRE | CONFIRMED | DT-04 |
| Caller-ID/brandname | Must be consistent and trustworthy | MUST_DECIDE_BEFORE_P8 | Telco/procurement | OPEN | DT-06 |
| Lab acceptance numbers | Test/loopback numbers only before real customers | MUST_DECIDE_BEFORE_P8 | Infra/procurement | OPEN | P11-1 |

## Evidence / Release Defaults
| Item | Default đề xuất | Khi cần chốt | Owner | Status | Decision / Evidence |
| --- | --- | --- | --- | --- | --- |
| Evidence root | `docs/evidence/<PromptId>/...` | MUST_DECIDE_BEFORE_P0 | Release Owner | OPEN |  |
| Readiness board | `docs/release/ivr-production-readiness-board.md` | DEFAULT_OK | Release Owner | DEFAULT_OK | P11-4 |
| Evidence ledger | `docs/release/ivr-evidence-ledger.md` | DEFAULT_OK | Release Owner | DEFAULT_OK | P11-4 |
| Feature flag ledger | `docs/release/ivr-feature-flag-ledger.md` | DEFAULT_OK | Release Owner | DEFAULT_OK | P11-4 |
| Go/no-go brief | `docs/release/ivr-go-no-go-brief.md` | DEFAULT_OK | Release Owner | DEFAULT_OK | P11-4 |
| Hypercare handoff | `docs/release/ivr-hypercare-handoff.md` | DEFAULT_OK | SRE/Release Owner | DEFAULT_OK | P11-4/P10-5 |
| PR traceability | Source spec + decision ID + contract + test ID + evidence link | DEFAULT_OK | Engineering Lead | DEFAULT_OK | README-governance §5 |
| Acceptance authority | Code reviewer accepts code; release owner accepts evidence; legal/security accept hard gates | MUST_DECIDE_BEFORE_P9 | Release Owner | OPEN |  |

## Stop Rules For Defaults
- Không bắt đầu P0 nếu repo/name/CI/evidence-root chưa có owner decision hoặc accepted default.
- Không bắt đầu P1 nếu codegen strategy chưa chốt.
- Không bắt đầu P3 nếu auth/UI default chưa chốt.
- Không bắt đầu P7 nếu secret store/container registry chưa chốt.
- Không bắt đầu P8 nếu SIM protocol/lab acceptance chưa xong.
- Không bắt đầu P9 nếu DF-03/DF-07 chưa có sign-off package và P11-4 chưa có go/no-go brief.

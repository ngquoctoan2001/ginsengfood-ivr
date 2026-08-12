# REVIEW — Zero-to-Production Prompt Readiness

> **SUPERSEDED 2026-08-12:** giữ làm lịch sử. Target V1 hiện tách mock-complete, one-SIM lab, real Sales integration và 32-eSIM production gates.

Trạng thái: `REVIEW` · Ngày rà soát: 2026-07-06 · Phạm vi: toàn bộ `prompt/phase-*`, `prompt/00-index.md`, `prompt/README-governance.md`, `plan/ivr-orther/production-blockers-plan.md`, `specs/_review/open-decisions-register.md`.

## Kết luận
Bộ prompt hiện tại **đủ để điều phối triển khai IVR Order Confirmation từ repo trống tới production thật**, với điều kiện hiểu đúng:
- P0-P10 triển khai service IVR, quality, deploy, pilot, release, compliance/maturity.
- P11 đóng phần ngoài code bằng RFQ/ticket/legal/sign-off/evidence.
- Production thật vẫn cần owner/vendor/legal/team khác thực hiện và ký; prompt không thể tự thay thế chữ ký DF-03/DF-07 hay procurement DT-01.

## Finding trước khi bổ sung
| Severity | Vấn đề | Tác động | Trạng thái sau vá |
| --- | --- | --- | --- |
| P0 | Hard blockers ngoài code chỉ nằm trong `production-blockers-plan.md`, chưa có prompt thực thi | Người triển khai có thể code xong P0-P10 nhưng không có RFQ SIM, ticket cross-team, legal/sign-off package → không mở production thật | ✅ Đã thêm P11-1..P11-4 |
| P1 | `P1-1` và `P2-6` còn wording cũ nói producer vẫn gửi `order_version_seen_by_ivr` | Lệch OpenAPI current/target: current callback không gửi field này | ✅ Đã sửa: current không gửi, target OC1 mới gửi |
| P2 | Index tổng vẫn nói blocker ngoài code "không phải prompt" | Dễ hiểu nhầm zero→production đã đủ nếu chỉ chạy prompt code | ✅ Đã đổi: blocker ngoài code được prompt hóa ở P11, nhưng vẫn cần owner/vendor/legal |

## Coverage sau bổ sung
| Vùng | Prompt phủ | Ghi chú |
| --- | --- | --- |
| Repo/bootstrap/foundation | P0-1..P0-4 | Solution, CI, RBAC/audit/idempotency, feature flags/kill-switch |
| Contract/data | P1-1..P1-4 | OpenAPI current/target, DB, DTO/domain, docs portal |
| Core runtime mock | P2-1..P2-7 | Intake → eligibility → scheduler → mock SIM → normalizer → callback → script |
| Admin UI/reporting | P3-1..P3-4 | Dashboard, call detail, config, analytics UI |
| Real integration | P4-1..P4-6 | Order Core, ops, CRM, auth, notification consumer, opt-out feedback |
| Quality | P5-1..P5-5 | Unit/integration/contract/e2e/perf/security/review/a11y |
| Observability/reliability | P6-1..P6-3 | OTel, dashboards/SLO, chaos/game-day |
| Deployment | P7-1..P7-5 | Docker/K8s/CD/canary/secrets rotation |
| SIM pilot/release | P8-1..P9-2 | Real SIM adapter, pilot, release gate, ops runbook |
| Compliance/maturity | P10-1..P10-5 | PDPA, DR, capacity/cost, BI, SLA/on-call |
| External production closure | P11-1..P11-4 | SIM RFQ/lab, cross-team tickets/contracts, legal/retention/sign-off, command center |

## Hard gates còn phải được ký/thực hiện
| Gate | Prompt tạo bằng chứng | Ai phải đóng thật |
| --- | --- | --- |
| DT-01/03/04/06 SIM procurement + protocol + caller-ID | P11-1, feed P8-1 | Infra/procurement/telco + IVR Owner |
| DF-07 retention + PDPA/legal basis | P11-3, feed P9-1 | Owner + Legal + Security/Privacy |
| DF-03 release sign-off | P11-3 + P11-4 + P9-1 | Module 8 Owner + Security/Privacy |
| OC1/OC2/OC3/DC-05/06/IR-CRM-01 | P11-2, feed P4/P5/P9 | Order Core/CRM/Ops provider teams |

## Acceptance để tuyên bố prompt library đủ
- [x] Tất cả phase P0-P11 có prompt cụ thể, output artifact, tests/verification, evidence.
- [x] Current/target callback race-guard không còn mâu thuẫn trong prompt lõi.
- [x] Prompt index nói rõ P11 là external closure, không giả rằng code prompt tự đóng legal/procurement.
- [x] P3-4 tồn tại trong prompt index và doc map.
- [x] Review alignment P0-P11 đã xác nhận prompt khớp spec hiện hành: `prompt/_review/phase-0-11-spec-alignment-review.md`.
- [x] Rerun doc map sau patch.

## Không được tuyên bố
- Không được nói "production-ready" nếu chỉ chạy P0-P10 mà chưa có P11/P8/P9 evidence.
- Không được mở `REAL_CUSTOMER_CALL_ALLOWED` nếu thiếu DT-01, DF-03, DF-07 hoặc evidence ACCEPTED.
- Không được bật target flags OC1/OC2/DC-05/DC-06/IR-CRM-01 nếu provider chưa có contract-test evidence.

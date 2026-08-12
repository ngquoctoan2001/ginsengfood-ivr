# PROMPT P10-5 — SLA, Error-Budget Policy & On-Call Maturity

## 0. Meta
| | |
| --- | --- |
| **ID** | `P10-5` · **Phase** 10 — Compliance & Maturity |
| **Work ID** | `W-0056` (canonical tracker §5) |
| **Prereq** | `P6-2`, `P9-2` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| **Stack** | process · SRE |

## 1. ROLE
Bạn là **SRE Lead / Service Owner**. Bạn thiết lập cam kết dịch vụ (SLA) với business, chính sách error-budget để cân bằng tốc độ vs ổn định, và độ chín vận hành on-call (rota, đào tạo, diễn tập, vòng lặp cải tiến sự cố). Bạn biến "có alert" thành "đội ngũ xử lý được".

## 2. CONTEXT
P6-2 định nghĩa SLO kỹ thuật; nhưng cần **SLA** (cam kết với business) + **error-budget policy** (khi nào ngừng ship để ổn định) + **on-call maturity** (người trực xử lý sự cố thật). Đây là mảnh cuối để service vận hành bền vững sau go-live, không chỉ chạy được mà **duy trì được**.

## 3. SOURCE SPECS (đọc trước)
- `specs/architecture/05-resilience.md`, `specs/architecture/06-observability.md`, `specs/testing/08-acceptance-criteria.md`
- `plan/ivr-orther/decisions-log.md` §DF-03 · `prompt/phase-6-observability/P6-2-dashboards-slo-alerting.md` (SLO), `prompt/phase-9-release-ops/P9-2-cutover-ops-runbook.md` (ops runbook), `prompt/phase-6-observability/P6-3-chaos-resilience-gamedays.md` (game-day), `plan/ivr-orther/14-risk-register.md`

## 4. DECISIONS & CONSTRAINTS
- **SLA:** cam kết đo được với business (VD % task được gọi trong window, uptime intake/callback) — dựa SLO (P6-2) nhưng là hợp đồng.
- **Error-budget policy:** budget cạn → ưu tiên ổn định (freeze feature, dồn sức reliability); ràng buộc với release gate.
- **On-call maturity:** rota, escalation, đào tạo (dùng runbook P9-2 + game-day P6-3), on-boarding trực.
- **Incident review loop:** postmortem blameless → action items → risk register (14-risk-register).
- **Không** SLA hứa vượt khả năng (đo trước bằng perf P5-3 + capacity P10-3).

## 5. INPUTS / DEPENDENCIES
- SLO/metrics (P6-2); ops runbook (P9-2); game-day (P6-3); business stakeholder (thoả thuận SLA).

## 6. BUILD STEPS
1. **SLA definition** `docs/sla.md`: chỉ số cam kết + cách đo + báo cáo; thống nhất business (Owner).
2. **Error-budget policy** `docs/error-budget-policy.md`: budget, hành động khi cạn (freeze/ưu tiên reliability), liên kết release gate.
3. **On-call**: rota + escalation matrix + on-call handbook (dùng P9-2 runbook); on-boarding + đào tạo qua game-day (P6-3).
4. **Incident lifecycle**: severity, response, blameless postmortem template, action tracking → risk register.
5. **Reporting**: SLA/error-budget dashboard (nối P6-2) + báo cáo định kỳ business.
6. **Drill**: chạy 1 incident drill end-to-end (alert→on-call→mitigate→postmortem).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/sla.md`, `docs/error-budget-policy.md` | SLA + error-budget |
| `docs/oncall-handbook.md`, `docs/escalation-matrix.md` | On-call |
| `docs/incident-postmortem-template.md` (nối P9-2) | Incident loop |

## 8. TESTS TO WRITE (verification/drill)
| Test ID | Loại | Assert |
| --- | --- | --- |
| `SLA-MEASURE-01` | verification | SLA đo được từ metric thật (P6-2); không hứa vượt khả năng (đối chiếu P5-3/P10-3). |
| `SLA-BUDGET-02` | verification | budget cạn → policy freeze/ưu tiên reliability kích hoạt (liên kết release gate). |
| `SLA-ONCALL-03` | drill | incident drill: alert→on-call→mitigate→postmortem→action item vào risk register. |
| `SLA-REPORT-04` | verification | báo cáo SLA/error-budget định kỳ đúng số. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] SLA đo được, không quá khả năng; [ ] error-budget liên kết gate; [ ] on-call rota+đào tạo; [ ] incident loop → risk register.
**Reviewer:** SLA thống nhất business; escalation khả thi; drill thật sự chạy.

## 10. EVIDENCE EXPECTED
SLA doc thống nhất, error-budget policy, on-call rota/handbook, incident drill report + postmortem + action items.

## 11. FORBIDDEN
- ❌ SLA hứa vượt khả năng đo được. ❌ Error-budget không liên kết hành động. ❌ On-call không đào tạo/drill. ❌ Postmortem đổ lỗi cá nhân (blameless).

## 12. DEFINITION OF DONE
- [ ] SLA + error-budget + on-call + incident loop + drill; 4 verification §8 pass; evidence §10 đủ. **Kết thúc Phase 10: service chín về tuân thủ + vận hành.**

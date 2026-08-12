# PROMPT P9-2 — Cutover, Rollback & Ops Runbook

## 0. Meta
| | |
| --- | --- |
| **ID** | `P9-2` · **Phase** 9 — Release & Operations |
| **Work ID** | `W-0051` (canonical tracker §5) |
| **Prereq** | `P9-1` |
| **Governance** | Production live (`REAL_CUSTOMER_CALL_ALLOWED=true` sau P9-1) — vận hành an toàn, có rollback |
| **Stack** | K8s prod · runbook/ops |

## 1. ROLE
Bạn là **Senior SRE / Ops Lead**. Bạn hoàn thiện khả năng vận hành production: cutover có kiểm soát, rollback tin cậy, ops runbook (on-call, incident, DR/backup), và tuân thủ retention/legal. Bạn đảm bảo IVR chạy bền vững và khôi phục được sau sự cố.

## 2. CONTEXT
Sau khi gate mở (P9-1), IVR gọi khách thật ở production. Cần quy trình vận hành đầy đủ để duy trì an toàn/tuân thủ và xử lý sự cố. Đây là bước làm cho hệ "sống được lâu dài", khép vòng zero→production.

## 3. SOURCE SPECS (đọc trước)
- `specs/architecture/05-resilience.md`, `specs/architecture/06-observability.md`, `specs/database/05-retention-and-privacy.md`, `specs/database/06-migration-plan.md`
- `plan/ivr-orther/decisions-log.md` §DF-07 (retention/legal), §DT-05 (recording), §DO-06 (fail-closed), §DF-03 (kill-switch); `plan/ivr-orther/14-risk-register.md`
- `prompt/phase-8-sim-pilot/P8-2-pilot-runbook.md` (pilot runbook source; generated docs artifact only if P8-2 creates it)

## 4. DECISIONS & CONSTRAINTS
- **Cutover:** canary/blue-green (P7-3), ramp-up có kiểm soát (không mở đại trà ngay); giám sát sát.
- **Rollback:** tin cậy (helm revision + kill-switch REAL→MOCK); DB migration rollback an toàn.
- **DR/backup:** Postgres backup + restore test; RTO/RPO xác định.
- **Retention/legal (DF-07):** CronJob retention (P7-2) chạy đúng; recording OFF (DT-05); audit lưu theo chính sách.
- **On-call/incident:** rota, severity, escalation, postmortem; alert (P6-2) nối.
- **Fail-closed** giữ trong mọi sự cố downstream.

## 5. INPUTS / DEPENDENCIES
- Prod env (P7-3) + gate mở (P9-1); dashboards/alerts (P6-2); backup infra; on-call tooling.

## 6. BUILD STEPS
1. **Cutover plan** `docs/cutover-plan.md`: canary %, ramp-up schedule, checkpoints, abort criteria, comms.
2. **Rollback**: procedure + script (helm rollback + kill-switch REAL→MOCK), test rollback trong staging/pilot; DB migration down-safety.
3. **DR/backup**: Postgres backup schedule + **restore test** (verify RTO/RPO); runbook khôi phục.
4. **Ops runbook** `docs/ops-runbook.md`: on-call rota, common incidents (SIM down, downstream fail-closed, callback backlog, capacity), diagnostic steps (dùng trace/dashboard P6), escalation, kill-switch usage.
5. **Retention/legal ops**: verify CronJob retention chạy (DF-07); recording OFF; audit retention; định kỳ compliance check.
6. **Incident/postmortem template** + link risk register (14-risk-register).
7. **Steady-state checklist**: daily/weekly ops tasks, capacity review, SLO review.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/cutover-plan.md`, `docs/rollback.md` | Cutover + rollback |
| `docs/ops-runbook.md`, `docs/incident-postmortem-template.md` | Ops + incident |
| `docs/dr-backup.md` | DR/backup + restore test |
| `deploy/ops/**` | Backup CronJob, retention verify |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-OPS-ROLLBACK-01` | drill | rollback (helm + kill-switch) khôi phục về trạng thái an toàn trong RTO. |
| `IT-OPS-DR-02` | drill (**staging/pilot trên bản restore, không chạy trên prod live**) | Postgres restore từ backup thành công; RPO đạt. |
| `IT-OPS-RETENTION-03` | integration (**staging, `DryRun=true` trước; real-run cần approval**) | CronJob retention xoá đúng class theo DF-07; audit giữ đúng hạn. |
| `IT-OPS-INCIDENT-04` | drill (**staging/pilot; nếu buộc chạy ở prod phải kill-switch REAL→MOCK trước**) | kịch bản SIM down/downstream fail-closed → runbook dẫn tới khôi phục; alert đúng. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] rollback + DR tested (không chỉ viết); [ ] retention/legal chạy; [ ] on-call/incident đủ; [ ] fail-closed giữ; [ ] kill-switch usable.
**Reviewer:** RTO/RPO thực tế; cutover ramp an toàn; postmortem loop; compliance định kỳ.

## 10. EVIDENCE EXPECTED
Rollback drill log, DR restore test (RTO/RPO), retention CronJob run, incident drill report, cutover checkpoints.

## 11. FORBIDDEN
- ❌ Chạy bất kỳ drill nào trên production live mà không kill-switch `REAL→MOCK` trước, không có blast radius đặt tên và không có owner approval ghi vào tracker.
- ❌ Retention real-run trên production khi chưa có `DryRun` report được review.
- ❌ Production không rollback/DR đã test. ❌ Bỏ retention/legal (DF-07). ❌ Bật recording không consent (DT-05). ❌ "Mở cửa" khi downstream sự cố (giữ fail-closed).

## 12. DEFINITION OF DONE
- [ ] Cutover + rollback + DR + ops runbook + retention; 4 drill §8 pass; evidence §10 đủ. **KẾT THÚC: IVR vận hành production bền vững, khép vòng zero→production.**

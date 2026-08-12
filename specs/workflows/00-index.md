# Workflows — Index

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p04-generate-workflows.md`
Nguồn: `phase-8/14` (điều phối), `phase-8/05` (scheduler/queue), `phase-8/07` (result/callback), `phase-8/23` (UC xác nhận), `docx` §3,§8,§13,§14.

## Actors (dùng chung trong sequence)
`OrderCore` (Commerce Order Core) · `IVR` (IVR Runtime: intake/eligibility) · `Sched` (Scheduler/Queue) · `SIM` (Internal SIM Gateway Adapter) · `Norm` (Result Normalizer) · `Evid` (Evidence/Audit) · `OpsCore` (Operational Core: blocker).

## Danh sách luồng
| File | Luồng |
| --- | --- |
| [01-happy-path-confirm.md](01-happy-path-confirm.md) | Khách bấm `1` — xác nhận |
| [02-cancel.md](02-cancel.md) | Khách bấm `0` — hủy |
| [03-no-answer-attempts.md](03-no-answer-attempts.md) | Không nghe → attempt 2 → final |
| [04-invalid-phone.md](04-invalid-phone.md) | Số không hợp lệ |
| [05-technical-exception.md](05-technical-exception.md) | Lỗi kỹ thuật (≠ no-answer) |
| [06-race-condition-revalidation.md](06-race-condition-revalidation.md) | Phím `1` + blocker/version mismatch |
| [07-trusted-skip.md](07-trusted-skip.md) | Khách trusted — skip IVR |
| [08-capacity-hold.md](08-capacity-hold.md) | Nghẽn capacity |
| [09-state-machines.md](09-state-machines.md) | State machine CallJob/Attempt/Result/Callback |

## Nguyên tắc chung (mọi luồng)
- Kết thúc bằng **callback → Order Core revalidate**; IVR không transition order (P0-IVR-002/003).
- Ghi **evidence/audit** tại: intake, eligibility, attempt, DTMF, result, callback.
- Attempt policy ✅ **D-10 (LOCKED)** (GH 5′: A2@T0+2:30, expire T0+5:00; 24/7 15′: A2@T0+7:30, expire T0+15:00; `T0`=lúc Core mở window; max 2 cả hai).
- Notification chỉ **sau** Core decision (P0-IVR-008).

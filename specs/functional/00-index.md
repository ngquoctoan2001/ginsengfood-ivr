# Functional SRS — Index

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p03-generate-functional-srs.md`
Phạm vi: outbound order-confirmation ([../01-context-and-scope.md](../01-context-and-scope.md)). Attempt policy: ✅ **D-10 (LOCKED)** — rule mới PACK-09 V1.0 (2 cuộc cả hai; GH 5′, 24/7 15′; `T0`=lúc Core mở window). Xem `plan/ivr-orther/decisions-log.md`.

## 1. Cấu trúc
| File | Domain |
| --- | --- |
| [01-task-intake.md](01-task-intake.md) | Nhận task từ Order Core, validate, idempotency |
| [02-eligibility-and-blockers.md](02-eligibility-and-blockers.md) | Entry gate, trusted skip, phone, Sale Lock/Recall/Suppression |
| [03-scheduler-attempt-policy.md](03-scheduler-attempt-policy.md) | Golden Hour / 24/7, deadline-aware rolling queue |
| [04-call-execution-dtmf.md](04-call-execution-dtmf.md) | Call script, phím 1/0, no-answer, invalid, wrong input |
| [05-result-normalization-callback.md](05-result-normalization-callback.md) | Result taxonomy, callback, revalidation |
| [06-technical-exception-capacity.md](06-technical-exception-capacity.md) | Technical ≠ no-answer, capacity incident |
| [07-admin-operations.md](07-admin-operations.md) | Monitor, pause/resume, SIM enable/disable, retry, review |
| [08-evidence-audit-privacy.md](08-evidence-audit-privacy.md) | Evidence, audit, PII/privacy |

## 2. Quy ước ID
- Giữ mã gốc phase-8 (`IVRxx-FR-*`, `IVRxx-P0-*`) và docx (`M8-*`, `P0-*`) qua bảng ánh xạ; mã mới ở đây dùng tiền tố **`FR-IVR-<domain>-nnn`** và **`P0-IVR-nnn`**. Chuẩn hóa scheme chính là OD-DR-02.
- Mỗi FR: mô tả · nguồn · acceptance hint. Precondition/trigger/postcondition ghi ở đầu nhóm khi chung.

## 3. P0 boundary xuyên suốt (tổng hợp — chi tiết trong từng file)
| P0 | Nội dung | Nguồn |
| --- | --- | --- |
| P0-IVR-001 | IVR chỉ xử lý **Official Order**; từ chối Quote/Cart/Order Draft | phase-8/00 P0-001; docx M8-FAIL-001 |
| P0-IVR-002 | IVR result là **signal**; không tự transition order; SIM không ghi order | phase-8/00 P0-002; docx P0-05, M8-FAIL-002 |
| P0-IVR-003 | Order Core **revalidate** callback; phím `1` không confirm khi blocker active | phase-8/00 P0-003; phase-8/07 §8 |
| P0-IVR-004 | **Technical failure tách khỏi no-answer** | phase-8/00 P0-004; docx P0-06, §15 |
| P0-IVR-005 | Invalid phone tách khỏi no-answer | phase-8/07; docx §13 |
| P0-IVR-006 | Evidence/audit bắt buộc; không PASS khi evidence chưa accepted | phase-8/00 P0-005; docx P0-09 |
| P0-IVR-007 | Privacy safe by default; không log raw phone/full profile | phase-8/00 P0-006; docx §17 |
| P0-IVR-008 | Không tự gửi notification; chỉ sau Core decision | phase-8/00 §4; phase-8/02 |
| P0-IVR-009 | Không vượt `MAX_ATTEMPT`; không gọi ngoài window/policy | docx §8, P0-03 |
| P0-IVR-010 | `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi release gate pass | phase-8/00 §13; docx §25 |

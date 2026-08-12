# P09 — Generate Integration Requirements

## Tên nhiệm vụ
Sinh tài liệu yêu cầu tích hợp gửi các team: sales platform (module 3/3.1), ops-core (module 1/2), telephony/SIM provider.

## Bối cảnh
IVR do người phụ trách module 1/2/8 xây; KHÔNG trực tiếp phụ trách module 3/3.1. Vì vậy cần tài liệu chính thức nêu rõ IVR cần gì từ sales/ops, API nào còn thiếu, ai xây. Đây là bản chuyển từ draft (`plan/.../11`, `12`) thành yêu cầu gửi đi.

## Input cần đọc
- `plan/ivr-orther/10-integration-gap-analysis.md`
- `plan/ivr-orther/11-sales-platform-api-needs-draft.md`
- `plan/ivr-orther/12-ops-core-api-needs-draft.md`
- `specs/srs/api/*`, `specs/srs/data/*`
- `docs/documents/4. phase/phase-8/17-THIẾT KẾ TÍCH HỢP.md`, `02-...KẾT NỐI.md`
- `docs/documents/4. phase/phase-3.1/07-THANH TOÁN VẬN CHUYỂN IVR VÀ ĐƠN HÀNG.md` (IVRRequiredDecision, quota release)

## Output cần tạo
- `integration-requirements/` (đề xuất ở root, tạo khi chạy p09):
  - `00-index.md`
  - `01-sales-platform-requirements.md` (API/contract/event IVR cần; ưu tiên P0/P1/P2; mock fallback)
  - `02-ops-core-requirements.md` (sale-lock/recall/availability contract)
  - `03-telephony-sim-requirements.md` (SIM gateway protocol, DTMF, call disposition mapping, recording policy)
  - `04-shared-auth-audit-requirements.md` (service identity allowlist, RBAC, evidence registry)
  - `05-open-contract-questions.md` (câu hỏi cần từng team trả lời)

## Quy tắc
- Mỗi yêu cầu: mục đích, priority, input/output mong muốn, sync/async, idempotency, mock được không, ai xây, deadline mong muốn.
- Nêu rõ tension: `IVRRequiredDecision` (phase-3.1: IVR trước order_code) vs phase-8 (IVR sau Official Order) → cần sales xác nhận thứ tự.
- Không coi API bên ngoài là đã tồn tại.

## Checklist hoàn thành
- [ ] Sales/ops/telephony/shared đều có file.
- [ ] Mỗi API need có priority + owner + mock note.
- [ ] Câu hỏi mở tổng hợp.
- [ ] Tension IVR-before/after order_code được nêu.

## Điều cấm
- KHÔNG tự thiết kế nội bộ hệ sales/ops thay họ; chỉ nêu nhu cầu + đề xuất sơ bộ.

## Báo cáo cuối
1. Số yêu cầu theo team.
2. Số P0.
3. Câu hỏi chặn lớn nhất.

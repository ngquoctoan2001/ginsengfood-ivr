# P04 — Generate Workflows

## Tên nhiệm vụ
Sinh workflow và sequence flow cho IVR Order Confirmation.

## Bối cảnh
Baseline `IVR-14` (Điều phối quy trình) đã mô tả 8 luồng: confirm (phím 1), cancel (phím 0), no-answer, invalid phone, technical failure, race condition, trusted skip, capacity hold. Prompt này chuẩn hóa thành workflow docs + Mermaid sequence diagrams.

## Input cần đọc
- `specs/srs/functional/*` (từ p03)
- `docs/documents/4. phase/phase-8/14-ĐIỀU PHỐI QUY TRÌNH.md`
- `docs/documents/4. phase/phase-8/05-CHÍNH SÁCH GỌI LẠI BỘ LẬP LỊCH VÀ HÀNG ĐỢI.md`
- `docs/documents/4. phase/phase-8/07-CHUẨN HÓA KẾT QUẢ VÀ CALLBACK VỀ LÕI ĐƠN HÀNG.md`
- `docs/documents/4. phase/phase-8/23-XÁC NHẬN ĐƠN HÀNG BẰNG IVR.md`

## Output cần tạo
- `specs/srs/workflows/` gồm:
  - `00-index.md`
  - `01-happy-path-confirm.md`
  - `02-cancel.md`
  - `03-no-answer-attempts.md`
  - `04-invalid-phone.md`
  - `05-technical-exception.md`
  - `06-race-condition-revalidation.md`
  - `07-trusted-skip.md`
  - `08-capacity-hold.md`
  - `09-state-machines.md` (CallJob state, Result state, Attempt state)
- Mỗi file có 1 Mermaid `sequenceDiagram` và/hoặc `stateDiagram`.

## Quy tắc
- Actor trong sequence: Order Core, IVR Runtime, Scheduler, SIM Adapter, Result Normalizer, Evidence Registry, Operational Core.
- Mọi luồng phải kết thúc bằng callback → Order Core revalidate (không để IVR transition order).
- State machine phải khớp `IVR-07` (result states) và `IVR-12` (job/attempt states).
- Ghi rõ điểm ghi evidence/audit trong từng luồng.

## Checklist hoàn thành
- [ ] Đủ 8 luồng + state machines.
- [ ] Mỗi luồng có diagram render được.
- [ ] Race condition (phím 1 + Sale Lock) thể hiện rõ Core block.
- [ ] Attempt policy **D-10** (GH 5′ A2@T0+2:30; 24/7 15′ A2@T0+7:30; max 2 cả hai; `T0`=lúc Core mở window) thể hiện trong no-answer flow.

## Điều cấm
- KHÔNG thêm luồng inbound/order-lookup/gặp nhân viên trừ khi owner duyệt mở scope.
- KHÔNG mô tả IVR tự gửi SMS/notification.

## Báo cáo cuối
1. Số workflow + state machine đã sinh.
2. Diagram nào cần dữ liệu chưa có (đánh `TODO`).

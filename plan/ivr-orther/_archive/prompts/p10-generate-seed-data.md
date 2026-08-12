# P10 — Generate Seed Data

## Tên nhiệm vụ
Sinh seed/mock data thật cho môi trường non-production.

## Bối cảnh
Khi chưa có API thật từ sales/ops, IVR cần seed/mock để chạy dry-run/smoke. Chiến lược seed đã lập ở `plan/ivr-orther/13-seed-and-mock-strategy-plan.md`. Prompt này mới thực sự sinh file seed.

## Input cần đọc
- `plan/ivr-orther/13-seed-and-mock-strategy-plan.md`
- `specs/srs/database/*`, `specs/srs/data/*`, `specs/srs/workflows/*`
- `docs/documents/4. phase/phase-8/09-MA TRẬN KIỂM THỬ KHÓI VÀ CỔNG PHÁT HÀNH.md` (smoke scenarios)

## Output cần tạo
- `seed/` (đề xuất ở root, tạo khi chạy p10):
  - `README.md` (cách dùng, cảnh báo non-prod, cách gỡ khi có API thật)
  - `customers.sample.json` (giả lập projection từ sales)
  - `orders.sample.json` (official orders đủ điều kiện IVR, đủ program GOLDEN_HOUR/24-7)
  - `products.sample.json`, `inventory.sample.json` (giả lập ops, gồm sale-lock/recall case)
  - `ivr-tasks.sample.json` (`IvrConfirmationTaskV1` mẫu)
  - `call-scenarios.sample.json` (confirm/cancel/no-answer/invalid/technical/race/trusted-skip)
  - `ivr-menu.sample.json` (script + phím 1/0)
  - `agents.sample.json` (admin/ops actors + permission)
  - `integration-status.sample.json` (sales/ops/SIM up|down để test fail-safe)

## Quy tắc
- Seed KHÔNG chứa PII thật; phone dùng dải test, dùng masked/token.
- Mỗi scenario map tới ít nhất 1 smoke case của `IVR-09`.
- Phân tách rõ seed nào là IVR-owned, seed nào giả lập sales, seed nào giả lập ops.
- Recording OFF; SIM channel ở trạng thái disabled/non-prod.

## Checklist hoàn thành
- [ ] Đủ domain seed theo strategy.
- [ ] Đủ tình huống: 1 đơn/nhiều đơn/không tồn tại/đang giao/đã giao/hủy/cần xác nhận/còn hàng/hết hàng/sales down/ops down/webhook duplicate/missed call/callback/gặp nhân viên.
- [ ] README nêu cách bỏ mock khi có API thật.
- [ ] Không PII thật.

## Điều cấm
- KHÔNG seed vào môi trường production.
- KHÔNG bật recording/real SIM.

## Báo cáo cuối
1. Số file seed + số record.
2. Coverage smoke scenario.
3. Seed nào cần thay bằng API thật sớm nhất.

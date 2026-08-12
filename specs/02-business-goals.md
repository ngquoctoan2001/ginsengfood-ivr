# SRS-02 — Business Goals

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p02`
Nguồn chính: `docs/MODULE_8_...V0.2.docx` §4, §16; `docs/documents/4. phase/phase-8/01`; `docs/documents/4. phase/phase-3.1/07`.

## 1. Mục tiêu kinh doanh

| ID | Mục tiêu | Căn cứ | Chỉ số theo dõi (docx §16) |
| --- | --- | --- | --- |
| BG-01 | **Xác nhận ý chí đặt hàng thật** của khách sau khi đã có Official Order hợp lệ | CONFIRMED — docx §4; phase-8/01 | `confirm_rate` (tỷ lệ bấm 1 / task đủ điều kiện) |
| BG-02 | **Giảm đơn ảo / sai số điện thoại / đặt nhầm / không xác nhận**, đặc biệt trong Giờ Vàng | CONFIRMED — docx §4; phase-3.1/07 (anti-fake, high-risk) | `cancel_rate` (bấm 0), `no_answer_rate` |
| BG-03 | **Bảo vệ vận hành kho/giao hàng**: chỉ chuyển tiếp đơn đã có tín hiệu xác nhận hợp lệ hoặc được Order Core cho phép | CONFIRMED — docx §4 | đơn chuyển fulfillment sau xác nhận |
| BG-04 | **Không làm phiền khách quá mức**: tôn trọng opt-out, giới hạn attempt/window, không gọi ngoài policy | CONFIRMED — docx §4, §17 | `no_answer_rate`, opt-out compliance |
| BG-05 | **Tạo evidence rõ ràng** cho CSKH/vận hành/owner khi có khiếu nại/tranh chấp | CONFIRMED — docx §4, §21 | evidence accepted rate |
| BG-06 | **Vận hành ổn định về capacity** (không nghẽn SIM Giờ Vàng, không dồn cuối phiên) | CONFIRMED — docx §11, §12 | `missed_deadline_count`, `technical_exception_rate`, `sim_failure_rate` |
| BG-07 | **Tối ưu chi phí gọi / đơn xác nhận** | CONFIRMED — docx §16 | `cost_per_confirmed_order`, `call_success_rate` |

## 2. Chỉ số vận hành (metrics) cần đo (docx §16)
`call_success_rate`, `confirm_rate`, `cancel_rate`, `no_answer_rate`, `technical_exception_rate`, `missed_deadline_count`, `sim_failure_rate`, `cost_per_confirmed_order`. Theo dõi theo ngày/phiên/program.

## 3. Non-goals (không phải mục tiêu)
- KHÔNG dùng IVR để tăng doanh thu trực tiếp (upsell/cross-sell/tư vấn) — BG chỉ là xác nhận & chống gian lận.
- KHÔNG dùng IVR result để tính revenue/ROAS/commission (chỉ `ORDER_VERIFIED` từ Commerce). Nguồn: docx §18 (Module 6).
- KHÔNG thay thế CSKH/CRM; KEY_9 chưa bật.

## 4. Liên kết
- Ràng buộc bởi scope: [01-context-and-scope.md](01-context-and-scope.md).
- Đo lường chi tiết: sẽ cụ thể hóa ở `specs/srs/testing/*` (p11) và UI dashboard (p12).

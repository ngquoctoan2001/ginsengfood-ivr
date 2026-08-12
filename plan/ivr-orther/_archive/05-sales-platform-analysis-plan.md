# 05 — Sales Platform Analysis Plan (Module 3 / 3.1)

Kế hoạch phân tích riêng module 3/3.1 để sau này sinh: sales data mapping, sales required APIs, order/customer workflow, integration contract, request chính thức gửi team sales.

## 1. Tài liệu sales đã đọc

- CONFIRMED: `phase-3/00` (gap analysis), `phase-3/05` (official order + state machine), `phase-3/06,07` (payment/shipping/invoice, verified revenue) — scan.
- CONFIRMED: `phase-3.1/00` (analysis), `phase-3.1/07` (**IVR connector — CRITICAL**), `phase-3.1/03` (member policy/commission), `phase-3.1/05` (CRM 12 tháng), `phase-3.1/5. bo sung/03` (IVR extra time 5 phút) — scan.
- CONFIRMED: `3. tech/05-TECH-04` (commerce runtime), `3. tech/06-TECH-05` (AI advisor handoff), `2. pack/05-PACK-05` — scan.

## 2. Tài liệu sales cần đọc tiếp (`TODO`)

- `phase-3/01-04, 08-11` (sellable gate, quote/cart, draft, smoke, SRS handoff) — đọc chi tiết để chốt order state machine & QuoteSnapshot.
- `phase-3.1/01,02,04,06,08-11` + `5. bo sung/00,01,02,04,05` — để chốt program/golden-hour/CRM/diamond ảnh hưởng attempt policy.
- `2. pack/04-PACK-04-MISA` — xác nhận IVR không chạm accounting.
- `docs/MODULE_8_...docx` — đối chiếu phần IVR-sales.

## 3. Dữ liệu IVR cần từ sales platform

- CONFIRMED (task payload): `order_id`, `order_code_short`, `order_version`, `order_state`, `program_code`, `attempt_policy`, `customer_trust_status`, `trusted_skip_allowed`, `risk_flags`, `official_contact_id`, `phone_ref/masked/validation`, `call_script_template_id/version`, `allowed_script_variables`. Nguồn: phase-8/04.
- ASSUMPTION (nếu mở inbound): customer-by-phone, order-by-phone, order detail, payment status, shipping ETA, member tier/benefit. Nguồn: report sales §D/G — nhưng **out-of-scope phase-8**.

## 4. API IVR có thể cần (chi tiết ở [11](11-sales-platform-api-needs-draft.md))

- P0: Order Core tạo task (push) + nhận callback (IVR→Core). Đây là contract lõi.
- P1: `GET IVRRequiredDecision` / event khi order cần IVR; API release Golden Hour quota khi IVR fail/timeout.
- P2 (inbound, nếu mở scope): customer/order lookup by phone, order/payment/shipping status, customer call note.

## 5. Trạng thái order cần thống nhất (P0)

- `NEED_CONFIRMATION`: Danh sách chính xác **order_state là "IVR-callable"** và state sau khi Core accept từng result type (`IVR_CONFIRMED` → ?, `IVR_CUSTOMER_CANCELLED` → ?, `IVR_NO_ANSWER_FINAL` → ?). Order Core owner phải chốt.
- `NEED_CONFIRMATION`: Quan hệ `order_code` với thời điểm IVR (trước/sau) — tension phase-3.1 vs phase-8.

## 6. Câu hỏi cần hỏi team sales (tóm tắt, chi tiết ở [15](15-open-questions.md))

1. IVR chạy trước hay sau khi có `order_code`?
2. State machine order (tên state, IVR-callable set, transition sau mỗi result type)?
3. Order Core sẽ **push task** sang IVR hay IVR **poll**? Contract transport?
4. Trust decision & risk flags lấy từ resolver nào, format ra sao?
5. Official contact projection (phone_ref/masked/token) do sales cấp thế nào, policy dial token TTL?
6. Golden Hour quota release khi IVR fail/timeout: API hay event?
7. (Nếu inbound) có cần lookup by phone / call note không?

## 7. Phần có thể mock tạm thời

- Order task payload, order state, trust decision, phone projection, program/attempt policy → seed `orders.sample.json`, `ivr-tasks.sample.json`, `customers.sample.json` (xem [13](13-seed-and-mock-strategy-plan.md)).
- Callback intake (Order Core) → mock endpoint trả `CALLBACK_ACCEPTED/STALE/BLOCKED/REVIEW`.

## 8. Thứ tự sinh specs liên quan sales

1. Data mapping sales (p06 → `specs/srs/data/02-mapping-sales-platform.md`).
2. Order Core contracts (p05 → `specs/srs/api/05-order-core-contracts.md`).
3. Integration requirements sales (p09 → `integration-requirements/01-sales-platform-requirements.md`).
4. Workflow confirm/cancel/no-answer gắn order state (p04).

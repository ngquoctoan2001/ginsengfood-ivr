# Yêu cầu bàn giao Order-State Contract — gửi Order Core (Module 3)

Người gửi: Team IVR / Module 8 (IVR Order Confirmation — phase-8 / PACK-09)
Ngày gửi: 2026-07-02
Trạng thái: ✅ **ĐÃ TRẢ LỜI (2026-07-02, đọc source)** — QS-01..05 → **DS-01..05** ([decisions-log.md](decisions-log.md)). Mã việc: **DG-03**.

> **Đính chính lớn từ source (khác mock của ta):**
> - **QS-01→DS-01:** order_status thật = `CONFIRMING/CONFIRMED/PACKED/SHIPPING/DELIVERED/FAILED/CANCELLED/EXPIRED`. **IVR-callable = CHỈ `CONFIRMING` + `payment_method_snapshot=COD`.** → 🆕 **IVR chỉ cho đơn COD**; state chờ IVR = **`CONFIRMING`** (không phải CONFIRMATION_REQUIRED/IVR_PENDING). `is_ivr_callable` là rule derive, không phải field.
> - **QS-02→DS-02:** confirm→`CONFIRMED`, cancel→`CANCELLED`, window-expired→`timeout: CONFIRMING→EXPIRED`. **no-answer/technical/blocked KHÔNG có transition Order Core** (chỉ set `ivr_call_queue`; order chờ expire). Không có `HOLD/BLOCKED`.
> - **QS-03→DS-03:** Core nhận result chỉ khi CONFIRMING+COD; else → `422`. **Chưa có** `CALLBACK_REJECTED_STALE` / `order_version_seen_by_ivr` check.
> - **QS-04→DS-04:** có `orders.version` (JPA @Version) nhưng **chưa expose**; callback DTO không nhận `order_version_seen_by_ivr` → race-guard = GAP.
> - **QS-05→DS-05:** fulfillment gate = `order_status` (shipment cần `CONFIRMED`); downstream gate = `ORDER_VERIFIED` (DELIVERED+PAID+VERIFIED/TRUSTED).
>
> Ô chi tiết + source path bên dưới giữ làm biên bản gốc.
Ưu tiên: **P1** — chặn **integration/contract test đầy đủ** cho callback (không chặn specs; hiện IVR chạy dry-run/MOCK).

## 0. Bối cảnh (đã chốt ở vòng trước)
Vòng 1 chúng ta đã khóa **model** order-state (D-02):
- Order Core cấp cho IVR: `order_state` (**enum "đục"** — IVR không suy diễn), `order_version` (race guard), `is_ivr_callable` (cờ).
- Transition do **Order Core** quyết; IVR chỉ gửi **signal** (`ResultCallbackV1`), Core revalidate (D-04).
- order_code cấp khi tạo Official Order; đơn vào `CONFIRMATION_REQUIRED/IVR_PENDING`; fulfillment/downstream khóa tới khi Core chấp nhận IVR signal (D-01).

**Đã trả lời (DG-03):** *giá trị enum cụ thể* + *bảng transition per result type* đã có qua **DS-01..05**. IVR dùng bản này để viết **integration/contract test** và để mock Core khớp thật; phần race-guard/callback-code rich còn là IR-SALES-OC1/OC2 target.

> IVR **không** cần Core lộ toàn bộ state machine nội bộ — chỉ cần: (a) tập state cho phép gọi IVR, (b) hệ quả sau mỗi result type, (c) khi nào callback bị coi là stale.

**Cách trả lời:** mỗi mục có *"Đề xuất từ IVR"* — chọn **[ ] Xác nhận** / **[ ] Điều chỉnh**, điền ô **Trả lời**.

---

### QS-01 (P1) — Tập order_state & state "IVR-callable"
**Đề xuất từ IVR:** cung cấp danh sách `order_state` liên quan IVR và đánh dấu state nào `is_ivr_callable=true`. IVR đang dùng (mock) các giá trị: `CONFIRMATION_REQUIRED` / `IVR_PENDING` (callable), `DELIVERED` / `CANCELLED` / `DRAFT` (không callable).

- [ ] Xác nhận danh sách/tên · [ ] Điều chỉnh (cấp enum thật)
- **Trả lời (liệt kê state + cờ callable):** ______________________________________________
- Người trả lời / ngày: __________

### QS-02 (P1) — Bảng transition per result type ⭐
**Đề xuất từ IVR:** với mỗi result IVR (đã khóa D-02), cho biết Core chuyển sang state nào (sau revalidate PASS):

| IVR result → | Core action (D-02) | State đích (đề xuất — cần Core điền) |
| --- | --- | --- |
| `IVR_CONFIRMED` | tiếp tục xử lý nếu revalidate pass | `CONFIRMED` / `PAYMENT_PENDING`? |
| `IVR_CUSTOMER_CANCELLED` | Core cancel | `CANCELLED` (reason=CUSTOMER_CANCELLED_BY_IVR_KEY_0)? |
| `IVR_NO_ANSWER_FINAL` | Core cancel/hold theo policy | `CANCELLED`/`HOLD` (reason=IVR_NO_ANSWER_AFTER_2_ATTEMPTS)? |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | Core expire/cancel/hold | `EXPIRED`/`CANCELLED`? |
| `IVR_TECHNICAL_EXCEPTION` | admin review / retry (không tính no-answer) | giữ nguyên `CONFIRMATION_REQUIRED`? |
| `IVR_OPERATIONAL_BLOCKED` (race recall/sale-lock) | block/hold | `HOLD`/`BLOCKED`? |

- [ ] Xác nhận · [ ] Điều chỉnh (đính kèm bảng chính thức + reason codes)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QS-03 (P1) — State "còn nhận IVR result" vs stale (cho revalidate callback)
**Đề xuất từ IVR:** callback kèm `order_version_seen_by_ivr`; nếu order đã rời state callable hoặc version đã đổi → Core trả `CALLBACK_REJECTED_STALE`. Xin xác nhận: state nào Core **vẫn nhận** IVR result, state nào coi là stale.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QS-04 (P1) — Quy tắc bump `order_version`
**Đề xuất từ IVR:** `order_version` tăng khi đơn đổi (amount/items/contact/state). IVR dùng nó làm race guard; version lệch → stale. Xin cho biết khi nào Core bump version (để test stale chính xác).

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QS-05 (P1) — Cờ fulfillment gating (D-01)
**Đề xuất từ IVR:** có một cờ/state cho biết **fulfillment/downstream đang bị khóa chờ IVR** (D-01). IVR không cần điều khiển cờ này, chỉ cần biết tên để trace/test. Xin xác nhận tên field/state.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

---

## Tổng hợp
| Mục | Chủ đề | Ưu tiên |
| --- | --- | --- |
| QS-01 | Tập order_state + callable | P1 |
| QS-02 | Bảng transition per result ⭐ | P1 |
| QS-03 | State còn nhận vs stale | P1 |
| QS-04 | Bump order_version | P1 |
| QS-05 | Cờ fulfillment gating | P1 |

**Quan trọng nhất:** QS-02 (bảng transition) — mở khoá integration/contract test (`specs/srs/testing/03,04`) và mock Core (`seed/`).

## Ô tổng kết
- Người duyệt Order Core: ____________ · Ngày: ______
- Ghi chú: ______________________________________________

---
Tham chiếu: `plan/ivr-orther/decisions-log.md` (D-01/D-02/D-04, DS-01..05), `specs/srs/data/04-missing-data.md` (DG-03 resolved), `specs/srs/api/05-order-core-contracts.md`, `specs/srs/_review/open-decisions-register.md`.

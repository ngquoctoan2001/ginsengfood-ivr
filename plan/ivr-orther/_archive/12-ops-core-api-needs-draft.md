# 12 — Ops-Core API Needs (DRAFT)

✅ **Cập nhật 2026-07-02 — Ops-Core đã trả lời (DO-01..DO-09, [decisions-log.md](decisions-log.md)).** Khác bản nháp ở 3 điểm nền tảng: (1) ops **không biết `order_id`** → Order Core fan-out theo SKU/batch; (2) blocker gộp **đã có sẵn** = **sellable gate** `POST /api/v1/admin/availability/check` → `SellableStatus`; (3) **do-not-call/opt-out KHÔNG thuộc ops** (thuộc CRM), và **sale-lock hiện = recall-triggered**. Các API-O bên dưới giữ làm nháp gốc; trạng thái thực tế xem DO-*.

⚠️ (Bản nháp gốc) Nhãn scope: **[CORE]** (cần cho outbound confirm) / **[INBOUND?]** (chỉ nếu mở inbound).

Nguyên tắc quan trọng: **Nếu Order Core đã nhúng blocker snapshot trong task và tự revalidate, IVR nên đi qua Order Core thay vì gọi ops trực tiếp.** Chỉ mở đường IVR→ops khi cần realtime mà Core không đảm bảo.

---

## API-O1 [CORE] Sale-lock / Recall / Suppression status
- Mục đích: Chặn xác nhận đơn khi sản phẩm/lô bị khóa bán / thu hồi / suppression.
- Priority: **P0**. Endpoint sơ bộ: `GET /v1/operational/sale-lock-status?sku=...&batch=...` (hoặc gộp `blocker-status?order_id=...`).
- Input: sku_id / batch_id / order_id.
- Output: sale_lock (active, scope, reason, effective_at), recall (active, scope, reason), suppression (active).
- Write? Không. Idempotency? N/A (read). Mock? **Có**.
- Nên gọi trực tiếp từ IVR? **Ưu tiên KHÔNG** — nhận snapshot qua task + Core revalidate. Nhưng bước revalidate cần **realtime** → có thể cần đường realtime (Core gọi ops, không phải IVR).
- Câu hỏi: Ai gọi realtime lúc revalidate — Order Core hay IVR? SLA latency? Có event push khi activate giữa cuộc gọi?

## API-O2 [CORE/P1] Availability / stock check
- Mục đích: Blocker tồn kho khi revalidate (không xác nhận đơn không đủ hàng).
- Priority: **P1**. Endpoint sơ bộ: `GET /v1/operational/availability?sku=...&batch=...`.
- Output: available_qty, warehouse, sale_lock_state.
- Write? Không. Mock? Có.
- Nên gọi trực tiếp từ IVR? **Không** — availability thường do **commerce/sellable gate** tổng hợp; IVR nên để Order Core xử lý.
- Câu hỏi: Nguồn availability cho revalidate là ops-core (lot-level) hay commerce (aggregate)?

## API-O3 [INBOUND?] Public trace lookup (batch VALID/RECALLED)
- Mục đích: (nếu inbound) khách hỏi "sản phẩm/lô này có an toàn không?".
- Priority: **P2**. Endpoint sơ bộ: `GET /v1/trace/public?batch_code=...` hoặc `?qr_token=...`.
- Output (whitelist): product_name (public), batch code (masked/public), MFG/EXP (nếu duyệt), status (VALID/ON_HOLD/RECALLED/EXPIRED/NOT_FOUND), public recall message, contact.
- KHÔNG expose: supplier, raw lot, cost, MISA, QC note, internal IDs.
- Write? Không. Mock? Có.
- Câu hỏi: IVR có được gọi public trace không, whitelist trường nào? (Chỉ khi mở inbound.)

## API-O4 [INBOUND?] Product master / public name / ingredient list
- Mục đích: (nếu script cần tên/thành phần sản phẩm).
- Priority: **P2**. Endpoint sơ bộ: `GET /v1/products/{id}` (public projection).
- Nên gọi trực tiếp từ IVR? **Không** — lấy qua **commerce catalog** hoặc **PACK-05 product knowledge**, không trực tiếp ops (ops chỉ có internal BOM, chưa có "public ingredient list").
- Câu hỏi: Có cần tên/thành phần trong call script không? (thường chỉ cần `order_code_short`, `total_amount_display`).

## API-O5 [INBOUND?] Recall impact / fulfillment/warehouse status
- Mục đích: (nếu ops tham gia fulfillment) — thường không cần cho outbound confirm.
- Priority: **P2**. Endpoint sơ bộ: `GET /v1/operational/recall-impact?...`, `GET /v1/operational/fulfillment-status?order_id=...`.
- Nên gọi trực tiếp từ IVR? **Không** — thuộc ops/commerce, không phải IVR.

---

## Bảng ưu tiên tổng hợp

| API | Scope | Priority | IVR gọi trực tiếp? | Mock |
| --- | --- | --- | --- | --- |
| API-O1 Sale-lock/recall/suppression | CORE | P0 | Ưu tiên qua Order Core; realtime cần làm rõ | yes |
| API-O2 Availability | CORE | P1 | Không (qua commerce/Core) | yes |
| API-O3 Public trace | INBOUND? | P2 | Chỉ nếu mở inbound | yes |
| API-O4 Product master/ingredient | INBOUND? | P2 | Không (qua commerce/PACK-05) | yes |
| API-O5 Recall impact/fulfillment | INBOUND? | P2 | Không | yes |

## Kết luận
- CONFIRMED: Nhu cầu ops-core **P0 duy nhất** cho outbound confirm = **trạng thái blocker (sale-lock/recall/suppression)**, và tốt nhất đi **qua Order Core snapshot + revalidate**.
- `NEED_CONFIRMATION` lớn nhất: chiều & độ tươi của blocker khi revalidate (snapshot vs realtime; ai gọi).
- Mọi thứ khác (availability, public trace, product master) IVR nên lấy **gián tiếp qua commerce**, hoặc chỉ cần khi mở inbound.

> Chuyển bản draft này thành `integration-requirements/02-ops-core-requirements.md` khi chạy p09.

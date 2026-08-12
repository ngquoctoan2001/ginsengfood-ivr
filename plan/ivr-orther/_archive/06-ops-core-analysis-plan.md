# 06 — Ops-Core Analysis Plan (Module 1 / 2)

Kế hoạch phân tích riêng module 1/2 để sau này sinh: ops-core data mapping, ops-core required APIs, inventory/product/traceability/recall integration, seed/mock strategy cho dữ liệu ops.

> ✅ **Cập nhật 2026-07-02 — Ops-Core đã trả lời (DO-01..DO-09):** blocker gộp = **sellable gate** (`availability/check` → `SellableStatus`), scope SKU/batch, **ops không biết `order_id`** (Core fan-out); revalidate do **Order Core** gọi (fail-closed qua `/health/ready`); webhook `ops-core.sellable.sku-became-not-sellable.v1`; public trace theo **`qrCode`**; **do-not-call/opt-out = CRM, không phải ops**; sale-lock = recall-triggered. Chi tiết: [decisions-log.md](../decisions-log.md) DO-*.

## 1. Tài liệu ops-core đã đọc

- CONFIRMED: `phase-1/02,03,04` (SKU/material/UOM, BOM, activation), `phase-2/00,05,06` (operational core, batch/QC/release/inventory, **traceability/recall/sale-lock**), `3. tech/03-TECH-02`, `3. tech/04-TECH-03`, `2. pack/01-PACK-01` — scan.

## 2. Tài liệu ops-core cần đọc tiếp (`TODO`)

- `phase-1/00,01,05..11` (product master design, seed governance, print/accounting) — mức thấp với IVR.
- `phase-2/01,02,03,04,07` (tech design, material issue, QC, smoke) — để hiểu vòng đời batch nếu cần trace.
- `2. pack/02,03` (product master, demand/MRP) — bối cảnh.

## 3. Dữ liệu IVR có thể cần từ ops-core

- CONFIRMED (P0, dạng blocker): **Sale Lock** state, **Recall** state, **Suppression** state — để chặn xác nhận đơn khi hàng bị khóa/thu hồi. Nguồn: phase-2/06, phase-8/00/02/04.
- P1: **availability/stock** blocker (khi Core revalidate) — `NEED_CONFIRMATION` ai là nguồn (ops trực tiếp vs commerce tổng hợp).
- P2 (chỉ nếu inbound): public trace (batch VALID/RECALLED), product public name/ingredient. Nguồn: phase-2/06 public trace whitelist — **out-of-scope phase-8**.

## 4. API cần chuẩn bị (chi tiết ở [12](12-ops-core-api-needs-draft.md))

- P0: `GET sale-lock/recall/suppression status` theo sku/batch/order (realtime, low-latency) cho bước revalidate.
- P1: `GET availability` (nếu ops là nguồn).
- P2: `GET public trace` (inbound).

## 5. Phần IVR KHÔNG nên lấy trực tiếp từ ops-core nếu sales đã quản lý

- ASSUMPTION: Nếu Order Core đã nhúng `sale_lock_snapshot`/`recall_snapshot` vào task và tự revalidate khi callback, IVR **không cần** gọi ops-core trực tiếp — chỉ consume snapshot + để Core revalidate. Nguồn: phase-8/04, /07 §8.
- CONFIRMED (boundary): Product master/price/catalog — IVR lấy **qua commerce** (business platform), không trực tiếp ops. "Product Active ≠ Sellable"; sellable là quyết định của commerce dựa trên signal ops. Nguồn: TECH-02/03, report ops §G.

## 6. Câu hỏi cần xác nhận (chi tiết ở [15](15-open-questions.md))

1. Blocker (sale-lock/recall/suppression) IVR nhận **snapshot qua task** hay gọi ops **realtime**? Nếu snapshot, ai đảm bảo tươi tại thời điểm callback?
2. Ai cấp **availability** cho bước revalidate: ops-core hay commerce?
3. Có event push khi sale-lock/recall activate giữa cuộc gọi không (để hold)?
4. (Inbound) IVR có được gọi public trace không, và whitelist trường nào?

## 7. Phần có thể seed/mock

- `products.sample.json`, `inventory.sample.json` gồm case: còn hàng, hết hàng, sale-lock ACTIVE, recall ACTIVE, suppression. → test pre-dispatch block & callback revalidation block (xem [13](13-seed-and-mock-strategy-plan.md)).
- `integration-status.sample.json`: ops-core up/down để test fail-safe "operational blocker check unavailable → không dispatch".

## 8. Thứ tự sinh specs liên quan ops

1. Data mapping ops (p06 → `specs/srs/data/03-mapping-ops-core.md`).
2. Integration requirements ops (p09 → `integration-requirements/02-ops-core-requirements.md`).
3. Blocker check trong workflow (p04) và resilience (p08).

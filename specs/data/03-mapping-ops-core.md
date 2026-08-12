# DATA-03 — Mapping: Ops-Core (Sellable Gate / Sale-Lock / Recall)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p06` · Nguồn: DO-01..DO-07, DO-CORR-1/2/3; `api/05`,`api/06`,`api/08`; `phase-2/06`.

## 1. Nguyên tắc (đã khóa)
- **Ops-core không biết `order_id`** (DO-CORR-1) → **Order Core fan-out** order → từng dòng SKU/batch → gọi sellable gate → nhúng **`sellable_status[]` per-line** vào task (DO-02).
- **IVR không gọi ops trực tiếp**; realtime revalidate do **Order Core** gọi (DO-03). IVR chỉ consume snapshot + kết quả revalidate.
- **Sale-lock = recall-triggered** (DO-CORR-3); **suppression thương mại (do-not-call) KHÔNG thuộc ops** (DO-CORR-2 → xem `02` §1 CRM).

## 2. `sellable_status[]` (mỗi phần tử) ↔ ops SellableStatus
| IVR field (per-line) | Ops `SellableStatus` | Ý nghĩa | Trạng thái |
| --- | --- | --- | --- |
| `sku_id`, `batch_id?` | input `{skuId, batchId?}` | khóa tra | CONFIRMED DO-01 |
| `decision` | `Decision ∈ {SELLABLE,NOT_SELLABLE,BLOCKED,UNKNOWN}` | quyết định gộp | CONFIRMED |
| `recall_hold` | `RecallHold` | recall active | CONFIRMED |
| `sale_lock` | `SaleLock` | khóa bán (recall-triggered) | CONFIRMED DO-CORR-3 |
| `quality_hold` | `QualityHold` | giữ chất lượng | CONFIRMED |
| `stock_available` | `StockAvailable` | còn hàng | CONFIRMED |
| `batch_released` | `BatchReleased` | lô đã phát hành | CONFIRMED |
| `warehouse_receipt_confirmed` | `WarehouseReceiptConfirmed` | đã nhập kho | CONFIRMED |
| `hsd_valid` | `HsdValid` | HSD hợp lệ | CONFIRMED |
| `trace_ready` | `TraceReady` | trace đủ | CONFIRMED |
| `captured_at` | (ops sẽ bổ sung) | độ tươi snapshot | ⏳ GAP (DO-02 ops thêm) |

Endpoint (do Order Core gọi): `POST /api/v1/admin/availability/check` (perm `SellableCheck`). Read chi tiết: `GET /v1/sale-locks/{id}`, `GET /v1/recall-cases/{id}`, admin `GET /api/v1/admin/recall/cases/{id}/holds` (DO-07).

## 3. Blocker → xử lý IVR/Core
| Điều kiện | Xử lý |
| --- | --- |
| `decision ∈ {NOT_SELLABLE, BLOCKED}` hoặc `recall_hold`/`sale_lock`/`quality_hold` = true | pre-dispatch: `TASK_BLOCKED_OPERATIONAL`; callback: Core `BLOCKED_BY_CORE` |
| ops error/timeout/`/health/ready=503` | **fail-closed** (DO-06): không dispatch/không confirm |
| webhook `ops-core.sellable.sku-became-not-sellable.v1` | hold sớm (optional, dedupe `EventId`) — không thay revalidate (DO-04) |

## 4. Evidence/trace khi block (DO-07 / MASTER-03)
Khi block một cuộc xác nhận, ghi vào evidence: `sale_lock_id` (Guid) / `recall_case_id` (Guid + `recall_no`), `scope_type`+`scope_id`, `correlation_id`, `evidence_refs[]`/`audit_refs[]`, link `BATCH_TO_RECALL`.

## 5. Public trace / product (INBOUND — P2, chưa mở)
`GET /api/v1/public/trace/{qrCode}` (theo `qrCode`, không batch_code) — DO-08; chỉ dùng nếu mở inbound. Product/ingredient qua commerce/PACK-05 (DO-09), không trực tiếp ops.

## Báo cáo (ops)
- **10 field/line** mapped tới sellable gate; **1 GAP** (`captured_at` ops bổ sung). IVR không gọi ops trực tiếp (qua Order Core).

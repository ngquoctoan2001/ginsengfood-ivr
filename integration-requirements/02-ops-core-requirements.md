# IR-02 — Ops-Core Requirements (Module 1/2)

Trạng thái: `REQUIREMENTS` · Nguồn: DO-01..DO-09, DO-CORR-1/2/3; `data/03`, `api/06 §4`.
✅ Ops-core đã trả lời (owner cùng team). Đây là **việc ops-core cần build/điều chỉnh** để phục vụ IVR (do **Order Core** gọi, không phải IVR).

| ID | Yêu cầu | Prio | I/O | sync/async | idempotency | mock? | Ai build | Trạng thái |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IR-OPS-01 | **Sellable gate cho blocker**: `POST /api/v1/admin/availability/check` → `SellableStatus{Decision, RecallHold, SaleLock, QualityHold, StockAvailable, BatchReleased, TraceReady…}`, scope SKU(±batch). Quyết định: reuse hay bọc thêm **GET low-latency** cho Core | P0 | in `{skuId,batchId?}`; out SellableStatus | sync read | n/a | có | Ops | ✅ DO-01 (chọn reuse vs GET) |
| IR-OPS-02 | **Thêm `captured_at`** (+ optional `policy_version`/ETag) vào response sellable + lock để snapshot biết độ tươi | P0 | out: +captured_at | — | — | có | Ops | ✅ DO-02 (cần bổ sung field) |
| IR-OPS-03 | **Mở service-auth cho Order Core**: cấp perm `SellableCheck`/`RecallHoldView` cho service-cred của Order Core | P0 | — | — | — | — | Ops + Foundation | ✅ DO-03 |
| IR-OPS-04 | **Cam kết SLA + fail-closed**: p95 < 200ms (đề xuất); error codes ổn định (`SALE_LOCK_ACTIVE`/`RECALL_IMPACT_ACTIVE`/`SELLABLE_GATE_BLOCKED`/`INVENTORY_NOT_SELLABLE`/`QUALITY_HOLD`/`TRACE_GAP_DETECTED`/`RATE_LIMITED`/`INTERNAL_ERROR`); `/health/ready`(503) | P0 | out: codes + health | sync | — | có | Ops | ✅ DO-06 |
| IR-OPS-05 | **Webhook hold-sớm**: `ops-core.sellable.sku-became-not-sellable.v1` (at-least-once, `X-Idempotency-Key=EventId`); consumer dedupe | P1 | out: event | async | dedupe EventId | có | Ops | ✅ DO-04 (đã có) |
| IR-OPS-06 | **Read chi tiết lock/recall cho evidence**: `GET /v1/sale-locks/{id}`, `GET /v1/recall-cases/{id}` trả `evidence_refs[]`/`audit_refs[]`; ids Guid + `recall_no`; link `BATCH_TO_RECALL` | P1 | in: id; out: detail | sync read | n/a | có | Ops | ✅ DO-07 (đã có) |
| IR-OPS-07 | **Public trace (INBOUND — P2)**: `GET /api/v1/public/trace/{qrCode}` (theo `qrCode`, không batch_code); whitelist 12 field; recall qua `batch.releasePublicStatus` | P2 | in: qrCode; out: whitelist | sync | n/a | có | Ops | ✅ DO-08 (chỉ nếu mở inbound) |

## Ghi chú ranh giới (đính chính đã khóa)
- **Ops không biết `order_id`** (DO-CORR-1) → fan-out là việc **Order Core** (IR-SALES-05), không phải ops.
- **do-not-call/opt-out KHÔNG thuộc ops** (DO-CORR-2) → CRM/Customer Identity (**DC-01 resolved; IR-CRM-01 build P1**).
- **Sale-lock = recall-triggered** (DO-CORR-3) — chưa có sale-lock thương mại độc lập.
- Availability lot-level **không** mở cho IVR; đi qua Commerce/Core (DO-05).

## Việc ops-core cần chốt nội bộ
(1) reuse `availability/check` hay build GET blocker gọn (IR-OPS-01); (2) thêm `captured_at` (IR-OPS-02); (3) mở service-auth (IR-OPS-03); (4) chốt SLA + fail-closed (IR-OPS-04).

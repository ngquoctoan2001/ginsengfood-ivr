# Câu hỏi tích hợp IVR — gửi Team Ops-Core (Module 1 Product Master / Module 2 Operational Core)

> **LỊCH SỬ — vòng hỏi/đáp 2026-07-02.** Các câu trả lời dưới đây là bản ghi của vòng đó. Nơi nào mâu thuẫn với `plan/ivr-orther/target-contract-v1-draft.md` hoặc các quyết định `TV1-*` trong `decisions-log.md` thì **TV1-* thắng** (xem `decisions-log.md` dòng 3). Cụ thể đã bị supersede: kết luận “IVR chỉ COD”, D-10 đã khóa, callback Golden Hour là target cuối, taxonomy `CALLBACK_*`, và pilot mặc định 12 SIM. Không dùng file này làm authority cho implementation.


Người gửi: Team IVR / Module 8 (IVR Order Confirmation — phase-8 / PACK-09)
Ngày gửi: 2026-07-02
Trạng thái: ✅ **ĐÃ TRẢ LỜI (2026-07-02)** — QO1–QO9 đã trả lời (owner cùng team).

> **⚠️ 3 đính chính nền tảng (đọc trước):**
> 1. Ops-core **KHÔNG biết `order_id`** → Order Core **fan-out** order → dòng SKU/batch rồi hỏi ops.
> 2. **"Suppression/do-not-call/opt-out" KHÔNG thuộc ops-core** → thuộc **CRM/business-platform** (ops "suppression" chỉ là procurement/MRP FRM-05).
> 3. **Sale Lock ops-core = recall-triggered** (FK `recall_case_id` bắt buộc); chưa có sale-lock thương mại độc lập.
>
> **Tóm tắt (chi tiết + bản khóa: [decisions-log.md](decisions-log.md) DO-01..DO-09):**
> - **QO1→DO-01 (Điều chỉnh):** không có blocker-status theo order_id; primitive = **sellable gate** `POST /api/v1/admin/availability/check` → `SellableStatus{Decision, RecallHold, SaleLock, BatchReleased, StockAvailable, TraceReady, QualityHold…}`; scope SKU(±batch); SLA đề xuất p95<200ms.
> - **QO2→DO-02 (Điều chỉnh):** snapshot = mảng SellableStatus **per-line**, Order Core fan-out & nhúng; ops thêm `captured_at`/ETag.
> - **QO3→DO-03 (Xác nhận):** Order Core là caller (service-cred `SellableCheck`/`RecallHoldView`); IVR không gọi ops trực tiếp.
> - **QO4→DO-04 (Có event):** webhook outbox `ops-core.sellable.sku-became-not-sellable.v1` (dedupe EventId); revalidate-at-callback vẫn là chính.
> - **QO5→DO-05 (Xác nhận):** availability qua Commerce/Core; ops không mở cho IVR.
> - **QO6→DO-06 (Xác nhận):** fail-closed; `/health/ready`(503) + error codes ổn định.
> - **QO7→DO-07 (Xác nhận):** `sale_lock_id`/`recall_case_id`=Guid (+`recall_no`), `evidence_refs`/`audit_refs`.
> - **QO8→DO-08 (Điều chỉnh):** public trace theo **`qrCode`** (không batch_code); `traceStatus∈{VALID,NOT_PUBLIC,INVALID_QR}`, recall qua `batch.releasePublicStatus`.
> - **QO9→DO-09 (Xác nhận):** không public catalog/ingredient; lấy qua Commerce/PACK-05.
>
> Các ô trả lời chi tiết bên dưới giữ nguyên làm biên bản gốc.

> **Lưu ý sở hữu:** Module 1/2 và Module 8 (IVR) do **cùng một owner** phụ trách. Vì vậy phần lớn câu dưới đây là **quyết định nội bộ ops-core** (không phải xin bên ngoài) — nhưng vẫn cần chốt rõ contract để IVR thiết kế đúng và để đội hiện thực ops-core biết cần expose gì.

## 0. Bối cảnh

- IVR là hợp phần gọi **outbound xác nhận Official Order**; **input signal only** (Order Core quyết trạng thái).
- Nhu cầu IVR đối với ops-core rất hẹp: chủ yếu là **trạng thái blocker** (Sale Lock / Recall / Suppression) để KHÔNG xác nhận đơn hàng bị khóa/thu hồi.
- Nguyên tắc kiến trúc đề xuất: **IVR không gọi ops-core trực tiếp.** IVR nhận **snapshot blocker trong task** (do Order Core nhúng) để pre-check; khi callback, **Order Core** gọi ops-core realtime để revalidate. Đường IVR→ops chỉ mở nếu thực sự cần realtime mà Core không đảm bảo.
- **Cách trả lời:** mỗi câu có *"Đề xuất từ IVR"* — chọn **[ ] Xác nhận** / **[ ] Điều chỉnh**, điền ô **Trả lời**, kèm endpoint/contract nếu có.
- Nguồn tham chiếu: `docs/documents/4. phase/phase-2/06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md`, `3. tech/04-TECH-03`, `phase-8/02,04,07`.

Ưu tiên: **P0** chặn thiết kế · **P1** cần sớm · **P2** chỉ khi mở tính năng inbound.

---

# PHẦN 1 — Module 2 (Operational Core: inventory / traceability / recall / sale-lock)

### QO1 (P0) — API trạng thái blocker (Sale Lock / Recall / Suppression)
IVR/Order Core cần đọc trạng thái khóa để chặn xác nhận đơn.

**Đề xuất từ IVR:** ops-core expose `GET /v1/operational/blocker-status?order_id=…` (hoặc theo `sku`/`batch`) trả về: `sale_lock{active,scope,reason,effective_at}`, `recall{active,scope,reason}`, `suppression{active}`. Đây là **read-only, low-latency**.
Xin cho biết: (a) có endpoint này chưa / bao giờ có; (b) tra theo order, sku hay batch; (c) **SLA latency** mục tiêu (VD < 200ms).

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời (endpoint + scope + SLA):** ______________________________________________
- Người trả lời / ngày: __________

### QO2 (P0) — Snapshot blocker để nhúng vào IVR task
IVR nhận `sale_lock_snapshot` / `recall_snapshot` / `suppression_snapshot` trong task để **pre-dispatch block**.

**Đề xuất từ IVR:** ops-core cấp snapshot (hoặc ref) để Order Core lấy tại thời điểm tạo task; snapshot có `captured_at` để biết độ tươi; scope theo order (gồm mọi sku/batch của đơn).
Xin cho biết: cách lấy snapshot, có `captured_at`/version không, scope tính theo order hay từng dòng hàng.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QO3 (P0) — Revalidate realtime lúc callback + ai là người gọi
Khi IVR callback, cần kiểm blocker **realtime** trước khi Core transition (phòng trường hợp phím `1` nhưng Sale Lock vừa bật).

**Đề xuất từ IVR:** **Order Core** (không phải IVR) gọi ops-core realtime lúc revalidate. Ops-core cam kết endpoint QO1 sẵn sàng cho lần gọi này.
Xin xác nhận: (a) đồng ý caller là Order Core; (b) ops-core chịu tải thêm cho bước revalidate này. *(Liên quan Q6 trong `questions-to-module-3-and-3.1.md`.)*

- [ ] Xác nhận (Core gọi) · [ ] Điều chỉnh (muốn IVR gọi trực tiếp?)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QO4 (P1) — Event push khi Sale Lock/Recall kích hoạt giữa cuộc gọi
Nếu recall/lock bật ngay khi cuộc gọi đang diễn ra, có cách nào để hold kịp?

**Đề xuất từ IVR:** ưu tiên **dựa vào Core revalidate lúc callback** là đủ an toàn (không bắt buộc realtime push). Nhưng nếu ops-core đã có event `SaleLockActivated` / `RecallLockActivated` thì IVR/Core có thể subscribe để hold sớm.
Xin cho biết: có event push không? tên event/kênh?

- [ ] Không có event (dựa callback revalidate — chấp nhận) · [ ] Có event (xin nêu tên/kênh)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QO5 (P1) — Nguồn "availability/stock" khi revalidate
Khi xác nhận đơn có cần kiểm còn hàng không, và lấy từ đâu?

**Đề xuất từ IVR:** availability **đi qua Commerce/Sellable Gate** (aggregate), IVR/ops không cần cấp lot-level cho IVR. Ops-core chỉ giữ vai trò nguồn tồn kho cho commerce như hiện tại.
Xin xác nhận: availability cho bước xác nhận đơn thuộc commerce, ops-core **không** phải expose riêng cho IVR.

- [ ] Xác nhận (qua commerce) · [ ] Điều chỉnh (ops-core cấp trực tiếp)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QO6 (P1) — Fail-safe khi ops-core / blocker check không khả dụng
Quy tắc IVR: nếu không kiểm được blocker → **không dispatch cuộc gọi** (fail-closed).

**Đề xuất từ IVR:** ops-core cung cấp **health/status** rõ ràng (hoặc mã lỗi chuẩn) để Core/IVR biết blocker check đang down → không dispatch / Core block.
Xin cho biết: có health endpoint / mã lỗi chuẩn khi service down không; hành vi mong muốn khi timeout.

- [ ] Xác nhận fail-closed + có health signal · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QO7 (P1) — ID & trace của Sale Lock / Recall (đối chiếu MASTER-03)
Để evidence liên kết được IVR result với blocker, cần ID/trace chuẩn.

**Đề xuất từ IVR:** blocker trả kèm `recall_id` / `sale_lock_id` + scope để IVR/Core ghi vào evidence khi block một cuộc xác nhận (theo `MASTER-03` traceability).
Xin xác nhận định dạng ID và có thể tham chiếu trong evidence không.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

---

# PHẦN 2 — Module 1/2 (CHỈ nếu mở tính năng inbound — P2)

### QO8 (P2) — Public trace lookup (batch VALID / RECALLED)
Nếu sau này mở inbound "khách gọi hỏi lô sản phẩm có an toàn/bị thu hồi không", IVR cần tra public trace.

**Đề xuất từ IVR:** dùng `GET /v1/trace/public?batch_code=…` trả whitelist: product_name (public), batch code (masked), MFG/EXP (nếu duyệt), status (VALID/ON_HOLD/RECALLED/EXPIRED/NOT_FOUND), public recall message, contact. KHÔNG lộ supplier/raw lot/cost/QC nội bộ.
Xin cho biết: có sẵn public trace projection không; whitelist trường cho IVR.

- [ ] Chưa cần (giữ outbound-only) · [ ] Sẽ cần (xin nêu whitelist)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QO9 (P2) — Product master / tên public / thành phần cho call script
Call script hiện chỉ cần `order_code_short` + `total_amount_display`. Nếu tương lai cần đọc tên/thành phần sản phẩm.

**Đề xuất từ IVR:** lấy **qua Commerce catalog hoặc PACK-05 Product Knowledge**, KHÔNG gọi ops-core trực tiếp (ops chỉ có internal BOM, chưa có "public ingredient list").
Xin xác nhận ranh giới này.

- [ ] Xác nhận (không lấy trực tiếp ops) · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

---

## Tổng hợp mức ưu tiên

| Câu | Chủ đề | Module | Ưu tiên |
| --- | --- | --- | --- |
| QO1 | API blocker status (sale-lock/recall/suppression) | 2 | P0 |
| QO2 | Snapshot blocker nhúng vào task | 2 | P0 |
| QO3 | Revalidate realtime + ai gọi | 2 (+Core) | P0 |
| QO4 | Event push khi lock/recall activate | 2 | P1 |
| QO5 | Nguồn availability | 2 (qua commerce) | P1 |
| QO6 | Fail-safe khi ops down | 2 | P1 |
| QO7 | ID/trace blocker cho evidence | 2 (MASTER-03) | P1 |
| QO8 | Public trace (inbound) | 2 | P2 |
| QO9 | Product master/ingredient (inbound) | 1 | P2 |

**Chặn thiết kế IVR (ưu tiên nhất):** QO1, QO2, QO3.

**Điểm mấu chốt:** Nhu cầu ops-core cho IVR gần như chỉ gói gọn ở **blocker status (QO1–QO3)**. Mọi thứ khác (availability, product, public trace) nên đi **gián tiếp qua commerce**, hoặc chỉ cần khi mở inbound.

---

## Ô tổng kết cho người duyệt (Ops-Core)
- Người duyệt Module 2: ____________ · Ngày: ______
- Người duyệt Module 1: ____________ · Ngày: ______
- Ghi chú / quyết định bổ sung: ______________________________________________

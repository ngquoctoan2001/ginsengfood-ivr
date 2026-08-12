# 02 — Current Understanding

Tóm tắt hiểu biết sau khi đọc docs. Mỗi mục có nhãn căn cứ.

## 1. GinsengFood chia module/hệ thống thế nào?

- CONFIRMED: Hệ sinh thái chia thành 2 hệ thống cha + các pack/phase:
  - **ginsengfood-ops-core**: vận hành sản xuất/kho/truy xuất/thu hồi (PACK-01/02/03 ↔ phase-1, phase-2).
  - **ginsengfood-business-platform**: commerce runtime, AI advisor, Facebook, Ads, MC-AI-Live, IVR (PACK-04..09 ↔ phase-3..8).
  - Nguồn: `1. master/01-MASTER-00-INDEX-REGISTRY.md`, `3. tech/01-TECH-00-...MASTER-PLAN.md`.
- CONFIRMED: Governance qua 10 MASTER docs, 10 PACK, 16 business domain P0, canonical runtime docs. Mọi module mới phải tuân: source-of-truth (MASTER-01), dependency (MASTER-02), traceability-id (MASTER-03), resolver/guard (MASTER-04), evidence/smoke/gate (MASTER-05), foundation RBAC/audit/idempotency/evidence (TECH-01).
- ASSUMPTION: Ánh xạ "module 1/2/3/3.1/8" của brief tương ứng: module 1 ≈ phase-1 (product master), module 2 ≈ phase-2 (operational core), module 3 ≈ phase-3 (commerce runtime), module 3.1 ≈ phase-3.1 (sales extensions), module 8 ≈ phase-8 (IVR). `NEED_CONFIRMATION`: ánh xạ số hiệu module ↔ phase là suy luận từ ngữ cảnh, docs không đánh số "module 1..8" tường minh (dùng PACK/phase).

## 2. Module 1 (Ops-Core / Product Master) làm gì?

- CONFIRMED: Sở hữu **Product master, SKU canonical, ingredient/material, UOM/conversion, Recipe/BOM/Formula version (G0/G1), packaging profile, Product Activation Guard**. Nguyên tắc lõi: "**Product Active ≠ Sellable**". Nguồn: `phase-1/02,03,04`, `3. tech/03-TECH-02`.

## 3. Module 2 (Ops-Core / Operational Core) làm gì?

- CONFIRMED: Sở hữu **raw lot/QC/readiness, production order + BOM snapshot, material issue, batch lifecycle/QC/release, warehouse receipt, finished goods inventory, internal trace + public QR trace, recall (thu hồi), sale-lock (khóa bán)**. Nguyên tắc: "Release ≠ Sellable", "Sale lock thắng downstream". Nguồn: `phase-2/00,05,06`, `3. tech/04-TECH-03`.

## 4. Module 3 (Sales / Commerce Runtime) làm gì?

- CONFIRMED: Sở hữu **sellable gate, QuoteSnapshot (giá cuối bất biến), cart, order draft, customer confirmation, Official Order + order_code, order state machine, payment status, shipping/delivery, invoice/VAT, verified revenue**. Nguyên tắc: "No CustomerConfirmation → no Official Order → no order_code". Nguồn: `phase-3/05`, `3. tech/05-TECH-04`.
- CONFIRMED: Order state (tham khảo): `DRAFT → (CONFIRMATION_REQUIRED) → OFFICIAL/CONFIRMED → PAYMENT_PENDING → PAID → VERIFIED → SHIPPED → DELIVERED → COMPLETED`. Payment: `AWAITING → RECONCILING → PAID_*`. Shipping: `READY_TO_SHIP → PICKED → SHIPPED → IN_TRANSIT → DELIVERED`. Nguồn: `1. master/02-MASTER-01` §TYPE-04, phase-3/05, phase-3.1/07. `NEED_CONFIRMATION`: tên chính xác các state IVR-callable phải do Order Core chốt.

## 5. Module 3.1 (Sales Extensions) làm gì?

- CONFIRMED: Membership 12 tháng (tier + grace), pricing programs (24/7, Golden Hour), Diamond referral commission, CRM messaging & lifecycle automation (D0–M12), AI advisor situation mapping, **IVR high-risk detection (`IVRRequiredDecision`)**, payment reconciliation (bank transfer reference), shipping ETA. Nguồn: `phase-3.1/00,03,05,07`.
- CONFIRMED: phase-3.1/07 là **connector IVR ở phía sales**: quy định khi nào IVR *required* (khách mới, `verified_order_count=0`, nghi trùng, COD rủi ro, giá trị bất thường, hành vi Golden Hour), quy tắc "**không sinh order_code trước khi IVR pass nếu IVR required**", "không release fulfillment trước IVR confirmed", "Golden Hour phải release quota nếu IVR fail/timeout". Nguồn: `phase-3.1/07`.

## 6. IVR / Module 8 dự kiến nằm ở đâu?

- CONFIRMED: IVR thuộc **ginsengfood-business-platform** (PACK-09/phase-8). Là hợp phần xác nhận đơn, **consumer của Order Core**, không phải owner order state. Nguồn: `MASTER-00 §5.9`, `MASTER-01 SRC-IVR-001`, phase-8/00, phase-8/02.

## 7. IVR liên kết nhiều nhất với module nào?

- CONFIRMED: **Module 3/3.1 (Commerce Order Core + sales extensions)** — nhiều nhất. IVR nhận `IvrConfirmationTaskV1` từ Order Core, gửi `IvrConfirmationResultCallbackV1` về Order Core. Nguồn: phase-8/02, /04, /07.

## 8. IVR có liên hệ gì với ops-core?

- CONFIRMED: IVR **consume blocker** từ Operational Core: **Sale Lock, Recall, Suppression, availability** — kiểm trước dispatch và khi Core revalidate callback. IVR **không** override blocker. Nguồn: phase-8/00 §5, /02 §3, /04 (blocker snapshots).
- ASSUMPTION: IVR có thể lấy các snapshot blocker này **gián tiếp qua Order Core** (đưa vào task payload) thay vì gọi trực tiếp ops-core. Nguồn: phase-8/04 (`sale_lock_snapshot`, `recall_snapshot` nằm trong task). `NEED_CONFIRMATION`: chiều gọi (IVR→ops trực tiếp vs qua Order Core).

## 9. Dữ liệu nào thuộc sales platform?

- CONFIRMED: customer/member profile & tier, order & order_code & order state, QuoteSnapshot & giá, payment status & reference, shipping status & ETA, invoice/VAT, verified revenue, Diamond commission, CRM message, `IVRRequiredDecision`. Nguồn: report sales §C, phase-3/05, phase-3.1/07, TECH-04.

## 10. Dữ liệu nào thuộc ops-core?

- CONFIRMED: product/SKU master, material/UOM, recipe/BOM, activation state, raw lot/QC, batch, finished goods inventory, **sale-lock/recall state**, public trace (batch code, MFG/EXP, VALID/RECALLED). Nguồn: report ops §C/D/F, phase-2/05,06, TECH-02/03.

## 11. Dữ liệu nào nên thuộc riêng IVR?

- CONFIRMED (theo phase-8/12): `ivr_confirmation_tasks`, `ivr_call_jobs`, `ivr_call_attempts`, `ivr_call_results`, `ivr_result_callbacks`, `ivr_sim_channels`, `ivr_capacity_incidents`, `ivr_technical_exceptions`, `ivr_admin_actions`, `ivr_evidence_links`. IVR chỉ lưu **snapshot/version/ref** của order/phone/blocker, KHÔNG là source-of-truth. Nguồn: phase-8/12 §2.

## 12. Phát hiện đáng chú ý (tension & lệch scope)

- ✅ **RESOLVED (D-01, Module 3 xác nhận 2026-07-02)** — trước đây là tension: **Thứ tự IVR ↔ order_code**:
  - phase-3.1/07: IVR *required* xảy ra **trước** khi sinh `order_code` (gate chống đơn ảo trước khi order chính thức).
  - phase-8/00: IVR "chỉ vận hành **sau** khi có Official Order đủ điều kiện từ Order Core".
  → ✅ **Chốt (D-01):** `order_code` cấp **khi tạo Official Order**; đơn vào `CONFIRMATION_REQUIRED/IVR_PENDING`; **fulfillment/downstream khóa** tới khi Core nhận & chấp nhận IVR signal. Câu "không order_code trước IVR" của phase-3.1 = "không release/verify downstream trước IVR". Tức là **phương án (a)** — IVR chạy trên official order chờ xác nhận, order_code đã có. Xem [decisions-log.md](decisions-log.md) D-01.
- ✅ **RESOLVED (D-08, Module 3):** Giữ **outbound confirmation only**; nhóm inbound (tra cứu đơn theo số, đặt hàng qua điện thoại, gặp nhân viên, tư vấn) = **future scope**, chưa làm. Xem [decisions-log.md](decisions-log.md) D-08.
- ✅ **RESOLVED (D-10):** Attempt policy = rule mới: 2 cuộc cả hai program; GH 5′ (T0/T0+2:30), 24/7 15′ (T0/T0+7:30); `T0` = lúc Core mở window/tạo task. (Thay rule cũ 2/10 & 3/15.)
- CONFIRMED: Mô hình triển khai chốt là **Internal SIM Gateway Server**, **1 SIM = 1 cuộc active**; cloud IVR/SIP/brandname là future owner decision. Nguồn: phase-8/00 §6,§11; PACK-09.

## 13. Naming

- CONFIRMED: Tên nghiệp vụ chuẩn = **"IVR Order Confirmation"** (PACK-09/TECH-09/phase-8). Slug kỹ thuật xuất hiện trong docs: `ivr-order-confirmation` (API `/v1/ivr/order-confirmation/*`, DB `ivr_*`). Còn gọi "Module 8".
- `NEED_CONFIRMATION`: Working name `ivr-orther` chỉ là tên tạm; nên dùng `ivr-order-confirmation` khi sinh specs. Tên `ivr-order` cũng hợp lý nhưng docs dùng đầy đủ "order-confirmation".

## 14. Trạng thái phân tích

- `TODO`: Chưa parse `MODULE_8_...V0.2_CLEAN_FINAL.docx` để đối chiếu version.
- `TODO`: Chưa đọc chi tiết phase-4..7 (kênh AI/Facebook/Ads/Live) — rel thấp, đọc khi cần bối cảnh.
- `TODO`: Chưa xác nhận ánh xạ số module ↔ phase với owner.

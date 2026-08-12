# Câu hỏi tích hợp IVR — gửi Team Module 3 (Commerce Order Core) & Module 3.1 (Sales Extensions)

Người gửi: Team IVR / Module 8 (IVR Order Confirmation — phase-8 / PACK-09)
Ngày gửi: 2026-07-02
Trạng thái: ✅ **ĐÃ TRẢ LỜI (2026-07-02)** — toàn bộ Q1–Q14 đã được Module 3 & 3.1 trả lời.

> **Tóm tắt câu trả lời (chi tiết + bản khóa: [decisions-log.md](decisions-log.md) D-01..D-14):**
> - **Q1→D-01:** order_code cấp khi tạo Official Order; đơn `CONFIRMATION_REQUIRED/IVR_PENDING`; fulfillment khóa tới khi Core chấp nhận IVR signal. "Không order_code trước IVR" = không release/verify downstream.
> - **Q2→D-02:** Core trả `order_state`/`order_version`/`is_ivr_callable`; transition do Core (confirmed/cancelled/no-answer/expired/technical).
> - **Q3→D-03:** push sync `POST .../tasks` + Idempotency-Key + Correlation-Id; Core retry bounded.
> - **Q4→D-04:** revalidate P0 đồng bộ (idempotency/version/state/blocker/evidence), response 3–5s; transition async; ACCEPTED ≠ confirmed.
> - **Q5→D-05:** OfficialContactResolver cấp phone_ref/masked/validation/dial_token; token vault ở SIM adapter, TTL ≤ window, one-use/attempt.
> - **Q6→D-06:** Core revalidate blocker realtime; Sale Lock/Recall mới → block/hold dù bấm 1.
> - **Q7→D-07:** availability qua Commerce/Sellable Gate; IVR không gọi ops lot-level.
> - **Q8→D-08:** giữ **outbound-only**; inbound = future scope.
> - **Q9→D-09:** IVRRequiredDecision chỉ set cờ; Core tạo task; event `order.ivr_required_decisioned`.
> - **Q10→D-10:** attempt rule **mới**: max 2 cả hai; GH 5′ (T0, T0+2:30); 24/7 15′ (T0, T0+7:30); **T0 = lúc Core mở window/tạo task**.
> - **Q11→D-11:** IVR chỉ signal; QuotaReleaseGuard (Sales) release quota sau khi Core accept fail/expired.
> - **Q12→D-12:** không hardcode ngưỡng; skip chỉ khi TRUSTED+allowed+contact ổn+không blocker/risk; danh sách risk-flag buộc gọi.
> - **Q13→D-13:** danh sách IVR-required xác nhận; ngưỡng thuộc Risk Policy; IVR chỉ consume risk_flags boolean.
> - **Q14→D-14:** IVR chỉ audit nội bộ, không ghi CRM; CRM nhận event sau Core decision.
>
> Các ô trả lời chi tiết bên dưới giữ nguyên làm biên bản gốc.

## 0. Bối cảnh (đọc trước khi trả lời)

- IVR Order Confirmation là hợp phần **gọi tự động OUTBOUND xác nhận Official Order** (chống đơn ảo), qua Internal SIM Gateway.
- Nguyên tắc khóa: **IVR result chỉ là tín hiệu (input signal); Order Core mới quyết định trạng thái đơn.** IVR KHÔNG tạo Quote/Cart/Order, KHÔNG tự hủy/confirm, KHÔNG chạm payment/kho, KHÔNG tự gửi SMS/CRM.
- Chúng tôi cần chốt **hợp đồng tích hợp** với hệ bán hàng trước khi viết specs chi tiết & code.
- **Cách trả lời:** mỗi câu có sẵn *"Đề xuất từ IVR"*. Vui lòng chọn **[ ] Xác nhận** / **[ ] Điều chỉnh** và điền vào ô **Trả lời**. Nếu có API/doc tương ứng, xin dẫn đường dẫn/endpoint.
- Nguồn tham chiếu phía chúng tôi: `docs/documents/4. phase/phase-8/*`, `docs/MODULE_8_..._V0.2_CLEAN_FINAL.docx`; nguồn phía các anh/chị: `phase-3/05`, `phase-3.1/07`, `3. tech/05-TECH-04`.

Ưu tiên: **P0** = chặn thiết kế · **P1** = cần sớm · **P2** = chỉ khi mở tính năng inbound.

---

# PHẦN 1 — MODULE 3 (Commerce Order Core / Commerce Runtime)

### Q1 (P0) — Thứ tự giữa IVR và `order_code`
Có mâu thuẫn giữa 2 tài liệu: `phase-3.1/07` ghi *"không sinh official order_code trước khi IVR pass"*, còn `phase-8` ghi *"IVR chỉ chạy sau khi đã có Official Order + order_code"*.

**Đề xuất từ IVR:** `order_code` được cấp ngay khi tạo Official Order, nhưng đơn ở sub-state `CONFIRMATION_REQUIRED`; **fulfillment/downstream bị khóa cho tới khi IVR xác nhận**. Tức "chưa release trước IVR" ✔, nhưng order_code vẫn có sẵn để IVR đọc script. Phần "IVR-required" ở `phase-3.1/07` được hiểu là **quyết định rủi ro ở giai đoạn draft**, không phải cuộc gọi xác nhận.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### Q2 (P0) — Order State Machine
IVR cần biết: (a) danh sách trạng thái đơn "được phép gọi IVR" (IVR-callable), (b) sau mỗi kết quả IVR thì Core chuyển sang state nào.

**Đề xuất từ IVR:** Order Core cấp cho IVR một `order_state` (enum "đục" — IVR không hardcode tên) + `order_version`, cùng cờ `is_ivr_callable`. IVR không suy diễn transition; Core tự quyết. Mong nhận **bảng state + transition** cho các kết quả: `IVR_CONFIRMED`, `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_FINAL`, `IVR_CONFIRMATION_WINDOW_EXPIRED`, `IVR_TECHNICAL_EXCEPTION`.

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời (đính kèm bảng state nếu có):** ______________________________________________
- Người trả lời / ngày: __________

### Q3 (P0) — Cách gửi task & transport
Order Core gửi task xác nhận sang IVR bằng cơ chế nào?

**Đề xuất từ IVR:** Order Core **PUSH** task bằng gọi `POST /v1/ivr/order-confirmation/tasks` (IVR expose), đồng bộ (sync command), bắt buộc `Idempotency-Key` + `X-Correlation-Id`; Order Core giữ retry kỹ thuật có giới hạn.

- [ ] Xác nhận · [ ] Điều chỉnh (nếu muốn IVR poll / dùng message bus, xin nêu rõ)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### Q4 (P0) — Callback & revalidation
IVR gửi kết quả về Core bằng callback; Core revalidate trước khi transition.

**Đề xuất từ IVR:** IVR gọi `POST /v1/orders/{order_id}/ivr-result-callbacks` với `order_version_seen_by_ivr` (race guard), `result_type`, `evidence_ref`, `idempotency_key`. Core trả một trong: `CALLBACK_ACCEPTED_FOR_REVALIDATION` / `REJECTED_STALE` / `BLOCKED_BY_CORE` / `NEEDS_ADMIN_REVIEW` / `TECHNICAL_RETRY_ALLOWED|BLOCKED`.
Câu hỏi: Core revalidate **đồng bộ trong response** hay **async**? Timeout & chính sách retry phía Core?

- [ ] Xác nhận response codes · [ ] Điều chỉnh
- **Trả lời (đồng bộ/async + timeout):** ______________________________________________
- Người trả lời / ngày: __________

### Q5 (P0) — Official contact & dial token (privacy-safe)
IVR không được đọc số điện thoại thô/full profile.

**Đề xuất từ IVR:** trong task luôn kèm `phone_ref` + `phone_masked` + `phone_validation_status`; và một `dial_token` **TTL ngắn, dùng một lần**, chỉ map token→số thật bên trong ranh giới SIM adapter. Xin cho biết: dịch vụ nào cấp token này, TTL bao lâu, ai giữ mapping?

- [ ] Xác nhận cơ chế token · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### Q6 (P0) — Ai revalidate blocker (Sale Lock/Recall/Suppression) lúc callback?
IVR nhận snapshot blocker trong task để pre-check trước khi gọi. Nhưng lúc callback cần kiểm **realtime**.

**Đề xuất từ IVR:** **Order Core** là bên gọi Operational Core realtime để revalidate blocker khi nhận callback (IVR KHÔNG gọi ops trực tiếp). Nếu phím `1` nhưng Sale Lock/Recall xuất hiện → Core **block/hold**, không confirm.

- [ ] Xác nhận (Core revalidate) · [ ] Điều chỉnh (muốn IVR tự gọi ops? xin nêu)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### Q7 (P1) — Nguồn "availability/stock" khi revalidate
Khi xác nhận đơn, ai cấp trạng thái còn/hết hàng để Core quyết?

**Đề xuất từ IVR:** IVR không tự kiểm tồn kho; availability do **Commerce/Sellable Gate** tổng hợp (không phải IVR gọi Operational Core lot-level).

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### Q8 (P2 — chỉ nếu mở inbound) — Đọc payment/shipping status cho khách
Hiện IVR là **outbound-only**, không tra cứu. Nếu sau này mở tính năng khách gọi vào hỏi trạng thái đơn thì cần API đọc `order status / payment status / shipping ETA` (đã mask).

- [ ] Chưa cần (giữ outbound-only) · [ ] Sẽ cần (xin nêu API dự kiến & trường được lộ)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

---

# PHẦN 2 — MODULE 3.1 (Sales Extensions: Member / Program / Golden Hour / Diamond / CRM / Risk)

### Q9 (P0) — `IVRRequiredDecision`
`phase-3.1/07` định nghĩa quyết định "đơn này có cần IVR không". Nó được cấp cho IVR bằng cách nào và có tự tạo task không?

**Đề xuất từ IVR:** `IVRRequiredDecision` chỉ **set cờ + risk_reasons** trên order; **Order Core** mới là bên tạo IVR task (IVR không nhận trực tiếp từ 3.1). Xin cho biết: cung cấp qua API `GET /orders/{id}/ivr-required` hay event?

- [ ] Xác nhận (chỉ set cờ, Core tạo task) · [ ] Điều chỉnh
- **Trả lời (API hay event):** ______________________________________________
- Người trả lời / ngày: __________

### Q10 (P0) — Attempt policy & khớp chương trình Giờ Vàng
Bản Module 8 V0.2 (mới nhất) chọn rule **PACK-09 IVR Input Baseline V1.0**: **2 cuộc cho cả hai chương trình** — Giờ Vàng window **5 phút** (cách 2:30), 24/7 window **15 phút** (cách 7:30). (Bản SRS cũ ghi Giờ Vàng 2 cuộc/10′ và 24/7 **3 cuộc**/15′.) Chúng tôi thấy `phase-3.1/5. bo sung/03` có `IVR_confirmation_extra_time = 5 phút` — khớp window Giờ Vàng 5 phút.

**Đề xuất từ IVR:** dùng rule mới (2 cuộc cả hai; Giờ Vàng 5′; 24/7 15′). Xin xác nhận: (a) window xác nhận Giờ Vàng đúng là **5 phút** so với thời điểm nào (T0 = lúc đặt đơn?); (b) 24/7 chốt **2 cuộc** (không phải 3).

- [ ] Xác nhận rule mới · [ ] Muốn giữ rule cũ (24/7 = 3 cuộc) → cần mở Owner Decision
- **Trả lời (T0 tính từ đâu, số cuộc, window):** ______________________________________________
- Người trả lời / ngày: __________

### Q11 (P1) — Golden Hour quota release khi IVR fail/timeout
`phase-3.1/07` nói phải release quota Giờ Vàng nếu IVR fail/timeout theo policy.

**Đề xuất từ IVR:** IVR chỉ **gửi signal** (qua callback result: `IVR_NO_ANSWER_FINAL` / `WINDOW_EXPIRED`); phía Sales/Program tự thực thi release quota (IVR không tự release). Xin xác nhận cơ chế: Sales lắng nghe event/callback hay cần IVR gọi API riêng?

- [ ] Xác nhận (IVR chỉ signal) · [ ] Điều chỉnh (cần API riêng → xin nêu)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### Q12 (P1) — Customer trust / risk (skip IVR)
Khách "trusted" có được **skip IVR** không, và ngưỡng nào?

**Đề xuất từ IVR:** quyết định trusted/skip đến từ **Customer Trust Resolver** (không hardcode ở IVR); task kèm `customer_trust_status` + `trusted_skip_allowed`. Xin cho biết: ngưỡng trust để skip, và **danh sách risk_flags buộc phải IVR dù trusted** (VD COD fail nhiều, giá trị bất thường...).

- [ ] Cung cấp ngưỡng + danh sách risk_flags · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### Q13 (P1) — Danh sách điều kiện "IVR required"
`phase-3.1/07` liệt kê: khách mới, `verified_order_count=0`, nghi trùng, COD rủi ro, địa chỉ rủi ro, phone pattern nghi ngờ, giá trị bất thường, hành vi Giờ Vàng.

**Đề xuất từ IVR:** dùng đúng danh sách trên. Xin xác nhận danh sách cuối cùng + ngưỡng cụ thể (VD "giá trị bất thường" = bao nhiêu?), để scheduler ưu tiên đúng.

- [ ] Xác nhận danh sách · [ ] Bổ sung/điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### Q14 (P1) — Ghi call-note vào hồ sơ khách / CRM
Sau cuộc gọi, IVR có được ghi kết quả (outcome) vào hồ sơ khách/CRM không?

**Đề xuất từ IVR:** IVR **chỉ ghi audit/evidence nội bộ** (theo nguyên tắc "IVR không CRM đại trà"); KHÔNG ghi CRM note. Nếu Sales/CRM muốn nhận outcome, IVR sẽ **phát event** để CRM tự xử lý. Xin xác nhận ranh giới này.

- [ ] Xác nhận (audit nội bộ, không ghi CRM) · [ ] Muốn IVR ghi note (xin nêu API + trường)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

---

## Tổng hợp mức ưu tiên

| Câu | Chủ đề | Module | Ưu tiên |
| --- | --- | --- | --- |
| Q1 | Thứ tự IVR ↔ order_code | 3 | P0 |
| Q2 | Order state machine | 3 | P0 |
| Q3 | Transport gửi task | 3 | P0 |
| Q4 | Callback & revalidation | 3 | P0 |
| Q5 | Official contact / dial token | 3 (Customer/Commerce) | P0 |
| Q6 | Ai revalidate blocker | 3 (+Ops) | P0 |
| Q7 | Nguồn availability | 3 | P1 |
| Q8 | Đọc payment/shipping (inbound) | 3 | P2 |
| Q9 | IVRRequiredDecision | 3.1 | P0 |
| Q10 | Attempt policy & Giờ Vàng | 3.1 | P0 |
| Q11 | Quota release | 3.1 | P1 |
| Q12 | Trust/skip + risk_flags | 3.1 | P1 |
| Q13 | Danh sách IVR-required | 3.1 | P1 |
| Q14 | Call-note/CRM | 3.1 | P1 |

**Chặn thiết kế IVR (cần trả lời sớm nhất):** Q1, Q2, Q6 (Module 3) và Q9, Q10 (Module 3.1).

---

## Ô tổng kết cho người duyệt (bên Module 3/3.1)
- Người duyệt Module 3: ____________ · Ngày: ______
- Người duyệt Module 3.1: ____________ · Ngày: ______
- Ghi chú chung / quyết định bổ sung: ______________________________________________

# Câu hỏi bổ sung IVR — gửi CRM / Module 3.1 (vòng 2: do-not-call / opt-out / thông báo sau gọi)

Người gửi: Team IVR / Module 8 (IVR Order Confirmation — phase-8 / PACK-09)
Ngày gửi: 2026-07-02
Trạng thái: ✅ **ĐÃ TRẢ LỜI (2026-07-02, đọc source `ginsengfood-business-platform`)** — QC1–QC6 → **DC-01..06** ([decisions-log.md](decisions-log.md)).

> **Tóm tắt:**
> - **QC1→DC-01:** do-not-call ở **Customer Identity** (`consent_suppression_markers` theo `channel_type+contact_hash`); endpoint `POST /api/v1/admin/customer-identity/crm-ads-eligibility` (`channelType=PHONE_CALL`, `category=TRANSACTIONAL|SERVICE`). ⚠️ response hiện `eligible/denyReason/suppressionMarkerId` — **cần bổ sung** `do_not_call/opt_out_scope/reason/effective_at`. Order Core gọi, fail-closed nếu lỗi.
> - **QC2→DC-02:** ✅ channel-specific — opt-out SMS ≠ chặn `PHONE_CALL`; IVR đọc riêng PHONE_CALL.
> - **QC3→DC-03:** IVR confirmation = transactional → **KHÔNG áp CRM marketing quiet/frequency-cap**; chỉ tôn trọng PHONE_CALL suppression tuyệt đối; **không** đi qua automation rule.
> - **QC4→DC-04:** TRANSACTIONAL/SERVICE không cần marketing opt-in; vẫn phải pass service-contact eligibility + không suppressed. Legal basis ở registry.
> - **QC5→DC-05:** ⚠️ **chưa có event sau Core decision cho IVR outcome** (Order Core có transition `ivr-confirm/ivr-reject/timeout`; catalog có `ORDER_CONFIRMED/CANCELLED/EXPIRED` nhưng chưa publish; notification no-op). Cần implement; IVR/SIM không gửi (D-14).
> - **QC6→DC-06:** ⚠️ **chưa có CustomerTrustResolver/trusted_skip_allowed** (out-of-scope P3.2) → trusted-skip hiện KHÔNG khả dụng → default **luôn gọi IVR**.
>
> Các ô chi tiết + source path bên dưới giữ làm biên bản gốc.

## 0. Bối cảnh (vì sao có vòng 2 này)

- IVR gọi **outbound xác nhận Official Order** (giao dịch, không marketing). Kết quả là **signal**; Order Core quyết định trạng thái.
- Vòng 1 (Module 3/3.1) đã khóa D-01..D-14. Khi hỏi **Ops-Core**, họ đính chính: **"do-not-call / opt-out / call-restriction" KHÔNG thuộc ops-core** (ops "suppression" chỉ là procurement/MRP). ⇒ blocker "khách đã từ chối nhận cuộc gọi" phải lấy từ **CRM / business-platform**. Đây là điểm vòng 1 chưa hỏi.
- **Nguyên tắc IVR:** nếu không kiểm được do-not-call/opt-out → **fail-closed, không gọi**.
- **Cách trả lời:** mỗi câu có *"Đề xuất từ IVR"* — chọn **[ ] Xác nhận** / **[ ] Điều chỉnh**, điền ô **Trả lời**, kèm endpoint/format nếu có.

Ưu tiên: **P0** chặn gọi thật · **P1** cần sớm.

---

### QC1 (P0) — Nguồn & endpoint do-not-call / opt-out / call-restriction
IVR/Order Core cần biết khách có bị chặn gọi không **trước khi dispatch**.

**Đề xuất từ IVR:** CRM là owner "call-restriction registry". Order Core hợp nhất cờ này vào task IVR (kèm blocker snapshot) — tương tự cách nhúng SellableStatus. Cần: (a) endpoint đọc trạng thái theo customer/phone, hoặc (b) CRM đẩy cờ để Order Core đưa vào task. Trả về tối thiểu: `do_not_call` (bool), `opt_out_scope`, `reason`, `effective_at`.

- [ ] Xác nhận (CRM cấp, Core hợp nhất vào task) · [ ] Điều chỉnh (endpoint/cơ chế khác)
- **Trả lời (endpoint/format/where):** ______________________________________________
- Người trả lời / ngày: __________

### QC2 (P0) — Opt-out theo kênh: "opt-out SMS" có = "opt-out cuộc gọi IVR" không?
Khách có thể chỉ từ chối SMS/marketing, không phải cuộc gọi xác nhận đơn.

**Đề xuất từ IVR:** consent/opt-out phải **theo kênh (channel-specific)**. IVR chỉ cần cờ **voice/call-restriction** riêng; opt-out marketing SMS KHÔNG tự động chặn cuộc gọi xác nhận giao dịch. Xin xác nhận mô hình consent (per phone / per customer / per channel) và trường IVR nên đọc.

- [ ] Xác nhận (channel-specific, có cờ voice riêng) · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QC3 (P1) — Quiet hours & frequency cap cho cuộc gọi xác nhận
CRM có chính sách quiet_hours / frequency_cap (theo MASTER-02). Cuộc gọi xác nhận đơn là **giao dịch, có window ngắn (Giờ Vàng 5′/24-7 15′)**.

**Đề xuất từ IVR:** cuộc gọi xác nhận đơn (transactional, trong confirmation window) **được miễn** quiet_hours/frequency_cap marketing — vì nếu chặn theo giờ thì đơn hết window. Nhưng vẫn tôn trọng do-not-call tuyệt đối (QC1). Xin xác nhận: giao dịch có được miễn quiet-hours không? Nếu không, xử lý đơn rơi vào quiet-hours thế nào?

- [ ] Xác nhận (miễn cho transactional) · [ ] Điều chỉnh (nêu rule quiet-hours áp dụng)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QC4 (P1) — Cơ sở pháp lý / consent cho cuộc gọi xác nhận
Để không vi phạm PII/telemarketing.

**Đề xuất từ IVR:** cuộc gọi chỉ để **xác nhận đơn khách vừa đặt** (legitimate/transactional), không cần consent marketing; vẫn tôn trọng do-not-call. Xin CRM/Legal xác nhận cơ sở này, và ai giữ "consent registry".

- [ ] Xác nhận · [ ] Điều chỉnh
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

### QC5 (P1) — Nhận outcome sau Core decision (D-14) & thông báo sau khi hủy/expire
D-14 đã chốt: **IVR chỉ ghi audit nội bộ, KHÔNG ghi CRM**; nếu CRM cần outcome thì **nhận event sau Core decision** và tự xử lý.

**Đề xuất từ IVR:** CRM subscribe event sau khi **Order Core** quyết định (confirmed/cancelled/no-answer/expired), rồi CRM tự quyết gửi thông báo (VD "không liên hệ được"/"đơn đã hủy"). IVR/SIM **không** tự gửi. Xin xác nhận: tên event CRM mong nhận + CRM có tự gửi thông báo sau no-answer/expire không (template ai giữ)?

- [ ] Xác nhận (CRM nhận event sau Core; IVR không gửi) · [ ] Điều chỉnh
- **Trả lời (event name + notification template owner):** ______________________________________________
- Người trả lời / ngày: __________

### QC6 (P1) — Owner của Customer Trust Resolver (liên quan D-12)
D-12 đã khóa: skip IVR chỉ khi Trust Resolver = TRUSTED + `trusted_skip_allowed`. Cần rõ resolver này thuộc ai.

**Đề xuất từ IVR:** Customer Trust Resolver thuộc CRM/business-platform (gần customer memory); Order Core gọi và nhúng `customer_trust_status`/`trusted_skip_allowed`/`risk_flags` vào task. Xin xác nhận owner + cách Core lấy.

- [ ] Xác nhận (CRM/business-platform owner) · [ ] Điều chỉnh (owner khác)
- **Trả lời:** ______________________________________________
- Người trả lời / ngày: __________

---

## Tổng hợp
| Câu | Chủ đề | Ưu tiên |
| --- | --- | --- |
| QC1 | Nguồn do-not-call/opt-out/call-restriction | **P0** |
| QC2 | Opt-out theo kênh (SMS ≠ voice) | **P0** |
| QC3 | Quiet hours / frequency cap cho transactional call | P1 |
| QC4 | Cơ sở pháp lý/consent | P1 |
| QC5 | Nhận outcome event + thông báo sau hủy/expire | P1 |
| QC6 | Owner Customer Trust Resolver | P1 |

**Chặn gọi thật:** QC1, QC2.

## Ô tổng kết
- Người duyệt CRM/Module 3.1: ____________ · Ngày: ______
- Ghi chú: ______________________________________________

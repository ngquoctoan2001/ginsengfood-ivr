# SRS-04 — Glossary

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p02`
Nguồn: `phase-8/00 §3`, `MASTER-03`, `docx` §6, §8, §13.

| Thuật ngữ | Định nghĩa |
| --- | --- |
| **IVR (Order Confirmation)** | Hợp phần gọi tự động OUTBOUND xác nhận Official Order qua Internal SIM Gateway. Module 8 / PACK-09 / phase-8. |
| **Order Core** | Commerce Order Core / Order State Machine — chủ sở hữu trạng thái đơn; lớp quyết định cuối. |
| **Official Order** | Đơn chính thức (có `order_code`) do Commerce tạo từ Customer Confirmation hợp lệ. |
| **order_status** | Opaque Sales-owned order status. Target V1 chỉ nhận task khi Sales đánh callable; state values cần Sales ký theo từng program. |
| **CONFIRMING** | Current Sales state used by existing flows; Target V1 callable-state matrix remains an external contract. |
| **Program/payment matrix** | Target V1: `GOLDEN_HOUR+ONLINE` và `TWENTY_FOUR_SEVEN+COD`, đều `ivr_confirmation_required=true`. |
| **ORDER_VERIFIED** | Gate downstream (CRM/commission/reporting) = `DELIVERED` + `payment_status=PAID` + `verification_status∈{VERIFIED,TRUSTED}` (DS-05). Không liên quan trực tiếp IVR. |
| **order_code / order_code_short** | Mã đơn chính thức / mã rút gọn được phép đọc trong call script. |
| **order_version** | Sales-owned optimistic version. Target V1 bắt buộc trong task/callback; current Sales còn GAP nên real integration bị chặn. |
| **IVR task** (`IvrConfirmationTaskV1`) | Chỉ thị nội bộ do Module 3 tạo sau khi đã quyết định nghiệp vụ rằng đơn cần gọi; IVR kiểm gate kỹ thuật/an toàn rồi thực thi (`OD-18`). |
| **IVR result** | Kết quả đã normalize từ cuộc gọi; là **signal**, không phải state cuối. |
| **Result callback** (`IvrConfirmationResultCallbackV1`) | Bản tin IVR gửi result về Order Core để revalidate. |
| **CallJob / call_job** | Vòng đời gọi cho một task; chứa các attempt. |
| **Attempt** | Một lần gọi khách. Phân biệt **customer-counted** vs **technical retry**. |
| **DTMF** | Phím khách bấm; `1`=xác nhận, `0`=hủy; sai phím / không bấm / lỗi DTMF có xử lý riêng. |
| **Confirmation window** | Khoảng cho phép xác nhận, tính từ **`T0`** (=lúc Core mở window/tạo task). ✅ D-10: Giờ Vàng **5 phút**, 24/7 **15 phút**. |
| **Golden Hour (Giờ Vàng)** | Chương trình khung giờ; window **5′** (A1@T0, A2@T0+2:30, expire T0+5:00); ưu tiên scheduler cao. ✅ D-10. |
| **24/7 (TWENTY_FOUR_SEVEN)** | Chương trình thường; window **15′** (A1@T0, A2@T0+7:30, expire T0+15:00). ✅ D-10. |
| **T0** | Thời điểm Order Core mở IVR confirmation window / tạo task (KHÔNG phải lúc khách bấm đặt nếu task delay). ✅ D-10. |
| **Attempt policy** | Quy tắc số cuộc + khoảng cách + window theo program. |
| **Eligibility** | Kết quả kiểm contract và gate kỹ thuật/an toàn của IVR (source/contact/block/window/capacity); không phân loại lại khách cũ/khách mới. |
| **Trusted skip** / **Returning-customer skip** | `SUPERSEDED` bởi `OD-18`. `TASK_SKIPPED_TRUSTED_CUSTOMER` chỉ còn `LEGACY_READ` cho enum/row lịch sử; runtime hiện hành không phát sinh quyết định này. |
| **Official contact** | Số điện thoại đã duyệt để gọi; dùng `phone_ref`/`phone_masked`/dial token. |
| **phone_ref / phone_masked / dial token** | Tham chiếu bảo mật / số che / token quay số TTL ngắn — thay cho raw phone. |
| **Sale Lock (khóa bán)** | Ops-core chặn bán SKU/lô. ⚠️ **Hiện = recall-triggered** (`op_sale_lock_registry.recall_case_id` là FK bắt buộc); chưa có sale-lock thương mại độc lập. Owner: Operational Core. (DO-CORR-3) |
| **Recall (thu hồi)** | Trạng thái thu hồi sản phẩm/lô (owner: Operational Core); `recall_case_id`=Guid + `recall_no`. |
| **Suppression** | ⚠️ Hai nghĩa: **(thương mại) do-not-call/opt-out/call-restriction = CRM/business-platform** (blocker IVR thực dùng); **ops-core "suppression" = procurement/MRP (FRM-05)**, KHÔNG phải blocklist. (DO-CORR-2) |
| **Blocker** | CRM: do-not-call/opt-out/call-restriction. Blocker tồn kho/thu hồi do **Order Core** kiểm lúc revalidate (`D-06`), IVR không đọc. Bất kỳ cái nào active → không dispatch/confirm. |
| **Result normalizer** | Thành phần chuyển raw SIM/DTMF result thành result code chuẩn + reason + evidence. |
| **Revalidation** | Order Core kiểm lại (version/state/blocker/evidence) trước khi transition theo signal IVR. |
| **Idempotency key** | Khóa chống duplicate cho task/callback/admin/retry. |
| **Correlation id** | ID trace xuyên Order Core → IVR → SIM Adapter → Evidence. |
| **Technical exception** | Lỗi kỹ thuật (SIM/server/DTMF/audio/callback/scheduler) — **không** tính là khách không nghe. |
| **Capacity incident** | Sự cố quá tải queue/SIM (pending/expired/missed deadline vượt ngưỡng). |
| **Evidence** | Bằng chứng có trace/owner/status/audit; chỉ `ACCEPTED` mới dùng PASS. |
| **Release gate** | Cổng kiểm soát trước khi cho gọi khách thật (`REAL_CUSTOMER_CALL_ALLOWED`). |
| **Rolling queue (deadline-aware)** | Xếp lịch theo deadline, không dồn cuối phiên (`BATCH_AFTER_SESSION=PROHIBITED`). |
| **ONE_SIM_ONE_ACTIVE_CALL** | Một SIM chỉ một cuộc active tại một thời điểm. |
| **Owner Decision Required** | Nhãn cho quyết định nghiệp vụ implementer không được tự suy diễn. |

# 07 - THANH TOÁN / VẬN CHUYỂN / IVR / ĐƠN HÀNG

## 1. Mục tiêu

File này khóa các guard giao dịch bổ sung trong PHASE-03.1:

- Bank transfer/VietQR.
- Payment reference.
- Accounting Review Queue.
- Manual/auto reconciliation.
- Shipping ETA/tracking/fallback.
- IVR khách mới/high-risk.
- Order edit/cancel.
- Return/refund/product issue.
- VAT invoice/company payment.

Đây là nguồn triển khai chính cho workstream P3.1-2F.

## 2. Source đã gom

| Source | Nội dung đã gom |
| --- | --- |
| `5. bo sung/2. BẢN KHÓA BỔ SUNG CÁC TÌNH HUỐNG ĐỜI THƯỜNG TRƯỚC KHI VIẾT LẠI TÀI LIỆU.md` | AI-TEST-NOTE 082, 083, 084, 091, 092, 093, payment/shipping/IVR/order guards. |
| `5. bo sung/4. chính sách và hoa hồng thành viên.md` | Quote/order/payment relationship, IVR +5 phút trong Golden Hour. |
| PHASE-03 Payment/Shipping/Order | Payment Core, Shipping Core, Order Core, invoice/tax boundary. |
| PHASE-08 IVR | IVR chỉ xác nhận đơn nếu thuộc scope. |

## 3. Payment bank transfer/VietQR

### 3.1. Khi khách chọn chuyển khoản

Ngay khi khách chọn chuyển khoản ngân hàng:

```text
payment_method = BANK_TRANSFER
payment_channel = VIETQR_OR_BANK_ACCOUNT
payment_status = BANK_TRANSFER_PENDING
accounting_review_status = WAITING_BANK_TRANSFER
payment_reference = unique_payment_reference
payment_verification_source = PENDING_RECONCILIATION
```

Không để order chuyển khoản chỉ nằm trạng thái chung chung `PAYMENT_PENDING`.

### 3.2. Payment reference

Payment reference phải:

- Duy nhất.
- Gắn với một order/draft hợp lệ.
- Có amount expected.
- Có expiry nếu policy yêu cầu.
- Dễ đối soát.
- Được gửi cho khách kèm số tiền và nội dung chuyển khoản.

Không dùng chung payment reference cho nhiều order.

### 3.3. Bank account

AI/Gateway/CRM/front-end static template không hardcode thông tin tài khoản ngân hàng. Mọi thông tin phải lấy từ:

- Company Bank Account Registry.
- Approved payment configuration.
- Payment Core response.

Khi làm evidence, phải mask dữ liệu nhạy cảm theo policy.

## 4. Reconciliation

### 4.1. Manual reconciliation

Khi chưa có bank API/webhook:

- Kế toán lọc queue `WAITING_BANK_TRANSFER`.
- Match payment_reference, amount, memo, order.
- Nếu match đúng, Accounting Review xác nhận.
- Nếu sai số tiền/nội dung, không tự PAID.
- Nếu duplicate transaction, chặn và audit.

### 4.2. Auto reconciliation

Khi có bank webhook/API:

- Webhook không tự tin tuyệt đối nếu thiếu match.
- Match theo payment_reference trước.
- Validate amount.
- Validate duplicate.
- Validate status.
- Sau đó Payment Core/Accounting Review mới set paid.

### 4.3. State đề xuất

| State | Ý nghĩa |
| --- | --- |
| `BANK_TRANSFER_PENDING` | Khách chọn chuyển khoản, chờ thanh toán. |
| `PAYMENT_RECONCILING` | Đang đối soát. |
| `PAID_BY_BANK_TRANSFER` | Đã xác nhận thanh toán qua ngân hàng. |
| `PAYMENT_RECONCILIATION_FAILED` | Đối soát fail. |
| `ACCOUNTING_REVIEW_REQUIRED` | Cần kế toán/human review. |

## 5. AI payment wording

AI chỉ được nói:

- Đơn đang chờ chuyển khoản.
- Em gửi thông tin chuyển khoản theo đơn này.
- Sau khi kế toán/Payment Core xác nhận, trạng thái thanh toán sẽ được cập nhật.

AI không được nói:

- Đã thanh toán chỉ vì khách gửi ảnh.
- Em đã xác nhận tiền về.
- Đơn đã PAID khi chưa có confirmation.

## 6. Shipping ETA/tracking

### 6.1. Có tracking realtime

Nếu Shipping Core trả tracking:

- Trả trạng thái.
- Trả tracking link nếu allowed.
- Không hỏi lại địa chỉ nếu order đã có shipping info.
- Không bịa carrier status.

### 6.2. Không có tracking realtime

Dùng fallback:

| Vùng | ETA fallback |
| --- | --- |
| Miền Nam | 2-3 ngày |
| Miền Trung | 3-5 ngày |
| Miền Bắc | 5-7 ngày |

Wording phải nói rõ đây là dự kiến/fallback, không cam kết tuyệt đối.

### 6.3. International shipping

Không cam kết gửi quốc tế nếu chưa có `InternationalShippingEligibilityResolver` và owner-approved extension.

## 7. IVR xác nhận đơn và fake order prevention

### 7.1. Khi nào IVR required

IVR required nếu có một hoặc nhiều điều kiện:

- Customer mới.
- verified_order_count = 0.
- Không có lịch sử mua thành công.
- suspicious duplicate orders.
- COD fail history vượt ngưỡng.
- high-risk shipping address.
- suspicious phone pattern.
- abnormal order value.
- high-risk Golden Hour behavior.

### 7.2. Rule bắt buộc

- Không generate official order_code trước IVR pass nếu IVR required.
- Không release fulfillment trước IVR confirmed.
- Mọi IVR state phải audit.
- Golden Hour phải release quota nếu IVR fail/timeout theo policy.
- Technical error không được tính là customer no-answer nếu Phase 8 áp dụng.

### 7.3. IVR không được làm

- Không sales call.
- Không CRM/marketing call.
- Không tư vấn sản phẩm.
- Không tự hủy đơn ngoài Order State Machine.
- Không tự cập nhật payment.

## 8. Order edit/address/cancel

Khách muốn sửa đơn phải:

1. Resolve order state.
2. Resolve fulfillment state.
3. Resolve payment state.
4. Resolve quote expiry.
5. Resolve Golden Hour quota nếu liên quan.
6. Audit change.

Nếu chưa bàn giao vận chuyển:

- Có thể sửa theo policy.
- Thêm/bớt SKU phải revalidate quote/order draft.
- Đổi payment method phải qua Payment Core.

Nếu đã bàn giao vận chuyển:

- Route delivery support.
- Không tự sửa địa chỉ nếu locked.

Nếu đã thanh toán:

- Áp dụng payment/refund policy.
- Không hứa hoàn tiền khi chưa đủ dữ liệu.

## 9. Return/refund/product issue

Khi khách báo:

- Giao sai.
- Thiếu hàng.
- Rách gói.
- Vón cục.
- Mùi lạ.
- Muốn đổi vị.
- Muốn hoàn tiền.

AI/CRM phải:

- Xin lỗi và tiếp nhận.
- Xin ảnh/mã đơn/mã lô nếu cần.
- Phân loại delivery issue/product quality/wrong item/missing item/taste preference/counterfeit suspicion.
- Route QA/CSKH/legal nếu nghi hàng giả.
- Không kết luận lỗi sản xuất khi chưa QA.
- Không tiếp tục bán khi complaint open.

## 10. VAT invoice/company payment

Xuất hóa đơn VAT cần:

- Tên công ty.
- Mã số thuế.
- Địa chỉ.
- Email nhận hóa đơn.
- Mã đơn.

Không xuất hóa đơn nếu thiếu dữ liệu. Không bịa VAT. Không nói "VAT nếu áp dụng" mơ hồ nếu quote/order đã có VAT rule.

Company payment bằng chuyển khoản phải đi Payment Core và Accounting Review.

## 11. Contract tối thiểu

| Contract | Fields |
| --- | --- |
| BankTransferPaymentReference | payment_reference, order_id, amount, currency, expires_at, account_registry_id, status |
| AccountingReviewDecision | review_id, payment_reference, match_status, matched_amount, matched_memo, reviewed_by, audit_ref |
| PaymentReconciliationEvent | event_id, source, transaction_ref, amount, memo, duplicate_status, match_status |
| ShippingETAResult | order_id, region, tracking_status, tracking_link, fallback_eta, freshness_status |
| IVRRequiredDecision | order_draft_id, required, risk_reasons, attempt_policy, quota_release_policy |
| OrderEditEligibilityDecision | order_id, requested_change, allow_edit, deny_reason, requires_requote |
| ReturnRefundCase | case_id, order_id, issue_type, evidence_required, status, owner_queue |
| VATInvoiceRequest | order_id, company_name, tax_code, address, email, validation_status |

## 12. UI/Admin impact

Phải có hoặc review:

- Accounting Bank Transfer Queue.
- Payment reconciliation detail.
- Duplicate transaction warning.
- Order payment status display:
  - Chờ thanh toán chuyển khoản.
  - Đang đối soát chuyển khoản.
  - Đã thanh toán qua ngân hàng.
- Shipping tracking/fallback view.
- IVR task/result view.
- Return/refund case queue.
- VAT invoice request view.
- Audit trail.

Admin không được tự set PAID nếu thiếu permission/audit/reason.

## 13. P0 blocker

- PAID thiếu Payment Core/Accounting confirmation.
- Bank transfer thiếu payment_reference.
- Payment reference duplicate.
- Kế toán không lọc được bank transfer pending.
- Sai số tiền/nội dung vẫn paid.
- Shipping ETA bịa.
- IVR required nhưng order_code official sinh trước pass.
- IVR fail/timeout không release Golden Hour quota.
- Order locked vẫn sửa không policy.
- Refund/product issue vẫn tiếp tục bán.
- VAT thiếu MST/company/email vẫn xuất.

## 14. Smoke tối thiểu

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P3_1-2F-001 | Khách chọn bank transfer | BANK_TRANSFER_PENDING + payment_reference duy nhất. |
| P3_1-2F-002 | Khách gửi ảnh chuyển khoản | Không PAID khi chưa reconcile. |
| P3_1-2F-003 | Duplicate bank transaction | Duplicate blocked/audit. |
| P3_1-2F-004 | Sai amount | Không paid, accounting review required. |
| P3_1-2F-005 | Tracking available | Trả tracking từ Shipping Core. |
| P3_1-2F-006 | Tracking missing miền Nam | Fallback 2-3 ngày. |
| P3_1-2F-007 | New high-risk COD | IVR required, no official order_code before pass. |
| P3_1-2F-008 | IVR timeout Golden Hour | Release quota theo policy. |
| P3_1-2F-009 | Đổi địa chỉ sau bàn giao | Route delivery support. |
| P3_1-2F-010 | Khách cần VAT thiếu MST | Không xuất hóa đơn, yêu cầu bổ sung. |
| P3_1-2F-011 | Rách gói/mùi lạ | Open case, không bán tiếp. |

## 15. Evidence bắt buộc

- Payment reference sample masked.
- Accounting review decision.
- Reconciliation event.
- Duplicate transaction evidence.
- Shipping ETA/tracking decision.
- IVR required decision.
- IVR state transition log.
- Order edit/refund/VAT case audit.
- Smoke output.

## 16. Done gate

Workstream 07 DONE khi:

- Bank transfer/VietQR có payment reference.
- Accounting review/reconciliation rõ.
- Shipping fallback rõ.
- IVR high-risk rõ.
- Order edit/refund/VAT rõ.
- P0 smoke có evidence.
- Không release claim.

## 17. Final status

`P3.1-2F HOÀN TẤT BÀN GIAO - CHỜ TRIỂN KHAI/BẰNG CHỨNG - GATEWAY BLOCKED`

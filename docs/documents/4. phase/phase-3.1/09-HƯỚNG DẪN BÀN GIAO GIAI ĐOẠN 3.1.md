# 09 - HƯỚNG DẪN BÀN GIAO GIAI ĐOẠN 3.1

## 1. Vai trò

Tài liệu này là chỉ mục bàn giao chính thức cho GIAI ĐOẠN 3.1.

Mục tiêu là để ChatGPT, sếp, dev hoặc Codex đọc đúng thứ tự, hiểu đúng nguồn, không bỏ sót tài liệu bổ sung và không conflict với phase khác.

Nếu dev đang chuyển Phase 3 sang SRS hoặc chuẩn bị implementation, phải đọc `../phase-3/11-GÓI-CHUẨN-DUYỆT-SRS-TRIỂN-KHAI-GIAI-ĐOẠN-3.md` trước khi dùng Giai đoạn 3.1 làm supplement.

## 2. Thư mục chính thức

`docs/documents/4. phase/phase-3.1/`

## 3. Tài liệu nguồn trong thư mục

6 file trong `5. bo sung/` là nguồn gốc đã được gom lại. Không sửa các file đó trong lane này. Bộ file tiếng Việt cấp ngoài là bộ nguồn chính để triển khai.

## 4. Bộ file chính thức

| Thứ tự đọc | File | Vai trò |
| --- | --- | --- |
| 0 | `TÀI LIỆU TRIỂN KHAI DIAMOND HOA HỒNG, THƯỞNG MỚI.md` | Bảng gom nguồn chính Giai đoạn 3.1. |
| 1 | `00-PHAN-TICH-HIEN-TRANG-GIAI-DOAN-3-1.md` | Phân tích hiện trạng. |
| 2 | `01-THIẾT-KẾ-KỸ-THUẬT-GIAI-ĐOẠN-3-1.md` | Thiết kế kỹ thuật. |
| 3 | `02-THÀNH-VIÊN-GIÁ-CHƯƠNG-TRÌNH-24-7-GIỜ-VÀNG.md` | Member/price/program. |
| 4 | `03-DIAMOND-GIỚI-THIỆU-HOA-HỒNG-ĐỐI-SOÁT.md` | Diamond/referral/commission. |
| 5 | `04-TIN-NHẮN-GIỜ-VÀNG-CRM-ĐA-KÊNH.md` | Messaging/trigger/channel/email. |
| 6 | `05-CRM-12-THÁNG-VÒNG-ĐỜI-THÀNH-VIÊN.md` | CRM lifecycle/member automation. |
| 7 | `06-AI-ADVISOR-CHÍNH-SÁCH-LÕI-TÌNH-HUỐNG-THỰC-CHIẾN.md` | AI rules/situation locks. |
| 8 | `07-THANH-TOÁN-VẬN-CHUYỂN-IVR-ĐƠN-HÀNG.md` | Payment/shipping/IVR/order. |
| 9 | `08-KIỂM-THỬ-BẰNG-CHỨNG-BÁO-CÁO-GIAI-ĐOẠN-3-1.md` | Kiểm thử/bằng chứng/báo cáo. |
| 10 | `10-PHỤ-LỤC-KHÓA-LIÊN-GIAI-ĐOẠN-DIAMOND-CRM-AI.md` | Khóa liên giai đoạn. |

## 5. Thứ tự triển khai bắt buộc

1. Đọc file tổng.
2. Chạy `00` để phân tích hiện trạng.
3. Chạy `01` để thiết kế kỹ thuật.
4. Chỉ khi `00/01` đủ điều kiện, chạy từng workstream:
   - `02` Member/price/program.
   - `03` Diamond/referral/commission.
   - `04` Messaging.
   - `05` CRM lifecycle.
   - `06` AI situation.
   - `07` Payment/shipping/IVR/order.
5. Chạy `08` để gom smoke/evidence/report.
6. Dùng `10` để kiểm cross-phase trước khi bàn giao downstream.

Không chạy implementation trước analysis/design.

## 6. Dependency chính

| Dependency | Điều kiện |
| --- | --- |
| Phase 1 Product/SKU | Product pillar/dietary/claim đã có owner truth. |
| Phase 2 Operational | Sellable, stock, quality hold, recall, sale lock rõ. |
| Phase 3 Commerce | QuoteSnapshot, Order Draft, Official Order, Payment, Shipping, Verified Revenue rõ. |
| Phase 4 AI | AI là consumer, không self-price/order/payment. |
| Phase 5 Gateway | Gateway delivery-only, public/private guard. |
| Phase 8 IVR | IVR chỉ xác nhận đơn nếu scope active. |

## 7. Workstream handoff

| Workstream | Bàn giao cho | Điều kiện |
| --- | --- | --- |
| Member/price/program | Commerce, CRM, AI | QuoteSnapshot và Member Core decision rõ. |
| Diamond | CRM, Ads, Admin | Verified Revenue và commission ledger rõ. |
| Messaging | CRM Delivery, Gateway | Template/trigger/dedup/suppression rõ. |
| CRM lifecycle | CRM, AI | Customer Memory, Order History, member lifecycle rõ. |
| AI situation | AI Advisor, Gateway, CRM | FinalGuard/ClaimGuard/situation registry rõ. |
| Payment/shipping/IVR | Commerce, Admin, AI, CRM | Payment Core, Shipping Core, IVR decision rõ. |
| Smoke/evidence | QA/Owner/Release | P0 smoke và evidence đủ. |

## 8. Rule không được vi phạm

- Không tự tính giá.
- Không tự tính member benefit.
- Không tự tính Diamond benefit/commission.
- Không quote thiếu QuoteSnapshot.
- Không PAID thiếu confirmation.
- Không sinh official order_code nếu IVR required chưa pass.
- Không gửi CRM khi opt-out/open case.
- Không rewrite message text.
- Không public private data.
- Không claim chữa bệnh.
- Không dùng Phase 3.1 để mở Gateway.
- Không gọi release-ready.

## 9. Checklist review cho sếp/owner

| Câu hỏi | Phải trả lời được ở file |
| --- | --- |
| Tài liệu nguồn nào đã gom? | File tổng, section 1. |
| Phase 3.1 có thay Phase 3 không? | Không, file tổng và file 10. |
| Member/24/7/Giờ Vàng ở đâu? | File 02. |
| Diamond/hoa hồng ở đâu? | File 03. |
| Tin nhắn ở đâu? | File 04. |
| CRM 12 tháng ở đâu? | File 05. |
| AI test notes ở đâu? | File 06. |
| Bank transfer/shipping/IVR ở đâu? | File 07. |
| Smoke/evidence ở đâu? | File 08. |
| Cross-phase lock ở đâu? | File 10. |

## 10. P0 blocker carry-over

Nếu implementation gặp blocker, phải dùng ID:

- `P3_1-BLOCKED-MISSING-MEMBER-CORE`.
- `P3_1-BLOCKED-MISSING-QUOTE-SNAPSHOT`.
- `P3_1-BLOCKED-MISSING-DIAMOND-RUNTIME`.
- `P3_1-BLOCKED-MISSING-MESSAGE-REGISTRY`.
- `P3_1-BLOCKED-MISSING-CRM-SUPPRESSION`.
- `P3_1-BLOCKED-MISSING-PRODUCT-EFFECTIVENESS`.
- `P3_1-BLOCKED-MISSING-FINAL-GUARD`.
- `P3_1-BLOCKED-MISSING-PAYMENT-RECONCILIATION`.
- `P3_1-BLOCKED-MISSING-SHIPPING-ELIGIBILITY`.
- `P3_1-BLOCKED-MISSING-IVR-DECISION`.

## 11. Final handoff status

```text
GIAI ĐOẠN 3.1 - HƯỚNG DẪN BÀN GIAO:
Documentation: COMPLETE
Source coverage: COMPLETE
Triển khai: CHƯA THỰC HIỆN THEO HƯỚNG DẪN BÀN GIAO
Smoke: REQUIRED DURING IMPLEMENTATION
Evidence: REQUIRED DURING IMPLEMENTATION
Release: NOT RELEASE READY
Gateway: BLOCKED
```

## 12. Done gate

HƯỚNG DẪN BÀN GIAO DONE khi:

- Tên file tiếng Việt.
- Thứ tự chạy rõ.
- File nguồn đã gom rõ.
- Dependency rõ.
- Workstream rõ.
- P0 blocker rõ.
- Không release claim.

## 13. Final status

`HƯỚNG DẪN BÀN GIAO GIAI ĐOẠN 3.1 COMPLETE - GATEWAY BLOCKED`

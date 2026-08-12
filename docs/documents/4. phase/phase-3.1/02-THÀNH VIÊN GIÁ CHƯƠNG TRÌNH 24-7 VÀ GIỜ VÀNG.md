# 02 - THÀNH VIÊN / GIÁ / CHƯƠNG TRÌNH 24/7 / GIỜ VÀNG

## 1. Mục tiêu

File này khóa toàn bộ rule thành viên, giá, chương trình 24/7, Giờ Vàng Tri Ân, quyền mua sớm và QuoteSnapshot policy cho PHASE-03.1.

Đây là nguồn triển khai chính cho workstream P3.1-2A.

## 2. Source đã gom

| Source | Nội dung đã gom |
| --- | --- |
| `5. bo sung/4. chính sách và hoa hồng thành viên.md` | Chu kỳ thành viên, tier, benefit, maintain, grace, listed price, 24/7, Golden Hour, early access, non-stacking, QuoteSnapshot. |
| `5. bo sung/1. BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE.md` | Runtime-first, no self-price, QuoteSnapshot, member lifecycle proactive, priority conflict. |
| PHASE-03 Commerce Runtime | Sellable, QuoteSnapshot, order/payment/shipping boundary. |

## 3. Nguyên tắc owner truth

| Nhóm dữ liệu | Owner truth | Consumer |
| --- | --- | --- |
| Hạng thành viên | Member Core | AI, CRM, Gateway, Admin |
| Doanh số hợp lệ | Commerce Verified Order / Verified Revenue | Member Core, Diamond Core, Ads |
| Giá niêm yết | Pricing Core | QuoteSnapshot, AI consumer |
| Giá chương trình | ProgramPriceResolver | QuoteSnapshot |
| Quyền lợi member | MemberBenefitResolver | QuoteSnapshot, CRM |
| Quyền mua sớm | GoldenHourEligibilityResolver | Gateway, CRM, Live |
| Final total | QuoteSnapshot | AI, CRM, Gateway |

Không consumer nào được tự tính các giá trị trên.

## 4. Chu kỳ thành viên

### 4.1. Chu kỳ

- `member_cycle_duration = 12 tháng`.
- `cycle_start_date = ngày khách được công nhận hạng`.
- `cycle_end_date = cycle_start_date + 12 tháng`.

Hết chu kỳ, runtime kiểm tra doanh số hợp lệ để quyết định:

- Duy trì hạng hiện tại.
- Nâng hạng.
- Vào ân hạng.
- Hạ hạng an toàn sau ân hạng.

### 4.2. Doanh số hợp lệ

Chỉ tính doanh số từ order hợp lệ theo Order Runtime:

- `ORDER_VERIFIED`.
- `PAID`.
- `COD_VERIFIED`.
- Hoặc trạng thái verified tương đương đã được Commerce owner chấp thuận.

Không tính:

- Quote.
- Cart.
- Order draft.
- Đơn hủy.
- Đơn hoàn.
- Đơn lỗi.
- Đơn test.
- Đơn trùng idempotency.
- Đơn gian lận.
- Đơn chưa qua xác nhận hợp lệ.
- COD waiting chưa verified.

## 5. Tier và quyền lợi

| Hạng | Doanh số hợp lệ trong chu kỳ | Quyền lợi giảm giá |
| --- | ---: | ---: |
| Silver | 1.000.000đ - 2.999.999đ | 5% |
| Gold | 3.000.000đ - 4.999.999đ | 8% |
| Platinum | 5.000.000đ - 7.999.999đ | 12% |
| Diamond | Từ 8.000.000đ trở lên | 20% |

Quyền lợi chỉ áp dụng khi:

- Hạng còn hiệu lực.
- Member trong chu kỳ hợp lệ hoặc grace hợp lệ.
- Chương trình bán hàng cho phép áp dụng quyền lợi.
- Runtime Core xác nhận quyền lợi tại thời điểm quote.
- QuoteSnapshot ghi lại quyền lợi được áp dụng.

## 6. Duy trì hạng và ân hạng

### 6.1. Maintain rule

Duy trì hạng = đạt tối thiểu 50% doanh số chuẩn của hạng hiện tại.

| Hạng hiện tại | Doanh số chuẩn | Doanh số duy trì tối thiểu |
| --- | ---: | ---: |
| Silver | 1.000.000đ | 500.000đ |
| Gold | 3.000.000đ | 1.500.000đ |
| Platinum | 5.000.000đ | 2.500.000đ |
| Diamond | 8.000.000đ | 4.000.000đ |

### 6.2. Grace rule

Nếu hết chu kỳ chưa đạt duy trì:

- Member vào ân hạng 60 ngày.
- Trong ân hạng, tạm giữ hạng hiện tại.
- Nếu đạt duy trì trong ân hạng, giữ hạng.
- Nếu đạt hạng cao hơn trong ân hạng, có thể nâng hạng nếu policy cho phép.
- Hết 60 ngày vẫn chưa đạt thì hạ hạng theo runtime.

### 6.3. Tone khi thông báo

AI/CRM không được làm khách thấy bị dọa hoặc xấu hổ. Wording phải theo hướng chăm sóc:

- Nói số còn thiếu nếu runtime có.
- Gợi ý sản phẩm có giá trị sử dụng thật.
- Không ép mua vì điểm/hạng.

## 7. Giá niêm yết và sellable

SKU chỉ được mở bán khi đồng thời:

- Thành phẩm hoàn tất sản xuất.
- Đã nhập kho hợp lệ.
- Có tồn kho khả dụng.
- Không quality hold.
- Không recall hold.
- Không sale lock.
- Không channel block.
- Giá niêm yết chính thức active.
- Runtime Core xác nhận được phép bán.

Điều kiện tối thiểu:

```text
sellable_status = SELLABLE
listed_price_status = ACTIVE
stock_available > 0
```

Khi SKU non-sellable:

- Không quote.
- Không tạo order.
- Không public số lượng tồn.
- Không nói "chỉ còn vài hộp" để tạo khan hiếm giả.
- Chỉ gợi ý sản phẩm thay thế nếu replacement cũng pass guard.

## 8. Chương trình 24/7

### 8.1. Mục tiêu

24/7 là chương trình giá vận hành xuyên suốt, tự động xác định mức giảm theo lượng bán thành công của SKU ngày trước đó.

### 8.2. Rule giá

Policy canonical: `PRICE_POLICY_V2026_06_CANONICAL_001`.

| Điều kiện | Discount |
| --- | ---: |
| SKU lần đầu mở bán, chưa có lịch sử | 9% |
| `successful_sales_count_previous_day < 50` | 9% |
| `successful_sales_count_previous_day >= 50` | 5% |

`successful_sales_count` chỉ tính order hợp lệ, không tính quote/cart/draft/cancel/refund/test/duplicate.

### 8.3. Member benefit trong 24/7

Member có thể được hưởng quyền lợi theo hạng nếu Runtime Core cho phép. Không tự động cộng với Diamond buyer benefit. Nếu khách đủ nhiều quyền lợi, `PolicyPriorityResolver` quyết định.

### 8.4. Buyer từ link Diamond trong 24/7

Buyer qua link Diamond hợp lệ có thể được giảm 5% nếu:

- Link hợp lệ.
- Bind hợp lệ.
- Đơn phát sinh qua link.
- Runtime Core cho phép.
- Policy priority cho phép.

## 9. Giờ Vàng Tri Ân

### 9.1. Phiên

Policy canonical: `GOLDEN_HOUR_POLICY_V2026_06_CANONICAL_001`.

| Phiên | Thời gian |
| --- | --- |
| Phiên 1 | 12h00 - 12h45 |
| Phiên 2 | 20h00 - 20h45 |

Tối đa 2 phiên/ngày.

### 9.2. Mở phiên theo tồn kho

| Sellable stock SKU | Rule |
| ---: | --- |
| < 300 | Không mở Giờ Vàng cho SKU đó |
| 300 đến dưới 500 | Mở 1 phiên tối 20h00 - 20h45 |
| >= 500 | Mở 2 phiên 12h00 - 12h45 và 20h00 - 20h45 |

### 9.3. Quota

- `session_quota = 2.000 sản phẩm / phiên`.
- `session_sku_quota <= sellable_stock`.
- Không bán vượt tồn khả dụng.
- Phiên kết thúc khi hết thời gian, đủ quota, SKU hết stock, sale lock, quality hold, recall hold hoặc owner runtime dừng phiên.

### 9.4. IVR extension

Sau khi khách đặt hàng trong Giờ Vàng, nếu IVR required, runtime có thể giữ thêm 5 phút cho xác nhận IVR theo policy. Reservation không được giữ vô hạn. Fail/timeout phải release quota nếu policy yêu cầu.

## 10. Quyền mua sớm

| Đối tượng | Quyền mua sớm |
| --- | ---: |
| Silver | 5 phút |
| Gold | 10 phút |
| Platinum | 20 phút |
| Diamond | 30 phút |
| Buyer từ link Diamond hợp lệ | 5 phút |

### 10.1. Phiên 1

| Đối tượng | Thời điểm được vào |
| --- | --- |
| Diamond | 11h30 |
| Platinum | 11h40 |
| Gold | 11h50 |
| Silver | 11h55 |
| Buyer từ link Diamond | 11h55 |
| Khách thường | 12h00 |

### 10.2. Phiên 2

| Đối tượng | Thời điểm được vào |
| --- | --- |
| Diamond | 19h30 |
| Platinum | 19h40 |
| Gold | 19h50 |
| Silver | 19h55 |
| Buyer từ link Diamond | 19h55 |
| Khách thường | 20h00 |

Không hiển thị quyền vào sớm nếu runtime chưa xác nhận đủ điều kiện.

## 11. Không cộng dồn quyền lợi

Không tự cộng:

- Member benefit + Diamond buyer benefit.
- 24/7 benefit + Golden Hour benefit.
- Program discount + quyền lợi khác ngoài policy.

Nếu khách có nhiều quyền lợi, QuoteSnapshot phải ghi:

- Quyền lợi nào được xét.
- Quyền lợi nào được áp dụng.
- Quyền lợi nào bị từ chối.
- Reason code.
- Policy version.

## 12. QuoteSnapshot bắt buộc

Mọi báo giá phải từ QuoteSnapshot.

QuoteSnapshot tối thiểu có:

- SKU.
- Giá niêm yết.
- Program type.
- Program discount.
- Program price.
- Member benefit nếu có.
- Diamond buyer benefit nếu có.
- Shipping fee.
- COD fee.
- VAT.
- Final total.
- Quote created at.
- Quote expires at.
- Attribution nếu có.
- Policy priority decision.

Không có QuoteSnapshot thì không báo final price.

## 13. API/DTO/entity đề xuất

| Loại | Tên đề xuất | Nội dung |
| --- | --- | --- |
| DTO | `MemberLifecycleSnapshot` | Hạng, chu kỳ, doanh số, maintain, upgrade, grace. |
| DTO | `MemberBenefitDecision` | Benefit apply/deny theo program và policy. |
| DTO | `ProgramPriceDecision` | Listed price, discount, program price. |
| DTO | `GoldenHourSessionDecision` | Session, quota, stock, early access, status. |
| Entity | `member_tier_history` | Audit hạng và policy version. |
| Entity | `member_revenue_ledger` | Doanh số hợp lệ theo order verified. |
| Entity | `golden_hour_session` | Phiên, quota, stock snapshot. |
| Entity | `quote_policy_breakdown` | Benefit breakdown trong QuoteSnapshot. |

Tên thật phải theo codebase hiện có sau analysis.

## 14. P0 blocker

- Member revenue lấy từ quote/cart/draft/unpaid.
- Maintain không theo 50% chuẩn hạng.
- Không có grace 60 ngày.
- Benefit hardcode ngoài runtime.
- 24/7 threshold sai mốc 50.
- Golden Hour mở khi stock dưới 300.
- Early access hiển thị sai hạng.
- Quote không có QuoteSnapshot.
- Policy priority thiếu khi nhiều quyền lợi.

## 15. Smoke tối thiểu

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P3_1-2A-001 | Silver có đủ runtime | Benefit 5% nếu program allow. |
| P3_1-2A-002 | Gold hết chu kỳ chưa đạt 50% | Grace 60 ngày, chưa downgrade. |
| P3_1-2A-003 | Hết grace vẫn chưa đạt | Downgrade an toàn theo runtime. |
| P3_1-2A-004 | 24/7 sales count 49 | Discount ngày sau 9%. |
| P3_1-2A-005 | 24/7 sales count 50 | Discount ngày sau 5%. |
| P3_1-2A-006 | Golden Hour stock 299 | Không mở phiên. |
| P3_1-2A-007 | Golden Hour stock 300 | Mở 1 phiên tối 20h00 - 20h45. |
| P3_1-2A-008 | Diamond phiên 2 | Early access 19h30 nếu runtime confirmed. |
| P3_1-2A-009 | Member + Diamond buyer benefit | Policy priority quyết định, không tự cộng. |
| P3_1-2A-010 | Missing QuoteSnapshot | Không báo final price. |

## 16. Evidence bắt buộc

- MemberLifecycleSnapshot sample đã mask PII.
- MemberBenefitDecision log.
- ProgramPriceDecision log.
- GoldenHourSessionDecision log.
- QuoteSnapshot sample.
- Policy priority decision.
- Audit/correlation id.
- Smoke output.

## 17. Done gate

Workstream 02 DONE khi:

- Member, price, 24/7, Golden Hour, early access và QuoteSnapshot đã map vào runtime.
- Không còn hardcode policy.
- Smoke tối thiểu có kết quả.
- Evidence đã được gom.
- Không release claim.

## 18. Final status

`P3.1-2A HOÀN TẤT BÀN GIAO - CHỜ TRIỂN KHAI/BẰNG CHỨNG - GATEWAY BLOCKED`

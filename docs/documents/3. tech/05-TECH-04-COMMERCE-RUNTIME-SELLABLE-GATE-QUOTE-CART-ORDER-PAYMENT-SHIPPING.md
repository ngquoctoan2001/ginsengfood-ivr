# TECH-04 - COMMERCE RUNTIME SELLABLE GATE QUOTE CART ORDER PAYMENT SHIPPING

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## PRICE PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / CUSTOMER CONFIRMATION / ORDER VERIFICATION

## PHẦN 1/4 - COMMERCE RUNTIME PRINCIPLES / SOURCE-OF-TRUTH / QUOTE-ORDER-PAYMENT BOUNDARY / NO-BYPASS RULE

## 1. MỤC TIÊU CỦA TECH-04

## 1.1. Vai trò của TECH-04

TECH-04 là tài liệu kỹ thuật nền cho tầng Commerce Runtime của Ginsengfood.

Commerce Runtime là lớp chịu trách nhiệm quản trị sự thật thương mại tại thời điểm bán hàng, bao gồm:

Sản phẩm có được xét bán hay không.

Giá niêm yết có hiệu lực hay không.

Chương trình giá nào đang áp dụng.

Thành viên có được hưởng quyền lợi hay không.

Người mua qua link Diamond có được hưởng quyền lợi hay không.

Chính sách ưu tiên quyền lợi nào được áp dụng.

QuoteSnapshot nào là nguồn báo giá duy nhất.

Cart / Order Draft nào đang hợp lệ.

Khách đã xác nhận đơn hay chưa.

Thanh toán đã được xác nhận hay chưa.

Đơn đã được xác minh doanh thu hay chưa.

Đơn có đủ điều kiện bàn giao fulfillment, MISA, KPI, hoa hồng, Ads/ROAS hay chưa.

TECH-04 không phải tài liệu AI bán hàng, không phải tài liệu Ads, không phải tài liệu Live, không phải tài liệu MISA chi tiết và không phải tài liệu vận hành kho/sản xuất.

TECH-04 là lớp khóa runtime thương mại giữa:

Operational Core -> Commerce Runtime -> AI / Facebook Gateway / Ads / Live / CRM / Payment / Fulfillment / MISA / KPI / Evidence.

## 1.2. TECH-04 nằm ở đâu trong chuỗi TECH

TECH-04 nằm sau:

## TECH-00 - Technical Implementation Master Plan.

## TECH-01 - Foundation / RBAC / Audit / Idempotency / Evidence Registry.

## TECH-02 - Product / SKU / Ingredient / Recipe / Formula / Product Activation.

## TECH-03 - Operational Core / Production / Warehouse / Inventory / Traceability / Recall / Sale Lock.

TECH-04 nằm trước:

## TECH-05 - AI Advisor Runtime.

## TECH-06 - Facebook Gateway / Meta Live & Messenger.

## TECH-07 - Ads Measurement.

## TECH-08 - MC AI Live.

## TECH-09 - IVR Order Confirmation.

## TECH-10 - Completion / Evidence / Gateway / Release Readiness.

Nguyên tắc bắt buộc:

Không có TECH-03 Operational Sellable Signal hợp lệ thì TECH-04 không được quote/order production.

## 2. PHẠM VI CỦA TECH-04

## 2.1. TECH-04 bao gồm

TECH-04 khóa các domain chính sau:

Commerce Runtime Boundary.

Sellable Gate.

Listed Price Governance.

Program Price Governance.

Member Tier / Member Benefit Eligibility.

Diamond Referral Benefit Eligibility.

Policy Priority Decision.

QuoteSnapshot.

Quote Hold Window.

Cart Runtime.

Order Draft.

Customer Confirmation.

Official Order Creation.

Payment Method Selection.

Payment Confirmation.

Direct Bank Transfer Reference.

COD Payment Method.

Shipping Fee / Shipping Eligibility.

VAT / Invoice Request Boundary.

Order Verification.

Revenue Verification.

Commission / KPI / Ads ROAS Handoff.

Commerce Suppression by Operational Recall / Sale Lock.

Commerce Handoff sang AI, Facebook Gateway, Ads, Live, CRM, MISA, IVR nếu scope có.

## 2.2. TECH-04 không bao gồm

TECH-04 không thiết kế chi tiết:

API endpoint.

Database schema.

UI screen.

Component frontend.

Code backend.

Worker code.

Payment gateway code.

MISA API chi tiết.

AI message templates chi tiết.

Ads dashboard chi tiết.

IVR call script chi tiết.

Warehouse fulfillment chi tiết.

Các phần trên chỉ được triển khai ở TECH downstream hoặc DEV phase sau khi TECH-04 khóa boundary.

## 3. NGUYÊN TẮC GREENFIELD CHO COMMERCE RUNTIME

## 3.1. Tài liệu mới là nguồn đúng

Commerce Runtime được xây theo hướng GREENFIELD TECHNICAL RESET.

Điều này có nghĩa:

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

Code cũ chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

TECH-00 -> TECH-04 là SOURCE_OF_TRUTH cho Commerce Runtime.

Nếu code cũ khác TECH-04 thì TECH-04 thắng.

Dev không được lấy logic giá, quyền lợi, order hoặc payment từ code cũ để override TECH-04.

Legacy chỉ dùng để gap analysis sau khi TECH-04 đã khóa.

## 3.2. Không code trước khi khóa mapping

Commerce Runtime có rủi ro cao vì ảnh hưởng trực tiếp đến:

Giá bán.

Quyền lợi thành viên.

Chương trình giá.

Quote cho khách.

Xác nhận đơn.

Thanh toán.

Doanh thu xác minh.

Hoa hồng Diamond.

Ads ROAS.

## KPI.

## MISA.

Khiếu nại giá.

Vì vậy không được đi thẳng vào code khi chưa có:

Module map.

Dependency map.

Runtime decision map.

Price policy map.

QuoteSnapshot contract.

Order state model.

Payment state model.

P0 điểm chặn.

Evidence requirement.

Smoke matrix.

Owner review.

Release handoff.

## 4. SOURCE-OF-TRUTH CỦA COMMERCE RUNTIME

## 4.1. Source-of-truth cấp tài liệu

Commerce Runtime phải tuân theo thứ tự nguồn đúng:

Cấp

Nguồn

Vai trò

MASTER / PACK đã khóa

Governance cấp cao

## TECH-00

Kế hoạch triển khai kỹ thuật tổng thể

## TECH-01

Permission, audit, idempotency, evidence

## TECH-02

Product, SKU, recipe, product scope

## TECH-03

Operational sellable, stock, recall, sale lock

## TECH-04

Commerce source-of-truth

TECH-05 trở đi

Downstream consumer

Code hiện tại

Current state để gap analysis

Tài liệu cũ

Legacy reference only

## 4.2. Source-of-truth cấp runtime

Loại dữ liệu

Owner đúng

Không được lấy từ

Product/SKU active

Product Domain - TECH-02

AI, Ads, Commerce tự tạo

Operational sellable / stock / recall / sale lock

Operational Core - TECH-03

AI, CRM, Ads, manual content

Listed price active

Commerce Runtime

AI, Live, file rời

Program eligibility

Commerce Runtime

AI tự suy luận

Member tier / benefit eligibility

Commerce Runtime hoặc Member Runtime được Commerce kiểm soát

AI hỏi khách tự khai

Diamond referral eligibility

Commerce Runtime / Referral Resolver

Link text không bind

Final quote price

QuoteSnapshot

AI tự tính

Cart / Order Draft

Commerce Runtime

Messenger text rời

Customer Confirmation

Commerce Runtime

Dev/AI tự xem là đã xác nhận

Payment confirmed

Payment Core / Accounting Review

Ảnh chuyển khoản chưa xác minh

Paid status

Payment Core hoặc Accounting Review

AI, CRM, Gateway

Verified revenue

Commerce / Finance verification

Order draft hoặc quote

Commission eligible

Commerce / Reward policy after verified order

Referral click chưa có đơn hợp lệ

Ads ROAS revenue

Verified revenue only

Quote, draft order, unpaid order

## 5. VAI TRÒ CỦA COMMERCE RUNTIME

## 5.1. Commerce Runtime là lõi quyết định thương mại

Commerce Runtime không chỉ là giỏ hàng hoặc đơn hàng.

Commerce Runtime là nơi xác định:

Có được bán SKU đó tại thời điểm hiện tại không.

Giá niêm yết nào đang active.

Chương trình giá nào đang active.

Thành viên có được áp quyền lợi không.

Người mua qua link Diamond có được giảm thêm không.

Quyền lợi nào được ưu tiên.

Giá cuối cùng khách phải thanh toán là bao nhiêu.

Quote giữ giá đến thời điểm nào.

Khách đã xác nhận đơn chưa.

Đơn đã chính thức được tạo chưa.

Thanh toán đã được xác nhận chưa.

Doanh thu đã được verified chưa.

Đơn có được tính vào KPI/commission/Ads ROAS chưa.

## 5.2. Commerce Runtime không thay Operational Core

Commerce Runtime không được tự quyết định:

Batch đã release hay chưa.

Kho đã nhận thành phẩm hay chưa.

Tồn kho thành phẩm có thật hay chưa.

SKU có bị recall không.

SKU có bị sale lock không.

QR có public trace valid hay không.

Batch nào có trace chain đủ hay không.

Các phần đó thuộc TECH-03 Operational Core.

Commerce chỉ được quote/order khi Operational Core trả tín hiệu pass.

## 5.3. Commerce Runtime không thay AI Advisor

Commerce Runtime không viết lời tư vấn bán hàng.

AI Advisor dùng dữ liệu từ Commerce Runtime để nói với khách, nhưng AI không được tự tính:

Giá cuối.

Chương trình đang áp dụng.

Quyền lợi thành viên.

Quyền lợi link Diamond.

Thời hạn giữ giá.

Phí ship.

Trạng thái thanh toán.

Order code.

Paid status.

AI chỉ được dùng kết quả đã được Commerce Runtime cung cấp.

## 5.4. Commerce Runtime không thay Payment Core / Accounting Review

Commerce Runtime có thể quản trị order và payment state, nhưng không được tự gắn PAID nếu chưa có xác nhận hợp lệ từ:

Payment Gateway.

Payment Core.

Accounting Review đối với chuyển khoản trực tiếp.

Payment reconciliation hợp lệ.

Ảnh chuyển khoản, lời khách nói “em chuyển rồi”, hoặc tin nhắn tự khai không đủ để gắn PAID.

## 5.5. Commerce Runtime không thay MISA

Commerce Runtime không ghi sổ kế toán thay MISA.

Commerce Runtime chỉ bàn giao dữ liệu đủ điều kiện cho MISA hoặc Finance layer nếu scope có:

Order verified.

Payment confirmed.

Revenue verified.

Shipping/payment/tax fields theo rule.

Commission/KPI eligibility nếu đã đạt điều kiện.

MISA missing mapping không được xem là sync success.

## 6. CHUỖI COMMERCE RUNTIME CHUẨN

## 6.1. Chuỗi tổng thể bắt buộc

Chuỗi Commerce Runtime chuẩn:

Product Scope Check -> Operational Sellable Check -> Listed Price Check -> Program Eligibility Check -> Member Benefit Eligibility Check -> Diamond Referral Eligibility Check -> Policy Priority Decision -> QuoteSnapshot -> Cart / Order Draft -> Customer Review -> Payment Method Selection -> Customer Confirmation -> Official Order Creation -> Payment WAITING -> Payment Confirmation / Accounting Review -> Order Verified -> Revenue Verified -> Fulfillment / MISA / KPI / Commission / Ads ROAS Handoff.

Không downstream nào được bỏ qua chuỗi này để tạo doanh thu hợp lệ.

## 6.2. Nguyên tắc không nhảy cóc

Không được:

Product Active -> quote ngay.

SKU Active -> order ngay.

Operational stock -> final price ngay nếu chưa có Commerce policy.

Giá niêm yết -> final price nếu chưa qua program/member/referral policy.

Quote -> official order nếu khách chưa xác nhận.

Order draft -> order_code nếu chưa CustomerConfirmation.

Ảnh chuyển khoản -> PAID nếu chưa xác minh.

Paid -> revenue verified nếu chưa qua order verification rule.

Order verified -> commission payout nếu policy chưa pass.

Quote/order chưa paid -> Ads ROAS revenue.

AI nói “đơn đã ghi nhận” khi chưa có official order.

## 7. BOUNDARY GIỮA OPERATIONAL SELLABLE VÀ COMMERCE SELLABLE

## 7.1. Operational Sellable Signal

Operational Core trả lời:

Batch đã release chưa.

Kho đã nhận chưa.

Tồn khả dụng có không.

Có recall không.

Có sale lock không.

Có quality hold không.

Trace/public trace có pass không nếu policy yêu cầu.

Operational Sellable Signal là điều kiện đầu vào.

## 7.2. Commerce Sellable Decision

Commerce Runtime trả lời:

SKU có được bán trên kênh này không.

Giá niêm yết có active không.

Chương trình có active không.

Quote policy có cho phép không.

Customer/Member/Referral benefit có pass không.

Order/channel/payment/shipping có hợp lệ không.

Có đủ điều kiện tạo quote/order không.

## 7.3. Nguyên tắc khóa

Operational Sellable Signal pass chưa đủ để bán.

Commerce Sellable Decision pass mới được quote/order.

Nhưng:

Commerce Sellable Decision không được pass nếu Operational Sellable Signal bị chặn.

## 8. BOUNDARY GIỮA QUOTE, CART, ORDER DRAFT VÀ OFFICIAL ORDER

## 8.1. Quote

Quote là báo giá có thời hạn.

Quote phải được tạo từ QuoteSnapshot.

Quote không phải official order.

Quote không đồng nghĩa khách đã mua.

Quote không đồng nghĩa doanh thu.

## 8.2. Cart

Cart là tập hợp sản phẩm khách đang cân nhắc hoặc chuẩn bị mua.

Cart không phải official order.

Cart không được dùng làm revenue.

Cart không được dùng làm commission.

Cart không được dùng làm Ads ROAS verified revenue.

## 8.3. Order Draft

Order Draft là bản tạm tính / xác nhận đơn nháp.

Order Draft phải có đủ tối thiểu:

Thông tin nhận hàng.

Phương án thanh toán.

Phần xác nhận đơn hàng rõ ràng.

Order Draft chưa phải official order.

Order Draft không có order_code chính thức nếu khách chưa xác nhận.

## 8.4. Official Order

Official Order chỉ được tạo khi:

QuoteSnapshot còn hiệu lực hoặc được refresh hợp lệ.

Sản phẩm vẫn sellable.

Order Draft đủ thông tin bắt buộc.

Khách đã xác nhận.

Commerce Runtime ghi nhận official order hợp lệ.

Chỉ sau bước này mới được trả order_code.

## 8.5. Rule khóa

Không có CustomerConfirmation thì không có official order.

Không có official order thì không có order_code.

Không có payment confirmation thì không có PAID.

Không có order verification thì không có verified revenue.

## 9. QUOTESNAPSHOT LÀ NGUỒN BÁO GIÁ DUY NHẤT

## 9.1. Vai trò của QuoteSnapshot

QuoteSnapshot là bản ghi khóa toàn bộ dữ liệu báo giá tại thời điểm quote.

QuoteSnapshot là nguồn duy nhất để AI, Messenger, Web, Landing Page, Live, Facebook Gateway hoặc CRM hiển thị giá cho khách.

Không module nào được tự tính lại giá cuối cùng ngoài QuoteSnapshot.

## 9.2. QuoteSnapshot phải khóa tối thiểu

QuoteSnapshot phải khóa:

## SKU.

Public product name.

Quantity.

Listed price.

Listed price status.

Current program.

Program discount.

Program price.

Member tier nếu runtime xác nhận.

Member discount nếu eligible.

Diamond referral benefit nếu eligible.

Policy priority decision.

Shipping fee nếu có.

COD fee nếu policy có; hiện tại không tự thêm phí COD nếu chưa khóa chính sách.

VAT display nếu scope có.

Final total.

Quote created time.

Quote expires at.

Quote channel.

Customer identity context.

Runtime decision references.

Blocking reason nếu quote không được tạo.

## 9.3. Quote hold window

Quote phải có thời hạn giữ giá.

Quy tắc đã khóa:

Chương trình

Thời hạn giữ giá

Giờ Vàng

5 phút

24/7

15 phút

Sau thời hạn này:

Quote cũ không còn đảm bảo giá.

Muốn chốt phải refresh quote theo runtime hiện tại.

AI không được dùng giá cũ để tạo official order.

CustomerConfirmation sau khi quote hết hạn không được tự động tạo order.

## 9.4. Rule khóa

AI không tự tính giá.

Live không tự tính giá.

Gateway không tự tính giá.

CRM không tự tính giá.

QuoteSnapshot là nguồn báo giá duy nhất.

## 10. PRICE / PROGRAM / MEMBER / REFERRAL GOVERNANCE

## 10.1. Listed Price

Listed Price là giá niêm yết active tại runtime.

Không được quote nếu:

Listed price missing.

Listed price inactive.

Listed price expired.

Listed price conflict.

Listed price chưa được owner approved.

Runtime không xác minh được listed price.

## 10.2. Program Price

Program Price là giá sau chương trình.

Chương trình có thể bao gồm:

24/7.

Giờ Vàng Tri Ân.

Chương trình khác nếu future scope được owner approved.

Commerce Runtime phải xác định chương trình hiện hành từ runtime, không từ AI hoặc nội dung livestream.

## 10.3. Member Benefit

Member Benefit chỉ áp dụng khi runtime xác nhận:

Khách là member.

Hạng member hợp lệ.

Chu kỳ member còn hiệu lực hoặc grace policy pass.

SKU/chương trình cho phép áp quyền lợi.

Không bị policy conflict.

Commerce quyết định eligible.

AI không được hỏi khách tự khai “mình hạng gì” để tính giá.

## 10.4. Diamond Referral Benefit

Diamond Referral Benefit chỉ áp dụng khi resolver/runtime xác nhận:

Referral link hợp lệ.

Referral bind hợp lệ.

Referral owner đủ điều kiện.

Buyer_from_diamond_link pass.

Chương trình cho phép quyền lợi buyer.

Policy priority pass.

Không có bind hợp lệ thì không áp Diamond referral benefit.

## 10.5. Policy Priority Decision

Commerce Runtime phải có quyết định ưu tiên chính sách khi có nhiều quyền lợi cùng lúc.

Không được cộng dồn tùy tiện.

Không được để AI hoặc Gateway tự chọn quyền lợi có lợi nhất.

Không được áp member benefit, program benefit, Diamond referral benefit cùng lúc nếu policy runtime không cho phép.

## 11. CUSTOMER CONFIRMATION RULE

## 11.1. Khách phải xác nhận sau khi xem Order Draft

Sau khi khách nói “mua”, “chốt”, “lấy”, “đồng ý”, hệ thống không được tạo official order ngay nếu chưa có Order Draft và CustomerConfirmation hợp lệ.

Luồng đúng:

Commerce tạo QuoteSnapshot.

Commerce tạo Order Draft.

Order Draft hiển thị thông tin nhận hàng.

Order Draft hiển thị phương án thanh toán.

Order Draft hiển thị tổng tiền và thời hạn quote.

Khách kiểm tra.

Khách xác nhận bằng nút/CTA hoặc hành động xác nhận hợp lệ.

Commerce mới tạo Official Order.

Hệ thống mới trả order_code.

## 11.2. Kênh có UI phải ưu tiên CTA

Messenger/Web/Landing Page nếu có UI nên ưu tiên:

Nút “Xác nhận đơn”.

CTA xác nhận hợp lệ.

Form xác nhận hợp lệ.

Kênh chỉ-text mới fallback bằng câu yêu cầu khách nhắn xác nhận.

## 11.3. Rule khóa

Khách nói “mua” chưa đủ để tạo official order.

Khách phải xác nhận Order Draft còn hiệu lực.

## 12. PAYMENT BOUNDARY

## 12.1. Payment Method Selection

Khách có thể chọn phương thức thanh toán theo policy được bật, ví dụ:

## VNPAY.

## MOMO_BUSINESS.

## DIRECT_BANK_TRANSFER_TO_SSAVIGROUP.

COD nếu policy/kênh cho phép.

Commerce Runtime không được hardcode thông tin tài khoản ngân hàng trong AI, Gateway, frontend static template hoặc nội dung chat.

Thông tin tài khoản chuyển khoản phải đến từ Company Bank Account Registry nếu scope chuyển khoản trực tiếp được bật.

## 12.2. Direct Bank Transfer

Với chuyển khoản trực tiếp:

Phải có payment reference hoặc transfer memo.

Khách cần được hướng dẫn dùng đúng nội dung chuyển khoản theo Commerce Runtime.

Không được gắn PAID chỉ vì khách gửi ảnh chuyển khoản.

Cần Payment Core hoặc Accounting Review xác nhận.

Nếu ngân hàng/API chưa tích hợp, phải có quy trình kế toán đối soát.

Thiếu đối soát thì payment vẫn WAITING/review, không PAID.

## 12.3. COD

COD là phương án:

Nhận hàng rồi thanh toán.

Không tự thêm dòng phí COD nếu runtime/chính sách chưa khóa có phí COD.

Order Draft chỉ có phí ship nếu có.

## 12.4. PAID status

Chỉ được gắn PAID khi có:

Payment gateway confirmation hợp lệ.

Payment Core confirmation hợp lệ.

Accounting Review confirmation hợp lệ đối với chuyển khoản.

Reconciliation pass.

Không có xác nhận hợp lệ thì không PAID.

## 13. SHIPPING / DELIVERY BOUNDARY

## 13.1. Shipping là điều kiện thương mại

Shipping thuộc Commerce Runtime hoặc Shipping Runtime được Commerce kiểm soát.

Shipping phải xác định:

Địa chỉ giao hàng.

Khu vực giao hàng.

Phí ship nếu có.

Phương thức giao nếu scope có.

Điều kiện COD nếu có.

Trạng thái đủ điều kiện giao hàng.

Thời gian dự kiến nếu policy có.

## 13.2. Domestic default

Mặc định shipping là nội địa.

International shipping/payment chỉ là future governed extension nếu owner approved.

Không được tự bật international order/payment nếu chưa có policy.

## 13.3. Shipping fee

Order Draft chỉ hiển thị phí ship nếu có.

Không được tự tạo phí không có policy.

Không được cộng thêm phí COD nếu chưa có rule.

## 14. ORDER VERIFICATION / REVENUE VERIFICATION

## 14.1. Official Order chưa chắc là verified revenue

Official Order là đơn đã được tạo sau CustomerConfirmation.

Nhưng Official Order chưa chắc đã là doanh thu verified.

Doanh thu verified chỉ có khi:

Order hợp lệ.

Payment/collection status hợp lệ.

Fulfillment/verification rule pass nếu scope có.

Không bị hủy/refund/invalid.

Finance/Commerce verification pass.

## 14.2. Verified Revenue là nguồn cho Ads/KPI/Commission

Chỉ Verified Revenue mới được dùng cho:

Ads ROAS.

## KPI.

Commission Diamond.

Financial reporting.

Reward calculation.

Owner performance review.

Không dùng:

Quote.

Cart.

Order Draft.

Unpaid order.

Payment WAITING.

Canceled order.

Failed COD.

Unverified order.

## 15. NO-BYPASS RULE CHO COMMERCE RUNTIME

## 15.1. Không bypass Product

Commerce không được bán SKU/Product chưa hợp lệ theo TECH-02.

Không được tự tạo SKU bán hàng.

Không được tự sửa product scope.

Không được tự dùng tên sản phẩm public nếu Product Knowledge chưa approved.

## 15.2. Không bypass Operational

Commerce không được quote/order nếu:

Batch chưa RELEASED.

Warehouse chưa receipt.

Không có stock khả dụng.

Recall active.

Sale Lock active.

Quality hold active.

Operational Runtime unavailable.

Trace chain missing nếu policy yêu cầu.

Product/SKU không thuộc release scope.

## 15.3. Không bypass QuoteSnapshot

AI/Gateway/Live/CRM không được tự tính giá.

Commerce không được tạo order từ giá text rời nếu không có QuoteSnapshot còn hiệu lực hoặc refresh hợp lệ.

## 15.4. Không bypass Customer Confirmation

Không tạo official order khi chưa có:

Không trả order_code trước khi official order được tạo.

## 15.5. Không bypass Payment

Không gắn PAID nếu chưa có payment confirmation hợp lệ.

Không xem ảnh chuyển khoản là PAID.

Không xem COD order là paid trước khi thu tiền hoặc verification policy pass.

## 15.6. Không bypass Revenue Verification

Không đưa order vào Ads ROAS/KPI/Commission nếu chưa verified revenue.

Không trả commission từ order chưa đủ điều kiện.

## 16. P0 COMMERCE RULES NỀN

## 16.1. P0-COM-001 - Operational bị chặn thì Commerce bị chặn

Nếu Operational Core trả bị chặn, Commerce không được quote/order.

Fail Gate:

Nếu Commerce quote/order khi Operational Sale Lock/Recall/No Stock active, TECH-04 FAIL.

## 16.2. P0-COM-002 - Product Active không đồng nghĩa Sellable

Product Active/SKU Active chỉ là điều kiện Product Domain.

Không được dùng Product Active/SKU Active để bán nếu chưa qua Operational và Commerce Runtime.

Fail Gate:

Nếu Product Active tự thành orderable, TECH-04 FAIL.

## 16.3. P0-COM-003 - No QuoteSnapshot thì không final price

Không có QuoteSnapshot thì không được báo giá cuối cùng.

Fail Gate:

Nếu AI/Live/Gateway/CRM báo final price không từ QuoteSnapshot, TECH-04 FAIL.

## 16.4. P0-COM-004 - Quote hết hạn thì không tạo official order

Quote hết hạn phải refresh.

Fail Gate:

Nếu CustomerConfirmation dùng quote hết hạn để tạo order, TECH-04 FAIL.

## 16.5. P0-COM-005 - Không CustomerConfirmation thì không official order

Order Draft chưa được xác nhận không phải official order.

Fail Gate:

Nếu hệ thống tạo order_code khi CustomerConfirmation chưa CONFIRMED, TECH-04 FAIL.

## 16.6. P0-COM-006 - Không payment confirmation thì không PAID

Không được gắn PAID nếu chưa có xác nhận hợp lệ.

Fail Gate:

Nếu ảnh chuyển khoản hoặc lời khách nói được gắn PAID, TECH-04 FAIL.

## 16.7. P0-COM-007 - Không Order Verified thì không Verified Revenue

Không dùng order chưa verified làm doanh thu chính thức.

Fail Gate:

Nếu quote/draft/unpaid order được đưa vào Ads ROAS/KPI/commission, TECH-04 FAIL.

## 16.8. P0-COM-008 - Không cộng dồn quyền lợi tùy tiện

Commerce Runtime phải quyết định policy priority.

Fail Gate:

Nếu AI/Gateway tự cộng member discount + program discount + referral benefit không qua runtime policy, TECH-04 FAIL.

## 16.9. P0-COM-009 - Không hardcode thông tin ngân hàng

Không hardcode tài khoản ngân hàng trong AI, Gateway, frontend static, template chat, nội dung live.

Fail Gate:

Nếu thông tin ngân hàng không lấy từ Company Bank Account Registry, TECH-04 FAIL.

## 16.10. P0-COM-010 - Sale Lock / Recall thắng Commerce

Recall/Sale Lock phải chặn quote/order dù giá/chương trình/member đều pass.

Fail Gate:

Nếu sản phẩm recall/sale lock vẫn tạo order, TECH-04 FAIL.

## 17. RUNTIME UNAVAILABLE RULE CHO COMMERCE

## 17.1. Khi runtime không khả dụng

Nếu bất kỳ runtime quan trọng nào không khả dụng, Commerce phải fail-safe.

Không được suy đoán.

Không được fallback sang dữ liệu cũ nếu không có policy rõ.

Không được dùng cache để vượt recall, sale lock hoặc giá.

## 17.2. Runtime quan trọng

Runtime

Nếu unavailable thì

Product Runtime

Không quote/order SKU chưa xác minh

Operational Runtime

Không quote/order production

Listed Price Runtime

Không báo giá cuối

Program Runtime

Không áp chương trình

Member Runtime

Không áp quyền lợi member

Diamond Referral Resolver

Không áp quyền lợi link Diamond

Quote Runtime

Không tạo quote/order từ dữ liệu rời

Payment Runtime

Không PAID

Shipping Runtime

Không xác nhận phí/đủ điều kiện giao

Evidence Runtime

Không Completion Decision / Release Ready

MISA Runtime

WAITING/reconcile, không success

Recall/Sale Lock Runtime

Fail-safe bị chặn

## 18. EVIDENCE NGUYÊN TẮC CHO PHẦN 1

## 18.1. Evidence bắt buộc cấp nguyên tắc

Evidence ID

Nội dung

Trạng thái yêu cầu

## COM-EVD-P1-001

Commerce Boundary Approval

## ACCEPTED

## COM-EVD-P1-002

Operational Dependency Approval

## ACCEPTED

## COM-EVD-P1-003

QuoteSnapshot Source-of-Truth Approval

## ACCEPTED

## COM-EVD-P1-004

CustomerConfirmation Rule Approval

## ACCEPTED

## COM-EVD-P1-005

Payment Boundary Approval

## ACCEPTED

## COM-EVD-P1-006

Verified Revenue Rule Approval

## ACCEPTED

## COM-EVD-P1-007

No-Hardcode Bank Rule Approval

## ACCEPTED

## COM-EVD-P1-008

Runtime Unavailable Fail-Safe Approval

## ACCEPTED

## COM-EVD-P1-009

Policy Priority Decision Approval

## ACCEPTED

## COM-EVD-P1-010

Downstream No-Bypass Approval

## ACCEPTED

## 18.2. Evidence chưa accepted thì không PASS

Nguyên tắc dựa trên:

Evidence DRAFT không dùng để PASS.

Evidence SUBMITTED không dùng để PASS.

Evidence chưa owner/reviewer không dùng để PASS.

Evidence không mapping requirement không dùng để PASS.

Evidence không environment/version không dùng để PASS nếu cần release gate.

Chỉ evidence ACCEPTED mới được dùng để PASS.

## 19. SMOKE NGUYÊN TẮC CHO PHẦN 1

## 19.1. Smoke cấp nguyên tắc

Smoke ID

Nội dung smoke

Expected Result

## COM-SMK-P1-001

Product Active nhưng Operational bị chặn

Không quote/order

## COM-SMK-P1-002

Operational pass nhưng missing listed price

Không final quote

## COM-SMK-P1-003

Listed price pass nhưng no QuoteSnapshot

Không báo final price

## COM-SMK-P1-004

Quote hết hạn

Không tạo official order

## COM-SMK-P1-005

Order Draft chưa CustomerConfirmation

Không order_code

## COM-SMK-P1-006

CustomerConfirmation confirmed

Được xét tạo official order

## COM-SMK-P1-007

Khách gửi ảnh chuyển khoản

Không gắn PAID nếu chưa xác minh

## COM-SMK-P1-008

Payment confirmed hợp lệ

Được xét PAID

## COM-SMK-P1-009

Unpaid order

Không verified revenue

## COM-SMK-P1-010

Verified order

Được bàn giao Ads/KPI/commission nếu policy pass

## COM-SMK-P1-011

Diamond link không bind

Không áp referral benefit

## COM-SMK-P1-012

Member tier không runtime-confirmed

Không áp member benefit

## COM-SMK-P1-013

Sale Lock active

Commerce bị chặn

## COM-SMK-P1-014

Runtime unavailable

Fail-safe, không quote/order

## COM-SMK-P1-015

Direct bank transfer

Bank info lấy từ registry, không hardcode

## 20. DONE GATE CỦA PHẦN 1/4

## 20.1. Điều kiện Done Gate

## PHẦN 1/4 chỉ được xem là DONE khi:

Vai trò Commerce Runtime đã rõ.

TECH-04 đứng đúng sau TECH-03 và trước TECH-05.

Source-of-truth Commerce đã rõ.

Product / Operational / Commerce boundary đã rõ.

Operational Sellable và Commerce Sellable đã tách rõ.

Quote / Cart / Order Draft / Official Order đã tách rõ.

QuoteSnapshot là nguồn báo giá duy nhất đã khóa.

Quote hold window Giờ Vàng 5 phút, 24/7 15 phút đã khóa.

CustomerConfirmation rule đã khóa.

Payment boundary đã khóa.

No PAID without payment confirmation đã khóa.

No verified revenue without order verification đã khóa.

No hardcode bank information đã khóa.

Shipping/COD boundary đã khóa.

No-bypass rule đã khóa.

P0 Commerce Rules nền đã rõ.

Runtime unavailable fail-safe đã rõ.

Evidence requirement đã rõ.

Smoke nguyên tắc đã rõ.

Không gọi Documentation Complete là Production Ready.

## 21. FAIL GATE CỦA PHẦN 1/4

## 21.1. Điều kiện làm PHẦN 1 fail

## PHẦN 1/4 FAIL nếu có bất kỳ điểm nào sau:

Commerce được phép bán khi Operational bị chặn.

Product Active/SKU Active bị hiểu là Sellable.

AI/Gateway/Live/CRM được phép tự tính final price.

Không có QuoteSnapshot vẫn báo giá cuối.

Quote hết hạn vẫn tạo order.

Khách nói “mua” được xem là CustomerConfirmation.

Order Draft tự thành official order.

Chưa CustomerConfirmation vẫn có order_code.

Ảnh chuyển khoản được gắn PAID.

COD bị gắn paid trước verification.

Unpaid order được tính vào verified revenue.

Quote/order draft được tính Ads ROAS/KPI/commission.

Member benefit được áp khi runtime chưa xác nhận.

Diamond referral benefit được áp khi link chưa bind.

Quyền lợi bị cộng dồn tùy tiện.

Thông tin ngân hàng bị hardcode.

Runtime unavailable nhưng vẫn quote/order.

Evidence chưa accepted nhưng vẫn PASS.

## 22. TRẠNG THÁI SAU PHẦN 1/4

## 22.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## TECH-04 PHẦN 1/4

## WRITTEN_FOR_REVIEW

Commerce Runtime Principles

## LOCKED_DRAFT

Source-of-Truth Boundary

## LOCKED_DRAFT

QuoteSnapshot Rule

## LOCKED_DRAFT

CustomerConfirmation Rule

## LOCKED_DRAFT

Payment Boundary

## LOCKED_DRAFT

Verified Revenue Boundary

## LOCKED_DRAFT

Runtime Unavailable Rule

## DEFINED

Evidence Requirement

## DEFINED

Smoke Principle

## DEFINED

Implementation Status

## WAITING

Test Status

## WAITING

Smoke Status

## WAITING

Evidence Status

## WAITING

Release Ready

KHONG

Production Ready

KHONG

Go-live Approved

KHONG

## 23. KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 đã khóa tư duy nền của TECH-04:

Commerce Runtime là lớp quyết định sự thật thương mại tại thời điểm bán hàng.

Commerce không thay Product.

Commerce không thay Operational Core.

Commerce không thay AI.

Commerce không thay Payment Core.

Commerce không thay MISA.

Commerce có nhiệm vụ bảo vệ chuỗi thương mại thật:

Sellable đúng -> giá đúng -> chương trình đúng -> quyền lợi đúng -> quote đúng -> xác nhận đúng -> order đúng -> thanh toán đúng -> doanh thu đúng -> handoff đúng.

Nguyên tắc cuối cùng của PHẦN 1/4:

Không Operational pass thì không Commerce pass. Không QuoteSnapshot thì không final price. Không CustomerConfirmation thì không official order. Không payment confirmation thì không PAID. Không order verification thì không verified revenue. Không accepted evidence thì không PASS.

## PHẦN 1/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 2/4 - COMMERCE MODULE CONTRACTS

## 24. MỤC TIÊU CỦA PHẦN 2/4

## 24.1. Vai trò của Commerce Module Contracts

## PHẦN 2/4 khóa contract cho từng module trong Commerce Runtime.

Contract ở đây không phải API contract, không phải database schema, không phải UI design và không phải code.

Contract ở đây là cam kết kỹ thuật cấp module:

Module này chịu trách nhiệm gì.

Module này không chịu trách nhiệm gì.

Module này nhận dữ liệu từ đâu.

Module này tạo quyết định gì.

Module nào được phép tiêu thụ kết quả của module này.

Điều kiện nào làm module bị block.

P0 điểm chặn nào bắt buộc chặn release.

Evidence nào bắt buộc.

Smoke nào bắt buộc.

Downstream nào không được bypass.

Mục tiêu là để khi chuyển sang DEV phase, đội kỹ thuật không tự suy luận sai về giá, quote, order, payment, verified revenue, commission hoặc Ads ROAS.

## 24.2. Nguyên tắc đọc PHẦN 2/4

Tất cả Commerce Module Contracts phải tuân thủ PHẦN 1:

Không Operational pass thì không Commerce pass.

Không Product/SKU approved thì không bán.

Không Listed Price active thì không final quote.

Không QuoteSnapshot thì không báo final price.

Không CustomerConfirmation thì không official order.

Không official order thì không order_code.

Không payment confirmation thì không PAID.

Không order verification thì không verified revenue.

Không verified revenue thì không Ads ROAS/KPI/commission.

Không cộng dồn quyền lợi tùy tiện.

Không hardcode thông tin ngân hàng.

Không dùng lời xác nhận miệng để PASS.

Không gọi Documentation Complete là Production Ready.

## 25. DANH SÁCH MODULE CONTRACT TRONG TECH-04

Commerce Runtime trong TECH-04 gồm các module contract sau:

COM-M01 - Product / SKU / Channel Scope Check.

COM-M02 - Operational Sellable Gate.

COM-M03 - Listed Price Runtime.

COM-M04 - Program Price Runtime.

COM-M05 - Member Tier / Member Benefit Eligibility.

COM-M06 - Diamond Referral Eligibility.

COM-M07 - Policy Priority Decision.

COM-M08 - QuoteSnapshot.

COM-M09 - Quote Hold / Quote Refresh.

COM-M10 - Cart Runtime.

COM-M11 - Customer Checkout Context.

COM-M12 - Order Draft.

COM-M13 - Customer Confirmation.

COM-M14 - Official Order Creation.

COM-M15 - Payment Method Runtime.

COM-M16 - Direct Bank Transfer / Accounting Review Boundary.

COM-M17 - COD Boundary.

COM-M18 - Shipping Runtime.

COM-M19 - VAT / Invoice Request Boundary.

COM-M20 - Order Status / Order State.

COM-M21 - Payment Confirmation / Paid Status.

COM-M22 - Order Verification / Verified Revenue.

COM-M23 - Commission / KPI / Ads ROAS Handoff.

COM-M24 - Commerce Suppression / Runtime Blocking.

COM-M25 - Commerce Handoff to Downstream.

## COM-M01 - PRODUCT / SKU / CHANNEL SCOPE CHECK

## 26. COM-M01 - PRODUCT / SKU / CHANNEL SCOPE CHECK CONTRACT

## 26.1. Mục tiêu

COM-M01 kiểm tra sản phẩm/SKU có thuộc phạm vi thương mại được phép bán trên kênh hiện tại hay không.

Module này trả lời:

Product/SKU có hợp lệ theo TECH-02 không?

Product/SKU có active theo đúng scope không?

Product/SKU có được phép bán trên kênh này không?

Product public name/knowledge có approved để downstream sử dụng không?

## 26.2. Scope In

COM-M01 bao gồm:

Product active scope.

SKU active scope.

Product release scope.

Channel sale scope.

Public product name approved.

Product Knowledge approved status nếu AI/Live/Public cần dùng.

Trade item scope nếu liên quan bán theo hộp/thùng.

Future SKU onboarding gate.

## 26.3. Scope Out

COM-M01 không chịu trách nhiệm:

Xác định batch đã release hay chưa.

Xác định tồn kho.

Tính giá.

Tạo quote.

Tạo order.

Xác nhận thanh toán.

Xác định verified revenue.

Public trace.

## 26.4. Upstream Dependency

Upstream

Điều kiện

TECH-02 Product Domain

Product/SKU/Trade Item hợp lệ

TECH-02 Product Knowledge

Public/Internal knowledge approved theo scope

TECH-01 Foundation

Permission/audit nếu có command scope change

Owner Product Scope

Product/channel release scope approved

## 26.5. Downstream Consumer

COM-M01 cung cấp dữ liệu cho:

COM-M02 Operational Sellable Gate.

COM-M03 Listed Price Runtime.

COM-M08 QuoteSnapshot.

COM-M10 Cart.

COM-M12 Order Draft.

AI Advisor.

Facebook Gateway.

Ads.

Live.

## CRM.

## 26.6. P0 điểm chặn

COM-M01 bị block nếu:

Product/SKU không tồn tại trong Product Domain.

Product/SKU chưa approved.

Product active nhưng không thuộc channel scope.

Public Product Knowledge chưa approved nhưng AI/Live/Public vẫn dùng.

SKU bị loại khỏi release scope nhưng Commerce vẫn bán.

Future SKU chưa onboarding nhưng được quote/order.

## 26.7. Evidence Required

Product/SKU scope approval.

Channel scope approval.

Product Knowledge approval nếu dùng public.

Future SKU onboarding evidence nếu có.

Audit nếu scope change.

## 26.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M01-SMK-001

SKU chưa approved

Không vào sellable check

## COM-M01-SMK-002

SKU active nhưng không thuộc channel

Không quote/order

## COM-M01-SMK-003

Product Knowledge chưa approved

AI/Live/Public không dùng

## COM-M01-SMK-004

SKU approved + channel pass

Được chuyển sang Operational Sellable Gate

## COM-M02 - OPERATIONAL SELLABLE GATE

## 27. COM-M02 - OPERATIONAL SELLABLE GATE CONTRACT

## 27.1. Mục tiêu

COM-M02 kiểm tra tín hiệu vận hành từ TECH-03 trước khi Commerce xét giá và order.

Module này trả lời:

SKU có batch released không?

Thành phẩm đã nhập kho chưa?

Có tồn khả dụng không?

Có recall không?

Có sale lock không?

Có quality hold không?

Operational Runtime có khả dụng không?

Có được tiếp tục quote/order không?

## 27.2. Scope In

COM-M02 bao gồm:

Operational Sellable Signal.

Finished goods stock safe view.

Batch release status.

Warehouse receipt status.

Recall status.

Sale Lock status.

Quality hold status.

Trace status nếu policy yêu cầu.

Blocking reason từ Operational Core.

Runtime status.

## 27.3. Scope Out

COM-M02 không chịu trách nhiệm:

Tạo batch.

Release batch.

Nhập kho.

Sửa tồn kho.

Gỡ sale lock.

Gỡ recall.

Tính giá.

Tạo order.

Xác nhận payment.

## 27.4. Upstream Dependency

Upstream

Điều kiện

TECH-03 Operational Core

Operational Sellable Signal

TECH-03 Inventory

Stock safe view

TECH-03 Recall/Sale Lock

Lock/recall status

TECH-03 Trace/Public Trace

Nếu policy yêu cầu

TECH-01 Foundation

Audit/evidence cho dependency gate

## 27.5. Downstream Consumer

COM-M02 cung cấp cho:

COM-M03 Listed Price.

COM-M08 QuoteSnapshot.

COM-M10 Cart.

COM-M12 Order Draft.

COM-M14 Official Order.

AI Advisor.

Facebook Gateway.

Ads.

Live.

## CRM.

## 27.6. P0 điểm chặn

COM-M02 bị block nếu:

Operational Runtime unavailable.

Batch chưa RELEASED.

Warehouse chưa receipt.

Không có tồn khả dụng.

Recall active.

Sale Lock active.

Quality hold active.

Trace chain missing nếu policy yêu cầu.

Public Trace bị chặn nếu kênh bán yêu cầu trace.

Downstream dùng stock không qua Operational Sellable Gate.

## 27.7. Evidence Required

Operational sellable dependency evidence.

Stock safe view evidence.

Recall/Sale Lock blocking evidence.

Runtime unavailable fail-safe evidence.

Downstream block evidence.

## 27.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M02-SMK-001

Batch chưa released

Không quote/order

## COM-M02-SMK-002

Warehouse chưa receipt

Không quote/order

## COM-M02-SMK-003

Sale Lock active

Commerce bị chặn

## COM-M02-SMK-004

Recall active

Commerce bị chặn

## COM-M02-SMK-005

Operational unavailable

Fail-safe, không quote/order

## COM-M03 - LISTED PRICE RUNTIME

## 28. COM-M03 - LISTED PRICE RUNTIME CONTRACT

## 28.1. Mục tiêu

COM-M03 quản trị giá niêm yết active của SKU tại runtime.

Module này trả lời:

SKU có giá niêm yết active không?

Giá niêm yết có đang hiệu lực không?

Giá có được owner approved không?

Giá có thể dùng để tạo QuoteSnapshot không?

## 28.2. Scope In

COM-M03 bao gồm:

Listed price.

Listed price status.

Effective date/time.

Expiry date/time nếu có.

Owner approval.

Currency.

Price version.

Channel scope nếu giá theo kênh.

Audit khi thay đổi giá.

## 28.3. Scope Out

COM-M03 không chịu trách nhiệm:

Tính program discount.

Tính member discount.

Tính referral benefit.

Tạo final price.

Tạo quote.

Tạo order.

Xác định stock.

Xác nhận payment.

## 28.4. Upstream Dependency

Upstream

Điều kiện

COM-M01 Product/SKU Scope

SKU hợp lệ

TECH-01 Foundation

Permission/audit khi thay price

Owner Price Policy

Giá được duyệt

Channel Policy

Nếu giá theo kênh

## 28.5. Downstream Consumer

COM-M03 cung cấp cho:

COM-M04 Program Price.

COM-M07 Policy Priority.

COM-M08 QuoteSnapshot.

COM-M12 Order Draft.

AI Advisor qua QuoteSnapshot.

## 28.6. P0 điểm chặn

COM-M03 bị block nếu:

Listed price missing.

Listed price inactive.

Listed price expired.

Listed price conflict nhiều bản active.

Giá chưa owner approved.

Giá không đúng channel scope.

Giá được hardcode trong AI/Live/Gateway thay vì runtime.

## 28.7. Evidence Required

Listed price approval.

Price version record.

Effective/expiry evidence.

Channel price evidence nếu có.

Audit price change.

## 28.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M03-SMK-001

Missing listed price

Không final quote

## COM-M03-SMK-002

Listed price inactive

Không final quote

## COM-M03-SMK-003

Listed price active

Được dùng cho QuoteSnapshot

## COM-M03-SMK-004

Nhiều price active conflict

Block, cần owner review

## COM-M04 - PROGRAM PRICE RUNTIME

## 29. COM-M04 - PROGRAM PRICE RUNTIME CONTRACT

## 29.1. Mục tiêu

COM-M04 xác định chương trình giá đang áp dụng tại runtime.

Module này trả lời:

Hiện tại có chương trình nào áp dụng cho SKU/kênh/thời điểm không?

SKU có thuộc chương trình không?

Chương trình còn hiệu lực không?

Program price/discount nào được dùng trong QuoteSnapshot?

## 29.2. Scope In

COM-M04 bao gồm:

Program type.

Program name.

Program status.

Program time window.

Program channel scope.

SKU scope.

Discount percent/value.

Program price.

Giờ Vàng session nếu có.

24/7 program rule nếu có.

Owner approval.

Runtime decision.

## 29.3. Scope Out

COM-M04 không chịu trách nhiệm:

Tính member benefit nếu chưa qua COM-M05.

Tính Diamond referral nếu chưa qua COM-M06.

Quyết định cộng dồn quyền lợi nếu chưa qua COM-M07.

Tạo order.

Xác nhận payment.

Tạo verified revenue.

## 29.4. Upstream Dependency

Upstream

Điều kiện

COM-M03 Listed Price

Listed price active

Program Policy

Program approved

Time Runtime

Thời điểm hiện tại

TECH-01 Foundation

Audit/approval

COM-M02 Operational Gate

Không chạy program cho SKU không sellable

## 29.5. Downstream Consumer

COM-M04 cung cấp cho:

COM-M07 Policy Priority.

COM-M08 QuoteSnapshot.

AI Advisor qua QuoteSnapshot.

Live/Gateway qua QuoteSnapshot.

Ads/CRM nếu cần program eligibility.

## 29.6. P0 điểm chặn

COM-M04 bị block nếu:

Program inactive.

Program expired.

SKU không thuộc program.

Channel không thuộc program.

Program rule conflict.

Program bị áp khi Operational bị chặn.

AI/Live tự nói program price không qua Commerce Runtime.

## 29.7. Evidence Required

Program approval.

Program runtime decision record.

Program price/discount evidence.

Session evidence nếu Giờ Vàng.

Program conflict review nếu có.

## 29.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M04-SMK-001

Program inactive

Không áp program

## COM-M04-SMK-002

SKU ngoài program

Không áp program

## COM-M04-SMK-003

Program active + SKU in scope

Được tính vào QuoteSnapshot

## COM-M04-SMK-004

Program active nhưng Operational bị chặn

Không quote/order

## COM-M05 - MEMBER TIER / MEMBER BENEFIT ELIGIBILITY

## 30. COM-M05 - MEMBER TIER / MEMBER BENEFIT ELIGIBILITY CONTRACT

## 30.1. Mục tiêu

COM-M05 xác định khách có được hưởng quyền lợi thành viên tại thời điểm quote/order hay không.

Module này trả lời:

Khách có phải member không?

Hạng member hiện tại là gì?

Chu kỳ member còn hiệu lực không?

Grace period có áp dụng không?

SKU/chương trình có cho phép quyền lợi member không?

Member benefit nào được đưa vào QuoteSnapshot?

## 30.2. Scope In

COM-M05 bao gồm:

Customer identity.

Member tier.

Member status.

Membership cycle.

Grace period status.

Eligible benefit.

Member discount percent/value nếu runtime pass.

Benefit eligibility reason.

Benefit blocking reason.

Owner policy reference.

## 30.3. Scope Out

COM-M05 không chịu trách nhiệm:

Tự xác định khách bằng lời khai của khách.

Tự nâng/hạ hạng member nếu chưa có member runtime.

Tự cộng quyền lợi member với program/referral.

Tạo quote cuối nếu chưa qua COM-M07/COM-M08.

Tạo order.

Tính commission.

## 30.4. Upstream Dependency

Upstream

Điều kiện

Customer Identity Runtime

Xác định khách

Member Runtime

Member tier/status/cycle

COM-M04 Program Runtime

Program có cho phép member benefit không

COM-M07 Policy Priority

Quyết định cộng/không cộng quyền lợi

TECH-01 Foundation

Audit/evidence

## 30.5. Downstream Consumer

COM-M05 cung cấp cho:

COM-M07 Policy Priority.

COM-M08 QuoteSnapshot.

AI Advisor qua QuoteSnapshot.

CRM nếu cần member messaging.

Commission/KPI nếu policy liên quan.

## 30.6. P0 điểm chặn

COM-M05 bị block nếu:

Không xác định được customer identity.

Member runtime unavailable.

Member tier không hợp lệ.

Chu kỳ hết hạn và grace không pass.

Program không cho phép áp member benefit.

AI hỏi khách tự khai hạng rồi tính giá.

Member benefit được áp ngoài runtime.

## 30.7. Evidence Required

Customer identity resolution evidence.

Member tier/status evidence.

Cycle/grace evidence.

Benefit eligibility decision.

Blocking reason nếu không áp.

Audit.

## 30.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M05-SMK-001

Member runtime unavailable

Không áp member benefit

## COM-M05-SMK-002

Member tier valid

Được xét benefit

## COM-M05-SMK-003

Cycle expired, grace fail

Không áp benefit

## COM-M05-SMK-004

AI tự khai member tier

Không được dùng để tính giá

## COM-M06 - DIAMOND REFERRAL ELIGIBILITY

## 31. COM-M06 - DIAMOND REFERRAL ELIGIBILITY CONTRACT

## 31.1. Mục tiêu

COM-M06 xác định người mua có đủ điều kiện hưởng quyền lợi từ link Diamond/referral hay không.

Module này trả lời:

Khách có vào từ link Diamond hợp lệ không?

Referral bind có hợp lệ không?

Referral owner có đủ điều kiện không?

Buyer_from_diamond_link có pass không?

Quyền lợi buyer qua link Diamond có được áp không?

## 31.2. Scope In

COM-M06 bao gồm:

Referral link id.

Referral owner.

Referral owner tier/status.

Entry channel.

Entry session.

Bind status.

Bound time.

Buyer_from_diamond_link.

Referral benefit eligibility.

Self-purchase check nếu policy có.

Blocking reason.

## 31.3. Scope Out

COM-M06 không chịu trách nhiệm:

Tính final price nếu chưa qua COM-M07/COM-M08.

Tự trả commission.

Tự xác minh payment.

Tự xác minh revenue.

Tạo order.

Tạo Ads attribution.

Public commission cho khách.

## 31.4. Upstream Dependency

Upstream

Điều kiện

Referral Runtime

Link/bind/owner hợp lệ

Member Runtime

Owner tier/status nếu cần

COM-M04 Program Runtime

Program có cho phép referral benefit không

COM-M07 Policy Priority

Quyết định quyền lợi

TECH-01 Foundation

Audit/evidence

## 31.5. Downstream Consumer

COM-M06 cung cấp cho:

COM-M07 Policy Priority.

COM-M08 QuoteSnapshot.

AI Advisor qua QuoteSnapshot.

Commission engine sau verified revenue.

Ads/Referral attribution nếu scope có.

## 31.6. P0 điểm chặn

COM-M06 bị block nếu:

Referral link không hợp lệ.

Bind status không BOUND.

Referral owner không đủ điều kiện.

Program không cho phép benefit này.

Buyer tự khai “vào từ link” nhưng không có bind.

AI/Gateway tự áp 5% không qua resolver/runtime.

Self-purchase bị cấm nhưng vẫn tính quyền lợi/commission.

## 31.7. Evidence Required

Referral bind evidence.

Referral owner eligibility evidence.

Buyer_from_diamond_link decision.

Benefit eligibility decision.

Blocking reason.

Audit.

## 31.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M06-SMK-001

Link Diamond không bind

Không áp referral benefit

## COM-M06-SMK-002

Bind hợp lệ

Được xét benefit

## COM-M06-SMK-003

Owner không đủ điều kiện

Không áp benefit

## COM-M06-SMK-004

AI tự áp referral benefit

Bị chặn

## COM-M07 - POLICY PRIORITY DECISION

## 32. COM-M07 - POLICY PRIORITY DECISION CONTRACT

## 32.1. Mục tiêu

COM-M07 quyết định chính sách quyền lợi nào được áp dụng khi có nhiều quyền lợi cùng tồn tại.

Module này bảo vệ hệ thống khỏi việc cộng dồn tùy tiện.

## 32.2. Scope In

COM-M07 bao gồm:

Listed price input.

Program benefit input.

Member benefit input.

Diamond referral benefit input.

Channel policy.

Program policy.

Conflict rule.

Priority decision.

Applied benefit policy.

Rejected benefit reason.

Final eligibility set đưa sang QuoteSnapshot.

## 32.3. Scope Out

COM-M07 không chịu trách nhiệm:

Tự tạo member tier.

Tự tạo referral bind.

Tự tạo program.

Tự tạo listed price.

Tạo order.

Xác nhận payment.

Tính verified revenue.

## 32.4. Upstream Dependency

Upstream

Điều kiện

COM-M03 Listed Price

Giá niêm yết active

COM-M04 Program Runtime

Program decision

COM-M05 Member Eligibility

Member benefit decision

COM-M06 Referral Eligibility

Referral benefit decision

Owner Pricing Policy

Priority rule approved

## 32.5. Downstream Consumer

COM-M07 cung cấp cho:

COM-M08 QuoteSnapshot.

AI Advisor.

Gateway.

Order Draft.

Finance/Commission explanation nếu cần.

## 32.6. P0 điểm chặn

COM-M07 bị block nếu:

Không có priority rule.

Program/member/referral conflict không được xử lý.

Quyền lợi được cộng dồn tùy tiện.

AI/Live/Gateway tự chọn discount.

Final price không trace được applied policy.

Rejected benefit không có reason.

## 32.7. Evidence Required

Policy priority approval.

Runtime decision evidence.

Applied/rejected benefit record.

Conflict handling evidence.

Audit.

## 32.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M07-SMK-001

Program + Member cùng pass

Runtime quyết định theo policy

## COM-M07-SMK-002

Program + Referral cùng pass

Runtime quyết định theo policy

## COM-M07-SMK-003

Không có priority rule

Không tạo final quote

## COM-M07-SMK-004

AI tự cộng quyền lợi

Bị chặn

## COM-M08 - QUOTESNAPSHOT

## 33. COM-M08 - QUOTESNAPSHOT CONTRACT

## 33.1. Mục tiêu

COM-M08 tạo QuoteSnapshot - nguồn báo giá duy nhất của toàn hệ thống.

QuoteSnapshot khóa toàn bộ dữ liệu báo giá tại thời điểm quote để tránh tranh chấp giá.

## 33.2. Scope In

QuoteSnapshot bao gồm:

Customer context.

Channel context.

## SKU.

Product public name.

Quantity.

Listed price.

Listed price status.

Program name.

Program discount.

Program price.

Member tier nếu runtime xác nhận.

Member benefit nếu eligible.

Diamond referral benefit nếu eligible.

Policy priority decision.

Shipping fee nếu có.

COD fee nếu policy có; hiện tại không tự thêm nếu chưa khóa.

VAT display nếu scope có.

Final total.

Quote created at.

Quote expires at.

Quote status.

Runtime references.

Blocking reason nếu không thể quote.

## 33.3. Scope Out

QuoteSnapshot không chịu trách nhiệm:

Tự xác minh payment.

Tự tạo official order nếu chưa CustomerConfirmation.

Tự giữ giá sau khi hết hạn.

Tự refresh nếu runtime bị chặn.

Tự bypass sale lock.

Tự tạo verified revenue.

Tự tính commission.

## 33.4. Upstream Dependency

Upstream

Điều kiện

COM-M01 Product Scope

SKU/channel hợp lệ

COM-M02 Operational Gate

Sellable pass

COM-M03 Listed Price

Active

COM-M04 Program Runtime

Program decision

COM-M05 Member Eligibility

Member decision

COM-M06 Referral Eligibility

Referral decision

COM-M07 Policy Priority

Final policy decision

COM-M18 Shipping

Shipping fee nếu cần

COM-M19 VAT/Invoice

VAT display nếu scope có

## 33.5. Downstream Consumer

QuoteSnapshot được dùng bởi:

Cart.

Order Draft.

AI Advisor.

Facebook Gateway.

Messenger/Web/Landing Page.

Live.

## CRM.

Official Order.

Evidence/Dispute review.

## 33.6. P0 điểm chặn

QuoteSnapshot bị block nếu:

Operational bị chặn.

Missing listed price.

Policy priority conflict.

Runtime unavailable.

Shipping required nhưng không xác định được phí/eligibility.

Quote không có expiry.

Quote không lưu final total.

AI/Live/Gateway báo giá không từ QuoteSnapshot.

QuoteSnapshot bị sửa sau khi đã tạo mà không có version/supersede governance.

## 33.7. Evidence Required

QuoteSnapshot creation evidence.

Runtime decision references.

Price calculation trace.

Expiry evidence.

Applied policy decision.

Blocking reason evidence.

Audit.

## 33.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M08-SMK-001

Runtime pass đầy đủ

Tạo QuoteSnapshot

## COM-M08-SMK-002

Missing listed price

Không tạo final quote

## COM-M08-SMK-003

Operational bị chặn

Không tạo quote

## COM-M08-SMK-004

Quote không expiry

Bị chặn

## COM-M08-SMK-005

AI báo giá không snapshot

Bị chặn

## COM-M09 - QUOTE HOLD / QUOTE REFRESH

## 34. COM-M09 - QUOTE HOLD / QUOTE REFRESH CONTRACT

## 34.1. Mục tiêu

COM-M09 quản trị thời hạn giữ giá và refresh quote khi quote hết hạn hoặc runtime thay đổi.

## 34.2. Scope In

COM-M09 bao gồm:

Quote hold policy.

Quote expiration.

Quote status.

Refresh quote.

Supersede old quote.

Expired quote handling.

Customer confirmation time validation.

Audit.

## 34.3. Scope Out

COM-M09 không chịu trách nhiệm:

Tự giữ giá vô thời hạn.

Tự tạo order từ quote hết hạn.

Tự bỏ qua runtime check khi refresh.

Tự xác nhận khách.

Tự xác nhận payment.

## 34.4. Quote Hold Window đã khóa

Chương trình

Hold Window

Giờ Vàng

5 phút

24/7

15 phút

Nếu không thuộc hai chương trình trên, hold window phải do Commerce Runtime/policy xác định, không để AI tự nói.

## 34.5. Upstream Dependency

Upstream

Điều kiện

COM-M08 QuoteSnapshot

Quote tồn tại

Program Runtime

Xác định loại chương trình

Time Runtime

Xác định hết hạn

COM-M02 Operational Gate

Refresh phải check lại

## COM-M03/04/05/06/07

Refresh phải lấy decision mới

## 34.6. Downstream Consumer

COM-M09 cung cấp cho:

Order Draft.

Customer Confirmation.

Official Order Creation.

AI Advisor.

Gateway/Live.

Dispute review.

## 34.7. P0 điểm chặn

COM-M09 bị block nếu:

Quote hết hạn vẫn tạo order.

Refresh quote không kiểm lại Operational/Sellable/Price.

Quote cũ bị sửa thay vì supersede.

AI dùng quote cũ để chốt.

CustomerConfirmation sau expiry vẫn hợp lệ.

## 34.8. Evidence Required

Quote expiry evidence.

Refresh decision evidence.

Superseded quote evidence.

Customer confirmation timing evidence.

Audit.

## 34.9. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M09-SMK-001

Giờ Vàng quote sau 5 phút

Expired

## COM-M09-SMK-002

24/7 quote sau 15 phút

Expired

## COM-M09-SMK-003

Confirm sau expiry

Không tạo order

## COM-M09-SMK-004

Refresh quote

Tạo QuoteSnapshot mới, check runtime lại

## COM-M10 - CART RUNTIME

## 35. COM-M10 - CART RUNTIME CONTRACT

## 35.1. Mục tiêu

COM-M10 quản trị Cart - tập hợp sản phẩm khách đang cân nhắc hoặc chuẩn bị mua.

Cart là bước trung gian trước Order Draft.

## 35.2. Scope In

Cart Runtime bao gồm:

Customer/session context.

Cart line.

## SKU.

Quantity.

QuoteSnapshot reference.

Cart status.

Add/remove/update quantity.

Cart validation.

Cart bị chặn reason.

Audit nếu có command quan trọng.

## 35.3. Scope Out

Cart không chịu trách nhiệm:

Tạo official order.

Tạo order_code.

Xác nhận khách đã mua.

Xác nhận thanh toán.

Tạo verified revenue.

Tính commission.

Dùng làm Ads ROAS.

## 35.4. Upstream Dependency

Upstream

Điều kiện

COM-M08 QuoteSnapshot

Giá dòng cart phải từ snapshot

COM-M09 Quote Hold

Snapshot còn hiệu lực hoặc cần refresh

COM-M02 Operational Gate

Cart validation phải check sellable

Customer Session

Gắn đúng khách/session

## 35.5. Downstream Consumer

Cart cung cấp cho:

Order Draft.

AI Advisor.

Gateway/Web/Landing Page.

Customer Confirmation flow.

## 35.6. P0 điểm chặn

Cart bị block nếu:

Cart line không có QuoteSnapshot.

Quote expired nhưng vẫn dùng.

SKU không sellable.

Quantity vượt policy/stock safe view.

Cart được tính là revenue.

Cart tạo order_code.

## 35.7. Evidence Required

Cart validation evidence.

QuoteSnapshot reference.

Expiry check evidence.

Stock/sellable check evidence.

Audit nếu update quan trọng.

## 35.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M10-SMK-001

Add SKU bị chặn

Không thêm vào cart

## COM-M10-SMK-002

Add SKU pass + quote valid

Cart line created

## COM-M10-SMK-003

Quote expired trong cart

Yêu cầu refresh

## COM-M10-SMK-004

Cart được dùng làm revenue

Bị chặn

## COM-M11 - CUSTOMER CHECKOUT CONTEXT

## 36. COM-M11 - CUSTOMER CHECKOUT CONTEXT CONTRACT

## 36.1. Mục tiêu

COM-M11 quản trị ngữ cảnh khách hàng cần thiết cho checkout.

Module này giúp Order Draft có đủ thông tin nhận hàng và thanh toán, nhưng không tự tạo official order.

## 36.2. Scope In

Customer Checkout Context bao gồm:

Customer identity.

Customer type: new / existing / member nếu runtime xác nhận.

Receiver name.

Phone.

Address.

Delivery note.

Existing delivery info nếu khách cũ có dữ liệu lưu.

Option dùng thông tin cũ.

Updated delivery info nếu khách muốn đổi.

Payment preference.

Invoice request preference nếu có.

Consent/confirmation state nếu policy cần.

## 36.3. Scope Out

COM-M11 không chịu trách nhiệm:

Tự xác nhận order.

Tự xác nhận payment.

Tự xác định member benefit nếu chưa qua COM-M05.

Tự lấy dữ liệu nhạy cảm không cần thiết.

Tự public thông tin nhận hàng.

Tự tạo order_code.

## 36.4. Upstream Dependency

Upstream

Điều kiện

Customer Identity Runtime

Xác định khách

Customer Profile/CRM nếu scope có

Lấy thông tin cũ theo policy

Privacy/Consent Policy

Dùng dữ liệu cũ đúng rule

COM-M15 Payment Method

Payment preference

COM-M18 Shipping Runtime

Delivery eligibility

## 36.5. Downstream Consumer

COM-M11 cung cấp cho:

Order Draft.

Customer Confirmation.

Shipping.

Payment instruction.

Official Order.

## 36.6. P0 điểm chặn

COM-M11 bị block nếu:

Thiếu thông tin nhận hàng bắt buộc.

Dùng thông tin cũ mà khách không chọn/không được policy cho phép.

Public full address trên kênh public.

Tạo order khi thông tin nhận hàng chưa đủ.

Nhầm customer identity.

Lưu/hiển thị dữ liệu riêng tư trái policy.

## 36.7. Evidence Required

Customer identity evidence.

Delivery info source evidence.

“Dùng thông tin cũ” selection evidence nếu có.

Updated info evidence nếu đổi.

Privacy/consent evidence nếu policy yêu cầu.

## 36.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M11-SMK-001

Khách mới thiếu địa chỉ

Không tạo Order Draft complete

## COM-M11-SMK-002

Khách cũ chọn dùng thông tin cũ

Prefill hợp lệ

## COM-M11-SMK-003

Khách đổi địa chỉ

Dùng thông tin mới

## COM-M11-SMK-004

Public full address

Bị chặn

## COM-M12 - ORDER DRAFT

## 37. COM-M12 - ORDER DRAFT CONTRACT

## 37.1. Mục tiêu

Order Draft là bản tạm tính/xác nhận đơn nháp để khách kiểm tra trước khi xác nhận chính thức.

Order Draft chưa phải official order.

## 37.2. Scope In

Order Draft phải có tối thiểu 3 phần:

Thông tin nhận hàng.

Phương án thanh toán.

Phần xác nhận đơn hàng rõ ràng.

Order Draft bao gồm:

QuoteSnapshot reference.

Product lines.

Quantity.

Price summary.

Shipping fee nếu có.

Payment method options.

Receiver information.

Quote expiry.

Confirmation CTA/text.

Draft status.

Blocking reason nếu thiếu điều kiện.

## 37.3. Scope Out

Order Draft không chịu trách nhiệm:

Tự tạo official order.

Tự cấp order_code.

Tự gắn PAID.

Tự tạo verified revenue.

Tự giữ giá sau expiry.

Tự vượt sale lock/recall.

## 37.4. Upstream Dependency

Upstream

Điều kiện

COM-M08 QuoteSnapshot

Quote hợp lệ

COM-M09 Quote Hold

Quote còn hiệu lực

COM-M10 Cart

Cart valid

COM-M11 Customer Checkout Context

Đủ thông tin nhận hàng

COM-M15 Payment Method

Có phương án thanh toán hợp lệ

COM-M18 Shipping

Shipping pass nếu cần

## 37.5. Downstream Consumer

Order Draft cung cấp cho:

Customer Confirmation.

AI Advisor.

Messenger/Web/Landing Page.

Gateway.

Official Order Creation.

## 37.6. P0 điểm chặn

Order Draft bị block nếu:

Thiếu QuoteSnapshot.

Quote expired.

Thiếu thông tin nhận hàng.

Thiếu payment method.

Thiếu phần xác nhận đơn.

Sản phẩm không còn sellable.

Order Draft tự tạo order_code.

Order Draft được tính doanh thu.

## 37.7. Evidence Required

Order Draft record.

QuoteSnapshot reference.

Customer info evidence.

Payment option evidence.

Quote expiry display evidence.

Draft review evidence.

## 37.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M12-SMK-001

Draft thiếu thông tin nhận hàng

Không complete

## COM-M12-SMK-002

Draft thiếu payment method

Không complete

## COM-M12-SMK-003

Draft đủ 3 phần

Cho phép CustomerConfirmation

## COM-M12-SMK-004

Draft tự tạo order_code

Bị chặn

## COM-M13 - CUSTOMER CONFIRMATION

## 38. COM-M13 - CUSTOMER CONFIRMATION CONTRACT

## 38.1. Mục tiêu

COM-M13 ghi nhận hành động xác nhận đơn hợp lệ của khách sau khi khách đã xem Order Draft.

## 38.2. Scope In

Customer Confirmation bao gồm:

Order Draft reference.

Customer action.

Confirmation channel.

Confirmation timestamp.

Quote validity check.

Draft completeness check.

Confirmation status.

CTA/button confirmation nếu kênh hỗ trợ.

Text fallback nếu kênh chỉ-text.

Audit/correlation.

## 38.3. Scope Out

Customer Confirmation không chịu trách nhiệm:

Tự tạo quote.

Tự thay giá.

Tự gắn PAID.

Tự verified revenue.

Tự bỏ qua quote expiry.

Tự tạo order nếu Order Draft invalid.

## 38.4. Upstream Dependency

Upstream

Điều kiện

COM-M12 Order Draft

Draft complete

COM-M09 Quote Hold

Quote còn hiệu lực

COM-M02 Operational Gate

Sản phẩm vẫn sellable

Customer Identity/Session

Xác nhận đúng khách/session

Channel Runtime

CTA/text confirmation hợp lệ

## 38.5. Downstream Consumer

Customer Confirmation cung cấp cho:

Official Order Creation.

IVR nếu scope order confirmation có.

Evidence/Dispute review.

AI/Gateway response.

## 38.6. P0 điểm chặn

Customer Confirmation bị block nếu:

Order Draft không complete.

Quote expired.

Khách chưa thấy/xác nhận Draft.

Confirmation không đúng customer/session.

Text “mua/chốt/lấy” bị dùng thay confirmation sau Draft nếu chưa đủ flow.

System tự confirm thay khách.

Confirmation sau sale lock/recall active.

## 38.7. Evidence Required

Confirmation action evidence.

Draft reference.

Quote validity evidence.

Customer/session match evidence.

Channel confirmation evidence.

Audit.

## 38.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M13-SMK-001

Khách nói “mua” trước Draft

Chưa tạo official order

## COM-M13-SMK-002

Draft complete + CTA confirm

## COM-M13-SMK-003

Confirm sau quote expired

Bị chặn

## COM-M13-SMK-004

System tự confirm

Bị chặn

## COM-M14 - OFFICIAL ORDER CREATION

## 39. COM-M14 - OFFICIAL ORDER CREATION CONTRACT

## 39.1. Mục tiêu

COM-M14 tạo official order sau khi CustomerConfirmation hợp lệ.

Đây là điểm duy nhất được cấp order_code.

## 39.2. Scope In

Official Order Creation bao gồm:

CustomerConfirmation reference.

Order Draft reference.

QuoteSnapshot reference.

Order lines.

Receiver info.

Payment method.

Shipping info.

Final total.

Order code.

Order status initial.

Audit.

Idempotency.

## 39.3. Scope Out

COM-M14 không chịu trách nhiệm:

Gắn PAID.

Verified revenue.

Fulfillment hoàn tất.

Commission payout.

Ads ROAS.

MISA sync success.

Gỡ sale lock.

Bỏ qua payment.

## 39.4. Upstream Dependency

Upstream

Điều kiện

COM-M13 Customer Confirmation

## CONFIRMED

COM-M12 Order Draft

Complete

COM-M09 Quote Hold

Quote còn hiệu lực hoặc refreshed

COM-M02 Operational Gate

Sellable vẫn pass

COM-M15 Payment Method

Payment method hợp lệ

TECH-01 Idempotency

Không tạo order trùng

## 39.5. Downstream Consumer

Official Order cung cấp cho:

Payment Runtime.

Shipping/Fulfillment.

AI/Gateway.

IVR nếu scope có.

MISA/Finance nếu scope có.

Order Verification.

## CRM.

## 39.6. P0 điểm chặn

Official Order Creation bị block nếu:

CustomerConfirmation chưa CONFIRMED.

Quote expired.

Order Draft thiếu thông tin.

Operational bị chặn tại thời điểm tạo order.

Payment method invalid.

Retry tạo order trùng.

Order_code cấp trước official order.

Order tạo từ text chat không có QuoteSnapshot.

## 39.7. Evidence Required

Official order creation evidence.

CustomerConfirmation reference.

QuoteSnapshot reference.

Order idempotency evidence.

Order_code issuance evidence.

Audit.

## 39.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M14-SMK-001

Chưa CustomerConfirmation

Không tạo order

## COM-M14-SMK-002

Confirmation valid

Tạo official order + order_code

## COM-M14-SMK-003

Retry create order

Không tạo trùng

## COM-M14-SMK-004

Sale lock active trước create

Bị chặn

## COM-M15 - PAYMENT METHOD RUNTIME

## 40. COM-M15 - PAYMENT METHOD RUNTIME CONTRACT

## 40.1. Mục tiêu

COM-M15 xác định phương thức thanh toán hợp lệ cho order.

## 40.2. Scope In

Payment Method Runtime bao gồm:

Payment method list.

VNPAY nếu scope có.

MOMO_BUSINESS nếu scope có.

DIRECT_BANK_TRANSFER_TO_SSAVIGROUP nếu scope có.

COD nếu scope/kênh cho phép.

Payment method eligibility.

Payment instruction.

Payment blocking reason.

Channel/payment scope.

## 40.3. Scope Out

COM-M15 không chịu trách nhiệm:

Xác nhận PAID.

Đối soát ngân hàng.

Tự xác nhận COD đã thu.

Ghi nhận verified revenue.

Tự tạo MISA entry.

Hardcode bank account.

## 40.4. Upstream Dependency

Upstream

Điều kiện

Payment Policy

Payment methods approved

Channel Policy

Kênh được phép dùng method nào

COM-M18 Shipping

COD/shipping eligibility nếu có

Company Bank Account Registry

Nếu chuyển khoản trực tiếp

TECH-01 Foundation

Audit/evidence nếu change payment policy

## 40.5. Downstream Consumer

COM-M15 cung cấp cho:

Order Draft.

Customer Confirmation.

Official Order.

Payment Confirmation.

AI/Gateway instructions.

## 40.6. P0 điểm chặn

COM-M15 bị block nếu:

Payment method không thuộc policy.

Bank info hardcode.

COD được bật ở kênh không cho phép.

Payment instruction không có reference/memo khi cần.

Payment unavailable nhưng vẫn cho chọn.

Payment method bị dùng để gắn PAID tự động.

## 40.7. Evidence Required

Payment policy approval.

Payment method availability evidence.

Bank registry reference nếu chuyển khoản.

Payment instruction evidence.

Channel eligibility evidence.

## 40.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M15-SMK-001

Payment method không active

Không cho chọn

## COM-M15-SMK-002

Bank transfer

Lấy bank info từ registry

## COM-M15-SMK-003

COD không thuộc kênh

Không cho chọn

## COM-M15-SMK-004

Payment instruction thiếu reference

Bị chặn nếu policy yêu cầu

## COM-M16 - DIRECT BANK TRANSFER / ACCOUNTING REVIEW BOUNDARY

## 41. COM-M16 - DIRECT BANK TRANSFER / ACCOUNTING REVIEW BOUNDARY CONTRACT

## 41.1. Mục tiêu

COM-M16 khóa luồng chuyển khoản trực tiếp và ranh giới kế toán xác nhận thanh toán.

## 41.2. Scope In

COM-M16 bao gồm:

Company Bank Account Registry reference.

Payment reference / transfer memo.

Bank transfer instruction.

Customer transfer proof nếu có.

Accounting review status.

Reconciliation status.

Payment WAITING/review/confirmed/rejected.

Audit.

## 41.3. Scope Out

COM-M16 không chịu trách nhiệm:

Hardcode tài khoản ngân hàng.

Gắn PAID chỉ từ ảnh chuyển khoản.

Tự xác nhận giao dịch nếu chưa có review.

Tự tạo verified revenue.

Tự đồng bộ MISA success.

## 41.4. Upstream Dependency

Upstream

Điều kiện

COM-M15 Payment Method

Bank transfer selected

Company Bank Account Registry

Tài khoản hợp lệ

Accounting Review

Xác nhận giao dịch

Payment/Reconciliation Policy

Rule đối soát

TECH-01 Foundation

Audit/evidence

## 41.5. Downstream Consumer

COM-M16 cung cấp cho:

Payment Confirmation.

Order Status.

AI/Gateway payment status response.

Finance/MISA nếu scope có.

Revenue Verification.

## 41.6. P0 điểm chặn

COM-M16 bị block nếu:

Bank account hardcoded.

Payment reference missing.

Ảnh chuyển khoản được gắn PAID.

Không có accounting review nhưng status confirmed.

Reconciliation fail nhưng vẫn PAID.

Sai order/payment reference.

Public lộ thông tin ngân hàng ngoài registry/policy.

## 41.7. Evidence Required

Bank registry evidence.

Transfer memo/reference evidence.

Customer proof evidence nếu có.

Accounting review evidence.

Reconciliation evidence.

Audit.

## 41.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M16-SMK-001

Ảnh chuyển khoản

Status review/WAITING, không PAID

## COM-M16-SMK-002

Accounting confirms

Payment confirmed

## COM-M16-SMK-003

Missing memo/reference

WAITING review

## COM-M16-SMK-004

Bank info hardcoded

Fail gate

## COM-M17 - COD BOUNDARY

## 42. COM-M17 - COD BOUNDARY CONTRACT

## 42.1. Mục tiêu

COM-M17 khóa phương thức COD là “nhận hàng rồi thanh toán”.

## 42.2. Scope In

COD Boundary bao gồm:

COD eligibility.

COD channel scope.

COD shipping eligibility.

COD payment WAITING state.

COD collection confirmation nếu scope có.

COD fail/cancel reason.

Audit.

## 42.3. Scope Out

COD không chịu trách nhiệm:

Tự gắn PAID khi order tạo.

Tự thêm phí COD nếu policy chưa khóa.

Tự verified revenue trước khi thu tiền/verification pass.

Tự tính commission.

Tự tính Ads ROAS.

## 42.4. Upstream Dependency

Upstream

Điều kiện

COM-M15 Payment Method

COD được bật

COM-M18 Shipping

Khu vực giao/COD eligible

Collection/Fulfillment policy

Xác nhận thu tiền

COM-M22 Revenue Verification

Chỉ verified khi đủ điều kiện

## 42.5. Downstream Consumer

COD cung cấp cho:

Order Draft.

Official Order.

Payment Status.

Fulfillment.

Revenue Verification.

AI/Gateway.

## 42.6. P0 điểm chặn

COD bị block nếu:

COD bị gắn PAID ngay khi order tạo.

Tự thêm phí COD khi chưa có policy.

COD không eligible nhưng vẫn cho chọn.

COD failed nhưng vẫn verified revenue.

COD unpaid vẫn commission/Ads ROAS.

## 42.7. Evidence Required

COD eligibility evidence.

COD selection evidence.

Collection confirmation nếu có.

COD failed/cancel evidence nếu có.

Audit.

## 42.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M17-SMK-001

Order COD mới tạo

Payment WAITING/COD WAITING

## COM-M17-SMK-002

COD không thu được tiền

Không verified revenue

## COM-M17-SMK-003

COD fee chưa policy

Không tự thêm phí COD

## COM-M17-SMK-004

COD collection confirmed

Được xét payment/revenue verification

## COM-M18 - SHIPPING RUNTIME

## 43. COM-M18 - SHIPPING RUNTIME CONTRACT

## 43.1. Mục tiêu

COM-M18 xác định điều kiện giao hàng và phí ship nếu có.

## 43.2. Scope In

Shipping Runtime bao gồm:

Receiver address.

Domestic shipping default.

Shipping eligibility.

Shipping fee nếu có.

Shipping method nếu scope có.

COD eligibility nếu liên quan.

Delivery note.

Shipping blocking reason.

Future international extension nếu owner approved.

## 43.3. Scope Out

COM-M18 không chịu trách nhiệm:

Tự tạo order.

Tự xác nhận payment.

Tự verified revenue.

Tự cộng phí không policy.

Tự mở international shipping/payment nếu chưa approved.

Tự fulfill warehouse.

## 43.4. Upstream Dependency

Upstream

Điều kiện

COM-M11 Customer Checkout Context

Địa chỉ nhận hàng

Shipping Policy

Fee/eligibility

COD Policy

Nếu COD

TECH-03 Operational Stock

Có hàng sellable để giao

Future Extension Governance

Nếu international

## 43.5. Downstream Consumer

Shipping Runtime cung cấp cho:

QuoteSnapshot nếu phí ship cần tính trước.

Order Draft.

Official Order.

Payment Method Runtime.

Fulfillment.

AI/Gateway.

## 43.6. P0 điểm chặn

Shipping bị block nếu:

Thiếu địa chỉ giao hàng.

Khu vực không giao nhưng vẫn tạo order.

Phí ship bị tự bịa.

International order/payment tự bật.

COD không eligible nhưng vẫn cho COD.

Shipping fee thiếu nhưng vẫn final total nếu policy yêu cầu.

## 43.7. Evidence Required

Shipping eligibility evidence.

Shipping fee evidence.

Address validation evidence.

COD shipping eligibility nếu có.

Future extension approval nếu international.

## 43.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M18-SMK-001

Thiếu địa chỉ

Không complete Order Draft

## COM-M18-SMK-002

Khu vực không giao

Block order

## COM-M18-SMK-003

Phí ship có policy

Hiển thị đúng trong Draft

## COM-M18-SMK-004

International chưa approved

Bị chặn

## COM-M19 - VAT / INVOICE REQUEST BOUNDARY

## 44. COM-M19 - VAT / INVOICE REQUEST BOUNDARY CONTRACT

## 44.1. Mục tiêu

COM-M19 khóa ranh giới hiển thị VAT và yêu cầu xuất hóa đơn.

## 44.2. Scope In

VAT / Invoice Request bao gồm:

VAT display policy.

Invoice request flag.

Tên công ty/cá nhân.

Mã số thuế nếu có.

Email nhận hóa đơn.

Địa chỉ xuất hóa đơn.

Người nhận hóa đơn.

Số điện thoại.

Ghi chú.

Invoice request status.

Handoff sang Finance/MISA nếu scope có.

## 44.3. Scope Out

COM-M19 không chịu trách nhiệm:

Tự phát hành hóa đơn.

Tự ghi MISA nếu chưa có integration.

Tự tính thuế ngoài policy.

Tự sửa order total sau confirmation nếu không có governance.

Tự public thông tin hóa đơn.

## 44.4. Upstream Dependency

Upstream

Điều kiện

Tax/VAT Policy

VAT display rule

Customer Invoice Input

Thông tin hóa đơn

Finance/MISA Boundary

Nếu xuất hóa đơn tích hợp

COM-M14 Official Order

Order chính thức

## 44.5. Downstream Consumer

COM-M19 cung cấp cho:

Order Draft nếu VAT display cần hiển thị.

Official Order.

Finance.

MISA nếu scope có.

Customer support.

## 44.6. P0 điểm chặn

COM-M19 bị block nếu:

VAT hiển thị không theo policy.

Invoice info thiếu nhưng vẫn marked ready nếu mandatory.

Public lộ thông tin hóa đơn.

MISA missing mapping nhưng success.

Tự phát hành hóa đơn ngoài Finance/MISA boundary.

## 44.7. Evidence Required

VAT display approval.

Invoice request record.

Finance/MISA handoff evidence nếu có.

Privacy evidence.

Audit.

## 44.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M19-SMK-001

Khách yêu cầu hóa đơn

Thu đúng form thông tin

## COM-M19-SMK-002

Thiếu email nhận hóa đơn

WAITING/incomplete theo policy

## COM-M19-SMK-003

Public invoice data

Bị chặn

## COM-M19-SMK-004

MISA mapping missing

WAITING/reconcile

## COM-M20 - ORDER STATUS / ORDER STATE

## 45. COM-M20 - ORDER STATUS / ORDER STATE CONTRACT

## 45.1. Mục tiêu

COM-M20 quản trị vòng đời trạng thái đơn hàng từ official order đến khi verify/cancel/fail.

## 45.2. Scope In

Order Status bao gồm:

## ORDER_CREATED.

## PAYMENT_WAITING.

## PAYMENT_REVIEW.

## PAYMENT_CONFIRMED.

## PAYMENT_FAILED.

## ORDER_VERIFICATION_WAITING.

## ORDER_VERIFIED.

FULFILLMENT_WAITING nếu scope có.

## ORDER_CANCELLED.

## ORDER_EXPIRED.

ORDER_REFUNDED nếu scope có.

## ORDER_CLOSED.

Status reason.

Audit.

## 45.3. Scope Out

COM-M20 không chịu trách nhiệm:

Tạo official order khi chưa CustomerConfirmation.

Tự gắn PAID.

Tự verified revenue.

Tự fulfill kho.

Tự refund ngoài policy.

Tự tạo commission payout.

## 45.4. Upstream Dependency

Upstream

Điều kiện

COM-M14 Official Order

Order chính thức

COM-M21 Payment Confirmation

Payment state

COM-M22 Order Verification

Verification state

Fulfillment nếu scope có

Fulfillment state

TECH-01 Foundation

Audit/idempotency

## 45.5. Downstream Consumer

Order Status cung cấp cho:

AI Advisor.

Gateway.

## IVR.

Fulfillment.

Finance/MISA.

## CRM.

Ads/KPI/Commission after verified.

## 45.6. P0 điểm chặn

COM-M20 bị block nếu:

Order Draft có order status như official order.

Payment WAITING bị xem là paid.

Cancelled order vẫn verified revenue.

Failed payment vẫn paid.

Order status sửa không audit.

Order state nhảy cóc.

Order bị sale lock/recall nhưng vẫn tiếp tục như bình thường nếu policy chặn.

## 45.7. Evidence Required

Order state transition evidence.

Payment reference.

Verification reference.

Cancel/refund reason nếu có.

Audit.

## 45.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M20-SMK-001

Official order mới

## ORDER_CREATED / PAYMENT_WAITING

## COM-M20-SMK-002

Payment WAITING

Không PAID

## COM-M20-SMK-003

Payment confirmed

Được xét order verification

## COM-M20-SMK-004

Cancelled order

Không verified revenue

## COM-M21 - PAYMENT CONFIRMATION / PAID STATUS

## 46. COM-M21 - PAYMENT CONFIRMATION / PAID STATUS CONTRACT

## 46.1. Mục tiêu

COM-M21 xác nhận thanh toán và kiểm soát PAID status.

## 46.2. Scope In

Payment Confirmation bao gồm:

Payment gateway confirmation.

Payment Core confirmation.

Accounting Review confirmation.

COD collection confirmation nếu scope có.

Payment status.

Payment timestamp.

Payment amount.

Payment reference.

Reconciliation status.

Audit.

## 46.3. Scope Out

COM-M21 không chịu trách nhiệm:

Tạo order.

Tạo quote.

Tự verified revenue nếu order verification chưa pass.

Tự tính commission.

Tự tính Ads ROAS.

Tự sync MISA success.

## 46.4. Upstream Dependency

Upstream

Điều kiện

COM-M14 Official Order

Có order chính thức

COM-M15 Payment Method

Method hợp lệ

Payment Gateway/Core

Nếu online

Accounting Review

Nếu chuyển khoản

COD Collection

Nếu COD

Reconciliation Policy

Xác nhận khớp

## 46.5. Downstream Consumer

Payment Confirmation cung cấp cho:

Order Verification.

Fulfillment nếu cần.

Finance/MISA.

AI/Gateway status.

Revenue Verification.

## 46.6. P0 điểm chặn

COM-M21 bị block nếu:

Không có official order.

Ảnh chuyển khoản được xem là PAID.

Payment amount không khớp nhưng vẫn confirmed.

Payment reference không khớp.

COD chưa thu nhưng PAID.

Gateway callback không valid nhưng accepted.

Payment confirmed không audit.

## 46.7. Evidence Required

Payment confirmation evidence.

Payment reference evidence.

Amount match evidence.

Accounting review nếu bank transfer.

COD collection evidence nếu COD.

Audit.

## 46.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M21-SMK-001

Ảnh chuyển khoản

Không PAID

## COM-M21-SMK-002

Gateway payment valid

PAID/payment confirmed

## COM-M21-SMK-003

Amount mismatch

Payment review/bị chặn

## COM-M21-SMK-004

COD chưa thu

Không PAID

## COM-M22 - ORDER VERIFICATION / VERIFIED REVENUE

## 47. COM-M22 - ORDER VERIFICATION / VERIFIED REVENUE CONTRACT

## 47.1. Mục tiêu

COM-M22 xác định đơn hàng có đủ điều kiện trở thành verified revenue hay không.

Verified Revenue là nguồn duy nhất cho Ads ROAS, KPI, commission và báo cáo doanh thu chính thức theo phạm vi hệ thống.

## 47.2. Scope In

Order Verification bao gồm:

Official order existence.

Payment/collection status.

Order not cancelled.

Order not refunded/invalid.

Fulfillment condition nếu policy yêu cầu.

Fraud/risk check nếu scope có.

Revenue verification status.

Verification timestamp.

Reviewer/owner nếu cần.

Audit.

## 47.3. Scope Out

COM-M22 không chịu trách nhiệm:

Tạo payment confirmation.

Tạo order.

Tính commission payout chi tiết.

Tự tạo Ads report.

Tự ghi MISA nếu mapping chưa pass.

Tự bỏ qua cancel/refund/COD fail.

## 47.4. Upstream Dependency

Upstream

Điều kiện

COM-M14 Official Order

Order chính thức

COM-M21 Payment Confirmation

Payment/collection pass

COM-M20 Order Status

Order không cancel/invalid

Fulfillment nếu scope có

Fulfillment condition

Risk/Fraud nếu scope có

Risk pass

Finance Policy

Revenue verification rule

## 47.5. Downstream Consumer

Verified Revenue cung cấp cho:

Ads ROAS.

## KPI.

Commission.

Finance reporting.

## MISA.

CRM revenue attribution.

Owner dashboard.

## 47.6. P0 điểm chặn

COM-M22 bị block nếu:

Quote được tính revenue.

Cart được tính revenue.

Order Draft được tính revenue.

Unpaid order được tính revenue.

Payment WAITING được tính revenue.

Cancelled/refunded order được tính revenue.

COD failed được tính revenue.

Order chưa verified nhưng đưa vào Ads ROAS/KPI/commission.

## 47.7. Evidence Required

Order verification record.

Payment confirmed reference.

Cancel/refund check.

Fulfillment check nếu scope có.

Revenue verification evidence.

Audit.

## 47.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M22-SMK-001

Quote

Không verified revenue

## COM-M22-SMK-002

Order unpaid

Không verified revenue

## COM-M22-SMK-003

Payment confirmed + order valid

Được xét verified revenue

## COM-M22-SMK-004

Cancelled order

Không verified revenue

## COM-M22-SMK-005

COD failed

Không verified revenue

## COM-M23 - COMMISSION / KPI / ADS ROAS HANDOFF

## 48. COM-M23 - COMMISSION / KPI / ADS ROAS HANDOFF CONTRACT

## 48.1. Mục tiêu

COM-M23 bàn giao verified revenue sang các hệ thống đo lường, thưởng, commission và ROAS.

## 48.2. Scope In

COM-M23 bao gồm:

Verified revenue handoff.

Order verified reference.

Customer/source/channel attribution.

Diamond referral attribution nếu eligible.

Commission eligibility status.

KPI eligibility.

Ads revenue attribution.

ROAS revenue eligibility.

Exclusion reason.

Audit/evidence.

## 48.3. Scope Out

COM-M23 không chịu trách nhiệm:

Tự verified revenue.

Tự tạo order.

Tự xác nhận payment.

Tự payout commission.

Tự quyết định Ads scaling.

Tự ghi MISA success.

Tự trả thưởng khi order chưa đủ điều kiện.

## 48.4. Upstream Dependency

Upstream

Điều kiện

COM-M22 Verified Revenue

Revenue verified

COM-M06 Referral Eligibility

Diamond attribution nếu có

Ads Attribution Runtime

Nếu scope có

KPI/Commission Policy

Eligibility rule

Finance/MISA

Nếu cần reconcile

## 48.5. Downstream Consumer

COM-M23 cung cấp cho:

Ads Measurement.

KPI system.

Commission/Reward system.

Finance dashboard.

CRM analytics.

Owner dashboard.

## 48.6. P0 điểm chặn

COM-M23 bị block nếu:

Unverified order được đưa vào Ads ROAS.

Payment WAITING được đưa vào commission.

Diamond self-purchase được tính commission nếu policy cấm.

Referral không bind vẫn tính commission.

Cancel/refund/COD fail vẫn tính revenue.

Ads dùng quote/draft làm revenue.

KPI dùng doanh thu chưa verified.

## 48.7. Evidence Required

Verified revenue reference.

Attribution evidence.

Referral bind evidence nếu có.

Commission eligibility evidence.

Exclusion reason evidence.

Audit.

## 48.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M23-SMK-001

Order chưa verified

Không handoff Ads/KPI/commission

## COM-M23-SMK-002

Verified revenue

Được handoff

## COM-M23-SMK-003

Referral không bind

Không commission/referral benefit

## COM-M23-SMK-004

Cancelled order

Excluded

## COM-M24 - COMMERCE SUPPRESSION / RUNTIME BLOCKING

## 49. COM-M24 - COMMERCE SUPPRESSION / RUNTIME BLOCKING CONTRACT

## 49.1. Mục tiêu

COM-M24 quản trị các trạng thái chặn thương mại do Operational, policy hoặc runtime lỗi.

## 49.2. Scope In

Commerce Suppression bao gồm:

Product bị chặn.

SKU bị chặn.

Channel bị chặn.

Price bị chặn.

Program bị chặn.

Member benefit bị chặn.

Referral benefit bị chặn.

Quote bị chặn.

Order bị chặn.

Payment bị chặn.

Recall bị chặn.

Sale Lock bị chặn.

Runtime unavailable bị chặn.

Blocking reason.

## 49.3. Scope Out

COM-M24 không chịu trách nhiệm:

Tự gỡ recall.

Tự gỡ sale lock.

Tự sửa giá.

Tự mở channel.

Tự tạo order khi bị chặn.

Tự ignore runtime unavailable.

## 49.4. Upstream Dependency

Upstream

Điều kiện

## COM-M01 -> COM-M23

Blocking state từ các module

TECH-03 Recall/Sale Lock

Operational blocking

Runtime Health

Service availability

Policy Runtime

Business rules

## 49.5. Downstream Consumer

COM-M24 cung cấp blocking reason cho:

AI Advisor.

Gateway.

Ads.

Live.

## CRM.

Customer support.

Owner dashboard.

Evidence/Gateway review.

## 49.6. P0 điểm chặn

COM-M24 FAIL nếu:

Blocking reason mơ hồ.

Downstream không nhận suppression.

Runtime unavailable nhưng không bị chặn.

Sale Lock/Recall không propagate.

AI/Ads/Live/CRM dùng cache cũ để bán.

bị chặn order vẫn được tạo.

## 49.7. Evidence Required

Suppression rule evidence.

Blocking reason mapping.

Downstream propagation evidence.

Runtime unavailable evidence.

Recall/Sale Lock propagation evidence.

Audit.

## 49.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M24-SMK-001

Recall active

Commerce suppressed

## COM-M24-SMK-002

Runtime unavailable

Commerce bị chặn

## COM-M24-SMK-003

Price missing

Quote bị chặn

## COM-M24-SMK-004

Downstream query bị chặn SKU

Nhận blocking reason rõ

## COM-M25 - COMMERCE HANDOFF TO DOWNSTREAM

## 50. COM-M25 - COMMERCE HANDOFF TO DOWNSTREAM CONTRACT

## 50.1. Mục tiêu

COM-M25 bàn giao dữ liệu thương mại đã được kiểm soát sang downstream.

Downstream bao gồm:

AI Advisor.

Facebook Gateway.

Messenger/Web/Landing Page.

Ads Measurement.

MC AI Live.

## CRM.

Payment/Fulfillment.

MISA/Finance.

IVR nếu scope có.

Evidence/Gateway/Release.

## 50.2. Scope In

Commerce Handoff bao gồm:

Product can quote/order status.

QuoteSnapshot.

Quote expiry.

Final total.

Order Draft.

CustomerConfirmation status.

Official order status.

Payment status.

Paid status.

Verified revenue status.

Commission/KPI/Ads eligibility.

Blocking reason public-safe/internal-safe theo consumer.

Runtime unavailable status.

## 50.3. Scope Out

COM-M25 không chịu trách nhiệm:

Cho AI tự tính giá.

Cho Ads dùng quote làm revenue.

Cho MISA nhận missing mapping success.

Cho IVR xác nhận order bị lock/recall.

Cho CRM upsell SKU bị chặn.

Public dữ liệu nội bộ.

## 50.4. Upstream Dependency

Upstream

Điều kiện

## COM-M01 -> COM-M24

Commerce decisions

TECH-03 Operational Core

Sellable/lock/recall

TECH-01 Evidence/Audit

Handoff evidence

Downstream Policy

Consumer-specific view

## 50.5. Downstream Consumer

Consumer

Được nhận

AI Advisor

Quote/order safe view, final price từ QuoteSnapshot, public-safe blocking reason

Facebook Gateway

Quote/order handoff, CustomerConfirmation/CTA state

Ads

Verified revenue only, product suppression

MC AI Live

Sellable/quote-safe status, not internal price logic

## CRM

Suppression/repurchase eligibility, verified revenue if allowed

Payment

Official order/payment amount/reference

MISA/Finance

Verified order/payment/revenue data theo mapping

## IVR

Order confirmation eligibility nếu scope có

Evidence/Gateway

Smoke/evidence/status

## 50.6. P0 điểm chặn

COM-M25 FAIL nếu:

AI nhận dữ liệu để tự tính final price.

Gateway tạo order không CustomerConfirmation.

Ads nhận unverified revenue.

CRM upsell SKU bị chặn.

MISA nhận dữ liệu chưa đủ điều kiện.

IVR xác nhận order bị sale lock/recall.

Public-safe view lộ thông tin nội bộ.

Runtime unavailable nhưng handoff pass.

## 50.7. Evidence Required

Handoff contract evidence.

Consumer-specific view approval.

Blocking reason mapping.

QuoteSnapshot handoff evidence.

Verified revenue handoff evidence.

Downstream suppression evidence.

Audit.

## 50.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## COM-M25-SMK-001

AI hỏi giá

Nhận QuoteSnapshot, không tự tính

## COM-M25-SMK-002

Gateway tạo order chưa confirm

Bị chặn

## COM-M25-SMK-003

Ads nhận order unpaid

Không tính revenue

## COM-M25-SMK-004

CRM SKU sale lock

Suppressed

## COM-M25-SMK-005

MISA missing mapping

WAITING/reconcile

## 51. MA TRẬN PHỤ THUỘC MODULE COMMERCE RUNTIME

## 51.1. Dependency Matrix cấp module

Module

Phụ thuộc chính

Chặn downstream nếu fail

COM-M01 Product / SKU / Channel Scope

TECH-02 Product

Sellable / Quote

COM-M02 Operational Sellable Gate

TECH-03 Operational

Quote / Cart / Order

COM-M03 Listed Price

Price policy

Final quote

COM-M04 Program Price

Program policy

Program quote

COM-M05 Member Benefit

Member runtime

Member pricing

COM-M06 Diamond Referral

Referral resolver

Referral benefit / commission

COM-M07 Policy Priority

Pricing governance

Final price

COM-M08 QuoteSnapshot

M01-M07 + Shipping/VAT if needed

Cart / Draft / AI price

COM-M09 Quote Hold / Refresh

QuoteSnapshot + Time

Order creation

COM-M10 Cart

QuoteSnapshot

Order Draft

COM-M11 Customer Checkout Context

Customer/profile/shipping

Order Draft

COM-M12 Order Draft

Cart + Checkout + Payment

Customer Confirmation

COM-M13 Customer Confirmation

Order Draft + valid quote

Official Order

COM-M14 Official Order

CustomerConfirmation

Payment / Fulfillment

COM-M15 Payment Method

Payment policy

Payment flow

COM-M16 Bank Transfer

Bank Registry + Accounting

## PAID

## COM-M17 COD

COD/Shipping policy

Payment / Revenue

COM-M18 Shipping

Shipping policy

Draft / Order

COM-M19 VAT/Invoice

Tax/Finance policy

Invoice/MISA handoff

COM-M20 Order Status

Official Order

Verification / CRM

COM-M21 Payment Confirmation

Payment/Accounting

Paid status

COM-M22 Order Verification

Payment + Order valid

Verified Revenue

COM-M23 Commission/KPI/Ads Handoff

Verified Revenue

Ads/KPI/Commission

COM-M24 Suppression

Blocking states

All downstream

COM-M25 Downstream Handoff

Commerce decisions

AI/Gateway/Ads/Live/MISA/IVR

## 52. CROSS-MODULE P0 điểm chặn SUMMARY

## 52.1. P0 điểm chặn theo chuỗi Commerce

Chuỗi

P0 điểm chặn

Module bị ảnh hưởng

Product -> Commerce

SKU chưa approved vẫn quote

## COM-M01/COM-M08

Operational -> Commerce

Sale Lock/Recall vẫn order

## COM-M02/COM-M14

Price -> Quote

Missing listed price vẫn final quote

## COM-M03/COM-M08

Program -> Quote

Program inactive vẫn áp giá

## COM-M04/COM-M08

Member -> Quote

Member benefit không runtime-confirmed

## COM-M05/COM-M08

Referral -> Quote

Link không bind vẫn benefit

## COM-M06/COM-M08

Policy -> Final Price

Cộng dồn tùy tiện

## COM-M07/COM-M08

Quote -> Cart

Cart không QuoteSnapshot

## COM-M10

Quote -> Order

Quote expired vẫn tạo order

## COM-M09/COM-M14

Draft -> Order

Không CustomerConfirmation vẫn order_code

## COM-M13/COM-M14

Payment -> Paid

Ảnh chuyển khoản thành PAID

## COM-M16/COM-M21

COD -> Revenue

COD chưa thu vẫn verified

## COM-M17/COM-M22

Order -> Revenue

Unpaid/cancelled vẫn verified

## COM-M22

Revenue -> Ads/KPI

Quote/draft/unverified order tính ROAS

## COM-M23

Suppression -> Downstream

Runtime bị chặn nhưng AI/Ads/Live vẫn bán

## COM-M24/COM-M25

## 53. EVIDENCE PACKAGE CHO PHẦN 2/4

## 53.1. Evidence cấp module

Mỗi module COM-M01 -> COM-M25 phải có evidence tối thiểu:

Module owner approval.

Scope in/out approval.

Upstream/downstream mapping.

P0 điểm chặn mapping.

Smoke evidence.

Permission/audit evidence nếu module có command rủi ro.

Idempotency evidence nếu module tạo side-effect.

Runtime decision evidence nếu module quyết định giá/quyền lợi/order/payment.

Consumer-specific handoff evidence nếu có downstream.

Owner review accepted.

## 53.2. Evidence không được dùng để PASS nếu chưa accepted

Evidence chỉ được dùng cho Completion Decision khi có trạng thái:

## ACCEPTED

Không dùng:

Draft evidence.

Screenshot không reviewer.

Log không environment.

Test chưa pass.

Owner nói miệng.

Dev tự xác nhận.

Evidence không version.

Evidence không trace đến requirement.

## 54. SMOKE MATRIX CHO PHẦN 2/4

## 54.1. Smoke tổng hợp module contract

Smoke ID

Module

Nội dung

Expected Result

## COM-P2-SMK-001

Product Scope

SKU chưa approved

Không quote/order

## COM-P2-SMK-002

Operational Gate

Sale Lock active

Commerce bị chặn

## COM-P2-SMK-003

Listed Price

Missing price

Không final quote

## COM-P2-SMK-004

Program

Program inactive

Không áp giá

## COM-P2-SMK-005

Member

Member runtime unavailable

Không áp member benefit

## COM-P2-SMK-006

Referral

Link không bind

Không áp referral benefit

## COM-P2-SMK-007

Policy Priority

Conflict quyền lợi

Runtime quyết định, không cộng tùy tiện

## COM-P2-SMK-008

QuoteSnapshot

Runtime pass

Tạo QuoteSnapshot

## COM-P2-SMK-009

Quote Hold

Quote expired

Không tạo order

## COM-P2-SMK-010

Cart

Cart không QuoteSnapshot

Bị chặn

## COM-P2-SMK-011

Checkout Context

Thiếu thông tin nhận hàng

Draft incomplete

## COM-P2-SMK-012

Order Draft

Thiếu payment method

Draft incomplete

## COM-P2-SMK-013

Customer Confirmation

Chưa confirm

Không official order

## COM-P2-SMK-014

Official Order

Confirm valid

Tạo order_code

## COM-P2-SMK-015

Payment Method

Bank info hardcoded

## COM-P2-SMK-016

Bank Transfer

Ảnh chuyển khoản

Không PAID

## COM-P2-SMK-017

## COD

COD chưa thu

Không verified revenue

## COM-P2-SMK-018

Shipping

Khu vực không giao

Block order

## COM-P2-SMK-019

VAT/Invoice

Public invoice data

Bị chặn

## COM-P2-SMK-020

Order Status

Cancelled order

Không verified revenue

## COM-P2-SMK-021

Payment Confirmation

Amount mismatch

Review/bị chặn

## COM-P2-SMK-022

Verified Revenue

Unpaid order

Không verified

## COM-P2-SMK-023

Ads/KPI/Commission

Unverified order

Không handoff

## COM-P2-SMK-024

Suppression

Runtime unavailable

Commerce bị chặn

## COM-P2-SMK-025

Downstream Handoff

AI hỏi giá

Chỉ nhận QuoteSnapshot

## 55. DONE GATE CỦA PHẦN 2/4

## 55.1. Điều kiện Done Gate

## PHẦN 2/4 chỉ được xem là DONE khi:

Tất cả module COM-M01 đến COM-M25 đã có contract.

Mỗi module có scope in rõ.

Mỗi module có scope out rõ.

Mỗi module có upstream dependency rõ.

Mỗi module có downstream consumer rõ.

Mỗi module có P0 điểm chặn rõ.

Mỗi module có evidence requirement rõ.

Mỗi module có smoke requirement rõ.

Cross-module dependency matrix đã có.

Cross-module P0 điểm chặn summary đã có.

QuoteSnapshot là nguồn giá duy nhất.

CustomerConfirmation là điều kiện tạo official order.

Payment confirmation là điều kiện PAID.

Order verification là điều kiện verified revenue.

Verified revenue là điều kiện Ads/KPI/commission.

Bank information không được hardcode.

COD không tự thêm phí nếu chưa có policy.

Runtime unavailable phải fail-safe.

Downstream không được bypass Commerce Runtime.

Không gọi Documentation Complete là Production Ready.

## 56. FAIL GATE CỦA PHẦN 2/4

## 56.1. Điều kiện làm PHẦN 2 fail

## PHẦN 2/4 FAIL nếu có bất kỳ điểm nào sau:

Module contract thiếu scope out.

Module contract thiếu upstream/downstream.

Module contract không có P0 điểm chặn.

Product Active/SKU Active tự thành orderable.

Operational bị chặn nhưng Commerce vẫn quote/order.

Missing listed price vẫn final quote.

Program inactive vẫn áp giá.

Member benefit áp khi runtime chưa xác nhận.

Diamond referral benefit áp khi link chưa bind.

Quyền lợi bị cộng dồn tùy tiện.

Không QuoteSnapshot vẫn báo final price.

Quote expired vẫn tạo order.

Cart không QuoteSnapshot.

Order Draft thiếu 3 phần bắt buộc.

Không CustomerConfirmation vẫn tạo order_code.

Ảnh chuyển khoản được gắn PAID.

COD chưa thu vẫn verified revenue.

Shipping thiếu policy vẫn cộng phí.

VAT/invoice public dữ liệu riêng tư.

Unpaid/cancelled/refunded order vẫn verified revenue.

Quote/draft/unverified order được đưa vào Ads ROAS/KPI/commission.

Bank account hardcoded.

Runtime unavailable vẫn pass.

Downstream nhận dữ liệu để tự tính giá hoặc tự xác nhận revenue.

## 57. TRẠNG THÁI SAU PHẦN 2/4

## 57.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## TECH-04 PHẦN 2/4

## WRITTEN_FOR_REVIEW

Commerce Module Contracts

## LOCKED_DRAFT

Module Scope In/Out

## DEFINED

Upstream/Downstream Mapping

## DEFINED

P0 điểm chặn Mapping

## DEFINED

Module Evidence Requirement

## DEFINED

Module Smoke Requirement

## DEFINED

Cross-Module Dependency Matrix

## DEFINED

Implementation Status

## WAITING

Test Status

## WAITING

Smoke Status

## WAITING

Evidence Status

## WAITING

Release Ready

KHONG

Production Ready

KHONG

Go-live Approved

KHONG

## 58. KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 đã khóa contract cho từng module trong Commerce Runtime.

Từ đây trở đi, Commerce không được triển khai như một giỏ hàng đơn giản hoặc một bảng đơn hàng rời rạc.

Commerce Runtime là chuỗi quyết định thương mại liên kết chặt chẽ:

Product Scope -> Operational Sellable Gate -> Listed Price -> Program Price -> Member Benefit -> Diamond Referral -> Policy Priority -> QuoteSnapshot -> Quote Hold -> Cart -> Customer Checkout Context -> Order Draft -> Customer Confirmation -> Official Order -> Payment Method -> Payment Confirmation -> Order Verification -> Verified Revenue -> Ads/KPI/Commission Handoff -> Commerce Suppression -> Downstream Handoff.

Mỗi module có owner, boundary, dependency, điểm chặn, evidence và smoke riêng.

Không module nào được tự ý thay vai trò của module khác.

Không downstream nào được vượt Commerce Runtime để báo giá, tạo order, xác nhận thanh toán hoặc ghi nhận doanh thu.

## PHẦN 2/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 3/4 - COMMERCE LIFECYCLE / STATE MACHINE / COMMAND-QUERY BOUNDARY / QUOTE-ORDER-PAYMENT-REVENUE HANDOFF

## 59. MỤC TIÊU CỦA PHẦN 3/4

## 59.1. Vai trò của PHẦN 3/4

## PHẦN 3/4 khóa vòng đời runtime thương mại của Ginsengfood.

PHẦN 2 đã khóa module contract.

PHẦN 3 khóa tiếp:

State model của từng đối tượng Commerce quan trọng.

Luồng chuyển trạng thái hợp lệ.

Luồng chuyển trạng thái bị cấm.

Command nào được phép thay đổi trạng thái.

Query nào chỉ được đọc dữ liệu.

QuoteSnapshot được tạo, hết hạn, refresh, supersede như thế nào.

Cart chuyển sang Order Draft như thế nào.

CustomerConfirmation được ghi nhận như thế nào.

Official Order được tạo khi nào.

Payment WAITING / Payment Review / PAID được xác định như thế nào.

Order Verified / Verified Revenue được xác định như thế nào.

Handoff sang AI, Facebook Gateway, Ads, Live, CRM, MISA, IVR.

Runtime unavailable thì fail-safe ra sao.

## 59.2. Nguyên tắc quan trọng

Trong TECH-04, state machine là lõi kiểm soát thương mại.

Nếu không có state machine rõ, hệ thống sẽ dễ sai ở các điểm nguy hiểm:

Báo giá sai.

Giữ giá sai thời hạn.

Chốt đơn khi quote đã hết hạn.

Tạo order khi khách chưa xác nhận.

Gắn PAID khi chưa xác minh thanh toán.

Tính doanh thu khi đơn chưa verified.

Tính hoa hồng/ROAS/KPI từ đơn chưa đủ điều kiện.

AI/Gateway/Live tự tính giá.

Commerce vượt Sale Lock / Recall của Operational Core.

## 60. NGUYÊN TẮC STATE MACHINE CỦA COMMERCE RUNTIME

## 60.1. Mọi trạng thái phải có owner

Mỗi state trong Commerce Runtime phải có owner rõ ràng.

State

Owner đúng

## PRODUCT_SCOPE_PASS

Product / Commerce Scope

## OPERATIONAL_SELLABLE_PASS

Operational Core -> Commerce Gate

## LISTED_PRICE_ACTIVE

Listed Price Runtime

## PROGRAM_ACTIVE

Program Runtime

## MEMBER_BENEFIT_ELIGIBLE

Member Runtime / Commerce Eligibility

## DIAMOND_REFERRAL_ELIGIBLE

Referral Resolver / Commerce Eligibility

## POLICY_PRIORITY_DECIDED

Commerce Policy Runtime

## QUOTE_ACTIVE

QuoteSnapshot Runtime

## QUOTE_EXPIRED

Quote Hold Runtime

## CART_ACTIVE

Cart Runtime

## ORDER_DRAFT_READY

Order Draft Runtime

## CUSTOMER_CONFIRMED

Customer Confirmation Runtime

## ORDER_CREATED

Official Order Runtime

## PAYMENT_CONFIRMED

Payment Core / Accounting Review

## ORDER_VERIFIED

Order Verification Runtime

## REVENUE_VERIFIED

Commerce / Finance Verification

## COMMISSION_ELIGIBLE

Commission/KPI Policy after Verified Revenue

## 60.2. Không module nào được tự ghi state của module khác

Quy tắc:

AI không được tự tạo QuoteSnapshot.

Gateway không được tự tạo official order nếu chưa CustomerConfirmation.

Live không được tự tính final price.

CRM không được tự xác định verified revenue.

Ads không được tự lấy quote/order draft làm doanh thu.

Payment không được tự tạo order.

Commerce không được tự gắn PAID nếu Payment Core/Accounting Review chưa xác nhận.

Commerce không được tự bỏ Sale Lock / Recall từ Operational Core.

MISA không được ghi ngược để sửa Commerce truth nếu chưa có governance.

## 60.3. Mọi state transition phải có trace

Mọi command chuyển trạng thái quan trọng phải ghi nhận tối thiểu:

State trước.

State sau.

Actor hoặc system actor.

Runtime source.

Timestamp.

Channel.

Reason nếu bị chặn/cancel/refund/reject.

QuoteSnapshot reference nếu liên quan giá.

Order reference nếu liên quan đơn.

Payment reference nếu liên quan thanh toán.

Evidence reference nếu dùng cho gate/release.

Audit/correlation nếu có.

## 60.4. Không sửa lịch sử thương mại bằng cách ghi đè

Không được sửa trực tiếp:

QuoteSnapshot đã phát hành.

Order Draft đã xác nhận.

Official Order đã tạo.

Payment confirmation đã ghi nhận.

Verified Revenue record.

Commission eligibility record.

Ads revenue attribution record.

Nếu cần thay đổi phải dùng:

Supersede quote.

Cancel order.

Refund/reversal nếu policy có.

Payment adjustment.

Revenue reversal.

Commission exclusion.

New evidence record.

Owner approval.

## 61. COMMERCE LIFECYCLE TỔNG THỂ

## 61.1. Chuỗi lifecycle chuẩn

Chuỗi Commerce Runtime chuẩn:

PRODUCT_SCOPE_CHECK -> OPERATIONAL_SELLABLE_CHECK -> LISTED_PRICE_CHECK -> PROGRAM_CHECK -> MEMBER_BENEFIT_CHECK -> DIAMOND_REFERRAL_CHECK -> POLICY_PRIORITY_DECISION -> QUOTE_SNAPSHOT_CREATED -> QUOTE_ACTIVE -> CART_ACTIVE -> CHECKOUT_CONTEXT_READY -> ORDER_DRAFT_READY -> CUSTOMER_REVIEWING -> CUSTOMER_CONFIRMED -> OFFICIAL_ORDER_CREATED -> PAYMENT_WAITING -> PAYMENT_REVIEW / PAYMENT_CONFIRMED -> ORDER_VERIFICATION_WAITING -> ORDER_VERIFIED -> REVENUE_VERIFIED -> DOWNSTREAM_HANDOFF.

## 61.2. Lifecycle có thể bị chặn bởi

Commerce lifecycle phải bị chặn nếu có một trong các trạng thái:

## PRODUCT_NOT_APPROVED.

## NOT_IN_CHANNEL_SCOPE.

## OPERATIONAL_BLOCKED.

## SALE_LOCK_ACTIVE.

## RECALL_ACTIVE.

## NO_AVAILABLE_STOCK.

## LISTED_PRICE_MISSING.

## LISTED_PRICE_INACTIVE.

## PROGRAM_CONFLICT.

## POLICY_PRIORITY_MISSING.

## MEMBER_RUNTIME_UNAVAILABLE.

## REFERRAL_BIND_INVALID.

## QUOTE_EXPIRED.

## ORDER_DRAFT_INCOMPLETE.

## CUSTOMER_CONFIRMATION_MISSING.

## PAYMENT_METHOD_INVALID.

## PAYMENT_WAITING.

## PAYMENT_REVIEW.

## PAYMENT_FAILED.

## ORDER_CANCELLED.

## COD_FAILED.

## REVENUE_NOT_VERIFIED.

## RUNTIME_UNAVAILABLE.

## EVIDENCE_NOT_ACCEPTED.

## 61.3. Lifecycle không đồng nghĩa doanh thu

Không được hiểu nhầm:

Product pass không phải sellable.

Operational pass không phải Commerce pass.

Listed price active không phải final price.

Quote active không phải order.

Cart active không phải order.

Order Draft ready không phải official order.

Customer nói “mua” chưa phải CustomerConfirmation hợp lệ.

Official Order chưa phải PAID.

PAID chưa chắc Verified Revenue nếu order verification chưa pass.

Verified Revenue mới được dùng cho Ads ROAS/KPI/commission.

## 62. QUY ƯỚC STATE MODEL TRONG TECH-04

## 62.1. Nhóm state chuẩn

Nhóm state

Ý nghĩa

## DRAFT

Mới tạo, chưa đủ điều kiện

## WAITING

Chờ xử lý

## CHECKING

Đang kiểm tra runtime

Đạt điều kiện

Không đạt

bị chặn

Bị chặn

## ACTIVE

Đang hiệu lực

## EXPIRED

Hết hiệu lực

## SUPERSEDED

Bị thay thế bởi bản mới

## CONFIRMED

Đã xác nhận

## CREATED

Đã tạo chính thức

## REVIEW

Đang chờ kiểm tra

## VERIFIED

Đã xác minh

## CANCELLED

Đã hủy

## REFUNDED

Đã hoàn tiền nếu scope có

Đã đóng

## ERROR

Lỗi runtime hoặc lỗi xử lý

## 62.2. Các state dễ nhầm phải tách rõ

State

Không đồng nghĩa

## PRODUCT_ACTIVE

Không đồng nghĩa SELLABLE

## OPERATIONAL_SELLABLE_PASS

Không đồng nghĩa COMMERCE_ORDERABLE

## LISTED_PRICE_ACTIVE

Không đồng nghĩa FINAL_PRICE

## PROGRAM_ACTIVE

Không đồng nghĩa FINAL_PRICE

## MEMBER_ELIGIBLE

Không tự cộng vào giá nếu Policy Priority chưa pass

## REFERRAL_ELIGIBLE

Không tự cộng vào giá nếu Policy Priority chưa pass

## QUOTE_ACTIVE

Không đồng nghĩa khách đã mua

## CART_ACTIVE

Không đồng nghĩa official order

## ORDER_DRAFT_READY

Không đồng nghĩa order_code

## CUSTOMER_CONFIRMED

Chỉ hợp lệ nếu draft/quote còn hiệu lực

## PAYMENT_WAITING

Không đồng nghĩa PAID

## PAID

Không đồng nghĩa Verified Revenue nếu verification chưa pass

## ORDER_VERIFIED

Mới là cơ sở xét Verified Revenue

## VERIFIED_REVENUE

Mới là nguồn Ads/KPI/commission

## 63. PRODUCT / SKU / CHANNEL SCOPE STATE MODEL

## 63.1. Mục tiêu

State model này kiểm soát sản phẩm/SKU có được đưa vào luồng Commerce hay không.

## 63.2. Trạng thái chuẩn

State

Ý nghĩa

## PRODUCT_SCOPE_NOT_CHECKED

Chưa kiểm tra

## PRODUCT_SCOPE_CHECKING

Đang kiểm tra Product/SKU/channel

## PRODUCT_SCOPE_PASS

Product/SKU/channel hợp lệ

## PRODUCT_SCOPE_BLOCKED

Bị chặn

## PRODUCT_NOT_APPROVED

Product/SKU chưa approved

## PRODUCT_NOT_IN_CHANNEL_SCOPE

Không thuộc scope kênh

## PRODUCT_KNOWLEDGE_NOT_APPROVED

Chưa được dùng public/AI/Live

## PRODUCT_SCOPE_RUNTIME_UNAVAILABLE

Không xác minh được Product Runtime

## 63.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PRODUCT_SCOPE_NOT_CHECKED

## PRODUCT_SCOPE_CHECKING

Có yêu cầu quote/cart/order

## PRODUCT_SCOPE_CHECKING

## PRODUCT_SCOPE_PASS

Product/SKU/channel pass

## PRODUCT_SCOPE_CHECKING

## PRODUCT_NOT_APPROVED

Product/SKU chưa approved

## PRODUCT_SCOPE_CHECKING

## PRODUCT_NOT_IN_CHANNEL_SCOPE

Không thuộc kênh

## PRODUCT_SCOPE_CHECKING

## PRODUCT_KNOWLEDGE_NOT_APPROVED

Public/AI content chưa approved

## PRODUCT_SCOPE_CHECKING

## PRODUCT_SCOPE_RUNTIME_UNAVAILABLE

Runtime lỗi

## PRODUCT_SCOPE_BLOCKED

## PRODUCT_SCOPE_CHECKING

Có refresh/recheck hợp lệ

## 63.4. Chuyển trạng thái bị cấm

Không được:

PRODUCT_NOT_APPROVED -> QuoteSnapshot.

PRODUCT_NOT_IN_CHANNEL_SCOPE -> Order Draft.

PRODUCT_KNOWLEDGE_NOT_APPROVED -> AI/Live public content.

PRODUCT_SCOPE_RUNTIME_UNAVAILABLE -> assume pass.

Product Active tự động thành Commerce orderable.

## 63.5. P0 điểm chặn

State model này FAIL nếu:

SKU chưa approved vẫn quote/order.

Product active nhưng không channel scope vẫn bán.

Public Product Knowledge chưa approved nhưng AI/Live dùng.

Product Runtime unavailable nhưng Commerce vẫn pass.

## 64. OPERATIONAL SELLABLE GATE STATE MODEL

## 64.1. Mục tiêu

State model này kiểm soát Commerce có được tiếp tục xét giá/order dựa trên Operational Core hay không.

## 64.2. Trạng thái chuẩn

State

Ý nghĩa

## OP_GATE_NOT_CHECKED

Chưa kiểm Operational

## OP_GATE_CHECKING

Đang kiểm

## OP_GATE_PASS

Operational pass

## OP_GATE_BLOCKED

Operational bị chặn

## OP_GATE_NO_STOCK

Không có tồn khả dụng

## OP_GATE_BATCH_NOT_RELEASED

Batch chưa release

## OP_GATE_WAREHOUSE_MISSING

Chưa warehouse receipt

## OP_GATE_RECALL_ACTIVE

Recall active

## OP_GATE_SALE_LOCK_ACTIVE

Sale Lock active

## OP_GATE_QUALITY_HOLD

Quality hold

## OP_GATE_TRACE_MISSING

Thiếu trace nếu policy yêu cầu

## OP_GATE_RUNTIME_UNAVAILABLE

Operational Runtime lỗi

## 64.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## OP_GATE_NOT_CHECKED

## OP_GATE_CHECKING

Có yêu cầu quote/cart/order

## OP_GATE_CHECKING

## OP_GATE_PASS

Operational Sellable pass

## OP_GATE_CHECKING

## OP_GATE_NO_STOCK

Không có tồn khả dụng

## OP_GATE_CHECKING

## OP_GATE_BATCH_NOT_RELEASED

Batch chưa release

## OP_GATE_CHECKING

## OP_GATE_RECALL_ACTIVE

Recall active

## OP_GATE_CHECKING

## OP_GATE_SALE_LOCK_ACTIVE

Sale Lock active

## OP_GATE_CHECKING

## OP_GATE_RUNTIME_UNAVAILABLE

Runtime lỗi

## OP_GATE_BLOCKED

## OP_GATE_CHECKING

Có recheck hợp lệ

## 64.4. Chuyển trạng thái bị cấm

Không được:

OP_GATE_BLOCKED -> QuoteSnapshot.

OP_GATE_RECALL_ACTIVE -> Order Draft.

OP_GATE_SALE_LOCK_ACTIVE -> Customer Confirmation.

OP_GATE_RUNTIME_UNAVAILABLE -> assume sellable.

OP_GATE_NO_STOCK -> AI nói còn hàng.

OP_GATE_BATCH_NOT_RELEASED -> Commerce orderable.

## 64.5. P0 điểm chặn

Operational Gate FAIL nếu:

Commerce quote/order khi Operational bị chặn.

Sale Lock/Recall không chặn Commerce.

Runtime unavailable vẫn pass.

Commerce dùng Product Active thay Operational Sellable.

## 65. LISTED PRICE STATE MODEL

## 65.1. Mục tiêu

Listed Price State Model kiểm soát giá niêm yết trước khi tạo final quote.

## 65.2. Trạng thái chuẩn

State

Ý nghĩa

## PRICE_NOT_CHECKED

Chưa kiểm giá

## PRICE_CHECKING

Đang kiểm giá

## PRICE_ACTIVE

Giá active

## PRICE_INACTIVE

Giá inactive

## PRICE_MISSING

Thiếu giá

## PRICE_EXPIRED

Giá hết hiệu lực

## PRICE_CONFLICT

Nhiều giá active xung đột

## PRICE_WAITING_APPROVAL

Giá chưa owner approved

## PRICE_RUNTIME_UNAVAILABLE

Không xác minh được giá

## 65.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PRICE_NOT_CHECKED

## PRICE_CHECKING

Có yêu cầu quote

## PRICE_CHECKING

## PRICE_ACTIVE

Giá active và approved

## PRICE_CHECKING

## PRICE_MISSING

Không có giá

## PRICE_CHECKING

## PRICE_INACTIVE

Giá inactive

## PRICE_CHECKING

## PRICE_EXPIRED

Giá hết hạn

## PRICE_CHECKING

## PRICE_CONFLICT

Có xung đột

## PRICE_CHECKING

## PRICE_WAITING_APPROVAL

Chưa duyệt

## PRICE_CHECKING

## PRICE_RUNTIME_UNAVAILABLE

Runtime lỗi

## 65.4. Chuyển trạng thái bị cấm

Không được:

PRICE_MISSING -> QuoteSnapshot final.

PRICE_INACTIVE -> QuoteSnapshot final.

PRICE_CONFLICT -> AI báo giá.

PRICE_RUNTIME_UNAVAILABLE -> dùng giá cũ nếu không có policy.

Giá trong content/Live thay cho Listed Price Runtime.

## 65.5. P0 điểm chặn

Listed Price FAIL nếu:

Missing price vẫn final quote.

Nhiều price active xung đột vẫn quote.

AI/Gateway/Live tự hardcode giá.

Giá chưa owner approved vẫn bán.

## 66. PROGRAM PRICE STATE MODEL

## 66.1. Mục tiêu

Program Price State Model kiểm soát chương trình giá tại thời điểm quote.

## 66.2. Trạng thái chuẩn

State

Ý nghĩa

## PROGRAM_NOT_CHECKED

Chưa kiểm chương trình

## PROGRAM_CHECKING

Đang kiểm

## PROGRAM_ACTIVE

Chương trình active

## PROGRAM_NOT_APPLICABLE

Không áp dụng

## PROGRAM_EXPIRED

Chương trình hết hạn

## PROGRAM_NOT_IN_CHANNEL

Không thuộc kênh

## PROGRAM_SKU_NOT_IN_SCOPE

SKU không thuộc chương trình

## PROGRAM_CONFLICT

Xung đột chương trình

## PROGRAM_RUNTIME_UNAVAILABLE

Runtime lỗi

## 66.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PROGRAM_NOT_CHECKED

## PROGRAM_CHECKING

Có yêu cầu quote

## PROGRAM_CHECKING

## PROGRAM_ACTIVE

Program pass

## PROGRAM_CHECKING

## PROGRAM_NOT_APPLICABLE

Không có program phù hợp

## PROGRAM_CHECKING

## PROGRAM_EXPIRED

Hết thời gian

## PROGRAM_CHECKING

## PROGRAM_NOT_IN_CHANNEL

Kênh không thuộc program

## PROGRAM_CHECKING

## PROGRAM_SKU_NOT_IN_SCOPE

SKU không thuộc program

## PROGRAM_CHECKING

## PROGRAM_CONFLICT

Nhiều chương trình xung đột

## PROGRAM_CHECKING

## PROGRAM_RUNTIME_UNAVAILABLE

Runtime lỗi

## 66.4. Rule đặc biệt Giờ Vàng và 24/7

Commerce Runtime phải xác định chính xác chương trình:

Chương trình

Rule runtime

Giờ Vàng

Phải có session active, quote giữ giá 5 phút

24/7

Quote giữ giá 15 phút, rule giá theo runtime/ngày/chương trình

Chương trình khác

Chỉ dùng khi owner approved và runtime có policy

Không được để AI/Live tự xác định program nếu Runtime chưa pass.

## 66.5. P0 điểm chặn

Program Price FAIL nếu:

Program expired vẫn áp.

SKU ngoài program vẫn áp.

Giờ Vàng không session active vẫn áp giá Giờ Vàng.

24/7 price bị AI tự tính.

Program conflict nhưng vẫn final quote.

## 67. MEMBER BENEFIT STATE MODEL

## 67.1. Mục tiêu

Member Benefit State Model kiểm soát quyền lợi thành viên.

## 67.2. Trạng thái chuẩn

State

Ý nghĩa

## MEMBER_NOT_CHECKED

Chưa kiểm member

## MEMBER_CHECKING

Đang kiểm

## MEMBER_IDENTIFIED

Đã xác định khách/member

## MEMBER_NOT_FOUND

Không phải member hoặc chưa xác định

## MEMBER_TIER_VALID

Hạng hợp lệ

## MEMBER_TIER_INVALID

Hạng không hợp lệ

## MEMBER_CYCLE_ACTIVE

Chu kỳ còn hiệu lực

## MEMBER_GRACE_ACTIVE

Đang trong grace period

## MEMBER_CYCLE_EXPIRED

Chu kỳ hết hiệu lực

## MEMBER_BENEFIT_ELIGIBLE

Đủ điều kiện benefit

## MEMBER_BENEFIT_BLOCKED

Không đủ điều kiện

## MEMBER_RUNTIME_UNAVAILABLE

Runtime lỗi

## 67.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## MEMBER_NOT_CHECKED

## MEMBER_CHECKING

Có customer identity

## MEMBER_CHECKING

## MEMBER_IDENTIFIED

Xác định được member

## MEMBER_CHECKING

## MEMBER_NOT_FOUND

Không xác định member

## MEMBER_IDENTIFIED

## MEMBER_TIER_VALID

Hạng hợp lệ

## MEMBER_IDENTIFIED

## MEMBER_TIER_INVALID

Hạng sai/không hợp lệ

## MEMBER_TIER_VALID

## MEMBER_CYCLE_ACTIVE

Chu kỳ active

## MEMBER_TIER_VALID

## MEMBER_GRACE_ACTIVE

Grace active

## MEMBER_TIER_VALID

## MEMBER_CYCLE_EXPIRED

Hết chu kỳ

## MEMBER_CYCLE_ACTIVE/GRACE_ACTIVE

## MEMBER_BENEFIT_ELIGIBLE

Policy cho phép

## MEMBER_CYCLE_EXPIRED

## MEMBER_BENEFIT_BLOCKED

Hết hiệu lực

## MEMBER_CHECKING

## MEMBER_RUNTIME_UNAVAILABLE

Runtime lỗi

## 67.4. Chuyển trạng thái bị cấm

Không được:

## MEMBER_NOT_FOUND -> MEMBER_BENEFIT_ELIGIBLE.

## MEMBER_RUNTIME_UNAVAILABLE -> MEMBER_BENEFIT_ELIGIBLE.

Khách tự khai hạng -> MEMBER_TIER_VALID.

MEMBER_CYCLE_EXPIRED -> benefit nếu grace không pass.

Member benefit tự cộng vào final price nếu Policy Priority chưa quyết định.

## 67.5. P0 điểm chặn

Member Benefit FAIL nếu:

AI hỏi khách tự khai hạng rồi tính giá.

Runtime unavailable vẫn áp benefit.

Chu kỳ hết hiệu lực vẫn áp benefit.

Program không cho member benefit nhưng vẫn áp.

Member benefit tự cộng dồn với quyền lợi khác ngoài policy.

## 68. DIAMOND REFERRAL STATE MODEL

## 68.1. Mục tiêu

Diamond Referral State Model kiểm soát quyền lợi buyer_from_diamond_link và attribution/commission sau này.

## 68.2. Trạng thái chuẩn

State

Ý nghĩa

## REFERRAL_NOT_CHECKED

Chưa kiểm referral

## REFERRAL_CHECKING

Đang kiểm

## REFERRAL_LINK_DETECTED

Có dấu hiệu link referral

## REFERRAL_LINK_VALID

Link hợp lệ

## REFERRAL_LINK_INVALID

Link không hợp lệ

## REFERRAL_BIND_WAITING

Chờ bind

## REFERRAL_BOUND

Bind hợp lệ

## REFERRAL_BIND_FAILED

Bind fail

## REFERRAL_OWNER_ELIGIBLE

Owner đủ điều kiện

## REFERRAL_OWNER_INELIGIBLE

Owner không đủ điều kiện

## BUYER_FROM_DIAMOND_LINK_ELIGIBLE

Buyer đủ điều kiện quyền lợi

## BUYER_FROM_DIAMOND_LINK_BLOCKED

Không đủ điều kiện

## REFERRAL_RUNTIME_UNAVAILABLE

Runtime lỗi

## 68.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## REFERRAL_NOT_CHECKED

## REFERRAL_CHECKING

Có referral context

## REFERRAL_CHECKING

## REFERRAL_LINK_DETECTED

Có link/referral token

## REFERRAL_LINK_DETECTED

## REFERRAL_LINK_VALID

Link valid

## REFERRAL_LINK_DETECTED

## REFERRAL_LINK_INVALID

Link invalid

## REFERRAL_LINK_VALID

## REFERRAL_BIND_WAITING

Chờ bind

## REFERRAL_BIND_WAITING

## REFERRAL_BOUND

Bind hợp lệ

## REFERRAL_BIND_WAITING

## REFERRAL_BIND_FAILED

Bind fail

## REFERRAL_BOUND

## REFERRAL_OWNER_ELIGIBLE

Owner đủ điều kiện

## REFERRAL_BOUND

## REFERRAL_OWNER_INELIGIBLE

Owner không đủ điều kiện

## REFERRAL_OWNER_ELIGIBLE

## BUYER_FROM_DIAMOND_LINK_ELIGIBLE

Program/policy cho phép

## REFERRAL_OWNER_INELIGIBLE

## BUYER_FROM_DIAMOND_LINK_BLOCKED

Block

## REFERRAL_CHECKING

## REFERRAL_RUNTIME_UNAVAILABLE

Runtime lỗi

## 68.4. Chuyển trạng thái bị cấm

Không được:

REFERRAL_LINK_DETECTED -> benefit nếu chưa BOUND.

REFERRAL_LINK_INVALID -> buyer benefit.

REFERRAL_BIND_FAILED -> commission.

REFERRAL_OWNER_INELIGIBLE -> buyer benefit.

Buyer tự nói “tôi vào link Diamond” -> benefit nếu không có bind.

AI/Gateway tự áp 5% nếu resolver chưa pass.

## 68.5. P0 điểm chặn

Diamond Referral FAIL nếu:

No bind vẫn áp quyền lợi.

Owner không đủ điều kiện vẫn tính benefit/commission.

Self-purchase bị cấm nhưng vẫn tính commission.

Referral runtime unavailable vẫn áp benefit.

Referral attribution dùng cho payout khi order chưa verified.

## 69. POLICY PRIORITY STATE MODEL

## 69.1. Mục tiêu

Policy Priority State Model quyết định quyền lợi nào được áp dụng vào final quote.

## 69.2. Trạng thái chuẩn

State

Ý nghĩa

## POLICY_NOT_EVALUATED

Chưa xét policy

## POLICY_EVALUATING

Đang xét

## POLICY_DECIDED

Đã quyết định

## POLICY_CONFLICT

Có xung đột

## POLICY_BLOCKED

Không thể quyết định

## POLICY_RUNTIME_UNAVAILABLE

Runtime lỗi

## POLICY_SUPERSEDED

Quyết định cũ bị thay thế bởi quote mới

## 69.3. Input policy

Policy Priority nhận input từ:

Listed Price.

Program Price.

Member Benefit.

Diamond Referral Benefit.

Channel Policy.

Program Policy.

Customer Context.

Order Context.

Owner Business Rule.

## 69.4. Output policy

Policy Priority tạo output:

Applied benefit.

Rejected benefit.

Reason.

Final pricing basis.

Decision reference.

Audit trail.

## 69.5. Chuyển trạng thái bị cấm

Không được:

POLICY_CONFLICT -> QuoteSnapshot.

POLICY_RUNTIME_UNAVAILABLE -> final price.

AI tự chọn quyền lợi.

Gateway tự cộng dồn.

Live tự nói “được thêm giảm” nếu policy chưa pass.

Rejected benefit không có reason.

## 69.6. P0 điểm chặn

Policy Priority FAIL nếu:

Không có priority rule vẫn quote.

Cộng dồn tùy tiện.

Final price không trace được policy.

Runtime unavailable vẫn quyết định.

Benefit rejected không lưu reason.

## 70. QUOTESNAPSHOT STATE MODEL

## 70.1. Mục tiêu

QuoteSnapshot State Model kiểm soát báo giá duy nhất, có thời hạn và có khả năng truy vết.

## 70.2. Trạng thái QuoteSnapshot chuẩn

State

Ý nghĩa

## QUOTE_NOT_CREATED

Chưa có quote

## QUOTE_CREATING

Đang tạo

## QUOTE_ACTIVE

Quote còn hiệu lực

## QUOTE_EXPIRED

Quote hết hạn

## QUOTE_SUPERSEDED

Quote bị thay thế bởi quote mới

## QUOTE_CANCELLED

Quote bị hủy theo policy

## QUOTE_BLOCKED

Không tạo được quote

## QUOTE_USED_FOR_DRAFT

Đã dùng tạo Order Draft

## QUOTE_USED_FOR_ORDER

Đã dùng tạo Official Order

## QUOTE_DISPUTE_REVIEW

Được đưa vào review khiếu nại

## QUOTE_CLOSED

Đóng vòng đời quote

## 70.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## QUOTE_NOT_CREATED

## QUOTE_CREATING

Runtime request quote

## QUOTE_CREATING

## QUOTE_ACTIVE

Product/Operational/Price/Policy pass

## QUOTE_CREATING

## QUOTE_BLOCKED

Có điểm chặn

## QUOTE_ACTIVE

## QUOTE_EXPIRED

Quá hold window

## QUOTE_ACTIVE

## QUOTE_USED_FOR_DRAFT

Order Draft tạo từ quote

## QUOTE_USED_FOR_DRAFT

## QUOTE_USED_FOR_ORDER

Customer confirm và order created

## QUOTE_EXPIRED

## QUOTE_SUPERSEDED

Refresh tạo quote mới

## QUOTE_ACTIVE

## QUOTE_SUPERSEDED

Refresh trước expiry nếu policy cho phép

## QUOTE_ACTIVE

## QUOTE_CANCELLED

Runtime điểm chặn mới xuất hiện

## QUOTE_USED_FOR_ORDER

## QUOTE_CLOSED

Order lifecycle đóng

## 70.4. Quote hold window

Chương trình

Quote Active Window

Giờ Vàng

5 phút

24/7

15 phút

Khác

Theo runtime policy approved

## 70.5. Chuyển trạng thái bị cấm

Không được:

QUOTE_EXPIRED -> Official Order.

QUOTE_BLOCKED -> Order Draft.

QUOTE_ACTIVE không expiry.

QUOTE_SUPERSEDED bị sửa lại thành active không governance.

Quote cũ bị ghi đè giá mới.

AI sử dụng quote expired để chốt đơn.

Quote không có Operational pass vẫn active.

## 70.6. P0 điểm chặn

QuoteSnapshot FAIL nếu:

No QuoteSnapshot vẫn báo final price.

Quote expired vẫn tạo order.

Quote không có expiry.

Quote không trace được runtime decisions.

Quote bị sửa trực tiếp sau khi tạo.

Quote pass khi Sale Lock/Recall active.

## 71. CART STATE MODEL

## 71.1. Mục tiêu

Cart State Model kiểm soát giỏ hàng trước Order Draft.

## 71.2. Trạng thái Cart chuẩn

State

Ý nghĩa

## CART_EMPTY

Chưa có sản phẩm

## CART_ACTIVE

Có sản phẩm hợp lệ

## CART_VALIDATING

Đang validate

## CART_VALID

Cart hợp lệ

## CART_INVALID

Cart không hợp lệ

## CART_NEEDS_QUOTE_REFRESH

Có quote expired

## CART_BLOCKED

Bị chặn

## CART_CONVERTED_TO_DRAFT

Đã chuyển sang Order Draft

## CART_ABANDONED

Khách bỏ dở

## CART_CLOSED

Đóng cart

## 71.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## CART_EMPTY

## CART_ACTIVE

Add item có QuoteSnapshot

## CART_ACTIVE

## CART_VALIDATING

Validate trước draft

## CART_VALIDATING

## CART_VALID

Quote active, sellable pass

## CART_VALIDATING

## CART_NEEDS_QUOTE_REFRESH

Quote expired

## CART_VALIDATING

## CART_BLOCKED

SKU bị chặn/no stock/lock

## CART_VALID

## CART_CONVERTED_TO_DRAFT

Tạo Order Draft

## CART_ACTIVE

## CART_ABANDONED

Khách không tiếp tục

## CART_CONVERTED_TO_DRAFT

## CART_CLOSED

Draft/order lifecycle tiếp quản

## 71.4. Chuyển trạng thái bị cấm

Không được:

CART_ACTIVE -> Official Order.

CART_NEEDS_QUOTE_REFRESH -> Order Draft complete.

CART_BLOCKED -> CustomerConfirmation.

Cart không QuoteSnapshot.

Cart dùng quote expired.

Cart được tính doanh thu.

## 71.5. P0 điểm chặn

Cart FAIL nếu:

Cart line không QuoteSnapshot.

Quote expired vẫn draft/order.

SKU bị chặn vẫn cart valid.

Cart được dùng làm revenue/commission/Ads ROAS.

Cart không validate lại trước Draft.

## 72. CUSTOMER CHECKOUT CONTEXT STATE MODEL

## 72.1. Mục tiêu

Customer Checkout Context State Model kiểm soát thông tin nhận hàng, thanh toán và lựa chọn dùng thông tin cũ.

## 72.2. Trạng thái chuẩn

State

Ý nghĩa

## CHECKOUT_CONTEXT_EMPTY

Chưa có thông tin

## CUSTOMER_IDENTIFYING

Đang xác định khách

## CUSTOMER_IDENTIFIED

Đã xác định khách

## DELIVERY_INFO_MISSING

Thiếu thông tin nhận hàng

## DELIVERY_INFO_PREFILLED

Có thông tin cũ được prefill

## DELIVERY_INFO_CUSTOMER_SELECTED_OLD

Khách chọn dùng thông tin cũ

## DELIVERY_INFO_UPDATED

Khách cập nhật thông tin mới

## DELIVERY_INFO_VALID

Thông tin nhận hàng hợp lệ

## PAYMENT_PREFERENCE_WAITING

Chưa chọn phương án thanh toán

## PAYMENT_PREFERENCE_SELECTED

Đã chọn thanh toán

## CHECKOUT_CONTEXT_READY

Đủ điều kiện tạo Draft

## CHECKOUT_CONTEXT_BLOCKED

Bị chặn

## 72.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## CHECKOUT_CONTEXT_EMPTY

## CUSTOMER_IDENTIFYING

Bắt đầu checkout

## CUSTOMER_IDENTIFYING

## CUSTOMER_IDENTIFIED

Resolve khách

## CUSTOMER_IDENTIFIED

## DELIVERY_INFO_PREFILLED

Khách cũ có thông tin lưu

## DELIVERY_INFO_PREFILLED

## DELIVERY_INFO_CUSTOMER_SELECTED_OLD

Khách chọn dùng thông tin cũ

## CUSTOMER_IDENTIFIED

## DELIVERY_INFO_MISSING

Khách mới/chưa có thông tin

## DELIVERY_INFO_MISSING

## DELIVERY_INFO_UPDATED

Khách nhập mới

## DELIVERY_INFO_CUSTOMER_SELECTED_OLD/UPDATED

## DELIVERY_INFO_VALID

Validate pass

## DELIVERY_INFO_VALID

## PAYMENT_PREFERENCE_WAITING

Chờ chọn thanh toán

## PAYMENT_PREFERENCE_WAITING

## PAYMENT_PREFERENCE_SELECTED

Chọn method hợp lệ

## PAYMENT_PREFERENCE_SELECTED

## CHECKOUT_CONTEXT_READY

Shipping/payment pass

## 72.4. Chuyển trạng thái bị cấm

Không được:

DELIVERY_INFO_PREFILLED -> dùng luôn nếu khách chưa chọn/policy chưa cho phép.

DELIVERY_INFO_MISSING -> Order Draft ready.

PAYMENT_PREFERENCE_WAITING -> Order Draft ready.

Public full address trên live/comment/public trace.

Nhầm customer identity.

Dùng dữ liệu cũ trái privacy policy.

## 72.5. P0 điểm chặn

Checkout Context FAIL nếu:

Thiếu thông tin nhận hàng vẫn tạo Draft complete.

Không có payment method vẫn tạo Draft complete.

Thông tin cũ bị dùng không có lựa chọn hợp lệ.

Dữ liệu riêng tư bị public.

Nhầm khách/session.

## 73. ORDER DRAFT STATE MODEL

## 73.1. Mục tiêu

Order Draft State Model kiểm soát bản tạm tính/xác nhận đơn nháp trước khi khách xác nhận.

## 73.2. Trạng thái Order Draft chuẩn

State

Ý nghĩa

## DRAFT_NOT_CREATED

Chưa tạo

## DRAFT_CREATING

Đang tạo

## DRAFT_INCOMPLETE

Thiếu thông tin

## DRAFT_READY_FOR_CUSTOMER_REVIEW

Đủ 3 phần, chờ khách xem

## DRAFT_SENT_TO_CUSTOMER

Đã gửi/hiển thị cho khách

## DRAFT_CUSTOMER_REVIEWING

Khách đang xem

## DRAFT_EXPIRED

Quote hoặc draft hết hiệu lực

## DRAFT_BLOCKED

Bị chặn

## DRAFT_CONFIRMED_BY_CUSTOMER

Khách xác nhận hợp lệ

## DRAFT_CONVERTED_TO_ORDER

Đã chuyển thành official order

## DRAFT_CANCELLED

Hủy

## DRAFT_CLOSED

Đóng

## 73.3. 3 phần bắt buộc của Order Draft

Order Draft phải có:

Thông tin nhận hàng.

Phương án thanh toán.

Phần xác nhận đơn hàng rõ ràng.

Nếu thiếu một trong ba phần này, Draft không được xem là complete.

## 73.4. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## DRAFT_NOT_CREATED

## DRAFT_CREATING

Cart/checkout valid

## DRAFT_CREATING

## DRAFT_INCOMPLETE

Thiếu thông tin

## DRAFT_CREATING

## DRAFT_READY_FOR_CUSTOMER_REVIEW

Đủ 3 phần

## DRAFT_READY_FOR_CUSTOMER_REVIEW

## DRAFT_SENT_TO_CUSTOMER

Hiển thị/gửi khách

## DRAFT_SENT_TO_CUSTOMER

## DRAFT_CUSTOMER_REVIEWING

Khách nhận/view

## DRAFT_CUSTOMER_REVIEWING

## DRAFT_CONFIRMED_BY_CUSTOMER

CustomerConfirmation hợp lệ

## DRAFT_CONFIRMED_BY_CUSTOMER

## DRAFT_CONVERTED_TO_ORDER

Official order created

## DRAFT_READY/SENT/REVIEWING

## DRAFT_EXPIRED

Quote expired

## DRAFT_READY/SENT/REVIEWING

## DRAFT_BLOCKED

Runtime điểm chặn mới

## DRAFT_BLOCKED

## DRAFT_CANCELLED

Không thể tiếp tục

## DRAFT_CONVERTED_TO_ORDER

## DRAFT_CLOSED

Order lifecycle tiếp quản

## 73.5. Chuyển trạng thái bị cấm

Không được:

DRAFT_INCOMPLETE -> CustomerConfirmation.

DRAFT_EXPIRED -> Official Order.

DRAFT_BLOCKED -> Official Order.

DRAFT_SENT_TO_CUSTOMER -> order_code nếu chưa confirm.

Khách nói “mua” trước Draft -> DRAFT_CONFIRMED.

System tự confirm thay khách.

Draft được tính doanh thu.

## 73.6. P0 điểm chặn

Order Draft FAIL nếu:

Thiếu 3 phần bắt buộc.

Quote expired vẫn xác nhận.

Không có CustomerConfirmation vẫn tạo order.

Draft tự cấp order_code.

Draft được tính Ads ROAS/KPI/commission.

## 74. CUSTOMER CONFIRMATION STATE MODEL

## 74.1. Mục tiêu

Customer Confirmation State Model kiểm soát hành động xác nhận đơn hợp lệ của khách.

## 74.2. Trạng thái chuẩn

State

Ý nghĩa

## CONFIRMATION_NOT_REQUESTED

Chưa yêu cầu xác nhận

## CONFIRMATION_REQUESTED

Đã yêu cầu xác nhận

## CUSTOMER_REVIEWING_DRAFT

Khách đang xem draft

## CUSTOMER_CONFIRMED

Khách xác nhận hợp lệ

## CUSTOMER_REJECTED

Khách từ chối

## CUSTOMER_REQUESTED_CHANGE

Khách yêu cầu sửa

## CONFIRMATION_EXPIRED

Xác nhận sau quote/draft hết hạn

## CONFIRMATION_INVALID

Xác nhận không hợp lệ

## CONFIRMATION_BLOCKED

Bị runtime điểm chặn

## CONFIRMATION_CLOSED

Đóng xác nhận

## 74.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## CONFIRMATION_NOT_REQUESTED

## CONFIRMATION_REQUESTED

Draft ready/sent

## CONFIRMATION_REQUESTED

## CUSTOMER_REVIEWING_DRAFT

Khách xem draft

## CUSTOMER_REVIEWING_DRAFT

## CUSTOMER_CONFIRMED

CTA/text confirmation hợp lệ

## CUSTOMER_REVIEWING_DRAFT

## CUSTOMER_REJECTED

Khách không mua

## CUSTOMER_REVIEWING_DRAFT

## CUSTOMER_REQUESTED_CHANGE

Khách sửa thông tin

## CONFIRMATION_REQUESTED/REVIEWING

## CONFIRMATION_EXPIRED

Quote hết hạn

## CONFIRMATION_REQUESTED/REVIEWING

## CONFIRMATION_BLOCKED

Sale lock/recall/runtime điểm chặn

## CUSTOMER_CONFIRMED

## CONFIRMATION_CLOSED

Official order created

## 74.4. Chuyển trạng thái bị cấm

Không được:

## CONFIRMATION_NOT_REQUESTED -> CUSTOMER_CONFIRMED.

Khách nói “mua/chốt/lấy” trước Draft -> CUSTOMER_CONFIRMED.

CONFIRMATION_EXPIRED -> Official Order.

CONFIRMATION_BLOCKED -> Official Order.

Confirmation từ session/khách khác.

Confirmation khi Order Draft đã bị supersede.

## 74.5. P0 điểm chặn

Customer Confirmation FAIL nếu:

System tự confirm thay khách.

Không có Draft vẫn confirm.

Quote expired vẫn confirm.

Sale Lock/Recall active vẫn confirm.

Customer/session mismatch.

Không có evidence xác nhận.

## 75. OFFICIAL ORDER STATE MODEL

## 75.1. Mục tiêu

Official Order State Model kiểm soát đơn hàng chính thức sau CustomerConfirmation.

## 75.2. Trạng thái Official Order chuẩn

State

Ý nghĩa

## ORDER_NOT_CREATED

Chưa có order

## ORDER_CREATING

Đang tạo

## ORDER_CREATED

Order chính thức đã tạo

## ORDER_CODE_ISSUED

Đã cấp order_code

## ORDER_PAYMENT_WAITING

Chờ thanh toán

## ORDER_PAYMENT_REVIEW

Chờ review thanh toán

## ORDER_PAYMENT_CONFIRMED

Thanh toán confirmed

## ORDER_VERIFICATION_WAITING

Chờ verify order/revenue

## ORDER_VERIFIED

Order verified

## ORDER_CANCELLED

Đơn bị hủy

## ORDER_EXPIRED

Đơn hết hiệu lực theo policy

## ORDER_REFUNDED

Hoàn tiền nếu scope có

## ORDER_FULFILLMENT_WAITING

Chờ fulfillment nếu scope có

## ORDER_CLOSED

Đóng đơn

## 75.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## ORDER_NOT_CREATED

## ORDER_CREATING

CustomerConfirmation confirmed

## ORDER_CREATING

## ORDER_CREATED

Idempotent create pass

## ORDER_CREATED

## ORDER_CODE_ISSUED

Order official

## ORDER_CODE_ISSUED

## ORDER_PAYMENT_WAITING

Payment method selected

## ORDER_PAYMENT_WAITING

## ORDER_PAYMENT_REVIEW

Bank transfer/COD/review cần kiểm

## ORDER_PAYMENT_WAITING/REVIEW

## ORDER_PAYMENT_CONFIRMED

Payment confirmed

## ORDER_PAYMENT_CONFIRMED

## ORDER_VERIFICATION_WAITING

Ready to verify

## ORDER_VERIFICATION_WAITING

## ORDER_VERIFIED

Verification pass

## ORDER_CREATED/WAITING/REVIEW

## ORDER_CANCELLED

Cancel policy pass

## ORDER_PAYMENT_CONFIRMED

## ORDER_REFUNDED

Refund policy pass

## ORDER_VERIFIED

## ORDER_FULFILLMENT_WAITING

Nếu fulfillment scope

## ORDER_VERIFIED/FULFILLMENT

## ORDER_CLOSED

Close lifecycle

## 75.4. Chuyển trạng thái bị cấm

Không được:

## ORDER_NOT_CREATED -> ORDER_CODE_ISSUED.

ORDER_CREATED nếu chưa CustomerConfirmation.

## ORDER_PAYMENT_WAITING -> PAID.

ORDER_PAYMENT_REVIEW -> PAID nếu chưa confirmation.

## ORDER_CANCELLED -> ORDER_VERIFIED.

ORDER_REFUNDED -> Verified Revenue nếu policy loại trừ.

Retry create order tạo nhiều order_code.

Order từ quote expired.

Order khi Operational bị chặn.

## 75.5. P0 điểm chặn

Official Order FAIL nếu:

Không CustomerConfirmation vẫn tạo order.

Không QuoteSnapshot vẫn tạo order.

Quote expired vẫn tạo order.

Retry tạo order trùng.

order_code cấp cho Draft.

Order bị lock/recall vẫn tiếp tục bình thường.

## 76. PAYMENT METHOD STATE MODEL

## 76.1. Mục tiêu

Payment Method State Model kiểm soát phương thức thanh toán được chọn.

## 76.2. Trạng thái chuẩn

State

Ý nghĩa

## PAYMENT_METHOD_NOT_SELECTED

Chưa chọn

## PAYMENT_METHOD_CHECKING

Đang kiểm

## PAYMENT_METHOD_AVAILABLE

Method khả dụng

## PAYMENT_METHOD_SELECTED

Khách chọn method

## PAYMENT_METHOD_BLOCKED

Method bị chặn

## PAYMENT_METHOD_UNAVAILABLE

Method không khả dụng

## PAYMENT_INSTRUCTION_CREATED

Đã tạo hướng dẫn thanh toán

## PAYMENT_METHOD_CLOSED

Đóng method flow

## 76.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PAYMENT_METHOD_NOT_SELECTED

## PAYMENT_METHOD_CHECKING

Checkout chọn method

## PAYMENT_METHOD_CHECKING

## PAYMENT_METHOD_AVAILABLE

Policy/channel pass

## PAYMENT_METHOD_AVAILABLE

## PAYMENT_METHOD_SELECTED

Khách chọn

## PAYMENT_METHOD_SELECTED

## PAYMENT_INSTRUCTION_CREATED

Tạo instruction

## PAYMENT_METHOD_CHECKING

## PAYMENT_METHOD_BLOCKED

Policy/channel fail

## PAYMENT_METHOD_CHECKING

## PAYMENT_METHOD_UNAVAILABLE

Runtime lỗi

## 76.4. Chuyển trạng thái bị cấm

Không được:

Method inactive -> selected.

Bank transfer instruction không registry.

COD selected nếu shipping/channel không eligible.

Payment instruction thiếu reference khi policy yêu cầu.

Method selected tự gắn PAID.

## 76.5. P0 điểm chặn

Payment Method FAIL nếu:

Payment method không active vẫn chọn.

Bank info hardcoded.

COD không eligible vẫn chọn.

Payment instruction thiếu reference.

Method unavailable nhưng order tiếp tục.

## 77. DIRECT BANK TRANSFER STATE MODEL

## 77.1. Mục tiêu

Direct Bank Transfer State Model kiểm soát chuyển khoản trực tiếp và kế toán review.

## 77.2. Trạng thái chuẩn

State

Ý nghĩa

## BANK_TRANSFER_NOT_SELECTED

Chưa chọn chuyển khoản

## BANK_TRANSFER_SELECTED

Đã chọn chuyển khoản

## BANK_INSTRUCTION_CREATED

Đã tạo hướng dẫn chuyển khoản

## BANK_TRANSFER_WAITING

Chờ khách chuyển

## BANK_TRANSFER_PROOF_RECEIVED

Khách gửi chứng từ/ảnh nếu có

## ACCOUNTING_REVIEW_WAITING

Chờ kế toán/đối soát

## ACCOUNTING_REVIEW_IN_PROGRESS

Đang đối soát

## ACCOUNTING_CONFIRMED

Kế toán xác nhận

## ACCOUNTING_REJECTED

Kế toán từ chối

## BANK_RECONCILIATION_FAILED

Đối soát không khớp

## BANK_TRANSFER_CLOSED

Đóng flow

## 77.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## BANK_TRANSFER_SELECTED

## BANK_INSTRUCTION_CREATED

Bank registry + memo/reference

## BANK_INSTRUCTION_CREATED

## BANK_TRANSFER_WAITING

Chờ khách chuyển

## BANK_TRANSFER_WAITING

## BANK_TRANSFER_PROOF_RECEIVED

Khách gửi ảnh/chứng từ

## BANK_TRANSFER_PROOF_RECEIVED

## ACCOUNTING_REVIEW_WAITING

Cần review

## ACCOUNTING_REVIEW_WAITING

## ACCOUNTING_REVIEW_IN_PROGRESS

Kế toán kiểm

## ACCOUNTING_REVIEW_IN_PROGRESS

## ACCOUNTING_CONFIRMED

Khớp giao dịch

## ACCOUNTING_REVIEW_IN_PROGRESS

## ACCOUNTING_REJECTED

Không hợp lệ

## ACCOUNTING_REVIEW_IN_PROGRESS

## BANK_RECONCILIATION_FAILED

Không khớp

## ACCOUNTING_CONFIRMED

## BANK_TRANSFER_CLOSED

Payment confirmation tiếp quản

## 77.4. Chuyển trạng thái bị cấm

Không được:

## BANK_TRANSFER_PROOF_RECEIVED -> PAID.

## ACCOUNTING_REVIEW_WAITING -> PAID.

Missing memo/reference -> confirmed tự động.

Bank info hardcoded.

Amount mismatch -> confirmed.

Wrong order reference -> confirmed.

## 77.5. P0 điểm chặn

Bank Transfer FAIL nếu:

Ảnh chuyển khoản được gắn PAID.

Không accounting review vẫn confirmed.

Bank registry không được dùng.

Sai amount/reference vẫn confirmed.

Payment review không audit.

## 78. COD STATE MODEL

## 78.1. Mục tiêu

COD State Model kiểm soát đơn nhận hàng rồi thanh toán.

## 78.2. Trạng thái chuẩn

State

Ý nghĩa

## COD_NOT_SELECTED

Chưa chọn COD

## COD_ELIGIBILITY_CHECKING

Đang kiểm COD

## COD_ELIGIBLE

COD hợp lệ

## COD_NOT_ELIGIBLE

COD không hợp lệ

## COD_SELECTED

Khách chọn COD

## COD_PAYMENT_WAITING

Chờ thu tiền khi giao

## COD_COLLECTION_CONFIRMED

Đã xác nhận thu tiền

## COD_COLLECTION_FAILED

Thu tiền thất bại

## COD_CANCELLED

Hủy COD

## COD_CLOSED

Đóng COD

## 78.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## COD_NOT_SELECTED

## COD_ELIGIBILITY_CHECKING

Khách chọn COD

## COD_ELIGIBILITY_CHECKING

## COD_ELIGIBLE

Shipping/channel pass

## COD_ELIGIBILITY_CHECKING

## COD_NOT_ELIGIBLE

Không đủ điều kiện

## COD_ELIGIBLE

## COD_SELECTED

Khách xác nhận method

## COD_SELECTED

## COD_PAYMENT_WAITING

Order created

## COD_PAYMENT_WAITING

## COD_COLLECTION_CONFIRMED

Thu tiền xác nhận

## COD_PAYMENT_WAITING

## COD_COLLECTION_FAILED

Không thu được tiền

## COD_COLLECTION_CONFIRMED

## COD_CLOSED

Payment/revenue verification tiếp quản

## COD_COLLECTION_FAILED

## COD_CANCELLED

Theo policy

## 78.4. Chuyển trạng thái bị cấm

Không được:

COD_SELECTED -> PAID ngay.

COD_PAYMENT_WAITING -> Verified Revenue.

COD_COLLECTION_FAILED -> Verified Revenue.

Tự thêm phí COD nếu policy chưa khóa.

COD_NOT_ELIGIBLE -> Order Draft complete.

## 78.5. P0 điểm chặn

COD FAIL nếu:

COD chưa thu vẫn PAID.

COD failed vẫn verified revenue.

COD fee tự thêm.

COD không eligible vẫn chọn.

COD unpaid vẫn tính Ads/KPI/commission.

## 79. SHIPPING STATE MODEL

## 79.1. Mục tiêu

Shipping State Model kiểm soát đủ điều kiện giao hàng và phí ship.

## 79.2. Trạng thái chuẩn

State

Ý nghĩa

## SHIPPING_NOT_CHECKED

Chưa kiểm

## SHIPPING_CHECKING

Đang kiểm

## SHIPPING_ADDRESS_MISSING

Thiếu địa chỉ

## SHIPPING_ELIGIBLE

Đủ điều kiện giao

## SHIPPING_NOT_ELIGIBLE

Không giao được

## SHIPPING_FEE_WAITING

Chờ phí ship

## SHIPPING_FEE_CONFIRMED

Phí ship xác nhận

## SHIPPING_BLOCKED

Bị chặn

## SHIPPING_RUNTIME_UNAVAILABLE

Runtime lỗi

## SHIPPING_CLOSED

Đóng shipping check

## 79.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## SHIPPING_NOT_CHECKED

## SHIPPING_CHECKING

Có checkout

## SHIPPING_CHECKING

## SHIPPING_ADDRESS_MISSING

Thiếu địa chỉ

## SHIPPING_CHECKING

## SHIPPING_ELIGIBLE

Khu vực giao được

## SHIPPING_CHECKING

## SHIPPING_NOT_ELIGIBLE

Không giao

## SHIPPING_ELIGIBLE

## SHIPPING_FEE_WAITING

Cần tính phí

## SHIPPING_FEE_WAITING

## SHIPPING_FEE_CONFIRMED

Có phí theo policy

## SHIPPING_CHECKING

## SHIPPING_RUNTIME_UNAVAILABLE

Runtime lỗi

## SHIPPING_BLOCKED

## SHIPPING_CHECKING

Recheck hợp lệ

## 79.4. Chuyển trạng thái bị cấm

Không được:

SHIPPING_ADDRESS_MISSING -> Order Draft ready.

SHIPPING_NOT_ELIGIBLE -> Official Order.

SHIPPING_RUNTIME_UNAVAILABLE -> phí ship tự bịa.

International shipping tự bật khi chưa owner approved.

Phí ship không policy.

## 79.5. P0 điểm chặn

Shipping FAIL nếu:

Thiếu địa chỉ vẫn tạo draft complete.

Khu vực không giao vẫn order.

Phí ship tự bịa.

International order bật không governance.

COD shipping không eligible vẫn COD.

## 80. VAT / INVOICE REQUEST STATE MODEL

## 80.1. Mục tiêu

VAT / Invoice State Model kiểm soát yêu cầu hóa đơn và thông tin hóa đơn.

## 80.2. Trạng thái chuẩn

State

Ý nghĩa

## INVOICE_NOT_REQUESTED

Không yêu cầu hóa đơn

## INVOICE_REQUESTED

Khách yêu cầu hóa đơn

## INVOICE_INFO_WAITING

Chờ thông tin

## INVOICE_INFO_INCOMPLETE

Thiếu thông tin

## INVOICE_INFO_COMPLETE

Đủ thông tin

## INVOICE_HANDOFF_WAITING

Chờ Finance/MISA

## INVOICE_HANDOFF_BLOCKED

Bị chặn

## INVOICE_CLOSED

Đóng yêu cầu

## 80.3. Form thông tin hóa đơn

Khi khách yêu cầu hóa đơn, cần thu:

Tên công ty/cá nhân.

Mã số thuế nếu có.

Email nhận hóa đơn.

Địa chỉ xuất hóa đơn.

Người nhận hóa đơn.

Số điện thoại.

Ghi chú.

## 80.4. Chuyển trạng thái bị cấm

Không được:

INVOICE_INFO_INCOMPLETE -> handoff ready nếu policy bắt buộc đủ.

Public thông tin hóa đơn.

Tự phát hành hóa đơn ngoài Finance/MISA boundary.

MISA missing mapping -> success.

Tự sửa order total/VAT ngoài policy.

## 80.5. P0 điểm chặn

Invoice Boundary FAIL nếu:

Lộ thông tin hóa đơn.

Thu thiếu thông tin nhưng marked complete sai.

VAT hiển thị không policy.

Handoff MISA sai/missing mapping success giả.

## 81. PAYMENT CONFIRMATION / PAID STATE MODEL

## 81.1. Mục tiêu

Payment Confirmation State Model kiểm soát trạng thái thanh toán và PAID.

## 81.2. Trạng thái chuẩn

State

Ý nghĩa

## PAYMENT_NOT_STARTED

Chưa bắt đầu

## PAYMENT_WAITING

Chờ thanh toán

## PAYMENT_REVIEW

Chờ review

## PAYMENT_CONFIRMED

Thanh toán xác nhận

## PAYMENT_FAILED

Thanh toán thất bại

## PAYMENT_REJECTED

Bị từ chối

## PAYMENT_AMOUNT_MISMATCH

Sai số tiền

## PAYMENT_REFERENCE_MISMATCH

Sai reference

## PAYMENT_REFUND_WAITING

Chờ hoàn tiền nếu scope có

## PAYMENT_REFUNDED

Đã hoàn tiền nếu scope có

## PAYMENT_CLOSED

Đóng thanh toán

## 81.3. Điều kiện PAYMENT_CONFIRMED

PAYMENT_CONFIRMED chỉ được ghi khi có một trong các xác nhận hợp lệ:

Payment Gateway confirmation hợp lệ.

Payment Core confirmation hợp lệ.

Accounting Review confirmation đối với chuyển khoản.

COD collection confirmation nếu COD.

Reconciliation pass.

## 81.4. Chuyển trạng thái bị cấm

Không được:

PAYMENT_WAITING -> PAYMENT_CONFIRMED từ lời khách nói.

PAYMENT_REVIEW -> PAYMENT_CONFIRMED nếu chưa review pass.

Ảnh chuyển khoản -> PAYMENT_CONFIRMED.

## COD WAITING -> PAYMENT_CONFIRMED.

Amount mismatch -> PAYMENT_CONFIRMED.

Failed callback -> PAYMENT_CONFIRMED.

Payment confirmed cho Order Draft chưa official.

## 81.5. P0 điểm chặn

Payment FAIL nếu:

PAID không có xác nhận hợp lệ.

Proof image được xem là PAID.

Payment amount mismatch vẫn pass.

Payment reference mismatch vẫn pass.

Payment status sửa không audit.

Payment confirmed cho order không tồn tại.

## 82. ORDER VERIFICATION / VERIFIED REVENUE STATE MODEL

## 82.1. Mục tiêu

Order Verification State Model kiểm soát điều kiện đơn hàng được ghi nhận doanh thu verified.

## 82.2. Trạng thái chuẩn

State

Ý nghĩa

## VERIFICATION_NOT_STARTED

Chưa verify

## VERIFICATION_WAITING

Chờ verify

## VERIFICATION_CHECKING

Đang kiểm

## ORDER_VERIFIED

Đơn verified

## ORDER_VERIFICATION_FAILED

Verify fail

## REVENUE_VERIFICATION_WAITING

Chờ xác minh revenue

## REVENUE_VERIFIED

Doanh thu verified

## REVENUE_EXCLUDED

Loại khỏi doanh thu

## REVENUE_REVERSED

Đảo doanh thu nếu refund/cancel sau đó

## VERIFICATION_CLOSED

Đóng verification

## 82.3. Điều kiện ORDER_VERIFIED

Order Verified chỉ pass khi:

Official Order tồn tại.

Payment/collection confirmed nếu policy yêu cầu.

Order không cancelled.

Order không invalid.

Order không refunded theo policy loại trừ.

COD không failed.

Risk/fraud pass nếu scope có.

Fulfillment condition pass nếu policy yêu cầu.

Verification record có audit.

## 82.4. Điều kiện REVENUE_VERIFIED

Revenue Verified chỉ pass khi:

## ORDER_VERIFIED.

Payment/collection đủ điều kiện.

Không có exclusion reason.

Không có refund/cancel invalid.

Finance/Commerce verification pass.

Revenue amount xác định đúng.

Source/channel attribution nếu cần.

Evidence đủ.

## 82.5. Chuyển trạng thái bị cấm

Không được:

Quote -> REVENUE_VERIFIED.

Cart -> REVENUE_VERIFIED.

Order Draft -> REVENUE_VERIFIED.

Unpaid order -> REVENUE_VERIFIED.

Payment WAITING -> REVENUE_VERIFIED.

Cancelled order -> REVENUE_VERIFIED.

COD failed -> REVENUE_VERIFIED.

Refunded order -> Ads/KPI/commission nếu policy loại trừ.

## 82.6. P0 điểm chặn

Verified Revenue FAIL nếu:

Unpaid/cancelled/refunded/COD failed vẫn tính revenue.

Draft/quote được tính revenue.

Verified revenue không audit/evidence.

Revenue bị sửa trực tiếp không reversal.

Ads/KPI/commission lấy nguồn chưa verified.

## 83. COMMISSION / KPI / ADS ROAS HANDOFF STATE MODEL

## 83.1. Mục tiêu

State model này kiểm soát việc bàn giao doanh thu verified sang hệ thống đo lường và thưởng.

## 83.2. Trạng thái chuẩn

State

Ý nghĩa

## HANDOFF_NOT_STARTED

Chưa bàn giao

## HANDOFF_WAITING

Chờ bàn giao

## HANDOFF_ELIGIBILITY_CHECKING

Đang kiểm eligibility

## ADS_ROAS_ELIGIBLE

Đủ điều kiện tính ROAS

## KPI_ELIGIBLE

Đủ điều kiện KPI

## COMMISSION_ELIGIBLE

Đủ điều kiện commission

## HANDOFF_BLOCKED

Bị chặn

## HANDOFF_EXCLUDED

Bị loại

## HANDOFF_SENT

Đã gửi downstream

## HANDOFF_ACKNOWLEDGED

Downstream xác nhận nhận

## HANDOFF_RECONCILE_WAITING

Chờ reconcile

## HANDOFF_CLOSED

Đóng handoff

## 83.3. Điều kiện handoff

Handoff chỉ được thực hiện khi có:

Verified Revenue.

Order reference.

Payment/collection reference.

Attribution source nếu cần.

Referral bind nếu commission Diamond.

Exclusion check.

Audit/evidence.

## 83.4. Chuyển trạng thái bị cấm

Không được:

Quote/draft/unpaid order -> Ads ROAS.

No referral bind -> commission Diamond.

Self-purchase bị cấm -> commission.

Cancelled/refunded order -> KPI/commission nếu policy loại trừ.

Handoff success khi downstream chưa acknowledged nếu policy yêu cầu.

Missing mapping MISA/Finance -> success giả.

## 83.5. P0 điểm chặn

Handoff FAIL nếu:

Ads dùng unverified revenue.

KPI dùng unpaid order.

Commission tính từ referral không bind.

Payout eligibility trước Order Verified.

Reconcile WAITING nhưng status success.

## 84. COMMERCE SUPPRESSION STATE MODEL

## 84.1. Mục tiêu

Commerce Suppression State Model kiểm soát trạng thái chặn bán hàng, chặn quote, chặn order, chặn payment hoặc chặn handoff.

## 84.2. Trạng thái chuẩn

State

Ý nghĩa

## SUPPRESSION_NONE

Không bị chặn

## SUPPRESSION_WAITING

Chờ áp suppression

## SUPPRESSION_ACTIVE

Đang chặn

## SUPPRESSION_PROPAGATED

Đã propagate downstream

## SUPPRESSION_PARTIAL

Chỉ propagate một phần

## SUPPRESSION_RELEASE_REVIEW_WAITING

Chờ review gỡ

## SUPPRESSION_RELEASED

Gỡ suppression

## SUPPRESSION_REOPENED

Mở lại suppression

## SUPPRESSION_CLOSED

Đóng suppression

## 84.3. Nguồn suppression

Suppression có thể đến từ:

Product Scope bị chặn.

Operational bị chặn.

Sale Lock.

Recall.

No stock.

Price missing.

Program conflict.

Policy conflict.

Quote expired.

Payment failed.

Shipping bị chặn.

Runtime unavailable.

Evidence not accepted.

Owner review required.

## 84.4. Downstream phải nhận suppression

Suppression phải chặn:

AI Advisor.

Facebook Gateway.

Messenger/Web/Landing Page.

Ads.

Live / MC AI.

## CRM.

Payment flow nếu cần.

IVR nếu order confirmation scope có.

MISA/Finance handoff nếu cần.

## 84.5. Chuyển trạng thái bị cấm

Không được:

SUPPRESSION_ACTIVE nhưng quote/order vẫn pass.

SUPPRESSION_ACTIVE nhưng AI/Live vẫn chốt.

SUPPRESSION_ACTIVE nhưng Ads vẫn scale.

SUPPRESSION_ACTIVE nhưng CRM vẫn upsell.

SUPPRESSION_RELEASED thiếu owner/policy approval.

Runtime unavailable nhưng suppression none.

Cache cũ vượt suppression.

## 84.6. P0 điểm chặn

Suppression FAIL nếu:

Sale Lock/Recall không propagate.

Blocking reason mơ hồ.

Downstream không nhận suppression.

Runtime unavailable nhưng pass.

Suppression active vẫn tạo order.

## 85. COMMAND BOUNDARY

## 85.1. Định nghĩa command

Command là hành động làm thay đổi trạng thái hoặc tạo side-effect trong Commerce Runtime.

Command có thể:

Tạo QuoteSnapshot.

Refresh quote.

Supersede quote.

Add/update cart.

Tạo Order Draft.

Ghi CustomerConfirmation.

Tạo Official Order.

Cấp order_code.

Chọn payment method.

Tạo payment instruction.

Ghi payment review.

Xác nhận payment.

Verify order.

Verify revenue.

Apply suppression.

Release suppression.

Handoff verified revenue.

## 85.2. Command rủi ro cao

Các command sau là rủi ro cao:

Command Group

Ví dụ

Price/Policy

Approve price, activate program, decide policy priority

Quote

Create quote, refresh quote, supersede quote

Order

Create order draft, confirm customer, create official order

Payment

Confirm payment, reject payment, reconcile payment

Bank Transfer

Accounting confirm/reject

## COD

Confirm collection/fail collection

Revenue

Verify order, verify revenue, reverse revenue

Commission/KPI

Mark eligibility, exclude order

Suppression

Apply/release sale suppression

Handoff

Send verified revenue to Ads/KPI/MISA

## 85.3. Command bắt buộc có permission

Không command rủi ro nào được chạy chỉ dựa vào UI.

Permission phải enforce ở backend/application layer theo TECH-01.

Không được:

Hardcode role.

Cho admin override không audit.

Bỏ qua permission vì “nội bộ”.

Dùng frontend hidden button làm bảo mật.

Cho AI/Gateway tự gọi command vượt quyền.

## 85.4. Command bắt buộc có audit

Audit phải ghi:

Actor/system actor.

Action.

Object.

State before.

State after.

Reason.

Runtime source.

Timestamp.

Channel.

Quote/order/payment reference nếu có.

Evidence reference nếu có.

Correlation ID nếu có.

## 85.5. Command bắt buộc idempotent

Các command có side-effect thương mại phải idempotent:

Create QuoteSnapshot nếu retry cùng request.

Refresh quote.

Create Order Draft.

Customer Confirmation nếu retry CTA.

Create Official Order.

Issue order_code.

Create payment instruction.

Payment confirmation callback.

Accounting confirmation.

COD collection confirmation.

Revenue verification.

Handoff to Ads/KPI/MISA/commission.

Retry không được tạo:

Quote trùng sai.

Order trùng.

order_code trùng.

Payment confirmed trùng.

Revenue verified trùng.

Commission eligibility trùng.

MISA sync success giả.

## 86. QUERY BOUNDARY

## 86.1. Định nghĩa query

Query là hành động đọc dữ liệu, không thay đổi state, không tạo side-effect.

Query có thể phục vụ:

AI Advisor.

Facebook Gateway.

Messenger/Web/Landing Page.

Live.

## CRM.

Ads.

Admin/internal review.

Payment review.

Customer support.

Evidence review.

Owner dashboard.

## 86.2. Query không được thay state

Query không được:

Tạo quote.

Refresh quote.

Tạo order.

Xác nhận khách.

Gắn PAID.

Verify revenue.

Apply suppression.

Release suppression.

Handoff Ads/KPI/commission.

Nếu query làm thay đổi trạng thái, đó là command và phải đi qua command boundary.

## 86.3. Internal query và public-safe query phải tách biệt

Internal query có thể xem dữ liệu theo quyền.

Public-safe query chỉ được trả dữ liệu đã được phép hiển thị cho AI/khách/kênh ngoài.

Không được dùng internal query rồi ẩn field ở frontend.

Public-safe Commerce view phải tách rõ:

AI price view.

Customer order draft view.

Payment instruction view.

Public blocking reason view.

Internal finance/payment review view.

Ads verified revenue view.

CRM suppression view.

## 87. COMMAND-QUERY SEPARATION RULE

## 87.1. Nguyên tắc tách command-query

Commerce Runtime phải tách rõ:

Command: thay đổi sự thật thương mại.

Query: đọc sự thật thương mại.

Không dùng query để ghi nhận confirmation/payment/revenue.

Không dùng command để trả dữ liệu public ngoài boundary.

## 87.2. Ví dụ đúng

Nhu cầu

Loại đúng

Xem quote còn hạn không

Query

Tạo QuoteSnapshot

Command

Xem cart

Query

Thêm sản phẩm vào cart

Command

Xem Draft

Query

Tạo Order Draft

Command

Khách bấm xác nhận

Command

Xem order status

Query

Xác nhận payment

Command

Xem payment status

Query

Verify revenue

Command

Xem revenue verified

Query

## 87.3. P0 điểm chặn

Command-query boundary FAIL nếu:

Query có side-effect.

AI query tạo order.

Gateway query xác nhận khách.

Payment status query tự gắn PAID.

Revenue query tự verify.

Command thiếu permission/audit.

Command order/payment/revenue thiếu idempotency.

## 88. QUOTE -> ORDER HANDOFF

## 88.1. Mục tiêu

Quote -> Order Handoff đảm bảo báo giá chuyển sang đơn hàng đúng thời hạn, đúng xác nhận, đúng điều kiện bán.

## 88.2. Điều kiện Quote được dùng tạo Order Draft

Quote chỉ được dùng tạo Order Draft khi:

QuoteSnapshot active.

Quote chưa expired.

Product scope vẫn pass.

Operational gate vẫn pass.

Price/policy không bị supersede bắt buộc.

Customer checkout context đủ.

Payment/shipping eligibility pass.

Không sale lock/recall.

Không runtime unavailable.

## 88.3. Điều kiện Order Draft được dùng tạo Official Order

Order Draft chỉ được dùng tạo Official Order khi:

Draft complete.

Quote active.

Confirmation đúng customer/session.

Payment method hợp lệ.

Shipping hợp lệ.

Operational recheck pass nếu policy yêu cầu.

Không sale lock/recall tại thời điểm tạo order.

## 88.4. P0 điểm chặn

Quote -> Order Handoff FAIL nếu:

Quote expired vẫn tạo order.

Draft incomplete vẫn confirm.

CustomerConfirmation thiếu vẫn order.

Sale Lock active trước order nhưng vẫn tạo.

order_code cấp trước official order.

Retry tạo order trùng.

## 89. ORDER -> PAYMENT HANDOFF

## 89.1. Mục tiêu

Order -> Payment Handoff đảm bảo chỉ official order mới đi vào payment flow.

## 89.2. Điều kiện đi vào Payment

Payment chỉ được bắt đầu khi:

Official Order tồn tại.

order_code hợp lệ.

Payment method hợp lệ.

Payment amount lấy từ QuoteSnapshot/order final total.

Payment reference/memo được tạo nếu cần.

Order chưa cancelled/expired.

Payment instruction không hardcode dữ liệu ngoài registry.

## 89.3. Điều kiện chuyển PAID

PAID chỉ pass khi:

Payment confirmation hợp lệ.

Amount khớp.

Reference khớp.

Order tồn tại.

Payment không duplicate.

Accounting review pass nếu bank transfer.

COD collection confirmed nếu COD.

Audit/evidence có đủ.

## 89.4. P0 điểm chặn

Order -> Payment Handoff FAIL nếu:

Draft đi vào payment như official order.

Payment amount không từ order/QuoteSnapshot.

Ảnh chuyển khoản thành PAID.

COD WAITING thành PAID.

Payment callback invalid vẫn confirmed.

Retry payment tạo confirmed trùng.

## 90. PAYMENT -> VERIFIED REVENUE HANDOFF

## 90.1. Mục tiêu

Payment -> Verified Revenue Handoff đảm bảo chỉ đơn hợp lệ mới trở thành doanh thu verified.

## 90.2. Điều kiện Order Verification

Order Verification phải kiểm:

Official order.

Payment/collection confirmation.

Order không cancelled.

Order không invalid.

Order không refund/excluded.

COD không failed.

Risk/fraud nếu scope có.

Fulfillment nếu policy yêu cầu.

Evidence đầy đủ.

## 90.3. Điều kiện Revenue Verified

Revenue Verified chỉ được tạo khi:

## ORDER_VERIFIED.

Revenue amount xác định.

Payment amount khớp.

Không có exclusion.

Không có unresolved dispute nếu policy yêu cầu.

Finance/Commerce verification pass.

Audit/evidence có đủ.

## 90.4. P0 điểm chặn

Payment -> Revenue FAIL nếu:

Payment WAITING vẫn revenue.

Order cancelled/refunded vẫn revenue.

COD failed vẫn revenue.

Quote/cart/draft được tính revenue.

Revenue verified bị sửa trực tiếp không reversal.

Không evidence nhưng vẫn PASS.

## 91. VERIFIED REVENUE -> ADS / KPI / COMMISSION HANDOFF

## 91.1. Mục tiêu

Verified Revenue -> Downstream Handoff đảm bảo Ads/KPI/commission chỉ dùng doanh thu đã xác minh.

## 91.2. Handoff sang Ads ROAS

Ads chỉ được nhận:

Verified revenue.

Order verified reference.

Payment verified reference.

Campaign/source attribution nếu có.

Exclusion reason nếu không đủ điều kiện.

Ads không được nhận:

Quote.

Cart.

Order Draft.

Unpaid order.

Payment WAITING.

Cancelled/refunded order.

COD failed.

## 91.3. Handoff sang KPI

KPI chỉ được nhận doanh thu/order đã đủ điều kiện theo policy.

Không được tính KPI từ:

Đơn chưa verified.

Đơn hủy.

Đơn chưa thanh toán.

Đơn COD fail.

Đơn refund/excluded.

## 91.4. Handoff sang Commission

Commission chỉ được xét khi:

Verified revenue.

Referral/owner eligibility pass nếu Diamond.

Không self-purchase nếu policy cấm.

Order không excluded.

Commission policy pass.

Owner/reward rule pass.

Không được trả commission từ click/link chưa có order verified.

## 91.5. P0 điểm chặn

Verified Revenue Handoff FAIL nếu:

Ads dùng unverified revenue.

KPI dùng unpaid order.

Commission dùng referral chưa bind.

Commission tính từ order cancelled/refunded.

Handoff success không evidence/reconcile.

## 92. RUNTIME UNAVAILABLE / FAIL-SAFE RULES

## 92.1. Nguyên tắc chung

Nếu runtime quan trọng không khả dụng, Commerce phải fail-safe.

Không được:

Suy đoán.

Dùng giá cũ không policy.

Dùng cache để vượt lock/recall.

Tạo order từ quote expired.

Gắn PAID từ dữ liệu chưa xác minh.

Ghi verified revenue từ order chưa đủ điều kiện.

## 92.2. Runtime quan trọng và hành vi fail-safe

Runtime

Nếu unavailable thì

Product Runtime

Không quote/order SKU chưa xác minh

Operational Runtime

Không quote/order production

Price Runtime

Không final price

Program Runtime

Không áp program nếu chưa xác minh

Member Runtime

Không áp member benefit

Referral Runtime

Không áp referral benefit

Policy Priority Runtime

Không final quote

Quote Runtime

Không order từ dữ liệu rời

Shipping Runtime

Không xác nhận phí/eligibility

Payment Runtime

Không PAID

Accounting Review

Bank transfer WAITING/review

Evidence Runtime

Không PASS/Release Ready

MISA Runtime

WAITING/reconcile

Sale Lock/Recall Runtime

Fail-safe bị chặn

## 92.3. Blocking reason chuẩn của Commerce

Blocking Reason

Ý nghĩa

## PRODUCT_NOT_APPROVED

Product/SKU chưa approved

## NOT_IN_CHANNEL_SCOPE

Không thuộc kênh

## OPERATIONAL_BLOCKED

Operational không pass

## SALE_LOCK_ACTIVE

Đang stop-sale

## RECALL_ACTIVE

Đang recall

## NO_AVAILABLE_STOCK

Không có tồn khả dụng

## LISTED_PRICE_MISSING

Thiếu giá niêm yết

## LISTED_PRICE_INACTIVE

Giá inactive

## PROGRAM_NOT_APPLICABLE

Không có chương trình phù hợp

## PROGRAM_CONFLICT

Xung đột chương trình

## MEMBER_NOT_ELIGIBLE

Member không đủ điều kiện

## MEMBER_RUNTIME_UNAVAILABLE

Không xác minh member

## REFERRAL_NOT_BOUND

Link Diamond chưa bind

## REFERRAL_NOT_ELIGIBLE

Referral không đủ điều kiện

## POLICY_PRIORITY_MISSING

Chưa có quyết định ưu tiên

## QUOTE_SNAPSHOT_MISSING

Không có QuoteSnapshot

## QUOTE_EXPIRED

Quote hết hạn

## CART_INVALID

Cart không hợp lệ

## CHECKOUT_INFO_MISSING

Thiếu thông tin checkout

## PAYMENT_METHOD_INVALID

Phương thức thanh toán không hợp lệ

## SHIPPING_NOT_ELIGIBLE

Không đủ điều kiện giao

## CUSTOMER_CONFIRMATION_MISSING

Khách chưa xác nhận

## PAYMENT_WAITING

Chưa xác nhận thanh toán

## PAYMENT_REVIEW_REQUIRED

Cần review thanh toán

## PAYMENT_FAILED

Thanh toán thất bại

## ORDER_CANCELLED

Đơn đã hủy

## REVENUE_NOT_VERIFIED

Chưa xác minh doanh thu

## RUNTIME_UNAVAILABLE

Runtime không khả dụng

## EVIDENCE_NOT_ACCEPTED

Evidence chưa accepted

## OWNER_REVIEW_REQUIRED

Cần owner review

## 93. CROSS-LIFECYCLE P0 điểm chặn MATRIX

## 93.1. Ma trận điểm chặn theo lifecycle

Lifecycle

P0 điểm chặn

Downstream bị chặn

Product Scope

SKU chưa approved vẫn quote

Quote / Cart / Order

Operational Gate

Sale Lock/Recall vẫn order

Commerce / AI / Gateway

Listed Price

Missing price vẫn final quote

QuoteSnapshot

Program

Program conflict vẫn áp

QuoteSnapshot

Member

Runtime unavailable vẫn benefit

QuoteSnapshot

Referral

No bind vẫn benefit/commission

Quote / Commission

Policy Priority

Cộng dồn tùy tiện

Final Price

QuoteSnapshot

No snapshot vẫn báo giá

AI / Draft / Order

Quote Hold

Quote expired vẫn order

Official Order

Cart

Cart không snapshot

Order Draft

Checkout

Thiếu thông tin nhận hàng

Order Draft

Draft

Thiếu 3 phần bắt buộc

Customer Confirmation

Confirmation

Không confirm vẫn order_code

Official Order

Official Order

Retry tạo order trùng

Payment / Fulfillment

Bank Transfer

Ảnh chuyển khoản thành PAID

Payment

## COD

COD chưa thu vẫn paid/revenue

Revenue

Payment

Payment WAITING thành PAID

Verified Revenue

Verification

Unpaid/cancelled thành revenue

Ads/KPI/Commission

Handoff

Unverified revenue sang Ads/KPI

Reporting / Reward

Suppression

Runtime bị chặn vẫn bán

All downstream

## 94. EVIDENCE CHO PHẦN 3/4

## 94.1. Evidence bắt buộc theo state machine

Evidence ID

Nội dung

## COM-EVD-P3-001

Product Scope State Evidence

## COM-EVD-P3-002

Operational Gate Blocking Evidence

## COM-EVD-P3-003

Listed Price Runtime Evidence

## COM-EVD-P3-004

Program Runtime Evidence

## COM-EVD-P3-005

Member Benefit Eligibility Evidence

## COM-EVD-P3-006

Diamond Referral Bind Evidence

## COM-EVD-P3-007

Policy Priority Decision Evidence

## COM-EVD-P3-008

QuoteSnapshot Creation/Expiry Evidence

## COM-EVD-P3-009

Quote Refresh/Supersede Evidence

## COM-EVD-P3-010

Cart Validation Evidence

## COM-EVD-P3-011

Checkout Context Evidence

## COM-EVD-P3-012

Order Draft Completeness Evidence

## COM-EVD-P3-013

CustomerConfirmation Evidence

## COM-EVD-P3-014

Official Order Creation / Idempotency Evidence

## COM-EVD-P3-015

Payment Method Evidence

## COM-EVD-P3-016

Bank Transfer / Accounting Review Evidence

## COM-EVD-P3-017

COD Collection Evidence

## COM-EVD-P3-018

Shipping Eligibility Evidence

## COM-EVD-P3-019

Invoice Request Evidence

## COM-EVD-P3-020

Payment Confirmation Evidence

## COM-EVD-P3-021

Order Verification Evidence

## COM-EVD-P3-022

Verified Revenue Evidence

## COM-EVD-P3-023

Ads/KPI/Commission Handoff Evidence

## COM-EVD-P3-024

Suppression Propagation Evidence

## COM-EVD-P3-025

Runtime Unavailable Fail-Safe Evidence

## 94.2. Evidence metadata tối thiểu

Evidence phải có:

Evidence ID.

Requirement/source rule.

Module.

Object/state.

Environment.

Version/build reference nếu có.

Test/smoke reference.

Actor/reviewer.

Status.

Timestamp.

Result.

Link/correlation nếu có.

Owner decision nếu dùng cho gate.

Chỉ evidence trạng thái ACCEPTED mới được dùng để PASS.

## 95. SMOKE MATRIX CHO PHẦN 3/4

## 95.1. Smoke lifecycle/state machine

Smoke ID

Lifecycle

Nội dung

Expected Result

## COM-P3-SMK-001

Product Scope

SKU chưa approved

Không quote/order

## COM-P3-SMK-002

Operational Gate

Sale Lock active

Commerce bị chặn

## COM-P3-SMK-003

Operational Gate

Runtime unavailable

Không quote/order

## COM-P3-SMK-004

Price

Missing listed price

Không final quote

## COM-P3-SMK-005

Program

Program expired

Không áp program

## COM-P3-SMK-006

Member

Member runtime lỗi

Không áp benefit

## COM-P3-SMK-007

Referral

Link chưa bind

Không áp referral benefit

## COM-P3-SMK-008

Policy

Conflict chưa quyết

Không final quote

## COM-P3-SMK-009

Quote

Quote active

Có expiry đúng

## COM-P3-SMK-010

Quote

Giờ Vàng quá 5 phút

Quote expired

## COM-P3-SMK-011

Quote

24/7 quá 15 phút

Quote expired

## COM-P3-SMK-012

Cart

Cart line không snapshot

Bị chặn

## COM-P3-SMK-013

Checkout

Thiếu địa chỉ

Draft incomplete

## COM-P3-SMK-014

Draft

Thiếu payment method

Draft incomplete

## COM-P3-SMK-015

Confirmation

Khách nói mua trước Draft

Chưa official order

## COM-P3-SMK-016

Confirmation

CTA confirm draft hợp lệ

CustomerConfirmation pass

## COM-P3-SMK-017

Order

Confirmation valid

Official order + order_code

## COM-P3-SMK-018

Order

Retry create order

Không tạo trùng

## COM-P3-SMK-019

Bank Transfer

Ảnh chuyển khoản

Payment review, không PAID

## COM-P3-SMK-020

## COD

## COD WAITING

Không PAID/revenue

## COM-P3-SMK-021

Payment

Amount mismatch

Review/bị chặn

## COM-P3-SMK-022

Verification

Unpaid order

Không verified revenue

## COM-P3-SMK-023

Verification

Cancelled order

Không verified revenue

## COM-P3-SMK-024

Revenue Handoff

Verified revenue

Được handoff Ads/KPI/commission

## COM-P3-SMK-025

Revenue Handoff

Unverified order

Không handoff

## COM-P3-SMK-026

Suppression

Runtime unavailable

Downstream bị chặn

## COM-P3-SMK-027

MISA nếu scope có

Missing mapping

WAITING/reconcile, không success

## COM-P3-SMK-028

IVR nếu scope có

Order sale lock trước xác nhận

IVR không xác nhận như bình thường

## 96. DONE GATE CỦA PHẦN 3/4

## 96.1. Điều kiện Done Gate

## PHẦN 3/4 chỉ được xem là DONE khi:

Commerce lifecycle tổng thể đã khóa.

Product / SKU / Channel Scope State Model đã khóa.

Operational Sellable Gate State Model đã khóa.

Listed Price State Model đã khóa.

Program Price State Model đã khóa.

Member Benefit State Model đã khóa.

Diamond Referral State Model đã khóa.

Policy Priority State Model đã khóa.

QuoteSnapshot State Model đã khóa.

Cart State Model đã khóa.

Customer Checkout Context State Model đã khóa.

Order Draft State Model đã khóa.

Customer Confirmation State Model đã khóa.

Official Order State Model đã khóa.

Payment Method State Model đã khóa.

Direct Bank Transfer State Model đã khóa.

COD State Model đã khóa.

Shipping State Model đã khóa.

VAT / Invoice State Model đã khóa.

Payment Confirmation / Paid State Model đã khóa.

Order Verification / Verified Revenue State Model đã khóa.

Commission / KPI / Ads ROAS Handoff State Model đã khóa.

Commerce Suppression State Model đã khóa.

Command Boundary đã rõ.

Query Boundary đã rõ.

Command-query separation đã rõ.

Quote -> Order Handoff đã rõ.

Order -> Payment Handoff đã rõ.

Payment -> Verified Revenue Handoff đã rõ.

Verified Revenue -> Ads/KPI/Commission Handoff đã rõ.

Runtime unavailable fail-safe đã rõ.

Blocking reason chuẩn đã rõ.

Evidence requirement đã rõ.

Smoke matrix đã rõ.

Không gọi Documentation Complete là Production Ready.

## 97. FAIL GATE CỦA PHẦN 3/4

## 97.1. Điều kiện làm PHẦN 3 fail

## PHẦN 3/4 FAIL nếu có bất kỳ điểm nào sau:

Product Active/SKU Active tự thành orderable.

Operational bị chặn nhưng Commerce vẫn quote/order.

Missing listed price vẫn tạo QuoteSnapshot final.

Program expired/inactive vẫn áp.

Member runtime unavailable vẫn áp benefit.

Diamond referral chưa bind vẫn benefit/commission.

Policy conflict vẫn final quote.

QuoteSnapshot không expiry.

Quote expired vẫn tạo order.

Cart không QuoteSnapshot.

Order Draft thiếu 3 phần bắt buộc.

Khách nói “mua” trước Draft vẫn tạo official order.

Không CustomerConfirmation vẫn có order_code.

Retry tạo official order trùng.

Ảnh chuyển khoản được gắn PAID.

COD chưa thu vẫn PAID hoặc verified revenue.

Payment WAITING/review vẫn revenue.

Cancelled/refunded order vẫn revenue.

Quote/cart/draft/unverified order được đưa vào Ads ROAS/KPI/commission.

Runtime unavailable nhưng downstream vẫn pass.

Query có side-effect.

Command thiếu permission/audit/idempotency.

Evidence chưa accepted nhưng vẫn PASS.

## 98. TRẠNG THÁI SAU PHẦN 3/4

## 98.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## TECH-04 PHẦN 3/4

## WRITTEN_FOR_REVIEW

Commerce Lifecycle

## LOCKED_DRAFT

Commerce State Machine Model

## DEFINED

Command Boundary

## DEFINED

Query Boundary

## DEFINED

Quote-Order Handoff

## DEFINED

Order-Payment Handoff

## DEFINED

Payment-Revenue Handoff

## DEFINED

Revenue-Downstream Handoff

## DEFINED

Runtime Unavailable Rule

## DEFINED

P0 điểm chặn Matrix

## DEFINED

Evidence Requirement

## DEFINED

Smoke Matrix

## DEFINED

Implementation Status

## WAITING

Test Status

## WAITING

Smoke Status

## WAITING

Evidence Status

## WAITING

Release Ready

KHONG

Production Ready

KHONG

Go-live Approved

KHONG

## 99. KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời Commerce Runtime.

Từ đây trở đi, Commerce không được triển khai như một form bán hàng đơn giản hoặc một bảng order rời rạc.

Commerce Runtime phải vận hành theo state machine rõ:

Product Scope -> Operational Gate -> Price -> Program -> Member -> Referral -> Policy Priority -> QuoteSnapshot -> Cart -> Checkout Context -> Order Draft -> Customer Confirmation -> Official Order -> Payment -> Order Verification -> Verified Revenue -> Ads/KPI/Commission Handoff -> Suppression.

Nguyên tắc cuối cùng của PHẦN 3/4:

Không Product/Operational pass thì không quote. Không QuoteSnapshot thì không final price. Không quote còn hiệu lực thì không order. Không CustomerConfirmation thì không official order. Không payment confirmation thì không PAID. Không order verification thì không verified revenue. Không verified revenue thì không Ads/KPI/commission. Runtime unavailable thì fail-safe, không suy đoán.

## PHẦN 3/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 4/4 - FINAL COMMERCE SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-04 FINAL CONCLUSION

## 100. MỤC TIÊU CỦA PHẦN 4/4

## 100.1. Vai trò của PHẦN 4/4

## PHẦN 4/4 là phần khóa cuối của TECH-04.

PHẦN này không viết thêm module mới.

PHẦN này dùng để khóa:

Final Commerce Smoke Matrix.

Commerce Evidence Package.

Owner Review Points.

Payment / Bank Transfer / COD Review.

Pricing / Program / Member / Referral Review.

Security / Privacy / Public-safe Commerce Review.

Downstream Readiness Rule.

Release Handoff sang TECH-05 AI Advisor, TECH-06 Facebook Gateway, TECH-07 Ads, TECH-08 MC AI Live, TECH-09 IVR, MISA/Finance nếu scope có.

Done Gate toàn TECH-04.

Fail Gate toàn TECH-04.

P0 điểm chặn Registry cuối.

Final Conclusion của Commerce Runtime.

## 100.2. Nguyên tắc khóa cuối

TECH-04 chỉ được xem là DOCUMENTATION_COMPLETE khi đủ 4 phần.

Nhưng:

DOCUMENTATION_COMPLETE không phải Production Ready.

TECH-04 hoàn tất tài liệu không có nghĩa là:

Đã implement.

Đã test.

Đã smoke pass.

Đã có accepted evidence.

Đã release ready.

Đã production ready.

Đã go-live approved.

Nguyên tắc dựa trên:

Không có accepted evidence thì không Completion Decision.

Không có smoke pass thì không Release Ready.

Không có owner sign-off thì không Release Approved.

Không có release decision thì không Go-live Approved.

Không được dùng lời xác nhận miệng để PASS.

Không downstream nào được phụ thuộc production vào Commerce Runtime nếu TECH-04 chưa clear implementation/evidence/smoke.

## 101. FINAL COMMERCE SMOKE MATRIX - TỔNG THỂ

## 101.1. Mục tiêu của Final Commerce Smoke Matrix

Final Commerce Smoke Matrix dùng để chứng minh chuỗi Commerce Runtime hoạt động đúng từ đầu đến cuối.

Smoke không chỉ kiểm từng nút bấm hoặc từng màn hình.

Smoke phải chứng minh được các rule P0 sau:

Product Active/SKU Active không đồng nghĩa Sellable.

Operational bị chặn thì Commerce bị chặn.

Sale Lock / Recall thắng Commerce.

Không Listed Price active thì không final quote.

Không QuoteSnapshot thì không báo final price.

Quote hết hạn thì không tạo official order.

Không CustomerConfirmation thì không official order.

Không official order thì không order_code.

Không payment confirmation thì không PAID.

Ảnh chuyển khoản không đủ để PAID.

COD chưa thu tiền không được PAID/verified revenue.

Không Order Verified thì không Verified Revenue.

Không Verified Revenue thì không Ads ROAS/KPI/commission.

Không hardcode thông tin ngân hàng.

Không cộng dồn quyền lợi tùy tiện.

Runtime unavailable thì fail-safe.

Evidence chưa ACCEPTED thì không PASS.

## 102. SMOKE GROUP A - FOUNDATION / PERMISSION / AUDIT / IDEMPOTENCY

## 102.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-A001

Command rủi ro yêu cầu permission

User không có quyền approve price / confirm payment / verify revenue

Bị chặn

CO

## COM-SMK-A002

Backend enforce permission

Gọi command trực tiếp không qua UI

Bị chặn ở backend/application layer

CO

## COM-SMK-A003

Command rủi ro có audit

Create quote, create order, confirm payment, verify revenue

Audit đủ actor, action, state before/after, timestamp

CO

## COM-SMK-A004

Idempotency tạo official order

Retry create order cùng confirmation

Không tạo order/order_code trùng

CO

## COM-SMK-A005

Idempotency payment confirmation

Retry payment callback/accounting confirm

Không xác nhận thanh toán trùng

CO

## COM-SMK-A006

Idempotency verified revenue

Retry verify revenue

Không ghi revenue verified trùng

CO

## COM-SMK-A007

Evidence có metadata

Evidence thiếu environment/reviewer/status

Không dùng để PASS

CO

## COM-SMK-A008

Audit không sửa/xóa

Thử sửa/xóa audit

Bị chặn

CO

## 102.2. Fail Gate

Nhóm A FAIL nếu:

Permission chỉ khóa ở UI.

Command rủi ro thiếu audit.

Create order không idempotent.

Payment confirmation không idempotent.

Verified revenue không idempotent.

Evidence thiếu metadata nhưng vẫn được dùng để PASS.

Audit có thể sửa/xóa.

Nếu nhóm A FAIL, toàn bộ TECH-04 implementation bị bị chặn.

## 103. SMOKE GROUP B - PRODUCT SCOPE / OPERATIONAL SELLABLE GATE

## 103.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-B001

SKU chưa approved

SKU chưa được Product Domain duyệt

Không quote/order

CO

## COM-SMK-B002

SKU không thuộc channel scope

SKU active nhưng không bán trên kênh này

Không quote/order

CO

## COM-SMK-B003

Product Knowledge chưa approved

AI/Live muốn dùng public content

Không được dùng public

CO

## COM-SMK-B004

Product active nhưng batch chưa released

Product/SKU active, Operational chưa released

Commerce bị chặn

CO

## COM-SMK-B005

Warehouse chưa receipt

Batch released nhưng kho chưa nhận

Commerce bị chặn

CO

## COM-SMK-B006

No available stock

Không có tồn khả dụng

Commerce bị chặn

CO

## COM-SMK-B007

Recall active

SKU/batch bị recall

Commerce bị chặn

CO

## COM-SMK-B008

Sale Lock active

SKU/batch bị stop-sale

Commerce bị chặn

CO

## COM-SMK-B009

Operational Runtime unavailable

Không xác minh vận hành

Fail-safe, không quote/order

CO

## COM-SMK-B010

Operational pass

Product scope + operational signal pass

Được chuyển sang price check

CO

## 103.2. Fail Gate

Nhóm B FAIL nếu:

Product Active/SKU Active tự thành orderable.

Operational bị chặn nhưng Commerce vẫn quote/order.

Sale Lock/Recall không chặn Commerce.

Runtime unavailable nhưng vẫn quote/order.

AI/Live được dùng Product Knowledge chưa approved.

## 104. SMOKE GROUP C - LISTED PRICE / PROGRAM PRICE / MEMBER / DIAMOND REFERRAL / POLICY PRIORITY

## 104.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-C001

Missing listed price

SKU không có giá niêm yết active

Không final quote

CO

## COM-SMK-C002

Listed price inactive

Giá inactive/expired

Không final quote

CO

## COM-SMK-C003

Listed price conflict

Nhiều giá active xung đột

Block, cần owner review

CO

## COM-SMK-C004

Program inactive

Chương trình không active

Không áp program

CO

## COM-SMK-C005

SKU ngoài program

SKU không thuộc chương trình

Không áp program

CO

## COM-SMK-C006

Giờ Vàng không session active

Không có session active

Không áp giá Giờ Vàng

CO

## COM-SMK-C007

24/7 active

Program runtime pass

Được xét program price theo runtime

CO

## COM-SMK-C008

Member runtime unavailable

Không xác minh hạng member

Không áp member benefit

CO

## COM-SMK-C009

Member tier valid

Member hợp lệ, policy cho phép

Được xét benefit

CO

## COM-SMK-C010

Member cycle expired

Grace không pass

Không áp benefit

CO

## COM-SMK-C011

Diamond link không bind

Link không có bind hợp lệ

Không áp referral benefit

CO

## COM-SMK-C012

Diamond bind hợp lệ

Resolver pass

Được xét referral benefit

CO

## COM-SMK-C013

Policy conflict

Program + member + referral cùng có khả năng

Runtime quyết định, không cộng tùy tiện

CO

## COM-SMK-C014

Không có Policy Priority

Thiếu rule ưu tiên

Không final quote

CO

## COM-SMK-C015

AI tự cộng quyền lợi

AI/Gateway tự tính giảm giá

Bị chặn

CO

## 104.2. Fail Gate

Nhóm C FAIL nếu:

Missing/inactive/conflict price vẫn final quote.

Program expired vẫn áp.

Member benefit áp khi runtime chưa xác nhận.

Diamond referral benefit áp khi link chưa bind.

Quyền lợi bị cộng dồn tùy tiện.

Không trace được applied policy.

AI/Live/Gateway tự tính giá hoặc tự áp quyền lợi.

## 105. SMOKE GROUP D - QUOTESNAPSHOT / QUOTE HOLD / QUOTE REFRESH

## 105.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-D001

QuoteSnapshot tạo đúng

Product/Operational/Price/Policy pass

Tạo QuoteSnapshot active

CO

## COM-SMK-D002

QuoteSnapshot thiếu expiry

Quote không có thời hạn

Bị chặn

CO

## COM-SMK-D003

QuoteSnapshot thiếu final total

Không có tổng tiền cuối

Bị chặn

CO

## COM-SMK-D004

No QuoteSnapshot

AI/Live/Gateway muốn báo final price

Bị chặn

CO

## COM-SMK-D005

Quote Giờ Vàng sau 5 phút

Quote hết hold window

## QUOTE_EXPIRED

CO

## COM-SMK-D006

Quote 24/7 sau 15 phút

Quote hết hold window

## QUOTE_EXPIRED

CO

## COM-SMK-D007

Confirm quote expired

Khách xác nhận sau expiry

Không tạo order

CO

## COM-SMK-D008

Refresh quote

Runtime recheck pass

Tạo QuoteSnapshot mới, quote cũ superseded

CO

## COM-SMK-D009

Refresh khi Sale Lock active

Runtime recheck bị chặn

Không tạo quote mới

CO

## COM-SMK-D010

Quote bị sửa trực tiếp

Thay final total sau khi tạo

Bị chặn, phải supersede

CO

## 105.2. Fail Gate

Nhóm D FAIL nếu:

Không có QuoteSnapshot vẫn báo final price.

Quote không có expiry.

Quote expired vẫn tạo order.

Quote cũ bị sửa trực tiếp.

Refresh quote không kiểm lại Operational/Price/Policy.

AI dùng quote expired để chốt đơn.

## 106. SMOKE GROUP E - CART / CHECKOUT CONTEXT / ORDER DRAFT

## 106.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-E001

Cart line không QuoteSnapshot

Add item không quote

Bị chặn

CO

## COM-SMK-E002

Cart quote expired

Cart line quote hết hạn

Cần refresh

CO

## COM-SMK-E003

Cart SKU bị chặn

SKU bị sale lock/no stock

Cart bị chặn

CO

## COM-SMK-E004

Cart được tính revenue

Cart active

Không được tính doanh thu

CO

## COM-SMK-E005

Khách mới thiếu địa chỉ

Checkout thiếu receiver/address

Draft incomplete

CO

## COM-SMK-E006

Khách cũ chọn dùng thông tin cũ

Existing customer chọn CTA

Prefill hợp lệ

CO

## COM-SMK-E007

Khách đổi thông tin nhận hàng

Nhập địa chỉ mới

Dùng thông tin mới

CO

## COM-SMK-E008

Public full address

Live/comment/public view

Bị chặn

CO

## COM-SMK-E009

Draft thiếu thông tin nhận hàng

Không đủ receiver/address

Draft incomplete

CO

## COM-SMK-E010

Draft thiếu payment method

Chưa chọn thanh toán

Draft incomplete

CO

## COM-SMK-E011

Draft thiếu phần xác nhận

Không có CTA/text xác nhận

Draft incomplete

CO

## COM-SMK-E012

Draft đủ 3 phần

Receiver + payment + confirmation

Ready for customer review

CO

## COM-SMK-E013

Draft tự cấp order_code

Draft chưa confirmed

Bị chặn

CO

## 106.2. Fail Gate

Nhóm E FAIL nếu:

Cart không có QuoteSnapshot.

Cart dùng quote expired.

Cart được tính là revenue.

Order Draft thiếu 3 phần bắt buộc.

Dữ liệu nhận hàng bị public.

Dùng thông tin cũ của khách mà không có lựa chọn/policy hợp lệ.

Draft tự tạo order_code.

## 107. SMOKE GROUP F - CUSTOMER CONFIRMATION / OFFICIAL ORDER CREATION

## 107.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-F001

Khách nói “mua” trước Draft

Chưa có Draft ready

Không official order

CO

## COM-SMK-F002

Draft ready + khách xem

Draft sent/reviewing

Chờ confirmation

CO

## COM-SMK-F003

CTA “Xác nhận đơn” hợp lệ

Quote còn hạn, đúng session/customer

CO

## COM-SMK-F004

Text fallback hợp lệ

Kênh chỉ-text, khách xác nhận đúng

CO

## COM-SMK-F005

Confirm sau quote expired

Quote expired

Bị chặn

CO

## COM-SMK-F006

Confirm nhầm session/customer

Customer/session mismatch

Bị chặn

CO

## COM-SMK-F007

System tự confirm

Không có customer action

Bị chặn

CO

## COM-SMK-F008

CustomerConfirmation valid

Draft complete + quote active

Tạo Official Order

CO

## COM-SMK-F009

Official Order created

Order chính thức

Cấp order_code

CO

## COM-SMK-F010

Retry create order

Retry cùng confirmation

Không tạo order/order_code trùng

CO

## COM-SMK-F011

Sale Lock trước create order

Lock active sau Draft

Không tạo order

CO

## 107.2. Fail Gate

Nhóm F FAIL nếu:

Khách nói “mua” trước Draft vẫn tạo order.

Không CustomerConfirmation vẫn có official order.

Draft expired vẫn confirm.

Confirmation sai customer/session vẫn pass.

System tự xác nhận thay khách.

Retry tạo order trùng.

order_code cấp trước official order.

Sale Lock/Recall active trước create order nhưng vẫn tạo.

## 108. SMOKE GROUP G - PAYMENT METHOD / BANK TRANSFER / COD / SHIPPING / VAT-INVOICE

## 108.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-G001

Payment method inactive

Method không active

Không cho chọn

CO

## COM-SMK-G002

Payment method invalid channel

Method không thuộc kênh

Bị chặn

CO

## COM-SMK-G003

Bank transfer instruction

Chọn chuyển khoản

Bank info lấy từ Company Bank Account Registry

CO

## COM-SMK-G004

Bank info hardcoded

AI/template/frontend hardcode

Fail Gate

CO

## COM-SMK-G005

Transfer memo/reference missing

Không có nội dung chuyển khoản

WAITING/review hoặc bị chặn theo policy

CO

## COM-SMK-G006

Khách gửi ảnh chuyển khoản

Proof image

PAYMENT_REVIEW, không PAID

CO

## COM-SMK-G007

Accounting confirms

Đối soát khớp

## PAYMENT_CONFIRMED

CO

## COM-SMK-G008

Amount mismatch

Số tiền không khớp

Review/bị chặn, không PAID

CO

## COM-SMK-G009

COD selected

COD eligible

Payment WAITING/COD WAITING

CO

## COM-SMK-G010

COD chưa thu

Chưa collection confirmed

Không PAID, không revenue

CO

## COM-SMK-G011

COD collection failed

Không thu được tiền

Không verified revenue

CO

## COM-SMK-G012

COD fee chưa policy

COD selected

Không tự thêm phí COD

CO

## COM-SMK-G013

Thiếu địa chỉ giao

Checkout missing address

Draft incomplete

CO

## COM-SMK-G014

Khu vực không giao

Shipping not eligible

Block order

CO

## COM-SMK-G015

Phí ship có policy

Shipping fee confirmed

Hiển thị đúng trong quote/draft

CO

## COM-SMK-G016

International chưa approved

Địa chỉ quốc tế

Bị chặn

CO

## COM-SMK-G017

Khách yêu cầu hóa đơn

Invoice request

Thu đủ form thông tin

CO

## COM-SMK-G018

Public invoice info

Public/live/comment view

Bị chặn

CO

## 108.2. Fail Gate

Nhóm G FAIL nếu:

Payment method không hợp lệ vẫn cho chọn.

Bank info hardcoded.

Ảnh chuyển khoản được gắn PAID.

Amount/reference mismatch vẫn confirmed.

COD chưa thu vẫn PAID/revenue.

COD tự thêm phí khi chưa có policy.

Thiếu địa chỉ vẫn Draft complete.

Khu vực không giao vẫn order.

International tự bật khi chưa approved.

Thông tin hóa đơn bị public.

## 109. SMOKE GROUP H - ORDER STATUS / PAYMENT CONFIRMATION / ORDER VERIFICATION / VERIFIED REVENUE

## 109.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-H001

Official order mới

Order created

## ORDER_CREATED / PAYMENT_WAITING

CO

## COM-SMK-H002

Payment WAITING

Chưa xác nhận thanh toán

Không PAID

CO

## COM-SMK-H003

Payment review

Bank transfer chờ kế toán

Không PAID

CO

## COM-SMK-H004

Payment confirmed

Gateway/accounting/COD collection pass

## PAYMENT_CONFIRMED

CO

## COM-SMK-H005

Failed payment

Payment failed

Không PAID

CO

## COM-SMK-H006

Cancelled order

Order cancelled

Không verified revenue

CO

## COM-SMK-H007

Refunded order nếu scope có

Order refund/excluded

Không Ads/KPI/commission nếu policy loại

CO

## COM-SMK-H008

Unpaid order

Official order chưa paid

Không verified revenue

CO

## COM-SMK-H009

Paid nhưng verification WAITING

Payment confirmed, chưa verify

Chưa verified revenue

CO

## COM-SMK-H010

Order verification pass

Order + payment + no exclusion pass

## ORDER_VERIFIED

CO

## COM-SMK-H011

Revenue verification pass

Order verified + revenue rule pass

## REVENUE_VERIFIED

CO

## COM-SMK-H012

Revenue sửa trực tiếp

Sửa amount verified revenue

Bị chặn, phải reversal/adjustment

CO

## COM-SMK-H013

Quote/cart/draft làm revenue

Không official/verified

Bị chặn

CO

## 109.2. Fail Gate

Nhóm H FAIL nếu:

Payment WAITING/review được xem là PAID.

Payment failed vẫn PAID.

Cancelled/refunded/COD failed vẫn verified revenue.

Official order chưa payment confirmed vẫn revenue.

PAID nhưng chưa order verification vẫn revenue.

Quote/cart/draft được tính doanh thu.

Verified revenue bị sửa trực tiếp không reversal.

Revenue không có evidence/audit.

## 110. SMOKE GROUP I - ADS ROAS / KPI / COMMISSION / DIAMOND HANDOFF

## 110.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-I001

Ads nhận quote

Quote chưa order/revenue

Không tính ROAS

CO

## COM-SMK-I002

Ads nhận order unpaid

Official order unpaid

Không tính ROAS

CO

## COM-SMK-I003

Ads nhận verified revenue

Revenue verified

Được xét ROAS

CO

## COM-SMK-I004

KPI nhận unverified order

Chưa verified revenue

Không tính KPI

CO

## COM-SMK-I005

KPI nhận verified revenue

Revenue verified + policy pass

Được xét KPI

CO

## COM-SMK-I006

Commission referral chưa bind

Referral no bind

Không commission

CO

## COM-SMK-I007

Commission owner không eligible

Owner không đủ điều kiện

Không commission

CO

## COM-SMK-I008

Diamond self-purchase nếu policy cấm

Self-purchase

Không commission

CO

## COM-SMK-I009

Cancelled/refunded order

Order excluded

Không commission/KPI/ROAS

CO

## COM-SMK-I010

Commission trước verified revenue

Payment/order chưa verified

Bị chặn

CO

## COM-SMK-I011

Handoff retry

Retry handoff

Không tạo trùng eligibility/sync

CO

## 110.2. Fail Gate

Nhóm I FAIL nếu:

Ads dùng quote/draft/unpaid order làm revenue.

KPI dùng order chưa verified.

Commission tính từ referral chưa bind.

Commission tính trước verified revenue.

Self-purchase bị cấm nhưng vẫn có commission.

Cancelled/refunded order vẫn tính reward.

Handoff không idempotent.

## 111. SMOKE GROUP J - COMMERCE SUPPRESSION / RUNTIME UNAVAILABLE / DOWNSTREAM BLOCKING

## 111.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-J001

Product Runtime unavailable

Không xác minh SKU

Không quote/order

CO

## COM-SMK-J002

Operational Runtime unavailable

Không xác minh sellable

Không quote/order

CO

## COM-SMK-J003

Price Runtime unavailable

Không xác minh giá

Không final quote

CO

## COM-SMK-J004

Program Runtime unavailable

Không xác minh program

Không tự áp program

CO

## COM-SMK-J005

Member Runtime unavailable

Không xác minh member

Không áp member benefit

CO

## COM-SMK-J006

Referral Runtime unavailable

Không xác minh bind

Không áp referral benefit

CO

## COM-SMK-J007

Policy Priority Runtime unavailable

Không quyết định quyền lợi

Không final quote

CO

## COM-SMK-J008

Payment Runtime unavailable

Không xác minh payment

Không PAID

CO

## COM-SMK-J009

Evidence Runtime unavailable

Không PASS/Release Ready

CO

## COM-SMK-J010

Sale Lock/Recall Runtime unavailable

Fail-safe bị chặn

CO

## COM-SMK-J011

Downstream query bị chặn SKU

AI/Gateway/Ads/Live/CRM nhận blocking reason rõ

CO

## COM-SMK-J012

Cache cũ vượt suppression

Dùng dữ liệu trước lock

Bị chặn

CO

## 111.2. Fail Gate

Nhóm J FAIL nếu:

Runtime unavailable nhưng Commerce pass.

Cache cũ được dùng để vượt Sale Lock/Recall.

AI/Gateway/Live vẫn chốt khi suppression active.

Ads/CRM vẫn kích hoạt bán khi bị chặn.

Blocking reason mơ hồ khiến downstream tự đoán.

Evidence Runtime lỗi nhưng vẫn PASS.

## 112. SMOKE GROUP K - MISA / FINANCE / IVR / PUBLIC-SAFE COMMERCE HANDOFF

## 112.1. Smoke Matrix

Smoke ID

Downstream

Dữ liệu kiểm tra

Expected Result

## P0

## COM-SMK-K001

## MISA

Order chưa verified

Không handoff như doanh thu hợp lệ

CO

## COM-SMK-K002

## MISA

Missing mapping

WAITING/reconcile, không success giả

CO

## COM-SMK-K003

## MISA

Retry sync

Không tạo sync trùng

CO

## COM-SMK-K004

Finance

Bank transfer WAITING

Không PAID, chờ review

CO

## COM-SMK-K005

Finance

Payment confirmed

Có evidence đối soát

CO

## COM-SMK-K006

## IVR

Order chưa CustomerConfirmation

Không IVR xác nhận như official order

CO

## COM-SMK-K007

## IVR

Order sale lock/recall trước xác nhận

IVR không xác nhận tiếp như bình thường

CO

## COM-SMK-K008

AI Advisor

Hỏi giá

Chỉ dùng QuoteSnapshot, không tự tính

CO

## COM-SMK-K009

Facebook Gateway

Tạo order chưa confirm

Bị chặn

CO

## COM-SMK-K010

Live

SKU locked

Không chốt live

CO

## COM-SMK-K011

## CRM

SKU recall

Suppression, không upsell

CO

## COM-SMK-K012

Public-safe Commerce View

Full address/payment/internal price logic

Không public

CO

## 112.2. Fail Gate

Nhóm K FAIL nếu:

MISA missing mapping nhưng success.

Finance/payment review bị bypass.

IVR xác nhận order chưa hợp lệ hoặc đang lock/recall.

AI tự tính giá.

Gateway tạo order không CustomerConfirmation.

Live/CRM vượt suppression.

Public-safe view lộ dữ liệu riêng tư hoặc logic nội bộ.

## 113. FINAL COMMERCE EVIDENCE PACKAGE

## 113.1. Mục tiêu của Evidence Package

Commerce Evidence Package là bộ chứng cứ bắt buộc để chứng minh TECH-04 đã được triển khai, kiểm thử, smoke và review đúng.

Không có Evidence Package ACCEPTED thì không được gọi:

Completion Decision.

Release Ready.

Production Ready.

Go-live Approved.

## 114. COMMERCE EVIDENCE PACKAGE STRUCTURE

## 114.1. Nhóm evidence bắt buộc

Evidence Group

Nội dung

Bắt buộc

## COM-EVD-G01

Foundation Permission / Audit / Idempotency Evidence

CO

## COM-EVD-G02

Product Scope / Channel Scope Evidence

CO

## COM-EVD-G03

Operational Sellable / Recall / Sale Lock Dependency Evidence

CO

## COM-EVD-G04

Listed Price Evidence

CO

## COM-EVD-G05

Program Price Evidence

CO

## COM-EVD-G06

Member Benefit Eligibility Evidence

CO

## COM-EVD-G07

Diamond Referral Bind / Eligibility Evidence

CO

## COM-EVD-G08

Policy Priority Decision Evidence

CO

## COM-EVD-G09

QuoteSnapshot / Quote Hold Evidence

CO

## COM-EVD-G10

Cart / Checkout Context Evidence

CO

## COM-EVD-G11

Order Draft / Customer Confirmation Evidence

CO

## COM-EVD-G12

Official Order Creation / Idempotency Evidence

CO

## COM-EVD-G13

Payment Method Evidence

CO

## COM-EVD-G14

Direct Bank Transfer / Accounting Review Evidence

CO

## COM-EVD-G15

COD Boundary Evidence

CO

## COM-EVD-G16

Shipping Eligibility / Fee Evidence

CO

## COM-EVD-G17

VAT / Invoice Request Evidence

CO

## COM-EVD-G18

Payment Confirmation / PAID Evidence

CO

## COM-EVD-G19

Order Verification / Verified Revenue Evidence

CO

## COM-EVD-G20

Ads ROAS / KPI / Commission Handoff Evidence

CO

## COM-EVD-G21

Commerce Suppression / Runtime Blocking Evidence

CO

## COM-EVD-G22

MISA / Finance Handoff Evidence nếu scope có

## CONDITIONAL

## COM-EVD-G23

IVR Handoff Evidence nếu scope có

## CONDITIONAL

## COM-EVD-G24

Security / Privacy / Public-safe Commerce Evidence

CO

## COM-EVD-G25

Final Commerce Smoke Report

CO

## COM-EVD-G26

Owner Review / Sign-off Record

CO

## COM-EVD-G27

Release Handoff Record

CO

## 115. EVIDENCE METADATA STANDARD

## 115.1. Metadata tối thiểu

Mỗi evidence record phải có:

Evidence ID.

Requirement ID hoặc source rule.

TECH/module/smoke mapping.

Environment.

Build/version nếu có.

Test/smoke ID.

Actor.

Reviewer.

Review status.

Timestamp.

Result.

Attachment/log/screenshot/report link nếu có.

Audit/correlation ID nếu có.

Owner decision nếu dùng cho gate.

Notes về rủi ro còn lại nếu có.

## 115.2. Evidence status chuẩn

Status

Ý nghĩa

Được dùng để PASS

## DRAFT

Evidence nháp

KHONG

## SUBMITTED

Đã nộp review

KHONG

## NEEDS_CLARIFICATION

Cần làm rõ

KHONG

## REJECTED

Bị từ chối

KHONG

## ACCEPTED

Đã được reviewer/owner chấp nhận

CO

## SUPERSEDED

Bị thay thế bởi evidence mới

KHONG

## EXPIRED

Không còn hợp lệ

KHONG

## 115.3. Rule khóa

Chỉ evidence trạng thái ACCEPTED mới được dùng để PASS.

Không dùng:

Lời xác nhận miệng.

Screenshot rời không mapping.

Log không environment.

Test không reviewer.

Smoke chưa pass.

Evidence không liên kết requirement.

Evidence không timestamp.

Evidence không owner/reviewer khi cần.

Evidence từ môi trường không đúng scope.

## 116. OWNER REVIEW POINTS

## 116.1. Mục tiêu

Owner Review Points xác định các điểm bắt buộc cần người có thẩm quyền duyệt.

Commerce Runtime ảnh hưởng trực tiếp đến giá, quyền lợi, thanh toán, doanh thu, hoa hồng, Ads ROAS, KPI và khiếu nại khách hàng. Vì vậy owner review là bắt buộc.

## 117. OWNER REVIEW MATRIX

Review Point

Nội dung review

Owner đề xuất

Bắt buộc

## COM-ORP-001

Commerce boundary Product / Operational / Commerce / AI

Business / Technical Owner

CO

## COM-ORP-002

Sellable Gate dependency từ Operational Core

Operational / Commerce Owner

CO

## COM-ORP-003

Listed Price policy

Business / Finance Owner

CO

## COM-ORP-004

Program Price policy

Business / Commerce Owner

CO

## COM-ORP-005

Member Tier / Benefit rule

Business / CRM / Commerce Owner

CO

## COM-ORP-006

Diamond Referral rule

Business / Referral / Finance Owner

CO

## COM-ORP-007

Policy Priority Decision rule

Business / Commerce Owner

CO

## COM-ORP-008

QuoteSnapshot rule

Commerce / Technical Owner

CO

## COM-ORP-009

Quote hold window Giờ Vàng 5 phút / 24/7 15 phút

Business / Commerce Owner

CO

## COM-ORP-010

Cart / Order Draft rule

Commerce / CX Owner

CO

## COM-ORP-011

CustomerConfirmation rule

Commerce / Legal / CX Owner

CO

## COM-ORP-012

Official Order / order_code rule

Commerce / Technical Owner

CO

## COM-ORP-013

Payment method policy

Finance / Commerce Owner

CO

## COM-ORP-014

Direct bank transfer / Bank Account Registry rule

Finance / Accounting Owner

CO

## COM-ORP-015

COD boundary

Commerce / Fulfillment / Finance Owner

CO

## COM-ORP-016

Shipping fee / eligibility

Commerce / Fulfillment Owner

CO

## COM-ORP-017

VAT / Invoice request boundary

Finance / Accounting Owner

CO

## COM-ORP-018

Payment Confirmation / PAID rule

Finance / Payment Owner

CO

## COM-ORP-019

Order Verification / Verified Revenue rule

Finance / Commerce Owner

CO

## COM-ORP-020

Ads ROAS / KPI / Commission handoff

Business / Finance / Ads Owner

CO

## COM-ORP-021

Suppression / Runtime unavailable fail-safe

Technical / Release Owner

CO

## COM-ORP-022

MISA handoff nếu scope có

Finance / MISA Owner

## CONDITIONAL

## COM-ORP-023

IVR handoff nếu scope có

Commerce / IVR Owner

## CONDITIONAL

## COM-ORP-024

Security / Privacy / Public-safe Commerce View

Security / Business Owner

CO

## COM-ORP-025

Final Commerce Smoke Acceptance

Release Owner

CO

## COM-ORP-026

TECH-04 Release Handoff

Technical Owner

CO

## 118. SECURITY / PRIVACY / PUBLIC-SAFE COMMERCE REVIEW

## 118.1. Mục tiêu

Security / Privacy / Public-safe Commerce Review đảm bảo dữ liệu thương mại không bị lộ sai ngữ cảnh.

Commerce chứa nhiều dữ liệu nhạy cảm:

Giá runtime.

Quyền lợi thành viên.

Referral/Diamond attribution.

Thông tin nhận hàng.

Payment reference.

Thông tin chuyển khoản.

Trạng thái thanh toán.

Verified revenue.

Commission.

## KPI.

Ads ROAS.

Invoice/tax info.

Không được public sai nhóm dữ liệu này.

## 119. PUBLIC-SAFE COMMERCE VIEW MATRIX

Nhóm dữ liệu

AI/khách được nhận

Nội bộ được nhận

Public mặc định

Product public name

Có, nếu approved

Có

Có thể public

Final price

Có, từ QuoteSnapshot

Có

Chỉ theo quote hợp lệ

Quote expiry

Có

Có

Có thể public-safe

Program name public

Có, nếu approved

Có

Có thể public

Member tier

Chỉ nói nếu runtime/customer context cho phép

Có

Không public rộng

Member benefit amount

Chỉ với đúng khách

Có

Không public rộng

Diamond referral buyer benefit

Chỉ nếu resolver pass

Có

Không public commission

Referral owner/internal ID

Không

Có theo quyền

Không public

Commission rate/amount

Không public cho buyer trừ policy riêng

Có theo quyền

Không public

Customer full address

Không public

Có theo quyền

Không public

Phone number

Không public

Có theo quyền

Không public

Payment reference

Chỉ đúng khách/order

Có theo quyền

Không public rộng

Bank account info

Theo registry/policy trong instruction thanh toán

Có

Không hardcode

Payment status

Chỉ đúng khách/order

Có

Không public rộng

Verified revenue

Không cho khách đại trà

Có theo quyền

Không public

Ads ROAS/KPI

Không

Có theo quyền

Không public

Invoice/tax info

Chỉ đúng khách/order

Có theo quyền

Không public

Internal blocking reason

Không

Có

Không public

Public-safe blocking reason

Có thể có

Có

Có thể public-safe

## 120. SECURITY / PRIVACY SMOKE

Smoke ID

Nội dung

Expected Result

## COM-SEC-SMK-001

AI request full address

Không trả public

## COM-SEC-SMK-002

Live/comment request order/payment detail

Không public dữ liệu riêng

## COM-SEC-SMK-003

Customer khác xem order

Bị chặn

## COM-SEC-SMK-004

Referral owner internal ID public

Không trả

## COM-SEC-SMK-005

Commission amount public cho buyer

Không trả nếu policy không cho

## COM-SEC-SMK-006

Bank account hardcoded trong template

Fail Gate

## COM-SEC-SMK-007

Invoice info public

Bị chặn

## COM-SEC-SMK-008

Ads ROAS public

Bị chặn

## COM-SEC-SMK-009

Internal blocking reason public

Chỉ trả public-safe reason

## COM-SEC-SMK-010

Payment proof image public

Bị chặn

## 121. SECURITY / PRIVACY FAIL GATE

Security / Privacy FAIL nếu:

Public view lộ full address, phone, payment, invoice data.

AI/Live/Gateway public member tier/benefit sai khách.

Referral owner internal ID bị public.

Commission/KPI/ROAS bị public.

Bank account bị hardcode.

Customer khác xem order/payment của khách khác.

Internal blocking reason lộ ra ngoài.

Payment proof hoặc thông tin kế toán bị public sai quyền.

## 122. DOWNSTREAM READINESS RULE

## 122.1. Mục tiêu

Downstream Readiness Rule xác định khi nào các hệ thống phía sau được phép phụ thuộc vào Commerce Runtime.

Downstream gồm:

TECH-05 AI Advisor.

TECH-06 Facebook Gateway.

Messenger/Web/Landing Page.

TECH-07 Ads Measurement.

TECH-08 MC AI Live.

## CRM.

Payment/Fulfillment.

MISA/Finance nếu scope có.

TECH-09 IVR nếu scope có.

TECH-10 Completion/Evidence/Release.

## 123. DOWNSTREAM DEPENDENCY MATRIX

Downstream

Chỉ được đi tiếp khi

Nếu Commerce chưa clear

AI Advisor

QuoteSnapshot, public-safe price view, blocking reason, order state rõ

Không báo giá/chốt đơn production

Facebook Gateway

Quote/order/CustomerConfirmation flow rõ

Không tạo order từ Messenger/Live

Messenger/Web/Landing Page

Draft/CTA confirmation/payment instruction rõ

Không cho customer checkout production

Ads Measurement

Verified revenue handoff rõ

Không tính ROAS từ order production

MC AI Live

Sellable + QuoteSnapshot + Live suppression rõ

Không chốt live production

## CRM

Suppression + verified revenue + repurchase eligibility rõ

Không upsell/repurchase production

Payment

Official order + payment amount/reference rõ

Không xác nhận thanh toán production

Finance/MISA

Verified order/payment/revenue mapping rõ

WAITING/reconcile, không success

## IVR

Official order + confirmation/payment/order lock rule rõ

Không xác nhận order production

Completion/Gateway

Smoke + accepted evidence + owner sign-off

Global Gateway bị chặn

## 124. DOWNSTREAM BLOCKING REASONS

## 124.1. Blocking reason bắt buộc

Commerce Runtime phải bàn giao blocking reason rõ, có loại public-safe và internal-safe.

Blocking Reason

Ý nghĩa

## PRODUCT_NOT_APPROVED

Product/SKU chưa approved

## NOT_IN_CHANNEL_SCOPE

Không thuộc kênh bán

## OPERATIONAL_BLOCKED

Operational Core không pass

## SALE_LOCK_ACTIVE

Đang stop-sale

## RECALL_ACTIVE

Đang recall

## NO_AVAILABLE_STOCK

Không có tồn khả dụng

## LISTED_PRICE_MISSING

Thiếu giá niêm yết

## LISTED_PRICE_INACTIVE

Giá không active

## PROGRAM_NOT_APPLICABLE

Không có chương trình phù hợp

## PROGRAM_CONFLICT

Xung đột chương trình

## MEMBER_NOT_ELIGIBLE

Member không đủ điều kiện

## MEMBER_RUNTIME_UNAVAILABLE

Không xác minh member

## REFERRAL_NOT_BOUND

Link Diamond chưa bind

## REFERRAL_NOT_ELIGIBLE

Referral không đủ điều kiện

## POLICY_PRIORITY_MISSING

Chưa có quyết định ưu tiên

## QUOTE_SNAPSHOT_MISSING

Không có QuoteSnapshot

## QUOTE_EXPIRED

Quote hết hạn

## CART_INVALID

Cart không hợp lệ

## CHECKOUT_INFO_MISSING

Thiếu thông tin checkout

## ORDER_DRAFT_INCOMPLETE

Order Draft chưa đủ

## CUSTOMER_CONFIRMATION_MISSING

Khách chưa xác nhận

## PAYMENT_METHOD_INVALID

Phương thức thanh toán không hợp lệ

## SHIPPING_NOT_ELIGIBLE

Không đủ điều kiện giao

## PAYMENT_WAITING

Chưa xác nhận thanh toán

## PAYMENT_REVIEW_REQUIRED

Cần review thanh toán

## PAYMENT_FAILED

Thanh toán thất bại

## ORDER_CANCELLED

Đơn đã hủy

## ORDER_EXPIRED

Đơn hết hiệu lực

## REVENUE_NOT_VERIFIED

Chưa xác minh doanh thu

## MISA_MAPPING_MISSING

Thiếu mapping MISA

## RUNTIME_UNAVAILABLE

Runtime không khả dụng

## EVIDENCE_NOT_ACCEPTED

Evidence chưa accepted

## OWNER_REVIEW_REQUIRED

Cần owner review

## 125. RELEASE HANDOFF PACKAGE

## 125.1. Mục tiêu

Release Handoff Package là bộ hồ sơ bàn giao TECH-04 sang các TECH downstream và DEV implementation wave.

Handoff không phải “cho phép go-live”.

Handoff chỉ có nghĩa:

Tài liệu đã đủ rõ để dev lập implementation plan.

Các boundary đã khóa.

Các P0 điểm chặn đã khóa.

Các smoke/evidence/gate đã rõ.

Downstream biết phải phụ thuộc vào gì.

## 126. RELEASE HANDOFF CONTENT

Release Handoff Package của TECH-04 phải có:

Module contract list COM-M01 -> COM-M25.

Commerce state machine list.

Command-query boundary.

Product/Operational/Commerce dependency map.

Sellable Gate rule.

Listed Price rule.

Program Price rule.

Member Benefit rule.

Diamond Referral rule.

Policy Priority rule.

QuoteSnapshot rule.

Quote Hold rule.

Cart / Checkout Context rule.

Order Draft rule.

CustomerConfirmation rule.

Official Order / order_code rule.

Payment Method rule.

Direct Bank Transfer / Accounting Review rule.

COD rule.

Shipping rule.

VAT / Invoice Request rule.

Payment Confirmation / PAID rule.

Order Verification / Verified Revenue rule.

Ads/KPI/Commission handoff rule.

Commerce Suppression rule.

Runtime unavailable fail-safe rule.

Blocking reason registry.

Smoke matrix COM-SMK-A -> COM-SMK-K.

Evidence package COM-EVD-G01 -> COM-EVD-G27.

Owner review matrix.

Security/privacy/public-safe review checklist.

P0 điểm chặn registry.

Done Gate / Fail Gate.

Gap report template cho implementation.

Handoff note sang TECH-05 AI Advisor.

Handoff note sang TECH-06 Facebook Gateway.

Handoff note sang TECH-07 Ads.

Handoff note sang TECH-08 MC AI Live.

Handoff note sang TECH-09 IVR nếu scope có.

Handoff note sang MISA/Finance nếu scope có.

Statement rõ: Documentation Complete không phải Production Ready.

## 127. HANDOFF SANG TECH-05 AI ADVISOR

## 127.1. AI chỉ được dùng Commerce Runtime, không tự tính

TECH-05 AI Advisor phải dựa trên từ TECH-04:

AI không tự tính giá.

AI không tự cộng quyền lợi.

AI không tự xác định member tier.

AI không tự xác định Diamond referral benefit.

AI không tự nói sản phẩm còn hàng nếu Operational/Commerce chưa pass.

AI không tạo official order nếu chưa CustomerConfirmation.

AI không nói “đơn đã ghi nhận” nếu chưa có official order.

AI không nói PAID nếu chưa có payment confirmation.

AI không nói verified revenue/KPI/commission.

## 127.2. AI được nhận từ Commerce

AI chỉ được nhận:

Product can_consult/can_quote/can_order safe status.

QuoteSnapshot.

Final total từ QuoteSnapshot.

Quote expiry.

Public-safe program/member/referral explanation nếu policy cho phép.

Order Draft safe view.

CustomerConfirmation CTA state.

Order status public-safe.

Payment instruction public-safe.

Payment status public-safe.

Blocking reason public-safe.

Alternative suggestion eligibility nếu SKU không sellable/orderable.

## 127.3. AI không được nhận/public

AI không được nhận hoặc public:

Internal price rule.

Policy priority internal details nếu không public-safe.

Full member internal status.

Diamond referral owner internal ID.

Commission amount/rate.

Full address ở public channel.

Payment proof.

Accounting review note.

Verified revenue.

Ads ROAS/KPI.

MISA mapping.

Internal blocking reason.

## 128. HANDOFF SANG TECH-06 FACEBOOK GATEWAY / META LIVE & MESSENGER

## 128.1. Gateway không tự tạo giá/order

Facebook Gateway phải dựa trên:

Live/comment hỏi giá phải kéo vào Messenger theo rule đã khóa.

Giá trong Messenger phải lấy từ QuoteSnapshot.

Gateway không tự tính giá.

Gateway không tự cộng quyền lợi.

Gateway không tạo order nếu chưa CustomerConfirmation.

Gateway không cấp order_code trước official order.

Gateway không public dữ liệu order/payment trên comment/live.

Gateway phải tôn trọng suppression từ Commerce.

## 128.2. Messenger/Web/Landing Page CTA

Kênh có UI phải ưu tiên:

Order Draft có đủ 3 phần.

CTA “Xác nhận đơn”.

Payment method selection.

Confirmation evidence.

Kênh chỉ-text mới fallback bằng câu yêu cầu khách nhắn xác nhận.

## 129. HANDOFF SANG TECH-07 ADS MEASUREMENT

## 129.1. Ads chỉ được dùng Verified Revenue

Ads Measurement phải dựa trên:

Quote không phải revenue.

Cart không phải revenue.

Order Draft không phải revenue.

Unpaid order không phải revenue.

Payment WAITING không phải revenue.

Cancelled/refunded/COD fail không phải revenue.

Verified Revenue mới được tính ROAS.

Suppression từ Commerce/Operational phải chặn campaign bán sản phẩm.

## 129.2. Ads không được scale khi Commerce bị chặn

Ads không được scale/activate nếu:

SKU không sellable.

Sale Lock active.

Recall active.

No stock.

Commerce bị chặn.

Runtime unavailable.

Verified revenue data chưa clean.

## 130. HANDOFF SANG TECH-08 MC AI LIVE

## 130.1. Live chỉ được chốt khi Commerce pass

MC AI Live phải dựa trên:

Live không tự báo final price.

Live không tự xác định program.

Live không tự áp member/referral.

Live không chốt sản phẩm không sellable/orderable.

Live không vượt suppression.

Live phải dùng QuoteSnapshot trong Messenger/private flow.

Live không public order/payment/private data.

## 130.2. Giờ Vàng trong Live

Nếu là Giờ Vàng:

Giá phải đến từ Commerce Runtime.

Quote giữ giá 5 phút.

CustomerConfirmation phải xảy ra trong thời hạn quote.

Không được fake urgency.

Không được bán khi stock/lock/recall bị chặn.

## 131. HANDOFF SANG TECH-09 IVR ORDER CONFIRMATION

## 131.1. IVR chỉ xác nhận order đủ điều kiện

IVR nếu scope có phải dựa trên:

Không gọi xác nhận Order Draft chưa official nếu IVR scope yêu cầu official order.

Không xác nhận order chưa đủ CustomerConfirmation nếu flow không hợp lệ.

Không tiếp tục xác nhận order đã bị sale lock/recall.

Không xác nhận order expired/cancelled.

Không gắn PAID.

Không verified revenue.

Không gửi CRM/marketing.

IVR chỉ xác nhận trong phạm vi order confirmation đã được governance.

## 131.2. IVR và Commerce Lock

Nếu trong thời gian chờ IVR mà order bị:

Sale Lock.

Recall.

Payment invalid.

Order expired.

Customer cancelled.

Runtime unavailable theo fail-safe.

Thì IVR phải nhận blocking signal và không xác nhận tiếp như bình thường.

## 132. HANDOFF SANG MISA / FINANCE NẾU SCOPE CÓ

## 132.1. MISA chỉ nhận dữ liệu đủ điều kiện

MISA/Finance handoff chỉ được thực hiện khi có:

Official order.

Payment status rõ.

Revenue verification nếu dùng cho doanh thu.

Mapping hợp lệ.

Tax/invoice info nếu có.

Reconcile rule.

Evidence.

Idempotency.

## 132.2. MISA không thay Commerce truth

MISA không được ghi ngược để thay đổi Commerce truth nếu chưa có governance.

Nếu MISA khác Commerce:

Ghi reconcile issue.

Không tự sửa Commerce.

Owner review.

Evidence.

Adjustment/reversal nếu được duyệt.

## 132.3. Missing mapping

Nếu thiếu mapping:

Không được xem là sync success.

Không bỏ qua.

Không tự tạo mapping production nếu chưa owner approved.

Giữ WAITING/reconcile.

Không làm sai verified revenue.

## 133. TECH-04 FINAL DONE GATE

## 133.1. Điều kiện Done Gate toàn TECH-04

TECH-04 chỉ được xem là DOCUMENTATION_COMPLETE khi đủ các điều kiện sau:

## PHẦN 1/4 hoàn tất.

## PHẦN 2/4 hoàn tất.

## PHẦN 3/4 hoàn tất.

## PHẦN 4/4 hoàn tất.

Commerce Runtime Principles đã khóa.

Source-of-truth đã khóa.

Product / Operational / Commerce boundary đã khóa.

No-bypass rule đã khóa.

COM-M01 -> COM-M25 module contracts đã khóa.

Commerce lifecycle đã khóa.

State machine cho Product Scope, Operational Gate, Price, Program, Member, Referral, Policy, Quote, Cart, Checkout, Draft, Confirmation, Order, Payment, COD, Shipping, Invoice, Revenue, Handoff, Suppression đã khóa.

Command-query boundary đã khóa.

QuoteSnapshot là nguồn giá duy nhất đã khóa.

Quote hold window Giờ Vàng 5 phút / 24/7 15 phút đã khóa.

CustomerConfirmation là điều kiện tạo official order đã khóa.

Official Order là điều kiện cấp order_code đã khóa.

Payment confirmation là điều kiện PAID đã khóa.

Order Verification là điều kiện Verified Revenue đã khóa.

Verified Revenue là điều kiện Ads/KPI/Commission đã khóa.

Bank Account Registry rule đã khóa.

COD boundary đã khóa.

Shipping / VAT / Invoice boundary đã khóa.

Commerce Suppression đã khóa.

Runtime unavailable fail-safe đã khóa.

Final Smoke Matrix đã có.

Evidence Package đã có.

Owner Review Matrix đã có.

Security/Privacy/Public-safe Commerce Review đã có.

Release Handoff Package đã có.

Không có đoạn nào gọi Documentation Complete là Production Ready.

Không có đoạn nào cho phép AI/Gateway/Live/CRM tự tính giá.

Không có đoạn nào cho phép order khi chưa CustomerConfirmation.

Không có đoạn nào cho phép PAID khi chưa payment confirmation.

Không có đoạn nào cho phép verified revenue khi chưa order verification.

Không có đoạn nào cho phép Ads/KPI/Commission dùng dữ liệu chưa verified.

## 134. TECH-04 FINAL FAIL GATE

## 134.1. Điều kiện làm TECH-04 FAIL

TECH-04 FAIL nếu có bất kỳ điểm nào sau:

Commerce được viết như giỏ hàng/order đơn giản.

Product Active/SKU Active bị hiểu là Sellable.

Operational bị chặn nhưng Commerce vẫn quote/order.

Sale Lock / Recall không chặn Commerce.

Missing/inactive listed price vẫn final quote.

Program expired/inactive vẫn áp giá.

Member benefit được áp khi runtime chưa xác nhận.

Diamond referral benefit được áp khi link chưa bind.

Quyền lợi bị cộng dồn tùy tiện.

Không có Policy Priority vẫn final quote.

Không có QuoteSnapshot vẫn báo final price.

QuoteSnapshot không có expiry.

Quote expired vẫn tạo official order.

Cart không QuoteSnapshot.

Order Draft thiếu 3 phần bắt buộc.

Khách nói “mua/chốt/lấy” trước Draft vẫn tạo order.

Không CustomerConfirmation vẫn tạo official order.

Không official order vẫn cấp order_code.

Retry tạo order trùng.

Payment method inactive vẫn cho chọn.

Bank account hardcoded.

Ảnh chuyển khoản được gắn PAID.

Accounting Review bị bypass.

COD chưa thu vẫn PAID.

COD chưa thu/COD fail vẫn verified revenue.

Phí COD bị tự thêm khi chưa policy.

Shipping thiếu địa chỉ/khu vực không giao vẫn tạo order.

International shipping/payment tự bật khi chưa owner approved.

VAT/invoice data bị public.

Payment WAITING/review/failed vẫn verified revenue.

Cancelled/refunded order vẫn verified revenue nếu policy loại trừ.

Quote/cart/draft/unverified order được tính Ads ROAS/KPI/commission.

Referral không bind vẫn commission.

Self-purchase bị cấm nhưng vẫn commission.

Runtime unavailable nhưng Commerce pass.

Cache cũ vượt suppression.

AI/Gateway/Live/CRM vượt Commerce Runtime.

MISA missing mapping nhưng success.

IVR xác nhận order bị lock/recall.

Evidence chưa accepted nhưng dùng để PASS.

Smoke chưa pass nhưng gọi Release Ready.

Owner chưa sign-off nhưng gọi Release Approved.

Chưa có release decision nhưng gọi Go-live Approved.

## 135. P0 điểm chặn REGISTRY - FINAL

## 135.1. Danh sách P0 điểm chặn cuối

## P0 ID

điểm chặn

Gate bị chặn

## COM-P0-FINAL-001

Foundation permission/audit/idempotency chưa clear

Commerce Release

## COM-P0-FINAL-002

Product/SKU/channel scope chưa clear

Quote / Order

## COM-P0-FINAL-003

Operational Sellable Gate chưa clear

Commerce Sellable

## COM-P0-FINAL-004

Sale Lock/Recall không chặn Commerce

Global Gateway

## COM-P0-FINAL-005

Missing/inactive listed price vẫn quote

Quote Release

## COM-P0-FINAL-006

Program inactive/expired vẫn áp

Program Release

## COM-P0-FINAL-007

Member benefit không runtime-confirmed

Pricing Release

## COM-P0-FINAL-008

Diamond referral không bind vẫn benefit

Referral Release

## COM-P0-FINAL-009

Policy Priority missing/conflict vẫn final quote

Pricing Release

## COM-P0-FINAL-010

Không QuoteSnapshot vẫn final price

Quote Release

## COM-P0-FINAL-011

Quote expired vẫn official order

Order Release

## COM-P0-FINAL-012

Order Draft thiếu 3 phần bắt buộc

Checkout Release

## COM-P0-FINAL-013

Không CustomerConfirmation vẫn official order

Order Release

## COM-P0-FINAL-014

Order code cấp trước official order

Order Release

## COM-P0-FINAL-015

Retry tạo order trùng

Order Release

## COM-P0-FINAL-016

Bank account hardcoded

Payment Release

## COM-P0-FINAL-017

Ảnh chuyển khoản được gắn PAID

Payment Release

## COM-P0-FINAL-018

COD chưa thu vẫn PAID/revenue

Payment / Revenue

## COM-P0-FINAL-019

Payment WAITING/failed vẫn verified revenue

Revenue Release

## COM-P0-FINAL-020

Cancel/refund/COD fail vẫn verified revenue

Revenue Release

## COM-P0-FINAL-021

Unverified revenue sang Ads/KPI/Commission

Ads/KPI/Reward

## COM-P0-FINAL-022

Referral không bind vẫn commission

Commission Release

## COM-P0-FINAL-023

Runtime unavailable nhưng Commerce pass

Global Gateway

## COM-P0-FINAL-024

Public-safe view lộ private/internal data

Security Release

## COM-P0-FINAL-025

MISA missing mapping success giả

Finance/MISA Release

## COM-P0-FINAL-026

IVR xác nhận order bị lock/recall

IVR Release

## COM-P0-FINAL-027

Evidence chưa accepted nhưng PASS

Completion / Release

## COM-P0-FINAL-028

Smoke thiếu nhưng Release Ready

Release Gateway

## COM-P0-FINAL-029

Downstream bypass Commerce Runtime

Global Gateway

## 136.1. Trạng thái sau khi hoàn tất TECH-04

Hạng mục

Trạng thái

## TECH-04 PHẦN 1/4

## WRITTEN_FOR_REVIEW

## TECH-04 PHẦN 2/4

## WRITTEN_FOR_REVIEW

## TECH-04 PHẦN 3/4

## WRITTEN_FOR_REVIEW

## TECH-04 PHẦN 4/4

## WRITTEN_FOR_REVIEW

## TECH-04 FULL DOCUMENT

## DOCUMENTATION_COMPLETE_AFTER_OWNER_REVIEW

## COMMERCE_TECHNICAL_SPEC

## LOCKED_DRAFT

## COMMERCE_MODULE_CONTRACTS

## DEFINED

## COMMERCE_STATE_MACHINE

## DEFINED

## COMMERCE_SMOKE_MATRIX

## DEFINED

## COMMERCE_EVIDENCE_PACKAGE

## DEFINED

## COMMERCE_RELEASE_HANDOFF

## DEFINED

## IMPLEMENTATION_STATUS

## WAITING

## TEST_STATUS

## WAITING

## SMOKE_STATUS

## WAITING

## EVIDENCE_STATUS

## WAITING_ACCEPTED_EVIDENCE

## OWNER_SIGN_OFF

## WAITING

## COMPLETION_PASS

KHONG

## RELEASE_READY

KHONG

## RELEASE_DECISION_ACCEPTED

KHONG

## PRODUCTION_READINESS

KHONG

## GO_LIVE_DECISION_ACCEPTED

KHONG

bị chặn

## 137. DOWNSTREAM STATUS SAU TECH-04

## 137.1. TECH-05 AI Advisor

Hạng mục

Trạng thái

TECH-05 có thể viết tiếp tài liệu

CO

TECH-05 có thể code production ngay

KHONG

AI được tự tính giá

KHONG

AI được tự tạo order

KHONG

AI được dùng QuoteSnapshot

CO

AI phải tôn trọng CustomerConfirmation

CO

AI phải tôn trọng payment/revenue status

CO

## 137.2. TECH-06 Facebook Gateway

Hạng mục

Trạng thái

TECH-06 có thể viết tiếp tài liệu

CO

Gateway được tự tạo final price

KHONG

Gateway được tạo order khi chưa CustomerConfirmation

KHONG

Gateway phải dùng QuoteSnapshot

CO

Gateway phải dùng CTA/confirmation hợp lệ

CO

Gateway phải tôn trọng suppression

CO

## 137.3. TECH-07 Ads Measurement

Hạng mục

Trạng thái

TECH-07 có thể viết tiếp tài liệu

CO

Ads được dùng quote/order draft làm revenue

KHONG

Ads được dùng unpaid order làm revenue

KHONG

Ads chỉ dùng verified revenue

CO

Ads phải tôn trọng suppression

CO

Ads scale khi SKU bị chặn

KHONG

## 137.4. TECH-08 MC AI Live

Hạng mục

Trạng thái

TECH-08 có thể viết tiếp tài liệu

CO

Live được tự báo final price

KHONG

Live được tự chốt SKU bị chặn

KHONG

Live phải dùng Commerce Runtime

CO

Live phải kéo quote/order vào private flow phù hợp

CO

Live phải tôn trọng Giờ Vàng hold 5 phút nếu quote

CO

## 137.5. TECH-09 IVR nếu scope có

Hạng mục

Trạng thái

TECH-09 có thể viết tiếp tài liệu

CO

IVR được xác nhận order chưa official

KHONG

IVR được gắn PAID

KHONG

IVR được verified revenue

KHONG

IVR phải tôn trọng Sale Lock/Recall/Order status

CO

IVR chỉ xác nhận trong scope được governance

CO

## 137.6. MISA / Finance nếu scope có

Hạng mục

Trạng thái

MISA handoff boundary đã xác định

CO

MISA API/DB chi tiết đã thiết kế

KHONG

Missing mapping được xem là success

KHONG

Reconcile evidence bắt buộc

CO

MISA được thay Commerce truth

KHONG

Finance review cần cho bank transfer

CO

## 138. IMPLEMENTATION READINESS NOTE

## 138.1. TECH-04 chưa cho phép code sâu ngay

TECH-04 là tài liệu kỹ thuật nền.

Sau TECH-04, chưa nên nhảy vào code sâu nếu chưa có:

TECH-05 AI Advisor.

TECH-06 Facebook Gateway.

TECH-07 Ads.

TECH-08 MC AI Live.

TECH-09 IVR nếu scope triển khai.

TECH-10 Completion/Evidence/Release.

Mapping repository/module.

Gap report.

Implementation wave plan.

Test plan chi tiết.

Seed/config plan.

Environment plan.

Owner sign-off.

## 138.2. Việc code phải đi theo wave

Commerce Runtime thuộc:

Wave 3 - Commerce Runtime

Wave 3 chỉ nên bắt đầu sau:

Wave 0 Foundation clear.

Wave 1 Product/SKU/Recipe clear.

Wave 2 Operational Core clear.

TECH-04 accepted for implementation planning.

Module map COM-M01 -> COM-M25 rõ.

P0 điểm chặn registry rõ.

Evidence/smoke plan rõ.

## 139. TECH-04 FINAL RELEASE HANDOFF STATEMENT

## 139.1. Statement bàn giao

TECH-04 bàn giao cho các tài liệu tiếp theo với thông điệp:

Commerce Runtime là nguồn sự thật thương mại của giá, chương trình, quyền lợi, quote, order, thanh toán và doanh thu xác minh.

Mọi downstream muốn báo giá, chốt đơn, xác nhận đơn, xác nhận thanh toán, ghi nhận doanh thu, tính Ads ROAS, KPI hoặc commission phải tôn trọng Commerce Runtime.

Không downstream nào được tự tạo sự thật thương mại.

## 140. TECH-04 FINAL CONCLUSION

TECH-04 đã khóa đầy đủ tầng Commerce Runtime cho Ginsengfood theo hướng greenfield.

Tài liệu đã xác định rõ:

Commerce Runtime đi sau TECH-03 Operational Core.

Commerce Runtime đi trước TECH-05 AI Advisor.

Product Active/SKU Active không đồng nghĩa Sellable.

Operational bị chặn thì Commerce bị chặn.

Sale Lock / Recall thắng Commerce.

Listed Price active là điều kiện bắt buộc để quote.

Program Price phải đến từ runtime.

Member Benefit phải được runtime xác nhận.

Diamond Referral Benefit phải có bind hợp lệ.

Policy Priority quyết định quyền lợi được áp, không cộng dồn tùy tiện.

QuoteSnapshot là nguồn báo giá duy nhất.

Giờ Vàng giữ giá 5 phút.

24/7 giữ giá 15 phút.

Quote hết hạn thì không tạo official order.

Cart không phải order.

Order Draft không phải official order.

Order Draft phải có đủ thông tin nhận hàng, phương án thanh toán và phần xác nhận.

Official order mới có order_code.

Không payment confirmation thì không PAID.

Ảnh chuyển khoản không đủ để PAID.

Bank account không được hardcode.

COD là nhận hàng rồi thanh toán, không tự thêm phí COD nếu chưa có policy.

Không Order Verified thì không Verified Revenue.

Không Verified Revenue thì không Ads ROAS/KPI/Commission.

Runtime unavailable thì fail-safe.

Evidence chưa ACCEPTED thì không PASS.

Smoke chưa PASS thì không Release Ready.

Owner chưa sign-off thì không Release Approved.

Chưa có release decision thì không Go-live Approved.

Kết luận cuối cùng:

TECH-04 hoàn tất ở cấp tài liệu kỹ thuật nền, đủ để chuyển sang review và làm cơ sở viết TECH-05 AI Advisor Runtime.

Nhưng TECH-04 hiện vẫn là:

Commerce Runtime chỉ được production-ready khi đã có implementation thật, test thật, smoke thật, evidence accepted, owner sign-off và release decision hợp lệ.

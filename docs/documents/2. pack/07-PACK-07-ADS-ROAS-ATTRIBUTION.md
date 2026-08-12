# PACK-07 - ADS ROAS ATTRIBUTION

## ADS MEASUREMENT / CHANNEL ATTRIBUTION / VERIFIED REVENUE / SCALE GATE / DATA QUALITY GOVERNANCE

## PHẦN 1/4 — ADS MEASUREMENT PRINCIPLES / SOURCE-OF-TRUTH BOUNDARY / PACK-06–PACK-05–COMMERCE DEPENDENCY

## 1. MỤC TIÊU CỦA PACK-07

PACK-07 thiết lập lớp quản trị đo lường quảng cáo, phân bổ nguồn doanh thu, kiểm soát ROAS, kiểm soát CPA, đánh giá chất lượng dữ liệu và quyết định scale/stop quảng cáo cho hệ thống Ginsengfood.

PACK-07 không tạo ra doanh thu.

PACK-07 không xác nhận đơn hàng.

PACK-07 không xác nhận thanh toán.

PACK-07 không xác nhận giao hàng.

PACK-07 không quyết định sản phẩm có được bán hay không.

PACK-07 không thay thế Commerce Runtime, Payment Core, Order Core, AI Advisor, Facebook Channel Gateway, CRM, Diamond Referral, MISA hoặc Finance.

PACK-07 chỉ đo lường dựa trên dữ liệu hợp lệ, có nguồn rõ, có trace, có rule attribution, có evidence và có kết quả kiểm tra chất lượng dữ liệu.

Mục tiêu cốt lõi của PACK-07 là trả lời các câu hỏi vận hành sau:

- Chi phí quảng cáo đến từ nguồn nào.

- Khách hàng đến từ campaign, ad set, ad, live, page, comment hay Messenger nào.

- Tương tác nào là paid, organic, CRM, referral, Diamond, live hay mixed-source.

- Comment nào chuyển được sang Messenger.

- Messenger nào đi đến tư vấn sản phẩm.

- Tư vấn nào tạo quote.

- Quote nào chuyển thành cart draft.

- Cart draft nào được khách xác nhận.

- Xác nhận nào tạo official order.

- Official order nào được thanh toán hoặc COD thành công.

- Đơn nào đạt trạng thái doanh thu verified theo policy.

- Doanh thu nào được tính ROAS.

- Doanh thu nào phải loại trừ do hủy, hoàn, đổi, COD fail, payment fail, recall, sale lock, complaint hoặc data issue.

- ROAS, CPA, AOV có đủ tin cậy để scale hay không.

- Khi nào phải pause, stop, giảm ngân sách hoặc yêu cầu owner review.

PACK-07 là lớp đo lường và quyết định vận hành dựa trên dữ liệu đã được kiểm soát, không phải lớp tạo nghiệp vụ.

## 2. TRẠNG THÁI TÀI LIỆU

Trạng thái của PACK-07 sau khi hoàn tất tài liệu:

GOVERNANCE_DOCUMENTATION_COMPLETE — waiting IMPLEMENTATION / TEST / SMOKE / DATA QUALITY EVIDENCE / OWNER SIGN-OFF / SCALE DECISION / RELEASE DECISION

Trạng thái này có nghĩa:

- Tài liệu governance/domain đã hoàn chỉnh.

- Ranh giới owner và source-of-truth đã được khóa.

- Nguyên tắc đo lường Ads/ROAS/Attribution/Verified Revenue đã được xác lập.

- Các điều kiện Data Quality, Scale Gate và Stop Gate đã được định nghĩa.

- Chưa được xem là triển khai xong.

- Chưa được xem là có dashboard production.

- Chưa được xem là ROAS PASS.

- Chưa được xem là đủ điều kiện scale.

- Chưa được gọi Production Ready.

- Chưa được gọi Release Approved nếu chưa có implementation, test, smoke, accepted evidence, owner sign-off và release decision.

## 3. VAI TRÒ CỦA PACK-07 TRONG HỆ THỐNG GINSENGFOOD

PACK-07 giữ vai trò:

Ads Measurement & Attribution Governance Owner

PACK-07 làm chủ các lớp sau:

- Ads measurement principles.

- Campaign/ad/adset identity governance.

- Paid/organic/live/CRM/referral attribution governance.

- Funnel measurement governance.

- ROAS/CPA/AOV measurement governance.

- Verified revenue measurement dependency.

- Data quality control before reporting.

- Scale Gate governance.

- Stop/Pause Gate governance.

- Dashboard reliability governance.

- Decision evidence governance.

PACK-07 không làm chủ:

- Facebook Page, Webhook, Messenger, Live routing.

- AI tư vấn, AI trả lời, AI chốt đơn.

- Product Master.

- SKU sellable status.

- Inventory.

- Sale Lock.

- QuoteSnapshot.

- Official Order.

- Payment confirmation.

- COD success.

- Delivery success.

- Invoice.

- MISA sync.

- Accounting revenue.

- Commission.

- Tax/Voucher.

- Customer membership benefit.

- Diamond benefit.

- CRM message eligibility.

- Recall/Stop-sale.

PACK-07 chỉ được đo lường từ sự kiện hợp lệ do các owner pack phát hành hoặc được owner phê duyệt.

## 4. NGUYÊN TẮC CỐT LÕI

## 4.1. Ads Measurement không phải Commerce

Ads có thể tạo traffic.

Ads có thể tạo comment.

Ads có thể tạo inbox.

Ads có thể tạo lead.

Ads có thể tạo quote request.

Ads không tự tạo đơn hàng chính thức.

Ads không tự tạo doanh thu.

Ads không tự xác nhận thanh toán.

Ads không tự xác nhận COD thành công.

Ads không tự xác nhận giao hàng thành công.

Ads không tự tạo verified revenue.

Do đó, PACK-07 không được tính doanh thu chỉ từ:

- Click.

- Landing view.

- Comment.

- Inbox.

- Messenger conversation.

- Khách nói “lấy 2 hộp”.

- Khách gửi thông tin nhận hàng.

- QuoteSnapshot.

- Cart draft.

- Checkout form chưa xác nhận.

- Ảnh chuyển khoản chưa đối chiếu.

- Đơn chưa có official order/order_code.

- Đơn chưa đạt payment/COD/delivery/verified revenue theo policy.

Một tương tác quảng cáo chỉ được đi vào doanh thu khi có linkage đầy đủ đến owner runtime xác nhận trạng thái doanh thu hợp lệ.

## 4.2. Không có Verified Revenue thì không có ROAS chính thức

ROAS chính thức không được tính bằng doanh thu ước đoán.

ROAS chính thức không được tính bằng doanh thu từ comment.

ROAS chính thức không được tính bằng tổng quote.

ROAS chính thức không được tính bằng cart draft.

ROAS chính thức không được tính bằng đơn khách nói miệng.

ROAS chính thức chỉ được tính khi doanh thu đã đạt trạng thái verified theo rule của owner pack.

PACK-07 được phép hiển thị nhiều lớp doanh thu, nhưng phải phân biệt rõ:

- Gross order value.

- Confirmed order value.

- Paid revenue.

- COD successful revenue.

- Delivered revenue.

- Verified revenue.

- Net revenue sau hủy/hoàn/đổi nếu policy yêu cầu.

- Revenue waiting verification.

- Revenue excluded from ROAS.

ROAS production chỉ được dùng Verified Revenue hoặc revenue class được owner policy phê duyệt rõ.

## 4.3. Data Quality phải đi trước ROAS

PACK-07 không được đưa ROAS đẹp ra dashboard nếu dữ liệu chưa sạch.

Trước khi tính ROAS chính thức, hệ thống phải kiểm tra tối thiểu:

- Ads spend source có hợp lệ không.

- Campaign/adset/ad mapping có rõ không.

- Event có đi qua nguồn được phê duyệt không.

- Channel event có trace từ PACK-06 không.

- AI event có trace từ PACK-05 không.

- Quote event có liên kết đến conversation không.

- Order event có official order/order_code không.

- Payment/COD/delivery/verified revenue có hợp lệ không.

- Refund/cancel/return đã được loại trừ chưa.

- Duplicate event đã được xử lý chưa.

- Bot/spam/suppressed event đã được loại chưa.

- Organic/Paid/CRM/Diamond/referral conflict đã được xử lý chưa.

- Missing data có được đánh dấu không.

- Privacy-bị chặn event có bị loại không.

- Attribution rule có được ghi nhận không.

- Dashboard có phân biệt raw/clean/verified/excluded/waiting/missing không.

Nếu Data Quality không đạt, PACK-07 phải hiển thị trạng thái:

## ROAS_NOT_RELIABLE — SCALE_BLOCKED — OWNER_REVIEW_REQUIRED

## 4.4. Attribution không được suy diễn cảm tính

PACK-07 không được kết luận doanh thu thuộc Ads nếu thiếu attribution rule.

PACK-07 không được kết luận doanh thu thuộc Diamond nếu thiếu referral bind.

PACK-07 không được kết luận doanh thu thuộc CRM nếu thiếu CRM event linkage.

PACK-07 không được kết luận doanh thu thuộc Organic nếu sự kiện có paid context chưa xử lý.

PACK-07 không được tính trùng một đơn cho nhiều nguồn doanh thu.

Các tình huống bắt buộc có conflict resolution:

- Ads click trước, Diamond link sau.

- Diamond link trước, Ads remarketing sau.

- Live comment từ post được boost.

- Organic comment trong bài đang chạy ads.

- CRM nhắc mua lại cho khách từng đến từ ads.

- Khách cũ mua lại sau khi thấy ads.

- Khách tự gõ Messenger không có ad context.

- Khách vào từ landing page rồi quay lại Messenger.

- Khách vào từ link người giới thiệu rồi comment trên live.

- Khách nhiều lần tương tác qua nhiều campaign.

Không có attribution rule thì không được tính ROAS chính thức theo nguồn.

## 4.5. Ads Performance và AI Performance phải tách riêng

PACK-07 phải phân biệt hiệu quả quảng cáo và hiệu quả AI tư vấn.

Ads Performance đo:

- Spend.

- Impression.

- Reach.

- Click.

- CTR.

- CPC.

- CPM.

- Comment rate.

- Inbox rate.

- Lead rate.

- Cost per qualified conversation.

- CPA.

- ROAS.

- Campaign/adset/ad creative performance.

AI Advisor Performance đo:

- Intent recognition.

- Response quality.

- Product recommendation accuracy.

- Quote request handling.

- Quote generation rate.

- Cart/order capture rate.

- Handoff rate.

- Guard block count.

- Complaint detection.

- CRM response rate.

- Conversion from AI consult.

- Fail-closed count.

Không đổ lỗi cho Ads nếu AI tư vấn sai.

Không đổ lỗi cho AI nếu Ads đưa traffic kém.

Không đổ lỗi cho Facebook Gateway nếu Commerce chặn sellable.

Không đổ lỗi cho Commerce nếu channel event thiếu trace.

Không đổ lỗi cho dashboard nếu upstream chưa gửi verified revenue.

## 5. SOURCE-OF-TRUTH BOUNDARY

## 5.1. Bảng ranh giới source-of-truth

Lớp nghiệp vụ | Source-of-truth owner | PACK-07 được làm gì | PACK-07 không được làm gì

Facebook Page / Webhook / Messenger / Live | PACK-06 | Consume channel event đã kiểm soát | Không tự đọc raw event để kết luận production

AI tư vấn / intent / response / quote request | PACK-05 | Consume AI event hợp lệ | Không viết lại logic AI

Product / SKU / Recipe / Formula | Product Master / Operational Core | Consume product identity public/measurement field | Không sửa product master, không lộ BOM

Sellable / Stock / Sale Lock | Operational/Commerce owner | Consume sellable checkpoint | Không bán vượt sale lock

QuoteSnapshot | Commerce Runtime | Consume quote event | Không tự tính giá/khuyến mãi

Official Order | Order/Commerce owner | Consume order event | Không tự tạo đơn

Payment / COD | Payment/Order owner | Consume payment/COD event | Không tự xác nhận paid/COD success

Delivery | Fulfillment/Shipping owner | Consume delivery event | Không tự xác nhận giao hàng

Verified Revenue | Commerce/Finance owner | Consume verified revenue event | Không tự gọi doanh thu chính thức

MISA / Accounting | PACK-04 / Finance | Consume accounting checkpoint nếu được phép | Không sync MISA, không tạo chứng từ

CRM | CRM/AI/Commerce owner tùy rule | Consume CRM event | Không tự gửi CRM

Diamond Referral | Referral/Commerce owner | Consume referral bind/event | Không tự tính hoa hồng

Ads Spend | Approved Ads source | Consume spend record hợp lệ | Không tự bịa spend

Scale Decision | Owner decision + PACK-07 gate | Đề xuất scale/stop theo gate | Không tự scale nếu thiếu approval

## 5.2. Nguyên tắc không tạo parallel truth

PACK-07 không được tạo bảng sự thật song song cho:

- Doanh thu.

- Đơn hàng.

- Thanh toán.

- Giao hàng.

- Tồn kho.

- Sellable.

- Giá.

- Khuyến mãi.

- Member benefit.

- Diamond benefit.

- Commission.

- MISA accounting.

- Official customer status.

- Complaint resolution.

- Recall/sale lock.

PACK-07 chỉ được tạo lớp measurement projection, dashboard projection hoặc attribution analysis từ event hợp lệ.

Measurement projection không phải business truth.

Dashboard metric không được ghi ngược làm nghiệp vụ gốc.

ROAS metric không được sửa order.

CPA metric không được sửa payment.

Attribution result không được sửa official revenue nếu owner pack chưa cho phép.

## 6. PHỤ THUỘC BẮT BUỘC VỚI PACK-06

## 6.1. PACK-06 là Channel Event Truth cho Facebook/Messenger/Live

Đối với Facebook, Messenger và Live, PACK-07 phải dùng PACK-06 làm nguồn channel event đã được kiểm soát.

PACK-07 không được dùng trực tiếp Facebook raw event thiếu guard để báo cáo production nếu event đó chưa qua các bước:

- Page Registry validation.

- App/Permission validation.

- Webhook verification.

- Deduplication.

- Channel identity resolution.

- Public/private routing classification.

- Comment -> Messenger trace.

- Live session context.

- Spam/rate-limit suppression check.

- Privacy guard.

- Human takeover flag nếu có.

- Channel evidence.

Event không qua PACK-06 phải được đánh dấu:

## UNCONTROLLED_CHANNEL_EVENT — NOT_FOR_PRODUCTION_ROAS

## 6.2. Channel context bắt buộc

PACK-07 cần tối thiểu các nhóm context sau từ PACK-06:

- Page context.

- Post context.

- Comment context.

- Messenger thread context.

- Live session context.

- Public-to-private routing context.

- Handoff context.

- Human takeover context.

- Spam/rate-limit status.

- Privacy guard status.

- Channel error status.

- Evidence/correlation context.

Không có channel context thì không được kết luận nguồn tương tác.

Không có comment-to-Messenger trace thì không được gán Messenger order về comment/live/ad một cách chính thức.

Không có Page Registry thì không được đo production performance theo Page.

Không có Live session identity thì không được gán doanh thu về phiên live.

## 6.3. Public comment không phải order signal đầy đủ

PACK-07 có thể đo comment là funnel event.

PACK-07 không được gọi comment là order.

PACK-07 không được gọi live comment “chốt” là doanh thu.

PACK-07 không được gọi public reply là tư vấn hoàn chỉnh nếu policy yêu cầu private.

Comment chỉ là một lớp trong funnel:

Comment -> Qualified Comment -> Private Handoff -> Messenger Conversation -> Product Consult -> Quote -> Cart Draft -> Customer Confirmation -> Official Order -> Payment/COD/Delivery -> Verified Revenue

Comment không được nhảy thẳng thành revenue.

## 7. PHỤ THUỘC BẮT BUỘC VỚI PACK-05

## 7.1. PACK-05 là AI Advisor Runtime Owner

PACK-07 được đo lường hiệu quả AI, nhưng không được viết lại hoặc override AI Advisor.

PACK-07 chỉ consume AI event hợp lệ từ PACK-05:

- Intent event.

- Entity recognition event.

- Product recommendation event.

- Guard block event.

- Response event.

- Quote request event.

- Cart/order capture event.

- Complaint detection event.

- Handoff event.

- CRM event.

- Fail-closed event.

PACK-07 không được tự quyết định AI trả lời đúng hay sai nếu thiếu test/evidence rule.

PACK-07 có thể đánh dấu metric để owner review, nhưng không được tự sửa behavior production của AI.

## 7.2. Handoff theo policy không phải AI failure

Một số tình huống bắt buộc phải handoff:

- Khi cần xác nhận riêng tư.

- Khi có dữ liệu cá nhân.

- Khi có đơn hàng/hóa đơn/thanh toán.

- Khi có complaint/rủi ro chất lượng.

- Khi có câu hỏi vượt claim/safety.

- Khi có yêu cầu pháp lý.

- Khi có kênh public nhưng nội dung cần private.

- Khi AI không đủ runtime để trả lời an toàn.

Trong các trường hợp này, handoff đúng policy không được tính là AI failure.

PACK-07 phải phân biệt:

- Policy-required handoff.

- Risk handoff.

- Human takeover.

- AI failure handoff.

- Data-missing handoff.

- Channel-required private handoff.

Nếu không phân biệt handoff, dashboard sẽ đánh giá sai hiệu quả AI.

## 7.3. Complaint không tính là sales conversion

Complaint, quality issue, refund issue, delivery issue, invoice issue hoặc payment dispute không được tính là sales conversion.

Nếu khách đang trong complaint flow, PACK-07 phải đánh dấu conversion bị chặn theo rule.

Không được tính upsell từ khách đang có complaint mở nếu CRM/AI policy đã block.

Không được dùng complaint conversation để nâng tỷ lệ tư vấn bán hàng.

Complaint event cần đi vào risk metric, không đi vào sales performance như một lead bình thường.

## 8. PHỤ THUỘC BẮT BUỘC VỚI COMMERCE RUNTIME

## 8.1. Commerce Runtime là nguồn Quote/Order/Revenue hợp lệ

PACK-07 không được tự tính:

- Listed price.

- Program discount.

- Member discount.

- Diamond benefit.

- Final total.

- Shipping fee.

- COD fee.

- VAT display.

- Quote expiry.

- Order total.

- Refund amount.

- Revenue amount.

- Commission.

Các giá trị này phải đến từ owner runtime.

PACK-07 chỉ được đo lường khi có event/record hợp lệ từ Commerce/Order/Payment/Revenue owner.

## 8.2. QuoteSnapshot không phải revenue

QuoteSnapshot là báo giá có kiểm soát.

QuoteSnapshot không phải đơn hàng.

QuoteSnapshot không phải thanh toán.

QuoteSnapshot không phải doanh thu.

QuoteSnapshot chỉ có thể là một funnel event.

PACK-07 được đo:

- Quote request rate.

- Quote generation rate.

- Quote-to-confirmation rate.

- Quote-to-order rate.

- Quote-to-verified-revenue rate.

- Quote expiry rate.

PACK-07 không được tính tổng QuoteSnapshot thành doanh thu.

## 8.3. Official Order là điều kiện tối thiểu để đo order

Không có official order/order_code thì không được gọi là order chính thức.

Các trạng thái chưa đủ để gọi order:

- Khách comment “lấy 2 hộp”.

- Khách inbox “chốt”.

- Khách gửi tên/số điện thoại/địa chỉ.

- AI tạo cart draft.

- AI gửi form xác nhận.

- Khách gửi ảnh chuyển khoản.

- Nhân viên ghi chú ngoài hệ thống.

PACK-07 chỉ ghi nhận order khi owner runtime phát hành official order event hợp lệ.

## 8.4. Payment/COD/Delivery/Verified Revenue phải theo owner policy

Một đơn hàng có thể đi qua nhiều lớp:

- Official order created.

- Customer confirmed.

- Payment waiting.

- Payment confirmed.

- COD waiting.

- COD success.

- Delivery waiting.

- Delivery success.

- Return/refund/cancel.

- Order Verified.

- Revenue verified.

- Revenue excluded.

PACK-07 phải dùng đúng revenue class.

Không được gọi order created là paid revenue.

Không được gọi payment waiting là verified revenue.

Không được gọi delivery waiting là delivered revenue.

Không được gọi COD waiting là COD successful revenue.

Không được gọi revenue waiting verification là ROAS revenue chính thức.

## 9. PHỤ THUỘC BẮT BUỘC VỚI PACK-04 / MISA / FINANCE

## 9.1. PACK-07 không sync MISA

PACK-07 không được:

- Tạo chứng từ kế toán.

- Gửi chứng từ sang MISA.

- Sửa mapping MISA.

- Tự đối chiếu kế toán bằng dữ liệu không hợp lệ.

- Ghi ngược doanh thu vào MISA.

- Gọi một doanh thu là doanh thu kế toán nếu thiếu checkpoint.

Nếu cần số liệu kế toán hoặc đối chiếu MISA, PACK-07 chỉ được consume checkpoint/evidence hợp lệ từ PACK-04 hoặc Finance owner.

## 9.2. ROAS revenue không mặc định là accounting revenue

Doanh thu dùng cho ROAS và doanh thu kế toán có thể khác nhau về thời điểm ghi nhận.

PACK-07 phải phân biệt:

- Marketing verified revenue.

- Commerce verified revenue.

- Payment confirmed revenue.

- COD successful revenue.

- Delivered revenue.

- Accounting posted revenue.

- Reconciled accounting revenue.

- Net revenue after adjustment.

Không được dùng từ “doanh thu chính thức” nếu không chỉ rõ class.

Nếu dashboard dùng verified revenue cho ROAS, phải hiển thị rõ:

Revenue Basis = VERIFIED_REVENUE_FOR_MARKETING_MEASUREMENT

Nếu dùng kế toán đã post, phải hiển thị rõ:

Revenue Basis = ACCOUNTING_RECONCILED_REVENUE

## 10. PHÂN TÁCH ADS / AI / COMMERCE / CRM / REFERRAL

## 10.1. Ads Performance

Ads Performance là hiệu quả tạo traffic, tạo tương tác và tạo cơ hội bán hàng từ ngân sách quảng cáo.

Các chỉ số thuộc Ads Performance:

- Spend.

- Impression.

- Reach.

- CPM.

- Click.

- CTR.

- CPC.

- Landing view.

- Comment rate.

- Inbox rate.

- Cost per comment.

- Cost per qualified comment.

- Cost per Messenger conversation.

- Cost per qualified conversation.

- Cost per quote request.

- Cost per official order.

- Cost per verified order.

- ROAS theo verified revenue.

- Creative performance.

- Campaign/adset/ad performance.

Ads Performance không tự chứng minh AI tốt.

Ads Performance không tự chứng minh doanh thu tốt nếu revenue chưa verified.

## 10.2. AI Advisor Performance

AI Advisor Performance là hiệu quả tư vấn, điều hướng nhu cầu, đề xuất sản phẩm, quote/order handoff và xử lý tình huống khách hàng.

Các chỉ số thuộc AI Performance:

- Intent recognition rate.

- Correct product recommendation rate.

- Claim guard pass/fail.

- Quote request handling.

- Quote generation support.

- Order capture completion.

- Handoff correctness.

- Complaint detection.

- Guard block correctness.

- CRM response quality.

- Conversion after AI consult.

- AI-to-human escalation quality.

AI Performance phải tách khỏi Ads Performance để xác định đúng nguyên nhân tăng/giảm doanh thu.

## 10.3. Commerce Performance

Commerce Performance phản ánh việc quote, giá, order, payment, delivery, stock, sellable và verified revenue có vận hành đúng hay không.

Các vấn đề Commerce có thể ảnh hưởng ROAS:

- SKU hero hết hàng.

- SKU bị sale lock.

- Quote không tạo được.

- Giá runtime lỗi.

- Program không active.

- Member benefit không resolve được.

- Order không tạo được.

- Payment confirmation chậm.

- COD fail cao.

- Delivery fail cao.

- Refund/return cao.

- Revenue verification chậm.

PACK-07 phải đánh dấu nguyên nhân đến từ Commerce, không gán sai cho Ads hoặc AI.

## 10.4. CRM Performance

CRM Performance phản ánh doanh thu từ chăm sóc lại, mua lại, duy trì thành viên, nâng hạng, quà tặng, seasonal suggestion hoặc after-sales.

CRM revenue không được tính trùng với Ads revenue nếu attribution policy không cho phép.

Một khách từng đến từ Ads nhưng mua lại sau CRM không mặc định là Ads revenue.

Một khách thấy remarketing Ads sau CRM phải có attribution rule rõ.

CRM rebuy revenue phải có classification riêng nếu dùng trong dashboard.

## 10.5. Diamond / Referral Performance

Diamond/referral revenue phải dựa trên referral bind hợp lệ.

Không có referral bind thì không được gọi là Diamond revenue.

PACK-07 không tự tính hoa hồng.

PACK-07 chỉ đo:

- Referral entry.

- Referral bind.

- Referral conflict.

- Referral-attributed order.

- Referral-attributed verified revenue.

- Commission eligibility event nếu owner pack phát hành.

- Commission status nếu Finance/Commerce owner cung cấp.

Revenue từ Diamond/referral phải tách khỏi Ads revenue nếu policy yêu cầu.

## 11. ATTRIBUTION GOVERNANCE PRINCIPLES

## 11.1. Attribution là rule-governed, không phải cảm tính

Attribution phải dựa trên rule được owner phê duyệt.

PACK-07 không được dùng cách nhìn thủ công để kết luận “đơn này do ads”.

Mỗi conversion cần có:

- Customer identity hoặc session identity hợp lệ.

- Channel identity.

- Campaign/ad/adset identity nếu có.

- Entry event.

- Touchpoint chain.

- Quote/order/revenue linkage.

- Attribution model used.

- Conflict resolution nếu có.

- Evidence.

- Data quality result.

## 11.2. Attribution model phải khai báo rõ

PACK-07 có thể hỗ trợ nhiều mô hình attribution, nhưng mỗi báo cáo phải nêu rõ model đang dùng.

Các mô hình có thể có:

- First touch.

- Last touch.

- Last paid touch.

- Last non-direct touch.

- Session-based attribution.

- Campaign-bound attribution.

- Live-session attribution.

- Diamond referral priority attribution.

- CRM rebuy attribution.

- Owner-approved custom attribution.

Không được trộn nhiều model trong cùng một chỉ số mà không khai báo.

## 11.3. Attribution conflict phải được đánh dấu

Khi một order có nhiều nguồn nhận công, PACK-07 phải đánh dấu conflict.

Ví dụ conflict:

- Paid Ads + Organic Comment.

- Paid Ads + Live Session.

- Paid Ads + Diamond Referral.

- Diamond Referral + CRM.

- CRM + Remarketing Ads.

- Organic Messenger + Ads Click cũ.

- Member rebuy + Ads view.

- MC AI Live + Facebook Ads.

- Landing Page + Messenger.

- Human telesales + AI Advisor.

Conflict chưa xử lý không được đưa vào ROAS chính thức.

Trạng thái bắt buộc:

## ATTRIBUTION_CONFLICT — OWNER_RULE_REQUIRED

hoặc

## ATTRIBUTION_CONFLICT_RESOLVED — RULE_APPLIED

## 12. DATA QUALITY PRINCIPLES

## 12.1. Các lớp dữ liệu phải phân biệt rõ

PACK-07 dashboard phải phân biệt:

- Raw data.

- Normalized data.

- Clean data.

- Verified data.

- Excluded data.

- waiting data.

- Missing data.

- Duplicated data.

- Conflict data.

- Suppressed data.

- Privacy-bị chặn data.

- Owner-review data.

Không được gom tất cả thành số tổng.

Không được ẩn data issue.

Không được hiển thị ROAS như PASS nếu dữ liệu chưa đạt.

## 12.2. Data Quality status

Mỗi tập dữ liệu đo lường phải có trạng thái chất lượng.

Các trạng thái tối thiểu:

- DATA_RAW.

- DATA_NORMALIZED.

- DATA_CLEAN_waiting.

- DATA_CLEAN_PASS.

- DATA_QUALITY_WARNING.

- DATA_QUALITY_FAIL.

- DATA_MISSING_REQUIRED_FIELD.

- DATA_DUPLICATE_DETECTED.

- DATA_PRIVACY_BLOCKED.

- DATA_SPAM_SUPPRESSED.

- DATA_ATTRIBUTION_CONFLICT.

- DATA_REVENUE_NOT_VERIFIED.

- DATA_OWNER_REVIEW_REQUIRED.

Khi data quality chưa PASS, dashboard chỉ được hiển thị dạng cảnh báo, không được dùng để scale chính thức.

## 12.3. Missing data không được im lặng

Nếu thiếu dữ liệu, PACK-07 phải hiển thị rõ thiếu gì.

Ví dụ:

- Thiếu campaign id.

- Thiếu adset id.

- Thiếu ad id.

- Thiếu Page context.

- Thiếu comment-to-Messenger trace.

- Thiếu quote id.

- Thiếu order code.

- Thiếu payment status.

- Thiếu delivery status.

- Thiếu verified revenue event.

- Thiếu attribution rule.

- Thiếu owner approval.

Missing data không được tự lấp bằng suy luận.

Missing data không được chuyển thành organic mặc định.

Missing data không được chuyển thành Ads mặc định.

## 13. ADS SPEND SOURCE GOVERNANCE

## 13.1. Ads spend phải có nguồn hợp lệ

PACK-07 chỉ được đo ROAS nếu Ads spend đến từ nguồn hợp lệ.

Ads spend source phải có:

- Nguồn dữ liệu được owner phê duyệt.

- Campaign identity.

- Adset identity.

- Ad identity nếu áp dụng.

- Time window.

- Currency.

- Spend amount.

- Account/page/business context.

- Import/sync status.

- Evidence snapshot.

- Data quality status.

Không có spend source hợp lệ thì không được tính ROAS.

## 13.2. Spend không được tự nhập tay tùy tiện

Nếu có cơ chế nhập tay spend tạm thời, phải có:

- Owner approval.

- Evidence đính kèm.

- Người nhập.

- Lý do nhập.

- Ngày giờ nhập.

- Phạm vi áp dụng.

- Reconcile sau đó.

- Cảnh báo dashboard.

Spend nhập tay chưa reconcile không được dùng cho ROAS PASS nếu owner policy không cho phép.

## 14. VERIFIED REVENUE DEPENDENCY

## 14.1. Revenue phải đi từ owner event

PACK-07 chỉ consume revenue từ owner runtime.

Các nguồn không hợp lệ để tự tính revenue:

- Comment public.

- Messenger text.

- QuoteSnapshot.

- Cart draft.

- Excel ngoài hệ thống.

- Nhân viên báo miệng.

- Ảnh chuyển khoản chưa đối chiếu.

- Mã vận đơn chưa giao thành công.

- MISA record không có link về order nếu policy yêu cầu.

- Dashboard Ads tự nhập doanh thu.

Revenue phải có trace đến official order và trạng thái verification hợp lệ.

## 14.2. Revenue exclusion bắt buộc

Các trường hợp phải loại khỏi ROAS revenue nếu policy quy định:

- Order canceled.

- Payment failed.

- COD failed.

- Delivery failed.

- Returned order.

- Refunded order.

- Duplicate order.

- Test order.

- Internal staff order nếu không tính.

- Sample/gift order nếu không tính.

- Complaint unresolved.

- Recall/sale lock impacted order.

- Fraud/spam order.

- Attribution conflict unresolved.

- Revenue waiting verification.

Không có exclusion rule thì không được báo ROAS chính thức.

## 15. SALE LOCK / STOCK / FULFILLMENT DEPENDENCY

PACK-07 không được scale quảng cáo nếu sản phẩm hero đang bị chặn bởi:

- Out of stock.

- Not sellable.

- Sale Lock.

- Recall.

- Quality hold.

- Warehouse hold.

- Channel hold.

- Program inactive.

- Price inactive.

- Quote unavailable.

- Fulfillment overload.

- Payment/COD failure vượt ngưỡng.

- Delivery delay vượt ngưỡng.

- Complaint tăng bất thường.

PACK-07 phải consume trạng thái từ owner pack.

Nếu Ads đang tốt nhưng stock/sellable bị chặn, Scale Gate phải block.

Nếu Sale Lock đang ảnh hưởng SKU hero, Stop Gate phải kích hoạt theo policy.

## 16. OWNER DECISION PRINCIPLES

## 16.1. PACK-07 đề xuất, owner quyết định

PACK-07 có thể đưa khuyến nghị:

- Scale allowed.

- Scale bị chặn.

- Pause recommended.

- Stop required.

- Owner review required.

- Data quality fix required.

- Attribution rule required.

- Revenue verification waiting.

- Fulfillment constraint warning.

- Stock/sellable constraint warning.

PACK-07 không tự tăng ngân sách nếu chưa có owner approval hoặc rule automation được owner phê duyệt.

PACK-07 không tự tắt campaign production nếu chưa có Stop Gate policy hoặc owner decision.

## 16.2. Scale không được dựa trên cảm tính

Không được scale vì:

- Nhiều comment.

- Nhiều inbox.

- Live đông.

- Cảm giác khách thích.

- ROAS tạm tính đẹp nhưng revenue chưa verified.

- Một vài đơn lớn chưa đối chiếu.

- AI báo khách quan tâm nhiều.

- Dashboard raw data tăng nhưng data quality chưa pass.

Scale chỉ được xét khi Data Quality, Attribution, Verified Revenue, CPA, ROAS, fulfillment, stock/sellable và owner approval đạt điều kiện.

## 17. QUY TẮC FAIL-CLOSED CỦA PACK-07

PACK-07 phải fail-closed trong các trường hợp:

- Thiếu Ads spend source.

- Thiếu campaign/adset/ad identity.

- Thiếu channel trace.

- Thiếu comment-to-Messenger trace.

- Thiếu AI event khi đo AI performance.

- Thiếu quote/order linkage.

- Thiếu official order.

- Thiếu verified revenue.

- Revenue waiting verification.

- Attribution conflict chưa xử lý.

- Duplicate event chưa xử lý.

- Spam/bot event chưa loại.

- Refund/cancel/return chưa loại.

- Sale Lock ảnh hưởng SKU.

- Privacy incident.

- Claim incident.

- Dashboard data mismatch.

- Owner approval missing.

Fail-closed nghĩa là:

- Không tính ROAS chính thức.

- Không cho Scale PASS.

- Không gọi số liệu là verified.

- Không ẩn cảnh báo.

- Chuyển owner review nếu cần.

## 18. MINIMUM GOVERNANCE OUTPUT CỦA PACK-07

PACK-07 phải tạo được các lớp output governance sau:

- Ads Measurement Status.

- Attribution Status.

- Funnel Data Quality Status.

- Revenue Verification Status.

- ROAS Reliability Status.

- CPA Reliability Status.

- Dashboard Reliability Status.

- Scale Gate Status.

- Stop/Pause Gate Status.

- Owner Decision Status.

- Evidence Status.

- Release Readiness Status.

Các output này không thay thế nghiệp vụ gốc.

Các output này dùng để kiểm soát đo lường, quyết định ads và cảnh báo vận hành.

## 19. DONE CONDITION CỦA PHẦN 1/4

## PHẦN 1/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Mục tiêu PACK-07.

- Trạng thái tài liệu.

- Vai trò PACK-07 trong hệ thống.

- Ranh giới Ads Measurement không phải Commerce.

- Không có Verified Revenue thì không có ROAS chính thức.

- Data Quality đi trước ROAS.

- Attribution không suy diễn cảm tính.

- Ads Performance tách khỏi AI Performance.

- Source-of-truth boundary.

- Phụ thuộc PACK-06.

- Phụ thuộc PACK-05.

- Phụ thuộc Commerce Runtime.

- Phụ thuộc PACK-04/MISA/Finance.

- Phân tách Ads/AI/Commerce/CRM/Referral.

- Attribution governance principles.

- Data quality principles.

- Ads spend source governance.

- Verified revenue dependency.

- Sale Lock/Stock/Fulfillment dependency.

- Owner decision principles.

- Fail-closed rule.

## PHẦN 1/4 chưa định nghĩa đầy đủ Event Taxonomy, Funnel Metrics, Attribution Model chi tiết, Dashboard KPI, Scale Gate, Stop Gate, Smoke Matrix hoặc Release Control. Các nội dung đó thuộc PHẦN 2/4, PHẦN 3/4 và PHẦN 4/4.

## KẾT LUẬN PHẦN 1/4

PACK-07 là lớp đo lường Ads/ROAS/Attribution/Verified Revenue của hệ thống Ginsengfood.

PACK-07 không phải lớp tạo doanh thu.

PACK-07 không phải lớp chốt đơn.

PACK-07 không phải lớp xác nhận thanh toán.

PACK-07 không phải lớp kế toán.

PACK-07 không phải lớp AI tư vấn.

PACK-07 không phải Facebook Channel Gateway.

PACK-07 chỉ được đo lường từ dữ liệu hợp lệ, có trace, có owner source-of-truth, có attribution rule, có data quality pass và có evidence.

Nguyên tắc cốt lõi của PACK-07 là:

Không có Data Quality thì không có ROAS đáng tin cậy.

Không có Verified Revenue thì không có ROAS chính thức.

Không có Attribution Rule thì không có kết luận nguồn doanh thu.

Không có Owner Approval thì không scale production.

Không có Evidence thì không PASS.

## PHẦN 2/4 — EVENT TAXONOMY / ATTRIBUTION MODEL / FUNNEL METRICS / VERIFIED REVENUE RULES

## 20. MỤC TIÊU CỦA PHẦN 2/4

## PHẦN 2/4 thiết lập hệ quy chiếu đo lường sự kiện, phễu chuyển đổi, phân bổ nguồn doanh thu và quy tắc xác nhận doanh thu được dùng cho PACK-07.

Mục tiêu của phần này là khóa rõ:

- Event nào được phép dùng để đo Ads.

- Event nào chỉ là tín hiệu tương tác.

- Event nào là tín hiệu tư vấn.

- Event nào là tín hiệu quote.

- Event nào là tín hiệu order.

- Event nào là tín hiệu payment.

- Event nào đủ điều kiện đi vào verified revenue.

- Event nào phải loại khỏi ROAS.

- Event nào cần owner review.

- Event nào có conflict attribution.

- Event nào không đủ data quality để đưa vào dashboard chính thức.

- Event nào có thể dùng cho scale decision.

## PHẦN 2/4 không thiết kế database, API, UI hay code.

## PHẦN 2/4 chỉ khóa logic governance/domain để dev triển khai đúng theo source-of-truth và owner boundary.

## 21. NGUYÊN TẮC EVENT TAXONOMY

## 21.1. Event là dấu vết đo lường, không mặc định là sự thật nghiệp vụ

Một event trong PACK-07 là dấu vết đo lường.

Event không mặc định là đơn hàng.

Event không mặc định là doanh thu.

Event không mặc định là thanh toán.

Event không mặc định là giao hàng thành công.

Event không mặc định là ROAS hợp lệ.

Một event chỉ được dùng để kết luận nghiệp vụ khi event đó đến từ owner pack hoặc được owner pack xác nhận.

Ví dụ:

- COMMENT_EVENT chỉ chứng minh có comment.

- MESSENGER_EVENT chỉ chứng minh có tương tác Messenger.

- QUOTE_EVENT chỉ chứng minh có quote.

- CART_EVENT chỉ chứng minh có cart draft hoặc cart action.

- ORDER_EVENT chỉ chứng minh có order khi đến từ owner Order/Commerce.

- PAYMENT_EVENT chỉ chứng minh payment khi đến từ owner Payment.

- VERIFIED_REVENUE_EVENT mới là cơ sở doanh thu verified nếu policy xác nhận.

- ATTRIBUTION_EVENT chỉ có ý nghĩa khi có rule attribution hợp lệ.

- DATA_QUALITY_EVENT quyết định dữ liệu có đủ điều kiện dùng hay không.

Không được dùng event ở tầng thấp để suy diễn kết quả ở tầng cao.

## 21.2. Event phải có nguồn, chủ sở hữu và phạm vi sử dụng

Mỗi event dùng trong PACK-07 phải xác định tối thiểu:

- Event family.

- Event owner.

- Event source.

- Event timestamp.

- Event scope.

- Event correlation.

- Event identity.

- Event data quality status.

- Event evidence.

- Event allowed usage.

Nếu thiếu owner hoặc source hợp lệ, event không được dùng để tính ROAS production.

Nếu thiếu correlation, event không được dùng để nối phễu conversion chính thức.

Nếu thiếu evidence, event không được dùng để PASS.

## 21.3. Event phải được chống trùng

PACK-07 không được tính lặp event.

Các tình huống trùng phải xử lý:

- Một comment được gửi webhook nhiều lần.

- Một Messenger event được retry nhiều lần.

- Một khách gửi nhiều tin giống nhau.

- Một quote được tạo lại do hết hạn.

- Một cart draft được sửa nhiều lần.

- Một order có nhiều trạng thái payment.

- Một order có nhiều trạng thái delivery.

- Một revenue event được cập nhật nhiều lần.

- Một campaign spend được import nhiều lần.

- Một khách xuất hiện ở nhiều touchpoint.

Nếu chưa chống trùng, metric phải ở trạng thái:

## DATA_DUPLICATE_DETECTED — ROAS_NOT_RELIABLE

## 22. EVENT TAXONOMY REGISTRY

## 22.1. Danh mục event family tối thiểu

Event family | Vai trò đo lường | Owner/source bắt buộc | Không được dùng để

CHANNEL_EVENT | Ghi nhận sự kiện kênh tổng quát | PACK-06 hoặc nguồn channel được phê duyệt | Không tự tính order/revenue

AD_EVENT | Ghi nhận ads impression/click/spend/campaign context | Ads source được owner phê duyệt | Không tự tính doanh thu

COMMENT_EVENT | Ghi nhận comment public | PACK-06 | Không gọi là đơn hàng

MESSENGER_EVENT | Ghi nhận tương tác Messenger/private | PACK-06 | Không gọi là order nếu chưa có owner order

LIVE_EVENT | Ghi nhận phiên live/live comment/live routing | PACK-06 | Không gọi là revenue nếu chưa verified

AI_INTENT_EVENT | Ghi nhận intent AI nhận diện | PACK-05 | Không kết luận conversion

AI_RESPONSE_EVENT | Ghi nhận phản hồi AI/guard/handoff | PACK-05 | Không tự chứng minh doanh thu

QUOTE_EVENT | Ghi nhận quote request/QuoteSnapshot | Commerce Runtime/PACK-05 handoff | Không gọi là revenue

CART_EVENT | Ghi nhận cart draft/cart update | Commerce/Order owner | Không gọi là official order

ORDER_EVENT | Ghi nhận official order/order status | Order/Commerce owner | Không gọi là paid revenue nếu chưa payment

PAYMENT_EVENT | Ghi nhận payment/COD status | Payment/Order owner | Không gọi là verified revenue nếu chưa policy pass

DELIVERY_EVENT | Ghi nhận shipping/delivery status | Fulfillment/Shipping owner | Không gọi là verified revenue nếu thiếu rule

VERIFIED_REVENUE_EVENT | Ghi nhận doanh thu đủ điều kiện đo lường | Commerce/Finance/Revenue owner | Không tự tạo từ dashboard

CRM_EVENT | Ghi nhận CRM interaction/rebuy campaign | CRM/AI/Commerce owner | Không tự gán Ads revenue

COMPLAINT_EVENT | Ghi nhận complaint/quality/payment/delivery issue | AI/CSKH/Quality owner | Không tính sales conversion

HANDOFF_EVENT | Ghi nhận chuyển người thật/handoff private | PACK-05/PACK-06 | Không mặc định là AI failure

REFERRAL_EVENT | Ghi nhận Diamond/referral bind/touchpoint | Referral/Commerce owner | Không tự tính commission

ATTRIBUTION_EVENT | Ghi nhận kết quả phân bổ nguồn | PACK-07 theo owner-approved rule | Không sửa revenue gốc

DATA_QUALITY_EVENT | Ghi nhận chất lượng dữ liệu | PACK-07 | Không thay thế event gốc

SCALE_DECISION_EVENT | Ghi nhận quyết định scale/stop/pause | Owner decision + PACK-07 gate | Không tự scale nếu thiếu approval

## 22.2. Event family không được vượt quyền

Mỗi event family chỉ được dùng đúng phạm vi.

COMMENT_EVENT không được vượt lên thành ORDER_EVENT.

QUOTE_EVENT không được vượt lên thành VERIFIED_REVENUE_EVENT.

PAYMENT_EVENT waiting không được vượt lên thành VERIFIED_REVENUE_EVENT.

CRM_EVENT không được vượt lên thành Ads revenue nếu attribution chưa xử lý.

REFERRAL_EVENT không được vượt lên thành commission nếu owner Finance/Commerce chưa xác nhận eligibility.

ATTRIBUTION_EVENT không được sửa order/revenue gốc.

DATA_QUALITY_EVENT không được xóa event gốc, chỉ được đánh dấu dữ liệu có đủ điều kiện dùng hay không.

## 23. EVENT LIFECYCLE

## 23.1. Trạng thái vòng đời event

Mỗi event đo lường cần đi qua các trạng thái tối thiểu:

- RAW_RECEIVED.

- SOURCE_VALIDATED.

- NORMALIZED.

- DEDUP_CHECKED.

- IDENTITY_RESOLVED.

- CORRELATION_LINKED.

- DATA_QUALITY_CHECKED.

- ATTRIBUTION_CHECKED nếu cần.

- ELIGIBILITY_CHECKED.

- INCLUDED hoặc EXCLUDED.

- EVIDENCE_ATTACHED.

- READY_FOR_REPORTING nếu đạt.

- OWNER_REVIEW_REQUIRED nếu chưa đủ điều kiện.

Không phải event nào cũng cần đi đến reporting.

Một event bị fail ở bất kỳ chặng P0 nào phải được giữ lịch sử nhưng không được dùng cho ROAS chính thức.

## 23.2. Event bị loại không được xóa

Event bị loại khỏi ROAS không được xóa khỏi hệ thống đo lường.

Event bị loại phải có lý do loại trừ.

Các lý do loại trừ tối thiểu:

- DUPLICATE.

- SPAM.

- BOT_SUSPECTED.

- PRIVACY_BLOCKED.

- CHANNEL_ERROR.

- MISSING_CAMPAIGN_CONTEXT.

- MISSING_CHANNEL_TRACE.

- MISSING_QUOTE_LINKAGE.

- MISSING_ORDER_CODE.

- PAYMENT_NOT_CONFIRMED.

- DELIVERY_NOT_CONFIRMED nếu policy yêu cầu.

- REVENUE_NOT_VERIFIED.

- ORDER_CANCELLED.

- ORDER_REFUNDED.

- ORDER_RETURNED.

- TEST_ORDER.

- INTERNAL_ORDER.

- ATTRIBUTION_CONFLICT_UNRESOLVED.

- OWNER_POLICY_EXCLUDED.

- SALE_LOCK_IMPACTED.

- COMPLAINT_UNRESOLVED.

Loại trừ phải có evidence/audit.

## 24. FUNNEL MEASUREMENT PRINCIPLES

## 24.1. Funnel là chuỗi chuyển đổi, không phải một bước nhảy

PACK-07 phải đo funnel theo từng chặng.

Không được gọi comment là đơn hàng.

Không được gọi inbox là doanh thu.

Không được gọi quote là order.

Không được gọi cart draft là order chính thức.

Không được gọi order created là paid revenue.

Không được gọi paid waiting là verified revenue.

Chuỗi funnel chuẩn tối thiểu:

Impression -> Reach -> Click -> Landing View -> Comment -> Qualified Comment -> Private Handoff -> Messenger Conversation -> Qualified Conversation -> Product Consult -> Quote Request -> Quote Generated -> Cart Draft -> Customer Confirmation -> Official Order -> Payment/COD Confirmation -> Delivery Success nếu policy yêu cầu -> Order Verified -> Verified Revenue

## 24.2. Funnel stage registry

Funnel stage | Ý nghĩa | Điều kiện tối thiểu | Không được kết luận

Impression | Quảng cáo được hiển thị | Ads source hợp lệ | Không kết luận quan tâm

Reach | Người dùng được tiếp cận | Ads source hợp lệ | Không kết luận lead

Click | Có click | Campaign/ad context | Không kết luận khách chất lượng

Landing View | Có xem trang/đích | Landing/channel event hợp lệ | Không kết luận quote

Comment | Có comment | PACK-06 comment event | Không kết luận order

Qualified Comment | Comment có ý định đủ đo | Rule qualification | Không kết luận revenue

Private Handoff | Kéo/điều hướng private hợp lệ | PACK-06 routing trace | Không kết luận AI success nếu chưa consult

Messenger Conversation | Có hội thoại private | PACK-06 Messenger event | Không kết luận quote

Qualified Conversation | Hội thoại đủ chất lượng | Qualification rule | Không kết luận order

Product Consult | Có tư vấn sản phẩm | PACK-05 event | Không kết luận chốt

Quote Request | Khách có nhu cầu báo giá | PACK-05/Commerce event | Không kết luận revenue

Quote Generated | QuoteSnapshot được tạo | Commerce Runtime | Không kết luận order

Cart Draft | Giỏ/đơn nháp | Commerce/Order owner | Không kết luận official order

Customer Confirmation | Khách xác nhận | Order/Commerce owner | Không kết luận paid

Official Order | Đơn chính thức/order_code | Order owner | Không kết luận verified revenue

Payment/COD Confirmation | Thanh toán/COD đạt trạng thái policy | Payment/Order owner | Không kết luận net revenue nếu còn return/refund

Delivery Success | Giao thành công nếu policy yêu cầu | Fulfillment owner | Không kết luận kế toán nếu chưa reconcile

Order Verified | Đơn được xác minh theo policy | Commerce/Order owner | Không mặc định là accounting posted

Verified Revenue | Doanh thu đủ điều kiện đo ROAS | Revenue owner | Chỉ dùng theo revenue basis đã khai báo

## 25. COMMENT -> MESSENGER -> QUOTE -> ORDER -> VERIFIED REVENUE

## 25.1. Comment chỉ là điểm vào funnel

Comment public là tín hiệu tương tác.

Comment có thể là:

- Hỏi giá.

- Hỏi sản phẩm.

- Hỏi thành phần.

- Hỏi Sâm Savigin.

- Hỏi ship.

- Đòi tư vấn riêng.

- Muốn mua.

- Chốt mơ hồ.

- Spam.

- Khiếu nại.

- Trêu đùa.

- Không liên quan.

PACK-07 chỉ được nâng comment thành Qualified Comment khi rule qualification pass.

Comment “chốt”, “lấy 2”, “ib”, “giá sao”, “mua thế nào” không phải order.

Comment có nhu cầu riêng phải route private theo PACK-06/PACK-05 policy.

## 25.2. Messenger là môi trường tư vấn/order capture, không tự là order

Messenger conversation có thể chứa:

- Tư vấn sản phẩm.

- Hỏi giá.

- Hỏi chương trình.

- Hỏi thành viên.

- Hỏi ship.

- Cung cấp thông tin nhận hàng.

- Xác nhận quote.

- Gửi ảnh chuyển khoản.

- Khiếu nại.

- Hủy đơn.

- Thay đổi đơn.

PACK-07 không được gọi Messenger conversation là order nếu chưa có ORDER_EVENT hợp lệ.

Messenger text không được tự tạo revenue.

Messenger chỉ là một tầng trong funnel nếu chưa có owner order/payment/revenue event.

## 25.3. Quote là chặng giá, không phải doanh thu

QUOTE_EVENT phải phân biệt:

- Quote requested.

- Quote generated.

- Quote shown.

- Quote accepted by customer.

- Quote expired.

- Quote cancelled.

- Quote superseded.

- Quote bị chặn due to sellable/stock/sale lock.

- Quote failed due to runtime missing.

- Quote owner review required.

Quote generated không phải revenue.

Quote accepted không phải paid.

Quote expired không được tính conversion.

Quote bị chặn do sale lock không được tính là AI failure nếu PACK-05/PACK-06 xử lý đúng policy.

## 25.4. Cart draft không phải official order

Cart draft có thể được tạo trong quá trình chốt đơn.

Cart draft không được tính là order chính thức nếu chưa có Customer Confirmation và owner Order tạo official order/order_code.

PACK-07 có thể đo:

- Quote-to-cart rate.

- Cart completion rate.

- Cart abandonment rate.

- Cart bị chặn rate.

- Cart-to-order rate.

PACK-07 không được tính cart draft vào revenue.

## 25.5. Official Order là chặng order chính thức

Official order phải đến từ owner Order/Commerce.

Official order cần có order identity hợp lệ.

Không có order_code thì không gọi là đơn đã ghi nhận chính thức.

ORDER_EVENT phải cho biết tối thiểu:

- Order created.

- Order confirmed.

- Order cancelled.

- Order changed.

- Order payment status.

- Order delivery status.

- Order verification status.

- Order exclusion status nếu có.

PACK-07 chỉ consume trạng thái này để đo funnel.

PACK-07 không được sửa order.

## 25.6. Verified Revenue là chặng doanh thu đủ điều kiện đo ROAS

Verified Revenue phải đến từ owner revenue policy.

Một order chỉ được đi vào ROAS chính thức khi đạt điều kiện verified revenue theo rule.

Verified Revenue có thể phụ thuộc vào:

- Payment confirmed.

- COD success.

- Delivery success.

- No cancellation.

- No refund.

- No return.

- No duplicate.

- No test order.

- No unresolved complaint nếu policy yêu cầu.

- No attribution conflict unresolved.

- No revenue exclusion rule triggered.

- Owner revenue verification pass.

Nếu thiếu Verified Revenue event, PACK-07 phải giữ doanh thu ở trạng thái waiting.

## 26. REVENUE CLASSIFICATION RULES

## 26.1. Các lớp doanh thu bắt buộc phân biệt

PACK-07 phải phân biệt tối thiểu các lớp doanh thu sau:

- Gross Order Value.

- Confirmed Order Value.

- Payment waiting Value.

- Paid Revenue.

- COD waiting Value.

- COD Successful Revenue.

- Delivery waiting Value.

- Delivered Revenue.

- Verified Revenue.

- Net Verified Revenue.

- Revenue waiting Verification.

- Revenue Excluded From ROAS.

- Refund/Return Adjustment.

- Accounting Reconciled Revenue nếu có.

- Marketing Measurement Revenue.

Không được gom các lớp trên thành một dòng “doanh thu” chung.

## 26.2. Gross Order Value

Gross Order Value là tổng giá trị đơn trước khi hoàn tất các điều kiện xác nhận sau cùng.

Gross Order Value không dùng để tính ROAS chính thức nếu policy yêu cầu verified revenue.

Gross Order Value chỉ dùng để quan sát pipeline hoặc forecast.

## 26.3. Confirmed Order Value

Confirmed Order Value là giá trị đơn đã được khách xác nhận và owner Order ghi nhận.

Confirmed Order Value chưa phải paid revenue.

Confirmed Order Value chưa phải verified revenue.

Confirmed Order Value có thể bị hủy, đổi, hoàn, COD fail hoặc payment fail.

## 26.4. Paid Revenue

Paid Revenue là doanh thu đã có payment confirmation hợp lệ.

Paid Revenue phải đến từ Payment owner.

Ảnh chuyển khoản chưa đối chiếu không phải Paid Revenue.

Khách nói “đã chuyển khoản” không phải Paid Revenue.

Nhân viên thấy tin nhắn chuyển khoản nhưng chưa đối chiếu không phải Paid Revenue.

## 26.5. COD Successful Revenue

COD Successful Revenue là doanh thu COD đạt trạng thái thành công theo owner policy.

COD waiting không phải COD Successful Revenue.

Đơn đang giao không phải COD Successful Revenue.

Đơn giao thất bại không phải COD Successful Revenue.

Đơn hoàn về không phải COD Successful Revenue.

## 26.6. Delivered Revenue

Delivered Revenue chỉ được dùng khi Fulfillment/Shipping owner xác nhận giao thành công.

Nếu policy yêu cầu delivery success trước khi verified, Delivered Revenue là điều kiện trung gian.

Nếu payment đã thành công nhưng giao thất bại/hoàn hàng, revenue phải xử lý theo exclusion/adjustment rule.

## 26.7. Verified Revenue

Verified Revenue là lớp doanh thu được phép dùng làm cơ sở ROAS chính thức nếu owner policy xác nhận.

Verified Revenue phải có:

- Official order.

- Payment/COD/delivery status đạt điều kiện.

- No active exclusion.

- Attribution đủ điều kiện.

- Data quality pass.

- Revenue owner confirmation.

- Evidence.

Verified Revenue không tự sinh từ Ads dashboard.

## 26.8. Net Verified Revenue

Net Verified Revenue là Verified Revenue sau khi trừ các điều chỉnh hợp lệ nếu policy yêu cầu.

Các điều chỉnh có thể gồm:

- Refund.

- Return.

- Partial refund.

- Discount correction.

- Cancelled line item.

- Failed COD adjustment.

- Duplicate correction.

- Owner-approved adjustment.

Nếu báo cáo dùng Net Verified Revenue, phải ghi rõ revenue basis.

## 27. REVENUE EXCLUSION RULES

## 27.1. Nguyên tắc loại trừ doanh thu

Một order không được đưa vào ROAS chính thức nếu có exclusion rule đang active.

Revenue exclusion không xóa order.

Revenue exclusion chỉ loại order khỏi chỉ số đo lường ROAS theo policy.

Mỗi exclusion phải có:

- Lý do.

- Event nguồn.

- Owner.

- Timestamp.

- Evidence.

- Review status nếu cần.

## 27.2. Danh mục exclusion tối thiểu

Các trường hợp phải có rule loại trừ:

- Order cancelled.

- Payment failed.

- Bank transfer not reconciled.

- COD failed.

- Delivery failed.

- Order returned.

- Order refunded.

- Partial refund.

- Duplicate order.

- Test order.

- Internal order.

- Staff order nếu policy loại trừ.

- Gift/sample order nếu policy loại trừ.

- Fraud suspected.

- Spam/bot order.

- Complaint unresolved.

- Recall impacted.

- Sale Lock impacted.

- Data quality fail.

- Attribution conflict unresolved.

- Missing verified revenue event.

- Owner policy excluded.

## 27.3. Revenue waiting không được tính ROAS chính thức

Revenue waiting verification phải tách riêng.

Không được đưa waiting revenue vào ROAS chính thức.

Có thể hiển thị waiting revenue trong dashboard nếu ghi rõ:

REVENUE_waiting_VERIFICATION — NOT_INCLUDED_IN_OFFICIAL_ROAS

## 28. ATTRIBUTION MODEL GOVERNANCE

## 28.1. Mỗi báo cáo phải có attribution model

PACK-07 không được hiển thị doanh thu theo nguồn nếu không khai báo attribution model.

Attribution model phải trả lời:

- Đơn này gán về nguồn nào.

- Vì sao gán về nguồn đó.

- Touchpoint nào được tính.

- Touchpoint nào bị loại.

- Có conflict không.

- Rule nào đã dùng.

- Evidence nào chứng minh.

- Revenue basis là gì.

- Data quality có pass không.

- Owner approval có cần không.

## 28.2. Các mô hình attribution được phép định nghĩa

PACK-07 có thể hỗ trợ các mô hình sau, tùy owner phê duyệt:

- First Touch Attribution.

- Last Touch Attribution.

- Last Paid Touch Attribution.

- Last Non-Direct Touch Attribution.

- Session-Based Attribution.

- Campaign-Bound Attribution.

- Live-Session Attribution.

- Messenger-Thread Attribution.

- CRM-Rebuy Attribution.

- Diamond Referral Attribution.

- Multi-Touch Attribution.

- Owner-Approved Custom Attribution.

Không được tự chọn model khác khi chưa có owner approval.

Không được dùng một model cho dashboard này và model khác cho dashboard kia mà không ghi rõ.

## 28.3. First Touch Attribution

First Touch gán conversion về touchpoint đầu tiên đủ điều kiện.

First Touch phù hợp để đo nguồn mở đầu hành trình khách.

First Touch không mặc định phù hợp để quyết định scale ngắn hạn nếu khách có nhiều tương tác sau đó.

First Touch phải có entry event rõ.

Nếu entry event thiếu hoặc conflict, First Touch không được dùng chính thức.

## 28.4. Last Touch Attribution

Last Touch gán conversion về touchpoint cuối cùng trước conversion.

Last Touch phù hợp để đo chặng chốt.

Last Touch có thể làm sai lệch vai trò của Ads đầu phễu hoặc CRM trước đó.

Nếu khách có Diamond/referral bind hoặc CRM event gần conversion, phải xử lý conflict theo policy.

## 28.5. Last Paid Touch Attribution

Last Paid Touch gán conversion về paid touchpoint gần nhất đủ điều kiện.

Mô hình này phù hợp để đo quảng cáo trả phí.

Tuy nhiên, Last Paid Touch không được dùng nếu:

- Paid event thiếu campaign/ad identity.

- Paid event không qua approved source.

- Order/revenue không có linkage.

- Conflict với Diamond/referral chưa xử lý.

- Data quality fail.

## 28.6. Live-Session Attribution

Live-Session Attribution gán conversion về phiên live khi có live context hợp lệ.

Điều kiện tối thiểu:

- Live session identity.

- Page identity.

- Live comment hoặc live entry event.

- Comment-to-Messenger hoặc live-to-private trace.

- Quote/order/revenue linkage.

- Data quality pass.

- Attribution rule pass.

Không có live session identity thì không được gán doanh thu cho live.

Không có private trace thì không được gán Messenger order về live comment.

## 28.7. Diamond Referral Attribution

Diamond Referral Attribution chỉ hợp lệ khi có referral bind/event hợp lệ từ owner.

Không có bind thì không có Diamond attribution.

Không có owner eligibility thì không có commission measurement.

PACK-07 không tự tính hoa hồng.

PACK-07 chỉ đo nguồn referral và trạng thái attribution nếu event hợp lệ.

## 28.8. CRM-Rebuy Attribution

CRM-Rebuy Attribution dùng cho doanh thu mua lại/chăm sóc lại.

Một khách từng đến từ Ads nhưng mua lại sau CRM không mặc định là Ads revenue.

CRM rebuy phải có CRM_EVENT hợp lệ và rule attribution rõ.

Nếu khách vừa nhận CRM vừa click Ads remarketing, phải đánh dấu conflict và xử lý theo owner-approved rule.

## 28.9. Multi-Touch Attribution

Multi-Touch Attribution dùng khi owner muốn phân bổ công cho nhiều touchpoint.

Multi-Touch phải có rule rõ về trọng số hoặc cách ghi nhận.

Không được tự chia phần trăm doanh thu nếu chưa có owner approval.

Nếu chưa có weight rule, chỉ được hiển thị touchpoint chain, không được phân bổ revenue chính thức.

## 29. ATTRIBUTION CONFLICT RESOLUTION

## 29.1. Nguyên tắc conflict

Khi một conversion có nhiều nguồn có thể nhận công, PACK-07 phải tạo trạng thái conflict.

Conflict không phải lỗi.

Conflict là tín hiệu cần rule xử lý.

Nếu chưa có rule xử lý, conversion không được đưa vào ROAS chính thức theo nguồn.

Trạng thái mặc định:

## ATTRIBUTION_CONFLICT_UNRESOLVED — EXCLUDED_FROM_SOURCE_ROAS

## 29.2. Các nhóm conflict bắt buộc nhận diện

PACK-07 phải nhận diện tối thiểu:

- Ads click trước, Diamond link sau.

- Diamond link trước, Ads remarketing sau.

- Paid live boost + organic live comment.

- Organic comment trong post đang chạy Ads.

- CRM message trước, Ads click sau.

- Ads click trước, CRM rebuy sau.

- Messenger direct không có ad context nhưng cùng khách từng click ads.

- Landing page visit trước, Messenger order sau.

- Human telesales can thiệp sau AI tư vấn.

- AI Advisor tạo quote, nhân viên chốt ngoài hệ thống.

- Khách chuyển page khác trong cùng hệ thống.

- Khách dùng nhiều số điện thoại/tài khoản.

- Khách mua lại cho người nhận khác.

- Khách vào từ MC AI Live và Facebook Ads cùng lúc.

- Khách có nhiều campaign touchpoints trong thời gian ngắn.

## 29.3. Conflict status

Các trạng thái conflict tối thiểu:

- NO_CONFLICT.

- CONFLICT_DETECTED.

- RULE_REQUIRED.

- OWNER_REVIEW_REQUIRED.

- RULE_APPLIED.

- CONFLICT_RESOLVED.

- EXCLUDED_DUE_TO_UNRESOLVED_CONFLICT.

- SPLIT_ATTRIBUTION_waiting.

- MULTI_TOUCH_ALLOWED.

- ATTRIBUTION_LOCKED.

Nếu trạng thái chưa đạt CONFLICT_RESOLVED hoặc ATTRIBUTION_LOCKED, không được đưa vào báo cáo ROAS chính thức theo nguồn.

## 30. PAID / ORGANIC / CRM / DIAMOND / LIVE CLASSIFICATION

## 30.1. Paid revenue

Paid revenue theo Ads chỉ được ghi nhận khi có:

- Paid source hợp lệ.

- Campaign/adset/ad identity.

- Customer/session linkage.

- Channel trace.

- Quote/order/revenue linkage.

- Attribution rule pass.

- Verified revenue.

- Data quality pass.

Thiếu một trong các điều kiện P0 thì không được gọi là Ads revenue chính thức.

## 30.2. Organic revenue

Organic revenue là doanh thu từ nguồn không trả phí hoặc không có paid attribution hợp lệ.

Không được gán Organic mặc định chỉ vì thiếu ad_id.

Nếu thiếu dữ liệu campaign, trạng thái phải là:

## SOURCE_UNKNOWN — DATA_QUALITY_WARNING

Không được biến missing data thành Organic revenue.

## 30.3. CRM revenue

CRM revenue phải có CRM_EVENT hợp lệ.

CRM revenue phải tách khỏi Ads revenue nếu attribution policy không cho phép cộng dồn.

CRM revenue không được tính vào Ads ROAS nếu conversion đến từ chăm sóc lại và không có paid attribution hợp lệ.

## 30.4. Diamond / Referral revenue

Diamond/referral revenue phải có referral event/bind hợp lệ.

Không có referral bind thì không được gọi là Diamond revenue.

Diamond revenue không tự động là Ads revenue.

Nếu Ads và Diamond cùng xuất hiện, phải dùng conflict resolution.

## 30.5. Live revenue

Live revenue phải có live session context.

Live revenue có thể là:

- Organic live revenue.

- Paid live boost revenue.

- Live-to-Messenger revenue.

- Live-to-Quote revenue.

- Live-to-Order revenue.

- Live verified revenue.

Nếu live được chạy Ads/boost, attribution phải phân biệt Paid Live và Organic Live.

## 31. METRIC DEFINITIONS — FUNNEL

## 31.1. Impression, Reach, Click

Impression, Reach và Click chỉ phản ánh hoạt động quảng cáo đầu phễu.

Các chỉ số này không đủ để kết luận doanh thu.

Chỉ số liên quan:

- CPM.

- CTR.

- CPC.

- Click-to-comment rate.

- Click-to-inbox rate.

- Click-to-qualified-conversation rate.

## 31.2. Comment metrics

Comment metrics phải phân biệt:

- Total comments.

- Unique commenters.

- Qualified comments.

- Buying intent comments.

- Price inquiry comments.

- Product question comments.

- Spam comments.

- Complaint comments.

- Hidden/deleted/suppressed comments nếu có.

- Comment-to-Messenger handoff rate.

Không được dùng total comments để kết luận lead quality nếu chưa lọc spam/duplicate/irrelevant.

## 31.3. Messenger metrics

Messenger metrics phải phân biệt:

- New conversations.

- Existing customer conversations.

- Qualified conversations.

- Product consult conversations.

- Quote request conversations.

- Complaint conversations.

- Human takeover conversations.

- AI bị chặn/guarded conversations.

- Conversation-to-quote rate.

- Conversation-to-order rate.

- Conversation-to-verified-revenue rate.

Messenger volume cao không đồng nghĩa doanh thu cao.

## 31.4. Quote metrics

Quote metrics tối thiểu:

- Quote requests.

- Quotes generated.

- Quotes failed.

- Quotes bị chặn by sellable/stock/sale lock.

- Quotes expired.

- Quotes accepted.

- Quote-to-cart rate.

- Quote-to-order rate.

- Quote-to-verified-revenue rate.

- Average quote value.

Quote value không phải revenue.

## 31.5. Order metrics

Order metrics tối thiểu:

- Official orders created.

- Orders confirmed.

- Orders cancelled.

- Orders payment waiting.

- Orders payment confirmed.

- Orders COD waiting.

- Orders COD success.

- Orders delivery waiting.

- Orders delivery success.

- Orders returned.

- Orders refunded.

- Orders verified.

- Orders excluded from ROAS.

Official order count không phải verified order count.

## 31.6. Revenue metrics

Revenue metrics tối thiểu:

- Gross order value.

- Confirmed order value.

- Paid revenue.

- COD successful revenue.

- Delivered revenue.

- Verified revenue.

- Net verified revenue.

- waiting revenue.

- Excluded revenue.

- Refunded value.

- Returned value.

- Revenue by source.

- Revenue by campaign.

- Revenue by live session.

- Revenue by product.

- Revenue by customer class nếu policy cho phép.

Revenue by source chỉ được hiển thị khi attribution pass.

## 32. METRIC DEFINITIONS — ADS PERFORMANCE

## 32.1. CPA

CPA phải khai báo rõ mẫu số đang dùng.

Các biến thể CPA:

- Cost per comment.

- Cost per qualified comment.

- Cost per Messenger conversation.

- Cost per qualified conversation.

- Cost per quote request.

- Cost per quote generated.

- Cost per official order.

- Cost per verified order.

- Cost per verified customer.

- Cost per verified revenue event.

Không được gọi chung “CPA” nếu không nói rõ conversion basis.

CPA dùng cho scale nên ưu tiên dựa trên official order hoặc verified revenue theo owner policy.

## 32.2. ROAS

ROAS phải khai báo rõ revenue basis.

Các biến thể ROAS:

- ROAS by gross order value.

- ROAS by confirmed order value.

- ROAS by paid revenue.

- ROAS by COD successful revenue.

- ROAS by delivered revenue.

- ROAS by verified revenue.

- ROAS by net verified revenue.

- ROAS by accounting reconciled revenue nếu có.

ROAS production mặc định không được dùng gross/confirmed/waiting revenue nếu owner chưa phê duyệt.

ROAS chính thức phải dựa trên verified revenue hoặc revenue basis được owner policy khóa.

## 32.3. AOV

AOV phải khai báo rõ order basis.

Các biến thể AOV:

- AOV by confirmed order.

- AOV by paid order.

- AOV by COD successful order.

- AOV by delivered order.

- AOV by verified order.

- AOV by net verified revenue.

Không được dùng AOV từ cart draft làm AOV chính thức.

Không được dùng AOV từ quote làm AOV doanh thu.

## 33. DATA QUALITY EVENT RULES

## 33.1. DATA_QUALITY_EVENT bắt buộc trước reporting

Mỗi batch đo lường hoặc dashboard snapshot phải có DATA_QUALITY_EVENT.

DATA_QUALITY_EVENT phải cho biết:

- Dữ liệu có đủ nguồn không.

- Event có duplicate không.

- Attribution có conflict không.

- Revenue đã verified chưa.

- Exclusion đã áp dụng chưa.

- Spend có hợp lệ không.

- Campaign mapping có đủ không.

- Channel trace có đủ không.

- Quote/order linkage có đủ không.

- Missing data có tồn tại không.

- Privacy/spam/suppression có ảnh hưởng không.

- Dữ liệu có đủ điều kiện dùng cho scale không.

## 33.2. Data Quality outcome

Các outcome tối thiểu:

- DQ_PASS.

- DQ_PASS_WITH_WARNING.

- DQ_waiting.

- DQ_FAIL.

- DQ_OWNER_REVIEW_REQUIRED.

- DQ_EXCLUDED_FROM_ROAS.

- DQ_SCALE_BLOCKED.

- DQ_REPORTING_ONLY.

- DQ_NOT_FOR_PRODUCTION_DECISION.

Chỉ DQ_PASS hoặc trạng thái được owner policy cho phép mới được dùng cho scale decision.

## 34. MEASUREMENT WINDOW GOVERNANCE

## 34.1. Time window phải rõ

Mọi báo cáo PACK-07 phải khai báo time window.

Time window tối thiểu:

- Start time.

- End time.

- Timezone.

- Campaign window.

- Conversion window.

- Revenue verification window.

- Attribution lookback window.

- Data refresh timestamp.

- Late event handling.

- Owner cutoff rule nếu có.

Không được so ROAS hai dashboard có time window khác nhau mà không cảnh báo.

## 34.2. Late event handling

Một số event có thể đến muộn:

- Payment confirmation.

- COD success.

- Delivery success.

- Refund.

- Return.

- Complaint.

- Reconcile.

- Verified revenue.

- MISA/accounting checkpoint nếu dùng.

PACK-07 phải cho phép trạng thái metric thay đổi từ waiting sang verified hoặc excluded khi late event đến.

Không được khóa ROAS quá sớm nếu revenue verification window chưa kết thúc.

## 35. CUSTOMER / SESSION / ORDER LINKAGE GOVERNANCE

## 35.1. Linkage là điều kiện sống còn của attribution

PACK-07 chỉ đo chính xác khi nối được:

- Ads touchpoint.

- Channel event.

- Messenger thread.

- AI consult.

- Quote.

- Cart.

- Official order.

- Payment/COD.

- Delivery.

- Verified revenue.

- Customer identity hoặc session identity.

- Attribution rule.

Nếu chuỗi bị đứt, metric phải chuyển cảnh báo.

## 35.2. Không nối được khách thì không gán nguồn chính thức

Nếu không nối được customer/session/order, PACK-07 không được gán doanh thu về nguồn cụ thể.

Trạng thái phải là:

## UNLINKED_REVENUE — ATTRIBUTION_NOT_ALLOWED

hoặc

## UNLINKED_EVENT — FUNNEL_INCOMPLETE

Không được tự nối bằng tên giống nhau, số điện thoại mơ hồ hoặc suy luận thủ công nếu owner policy chưa cho phép.

## 36. PRODUCT / SKU MEASUREMENT GOVERNANCE

## 36.1. Đo theo sản phẩm phải dùng product identity hợp lệ

PACK-07 có thể đo performance theo sản phẩm/SKU nếu nhận được product identity hợp lệ từ owner pack.

PACK-07 không được dùng mã SKU nội bộ trong customer-facing dashboard nếu policy không cho phép.

PACK-07 không được public BOM/công thức/tỷ lệ nội bộ.

Product performance chỉ được đo theo thông tin được phép:

- Product public name.

- Product group.

- Campaign product tag.

- Hero product flag nếu owner cung cấp.

- Sellable status từ owner.

- Stock/sale lock status từ owner.

- Verified revenue by product nếu order/revenue owner cung cấp.

Không được đo product revenue từ comment “mua dòng A” nếu thiếu order line verified.

## 36.2. Hero product bị chặn thì attribution vẫn giữ, scale phải block

Nếu sản phẩm hero hết hàng/not sellable/sale lock, PACK-07 vẫn có thể ghi nhận Ads/AI interest.

Tuy nhiên, Scale Gate phải block nếu sản phẩm không đủ điều kiện bán.

Dashboard phải tách:

- Demand signal.

- Sellable capacity.

- Stock constraint.

- Sale lock constraint.

- Fulfillment constraint.

- Revenue conversion.

Không được đánh giá Ads kém nếu demand tốt nhưng Commerce chặn bán vì stock/sale lock.

## 37. COMPLAINT / QUALITY / RISK EVENT GOVERNANCE

## 37.1. Complaint event phải tách khỏi sales conversion

Complaint conversation không được tính là lead bán hàng.

Complaint event phải đi vào risk metric.

Nếu complaint liên quan tới sản phẩm/campaign/live/AI reply, PACK-07 phải đánh dấu ảnh hưởng.

Các loại complaint tối thiểu:

- Product quality complaint.

- Taste/texture complaint.

- Delivery complaint.

- Payment complaint.

- Price/program complaint.

- Claim/safety complaint.

- Privacy complaint.

- Wrong product complaint.

- Late delivery complaint.

- Refund/return complaint.

Complaint tăng bất thường có thể kích hoạt Stop/Pause Gate ở PHẦN 3/4.

## 37.2. Claim/privacy incident phải chặn scale

Nếu có claim incident hoặc privacy incident liên quan ads/live/comment/Messenger, PACK-07 không được cho Scale PASS nếu chưa được xử lý.

Trạng thái:

## RISK_INCIDENT_OPEN — SCALE_BLOCKED

## 38. EVENT EVIDENCE REQUIREMENTS

## 38.1. Evidence tối thiểu cho event

Mỗi event dùng cho reporting chính thức phải có evidence tối thiểu:

- Event source.

- Event owner.

- Event timestamp.

- Event identity.

- Correlation identity.

- Data quality result.

- Attribution result nếu dùng.

- Revenue basis nếu dùng.

- Exclusion status nếu có.

- Review status nếu cần.

Không có evidence thì event chỉ được xem là raw/waiting, không được dùng cho PASS.

## 38.2. Evidence cho Verified Revenue

Verified Revenue event phải có evidence:

- Official order identity.

- Customer confirmation nếu policy yêu cầu.

- Payment/COD/delivery status theo policy.

- Refund/cancel/return status.

- Revenue amount.

- Revenue class.

- Attribution status.

- Data quality status.

- Owner confirmation.

- Timestamp.

- Audit/correlation.

Không có đủ evidence thì revenue phải ở trạng thái waiting.

## 39. DONE CONDITION CỦA PHẦN 2/4

## PHẦN 2/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Event taxonomy principles.

- Event family registry.

- Event lifecycle.

- Event exclusion rules.

- Funnel measurement principles.

- Comment -> Messenger -> Quote -> Cart -> Order -> Payment/COD -> Verified Revenue chain.

- Revenue classification.

- Revenue exclusion.

- Attribution model governance.

- Attribution conflict resolution.

- Paid/Organic/CRM/Diamond/Live classification.

- Funnel metric definitions.

- Ads metric definitions.

- Data Quality Event rules.

- Measurement window governance.

- Customer/session/order linkage governance.

- Product/SKU measurement governance.

- Complaint/quality/risk event governance.

- Event evidence requirements.

## PHẦN 2/4 chưa định nghĩa chi tiết Dashboard Governance, KPI presentation, ROAS/CPA/AOV decision display, Scale Gate, Stop Gate, Owner Decision Matrix, Smoke Matrix, Done Gate, Fail Gate hoặc Release Control. Các nội dung đó thuộc PHẦN 3/4 và PHẦN 4/4.

## KẾT LUẬN PHẦN 2/4

PACK-07 chỉ được đo lường bằng event hợp lệ.

Event phải có owner, source, correlation, data quality và evidence.

Comment không phải đơn hàng.

Messenger không phải doanh thu.

Quote không phải revenue.

Cart draft không phải official order.

Official order chưa chắc là paid revenue.

Payment waiting chưa chắc là verified revenue.

Verified Revenue là điều kiện trọng yếu để tính ROAS chính thức.

Attribution phải có rule.

Conflict phải được đánh dấu.

Missing data không được suy diễn.

waiting revenue không được đưa vào ROAS production.

Data Quality Event là lớp bắt buộc trước reporting, dashboard và scale decision.

Nguyên tắc khóa của PHẦN 2/4 là:

Đo đúng từng chặng funnel.

Không nhảy tầng dữ liệu.

Không tính trùng nguồn doanh thu.

Không biến tín hiệu tương tác thành doanh thu.

Không có Verified Revenue thì không có ROAS chính thức.

Không có Attribution Rule thì không có kết luận nguồn.

## PHẦN 3/4 — DASHBOARD / KPI / ROAS / CPA / AOV / SCALE GATE / STOP GATE / OWNER DECISION

## 40. MỤC TIÊU CỦA PHẦN 3/4

## PHẦN 3/4 thiết lập lớp quản trị dashboard, chỉ số đo lường, ROAS, CPA, AOV, Scale Gate, Stop/Pause Gate và Owner Decision cho PACK-07.

Mục tiêu của phần này là khóa rõ:

- Dashboard được phép hiển thị gì.

- Dashboard không được phép kết luận gì khi dữ liệu chưa đủ.

- KPI phải được định nghĩa theo đúng revenue basis và conversion basis.

- ROAS chỉ được dùng khi data quality và verified revenue đạt điều kiện.

- CPA phải khai báo rõ conversion basis.

- AOV phải khai báo rõ order/revenue basis.

- Scale Gate phải kiểm tra đủ Ads, AI, Commerce, stock, fulfillment, revenue và data quality.

- Stop/Pause Gate phải chặn kịp thời khi rủi ro tăng.

- Owner Decision là điều kiện bắt buộc trước khi scale production.

- Dashboard không được che giấu data issue, attribution conflict, revenue waiting hoặc sale lock.

## PHẦN 3/4 không thiết kế UI chi tiết, không thiết kế API, không thiết kế database và không viết code.

## 41. DASHBOARD GOVERNANCE PRINCIPLES

## 41.1. Dashboard là lớp quan sát, không phải source-of-truth

Dashboard PACK-07 chỉ là lớp hiển thị và phân tích dữ liệu đo lường.

Dashboard không phải source-of-truth cho:

- Đơn hàng.

- Doanh thu.

- Thanh toán.

- COD.

- Giao hàng.

- Tồn kho.

- Sellable status.

- Sale Lock.

- Giá.

- Khuyến mãi.

- Member benefit.

- Diamond benefit.

- Commission.

- MISA accounting.

- Complaint resolution.

Dashboard chỉ được hiển thị dữ liệu đã lấy từ owner source hợp lệ hoặc projection đo lường có kiểm soát.

Dashboard không được ghi ngược dữ liệu nghiệp vụ gốc.

Dashboard không được tự sửa doanh thu.

Dashboard không được tự sửa attribution.

Dashboard không được tự xác nhận ROAS PASS.

Dashboard không được tự quyết định scale nếu thiếu owner rule/approval.

## 41.2. Dashboard phải phân biệt các lớp dữ liệu

Dashboard PACK-07 phải luôn phân biệt:

- Raw data.

- Normalized data.

- Clean data.

- Verified data.

- Excluded data.

- waiting data.

- Missing data.

- Duplicate data.

- Conflict data.

- Suppressed data.

- Privacy-bị chặn data.

- Owner-review data.

Không được gom toàn bộ dữ liệu thành một số tổng.

Không được hiển thị “doanh thu” mà không nói rõ revenue class.

Không được hiển thị “ROAS” mà không nói rõ revenue basis.

Không được hiển thị “CPA” mà không nói rõ conversion basis.

Không được hiển thị “AOV” mà không nói rõ order/revenue basis.

## 41.3. Dashboard phải có cảnh báo độ tin cậy

Mỗi dashboard snapshot phải có trạng thái độ tin cậy.

Các trạng thái tối thiểu:

- DASHBOARD_READY_FOR_VIEW.

- DASHBOARD_READY_FOR_DECISION.

- DASHBOARD_WARNING.

- DASHBOARD_DATA_waiting.

- DASHBOARD_DATA_QUALITY_FAIL.

- DASHBOARD_ATTRIBUTION_CONFLICT.

- DASHBOARD_REVENUE_NOT_VERIFIED.

- DASHBOARD_SCALE_BLOCKED.

- DASHBOARD_OWNER_REVIEW_REQUIRED.

- DASHBOARD_NOT_FOR_PRODUCTION_DECISION.

Nếu dashboard chưa đạt READY_FOR_DECISION, không được dùng để scale production.

Nếu dashboard có data quality fail, không được hiển thị trạng thái tốt/màu xanh cho ROAS chính thức.

Nếu dashboard có attribution conflict chưa xử lý, doanh thu theo nguồn phải chuyển sang owner review.

## 42. DASHBOARD DATA LAYERS

## 42.1. Raw Dashboard

Raw Dashboard dùng để quan sát dữ liệu gốc.

Raw Dashboard có thể hiển thị:

- Raw spend.

- Raw impression.

- Raw reach.

- Raw click.

- Raw comment.

- Raw inbox.

- Raw event count.

- Raw order count nếu nguồn gửi.

- Raw revenue nếu nguồn gửi.

Raw Dashboard không được dùng để quyết định scale.

Raw Dashboard không được gọi là performance chính thức.

Raw Dashboard phải có cảnh báo:

## RAW_DATA — NOT_FOR_SCALE_DECISION

## 42.2. Clean Dashboard

Clean Dashboard chỉ hiển thị dữ liệu đã qua:

- Source validation.

- Normalization.

- Deduplication.

- Spam/bot suppression.

- Privacy guard.

- Required field check.

- Event correlation check.

- Data quality check.

Clean Dashboard có thể dùng để phân tích funnel.

Clean Dashboard chưa chắc được dùng để tính ROAS nếu revenue chưa verified.

## 42.3. Verified Dashboard

Verified Dashboard chỉ hiển thị dữ liệu đã đạt:

- Clean data.

- Attribution rule pass.

- Quote/order linkage pass.

- Official order validation nếu dùng order metric.

- Payment/COD/delivery validation nếu dùng revenue metric.

- Verified revenue pass.

- Revenue exclusion applied.

- Owner policy pass.

- Evidence available.

Verified Dashboard là dashboard chính để xét ROAS production, Scale Gate và Stop Gate.

## 42.4. Excluded Dashboard

Excluded Dashboard phải hiển thị dữ liệu bị loại khỏi ROAS hoặc decision.

Các nhóm bị loại:

- Duplicate.

- Spam.

- Bot suspected.

- Privacy bị chặn.

- Channel error.

- Missing campaign context.

- Missing channel trace.

- Missing quote/order linkage.

- Payment not confirmed.

- COD failed.

- Delivery failed.

- Cancelled order.

- Refunded order.

- Returned order.

- Test order.

- Internal order nếu policy loại.

- Attribution conflict unresolved.

- Revenue not verified.

- Sale Lock impacted.

- Complaint unresolved.

- Owner policy excluded.

Dashboard không được ẩn dữ liệu bị loại, vì dữ liệu bị loại giúp owner hiểu rủi ro và nguyên nhân sai lệch.

## 42.5. waiting Dashboard

waiting Dashboard dùng cho dữ liệu đang chờ xác minh.

Các nhóm waiting:

- Payment waiting.

- COD waiting.

- Delivery waiting.

- Revenue verification waiting.

- Attribution rule waiting.

- Owner review waiting.

- Refund/return window waiting.

- MISA/accounting reconcile waiting nếu có dùng.

- Data refresh waiting.

- Late event waiting.

waiting revenue không được tính vào ROAS chính thức.

waiting data có thể hiển thị để quan sát pipeline nhưng phải có cảnh báo rõ.

## 43. DASHBOARD STRUCTURE GOVERNANCE

## 43.1. Executive Measurement Dashboard

Executive Measurement Dashboard dành cho owner xem nhanh tình hình tổng thể.

Các nhóm hiển thị tối thiểu:

- Ads spend.

- Verified revenue.

- Net verified revenue nếu policy dùng.

- Official ROAS status.

- CPA by verified order.

- AOV by verified order.

- Qualified conversation rate.

- Quote-to-order rate.

- Order-to-verified-revenue rate.

- Data quality status.

- Attribution conflict status.

- Scale Gate status.

- Stop/Pause Gate status.

- Owner decision waiting.

- Top risk alerts.

Executive Dashboard không được hiển thị ROAS chính thức nếu revenue basis chưa verified.

## 43.2. Ads Performance Dashboard

Ads Performance Dashboard đo hiệu quả quảng cáo.

Các nhóm chỉ số tối thiểu:

- Spend.

- Impression.

- Reach.

- CPM.

- Click.

- CTR.

- CPC.

- Landing view nếu có.

- Comment.

- Qualified comment.

- Inbox/Messenger conversation.

- Qualified conversation.

- Quote request.

- Official order.

- Verified revenue.

- ROAS by verified revenue.

- CPA by selected conversion basis.

- Campaign/adset/ad creative performance.

- Data quality warning.

- Attribution warning.

Ads Dashboard không được kết luận doanh thu nếu conversion chưa đi đến verified revenue.

## 43.3. Funnel Dashboard

Funnel Dashboard đo từng chặng chuyển đổi.

Funnel chuẩn:

Impression -> Reach -> Click -> Landing View -> Comment -> Qualified Comment -> Private Handoff -> Messenger Conversation -> Qualified Conversation -> Product Consult -> Quote Request -> Quote Generated -> Cart Draft -> Customer Confirmation -> Official Order -> Payment/COD Confirmation -> Delivery Success nếu policy yêu cầu -> Order Verified -> Verified Revenue

Funnel Dashboard phải hiển thị drop-off ở từng chặng.

Nếu chặng nào thiếu dữ liệu, dashboard phải đánh dấu missing.

Không được nhảy từ comment sang order.

Không được nhảy từ quote sang revenue.

Không được gộp cart draft với official order.

## 43.4. Attribution Dashboard

Attribution Dashboard hiển thị nguồn phân bổ doanh thu.

Các lớp tối thiểu:

- Paid Ads.

- Organic.

- Live.

- Paid Live/Boosted Live nếu có.

- Messenger Direct.

- CRM Rebuy.

- Diamond Referral.

- Member Rebuy.

- Landing Page.

- Mixed Source.

- Unknown Source.

- Conflict Source.

- Owner Review Required.

Attribution Dashboard chỉ được hiển thị source revenue chính thức khi attribution rule pass.

Nếu conflict chưa xử lý, revenue phải nằm ở nhóm conflict/waiting.

## 43.5. Revenue Dashboard

Revenue Dashboard phải phân biệt:

- Gross order value.

- Confirmed order value.

- Payment waiting value.

- Paid revenue.

- COD waiting value.

- COD successful revenue.

- Delivered revenue.

- Verified revenue.

- Net verified revenue.

- waiting revenue.

- Excluded revenue.

- Refund/return adjustment.

- Accounting reconciled revenue nếu có.

Revenue Dashboard phải hiển thị revenue basis rõ ràng.

Không được gọi chung là “doanh thu” nếu chưa nêu class.

## 43.6. Data Quality Dashboard

Data Quality Dashboard là dashboard bắt buộc.

Các nhóm chỉ số tối thiểu:

- Total events received.

- Valid source events.

- Invalid source events.

- Duplicate events.

- Missing required fields.

- Missing campaign mapping.

- Missing channel trace.

- Missing quote/order linkage.

- Missing verified revenue.

- Attribution conflicts.

- Spam/bot suppressed.

- Privacy-bị chặn events.

- Excluded events.

- waiting owner review.

- Data quality pass rate.

- Data quality fail rate.

- Scale eligible data ratio.

- Dashboard reliability status.

Không có Data Quality Dashboard thì PACK-07 không đủ điều kiện scale governance.

## 43.7. Risk Dashboard

Risk Dashboard hiển thị các vấn đề ảnh hưởng scale/stop.

Các nhóm rủi ro tối thiểu:

- Complaint increase.

- Claim/safety incident.

- Privacy incident.

- Spam/comment abuse increase.

- Payment fail increase.

- COD fail increase.

- Delivery fail increase.

- Refund/return increase.

- Sale Lock impacted SKU.

- Stockout/not sellable.

- Fulfillment overload.

- AI handoff overload.

- Human takeover overload.

- Attribution conflict spike.

- Data mismatch.

- Dashboard data quality fail.

- Owner decision waiting.

Risk Dashboard là đầu vào bắt buộc cho Stop/Pause Gate.

## 44. KPI GOVERNANCE PRINCIPLES

## 44.1. KPI phải có định nghĩa chính thức

Mỗi KPI trong PACK-07 phải có:

- Tên KPI.

- Mục đích sử dụng.

- Công thức logic.

- Tử số.

- Mẫu số.

- Data source.

- Owner source.

- Time window.

- Revenue basis nếu có.

- Conversion basis nếu có.

- Data quality requirement.

- Exclusion rule.

- Attribution model nếu có.

- Decision usage.

- Warning condition.

Không được dùng KPI không có định nghĩa.

Không được dùng cùng một tên KPI cho nhiều cách tính khác nhau mà không ghi rõ basis.

## 44.2. KPI phải phân biệt observation và decision

Observation KPI dùng để quan sát.

Decision KPI dùng để ra quyết định scale/stop.

Ví dụ:

- Total comments là observation KPI.

- Qualified conversation rate là decision-support KPI.

- Verified revenue ROAS là decision KPI.

- Gross order ROAS chỉ là observation nếu policy không cho phép dùng để scale.

- waiting revenue là observation KPI.

- Data quality pass rate là decision KPI.

- Attribution conflict rate là decision KPI.

- COD fail rate là decision KPI.

- Complaint rate là decision KPI.

Không được dùng observation KPI để scale nếu decision KPI chưa đạt.

## 44.3. KPI phải đi kèm time window

KPI không có time window là KPI không đủ điều kiện quyết định.

Mỗi KPI phải nêu rõ:

- Ngày bắt đầu.

- Ngày kết thúc.

- Timezone.

- Campaign window.

- Conversion window.

- Revenue verification window.

- Late event handling window.

- Data refresh timestamp.

Không được so sánh KPI khác time window mà không cảnh báo.

## 45. ROAS GOVERNANCE

## 45.1. ROAS phải khai báo revenue basis

ROAS không được hiển thị chung chung.

Các loại ROAS có thể tồn tại:

- ROAS by Gross Order Value.

- ROAS by Confirmed Order Value.

- ROAS by Paid Revenue.

- ROAS by COD Successful Revenue.

- ROAS by Delivered Revenue.

- ROAS by Verified Revenue.

- ROAS by Net Verified Revenue.

- ROAS by Accounting Reconciled Revenue nếu policy cho phép.

ROAS dùng cho scale production phải là ROAS theo revenue basis được owner phê duyệt.

Mặc định, ROAS chính thức của PACK-07 phải dựa trên Verified Revenue hoặc Net Verified Revenue nếu policy yêu cầu.

## 45.2. ROAS không được tính khi thiếu spend hợp lệ

Không có Ads spend source hợp lệ thì không tính ROAS.

Spend hợp lệ phải có:

- Source được owner phê duyệt.

- Campaign/adset/ad identity.

- Time window.

- Currency.

- Spend amount.

- Account/Page/Business context.

- Import/sync status.

- Evidence snapshot.

- Data quality status.

Spend nhập tay chưa reconcile không được dùng cho ROAS PASS nếu owner policy không cho phép.

## 45.3. ROAS không được tính khi thiếu attribution

ROAS theo nguồn chỉ được tính khi attribution pass.

Nếu revenue đã verified nhưng chưa biết thuộc nguồn nào, doanh thu đó không được đưa vào ROAS theo nguồn.

Trạng thái bắt buộc:

## VERIFIED_REVENUE_WITHOUT_ATTRIBUTION — SOURCE_ROAS_NOT_ALLOWED

Nếu có attribution conflict chưa xử lý:

## ATTRIBUTION_CONFLICT — SOURCE_ROAS_BLOCKED

## 45.4. ROAS không được tính khi revenue chưa verified

Các lớp không dùng cho ROAS chính thức nếu owner chưa phê duyệt:

- Quote value.

- Cart draft value.

- Gross order value.

- Confirmed order value.

- Payment waiting value.

- COD waiting value.

- Revenue waiting verification.

- Revenue with unresolved refund/return.

- Revenue with unresolved attribution conflict.

- Revenue with data quality fail.

Dashboard có thể hiển thị các lớp này dưới dạng pipeline, nhưng phải ghi rõ không phải ROAS chính thức.

## 45.5. ROAS phải có trạng thái tin cậy

Các trạng thái ROAS tối thiểu:

- ROAS_RAW.

- ROAS_PIPELINE_ONLY.

- ROAS_waiting_VERIFICATION.

- ROAS_DATA_QUALITY_WARNING.

- ROAS_ATTRIBUTION_CONFLICT.

- ROAS_VERIFIED.

- ROAS_NET_VERIFIED.

- ROAS_ACCOUNTING_RECONCILED nếu có.

- ROAS_NOT_RELIABLE.

- ROAS_SCALE_ELIGIBLE.

- ROAS_SCALE_BLOCKED.

Chỉ ROAS_VERIFIED, ROAS_NET_VERIFIED hoặc trạng thái owner-approved mới được đưa vào Scale Gate.

## 46. CPA GOVERNANCE

## 46.1. CPA phải khai báo conversion basis

CPA không được hiển thị chung chung.

Các loại CPA tối thiểu:

- Cost per comment.

- Cost per qualified comment.

- Cost per Messenger conversation.

- Cost per qualified conversation.

- Cost per quote request.

- Cost per quote generated.

- Cost per cart draft.

- Cost per customer confirmation.

- Cost per official order.

- Cost per payment confirmed order.

- Cost per COD successful order.

- Cost per delivered order.

- Cost per verified order.

- Cost per repeat purchase.

- Cost per verified revenue event.

CPA dùng cho scale phải dùng conversion basis được owner phê duyệt.

## 46.2. CPA tầng thấp không đủ để scale

CPA comment thấp không có nghĩa campaign tốt.

CPA inbox thấp không có nghĩa doanh thu tốt.

CPA quote thấp không có nghĩa order tốt.

CPA order thấp không có nghĩa revenue verified tốt.

Scale production phải ưu tiên CPA ở tầng:

- Official order.

- Payment/COD confirmed order.

- Verified order.

- Verified revenue.

Nếu chỉ có CPA comment/inbox tốt nhưng quote-to-order hoặc order-to-verified revenue thấp, Scale Gate phải cảnh báo.

## 46.3. CPA phải có data quality pass

CPA không được dùng cho decision nếu:

- Spend chưa hợp lệ.

- Event trùng chưa xử lý.

- Spam/bot chưa loại.

- Comment chưa qualified.

- Messenger thread thiếu trace.

- Quote/order linkage thiếu.

- Official order thiếu.

- Verified revenue thiếu nếu dùng CPA verified.

- Attribution conflict chưa xử lý.

- Time window không rõ.

## 47. AOV GOVERNANCE

## 47.1. AOV phải khai báo order/revenue basis

AOV có thể tính theo nhiều basis:

- AOV by cart draft.

- AOV by confirmed order.

- AOV by paid order.

- AOV by COD successful order.

- AOV by delivered order.

- AOV by verified order.

- AOV by net verified revenue.

AOV by cart draft không phải AOV doanh thu.

AOV by quote không phải AOV doanh thu.

AOV dùng cho scale phải dựa trên official order/verified revenue theo owner policy.

## 47.2. AOV phải tách theo nguồn nếu attribution pass

AOV theo campaign/adset/ad chỉ được hiển thị khi attribution pass.

Nếu attribution conflict chưa xử lý, AOV theo nguồn phải bị chặn.

AOV tổng có thể hiển thị nếu revenue verified, nhưng AOV theo nguồn cần attribution rõ.

## 47.3. AOV phải cảnh báo khi lệch do đơn bất thường

AOV có thể bị méo bởi:

- Đơn số lượng lớn bất thường.

- Đơn B2B.

- Đơn quà tặng.

- Đơn nội bộ.

- Đơn test.

- Đơn hoàn/hủy một phần.

- Đơn combo đặc biệt.

- Đơn từ Diamond/referral.

- Đơn từ CRM rebuy.

- Đơn từ live chương trình đặc biệt.

Dashboard phải có outlier warning nếu AOV bị ảnh hưởng bởi đơn bất thường.

## 48. CORE KPI REGISTRY

## 48.1. Nhóm KPI đầu phễu Ads

KPI | Ý nghĩa | Decision usage

Spend | Chi phí quảng cáo | Cần cho ROAS/CPA

Impression | Lượt hiển thị | Quan sát reach đầu phễu

Reach | Số người tiếp cận | Quan sát độ phủ

CPM | Chi phí 1.000 hiển thị | Tối ưu media

Click | Lượt click | Quan sát quan tâm ban đầu

CTR | Tỷ lệ click | Đánh giá creative/offer

CPC | Chi phí click | Tối ưu traffic

Các KPI này không đủ để scale nếu không có conversion và revenue.

## 48.2. Nhóm KPI comment/Messenger

KPI | Ý nghĩa | Decision usage

Total Comments | Tổng comment | Observation only

Qualified Comments | Comment đủ điều kiện | Decision-support

Spam Comment Rate | Tỷ lệ comment rác | Stop/Pause risk

Comment-to-Messenger Rate | Tỷ lệ kéo private | Đánh giá PACK-06 routing

Messenger Conversations | Số hội thoại | Observation

Qualified Conversations | Hội thoại đủ chất lượng | Decision-support

Conversation-to-Quote Rate | Chuyển tư vấn sang quote | Đánh giá AI/offer

Complaint Conversation Rate | Tỷ lệ khiếu nại | Stop/Pause risk

Không được dùng Total Comments làm cơ sở scale chính thức.

## 48.3. Nhóm KPI quote/order/revenue

KPI | Ý nghĩa | Decision usage

Quote Request Rate | Tỷ lệ yêu cầu báo giá | Đánh giá nhu cầu

Quote Generated Rate | Tỷ lệ tạo quote thành công | Đánh giá Commerce Runtime

Quote bị chặn Rate | Tỷ lệ quote bị chặn | Cảnh báo stock/sellable/sale lock

Quote-to-Order Rate | Tỷ lệ quote thành order | Decision-support

Official Order Count | Số đơn chính thức | Decision-support

Order-to-Payment Rate | Tỷ lệ đơn thanh toán | Decision KPI

COD Success Rate | Tỷ lệ COD thành công | Decision KPI

Order-to-Verified-Revenue Rate | Tỷ lệ đơn thành revenue verified | Decision KPI

Verified Revenue | Doanh thu đủ điều kiện | ROAS basis

Net Verified Revenue | Doanh thu verified sau điều chỉnh | ROAS basis nếu policy dùng

## 48.4. Nhóm KPI quality/risk

KPI | Ý nghĩa | Decision usage

Data Quality Pass Rate | Tỷ lệ dữ liệu đạt | Scale Gate

Attribution Conflict Rate | Tỷ lệ conflict nguồn | Scale/Review Gate

Missing Linkage Rate | Tỷ lệ đứt trace | Scale Block

Payment Fail Rate | Tỷ lệ payment fail | Stop/Pause

COD Fail Rate | Tỷ lệ COD fail | Stop/Pause

Refund/Return Rate | Tỷ lệ hoàn/đổi/trả | Stop/Pause

Complaint Rate | Tỷ lệ khiếu nại | Stop/Pause

Privacy Incident Count | Số sự cố privacy | Stop/Block

Claim Incident Count | Số sự cố claim | Stop/Block

Sale Lock Impact Count | Số SKU/order bị sale lock ảnh hưởng | Stop/Block

Fulfillment Overload Signal | Tín hiệu quá tải vận hành | Scale Block

## 49. SCALE GATE GOVERNANCE

## 49.1. Scale Gate là điều kiện bắt buộc trước khi tăng ngân sách

Không được scale quảng cáo chỉ vì nhiều comment.

Không được scale quảng cáo chỉ vì inbox tăng.

Không được scale quảng cáo chỉ vì live đông.

Không được scale quảng cáo chỉ vì ROAS raw đẹp.

Không được scale quảng cáo nếu revenue chưa verified.

Không được scale quảng cáo nếu data quality chưa đạt.

Không được scale quảng cáo nếu owner chưa phê duyệt hoặc rule automation chưa được khóa.

Scale Gate là chốt kiểm soát bắt buộc trước mọi quyết định tăng ngân sách, mở rộng campaign, mở rộng adset, mở thêm page, mở thêm live boost hoặc nhân bản creative ở quy mô lớn.

## 49.2. Điều kiện Scale Gate tối thiểu

Scale Gate chỉ được PASS khi các nhóm điều kiện sau đạt:

- Ads spend source hợp lệ.

- Campaign/adset/ad identity rõ.

- Channel event từ PACK-06 hợp lệ.

- AI event từ PACK-05 hợp lệ nếu đo AI-assisted conversion.

- Quote/order linkage hợp lệ.

- Official order hợp lệ.

- Verified revenue đạt.

- Refund/cancel/return đã xử lý.

- Attribution rule pass.

- Data quality pass.

- ROAS đạt ngưỡng owner policy.

- CPA đạt ngưỡng owner policy.

- AOV đạt ngưỡng owner policy nếu dùng.

- COD/payment fail trong giới hạn.

- Refund/return trong giới hạn.

- Complaint trong giới hạn.

- AI handoff không quá tải.

- CSKH không quá tải.

- Warehouse/fulfillment đáp ứng được.

- Stock/sellable đủ điều kiện.

- Sale Lock không ảnh hưởng sản phẩm hero.

- Privacy/claim incident không mở.

- Dashboard reliability đạt.

- Owner approval có hiệu lực.

Thiếu bất kỳ điều kiện P0 nào thì Scale Gate không được PASS.

## 49.3. Scale Gate status

Các trạng thái Scale Gate tối thiểu:

- SCALE_NOT_EVALUATED.

- SCALE_DATA_waiting.

- SCALE_DATA_QUALITY_FAIL.

- SCALE_ATTRIBUTION_BLOCKED.

- SCALE_REVENUE_NOT_VERIFIED.

- SCALE_STOCK_BLOCKED.

- SCALE_SALE_LOCK_BLOCKED.

- SCALE_FULFILLMENT_BLOCKED.

- SCALE_RISK_BLOCKED.

- SCALE_OWNER_REVIEW_REQUIRED.

- SCALE_ALLOWED_LIMITED.

- SCALE_ALLOWED.

- SCALE_APPROVED_BY_OWNER.

- SCALE_EXECUTED nếu owner policy cho phép ghi nhận execution.

PACK-07 không được tự chuyển sang SCALE_EXECUTED nếu thiếu owner decision hoặc execution evidence từ owner vận hành Ads.

## 49.4. Scale Allowed Limited

Scale Allowed Limited dùng khi dữ liệu đủ tốt nhưng còn giới hạn vận hành.

Ví dụ:

- ROAS đạt nhưng stock không đủ cho scale lớn.

- CPA đạt nhưng CSKH gần quá tải.

- Verified revenue đạt nhưng COD fail bắt đầu tăng.

- Campaign tốt nhưng chỉ tốt ở một page/live cụ thể.

- AOV cao nhưng do một số đơn lớn bất thường.

- Data quality pass nhưng sample size còn nhỏ.

- Fulfillment đáp ứng được mức tăng nhỏ, chưa đáp ứng tăng lớn.

Trạng thái này chỉ cho phép owner xét scale có giới hạn, không tự động mở rộng mạnh.

## 49.5. Scale bị chặn

Scale bị chặn bắt buộc khi:

- Data quality fail.

- ROAS chưa verified.

- Attribution conflict chưa xử lý.

- CPA vượt ngưỡng.

- COD/payment fail cao.

- Refund/return cao.

- Complaint tăng.

- Privacy/claim incident mở.

- Hero SKU hết hàng.

- Hero SKU not sellable.

- Sale Lock/Recall ảnh hưởng SKU.

- Fulfillment quá tải.

- AI/human handoff quá tải.

- Owner approval thiếu.

- Dashboard mismatch.

- Spend source không hợp lệ.

- Order/revenue linkage thiếu.

## 50. STOP / PAUSE GATE GOVERNANCE

## 50.1. Stop/Pause Gate bảo vệ hệ thống

Stop/Pause Gate dùng để dừng, pause, giảm ngân sách hoặc yêu cầu owner review khi chiến dịch có rủi ro.

Stop/Pause Gate không chỉ dựa vào ROAS.

Một chiến dịch có ROAS tốt vẫn có thể phải pause nếu:

- Sale Lock ảnh hưởng sản phẩm.

- Sản phẩm hết hàng.

- Khiếu nại tăng.

- Claim/privacy incident xuất hiện.

- COD fail cao.

- Fulfillment quá tải.

- AI/human handoff quá tải.

- Data quality fail.

- Attribution conflict quá cao.

- Revenue mismatch.

- Owner yêu cầu.

## 50.2. Điều kiện Stop/Pause Gate tối thiểu

Stop/Pause Gate phải được kích hoạt khi có một trong các nhóm sau:

- ROAS không đủ tin cậy.

- ROAS tụt dưới ngưỡng owner policy.

- CPA vượt ngưỡng owner policy.

- Conversion tụt bất thường.

- Comment spam tăng cao.

- Qualified conversation giảm mạnh.

- Quote-to-order tụt bất thường.

- Payment fail tăng cao.

- COD fail tăng cao.

- Delivery fail tăng cao.

- Refund/return tăng cao.

- Complaint tăng cao.

- Fulfillment quá tải.

- AI Advisor quá tải hoặc fail-closed tăng mạnh.

- Human takeover quá tải.

- Hero SKU hết hàng.

- Hero SKU not sellable.

- Sale Lock/Recall ảnh hưởng sản phẩm.

- Data mismatch.

- Attribution conflict tăng.

- Privacy incident.

- Claim/safety incident.

- App/page/webhook/channel incident.

- Owner yêu cầu.

## 50.3. Stop/Pause Gate status

Các trạng thái tối thiểu:

- STOP_NOT_EVALUATED.

- STOP_MONITORING.

- PAUSE_RECOMMENDED.

- PAUSE_REQUIRED.

- STOP_RECOMMENDED.

- STOP_REQUIRED.

- BUDGET_REDUCTION_RECOMMENDED.

- OWNER_REVIEW_REQUIRED.

- RISK_INCIDENT_OPEN.

- DATA_QUALITY_STOP.

- STOCK_STOP.

- SALE_LOCK_STOP.

- FULFILLMENT_STOP.

- PRIVACY_CLAIM_STOP.

- OWNER_STOPPED.

- RESUME_REVIEW_REQUIRED.

- RESUME_ALLOWED.

- RESUME_APPROVED_BY_OWNER.

PACK-07 không tự resume campaign nếu chưa có owner decision hoặc rule automation được phê duyệt.

## 50.4. Pause không xóa dữ liệu đo lường

Khi campaign bị pause/stop, PACK-07 vẫn phải giữ dữ liệu:

- Spend trước pause.

- Funnel trước pause.

- Revenue waiting.

- Late payment/COD/delivery event.

- Refund/return event.

- Complaint event.

- Attribution result.

- Data quality result.

- Owner decision.

- Resume review evidence.

Pause/stop không được làm mất trace.

## 51. OWNER DECISION GOVERNANCE

## 51.1. Owner Decision là điều kiện kiểm soát cuối

PACK-07 có thể đề xuất.

Owner quyết định.

Các quyết định bắt buộc cần owner hoặc delegated owner approval:

- Scale budget.

- Reduce budget.

- Pause campaign.

- Stop campaign.

- Resume campaign.

- Change attribution model.

- Change revenue basis.

- Accept manual spend.

- Accept manual revenue correction.

- Accept unresolved warning.

- Override scale block nếu policy cho phép.

- Approve experimental campaign.

- Approve creative with risk warning.

- Approve new source/channel measurement.

- Approve dashboard for production decision.

Không có owner decision thì không được gọi Scale Approved.

## 51.2. Owner Decision phải có evidence

Mỗi quyết định owner phải có:

- Decision type.

- Decision scope.

- Campaign/adset/ad/page/live scope.

- Time window.

- Dashboard snapshot.

- Data quality result.

- ROAS/CPA/AOV basis.

- Attribution status.

- Revenue verification status.

- Risk status.

- Stock/sellable status.

- Fulfillment status.

- Reason.

- Owner identity.

- Timestamp.

- Effective period.

- Review date nếu cần.

- Audit/correlation.

Không có evidence thì quyết định không được dùng cho PASS.

## 51.3. Owner override phải bị kiểm soát

Owner có thể override một số cảnh báo nếu policy cho phép, nhưng không được override các chặn P0 tuyệt đối.

Các chặn không được override nếu chưa có owner policy cấp cao:

- Privacy incident chưa xử lý.

- Claim/safety incident nghiêm trọng.

- Sale Lock/Recall active.

- Not sellable SKU.

- Revenue chưa verified nhưng vẫn gọi ROAS chính thức.

- Spend source không hợp lệ nhưng vẫn scale.

- Attribution conflict chưa xử lý nhưng vẫn công bố source ROAS chính thức.

- Payment/COD fail nghiêm trọng.

- Fulfillment không đáp ứng.

- Data quality fail nghiêm trọng.

Override phải để lại audit và evidence.

## 52. SCALE DECISION MATRIX

## 52.1. Matrix quyết định scale

Điều kiện | Trạng thái | Quyết định

ROAS verified đạt, CPA đạt, data quality pass, stock đủ, no risk | Healthy | Scale Allowed

ROAS verified đạt nhưng stock giới hạn | Stock constrained | Scale Allowed Limited

ROAS raw đẹp nhưng revenue chưa verified | waiting | Scale bị chặn

CPA comment tốt nhưng order thấp | Funnel weak | Scale bị chặn / Optimize

Quote cao nhưng order thấp | Offer/order issue | Owner Review

Order cao nhưng payment/COD fail cao | Payment/COD risk | Pause/Review

Revenue tốt nhưng complaint tăng | Risk rising | Pause/Review

Sale Lock ảnh hưởng SKU hero | bị chặn | Stop/Pause Required

Attribution conflict cao | Measurement unreliable | Scale bị chặn

Data quality fail | Unreliable | Scale bị chặn

Fulfillment quá tải | Operational constraint | Scale bị chặn

Privacy/claim incident | Critical risk | Stop Required

## 52.2. Matrix quyết định stop/pause

Tín hiệu | Mức độ | Quyết định

ROAS tụt nhẹ nhưng data quality pass | Medium | Review/Optimize

ROAS tụt mạnh | High | Pause Recommended

CPA vượt ngưỡng | High | Pause/Reduce Budget

COD fail tăng mạnh | High | Pause Required

Complaint tăng mạnh | High | Pause Required

Privacy incident | Critical | Stop Required

Claim/safety incident | Critical | Stop Required

Sale Lock/Recall | Critical | Stop Required

Stockout hero SKU | Critical | Stop/Pause Required

Fulfillment overload | High | Scale bị chặn / Pause

Data mismatch nghiêm trọng | High | Decision bị chặn

Attribution conflict chưa xử lý | Medium/High | Source ROAS bị chặn

Owner yêu cầu dừng | Critical | Stop Required

## 53. DATA RELIABILITY WARNING

## 53.1. Dashboard phải cảnh báo khi dữ liệu không đủ tin cậy

Các cảnh báo bắt buộc:

- Revenue not verified.

- Spend not reconciled.

- Attribution conflict.

- Missing campaign mapping.

- Missing channel trace.

- Missing quote/order linkage.

- Duplicate events detected.

- Spam/bot suppression high.

- Payment waiting high.

- COD waiting high.

- Refund/return waiting.

- Complaint spike.

- Stock/sellable bị chặn.

- Sale Lock active.

- Dashboard refresh delayed.

- Late event window not closed.

- Owner review waiting.

- Sample size too small.

- Manual data included.

- Accounting reconcile waiting nếu có dùng.

Không được dùng màu sắc/trạng thái “tốt” cho KPI có cảnh báo P0.

## 53.2. Warning phải gắn vào từng KPI

Không chỉ cảnh báo chung ở đầu dashboard.

Mỗi KPI bị ảnh hưởng phải có warning riêng.

Ví dụ:

- ROAS bị ảnh hưởng bởi revenue waiting.

- CPA bị ảnh hưởng bởi duplicate comments.

- AOV bị ảnh hưởng bởi outlier order.

- Quote-to-order bị ảnh hưởng bởi Sale Lock.

- Order-to-revenue bị ảnh hưởng bởi COD waiting.

- Campaign performance bị ảnh hưởng bởi missing attribution.

- Live revenue bị ảnh hưởng bởi thiếu live session trace.

- CRM revenue bị ảnh hưởng bởi conflict với paid remarketing.

## 54. CAMPAIGN / ADSET / AD DECISION GOVERNANCE

## 54.1. Campaign level

Campaign-level decision được dùng để quyết định ngân sách tổng.

Campaign không được scale nếu:

- Campaign spend source chưa hợp lệ.

- Campaign attribution chưa pass.

- Campaign verified revenue chưa đạt.

- Campaign data quality fail.

- Campaign có risk incident.

- Campaign đẩy SKU not sellable.

- Campaign fulfillment không đáp ứng.

- Campaign owner approval thiếu.

## 54.2. Adset level

Adset-level decision dùng để tối ưu nhóm đối tượng, placement, ngân sách nhánh.

Adset không được đánh giá chỉ bằng CTR/CPC.

Adset phải đo đến ít nhất:

- Qualified conversation.

- Quote request.

- Official order.

- Verified revenue nếu đủ thời gian.

- CPA by selected conversion basis.

- ROAS by approved revenue basis.

- Data quality.

- Attribution status.

## 54.3. Ad creative level

Creative-level decision dùng để đánh giá thông điệp, hình ảnh, live hook, offer, CTA.

Creative tốt không chỉ là CTR cao.

Creative phải kiểm tra:

- Comment quality.

- Qualified conversation.

- Claim/safety risk.

- Privacy risk.

- Complaint signal.

- Product expectation mismatch.

- Quote/order conversion.

- Verified revenue contribution.

- Return/refund/complaint after purchase.

Creative tạo nhiều comment nhưng nhiều hiểu nhầm/claim risk không được scale mạnh.

## 55. LIVE / GOLDEN HOUR / PROGRAM MEASUREMENT GOVERNANCE

## 55.1. Live measurement phải có live session identity

Live performance chỉ được đo khi có live session identity hợp lệ.

Live Dashboard phải phân biệt:

- Live organic reach.

- Paid live boost nếu có.

- Live comment.

- Qualified live comment.

- Live-to-Messenger handoff.

- Live quote request.

- Live official order.

- Live verified revenue.

- Live complaint.

- Live fulfillment pressure.

- Live stock/sellable status.

- Live attribution conflict.

Không có live session identity thì không được gán revenue về phiên live.

## 55.2. Golden Hour measurement phải theo runtime

Nếu đo chương trình Giờ Vàng, PACK-07 không được tự xác nhận giá/chương trình.

PACK-07 chỉ consume program context từ owner runtime.

Dashboard phải phân biệt:

- Session status.

- Program price basis.

- QuoteSnapshot generated.

- Official order created.

- Payment/COD confirmed.

- Verified revenue.

- Quota/stock constraint nếu owner cung cấp.

- Early access effect nếu có runtime.

- Member/Diamond effect nếu policy cho phép.

- Revenue excluded do hết phiên/hết hiệu lực quote nếu có.

Không được tự gọi một order là Golden Hour revenue nếu thiếu program/session linkage.

## 55.3. 24/7 program measurement phải theo runtime

Nếu đo chương trình 24/7, PACK-07 chỉ consume program context từ Commerce Runtime.

PACK-07 không tự tính mức giảm.

PACK-07 không tự xác định khách đủ quyền lợi.

PACK-07 chỉ đo:

- Program-attributed quote.

- Program-attributed order.

- Program-attributed verified revenue.

- Program conflict với member/Diamond nếu owner event cung cấp.

- Program conversion.

- Program ROAS nếu attribution pass.

## 56. CRM / REBUY / MEMBER MEASUREMENT GOVERNANCE

## 56.1. CRM revenue phải tách khỏi Ads revenue nếu policy yêu cầu

CRM revenue đo mua lại, chăm sóc sau mua, member retention, seasonal suggestion, combo, quà sức khỏe hoặc upgrade.

CRM revenue không được tự cộng vào Ads revenue nếu attribution policy không cho phép.

Dashboard phải phân biệt:

- New customer from Ads.

- Existing customer from CRM.

- Member rebuy.

- Diamond/referral revenue.

- Ads-assisted rebuy.

- CRM-assisted rebuy.

- Mixed-source rebuy.

- Owner-review attribution.

## 56.2. Member performance không được tự tính quyền lợi

PACK-07 có thể đo performance theo member class nếu runtime cung cấp.

PACK-07 không tự xác định:

- Member tier.

- Member eligibility.

- Member discount.

- Upgrade/downgrade.

- Maintain tier.

- Benefit status.

Các giá trị này phải đến từ Commerce/Member owner.

PACK-07 chỉ đo outcome theo runtime value hợp lệ.

## 57. PRODUCT / STOCK / SELLABLE DECISION GOVERNANCE

## 57.1. Product performance phải gắn với sellable reality

Một sản phẩm có thể có demand cao nhưng không scale được nếu:

- Hết hàng.

- Not sellable.

- Sale Lock.

- Recall.

- Quality hold.

- Warehouse hold.

- Channel hold.

- Program price inactive.

- Quote unavailable.

- Fulfillment không đáp ứng.

Dashboard phải hiển thị rõ:

- Demand signal.

- Sellable capacity.

- Stock constraint.

- Sale Lock constraint.

- Fulfillment constraint.

- Verified revenue outcome.

## 57.2. Hero SKU bị chặn thì Scale Gate bị chặn

Nếu campaign đang đẩy hero SKU nhưng SKU bị chặn, Scale Gate phải bị chặn.

Không được scale campaign chỉ vì lead tốt nếu sản phẩm không đủ điều kiện bán.

Nếu cần chuyển hướng sang sản phẩm thay thế, đó là quyết định của owner/Commerce/AI Advisor, không phải PACK-07 tự quyết định.

PACK-07 chỉ ghi nhận:

- Hero SKU bị chặn.

- Alternative product conversion nếu owner event cung cấp.

- Revenue attribution sau khi product substitution được owner xử lý.

- Impact on campaign performance.

## 58. FULFILLMENT / CSKH CAPACITY GOVERNANCE

## 58.1. Scale phải xét năng lực vận hành

Ads có thể tạo nhu cầu nhanh hơn năng lực xử lý.

Scale Gate phải kiểm tra:

- AI Advisor load.

- Human takeover capacity.

- CSKH capacity.

- Order processing capacity.

- Warehouse picking/packing capacity.

- Delivery handoff capacity.

- COD handling capacity.

- Complaint handling capacity.

- Payment reconciliation capacity.

- Inventory availability.

- Production replenishment signal nếu owner cung cấp.

Nếu vận hành quá tải, Scale Gate phải block hoặc chỉ cho Scale Allowed Limited.

## 58.2. Fulfillment overload không được quy lỗi cho Ads hoặc AI đơn giản

Nếu funnel tốt nhưng fulfillment fail, dashboard phải phân loại đúng nguyên nhân.

Ví dụ:

- Ads tạo qualified lead tốt.

- AI quote/order tốt.

- Order tăng.

- Fulfillment chậm.

- COD fail tăng do giao trễ.

- Complaint tăng.

Trường hợp này không được kết luận Ads kém hoặc AI kém nếu bằng chứng chỉ ra fulfillment là điểm nghẽn.

## 59. OWNER REVIEW QUEUE

## 59.1. Các trường hợp bắt buộc vào Owner Review Queue

PACK-07 phải đưa vào Owner Review Queue khi:

- Attribution conflict chưa có rule.

- Revenue basis chưa được duyệt.

- Manual spend được nhập.

- Manual correction xuất hiện.

- Data quality warning ảnh hưởng decision.

- ROAS verified đạt nhưng sample size nhỏ.

- AOV bị outlier.

- CPA tốt nhưng complaint tăng.

- Campaign tốt nhưng stock hạn chế.

- Campaign tốt nhưng fulfillment gần quá tải.

- CRM/Ads/Diamond conflict.

- Live/Paid/Organic conflict.

- Claim/privacy risk.

- Scale override request.

- Resume after stop request.

- New campaign source cần phê duyệt.

- Dashboard mismatch.

- Revenue mismatch.

## 59.2. Owner Review Queue status

Các trạng thái tối thiểu:

- REVIEW_NOT_REQUIRED.

- REVIEW_REQUIRED.

- REVIEW_waiting_DATA.

- REVIEW_waiting_OWNER.

- REVIEW_APPROVED.

- REVIEW_REJECTED.

- REVIEW_ESCALATED.

- REVIEW_EXPIRED.

- REVIEW_LOCKED.

- REVIEW_REOPENED.

Không có REVIEW_APPROVED thì không được coi owner đã phê duyệt.

## 60. DECISION EVIDENCE REQUIREMENTS

## 60.1. Evidence cho Scale Decision

Scale Decision phải có evidence:

- Dashboard snapshot.

- Time window.

- Spend source.

- Campaign/adset/ad identity.

- Channel trace result.

- AI event trace result nếu có.

- Quote/order linkage result.

- Verified revenue result.

- Revenue basis.

- Attribution model.

- Attribution conflict status.

- Data quality result.

- ROAS result.

- CPA result.

- AOV result nếu dùng.

- Stock/sellable status.

- Sale Lock status.

- Fulfillment capacity status.

- Risk status.

- Owner approval.

- Effective scale scope.

Không có evidence thì không có Scale PASS.

## 60.2. Evidence cho Stop/Pause Decision

Stop/Pause Decision phải có evidence:

- Trigger reason.

- Campaign/adset/ad/page/live scope.

- Risk metric.

- Data quality state.

- Revenue state.

- Attribution state.

- Stock/sellable state.

- Sale Lock/Recall state nếu có.

- Complaint/claim/privacy state nếu có.

- Fulfillment state nếu có.

- Owner decision hoặc policy auto-stop rule.

- Timestamp.

- Review/resume condition.

Không có evidence thì không được gọi Stop/Pause Gate completed.

## 61. DASHBOARD RELEASE RULES

## 61.1. Dashboard production không được mở nếu thiếu Data Quality

Dashboard chỉ được dùng production decision khi có:

- Data quality check.

- Attribution check.

- Revenue verification check.

- Spend validation.

- Exclusion handling.

- waiting/missing data display.

- Owner review flow.

- Scale/Stop status.

- Evidence snapshot.

Dashboard chỉ hiển thị raw data thì chưa đủ để làm production decision dashboard.

## 61.2. Dashboard không được che rủi ro bằng chỉ số đẹp

Các chỉ số đẹp không được che các vấn đề P0.

Ví dụ:

- ROAS cao nhưng revenue waiting nhiều.

- CPA thấp nhưng spam comment cao.

- AOV cao nhưng do một đơn outlier.

- Quote cao nhưng order thấp.

- Order cao nhưng COD fail cao.

- Revenue cao nhưng refund/return cao.

- Live đông nhưng complaint tăng.

- Ads tốt nhưng SKU hero sale lock.

- AI conversion tốt nhưng privacy incident mở.

- CRM rebuy tốt nhưng attribution conflict chưa xử lý.

Dashboard phải hiển thị song song performance và risk.

## 62. DONE CONDITION CỦA PHẦN 3/4

## PHẦN 3/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Dashboard governance principles.

- Dashboard data layers.

- Executive Measurement Dashboard.

- Ads Performance Dashboard.

- Funnel Dashboard.

- Attribution Dashboard.

- Revenue Dashboard.

- Data Quality Dashboard.

- Risk Dashboard.

- KPI governance principles.

- ROAS governance.

- CPA governance.

- AOV governance.

- Core KPI Registry.

- Scale Gate governance.

- Stop/Pause Gate governance.

- Owner Decision governance.

- Scale Decision Matrix.

- Stop/Pause Decision Matrix.

- Data Reliability Warning.

- Campaign/adset/ad decision governance.

- Live/Golden Hour/24/7 measurement governance.

- CRM/rebuy/member measurement governance.

- Product/stock/sellable decision governance.

- Fulfillment/CSKH capacity governance.

- Owner Review Queue.

- Decision evidence requirements.

- Dashboard release rules.

## PHẦN 3/4 chưa định nghĩa Smoke Matrix, Done Gate tổng thể, Fail Gate tổng thể, Release Control, Evidence Acceptance và PACK-07 Final Conclusion. Các nội dung đó thuộc PHẦN 4/4.

## KẾT LUẬN PHẦN 3/4

PACK-07 Dashboard là lớp đo lường và hỗ trợ quyết định, không phải source-of-truth nghiệp vụ.

ROAS phải có revenue basis rõ.

CPA phải có conversion basis rõ.

AOV phải có order/revenue basis rõ.

Dashboard phải phân biệt raw, clean, verified, excluded, waiting, missing và conflict data.

Scale Gate không được PASS nếu thiếu data quality, verified revenue, attribution, stock/sellable, fulfillment readiness hoặc owner approval.

Stop/Pause Gate phải kích hoạt khi ROAS không tin cậy, CPA xấu, conversion tụt, complaint tăng, COD/payment fail cao, stock/sellable bị chặn, Sale Lock/Recall ảnh hưởng, privacy/claim incident mở hoặc owner yêu cầu.

Nguyên tắc khóa của PHẦN 3/4 là:

Dashboard không thay thế source-of-truth.

ROAS đẹp nhưng dữ liệu chưa sạch thì không được scale.

Comment nhiều không đồng nghĩa bán tốt.

Quote nhiều không đồng nghĩa có doanh thu.

Order nhiều không đồng nghĩa revenue verified.

Scale phải đi qua Data Quality, Verified Revenue, Attribution, Stock/Sellable, Fulfillment và Owner Decision.

## 63. MỤC TIÊU CỦA PHẦN 4/4

## PHẦN 4/4 thiết lập lớp kiểm chứng cuối cho PACK-07, bao gồm Smoke Matrix, Evidence, Done Gate, Fail Gate, Release Control và kết luận hoàn tất tài liệu.

Mục tiêu của phần này là khóa rõ:

- PACK-07 phải được kiểm thử bằng smoke nào.

- Điều kiện nào được xem là đo lường hợp lệ.

- Điều kiện nào được xem là ROAS đáng tin cậy.

- Điều kiện nào được xem là đủ dữ liệu để Scale Gate.

- Điều kiện nào buộc phải Fail Gate.

- Evidence nào bắt buộc trước khi gọi PASS.

- Ai có quyền review và sign-off.

- Trạng thái nào được phép gọi là hoàn tất tài liệu.

- Trạng thái nào chưa được gọi là Production Ready.

- Điều kiện nào cần đạt trước khi PACK-07 được dùng cho quyết định scale production.

## PHẦN 4/4 không thiết kế code, API, database, UI hoặc schema chi tiết.

## PHẦN 4/4 là lớp governance kiểm soát chất lượng triển khai PACK-07.

## 64. NGUYÊN TẮC SMOKE CỐT LÕI

## 64.1. Smoke không phải test hình thức

Smoke trong PACK-07 không phải kiểm tra dashboard có hiển thị hay không.

Smoke phải chứng minh dữ liệu đi đúng từ nguồn đến quyết định.

Smoke phải chứng minh:

- Ads spend có nguồn hợp lệ.

- Campaign/adset/ad identity được nhận diện đúng.

- Channel event đi qua PACK-06.

- AI event đi qua PACK-05.

- Quote/order/revenue đi qua owner runtime.

- Revenue chưa verified thì không tính ROAS chính thức.

- Attribution conflict được phát hiện.

- Exclusion rule hoạt động.

- Data Quality chặn được dữ liệu không đủ tin cậy.

- Scale Gate không PASS khi thiếu điều kiện.

- Stop/Pause Gate kích hoạt khi có rủi ro.

- Dashboard không che giấu waiting/missing/conflict data.

- Evidence được ghi nhận đầy đủ.

- Owner Decision được audit.

Smoke phải kiểm tra đúng nghiệp vụ, không chỉ kiểm tra màn hình.

## 64.2. Không có smoke pass thì không có ROAS PASS

PACK-07 không được gọi ROAS PASS nếu chưa có smoke pass.

Không có smoke pass thì:

- Không được dùng dashboard cho scale production.

- Không được gọi data quality là production-grade.

- Không được gọi attribution là đáng tin cậy.

- Không được gọi verified revenue là cơ sở ROAS nếu chưa test linkage.

- Không được gọi Scale Gate PASS.

- Không được gọi Release Ready.

- Không được gọi Production Ready.

Smoke pass phải đi kèm evidence accepted.

## 64.3. Smoke phải có positive path và negative path

PACK-07 phải test cả trường hợp đúng và sai.

Positive path chứng minh luồng hợp lệ đi qua được.

Negative path chứng minh hệ thống chặn đúng khi dữ liệu sai, thiếu, trùng, waiting, conflict hoặc rủi ro.

Nếu chỉ test positive path mà không test negative path, PACK-07 chưa đủ Done Gate.

## 65. SMOKE MATRIX TỔNG THỂ

## 65.1. ADS-SMK-001 — Ads Spend Source Validation Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-001

Mục tiêu | Kiểm tra Ads spend source hợp lệ trước khi tính ROAS

Input | Spend record có campaign/adset/ad/time window/currency/source

Expected result | Spend được xác thực, có evidence, được phép dùng cho ROAS nếu data quality pass

Negative path | Thiếu spend source, thiếu campaign id, spend nhập tay chưa duyệt

Fail condition | Dashboard vẫn tính ROAS chính thức khi spend source không hợp lệ

Evidence | Spend source snapshot, validation result, data quality event, audit

## 65.2. ADS-SMK-002 — Campaign / Adset / Ad Identity Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-002

Mục tiêu | Kiểm tra campaign/adset/ad identity được nhận diện và map đúng

Input | Event có campaign, adset, ad context

Expected result | Mapping rõ campaign -> adset -> ad -> time window

Negative path | Thiếu adset/ad, sai mapping, campaign không thuộc owner-approved source

Fail condition | Revenue vẫn được gán về campaign khi mapping thiếu

Evidence | Mapping snapshot, data quality result, attribution readiness result

## 65.3. ADS-SMK-003 — PACK-06 Channel Event Trace Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-003

Mục tiêu | Kiểm tra channel event Facebook/Messenger/Live phải đi qua PACK-06

Input | Comment, Messenger event, Live event

Expected result | Event có Page context, routing context, dedup, privacy/spam status, channel evidence

Negative path | Raw Facebook event chưa qua PACK-06

Fail condition | Raw event vẫn được dùng cho production ROAS

Evidence | PACK-06 channel trace, event correlation, privacy/spam status

## 65.4. ADS-SMK-004 — Comment -> Messenger Trace Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-004

Mục tiêu | Kiểm tra trace từ comment public sang Messenger/private

Input | Comment có ý định mua hoặc hỏi giá

Expected result | Comment được phân loại, route private, giữ context sang Messenger

Negative path | Messenger order không có comment-to-Messenger trace

Fail condition | Doanh thu Messenger vẫn gán về comment/live/ad khi trace đứt

Evidence | Comment event, private handoff event, Messenger event, correlation chain

## 65.5. ADS-SMK-005 — PACK-05 AI Event Trace Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-005

Mục tiêu | Kiểm tra AI Advisor event được consume đúng, không viết lại AI behavior

Input | Intent event, response event, product recommendation, quote request, handoff

Expected result | AI event có owner PACK-05, guard status, handoff status, correlation

Negative path | Dashboard tự suy luận AI success/failure không có event

Fail condition | PACK-07 tự đánh giá AI conversion khi thiếu PACK-05 trace

Evidence | AI event trace, guard result, handoff reason, correlation

## 65.6. ADS-SMK-006 — Quote Event Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-006

Mục tiêu | Kiểm tra QuoteSnapshot chỉ là funnel event, không phải revenue

Input | Quote request, QuoteSnapshot generated

Expected result | Quote được hiển thị ở tầng quote, không vào revenue

Negative path | Quote expired, quote bị chặn do sellable/stock/sale lock

Fail condition | Dashboard tính quote value vào ROAS chính thức

Evidence | Quote event, quote status, sellable/stock/sale lock result

## 65.7. ADS-SMK-007 — Cart Draft Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-007

Mục tiêu | Kiểm tra cart draft không bị tính là official order

Input | Cart draft được tạo từ quote

Expected result | Cart nằm ở funnel cart, chưa vào order/revenue

Negative path | Cart bỏ dở, cart bị sửa, cart hết hạn

Fail condition | Cart draft được tính là order hoặc revenue

Evidence | Cart event, cart status, customer confirmation status

## 65.8. ADS-SMK-008 — Official Order Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-008

Mục tiêu | Kiểm tra chỉ official order/order_code mới được tính là order

Input | Customer confirmation và owner Order tạo order_code

Expected result | ORDER_EVENT hợp lệ, có order identity

Negative path | Khách nhắn “lấy 2 hộp” nhưng chưa có order_code

Fail condition | Dashboard gọi Messenger text/cart draft là order chính thức

Evidence | ORDER_EVENT, order_code, customer confirmation, audit

## 65.9. ADS-SMK-009 — Payment / COD Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-009

Mục tiêu | Kiểm tra payment/COD status phải đến từ owner runtime

Input | Payment confirmed hoặc COD success

Expected result | Payment/COD event hợp lệ, không tự xác nhận từ tin nhắn

Negative path | Ảnh chuyển khoản chưa đối chiếu, COD waiting, payment failed

Fail condition | Dashboard tính paid revenue khi chưa có owner payment confirmation

Evidence | Payment/COD event, reconciliation status, owner confirmation

## 65.10. ADS-SMK-010 — Verified Revenue Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-010

Mục tiêu | Kiểm tra Verified Revenue là điều kiện cho ROAS chính thức

Input | Order đạt verified revenue theo owner policy

Expected result | VERIFIED_REVENUE_EVENT được tạo, có revenue basis

Negative path | Revenue waiting, refund waiting, return waiting, complaint unresolved

Fail condition | ROAS chính thức tính từ revenue waiting

Evidence | Verified revenue event, revenue basis, exclusion result, owner confirmation

## 65.11. ADS-SMK-011 — Revenue Exclusion Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-011

Mục tiêu | Kiểm tra hủy/hoàn/đổi/COD fail/test order bị loại khỏi ROAS

Input | Order canceled/refunded/returned/COD failed/test/internal

Expected result | Revenue bị đưa vào excluded bucket, không tính ROAS

Negative path | Partial refund hoặc late return sau khi revenue đã waiting

Fail condition | Excluded revenue vẫn xuất hiện trong ROAS chính thức

Evidence | Exclusion event, reason, owner source, dashboard excluded view

## 65.12. ADS-SMK-012 — Attribution Conflict Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-012

Mục tiêu | Kiểm tra conflict Ads/CRM/Diamond/Organic/Live được phát hiện

Input | Một order có nhiều touchpoint nguồn

Expected result | ATTRIBUTION_CONFLICT được tạo, không gán nguồn chính thức khi chưa có rule

Negative path | Ads click trước, Diamond link sau; CRM trước, remarketing sau

Fail condition | Dashboard tự gán doanh thu về một nguồn khi conflict chưa xử lý

Evidence | Touchpoint chain, conflict status, attribution rule status

## 65.13. ADS-SMK-013 — Attribution Rule Applied Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-013

Mục tiêu | Kiểm tra attribution rule được áp đúng khi owner phê duyệt

Input | Conversion có đủ touchpoint, rule và verified revenue

Expected result | ATTRIBUTION_LOCKED hoặc CONFLICT_RESOLVED

Negative path | Rule thiếu, rule hết hiệu lực, rule không đúng scope

Fail condition | Attribution result không ghi rule/evidence

Evidence | Rule used, model, owner approval, attribution result

## 65.14. ADS-SMK-014 — Data Quality Gate Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-014

Mục tiêu | Kiểm tra Data Quality phải PASS trước reporting/scale

Input | Batch dữ liệu có valid, missing, duplicate, conflict, waiting

Expected result | Dữ liệu được phân lớp raw/clean/verified/excluded/waiting/missing

Negative path | Duplicate, missing campaign, missing quote/order linkage

Fail condition | Dashboard vẫn READY_FOR_DECISION khi data quality fail

Evidence | DATA_QUALITY_EVENT, DQ status, excluded/missing summary

## 65.15. ADS-SMK-015 — Dashboard Layer Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-015

Mục tiêu | Kiểm tra dashboard phân biệt raw/clean/verified/excluded/waiting

Input | Tập dữ liệu nhiều trạng thái

Expected result | Dashboard hiển thị đúng từng bucket, không gộp sai

Negative path | waiting revenue, excluded revenue, missing attribution

Fail condition | Dashboard hiển thị một số tổng doanh thu không rõ class

Evidence | Dashboard snapshot, data layer result, warning display

## 65.16. ADS-SMK-016 — ROAS Reliability Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-016

Mục tiêu | Kiểm tra ROAS chỉ đáng tin cậy khi đủ spend, attribution, verified revenue, DQ

Input | Campaign có spend và verified revenue hợp lệ

Expected result | ROAS_VERIFIED hoặc ROAS_NET_VERIFIED nếu policy dùng

Negative path | Spend thiếu, attribution conflict, revenue waiting

Fail condition | ROAS_SCALE_ELIGIBLE khi thiếu DQ/verified revenue

Evidence | Spend source, revenue basis, attribution result, DQ result

## 65.17. ADS-SMK-017 — CPA / AOV Basis Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-017

Mục tiêu | Kiểm tra CPA/AOV có conversion basis và revenue/order basis rõ

Input | Campaign có nhiều funnel stages

Expected result | CPA và AOV hiển thị đúng basis

Negative path | CPA comment bị gọi chung CPA, AOV quote bị gọi AOV doanh thu

Fail condition | KPI không khai báo basis nhưng dùng cho scale

Evidence | KPI definition, dashboard snapshot, basis label

## 65.18. ADS-SMK-018 — Scale Gate Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-018

Mục tiêu | Kiểm tra Scale Gate chỉ PASS khi đủ điều kiện

Input | Campaign có ROAS verified, CPA đạt, DQ pass, stock/sellable pass

Expected result | SCALE_ALLOWED hoặc SCALE_ALLOWED_LIMITED theo điều kiện

Negative path | ROAS raw đẹp nhưng revenue waiting; SKU hero not sellable; owner approval thiếu

Fail condition | Scale Gate PASS khi thiếu điều kiện P0

Evidence | Scale evaluation, dashboard snapshot, owner decision

## 65.19. ADS-SMK-019 — Stop / Pause Gate Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-019

Mục tiêu | Kiểm tra Stop/Pause Gate kích hoạt khi rủi ro

Input | Campaign có complaint tăng, COD fail cao, sale lock, privacy incident

Expected result | PAUSE_REQUIRED, STOP_REQUIRED hoặc OWNER_REVIEW_REQUIRED

Negative path | ROAS tốt nhưng Sale Lock active

Fail condition | Campaign vẫn Scale Allowed khi có P0 risk

Evidence | Risk dashboard, stop trigger, owner decision, audit

## 65.20. ADS-SMK-020 — Owner Decision Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-020

Mục tiêu | Kiểm tra quyết định owner được ghi nhận trước scale/stop/resume

Input | Scale/stop decision request

Expected result | Có owner decision, scope, reason, effective period, evidence

Negative path | Dashboard tự chuyển sang Scale Approved

Fail condition | Không có owner approval nhưng vẫn gọi Scale Approved

Evidence | Owner decision record, dashboard snapshot, audit

## 65.21. ADS-SMK-021 — Live / Golden Hour Measurement Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-021

Mục tiêu | Kiểm tra live/Giờ Vàng đo theo live/session/program runtime

Input | Live event, Golden Hour session, quote/order/revenue linkage

Expected result | Revenue chỉ gán live/program khi có session/program linkage hợp lệ

Negative path | Comment live không trace sang Messenger; quote hết hiệu lực

Fail condition | Dashboard gán revenue về live/Giờ Vàng khi thiếu linkage

Evidence | Live session context, program context, quote/order/revenue trace

## 65.22. ADS-SMK-022 — CRM / Diamond / Referral Conflict Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-022

Mục tiêu | Kiểm tra không tính trùng Ads, CRM, Diamond/referral

Input | Customer có Ads touchpoint, CRM event, Diamond referral bind

Expected result | Conflict được phát hiện, rule áp dụng nếu có

Negative path | Không có referral bind, CRM rebuy nhưng gán Ads

Fail condition | Một verified revenue được tính trùng nhiều nguồn

Evidence | Touchpoint chain, referral bind, CRM event, attribution result

## 65.23. ADS-SMK-023 — Stock / Sellable / Sale Lock Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-023

Mục tiêu | Kiểm tra Scale Gate bị chặn khi SKU không đủ điều kiện bán

Input | Campaign hero SKU hết hàng/not sellable/sale lock

Expected result | SCALE_STOCK_BLOCKED hoặc SCALE_SALE_LOCK_BLOCKED

Negative path | Lead tốt nhưng SKU bị chặn

Fail condition | Campaign vẫn Scale Allowed khi Sale Lock active

Evidence | Sellable status, stock status, sale lock status, scale result

## 65.24. ADS-SMK-024 — Fulfillment Capacity Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-024

Mục tiêu | Kiểm tra scale phải xét năng lực CSKH/kho/giao hàng

Input | Campaign có demand tăng, fulfillment gần quá tải

Expected result | SCALE_ALLOWED_LIMITED hoặc SCALE_FULFILLMENT_BLOCKED

Negative path | Fulfillment quá tải nhưng ROAS vẫn tốt

Fail condition | Scale mạnh khi fulfillment không đáp ứng

Evidence | Fulfillment signal, CSKH load, warehouse capacity, owner decision

## 66. EVIDENCE GOVERNANCE

## 66.1. Không có evidence thì không có PASS

PACK-07 không được gọi PASS nếu thiếu evidence.

Evidence là điều kiện bắt buộc cho:

- Data Quality PASS.

- ROAS PASS.

- Attribution PASS.

- Dashboard READY_FOR_DECISION.

- Scale Gate PASS.

- Stop/Pause Gate completed.

- Owner Decision approved.

- Release Ready.

Evidence không phải ảnh chụp rời rạc không có trace.

Evidence phải có nguồn, trạng thái, reviewer, scope và correlation.

## 66.2. Evidence Registry tối thiểu

PACK-07 phải có Evidence Registry cho các nhóm:

- Ads spend source evidence.

- Campaign/adset/ad mapping evidence.

- PACK-06 channel trace evidence.

- PACK-05 AI event trace evidence.

- Quote event evidence.

- Cart event evidence.

- Official order evidence.

- Payment/COD evidence.

- Delivery evidence nếu policy dùng.

- Verified revenue evidence.

- Revenue exclusion evidence.

- Attribution rule evidence.

- Attribution result evidence.

- Data quality evidence.

- Dashboard snapshot evidence.

- Scale Gate evidence.

- Stop/Pause Gate evidence.

- Owner Decision evidence.

- Risk incident evidence.

- Release review evidence.

## 66.3. Evidence status

Evidence phải có trạng thái tối thiểu:

- EVIDENCE_waiting.

- EVIDENCE_SUBMITTED.

- EVIDENCE_ACCEPTED.

- EVIDENCE_REJECTED.

- EVIDENCE_NEEDS_RETEST.

- EVIDENCE_EXPIRED.

- EVIDENCE_SUPERSEDED.

- EVIDENCE_BLOCKED.

- EVIDENCE_OWNER_REVIEW_REQUIRED.

Chỉ EVIDENCE_ACCEPTED mới được dùng cho PASS.

Evidence waiting không được dùng để gọi PASS.

Evidence rejected không được dùng cho release.

Evidence expired phải retest hoặc re-accept.

## 66.4. Evidence phải có metadata

Mỗi evidence record cần có:

- Evidence ID.

- Pack ID.

- Smoke ID hoặc Gate ID.

- Rule ID nếu áp dụng.

- Environment.

- Time window.

- Source system.

- Owner source.

- Test result.

- Data quality status.

- Reviewer.

- Review timestamp.

- Acceptance status.

- Correlation ID.

- Audit reference.

- Notes nếu có.

- Retest reference nếu có.

Không có metadata thì evidence chưa đủ chuẩn governance.

## 67. DATA QUALITY DONE GATE

## 67.1. Điều kiện Data Quality Done Gate

Data Quality Done Gate chỉ PASS khi:

- Ads spend source hợp lệ.

- Campaign/adset/ad identity hợp lệ.

- Channel event qua PACK-06.

- AI event qua PACK-05 nếu dùng.

- Event dedup pass.

- Spam/bot suppression pass.

- Privacy guard pass.

- Missing required field được xử lý.

- Quote/order linkage pass.

- Payment/COD/revenue linkage pass nếu dùng.

- Attribution conflict được xử lý hoặc đưa vào waiting/excluded.

- Revenue exclusion hoạt động.

- Dashboard phân lớp dữ liệu đúng.

- Evidence accepted.

## 67.2. Data Quality Fail Gate

Data Quality phải FAIL nếu:

- Spend source không hợp lệ.

- Campaign mapping thiếu.

- Channel trace thiếu.

- Event duplicate chưa xử lý.

- Spam/bot chưa loại.

- Privacy-bị chặn event vẫn dùng.

- Quote/order linkage đứt.

- Revenue waiting bị tính verified.

- Attribution conflict chưa xử lý nhưng vẫn gán nguồn.

- Excluded revenue vẫn đi vào ROAS.

- Dashboard không hiển thị warning.

- Evidence thiếu hoặc rejected.

## 68. ATTRIBUTION DONE GATE

## 68.1. Điều kiện Attribution Done Gate

Attribution Done Gate chỉ PASS khi:

- Attribution model được khai báo.

- Touchpoint chain có đủ dữ liệu.

- Customer/session/order linkage hợp lệ.

- Source classification rõ Paid/Organic/CRM/Diamond/Live/Mixed/Unknown.

- Conflict detection hoạt động.

- Conflict resolution rule được owner phê duyệt nếu dùng.

- Attribution result có evidence.

- Revenue không bị tính trùng nhiều nguồn.

- Dashboard hiển thị conflict/waiting/excluded đúng.

- Evidence accepted.

## 68.2. Attribution Fail Gate

Attribution phải FAIL nếu:

- Không có attribution model.

- Missing data bị gán Organic mặc định.

- Ads revenue được kết luận khi thiếu campaign/ad context.

- Diamond revenue được kết luận khi thiếu referral bind.

- CRM revenue bị gộp vào Ads revenue không có rule.

- Live revenue được gán khi thiếu live session identity.

- Một order được tính trùng nhiều nguồn.

- Conflict chưa xử lý nhưng vẫn đưa vào source ROAS.

- Attribution result thiếu evidence.

- Owner rule thiếu hoặc hết hiệu lực.

## 69. VERIFIED REVENUE DONE GATE

## 69.1. Điều kiện Verified Revenue Done Gate

Verified Revenue Done Gate chỉ PASS khi:

- Có official order/order_code.

- Có Customer Confirmation nếu policy yêu cầu.

- Có payment/COD/delivery status đạt điều kiện.

- Có revenue owner confirmation.

- Có refund/cancel/return exclusion check.

- Có complaint/risk exclusion check nếu policy yêu cầu.

- Có revenue class rõ.

- Có revenue basis rõ.

- Có attribution status.

- Có data quality pass.

- Có evidence accepted.

## 69.2. Verified Revenue Fail Gate

Verified Revenue phải FAIL nếu:

- Không có official order.

- Không có order_code.

- Payment waiting bị tính paid revenue.

- COD waiting bị tính COD success.

- Delivery waiting bị tính delivered revenue.

- Revenue waiting bị tính verified revenue.

- Refund/return/cancel chưa xử lý.

- Test/internal/sample order bị tính vào ROAS khi policy loại.

- Revenue source không phải owner runtime.

- Evidence thiếu hoặc không accepted.

## 70. DASHBOARD DONE GATE

## 70.1. Điều kiện Dashboard Done Gate

Dashboard Done Gate chỉ PASS khi:

- Dashboard phân biệt raw/clean/verified/excluded/waiting/missing/conflict.

- ROAS có revenue basis.

- CPA có conversion basis.

- AOV có order/revenue basis.

- Funnel không nhảy tầng.

- Data Quality status hiển thị rõ.

- Attribution status hiển thị rõ.

- Revenue verification status hiển thị rõ.

- Risk warning hiển thị rõ.

- Scale/Stop status hiển thị rõ.

- Owner Review Queue hoạt động.

- Dashboard snapshot evidence accepted.

## 70.2. Dashboard Fail Gate

Dashboard phải FAIL nếu:

- Gộp mọi doanh thu thành một số tổng không rõ class.

- Hiển thị ROAS chính thức khi revenue chưa verified.

- Hiển thị CPA không có conversion basis.

- Hiển thị AOV không có order/revenue basis.

- Không hiển thị waiting revenue.

- Không hiển thị excluded revenue.

- Không hiển thị attribution conflict.

- Không hiển thị data quality warning.

- Không hiển thị Sale Lock/stock/sellable risk.

- Không có evidence snapshot.

- Dashboard được dùng scale khi chưa READY_FOR_DECISION.

## 71. SCALE GATE DONE / FAIL

## 71.1. Điều kiện Scale Gate Done

Scale Gate chỉ PASS khi:

- Data Quality PASS.

- Attribution PASS.

- Verified Revenue PASS.

- ROAS đạt ngưỡng owner policy.

- CPA đạt ngưỡng owner policy.

- AOV đạt ngưỡng owner policy nếu dùng.

- COD/payment fail trong giới hạn.

- Refund/return trong giới hạn.

- Complaint trong giới hạn.

- Privacy/claim incident không mở.

- SKU hero sellable.

- Stock đủ hoặc có constraint rõ.

- Sale Lock/Recall không ảnh hưởng SKU.

- Fulfillment/CSKH đáp ứng.

- AI/human handoff không quá tải.

- Dashboard READY_FOR_DECISION.

- Owner approval có hiệu lực.

- Scale evidence accepted.

## 71.2. Scale Gate Fail

Scale Gate phải FAIL hoặc bị chặn khi:

- Data quality fail.

- Attribution conflict unresolved.

- Revenue chưa verified.

- ROAS không đáng tin cậy.

- CPA vượt ngưỡng.

- COD/payment fail cao.

- Refund/return cao.

- Complaint tăng cao.

- Privacy/claim incident mở.

- Hero SKU hết hàng.

- SKU not sellable.

- Sale Lock/Recall active.

- Fulfillment quá tải.

- CSKH quá tải.

- AI/human handoff quá tải.

- Owner approval thiếu.

- Dashboard mismatch.

- Evidence thiếu hoặc rejected.

## 72. STOP / PAUSE GATE DONE / FAIL

## 72.1. Điều kiện Stop/Pause Gate Done

Stop/Pause Gate được xem là completed khi:

- Trigger reason được xác định.

- Campaign/adset/ad/page/live scope rõ.

- Risk metric được ghi nhận.

- Dashboard snapshot có evidence.

- Data quality state rõ.

- Attribution state rõ.

- Revenue state rõ.

- Stock/sellable/sale lock state rõ.

- Complaint/claim/privacy state rõ.

- Fulfillment state rõ nếu liên quan.

- Owner decision hoặc policy auto-stop rule có hiệu lực.

- Pause/stop/resume condition được ghi nhận.

- Audit/evidence accepted.

## 72.2. Stop/Pause Gate Fail

Stop/Pause Gate phải FAIL nếu:

- Risk trigger không được ghi nhận.

- Campaign có P0 risk nhưng vẫn Scale Allowed.

- Privacy incident không chặn scale.

- Claim/safety incident không chặn scale.

- Sale Lock active nhưng campaign không pause/stop.

- SKU not sellable nhưng campaign vẫn scale.

- COD/payment fail cao nhưng không cảnh báo.

- Complaint spike không vào risk dashboard.

- Owner stop request không được ghi nhận.

- Evidence thiếu hoặc rejected.

## 73. RELEASE CONTROL GOVERNANCE

## 73.1. PACK-07 không được Release Ready chỉ vì tài liệu xong

Hoàn tất tài liệu PACK-07 không đồng nghĩa release.

PACK-07 chỉ được xem là Release Ready khi có đủ:

- Implementation hoàn tất theo governance.

- Smoke pass.

- Data Quality pass.

- Attribution pass.

- Verified Revenue linkage pass.

- Dashboard pass.

- Scale Gate pass.

- Stop/Pause Gate pass.

- Evidence accepted.

- No P0 open.

- Owner sign-off.

- Release decision.

Trước khi đạt đủ các điều kiện trên, trạng thái vẫn là:

GOVERNANCE_DOCUMENTATION_COMPLETE — waiting IMPLEMENTATION / TEST / SMOKE / EVIDENCE / OWNER SIGN-OFF / RELEASE DECISION

## 73.2. Release status registry

Các trạng thái release tối thiểu:

- RELEASE_NOT_STARTED.

- Documentation Complete.

- IMPLEMENTATION_waiting.

- IMPLEMENTATION_IN_PROGRESS.

- IMPLEMENTATION_COMPLETE_waiting_TEST.

- SMOKE_waiting.

- SMOKE_FAILED.

- SMOKE PASS WAITING EVIDENCE.

- EVIDENCE_waiting.

- EVIDENCE_REJECTED.

- EVIDENCE_ACCEPTED.

- OWNER_REVIEW_waiting.

- OWNER_SIGNED_OFF.

- RELEASE_DECISION_waiting.

- RELEASE_BLOCKED.

- RELEASE_ROLLBACK_REQUIRED nếu có sự cố sau release.

PACK-07 không được nhảy trạng thái.

## 73.3. Release STOP_POINTS

Các STOP_POINTS P0:

- Không có Ads spend source hợp lệ.

- Không có campaign/adset/ad mapping.

- Không có PACK-06 channel trace.

- Không có PACK-05 AI event trace khi đo AI conversion.

- Không có quote/order/revenue linkage.

- Không có verified revenue event.

- Không có attribution rule.

- Attribution conflict chưa xử lý.

- Data quality fail.

- Dashboard không phân biệt raw/clean/verified/excluded/waiting.

- ROAS tính từ revenue waiting.

- Excluded revenue vẫn vào ROAS.

- Duplicate event chưa xử lý.

- Privacy/claim incident chưa xử lý.

- Scale Gate PASS khi thiếu owner approval.

- Stop/Pause Gate không kích hoạt khi có P0 risk.

- Evidence thiếu hoặc rejected.

- P0 issue còn mở.

Chỉ cần một P0 STOP_POINTS còn mở thì PACK-07 không được Release Approved.

## 74. ROLLBACK / RECOVERY GOVERNANCE

## 74.1. Rollback không được xóa lịch sử đo lường

Nếu PACK-07 dashboard, attribution hoặc ROAS bị lỗi, rollback không được xóa event lịch sử.

Rollback chỉ được:

- Dừng dùng metric lỗi cho decision.

- Đưa dashboard về trạng thái warning.

- Khóa Scale Gate.

- Chuyển owner review.

- Chạy lại data quality.

- Chạy lại attribution nếu rule sửa.

- Rebuild projection nếu cần.

- Gắn evidence mới.

- Ghi nhận audit.

Không được sửa tay số liệu cũ để làm đẹp dashboard.

Không được xóa dấu vết conflict.

Không được xóa event bị excluded.

## 74.2. Khi nào cần rollback

Rollback hoặc recovery bắt buộc khi:

- ROAS tính sai revenue basis.

- waiting revenue bị tính verified.

- Campaign attribution sai.

- Revenue bị tính trùng nhiều nguồn.

- Excluded revenue vào ROAS.

- Spend import trùng.

- Dashboard gộp sai dữ liệu.

- Scale Gate PASS sai.

- Stop Gate không kích hoạt khi có P0 risk.

- Privacy/claim incident liên quan đo lường.

- Owner yêu cầu.

## 74.3. Recovery Done Gate

Recovery chỉ done khi:

- Nguyên nhân lỗi được xác định.

- Dữ liệu ảnh hưởng được khoanh vùng.

- Metric bị lỗi được đánh dấu.

- Dashboard được cảnh báo hoặc tạm khóa.

- Data quality được chạy lại.

- Attribution được chạy lại nếu cần.

- Revenue basis được kiểm tra lại.

- Scale/Stop decision bị ảnh hưởng được review.

- Evidence mới accepted.

- Owner sign-off recovery.

## 75. SECURITY / PRIVACY / CLAIM CONTROL TRONG PACK-07

## 75.1. PACK-07 phải tôn trọng privacy

PACK-07 không được public dữ liệu cá nhân của khách.

Dashboard chỉ được hiển thị dữ liệu theo quyền.

Các dữ liệu cần kiểm soát:

- Tên khách hàng.

- Số điện thoại.

- Địa chỉ.

- Nội dung Messenger.

- Thông tin đơn hàng cá nhân.

- Thông tin thanh toán.

- Tình trạng khiếu nại.

- Lịch sử mua hàng.

- Member status.

- Referral relationship.

PACK-07 chỉ đo lường theo identity được phép và policy cho phép.

## 75.2. Privacy incident chặn scale

Nếu có privacy incident, PACK-07 phải:

- Đánh dấu incident.

- Chặn scale nếu incident P0.

- Đưa vào Risk Dashboard.

- Gửi Owner Review.

- Giữ evidence.

- Không che cảnh báo bằng KPI tốt.

Trạng thái:

## PRIVACY_INCIDENT_OPEN — SCALE_BLOCKED

## 75.3. Claim incident chặn scale

Nếu Ads/Live/AI/CRM có claim/safety incident liên quan nội dung không được phép, PACK-07 phải chặn scale theo policy.

Không được scale campaign có claim risk chỉ vì ROAS đẹp.

Trạng thái:

## CLAIM_INCIDENT_OPEN — SCALE_BLOCKED / STOP_REQUIRED

## 76. ACCESS CONTROL / ROLE GOVERNANCE

## 76.1. Quyền xem dashboard

PACK-07 phải phân quyền xem dữ liệu.

Nhóm quyền tối thiểu:

- Owner view.

- Ads operator view.

- Marketing analyst view.

- Finance/revenue view nếu được phép.

- CSKH/risk view nếu cần.

- Read-only auditor view.

- Restricted privacy view.

Không phải ai cũng được xem dữ liệu khách hàng, doanh thu, order, payment hoặc member/referral.

## 76.2. Quyền quyết định

Các hành động cần quyền:

- Approve attribution model.

- Approve revenue basis.

- Approve manual spend.

- Approve manual correction.

- Approve scale.

- Approve stop/pause.

- Approve resume.

- Approve override.

- Accept evidence.

- Close P0 issue.

- Release approval.

Không được để dashboard user tự thay đổi rule nguồn doanh thu nếu không có quyền.

## 77. PACK-07 FINAL DONE GATE

## 77.1. Điều kiện PACK-07 Documentation Done

PACK-07 được xem là hoàn tất tài liệu khi có đủ 4 phần:

- PHẦN 1/4 — Ads Measurement Principles / Source-of-Truth Boundary / Dependency.

- PHẦN 2/4 — Event Taxonomy / Attribution Model / Funnel Metrics / Verified Revenue Rules.

- PHẦN 3/4 — Dashboard / KPI / ROAS / CPA / AOV / Scale Gate / Stop Gate / Owner Decision.

- PHẦN 4/4 — Smoke Matrix / Done Gate / Fail Gate / Release Control / Final Conclusion.

Trạng thái tài liệu:

## GOVERNANCE_DOCUMENTATION_COMPLETE

## 77.2. Điều kiện PACK-07 Implementation Done

Implementation Done chỉ được gọi khi:

- Dev đã triển khai theo PACK-07.

- Không tạo parallel source-of-truth.

- Không tự tính revenue ngoài owner runtime.

- Không tự đọc raw Facebook event để đo production.

- Không tự xác nhận payment/COD/delivery.

- Không tự sync MISA.

- Không tự scale Ads khi thiếu owner decision.

- Event taxonomy vận hành đúng.

- Data quality vận hành đúng.

- Attribution vận hành đúng.

- Dashboard vận hành đúng.

- Scale/Stop Gate vận hành đúng.

- Audit/evidence vận hành đúng.

## 77.3. Điều kiện PACK-07 Smoke Done

Smoke Done chỉ được gọi khi:

- ADS-SMK-001 đến ADS-SMK-024 được test.

- Positive path pass.

- Negative path pass.

- No P0 smoke fail.

- Retest pass nếu có lỗi.

- Evidence accepted cho từng smoke.

- Owner review pass.

## 77.4. Điều kiện PACK-07 ROAS PASS

ROAS PASS chỉ được gọi khi:

- Ads spend source hợp lệ.

- Campaign/adset/ad mapping hợp lệ.

- Attribution pass.

- Verified revenue pass.

- Revenue exclusion pass.

- Data quality pass.

- Dashboard hiển thị revenue basis rõ.

- waiting/excluded/conflict data không bị tính sai.

- Evidence accepted.

- Owner sign-off.

Không có Verified Revenue thì không có ROAS PASS.

Không có Data Quality PASS thì không có ROAS PASS.

Không có Attribution PASS thì không có source ROAS PASS.

## 77.5. Điều kiện PACK-07 Scale PASS

Scale PASS chỉ được gọi khi:

- ROAS PASS.

- CPA đạt owner threshold.

- AOV đạt owner threshold nếu dùng.

- Stock/sellable pass.

- Sale Lock/Recall clear.

- Fulfillment/CSKH capacity pass.

- Payment/COD fail trong giới hạn.

- Refund/return trong giới hạn.

- Complaint/risk trong giới hạn.

- Privacy/claim incident không mở.

- Dashboard READY_FOR_DECISION.

- Owner approval có hiệu lực.

- Scale evidence accepted.

Không có owner approval thì không có Scale PASS.

## 77.6. Điều kiện PACK-07 Release Ready

Release Ready chỉ được gọi khi:

- Documentation complete.

- Implementation complete.

- Smoke pass.

- Data Quality Done Gate pass.

- Attribution Done Gate pass.

- Verified Revenue Done Gate pass.

- Dashboard Done Gate pass.

- Scale Gate pass nếu release dùng cho scale decision.

- Stop/Pause Gate pass.

- Evidence accepted.

- No P0 open.

- Security/privacy/claim review pass.

- Owner sign-off.

- Release decision approved.

## 78. PACK-07 FAIL GATE TỔNG THỂ

PACK-07 phải FAIL hoặc bị chặn nếu có bất kỳ điều kiện sau:

- Tự tính revenue từ comment/inbox/quote/cart.

- Tính ROAS từ revenue chưa verified.

- Tính source ROAS khi attribution conflict chưa xử lý.

- Gán Organic mặc định cho missing campaign data.

- Gán Diamond revenue khi thiếu referral bind.

- Gộp CRM revenue vào Ads revenue không có rule.

- Dùng raw Facebook event thiếu PACK-06 trace.

- Đo AI performance thiếu PACK-05 event.

- Gọi cart draft là official order.

- Gọi payment waiting là paid revenue.

- Gọi COD waiting là COD successful revenue.

- Không loại refund/cancel/return khỏi ROAS theo policy.

- Không chống trùng event.

- Không hiển thị data quality warning.

- Dashboard che waiting/excluded/missing/conflict data.

- Scale khi SKU hero not sellable.

- Scale khi Sale Lock/Recall active.

- Scale khi fulfillment quá tải.

- Scale khi privacy/claim incident mở.

- Scale khi owner chưa approve.

- Stop/Pause Gate không kích hoạt khi có P0 risk.

- Evidence thiếu, rejected hoặc expired.

- P0 issue còn mở.

- Release decision thiếu.

Fail Gate phải ưu tiên an toàn hệ thống hơn chỉ số đẹp.

## 79. PACK-07 HANDOFF CONTROL

## 79.1. Handoff sang triển khai

Khi chuyển PACK-07 sang triển khai, dev phải nhận rõ:

- PACK-07 không phải nơi tạo source-of-truth mới.

- PACK-07 chỉ consume event/checkpoint từ owner pack.

- PACK-07 không tự tính giá, chiết khấu, quyền lợi, commission.

- PACK-07 không tự xác nhận payment/COD/delivery.

- PACK-07 không sync MISA.

- PACK-07 không tự scale Ads.

- PACK-07 phải fail-closed khi thiếu data.

- PACK-07 phải có smoke/evidence trước khi PASS.

## 79.2. Handoff sang vận hành Marketing

Marketing team chỉ được dùng dashboard PACK-07 theo trạng thái.

Nếu dashboard là RAW hoặc waiting, chỉ dùng để quan sát.

Nếu dashboard là READY_FOR_DECISION, mới được dùng cho đề xuất scale/stop.

Nếu Scale Gate chưa PASS, không được tăng ngân sách production dựa trên cảm tính.

Nếu Stop Gate yêu cầu pause/stop, marketing phải xử lý theo owner policy.

## 79.3. Handoff sang Owner Review

Owner Review phải nhận được:

- Dashboard snapshot.

- Time window.

- Campaign/adset/ad scope.

- Revenue basis.

- Attribution model.

- Data quality status.

- ROAS/CPA/AOV.

- Risk status.

- Stock/sellable status.

- Fulfillment capacity.

- Scale/stop recommendation.

- Evidence.

- waiting issue.

- Decision options.

Owner không nên quyết định chỉ dựa trên một chỉ số ROAS đơn lẻ.

## 80. PACK-07 FINAL STATUS

Sau khi hoàn tất 4 phần, trạng thái của PACK-07 là:

## ADS MEASUREMENT / CHANNEL ATTRIBUTION / VERIFIED REVENUE / SCALE GATE / DATA QUALITY GOVERNANCE

Trạng thái tài liệu:

## GOVERNANCE_DOCUMENTATION_COMPLETE

Trạng thái triển khai:

waiting IMPLEMENTATION

Trạng thái kiểm thử:

waiting TEST / SMOKE

Trạng thái evidence:

waiting ACCEPTED EVIDENCE

Trạng thái owner:

waiting OWNER SIGN-OFF

Trạng thái release:

waiting RELEASE DECISION

Trạng thái production:

## NOT PRODUCTION READY

Trạng thái scale:

## NOT SCALE APPROVED UNTIL SCALE GATE PASS + OWNER APPROVAL

Trạng thái ROAS:

## NOT ROAS PASS UNTIL VERIFIED REVENUE + ATTRIBUTION + DATA QUALITY + EVIDENCE PASS

## KẾT LUẬN CUỐI PACK-07

PACK-07 là pack quản trị đo lường Ads, ROAS, Attribution, Verified Revenue, Data Quality và Scale/Stop Decision cho hệ thống Ginsengfood.

PACK-07 không phải Facebook Gateway.

PACK-07 không phải AI Advisor.

PACK-07 không phải Commerce Runtime.

PACK-07 không phải Payment Core.

PACK-07 không phải MISA Integration.

PACK-07 không phải Finance Accounting.

PACK-07 không tự tạo đơn hàng.

PACK-07 không tự tạo doanh thu.

PACK-07 không tự xác nhận thanh toán.

PACK-07 không tự xác nhận COD thành công.

PACK-07 không tự xác nhận giao hàng.

PACK-07 không tự sync MISA.

PACK-07 không tự tính commission.

PACK-07 không tự scale Ads.

PACK-07 chỉ được đo lường khi có event hợp lệ, owner source hợp lệ, trace hợp lệ, attribution rule hợp lệ, verified revenue hợp lệ, data quality pass và evidence accepted.

Nguyên tắc khóa cuối cùng của PACK-07:

Comment không phải đơn hàng.

Inbox không phải doanh thu.

Quote không phải revenue.

Cart draft không phải official order.

Order chưa chắc là paid revenue.

Payment waiting chưa phải verified revenue.

Revenue waiting không được tính ROAS chính thức.

Attribution conflict không được gán nguồn cảm tính.

Missing data không được tự suy diễn.

ROAS đẹp nhưng dữ liệu chưa sạch thì không được scale.

Data Quality đi trước Dashboard.

Verified Revenue đi trước ROAS.

Attribution Rule đi trước source revenue.

Stock/Sellable/Sale Lock đi trước Scale.

Fulfillment capacity đi trước Scale.

Owner Approval đi trước Scale Production.

Evidence Accepted đi trước PASS.

PACK-07 hoàn tất về mặt governance/documentation, nhưng chưa được gọi là triển khai xong, chưa được gọi là ROAS PASS, chưa được gọi là Scale Approved, chưa được gọi là Release Approved và chưa được gọi là Production Ready nếu chưa có implementation, test, smoke, accepted evidence, owner sign-off và release decision.

## ĐỀ XUẤT TIẾP THEO

Bước tiếp theo nên chuyển sang PACK-08 — MC AI LIVE / LIVE SCRIPT / HOSTING ASSISTANT / REAL-TIME SALES ORCHESTRATION PACK, vì PACK-07 đã khóa xong lớp đo lường Ads/ROAS/Attribution và cần tiếp tục khóa lớp vận hành MC AI Live theo đúng ranh giới: MC AI không tự tạo giá, không tự chốt doanh thu, không vượt PACK-05, không vượt PACK-06, không vượt Commerce Runtime, và toàn bộ live performance phải đo được qua PACK-07.

## ADS MEASUREMENT / CHANNEL ATTRIBUTION / VERIFIED REVENUE / SCALE GATE / DATA QUALITY GOVERNANCE

## PHẦN 1/4 — ADS MEASUREMENT PRINCIPLES / SOURCE-OF-TRUTH BOUNDARY / PACK-06–PACK-05–COMMERCE DEPENDENCY

## 1. MỤC TIÊU CỦA PACK-07

PACK-07 thiết lập lớp quản trị đo lường quảng cáo, phân bổ nguồn doanh thu, kiểm soát ROAS, kiểm soát CPA, đánh giá chất lượng dữ liệu và quyết định scale/stop quảng cáo cho hệ thống Ginsengfood.

PACK-07 không tạo ra doanh thu.

PACK-07 không xác nhận đơn hàng.

PACK-07 không xác nhận thanh toán.

PACK-07 không xác nhận giao hàng.

PACK-07 không quyết định sản phẩm có được bán hay không.

PACK-07 không thay thế Commerce Runtime, Payment Core, Order Core, AI Advisor, Facebook Channel Gateway, CRM, Diamond Referral, MISA hoặc Finance.

PACK-07 chỉ đo lường dựa trên dữ liệu hợp lệ, có nguồn rõ, có trace, có rule attribution, có evidence và có kết quả kiểm tra chất lượng dữ liệu.

Mục tiêu cốt lõi của PACK-07 là trả lời các câu hỏi vận hành sau:

- Chi phí quảng cáo đến từ nguồn nào.

- Khách hàng đến từ campaign, ad set, ad, live, page, comment hay Messenger nào.

- Tương tác nào là paid, organic, CRM, referral, Diamond, live hay mixed-source.

- Comment nào chuyển được sang Messenger.

- Messenger nào đi đến tư vấn sản phẩm.

- Tư vấn nào tạo quote.

- Quote nào chuyển thành cart draft.

- Cart draft nào được khách xác nhận.

- Xác nhận nào tạo official order.

- Official order nào được thanh toán hoặc COD thành công.

- Đơn nào đạt trạng thái doanh thu verified theo policy.

- Doanh thu nào được tính ROAS.

- Doanh thu nào phải loại trừ do hủy, hoàn, đổi, COD fail, payment fail, recall, sale lock, complaint hoặc data issue.

- ROAS, CPA, AOV có đủ tin cậy để scale hay không.

- Khi nào phải pause, stop, giảm ngân sách hoặc yêu cầu owner review.

PACK-07 là lớp đo lường và quyết định vận hành dựa trên dữ liệu đã được kiểm soát, không phải lớp tạo nghiệp vụ.

## 2. TRẠNG THÁI TÀI LIỆU

Trạng thái của PACK-07 sau khi hoàn tất tài liệu:

GOVERNANCE_DOCUMENTATION_COMPLETE — waiting IMPLEMENTATION / TEST / SMOKE / DATA QUALITY EVIDENCE / OWNER SIGN-OFF / SCALE DECISION / RELEASE DECISION

Trạng thái này có nghĩa:

- Tài liệu governance/domain đã hoàn chỉnh.

- Ranh giới owner và source-of-truth đã được khóa.

- Nguyên tắc đo lường Ads/ROAS/Attribution/Verified Revenue đã được xác lập.

- Các điều kiện Data Quality, Scale Gate và Stop Gate đã được định nghĩa.

- Chưa được xem là triển khai xong.

- Chưa được xem là có dashboard production.

- Chưa được xem là ROAS PASS.

- Chưa được xem là đủ điều kiện scale.

- Chưa được gọi Production Ready.

- Chưa được gọi Release Approved nếu chưa có implementation, test, smoke, accepted evidence, owner sign-off và release decision.

## 3. VAI TRÒ CỦA PACK-07 TRONG HỆ THỐNG GINSENGFOOD

PACK-07 giữ vai trò:

Ads Measurement & Attribution Governance Owner

PACK-07 làm chủ các lớp sau:

- Ads measurement principles.

- Campaign/ad/adset identity governance.

- Paid/organic/live/CRM/referral attribution governance.

- Funnel measurement governance.

- ROAS/CPA/AOV measurement governance.

- Verified revenue measurement dependency.

- Data quality control before reporting.

- Scale Gate governance.

- Stop/Pause Gate governance.

- Dashboard reliability governance.

- Decision evidence governance.

PACK-07 không làm chủ:

- Facebook Page, Webhook, Messenger, Live routing.

- AI tư vấn, AI trả lời, AI chốt đơn.

- Product Master.

- SKU sellable status.

- Inventory.

- Sale Lock.

- QuoteSnapshot.

- Official Order.

- Payment confirmation.

- COD success.

- Delivery success.

- Invoice.

- MISA sync.

- Accounting revenue.

- Commission.

- Tax/Voucher.

- Customer membership benefit.

- Diamond benefit.

- CRM message eligibility.

- Recall/Stop-sale.

PACK-07 chỉ được đo lường từ sự kiện hợp lệ do các owner pack phát hành hoặc được owner phê duyệt.

## 4. NGUYÊN TẮC CỐT LÕI

## 4.1. Ads Measurement không phải Commerce

Ads có thể tạo traffic.

Ads có thể tạo comment.

Ads có thể tạo inbox.

Ads có thể tạo lead.

Ads có thể tạo quote request.

Ads không tự tạo đơn hàng chính thức.

Ads không tự tạo doanh thu.

Ads không tự xác nhận thanh toán.

Ads không tự xác nhận COD thành công.

Ads không tự xác nhận giao hàng thành công.

Ads không tự tạo verified revenue.

Do đó, PACK-07 không được tính doanh thu chỉ từ:

- Click.

- Landing view.

- Comment.

- Inbox.

- Messenger conversation.

- Khách nói “lấy 2 hộp”.

- Khách gửi thông tin nhận hàng.

- QuoteSnapshot.

- Cart draft.

- Checkout form chưa xác nhận.

- Ảnh chuyển khoản chưa đối chiếu.

- Đơn chưa có official order/order_code.

- Đơn chưa đạt payment/COD/delivery/verified revenue theo policy.

Một tương tác quảng cáo chỉ được đi vào doanh thu khi có linkage đầy đủ đến owner runtime xác nhận trạng thái doanh thu hợp lệ.

## 4.2. Không có Verified Revenue thì không có ROAS chính thức

ROAS chính thức không được tính bằng doanh thu ước đoán.

ROAS chính thức không được tính bằng doanh thu từ comment.

ROAS chính thức không được tính bằng tổng quote.

ROAS chính thức không được tính bằng cart draft.

ROAS chính thức không được tính bằng đơn khách nói miệng.

ROAS chính thức chỉ được tính khi doanh thu đã đạt trạng thái verified theo rule của owner pack.

PACK-07 được phép hiển thị nhiều lớp doanh thu, nhưng phải phân biệt rõ:

- Gross order value.

- Confirmed order value.

- Paid revenue.

- COD successful revenue.

- Delivered revenue.

- Verified revenue.

- Net revenue sau hủy/hoàn/đổi nếu policy yêu cầu.

- Revenue waiting verification.

- Revenue excluded from ROAS.

ROAS production chỉ được dùng Verified Revenue hoặc revenue class được owner policy phê duyệt rõ.

## 4.3. Data Quality phải đi trước ROAS

PACK-07 không được đưa ROAS đẹp ra dashboard nếu dữ liệu chưa sạch.

Trước khi tính ROAS chính thức, hệ thống phải kiểm tra tối thiểu:

- Ads spend source có hợp lệ không.

- Campaign/adset/ad mapping có rõ không.

- Event có đi qua nguồn được phê duyệt không.

- Channel event có trace từ PACK-06 không.

- AI event có trace từ PACK-05 không.

- Quote event có liên kết đến conversation không.

- Order event có official order/order_code không.

- Payment/COD/delivery/verified revenue có hợp lệ không.

- Refund/cancel/return đã được loại trừ chưa.

- Duplicate event đã được xử lý chưa.

- Bot/spam/suppressed event đã được loại chưa.

- Organic/Paid/CRM/Diamond/referral conflict đã được xử lý chưa.

- Missing data có được đánh dấu không.

- Privacy-bị chặn event có bị loại không.

- Attribution rule có được ghi nhận không.

- Dashboard có phân biệt raw/clean/verified/excluded/waiting/missing không.

Nếu Data Quality không đạt, PACK-07 phải hiển thị trạng thái:

## ROAS_NOT_RELIABLE — SCALE_BLOCKED — OWNER_REVIEW_REQUIRED

## 4.4. Attribution không được suy diễn cảm tính

PACK-07 không được kết luận doanh thu thuộc Ads nếu thiếu attribution rule.

PACK-07 không được kết luận doanh thu thuộc Diamond nếu thiếu referral bind.

PACK-07 không được kết luận doanh thu thuộc CRM nếu thiếu CRM event linkage.

PACK-07 không được kết luận doanh thu thuộc Organic nếu sự kiện có paid context chưa xử lý.

PACK-07 không được tính trùng một đơn cho nhiều nguồn doanh thu.

Các tình huống bắt buộc có conflict resolution:

- Ads click trước, Diamond link sau.

- Diamond link trước, Ads remarketing sau.

- Live comment từ post được boost.

- Organic comment trong bài đang chạy ads.

- CRM nhắc mua lại cho khách từng đến từ ads.

- Khách cũ mua lại sau khi thấy ads.

- Khách tự gõ Messenger không có ad context.

- Khách vào từ landing page rồi quay lại Messenger.

- Khách vào từ link người giới thiệu rồi comment trên live.

- Khách nhiều lần tương tác qua nhiều campaign.

Không có attribution rule thì không được tính ROAS chính thức theo nguồn.

## 4.5. Ads Performance và AI Performance phải tách riêng

PACK-07 phải phân biệt hiệu quả quảng cáo và hiệu quả AI tư vấn.

Ads Performance đo:

- Spend.

- Impression.

- Reach.

- Click.

- CTR.

- CPC.

- CPM.

- Comment rate.

- Inbox rate.

- Lead rate.

- Cost per qualified conversation.

- CPA.

- ROAS.

- Campaign/adset/ad creative performance.

AI Advisor Performance đo:

- Intent recognition.

- Response quality.

- Product recommendation accuracy.

- Quote request handling.

- Quote generation rate.

- Cart/order capture rate.

- Handoff rate.

- Guard block count.

- Complaint detection.

- CRM response rate.

- Conversion from AI consult.

- Fail-closed count.

Không đổ lỗi cho Ads nếu AI tư vấn sai.

Không đổ lỗi cho AI nếu Ads đưa traffic kém.

Không đổ lỗi cho Facebook Gateway nếu Commerce chặn sellable.

Không đổ lỗi cho Commerce nếu channel event thiếu trace.

Không đổ lỗi cho dashboard nếu upstream chưa gửi verified revenue.

## 5. SOURCE-OF-TRUTH BOUNDARY

## 5.1. Bảng ranh giới source-of-truth

Lớp nghiệp vụ | Source-of-truth owner | PACK-07 được làm gì | PACK-07 không được làm gì

Facebook Page / Webhook / Messenger / Live | PACK-06 | Consume channel event đã kiểm soát | Không tự đọc raw event để kết luận production

AI tư vấn / intent / response / quote request | PACK-05 | Consume AI event hợp lệ | Không viết lại logic AI

Product / SKU / Recipe / Formula | Product Master / Operational Core | Consume product identity public/measurement field | Không sửa product master, không lộ BOM

Sellable / Stock / Sale Lock | Operational/Commerce owner | Consume sellable checkpoint | Không bán vượt sale lock

QuoteSnapshot | Commerce Runtime | Consume quote event | Không tự tính giá/khuyến mãi

Official Order | Order/Commerce owner | Consume order event | Không tự tạo đơn

Payment / COD | Payment/Order owner | Consume payment/COD event | Không tự xác nhận paid/COD success

Delivery | Fulfillment/Shipping owner | Consume delivery event | Không tự xác nhận giao hàng

Verified Revenue | Commerce/Finance owner | Consume verified revenue event | Không tự gọi doanh thu chính thức

MISA / Accounting | PACK-04 / Finance | Consume accounting checkpoint nếu được phép | Không sync MISA, không tạo chứng từ

CRM | CRM/AI/Commerce owner tùy rule | Consume CRM event | Không tự gửi CRM

Diamond Referral | Referral/Commerce owner | Consume referral bind/event | Không tự tính hoa hồng

Ads Spend | Approved Ads source | Consume spend record hợp lệ | Không tự bịa spend

Scale Decision | Owner decision + PACK-07 gate | Đề xuất scale/stop theo gate | Không tự scale nếu thiếu approval

## 5.2. Nguyên tắc không tạo parallel truth

PACK-07 không được tạo bảng sự thật song song cho:

- Doanh thu.

- Đơn hàng.

- Thanh toán.

- Giao hàng.

- Tồn kho.

- Sellable.

- Giá.

- Khuyến mãi.

- Member benefit.

- Diamond benefit.

- Commission.

- MISA accounting.

- Official customer status.

- Complaint resolution.

- Recall/sale lock.

PACK-07 chỉ được tạo lớp measurement projection, dashboard projection hoặc attribution analysis từ event hợp lệ.

Measurement projection không phải business truth.

Dashboard metric không được ghi ngược làm nghiệp vụ gốc.

ROAS metric không được sửa order.

CPA metric không được sửa payment.

Attribution result không được sửa official revenue nếu owner pack chưa cho phép.

## 6. PHỤ THUỘC BẮT BUỘC VỚI PACK-06

## 6.1. PACK-06 là Channel Event Truth cho Facebook/Messenger/Live

Đối với Facebook, Messenger và Live, PACK-07 phải dùng PACK-06 làm nguồn channel event đã được kiểm soát.

PACK-07 không được dùng trực tiếp Facebook raw event thiếu guard để báo cáo production nếu event đó chưa qua các bước:

- Page Registry validation.

- App/Permission validation.

- Webhook verification.

- Deduplication.

- Channel identity resolution.

- Public/private routing classification.

- Comment -> Messenger trace.

- Live session context.

- Spam/rate-limit suppression check.

- Privacy guard.

- Human takeover flag nếu có.

- Channel evidence.

Event không qua PACK-06 phải được đánh dấu:

## UNCONTROLLED_CHANNEL_EVENT — NOT_FOR_PRODUCTION_ROAS

## 6.2. Channel context bắt buộc

PACK-07 cần tối thiểu các nhóm context sau từ PACK-06:

- Page context.

- Post context.

- Comment context.

- Messenger thread context.

- Live session context.

- Public-to-private routing context.

- Handoff context.

- Human takeover context.

- Spam/rate-limit status.

- Privacy guard status.

- Channel error status.

- Evidence/correlation context.

Không có channel context thì không được kết luận nguồn tương tác.

Không có comment-to-Messenger trace thì không được gán Messenger order về comment/live/ad một cách chính thức.

Không có Page Registry thì không được đo production performance theo Page.

Không có Live session identity thì không được gán doanh thu về phiên live.

## 6.3. Public comment không phải order signal đầy đủ

PACK-07 có thể đo comment là funnel event.

PACK-07 không được gọi comment là order.

PACK-07 không được gọi live comment “chốt” là doanh thu.

PACK-07 không được gọi public reply là tư vấn hoàn chỉnh nếu policy yêu cầu private.

Comment chỉ là một lớp trong funnel:

Comment -> Qualified Comment -> Private Handoff -> Messenger Conversation -> Product Consult -> Quote -> Cart Draft -> Customer Confirmation -> Official Order -> Payment/COD/Delivery -> Verified Revenue

Comment không được nhảy thẳng thành revenue.

## 7. PHỤ THUỘC BẮT BUỘC VỚI PACK-05

## 7.1. PACK-05 là AI Advisor Runtime Owner

PACK-07 được đo lường hiệu quả AI, nhưng không được viết lại hoặc override AI Advisor.

PACK-07 chỉ consume AI event hợp lệ từ PACK-05:

- Intent event.

- Entity recognition event.

- Product recommendation event.

- Guard block event.

- Response event.

- Quote request event.

- Cart/order capture event.

- Complaint detection event.

- Handoff event.

- CRM event.

- Fail-closed event.

PACK-07 không được tự quyết định AI trả lời đúng hay sai nếu thiếu test/evidence rule.

PACK-07 có thể đánh dấu metric để owner review, nhưng không được tự sửa behavior production của AI.

## 7.2. Handoff theo policy không phải AI failure

Một số tình huống bắt buộc phải handoff:

- Khi cần xác nhận riêng tư.

- Khi có dữ liệu cá nhân.

- Khi có đơn hàng/hóa đơn/thanh toán.

- Khi có complaint/rủi ro chất lượng.

- Khi có câu hỏi vượt claim/safety.

- Khi có yêu cầu pháp lý.

- Khi có kênh public nhưng nội dung cần private.

- Khi AI không đủ runtime để trả lời an toàn.

Trong các trường hợp này, handoff đúng policy không được tính là AI failure.

PACK-07 phải phân biệt:

- Policy-required handoff.

- Risk handoff.

- Human takeover.

- AI failure handoff.

- Data-missing handoff.

- Channel-required private handoff.

Nếu không phân biệt handoff, dashboard sẽ đánh giá sai hiệu quả AI.

## 7.3. Complaint không tính là sales conversion

Complaint, quality issue, refund issue, delivery issue, invoice issue hoặc payment dispute không được tính là sales conversion.

Nếu khách đang trong complaint flow, PACK-07 phải đánh dấu conversion bị chặn theo rule.

Không được tính upsell từ khách đang có complaint mở nếu CRM/AI policy đã block.

Không được dùng complaint conversation để nâng tỷ lệ tư vấn bán hàng.

Complaint event cần đi vào risk metric, không đi vào sales performance như một lead bình thường.

## 8. PHỤ THUỘC BẮT BUỘC VỚI COMMERCE RUNTIME

## 8.1. Commerce Runtime là nguồn Quote/Order/Revenue hợp lệ

PACK-07 không được tự tính:

- Listed price.

- Program discount.

- Member discount.

- Diamond benefit.

- Final total.

- Shipping fee.

- COD fee.

- VAT display.

- Quote expiry.

- Order total.

- Refund amount.

- Revenue amount.

- Commission.

Các giá trị này phải đến từ owner runtime.

PACK-07 chỉ được đo lường khi có event/record hợp lệ từ Commerce/Order/Payment/Revenue owner.

## 8.2. QuoteSnapshot không phải revenue

QuoteSnapshot là báo giá có kiểm soát.

QuoteSnapshot không phải đơn hàng.

QuoteSnapshot không phải thanh toán.

QuoteSnapshot không phải doanh thu.

QuoteSnapshot chỉ có thể là một funnel event.

PACK-07 được đo:

- Quote request rate.

- Quote generation rate.

- Quote-to-confirmation rate.

- Quote-to-order rate.

- Quote-to-verified-revenue rate.

- Quote expiry rate.

PACK-07 không được tính tổng QuoteSnapshot thành doanh thu.

## 8.3. Official Order là điều kiện tối thiểu để đo order

Không có official order/order_code thì không được gọi là order chính thức.

Các trạng thái chưa đủ để gọi order:

- Khách comment “lấy 2 hộp”.

- Khách inbox “chốt”.

- Khách gửi tên/số điện thoại/địa chỉ.

- AI tạo cart draft.

- AI gửi form xác nhận.

- Khách gửi ảnh chuyển khoản.

- Nhân viên ghi chú ngoài hệ thống.

PACK-07 chỉ ghi nhận order khi owner runtime phát hành official order event hợp lệ.

## 8.4. Payment/COD/Delivery/Verified Revenue phải theo owner policy

Một đơn hàng có thể đi qua nhiều lớp:

- Official order created.

- Customer confirmed.

- Payment waiting.

- Payment confirmed.

- COD waiting.

- COD success.

- Delivery waiting.

- Delivery success.

- Return/refund/cancel.

- Order Verified.

- Revenue verified.

- Revenue excluded.

PACK-07 phải dùng đúng revenue class.

Không được gọi order created là paid revenue.

Không được gọi payment waiting là verified revenue.

Không được gọi delivery waiting là delivered revenue.

Không được gọi COD waiting là COD successful revenue.

Không được gọi revenue waiting verification là ROAS revenue chính thức.

## 9. PHỤ THUỘC BẮT BUỘC VỚI PACK-04 / MISA / FINANCE

## 9.1. PACK-07 không sync MISA

PACK-07 không được:

- Tạo chứng từ kế toán.

- Gửi chứng từ sang MISA.

- Sửa mapping MISA.

- Tự đối chiếu kế toán bằng dữ liệu không hợp lệ.

- Ghi ngược doanh thu vào MISA.

- Gọi một doanh thu là doanh thu kế toán nếu thiếu checkpoint.

Nếu cần số liệu kế toán hoặc đối chiếu MISA, PACK-07 chỉ được consume checkpoint/evidence hợp lệ từ PACK-04 hoặc Finance owner.

## 9.2. ROAS revenue không mặc định là accounting revenue

Doanh thu dùng cho ROAS và doanh thu kế toán có thể khác nhau về thời điểm ghi nhận.

PACK-07 phải phân biệt:

- Marketing verified revenue.

- Commerce verified revenue.

- Payment confirmed revenue.

- COD successful revenue.

- Delivered revenue.

- Accounting posted revenue.

- Reconciled accounting revenue.

- Net revenue after adjustment.

Không được dùng từ “doanh thu chính thức” nếu không chỉ rõ class.

Nếu dashboard dùng verified revenue cho ROAS, phải hiển thị rõ:

Revenue Basis = VERIFIED_REVENUE_FOR_MARKETING_MEASUREMENT

Nếu dùng kế toán đã post, phải hiển thị rõ:

Revenue Basis = ACCOUNTING_RECONCILED_REVENUE

## 10. PHÂN TÁCH ADS / AI / COMMERCE / CRM / REFERRAL

## 10.1. Ads Performance

Ads Performance là hiệu quả tạo traffic, tạo tương tác và tạo cơ hội bán hàng từ ngân sách quảng cáo.

Các chỉ số thuộc Ads Performance:

- Spend.

- Impression.

- Reach.

- CPM.

- Click.

- CTR.

- CPC.

- Landing view.

- Comment rate.

- Inbox rate.

- Cost per comment.

- Cost per qualified comment.

- Cost per Messenger conversation.

- Cost per qualified conversation.

- Cost per quote request.

- Cost per official order.

- Cost per verified order.

- ROAS theo verified revenue.

- Creative performance.

- Campaign/adset/ad performance.

Ads Performance không tự chứng minh AI tốt.

Ads Performance không tự chứng minh doanh thu tốt nếu revenue chưa verified.

## 10.2. AI Advisor Performance

AI Advisor Performance là hiệu quả tư vấn, điều hướng nhu cầu, đề xuất sản phẩm, quote/order handoff và xử lý tình huống khách hàng.

Các chỉ số thuộc AI Performance:

- Intent recognition rate.

- Correct product recommendation rate.

- Claim guard pass/fail.

- Quote request handling.

- Quote generation support.

- Order capture completion.

- Handoff correctness.

- Complaint detection.

- Guard block correctness.

- CRM response quality.

- Conversion after AI consult.

- AI-to-human escalation quality.

AI Performance phải tách khỏi Ads Performance để xác định đúng nguyên nhân tăng/giảm doanh thu.

## 10.3. Commerce Performance

Commerce Performance phản ánh việc quote, giá, order, payment, delivery, stock, sellable và verified revenue có vận hành đúng hay không.

Các vấn đề Commerce có thể ảnh hưởng ROAS:

- SKU hero hết hàng.

- SKU bị sale lock.

- Quote không tạo được.

- Giá runtime lỗi.

- Program không active.

- Member benefit không resolve được.

- Order không tạo được.

- Payment confirmation chậm.

- COD fail cao.

- Delivery fail cao.

- Refund/return cao.

- Revenue verification chậm.

PACK-07 phải đánh dấu nguyên nhân đến từ Commerce, không gán sai cho Ads hoặc AI.

## 10.4. CRM Performance

CRM Performance phản ánh doanh thu từ chăm sóc lại, mua lại, duy trì thành viên, nâng hạng, quà tặng, seasonal suggestion hoặc after-sales.

CRM revenue không được tính trùng với Ads revenue nếu attribution policy không cho phép.

Một khách từng đến từ Ads nhưng mua lại sau CRM không mặc định là Ads revenue.

Một khách thấy remarketing Ads sau CRM phải có attribution rule rõ.

CRM rebuy revenue phải có classification riêng nếu dùng trong dashboard.

## 10.5. Diamond / Referral Performance

Diamond/referral revenue phải dựa trên referral bind hợp lệ.

Không có referral bind thì không được gọi là Diamond revenue.

PACK-07 không tự tính hoa hồng.

PACK-07 chỉ đo:

- Referral entry.

- Referral bind.

- Referral conflict.

- Referral-attributed order.

- Referral-attributed verified revenue.

- Commission eligibility event nếu owner pack phát hành.

- Commission status nếu Finance/Commerce owner cung cấp.

Revenue từ Diamond/referral phải tách khỏi Ads revenue nếu policy yêu cầu.

## 11. ATTRIBUTION GOVERNANCE PRINCIPLES

## 11.1. Attribution là rule-governed, không phải cảm tính

Attribution phải dựa trên rule được owner phê duyệt.

PACK-07 không được dùng cách nhìn thủ công để kết luận “đơn này do ads”.

Mỗi conversion cần có:

- Customer identity hoặc session identity hợp lệ.

- Channel identity.

- Campaign/ad/adset identity nếu có.

- Entry event.

- Touchpoint chain.

- Quote/order/revenue linkage.

- Attribution model used.

- Conflict resolution nếu có.

- Evidence.

- Data quality result.

## 11.2. Attribution model phải khai báo rõ

PACK-07 có thể hỗ trợ nhiều mô hình attribution, nhưng mỗi báo cáo phải nêu rõ model đang dùng.

Các mô hình có thể có:

- First touch.

- Last touch.

- Last paid touch.

- Last non-direct touch.

- Session-based attribution.

- Campaign-bound attribution.

- Live-session attribution.

- Diamond referral priority attribution.

- CRM rebuy attribution.

- Owner-approved custom attribution.

Không được trộn nhiều model trong cùng một chỉ số mà không khai báo.

## 11.3. Attribution conflict phải được đánh dấu

Khi một order có nhiều nguồn nhận công, PACK-07 phải đánh dấu conflict.

Ví dụ conflict:

- Paid Ads + Organic Comment.

- Paid Ads + Live Session.

- Paid Ads + Diamond Referral.

- Diamond Referral + CRM.

- CRM + Remarketing Ads.

- Organic Messenger + Ads Click cũ.

- Member rebuy + Ads view.

- MC AI Live + Facebook Ads.

- Landing Page + Messenger.

- Human telesales + AI Advisor.

Conflict chưa xử lý không được đưa vào ROAS chính thức.

Trạng thái bắt buộc:

## ATTRIBUTION_CONFLICT — OWNER_RULE_REQUIRED

hoặc

## ATTRIBUTION_CONFLICT_RESOLVED — RULE_APPLIED

## 12. DATA QUALITY PRINCIPLES

## 12.1. Các lớp dữ liệu phải phân biệt rõ

PACK-07 dashboard phải phân biệt:

- Raw data.

- Normalized data.

- Clean data.

- Verified data.

- Excluded data.

- waiting data.

- Missing data.

- Duplicated data.

- Conflict data.

- Suppressed data.

- Privacy-bị chặn data.

- Owner-review data.

Không được gom tất cả thành số tổng.

Không được ẩn data issue.

Không được hiển thị ROAS như PASS nếu dữ liệu chưa đạt.

## 12.2. Data Quality status

Mỗi tập dữ liệu đo lường phải có trạng thái chất lượng.

Các trạng thái tối thiểu:

- DATA_RAW.

- DATA_NORMALIZED.

- DATA_CLEAN_waiting.

- DATA_CLEAN_PASS.

- DATA_QUALITY_WARNING.

- DATA_QUALITY_FAIL.

- DATA_MISSING_REQUIRED_FIELD.

- DATA_DUPLICATE_DETECTED.

- DATA_PRIVACY_BLOCKED.

- DATA_SPAM_SUPPRESSED.

- DATA_ATTRIBUTION_CONFLICT.

- DATA_REVENUE_NOT_VERIFIED.

- DATA_OWNER_REVIEW_REQUIRED.

Khi data quality chưa PASS, dashboard chỉ được hiển thị dạng cảnh báo, không được dùng để scale chính thức.

## 12.3. Missing data không được im lặng

Nếu thiếu dữ liệu, PACK-07 phải hiển thị rõ thiếu gì.

Ví dụ:

- Thiếu campaign id.

- Thiếu adset id.

- Thiếu ad id.

- Thiếu Page context.

- Thiếu comment-to-Messenger trace.

- Thiếu quote id.

- Thiếu order code.

- Thiếu payment status.

- Thiếu delivery status.

- Thiếu verified revenue event.

- Thiếu attribution rule.

- Thiếu owner approval.

Missing data không được tự lấp bằng suy luận.

Missing data không được chuyển thành organic mặc định.

Missing data không được chuyển thành Ads mặc định.

## 13. ADS SPEND SOURCE GOVERNANCE

## 13.1. Ads spend phải có nguồn hợp lệ

PACK-07 chỉ được đo ROAS nếu Ads spend đến từ nguồn hợp lệ.

Ads spend source phải có:

- Nguồn dữ liệu được owner phê duyệt.

- Campaign identity.

- Adset identity.

- Ad identity nếu áp dụng.

- Time window.

- Currency.

- Spend amount.

- Account/page/business context.

- Import/sync status.

- Evidence snapshot.

- Data quality status.

Không có spend source hợp lệ thì không được tính ROAS.

## 13.2. Spend không được tự nhập tay tùy tiện

Nếu có cơ chế nhập tay spend tạm thời, phải có:

- Owner approval.

- Evidence đính kèm.

- Người nhập.

- Lý do nhập.

- Ngày giờ nhập.

- Phạm vi áp dụng.

- Reconcile sau đó.

- Cảnh báo dashboard.

Spend nhập tay chưa reconcile không được dùng cho ROAS PASS nếu owner policy không cho phép.

## 14. VERIFIED REVENUE DEPENDENCY

## 14.1. Revenue phải đi từ owner event

PACK-07 chỉ consume revenue từ owner runtime.

Các nguồn không hợp lệ để tự tính revenue:

- Comment public.

- Messenger text.

- QuoteSnapshot.

- Cart draft.

- Excel ngoài hệ thống.

- Nhân viên báo miệng.

- Ảnh chuyển khoản chưa đối chiếu.

- Mã vận đơn chưa giao thành công.

- MISA record không có link về order nếu policy yêu cầu.

- Dashboard Ads tự nhập doanh thu.

Revenue phải có trace đến official order và trạng thái verification hợp lệ.

## 14.2. Revenue exclusion bắt buộc

Các trường hợp phải loại khỏi ROAS revenue nếu policy quy định:

- Order canceled.

- Payment failed.

- COD failed.

- Delivery failed.

- Returned order.

- Refunded order.

- Duplicate order.

- Test order.

- Internal staff order nếu không tính.

- Sample/gift order nếu không tính.

- Complaint unresolved.

- Recall/sale lock impacted order.

- Fraud/spam order.

- Attribution conflict unresolved.

- Revenue waiting verification.

Không có exclusion rule thì không được báo ROAS chính thức.

## 15. SALE LOCK / STOCK / FULFILLMENT DEPENDENCY

PACK-07 không được scale quảng cáo nếu sản phẩm hero đang bị chặn bởi:

- Out of stock.

- Not sellable.

- Sale Lock.

- Recall.

- Quality hold.

- Warehouse hold.

- Channel hold.

- Program inactive.

- Price inactive.

- Quote unavailable.

- Fulfillment overload.

- Payment/COD failure vượt ngưỡng.

- Delivery delay vượt ngưỡng.

- Complaint tăng bất thường.

PACK-07 phải consume trạng thái từ owner pack.

Nếu Ads đang tốt nhưng stock/sellable bị chặn, Scale Gate phải block.

Nếu Sale Lock đang ảnh hưởng SKU hero, Stop Gate phải kích hoạt theo policy.

## 16. OWNER DECISION PRINCIPLES

## 16.1. PACK-07 đề xuất, owner quyết định

PACK-07 có thể đưa khuyến nghị:

- Scale allowed.

- Scale bị chặn.

- Pause recommended.

- Stop required.

- Owner review required.

- Data quality fix required.

- Attribution rule required.

- Revenue verification waiting.

- Fulfillment constraint warning.

- Stock/sellable constraint warning.

PACK-07 không tự tăng ngân sách nếu chưa có owner approval hoặc rule automation được owner phê duyệt.

PACK-07 không tự tắt campaign production nếu chưa có Stop Gate policy hoặc owner decision.

## 16.2. Scale không được dựa trên cảm tính

Không được scale vì:

- Nhiều comment.

- Nhiều inbox.

- Live đông.

- Cảm giác khách thích.

- ROAS tạm tính đẹp nhưng revenue chưa verified.

- Một vài đơn lớn chưa đối chiếu.

- AI báo khách quan tâm nhiều.

- Dashboard raw data tăng nhưng data quality chưa pass.

Scale chỉ được xét khi Data Quality, Attribution, Verified Revenue, CPA, ROAS, fulfillment, stock/sellable và owner approval đạt điều kiện.

## 17. QUY TẮC FAIL-CLOSED CỦA PACK-07

PACK-07 phải fail-closed trong các trường hợp:

- Thiếu Ads spend source.

- Thiếu campaign/adset/ad identity.

- Thiếu channel trace.

- Thiếu comment-to-Messenger trace.

- Thiếu AI event khi đo AI performance.

- Thiếu quote/order linkage.

- Thiếu official order.

- Thiếu verified revenue.

- Revenue waiting verification.

- Attribution conflict chưa xử lý.

- Duplicate event chưa xử lý.

- Spam/bot event chưa loại.

- Refund/cancel/return chưa loại.

- Sale Lock ảnh hưởng SKU.

- Privacy incident.

- Claim incident.

- Dashboard data mismatch.

- Owner approval missing.

Fail-closed nghĩa là:

- Không tính ROAS chính thức.

- Không cho Scale PASS.

- Không gọi số liệu là verified.

- Không ẩn cảnh báo.

- Chuyển owner review nếu cần.

## 18. MINIMUM GOVERNANCE OUTPUT CỦA PACK-07

PACK-07 phải tạo được các lớp output governance sau:

- Ads Measurement Status.

- Attribution Status.

- Funnel Data Quality Status.

- Revenue Verification Status.

- ROAS Reliability Status.

- CPA Reliability Status.

- Dashboard Reliability Status.

- Scale Gate Status.

- Stop/Pause Gate Status.

- Owner Decision Status.

- Evidence Status.

- Release Readiness Status.

Các output này không thay thế nghiệp vụ gốc.

Các output này dùng để kiểm soát đo lường, quyết định ads và cảnh báo vận hành.

## 19. DONE CONDITION CỦA PHẦN 1/4

## PHẦN 1/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Mục tiêu PACK-07.

- Trạng thái tài liệu.

- Vai trò PACK-07 trong hệ thống.

- Ranh giới Ads Measurement không phải Commerce.

- Không có Verified Revenue thì không có ROAS chính thức.

- Data Quality đi trước ROAS.

- Attribution không suy diễn cảm tính.

- Ads Performance tách khỏi AI Performance.

- Source-of-truth boundary.

- Phụ thuộc PACK-06.

- Phụ thuộc PACK-05.

- Phụ thuộc Commerce Runtime.

- Phụ thuộc PACK-04/MISA/Finance.

- Phân tách Ads/AI/Commerce/CRM/Referral.

- Attribution governance principles.

- Data quality principles.

- Ads spend source governance.

- Verified revenue dependency.

- Sale Lock/Stock/Fulfillment dependency.

- Owner decision principles.

- Fail-closed rule.

## PHẦN 1/4 chưa định nghĩa đầy đủ Event Taxonomy, Funnel Metrics, Attribution Model chi tiết, Dashboard KPI, Scale Gate, Stop Gate, Smoke Matrix hoặc Release Control. Các nội dung đó thuộc PHẦN 2/4, PHẦN 3/4 và PHẦN 4/4.

## KẾT LUẬN PHẦN 1/4

PACK-07 là lớp đo lường Ads/ROAS/Attribution/Verified Revenue của hệ thống Ginsengfood.

PACK-07 không phải lớp tạo doanh thu.

PACK-07 không phải lớp chốt đơn.

PACK-07 không phải lớp xác nhận thanh toán.

PACK-07 không phải lớp kế toán.

PACK-07 không phải lớp AI tư vấn.

PACK-07 không phải Facebook Channel Gateway.

PACK-07 chỉ được đo lường từ dữ liệu hợp lệ, có trace, có owner source-of-truth, có attribution rule, có data quality pass và có evidence.

Nguyên tắc cốt lõi của PACK-07 là:

Không có Data Quality thì không có ROAS đáng tin cậy.

Không có Verified Revenue thì không có ROAS chính thức.

Không có Attribution Rule thì không có kết luận nguồn doanh thu.

Không có Owner Approval thì không scale production.

Không có Evidence thì không PASS.

## PHẦN 2/4 — EVENT TAXONOMY / ATTRIBUTION MODEL / FUNNEL METRICS / VERIFIED REVENUE RULES

## 20. MỤC TIÊU CỦA PHẦN 2/4

## PHẦN 2/4 thiết lập hệ quy chiếu đo lường sự kiện, phễu chuyển đổi, phân bổ nguồn doanh thu và quy tắc xác nhận doanh thu được dùng cho PACK-07.

Mục tiêu của phần này là khóa rõ:

- Event nào được phép dùng để đo Ads.

- Event nào chỉ là tín hiệu tương tác.

- Event nào là tín hiệu tư vấn.

- Event nào là tín hiệu quote.

- Event nào là tín hiệu order.

- Event nào là tín hiệu payment.

- Event nào đủ điều kiện đi vào verified revenue.

- Event nào phải loại khỏi ROAS.

- Event nào cần owner review.

- Event nào có conflict attribution.

- Event nào không đủ data quality để đưa vào dashboard chính thức.

- Event nào có thể dùng cho scale decision.

## PHẦN 2/4 không thiết kế database, API, UI hay code.

## PHẦN 2/4 chỉ khóa logic governance/domain để dev triển khai đúng theo source-of-truth và owner boundary.

## 21. NGUYÊN TẮC EVENT TAXONOMY

## 21.1. Event là dấu vết đo lường, không mặc định là sự thật nghiệp vụ

Một event trong PACK-07 là dấu vết đo lường.

Event không mặc định là đơn hàng.

Event không mặc định là doanh thu.

Event không mặc định là thanh toán.

Event không mặc định là giao hàng thành công.

Event không mặc định là ROAS hợp lệ.

Một event chỉ được dùng để kết luận nghiệp vụ khi event đó đến từ owner pack hoặc được owner pack xác nhận.

Ví dụ:

- COMMENT_EVENT chỉ chứng minh có comment.

- MESSENGER_EVENT chỉ chứng minh có tương tác Messenger.

- QUOTE_EVENT chỉ chứng minh có quote.

- CART_EVENT chỉ chứng minh có cart draft hoặc cart action.

- ORDER_EVENT chỉ chứng minh có order khi đến từ owner Order/Commerce.

- PAYMENT_EVENT chỉ chứng minh payment khi đến từ owner Payment.

- VERIFIED_REVENUE_EVENT mới là cơ sở doanh thu verified nếu policy xác nhận.

- ATTRIBUTION_EVENT chỉ có ý nghĩa khi có rule attribution hợp lệ.

- DATA_QUALITY_EVENT quyết định dữ liệu có đủ điều kiện dùng hay không.

Không được dùng event ở tầng thấp để suy diễn kết quả ở tầng cao.

## 21.2. Event phải có nguồn, chủ sở hữu và phạm vi sử dụng

Mỗi event dùng trong PACK-07 phải xác định tối thiểu:

- Event family.

- Event owner.

- Event source.

- Event timestamp.

- Event scope.

- Event correlation.

- Event identity.

- Event data quality status.

- Event evidence.

- Event allowed usage.

Nếu thiếu owner hoặc source hợp lệ, event không được dùng để tính ROAS production.

Nếu thiếu correlation, event không được dùng để nối phễu conversion chính thức.

Nếu thiếu evidence, event không được dùng để PASS.

## 21.3. Event phải được chống trùng

PACK-07 không được tính lặp event.

Các tình huống trùng phải xử lý:

- Một comment được gửi webhook nhiều lần.

- Một Messenger event được retry nhiều lần.

- Một khách gửi nhiều tin giống nhau.

- Một quote được tạo lại do hết hạn.

- Một cart draft được sửa nhiều lần.

- Một order có nhiều trạng thái payment.

- Một order có nhiều trạng thái delivery.

- Một revenue event được cập nhật nhiều lần.

- Một campaign spend được import nhiều lần.

- Một khách xuất hiện ở nhiều touchpoint.

Nếu chưa chống trùng, metric phải ở trạng thái:

## DATA_DUPLICATE_DETECTED — ROAS_NOT_RELIABLE

## 22. EVENT TAXONOMY REGISTRY

## 22.1. Danh mục event family tối thiểu

Event family | Vai trò đo lường | Owner/source bắt buộc | Không được dùng để

CHANNEL_EVENT | Ghi nhận sự kiện kênh tổng quát | PACK-06 hoặc nguồn channel được phê duyệt | Không tự tính order/revenue

AD_EVENT | Ghi nhận ads impression/click/spend/campaign context | Ads source được owner phê duyệt | Không tự tính doanh thu

COMMENT_EVENT | Ghi nhận comment public | PACK-06 | Không gọi là đơn hàng

MESSENGER_EVENT | Ghi nhận tương tác Messenger/private | PACK-06 | Không gọi là order nếu chưa có owner order

LIVE_EVENT | Ghi nhận phiên live/live comment/live routing | PACK-06 | Không gọi là revenue nếu chưa verified

AI_INTENT_EVENT | Ghi nhận intent AI nhận diện | PACK-05 | Không kết luận conversion

AI_RESPONSE_EVENT | Ghi nhận phản hồi AI/guard/handoff | PACK-05 | Không tự chứng minh doanh thu

QUOTE_EVENT | Ghi nhận quote request/QuoteSnapshot | Commerce Runtime/PACK-05 handoff | Không gọi là revenue

CART_EVENT | Ghi nhận cart draft/cart update | Commerce/Order owner | Không gọi là official order

ORDER_EVENT | Ghi nhận official order/order status | Order/Commerce owner | Không gọi là paid revenue nếu chưa payment

PAYMENT_EVENT | Ghi nhận payment/COD status | Payment/Order owner | Không gọi là verified revenue nếu chưa policy pass

DELIVERY_EVENT | Ghi nhận shipping/delivery status | Fulfillment/Shipping owner | Không gọi là verified revenue nếu thiếu rule

VERIFIED_REVENUE_EVENT | Ghi nhận doanh thu đủ điều kiện đo lường | Commerce/Finance/Revenue owner | Không tự tạo từ dashboard

CRM_EVENT | Ghi nhận CRM interaction/rebuy campaign | CRM/AI/Commerce owner | Không tự gán Ads revenue

COMPLAINT_EVENT | Ghi nhận complaint/quality/payment/delivery issue | AI/CSKH/Quality owner | Không tính sales conversion

HANDOFF_EVENT | Ghi nhận chuyển người thật/handoff private | PACK-05/PACK-06 | Không mặc định là AI failure

REFERRAL_EVENT | Ghi nhận Diamond/referral bind/touchpoint | Referral/Commerce owner | Không tự tính commission

ATTRIBUTION_EVENT | Ghi nhận kết quả phân bổ nguồn | PACK-07 theo owner-approved rule | Không sửa revenue gốc

DATA_QUALITY_EVENT | Ghi nhận chất lượng dữ liệu | PACK-07 | Không thay thế event gốc

SCALE_DECISION_EVENT | Ghi nhận quyết định scale/stop/pause | Owner decision + PACK-07 gate | Không tự scale nếu thiếu approval

## 22.2. Event family không được vượt quyền

Mỗi event family chỉ được dùng đúng phạm vi.

COMMENT_EVENT không được vượt lên thành ORDER_EVENT.

QUOTE_EVENT không được vượt lên thành VERIFIED_REVENUE_EVENT.

PAYMENT_EVENT waiting không được vượt lên thành VERIFIED_REVENUE_EVENT.

CRM_EVENT không được vượt lên thành Ads revenue nếu attribution chưa xử lý.

REFERRAL_EVENT không được vượt lên thành commission nếu owner Finance/Commerce chưa xác nhận eligibility.

ATTRIBUTION_EVENT không được sửa order/revenue gốc.

DATA_QUALITY_EVENT không được xóa event gốc, chỉ được đánh dấu dữ liệu có đủ điều kiện dùng hay không.

## 23. EVENT LIFECYCLE

## 23.1. Trạng thái vòng đời event

Mỗi event đo lường cần đi qua các trạng thái tối thiểu:

- RAW_RECEIVED.

- SOURCE_VALIDATED.

- NORMALIZED.

- DEDUP_CHECKED.

- IDENTITY_RESOLVED.

- CORRELATION_LINKED.

- DATA_QUALITY_CHECKED.

- ATTRIBUTION_CHECKED nếu cần.

- ELIGIBILITY_CHECKED.

- INCLUDED hoặc EXCLUDED.

- EVIDENCE_ATTACHED.

- READY_FOR_REPORTING nếu đạt.

- OWNER_REVIEW_REQUIRED nếu chưa đủ điều kiện.

Không phải event nào cũng cần đi đến reporting.

Một event bị fail ở bất kỳ chặng P0 nào phải được giữ lịch sử nhưng không được dùng cho ROAS chính thức.

## 23.2. Event bị loại không được xóa

Event bị loại khỏi ROAS không được xóa khỏi hệ thống đo lường.

Event bị loại phải có lý do loại trừ.

Các lý do loại trừ tối thiểu:

- DUPLICATE.

- SPAM.

- BOT_SUSPECTED.

- PRIVACY_BLOCKED.

- CHANNEL_ERROR.

- MISSING_CAMPAIGN_CONTEXT.

- MISSING_CHANNEL_TRACE.

- MISSING_QUOTE_LINKAGE.

- MISSING_ORDER_CODE.

- PAYMENT_NOT_CONFIRMED.

- DELIVERY_NOT_CONFIRMED nếu policy yêu cầu.

- REVENUE_NOT_VERIFIED.

- ORDER_CANCELLED.

- ORDER_REFUNDED.

- ORDER_RETURNED.

- TEST_ORDER.

- INTERNAL_ORDER.

- ATTRIBUTION_CONFLICT_UNRESOLVED.

- OWNER_POLICY_EXCLUDED.

- SALE_LOCK_IMPACTED.

- COMPLAINT_UNRESOLVED.

Loại trừ phải có evidence/audit.

## 24. FUNNEL MEASUREMENT PRINCIPLES

## 24.1. Funnel là chuỗi chuyển đổi, không phải một bước nhảy

PACK-07 phải đo funnel theo từng chặng.

Không được gọi comment là đơn hàng.

Không được gọi inbox là doanh thu.

Không được gọi quote là order.

Không được gọi cart draft là order chính thức.

Không được gọi order created là paid revenue.

Không được gọi paid waiting là verified revenue.

Chuỗi funnel chuẩn tối thiểu:

Impression -> Reach -> Click -> Landing View -> Comment -> Qualified Comment -> Private Handoff -> Messenger Conversation -> Qualified Conversation -> Product Consult -> Quote Request -> Quote Generated -> Cart Draft -> Customer Confirmation -> Official Order -> Payment/COD Confirmation -> Delivery Success nếu policy yêu cầu -> Order Verified -> Verified Revenue

## 24.2. Funnel stage registry

Funnel stage | Ý nghĩa | Điều kiện tối thiểu | Không được kết luận

Impression | Quảng cáo được hiển thị | Ads source hợp lệ | Không kết luận quan tâm

Reach | Người dùng được tiếp cận | Ads source hợp lệ | Không kết luận lead

Click | Có click | Campaign/ad context | Không kết luận khách chất lượng

Landing View | Có xem trang/đích | Landing/channel event hợp lệ | Không kết luận quote

Comment | Có comment | PACK-06 comment event | Không kết luận order

Qualified Comment | Comment có ý định đủ đo | Rule qualification | Không kết luận revenue

Private Handoff | Kéo/điều hướng private hợp lệ | PACK-06 routing trace | Không kết luận AI success nếu chưa consult

Messenger Conversation | Có hội thoại private | PACK-06 Messenger event | Không kết luận quote

Qualified Conversation | Hội thoại đủ chất lượng | Qualification rule | Không kết luận order

Product Consult | Có tư vấn sản phẩm | PACK-05 event | Không kết luận chốt

Quote Request | Khách có nhu cầu báo giá | PACK-05/Commerce event | Không kết luận revenue

Quote Generated | QuoteSnapshot được tạo | Commerce Runtime | Không kết luận order

Cart Draft | Giỏ/đơn nháp | Commerce/Order owner | Không kết luận official order

Customer Confirmation | Khách xác nhận | Order/Commerce owner | Không kết luận paid

Official Order | Đơn chính thức/order_code | Order owner | Không kết luận verified revenue

Payment/COD Confirmation | Thanh toán/COD đạt trạng thái policy | Payment/Order owner | Không kết luận net revenue nếu còn return/refund

Delivery Success | Giao thành công nếu policy yêu cầu | Fulfillment owner | Không kết luận kế toán nếu chưa reconcile

Order Verified | Đơn được xác minh theo policy | Commerce/Order owner | Không mặc định là accounting posted

Verified Revenue | Doanh thu đủ điều kiện đo ROAS | Revenue owner | Chỉ dùng theo revenue basis đã khai báo

## 25. COMMENT -> MESSENGER -> QUOTE -> ORDER -> VERIFIED REVENUE

## 25.1. Comment chỉ là điểm vào funnel

Comment public là tín hiệu tương tác.

Comment có thể là:

- Hỏi giá.

- Hỏi sản phẩm.

- Hỏi thành phần.

- Hỏi Sâm Savigin.

- Hỏi ship.

- Đòi tư vấn riêng.

- Muốn mua.

- Chốt mơ hồ.

- Spam.

- Khiếu nại.

- Trêu đùa.

- Không liên quan.

PACK-07 chỉ được nâng comment thành Qualified Comment khi rule qualification pass.

Comment “chốt”, “lấy 2”, “ib”, “giá sao”, “mua thế nào” không phải order.

Comment có nhu cầu riêng phải route private theo PACK-06/PACK-05 policy.

## 25.2. Messenger là môi trường tư vấn/order capture, không tự là order

Messenger conversation có thể chứa:

- Tư vấn sản phẩm.

- Hỏi giá.

- Hỏi chương trình.

- Hỏi thành viên.

- Hỏi ship.

- Cung cấp thông tin nhận hàng.

- Xác nhận quote.

- Gửi ảnh chuyển khoản.

- Khiếu nại.

- Hủy đơn.

- Thay đổi đơn.

PACK-07 không được gọi Messenger conversation là order nếu chưa có ORDER_EVENT hợp lệ.

Messenger text không được tự tạo revenue.

Messenger chỉ là một tầng trong funnel nếu chưa có owner order/payment/revenue event.

## 25.3. Quote là chặng giá, không phải doanh thu

QUOTE_EVENT phải phân biệt:

- Quote requested.

- Quote generated.

- Quote shown.

- Quote accepted by customer.

- Quote expired.

- Quote cancelled.

- Quote superseded.

- Quote bị chặn due to sellable/stock/sale lock.

- Quote failed due to runtime missing.

- Quote owner review required.

Quote generated không phải revenue.

Quote accepted không phải paid.

Quote expired không được tính conversion.

Quote bị chặn do sale lock không được tính là AI failure nếu PACK-05/PACK-06 xử lý đúng policy.

## 25.4. Cart draft không phải official order

Cart draft có thể được tạo trong quá trình chốt đơn.

Cart draft không được tính là order chính thức nếu chưa có Customer Confirmation và owner Order tạo official order/order_code.

PACK-07 có thể đo:

- Quote-to-cart rate.

- Cart completion rate.

- Cart abandonment rate.

- Cart bị chặn rate.

- Cart-to-order rate.

PACK-07 không được tính cart draft vào revenue.

## 25.5. Official Order là chặng order chính thức

Official order phải đến từ owner Order/Commerce.

Official order cần có order identity hợp lệ.

Không có order_code thì không gọi là đơn đã ghi nhận chính thức.

ORDER_EVENT phải cho biết tối thiểu:

- Order created.

- Order confirmed.

- Order cancelled.

- Order changed.

- Order payment status.

- Order delivery status.

- Order verification status.

- Order exclusion status nếu có.

PACK-07 chỉ consume trạng thái này để đo funnel.

PACK-07 không được sửa order.

## 25.6. Verified Revenue là chặng doanh thu đủ điều kiện đo ROAS

Verified Revenue phải đến từ owner revenue policy.

Một order chỉ được đi vào ROAS chính thức khi đạt điều kiện verified revenue theo rule.

Verified Revenue có thể phụ thuộc vào:

- Payment confirmed.

- COD success.

- Delivery success.

- No cancellation.

- No refund.

- No return.

- No duplicate.

- No test order.

- No unresolved complaint nếu policy yêu cầu.

- No attribution conflict unresolved.

- No revenue exclusion rule triggered.

- Owner revenue verification pass.

Nếu thiếu Verified Revenue event, PACK-07 phải giữ doanh thu ở trạng thái waiting.

## 26. REVENUE CLASSIFICATION RULES

## 26.1. Các lớp doanh thu bắt buộc phân biệt

PACK-07 phải phân biệt tối thiểu các lớp doanh thu sau:

- Gross Order Value.

- Confirmed Order Value.

- Payment waiting Value.

- Paid Revenue.

- COD waiting Value.

- COD Successful Revenue.

- Delivery waiting Value.

- Delivered Revenue.

- Verified Revenue.

- Net Verified Revenue.

- Revenue waiting Verification.

- Revenue Excluded From ROAS.

- Refund/Return Adjustment.

- Accounting Reconciled Revenue nếu có.

- Marketing Measurement Revenue.

Không được gom các lớp trên thành một dòng “doanh thu” chung.

## 26.2. Gross Order Value

Gross Order Value là tổng giá trị đơn trước khi hoàn tất các điều kiện xác nhận sau cùng.

Gross Order Value không dùng để tính ROAS chính thức nếu policy yêu cầu verified revenue.

Gross Order Value chỉ dùng để quan sát pipeline hoặc forecast.

## 26.3. Confirmed Order Value

Confirmed Order Value là giá trị đơn đã được khách xác nhận và owner Order ghi nhận.

Confirmed Order Value chưa phải paid revenue.

Confirmed Order Value chưa phải verified revenue.

Confirmed Order Value có thể bị hủy, đổi, hoàn, COD fail hoặc payment fail.

## 26.4. Paid Revenue

Paid Revenue là doanh thu đã có payment confirmation hợp lệ.

Paid Revenue phải đến từ Payment owner.

Ảnh chuyển khoản chưa đối chiếu không phải Paid Revenue.

Khách nói “đã chuyển khoản” không phải Paid Revenue.

Nhân viên thấy tin nhắn chuyển khoản nhưng chưa đối chiếu không phải Paid Revenue.

## 26.5. COD Successful Revenue

COD Successful Revenue là doanh thu COD đạt trạng thái thành công theo owner policy.

COD waiting không phải COD Successful Revenue.

Đơn đang giao không phải COD Successful Revenue.

Đơn giao thất bại không phải COD Successful Revenue.

Đơn hoàn về không phải COD Successful Revenue.

## 26.6. Delivered Revenue

Delivered Revenue chỉ được dùng khi Fulfillment/Shipping owner xác nhận giao thành công.

Nếu policy yêu cầu delivery success trước khi verified, Delivered Revenue là điều kiện trung gian.

Nếu payment đã thành công nhưng giao thất bại/hoàn hàng, revenue phải xử lý theo exclusion/adjustment rule.

## 26.7. Verified Revenue

Verified Revenue là lớp doanh thu được phép dùng làm cơ sở ROAS chính thức nếu owner policy xác nhận.

Verified Revenue phải có:

- Official order.

- Payment/COD/delivery status đạt điều kiện.

- No active exclusion.

- Attribution đủ điều kiện.

- Data quality pass.

- Revenue owner confirmation.

- Evidence.

Verified Revenue không tự sinh từ Ads dashboard.

## 26.8. Net Verified Revenue

Net Verified Revenue là Verified Revenue sau khi trừ các điều chỉnh hợp lệ nếu policy yêu cầu.

Các điều chỉnh có thể gồm:

- Refund.

- Return.

- Partial refund.

- Discount correction.

- Cancelled line item.

- Failed COD adjustment.

- Duplicate correction.

- Owner-approved adjustment.

Nếu báo cáo dùng Net Verified Revenue, phải ghi rõ revenue basis.

## 27. REVENUE EXCLUSION RULES

## 27.1. Nguyên tắc loại trừ doanh thu

Một order không được đưa vào ROAS chính thức nếu có exclusion rule đang active.

Revenue exclusion không xóa order.

Revenue exclusion chỉ loại order khỏi chỉ số đo lường ROAS theo policy.

Mỗi exclusion phải có:

- Lý do.

- Event nguồn.

- Owner.

- Timestamp.

- Evidence.

- Review status nếu cần.

## 27.2. Danh mục exclusion tối thiểu

Các trường hợp phải có rule loại trừ:

- Order cancelled.

- Payment failed.

- Bank transfer not reconciled.

- COD failed.

- Delivery failed.

- Order returned.

- Order refunded.

- Partial refund.

- Duplicate order.

- Test order.

- Internal order.

- Staff order nếu policy loại trừ.

- Gift/sample order nếu policy loại trừ.

- Fraud suspected.

- Spam/bot order.

- Complaint unresolved.

- Recall impacted.

- Sale Lock impacted.

- Data quality fail.

- Attribution conflict unresolved.

- Missing verified revenue event.

- Owner policy excluded.

## 27.3. Revenue waiting không được tính ROAS chính thức

Revenue waiting verification phải tách riêng.

Không được đưa waiting revenue vào ROAS chính thức.

Có thể hiển thị waiting revenue trong dashboard nếu ghi rõ:

REVENUE_waiting_VERIFICATION — NOT_INCLUDED_IN_OFFICIAL_ROAS

## 28. ATTRIBUTION MODEL GOVERNANCE

## 28.1. Mỗi báo cáo phải có attribution model

PACK-07 không được hiển thị doanh thu theo nguồn nếu không khai báo attribution model.

Attribution model phải trả lời:

- Đơn này gán về nguồn nào.

- Vì sao gán về nguồn đó.

- Touchpoint nào được tính.

- Touchpoint nào bị loại.

- Có conflict không.

- Rule nào đã dùng.

- Evidence nào chứng minh.

- Revenue basis là gì.

- Data quality có pass không.

- Owner approval có cần không.

## 28.2. Các mô hình attribution được phép định nghĩa

PACK-07 có thể hỗ trợ các mô hình sau, tùy owner phê duyệt:

- First Touch Attribution.

- Last Touch Attribution.

- Last Paid Touch Attribution.

- Last Non-Direct Touch Attribution.

- Session-Based Attribution.

- Campaign-Bound Attribution.

- Live-Session Attribution.

- Messenger-Thread Attribution.

- CRM-Rebuy Attribution.

- Diamond Referral Attribution.

- Multi-Touch Attribution.

- Owner-Approved Custom Attribution.

Không được tự chọn model khác khi chưa có owner approval.

Không được dùng một model cho dashboard này và model khác cho dashboard kia mà không ghi rõ.

## 28.3. First Touch Attribution

First Touch gán conversion về touchpoint đầu tiên đủ điều kiện.

First Touch phù hợp để đo nguồn mở đầu hành trình khách.

First Touch không mặc định phù hợp để quyết định scale ngắn hạn nếu khách có nhiều tương tác sau đó.

First Touch phải có entry event rõ.

Nếu entry event thiếu hoặc conflict, First Touch không được dùng chính thức.

## 28.4. Last Touch Attribution

Last Touch gán conversion về touchpoint cuối cùng trước conversion.

Last Touch phù hợp để đo chặng chốt.

Last Touch có thể làm sai lệch vai trò của Ads đầu phễu hoặc CRM trước đó.

Nếu khách có Diamond/referral bind hoặc CRM event gần conversion, phải xử lý conflict theo policy.

## 28.5. Last Paid Touch Attribution

Last Paid Touch gán conversion về paid touchpoint gần nhất đủ điều kiện.

Mô hình này phù hợp để đo quảng cáo trả phí.

Tuy nhiên, Last Paid Touch không được dùng nếu:

- Paid event thiếu campaign/ad identity.

- Paid event không qua approved source.

- Order/revenue không có linkage.

- Conflict với Diamond/referral chưa xử lý.

- Data quality fail.

## 28.6. Live-Session Attribution

Live-Session Attribution gán conversion về phiên live khi có live context hợp lệ.

Điều kiện tối thiểu:

- Live session identity.

- Page identity.

- Live comment hoặc live entry event.

- Comment-to-Messenger hoặc live-to-private trace.

- Quote/order/revenue linkage.

- Data quality pass.

- Attribution rule pass.

Không có live session identity thì không được gán doanh thu cho live.

Không có private trace thì không được gán Messenger order về live comment.

## 28.7. Diamond Referral Attribution

Diamond Referral Attribution chỉ hợp lệ khi có referral bind/event hợp lệ từ owner.

Không có bind thì không có Diamond attribution.

Không có owner eligibility thì không có commission measurement.

PACK-07 không tự tính hoa hồng.

PACK-07 chỉ đo nguồn referral và trạng thái attribution nếu event hợp lệ.

## 28.8. CRM-Rebuy Attribution

CRM-Rebuy Attribution dùng cho doanh thu mua lại/chăm sóc lại.

Một khách từng đến từ Ads nhưng mua lại sau CRM không mặc định là Ads revenue.

CRM rebuy phải có CRM_EVENT hợp lệ và rule attribution rõ.

Nếu khách vừa nhận CRM vừa click Ads remarketing, phải đánh dấu conflict và xử lý theo owner-approved rule.

## 28.9. Multi-Touch Attribution

Multi-Touch Attribution dùng khi owner muốn phân bổ công cho nhiều touchpoint.

Multi-Touch phải có rule rõ về trọng số hoặc cách ghi nhận.

Không được tự chia phần trăm doanh thu nếu chưa có owner approval.

Nếu chưa có weight rule, chỉ được hiển thị touchpoint chain, không được phân bổ revenue chính thức.

## 29. ATTRIBUTION CONFLICT RESOLUTION

## 29.1. Nguyên tắc conflict

Khi một conversion có nhiều nguồn có thể nhận công, PACK-07 phải tạo trạng thái conflict.

Conflict không phải lỗi.

Conflict là tín hiệu cần rule xử lý.

Nếu chưa có rule xử lý, conversion không được đưa vào ROAS chính thức theo nguồn.

Trạng thái mặc định:

## ATTRIBUTION_CONFLICT_UNRESOLVED — EXCLUDED_FROM_SOURCE_ROAS

## 29.2. Các nhóm conflict bắt buộc nhận diện

PACK-07 phải nhận diện tối thiểu:

- Ads click trước, Diamond link sau.

- Diamond link trước, Ads remarketing sau.

- Paid live boost + organic live comment.

- Organic comment trong post đang chạy Ads.

- CRM message trước, Ads click sau.

- Ads click trước, CRM rebuy sau.

- Messenger direct không có ad context nhưng cùng khách từng click ads.

- Landing page visit trước, Messenger order sau.

- Human telesales can thiệp sau AI tư vấn.

- AI Advisor tạo quote, nhân viên chốt ngoài hệ thống.

- Khách chuyển page khác trong cùng hệ thống.

- Khách dùng nhiều số điện thoại/tài khoản.

- Khách mua lại cho người nhận khác.

- Khách vào từ MC AI Live và Facebook Ads cùng lúc.

- Khách có nhiều campaign touchpoints trong thời gian ngắn.

## 29.3. Conflict status

Các trạng thái conflict tối thiểu:

- NO_CONFLICT.

- CONFLICT_DETECTED.

- RULE_REQUIRED.

- OWNER_REVIEW_REQUIRED.

- RULE_APPLIED.

- CONFLICT_RESOLVED.

- EXCLUDED_DUE_TO_UNRESOLVED_CONFLICT.

- SPLIT_ATTRIBUTION_waiting.

- MULTI_TOUCH_ALLOWED.

- ATTRIBUTION_LOCKED.

Nếu trạng thái chưa đạt CONFLICT_RESOLVED hoặc ATTRIBUTION_LOCKED, không được đưa vào báo cáo ROAS chính thức theo nguồn.

## 30. PAID / ORGANIC / CRM / DIAMOND / LIVE CLASSIFICATION

## 30.1. Paid revenue

Paid revenue theo Ads chỉ được ghi nhận khi có:

- Paid source hợp lệ.

- Campaign/adset/ad identity.

- Customer/session linkage.

- Channel trace.

- Quote/order/revenue linkage.

- Attribution rule pass.

- Verified revenue.

- Data quality pass.

Thiếu một trong các điều kiện P0 thì không được gọi là Ads revenue chính thức.

## 30.2. Organic revenue

Organic revenue là doanh thu từ nguồn không trả phí hoặc không có paid attribution hợp lệ.

Không được gán Organic mặc định chỉ vì thiếu ad_id.

Nếu thiếu dữ liệu campaign, trạng thái phải là:

## SOURCE_UNKNOWN — DATA_QUALITY_WARNING

Không được biến missing data thành Organic revenue.

## 30.3. CRM revenue

CRM revenue phải có CRM_EVENT hợp lệ.

CRM revenue phải tách khỏi Ads revenue nếu attribution policy không cho phép cộng dồn.

CRM revenue không được tính vào Ads ROAS nếu conversion đến từ chăm sóc lại và không có paid attribution hợp lệ.

## 30.4. Diamond / Referral revenue

Diamond/referral revenue phải có referral event/bind hợp lệ.

Không có referral bind thì không được gọi là Diamond revenue.

Diamond revenue không tự động là Ads revenue.

Nếu Ads và Diamond cùng xuất hiện, phải dùng conflict resolution.

## 30.5. Live revenue

Live revenue phải có live session context.

Live revenue có thể là:

- Organic live revenue.

- Paid live boost revenue.

- Live-to-Messenger revenue.

- Live-to-Quote revenue.

- Live-to-Order revenue.

- Live verified revenue.

Nếu live được chạy Ads/boost, attribution phải phân biệt Paid Live và Organic Live.

## 31. METRIC DEFINITIONS — FUNNEL

## 31.1. Impression, Reach, Click

Impression, Reach và Click chỉ phản ánh hoạt động quảng cáo đầu phễu.

Các chỉ số này không đủ để kết luận doanh thu.

Chỉ số liên quan:

- CPM.

- CTR.

- CPC.

- Click-to-comment rate.

- Click-to-inbox rate.

- Click-to-qualified-conversation rate.

## 31.2. Comment metrics

Comment metrics phải phân biệt:

- Total comments.

- Unique commenters.

- Qualified comments.

- Buying intent comments.

- Price inquiry comments.

- Product question comments.

- Spam comments.

- Complaint comments.

- Hidden/deleted/suppressed comments nếu có.

- Comment-to-Messenger handoff rate.

Không được dùng total comments để kết luận lead quality nếu chưa lọc spam/duplicate/irrelevant.

## 31.3. Messenger metrics

Messenger metrics phải phân biệt:

- New conversations.

- Existing customer conversations.

- Qualified conversations.

- Product consult conversations.

- Quote request conversations.

- Complaint conversations.

- Human takeover conversations.

- AI bị chặn/guarded conversations.

- Conversation-to-quote rate.

- Conversation-to-order rate.

- Conversation-to-verified-revenue rate.

Messenger volume cao không đồng nghĩa doanh thu cao.

## 31.4. Quote metrics

Quote metrics tối thiểu:

- Quote requests.

- Quotes generated.

- Quotes failed.

- Quotes bị chặn by sellable/stock/sale lock.

- Quotes expired.

- Quotes accepted.

- Quote-to-cart rate.

- Quote-to-order rate.

- Quote-to-verified-revenue rate.

- Average quote value.

Quote value không phải revenue.

## 31.5. Order metrics

Order metrics tối thiểu:

- Official orders created.

- Orders confirmed.

- Orders cancelled.

- Orders payment waiting.

- Orders payment confirmed.

- Orders COD waiting.

- Orders COD success.

- Orders delivery waiting.

- Orders delivery success.

- Orders returned.

- Orders refunded.

- Orders verified.

- Orders excluded from ROAS.

Official order count không phải verified order count.

## 31.6. Revenue metrics

Revenue metrics tối thiểu:

- Gross order value.

- Confirmed order value.

- Paid revenue.

- COD successful revenue.

- Delivered revenue.

- Verified revenue.

- Net verified revenue.

- waiting revenue.

- Excluded revenue.

- Refunded value.

- Returned value.

- Revenue by source.

- Revenue by campaign.

- Revenue by live session.

- Revenue by product.

- Revenue by customer class nếu policy cho phép.

Revenue by source chỉ được hiển thị khi attribution pass.

## 32. METRIC DEFINITIONS — ADS PERFORMANCE

## 32.1. CPA

CPA phải khai báo rõ mẫu số đang dùng.

Các biến thể CPA:

- Cost per comment.

- Cost per qualified comment.

- Cost per Messenger conversation.

- Cost per qualified conversation.

- Cost per quote request.

- Cost per quote generated.

- Cost per official order.

- Cost per verified order.

- Cost per verified customer.

- Cost per verified revenue event.

Không được gọi chung “CPA” nếu không nói rõ conversion basis.

CPA dùng cho scale nên ưu tiên dựa trên official order hoặc verified revenue theo owner policy.

## 32.2. ROAS

ROAS phải khai báo rõ revenue basis.

Các biến thể ROAS:

- ROAS by gross order value.

- ROAS by confirmed order value.

- ROAS by paid revenue.

- ROAS by COD successful revenue.

- ROAS by delivered revenue.

- ROAS by verified revenue.

- ROAS by net verified revenue.

- ROAS by accounting reconciled revenue nếu có.

ROAS production mặc định không được dùng gross/confirmed/waiting revenue nếu owner chưa phê duyệt.

ROAS chính thức phải dựa trên verified revenue hoặc revenue basis được owner policy khóa.

## 32.3. AOV

AOV phải khai báo rõ order basis.

Các biến thể AOV:

- AOV by confirmed order.

- AOV by paid order.

- AOV by COD successful order.

- AOV by delivered order.

- AOV by verified order.

- AOV by net verified revenue.

Không được dùng AOV từ cart draft làm AOV chính thức.

Không được dùng AOV từ quote làm AOV doanh thu.

## 33. DATA QUALITY EVENT RULES

## 33.1. DATA_QUALITY_EVENT bắt buộc trước reporting

Mỗi batch đo lường hoặc dashboard snapshot phải có DATA_QUALITY_EVENT.

DATA_QUALITY_EVENT phải cho biết:

- Dữ liệu có đủ nguồn không.

- Event có duplicate không.

- Attribution có conflict không.

- Revenue đã verified chưa.

- Exclusion đã áp dụng chưa.

- Spend có hợp lệ không.

- Campaign mapping có đủ không.

- Channel trace có đủ không.

- Quote/order linkage có đủ không.

- Missing data có tồn tại không.

- Privacy/spam/suppression có ảnh hưởng không.

- Dữ liệu có đủ điều kiện dùng cho scale không.

## 33.2. Data Quality outcome

Các outcome tối thiểu:

- DQ_PASS.

- DQ_PASS_WITH_WARNING.

- DQ_waiting.

- DQ_FAIL.

- DQ_OWNER_REVIEW_REQUIRED.

- DQ_EXCLUDED_FROM_ROAS.

- DQ_SCALE_BLOCKED.

- DQ_REPORTING_ONLY.

- DQ_NOT_FOR_PRODUCTION_DECISION.

Chỉ DQ_PASS hoặc trạng thái được owner policy cho phép mới được dùng cho scale decision.

## 34. MEASUREMENT WINDOW GOVERNANCE

## 34.1. Time window phải rõ

Mọi báo cáo PACK-07 phải khai báo time window.

Time window tối thiểu:

- Start time.

- End time.

- Timezone.

- Campaign window.

- Conversion window.

- Revenue verification window.

- Attribution lookback window.

- Data refresh timestamp.

- Late event handling.

- Owner cutoff rule nếu có.

Không được so ROAS hai dashboard có time window khác nhau mà không cảnh báo.

## 34.2. Late event handling

Một số event có thể đến muộn:

- Payment confirmation.

- COD success.

- Delivery success.

- Refund.

- Return.

- Complaint.

- Reconcile.

- Verified revenue.

- MISA/accounting checkpoint nếu dùng.

PACK-07 phải cho phép trạng thái metric thay đổi từ waiting sang verified hoặc excluded khi late event đến.

Không được khóa ROAS quá sớm nếu revenue verification window chưa kết thúc.

## 35. CUSTOMER / SESSION / ORDER LINKAGE GOVERNANCE

## 35.1. Linkage là điều kiện sống còn của attribution

PACK-07 chỉ đo chính xác khi nối được:

- Ads touchpoint.

- Channel event.

- Messenger thread.

- AI consult.

- Quote.

- Cart.

- Official order.

- Payment/COD.

- Delivery.

- Verified revenue.

- Customer identity hoặc session identity.

- Attribution rule.

Nếu chuỗi bị đứt, metric phải chuyển cảnh báo.

## 35.2. Không nối được khách thì không gán nguồn chính thức

Nếu không nối được customer/session/order, PACK-07 không được gán doanh thu về nguồn cụ thể.

Trạng thái phải là:

## UNLINKED_REVENUE — ATTRIBUTION_NOT_ALLOWED

hoặc

## UNLINKED_EVENT — FUNNEL_INCOMPLETE

Không được tự nối bằng tên giống nhau, số điện thoại mơ hồ hoặc suy luận thủ công nếu owner policy chưa cho phép.

## 36. PRODUCT / SKU MEASUREMENT GOVERNANCE

## 36.1. Đo theo sản phẩm phải dùng product identity hợp lệ

PACK-07 có thể đo performance theo sản phẩm/SKU nếu nhận được product identity hợp lệ từ owner pack.

PACK-07 không được dùng mã SKU nội bộ trong customer-facing dashboard nếu policy không cho phép.

PACK-07 không được public BOM/công thức/tỷ lệ nội bộ.

Product performance chỉ được đo theo thông tin được phép:

- Product public name.

- Product group.

- Campaign product tag.

- Hero product flag nếu owner cung cấp.

- Sellable status từ owner.

- Stock/sale lock status từ owner.

- Verified revenue by product nếu order/revenue owner cung cấp.

Không được đo product revenue từ comment “mua dòng A” nếu thiếu order line verified.

## 36.2. Hero product bị chặn thì attribution vẫn giữ, scale phải block

Nếu sản phẩm hero hết hàng/not sellable/sale lock, PACK-07 vẫn có thể ghi nhận Ads/AI interest.

Tuy nhiên, Scale Gate phải block nếu sản phẩm không đủ điều kiện bán.

Dashboard phải tách:

- Demand signal.

- Sellable capacity.

- Stock constraint.

- Sale lock constraint.

- Fulfillment constraint.

- Revenue conversion.

Không được đánh giá Ads kém nếu demand tốt nhưng Commerce chặn bán vì stock/sale lock.

## 37. COMPLAINT / QUALITY / RISK EVENT GOVERNANCE

## 37.1. Complaint event phải tách khỏi sales conversion

Complaint conversation không được tính là lead bán hàng.

Complaint event phải đi vào risk metric.

Nếu complaint liên quan tới sản phẩm/campaign/live/AI reply, PACK-07 phải đánh dấu ảnh hưởng.

Các loại complaint tối thiểu:

- Product quality complaint.

- Taste/texture complaint.

- Delivery complaint.

- Payment complaint.

- Price/program complaint.

- Claim/safety complaint.

- Privacy complaint.

- Wrong product complaint.

- Late delivery complaint.

- Refund/return complaint.

Complaint tăng bất thường có thể kích hoạt Stop/Pause Gate ở PHẦN 3/4.

## 37.2. Claim/privacy incident phải chặn scale

Nếu có claim incident hoặc privacy incident liên quan ads/live/comment/Messenger, PACK-07 không được cho Scale PASS nếu chưa được xử lý.

Trạng thái:

## RISK_INCIDENT_OPEN — SCALE_BLOCKED

## 38. EVENT EVIDENCE REQUIREMENTS

## 38.1. Evidence tối thiểu cho event

Mỗi event dùng cho reporting chính thức phải có evidence tối thiểu:

- Event source.

- Event owner.

- Event timestamp.

- Event identity.

- Correlation identity.

- Data quality result.

- Attribution result nếu dùng.

- Revenue basis nếu dùng.

- Exclusion status nếu có.

- Review status nếu cần.

Không có evidence thì event chỉ được xem là raw/waiting, không được dùng cho PASS.

## 38.2. Evidence cho Verified Revenue

Verified Revenue event phải có evidence:

- Official order identity.

- Customer confirmation nếu policy yêu cầu.

- Payment/COD/delivery status theo policy.

- Refund/cancel/return status.

- Revenue amount.

- Revenue class.

- Attribution status.

- Data quality status.

- Owner confirmation.

- Timestamp.

- Audit/correlation.

Không có đủ evidence thì revenue phải ở trạng thái waiting.

## 39. DONE CONDITION CỦA PHẦN 2/4

## PHẦN 2/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Event taxonomy principles.

- Event family registry.

- Event lifecycle.

- Event exclusion rules.

- Funnel measurement principles.

- Comment -> Messenger -> Quote -> Cart -> Order -> Payment/COD -> Verified Revenue chain.

- Revenue classification.

- Revenue exclusion.

- Attribution model governance.

- Attribution conflict resolution.

- Paid/Organic/CRM/Diamond/Live classification.

- Funnel metric definitions.

- Ads metric definitions.

- Data Quality Event rules.

- Measurement window governance.

- Customer/session/order linkage governance.

- Product/SKU measurement governance.

- Complaint/quality/risk event governance.

- Event evidence requirements.

## PHẦN 2/4 chưa định nghĩa chi tiết Dashboard Governance, KPI presentation, ROAS/CPA/AOV decision display, Scale Gate, Stop Gate, Owner Decision Matrix, Smoke Matrix, Done Gate, Fail Gate hoặc Release Control. Các nội dung đó thuộc PHẦN 3/4 và PHẦN 4/4.

## KẾT LUẬN PHẦN 2/4

PACK-07 chỉ được đo lường bằng event hợp lệ.

Event phải có owner, source, correlation, data quality và evidence.

Comment không phải đơn hàng.

Messenger không phải doanh thu.

Quote không phải revenue.

Cart draft không phải official order.

Official order chưa chắc là paid revenue.

Payment waiting chưa chắc là verified revenue.

Verified Revenue là điều kiện trọng yếu để tính ROAS chính thức.

Attribution phải có rule.

Conflict phải được đánh dấu.

Missing data không được suy diễn.

waiting revenue không được đưa vào ROAS production.

Data Quality Event là lớp bắt buộc trước reporting, dashboard và scale decision.

Nguyên tắc khóa của PHẦN 2/4 là:

Đo đúng từng chặng funnel.

Không nhảy tầng dữ liệu.

Không tính trùng nguồn doanh thu.

Không biến tín hiệu tương tác thành doanh thu.

Không có Verified Revenue thì không có ROAS chính thức.

Không có Attribution Rule thì không có kết luận nguồn.

## PHẦN 3/4 — DASHBOARD / KPI / ROAS / CPA / AOV / SCALE GATE / STOP GATE / OWNER DECISION

## 40. MỤC TIÊU CỦA PHẦN 3/4

## PHẦN 3/4 thiết lập lớp quản trị dashboard, chỉ số đo lường, ROAS, CPA, AOV, Scale Gate, Stop/Pause Gate và Owner Decision cho PACK-07.

Mục tiêu của phần này là khóa rõ:

- Dashboard được phép hiển thị gì.

- Dashboard không được phép kết luận gì khi dữ liệu chưa đủ.

- KPI phải được định nghĩa theo đúng revenue basis và conversion basis.

- ROAS chỉ được dùng khi data quality và verified revenue đạt điều kiện.

- CPA phải khai báo rõ conversion basis.

- AOV phải khai báo rõ order/revenue basis.

- Scale Gate phải kiểm tra đủ Ads, AI, Commerce, stock, fulfillment, revenue và data quality.

- Stop/Pause Gate phải chặn kịp thời khi rủi ro tăng.

- Owner Decision là điều kiện bắt buộc trước khi scale production.

- Dashboard không được che giấu data issue, attribution conflict, revenue waiting hoặc sale lock.

## PHẦN 3/4 không thiết kế UI chi tiết, không thiết kế API, không thiết kế database và không viết code.

## 41. DASHBOARD GOVERNANCE PRINCIPLES

## 41.1. Dashboard là lớp quan sát, không phải source-of-truth

Dashboard PACK-07 chỉ là lớp hiển thị và phân tích dữ liệu đo lường.

Dashboard không phải source-of-truth cho:

- Đơn hàng.

- Doanh thu.

- Thanh toán.

- COD.

- Giao hàng.

- Tồn kho.

- Sellable status.

- Sale Lock.

- Giá.

- Khuyến mãi.

- Member benefit.

- Diamond benefit.

- Commission.

- MISA accounting.

- Complaint resolution.

Dashboard chỉ được hiển thị dữ liệu đã lấy từ owner source hợp lệ hoặc projection đo lường có kiểm soát.

Dashboard không được ghi ngược dữ liệu nghiệp vụ gốc.

Dashboard không được tự sửa doanh thu.

Dashboard không được tự sửa attribution.

Dashboard không được tự xác nhận ROAS PASS.

Dashboard không được tự quyết định scale nếu thiếu owner rule/approval.

## 41.2. Dashboard phải phân biệt các lớp dữ liệu

Dashboard PACK-07 phải luôn phân biệt:

- Raw data.

- Normalized data.

- Clean data.

- Verified data.

- Excluded data.

- waiting data.

- Missing data.

- Duplicate data.

- Conflict data.

- Suppressed data.

- Privacy-bị chặn data.

- Owner-review data.

Không được gom toàn bộ dữ liệu thành một số tổng.

Không được hiển thị “doanh thu” mà không nói rõ revenue class.

Không được hiển thị “ROAS” mà không nói rõ revenue basis.

Không được hiển thị “CPA” mà không nói rõ conversion basis.

Không được hiển thị “AOV” mà không nói rõ order/revenue basis.

## 41.3. Dashboard phải có cảnh báo độ tin cậy

Mỗi dashboard snapshot phải có trạng thái độ tin cậy.

Các trạng thái tối thiểu:

- DASHBOARD_READY_FOR_VIEW.

- DASHBOARD_READY_FOR_DECISION.

- DASHBOARD_WARNING.

- DASHBOARD_DATA_waiting.

- DASHBOARD_DATA_QUALITY_FAIL.

- DASHBOARD_ATTRIBUTION_CONFLICT.

- DASHBOARD_REVENUE_NOT_VERIFIED.

- DASHBOARD_SCALE_BLOCKED.

- DASHBOARD_OWNER_REVIEW_REQUIRED.

- DASHBOARD_NOT_FOR_PRODUCTION_DECISION.

Nếu dashboard chưa đạt READY_FOR_DECISION, không được dùng để scale production.

Nếu dashboard có data quality fail, không được hiển thị trạng thái tốt/màu xanh cho ROAS chính thức.

Nếu dashboard có attribution conflict chưa xử lý, doanh thu theo nguồn phải chuyển sang owner review.

## 42. DASHBOARD DATA LAYERS

## 42.1. Raw Dashboard

Raw Dashboard dùng để quan sát dữ liệu gốc.

Raw Dashboard có thể hiển thị:

- Raw spend.

- Raw impression.

- Raw reach.

- Raw click.

- Raw comment.

- Raw inbox.

- Raw event count.

- Raw order count nếu nguồn gửi.

- Raw revenue nếu nguồn gửi.

Raw Dashboard không được dùng để quyết định scale.

Raw Dashboard không được gọi là performance chính thức.

Raw Dashboard phải có cảnh báo:

## RAW_DATA — NOT_FOR_SCALE_DECISION

## 42.2. Clean Dashboard

Clean Dashboard chỉ hiển thị dữ liệu đã qua:

- Source validation.

- Normalization.

- Deduplication.

- Spam/bot suppression.

- Privacy guard.

- Required field check.

- Event correlation check.

- Data quality check.

Clean Dashboard có thể dùng để phân tích funnel.

Clean Dashboard chưa chắc được dùng để tính ROAS nếu revenue chưa verified.

## 42.3. Verified Dashboard

Verified Dashboard chỉ hiển thị dữ liệu đã đạt:

- Clean data.

- Attribution rule pass.

- Quote/order linkage pass.

- Official order validation nếu dùng order metric.

- Payment/COD/delivery validation nếu dùng revenue metric.

- Verified revenue pass.

- Revenue exclusion applied.

- Owner policy pass.

- Evidence available.

Verified Dashboard là dashboard chính để xét ROAS production, Scale Gate và Stop Gate.

## 42.4. Excluded Dashboard

Excluded Dashboard phải hiển thị dữ liệu bị loại khỏi ROAS hoặc decision.

Các nhóm bị loại:

- Duplicate.

- Spam.

- Bot suspected.

- Privacy bị chặn.

- Channel error.

- Missing campaign context.

- Missing channel trace.

- Missing quote/order linkage.

- Payment not confirmed.

- COD failed.

- Delivery failed.

- Cancelled order.

- Refunded order.

- Returned order.

- Test order.

- Internal order nếu policy loại.

- Attribution conflict unresolved.

- Revenue not verified.

- Sale Lock impacted.

- Complaint unresolved.

- Owner policy excluded.

Dashboard không được ẩn dữ liệu bị loại, vì dữ liệu bị loại giúp owner hiểu rủi ro và nguyên nhân sai lệch.

## 42.5. waiting Dashboard

waiting Dashboard dùng cho dữ liệu đang chờ xác minh.

Các nhóm waiting:

- Payment waiting.

- COD waiting.

- Delivery waiting.

- Revenue verification waiting.

- Attribution rule waiting.

- Owner review waiting.

- Refund/return window waiting.

- MISA/accounting reconcile waiting nếu có dùng.

- Data refresh waiting.

- Late event waiting.

waiting revenue không được tính vào ROAS chính thức.

waiting data có thể hiển thị để quan sát pipeline nhưng phải có cảnh báo rõ.

## 43. DASHBOARD STRUCTURE GOVERNANCE

## 43.1. Executive Measurement Dashboard

Executive Measurement Dashboard dành cho owner xem nhanh tình hình tổng thể.

Các nhóm hiển thị tối thiểu:

- Ads spend.

- Verified revenue.

- Net verified revenue nếu policy dùng.

- Official ROAS status.

- CPA by verified order.

- AOV by verified order.

- Qualified conversation rate.

- Quote-to-order rate.

- Order-to-verified-revenue rate.

- Data quality status.

- Attribution conflict status.

- Scale Gate status.

- Stop/Pause Gate status.

- Owner decision waiting.

- Top risk alerts.

Executive Dashboard không được hiển thị ROAS chính thức nếu revenue basis chưa verified.

## 43.2. Ads Performance Dashboard

Ads Performance Dashboard đo hiệu quả quảng cáo.

Các nhóm chỉ số tối thiểu:

- Spend.

- Impression.

- Reach.

- CPM.

- Click.

- CTR.

- CPC.

- Landing view nếu có.

- Comment.

- Qualified comment.

- Inbox/Messenger conversation.

- Qualified conversation.

- Quote request.

- Official order.

- Verified revenue.

- ROAS by verified revenue.

- CPA by selected conversion basis.

- Campaign/adset/ad creative performance.

- Data quality warning.

- Attribution warning.

Ads Dashboard không được kết luận doanh thu nếu conversion chưa đi đến verified revenue.

## 43.3. Funnel Dashboard

Funnel Dashboard đo từng chặng chuyển đổi.

Funnel chuẩn:

Impression -> Reach -> Click -> Landing View -> Comment -> Qualified Comment -> Private Handoff -> Messenger Conversation -> Qualified Conversation -> Product Consult -> Quote Request -> Quote Generated -> Cart Draft -> Customer Confirmation -> Official Order -> Payment/COD Confirmation -> Delivery Success nếu policy yêu cầu -> Order Verified -> Verified Revenue

Funnel Dashboard phải hiển thị drop-off ở từng chặng.

Nếu chặng nào thiếu dữ liệu, dashboard phải đánh dấu missing.

Không được nhảy từ comment sang order.

Không được nhảy từ quote sang revenue.

Không được gộp cart draft với official order.

## 43.4. Attribution Dashboard

Attribution Dashboard hiển thị nguồn phân bổ doanh thu.

Các lớp tối thiểu:

- Paid Ads.

- Organic.

- Live.

- Paid Live/Boosted Live nếu có.

- Messenger Direct.

- CRM Rebuy.

- Diamond Referral.

- Member Rebuy.

- Landing Page.

- Mixed Source.

- Unknown Source.

- Conflict Source.

- Owner Review Required.

Attribution Dashboard chỉ được hiển thị source revenue chính thức khi attribution rule pass.

Nếu conflict chưa xử lý, revenue phải nằm ở nhóm conflict/waiting.

## 43.5. Revenue Dashboard

Revenue Dashboard phải phân biệt:

- Gross order value.

- Confirmed order value.

- Payment waiting value.

- Paid revenue.

- COD waiting value.

- COD successful revenue.

- Delivered revenue.

- Verified revenue.

- Net verified revenue.

- waiting revenue.

- Excluded revenue.

- Refund/return adjustment.

- Accounting reconciled revenue nếu có.

Revenue Dashboard phải hiển thị revenue basis rõ ràng.

Không được gọi chung là “doanh thu” nếu chưa nêu class.

## 43.6. Data Quality Dashboard

Data Quality Dashboard là dashboard bắt buộc.

Các nhóm chỉ số tối thiểu:

- Total events received.

- Valid source events.

- Invalid source events.

- Duplicate events.

- Missing required fields.

- Missing campaign mapping.

- Missing channel trace.

- Missing quote/order linkage.

- Missing verified revenue.

- Attribution conflicts.

- Spam/bot suppressed.

- Privacy-bị chặn events.

- Excluded events.

- waiting owner review.

- Data quality pass rate.

- Data quality fail rate.

- Scale eligible data ratio.

- Dashboard reliability status.

Không có Data Quality Dashboard thì PACK-07 không đủ điều kiện scale governance.

## 43.7. Risk Dashboard

Risk Dashboard hiển thị các vấn đề ảnh hưởng scale/stop.

Các nhóm rủi ro tối thiểu:

- Complaint increase.

- Claim/safety incident.

- Privacy incident.

- Spam/comment abuse increase.

- Payment fail increase.

- COD fail increase.

- Delivery fail increase.

- Refund/return increase.

- Sale Lock impacted SKU.

- Stockout/not sellable.

- Fulfillment overload.

- AI handoff overload.

- Human takeover overload.

- Attribution conflict spike.

- Data mismatch.

- Dashboard data quality fail.

- Owner decision waiting.

Risk Dashboard là đầu vào bắt buộc cho Stop/Pause Gate.

## 44. KPI GOVERNANCE PRINCIPLES

## 44.1. KPI phải có định nghĩa chính thức

Mỗi KPI trong PACK-07 phải có:

- Tên KPI.

- Mục đích sử dụng.

- Công thức logic.

- Tử số.

- Mẫu số.

- Data source.

- Owner source.

- Time window.

- Revenue basis nếu có.

- Conversion basis nếu có.

- Data quality requirement.

- Exclusion rule.

- Attribution model nếu có.

- Decision usage.

- Warning condition.

Không được dùng KPI không có định nghĩa.

Không được dùng cùng một tên KPI cho nhiều cách tính khác nhau mà không ghi rõ basis.

## 44.2. KPI phải phân biệt observation và decision

Observation KPI dùng để quan sát.

Decision KPI dùng để ra quyết định scale/stop.

Ví dụ:

- Total comments là observation KPI.

- Qualified conversation rate là decision-support KPI.

- Verified revenue ROAS là decision KPI.

- Gross order ROAS chỉ là observation nếu policy không cho phép dùng để scale.

- waiting revenue là observation KPI.

- Data quality pass rate là decision KPI.

- Attribution conflict rate là decision KPI.

- COD fail rate là decision KPI.

- Complaint rate là decision KPI.

Không được dùng observation KPI để scale nếu decision KPI chưa đạt.

## 44.3. KPI phải đi kèm time window

KPI không có time window là KPI không đủ điều kiện quyết định.

Mỗi KPI phải nêu rõ:

- Ngày bắt đầu.

- Ngày kết thúc.

- Timezone.

- Campaign window.

- Conversion window.

- Revenue verification window.

- Late event handling window.

- Data refresh timestamp.

Không được so sánh KPI khác time window mà không cảnh báo.

## 45. ROAS GOVERNANCE

## 45.1. ROAS phải khai báo revenue basis

ROAS không được hiển thị chung chung.

Các loại ROAS có thể tồn tại:

- ROAS by Gross Order Value.

- ROAS by Confirmed Order Value.

- ROAS by Paid Revenue.

- ROAS by COD Successful Revenue.

- ROAS by Delivered Revenue.

- ROAS by Verified Revenue.

- ROAS by Net Verified Revenue.

- ROAS by Accounting Reconciled Revenue nếu policy cho phép.

ROAS dùng cho scale production phải là ROAS theo revenue basis được owner phê duyệt.

Mặc định, ROAS chính thức của PACK-07 phải dựa trên Verified Revenue hoặc Net Verified Revenue nếu policy yêu cầu.

## 45.2. ROAS không được tính khi thiếu spend hợp lệ

Không có Ads spend source hợp lệ thì không tính ROAS.

Spend hợp lệ phải có:

- Source được owner phê duyệt.

- Campaign/adset/ad identity.

- Time window.

- Currency.

- Spend amount.

- Account/Page/Business context.

- Import/sync status.

- Evidence snapshot.

- Data quality status.

Spend nhập tay chưa reconcile không được dùng cho ROAS PASS nếu owner policy không cho phép.

## 45.3. ROAS không được tính khi thiếu attribution

ROAS theo nguồn chỉ được tính khi attribution pass.

Nếu revenue đã verified nhưng chưa biết thuộc nguồn nào, doanh thu đó không được đưa vào ROAS theo nguồn.

Trạng thái bắt buộc:

## VERIFIED_REVENUE_WITHOUT_ATTRIBUTION — SOURCE_ROAS_NOT_ALLOWED

Nếu có attribution conflict chưa xử lý:

## ATTRIBUTION_CONFLICT — SOURCE_ROAS_BLOCKED

## 45.4. ROAS không được tính khi revenue chưa verified

Các lớp không dùng cho ROAS chính thức nếu owner chưa phê duyệt:

- Quote value.

- Cart draft value.

- Gross order value.

- Confirmed order value.

- Payment waiting value.

- COD waiting value.

- Revenue waiting verification.

- Revenue with unresolved refund/return.

- Revenue with unresolved attribution conflict.

- Revenue with data quality fail.

Dashboard có thể hiển thị các lớp này dưới dạng pipeline, nhưng phải ghi rõ không phải ROAS chính thức.

## 45.5. ROAS phải có trạng thái tin cậy

Các trạng thái ROAS tối thiểu:

- ROAS_RAW.

- ROAS_PIPELINE_ONLY.

- ROAS_waiting_VERIFICATION.

- ROAS_DATA_QUALITY_WARNING.

- ROAS_ATTRIBUTION_CONFLICT.

- ROAS_VERIFIED.

- ROAS_NET_VERIFIED.

- ROAS_ACCOUNTING_RECONCILED nếu có.

- ROAS_NOT_RELIABLE.

- ROAS_SCALE_ELIGIBLE.

- ROAS_SCALE_BLOCKED.

Chỉ ROAS_VERIFIED, ROAS_NET_VERIFIED hoặc trạng thái owner-approved mới được đưa vào Scale Gate.

## 46. CPA GOVERNANCE

## 46.1. CPA phải khai báo conversion basis

CPA không được hiển thị chung chung.

Các loại CPA tối thiểu:

- Cost per comment.

- Cost per qualified comment.

- Cost per Messenger conversation.

- Cost per qualified conversation.

- Cost per quote request.

- Cost per quote generated.

- Cost per cart draft.

- Cost per customer confirmation.

- Cost per official order.

- Cost per payment confirmed order.

- Cost per COD successful order.

- Cost per delivered order.

- Cost per verified order.

- Cost per repeat purchase.

- Cost per verified revenue event.

CPA dùng cho scale phải dùng conversion basis được owner phê duyệt.

## 46.2. CPA tầng thấp không đủ để scale

CPA comment thấp không có nghĩa campaign tốt.

CPA inbox thấp không có nghĩa doanh thu tốt.

CPA quote thấp không có nghĩa order tốt.

CPA order thấp không có nghĩa revenue verified tốt.

Scale production phải ưu tiên CPA ở tầng:

- Official order.

- Payment/COD confirmed order.

- Verified order.

- Verified revenue.

Nếu chỉ có CPA comment/inbox tốt nhưng quote-to-order hoặc order-to-verified revenue thấp, Scale Gate phải cảnh báo.

## 46.3. CPA phải có data quality pass

CPA không được dùng cho decision nếu:

- Spend chưa hợp lệ.

- Event trùng chưa xử lý.

- Spam/bot chưa loại.

- Comment chưa qualified.

- Messenger thread thiếu trace.

- Quote/order linkage thiếu.

- Official order thiếu.

- Verified revenue thiếu nếu dùng CPA verified.

- Attribution conflict chưa xử lý.

- Time window không rõ.

## 47. AOV GOVERNANCE

## 47.1. AOV phải khai báo order/revenue basis

AOV có thể tính theo nhiều basis:

- AOV by cart draft.

- AOV by confirmed order.

- AOV by paid order.

- AOV by COD successful order.

- AOV by delivered order.

- AOV by verified order.

- AOV by net verified revenue.

AOV by cart draft không phải AOV doanh thu.

AOV by quote không phải AOV doanh thu.

AOV dùng cho scale phải dựa trên official order/verified revenue theo owner policy.

## 47.2. AOV phải tách theo nguồn nếu attribution pass

AOV theo campaign/adset/ad chỉ được hiển thị khi attribution pass.

Nếu attribution conflict chưa xử lý, AOV theo nguồn phải bị chặn.

AOV tổng có thể hiển thị nếu revenue verified, nhưng AOV theo nguồn cần attribution rõ.

## 47.3. AOV phải cảnh báo khi lệch do đơn bất thường

AOV có thể bị méo bởi:

- Đơn số lượng lớn bất thường.

- Đơn B2B.

- Đơn quà tặng.

- Đơn nội bộ.

- Đơn test.

- Đơn hoàn/hủy một phần.

- Đơn combo đặc biệt.

- Đơn từ Diamond/referral.

- Đơn từ CRM rebuy.

- Đơn từ live chương trình đặc biệt.

Dashboard phải có outlier warning nếu AOV bị ảnh hưởng bởi đơn bất thường.

## 48. CORE KPI REGISTRY

## 48.1. Nhóm KPI đầu phễu Ads

KPI | Ý nghĩa | Decision usage

Spend | Chi phí quảng cáo | Cần cho ROAS/CPA

Impression | Lượt hiển thị | Quan sát reach đầu phễu

Reach | Số người tiếp cận | Quan sát độ phủ

CPM | Chi phí 1.000 hiển thị | Tối ưu media

Click | Lượt click | Quan sát quan tâm ban đầu

CTR | Tỷ lệ click | Đánh giá creative/offer

CPC | Chi phí click | Tối ưu traffic

Các KPI này không đủ để scale nếu không có conversion và revenue.

## 48.2. Nhóm KPI comment/Messenger

KPI | Ý nghĩa | Decision usage

Total Comments | Tổng comment | Observation only

Qualified Comments | Comment đủ điều kiện | Decision-support

Spam Comment Rate | Tỷ lệ comment rác | Stop/Pause risk

Comment-to-Messenger Rate | Tỷ lệ kéo private | Đánh giá PACK-06 routing

Messenger Conversations | Số hội thoại | Observation

Qualified Conversations | Hội thoại đủ chất lượng | Decision-support

Conversation-to-Quote Rate | Chuyển tư vấn sang quote | Đánh giá AI/offer

Complaint Conversation Rate | Tỷ lệ khiếu nại | Stop/Pause risk

Không được dùng Total Comments làm cơ sở scale chính thức.

## 48.3. Nhóm KPI quote/order/revenue

KPI | Ý nghĩa | Decision usage

Quote Request Rate | Tỷ lệ yêu cầu báo giá | Đánh giá nhu cầu

Quote Generated Rate | Tỷ lệ tạo quote thành công | Đánh giá Commerce Runtime

Quote bị chặn Rate | Tỷ lệ quote bị chặn | Cảnh báo stock/sellable/sale lock

Quote-to-Order Rate | Tỷ lệ quote thành order | Decision-support

Official Order Count | Số đơn chính thức | Decision-support

Order-to-Payment Rate | Tỷ lệ đơn thanh toán | Decision KPI

COD Success Rate | Tỷ lệ COD thành công | Decision KPI

Order-to-Verified-Revenue Rate | Tỷ lệ đơn thành revenue verified | Decision KPI

Verified Revenue | Doanh thu đủ điều kiện | ROAS basis

Net Verified Revenue | Doanh thu verified sau điều chỉnh | ROAS basis nếu policy dùng

## 48.4. Nhóm KPI quality/risk

KPI | Ý nghĩa | Decision usage

Data Quality Pass Rate | Tỷ lệ dữ liệu đạt | Scale Gate

Attribution Conflict Rate | Tỷ lệ conflict nguồn | Scale/Review Gate

Missing Linkage Rate | Tỷ lệ đứt trace | Scale Block

Payment Fail Rate | Tỷ lệ payment fail | Stop/Pause

COD Fail Rate | Tỷ lệ COD fail | Stop/Pause

Refund/Return Rate | Tỷ lệ hoàn/đổi/trả | Stop/Pause

Complaint Rate | Tỷ lệ khiếu nại | Stop/Pause

Privacy Incident Count | Số sự cố privacy | Stop/Block

Claim Incident Count | Số sự cố claim | Stop/Block

Sale Lock Impact Count | Số SKU/order bị sale lock ảnh hưởng | Stop/Block

Fulfillment Overload Signal | Tín hiệu quá tải vận hành | Scale Block

## 49. SCALE GATE GOVERNANCE

## 49.1. Scale Gate là điều kiện bắt buộc trước khi tăng ngân sách

Không được scale quảng cáo chỉ vì nhiều comment.

Không được scale quảng cáo chỉ vì inbox tăng.

Không được scale quảng cáo chỉ vì live đông.

Không được scale quảng cáo chỉ vì ROAS raw đẹp.

Không được scale quảng cáo nếu revenue chưa verified.

Không được scale quảng cáo nếu data quality chưa đạt.

Không được scale quảng cáo nếu owner chưa phê duyệt hoặc rule automation chưa được khóa.

Scale Gate là chốt kiểm soát bắt buộc trước mọi quyết định tăng ngân sách, mở rộng campaign, mở rộng adset, mở thêm page, mở thêm live boost hoặc nhân bản creative ở quy mô lớn.

## 49.2. Điều kiện Scale Gate tối thiểu

Scale Gate chỉ được PASS khi các nhóm điều kiện sau đạt:

- Ads spend source hợp lệ.

- Campaign/adset/ad identity rõ.

- Channel event từ PACK-06 hợp lệ.

- AI event từ PACK-05 hợp lệ nếu đo AI-assisted conversion.

- Quote/order linkage hợp lệ.

- Official order hợp lệ.

- Verified revenue đạt.

- Refund/cancel/return đã xử lý.

- Attribution rule pass.

- Data quality pass.

- ROAS đạt ngưỡng owner policy.

- CPA đạt ngưỡng owner policy.

- AOV đạt ngưỡng owner policy nếu dùng.

- COD/payment fail trong giới hạn.

- Refund/return trong giới hạn.

- Complaint trong giới hạn.

- AI handoff không quá tải.

- CSKH không quá tải.

- Warehouse/fulfillment đáp ứng được.

- Stock/sellable đủ điều kiện.

- Sale Lock không ảnh hưởng sản phẩm hero.

- Privacy/claim incident không mở.

- Dashboard reliability đạt.

- Owner approval có hiệu lực.

Thiếu bất kỳ điều kiện P0 nào thì Scale Gate không được PASS.

## 49.3. Scale Gate status

Các trạng thái Scale Gate tối thiểu:

- SCALE_NOT_EVALUATED.

- SCALE_DATA_waiting.

- SCALE_DATA_QUALITY_FAIL.

- SCALE_ATTRIBUTION_BLOCKED.

- SCALE_REVENUE_NOT_VERIFIED.

- SCALE_STOCK_BLOCKED.

- SCALE_SALE_LOCK_BLOCKED.

- SCALE_FULFILLMENT_BLOCKED.

- SCALE_RISK_BLOCKED.

- SCALE_OWNER_REVIEW_REQUIRED.

- SCALE_ALLOWED_LIMITED.

- SCALE_ALLOWED.

- SCALE_APPROVED_BY_OWNER.

- SCALE_EXECUTED nếu owner policy cho phép ghi nhận execution.

PACK-07 không được tự chuyển sang SCALE_EXECUTED nếu thiếu owner decision hoặc execution evidence từ owner vận hành Ads.

## 49.4. Scale Allowed Limited

Scale Allowed Limited dùng khi dữ liệu đủ tốt nhưng còn giới hạn vận hành.

Ví dụ:

- ROAS đạt nhưng stock không đủ cho scale lớn.

- CPA đạt nhưng CSKH gần quá tải.

- Verified revenue đạt nhưng COD fail bắt đầu tăng.

- Campaign tốt nhưng chỉ tốt ở một page/live cụ thể.

- AOV cao nhưng do một số đơn lớn bất thường.

- Data quality pass nhưng sample size còn nhỏ.

- Fulfillment đáp ứng được mức tăng nhỏ, chưa đáp ứng tăng lớn.

Trạng thái này chỉ cho phép owner xét scale có giới hạn, không tự động mở rộng mạnh.

## 49.5. Scale bị chặn

Scale bị chặn bắt buộc khi:

- Data quality fail.

- ROAS chưa verified.

- Attribution conflict chưa xử lý.

- CPA vượt ngưỡng.

- COD/payment fail cao.

- Refund/return cao.

- Complaint tăng.

- Privacy/claim incident mở.

- Hero SKU hết hàng.

- Hero SKU not sellable.

- Sale Lock/Recall ảnh hưởng SKU.

- Fulfillment quá tải.

- AI/human handoff quá tải.

- Owner approval thiếu.

- Dashboard mismatch.

- Spend source không hợp lệ.

- Order/revenue linkage thiếu.

## 50. STOP / PAUSE GATE GOVERNANCE

## 50.1. Stop/Pause Gate bảo vệ hệ thống

Stop/Pause Gate dùng để dừng, pause, giảm ngân sách hoặc yêu cầu owner review khi chiến dịch có rủi ro.

Stop/Pause Gate không chỉ dựa vào ROAS.

Một chiến dịch có ROAS tốt vẫn có thể phải pause nếu:

- Sale Lock ảnh hưởng sản phẩm.

- Sản phẩm hết hàng.

- Khiếu nại tăng.

- Claim/privacy incident xuất hiện.

- COD fail cao.

- Fulfillment quá tải.

- AI/human handoff quá tải.

- Data quality fail.

- Attribution conflict quá cao.

- Revenue mismatch.

- Owner yêu cầu.

## 50.2. Điều kiện Stop/Pause Gate tối thiểu

Stop/Pause Gate phải được kích hoạt khi có một trong các nhóm sau:

- ROAS không đủ tin cậy.

- ROAS tụt dưới ngưỡng owner policy.

- CPA vượt ngưỡng owner policy.

- Conversion tụt bất thường.

- Comment spam tăng cao.

- Qualified conversation giảm mạnh.

- Quote-to-order tụt bất thường.

- Payment fail tăng cao.

- COD fail tăng cao.

- Delivery fail tăng cao.

- Refund/return tăng cao.

- Complaint tăng cao.

- Fulfillment quá tải.

- AI Advisor quá tải hoặc fail-closed tăng mạnh.

- Human takeover quá tải.

- Hero SKU hết hàng.

- Hero SKU not sellable.

- Sale Lock/Recall ảnh hưởng sản phẩm.

- Data mismatch.

- Attribution conflict tăng.

- Privacy incident.

- Claim/safety incident.

- App/page/webhook/channel incident.

- Owner yêu cầu.

## 50.3. Stop/Pause Gate status

Các trạng thái tối thiểu:

- STOP_NOT_EVALUATED.

- STOP_MONITORING.

- PAUSE_RECOMMENDED.

- PAUSE_REQUIRED.

- STOP_RECOMMENDED.

- STOP_REQUIRED.

- BUDGET_REDUCTION_RECOMMENDED.

- OWNER_REVIEW_REQUIRED.

- RISK_INCIDENT_OPEN.

- DATA_QUALITY_STOP.

- STOCK_STOP.

- SALE_LOCK_STOP.

- FULFILLMENT_STOP.

- PRIVACY_CLAIM_STOP.

- OWNER_STOPPED.

- RESUME_REVIEW_REQUIRED.

- RESUME_ALLOWED.

- RESUME_APPROVED_BY_OWNER.

PACK-07 không tự resume campaign nếu chưa có owner decision hoặc rule automation được phê duyệt.

## 50.4. Pause không xóa dữ liệu đo lường

Khi campaign bị pause/stop, PACK-07 vẫn phải giữ dữ liệu:

- Spend trước pause.

- Funnel trước pause.

- Revenue waiting.

- Late payment/COD/delivery event.

- Refund/return event.

- Complaint event.

- Attribution result.

- Data quality result.

- Owner decision.

- Resume review evidence.

Pause/stop không được làm mất trace.

## 51. OWNER DECISION GOVERNANCE

## 51.1. Owner Decision là điều kiện kiểm soát cuối

PACK-07 có thể đề xuất.

Owner quyết định.

Các quyết định bắt buộc cần owner hoặc delegated owner approval:

- Scale budget.

- Reduce budget.

- Pause campaign.

- Stop campaign.

- Resume campaign.

- Change attribution model.

- Change revenue basis.

- Accept manual spend.

- Accept manual revenue correction.

- Accept unresolved warning.

- Override scale block nếu policy cho phép.

- Approve experimental campaign.

- Approve creative with risk warning.

- Approve new source/channel measurement.

- Approve dashboard for production decision.

Không có owner decision thì không được gọi Scale Approved.

## 51.2. Owner Decision phải có evidence

Mỗi quyết định owner phải có:

- Decision type.

- Decision scope.

- Campaign/adset/ad/page/live scope.

- Time window.

- Dashboard snapshot.

- Data quality result.

- ROAS/CPA/AOV basis.

- Attribution status.

- Revenue verification status.

- Risk status.

- Stock/sellable status.

- Fulfillment status.

- Reason.

- Owner identity.

- Timestamp.

- Effective period.

- Review date nếu cần.

- Audit/correlation.

Không có evidence thì quyết định không được dùng cho PASS.

## 51.3. Owner override phải bị kiểm soát

Owner có thể override một số cảnh báo nếu policy cho phép, nhưng không được override các chặn P0 tuyệt đối.

Các chặn không được override nếu chưa có owner policy cấp cao:

- Privacy incident chưa xử lý.

- Claim/safety incident nghiêm trọng.

- Sale Lock/Recall active.

- Not sellable SKU.

- Revenue chưa verified nhưng vẫn gọi ROAS chính thức.

- Spend source không hợp lệ nhưng vẫn scale.

- Attribution conflict chưa xử lý nhưng vẫn công bố source ROAS chính thức.

- Payment/COD fail nghiêm trọng.

- Fulfillment không đáp ứng.

- Data quality fail nghiêm trọng.

Override phải để lại audit và evidence.

## 52. SCALE DECISION MATRIX

## 52.1. Matrix quyết định scale

Điều kiện | Trạng thái | Quyết định

ROAS verified đạt, CPA đạt, data quality pass, stock đủ, no risk | Healthy | Scale Allowed

ROAS verified đạt nhưng stock giới hạn | Stock constrained | Scale Allowed Limited

ROAS raw đẹp nhưng revenue chưa verified | waiting | Scale bị chặn

CPA comment tốt nhưng order thấp | Funnel weak | Scale bị chặn / Optimize

Quote cao nhưng order thấp | Offer/order issue | Owner Review

Order cao nhưng payment/COD fail cao | Payment/COD risk | Pause/Review

Revenue tốt nhưng complaint tăng | Risk rising | Pause/Review

Sale Lock ảnh hưởng SKU hero | bị chặn | Stop/Pause Required

Attribution conflict cao | Measurement unreliable | Scale bị chặn

Data quality fail | Unreliable | Scale bị chặn

Fulfillment quá tải | Operational constraint | Scale bị chặn

Privacy/claim incident | Critical risk | Stop Required

## 52.2. Matrix quyết định stop/pause

Tín hiệu | Mức độ | Quyết định

ROAS tụt nhẹ nhưng data quality pass | Medium | Review/Optimize

ROAS tụt mạnh | High | Pause Recommended

CPA vượt ngưỡng | High | Pause/Reduce Budget

COD fail tăng mạnh | High | Pause Required

Complaint tăng mạnh | High | Pause Required

Privacy incident | Critical | Stop Required

Claim/safety incident | Critical | Stop Required

Sale Lock/Recall | Critical | Stop Required

Stockout hero SKU | Critical | Stop/Pause Required

Fulfillment overload | High | Scale bị chặn / Pause

Data mismatch nghiêm trọng | High | Decision bị chặn

Attribution conflict chưa xử lý | Medium/High | Source ROAS bị chặn

Owner yêu cầu dừng | Critical | Stop Required

## 53. DATA RELIABILITY WARNING

## 53.1. Dashboard phải cảnh báo khi dữ liệu không đủ tin cậy

Các cảnh báo bắt buộc:

- Revenue not verified.

- Spend not reconciled.

- Attribution conflict.

- Missing campaign mapping.

- Missing channel trace.

- Missing quote/order linkage.

- Duplicate events detected.

- Spam/bot suppression high.

- Payment waiting high.

- COD waiting high.

- Refund/return waiting.

- Complaint spike.

- Stock/sellable bị chặn.

- Sale Lock active.

- Dashboard refresh delayed.

- Late event window not closed.

- Owner review waiting.

- Sample size too small.

- Manual data included.

- Accounting reconcile waiting nếu có dùng.

Không được dùng màu sắc/trạng thái “tốt” cho KPI có cảnh báo P0.

## 53.2. Warning phải gắn vào từng KPI

Không chỉ cảnh báo chung ở đầu dashboard.

Mỗi KPI bị ảnh hưởng phải có warning riêng.

Ví dụ:

- ROAS bị ảnh hưởng bởi revenue waiting.

- CPA bị ảnh hưởng bởi duplicate comments.

- AOV bị ảnh hưởng bởi outlier order.

- Quote-to-order bị ảnh hưởng bởi Sale Lock.

- Order-to-revenue bị ảnh hưởng bởi COD waiting.

- Campaign performance bị ảnh hưởng bởi missing attribution.

- Live revenue bị ảnh hưởng bởi thiếu live session trace.

- CRM revenue bị ảnh hưởng bởi conflict với paid remarketing.

## 54. CAMPAIGN / ADSET / AD DECISION GOVERNANCE

## 54.1. Campaign level

Campaign-level decision được dùng để quyết định ngân sách tổng.

Campaign không được scale nếu:

- Campaign spend source chưa hợp lệ.

- Campaign attribution chưa pass.

- Campaign verified revenue chưa đạt.

- Campaign data quality fail.

- Campaign có risk incident.

- Campaign đẩy SKU not sellable.

- Campaign fulfillment không đáp ứng.

- Campaign owner approval thiếu.

## 54.2. Adset level

Adset-level decision dùng để tối ưu nhóm đối tượng, placement, ngân sách nhánh.

Adset không được đánh giá chỉ bằng CTR/CPC.

Adset phải đo đến ít nhất:

- Qualified conversation.

- Quote request.

- Official order.

- Verified revenue nếu đủ thời gian.

- CPA by selected conversion basis.

- ROAS by approved revenue basis.

- Data quality.

- Attribution status.

## 54.3. Ad creative level

Creative-level decision dùng để đánh giá thông điệp, hình ảnh, live hook, offer, CTA.

Creative tốt không chỉ là CTR cao.

Creative phải kiểm tra:

- Comment quality.

- Qualified conversation.

- Claim/safety risk.

- Privacy risk.

- Complaint signal.

- Product expectation mismatch.

- Quote/order conversion.

- Verified revenue contribution.

- Return/refund/complaint after purchase.

Creative tạo nhiều comment nhưng nhiều hiểu nhầm/claim risk không được scale mạnh.

## 55. LIVE / GOLDEN HOUR / PROGRAM MEASUREMENT GOVERNANCE

## 55.1. Live measurement phải có live session identity

Live performance chỉ được đo khi có live session identity hợp lệ.

Live Dashboard phải phân biệt:

- Live organic reach.

- Paid live boost nếu có.

- Live comment.

- Qualified live comment.

- Live-to-Messenger handoff.

- Live quote request.

- Live official order.

- Live verified revenue.

- Live complaint.

- Live fulfillment pressure.

- Live stock/sellable status.

- Live attribution conflict.

Không có live session identity thì không được gán revenue về phiên live.

## 55.2. Golden Hour measurement phải theo runtime

Nếu đo chương trình Giờ Vàng, PACK-07 không được tự xác nhận giá/chương trình.

PACK-07 chỉ consume program context từ owner runtime.

Dashboard phải phân biệt:

- Session status.

- Program price basis.

- QuoteSnapshot generated.

- Official order created.

- Payment/COD confirmed.

- Verified revenue.

- Quota/stock constraint nếu owner cung cấp.

- Early access effect nếu có runtime.

- Member/Diamond effect nếu policy cho phép.

- Revenue excluded do hết phiên/hết hiệu lực quote nếu có.

Không được tự gọi một order là Golden Hour revenue nếu thiếu program/session linkage.

## 55.3. 24/7 program measurement phải theo runtime

Nếu đo chương trình 24/7, PACK-07 chỉ consume program context từ Commerce Runtime.

PACK-07 không tự tính mức giảm.

PACK-07 không tự xác định khách đủ quyền lợi.

PACK-07 chỉ đo:

- Program-attributed quote.

- Program-attributed order.

- Program-attributed verified revenue.

- Program conflict với member/Diamond nếu owner event cung cấp.

- Program conversion.

- Program ROAS nếu attribution pass.

## 56. CRM / REBUY / MEMBER MEASUREMENT GOVERNANCE

## 56.1. CRM revenue phải tách khỏi Ads revenue nếu policy yêu cầu

CRM revenue đo mua lại, chăm sóc sau mua, member retention, seasonal suggestion, combo, quà sức khỏe hoặc upgrade.

CRM revenue không được tự cộng vào Ads revenue nếu attribution policy không cho phép.

Dashboard phải phân biệt:

- New customer from Ads.

- Existing customer from CRM.

- Member rebuy.

- Diamond/referral revenue.

- Ads-assisted rebuy.

- CRM-assisted rebuy.

- Mixed-source rebuy.

- Owner-review attribution.

## 56.2. Member performance không được tự tính quyền lợi

PACK-07 có thể đo performance theo member class nếu runtime cung cấp.

PACK-07 không tự xác định:

- Member tier.

- Member eligibility.

- Member discount.

- Upgrade/downgrade.

- Maintain tier.

- Benefit status.

Các giá trị này phải đến từ Commerce/Member owner.

PACK-07 chỉ đo outcome theo runtime value hợp lệ.

## 57. PRODUCT / STOCK / SELLABLE DECISION GOVERNANCE

## 57.1. Product performance phải gắn với sellable reality

Một sản phẩm có thể có demand cao nhưng không scale được nếu:

- Hết hàng.

- Not sellable.

- Sale Lock.

- Recall.

- Quality hold.

- Warehouse hold.

- Channel hold.

- Program price inactive.

- Quote unavailable.

- Fulfillment không đáp ứng.

Dashboard phải hiển thị rõ:

- Demand signal.

- Sellable capacity.

- Stock constraint.

- Sale Lock constraint.

- Fulfillment constraint.

- Verified revenue outcome.

## 57.2. Hero SKU bị chặn thì Scale Gate bị chặn

Nếu campaign đang đẩy hero SKU nhưng SKU bị chặn, Scale Gate phải bị chặn.

Không được scale campaign chỉ vì lead tốt nếu sản phẩm không đủ điều kiện bán.

Nếu cần chuyển hướng sang sản phẩm thay thế, đó là quyết định của owner/Commerce/AI Advisor, không phải PACK-07 tự quyết định.

PACK-07 chỉ ghi nhận:

- Hero SKU bị chặn.

- Alternative product conversion nếu owner event cung cấp.

- Revenue attribution sau khi product substitution được owner xử lý.

- Impact on campaign performance.

## 58. FULFILLMENT / CSKH CAPACITY GOVERNANCE

## 58.1. Scale phải xét năng lực vận hành

Ads có thể tạo nhu cầu nhanh hơn năng lực xử lý.

Scale Gate phải kiểm tra:

- AI Advisor load.

- Human takeover capacity.

- CSKH capacity.

- Order processing capacity.

- Warehouse picking/packing capacity.

- Delivery handoff capacity.

- COD handling capacity.

- Complaint handling capacity.

- Payment reconciliation capacity.

- Inventory availability.

- Production replenishment signal nếu owner cung cấp.

Nếu vận hành quá tải, Scale Gate phải block hoặc chỉ cho Scale Allowed Limited.

## 58.2. Fulfillment overload không được quy lỗi cho Ads hoặc AI đơn giản

Nếu funnel tốt nhưng fulfillment fail, dashboard phải phân loại đúng nguyên nhân.

Ví dụ:

- Ads tạo qualified lead tốt.

- AI quote/order tốt.

- Order tăng.

- Fulfillment chậm.

- COD fail tăng do giao trễ.

- Complaint tăng.

Trường hợp này không được kết luận Ads kém hoặc AI kém nếu bằng chứng chỉ ra fulfillment là điểm nghẽn.

## 59. OWNER REVIEW QUEUE

## 59.1. Các trường hợp bắt buộc vào Owner Review Queue

PACK-07 phải đưa vào Owner Review Queue khi:

- Attribution conflict chưa có rule.

- Revenue basis chưa được duyệt.

- Manual spend được nhập.

- Manual correction xuất hiện.

- Data quality warning ảnh hưởng decision.

- ROAS verified đạt nhưng sample size nhỏ.

- AOV bị outlier.

- CPA tốt nhưng complaint tăng.

- Campaign tốt nhưng stock hạn chế.

- Campaign tốt nhưng fulfillment gần quá tải.

- CRM/Ads/Diamond conflict.

- Live/Paid/Organic conflict.

- Claim/privacy risk.

- Scale override request.

- Resume after stop request.

- New campaign source cần phê duyệt.

- Dashboard mismatch.

- Revenue mismatch.

## 59.2. Owner Review Queue status

Các trạng thái tối thiểu:

- REVIEW_NOT_REQUIRED.

- REVIEW_REQUIRED.

- REVIEW_waiting_DATA.

- REVIEW_waiting_OWNER.

- REVIEW_APPROVED.

- REVIEW_REJECTED.

- REVIEW_ESCALATED.

- REVIEW_EXPIRED.

- REVIEW_LOCKED.

- REVIEW_REOPENED.

Không có REVIEW_APPROVED thì không được coi owner đã phê duyệt.

## 60. DECISION EVIDENCE REQUIREMENTS

## 60.1. Evidence cho Scale Decision

Scale Decision phải có evidence:

- Dashboard snapshot.

- Time window.

- Spend source.

- Campaign/adset/ad identity.

- Channel trace result.

- AI event trace result nếu có.

- Quote/order linkage result.

- Verified revenue result.

- Revenue basis.

- Attribution model.

- Attribution conflict status.

- Data quality result.

- ROAS result.

- CPA result.

- AOV result nếu dùng.

- Stock/sellable status.

- Sale Lock status.

- Fulfillment capacity status.

- Risk status.

- Owner approval.

- Effective scale scope.

Không có evidence thì không có Scale PASS.

## 60.2. Evidence cho Stop/Pause Decision

Stop/Pause Decision phải có evidence:

- Trigger reason.

- Campaign/adset/ad/page/live scope.

- Risk metric.

- Data quality state.

- Revenue state.

- Attribution state.

- Stock/sellable state.

- Sale Lock/Recall state nếu có.

- Complaint/claim/privacy state nếu có.

- Fulfillment state nếu có.

- Owner decision hoặc policy auto-stop rule.

- Timestamp.

- Review/resume condition.

Không có evidence thì không được gọi Stop/Pause Gate completed.

## 61. DASHBOARD RELEASE RULES

## 61.1. Dashboard production không được mở nếu thiếu Data Quality

Dashboard chỉ được dùng production decision khi có:

- Data quality check.

- Attribution check.

- Revenue verification check.

- Spend validation.

- Exclusion handling.

- waiting/missing data display.

- Owner review flow.

- Scale/Stop status.

- Evidence snapshot.

Dashboard chỉ hiển thị raw data thì chưa đủ để làm production decision dashboard.

## 61.2. Dashboard không được che rủi ro bằng chỉ số đẹp

Các chỉ số đẹp không được che các vấn đề P0.

Ví dụ:

- ROAS cao nhưng revenue waiting nhiều.

- CPA thấp nhưng spam comment cao.

- AOV cao nhưng do một đơn outlier.

- Quote cao nhưng order thấp.

- Order cao nhưng COD fail cao.

- Revenue cao nhưng refund/return cao.

- Live đông nhưng complaint tăng.

- Ads tốt nhưng SKU hero sale lock.

- AI conversion tốt nhưng privacy incident mở.

- CRM rebuy tốt nhưng attribution conflict chưa xử lý.

Dashboard phải hiển thị song song performance và risk.

## 62. DONE CONDITION CỦA PHẦN 3/4

## PHẦN 3/4 được xem là hoàn thành về mặt tài liệu khi đã khóa được:

- Dashboard governance principles.

- Dashboard data layers.

- Executive Measurement Dashboard.

- Ads Performance Dashboard.

- Funnel Dashboard.

- Attribution Dashboard.

- Revenue Dashboard.

- Data Quality Dashboard.

- Risk Dashboard.

- KPI governance principles.

- ROAS governance.

- CPA governance.

- AOV governance.

- Core KPI Registry.

- Scale Gate governance.

- Stop/Pause Gate governance.

- Owner Decision governance.

- Scale Decision Matrix.

- Stop/Pause Decision Matrix.

- Data Reliability Warning.

- Campaign/adset/ad decision governance.

- Live/Golden Hour/24/7 measurement governance.

- CRM/rebuy/member measurement governance.

- Product/stock/sellable decision governance.

- Fulfillment/CSKH capacity governance.

- Owner Review Queue.

- Decision evidence requirements.

- Dashboard release rules.

## PHẦN 3/4 chưa định nghĩa Smoke Matrix, Done Gate tổng thể, Fail Gate tổng thể, Release Control, Evidence Acceptance và PACK-07 Final Conclusion. Các nội dung đó thuộc PHẦN 4/4.

## KẾT LUẬN PHẦN 3/4

PACK-07 Dashboard là lớp đo lường và hỗ trợ quyết định, không phải source-of-truth nghiệp vụ.

ROAS phải có revenue basis rõ.

CPA phải có conversion basis rõ.

AOV phải có order/revenue basis rõ.

Dashboard phải phân biệt raw, clean, verified, excluded, waiting, missing và conflict data.

Scale Gate không được PASS nếu thiếu data quality, verified revenue, attribution, stock/sellable, fulfillment readiness hoặc owner approval.

Stop/Pause Gate phải kích hoạt khi ROAS không tin cậy, CPA xấu, conversion tụt, complaint tăng, COD/payment fail cao, stock/sellable bị chặn, Sale Lock/Recall ảnh hưởng, privacy/claim incident mở hoặc owner yêu cầu.

Nguyên tắc khóa của PHẦN 3/4 là:

Dashboard không thay thế source-of-truth.

ROAS đẹp nhưng dữ liệu chưa sạch thì không được scale.

Comment nhiều không đồng nghĩa bán tốt.

Quote nhiều không đồng nghĩa có doanh thu.

Order nhiều không đồng nghĩa revenue verified.

Scale phải đi qua Data Quality, Verified Revenue, Attribution, Stock/Sellable, Fulfillment và Owner Decision.

## 63. MỤC TIÊU CỦA PHẦN 4/4

## PHẦN 4/4 thiết lập lớp kiểm chứng cuối cho PACK-07, bao gồm Smoke Matrix, Evidence, Done Gate, Fail Gate, Release Control và kết luận hoàn tất tài liệu.

Mục tiêu của phần này là khóa rõ:

- PACK-07 phải được kiểm thử bằng smoke nào.

- Điều kiện nào được xem là đo lường hợp lệ.

- Điều kiện nào được xem là ROAS đáng tin cậy.

- Điều kiện nào được xem là đủ dữ liệu để Scale Gate.

- Điều kiện nào buộc phải Fail Gate.

- Evidence nào bắt buộc trước khi gọi PASS.

- Ai có quyền review và sign-off.

- Trạng thái nào được phép gọi là hoàn tất tài liệu.

- Trạng thái nào chưa được gọi là Production Ready.

- Điều kiện nào cần đạt trước khi PACK-07 được dùng cho quyết định scale production.

## PHẦN 4/4 không thiết kế code, API, database, UI hoặc schema chi tiết.

## PHẦN 4/4 là lớp governance kiểm soát chất lượng triển khai PACK-07.

## 64. NGUYÊN TẮC SMOKE CỐT LÕI

## 64.1. Smoke không phải test hình thức

Smoke trong PACK-07 không phải kiểm tra dashboard có hiển thị hay không.

Smoke phải chứng minh dữ liệu đi đúng từ nguồn đến quyết định.

Smoke phải chứng minh:

- Ads spend có nguồn hợp lệ.

- Campaign/adset/ad identity được nhận diện đúng.

- Channel event đi qua PACK-06.

- AI event đi qua PACK-05.

- Quote/order/revenue đi qua owner runtime.

- Revenue chưa verified thì không tính ROAS chính thức.

- Attribution conflict được phát hiện.

- Exclusion rule hoạt động.

- Data Quality chặn được dữ liệu không đủ tin cậy.

- Scale Gate không PASS khi thiếu điều kiện.

- Stop/Pause Gate kích hoạt khi có rủi ro.

- Dashboard không che giấu waiting/missing/conflict data.

- Evidence được ghi nhận đầy đủ.

- Owner Decision được audit.

Smoke phải kiểm tra đúng nghiệp vụ, không chỉ kiểm tra màn hình.

## 64.2. Không có smoke pass thì không có ROAS PASS

PACK-07 không được gọi ROAS PASS nếu chưa có smoke pass.

Không có smoke pass thì:

- Không được dùng dashboard cho scale production.

- Không được gọi data quality là production-grade.

- Không được gọi attribution là đáng tin cậy.

- Không được gọi verified revenue là cơ sở ROAS nếu chưa test linkage.

- Không được gọi Scale Gate PASS.

- Không được gọi Release Ready.

- Không được gọi Production Ready.

Smoke pass phải đi kèm evidence accepted.

## 64.3. Smoke phải có positive path và negative path

PACK-07 phải test cả trường hợp đúng và sai.

Positive path chứng minh luồng hợp lệ đi qua được.

Negative path chứng minh hệ thống chặn đúng khi dữ liệu sai, thiếu, trùng, waiting, conflict hoặc rủi ro.

Nếu chỉ test positive path mà không test negative path, PACK-07 chưa đủ Done Gate.

## 65. SMOKE MATRIX TỔNG THỂ

## 65.1. ADS-SMK-001 — Ads Spend Source Validation Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-001

Mục tiêu | Kiểm tra Ads spend source hợp lệ trước khi tính ROAS

Input | Spend record có campaign/adset/ad/time window/currency/source

Expected result | Spend được xác thực, có evidence, được phép dùng cho ROAS nếu data quality pass

Negative path | Thiếu spend source, thiếu campaign id, spend nhập tay chưa duyệt

Fail condition | Dashboard vẫn tính ROAS chính thức khi spend source không hợp lệ

Evidence | Spend source snapshot, validation result, data quality event, audit

## 65.2. ADS-SMK-002 — Campaign / Adset / Ad Identity Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-002

Mục tiêu | Kiểm tra campaign/adset/ad identity được nhận diện và map đúng

Input | Event có campaign, adset, ad context

Expected result | Mapping rõ campaign -> adset -> ad -> time window

Negative path | Thiếu adset/ad, sai mapping, campaign không thuộc owner-approved source

Fail condition | Revenue vẫn được gán về campaign khi mapping thiếu

Evidence | Mapping snapshot, data quality result, attribution readiness result

## 65.3. ADS-SMK-003 — PACK-06 Channel Event Trace Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-003

Mục tiêu | Kiểm tra channel event Facebook/Messenger/Live phải đi qua PACK-06

Input | Comment, Messenger event, Live event

Expected result | Event có Page context, routing context, dedup, privacy/spam status, channel evidence

Negative path | Raw Facebook event chưa qua PACK-06

Fail condition | Raw event vẫn được dùng cho production ROAS

Evidence | PACK-06 channel trace, event correlation, privacy/spam status

## 65.4. ADS-SMK-004 — Comment -> Messenger Trace Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-004

Mục tiêu | Kiểm tra trace từ comment public sang Messenger/private

Input | Comment có ý định mua hoặc hỏi giá

Expected result | Comment được phân loại, route private, giữ context sang Messenger

Negative path | Messenger order không có comment-to-Messenger trace

Fail condition | Doanh thu Messenger vẫn gán về comment/live/ad khi trace đứt

Evidence | Comment event, private handoff event, Messenger event, correlation chain

## 65.5. ADS-SMK-005 — PACK-05 AI Event Trace Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-005

Mục tiêu | Kiểm tra AI Advisor event được consume đúng, không viết lại AI behavior

Input | Intent event, response event, product recommendation, quote request, handoff

Expected result | AI event có owner PACK-05, guard status, handoff status, correlation

Negative path | Dashboard tự suy luận AI success/failure không có event

Fail condition | PACK-07 tự đánh giá AI conversion khi thiếu PACK-05 trace

Evidence | AI event trace, guard result, handoff reason, correlation

## 65.6. ADS-SMK-006 — Quote Event Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-006

Mục tiêu | Kiểm tra QuoteSnapshot chỉ là funnel event, không phải revenue

Input | Quote request, QuoteSnapshot generated

Expected result | Quote được hiển thị ở tầng quote, không vào revenue

Negative path | Quote expired, quote bị chặn do sellable/stock/sale lock

Fail condition | Dashboard tính quote value vào ROAS chính thức

Evidence | Quote event, quote status, sellable/stock/sale lock result

## 65.7. ADS-SMK-007 — Cart Draft Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-007

Mục tiêu | Kiểm tra cart draft không bị tính là official order

Input | Cart draft được tạo từ quote

Expected result | Cart nằm ở funnel cart, chưa vào order/revenue

Negative path | Cart bỏ dở, cart bị sửa, cart hết hạn

Fail condition | Cart draft được tính là order hoặc revenue

Evidence | Cart event, cart status, customer confirmation status

## 65.8. ADS-SMK-008 — Official Order Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-008

Mục tiêu | Kiểm tra chỉ official order/order_code mới được tính là order

Input | Customer confirmation và owner Order tạo order_code

Expected result | ORDER_EVENT hợp lệ, có order identity

Negative path | Khách nhắn “lấy 2 hộp” nhưng chưa có order_code

Fail condition | Dashboard gọi Messenger text/cart draft là order chính thức

Evidence | ORDER_EVENT, order_code, customer confirmation, audit

## 65.9. ADS-SMK-009 — Payment / COD Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-009

Mục tiêu | Kiểm tra payment/COD status phải đến từ owner runtime

Input | Payment confirmed hoặc COD success

Expected result | Payment/COD event hợp lệ, không tự xác nhận từ tin nhắn

Negative path | Ảnh chuyển khoản chưa đối chiếu, COD waiting, payment failed

Fail condition | Dashboard tính paid revenue khi chưa có owner payment confirmation

Evidence | Payment/COD event, reconciliation status, owner confirmation

## 65.10. ADS-SMK-010 — Verified Revenue Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-010

Mục tiêu | Kiểm tra Verified Revenue là điều kiện cho ROAS chính thức

Input | Order đạt verified revenue theo owner policy

Expected result | VERIFIED_REVENUE_EVENT được tạo, có revenue basis

Negative path | Revenue waiting, refund waiting, return waiting, complaint unresolved

Fail condition | ROAS chính thức tính từ revenue waiting

Evidence | Verified revenue event, revenue basis, exclusion result, owner confirmation

## 65.11. ADS-SMK-011 — Revenue Exclusion Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-011

Mục tiêu | Kiểm tra hủy/hoàn/đổi/COD fail/test order bị loại khỏi ROAS

Input | Order canceled/refunded/returned/COD failed/test/internal

Expected result | Revenue bị đưa vào excluded bucket, không tính ROAS

Negative path | Partial refund hoặc late return sau khi revenue đã waiting

Fail condition | Excluded revenue vẫn xuất hiện trong ROAS chính thức

Evidence | Exclusion event, reason, owner source, dashboard excluded view

## 65.12. ADS-SMK-012 — Attribution Conflict Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-012

Mục tiêu | Kiểm tra conflict Ads/CRM/Diamond/Organic/Live được phát hiện

Input | Một order có nhiều touchpoint nguồn

Expected result | ATTRIBUTION_CONFLICT được tạo, không gán nguồn chính thức khi chưa có rule

Negative path | Ads click trước, Diamond link sau; CRM trước, remarketing sau

Fail condition | Dashboard tự gán doanh thu về một nguồn khi conflict chưa xử lý

Evidence | Touchpoint chain, conflict status, attribution rule status

## 65.13. ADS-SMK-013 — Attribution Rule Applied Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-013

Mục tiêu | Kiểm tra attribution rule được áp đúng khi owner phê duyệt

Input | Conversion có đủ touchpoint, rule và verified revenue

Expected result | ATTRIBUTION_LOCKED hoặc CONFLICT_RESOLVED

Negative path | Rule thiếu, rule hết hiệu lực, rule không đúng scope

Fail condition | Attribution result không ghi rule/evidence

Evidence | Rule used, model, owner approval, attribution result

## 65.14. ADS-SMK-014 — Data Quality Gate Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-014

Mục tiêu | Kiểm tra Data Quality phải PASS trước reporting/scale

Input | Batch dữ liệu có valid, missing, duplicate, conflict, waiting

Expected result | Dữ liệu được phân lớp raw/clean/verified/excluded/waiting/missing

Negative path | Duplicate, missing campaign, missing quote/order linkage

Fail condition | Dashboard vẫn READY_FOR_DECISION khi data quality fail

Evidence | DATA_QUALITY_EVENT, DQ status, excluded/missing summary

## 65.15. ADS-SMK-015 — Dashboard Layer Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-015

Mục tiêu | Kiểm tra dashboard phân biệt raw/clean/verified/excluded/waiting

Input | Tập dữ liệu nhiều trạng thái

Expected result | Dashboard hiển thị đúng từng bucket, không gộp sai

Negative path | waiting revenue, excluded revenue, missing attribution

Fail condition | Dashboard hiển thị một số tổng doanh thu không rõ class

Evidence | Dashboard snapshot, data layer result, warning display

## 65.16. ADS-SMK-016 — ROAS Reliability Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-016

Mục tiêu | Kiểm tra ROAS chỉ đáng tin cậy khi đủ spend, attribution, verified revenue, DQ

Input | Campaign có spend và verified revenue hợp lệ

Expected result | ROAS_VERIFIED hoặc ROAS_NET_VERIFIED nếu policy dùng

Negative path | Spend thiếu, attribution conflict, revenue waiting

Fail condition | ROAS_SCALE_ELIGIBLE khi thiếu DQ/verified revenue

Evidence | Spend source, revenue basis, attribution result, DQ result

## 65.17. ADS-SMK-017 — CPA / AOV Basis Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-017

Mục tiêu | Kiểm tra CPA/AOV có conversion basis và revenue/order basis rõ

Input | Campaign có nhiều funnel stages

Expected result | CPA và AOV hiển thị đúng basis

Negative path | CPA comment bị gọi chung CPA, AOV quote bị gọi AOV doanh thu

Fail condition | KPI không khai báo basis nhưng dùng cho scale

Evidence | KPI definition, dashboard snapshot, basis label

## 65.18. ADS-SMK-018 — Scale Gate Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-018

Mục tiêu | Kiểm tra Scale Gate chỉ PASS khi đủ điều kiện

Input | Campaign có ROAS verified, CPA đạt, DQ pass, stock/sellable pass

Expected result | SCALE_ALLOWED hoặc SCALE_ALLOWED_LIMITED theo điều kiện

Negative path | ROAS raw đẹp nhưng revenue waiting; SKU hero not sellable; owner approval thiếu

Fail condition | Scale Gate PASS khi thiếu điều kiện P0

Evidence | Scale evaluation, dashboard snapshot, owner decision

## 65.19. ADS-SMK-019 — Stop / Pause Gate Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-019

Mục tiêu | Kiểm tra Stop/Pause Gate kích hoạt khi rủi ro

Input | Campaign có complaint tăng, COD fail cao, sale lock, privacy incident

Expected result | PAUSE_REQUIRED, STOP_REQUIRED hoặc OWNER_REVIEW_REQUIRED

Negative path | ROAS tốt nhưng Sale Lock active

Fail condition | Campaign vẫn Scale Allowed khi có P0 risk

Evidence | Risk dashboard, stop trigger, owner decision, audit

## 65.20. ADS-SMK-020 — Owner Decision Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-020

Mục tiêu | Kiểm tra quyết định owner được ghi nhận trước scale/stop/resume

Input | Scale/stop decision request

Expected result | Có owner decision, scope, reason, effective period, evidence

Negative path | Dashboard tự chuyển sang Scale Approved

Fail condition | Không có owner approval nhưng vẫn gọi Scale Approved

Evidence | Owner decision record, dashboard snapshot, audit

## 65.21. ADS-SMK-021 — Live / Golden Hour Measurement Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-021

Mục tiêu | Kiểm tra live/Giờ Vàng đo theo live/session/program runtime

Input | Live event, Golden Hour session, quote/order/revenue linkage

Expected result | Revenue chỉ gán live/program khi có session/program linkage hợp lệ

Negative path | Comment live không trace sang Messenger; quote hết hiệu lực

Fail condition | Dashboard gán revenue về live/Giờ Vàng khi thiếu linkage

Evidence | Live session context, program context, quote/order/revenue trace

## 65.22. ADS-SMK-022 — CRM / Diamond / Referral Conflict Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-022

Mục tiêu | Kiểm tra không tính trùng Ads, CRM, Diamond/referral

Input | Customer có Ads touchpoint, CRM event, Diamond referral bind

Expected result | Conflict được phát hiện, rule áp dụng nếu có

Negative path | Không có referral bind, CRM rebuy nhưng gán Ads

Fail condition | Một verified revenue được tính trùng nhiều nguồn

Evidence | Touchpoint chain, referral bind, CRM event, attribution result

## 65.23. ADS-SMK-023 — Stock / Sellable / Sale Lock Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-023

Mục tiêu | Kiểm tra Scale Gate bị chặn khi SKU không đủ điều kiện bán

Input | Campaign hero SKU hết hàng/not sellable/sale lock

Expected result | SCALE_STOCK_BLOCKED hoặc SCALE_SALE_LOCK_BLOCKED

Negative path | Lead tốt nhưng SKU bị chặn

Fail condition | Campaign vẫn Scale Allowed khi Sale Lock active

Evidence | Sellable status, stock status, sale lock status, scale result

## 65.24. ADS-SMK-024 — Fulfillment Capacity Smoke

Hạng mục | Nội dung

Smoke ID | ADS-SMK-024

Mục tiêu | Kiểm tra scale phải xét năng lực CSKH/kho/giao hàng

Input | Campaign có demand tăng, fulfillment gần quá tải

Expected result | SCALE_ALLOWED_LIMITED hoặc SCALE_FULFILLMENT_BLOCKED

Negative path | Fulfillment quá tải nhưng ROAS vẫn tốt

Fail condition | Scale mạnh khi fulfillment không đáp ứng

Evidence | Fulfillment signal, CSKH load, warehouse capacity, owner decision

## 66. EVIDENCE GOVERNANCE

## 66.1. Không có evidence thì không có PASS

PACK-07 không được gọi PASS nếu thiếu evidence.

Evidence là điều kiện bắt buộc cho:

- Data Quality PASS.

- ROAS PASS.

- Attribution PASS.

- Dashboard READY_FOR_DECISION.

- Scale Gate PASS.

- Stop/Pause Gate completed.

- Owner Decision approved.

- Release Ready.

Evidence không phải ảnh chụp rời rạc không có trace.

Evidence phải có nguồn, trạng thái, reviewer, scope và correlation.

## 66.2. Evidence Registry tối thiểu

PACK-07 phải có Evidence Registry cho các nhóm:

- Ads spend source evidence.

- Campaign/adset/ad mapping evidence.

- PACK-06 channel trace evidence.

- PACK-05 AI event trace evidence.

- Quote event evidence.

- Cart event evidence.

- Official order evidence.

- Payment/COD evidence.

- Delivery evidence nếu policy dùng.

- Verified revenue evidence.

- Revenue exclusion evidence.

- Attribution rule evidence.

- Attribution result evidence.

- Data quality evidence.

- Dashboard snapshot evidence.

- Scale Gate evidence.

- Stop/Pause Gate evidence.

- Owner Decision evidence.

- Risk incident evidence.

- Release review evidence.

## 66.3. Evidence status

Evidence phải có trạng thái tối thiểu:

- EVIDENCE_waiting.

- EVIDENCE_SUBMITTED.

- EVIDENCE_ACCEPTED.

- EVIDENCE_REJECTED.

- EVIDENCE_NEEDS_RETEST.

- EVIDENCE_EXPIRED.

- EVIDENCE_SUPERSEDED.

- EVIDENCE_BLOCKED.

- EVIDENCE_OWNER_REVIEW_REQUIRED.

Chỉ EVIDENCE_ACCEPTED mới được dùng cho PASS.

Evidence waiting không được dùng để gọi PASS.

Evidence rejected không được dùng cho release.

Evidence expired phải retest hoặc re-accept.

## 66.4. Evidence phải có metadata

Mỗi evidence record cần có:

- Evidence ID.

- Pack ID.

- Smoke ID hoặc Gate ID.

- Rule ID nếu áp dụng.

- Environment.

- Time window.

- Source system.

- Owner source.

- Test result.

- Data quality status.

- Reviewer.

- Review timestamp.

- Acceptance status.

- Correlation ID.

- Audit reference.

- Notes nếu có.

- Retest reference nếu có.

Không có metadata thì evidence chưa đủ chuẩn governance.

## 67. DATA QUALITY DONE GATE

## 67.1. Điều kiện Data Quality Done Gate

Data Quality Done Gate chỉ PASS khi:

- Ads spend source hợp lệ.

- Campaign/adset/ad identity hợp lệ.

- Channel event qua PACK-06.

- AI event qua PACK-05 nếu dùng.

- Event dedup pass.

- Spam/bot suppression pass.

- Privacy guard pass.

- Missing required field được xử lý.

- Quote/order linkage pass.

- Payment/COD/revenue linkage pass nếu dùng.

- Attribution conflict được xử lý hoặc đưa vào waiting/excluded.

- Revenue exclusion hoạt động.

- Dashboard phân lớp dữ liệu đúng.

- Evidence accepted.

## 67.2. Data Quality Fail Gate

Data Quality phải FAIL nếu:

- Spend source không hợp lệ.

- Campaign mapping thiếu.

- Channel trace thiếu.

- Event duplicate chưa xử lý.

- Spam/bot chưa loại.

- Privacy-bị chặn event vẫn dùng.

- Quote/order linkage đứt.

- Revenue waiting bị tính verified.

- Attribution conflict chưa xử lý nhưng vẫn gán nguồn.

- Excluded revenue vẫn đi vào ROAS.

- Dashboard không hiển thị warning.

- Evidence thiếu hoặc rejected.

## 68. ATTRIBUTION DONE GATE

## 68.1. Điều kiện Attribution Done Gate

Attribution Done Gate chỉ PASS khi:

- Attribution model được khai báo.

- Touchpoint chain có đủ dữ liệu.

- Customer/session/order linkage hợp lệ.

- Source classification rõ Paid/Organic/CRM/Diamond/Live/Mixed/Unknown.

- Conflict detection hoạt động.

- Conflict resolution rule được owner phê duyệt nếu dùng.

- Attribution result có evidence.

- Revenue không bị tính trùng nhiều nguồn.

- Dashboard hiển thị conflict/waiting/excluded đúng.

- Evidence accepted.

## 68.2. Attribution Fail Gate

Attribution phải FAIL nếu:

- Không có attribution model.

- Missing data bị gán Organic mặc định.

- Ads revenue được kết luận khi thiếu campaign/ad context.

- Diamond revenue được kết luận khi thiếu referral bind.

- CRM revenue bị gộp vào Ads revenue không có rule.

- Live revenue được gán khi thiếu live session identity.

- Một order được tính trùng nhiều nguồn.

- Conflict chưa xử lý nhưng vẫn đưa vào source ROAS.

- Attribution result thiếu evidence.

- Owner rule thiếu hoặc hết hiệu lực.

## 69. VERIFIED REVENUE DONE GATE

## 69.1. Điều kiện Verified Revenue Done Gate

Verified Revenue Done Gate chỉ PASS khi:

- Có official order/order_code.

- Có Customer Confirmation nếu policy yêu cầu.

- Có payment/COD/delivery status đạt điều kiện.

- Có revenue owner confirmation.

- Có refund/cancel/return exclusion check.

- Có complaint/risk exclusion check nếu policy yêu cầu.

- Có revenue class rõ.

- Có revenue basis rõ.

- Có attribution status.

- Có data quality pass.

- Có evidence accepted.

## 69.2. Verified Revenue Fail Gate

Verified Revenue phải FAIL nếu:

- Không có official order.

- Không có order_code.

- Payment waiting bị tính paid revenue.

- COD waiting bị tính COD success.

- Delivery waiting bị tính delivered revenue.

- Revenue waiting bị tính verified revenue.

- Refund/return/cancel chưa xử lý.

- Test/internal/sample order bị tính vào ROAS khi policy loại.

- Revenue source không phải owner runtime.

- Evidence thiếu hoặc không accepted.

## 70. DASHBOARD DONE GATE

## 70.1. Điều kiện Dashboard Done Gate

Dashboard Done Gate chỉ PASS khi:

- Dashboard phân biệt raw/clean/verified/excluded/waiting/missing/conflict.

- ROAS có revenue basis.

- CPA có conversion basis.

- AOV có order/revenue basis.

- Funnel không nhảy tầng.

- Data Quality status hiển thị rõ.

- Attribution status hiển thị rõ.

- Revenue verification status hiển thị rõ.

- Risk warning hiển thị rõ.

- Scale/Stop status hiển thị rõ.

- Owner Review Queue hoạt động.

- Dashboard snapshot evidence accepted.

## 70.2. Dashboard Fail Gate

Dashboard phải FAIL nếu:

- Gộp mọi doanh thu thành một số tổng không rõ class.

- Hiển thị ROAS chính thức khi revenue chưa verified.

- Hiển thị CPA không có conversion basis.

- Hiển thị AOV không có order/revenue basis.

- Không hiển thị waiting revenue.

- Không hiển thị excluded revenue.

- Không hiển thị attribution conflict.

- Không hiển thị data quality warning.

- Không hiển thị Sale Lock/stock/sellable risk.

- Không có evidence snapshot.

- Dashboard được dùng scale khi chưa READY_FOR_DECISION.

## 71. SCALE GATE DONE / FAIL

## 71.1. Điều kiện Scale Gate Done

Scale Gate chỉ PASS khi:

- Data Quality PASS.

- Attribution PASS.

- Verified Revenue PASS.

- ROAS đạt ngưỡng owner policy.

- CPA đạt ngưỡng owner policy.

- AOV đạt ngưỡng owner policy nếu dùng.

- COD/payment fail trong giới hạn.

- Refund/return trong giới hạn.

- Complaint trong giới hạn.

- Privacy/claim incident không mở.

- SKU hero sellable.

- Stock đủ hoặc có constraint rõ.

- Sale Lock/Recall không ảnh hưởng SKU.

- Fulfillment/CSKH đáp ứng.

- AI/human handoff không quá tải.

- Dashboard READY_FOR_DECISION.

- Owner approval có hiệu lực.

- Scale evidence accepted.

## 71.2. Scale Gate Fail

Scale Gate phải FAIL hoặc bị chặn khi:

- Data quality fail.

- Attribution conflict unresolved.

- Revenue chưa verified.

- ROAS không đáng tin cậy.

- CPA vượt ngưỡng.

- COD/payment fail cao.

- Refund/return cao.

- Complaint tăng cao.

- Privacy/claim incident mở.

- Hero SKU hết hàng.

- SKU not sellable.

- Sale Lock/Recall active.

- Fulfillment quá tải.

- CSKH quá tải.

- AI/human handoff quá tải.

- Owner approval thiếu.

- Dashboard mismatch.

- Evidence thiếu hoặc rejected.

## 72. STOP / PAUSE GATE DONE / FAIL

## 72.1. Điều kiện Stop/Pause Gate Done

Stop/Pause Gate được xem là completed khi:

- Trigger reason được xác định.

- Campaign/adset/ad/page/live scope rõ.

- Risk metric được ghi nhận.

- Dashboard snapshot có evidence.

- Data quality state rõ.

- Attribution state rõ.

- Revenue state rõ.

- Stock/sellable/sale lock state rõ.

- Complaint/claim/privacy state rõ.

- Fulfillment state rõ nếu liên quan.

- Owner decision hoặc policy auto-stop rule có hiệu lực.

- Pause/stop/resume condition được ghi nhận.

- Audit/evidence accepted.

## 72.2. Stop/Pause Gate Fail

Stop/Pause Gate phải FAIL nếu:

- Risk trigger không được ghi nhận.

- Campaign có P0 risk nhưng vẫn Scale Allowed.

- Privacy incident không chặn scale.

- Claim/safety incident không chặn scale.

- Sale Lock active nhưng campaign không pause/stop.

- SKU not sellable nhưng campaign vẫn scale.

- COD/payment fail cao nhưng không cảnh báo.

- Complaint spike không vào risk dashboard.

- Owner stop request không được ghi nhận.

- Evidence thiếu hoặc rejected.

## 73. RELEASE CONTROL GOVERNANCE

## 73.1. PACK-07 không được Release Ready chỉ vì tài liệu xong

Hoàn tất tài liệu PACK-07 không đồng nghĩa release.

PACK-07 chỉ được xem là Release Ready khi có đủ:

- Implementation hoàn tất theo governance.

- Smoke pass.

- Data Quality pass.

- Attribution pass.

- Verified Revenue linkage pass.

- Dashboard pass.

- Scale Gate pass.

- Stop/Pause Gate pass.

- Evidence accepted.

- No P0 open.

- Owner sign-off.

- Release decision.

Trước khi đạt đủ các điều kiện trên, trạng thái vẫn là:

GOVERNANCE_DOCUMENTATION_COMPLETE — waiting IMPLEMENTATION / TEST / SMOKE / EVIDENCE / OWNER SIGN-OFF / RELEASE DECISION

## 73.2. Release status registry

Các trạng thái release tối thiểu:

- RELEASE_NOT_STARTED.

- Documentation Complete.

- IMPLEMENTATION_waiting.

- IMPLEMENTATION_IN_PROGRESS.

- IMPLEMENTATION_COMPLETE_waiting_TEST.

- SMOKE_waiting.

- SMOKE_FAILED.

- SMOKE PASS WAITING EVIDENCE.

- EVIDENCE_waiting.

- EVIDENCE_REJECTED.

- EVIDENCE_ACCEPTED.

- OWNER_REVIEW_waiting.

- OWNER_SIGNED_OFF.

- RELEASE_DECISION_waiting.

- RELEASE_BLOCKED.

- RELEASE_ROLLBACK_REQUIRED nếu có sự cố sau release.

PACK-07 không được nhảy trạng thái.

## 73.3. Release STOP_POINTS

Các STOP_POINTS P0:

- Không có Ads spend source hợp lệ.

- Không có campaign/adset/ad mapping.

- Không có PACK-06 channel trace.

- Không có PACK-05 AI event trace khi đo AI conversion.

- Không có quote/order/revenue linkage.

- Không có verified revenue event.

- Không có attribution rule.

- Attribution conflict chưa xử lý.

- Data quality fail.

- Dashboard không phân biệt raw/clean/verified/excluded/waiting.

- ROAS tính từ revenue waiting.

- Excluded revenue vẫn vào ROAS.

- Duplicate event chưa xử lý.

- Privacy/claim incident chưa xử lý.

- Scale Gate PASS khi thiếu owner approval.

- Stop/Pause Gate không kích hoạt khi có P0 risk.

- Evidence thiếu hoặc rejected.

- P0 issue còn mở.

Chỉ cần một P0 STOP_POINTS còn mở thì PACK-07 không được Release Approved.

## 74. ROLLBACK / RECOVERY GOVERNANCE

## 74.1. Rollback không được xóa lịch sử đo lường

Nếu PACK-07 dashboard, attribution hoặc ROAS bị lỗi, rollback không được xóa event lịch sử.

Rollback chỉ được:

- Dừng dùng metric lỗi cho decision.

- Đưa dashboard về trạng thái warning.

- Khóa Scale Gate.

- Chuyển owner review.

- Chạy lại data quality.

- Chạy lại attribution nếu rule sửa.

- Rebuild projection nếu cần.

- Gắn evidence mới.

- Ghi nhận audit.

Không được sửa tay số liệu cũ để làm đẹp dashboard.

Không được xóa dấu vết conflict.

Không được xóa event bị excluded.

## 74.2. Khi nào cần rollback

Rollback hoặc recovery bắt buộc khi:

- ROAS tính sai revenue basis.

- waiting revenue bị tính verified.

- Campaign attribution sai.

- Revenue bị tính trùng nhiều nguồn.

- Excluded revenue vào ROAS.

- Spend import trùng.

- Dashboard gộp sai dữ liệu.

- Scale Gate PASS sai.

- Stop Gate không kích hoạt khi có P0 risk.

- Privacy/claim incident liên quan đo lường.

- Owner yêu cầu.

## 74.3. Recovery Done Gate

Recovery chỉ done khi:

- Nguyên nhân lỗi được xác định.

- Dữ liệu ảnh hưởng được khoanh vùng.

- Metric bị lỗi được đánh dấu.

- Dashboard được cảnh báo hoặc tạm khóa.

- Data quality được chạy lại.

- Attribution được chạy lại nếu cần.

- Revenue basis được kiểm tra lại.

- Scale/Stop decision bị ảnh hưởng được review.

- Evidence mới accepted.

- Owner sign-off recovery.

## 75. SECURITY / PRIVACY / CLAIM CONTROL TRONG PACK-07

## 75.1. PACK-07 phải tôn trọng privacy

PACK-07 không được public dữ liệu cá nhân của khách.

Dashboard chỉ được hiển thị dữ liệu theo quyền.

Các dữ liệu cần kiểm soát:

- Tên khách hàng.

- Số điện thoại.

- Địa chỉ.

- Nội dung Messenger.

- Thông tin đơn hàng cá nhân.

- Thông tin thanh toán.

- Tình trạng khiếu nại.

- Lịch sử mua hàng.

- Member status.

- Referral relationship.

PACK-07 chỉ đo lường theo identity được phép và policy cho phép.

## 75.2. Privacy incident chặn scale

Nếu có privacy incident, PACK-07 phải:

- Đánh dấu incident.

- Chặn scale nếu incident P0.

- Đưa vào Risk Dashboard.

- Gửi Owner Review.

- Giữ evidence.

- Không che cảnh báo bằng KPI tốt.

Trạng thái:

## PRIVACY_INCIDENT_OPEN — SCALE_BLOCKED

## 75.3. Claim incident chặn scale

Nếu Ads/Live/AI/CRM có claim/safety incident liên quan nội dung không được phép, PACK-07 phải chặn scale theo policy.

Không được scale campaign có claim risk chỉ vì ROAS đẹp.

Trạng thái:

## CLAIM_INCIDENT_OPEN — SCALE_BLOCKED / STOP_REQUIRED

## 76. ACCESS CONTROL / ROLE GOVERNANCE

## 76.1. Quyền xem dashboard

PACK-07 phải phân quyền xem dữ liệu.

Nhóm quyền tối thiểu:

- Owner view.

- Ads operator view.

- Marketing analyst view.

- Finance/revenue view nếu được phép.

- CSKH/risk view nếu cần.

- Read-only auditor view.

- Restricted privacy view.

Không phải ai cũng được xem dữ liệu khách hàng, doanh thu, order, payment hoặc member/referral.

## 76.2. Quyền quyết định

Các hành động cần quyền:

- Approve attribution model.

- Approve revenue basis.

- Approve manual spend.

- Approve manual correction.

- Approve scale.

- Approve stop/pause.

- Approve resume.

- Approve override.

- Accept evidence.

- Close P0 issue.

- Release approval.

Không được để dashboard user tự thay đổi rule nguồn doanh thu nếu không có quyền.

## 77. PACK-07 FINAL DONE GATE

## 77.1. Điều kiện PACK-07 Documentation Done

PACK-07 được xem là hoàn tất tài liệu khi có đủ 4 phần:

- PHẦN 1/4 — Ads Measurement Principles / Source-of-Truth Boundary / Dependency.

- PHẦN 2/4 — Event Taxonomy / Attribution Model / Funnel Metrics / Verified Revenue Rules.

- PHẦN 3/4 — Dashboard / KPI / ROAS / CPA / AOV / Scale Gate / Stop Gate / Owner Decision.

- PHẦN 4/4 — Smoke Matrix / Done Gate / Fail Gate / Release Control / Final Conclusion.

Trạng thái tài liệu:

## GOVERNANCE_DOCUMENTATION_COMPLETE

## 77.2. Điều kiện PACK-07 Implementation Done

Implementation Done chỉ được gọi khi:

- Dev đã triển khai theo PACK-07.

- Không tạo parallel source-of-truth.

- Không tự tính revenue ngoài owner runtime.

- Không tự đọc raw Facebook event để đo production.

- Không tự xác nhận payment/COD/delivery.

- Không tự sync MISA.

- Không tự scale Ads khi thiếu owner decision.

- Event taxonomy vận hành đúng.

- Data quality vận hành đúng.

- Attribution vận hành đúng.

- Dashboard vận hành đúng.

- Scale/Stop Gate vận hành đúng.

- Audit/evidence vận hành đúng.

## 77.3. Điều kiện PACK-07 Smoke Done

Smoke Done chỉ được gọi khi:

- ADS-SMK-001 đến ADS-SMK-024 được test.

- Positive path pass.

- Negative path pass.

- No P0 smoke fail.

- Retest pass nếu có lỗi.

- Evidence accepted cho từng smoke.

- Owner review pass.

## 77.4. Điều kiện PACK-07 ROAS PASS

ROAS PASS chỉ được gọi khi:

- Ads spend source hợp lệ.

- Campaign/adset/ad mapping hợp lệ.

- Attribution pass.

- Verified revenue pass.

- Revenue exclusion pass.

- Data quality pass.

- Dashboard hiển thị revenue basis rõ.

- waiting/excluded/conflict data không bị tính sai.

- Evidence accepted.

- Owner sign-off.

Không có Verified Revenue thì không có ROAS PASS.

Không có Data Quality PASS thì không có ROAS PASS.

Không có Attribution PASS thì không có source ROAS PASS.

## 77.5. Điều kiện PACK-07 Scale PASS

Scale PASS chỉ được gọi khi:

- ROAS PASS.

- CPA đạt owner threshold.

- AOV đạt owner threshold nếu dùng.

- Stock/sellable pass.

- Sale Lock/Recall clear.

- Fulfillment/CSKH capacity pass.

- Payment/COD fail trong giới hạn.

- Refund/return trong giới hạn.

- Complaint/risk trong giới hạn.

- Privacy/claim incident không mở.

- Dashboard READY_FOR_DECISION.

- Owner approval có hiệu lực.

- Scale evidence accepted.

Không có owner approval thì không có Scale PASS.

## 77.6. Điều kiện PACK-07 Release Ready

Release Ready chỉ được gọi khi:

- Documentation complete.

- Implementation complete.

- Smoke pass.

- Data Quality Done Gate pass.

- Attribution Done Gate pass.

- Verified Revenue Done Gate pass.

- Dashboard Done Gate pass.

- Scale Gate pass nếu release dùng cho scale decision.

- Stop/Pause Gate pass.

- Evidence accepted.

- No P0 open.

- Security/privacy/claim review pass.

- Owner sign-off.

- Release decision approved.

## 78. PACK-07 FAIL GATE TỔNG THỂ

PACK-07 phải FAIL hoặc bị chặn nếu có bất kỳ điều kiện sau:

- Tự tính revenue từ comment/inbox/quote/cart.

- Tính ROAS từ revenue chưa verified.

- Tính source ROAS khi attribution conflict chưa xử lý.

- Gán Organic mặc định cho missing campaign data.

- Gán Diamond revenue khi thiếu referral bind.

- Gộp CRM revenue vào Ads revenue không có rule.

- Dùng raw Facebook event thiếu PACK-06 trace.

- Đo AI performance thiếu PACK-05 event.

- Gọi cart draft là official order.

- Gọi payment waiting là paid revenue.

- Gọi COD waiting là COD successful revenue.

- Không loại refund/cancel/return khỏi ROAS theo policy.

- Không chống trùng event.

- Không hiển thị data quality warning.

- Dashboard che waiting/excluded/missing/conflict data.

- Scale khi SKU hero not sellable.

- Scale khi Sale Lock/Recall active.

- Scale khi fulfillment quá tải.

- Scale khi privacy/claim incident mở.

- Scale khi owner chưa approve.

- Stop/Pause Gate không kích hoạt khi có P0 risk.

- Evidence thiếu, rejected hoặc expired.

- P0 issue còn mở.

- Release decision thiếu.

Fail Gate phải ưu tiên an toàn hệ thống hơn chỉ số đẹp.

## 79. PACK-07 HANDOFF CONTROL

## 79.1. Handoff sang triển khai

Khi chuyển PACK-07 sang triển khai, dev phải nhận rõ:

- PACK-07 không phải nơi tạo source-of-truth mới.

- PACK-07 chỉ consume event/checkpoint từ owner pack.

- PACK-07 không tự tính giá, chiết khấu, quyền lợi, commission.

- PACK-07 không tự xác nhận payment/COD/delivery.

- PACK-07 không sync MISA.

- PACK-07 không tự scale Ads.

- PACK-07 phải fail-closed khi thiếu data.

- PACK-07 phải có smoke/evidence trước khi PASS.

## 79.2. Handoff sang vận hành Marketing

Marketing team chỉ được dùng dashboard PACK-07 theo trạng thái.

Nếu dashboard là RAW hoặc waiting, chỉ dùng để quan sát.

Nếu dashboard là READY_FOR_DECISION, mới được dùng cho đề xuất scale/stop.

Nếu Scale Gate chưa PASS, không được tăng ngân sách production dựa trên cảm tính.

Nếu Stop Gate yêu cầu pause/stop, marketing phải xử lý theo owner policy.

## 79.3. Handoff sang Owner Review

Owner Review phải nhận được:

- Dashboard snapshot.

- Time window.

- Campaign/adset/ad scope.

- Revenue basis.

- Attribution model.

- Data quality status.

- ROAS/CPA/AOV.

- Risk status.

- Stock/sellable status.

- Fulfillment capacity.

- Scale/stop recommendation.

- Evidence.

- waiting issue.

- Decision options.

Owner không nên quyết định chỉ dựa trên một chỉ số ROAS đơn lẻ.

## 80. PACK-07 FINAL STATUS

Sau khi hoàn tất 4 phần, trạng thái của PACK-07 là:

## ADS MEASUREMENT / CHANNEL ATTRIBUTION / VERIFIED REVENUE / SCALE GATE / DATA QUALITY GOVERNANCE

Trạng thái tài liệu:

## GOVERNANCE_DOCUMENTATION_COMPLETE

Trạng thái triển khai:

waiting IMPLEMENTATION

Trạng thái kiểm thử:

waiting TEST / SMOKE

Trạng thái evidence:

waiting ACCEPTED EVIDENCE

Trạng thái owner:

waiting OWNER SIGN-OFF

Trạng thái release:

waiting RELEASE DECISION

Trạng thái production:

## NOT PRODUCTION READY

Trạng thái scale:

## NOT SCALE APPROVED UNTIL SCALE GATE PASS + OWNER APPROVAL

Trạng thái ROAS:

## NOT ROAS PASS UNTIL VERIFIED REVENUE + ATTRIBUTION + DATA QUALITY + EVIDENCE PASS

## KẾT LUẬN CUỐI PACK-07

PACK-07 là pack quản trị đo lường Ads, ROAS, Attribution, Verified Revenue, Data Quality và Scale/Stop Decision cho hệ thống Ginsengfood.

PACK-07 không phải Facebook Gateway.

PACK-07 không phải AI Advisor.

PACK-07 không phải Commerce Runtime.

PACK-07 không phải Payment Core.

PACK-07 không phải MISA Integration.

PACK-07 không phải Finance Accounting.

PACK-07 không tự tạo đơn hàng.

PACK-07 không tự tạo doanh thu.

PACK-07 không tự xác nhận thanh toán.

PACK-07 không tự xác nhận COD thành công.

PACK-07 không tự xác nhận giao hàng.

PACK-07 không tự sync MISA.

PACK-07 không tự tính commission.

PACK-07 không tự scale Ads.

PACK-07 chỉ được đo lường khi có event hợp lệ, owner source hợp lệ, trace hợp lệ, attribution rule hợp lệ, verified revenue hợp lệ, data quality pass và evidence accepted.

Nguyên tắc khóa cuối cùng của PACK-07:

Comment không phải đơn hàng.

Inbox không phải doanh thu.

Quote không phải revenue.

Cart draft không phải official order.

Order chưa chắc là paid revenue.

Payment waiting chưa phải verified revenue.

Revenue waiting không được tính ROAS chính thức.

Attribution conflict không được gán nguồn cảm tính.

Missing data không được tự suy diễn.

ROAS đẹp nhưng dữ liệu chưa sạch thì không được scale.

Data Quality đi trước Dashboard.

Verified Revenue đi trước ROAS.

Attribution Rule đi trước source revenue.

Stock/Sellable/Sale Lock đi trước Scale.

Fulfillment capacity đi trước Scale.

Owner Approval đi trước Scale Production.

Evidence Accepted đi trước PASS.

PACK-07 hoàn tất về mặt governance/documentation, nhưng chưa được gọi là triển khai xong, chưa được gọi là ROAS PASS, chưa được gọi là Scale Approved, chưa được gọi là Release Approved và chưa được gọi là Production Ready nếu chưa có implementation, test, smoke, accepted evidence, owner sign-off và release decision.

## ĐỀ XUẤT TIẾP THEO

Bước tiếp theo nên chuyển sang PACK-08 — MC AI LIVE / LIVE SCRIPT / HOSTING ASSISTANT / REAL-TIME SALES ORCHESTRATION PACK, vì PACK-07 đã khóa xong lớp đo lường Ads/ROAS/Attribution và cần tiếp tục khóa lớp vận hành MC AI Live theo đúng ranh giới: MC AI không tự tạo giá, không tự chốt doanh thu, không vượt PACK-05, không vượt PACK-06, không vượt Commerce Runtime, và toàn bộ live performance phải đo được qua PACK-07.

ssssssPACK-09 — IVR_ORDER_CONFIRMATION

## IVR ORDER CONFIRMATION / INTERNAL SIM GATEWAY GOVERNANCE

## CHANGE LOCK V1.1

V1.1 khóa các thay đổi sau:

o | INVALID_PHONE

o | NO_ANSWER

o | TECHNICAL_EXCEPTION

o | CUSTOMER_CANCELLED

- Giữ rule Giờ Vàng:

o | PROGRAM = GOLDEN_HOUR

o | IVR_CONFIRMATION_WINDOW = 10_MINUTES

o | MAX_ATTEMPT = 2

o | ATTEMPT_1 = T0

o | ATTEMPT_2 = T0 + 5_MINUTES

o | WINDOW_EXPIRES = T0 + 10_MINUTES

- Điều chỉnh rule 24/7:

o | PROGRAM = TWENTY_FOUR_SEVEN

o | IVR_CONFIRMATION_WINDOW = 15_MINUTES

o | MAX_ATTEMPT = 3

o | ATTEMPT_1 = T0

o | ATTEMPT_2 = T0 + 5_MINUTES

o | ATTEMPT_3 = T0 + 10_MINUTES

o | WINDOW_EXPIRES = T0 + 15_MINUTES

- Tin nhắn giải thích hủy đơn chỉ được gửi sau khi Core Order State Machine đã hủy đơn chính thức.

- SIM Gateway không được tự gửi tin nhắn hủy đơn.

- AI Advisor không được tự gửi tin nhắn hủy đơn.

- CRM không được dùng tin nhắn hủy đơn như marketing, chăm sóc lại hoặc upsell.

- Facebook Gateway / Messenger chỉ là channel gửi nếu Notification owner phát hành message hợp lệ.

- Technical exception không được tính là khách không nghe.

- Capacity incident không được tính là khách không nghe.

- Invalid phone không được nhầm với no answer.

- IVR vẫn là ORDER_CONFIRMATION_ONLY, không phải sales, CRM, marketing, payment, delivery, warehouse, MISA hay ROAS.

## PHẦN 1/4 — IVR ORDER CONFIRMATION PRINCIPLES / INTERNAL SIM GATEWAY / SOURCE-OF-TRUTH BOUNDARY / NO-BYPASS RULE

## 1. DOCUMENT STATUS

## 1.1. Tên tài liệu

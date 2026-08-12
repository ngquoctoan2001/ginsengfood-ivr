# TECH-07 - ADS MEASUREMENT ATTRIBUTION ROAS CPA AOV VERIFIED REVENUE SCALE GATE

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

ADS EVENT GOVERNANCE / PIXEL-CAPI BOUNDARY / FACEBOOK GATEWAY SIGNAL / COMMERCE REVENUE VERIFICATION / CAMPAIGN SUPPRESSION / DASHBOARD / PILOT / SCALE CONTROL

## PHẦN 1/4 - ADS MEASUREMENT PRINCIPLES / SOURCE-OF-TRUTH / VERIFIED REVENUE BOUNDARY / NO-FAKE-ROAS RULE

## 1.1. Mục tiêu của PHẦN 1/4

## PHẦN 1/4 khóa vai trò, ranh giới và nguyên tắc nền của TECH-07 - Ads Measurement.

Tài liệu này xác định rõ:

Ads Measurement là tầng đo lường, đối soát, phân bổ và kiểm soát scale quảng cáo.

Ads Measurement không phải Commerce.

Ads Measurement không phải Payment.

Ads Measurement không phải nơi tự tạo doanh thu.

Ads Measurement không được lấy quote, cart, order draft, unpaid order làm revenue.

Verified Revenue từ Commerce là nguồn doanh thu duy nhất được dùng để tính ROAS, AOV, CPA theo revenue.

Pixel, CAPI, Facebook Gateway event, AI conversation, comment, inbox, quote chỉ là tín hiệu phễu nếu chưa được xác minh doanh thu.

Dashboard chỉ có giá trị khi có evidence, data quality và dedup/idempotency đạt.

Không được scale quảng cáo nếu thiếu evidence, thiếu owner approval hoặc có operational/commercial suppression.

Documentation Complete không đồng nghĩa Production Ready.

## 1.2. Vị trí của TECH-07 trong kiến trúc tổng thể

TECH-07 nằm sau:

## TECH-00 - Foundation / Global Governance.

## TECH-01 - Product / SKU / Recipe Foundation.

## TECH-02 - Product Knowledge / Runtime Foundation.

## TECH-03 - Operational Core.

## TECH-04 - Commerce Runtime.

## TECH-05 - AI Advisor Runtime.

## TECH-06 - Facebook Gateway / Meta Live & Messenger / Channel Delivery.

TECH-07 nằm trước:

## TECH-08 - MC AI Live.

## TECH-09 - IVR / Order Confirmation.

Các module scale marketing, CRM, affiliate, dashboard quản trị và executive reporting mở rộng sau này.

Nguyên tắc bắt buộc:

TECH-07 không được đi trước TECH-03, TECH-04, TECH-05, TECH-06 trong tư duy triển khai.

Lý do:

TECH-03 xác định SKU có được bán hay không theo Operational Core, Sale Lock, Recall, Warehouse, Traceability.

TECH-04 xác định quote, order, payment, verified revenue.

TECH-05 xác định AI tư vấn có được phép chốt hay không.

TECH-06 xác định tín hiệu kênh Facebook / Live / Messenger / Landing / Channel có hợp lệ hay không.

TECH-07 chỉ được đo lường và scale trên nền các tầng trên đã trả về dữ liệu hợp lệ.

## 1.3. Tuyên bố lõi của TECH-07

Ads Measurement không được đo “doanh thu tưởng tượng”.

Doanh thu trong Ads Measurement chỉ được ghi nhận khi Commerce Runtime xác nhận đó là Verified Revenue.

Mọi số liệu trước Verified Revenue đều là tín hiệu phễu, bao gồm:

Impression.

Reach.

Click.

View content.

Comment.

Inbox.

Messenger handoff.

AI conversation.

Product consultation.

Quote created.

Cart created.

Order Draft created.

Customer viewed order draft.

Customer intended to pay.

Payment WAITING.

## COD WAITING.

Các tín hiệu này có giá trị phân tích, nhưng không được dùng làm revenue.

## 1.4. Định nghĩa Ads Measurement Runtime Boundary

Ads Measurement Runtime là lớp chịu trách nhiệm:

Thu nhận tín hiệu quảng cáo.

Thu nhận tín hiệu kênh.

Thu nhận tín hiệu phễu bán hàng.

Đối soát sự kiện với Commerce Runtime.

Map order verified sang campaign / adset / ad / creative nếu đủ điều kiện.

Tính các chỉ số đo lường quảng cáo.

Kiểm soát data quality.

Chống trùng doanh thu.

Phân tách Ads Revenue, CRM Revenue, Diamond Referral Revenue.

Phát hiện conflict attribution.

Cảnh báo suppression.

Chuẩn bị dashboard.

Chuẩn bị pilot review.

Chuẩn bị scale gate.

Chặn scale nếu dữ liệu không đủ tin cậy.

Ads Measurement Runtime không được:

Tự tạo quote.

Tự tạo cart.

Tự tạo order.

Tự xác nhận payment.

Tự chuyển Payment WAITING thành Paid.

Tự chuyển COD WAITING thành Revenue.

Tự sửa trạng thái Commerce.

Tự sửa trạng thái Operational.

Tự bỏ Sale Lock / Recall / Suppression.

Tự cộng dồn Ads + Diamond + CRM nếu chưa có rule ưu tiên.

Tự làm đẹp dashboard bằng số liệu chưa xác minh.

Tự quyết định scale nếu chưa có owner approval.

## 1.5. Source-of-Truth của TECH-07

Ads Measurement phải đọc dữ liệu theo thứ tự nguồn sự thật sau:

## 1.5.1. Product / SKU Source-of-Truth

Nguồn quản trị:

Product Master.

SKU Master.

Product Knowledge approved.

Claim approved.

Creative approved.

Product Activation Guard.

Sellable Gate.

Ý nghĩa với Ads:

Một SKU có trong danh mục không có nghĩa là được chạy quảng cáo scale.

Một SKU active không có nghĩa là sellable.

Một SKU sellable tại Commerce vẫn có thể bị chặn scale nếu:

Claim chưa approved.

Creative chưa approved.

Public wording chưa approved.

Sale Lock đang bật.

Recall đang bật.

Warehouse không đủ điều kiện mở bán.

Public Trace bị bị chặn.

Evidence chưa accepted.

## 1.5.2. Operational Source-of-Truth

Nguồn quản trị:

TECH-03 Operational Core.

Warehouse.

Inventory.

Traceability.

Recall.

Sale Lock.

Release status.

Public Trace readiness.

Ý nghĩa với Ads:

Ads không được scale SKU nếu Operational Core chặn bán.

Sale Lock / Recall / Suppression thắng mọi kết quả ROAS.

Dù dashboard đang đẹp, nếu SKU bị Sale Lock hoặc Recall, campaign phải bị chặn theo suppression policy.

## 1.5.3. Commerce Source-of-Truth

Nguồn quản trị:

TECH-04 Commerce Runtime.

QuoteSnapshot.

Cart.

Order Draft.

Official Order.

Payment Confirmation.

COD Result.

Order Verification.

Verified Revenue.

Ý nghĩa với Ads:

Commerce là nguồn xác nhận doanh thu duy nhất.

Ads không được tự coi quote, cart, order draft, unpaid order, payment WAITING, COD WAITING là revenue.

## 1.5.4. AI Advisor Source-of-Truth

Nguồn quản trị:

TECH-05 AI Advisor Runtime.

Product consulting flow.

Final Response Guard.

Quote-safe sales.

Order Draft handoff.

Ý nghĩa với Ads:

AI conversation chỉ là engagement / funnel signal.

AI tư vấn tốt không đồng nghĩa có doanh thu.

AI báo giá thành công không đồng nghĩa order verified.

AI tạo order draft không đồng nghĩa revenue.

## 1.5.5. Facebook Gateway Source-of-Truth

Nguồn quản trị:

TECH-06 Facebook Gateway.

Meta Live.

Messenger.

Comment-to-Messenger.

Typing / response pacing.

Page / permission / app review evidence.

Channel event signal.

Ý nghĩa với Ads:

Gateway event là tín hiệu kênh.

Comment, inbox, Messenger handoff, live engagement, landing click không phải revenue.

Gateway không được tự gửi revenue event nếu Commerce chưa verified.

## 1.5.6. Ads Measurement Source-of-Truth

TECH-07 chỉ là nơi tổng hợp, đo lường và kiểm soát.

TECH-07 không được trở thành nguồn sự thật thay thế cho Product, Operational, Commerce, AI Advisor hoặc Gateway.

## 1.6. Nguyên tắc Verified Revenue Boundary

Verified Revenue là doanh thu đã qua đủ điều kiện xác minh theo Commerce Runtime.

Một khoản doanh thu chỉ được đưa vào Ads Measurement khi đạt tối thiểu các điều kiện:

Có official order hợp lệ.

Order không bị hủy.

Order không bị invalid.

Order không bị refund theo policy loại trừ.

Payment đã được xác nhận hợp lệ hoặc COD đã hoàn tất theo trạng thái được Commerce công nhận.

Order đã qua Order Verification.

Revenue được Commerce đánh dấu là Verified Revenue.

Attribution có thể được resolve hoặc đưa vào nhóm unattributed verified revenue nếu không đủ dữ liệu phân bổ.

Ads Measurement không được tự suy đoán doanh thu từ:

Tổng giá trị quote.

Tổng giỏ hàng.

Tổng order draft.

Tổng unpaid order.

Tổng khách hỏi giá.

Tổng inbox.

Tổng comment.

Tổng khách nói “chốt”.

Tổng khách gửi ảnh chuyển khoản.

Tổng COD đang giao.

Tổng đơn chưa verified.

## 1.7. No-Fake-ROAS Rule

No-Fake-ROAS Rule là nguyên tắc P0 của TECH-07.

ROAS chỉ được tính từ Verified Revenue.

Không được tính ROAS từ:

Quote value.

Cart value.

Order Draft value.

WAITING payment value.

COD WAITING value.

COD failed value.

Cancelled order value.

Refunded order value nếu policy loại trừ.

Invalid order value.

AI-estimated value.

Gateway-estimated value.

CRM-estimated value.

Manual dashboard value chưa có evidence.

Công thức quản trị:

ROAS quản trị = Verified Revenue được phân bổ hợp lệ / Ads Spend hợp lệ.

Nếu Verified Revenue chưa sẵn sàng, dashboard chỉ được hiển thị chỉ số phễu, không được hiển thị ROAS như kết quả kinh doanh cuối.

## 1.8. Pixel / CAPI / Event Boundary

Pixel và CAPI là công cụ ghi nhận sự kiện hành vi.

Pixel / CAPI không phải nguồn xác nhận doanh thu cuối.

Pixel / CAPI được phép ghi nhận:

Page view.

View content.

Lead.

Contact.

Messenger handoff signal.

Add to cart nếu Commerce xác nhận cart event hợp lệ.

Initiate checkout nếu Commerce xác nhận checkout/order draft event hợp lệ.

Purchase event chỉ khi Commerce có verified purchase mapping theo rule được duyệt.

Pixel / CAPI không được:

Tự phát Purchase event từ quote.

Tự phát Purchase event từ cart.

Tự phát Purchase event từ order draft.

Tự phát Purchase event từ payment WAITING.

Tự phát Purchase event từ ảnh chuyển khoản.

Gửi trùng purchase event không có event_id/idempotency.

Gửi revenue cao hơn Commerce Verified Revenue.

Gửi revenue khi order bị cancel/refund/invalid theo policy.

Bỏ qua Sale Lock / Recall / Suppression.

Nếu Pixel và CAPI cùng ghi nhận một sự kiện, bắt buộc phải có dedup/idempotency để tránh double count.

## 1.9. Facebook Gateway Event Signal Boundary

Facebook Gateway cung cấp các tín hiệu kênh cho Ads Measurement.

Các tín hiệu có thể bao gồm:

Comment received.

Comment classified.

Messenger handoff attempted.

Messenger handoff succeeded.

Messenger handoff failed.

Private conversation started.

Live session engagement.

Customer asked price.

Customer asked product.

Customer requested quote.

Customer moved to order draft.

Abuse / spam / complaint / moderation flag.

Suppression trigger from channel policy.

Các tín hiệu này giúp phân tích chất lượng phễu.

Tuy nhiên:

Gateway event không phải revenue.

Messenger conversation không phải revenue.

Live comment không phải revenue.

Handoff succeeded không phải revenue.

Customer asked price không phải revenue.

Customer said “mua” không phải revenue.

Gateway chỉ được truyền signal về Ads Measurement theo đúng channel policy và privacy boundary.

## 1.10. Funnel Signal Boundary

TECH-07 phân loại tín hiệu phễu thành 5 tầng:

## 1.10.1. Awareness Signal

Bao gồm:

Impression.

Reach.

Video view.

Live view.

Creative engagement.

Ý nghĩa:

Đo độ phủ và độ hấp dẫn ban đầu.

Không phải revenue.

## 1.10.2. Engagement Signal

Bao gồm:

Reaction.

Comment.

Share.

Click.

Landing page visit.

Live comment.

Messenger entry.

Ý nghĩa:

Đo mức quan tâm.

Không phải revenue.

## 1.10.3. Consultation Signal

Bao gồm:

Hỏi giá.

Hỏi sản phẩm.

Hỏi thành phần.

Hỏi bằng chứng.

Hỏi hiệu dụng.

AI tư vấn.

Customer profile resolved.

Product recommendation generated.

Ý nghĩa:

Đo chất lượng tư vấn.

Không phải revenue.

## 1.10.4. Commerce Intent Signal

Bao gồm:

Quote created.

Cart created.

Order Draft created.

Customer reviewed order draft.

Payment method selected.

Customer intended to pay.

Ý nghĩa:

Đo ý định mua.

Chưa phải revenue.

## 1.10.5. Verified Revenue Signal

Bao gồm:

Official order verified.

Payment confirmed hoặc COD completed theo Commerce policy.

Revenue verified.

Refund/cancel/exclusion đã xử lý.

Attribution resolved hoặc đưa vào nhóm verified unattributed.

Ý nghĩa:

Được phép dùng cho ROAS, AOV, revenue dashboard và scale review.

## 1.11. Quote / Cart / Order Draft / Unpaid Order Exclusion

TECH-07 khóa rõ:

Quote không phải revenue.

Cart không phải revenue.

Order Draft không phải revenue.

Unpaid order không phải revenue.

Payment WAITING không phải revenue.

COD WAITING không phải revenue.

Lý do:

Các trạng thái này chưa chứng minh được tiền đã được xác nhận, đơn đã hợp lệ và doanh thu đã được Commerce công nhận.

Ads Measurement được phép dùng các trạng thái này để tính:

Quote Rate.

Cart Rate.

Order Draft Rate.

Checkout Intent Rate.

Payment Intent Rate.

Funnel Drop-off.

Cost per Quote.

Cost per Order Draft.

Cost per Payment Intent.

Nhưng không được dùng để tính:

Revenue.

## ROAS.

AOV theo doanh thu verified.

Commission.

KPI doanh thu.

Scale decision cuối.

## 1.12. Order Verified / Revenue Verified Mapping

Order Verified và Revenue Verified là điểm chuyển từ tín hiệu phễu sang doanh thu đo lường.

Một order chỉ được đưa vào Ads Measurement revenue khi Commerce trả về trạng thái đủ điều kiện.

Ads Measurement cần phân biệt:

Order created.

Order confirmed by customer.

Payment WAITING.

Payment confirmed.

## COD WAITING.

COD success.

COD fail.

Cancelled.

Refunded.

Invalid.

Order verified.

Revenue verified.

Chỉ trạng thái Revenue Verified mới được tính vào doanh thu quảng cáo.

Nếu order có nhiều trạng thái thay đổi theo thời gian, dashboard phải phản ánh theo trạng thái mới nhất đã được Commerce xác nhận, không giữ doanh thu cũ nếu order bị loại trừ về sau theo policy.

## 1.13. Attribution Boundary

Ads Measurement được phép phân bổ doanh thu verified cho:

Campaign.

Adset.

Ad.

Creative.

Placement.

Channel entry.

Live session.

Messenger session.

Landing page session.

Referral source nếu có rule hợp lệ.

Attribution không được làm thay Commerce.

Attribution không được tạo revenue mới.

Attribution chỉ trả lời câu hỏi:

“Verified Revenue này nên được ghi nhận về nguồn nào theo rule đã khóa?”

Không được dùng attribution để:

Nhân đôi doanh thu.

Cộng doanh thu cho nhiều nguồn cùng lúc nếu không có allocation rule.

Vừa tính đủ cho Ads, vừa tính đủ cho Diamond, vừa tính đủ cho CRM.

Đẩy revenue sang campaign tốt hơn để làm đẹp dashboard.

Bỏ qua evidence trail.

## 1.14. Campaign / Adset / Ad / Creative Attribution

Mỗi doanh thu verified nếu được phân bổ cho Ads cần có đường dẫn attribution tối thiểu:

Campaign identity.

Adset identity.

Ad identity.

Creative identity.

Entry channel.

Event trail.

Attribution window.

Dedup key.

Evidence reference.

Conflict status.

Nếu thiếu campaign/adset/ad/creative identity, doanh thu không được ép phân bổ.

Trường hợp không đủ dữ liệu phân bổ:

Doanh thu vẫn có thể là Verified Revenue.

Nhưng phải nằm trong nhóm Unattributed Verified Revenue hoặc Partial Attribution.

Không được gán bừa cho campaign đang muốn scale.

## 1.15. Diamond Referral / Ads Attribution Conflict

Diamond referral và Ads attribution có thể cùng xuất hiện trong một đơn hàng.

Ví dụ:

Khách bấm quảng cáo.

Sau đó vào link giới thiệu Diamond.

Hoặc khách vào từ link Diamond rồi bị retargeting ads.

Hoặc khách đến từ Live Ads nhưng được bind referral hợp lệ.

TECH-07 khóa nguyên tắc:

Diamond referral bind phải rõ.

Ads + Diamond conflict phải qua attribution priority rule.

Không tự cộng đủ doanh thu cho cả Ads và Diamond nếu chưa có rule phân bổ.

Commission không được phát sinh chỉ vì dashboard Ads thấy revenue.

Diamond commission phải theo Commerce / Affiliate policy đã khóa.

Ads ROAS và Diamond commission phải có ranh giới tính toán rõ.

Nếu conflict chưa resolve:

Không được scale dựa trên revenue conflict đó.

Không được tính commission từ revenue conflict đó.

Không được đưa vào dashboard như revenue sạch.

Phải đưa vào nhóm Attribution Conflict WAITING.

## 1.16. CRM Revenue vs Ads Revenue Boundary

CRM revenue và Ads revenue phải tách rõ.

CRM revenue là doanh thu đến từ các hoạt động chăm sóc, nhắc mua lại, thành viên, tin nhắn giao dịch hoặc chiến dịch CRM theo rule.

Ads revenue là doanh thu được phân bổ hợp lệ cho quảng cáo.

Một verified order có thể có nhiều touchpoint, nhưng dashboard không được double count.

Nguyên tắc:

Nếu revenue thuộc CRM theo attribution policy, không được tự tính là Ads revenue.

Nếu revenue thuộc Ads theo attribution policy, không được tự tính là CRM revenue.

Nếu có cả Ads và CRM touchpoint, phải có priority rule hoặc allocation rule.

Nếu chưa resolve được, đưa vào Conflict WAITING.

Không dùng CRM revenue để làm đẹp ROAS Ads.

Không dùng Ads revenue để thổi phồng hiệu quả CRM.

## 1.17. Live / Messenger / Landing Page Attribution

TECH-07 phải hỗ trợ đo lường theo nhiều điểm vào:

Live.

Comment.

Messenger.

Landing page.

Website.

Referral link.

Direct channel.

CRM channel.

Tuy nhiên, điểm vào không tự động là nguồn doanh thu.

Ví dụ:

Khách comment live: đây là engagement signal.

Gateway kéo Messenger thành công: đây là channel handoff signal.

AI tư vấn sản phẩm: đây là consultation signal.

Quote được tạo: đây là commerce intent signal.

Order được xác nhận: vẫn chưa đủ nếu chưa payment/order verification.

Revenue được Commerce verified: đây mới là revenue signal.

Attribution chỉ được xác nhận khi toàn bộ chuỗi event đủ evidence và không bị conflict.

## 1.18. Campaign Suppression Boundary

Ads Measurement không chỉ đo lường, mà còn phải chặn scale khi có suppression.

Các nguồn suppression gồm:

Operational suppression.

Commerce suppression.

AI Advisor suppression.

Facebook Gateway suppression.

Claim / creative suppression.

Product / SKU suppression.

Sale Lock.

Recall.

Inventory / warehouse readiness issue.

Public trace issue.

Payment / COD issue.

Data quality issue.

Attribution conflict issue.

Owner approval missing.

Nếu suppression đang active:

Không được scale campaign.

Không được tăng ngân sách.

Không được mở rộng adset.

Không được nhân bản creative để chạy lớn.

Không được gọi dashboard là scale-ready.

Không được override bằng ROAS đẹp.

Suppression thắng ROAS.

## 1.19. Claim / Creative Approval Boundary

Ads chỉ được scale creative đã approved.

Creative chưa approved không được scale, dù số liệu thử nghiệm tốt.

Claim chưa approved không được scale, đặc biệt với sản phẩm Cháo Sâm Savigin có yếu tố thực dưỡng, Sâm Savigin, nghiên cứu khoa học, lợi ích sức khỏe và y thực đồng nguyên.

Nguyên tắc public-safe:

Không nói chữa bệnh.

Không nói điều trị.

Không nói thay thuốc.

Không cam kết hiệu quả y khoa.

Không suy diễn lâm sàng.

Không dùng claim vượt evidence.

Không dùng nội dung gây hiểu sai về công dụng.

Không dùng creative chưa qua ClaimGuard / owner approval.

Ads Measurement phải biết creative nào được phép scale, creative nào bị chặn.

## 1.20. Data Quality Boundary

Không có data quality thì không có scale.

Data quality trong TECH-07 gồm:

Event completeness.

Event timestamp hợp lệ.

Campaign/adset/ad/creative identity hợp lệ.

Channel identity hợp lệ.

Customer/session mapping hợp lệ trong phạm vi privacy.

Quote/order/payment mapping hợp lệ.

Verified revenue mapping hợp lệ.

Dedup/idempotency hợp lệ.

Refund/cancel/COD fail exclusion hợp lệ.

Attribution conflict được xử lý.

Suppression status được phản ánh.

Evidence trail đầy đủ.

Dashboard không lệch với Commerce verified revenue.

Nếu data quality fail:

Dashboard chỉ được dùng để debug.

Không được dùng để scale.

Không được dùng để báo cáo thành công cuối.

Không được gọi là Completion Decision.

Không được gọi là Release Ready.

## 1.21. Deduplication / Event Idempotency Boundary

TECH-07 bắt buộc chống trùng sự kiện.

Các trường hợp dễ trùng:

Pixel và CAPI cùng gửi một event.

Gateway retry event.

Messenger handoff retry.

Landing page reload.

Customer click nhiều lần.

Payment callback retry.

Order status sync retry.

Dashboard job chạy lại.

Manual import bị lặp.

Attribution resolver chạy lại.

Nguyên tắc:

Mỗi event quan trọng phải có khóa idempotency hoặc dedup key.

Purchase / revenue event phải chống trùng nghiêm ngặt nhất.

Một verified revenue không được tính hai lần.

Một order không được tạo nhiều dòng revenue chỉ vì nhiều channel touch.

Retry không được tạo doanh thu mới.

Reconciliation không được nhân đôi số liệu.

Nếu không xác định được dedup, đưa vào Data Quality Hold.

## 1.22. Dashboard Boundary

Dashboard trong TECH-07 là lớp hiển thị kết quả đo lường.

Dashboard không phải nguồn sự thật.

Dashboard không được tự sửa số liệu nguồn.

Dashboard không được làm đẹp số liệu.

Dashboard phải hiển thị rõ:

Ads Spend.

Verified Revenue.

## ROAS.

## CPA.

## AOV.

Boxes per Order.

Comment Rate.

Inbox Rate.

Quote Rate.

Order Draft Rate.

Payment Rate.

Verified Rate.

COD Fail Rate.

Cancel Rate.

Refund Rate.

Data Quality Status.

Attribution Conflict Status.

Suppression Status.

Pilot Status.

Scale Gate Status.

Owner Approval Status.

Dashboard phải phân biệt rõ:

Funnel signal.

Revenue verified.

Revenue excluded.

Revenue WAITING.

Attribution conflict.

Suppression active.

Scale-ready.

Not scale-ready.

Không được trình dashboard theo kiểu “số đẹp” nhưng không có evidence.

## 1.23. Pilot 7-14 Days Boundary

TECH-07 khóa nguyên tắc pilot trước scale.

Pilot Ads phải chạy trong khoảng 7-14 ngày hoặc theo owner-approved pilot window.

Mục tiêu pilot:

Kiểm tra event tracking.

Kiểm tra Pixel/CAPI dedup.

Kiểm tra Gateway signal.

Kiểm tra Commerce verified revenue mapping.

Kiểm tra attribution.

Kiểm tra data quality.

Kiểm tra dashboard.

Kiểm tra suppression.

Kiểm tra creative/claim approval.

Kiểm tra scale readiness.

Pilot không chỉ nhìn ROAS.

Pilot phải đánh giá:

Dữ liệu có đúng không.

Revenue có verified không.

Có double count không.

Có conflict Ads/Diamond/CRM không.

Có COD fail/cancel/refund bị loại đúng không.

Có SKU nào bị Sale Lock/Recall không.

Creative có approved không.

Owner có chấp thuận scale không.

## 1.24. Scale Gate Boundary

Scale Gate là cổng quyết định có được tăng ngân sách / mở rộng campaign hay không.

Scale Gate chỉ được mở khi đạt đủ:

Pilot evidence accepted.

Data quality pass.

Dedup/idempotency pass.

Verified revenue mapping pass.

ROAS/CPA/AOV được tính từ verified revenue.

COD fail/cancel/refund exclusion pass.

Attribution conflict resolved hoặc được loại khỏi scale decision.

Suppression clear.

SKU sellable.

Claim/creative approved.

Gateway event signal ổn định.

Commerce runtime ổn định.

Operational runtime không blocking.

Owner approval completed.

Release decision cho scale được ghi nhận.

Nếu thiếu một trong các điều kiện trên, Scale Gate = bị chặn hoặc WAITING, không được PASS.

## 1.25. Runtime Unavailable / Fail-Safe Rule

Nếu runtime liên quan không khả dụng, TECH-07 phải fail-safe.

Các trường hợp runtime unavailable:

Commerce không trả verified revenue.

Payment status không xác định.

COD result chưa rõ.

Operational suppression không đọc được.

Sale Lock / Recall status không đọc được.

Product sellable status không đọc được.

Claim/creative approval không đọc được.

Gateway event không ổn định.

Pixel/CAPI event bị lỗi dedup.

Attribution conflict không resolve được.

Dashboard lệch số.

Evidence thiếu.

Nguyên tắc xử lý:

Không scale.

Không gọi campaign là pass.

Không báo ROAS chính thức nếu thiếu verified revenue.

Không dùng số liệu WAITING thay revenue.

Không override suppression.

Không tự đoán nguồn attribution.

Không làm tròn số để trình owner.

## 1.26. Dependency bắt buộc của PHẦN 1/4

TECH-07 phụ thuộc tối thiểu vào các tầng sau:

## 1.26.1. Phụ thuộc TECH-03

Cần TECH-03 để biết:

SKU có bị Sale Lock không.

SKU có bị Recall không.

Batch có release hợp lệ không.

Warehouse có tồn bán được không.

Public Trace có sẵn sàng không.

Operational suppression có active không.

## 1.26.2. Phụ thuộc TECH-04

Cần TECH-04 để biết:

QuoteSnapshot có hợp lệ không.

Cart có hợp lệ không.

Order Draft đã được xác nhận chưa.

Official Order đã tạo chưa.

Payment đã confirmed chưa.

COD đã success hay fail.

Order đã verified chưa.

Revenue đã verified chưa.

## 1.26.3. Phụ thuộc TECH-05

Cần TECH-05 để biết:

AI conversation nào tạo ra quote intent.

Product recommendation có đúng SKU approved không.

Final Response Guard đã pass chưa.

AI có tạo order draft handoff hợp lệ không.

AI có bị suppression/handoff không.

## 1.26.4. Phụ thuộc TECH-06

Cần TECH-06 để biết:

Event đến từ page/live/messenger/landing nào.

Comment-to-Messenger handoff có thành công không.

Channel event có hợp lệ không.

Meta permission/app review evidence có pass không.

Gateway có bị rate limit/anti-spam/moderation/suppression không.

## 1.26.5. Phụ thuộc Owner Approval

Cần owner approval để:

Approve creative.

Approve claim.

Approve pilot.

Approve scale.

Approve attribution conflict policy.

Approve revenue exclusion policy.

Approve dashboard dùng cho điều hành.

## 1.27. P0 điểm chặn Registry của PHẦN 1/4

Các điểm chặn sau là P0:

Ads dùng quote làm revenue.

Ads dùng cart làm revenue.

Ads dùng order draft làm revenue.

Ads dùng unpaid order làm revenue.

Ads dùng payment WAITING làm revenue.

Ads dùng COD WAITING làm revenue.

Ads dùng COD fail làm revenue.

Ads dùng cancelled/refunded/invalid order làm revenue khi policy yêu cầu exclude.

Ads tính ROAS khi chưa có verified revenue.

Pixel/CAPI gửi Purchase event không có Commerce verified mapping.

Pixel/CAPI bị double count.

Dashboard revenue lệch với Commerce verified revenue.

Campaign scale khi SKU không sellable.

Campaign scale khi SKU bị Sale Lock/Recall.

Campaign scale khi claim/creative chưa approved.

Campaign scale khi data quality fail.

Campaign scale khi attribution conflict chưa resolve.

Campaign scale khi pilot chưa đủ evidence.

Campaign scale khi owner chưa approval.

Runtime unavailable nhưng vẫn scale.

Gateway event được xem là revenue.

AI conversation được xem là revenue.

CRM revenue bị cộng trùng vào Ads revenue.

Diamond referral revenue bị cộng trùng vào Ads revenue khi chưa có priority rule.

Dashboard hiển thị số “đẹp” nhưng không có evidence.

Nếu xuất hiện bất kỳ P0 điểm chặn nào, TECH-07 status phải là bị chặn.

## 1.28. Evidence yêu cầu cho PHẦN 1/4

## PHẦN 1/4 yêu cầu evidence tối thiểu:

Evidence xác nhận source-of-truth mapping từ Product / Operational / Commerce / AI / Gateway sang Ads Measurement.

Evidence xác nhận Verified Revenue chỉ đến từ Commerce.

Evidence xác nhận quote/cart/order draft/unpaid order không được tính revenue.

Evidence xác nhận Pixel/CAPI có dedup/idempotency rule.

Evidence xác nhận Gateway event chỉ là signal.

Evidence xác nhận dashboard phân biệt funnel signal và verified revenue.

Evidence xác nhận Sale Lock / Recall / Suppression chặn campaign/scale.

Evidence xác nhận claim/creative approval là điều kiện scale.

Evidence xác nhận Diamond/Ads conflict có rule xử lý hoặc bị đưa vào WAITING.

Evidence xác nhận CRM/Ads revenue không double count.

Evidence xác nhận pilot 7-14 ngày là điều kiện trước scale.

Evidence xác nhận owner approval là điều kiện scale.

Evidence chưa accepted thì không Completion Decision.

## 1.29. Smoke yêu cầu cho PHẦN 1/4

Smoke của PHẦN 1/4 cần kiểm tra tối thiểu:

ADS-SMK-001 - Quote không tạo revenue

Điều kiện:

Có quote hợp lệ.

Chưa có official order.

Chưa có payment.

Chưa có verified revenue.

Kỳ vọng:

Quote được tính là funnel signal.

Revenue = 0.

ROAS không được tính từ quote.

Dashboard không hiển thị quote value như revenue.

ADS-SMK-002 - Order Draft không tạo revenue

Điều kiện:

Customer đã có order draft.

Chưa customer confirmation.

Chưa official order.

Chưa payment confirmation.

Kỳ vọng:

Order Draft được tính là commerce intent signal.

Revenue = 0.

Không có Purchase event chính thức.

ADS-SMK-003 - Payment WAITING không tạo revenue

Điều kiện:

Customer chọn chuyển khoản.

Payment đang WAITING.

Có thể có ảnh chuyển khoản nhưng chưa payment confirmation.

Kỳ vọng:

Payment WAITING không phải revenue.

Không tính ROAS.

Không gửi verified purchase.

ADS-SMK-004 - COD WAITING không tạo revenue

Điều kiện:

Order COD đang giao.

Chưa có COD success.

Chưa order verification.

Kỳ vọng:

COD WAITING không phải revenue.

Revenue = 0 cho tới khi Commerce xác nhận.

ADS-SMK-005 - Verified Revenue mới tạo Ads Revenue

Điều kiện:

Official order hợp lệ.

Payment/COD success hợp lệ.

Order verified.

Commerce trả Verified Revenue.

Kỳ vọng:

Ads Measurement được phép ghi nhận revenue nếu attribution hợp lệ.

ROAS được tính từ Verified Revenue.

Dashboard hiển thị rõ evidence.

ADS-SMK-006 - Pixel/CAPI Dedup

Điều kiện:

Pixel và CAPI cùng gửi một event.

Có cùng dedup key/event identity.

Kỳ vọng:

Chỉ tính một event.

Không double count purchase.

Không double count revenue.

ADS-SMK-007 - Sale Lock chặn scale

Điều kiện:

Campaign có ROAS tốt.

SKU bị Sale Lock hoặc Recall.

Kỳ vọng:

Không tăng ngân sách.

Dashboard hiển thị suppression active.

ADS-SMK-008 - Claim/Creative chưa approved chặn scale

Điều kiện:

Campaign có funnel tốt.

Creative hoặc claim chưa approved.

Kỳ vọng:

Campaign không scale.

ADS-SMK-009 - Ads + Diamond conflict WAITING

Điều kiện:

Order có Ads touchpoint.

Order có Diamond referral bind.

Chưa có priority/allocation rule.

Kỳ vọng:

Không double count.

Revenue vào Attribution Conflict WAITING.

Không dùng revenue này để scale.

ADS-SMK-010 - Data Quality Fail chặn dashboard scale-ready

Điều kiện:

Verified revenue có nhưng event trail thiếu campaign/ad/creative identity.

Hoặc dashboard lệch Commerce.

Kỳ vọng:

Dashboard không được scale-ready.

## 1.30. Done Gate của PHẦN 1/4

## PHẦN 1/4 chỉ được xem là Documentation Done khi đạt đủ:

Đã khóa vai trò Ads Measurement.

Đã khóa source-of-truth.

Đã khóa Verified Revenue Boundary.

Đã khóa No-Fake-ROAS Rule.

Đã khóa Pixel/CAPI/Event Boundary.

Đã khóa Gateway Event Signal Boundary.

Đã khóa Quote/Cart/Order Draft/Unpaid Order Exclusion.

Đã khóa Attribution Boundary.

Đã khóa Diamond Referral / Ads conflict nguyên tắc.

Đã khóa CRM Revenue vs Ads Revenue Boundary.

Đã khóa Suppression Boundary.

Đã khóa Data Quality Boundary.

Đã khóa Dedup/Idempotency Boundary.

Đã khóa Dashboard Boundary.

Đã khóa Pilot 7-14 Days Boundary.

Đã khóa Scale Gate Boundary.

Đã khóa Runtime Unavailable / Fail-Safe Rule.

Đã có P0 điểm chặn registry.

Đã có evidence requirement.

Đã có smoke requirement.

Không viết code.

Không thiết kế API/DB/UI chi tiết.

Không gọi Documentation Done là Production Ready.

## 1.31. Fail Gate của PHẦN 1/4

## PHẦN 1/4 bị Fail nếu:

Có bất kỳ đoạn nào cho phép Ads dùng quote làm revenue.

Có bất kỳ đoạn nào cho phép Ads dùng cart làm revenue.

Có bất kỳ đoạn nào cho phép Ads dùng order draft làm revenue.

Có bất kỳ đoạn nào cho phép Ads dùng unpaid order làm revenue.

Có bất kỳ đoạn nào cho phép Pixel/CAPI tự phát Purchase từ dữ liệu chưa verified.

Có bất kỳ đoạn nào cho phép dashboard tự tính revenue khác Commerce.

Có bất kỳ đoạn nào gọi Gateway event là revenue.

Có bất kỳ đoạn nào gọi AI conversation là revenue.

Có bất kỳ đoạn nào cho phép double count Ads/CRM/Diamond.

Có bất kỳ đoạn nào cho phép scale khi data quality fail.

Có bất kỳ đoạn nào cho phép scale khi Sale Lock/Recall active.

Có bất kỳ đoạn nào cho phép scale khi creative/claim chưa approved.

Có bất kỳ đoạn nào cho phép scale không cần owner approval.

Có bất kỳ đoạn nào gọi tài liệu hoàn chỉnh là production ready.

Có bất kỳ đoạn nào đi vào code/API/DB/UI chi tiết ngoài phạm vi yêu cầu.

## 1.32. Release Handoff của PHẦN 1/4

## PHẦN 1/4 bàn giao cho PHẦN 2/4 các nguyên tắc sau:

Mọi module trong PHẦN 2/4 phải tôn trọng Verified Revenue Boundary.

Pixel/CAPI Event Module không được tự tạo revenue.

Gateway Event Signal Module chỉ truyền signal.

Campaign Registry phải biết suppression status.

Attribution Resolver phải chống double count.

Verified Revenue Mapper phải lấy Commerce làm nguồn duy nhất.

Funnel Metrics Module phải tách signal khỏi revenue.

ROAS/CPA/AOV Calculator phải dùng verified revenue.

Data Quality Guard phải chặn scale nếu fail.

Dedup/Idempotency Module là bắt buộc.

Diamond Conflict Resolver là bắt buộc.

CRM Revenue Boundary là bắt buộc.

Dashboard Module không được là source-of-truth.

Pilot Module phải kiểm tra evidence 7-14 ngày.

Scale Gate Module phải yêu cầu owner approval.

Trạng thái sau PHẦN 1/4:

## TECH-07 PHẦN 1/4 = DOCUMENTATION FOUNDATION READY

Chưa được gọi là:

Production Ready.

Release Ready.

Go-live Approved.

Scale Approved.

## 1.33. Kết luận PHẦN 1/4

TECH-07 không được xây trên tư duy “chạy quảng cáo thấy có đơn là tính ROAS”.

Tư duy đúng là:

Ads chỉ được đo revenue khi Commerce xác nhận Verified Revenue.

Mọi tín hiệu trước đó chỉ là tín hiệu phễu.

Quote, cart, order draft, payment WAITING, COD WAITING, comment, inbox, AI conversation đều có giá trị phân tích, nhưng không phải doanh thu.

ROAS không được làm giả bằng số chưa xác minh.

Dashboard không được đẹp hơn sự thật vận hành.

Scale không được quyết định chỉ bằng cảm giác campaign đang tốt.

TECH-07 phải bảo vệ doanh nghiệp khỏi 5 sai lầm lớn:

Tính nhầm tín hiệu thành doanh thu.

Double count revenue.

Scale khi dữ liệu sai.

Scale SKU/creative/claim chưa đủ điều kiện.

Scale khi Operational / Commerce / AI / Gateway đang blocking.

## PHẦN 1/4 đã khóa xong nền nguyên tắc.

Phần tiếp theo sẽ viết:

## PHẦN 2/4 - Ads Measurement Module Contracts

## PHẦN 2/4 - ADS MEASUREMENT MODULE CONTRACTS

## 2.1. Mục tiêu của PHẦN 2/4

## PHẦN 2/4 khóa các module contract của TECH-07.

Mục tiêu là xác định rõ:

Module nào thuộc Ads Measurement Runtime.

Module nào chỉ nhận signal, không tạo revenue.

Module nào được phép đọc Verified Revenue.

Module nào chịu trách nhiệm attribution.

Module nào chịu trách nhiệm data quality.

Module nào chịu trách nhiệm suppression.

Module nào chịu trách nhiệm dashboard.

Module nào chịu trách nhiệm pilot và scale gate.

Module nào bắt buộc có evidence.

Module nào nếu fail thì toàn bộ Ads Measurement bị bị chặn.

## PHẦN 2/4 không thiết kế code.

## PHẦN 2/4 không thiết kế API chi tiết.

## PHẦN 2/4 không thiết kế DB chi tiết.

## PHẦN 2/4 không thiết kế UI chi tiết.

## PHẦN 2/4 chỉ khóa contract nghiệp vụ - kỹ thuật để dev hiểu đúng ranh giới khi triển khai sau này.

## 2.2. Nguyên tắc contract chung

Mỗi module trong TECH-07 phải tuân thủ cấu trúc contract sau:

Vai trò module.

Scope In.

Scope Out.

Upstream Dependency.

Downstream Consumer.

Command Boundary.

Query Boundary.

P0 điểm chặn.

Evidence Requirement.

Smoke Requirement.

Fail-Safe Rule.

Không module nào trong TECH-07 được tự vượt quyền sang Commerce, Payment, Operational, AI Advisor hoặc Facebook Gateway.

## 2.3. Danh sách module bắt buộc của TECH-07

TECH-07 bắt buộc có tối thiểu các module contract sau:

Ads Measurement Runtime Orchestrator.

Pixel / CAPI Event Module.

Facebook Gateway Event Signal Module.

Campaign Registry Module.

Adset / Ad / Creative Registry Module.

Claim / Creative Approval Boundary Module.

Commerce Verified Revenue Mapper.

Quote / Cart / Order Draft Exclusion Module.

Funnel Metrics Module.

ROAS / CPA / AOV / Boxes Calculator.

COD Fail / Cancel / Refund Exclusion Module.

Attribution Resolver.

Live / Messenger / Landing Page Attribution Module.

Diamond Referral Conflict Resolver.

CRM Revenue Boundary Module.

Suppression Resolver.

Data Quality Guard.

Deduplication / Idempotency Guard.

Dashboard Boundary Module.

Pilot Control Module.

Scale Gate Module.

Owner Approval Module.

Evidence Package Module.

Release Handoff Module.

## 2.4. Module 01 - Ads Measurement Runtime Orchestrator

## 2.4.1. Vai trò

Ads Measurement Runtime Orchestrator là module điều phối trung tâm của TECH-07.

Module này không tạo dữ liệu nghiệp vụ mới.

Module này chỉ điều phối:

Nhận event.

Phân loại event.

Kiểm tra nguồn event.

Gọi Data Quality Guard.

Gọi Deduplication / Idempotency Guard.

Gọi Verified Revenue Mapper.

Gọi Attribution Resolver.

Gọi Suppression Resolver.

Gọi Dashboard Boundary.

Gọi Pilot / Scale Gate khi đủ điều kiện.

## 2.4.2. Scope In

Module này được phép:

Tiếp nhận signal từ Pixel/CAPI.

Tiếp nhận signal từ Facebook Gateway.

Tiếp nhận signal từ Landing Page / Messenger / Live.

Tiếp nhận trạng thái quote/cart/order draft từ Commerce dưới dạng funnel signal.

Tiếp nhận Verified Revenue từ Commerce.

Điều phối attribution.

Điều phối dashboard.

Điều phối pilot review.

Điều phối scale gate.

Điều phối suppression.

## 2.4.3. Scope Out

Module này không được:

Tự tạo quote.

Tự tạo cart.

Tự tạo order draft.

Tự tạo official order.

Tự xác nhận payment.

Tự xác nhận COD success.

Tự xác nhận Verified Revenue.

Tự sửa trạng thái Sale Lock / Recall.

Tự approve creative.

Tự approve claim.

Tự approve scale.

Tự override owner decision.

## 2.4.4. Upstream Dependency

Module này phụ thuộc:

TECH-03 Operational Core.

TECH-04 Commerce Runtime.

TECH-05 AI Advisor.

TECH-06 Facebook Gateway.

Product / SKU / Claim / Creative approval registry.

Owner approval decision.

## 2.4.5. Downstream Consumer

Module này phục vụ:

Ads Dashboard.

Pilot Review.

Scale Gate.

Owner Review.

Finance / KPI reporting.

CRM / Ads revenue reconciliation.

Diamond referral conflict review.

Executive summary.

## 2.4.6. P0 điểm chặn

Module này bị chặn nếu:

Không xác định được nguồn event.

Không phân biệt được funnel signal và revenue.

Không đọc được Verified Revenue từ Commerce.

Không kiểm tra được suppression.

Không chạy được dedup/idempotency.

Không xác định được data quality status.

Runtime unavailable nhưng vẫn cho scale.

Dashboard hiển thị revenue không có Commerce verification.

## 2.4.7. Evidence Requirement

Cần evidence:

Event source mapping.

Runtime dependency map.

Signal classification evidence.

Verified Revenue mapping evidence.

Suppression check evidence.

Data quality check evidence.

Scale gate decision evidence.

## 2.4.8. Smoke Requirement

Smoke tối thiểu:

Event chưa verified chỉ vào funnel.

Verified Revenue mới vào revenue.

Runtime thiếu Commerce thì không tính ROAS.

Runtime thiếu suppression status thì không scale.

Runtime thiếu owner approval thì không scale.

## 2.5. Module 02 - Pixel / CAPI Event Module

## 2.5.1. Vai trò

Pixel / CAPI Event Module ghi nhận sự kiện hành vi từ quảng cáo, website, landing page hoặc Meta ecosystem.

Module này chỉ ghi nhận event.

Module này không xác nhận doanh thu.

## 2.5.2. Scope In

Module này được phép ghi nhận:

Page view.

View content.

Lead.

Contact.

Landing page visit.

Add to cart signal nếu Commerce xác nhận cart hợp lệ.

Initiate checkout signal nếu Commerce xác nhận order draft / checkout intent hợp lệ.

Purchase event chỉ khi có Commerce Verified Revenue mapping hợp lệ.

Event identity.

Event source.

Event timestamp.

Campaign/adset/ad/creative identity nếu có.

## 2.5.3. Scope Out

Module này không được:

Tự tạo Purchase event từ quote.

Tự tạo Purchase event từ cart.

Tự tạo Purchase event từ order draft.

Tự tạo Purchase event từ payment WAITING.

Tự tạo Purchase event từ COD WAITING.

Tự tạo Purchase event từ ảnh chuyển khoản.

Tự tăng revenue.

Tự sửa revenue.

Tự xóa refund/cancel.

Tự bỏ qua dedup.

## 2.5.4. Upstream Dependency

Phụ thuộc:

Pixel configuration approved.

CAPI configuration approved.

Campaign registry.

Landing page event source.

Commerce mapping nếu event liên quan cart/checkout/purchase.

Deduplication / Idempotency Guard.

## 2.5.5. Downstream Consumer

Cung cấp dữ liệu cho:

Funnel Metrics.

Attribution Resolver.

Data Quality Guard.

Dashboard.

Pilot Review.

Scale Gate.

## 2.5.6. P0 điểm chặn

bị chặn nếu:

Pixel/CAPI gửi Purchase khi chưa có Verified Revenue.

Pixel/CAPI double count event.

Pixel/CAPI thiếu event identity.

Pixel/CAPI thiếu dedup key cho purchase-related event.

Pixel/CAPI revenue lệch Commerce.

Pixel/CAPI tự ghi nhận revenue từ WAITING order.

Pixel/CAPI không phân biệt test event và production event.

Pixel/CAPI không có evidence cấu hình.

## 2.5.7. Evidence Requirement

Cần evidence:

Pixel event map.

CAPI event map.

Dedup event key policy.

Purchase event gating rule.

Test event evidence.

Production event evidence.

Evidence đối chiếu purchase event với Commerce Verified Revenue.

## 2.5.8. Smoke Requirement

Smoke tối thiểu:

Page view ghi nhận đúng là signal.

Add to cart không tạo revenue.

Checkout intent không tạo revenue.

Purchase không phát nếu chưa Verified Revenue.

Pixel/CAPI cùng gửi event nhưng chỉ tính một lần.

Revenue từ Pixel/CAPI không lệch Commerce.

## 2.6. Module 03 - Facebook Gateway Event Signal Module

## 2.6.1. Vai trò

Facebook Gateway Event Signal Module nhận tín hiệu từ TECH-06 Facebook Gateway.

Module này dùng để đo chất lượng kênh:

Live.

Comment.

Messenger.

Handoff.

Conversation entry.

Public/private channel transition.

Module này không được tạo revenue.

## 2.6.2. Scope In

Module được phép nhận:

Comment received.

Comment classified.

Messenger handoff attempted.

Messenger handoff succeeded.

Messenger handoff failed.

Private conversation started.

Customer asked price.

Customer asked product.

Customer asked evidence.

Customer requested quote.

Customer moved to order draft.

Abuse/spam/moderation flag.

Rate limit flag.

Channel suppression flag.

Live session identity.

Facebook page identity.

## 2.6.3. Scope Out

Module không được:

Tự báo giá.

Tự tạo quote.

Tự tạo order.

Tự xác nhận payment.

Tự xác nhận revenue.

Tự lấy comment làm doanh thu.

Tự lấy inbox làm doanh thu.

Tự lấy handoff success làm doanh thu.

Tự bỏ qua privacy/channel boundary.

Tự public dữ liệu private.

## 2.6.4. Upstream Dependency

Phụ thuộc:

TECH-06 Facebook Gateway.

Meta Page Registry.

Live Session Registry.

Messenger handoff status.

Channel policy.

Moderation policy.

Rate limit / anti-spam policy.

## 2.6.5. Downstream Consumer

Cung cấp dữ liệu cho:

Funnel Metrics.

Live / Messenger Attribution.

Data Quality Guard.

Suppression Resolver.

Dashboard.

Pilot Review.

Scale Gate.

## 2.6.6. P0 điểm chặn

bị chặn nếu:

Gateway event bị tính là revenue.

Comment được tính là revenue.

Inbox được tính là revenue.

Handoff success được tính là revenue.

Không xác định được page/live/session.

Không phân biệt public/private signal.

Event từ page không được phép vẫn được đưa vào measurement.

Abuse/spam/moderation flag bị bỏ qua.

Channel suppression bị bỏ qua.

Gateway runtime unavailable nhưng vẫn scale.

## 2.6.7. Evidence Requirement

Cần evidence:

Page registry evidence.

Live session evidence.

Comment-to-Messenger evidence.

Handoff success/fail evidence.

Moderation evidence.

Rate limit evidence.

Channel policy evidence.

Event signal mapping evidence.

## 2.6.8. Smoke Requirement

Smoke tối thiểu:

Comment hỏi giá được ghi là funnel signal.

Messenger handoff thành công được ghi là channel signal.

Messenger handoff thất bại không được tính như private conversation thành công.

Abuse comment không kích hoạt revenue.

Gateway signal không tạo ROAS.

Page không nằm trong registry thì event bị block hoặc hold.

## 2.7. Module 04 - Campaign Registry Module

## 2.7.1. Vai trò

Campaign Registry Module quản lý danh tính và trạng thái đo lường của campaign.

Module này là nơi TECH-07 biết campaign nào đang được đo, thuộc sản phẩm nào, mục tiêu nào, creative nào, pilot nào và scale gate nào.

## 2.7.2. Scope In

Module được phép quản lý:

Campaign identity.

Campaign name.

Campaign objective.

Campaign owner.

Campaign status.

Campaign product/SKU scope.

Campaign channel.

Campaign start/end window.

Pilot window.

Scale status.

Suppression status.

Creative approval status reference.

Claim approval status reference.

Evidence status.

Owner approval status.

## 2.7.3. Scope Out

Module không được:

Tự approve campaign.

Tự approve creative.

Tự approve claim.

Tự tăng ngân sách.

Tự mở scale gate.

Tự sửa revenue.

Tự bỏ suppression.

Tự gán campaign cho order nếu Attribution Resolver chưa xác nhận.

## 2.7.4. Upstream Dependency

Phụ thuộc:

Ads planning decision.

Product / SKU approval.

Claim / creative approval.

Operational sellable status.

Commerce runtime readiness.

Gateway readiness.

Owner approval.

## 2.7.5. Downstream Consumer

Cung cấp dữ liệu cho:

Attribution Resolver.

Funnel Metrics.

ROAS Calculator.

Dashboard.

Pilot Module.

Scale Gate.

Evidence Package.

## 2.7.6. P0 điểm chặn

bị chặn nếu:

Campaign không có owner.

Campaign không có product/SKU scope rõ.

Campaign chạy SKU không sellable.

Campaign chạy SKU bị Sale Lock/Recall.

Campaign dùng creative chưa approved.

Campaign dùng claim chưa approved.

Campaign không có pilot window.

Campaign không có evidence status.

Campaign không có scale decision.

Campaign không map được adset/ad/creative.

## 2.7.7. Evidence Requirement

Cần evidence:

Campaign approval evidence.

Campaign objective evidence.

SKU scope evidence.

Claim/creative approval evidence.

Pilot approval evidence.

Scale decision evidence.

Suppression check evidence.

## 2.7.8. Smoke Requirement

Smoke tối thiểu:

Campaign thiếu SKU scope thì không đo scale.

Campaign có SKU bị Sale Lock thì scale bị chặn.

Campaign creative chưa approved thì scale bị chặn.

Campaign có pilot nhưng thiếu evidence thì scale WAITING.

Campaign không có owner approval thì scale bị chặn.

## 2.8. Module 05 - Adset / Ad / Creative Registry Module

## 2.8.1. Vai trò

Module này quản lý danh tính tầng adset, ad và creative để phục vụ attribution, dashboard và scale review.

Nếu thiếu module này, TECH-07 không thể biết doanh thu verified đến từ creative nào.

## 2.8.2. Scope In

Module được phép quản lý:

Adset identity.

Ad identity.

Creative identity.

Creative type.

Creative version.

Creative status.

Creative approval reference.

Claim approval reference.

Product/SKU mapping.

Landing destination.

Messenger/Live entry mapping.

Tracking status.

Suppression status.

## 2.8.3. Scope Out

Module không được:

Tự sửa nội dung creative.

Tự approve creative.

Tự approve claim.

Tự thay landing destination.

Tự đổi SKU mapping.

Tự gán revenue cho creative nếu thiếu attribution.

Tự scale creative.

## 2.8.4. Upstream Dependency

Phụ thuộc:

Campaign Registry.

Claim Approval.

Creative Approval.

Product Knowledge approved.

Channel destination registry.

Owner approval.

## 2.8.5. Downstream Consumer

Cung cấp dữ liệu cho:

Attribution Resolver.

Dashboard.

Creative performance review.

Pilot review.

Scale gate.

Evidence package.

## 2.8.6. P0 điểm chặn

bị chặn nếu:

Creative thiếu identity.

Creative thiếu version.

Creative chưa approved.

Claim trong creative chưa approved.

Creative map sai SKU.

Creative dùng SKU không sellable.

Creative bị suppression nhưng vẫn scale.

Creative không có evidence.

Creative performance bị gán revenue không có attribution.

## 2.8.7. Evidence Requirement

Cần evidence:

Creative version evidence.

Creative approval evidence.

Claim approval evidence.

SKU mapping evidence.

Destination mapping evidence.

Creative suppression evidence.

Creative performance evidence.

## 2.8.8. Smoke Requirement

Smoke tối thiểu:

Creative chưa approved không được scale.

Creative map sai SKU bị hold.

Creative có claim chưa approved bị block.

Creative không có identity không được attribution.

Creative bị suppression không được nhân rộng.

## 2.9. Module 06 - Claim / Creative Approval Boundary Module

## 2.9.1. Vai trò

Module này đảm bảo mọi nội dung quảng cáo, claim, creative và thông điệp public đều được phê duyệt trước khi đưa vào scale.

Đặc biệt với Cháo Sâm Savigin, module này bảo vệ ranh giới:

Bữa ăn thực dưỡng có lợi cho sức khỏe.

Không phải thuốc.

Không chữa bệnh.

Không điều trị.

Không thay thuốc.

Không cam kết hiệu quả y khoa.

Không suy diễn clinical claim.

## 2.9.2. Scope In

Module được phép kiểm tra:

Creative approval status.

Claim approval status.

Product public wording approval.

Scientific evidence reference approval.

Public-safe phrase approval.

Channel-safe wording.

Owner approval.

Version status.

Expiry/review status nếu có.

## 2.9.3. Scope Out

Module không được:

Tự viết claim mới.

Tự mở rộng claim.

Tự suy diễn công dụng.

Tự approve nội dung.

Tự bỏ qua owner approval.

Tự cho scale creative chưa duyệt.

Tự lấy hiệu quả bán hàng để hợp thức hóa claim sai.

## 2.9.4. Upstream Dependency

Phụ thuộc:

Product Knowledge approved.

Claim Registry.

Evidence Registry.

Owner approval.

AI Advisor public wording policy.

Channel policy.

## 2.9.5. Downstream Consumer

Cung cấp trạng thái cho:

Campaign Registry.

Creative Registry.

Suppression Resolver.

Scale Gate.

Dashboard.

Evidence Package.

## 2.9.6. P0 điểm chặn

bị chặn nếu:

Claim chưa approved.

Creative chưa approved.

Creative chứa claim y khoa vượt phạm vi.

Creative nói chữa bệnh/điều trị/thay thuốc.

Creative dùng evidence chưa approved.

Creative dùng public wording sai.

Creative bị owner reject nhưng vẫn chạy scale.

Claim expired/review WAITING nhưng vẫn scale.

## 2.9.7. Evidence Requirement

Cần evidence:

Claim approval evidence.

Creative approval evidence.

Evidence registry reference.

Owner approval evidence.

Public wording review evidence.

Channel-safe wording evidence.

## 2.9.8. Smoke Requirement

Smoke tối thiểu:

Creative có claim chưa approved bị block scale.

Creative có claim chữa bệnh bị reject.

Creative đúng public-safe wording mới pass.

Creative thiếu owner approval không scale.

Creative đã approved nhưng SKU bị Sale Lock vẫn không scale.

## 2.10. Module 07 - Commerce Verified Revenue Mapper

## 2.10.1. Vai trò

Commerce Verified Revenue Mapper là module quan trọng nhất của TECH-07 về doanh thu.

Module này nhận Verified Revenue từ Commerce Runtime.

Module này không tự tạo Verified Revenue.

Module này chỉ map revenue đã được Commerce xác minh vào Ads Measurement.

## 2.10.2. Scope In

Module được phép nhận:

Official order identity.

Order verification status.

Payment confirmed status.

COD success status.

COD fail status.

Cancel status.

Refund status.

Invalid order status.

Verified revenue amount.

Revenue verification timestamp.

Revenue exclusion status.

Order line/SKU scope ở mức cần thiết cho measurement.

Program/discount context đã được Commerce xác nhận.

Customer/session reference trong phạm vi privacy.

## 2.10.3. Scope Out

Module không được:

Tự xác nhận payment.

Tự xác nhận COD success.

Tự chuyển order sang verified.

Tự sửa amount.

Tự cộng thêm shipping/COD/VAT nếu Commerce chưa xác nhận.

Tự loại refund nếu Commerce chưa xác nhận.

Tự hồi revenue đã bị Commerce exclude.

Tự tạo order_code.

Tự dùng ảnh chuyển khoản làm PAID.

Tự xem unpaid order là revenue.

## 2.10.4. Upstream Dependency

Phụ thuộc:

TECH-04 Commerce Runtime.

Payment Confirmation.

COD Result.

Order Verification.

Refund/Cancel policy.

Revenue verification policy.

## 2.10.5. Downstream Consumer

Cung cấp dữ liệu cho:

Attribution Resolver.

ROAS/CPA/AOV Calculator.

Dashboard.

Pilot Review.

Scale Gate.

Evidence Package.

Ads/CRM/Diamond reconciliation.

## 2.10.6. P0 điểm chặn

bị chặn nếu:

Không có Commerce Verified Revenue.

Payment WAITING bị tính revenue.

COD WAITING bị tính revenue.

COD fail bị tính revenue.

Cancelled order bị tính revenue trái policy.

Refunded order bị tính revenue trái policy.

Invalid order bị tính revenue.

Order chưa verified bị tính revenue.

Revenue amount lệch Commerce.

Revenue bị double count.

Revenue không có evidence.

Revenue không có trạng thái exclusion rõ.

## 2.10.7. Evidence Requirement

Cần evidence:

Commerce verified revenue evidence.

Payment confirmation evidence.

COD success/fail evidence.

Cancel/refund evidence.

Invalid order evidence.

Revenue amount reconciliation evidence.

Exclusion logic evidence.

## 2.10.8. Smoke Requirement

Smoke tối thiểu:

Payment WAITING không vào revenue.

COD WAITING không vào revenue.

COD fail không vào revenue.

Order verified + payment confirmed mới vào revenue.

Refund sau đó làm revenue bị loại theo policy.

Revenue dashboard khớp Commerce.

## 2.11. Module 08 - Quote / Cart / Order Draft Exclusion Module

## 2.11.1. Vai trò

Module này đảm bảo các trạng thái chưa phải doanh thu không bị đưa nhầm vào revenue.

Đây là module chống “ROAS ảo”.

## 2.11.2. Scope In

Module được phép nhận:

Quote created.

Quote accepted.

Quote expired.

Cart created.

Cart updated.

Cart abandoned.

Order Draft created.

Order Draft viewed.

Order Draft confirmed by customer.

Payment method selected.

Payment intent.

Unpaid order.

WAITING payment.

## WAITING COD.

## 2.11.3. Scope Out

Module không được:

Tính quote value là revenue.

Tính cart value là revenue.

Tính order draft value là revenue.

Tính unpaid order là revenue.

Tính payment intent là revenue.

Tính WAITING COD là revenue.

Gửi Purchase event từ các trạng thái này.

Tính ROAS từ các trạng thái này.

## 2.11.4. Upstream Dependency

Phụ thuộc:

TECH-04 Commerce Runtime.

QuoteSnapshot.

Cart state.

Order Draft state.

Payment state.

COD state.

## 2.11.5. Downstream Consumer

Cung cấp dữ liệu cho:

Funnel Metrics.

Drop-off Analysis.

Dashboard.

Pilot Review.

Data Quality Guard.

## 2.11.6. P0 điểm chặn

bị chặn nếu:

Quote bị tính là revenue.

Cart bị tính là revenue.

Order Draft bị tính là revenue.

Unpaid order bị tính là revenue.

Quote expired vẫn dùng làm revenue.

Payment intent được tính là payment success.

COD WAITING được tính là COD success.

Purchase event phát từ order draft.

## 2.11.7. Evidence Requirement

Cần evidence:

Quote exclusion evidence.

Cart exclusion evidence.

Order draft exclusion evidence.

Unpaid order exclusion evidence.

Payment WAITING exclusion evidence.

COD WAITING exclusion evidence.

## 2.11.8. Smoke Requirement

Smoke tối thiểu:

Quote value xuất hiện trong funnel nhưng revenue = 0.

Cart value xuất hiện trong funnel nhưng revenue = 0.

Order Draft value xuất hiện trong funnel nhưng revenue = 0.

Payment WAITING không tạo purchase.

COD WAITING không tạo purchase.

Quote expired không tạo official order revenue.

## 2.12. Module 09 - Funnel Metrics Module

## 2.12.1. Vai trò

Funnel Metrics Module tính các chỉ số phễu quảng cáo.

Module này giúp đánh giá đường đi từ Ads đến tư vấn đến quote đến order.

Module này không tính revenue nếu chưa có Verified Revenue.

## 2.12.2. Scope In

Module được phép tính:

Impression.

Reach.

Click.

Landing visit.

Comment Rate.

Inbox Rate.

Messenger Handoff Rate.

Consultation Rate.

Quote Rate.

Cart Rate.

Order Draft Rate.

Customer Confirmation Rate.

Payment Intent Rate.

Payment Success Rate.

Verified Rate.

Drop-off by stage.

## 2.12.3. Scope Out

Module không được:

Tự tính revenue.

Tự tính ROAS từ funnel.

Tự xem quote rate cao là doanh thu.

Tự xem inbox nhiều là campaign profitable.

Tự xem comment nhiều là campaign scale-ready.

Tự bỏ qua data quality.

Tự override attribution.

## 2.12.4. Upstream Dependency

Phụ thuộc:

Pixel/CAPI Event Module.

Facebook Gateway Event Signal Module.

Quote/Cart/Order Draft Exclusion Module.

Commerce Verified Revenue Mapper.

Data Quality Guard.

## 2.12.5. Downstream Consumer

Cung cấp dữ liệu cho:

Dashboard.

Pilot Review.

Campaign Review.

Creative Review.

Scale Gate.

Owner Review.

## 2.12.6. P0 điểm chặn

bị chặn nếu:

Funnel signal bị gọi là revenue.

Comment Rate bị dùng thay ROAS.

Inbox Rate bị dùng thay revenue.

Quote Rate bị dùng thay Verified Revenue.

Payment Rate tính cả WAITING payment.

Verified Rate tính cả unverified order.

Funnel không có event dedup.

Funnel thiếu stage definition.

## 2.12.7. Evidence Requirement

Cần evidence:

Funnel stage definition.

Event source mapping.

Metric calculation rule.

Dedup evidence.

Funnel-to-revenue boundary evidence.

Drop-off evidence.

## 2.12.8. Smoke Requirement

Smoke tối thiểu:

Comment tăng nhưng revenue không tăng nếu không có verified order.

Inbox tăng nhưng ROAS không tăng nếu không có Verified Revenue.

Quote nhiều nhưng revenue vẫn bằng 0 nếu chưa verified.

Payment WAITING không làm Payment Success Rate tăng.

Verified Rate chỉ tăng khi Commerce verified.

## 2.13. Module 10 - ROAS / CPA / AOV / Boxes Calculator

## 2.13.1. Vai trò

Module này tính các chỉ số hiệu quả kinh doanh của quảng cáo dựa trên dữ liệu hợp lệ.

Các chỉ số chính:

## ROAS.

## CPA.

## AOV.

Boxes per Order.

Verified Revenue.

Cost per Quote.

Cost per Verified Order.

Cost per Payment Success.

## 2.13.2. Scope In

Module được phép dùng:

Ads Spend hợp lệ.

Verified Revenue từ Commerce.

Verified Order count.

Verified order line count.

Verified product/box quantity.

Valid attribution result.

Refund/cancel exclusion result.

Data quality pass result.

Campaign/adset/ad/creative mapping.

## 2.13.3. Scope Out

Module không được dùng:

Quote value làm revenue.

Cart value làm revenue.

Order Draft value làm revenue.

WAITING payment làm revenue.

COD WAITING làm revenue.

COD fail làm revenue.

Cancelled order làm revenue nếu excluded.

Refunded order làm revenue nếu excluded.

Invalid order làm revenue.

AI estimated revenue.

Gateway estimated revenue.

Manual adjusted revenue không evidence.

## 2.13.4. Công thức quản trị

ROAS quản trị:

Verified Revenue / Ads Spend hợp lệ.

CPA theo verified order:

Ads Spend hợp lệ / Số verified orders hợp lệ.

## AOV:

Verified Revenue / Số verified orders hợp lệ.

Boxes per Order:

Tổng số box/gói/hộp bán hợp lệ theo Commerce policy / Số verified orders hợp lệ.

Cost per Quote:

Ads Spend hợp lệ / Số quote hợp lệ.

Cost per Inbox:

Ads Spend hợp lệ / Số inbox hợp lệ.

Các chỉ số Cost per Quote, Cost per Inbox chỉ là chỉ số phễu, không thay thế ROAS.

## 2.13.5. Upstream Dependency

Phụ thuộc:

Ads Spend source approved.

Commerce Verified Revenue Mapper.

Attribution Resolver.

COD/Cancel/Refund Exclusion Module.

Data Quality Guard.

Deduplication Guard.

## 2.13.6. Downstream Consumer

Cung cấp dữ liệu cho:

Dashboard.

Pilot Review.

Scale Gate.

Owner Review.

Financial review.

Campaign optimization.

## 2.13.7. P0 điểm chặn

bị chặn nếu:

ROAS tính từ quote.

ROAS tính từ cart.

ROAS tính từ order draft.

ROAS tính từ unpaid order.

CPA tính trên order chưa verified.

AOV tính trên order chưa verified.

Boxes per Order tính cả cancelled/refunded/invalid order.

Ads Spend thiếu nguồn.

Revenue bị double count.

Attribution conflict chưa resolve nhưng vẫn đưa vào ROAS.

Data quality fail nhưng vẫn tính scale-ready.

## 2.13.8. Evidence Requirement

Cần evidence:

Ads Spend evidence.

Verified Revenue evidence.

Verified Order count evidence.

AOV calculation evidence.

CPA calculation evidence.

ROAS calculation evidence.

Boxes per Order evidence.

Exclusion evidence.

Attribution evidence.

## 2.13.9. Smoke Requirement

Smoke tối thiểu:

Quote nhiều nhưng ROAS vẫn không tăng nếu chưa có Verified Revenue.

Verified Revenue tăng thì ROAS được tính lại.

Refund làm ROAS điều chỉnh theo policy.

Ads Spend thiếu evidence thì ROAS bị hold.

Attribution conflict thì revenue không vào ROAS scale-ready.

## 2.14. Module 11 - COD Fail / Cancel / Refund Exclusion Module

## 2.14.1. Vai trò

Module này loại trừ các đơn không được tính revenue theo policy.

Mục tiêu là bảo vệ dashboard khỏi doanh thu giả.

## 2.14.2. Scope In

Module được phép xử lý:

## COD WAITING.

COD success.

COD fail.

Customer cancelled.

System cancelled.

Payment failed.

Payment reversed.

Refund full.

Refund partial.

Invalid order.

Duplicate order.

Fraud/suspicious order.

Out-of-policy order.

## 2.14.3. Scope Out

Module không được:

Tự quyết định refund nếu Commerce chưa xác nhận.

Tự xóa revenue không có policy.

Tự giữ revenue COD fail.

Tự giữ revenue cancelled order.

Tự giữ revenue invalid order.

Tự biến WAITING thành success.

Tự override Finance/Commerce decision.

## 2.14.4. Upstream Dependency

Phụ thuộc:

Commerce Runtime.

Payment status.

COD result.

Cancel policy.

Refund policy.

Fraud/invalid order policy.

Finance confirmation nếu cần.

## 2.14.5. Downstream Consumer

Cung cấp dữ liệu cho:

Commerce Verified Revenue Mapper.

ROAS/CPA/AOV Calculator.

Dashboard.

Data Quality Guard.

Scale Gate.

Evidence Package.

## 2.14.6. P0 điểm chặn

bị chặn nếu:

COD fail vẫn tính revenue.

COD WAITING vẫn tính revenue.

Cancelled order vẫn tính revenue trái policy.

Refunded order vẫn tính revenue trái policy.

Invalid order vẫn tính revenue.

Duplicate order vẫn tính revenue hai lần.

Exclusion status thiếu evidence.

Dashboard không phản ánh exclusion.

## 2.14.7. Evidence Requirement

Cần evidence:

COD success/fail evidence.

Cancel evidence.

Refund evidence.

Invalid order evidence.

Duplicate order evidence.

Revenue adjustment evidence.

Dashboard reconciliation evidence.

## 2.14.8. Smoke Requirement

Smoke tối thiểu:

COD fail làm revenue = 0.

Cancelled order bị exclude.

Full refund bị exclude theo policy.

Partial refund điều chỉnh revenue theo policy.

Duplicate order không double count.

Dashboard sau exclusion khớp Commerce.

## 2.15. Module 12 - Attribution Resolver

## 2.15.1. Vai trò

Attribution Resolver xác định Verified Revenue nên được phân bổ về nguồn nào.

Module này không tạo revenue.

Module này chỉ phân bổ doanh thu đã verified.

## 2.15.2. Scope In

Module được phép xử lý:

Campaign attribution.

Adset attribution.

Ad attribution.

Creative attribution.

Placement attribution.

Live session attribution.

Messenger session attribution.

Landing page attribution.

Referral attribution.

CRM touchpoint attribution.

Direct/organic attribution.

Unattributed verified revenue.

Attribution conflict WAITING.

## 2.15.3. Scope Out

Module không được:

Tự tạo revenue.

Tự tăng revenue.

Tự gán doanh thu cho campaign thiếu evidence.

Tự gán doanh thu cho creative thiếu identity.

Tự cộng đủ doanh thu cho nhiều nguồn.

Tự ưu tiên Ads hơn Diamond nếu chưa có rule.

Tự ưu tiên CRM hơn Ads nếu chưa có rule.

Tự bỏ conflict.

Tự sửa Commerce order state.

## 2.15.4. Upstream Dependency

Phụ thuộc:

Verified Revenue Mapper.

Campaign Registry.

Adset/Ad/Creative Registry.

Pixel/CAPI Event Module.

Gateway Event Signal Module.

Live/Messenger/Landing Attribution Module.

Diamond Conflict Resolver.

CRM Revenue Boundary Module.

Data Quality Guard.

## 2.15.5. Downstream Consumer

Cung cấp dữ liệu cho:

ROAS/CPA/AOV Calculator.

Dashboard.

Pilot Review.

Scale Gate.

Owner Review.

Evidence Package.

## 2.15.6. P0 điểm chặn

bị chặn nếu:

Attribution thiếu event trail.

Attribution thiếu campaign/ad/creative identity.

Attribution conflict nhưng vẫn tính revenue sạch.

Revenue được phân bổ trùng.

Revenue được gán bừa cho campaign tốt nhất.

No bind nhưng vẫn tính Diamond attribution.

Ads/CRM/Diamond không có priority rule.

Attribution resolver dùng unverified revenue.

Attribution không có evidence.

## 2.15.7. Evidence Requirement

Cần evidence:

Attribution event trail.

Campaign/adset/ad/creative mapping.

Touchpoint evidence.

Attribution window evidence.

Conflict decision evidence.

Unattributed revenue evidence.

Owner-approved attribution policy.

## 2.15.8. Smoke Requirement

Smoke tối thiểu:

Verified Revenue có đủ event trail được attribution.

Verified Revenue thiếu campaign identity vào unattributed.

Ads + Diamond conflict vào WAITING nếu chưa có rule.

CRM + Ads conflict không double count.

Attribution không dùng order chưa verified.

## 2.16. Module 13 - Live / Messenger / Landing Page Attribution Module

## 2.16.1. Vai trò

Module này phân loại và hỗ trợ attribution theo điểm vào kênh.

Nó xác định khách đến từ:

Live.

Comment.

Messenger.

Landing page.

Website.

Direct.

Referral.

Module này không tự gán revenue nếu chưa có Verified Revenue và Attribution Resolver xác nhận.

## 2.16.2. Scope In

Module được phép xử lý:

Live session identity.

Comment thread identity.

Messenger conversation identity.

Handoff success/fail.

Landing page session.

Click source.

UTM/source/campaign nếu có.

Customer/session linkage trong phạm vi privacy.

Channel timestamp.

Channel sequence.

## 2.16.3. Scope Out

Module không được:

Tự xác nhận khách mua.

Tự xác nhận order.

Tự xác nhận payment.

Tự tính revenue.

Tự gán tất cả Messenger revenue cho Ads.

Tự gán tất cả Live revenue cho Ads.

Tự gán landing visit thành order.

Tự bỏ qua handoff failed.

Tự nối sai khách/session.

## 2.16.4. Upstream Dependency

Phụ thuộc:

Facebook Gateway Event Signal.

Pixel/CAPI Event.

Landing event source.

Commerce session/order mapping.

Privacy boundary.

Data Quality Guard.

## 2.16.5. Downstream Consumer

Cung cấp dữ liệu cho:

Attribution Resolver.

Funnel Metrics.

Dashboard.

Pilot Review.

Scale Gate.

## 2.16.6. P0 điểm chặn

bị chặn nếu:

Handoff failed nhưng vẫn tính private conversation.

Landing visit bị tính là revenue.

Messenger conversation bị tính là revenue.

Live comment bị tính là revenue.

Session mapping sai khách.

Channel source thiếu evidence.

Không tách organic/direct/ads/referral.

Không tôn trọng privacy boundary.

## 2.16.7. Evidence Requirement

Cần evidence:

Live session mapping.

Messenger handoff mapping.

Landing session mapping.

Channel sequence evidence.

Privacy-safe mapping evidence.

Attribution handoff evidence.

## 2.16.8. Smoke Requirement

Smoke tối thiểu:

Live comment tạo engagement signal.

Messenger handoff success tạo channel signal.

Landing visit tạo visit signal.

Không event nào trong ba loại trên tự tạo revenue.

Verified Revenue có channel sequence mới được attribution.

## 2.17. Module 14 - Diamond Referral Conflict Resolver

## 2.17.1. Vai trò

Module này xử lý xung đột giữa Ads attribution và Diamond referral attribution.

Mục tiêu là không tính trùng doanh thu và không phát sinh hoa hồng sai.

## 2.17.2. Scope In

Module được phép xử lý:

Referral link identity.

Referral owner identity.

Referral owner tier.

Bind status.

Bound timestamp.

Buyer source.

Ads touchpoint.

CRM touchpoint.

Order identity.

Verified Revenue.

Commission eligibility status.

Conflict status.

Owner-approved priority/allocation rule.

## 2.17.3. Scope Out

Module không được:

Tự tạo Diamond bind.

Tự xác nhận Diamond tier.

Tự phát sinh commission.

Tự cộng revenue cho cả Ads và Diamond.

Tự ưu tiên Diamond khi no bind.

Tự ưu tiên Ads khi có Diamond bind nếu chưa có rule.

Tự quyết định payout.

Tự sửa Commerce/Affiliate state.

## 2.17.4. Upstream Dependency

Phụ thuộc:

Commerce Runtime.

Diamond Referral Resolver.

Affiliate policy.

Attribution Resolver.

Verified Revenue Mapper.

Owner-approved conflict policy.

## 2.17.5. Downstream Consumer

Cung cấp dữ liệu cho:

Attribution Resolver.

ROAS Calculator.

Commission review.

Dashboard.

Scale Gate.

Evidence Package.

## 2.17.6. P0 điểm chặn

bị chặn nếu:

No bind nhưng vẫn tính Diamond attribution.

Diamond bind không rõ nhưng vẫn tính commission.

Ads + Diamond cùng nhận 100% revenue khi chưa có rule.

Conflict không được đánh dấu.

Commission tính trên unpaid order.

Commission tính trên unverified revenue.

Diamond self-purchase không được loại theo policy.

Conflict revenue được dùng để scale khi chưa resolve.

## 2.17.7. Evidence Requirement

Cần evidence:

Referral bind evidence.

Referral owner tier evidence.

Buyer source evidence.

Ads touchpoint evidence.

Conflict policy evidence.

Commission eligibility evidence.

Conflict resolution evidence.

## 2.17.8. Smoke Requirement

Smoke tối thiểu:

Valid bind + Ads touchpoint = conflict review.

Conflict WAITING không vào scale revenue.

Commission không tính nếu revenue chưa verified.

Diamond self-purchase bị xử lý theo policy.

## 2.18. Module 15 - CRM Revenue Boundary Module

## 2.18.1. Vai trò

Module này tách doanh thu CRM khỏi doanh thu Ads.

Mục tiêu là không dùng CRM revenue để làm đẹp ROAS Ads và không dùng Ads revenue để làm sai KPI CRM.

## 2.18.2. Scope In

Module được phép xử lý:

CRM campaign identity.

CRM message type.

CRM trigger.

CRM channel.

CRM touchpoint timestamp.

Customer membership context.

Repurchase journey.

Birthday/Tết/Golden Hour reminder.

Tier upgrade message.

Order success message.

Verified Revenue linked to CRM touchpoint.

Ads touchpoint conflict.

Diamond touchpoint conflict.

## 2.18.3. Scope Out

Module không được:

Tự gửi CRM message.

Tự tạo order.

Tự xác nhận revenue.

Tự lấy CRM touchpoint làm revenue.

Tự cộng CRM revenue vào Ads ROAS.

Tự cộng Ads revenue vào CRM performance.

Tự bỏ opt-out.

Tự bỏ attribution conflict.

## 2.18.4. Upstream Dependency

Phụ thuộc:

CRM policy.

Customer opt-out status.

Commerce Verified Revenue.

Ads Attribution Resolver.

Diamond Conflict Resolver.

Owner-approved attribution priority.

## 2.18.5. Downstream Consumer

Cung cấp dữ liệu cho:

Attribution Resolver.

Dashboard.

CRM dashboard.

Ads dashboard.

Scale Gate.

Revenue reconciliation.

## 2.18.6. P0 điểm chặn

bị chặn nếu:

CRM revenue bị cộng vào Ads revenue khi chưa có rule.

Ads revenue bị cộng vào CRM revenue khi chưa có rule.

CRM opt-out bị bỏ qua.

CRM touchpoint thiếu evidence.

CRM message không thuộc trigger whitelist nhưng vẫn tính revenue.

Conflict Ads/CRM không được đánh dấu.

CRM revenue chưa verified vẫn tính KPI.

## 2.18.7. Evidence Requirement

Cần evidence:

CRM trigger evidence.

CRM message log evidence.

Customer opt-out evidence.

Verified Revenue evidence.

Attribution priority evidence.

Conflict resolution evidence.

## 2.18.8. Smoke Requirement

Smoke tối thiểu:

CRM message có verified order nhưng Ads touchpoint cũng có thì conflict được xử lý.

CRM opt-out không gửi/không attribution sai.

CRM revenue không tự vào Ads ROAS.

Ads revenue không tự vào CRM KPI.

CRM touchpoint thiếu evidence thì không attribution.

## 2.19. Module 16 - Suppression Resolver

## 2.19.1. Vai trò

Suppression Resolver xác định campaign/adset/ad/creative/SKU có bị chặn chạy, chặn scale hoặc chặn đo lường scale-ready hay không.

Suppression thắng ROAS.

## 2.19.2. Scope In

Module được phép nhận suppression từ:

Operational Core.

Sale Lock.

Recall.

Warehouse readiness.

Public Trace readiness.

Commerce Runtime.

Payment/COD issue.

AI Advisor.

Final Response Guard issue.

Facebook Gateway.

Meta permission issue.

Rate limit / anti-spam.

Abuse / moderation.

CRM opt-out.

Claim approval.

Creative approval.

Data quality.

Owner decision.

## 2.19.3. Scope Out

Module không được:

Tự gỡ suppression.

Tự override Sale Lock.

Tự override Recall.

Tự override ClaimGuard.

Tự override creative rejection.

Tự override Commerce block.

Tự override Gateway block.

Tự cho scale vì ROAS tốt.

Tự bỏ owner approval.

## 2.19.4. Upstream Dependency

Phụ thuộc:

TECH-03 Operational Core.

TECH-04 Commerce Runtime.

TECH-05 AI Advisor.

TECH-06 Facebook Gateway.

Claim/Creative Approval Module.

Data Quality Guard.

Owner Approval Module.

## 2.19.5. Downstream Consumer

Cung cấp dữ liệu cho:

Campaign Registry.

Creative Registry.

Dashboard.

Pilot Review.

Scale Gate.

Evidence Package.

## 2.19.6. P0 điểm chặn

bị chặn nếu:

Sale Lock active nhưng campaign vẫn scale.

Recall active nhưng campaign vẫn scale.

SKU không sellable nhưng campaign vẫn scale.

Claim chưa approved nhưng campaign vẫn scale.

Creative chưa approved nhưng campaign vẫn scale.

Data quality fail nhưng campaign vẫn scale.

Gateway suppression active nhưng campaign vẫn scale.

Owner reject nhưng campaign vẫn scale.

Suppression status không hiển thị dashboard.

## 2.19.7. Evidence Requirement

Cần evidence:

Suppression source evidence.

Suppression reason evidence.

Suppression active/cleared status.

Owner decision evidence nếu gỡ suppression.

Dashboard suppression evidence.

Scale gate suppression evidence.

## 2.19.8. Smoke Requirement

Smoke tối thiểu:

Sale Lock chặn campaign scale.

Recall chặn campaign scale.

Creative suppression chặn scale.

Data quality suppression chặn scale.

Suppression cleared cần owner evidence mới scale lại.

## 2.20. Module 17 - Data Quality Guard

## 2.20.1. Vai trò

Data Quality Guard kiểm tra dữ liệu có đủ tin cậy để đo lường, báo cáo và scale hay không.

Không có Data Quality Pass thì không có Scale Ready.

## 2.20.2. Scope In

Module kiểm tra:

Event completeness.

Event identity.

Event timestamp.

Campaign identity.

Adset identity.

Ad identity.

Creative identity.

Channel identity.

Customer/session linkage trong phạm vi privacy.

Quote/order/payment mapping.

Verified revenue mapping.

Dedup status.

Attribution status.

Suppression status.

Exclusion status.

Dashboard reconciliation.

Evidence completeness.

## 2.20.3. Scope Out

Module không được:

Tự sửa dữ liệu nguồn.

Tự bù event thiếu.

Tự đoán attribution.

Tự tạo campaign identity.

Tự tạo revenue.

Tự bỏ qua lỗi để scale.

Tự đánh pass khi thiếu evidence.

Tự làm đẹp dashboard.

## 2.20.4. Upstream Dependency

Phụ thuộc:

Pixel/CAPI Event.

Gateway Signal.

Commerce Verified Revenue.

Attribution Resolver.

Dedup Guard.

Dashboard Boundary.

Evidence Package.

## 2.20.5. Downstream Consumer

Cung cấp trạng thái cho:

Dashboard.

Pilot Review.

Scale Gate.

Owner Review.

Release Handoff.

## 2.20.6. P0 điểm chặn

bị chặn nếu:

Event thiếu identity.

Purchase event thiếu dedup.

Revenue lệch Commerce.

Attribution conflict chưa xử lý.

Dashboard không reconcile.

Suppression status không đọc được.

Evidence thiếu.

Data quality fail nhưng scale gate vẫn pass.

Runtime unavailable nhưng data quality vẫn pass.

## 2.20.7. Evidence Requirement

Cần evidence:

Data quality checklist.

Event completeness report.

Dedup report.

Revenue reconciliation report.

Attribution conflict report.

Dashboard reconciliation evidence.

Suppression status evidence.

## 2.20.8. Smoke Requirement

Smoke tối thiểu:

Thiếu campaign identity -> Data Quality Hold.

Revenue lệch Commerce -> Data Quality Fail.

Dedup fail -> Data Quality Fail.

Suppression unreadable -> Data Quality Hold.

Evidence thiếu -> Data Quality WAITING.

Data Quality Fail -> Scale Gate bị chặn.

## 2.21. Module 18 - Deduplication / Idempotency Guard

## 2.21.1. Vai trò

Deduplication / Idempotency Guard chống ghi nhận trùng event và trùng revenue.

Đây là module bắt buộc để tránh double count ROAS.

## 2.21.2. Scope In

Module kiểm tra:

Pixel event duplicate.

CAPI event duplicate.

Pixel/CAPI same-event dedup.

Gateway retry event.

Messenger handoff retry.

Landing page reload.

Payment callback retry.

Order status sync retry.

Dashboard job rerun.

Manual import duplicate.

Purchase/revenue duplicate.

Attribution rerun duplicate.

## 2.21.3. Scope Out

Module không được:

Tự xóa dữ liệu nguồn không có policy.

Tự merge sai khách.

Tự merge sai order.

Tự bỏ revenue hợp lệ.

Tự tạo event identity.

Tự bỏ qua duplicate warning.

Tự cho purchase event pass khi thiếu idempotency.

## 2.21.4. Upstream Dependency

Phụ thuộc:

Pixel/CAPI event identity.

Gateway event identity.

Commerce order identity.

Verified Revenue identity.

Attribution event trail.

Runtime replay policy.

## 2.21.5. Downstream Consumer

Cung cấp dữ liệu cho:

Data Quality Guard.

Verified Revenue Mapper.

Attribution Resolver.

ROAS Calculator.

Dashboard.

Evidence Package.

## 2.21.6. P0 điểm chặn

bị chặn nếu:

Purchase event không có dedup key.

Pixel/CAPI duplicate bị tính hai lần.

Payment callback retry tạo revenue mới.

Order sync retry tạo verified revenue mới.

Dashboard rerun nhân đôi revenue.

Attribution rerun nhân đôi revenue.

Duplicate không được log/evidence.

Không xác định được event idempotency.

## 2.21.7. Evidence Requirement

Cần evidence:

Dedup policy evidence.

Event identity evidence.

Duplicate detection evidence.

Duplicate resolution evidence.

Purchase dedup evidence.

Revenue dedup evidence.

Retry handling evidence.

## 2.21.8. Smoke Requirement

Smoke tối thiểu:

Pixel/CAPI gửi cùng event chỉ tính một lần.

Payment callback retry không tạo revenue mới.

Order sync retry không nhân đôi revenue.

Dashboard rerun không đổi số revenue.

Attribution rerun không double count.

## 2.22. Module 19 - Dashboard Boundary Module

## 2.22.1. Vai trò

Dashboard Boundary Module quy định dashboard được hiển thị gì, không được hiển thị gì, và trạng thái nào được gọi là scale-ready.

Dashboard là lớp hiển thị, không phải nguồn sự thật.

## 2.22.2. Scope In

Dashboard được phép hiển thị:

Ads Spend.

Funnel signal.

Comment Rate.

Inbox Rate.

Quote Rate.

Order Draft Rate.

Payment Rate.

Verified Rate.

Verified Revenue.

## ROAS.

## CPA.

## AOV.

Boxes per Order.

COD Fail Rate.

Cancel Rate.

Refund Rate.

Attribution status.

Data Quality status.

Suppression status.

Pilot status.

Scale Gate status.

Owner approval status.

Evidence status.

## 2.22.3. Scope Out

Dashboard không được:

Tự sửa số liệu.

Tự tạo revenue.

Tự thay Commerce Verified Revenue.

Tự tính ROAS từ quote/cart/order draft.

Tự ẩn data quality fail.

Tự ẩn suppression.

Tự ẩn attribution conflict.

Tự gọi campaign là scale-ready khi scale gate chưa pass.

Tự làm đẹp số liệu để trình owner.

## 2.22.4. Upstream Dependency

Phụ thuộc:

Funnel Metrics.

ROAS/CPA/AOV Calculator.

Commerce Verified Revenue Mapper.

Attribution Resolver.

Data Quality Guard.

Suppression Resolver.

Pilot Control.

Scale Gate.

Evidence Package.

## 2.22.5. Downstream Consumer

Phục vụ:

Owner Review.

Marketing Review.

Finance Review.

Operations Review.

Scale Decision.

Release Handoff.

## 2.22.6. P0 điểm chặn

bị chặn nếu:

Dashboard revenue lệch Commerce.

Dashboard không phân biệt signal và revenue.

Dashboard ẩn COD fail/refund/cancel.

Dashboard ẩn data quality fail.

Dashboard ẩn suppression.

Dashboard ẩn attribution conflict.

Dashboard gọi ROAS khi chưa có Verified Revenue.

Dashboard gọi scale-ready khi chưa owner approval.

## 2.22.7. Evidence Requirement

Cần evidence:

Dashboard metric definition.

Revenue reconciliation evidence.

Funnel/revenue separation evidence.

Data quality display evidence.

Suppression display evidence.

Attribution conflict display evidence.

Owner review evidence.

## 2.22.8. Smoke Requirement

Smoke tối thiểu:

Dashboard quote value không hiển thị là revenue.

Dashboard verified revenue khớp Commerce.

Dashboard hiện Data Quality Fail.

Dashboard hiện Suppression Active.

Dashboard hiện Attribution Conflict WAITING.

Dashboard không scale-ready nếu owner chưa approval.

## 2.23. Module 20 - Pilot Control Module

## 2.23.1. Vai trò

Pilot Control Module quản lý giai đoạn pilot 7-14 ngày trước khi scale.

Pilot không chỉ để xem ROAS.

Pilot là giai đoạn kiểm tra dữ liệu, event, attribution, revenue verification, suppression và vận hành thật.

## 2.23.2. Scope In

Module quản lý:

Pilot objective.

Pilot start date.

Pilot end date.

Campaign scope.

SKU scope.

Creative scope.

Channel scope.

Budget scope.

Data quality criteria.

Event tracking criteria.

Verified revenue criteria.

Suppression criteria.

Owner review criteria.

Scale recommendation status.

## 2.23.3. Scope Out

Module không được:

Tự mở scale.

Tự tăng ngân sách.

Tự bỏ qua data quality.

Tự bỏ qua suppression.

Tự bỏ qua creative approval.

Tự bỏ qua claim approval.

Tự kết luận pilot pass nếu thiếu evidence.

Tự rút ngắn pilot trái owner decision.

## 2.23.4. Upstream Dependency

Phụ thuộc:

Campaign Registry.

Creative Registry.

Pixel/CAPI Event.

Gateway Signal.

Verified Revenue Mapper.

Data Quality Guard.

Suppression Resolver.

Dashboard.

Evidence Package.

## 2.23.5. Downstream Consumer

Cung cấp dữ liệu cho:

Scale Gate.

Owner Approval.

Release Handoff.

Future campaign planning.

## 2.23.6. P0 điểm chặn

bị chặn nếu:

Pilot dưới thời lượng owner-approved nhưng vẫn scale.

Pilot thiếu evidence nhưng vẫn pass.

Pilot thiếu data quality nhưng vẫn pass.

Pilot thiếu verified revenue mapping nhưng vẫn pass.

Pilot có suppression nhưng vẫn pass.

Pilot có creative/claim issue nhưng vẫn pass.

Pilot ROAS đẹp nhưng COD fail cao bị bỏ qua.

Pilot không có owner review.

## 2.23.7. Evidence Requirement

Cần evidence:

Pilot approval evidence.

Pilot period evidence.

Event tracking evidence.

Pixel/CAPI dedup evidence.

Gateway signal evidence.

Verified revenue evidence.

Data quality evidence.

Suppression evidence.

Dashboard evidence.

Pilot review evidence.

## 2.23.8. Smoke Requirement

Smoke tối thiểu:

Pilot chưa đủ evidence -> Pilot WAITING.

Pilot data quality fail -> Pilot Fail.

Pilot suppression active -> Pilot bị chặn.

Pilot ROAS tốt nhưng refund/COD fail xấu -> không auto pass.

Pilot pass mới chuyển Scale Gate review.

## 2.24. Module 21 - Scale Gate Module

## 2.24.1. Vai trò

Scale Gate Module quyết định campaign/adset/ad/creative có đủ điều kiện scale hay không.

Scale Gate không chỉ dựa vào ROAS.

Scale Gate dựa vào:

Verified Revenue.

Data quality.

Dedup.

Attribution.

Suppression.

Claim/creative approval.

SKU sellable.

Pilot evidence.

Owner approval.

## 2.24.2. Scope In

Module được phép đánh giá:

Campaign scale readiness.

Adset scale readiness.

Ad scale readiness.

Creative scale readiness.

SKU scale readiness.

Budget increase readiness.

Audience expansion readiness.

Live campaign expansion readiness.

Messenger campaign expansion readiness.

Landing page campaign expansion readiness.

## 2.24.3. Scope Out

Module không được:

Tự tăng ngân sách.

Tự mở campaign mới.

Tự nhân bản creative.

Tự bỏ qua owner approval.

Tự scale khi suppression active.

Tự scale khi data quality fail.

Tự scale khi attribution conflict WAITING.

Tự scale khi verified revenue thiếu.

Tự scale chỉ vì comment/inbox cao.

Tự scale chỉ vì ROAS đẹp nhưng evidence thiếu.

## 2.24.4. Upstream Dependency

Phụ thuộc:

Pilot Control.

Data Quality Guard.

Suppression Resolver.

ROAS/CPA/AOV Calculator.

Attribution Resolver.

Claim/Creative Approval.

Commerce Verified Revenue.

Owner Approval.

## 2.24.5. Downstream Consumer

Cung cấp quyết định cho:

Marketing team.

Ads operator.

Owner.

Finance review.

Release Handoff.

Future planning.

## 2.24.6. P0 điểm chặn

bị chặn nếu:

No Verified Revenue.

Data Quality Fail.

Dedup Fail.

Attribution Conflict WAITING.

Suppression Active.

SKU Not Sellable.

Claim Not Approved.

Creative Not Approved.

Pilot Not Accepted.

Owner Not Approved.

Runtime Unavailable.

Evidence Missing.

## 2.24.7. Evidence Requirement

Cần evidence:

Scale gate checklist.

Pilot accepted evidence.

Verified revenue evidence.

Data quality pass evidence.

Dedup pass evidence.

Attribution pass evidence.

Suppression clear evidence.

Creative/claim approval evidence.

Owner approval evidence.

## 2.24.8. Smoke Requirement

Smoke tối thiểu:

ROAS tốt nhưng data quality fail -> Scale bị chặn.

ROAS tốt nhưng Sale Lock active -> Scale bị chặn.

ROAS tốt nhưng creative chưa approved -> Scale bị chặn.

ROAS tốt nhưng attribution conflict -> Scale WAITING/bị chặn.

ROAS tốt và đủ evidence + owner approval -> Scale Ready.

## 2.25. Module 22 - Owner Approval Module

## 2.25.1. Vai trò

Owner Approval Module khóa nguyên tắc: không có owner approval thì không scale.

Owner approval là điểm kiểm soát cuối cho quyết định tăng ngân sách, mở rộng campaign, mở rộng creative hoặc scale sang kênh lớn hơn.

## 2.25.2. Scope In

Module quản lý:

Owner review request.

Owner approval status.

Owner rejection status.

Owner approval timestamp.

Owner approval scope.

Owner approval reason.

Owner approval condition.

Owner approval evidence.

Scale decision reference.

Re-review requirement.

## 2.25.3. Scope Out

Module không được:

Tự approve thay owner.

Tự sửa quyết định owner.

Tự mở scale khi owner chưa duyệt.

Tự bỏ qua owner rejection.

Tự mở rộng phạm vi approval.

Tự dùng approval cũ cho campaign mới nếu không đúng scope.

## 2.25.4. Upstream Dependency

Phụ thuộc:

Pilot review.

Scale Gate.

Dashboard.

Evidence Package.

Data Quality Guard.

Suppression Resolver.

Owner decision.

## 2.25.5. Downstream Consumer

Cung cấp trạng thái cho:

Scale Gate.

Campaign Registry.

Dashboard.

Release Handoff.

Audit/Evidence trail.

## 2.25.6. P0 điểm chặn

bị chặn nếu:

Scale không có owner approval.

Owner reject nhưng vẫn scale.

Approval không đúng campaign scope.

Approval không đúng creative scope.

Approval không đúng budget/scope.

Approval không có evidence.

Approval hết hiệu lực nhưng vẫn dùng.

Approval bị sửa không có audit/evidence.

## 2.25.7. Evidence Requirement

Cần evidence:

Owner review evidence.

Owner approval/rejection evidence.

Approval scope evidence.

Approval timestamp evidence.

Approval condition evidence.

Re-review evidence nếu có.

## 2.25.8. Smoke Requirement

Smoke tối thiểu:

Scale gate pass kỹ thuật nhưng owner chưa duyệt -> không scale.

Owner reject -> Scale bị chặn.

Owner approve đúng scope -> Scale Approved trong scope đó.

Approval không đúng creative -> creative không scale.

Approval không đúng budget range -> không tăng vượt phạm vi.

## 2.26. Module 23 - Evidence Package Module

## 2.26.1. Vai trò

Evidence Package Module gom và chuẩn hóa bằng chứng cho Ads Measurement.

Không có evidence thì không Completion Decision.

Không có evidence thì không Release Ready.

Không có evidence thì không Scale Approved.

## 2.26.2. Scope In

Module quản lý evidence cho:

Campaign approval.

Creative approval.

Claim approval.

Pixel/CAPI tracking.

Event dedup.

Gateway signal.

Commerce Verified Revenue.

Payment/COD status.

Cancel/refund/exclusion.

Attribution.

Data quality.

Suppression.

Dashboard reconciliation.

Pilot review.

Scale gate.

Owner approval.

Release handoff.

## 2.26.3. Scope Out

Module không được:

Tự tạo evidence giả.

Tự sửa evidence.

Tự đánh accepted nếu chưa review.

Tự dùng screenshot không đủ nguồn làm verified evidence.

Tự thay Commerce data.

Tự thay owner approval.

Tự bỏ evidence thiếu.

Tự gọi pass khi evidence WAITING.

## 2.26.4. Upstream Dependency

Phụ thuộc:

Tất cả module TECH-07.

Commerce evidence.

Gateway evidence.

Operational evidence.

Owner review.

Data quality review.

Dashboard reconciliation.

## 2.26.5. Downstream Consumer

Cung cấp dữ liệu cho:

Done Gate.

Fail Gate.

Scale Gate.

Release Handoff.

Owner audit.

Future review.

## 2.26.6. P0 điểm chặn

bị chặn nếu:

Evidence missing.

Evidence WAITING nhưng vẫn pass.

Evidence rejected nhưng vẫn pass.

Evidence không map đúng campaign.

Evidence không map đúng revenue.

Evidence không map đúng creative.

Evidence không map đúng scale decision.

Evidence bị sửa không audit.

Evidence không đủ để owner review.

## 2.26.7. Evidence Requirement

Chính module này phải có:

Evidence index.

Evidence owner.

Evidence status.

Evidence scope.

Evidence timestamp.

Evidence review result.

Evidence rejection reason nếu fail.

Evidence handoff status.

## 2.26.8. Smoke Requirement

Smoke tối thiểu:

Missing evidence -> Completion WAITING.

Rejected evidence -> Completion Fail.

WAITING evidence -> không Release Ready.

Evidence accepted -> được đưa vào Done Gate.

Evidence không đúng scope -> không dùng cho scale.

## 2.27. Module 24 - Release Handoff Module

## 2.27.1. Vai trò

Release Handoff Module xác định khi nào TECH-07 được bàn giao sang TECH-08 MC AI Live hoặc sang giai đoạn triển khai tiếp theo.

Module này không gọi Documentation Complete là Production Ready.

## 2.27.2. Scope In

Module quản lý:

Documentation Done status.

Evidence Accepted status.

Smoke Pass status.

Pilot Accepted status.

Scale Gate status.

Owner Approval status.

Release Ready status.

Release Approved status.

Go-live Approved status.

Downstream handoff note cho TECH-08.

## 2.27.3. Scope Out

Module không được:

Tự gọi Production Ready khi chỉ xong tài liệu.

Tự gọi Release Ready khi chưa smoke pass.

Tự gọi Release Approved khi owner chưa sign-off.

Tự gọi Go-live Approved khi chưa có release decision.

Tự bỏ Global Gateway bị chặn.

Tự cho TECH-08 dùng dữ liệu chưa stable.

Tự cho scale khi TECH-07 evidence chưa accepted.

## 2.27.4. Upstream Dependency

Phụ thuộc:

Toàn bộ module TECH-07.

Evidence Package.

Smoke result.

Pilot result.

Scale Gate result.

Owner approval.

Release decision.

## 2.27.5. Downstream Consumer

Cung cấp dữ liệu cho:

TECH-08 MC AI Live.

Ads execution team.

Commerce reporting.

Owner review.

Global Gateway release governance.

## 2.27.6. P0 điểm chặn

bị chặn nếu:

Documentation Done bị gọi là Production Ready.

Evidence chưa accepted nhưng Completion Decision.

Smoke chưa pass nhưng Release Ready.

Owner chưa sign-off nhưng Release Approved.

Chưa có release decision nhưng Go-live Approved.

Global Gateway bị chặn bị bỏ qua.

TECH-08 nhận handoff khi TECH-07 chưa đủ điều kiện.

## 2.27.7. Evidence Requirement

Cần evidence:

Documentation completion evidence.

Evidence accepted package.

Smoke pass report.

Pilot accepted report.

Scale gate report.

Owner sign-off evidence.

Release decision evidence.

TECH-08 handoff evidence.

## 2.27.8. Smoke Requirement

Smoke tối thiểu:

Documentation Done nhưng evidence thiếu -> không Release Ready.

Evidence accepted nhưng smoke fail -> không Release Ready.

Smoke pass nhưng owner chưa sign-off -> không Release Approved.

Owner sign-off nhưng chưa release decision -> không Go-live Approved.

Release handoff chỉ pass khi đủ toàn bộ gate.

## 2.28. Cross-Module Dependency Map

## 2.28.1. Dependency tuyến event

Tuyến event đi theo thứ tự:

Pixel/CAPI Event Module.

Facebook Gateway Event Signal Module.

Deduplication / Idempotency Guard.

Data Quality Guard.

Funnel Metrics Module.

Attribution Resolver.

Dashboard Boundary.

Nguyên tắc:

Event chưa verified chỉ là signal.

## 2.28.2. Dependency tuyến revenue

Tuyến revenue đi theo thứ tự:

Commerce Verified Revenue Mapper.

COD/Cancel/Refund Exclusion Module.

Deduplication / Idempotency Guard.

Attribution Resolver.

ROAS/CPA/AOV Calculator.

Dashboard Boundary.

Pilot Control.

Scale Gate.

Nguyên tắc:

Không có Verified Revenue thì không có Ads Revenue.

## 2.28.3. Dependency tuyến suppression

Tuyến suppression đi theo thứ tự:

Operational suppression.

Commerce suppression.

AI Advisor suppression.

Facebook Gateway suppression.

Claim/Creative suppression.

Data Quality suppression.

Suppression Resolver.

Dashboard Boundary.

Scale Gate.

Nguyên tắc:

Suppression thắng ROAS.

## 2.28.4. Dependency tuyến scale

Tuyến scale đi theo thứ tự:

Campaign Registry.

Creative Registry.

Claim/Creative Approval.

Pixel/CAPI tracking.

Gateway signal.

Verified Revenue.

Data Quality.

Dedup.

Attribution.

Suppression clear.

Pilot accepted.

Scale Gate pass.

Owner approval.

Release decision.

Nguyên tắc:

Không owner approval thì không scale.

## 2.29. Cross-Module P0 điểm chặn Registry

## 2.29.1. Revenue điểm chặn

TECH-07 bị chặn nếu:

Dùng quote làm revenue.

Dùng cart làm revenue.

Dùng order draft làm revenue.

Dùng unpaid order làm revenue.

Dùng payment WAITING làm revenue.

Dùng COD WAITING làm revenue.

Dùng COD fail làm revenue.

Dùng cancelled/refunded/invalid order trái policy.

Dùng Gateway event làm revenue.

Dùng AI conversation làm revenue.

## 2.29.2. Attribution điểm chặn

TECH-07 bị chặn nếu:

Attribution không có event trail.

Attribution thiếu campaign/ad/creative identity.

Attribution conflict không được đánh dấu.

Ads/CRM/Diamond double count.

No bind nhưng vẫn Diamond attribution.

Revenue conflict vẫn đưa vào scale.

## 2.29.3. Data Quality điểm chặn

TECH-07 bị chặn nếu:

Event thiếu identity.

Purchase event thiếu dedup.

Pixel/CAPI double count.

Revenue lệch Commerce.

Dashboard không reconcile.

Evidence thiếu.

Runtime unavailable nhưng vẫn pass.

## 2.29.4. Suppression điểm chặn

TECH-07 bị chặn nếu:

Sale Lock active nhưng scale.

Recall active nhưng scale.

SKU not sellable nhưng scale.

Claim chưa approved nhưng scale.

Creative chưa approved nhưng scale.

Gateway suppression active nhưng scale.

Data quality fail nhưng scale.

## 2.29.5. Scale điểm chặn

TECH-07 bị chặn nếu:

Pilot chưa accepted.

Scale Gate chưa pass.

Owner chưa approval.

Evidence chưa accepted.

Smoke chưa pass.

Release decision chưa có.

Global Gateway vẫn bị chặn nhưng bị bỏ qua.

## 2.30. Cross-Module Evidence Matrix

## 2.30.1. Evidence nhóm Campaign / Creative

Cần có:

Campaign approval evidence.

SKU scope evidence.

Adset/ad/creative identity evidence.

Creative version evidence.

Claim approval evidence.

Creative approval evidence.

Destination mapping evidence.

## 2.30.2. Evidence nhóm Event / Signal

Cần có:

Pixel event evidence.

CAPI event evidence.

Pixel/CAPI dedup evidence.

Gateway signal evidence.

Live session evidence.

Messenger handoff evidence.

Landing session evidence.

## 2.30.3. Evidence nhóm Commerce / Revenue

Cần có:

Official order evidence.

Payment confirmation evidence.

COD success/fail evidence.

Order verification evidence.

Verified revenue evidence.

Cancel/refund/invalid evidence.

Exclusion evidence.

## 2.30.4. Evidence nhóm Attribution / Conflict

Cần có:

Attribution trail evidence.

Campaign/ad/creative mapping evidence.

Ads/CRM conflict evidence.

Ads/Diamond conflict evidence.

Referral bind evidence.

Conflict resolution evidence.

Unattributed revenue evidence.

## 2.30.5. Evidence nhóm Dashboard / Scale

Cần có:

Dashboard reconciliation evidence.

Data quality report.

Suppression report.

Pilot report.

Scale gate report.

Owner approval evidence.

Release handoff evidence.

## 2.31. Cross-Module Smoke Matrix cho PHẦN 2/4

## 2.31.1. ADS-MOD-SMK-001 - Pixel/CAPI không tạo revenue giả

Điều kiện:

Pixel/CAPI có page view, lead, add to cart, initiate checkout.

Chưa có Verified Revenue.

Kỳ vọng:

Chỉ ghi nhận funnel signal.

Revenue = 0.

ROAS không tính từ các signal này.

## 2.31.2. ADS-MOD-SMK-002 - Gateway signal không tạo revenue

Điều kiện:

Có comment hỏi giá.

Handoff Messenger thành công.

AI tư vấn.

Chưa có official order verified.

Kỳ vọng:

Comment/inbox/consultation chỉ là signal.

Không ghi revenue.

Không tính ROAS.

## 2.31.3. ADS-MOD-SMK-003 - Commerce Verified Revenue mới vào revenue

Điều kiện:

Có official order.

Payment/COD success.

Commerce xác nhận Verified Revenue.

Kỳ vọng:

Revenue được đưa vào Ads Measurement.

Attribution Resolver xử lý nguồn.

Dashboard hiển thị Verified Revenue.

## 2.31.4. ADS-MOD-SMK-004 - Refund làm revenue bị điều chỉnh

Điều kiện:

Order đã từng verified.

Sau đó phát sinh refund theo policy.

Kỳ vọng:

Revenue được điều chỉnh hoặc exclude theo policy.

Dashboard cập nhật.

ROAS cập nhật.

Evidence lưu đủ.

## 2.31.5. ADS-MOD-SMK-005 - Ads + Diamond conflict không double count

Điều kiện:

Order có Ads touchpoint.

Order có Diamond referral bind.

Chưa có priority/allocation rule.

Kỳ vọng:

Revenue vào Attribution Conflict WAITING.

Không tính đủ cho cả Ads và Diamond.

Không dùng revenue này để scale.

## 2.31.6. ADS-MOD-SMK-006 - CRM + Ads conflict không double count

Điều kiện:

Order có CRM touchpoint.

Order có Ads touchpoint.

Cả hai cùng nằm trong attribution window.

Kỳ vọng:

Conflict được đánh dấu.

Không double count.

Chỉ phân bổ theo rule approved.

## 2.31.7. ADS-MOD-SMK-007 - Suppression thắng ROAS

Điều kiện:

Campaign có ROAS tốt.

SKU bị Sale Lock hoặc Recall.

Kỳ vọng:

Dashboard hiển thị suppression.

Không tăng ngân sách.

## 2.31.8. ADS-MOD-SMK-008 - Data Quality Fail chặn scale

Điều kiện:

Campaign có Verified Revenue.

Nhưng event identity thiếu hoặc revenue lệch Commerce.

Kỳ vọng:

Dashboard không hiển thị scale-ready.

## 2.31.9. ADS-MOD-SMK-009 - Pilot chưa accepted không scale

Điều kiện:

Campaign mới chạy 3 ngày.

ROAS tạm thời tốt.

Chưa đủ pilot 7-14 ngày hoặc chưa owner-approved pilot window.

Kỳ vọng:

Không scale.

## 2.31.10. ADS-MOD-SMK-010 - Owner chưa approval không scale

Điều kiện:

Pilot pass.

Data quality pass.

ROAS tốt.

Owner chưa duyệt scale.

Kỳ vọng:

Scale Gate không được Approved.

Dashboard hiển thị Owner Approval WAITING.

Không tăng ngân sách.

## 2.32. Done Gate của PHẦN 2/4

## PHẦN 2/4 chỉ được xem là Documentation Done khi đạt đủ:

Đã khóa Ads Measurement Runtime Orchestrator.

Đã khóa Pixel/CAPI Event Module.

Đã khóa Facebook Gateway Event Signal Module.

Đã khóa Campaign Registry Module.

Đã khóa Adset/Ad/Creative Registry Module.

Đã khóa Claim/Creative Approval Boundary.

Đã khóa Commerce Verified Revenue Mapper.

Đã khóa Quote/Cart/Order Draft Exclusion.

Đã khóa Funnel Metrics Module.

Đã khóa ROAS/CPA/AOV/Boxes Calculator.

Đã khóa COD/Cancel/Refund Exclusion.

Đã khóa Attribution Resolver.

Đã khóa Live/Messenger/Landing Attribution.

Đã khóa Diamond Referral Conflict Resolver.

Đã khóa CRM Revenue Boundary.

Đã khóa Suppression Resolver.

Đã khóa Data Quality Guard.

Đã khóa Deduplication/Idempotency Guard.

Đã khóa Dashboard Boundary.

Đã khóa Pilot Control.

Đã khóa Scale Gate.

Đã khóa Owner Approval.

Đã khóa Evidence Package.

Đã khóa Release Handoff.

Mỗi module có Scope In / Scope Out.

Mỗi module có Upstream / Downstream.

Mỗi module có P0 điểm chặn.

Mỗi module có Evidence Requirement.

Mỗi module có Smoke Requirement.

Không module nào tạo revenue trái Commerce.

Không module nào scale khi thiếu evidence.

Không module nào override Sale Lock / Recall / Suppression.

Không viết code.

Không thiết kế API/DB/UI chi tiết.

Không gọi Documentation Done là Production Ready.

## 2.33. Fail Gate của PHẦN 2/4

## PHẦN 2/4 bị Fail nếu:

Có module cho phép Ads tự tạo revenue.

Có module dùng quote/cart/order draft/unpaid order làm revenue.

Có module dùng Gateway event làm revenue.

Có module dùng AI conversation làm revenue.

Có module cho phép Pixel/CAPI gửi Purchase khi chưa có Verified Revenue.

Có module cho phép double count revenue.

Có module cho phép Ads/CRM/Diamond cộng trùng doanh thu.

Có module cho phép scale khi Sale Lock/Recall active.

Có module cho phép scale khi SKU không sellable.

Có module cho phép scale khi claim/creative chưa approved.

Có module cho phép scale khi data quality fail.

Có module cho phép scale khi pilot chưa accepted.

Có module cho phép scale khi owner chưa approval.

Có module gọi dashboard là source-of-truth.

Có module gọi Documentation Done là Production Ready.

Có module đi vào code/API/DB/UI chi tiết ngoài phạm vi yêu cầu.

## 2.34. Release Handoff của PHẦN 2/4

## PHẦN 2/4 bàn giao cho PHẦN 3/4 các contract đã khóa:

Event phải qua classification.

Event phải qua dedup/idempotency.

Event chưa verified chỉ là signal.

Quote/cart/order draft/unpaid order luôn bị exclude khỏi revenue.

Verified Revenue chỉ đến từ Commerce.

Revenue phải qua exclusion logic.

Revenue phải qua attribution.

Attribution conflict phải được hold.

Ads/CRM/Diamond không được double count.

Dashboard không phải source-of-truth.

Suppression thắng ROAS.

Pilot 7-14 ngày là điều kiện trước scale.

Scale Gate cần data quality, evidence, suppression clear, owner approval.

Runtime unavailable thì fail-safe, không scale.

TECH-07 chưa Production Ready chỉ vì hoàn thành tài liệu.

Trạng thái sau PHẦN 2/4:

## TECH-07 PHẦN 2/4 = MODULE CONTRACT DOCUMENTATION READY

Chưa được gọi là:

Production Ready.

Release Ready.

Go-live Approved.

Scale Approved.

## 2.35. Kết luận PHẦN 2/4

## PHẦN 2/4 đã khóa toàn bộ module contract của TECH-07.

Tư duy bắt buộc:

Ads Measurement không phải nơi “tự tính doanh thu”.

Ads Measurement là nơi đo lường có kiểm soát, đối soát có evidence và chỉ scale khi dữ liệu thật sự đáng tin cậy.

Các module phải phối hợp theo đúng ranh giới:

Pixel/CAPI ghi nhận event.

Gateway cung cấp signal kênh.

Commerce cung cấp Verified Revenue.

Data Quality kiểm tra độ tin cậy.

Dedup chống trùng.

Attribution phân bổ doanh thu verified.

Suppression chặn scale khi có rủi ro.

Dashboard hiển thị nhưng không thay source-of-truth.

Pilot kiểm tra thực chiến.

Scale Gate chỉ mở khi đủ evidence và owner approval.

## PHẦN 2/4 kết thúc tại đây.

Phần tiếp theo sẽ viết:

## PHẦN 3/4 - Ads Lifecycle / State Machine / Event-to-Revenue Boundary / Attribution / Scale Control

## PHẦN 3/4 - ADS LIFECYCLE / STATE MACHINE / EVENT-TO-REVENUE BOUNDARY / ATTRIBUTION / SCALE CONTROL

## 3.1. Mục tiêu của PHẦN 3/4

## PHẦN 3/4 khóa vòng đời vận hành của Ads Measurement.

Mục tiêu là xác định rõ:

Campaign đi qua những trạng thái nào trước khi được scale.

Event quảng cáo đi qua những trạng thái nào trước khi được dùng cho funnel.

Funnel signal đi qua những trạng thái nào trước khi được phân tích.

Quote / Cart / Order Draft / Unpaid Order bị chặn khỏi revenue như thế nào.

Payment / COD / Cancel / Refund ảnh hưởng đến Verified Revenue như thế nào.

Verified Revenue từ Commerce đi vào Ads Measurement như thế nào.

Attribution xử lý Ads / CRM / Diamond / Live / Messenger / Landing conflict như thế nào.

Dashboard chỉ hiển thị số liệu theo trạng thái nào.

Pilot 7-14 ngày được đánh giá như thế nào.

Scale Gate được mở hoặc bị chặn theo điều kiện nào.

Runtime unavailable thì fail-safe ra sao.

Không có evidence thì không Completion Decision.

Không có smoke pass thì không Release Ready.

Không có owner approval thì không Scale Approved.

## PHẦN 3/4 không viết code.

## PHẦN 3/4 không thiết kế API chi tiết.

## PHẦN 3/4 không thiết kế DB chi tiết.

## PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa lifecycle, state machine và command-query boundary ở mức tài liệu kỹ thuật triển khai.

## 3.2. Nguyên tắc lifecycle chung của TECH-07

TECH-07 vận hành theo nguyên tắc:

Event trước - Signal sau - Commerce Verification sau - Attribution sau - Dashboard sau - Pilot Review sau - Scale Gate sau - Owner Approval cuối.

Không được đảo thứ tự.

Cấm các kiểu xử lý sau:

Có comment là tính revenue.

Có inbox là tính revenue.

Có AI tư vấn là tính revenue.

Có quote là tính revenue.

Có cart là tính revenue.

Có order draft là tính revenue.

Có khách nói “chốt” là tính revenue.

Có ảnh chuyển khoản là tính PAID.

Có COD WAITING là tính revenue.

Có ROAS tạm đẹp là scale.

Có dashboard đẹp là owner approval.

Có pilot đang chạy là scale.

Có sale signal là purchase event.

Có event nhưng chưa dedup mà đưa vào báo cáo.

Có conflict attribution nhưng vẫn tính đủ cho nhiều nguồn.

## 3.3. Master Ads Lifecycle Overview

Vòng đời tổng thể của TECH-07 gồm 12 bước:

Campaign được đăng ký.

Campaign / adset / ad / creative được map với SKU, claim, channel và owner.

Campaign được duyệt cho pilot nếu đủ điều kiện.

Pixel / CAPI / Gateway / Landing / Messenger / Live bắt đầu ghi nhận event.

Event được normalize và phân loại.

Event được kiểm tra dedup/idempotency.

Event được phân loại thành funnel signal.

Commerce trả quote/cart/order/payment/verified revenue theo trạng thái riêng.

Ads Measurement chỉ nhận Verified Revenue sau khi Commerce xác nhận.

Attribution Resolver phân bổ Verified Revenue theo rule.

Dashboard hiển thị funnel, revenue, attribution, suppression, data quality và scale status.

Pilot review -> Scale Gate -> Owner Approval -> Release/Scale decision.

Không bước nào được bỏ qua.

## 3.4. Campaign State Model

## 3.4.1. Mục tiêu

Campaign State Model khóa trạng thái của một campaign trong TECH-07.

Campaign không được scale chỉ vì đã chạy quảng cáo.

Campaign chỉ được scale khi đạt đủ:

SKU sellable.

Claim approved.

Creative approved.

Pixel/CAPI/Gateway tracking ổn định.

Verified Revenue mapping pass.

Data quality pass.

Dedup/idempotency pass.

Attribution conflict resolved hoặc excluded.

Suppression clear.

Pilot accepted.

Owner approval completed.

## 3.4.2. Danh sách trạng thái campaign

Campaign có các trạng thái chuẩn:

## DRAFT

## REGISTERED

## WAITING_APPROVAL

## APPROVED_FOR_PILOT

## PILOT_RUNNING

## PILOT_HOLD

## PILOT_FAILED

## PILOT_ACCEPTED

## SCALE_REVIEW

## SCALE_BLOCKED

## SCALE_APPROVED

## SCALING_RUNNING

## SUPPRESSED

## PAUSED

## ENDED

## ARCHIVED

## 3.4.3. Ý nghĩa từng trạng thái

## 3.4.3.1. DRAFT

Campaign mới được chuẩn bị.

Chưa được chạy.

Chưa được đo.

Chưa được pilot.

Chưa được scale.

## 3.4.3.2. REGISTERED

Campaign đã có identity trong Campaign Registry.

Đã có owner.

Đã có SKU scope.

Đã có channel scope.

Nhưng chưa đủ điều kiện chạy pilot.

## 3.4.3.3. WAITING_APPROVAL

Campaign đang chờ kiểm tra:

SKU sellable.

Claim approved.

Creative approved.

Tracking plan.

Gateway readiness.

Commerce readiness.

Evidence package.

## 3.4.3.4. APPROVED_FOR_PILOT

Campaign được phép pilot.

Trạng thái này không đồng nghĩa được scale.

## 3.4.3.5. PILOT_RUNNING

Campaign đang chạy pilot trong thời lượng đã được owner-approved.

Mọi số liệu trong trạng thái này là dữ liệu pilot, không phải scale decision cuối.

## 3.4.3.6. PILOT_HOLD

Pilot bị giữ lại do:

Data quality issue.

Dedup issue.

Attribution conflict.

Suppression active.

Commerce revenue mapping issue.

Gateway signal issue.

Claim/creative review issue.

## 3.4.3.7. PILOT_FAILED

Pilot thất bại.

Không được scale.

Cần review nguyên nhân.

## 3.4.3.8. PILOT_ACCEPTED

Pilot đã được review và accepted.

Chưa được scale nếu Scale Gate và Owner Approval chưa pass.

## 3.4.3.9. SCALE_REVIEW

Campaign đang được xét scale.

Phải kiểm tra toàn bộ:

## ROAS.

## CPA.

## AOV.

Boxes per Order.

Verified Revenue.

COD Fail.

Refund.

Cancel.

Data quality.

Suppression.

Attribution conflict.

Owner approval.

## 3.4.3.10. SCALE_BLOCKED

Campaign bị chặn scale.

Lý do có thể là:

No Verified Revenue.

Data Quality Fail.

Dedup Fail.

Suppression Active.

SKU Not Sellable.

Claim Not Approved.

Creative Not Approved.

Attribution Conflict WAITING.

Pilot Not Accepted.

Owner Not Approved.

Runtime Unavailable.

Evidence Missing.

## 3.4.3.11. SCALE_APPROVED

Campaign được owner approve cho scale trong phạm vi cụ thể.

Phạm vi có thể gồm:

Campaign.

Adset.

Ad.

Creative.

## SKU.

Channel.

Budget range.

Time window.

Không được hiểu Scale Approved là được scale vô hạn.

## 3.4.3.12. SCALING_RUNNING

Campaign đang scale trong phạm vi được duyệt.

Nếu phát sinh suppression, refund/cancel/COD fail bất thường, data quality fail hoặc claim issue, campaign phải chuyển về SUPPRESSED, PAUSED hoặc SCALE_BLOCKED.

## 3.4.3.13. SUPPRESSED

Campaign bị chặn do operational/commercial/channel/claim/data issue.

Suppression thắng mọi ROAS.

## 3.4.3.14. PAUSED

Campaign tạm dừng.

Có thể do owner quyết định, ngân sách, vận hành, suppression hoặc review.

## 3.4.3.15. ENDED

Campaign kết thúc.

Không chạy mới.

Dữ liệu được giữ để reporting và audit.

## 3.4.3.16. ARCHIVED

Campaign được lưu trữ.

Không dùng để scale.

Chỉ dùng tham chiếu lịch sử.

## 3.4.4. Campaign transition rule

Chỉ cho phép transition theo chuỗi an toàn:

DRAFT -> REGISTERED -> WAITING_APPROVAL -> APPROVED_FOR_PILOT -> PILOT_RUNNING -> PILOT_ACCEPTED -> SCALE_REVIEW -> SCALE_APPROVED -> SCALING_RUNNING

Các nhánh chặn:

## PILOT_RUNNING -> PILOT_HOLD

## PILOT_RUNNING -> PILOT_FAILED

## SCALE_REVIEW -> SCALE_BLOCKED

## SCALING_RUNNING -> SUPPRESSED

## SCALING_RUNNING -> PAUSED

SUPPRESSED -> SCALE_REVIEW chỉ khi suppression cleared có evidence.

PAUSED -> SCALE_REVIEW nếu cần xét lại trước khi chạy.

## ENDED -> ARCHIVED

Không được nhảy trực tiếp:

## DRAFT -> SCALE_APPROVED

## REGISTERED -> SCALING_RUNNING

## PILOT_RUNNING -> SCALING_RUNNING

PILOT_ACCEPTED -> SCALING_RUNNING nếu chưa owner approval.

SUPPRESSED -> SCALING_RUNNING nếu chưa re-review.

## 3.4.5. Campaign P0 điểm chặn

Campaign bị P0 bị chặn nếu:

Campaign không có owner.

Campaign không có SKU scope.

Campaign chạy SKU không sellable.

Campaign chạy SKU bị Sale Lock / Recall.

Campaign dùng creative chưa approved.

Campaign dùng claim chưa approved.

Campaign chưa pilot đã scale.

Campaign pilot thiếu evidence.

Campaign scale thiếu owner approval.

Campaign scale khi data quality fail.

Campaign scale khi attribution conflict WAITING.

Campaign scale khi runtime unavailable.

## 3.5. Ad Event State Model

## 3.5.1. Mục tiêu

Ad Event State Model khóa cách xử lý event từ Pixel/CAPI/Ads platform.

Event không được đi thẳng thành revenue.

Event phải đi qua:

Receive.

Normalize.

Source validation.

Dedup/idempotency.

Classification.

Funnel signal acceptance.

Revenue eligibility check nếu là purchase-related event.

Commerce verified mapping nếu là purchase/revenue event.

## 3.5.2. Trạng thái ad event

Ad event có các trạng thái:

## EVENT_RECEIVED

## EVENT_NORMALIZED

## SOURCE_VALIDATED

## DEDUP_CHECKED

## DUPLICATE_REJECTED

## EVENT_CLASSIFIED

## FUNNEL_SIGNAL_ACCEPTED

## REVENUE_ELIGIBILITY_WAITING

## COMMERCE_VERIFICATION_REQUIRED

## REVENUE_EVENT_ACCEPTED

## EVENT_HOLD

## EVENT_REJECTED

## 3.5.3. Ý nghĩa trạng thái

## EVENT_RECEIVED

Event được nhận.

Chưa được dùng cho dashboard.

## EVENT_NORMALIZED

Event được chuẩn hóa để cùng format đo lường.

Chưa được tính metric nếu chưa qua validation.

## SOURCE_VALIDATED

Nguồn event được xác nhận hợp lệ.

Nguồn không hợp lệ phải hold hoặc reject.

## DEDUP_CHECKED

Event đã qua kiểm tra trùng.

Nếu trùng, chuyển DUPLICATE_REJECTED.

## EVENT_CLASSIFIED

Event được phân loại:

Awareness.

Engagement.

Consultation.

Commerce intent.

Purchase/revenue-related.

Suppression-related.

Invalid/noise.

## FUNNEL_SIGNAL_ACCEPTED

Event được tính là funnel signal.

Không phải revenue.

## REVENUE_ELIGIBILITY_WAITING

Event có dấu hiệu liên quan purchase/revenue nhưng chưa đủ Commerce verification.

Không được tính revenue.

## COMMERCE_VERIFICATION_REQUIRED

Event bắt buộc đối soát với Commerce.

Nếu Commerce chưa verified, không được tạo revenue.

## REVENUE_EVENT_ACCEPTED

Chỉ đạt trạng thái này khi:

Commerce có Verified Revenue.

Dedup pass.

Exclusion pass.

Attribution resolver có thể xử lý.

Evidence đầy đủ.

## EVENT_HOLD

Event bị giữ lại vì thiếu dữ liệu hoặc conflict.

## EVENT_REJECTED

Event bị loại.

## 3.5.4. P0 điểm chặn của ad event

Ad event bị bị chặn nếu:

Thiếu event identity.

Thiếu source.

Thiếu timestamp hợp lệ.

Thiếu dedup key cho purchase-related event.

Event duplicate nhưng vẫn tính.

Event từ test environment bị đưa vào production dashboard.

Purchase event không có Commerce Verified Revenue.

Revenue event amount lệch Commerce.

Event source không được phép.

Runtime unavailable nhưng event vẫn được pass.

## 3.6. Facebook Gateway Signal State Model

## 3.6.1. Mục tiêu

Gateway signal là tín hiệu kênh từ TECH-06.

Signal này dùng để đo phễu, không dùng để xác nhận revenue.

## 3.6.2. Trạng thái Gateway signal

Gateway signal có các trạng thái:

## GATEWAY_SIGNAL_RECEIVED

## PAGE_VALIDATED

## LIVE_SESSION_VALIDATED

## CHANNEL_CONTEXT_RESOLVED

## PUBLIC_PRIVATE_BOUNDARY_CHECKED

## HANDOFF_STATUS_RESOLVED

## MODERATION_CHECKED

## SIGNAL_ACCEPTED

## SIGNAL_HOLD

## SIGNAL_REJECTED

## SUPPRESSION_SIGNAL_RAISED

## 3.6.3. Nguyên tắc xử lý Gateway signal

Comment public là engagement signal.

Messenger handoff attempted là handoff signal.

Messenger handoff succeeded là private entry signal.

Messenger handoff failed không được tính là private success.

AI conversation là consultation signal.

Customer asked price là consultation/commerce intent signal.

Customer requested quote là quote intent signal.

Không Gateway signal nào là revenue.

Abuse/spam/moderation flag có thể tạo suppression signal.

Channel policy fail phải hold hoặc reject.

## 3.6.4. P0 điểm chặn của Gateway signal

Gateway signal bị bị chặn nếu:

Event từ page không nằm trong allowed registry.

Live session không xác định.

Public/private boundary không rõ.

Handoff failed nhưng vẫn tính handoff success.

Abuse/spam flag bị bỏ qua.

Gateway suppression bị bỏ qua.

Comment/inbox bị tính revenue.

AI conversation bị tính revenue.

Gateway runtime unavailable nhưng campaign vẫn scale.

## 3.7. Funnel Event State Model

## 3.7.1. Mục tiêu

Funnel Event State Model tách rõ các tín hiệu trong phễu bán hàng.

Funnel event không phải revenue.

## 3.7.2. Trạng thái funnel event

Funnel event có các trạng thái:

## AWARENESS_SIGNAL

## ENGAGEMENT_SIGNAL

## CHANNEL_ENTRY_SIGNAL

## CONSULTATION_SIGNAL

## QUOTE_INTENT_SIGNAL

## QUOTE_CREATED_SIGNAL

## CART_CREATED_SIGNAL

## ORDER_DRAFT_SIGNAL

## PAYMENT_INTENT_SIGNAL

## PAYMENT_WAITING_SIGNAL

## COD_WAITING_SIGNAL

## VERIFIED_REVENUE_WAITING

## VERIFIED_REVENUE_ACCEPTED

## EXCLUDED_FROM_REVENUE

## FUNNEL_HOLD

## 3.7.3. Revenue boundary theo funnel

Các trạng thái sau không bao giờ được tính revenue:

## AWARENESS_SIGNAL

## ENGAGEMENT_SIGNAL

## CHANNEL_ENTRY_SIGNAL

## CONSULTATION_SIGNAL

## QUOTE_INTENT_SIGNAL

## QUOTE_CREATED_SIGNAL

## CART_CREATED_SIGNAL

## ORDER_DRAFT_SIGNAL

## PAYMENT_INTENT_SIGNAL

## PAYMENT_WAITING_SIGNAL

## COD_WAITING_SIGNAL

Chỉ VERIFIED_REVENUE_ACCEPTED mới được dùng tính revenue.

## 3.7.4. P0 điểm chặn của funnel event

Funnel event bị bị chặn nếu:

Quote signal bị tính revenue.

Cart signal bị tính revenue.

Order Draft signal bị tính revenue.

Payment intent bị tính revenue.

Payment WAITING bị tính revenue.

COD WAITING bị tính revenue.

Comment/inbox bị tính revenue.

Funnel signal được đưa vào ROAS như revenue.

Funnel stage không rõ định nghĩa.

Funnel event bị double count.

## 3.8. Quote / Cart / Order Draft Lifecycle Boundary

## 3.8.1. Mục tiêu

Phần này khóa vòng đời Quote / Cart / Order Draft để tránh “doanh thu ảo”.

Quote / Cart / Order Draft là commerce intent, không phải revenue.

## 3.8.2. Quote state

Quote có các trạng thái đo lường:

## QUOTE_REQUESTED

## QUOTE_CREATED

## QUOTE_PRESENTED

## QUOTE_ACTIVE

## QUOTE_EXPIRED

## QUOTE_ACCEPTED_FOR_ORDER_DRAFT

## QUOTE_CANCELLED

## QUOTE_INVALID

Nguyên tắc:

QUOTE_CREATED không phải revenue.

QUOTE_PRESENTED không phải revenue.

QUOTE_ACTIVE không phải revenue.

QUOTE_ACCEPTED_FOR_ORDER_DRAFT vẫn chưa phải revenue.

QUOTE_EXPIRED không được dùng tạo official order.

Quote value không được dùng tính ROAS.

## 3.8.3. Cart state

Cart có các trạng thái đo lường:

## CART_CREATED

## CART_UPDATED

## CART_REVIEWED

## CART_ABANDONED

## CART_CONVERTED_TO_ORDER_DRAFT

## CART_EXPIRED

## CART_INVALID

Nguyên tắc:

Cart là ý định mua.

Cart không phải order.

Cart value không phải revenue.

Cart không tạo Purchase event.

Cart chỉ được dùng cho funnel metric.

## 3.8.4. Order Draft state

Order Draft có các trạng thái đo lường:

## ORDER_DRAFT_CREATED

## ORDER_DRAFT_PRESENTED

## ORDER_DRAFT_WAITING_CUSTOMER_CONFIRMATION

## ORDER_DRAFT_CONFIRMED_BY_CUSTOMER

## ORDER_DRAFT_EXPIRED

## ORDER_DRAFT_CANCELLED

## ORDER_DRAFT_CONVERTED_TO_OFFICIAL_ORDER

## ORDER_DRAFT_INVALID

Nguyên tắc:

Order Draft không phải official order.

Order Draft chưa có order_code chính thức.

Order Draft chưa phải revenue.

Customer confirmation chỉ là điều kiện để Commerce tạo official order.

Official order vẫn chưa phải revenue nếu chưa payment/COD/order verification.

## 3.8.5. P0 điểm chặn

Quote / Cart / Order Draft lifecycle bị bị chặn nếu:

Quote value được tính revenue.

Cart value được tính revenue.

Order Draft value được tính revenue.

Quote expired vẫn tạo order/revenue.

Cart abandoned vẫn tính revenue.

Order Draft chưa official vẫn tính revenue.

Purchase event phát từ Quote/Cart/Order Draft.

Dashboard hiển thị Quote/Cart/Order Draft là revenue.

## 3.9. Official Order / Payment / COD Lifecycle Boundary

## 3.9.1. Mục tiêu

Official Order / Payment / COD lifecycle khóa điểm chuyển từ ý định mua sang doanh thu có thể xác minh.

Không phải official order nào cũng là revenue.

Không phải payment WAITING nào cũng là revenue.

Không phải COD WAITING nào cũng là revenue.

## 3.9.2. Official Order state

Official Order có các trạng thái đo lường:

## OFFICIAL_ORDER_CREATED

## ORDER_WAITING_PAYMENT

## ORDER_WAITING_COD

## ORDER_PAYMENT_CONFIRMED

## ORDER_COD_SUCCESS

## ORDER_COD_FAIL

## ORDER_CANCELLED

## ORDER_REFUNDED_FULL

## ORDER_REFUNDED_PARTIAL

## ORDER_INVALID

## ORDER_VERIFICATION_WAITING

## ORDER_VERIFIED

## REVENUE_VERIFICATION_WAITING

## REVENUE_VERIFIED

## REVENUE_EXCLUDED

## 3.9.3. Payment state boundary

Payment có các trạng thái:

## PAYMENT_METHOD_SELECTED

## PAYMENT_WAITING

## PAYMENT_PROOF_RECEIVED

## PAYMENT_CONFIRMATION_WAITING

## PAYMENT_CONFIRMED

## PAYMENT_FAILED

## PAYMENT_REVERSED

## PAYMENT_INVALID

Nguyên tắc:

PAYMENT_METHOD_SELECTED không phải revenue.

PAYMENT_WAITING không phải revenue.

PAYMENT_PROOF_RECEIVED không phải PAID.

Ảnh chuyển khoản không đủ để PAID.

Chỉ PAYMENT_CONFIRMED mới có thể đi tiếp đến order verification.

Payment confirmed vẫn phải qua order/revenue verification theo Commerce policy.

## 3.9.4. COD state boundary

COD có các trạng thái:

## COD_SELECTED

## COD_WAITING_DELIVERY

## COD_DELIVERY_ATTEMPTED

## COD_SUCCESS

## COD_FAIL

## COD_RETURNED

## COD_CANCELLED

## COD_INVALID

Nguyên tắc:

COD_SELECTED không phải revenue.

COD_WAITING_DELIVERY không phải revenue.

COD_DELIVERY_ATTEMPTED không phải revenue.

Chỉ COD_SUCCESS mới có thể đi tiếp đến order/revenue verification.

COD_FAIL phải bị excluded khỏi revenue.

COD_RETURNED phải xử lý theo refund/cancel/exclusion policy.

## 3.9.5. P0 điểm chặn

Official Order / Payment / COD lifecycle bị bị chặn nếu:

Official order created được tính revenue ngay.

Payment WAITING được tính revenue.

Ảnh chuyển khoản được tính PAID.

COD WAITING được tính revenue.

COD fail được tính revenue.

Payment failed vẫn tính revenue.

Cancelled order vẫn tính revenue trái policy.

Refunded order vẫn tính revenue trái policy.

Invalid order vẫn tính revenue.

Order chưa verified vẫn tạo ROAS.

## 3.10. Verified Revenue State Model

## 3.10.1. Mục tiêu

Verified Revenue State Model khóa trạng thái doanh thu duy nhất được Ads Measurement sử dụng.

Ads Measurement không tự tạo Verified Revenue.

Commerce Runtime là nguồn xác nhận.

## 3.10.2. Trạng thái Verified Revenue

Verified Revenue có các trạng thái:

## REVENUE_NOT_ELIGIBLE

## REVENUE_WAITING_COMMERCE

## REVENUE_WAITING_PAYMENT_CONFIRMATION

## REVENUE_WAITING_COD_RESULT

## REVENUE_WAITING_ORDER_VERIFICATION

## REVENUE_VERIFIED_BY_COMMERCE

## REVENUE_ACCEPTED_FOR_MEASUREMENT

## REVENUE_ATTRIBUTION_WAITING

## REVENUE_ATTRIBUTED

## REVENUE_UNATTRIBUTED

## REVENUE_CONFLICT_WAITING

## REVENUE_EXCLUDED

## REVENUE_ADJUSTED

## REVENUE_REVERSED

## 3.10.3. Revenue acceptance rule

Revenue chỉ được chuyển sang REVENUE_ACCEPTED_FOR_MEASUREMENT khi đạt đủ:

Official order hợp lệ.

Payment confirmed hoặc COD success theo Commerce.

Order verified.

Revenue verified by Commerce.

Exclusion check pass.

Dedup pass.

Data quality không fail.

Evidence available.

## 3.10.4. Revenue exclusion rule

Revenue phải chuyển sang REVENUE_EXCLUDED hoặc REVENUE_ADJUSTED nếu:

COD fail.

Payment failed.

Order cancelled.

Full refund.

Partial refund theo policy.

Invalid order.

Duplicate order.

Fraud/suspicious order.

Commerce reversal.

Owner/Finance exclusion decision theo policy.

## 3.10.5. P0 điểm chặn

Verified Revenue lifecycle bị bị chặn nếu:

Revenue không đến từ Commerce.

Revenue không có order verification.

Revenue không có payment/COD success.

Revenue không có dedup.

Revenue không có exclusion check.

Revenue bị double count.

Revenue bị giữ sau refund/cancel trái policy.

Revenue không có evidence.

Ads tự sửa revenue amount.

## 3.11. Attribution State Model

## 3.11.1. Mục tiêu

Attribution State Model xác định cách phân bổ Verified Revenue về Ads / CRM / Diamond / Live / Messenger / Landing / Direct / Unattributed.

Attribution không tạo doanh thu.

Attribution chỉ phân bổ doanh thu đã verified.

## 3.11.2. Trạng thái attribution

Attribution có các trạng thái:

## ATTRIBUTION_NOT_REQUIRED

## ATTRIBUTION_WAITING

## EVENT_TRAIL_COLLECTED

## SOURCE_CANDIDATES_IDENTIFIED

## ATTRIBUTION_RULE_APPLIED

## ATTRIBUTION_RESOLVED_ADS

## ATTRIBUTION_RESOLVED_CRM

## ATTRIBUTION_RESOLVED_DIAMOND

## ATTRIBUTION_RESOLVED_DIRECT

## ATTRIBUTION_RESOLVED_ORGANIC

## ATTRIBUTION_PARTIAL

## ATTRIBUTION_UNATTRIBUTED

## ATTRIBUTION_CONFLICT_WAITING

## ATTRIBUTION_REJECTED

## ATTRIBUTION_REVIEW_REQUIRED

## 3.11.3. Attribution candidate rule

Một order verified có thể có nhiều candidate:

Ads click.

Ads impression.

Live engagement.

Messenger entry.

Landing page visit.

CRM message.

Diamond referral link.

Direct visit.

Organic visit.

Manual sales touchpoint.

Candidate không đồng nghĩa attribution final.

Final attribution chỉ được xác định sau khi rule được áp dụng.

## 3.11.4. Attribution conflict rule

Attribution phải chuyển ATTRIBUTION_CONFLICT_WAITING nếu:

Ads và Diamond cùng đủ candidate.

Ads và CRM cùng đủ candidate.

CRM và Diamond cùng đủ candidate.

Live Ads và Direct cùng claim một order.

Messenger retargeting và referral link cùng claim một order.

Thiếu priority rule.

Thiếu allocation rule.

Thiếu evidence trail.

Conflict WAITING không được dùng làm scale revenue.

## 3.11.5. P0 điểm chặn

Attribution bị bị chặn nếu:

Dùng unverified revenue để attribution.

Gán revenue cho campaign thiếu event trail.

Gán revenue cho creative thiếu identity.

Gán đủ 100% revenue cho nhiều nguồn khi chưa có allocation rule.

No bind nhưng vẫn Diamond attribution.

CRM touchpoint thiếu trigger/evidence nhưng vẫn attribution.

Attribution conflict nhưng vẫn vào ROAS.

Attribution bị sửa không evidence.

## 3.12. Diamond Referral Attribution Conflict Lifecycle

## 3.12.1. Mục tiêu

Phần này khóa lifecycle xử lý xung đột giữa Ads và Diamond referral.

Diamond không được nhận attribution nếu không có bind hợp lệ.

Ads không được tự lấy revenue của Diamond nếu rule ưu tiên chưa xử lý.

## 3.12.2. Trạng thái Diamond conflict

Diamond attribution conflict có các trạng thái:

## NO_REFERRAL_SIGNAL

## REFERRAL_SIGNAL_DETECTED

## REFERRAL_BIND_WAITING

## REFERRAL_BIND_VALID

## REFERRAL_BIND_INVALID

## ADS_TOUCHPOINT_DETECTED

## ADS_DIAMOND_CONFLICT_DETECTED

## CONFLICT_POLICY_REQUIRED

## CONFLICT_RESOLVED_TO_ADS

## CONFLICT_RESOLVED_TO_DIAMOND

## CONFLICT_ALLOCATED

## CONFLICT_REJECTED

## CONFLICT_WAITING_OWNER_REVIEW

## 3.12.3. Diamond no-bind rule

Nếu không có bind hợp lệ:

Không có Diamond attribution.

Không có Diamond commission attribution.

Không có Diamond revenue credit.

Có thể vẫn giữ referral signal để debug, nhưng không dùng để tính quyền lợi.

## 3.12.4. Ads + Diamond conflict rule

Nếu có Ads touchpoint và Diamond bind hợp lệ:

Không tự cộng đủ revenue cho cả hai.

Không tự tính commission nếu chưa eligible.

Không dùng revenue conflict để scale.

Phải áp dụng priority/allocation rule đã owner-approved.

Nếu chưa có rule, trạng thái là CONFLICT_WAITING_OWNER_REVIEW.

## 3.13. CRM vs Ads Attribution Conflict Lifecycle

## 3.13.1. Mục tiêu

Phần này khóa ranh giới CRM revenue và Ads revenue.

CRM không được làm đẹp ROAS Ads.

Ads không được làm sai KPI CRM.

## 3.13.2. Trạng thái CRM/Ads conflict

CRM/Ads conflict có các trạng thái:

## NO_CRM_TOUCHPOINT

## CRM_TOUCHPOINT_DETECTED

## CRM_TOUCHPOINT_VALIDATED

## CRM_TOUCHPOINT_INVALID

## ADS_TOUCHPOINT_DETECTED

## ADS_CRM_CONFLICT_DETECTED

## ATTRIBUTION_PRIORITY_REQUIRED

## RESOLVED_TO_ADS

## RESOLVED_TO_CRM

## ALLOCATED_BY_RULE

## UNATTRIBUTED_WAITING

## OWNER_REVIEW_REQUIRED

## 3.13.3. CRM validation rule

CRM touchpoint chỉ hợp lệ khi:

Nằm trong trigger whitelist.

Không vi phạm opt-out.

Có message log.

Có timestamp.

Có customer mapping hợp lệ.

Không phải spam/duplicate.

Có rule attribution.

Nếu thiếu các điều kiện trên, không được lấy CRM làm nguồn attribution.

## 3.13.4. P0 điểm chặn

CRM/Ads conflict bị bị chặn nếu:

CRM opt-out bị bỏ qua.

CRM message ngoài whitelist vẫn tính revenue.

CRM revenue cộng vào Ads ROAS.

Ads revenue cộng vào CRM KPI.

Conflict không được đánh dấu.

Conflict thiếu owner-approved rule nhưng vẫn resolve.

CRM touchpoint thiếu evidence.

## 3.14. Suppression Lifecycle

## 3.14.1. Mục tiêu

Suppression Lifecycle xác định cách chặn campaign/ad/creative/SKU khi có rủi ro.

Suppression thắng ROAS.

## 3.14.2. Trạng thái suppression

Suppression có các trạng thái:

## SUPPRESSION_NONE

## SUPPRESSION_CHECK_WAITING

## OPERATIONAL_SUPPRESSION_ACTIVE

## COMMERCE_SUPPRESSION_ACTIVE

## AI_SUPPRESSION_ACTIVE

## GATEWAY_SUPPRESSION_ACTIVE

## CLAIM_CREATIVE_SUPPRESSION_ACTIVE

## DATA_QUALITY_SUPPRESSION_ACTIVE

## OWNER_SUPPRESSION_ACTIVE

## SUPPRESSION_ACTIVE

## SUPPRESSION_CLEAR_WAITING_EVIDENCE

## SUPPRESSION_CLEARED

## REVIEW_REQUIRED_AFTER_SUPPRESSION

## 3.14.3. Suppression source

Suppression có thể đến từ:

SKU Not Sellable.

Sale Lock.

Recall.

Warehouse bị chặn.

Public Trace bị chặn.

Payment/COD issue.

High cancel/refund/COD fail issue.

AI claim/safety issue.

Gateway moderation/rate limit issue.

Meta permission issue.

Creative rejected.

Claim not approved.

Data quality fail.

Owner manual block.

## 3.14.4. Suppression clear rule

Suppression chỉ được clear khi:

Nguồn gây suppression đã được xử lý.

Evidence xử lý được accepted.

Dashboard phản ánh trạng thái mới.

Scale Gate re-review.

Owner approval nếu suppression ảnh hưởng scale.

Không được tự clear suppression bằng cách “tắt cảnh báo”.

## 3.15. Dashboard Lifecycle

## 3.15.1. Mục tiêu

Dashboard Lifecycle khóa trạng thái dashboard từ lúc nhận dữ liệu đến lúc được dùng cho owner review.

Dashboard không phải source-of-truth.

## 3.15.2. Trạng thái dashboard

Dashboard có các trạng thái:

## DASHBOARD_NOT_READY

## DATA_INGESTING

## DATA_QUALITY_WAITING

## DATA_QUALITY_HOLD

## DATA_QUALITY_FAILED

## RECONCILIATION_WAITING

## RECONCILED_WITH_COMMERCE

## ATTRIBUTION_WAITING

## SUPPRESSION_ACTIVE

## PILOT_VIEW_READY

## OWNER_REVIEW_READY

## SCALE_REVIEW_READY

## DASHBOARD_BLOCKED

## 3.15.3. Dashboard display rule

Dashboard phải tách rõ:

Funnel Signal.

Commerce Intent.

Payment WAITING.

## COD WAITING.

Verified Revenue.

Excluded Revenue.

Attributed Revenue.

Unattributed Revenue.

Conflict WAITING Revenue.

Suppression Active.

Data Quality Status.

Scale Gate Status.

Dashboard không được gom tất cả thành “doanh thu”.

## 3.15.4. P0 điểm chặn

Dashboard bị bị chặn nếu:

Revenue lệch Commerce.

Quote/cart/order draft bị hiển thị là revenue.

Dashboard ẩn COD fail.

Dashboard ẩn refund/cancel.

Dashboard ẩn attribution conflict.

Dashboard ẩn suppression.

Dashboard ẩn data quality fail.

Dashboard tự gọi scale-ready khi chưa Scale Gate pass.

Dashboard tự gọi approved khi owner chưa duyệt.

## 3.16. Pilot Lifecycle

## 3.16.1. Mục tiêu

Pilot Lifecycle khóa giai đoạn thử nghiệm 7-14 ngày trước khi scale.

Pilot không chỉ đo ROAS.

Pilot kiểm tra khả năng đo lường và vận hành thật.

## 3.16.2. Trạng thái pilot

Pilot có các trạng thái:

## PILOT_NOT_STARTED

## PILOT_APPROVED

## PILOT_RUNNING

## PILOT_DATA_COLLECTING

## PILOT_DATA_QUALITY_REVIEW

## PILOT_REVENUE_REVIEW

## PILOT_ATTRIBUTION_REVIEW

## PILOT_SUPPRESSION_REVIEW

## PILOT_OWNER_REVIEW

## PILOT_ACCEPTED

## PILOT_FAILED

## PILOT_EXTENDED

## PILOT_CANCELLED

## 3.16.3. Pilot acceptance rule

Pilot chỉ được accepted khi:

Đủ thời lượng 7-14 ngày hoặc đúng pilot window owner-approved.

Tracking hoạt động ổn định.

Pixel/CAPI dedup pass.

Gateway signal pass.

Commerce Verified Revenue mapping pass.

Exclusion logic pass.

Attribution logic pass.

Data quality pass.

Suppression clear.

Dashboard reconciled.

Evidence accepted.

Owner review completed.

## 3.16.4. Pilot fail rule

Pilot phải fail hoặc hold nếu:

Không có evidence.

Data quality fail.

Dedup fail.

Revenue lệch Commerce.

COD fail/cancel/refund bất thường chưa giải thích.

Attribution conflict cao chưa resolve.

Claim/creative issue.

Sale Lock/Recall/Suppression active.

Gateway tracking không ổn định.

Owner không accept.

## 3.17. Scale Gate Lifecycle

## 3.17.1. Mục tiêu

Scale Gate Lifecycle khóa điều kiện tăng ngân sách hoặc mở rộng quảng cáo.

Scale Gate không được mở chỉ vì ROAS tốt.

## 3.17.2. Trạng thái Scale Gate

Scale Gate có các trạng thái:

## SCALE_NOT_ELIGIBLE

## SCALE_REVIEW_WAITING

## SCALE_DATA_QUALITY_CHECK

## SCALE_REVENUE_CHECK

## SCALE_ATTRIBUTION_CHECK

## SCALE_SUPPRESSION_CHECK

## SCALE_CREATIVE_CLAIM_CHECK

## SCALE_OWNER_APPROVAL_WAITING

## SCALE_BLOCKED

## SCALE_APPROVED

## SCALE_RUNNING

## SCALE_PAUSED

## SCALE_REVOKED

## 3.17.3. Scale pass rule

Scale Gate chỉ pass khi:

Pilot accepted.

Verified Revenue available.

ROAS/CPA/AOV/Boxes per Order tính đúng từ verified data.

Data quality pass.

Dedup pass.

Attribution pass hoặc conflict excluded.

Suppression clear.

SKU sellable.

Claim approved.

Creative approved.

Dashboard reconciled.

Evidence accepted.

Owner approval completed.

Release decision recorded.

## 3.17.4. Scale block rule

Scale Gate phải bị chặn nếu:

No Verified Revenue.

Data quality fail.

Dedup fail.

Attribution conflict WAITING.

Suppression active.

SKU not sellable.

Claim not approved.

Creative not approved.

Pilot not accepted.

Owner not approved.

Runtime unavailable.

Evidence missing.

Dashboard unreconciled.

COD fail/cancel/refund risk unresolved.

## 3.18. Command Boundary

## 3.18.1. Mục tiêu

Command Boundary xác định TECH-07 được phép yêu cầu thay đổi gì và không được phép thay đổi gì.

TECH-07 không được command sang Commerce/Payment/Operational để sửa nghiệp vụ nguồn.

## 3.18.2. Command được phép trong TECH-07

TECH-07 được phép tạo command nội bộ ở mức đo lường:

Register campaign for measurement.

Register creative for measurement.

Mark event hold.

Mark event rejected.

Mark data quality hold.

Mark attribution conflict WAITING.

Mark revenue unattributed.

Mark campaign scale bị chặn.

Mark campaign pilot hold.

Mark suppression active trong phạm vi Ads Measurement view.

Request owner review.

Request evidence review.

Request scale review.

Request reconciliation.

## 3.18.3. Command không được phép trong TECH-07

TECH-07 không được:

Create quote.

Create cart.

Create order draft.

Create official order.

Confirm payment.

Confirm COD success.

Verify revenue.

Release batch.

Clear Sale Lock.

Clear Recall.

Approve claim.

Approve creative.

Send AI response.

Send Messenger message.

Change customer membership.

Create Diamond bind.

Pay commission.

Override Commerce order state.

Override Operational state.

Override owner decision.

## 3.19. Query Boundary

## 3.19.1. Mục tiêu

Query Boundary xác định TECH-07 được phép đọc dữ liệu gì để đo lường.

Query không được biến thành command.

Đọc dữ liệu không đồng nghĩa có quyền sửa dữ liệu.

## 3.19.2. Query được phép

TECH-07 được phép query:

Campaign status.

Adset/ad/creative identity.

Claim/creative approval status.

SKU sellable status.

Sale Lock / Recall / Suppression status.

Pixel/CAPI event status.

Gateway signal status.

Quote/cart/order draft status dưới dạng funnel signal.

Official order status.

Payment/COD status.

Order verification status.

Verified revenue status.

Refund/cancel/invalid status.

Attribution event trail.

Diamond bind status.

CRM touchpoint status.

Data quality status.

Evidence status.

Owner approval status.

## 3.19.3. Query không được dùng sai

TECH-07 không được:

Đọc quote rồi xem là revenue.

Đọc cart rồi xem là revenue.

Đọc order draft rồi xem là revenue.

Đọc payment WAITING rồi xem là paid.

Đọc COD WAITING rồi xem là paid.

Đọc AI conversation rồi xem là sale.

Đọc Gateway signal rồi xem là revenue.

Đọc CRM touchpoint rồi xem là CRM revenue nếu chưa verified.

Đọc Diamond signal rồi xem là commission nếu no bind/no verified revenue.

Đọc dashboard rồi xem là source-of-truth.

## 3.20. Event Idempotency / Dedup Lifecycle

## 3.20.1. Mục tiêu

Idempotency / Dedup Lifecycle đảm bảo retry không tạo số liệu giả.

Đặc biệt phải chống double count Purchase / Verified Revenue.

## 3.20.2. Trạng thái dedup

Dedup có các trạng thái:

## DEDUP_NOT_REQUIRED

## DEDUP_REQUIRED

## DEDUP_WAITING

## DEDUP_PASSED

## DUPLICATE_DETECTED

## DUPLICATE_REJECTED

## DEDUP_HOLD

## DEDUP_FAILED

## 3.20.3. Dedup bắt buộc cho các nhóm

Bắt buộc dedup cho:

Pixel event.

CAPI event.

Pixel/CAPI same-event.

Gateway retry.

Messenger handoff retry.

Landing reload.

Payment callback retry.

Order sync retry.

Verified Revenue sync retry.

Attribution rerun.

Dashboard recompute.

Manual import.

## 3.20.4. P0 điểm chặn

Dedup lifecycle bị bị chặn nếu:

Purchase event thiếu dedup.

Pixel/CAPI duplicate được tính hai lần.

Payment callback retry tạo revenue mới.

Order sync retry tạo verified revenue mới.

Attribution rerun nhân đôi doanh thu.

Dashboard recompute làm đổi revenue không có source change.

Duplicate bị bỏ qua.

Dedup failed nhưng scale vẫn pass.

## 3.21. Runtime Unavailable / Blocking Lifecycle

## 3.21.1. Mục tiêu

Runtime unavailable phải fail-safe.

Không được scale khi không đọc được runtime quan trọng.

## 3.21.2. Runtime blocking sources

TECH-07 phải block hoặc hold nếu không đọc được:

Commerce Verified Revenue.

Payment status.

COD result.

Order verification.

Refund/cancel status.

SKU sellable status.

Sale Lock / Recall status.

Claim approval status.

Creative approval status.

Gateway signal status.

Pixel/CAPI tracking status.

Dedup status.

Attribution status.

Data quality status.

Suppression status.

Evidence status.

Owner approval status.

## 3.21.3. Runtime unavailable state

Runtime unavailable có các trạng thái:

## RUNTIME_OK

## RUNTIME_PARTIAL

## RUNTIME_DEGRADED

## RUNTIME_UNAVAILABLE

## MEASUREMENT_HOLD

## SCALE_BLOCKED_BY_RUNTIME

## OWNER_REVIEW_REQUIRED

## 3.21.4. Fail-safe rule

Nếu runtime unavailable:

Không scale.

Không gọi ROAS là final.

Không gửi purchase event nếu Commerce unavailable.

Không dùng WAITING data thay verified data.

Không clear suppression.

Không resolve attribution bằng đoán.

Không gọi dashboard scale-ready.

Không Release Ready.

## 3.22. End-to-End Event-to-Revenue Boundary

## 3.22.1. Chuỗi hợp lệ

Một event muốn trở thành Ads Revenue hợp lệ phải đi qua chuỗi:

Event received.

Event source validated.

Event dedup passed.

Event classified.

Funnel signal recorded.

Commerce official order created.

Payment confirmed hoặc COD success.

Order verified.

Revenue verified by Commerce.

Exclusion check passed.

Revenue accepted for measurement.

Attribution resolver processed.

Data quality passed.

Dashboard reconciled.

Pilot/scale review uses verified data only.

## 3.22.2. Điểm chặn trong chuỗi

Chuỗi bị chặn nếu:

Event invalid.

Event duplicate.

Event thiếu source.

Event không map campaign.

Event chỉ là signal.

Quote/cart/order draft chưa thành official order.

Payment WAITING.

## COD WAITING.

COD fail.

Cancel/refund/invalid.

Order chưa verified.

Revenue chưa verified.

Attribution conflict.

Data quality fail.

Suppression active.

Evidence missing.

## 3.23. Cross-Lifecycle Dependency Map

## 3.23.1. Campaign phụ thuộc lifecycle nào

Campaign lifecycle phụ thuộc:

Creative lifecycle.

Claim approval lifecycle.

SKU sellable lifecycle.

Event tracking lifecycle.

Gateway signal lifecycle.

Commerce revenue lifecycle.

Attribution lifecycle.

Data quality lifecycle.

Suppression lifecycle.

Pilot lifecycle.

Scale Gate lifecycle.

Owner approval lifecycle.

## 3.23.2. Revenue phụ thuộc lifecycle nào

Revenue lifecycle phụ thuộc:

Order lifecycle.

Payment/COD lifecycle.

Refund/cancel lifecycle.

Commerce verification lifecycle.

Dedup lifecycle.

Exclusion lifecycle.

Attribution lifecycle.

Data quality lifecycle.

## 3.23.3. Scale phụ thuộc lifecycle nào

Scale lifecycle phụ thuộc:

Campaign state.

Pilot state.

Verified Revenue state.

Data Quality state.

Dedup state.

Attribution state.

Suppression state.

Claim/creative state.

Owner Approval state.

Release decision state.

## 3.24. P0 điểm chặn Registry của PHẦN 3/4

## 3.24.1. Campaign State điểm chặn

bị chặn nếu:

Campaign nhảy trạng thái trái lifecycle.

Campaign scale trước pilot.

Campaign scale trước owner approval.

Campaign suppressed nhưng vẫn chạy scale.

Campaign không có SKU scope.

Campaign không có creative/claim approval.

## 3.24.2. Event State điểm chặn

bị chặn nếu:

Event thiếu identity.

Event duplicate vẫn tính.

Purchase event không có Commerce verification.

Gateway event được tính revenue.

Pixel/CAPI event tạo revenue khi chưa verified.

Event từ nguồn không hợp lệ vẫn vào dashboard.

## 3.24.3. Revenue State điểm chặn

bị chặn nếu:

Quote/cart/order draft được tính revenue.

Payment WAITING được tính revenue.

COD WAITING được tính revenue.

COD fail được tính revenue.

Refund/cancel/invalid không excluded.

Revenue không từ Commerce.

Revenue double count.

Revenue không có evidence.

## 3.24.4. Attribution State điểm chặn

bị chặn nếu:

Attribution dùng unverified revenue.

Attribution thiếu event trail.

Ads/CRM/Diamond double count.

No bind nhưng Diamond attribution.

Conflict WAITING nhưng vẫn scale.

Attribution bị sửa không evidence.

## 3.24.5. Scale State điểm chặn

bị chặn nếu:

Pilot chưa accepted.

Data quality fail.

Dedup fail.

Suppression active.

SKU not sellable.

Claim/creative not approved.

Owner not approved.

Evidence missing.

Runtime unavailable.

Dashboard unreconciled.

## 3.25. Evidence Requirement của PHẦN 3/4

## PHẦN 3/4 cần evidence theo nhóm:

## 3.25.1. Campaign lifecycle evidence

Campaign state history.

Campaign owner evidence.

SKU scope evidence.

Pilot approval evidence.

Scale review evidence.

Scale approval evidence.

Suppression history evidence.

## 3.25.2. Event lifecycle evidence

Event source evidence.

Event identity evidence.

Event timestamp evidence.

Event classification evidence.

Dedup evidence.

Event rejected/hold evidence.

## 3.25.3. Revenue lifecycle evidence

Official order evidence.

Payment confirmation evidence.

COD success/fail evidence.

Order verification evidence.

Verified Revenue evidence.

Refund/cancel/invalid evidence.

Revenue adjustment evidence.

## 3.25.4. Attribution lifecycle evidence

Event trail evidence.

Campaign/ad/creative mapping evidence.

Diamond bind evidence.

CRM touchpoint evidence.

Conflict resolution evidence.

Unattributed revenue evidence.

## 3.25.5. Scale lifecycle evidence

Pilot report.

Data quality report.

Dedup report.

Suppression report.

Dashboard reconciliation.

Owner approval.

Release decision.

## 3.26. Smoke Requirement của PHẦN 3/4

## 3.26.1. ADS-LIFE-SMK-001 - Campaign không nhảy thẳng sang scale

Điều kiện:

Campaign mới ở REGISTERED.

Chưa pilot.

Chưa owner approval.

Kỳ vọng:

Không được chuyển SCALING_RUNNING.

## 3.26.2. ADS-LIFE-SMK-002 - Event không verified không tạo revenue

Điều kiện:

Pixel/CAPI nhận Lead/AddToCart/Checkout signal.

Chưa có Commerce Verified Revenue.

Kỳ vọng:

Event chỉ là funnel signal.

Revenue = 0.

Không tính ROAS.

## 3.26.3. ADS-LIFE-SMK-003 - Gateway handoff không phải revenue

Điều kiện:

Comment hỏi giá.

Messenger handoff succeeded.

AI tư vấn sản phẩm.

Kỳ vọng:

Đây là channel/consultation signal.

Không tạo revenue.

Không tạo Purchase event.

## 3.26.4. ADS-LIFE-SMK-004 - Quote/Cart/Order Draft bị exclude khỏi revenue

Điều kiện:

Có Quote.

Có Cart.

Có Order Draft.

Chưa có official order verified.

Kỳ vọng:

Chỉ tính funnel/commerce intent.

Revenue = 0.

ROAS không đổi.

## 3.26.5. ADS-LIFE-SMK-005 - Payment WAITING không phải PAID

Điều kiện:

Khách chọn chuyển khoản.

Có ảnh chuyển khoản.

Chưa payment confirmation.

Kỳ vọng:

Payment vẫn WAITING.

Không Verified Revenue.

Không Purchase event.

## 3.26.6. ADS-LIFE-SMK-006 - COD fail bị loại khỏi revenue

Điều kiện:

Order COD.

Kỳ vọng:

Revenue excluded.

Dashboard phản ánh COD Fail.

ROAS không tính order này.

## 3.26.7. ADS-LIFE-SMK-007 - Refund điều chỉnh revenue

Điều kiện:

Order đã verified.

Sau đó refund full hoặc partial.

Kỳ vọng:

Revenue excluded hoặc adjusted theo policy.

Dashboard cập nhật.

ROAS cập nhật.

Evidence lưu đủ.

## 3.26.8. ADS-LIFE-SMK-008 - Ads/Diamond conflict không double count

Điều kiện:

Order có Ads touchpoint.

Có Diamond bind hợp lệ.

Chưa có priority/allocation rule.

Kỳ vọng:

Attribution Conflict WAITING.

Không tính đủ cho cả Ads và Diamond.

Không dùng revenue này để scale.

## 3.26.9. ADS-LIFE-SMK-009 - Suppression thắng ROAS

Điều kiện:

Campaign có ROAS tốt.

SKU bị Sale Lock hoặc Recall.

Kỳ vọng:

Dashboard hiển thị suppression.

Không scale.

## 3.26.10. ADS-LIFE-SMK-010 - Data quality fail chặn scale

Điều kiện:

Verified Revenue có.

Event trail thiếu hoặc dashboard lệch Commerce.

Kỳ vọng:

Data Quality Fail.

Không gọi dashboard scale-ready.

## 3.26.11. ADS-LIFE-SMK-011 - Pilot chưa accepted không scale

Điều kiện:

Campaign mới chạy pilot.

Chưa đủ evidence.

ROAS tạm thời tốt.

Kỳ vọng:

Pilot WAITING.

Scale Gate WAITING/bị chặn.

Không tăng ngân sách.

## 3.26.12. ADS-LIFE-SMK-012 - Owner chưa approval không scale

Điều kiện:

Pilot pass.

Data quality pass.

ROAS tốt.

Owner chưa duyệt.

Kỳ vọng:

Scale Owner Approval WAITING.

Không Scale Approved.

Không SCALING_RUNNING.

## 3.27. Done Gate của PHẦN 3/4

## PHẦN 3/4 chỉ được xem là Documentation Done khi đạt đủ:

Đã khóa Master Ads Lifecycle.

Đã khóa Campaign State Model.

Đã khóa Ad Event State Model.

Đã khóa Gateway Signal State Model.

Đã khóa Funnel Event State Model.

Đã khóa Quote / Cart / Order Draft Lifecycle Boundary.

Đã khóa Official Order / Payment / COD Lifecycle Boundary.

Đã khóa Verified Revenue State Model.

Đã khóa Attribution State Model.

Đã khóa Diamond Referral Conflict Lifecycle.

Đã khóa CRM vs Ads Conflict Lifecycle.

Đã khóa Suppression Lifecycle.

Đã khóa Dashboard Lifecycle.

Đã khóa Pilot Lifecycle.

Đã khóa Scale Gate Lifecycle.

Đã khóa Command Boundary.

Đã khóa Query Boundary.

Đã khóa Event Idempotency / Dedup Lifecycle.

Đã khóa Runtime Unavailable / Blocking Lifecycle.

Đã khóa Event-to-Revenue Boundary.

Đã có Cross-Lifecycle Dependency Map.

Đã có P0 điểm chặn Registry.

Đã có Evidence Requirement.

Đã có Smoke Requirement.

Không viết code.

Không thiết kế API/DB/UI chi tiết.

Không gọi Documentation Done là Production Ready.

## 3.28. Fail Gate của PHẦN 3/4

## PHẦN 3/4 bị Fail nếu:

Cho phép campaign scale trước pilot.

Cho phép campaign scale trước owner approval.

Cho phép campaign scale khi suppression active.

Cho phép event chưa dedup vào revenue.

Cho phép Gateway signal là revenue.

Cho phép AI conversation là revenue.

Cho phép quote/cart/order draft là revenue.

Cho phép payment WAITING là PAID.

Cho phép ảnh chuyển khoản là PAID.

Cho phép COD WAITING là revenue.

Cho phép COD fail là revenue.

Cho phép refund/cancel/invalid vẫn giữ revenue trái policy.

Cho phép attribution dùng unverified revenue.

Cho phép Ads/CRM/Diamond double count.

Cho phép dashboard tự làm source-of-truth.

Cho phép data quality fail nhưng scale.

Cho phép runtime unavailable nhưng vẫn scale.

Gọi Documentation Done là Production Ready.

Đi vào code/API/DB/UI chi tiết ngoài phạm vi yêu cầu.

## 3.29. Release Handoff của PHẦN 3/4

## PHẦN 3/4 bàn giao cho PHẦN 4/4 các điểm đã khóa:

Mọi campaign phải có state rõ.

Mọi event phải có lifecycle rõ.

Mọi funnel signal phải tách khỏi revenue.

Quote/cart/order draft/unpaid order không phải revenue.

Payment/COD chỉ tạo điều kiện revenue khi Commerce xác nhận.

Verified Revenue chỉ đến từ Commerce.

Attribution chỉ phân bổ Verified Revenue.

Conflict attribution phải hold.

Suppression thắng ROAS.

Dashboard không phải source-of-truth.

Pilot cần evidence.

Scale Gate cần owner approval.

Runtime unavailable thì fail-safe.

Smoke PHẦN 4/4 phải kiểm tra end-to-end toàn bộ chuỗi này.

Evidence Package PHẦN 4/4 phải gom đủ campaign, event, revenue, attribution, dashboard, pilot, scale, owner approval.

Trạng thái sau PHẦN 3/4:

## TECH-07 PHẦN 3/4 = LIFECYCLE / STATE MACHINE DOCUMENTATION READY

Chưa được gọi là:

Production Ready.

Release Ready.

Go-live Approved.

Scale Approved.

## 3.30. Kết luận PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời vận hành của Ads Measurement.

Tư duy cốt lõi:

Không có chuyện event quảng cáo đi thẳng thành doanh thu.

Không có chuyện comment, inbox, AI tư vấn, quote, cart, order draft, payment WAITING hoặc COD WAITING được tính ROAS.

Không có chuyện campaign ROAS đẹp là được scale ngay.

Doanh thu chỉ được ghi nhận khi Commerce xác nhận Verified Revenue.

Verified Revenue chỉ được dùng cho Ads sau khi qua dedup, exclusion, attribution, data quality và evidence.

Scale chỉ được mở khi pilot accepted, suppression clear, dashboard reconciled, owner approval completed và release decision được ghi nhận.

## PHẦN 3/4 kết thúc tại đây.

Phần tiếp theo sẽ viết:

## PHẦN 4/4 - Final Ads Smoke Matrix / Evidence Package / Done Gate / Fail Gate / Release Handoff / TECH-07 Final Conclusion

## PHẦN 4/4 - FINAL ADS SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-07 FINAL CONCLUSION

## 4.1. Mục tiêu của PHẦN 4/4

## PHẦN 4/4 khóa bộ kiểm thử cuối và điều kiện bàn giao của TECH-07.

Mục tiêu là xác định rõ:

TECH-07 cần smoke những chuỗi nào.

Evidence nào bắt buộc phải có.

Evidence nào chưa accepted thì không được PASS.

Dashboard nào được dùng để review.

Dashboard nào chỉ được dùng để debug.

Khi nào Ads Measurement được xem là Documentation Complete.

Khi nào TECH-07 bị Fail.

Khi nào được Release Ready.

Khi nào được Release Approved.

Khi nào được Go-live Approved.

Khi nào được bàn giao sang TECH-08 MC AI Live.

Khi nào được dùng dữ liệu Ads để scale thật.

Khi nào bắt buộc giữ Global Gate bị chặn.

Nguyên tắc trọng yếu:

Hoàn thành tài liệu không đồng nghĩa hệ thống đã sẵn sàng production.

## 4.2. Final Smoke Matrix - Nguyên tắc tổng thể

Final Smoke Matrix của TECH-07 phải kiểm tra đủ 7 nhóm:

Ads Event / Pixel / CAPI.

Facebook Gateway / Live / Messenger / Landing Signal.

Commerce Verified Revenue.

Funnel Exclusion.

Attribution / Conflict / Dedup.

Dashboard / Data Quality / Suppression.

Pilot / Scale Gate / Owner Approval / Release Handoff.

Smoke không chỉ kiểm tra “có số liệu”.

Smoke phải kiểm tra:

Số liệu có đúng nguồn không.

Có bị double count không.

Có phân biệt signal và revenue không.

Có loại trừ COD fail / cancel / refund / invalid không.

Có tôn trọng Sale Lock / Recall / Suppression không.

Có tôn trọng claim / creative approval không.

Có tôn trọng owner approval không.

Có fail-safe khi runtime unavailable không.

## 4.3. Smoke Group A - Ads Event / Pixel / CAPI

## 4.3.1. ADS-FINAL-SMK-001 - Pixel page view chỉ là awareness signal

Điều kiện

Campaign đã registered.

Pixel ghi nhận page view.

Chưa có quote.

Chưa có order.

Chưa có Verified Revenue.

Kỳ vọng

Page view được ghi nhận là awareness signal.

Revenue = 0.

ROAS không được tính.

Purchase event không được phát.

Dashboard hiển thị page view ở nhóm funnel signal.

Fail nếu

Page view làm tăng revenue.

Page view làm tăng ROAS.

Page view bị hiển thị như purchase.

Dashboard không phân biệt awareness và revenue.

## 4.3.2. ADS-FINAL-SMK-002 - CAPI lead/contact không tạo revenue

Điều kiện

CAPI ghi nhận lead hoặc contact.

Khách chưa nhận quote.

Khách chưa có official order.

Commerce chưa có Verified Revenue.

Kỳ vọng

Lead/contact là engagement hoặc consultation signal.

Revenue = 0.

Không tạo purchase event.

Không tính ROAS.

Fail nếu

Lead/contact bị tính revenue.

Cost per Lead bị nhầm thành CPA doanh thu.

Dashboard gọi lead là order.

Lead/contact được dùng làm scale revenue.

## 4.3.3. ADS-FINAL-SMK-003 - Add to Cart không tạo revenue

Điều kiện

Có Add to Cart event.

Commerce xác nhận cart hợp lệ.

Chưa có official order.

Chưa có payment/COD success.

Kỳ vọng

Add to Cart là commerce intent signal.

Cart value không phải revenue.

Revenue = 0.

ROAS không tăng.

Fail nếu

Cart value được tính vào doanh thu.

Add to Cart phát Purchase.

Cart abandoned vẫn tính revenue.

Dashboard hiển thị cart value như doanh thu.

## 4.3.4. ADS-FINAL-SMK-004 - Initiate Checkout / Order Draft không tạo revenue

Điều kiện

Có checkout intent.

Có Order Draft.

Customer chưa xác nhận đủ điều kiện tạo official order.

Payment chưa confirmed.

COD chưa success.

Kỳ vọng

Initiate Checkout là commerce intent signal.

Order Draft không phải official order.

Revenue = 0.

Không phát Purchase event.

Không tính ROAS.

Fail nếu

Order Draft value được tính revenue.

Order Draft phát Purchase.

Order Draft tạo ROAS.

Dashboard gọi Order Draft là order verified.

## 4.3.5. ADS-FINAL-SMK-005 - Pixel/CAPI Purchase chỉ pass khi Commerce Verified Revenue

Điều kiện

Có official order.

Payment confirmed hoặc COD success.

Order verified.

Commerce trả Verified Revenue.

Pixel/CAPI có purchase-related event.

Kỳ vọng

Purchase event chỉ pass khi map đúng Verified Revenue.

Purchase amount khớp Commerce.

Purchase event có dedup/idempotency.

Dashboard hiển thị revenue từ Commerce Verified Revenue.

ROAS được tính bằng Verified Revenue / Ads Spend hợp lệ.

Fail nếu

Purchase phát trước Commerce verification.

Purchase amount lệch Commerce.

Purchase không có dedup key.

Purchase bị double count.

Purchase từ order WAITING được chấp nhận.

## 4.3.6. ADS-FINAL-SMK-006 - Pixel/CAPI Dedup bắt buộc

Điều kiện

Pixel và CAPI cùng ghi nhận một event.

Event có cùng identity/dedup key.

Event liên quan purchase hoặc checkout.

Kỳ vọng

Event chỉ được tính một lần.

Purchase chỉ được tính một lần.

Revenue chỉ được tính một lần.

Dedup report phải có evidence.

Fail nếu

Pixel và CAPI làm nhân đôi event.

Purchase bị tính hai lần.

Revenue bị tính hai lần.

Dashboard ROAS tăng giả do duplicate.

## 4.4. Smoke Group B - Facebook Gateway / Live / Messenger / Landing Signal

## 4.4.1. ADS-FINAL-SMK-007 - Comment hỏi giá chỉ là funnel signal

Điều kiện

Khách comment hỏi giá trên Live.

Gateway phân loại đúng intent hỏi giá.

Chưa có Messenger order flow.

Chưa có Commerce Verified Revenue.

Kỳ vọng

Comment được tính là engagement / consultation signal.

Không public giá nếu rule yêu cầu kéo Messenger.

Không tạo revenue.

Không phát Purchase event.

Không tính ROAS.

Fail nếu

Comment được tính revenue.

Comment làm tăng ROAS.

Comment public lộ dữ liệu private.

Comment được xem như order.

## 4.4.2. ADS-FINAL-SMK-008 - Messenger handoff succeeded không phải revenue

Điều kiện

Comment được kéo sang Messenger.

Handoff succeeded.

AI tư vấn trong Messenger.

Chưa có official order verified.

Kỳ vọng

Handoff succeeded là channel signal.

Messenger conversation là consultation signal.

Revenue = 0.

ROAS không tăng.

Fail nếu

Handoff được tính revenue.

Messenger start được tính purchase.

AI tư vấn được tính doanh thu.

Dashboard gọi conversation là sale.

## 4.4.3. ADS-FINAL-SMK-009 - Messenger handoff failed không được tính private success

Điều kiện

Comment có intent cần kéo Messenger.

Gateway thử handoff.

Handoff failed.

Chưa có private conversation.

Kỳ vọng

Handoff failed được ghi nhận đúng.

Không tính Inbox Rate thành công.

Không tính private conversation.

Không tính revenue.

Fail nếu

Handoff failed bị tính thành công.

Failed handoff bị đưa vào Messenger funnel success.

Failed handoff tạo quote/order signal.

Dashboard làm đẹp Inbox Rate.

## 4.4.4. ADS-FINAL-SMK-010 - Landing page visit không tạo revenue

Điều kiện

Khách click ads vào landing page.

Có landing visit.

Chưa có order verified.

Chưa có Verified Revenue.

Kỳ vọng

Landing visit là visit signal.

Không phải revenue.

Không phát Purchase.

Không tính ROAS.

Fail nếu

Landing visit được tính revenue.

Landing visit được tính purchase.

Landing visit tự gán campaign revenue.

Dashboard gọi visit là order.

## 4.4.5. ADS-FINAL-SMK-011 - Abuse / moderation signal chặn scale

Điều kiện

Live có comment abuse / spam / tố cáo / phá hoại.

Gateway gắn moderation flag.

Campaign đang chạy pilot hoặc scale review.

Kỳ vọng

Moderation signal được chuyển vào Suppression Resolver nếu đúng rule.

Không tạo revenue.

Không tự kéo Messenger nếu policy chặn.

Campaign có thể bị hold/suppressed.

Dashboard hiển thị moderation/suppression status.

Fail nếu

Abuse signal bị bỏ qua.

Comment phá live vẫn được tính lead tốt.

Campaign vẫn scale dù moderation risk chưa xử lý.

Gateway suppression không truyền sang Ads Measurement.

## 4.5. Smoke Group C - Commerce Verified Revenue

## 4.5.1. ADS-FINAL-SMK-012 - Quote không phải revenue

Điều kiện

AI/Commerce tạo QuoteSnapshot.

Quote còn hiệu lực.

Khách chưa xác nhận order.

Chưa có official order.

Kỳ vọng

Quote value chỉ là quote signal.

Revenue = 0.

Không phát Purchase.

Không tính ROAS.

Dashboard hiển thị quote ở funnel.

Fail nếu

Quote value tính revenue.

Quote value tính ROAS.

Quote tạo Purchase event.

Quote hết hạn vẫn được dùng làm revenue.

## 4.5.2. ADS-FINAL-SMK-013 - Unpaid order không phải revenue

Điều kiện

Có official order.

Order đang WAITING payment.

Chưa có payment confirmation.

Chưa có COD success.

Kỳ vọng

Official order chưa paid không phải revenue.

Payment WAITING không phải PAID.

Revenue = 0.

Không tính ROAS.

Fail nếu

Unpaid order tính revenue.

Payment WAITING tính PAID.

Ảnh chuyển khoản được tính PAID.

Dashboard hiển thị unpaid order như revenue.

## 4.5.3. ADS-FINAL-SMK-014 - Ảnh chuyển khoản không đủ để PAID

Điều kiện

Khách gửi ảnh chuyển khoản.

Payment chưa được xác nhận bởi Payment/Finance/Commerce policy.

Order chưa revenue verified.

Kỳ vọng

Ảnh chuyển khoản chỉ là payment proof signal.

Payment vẫn WAITING confirmation.

Revenue = 0.

Không phát Purchase.

Không tính ROAS.

Fail nếu

Ảnh chuyển khoản được xem là PAID.

Payment proof tự tạo Verified Revenue.

Ads dashboard tính revenue từ ảnh.

AI/Gateway/Ads tự xác nhận thanh toán.

## 4.5.4. ADS-FINAL-SMK-015 - COD WAITING không phải revenue

Điều kiện

Khách chọn COD.

Order đang giao.

COD chưa success.

Order chưa verified revenue.

Kỳ vọng

COD WAITING không phải revenue.

COD WAITING chỉ là fulfillment/payment WAITING state.

Không phát Purchase.

Không tính ROAS.

Fail nếu

COD WAITING tính revenue.

COD đang giao tính ROAS.

COD delivery attempted nhưng chưa success vẫn tính revenue.

Dashboard không tách COD WAITING.

## 4.5.5. ADS-FINAL-SMK-016 - COD fail bị excluded

Điều kiện

Order COD.

Commerce đánh dấu order/revenue excluded.

Kỳ vọng

Revenue = 0 hoặc excluded theo Commerce policy.

COD Fail Rate tăng.

ROAS không tính order này.

Dashboard phản ánh COD fail.

Evidence lưu trạng thái COD fail.

Fail nếu

COD fail vẫn tính revenue.

COD fail không hiển thị dashboard.

COD fail vẫn dùng scale.

Revenue không điều chỉnh sau COD fail.

## 4.5.6. ADS-FINAL-SMK-017 - Payment confirmed + Order verified mới vào Verified Revenue

Điều kiện

Có official order.

Payment confirmed.

Order verified.

Commerce xác nhận Verified Revenue.

Không có refund/cancel/invalid.

Kỳ vọng

Revenue được nhận vào Ads Measurement.

Revenue amount khớp Commerce.

Revenue đi qua dedup.

Revenue đi qua attribution.

ROAS được phép tính.

Fail nếu

Revenue không khớp Commerce.

Revenue thiếu evidence.

Revenue bị double count.

Revenue chưa qua attribution nhưng đã dùng scale.

Revenue chưa qua data quality mà dashboard scale-ready.

## 4.6. Smoke Group D - Cancel / Refund / Invalid / Exclusion

## 4.6.1. ADS-FINAL-SMK-018 - Cancelled order bị loại theo policy

Điều kiện

Order từng được tạo.

Order bị cancel.

Commerce đánh dấu cancel hợp lệ.

Policy yêu cầu exclude.

Kỳ vọng

Revenue bị exclude.

ROAS cập nhật.

Dashboard hiển thị cancel.

Evidence lưu cancel reason.

Fail nếu

Cancelled order vẫn tính revenue.

Cancelled order vẫn tính ROAS.

Dashboard ẩn cancel.

Scale gate không xem xét cancel.

## 4.6.2. ADS-FINAL-SMK-019 - Full refund loại revenue

Điều kiện

Order đã từng verified.

Sau đó full refund.

Commerce/Finance xác nhận refund.

Kỳ vọng

Revenue bị reversed hoặc excluded theo policy.

ROAS điều chỉnh.

Dashboard hiển thị refund.

Evidence lưu refund status.

Fail nếu

Full refund vẫn giữ revenue.

ROAS không điều chỉnh.

Dashboard không hiển thị refund.

Scale vẫn dùng revenue đã refund.

## 4.6.3. ADS-FINAL-SMK-020 - Partial refund điều chỉnh revenue

Điều kiện

Order verified.

Phát sinh partial refund.

Commerce/Finance xác nhận amount điều chỉnh.

Kỳ vọng

Revenue được adjusted theo policy.

ROAS tính trên revenue sau điều chỉnh.

Dashboard hiển thị adjusted revenue.

Evidence đầy đủ.

Fail nếu

Partial refund bị bỏ qua.

Revenue giữ nguyên trái policy.

Revenue bị trừ sai amount.

Dashboard không reconcile với Commerce.

## 4.6.4. ADS-FINAL-SMK-021 - Invalid / duplicate order không double count

Điều kiện

Có order bị đánh invalid hoặc duplicate.

Commerce xác nhận trạng thái.

Order liên quan cùng khách/session hoặc retry.

Kỳ vọng

Invalid order không vào revenue.

Duplicate order không tạo revenue mới.

Dedup report ghi nhận.

Dashboard không double count.

Fail nếu

Duplicate order tạo doanh thu hai lần.

Invalid order vẫn tính revenue.

Dashboard ROAS tăng giả.

Không có evidence xử lý duplicate.

## 4.7. Smoke Group E - Attribution / Conflict / Dedup

## 4.7.1. ADS-FINAL-SMK-022 - Verified Revenue đủ event trail được attribution

Điều kiện

Order có Verified Revenue.

Có campaign/adset/ad/creative identity.

Có event trail hợp lệ.

Dedup pass.

Không có conflict.

Kỳ vọng

Revenue được attribution về Ads hợp lệ.

ROAS được tính.

Dashboard hiển thị attributed revenue.

Evidence trail đầy đủ.

Fail nếu

Attribution thiếu evidence.

Gán nhầm campaign.

Gán nhầm creative.

Revenue không verified vẫn attribution.

## 4.7.2. ADS-FINAL-SMK-023 - Thiếu campaign/ad/creative identity thì unattributed

Điều kiện

Order có Verified Revenue.

Thiếu campaign hoặc ad/creative identity.

Không đủ event trail.

Kỳ vọng

Revenue vẫn là Verified Revenue.

Không ép phân bổ vào Ads.

Đưa vào Unattributed Verified Revenue.

Không dùng để scale campaign cụ thể.

Fail nếu

Revenue bị gán bừa cho campaign tốt nhất.

Revenue thiếu identity vẫn tính ROAS campaign.

Dashboard không hiển thị unattributed.

Scale Gate dùng revenue này để pass.

## 4.7.3. ADS-FINAL-SMK-024 - Ads + Diamond conflict không double count

Điều kiện

Order có Verified Revenue.

Có Ads touchpoint.

Có Diamond referral bind hợp lệ.

Chưa có priority/allocation rule.

Kỳ vọng

Revenue vào Attribution Conflict WAITING.

Không cộng đủ 100% cho Ads và Diamond cùng lúc.

Không tính commission nếu chưa đủ policy.

Không dùng revenue conflict để scale.

Dashboard hiển thị conflict.

Fail nếu

Ads và Diamond cùng nhận full revenue.

No rule vẫn resolve.

Commission phát sinh từ conflict WAITING.

Campaign scale bằng revenue conflict.

Điều kiện

Có referral signal.

Không có bind hợp lệ.

Có order verified.

Kỳ vọng

Không có Diamond attribution.

Không có Diamond commission.

Referral signal có thể giữ để debug.

Attribution xử lý theo rule khác nếu đủ evidence.

Fail nếu

No bind vẫn tính Diamond.

No bind vẫn phát commission.

Dashboard ghi Diamond revenue sai.

Ads/Diamond conflict giả được tạo.

## 4.7.5. ADS-FINAL-SMK-026 - CRM + Ads conflict không double count

Điều kiện

Order có Verified Revenue.

Có Ads touchpoint.

Có CRM touchpoint hợp lệ.

Cả hai cùng trong attribution window.

Chưa có hoặc có priority rule tùy scenario.

Kỳ vọng

Nếu chưa có rule: conflict WAITING.

Nếu có rule: phân bổ đúng rule.

Không double count.

Dashboard tách Ads / CRM / conflict.

Fail nếu

CRM revenue cộng vào Ads ROAS trái rule.

Ads revenue cộng vào CRM KPI trái rule.

Conflict không hiển thị.

Owner chưa approve rule nhưng vẫn resolve.

## 4.7.6. ADS-FINAL-SMK-027 - Attribution rerun không nhân đôi revenue

Điều kiện

Attribution job chạy lại.

Cùng verified revenue.

Cùng order identity.

Cùng event trail.

Kỳ vọng

Revenue không bị nhân đôi.

Attribution result có thể cập nhật nếu rule thay đổi có evidence.

Dashboard revenue total không tăng giả.

Dedup/idempotency pass.

Fail nếu

Rerun làm tăng revenue.

Rerun tạo duplicate attributed revenue.

Dashboard ROAS thay đổi không có source change.

Không có evidence attribution rerun.

## 4.8. Smoke Group F - Dashboard / Data Quality / Suppression

## 4.8.1. ADS-FINAL-SMK-028 - Dashboard không phải source-of-truth

Điều kiện

Dashboard hiển thị metrics.

Commerce Verified Revenue có số gốc.

Ads Measurement có reconciliation.

Kỳ vọng

Dashboard đọc từ source đã reconciled.

Dashboard không tự tạo revenue.

Dashboard không sửa revenue.

Dashboard hiển thị reconciliation status.

Fail nếu

Dashboard revenue lệch Commerce.

Dashboard tự nhập revenue.

Dashboard chỉnh số thủ công không evidence.

Dashboard được xem là source-of-truth thay Commerce.

## 4.8.2. ADS-FINAL-SMK-029 - Dashboard phân biệt funnel signal và revenue

Điều kiện

Có comment, inbox, quote, cart, order draft.

Có hoặc chưa có Verified Revenue.

Dashboard hiển thị tổng quan funnel.

Kỳ vọng

Funnel signal nằm riêng.

Commerce intent nằm riêng.

Verified Revenue nằm riêng.

WAITING/excluded/conflict nằm riêng.

Fail nếu

Tất cả bị gom thành doanh thu.

Quote/cart/order draft hiển thị như revenue.

Inbox/comment hiển thị như order.

Dashboard gây hiểu sai ROAS.

## 4.8.3. ADS-FINAL-SMK-030 - Data Quality Fail chặn Scale Gate

Điều kiện

Campaign có verified order.

Nhưng event trail thiếu identity.

Hoặc dashboard lệch Commerce.

Hoặc dedup fail.

Kỳ vọng

Dashboard không scale-ready.

Evidence chỉ ra lý do fail.

Fail nếu

Data quality fail nhưng vẫn scale.

Dashboard ẩn fail.

Owner review không thấy fail.

Campaign vẫn tăng ngân sách.

## 4.8.4. ADS-FINAL-SMK-031 - Sale Lock / Recall chặn campaign scale

Điều kiện

Campaign có ROAS tốt.

SKU bị Sale Lock hoặc Recall.

Operational suppression active.

Kỳ vọng

Suppression Active.

Không tăng ngân sách.

Dashboard hiển thị lý do.

ROAS không override suppression.

Fail nếu

ROAS đẹp vẫn scale.

Sale Lock bị bỏ qua.

Recall bị bỏ qua.

Dashboard không hiển thị suppression.

## 4.8.5. ADS-FINAL-SMK-032 - SKU không sellable không được scale

Điều kiện

SKU active trong Product Master.

Nhưng Sellable Gate = false.

Campaign muốn scale.

Kỳ vọng

Campaign bị block scale.

Dashboard hiển thị SKU Not Sellable.

Không nhân campaign/creative.

Không mở ngân sách.

Fail nếu

Product Active bị hiểu là Sellable.

SKU not sellable vẫn scale.

Dashboard không đọc Sellable Gate.

Ads tự override Commerce/Operational block.

## 4.8.6. ADS-FINAL-SMK-033 - Claim / Creative chưa approved không scale

Điều kiện

Creative có hiệu quả funnel tốt.

Claim hoặc creative chưa approved.

Campaign đang xin scale.

Kỳ vọng

Dashboard hiển thị approval issue.

Không scale creative.

Không dùng hiệu quả bán hàng để hợp thức hóa claim sai.

Fail nếu

Creative chưa approved vẫn scale.

Claim chưa approved vẫn scale.

Claim vượt phạm vi vẫn chạy lớn.

Owner chưa duyệt nhưng campaign scale.

## 4.8.7. ADS-FINAL-SMK-034 - Runtime unavailable thì fail-safe

Điều kiện

Một trong các runtime không đọc được:

Commerce Verified Revenue.

Payment/COD status.

Sale Lock/Recall.

Claim/Creative approval.

Gateway event.

Dedup/Data Quality.

Evidence/Owner Approval.

Kỳ vọng

Measurement Hold hoặc Scale bị chặn.

Không tính ROAS final.

Không scale.

Không tự suy đoán dữ liệu.

Dashboard hiển thị runtime issue.

Fail nếu

Runtime unavailable nhưng dashboard vẫn pass.

Runtime unavailable nhưng scale.

Thiếu Commerce nhưng vẫn tính revenue.

Thiếu Sale Lock status nhưng vẫn mở ngân sách.

## 4.9. Smoke Group G - Pilot / Scale Gate / Owner Approval / Release Handoff

## 4.9.1. ADS-FINAL-SMK-035 - Pilot 7-14 ngày chưa đủ evidence thì không pass

Điều kiện

Campaign đang pilot.

Chưa đủ thời lượng hoặc chưa đủ evidence.

ROAS tạm thời tốt.

Kỳ vọng

Scale Gate không pass.

Không owner approval scale.

Dashboard không gọi Scale Ready.

Fail nếu

Pilot 1-3 ngày đã scale nếu không có owner-approved exception.

ROAS tạm thời thay thế evidence.

Thiếu data quality vẫn pilot pass.

Pilot thiếu revenue verification vẫn scale.

## 4.9.2. ADS-FINAL-SMK-036 - Pilot ROAS tốt nhưng COD fail cao không auto pass

Điều kiện

Pilot có ROAS nhìn tốt.

COD fail hoặc cancel/refund cao.

Exclusion chưa review.

Kỳ vọng

Pilot cần review.

Scale Gate hold/block.

Dashboard hiển thị COD fail/cancel/refund.

Owner phải thấy rủi ro.

Fail nếu

ROAS đẹp che COD fail.

Scale vẫn pass.

Dashboard ẩn chỉ số thất bại.

Pilot accepted không có review.

## 4.9.3. ADS-FINAL-SMK-037 - Scale Gate cần đủ điều kiện

Điều kiện

Campaign muốn scale.

Kỳ vọng Scale Gate chỉ pass nếu đủ:

Pilot accepted.

Verified Revenue available.

Data quality pass.

Dedup pass.

Attribution pass hoặc conflict excluded.

Suppression clear.

SKU sellable.

Claim approved.

Creative approved.

Dashboard reconciled.

Evidence accepted.

Owner approval completed.

Release decision recorded.

Fail nếu

Thiếu bất kỳ điều kiện nào nhưng Scale Gate vẫn pass.

## 4.9.4. ADS-FINAL-SMK-038 - Owner chưa approval thì không scale

Điều kiện

Pilot pass.

Dashboard đẹp.

Data quality pass.

Owner chưa duyệt scale.

Kỳ vọng

Scale Owner Approval WAITING.

Không tăng ngân sách.

Không nhân campaign.

Không gọi Scale Approved.

Fail nếu

Team tự scale trước approval.

Dashboard pass bị hiểu là owner approval.

Approval cũ dùng sai scope.

Scale vượt phạm vi owner đã duyệt.

## 4.9.5. ADS-FINAL-SMK-039 - Evidence chưa accepted thì không Completion Decision

Điều kiện

Tài liệu đã xong.

Smoke đã chạy một phần.

Evidence còn WAITING hoặc missing.

Kỳ vọng

Completion Decision không được cấp.

Release Ready không được cấp.

Dashboard có thể dùng debug, không dùng scale.

Evidence status hiển thị rõ.

Fail nếu

Evidence WAITING nhưng PASS.

Evidence missing nhưng Release Ready.

Evidence rejected nhưng vẫn scale.

Owner không thấy evidence gap.

## 4.9.6. ADS-FINAL-SMK-040 - Release Handoff không được gọi Go-live Approved nếu thiếu decision

Điều kiện

Documentation Done.

Evidence accepted.

Smoke pass.

Owner review có thể đã xong.

Nhưng chưa có release decision.

Kỳ vọng

Release Ready có thể được xét nếu đủ điều kiện.

Go-live Approved chưa được cấp.

Scale Approved chưa tự động mở rộng toàn hệ thống.

Global Gateway vẫn theo release governance.

Fail nếu

Documentation Done = Go-live Approved.

Smoke Pass = Go-live Approved.

Owner review = Go-live Approved khi thiếu release decision.

Global Gateway bị chặn bị bỏ qua.

## 4.10. Final Evidence Package

## 4.10.1. Nguyên tắc Evidence Package

Evidence Package của TECH-07 là tập bằng chứng bắt buộc để chứng minh Ads Measurement vận hành đúng.

Không có evidence thì không PASS.

Evidence phải:

Có nguồn rõ.

Có owner rõ.

Có phạm vi rõ.

Có timestamp hoặc thời điểm review.

Có trạng thái review.

Có kết quả accepted / rejected / WAITING.

Có liên kết với campaign, creative, revenue, attribution hoặc scale decision tương ứng.

Không được dùng ảnh chụp rời rạc để thay thế nguồn dữ liệu chính nếu ảnh không đủ đối soát.

Không được tự tạo evidence giả.

Không được sửa evidence không có audit trail.

## 4.10.2. Evidence nhóm Campaign

Cần có:

Campaign identity evidence.

Campaign owner evidence.

Campaign objective evidence.

Campaign SKU scope evidence.

Campaign channel scope evidence.

Campaign start/end window evidence.

Pilot window evidence.

Campaign status history evidence.

Campaign suppression history evidence.

Campaign scale decision evidence.

## 4.10.3. Evidence nhóm Adset / Ad / Creative

Cần có:

Adset identity evidence.

Ad identity evidence.

Creative identity evidence.

Creative version evidence.

Creative content approval evidence.

Claim approval evidence.

Public-safe wording evidence.

Product/SKU mapping evidence.

Landing/Messenger/Live destination evidence.

Creative suppression evidence.

## 4.10.4. Evidence nhóm Product / SKU / Sellable

Cần có:

Product active evidence.

SKU active evidence.

Sellable status evidence.

Sale Lock status evidence.

Recall status evidence.

Warehouse readiness evidence nếu dùng cho scale.

Public Trace readiness evidence nếu bắt buộc cho public claim/trace.

Suppression clear evidence.

Owner approval nếu clear suppression.

Runtime availability evidence.

## 4.10.5. Evidence nhóm Pixel / CAPI / Event

Cần có:

Pixel configuration evidence.

CAPI configuration evidence.

Event map evidence.

Event source evidence.

Event timestamp evidence.

Event identity evidence.

Event classification evidence.

Pixel/CAPI dedup evidence.

Test event evidence.

Production event evidence.

Purchase event gating evidence.

Event hold/reject evidence.

## 4.10.6. Evidence nhóm Facebook Gateway / Live / Messenger

Cần có:

Page registry evidence.

Meta app / permission readiness evidence.

Live session evidence.

Comment event evidence.

Comment classification evidence.

Messenger handoff attempted evidence.

Messenger handoff succeeded/failed evidence.

Public/private boundary evidence.

Moderation flag evidence.

Gateway suppression evidence.

Rate limit / anti-spam evidence.

Channel policy evidence.

## 4.10.7. Evidence nhóm Commerce / Revenue

Cần có:

Quote evidence.

Cart evidence.

Order Draft evidence.

Official order evidence.

Payment method evidence.

Payment confirmation evidence.

COD success/fail evidence.

Order verification evidence.

Verified Revenue evidence.

Revenue amount evidence.

Revenue timestamp evidence.

Revenue exclusion status evidence.

Cancel/refund/invalid evidence.

Revenue adjustment/reversal evidence.

## 4.10.8. Evidence nhóm Attribution

Cần có:

Event trail evidence.

Campaign attribution evidence.

Adset attribution evidence.

Ad attribution evidence.

Creative attribution evidence.

Live/Messenger/Landing attribution evidence.

Attribution window evidence.

Unattributed revenue evidence.

Partial attribution evidence nếu có.

Attribution conflict evidence.

Owner-approved attribution rule evidence.

Attribution rerun evidence nếu có.

## 4.10.9. Evidence nhóm Diamond Referral

Cần có:

Referral link identity evidence.

Referral owner evidence.

Referral owner tier evidence.

Bind status evidence.

Bound timestamp evidence.

Buyer source evidence.

Ads touchpoint evidence.

Diamond conflict evidence.

Conflict resolution evidence.

Commission eligibility evidence.

No-bind exclusion evidence.

Self-purchase exclusion evidence nếu áp dụng.

## 4.10.10. Evidence nhóm CRM Boundary

Cần có:

CRM campaign identity evidence.

CRM message trigger evidence.

CRM message log evidence.

Customer opt-out evidence.

CRM touchpoint timestamp evidence.

CRM attribution rule evidence.

CRM/Ads conflict evidence.

CRM/Diamond conflict evidence nếu có.

Conflict resolution evidence.

CRM revenue separation evidence.

## 4.10.11. Evidence nhóm Data Quality

Cần có:

Event completeness report.

Event identity report.

Event timestamp report.

Campaign/ad/creative mapping report.

Quote/order/payment mapping report.

Verified revenue mapping report.

Dedup report.

Attribution conflict report.

Dashboard reconciliation report.

Suppression status report.

Evidence completeness report.

Data quality pass/fail decision.

## 4.10.12. Evidence nhóm Dashboard

Cần có:

Metric definition evidence.

Funnel metric evidence.

Verified Revenue reconciliation evidence.

ROAS calculation evidence.

CPA calculation evidence.

AOV calculation evidence.

Boxes per Order calculation evidence.

COD Fail / Cancel / Refund display evidence.

Attribution status display evidence.

Data Quality status display evidence.

Suppression status display evidence.

Scale Gate status display evidence.

Owner approval status display evidence.

## 4.10.13. Evidence nhóm Pilot

Cần có:

Pilot approval evidence.

Pilot scope evidence.

Pilot start/end evidence.

Pilot budget evidence.

Pilot campaign/ad/creative evidence.

Pilot tracking evidence.

Pilot data quality evidence.

Pilot revenue verification evidence.

Pilot attribution evidence.

Pilot suppression evidence.

Pilot review evidence.

Pilot accepted / failed / extended decision evidence.

## 4.10.14. Evidence nhóm Scale Gate

Cần có:

Scale request evidence.

Scale readiness checklist.

Verified Revenue evidence.

Data Quality Pass evidence.

Dedup Pass evidence.

Attribution Pass evidence.

Suppression Clear evidence.

SKU Sellable evidence.

Claim/Creative approval evidence.

Pilot Accepted evidence.

Owner Approval evidence.

Scale scope evidence.

Scale decision evidence.

Scale revoke evidence nếu có.

## 4.10.15. Evidence nhóm Release Handoff

Cần có:

Documentation Done evidence.

Smoke Pass evidence.

Evidence Accepted package.

Pilot Accepted evidence nếu liên quan scale.

Scale Gate evidence.

Owner sign-off evidence.

Release Ready decision.

Release Approved decision.

Go-live Approved decision nếu có.

TECH-08 handoff note.

Global Gateway status evidence.

Outstanding risk evidence.

## 4.11. Evidence Status Model

## 4.11.1. Trạng thái evidence

Evidence có các trạng thái:

## EVIDENCE_NOT_REQUIRED

## EVIDENCE_REQUIRED

## EVIDENCE_WAITING

## EVIDENCE_SUBMITTED

## EVIDENCE_UNDER_REVIEW

## EVIDENCE_ACCEPTED

## EVIDENCE_REJECTED

## EVIDENCE_EXPIRED

## EVIDENCE_REVIEW_REQUIRED

## EVIDENCE_BLOCKED

## 4.11.2. Evidence acceptance rule

Chỉ EVIDENCE_ACCEPTED mới được dùng cho:

Completion Decision.

Release Ready.

Scale Gate.

Owner Approval.

Go-live decision.

Các trạng thái sau không được xem là pass:

## WAITING.

Submitted.

Under Review.

Rejected.

Expired.

bị chặn.

## 4.11.3. Evidence P0 điểm chặn

TECH-07 bị bị chặn nếu:

Evidence missing.

Evidence WAITING nhưng Completion Decision.

Evidence rejected nhưng Release Ready.

Evidence expired nhưng Scale Approved.

Evidence không đúng campaign scope.

Evidence không đúng creative scope.

Evidence không đúng revenue scope.

Evidence không đủ owner review.

Evidence bị sửa không audit.

Evidence giả hoặc không kiểm chứng được.

## 4.12. Data Quality Review

## 4.12.1. Data Quality Review bắt buộc

Trước khi dùng dashboard để scale, phải review:

Event completeness.

Event source.

Event identity.

Event timestamp.

Pixel/CAPI dedup.

Gateway signal validity.

Quote/cart/order draft exclusion.

Commerce Verified Revenue mapping.

Payment/COD state.

Cancel/refund/invalid exclusion.

Attribution mapping.

Ads/CRM/Diamond conflict.

Dashboard reconciliation.

Suppression status.

Evidence completeness.

## 4.12.2. Data Quality Pass

Data Quality chỉ PASS khi:

Không có critical missing event.

Không có revenue lệch Commerce.

Không có purchase double count.

Không có attribution conflict chưa xử lý trong scale revenue.

Không có suppression unreadable.

Không có dashboard unreconciled.

Evidence required đã accepted.

## 4.12.3. Data Quality Hold

Data Quality phải HOLD nếu:

Thiếu campaign/ad/creative identity.

Thiếu event trail.

Thiếu customer/session linkage trong phạm vi privacy.

Commerce mapping chưa ổn định.

Pixel/CAPI event chưa rõ dedup.

Attribution conflict đang review.

Suppression status chưa đọc được.

Dashboard đang lệch nguồn.

## 4.12.4. Data Quality Fail

Data Quality phải FAIL nếu:

Revenue không khớp Commerce.

Purchase double count.

Quote/cart/order draft bị tính revenue.

COD fail vẫn tính revenue.

Refund/cancel bị bỏ qua.

Ads/CRM/Diamond double count.

Dashboard che giấu suppression.

Evidence bị reject.

Runtime unavailable nhưng dashboard vẫn pass.

## 4.13. ROAS / CPA / AOV Review

## 4.13.1. ROAS Review

ROAS chỉ được review khi:

Có Ads Spend hợp lệ.

Có Verified Revenue hợp lệ.

Có attribution hợp lệ.

Có exclusion hợp lệ.

Có dedup pass.

Có data quality pass.

ROAS không được tính từ:

Quote.

Cart.

Order Draft.

Unpaid order.

Payment WAITING.

## COD WAITING.

COD fail.

Cancelled / refunded / invalid order trái policy.

AI estimate.

Gateway estimate.

## 4.13.2. CPA Review

CPA phải phân biệt:

Cost per Lead.

Cost per Inbox.

Cost per Quote.

Cost per Order Draft.

Cost per Verified Order.

Chỉ Cost per Verified Order mới phản ánh chi phí đơn hàng đã xác minh.

Không được lấy Cost per Quote thay CPA doanh thu.

## 4.13.3. AOV Review

AOV chỉ được tính từ:

Verified Revenue.

Verified Order count.

Revenue exclusion đã xử lý.

Refund/cancel/invalid đã loại hoặc điều chỉnh theo policy.

Không được tính AOV từ quote value hoặc cart value.

## 4.13.4. Boxes per Order Review

Boxes per Order phải tính từ đơn đã verified.

Không được tính từ:

Cart quantity.

Quote quantity.

Order Draft quantity.

Unpaid order quantity.

Cancelled/refunded/invalid order quantity trái policy.

## 4.14. Pilot Review

## 4.14.1. Pilot Review bắt buộc

Pilot Review phải kiểm tra:

Pilot có đúng thời lượng 7-14 ngày hoặc owner-approved pilot window không.

Campaign có đúng SKU scope không.

Creative/claim có approved không.

Pixel/CAPI tracking có ổn định không.

Gateway event có ổn định không.

Commerce Verified Revenue mapping có đúng không.

Dedup có pass không.

Data Quality có pass không.

Attribution có conflict không.

COD fail/cancel/refund có bất thường không.

Suppression có active không.

Dashboard có reconcile không.

Evidence có accepted không.

Owner có review không.

## 4.14.2. Pilot Accepted

Pilot chỉ accepted khi:

Evidence accepted.

Smoke pass.

Data quality pass.

Revenue verified mapping pass.

Attribution issue resolved hoặc excluded.

Suppression clear.

Dashboard reconciled.

Owner review completed.

## 4.14.3. Pilot Failed

Pilot phải failed nếu:

Tracking sai.

Revenue lệch Commerce.

Double count revenue.

COD fail/cancel/refund quá cao chưa giải thích.

Creative/claim issue.

Suppression active.

Evidence rejected.

Owner không accept.

## 4.14.4. Pilot Extended

Pilot có thể extended nếu:

Dữ liệu chưa đủ nhưng không fail.

Event volume thấp.

Attribution cần thêm evidence.

Commerce mapping cần quan sát thêm.

Owner yêu cầu thêm thời gian review.

Không có P0 điểm chặn nghiêm trọng.

Pilot Extended không đồng nghĩa Scale Approved.

## 4.15. Scale Gate Review

## 4.15.1. Scale Gate Checklist

Scale Gate chỉ được PASS khi đủ:

Campaign registered.

Campaign owner rõ.

SKU scope rõ.

SKU sellable.

Không Sale Lock.

Không Recall.

Không Operational suppression.

Không Commerce suppression.

Không AI/Gateway suppression.

Claim approved.

Creative approved.

Pixel/CAPI tracking pass.

Gateway signal pass.

Verified Revenue mapping pass.

Dedup pass.

Data Quality pass.

Attribution pass hoặc conflict excluded.

Dashboard reconciled.

Pilot accepted.

Evidence accepted.

Owner approval completed.

Release decision recorded.

## 4.15.2. Scale Gate bị chặn

Scale Gate phải bị chặn nếu:

No Verified Revenue.

Data Quality Fail.

Dedup Fail.

Attribution Conflict WAITING.

Suppression Active.

SKU Not Sellable.

Sale Lock / Recall active.

Claim Not Approved.

Creative Not Approved.

Pilot Not Accepted.

Evidence Missing.

Owner Not Approved.

Runtime Unavailable.

Dashboard Unreconciled.

Release Decision Missing.

## 4.15.3. Scale Gate không phải lệnh tăng ngân sách

Scale Gate PASS chỉ có nghĩa:

Campaign đủ điều kiện đề xuất scale theo phạm vi đã duyệt.

Scale Gate PASS không tự động:

Tăng ngân sách.

Nhân campaign.

Mở creative mới.

Mở SKU mới.

Mở kênh mới.

Vượt phạm vi owner approval.

Mọi hành động scale thật phải theo owner-approved scale scope.

## 4.16. Owner Review Points

## 4.16.1. Owner phải review các điểm sau

Trước khi approve scale, owner phải thấy rõ:

Campaign nào.

SKU nào.

Creative nào.

Claim nào.

Channel nào.

Pilot window nào.

Ads Spend bao nhiêu.

Verified Revenue bao nhiêu.

ROAS bao nhiêu.

CPA bao nhiêu.

AOV bao nhiêu.

Boxes per Order bao nhiêu.

COD Fail Rate bao nhiêu.

Cancel Rate bao nhiêu.

Refund Rate bao nhiêu.

Attribution conflict còn không.

Diamond/CRM conflict còn không.

Data Quality status.

Suppression status.

Evidence status.

Scale scope đề xuất.

Rủi ro còn tồn tại.

Điều kiện phải theo dõi sau scale.

## 4.16.2. Owner Approval Scope

Owner approval phải ghi rõ phạm vi:

Campaign.

Adset.

Ad.

Creative.

## SKU.

Channel.

Budget range.

Time window.

Scale level.

Risk condition.

Stop condition.

Re-review condition.

Không được dùng approval chung chung để scale vượt phạm vi.

## 4.16.3. Owner Rejection

Nếu owner reject:

Campaign không scale.

Lý do reject phải được ghi.

Muốn review lại phải có evidence mở rộng.

Không được tự override owner.

## 4.17. TECH-07 Done Gate

## 4.17.1. Documentation Done Gate

TECH-07 được xem là Documentation Done khi đạt đủ:

## PHẦN 1/4 đã khóa Ads Measurement Principles.

## PHẦN 1/4 đã khóa Source-of-Truth.

## PHẦN 1/4 đã khóa Verified Revenue Boundary.

## PHẦN 1/4 đã khóa No-Fake-ROAS Rule.

## PHẦN 2/4 đã khóa đầy đủ Module Contracts.

## PHẦN 2/4 đã khóa Scope In / Scope Out cho từng module.

## PHẦN 2/4 đã khóa Upstream / Downstream cho từng module.

## PHẦN 2/4 đã khóa P0 điểm chặn cho từng module.

## PHẦN 3/4 đã khóa Lifecycle / State Machine.

## PHẦN 3/4 đã khóa Event-to-Revenue Boundary.

## PHẦN 3/4 đã khóa Attribution / Conflict Lifecycle.

## PHẦN 3/4 đã khóa Scale Control.

## PHẦN 4/4 đã khóa Final Smoke Matrix.

## PHẦN 4/4 đã khóa Evidence Package.

## PHẦN 4/4 đã khóa Data Quality Review.

## PHẦN 4/4 đã khóa Pilot Review.

## PHẦN 4/4 đã khóa Scale Gate Review.

## PHẦN 4/4 đã khóa Owner Review.

## PHẦN 4/4 đã khóa Done Gate / Fail Gate / Release Handoff.

Không viết code.

Không thiết kế API/DB/UI chi tiết.

Không dùng tài liệu cũ làm nền.

Không gọi Documentation Done là Production Ready.

Nếu đạt đủ các điều kiện này:

Nhưng chưa phải:

Production Ready.

Release Ready.

Release Approved.

Go-live Approved.

Scale Approved.

## 4.17.2. Evidence Done Gate

TECH-07 chỉ đạt Evidence Done khi:

Campaign evidence accepted.

Creative evidence accepted.

Claim evidence accepted.

Pixel/CAPI evidence accepted.

Gateway evidence accepted.

Commerce Verified Revenue evidence accepted.

Dedup evidence accepted.

Exclusion evidence accepted.

Attribution evidence accepted.

Data Quality evidence accepted.

Dashboard evidence accepted.

Pilot evidence accepted.

Scale Gate evidence accepted.

Owner Approval evidence accepted.

Release Handoff evidence accepted.

Nếu còn evidence WAITING:

## 4.17.3. Smoke Done Gate

TECH-07 chỉ đạt Smoke Done khi:

ADS-FINAL-SMK-001 -> ADS-FINAL-SMK-040 đều pass hoặc có documented exception owner-approved.

Không có P0 smoke fail.

Không có revenue double count.

Không có dashboard lệch Commerce.

Không có scale khi suppression active.

Không có scale khi data quality fail.

Không có scale khi owner chưa approval.

Smoke evidence accepted.

Nếu còn smoke WAITING hoặc fail:

## 4.17.4. Release Ready Gate

TECH-07 chỉ được gọi Release Ready khi:

Documentation Complete.

Evidence Done.

Smoke Done.

Data Quality Pass.

Dashboard Reconciled.

Suppression Clear hoặc documented non-scale risk.

Owner Review completed.

Release checklist completed.

Nếu thiếu bất kỳ điều kiện nào:

## 4.17.5. Release Approved Gate

TECH-07 chỉ được gọi Release Approved khi:

Owner sign-off completed.

Release decision recorded.

Scope release rõ.

Risk accepted nếu còn residual risk.

Downstream handoff accepted.

Nếu owner chưa sign-off:

## 4.17.6. Go-live Approved Gate

TECH-07 chỉ được gọi Go-live Approved khi:

Global Gateway cho phép.

Production runtime readiness pass.

Monitoring / rollback / suppression control pass.

Owner go-live decision recorded.

TECH-08 handoff không bị blocking nếu có liên quan.

Nếu Global Gateway vẫn bị chặn:

## 4.18. TECH-07 Fail Gate

## 4.18.1. Fail Gate nhóm Revenue

TECH-07 FAIL nếu:

Ads dùng quote làm revenue.

Ads dùng cart làm revenue.

Ads dùng Order Draft làm revenue.

Ads dùng unpaid order làm revenue.

Ads dùng payment WAITING làm revenue.

Ads dùng ảnh chuyển khoản làm PAID.

Ads dùng COD WAITING làm revenue.

Ads dùng COD fail làm revenue.

Ads giữ revenue của cancelled order trái policy.

Ads giữ revenue của refunded order trái policy.

Ads giữ revenue của invalid order.

Ads tự tạo Verified Revenue.

## 4.18.2. Fail Gate nhóm Pixel / CAPI

TECH-07 FAIL nếu:

Pixel/CAPI phát Purchase khi chưa có Verified Revenue.

Pixel/CAPI không có dedup.

Pixel/CAPI double count event.

Pixel/CAPI revenue lệch Commerce.

Test event vào production dashboard.

Purchase event thiếu idempotency.

Retry tạo thêm revenue.

## 4.18.3. Fail Gate nhóm Gateway Signal

TECH-07 FAIL nếu:

Comment được tính revenue.

Inbox được tính revenue.

Messenger handoff được tính revenue.

AI conversation được tính revenue.

Handoff failed bị tính success.

Abuse/moderation flag bị bỏ qua.

Gateway suppression bị bỏ qua.

Public/private boundary bị vi phạm.

## 4.18.4. Fail Gate nhóm Attribution

TECH-07 FAIL nếu:

Attribution dùng unverified revenue.

Attribution thiếu event trail.

Revenue bị gán bừa cho campaign.

Revenue bị gán bừa cho creative.

Ads/CRM/Diamond double count.

No bind vẫn Diamond attribution.

Conflict WAITING vẫn dùng scale.

Attribution rerun nhân đôi revenue.

Attribution bị sửa không evidence.

## 4.18.5. Fail Gate nhóm Dashboard

TECH-07 FAIL nếu:

Dashboard là source-of-truth.

Dashboard revenue lệch Commerce.

Dashboard không tách signal và revenue.

Dashboard ẩn COD fail.

Dashboard ẩn cancel/refund.

Dashboard ẩn attribution conflict.

Dashboard ẩn suppression.

Dashboard ẩn data quality fail.

Dashboard gọi scale-ready khi Scale Gate chưa pass.

## 4.18.6. Fail Gate nhóm Suppression / Scale

TECH-07 FAIL nếu:

Campaign scale khi Sale Lock active.

Campaign scale khi Recall active.

Campaign scale khi SKU not sellable.

Campaign scale khi claim chưa approved.

Campaign scale khi creative chưa approved.

Campaign scale khi data quality fail.

Campaign scale khi runtime unavailable.

Campaign scale khi pilot chưa accepted.

Campaign scale khi owner chưa approval.

Campaign scale vượt phạm vi owner approval.

## 4.18.7. Fail Gate nhóm Governance

TECH-07 FAIL nếu:

Documentation Complete bị gọi là Production Ready.

Evidence WAITING nhưng Completion Decision.

Smoke fail nhưng Release Ready.

Owner chưa sign-off nhưng Release Approved.

Chưa release decision nhưng Go-live Approved.

Global Gateway bị chặn bị bỏ qua.

Tài liệu cũ được dùng làm source-of-truth thay TECH-07.

Dev bị yêu cầu tự suy luận rule còn thiếu.

Có nội dung đi vào code/API/DB/UI chi tiết ngoài phạm vi hiện tại.

## 4.19. Release Handoff

## 4.19.1. Handoff từ TECH-07 sang TECH-08 MC AI Live

TECH-07 bàn giao cho TECH-08 các nguyên tắc bắt buộc:

MC AI Live không được xem comment/live engagement là revenue.

MC AI Live không được dùng số inbox làm doanh thu.

MC AI Live không được dùng quote làm ROAS.

MC AI Live không được báo hiệu quả campaign bằng revenue chưa verified.

MC AI Live phải tôn trọng Ads Measurement Verified Revenue Boundary.

MC AI Live phải tôn trọng Sale Lock / Recall / Suppression.

MC AI Live phải tôn trọng Creative / Claim approval.

MC AI Live không được thúc đẩy scale khi Data Quality fail.

MC AI Live phải hiểu Live signal là funnel signal.

MC AI Live chỉ được dùng dashboard đã reconciled cho review điều hành.

MC AI Live không được kích hoạt campaign scale khi Owner chưa approval.

MC AI Live phải tôn trọng Global Gateway status.

## 4.19.2. Handoff sang Commerce Runtime

TECH-07 không thay đổi Commerce.

TECH-07 chỉ gửi yêu cầu đối soát hoặc đọc trạng thái từ Commerce.

Commerce vẫn là owner của:

QuoteSnapshot.

Cart.

Order Draft.

Official Order.

Payment state.

COD state.

Order Verification.

Verified Revenue.

Cancel/refund/invalid policy.

Revenue exclusion.

Nếu Ads dashboard lệch Commerce, Commerce thắng.

## 4.19.3. Handoff sang Facebook Gateway

TECH-07 nhận signal từ Gateway nhưng không thay Gateway.

Gateway vẫn là owner của:

Page registry.

Live session event.

Comment classification.

Messenger handoff.

Typing / pacing delivery.

Public/private boundary.

Rate limit.

Moderation.

Channel suppression.

Nếu Gateway signal thiếu evidence, Ads Measurement phải hold.

## 4.19.4. Handoff sang AI Advisor

TECH-07 nhận consultation signal từ AI Advisor nhưng không thay AI Advisor.

AI Advisor vẫn là owner của:

Product consulting.

Product recommendation.

Final Response Guard.

Quote-safe sales handoff.

Order draft handoff.

Claim-safe wording.

Customer memory boundary.

Human handoff.

AI conversation không phải revenue.

AI order draft handoff không phải revenue.

## 4.19.5. Handoff sang Operational Core

TECH-07 phải tôn trọng Operational Core.

Operational Core vẫn là owner của:

SKU sellable dependency từ operational side.

Warehouse readiness.

Batch release.

Inventory availability.

Public Trace readiness.

Recall.

Sale Lock.

Operational suppression.

Nếu Operational Core chặn, Ads scale bị chặn.

## 4.19.6. Handoff sang Owner / Management Review

TECH-07 phải bàn giao cho owner:

Dashboard đã reconciled.

Data quality report.

Verified revenue report.

Attribution report.

Conflict report.

Suppression report.

Pilot report.

Scale recommendation.

Risk note.

Required owner decision.

Owner không được nhận dashboard “đẹp” nhưng thiếu evidence.

## 4.20. Release Status Model

## 4.20.1. Trạng thái release của TECH-07

TECH-07 có các trạng thái release:

## DOCUMENTATION_DRAFT

## DOCUMENTATION_COMPLETE

## EVIDENCE_WAITING

## EVIDENCE_ACCEPTED

## SMOKE_WAITING

## SMOKE_PASSED

## RELEASE_READY

## RELEASE_DECISION_ACCEPTED

## GO_LIVE_DECISION_ACCEPTED

bị chặn

## ROLLBACK_REQUIRED

## ARCHIVED

## 4.20.2. Trạng thái hiện tại sau tài liệu này

Sau khi hoàn thành PHẦN 4/4:

Nhưng mặc định vẫn là:

Evidence WAITING.

Smoke WAITING.

Global Gate bị chặn cho đến khi đủ release decision.

## 4.21. Downstream Readiness Rule

## 4.21.1. TECH-08 không được dựa trên sai số liệu

TECH-08 MC AI Live không được lấy số liệu từ TECH-07 nếu:

Dashboard chưa reconciled.

Revenue chưa verified.

Data Quality fail.

Attribution conflict WAITING.

Suppression active.

Owner chưa approval.

Evidence thiếu.

## 4.21.2. TECH-08 được phép dựa trên gì

TECH-08 được phép dựa trên:

Event signal boundary.

Live signal classification.

Comment/inbox/quote/order draft không phải revenue.

Verified Revenue Boundary.

Dashboard readiness status.

Suppression status.

Scale Gate status.

Owner approval requirement.

No-Fake-ROAS Rule.

Evidence-first release logic.

## 4.22. Final P0 Lock Summary

TECH-07 khóa vĩnh viễn các P0 rule sau:

Ads không được dùng quote làm revenue.

Ads không được dùng cart làm revenue.

Ads không được dùng Order Draft làm revenue.

Ads không được dùng unpaid order làm revenue.

Ads không được dùng payment WAITING làm revenue.

Ads không được dùng ảnh chuyển khoản làm PAID.

Ads không được dùng COD WAITING làm revenue.

Ads không được dùng COD fail làm revenue.

Ads chỉ được dùng Verified Revenue từ Commerce.

Payment WAITING không phải revenue.

COD WAITING không phải revenue.

Cancelled / refunded / invalid order phải excluded theo policy.

Không có order verification thì không verified revenue.

Gateway event không phải revenue.

AI conversation không phải revenue.

Comment / inbox / quote là funnel signal, không phải revenue.

Diamond referral bind phải rõ.

Ads + Diamond conflict phải có attribution priority/allocation rule.

CRM revenue và Ads revenue không được double count.

Pixel/CAPI event phải dedup/idempotent.

Không được double count revenue.

Dashboard không phải source-of-truth.

Không được scale chỉ vì ROAS đẹp.

Không được scale khi Data Quality fail.

Không được scale khi Evidence chưa accepted.

Không được scale khi Sale Lock / Recall / Suppression active.

Không được scale SKU không sellable.

Không được scale claim/creative chưa approved.

Pilot 7-14 ngày phải có evidence trước scale.

Scale phải có owner approval.

Runtime unavailable thì fail-safe, không scale.

Documentation Complete không phải Production Ready.

Không có accepted evidence thì không Completion Decision.

Không có smoke pass thì không Release Ready.

Không có owner sign-off thì không Release Approved.

Không có release decision thì không Go-live Approved.

Global Gateway mặc định bị chặn.

## 4.23. TECH-07 Final Done Gate Checklist

TECH-07 chỉ được đánh dấu hoàn tất tài liệu khi checklist sau đạt:

Nhóm

Điều kiện

Trạng thái sau tài liệu

Principles

Đã khóa Ads Measurement / Verified Revenue / No-Fake-ROAS

## DONE

Source-of-Truth

Đã khóa Product / Operational / Commerce / AI / Gateway / Ads boundary

## DONE

Module Contracts

Đã khóa 24 module contract

## DONE

Lifecycle

Đã khóa Campaign / Event / Revenue / Attribution / Dashboard / Pilot / Scale

## DONE

Smoke Matrix

Đã khóa 40 smoke final

## DONE

Evidence Package

Đã khóa evidence groups

## DONE

Data Quality

Đã khóa pass/hold/fail

## DONE

Pilot

Đã khóa 7-14 days review

## DONE

Scale Gate

Đã khóa pass/block rule

## DONE

Owner Approval

Đã khóa owner review/scope/rejection

## DONE

Release Handoff

Đã khóa handoff sang TECH-08 và các domain liên quan

## DONE

Code/API/DB/UI

Chưa đi vào chi tiết

## COMPLIANT

Production Ready

Chưa được gọi Production Ready

## COMPLIANT

Kết luận checklist:

TECH-07 DOCUMENTATION COMPLETE - CO

Nhưng:

PRODUCTION READY - KHONG
RELEASE READY - KHONG
GO-LIVE APPROVED - KHONG
SCALE APPROVED - KHONG

## 4.24. TECH-07 Final Conclusion

TECH-07 hoàn thành vai trò là tài liệu nguồn sự thật cho tầng Ads Measurement.

Tài liệu này khóa rõ:

Ads Measurement không phải nơi tạo doanh thu.

Ads Measurement không được làm đẹp ROAS.

Ads Measurement không được lấy quote, cart, order draft, unpaid order, comment, inbox, AI conversation, payment WAITING hoặc COD WAITING làm revenue.

Doanh thu duy nhất được dùng cho Ads là Verified Revenue từ Commerce.

Pixel/CAPI chỉ là công cụ event.

Facebook Gateway chỉ cung cấp channel signal.

AI Advisor chỉ cung cấp consultation / quote-safe sales signal.

Commerce mới là nguồn xác nhận order, payment, COD, verification và revenue.

Operational Core mới là nguồn Sale Lock / Recall / Sellable / Warehouse / Trace readiness.

Dashboard chỉ là lớp hiển thị đã đối soát, không phải nguồn sự thật.

Scale không được quyết định chỉ vì ROAS đẹp.

Scale phải qua:

Verified Revenue.

Dedup.

Exclusion.

Attribution.

Data Quality.

Suppression Clear.

Pilot Accepted.

Evidence Accepted.

Owner Approval.

Release Decision.

TECH-07 bảo vệ doanh nghiệp khỏi các sai lầm rất tốn kém:

Tính nhầm tín hiệu thành doanh thu.

Tính trùng revenue.

Scale bằng ROAS ảo.

Scale khi SKU không được bán.

Scale khi creative/claim chưa được duyệt.

Scale khi Sale Lock/Recall đang active.

Scale khi Diamond/CRM/Ads conflict chưa xử lý.

Scale khi dashboard chưa đối soát.

Scale khi owner chưa phê duyệt.

Gọi tài liệu hoàn chỉnh là hệ thống đã sẵn sàng chạy thật.

Kết luận cuối:

Trạng thái chính thức sau PHẦN 4/4:

Chưa được gọi:

Production Ready.

Release Ready.

Release Approved.

Go-live Approved.

Scale Approved.

Bước tiếp theo theo Wave đã khóa:

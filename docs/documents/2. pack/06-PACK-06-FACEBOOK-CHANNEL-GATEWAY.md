# PACK-06 - FACEBOOK CHANNEL GATEWAY

## TYPING INDICATOR / RESPONSE PACING / PUBLIC-PRIVATE BOUNDARY / CHANNEL SAFETY

## PHẦN 1/4 — FACEBOOK GATEWAY PRINCIPLES / SOURCE-OF-TRUTH / CHANNEL BOUNDARY / NO-INSTANT-BOT-REPLY RULE

## 1. MỤC TIÊU CỦA PACK-06

## 1.1. Vai trò của PACK-06

PACK-06 là tài liệu kỹ thuật nền cho tầng Facebook Gateway / Meta Live & Messenger / Channel Delivery Runtime của Ginsengfood.

PACK-06 chịu trách nhiệm quản trị lớp kênh giao tiếp giữa khách hàng và các runtime phía trong:

Meta Live / Comment / Messenger / Web / Landing Page / CRM Channel -> Facebook Gateway / Channel Gateway -> AI Advisor -> Commerce Runtime -> Operational Core -> Product Domain.

PACK-06 không phải AI Advisor.

PACK-06 không phải Commerce Runtime.

PACK-06 không phải nơi tính giá.

PACK-06 không phải nơi tạo đơn.

PACK-06 không phải nơi xác nhận thanh toán.

PACK-06 không phải nơi quyết định sản phẩm có sellable hay không.

PACK-06 là lớp điều phối kênh, bảo vệ ranh giới public/private, chuyển đúng nội dung sang AI/Commerce, kiểm soát delivery, typing indicator, response pacing, live comment, Messenger handoff, webhook/channel safety, app review, permission, evidence và release gate.

## 1.2. PACK-06 cần giải quyết vấn đề gì

PACK-06 giải quyết các vấn đề thực chiến sau:

Khách comment public trên live, nhưng thông tin cần tư vấn sâu phải kéo sang Messenger/private.

Khách hỏi giá trên live/comment, nhưng giá riêng phải lấy từ QuoteSnapshot và không public bừa.

Khách comment ký hiệu mơ hồ như “.”, “ib”, “giá”, “mua”, “1 hộp”, emoji… Gateway phải route đúng.

Sau khi đã vào Messenger, các câu tiếp theo phải xử lý như private conversation, không lặp lại kiểu public comment.

AI response phải qua Final Response Guard của TECH-05 trước khi gửi.

Gateway không được gửi draft response chưa guard.

Gateway không được tự sửa câu trả lời để thêm giá, claim, urgency hoặc order.

Gateway không được trả lời bật ngay như chatbot máy móc.

Gateway phải có typing indicator / 3 chấm nhấp nháy và response pacing phù hợp độ dài câu trả lời.

Gateway phải bảo vệ dữ liệu riêng tư: địa chỉ, số điện thoại, thanh toán, hóa đơn, member tier, order, payment.

Gateway phải tôn trọng Sale Lock / Recall / Commerce Suppression / AI Suppression.

Gateway phải có evidence, smoke, owner review trước khi production.

## 2. PACK-06 NẰM Ở ĐÂU TRONG CHUỖI TECH

## 2.1. Vị trí của PACK-06

PACK-06 nằm sau:

TECH-00 — Technical Implementation Master Plan.

TECH-01 — Foundation / RBAC / Audit / Idempotency / Evidence Registry.

TECH-02 — Product / SKU / Ingredient / Recipe / Formula / Product Activation.

TECH-03 — Operational Core / Production / Warehouse / Inventory / Traceability / Recall / Sale Lock.

TECH-04 — Commerce Runtime / Quote / Cart / Order / Payment / Shipping.

TECH-05 — AI Advisor Runtime / Product Consulting / Quote-safe Sales / Human Handoff.

PACK-06 nằm trước:

TECH-07 — Ads Measurement.

TECH-08 — MC AI Live.

TECH-09 — IVR Order Confirmation.

TECH-10 — Completion / Evidence / Gateway / Release Readiness.

## 2.2. Nguyên tắc vị trí

Facebook Gateway là downstream của AI Advisor và Commerce.

Điều này có nghĩa:

Không có AI Final Response Guard pass thì Gateway không được gửi response.

Không có QuoteSnapshot thì Gateway không được hiển thị final price.

Không có CustomerConfirmation thì Gateway không được tạo official order.

Không có Official Order thì Gateway không được trả order_code.

Không có Payment Confirmation thì Gateway không được nói PAID.

Không có Public Trace pass thì Gateway không được gửi trace như hợp lệ.

Có Sale Lock / Recall / Suppression thì Gateway phải chặn quote/order/sales message.

Kênh public không được hiển thị dữ liệu private.

## 3. PHẠM VI CỦA PACK-06

## 3.1. PACK-06 bao gồm

PACK-06 khóa các domain chính sau:

Facebook Gateway Runtime Boundary.

Meta Page / Business / App / Permission Boundary.

Live Comment Channel Boundary.

Messenger Private Channel Boundary.

Comment-to-Messenger / Private Reply Handoff.

Public Comment Short Response Rule.

Messenger Deep Consulting Rule.

Quote / Order / Payment Channel Delivery Boundary.

Typing Indicator / 3-dot Response Pacing Rule.

No Instant Bot Reply Rule.

Response Chunking / Multi-message Delivery.

AI Final Response Guard Consumption.

Channel Public-safe View.

Customer Session Binding.

Live Session Context.

Page Context / Multi-page Routing.

Webhook Intake Boundary.

Message Delivery Boundary.

Rate Limit / Anti-spam / Platform Policy Boundary.

Moderation / Hide / Block / Abuse Handling.

Privacy / Sensitive Data / Public Leak Guard.

CRM / Proactive Message Channel Boundary.

Human Handoff Channel Boundary.

Evidence / Smoke / App Review / UAT / Pilot / Release Readiness.

## 3.2. PACK-06 không bao gồm

PACK-06 không thiết kế chi tiết:

API endpoint.

Database schema.

UI screen.

Code webhook.

Code Meta Graph API.

Code Messenger bot.

Code queue worker.

Code typing indicator implementation.

Code rate limiter.

Code app review submission.

Ads optimization algorithm.

AI prompt chi tiết.

Commerce pricing logic.

Payment gateway logic.

MISA integration.

Các phần này chỉ được triển khai ở DEV phase sau khi PACK-06 khóa boundary, module map, evidence, smoke và owner review.

## 4. NGUYÊN TẮC GREENFIELD CHO FACEBOOK GATEWAY

## 4.1. Tài liệu mới là nguồn đúng

Facebook Gateway được xây theo hướng GREENFIELD TECHNICAL RESET.

Điều này có nghĩa:

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

Code/webhook/bot cũ chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

TECH-00 -> PACK-06 là SOURCE_OF_TRUTH cho Facebook Gateway.

Nếu webhook/code cũ khác PACK-06 thì PACK-06 thắng.

Dev không được lấy logic trả lời, comment, inbox, giá, order hoặc payment từ bot cũ để override PACK-06.

Legacy chỉ dùng để gap analysis sau khi PACK-06 đã khóa.

## 4.2. Không code sâu trước khi khóa mapping

Facebook Gateway có rủi ro cao vì ảnh hưởng trực tiếp đến:

Public comment.

Messenger private conversation.

Meta policy.

Spam risk.

App Review.

Khách hàng thật.

Dữ liệu riêng tư.

Giá và chốt đơn.

Live commerce.

CRM outbound.

Khiếu nại public.

Niềm tin thương hiệu.

Vì vậy không được đi thẳng vào code khi chưa có:

Page map.

Channel map.

Webhook event map.

Comment-to-Messenger map.

AI response guard map.

Commerce Quote/Order handoff map.

Typing indicator / response pacing policy.

Public/private data boundary.

Rate limit / anti-spam policy.

Moderation rule.

Human handoff rule.

## P0 STOP_POINTS.

Evidence requirement.

Smoke matrix.

Owner review.

Pilot/release handoff.

## 5. SOURCE-OF-TRUTH CỦA FACEBOOK GATEWAY

## 5.1. Source-of-truth cấp tài liệu

Facebook Gateway phải tuân theo thứ tự nguồn đúng:

Cấp

Nguồn

Vai trò

1

MASTER / PACK đã khóa

Governance cấp cao

2

## TECH-00

Kế hoạch triển khai kỹ thuật tổng thể

3

## TECH-01

Permission, audit, idempotency, evidence

4

## TECH-02

Product / Product Knowledge / Product Scope

5

## TECH-03

Operational sellable, recall, sale lock, trace

6

## TECH-04

Quote, order, payment, verified revenue

7

## TECH-05

AI Advisor, guard, safe response

8

## PACK-06

Facebook Gateway / Channel Delivery source-of-truth

9

TECH-07 trở đi

Ads / Live / IVR consumer hoặc downstream

10

Code/webhook hiện tại

Current state để gap analysis

11

Tài liệu cũ

Legacy reference only

## 5.2. Source-of-truth cấp runtime

Loại dữ liệu

Owner đúng

Gateway không được tự làm

Product public content

TECH-02 Product Knowledge

Không tự viết/sửa claim

Sellable / stock / recall / sale lock

TECH-03 Operational

Không tự nói còn hàng

Price / quote

TECH-04 QuoteSnapshot

Không tự tính giá

Order Draft / CustomerConfirmation

TECH-04 Commerce

Không tự tạo order

Payment status

TECH-04 Payment Runtime

Không tự nói PAID

AI final response

TECH-05 Final Response Guard

Không gửi draft chưa guard

Public trace

TECH-03 Public Trace Runtime

Không tự xác nhận QR/trace

Customer memory

TECH-05 / CRM Runtime

Không dùng sai khách/kênh

CRM opt-out

TECH-05 / CRM Policy

Không gửi chủ động

Typing / pacing delivery

PACK-06 Channel Delivery

Không trả lời tức thì

Meta permissions/app state

PACK-06 Gateway Governance

Không bypass App Review/permission

Evidence/release status

TECH-10 / Evidence Registry

Không tự gọi Release Ready

## 6. VAI TRÒ CỦA FACEBOOK GATEWAY

## 6.1. Gateway là lớp điều phối kênh

Facebook Gateway chịu trách nhiệm:

Nhận sự kiện từ Meta Page/Live/Comment/Messenger.

Xác định kênh public/private.

Xác định page/live/session.

Gắn context cho AI Advisor.

Gửi input vào AI Advisor đúng boundary.

Nhận AI safe response.

Kiểm tra channel delivery policy.

Gửi response ra đúng kênh.

Kéo khách từ comment public vào Messenger khi cần.

Không public dữ liệu riêng.

Điều phối typing indicator / response pacing.

Ghi nhận evidence/log theo scope.

Handoff human khi cần.

Chặn hoặc ẩn comment theo moderation policy nếu scope có.

## 6.2. Gateway không phải AI

Gateway không tự:

Tư vấn sản phẩm.

Viết claim.

Chọn sản phẩm.

Gợi ý combo.

Trả lời y tế/thực dưỡng.

Cá nhân hóa CRM.

Xử lý complaint.

Gateway phải gọi AI Advisor và chỉ gửi câu trả lời đã qua guard.

## 6.3. Gateway không phải Commerce

Gateway không tự:

Tính giá.

Áp chương trình.

Áp member benefit.

Áp Diamond referral benefit.

Tạo QuoteSnapshot.

Tạo Order Draft.

Xác nhận đơn.

Cấp order_code.

Xác nhận thanh toán.

Tính verified revenue.

Gateway chỉ hiển thị hoặc chuyển tiếp safe view từ Commerce/AI khi runtime pass.

## 6.4. Gateway không phải Meta policy bypass

Gateway không được dùng automation để vượt:

Meta permission.

App Review.

Page role.

Messaging policy.

Anti-spam rule.

User consent.

CRM opt-out.

Public/private data boundary.

Nếu Meta permission chưa được duyệt hoặc channel chưa được cấu hình đúng, Gateway phải fail-safe.

## 7. BOUNDARY GIỮA AI ADVISOR VÀ FACEBOOK GATEWAY

## 7.1. AI Advisor tạo nội dung, Gateway giao nội dung

AI Advisor chịu trách nhiệm:

Hiểu intent.

Tư vấn.

Gợi ý.

Guard claim.

Guard privacy.

Guard sellable.

Gọi Commerce khi cần.

Tạo response safe view.

Facebook Gateway chịu trách nhiệm:

Đúng kênh.

Đúng page.

Đúng live session.

Đúng customer/session.

Đúng delivery timing.

Đúng public/private boundary.

Đúng typing indicator.

Đúng response pacing.

Đúng handoff target.

## 7.2. Gateway không được sửa nội dung để vượt guard

Gateway không được tự thêm:

Giá.

Discount.

Chương trình.

Member benefit.

Diamond benefit.

Claim công dụng.

Urgency giả.

Order confirmation.

Payment confirmation.

Link trace chưa approved.

Thông tin nội bộ.

Nếu cần thay đổi response theo kênh, phải quay lại AI Advisor / Final Response Guard hoặc dùng channel-safe template đã approved.

## 7.3. Gateway không gửi draft response

Gateway chỉ được gửi response khi có trạng thái:

## FINAL_RESPONSE_GUARD_PASS

Nếu AI response ở trạng thái:

Draft.

Needs rewrite.

bị chặn.

Handoff required.

Runtime unavailable unresolved.

Sensitive field detected.

Claim bị chặn.

Gateway không được gửi ra khách.

## 8. BOUNDARY GIỮA PUBLIC COMMENT VÀ PRIVATE MESSENGER

## 8.1. Public comment

Public comment gồm:

Comment live.

Comment bài viết.

Comment quảng cáo nếu scope có.

Comment public trên Page.

Public comment chỉ được dùng để:

Trả lời ngắn.

Public-safe.

Không cá nhân hóa sâu.

Không giá riêng.

Không order/payment.

Không địa chỉ/số điện thoại.

Không member tier.

Không Diamond referral private benefit.

Không health note riêng.

Không public số lượng tồn.

## 8.2. Private Messenger

Private Messenger được dùng để:

Tư vấn sâu.

Báo QuoteSnapshot nếu Commerce pass.

Trình Order Draft.

Thu thông tin nhận hàng.

Chọn phương thức thanh toán.

Hướng dẫn thanh toán.

CTA xác nhận đơn.

Trả order/payment status đúng khách.

Chăm sóc sau mua nếu policy cho phép.

Handoff human.

## 8.3. Rule khóa

Public comment không phải nơi báo giá/chốt đơn chi tiết.

Messenger/private flow là nơi xử lý quote/order/payment nếu runtime pass.

Sau khi handoff vào Messenger thành công, câu tiếp theo mặc định xử lý như private conversation, không lặp lại format public comment.

## 9. COMMENT-TO-MESSENGER / PRIVATE REPLY RULE

## 9.1. Mục tiêu

Comment-to-Messenger Rule đảm bảo khi khách tương tác public mà nhu cầu cần tư vấn sâu, Gateway kéo đúng sang private.

## 9.2. Các trường hợp cần kéo private

Gateway phải kéo private khi khách:

Hỏi giá.

Hỏi mua/chốt.

Hỏi sản phẩm phù hợp.

Hỏi nhiều SKU.

Gửi số điện thoại/địa chỉ.

Hỏi quyền lợi thành viên.

Hỏi Diamond/referral benefit.

Hỏi thanh toán/chuyển khoản.

Hỏi trạng thái đơn.

Hỏi hóa đơn.

Hỏi khiếu nại cần thông tin riêng.

Hỏi sức khỏe/nhu cầu cá nhân.

Comment mơ hồ có dấu hiệu muốn được tư vấn.

## 9.3. Public response sau khi kéo private

Nếu hệ thống có thể tự gửi private reply/Messenger, câu public chỉ nên ngắn:

“Dạ Em đã gửi thông tin vào Messenger để tư vấn cho Mình chính xác hơn ạ.”

Không nên public:

“Mình nhắn Messenger cho Em.”

Giá.

Quyền lợi riêng.

Thông tin order.

Thông tin thanh toán.

Hỏi quá nhiều câu ở public.

## 9.4. Không kéo private với comment rủi ro cần moderation

Một số comment rủi ro có thể cần hide/moderation/handoff thay vì kéo private, ví dụ:

Chửi thề, xúc phạm nặng.

Tố cáo kiểu “lừa đảo”, “bị gạt”.

Nội dung pháp lý nguy cơ cao.

Spam độc hại.

Nội dung gây hỗn loạn live.

Rule cụ thể sẽ được khóa trong module moderation ở PHẦN 2/4.

## 10. TYPING INDICATOR / RESPONSE PACING / NO-INSTANT-BOT-REPLY RULE

## 10.1. Mục tiêu

Typing Indicator / Response Pacing là rule bắt buộc trong PACK-06.

Mục tiêu là tạo nhịp hội thoại tự nhiên, giảm cảm giác máy móc, tránh tình trạng khách vừa gửi câu hỏi thì câu trả lời bật ra ngay lập tức.

Đây là yêu cầu bắt buộc cho các kênh có hỗ trợ typing indicator như:

Messenger.

Web chat.

Landing Page chat.

Private channel.

Channel Gateway có trạng thái đang nhập.

## 10.2. Rule khóa

Gateway không được gửi câu trả lời ngay lập tức theo kiểu chatbot bật đáp án.

Trước khi gửi response, Gateway phải:

Nhận input.

Gắn channel/session context.

Gửi input vào AI Advisor.

Chờ AI Final Response Guard pass.

Kích hoạt typing indicator / 3 dấu chấm nhấp nháy nếu channel hỗ trợ.

Áp response pacing theo độ dài và độ phức tạp câu trả lời.

Gửi response theo nhịp phù hợp.

Nếu câu dài, có thể chia thành nhiều message theo policy.

Không làm lộ dữ liệu trước khi guard pass.

Không giả mạo người thật.

## 10.3. Response pacing theo độ dài câu trả lời

Gateway phải phân loại response trước khi gửi:

Loại response

Ví dụ

Pacing direction

Câu rất ngắn

Chào, xác nhận đã nhận, câu kéo Messenger

Delay ngắn, có typing nếu channel hỗ trợ

Câu ngắn

Trả lời 1 ý, hỏi thêm 1 câu

Delay ngắn-vừa

Câu tư vấn vừa

Gợi ý sản phẩm, giải thích 2–3 ý

Delay vừa

Câu tư vấn sâu

So sánh sản phẩm, giải thích nhu cầu

Delay dài hơn hoặc chia message

Quote response

Báo giá từ QuoteSnapshot

Delay vừa, không gửi tức thì

Order Draft

Bản xác nhận đơn đủ 3 phần

Delay dài hơn, có thể chia block

Payment instruction

Hướng dẫn thanh toán

Delay vừa, rõ ràng

Complaint / handoff

Ghi nhận khiếu nại

Delay vừa, nghiêm túc, không vội

## 10.4. Typing indicator không được bypass guard

Typing indicator chỉ là trạng thái delivery.

Typing indicator không được:

Gửi nội dung chưa guard.

Gửi giá trước QuoteSnapshot.

Gửi order trước CustomerConfirmation.

Gửi payment trước Payment Confirmation.

Thay thế response chính thức.

Che giấu runtime lỗi.

Làm khách tưởng đã có người xử lý xong khi chưa có handoff.

## 10.5. Không giả mạo người thật

Rule này dùng để tạo trải nghiệm hội thoại tự nhiên, không dùng để lừa khách rằng đang nói chuyện với người thật.

Nếu khách hỏi bản chất kênh tư vấn, phải minh bạch theo public-safe wording:

“Dạ đây là trợ lý tư vấn tự động của Ginsengfood, Em hỗ trợ Mình theo thông tin chính thức của thương hiệu và sẽ chuyển người phụ trách khi cần ạ.”

Không được nói:

“Em là người thật” nếu thực tế là AI.

“Nhân viên đang gõ” nếu response do AI.

“Chuyên viên đang trả lời” nếu chưa human handoff.

## 10.6. P0 rule

P0-CH-UX-001 — No Instant Bot Reply

PACK-06 FAIL nếu:

Khách vừa gửi câu hỏi, Gateway gửi response tức thì không có typing/pacing ở kênh có hỗ trợ.

Câu trả lời dài bật ra ngay một khối.

Order Draft bật ra ngay như máy.

Quote bật ra ngay không pacing.

Gateway gửi response trước Final Response Guard.

Typing indicator làm khách hiểu sai là human đang xử lý.

Runtime lỗi nhưng Gateway vẫn nháy typing rồi gửi nội dung bịa.

## 11. RESPONSE CHUNKING / MULTI-MESSAGE DELIVERY RULE

## 11.1. Mục tiêu

Một số câu trả lời dài không nên gửi thành một khối lớn.

Gateway cần hỗ trợ chia message hợp lý, đặc biệt trong Messenger/Web.

## 11.2. Khi nào nên chia message

Có thể chia message khi:

Tư vấn nhiều ý.

So sánh nhiều sản phẩm.

Trình Order Draft có nhiều dòng.

Hướng dẫn thanh toán.

Hướng dẫn truy xuất.

Giải thích complaint/handoff.

CRM chăm sóc sau mua.

## 11.3. Khi nào không nên chia message

Không chia quá nhiều khi:

Live comment public.

Câu trả lời ngắn.

Khách cần CTA nhanh.

Nội dung có nguy cơ spam.

Kênh có giới hạn gửi nhiều message.

Platform policy hạn chế.

## 11.4. Rule khóa

Chia message không được làm sai ý nghĩa nghiệp vụ.

Không được:

Tách giá khỏi điều kiện quote/expiry.

Tách order total khỏi CTA xác nhận.

Tách payment instruction khỏi payment reference.

Tách cảnh báo privacy/handoff khỏi nội dung chính.

Gửi message 1 trước guard rồi message 2 sau guard.

Tất cả message trong một response package phải qua guard trước khi gửi.

## 12. PUBLIC-SAFE CHANNEL VIEW

## 12.1. Public-safe view là bắt buộc

Gateway không được gửi raw AI/internal data.

Gateway chỉ được gửi view phù hợp kênh:

Public comment safe view.

Messenger private safe view.

Web chat safe view.

CRM outbound safe view.

Human handoff internal view.

App review/test view.

## 12.2. Dữ liệu bị cấm public ở comment/live

Không được public:

Full address.

Phone.

Payment instruction riêng của order.

Payment proof.

Order status cá nhân.

Member tier/benefit riêng.

Diamond/referral private benefit.

Commission.

Invoice/tax info.

Product internal formula.

Supplier evidence.

QC defect/internal note.

Costing.

MISA/private data.

Internal trace.

Internal blocking reason.

Stack/log/error kỹ thuật.

## 12.3. Public-safe blocking reason

Khi bị chặn, Gateway chỉ được public lý do an toàn.

Ví dụ:

“Dạ Em đã gửi thông tin vào Messenger để tư vấn chính xác hơn ạ.”

“Dạ nội dung này Em xin chuyển bộ phận phụ trách hỗ trợ Mình ạ.”

“Dạ hiện dòng này chưa thể lên đơn, Em sẽ tư vấn Mình lựa chọn phù hợp khác trong Messenger ạ.”

Không public:

“SKU bị recall.”

“Batch lỗi QC.”

“Không pass internal trace.”

“Member resolver lỗi.”

“MISA mapping missing.”

“Payment mismatch nội bộ.”

## 13. META PAGE / BUSINESS / APP / PERMISSION BOUNDARY

## 13.1. Mục tiêu

PACK-06 phải khóa ranh giới vận hành Meta trước khi production.

Gateway chỉ được chạy trên Page/App/Business đã được cấu hình, kiểm soát và review.

## 13.2. Các đối tượng cần quản trị

PACK-06 cần quản trị:

Meta Business.

Meta App.

Page chính thức.

Page test.

Page live bán hàng.

Page phụ nếu có.

App role.

Page role.

Permission.

Webhook subscription.

Callback verification.

App mode.

Test user/test page.

Production page mapping.

App Review status.

Token/secret reference.

Permission expiry/renewal if applicable.

Không thiết kế token/secret chi tiết ở đây, nhưng phải khóa rằng secret không được hardcode.

## 13.3. Rule khóa

Gateway không được production nếu:

Page chưa xác định owner.

App chưa xác định owner.

Permission chưa approved.

Webhook chưa verified.

Page mapping chưa rõ.

Test page và production page bị nhầm.

Token/secret hardcoded.

App Review chưa pass cho permission cần dùng.

Không có smoke page/live/Messenger.

## 14. LIVE SESSION BOUNDARY

## 14.1. Mục tiêu

Live Session Boundary quản trị tình huống bán hàng qua live.

Live có nhiều rủi ro:

Nhiều comment cùng lúc.

Comment mơ hồ.

Hỏi giá public.

Chốt đơn public.

Spam.

Tố cáo/chửi bới.

Nhầm session.

Giá theo Giờ Vàng.

Tồn kho/sellable thay đổi.

AI trả lời quá nhanh gây cảm giác bot.

## 14.2. Live session context phải có

Mỗi live session cần xác định:

Page.

Live session ID/context.

Thời gian live.

Sản phẩm hero nếu scope có.

SKU được phép nói.

SKU không được nói nếu policy có.

Giá không public riêng nếu rule yêu cầu private.

Messenger handoff status.

Comment rate.

Inbox rate.

Suppression status.

Human moderation status.

Evidence/test context.

## 14.3. Rule khóa

Live comment đầu tiên chỉ dùng để kích hoạt public-safe response hoặc private handoff.

Sau khi khách vào Messenger thành công, câu tiếp theo xử lý như private.

Không được tiếp tục lặp lại public comment template trong Messenger.

## 15. MESSENGER PRIVATE FLOW BOUNDARY

## 15.1. Mục tiêu

Messenger Private Flow là nơi tư vấn sâu, quote, order draft và confirmation nếu runtime pass.

## 15.2. Messenger được phép xử lý

Messenger được phép:

Tư vấn nhu cầu.

Cá nhân hóa nếu customer identity/memory pass.

Báo giá từ QuoteSnapshot.

Hiển thị quote expiry.

Trình Order Draft đủ 3 phần.

Cho khách xác nhận đơn.

Hướng dẫn payment theo Commerce.

Trả trạng thái order/payment đúng khách.

Handoff human.

CRM care nếu policy cho phép.

## 15.3. Messenger vẫn phải guard

Messenger là private hơn public comment, nhưng không có nghĩa được gửi mọi dữ liệu.

Messenger vẫn không được:

Tự tính giá.

Public internal formula/costing/QC/MISA.

Trả order/payment sai khách.

Nói PAID chưa confirmed.

Nói claim chưa approved.

Bán SKU bị chặn.

Bỏ qua typing indicator/response pacing.

## 16. CRM / PROACTIVE MESSAGE CHANNEL BOUNDARY

## 16.1. Mục tiêu

Gateway có thể là kênh gửi CRM/proactive messaging nếu policy cho phép.

Tuy nhiên CRM outbound có rủi ro spam và privacy cao.

## 16.2. Điều kiện gửi CRM qua Gateway

Chỉ gửi CRM khi:

Khách không opt-out.

Communication preference cho phép.

Message fatigue pass.

Nội dung public-safe.

SKU gợi ý sellable nếu có bán.

Không Sale Lock/Recall.

Không quá giới hạn platform.

Không gửi dữ liệu nhạy cảm sai mục đích.

Có owner-approved CRM flow.

## 16.3. Rule khóa

Gateway không được gửi CRM nếu:

Customer opt-out.

Khách yêu cầu không nhận tin.

Khách yêu cầu không lưu thông tin và đang waiting xử lý.

SKU bị suppressed.

Runtime unavailable không xác minh được.

Message fatigue fail.

Platform policy không cho phép.

## 17. MODERATION / HIDE / BLOCK / ABUSE BOUNDARY

## 17.1. Mục tiêu

Moderation Boundary kiểm soát comment có rủi ro.

## 17.2. Nhóm comment cần xử lý đặc biệt

Các nhóm comment cần policy riêng:

Chửi thề/xúc phạm.

Tố cáo “lừa đảo”, “bị gạt”.

Đe dọa pháp lý.

Spam link.

Nội dung độc hại.

Nội dung gây rối live.

Nội dung lộ dữ liệu cá nhân của khách.

Khiếu nại chất lượng public.

Phản ứng bất lợi public.

## 17.3. Rule khóa

Gateway không được xử lý các comment rủi ro như comment bán hàng bình thường.

Tùy policy, Gateway có thể:

Hide comment.

Không kéo Messenger.

Handoff human.

Gắn moderation status.

Ghi evidence.

Không phản hồi công khai nếu làm tình huống phức tạp hơn.

Không tự tranh luận public.

Không tự kết luận đúng/sai.

## 18. NO-BYPASS RULE CHO FACEBOOK GATEWAY

## 18.1. Không bypass Product

Gateway không được:

Public sản phẩm chưa approved.

Public Product Knowledge chưa approved.

Public claim chưa approved.

Public SKU internal.

Tự tạo product link/media chưa approved.

## 18.2. Không bypass Operational

Gateway không được:

Bán SKU không sellable.

Chạy quote/order khi Sale Lock/Recall active.

Nói còn hàng khi Operational unavailable.

Public số lượng tồn kho.

Public trace khi Public Trace bị chặn.

## 18.3. Không bypass Commerce

Gateway không được:

Tự tính giá.

Tự áp member/referral benefit.

Tự tạo QuoteSnapshot.

Tự tạo Order Draft.

Tự xác nhận đơn.

Tự tạo official order.

Tự cấp order_code.

Tự xác nhận payment.

Tự tính verified revenue.

## 18.4. Không bypass AI Advisor

Gateway không được:

Gửi response chưa qua Final Response Guard.

Tự sửa response để thêm claim/giá/chốt.

Tự bỏ Sensitive Guard.

Tự bỏ Medical Safety Guard.

Tự bỏ Human Handoff.

Tự bỏ Brand Voice policy.

## 18.5. Không bypass Meta policy

Gateway không được:

Gửi message ngoài quyền platform.

Spam khách.

Dùng permission chưa approved.

Gửi CRM khi opt-out.

Dùng Page/App sai scope.

Dùng token/secret hardcoded.

Gửi tin quá nhanh như máy ở kênh hỗ trợ pacing.

## 19. P0 FACEBOOK GATEWAY RULES NỀN

## 19.1. P0-FB-001 — Public comment không được public dữ liệu private

Public comment không được hiển thị:

Giá quote riêng.

Địa chỉ.

Số điện thoại.

Order.

Payment.

Member tier.

Diamond benefit riêng.

Hóa đơn.

Dữ liệu sức khỏe riêng.

Dữ liệu nội bộ.

Fail Gate:

Nếu Gateway public dữ liệu private trên comment/live, PACK-06 FAIL.

## 19.2. P0-FB-002 — Gateway không được tự tính giá

Final price chỉ từ QuoteSnapshot.

Fail Gate:

Nếu Gateway tự tính hoặc sửa giá, PACK-06 FAIL.

## 19.3. P0-FB-003 — Gateway không được tạo order ngoài Commerce

Order chỉ do Commerce tạo sau CustomerConfirmation.

Fail Gate:

Nếu Gateway tạo order/order_code ngoài Commerce, PACK-06 FAIL.

## 19.4. P0-FB-004 — Gateway không gửi response chưa qua Final Response Guard

AI response phải pass TECH-05 Final Response Guard trước khi Gateway gửi.

Fail Gate:

Nếu Gateway gửi draft/bị chặn response, PACK-06 FAIL.

## 19.5. P0-FB-005 — Comment hỏi giá phải kéo private theo rule

Khi khách hỏi giá ở public comment/live, Gateway không public quote riêng.

Fail Gate:

Nếu Gateway public giá riêng hoặc quyền lợi riêng ở comment/live, PACK-06 FAIL.

## 19.6. P0-FB-006 — Sau Messenger handoff phải xử lý như private

Sau khi khách vào Messenger thành công, các câu tiếp theo xử lý theo private flow.

Fail Gate:

Nếu Messenger vẫn trả lặp format public comment, PACK-06 FAIL.

## 19.7. P0-FB-007 — Typing indicator / response pacing bắt buộc

Ở kênh hỗ trợ typing indicator, Gateway phải có 3 chấm/typing và pacing theo độ dài câu trả lời.

Fail Gate:

Nếu Gateway bật câu trả lời ngay lập tức như bot, PACK-06 FAIL.

## 19.8. P0-FB-008 — Không giả mạo người thật

Typing indicator dùng để tạo nhịp tự nhiên, không để lừa khách.

Fail Gate:

Nếu Gateway/AI nói hoặc ám chỉ sai rằng khách đang nói với người thật khi thực tế là AI, PACK-06 FAIL.

## 19.9. P0-FB-009 — Sale Lock / Recall / Suppression thắng Gateway

Gateway phải chặn sales response khi upstream suppression active.

Fail Gate:

Nếu Gateway vẫn chốt SKU locked/recalled, PACK-06 FAIL.

## 19.10. P0-FB-010 — CRM opt-out thắng proactive messaging

Nếu khách opt-out, Gateway không được gửi CRM chủ động.

Fail Gate:

Nếu Gateway gửi CRM cho khách opt-out, PACK-06 FAIL.

## 20. RUNTIME UNAVAILABLE RULE CHO FACEBOOK GATEWAY

## 20.1. Nguyên tắc chung

Nếu runtime quan trọng không khả dụng, Gateway phải fail-safe.

Không được:

Gửi giá bịa.

Gửi order bịa.

Gửi trace bịa.

Gửi payment status bịa.

Gửi memory sai.

Gửi public data nhạy cảm.

Dùng cache để vượt Sale Lock / Recall.

Gửi response chưa guard.

Nháy typing rồi trả nội dung không có căn cứ.

## 20.2. Runtime quan trọng

Runtime

Nếu unavailable thì Gateway phải

Channel Runtime

Không gửi dữ liệu nhạy cảm

Page/App Config

Không production delivery

Meta Permission/App Review

Không bật feature production

AI Advisor Runtime

Không gửi response tự chế

Final Response Guard

Không gửi response

Product Runtime

Không public product detail chưa xác minh

Operational Runtime

Không chốt/sellable

Commerce Runtime

Không quote/order

Quote Runtime

Không báo final price

Customer Identity Runtime

Không trả order/payment/memory

Payment Runtime

Không nói PAID

Public Trace Runtime

Không gửi trace valid

CRM Preference Runtime

Không gửi proactive

Human Handoff Runtime

Không hứa đã tạo ticket

Evidence Runtime

Không PASS/Release Ready

## 21. EVIDENCE NGUYÊN TẮC CHO PHẦN 1

## 21.1. Evidence bắt buộc cấp nguyên tắc

Evidence ID

Nội dung

Trạng thái yêu cầu

## FB-EVD-P1-001

Facebook Gateway Boundary Approval

## ACCEPTED

## FB-EVD-P1-002

Public / Private Channel Boundary Approval

## ACCEPTED

## FB-EVD-P1-003

Comment-to-Messenger Rule Approval

## ACCEPTED

## FB-EVD-P1-004

AI Final Response Guard Dependency Approval

## ACCEPTED

## FB-EVD-P1-005

Commerce Quote/Order No-bypass Approval

## ACCEPTED

## FB-EVD-P1-006

Typing Indicator / Response Pacing Approval

## ACCEPTED

## FB-EVD-P1-007

No Instant Bot Reply Approval

## ACCEPTED

## FB-EVD-P1-008

No Human Impersonation Approval

## ACCEPTED

## FB-EVD-P1-009

Public-safe Data Boundary Approval

## ACCEPTED

## FB-EVD-P1-010

Meta Page/App/Permission Boundary Approval

## ACCEPTED

## FB-EVD-P1-011

CRM Opt-out / Proactive Message Boundary Approval

## ACCEPTED

## FB-EVD-P1-012

Moderation / Abuse Boundary Approval

## ACCEPTED

## FB-EVD-P1-013

Runtime Unavailable Fail-Safe Approval

## ACCEPTED

## 21.2. Evidence chưa accepted thì không PASS

Evidence DRAFT không dùng để PASS.

Evidence SUBMITTED không dùng để PASS.

Evidence chưa owner/reviewer không dùng để PASS.

Evidence không mapping requirement không dùng để PASS.

Evidence không environment/version không dùng để PASS nếu cần release gate.

Screenshot rời không đủ làm evidence.

Lời xác nhận miệng không được dùng để PASS.

Chỉ evidence ACCEPTED mới được dùng để PASS.

## 22. SMOKE NGUYÊN TẮC CHO PHẦN 1

## 22.1. Smoke cấp nguyên tắc

Smoke ID

Nội dung smoke

Expected Result

## FB-SMK-P1-001

Live comment hỏi giá

Không public giá riêng, kéo private

## FB-SMK-P1-002

Messenger hỏi giá

Gateway dùng AI/Commerce QuoteSnapshot safe view

## FB-SMK-P1-003

Public comment có địa chỉ/số điện thoại

Không public lại dữ liệu

## FB-SMK-P1-004

AI response chưa Final Guard

Gateway không gửi

## FB-SMK-P1-005

Gateway tự sửa response thêm giá

Bị chặn

## FB-SMK-P1-006

Khách vào Messenger sau handoff

Xử lý như private conversation

## FB-SMK-P1-007

Kênh hỗ trợ typing

Có 3 chấm/typing + pacing

## FB-SMK-P1-008

Response dài

Không bật ngay một khối; pacing/chunking phù hợp

## FB-SMK-P1-009

Khách hỏi “đây có phải người thật không”

Minh bạch là trợ lý tự động

## FB-SMK-P1-010

Sale Lock active

Gateway không chốt

## FB-SMK-P1-011

CRM opt-out

Không gửi proactive

## FB-SMK-P1-012

Permission/App Review missing

Feature production bị chặn

## FB-SMK-P1-013

Runtime unavailable

Gateway fail-safe

## FB-SMK-P1-014

Comment chửi/tố cáo rủi ro

Moderation/handoff theo policy, không xử lý bán hàng thường

## 23. DONE GATE CỦA PHẦN 1/4

## 23.1. Điều kiện Done Gate

## PHẦN 1/4 chỉ được xem là DONE khi:

Vai trò Facebook Gateway đã rõ.

PACK-06 đứng đúng sau TECH-05 và trước TECH-07/08/09.

Source-of-truth Gateway đã rõ.

Boundary giữa AI Advisor và Gateway đã rõ.

Boundary giữa Commerce và Gateway đã rõ.

Public comment và private Messenger đã tách rõ.

Comment-to-Messenger rule đã khóa.

Sau Messenger handoff xử lý như private đã khóa.

Typing Indicator / 3 chấm đã khóa.

Response Pacing / No Instant Bot Reply đã khóa.

No Human Impersonation đã khóa.

Response Chunking rule đã khóa.

Public-safe Channel View đã khóa.

Meta Page/App/Permission boundary đã khóa.

Live Session boundary đã khóa.

Messenger Private Flow boundary đã khóa.

CRM/proactive boundary đã khóa.

Moderation/abuse boundary đã khóa.

No-bypass rule đã khóa.

P0 Gateway Rules nền đã rõ.

Runtime unavailable fail-safe đã rõ.

Evidence requirement đã rõ.

Smoke nguyên tắc đã rõ.

Không gọi Documentation Complete là Production Ready.

## 24. FAIL GATE CỦA PHẦN 1/4

## 24.1. Điều kiện làm PHẦN 1 fail

## PHẦN 1/4 FAIL nếu có bất kỳ điểm nào sau:

Gateway được viết như bot trả lời tự do.

Gateway gửi response chưa qua AI Final Response Guard.

Gateway tự sửa response để thêm giá/claim/chốt.

Gateway public final quote riêng trên live/comment.

Gateway public order/payment/address/member/referral data.

Gateway tự tính giá.

Gateway tự tạo order.

Gateway tự xác nhận thanh toán.

Gateway bỏ qua CustomerConfirmation.

Gateway bỏ qua Sale Lock/Recall.

Gateway dùng Product Knowledge/claim chưa approved.

Public/private channel bị nhầm.

Sau Messenger handoff vẫn trả như public comment.

Không có typing indicator ở kênh hỗ trợ.

Response bật ngay lập tức như chatbot máy móc.

Typing indicator dùng để giả mạo người thật.

CRM gửi khi khách opt-out.

Permission/App Review thiếu vẫn bật production.

Runtime unavailable nhưng Gateway suy đoán.

Evidence chưa accepted nhưng vẫn PASS.

## 25. TRẠNG THÁI SAU PHẦN 1/4

## 25.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## PACK-06 PHẦN 1/4

## WRITTEN_FOR_REVIEW

Facebook Gateway Principles

## LOCKED_DRAFT

Source-of-Truth Boundary

## LOCKED_DRAFT

Public / Private Channel Boundary

## LOCKED_DRAFT

Comment-to-Messenger Rule

## LOCKED_DRAFT

Typing Indicator / Response Pacing Rule

## LOCKED_DRAFT

No Instant Bot Reply Rule

## LOCKED_DRAFT

No Human Impersonation Rule

## LOCKED_DRAFT

Meta Page/App/Permission Boundary

## LOCKED_DRAFT

Live Session Boundary

## LOCKED_DRAFT

Messenger Private Flow Boundary

## LOCKED_DRAFT

CRM / Proactive Boundary

## LOCKED_DRAFT

Moderation Boundary

## LOCKED_DRAFT

Runtime Unavailable Rule

## DEFINED

Evidence Requirement

## DEFINED

Smoke Principle

## DEFINED

Implementation Status

waiting

Test Status

waiting

Smoke Status

waiting

Evidence Status

waiting

Release Ready

## KHÔNG

Production Ready

## KHÔNG

Go-live Approved

## KHÔNG

## 26. KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 đã khóa tư duy nền của PACK-06:

Facebook Gateway là lớp điều phối kênh, không phải AI, không phải Commerce, không phải Payment, không phải Operational, không phải nơi tự tạo sự thật nghiệp vụ.

Gateway có nhiệm vụ bảo vệ:

Đúng kênh.

Đúng page.

Đúng live session.

Đúng customer/session.

Đúng public/private boundary.

Đúng AI Final Response Guard.

Đúng QuoteSnapshot.

Đúng CustomerConfirmation.

Đúng Payment Confirmation.

Đúng Sale Lock / Recall / Suppression.

Đúng CRM opt-out.

Đúng typing indicator / response pacing.

Đúng Meta permission/App Review.

Đúng evidence/smoke/release gate.

Nguyên tắc cuối cùng của PHẦN 1/4:

Gateway không được trả lời tức thì như chatbot máy móc. Ở kênh hỗ trợ typing indicator, phải có 3 chấm/typing và response pacing phù hợp. Nhưng Gateway không được giả mạo người thật; nếu khách hỏi, phải minh bạch đây là trợ lý tư vấn tự động của thương hiệu.

## PHẦN 1/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 2/4 — FACEBOOK GATEWAY MODULE CONTRACTS

## 27. MỤC TIÊU CỦA PHẦN 2/4

## 27.1. Vai trò của Facebook Gateway Module Contracts

## PHẦN 2/4 khóa contract cho từng module trong Facebook Gateway / Channel Gateway.

Contract ở đây không phải API contract, không phải DB schema, không phải UI design và không phải code.

Contract ở đây là cam kết kỹ thuật cấp module:

Module này chịu trách nhiệm gì.

Module này không chịu trách nhiệm gì.

Module này nhận dữ liệu từ đâu.

Module này bàn giao dữ liệu cho module nào.

Điều kiện nào làm module bị block.

P0 STOP_POINTS nào bắt buộc chặn release.

Evidence nào bắt buộc.

Smoke nào bắt buộc.

Downstream nào không được bypass.

Mục tiêu là để khi chuyển sang DEV phase, đội kỹ thuật không tự suy luận sai về live comment, Messenger, quote, order, typing indicator, response pacing, moderation, CRM hoặc Meta App Review.

## 27.2. Nguyên tắc đọc PHẦN 2/4

Tất cả module contract trong PACK-06 phải tuân thủ PHẦN 1:

Gateway không phải AI.

Gateway không phải Commerce.

Gateway không tự tính giá.

Gateway không tự tạo order.

Gateway không tự xác nhận payment.

Gateway không gửi response chưa qua AI Final Response Guard.

Public comment không được public dữ liệu private.

Comment hỏi giá/mua/tư vấn sâu phải kéo private theo rule.

Messenger/private flow được xử lý sâu hơn nhưng vẫn phải guard.

Kênh có typing indicator phải có 3 chấm/typing và response pacing.

Không được trả lời bật ngay như chatbot máy móc.

Không được giả mạo người thật.

Sale Lock / Recall / Suppression thắng Gateway.

CRM opt-out thắng proactive messaging.

Runtime unavailable thì fail-safe.

## 28. DANH SÁCH MODULE CONTRACT TRONG PACK-06

Facebook Gateway trong PACK-06 gồm các module contract sau:

FB-M01 — Gateway Orchestrator.

FB-M02 — Meta Business / App / Page Registry.

FB-M03 — Permission / App Review / Mode Control.

FB-M04 — Webhook Intake Boundary.

FB-M05 — Page / Live Session Context Resolver.

FB-M06 — Channel Context Resolver.

FB-M07 — Public Comment Intake.

FB-M08 — Comment Intent / Trigger Classifier.

FB-M09 — Comment-to-Messenger / Private Reply Handoff.

FB-M10 — Messenger Private Session Runtime.

FB-M11 — Customer Identity / Channel Session Binding.

FB-M12 — AI Advisor Handoff.

FB-M13 — AI Final Response Guard Consumer.

FB-M14 — Channel Delivery Controller.

FB-M15 — Typing Indicator / Response Pacing Controller.

FB-M16 — Response Chunking / Multi-message Delivery.

FB-M17 — Quote / Order / Payment Safe Delivery.

FB-M18 — Public-safe Channel View / Data Redaction.

FB-M19 — Moderation / Abuse / Hide / Block Boundary.

FB-M20 — Rate Limit / Anti-spam / Platform Policy Guard.

FB-M21 — CRM / Proactive Messaging Boundary.

FB-M22 — Human Handoff Channel Boundary.

FB-M23 — Runtime Suppression / Blocking Propagation.

FB-M24 — Gateway Evidence / Smoke / App Review Runtime.

FB-M25 — Downstream Handoff to Ads / MC AI Live / IVR / TECH-10.

## FB-M01 — GATEWAY ORCHESTRATOR

## 29. FB-M01 — GATEWAY ORCHESTRATOR CONTRACT

## 29.1. Mục tiêu

Gateway Orchestrator là module điều phối trung tâm của Facebook Gateway.

Module này nhận event từ kênh, phân loại public/private, gọi đúng resolver, gọi AI Advisor, nhận response đã guard, điều phối typing/pacing và gửi response ra đúng kênh.

## 29.2. Scope In

Gateway Orchestrator bao gồm:

Event routing.

Page routing.

Live session routing.

Channel routing.

Public/private routing.

AI Advisor handoff.

Final Response Guard status check.

Delivery policy check.

Typing indicator trigger.

Response pacing trigger.

Moderation trigger.

Human handoff trigger.

Evidence/correlation reference nếu scope có.

## 29.3. Scope Out

Gateway Orchestrator không chịu trách nhiệm:

Tự viết nội dung tư vấn.

Tự tính giá.

Tự tạo QuoteSnapshot.

Tự tạo order.

Tự xác nhận payment.

Tự xác nhận PAID.

Tự public trace.

Tự gỡ Sale Lock / Recall.

Tự bỏ qua Meta permission.

Tự gửi response chưa qua AI guard.

## 29.4. Upstream Dependency

Upstream

Điều kiện

TECH-01 Foundation

Audit, evidence, idempotency nếu event retry

TECH-05 AI Advisor

Final response / safe view

TECH-04 Commerce

Quote/order/payment safe view qua AI

TECH-03 Operational

Suppression, Sale Lock, Recall qua AI/Commerce

Meta Page/App Config

Page/app/permission hợp lệ

Channel Policy

Public/private/delivery rules

## 29.5. Downstream Consumer

Gateway Orchestrator cung cấp cho:

Comment responder.

Messenger sender.

CRM sender.

Human handoff.

Moderation runtime.

Evidence runtime.

TECH-07 / TECH-08 / TECH-09 handoff nếu scope có.

## 29.6. P0 STOP_POINTS

Gateway Orchestrator FAIL nếu:

Gửi response chưa qua Final Response Guard.

Tự sửa response để thêm giá/claim/order.

Nhầm public/private channel.

Không gọi suppression check.

Bỏ qua typing/pacing ở kênh hỗ trợ.

Gửi event trùng tạo nhiều response gây spam.

Runtime unavailable nhưng vẫn gửi response khẳng định.

## 29.7. Evidence Required

Orchestrator flow approval.

Event routing evidence.

AI handoff evidence.

Final Guard consumption evidence.

Delivery pacing evidence.

Suppression evidence.

Retry/idempotency evidence nếu có.

## 29.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M01-SMK-001

Live comment hỏi giá

Route private, không public quote

## FB-M01-SMK-002

Messenger hỏi giá

Route AI/Commerce safe view

## FB-M01-SMK-003

AI response chưa guard

Không gửi

## FB-M01-SMK-004

Runtime unavailable

Fail-safe

## FB-M01-SMK-005

Retry event

Không gửi spam/trùng

## FB-M02 — META BUSINESS / APP / PAGE REGISTRY

## 30. FB-M02 — META BUSINESS / APP / PAGE REGISTRY CONTRACT

## 30.1. Mục tiêu

Module này quản trị danh mục Meta Business, Meta App và Page được phép vận hành.

## 30.2. Scope In

Bao gồm:

Meta Business reference.

Meta App reference.

Page chính thức.

Page test.

Page live bán hàng.

Page phụ nếu có.

Page role.

App role.

Page status.

Environment mapping: DEV / STAGING / PROD.

Owner mapping.

Technical contact.

Feature scope per page.

## 30.3. Scope Out

Module này không chịu trách nhiệm:

Tự cấp permission.

Tự bypass App Review.

Tự dùng Page chưa owner approved.

Tự trộn Page test và Page production.

Tự hardcode token/secret.

Tự bật AI auto-reply khi page chưa approved.

## 30.4. Upstream Dependency

Upstream

Điều kiện

Business Owner

Xác nhận page/app

Technical Owner

Xác nhận environment

Meta App Config

App tồn tại

Security Policy

Token/secret governance

TECH-01 Evidence

Evidence mapping

## 30.5. Downstream Consumer

Dữ liệu registry cung cấp cho:

Permission/App Review.

Webhook Intake.

Live Session Resolver.

Channel Context.

Evidence/UAT/Pilot.

## 30.6. P0 STOP_POINTS

Registry FAIL nếu:

Page production chưa owner approved.

Dùng nhầm Page test làm production.

Không có Page owner.

Không có App owner.

Token/secret hardcoded.

Page không thuộc release scope nhưng vẫn auto-reply.

## 30.7. Evidence Required

Page registry approval.

Business/App owner approval.

Environment mapping evidence.

Token/secret non-hardcode evidence.

Page release scope evidence.

## 30.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M02-SMK-001

Page chưa approved

Không bật auto-reply

## FB-M02-SMK-002

Page test

Không gửi khách production

## FB-M02-SMK-003

Page production approved

Cho phép pilot theo scope

## FB-M02-SMK-004

Token hardcoded

Fail Gate

## FB-M03 — PERMISSION / APP REVIEW / MODE CONTROL

## 31. FB-M03 — PERMISSION / APP REVIEW / MODE CONTROL CONTRACT

## 31.1. Mục tiêu

Module này kiểm soát permission, App Review và chế độ app trước khi Gateway chạy production.

## 31.2. Scope In

Bao gồm:

Required permissions.

Permission status.

App mode: development / live.

App Review status.

Webhook subscription status.

Page permission grant.

Feature flag per permission.

Permission expiry/review if applicable.

Pilot approval.

Production enablement.

## 31.3. Scope Out

Module này không chịu trách nhiệm:

Tự gửi message bằng permission chưa duyệt.

Tự dùng workaround để vượt Meta policy.

Tự bật production khi App Review chưa pass.

Tự gửi CRM ngoài policy.

Tự bỏ qua permission missing.

## 31.4. Upstream Dependency

Upstream

Điều kiện

Meta App Registry

App/page hợp lệ

Meta Permission Review

Permission approved

Security Owner

Secret/token governance

Release Owner

Production enable approval

TECH-10 Evidence

Gateway evidence

## 31.5. Downstream Consumer

Cung cấp cho:

Webhook Intake.

Message Delivery.

Comment-to-Messenger.

CRM sender.

Pilot/release gate.

## 31.6. P0 STOP_POINTS

Permission/App Review FAIL nếu:

Permission chưa approved nhưng feature bật production.

App đang dev mode nhưng dùng cho khách thật trái policy.

Webhook chưa verified.

Page chưa grant permission.

App Review evidence thiếu nhưng vẫn release.

CRM feature bật khi messaging policy chưa clear.

## 31.7. Evidence Required

Permission matrix.

App Review evidence.

App mode evidence.

Webhook subscription evidence.

Page grant evidence.

Owner release decision.

## 31.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M03-SMK-001

Permission missing

Feature bị chặn

## FB-M03-SMK-002

Webhook chưa verified

Không production

## FB-M03-SMK-003

App Review pass

Cho phép pilot theo scope

## FB-M03-SMK-004

Missing evidence

Không Release Ready

## FB-M04 — WEBHOOK INTAKE BOUNDARY

## 32. FB-M04 — WEBHOOK INTAKE BOUNDARY CONTRACT

## 32.1. Mục tiêu

Webhook Intake nhận event từ Meta nhưng không xử lý nghiệp vụ trực tiếp.

## 32.2. Scope In

Bao gồm:

Event receipt.

Event verification.

Event type classification.

Page/app validation.

Duplicate event detection.

Event timestamp.

Source channel.

Raw event reference.

Correlation ID nếu scope có.

Handoff sang Orchestrator.

## 32.3. Scope Out

Webhook Intake không chịu trách nhiệm:

Tự trả lời khách.

Tự gọi AI.

Tự tính giá.

Tự tạo order.

Tự xác nhận payment.

Tự hide/block comment nếu chưa qua moderation policy.

Tự public dữ liệu event.

## 32.4. Upstream Dependency

Upstream

Điều kiện

Meta App/Webhook Config

Webhook verified

Page Registry

Page hợp lệ

TECH-01 Foundation

Idempotency/audit nếu cần

Security Policy

Signature/verification policy

## 32.5. Downstream Consumer

Webhook Intake cung cấp cho:

Gateway Orchestrator.

Live Comment Intake.

Messenger Runtime.

Moderation Runtime.

Evidence logging.

## 32.6. P0 STOP_POINTS

Webhook Intake FAIL nếu:

Nhận event từ Page không thuộc registry.

Không kiểm duplicate event.

Duplicate event gửi nhiều response.

Webhook invalid vẫn xử lý.

Raw payload lộ ra response.

Event không rõ kênh vẫn trả private data.

## 32.7. Evidence Required

Webhook verification evidence.

Event type mapping evidence.

Duplicate handling evidence.

Page validation evidence.

Security verification evidence.

## 32.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M04-SMK-001

Valid comment event

Handoff Orchestrator

## FB-M04-SMK-002

Duplicate event

Không gửi response trùng

## FB-M04-SMK-003

Unknown page event

Block

## FB-M04-SMK-004

Invalid webhook

Reject/fail-safe

## FB-M05 — PAGE / LIVE SESSION CONTEXT RESOLVER

## 33. FB-M05 — PAGE / LIVE SESSION CONTEXT RESOLVER CONTRACT

## 33.1. Mục tiêu

Module này xác định Page, live session và ngữ cảnh live để Gateway xử lý đúng.

## 33.2. Scope In

Bao gồm:

Page ID/name.

Page role/scope.

Live session context.

Live status.

Hero SKU nếu scope có.

SKU allowed/disallowed nếu policy có.

Live time window.

Campaign/session context nếu có.

Comment volume/rate context.

Suppression status.

Human moderator status.

## 33.3. Scope Out

Module này không chịu trách nhiệm:

Tự quyết định giá live.

Tự xác định SKU sellable.

Tự viết script live.

Tự override AI response.

Tự xử lý comment abuse ngoài moderation policy.

Tự chạy ads.

## 33.4. Upstream Dependency

Upstream

Điều kiện

Page Registry

Page hợp lệ

Live Session Registry

Live context nếu có

Product Scope

SKU allowed nếu scope có

Commerce/Operational

Sellable/suppression nếu cần

MC AI Live Policy

Nếu live script scope có

## 33.5. Downstream Consumer

Cung cấp cho:

Comment Intake.

Intent Classifier.

AI Advisor Handoff.

Live Response Boundary.

Moderation.

MC AI Live handoff.

## 33.6. P0 STOP_POINTS

Live Context FAIL nếu:

Nhầm Page/live session.

Live đã kết thúc nhưng vẫn dùng context cũ.

Hero SKU bị Sale Lock nhưng live vẫn chốt.

SKU không allowed nhưng AI/Gateway vẫn nói.

Live context unavailable nhưng Gateway public giá/chốt.

## 33.7. Evidence Required

Live session mapping evidence.

Page-live ownership evidence.

SKU allowed/suppressed evidence.

Live smoke evidence.

Moderator handoff evidence nếu có.

## 33.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M05-SMK-001

Live active

Resolve đúng live context

## FB-M05-SMK-002

Live ended

Không dùng context cũ để chốt

## FB-M05-SMK-003

Hero SKU locked

Suppression

## FB-M05-SMK-004

Wrong page

Block

## FB-M06 — CHANNEL CONTEXT RESOLVER

## 34. FB-M06 — CHANNEL CONTEXT RESOLVER CONTRACT

## 34.1. Mục tiêu

Module này xác định event là public comment, private Messenger, CRM outbound, web/landing hoặc internal test.

## 34.2. Scope In

Bao gồm:

Public comment.

Live comment.

Messenger private.

Private reply.

Web chat nếu scope có.

Landing Page chat nếu scope có.

CRM outbound.

Internal preview/test.

Channel capability: CTA, typing, chunking, media, rate limit.

Public/private data allowance.

## 34.3. Scope Out

Không chịu trách nhiệm:

Tự tạo response.

Tự xác định intent.

Tự tính giá.

Tự gửi dữ liệu riêng.

Tự bypass privacy.

Tự bypass typing/pacing.

## 34.4. Upstream Dependency

Upstream

Điều kiện

Webhook Intake

Event type

Page/Live Context

Public/live

Messenger Runtime

Private context

Channel Policy

Allowed actions

Privacy Policy

Data boundary

## 34.5. Downstream Consumer

Cung cấp cho:

Comment Classifier.

AI Advisor Handoff.

Channel Delivery.

Sensitive Guard.

Typing/Pacing Controller.

CRM Boundary.

## 34.6. P0 STOP_POINTS

Channel Context FAIL nếu:

Public comment bị xử lý như private.

Messenger bị xử lý như public comment sau handoff.

Channel không hỗ trợ CTA nhưng Gateway yêu cầu bấm CTA.

Kênh hỗ trợ typing nhưng không dùng pacing.

Channel unknown vẫn trả dữ liệu private.

## 34.7. Evidence Required

Channel mapping evidence.

Channel capability evidence.

Public/private boundary evidence.

CTA/typing capability evidence.

Negative channel test evidence.

## 34.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M06-SMK-001

Live comment

Public channel

## FB-M06-SMK-002

Messenger after handoff

Private channel

## FB-M06-SMK-003

Unknown channel

Fail-safe

## FB-M06-SMK-004

Typing-supported channel

Pacing required

## FB-M07 — PUBLIC COMMENT INTAKE

## 35. FB-M07 — PUBLIC COMMENT INTAKE CONTRACT

## 35.1. Mục tiêu

Public Comment Intake nhận comment public và chuẩn bị cho phân loại intent/moderation.

## 35.2. Scope In

Bao gồm:

Comment text.

Comment author/channel ID.

Comment timestamp.

Comment source post/live.

Basic normalization.

Emoji/symbol handling.

Attachment/media indicator nếu có.

Public privacy risk detection sơ bộ.

Handoff sang classifier/moderation.

## 35.3. Scope Out

Không chịu trách nhiệm:

Trả lời trực tiếp.

Báo giá.

Chốt đơn.

Thu thông tin nhận hàng.

Gửi Messenger nếu chưa qua rule.

Hide/block nếu chưa moderation policy.

Public lại dữ liệu cá nhân.

## 35.4. Upstream Dependency

Upstream

Điều kiện

Webhook Intake

Valid comment event

Channel Context

Public comment

Page/Live Context

Source context

Privacy Policy

Public data handling

## 35.5. Downstream Consumer

Cung cấp cho:

Comment Intent Classifier.

Moderation Boundary.

Comment-to-Messenger Handoff.

AI Advisor Handoff nếu public-safe.

## 35.6. P0 STOP_POINTS

Public Comment Intake FAIL nếu:

Public comment chứa phone/address bị Gateway lặp lại.

Comment hỏi giá được trả giá public ngay.

Comment mơ hồ bị bỏ qua khi policy cần kéo Messenger.

Comment abuse đi vào flow bán hàng thường.

Duplicate comment event gây spam response.

## 35.7. Evidence Required

Comment normalization evidence.

Public privacy handling evidence.

Mơ hồ/comment trigger evidence.

Duplicate comment evidence.

Moderation routing evidence.

## 35.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M07-SMK-001

Comment “giá?”

Route private

## FB-M07-SMK-002

Comment “.”

Route theo mơ hồ/private policy

## FB-M07-SMK-003

Comment có số điện thoại

Không public lại

## FB-M07-SMK-004

Duplicate comment

Không spam

## FB-M08 — COMMENT INTENT / TRIGGER CLASSIFIER

## 36. FB-M08 — COMMENT INTENT / TRIGGER CLASSIFIER CONTRACT

## 36.1. Mục tiêu

Module này phân loại comment public để quyết định trả public, kéo private, moderation hoặc handoff.

## 36.2. Scope In

Nhóm trigger cần nhận diện:

Hỏi giá.

Hỏi mua/chốt.

Hỏi tư vấn.

Hỏi sản phẩm.

Hỏi thành phần/công dụng.

Gửi số điện thoại/địa chỉ.

Comment mơ hồ/ký hiệu.

Khiếu nại.

Tố cáo/chửi bới.

Spam.

Hỏi order/payment.

Hỏi đại lý/CTV/Diamond nếu scope có.

Nội dung cần human/moderation.

## 36.3. Scope Out

Không chịu trách nhiệm:

Tự trả lời final.

Tự kéo private nếu chưa qua handoff module.

Tự hide/block nếu chưa moderation policy.

Tự phân loại quyền lợi member/referral.

Tự tính giá.

## 36.4. Upstream Dependency

Upstream

Điều kiện

Public Comment Intake

Normalized comment

Channel Context

Public context

Moderation Policy

Abuse/scam/profanity

AI Intent Taxonomy

Intent mapping

Comment-to-Messenger Policy

Private trigger

## 36.5. Downstream Consumer

Cung cấp cho:

Comment-to-Messenger Handoff.

Public Short Response.

Moderation Boundary.

Human Handoff.

AI Advisor Handoff.

## 36.6. P0 STOP_POINTS

Classifier FAIL nếu:

Hỏi giá bị trả public giá.

Mua/chốt bị tạo order public.

Comment mơ hồ không kéo private khi policy yêu cầu.

Comment chửi/tố cáo đi vào bán hàng thường.

Khiếu nại chất lượng không handoff.

Hỏi order/payment public bị trả private data.

## 36.7. Evidence Required

Intent trigger matrix.

Mơ hồ/comment symbol test.

Abuse/profanity classifier evidence.

Complaint routing evidence.

Private handoff trigger evidence.

## 36.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M08-SMK-001

“bao nhiêu”

Private quote route

## FB-M08-SMK-002

“chốt 1 hộp”

Private order route

## FB-M08-SMK-003

“ib” / emoji

Private route if policy

## FB-M08-SMK-004

“lừa đảo”

Moderation/handoff, không bán thường

## FB-M08-SMK-005

Khiếu nại

Human handoff

## FB-M09 — COMMENT-TO-MESSENGER / PRIVATE REPLY HANDOFF

## 37. FB-M09 — COMMENT-TO-MESSENGER / PRIVATE REPLY HANDOFF CONTRACT

## 37.1. Mục tiêu

Module này chuyển khách từ public comment sang Messenger/private flow khi cần tư vấn sâu, quote hoặc order.

## 37.2. Scope In

Bao gồm:

Private reply trigger.

Messenger thread/session creation or binding.

Public acknowledgement.

Private message handoff.

Handoff status.

Failure handling.

Duplicate handoff prevention.

Public-safe wording.

Evidence/correlation.

## 37.3. Scope Out

Không chịu trách nhiệm:

Tự tính giá.

Tự tạo quote.

Tự tạo order.

Tự xác nhận payment.

Public dữ liệu riêng.

Kéo private cho comment cần hide/moderation nếu policy cấm.

## 37.4. Upstream Dependency

Upstream

Điều kiện

Comment Classifier

Private trigger

Meta Permission

Private reply allowed

Channel Context

Public -> private

AI Advisor

Private welcome/response safe view

Moderation Policy

Không kéo private nếu abuse nghiêm trọng

## 37.5. Downstream Consumer

Cung cấp cho:

Messenger Private Runtime.

AI Advisor.

Customer Session Binding.

Evidence logging.

## 37.6. P0 STOP_POINTS

Private Handoff FAIL nếu:

Public comment hỏi giá nhưng không kéo private.

Public response bảo khách tự nhắn khi hệ thống có thể tự gửi private theo rule.

Private handoff tạo nhiều thread/spam.

Kéo private comment abuse nghiêm trọng trái policy.

Handoff thành công nhưng Messenger vẫn xử lý như public.

Handoff fail nhưng public nói đã gửi Messenger.

## 37.7. Evidence Required

Private handoff trigger evidence.

Meta permission evidence.

Handoff success/fail evidence.

Duplicate prevention evidence.

Public acknowledgement approval.

## 37.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M09-SMK-001

Comment hỏi giá

Private handoff

## FB-M09-SMK-002

Handoff success

Messenger private context

## FB-M09-SMK-003

Handoff fail

Không nói đã gửi

## FB-M09-SMK-004

Abuse nghiêm trọng

Moderation, không kéo private

## FB-M10 — MESSENGER PRIVATE SESSION RUNTIME

## 38. FB-M10 — MESSENGER PRIVATE SESSION RUNTIME CONTRACT

## 38.1. Mục tiêu

Module này quản trị phiên trò chuyện private trong Messenger.

## 38.2. Scope In

Bao gồm:

Messenger thread/session.

Customer-channel binding.

Conversation state.

Post-handoff private context.

AI Advisor input.

Quote/order/payment safe view delivery.

Typing/pacing support.

Human handoff support.

Session expiry if policy có.

Privacy guard.

## 38.3. Scope Out

Không chịu trách nhiệm:

Tự tư vấn không qua AI.

Tự tính giá.

Tự tạo order.

Tự xác nhận payment.

Tự dùng memory sai khách.

Public lại thông tin private.

## 38.4. Upstream Dependency

Upstream

Điều kiện

Private Handoff

Messenger thread

Customer Identity

Session/customer binding

AI Advisor

Response safe view

Commerce Runtime

Quote/order/payment safe view

Typing/Pacing Controller

Delivery pacing

## 38.5. Downstream Consumer

Cung cấp cho:

Channel Delivery.

Customer Confirmation.

Payment instruction delivery.

Human handoff.

CRM boundary if allowed.

## 38.6. P0 STOP_POINTS

Messenger Runtime FAIL nếu:

Sai session/customer.

Sau handoff vẫn dùng public template.

Báo giá không QuoteSnapshot.

Tạo order không CustomerConfirmation.

Nói PAID không Payment Confirmation.

Không typing/pacing ở kênh hỗ trợ.

Lộ private data sai khách.

## 38.7. Evidence Required

Session binding evidence.

Private context evidence.

Quote/order delivery evidence.

Typing/pacing evidence.

Privacy evidence.

## 38.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M10-SMK-001

Messenger sau comment handoff

Private context

## FB-M10-SMK-002

Messenger hỏi giá

QuoteSnapshot safe view

## FB-M10-SMK-003

Sai session

Không trả order/payment

## FB-M10-SMK-004

Typing-supported

Pacing active

## FB-M11 — CUSTOMER IDENTITY / CHANNEL SESSION BINDING

## 39. FB-M11 — CUSTOMER IDENTITY / CHANNEL SESSION BINDING CONTRACT

## 39.1. Mục tiêu

Module này gắn khách, channel user, page, live/comment context và Messenger session đúng với nhau.

## 39.2. Scope In

Bao gồm:

Channel user ID.

Page ID.

Thread/session ID.

Live/comment source.

Customer identity nếu resolve được.

New/existing customer.

Member identity nếu runtime pass.

Referral context nếu có.

Session continuity.

Cross-channel binding nếu policy cho phép.

## 39.3. Scope Out

Không chịu trách nhiệm:

Tự nhận diện khách bằng suy đoán.

Tự gán member tier.

Tự dùng order/payment của khách khác.

Tự public identity.

Tự lưu dữ liệu ngoài privacy policy.

## 39.4. Upstream Dependency

Upstream

Điều kiện

Channel Runtime

User/thread/page

Customer Identity Runtime

Customer match

Commerce Runtime

Quote/order session

Privacy Policy

Identity use

Referral Runtime

Referral bind nếu có

## 39.5. Downstream Consumer

Cung cấp cho:

AI Advisor.

Messenger Runtime.

Order/Payment safe response.

CRM Boundary.

Human handoff.

## 39.6. P0 STOP_POINTS

Session Binding FAIL nếu:

Sai khách.

Sai thread.

Sai page/live.

Trả order/payment sai người.

CustomerConfirmation sai session vẫn pass.

Member/referral context dùng sai người.

## 39.7. Evidence Required

Session binding evidence.

Customer identity evidence.

Cross-channel matching evidence nếu có.

Payment/order session evidence.

Privacy evidence.

## 39.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M11-SMK-001

Comment -> Messenger

Giữ đúng user/session

## FB-M11-SMK-002

Khách khác hỏi order

Bị chặn

## FB-M11-SMK-003

Member unknown

Không nói tier

## FB-M11-SMK-004

Referral context

Chỉ dùng nếu bind pass

## FB-M12 — AI ADVISOR HANDOFF

## 40. FB-M12 — AI ADVISOR HANDOFF CONTRACT

## 40.1. Mục tiêu

Module này gửi input đã có context vào AI Advisor và nhận lại response package.

## 40.2. Scope In

Bao gồm:

Channel context.

Customer/session context.

Page/live context.

Comment/Messenger input.

Intent hints nếu có.

Moderation hints nếu có.

Commerce context nếu có.

Suppression context.

AI response package.

Guard status.

Handoff reason nếu có.

## 40.3. Scope Out

Không chịu trách nhiệm:

Tự viết response thay AI.

Tự bỏ guard.

Tự sửa AI output để thêm giá/claim/chốt.

Tự retry vô hạn gây spam.

Tự ignore AI bị chặn/handoff status.

## 40.4. Upstream Dependency

Upstream

Điều kiện

Channel Context

Public/private

Customer Session

Customer binding

Intent Classifier

Intent hint

TECH-05 AI Advisor

Response engine

Suppression Runtime

Blocking status

## 40.5. Downstream Consumer

Cung cấp cho:

Final Response Guard Consumer.

Delivery Controller.

Human Handoff.

Evidence runtime.

## 40.6. P0 STOP_POINTS

AI Handoff FAIL nếu:

Gateway gửi thiếu channel context khiến AI public sai.

Gateway gửi thiếu customer/session khi hỏi order/payment.

Gateway dùng AI draft chưa guard.

Gateway tự fallback bằng template không guard.

AI bị chặn nhưng Gateway vẫn gửi.

## 40.7. Evidence Required

AI handoff payload/context evidence.

Response package evidence.

Guard status evidence.

bị chặn/handoff handling evidence.

Retry evidence.

## 40.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M12-SMK-001

Public comment -> AI

Có public context

## FB-M12-SMK-002

Messenger order status

Có customer/session context

## FB-M12-SMK-003

AI bị chặn

Gateway không gửi

## FB-M12-SMK-004

Missing context

Fail-safe

## FB-M13 — AI FINAL RESPONSE GUARD CONSUMER

## 41. FB-M13 — AI FINAL RESPONSE GUARD CONSUMER CONTRACT

## 41.1. Mục tiêu

Module này chỉ cho phép Gateway gửi response khi AI Final Response Guard pass.

## 41.2. Scope In

Bao gồm:

Guard status check.

Guard pass/fail.

Rewrite required status.

bị chặn status.

Handoff required status.

Public-safe view selection.

Delivery allowed/bị chặn decision.

Evidence reference.

## 41.3. Scope Out

Không chịu trách nhiệm:

Tự approve response.

Tự rewrite claim.

Tự remove sensitive fields ngoài approved redaction flow.

Tự gửi response bị chặn.

Tự override human handoff.

## 41.4. Upstream Dependency

Upstream

Điều kiện

TECH-05 Final Response Guard

Guard result

Sensitive Guard

Privacy pass

Claim Guard

Claim pass

Commerce/Operational

Sellable/quote/order pass

Channel Context

Public/private view

## 41.5. Downstream Consumer

Cung cấp cho:

Channel Delivery.

Typing/Pacing.

Response Chunking.

Human Handoff.

Evidence runtime.

## 41.6. P0 STOP_POINTS

Final Guard Consumer FAIL nếu:

Guard fail nhưng gửi response.

Handoff required nhưng Gateway tự trả lời.

Sensitive detected nhưng gửi public.

Quote missing nhưng gửi giá.

Claim bị chặn nhưng gửi.

## 41.7. Evidence Required

Guard status evidence.

Response allowed/bị chặn evidence.

Negative guard test.

Handoff status evidence.

Channel-safe view evidence.

## 41.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M13-SMK-001

Guard pass

Cho delivery

## FB-M13-SMK-002

Guard fail

Không gửi

## FB-M13-SMK-003

Rewrite required

## FB-M13-SMK-004

Handoff required

Route handoff

## FB-M14 — CHANNEL DELIVERY CONTROLLER

## 42. FB-M14 — CHANNEL DELIVERY CONTROLLER CONTRACT

## 42.1. Mục tiêu

Module này gửi response đã approved ra đúng kênh với đúng delivery policy.

## 42.2. Scope In

Bao gồm:

Delivery channel selection.

Comment reply.

Private message.

CRM outbound.

Human handoff acknowledgement.

Delivery eligibility.

Delivery timing.

Delivery retry.

Delivery failure handling.

Duplicate response prevention.

Correlation/evidence.

## 42.3. Scope Out

Không chịu trách nhiệm:

Tự tạo nội dung.

Tự bỏ guard.

Tự tính giá.

Tự xác nhận order/payment.

Tự spam khi retry.

Tự gửi CRM ngoài policy.

## 42.4. Upstream Dependency

Upstream

Điều kiện

Final Response Guard Consumer

Response allowed

Channel Context

Kênh gửi

Typing/Pacing Controller

Pacing

Rate Limit Guard

Platform policy

CRM Policy

Outbound eligibility

## 42.5. Downstream Consumer

Cung cấp cho:

Meta comment/Messenger.

CRM outbound.

Evidence runtime.

Human handoff notification.

## 42.6. P0 STOP_POINTS

Delivery Controller FAIL nếu:

Gửi sai kênh.

Gửi public response vào private hoặc ngược lại sai context.

Gửi trùng do retry.

Gửi quá nhanh không pacing.

Gửi CRM khi opt-out.

Gửi response khi rate limit/policy block.

## 42.7. Evidence Required

Delivery mapping evidence.

Delivery success/fail evidence.

Duplicate prevention evidence.

Rate limit evidence.

Pacing evidence.

## 42.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M14-SMK-001

Public reply allowed

Gửi đúng comment

## FB-M14-SMK-002

Private reply allowed

Gửi đúng Messenger

## FB-M14-SMK-003

Retry delivery

Không spam

## FB-M14-SMK-004

Rate limit active

Block/queue theo policy

## FB-M15 — TYPING INDICATOR / RESPONSE PACING CONTROLLER

## 43. FB-M15 — TYPING INDICATOR / RESPONSE PACING CONTROLLER CONTRACT

## 43.1. Mục tiêu

Module này kiểm soát 3 chấm/typing indicator và độ trễ trả lời theo độ dài/độ phức tạp response.

## 43.2. Scope In

Bao gồm:

Channel typing capability.

Typing indicator trigger.

Response length classification.

Response complexity classification.

Delay window policy.

Chunk pacing.

Order Draft pacing.

Quote response pacing.

Payment response pacing.

Human handoff acknowledgement pacing.

No-instant-bot-reply enforcement.

## 43.3. Scope Out

Không chịu trách nhiệm:

Tạo nội dung response.

Gửi response chưa guard.

Giả mạo người thật.

Che giấu runtime lỗi.

Delay vô hạn.

Dùng typing indicator như xác nhận human xử lý.

## 43.4. Upstream Dependency

Upstream

Điều kiện

Channel Context

Channel có hỗ trợ typing không

Final Response Guard

Response đã pass

Delivery Controller

Gửi response

Response Chunking

Nếu response dài

No Human Impersonation Policy

Minh bạch

## 43.5. Downstream Consumer

Cung cấp cho:

Messenger delivery.

Web/Landing chat delivery.

CRM nếu channel hỗ trợ.

Evidence/smoke runtime.

## 43.6. P0 STOP_POINTS

Typing/Pacing FAIL nếu:

Kênh hỗ trợ typing nhưng response bật ra ngay.

Câu dài bật ra một khối ngay.

Quote/order draft gửi tức thì.

Typing indicator bật trước guard pass rồi gửi nội dung fail.

Typing indicator làm khách tưởng đang nói với người thật.

Runtime lỗi nhưng vẫn typing rồi bịa response.

## 43.7. Evidence Required

Typing capability matrix.

Pacing policy approval.

Response length classification evidence.

No-instant-reply smoke.

No-human-impersonation evidence.

Channel timing evidence.

## 43.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M15-SMK-001

Messenger câu ngắn

Có typing + delay hợp lý

## FB-M15-SMK-002

Messenger câu dài

Pacing/chunking

## FB-M15-SMK-003

Order Draft

Không gửi tức thì

## FB-M15-SMK-004

Khách hỏi người thật không

Minh bạch là trợ lý tự động

## FB-M15-SMK-005

Guard fail

Không gửi dù đã typing

## FB-M16 — RESPONSE CHUNKING / MULTI-MESSAGE DELIVERY

## 44. FB-M16 — RESPONSE CHUNKING / MULTI-MESSAGE DELIVERY CONTRACT

## 44.1. Mục tiêu

Module này chia response dài thành nhiều message phù hợp khi kênh cho phép.

## 44.2. Scope In

Bao gồm:

Response package segmentation.

Message order.

Chunk timing.

Quote response grouping.

Order Draft grouping.

Payment instruction grouping.

Complaint/handoff grouping.

Maximum chunks policy.

Anti-spam check.

All chunks guarded before send.

## 44.3. Scope Out

Không chịu trách nhiệm:

Tự thay nội dung.

Tách sai nghiệp vụ.

Gửi chunk chưa guard.

Gửi quá nhiều message gây spam.

Tách giá khỏi expiry.

Tách order total khỏi CTA.

Tách payment reference khỏi instruction.

## 44.4. Upstream Dependency

Upstream

Điều kiện

AI Response Package

Response approved

Final Guard

All chunks safe

Channel Capability

Multi-message allowed

Typing/Pacing

Timing

Rate Limit Guard

Anti-spam

## 44.5. Downstream Consumer

Cung cấp cho:

Messenger/Web delivery.

CRM delivery nếu scope có.

Evidence runtime.

## 44.6. P0 STOP_POINTS

Chunking FAIL nếu:

Chunk 1 gửi trước guard, chunk 2 sau guard.

Tách Quote price khỏi expiry.

Tách Order Draft khỏi confirmation CTA.

Tách payment memo khỏi bank instruction.

Gửi quá nhiều message gây spam.

Public comment bị chunk nhiều gây rối live.

## 44.7. Evidence Required

Chunking policy approval.

Sample chunk evidence.

Quote/order/payment chunk smoke.

Anti-spam evidence.

Guard-all-chunks evidence.

## 44.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M16-SMK-001

Tư vấn dài

Chia hợp lý

## FB-M16-SMK-002

Quote response

Giá + expiry không tách sai

## FB-M16-SMK-003

Order Draft

CTA đi cùng đủ context

## FB-M16-SMK-004

Public comment

Không chunk dài

## FB-M17 — QUOTE / ORDER / PAYMENT SAFE DELIVERY

## 45. FB-M17 — QUOTE / ORDER / PAYMENT SAFE DELIVERY CONTRACT

## 45.1. Mục tiêu

Module này đảm bảo Gateway chỉ delivery quote/order/payment từ Commerce safe view.

## 45.2. Scope In

Bao gồm:

QuoteSnapshot delivery.

Quote expiry delivery.

Order Draft delivery.

CustomerConfirmation CTA delivery.

Official order_code delivery.

Payment instruction delivery.

Payment status delivery.

Bank transfer instruction from registry.

COD explanation.

Invoice form delivery if allowed.

Blocking reason delivery.

## 45.3. Scope Out

Không chịu trách nhiệm:

Tự tính giá.

Tự tạo quote.

Tự tạo order.

Tự xác nhận payment.

Tự gắn PAID.

Tự thêm phí COD.

Tự hardcode bank account.

Tự verified revenue.

## 45.4. Upstream Dependency

Upstream

Điều kiện

TECH-04 Commerce

Quote/order/payment safe view

TECH-05 AI Advisor

Final response

Customer Session

Đúng khách

Channel Context

Private delivery

Sensitive Guard

No leak

## 45.5. Downstream Consumer

Cung cấp cho:

Messenger/Web private flow.

Customer confirmation.

Payment instruction.

Evidence/dispute review.

## 45.6. P0 STOP_POINTS

Safe Delivery FAIL nếu:

Báo giá không QuoteSnapshot.

Quote expired vẫn gửi như active.

Order Draft thiếu 3 phần vẫn gửi confirm.

order_code gửi khi chưa official order.

Payment waiting nói PAID.

Bank account hardcoded.

Public comment nhận quote/order/payment riêng.

## 45.7. Evidence Required

Quote delivery evidence.

Order Draft delivery evidence.

CustomerConfirmation delivery evidence.

Payment delivery evidence.

Bank registry usage evidence.

Privacy evidence.

## 45.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M17-SMK-001

Messenger quote

Từ QuoteSnapshot

## FB-M17-SMK-002

Public quote

Bị chặn/kéo private

## FB-M17-SMK-003

Payment proof

Không PAID

## FB-M17-SMK-004

Bank instruction

Registry only

## FB-M18 — PUBLIC-SAFE CHANNEL VIEW / DATA REDACTION

## 46. FB-M18 — PUBLIC-SAFE CHANNEL VIEW / DATA REDACTION CONTRACT

## 46.1. Mục tiêu

Module này đảm bảo dữ liệu ra ngoài đã đúng public-safe/private-safe view.

## 46.2. Scope In

Bao gồm:

Public comment safe view.

Messenger private safe view.

CRM safe view.

Human handoff internal view.

Field redaction.

Blocking reason transformation.

Sensitive field denylist.

Internal field denylist.

Wrong-customer protection.

## 46.3. Scope Out

Không chịu trách nhiệm:

Tự approve dữ liệu nội bộ.

Tự public trace.

Tự bỏ sensitive guard.

Tự quyết định khách có quyền xem nếu identity chưa pass.

Tự sửa giá/order/payment.

## 46.4. Upstream Dependency

Upstream

Điều kiện

TECH-05 Sensitive Guard

Sensitive status

Channel Context

Public/private

Customer Identity

Right customer

Commerce/Public Trace Safe View

Data source

Privacy Policy

Exposure rules

## 46.5. Downstream Consumer

Cung cấp cho:

Delivery Controller.

CRM Sender.

Human Handoff.

Evidence runtime.

## 46.6. P0 STOP_POINTS

Data Redaction FAIL nếu:

Public full address/phone/payment/invoice.

Public member tier/benefit riêng.

Public commission/referral owner internal ID.

Public supplier/QC/costing/MISA/internal trace.

Public internal blocking reason.

Trả dữ liệu sai khách.

## 46.7. Evidence Required

Public-safe view approval.

Sensitive field denylist.

Redaction evidence.

Wrong-customer smoke.

Internal blocking reason transformation evidence.

## 46.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M18-SMK-001

Public private data

Redacted/bị chặn

## FB-M18-SMK-002

Wrong customer order

bị chặn

## FB-M18-SMK-003

Internal reason

Public-safe rewrite

## FB-M18-SMK-004

Messenger right customer

Allowed safe data

## FB-M19 — MODERATION / ABUSE / HIDE / BLOCK BOUNDARY

## 47. FB-M19 — MODERATION / ABUSE / HIDE / BLOCK BOUNDARY CONTRACT

## 47.1. Mục tiêu

Module này xử lý comment rủi ro: chửi thề, xúc phạm, spam, tố cáo, phá live, nội dung nhạy cảm.

## 47.2. Scope In

Bao gồm:

Profanity detection.

Abuse detection.

Scam accusation detection.

Spam/troll detection.

Brand attack detection.

Public complaint routing.

Hide recommendation.

Blocklist recommendation nếu policy có.

Human moderation handoff.

Evidence logging.

## 47.3. Scope Out

Không chịu trách nhiệm:

Tự chửi lại.

Tự tranh luận public.

Tự kết luận người comment là đối thủ.

Tự xử lý pháp lý.

Tự blacklist khách thật có khiếu nại hợp lệ nếu chưa review.

Tự kéo Messenger với abuse nghiêm trọng nếu policy cấm.

## 47.4. Upstream Dependency

Upstream

Điều kiện

Comment Intake

Public comment

Moderation Policy

Abuse/profanity/scam rules

Human Handoff Policy

Legal/CS route

Privacy Policy

Complaint data

Evidence Registry

Moderation evidence

## 47.5. Downstream Consumer

Cung cấp cho:

Public response suppression.

Hide/block workflow.

Human moderation.

CS/QC handoff nếu complaint thật.

Evidence/review.

## 47.6. P0 STOP_POINTS

Moderation FAIL nếu:

Comment chửi nặng vẫn được AI bán hàng.

Tố cáo “lừa đảo” bị trả lời kiểu bán hàng.

Khiếu nại thật có mã đơn/QR bị auto blacklist.

Gateway đôi co public.

Không phát hiện biến thể chửi thề phổ biến.

Không có evidence cho hide/block/handoff.

## 47.7. Evidence Required

Moderation policy approval.

Profanity/abuse classifier evidence.

Hide/block rule evidence.

Complaint exception evidence.

Human moderation handoff evidence.

## 47.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M19-SMK-001

Comment chửi thề

Hide/handoff, không bán hàng

## FB-M19-SMK-002

“lừa đảo”

Moderation/handoff

## FB-M19-SMK-003

Spam link

Block/hide theo policy

## FB-M19-SMK-004

Khiếu nại có mã đơn

CS/human route, không auto blacklist

## FB-M20 — RATE LIMIT / ANTI-SPAM / PLATFORM POLICY GUARD

## 48. FB-M20 — RATE LIMIT / ANTI-SPAM / PLATFORM POLICY GUARD CONTRACT

## 48.1. Mục tiêu

Module này đảm bảo Gateway không gửi quá nhanh, quá nhiều hoặc vi phạm chính sách nền tảng.

## 48.2. Scope In

Bao gồm:

Per-user message frequency.

Per-page message rate.

Live comment response throttling.

CRM outbound limit.

Retry limit.

Duplicate response prevention.

Platform policy check.

Quiet hours nếu scope có.

Message fatigue.

Incident flag nếu rate bất thường.

## 48.3. Scope Out

Không chịu trách nhiệm:

Tự bỏ qua suppression.

Tự gửi CRM khi opt-out.

Tự spam để tăng conversion.

Tự vượt Meta policy.

Tự retry vô hạn.

Tự gửi comment hàng loạt không kiểm soát.

## 48.4. Upstream Dependency

Upstream

Điều kiện

Channel Delivery

Send request

Meta Platform Policy

Messaging rules

CRM Policy

Outbound frequency

Comment/Live Rate

Live load

TECH-01 Audit/Evidence

Incident/evidence

## 48.5. Downstream Consumer

Cung cấp cho:

Delivery Controller.

CRM Boundary.

Live Comment Response.

Evidence runtime.

## 48.6. P0 STOP_POINTS

Rate Limit FAIL nếu:

Gateway spam comment/message.

Retry lỗi gửi nhiều lần.

CRM gửi quá dày.

Bỏ qua opt-out.

Vi phạm platform policy.

Live volume cao nhưng không throttle.

## 48.7. Evidence Required

Rate limit policy approval.

Retry policy evidence.

CRM fatigue evidence.

Live load smoke.

Platform policy compliance evidence.

## 48.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M20-SMK-001

Retry delivery

Không spam

## FB-M20-SMK-002

Live comment spike

Throttle/queue

## FB-M20-SMK-003

CRM high frequency

Block

## FB-M20-SMK-004

Opt-out

No send

## FB-M21 — CRM / PROACTIVE MESSAGING BOUNDARY

## 49. FB-M21 — CRM / PROACTIVE MESSAGING BOUNDARY CONTRACT

## 49.1. Mục tiêu

Module này kiểm soát tin nhắn chủ động qua Messenger hoặc channel tương ứng.

## 49.2. Scope In

Bao gồm:

CRM eligibility.

Opt-in/opt-out.

Message fatigue.

Repurchase eligibility.

Post-purchase care.

Seasonal suggestion.

Member message if runtime pass.

Suppression when Sale Lock/Recall.

Public-safe personalization.

CRM delivery evidence.

## 49.3. Scope Out

Không chịu trách nhiệm:

Gửi khi opt-out.

Gửi sản phẩm suppressed.

Báo giá không QuoteSnapshot.

Public private data.

Gửi quá dày.

Gửi marketing trái platform policy.

Tự xác nhận order/payment.

## 49.4. Upstream Dependency

Upstream

Điều kiện

TECH-05 CRM Boundary

CRM content/safety

Customer Preference

Opt-in/out

Commerce/Operational

Sellable/suppression

Rate Limit Guard

Frequency

Channel Policy

Outbound allowed

## 49.5. Downstream Consumer

Cung cấp cho:

Messenger delivery.

CRM analytics if scope có.

Evidence/reporting.

## 49.6. P0 STOP_POINTS

CRM Boundary FAIL nếu:

Gửi cho khách opt-out.

Upsell SKU Sale Lock/Recall.

Gửi quote không snapshot.

Gửi quá dày.

Dùng dữ liệu nhạy cảm sai mục đích.

Không có evidence CRM send.

## 49.7. Evidence Required

CRM send policy.

Opt-out evidence.

Message fatigue evidence.

Suppression evidence.

CRM delivery smoke.

## 49.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M21-SMK-001

Opt-out customer

No send

## FB-M21-SMK-002

SKU recall

Suppressed

## FB-M21-SMK-003

CRM quote

QuoteSnapshot required

## FB-M21-SMK-004

High frequency

Block

## FB-M22 — HUMAN HANDOFF CHANNEL BOUNDARY

## 50. FB-M22 — HUMAN HANDOFF CHANNEL BOUNDARY CONTRACT

## 50.1. Mục tiêu

Module này điều phối handoff từ Gateway sang người phụ trách khi AI/Gateway không được xử lý tự động.

## 50.2. Scope In

Bao gồm:

Handoff trigger.

Handoff reason.

Customer/session context.

Page/live/comment context.

Order/payment context nếu có.

Priority.

Owner/department route.

Safe acknowledgement to customer.

Handoff status.

Failure handling.

## 50.3. Scope Out

Không chịu trách nhiệm:

Tự giải quyết complaint.

Tự xác nhận refund/payment.

Tự kết luận quality/legal.

Tự nói đã tạo ticket nếu handoff fail.

Tự close handoff.

Tự public internal handoff data.

## 50.4. Upstream Dependency

Upstream

Điều kiện

TECH-05 Human Handoff

Handoff required

Moderation Runtime

Abuse/legal/comment risk

Commerce Runtime

Order/payment if relevant

QC/CS Policy

Complaint route

Privacy Policy

Data handling

## 50.5. Downstream Consumer

Cung cấp cho:

CS team.

QC/Recall team.

Finance.

Legal/Owner.

Human moderator.

Evidence runtime.

## 50.6. P0 STOP_POINTS

Human Handoff FAIL nếu:

Required handoff không tạo.

Handoff sai owner.

Handoff fail nhưng nói đã chuyển.

Public internal complaint/payment info.

AI/Gateway tiếp tục bán khi complaint/adverse event cần handoff.

Không tạm dừng CRM khi privacy request.

## 50.7. Evidence Required

Handoff trigger evidence.

Owner route evidence.

Handoff success/fail evidence.

Safe acknowledgement approval.

Complaint/payment/privacy handoff evidence.

## 50.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M22-SMK-001

Khiếu nại chất lượng

Handoff CS/QC

## FB-M22-SMK-002

Payment dispute

Handoff Finance

## FB-M22-SMK-003

Handoff fail

Safe response, không hứa

## FB-M22-SMK-004

Privacy request

Handoff + CRM pause

## FB-M23 — RUNTIME SUPPRESSION / BLOCKING PROPAGATION

## 51. FB-M23 — RUNTIME SUPPRESSION / BLOCKING PROPAGATION CONTRACT

## 51.1. Mục tiêu

Module này đảm bảo suppression từ Operational, Commerce, AI, CRM và Moderation được propagate đến Gateway delivery.

## 51.2. Scope In

Bao gồm:

Product suppressed.

SKU suppressed.

Sale Lock.

Recall.

No stock.

Commerce bị chặn.

AI response bị chặn.

Claim bị chặn.

Channel bị chặn.

CRM opt-out.

Moderation bị chặn.

Runtime unavailable.

Blocking reason public-safe/internal-safe.

## 51.3. Scope Out

Không chịu trách nhiệm:

Tự gỡ suppression.

Tự override lock.

Tự bán khi bị chặn.

Tự dùng cache cũ.

Tự public internal blocking reason.

## 51.4. Upstream Dependency

Upstream

Điều kiện

TECH-03 Operational

Sale Lock/Recall/stock

TECH-04 Commerce

Commerce suppression

TECH-05 AI Advisor

AI guard/suppression

CRM Policy

Opt-out

Moderation Policy

Abuse block

## 51.5. Downstream Consumer

Cung cấp cho:

Delivery Controller.

CRM Boundary.

Live Comment Response.

Messenger flow.

MC AI Live / Ads / IVR handoff.

## 51.6. P0 STOP_POINTS

Suppression Propagation FAIL nếu:

Sale Lock/Recall không chặn Gateway.

AI bị chặn nhưng Gateway vẫn gửi.

CRM opt-out vẫn outbound.

Moderation bị chặn vẫn public response.

Runtime unavailable vẫn assume pass.

Cache cũ vượt suppression.

## 51.7. Evidence Required

Suppression mapping evidence.

Blocking reason mapping.

Propagation smoke.

Cache invalidation/fail-safe evidence.

Downstream suppression evidence.

## 51.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M23-SMK-001

Sale Lock active

Gateway bị chặn

## FB-M23-SMK-002

AI response bị chặn

No delivery

## FB-M23-SMK-003

CRM opt-out

No outbound

## FB-M23-SMK-004

Cache old sellable

Not used to bypass

## FB-M24 — GATEWAY EVIDENCE / SMOKE / APP REVIEW RUNTIME

## 52. FB-M24 — GATEWAY EVIDENCE / SMOKE / APP REVIEW RUNTIME CONTRACT

## 52.1. Mục tiêu

Module này quản trị evidence, smoke, App Review, pilot và release readiness cho PACK-06.

## 52.2. Scope In

Bao gồm:

Page/App evidence.

Permission/App Review evidence.

Webhook evidence.

Live smoke evidence.

Messenger smoke evidence.

Typing/pacing evidence.

Comment-to-Messenger evidence.

Moderation evidence.

CRM evidence.

Human handoff evidence.

Pilot report.

Owner review.

Release handoff.

## 52.3. Scope Out

Không chịu trách nhiệm:

Tự gọi PASS khi evidence chưa accepted.

Tự release production.

Tự bỏ qua App Review.

Tự bỏ qua P0 smoke.

Tự dùng screenshot rời làm PASS.

Tự thay owner sign-off.

## 52.4. Upstream Dependency

Upstream

Điều kiện

TECH-01 Evidence Registry

Evidence status

Meta App/Page

App/Page evidence

Smoke Matrix

Test cases

Owner Review

Sign-off

TECH-10 Gateway

Release decision

## 52.5. Downstream Consumer

Cung cấp cho:

TECH-10 Completion/Evidence/Release.

Release owner.

Dev handoff.

App Review pack.

UAT/Pilot.

## 52.6. P0 STOP_POINTS

Evidence Runtime FAIL nếu:

Evidence DRAFT dùng PASS.

App Review missing nhưng production.

P0 smoke fail nhưng Release Ready.

Owner chưa sign-off nhưng Release Approved.

Không có live/Messenger smoke.

Typing/pacing smoke thiếu.

Moderation smoke thiếu.

## 52.7. Evidence Required

Smoke report.

App Review report.

Permission evidence.

Pilot evidence.

Owner review evidence.

Accepted evidence package.

## 52.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M24-SMK-001

Evidence draft

Không PASS

## FB-M24-SMK-002

App Review missing

Block production

## FB-M24-SMK-003

Smoke pass + accepted evidence

Được xét release review

## FB-M24-SMK-004

Owner missing

Không Release Approved

## FB-M25 — DOWNSTREAM HANDOFF TO ADS / MC AI LIVE / IVR / TECH-10

## 53. FB-M25 — DOWNSTREAM HANDOFF CONTRACT

## 53.1. Mục tiêu

Module này bàn giao dữ liệu an toàn từ Facebook Gateway sang Ads, MC AI Live, IVR và TECH-10 nếu scope có.

## 53.2. Scope In

Bao gồm:

Channel event safe view.

Conversation event safe view.

Comment-to-Messenger metrics.

Inbox rate if scope có.

Quote/order handoff status.

Suppression status.

Moderation status.

Live engagement safe metrics.

No unverified revenue to Ads.

IVR order eligibility if scope có.

Evidence handoff.

## 53.3. Scope Out

Không chịu trách nhiệm:

Gửi Ads verified revenue.

Cho Ads dùng quote/cart/order draft làm revenue.

Cho MC AI Live tự chốt SKU bị chặn.

Cho IVR xác nhận order chưa official/locked.

Public customer private data.

Gửi internal thread/payment details cho downstream không có quyền.

## 53.4. Upstream Dependency

Upstream

Điều kiện

Gateway Delivery

Event status

AI Advisor

Safe response/suppression

Commerce

Verified status if relevant

TECH-07 Ads

Ads data contract later

## TECH-08 MC AI

Live script/runtime later

## TECH-09 IVR

Order confirmation later

TECH-10 Evidence

Release handoff

## 53.5. Downstream Consumer

Cung cấp cho:

TECH-07 Ads Measurement.

TECH-08 MC AI Live.

## TECH-09 IVR.

TECH-10 Completion/Evidence/Release.

Owner dashboard if scope có.

## 53.6. P0 STOP_POINTS

Downstream Handoff FAIL nếu:

Ads nhận unverified revenue từ Gateway.

MC AI Live nhận SKU bị chặn để chốt.

IVR nhận order chưa official hoặc bị lock/recall.

Downstream nhận customer private data sai quyền.

Handoff thiếu suppression status.

Evidence handoff thiếu App Review/smoke.

## 53.7. Evidence Required

Downstream handoff approval.

Safe metrics evidence.

No revenue leakage evidence.

Suppression handoff evidence.

IVR eligibility evidence nếu scope có.

TECH-10 release handoff evidence.

## 53.8. Smoke Required

Smoke ID

Nội dung

Expected Result

## FB-M25-SMK-001

Ads asks revenue

Gateway không gửi unverified revenue

## FB-M25-SMK-002

MC AI asks live SKU

Suppression included

## FB-M25-SMK-003

IVR asks order

Only eligible official order

## FB-M25-SMK-004

TECH-10 evidence

Complete handoff package

## 54. MA TRẬN PHỤ THUỘC MODULE FACEBOOK GATEWAY

## 54.1. Dependency Matrix cấp module

Module

Phụ thuộc chính

Chặn downstream nếu fail

FB-M01 Gateway Orchestrator

All gateway modules

Toàn Gateway

FB-M02 Meta Registry

Business/App/Page Owner

App Review / Webhook

FB-M03 Permission/App Review

Meta permission

Production delivery

FB-M04 Webhook Intake

Webhook verification

Event processing

FB-M05 Page/Live Context

Page/Live registry

Live response

FB-M06 Channel Context

Event/channel mapping

Public/private delivery

FB-M07 Public Comment Intake

Valid comment event

Comment routing

FB-M08 Comment Classifier

Intent/moderation policy

Private handoff/moderation

FB-M09 Comment-to-Messenger

Permission + classifier

Messenger flow

FB-M10 Messenger Runtime

Private session

Quote/order/payment delivery

FB-M11 Session Binding

Customer identity

Order/payment/memory

FB-M12 AI Handoff

AI Advisor

Response generation

FB-M13 Final Guard Consumer

TECH-05 Guard

Delivery

FB-M14 Delivery Controller

Channel policy

Message sending

FB-M15 Typing/Pacing

Channel capability

Messenger/Web UX

FB-M16 Chunking

Response package

Multi-message delivery

FB-M17 Quote/Order/Payment Delivery

Commerce safe view

Checkout

FB-M18 Public-safe View

Sensitive Guard

Privacy/security

FB-M19 Moderation

Abuse policy

Public safety

FB-M20 Rate Limit

Platform policy

Delivery/CRM

FB-M21 CRM Boundary

CRM preference

Proactive messaging

FB-M22 Human Handoff

Handoff policy

CS/QC/Finance

FB-M23 Suppression

Operational/Commerce/AI

All sales delivery

FB-M24 Evidence Runtime

Evidence registry

Release

FB-M25 Downstream Handoff

Safe metrics/status

## TECH-07/08/09/10

## 55. CROSS-MODULE P0 STOP_POINTS SUMMARY

## 55.1. P0 STOP_POINTS theo chuỗi Gateway

Chuỗi

## P0 STOP_POINTS

Module bị ảnh hưởng

Meta Registry -> Permission

Page/App chưa approved

## FB-M02/03

Webhook -> Orchestrator

Event invalid/duplicate

## FB-M04/01

Channel -> Delivery

Public/private nhầm

## FB-M06/14

Comment -> Intent

Hỏi giá không kéo private

## FB-M08/09

Comment -> Moderation

Abuse vào flow bán hàng

## FB-M08/19

Private Handoff -> Messenger

Handoff thành công nhưng vẫn public template

## FB-M09/10

Messenger -> AI

Thiếu customer/session context

## FB-M10/12

AI -> Delivery

Response chưa Final Guard

## FB-M13/14

Delivery -> UX

Không typing/pacing

## FB-M15

Chunking -> Send

Chunk chưa guard hoặc tách sai nghiệp vụ

## FB-M16

Commerce -> Delivery

Quote/order/payment không safe view

## FB-M17

Privacy -> Output

Lộ private/internal data

## FB-M18

Rate Limit -> Send

Spam/retry trùng

## FB-M20

CRM -> Send

Opt-out vẫn gửi

## FB-M21

Handoff -> Human

Required handoff không tạo

## FB-M22

Suppression -> All

Sale Lock/Recall không chặn

## FB-M23

Evidence -> Release

Evidence chưa accepted vẫn PASS

## FB-M24

Downstream -> Ads/Live/IVR

Handoff thiếu suppression/safe view

## FB-M25

## 56. EVIDENCE PACKAGE CHO PHẦN 2/4

## 56.1. Evidence cấp module

Mỗi module FB-M01 -> FB-M25 phải có evidence tối thiểu:

Module owner approval.

Scope in/out approval.

Upstream/downstream mapping.

P0 STOP_POINTS mapping.

Smoke evidence.

Meta page/app/permission evidence nếu liên quan.

Public/private boundary evidence nếu module có output.

AI Final Response Guard evidence nếu module gửi response.

Typing/pacing evidence nếu module delivery private channel.

Suppression evidence nếu module liên quan bán hàng.

Moderation evidence nếu module xử lý comment rủi ro.

Owner review accepted.

## 56.2. Evidence không được dùng để PASS nếu chưa accepted

Evidence chỉ được dùng cho Completion PASS khi có trạng thái:

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

App Review chưa có chứng cứ.

Smoke chưa có expected result.

## 57. SMOKE MATRIX CHO PHẦN 2/4

## 57.1. Smoke tổng hợp module contract

Smoke ID

Module

Nội dung

Expected Result

## FB-P2-SMK-001

Orchestrator

Live comment hỏi giá

Route private

## FB-P2-SMK-002

Meta Registry

Page chưa approved

Không bật auto-reply

## FB-P2-SMK-003

Permission

App Review missing

Production bị chặn

## FB-P2-SMK-004

Webhook

Duplicate event

Không spam

## FB-P2-SMK-005

Live Context

Hero SKU locked

Suppression

## FB-P2-SMK-006

Channel

Public/private phân biệt đúng

## FB-P2-SMK-007

Comment Intake

Comment có phone/address

Không public lại

## FB-P2-SMK-008

Comment Classifier

“chốt 1 hộp”

Private order route

## FB-P2-SMK-009

Comment-to-Messenger

Handoff success

Messenger private context

## FB-P2-SMK-010

Messenger Runtime

Hỏi giá

QuoteSnapshot safe view

## FB-P2-SMK-011

Session Binding

Sai khách

Không trả order/payment

## FB-P2-SMK-012

AI Handoff

Missing channel context

Fail-safe

## FB-P2-SMK-013

Final Guard Consumer

Guard fail

Không gửi

## FB-P2-SMK-014

Delivery

Retry send

Không gửi trùng

## FB-P2-SMK-015

Typing/Pacing

Messenger response

Có typing + pacing

## FB-P2-SMK-016

Chunking

Order Draft dài

Chunk đúng, không tách sai

## FB-P2-SMK-017

Quote/Order/Payment

Bank instruction

Registry only

## FB-P2-SMK-018

Public-safe View

Internal data

Redact/block

## FB-P2-SMK-019

Moderation

Comment chửi/tố cáo

Hide/handoff theo policy

## FB-P2-SMK-020

Rate Limit

Live spike

Throttle/queue

## FB-P2-SMK-021

## CRM

Opt-out

No send

## FB-P2-SMK-022

Human Handoff

Payment dispute

Finance handoff

## FB-P2-SMK-023

Suppression

Sale Lock active

Gateway bị chặn

## FB-P2-SMK-024

Evidence

Evidence draft

Không PASS

## FB-P2-SMK-025

Downstream

Ads asks revenue

Không gửi unverified revenue

## 58. DONE GATE CỦA PHẦN 2/4

## 58.1. Điều kiện Done Gate

## PHẦN 2/4 chỉ được xem là DONE khi:

Tất cả module FB-M01 đến FB-M25 đã có contract.

Mỗi module có scope in rõ.

Mỗi module có scope out rõ.

Mỗi module có upstream dependency rõ.

Mỗi module có downstream consumer rõ.

Mỗi module có P0 STOP_POINTS rõ.

Mỗi module có evidence requirement rõ.

Mỗi module có smoke requirement rõ.

Cross-module dependency matrix đã có.

Cross-module P0 STOP_POINTS summary đã có.

Meta Page/App/Permission boundary đã khóa.

Webhook Intake boundary đã khóa.

Public Comment / Messenger boundary đã khóa.

Comment-to-Messenger handoff đã khóa.

AI Final Response Guard consumer đã khóa.

Typing Indicator / Response Pacing Controller đã khóa.

Response Chunking đã khóa.

Quote/Order/Payment Safe Delivery đã khóa.

Public-safe View / Redaction đã khóa.

Moderation / Abuse Boundary đã khóa.

Rate Limit / Anti-spam đã khóa.

CRM / Proactive Boundary đã khóa.

Human Handoff Boundary đã khóa.

Runtime Suppression propagation đã khóa.

Evidence / App Review Runtime đã khóa.

Downstream Handoff đã khóa.

Không gọi Documentation Complete là Production Ready.

## 59. FAIL GATE CỦA PHẦN 2/4

## 59.1. Điều kiện làm PHẦN 2 fail

## PHẦN 2/4 FAIL nếu có bất kỳ điểm nào sau:

Module contract thiếu scope out.

Module contract thiếu upstream/downstream.

Module contract không có P0 STOP_POINTS.

Page/App chưa approved vẫn production.

Permission/App Review missing vẫn bật feature.

Webhook duplicate gây spam.

Public/private channel bị nhầm.

Comment hỏi giá không kéo private.

Public comment hiển thị giá/order/payment riêng.

Handoff Messenger thành công nhưng vẫn xử lý như public.

Gateway gửi response chưa qua AI Final Response Guard.

Kênh hỗ trợ typing nhưng không có typing/pacing.

Response dài bật ra ngay một khối.

Gateway giả mạo người thật.

Quote/order/payment không lấy từ Commerce safe view.

Lộ dữ liệu riêng tư/nội bộ.

Abuse/tố cáo đi vào flow bán hàng thường.

Rate limit/anti-spam bị bỏ qua.

CRM gửi khi opt-out.

Human handoff bắt buộc nhưng không tạo.

Sale Lock/Recall không chặn Gateway.

Evidence DRAFT dùng PASS.

Downstream nhận unverified revenue/private data/suppression thiếu.

## 60. TRẠNG THÁI SAU PHẦN 2/4

## 60.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## PACK-06 PHẦN 2/4

## WRITTEN_FOR_REVIEW

Facebook Gateway Module Contracts

## LOCKED_DRAFT

Module Scope In/Out

## DEFINED

Upstream/Downstream Mapping

## DEFINED

P0 STOP_POINTS Mapping

## DEFINED

Module Evidence Requirement

## DEFINED

Module Smoke Requirement

## DEFINED

Cross-Module Dependency Matrix

## DEFINED

Implementation Status

waiting

Test Status

waiting

Smoke Status

waiting

Evidence Status

waiting

Release Ready

## KHÔNG

Production Ready

## KHÔNG

Go-live Approved

## KHÔNG

## 61. KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 đã khóa contract cho từng module trong Facebook Gateway / Meta Live & Messenger Runtime.

Từ đây trở đi, Facebook Gateway không được triển khai như một bot trả lời comment tự do hoặc một webhook tự động gửi tin nhắn đơn giản.

Facebook Gateway là chuỗi module liên kết chặt chẽ:

Gateway Orchestrator -> Meta Registry -> Permission/App Review -> Webhook Intake -> Page/Live Context -> Channel Context -> Public Comment Intake -> Comment Intent Classifier -> Comment-to-Messenger Handoff -> Messenger Runtime -> Customer Session Binding -> AI Advisor Handoff -> Final Response Guard Consumer -> Channel Delivery -> Typing/Pacing -> Chunking -> Quote/Order/Payment Safe Delivery -> Public-safe View -> Moderation -> Rate Limit -> CRM Boundary -> Human Handoff -> Suppression Propagation -> Evidence Runtime -> Downstream Handoff.

Mỗi module có owner, boundary, dependency, STOP_POINTS, evidence và smoke riêng.

Không module nào được tự ý thay vai trò của Product, Operational, Commerce, AI Advisor, Payment, Public Trace hoặc Human Owner.

Không downstream nào được nhận output Gateway nếu chưa có safe view, suppression status và evidence phù hợp.

## PHẦN 2/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 3/4 — GATEWAY LIFECYCLE / STATE MACHINE / COMMAND-QUERY BOUNDARY / DELIVERY-PACING-HANDOFF CONTROL

## 62. MỤC TIÊU CỦA PHẦN 3/4

## 62.1. Vai trò của PHẦN 3/4

## PHẦN 3/4 khóa vòng đời xử lý của Facebook Gateway / Meta Live & Messenger Runtime.

PHẦN 2 đã khóa module contract.

PHẦN 3 khóa tiếp:

State machine của toàn Gateway.

State model cho Meta App / Page / Permission.

State model cho Webhook Intake.

State model cho Live Comment.

State model cho Comment-to-Messenger.

State model cho Messenger Private Session.

State model cho AI Advisor Handoff.

State model cho Final Response Guard Consumer.

State model cho Typing Indicator / Response Pacing.

State model cho Response Chunking.

State model cho Quote / Order / Payment Delivery.

State model cho Public-safe View / Redaction.

State model cho Moderation.

State model cho CRM outbound.

State model cho Human Handoff.

State model cho Runtime Suppression.

Command boundary.

Query boundary.

Runtime unavailable rule.

Downstream handoff sang Ads, MC AI Live, IVR, TECH-10.

## 62.2. Nguyên tắc trọng tâm

Facebook Gateway không được vận hành như một bot webhook trả lời tức thì.

Mọi event phải đi theo lifecycle có kiểm soát:

Event -> Validate -> Resolve Page/Channel/Session -> Classify -> Route -> AI Advisor -> Final Guard -> Delivery Policy -> Typing/Pacing -> Send / Handoff / Suppress.

Không được:

Gửi response trước khi AI Final Response Guard pass.

Public dữ liệu private.

Tự tính giá.

Tự tạo order.

Tự xác nhận payment.

Tự vượt Sale Lock / Recall.

Tự gửi CRM khi opt-out.

Trả lời bật ngay như chatbot máy móc ở kênh có typing indicator.

Giả mạo người thật.

## 63. NGUYÊN TẮC STATE MACHINE CỦA FACEBOOK GATEWAY

## 63.1. Mọi trạng thái phải có owner

State

Owner đúng

## META_APP_CONFIGURED

Meta App / Gateway Config Owner

## PAGE_APPROVED

Page Registry Owner

## PERMISSION_APPROVED

App Review / Permission Owner

## WEBHOOK_VERIFIED

Gateway Technical Owner

## EVENT_VALIDATED

Webhook Intake

## CHANNEL_RESOLVED

Channel Context Resolver

## LIVE_CONTEXT_RESOLVED

Page / Live Session Resolver

## COMMENT_CLASSIFIED

Comment Classifier

## PRIVATE_HANDOFF_CREATED

Comment-to-Messenger Runtime

## MESSENGER_SESSION_BOUND

Messenger Session Runtime

## AI_RESPONSE_REQUESTED

AI Advisor Handoff

## FINAL_GUARD_PASS

TECH-05 Final Response Guard

## DELIVERY_ALLOWED

Channel Delivery Controller

## TYPING_ACTIVE

Typing/Pacing Controller

## RESPONSE_SENT

Delivery Controller

## RESPONSE_SUPPRESSED

Suppression Runtime

## HUMAN_HANDOFF_CREATED

Human Handoff Boundary

## EVIDENCE_ACCEPTED

Evidence Runtime / Owner Review

## 63.2. Không module nào được tự ghi state của module khác

Gateway không được:

Tự ghi AI Final Guard pass.

Tự ghi QuoteSnapshot available.

Tự ghi Official Order created.

Tự ghi Payment confirmed.

Tự ghi Sale Lock released.

Tự ghi Recall closed.

Tự ghi Permission approved.

Tự ghi App Review pass.

Tự ghi Evidence accepted.

Tự ghi Production Ready.

Gateway chỉ được tiêu thụ state từ owner runtime tương ứng.

## 63.3. State transition phải có trace

Các transition quan trọng phải có:

Event ID hoặc correlation reference.

Page / App / Live session.

Channel type.

Customer/channel user/session nếu có.

Previous state.

New state.

Trigger.

Actor hoặc system actor.

Runtime source.

Blocking reason nếu có.

Guard result nếu có.

Delivery status nếu có.

Timestamp.

Evidence reference nếu dùng cho release.

## 64. GATEWAY LIFECYCLE TỔNG THỂ

## 64.1. Chuỗi lifecycle chuẩn

Chuỗi Gateway Runtime chuẩn:

META_APP_CONFIGURED -> PAGE_APPROVED -> PERMISSION_APPROVED -> WEBHOOK_VERIFIED -> EVENT_RECEIVED -> EVENT_VALIDATED -> PAGE_CONTEXT_RESOLVED -> CHANNEL_CONTEXT_RESOLVED -> SESSION_BOUND -> INTENT_CLASSIFIED -> ROUTE_DECIDED -> AI_ADVISOR_REQUESTED -> AI_RESPONSE_PACKAGE_RECEIVED -> FINAL_RESPONSE_GUARD_CHECKED -> DELIVERY_POLICY_CHECKED -> SUPPRESSION_CHECKED -> TYPING_PACING_STARTED -> RESPONSE_DELIVERED / HUMAN_HANDOFF_CREATED / RESPONSE_SUPPRESSED.

## 64.2. Lifecycle có thể bị chặn bởi

Gateway lifecycle phải bị chặn nếu có:

## PAGE_NOT_APPROVED.

## APP_REVIEW_NOT_APPROVED.

## PERMISSION_MISSING.

## WEBHOOK_INVALID.

## EVENT_DUPLICATE.

## CHANNEL_UNKNOWN.

## PUBLIC_PRIVATE_BOUNDARY_RISK.

## SESSION_MISMATCH.

## AI_FINAL_GUARD_FAIL.

## RESPONSE_REWRITE_REQUIRED.

## HUMAN_HANDOFF_REQUIRED.

## SALE_LOCK_ACTIVE.

## RECALL_ACTIVE.

## CRM_OPT_OUT.

## RATE_LIMIT_ACTIVE.

## MODERATION_REQUIRED.

## RUNTIME_UNAVAILABLE.

## EVIDENCE_NOT_ACCEPTED.

## 64.3. Lifecycle không đồng nghĩa được gửi response

Event nhận được không có nghĩa được trả lời.

AI có draft không có nghĩa được gửi.

Messenger private không có nghĩa được gửi mọi dữ liệu.

Typing indicator không có nghĩa nội dung đã pass.

Response chỉ được gửi khi:

Page/App/Permission pass.

Webhook/event hợp lệ.

Channel đúng.

Customer/session đúng nếu có dữ liệu riêng.

AI Final Guard pass.

Public-safe view pass.

Suppression không active.

Rate limit pass.

Delivery policy pass.

Typing/pacing được áp dụng nếu kênh hỗ trợ.

## 65. META APP / PAGE / PERMISSION STATE MODEL

## 65.1. Mục tiêu

State model này kiểm soát điều kiện Meta App, Page và permission trước khi Gateway được vận hành thật.

## 65.2. Trạng thái chuẩn

State

Ý nghĩa

## META_CONFIG_NOT_STARTED

Chưa cấu hình

## META_BUSINESS_IDENTIFIED

Business đã xác định

## META_APP_REGISTERED

App đã đăng ký

## META_APP_OWNER_APPROVED

Owner đã duyệt App

## PAGE_REGISTERED

Page được ghi nhận

## PAGE_OWNER_APPROVED

Page được owner duyệt

## PAGE_ENVIRONMENT_MAPPED

DEV/STAGING/PROD rõ

## PERMISSION_REQUESTED

Đã yêu cầu permission

## PERMISSION_APPROVED

Permission được duyệt

APP_REVIEW_waiting

Chờ App Review

## APP_REVIEW_APPROVED

App Review approved

## APP_REVIEW_REJECTED

App Review bị từ chối

## WEBHOOK_SUBSCRIBED

Webhook subscribed

## META_PRODUCTION_ENABLED

Cho phép production theo scope

## META_CONFIG_BLOCKED

Bị chặn

## 65.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## META_CONFIG_NOT_STARTED

## META_BUSINESS_IDENTIFIED

Business xác định

## META_BUSINESS_IDENTIFIED

## META_APP_REGISTERED

App tồn tại

## META_APP_REGISTERED

## META_APP_OWNER_APPROVED

Owner duyệt

## META_APP_OWNER_APPROVED

## PAGE_REGISTERED

Page đưa vào registry

## PAGE_REGISTERED

## PAGE_OWNER_APPROVED

Owner duyệt Page

## PAGE_OWNER_APPROVED

## PAGE_ENVIRONMENT_MAPPED

Map DEV/STAGING/PROD

## PAGE_ENVIRONMENT_MAPPED

## PERMISSION_REQUESTED

Permission cần dùng được liệt kê

## PERMISSION_REQUESTED

## PERMISSION_APPROVED

Permission approved

## PERMISSION_REQUESTED

APP_REVIEW_waiting

Cần review

APP_REVIEW_waiting

## APP_REVIEW_APPROVED

App Review pass

APP_REVIEW_waiting

## APP_REVIEW_REJECTED

App Review fail

## APP_REVIEW_APPROVED

## WEBHOOK_SUBSCRIBED

Webhook subscribed

## WEBHOOK_SUBSCRIBED

## META_PRODUCTION_ENABLED

Owner release scope pass

## 65.4. Chuyển trạng thái bị cấm

Không được:

PAGE_REGISTERED -> production nếu chưa owner approved.

APP_REVIEW_waiting -> production.

PERMISSION_REQUESTED -> delivery feature nếu chưa approved.

Page test -> production customer.

App dev/test -> production customer trái scope.

Missing webhook subscription -> event processing production.

Token/secret hardcoded -> production.

## 65.5. P0 STOP_POINTS

Meta Config FAIL nếu:

App/Page chưa approved vẫn chạy production.

Permission missing vẫn bật feature.

App Review missing vẫn go-live.

Page test gửi khách thật.

Secret/token hardcoded.

Webhook chưa verified vẫn xử lý event.

## 66. WEBHOOK EVENT STATE MODEL

## 66.1. Mục tiêu

Webhook Event State Model kiểm soát event từ Meta trước khi vào Gateway Orchestrator.

## 66.2. Trạng thái chuẩn

State

Ý nghĩa

## EVENT_NOT_RECEIVED

Chưa nhận event

## EVENT_RECEIVED

Đã nhận

## EVENT_VERIFYING

Đang verify

## EVENT_VERIFIED

Event hợp lệ

## EVENT_INVALID

Event không hợp lệ

## EVENT_DUPLICATE

Event trùng

## EVENT_PAGE_UNKNOWN

Page không thuộc registry

## EVENT_TYPE_CLASSIFIED

Đã phân loại event

## EVENT_ROUTED

Đã route sang module xử lý

## EVENT_BLOCKED

Bị chặn

## EVENT_CLOSED

Đóng event

## 66.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## EVENT_NOT_RECEIVED

## EVENT_RECEIVED

Webhook nhận event

## EVENT_RECEIVED

## EVENT_VERIFYING

Bắt đầu verify

## EVENT_VERIFYING

## EVENT_VERIFIED

Signature/page/app pass

## EVENT_VERIFYING

## EVENT_INVALID

Verify fail

## EVENT_VERIFIED

## EVENT_DUPLICATE

Duplicate detected

## EVENT_VERIFIED

## EVENT_PAGE_UNKNOWN

Page không registry

## EVENT_VERIFIED

## EVENT_TYPE_CLASSIFIED

Loại event xác định

## EVENT_TYPE_CLASSIFIED

## EVENT_ROUTED

Route module đúng

## EVENT_INVALID/PAGE_UNKNOWN/DUPLICATE

## EVENT_BLOCKED

Không xử lý tiếp

## EVENT_ROUTED

## EVENT_CLOSED

Module tiếp quản

## 66.4. Chuyển trạng thái bị cấm

Không được:

EVENT_INVALID -> AI Advisor.

EVENT_DUPLICATE -> gửi thêm response.

EVENT_PAGE_UNKNOWN -> delivery production.

EVENT_RECEIVED -> response trực tiếp.

Raw event payload -> public response.

## 66.5. P0 STOP_POINTS

Webhook FAIL nếu:

Event invalid vẫn xử lý.

Duplicate event gây spam.

Page unknown vẫn gửi response.

Event không phân loại vẫn trả dữ liệu private.

Không có correlation/evidence cho event production.

## 67. CHANNEL CONTEXT STATE MODEL

## 67.1. Mục tiêu

Channel Context State Model phân biệt public comment, Messenger private, CRM outbound, internal test và kênh unknown.

## 67.2. Trạng thái chuẩn

State

Ý nghĩa

## CHANNEL_NOT_RESOLVED

Chưa xác định kênh

## CHANNEL_RESOLVING

Đang xác định

## CHANNEL_PUBLIC_COMMENT

Comment public

## CHANNEL_LIVE_COMMENT

Comment live

## CHANNEL_PRIVATE_MESSENGER

Messenger private

## CHANNEL_PRIVATE_REPLY

Private reply từ comment

## CHANNEL_CRM_OUTBOUND

CRM chủ động

## CHANNEL_INTERNAL_TEST

Test/internal

## CHANNEL_UNKNOWN

Không xác định

## CHANNEL_BLOCKED

Kênh bị chặn

## 67.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## CHANNEL_NOT_RESOLVED

## CHANNEL_RESOLVING

Event verified

## CHANNEL_RESOLVING

## CHANNEL_PUBLIC_COMMENT

Event comment public

## CHANNEL_RESOLVING

## CHANNEL_LIVE_COMMENT

Event live comment

## CHANNEL_RESOLVING

## CHANNEL_PRIVATE_MESSENGER

Messenger event

## CHANNEL_RESOLVING

## CHANNEL_PRIVATE_REPLY

Private reply flow

## CHANNEL_RESOLVING

## CHANNEL_CRM_OUTBOUND

CRM send request

## CHANNEL_RESOLVING

## CHANNEL_INTERNAL_TEST

Test context

## CHANNEL_RESOLVING

## CHANNEL_UNKNOWN

Không đủ dữ liệu

## CHANNEL_UNKNOWN

## CHANNEL_BLOCKED

Không đủ an toàn

## 67.4. Chuyển trạng thái bị cấm

Không được:

CHANNEL_PUBLIC_COMMENT -> gửi private data.

CHANNEL_LIVE_COMMENT -> public final quote riêng.

CHANNEL_UNKNOWN -> order/payment response.

CHANNEL_CRM_OUTBOUND -> send nếu opt-out.

CHANNEL_PRIVATE_MESSENGER -> dùng lại public comment template sau handoff.

## 67.5. P0 STOP_POINTS

Channel Context FAIL nếu:

Public/private bị nhầm.

Messenger sau handoff vẫn xử lý như public.

Public comment lộ dữ liệu riêng.

CRM outbound bỏ qua opt-out.

Kênh unknown vẫn delivery dữ liệu nhạy cảm.

## 68. PUBLIC COMMENT / LIVE COMMENT STATE MODEL

## 68.1. Mục tiêu

State model này kiểm soát comment public/live trước khi trả lời, kéo private hoặc moderation.

## 68.2. Trạng thái chuẩn

State

Ý nghĩa

## COMMENT_NOT_RECEIVED

Chưa có comment

## COMMENT_RECEIVED

Đã nhận comment

## COMMENT_NORMALIZED

Đã chuẩn hóa

## COMMENT_CLASSIFYING

Đang phân loại

## COMMENT_PUBLIC_SAFE_RESPONSE_ALLOWED

Có thể trả public ngắn

## COMMENT_PRIVATE_HANDOFF_REQUIRED

Cần kéo Messenger

## COMMENT_MODERATION_REQUIRED

Cần moderation

## COMMENT_HUMAN_HANDOFF_REQUIRED

Cần người phụ trách

## COMMENT_SUPPRESSED

Không trả public

## COMMENT_RESPONDED_PUBLIC

Đã trả public

## COMMENT_PRIVATE_HANDOFF_CREATED

Đã tạo handoff private

## COMMENT_CLOSED

Đóng comment

## 68.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## COMMENT_NOT_RECEIVED

## COMMENT_RECEIVED

Có comment event

## COMMENT_RECEIVED

## COMMENT_NORMALIZED

Normalize text/symbol/emoji

## COMMENT_NORMALIZED

## COMMENT_CLASSIFYING

Bắt đầu phân loại

## COMMENT_CLASSIFYING

## COMMENT_PUBLIC_SAFE_RESPONSE_ALLOWED

Hỏi thông tin public-safe

## COMMENT_CLASSIFYING

## COMMENT_PRIVATE_HANDOFF_REQUIRED

Hỏi giá/mua/tư vấn sâu

## COMMENT_CLASSIFYING

## COMMENT_MODERATION_REQUIRED

Abuse/spam/tố cáo

## COMMENT_CLASSIFYING

## COMMENT_HUMAN_HANDOFF_REQUIRED

Complaint/quality/legal

## COMMENT_PUBLIC_SAFE_RESPONSE_ALLOWED

## COMMENT_RESPONDED_PUBLIC

Response pass guard

## COMMENT_PRIVATE_HANDOFF_REQUIRED

## COMMENT_PRIVATE_HANDOFF_CREATED

Private handoff success

## COMMENT_MODERATION_REQUIRED

## COMMENT_SUPPRESSED

Hide/block/no reply theo policy

## COMMENT_RESPONDED_PUBLIC/PRIVATE_HANDOFF_CREATED/SUPPRESSED

## COMMENT_CLOSED

Hoàn tất

## 68.4. Chuyển trạng thái bị cấm

Không được:

COMMENT_PRIVATE_HANDOFF_REQUIRED -> public final quote.

COMMENT_PUBLIC_SAFE_RESPONSE_ALLOWED -> public order/payment.

COMMENT_MODERATION_REQUIRED -> flow bán hàng thường.

COMMENT_HUMAN_HANDOFF_REQUIRED -> AI tự xử lý.

COMMENT có phone/address -> lặp lại public.

COMMENT hỏi giá -> public member/referral benefit.

## 68.5. P0 STOP_POINTS

Public Comment FAIL nếu:

Public giá riêng.

Public order/payment/address.

Comment hỏi mua tạo order public.

Abuse/tố cáo không moderation.

Complaint public không handoff.

Comment mơ hồ bị trả lời sai hướng hoặc bỏ qua policy private.

## 69. COMMENT INTENT / PRIVATE HANDOFF STATE MODEL

## 69.1. Mục tiêu

State model này kiểm soát việc comment public chuyển sang Messenger/private.

## 69.2. Trạng thái chuẩn

State

Ý nghĩa

## PRIVATE_HANDOFF_NOT_STARTED

Chưa bắt đầu

## PRIVATE_TRIGGER_DETECTED

Comment cần private

## PRIVATE_HANDOFF_ELIGIBILITY_CHECKING

Kiểm permission/policy

## PRIVATE_HANDOFF_ALLOWED

Được phép gửi private

## PRIVATE_HANDOFF_BLOCKED

Không được gửi private

## PRIVATE_MESSAGE_CREATING

Đang tạo private message

## PRIVATE_MESSAGE_SENT

Đã gửi private

## PRIVATE_THREAD_BOUND

Thread/session đã bind

## PUBLIC_ACK_ALLOWED

Được public xác nhận ngắn

## PUBLIC_ACK_SENT

Đã public xác nhận

## PRIVATE_HANDOFF_FAILED

Handoff thất bại

## PRIVATE_HANDOFF_CLOSED

Đóng handoff

## 69.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PRIVATE_HANDOFF_NOT_STARTED

## PRIVATE_TRIGGER_DETECTED

Comment hỏi giá/mua/tư vấn sâu

## PRIVATE_TRIGGER_DETECTED

## PRIVATE_HANDOFF_ELIGIBILITY_CHECKING

Kiểm Meta permission/policy

## PRIVATE_HANDOFF_ELIGIBILITY_CHECKING

## PRIVATE_HANDOFF_ALLOWED

Permission pass

## PRIVATE_HANDOFF_ELIGIBILITY_CHECKING

## PRIVATE_HANDOFF_BLOCKED

Permission/policy fail

## PRIVATE_HANDOFF_ALLOWED

## PRIVATE_MESSAGE_CREATING

Tạo private message safe

## PRIVATE_MESSAGE_CREATING

## PRIVATE_MESSAGE_SENT

Send success

## PRIVATE_MESSAGE_SENT

## PRIVATE_THREAD_BOUND

Bind session

## PRIVATE_MESSAGE_SENT

## PUBLIC_ACK_ALLOWED

Có thể public ack

## PUBLIC_ACK_ALLOWED

## PUBLIC_ACK_SENT

Ack ngắn safe

## PRIVATE_MESSAGE_CREATING

## PRIVATE_HANDOFF_FAILED

Send fail

## PRIVATE_MESSAGE_SENT/PUBLIC_ACK_SENT/FAILED

## PRIVATE_HANDOFF_CLOSED

Đóng flow

## 69.4. Chuyển trạng thái bị cấm

Không được:

PRIVATE_HANDOFF_BLOCKED -> nói đã gửi Messenger.

PRIVATE_HANDOFF_FAILED -> public “đã gửi”.

PRIVATE_TRIGGER_DETECTED -> public quote.

PRIVATE_MESSAGE_SENT nhưng không bind session -> xử lý như private chắc chắn.

Abuse nghiêm trọng -> private handoff nếu moderation policy cấm.

## 69.5. P0 STOP_POINTS

Private Handoff FAIL nếu:

Comment hỏi giá không kéo private.

Handoff fail nhưng public nói đã gửi.

Handoff tạo spam nhiều tin.

Không bind session sau private message.

Handoff private bỏ qua moderation.

## 70. MESSENGER PRIVATE SESSION STATE MODEL

## 70.1. Mục tiêu

Messenger Session State Model kiểm soát private conversation sau khi khách vào Messenger.

## 70.2. Trạng thái chuẩn

State

Ý nghĩa

## MESSENGER_SESSION_NOT_STARTED

Chưa có session

## MESSENGER_SESSION_CREATED

Tạo session

## MESSENGER_SESSION_BOUND

Bind với user/page/comment

## MESSENGER_PRIVATE_CONTEXT_ACTIVE

Private context active

## MESSENGER_AI_PROCESSING

Đang xử lý AI

## MESSENGER_RESPONSE_READY

Response safe ready

## MESSENGER_TYPING_ACTIVE

Typing active

## MESSENGER_RESPONSE_SENT

Đã gửi response

## MESSENGER_HUMAN_HANDOFF

Handoff human

## MESSENGER_SESSION_EXPIRED

Session hết hiệu lực nếu policy có

## MESSENGER_SESSION_BLOCKED

Bị chặn

## MESSENGER_SESSION_CLOSED

Đóng session

## 70.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## MESSENGER_SESSION_NOT_STARTED

## MESSENGER_SESSION_CREATED

Private handoff/input

## MESSENGER_SESSION_CREATED

## MESSENGER_SESSION_BOUND

Bind customer/channel

## MESSENGER_SESSION_BOUND

## MESSENGER_PRIVATE_CONTEXT_ACTIVE

Context valid

## MESSENGER_PRIVATE_CONTEXT_ACTIVE

## MESSENGER_AI_PROCESSING

Input gửi AI

## MESSENGER_AI_PROCESSING

## MESSENGER_RESPONSE_READY

Final Guard pass

## MESSENGER_RESPONSE_READY

## MESSENGER_TYPING_ACTIVE

Pacing start

## MESSENGER_TYPING_ACTIVE

## MESSENGER_RESPONSE_SENT

Delivery pass

## MESSENGER_AI_PROCESSING

## MESSENGER_HUMAN_HANDOFF

Handoff required

## MESSENGER_PRIVATE_CONTEXT_ACTIVE

## MESSENGER_SESSION_EXPIRED

Session expiry

## MESSENGER_SESSION_BLOCKED/CLOSED

Không xử lý tiếp

Theo policy

## 70.4. Chuyển trạng thái bị cấm

Không được:

MESSENGER_SESSION_CREATED chưa bound -> trả order/payment.

MESSENGER_AI_PROCESSING -> gửi response chưa guard.

MESSENGER_RESPONSE_READY -> gửi tức thì nếu kênh typing-enabled.

MESSENGER_SESSION_EXPIRED -> dùng quote/order cũ.

Sau handoff Messenger vẫn dùng public template.

## 70.5. P0 STOP_POINTS

Messenger Session FAIL nếu:

Sai session/customer.

Không typing/pacing.

Response chưa guard vẫn gửi.

Quote/order/payment sai context.

Messenger private vẫn bị xử lý như live comment.

## 71. AI ADVISOR HANDOFF / FINAL GUARD STATE MODEL

## 71.1. Mục tiêu

State model này kiểm soát việc Gateway gửi input sang AI và chỉ delivery khi Final Guard pass.

## 71.2. Trạng thái chuẩn

State

Ý nghĩa

## AI_HANDOFF_NOT_STARTED

Chưa gửi AI

## AI_CONTEXT_PREPARED

Context đã chuẩn bị

## AI_REQUEST_SENT

Đã gửi AI

## AI_RESPONSE_RECEIVED

Nhận response package

## AI_RESPONSE_DRAFT

Response draft

## AI_FINAL_GUARD_CHECKING

Đang kiểm Final Guard

## AI_FINAL_GUARD_PASS

Final Guard pass

## AI_FINAL_GUARD_FAIL

Final Guard fail

## AI_RESPONSE_REWRITE_REQUIRED

Cần rewrite

## AI_HUMAN_HANDOFF_REQUIRED

AI yêu cầu handoff

## AI_RESPONSE_BLOCKED

Response bị chặn

## AI_HANDOFF_FAILED

Lỗi AI/handoff

## AI_HANDOFF_CLOSED

Đóng flow

## 71.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## AI_HANDOFF_NOT_STARTED

## AI_CONTEXT_PREPARED

Channel/customer/page context ready

## AI_CONTEXT_PREPARED

## AI_REQUEST_SENT

Gửi AI Advisor

## AI_REQUEST_SENT

## AI_RESPONSE_RECEIVED

Nhận response

## AI_RESPONSE_RECEIVED

## AI_RESPONSE_DRAFT

Có draft

## AI_RESPONSE_DRAFT

## AI_FINAL_GUARD_CHECKING

Guard check

## AI_FINAL_GUARD_CHECKING

## AI_FINAL_GUARD_PASS

Guard pass

## AI_FINAL_GUARD_CHECKING

## AI_FINAL_GUARD_FAIL

Guard fail

## AI_FINAL_GUARD_CHECKING

## AI_RESPONSE_REWRITE_REQUIRED

Cần rewrite

## AI_FINAL_GUARD_CHECKING

## AI_HUMAN_HANDOFF_REQUIRED

Cần human

## AI_FINAL_GUARD_FAIL

## AI_RESPONSE_BLOCKED

Không gửi

## AI_REQUEST_SENT

## AI_HANDOFF_FAILED

AI runtime lỗi

## AI_FINAL_GUARD_PASS

## AI_HANDOFF_CLOSED

Delivery tiếp quản

## 71.4. Chuyển trạng thái bị cấm

Không được:

AI_RESPONSE_DRAFT -> delivery.

AI_FINAL_GUARD_FAIL -> delivery.

AI_HUMAN_HANDOFF_REQUIRED -> Gateway tự trả lời bán hàng.

AI_HANDOFF_FAILED -> Gateway dùng template tự chế có giá/claim/order.

Context thiếu channel -> AI request production.

## 71.5. P0 STOP_POINTS

AI Handoff / Final Guard FAIL nếu:

Gateway gửi response chưa pass guard.

Missing context gây public/private sai.

AI bị chặn nhưng Gateway vẫn gửi.

Handoff required nhưng Gateway xử lý tự động.

## 72. DELIVERY STATE MODEL

## 72.1. Mục tiêu

Delivery State Model kiểm soát việc gửi message/comment sau khi response đã được phép.

## 72.2. Trạng thái chuẩn

State

Ý nghĩa

## DELIVERY_NOT_STARTED

Chưa gửi

## DELIVERY_POLICY_CHECKING

Đang kiểm policy

## DELIVERY_ALLOWED

Được phép gửi

## DELIVERY_BLOCKED

Bị chặn

## DELIVERY_RATE_LIMITED

Bị rate limit

## DELIVERY_TYPING_REQUIRED

Cần typing/pacing

## DELIVERY_QUEUED

Xếp hàng gửi

## DELIVERY_SENDING

Đang gửi

## DELIVERY_SENT

Gửi thành công

## DELIVERY_FAILED

Gửi thất bại

DELIVERY_RETRY_waiting

Chờ retry

## DELIVERY_DUPLICATE_BLOCKED

Chặn gửi trùng

## DELIVERY_CLOSED

Đóng delivery

## 72.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## DELIVERY_NOT_STARTED

## DELIVERY_POLICY_CHECKING

Final Guard pass

## DELIVERY_POLICY_CHECKING

## DELIVERY_ALLOWED

Channel/rate/privacy pass

## DELIVERY_POLICY_CHECKING

## DELIVERY_BLOCKED

Policy fail

## DELIVERY_ALLOWED

## DELIVERY_TYPING_REQUIRED

Kênh hỗ trợ typing

## DELIVERY_ALLOWED

## DELIVERY_QUEUED

Nếu queue policy

## DELIVERY_TYPING_REQUIRED

## DELIVERY_QUEUED

Pacing scheduled

## DELIVERY_QUEUED

## DELIVERY_SENDING

Đến thời điểm gửi

## DELIVERY_SENDING

## DELIVERY_SENT

Send success

## DELIVERY_SENDING

## DELIVERY_FAILED

Send fail

## DELIVERY_FAILED

DELIVERY_RETRY_waiting

Retry allowed

DELIVERY_RETRY_waiting

## DELIVERY_SENDING

Retry

DELIVERY_RETRY_waiting

## DELIVERY_DUPLICATE_BLOCKED

Duplicate/rate risk

## DELIVERY_SENT

## DELIVERY_CLOSED

Hoàn tất

## 72.4. Chuyển trạng thái bị cấm

Không được:

## DELIVERY_NOT_STARTED -> DELIVERY_SENT.

## DELIVERY_BLOCKED -> DELIVERY_SENT.

DELIVERY_RATE_LIMITED -> spam retry.

DELIVERY_TYPING_REQUIRED -> gửi ngay không pacing.

DELIVERY_FAILED -> nói đã gửi thành công.

DELIVERY_DUPLICATE_BLOCKED -> gửi lại nhiều lần.

## 72.5. P0 STOP_POINTS

Delivery FAIL nếu:

Gửi sai kênh.

Gửi trùng do retry.

Gửi khi rate limit active.

Gửi response chưa đủ typing/pacing.

Gửi thất bại nhưng log/say success.

Gửi CRM khi opt-out.

## 73. TYPING INDICATOR / RESPONSE PACING STATE MODEL

## 73.1. Mục tiêu

State model này khóa rule No Instant Bot Reply.

## 73.2. Trạng thái chuẩn

State

Ý nghĩa

## PACING_NOT_REQUIRED

Kênh không cần/không hỗ trợ

## PACING_REQUIRED

Kênh cần pacing

## RESPONSE_LENGTH_CLASSIFYING

Đang phân loại độ dài

## RESPONSE_COMPLEXITY_CLASSIFYING

Đang phân loại độ phức tạp

## TYPING_INDICATOR_READY

Có thể bật typing

## TYPING_INDICATOR_ACTIVE

Đang hiển thị 3 chấm

## PACING_DELAY_ACTIVE

Đang áp delay

## RESPONSE_CHUNKING_REQUIRED

Cần chia message

## PACING_READY_TO_SEND

Sẵn sàng gửi

## PACING_BLOCKED

Bị chặn

## PACING_CLOSED

Đóng pacing

## 73.3. Phân loại response để pacing

Loại response

State gợi ý

Câu chào / xác nhận ngắn

Short response

Câu kéo Messenger

Short response

Tư vấn 1 ý

Short-medium response

Gợi ý sản phẩm

Medium response

So sánh / tư vấn sâu

Long / complex response

QuoteSnapshot

Commerce-sensitive response

Order Draft

High-structure response

Payment instruction

Payment-sensitive response

Complaint/handoff

Sensitive response

## 73.4. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## PACING_REQUIRED

## RESPONSE_LENGTH_CLASSIFYING

Final Guard pass

## RESPONSE_LENGTH_CLASSIFYING

## RESPONSE_COMPLEXITY_CLASSIFYING

Đã phân loại độ dài

## RESPONSE_COMPLEXITY_CLASSIFYING

## TYPING_INDICATOR_READY

Pacing policy quyết định

## TYPING_INDICATOR_READY

## TYPING_INDICATOR_ACTIVE

Bật typing/3 chấm

## TYPING_INDICATOR_ACTIVE

## PACING_DELAY_ACTIVE

Delay theo policy

## PACING_DELAY_ACTIVE

## RESPONSE_CHUNKING_REQUIRED

Response dài/cấu trúc

## PACING_DELAY_ACTIVE

## PACING_READY_TO_SEND

Response ngắn/vừa

## RESPONSE_CHUNKING_REQUIRED

## PACING_READY_TO_SEND

Chunk package pass

## PACING_READY_TO_SEND

## PACING_CLOSED

Delivery tiếp quản

## 73.5. Chuyển trạng thái bị cấm

Không được:

FINAL_RESPONSE_GUARD chưa pass -> TYPING_INDICATOR_ACTIVE kèm content.

PACING_REQUIRED -> gửi ngay.

TYPING_INDICATOR_ACTIVE -> gửi nội dung fail guard.

PACING_DELAY_ACTIVE -> bịa nội dung khi runtime lỗi.

Typing indicator -> nói hoặc ngụ ý là người thật đang gõ.

Response dài -> gửi ngay một khối nếu channel hỗ trợ chunking/pacing.

## 73.6. P0 STOP_POINTS

Typing/Pacing FAIL nếu:

Messenger/Web trả lời tức thì không 3 chấm/pacing.

Quote/order draft bật ra ngay.

Câu dài bật ra một khối.

Typing indicator dùng để giả mạo human.

Guard fail nhưng vẫn gửi sau typing.

Runtime lỗi nhưng typing xong trả nội dung bịa.

## 74. RESPONSE CHUNKING STATE MODEL

## 74.1. Mục tiêu

Response Chunking State Model kiểm soát việc chia response dài thành nhiều message.

## 74.2. Trạng thái chuẩn

State

Ý nghĩa

## CHUNKING_NOT_REQUIRED

Không cần chia

## CHUNKING_REQUIRED

Cần chia

## CHUNKING_PLANNING

Đang chia cấu trúc

## CHUNK_PACKAGE_CREATED

Đã tạo gói chunk

## CHUNK_GUARD_CHECKING

Kiểm từng chunk

## CHUNK_GUARD_PASS

Toàn bộ chunk pass

## CHUNK_GUARD_FAIL

Có chunk fail

## CHUNK_SEND_SCHEDULED

Lên lịch gửi từng chunk

## CHUNK_SENDING

Đang gửi

## CHUNK_SENT

Đã gửi

## CHUNK_BLOCKED

Bị chặn

## CHUNK_CLOSED

Đóng chunking

## 74.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## CHUNKING_REQUIRED

## CHUNKING_PLANNING

Response dài/cấu trúc

## CHUNKING_PLANNING

## CHUNK_PACKAGE_CREATED

Chia hợp lý

## CHUNK_PACKAGE_CREATED

## CHUNK_GUARD_CHECKING

Kiểm từng chunk

## CHUNK_GUARD_CHECKING

## CHUNK_GUARD_PASS

Tất cả chunk pass

## CHUNK_GUARD_CHECKING

## CHUNK_GUARD_FAIL

Có chunk fail

## CHUNK_GUARD_PASS

## CHUNK_SEND_SCHEDULED

Pacing schedule

## CHUNK_SEND_SCHEDULED

## CHUNK_SENDING

Đến thời điểm gửi

## CHUNK_SENDING

## CHUNK_SENT

Gửi thành công

## CHUNK_GUARD_FAIL

## CHUNK_BLOCKED

Không gửi

## CHUNK_SENT

## CHUNK_CLOSED

Hoàn tất

## 74.4. Chuyển trạng thái bị cấm

Không được:

Gửi chunk 1 trước khi toàn bộ package guard pass.

Tách giá khỏi expiry.

Tách tổng đơn khỏi CTA xác nhận.

Tách payment reference khỏi bank instruction.

Tách cảnh báo/handoff khỏi context.

Public comment bị chia nhiều message dài gây spam.

Chunk fail vẫn gửi phần còn lại làm sai nghĩa.

## 74.5. P0 STOP_POINTS

Chunking FAIL nếu:

Chunk chưa guard vẫn gửi.

Chunk tách sai nghiệp vụ.

Order Draft bị chia mất CTA hoặc thiếu thông tin.

Payment instruction thiếu memo/reference.

Gửi quá nhiều chunk gây spam/rate limit.

## 75. QUOTE / ORDER / PAYMENT DELIVERY STATE MODEL

## 75.1. Mục tiêu

State model này đảm bảo Gateway chỉ gửi quote/order/payment từ Commerce safe view.

## 75.2. Trạng thái chuẩn

State

Ý nghĩa

## COMMERCE_DELIVERY_NOT_STARTED

Chưa delivery commerce

## QUOTE_DELIVERY_REQUESTED

Cần gửi quote

## QUOTE_SNAPSHOT_VERIFIED

QuoteSnapshot verified

## QUOTE_DELIVERY_BLOCKED

Không được gửi quote

## ORDER_DRAFT_DELIVERY_REQUESTED

Cần gửi Draft

## ORDER_DRAFT_VERIFIED

Draft đủ 3 phần

## ORDER_DRAFT_DELIVERY_BLOCKED

Draft bị chặn

## CUSTOMER_CONFIRMATION_CTA_READY

CTA/text confirm sẵn sàng

## OFFICIAL_ORDER_DELIVERY_REQUESTED

Cần gửi order_code

## OFFICIAL_ORDER_VERIFIED

Official order có order_code

## PAYMENT_INSTRUCTION_READY

Hướng dẫn thanh toán sẵn sàng

## PAYMENT_STATUS_VERIFIED

Status payment safe

## COMMERCE_DELIVERY_SENT

Đã gửi

## COMMERCE_DELIVERY_CLOSED

Đóng delivery commerce

## 75.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## COMMERCE_DELIVERY_NOT_STARTED

## QUOTE_DELIVERY_REQUESTED

Khách hỏi giá/private

## QUOTE_DELIVERY_REQUESTED

## QUOTE_SNAPSHOT_VERIFIED

Commerce snapshot active

## QUOTE_DELIVERY_REQUESTED

## QUOTE_DELIVERY_BLOCKED

Missing/expired/bị chặn

## COMMERCE_DELIVERY_NOT_STARTED

## ORDER_DRAFT_DELIVERY_REQUESTED

Khách muốn mua

## ORDER_DRAFT_DELIVERY_REQUESTED

## ORDER_DRAFT_VERIFIED

Draft đủ 3 phần

## ORDER_DRAFT_VERIFIED

## CUSTOMER_CONFIRMATION_CTA_READY

CTA/text hợp lệ

## ORDER_DRAFT_DELIVERY_REQUESTED

## ORDER_DRAFT_DELIVERY_BLOCKED

Draft incomplete/expired

## OFFICIAL_ORDER_DELIVERY_REQUESTED

## OFFICIAL_ORDER_VERIFIED

Commerce official order

## PAYMENT_INSTRUCTION_READY

## PAYMENT_STATUS_VERIFIED

Status từ Commerce/Payment

## QUOTE_SNAPSHOT_VERIFIED/ORDER_DRAFT_VERIFIED/PAYMENT_STATUS_VERIFIED

## COMMERCE_DELIVERY_SENT

Delivery pass

## COMMERCE_DELIVERY_SENT

## COMMERCE_DELIVERY_CLOSED

Hoàn tất

## 75.4. Chuyển trạng thái bị cấm

Không được:

QUOTE_DELIVERY_REQUESTED -> gửi giá nếu không QuoteSnapshot.

QUOTE_SNAPSHOT expired -> gửi như active.

ORDER_DRAFT_VERIFIED nếu thiếu 3 phần.

CUSTOMER_CONFIRMATION_CTA_READY nếu quote expired.

OFFICIAL_ORDER_VERIFIED nếu chưa Commerce official order.

PAYMENT_STATUS_VERIFIED nếu chỉ có ảnh chuyển khoản.

Payment waiting/review -> nói PAID.

Public comment -> quote/order/payment private data.

## 75.5. P0 STOP_POINTS

Commerce Delivery FAIL nếu:

Giá không từ QuoteSnapshot.

Order Draft thiếu 3 phần.

Gateway tạo CustomerConfirmation giả.

Gateway trả order_code trước official order.

Payment proof image thành PAID.

Bank account hardcoded.

Public comment lộ commerce private data.

## 76. PUBLIC-SAFE VIEW / REDACTION STATE MODEL

## 76.1. Mục tiêu

State model này kiểm soát public-safe/private-safe view trước khi delivery.

## 76.2. Trạng thái chuẩn

State

Ý nghĩa

## VIEW_NOT_SELECTED

Chưa chọn view

## VIEW_SELECTING

Đang chọn view

## PUBLIC_SAFE_VIEW_SELECTED

View public-safe

## PRIVATE_SAFE_VIEW_SELECTED

View private-safe

## INTERNAL_VIEW_SELECTED

View nội bộ

## REDACTION_CHECKING

Đang kiểm redaction

## REDACTION_REQUIRED

Cần che/loại field

## REDACTION_APPLIED

Đã redaction

## VIEW_GUARD_PASS

View pass

## VIEW_GUARD_FAIL

View fail

## VIEW_BLOCKED

Bị chặn

## 76.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## VIEW_NOT_SELECTED

## VIEW_SELECTING

Có response/data

## VIEW_SELECTING

## PUBLIC_SAFE_VIEW_SELECTED

Public comment/live

## VIEW_SELECTING

## PRIVATE_SAFE_VIEW_SELECTED

Messenger đúng khách

## VIEW_SELECTING

## INTERNAL_VIEW_SELECTED

Human/internal

## PUBLIC/PRIVATE/INTERNAL

## REDACTION_CHECKING

Kiểm field

## REDACTION_CHECKING

## REDACTION_REQUIRED

Có field không phù hợp

## REDACTION_REQUIRED

## REDACTION_APPLIED

Redact thành công

## REDACTION_CHECKING/REDACTION_APPLIED

## VIEW_GUARD_PASS

View safe

## REDACTION_CHECKING

## VIEW_GUARD_FAIL

Có leak không thể redact

## VIEW_GUARD_FAIL

## VIEW_BLOCKED

Không gửi

## 76.4. Chuyển trạng thái bị cấm

Không được:

INTERNAL_VIEW_SELECTED -> public delivery.

VIEW_GUARD_FAIL -> delivery.

REDACTION_REQUIRED -> delivery khi chưa redact.

Sai customer/session -> private safe view.

Public internal blocking reason.

Public payment/order/address/member/referral private data.

## 76.5. P0 STOP_POINTS

Public-safe View FAIL nếu:

Lộ dữ liệu riêng tư.

Lộ internal field.

Lộ payment/order sai khách.

Lộ commission/KPI/ROAS/MISA.

Lộ internal trace/QC/costing/supplier.

Redaction không áp nhưng vẫn gửi.

## 77. MODERATION STATE MODEL

## 77.1. Mục tiêu

Moderation State Model kiểm soát comment rủi ro trước khi Gateway phản hồi.

## 77.2. Trạng thái chuẩn

State

Ý nghĩa

## MODERATION_NOT_CHECKED

Chưa kiểm

## MODERATION_CHECKING

Đang kiểm

## MODERATION_PASS

Không rủi ro

## MODERATION_ABUSE_DETECTED

Chửi/xúc phạm

## MODERATION_SPAM_DETECTED

Spam

## MODERATION_SCAM_ACCUSATION

Tố cáo/lừa đảo

## MODERATION_LEGAL_RISK

Pháp lý

## MODERATION_QUALITY_COMPLAINT

Khiếu nại chất lượng

## MODERATION_HIDE_RECOMMENDED

Đề xuất hide

## MODERATION_HUMAN_REVIEW_REQUIRED

Cần human

## MODERATION_BLOCKED

Không trả tự động

## MODERATION_CLOSED

Đóng moderation

## 77.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## MODERATION_NOT_CHECKED

## MODERATION_CHECKING

Comment public

## MODERATION_CHECKING

## MODERATION_PASS

Không rủi ro

## MODERATION_CHECKING

## MODERATION_ABUSE_DETECTED

Chửi/xúc phạm

## MODERATION_CHECKING

## MODERATION_SPAM_DETECTED

Spam/link

## MODERATION_CHECKING

## MODERATION_SCAM_ACCUSATION

Tố cáo/lừa đảo

## MODERATION_CHECKING

## MODERATION_LEGAL_RISK

Pháp lý

## MODERATION_CHECKING

## MODERATION_QUALITY_COMPLAINT

Khiếu nại

## MODERATION_ABUSE/SPAM

## MODERATION_HIDE_RECOMMENDED

Policy cho hide

## MODERATION_SCAM/LEGAL/QUALITY

## MODERATION_HUMAN_REVIEW_REQUIRED

Cần human

## MODERATION_HUMAN_REVIEW_REQUIRED

## MODERATION_BLOCKED

Không auto sales

## MODERATION_PASS

## MODERATION_CLOSED

Comment flow tiếp tục

## 77.4. Chuyển trạng thái bị cấm

Không được:

MODERATION_ABUSE_DETECTED -> bán hàng thường.

MODERATION_SCAM_ACCUSATION -> auto chốt Messenger.

MODERATION_QUALITY_COMPLAINT -> trả lời marketing.

MODERATION_LEGAL_RISK -> AI tự tranh luận.

MODERATION_HIDE_RECOMMENDED -> hide không evidence nếu policy cần.

Khiếu nại thật có order/QR -> auto blacklist.

## 77.5. P0 STOP_POINTS

Moderation FAIL nếu:

Comment rủi ro vào flow bán hàng thường.

Gateway đôi co public.

Khiếu nại chất lượng không handoff.

Tố cáo “lừa đảo” trả lời như bán hàng.

Hide/block thiếu evidence/policy.

## 78. RATE LIMIT / ANTI-SPAM STATE MODEL

## 78.1. Mục tiêu

State model này kiểm soát tần suất gửi, retry và chống spam.

## 78.2. Trạng thái chuẩn

State

Ý nghĩa

## RATE_NOT_CHECKED

Chưa kiểm

## RATE_CHECKING

Đang kiểm

## RATE_PASS

Tần suất hợp lệ

## RATE_LIMIT_WARNING

Gần ngưỡng

## RATE_LIMIT_ACTIVE

Đang bị giới hạn

## MESSAGE_QUEUED

Tin nhắn xếp hàng

## MESSAGE_THROTTLED

Tin nhắn bị giảm tốc

## MESSAGE_BLOCKED_BY_RATE

Bị chặn do rate

## RETRY_ALLOWED

Cho retry

## RETRY_BLOCKED

Chặn retry

## RATE_CLOSED

Đóng kiểm rate

## 78.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## RATE_NOT_CHECKED

## RATE_CHECKING

Có delivery request

## RATE_CHECKING

## RATE_PASS

Trong ngưỡng

## RATE_CHECKING

## RATE_LIMIT_WARNING

Gần ngưỡng

## RATE_CHECKING

## RATE_LIMIT_ACTIVE

Vượt ngưỡng

## RATE_LIMIT_ACTIVE

## MESSAGE_QUEUED

Queue nếu policy cho

## RATE_LIMIT_ACTIVE

## MESSAGE_THROTTLED

Throttle

## RATE_LIMIT_ACTIVE

## MESSAGE_BLOCKED_BY_RATE

Block nếu cần

## DELIVERY_FAILED

## RETRY_ALLOWED

Retry trong policy

## DELIVERY_FAILED

## RETRY_BLOCKED

Retry vượt ngưỡng

## RATE_PASS/QUEUED/THROTTLED

## RATE_CLOSED

Hoàn tất

## 78.4. Chuyển trạng thái bị cấm

Không được:

RATE_LIMIT_ACTIVE -> gửi hàng loạt.

RETRY_BLOCKED -> retry tiếp.

CRM opt-out -> queue gửi sau.

Live spike -> spam từng comment giống nhau.

Delivery failed -> vô hạn retry.

Rate state missing -> production send.

## 78.5. P0 STOP_POINTS

Rate Limit FAIL nếu:

Gateway spam comment/message.

Retry gây gửi trùng.

CRM gửi quá dày.

Live volume cao không throttle.

Platform policy bị vượt.

Opt-out vẫn gửi.

## 79. CRM OUTBOUND STATE MODEL

## 79.1. Mục tiêu

CRM Outbound State Model kiểm soát proactive message qua Messenger/channel.

## 79.2. Trạng thái chuẩn

State

Ý nghĩa

## CRM_NOT_STARTED

Chưa CRM

## CRM_ELIGIBILITY_CHECKING

Đang kiểm eligible

## CRM_OPT_IN_CONFIRMED

Được phép gửi

## CRM_OPT_OUT_ACTIVE

Khách opt-out

## CRM_FATIGUE_PASS

Tần suất pass

## CRM_FATIGUE_BLOCKED

Tần suất fail

## CRM_CONTENT_READY

Nội dung ready

## CRM_SUPPRESSED

Bị suppression

## CRM_SEND_ALLOWED

Được gửi

## CRM_SEND_BLOCKED

Bị chặn

## CRM_SENT

Đã gửi

## CRM_CLOSED

Đóng CRM

## 79.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## CRM_NOT_STARTED

## CRM_ELIGIBILITY_CHECKING

Có CRM campaign/message

## CRM_ELIGIBILITY_CHECKING

## CRM_OPT_IN_CONFIRMED

Preference pass

## CRM_ELIGIBILITY_CHECKING

## CRM_OPT_OUT_ACTIVE

Opt-out

## CRM_OPT_IN_CONFIRMED

## CRM_FATIGUE_PASS

Frequency pass

## CRM_OPT_IN_CONFIRMED

## CRM_FATIGUE_BLOCKED

Frequency fail

## CRM_FATIGUE_PASS

## CRM_CONTENT_READY

Content safe

## CRM_CONTENT_READY

## CRM_SUPPRESSED

SKU/claim/runtime bị chặn

## CRM_CONTENT_READY

## CRM_SEND_ALLOWED

Policy pass

## CRM_SEND_ALLOWED

## CRM_SENT

Delivery pass

## CRM_OPT_OUT/FATIGUE_BLOCKED/SUPPRESSED

## CRM_SEND_BLOCKED

Không gửi

## CRM_SENT/SEND_BLOCKED

## CRM_CLOSED

Hoàn tất

## 79.4. Chuyển trạng thái bị cấm

Không được:

## CRM_OPT_OUT_ACTIVE -> CRM_SENT.

## CRM_FATIGUE_BLOCKED -> CRM_SENT.

## CRM_SUPPRESSED -> CRM_SENT.

CRM content có quote không QuoteSnapshot.

CRM upsell SKU recall/sale lock.

CRM privacy request waiting vẫn gửi.

## 79.5. P0 STOP_POINTS

CRM FAIL nếu:

Gửi cho khách opt-out.

Gửi quá dày.

Gửi SKU suppressed.

Gửi quote không snapshot.

Gửi dữ liệu nhạy cảm sai mục đích.

Không có evidence CRM delivery.

## 80. HUMAN HANDOFF STATE MODEL

## 80.1. Mục tiêu

Human Handoff State Model kiểm soát việc Gateway chuyển người phụ trách.

## 80.2. Trạng thái chuẩn

State

Ý nghĩa

## HANDOFF_NOT_REQUIRED

Không cần handoff

## HANDOFF_CHECKING

Đang kiểm

## HANDOFF_REQUIRED

Cần handoff

## HANDOFF_REASON_CLASSIFIED

Lý do đã phân loại

## HANDOFF_OWNER_RESOLVED

Owner/bộ phận đã xác định

## HANDOFF_CONTEXT_PREPARED

Context đã chuẩn bị

## HANDOFF_CREATED

Tạo handoff/ticket

## HANDOFF_ACK_TO_CUSTOMER_READY

Có câu xác nhận safe

## HANDOFF_ACK_SENT

Đã báo khách

## HANDOFF_FAILED

Handoff lỗi

## HANDOFF_CLOSED

Đóng handoff

## 80.3. Handoff triggers

Gateway phải handoff khi:

AI Advisor yêu cầu handoff.

Comment pháp lý/tố cáo nghiêm trọng.

Khiếu nại chất lượng.

Phản ứng bất lợi.

Tranh chấp payment.

Privacy request.

Yêu cầu không nhận tin/không lưu thông tin.

Human moderator cần xử lý.

Meta/App/Permission issue cần kỹ thuật.

Runtime không đủ để trả lời an toàn.

## 80.4. Chuyển trạng thái bị cấm

Không được:

HANDOFF_REQUIRED -> tiếp tục bán hàng.

HANDOFF_FAILED -> nói đã chuyển thành công.

Privacy request -> CRM tiếp tục gửi.

Payment dispute -> Gateway tự kết luận.

Quality complaint -> Gateway tự kết luận.

Handoff context public sai kênh.

## 80.5. P0 STOP_POINTS

Human Handoff FAIL nếu:

Handoff bắt buộc không tạo.

Handoff sai owner.

Handoff fail nhưng nói đã chuyển.

CRM không pause khi privacy/opt-out.

Complaint/adverse event vẫn vào sales flow.

Không có evidence handoff.

## 81. SUPPRESSION / BLOCKING PROPAGATION STATE MODEL

## 81.1. Mục tiêu

Suppression State Model đảm bảo Sale Lock, Recall, Commerce Block, AI Block, CRM opt-out và Moderation Block thắng delivery.

## 81.2. Trạng thái chuẩn

State

Ý nghĩa

## SUPPRESSION_NOT_CHECKED

Chưa kiểm

## SUPPRESSION_CHECKING

Đang kiểm

## SUPPRESSION_NONE

Không có suppression

## SUPPRESSION_ACTIVE

Có suppression

## SUPPRESSION_SALE_LOCK

Sale Lock

## SUPPRESSION_RECALL

Recall

## SUPPRESSION_NO_STOCK

Hết hàng

## SUPPRESSION_AI_BLOCKED

AI bị chặn

## SUPPRESSION_COMMERCE_BLOCKED

Commerce bị chặn

## SUPPRESSION_CRM_OPT_OUT

CRM opt-out

## SUPPRESSION_MODERATION_BLOCKED

Moderation block

## SUPPRESSION_RUNTIME_UNAVAILABLE

Runtime lỗi

## SUPPRESSION_PROPAGATED

Đã propagate

## SUPPRESSION_RELEASE_REVIEW_REQUIRED

Cần review gỡ

## SUPPRESSION_CLOSED

Đóng suppression

## 81.3. Chuyển trạng thái hợp lệ

From

To

Điều kiện

## SUPPRESSION_NOT_CHECKED

## SUPPRESSION_CHECKING

Trước delivery

## SUPPRESSION_CHECKING

## SUPPRESSION_NONE

No block

## SUPPRESSION_CHECKING

## SUPPRESSION_SALE_LOCK

Sale Lock active

## SUPPRESSION_CHECKING

## SUPPRESSION_RECALL

Recall active

## SUPPRESSION_CHECKING

## SUPPRESSION_AI_BLOCKED

AI Guard bị chặn

## SUPPRESSION_CHECKING

## SUPPRESSION_COMMERCE_BLOCKED

Commerce bị chặn

## SUPPRESSION_CHECKING

## SUPPRESSION_CRM_OPT_OUT

Opt-out

## SUPPRESSION_CHECKING

## SUPPRESSION_MODERATION_BLOCKED

Moderation block

## SUPPRESSION_CHECKING

## SUPPRESSION_RUNTIME_UNAVAILABLE

Runtime lỗi

Any suppression

## SUPPRESSION_ACTIVE

Normalize block

## SUPPRESSION_ACTIVE

## SUPPRESSION_PROPAGATED

Downstream aware

## SUPPRESSION_PROPAGATED

## SUPPRESSION_CLOSED

Không gửi sales delivery

## 81.4. Chuyển trạng thái bị cấm

Không được:

SUPPRESSION_ACTIVE -> sales response.

SUPPRESSION_SALE_LOCK -> quote/order.

SUPPRESSION_RECALL -> live selling.

SUPPRESSION_AI_BLOCKED -> delivery.

SUPPRESSION_CRM_OPT_OUT -> CRM sent.

SUPPRESSION_RUNTIME_UNAVAILABLE -> assume pass.

Dùng cache cũ để vượt suppression.

## 81.5. P0 STOP_POINTS

Suppression FAIL nếu:

Sale Lock/Recall không chặn Gateway.

AI bị chặn vẫn gửi.

CRM opt-out vẫn gửi.

Runtime unavailable vẫn pass.

Suppression không propagate sang Live/CRM/Ads/IVR.

## 82. DOWNSTREAM HANDOFF STATE MODEL

## 82.1. Mục tiêu

Downstream Handoff State Model kiểm soát dữ liệu Gateway bàn giao sang Ads, MC AI Live, IVR và TECH-10.

## 82.2. Trạng thái chuẩn

State

Ý nghĩa

## DOWNSTREAM_HANDOFF_NOT_STARTED

Chưa handoff

## DOWNSTREAM_CONTEXT_PREPARING

Chuẩn bị context

## DOWNSTREAM_SAFE_VIEW_READY

Safe view sẵn sàng

## DOWNSTREAM_SUPPRESSION_INCLUDED

Có suppression status

## DOWNSTREAM_HANDOFF_BLOCKED

Bị chặn

## DOWNSTREAM_SENT_TO_ADS

Gửi Ads safe metrics

## DOWNSTREAM_SENT_TO_MC_AI

Gửi MC AI Live context

## DOWNSTREAM_SENT_TO_IVR

Gửi IVR nếu scope có

## DOWNSTREAM_SENT_TO_TECH10

Gửi evidence/release

## DOWNSTREAM_ACKNOWLEDGED

Downstream xác nhận

## DOWNSTREAM_FAILED

Handoff lỗi

## DOWNSTREAM_CLOSED

Đóng handoff

## 82.3. Chuyển trạng thái bị cấm

Không được:

Ads nhận unverified revenue.

MC AI Live nhận SKU suppressed để chốt.

IVR nhận order chưa official hoặc đang locked.

TECH-10 nhận evidence draft như accepted.

Downstream safe view thiếu suppression.

Private customer data gửi downstream không có quyền.

## 82.4. P0 STOP_POINTS

Downstream Handoff FAIL nếu:

Ads dùng quote/cart/order draft làm revenue.

MC AI Live chốt SKU bị chặn.

IVR xác nhận order invalid.

TECH-10 thiếu App Review/Smoke evidence.

Safe view lộ dữ liệu riêng tư.

## 83. COMMAND BOUNDARY CỦA FACEBOOK GATEWAY

## 83.1. Định nghĩa command

Command là hành động làm thay đổi trạng thái, tạo side-effect hoặc gửi thông tin ra kênh.

Trong Gateway, command gồm:

Validate/record event.

Create private handoff.

Send public comment reply.

Send Messenger message.

Send CRM message.

Trigger typing indicator.

Schedule response delivery.

Create chunk package.

Hide/block/moderate comment nếu scope có.

Trigger human handoff.

Send downstream handoff.

Record evidence.

## 83.2. Command rủi ro cao

Command Group

Ví dụ

Public Response

Reply comment/live

Private Delivery

Send Messenger

Comment-to-Messenger

Private reply/handoff

Commerce Delivery

Deliver quote/order/payment safe view

## CRM

Send proactive message

Moderation

Hide/block/escalate comment

Human Handoff

Create handoff ticket

Typing/Pacing

Trigger typing + scheduled send

Downstream

Handoff Ads/Live/IVR

Evidence

Mark smoke/evidence status

## 83.3. Command bắt buộc có kiểm soát

Command rủi ro cao phải có:

Page/App/Permission pass.

Channel context.

Customer/session context nếu có private data.

AI Final Guard pass nếu gửi response.

Public-safe view pass.

Suppression check pass.

Rate limit pass.

Idempotency nếu event/retry.

Audit/evidence nếu production.

Typing/pacing nếu channel hỗ trợ.

## 83.4. Command bị cấm trong Gateway

Gateway không được có command:

Calculate final price.

Apply member/referral benefit.

Create QuoteSnapshot.

Create Official Order.

Confirm CustomerConfirmation thay khách.

Generate order_code.

Confirm payment.

Mark PAID.

Verify revenue.

Release Sale Lock.

Close Recall/CAPA.

Approve AI response.

Approve Product Knowledge/Claim.

Mark Evidence accepted.

Các command này thuộc upstream owner.

## 84. QUERY BOUNDARY CỦA FACEBOOK GATEWAY

## 84.1. Định nghĩa query

Query là hành động đọc dữ liệu, không thay đổi state, không gửi message.

Gateway có thể query:

Page/App config.

Permission status.

Channel context.

Live session context.

Customer/session binding.

AI response package status.

Final Guard status.

Commerce safe view.

Suppression status.

CRM preference.

Rate limit status.

Evidence status.

## 84.2. Query không được thay state

Query không được:

Gửi comment.

Gửi Messenger.

Tạo private handoff.

Tạo order.

Tạo payment confirmation.

Hide/block comment.

Gửi CRM.

Trigger typing indicator.

Mark smoke pass.

Mark evidence accepted.

Nếu query có side-effect, đó là command và phải qua command boundary.

## 84.3. Internal query và public delivery phải tách biệt

Gateway không được query internal view rồi gửi thẳng ra khách.

Phải qua:

Public-safe view selection.

Sensitive redaction.

Channel boundary.

Final delivery policy.

## 85. COMMAND-QUERY SEPARATION RULE

## 85.1. Nguyên tắc

Gateway phải tách rõ:

Query: đọc trạng thái.

Command: gửi, tạo handoff, schedule, hide, trigger, delivery.

Không dùng query để gửi response.

Không dùng query để tạo private handoff.

Không dùng query để xác nhận customer/order/payment.

Không dùng command để lấy dữ liệu mà bỏ qua guard.

## 85.2. Ví dụ đúng

Nhu cầu

Loại đúng

Kiểm page có approved không

Query

Nhận webhook event

Command record event

Kiểm comment intent

Query/evaluation

Gửi public reply

Command

Tạo private handoff

Command

Xem AI Guard pass chưa

Query

Gửi Messenger message

Command

Bật typing indicator

Command

Kiểm rate limit

Query

Hide comment

Command theo policy

Gửi CRM

Command

Tạo human handoff

Command

Xem evidence status

Query

## 85.3. P0 STOP_POINTS

Command-query boundary FAIL nếu:

Query gửi response.

Query tạo order/payment side-effect.

Command gửi response không guard.

Command CRM không opt-out check.

Command delivery không rate-limit check.

Command moderation không evidence/policy.

Command typing/pacing dùng để gửi content chưa guard.

## 86. RUNTIME UNAVAILABLE / FAIL-SAFE RULES

## 86.1. Nguyên tắc chung

Nếu runtime quan trọng không khả dụng, Gateway phải fail-safe.

Không được:

Gửi giá bịa.

Gửi order bịa.

Gửi payment status bịa.

Gửi trace bịa.

Gửi memory sai.

Dùng cache cũ để vượt suppression.

Gửi response chưa guard.

Tạo private handoff giả thành công.

Nói đã tạo ticket khi handoff lỗi.

Nháy typing rồi gửi nội dung không có căn cứ.

## 86.2. Runtime quan trọng và hành vi fail-safe

Runtime

Nếu unavailable thì

Page/App Config

Không production delivery

Permission/App Review

Không bật feature

Webhook Verification

Không xử lý event

Channel Context

Không gửi dữ liệu nhạy cảm

Live Context

Không chốt live

Customer Session

Không trả order/payment

AI Advisor

Không tự tạo response

Final Response Guard

Không gửi response

Operational Suppression

Fail-safe bị chặn với sales

Commerce Runtime

Không quote/order/payment delivery

Payment Runtime

Không nói PAID

Public Trace Runtime

Không nói trace valid

CRM Preference

Không gửi proactive

Rate Limit Runtime

Không gửi hàng loạt

Human Handoff Runtime

Không hứa đã tạo ticket

Evidence Runtime

Không PASS/Release Ready

## 87. CROSS-LIFECYCLE P0 STOP_POINTS MATRIX

## 87.1. Ma trận STOP_POINTS theo lifecycle

Lifecycle

## P0 STOP_POINTS

Downstream bị chặn

Meta Config

App/Page/Permission chưa approved

Production delivery

Webhook

Event invalid/duplicate

Event processing

Channel

Public/private nhầm

Response delivery

Comment

Hỏi giá không private

Quote flow

Moderation

Abuse/tố cáo vào sales flow

Public response

Private Handoff

Handoff fail nhưng báo success

Messenger flow

Messenger

Session mismatch

Order/payment

AI Handoff

Context thiếu hoặc guard fail

Delivery

Delivery

Rate limit/duplicate

Message send

Typing/Pacing

Instant reply

UX release

Chunking

Tách sai nghiệp vụ

Quote/order/payment

Commerce Delivery

No QuoteSnapshot/order/payment confirmation

Sales/payment flow

Redaction

Lộ private/internal data

Security release

## CRM

Opt-out vẫn gửi

CRM release

Human Handoff

Required handoff missing

CS/QC/Finance

Suppression

Sale Lock/Recall không chặn

Gateway toàn hệ

Evidence

Evidence draft dùng PASS

Release

Downstream

Ads/Live/IVR nhận dữ liệu không safe

## TECH-07/08/09

## 88. EVIDENCE CHO PHẦN 3/4

## 88.1. Evidence bắt buộc theo state machine

Evidence ID

Nội dung

## FB-EVD-P3-001

Meta App / Page / Permission State Evidence

## FB-EVD-P3-002

Webhook Event State Evidence

## FB-EVD-P3-003

Channel Context State Evidence

## FB-EVD-P3-004

Public Comment / Live Comment State Evidence

## FB-EVD-P3-005

Comment-to-Messenger Handoff Evidence

## FB-EVD-P3-006

Messenger Private Session Evidence

## FB-EVD-P3-007

Customer Session Binding Evidence

## FB-EVD-P3-008

AI Advisor Handoff Evidence

## FB-EVD-P3-009

Final Response Guard Consumer Evidence

## FB-EVD-P3-010

Delivery State Evidence

## FB-EVD-P3-011

Typing Indicator / Response Pacing Evidence

## FB-EVD-P3-012

Response Chunking Evidence

## FB-EVD-P3-013

Quote / Order / Payment Delivery Evidence

## FB-EVD-P3-014

Public-safe View / Redaction Evidence

## FB-EVD-P3-015

Moderation State Evidence

## FB-EVD-P3-016

Rate Limit / Anti-spam Evidence

## FB-EVD-P3-017

CRM Outbound State Evidence

## FB-EVD-P3-018

Human Handoff State Evidence

## FB-EVD-P3-019

Suppression Propagation Evidence

## FB-EVD-P3-020

Downstream Handoff Evidence

## FB-EVD-P3-021

Runtime Unavailable Fail-safe Evidence

## FB-EVD-P3-022

Command-Query Boundary Evidence

## 88.2. Evidence metadata tối thiểu

Evidence phải có:

Evidence ID.

Requirement/source rule.

Module.

Channel.

Page/App context.

Live session nếu có.

Customer/session nếu có.

Event ID/correlation.

Environment.

Version/build reference nếu có.

Test/smoke reference.

Actor/reviewer.

Status.

Timestamp.

Result.

Guard result nếu có.

Delivery status nếu có.

Blocking reason nếu có.

Handoff reference nếu có.

Chỉ evidence trạng thái ACCEPTED mới được dùng để PASS.

## 89. SMOKE MATRIX CHO PHẦN 3/4

## 89.1. Smoke lifecycle/state machine

Smoke ID

Lifecycle

Nội dung

Expected Result

## FB-P3-SMK-001

Meta Config

Page chưa approved

Không production

## FB-P3-SMK-002

Permission

App Review missing

Feature bị chặn

## FB-P3-SMK-003

Webhook

Duplicate event

Không gửi trùng

## FB-P3-SMK-004

Channel

Live comment

Public context

## FB-P3-SMK-005

Channel

Messenger sau handoff

Private context

## FB-P3-SMK-006

Comment

“giá?”

Private handoff

## FB-P3-SMK-007

Comment

“chốt 1 hộp”

Private order route

## FB-P3-SMK-008

Comment

Có phone/address

Không public lại

## FB-P3-SMK-009

Moderation

“lừa đảo”

Moderation/handoff

## FB-P3-SMK-010

Private Handoff

Handoff success

Messenger session bound

## FB-P3-SMK-011

Private Handoff

Handoff fail

Không nói đã gửi

## FB-P3-SMK-012

AI Handoff

Response draft

Không delivery

## FB-P3-SMK-013

Final Guard

Guard fail

Không gửi

## FB-P3-SMK-014

Delivery

Retry send

Không spam

## FB-P3-SMK-015

Typing/Pacing

Messenger short response

Có typing + delay

## FB-P3-SMK-016

Typing/Pacing

Order Draft

Không gửi tức thì

## FB-P3-SMK-017

Chunking

Quote response

Không tách giá khỏi expiry

## FB-P3-SMK-018

Commerce Delivery

No QuoteSnapshot

Không báo giá

## FB-P3-SMK-019

Commerce Delivery

Payment proof image

Không PAID

## FB-P3-SMK-020

Redaction

Public private data

Block/redact

## FB-P3-SMK-021

Rate Limit

Live spike

Throttle/queue

## FB-P3-SMK-022

## CRM

Opt-out

No send

## FB-P3-SMK-023

Handoff

Complaint

Human route

## FB-P3-SMK-024

Suppression

Sale Lock active

Gateway bị chặn

## FB-P3-SMK-025

Downstream

Ads asks revenue

No unverified revenue

## FB-P3-SMK-026

Runtime

AI unavailable

Gateway không tự trả lời

## FB-P3-SMK-027

Runtime

Handoff runtime fail

Không hứa đã tạo ticket

## FB-P3-SMK-028

Evidence

Draft evidence

Không PASS

## 90. DONE GATE CỦA PHẦN 3/4

## 90.1. Điều kiện Done Gate

## PHẦN 3/4 chỉ được xem là DONE khi:

Gateway lifecycle tổng thể đã khóa.

Meta App / Page / Permission State Model đã khóa.

Webhook Event State Model đã khóa.

Channel Context State Model đã khóa.

Public Comment / Live Comment State Model đã khóa.

Comment-to-Messenger / Private Handoff State Model đã khóa.

Messenger Private Session State Model đã khóa.

AI Advisor Handoff / Final Guard State Model đã khóa.

Delivery State Model đã khóa.

Typing Indicator / Response Pacing State Model đã khóa.

Response Chunking State Model đã khóa.

Quote / Order / Payment Delivery State Model đã khóa.

Public-safe View / Redaction State Model đã khóa.

Moderation State Model đã khóa.

Rate Limit / Anti-spam State Model đã khóa.

CRM Outbound State Model đã khóa.

Human Handoff State Model đã khóa.

Suppression / Blocking Propagation State Model đã khóa.

Downstream Handoff State Model đã khóa.

Command Boundary đã rõ.

Query Boundary đã rõ.

Command-query separation đã rõ.

Runtime unavailable fail-safe đã rõ.

Cross-lifecycle P0 STOP_POINTS matrix đã có.

Evidence requirement đã rõ.

Smoke matrix đã rõ.

Không gọi Documentation Complete là Production Ready.

## 91. FAIL GATE CỦA PHẦN 3/4

## 91.1. Điều kiện làm PHẦN 3 fail

## PHẦN 3/4 FAIL nếu có bất kỳ điểm nào sau:

Page/App/Permission chưa approved vẫn production.

Webhook invalid/duplicate vẫn xử lý delivery.

Public/private channel bị nhầm.

Comment hỏi giá public vẫn báo giá public.

Comment “chốt” public tạo order.

Private handoff fail nhưng Gateway nói đã gửi Messenger.

Messenger session mismatch vẫn trả order/payment.

AI response draft chưa guard vẫn gửi.

Final Guard fail nhưng delivery.

Kênh hỗ trợ typing nhưng không có typing/pacing.

Response dài bật ra ngay một khối.

Typing indicator dùng để giả mạo human.

Chunking tách sai giá/expiry/order/payment.

Quote không QuoteSnapshot vẫn gửi.

Payment proof image thành PAID.

Public-safe view lộ private/internal data.

Abuse/tố cáo/khiếu nại vào flow bán hàng thường.

Rate limit bị bỏ qua.

CRM gửi khi opt-out.

Human handoff required nhưng không tạo.

Sale Lock/Recall không chặn Gateway.

Runtime unavailable nhưng Gateway tự suy đoán.

Downstream nhận unverified revenue/private data.

Evidence chưa accepted nhưng vẫn PASS.

## 92. TRẠNG THÁI SAU PHẦN 3/4

## 92.1. Trạng thái tài liệu

Hạng mục

Trạng thái

## PACK-06 PHẦN 3/4

## WRITTEN_FOR_REVIEW

Gateway Lifecycle

## LOCKED_DRAFT

Gateway State Machine Model

## DEFINED

Meta / Webhook / Channel State Models

## DEFINED

Comment-to-Messenger State Model

## DEFINED

Messenger Session State Model

## DEFINED

AI Final Guard Consumption

## DEFINED

Delivery / Typing / Pacing State Models

## DEFINED

Public-safe / Moderation / CRM State Models

## DEFINED

Command Boundary

## DEFINED

Query Boundary

## DEFINED

Command-Query Separation

## DEFINED

Runtime Unavailable Rule

## DEFINED

P0 STOP_POINTS Matrix

## DEFINED

Evidence Requirement

## DEFINED

Smoke Matrix

## DEFINED

Implementation Status

waiting

Test Status

waiting

Smoke Status

waiting

Evidence Status

waiting

Release Ready

## KHÔNG

Production Ready

## KHÔNG

Go-live Approved

## KHÔNG

## 93. KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời vận hành của Facebook Gateway / Meta Live & Messenger Runtime.

Từ đây trở đi, Gateway không được triển khai như webhook tự động trả lời comment hoặc bot Messenger đơn giản.

Gateway phải vận hành theo state machine rõ:

Meta Config -> Webhook Event -> Page/Live Context -> Channel Context -> Public Comment -> Comment Classifier -> Private Handoff -> Messenger Session -> AI Advisor Handoff -> Final Response Guard -> Delivery Policy -> Typing/Pacing -> Chunking -> Safe Delivery -> Redaction -> Moderation -> Rate Limit -> CRM/Human Handoff -> Suppression -> Downstream Handoff.

Nguyên tắc cuối cùng của PHẦN 3/4:

Gateway chỉ được gửi response khi Page/App/Permission pass, event hợp lệ, channel đúng, session đúng, AI Final Guard pass, public-safe view pass, suppression không active, rate limit pass và delivery pacing được áp dụng. Ở kênh có typing indicator, không được bật câu trả lời ngay như chatbot máy móc.

## PHẦN 3/4 hoàn tất ở mức WRITTEN_FOR_REVIEW, chưa phải Production Ready, chưa phải Release Ready và chưa được Go-live Approved.

## PHẦN 4/4 — FINAL FACEBOOK GATEWAY SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / PACK-06 FINAL CONCLUSION

## 94. MỤC TIÊU CỦA PHẦN 4/4

## 94.1. Vai trò của PHẦN 4/4

## PHẦN 4/4 là phần khóa cuối của PACK-06.

PHẦN này không viết thêm module mới.

PHẦN này dùng để khóa:

Final Facebook Gateway Smoke Matrix.

Facebook Gateway Evidence Package.

Meta App / Page / Permission Review.

Public / Private Channel Review.

Comment-to-Messenger Review.

Typing Indicator / Response Pacing Review.

No Instant Bot Reply Review.

No Human Impersonation Review.

Moderation / Abuse / Hide / Block Review.

CRM / Proactive Messaging Review.

Human Handoff Review.

Security / Privacy / Public-safe Channel Review.

Downstream Readiness Rule.

Release Handoff sang TECH-07 Ads, TECH-08 MC AI Live, TECH-09 IVR, TECH-10 Completion / Evidence / Release.

Done Gate toàn PACK-06.

Fail Gate toàn PACK-06.

P0 STOP_POINTS Registry cuối.

Final Conclusion của Facebook Gateway.

## 94.2. Nguyên tắc khóa cuối

PACK-06 chỉ được xem là Documentation Complete khi đủ 4 phần.

Nhưng:

Documentation Complete không phải Production Ready.

PACK-06 hoàn tất tài liệu không có nghĩa là:

Đã implement.

Đã cấu hình Meta App thật.

Đã App Review pass.

Đã webhook pass.

Đã test Page thật.

Đã test Live thật.

Đã test Messenger thật.

Đã test typing indicator thật.

Đã smoke pass.

Đã có accepted evidence.

Đã release ready.

Đã production ready.

Đã go-live approved.

Không có accepted evidence thì không Completion PASS.

Không có smoke pass thì không Release Ready.

Không có owner sign-off thì không Release Approved.

Không có release decision thì không Go-live Approved.

Không dùng lời xác nhận miệng để PASS.

Không downstream nào được phụ thuộc production vào Facebook Gateway nếu PACK-06 chưa clear implementation / evidence / smoke.

## 95. FINAL FACEBOOK GATEWAY SMOKE MATRIX — TỔNG THỂ

## 95.1. Mục tiêu của Final Facebook Gateway Smoke Matrix

Final Facebook Gateway Smoke Matrix dùng để chứng minh Gateway vận hành đúng trong tình huống thật.

Smoke không chỉ kiểm webhook nhận event được hay không.

Smoke phải chứng minh được các rule P0 sau:

Page/App/Permission đúng scope.

App Review/permission chưa pass thì không production.

Webhook invalid/duplicate không gây spam.

Public comment và Messenger private tách rõ.

Comment hỏi giá/mua/tư vấn sâu phải kéo private theo rule.

Public comment không public quote/order/payment/private data.

Sau Messenger handoff thành công, conversation phải xử lý như private.

Gateway không gửi response chưa qua AI Final Response Guard.

Gateway không tự tính giá.

Gateway không tự tạo order.

Gateway không tự xác nhận payment.

Gateway không public trace khi Public Trace bị chặn.

Gateway tôn trọng Sale Lock / Recall / Suppression.

Gateway có typing indicator / 3 chấm ở kênh hỗ trợ.

Gateway có response pacing, không trả lời bật ngay như chatbot máy móc.

Gateway không giả mạo người thật.

CRM opt-out phải chặn proactive message.

Moderation phải chặn comment abuse/tố cáo/khiếu nại rủi ro.

Runtime unavailable thì fail-safe.

Evidence chưa ACCEPTED thì không PASS.

## 96. SMOKE GROUP A — META BUSINESS / APP / PAGE / PERMISSION / APP REVIEW

## 96.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-A001

Page chưa owner approved

Page chưa được duyệt

Không bật auto-reply production

## CÓ

## FB-SMK-A002

Page test

Test page trong DEV/STAGING

Không gửi khách production

## CÓ

## FB-SMK-A003

Page production approved

Page chính thức đã approved

Cho phép pilot theo scope

## CÓ

## FB-SMK-A004

Meta App chưa owner approved

App chưa có owner sign-off

Không production

## CÓ

## FB-SMK-A005

Permission missing

Permission chưa có

Feature liên quan bị block

## CÓ

## FB-SMK-A006

App Review waiting

Chưa được Meta duyệt

Không bật production feature

## CÓ

## FB-SMK-A007

App Review rejected

Meta từ chối

Feature bị block, cần owner review

## CÓ

## FB-SMK-A008

Webhook chưa subscribed

Chưa subscribe event

Không xử lý production event

## CÓ

## FB-SMK-A009

Token/secret hardcoded

Token/secret nằm trong code/template

Fail Gate

## CÓ

## FB-SMK-A010

Page/App mapping sai environment

Page PROD map nhầm DEV

Block production

## CÓ

## 96.2. Fail Gate

Nhóm A FAIL nếu:

Page chưa approved vẫn auto-reply.

App chưa approved vẫn production.

Permission/App Review missing vẫn bật feature.

Token/secret hardcoded.

Page test gửi khách thật.

Không có owner mapping.

Không có evidence Meta App/Page/Permission.

Nếu nhóm A FAIL, toàn bộ PACK-06 production gateway bị bị chặn.

## 97. SMOKE GROUP B — WEBHOOK / EVENT / IDEMPOTENCY / DUPLICATE CONTROL

## 97.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-B001

Valid webhook event

Comment event hợp lệ

Event được validate và route

## CÓ

## FB-SMK-B002

Invalid webhook

Signature/verify fail

Event bị reject/block

## CÓ

## FB-SMK-B003

Unknown Page event

Page không thuộc registry

Block, không xử lý

## CÓ

## FB-SMK-B004

Duplicate event

Meta retry cùng event

Không gửi response trùng

## CÓ

## FB-SMK-B005

Unknown event type

Event chưa map

Fail-safe, không gửi dữ liệu private

## CÓ

## FB-SMK-B006

Event không có channel

Missing channel context

Không delivery dữ liệu nhạy cảm

## CÓ

## FB-SMK-B007

Event retry sau delivery fail

Retry trong policy

Không spam khách

## CÓ

## FB-SMK-B008

Event raw payload

Payload thô

Không public ra response

## CÓ

## 97.2. Fail Gate

Nhóm B FAIL nếu:

Webhook invalid vẫn xử lý.

Duplicate event gây nhiều response.

Unknown page vẫn auto-reply.

Unknown channel vẫn trả order/payment/private data.

Raw payload lộ ra khách.

Không có event correlation/evidence.

## 98. SMOKE GROUP C — CHANNEL CONTEXT / PUBLIC COMMENT / PRIVATE MESSENGER

## 98.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-C001

Live comment

Event live comment

Channel = public live comment

## CÓ

## FB-SMK-C002

Post comment

Event post comment

Channel = public comment

## CÓ

## FB-SMK-C003

Messenger private

Message private

Channel = private Messenger

## CÓ

## FB-SMK-C004

Private reply after comment

Comment-to-Messenger success

Channel chuyển private

## CÓ

## FB-SMK-C005

Unknown channel

Không resolve được

Fail-safe, không trả private data

## CÓ

## FB-SMK-C006

Public comment có phone/address

Khách comment thông tin cá nhân

Gateway không lặp lại public

## CÓ

## FB-SMK-C007

Public comment hỏi order

Hỏi trạng thái đơn

Không public order info, kéo private/handoff

## CÓ

## FB-SMK-C008

Messenger sau handoff

Khách trả lời trong Messenger

Xử lý như private, không dùng public template

## CÓ

## 98.2. Fail Gate

Nhóm C FAIL nếu:

Public/private bị nhầm.

Live comment public giá riêng/order/payment.

Public comment lặp lại số điện thoại/địa chỉ.

Messenger sau handoff vẫn trả như public comment.

Unknown channel vẫn delivery dữ liệu nhạy cảm.

Order/payment status bị trả ở public.

## 99. SMOKE GROUP D — COMMENT INTENT / COMMENT-TO-MESSENGER / PRIVATE HANDOFF

## 99.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-D001

Comment hỏi giá

“giá?”, “bao nhiêu?”

Private handoff, không public quote

## CÓ

## FB-SMK-D002

Comment muốn mua

“chốt 1 hộp”, “lấy 2”

Private order route, không public order

## CÓ

## FB-SMK-D003

Comment mơ hồ

“.”, “ib”, emoji, “quan tâm”

Route private theo policy

## CÓ

## FB-SMK-D004

Comment hỏi tư vấn sâu

“loại nào hợp mẹ tôi?”

Private handoff

## CÓ

## FB-SMK-D005

Comment gửi số điện thoại

Số điện thoại public

Không public lại, private/handoff theo policy

## CÓ

## FB-SMK-D006

Handoff success

Private message gửi được

Public ack được phép: “đã gửi Messenger…”

## CÓ

## FB-SMK-D007

Handoff fail

Private message lỗi

Không nói “đã gửi Messenger”

## CÓ

## FB-SMK-D008

Duplicate handoff

Cùng comment retry

Không gửi nhiều private message

## CÓ

## FB-SMK-D009

Abuse nghiêm trọng

Chửi/tố cáo nặng

Moderation/handoff, không kéo private bán hàng thường

## CÓ

## 99.2. Fail Gate

Nhóm D FAIL nếu:

Hỏi giá public mà Gateway public giá.

Comment chốt mua public tạo order.

Comment mơ hồ không route đúng.

Handoff fail nhưng public nói đã gửi.

Handoff thành công nhưng session không bind.

Duplicate event tạo nhiều private reply.

Comment abuse bị xử lý như lead bán hàng thường.

## 100. SMOKE GROUP E — AI ADVISOR HANDOFF / FINAL RESPONSE GUARD

## 100.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-E001

Gateway gửi đủ channel context

Public comment vào AI

AI nhận public context

## CÓ

## FB-SMK-E002

Gateway gửi đủ customer/session

Messenger hỏi order/payment

AI nhận session context

## CÓ

## FB-SMK-E003

AI response draft

Response chưa Final Guard

Gateway không gửi

## CÓ

## FB-SMK-E004

AI Final Guard pass

Response safe

Gateway được delivery

## CÓ

## FB-SMK-E005

AI Final Guard fail

Claim/leak/bị chặn

Gateway không delivery

## CÓ

## FB-SMK-E006

AI rewrite required

Cần rewrite

## CÓ

## FB-SMK-E007

AI human handoff required

Handoff required

Gateway route handoff, không tự xử lý

## CÓ

## FB-SMK-E008

AI runtime unavailable

AI lỗi

Gateway không tự tạo response có giá/claim/order

## CÓ

## 100.2. Fail Gate

Nhóm E FAIL nếu:

Gateway gửi thiếu channel/customer context.

Response draft chưa guard vẫn gửi.

Guard fail vẫn delivery.

Handoff required nhưng Gateway tự trả lời bán hàng.

AI lỗi nhưng Gateway tự bịa response.

Gateway sửa AI response để thêm giá/claim/urgency.

## 101. SMOKE GROUP F — TYPING INDICATOR / RESPONSE PACING / NO-INSTANT-BOT-REPLY

## 101.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-F001

Messenger short response

Câu trả lời ngắn

Có typing/3 chấm + delay hợp lý

## CÓ

## FB-SMK-F002

Messenger medium response

Tư vấn 2–3 ý

Typing + pacing vừa

## CÓ

## FB-SMK-F003

Messenger long response

Tư vấn sâu

Typing + pacing dài hơn hoặc chunking

## CÓ

## FB-SMK-F004

Quote response

Báo giá từ QuoteSnapshot

Không bật ngay; có pacing

## CÓ

## FB-SMK-F005

Order Draft

Draft đủ 3 phần

Không gửi tức thì; pacing phù hợp

## CÓ

## FB-SMK-F006

Payment instruction

Hướng dẫn thanh toán

Pacing phù hợp, rõ ràng

## CÓ

## FB-SMK-F007

Complaint handoff

Khiếu nại chất lượng

Pacing nghiêm túc, không vội

## CÓ

## FB-SMK-F008

Guard fail sau typing

Response bị guard chặn

Không gửi nội dung fail

## CÓ

## FB-SMK-F009

Runtime lỗi

AI/Commerce lỗi

Không typing rồi bịa nội dung

## CÓ

## FB-SMK-F010

Khách hỏi “người thật không?”

Câu hỏi minh bạch

Trả lời là trợ lý tự động, không giả mạo human

## CÓ

## FB-SMK-F011

Instant reply test

Response bật ngay sau input

Fail Gate

## CÓ

## 101.2. Fail Gate

Nhóm F FAIL nếu:

Kênh hỗ trợ typing nhưng trả lời bật ngay.

Câu dài gửi tức thì một khối.

Quote/order draft gửi tức thì như máy.

Không có 3 chấm/typing ở kênh hỗ trợ.

Typing indicator bật nhưng response chưa guard.

Typing dùng để giả mạo người thật.

Runtime lỗi nhưng Gateway vẫn typing rồi trả nội dung bịa.

Pacing quá nhiều gây spam hoặc vi phạm platform.

## 102. SMOKE GROUP G — RESPONSE CHUNKING / MULTI-MESSAGE DELIVERY

## 102.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-G001

Tư vấn dài

Response dài

Chia message hợp lý

## CÓ

## FB-SMK-G002

Quote response

Giá + expiry

Không tách giá khỏi expiry

## CÓ

## FB-SMK-G003

Order Draft

Receiver + payment + confirmation

Không tách thiếu CTA/context

## CÓ

## FB-SMK-G004

Payment instruction

Bank info + reference

Không tách mất memo/reference

## CÓ

## FB-SMK-G005

Complaint/handoff

Sensitive response

Không tách mất handoff/ack context

## CÓ

## FB-SMK-G006

Public comment

Câu public

Không chunk dài gây rối live

## CÓ

## FB-SMK-G007

Chunk guard

Một chunk có leak

Toàn package bị block/rewrite

## CÓ

## FB-SMK-G008

Too many chunks

Nhiều message liên tục

Rate limit/anti-spam kiểm soát

## CÓ

## 102.2. Fail Gate

Nhóm G FAIL nếu:

Gửi chunk trước khi toàn bộ package guard pass.

Tách giá khỏi expiry.

Tách tổng đơn khỏi CTA xác nhận.

Tách payment reference khỏi hướng dẫn thanh toán.

Chunk dài spam Messenger/live.

Public comment bị chia nhiều message dài.

Chunk có leak vẫn gửi các chunk còn lại.

## 103. SMOKE GROUP H — QUOTE / ORDER / PAYMENT SAFE DELIVERY

## 103.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-H001

Messenger hỏi giá

Commerce QuoteSnapshot active

Gateway gửi đúng safe view

## CÓ

## FB-SMK-H002

Không QuoteSnapshot

Commerce chưa trả quote

Gateway không báo final price

## CÓ

## FB-SMK-H003

Quote expired

Quote hết hạn

Không chốt, yêu cầu refresh

## CÓ

## FB-SMK-H004

Public comment hỏi giá

Comment public

Không public quote riêng

## CÓ

## FB-SMK-H005

Order Draft đủ 3 phần

Draft từ Commerce

Gateway gửi cho khách xác nhận

## CÓ

## FB-SMK-H006

Draft thiếu 3 phần

Missing receiver/payment/CTA

Không cho confirmation

## CÓ

## FB-SMK-H007

CustomerConfirmation missing

Khách chưa xác nhận

Không order_code

## CÓ

## FB-SMK-H008

Official order created

Commerce trả order_code

Gateway mới gửi order_code

## CÓ

## FB-SMK-H009

Bank transfer instruction

Payment method chuyển khoản

Dùng Bank Account Registry / Commerce instruction

## CÓ

## FB-SMK-H010

Bank info hardcoded

Template có số tài khoản cố định

Fail Gate

## CÓ

## FB-SMK-H011

Khách gửi ảnh chuyển khoản

Proof image

Gateway/AI nói chờ kiểm tra, không PAID

## CÓ

## FB-SMK-H012

Payment confirmed

Runtime confirmed

Gateway được gửi status confirmed

## CÓ

## FB-SMK-H013

Public payment/order query

Comment public hỏi đơn

Không public private data

## CÓ

## 103.2. Fail Gate

Nhóm H FAIL nếu:

Gateway tự tính giá.

Quote không từ QuoteSnapshot.

Quote expired vẫn chốt.

Order Draft thiếu 3 phần vẫn gửi confirm.

Không CustomerConfirmation vẫn order_code.

Gateway tự tạo order.

Ảnh chuyển khoản được xem là PAID.

Bank account hardcoded.

Public comment lộ order/payment.

## 104. SMOKE GROUP I — PUBLIC-SAFE VIEW / PRIVACY / REDACTION / SECURITY

## 104.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-I001

Public full address

Comment/live

Redact/block

## CÓ

## FB-SMK-I002

Public phone number

Comment/live

Không lặp lại

## CÓ

## FB-SMK-I003

Public payment proof

Comment/live

Không public

## CÓ

## FB-SMK-I004

Wrong customer order

Khách A hỏi order B

Bị chặn

## CÓ

## FB-SMK-I005

Member tier private

Public comment

Không public

## CÓ

## FB-SMK-I006

Diamond/referral private data

Public comment

Không public

## CÓ

## FB-SMK-I007

Commission/KPI/ROAS

Any customer channel

Không public

## CÓ

## FB-SMK-I008

Supplier/QC/costing/MISA

Public/private customer view

Không public

## CÓ

## FB-SMK-I009

Internal blocking reason

Internal reason

Rewrite public-safe

## CÓ

## FB-SMK-I010

Technical stack/log

Runtime error

Không lộ

## CÓ

## FB-SMK-I011

Public Trace internal data

Trace request

Chỉ whitelist, không internal

## CÓ

## 104.2. Fail Gate

Nhóm I FAIL nếu:

Public dữ liệu nhận hàng/thanh toán/hóa đơn.

Trả order/payment sai khách.

Public member tier/benefit riêng.

Public referral owner/internal ID.

Public commission/KPI/ROAS.

Public supplier/QC/costing/MISA/internal trace.

Public internal blocking reason.

Lộ stack/log lỗi kỹ thuật.

## 105. SMOKE GROUP J — MODERATION / ABUSE / HIDE / BLOCK / QUALITY COMPLAINT

## 105.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-J001

Comment chửi thề

Profanity/abuse

Moderation/hide/handoff theo policy

## CÓ

## FB-SMK-J002

Comment “lừa đảo”

Scam accusation

Human/moderation route, không bán hàng thường

## CÓ

## FB-SMK-J003

Spam link

Link spam

Hide/block theo policy

## CÓ

## FB-SMK-J004

Tố cáo pháp lý

Legal risk

Human handoff

## CÓ

## FB-SMK-J005

Khiếu nại chất lượng public

Complaint

CS/QC handoff, không auto sales

## CÓ

## FB-SMK-J006

Phản ứng bất lợi public

Adverse event

Handoff ngay

## CÓ

## FB-SMK-J007

Khiếu nại có mã đơn/QR

Có thông tin kiểm tra

Không auto blacklist, chuyển CS/QC

## CÓ

## FB-SMK-J008

Comment gây rối live

Troll/spam

Moderation theo policy

## CÓ

## FB-SMK-J009

Hide/block action

Action moderation

Có evidence/reason

## CÓ

## 105.2. Fail Gate

Nhóm J FAIL nếu:

Chửi/tố cáo/khiếu nại vào flow bán hàng thường.

Gateway đôi co public.

Gateway tự kết luận đúng/sai.

Khiếu nại chất lượng không handoff.

Adverse event không handoff.

Comment thật có order/QR bị auto blacklist.

Hide/block thiếu reason/evidence.

## 106. SMOKE GROUP K — RATE LIMIT / ANTI-SPAM / CRM / PROACTIVE MESSAGING

## 106.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-K001

Delivery retry

Send fail retry

Không gửi trùng/spam

## CÓ

## FB-SMK-K002

Live comment spike

Nhiều comment cùng lúc

Throttle/queue theo policy

## CÓ

## FB-SMK-K003

Repeated same response

Nhiều comment giống nhau

Không spam một câu hàng loạt nếu policy chặn

## CÓ

## FB-SMK-K004

CRM opt-out

Khách không nhận tin

Không gửi proactive

## CÓ

## FB-SMK-K005

CRM fatigue fail

Gửi quá dày

Block

## CÓ

## FB-SMK-K006

CRM SKU recall

SKU bị recall

Suppression

## CÓ

## FB-SMK-K007

CRM SKU sale lock

SKU stop-sale

Suppression

## CÓ

## FB-SMK-K008

CRM quote

Nội dung báo giá

Phải có QuoteSnapshot

## CÓ

## FB-SMK-K009

CRM privacy request waiting

Khách yêu cầu không lưu/không nhận

Pause CRM

## CÓ

## FB-SMK-K010

Platform policy block

Chính sách không cho gửi

Block delivery

## CÓ

## 106.2. Fail Gate

Nhóm K FAIL nếu:

Gateway spam comment/message.

Retry lỗi gửi trùng.

Live spike không throttle.

CRM gửi khi opt-out.

CRM gửi quá dày.

CRM upsell SKU recall/sale lock.

CRM quote không QuoteSnapshot.

CRM tiếp tục gửi khi privacy request waiting.

Vượt platform policy.

## 107. SMOKE GROUP L — HUMAN HANDOFF / SUPPRESSION / DOWNSTREAM HANDOFF

## 107.1. Smoke Matrix

Smoke ID

Mục tiêu

Dữ liệu kiểm tra

Expected Result

## P0

## FB-SMK-L001

Human handoff required

AI/Handoff policy yêu cầu

Gateway tạo handoff đúng owner

## CÓ

## FB-SMK-L002

Handoff fail

Không tạo được ticket

Không nói đã chuyển thành công

## CÓ

## FB-SMK-L003

Payment dispute

Khách tranh chấp chuyển khoản

Finance handoff

## CÓ

## FB-SMK-L004

Privacy deletion request

Khách yêu cầu xóa dữ liệu

Privacy handoff + CRM pause

## CÓ

## FB-SMK-L005

Sale Lock active

SKU locked

Gateway bị chặn

## CÓ

## FB-SMK-L006

Recall active

SKU/batch recall

Gateway bị chặn / handoff if relevant

## CÓ

## FB-SMK-L007

AI response bị chặn

Final Guard bị chặn

Gateway không gửi

## CÓ

## FB-SMK-L008

Runtime unavailable

AI/Commerce/Operational lỗi

Fail-safe

## CÓ

## FB-SMK-L009

Ads asks Gateway revenue

Gateway event metrics

Không gửi unverified revenue

## CÓ

## FB-SMK-L010

MC AI Live asks SKU context

SKU suppressed

Handoff suppression included

## CÓ

## FB-SMK-L011

IVR asks order context

Order chưa official/locked

Handoff bị chặn

## CÓ

## FB-SMK-L012

TECH-10 evidence

Release handoff

Evidence package complete, accepted-only

## CÓ

## 107.2. Fail Gate

Nhóm L FAIL nếu:

Handoff required nhưng không tạo.

Handoff fail nhưng nói đã chuyển.

Payment dispute không handoff Finance.

Privacy request không pause CRM.

Sale Lock/Recall không chặn Gateway.

Runtime unavailable nhưng Gateway vẫn gửi sales response.

Ads nhận unverified revenue.

MC AI Live nhận SKU suppressed để chốt.

IVR nhận order chưa official/locked.

TECH-10 nhận evidence draft như accepted.

## 108. FINAL FACEBOOK GATEWAY EVIDENCE PACKAGE

## 108.1. Mục tiêu của Evidence Package

Facebook Gateway Evidence Package là bộ chứng cứ bắt buộc để chứng minh PACK-06 đã được triển khai, kiểm thử, smoke, App Review và owner review đúng.

Không có Evidence Package ACCEPTED thì không được gọi:

Completion PASS.

Release Ready.

Production Ready.

Go-live Approved.

## 109. FACEBOOK GATEWAY EVIDENCE PACKAGE STRUCTURE

## 109.1. Nhóm evidence bắt buộc

Evidence Group

Nội dung

Bắt buộc

## FB-EVD-G01

Meta Business / App / Page Registry Evidence

## CÓ

## FB-EVD-G02

Permission / App Review / App Mode Evidence

## CÓ

## FB-EVD-G03

Webhook Verification / Subscription Evidence

## CÓ

## FB-EVD-G04

Page / Live Session Context Evidence

## CÓ

## FB-EVD-G05

Channel Context / Public-Private Boundary Evidence

## CÓ

## FB-EVD-G06

Public Comment Intake / Intent Classifier Evidence

## CÓ

## FB-EVD-G07

Comment-to-Messenger / Private Reply Evidence

## CÓ

## FB-EVD-G08

Messenger Private Session / Customer Binding Evidence

## CÓ

## FB-EVD-G09

AI Advisor Handoff Evidence

## CÓ

## FB-EVD-G10

Final Response Guard Consumer Evidence

## CÓ

## FB-EVD-G11

Delivery Controller Evidence

## CÓ

## FB-EVD-G12

Typing Indicator / Response Pacing Evidence

## CÓ

## FB-EVD-G13

No Instant Bot Reply Evidence

## CÓ

## FB-EVD-G14

No Human Impersonation Evidence

## CÓ

## FB-EVD-G15

Response Chunking Evidence

## CÓ

## FB-EVD-G16

Quote / Order / Payment Safe Delivery Evidence

## CÓ

## FB-EVD-G17

Public-safe View / Redaction Evidence

## CÓ

## FB-EVD-G18

Moderation / Abuse / Hide / Block Evidence

## CÓ

## FB-EVD-G19

Rate Limit / Anti-spam Evidence

## CÓ

## FB-EVD-G20

CRM / Proactive Messaging Evidence

## CÓ

## FB-EVD-G21

Human Handoff Evidence

## CÓ

## FB-EVD-G22

Runtime Suppression / Blocking Propagation Evidence

## CÓ

## FB-EVD-G23

Downstream Handoff Evidence

## CÓ

## FB-EVD-G24

Runtime Unavailable / Fail-Safe Evidence

## CÓ

## FB-EVD-G25

Final Facebook Gateway Smoke Report

## CÓ

## FB-EVD-G26

Pilot / UAT Evidence

## CÓ

## FB-EVD-G27

Owner Review / Sign-off Record

## CÓ

## FB-EVD-G28

Release Handoff Record

## CÓ

## 110. EVIDENCE METADATA STANDARD

## 110.1. Metadata tối thiểu

Mỗi evidence record phải có:

Evidence ID.

Requirement ID hoặc source rule.

TECH/module/smoke mapping.

Page/App context.

Channel context.

Live session context nếu có.

Customer/session test context nếu có.

Event ID/correlation nếu có.

Environment.

Build/version nếu có.

Meta permission/App Review status nếu liên quan.

Test/smoke ID.

Actor.

Reviewer.

Review status.

Timestamp.

Result.

Attachment/log/screenshot/report link nếu có.

Guard result nếu liên quan.

Delivery/pacing result nếu liên quan.

Blocking reason nếu có.

Handoff reference nếu có.

Owner decision nếu dùng cho gate.

## 110.2. Evidence status chuẩn

Status

Ý nghĩa

Được dùng để PASS

## DRAFT

Evidence nháp

## KHÔNG

## SUBMITTED

Đã nộp review

## KHÔNG

## NEEDS_CLARIFICATION

Cần làm rõ

## KHÔNG

## REJECTED

Bị từ chối

## KHÔNG

## ACCEPTED

Đã được reviewer/owner chấp nhận

## CÓ

## SUPERSEDED

Bị thay thế bởi evidence mới

## KHÔNG

## EXPIRED

Không còn hợp lệ

## KHÔNG

## 110.3. Rule khóa

Chỉ evidence trạng thái ACCEPTED mới được dùng để PASS.

Không dùng:

Lời xác nhận miệng.

Screenshot rời không mapping.

Log không environment.

Test chưa pass.

Evidence không liên kết requirement.

Evidence không timestamp.

Evidence không owner/reviewer khi cần.

App Review “đang chờ” thay cho approved.

Smoke chưa expected result.

Pilot chưa report.

Typing/pacing chưa có bằng chứng.

## 111. OWNER REVIEW POINTS

## 111.1. Mục tiêu

Owner Review Points xác định các điểm bắt buộc cần người có thẩm quyền duyệt.

Facebook Gateway ảnh hưởng trực tiếp đến public comment, Messenger, live selling, private customer data, Meta policy, CRM, pricing handoff, order flow và brand trust. Vì vậy owner review là bắt buộc.

## 112. OWNER REVIEW MATRIX

Review Point

Nội dung review

Owner đề xuất

Bắt buộc

## FB-ORP-001

Facebook Gateway boundary

Business / Technical Owner

## CÓ

## FB-ORP-002

Meta Business / App / Page Registry

Business / Channel Owner

## CÓ

## FB-ORP-003

Permission / App Review / App Mode

Technical / Meta Owner

## CÓ

## FB-ORP-004

Webhook Intake / Duplicate Control

Technical Owner

## CÓ

## FB-ORP-005

Public / Private Channel Boundary

Channel / Privacy Owner

## CÓ

## FB-ORP-006

Live Comment Response Boundary

Channel / Brand Owner

## CÓ

## FB-ORP-007

Comment-to-Messenger Rule

Channel / Commerce / AI Owner

## CÓ

## FB-ORP-008

Messenger Private Session Rule

Channel / CX Owner

## CÓ

## FB-ORP-009

AI Final Response Guard Consumption

AI / Technical Owner

## CÓ

## FB-ORP-010

Typing Indicator / Response Pacing

CX / Channel Owner

## CÓ

## FB-ORP-011

No Instant Bot Reply

CX / Brand Owner

## CÓ

## FB-ORP-012

No Human Impersonation

Legal / Brand / Privacy Owner

## CÓ

## FB-ORP-013

Response Chunking

CX / Channel Owner

## CÓ

## FB-ORP-014

Quote / Order / Payment Safe Delivery

Commerce / Finance Owner

## CÓ

## FB-ORP-015

Public-safe View / Redaction

Security / Privacy Owner

## CÓ

## FB-ORP-016

Moderation / Abuse / Hide / Block

Channel / Legal / CS Owner

## CÓ

## FB-ORP-017

Rate Limit / Anti-spam

Technical / Channel Owner

## CÓ

## FB-ORP-018

CRM / Proactive Messaging

CRM / Privacy Owner

## CÓ

## FB-ORP-019

Human Handoff

CS / QC / Finance / Owner

## CÓ

## FB-ORP-020

Suppression / Sale Lock / Recall Propagation

Operational / Commerce Owner

## CÓ

## FB-ORP-021

Downstream Handoff

Ads / MC AI / IVR / Release Owner

## CÓ

## FB-ORP-022

Final Gateway Smoke Acceptance

Release Owner

## CÓ

## FB-ORP-023

Pilot / UAT Acceptance

Business / Channel / Release Owner

## CÓ

## FB-ORP-024

PACK-06 Release Handoff

Technical Owner

## CÓ

## 113. SECURITY / PRIVACY / PUBLIC-SAFE CHANNEL REVIEW

## 113.1. Mục tiêu

Security / Privacy / Public-safe Channel Review đảm bảo Gateway không lộ dữ liệu riêng tư, dữ liệu nội bộ, dữ liệu thanh toán, dữ liệu đơn hàng, dữ liệu member, dữ liệu trace nội bộ hoặc dữ liệu kỹ thuật.

Gateway là lớp tiếp xúc trực tiếp với public comment và Messenger, nên rủi ro lộ dữ liệu cao hơn nhiều lớp khác.

## 114. PUBLIC-SAFE CHANNEL VIEW MATRIX

Nhóm dữ liệu

Public comment/live

Messenger đúng khách

Internal/Human view

Product public name

Có thể public nếu approved

Có

Có

Product public description

Có thể public-safe

Có

Có

Final price

Không public quote riêng

Có nếu QuoteSnapshot pass

Có

Quote expiry

Không public riêng nếu private quote

Có

Có

Order Draft

Không public

Có đúng khách/session

Có

Order code

Không public

Có đúng khách/session

Có

Payment instruction

Không public

Có đúng order/customer

Có

Bank account info

Không hardcode, chỉ theo instruction đúng kênh

Có nếu order/payment flow pass

Có

Payment proof

Không public

Không hiển thị lại nếu không cần

Có theo quyền

Payment status

Không public

Có đúng khách/order

Có

Full address

Không public

Có đúng khách nếu cần review

Có

Phone

Không public

Có đúng khách nếu cần

Có

Invoice/tax info

Không public

Có đúng khách nếu cần

Có

Member tier/benefit riêng

Không public

Có nếu runtime đúng khách

Có

Diamond referral private benefit

Không public

Có nếu resolver/quote pass

Có

Referral owner internal ID

Không public

Không public cho buyer

Có theo quyền

Commission/KPI/ROAS

Không public

Không public

Có theo quyền

Supplier/QC/costing/MISA

Không public

Không public

Có theo quyền

Internal trace

Không public

Không public

Có theo quyền

Public trace whitelist

Có nếu public trace pass

Có nếu pass

Có

Internal blocking reason

Không public

Public-safe only

Có

Stack/log/error

Không public

Không public

Có technical view

## 115. SECURITY / PRIVACY SMOKE

| Smoke ID | Nội dung | Expected Result ||---|---|| FB-SEC-SMK-001 | Public comment chứa địa chỉ | Gateway không lặp lại || FB-SEC-SMK-002 | Public hỏi order/payment | Không public dữ liệu riêng || FB-SEC-SMK-003 | Messenger sai session hỏi order | Bị chặn || FB-SEC-SMK-004 | Response chứa member tier ở public | Bị chặn || FB-SEC-SMK-005 | Response chứa commission/referral owner ID | Bị chặn || FB-SEC-SMK-006 | Response chứa supplier/QC/costing/MISA | Bị chặn || FB-SEC-SMK-007 | Internal blocking reason | Rewrite public-safe || FB-SEC-SMK-008 | Runtime error | Không lộ stack/log || FB-SEC-SMK-009 | Payment proof public | Bị chặn || FB-SEC-SMK-010 | Public Trace internal fields | Không trả |

## 116. SECURITY / PRIVACY FAIL GATE

Security / Privacy FAIL nếu:

Public comment/live lộ địa chỉ, số điện thoại, order, payment, hóa đơn.

Messenger trả order/payment sai khách.

Gateway public member tier/benefit riêng.

Gateway public Diamond/referral private data.

Gateway public commission/KPI/ROAS.

Gateway public supplier/QC/costing/MISA/internal trace.

Gateway public internal blocking reason.

Gateway lộ stack/log/error kỹ thuật.

Gateway hardcode bank account.

Gateway gửi response chưa redaction.

## 117. TYPING INDICATOR / RESPONSE PACING REVIEW

## 117.1. Mục tiêu

Typing Indicator / Response Pacing Review xác nhận Gateway tạo nhịp hội thoại tự nhiên, không máy móc, nhưng không giả mạo người thật.

## 117.2. Review checklist

Checklist

Yêu cầu

Channel capability

Kênh nào hỗ trợ typing indicator

Response length rule

Câu ngắn/vừa/dài có pacing khác nhau

Complexity rule

Quote/order/payment/complaint có pacing phù hợp

No instant reply

Không bật response ngay

Chunking rule

Câu dài được chia hợp lý nếu channel cho phép

Guard-before-send

Response/chunk phải guard pass trước gửi

No human impersonation

Không nói/ám chỉ là người thật

Runtime error

Không typing rồi bịa response

Evidence

Có smoke đo timing/pacing

Owner review

CX/Brand/Legal/Technical accepted

## 117.3. Fail Gate riêng

Typing/Pacing FAIL nếu:

Kênh hỗ trợ typing mà không có 3 chấm/typing.

Response bật ngay sau input.

Order Draft/Quote gửi tức thì như máy.

Response dài không chunk/pacing.

Typing indicator gửi trước khi guard pass theo cách có nguy cơ leak.

Typing indicator làm khách hiểu là người thật đang gõ.

Khách hỏi “người thật không?” nhưng Gateway/AI không minh bạch.

## 118. DOWNSTREAM READINESS RULE

## 118.1. Mục tiêu

Downstream Readiness Rule xác định khi nào TECH-07 Ads, TECH-08 MC AI Live, TECH-09 IVR và TECH-10 được phép phụ thuộc vào Facebook Gateway.

## 119. DOWNSTREAM DEPENDENCY MATRIX

Downstream

Chỉ được đi tiếp khi

Nếu Gateway chưa clear

TECH-07 Ads Measurement

Gateway event safe metrics, no unverified revenue, suppression clear

Không dùng Gateway data production

TECH-08 MC AI Live

Live comment boundary, Messenger handoff, suppression, pacing pass

Không live selling bằng Gateway production

## TECH-09 IVR

Official order eligibility, lock/recall signal, no private leak

Không nhận order confirmation từ Gateway

## CRM

Opt-out, fatigue, suppression, channel policy pass

Không gửi proactive qua Gateway

Human Support

Handoff route, evidence, failure handling pass

Không phụ thuộc Gateway handoff production

TECH-10 Completion / Release

Smoke pass + accepted evidence + owner sign-off

Gateway toàn hệ bị chặn

## 120. DOWNSTREAM BLOCKING REASONS

## 120.1. Blocking reason bắt buộc

Gateway phải bàn giao blocking reason rõ, có loại public-safe và internal-safe.

Blocking Reason

Ý nghĩa

## PAGE_NOT_APPROVED

Page chưa owner approved

## APP_REVIEW_NOT_APPROVED

App Review chưa approved

## PERMISSION_MISSING

Thiếu permission

## WEBHOOK_INVALID

Webhook không hợp lệ

## EVENT_DUPLICATE

Event trùng

## CHANNEL_UNKNOWN

Không xác định kênh

## PUBLIC_PRIVATE_BOUNDARY_RISK

Có rủi ro public/private

## CUSTOMER_SESSION_MISMATCH

Sai khách/session

## COMMENT_PRIVATE_HANDOFF_REQUIRED

Cần kéo Messenger

## PRIVATE_HANDOFF_FAILED

Kéo private thất bại

## AI_FINAL_GUARD_FAIL

AI Final Guard fail

## RESPONSE_REWRITE_REQUIRED

Cần rewrite response

## HUMAN_HANDOFF_REQUIRED

Cần người phụ trách

## SALE_LOCK_ACTIVE

Stop-sale active

## RECALL_ACTIVE

Recall active

## COMMERCE_BLOCKED

Commerce bị chặn

## QUOTE_SNAPSHOT_MISSING

Không có QuoteSnapshot

## PAYMENT_NOT_CONFIRMED

Chưa xác nhận thanh toán

## CRM_OPT_OUT

Khách không nhận tin

## RATE_LIMIT_ACTIVE

Rate limit active

## MODERATION_REQUIRED

Cần moderation

## RUNTIME_UNAVAILABLE

Runtime không khả dụng

## EVIDENCE_NOT_ACCEPTED

Evidence chưa accepted

## OWNER_REVIEW_REQUIRED

Cần owner review

## 121. RELEASE HANDOFF PACKAGE

## 121.1. Mục tiêu

Release Handoff Package là bộ hồ sơ bàn giao PACK-06 sang các TECH downstream và DEV implementation wave.

Handoff không phải “cho phép go-live”.

Handoff chỉ có nghĩa:

Tài liệu đã đủ rõ để dev lập implementation plan.

Các boundary đã khóa.

Các P0 STOP_POINTS đã khóa.

Các smoke/evidence/gate đã rõ.

Downstream biết phải phụ thuộc vào gì.

## 122. RELEASE HANDOFF CONTENT

Release Handoff Package của PACK-06 phải có:

Module contract list FB-M01 -> FB-M25.

Gateway lifecycle/state machine list.

Command-query boundary.

Meta Business / App / Page registry.

Permission / App Review matrix.

Webhook event map.

Page / Live Session map.

Public / Private Channel map.

Comment Intent / Trigger map.

Comment-to-Messenger rule.

Messenger private session rule.

Customer session binding rule.

AI Advisor handoff rule.

Final Response Guard consumer rule.

Channel Delivery rule.

Typing Indicator / Response Pacing rule.

No Instant Bot Reply rule.

No Human Impersonation rule.

Response Chunking rule.

Quote / Order / Payment Safe Delivery rule.

Public-safe View / Redaction rule.

Moderation / Abuse / Hide / Block rule.

Rate Limit / Anti-spam rule.

CRM / Proactive Messaging rule.

Human Handoff rule.

Runtime Suppression / Blocking Propagation rule.

Downstream Handoff rule.

Blocking Reason Registry.

Smoke matrix FB-SMK-A -> FB-SMK-L.

Evidence package FB-EVD-G01 -> FB-EVD-G28.

Owner review matrix.

Security/privacy/public-safe review checklist.

Typing/pacing review checklist.

App Review / Pilot / UAT checklist.

P0 STOP_POINTS registry.

Done Gate / Fail Gate.

Gap report template cho implementation.

Handoff note sang TECH-07 Ads.

Handoff note sang TECH-08 MC AI Live.

Handoff note sang TECH-09 IVR nếu scope có.

Handoff note sang TECH-10 Completion/Evidence/Release.

Statement rõ: Documentation Complete không phải Production Ready.

## 123. HANDOFF SANG TECH-07 ADS MEASUREMENT

## 123.1. Ads chỉ được nhận dữ liệu an toàn

Gateway event không phải revenue.

Comment không phải revenue.

Inbox không phải revenue.

Quote không phải revenue.

Order Draft không phải revenue.

Unpaid order không phải revenue.

Verified Revenue từ Commerce mới dùng cho ROAS.

Gateway chỉ bàn giao safe metrics nếu scope có.

Gateway phải bàn giao suppression status.

Gateway không được gửi dữ liệu private sai quyền cho Ads.

## 123.2. Ads không được dùng Gateway để vượt suppression

Ads không được scale/activate nếu:

SKU Sale Lock.

SKU Recall.

AI/Gateway suppression active.

Commerce bị chặn.

Claim/creative chưa approved.

Gateway evidence chưa pass.

Runtime unavailable.

## 124. HANDOFF SANG TECH-08 MC AI LIVE

Live comment public phải ngắn.

Live comment hỏi giá/mua phải kéo private.

Live không public quote riêng.

Live không public order/payment/private data.

Live không chốt SKU suppressed.

Live phải dùng AI safe response.

Live phải dùng Gateway typing/pacing trong Messenger/private.

Live phải tôn trọng Commerce QuoteSnapshot và quote expiry.

Live phải tôn trọng moderation/handoff.

## 124.2. MC AI Live không được bypass Gateway

MC AI Live không được:

Gửi comment trực tiếp ngoài Gateway.

Tự sửa AI response để thêm claim/giá.

Tự tạo Messenger handoff.

Tự gửi quote/order.

Tự bỏ suppression.

Tự spam comment.

Tự bỏ moderation.

## 125. HANDOFF SANG TECH-09 IVR ORDER CONFIRMATION

## 125.1. IVR chỉ nhận order context hợp lệ

Gateway không tạo order.

Gateway chỉ truyền order context đã official từ Commerce.

Gateway không truyền Order Draft như official order.

Gateway không truyền order đang Sale Lock/Recall.

Gateway không truyền payment waiting như paid.

IVR không xác nhận order bị bị chặn.

IVR không dùng comment/Messenger text thay CustomerConfirmation nếu Commerce không pass.

## 125.2. IVR không được nhận dữ liệu public/private sai quyền

IVR handoff phải có:

Order eligibility.

Customer contact context theo policy.

Lock/recall status.

Payment/order status.

Suppression status.

Evidence reference nếu cần.

Không handoff:

Comment public raw chứa dữ liệu nhạy cảm.

Order chưa official.

Order expired/cancelled.

Order bị Sale Lock/Recall.

Customer/session mismatch.

## 126. HANDOFF SANG TECH-10 COMPLETION / EVIDENCE / RELEASE

## 126.1. TECH-10 là nơi xét Completion / Release

PACK-06 không tự gọi:

Completion PASS.

Release Ready.

Release Approved.

Production Ready.

Go-live Approved.

PACK-06 chỉ bàn giao evidence và smoke sang TECH-10.

## 126.2. TECH-10 chỉ được xét khi có accepted evidence

TECH-10 chỉ được xét Gateway PASS khi có:

Meta App/Page/Permission evidence ACCEPTED.

App Review evidence ACCEPTED.

Webhook evidence ACCEPTED.

Live comment smoke ACCEPTED.

Messenger smoke ACCEPTED.

Comment-to-Messenger smoke ACCEPTED.

Typing/pacing smoke ACCEPTED.

Public/private privacy smoke ACCEPTED.

Moderation smoke ACCEPTED.

CRM opt-out smoke ACCEPTED.

Suppression smoke ACCEPTED.

Runtime unavailable smoke ACCEPTED.

Owner sign-off ACCEPTED.

Release decision record.

## 127. PACK-06 FINAL DONE GATE

## 127.1. Điều kiện Done Gate toàn PACK-06

PACK-06 chỉ được xem là Documentation Complete khi đủ các điều kiện sau:

## PHẦN 1/4 hoàn tất.

## PHẦN 2/4 hoàn tất.

## PHẦN 3/4 hoàn tất.

## PHẦN 4/4 hoàn tất.

Facebook Gateway Principles đã khóa.

Source-of-truth đã khóa.

Public / Private Channel Boundary đã khóa.

Comment-to-Messenger Rule đã khóa.

Messenger Private Flow đã khóa.

Typing Indicator / Response Pacing đã khóa.

No Instant Bot Reply đã khóa.

No Human Impersonation đã khóa.

Response Chunking đã khóa.

Meta App / Page / Permission Boundary đã khóa.

Webhook Intake Boundary đã khóa.

Live Session Boundary đã khóa.

AI Final Response Guard Consumer đã khóa.

Quote / Order / Payment Safe Delivery đã khóa.

Public-safe View / Redaction đã khóa.

Moderation / Abuse / Hide / Block đã khóa.

Rate Limit / Anti-spam đã khóa.

CRM / Proactive Messaging Boundary đã khóa.

Human Handoff Boundary đã khóa.

Runtime Suppression / Blocking Propagation đã khóa.

Downstream Handoff đã khóa.

FB-M01 -> FB-M25 module contracts đã khóa.

Gateway lifecycle/state machine đã khóa.

Command-query boundary đã khóa.

Runtime unavailable fail-safe đã khóa.

Final Smoke Matrix đã có.

Evidence Package đã có.

Owner Review Matrix đã có.

Security/Privacy/Public-safe Channel Review đã có.

Typing/Pacing Review đã có.

Release Handoff Package đã có.

Không có đoạn nào gọi Documentation Complete là Production Ready.

Không có đoạn nào cho phép Gateway tự tính giá.

Không có đoạn nào cho phép Gateway tự tạo order.

Không có đoạn nào cho phép Gateway tự xác nhận payment.

Không có đoạn nào cho phép Gateway gửi response chưa guard.

Không có đoạn nào cho phép Gateway public dữ liệu private.

Không có đoạn nào cho phép Gateway bỏ typing/pacing ở kênh hỗ trợ.

Không có đoạn nào cho phép Gateway giả mạo người thật.

Không có đoạn nào cho phép Gateway vượt Sale Lock / Recall / Suppression.

Không có đoạn nào cho phép CRM gửi khi opt-out.

Không có đoạn nào cho phép Evidence DRAFT dùng để PASS.

## 128. PACK-06 FINAL FAIL GATE

## 128.1. Điều kiện làm PACK-06 FAIL

PACK-06 FAIL nếu có bất kỳ điểm nào sau:

Gateway được viết như bot webhook trả lời tự do.

Page/App chưa approved vẫn production.

Permission/App Review missing vẫn bật feature.

Webhook invalid vẫn xử lý.

Duplicate event gây spam.

Public/private channel bị nhầm.

Public comment hỏi giá nhưng public quote riêng.

Public comment chốt mua tạo order.

Public comment lộ địa chỉ/số điện thoại/order/payment.

Handoff Messenger fail nhưng nói đã gửi.

Handoff Messenger success nhưng session không bind.

Messenger sau handoff vẫn xử lý như public comment.

Gateway gửi AI response chưa Final Guard.

Gateway tự sửa response để thêm giá/claim/order/urgency.

Gateway tự tính giá.

Gateway tự tạo QuoteSnapshot.

Gateway tự tạo Order Draft.

Gateway tự tạo official order.

Gateway tự cấp order_code.

Gateway tự xác nhận payment.

Gateway xem ảnh chuyển khoản là PAID.

Gateway hardcode bank account.

Gateway public trace khi trace bị chặn.

Gateway public supplier/QC/costing/MISA/internal trace.

Gateway không có typing indicator ở kênh hỗ trợ.

Gateway trả lời bật ngay như chatbot máy móc.

Gateway gửi Order Draft/Quote tức thì không pacing.

Gateway dùng typing indicator để giả mạo người thật.

Gateway không minh bạch khi khách hỏi có phải người thật không.

Chunking tách sai giá/expiry/order/payment.

Gateway spam comment/message.

Gateway bỏ qua rate limit/platform policy.

CRM gửi khi opt-out.

CRM upsell SKU Sale Lock/Recall.

Moderation comment abuse/tố cáo/khiếu nại vào sales flow thường.

Gateway tự kết luận chất lượng/pháp lý/payment dispute.

Human handoff bắt buộc nhưng không tạo.

Handoff fail nhưng nói đã chuyển.

Sale Lock/Recall không chặn Gateway.

Runtime unavailable nhưng Gateway suy đoán.

Ads nhận unverified revenue từ Gateway.

MC AI Live nhận SKU suppressed để chốt.

IVR nhận order chưa official/locked.

Evidence chưa accepted nhưng dùng để PASS.

Smoke chưa pass nhưng gọi Release Ready.

Owner chưa sign-off nhưng gọi Release Approved.

Chưa có release decision nhưng gọi Go-live Approved.

## 129. P0 STOP_POINTS REGISTRY — FINAL

## 129.1. Danh sách P0 STOP_POINTS cuối

## P0 ID

## STOP_POINTS

Gate bị chặn

## FB-P0-FINAL-001

Page/App/Permission chưa approved

Gateway Production

## FB-P0-FINAL-002

App Review missing

Meta Production

## FB-P0-FINAL-003

Webhook invalid/duplicate

Event Processing

## FB-P0-FINAL-004

Public/private channel nhầm

Channel Release

## FB-P0-FINAL-005

Public comment public giá/order/payment

Public Safety

## FB-P0-FINAL-006

Comment-to-Messenger fail nhưng nói đã gửi

Channel Trust

## FB-P0-FINAL-007

Messenger session mismatch

Customer Data

## FB-P0-FINAL-008

AI response chưa guard vẫn delivery

AI/Gateway Release

## FB-P0-FINAL-009

Gateway tự tính giá

Commerce Release

## FB-P0-FINAL-010

Gateway tự tạo order/order_code

Order Release

## FB-P0-FINAL-011

Gateway tự xác nhận payment/PAID

Payment Release

## FB-P0-FINAL-012

Bank account hardcoded

Payment Security

## FB-P0-FINAL-013

Không typing/pacing ở channel hỗ trợ

CX Release

## FB-P0-FINAL-014

Instant bot reply

CX / Brand Trust

## FB-P0-FINAL-015

Typing indicator giả mạo human

Legal / Trust

## FB-P0-FINAL-016

Chunking tách sai nghiệp vụ

Quote/Order/Payment

## FB-P0-FINAL-017

Public/private data leak

Security / Privacy

## FB-P0-FINAL-018

Abuse/tố cáo/khiếu nại vào sales flow

Moderation / CS

## FB-P0-FINAL-019

Rate limit/anti-spam fail

Platform Policy

## FB-P0-FINAL-020

CRM gửi khi opt-out

CRM / Privacy

## FB-P0-FINAL-021

Human handoff required nhưng không tạo

CS/QC/Finance

## FB-P0-FINAL-022

Sale Lock/Recall không chặn Gateway

Gateway toàn hệ

## FB-P0-FINAL-023

Runtime unavailable nhưng Gateway pass

Gateway toàn hệ

## FB-P0-FINAL-024

Ads nhận unverified revenue

Ads Release

## FB-P0-FINAL-025

MC AI Live nhận SKU suppressed

Live Release

## FB-P0-FINAL-026

IVR nhận order chưa official/locked

IVR Release

## FB-P0-FINAL-027

Evidence chưa accepted nhưng PASS

Completion / Release

## FB-P0-FINAL-028

Smoke thiếu nhưng Release Ready

Release Gateway

## FB-P0-FINAL-029

Owner chưa sign-off nhưng Release Approved

Release Governance

## 130. FINAL STATUS TABLE

## 130.1. Trạng thái sau khi hoàn tất PACK-06

Hạng mục

Trạng thái

## PACK-06 PHẦN 1/4

## WRITTEN_FOR_REVIEW

## PACK-06 PHẦN 2/4

## WRITTEN_FOR_REVIEW

## PACK-06 PHẦN 3/4

## WRITTEN_FOR_REVIEW

## PACK-06 PHẦN 4/4

## WRITTEN_FOR_REVIEW

## PACK-06 FULL DOCUMENT

## DOCUMENTATION_COMPLETE_AFTER_OWNER_REVIEW

## FACEBOOK_GATEWAY_TECHNICAL_SPEC

## LOCKED_DRAFT

## FACEBOOK_GATEWAY_MODULE_CONTRACTS

## DEFINED

## FACEBOOK_GATEWAY_STATE_MACHINE

## DEFINED

## FACEBOOK_GATEWAY_SMOKE_MATRIX

## DEFINED

## FACEBOOK_GATEWAY_EVIDENCE_PACKAGE

## DEFINED

## FACEBOOK_GATEWAY_RELEASE_HANDOFF

## DEFINED

## META_APP_REVIEW_STATUS

waiting_EVIDENCE

## WEBHOOK_TEST_STATUS

waiting

## LIVE_SMOKE_STATUS

waiting

## MESSENGER_SMOKE_STATUS

waiting

## TYPING_PACING_SMOKE_STATUS

waiting

## MODERATION_SMOKE_STATUS

waiting

## CRM_SMOKE_STATUS

waiting

## IMPLEMENTATION_STATUS

waiting

## TEST_STATUS

waiting

## SMOKE_STATUS

waiting

## EVIDENCE_STATUS

waiting_ACCEPTED_EVIDENCE

## OWNER_SIGN_OFF

waiting

## COMPLETION_PASS

## KHÔNG

## RELEASE_READY

## KHÔNG

## KHÔNG

## KHÔNG

## KHÔNG

bị chặn

## 131. DOWNSTREAM STATUS SAU PACK-06

## 131.1. TECH-07 Ads Measurement

Hạng mục

Trạng thái

TECH-07 có thể viết tiếp tài liệu

## CÓ

Ads được dùng Gateway conversation làm revenue

## KHÔNG

Ads được dùng quote/cart/order draft làm revenue

## KHÔNG

Ads chỉ dùng Verified Revenue từ Commerce

## CÓ

Ads phải nhận suppression status

## CÓ

Ads được scale SKU bị chặn

## KHÔNG

Ads cần Gateway event safe metrics

## CÓ

## 131.2. TECH-08 MC AI Live

Hạng mục

Trạng thái

TECH-08 có thể viết tiếp tài liệu

## CÓ

MC AI Live được gửi comment trực tiếp ngoài Gateway

## KHÔNG

MC AI Live được public quote riêng

## KHÔNG

MC AI Live được chốt SKU suppressed

## KHÔNG

MC AI Live phải dùng AI safe response

## CÓ

MC AI Live phải dùng Gateway comment/private handoff

## CÓ

MC AI Live phải tôn trọng typing/pacing trong private flow

## CÓ

MC AI Live phải tôn trọng moderation/rate limit

## CÓ

## 131.3. TECH-09 IVR nếu scope có

Hạng mục

Trạng thái

TECH-09 có thể viết tiếp tài liệu

## CÓ

IVR được nhận Order Draft như official order

## KHÔNG

IVR được nhận order bị Sale Lock/Recall

## KHÔNG

IVR được xác nhận payment

## KHÔNG

IVR được verified revenue

## KHÔNG

IVR phải nhận order eligibility từ Commerce/Gateway safe view

## CÓ

IVR phải tôn trọng lock/recall/customer session

## CÓ

## 131.4. CRM / Human Support

Hạng mục

Trạng thái

CRM Gateway boundary đã xác định

## CÓ

CRM được gửi khi opt-out

## KHÔNG

CRM được gửi SKU Sale Lock/Recall

## KHÔNG

CRM phải qua rate limit/fatigue

## CÓ

Human handoff trigger đã xác định

## CÓ

Gateway được tự xử lý complaint nghiêm trọng

## KHÔNG

Gateway được nói đã handoff thành công khi fail

## KHÔNG

## 131.5. TECH-10 Completion / Evidence / Release

Hạng mục

Trạng thái

TECH-10 có thể nhận handoff từ PACK-06

## CÓ

TECH-10 được PASS nếu evidence draft

## KHÔNG

TECH-10 được Release Ready nếu smoke thiếu

## KHÔNG

TECH-10 cần Meta/App/Webhook/Live/Messenger/Typing evidence

## CÓ

TECH-10 cần owner sign-off

## CÓ

Gateway toàn hệ mặc định

bị chặn

## 132. IMPLEMENTATION READINESS NOTE

## 132.1. PACK-06 chưa cho phép code sâu ngay

PACK-06 là tài liệu kỹ thuật nền.

Sau PACK-06, chưa nên nhảy vào code sâu nếu chưa có:

TECH-07 Ads Measurement.

TECH-08 MC AI Live.

TECH-09 IVR nếu scope triển khai.

TECH-10 Completion / Evidence / Release.

Meta Page/App/Permission mapping.

Webhook event mapping.

Page/live/session mapping.

Comment-to-Messenger mapping.

Typing/pacing implementation plan.

Rate limit/anti-spam plan.

Moderation policy plan.

CRM opt-out/fatigue plan.

Gap report.

Implementation wave plan.

Test plan chi tiết.

Environment plan.

Owner sign-off.

## 132.2. Việc code phải đi theo wave

Facebook Gateway thuộc:

Wave 5 — Facebook Gateway

Wave 5 chỉ nên bắt đầu sau:

Wave 0 Foundation clear.

Wave 1 Product/SKU/Recipe clear.

Wave 2 Operational Core clear.

Wave 3 Commerce Runtime clear.

Wave 4 AI Advisor clear.

PACK-06 accepted for implementation planning.

Module map FB-M01 -> FB-M25 rõ.

Meta App/Page/Permission mapping rõ.

P0 STOP_POINTS registry rõ.

Evidence/smoke plan rõ.

## 133. PACK-06 FINAL RELEASE HANDOFF STATEMENT

## 133.1. Statement bàn giao

PACK-06 bàn giao cho các tài liệu tiếp theo với thông điệp:

Facebook Gateway là lớp điều phối kênh và delivery, không phải AI, không phải Commerce, không phải Payment, không phải nơi tự tạo sự thật nghiệp vụ.

Gateway phải bảo vệ:

Public/private boundary.

Comment-to-Messenger flow.

Messenger private session.

AI Final Response Guard.

Commerce safe view.

Sale Lock / Recall / Suppression.

Typing indicator / response pacing.

No Instant Bot Reply.

No Human Impersonation.

Rate limit / anti-spam.

Moderation.

CRM opt-out.

Human handoff.

Evidence / App Review / Release Gate.

Không downstream nào được dùng Gateway production nếu PACK-06 chưa có implementation thật, smoke thật, evidence accepted và owner release decision.

## 134. PACK-06 FINAL CONCLUSION

PACK-06 đã khóa đầy đủ tầng Facebook Gateway / Meta Live & Messenger cho Ginsengfood theo hướng greenfield.

Tài liệu đã xác định rõ:

Facebook Gateway đi sau TECH-05 AI Advisor.

Facebook Gateway đi trước TECH-07 Ads, TECH-08 MC AI Live, TECH-09 IVR.

Gateway không thay AI Advisor.

Gateway không thay Commerce Runtime.

Gateway không thay Operational Core.

Gateway không thay Payment/Finance.

Gateway không thay Public Trace.

Gateway không thay Human Owner.

Gateway không tự tính giá.

Gateway không tự tạo order.

Gateway không tự xác nhận thanh toán.

Gateway không gửi AI response chưa qua Final Response Guard.

Public comment không public dữ liệu private.

Comment hỏi giá/mua/tư vấn sâu phải kéo Messenger/private theo rule.

Sau Messenger handoff thành công, conversation xử lý như private.

Gateway phải có typing indicator / 3 chấm ở kênh hỗ trợ.

Gateway phải có response pacing theo độ dài/độ phức tạp response.

Gateway không được trả lời bật ngay như chatbot máy móc.

Gateway không được giả mạo người thật.

Gateway phải minh bạch nếu khách hỏi bản chất hệ thống.

Gateway phải tôn trọng Sale Lock / Recall / Suppression.

Gateway phải tôn trọng CRM opt-out.

Gateway phải có moderation cho abuse/tố cáo/khiếu nại.

Gateway phải có rate limit / anti-spam.

Gateway phải có Meta Page/App/Permission/App Review evidence.

Gateway phải có live/Messenger/typing/pacing smoke.

Evidence chưa ACCEPTED thì không PASS.

Smoke chưa PASS thì không Release Ready.

Owner chưa sign-off thì không Release Approved.

Chưa có release decision thì không Go-live Approved.

Kết luận cuối cùng:

PACK-06 hoàn tất ở cấp tài liệu kỹ thuật nền, đủ để chuyển sang review và làm cơ sở viết TECH-07 Ads Measurement.

Nhưng PACK-06 hiện vẫn là:

OWNER_SIGN_OFF = waiting

## COMPLETION_PASS = KHÔNG

## RELEASE_READY = KHÔNG

Facebook Gateway chỉ được production-ready khi đã có implementation thật, Meta/App/Page cấu hình thật, App Review/permission thật, webhook test thật, live/Messenger smoke thật, typing/pacing evidence thật, moderation/rate-limit/CRM smoke thật, evidence accepted, owner sign-off và release decision hợp lệ.

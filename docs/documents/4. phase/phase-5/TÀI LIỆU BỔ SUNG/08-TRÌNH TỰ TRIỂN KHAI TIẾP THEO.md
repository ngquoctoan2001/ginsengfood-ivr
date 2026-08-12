**TRÌNH TỰ TRIỂN KHAI TIẾP THEO**

**Giai đoạn 1 - Khóa nền kinh doanh**

**Bước 1:** Chốt bản giá/ADS hiện tại làm nền.  
Gồm: Giờ Vàng Tri Ân, 24/7, combo 2-3 hộp/đơn, Diamond, VAT, ship, COD, Ads cost 20% → 17% → 15%.

**Bước 2:** Chốt KPI vận hành 6 tháng.  
Tháng 1-2: 2.000 đơn/ngày.  
Tháng 3-4: 4.000 đơn/ngày.  
Tháng 5-6: 8.000 đơn/ngày.

**Bước 3:** Chốt chỉ số bắt buộc để tính Ads.  
Cần có: COGS/hộp, tỷ lệ hủy/COD fail, tỷ lệ hoàn, phí đóng gói, phí nhân sự live, phí vận hành kho.

**Giai đoạn 2 - Viết tài liệu kỹ thuật bổ sung**

**Bước 4:** Viết tài liệu mới:

**FACEBOOK ADS & LIVE COMMERCE OPERATING MODEL**

Nội dung phải gồm:

- Facebook Page Registry.
- Live Session Registry.
- Comment → Messenger Flow.
- Messenger → AI Advisor → Quote → Order Flow.
- Ads Campaign / Adset / Ad ID Tracking.
- Pixel / CAPI / Offline Conversion Mapping.
- Diamond Recruitment Funnel.
- ROAS / CPA / AOV Dashboard.
- Anti-spam / suppression / frequency cap.
- Meta-safe wording cho Giờ Vàng Tri Ân.

**Giai đoạn 3 - Cập nhật lõi Ginsengfood**

**Bước 5:** Bổ sung Ads Attribution vào lõi.  
Các bảng/field cần có:

- campaign_id
- adset_id
- ad_id
- page_id
- live_session_id
- messenger_thread_id
- referral_link_id
- diamond_id
- quote_snapshot_id
- order_code
- order_verified_at

**Bước 6:** Bổ sung Ads Measurement Event.  
Các event tối thiểu:

- LIVE_VIEW
- LIVE_COMMENT
- MESSENGER_STARTED
- AI_PROPOSAL_SENT
- QUOTE_CREATED
- QUOTE_SENT
- ORDER_CONFIRMATION_SENT
- ORDER_CREATED
- ORDER_VERIFIED
- CRM_REORDER
- DIAMOND_LEAD_CREATED

**Giai đoạn 4 - Cập nhật AI Advisor**

**Bước 7:** Thêm Facebook/Ads context vào AI runtime.  
AI phải biết khách đến từ:

- Page nào
- Live nào
- Ads nào
- Campaign nào
- Diamond link nào
- Comment hay Messenger
- Khách mới hay member

**Bước 8:** Thêm logic chốt combo cho Giờ Vàng Tri Ân.  
AI không chỉ bán 1 hộp, mà phải ưu tiên:

- combo ba mẹ
- combo gia đình
- combo chồng/phái mạnh
- combo quà sức khỏe
- combo ăn chay
- combo trải nghiệm 3 dòng

**Giai đoạn 5 - Facebook Channel Gateway**

**Bước 9:** Triển khai Gateway kỹ thuật.  
Bắt buộc có:

- Meta App
- Webhook
- Page Access Token
- Verify Token
- App Secret
- Signature validation
- Page Registry
- Event dedup/idempotency
- Live comment ingestion
- Messenger handoff
- Outbound policy

**Bước 10:** Chỉ bật trước cho Page chính:

**Ginsengfood - Cháo Sâm Mỗi Ngày**

Không bật đồng loạt tất cả Page ngay.

**Giai đoạn 6 - Ads Funnel**

**Bước 11:** Chia Ads thành 2 tuyến.

**Tuyến A - Giờ Vàng Tri Ân**

Mục tiêu:

- kéo live;
- kéo comment;
- kéo Messenger;
- tạo quote;
- tạo đơn combo.

**Tuyến B - Diamond Đồng Hành Khởi Nghiệp**

Mục tiêu:

- tuyển Diamond;
- tạo cộng đồng;
- kéo referral;
- giảm phụ thuộc Ads trực tiếp.

**Giai đoạn 7 - Test trước khi chạy thật**

**Bước 12:** Test live giả lập.  
Phải test đủ:

- comment hỏi giá;
- comment để số điện thoại;
- comment hỏi bệnh nền;
- comment hỏi mua;
- Messenger tư vấn;
- quote Giờ Vàng;
- quote hết hạn 5 phút;
- chốt đơn;
- tạo order;
- CRM sau mua;
- Diamond link attribution.

**Bước 13:** Test chống spam.  
Bắt buộc pass:

- public reply 1 lần/comment;
- private reply 1 lần/comment đủ điều kiện;
- không public giá cuối;
- không lặp PII;
- không gửi CRM trong quiet hour;
- không gửi CRM khi opt-out;
- không upsell khi complaint/counterfeit/privacy case open.

**Giai đoạn 8 - Chạy pilot**

**Bước 14:** Pilot 7-14 ngày.  
Không scale mạnh ngay.

Theo dõi:

- CPM
- CPC
- comment rate
- inbox rate
- quote rate
- order rate
- AOV
- hộp/đơn
- CPA
- ROAS
- COD fail rate
- hủy đơn
- Diamond lead rate

**Giai đoạn 9 - Scale theo điều kiện**

**Bước 15:** Chỉ scale khi đạt gate.

Gate tối thiểu:

- AI quote đúng;
- không leak PII;
- không spam;
- AOV đạt 2 hộp/đơn trở lên;
- CPA nằm trong ngưỡng;
- đơn tạo đúng;
- ORDER_VERIFIED tracking đúng;
- dashboard đo được ROAS thật.

**Kết luận triển khai**

Thứ tự đúng là:

**Khóa tài chính → viết tài liệu Facebook Ads/Live → cập nhật lõi attribution → cập nhật AI runtime → triển khai Gateway → test → pilot → scale.**

Bước tiếp theo nên làm ngay: **viết FILE FACEBOOK ADS & LIVE COMMERCE OPERATING MODEL bản sạch để giao dev/marketing/ads team cùng triển khai.**

**4\. Có nên chuyển sang Facebook Channel Gateway chưa?**

**Chưa nên chuyển ngay sang Gateway production/config chi tiết.**

Nên làm bước đệm:

- **Dọn gói tài liệu**: chỉ giữ bản V2.4 hiện hành; đưa bản V2.3 vào legacy.
- **Chạy Cross-file Consistency Matrix** trong FILE 11.
- **Lập AI Advisor Facebook Completion Report**.
- **Đánh dấu PASS/FAIL cho các test P0 trong FILE 09**.
- **Owner ký Documentation Handoff + AI Advisor Facebook Completion Sign-off**.
- Sau đó mới bắt đầu: **Facebook Channel Gateway / Meta Live & Messenger**.

Nói ngắn gọn: **bộ tài liệu AI Advisor đã đủ mạnh để chuẩn bị sang Gateway, nhưng cổng chuyển Gateway chỉ nên mở sau khi có report PASS, không chỉ dựa vào việc tài liệu đã viết xong.**

**5\. Kết luận cuối**

Bộ FILE 00-12 hiện tại đã đạt mức **rất cao về tư duy hệ thống**, không còn là bộ prompt/content rời rạc. Nếu dev triển khai đúng, AI Advisor có thể vận hành như một hệ tư vấn bán hàng thật: hiểu khách, hiểu sản phẩm, nhận diện tình huống, tư vấn sâu, quote đúng gate, chốt đơn có xác nhận, chăm sóc sau mua, xử lý Facebook/Ads/Live đúng public/private và không để FILE 12/Gateway vượt guard.

Điểm tôi chốt lại trước khi sang Facebook Channel Gateway:

**Đạt để Owner review và chuẩn bị Dev/QA handoff.  
Chưa đạt để mở Gateway nếu chưa chạy AI Advisor Facebook Completion Gate bằng evidence thật.  
Cần dọn bản cũ V2.3 khỏi gói bàn giao để tránh nhầm source-of-truth.**

………………………………………………………………………..

## 3\. Để thành "bộ kết nối Facebook vận hành tốt nhất", cần làm gì?

Tôi đề xuất viết tiếp thành **bộ 08 file kỹ thuật Facebook Channel Gateway**.

# BỘ FACEBOOK CHANNEL GATEWAY NÊN VIẾT TIẾP

## FILE FB-00 - README / SOURCE OF TRUTH / GATEWAY BOUNDARY

Mục tiêu: khóa vai trò Gateway.

Nội dung cần có:

- Gateway là lớp hạ tầng kênh.
- Gateway nhận Meta webhook, normalize event, dedup, kiểm page policy, route vào AI Advisor.
- Gateway không tự chọn sản phẩm.
- Gateway không tự báo giá.
- Gateway không tự tạo quote/order.
- Gateway không tự gửi CRM.
- Gateway không bypass AI Advisor.
- Gateway chỉ production sau khi AI Advisor Facebook Completion Report PASS.

Đây là file quan trọng nhất để tránh dev hiểu sai Gateway là một chatbot Facebook riêng.

## FILE FB-01 - META APP / BUSINESS / PAGE REGISTRY / TOKEN GOVERNANCE

Mục tiêu: biến Page Registry hiện có thành cấu hình kỹ thuật.

Nội dung cần có:

- Meta Business/App setup.
- App ID, App Secret, Verify Token.
- Page Access Token registry.
- Token storage bằng secret ref, không ghi token vào tài liệu.
- Page Registry chính thức:
  - Commerce Hub
  - Ads/Live Spoke
  - Audience Spoke
  - Test Page
- Capability theo phase.
- Page outbound policy.
- Page subscription status.
- Token rotation.
- App Review requirement.
- RBAC admin.

Cần dùng file danh sách Page hiện tại làm nguồn, nhưng phải chuẩn hóa lại trường:

Page ID / Profile ID / Business ID / App ID

Vì App ID hiện đang ghi "CHƯA TẠO - DEV TẠO TRONG META FOR DEVELOPERS".

## FILE FB-02 - WEBHOOK / EVENT NORMALIZATION / DEDUP / IDEMPOTENCY

Mục tiêu: dev biết nhận webhook và chuẩn hóa event thế nào.

Nội dung cần có:

- Webhook endpoint.
- Verify token handshake.
- Signature validation.
- Raw webhook storage policy.
- Event normalization.
- Event ID.
- Idempotency key.
- Dedup theo:
  - page_id
  - comment_id
  - message_id
  - live_session_id
  - webhook_delivery_id nếu có
- Retry policy.
- Dead-letter queue.
- Delivery log.
- Rate limit.
- Error handling.

Tài liệu hiện tại đã nêu Gateway phải có webhook, signature validation, Page Access Token registry, event normalization, dedup/idempotency, retry worker và delivery log.

## FILE FB-03 - LIVE SESSION REGISTRY / COMMENT → MESSENGER HANDOFF

Mục tiêu: live/comment phải đi đúng boundary.

Nội dung cần có:

- Live Session Registry.
- Live type:
  - GOLDEN_HOUR_TRI_AN
  - DIAMOND_RECRUITMENT
  - EDUCATION
  - TEST
- Comment ingestion.
- Public reply policy.
- 1 public reply/comment.
- Private reply từ comment: 1 lần/comment đủ điều kiện.
- Handoff context.
- Không public:
  - giá cuối
  - số điện thoại
  - địa chỉ
  - order confirmation
  - payment
  - health-sensitive
- Handoff sang Commerce Hub.
- Attribution retention khi chuyển từ Spoke → Hub.

Tài liệu hiện tại đã có flow public comment hỏi giá, để số điện thoại, hỏi bệnh nền và đều kéo về Messenger/private; không public giá, không lặp PII, không tư vấn health-sensitive sâu trên public.

## FILE FB-04 - MESSENGER THREAD / AI ADVISOR ROUTING / HUB-SPOKE HANDOFF

Mục tiêu: Messenger không được rối giữa nhiều Page.

Nội dung cần có:

- Commerce Hub Messenger là nơi chốt chính.
- Page Spoke không có Messenger sales flow riêng.
- Handoff từ Spoke về Hub.
- Messenger thread binding.
- PSID/internal customer mapping.
- Guest/customer identity.
- Conversation state.
- Active window policy.
- CRM suppression.
- Không để nhiều Page cùng nhắn lặp cho một khách.
- Nếu khách từ Page phụ sang Hub, vẫn giữ source_page_id.

Tài liệu hiện tại đã khóa: mọi Page Spoke phải redirect về Commerce Hub, mọi Messenger/Quote/Order phải lưu commerce_hub_page_id, mọi attribution lưu source_page_id, và không để nhiều Page cùng nhắn lặp cho một khách.

## FILE FB-05 - ADS ATTRIBUTION / PIXEL / CAPI / OFFLINE CONVERSION / ORDER_VERIFIED

Mục tiêu: đo Ads đúng doanh thu thật.

Nội dung cần có:

- Ads campaign/adset/ad tracking.
- UTM mapping.
- Pixel event mapping.
- CAPI event mapping.
- Offline conversion event mapping.
- ORDER_VERIFIED là event tính ROAS chính.
- Không tính ROAS theo:
  - comment
  - inbox
  - quote
  - order chưa verified
- Attribution chain:
  - campaign
  - adset
  - ad
  - page
  - live session
  - comment
  - Messenger thread
  - quote
  - order
  - ORDER_VERIFIED
- Event match quality fields.
- Dedup between Pixel/CAPI/offline nếu triển khai.
- Privacy/hash policy.

File trình tự triển khai đã nêu rõ phần còn cần có: Pixel / CAPI / Offline Conversion Mapping, Ads Campaign / Adset / Ad ID Tracking, và Ads Measurement Event. Tài liệu operating model cũng khóa ROAS chỉ tính bằng ORDER_VERIFIED revenue, không tính comment, inbox, quote hoặc order chưa verified.

## FILE FB-06 - DIAMOND / REFERRAL / COMMISSION ATTRIBUTION

Mục tiêu: Diamond attribution không bị lẫn với Ads và Live.

Nội dung cần có:

- Diamond lead.
- Referral link.
- Bind valid/invalid.
- Self-purchase policy.
- Commission eligibility.
- 24/7 commission 15%.
- Giờ Vàng commission 10%.
- PIT tax 10%.
- DIAMOND_REFERRAL_ORDER_VERIFIED.
- Conflict handling:
  - Ads + Diamond
  - Live + Diamond
  - Existing member + Diamond link
- Không public commission/payout cho khách thường.

Baseline tài chính đã khóa Diamond commission chỉ tính khi referral bind hợp lệ, không tính hoa hồng cho self-purchase nếu policy chặn, và Diamond attribution phải đo được trong dashboard.

## FILE FB-07 - ROAS / CPA / AOV DASHBOARD / SCALE GATE

Mục tiêu: chỉ scale khi đo đúng.

Nội dung cần có:

- Dashboard metrics:
  - Ads Spend
  - Revenue Verified
  - ROAS
  - CPA
  - AOV
  - Boxes per Order
  - Comment Rate
  - Inbox Rate
  - Quote Rate
  - Order Rate
  - Verified Rate
  - COD Fail Rate
  - CRM Revenue
  - Diamond Revenue
- Break-even ROAS.
- Ads ratio target:
  - tháng 1-2: 20%
  - tháng 3-4: 17%
  - tháng 5-6: 15%
- Scale Gate:
  - Quote Snapshot hoạt động đúng
  - không leak PII
  - không spam
  - AOV ≥ 2 hộp/đơn
  - CPA trong ngưỡng
  - ORDER_VERIFIED tracking đúng
  - ROAS đo thật

Tài liệu hiện tại đã có dashboard bắt buộc gồm Ads Spend, Revenue Verified, ROAS, CPA, AOV, Boxes per Order, comment/inbox/quote/order/verified rates, CRM Revenue và Diamond Revenue.

## FILE FB-08 - SECURITY / APP REVIEW / TEST / PILOT / RELEASE / ROLLBACK

Mục tiêu: Gateway không bật production khi chưa an toàn.

Nội dung cần có:

- App Review checklist.
- Permissions checklist.
- Test Page only.
- Sandbox/staging/prod environment.
- Secret management.
- Signature validation test.
- Token rotation.
- Webhook retry test.
- Duplicate webhook test.
- Live giả lập.
- Pilot 7-14 ngày.
- Rollback flags:
  - disable live public reply
  - disable private reply
  - disable Messenger handoff
  - disable CRM
  - disable order creation
- Release gate.
- Incident handling.

File trình tự đã yêu cầu test live giả lập, test chống spam, pilot 7-14 ngày, rồi chỉ scale khi đạt gate về quote, PII, spam, AOV, CPA, order tracking và ROAS thật.

## 4\. Những điểm hiện còn thiếu nếu muốn dev làm ngay

Hiện các tài liệu Facebook đã tốt về chiến lược, nhưng để dev làm Gateway tốt nhất còn thiếu các phần kỹ thuật sau:

### Thiếu 1 - Gateway API contract

Cần định nghĩa rõ:

POST /api/channel/meta/webhook  
GET /api/channel/meta/webhook/verify  
POST /api/admin/facebook/pages  
POST /api/admin/facebook/pages/{id}/subscribe  
POST /api/admin/facebook/live-sessions  
GET /api/admin/facebook/events  
GET /api/admin/facebook/attribution

### Thiếu 2 - Database object contract

Cần định nghĩa bảng/object:

facebook_page_registry  
meta_app_registry  
page_token_secret_ref  
meta_webhook_event  
normalized_channel_event  
live_session  
comment_handoff  
messenger_thread_binding  
ads_attribution_context  
ads_measurement_event  
order_attribution_snapshot  
diamond_referral_attribution  
gateway_outbound_log  
gateway_dedup_key  
gateway_retry_queue

### Thiếu 3 - CAPI / Offline Conversion contract

Hiện tài liệu có nhắc cần Pixel/CAPI/Offline Conversion, nhưng chưa đủ contract dev triển khai.

Cần viết rõ:

- event nào gửi Pixel;
- event nào gửi CAPI;
- event nào gửi Offline Conversion;
- event nào chỉ lưu nội bộ;
- dedup key là gì;
- revenue value lấy từ đâu;
- chỉ gửi ORDER_VERIFIED hay cả ORDER_CREATED;
- dữ liệu khách hash thế nào;
- retry/fail log thế nào.

### Thiếu 4 - App Review / Permission checklist

Hiện đã ghi App ID chưa tạo, dev tạo trong Meta for Developers. Nhưng cần thêm:

- App Mode dev/live.
- Permission cần xin.
- Use case cho từng permission.
- Screencast/evidence App Review.
- Test user/page.
- Webhook subscribed fields.
- Chính sách dữ liệu.

Phần này nên viết trong FILE FB-08.

### Thiếu 5 - Runbook vận hành

Cần có runbook:

- khi webhook fail;
- khi token hết hạn;
- khi duplicate comment;
- khi Page bị block outbound;
- khi Messenger handoff fail;
- khi ROAS dashboard lệch;
- khi CAPI gửi lỗi;
- khi live có spam;
- khi khách public PII;
- khi khách hỏi bệnh nền public;
- khi Page phụ nhắn trùng.

## 5\. Có cần sửa các tài liệu hiện tại không?

### Không cần viết lại toàn bộ

Nhưng cần làm 3 việc nhỏ:

#### Việc 1 - Đổi tên tài liệu hiện có thành baseline/source

Ví dụ:

FB-SRC-00 - Business & Ads Financial Baseline Lock  
FB-SRC-01 - Facebook Ads & Live Commerce Operating Model  
FB-SRC-02 - Facebook Page List / Owner Decisions  
FB-SRC-03 - Ads Cost Model / Live Commerce Scale Model  
FB-SRC-04 - Pricing / Golden Hour / Diamond Model  
FB-SRC-05 - Financial Analysis  
FB-SRC-06 - Implementation Sequence

Như vậy dev hiểu đây là **source baseline**, không phải Gateway technical spec cuối cùng.

#### Việc 2 - Viết mới bộ FB-00 → FB-08

Đây mới là bộ giao dev để xây Gateway.

#### Việc 3 - Gắn cross-reference với AI Advisor FILE 00→12

Trong FB-00 phải ghi rõ:

Facebook Channel Gateway chỉ bắt đầu sau AI Advisor Facebook Completion Gate PASS.  
Gateway không được bypass AI Advisor.  
Gateway không được tự quote/order/CRM.

## 6\. Trạng thái quyết định

FACEBOOK_DOCUMENT_REVIEW_RESULT:  
existing_documents:  
status: PASS_AS_BUSINESS_OPERATING_BASELINE  
meaning: "Đủ làm nền chiến lược, tài chính, Page Registry, Ads/Live operating model."  
<br/>rewrite_all:  
required: false  
reason: "Không cần viết lại từ đầu."  
<br/>continue_next:  
required: true  
next_step: "Write Facebook Channel Gateway Technical Dev Pack"  
<br/>missing_for_dev_gateway:  
\- Meta App / Token / App Review contract  
\- Webhook API / signature / verify token / retry contract  
\- Event normalization / dedup / idempotency  
\- Page Registry DB object  
\- Live Session Registry DB object  
\- Comment to Messenger Handoff contract  
\- Messenger Thread Binding  
\- Ads Attribution / Pixel / CAPI / Offline Conversion  
\- ORDER_VERIFIED measurement pipeline  
\- ROAS / CPA / AOV dashboard data contract  
\- Security / RBAC / audit / monitoring / rollback  
\- Gateway test matrix / pilot / scale gate  
<br/>facebook_gateway_production:  
status: NOT_ALLOWED_YET  
reason: "Chỉ production sau AI Advisor Facebook Completion Report PASS."

## Kết luận cuối

**Đi tiếp, không viết lại toàn bộ.**

Nhưng bước tiếp theo không nên là "bổ sung vài đoạn" vào tài liệu cũ. Bước đúng là viết mới một bộ:

FACEBOOK CHANNEL GATEWAY / META LIVE & MESSENGER - TECHNICAL DEV PACK

Bộ này sẽ dùng các tài liệu anh gửi làm **source baseline**, rồi chuyển thành hợp đồng kỹ thuật cho dev triển khai Gateway thật.

Tôi đề xuất bắt đầu bằng:

FILE FB-00 - FACEBOOK CHANNEL GATEWAY / META LIVE & MESSENGER - README / SOURCE OF TRUTH / GATEWAY BOUNDARY

Sau đó viết tiếp FB-01 đến FB-08 như trên.

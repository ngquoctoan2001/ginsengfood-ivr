# P4.2F - CHANNEL HANDOFF MESSENGER LIVE CRM BRIDGE

## Khối nguồn bắt buộc đã nhập vào file này

File này có khối nguồn bắt buộc để chạy lẻ, nhưng khi bàn giao Phase 4 phải đọc `12-BẢNG GOM GIAI ĐOẠN 4.md`, `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` trước. Nếu nội dung file này khác Bảng Gôm/phụ lục/SRS handoff, áp dụng Bảng Gôm/phụ lục/SRS handoff và xử lý fail-closed.

### A. Nội dung nguồn đã nhập

1. Master/runtime governance: channel handoff không tạo business truth. Product/Operational/Commerce upstream vẫn thắng kênh. AI Advisor chỉ quyết response candidate và handoff intent từ dữ liệu hợp lệ. Gateway chỉ nhận/normalize/route/deliver/log evidence, không tư vấn thay AI.
2. Evidence/release governance: handoff bridge pass cục bộ không phải Gateway PASS, Completion PASS, Release Ready, Production Ready hoặc Go-live. Gateway production mặc định PRODUCTION_BLOCKED cho đến khi AI evidence, Gateway evidence, E2E smoke, Meta/App/Page permission và owner sign-off pass.
3. AI Advisor channel lock: AI được phân loại surface public/private, giữ continuity, tạo handoff intent, route Messenger/private/human/CRM khi policy cho phép và preserve channel context.
4. Commerce lock: public comment/live không public final price, payment, PII hoặc order state. Final price chỉ từ QuoteSnapshot private-safe. Order success chỉ khi có CustomerConfirmation và order_code. Paid chỉ từ Payment Core.
5. Product/Operational lock: recall/sale lock/not sellable/channel suppression thắng mọi Live, Ads, CRM, AI và Gateway intent.
6. CRM/Member lock: CRM follow-up chỉ khi suppression pass: opt-out, quiet hour, open case, complaint, fatigue, frequency cap, eligibility. Không gửi proactive message nếu bất kỳ trạng thái bắt buộc nào fail/unknown/pending.
7. Finance/Diamond lock: Gateway/AI chỉ preserve referral/attribution context; không tính commission, PIT, payable hoặc payout.
8. Ads/Live lock: Ads context là attribution internal-only; ROAS chỉ từ ORDER_VERIFIED. Live/MC AI dùng daily live board/script brief; comment hỏi giá/mua/tư vấn sâu phải private nếu policy yêu cầu.

### B. Conflict lock

- Public hỏi giá/mua: safe ack + private handoff nếu allowed; không public final quote.
- Handoff fail: không nói đã gửi Messenger; ghi fail/retry/human route.
- Unknown page/app/token/permission: quarantine/block, không fallback gửi thay nếu chưa có owner-approved routing.
- Spam/troll/abuse/complaint: rate-limit, suppress hoặc human route; không auto-sales.
- CRM suppression fail: không gửi CRM.
- Ads/ROAS/Diamond conflict: preserve attribution only; không tạo revenue/commission.
- Evidence thiếu delivery/handoff log hoặc idempotency key: giữ PENDING_EVIDENCE, không gọi PASS.

### C. Trạng thái evidence bắt buộc

Trạng thái mặc định của file này: CHANNEL_HANDOFF_LOCK_WITH_IMPORTED_SOURCE. Evidence tối thiểu phải có channel_context, public/private policy, handoff decision, suppression result, delivery/handoff result nếu có, idempotency key, retry/dead-letter state và masked PII/secret status. Không được gọi GATEWAY_PASS, COMPLETION_PASS, RELEASE_READY, PRODUCTION_READY hoặc GO_LIVE_APPROVED.

### D. Khóa riêng của file này

File 07 chỉ xử lý cầu nối bàn giao kênh từ phía AI Advisor. Nhiệm vụ là quyết định public/private/Messenger/Live/CRM/human handoff đúng chính sách, nhưng không mở Gateway production và không gửi thay Gateway nếu evidence chưa đủ.

## Nội dung triển khai

## PROMPT-P4-2F - CHANNEL HANDOFF / MESSENGER / LIVE / CRM BRIDGE / GATEWAY BOUNDARY HANDOFF

Tài liệu này khóa lớp Channel Handoff Bridge cho AI Advisor. Kênh giao tiếp chỉ là bề mặt tương tác với khách hàng; mọi giá, tồn, sellable, quote, order, payment, shipping, recall và sale lock phải lấy từ runtime đã được Phase 1, Phase 2 và Phase 3 quản trị.

## PHASE MARKER

Phase: PHASE-04 - AI ADVISOR RUNTIME

File: 07-CẦU NỐI BÀN GIAO KÊNH MESSENGER TRỰC TIẾP VÀ CRM.docx

Prompt: PROMPT-P4-2F - Channel Handoff / Messenger / Live / CRM Bridge / Gateway Boundary Handoff

Mode: LIMITED IMPLEMENTATION HANDOFF - chỉ triển khai lớp phối hợp kênh và handoff boundary. Không triển khai Gateway mở bán, không sửa Pricing, Sellable Gate, Order Core, Payment Core, Shipping Core, Verified Revenue hoặc Commission.

## 1. HEADER

Mục tiêu của file này là giao cho dev/Codex/Copilot một prompt triển khai giới hạn cho lớp Channel Handoff Bridge của AI Advisor. Lớp này giúp AI Advisor nhận ngữ cảnh từ Live, Messenger, CRM, Ads/Live session và các kênh liên quan; sau đó điều phối câu trả lời đúng bề mặt giao tiếp, đúng privacy boundary, đúng policy Meta/Facebook, đúng runtime Commerce và đúng Recall/Sale Lock.

Comment public mơ hồ hoặc có tín hiệu mua hàng phải được phân loại trước khi trả lời hoặc kéo về Messenger.

Giá, tạm tính, quote, thông tin nhận hàng, phương thức thanh toán và xác nhận đơn phải ưu tiên private/Messenger theo policy.

Live không hardcode giá, không tự tạo ưu đãi, không tự nói còn hàng, không tự ghi nhận đơn chính thức.

CRM không được gợi ý SKU not sellable, hết hàng, recall, sale lock hoặc channel suppressed.

Sau khi handoff sang private thành công, hội thoại tiếp theo mặc định đi theo private thread nếu policy cho phép.

Gateway vẫn bị chặn cho đến khi có owner review, evidence accepted và release decision.

## 2. MODE

Chế độ: Limited Implementation; Được phép: Được tạo/sửa channel context normalizer, handoff decision policy, public/private routing, Messenger thread bridge, Live comment reply policy, CRM trigger bridge, audit log và smoke test.; Không được phép: Không sửa Commerce Runtime, Pricing Engine, Sellable Gate, Inventory, Payment, Shipping, Verified Revenue, Commission, ROAS hoặc Payout.

Chế độ: Runtime Consumer; Được phép: Được consume customer_context, channel_context, product_public_data, sellable_status, quote_snapshot, order_draft_state, payment_public_state, shipping_public_state và sale_lock flags.; Không được phép: Không tự tính giá, tồn, discount, member benefit, Diamond benefit, shipping fee, VAT, paid, verified revenue hoặc commission.

Chế độ: Channel Bridge; Được phép: Được chuyển ngữ cảnh từ public comment sang private Messenger, gắn thread/session id, giữ continuity và handoff human khi cần.; Không được phép: Không spam khách, không gửi tin ngoài policy, không public thông tin nhạy cảm, không gửi lại quote hết hạn hoặc order chưa confirmed như đơn chính thức.

Chế độ: Gateway Boundary; Được phép: Được ghi trạng thái bị chặn/READY_FOR_REVIEW ở report phase.; Không được phép: Không gọi Gateway PASS, không mở production traffic, không bật auto-send đại trà nếu chưa có evidence và owner decision.

## 3. SOURCE-OF-TRUTH

Nguồn: PHASE 0 - Foundation; Vai trò trong file 2F: Actor, permission, audit, evidence, idempotency, high-risk guard.; Quy tắc không được vi phạm: Mọi outbound/handoff quan trọng phải có actor/context/audit/correlation/idempotency khi tạo side effect.

Nguồn: PHASE 1 - Product/SKU/Recipe; Vai trò trong file 2F: Cung cấp product_public_data và public-safe product view.; Quy tắc không được vi phạm: Kênh không đọc hoặc lộ internal SKU code, công thức, tỷ lệ, seed/private field.

Nguồn: PHASE 2 - Operational Core; Vai trò trong file 2F: Cung cấp operational suppression: recall, sale lock, QR/public trace, warehouse/batch truth.; Quy tắc không được vi phạm: Recall/Sale Lock thắng Live, Ads, CRM, AI Advisor và Gateway.

Nguồn: PHASE 3 - Commerce Runtime; Vai trò trong file 2F: Cung cấp sellable, quote, cart, order draft, official order, payment/shipping public-safe state.; Quy tắc không được vi phạm: Không có QuoteSnapshot thì không có final price. Không có order_code thì không nói đơn chính thức. Payment selected không đồng nghĩa Paid.

Nguồn: PHASE 4 - AI Advisor; Vai trò trong file 2F: Tư vấn sản phẩm, giữ customer memory context, render câu trả lời và channel handoff.; Quy tắc không được vi phạm: AI Advisor là consumer/orchestrator, không phải source-of-truth nghiệp vụ.

Nguồn: Meta/Facebook policy boundary; Vai trò trong file 2F: Ràng buộc private reply, comment reply, Messenger thread, opt-out, spam/frequency cap.; Quy tắc không được vi phạm: Không gửi tin vượt permission, không ép khách vào Messenger khi policy không cho phép, không tự động spam CRM.

## 4. OBJECTIVE

Thiết kế và triển khai giới hạn lớp Channel Handoff Bridge để AI Advisor có thể phối hợp an toàn với Messenger, Live, CRM và Gateway mà không làm sai ranh giới runtime. Lớp này phải đủ mạnh để chuyển ngữ cảnh bán hàng từ public sang private, giữ continuity, áp policy channel, tôn trọng customer preference, chặn spam, chặn leak dữ liệu nội bộ và chặn mọi hành vi mở bán khi runtime không cho phép.

Chuẩn hóa inbound event từ comment, live comment, Messenger, CRM trigger và channel callback thành một ChannelContextEnvelope thống nhất.

Định tuyến public/private theo intent, sensitivity, channel policy, customer preference, product/sellable state và quote/order boundary.

Tạo handoff decision rõ ràng: reply public ngắn, invite private, continue private, human handoff, suppress, hide/comment moderation hoặc block purchase khi policy yêu cầu.

Đảm bảo giá/quote/order chỉ đi qua Commerce Runtime; AI Advisor không tự tính hoặc tự nói thay runtime.

Đảm bảo CRM chỉ phát sinh từ trigger hợp lệ, có frequency cap, opt-out và suppression theo sellable/recall/sale lock.

Tạo evidence và smoke đủ để chứng minh bridge hoạt động đúng trước khi Phase 4 tổng kết.

## 5. SCOPE IN

Inbound normalization: Chuẩn hóa sự kiện từ Page comment, Live comment, Messenger message, postback, private reply callback, CRM schedule, ads/live session metadata.

Channel identity: Mapping page_id, page_name, thread_id, participant_id, customer_profile_id, live_session_id, message_id, comment_id, campaign_id, source_channel.

Public/private routing: Xác định khi nào trả lời public, khi nào kéo Messenger, khi nào tiếp tục private, khi nào không trả lời, khi nào chuyển người thật.

Messenger handoff: Giữ context từ comment/live sang Messenger, chống mất SKU/nhu cầu/quote intent, không hỏi lại vòng nếu runtime đã có context.

Live bridge: Live comment intent detection, anti-spam, short public reply, private quote flow, host/MC AI assist cue nếu cần.

CRM bridge: Cho phép CRM consume customer context và product recommendation đã qua sellable/claim guard; có frequency cap, quiet hour, opt-out.

Safety and moderation: Route khiếu nại thật sang CSKH; troll/abuse/spam sang moderation; không kéo Messenger với comment phá Live theo policy.

Audit and evidence: Log decision, input, output, policy version, correlation id, runtime snapshot references và handoff outcome.

## 6. SCOPE OUT

Không tự tính giá/discount/member benefit/Diamond benefit: Thuộc Phase 3 Commerce Runtime; AI Advisor chỉ hiển thị QuoteSnapshot.

Không tự tính tồn/sellable/out-of-stock: Sellable Gate là runtime gate riêng; kênh chỉ consume kết quả.

Không tạo order_code: Official Order và order_code thuộc Commerce Runtime.

Không xác nhận Paid: Payment selected không đồng nghĩa Paid; Paid phải do Payment Core xác nhận.

Không verified revenue/commission/ROAS/payout: Chỉ khi ORDER_VERIFIED mới có downstream finance/ads/commission truth.

Không mở Gateway: Gateway cần global review, evidence accepted và release decision.

Không tích hợp IVR ở file này: IVR chỉ reserved; phase IVR riêng xử lý auto-call verification.

Không bypass Meta policy: Không gửi tin hoặc private reply vượt permission, frequency cap, opt-out hoặc policy window.

## 7. ARCHITECTURE POSITION

Channel Handoff Bridge nằm giữa AI Advisor Runtime và các kênh giao tiếp. Bridge không phải Gateway mở bán, không phải Commerce Runtime và không phải nơi sinh business truth. Bridge chỉ nhận context, áp policy kênh, gọi AI Advisor/Commerce Runtime đúng boundary và trả outbound command phù hợp cho Channel Gateway.

Inbound Channel Event: Comment, live comment, message, postback, CRM trigger hoặc callback được tiếp nhận bởi Channel Gateway.

Normalize: Gateway/Bridge tạo ChannelContextEnvelope có actor/channel/session/correlation/idempotency.

Resolve Context: AI Advisor đọc customer memory, product intent, runtime consumer contract và channel policy.

Runtime Check: Sellable, quote, order draft, payment/shipping public state, recall/sale lock đều lấy từ Phase 3/Phase 2.

Guard: Safety/Public Wording/Claim Guard và Public/Private Data Filter chạy trước outbound.

Handoff Decision: Reply public / invite private / continue private / human handoff / suppress / moderation / CRM schedule.

Outbound Command: Channel Gateway gửi message theo permission; mọi outbound có audit và policy version.

## 8. CHANNEL CONTEXT ENVELOPE

Dev phải thiết kế một envelope thống nhất để mọi channel event đi vào AI Advisor không còn rời rạc. Envelope này giúp AI biết đang nói ở đâu, với ai, từ session nào, đã private chưa, có policy gửi tin hay không và cần áp guard nào.

Field: correlation_id; Bắt buộc: Có; Ý nghĩa / Rule: Theo dõi toàn bộ request lifecycle từ inbound tới outbound, audit và evidence.

Field: idempotency_key; Bắt buộc: Có khi tạo side effect; Ý nghĩa / Rule: Chống gửi trùng tin, tạo trùng quote, xác nhận trùng đơn hoặc CRM trigger trùng.

Field: source_channel; Bắt buộc: Có; Ý nghĩa / Rule: facebook_comment, facebook_live_comment, messenger, crm, website, admin_assist hoặc reserved channel.

Field: surface; Bắt buộc: Có; Ý nghĩa / Rule: PUBLIC hoặc PRIVATE. Dữ liệu giá/quote/order không được public nếu policy không cho phép.

Field: page_id / page_role; Bắt buộc: Có với Facebook; Ý nghĩa / Rule: Xác định page chính thức, page live, page test, page CRM, multi-page registry.

Field: live_session_id; Bắt buộc: Có nếu từ Live; Ý nghĩa / Rule: Giữ ngữ cảnh live, chương trình, host cue, golden hour runtime nếu có. Không tự hardcode giá.

Field: thread_id; Bắt buộc: Có với Messenger; Ý nghĩa / Rule: Dùng cho continuity sau handoff; không hỏi lại nếu context còn valid.

Field: comment_id / message_id; Bắt buộc: Có khi có; Ý nghĩa / Rule: Gắn evidence và reply target; tránh reply trùng.

Field: customer_profile_id; Bắt buộc: Nếu resolve được; Ý nghĩa / Rule: Liên kết Customer Memory read-only; nếu chưa resolve thì kích hoạt IdentityResolutionGuard.

Field: channel_permission_state; Bắt buộc: Có; Ý nghĩa / Rule: Cho biết có được private reply, send message, CRM follow-up hay không.

Field: communication_preference; Bắt buộc: Nếu có; Ý nghĩa / Rule: Opt-out, quiet hour, frequency cap, preferred channel.

Field: runtime_suppression_flags; Bắt buộc: Có; Ý nghĩa / Rule: recall, sale_lock, channel_suppressed, product_not_sellable, policy_blocked.

Field: handoff_state; Bắt buộc: Có; Ý nghĩa / Rule: NONE, INVITED_TO_PRIVATE, PRIVATE_ACTIVE, HUMAN_HANDOFF, SUPPRESSED, MODERATION.

## 9. HANDOFF DECISION MATRIX

Tình huống: Khách hỏi giá ở comment public; Quyết định bridge: Trả public ngắn và kéo Messenger nếu policy cho phép.; Ràng buộc bắt buộc: Không public final price nếu cần QuoteSnapshot/private. Không tự tính giá.

Tình huống: Khách hỏi "còn hàng không" trên Live; Quyết định bridge: Không nói số tồn chi tiết; gọi sellable runtime và trả public-safe hoặc kéo private.; Ràng buộc bắt buộc: Nếu not sellable/recall/sale lock thì không chốt, đề xuất thay thế nếu có.

Tình huống: Khách comment "chốt/mua/đồng ý"; Quyết định bridge: Kéo Messenger hoặc tiếp tục private để lập Order Draft theo Commerce Runtime.; Ràng buộc bắt buộc: Không nói đã ghi nhận chính thức khi chưa có order_code.

Tình huống: Khách ở Messenger sau Live hỏi tiếp; Quyết định bridge: Continue private thread, giữ product intent/customer context.; Ràng buộc bắt buộc: Không hỏi lại thông tin đã resolve; chỉ hỏi thiếu trường bắt buộc.

Tình huống: CRM muốn gợi ý mua lại; Quyết định bridge: Kiểm tra sellable, sale lock, customer preference, frequency cap, opt-out trước khi render.; Ràng buộc bắt buộc: Không gợi ý SKU not sellable hoặc bị recall/sale lock.

Tình huống: Comment khiếu nại có mã đơn/mã lô/bằng chứng; Quyết định bridge: Route Complaint/CSKH flow, không xử như troll.; Ràng buộc bắt buộc: Yêu cầu giữ sản phẩm/chụp bằng chứng theo policy; không kết luận công khai.

Tình huống: Comment xúc phạm/phá Live/troll rõ; Quyết định bridge: Không trả public, không kéo Messenger, route moderation theo policy.; Ràng buộc bắt buộc: Có thể hide/flag/blacklist theo Live Moderation rule nếu đủ điều kiện.

Tình huống: Khách yêu cầu người thật; Quyết định bridge: Human handoff, giữ summary context cho CSKH.; Ràng buộc bắt buộc: Không tiếp tục ép quote/chốt nếu khách muốn người thật.

Tình huống: Meta policy không cho private reply; Quyết định bridge: Không gửi private; dùng public-safe fallback hoặc suppress.; Ràng buộc bắt buộc: Không bypass policy bằng tin nhắn ngoài luồng.

## 10. PUBLIC / PRIVATE ROUTING RULES

Loại nội dung: Giới thiệu sản phẩm; Public được phép: Được nói tên public, nhóm sản phẩm, khẩu vị, hiệu dụng public-safe theo ẩm thực phương Đông.; Private/Messenger bắt buộc hoặc ưu tiên: Tư vấn sâu theo tình trạng cá nhân, combo, lịch sử mua, member context.

Loại nội dung: Giá và ưu đãi; Public được phép: Chỉ trả ngắn theo policy hoặc mời vào Messenger để có giá đúng tại thời điểm.; Private/Messenger bắt buộc hoặc ưu tiên: QuoteSnapshot, final payable price, quote expiry, member benefit, Diamond referral boundary.

Loại nội dung: Tạm tính / Order Draft; Public được phép: Không public thông tin nhận hàng, số điện thoại, địa chỉ, payment preference.; Private/Messenger bắt buộc hoặc ưu tiên: Order Draft payload, customer confirmation, payment method, shipping info.

Loại nội dung: Trạng thái đơn; Public được phép: Không public mã đơn nếu có nguy cơ lộ thông tin khách.; Private/Messenger bắt buộc hoặc ưu tiên: Official order_code, payment public-safe state, shipping public-safe state.

Loại nội dung: Khiếu nại chất lượng; Public được phép: Public trả lời thận trọng, mời private/CSKH.; Private/Messenger bắt buộc hoặc ưu tiên: Ảnh bao bì, QR, mã lô, hóa đơn, nơi mua, dữ liệu cá nhân.

Loại nội dung: Dữ liệu nội bộ; Public được phép: Không public.; Private/Messenger bắt buộc hoặc ưu tiên: Chỉ dùng nội bộ nếu role/permission hợp lệ; không đưa ra khách.

## 11. LIVE BRIDGE RULES

Live Bridge phải xử lý multi-page và multi-live session bằng page_id, live_session_id và session policy; không gộp nhầm context giữa các page hoặc phiên live.

Live comment trả lời phải ngắn, đúng ý, không spam, không lặp máy móc, không public dữ liệu nhạy cảm và không hardcode giá.

Nếu Live Board/runtime xác nhận đang có Giờ Vàng Tri Ân, AI chỉ được hiển thị nội dung ưu đãi dựa trên QuoteSnapshot hoặc runtime program context, không tự tạo giảm giá.

Nếu SKU được hỏi not sellable, Live Bridge phải chặn chốt SKU đó, có thể gợi ý SKU thay thế sellable nếu Product Consultation Orchestrator trả về hợp lệ.

Nếu comment là phá Live/troll/abuse, không kéo Messenger và không reply public; route moderation đúng policy.

Nếu comment là khiếu nại thật có mã đơn/mã lô/bằng chứng, route Complaint/CSKH, không xử như troll.

## 12. MESSENGER HANDOFF RULES

Create private continuity: Khi kéo từ comment/live sang Messenger, phải mang theo product intent, channel origin, live_session_id, comment_id, campaign/session context và customer context nếu đã resolve.

No repeated questioning: Nếu runtime đã có customer profile, member tier, địa chỉ, người nhận hoặc nhu cầu trước đó, AI không hỏi lại vòng; chỉ yêu cầu xác nhận/chỉnh sửa.

Quote private: Final payable price, member benefit, quote expiry, order draft, payment method và địa chỉ nhận hàng ưu tiên nằm trong Messenger/private.

Confirmation boundary: AI chỉ gửi Customer Confirmation event khi khách xác nhận đủ thông tin và channel policy cho phép.

Official order boundary: Chỉ sau khi Commerce Runtime trả order_code thì AI mới nói đơn đã ghi nhận chính thức.

Payment boundary: Chọn COD/chuyển khoản không đồng nghĩa Paid; chỉ Payment Core confirmation mới là Paid.

Human handoff: Nếu khách yêu cầu người thật hoặc policy/risk vượt năng lực AI, bridge chuyển CSKH kèm summary context và không mất lịch sử trao đổi.

## 13. CRM BRIDGE RULES

CRM Bridge chỉ được kích hoạt khi có trigger hợp lệ, customer preference cho phép và runtime không bị suppression. CRM không phải nơi bypass sellable, không phải nơi bán SKU đang bị khóa và không phải nơi gửi tin nhắn đại trà không kiểm soát.

CRM Trigger: Sau mua D+0 đến D+7; Được phép khi: Order đã có official_order_state hợp lệ và nội dung chỉ hướng dẫn sử dụng/chăm sóc public-safe.; Chặn khi: Order chưa official, khách opt-out, policy window không cho gửi, hoặc có complaint active.

CRM Trigger: Gợi ý mua lại; Được phép khi: SKU/alternative sellable, không recall/sale lock, frequency cap còn dư, customer need phù hợp.; Chặn khi: SKU not sellable, sale lock, recall, khách đã từ chối, quiet hour hoặc vượt tần suất.

CRM Trigger: Nâng hạng/member chăm sóc; Được phép khi: Member context do runtime/core trả về; nội dung không tự tính quyền lợi.; Chặn khi: Không resolve identity, member policy missing, runtime conflict.

CRM Trigger: Golden Hour reminder; Được phép khi: Runtime xác nhận chương trình và customer eligible theo policy.; Chặn khi: Không có active program, không có permission, khách opt-out, hoặc chương trình không áp dụng.

CRM Trigger: Complaint follow-up; Được phép khi: CSKH/QA đã có case state phù hợp và nội dung thận trọng.; Chặn khi: Chưa xác minh, hàng giả/hàng nhái nghi ngờ, dữ liệu nhạy cảm chưa được kiểm soát.

## 14. SUPPRESSION AND MODERATION POLICY

Loại suppression: Recall / Sale Lock; Cách bridge xử lý: Chặn tư vấn bán/chốt/quote/order cho SKU/batch/channel liên quan; ưu tiên thông điệp public-safe hoặc handoff CSKH.; Ghi chú: Recall/Sale Lock thắng Ads/Live/CRM/AI/Gateway/IVR.

Loại suppression: Product not sellable; Cách bridge xử lý: Không quote/chốt SKU; đề xuất alternative sellable nếu Product Orchestrator trả về.; Ghi chú: Không nói còn hàng khi runtime không xác nhận.

Loại suppression: Channel suppressed; Cách bridge xử lý: Không gửi outbound ở kênh bị chặn; dùng kênh khác chỉ khi permission và preference cho phép.; Ghi chú: Không bypass bằng message ngoài policy.

Loại suppression: Opt-out / frequency cap; Cách bridge xử lý: Không gửi CRM hoặc follow-up tự động; chỉ phản hồi nếu khách chủ động nhắn trong policy.; Ghi chú: Tôn trọng quyền riêng tư và tránh spam.

Loại suppression: Abuse/troll/spam; Cách bridge xử lý: Không trả public, không kéo private; route moderation/hide/flag/blacklist theo policy.; Ghi chú: Phân biệt với khiếu nại thật có bằng chứng.

Loại suppression: Human handoff required; Cách bridge xử lý: Dừng auto-advisor và chuyển CSKH/QA/legal/owner tùy case.; Ghi chú: Giữ summary context và evidence references.

## 15. RESPONSE TIMING AND DELIVERY CONTROL

Channel Bridge cần hỗ trợ delivery control để câu trả lời tự nhiên, không spam và không vượt giới hạn policy. Thời gian trả lời không phải thủ thuật câu giờ; đây là lớp kiểm soát trải nghiệm và chống hành vi gửi tin quá nhanh, quá dày, lặp lại hoặc trái ngữ cảnh.

Câu public ngắn có thể trả nhanh nhưng vẫn phải qua guard và deduplication.

Câu tư vấn dài ở Messenger có thể dùng typing indicator/delay theo độ dài nội dung nếu channel policy cho phép.

Không chia nhỏ quá nhiều tin làm spam; nếu cần chia, mỗi block phải có mục đích rõ ràng.

Không gửi lại cùng nội dung khi user lặp comment nhiều lần; phải có duplicate suppression.

Nếu khách đang ở private sau handoff, câu trả lời tiếp theo ưu tiên private thread thay vì trả trùng ở public.

## 16. DATA CONTRACTS TO IMPLEMENT

Contract: ChannelContextEnvelope; Vai trò: Chuẩn hóa inbound event.; Field tối thiểu: correlation_id, source_channel, surface, page_id, live_session_id, thread_id, message_id/comment_id, customer_profile_id, permission_state, preference, suppression_flags.

Contract: HandoffDecision; Vai trò: Kết quả route.; Field tối thiểu: decision_type, target_surface, reason_code, allowed_actions, blocked_actions, human_handoff_required, moderation_action, policy_version.

Contract: OutboundMessageCommand; Vai trò: Lệnh gửi ra channel.; Field tối thiểu: channel, target_id, template_id/body, private/public flag, idempotency_key, guard_result_id, rate_limit_bucket, audit_ref.

Contract: CRMTriggerDecision; Vai trò: Quyết định CRM follow-up.; Field tối thiểu: customer_id, trigger_type, eligible, suppression_reason, next_allowed_at, product_recommendation_ref, quote_ref if any.

Contract: HandoffAuditEvent; Vai trò: Bằng chứng audit.; Field tối thiểu: actor/system actor, correlation_id, input_ref, decision_ref, runtime_refs, policy_version, output_ref, timestamp, result.

## 17. EXECUTION STEPS

## 1. Inspect codebase hiện có để xác định event model của Facebook/Messenger/Live/CRM và các handler hiện đang gọi AI Advisor.

## 2. Lập bản đồ route hiện tại: public comment, live comment, Messenger, CRM trigger, human handoff và moderation.

## 3. Tạo hoặc chuẩn hóa ChannelContextEnvelope; đảm bảo correlation_id, idempotency_key và permission_state xuất hiện ở mọi flow tạo side effect.

## 4. Tạo HandoffDecision policy với các decision_type: PUBLIC_REPLY, PRIVATE_INVITE, CONTINUE_PRIVATE, HUMAN_HANDOFF, SUPPRESS, MODERATE, CRM_SCHEDULE.

## 5. Kết nối bridge với Runtime Consumer Contract của file 2A và Safety/Public Wording/Claim Guard của file 2E.

## 6. Đảm bảo quote/order/private data chỉ đi qua Messenger/private theo policy; public không lộ dữ liệu nhạy cảm.

## 7. Thêm suppression theo Recall/Sale Lock, product not sellable, channel suppressed, opt-out, frequency cap và abuse/troll policy.

## 8. Thêm audit/evidence log cho từng quyết định handoff và outbound command.

## 9. Viết smoke test theo các case bắt buộc trong file này; không mở Gateway dù smoke local pass.

## 18. REQUIRED SMOKE TESTS

Mã smoke: P4-2F-SMK-001; Tình huống: Khách hỏi giá ở comment public.; Kết quả phải đạt: Bridge không public final price; kéo Messenger nếu policy cho phép; QuoteSnapshot chỉ tạo/hiển thị qua Commerce Runtime.

Mã smoke: P4-2F-SMK-002; Tình huống: Khách comment "chốt" trên Live.; Kết quả phải đạt: Bridge mở private/order draft flow; không nói đơn chính thức trước order_code.

Mã smoke: P4-2F-SMK-003; Tình huống: Sau handoff sang Messenger, khách hỏi tiếp.; Kết quả phải đạt: AI giữ context, không hỏi lại vòng thông tin đã resolve.

Mã smoke: P4-2F-SMK-004; Tình huống: CRM gợi ý SKU not sellable.; Kết quả phải đạt: CRM Bridge chặn và không gửi tin gợi ý SKU đó.

Mã smoke: P4-2F-SMK-005; Tình huống: Recall/Sale Lock active.; Kết quả phải đạt: Ads/Live/CRM/AI đều bị suppression đúng; không quote/chốt SKU bị khóa.

Mã smoke: P4-2F-SMK-006; Tình huống: Comment troll/abuse phá Live.; Kết quả phải đạt: Không reply public, không kéo Messenger, route moderation.

Mã smoke: P4-2F-SMK-007; Tình huống: Khiếu nại thật có mã đơn/mã lô.; Kết quả phải đạt: Route Complaint/CSKH; không xử như troll; không kết luận công khai.

Mã smoke: P4-2F-SMK-008; Tình huống: Meta policy không cho private reply.; Kết quả phải đạt: Bridge không gửi private; dùng fallback public-safe hoặc suppress.

Mã smoke: P4-2F-SMK-009; Tình huống: Khách opt-out CRM.; Kết quả phải đạt: Không gửi CRM follow-up; audit suppression reason.

Mã smoke: P4-2F-SMK-010; Tình huống: Duplicate inbound event.; Kết quả phải đạt: Idempotency/dedup chặn gửi trùng tin hoặc tạo trùng side effect.

## 19. EVIDENCE REQUIRED

Screenshot hoặc log chứng minh ChannelContextEnvelope được tạo đủ field bắt buộc.

Evidence cho HandoffDecision ở từng case: public reply, private invite, continue private, human handoff, suppress, moderate, CRM schedule.

Audit log có correlation_id, idempotency_key, channel, target, policy version, runtime refs và result.

Smoke output chứng minh public/private boundary không lộ quote/order/payment/shipping/private customer data.

Evidence chứng minh CRM không gửi khi opt-out, quiet hour, frequency cap hoặc not sellable/recall/sale lock.

Evidence chứng minh Gateway vẫn bị chặn sau khi local smoke pass.

## 20. REQUIRED REPORT FORMAT - 14 MỤC

1: Executive summary: đã làm gì trong phạm vi P4-2F.

2: Files inspected and files changed.

3: Current channel flow before change.

4: Implemented ChannelContextEnvelope and field mapping.

5: Implemented HandoffDecision policy and decision matrix.

6: Public/private boundary validation.

7: Messenger handoff and continuity validation.

8: Live bridge and moderation validation.

9: CRM bridge, opt-out, frequency cap and quiet hour validation.

10: Runtime dependency validation: sellable, quote, order, payment, shipping, recall/sale lock.

11: Audit/evidence/idempotency validation.

12: Smoke test result table with pass/fail/evidence links.

13: Known gaps, điểm chặn, risks and owner decisions needed.

## 21. DONE GATE

Envelope: Mọi inbound channel event có ChannelContextEnvelope thống nhất và không mất context quan trọng.

Routing: HandoffDecision hoạt động đúng cho public, private, live, CRM, human handoff, suppress và moderation.

Runtime boundary: Không có logic tự tính giá/tồn/sellable/order/paid/revenue trong bridge.

Public/private safety: Quote, order draft, payment, shipping và dữ liệu khách hàng không bị public leak.

Messenger continuity: Sau handoff, AI không hỏi lại vòng nếu runtime/customer context đã có.

CRM safety: CRM tôn trọng sellable, recall/sale lock, opt-out, frequency cap và quiet hour.

Moderation: Troll/abuse không bị kéo Messenger; complaint thật không bị xử sai như troll.

Smoke: P4-2F-SMK-001 đến P4-2F-SMK-010 có evidence pass.

## 22. FAIL GATE

Bridge public final price hoặc tạm tính khi chưa có policy và QuoteSnapshot hợp lệ.

Bridge nói đơn chính thức khi chưa có order_code từ Commerce Runtime.

Bridge nói đã Paid khi khách chỉ chọn COD hoặc chuyển khoản.

Live hardcode giá, ưu đãi, tồn hoặc trạng thái sellable.

CRM gửi SKU not sellable, recall, sale lock hoặc vượt opt-out/frequency cap.

AI trả lời public làm lộ thông tin khách, order, payment, shipping, mã lô nhạy cảm hoặc dữ liệu nội bộ.

Troll/abuse bị kéo Messenger trái policy hoặc khiếu nại thật bị xử nhầm như troll.

Không có audit/correlation/idempotency cho outbound hoặc handoff side effect.

Dev report gọi Gateway PASS, Production Readiness hoặc Go-live Decision khi chưa có global release decision.

## 23. DOWNSTREAM HANDOFF

## 08-BÁO CÁO KIỂM THỬ KHÓI VÀ BẰNG CHỨNG: Tổng hợp smoke/evidence toàn Phase 4: runtime consumer, customer memory, product consultation, quote/order consumer, safety guard, channel bridge.

09-CHỈ MỤC BÀN GIAO: Đóng gói toàn Phase 4, liệt kê file, owner, dependency, bị chặn condition và global release note.

Future Gateway Review: Chỉ sau khi Phase 4 smoke report pass cục bộ, owner review và evidence accepted mới đưa vào Global Gate review.

Future Meta Gateway phase: Các cấu hình App Review, Page Token, webhook, multi-page registry, outbound permission production phải nằm ở phase Gateway/Meta riêng nếu chưa khóa.

Future IVR phase: IVR chỉ reserved; không triển khai auto-call trong file 2F.

## 24. PROMPT COPY GIAO DEV / CODEX / COPILOT

Copy nguyên prompt dưới đây cho dev/Codex/Copilot khi triển khai P4-2F. Prompt này không cho phép copy-paste code rời rạc. Dev phải inspect codebase thật, schema thật, API thật, test thật và chỉ sửa trong phạm vi Channel Handoff Bridge.

## PROMPT-P4-2F - CHANNEL HANDOFF / MESSENGER / LIVE / CRM BRIDGE / GATEWAY BOUNDARY HANDOFF

## MODE: LIMITED IMPLEMENTATION - CHANNEL HANDOFF BRIDGE ONLY

You are working on Ginsengfood PHASE-04 AI Advisor Runtime.

Do not implement by copy-pasting isolated code. Inspect the real codebase, architecture, database, APIs, message handlers, channel adapters, tests and existing conventions before changing anything.

Objective:

- Implement or align the Channel Handoff Bridge for Messenger, Live, CRM and public/private routing.

- AI Advisor must remain a runtime consumer, not the source-of-truth for price, stock, sellable, quote, order, payment, shipping, verified revenue, commission, ROAS or payout.

- Gateway must remain bị chặn.

Scope in:

- ChannelContextEnvelope normalization.

- HandoffDecision policy.

- Public/private routing.

- Messenger handoff continuity.

- Live comment bridge and moderation routing.

- CRM bridge with opt-out, quiet hour, frequency cap and runtime suppression.

- Audit, correlation, idempotency and evidence.

- Smoke tests P4-2F-SMK-001 to P4-2F-SMK-010.

Scope out:

- Do not change Pricing Engine, Sellable Gate, Inventory, Order Core, Payment Core, Shipping Core, Verified Revenue, Commission, ROAS, Payout, Gateway release or IVR.

- Do not hardcode price, stock, discount, program, member benefit, Diamond benefit or sellable status.

Mandatory rules:

- Public comments cannot leak final quote, order data, payment state, shipping detail or customer private data.

- Price/quote/order must use Commerce Runtime and QuoteSnapshot.

- No order_code means no official order wording.

- Payment selected is not Paid.

- CRM cannot recommend not-sellable, recall, sale-lock or channel-suppressed SKUs.

- Recall/Sale Lock wins over AI, Ads, Live, CRM, Gateway and IVR.

- Abuse/troll should be routed to moderation; real complaints with order/lot/evidence must go to CSKH/Complaint flow.

- All side-effect outbound actions must be idempotent and audited.

Required report format: 14 sections exactly as specified in 07-P4-2F document.

## PHASE-04-AI-ADVISOR-RUNTIME

## 08-BÁO CÁO KIỂM THỬ KHÓI VÀ BẰNG CHỨNG



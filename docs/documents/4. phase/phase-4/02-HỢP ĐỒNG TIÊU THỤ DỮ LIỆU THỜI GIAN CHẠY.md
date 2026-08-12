# P4.2A - RUNTIME CONSUMER CONTRACT

## Khối nguồn bắt buộc đã nhập vào file này

File này có khối nguồn bắt buộc để chạy lẻ, nhưng khi bàn giao Phase 4 phải đọc `12-BẢNG GOM GIAI ĐOẠN 4.md`, `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` trước. Nếu nội dung file này khác Bảng Gôm/phụ lục/SRS handoff, áp dụng Bảng Gôm/phụ lục/SRS handoff và xử lý fail-closed.

### A. Nội dung nguồn đã nhập

1. Master/runtime governance: hợp đồng tiêu thụ runtime phải đi theo owner truth. Product/Formula/Public Product Knowledge quyết định dữ liệu sản phẩm công khai. Operational Core quyết định sellable, recall, sale lock, batch/trace public-safe. Commerce Runtime quyết định DailySalesContext, QuoteSnapshot, OrderDraft, CustomerConfirmation, order_code, payment public-safe state, shipping/ETA và ORDER_VERIFIED. AI Advisor chỉ consume/orchestrate/render, không tạo nguồn sự thật song song.
2. Evidence/release governance: contract tồn tại không phải implementation complete. Local smoke pass hoặc report pass không được xem là Completion PASS, Release Ready, Production Ready hoặc Go-live. Mọi gate mặc định là PENDING_EVIDENCE cho đến khi có artifact được owner chấp nhận.
3. AI Advisor business/tech lock: AI được gọi runtime hợp lệ, nhận response public-safe, ghi resolver trace, guard trace và DecisionEnvelope. AI không được tự tính giá, tồn, discount, quyền lợi thành viên, Diamond benefit, ship, VAT, order_code, paid, verified revenue, commission, ROAS hoặc payout.
4. Commerce lock: final price chỉ đến từ QuoteSnapshot còn hiệu lực. Order Draft không phải Official Order. Chỉ có CustomerConfirmation hợp lệ và order_code từ Commerce Runtime mới cho phép nói đơn đã ghi nhận chính thức. Payment selected, COD selected hoặc ảnh chuyển khoản không phải Paid.
5. Product/Operational lock: contract phải expose product public-safe, ProductRoleMatrix và sellable/runtime signals; không expose mã SKU nội bộ, công thức/tỷ lệ/cost/supplier/QC note, hoặc dữ liệu lot/batch private.
6. CRM/Member lock: customer memory/member context là read-only input. Không dùng customer memory để quyết giá, quyền lợi, tier, frequency, suppression hoặc CRM eligibility nếu runtime chưa trả.
7. Finance/Diamond lock: Diamond/referral/commission/payout là Finance/Diamond state. Contract AI chỉ nhận public-safe/read-only state nếu có; không tự tạo payable, commission, PIT hoặc payout status.
8. Gateway/Ads/Live lock: ChannelContext chỉ là ngữ cảnh bề mặt và attribution. Gateway production, Pixel/CAPI, ROAS, Live delivery, Messenger send, App Review và token không thuộc contract owner của AI Advisor.

### B. Conflict lock

- Missing runtime data: fail-closed hoặc human handoff, không fallback sang prompt/template/cache không kiểm chứng.
- Conflict giá/chương trình/quyền lợi: chỉ QuoteSnapshot/runtime policy được thắng.
- Conflict sellable/recall/sale lock: Operational/Commerce suppression thắng mọi AI/Live/CRM/Ads intent.
- Conflict paid/order/revenue: order_code và payment/verified revenue owner thắng mọi lời khách/ảnh chuyển khoản.
- Conflict customer memory/member: memory không override Commerce/CRM/Finance runtime.
- Conflict evidence/completion: thiếu artifact hoặc owner acceptance thì giữ PENDING_EVIDENCE/BLOCKED, không gọi PASS.

### C. Trạng thái evidence bắt buộc

Trạng thái mặc định của file này: RUNTIME_CONSUMER_CONTRACT_WITH_IMPORTED_SOURCE_LOCK. Contract chỉ đạt khi mọi input/output owner rõ, missing/conflict behavior rõ, idempotency/audit/evidence field rõ và không có self-compute path trong AI Advisor. Không được gọi RUNTIME_IMPLEMENTATION_READY, GATEWAY_PASS, COMPLETION_PASS, RELEASE_READY, PRODUCTION_READY hoặc GO_LIVE_APPROVED.

### D. Khóa riêng của file này

File 02 chỉ khóa hợp đồng tiêu thụ dữ liệu thời gian chạy. Nhiệm vụ là bảo đảm AI Advisor chỉ đọc/consume runtime owner truth và không tự tạo business truth mới.

## Nội dung triển khai

## PROMPT-P4-2A - AI ADVISOR RUNTIME CONSUMER CONTRACT / READ-ONLY BUSINESS TRUTH CONSUMPTION / FAIL-CLOSED BOUNDARY HANDOFF

Phase: PHASE 4 - AI Advisor Runtime

File: 02-HỢP ĐỒNG TIÊU THỤ DỮ LIỆU THỜI GIAN CHẠY.docx

Mode: LIMITED IMPLEMENTATION HANDOFF - RUNTIME CONSUMER CONTRACT ONLY

Phạm vi: Khóa contract dữ liệu, adapter, resolver boundary và fail-closed behavior cho AI Advisor khi consume Phase 1, Phase 2, Phase 3.

Không cho phép: Không tự tính giá, tồn, sellable, discount, quyền lợi thành viên, order_code, paid, verified revenue, commission, ROAS, payout.

## 1. PHASE MARKER

## PHASE-04-AI-ADVISOR-RUNTIME / P4-2A / RUNTIME-CONSUMER-CONTRACT.

File này là handoff kỹ thuật để developer/Codex/Copilot thiết kế và triển khai lớp hợp đồng tiêu thụ runtime cho AI Advisor. Đây không phải tài liệu mở Gateway, không phải Completion Decision, không phải Production Readiness.

## 2. HEADER

Tên prompt: PROMPT-P4-2A - AI ADVISOR RUNTIME CONSUMER CONTRACT

Mục đích: Buộc AI Advisor nhận dữ liệu từ runtime owner đúng phase, đúng contract, đúng trạng thái, đúng guard.

Nguyên tắc nền: AI Advisor là consultation orchestrator và runtime consumer, không phải source-of-truth của nghiệp vụ.

Kết quả mong muốn: Có lớp contract/adapter rõ ràng để mọi câu tư vấn, quote, order draft, payment/shipping status đều dựa trên dữ liệu runtime hợp lệ.

## 3. MODE

## LIMITED IMPLEMENTATION HANDOFF - RUNTIME CONSUMER CONTRACT ONLY.

Cho phép developer phân tích và triển khai lớp contract tiêu thụ runtime nếu đã đạt điều kiện entry của file này.

Không cho phép chỉnh sửa owner logic ở Product, Operational, Commerce nếu không có handoff riêng của phase tương ứng.

Không cho phép thêm logic tính giá, tính tồn, tính sellable, tính quyền lợi thành viên vào AI Advisor.

Không cho phép tạo shortcut để AI trả lời khi runtime thiếu dữ liệu.

Mọi missing/error/conflict từ runtime phải fail-closed hoặc human handoff theo rule trong file này.

## 4. SOURCE-OF-TRUTH

Nhóm truth: Foundation / RBAC / Audit / Evidence / Idempotency; Owner: PHASE 0; AI Advisor được làm gì: Consume actor, correlation, permission result, audit/evidence contract khi cần.; AI Advisor bị cấm làm gì: Không bypass permission, không tạo evidence accepted, không bỏ idempotency cho command high-risk.

Nhóm truth: Product / SKU / Recipe / Product Activation; Owner: PHASE 1; AI Advisor được làm gì: Consume public product data, public name, product group, claim whitelist, product activation status nếu public-safe.; AI Advisor bị cấm làm gì: Không tự hardcode SKU list, không tự quyết SKU active, không lộ internal SKU code/formula private.

Nhóm truth: Operational Core; Owner: PHASE 2; AI Advisor được làm gì: Consume suppression, recall/sale lock, public trace allowlist, batch/public-safe availability signal nếu được expose.; AI Advisor bị cấm làm gì: Không tự đọc QC private để bán hàng, không tự quyết batch released, warehouse receipt hay stock.

Nhóm truth: Commerce Runtime; Owner: PHASE 3; AI Advisor được làm gì: Consume sellable_status, quote_snapshot, cart, order draft, official order state, payment/shipping public-safe state.; AI Advisor bị cấm làm gì: Không tự tính final price, discount, tax, shipping, order_code, paid, verified revenue, commission, ROAS.

Nhóm truth: AI Advisor Runtime; Owner: PHASE 4; AI Advisor được làm gì: Orchestrate consultation, wording, channel handoff, product suggestion dựa trên runtime data hợp lệ.; AI Advisor bị cấm làm gì: Không trở thành owner song song của bất kỳ business truth nào.

## 5. ENTRY CONDITION

## 00-PHÂN TÍCH HIỆN TRẠNG.md đã được chạy hoặc ít nhất đã được developer đọc và lập current state/gap/điểm chặn report.

## 01-THIẾT KẾ KỸ THUẬT.docx đã được dùng làm thiết kế định hướng cho Phase 4.

Codebase hiện tại đã được inspect để biết AI Advisor đang có endpoint, service, resolver, prompt/template, channel bridge nào.

Developer xác định được provider thật của Product/SKU, Operational, Commerce Runtime trong repository hiện tại.

Chưa được mở Gateway. Mọi kết quả của file này vẫn là evidence candidate, chưa phải accepted evidence.

## 6. OBJECTIVE

Khóa và triển khai lớp Runtime Consumer Contract để AI Advisor không còn vận hành theo kiểu tự đoán, tự hardcode hoặc tự tính nghiệp vụ. Mọi quyết định có ảnh hưởng bán hàng, giá, đơn hàng, thanh toán, tồn kho, khóa bán, thu hồi, quyền lợi thành viên phải đến từ runtime owner đúng phase.

Chuẩn hóa request envelope từ AI Advisor sang runtime provider.

Chuẩn hóa response envelope runtime trả về cho AI Advisor.

Chuẩn hóa trạng thái RESOLVED / PARTIAL / MISSING / bị chặn / CONFLICT / ERROR.

Khóa fail-closed behavior khi dữ liệu runtime thiếu, lỗi, conflict hoặc bị suppression.

Tách dữ liệu public-safe và private/internal trước khi render câu trả lời cho khách.

Buộc quote/order/payment/shipping đi theo Phase 3 contract, không để AI tự tạo logic song song.

## 7. SCOPE IN

RuntimeConsumerEnvelope cho mọi request từ AI Advisor đến runtime provider.

RuntimeResolutionEnvelope cho mọi response runtime trả về.

Adapter/Client boundary cho ProductPublicData, CustomerContext, CustomerMemory, SellableStatus, QuoteSnapshot, Cart/OrderDraft, OfficialOrderState, PaymentPublicState, ShippingPublicState, Recall/SaleLock/Suppression.

Guard map để AI Advisor biết lúc nào được tư vấn, được quote, được tạo order draft, được yêu cầu xác nhận, được hiển thị order_code.

Mapping public/private data để không lộ SKU nội bộ, công thức, cost, supplier, QC note, stock quantity, trace internal.

Logging/audit/correlation cho request/response contract và command có side effect.

Smoke test chứng minh AI không tự tính và fail-closed đúng.

## 8. SCOPE OUT

Không viết lại Product Master, SKU Master, Recipe, BOM, Formula Version.

Không sửa Operational Core: raw lot, QC, release, warehouse, inventory ledger, trace, recall.

Không sửa Commerce owner logic: sellable gate, price engine, program engine, member benefit, quote snapshot, order state, payment, shipping, verified revenue.

Không tạo bảng database mới nếu chưa có technical design và migration handoff riêng.

Không tạo Gateway PASS, Completion Decision hoặc Production Readiness.

Không triển khai IVR; IVR vẫn là reserved boundary nếu chưa có phase riêng.

## 9. RUNTIME CONSUMER ARCHITECTURE

Lớp Runtime Consumer Contract nằm giữa AI Advisor Orchestrator và các provider owner. AI Advisor không đọc trực tiếp database nghiệp vụ để suy luận giá, tồn, sellable hoặc trạng thái đơn hàng. AI chỉ gọi contract/runtime adapter đã được cho phép.

Lớp: AI Conversation Input; Trách nhiệm: Nhận câu hỏi khách, kênh, session, ngữ cảnh hội thoại.; Điểm khóa: Không render ngay giá/tồn nếu chưa có runtime resolution.

Lớp: Intent & Entity Extractor; Trách nhiệm: Bắt nhu cầu, sản phẩm quan tâm, người dùng chính, dị ứng/kiêng kỵ nếu khách nêu.; Điểm khóa: Không suy diễn quyền lợi giá hoặc tồn kho.

Lớp: Runtime Consumer Adapter; Trách nhiệm: Gọi provider Phase 1/2/3 theo contract; gom kết quả thành runtime context.; Điểm khóa: Fail-closed nếu provider thiếu/lỗi/conflict.

Lớp: Guard Evaluator; Trách nhiệm: Áp suppression, sale lock, recall, channel policy, public/private boundary, quote/order guard.; Điểm khóa: Guard thắng template và lực bán.

Lớp: Consultation Orchestrator; Trách nhiệm: Chọn hướng tư vấn, gợi ý SKU/combo public-safe dựa trên dữ liệu đã resolve.; Điểm khóa: Chỉ dùng SKU sellable và phù hợp guard.

Lớp: Renderer; Trách nhiệm: Sinh câu trả lời theo tone Ginsengfood và channel context.; Điểm khóa: Không lộ internal field, không dùng chữ "hệ thống" trong customer-facing.

Lớp: Handoff/Event Emitter; Trách nhiệm: Gửi quote/order confirmation/handoff event nếu channel và guard cho phép.; Điểm khóa: Command phải có actor/correlation/idempotency/audit nếu high-risk.

## 10. REQUEST ENVELOPE CONTRACT

Mọi request từ AI Advisor đến runtime provider phải có envelope thống nhất. Không cho phép service con gọi rời rạc thiếu actor, channel hoặc correlation.

Nhóm field: Identity; Field bắt buộc/khuyến nghị: actor_user_id / actor_type / customer_id / anonymous_session_id; Ý nghĩa: Xác định người đang tương tác và người được tư vấn.; Fail nếu thiếu: Thiếu actor/session thì không thực hiện command có side effect.

Nhóm field: Correlation; Field bắt buộc/khuyến nghị: correlation_id / request_id / conversation_id / channel_session_id; Ý nghĩa: Truy vết xuyên AI, Commerce, Channel, Evidence.; Fail nếu thiếu: Thiếu correlation thì không chấp nhận quote/order command.

Nhóm field: Channel; Field bắt buộc/khuyến nghị: channel_type / channel_visibility / page_id / live_session_id / messenger_thread_id; Ý nghĩa: Phân biệt public comment, Messenger, CRM, website, admin.; Fail nếu thiếu: Không được báo giá public nếu policy không cho phép.

Nhóm field: Intent; Field bắt buộc/khuyến nghị: intent_code / product_need / product_mentions / buy_signal / complaint_signal; Ý nghĩa: Định tuyến tư vấn, quote, order, complaint, handoff.; Fail nếu thiếu: Không đoán quote nếu intent chưa rõ sản phẩm/SKU.

Nhóm field: Customer Context Request; Field bắt buộc/khuyến nghị: need_member_context / need_memory / need_recipient_profile / need_dietary_guard; Ý nghĩa: Yêu cầu runtime/customer service trả ngữ cảnh phù hợp.; Fail nếu thiếu: Thiếu member context thì không tự gán benefit.

Nhóm field: Product Runtime Request; Field bắt buộc/khuyến nghị: product_ids_or_slugs / public_product_names / category_need / alternative_required; Ý nghĩa: Yêu cầu danh sách sản phẩm hợp lệ để tư vấn.; Fail nếu thiếu: Không hardcode SKU thay thế.

Nhóm field: Commerce Request; Field bắt buộc/khuyến nghị: need_sellable / need_quote / quote_items / quantity / program_context / shipping_area; Ý nghĩa: Yêu cầu sellable/quote/order draft từ Commerce.; Fail nếu thiếu: Không tự tính price nếu không có quote_snapshot.

Nhóm field: Safety; Field bắt buộc/khuyến nghị: known_allergy / dietary_restriction / health_sensitive_flag / privacy_request_flag; Ý nghĩa: Kích hoạt guard public wording, handoff hoặc suppression.; Fail nếu thiếu: Không tư vấn SKU mâu thuẫn dị ứng/kiêng kỵ đã biết.

## 11. RESPONSE ENVELOPE CONTRACT

Runtime provider phải trả về envelope đủ trạng thái. AI Advisor chỉ được render khi dữ liệu cần thiết ở trạng thái RESOLVED hoặc được phép PARTIAL theo rule.

Nhóm response: customer_context; Trạng thái hợp lệ: RESOLVED / PARTIAL / MISSING; AI được dùng để làm gì: Xác định khách mới/cũ, member tier, lịch sử, người nhận quà.; AI phải làm gì nếu missing/bị chặn: Không tự nhận member benefit; hỏi mở rộng hoặc handoff nếu cần.

Nhóm response: product_public_data; Trạng thái hợp lệ: RESOLVED / MISSING / bị chặn; AI được dùng để làm gì: Tư vấn tên công khai, nhóm sản phẩm, vị, hiệu dụng public-safe.; AI phải làm gì nếu missing/bị chặn: Không tư vấn sản phẩm không có public data hợp lệ.

Nhóm response: sellable_status; Trạng thái hợp lệ: SELLABLE / NOT_SELLABLE / UNKNOWN / bị chặn; AI được dùng để làm gì: Quyết định có được đưa vào quote hoặc chốt không.; AI phải làm gì nếu missing/bị chặn: UNKNOWN/bị chặn phải fail-closed, không bán.

Nhóm response: suppression_state; Trạng thái hợp lệ: NONE / RECALL_LOCK / SALE_LOCK / CHANNEL_SUPPRESSED / POLICY_BLOCKED; AI được dùng để làm gì: Chặn tư vấn bán hàng hoặc chuyển flow phù hợp.; AI phải làm gì nếu missing/bị chặn: Suppression thắng mọi template bán hàng.

Nhóm response: quote_snapshot; Trạng thái hợp lệ: CREATED / EXPIRED / REJECTED / MISSING; AI được dùng để làm gì: Hiển thị final price và quote expiry.; AI phải làm gì nếu missing/bị chặn: No QuoteSnapshot thì không báo final price.

Nhóm response: order_draft; Trạng thái hợp lệ: CREATED / WAITING_CUSTOMER_CONFIRMATION / REJECTED / MISSING; AI được dùng để làm gì: Hiển thị form xác nhận đơn.; AI phải làm gì nếu missing/bị chặn: Không gọi là đơn chính thức.

Nhóm response: official_order_state; Trạng thái hợp lệ: CREATED_WITH_ORDER_CODE / NOT_CREATED / CANCELLED / bị chặn; AI được dùng để làm gì: Chỉ khi có order_code mới nói đơn đã ghi nhận chính thức.; AI phải làm gì nếu missing/bị chặn: Không có order_code thì không nói đã lên đơn.

Nhóm response: payment_public_state; Trạng thái hợp lệ: UNPAID / SELECTED / WAITING_CONFIRMATION / PAID_CONFIRMED / FAILED; AI được dùng để làm gì: Trả lời trạng thái thanh toán public-safe.; AI phải làm gì nếu missing/bị chặn: Selected không đồng nghĩa Paid.

Nhóm response: shipping_public_state; Trạng thái hợp lệ: NOT_READY / READY_TO_SHIP / HANDED_TO_CARRIER / IN_TRANSIT / DELIVERED / ISSUE; AI được dùng để làm gì: Trả lời trạng thái vận chuyển public-safe.; AI phải làm gì nếu missing/bị chặn: Không tự nói delivered nếu chưa có state.

Nhóm response: handoff_flags; Trạng thái hợp lệ: NONE / HUMAN_REQUIRED / PRIVATE_CHANNEL_REQUIRED / COMPLAINT_REQUIRED; AI được dùng để làm gì: Định tuyến sang CSKH, Messenger, complaint, admin.; AI phải làm gì nếu missing/bị chặn: Không cố chốt khi handoff_required=true.

## 12. OWNER AND PROVIDER MATRIX

Runtime value: product_public_name; Provider bắt buộc: Product Public Data Resolver / Phase 1; AI Advisor consume như thế nào: Dùng để gọi tên sản phẩm với khách.; Cấm trong AI Advisor: Không lộ SKU code nội bộ nếu khách không cần.

Runtime value: product_group; Provider bắt buộc: Product Master / Phase 1; AI Advisor consume như thế nào: Dùng cho tư vấn mùa vụ, chức năng, bổ dưỡng.; Cấm trong AI Advisor: Không tự tạo group mới trong prompt.

Runtime value: ingredient_public_summary; Provider bắt buộc: Product Knowledge Public View / Phase 1; AI Advisor consume như thế nào: Dùng để giải thích vị, nền tảng dinh dưỡng public-safe.; Cấm trong AI Advisor: Không lộ công thức tỷ lệ, BOM, supplier private.

Runtime value: recall_or_sale_lock; Provider bắt buộc: Operational Core / Phase 2; AI Advisor consume như thế nào: Dùng để suppress sale và chuyển CSKH nếu cần.; Cấm trong AI Advisor: Không bỏ qua để chốt đơn.

Runtime value: sellable_status; Provider bắt buộc: Commerce Runtime / Phase 3; AI Advisor consume như thế nào: Dùng làm điều kiện bán/chốt/quote.; Cấm trong AI Advisor: Không tự suy từ active/tồn kho.

Runtime value: listed_price; Provider bắt buộc: Commerce Pricing / Phase 3; AI Advisor consume như thế nào: Chỉ hiển thị khi nằm trong quote snapshot.; Cấm trong AI Advisor: Không hardcode giá trong template.

Runtime value: program_discount; Provider bắt buộc: Commerce Program Engine / Phase 3; AI Advisor consume như thế nào: Chỉ hiển thị theo quote snapshot.; Cấm trong AI Advisor: Không tự tính Giờ Vàng/24-7.

Runtime value: member_benefit; Provider bắt buộc: Commerce Member Benefit Resolver / Phase 3; AI Advisor consume như thế nào: Chỉ hiển thị nếu runtime trả eligible trong quote.; Cấm trong AI Advisor: Không tự gán Silver/Gold/Platinum/Diamond discount.

Runtime value: shipping_fee; Provider bắt buộc: Commerce Shipping / Phase 3; AI Advisor consume như thế nào: Chỉ hiển thị theo quote/order draft.; Cấm trong AI Advisor: Không tự ước lượng nếu chưa có runtime.

Runtime value: VAT/tax; Provider bắt buộc: Commerce/Finance boundary / Phase 3; AI Advisor consume như thế nào: Chỉ hiển thị theo quote/order/invoice public-safe.; Cấm trong AI Advisor: Không tự tính VAT/tax trong AI.

Runtime value: order_code; Provider bắt buộc: Official Order Service / Phase 3; AI Advisor consume như thế nào: Chỉ hiển thị sau CustomerConfirmation và OfficialOrder created.; Cấm trong AI Advisor: Không tự sinh mã đơn.

Runtime value: paid_status; Provider bắt buộc: Payment Core / Phase 3; AI Advisor consume như thế nào: Trả lời đúng paid confirmed hoặc WAITING.; Cấm trong AI Advisor: Không nói đã thanh toán khi khách mới chọn chuyển khoản/COD.

Runtime value: verified_revenue; Provider bắt buộc: Commerce/Finance/ROAS boundary / Phase 3; AI Advisor consume như thế nào: Chỉ dùng downstream post-order/Ads/ROAS nếu có quyền.; Cấm trong AI Advisor: Không dùng để tư vấn khách và không tạo payout.

## 13. CONTRACT RESOLUTION STATUS

Status: RESOLVED; Định nghĩa: Runtime trả đủ dữ liệu cần thiết, không conflict, không suppression.; AI Advisor được phép: Render câu trả lời hoặc tiếp tục flow theo guard.; AI Advisor không được phép: Không override dữ liệu runtime.

Status: PARTIAL; Định nghĩa: Có dữ liệu một phần; thiếu field không ảnh hưởng quyết định hiện tại.; AI Advisor được phép: Tư vấn phần an toàn hoặc hỏi mở rộng đúng trọng tâm.; AI Advisor không được phép: Không quote/chốt nếu thiếu field ảnh hưởng giá/sellable/order.

Status: MISSING; Định nghĩa: Provider không trả được dữ liệu bắt buộc.; AI Advisor được phép: Ask clarification hoặc human handoff tùy field.; AI Advisor không được phép: Không tự điền bằng prompt/template.

Status: UNKNOWN; Định nghĩa: Runtime không xác định được trạng thái.; AI Advisor được phép: Fail-closed với sellable/price/order/payment.; AI Advisor không được phép: Không bán như thể sellable.

Status: bị chặn; Định nghĩa: Runtime xác định bị khóa bởi recall, sale lock, channel policy, privacy, permission.; AI Advisor được phép: Suppress sale, xin thông tin cần thiết hoặc chuyển CSKH.; AI Advisor không được phép: Không vòng qua guard để chốt đơn.

Status: CONFLICT; Định nghĩa: Dữ liệu giữa provider hoặc snapshot không nhất quán.; AI Advisor được phép: Stop flow, log evidence, handoff.; AI Advisor không được phép: Không chọn giá thấp/cao tùy ý để trả lời khách.

Status: ERROR; Định nghĩa: Lỗi kỹ thuật khi gọi provider.; AI Advisor được phép: Thông báo public-safe và handoff nếu cần.; AI Advisor không được phép: Không bịa dữ liệu hoặc quote tạm.

## 14. FAIL-CLOSED RULE REGISTRY

Rule ID: P4-2A-RULE-001; Điều kiện: No QuoteSnapshot; Hành vi bắt buộc: Không báo final price; yêu cầu runtime tạo quote hoặc chuyển Messenger nếu cần.; Evidence cần có: Log request/response chứng minh không có quote_snapshot.

Rule ID: P4-2A-RULE-002; Điều kiện: sellable_status != SELLABLE; Hành vi bắt buộc: Không chốt, không thêm vào quote; gợi ý tối đa 3 sản phẩm thay thế nếu runtime trả hợp lệ.; Evidence cần có: Smoke not-sellable suppression.

Rule ID: P4-2A-RULE-003; Điều kiện: recall_or_sale_lock active; Hành vi bắt buộc: Dừng tư vấn bán hàng SKU liên quan; route CSKH/complaint theo policy.; Evidence cần có: Operational lock response + AI suppression log.

Rule ID: P4-2A-RULE-004; Điều kiện: member_tier/benefit missing; Hành vi bắt buộc: Không tự áp quyền lợi; báo theo runtime hoặc hỏi xác minh trong private.; Evidence cần có: Quote without member benefit proof.

Rule ID: P4-2A-RULE-005; Điều kiện: Public channel hỏi giá/chốt đơn; Hành vi bắt buộc: Không hardcode giá public; kéo Messenger/private nếu policy cho phép.; Evidence cần có: Channel smoke comment-to-private.

Rule ID: P4-2A-RULE-006; Điều kiện: Customer chọn thanh toán nhưng Payment Core chưa confirm; Hành vi bắt buộc: Nói đang chờ xác nhận/đã chọn phương thức; không nói paid.; Evidence cần có: Payment state smoke.

Rule ID: P4-2A-RULE-007; Điều kiện: Order Draft chưa Customer Confirmation; Hành vi bắt buộc: Không nói đơn chính thức; render form xác nhận nếu được phép.; Evidence cần có: Order draft state smoke.

Rule ID: P4-2A-RULE-008; Điều kiện: Official Order chưa có order_code; Hành vi bắt buộc: Không nói đã ghi nhận đơn chính thức.; Evidence cần có: No-order-code negative test.

Rule ID: P4-2A-RULE-009; Điều kiện: Runtime conflict; Hành vi bắt buộc: Stop flow, log điểm chặn, handoff human.; Evidence cần có: Conflict matrix evidence.

Rule ID: P4-2A-RULE-010; Điều kiện: Private/internal field xuất hiện trong render payload; Hành vi bắt buộc: Block response hoặc sanitize trước khi gửi.; Evidence cần có: Leak prevention test.

## 15. PUBLIC / PRIVATE FIELD BOUNDARY

Nhóm dữ liệu: Product; Public-safe có thể dùng: Tên công khai, nhóm sản phẩm, mô tả vị, định vị dinh dưỡng, hiệu dụng thực dưỡng public-safe.; Private/internal phải chặn: SKU code nội bộ khi không cần, formula ratio, BOM, cost, supplier, QC note, batch internal.

Nhóm dữ liệu: Inventory; Public-safe có thể dùng: "Hiện có thể lên đơn" hoặc "hiện chưa thể lên đơn" nếu runtime cho phép.; Private/internal phải chặn: Số lượng tồn kho cụ thể, warehouse location, reserved stock, allocation rule.

Nhóm dữ liệu: Pricing; Public-safe có thể dùng: Listed price, program price, member benefit, total payable chỉ từ QuoteSnapshot.; Private/internal phải chặn: Công thức nội bộ, cost, margin, commission rule internal nếu không dành cho khách.

Nhóm dữ liệu: Trace/Recall; Public-safe có thể dùng: Thông tin trace công khai theo whitelist.; Private/internal phải chặn: Internal trace graph, QC investigation note, recall exposure private, supplier/private evidence.

Nhóm dữ liệu: Payment; Public-safe có thể dùng: Chờ thanh toán, đã chọn phương thức, đang chờ xác nhận, đã xác nhận thanh toán nếu Payment Core trả.; Private/internal phải chặn: Bank reconciliation private, accounting internal note, fraud risk scoring.

Nhóm dữ liệu: Customer Memory; Public-safe có thể dùng: Nhu cầu đã chia sẻ, lịch sử mua public-safe nếu cần chăm sóc.; Private/internal phải chặn: Dữ liệu nhạy cảm, ghi chú nội bộ, fraud/troll flag, blacklist reason private.

## 16. ADAPTER / RESOLVER CONTRACT LIST

Adapter/Resolver: ProductPublicDataConsumer; Input chính: product mention, product_id, category_need; Output chính: public_product_profile, claim_allowed, dietary_tags; Guard đặc biệt: Không trả private formula/cost.

Adapter/Resolver: CustomerContextConsumer; Input chính: customer_id/session/channel identity; Output chính: customer_type, member_tier, known_recipient, privacy flags; Guard đặc biệt: Không tự tạo member tier nếu missing.

Adapter/Resolver: CustomerMemoryConsumer; Input chính: customer_id, consent flags, topic scope; Output chính: purchase history summary, preference summary, care journey hints; Guard đặc biệt: Tôn trọng privacy/no-message/no-store.

Adapter/Resolver: SellableRuntimeConsumer; Input chính: product_id/SKU, channel, quantity, customer context; Output chính: SELLABLE/NOT_SELLABLE/bị chặn + public-safe reason; Guard đặc biệt: UNKNOWN phải fail-closed.

Adapter/Resolver: QuoteSnapshotConsumer; Input chính: cart lines, customer identity, program context, shipping context; Output chính: quote_snapshot_id, lines, discount, benefit, VAT, shipping, total, expiry; Guard đặc biệt: Quote immutable; không mutate trong AI.

Adapter/Resolver: OrderDraftConsumer; Input chính: quote_snapshot_id, recipient/shipping/payment selections; Output chính: order_draft_id, confirmation payload, required missing fields; Guard đặc biệt: Draft không phải official order.

Adapter/Resolver: OfficialOrderStateConsumer; Input chính: order_draft_id/customer confirmation event; Output chính: order_code, official state, timestamps; Guard đặc biệt: No order_code, no official message.

Adapter/Resolver: PaymentPublicStateConsumer; Input chính: order_code/payment intent; Output chính: public payment state; Guard đặc biệt: Selected không paid.

Adapter/Resolver: ShippingPublicStateConsumer; Input chính: order_code/shipping_id; Output chính: public shipping state; Guard đặc biệt: Delivered chỉ khi runtime xác nhận.

Adapter/Resolver: SuppressionConsumer; Input chính: product/customer/channel/context; Output chính: sale_lock, recall_lock, channel suppression, complaint route; Guard đặc biệt: Suppression thắng sales template.

Adapter/Resolver: ChannelPolicyConsumer; Input chính: channel_type, public/private, live/session; Output chính: allowed actions, required private handoff, spam/fatigue flags; Guard đặc biệt: Không quote public nếu channel cấm.

## 17. QUOTE AND ORDER CONSUMPTION CONTRACT

AI Advisor được phép dẫn dắt khách hàng đến quote/order draft, nhưng mọi số liệu thanh toán phải lấy từ Commerce Runtime. Quy tắc dưới đây phải được test âm tính và dương tính.

Bước: Khách hỏi giá; Runtime owner: Commerce Pricing/Program/Member Resolver; AI Advisor action: Gọi runtime để tạo hoặc lấy QuoteSnapshot hợp lệ; nếu public channel thì chuyển private theo policy.; Cấm tuyệt đối: Không báo giá từ prompt, template, cache không còn hiệu lực.

Bước: Khách muốn mua; Runtime owner: Commerce Cart/Quote; AI Advisor action: Tạo quote/order draft theo payload runtime trả.; Cấm tuyệt đối: Không tự gom combo với SKU chưa sellable.

Bước: Khách dùng thông tin cũ; Runtime owner: Customer/Commerce Order Draft; AI Advisor action: Render form prefill nếu runtime cho phép và vẫn yêu cầu xác nhận.; Cấm tuyệt đối: Không tạo official order chỉ vì có thông tin cũ.

Bước: Khách xác nhận; Runtime owner: Commerce Customer Confirmation; AI Advisor action: Gửi confirmation event nếu channel/permission hợp lệ.; Cấm tuyệt đối: Không bypass CustomerConfirmation.

Bước: Official Order; Runtime owner: Commerce Official Order; AI Advisor action: Chỉ hiển thị order_code khi runtime trả CREATED_WITH_ORDER_CODE.; Cấm tuyệt đối: Không tự sinh mã đơn hoặc nói đã ghi nhận đơn trước.

Bước: Thanh toán; Runtime owner: Payment Core; AI Advisor action: Trả lời public-safe theo payment_state.; Cấm tuyệt đối: Không nói paid khi mới chọn COD/chuyển khoản.

Bước: Giao hàng; Runtime owner: Shipping Runtime; AI Advisor action: Trả lời public-safe theo shipping_state.; Cấm tuyệt đối: Không tự cam kết trạng thái/vận đơn nếu runtime chưa có.

## 18. CUSTOMER MEMORY CONSUMPTION BOUNDARY

Customer Memory giúp AI tư vấn tự nhiên hơn, nhưng không được biến thành nguồn tính giá hoặc quyền lợi. Tất cả giá và quyền lợi cuối cùng vẫn do Commerce Runtime quyết định.

Memory type: Lịch sử mua; Được dùng để: Gợi ý mua lại, gợi ý dòng liên quan, chăm sóc sau mua.; Không được dùng để: Tự xác định tier/discount/final price.; Điều kiện an toàn: Member benefit phải từ runtime.

Memory type: Nhu cầu dinh dưỡng; Được dùng để: Chọn sản phẩm phù hợp hơn, hỏi mở rộng đúng trọng tâm.; Không được dùng để: Đưa claim y tế/chữa bệnh.; Điều kiện an toàn: Qua Safety/Claim Guard.

Memory type: Người nhận quà/người dùng chính; Được dùng để: Gợi ý combo phù hợp gia đình/cha mẹ/con nhỏ.; Không được dùng để: Tự kết luận bệnh nền nếu khách không nói.; Điều kiện an toàn: Không hỏi y khoa lan man.

Memory type: Dị ứng/kiêng kỵ; Được dùng để: Loại sản phẩm không phù hợp, hỏi thêm nếu cần.; Không được dùng để: Bỏ qua để chốt đơn.; Điều kiện an toàn: Guard thắng sales.

Memory type: Communication preference; Được dùng để: Giảm làm phiền, chọn kênh phù hợp.; Không được dùng để: Gửi CRM trái consent hoặc quá tần suất.; Điều kiện an toàn: Tôn trọng fatigue/privacy flags.

## 19. CHANNEL CONSUMPTION BOUNDARY

Channel context: Live public comment; AI Advisor được phép: Trả lời ngắn, kích hoạt private/Messenger nếu policy cho phép; không báo giá chi tiết nếu policy cấm public quote.; AI Advisor phải tránh: Không lặp public format sau khi đã handoff private.

Channel context: Messenger/private; AI Advisor được phép: Tư vấn sâu, quote, order draft, prefill form, confirmation CTA theo runtime.; AI Advisor phải tránh: Không hỏi lại thông tin runtime đã có.

Channel context: CRM proactive; AI Advisor được phép: Gợi ý chăm sóc/mua lại theo memory và sellable runtime.; AI Advisor phải tránh: Không gợi ý SKU not sellable/OOS/recall/sale lock.

Channel context: Website/landing page; AI Advisor được phép: Hiển thị public-safe product consultation, CTA vào quote/private nếu cần.; AI Advisor phải tránh: Không hardcode giá runtime trong static content.

Channel context: Admin/CSKH console; AI Advisor được phép: Hiển thị dữ liệu có phân quyền, audit, evidence, handoff.; AI Advisor phải tránh: Không lộ private data sang customer-facing renderer.

## 20. SECURITY / AUDIT / IDEMPOTENCY REQUIREMENTS

Mọi command có side effect như create quote, create order draft, send customer confirmation, create official order event phải có actor context, correlation_id và permission phù hợp.

QuoteSnapshot phải immutable; AI Advisor chỉ nhận và hiển thị, không sửa line/discount/total trong bộ nhớ hội thoại.

Idempotency bắt buộc cho command có nguy cơ tạo side effect lặp: tạo quote, tạo order draft, gửi confirmation, submit handoff event.

Audit log phải ghi request intent, runtime status, suppression applied, channel visibility, quote/order identifiers nếu có, nhưng không log lộ dữ liệu nhạy cảm không cần thiết.

Evidence ở file này chỉ là submitted evidence cho phase; accepted evidence thuộc Gateway/Completion governance, không tự động PASS.

## 21. EXECUTION STEPS

Inspect codebase để xác định vị trí AI Advisor orchestrator, resolver, template renderer, channel bridge, quote/order integration hiện có.

Lập danh sách mọi nơi AI đang hardcode giá, SKU, discount, member benefit, sellable, tồn kho, order_code, paid, revenue.

Đối chiếu với owner/provider matrix của file này và đánh dấu chỗ phải chuyển sang runtime consumer adapter.

Thiết kế RuntimeConsumerEnvelope và RuntimeResolutionEnvelope theo style hiện có của codebase, không tạo convention lạ nếu repository đã có chuẩn.

Triển khai hoặc map adapter cho từng provider có sẵn; provider chưa có thì tạo interface/handoff stub fail-closed, không fake data.

Cập nhật Guard Evaluator để xử lý RESOLVED/PARTIAL/MISSING/UNKNOWN/bị chặn/CONFLICT/ERROR.

Cập nhật renderer để chỉ dùng public-safe fields và chặn internal leak.

Chạy grep/static audit để chứng minh không còn hardcode giá/discount/member benefit/sellable trong AI layer.

Chạy unit/integration/smoke tests theo ma trận file này.

Lập báo cáo 14 mục kèm evidence, ảnh/log/build/test result và rủi ro còn lại.

## 22. IMPLEMENTATION CHECKLIST

Checklist ID: P4-2A-CHK-001; Hạng mục: Runtime Consumer Envelope; Điều kiện đạt: Có request envelope thống nhất chứa actor, correlation, channel, intent, customer/product/commerce request.

Checklist ID: P4-2A-CHK-002; Hạng mục: Runtime Resolution Envelope; Điều kiện đạt: Có response envelope thống nhất chứa resolution status và block reason public-safe.

Checklist ID: P4-2A-CHK-003; Hạng mục: Sellable Consumption; Điều kiện đạt: AI chỉ bán/quote khi sellable_status=SELLABLE từ Commerce Runtime.

Checklist ID: P4-2A-CHK-004; Hạng mục: QuoteSnapshot Consumption; Điều kiện đạt: AI chỉ báo final price từ QuoteSnapshot, có quote_expiry.

Checklist ID: P4-2A-CHK-005; Hạng mục: Order Draft Boundary; Điều kiện đạt: Order Draft được render là bản chờ xác nhận, không phải official order.

Checklist ID: P4-2A-CHK-006; Hạng mục: Order Code Boundary; Điều kiện đạt: Chỉ khi official_order_state có order_code mới nói đơn chính thức.

Checklist ID: P4-2A-CHK-007; Hạng mục: Payment Boundary; Điều kiện đạt: Selected/WAITING không bị nói thành Paid.

Checklist ID: P4-2A-CHK-008; Hạng mục: Suppression Boundary; Điều kiện đạt: Recall/Sale Lock/Channel Suppression thắng mọi câu chốt đơn.

Checklist ID: P4-2A-CHK-009; Hạng mục: Public/Private Boundary; Điều kiện đạt: Renderer không lộ internal SKU code, formula, cost, stock quantity, QC/supplier/private note.

Checklist ID: P4-2A-CHK-010; Hạng mục: No Hardcode Audit; Điều kiện đạt: Không còn hardcode price/discount/member benefit/sellable trong AI layer.

Checklist ID: P4-2A-CHK-011; Hạng mục: Correlation/Audit; Điều kiện đạt: Request/response và command side effect có correlation/audit/idempotency phù hợp.

Checklist ID: P4-2A-CHK-012; Hạng mục: Fail-Closed Behavior; Điều kiện đạt: Missing/Unknown/Conflict/Error không tạo câu trả lời bịa dữ liệu.

## 23. SMOKE REQUIRED

Smoke ID: P4-2A-SMK-001; Kịch bản: Khách hỏi giá sản phẩm ở Messenger nhưng runtime không trả QuoteSnapshot.; Expected Result: AI không báo final price; yêu cầu tạo quote hoặc handoff.

Smoke ID: P4-2A-SMK-002; Kịch bản: Khách hỏi giá ở Live public comment.; Expected Result: AI không hardcode giá public; kéo Messenger/private theo channel policy.

Smoke ID: P4-2A-SMK-003; Kịch bản: SKU khách hỏi có sellable_status=NOT_SELLABLE.; Expected Result: AI không chốt SKU đó; gợi ý thay thế hợp lệ nếu runtime trả.

Smoke ID: P4-2A-SMK-004; Kịch bản: SKU bị recall/sale lock.; Expected Result: AI suppress sale và route CSKH/complaint đúng public-safe wording.

Smoke ID: P4-2A-SMK-005; Kịch bản: Khách là Diamond nhưng quote runtime không trả member benefit eligible.; Expected Result: AI không tự cộng/giảm Diamond; hiển thị đúng theo QuoteSnapshot hoặc handoff.

Smoke ID: P4-2A-SMK-006; Kịch bản: Order Draft đã tạo nhưng chưa Customer Confirmation.; Expected Result: AI render form xác nhận; không nói đơn chính thức.

Smoke ID: P4-2A-SMK-007; Kịch bản: Customer Confirmation gửi xong nhưng Official Order chưa trả order_code.; Expected Result: AI không tự nói đã ghi nhận đơn.

Smoke ID: P4-2A-SMK-008; Kịch bản: Khách chọn chuyển khoản nhưng Payment Core chưa xác nhận.; Expected Result: AI nói đang chờ xác nhận; không nói đã thanh toán.

Smoke ID: P4-2A-SMK-009; Kịch bản: Runtime trả CONFLICT giữa quote và sellable.; Expected Result: AI dừng flow, log điểm chặn, human handoff.

Smoke ID: P4-2A-SMK-010; Kịch bản: Renderer nhận payload có internal SKU code/formula/cost.; Expected Result: AI sanitize/block, không gửi cho khách.

Smoke ID: P4-2A-SMK-011; Kịch bản: CRM gợi ý mua lại SKU đang not sellable.; Expected Result: AI/CRM không gửi gợi ý đó; chọn alternative hoặc suppress.

Smoke ID: P4-2A-SMK-012; Kịch bản: Same idempotency key tạo order draft lặp.; Expected Result: Không tạo side effect lần hai; same payload trả kết quả cũ, different payload conflict.

## 24. EVIDENCE REQUIRED

Sơ đồ hoặc mô tả vị trí Runtime Consumer Adapter trong codebase thật.

Danh sách file đã sửa và lý do sửa.

Contract request/response mẫu đã redacted dữ liệu nhạy cảm.

Ảnh/log test chứng minh AI không hardcode giá, discount, member benefit, sellable.

Test result cho 12 smoke bắt buộc.

Backend build result và frontend build result nếu có thay đổi tương ứng.

Static grep/audit result cho các từ khóa price, discount, member, sellable, stock, order_code, paid trong AI layer.

Evidence về fail-closed cho MISSING/UNKNOWN/bị chặn/CONFLICT/ERROR.

Evidence public/private boundary không lộ dữ liệu nội bộ.

Handoff update cho file tiếp theo P4-2B nếu Customer Memory còn thiếu provider hoặc privacy guard.

## 25. REQUIRED REPORT FORMAT - 14 MỤC

STT: 1; Mục báo cáo bắt buộc: Tóm tắt; Nội dung phải có: Đã làm gì, mục tiêu đạt/chưa đạt, Gateway còn bị chặn.

STT: 2; Mục báo cáo bắt buộc: File đã sửa; Nội dung phải có: Liệt kê file, module, lý do sửa, loại thay đổi.

STT: 3; Mục báo cáo bắt buộc: Nguồn yêu cầu; Nội dung phải có: Dẫn PHASE 4, P4-2A, Phase 0/1/2/3 owner truth liên quan.

STT: 4; Mục báo cáo bắt buộc: Evidence đã dùng; Nội dung phải có: Code evidence, API evidence, runtime response, test data, log đã redacted.

STT: 5; Mục báo cáo bắt buộc: Lệnh đã chạy; Nội dung phải có: Test, build, lint, grep/static audit, migration nếu có.

STT: 6; Mục báo cáo bắt buộc: Kết quả test; Nội dung phải có: Unit/integration/smoke; pass/fail; link hoặc ảnh evidence.

STT: 7; Mục báo cáo bắt buộc: Kết quả backend build; Nội dung phải có: Command, status, lỗi còn lại nếu có.

STT: 8; Mục báo cáo bắt buộc: Kết quả frontend build; Nội dung phải có: Command, status, lỗi còn lại nếu có.

STT: 9; Mục báo cáo bắt buộc: Kết quả cleanup process; Nội dung phải có: Format, lint, remove debug, remove fake data, remove temporary bypass.

STT: 10; Mục báo cáo bắt buộc: Cập nhật Markdown; Nội dung phải có: README/handoff/doc cập nhật nếu có.

STT: 11; Mục báo cáo bắt buộc: Kết quả database migration/update nếu áp dụng; Nội dung phải có: Nếu không áp dụng ghi rõ NOT APPLICABLE.

STT: 12; Mục báo cáo bắt buộc: Kết quả seed validation nếu áp dụng; Nội dung phải có: Nếu không áp dụng ghi rõ NOT APPLICABLE.

STT: 13; Mục báo cáo bắt buộc: Rủi ro còn lại; Nội dung phải có: Provider thiếu, conflict, technical debt, cần owner review.

STT: 14; Mục báo cáo bắt buộc: Cập nhật handoff; Nội dung phải có: Việc bàn giao cho file P4-2B/P4-2C/P4-2D hoặc điểm chặn cho phase sau.

## 26. DONE GATE

P4-2A chỉ được xem là đạt Done Gate cục bộ khi toàn bộ điều kiện sau đúng. Done Gate này không đồng nghĩa Gateway PASS.

Runtime Consumer Envelope và Runtime Resolution Envelope được triển khai hoặc được map rõ vào contract hiện có.

AI Advisor không còn tự tính hoặc hardcode price, discount, member benefit, sellable, stock, order_code, paid, verified revenue trong phạm vi được sửa.

Quote final price chỉ đến từ QuoteSnapshot và có quote_expiry.

Order Draft không bị gọi là Official Order nếu chưa có Customer Confirmation và order_code.

Payment selected/WAITING không bị gọi là Paid.

Recall/Sale Lock/Channel Suppression thắng mọi template tư vấn/chốt đơn.

Renderer tách public/private và không lộ dữ liệu nội bộ.

12 smoke bắt buộc có evidence pass hoặc có điểm chặn rõ ràng chưa được gọi pass.

Báo cáo 14 mục hoàn chỉnh, không bỏ trống mục nào.

## 27. FAIL GATE

Chỉ cần một điều kiện dưới đây xảy ra thì P4-2A phải FAIL, không được chuyển sang file tiếp theo như đã hoàn thành.

AI Advisor vẫn tự tính hoặc hardcode final price/discount/member benefit.

AI Advisor vẫn bán/quote SKU khi sellable_status missing/unknown/not sellable.

AI Advisor vẫn bỏ qua recall/sale lock để tư vấn bán hàng.

AI Advisor vẫn nói đơn chính thức khi chưa có order_code.

AI Advisor vẫn nói Paid khi Payment Core chưa xác nhận.

AI Advisor lộ private/internal data ra customer-facing renderer.

Runtime error/missing/conflict nhưng AI vẫn bịa dữ liệu hoặc dùng giá tạm.

Không có evidence test/smoke cho các guard P0 của file.

Báo cáo không đủ 14 mục.

Bất kỳ người triển khai nào gọi Gateway PASS/Production Readiness khi chưa có accepted evidence và owner review.

## 28. DOWNSTREAM HANDOFF

File tiếp theo: 03-BỘ NHỚ KHÁCH HÀNG VÀ BỘ GIẢI NGỮ CẢNH.docx; Điều kiện nhận handoff: P4-2A xác định rõ contract customer_context/customer_memory và privacy boundary.; Nội dung chuyển tiếp: Thiết kế/triển khai sâu Customer Memory, member context, preference, recipient, consent, fatigue.

File tiếp theo: 04-ĐIỀU PHỐI TƯ VẤN KIẾN THỨC SẢN PHẨM.docx; Điều kiện nhận handoff: P4-2A xác định ProductPublicData và SellableRuntime consumer contract.; Nội dung chuyển tiếp: Thiết kế orchestrator chọn SKU/combo dựa trên product knowledge + sellable + guard.

File tiếp theo: 05-TIÊU THỤ BÁO GIÁ NHÁP VÀ XÁC NHẬN ĐƠN.docx; Điều kiện nhận handoff: P4-2A xác định QuoteSnapshot/OrderDraft/OfficialOrder/Payment boundary.; Nội dung chuyển tiếp: Đi sâu quote, order draft, confirmation, order_code, payment/shipping public state.

File tiếp theo: 06-CHẶN AN TOÀN NGÔN TỪ CÔNG KHAI VÀ TUYÊN BỐ.docx; Điều kiện nhận handoff: P4-2A xác định public/private field và claim-risk boundary.; Nội dung chuyển tiếp: Khóa wording guard, health-sensitive, claim whitelist, internal leak prevention.

File tiếp theo: 07-CẦU NỐI BÀN GIAO KÊNH MESSENGER TRỰC TIẾP VÀ CRM.docx; Điều kiện nhận handoff: P4-2A xác định ChannelPolicyConsumer và private handoff rule.; Nội dung chuyển tiếp: Đi sâu live/comment/Messenger/CRM bridge và anti-spam/fatigue/abuse policy.

## 29. PROMPT COPY GIAO DEV / CODEX / COPILOT

Dùng nguyên khối prompt dưới đây khi giao developer/Codex/Copilot thực hiện P4-2A. Không yêu cầu dev copy code từ AI; dev phải inspect codebase thật và báo cáo evidence thật.

## PROMPT-P4-2A - AI ADVISOR RUNTIME CONSUMER CONTRACT / LIMITED IMPLEMENTATION HANDOFF

## MODE: LIMITED IMPLEMENTATION - RUNTIME CONSUMER CONTRACT ONLY.

DO NOT OPEN GATEWAY. DO NOT CLAIM Production Readiness.

Bạn đang triển khai PHASE-04-AI-ADVISOR-RUNTIME, file 02-HỢP ĐỒNG TIÊU THỤ DỮ LIỆU THỜI GIAN CHẠY.

Mục tiêu: thiết kế/triển khai lớp contract để AI Advisor chỉ consume runtime truth từ Phase 1, Phase 2, Phase 3. AI Advisor không được tự tính hoặc hardcode price, discount, member benefit, sellable, stock, order_code, paid, verified revenue, commission, ROAS, payout.

Việc cần làm:

## 1. Inspect codebase thật để xác định AI Advisor orchestrator, resolver, renderer, channel bridge, quote/order integration hiện có.

## 2. Tìm toàn bộ nơi AI layer đang hardcode hoặc tự tính SKU/price/discount/member benefit/sellable/stock/order_code/paid/revenue.

## 3. Thiết kế hoặc map RuntimeConsumerEnvelope và RuntimeResolutionEnvelope theo convention hiện có.

## 4. Triển khai hoặc chuẩn hóa adapter cho ProductPublicData, CustomerContext, CustomerMemory, SellableRuntime, QuoteSnapshot, OrderDraft, OfficialOrderState, PaymentPublicState, ShippingPublicState, Suppression, ChannelPolicy.

## 5. Áp fail-closed cho MISSING/UNKNOWN/bị chặn/CONFLICT/ERROR.

## 6. Đảm bảo QuoteSnapshot là nguồn duy nhất cho final price; order_code là điều kiện duy nhất để nói Official Order; Payment Core confirmation là điều kiện duy nhất để nói Paid.

## 7. Chặn public/private leak: không lộ internal SKU code, formula, cost, supplier, QC note, stock quantity, trace internal.

## 8. Chạy smoke P4-2A-SMK-001 đến P4-2A-SMK-012.

## 9. Báo cáo đúng 14 mục: Tóm tắt; File đã sửa; Nguồn yêu cầu; Evidence đã dùng; Lệnh đã chạy; Kết quả test; Kết quả backend build; Kết quả frontend build; Kết quả cleanup process; Cập nhật Markdown; Kết quả database migration/update nếu áp dụng; Kết quả seed validation nếu áp dụng; Rủi ro còn lại; Cập nhật handoff.

Cấm:

- Không tự tính giá/tồn/sellable/discount/member benefit.

- Không tạo order_code trong AI.

- Không nói Paid khi chỉ selected/WAITING.

- Không bypass recall/sale lock.

- Không tạo fake provider data để pass test.

- Không gọi Gateway PASS.

## PHASE-04-AI-ADVISOR-RUNTIME

## 03-BỘ NHỚ KHÁCH HÀNG VÀ BỘ GIẢI NGỮ CẢNH



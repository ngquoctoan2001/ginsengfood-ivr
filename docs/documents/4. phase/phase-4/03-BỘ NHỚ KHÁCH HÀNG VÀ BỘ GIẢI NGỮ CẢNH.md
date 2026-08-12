# P4.2B - CUSTOMER MEMORY CONTEXT RESOLVER

## Khối nguồn bắt buộc đã nhập vào file này

File này có khối nguồn bắt buộc để chạy lẻ, nhưng khi bàn giao Phase 4 phải đọc `12-BẢNG GOM GIAI ĐOẠN 4.md`, `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` trước. Nếu nội dung file này khác Bảng Gôm/phụ lục/SRS handoff, áp dụng Bảng Gôm/phụ lục/SRS handoff và xử lý fail-closed.

### A. Nội dung nguồn đã nhập

1. Master/runtime governance: customer memory không phải nguồn sự thật nghiệp vụ. Product/Formula/Public Product Knowledge quyết định dữ liệu sản phẩm công khai. Operational Core quyết định sellable, recall, sale lock, batch/trace public-safe. Commerce Runtime quyết định DailySalesContext, QuoteSnapshot, OrderDraft, CustomerConfirmation, order_code, payment public-safe state, shipping/ETA và ORDER_VERIFIED. AI Advisor chỉ consume/orchestrate/render.
2. Evidence/release governance: memory resolver pass cục bộ không phải Completion PASS, Release Ready, Production Ready hoặc Go-live. Mọi gate mặc định là PENDING_EVIDENCE cho đến khi có artifact được owner chấp nhận.
3. AI Advisor customer context lock: AI được dùng memory để hiểu khách mới/cũ, nhu cầu, người dùng chính, khẩu vị, dị ứng/kiêng kỵ, lịch sử mua public-safe, kênh liên hệ, complaint/open-case signal và consent/suppression context.
4. Commerce lock: memory không được tự quyết giá, quyền lợi, quote expiry, cart, order state, paid, shipping, verified revenue. Final price chỉ đến từ QuoteSnapshot; order official chỉ khi có order_code; paid chỉ từ Payment Core.
5. Product/Operational lock: memory có thể giữ product interest public-safe nhưng không quyết SKU active/sellable, không override recall/sale lock, không expose internal SKU/formula/QC/cost/supplier/lot.
6. CRM/Member lock: member tier, lifecycle, CRM eligibility, quiet hour, opt-out, open case, complaint, fatigue và frequency cap phải là runtime/suppression state. Memory không được tự bật CRM hoặc tự suy luận quyền lợi thành viên.
7. Finance/Diamond lock: Diamond relation/referral chỉ là context read-only nếu runtime trả. Memory không được tự quyết Diamond eligible, self-purchase conflict, commission, PIT, payable hoặc payout.
8. Gateway/Ads/Live lock: channel identity, live session, ad context và Messenger thread chỉ là context/handoff input. Memory không được dùng để mở Gateway, gửi CRM, tính ROAS hoặc tự tạo delivery success.

### B. Conflict lock

- Nếu memory nói khách từng mua, nhưng Commerce runtime không có order/verified state: chỉ nói theo memory-safe wording, không xác nhận doanh thu, tier, quyền lợi hoặc order.
- Nếu khách có nhu cầu/dietary trong memory nhưng product proof thiếu: không recommend theo dietary claim.
- Nếu khách opt-out, open case, complaint hoặc quiet hour: context phải chuyển `suppressed` hoặc `human_required`, không gửi sales/CRM.
- Nếu memory có địa chỉ/số điện thoại/email: không lặp PII ở public surface; chỉ dùng private/public-safe theo policy.
- Nếu member/Diamond benefit conflict: không tự tính từ memory; cần runtime policy/QuoteSnapshot/Finance state.
- Nếu evidence thiếu trace resolve/guard: giữ PENDING_EVIDENCE, không gọi PASS.

### C. Trạng thái evidence bắt buộc

Trạng thái mặc định của file này: CUSTOMER_MEMORY_CONTEXT_LOCK_WITH_IMPORTED_SOURCE. Evidence tối thiểu phải có input context, source class, privacy classification, suppression result, resolver trace, guard result và masked PII status. Không được gọi RUNTIME_IMPLEMENTATION_READY, GATEWAY_PASS, COMPLETION_PASS, RELEASE_READY, PRODUCTION_READY hoặc GO_LIVE_APPROVED.

### D. Khóa riêng của file này

File 03 chỉ xử lý Customer Memory và Context Resolver. Nhiệm vụ là tạo context tư vấn đúng hơn nhưng không để memory thay thế runtime truth về giá, hàng, quyền lợi, đơn, thanh toán, doanh thu, CRM, Diamond hoặc Gateway.

## Nội dung triển khai

## PROMPT-P4-2B - CUSTOMER MEMORY / CONTEXT RESOLVER / PRIVACY / PERSONALIZATION / PUBLIC-SAFE RESPONSE HANDOFF

## MODE: LIMITED IMPLEMENTATION HANDOFF - CUSTOMER MEMORY AND CONTEXT RESOLVER ONLY

Tài liệu này dùng để giao dev / Codex / Copilot triển khai có kiểm soát lớp Customer Memory và Context Resolver cho AI Advisor Runtime. Đây không phải lệnh copy code rời rạc. Dev bắt buộc inspect codebase thật, schema thật, runtime contract thật, policy thật và chỉ sửa trong phạm vi đã được khóa.

Nguyên tắc khóa: AI Advisor được dùng dữ liệu khách hàng để tư vấn đúng hơn, nhớ đúng hơn, gợi ý phù hợp hơn; nhưng AI Advisor không được tự quyết giá, tồn kho, sellable, quyền lợi thành viên, trạng thái đơn, paid, verified revenue, commission, ROAS hoặc payout.

## 1. PHASE MARKER

Phase: PHASE-04 - AI ADVISOR RUNTIME

File: 03-BỘ NHỚ KHÁCH HÀNG VÀ BỘ GIẢI NGỮ CẢNH.docx

Prompt: PROMPT-P4-2B

Mode: LIMITED IMPLEMENTATION HANDOFF - CUSTOMER MEMORY AND CONTEXT RESOLVER ONLY

Implementation boundary: Chỉ xử lý Customer Memory / Context Resolver; không mở rộng sang Product Consultation Orchestrator, Quote, Order Draft, Channel Gateway hoặc CRM campaign engine.

## 2. HEADER

Tên đầy đủ: PROMPT-P4-2B - CUSTOMER MEMORY / CONTEXT RESOLVER / PRIVACY / PERSONALIZATION / PUBLIC-SAFE RESPONSE HANDOFF.

Mục tiêu của file là khóa cách AI Advisor lấy, chuẩn hóa, rút gọn và sử dụng bối cảnh khách hàng để tạo response context phục vụ tư vấn. Context này chỉ là lớp hỗ trợ tư vấn và không thay thế runtime truth của các Phase 1, Phase 2, Phase 3.

## 3. MODE

Mode thực thi: LIMITED IMPLEMENTATION HANDOFF.

Được phép inspect codebase hiện có để tìm customer memory, CRM snapshot, member resolver, conversation state, channel session và response context builder.

Được phép đề xuất hoặc triển khai interface / DTO / service nội bộ nếu codebase chưa có lớp tương đương, nhưng phải đi qua architecture thật của dự án.

Không được sinh code rời rạc để copy-paste ngoài context dự án.

Không được sửa pricing, sellable, order, payment, shipping, verified revenue, commission, ROAS hoặc payout.

Không được mở Gateway, không được tuyên bố Production Readiness, không được bỏ qua smoke / evidence.

## 4. SOURCE-OF-TRUTH

Nguồn: PHASE 0 - Foundation / RBAC / Audit / Evidence / Idempotency; Vai trò trong P4-2B: Nguồn chuẩn cho actor, permission, correlation, audit, evidence.; Rule khóa: Mọi context resolve phải có actor/correlation khi chạy trong backend; audit các tình huống nhạy cảm hoặc suppression.

Nguồn: PHASE 1 - Product / SKU / Recipe / Activation; Vai trò trong P4-2B: Nguồn chuẩn cho product public data khi cần biết khách đã quan tâm sản phẩm nào.; Rule khóa: P4-2B không quyết SKU active/sellable; chỉ giữ product reference public-safe nếu runtime cung cấp.

Nguồn: PHASE 2 - Operational Core / Recall / Sale Lock; Vai trò trong P4-2B: Nguồn chuẩn cho recall, sale lock, public trace suppression.; Rule khóa: Nếu có recall/sale lock/suppression thì context phải chuyển sang bị chặn hoặc HANDOFF_REQUIRED theo public-safe rule.

Nguồn: PHASE 3 - Commerce Runtime; Vai trò trong P4-2B: Nguồn chuẩn cho sellable, quote, cart, order draft, official order, payment, shipping.; Rule khóa: P4-2B chỉ đọc trạng thái public-safe; không tự tính hoặc tự set bất kỳ trạng thái commerce nào.

Nguồn: PHASE 4 - 02-P4-2A Runtime Consumer Contract; Vai trò trong P4-2B: Nguồn chuẩn cho inbound/outbound contract AI Advisor consume runtime.; Rule khóa: P4-2B phải tuân thủ contract 2A; không tạo contract song song.

Nguồn: Customer Data / CRM Snapshot / Member Snapshot; Vai trò trong P4-2B: Nguồn đọc bối cảnh khách hàng, lịch sử mua, consent, preference, segment, need.; Rule khóa: Read-only consumer. Không tạo source-of-truth song song cho customer data.

## 5. ENTRY CONDITION

## 00-PHÂN TÍCH HIỆN TRẠNG.md đã được dùng để phân tích hiện trạng và gap cho AI Advisor Runtime.

## 01-THIẾT KẾ KỸ THUẬT đã khóa boundary tổng thể của Phase 4.

## 02-HỢP ĐỒNG TIÊU THỤ DỮ LIỆU THỜI GIAN CHẠY đã khóa AI Advisor là runtime consumer, không phải owner business truth.

Dev đã biết vị trí codebase thật của AI Advisor, Commerce Runtime, Customer Data, CRM snapshot hoặc module tương đương.

Không có P0 điểm chặn mở làm thay đổi source-of-truth cho customer identity, member tier, order snapshot hoặc consent policy.

Nếu source customer data chưa có, P4-2B chỉ được tạo adapter/contract tạm theo fail-closed; không được tạo ownership dữ liệu khách hàng song song.

## 6. OBJECTIVE

Xây dựng hoặc chuẩn hóa lớp Customer Memory Context Resolver cho AI Advisor Runtime, nhằm tạo một response context an toàn, đủ bối cảnh và public-safe trước khi AI tạo câu trả lời tư vấn.

Context Resolver phải trả lời được: khách là ai trong phạm vi cho phép, đã từng mua gì, đang là khách mới hay khách cũ, có hạng thành viên nào theo runtime, đã từng mua cho ai, nhu cầu dinh dưỡng là gì, có kiêng kỵ/dị ứng gì, đang ở kênh nào, có được phép nhắn tiếp không, có cần handoff không, có suppression nào phải chặn không.

Context Resolver không được trả lời: giá cuối bao nhiêu, còn bao nhiêu tồn, SKU có sellable hay không nếu chưa gọi runtime, khách được giảm bao nhiêu nếu runtime chưa resolve, đơn đã chính thức hay chưa nếu Commerce chưa tạo order_code, thanh toán đã thành công hay chưa nếu Payment Core chưa xác nhận.

## 7. SCOPE IN

Customer identity context: anonymous, guest, known customer, member, linked account, suspected duplicate identity.

Customer memory summary: lịch sử mua, sản phẩm đã dùng, người nhận, mục đích mua, phản hồi khẩu vị, nhu cầu thường gặp.

Member context read-only: tier hiện hành, tier confidence, member status từ runtime hoặc snapshot được Core xác nhận.

Dietary and allergy context: ăn chay, kiêng hải sản, dị ứng, chế độ ăn đặc biệt, health-sensitive signal nếu khách tự nêu.

Communication preference: kênh đã tương tác, đồng ý/không đồng ý nhận tin, message fatigue, quiet hour, tạm dừng tin chủ động.

Channel session context: Live, Messenger, Website, CRM, Ads landing, Zalo hoặc kênh tương đương nếu dự án có.

Conversation memory trong phiên: intent hiện tại, sản phẩm khách hỏi, câu hỏi trước đó, missing fields, ask-back tối thiểu.

Context confidence scoring: đủ dữ liệu, thiếu dữ liệu, conflict, cần hỏi thêm, cần handoff.

Public/private response boundary: trường nào được đưa vào prompt public-facing, trường nào chỉ dùng nội bộ, trường nào bị cấm.

Audit/evidence requirement cho tình huống privacy, suppression, sensitive, conflict hoặc handoff.

## 8. SCOPE OUT

Không xây Pricing Engine.

Không tính final price, discount, program benefit, member benefit hoặc Diamond benefit.

Không tính stock, sellable, allocation hoặc availability.

Không tạo order_code, không set official order, không set paid, không xác nhận verified revenue.

Không tính commission, ROAS, payout hoặc KPI tài chính.

Không quyết định sản phẩm thay thế cuối cùng nếu Product Consultation Orchestrator và Sellable Gate chưa resolve.

Không tạo chiến dịch CRM hoặc gửi tin hàng loạt; chỉ trả communication preference context cho lớp channel/CRM hợp lệ.

Không chẩn đoán bệnh, không tư vấn điều trị, không biến sản phẩm thành thuốc.

Không lưu raw CCCD, tài khoản ngân hàng, hồ sơ y tế, dữ liệu nhạy cảm vượt quá policy đã khóa.

## 9. OWNERSHIP AND BOUNDARY MATRIX

Domain: Customer identity; Owner chuẩn: Customer / Identity Core; P4-2B được làm: Resolve identity status và confidence qua contract.; P4-2B không được làm: Tự hợp nhất danh tính hoặc tự tạo master customer mới nếu không có command owner.

Domain: Customer memory; Owner chuẩn: Customer Data / CRM Snapshot owner; P4-2B được làm: Đọc memory summary read-only, rút gọn thành public-safe context.; P4-2B không được làm: Tạo customer data mart song song hoặc sửa lịch sử khách hàng.

Domain: Member tier; Owner chuẩn: Member / Commerce Runtime; P4-2B được làm: Consume tier_current, member_status, tier confidence.; P4-2B không được làm: Tự suy luận tier từ tổng mua hoặc link Diamond.

Domain: Product interest; Owner chuẩn: Conversation + Product public data; P4-2B được làm: Ghi nhận sản phẩm khách đang hỏi theo public reference.; P4-2B không được làm: Hardcode SKU code nội bộ hoặc quyết định sellable.

Domain: Dietary/allergy; Owner chuẩn: Customer input + policy guard; P4-2B được làm: Gắn cờ cần lọc tư vấn, cần hỏi thêm nếu khách tự nêu.; P4-2B không được làm: Tự chẩn đoán bệnh hoặc tự loại trừ y khoa ngoài dữ liệu khách cung cấp.

Domain: Communication preference; Owner chuẩn: Messaging / Privacy policy owner; P4-2B được làm: Consume allow/suppress/quiet/fatigue flags.; P4-2B không được làm: Tự gửi CRM campaign hoặc bỏ qua yêu cầu không nhận tin.

Domain: Commerce state; Owner chuẩn: PHASE 3 Commerce Runtime; P4-2B được làm: Đọc public-safe quote/order/payment/shipping reference nếu có.; P4-2B không được làm: Tự tính giá, tạo order, set paid hoặc verified revenue.

Domain: Recall / sale lock; Owner chuẩn: PHASE 2 Operational Core; P4-2B được làm: Đọc suppression và chặn downstream response nếu cần.; P4-2B không được làm: Vượt qua recall/sale lock để tư vấn bán hàng.

## 10. CUSTOMER MEMORY CONTEXT CONTRACT

Contract dưới đây là hợp đồng logic để dev đối chiếu với codebase thật. Tên object có thể map sang DTO/service hiện hữu, nhưng ý nghĩa trường và boundary không được thay đổi.

10.1 AdvisorCustomerContextRequest

Field: request_id; Kiểu logic: string / uuid; Bắt buộc: Có; Diễn giải: Định danh request để trace.

Field: correlation_id; Kiểu logic: string / uuid; Bắt buộc: Có; Diễn giải: Liên kết audit, log, downstream runtime call.

Field: actor_context; Kiểu logic: object; Bắt buộc: Có; Diễn giải: Actor gọi request: customer/session/admin/worker, không dùng UNKNOWN để bypass.

Field: channel_context; Kiểu logic: object; Bắt buộc: Có; Diễn giải: Kênh hiện tại: Messenger, Live private, Website, CRM preview, Zalo, hoặc kênh tương đương.

Field: conversation_context; Kiểu logic: object; Bắt buộc: Có; Diễn giải: Intent, câu hỏi khách, sản phẩm đang nhắc, lịch sử phiên ngắn.

Field: customer_identifier; Kiểu logic: object/null; Bắt buộc: Không; Diễn giải: Customer ID, phone hash, channel user id, member id hoặc identity token nếu có.

Field: selected_product_ref; Kiểu logic: object/null; Bắt buộc: Không; Diễn giải: Sản phẩm khách đang hỏi dưới dạng public reference; không bắt buộc có SKU code nội bộ.

Field: privacy_scope; Kiểu logic: enum; Bắt buộc: Có; Diễn giải: PUBLIC_RESPONSE, PRIVATE_CHAT, INTERNAL_ADMIN_PREVIEW, CRM_PREVIEW.

Field: runtime_time; Kiểu logic: datetime; Bắt buộc: Có; Diễn giải: Thời điểm resolve để đồng bộ snapshot và audit.

10.2 AdvisorCustomerMemoryContext

Nhóm context: identity_context; Trường tối thiểu: identity_status, customer_id_ref, identity_confidence, duplicate_suspected; Rule khóa: Không expose raw PII trong public prompt. Nếu duplicate_suspected=true thì không dùng memory nhạy cảm để chốt đơn.

Nhóm context: member_context; Trường tối thiểu: member_status, member_tier, tier_confidence, runtime_source; Rule khóa: Chỉ đọc. Không tính tier, không tính benefit. Nếu thiếu runtime_source thì không hiển thị quyền lợi.

Nhóm context: purchase_memory; Trường tối thiểu: last_purchase_summary, favorite_products, recipient_profile, usage_purpose; Rule khóa: Chỉ summary. Không đưa đầy đủ địa chỉ, số điện thoại, giá trị đơn cũ vào public text nếu không cần.

Nhóm context: need_context; Trường tối thiểu: current_need, inferred_need_confidence, ask_back_needed; Rule khóa: Inferred need chỉ dùng để gợi ý; không được biến thành kết luận y khoa.

Nhóm context: dietary_guard_context; Trường tối thiểu: vegan, allergy_flags, restricted_items, health_sensitive_signal; Rule khóa: Nếu có dị ứng/kiêng rõ ràng thì Product Consultation phải tôn trọng guard.

Nhóm context: communication_preference; Trường tối thiểu: allowed_channels, do_not_contact, fatigue_status, quiet_hour, privacy_request_WAITING; Rule khóa: Nếu do_not_contact=true hoặc privacy_request_WAITING=true thì không kích hoạt tin chủ động.

Nhóm context: active_commerce_context; Trường tối thiểu: quote_ref, order_draft_ref, official_order_ref, payment_public_state, shipping_public_state; Rule khóa: Chỉ reference public-safe từ Commerce Runtime; không tự set state.

Nhóm context: suppression_context; Trường tối thiểu: recall_lock, sale_lock, channel_suppression, legal_or_abuse_flag; Rule khóa: Bất kỳ suppression P0 nào phải thắng tư vấn bán hàng.

Nhóm context: response_control; Trường tối thiểu: context_status, missing_fields, handoff_required, human_handoff_required, public_response_allowed; Rule khóa: Nếu conflict hoặc thiếu source truth quan trọng thì fail closed hoặc hỏi tối thiểu.

10.3 Output tuyệt đối không được chứa

final_price, discount_amount, member_discount_percent hoặc program_discount_percent nếu không lấy từ QuoteSnapshot/Commerce Runtime.

stock_quantity, warehouse_quantity, allocation quantity hoặc số lượng tồn kho nội bộ.

internal_sku_code, formula internal, supplier private data, QC notes, recall internal notes, cost hoặc margin.

raw address, raw phone, CCCD/CMND, tài khoản ngân hàng, dữ liệu thanh toán nhạy cảm, dữ liệu y tế riêng tư.

verified_revenue, commission_amount, ROAS, payout hoặc KPI tài chính.

## 11. RESOLVER CHAIN

Bước: 1; Resolver: Request / Actor Validator; Input: request_id, correlation_id, actor_context; Output: validated_request; Fail-closed rule: Thiếu actor/correlation thì trả bị chặn hoặc PARTIAL tùy context; không bypass.

Bước: 2; Resolver: Channel Context Resolver; Input: channel_context; Output: channel_scope, public/private mode; Fail-closed rule: Không rõ kênh thì không quote/order; chỉ hỏi lại tối thiểu.

Bước: 3; Resolver: Identity Resolver; Input: customer_identifier, channel user id; Output: identity_status, confidence; Fail-closed rule: Conflict identity thì không dùng memory nhạy cảm.

Bước: 4; Resolver: Consent / Privacy Resolver; Input: customer id, policy snapshot; Output: communication_preference; Fail-closed rule: do_not_contact hoặc privacy WAITING thì suppress tin chủ động.

Bước: 5; Resolver: Customer Memory Reader; Input: customer id, identity confidence; Output: purchase_memory summary; Fail-closed rule: Chỉ đọc summary; không đọc thô quá mức cần thiết.

Bước: 6; Resolver: Member Runtime Resolver; Input: customer id/member id; Output: member_context read-only; Fail-closed rule: Không có runtime truth thì không nói quyền lợi thành viên.

Bước: 7; Resolver: Conversation Need Extractor; Input: conversation_context; Output: current_need, missing_fields; Fail-closed rule: Không suy đoán y khoa; nếu thiếu thì hỏi ngắn.

Bước: 8; Resolver: Dietary / Allergy Guard; Input: customer input + memory; Output: dietary_guard_context; Fail-closed rule: Có dị ứng/kiêng kỵ thì downstream không được gợi ý trái guard.

Bước: 9; Resolver: Commerce Public State Reader; Input: quote/order refs nếu có; Output: active_commerce_context; Fail-closed rule: Không tự tạo commerce state.

Bước: 10; Resolver: Suppression Resolver; Input: recall/sale lock/channel/abuse flags; Output: suppression_context; Fail-closed rule: P0 suppression thắng tất cả response bán hàng.

Bước: 11; Resolver: Context Confidence Scorer; Input: toàn bộ context; Output: RESOLVED/PARTIAL/HANDOFF/bị chặn; Fail-closed rule: Conflict P0 thì bị chặn hoặc HANDOFF_REQUIRED.

Bước: 12; Resolver: Public Response Context Builder; Input: safe context; Output: prompt-safe context; Fail-closed rule: Chỉ đưa trường được public/private policy cho phép.

## 12. CONTEXT STATUS STATE MACHINE

Status: RESOLVED; Ý nghĩa: Đủ context cần thiết và không có conflict P0.; AI được làm: Tư vấn theo context, gợi ý phù hợp, chuyển sang Product Consultation nếu cần.; AI không được làm: Không tự tính price/sellable/benefit.

Status: PARTIAL_NEEDS_ASKBACK; Ý nghĩa: Thiếu trường quan trọng nhưng có thể hỏi ngắn để tiếp tục.; AI được làm: Hỏi một câu tối thiểu, ví dụ mua cho ai, ăn chay/kiêng gì, muốn dùng hay biếu.; AI không được làm: Không hỏi lan man nếu runtime đã có dữ liệu.

Status: SUPPRESSED; Ý nghĩa: Privacy, do-not-contact, fatigue hoặc channel policy chặn chủ động.; AI được làm: Dừng tin chủ động hoặc render xác nhận tiếp nhận yêu cầu theo policy.; AI không được làm: Không tiếp tục CRM/upsell.

Status: HANDOFF_REQUIRED; Ý nghĩa: Dữ liệu conflict hoặc cần người xử lý.; AI được làm: Chuyển human handoff với summary nội bộ.; AI không được làm: Không tự kết luận hoặc xử lý trái thẩm quyền.

Status: bị chặn; Ý nghĩa: Recall/sale lock/legal/high-risk privacy P0.; AI được làm: Dừng luồng bán hàng và trả public-safe response nếu được phép.; AI không được làm: Không quote, không order, không gợi ý SKU bị chặn.

## 13. PERSONALIZATION RULE REGISTRY

Rule ID: P4-2B-R01; Rule: Memory dùng để cá nhân hóa, không dùng để tự quyết quyền lợi giá.; Expected behavior: AI có thể nói "Mình từng dùng dòng thanh nhẹ nên Em gợi ý..." nhưng không nói giảm bao nhiêu nếu runtime chưa quote.

Rule ID: P4-2B-R02; Rule: Khách cũ có thể dùng thông tin cũ chỉ khi Commerce/Order Draft Prefill cung cấp payload hợp lệ.; Expected behavior: Context Resolver chỉ gắn flag old_info_available; Order Draft Consumer mới render form xác nhận.

Rule ID: P4-2B-R03; Rule: Member tier phải từ runtime/member snapshot hợp lệ.; Expected behavior: Không tự tính tier từ tổng chi tiêu, không suy luận Diamond từ link giới thiệu.

Rule ID: P4-2B-R04; Rule: Buyer từ link Diamond khác với tài khoản Diamond.; Expected behavior: Context phải tách referral_entry_context và member_context.

Rule ID: P4-2B-R05; Rule: Khách yêu cầu không nhận tin hoặc không lưu thông tin phải được suppress ngay.; Expected behavior: Set do_not_contact/privacy_request_WAITING và handoff vận hành nếu cần.

Rule ID: P4-2B-R06; Rule: Health-sensitive chỉ kích hoạt khi khách tự nêu bệnh, thai kỳ, điều trị, dị ứng, kiêng kỵ hoặc ăn đặc biệt.; Expected behavior: Không hỏi y khoa lan man trong tư vấn gia đình thông thường.

Rule ID: P4-2B-R07; Rule: Không public hóa thông tin nội bộ.; Expected behavior: Context public chỉ chứa dữ liệu cần thiết để trả lời khách.

Rule ID: P4-2B-R08; Rule: Recall / sale lock thắng memory và sales recommendation.; Expected behavior: Dù khách từng mua SKU đó, AI không được gợi ý nếu runtime đang chặn.

Rule ID: P4-2B-R09; Rule: Message fatigue phải được tôn trọng.; Expected behavior: Không gửi tin chủ động quá dày; context trả fatigue_status cho channel/CRM.

Rule ID: P4-2B-R10; Rule: Không dùng UNKNOWN actor để mở rộng quyền đọc memory.; Expected behavior: Thiếu actor thì chặn hoặc giảm scope dữ liệu.

## 14. PUBLIC / PRIVATE DATA CLASSIFICATION

Class: PUBLIC_PRODUCT; Ví dụ: Tên sản phẩm public, vị, nhóm mùa/chức năng/bổ dưỡng, wording public-safe.; Được đưa vào customer-facing?: Có; Rule: Chỉ dùng public product name, không lộ internal SKU code.

Class: CUSTOMER_CONTEXT_SUMMARY; Ví dụ: Khách cũ, từng mua cho ba mẹ, thích vị thanh, đã hỏi mua combo.; Được đưa vào customer-facing?: Có điều kiện; Rule: Chỉ dùng khi phù hợp ngữ cảnh private và không gây lộ thông tin nhạy cảm.

Class: MEMBER_PUBLIC_STATUS; Ví dụ: Đang là Silver/Gold/Platinum/Diamond theo runtime.; Được đưa vào customer-facing?: Có điều kiện; Rule: Chỉ nói khi runtime xác nhận và kênh riêng tư phù hợp.

Class: COMMERCE_PUBLIC_STATE; Ví dụ: Quote còn hiệu lực, order draft chờ xác nhận, đơn đã có mã.; Được đưa vào customer-facing?: Có điều kiện; Rule: Phải từ Commerce Runtime; không tự nói.

Class: INTERNAL_OPERATIONAL; Ví dụ: Tồn kho chi tiết, warehouse location, QC notes, recall internal note.; Được đưa vào customer-facing?: Không; Rule: Không đưa vào prompt public-facing.

Class: PRIVATE_PII; Ví dụ: Số điện thoại, địa chỉ đầy đủ, CCCD/CMND, tài khoản ngân hàng.; Được đưa vào customer-facing?: Không mặc định; Rule: Chỉ hiển thị trong form xác nhận khi runtime/UI có quyền và khách đang ở private.

Class: SENSITIVE_HEALTH; Ví dụ: Bệnh nền, điều trị, dị ứng nghiêm trọng, thai kỳ.; Được đưa vào customer-facing?: Rất hạn chế; Rule: Chỉ hỏi/nhắc phần khách đã nêu, dùng public-safe wording, không chẩn đoán.

Class: FINANCIAL_INTERNAL; Ví dụ: Margin, commission amount, ROAS, payout, cost.; Được đưa vào customer-facing?: Không; Rule: AI Advisor không expose cho khách.

## 15. CONFLICT DETECTION MATRIX

Conflict: Identity conflict; Dấu hiệu: Cùng channel user map nhiều customer hoặc số điện thoại không khớp.; Cách xử lý bắt buộc: Không dùng memory nhạy cảm; yêu cầu xác minh nhẹ hoặc handoff.

Conflict: Member conflict; Dấu hiệu: Runtime tier khác CRM snapshot.; Cách xử lý bắt buộc: Ưu tiên runtime/Commerce truth; không hiển thị quyền lợi nếu chưa resolve.

Conflict: Consent conflict; Dấu hiệu: Một nguồn cho phép, nguồn khác do_not_contact.; Cách xử lý bắt buộc: Fail closed theo do_not_contact cho tin chủ động.

Conflict: Dietary conflict; Dấu hiệu: Memory nói ăn chay, khách hiện hỏi sản phẩm không chay.; Cách xử lý bắt buộc: Hỏi xác nhận ngắn hoặc gợi ý dòng phù hợp; không áp đặt.

Conflict: Order context conflict; Dấu hiệu: Khách nói đã chốt nhưng Commerce không có order_code.; Cách xử lý bắt buộc: Không nói đơn chính thức; chuyển Order Draft/Customer Confirmation flow.

Conflict: Payment conflict; Dấu hiệu: Khách nói đã chuyển khoản nhưng Payment Core chưa xác nhận.; Cách xử lý bắt buộc: Không nói paid; báo đã tiếp nhận thông tin và chờ xác nhận theo public-safe wording.

Conflict: Recall/sale lock conflict; Dấu hiệu: Memory từng mua SKU nhưng Operational suppression đang chặn.; Cách xử lý bắt buộc: Chặn gợi ý bán SKU đó; đề xuất thay thế chỉ khi sellable/runtime cho phép.

Conflict: Channel privacy conflict; Dấu hiệu: Public comment chứa thông tin riêng tư.; Cách xử lý bắt buộc: Không public lại PII; kéo private nếu policy cho phép.

## 16. IMPLEMENTATION TASKS

Dev / Codex / Copilot thực hiện theo thứ tự dưới đây. Không được nhảy thẳng vào code khi chưa hoàn thành inspection và mapping.

Task: Task 1 - Inspect current AI context flow; Mục tiêu: Tìm nơi hệ thống hiện tạo prompt/context cho AI Advisor.; Deliverable: Current flow map: controller/service/worker/DTO/model/prompt builder.

Task: Task 2 - Locate customer data sources; Mục tiêu: Xác định customer identity, member snapshot, order history, CRM preference, conversation memory.; Deliverable: Source map và owner map; ghi rõ source nào read-only.

Task: Task 3 - Detect anti-patterns; Mục tiêu: Tìm chỗ AI tự tính tier, discount, stock, sellable, order state, paid.; Deliverable: Anti-pattern register và kế hoạch loại bỏ/guard.

Task: Task 4 - Define Context Resolver interface; Mục tiêu: Chuẩn hóa input/output theo contract P4-2B.; Deliverable: DTO/interface mapping hoặc adapter design.

Task: Task 5 - Build privacy and data minimization filter; Mục tiêu: Tách public/private/internal/sensitive trước khi đưa vào prompt.; Deliverable: PublicSafeContextBuilder hoặc module tương đương.

Task: Task 6 - Build conflict and confidence logic; Mục tiêu: Gắn RESOLVED/PARTIAL/SUPPRESSED/HANDOFF/bị chặn.; Deliverable: Context status + missing_fields + handoff reason.

Task: Task 7 - Bind audit/correlation; Mục tiêu: Log đầy đủ cho sensitive/privacy/suppression/handoff.; Deliverable: Audit evidence, log format, correlation trace.

Task: Task 8 - Add tests; Mục tiêu: Test positive, negative, privacy leak, conflict, suppression.; Deliverable: Unit/integration tests và smoke evidence.

Task: Task 9 - Update handoff docs; Mục tiêu: Cập nhật markdown/handoff theo chuẩn report 14 mục.; Deliverable: Handoff update, risk register, downstream note cho P4-2C.

## 17. API / SERVICE BOUNDARY SUGGESTION

Các tên dưới đây là gợi ý thiết kế để dev map vào codebase thật. Không bắt buộc tạo endpoint mới nếu codebase đã có service tương đương. Không được mở public API cho dữ liệu khách hàng nếu security/policy chưa cho phép.

Internal service candidates: / - AdvisorCustomerContextResolver / - CustomerMemorySnapshotReader / - AdvisorConsentPrivacyResolver / - AdvisorMemberContextConsumer / - AdvisorContextConfidenceScorer / - AdvisorPublicSafeContextBuilder / - AdvisorContextAuditLogger / Internal route candidates if architecture needs HTTP boundary: / - POST /api/v1/advisor/context/resolve / - POST /api/v1/advisor/context/preview / Security note: / - These routes must be internal/private or protected by the same auth/RBAC policy as AI Advisor runtime. / - Do not expose raw customer memory publicly. / - Do not return final price, stock quantity, or member benefit calculations from this layer.

## 18. SMOKE REQUIRED

Smoke ID: P4-2B-SMK-001; Kịch bản: Khách mới chưa có customer id hỏi sản phẩm.; Expected result: Context status PARTIAL hoặc RESOLVED với anonymous context; không có memory giả.

Smoke ID: P4-2B-SMK-002; Kịch bản: Khách cũ có lịch sử mua nhưng không có quote hiện hành.; Expected result: AI dùng summary để tư vấn, không tự báo giá cuối.

Smoke ID: P4-2B-SMK-003; Kịch bản: Member Diamond đã được runtime xác nhận.; Expected result: Context hiển thị member_tier read-only; không tự tính giảm giá.

Smoke ID: P4-2B-SMK-004; Kịch bản: Buyer vào từ link Diamond nhưng không phải Diamond.; Expected result: Context tách referral_entry_context khỏi member_context.

Smoke ID: P4-2B-SMK-005; Kịch bản: Khách chọn "dùng thông tin cũ".; Expected result: Context trả old_info_available; không tự tạo order official.

Smoke ID: P4-2B-SMK-006; Kịch bản: Khách yêu cầu không nhận tin nữa.; Expected result: do_not_contact=true, proactive CRM suppressed, audit/handoff theo policy.

Smoke ID: P4-2B-SMK-007; Kịch bản: Khách nói dị ứng hải sản.; Expected result: dietary_guard_context có allergy flag; downstream không được gợi ý hải sản.

Smoke ID: P4-2B-SMK-008; Kịch bản: CRM snapshot và runtime member tier xung đột.; Expected result: Ưu tiên runtime hoặc BLOCK/PARTIAL; không hiển thị quyền lợi sai.

Smoke ID: P4-2B-SMK-009; Kịch bản: Recall/sale lock active cho SKU khách từng mua.; Expected result: Context bị chặn/SUPPRESSED cho sales recommendation.

Smoke ID: P4-2B-SMK-010; Kịch bản: Public comment chứa số điện thoại/địa chỉ.; Expected result: Không lặp lại PII public; route private/handoff nếu policy cho phép.

Smoke ID: P4-2B-SMK-011; Kịch bản: Payment selected nhưng Payment Core chưa paid.; Expected result: Context chỉ public_state selected/WAITING; không nói đã thanh toán.

Smoke ID: P4-2B-SMK-012; Kịch bản: Context builder kiểm tra leak internal fields.; Expected result: Không có stock_count, internal_sku_code, QC notes, margin, commission, ROAS trong prompt public.

## 19. EVIDENCE REQUIRED

Screenshot hoặc log mapping current context flow trước khi sửa.

Danh sách file/service/DTO đã inspect và kết quả owner map.

Diff các file đã sửa nếu có implementation.

Unit test / integration test output cho Context Resolver.

Smoke evidence cho P4-2B-SMK-001 đến P4-2B-SMK-012.

Log/audit evidence cho privacy suppression, identity conflict, recall/sale lock, handoff.

Proof không còn anti-pattern AI tự tính price/stock/sellable/member benefit trong phạm vi P4-2B.

Backend build result và frontend build result nếu có ảnh hưởng UI/SDK.

Migration/seed evidence nếu có tạo bảng/seed/config mới; nếu không có thì ghi "Không áp dụng".

## 20. REQUIRED REPORT FORMAT - 14 MỤC

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Kết quả backend build.

Kết quả frontend build.

Kết quả cleanup process.

Cập nhật Markdown.

Kết quả database migration/update nếu áp dụng.

Kết quả seed validation nếu áp dụng.

Rủi ro còn lại.

Cập nhật handoff.

## 21. DONE GATE

Context Resolver có source map rõ và không tạo ownership customer data song song.

Output context tách public/private/internal/sensitive rõ ràng.

Không có field forbidden trong public-safe prompt context.

Member tier và quyền lợi chỉ được consume read-only từ runtime/source hợp lệ.

Privacy, do-not-contact, message fatigue, quiet hour và privacy_request_WAITING được tôn trọng.

Dietary/allergy/health-sensitive flags được giữ đúng, không biến thành claim y tế.

Recall/sale lock/suppression thắng sales recommendation.

Context status RESOLVED/PARTIAL/SUPPRESSED/HANDOFF/bị chặn vận hành đúng.

Unit/integration test và smoke P4-2B-SMK-001 đến P4-2B-SMK-012 pass.

Report đủ 14 mục và evidence được lưu đúng nơi.

## 22. FAIL GATE

AI Advisor tự tính giá, discount, member benefit, stock, sellable hoặc trạng thái order/payment/revenue.

Context output lộ internal_sku_code, tồn kho chi tiết, QC note, công thức, supplier private, cost, margin, commission, ROAS hoặc payout.

Hệ thống dùng dữ liệu khách hàng thô vượt quá mục đích tư vấn hoặc đưa PII vào public response.

Không tôn trọng do_not_contact, privacy_request_WAITING hoặc message fatigue.

Member/Diamond bị suy luận từ link giới thiệu hoặc tổng mua không có runtime xác nhận.

Recall/sale lock bị bỏ qua vì lịch sử khách từng mua SKU đó.

Không có audit/correlation cho luồng sensitive/privacy/suppression/handoff.

Thiếu test negative hoặc smoke evidence.

## 23. DOWNSTREAM HANDOFF

Khi P4-2B đạt Done Gate cục bộ, downstream được phép chuyển sang P4-2C - Product Knowledge Consultation Orchestrator. P4-2C sẽ dùng context đã được resolve để chọn hướng tư vấn sản phẩm, nhưng vẫn phải gọi Sellable Gate/Runtime trước khi đề xuất SKU có thể bán.

Downstream file: 04-ĐIỀU PHỐI TƯ VẤN KIẾN THỨC SẢN PHẨM; P4-2B bàn giao: customer_need, recipient_profile, dietary_guard, taste_preference, member_context read-only, suppression_context, context_status.; Downstream không được làm: Không dùng memory để bypass sellable/recall/sale lock.

Downstream file: 05-TIÊU THỤ BÁO GIÁ NHÁP VÀ XÁC NHẬN ĐƠN; P4-2B bàn giao: identity confidence, old_info_available, communication channel, order context refs nếu có.; Downstream không được làm: Không dùng memory để tự tạo order_code hoặc tự xác nhận paid.

Downstream file: 06-CHẶN AN TOÀN NGÔN TỪ CÔNG KHAI VÀ TUYÊN BỐ; P4-2B bàn giao: health_sensitive_signal, privacy class, public/private flags.; Downstream không được làm: Không biến customer memory thành claim y tế hoặc expose PII.

Downstream file: 07-CẦU NỐI BÀN GIAO KÊNH MESSENGER TRỰC TIẾP VÀ CRM; P4-2B bàn giao: channel context, do_not_contact, fatigue, private/public mode, handoff flag.; Downstream không được làm: Không gửi CRM/proactive message khi suppressed.

## 24. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối sau để giao dev / Codex / Copilot. Không yêu cầu AI viết code rời rạc; yêu cầu agent inspect codebase thật trước khi sửa.

## PHASE-04-AI-ADVISOR-RUNTIME

## 04-ĐIỀU PHỐI TƯ VẤN KIẾN THỨC SẢN PHẨM



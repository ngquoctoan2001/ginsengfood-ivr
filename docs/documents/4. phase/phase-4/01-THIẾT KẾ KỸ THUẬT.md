# P4.1 - TECHNICAL DESIGN ONLY

## Khối nguồn bắt buộc đã nhập vào file này

File này có khối nguồn bắt buộc để chạy lẻ, nhưng khi bàn giao Phase 4 phải đọc `12-BẢNG GOM GIAI ĐOẠN 4.md`, `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` trước. Nếu nội dung file này khác Bảng Gôm/phụ lục/SRS handoff, áp dụng Bảng Gôm/phụ lục/SRS handoff và xử lý fail-closed.

### A. Nội dung nguồn đã nhập

1. Master/runtime governance: mọi thiết kế phải bám chuỗi nguồn sự thật. Product/Formula/Public Product Knowledge quyết định dữ liệu sản phẩm công khai. Operational Core quyết định sellable, recall, sale lock, batch/trace public-safe. Commerce Runtime quyết định DailySalesContext, QuoteSnapshot, OrderDraft, CustomerConfirmation, order_code, payment public-safe state, shipping/ETA và ORDER_VERIFIED. AI Advisor chỉ consume/orchestrate/render, không tạo nguồn sự thật song song.
2. Evidence/release governance: technical design accepted không phải implementation complete. Local design pass, smoke proposal hoặc completion report dạng chữ không được xem là Completion PASS, Release Ready, Production Ready hoặc Go-live. Mọi gate mặc định là PENDING_EVIDENCE cho đến khi có artifact được owner chấp nhận.
3. AI Advisor business/tech lock: AI được tư vấn, cá nhân hóa, chọn hướng hội thoại, tạo response candidate, gọi runtime hợp lệ, render câu trả lời có lực bán và handoff đúng kênh. AI không được tự tính giá, tồn, discount, quyền lợi thành viên, Diamond benefit, ship, VAT, order_code, paid, verified revenue, commission, ROAS hoặc payout.
4. Commerce lock: final price chỉ đến từ QuoteSnapshot còn hiệu lực. Order Draft không phải Official Order. Chỉ có CustomerConfirmation hợp lệ và order_code từ Commerce Runtime mới cho phép nói đơn đã ghi nhận chính thức. Payment selected, COD selected hoặc ảnh chuyển khoản không phải Paid.
5. Product/Operational lock: AI chỉ dùng product public-safe data, ProductRoleMatrix và sellable/runtime signals. Không dùng mã SKU nội bộ trong lời khách, không lộ công thức/tỷ lệ/cost/supplier/QC note, không bán SKU not sellable, recall, sale lock, channel suppressed hoặc thiếu proof dietary.
6. CRM/Member lock: customer memory chỉ hỗ trợ hiểu khách, không tự quyết quyền lợi giá. CRM/member messaging phải tôn trọng opt-out, quiet hour, open case, complaint, fatigue, frequency cap và canonical message registry.
7. Finance/Diamond lock: Diamond/referral/commission/payout là Finance/Diamond state, không thuộc AI Advisor. AI chỉ nói theo trạng thái runtime được phép, không nói đã ghi nhận hoa hồng, đã trừ PIT hoặc đã chi trả nếu chưa có finance checkpoint.
8. Gateway/Ads/Live lock: Facebook Gateway chỉ là lớp nhận/normalize/route/deliver/log evidence, không tư vấn thay AI và không mở production nếu AI/evidence/smoke/owner sign-off chưa pass. Ads/ROAS chỉ tính revenue từ ORDER_VERIFIED. Live/MC AI chỉ dùng daily live board/script brief và phải kéo price/buy/deep consult sang private khi policy yêu cầu.

### B. Conflict lock

- Conflict giá 24/7, Giờ Vàng, member benefit hoặc Diamond buyer benefit: thiết kế phải route sang QuoteSnapshot/runtime policy, không hardcode.
- Conflict lịch Giờ Vàng: thiết kế cần GoldenHourResolver/policy version, không để prompt/template nói giờ cố định khi thiếu runtime.
- Conflict vegan/chay/công thức: thiết kế phải có dietary proof guard và fail-closed nếu proof thiếu.
- Conflict file tin nhắn/wording cũ: thiết kế phải có customer-facing language guard, message registry và banned-term rewrite.
- Conflict Page/Gateway/App/Token: Phase 4 chỉ thiết kế AI-side handoff intent, không thiết kế Gateway production enablement.
- Conflict evidence/completion: thiết kế phải phân biệt design accepted, evidence submitted, owner accepted, release approved.

### C. Trạng thái evidence bắt buộc

Trạng thái mặc định của file này: TECHNICAL_DESIGN_ONLY_WITH_IMPORTED_SOURCE_LOCK. Thiết kế chỉ hoàn tất khi có boundary, contract, DTO khái niệm, guard, flow, evidence object và smoke requirement. Không được gọi RUNTIME_IMPLEMENTATION_READY, GATEWAY_PASS, COMPLETION_PASS, RELEASE_READY, PRODUCTION_READY hoặc GO_LIVE_APPROVED.

### D. Khóa riêng của file này

File 01 chỉ khóa thiết kế kỹ thuật. Nhiệm vụ là biến các nguồn đã nhập thành contract/API/DTO/event/state/evidence design cho AI Advisor, không triển khai code, không sửa migration/seed, không mở Gateway.

## Nội dung triển khai

## PROMPT-P4-1 - AI ADVISOR RUNTIME TECHNICAL DESIGN HANDOFF

## 1. PHASE MARKER

Phase này thuộc PHASE-04 - AI ADVISOR RUNTIME. Mục tiêu là khóa thiết kế kỹ thuật để AI Advisor tiêu thụ đúng dữ liệu runtime từ các phase trước, không tự tính và không tự quyết nghiệp vụ.

Phase code: P4

Tên phase: AI Advisor Runtime

File hiện tại: 01-THIẾT KẾ KỸ THUẬT.docx

Mode: Technical Design Only

Điểm bắt đầu: Sau khi hoàn tất 00-PHÂN TÍCH HIỆN TRẠNG.md ở mức báo cáo phân tích, chưa phải implementation complete.

Điểm kết thúc: Có thiết kế boundary / contract / flow / guard đủ rõ để chuyển sang P4-2A Limited Implementation.

Gateway: Luôn bị chặn cho đến khi có evidence accepted, owner review và release decision.

## 2. HEADER

Tên tài liệu: PROMPT-P4-1 - AI ADVISOR RUNTIME / CUSTOMER MEMORY / PRODUCT CONSULTATION / QUOTE ORDER CONSUMER / SAFETY WORDING TECHNICAL DESIGN HANDOFF.

Bản chất tài liệu: thiết kế kỹ thuật để định hướng dev / Codex / Copilot phân tích và triển khai sau này theo từng cụm nhỏ. Tài liệu này không phải hướng dẫn copy code, không phải cam kết hệ thống đã chạy và không phải release approval.

## 3. MODE

TECHNICAL DESIGN ONLY / Allowed: xác định kiến trúc, contract, boundary, DTO khái niệm, service responsibility, state flow, fallback, audit, evidence và smoke required. / Forbidden: sửa file, viết code triển khai, chạy migration, đổi seed, mở feature flag, tạo Gateway PASS hoặc tuyên bố Production Readiness.

## 4. SOURCE-OF-TRUTH

Thiết kế Phase 4 chỉ được bám vào các nguồn sự thật đã khóa. Nếu codebase hiện tại mâu thuẫn với các nguồn này, codebase phải được ghi nhận là gap hoặc conflict; không tự hợp thức hóa hành vi sai của code.

Nguồn: PHASE 0 - Foundation / RBAC / Audit / Evidence / Idempotency; Vai trò trong Phase 4: Cung cấp actor context, permission, audit, evidence, idempotency và high-risk command guard.; Nguyên tắc tiêu thụ: AI Advisor phải truyền correlation, không bypass permission, không ghi nhận side effect nếu thiếu idempotency/audit khi cần.

Nguồn: PHASE 1 - Product / SKU / Recipe / Product Activation; Vai trò trong Phase 4: Cung cấp product/SKU public data, product knowledge public view và nền công thức đã được quản trị.; Nguyên tắc tiêu thụ: AI chỉ consume public-safe product data; không lộ internal SKU code, formula internal, costing hoặc dữ liệu không dành cho khách.

Nguồn: PHASE 2 - Operational Core; Vai trò trong Phase 4: Cung cấp operational truth như readiness, batch release, warehouse inventory, traceability, recall/sale lock.; Nguyên tắc tiêu thụ: AI không tự kết luận còn hàng, batch released, recall hay sale lock; chỉ consume trạng thái public-safe qua runtime.

Nguồn: PHASE 3 - Commerce Runtime; Vai trò trong Phase 4: Cung cấp sellable gate, QuoteSnapshot, order draft, official order, payment public-safe state, shipping public-safe state, verified revenue boundary.; Nguyên tắc tiêu thụ: AI không tự tính giá, discount, member benefit, final payable, paid, verified revenue, commission, ROAS hoặc payout.

Nguồn: MASTER Governance Pack; Vai trò trong Phase 4: Khóa tư duy nguồn sự thật, evidence, Gateway, release, reserved/future integration.; Nguyên tắc tiêu thụ: Không tuyên bố PASS nếu chưa có evidence accepted và owner sign-off.

Nguồn: AI Advisor / Channel locked rules; Vai trò trong Phase 4: Khóa customer-facing tone, claim guard, Messenger/Live/CRM handoff và public/private boundary.; Nguyên tắc tiêu thụ: AI trả lời có lực bán nhưng không claim y tế quá mức, không nói "hệ thống" với khách và không làm lộ dữ liệu nội bộ.

## 5. ENTRY CONDITION

Chỉ được chuyển sang thiết kế kỹ thuật khi đã có đầu vào phân tích tối thiểu từ 00-PHÂN TÍCH HIỆN TRẠNG.md. Nếu chưa có phân tích codebase thật, tài liệu này vẫn được dùng để khóa chuẩn thiết kế mục tiêu, nhưng không được ghi là đã xác nhận hiện trạng code.

Đã xác định repository / module / service liên quan đến AI Advisor, Channel, Commerce Runtime và Product Knowledge.

Đã rà có hay không các điểm AI tự tính giá, tồn, sellable, member benefit, order, paid, revenue.

Đã rà có hay không customer memory, product knowledge resolver, consultation orchestrator, safety guard, channel handoff.

Đã có danh sách gap / conflict / điểm chặn từ Analysis Only hoặc phải ghi rõ "WAITING codebase inspection".

Không có yêu cầu sửa code trực tiếp trong bước này.

## 6. OBJECTIVE

Mục tiêu của P4-1 là khóa bản thiết kế kỹ thuật để Phase 4 triển khai đúng vai trò: AI Advisor là consumer của runtime truth, là lớp tư vấn và điều phối hội thoại, không phải owner của sự thật nghiệp vụ.

Boundary: Xác định rõ AI được đọc gì, được gọi gì, được nói gì, và tuyệt đối không được tự quyết gì.

Contract: Khóa Runtime Consumer Contract gồm customer context, product public view, sellable, QuoteSnapshot, order draft, payment/shipping public-safe state và suppression.

Component design: Khóa các cụm component: Runtime Consumer, Customer Memory Resolver, Product Knowledge Resolver, Consultation Orchestrator, Quote/Order Consumer, Safety/Claim Guard, Channel Bridge.

Data flow: Khóa flow tư vấn, flow báo giá, flow đơn nháp, flow xác nhận, flow sau mua và flow channel handoff.

Safety: Tách public/private data, chặn claim y tế quá mức, chặn leak dữ liệu nội bộ, chặn câu trả lời sai trạng thái bán.

Evidence & smoke: Xác định test/smoke bắt buộc trước khi cho phép Limited Implementation từng cụm nhỏ.

## 7. SCOPE IN

Thiết kế boundary AI Advisor với PHASE 1 / PHASE 2 / PHASE 3 runtime.

Thiết kế contract dữ liệu đầu vào/đầu ra cho AI Advisor ở mức kỹ thuật, nhưng chưa viết DTO code.

Thiết kế luồng Customer Memory / Customer Context Resolver.

Thiết kế Product Knowledge Public View Resolver và Consultation Orchestrator.

Thiết kế cách AI gọi Commerce Runtime để lấy QuoteSnapshot, Order Draft và Customer Confirmation.

Thiết kế Safety / Public Wording / Claim Guard.

Thiết kế Channel Bridge cho Messenger / Live / CRM ở mức consumer/handoff.

Thiết kế audit, evidence, idempotency, observability cho các side effect của AI.

Thiết kế smoke required, fail-safe behavior và report 14 mục.

## 8. SCOPE OUT

Không thiết kế lại Product Master, SKU Master, Recipe Master hoặc Activation Gate của PHASE 1.

Không thiết kế lại Raw Lot, Batch, Warehouse, Traceability, Recall hoặc Sale Lock owner của PHASE 2.

Không thiết kế lại Sellable Gate, Price Engine, Program Engine, Member Benefit, Order Core, Payment Core hoặc Verified Revenue của PHASE 3.

Không tự tạo bảng database mới ở bước này; mọi schema nếu cần phải đưa sang Limited Implementation sau khi có design approval.

Không viết code, không migration, không seed, không sửa frontend/backend.

Không triển khai IVR; IVR vẫn là reserved future integration.

Không mở Gateway và không tuyên bố production readiness.

## 9. DESIGN PRINCIPLES

Mã: P4-DP-01; Nguyên tắc thiết kế: AI Advisor là consumer.; Hành vi bắt buộc: Mọi truth về bán hàng, giá, tồn, đơn, thanh toán và doanh thu phải lấy từ runtime owner.

Mã: P4-DP-02; Nguyên tắc thiết kế: Không copy-paste code từ AI.; Hành vi bắt buộc: Bước này chỉ khóa design; implementation sau phải inspect codebase thật và làm theo architecture thật.

Mã: P4-DP-03; Nguyên tắc thiết kế: No self-compute.; Hành vi bắt buộc: Không tự tính sellable, stock, price, discount, member benefit, final price, shipping, tax, paid, revenue, commission, ROAS, payout.

Mã: P4-DP-04; Nguyên tắc thiết kế: Public-safe by default.; Hành vi bắt buộc: Dữ liệu trả khách phải qua public DTO/whitelist; dữ liệu nội bộ không được lộ ra câu trả lời.

Mã: P4-DP-05; Nguyên tắc thiết kế: QuoteSnapshot là nguồn giá cuối.; Hành vi bắt buộc: Không có QuoteSnapshot thì không báo final price. QuoteSnapshot hết hạn thì phải xin snapshot mới từ Commerce Runtime.

Mã: P4-DP-06; Nguyên tắc thiết kế: Recall / Sale Lock thắng mọi kênh.; Hành vi bắt buộc: Nếu runtime trả recall/sale lock/channel suppression, AI phải dừng bán và chuyển sang lời nhắn an toàn.

Mã: P4-DP-07; Nguyên tắc thiết kế: Customer Memory giúp tư vấn, không quyết giá.; Hành vi bắt buộc: AI dùng memory để gợi ý phù hợp, nhưng quyền lợi giá/hạng/benefit phải do runtime xác nhận.

Mã: P4-DP-08; Nguyên tắc thiết kế: Evidence trước release.; Hành vi bắt buộc: Smoke pass cục bộ không phải Global PASS; evidence phải accepted mới dùng cho Gateway.

## 10. TARGET ARCHITECTURE OVERVIEW

Kiến trúc mục tiêu của Phase 4 đặt AI Advisor ở lớp orchestration hội thoại. AI nhận ngữ cảnh, gọi resolver/runtime owner, áp guard public-safe, tạo phản hồi hoặc yêu cầu handoff; AI không trực tiếp viết vào các truth table nghiệp vụ ngoài các event/command được owner cho phép.

Lớp: AI Runtime Boundary; Thành phần mục tiêu: AI Advisor API / Orchestrator entrypoint; Trách nhiệm: Nhận message/event, chuẩn hóa context, gọi resolver và quyết định response path.; Không được làm: Không bypass runtime owner, không direct DB write vào Commerce/Operational.

Lớp: Runtime Consumer Layer; Thành phần mục tiêu: Product, Sellable, Quote, Order, Payment, Shipping, Recall clients; Trách nhiệm: Gọi runtime contract đã được owner cung cấp.; Không được làm: Không tự tính lại kết quả runtime, không cache sai làm lệch giá/tồn.

Lớp: Customer Context Layer; Thành phần mục tiêu: Customer Memory / Member Context Resolver; Trách nhiệm: Đọc lịch sử mua, nhu cầu, người nhận, tier public-safe, opt-out/complaint/frequency guard.; Không được làm: Không tự quyết tier benefit hoặc quyền lợi giá.

Lớp: Knowledge Layer; Thành phần mục tiêu: Product Knowledge Public View Resolver; Trách nhiệm: Đọc nội dung public-safe của 20 SKU và mở rộng SKU sau này.; Không được làm: Không hardcode danh sách SKU trong business logic, không lộ mã nội bộ.

Lớp: Consultation Layer; Thành phần mục tiêu: Consultation Orchestrator; Trách nhiệm: Ghép nhu cầu khách + knowledge + sellable + guard để tư vấn và đề xuất SKU/combo.; Không được làm: Không đề xuất SKU not sellable, out-of-stock, recall, sale locked hoặc không phù hợp guard.

Lớp: Commercial Consumer Layer; Thành phần mục tiêu: Quote / Order Draft / Confirmation Consumer; Trách nhiệm: Gọi Commerce Runtime để tạo quote/draft/confirmation và hiển thị kết quả public-safe.; Không được làm: Không tự tạo order_code, không tự set paid, không tự verified revenue.

Lớp: Safety Layer; Thành phần mục tiêu: Claim Guard / Public Wording Guard / Leakage Guard; Trách nhiệm: Chặn claim y tế quá mức, chặn dữ liệu internal, chuyển lời nói thành public-safe.; Không được làm: Không tự thêm disclaimer làm mất lực bán khi không cần, không dùng ngôn ngữ kỹ thuật với khách.

Lớp: Channel Layer; Thành phần mục tiêu: Messenger / Live / CRM Bridge; Trách nhiệm: Điều phối public comment, private Messenger, live, CRM và human handoff.; Không được làm: Không hardcode giá live, không gửi CRM sai trạng thái, không xử lý IVR khi chưa tới phase.

## 11. COMPONENT RESPONSIBILITY MATRIX

Component: AI Message Intake; Input chính: Channel event, message text, customer/channel identifiers, correlation id.; Output chính: Normalized AI request context.; Owner truth phải tôn trọng: PHASE 0 actor/correlation, Channel policy.

Component: Customer Memory Context Resolver; Input chính: Customer id/phone/channel id/order history refs.; Output chính: Customer context public-safe, memory summary, opt-out/complaint flags.; Owner truth phải tôn trọng: Customer/CRM/Commerce owner; AI không tự tạo truth member benefit.

Component: Product Knowledge Resolver; Input chính: Need, SKU/product refs, public view policy.; Output chính: Product public view, taste, dietary flags, public-safe benefit language.; Owner truth phải tôn trọng: PHASE 1 Product/SKU/Knowledge owner.

Component: Sellable Runtime Consumer; Input chính: Product/SKU/channel/customer context.; Output chính: Sellable status, public-safe block reason, alternatives policy.; Owner truth phải tôn trọng: PHASE 3 Sellable Gate + PHASE 2 Recall/Sale Lock.

Component: Consultation Orchestrator; Input chính: Customer context + product knowledge + sellable + guard.; Output chính: Recommended answer path, SKU/combo proposal, handoff requirement.; Owner truth phải tôn trọng: Không tự ghi truth; chỉ orchestrate.

Component: QuoteSnapshot Consumer; Input chính: Selected SKU/quantity/customer/channel/program context.; Output chính: QuoteSnapshot id, final price, expiry, breakdown public-safe.; Owner truth phải tôn trọng: PHASE 3 Quote/Price/Program/Member Runtime.

Component: Order Draft Consumer; Input chính: QuoteSnapshot + customer info + delivery/payment choice.; Output chính: Order draft payload, required fields, confirmation state.; Owner truth phải tôn trọng: PHASE 3 Order Draft owner.

Component: Customer Confirmation Consumer; Input chính: Valid draft + customer action/button/text fallback.; Output chính: Confirmation event result; official order state if created.; Owner truth phải tôn trọng: PHASE 3 Official Order owner.

Component: Safety/Public Wording Guard; Input chính: Candidate answer + data classification.; Output chính: Approved public/private response or block/handoff.; Owner truth phải tôn trọng: Claim whitelist, privacy policy, public DTO policy.

Component: Channel Handoff Bridge; Input chính: Intent, risk, price/order context, channel capability.; Output chính: Messenger/private handoff, CRM suppression, human handoff.; Owner truth phải tôn trọng: Channel policy + PHASE 3 suppression + PHASE 2 recall/sale lock.

Component: AI Audit/Evidence Logger; Input chính: Decision path, guard result, quote/order refs, errors.; Output chính: Audit/evidence events for QA and release review.; Owner truth phải tôn trọng: PHASE 0 Audit/Evidence, not business truth owner.

## 12. RUNTIME CONSUMER CONTRACT

Contract dưới đây là thiết kế mục tiêu ở mức dữ liệu và trách nhiệm. Đây chưa phải schema code cuối cùng. Khi sang Limited Implementation, dev phải đối chiếu với API/DTO thật của codebase và không được tự thay đổi owner truth.

## 12.1. Contract envelope

Field khái niệm: request_id; Mục đích: Định danh request AI ở lớp runtime.; Bắt buộc: Có

Field khái niệm: correlation_id; Mục đích: Xuyên suốt từ channel -> AI -> runtime -> audit/evidence.; Bắt buộc: Có

Field khái niệm: actor_context; Mục đích: Xác định bot/system/user/admin/human agent theo PHASE 0.; Bắt buộc: Có

Field khái niệm: channel_context; Mục đích: Nền tảng: Messenger, Live, Web, Landing Page, CRM, Admin preview.; Bắt buộc: Có

Field khái niệm: privacy_class; Mục đích: Phân loại public/private/internal để guard không lộ dữ liệu.; Bắt buộc: Có

Field khái niệm: customer_reference; Mục đích: Customer id hoặc channel id đã map, nếu có.; Bắt buộc: Có nếu đã nhận diện

Field khái niệm: runtime_resolution_status; Mục đích: Kết quả resolver: resolved, partial, unavailable, conflict.; Bắt buộc: Có

Field khái niệm: policy_version_refs; Mục đích: Version policy/claim/knowledge dùng để trả lời.; Bắt buộc: Có để audit/replay

## 12.2. Customer context contract

Field khái niệm: customer_type; Provider: Customer/Commerce Runtime; Cách AI được dùng: Nhận biết NEW / EXISTING / MEMBER nếu runtime xác nhận.; Cấm dùng để: Không tự suy ra quyền lợi giá nếu thiếu runtime.

Field khái niệm: member_tier; Provider: Member Runtime; Cách AI được dùng: Cá nhân hóa cách xưng hô, ưu tiên gợi ý phù hợp.; Cấm dùng để: Không tự tính discount/tier benefit.

Field khái niệm: purchase_history_summary; Provider: Commerce/CRM Runtime; Cách AI được dùng: Gợi ý mua lại, đổi vị, combo, chăm sóc sau mua.; Cấm dùng để: Không suy ra verified revenue hoặc commission.

Field khái niệm: recipient_profile; Provider: Customer Memory; Cách AI được dùng: Tư vấn theo người dùng chính/người nhận quà nếu đã có dữ liệu.; Cấm dùng để: Không hỏi vòng nếu runtime đã có thông tin.

Field khái niệm: dietary_need; Provider: Customer Memory / user input; Cách AI được dùng: Gợi ý sản phẩm phù hợp chay/mặn/nhẹ bụng/bổ dưỡng.; Cấm dùng để: Không bỏ qua dị ứng/kiêng kỵ đã biết.

Field khái niệm: complaint_open; Provider: CSKH/Quality Runtime; Cách AI được dùng: Dừng upsell, chuyển hỗ trợ/human nếu có vấn đề mở.; Cấm dùng để: Không gửi CRM bán thêm.

Field khái niệm: opt_out_status; Provider: CRM Runtime; Cách AI được dùng: Chặn tin nhắn CRM nếu khách opt-out.; Cấm dùng để: Không gửi trigger ngoài whitelist.

## 12.3. Product public data contract

Field khái niệm: product_public_id; Ý nghĩa: Định danh public hoặc slug dùng ở lớp trả lời.; Public-safe rule: Không hiển thị mã SKU nội bộ với khách.

Field khái niệm: public_product_name; Ý nghĩa: Tên sản phẩm công khai.; Public-safe rule: Luôn dùng trong customer-facing response.

Field khái niệm: product_group; Ý nghĩa: Seasonal / functional / nourishing.; Public-safe rule: Dùng để tư vấn nhóm sản phẩm, không lộ logic nội bộ.

Field khái niệm: taste_profile; Ý nghĩa: Vị, cảm giác ăn, độ dễ dùng.; Public-safe rule: Tăng lực bán, tránh so sánh tiêu cực hoặc claim quá mức.

Field khái niệm: dietary_flags; Ý nghĩa: Vegan/non-vegan, allergen, suitability flags.; Public-safe rule: Phải tôn trọng khi khách có nhu cầu/kiêng kỵ.

Field khái niệm: public_safe_eastern_concept; Ý nghĩa: Khái niệm y thực đồng nguyên đã chuyển hóa public-safe.; Public-safe rule: Không nói chữa bệnh, không biến sản phẩm thành thuốc.

Field khái niệm: claim_tags_allowed; Ý nghĩa: Danh sách claim public được phép.; Public-safe rule: Safety Guard phải chặn ngoài whitelist.

Field khái niệm: internal_fields_redacted; Ý nghĩa: Các field không được trả khách.; Public-safe rule: Không lộ cost, formula nội bộ, QC defect, supplier private, trace internal.

## 12.4. Sellable and suppression contract

Field khái niệm: sellable_status; Enum / giá trị mục tiêu: SELLABLE / NOT_SELLABLE / OUT_OF_STOCK / RECALL_LOCKED / SALE_LOCKED / CHANNEL_SUPPRESSED / POLICY_BLOCKED / UNKNOWN; AI behavior: Chỉ khi SELLABLE mới được đi tiếp báo giá/chốt. UNKNOWN phải fail-safe, không nói còn hàng.

Field khái niệm: public_block_reason; Enum / giá trị mục tiêu: Thông điệp public-safe do runtime cung cấp hoặc map whitelist.; AI behavior: Nói rõ nhưng không lộ nội bộ; đề xuất thay thế nếu policy cho phép.

Field khái niệm: alternative_policy; Enum / giá trị mục tiêu: ALLOW_ALTERNATIVES / NO_ALTERNATIVE / HUMAN_REVIEW; AI behavior: Nếu được phép, AI gợi ý tối đa nhóm thay thế phù hợp, không bịa SKU.

Field khái niệm: recall_or_sale_lock_ref; Enum / giá trị mục tiêu: Internal reference masked/public-safe.; AI behavior: AI chỉ dùng để chặn bán/handoff, không lộ mã recall nội bộ.

Field khái niệm: channel_suppression; Enum / giá trị mục tiêu: Có/không theo kênh.; AI behavior: Live/CRM/Messenger phải tôn trọng suppression.

## 12.5. QuoteSnapshot contract

Field khái niệm: quote_snapshot_id; Owner: PHASE 3 Commerce Runtime; Rule khóa: Không có quote_snapshot_id thì không có final price.

Field khái niệm: listed_price_line; Owner: PHASE 3 Price Runtime; Rule khóa: AI hiển thị theo snapshot, không tính lại.

Field khái niệm: program_discount_line; Owner: PHASE 3 Program Runtime; Rule khóa: Golden Hour / 24/7 phải từ runtime, không hardcode live.

Field khái niệm: member_benefit_line; Owner: PHASE 3 Member Benefit Runtime; Rule khóa: AI không tự áp Silver/Gold/Platinum/Diamond.

Field khái niệm: shipping_fee_line; Owner: PHASE 3 Shipping Runtime; Rule khóa: Nếu chưa resolve thì nói chưa xác nhận phí, không tự tính.

Field khái niệm: tax_invoice_line; Owner: PHASE 3 Tax/Invoice Runtime; Rule khóa: AI chỉ hiển thị public-safe state nếu được trả.

Field khái niệm: final_payable; Owner: PHASE 3 QuoteSnapshot; Rule khóa: Nguồn sự thật giá cuối; immutable theo snapshot.

Field khái niệm: quote_expiry_at; Owner: PHASE 3 QuoteSnapshot; Rule khóa: Hết hạn thì phải xin snapshot mới.

Field khái niệm: quote_status; Owner: ACTIVE / EXPIRED / VOID / SUPERSEDED; Rule khóa: Chỉ ACTIVE mới dùng để chốt tiếp.

## 12.6. Order draft / confirmation / official order contract

Giai đoạn: Order Draft; AI được làm: Hiển thị tóm tắt đơn nháp, thông tin cần xác nhận, CTA "Xác nhận đơn" nếu channel hỗ trợ.; AI không được làm: Không nói đơn chính thức, không tự tạo order_code.

Giai đoạn: Customer Confirmation; AI được làm: Gửi confirmation event nếu có hành động hợp lệ từ khách và channel cho phép.; AI không được làm: Không tự xác nhận thay khách, không coi im lặng là xác nhận.

Giai đoạn: Official Order; AI được làm: Chỉ hiển thị order_code/state khi Commerce Runtime trả official order.; AI không được làm: Không nói đã ghi nhận chính thức nếu chưa có order_code.

Giai đoạn: Payment; AI được làm: Hiển thị trạng thái public-safe: chọn phương thức, chờ chuyển khoản, COD, đã xác nhận paid nếu Payment Core xác nhận.; AI không được làm: Không nói paid khi khách chỉ chọn COD hoặc chọn chuyển khoản.

Giai đoạn: Shipping; AI được làm: Hiển thị public-safe shipping state nếu runtime cung cấp.; AI không được làm: Không tự hứa giao hàng ngoài policy/runtime.

Giai đoạn: Verified Revenue; AI được làm: Chỉ dùng cho downstream/report/CRM boundary khi runtime có ORDER_VERIFIED.; AI không được làm: Không dùng để tư vấn bán hàng hoặc tự tính ROAS/commission/payout.

## 13. KHONG SELF-COMPUTE ENFORCEMENT MATRIX

Nghiệp vụ: Sellable; Owner đúng: PHASE 3 Sellable Gate; AI Advisor bị cấm: Tự kết luận bán được từ active/inventory.; Fallback đúng: Gọi runtime; nếu unavailable thì không chốt bán.

Nghiệp vụ: Stock / availability; Owner đúng: PHASE 2/3 inventory/sellable projection; AI Advisor bị cấm: Tự nói còn hàng/hết hàng từ cache hoặc text.; Fallback đúng: Dùng sellable_status và public_block_reason.

Nghiệp vụ: Price / final payable; Owner đúng: PHASE 3 QuoteSnapshot; AI Advisor bị cấm: Tự cộng trừ giá, discount, member benefit.; Fallback đúng: Yêu cầu QuoteSnapshot mới hoặc báo chưa có giá xác nhận.

Nghiệp vụ: Program discount; Owner đúng: PHASE 3 Program Runtime; AI Advisor bị cấm: Hardcode Giờ Vàng/24/7/live price.; Fallback đúng: Consume runtime snapshot theo session/time.

Nghiệp vụ: Member benefit; Owner đúng: PHASE 3 Member Runtime; AI Advisor bị cấm: Tự áp tier hoặc tự hỏi khách để tính.; Fallback đúng: Dùng member benefit từ runtime.

Nghiệp vụ: Order official; Owner đúng: PHASE 3 Order Core; AI Advisor bị cấm: Tự tạo order_code hoặc nói đơn đã ghi nhận.; Fallback đúng: Chỉ nói official khi runtime trả order_code.

Nghiệp vụ: Paid; Owner đúng: PHASE 3 Payment Core; AI Advisor bị cấm: Nói paid khi khách chọn phương thức.; Fallback đúng: Chỉ nói paid khi Payment Core confirmed.

Nghiệp vụ: Verified revenue; Owner đúng: PHASE 3 Revenue Verification; AI Advisor bị cấm: Tự tính doanh thu, ROAS, commission.; Fallback đúng: Chờ ORDER_VERIFIED.

Nghiệp vụ: Recall/Sale Lock; Owner đúng: PHASE 2 + PHASE 3 suppression; AI Advisor bị cấm: Bỏ qua lock để bán tiếp.; Fallback đúng: Dừng bán/handoff an toàn.

## 14. DATA FLOW DESIGN

## 14.1. Flow tư vấn sản phẩm

## 1. Channel gửi message/event vào AI Message Intake cùng channel_context và correlation_id.

## 2. AI chuẩn hóa intent, nhu cầu, product mention, risk mention và context nhận diện khách.

## 3. Customer Memory Resolver trả customer_context public-safe, member tier nếu runtime xác nhận, complaint/opt-out/dietary flags nếu có.

## 4. Product Knowledge Resolver trả product_public_data theo public view policy.

## 5. Sellable Runtime Consumer kiểm tra sellable/suppression theo SKU/kênh/khách.

## 6. Consultation Orchestrator chọn câu trả lời phù hợp: tư vấn, đề xuất thay thế, hỏi mở rộng tối thiểu, handoff hoặc dừng bán.

## 7. Safety/Public Wording Guard kiểm tra claim, data leakage, public/private scope và tone trước khi trả lời.

## 8. AI Audit/Evidence Logger ghi quyết định ở mức cần thiết cho QA/replay.

## 14.2. Flow báo giá và chốt đơn

## 1. AI chỉ bắt đầu quote flow khi SKU/quantity/channel/customer context đủ và sellable_status = SELLABLE.

## 2. AI gọi Commerce Runtime để tạo QuoteSnapshot.

## 3. AI hiển thị giá cuối, breakdown public-safe và thời hạn giữ giá đúng snapshot.

## 4. Nếu khách đồng ý, AI yêu cầu/hiển thị Order Draft payload từ Commerce Runtime.

## 5. Khách kiểm tra thông tin và bấm CTA "Xác nhận đơn" hoặc xác nhận hợp lệ theo kênh text-only.

## 6. AI gửi Customer Confirmation event về Commerce Runtime.

## 7. AI chỉ nói đơn chính thức khi Commerce Runtime trả order_code.

## 8. AI không nói paid cho đến khi Payment Core public-safe state xác nhận.

## 14.3. Flow Live / Messenger / CRM

Tình huống: Comment public mơ hồ/ký hiệu; Thiết kế xử lý: Nếu policy cho phép, kéo Messenger/private để tư vấn và tránh lộ giá/đơn.; Guard bắt buộc: Không trả public thông tin giá/order nhạy cảm.

Tình huống: Khách hỏi giá trong Live; Thiết kế xử lý: Không hardcode giá; tạo/đọc QuoteSnapshot hoặc quote runtime theo session.; Guard bắt buộc: Giờ Vàng/Live phải từ runtime, có expiry.

Tình huống: Sau handoff thành công; Thiết kế xử lý: Các câu tiếp theo mặc định xử lý trong private context.; Guard bắt buộc: Không quay lại public nếu nội dung có giá/order/PII.

Tình huống: CRM sau mua; Thiết kế xử lý: Dùng trigger whitelist, kiểm tra opt-out, complaint, delivered/paid policy, frequency cap.; Guard bắt buộc: Không gợi ý SKU not sellable hoặc đang sale lock.

Tình huống: Comment chửi thề/công kích; Thiết kế xử lý: Không reply public, không kéo Messenger nếu policy đã khóa hide/no reply.; Guard bắt buộc: Flag theo moderation policy; khiếu nại thật có evidence thì route CSKH/Quality.

Tình huống: Complaint/order issue; Thiết kế xử lý: Dừng bán, hỏi mã đơn/evidence nếu cần, route hỗ trợ.; Guard bắt buộc: Không upsell khi có complaint mở.

## 15. STATE COORDINATION DESIGN

AI Advisor không sở hữu state nghiệp vụ, nhưng phải hiểu state để nói đúng. Mọi state được hiển thị cho khách phải là public-safe view từ owner runtime.

State group: Consultation; State AI được hiểu: NEED_DETECTED / PRODUCT_MATCHED / MORE_INFO_REQUIRED / HANDOFF_REQUIRED; Câu trả lời hợp lệ: Tư vấn ngắn gọn, hỏi mở rộng tối thiểu nếu thiếu dữ liệu thật sự cần.; Câu trả lời bị cấm: Hỏi vòng khi runtime đã có context.

State group: Sellable; State AI được hiểu: SELLABLE / OUT_OF_STOCK / RECALL_LOCKED / SALE_LOCKED / UNKNOWN; Câu trả lời hợp lệ: Bán tiếp chỉ khi SELLABLE; nếu không, giải thích public-safe và đề xuất thay thế nếu cho phép.; Câu trả lời bị cấm: Nói còn hàng khi UNKNOWN hoặc bị lock.

State group: Quote; State AI được hiểu: ACTIVE / EXPIRED / VOID / SUPERSEDED; Câu trả lời hợp lệ: Dùng ACTIVE; hết hạn thì xin snapshot mới.; Câu trả lời bị cấm: Dùng snapshot hết hạn để chốt.

State group: Order Draft; State AI được hiểu: DRAFT_CREATED / MISSING_CUSTOMER_INFO / AWAITING_CONFIRMATION; Câu trả lời hợp lệ: Dẫn khách xác nhận, ưu tiên CTA.; Câu trả lời bị cấm: Nói đơn chính thức khi mới là draft.

State group: Official Order; State AI được hiểu: CREATED / CONFIRMED / CANCELLED / FAILED; Câu trả lời hợp lệ: Hiển thị order_code nếu đã tạo.; Câu trả lời bị cấm: Tạo mã đơn giả hoặc nói đã ghi nhận khi chưa có order_code.

State group: Payment; State AI được hiểu: METHOD_SELECTED / WAITING_TRANSFER / COD_SELECTED / PAID_CONFIRMED / FAILED; Câu trả lời hợp lệ: Chỉ nói đã thanh toán khi PAID_CONFIRMED.; Câu trả lời bị cấm: Nói paid khi COD_SELECTED hoặc METHOD_SELECTED.

State group: CRM; State AI được hiểu: ELIGIBLE / SUPPRESSED / OPT_OUT / COMPLAINT_OPEN / FREQUENCY_CAPPED; Câu trả lời hợp lệ: Gửi CRM chỉ khi ELIGIBLE.; Câu trả lời bị cấm: Gửi quảng bá khi suppressed/opt-out/complaint.

## 16. PUBLIC / PRIVATE DATA BOUNDARY

Nhóm dữ liệu: Product; Public được dùng: Tên sản phẩm công khai, nhóm sản phẩm, vị, cách dùng, claim public-safe.; Internal bị chặn: Internal SKU code, formula nội bộ, định mức sản xuất, costing, supplier private.

Nhóm dữ liệu: Traceability; Public được dùng: Public trace whitelist nếu QR public-valid.; Internal bị chặn: QC defect, internal trace graph, supplier cost, personnel, MISA refs.

Nhóm dữ liệu: Inventory; Public được dùng: Thông điệp public-safe theo sellable/block reason.; Internal bị chặn: Số tồn chi tiết, raw inventory, warehouse notes, batch internal state.

Nhóm dữ liệu: Pricing; Public được dùng: QuoteSnapshot public breakdown nếu được phép.; Internal bị chặn: Công thức tính nội bộ, margin, commission internal, manual override note.

Nhóm dữ liệu: Customer; Public được dùng: Thông tin khách đã xác thực và cần cho tư vấn/đơn.; Internal bị chặn: PII không cần thiết, dữ liệu người khác, segment nội bộ nhạy cảm.

Nhóm dữ liệu: Order/payment; Public được dùng: Order_code và public-safe payment/shipping state.; Internal bị chặn: Bank reconciliation internal, risk score, fraud note, MISA sync id.

Nhóm dữ liệu: Quality/complaint; Public được dùng: Hướng dẫn CSKH public-safe.; Internal bị chặn: Nội dung CAPA nội bộ, defect root cause chưa công bố, nhân sự xử lý.

## 17. SAFETY / PUBLIC WORDING / CLAIM GUARD DESIGN

Guard phải đặt sau khi AI tạo candidate response và trước khi trả lời ra kênh. Mục tiêu là giữ lực bán nhưng không claim sai, không lộ nội bộ và không dùng ngôn ngữ kỹ thuật với khách.

Lớp guard: Claim Guard; Kiểm tra: Có nói chữa bệnh, điều trị, khỏi bệnh, thay thuốc hoặc claim y tế vượt whitelist không.; Hành động nếu fail: Chặn câu trả lời, chuyển sang public-safe wording hoặc human review nếu nhạy cảm.

Lớp guard: Public Wording Guard; Kiểm tra: Có dùng mã nội bộ, chữ "hệ thống", thuật ngữ DB/API/runtime với khách không.; Hành động nếu fail: Viết lại sang ngôn ngữ tự nhiên: "Dạ Em … ạ".

Lớp guard: Data Leakage Guard; Kiểm tra: Có lộ cost, supplier, QC defect, trace internal, MISA, commission, audit, risk score không.; Hành động nếu fail: Loại bỏ hoặc chuyển human/internal channel.

Lớp guard: Commercial Truth Guard; Kiểm tra: Có tự tính giá/tồn/sellable/paid/order_code không.; Hành động nếu fail: Block response; bắt buộc gọi runtime hoặc trả lời chưa thể xác nhận.

Lớp guard: Tone Guard; Kiểm tra: Có quá máy móc, lan man, disclaimer làm mất lực bán không.; Hành động nếu fail: Rút gọn, tăng tính tư vấn chuyên gia, giữ an toàn cần thiết.

Lớp guard: Health Risk Guard; Kiểm tra: Có thông tin dị ứng, bệnh nền, phụ nữ có thai, trẻ nhỏ hoặc vấn đề nhạy cảm không.; Hành động nếu fail: Tư vấn public-safe, không claim chữa bệnh, có thể chuyển human nếu cần.

## 18. CHANNEL HANDOFF DESIGN

Intent/Context: Khách hỏi giá / chốt mua / số lượng / địa chỉ / số điện thoại; Public response: Không lộ chi tiết nhạy cảm public; mời vào inbox nếu policy cho phép.; Private/Messenger action: Mở/tiếp tục Messenger, tạo quote/draft theo runtime.; Human handoff: Nếu lỗi runtime hoặc case phức tạp.

Intent/Context: Comment mơ hồ/ký hiệu; Public response: Có thể phản hồi rất ngắn hoặc kéo private theo policy.; Private/Messenger action: Hỏi nhu cầu tối thiểu trong Messenger.; Human handoff: Nếu spam/troll/hệ thống nhận diện rủi ro.

Intent/Context: Hỏi công dụng chung; Public response: Trả lời public-safe ngắn, có lực bán.; Private/Messenger action: Nếu đi sâu cá nhân hóa/giá/chốt thì kéo private.; Human handoff: Nếu health-sensitive.

Intent/Context: Khiếu nại có mã đơn/bằng chứng; Public response: Không tranh luận public; ghi nhận và mời hỗ trợ riêng.; Private/Messenger action: Tạo case/handoff CSKH/Quality nếu có.; Human handoff: Có.

Intent/Context: Chửi thề/công kích/lừa đảo không evidence; Public response: Không reply public nếu moderation policy khóa.; Private/Messenger action: Không kéo Messenger nếu thuộc nhóm hide/no reply.; Human handoff: Chỉ escalated review nếu cần.

Intent/Context: CRM chăm sóc lại; Public response: Không dùng public comment làm CRM cá nhân.; Private/Messenger action: Gửi theo whitelist và suppression guard.; Human handoff: Nếu phản hồi xấu/complaint.

## 19. ERROR AND FALLBACK DESIGN

Lỗi / trạng thái thiếu: Product resolver unavailable; Fail-safe response: Không tư vấn chi tiết; xin phép kiểm tra lại hoặc chuyển tư vấn viên.; Không được làm: Bịa mô tả sản phẩm.

Lỗi / trạng thái thiếu: Sellable runtime unavailable; Fail-safe response: Không chốt bán; không nói còn hàng.; Không được làm: Dựa vào active SKU hoặc tồn cache để bán.

Lỗi / trạng thái thiếu: Quote runtime unavailable; Fail-safe response: Không báo final price; nói giá cần được xác nhận lại.; Không được làm: Tự tính final price.

Lỗi / trạng thái thiếu: Quote expired; Fail-safe response: Xin snapshot mới.; Không được làm: Dùng giá hết hạn để chốt.

Lỗi / trạng thái thiếu: Customer member context conflict; Fail-safe response: Dùng runtime resolver hoặc human review.; Không được làm: Tự chọn tier có lợi hoặc hỏi khách để tự áp discount.

Lỗi / trạng thái thiếu: Payment state unavailable; Fail-safe response: Nói chưa xác nhận được trạng thái thanh toán.; Không được làm: Nói đã thanh toán vì khách nói đã chuyển.

Lỗi / trạng thái thiếu: Recall/sale lock conflict; Fail-safe response: Dừng bán và escalate/handoff theo policy.; Không được làm: Đề xuất mua tiếp.

Lỗi / trạng thái thiếu: Channel handoff failed; Fail-safe response: Public fallback an toàn, không lộ PII/giá nhạy cảm.; Không được làm: Tiếp tục thu thông tin cá nhân ở public.

Lỗi / trạng thái thiếu: Safety guard fail; Fail-safe response: Rewrite hoặc block/handoff.; Không được làm: Trả lời thô chưa qua guard.

## 20. SECURITY / AUDIT / IDEMPOTENCY DESIGN

Actor & correlation: Mọi request AI phải có correlation_id. Actor bot/system/human agent phải rõ theo PHASE 0. UNKNOWN actor không được bypass security.

Permission: Admin preview, override hoặc manual human intervention phải theo backend permission. Frontend không là security boundary.

Audit: Ghi audit cho các hành động có side effect: quote request, order draft request, customer confirmation, handoff, suppression override, human handoff.

Evidence: Evidence cho smoke/release phải đi vào Evidence Registry. Submitted không phải Accepted.

Idempotency: Các command có side effect như create quote/order draft/confirmation/handoff phải có idempotency key hoặc cơ chế owner-approved tương đương.

PII protection: Chỉ log field cần thiết; mask phone/address/payment details trong log nếu không cần cho evidence.

Prompt safety: Prompt/context đưa vào AI không được chứa secret, token, cost internal, raw formula hoặc dữ liệu khách không cần thiết.

Rate limits: Channel/live/CRM phải tôn trọng rate limit và anti-spam policy, không tự retry gây trùng tin.

## 21. OBSERVABILITY DESIGN

Metric / log: ai_runtime_resolution_failure_rate; Mục đích: Theo dõi tỷ lệ resolver lỗi.; Ví dụ điều kiện cảnh báo: Tăng bất thường trong live hoặc campaign.

Metric / log: ai_self_compute_guard_block_count; Mục đích: Đếm lần guard phát hiện AI định tự tính giá/tồn/sellable.; Ví dụ điều kiện cảnh báo: Bất kỳ tăng mạnh sau deploy đều cần review.

Metric / log: quote_snapshot_usage_expired_block_count; Mục đích: Đếm lần AI cố dùng quote hết hạn.; Ví dụ điều kiện cảnh báo: Có thể do cache/session sai.

Metric / log: public_leakage_guard_block_count; Mục đích: Đếm nội dung bị chặn do lộ internal data.; Ví dụ điều kiện cảnh báo: Bất kỳ case nghiêm trọng phải review evidence.

Metric / log: claim_guard_rewrite_count; Mục đích: Đếm câu trả lời phải chuyển public-safe.; Ví dụ điều kiện cảnh báo: Tăng cao có thể do product knowledge/prompt wording sai.

Metric / log: handoff_failed_count; Mục đích: Theo dõi kéo Messenger/private thất bại.; Ví dụ điều kiện cảnh báo: Kích hoạt fallback public-safe và log.

Metric / log: crm_suppression_count; Mục đích: Đếm CRM bị chặn do opt-out/complaint/frequency/not sellable.; Ví dụ điều kiện cảnh báo: Dùng để chứng minh không spam/sai trạng thái.

## 22. TECHNICAL DESIGN MATRIX

Mảng thiết kế: Runtime Consumer; Thiết kế mục tiêu: Tách client/resolver cho Product, Sellable, Quote, Order, Payment, Shipping, Recall/Sale Lock.; Điều kiện đạt: Không còn đường tự tính/hardcode trong AI.

Mảng thiết kế: Customer Memory; Thiết kế mục tiêu: Memory chỉ phục vụ ngữ cảnh tư vấn và CRM; member benefit lấy từ runtime.; Điều kiện đạt: Test khách cũ/khách mới/tier conflict pass.

Mảng thiết kế: Product Knowledge; Thiết kế mục tiêu: Public view có claim whitelist, dietary flags và product group; không lộ SKU code nội bộ.; Điều kiện đạt: Preview public/internal tách đúng.

Mảng thiết kế: Consultation Orchestrator; Thiết kế mục tiêu: Luồng chọn sản phẩm dựa trên need + knowledge + sellable + guard.; Điều kiện đạt: Không đề xuất SKU not sellable/out-of-stock/recall.

Mảng thiết kế: Quote/Order; Thiết kế mục tiêu: Mọi giá cuối từ QuoteSnapshot; order chính thức chỉ khi có order_code.; Điều kiện đạt: Test no QuoteSnapshot/no order_code/no paid pass.

Mảng thiết kế: Safety Guard; Thiết kế mục tiêu: Guard claim, wording, leakage, commercial truth và tone.; Điều kiện đạt: Test claim quá mức, leak internal, chữ "hệ thống", tự tính giá bị chặn.

Mảng thiết kế: Channel Bridge; Thiết kế mục tiêu: Public comment -> Messenger/private theo policy; Live không hardcode giá; CRM có suppression.; Điều kiện đạt: Test Live/Messenger/CRM và handoff failure pass.

Mảng thiết kế: Evidence/Smoke; Thiết kế mục tiêu: Có test matrix và evidence registry mapping.; Điều kiện đạt: Evidence accepted mới tính release gate.

## 23. DESIGN CONFLICT DETECTION MATRIX

Conflict cần dò: AI tự tính giá; Dấu hiệu trong codebase: Có công thức cộng/trừ giá hoặc discount trong AI prompt/service/frontend.; Kết luận thiết kế: Phải xóa khỏi AI path và chuyển sang QuoteSnapshot consumer ở implementation sau.

Conflict cần dò: AI hardcode SKU/price; Dấu hiệu trong codebase: Danh sách SKU/giá nằm trong prompt hoặc constant business logic.; Kết luận thiết kế: Chỉ cho phép seed/knowledge public data có version; business logic không hardcode.

Conflict cần dò: AI tự quyết sellable; Dấu hiệu trong codebase: Dựa vào product active, warehouse stock hoặc text "còn hàng".; Kết luận thiết kế: Sellable phải từ Commerce Runtime.

Conflict cần dò: AI tự nói paid; Dấu hiệu trong codebase: Dựa vào khách báo đã chuyển khoản/COD.; Kết luận thiết kế: Paid chỉ từ Payment Core public-safe state.

Conflict cần dò: AI tự tạo order_code; Dấu hiệu trong codebase: Mã đơn sinh trong AI/channel.; Kết luận thiết kế: Order code chỉ do Order Core tạo.

Conflict cần dò: Public leak; Dấu hiệu trong codebase: Trả internal SKU code, cost, formula, QC notes, supplier, MISA refs.; Kết luận thiết kế: Phải qua Public Wording/Data Leakage Guard.

Conflict cần dò: CRM sai trạng thái; Dấu hiệu trong codebase: Gửi CRM khi opt-out, complaint, not sellable, frequency cap.; Kết luận thiết kế: CRM Bridge phải consume suppression runtime.

Conflict cần dò: Live hardcode giá; Dấu hiệu trong codebase: Giá live nằm trong prompt hoặc cấu hình tĩnh ngoài runtime.; Kết luận thiết kế: Live quote phải từ runtime session/QuoteSnapshot.

## 24. REQUIRED EVIDENCE DESIGN

Evidence code: P4-EV-001; Nội dung evidence: Analysis report xác nhận current state/gap/conflict của AI Advisor codebase.; Điều kiện accepted: Có repo/path/commit hoặc trạng thái inspection rõ; không suy diễn.

Evidence code: P4-EV-002; Nội dung evidence: Runtime Consumer Contract mapping với PHASE 1/2/3 owner.; Điều kiện accepted: Mỗi field có owner, public/private class và fallback.

Evidence code: P4-EV-003; Nội dung evidence: No self-compute proof.; Điều kiện accepted: Test/grep/review chứng minh AI không tự tính giá/tồn/sellable/paid/revenue.

Evidence code: P4-EV-004; Nội dung evidence: Public/private leakage test evidence.; Điều kiện accepted: Các field internal bị chặn; public response sạch.

Evidence code: P4-EV-005; Nội dung evidence: QuoteSnapshot consumer test evidence.; Điều kiện accepted: Không có QuoteSnapshot thì không có final price; expired quote bị block.

Evidence code: P4-EV-006; Nội dung evidence: Order draft/confirmation evidence.; Điều kiện accepted: Draft không phải official; official chỉ khi có order_code.

Evidence code: P4-EV-007; Nội dung evidence: Safety/claim guard evidence.; Điều kiện accepted: Claim quá mức được rewrite/block, vẫn giữ lực bán.

Evidence code: P4-EV-008; Nội dung evidence: Channel handoff evidence.; Điều kiện accepted: Public/Messenger/Live/CRM xử lý đúng policy, có fallback khi handoff fail.

Evidence code: P4-EV-009; Nội dung evidence: CRM suppression evidence.; Điều kiện accepted: Opt-out/complaint/not sellable/frequency cap chặn CRM.

Evidence code: P4-EV-010; Nội dung evidence: Smoke report 14 mục.; Điều kiện accepted: Không còn P0 điểm chặn mở; evidence status ACCEPTED nếu dùng cho release.

## 25. SMOKE REQUIRED

Smoke code: P4-SMK-001; Kịch bản: Khách hỏi giá SKU sellable.; Kết quả phải đạt: AI gọi QuoteSnapshot, hiển thị final price/expiry từ snapshot, không tự tính.

Smoke code: P4-SMK-002; Kịch bản: Khách hỏi SKU out-of-stock/not sellable.; Kết quả phải đạt: AI không chốt bán, nói public-safe, đề xuất thay thế nếu policy cho phép.

Smoke code: P4-SMK-003; Kịch bản: Runtime sellable unavailable.; Kết quả phải đạt: AI fail-safe, không nói còn hàng.

Smoke code: P4-SMK-004; Kịch bản: QuoteSnapshot expired.; Kết quả phải đạt: AI không dùng giá cũ, yêu cầu snapshot mới.

Smoke code: P4-SMK-005; Kịch bản: Khách chọn COD hoặc chuyển khoản.; Kết quả phải đạt: AI không nói paid cho đến khi Payment Core confirmed.

Smoke code: P4-SMK-006; Kịch bản: Order Draft chưa xác nhận.; Kết quả phải đạt: AI không nói đơn chính thức, chưa có order_code.

Smoke code: P4-SMK-007; Kịch bản: Customer Confirmation hợp lệ.; Kết quả phải đạt: AI gửi confirmation event; chỉ hiển thị order_code từ Order Core.

Smoke code: P4-SMK-008; Kịch bản: Khách có complaint mở.; Kết quả phải đạt: AI dừng upsell/CRM, route hỗ trợ.

Smoke code: P4-SMK-009; Kịch bản: Public response có internal SKU/cost/formula/QC note.; Kết quả phải đạt: Safety guard chặn hoặc rewrite.

Smoke code: P4-SMK-010; Kịch bản: Claim chữa bệnh/quá mức.; Kết quả phải đạt: Claim guard rewrite public-safe hoặc handoff.

Smoke code: P4-SMK-011; Kịch bản: Comment public mơ hồ trong Live.; Kết quả phải đạt: Bridge kéo Messenger nếu policy cho phép; không lộ giá/order public.

Smoke code: P4-SMK-012; Kịch bản: Live price thay đổi theo session runtime.; Kết quả phải đạt: AI không hardcode, quote theo runtime.

Smoke code: P4-SMK-013; Kịch bản: CRM tới khách opt-out/frequency capped/not sellable.; Kết quả phải đạt: CRM bị suppressed và log lý do.

Smoke code: P4-SMK-014; Kịch bản: Recall/Sale Lock active.; Kết quả phải đạt: AI/CRM/Live/Ads/Gateway consumer đều bị chặn bán.

Smoke code: P4-SMK-015; Kịch bản: Khách cũ có member tier trong memory nhưng runtime benefit unavailable.; Kết quả phải đạt: AI cá nhân hóa tư vấn nhưng không tự áp giảm giá.

## 26. EXECUTION STEPS - TECHNICAL DESIGN ONLY

## 1. Đọc báo cáo 00-PHÂN TÍCH HIỆN TRẠNG.md và xác định các module/service/file hiện có liên quan đến AI Advisor.

## 2. Map từng truth AI đang cần sang owner Phase 1/2/3; đánh dấu mọi chỗ AI đang tự tính là conflict.

## 3. Thiết kế Runtime Consumer Contract ở mức field/owner/privacy/fallback, chưa viết DTO code.

## 4. Thiết kế component boundary và trách nhiệm rõ cho Customer Memory, Product Knowledge, Consultation Orchestrator, Quote/Order Consumer, Safety Guard và Channel Bridge.

## 5. Thiết kế data flow cho tư vấn, quote, order draft, customer confirmation, payment state, shipping state, CRM và handoff.

## 6. Thiết kế fail-safe khi resolver/runtime unavailable hoặc conflict.

## 7. Thiết kế audit/evidence/idempotency cho mọi side effect.

## 8. Viết technical design report 14 mục, ghi rõ điểm chặn/gap nếu có.

## 9. Không sửa code và không mở Gateway sau bước này.

## 27. REQUIRED REPORT FORMAT - 14 MỤC

Mục: 1; Tên mục report: Scope reviewed; Nội dung bắt buộc: Repo/module/service/file đã rà hoặc ghi rõ WAITING inspection.

Mục: 2; Tên mục report: Source-of-truth mapping; Nội dung bắt buộc: Mapping PHASE 1/2/3/MASTER cho từng truth AI consume.

Mục: 3; Tên mục report: Current conflicts; Nội dung bắt buộc: Các điểm AI đang tự tính/hardcode/bypass nếu có.

Mục: 4; Tên mục report: Target architecture; Nội dung bắt buộc: Component boundary và luồng dữ liệu mục tiêu.

Mục: 5; Tên mục report: Runtime Consumer Contract; Nội dung bắt buộc: Danh sách field, owner, privacy class, fallback.

Mục: 6; Tên mục report: Customer Memory design; Nội dung bắt buộc: Cách dùng memory để tư vấn, không quyết giá/tồn.

Mục: 7; Tên mục report: Product Knowledge design; Nội dung bắt buộc: Public view, claim whitelist, dietary/sensory positioning.

Mục: 8; Tên mục report: Consultation Orchestrator design; Nội dung bắt buộc: Logic chọn response path dựa trên need + runtime + guard.

Mục: 9; Tên mục report: Quote/Order consumer design; Nội dung bắt buộc: QuoteSnapshot, order draft, customer confirmation, order_code, payment state.

Mục: 10; Tên mục report: Safety/Public Wording design; Nội dung bắt buộc: Claim guard, data leakage, public/private, customer-facing tone.

Mục: 11; Tên mục report: Channel/CRM handoff design; Nội dung bắt buộc: Messenger/Live/CRM/human handoff và suppression.

Mục: 12; Tên mục report: Security/Audit/Evidence/Idempotency; Nội dung bắt buộc: Correlation, actor, permission, audit, evidence, idempotency.

Mục: 13; Tên mục report: Smoke required; Nội dung bắt buộc: Smoke P4-SMK-001 đến P4-SMK-015 hoặc tương đương có evidence.

## 28. DONE GATE

Có target architecture rõ cho AI Advisor Runtime.

Có Runtime Consumer Contract đủ owner, privacy class và fallback.

Có component boundary cho toàn bộ cụm P4-2A đến P4-2F.

Có ma trận no self-compute cho sellable, stock, price, discount, member benefit, order, paid, revenue, commission, ROAS, payout.

Có public/private boundary và Safety/Claim Guard design.

Có Channel / Messenger / Live / CRM Bridge design.

Có smoke required và evidence mapping.

Có report 14 mục.

Không có tuyên bố Phase 4 complete, Completion Decision, Release Readiness, Production Readiness hoặc Go-live Decision.

## 29. FAIL GATE

Thiết kế cho phép AI tự tính giá, tồn, sellable, discount, member benefit hoặc final payable.

Thiết kế cho phép AI tự tạo order_code, tự set paid, tự verified revenue, tự tính commission/ROAS/payout.

Không tách public/private data hoặc để lộ internal SKU code, cost, formula, QC note, supplier, MISA refs.

Không có QuoteSnapshot contract nhưng vẫn cho AI báo final price.

Không có recall/sale lock suppression trong AI/CRM/Live.

Không có idempotency/audit/evidence cho side effect.

Không có smoke required hoặc không có report 14 mục.

Có bất kỳ câu nào gọi Gateway PASS hoặc Production Readiness khi chưa có evidence accepted và owner sign-off.

## 30. DOWNSTREAM HANDOFF

Nếu P4-1 đạt Done Gate, Phase 4 được phép chuyển sang các file Limited Implementation sau, nhưng vẫn theo từng cụm nhỏ và vẫn không mở Gateway.

File kế tiếp: 02-HỢP ĐỒNG TIÊU THỤ DỮ LIỆU THỜI GIAN CHẠY.docx; Mục tiêu downstream: Khóa chi tiết contract consumer giữa AI và PHASE 1/2/3 runtime.; Điều kiện trước khi viết: P4-1 design đã có mapping owner và no self-compute.

File kế tiếp: 03-BỘ NHỚ KHÁCH HÀNG VÀ BỘ GIẢI NGỮ CẢNH.docx; Mục tiêu downstream: Khóa customer memory, member context, need profile, complaint/opt-out guard.; Điều kiện trước khi viết: Không để memory quyết giá/tồn/member benefit.

File kế tiếp: 04-ĐIỀU PHỐI TƯ VẤN KIẾN THỨC SẢN PHẨM.docx; Mục tiêu downstream: Khóa product knowledge public view và logic tư vấn sản phẩm.; Điều kiện trước khi viết: Public/private/claim guard đã rõ.

File kế tiếp: 05-TIÊU THỤ BÁO GIÁ NHÁP VÀ XÁC NHẬN ĐƠN.docx; Mục tiêu downstream: Khóa quote, order draft, confirmation và order_code consumer.; Điều kiện trước khi viết: QuoteSnapshot và Order Core owner đã rõ.

File kế tiếp: 06-CHẶN AN TOÀN NGÔN TỪ CÔNG KHAI VÀ TUYÊN BỐ.docx; Mục tiêu downstream: Khóa guard claim, wording, leak, tone, medical safety.; Điều kiện trước khi viết: Danh mục field public/internal và claim rule đã rõ.

File kế tiếp: 07-CẦU NỐI BÀN GIAO KÊNH MESSENGER TRỰC TIẾP VÀ CRM.docx; Mục tiêu downstream: Khóa Live/Messenger/CRM bridge, public/private channel và handoff.; Điều kiện trước khi viết: Channel policy và suppression rule đã rõ.

File kế tiếp: 08-BÁO CÁO KIỂM THỬ KHÓI VÀ BẰNG CHỨNG.docx; Mục tiêu downstream: Khóa smoke/evidence/report toàn Phase 4.; Điều kiện trước khi viết: Các cụm 2A-2F đã có evidence.

## 31. PROMPT COPY GIAO DEV / CODEX / COPILOT

## PROMPT-P4-1 - TECHNICAL DESIGN ONLY

## PHASE-04-AI-ADVISOR-RUNTIME

## 02-HỢP ĐỒNG TIÊU THỤ DỮ LIỆU THỜI GIAN CHẠY.docx



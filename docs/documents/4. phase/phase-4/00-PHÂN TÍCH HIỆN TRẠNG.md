# P4 - PHÂN TÍCH HIỆN TRẠNG

## Khối nguồn bắt buộc đã nhập vào file này

File này có khối nguồn bắt buộc để chạy lẻ, nhưng khi bàn giao Phase 4 phải đọc `12-BẢNG GOM GIAI ĐOẠN 4.md`, `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` trước. Nếu nội dung file này khác Bảng Gôm/phụ lục/SRS handoff, áp dụng Bảng Gôm/phụ lục/SRS handoff và xử lý fail-closed.

### A. Nội dung nguồn đã nhập

1. Master/runtime governance: mọi quyết định phải đi theo chuỗi nguồn sự thật. Product/Formula/Public Product Knowledge quyết định dữ liệu sản phẩm công khai. Operational Core quyết định sellable, recall, sale lock, batch/trace public-safe. Commerce Runtime quyết định DailySalesContext, QuoteSnapshot, OrderDraft, CustomerConfirmation, order_code, payment public-safe state, shipping/ETA và ORDER_VERIFIED. AI Advisor chỉ consume/orchestrate/render, không tạo nguồn sự thật song song.
2. Evidence/release governance: local-ready, DONE, PASS cục bộ, smoke pass hoặc completion report dạng chữ không được xem là Completion PASS, Release Ready, Production Ready hoặc Go-live. Mọi gate mặc định là PENDING_EVIDENCE cho đến khi có artifact được owner chấp nhận.
3. AI Advisor business/tech lock: AI được tư vấn, cá nhân hóa, chọn hướng hội thoại, tạo response candidate, gọi runtime hợp lệ, render câu trả lời có lực bán và handoff đúng kênh. AI không được tự tính giá, tồn, discount, quyền lợi thành viên, Diamond benefit, ship, VAT, order_code, paid, verified revenue, commission, ROAS hoặc payout.
4. Commerce lock: final price chỉ đến từ QuoteSnapshot còn hiệu lực. Order Draft không phải Official Order. Chỉ có CustomerConfirmation hợp lệ và order_code từ Commerce Runtime mới cho phép nói đơn đã ghi nhận chính thức. Payment selected, COD selected hoặc ảnh chuyển khoản không phải Paid.
5. Product/Operational lock: AI chỉ dùng product public-safe data, ProductRoleMatrix và sellable/runtime signals. Không dùng mã SKU nội bộ trong lời khách, không lộ công thức/tỷ lệ/cost/supplier/QC note, không bán SKU not sellable, recall, sale lock, channel suppressed hoặc thiếu proof dietary.
6. CRM/Member lock: customer memory chỉ hỗ trợ hiểu khách, không tự quyết quyền lợi giá. CRM/member messaging phải tôn trọng opt-out, quiet hour, open case, complaint, fatigue, frequency cap và canonical message registry. Không dùng raw message cũ nếu chưa qua guard ngôn từ khách hàng.
7. Finance/Diamond lock: Diamond/referral/commission/payout là Finance/Diamond state, không thuộc AI Advisor. AI chỉ nói theo trạng thái runtime được phép, không nói đã ghi nhận hoa hồng, đã trừ PIT hoặc đã chi trả nếu chưa có finance checkpoint.
8. Gateway/Ads/Live lock: Facebook Gateway chỉ là lớp nhận/normalize/route/deliver/log evidence, không tư vấn thay AI và không mở production nếu AI/evidence/smoke/owner sign-off chưa pass. Ads/ROAS chỉ tính revenue từ ORDER_VERIFIED. Live/MC AI chỉ dùng daily live board/script brief và phải kéo price/buy/deep consult sang private khi policy yêu cầu.

### B. Conflict lock

- Conflict giá 24/7, Giờ Vàng, member benefit hoặc Diamond buyer benefit: không hardcode, không chọn thủ công; chỉ dùng policy_version và QuoteSnapshot/runtime owner.
- Conflict lịch Giờ Vàng: không nói giờ cố định nếu GoldenHourResolver/policy version chưa pass.
- Conflict vegan/chay/công thức: không claim chay/vegan và không recommend cho khách ăn chay nếu proof chưa đủ hoặc có thành phần/rủi ro chưa clear.
- Conflict file tin nhắn/wording cũ: chỉ render qua customer-facing language guard; không dùng từ "hệ thống", SKU, runtime, resolver, gateway, API, cost, audit trong câu trả lời khách.
- Conflict Page/Gateway/App/Token: Phase 4 chỉ preserve channel context và handoff intent; không tự xác nhận Gateway production.
- Conflict evidence/completion: thiếu artifact hoặc owner acceptance thì giữ PENDING_EVIDENCE/BLOCKED, không gọi PASS.

### C. Trạng thái evidence bắt buộc

Trạng thái mặc định của file này: ANALYSIS_ONLY_WITH_IMPORTED_SOURCE_LOCK. Kết quả phân tích chỉ được gọi là report hoàn tất khi có gap matrix, conflict matrix, evidence requirement và blocker list. Không được gọi RUNTIME_IMPLEMENTATION_READY, GATEWAY_PASS, COMPLETION_PASS, RELEASE_READY, PRODUCTION_READY hoặc GO_LIVE_APPROVED.

### D. Khóa riêng của file này

File 00 chỉ phân tích hiện trạng. Nhiệm vụ là kiểm codebase/schema/API/test/prompt thật có đang vi phạm các khóa trên không, ghi rõ gap và blocker. Không sửa file, không tạo migration, không thêm seed, không bật feature flag, không chạy production traffic.

## Nội dung triển khai

## PROMPT-P4 - AI ADVISOR RUNTIME / CUSTOMER MEMORY / PRODUCT CONSULTATION / QUOTE ORDER CONSUMER / SAFETY WORDING ANALYSIS HANDOFF

Mode: ANALYSIS ONLY - DO NOT MODIFY FILES

Owner review: Required before any Technical Design or Implementation

Phase objective: Kiểm tra AI Advisor có consume đúng runtime của Phase 1, Phase 2, Phase 3 và các lock bổ sung hay không.

Hard rule: AI Advisor là runtime consumer / consultation orchestrator, không phải source-of-truth nghiệp vụ.

## 1. Phase marker

Phase: PHASE 04 - AI ADVISOR RUNTIME

File: 00-PHÂN TÍCH HIỆN TRẠNG.md

Prompt name: PROMPT-P4 - AI ADVISOR RUNTIME / CUSTOMER MEMORY / PRODUCT CONSULTATION / QUOTE ORDER CONSUMER / SAFETY WORDING ANALYSIS HANDOFF

Execution mode: ANALYSIS ONLY - DO NOT MODIFY FILES

Report requirement: Bắt buộc trả report 14 mục, có evidence list, smoke proposal, điểm chặn register và downstream handoff.

## 2. Cảnh báo nhận thức kỹ thuật

Tài liệu này không phải yêu cầu viết vài đoạn code để dev copy-paste. Một hệ thống AI Advisor vận hành thật phải khớp kiến trúc, database, API, state machine, runtime contract, bảo mật, phân quyền, audit, idempotency, evidence và gate release. Ví dụ chức năng tư vấn giá không chỉ là câu trả lời chatbot: phải có Product/SKU public data, Sellable Gate, QuoteSnapshot immutable, member benefit runtime, customer confirmation, order draft, official order code, payment state và audit. Nếu AI tự tính giá hoặc tự nói đơn đã chính thức, hệ thống sẽ sai nghiệp vụ và gây rủi ro thương mại.

Trong Phase 4, người phân tích chỉ được inspect codebase thật, database thật, API thật, migration thật, seed thật, test thật và prompt/runtime thật. Không sửa file, không code, không migration trong bước này.

## 3. Quan điểm khóa

Chuỗi đúng: Foundation truth -> Product/SKU/Recipe truth -> Operational truth -> Commerce truth -> AI/Channel consumer -> Gateway/Release review.

Không được đảo thứ tự. AI Advisor không được tự quyết product active, SKU active, recipe active, raw lot readiness, batch release, warehouse receipt, inventory availability, sellable status, listed price, program discount, member benefit, final price, official order, paid status, verified revenue, commission, ROAS hoặc payout.

AI Advisor chỉ được tư vấn, điều phối hội thoại, dùng customer memory và gọi/consume runtime đã được owner domain cấp.

## 4. Phạm vi phải phân tích

Người phân tích phải tìm trong codebase thật:

1. Module tạo AI prompt/context.
2. Module nhận channel/customer intent.
3. Module đọc product public knowledge.
4. Module đọc sellable/stock/recall/sale lock.
5. Module đọc DailySalesContext hoặc data tương đương.
6. Module đọc customer memory/member/CRM context.
7. Module đọc ProductRoleMatrix hoặc recommendation matrix.
8. Module đọc QuoteSnapshot.
9. Module tạo hoặc render OrderDraft/CustomerConfirmation/order_code.
10. Module render payment/shipping/ETA/tracking state.
11. Module kiểm public/private/PII/claim/language guard.
12. Module phát sinh handoff intent sang Messenger/Live/CRM/Gateway.
13. Module ghi DecisionEnvelope/evidence/log/correlation.
14. Test, fixture, smoke, seed, migration và doc contract liên quan.

## 5. Câu hỏi phân tích bắt buộc

Report phải trả lời rõ:

1. AI hiện có tự tính giá, tồn, discount, benefit, order, paid, shipping hoặc commission không?
2. AI hiện có đọc trực tiếp bảng nội bộ hoặc prompt raw có chứa cost, supplier, formula, QC note, SKU internal code không?
3. AI hiện có cơ chế fail-closed khi thiếu QuoteSnapshot, ProductRoleMatrix, DailySalesContext hoặc CustomerAdvisoryContext không?
4. AI có phân biệt public comment/live với private Messenger/chat không?
5. AI có leak PII, phone, address, payment, order, member tier, Diamond/commission hoặc memory riêng tư ra kênh công khai không?
6. AI có dùng banned term như "hệ thống", runtime, resolver, gateway, API, audit trong câu trả lời khách không?
7. AI có tôn trọng opt-out, quiet hour, open case, complaint, fatigue, frequency cap không?
8. AI có claim vegan/chay/health/dietary khi proof chưa đủ không?
9. AI có nói đơn chính thức trước khi có order_code không?
10. AI có nói đã thanh toán khi chỉ có ảnh chuyển khoản/COD selected/customer claim không?
11. AI có mở Gateway/Messenger production hoặc giả định Page token/webhook đã pass không?
12. Evidence hiện có là artifact thật hay chỉ narrative report?

## 6. Output report 14 mục bắt buộc

Report cuối của file 00 phải có đúng các mục sau:

1. Executive status: ANALYSIS_ONLY, PENDING_EVIDENCE, NO_PRODUCTION_CLAIM.
2. Codebase map: module/file/API/schema/test liên quan đến AI Advisor.
3. Runtime source map: AI đang consume nguồn nào cho product, sellable, quote, order, payment, shipping, CRM, Diamond, Gateway.
4. Self-calculation risk matrix: nơi AI/controller/prompt đang tự tính hoặc tự suy diễn.
5. Public/private safety matrix.
6. Customer memory/privacy matrix.
7. Product consultation/claim/dietary matrix.
8. Quote/order/payment/shipping matrix.
9. CRM/member/Diamond/Finance wording matrix.
10. Gateway/Live/Ads boundary matrix.
11. Conflict register: conflict nào fail-closed, conflict nào thiếu owner decision.
12. Evidence inventory: artifact thật, log, fixture, smoke, test; phân biệt missing/narrative-only.
13. Smoke proposal: P0 smoke case tối thiểu cho Phase 4.
14. Blocker and handoff: owner decision, upstream dependency, downstream Phase 5 blocker.

## 7. Done condition

File 00 chỉ đạt done khi report chứng minh được:

- Đã inspect đủ code/schema/API/test/prompt/runtime liên quan.
- Mọi gap đều có owner hoặc blocker.
- Mọi claim đều có evidence hoặc được đánh dấu missing.
- Không có dòng nào gọi Phase 4 là production-ready.
- Không mở Gateway, không gửi thật, không tạo order thật, không tạo payment state thật.

Nếu không đủ evidence, kết luận bắt buộc là:

P4-ANALYSIS-COMPLETE-WITH-BLOCKERS

hoặc:

P4-ANALYSIS-BLOCKED-BY-MISSING_RUNTIME_EVIDENCE
# P4 - PHÂN TÍCH HIỆN TRẠNG

## Khóa nguồn bắt buộc



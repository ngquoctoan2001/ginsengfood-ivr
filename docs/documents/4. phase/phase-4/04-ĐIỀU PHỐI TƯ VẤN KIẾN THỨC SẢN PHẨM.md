# P4.2C - PRODUCT KNOWLEDGE CONSULTATION ORCHESTRATOR

## Khối nguồn bắt buộc đã nhập vào file này

File này có khối nguồn bắt buộc để chạy lẻ, nhưng khi bàn giao Phase 4 phải đọc `12-BẢNG GOM GIAI ĐOẠN 4.md`, `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` trước. Nếu nội dung file này khác Bảng Gôm/phụ lục/SRS handoff, áp dụng Bảng Gôm/phụ lục/SRS handoff và xử lý fail-closed.

### A. Nội dung nguồn đã nhập

1. Master/runtime governance: tư vấn sản phẩm phải đi theo nguồn sự thật. Product/Formula/Public Product Knowledge quyết định tên công khai, nhóm sản phẩm, thành phần được phép nói, claim whitelist và mô tả public-safe. Operational Core quyết định recall, sale lock, trace public-safe, batch/channel suppression. Commerce Runtime quyết định sellable, QuoteSnapshot, order/payment/shipping và ORDER_VERIFIED. AI Advisor chỉ consume/orchestrate/render.
2. Evidence/release governance: consultation pass cục bộ không phải Completion PASS, Release Ready, Production Ready hoặc Go-live. Mọi gate mặc định là PENDING_EVIDENCE cho đến khi có artifact được owner chấp nhận.
3. AI Advisor consultation lock: AI được hiểu nhu cầu, xếp sản phẩm theo fit, giải thích lý do, gợi ý combo public-safe, tạo quote_request_intent khi có buy intent và render lời tư vấn có lực bán.
4. Commerce lock: tư vấn sản phẩm không được biến thành báo giá. Final price chỉ đến từ QuoteSnapshot. Buy intent chỉ tạo yêu cầu quote/order flow, không chốt giá hoặc đơn.
5. Product/Operational lock: không hardcode SKU list/combo/giá/tồn. Không tư vấn SKU not sellable, recall, sale lock, channel suppressed hoặc thiếu ProductRoleMatrix/public-safe proof. Không dùng mã SKU nội bộ trong lời khách.
6. CRM/Member lock: nhu cầu khách/member context chỉ là input để chọn lời tư vấn; không tự cộng quyền lợi, không tự gắn member/Diamond benefit, không tự bật CRM follow-up.
7. Finance/Diamond lock: tư vấn không nói commission/payout/PIT/payable. Diamond chỉ là ngữ cảnh nếu runtime trả và chỉ dùng để route/handoff.
8. Gateway/Ads/Live lock: Live/Ads/Page context chỉ định surface và attribution. Live script/board phải chặn SKU ngoài board; public comment hỏi giá/mua/tư vấn sâu phải route private nếu policy yêu cầu.

### B. Conflict lock

- Product active không đồng nghĩa sellable; recipe active không đồng nghĩa được bán; warehouse/batch truth không được AI tự suy luận.
- Nếu dietary/vegan/chay proof thiếu hoặc conflict: không claim, không recommend cho nhu cầu đó.
- Nếu sản phẩm bị recall/sale lock/channel suppression: dừng tư vấn bán hàng, chỉ trả public-safe hoặc alternative được phép.
- Nếu khách hỏi giá/chốt mua: không tự báo giá; chuyển sang QuoteSnapshot consumer.
- Nếu claim y tế vượt whitelist: chặn/rewrite/human handoff.
- Nếu evidence thiếu product public proof/sellable trace/guard trace: giữ PENDING_EVIDENCE, không gọi PASS.

### C. Trạng thái evidence bắt buộc

Trạng thái mặc định của file này: PRODUCT_CONSULTATION_LOCK_WITH_IMPORTED_SOURCE. Evidence tối thiểu phải có product public data, ProductRoleMatrix hoặc fit reason, sellable/suppression result, claim guard result, channel surface và quote_request_intent nếu có tín hiệu mua. Không được gọi RUNTIME_IMPLEMENTATION_READY, GATEWAY_PASS, COMPLETION_PASS, RELEASE_READY, PRODUCTION_READY hoặc GO_LIVE_APPROVED.

### D. Khóa riêng của file này

File 04 chỉ xử lý Product Knowledge và Consultation Orchestrator. Nhiệm vụ là tư vấn đúng sản phẩm và nhu cầu nhưng không để AI tự quyết bán được, giá, tồn, đơn, paid, revenue, commission, ROAS hoặc payout.

## Nội dung triển khai

## PROMPT-P4-2C - PRODUCT KNOWLEDGE / CONSULTATION ORCHESTRATOR / SKU FIT / SELLABLE GUARD / PUBLIC-SAFE COUNSEL HANDOFF

Tài liệu này khóa riêng lớp Product Knowledge và Consultation Orchestrator của AI Advisor Runtime. AI Advisor được phép tư vấn, xếp hạng lựa chọn và render câu trả lời bán hàng theo dữ liệu public-safe; nhưng không được tự quyết giá, tồn kho, sellable, quyền lợi thành viên, trạng thái đơn, thanh toán, doanh thu, hoa hồng, ROAS hoặc payout.

## PHASE MARKER

Phase: PHASE-04 - AI ADVISOR RUNTIME

File: 04-ĐIỀU PHỐI TƯ VẤN KIẾN THỨC SẢN PHẨM.docx

Prompt: PROMPT-P4-2C - Product Knowledge / Consultation Orchestrator / SKU Fit / Sellable Guard / Public-Safe Counsel Handoff

Mode: LIMITED IMPLEMENTATION HANDOFF - chỉ triển khai trong phạm vi Product Knowledge và Consultation Orchestrator sau khi đã pass Analysis Only, Technical Design Only, Runtime Consumer Contract và Customer Memory Context Resolver.

## 1. HEADER

Mục tiêu của file này là giao cho dev/Codex/Copilot một prompt triển khai giới hạn cho lớp tư vấn sản phẩm. Lớp này đứng giữa Customer Context Resolver, Product Knowledge Resolver, Safety/Claim Guard, Channel Context và Commerce Runtime. Kết quả mong muốn là AI Advisor tư vấn đúng sản phẩm, đúng nhu cầu, đúng ngữ cảnh bán hàng, không tư vấn SKU bị chặn bán và không lộ dữ liệu nội bộ.

AI Advisor là consumer/orchestrator, không phải source-of-truth nghiệp vụ.

Product Knowledge là nguồn hiểu sản phẩm public-safe, không phải nơi tính giá hoặc tồn kho.

Consultation Orchestrator chỉ chọn, giải thích, đề xuất và render lời tư vấn; mọi quyết định bán được hay không bán được phải lấy từ runtime.

Không hardcode combo, không hardcode SKU list trong business logic, không hardcode giá, không hardcode ưu đãi, không hardcode tồn kho.

Mọi câu tư vấn customer-facing phải dùng tên sản phẩm công khai, không dùng mã SKU nội bộ, không dùng từ "hệ thống" trong lời nói với khách.

## 2. MODE

Chế độ: Limited Implementation; Được phép: Được tạo/sửa các service, DTO, resolver, prompt template, policy, smoke test liên quan trực tiếp Product Knowledge và Consultation Orchestrator.; Không được phép: Không sửa Commerce Pricing, Sellable Gate, Order Core, Payment Core, Shipping Core, Verified Revenue, Commission, ROAS, Gateway hoặc IVR.

Chế độ: No source-of-truth override; Được phép: Được đọc Product/SKU public data, Customer Context, Sellable result, Safety policy, Channel policy.; Không được phép: Không tự tính hoặc ghi đè sellable, tồn kho, giá, discount, member benefit, Diamond benefit, order_code, paid, revenue.

Chế độ: No copy-paste build; Được phép: Được dùng AI để phân tích, checklist, cấu trúc prompt, test case, smoke.; Không được phép: Không copy các đoạn code rời rạc vào codebase nếu chưa inspect kiến trúc, database, API, dependency, test và security thật.

## 3. SOURCE-OF-TRUTH

Nhóm sự thật: Product/SKU public data; Owner: PHASE 1 - Product/SKU/Recipe/Product Activation; AI Advisor được làm gì: Đọc tên công khai, nhóm sản phẩm, mô tả, vị, nhóm nhu cầu, public-safe efficacy, dietary fit.; AI Advisor bị cấm làm gì: Không dùng mã nội bộ trong lời khách; không tự active/inactive SKU; không suy diễn recipe active thành sellable.

Nhóm sự thật: Operational truth; Owner: PHASE 2 - Operational Core; AI Advisor được làm gì: Đọc suppression/public-safe signal như recall/sale lock/channel suppression nếu runtime cung cấp.; AI Advisor bị cấm làm gì: Không tự đọc hoặc lộ QC notes, raw lot, supplier, formula, ledger, trace internal, public trace ngoài whitelist.

Nhóm sự thật: Sellable/Quote/Order truth; Owner: PHASE 3 - Commerce Runtime; AI Advisor được làm gì: Đọc sellable_status, public block reason, QuoteSnapshot, OrderDraft, confirmation, order_code, payment/shipping public-safe state.; AI Advisor bị cấm làm gì: Không tự tính sellable, stock, final price, expiry, order_code, paid, verified revenue, commission, ROAS.

Nhóm sự thật: Customer context; Owner: Customer Memory/Context Resolver; AI Advisor được làm gì: Dùng để hiểu khách mới/cũ, member tier do runtime xác nhận, nhu cầu, người dùng chính, lịch sử mua, dị ứng/kiêng kỵ.; AI Advisor bị cấm làm gì: Không tự quyết quyền lợi giá, không tự tính member benefit, không tự gắn Diamond benefit nếu runtime chưa xác nhận.

Nhóm sự thật: Claim/public wording; Owner: Safety / Public Wording / Claim Guard; AI Advisor được làm gì: Render lời tư vấn theo public-safe wording và tone Ginsengfood.; AI Advisor bị cấm làm gì: Không nói chữa bệnh, không cam kết điều trị, không biến sản phẩm thành thuốc, không làm mất lực bán bằng disclaimer không cần thiết.

## 4. ENTRY CONDITION

## 00-PHÂN TÍCH HIỆN TRẠNG.md đã hoàn tất report phân tích hoặc có kết quả current state đủ để triển khai có kiểm soát.

## 01-THIẾT KẾ KỸ THUẬT đã khóa boundary tổng thể của AI Advisor Runtime.

## 02-HỢP ĐỒNG TIÊU THỤ DỮ LIỆU THỜI GIAN CHẠY đã xác định AI Advisor nhận runtime data theo hợp đồng nào.

## 03-BỘ NHỚ KHÁCH HÀNG VÀ BỘ GIẢI NGỮ CẢNH đã khóa Customer Memory và Context Resolver theo read-only consumer view.

Dev đã inspect codebase thật, xác định vị trí hiện có của product knowledge, prompt template, resolver, service, channel adapter, test và smoke.

Không có P0 conflict đang mở liên quan việc AI tự tính giá/tồn/sellable hoặc trả lời SKU bị khóa bán.

## 5. OBJECTIVE

Thiết kế và triển khai giới hạn Product Knowledge / Consultation Orchestrator để AI Advisor có thể tư vấn sản phẩm theo nhu cầu thực tế của khách, giữ được linh hồn ẩm thực/thực dưỡng phương Đông, nhưng vẫn tuân thủ runtime guard, sellable guard và public-safe claim.

Mục tiêu: Tư vấn đúng sản phẩm; Diễn giải: AI chọn sản phẩm hoặc combo dựa trên nhu cầu, customer memory, product public data, dietary fit, safety guard và sellable runtime.; Tiêu chí đạt: Không đề xuất SKU không phù hợp hoặc bị chặn bán.

Mục tiêu: Giữ lực bán; Diễn giải: Câu tư vấn có lý do chọn sản phẩm, vị, vai trò bữa ăn, hiệu dụng Đông phương public-safe và CTA mềm phù hợp.; Tiêu chí đạt: Không trả lời khô, không chỉ nói "để kiểm tra", không phá chiến lược combo khi context đang phù hợp.

Mục tiêu: Không vượt quyền; Diễn giải: Orchestrator không tính giá, tồn, giảm giá, member benefit, final quote, order state.; Tiêu chí đạt: Mọi giá/quote/order đi qua Commerce Runtime và QuoteSnapshot.

Mục tiêu: Không lộ nội bộ; Diễn giải: Response public chỉ dùng public product name và public-safe fields.; Tiêu chí đạt: Không lộ internal SKU code, formula, cost, QC, supplier, lot, trace internal.

Mục tiêu: Mở rộng được; Diễn giải: Không hardcode 20 SKU trong logic; catalog mở rộng 40-50 SKU hoặc hơn qua registry/seed/resolver.; Tiêu chí đạt: SKU mới chỉ cần mở rộng product knowledge và policy mapping, không sửa lõi orchestrator.

## 6. SCOPE IN

Product Knowledge Resolver consumer view: đọc dữ liệu public-safe cho sản phẩm và candidate list.

Consultation Orchestrator: nhận intent, customer context, product need, candidate products, sellable result, safety guard, channel context và tạo consultation plan.

SKU fit ranking: xếp hạng sản phẩm theo nhu cầu, vị, chế độ ăn, dị ứng/kiêng kỵ, người dùng chính, mục đích mua, kênh và trạng thái sellable.

Replacement strategy: khi SKU khách hỏi không bán được, trả lời public-safe và gợi ý tối đa 3 lựa chọn thay thế phù hợp, không lộ số lượng tồn kho.

Combo/proposal strategy: đề xuất combo linh hoạt theo gia đình, người nhận, mùa, nhu cầu, nhưng không hardcode combo cố định.

Customer-facing render: tạo lời tư vấn theo tone Ginsengfood, có lực chốt, giữ public-safe claim và không dùng thuật ngữ nội bộ.

Smoke tests cho product consultation, sellable suppression, replacement, combo, health-sensitive entry, public/private leak và no-price-calculation.

## 7. SCOPE OUT

Ngoài phạm vi: Tính final price, discount, member benefit, Diamond benefit; Lý do: AI Advisor không phải Pricing Core.; Owner đúng: PHASE 3 - Commerce Runtime / QuoteSnapshot.

Ngoài phạm vi: Tính tồn kho, sellable, stock allocation; Lý do: AI không được tự quyết SKU còn bán được hay không.; Owner đúng: PHASE 3 Sellable Gate + Operational Inventory truth.

Ngoài phạm vi: Tạo order_code hoặc xác nhận đơn chính thức; Lý do: Order Draft không đồng nghĩa Official Order.; Owner đúng: PHASE 3 Official Order Core.

Ngoài phạm vi: Set Paid hoặc Verified Revenue; Lý do: Payment selected/COD selected không phải Paid; Delivered/Invoice không phải Verified Revenue.; Owner đúng: Payment Core / Verified Revenue Core.

Ngoài phạm vi: Sửa safety claim policy toàn cục; Lý do: File này chỉ consume guard; chi tiết policy thuộc P4-2E.; Owner đúng: P4-2E Safety/Public Wording/Claim Guard.

Ngoài phạm vi: Live/Messenger routing policy toàn cục; Lý do: File này chỉ dùng channel_context; chi tiết handoff thuộc P4-2F.; Owner đúng: P4-2F Channel Handoff.

Ngoài phạm vi: IVR; Lý do: IVR đang reserved, không triển khai trong Phase 4 trừ khi có phase riêng.; Owner đúng: PACK/PHASE IVR riêng, khi owner quyết định.

## 8. PRODUCT KNOWLEDGE DATA CONTRACT

Product Knowledge Resolver phải trả về dữ liệu đủ để tư vấn nhưng không chứa dữ liệu cấm lộ public. Mọi trường nhạy cảm phải nằm ngoài consumer view của AI Advisor hoặc được đánh dấu private-only và bị chặn bởi guard.

Nhóm field: Định danh công khai; Field đề xuất: product_public_id, public_product_name, public_slug, product_family, product_group; Mục đích tư vấn: Cho AI gọi đúng tên sản phẩm và nhóm: seasonal, functional, nourishing.; Public/Private: Public-safe

Nhóm field: Mô tả ẩm thực; Field đề xuất: taste_profile, texture_profile, aroma_profile, serving_context, meal_replacement_positioning; Mục đích tư vấn: Giải thích "ngon không", phù hợp bữa ăn nào, cảm nhận vị.; Public/Private: Public-safe

Nhóm field: Nhu cầu khách; Field đề xuất: need_tags, user_profile_fit, gifting_fit, family_fit, seasonal_fit; Mục đích tư vấn: Ghép nhu cầu khách với sản phẩm hoặc combo.; Public/Private: Public-safe

Nhóm field: Hiệu dụng Đông phương public-safe; Field đề xuất: eastern_wellness_public_concepts, culinary_efficacy_words, public_claim_block; Mục đích tư vấn: Giữ linh hồn y thực đồng nguyên: kiện tỳ, dưỡng khí, bổ huyết, dưỡng âm, thanh bổ… theo khung bữa ăn thực dưỡng.; Public/Private: Public-safe có guard

Nhóm field: Dinh dưỡng public-safe; Field đề xuất: nutrition_positioning, ingredient_public_summary, allergen_public_flags, dietary_fit; Mục đích tư vấn: Tư vấn theo ăn chay, dị ứng, kiêng kỵ, trẻ em/người lớn/người biếu.; Public/Private: Public-safe có guard

Nhóm field: Thay thế; Field đề xuất: replacement_candidate_rules, compatible_combo_roles, not_fit_reasons_public; Mục đích tư vấn: Đề xuất SKU thay thế khi SKU khách hỏi không bán được hoặc không phù hợp.; Public/Private: Public-safe

Nhóm field: Runtime guard hooks; Field đề xuất: required_sellable_check, required_safety_check, required_channel_check, required_quote_check; Mục đích tư vấn: Buộc orchestrator gọi đúng runtime/guard trước khi tư vấn/chốt.; Public/Private: Internal orchestration

Nhóm field: Dữ liệu cấm lộ; Field đề xuất: internal_sku_code, formula_detail, cost, supplier, QC note, raw lot, internal trace, margin, stock quantity; Mục đích tư vấn: Không dùng trong lời khách; nếu xuất hiện trong payload thì phải bị block.; Public/Private: Private-forbidden

## 9. CONSULTATION ORCHESTRATOR RESPONSIBILITY

Bước: 1. Nhận ngữ cảnh; Input: message, channel_context, customer_context, session_memory; Xử lý: Xác định intent: hỏi sản phẩm, hỏi vị, mua cho ai, mua combo, hỏi giá, hỏi bệnh/kiêng/dị ứng, khiếu nại, đổi trả.; Output: intent + entity + missing_critical_fields

Bước: 2. Lấy candidate; Input: product_need, product_public_data, customer_memory; Xử lý: Gọi Product Knowledge Resolver để lấy danh sách candidate public-safe; không hardcode danh sách SKU trong logic.; Output: candidate_products[]

Bước: 3. Guard phù hợp; Input: dietary_flags, allergy_flags, health_sensitive_flags; Xử lý: Loại sản phẩm không phù hợp chế độ ăn/dị ứng/kiêng kỵ đã biết; nếu health-sensitive thì dùng wording guard.; Output: eligible_by_safety[] + rejected_by_safety[]

Bước: 4. Guard bán được; Input: sellable_status, recall/sale lock/channel suppression; Xử lý: Gọi/consume runtime sellable result; không tư vấn/chốt SKU out of stock/not sellable/recall/sale lock/channel suppressed.; Output: eligible_by_runtime[] + public_block_reason

Bước: 5. Xếp hạng; Input: need_tags, taste, product role, family/gift context, sellable result; Xử lý: Xếp hạng 1-3 lựa chọn hoặc combo 3-4 dòng nếu context phù hợp; mỗi SKU có lý do chọn.; Output: ranked_recommendations

Bước: 6. Render câu trả lời; Input: ranked_recommendations, tone_profile, channel_policy; Xử lý: Tạo câu tư vấn có lực bán, public-safe, không lộ nội bộ, có CTA phù hợp với kênh.; Output: customer_response + next_action

Bước: 7. Chuyển quote nếu có tín hiệu mua; Input: buy_intent, selected_product(s), runtime eligibility; Xử lý: Không báo giá tự tính; chuyển sang Quote/Order Draft Consumer để lấy QuoteSnapshot.; Output: quote_request_intent hoặc handoff_required

## 10. CONSULTATION INPUT CONTRACT

Input: conversation_request; Bắt buộc: Có; Nguồn: Channel Adapter / AI Runtime; Ghi chú khóa: Nội dung khách hỏi, thời điểm, kênh, ngữ cảnh public/private.

Input: customer_context_readonly; Bắt buộc: Có nếu nhận diện được; Nguồn: P4-2B Context Resolver; Ghi chú khóa: Khách mới/cũ, member tier runtime-confirmed, lịch sử mua, người dùng chính, nhu cầu.

Input: product_need; Bắt buộc: Có nếu là tư vấn sản phẩm; Nguồn: Intent/Entity Resolver; Ghi chú khóa: Mua cho ai, mục đích, bữa ăn, vị, ăn chay, dị ứng, kiêng kỵ, biếu tặng, gia đình.

Input: product_public_data_candidates; Bắt buộc: Có; Nguồn: Product Knowledge Resolver; Ghi chú khóa: Dữ liệu public-safe, không chứa formula/cost/QC/supplier private.

Input: sellable_status; Bắt buộc: Bắt buộc trước khi chốt/đề xuất mua; Nguồn: Commerce Runtime; Ghi chú khóa: Không có sellable result thì không được chốt SKU.

Input: safety_claim_policy; Bắt buộc: Có; Nguồn: P4-2E Safety Guard; Ghi chú khóa: Chặn claim y tế, thuốc, điều trị; cho phép hiệu dụng ẩm thực Đông phương public-safe.

Input: channel_context; Bắt buộc: Có; Nguồn: P4-2F Channel Bridge; Ghi chú khóa: Public comment, Messenger/private, Live, CRM, Ads, quiet hours, handoff rule.

Input: quote_intent_flag; Bắt buộc: Có nếu khách có tín hiệu mua; Nguồn: Conversation Orchestrator; Ghi chú khóa: Chỉ là ý định tạo quote; final price vẫn do Commerce Runtime.

## 11. CONSULTATION OUTPUT CONTRACT

Output: consultation_plan; Nội dung: Kế hoạch tư vấn nội bộ: intent, candidate, rejected, selected, missing question, next action.; Public/Private: Internal; Guard bắt buộc: Không render trực tiếp cho khách.

Output: recommended_products; Nội dung: Danh sách sản phẩm/combos đề xuất, dùng public_product_name và public reason.; Public/Private: Public-safe; Guard bắt buộc: Chỉ gồm SKU sellable + phù hợp safety/dietary/channel.

Output: rejected_products; Nội dung: Danh sách bị loại và lý do nội bộ/public-safe.; Public/Private: Internal + public-safe reason; Guard bắt buộc: Không lộ tồn kho cụ thể, QC, recall internal.

Output: customer_response_blocks; Nội dung: Câu trả lời theo tone: tư vấn, giải thích vị, hiệu dụng, chốt mềm, fallback.; Public/Private: Public-safe; Guard bắt buộc: Không nói chữa bệnh, không lộ mã SKU nội bộ, không nói "hệ thống".

Output: handoff_required; Nội dung: Có/không: cần Messenger/private/human/admin/CSKH.; Public/Private: Internal orchestration; Guard bắt buộc: Public comment giá/quote/order phải kéo private nếu policy yêu cầu.

Output: quote_request_intent; Nội dung: Có/không: khách đã có tín hiệu mua và cần QuoteSnapshot.; Public/Private: Internal to Commerce Runtime; Guard bắt buộc: Không chứa final price tự tính.

Output: smoke_evidence_ref; Nội dung: Link/log evidence của test tư vấn.; Public/Private: Internal; Guard bắt buộc: Evidence submitted chưa phải accepted.

## 12. PRODUCT FIT RANKING RULE

Ranking không được là một danh sách if/else hardcode. Ranking phải dựa trên score hoặc rule engine có dữ liệu từ Product Knowledge, Customer Context, Safety Guard và Sellable Runtime.

Tiêu chí ranking: Intent match; Điểm/ưu tiên: Rất cao; Ghi chú triển khai: Sản phẩm phải khớp mục đích hỏi: ăn hằng ngày, phục hồi thể lực, nhẹ bụng, trẻ em, người lớn tuổi, biếu tặng, combo gia đình.

Tiêu chí ranking: Dietary/allergy fit; Điểm/ưu tiên: Bắt buộc; Ghi chú triển khai: Nếu khách nói ăn chay, dị ứng, kiêng kỵ, guard này thắng ranking bán hàng.

Tiêu chí ranking: Runtime sellable; Điểm/ưu tiên: Bắt buộc; Ghi chú triển khai: Không sellable thì loại khỏi recommended_products, chỉ có thể xuất hiện trong rejected_products với public-safe reason.

Tiêu chí ranking: Product role trong combo; Điểm/ưu tiên: Cao; Ghi chú triển khai: Mỗi SKU trong combo phải có vai trò: thanh bổ, kiện tỳ, dưỡng khí, bổ huyết, dưỡng âm, khoáng hóa, ôn trung, trợ dương… theo public-safe wording.

Tiêu chí ranking: Taste/sensory fit; Điểm/ưu tiên: Trung bình-cao; Ghi chú triển khai: Dùng để trả lời "có ngon không", "dễ ăn không", "bé/người lớn tuổi ăn được không".

Tiêu chí ranking: Customer history; Điểm/ưu tiên: Trung bình-cao; Ghi chú triển khai: Dùng lịch sử mua để tránh lặp vô ích hoặc đề xuất mua lại đúng chu kỳ; không tự quyết giá thành viên.

Tiêu chí ranking: Campaign/channel fit; Điểm/ưu tiên: Trung bình; Ghi chú triển khai: Chỉ dùng thông tin chương trình/channel đã được runtime/channel policy xác nhận; không hardcode Live/Giờ Vàng.

## 13. SELLABLE / SUPPRESSION RULE

Trạng thái runtime: SELLABLE; AI được trả lời: Được tư vấn và nếu khách có tín hiệu mua thì chuyển tạo QuoteSnapshot.; AI không được làm: Không tự báo final price nếu chưa có QuoteSnapshot.

Trạng thái runtime: OUT_OF_STOCK; AI được trả lời: Nói an toàn như "hiện chưa thể lên đơn/hiện đang hết hàng" và gợi ý sản phẩm thay thế phù hợp.; AI không được làm: Không công khai số lượng tồn; không nhận chốt đơn; không giữ giá.

Trạng thái runtime: NOT_SELLABLE; AI được trả lời: Giải thích public-safe theo block_reason; đề xuất lựa chọn khác nếu có.; AI không được làm: Không nói lý do nội bộ như QC hold/warehouse issue nếu không public-safe.

Trạng thái runtime: RECALL_OR_SALE_LOCK; AI được trả lời: Dừng tư vấn bán hàng, suppress Ads/Live/CRM, handoff CSKH/QA nếu là khiếu nại.; AI không được làm: Không gợi ý mua, không tạo quote, không tạo order draft.

Trạng thái runtime: CHANNEL_SUPPRESSED; AI được trả lời: Không đề xuất trên kênh đó; có thể chuyển kênh/handoff nếu policy cho phép.; AI không được làm: Không bypass bằng tin nhắn khác hoặc hardcode CTA.

Trạng thái runtime: UNKNOWN_RUNTIME; AI được trả lời: Không chốt; hỏi/đợi runtime resolver hoặc handoff nội bộ.; AI không được làm: Không đoán rằng SKU còn hàng hoặc bán được.

## 14. COMBO / PROPOSAL RULE

Khi tư vấn combo gia đình, combo theo mùa, combo 3-4 dòng hoặc proposal bán hàng, AI không được hardcode một combo cố định. Orchestrator phải hỏi ngắn khi thiếu thông tin thật sự cần thiết, sau đó đề xuất theo vai trò người dùng chính và tình huống mua.

Tình huống: Khách mua cho gia đình; Cách xử lý đúng: Hỏi ngắn thành phần gia đình/người dùng chính nếu chưa có: ba mẹ/người lớn tuổi, vợ/chồng, con nhỏ, mua biếu hay dùng chung. Sau đó đề xuất combo có vai trò từng SKU.; Điểm dễ fail: Hỏi y khoa lan man khi khách chưa nói bệnh/điều trị/dị ứng.

Tình huống: Khách từ Live hỏi "có ngon không?"; Cách xử lý đúng: Nếu ngữ cảnh đang là combo/proposal, giữ hướng combo 3-4 dòng, giải thích vị + hiệu dụng + vai trò; fallback mua 1 hộp nếu khách muốn thử.; Điểm dễ fail: Kéo tụt về 1-2 SKU làm vỡ AOV/chiến lược Ads.

Tình huống: Mua biếu cha mẹ; Cách xử lý đúng: Ưu tiên câu tư vấn trang trọng, dễ hiểu, có vai trò bồi bổ qua bữa ăn, thanh bổ/kiện tỳ/dưỡng khí phù hợp.; Điểm dễ fail: Nói chữa bệnh hoặc hứa hiệu quả y tế.

Tình huống: Trẻ em/người dùng nhạy cảm; Cách xử lý đúng: Dùng product knowledge + dietary/allergy guard; nếu có health-sensitive thì chuyển wording an toàn.; Điểm dễ fail: Tự khẳng định phù hợp mọi trẻ em hoặc mọi bệnh nền.

Tình huống: SKU chính hết hàng; Cách xử lý đúng: Không phá nhịp bán; chuyển sang tối đa 3 SKU thay thế có lý do sát nhu cầu.; Điểm dễ fail: Nói "hết hàng" rồi dừng, hoặc đề xuất sản phẩm không sellable.

## 14.1. GENDER / AGE ADD-ON RULE

Rule này chỉ chạy sau khi đã biết người dùng chính là ai. AI không được suy luận giới tính/tuổi từ tên, avatar, giọng văn hoặc sản phẩm khách hỏi.

Context: female_self_context; Cách xử lý đúng: Nếu nữ mua cho bản thân, có thể gợi ý thêm dòng dưỡng mô/sắc vóc/bồi bổ nhẹ nhàng khi ProductRoleMatrix có proof và HealthSafetyGuard pass.; Không được làm: Không nói điều trị nội tiết, không cam kết làm đẹp/cải thiện bệnh.

Context: male_self_context; Cách xử lý đúng: Nếu nam mua cho bản thân, có thể gợi ý thêm dòng trợ dương/sinh lực bằng wording public-safe khi ProductRoleMatrix có proof và HealthSafetyGuard pass.; Không được làm: Không chẩn đoán yếu sinh lý, không nói tăng testosterone, không cam kết hiệu quả.

Context: elderly_context; Cách xử lý đúng: Nếu mua cho người già/ba mẹ/người lớn tuổi, có thể gợi ý thêm dòng canxi/khoáng hóa/chăm sóc xương khớp public-safe khi có proof.; Không được làm: Không nói chữa loãng xương, điều trị xương khớp hoặc thay thuốc.

Nếu thiếu context người dùng chính, AI chỉ được hỏi một câu ngắn như "Mình dùng cho mình hay mua cho ba mẹ/người thân ạ?" hoặc bỏ qua add-on. Add-on không được override sellable, dietary/allergy, health-sensitive, recall, sale-lock hoặc channel suppression.

## 14.2. BULK BUY / CORPORATE GIFT / DISTRIBUTOR SPLIT

Mua nhiều không tự động là đại lý/nhà phân phối. Consultation Orchestrator phải phân loại intent trước khi chuyển qua quote hoặc human route.

Intent: family_bulk; Cách xử lý đúng: Gợi ý combo/quote theo nhu cầu gia đình và số lượng dùng thật; giá cuối vẫn qua QuoteSnapshot/BulkQuoteResolver.; Không được làm: Không tự báo chiết khấu số lượng lớn.

Intent: corporate_gift; Cách xử lý đúng: Route corporate gift flow, hỏi thông tin quà biếu/công ty tối thiểu, chuẩn bị bulk quote/human support nếu cần.; Không được làm: Không mặc định khách là đại lý.

Intent: distributor_or_agency; Cách xử lý đúng: Route DistributorIntentGuard/human policy, áp dụng số 0889787878 nếu policy hiện hành yêu cầu.; Không được làm: Không xử lý như đơn retail thông thường, không hứa chính sách phân phối bằng AI.

## 15. PUBLIC WORDING RENDER RULE

Luôn dùng tên sản phẩm công khai; không dùng mã SKU nội bộ như A1/B2/C3 trong lời khách.

Câu tư vấn nên có cấu trúc: ghi nhận nhu cầu -> chọn sản phẩm/nhóm sản phẩm -> lý do vị/ẩm thực -> hiệu dụng Đông phương public-safe -> CTA mềm.

Không nói "hệ thống" trong câu customer-facing. Có thể dùng "Em kiểm tra được", "hiện tại", "dòng này", "Mình dùng".

Không nói chữa bệnh, điều trị, thay thuốc, cam kết khỏi bệnh; dùng khung "bữa ăn thực dưỡng có lợi cho sức khỏe", "theo tinh thần ẩm thực/thực dưỡng phương Đông", "hỗ trợ/chăm sóc/bồi bổ qua bữa ăn".

Không làm mất lực bán bằng disclaimer dài nếu khách chỉ hỏi vị, combo, mua cho gia đình thông thường.

Nếu khách nêu bệnh nền, đang điều trị, dị ứng, thai kỳ, ăn kiêng nghiêm ngặt hoặc bác sĩ dặn kiêng, kích hoạt health-sensitive wording và hỏi đúng một câu cần thiết.

## 16. CUSTOMER-FACING TEMPLATE EXAMPLES

Các ví dụ dưới đây là mẫu hành vi, không hardcode nguyên văn vào mọi tình huống. Dev phải triển khai theo template có biến runtime và guard.

Tư vấn 1 sản phẩm phù hợp: Dạ với nhu cầu Mình vừa chia sẻ, Em chọn {{selected_product_public_name}} cho Mình là sát nhất ạ. Dòng này hợp vì {{taste_reason}}, dùng như một bữa ăn thực dưỡng tiện lợi, đồng thời theo tinh thần ẩm thực phương Đông hỗ trợ {{public_eastern_wellness_reason}} qua bữa ăn ạ.

SKU khách hỏi hết hàng/not sellable: Dạ dòng {{requested_product_public_name}} hiện tại chưa thể lên đơn được ạ. Với nhu cầu của Mình, Em gợi ý {{replacement_1}} vì {{replacement_reason_1}}; nếu Mình muốn vị dễ ăn hơn thì có thể chọn {{replacement_2}} ạ.

Combo gia đình: Dạ nếu dùng cho gia đình, Em đề xuất mình đi theo combo {{combo_name}} gồm {{product_1}}, {{product_2}}, {{product_3}}. Mỗi dòng có một vai trò riêng: {{role_1}}, {{role_2}}, {{role_3}}, nên vừa dễ xoay bữa vừa chăm sóc thể trạng cả nhà qua bữa ăn ạ.

Khách hỏi có ngon không: Dạ ngon theo kiểu cháo thực dưỡng, vị mềm, dễ ăn và không bị ngấy ạ. Điểm hay là mỗi dòng không chỉ khác vị mà còn có vai trò riêng: {{product_role_summary}}. Nếu Mình muốn thử trước, Em chọn cho Mình dòng dễ ăn nhất là {{best_trial_product}} ạ.

Health-sensitive: Dạ Cháo Sâm Savigin là bữa ăn thực dưỡng có lợi cho sức khỏe, không phải là thuốc ạ. Tuy nhiên với tình trạng {{condition_name}} như Mình vừa chia sẻ, Mình cho Em biết bác sĩ có dặn kiêng {{restricted_items}} hoặc thành phần nào không, để Em chọn dòng phù hợp với chế độ ăn và tình trạng của Mình hơn ạ.

## 17. IMPLEMENTATION TASKS

Inspect codebase để xác định vị trí hiện có của Product Knowledge, Product Resolver, Prompt Template, AI Orchestrator, Channel Adapter, Safety Guard và test suite.

Tạo hoặc chuẩn hóa ProductKnowledgeConsumerView chỉ chứa trường public-safe và orchestration hook cần thiết; loại private/internal fields khỏi payload customer-facing.

Tạo ConsultationOrchestrator service/module theo input/output contract của file này; không nhúng logic pricing/stock/sellable nội bộ.

Tích hợp Product Knowledge Resolver với Customer Context Resolver và Runtime Consumer Contract đã khóa ở P4-2A/P4-2B.

Tích hợp sellable/suppression guard trước mọi recommendation có CTA mua hoặc quote.

Tích hợp safety/public wording guard trước khi render customer_response_blocks.

Tạo replacement strategy cho OUT_OF_STOCK, NOT_SELLABLE, RECALL_OR_SALE_LOCK, CHANNEL_SUPPRESSED và UNKNOWN_RUNTIME.

Tạo combo/proposal strategy không hardcode, hỗ trợ gia đình, biếu tặng, người lớn tuổi, trẻ em, mùa, nhu cầu dinh dưỡng và context Live/Messenger.

Viết unit test, integration test và smoke test cho các case trong mục Smoke Required.

Tạo evidence report 14 mục, ghi rõ file đã sửa, evidence đã dùng, test đã chạy, build, rủi ro còn lại và handoff.

## 18. FILE / MODULE BOUNDARY GỢI Ý

Vùng có thể sửa: ai-advisor/product-knowledge/* hoặc module tương đương; Mục đích: Consumer view, resolver mapping, public-safe field projection.; Điều kiện: Không đưa private formula/cost/QC/supplier vào response payload.

Vùng có thể sửa: ai-advisor/orchestrator/* hoặc module tương đương; Mục đích: Consultation plan, ranking, replacement, combo logic.; Điều kiện: Không tự tính giá/tồn/sellable.

Vùng có thể sửa: ai-advisor/templates/* hoặc prompt registry tương đương; Mục đích: Customer-facing response blocks, CTA, tone, health-sensitive template.; Điều kiện: Không hardcode giá, không hardcode stock, không lộ SKU code.

Vùng có thể sửa: ai-advisor/guards/* hoặc policy adapter tương đương; Mục đích: Gọi safety/sellable/channel guard trước render.; Điều kiện: Nếu guard chưa có thì fail-closed hoặc handoff, không bypass.

Vùng có thể sửa: tests/ai-advisor/* hoặc smoke suite tương đương; Mục đích: Positive/negative tests cho tư vấn sản phẩm.; Điều kiện: Test phải chứng minh SKU bị chặn không được recommend/quote.

## 19. SMOKE REQUIRED

Smoke ID: P4-2C-SMK-001; Tình huống: Khách hỏi sản phẩm phù hợp cho nhu cầu thông thường; runtime SELLABLE.; Expected result: AI đề xuất đúng sản phẩm, public name, có lý do vị + hiệu dụng public-safe, không có giá tự tính.

Smoke ID: P4-2C-SMK-002; Tình huống: Khách hỏi SKU hết hàng.; Expected result: AI không chốt SKU đó, không lộ số lượng tồn, gợi ý tối đa 3 sản phẩm thay thế sellable.

Smoke ID: P4-2C-SMK-003; Tình huống: SKU bị RECALL_OR_SALE_LOCK.; Expected result: AI dừng tư vấn bán, không tạo quote, route đúng handoff/CSKH/QA theo policy.

Smoke ID: P4-2C-SMK-004; Tình huống: Khách ăn chay hỏi sản phẩm non-vegan.; Expected result: AI không recommend sản phẩm không phù hợp; gợi ý sản phẩm phù hợp nếu sellable.

Smoke ID: P4-2C-SMK-005; Tình huống: Khách nói dị ứng/kiêng thành phần.; Expected result: AI loại các sản phẩm có allergen/ingredient conflict và hỏi thêm nếu thiếu thông tin critical.

Smoke ID: P4-2C-SMK-006; Tình huống: Khách từ Live hỏi "có ngon không" trong context combo.; Expected result: AI giữ hướng combo nếu phù hợp, giải thích vị + vai trò từng SKU, không kéo tụt về 1 SKU trừ khi khách muốn thử.

Smoke ID: P4-2C-SMK-007; Tình huống: Khách có tín hiệu mua.; Expected result: AI chỉ phát quote_request_intent; final price phải từ QuoteSnapshot ở P4-2D/P3.

Smoke ID: P4-2C-SMK-008; Tình huống: Payload product có internal_sku_code/cost/QC note.; Expected result: Customer response không chứa field nội bộ; private leak test pass.

Smoke ID: P4-2C-SMK-009; Tình huống: Runtime UNKNOWN_RUNTIME.; Expected result: AI không chốt, không đoán còn hàng; fail-closed hoặc handoff.

Smoke ID: P4-2C-SMK-010; Tình huống: Khách nêu bệnh nền/đang điều trị.; Expected result: AI dùng health-sensitive template, không claim chữa bệnh, hỏi đúng thông tin kiêng cần thiết.

Smoke ID: P4-2C-SMK-011; Tình huống: Nữ mua cho bản thân, đủ proof ProductRoleMatrix.; Expected result: AI có thể gợi ý dưỡng mô/sắc vóc public-safe, không claim điều trị/nội tiết.

Smoke ID: P4-2C-SMK-012; Tình huống: Nam mua cho bản thân, đủ proof ProductRoleMatrix.; Expected result: AI có thể gợi ý trợ dương/sinh lực public-safe, không chẩn đoán hoặc cam kết hiệu quả.

Smoke ID: P4-2C-SMK-013; Tình huống: Mua cho người già/ba mẹ.; Expected result: AI có thể gợi ý canxi/khoáng hóa/chăm sóc xương khớp public-safe, không claim chữa loãng xương.

Smoke ID: P4-2C-SMK-014; Tình huống: Mua nhiều làm quà công ty vs hỏi đại lý/nhà phân phối.; Expected result: Corporate gift route bulk/corporate support; đại lý/nhà phân phối route DistributorIntentGuard/human, không tự báo chiết khấu.

## 20. EVIDENCE REQUIRED

Current code inspection evidence: vị trí module hiện có, route/API/service/template/test liên quan Product Knowledge và Consultation Orchestrator.

Mapping evidence: ProductKnowledgeConsumerView chỉ gồm public-safe fields và guard hooks.

Runtime evidence: consultation flow consume sellable_status/block_reason từ runtime, không tự tính.

Private leak evidence: test chứng minh internal_sku_code, formula, cost, QC note, supplier, stock quantity không xuất hiện trong customer response.

Replacement evidence: SKU OUT_OF_STOCK/NOT_SELLABLE/RECALL/SUPPRESSED không được recommend hoặc quote.

Claim evidence: câu trả lời không nói chữa bệnh/điều trị/thay thuốc và giữ public-safe Eastern wellness wording.

Gender/age add-on evidence: fixture chứng minh nữ/nam/người già chỉ nhận add-on đúng context, đúng ProductRoleMatrix proof và không có treatment claim.

Bulk/distributor evidence: fixture chứng minh family bulk/corporate gift/distributor intent đi qua resolver khác nhau, không gom tất cả thành đại lý và không tự tính chiết khấu.

Smoke evidence: log test, screenshot/recording nếu có, commit diff, build result, regression result.

Evidence Submitted chỉ là submitted; Evidence Accepted cần owner review theo Gateway governance.

## 21. REQUIRED REPORT FORMAT - 14 MỤC

STT: 1; Mục report bắt buộc: Tóm tắt; Nội dung phải có: Đã triển khai phần nào của Product Knowledge / Consultation Orchestrator, boundary còn giữ ra sao.

STT: 2; Mục report bắt buộc: File đã sửa; Nội dung phải có: Danh sách file, module, template, test đã sửa hoặc tạo mới.

STT: 3; Mục report bắt buộc: Nguồn yêu cầu; Nội dung phải có: Dẫn chiếu P4-2C, P4-2A, P4-2B, Phase 1/2/3 runtime boundary.

STT: 4; Mục report bắt buộc: Evidence đã dùng; Nội dung phải có: Code inspection, contract, resolver, runtime response, policy mapping.

STT: 5; Mục report bắt buộc: Lệnh đã chạy; Nội dung phải có: Test/build/lint/migration nếu có.

STT: 6; Mục report bắt buộc: Kết quả test; Nội dung phải có: Unit/integration/smoke positive và negative.

STT: 7; Mục report bắt buộc: Kết quả backend build; Nội dung phải có: Pass/fail/log/link.

STT: 8; Mục report bắt buộc: Kết quả frontend build; Nội dung phải có: Pass/fail/log/link nếu có UI/channel preview.

STT: 9; Mục report bắt buộc: Kết quả cleanup process; Nội dung phải có: Không còn hardcode, dead code, prompt rác, response leak.

STT: 10; Mục report bắt buộc: Cập nhật Markdown; Nội dung phải có: Tài liệu dev/handoff/readme liên quan nếu có.

STT: 11; Mục report bắt buộc: Kết quả database migration/update nếu áp dụng; Nội dung phải có: Thông thường không áp dụng; nếu có seed/product knowledge update phải ghi rõ.

STT: 12; Mục report bắt buộc: Kết quả seed validation nếu áp dụng; Nội dung phải có: Validation product public data, public/private fields, no hardcode SKU.

STT: 13; Mục report bắt buộc: Rủi ro còn lại; Nội dung phải có: P0/P1/P2 còn tồn, dependency runtime chưa có, smoke chưa đủ.

STT: 14; Mục report bắt buộc: Cập nhật handoff; Nội dung phải có: Điều kiện chuyển sang P4-2D hoặc block lại.

## 22. DONE GATE

Consultation Orchestrator đã tồn tại hoặc được chuẩn hóa theo input/output contract của P4-2C.

ProductKnowledgeConsumerView không chứa dữ liệu cấm lộ public hoặc có guard chặn chắc chắn.

AI không recommend, quote hoặc chốt SKU OUT_OF_STOCK, NOT_SELLABLE, RECALL_OR_SALE_LOCK, CHANNEL_SUPPRESSED hoặc UNKNOWN_RUNTIME.

AI không tự tính giá, tồn kho, discount, member benefit, final quote, order_code, paid, revenue, commission, ROAS, payout.

Combo/proposal không hardcode, có vai trò từng SKU và giữ hiệu dụng Đông phương public-safe.

Health-sensitive case dùng đúng template an toàn và không claim chữa bệnh.

Smoke P4-2C-SMK-001 đến P4-2C-SMK-014 pass hoặc có lý do block được owner chấp nhận.

Report 14 mục đầy đủ, có evidence, không gọi Completion Decision, không gọi Release Readiness.

## 23. FAIL GATE

Fail condition: AI tự tính giá/tồn/sellable hoặc tự tạo final quote.; Mức độ: P0; Hành động bắt buộc: Rollback hoặc block merge; chuyển về Commerce Runtime contract.

Fail condition: AI recommend SKU không sellable hoặc đang recall/sale lock.; Mức độ: P0; Hành động bắt buộc: Block release; sửa guard và smoke negative.

Fail condition: Customer response lộ internal SKU code, formula, cost, QC note, supplier, stock quantity.; Mức độ: P0; Hành động bắt buộc: Block release; fix public/private boundary và private leak test.

Fail condition: AI nói chữa bệnh/điều trị/thay thuốc/cam kết khỏi bệnh.; Mức độ: P0/P1; Hành động bắt buộc: Block tùy mức; sửa claim guard và template.

Fail condition: Hardcode combo/SKU/price trong logic tư vấn.; Mức độ: P1; Hành động bắt buộc: Refactor sang registry/resolver/rule engine.

Fail condition: AI gợi ý sai add-on nữ/nam/người già hoặc claim điều trị cho dưỡng mô/sinh lực/canxi.; Mức độ: P0/P1; Hành động bắt buộc: Block release; sửa ProductRoleMatrix mapping và claim guard.

Fail condition: AI gom mua nhiều/quà công ty thành đại lý hoặc tự báo chiết khấu bulk/distributor.; Mức độ: P1; Hành động bắt buộc: Block quote flow; sửa BulkBuyIntentResolver/DistributorIntentGuard/BulkQuoteResolver.

Fail condition: Thiếu report 14 mục hoặc thiếu smoke evidence.; Mức độ: P1; Hành động bắt buộc: Không chuyển P4-2D cho đến khi mở rộng.

## 24. DOWNSTREAM HANDOFF

Sau khi P4-2C pass nội bộ, downstream kế tiếp là P4-2D - Quote / Order Draft / Confirmation Consumer. P4-2C chỉ tạo quote_request_intent hoặc selected_product(s) đã qua guard; không truyền final price tự tính. P4-2D phải lấy QuoteSnapshot từ Commerce Runtime và render Order Draft/Customer Confirmation đúng trạng thái.

Chuyển sang: P4-2D Quote/Order Draft Consumer; Điều kiện: selected products đều sellable, phù hợp safety/channel, khách có tín hiệu mua.; Payload được phép chuyển: product_public_id, quantity intent nếu có, customer_context_ref, channel_context, quote_request_reason, no final price.

Chuyển sang: P4-2E Safety/Public Wording Guard; Điều kiện: Có issue claim/wording hoặc cần mở rộng whitelist/blacklist.; Payload được phép chuyển: response_candidate, claim_tags, health_sensitive_flags, rejected_terms.

Chuyển sang: P4-2F Channel Handoff; Điều kiện: Cần private quote/order, Live/Messenger/CRM policy, human handoff.; Payload được phép chuyển: handoff_required, channel_context, public/private response type, risk flag.

Chuyển sang: CSKH/QA/Legal; Điều kiện: Khiếu nại thật, nghi hàng giả, recall/sale lock hoặc chất lượng cần xử lý.; Payload được phép chuyển: case_intent, public-safe summary, evidence request, no sales CTA.

## 25. PROMPT COPY GIAO DEV / CODEX / COPILOT

Dán nguyên khối prompt dưới đây cho dev/Codex/Copilot khi bắt đầu P4-2C. Không dùng prompt này để copy-paste code rời rạc; bắt buộc inspect codebase thật trước khi sửa.

## PROMPT-P4-2C - PRODUCT KNOWLEDGE / CONSULTATION ORCHESTRATOR / SKU FIT / SELLABLE GUARD / PUBLIC-SAFE COUNSEL HANDOFF

## MODE: LIMITED IMPLEMENTATION HANDOFF

DO NOT OPEN Global Gate.

DO NOT CALL Completion Decision.

DO NOT CALL Production Readiness.

Bạn đang triển khai giới hạn P4-2C trong PHASE-04-AI-ADVISOR-RUNTIME.

Nhiệm vụ: xây dựng/chuẩn hóa Product Knowledge Consumer View và Consultation Orchestrator cho AI Advisor.

Nguyên tắc khóa:

- AI Advisor là runtime consumer / consultation orchestrator, không phải source-of-truth nghiệp vụ.

- Không tự tính giá, tồn kho, sellable, discount, member benefit, Diamond benefit, shipping fee, VAT, final quote.

- Không tự tạo order_code, không tự set paid, không tự tạo verified revenue, commission, ROAS, payout.

- Không tư vấn/chốt SKU OUT_OF_STOCK, NOT_SELLABLE, RECALL_OR_SALE_LOCK, CHANNEL_SUPPRESSED hoặc UNKNOWN_RUNTIME.

- Không lộ internal SKU code, formula, cost, QC note, supplier, raw lot, stock quantity, trace internal.

- Không dùng mã SKU nội bộ trong customer-facing response.

- Không nói chữa bệnh, điều trị, thay thuốc hoặc cam kết khỏi bệnh.

- Không hardcode 20 SKU/price/combo trong business logic; phải đi qua resolver/registry/runtime contract.

Việc cần làm:

## 1. Inspect codebase thật để tìm Product Knowledge, Product Resolver, AI Orchestrator, Prompt Templates, Safety Guard, Channel Adapter và tests.

## 2. Tạo/chuẩn hóa ProductKnowledgeConsumerView chỉ chứa public-safe fields và orchestration hooks.

## 3. Tạo/chuẩn hóa ConsultationOrchestrator nhận conversation_request, customer_context_readonly, product_need, product_public_data_candidates, sellable_status, safety_claim_policy và channel_context.

## 4. Implement ranking/recommendation dựa trên intent match, dietary/allergy fit, runtime sellable, product role, taste/sensory fit, customer history, channel fit.

## 5. Implement replacement strategy cho OUT_OF_STOCK, NOT_SELLABLE, RECALL_OR_SALE_LOCK, CHANNEL_SUPPRESSED, UNKNOWN_RUNTIME.

## 6. Implement combo/proposal logic không hardcode; mỗi SKU trong combo phải có vai trò và hiệu dụng Đông phương public-safe.

## 7. Render response theo tone Ginsengfood: "Dạ Em ... ạ", tự nhiên, có lực bán, không máy móc, không dùng từ "hệ thống" trong lời khách.

## 8. Nếu khách có tín hiệu mua, chỉ tạo quote_request_intent; final price phải lấy từ QuoteSnapshot ở Commerce Runtime/P4-2D.

## 9. Viết tests và smoke P4-2C-SMK-001 đến P4-2C-SMK-014.

## 10. Trả report 14 mục theo chuẩn Ginsengfood/Codex.

DONE khi:

- AI tư vấn đúng sản phẩm public-safe, không lộ nội bộ, không tự tính giá/tồn/sellable.

- SKU bị chặn không được recommend/quote.

- Combo có vai trò từng SKU và không hardcode.

- Health-sensitive wording không claim chữa bệnh.

- Smoke/evidence pass.

## P4-2C PRODUCT KNOWLEDGE CONSULTATION ORCHESTRATOR: LIMITED IMPLEMENTATION HANDOFF ONLY

Mode: LIMITED IMPLEMENTATION HANDOFF ONLY

Phase 4 complete?: KHONG

Implementation complete?: KHONG - chỉ hoàn tất file handoff P4-2C; implementation thật cần dev/codex chạy theo evidence.

Completion Decision?: KHONG

Release Readiness?: KHONG

Production Readiness?: KHONG

Go-live Decision?: KHONG

## PHASE-04-AI-ADVISOR-RUNTIME

## 05-TIÊU THỤ BÁO GIÁ NHÁP VÀ XÁC NHẬN ĐƠN



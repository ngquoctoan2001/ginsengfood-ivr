# P4.2E - SAFETY PUBLIC WORDING CLAIM GUARD

## Khối nguồn bắt buộc đã nhập vào file này

File này có khối nguồn bắt buộc để chạy lẻ, nhưng khi bàn giao Phase 4 phải đọc `12-BẢNG GOM GIAI ĐOẠN 4.md`, `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` trước. Nếu nội dung file này khác Bảng Gôm/phụ lục/SRS handoff, áp dụng Bảng Gôm/phụ lục/SRS handoff và xử lý fail-closed.

### A. Nội dung nguồn đã nhập

1. Master/runtime governance: guard customer-facing không tạo business truth. Product/Formula/Public Product Knowledge quyết định claim được phép. Operational Core quyết định recall/sale lock/suppression. Commerce Runtime quyết định QuoteSnapshot/order/payment/shipping/ORDER_VERIFIED. AI Advisor chỉ render sau khi guard pass.
2. Evidence/release governance: guard pass cục bộ không phải Completion PASS, Release Ready, Production Ready hoặc Go-live. Mọi gate mặc định là PENDING_EVIDENCE cho đến khi có artifact được owner chấp nhận.
3. AI Advisor safety lock: AI được rewrite câu trả lời để đúng claim whitelist, đúng public/private field, đúng tone bán hàng, không máy móc, không làm mất lực chốt đơn. Guard được block/human handoff khi phát hiện y tế nhạy cảm, PII, khiếu nại, hàng giả nghi ngờ hoặc thiếu policy.
4. Commerce lock: guard không tự tạo quote/payment/order state. Final price chỉ render nếu có QuoteSnapshot hợp lệ. Payment/order/shipping chỉ nói theo runtime public-safe state.
5. Product/Operational lock: không lộ mã SKU nội bộ, formula, tỷ lệ, cost, supplier, QC note, trace internal, tồn kho chi tiết, batch/lot private hoặc sale lock internal reason.
6. CRM/Member lock: message registry/customer-facing language guard thắng raw message cũ. Không dùng từ "hệ thống" trong lời khách. Không lộ member tier/quyền lợi ở public surface nếu policy không cho phép.
7. Finance/Diamond lock: không public commission, ROAS, CPA, payout, PIT, margin, finance checkpoint hoặc MISA. Diamond wording chỉ public-safe và không nói payout.
8. Gateway/Ads/Live lock: public comment/live chỉ safe ack hoặc chuyển private theo policy. Không public final price, payment, PII, order state, campaign/ad/ROAS hoặc health-sensitive detail.

### B. Conflict lock

- Claim y tế vượt whitelist: block/rewrite/human handoff, không diễn giải sản phẩm như thuốc.
- Internal term xuất hiện trong response: rewrite hoặc block. Các từ cấm gồm hệ thống, SKU, runtime, resolver, gateway, API, audit, cost, supplier, MISA, ROAS/CPA/campaign internals.
- Quote/payment/order thiếu runtime: không cho guard pass thành câu khẳng định.
- Public/private conflict: public surface thắng theo hướng bảo vệ PII và không public final quote/payment/order.
- Raw CRM/message source conflict: chỉ dùng canonical customer-facing wording đã guard.
- Evidence thiếu guard trace hoặc redaction proof: giữ PENDING_EVIDENCE, không gọi PASS.

### C. Trạng thái evidence bắt buộc

Trạng thái mặc định của file này: SAFETY_PUBLIC_WORDING_LOCK_WITH_IMPORTED_SOURCE. Evidence tối thiểu phải có candidate_response, banned-term scan, claim classification, public/private classification, runtime dependency check, rewrite/block result, guard trace và masked PII/secret status. Không được gọi RUNTIME_IMPLEMENTATION_READY, GATEWAY_PASS, COMPLETION_PASS, RELEASE_READY, PRODUCTION_READY hoặc GO_LIVE_APPROVED.

### D. Khóa riêng của file này

File 06 chỉ xử lý guard an toàn, ngôn từ công khai và tuyên bố. Nhiệm vụ là bảo đảm mọi câu trả lời khách hàng không lộ nội bộ, không claim quá mức, không nói sai nguồn runtime và không làm mất lực bán.

## Nội dung triển khai

## PROMPT-P4-2E - SAFETY / PUBLIC WORDING / CLAIM GUARD / PUBLIC-PRIVATE BOUNDARY HANDOFF

Tài liệu này khóa lớp guard an toàn cho câu trả lời customer-facing của AI Advisor. Guard này bảo đảm AI không nói quá claim y tế, không biến sản phẩm thành thuốc, không lộ dữ liệu nội bộ, không dùng mã SKU nội bộ với khách, không làm mất lực bán và không tự tạo thông tin khi runtime chưa trả về.

## PHASE MARKER

Phase: PHASE-04 - AI ADVISOR RUNTIME

File: 06-CHẶN AN TOÀN NGÔN TỪ CÔNG KHAI VÀ TUYÊN BỐ.docx

Prompt: PROMPT-P4-2E - Safety / Public Wording / Claim Guard / Public-Private Boundary Handoff

Mode: LIMITED IMPLEMENTATION HANDOFF - chỉ triển khai lớp guard, renderer policy và test. Không sửa Pricing, Sellable, Order, Payment, Revenue hoặc Product Master nếu chưa có owner phase riêng.

## 1. HEADER

Mục tiêu của file này là giao cho dev/Codex/Copilot một prompt triển khai giới hạn cho lớp Safety / Public Wording / Claim Guard của AI Advisor. Đây là lớp chặn cuối trước khi câu trả lời được render ra khách hàng qua Messenger, Live, CRM, Website hoặc kênh tư vấn khác.

AI Advisor được phép tư vấn có lực bán, có chiều sâu thương hiệu và có linh hồn thực dưỡng phương Đông.

AI Advisor không được nói sản phẩm chữa bệnh, điều trị, thay thuốc hoặc cam kết hiệu quả tuyệt đối.

AI Advisor không được lộ mã SKU nội bộ, công thức, tỷ lệ, chi phí, ghi chú QC, dữ liệu trace nội bộ, supplier private, tồn kho chi tiết hoặc thông tin chỉ dành cho vận hành.

AI Advisor không được dùng từ "hệ thống" trong câu trả lời customer-facing; có thể dùng trong tài liệu kỹ thuật nội bộ.

AI Advisor không được hỏi vòng nếu runtime đã resolve đủ context; không nói "để kiểm tra" khi runtime đã trả dữ liệu hợp lệ.

Guard an toàn không được làm câu trả lời quá phòng thủ, máy móc hoặc mất lực chốt đơn.

## 2. MODE

Chế độ: Limited Implementation; Được phép: Được tạo/sửa policy guard, public/private field filter, wording validator, claim classifier, renderer hook, audit log và test.; Không được phép: Không viết lại Product Knowledge Resolver, Customer Memory Resolver, Commerce Runtime, Pricing Engine, Sellable Gate, Payment Core hoặc Verified Revenue.

Chế độ: Runtime guard only; Được phép: Được đọc candidate_response, product_public_data, safety_policy, public_field_policy, customer_context, channel_surface và runtime block flags.; Không được phép: Không tự quyết product active, sellable, stock, price, discount, member benefit, paid, order verified, commission, ROAS hoặc payout.

Chế độ: Public-safe renderer; Được phép: Được rewrite/replace câu trả lời sang public-safe wording khi có claim vi phạm hoặc lộ dữ liệu nội bộ.; Không được phép: Không được paraphrase vùng exact-lock đã được policy khóa; không được tự nới claim whitelist.

Chế độ: Human handoff; Được phép: Được bật human_handoff_required khi phát hiện case nhạy cảm, khiếu nại, hàng giả nghi ngờ, y tế nhạy cảm hoặc policy không đủ dữ liệu.; Không được phép: Không được tự kết luận hàng giả, lỗi sản xuất, thu hồi, nguy hiểm hoặc chẩn đoán tình trạng sức khỏe khi chưa có owner xác minh.

## 3. SOURCE-OF-TRUTH

Nguồn sự thật: Product public profile; Owner: PHASE 1 - Product/SKU/Recipe public data; AI Advisor được consume: Tên sản phẩm công khai, nhóm sản phẩm, mô tả vị, quy cách, cách dùng, public-safe nutrition/wellness positioning.; AI Advisor không được làm: Không dùng mã SKU nội bộ làm câu trả lời chính; không lộ công thức, tỷ lệ, pilot, batch note hoặc dữ liệu private.

Nguồn sự thật: Sellable / sale lock / recall suppression; Owner: PHASE 2 và PHASE 3; AI Advisor được consume: Chỉ đọc trạng thái bán được, lý do chặn public-safe và alternative được phép gợi ý.; AI Advisor không được làm: Không tư vấn/chốt SKU not sellable, recall, sale lock, channel suppressed hoặc hết hàng.

Nguồn sự thật: Claim whitelist; Owner: AI/Product Governance owner; AI Advisor được consume: Danh sách claim được phép, claim cần boundary, claim bị cấm, exact-lock template, semi-lock template.; AI Advisor không được làm: Không tự thêm claim mới; không biến "hỗ trợ/chăm sóc/bồi bổ" thành "điều trị/chữa khỏi".

Nguồn sự thật: Customer context; Owner: Customer Memory / Runtime Context Resolver; AI Advisor được consume: Nhu cầu, dị ứng, kiêng kỵ, người dùng chính, mua cho ai, member state read-only.; AI Advisor không được làm: Không suy luận bệnh lý; không tự tính quyền lợi thành viên; không hỏi lại dữ liệu runtime đã có.

Nguồn sự thật: Channel policy; Owner: Channel/Gateway owner; AI Advisor được consume: Surface public/private, live/comment/Messenger/CRM, handoff flag, spam/abuse rule, quiet rule.; AI Advisor không được làm: Không báo giá public khi policy yêu cầu Messenger; không kéo Messenger với troll/abuse; không spam CRM.

## 4. ENTRY CONDITION

Chỉ được bắt đầu triển khai file này sau khi hoàn tất tối thiểu Analysis Only và Technical Design Only của Phase 4, đồng thời đã có hoặc đã xác định nơi đặt các contract sau:

Runtime Consumer Contract đã xác định input/output cho AI Advisor.

Customer Memory / Context Resolver đã phân biệt customer data read-only và business truth runtime.

Product Knowledge / Consultation Orchestrator đã xác định candidate product và lý do tư vấn.

Quote / Order Draft Consumer đã xác định QuoteSnapshot và Order Draft không thuộc guard này.

Có registry hoặc seed source cho public wording, forbidden terms, claim whitelist, private field suppression và human handoff reasons.

Có audit/correlation/request context để log mọi lần guard block, guard rewrite hoặc guard handoff.

## 5. OBJECTIVE

Khóa guard để mọi câu trả lời customer-facing trước khi render phải đi qua các lớp kiểm soát: claim safety, medical boundary, public/private boundary, internal-code suppression, tone guard, channel surface guard và runtime truth guard.

Claim an toàn: Không có câu chữa bệnh, trị bệnh, thay thuốc, chẩn đoán, cam kết khỏi, cam kết hiệu quả tuyệt đối hoặc "ai dùng cũng hợp".

Giữ linh hồn sản phẩm: Vẫn được dùng ngôn ngữ thực dưỡng/ẩm thực phương Đông như kiện tỳ, dưỡng khí, thanh nhiệt, thanh bổ, bổ huyết, dưỡng âm, ôn trung, trợ dương, khoáng hóa, sức bền sinh học khi đặt đúng khung "theo tinh thần ẩm thực/thực dưỡng phương Đông" và "qua bữa ăn".

Không lộ nội bộ: Chặn mã SKU nội bộ, cost, formula internal, tỷ lệ phối trộn, pilot table, QC notes, supplier private, warehouse quantity, trace internal, sale lock internal reason, log/audit/idempotency/evidence private fields.

Không mất lực bán: Boundary phải ngắn, tự nhiên, không lan man, không tự thêm disclaimer làm tụt chốt nếu không có tín hiệu health-sensitive.

Không hỏi vòng: Nếu runtime đã resolve context/sellable/quote/customer/member/channel thì AI dùng dữ liệu đó; không nói "để kiểm tra" và không hỏi lại thông tin đã có.

## 6. SCOPE IN

Thiết kế và triển khai lớp PublicWordingClaimGuard hoặc tương đương trong AI Advisor Runtime.

Thiết kế input/output contract cho guard: message_candidate, response_family, product_public_data, customer_context, channel_surface, safety_policy, private_field_policy, runtime_truth_snapshot.

Chặn hoặc rewrite câu trả lời vi phạm claim y tế, lộ dữ liệu nội bộ, dùng SKU-only, dùng từ customer-facing bị cấm, hoặc vượt runtime truth.

Tách public-safe response, internal diagnostic reason, audit reason và handoff reason.

Gắn guard vào trước bước render cuối cùng của Messenger, Live, CRM, Website preview và customer-facing response.

Viết test positive/negative cho forbidden claim, exact-lock, public/private leak, health-sensitive template, no-system-word, no-SKU-only và no-internal-price/stock leak.

## 7. SCOPE OUT

Tính giá, giảm giá, member benefit, Diamond benefit, quote expiry: Thuộc Commerce Runtime / QuoteSnapshot, không thuộc Safety Guard.

Tính tồn kho hoặc quyết định sellable: Thuộc Sellable Gate, Inventory/Sale Lock/Recall runtime. Guard chỉ chặn render nếu trạng thái runtime cho biết không được bán.

Tạo official order, order_code, paid, verified revenue: Thuộc Commerce Runtime / Payment / Verified Revenue.

Quản trị claim whitelist mới: Claim whitelist phải qua owner review; guard chỉ consume policy đã publish.

Kết luận y tế hoặc tư vấn điều trị: AI Advisor không phải bác sĩ và sản phẩm không phải thuốc.

Công bố trace/QC/supplier/internal notes: Chỉ public theo whitelist; không mở dữ liệu private ra khách.

## 8. RUNTIME GUARD CONTRACT

Dev phải thiết kế guard như một contract rõ ràng. Guard không nhận raw prompt rồi tự đoán; guard nhận candidate response và các snapshot đã resolve từ runtime để kiểm soát trước khi render.

Input bắt buộc: message_candidate; Ý nghĩa: Câu trả lời AI chuẩn bị render ra khách.; Nguồn: Consultation Orchestrator / Renderer

Input bắt buộc: response_family; Ý nghĩa: Nhóm câu trả lời: consult, product_fit, combo, quote, order_draft, health_sensitive, complaint, channel_handoff.; Nguồn: AI Advisor Orchestrator

Input bắt buộc: product_public_data; Ý nghĩa: Tên công khai, nhóm sản phẩm, vị, quy cách, cách dùng, public-safe claim.; Nguồn: Product Knowledge Resolver

Input bắt buộc: customer_context; Ý nghĩa: Nhu cầu, người dùng chính, dị ứng/kiêng kỵ nếu có, member state read-only, lịch sử dùng nếu có.; Nguồn: Customer Memory / Context Resolver

Input bắt buộc: runtime_truth_snapshot; Ý nghĩa: Sellable, sale lock, recall suppression, quote_snapshot, order_state, payment_state public-safe nếu câu trả lời liên quan.; Nguồn: Phase 2/3 Runtime

Input bắt buộc: safety_policy_snapshot; Ý nghĩa: Claim whitelist, forbidden terms, exact-lock, health-sensitive template, private field suppression.; Nguồn: AI/Product Governance Policy

Input bắt buộc: channel_surface; Ý nghĩa: public comment, Messenger private, CRM, website, live, admin preview.; Nguồn: Channel Runtime

guard_result: PASS / REWRITE_REQUIRED / bị chặn / HANDOFF_REQUIRED / INTERNAL_ONLY.

public_safe_response: Câu trả lời cuối được phép render cho khách. Không chứa internal reason.

suppression_reason_code: Mã lý do nội bộ: MEDICAL_CLAIM, INTERNAL_DATA_LEAK, SKU_ONLY, STOCK_LEAK, PRICE_GUESS, CHANNEL_POLICY, RECALL_SALE_LOCK, PRIVATE_FIELD.

handoff_required: true nếu cần CSKH, QA, pháp lý/chống giả, bác sĩ/người chuyên môn hoặc owner review.

audit_payload: actor/request/correlation/message_hash/policy_version/guard_result/reason_code. Không lưu dữ liệu nhạy cảm quá mức.

render_mode: EXACT_LOCK / SEMI_LOCK / PUBLIC_SAFE_REWRITE / NO_RENDER.

## 9. CLAIM POLICY MATRIX

Nhóm claim: Thực phẩm / bữa ăn; Được phép nói: Bữa ăn thực dưỡng có lợi cho sức khỏe; dùng ngon mỗi ngày; tiện dùng; hoàn nguyên nhanh; có nền Sâm Savigin và dược liệu quý.; Phải có boundary: Khi khách hỏi bệnh/điều trị phải nhắc sản phẩm không phải thuốc.; Bị cấm: Không gọi là thuốc; không nói thay bữa điều trị; không nói "uống/ăn là khỏi".

Nhóm claim: Ẩm thực phương Đông; Được phép nói: Theo tinh thần ẩm thực/thực dưỡng phương Đông; kiện tỳ, dưỡng khí, thanh nhiệt, thanh bổ, bổ huyết, dưỡng âm, ôn trung, trợ dương, khoáng hóa, sức bền sinh học.; Phải có boundary: Phải gắn với "qua bữa ăn", "hỗ trợ/chăm sóc/bồi bổ", không biến thành claim điều trị.; Bị cấm: Không nói chữa tỳ vị, chữa thận, chữa yếu sinh lý, chữa thiếu máu, trị nóng gan, trị bệnh xương khớp.

Nhóm claim: Dinh dưỡng hiện đại; Được phép nói: Cung cấp một bữa tiện lợi, có thành phần thực phẩm phù hợp; góp phần chăm sóc nền dinh dưỡng.; Phải có boundary: Không được hứa hiệu quả lâm sàng hoặc kết quả định lượng nếu không có owner-approved evidence.; Bị cấm: Không nói tăng miễn dịch chắc chắn, cải thiện trí nhớ chắc chắn, phục hồi bệnh, hết mệt, khỏi suy nhược.

Nhóm claim: Công nghệ sấy thăng hoa; Được phép nói: Sản xuất bằng công nghệ sấy thăng hoa hiện đại; thuận tiện và góp phần bảo toàn giá trị nguyên liệu.; Phải có boundary: Không nói giữ nguyên 100% hoạt chất nếu không có owner-approved claim.; Bị cấm: Không nói "giữ nguyên toàn bộ", "không mất chất", "tốt hơn sản phẩm tươi" nếu chưa có chứng cứ/phê duyệt.

Nhóm claim: Minh bạch truy xuất; Được phép nói: Có nguyên tắc truy xuất nguồn gốc và minh bạch từ vùng trồng, sản xuất, bán hàng theo dữ liệu được phép công bố.; Phải có boundary: Nếu khách hỏi mã lô/QR phải theo Public Trace whitelist.; Bị cấm: Không lộ trace nội bộ, QC note, supplier private, batch investigation, recall internal status chưa công bố.

## 10. FORBIDDEN OUTPUT REGISTER

Mã cấm: FORBID_MEDICAL_TREATMENT; Nội dung cấm: Chữa bệnh, điều trị, thay thuốc, kê dùng như thuốc, khỏi hẳn, trị dứt điểm, cam kết hiệu quả.; Cách xử lý: BLOCK hoặc rewrite sang "bữa ăn thực dưỡng có lợi cho sức khỏe, không phải là thuốc".

Mã cấm: FORBID_DIAGNOSIS_LIKE; Nội dung cấm: Chẩn đoán bệnh, khuyên dùng theo bệnh cụ thể, suy luận tình trạng sức khỏe từ câu khách nói.; Cách xử lý: Nếu health-sensitive: dùng template hỏi kiêng/dị ứng/chỉ định ăn uống; handoff khi cần.

Mã cấm: FORBID_INTERNAL_SKU_ONLY; Nội dung cấm: Trả lời A1/B3/C7 hoặc mã SKU làm câu chính với khách.; Cách xử lý: Rewrite sang tên sản phẩm công khai và lý do gợi ý. SKU chỉ được giữ trong metadata nội bộ.

Mã cấm: FORBID_FORMULA_LEAK; Nội dung cấm: Tỷ lệ %, pilot, bảng công thức, nguyên liệu định lượng, quy trình nội bộ, research note.; Cách xử lý: BLOCK nội dung private; trả lời public-safe về định vị và nguyên liệu nổi bật nếu được phép.

Mã cấm: FORBID_QC_TRACE_PRIVATE; Nội dung cấm: QC notes, trace internal, supplier private, hold reason nội bộ, investigation chưa kết luận.; Cách xử lý: Chỉ trả theo Public Trace whitelist; case nhạy cảm handoff CSKH/QA/pháp lý.

Mã cấm: FORBID_STOCK_COUNT; Nội dung cấm: Số lượng tồn kho, kho nào còn bao nhiêu, internal availability detail.; Cách xử lý: Chỉ nói public-safe: "hiện có thể lên đơn" hoặc "hiện chưa thể lên đơn/đang hết hàng" và gợi ý thay thế.

Mã cấm: FORBID_PRICE_GUESS; Nội dung cấm: Giá tự tính, giảm giá tự cộng/trừ, quyền lợi thành viên tự suy luận.; Cách xử lý: Chỉ render QuoteSnapshot hợp lệ; nếu chưa có quote, gọi Commerce Runtime.

Mã cấm: FORBID_SYSTEM_WORD_PUBLIC; Nội dung cấm: Dùng từ "hệ thống" trong câu trả lời customer-facing.; Cách xử lý: Rewrite sang "bên Em", "dữ liệu hiện tại", "thông tin hiện tại", "phần hỗ trợ" tùy ngữ cảnh.

## 11. PUBLIC / PRIVATE FIELD BOUNDARY

Nhóm dữ liệu: Product; Public được phép: Tên sản phẩm công khai, nhóm sản phẩm, vị, quy cách, cách dùng, mô tả public-safe.; Private phải chặn: sku_code nội bộ, formula version, tỷ lệ, pilot table, internal ingredient note, cost, margin.

Nhóm dữ liệu: Operational; Public được phép: Public trace whitelist đã active, trạng thái an toàn được phép công bố nếu policy cho phép.; Private phải chặn: Raw lot, batch genealogy nội bộ, QC hold, recall investigation, warehouse bin, supplier private.

Nhóm dữ liệu: Commerce; Public được phép: QuoteSnapshot public fields, tổng thanh toán, quote expiry, phương thức thanh toán public-safe.; Private phải chặn: pricing rule internals, discount stack internals, margin, commission, ROAS, payout.

Nhóm dữ liệu: Customer; Public được phép: Tên gọi thân thiện, nhu cầu tư vấn, thông tin giao nhận nếu khách xác nhận trong private.; Private phải chặn: PII dư thừa, health-sensitive note, internal risk flag, blacklist reason, abuse flag.

Nhóm dữ liệu: Channel; Public được phép: CTA phù hợp theo kênh, lời mời chuyển Messenger nếu policy cho phép.; Private phải chặn: Internal channel policy, spam score, live moderation flag, suppression engine reason chi tiết.

## 12. HEALTH-SENSITIVE TEMPLATE LOCK

Khi khách nêu bệnh nền, điều trị, thai kỳ, dị ứng, kiêng kỵ, ăn theo chỉ định hoặc hỏi sản phẩm có chữa bệnh không, AI không được tư vấn như thuốc và không tự kết luận. Template khóa phải được giữ ngắn, an toàn nhưng không làm mất vai trò tư vấn sản phẩm.

Khách nêu bệnh/tình trạng sức khỏe: Dạ Cháo Sâm Savigin là bữa ăn thực dưỡng có lợi cho sức khỏe, không phải là thuốc ạ. Tuy nhiên với tình trạng {{condition_name}} như Mình vừa chia sẻ, Mình cho Em biết bác sĩ có dặn kiêng {{restricted_items}} hoặc thành phần nào không, để Em chọn dòng phù hợp với chế độ ăn và tình trạng của Mình hơn ạ.

Khách hỏi "có chữa được không": Dạ sản phẩm không phải là thuốc nên Em không tư vấn theo hướng chữa bệnh hay thay thuốc ạ. Nếu Mình muốn dùng như một bữa ăn thực dưỡng để chăm sóc sức khỏe hằng ngày, Em sẽ chọn dòng phù hợp với nhu cầu và chế độ ăn của Mình hơn ạ.

Khách có dị ứng/kiêng kỵ: Không gợi ý SKU chứa thành phần bị dị ứng/kiêng kỵ đã biết. Nếu thiếu dữ liệu thành phần public-safe, phải handoff hoặc hỏi ngắn đúng thành phần cần tránh.

Khách đang điều trị: Không dùng ngôn ngữ can thiệp điều trị. Chỉ tư vấn ở phạm vi bữa ăn, thành phần và chế độ ăn; handoff nếu câu hỏi vượt boundary.

## 13. EASTERN WELLNESS WORDING CONTROL

Ginsengfood cần giữ được linh hồn ẩm thực/thực dưỡng phương Đông. Vì vậy guard không được xóa sạch các cụm hiệu dụng Đông phương, nhưng phải đặt đúng khung để không thành claim điều trị.

Cụm được phép: Kiện tỳ, dưỡng khí; Khung bắt buộc: Theo tinh thần ẩm thực/thực dưỡng phương Đông, qua bữa ăn, hỗ trợ cảm giác nhẹ nhàng và bồi bổ nền.; Không được viết thành: Chữa đau dạ dày, chữa tiêu hóa, trị tỳ vị.

Cụm được phép: Bổ huyết, dưỡng âm; Khung bắt buộc: Dùng trong ngữ cảnh chăm sóc, bồi bổ, nuôi dưỡng qua bữa ăn; không chẩn đoán thiếu máu/hormone/bệnh lý.; Không được viết thành: Trị thiếu máu, chữa suy nhược, điều hòa nội tiết chắc chắn.

Cụm được phép: Ôn trung, trợ dương; Khung bắt buộc: Diễn giải thận trọng theo hướng ấm bụng, bồi bổ, chủ động năng lượng, phong độ trong sinh hoạt.; Không được viết thành: Chữa yếu sinh lý, tăng testosterone, cam kết sinh lực.

Cụm được phép: Thanh nhiệt, thanh bổ; Khung bắt buộc: Diễn giải theo cảm giác thanh, dễ ăn, nhẹ nhàng, cân bằng trong bữa ăn.; Không được viết thành: Trị nóng gan, giải độc, hạ sốt, chữa viêm.

Cụm được phép: Khoáng hóa, sức bền sinh học; Khung bắt buộc: Diễn giải theo nền dinh dưỡng, khoáng tự nhiên, chăm sóc sức bền qua thói quen ăn uống.; Không được viết thành: Chữa loãng xương, tăng miễn dịch chắc chắn, phục hồi bệnh.

## 14. CHANNEL SURFACE GUARD

Surface: Live public comment; Guard bắt buộc: Không báo giá chi tiết nếu policy yêu cầu Messenger; không lộ membership/private info; không quote order; không xử lý khiếu nại sâu public.; Ví dụ hành vi đúng: Trả ngắn, kéo Messenger nếu phù hợp; nếu troll/abuse thì không kéo Messenger, route moderation policy.

Surface: Messenger private; Guard bắt buộc: Được tư vấn sâu hơn, quote/order draft nếu runtime trả hợp lệ; vẫn không lộ private internal.; Ví dụ hành vi đúng: Render QuoteSnapshot, OrderDraftPrefillForm, hỏi xác nhận thông tin khi cần.

Surface: CRM proactive; Guard bắt buộc: Chỉ gửi nội dung phù hợp consent, fatigue, channel policy, sellable, sale lock và message trigger whitelist.; Ví dụ hành vi đúng: Không gợi ý SKU not sellable; không gửi sai hạng; không nhắc quá dày.

Surface: Website/landing preview; Guard bắt buộc: Chỉ dùng public-safe product profile; không dùng customer PII hoặc internal runtime state.; Ví dụ hành vi đúng: Hiển thị mô tả sản phẩm, cách dùng, CTA public.

Surface: Admin preview; Guard bắt buộc: Có thể hiển thị internal reason cho nhân sự có quyền, nhưng không được copy internal reason vào customer-facing response.; Ví dụ hành vi đúng: Tách preview_public và preview_internal rõ ràng.

## 15. GUARD EXECUTION FLOW

Nhận message_candidate từ Consultation Orchestrator hoặc Renderer.

Kiểm tra channel_surface để xác định mức public/private/admin preview.

Kiểm tra runtime_truth_snapshot: sellable, sale lock, recall suppression, quote_snapshot, order/payment state nếu liên quan.

Chạy forbidden term classifier: medical treatment, diagnosis-like, guarantee, SKU-only, internal formula, stock count, price guess, internal trace/QC leak.

Chạy public/private field filter để loại mọi field không được render ra khách.

Chạy claim whitelist: allowed, boundary-required, forbidden.

Nếu health-sensitive thì render theo template lock hoặc handoff nếu vượt boundary.

Nếu exact-lock thì chỉ thay biến, không paraphrase.

Nếu semi-lock thì rewrite trong phạm vi public-safe, giữ tone "Dạ Em … ạ", tự nhiên và có lực bán.

Sinh audit_payload với policy_version, guard_result và reason_code.

Trả public_safe_response hoặc BLOCK/HANDOFF cho Orchestrator.

## 16. IMPLEMENTATION TASKS

Mã task: P4-2E-T01; Việc dev/Codex/Copilot phải làm: Inspect codebase hiện có để tìm nơi render câu trả lời cuối cùng: consult, product recommend, combo, quote, order draft, CRM preview, live reply.; Output mong đợi: Render touchpoint map.

Mã task: P4-2E-T02; Việc dev/Codex/Copilot phải làm: Tạo hoặc chuẩn hóa PublicWordingClaimGuard contract, không để logic guard rải rác trong controller/template.; Output mong đợi: Một guard entry duy nhất hoặc rõ route/hook theo từng surface.

Mã task: P4-2E-T03; Việc dev/Codex/Copilot phải làm: Tạo forbidden output registry và claim policy adapter đọc từ seed/config/policy runtime.; Output mong đợi: Không hardcode claim rải trong service.

Mã task: P4-2E-T04; Việc dev/Codex/Copilot phải làm: Tạo public/private field suppression layer trước khi render.; Output mong đợi: Không lộ SKU nội bộ, formula, cost, QC, trace private, stock count.

Mã task: P4-2E-T05; Việc dev/Codex/Copilot phải làm: Gắn health-sensitive template lock và exact-lock renderer.; Output mong đợi: Template không bị paraphrase; thiếu biến thì fallback an toàn.

Mã task: P4-2E-T06; Việc dev/Codex/Copilot phải làm: Gắn audit/correlation cho mọi lần PASS/REWRITE/BLOCK/HANDOFF.; Output mong đợi: Có audit evidence để review.

Mã task: P4-2E-T07; Việc dev/Codex/Copilot phải làm: Viết unit test, feature test và smoke cho toàn bộ negative cases.; Output mong đợi: Test fail nếu AI nói chữa bệnh, SKU-only, lộ internal, giá tự tính hoặc dùng "hệ thống" public.

## 17. TEST MATRIX

Test ID: P4-2E-TST-001; Case: Candidate response nói "chữa bệnh/điều trị/thay thuốc/khỏi hẳn".; Expected result: Guard BLOCK hoặc rewrite sang public-safe; reason_code = MEDICAL_CLAIM.

Test ID: P4-2E-TST-002; Case: Candidate response dùng mã A1/B3/C7 làm câu trả lời chính với khách.; Expected result: Guard rewrite sang tên sản phẩm công khai và lý do tư vấn; reason_code = SKU_ONLY.

Test ID: P4-2E-TST-003; Case: Candidate response lộ tỷ lệ %, pilot, formula, cost, QC note.; Expected result: Guard BLOCK private fields; reason_code = INTERNAL_DATA_LEAK.

Test ID: P4-2E-TST-004; Case: Khách nói có bệnh nền và hỏi dùng được không.; Expected result: Guard dùng health-sensitive template; không tư vấn như thuốc.

Test ID: P4-2E-TST-005; Case: Quote response thiếu QuoteSnapshot nhưng lại báo giá cuối.; Expected result: Guard BLOCK/REQUIRE_RUNTIME_QUOTE; không render giá tự tính.

Test ID: P4-2E-TST-006; Case: SKU not sellable/recall/sale lock nhưng câu trả lời vẫn chốt mua.; Expected result: Guard BLOCK, chuyển sang alternative/handoff theo runtime.

Test ID: P4-2E-TST-007; Case: Public comment lộ thông tin thành viên hoặc thông tin giao hàng.; Expected result: Guard BLOCK public leak; chuyển Messenger/private nếu policy cho phép.

Test ID: P4-2E-TST-008; Case: Customer-facing response có từ "hệ thống".; Expected result: Guard rewrite sang wording public-safe phù hợp.

Test ID: P4-2E-TST-009; Case: Exact-lock template bị AI paraphrase.; Expected result: Test fail. Exact-lock chỉ được replace biến hợp lệ.

Test ID: P4-2E-TST-010; Case: Boundary quá dài làm mất lực bán khi khách chỉ hỏi khẩu vị/sản phẩm.; Expected result: Guard không tự thêm disclaimer không cần thiết; câu trả lời vẫn có lý do tư vấn và CTA mềm.

## 18. SMOKE REQUIRED

Smoke 1: Product consult bình thường -> trả public-safe, có tên sản phẩm công khai, có lý do gợi ý, không có mã SKU nội bộ.

Smoke 2: Health-sensitive -> dùng template khóa, không chữa bệnh, không thay thuốc, không cam kết.

Smoke 3: Product not sellable -> không chốt mua, không lộ tồn kho, gợi ý thay thế theo runtime.

Smoke 4: Quote -> chỉ render giá từ QuoteSnapshot, có quote expiry, không tự tính quyền lợi thành viên.

Smoke 5: Live public -> không lộ giá/private info nếu policy yêu cầu Messenger; không dùng "hệ thống" trong câu trả lời khách.

Smoke 6: Internal data leak injection -> guard block formula/cost/QC/trace/supplier private.

Smoke 7: Exact-lock -> biến được thay đúng, câu chữ không bị paraphrase.

Smoke 8: Audit -> mỗi block/rewrite/handoff có guard_result, reason_code, policy_version, correlation_id.

## 19. EVIDENCE REQUIRED

Code diff / file map: Liệt kê file guard, policy adapter, renderer hook, test file, seed/config nếu có.

Policy snapshot: Claim whitelist, forbidden terms, private field suppression, health-sensitive template, exact-lock list.

Test output: Unit/feature/smoke pass, đặc biệt negative test phải chứng minh guard chặn được vi phạm.

Screenshot/log: Ví dụ input/output trước và sau guard cho các case y tế, SKU-only, internal leak, quote without snapshot.

Audit sample: Một audit record mẫu có policy_version, guard_result, reason_code, correlation_id.

Owner review note: Xác nhận guard không làm mất linh hồn tư vấn và không làm mất lực chốt đơn.

## 20. REQUIRED REPORT FORMAT - 14 MỤC

## 1. Tóm tắt.

## 2. File đã sửa.

## 3. Nguồn yêu cầu.

## 4. Evidence đã dùng.

## 5. Lệnh đã chạy.

## 6. Kết quả test.

## 7. Kết quả backend build.

## 8. Kết quả frontend build.

## 9. Kết quả cleanup process.

## 10. Cập nhật Markdown.

## 11. Kết quả database migration/update nếu áp dụng.

## 12. Kết quả seed validation nếu áp dụng.

## 13. Rủi ro còn lại.

## 14. Cập nhật handoff.

## 21. DONE GATE

DG-01: Tất cả customer-facing responses đi qua guard trước khi render.

DG-02: Forbidden medical/treatment/diagnosis/guarantee claims bị block hoặc rewrite đúng policy.

DG-03: Public/private field filter chặn được SKU internal, formula, cost, QC, trace private, supplier private, stock count.

DG-04: Health-sensitive template hoạt động đúng, không làm mất lực tư vấn khi không cần boundary.

DG-05: Exact-lock không bị paraphrase; semi-lock rewrite còn tự nhiên và có lực bán.

DG-06: Không có response customer-facing dùng từ "hệ thống".

DG-07: Không có giá/tồn/sellable/member benefit/order/paid/revenue tự tính trong guard.

DG-08: Có audit/evidence/test output đủ để owner review.

## 22. FAIL GATE

AI nói chữa bệnh, điều trị, thay thuốc, khỏi hẳn, cam kết hiệu quả hoặc chẩn đoán tình trạng sức khỏe.

AI dùng mã SKU nội bộ làm câu trả lời chính với khách.

AI lộ công thức, tỷ lệ %, chi phí, QC note, trace internal, supplier private, stock count hoặc internal sale lock reason.

AI báo giá khi không có QuoteSnapshot hoặc tự tính quyền lợi thành viên/Diamond.

AI chốt SKU not sellable, recall, sale lock, out of stock hoặc channel suppressed.

AI hỏi lại thông tin runtime đã có hoặc nói "để kiểm tra" khi runtime đã resolve.

AI dùng từ "hệ thống" trong customer-facing response.

Guard làm câu trả lời quá phòng thủ, lan man, mất lực bán trong tình huống tư vấn bình thường.

Không có test negative hoặc audit evidence cho các case guard quan trọng.

## 23. DOWNSTREAM HANDOFF

## 07-CẦU NỐI BÀN GIAO KÊNH MESSENGER TRỰC TIẾP VÀ CRM: Chỉ chuyển sang Channel Bridge khi guard đã kiểm soát được public/private wording và channel surface policy.

## 08-BÁO CÁO KIỂM THỬ KHÓI VÀ BẰNG CHỨNG: Chỉ lập smoke/evidence report sau khi có test output và audit sample cho guard.

Gateway/Release: Không được mở. Guard pass cục bộ không đồng nghĩa Gateway PASS, Completion Decision hoặc Production Readiness.

## 24. PROMPT COPY GIAO DEV / CODEX / COPILOT

## PROMPT-P4-2E - SAFETY / PUBLIC WORDING / CLAIM GUARD / PUBLIC-PRIVATE BOUNDARY HANDOFF

## MODE: LIMITED IMPLEMENTATION HANDOFF

DO NOT rewrite Commerce Runtime, Pricing Engine, Sellable Gate, Payment Core, Verified Revenue or Product Master.

## OBJECTIVE:

Implement or normalize the AI Advisor Safety / Public Wording / Claim Guard so every customer-facing response is checked before rendering.

## MUST DO:

## 1. Inspect current render touchpoints for consult, product recommendation, combo, quote, order draft, CRM preview and live reply.

## 2. Create/normalize one guard contract receiving message_candidate, response_family, product_public_data, customer_context, runtime_truth_snapshot, safety_policy_snapshot and channel_surface.

## 3. Block or rewrite forbidden outputs: treatment, diagnosis-like, guaranteed result, internal SKU-only, formula/cost/QC/trace/supplier leak, stock count, price guess and public "hệ thống" wording.

## 4. Preserve approved Eastern wellness language only inside public-safe framing: "theo tinh thần ẩm thực/thực dưỡng phương Đông", "qua bữa ăn", "hỗ trợ/chăm sóc/bồi bổ".

## 5. Apply health-sensitive template when customer mentions disease, treatment, pregnancy, allergy, dietary restrictions or doctor instruction.

## 6. Enforce exact-lock without paraphrase; only variable replacement is allowed.

## 7. Write tests for all negative cases and generate audit evidence.

## MUST NOT DO:

## 1. Do not calculate price, discount, member benefit, stock, sellable, order_code, paid, verified revenue, commission, ROAS or payout.

## 2. Do not expose internal SKU code, formula, pilot, cost, QC notes, trace internal, supplier private, stock count or internal suppression reason to customers.

## 3. Do not create new medical claims or claim whitelist entries without owner-approved policy.

## 4. Do not call this Gateway PASS, Completion Decision, Release Readiness or Production Readiness.

## REPORT FORMAT:

Return the standard 14-section implementation report: Tóm tắt; File đã sửa; Nguồn yêu cầu; Evidence đã dùng; Lệnh đã chạy; Kết quả test; Kết quả backend build; Kết quả frontend build; Kết quả cleanup process; Cập nhật Markdown; Kết quả database migration/update nếu áp dụng; Kết quả seed validation nếu áp dụng; Rủi ro còn lại; Cập nhật handoff.

PHASE 4 COMPLETE: KHONG

IMPLEMENTATION COMPLETE: KHONG

Completion Decision: KHONG

Release Readiness: KHONG

Production Readiness: KHONG

Go-live Decision: KHONG

Kết luận khóa: File 06-P4-2E chỉ khóa lớp Safety / Public Wording / Claim Guard cho AI Advisor. Guard pass cục bộ không đồng nghĩa Phase 4 complete, không đồng nghĩa implementation complete, không đồng nghĩa Gateway PASS và không được dùng để tuyên bố production-ready.

## PHASE-04-AI-ADVISOR-RUNTIME

## 07-CẦU NỐI BÀN GIAO KÊNH MESSENGER TRỰC TIẾP VÀ CRM



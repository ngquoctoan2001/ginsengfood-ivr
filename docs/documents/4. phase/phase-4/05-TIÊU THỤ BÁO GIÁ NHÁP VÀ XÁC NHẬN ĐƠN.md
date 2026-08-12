# P4.2D - QUOTE ORDER DRAFT CONFIRMATION CONSUMER

## Khối nguồn bắt buộc đã nhập vào file này

File này có khối nguồn bắt buộc để chạy lẻ, nhưng khi bàn giao Phase 4 phải đọc `12-BẢNG GOM GIAI ĐOẠN 4.md`, `10-PHỤ LỤC KHÓA THỜI GIAN CHẠY THỰC CHIẾN CỐ VẤN AI.md` và `11-MA TRẬN SRS VÀ BÀN GIAO TRIỂN KHAI.md` trước. Nếu nội dung file này khác Bảng Gôm/phụ lục/SRS handoff, áp dụng Bảng Gôm/phụ lục/SRS handoff và xử lý fail-closed.

### A. Nội dung nguồn đã nhập

1. Master/runtime governance: quote/order phải đi theo Commerce Runtime, không theo prompt. Product/Operational chỉ cung cấp product/sellable/suppression truth. Commerce Runtime quyết định sellable, QuoteSnapshot, OrderDraft, CustomerConfirmation, official order, order_code, payment public-safe state, shipping/ETA và ORDER_VERIFIED. AI Advisor chỉ consume/render/handoff.
2. Evidence/release governance: quote/order consumer pass cục bộ không phải Completion PASS, Release Ready, Production Ready hoặc Go-live. Mọi gate mặc định là PENDING_EVIDENCE cho đến khi có artifact được owner chấp nhận.
3. AI Advisor quote/order lock: AI được yêu cầu quote, render QuoteSnapshot public-safe, render Order Draft, hỏi CustomerConfirmation, hiển thị order_code nếu runtime trả và hướng dẫn payment/shipping theo state public-safe.
4. Commerce lock: final price chỉ đến từ QuoteSnapshot còn hiệu lực. Quote expired phải xin snapshot mới. Order Draft không phải Official Order. CustomerConfirmation bắt buộc trước official order. Không có order_code thì không nói đơn chính thức. Payment selected/COD selected/ảnh chuyển khoản không phải Paid. ORDER_VERIFIED mới là revenue truth.
5. Product/Operational lock: trước quote phải có sellable/suppression pass. Recall/sale lock/not sellable/channel suppressed chặn quote/order dù khách muốn mua.
6. CRM/Member lock: member benefit chỉ hiển thị nếu QuoteSnapshot/runtime trả eligible. Không tự cộng member benefit, Golden Hour benefit hoặc Diamond buyer benefit.
7. Finance/Diamond lock: Diamond commission, PIT, payable, payout không thuộc quote/order consumer. AI không nói đã ghi nhận hoa hồng hoặc đã chi trả.
8. Gateway/Ads/Live lock: public comment/live không public final quote/PII/payment/order state. Ads/ROAS chỉ nhận verified revenue từ ORDER_VERIFIED; quote/order draft không phải revenue.

### B. Conflict lock

- No QuoteSnapshot: không final price.
- Quote expired: không dùng giá cũ.
- Order Draft chưa CustomerConfirmation: không nói đơn đã chốt.
- Không có order_code: không nói đơn chính thức.
- Payment selected hoặc screenshot: không nói paid.
- Member/Diamond/Giờ Vàng benefit thiếu policy/runtime: không tự cộng.
- Sellable/recall/sale lock fail: không quote, không draft, không chốt.
- Evidence thiếu quote/order/payment/shipping artifact: giữ PENDING_EVIDENCE, không gọi PASS.

### C. Trạng thái evidence bắt buộc

Trạng thái mặc định của file này: QUOTE_ORDER_CONSUMER_LOCK_WITH_IMPORTED_SOURCE. Evidence tối thiểu phải có sellable result, QuoteSnapshot id/ref, quote expiry, order draft ref, CustomerConfirmation result, order_code nếu official, payment state public-safe, shipping state public-safe, idempotency key và masked PII status. Không được gọi RUNTIME_IMPLEMENTATION_READY, GATEWAY_PASS, COMPLETION_PASS, RELEASE_READY, PRODUCTION_READY hoặc GO_LIVE_APPROVED.

### D. Khóa riêng của file này

File 05 chỉ xử lý tiêu thụ báo giá, nháp đơn và xác nhận đơn. Nhiệm vụ là render đúng dữ liệu Commerce Runtime và chặn mọi đường tự tính/tự chốt/tự xác nhận thanh toán/doanh thu.

## Nội dung triển khai

## PROMPT-P4-2D - QUOTE SNAPSHOT / ORDER DRAFT / CUSTOMER CONFIRMATION / OFFICIAL ORDER CONSUMER HANDOFF

Tài liệu này khóa riêng lớp AI Advisor consume QuoteSnapshot, render Order Draft, lấy Customer Confirmation và hiển thị Official Order theo kết quả Commerce Runtime. AI Advisor không được tự tính giá cuối, không tự tạo order_code, không tự nói đã thanh toán, không tự ghi nhận doanh thu, không tự tính hoa hồng, ROAS hoặc payout.

## PHASE MARKER

Phase: PHASE-04 - AI ADVISOR RUNTIME

File: 05-TIÊU THỤ BÁO GIÁ NHÁP VÀ XÁC NHẬN ĐƠN.docx

Prompt: PROMPT-P4-2D - QuoteSnapshot / Order Draft / Customer Confirmation / Official Order Consumer Handoff

Mode: LIMITED IMPLEMENTATION HANDOFF - chỉ triển khai lớp consumer cho quote, order draft và xác nhận khách. Không sửa lõi Commerce Runtime nếu chưa có phase owner riêng.

## 1. HEADER

Mục tiêu của file này là giao cho dev/Codex/Copilot một prompt triển khai giới hạn cho lớp AI Advisor kết nối Commerce Runtime ở đoạn cuối của tư vấn bán hàng: từ nhu cầu và sản phẩm đã được đề xuất, chuyển sang quote hợp lệ, order draft, xác nhận khách, official order, payment public-safe state và shipping public-safe state.

AI Advisor chỉ là consumer và renderer của quote/order runtime, không phải owner của giá, tồn, giảm giá, quyền lợi thành viên, đơn hàng, thanh toán hoặc doanh thu.

QuoteSnapshot là nguồn sự thật duy nhất cho final price mà AI được hiển thị với khách.

Order Draft không đồng nghĩa Official Order; Customer Confirmation mới cho phép tạo Official Order.

Không có order_code thì AI không được nói đơn hàng đã ghi nhận chính thức.

Payment selected không đồng nghĩa Paid; COD selected không paid; bank transfer selected không paid; chỉ Payment Core confirmation mới là Paid.

AI không được public số lượng tồn kho và không được chốt sản phẩm hết hàng hoặc bị sale lock/recall/channel suppressed.

## 2. MODE

Chế độ: Limited Implementation; Được phép: Được tạo/sửa adapter, DTO, renderer, form payload, guard, test và smoke cho AI consume quote/order draft/confirmation.; Không được phép: Không viết lại Sellable Gate, Pricing Engine, Member Benefit Engine, Payment Core, Shipping Core, Invoice, Verified Revenue, Commission hoặc ROAS.

Chế độ: Consumer only; Được phép: Được gọi API/contract đã có của Commerce Runtime để lấy QuoteSnapshot, OrderDraft, confirmation_state, official_order_state, payment_state public-safe, shipping_state public-safe.; Không được phép: Không tự tính final price, không tự gán quote expiry, không tự tạo order_code, không tự set paid, không tự verified revenue.

Chế độ: No code copy-paste; Được phép: Được dùng AI để viết prompt/checklist/test case và hướng dẫn dev inspect codebase.; Không được phép: Không copy code rời rạc vào dự án nếu chưa khớp kiến trúc, database, auth/RBAC, idempotency, audit, DTO và test thật.

## 3. SOURCE-OF-TRUTH

Nhóm dữ liệu/trạng thái: Sellable status; Owner: PHASE 3 - Commerce Runtime Sellable Gate; AI Advisor được làm gì: Đọc sellable_status và public-safe block_reason trước khi yêu cầu quote.; AI Advisor bị cấm làm gì: Không tự quyết SKU còn bán được; không dùng Product Active, SKU Active hoặc Warehouse Receipt thay cho Sellable.

Nhóm dữ liệu/trạng thái: QuoteSnapshot; Owner: PHASE 3 - Price/Program/Member Benefit/QuoteSnapshot; AI Advisor được làm gì: Hiển thị giá niêm yết, giá chương trình, quyền lợi hạng nếu runtime trả eligible, phí ship, VAT, tổng thanh toán và thời hạn giữ giá từ snapshot.; AI Advisor bị cấm làm gì: Không tự tính giá, không tự cộng/trừ ưu đãi, không tự suy luận Diamond/member benefit, không tự kéo dài thời hạn giữ giá.

Nhóm dữ liệu/trạng thái: Cart/Order Draft; Owner: PHASE 3 - Cart / Order Draft / Customer Confirmation; AI Advisor được làm gì: Render order draft payload, prefill thông tin cũ nếu runtime cho phép, yêu cầu khách xác nhận rõ.; AI Advisor bị cấm làm gì: Không gọi Order Draft là Official Order; không bỏ qua bước Customer Confirmation.

Nhóm dữ liệu/trạng thái: Official Order; Owner: PHASE 3 - Official Order / State Machine; AI Advisor được làm gì: Chỉ hiển thị order_code sau khi Commerce Runtime trả official_order_state hợp lệ.; AI Advisor bị cấm làm gì: Không tự tạo order_code; không nói "đã ghi nhận đơn chính thức" khi chưa có order_code.

Nhóm dữ liệu/trạng thái: Payment state; Owner: PHASE 3 - Payment Core; AI Advisor được làm gì: Hiển thị hướng dẫn thanh toán và trạng thái public-safe do Payment Core trả về.; AI Advisor bị cấm làm gì: Không nói đã thanh toán khi mới chọn COD/chuyển khoản; không tự đối soát ngân hàng; không tự set Paid.

Nhóm dữ liệu/trạng thái: Shipping state; Owner: PHASE 3 - Shipping Core; AI Advisor được làm gì: Hiển thị trạng thái giao hàng public-safe nếu có.; AI Advisor bị cấm làm gì: Không tự cam kết thời gian giao nếu runtime không có SLA/estimate hợp lệ.

Nhóm dữ liệu/trạng thái: Verified revenue/commission/ROAS; Owner: PHASE 3 - Verified Revenue / Commission / ROAS Handoff; AI Advisor được làm gì: Chỉ hiểu boundary để không render sai; không dùng trong tư vấn khách.; AI Advisor bị cấm làm gì: Không tự gọi order là verified revenue, không final commission, không payout, không verified ROAS.

## 4. ENTRY CONDITION

## 04-P4-2C Product Knowledge / Consultation Orchestrator đã đề xuất được sản phẩm hoặc combo hợp lệ về mặt tư vấn.

Runtime Consumer Contract đã có trường sellable_status, block_reason public-safe, quote_snapshot, quote_expiry, order_draft_payload, confirmation_state, official_order_state, payment_state public-safe và shipping_state public-safe.

Customer Memory Context Resolver đã xác định được customer identity ở mức đủ dùng: khách mới/khách cũ/member tier nếu runtime xác nhận, thông tin nhận hàng cũ nếu được phép dùng, nhu cầu người dùng chính hoặc người nhận quà.

Commerce Runtime có API/contract hoặc service boundary để tạo quote, tạo order draft, nhận customer confirmation và trả official order result.

RBAC, actor context, correlation_id, idempotency_key, audit log đã sẵn sàng cho các command có side effect.

Không còn điểm chặn P0 về việc AI tự tính giá, bỏ qua QuoteSnapshot, tạo order không cần xác nhận hoặc nói đã paid sai.

## 5. OBJECTIVE

Thiết kế và triển khai giới hạn lớp Quote / Order Draft / Confirmation Consumer để AI Advisor có thể chuyển mạch từ tư vấn sang chốt đơn đúng kỹ thuật: quote hợp lệ, form xác nhận rõ, không lẫn lộn draft với official order, không lẫn payment selected với paid, và không tạo doanh thu/hoa hồng/ROAS sai.

Mục tiêu: Tạo quote đúng nguồn; Diễn giải: AI yêu cầu Commerce Runtime tạo hoặc lấy QuoteSnapshot khi khách có tín hiệu mua.; Tiêu chí đạt: Không có QuoteSnapshot thì không render final price.

Mục tiêu: Hiển thị giá đúng snapshot; Diễn giải: AI render đúng listed price, program price/discount, member/Diamond benefit nếu runtime trả eligible, ship, VAT, tổng thanh toán, quote expiry.; Tiêu chí đạt: Không tự tính hoặc tự sửa số tiền.

Mục tiêu: Order Draft rõ ràng; Diễn giải: AI hiển thị bản nháp đơn hàng có dòng sản phẩm, giá, tổng, thông tin nhận hàng, phương thức thanh toán và CTA xác nhận.; Tiêu chí đạt: Không gọi Draft là đơn chính thức.

Mục tiêu: Customer Confirmation bắt buộc; Diễn giải: AI chỉ gửi confirmation event khi khách xác nhận rõ hoặc bấm nút xác nhận hợp lệ.; Tiêu chí đạt: Không tự xác nhận thay khách; không tạo official order từ tín hiệu mơ hồ.

Mục tiêu: Official Order đúng trạng thái; Diễn giải: AI chỉ nói đơn chính thức khi Commerce Runtime trả order_code.; Tiêu chí đạt: Không có order_code thì không nói đã ghi nhận đơn.

Mục tiêu: Payment/Shipping public-safe; Diễn giải: AI hiển thị trạng thái thanh toán/giao hàng theo runtime public-safe state.; Tiêu chí đạt: Không nói đã paid khi chưa có Payment Core confirmation.

## 6. SCOPE IN

Quote request adapter từ AI Advisor sang Commerce Runtime.

QuoteSnapshot consumer DTO và renderer customer-facing.

Order Draft payload renderer, bao gồm prefill form cho khách cũ khi runtime cho phép.

New customer order capture form fallback khi thiếu thông tin nhận hàng.

Customer Confirmation event adapter hoặc confirmation handoff theo kênh.

Official Order result consumer và order_code public renderer.

Payment selected/payment WAITING/paid public-safe renderer theo Payment Core.

Shipping public-safe renderer theo Shipping Core.

Guard chống quote hết hạn, sản phẩm không sellable, recall/sale lock, thiếu identity, thiếu member benefit, thiếu shipping/tax required field.

Audit, idempotency, correlation và evidence cho các command có side effect.

Smoke test cho luồng khách mới, khách cũ, member/Diamond, quote hết hạn, out of stock, payment selected nhưng chưa paid, official order thành công và thất bại.

## 7. SCOPE OUT

Không thiết kế lại Price Engine, Program Engine, Member Benefit Engine hoặc Diamond Referral Engine.

Không thiết kế lại Sellable Gate hoặc Stock Allocation Core.

Không tạo logic Payment Core, reconciliation ngân hàng, MISA posting, invoice/tax engine hoặc COD settlement.

Không tạo Verified Revenue, commission finalization, ROAS verified attribution hoặc payout.

Không triển khai IVR; IVR vẫn reserved cho phase riêng.

Không mở Gateway, không gọi Completion Decision, không gọi Production Readiness.

Không hardcode giá, thời hạn giữ giá, SKU, tồn kho, phí ship, VAT, quyền lợi thành viên trong prompt hoặc frontend.

## 8. RUNTIME CONTRACT REQUIRED

AI Advisor phải nhận đủ dữ liệu runtime ở dạng read-only hoặc command result. Thiếu trường bắt buộc thì fail-closed, không đoán.

Contract group: actor_context; Trường bắt buộc: actor_user_id, actor_type, channel, source_channel, correlation_id, idempotency_key cho command; Nguồn: PHASE 0; Fail-closed nếu thiếu: Không tạo quote/order draft/confirmation command.

Contract group: customer_context; Trường bắt buộc: customer_id hoặc guest_token, customer_price_identity, member_tier nếu có, verified_contact_level, saved_address_permission; Nguồn: Customer Context Resolver / Commerce Runtime; Fail-closed nếu thiếu: Không quote quyền lợi member; chuyển sang xác minh hoặc capture tối thiểu.

Contract group: sellable_result; Trường bắt buộc: sku_id/public_product_id, sellable_status, block_reason_public_safe, sellable_checked_at; Nguồn: Commerce Runtime Sellable Gate; Fail-closed nếu thiếu: Không tạo quote; render lý do public-safe và gợi ý thay thế nếu có.

Contract group: quote_snapshot; Trường bắt buộc: quote_snapshot_id, quote_code nếu có, line_items, listed_price, program_price/discount, member_benefit, shipping_fee, VAT, total_payable, quote_expiry_at, quote_policy_label; Nguồn: Commerce Runtime QuoteSnapshot; Fail-closed nếu thiếu: Không render final price; yêu cầu runtime tạo quote lại.

Contract group: order_draft_payload; Trường bắt buộc: draft_id, line_items, totals, receiver_name, phone, address, payment_options, confirmation_required, buttons/actions nếu channel hỗ trợ; Nguồn: Commerce Runtime Order Draft; Fail-closed nếu thiếu: Không xin xác nhận đơn; capture thiếu thông tin hoặc handoff.

Contract group: confirmation_state; Trường bắt buộc: confirmation_required, confirmation_method, confirmed_at, confirmed_by, confirmation_text/button_event; Nguồn: Commerce Runtime / Channel Adapter; Fail-closed nếu thiếu: Không tạo Official Order.

Contract group: official_order_state; Trường bắt buộc: official_order_id, order_code, order_status, created_at, quote_snapshot_id; Nguồn: Commerce Runtime Official Order; Fail-closed nếu thiếu: Không nói đơn chính thức; render thông báo đang chờ xác nhận nội bộ hoặc lỗi public-safe.

Contract group: order_success_message_state; Trường bắt buộc: order_code, order_success_template_code, payment_or_shipping_public_next_step, post_purchase_care_allowed; Nguồn: Commerce Runtime + CRM/Member canonical registry; Fail-closed nếu thiếu: Không render câu chúc mừng đơn thành công; chỉ nói trạng thái đang cần kiểm tra public-safe.

Contract group: payment_state_public; Trường bắt buộc: payment_method_selected, payment_status_public, payment_instruction, payment_confirmed_at nếu có; Nguồn: Payment Core; Fail-closed nếu thiếu: Không nói paid; chỉ hướng dẫn thanh toán/chờ xác nhận.

Contract group: shipping_state_public; Trường bắt buộc: shipping_method, shipping_status_public, estimated_window nếu runtime trả; Nguồn: Shipping Core; Fail-closed nếu thiếu: Không cam kết giao hàng ngoài dữ liệu runtime.

## 9. STATE CHAIN LOCK

Bước: 1; Trạng thái: CONSULTATION_READY; AI được nói với khách: Dạ với nhu cầu Mình vừa chia sẻ, Em chọn dòng này là sát nhất ạ.; AI không được nói: Không báo giá cuối nếu chưa có QuoteSnapshot.

Bước: 2; Trạng thái: QUOTE_REQUESTED; AI được nói với khách: Em đang lên bản giá theo chương trình hiện tại cho Mình ạ.; AI không được nói: Không tự tính giá hoặc đoán ưu đãi.

Bước: 3; Trạng thái: QUOTE_SNAPSHOT_CREATED; AI được nói với khách: Hiển thị đúng giá/tổng/thời hạn giữ giá theo snapshot.; AI không được nói: Không sửa số tiền hoặc kéo dài thời hạn giữ giá.

Bước: 4; Trạng thái: ORDER_DRAFT_CREATED; AI được nói với khách: Hiển thị bản xác nhận đơn dạng form/draft.; AI không được nói: Không nói đơn đã ghi nhận chính thức.

Bước: 5; Trạng thái: CUSTOMER_CONFIRMATION_WAITING; AI được nói với khách: Mình kiểm tra lại thông tin, bấm/nhắn xác nhận giúp Em ạ.; AI không được nói: Không tạo order nếu khách chưa xác nhận rõ.

Bước: 6; Trạng thái: CUSTOMER_CONFIRMED; AI được nói với khách: Dạ Em đã nhận xác nhận của Mình, đang chuyển sang ghi nhận đơn chính thức ạ.; AI không được nói: Không tự tạo order_code trong AI.

Bước: 7; Trạng thái: OFFICIAL_ORDER_CREATED; AI được nói với khách: Dạ đơn của Mình đã được ghi nhận với mã đơn {{order_code}} ạ.; AI không được nói: Không nói có mã đơn nếu runtime chưa trả order_code.

Bước: 8; Trạng thái: PAYMENT_SELECTED; AI được nói với khách: Hiển thị hướng dẫn thanh toán hoặc COD theo trạng thái public-safe.; AI không được nói: Không nói đã thanh toán.

Bước: 9; Trạng thái: PAYMENT_CONFIRMED_PAID; AI được nói với khách: Chỉ khi Payment Core xác nhận: Dạ đơn của Mình đã được ghi nhận thanh toán ạ.; AI không được nói: Không tự set paid từ ảnh chuyển khoản nếu chưa qua quy trình xác minh.

Bước: 10; Trạng thái: SHIPPING_IN_PROGRESS; AI được nói với khách: Hiển thị trạng thái vận chuyển public-safe nếu runtime có.; AI không được nói: Không hứa thời gian giao ngoài SLA/estimate runtime.

## 10. ORDER DRAFT RENDERING RULE

Order Draft phải được render như một bản xác nhận đơn có cấu trúc, không chỉ là đoạn tóm tắt văn bản. Khi kênh hỗ trợ UI button/form, phải dùng button. Khi không hỗ trợ, text fallback vẫn phải mô phỏng form đầy đủ và yêu cầu khách xác nhận rõ.

Tình huống: Khách cũ có thông tin nhận hàng hợp lệ; Render bắt buộc: Form prefill: sản phẩm, giá, tổng tiền, tên, số điện thoại, địa chỉ cũ, phương thức thanh toán.; CTA bắt buộc: [Xác nhận đơn] + [Sửa thông tin nhận hàng]; Fail nếu: Chỉ nói "Em dùng thông tin cũ" mà không render form/draft.

Tình huống: Khách mới hoặc thiếu thông tin; Render bắt buộc: Form capture tối thiểu: tên người nhận, số điện thoại, địa chỉ, phương thức thanh toán, xác nhận quote còn hiệu lực.; CTA bắt buộc: [Gửi thông tin nhận hàng] hoặc text fallback yêu cầu đủ trường; Fail nếu: Tạo official order khi thiếu thông tin bắt buộc.

Tình huống: Member/Diamond eligible theo runtime; Render bắt buộc: Phải hiển thị quyền lợi hạng nếu runtime trả eligible; nếu runtime nói không cộng chương trình thì render đúng policy.; CTA bắt buộc: [Xác nhận đơn]; Fail nếu: Bỏ giảm hạng hợp lệ hoặc tự cộng giảm hạng khi runtime không cho.

Tình huống: Quote sắp hết hạn/hết hạn; Render bắt buộc: Hiển thị thời hạn giữ giá nếu còn hiệu lực; nếu hết hạn phải xin tạo quote mới.; CTA bắt buộc: [Tạo lại giá hiện tại] nếu channel hỗ trợ; Fail nếu: Dùng quote hết hạn để tạo order.

Tình huống: SKU bị chặn sau khi draft; Render bắt buộc: Dừng chốt, render public-safe block reason và gợi ý thay thế nếu có.; CTA bắt buộc: [Chọn sản phẩm thay thế] nếu có; Fail nếu: Vẫn cho xác nhận đơn với SKU không sellable.

## 11. CUSTOMER-FACING TEMPLATE LOCK

Các mẫu dưới đây là khung render. Dev không hardcode số tiền trong template; mọi biến số tiền, quyền lợi, thời hạn và mã đơn phải lấy từ runtime.

Mã template: TPL-QUOTE-READY; Khi dùng: QuoteSnapshot còn hiệu lực; Khung câu customer-facing: Dạ Em lên bản giá hiện tại cho Mình rồi ạ. {{quote_line_summary}} Tổng thanh toán là {{total_payable}}. Giá này được giữ đến {{quote_expiry_at}} theo chương trình hiện tại ạ.

Mã template: TPL-ORDER-DRAFT-PREFILL; Khi dùng: Khách cũ dùng thông tin cũ; Khung câu customer-facing: Dạ Em dùng thông tin nhận hàng cũ để lên bản xác nhận đơn cho Mình ạ. Mình kiểm tra lại thông tin bên dưới, đúng rồi thì bấm Xác nhận đơn giúp Em ạ.

Mã template: TPL-ORDER-DRAFT-NEW; Khi dùng: Khách mới/thiếu thông tin; Khung câu customer-facing: Dạ để Em lên đơn đúng cho Mình, Mình gửi giúp Em tên người nhận, số điện thoại, địa chỉ nhận hàng và phương thức thanh toán mong muốn ạ.

Mã template: TPL-QUOTE-EXPIRED; Khi dùng: Quote hết hạn; Khung câu customer-facing: Dạ bản giá vừa rồi đã hết thời hạn giữ giá. Em sẽ lên lại giá hiện tại cho Mình trước khi xác nhận đơn ạ.

Mã template: TPL-OFFICIAL-ORDER; Khi dùng: Official Order đã có order_code; Khung câu customer-facing: Dạ đơn của Mình đã được ghi nhận chính thức với mã đơn {{order_code}} ạ. {{payment_or_shipping_public_next_step}}

Mã template: TPL-OFFICIAL-ORDER-SUCCESS-CARE; Khi dùng: Official Order đã có order_code và `post_purchase_care_allowed=true`; Khung câu customer-facing: Dạ chúc mừng Mình, đơn hàng của Mình đã được Ginsengfood ghi nhận thành công với mã đơn {{order_code}} ạ. {{payment_or_shipping_public_next_step}} Sau khi Mình nhận hàng, bên Em sẽ tiếp tục chăm sóc và nhắc cách dùng phù hợp ạ.

Rule template: TPL-OFFICIAL-ORDER-SUCCESS-CARE không được nói đã thanh toán, đã xuất kho hoặc đang giao nếu `payment_state_public` / `shipping_state_public` chưa trả trạng thái đó.

Mã template: TPL-PAYMENT-WAITING; Khi dùng: Đã chọn phương thức nhưng chưa paid; Khung câu customer-facing: Dạ phương thức thanh toán của Mình đã được ghi nhận. Khi bộ phận thanh toán xác nhận thành công, trạng thái thanh toán sẽ được cập nhật ạ.

Mã template: TPL-SELLABLE-bị chặn; Khi dùng: SKU không bán được; Khung câu customer-facing: Dạ dòng này hiện chưa thể lên đơn ở thời điểm này. Em gợi ý Mình chuyển sang {{alternative_product_public_name}} vì {{alternative_reason}} ạ.

## 12. GUARD MATRIX

Guard: QUOTE_REQUIRED; Điều kiện kích hoạt: Khách hỏi giá/chốt mua nhưng chưa có QuoteSnapshot; Hành động của AI: Gọi Commerce Runtime quote flow; không render final price.; Bằng chứng cần có: Request/response quote, correlation_id, runtime source.

Guard: QUOTE_EXPIRED; Điều kiện kích hoạt: quote_expiry_at đã qua hoặc runtime trả expired; Hành động của AI: Không dùng snapshot cũ; yêu cầu tạo quote mới.; Bằng chứng cần có: Expired snapshot id, new quote request.

Guard: SELLABLE_BLOCKED; Điều kiện kích hoạt: sellable_status không cho bán; Hành động của AI: Không tạo quote/order; render public-safe block reason, gợi ý thay thế nếu có.; Bằng chứng cần có: Sellable result + block reason.

Guard: MEMBER_BENEFIT_UNRESOLVED; Điều kiện kích hoạt: Khách có tín hiệu member nhưng runtime chưa resolve quyền lợi; Hành động của AI: Không quote final member price; yêu cầu resolver/handoff theo policy.; Bằng chứng cần có: Member resolver response.

Guard: ORDER_DRAFT_NOT_CONFIRMED; Điều kiện kích hoạt: Draft đã tạo nhưng chưa có confirmation; Hành động của AI: Không tạo official order; nhắc khách xác nhận rõ.; Bằng chứng cần có: Draft id, confirmation state.

Guard: ORDER_CODE_MISSING; Điều kiện kích hoạt: Official order result không có order_code; Hành động của AI: Không nói đơn chính thức; báo public-safe đang cần xử lý hoặc handoff.; Bằng chứng cần có: Order API response/error.

Guard: PAYMENT_UNCONFIRMED; Điều kiện kích hoạt: COD/chuyển khoản đã chọn nhưng Payment Core chưa confirmed paid; Hành động của AI: Không nói paid; chỉ hướng dẫn/chờ xác nhận.; Bằng chứng cần có: Payment public state.

Guard: HIGH_RISK_COMMAND_MISSING_IDEMPOTENCY; Điều kiện kích hoạt: Command tạo quote/order/confirmation thiếu idempotency_key; Hành động của AI: Block command, trả lỗi kỹ thuật cho dev, không tiếp tục.; Bằng chứng cần có: Request metadata audit.

Guard: RECALL_SALE_LOCK_SUPPRESSION; Điều kiện kích hoạt: Recall/sale lock/channel suppression active; Hành động của AI: Dừng bán; không quote; không draft; không quảng bá.; Bằng chứng cần có: Runtime suppression signal.

## 13. DTO / API BOUNDARY GỢI Ý

Tên DTO và endpoint bên dưới là gợi ý boundary để dev đối chiếu codebase thật. Không được copy-dán như code hoàn chỉnh nếu kiến trúc thật khác; dev phải map sang service/module hiện có.

Boundary: CreateQuoteForAdvisor; Input chính: customer_context_id/guest_token, channel, selected_items, program_context, correlation_id, idempotency_key; Output chính: quote_snapshot_id, totals, quote_expiry_at, quote_status; Owner/ghi chú: Commerce Runtime owner; AI chỉ gọi/consume.

Boundary: GetQuoteSnapshotPublicView; Input chính: quote_snapshot_id, channel, customer_context_id; Output chính: public quote lines, discounts eligible, shipping, VAT, total, expiry; Owner/ghi chú: Không trả cost/formula/internal notes.

Boundary: CreateOrderDraftFromQuote; Input chính: quote_snapshot_id, customer_context, saved_address_choice, payment_option; Output chính: order_draft_payload, missing_fields, confirmation_required; Owner/ghi chú: Order Draft owner thuộc Commerce Runtime.

Boundary: SubmitCustomerConfirmation; Input chính: draft_id, confirmation_method, confirmation_value, actor/channel/correlation/idempotency; Output chính: confirmation_state, official_order_result nếu sync flow cho phép; Owner/ghi chú: High-risk command, cần audit/idempotency.

Boundary: GetOfficialOrderPublicState; Input chính: official_order_id hoặc order_code; Output chính: order_code, order_status_public, payment_state_public, shipping_state_public; Owner/ghi chú: Không trả revenue/commission/ROAS nội bộ.

Boundary: RefreshQuoteIfExpired; Input chính: expired_quote_snapshot_id, current cart/intent context; Output chính: new_quote_snapshot hoặc block reason; Owner/ghi chú: Không dùng snapshot hết hạn.

## 14. CHANNEL RULES

Kênh/ngữ cảnh: Live public comment; AI được làm: Trả lời ngắn để kéo vào Messenger/private nếu policy cho phép; không báo final price công khai.; AI không được làm: Không public giá cuối, không public thông tin đơn, không hỏi thông tin cá nhân công khai.

Kênh/ngữ cảnh: Messenger/private; AI được làm: Tư vấn sâu, tạo quote, render order draft, lấy xác nhận khách, hướng dẫn thanh toán.; AI không được làm: Không bỏ qua QuoteSnapshot/Confirmation; không lộ internal data.

Kênh/ngữ cảnh: CRM follow-up; AI được làm: Nhắc lại order draft/quote nếu còn hiệu lực hoặc đề xuất tạo quote mới.; AI không được làm: Không gửi quote hết hạn như giá hiện tại; không gợi ý SKU not sellable.

Kênh/ngữ cảnh: Website/Admin UI; AI được làm: Hiển thị form xác nhận, button action, trạng thái thanh toán/giao hàng public-safe.; AI không được làm: Không cho frontend tự tính giá hoặc quyết sellable.

Kênh/ngữ cảnh: IVR; AI được làm: Không triển khai trong file này; chỉ chừa boundary nếu sau này IVR consume order risk confirmation.; AI không được làm: Không nhúng IVR logic vào AI Advisor hoặc Commerce flow ở Phase 4.

## 15. EXECUTION STEPS

Inspect codebase thật để xác định các module hiện có: Advisor service, Commerce client, Quote API, Order Draft API, Payment/Shipping public state, Channel Adapter, DTO, prompt renderer, tests.

Map toàn bộ flow hiện tại vào STATE CHAIN LOCK của file này; ghi rõ bước nào đang thiếu, bước nào đang tự tính sai, bước nào đang hardcode.

Tạo hoặc chuẩn hóa QuoteSnapshot consumer DTO; đảm bảo mọi số tiền hiển thị đều đến từ runtime.

Tạo hoặc chuẩn hóa OrderDraft renderer dạng form/prefill; không dùng đoạn text tóm tắt thay cho xác nhận đơn.

Tạo Customer Confirmation adapter; mọi command tạo official order phải có actor, permission nếu cần, correlation_id, idempotency_key và audit.

Chuẩn hóa official order result renderer; chỉ render order_code khi runtime trả thành công.

Chuẩn hóa Payment/Shipping public-safe renderer; payment selected không được thành paid.

Thêm guard cho quote hết hạn, sellable bị chặn, recall/sale lock, missing member benefit, missing order code, missing idempotency.

Thêm unit/integration/smoke test theo ma trận bên dưới.

Xuất report 14 mục; không gọi Completion Decision nếu evidence chưa accepted.

## 16. SMOKE REQUIRED

Smoke ID: P4-2D-SMK-001; Kịch bản: Khách hỏi giá sau tư vấn sản phẩm hợp lệ; Expected result: AI gọi quote flow và render đúng QuoteSnapshot; không tự tính giá.; Evidence: Request/response quote + screenshot/render output.

Smoke ID: P4-2D-SMK-002; Kịch bản: Quote hết hạn; Expected result: AI không dùng snapshot cũ; yêu cầu tạo quote mới.; Evidence: Expired snapshot + new request log.

Smoke ID: P4-2D-SMK-003; Kịch bản: Khách cũ chọn dùng thông tin cũ; Expected result: AI render OrderDraftPrefillForm đầy đủ với nút/CTA xác nhận và sửa thông tin.; Evidence: Form screenshot/text fallback + draft payload.

Smoke ID: P4-2D-SMK-004; Kịch bản: Khách mới thiếu địa chỉ; Expected result: AI không tạo official order; yêu cầu capture trường thiếu.; Evidence: Missing fields response.

Smoke ID: P4-2D-SMK-005; Kịch bản: Khách xác nhận draft còn hiệu lực; Expected result: Commerce Runtime tạo official order và trả order_code; AI hiển thị mã đơn.; Evidence: Confirmation event + order result.

Smoke ID: P4-2D-SMK-006; Kịch bản: Runtime không trả order_code; Expected result: AI không nói đơn chính thức; handoff/fail public-safe.; Evidence: Error response + rendered response.

Smoke ID: P4-2D-SMK-007; Kịch bản: COD selected; Expected result: AI nói đã chọn COD/chờ xử lý, không nói paid.; Evidence: Payment state render.

Smoke ID: P4-2D-SMK-008; Kịch bản: Bank transfer selected nhưng chưa xác nhận ngân hàng; Expected result: AI không nói paid; hướng dẫn/chờ xác nhận theo Payment Core.; Evidence: Payment public state.

Smoke ID: P4-2D-SMK-009; Kịch bản: SKU bị sale lock sau khi đã draft; Expected result: AI dừng xác nhận, không tạo official order, đề xuất thay thế nếu có.; Evidence: Suppression signal + response.

Smoke ID: P4-2D-SMK-010; Kịch bản: Member/Diamond eligible theo runtime; Expected result: Quote/order draft hiển thị quyền lợi hạng nếu runtime cho phép; không tự cộng nếu không eligible.; Evidence: QuoteSnapshot with benefit fields.

Smoke ID: P4-2D-SMK-011; Kịch bản: Duplicate confirmation idempotency key same payload; Expected result: Không tạo đơn lần hai; trả same result hoặc idempotent response.; Evidence: Idempotency evidence.

Smoke ID: P4-2D-SMK-012; Kịch bản: Duplicate confirmation idempotency key different payload; Expected result: Conflict; không tạo side effect mới.; Evidence: Conflict log + audit.

Smoke ID: P4-2D-SMK-013; Kịch bản: Official Order có order_code nhưng payment/shipping chưa xác nhận.; Expected result: AI dùng template chúc mừng đơn thành công public-safe, có order_code, không nói paid/shipped nếu runtime chưa xác nhận.; Evidence: Official order result + rendered customer message + guard trace.

## 17. EVIDENCE REQUIRED

Contract mapping từ AI Advisor sang Commerce Runtime: endpoint/service, DTO, owner, field mapping, fail-closed behavior.

Evidence rằng không còn đoạn AI tự tính final price, discount, member benefit, shipping fee, VAT, sellable hoặc stock trong layer Advisor.

Evidence rằng Order Draft renderer có form/prefill/CTA xác nhận, không chỉ là text tóm tắt.

Evidence rằng Customer Confirmation là điều kiện trước Official Order.

Evidence rằng không có order_code thì không render official order message.

Evidence rằng template chúc mừng đơn thành công chỉ render sau `order_code`, có registry/template code và không nói paid/shipped ngoài runtime.

Evidence rằng payment selected không bị render thành paid.

Audit/idempotency evidence cho command quote/order draft/confirmation nếu có side effect.

Smoke screenshots/logs cho ít nhất 13 smoke case nêu trên.

Negative test evidence cho quote expired, sellable bị chặn, missing member benefit, missing idempotency và missing order_code.

## 18. REQUIRED REPORT FORMAT - 14 MỤC

STT: 1; Mục báo cáo bắt buộc: Tóm tắt; Nội dung tối thiểu phải có: Nêu phạm vi đã làm trong P4-2D và trạng thái còn bị chặn/PASS cục bộ.

STT: 2; Mục báo cáo bắt buộc: File đã sửa; Nội dung tối thiểu phải có: Liệt kê đường dẫn file thật đã tạo/sửa; không ghi chung chung.

STT: 3; Mục báo cáo bắt buộc: Nguồn yêu cầu; Nội dung tối thiểu phải có: Dẫn PHASE 4, P4-2D, Commerce Runtime boundary, QuoteSnapshot/Order Draft/Customer Confirmation rules.

STT: 4; Mục báo cáo bắt buộc: Evidence đã dùng; Nội dung tối thiểu phải có: Log, screenshot, request/response, test result, smoke evidence, audit/idempotency evidence.

STT: 5; Mục báo cáo bắt buộc: Lệnh đã chạy; Nội dung tối thiểu phải có: Commands build/test/smoke/lint/migration nếu có.

STT: 6; Mục báo cáo bắt buộc: Kết quả test; Nội dung tối thiểu phải có: Unit/integration/e2e/smoke; pass/fail rõ.

STT: 7; Mục báo cáo bắt buộc: Kết quả backend build; Nội dung tối thiểu phải có: Build status backend và lỗi còn lại nếu có.

STT: 8; Mục báo cáo bắt buộc: Kết quả frontend build; Nội dung tối thiểu phải có: Build status frontend/admin/channel UI và lỗi còn lại nếu có.

STT: 9; Mục báo cáo bắt buộc: Kết quả cleanup process; Nội dung tối thiểu phải có: Rà soát hardcode, dead code, duplicate logic, secret/internal leak.

STT: 10; Mục báo cáo bắt buộc: Cập nhật Markdown; Nội dung tối thiểu phải có: Docs/handoff/changelog/README cập nhật gì.

STT: 11; Mục báo cáo bắt buộc: Kết quả database migration/update nếu áp dụng; Nội dung tối thiểu phải có: Nêu không áp dụng hoặc migration id/status nếu có.

STT: 12; Mục báo cáo bắt buộc: Kết quả seed validation nếu áp dụng; Nội dung tối thiểu phải có: Nêu không áp dụng hoặc seed validation result nếu có.

STT: 13; Mục báo cáo bắt buộc: Rủi ro còn lại; Nội dung tối thiểu phải có: P0/P1/P2 risk, điểm chặn, downstream impact.

STT: 14; Mục báo cáo bắt buộc: Cập nhật handoff; Nội dung tối thiểu phải có: Việc còn lại cho P4-2E Safety/Public Wording/Claim Guard hoặc phase sau.

## 19. DONE GATE

AI Advisor chỉ render final price từ QuoteSnapshot, không còn tự tính giá hoặc ưu đãi.

Quote hết hạn bị block và yêu cầu tạo quote mới.

Order Draft được render thành form/payload xác nhận rõ, có prefill cho khách cũ khi runtime cho phép.

Customer Confirmation là điều kiện bắt buộc trước Official Order.

AI chỉ nói đơn chính thức khi có order_code từ Commerce Runtime.

AI chỉ render chúc mừng đơn hàng thành công bằng template approved sau order_code và không tự nói paid/shipped nếu runtime chưa xác nhận.

Payment selected không bị render thành Paid; COD/chuyển khoản chỉ là phương thức đã chọn hoặc chờ xác nhận.

SKU not sellable/recall/sale lock/channel suppressed không được quote/order.

Không public số lượng tồn kho, không lộ internal SKU code, cost, formula, QC notes, supplier, lot hoặc trace internal.

Tất cả command có side effect có actor/correlation/idempotency/audit evidence.

## 20. FAIL GATE

AI tự tính final price, discount, member benefit, shipping fee hoặc VAT.

AI báo giá cuối khi chưa có QuoteSnapshot.

AI dùng quote hết hạn để tạo Order Draft hoặc Official Order.

AI gọi Order Draft là Official Order hoặc nói đơn đã ghi nhận khi chưa có order_code.

AI render chúc mừng đơn hàng thành công khi chưa có order_code hoặc nói đơn đã thanh toán/đã giao ngoài runtime.

AI tự tạo order_code hoặc tạo đơn không cần Customer Confirmation.

AI nói đã thanh toán khi khách mới chọn COD/chuyển khoản nhưng Payment Core chưa xác nhận.

AI quote/chốt SKU hết hàng, not sellable, sale lock, recall hoặc suppressed.

AI public số lượng tồn kho hoặc lộ dữ liệu nội bộ.

Command tạo order/confirmation thiếu idempotency/audit/correlation.

Không có smoke/evidence nhưng report ghi Done hoặc Pass.

## 21. DOWNSTREAM HANDOFF

File tiếp theo: 06-CHẶN AN TOÀN NGÔN TỪ CÔNG KHAI VÀ TUYÊN BỐ.docx; Nhận từ P4-2D: Các template quote/order/payment/shipping customer-facing cần được đưa qua claim guard và public/private guard.; Yêu cầu giữ nguyên: Không nói quá claim, không lộ nội bộ, không dùng "hệ thống" trong câu trả lời khách, không làm mất lực bán.

File tiếp theo: 07-CẦU NỐI BÀN GIAO KÊNH MESSENGER TRỰC TIẾP VÀ CRM.docx; Nhận từ P4-2D: Order Draft/Quote flow phải phân biệt public comment, Messenger/private, CRM follow-up, website/admin UI.; Yêu cầu giữ nguyên: Giá/quote/order ưu tiên private; public không thu thập thông tin cá nhân.

## 22. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây cho dev/Codex/Copilot khi bắt đầu P4-2D. Prompt này không cho phép sửa code bừa bãi; phải inspect, map, triển khai giới hạn và báo cáo đủ evidence.

## PROMPT-P4-2D - QUOTE SNAPSHOT / ORDER DRAFT / CUSTOMER CONFIRMATION / OFFICIAL ORDER CONSUMER HANDOFF

## MODE: LIMITED IMPLEMENTATION HANDOFF.

DO NOT OPEN Global Gate.

DO NOT CALL Completion Decision.

DO NOT CALL Production Readiness.

Bạn đang triển khai PHASE-04-AI-ADVISOR-RUNTIME, file 05-TIÊU THỤ BÁO GIÁ NHÁP VÀ XÁC NHẬN ĐƠN.docx.

Mục tiêu:

- Khóa và triển khai lớp AI Advisor consume QuoteSnapshot, Order Draft, Customer Confirmation, Official Order result, Payment public-safe state và Shipping public-safe state từ Commerce Runtime.

- AI Advisor chỉ là consumer/renderer, không phải source-of-truth cho giá, tồn, sellable, order, payment, verified revenue, commission, ROAS hoặc payout.

Bắt buộc inspect trước khi sửa:

## 1. Tìm các module/service hiện có liên quan AI Advisor, Commerce client, quote, cart, order draft, customer confirmation, official order, payment, shipping, channel adapter và prompt renderer.

## 2. Lập current state mapping theo STATE CHAIN LOCK: CONSULTATION_READY -> QUOTE_REQUESTED -> QUOTE_SNAPSHOT_CREATED -> ORDER_DRAFT_CREATED -> CUSTOMER_CONFIRMATION_WAITING -> CUSTOMER_CONFIRMED -> OFFICIAL_ORDER_CREATED -> PAYMENT_SELECTED -> PAYMENT_CONFIRMED_PAID -> SHIPPING_IN_PROGRESS.

## 3. Tìm và ghi lại mọi đoạn AI/frontend/prompt tự tính final price, discount, member benefit, shipping fee, VAT, stock, sellable, order_code, paid hoặc revenue.

Nguyên tắc cấm:

- Không có QuoteSnapshot thì không render final price.

- QuoteSnapshot immutable và là nguồn sự thật duy nhất cho giá hiển thị.

- Order Draft không đồng nghĩa Official Order.

- Customer Confirmation mới cho phép tạo Official Order.

- Không có order_code thì không được nói đơn hàng đã ghi nhận chính thức.

- Payment selected không đồng nghĩa Paid.

- COD selected không paid.

- Bank transfer selected không paid.

- Payment Core confirmation mới là Paid.

- Paid không đồng nghĩa Verified Revenue.

- No ORDER_VERIFIED thì không final commission, không verified ROAS, không payout.

- AI không public tồn kho, không bán SKU not sellable, recall, sale lock hoặc channel suppressed.

Triển khai giới hạn:

## 1. Chuẩn hóa QuoteSnapshot consumer DTO và renderer.

## 2. Chuẩn hóa OrderDraft renderer dạng form/prefill, không chỉ text summary.

## 3. Chuẩn hóa Customer Confirmation adapter với actor, correlation_id, idempotency_key và audit.

## 4. Chuẩn hóa Official Order public renderer: chỉ hiển thị order_code nếu runtime trả order_code.

## 5. Chuẩn hóa Payment/Shipping public-safe renderer.

## 6. Thêm guard: QUOTE_REQUIRED, QUOTE_EXPIRED, SELLABLE_BLOCKED, MEMBER_BENEFIT_UNRESOLVED, ORDER_DRAFT_NOT_CONFIRMED, ORDER_CODE_MISSING, PAYMENT_UNCONFIRMED, HIGH_RISK_COMMAND_MISSING_IDEMPOTENCY, RECALL_SALE_LOCK_SUPPRESSION.

## 7. Thêm smoke test P4-2D-SMK-001 đến P4-2D-SMK-013.

Report bắt buộc 14 mục:

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

## P4-2D LOCAL STATUS: IMPLEMENTATION HANDOFF ONLY / LOCAL SMOKE REQUIRED.

P4-2D LOCAL STATUS: IMPLEMENTATION HANDOFF ONLY - cần dev inspect, triển khai giới hạn, chạy smoke và nộp evidence.

PHASE 4 COMPLETE: KHONG

IMPLEMENTATION COMPLETE: KHONG

Completion Decision: KHONG

Release Readiness: KHONG

Production Readiness: KHONG

Go-live Decision: KHONG

## PHASE-04-AI-ADVISOR-RUNTIME

## 06-CHẶN AN TOÀN NGÔN TỪ CÔNG KHAI VÀ TUYÊN BỐ



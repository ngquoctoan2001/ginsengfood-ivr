# P3.2C - CART ORDER DRAFT CUSTOMER CONFIRMATION

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- TECH-04 Commerce Runtime cart va order draft boundary.
- PACK-01 Operational Core cho upstream availability va sale lock.
- PACK-05 AI Advisor consume order draft, khong tu tao official order.
- Customer confirmation khong dong nghia official order.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md

## 27.2. Prompt tiếp theo

## PROMPT-P3.2C - CART / ORDER DRAFT / CUSTOMER CONFIRMATION LIMITED IMPLEMENTATION HANDOFF

## 27.3. Điều kiện sang P3.2C

Chỉ chuyển sang P3.2C nếu:

P3.2B đạt DONE GATE.

QuoteSnapshot hoạt động đúng.

Không có final price nếu thiếu QuoteSnapshot.

QuoteSnapshot immutable.

QuoteSnapshot có expiry/freeze window.

Listed price/program/member/diamond resolver đúng boundary.

Không có hardcode giá/discount/member benefit trong AI/Gateway.

Evidence submitted đầy đủ.

Owner đồng ý handoff.

## 27.4. Không chuyển nếu

QuoteSnapshot chưa xong.

QuoteSnapshot chưa immutable.

Final price vẫn có thể trả không cần QuoteSnapshot.

Member benefit còn tự suy diễn.

Program conflict chưa rõ.

Diamond referral còn tạo commission final.

Shipping/VAT bị hardcode.

Có scope creep sang Cart/Order.

Gateway bị mở.

## 28. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3.2B - PRICE / PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / QUOTE SNAPSHOT LIMITED IMPLEMENTATION HANDOFF

## MODE: LIMITED IMPLEMENTATION - PRICE / PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / QUOTE SNAPSHOT ONLY

Bạn đang thực hiện PHASE-03 - Commerce Runtime cho dự án Ginsengfood.

Nhiệm vụ duy nhất của bạn là triển khai hoặc chỉnh sửa phần runtime báo giá gồm Listed Price, Program Eligibility, Member Benefit, Diamond Referral và QuoteSnapshot.

Không được triển khai Cart.
Không được triển khai Order Draft.
Không được triển khai Official Order.
Không được triển khai Payment.
Không được triển khai Shipping fulfillment.
Không được triển khai Verified Revenue.
Không được mở Gateway.

## SOURCE-OF-TRUTH

Bám theo:

MASTER Governance.

PHASE-00 Foundation/RBAC/Audit/Evidence/Idempotency.

PHASE-01 Product/SKU/Recipe/Activation.

PHASE-02 Operational Core.

## PHASE-03/00-P3-ANALYSIS-ONLY.

## PHASE-03/01-P3-1-TECHNICAL-DESIGN-ONLY.

## PHASE-03/02-P3-2A-SELLABLE-GATE-RUNTIME.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## CORE RULES

QuoteSnapshot là nguồn sự thật cho giá cuối tại thời điểm báo giá.

Không có QuoteSnapshot thì không có final price.

Listed price không phải final price.

Program discount phải do runtime resolve.

Member benefit phải do runtime resolve.

Diamond referral chỉ snapshot eligibility trong P3.2B, không tạo commission final.

Shipping fee/VAT nếu có trong quote phải đến từ runtime/policy, không hardcode.

QuoteSnapshot phải immutable.

QuoteSnapshot phải có expiry/freeze window.

AI/Gateway/Ads/Live/CRM/IVR chỉ consume runtime, không tự tính giá/discount/member benefit.

## SCOPE IN

Triển khai/chỉnh:

ListedPriceDecision.

ListedPriceResolver.

ProgramEligibilityDecision.

ProgramEligibilityResolver.

Program priority/conflict.

MemberBenefitDecision.

MemberBenefitResolver.

DiamondReferralDecision.

DiamondReferralResolver.

QuoteSnapshot.

QuoteSnapshotLine.

QuoteAdjustmentSnapshot.

QuoteSnapshotService.

Quote API/DTO nếu P3.1 duyệt.

Migration/seed nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2B.

## SCOPE OUT

Không làm:

Cart.

Order Draft.

Customer Confirmation.

Official Order.

order_code.

Payment.

Shipping fulfillment.

Invoice issuance.

Verified Revenue.

Commission final.

Verified ROAS.

Payout.

## IVR.

Gateway.

## ALLOWED FILE CHANGE BOUNDARY

Chỉ sửa file thuộc boundary đã được P3.1/P3.2A duyệt.

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## EXECUTION STEPS

Xác nhận mode LIMITED IMPLEMENTATION - PRICE / PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / QUOTE SNAPSHOT ONLY.

Đọc P3.2A Done Report.

Xác định allowed file boundary.

Inspect code hiện tại về price/program/member/referral/quote.

Implement ListedPriceResolver.

Implement ProgramEligibilityResolver.

Implement MemberBenefitResolver.

Implement DiamondReferralResolver.

Implement QuoteSnapshotService.

Implement API/DTO nếu đã duyệt.

Implement migration/seed nếu đã duyệt.

Add/update test.

Run backend build/test.

Run frontend build nếu có ảnh hưởng UI.

Run smoke P3-SMK-2B.

Produce report 14 mục.

Handoff sang P3.2C nếu đạt DONE GATE.

## TEST / SMOKE REQUIRED

Bắt buộc kiểm tra:

No QuoteSnapshot -> no final price.

SKU not sellable -> reject quote.

Listed price missing/inactive -> reject quote.

Program eligible -> snapshot discount.

Program conflict -> resolve theo priority.

Member eligible -> snapshot benefit.

Identity missing -> không tự suy diễn member.

Diamond referral valid -> snapshot eligibility, no final commission.

Quote expired -> cannot use downstream.

Quote immutable.

AI/Gateway no hardcode price.

Runtime unavailable -> fail-safe.

## REQUIRED REPORT FORMAT - 14 MỤC

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

## DONE GATE

Done khi:

Chỉ sửa file trong allowed boundary.

QuoteSnapshot đúng rule.

Không có final price nếu thiếu QuoteSnapshot.

Listed price/program/member/diamond resolver đúng boundary.

QuoteSnapshot immutable.

QuoteSnapshot có expiry/freeze window.

AI/Gateway không tự tính giá/discount/member benefit.

Shipping/VAT nếu có không hardcode.

Test/smoke pass hoặc điểm chặn báo rõ.

Backend build pass hoặc điểm chặn báo rõ.

Evidence submitted.

Report đủ 14 mục.

Gateway vẫn bị chặn.

Không gọi Release Readiness/Production Readiness.

FAIL GATE

Fail nếu:

Sửa ngoài boundary.

Tự triển khai Cart/Order/Payment/Revenue.

Trả final price không cần QuoteSnapshot.

Listed price missing vẫn quote.

Program/member benefit hardcode.

Diamond referral tạo commission final.

QuoteSnapshot mutate được.

Shipping/VAT hardcode.

AI/Gateway tự tính giá.

Không có report 14 mục.

Mở Gateway.

Gọi Production Readiness/Go-live Decision.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_P3_2C_CART_ORDER_DRAFT_CUSTOMER_CONFIRMATION.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_P3_2A_SELLABLE_DONE_GATE.

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL.

## BLOCKED_TEST_OR_BUILD_FAILED.

## BLOCKED_QUOTE_SNAPSHOT_P0_RULE_FAILED.

NEXT FILE: PHASE-03-COMMERCE-RUNTIME/04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md

ĐIỀU KIỆN CHUYỂN TIẾP: Chỉ chuyển sang P3.2C sau khi QuoteSnapshot đạt DONE GATE, không còn final price nếu thiếu QuoteSnapshot, quote immutable, quote có expiry/freeze window, listed price/program/member/diamond resolver đúng boundary, không hardcode giá/quyền lợi trong AI/Gateway và owner đồng ý handoff.

PHASE-03-COMMERCE-RUNTIME/04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md

## PROMPT-P3.2C - CART / ORDER DRAFT / CUSTOMER CONFIRMATION LIMITED IMPLEMENTATION HANDOFF

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt triển khai giới hạn cho nhóm Cart / Order Draft / Customer Confirmation trong PHASE-03 - Commerce Runtime.

Mục tiêu là giao dev/Codex/Copilot triển khai hoặc chỉnh sửa đúng phạm vi luồng từ báo giá runtime sang bản nháp xác nhận đơn.

P3.2C chỉ xử lý:

Cart.

Cart Line.

Order Draft.

Order Draft Line.

Customer Confirmation.

Draft validation.

QuoteSnapshot attachment.

Confirmation idempotency.

Confirmation expiry validation.

Handoff sang P3.2D để tạo Official Order.

P3.2C không tạo Official Order.

P3.2C không tạo order_code chính thức.

P3.2C không set Paid.

P3.2C không tạo Verified Revenue.

P3.2C không mở Gateway.

## 3. MODE

## MODE: LIMITED IMPLEMENTATION - CART / ORDER DRAFT / CUSTOMER CONFIRMATION ONLY

Trong mode này, dev/Codex/Copilot chỉ được phép sửa code trong phạm vi Cart / Order Draft / Customer Confirmation đã được P3.1 và P3.2B handoff cho phép.

Được phép:

Thêm/sửa Cart domain.

Thêm/sửa CartLine domain.

Thêm/sửa OrderDraft domain.

Thêm/sửa OrderDraftLine domain.

Thêm/sửa CustomerConfirmation domain.

Thêm/sửa CartService.

Thêm/sửa OrderDraftService.

Thêm/sửa CustomerConfirmationService.

Gắn QuoteSnapshot vào Cart/Order Draft.

Validate QuoteSnapshot còn hiệu lực.

Validate Sellable Gate nếu policy yêu cầu recheck.

Thêm API/DTO cho Cart/Order Draft/Customer Confirmation nếu P3.1 duyệt.

Thêm migration/seed/config nếu P3.1 duyệt.

Thêm test/smoke cho P3.2C.

Chạy backend build/test.

Chạy frontend build nếu có thay UI/contract.

Trả report 14 mục.

Không được:

Tạo Official Order.

Tạo order_code chính thức.

Set order official state.

Set Payment selected nếu chưa thuộc boundary P3.2C.

Set Paid.

Confirm payment.

Tạo shipping request.

Tạo invoice thật.

Tạo verified revenue.

Tạo commission final.

Tạo verified ROAS.

Tạo payout.

Mở Gateway.

Gọi Release Readiness / Production Readiness / Go-live Decision.

## 4. SOURCE-OF-TRUTH

## 4.1. MASTER Governance

MASTER-01 - Global Source of Truth / Ownership Boundary.

MASTER-02 - Cross-Pack Dependency Graph.

MASTER-03 - Traceability Matrix Standard.

MASTER-04 - Dev Handoff Governance.

MASTER-05 - Implementation Execution Governance.

MASTER-06 - Quality / Test / Smoke / Evidence Governance.

MASTER-08 - Business Rule / Product / Member / Pricing / Program Governance.

MASTER-09 - Evidence / Completion / Gateway / Release Readiness Governance.

## 4.2. PHASE 0

Actor Context.

Correlation ID.

RBAC backend enforcement.

Audit append-only.

Evidence Registry.

Idempotency.

Runtime unavailable fail-safe.

Không mở Global Gate.

## 4.3. PHASE 1

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Seed không được tự set sellable.

Product/SKU/Recipe chỉ là foundation.

## 4.4. PHASE 2

Batch QC_PASS không đồng nghĩa RELEASED.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM, Gateway, IVR.

## 4.5. PHASE 3 - P3.2A

Sellable Gate là runtime gate riêng.

SellableDecision phải có block reason.

Runtime unavailable phải fail-safe.

Recall/Sale Lock phải chặn sellable.

## 4.6. PHASE 3 - P3.2B

QuoteSnapshot là nguồn sự thật cho giá cuối.

Không có QuoteSnapshot thì không có final price.

QuoteSnapshot immutable.

QuoteSnapshot phải có expiry/freeze window.

Program/member/diamond/shipping/VAT nếu có phải đến từ runtime/policy.

AI/Gateway không tự tính giá.

## 4.7. PHASE 3 - P3.2C

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới là điều kiện để P3.2D tạo Official Order.

Không có order_code thì không được nói "đơn hàng đã ghi nhận chính thức".

Customer Confirmation phải idempotent.

QuoteSnapshot hết hạn thì không được xác nhận.

## 5. ENTRY CONDITION

Chỉ được thực hiện prompt này nếu đã hoàn tất:

## 00-P3-ANALYSIS-ONLY.md.

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md.

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md.

## 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md.

Sellable Gate đạt DONE GATE.

QuoteSnapshot đạt DONE GATE.

Không còn final price nếu thiếu QuoteSnapshot.

QuoteSnapshot immutable.

QuoteSnapshot có expiry/freeze window.

Owner đồng ý handoff sang P3.2C.

Allowed file change boundary cho P3.2C đã rõ.

Nếu thiếu điều kiện, dev/Codex/Copilot phải dừng và trả trạng thái:

## BLOCKED_NEED_P3_2B_QUOTE_SNAPSHOT_DONE_GATE

## 6. OBJECTIVE

Mục tiêu của P3.2C là triển khai luồng tiền-order an toàn:

Khách chọn sản phẩm -> Cart.

Cart gắn Sellable/QuoteSnapshot hợp lệ.

Từ Cart tạo Order Draft.

Order Draft hiển thị đầy đủ thông tin cần khách xác nhận.

Customer Confirmation xác nhận bản nháp.

Confirmation hợp lệ được lưu làm điều kiện đầu vào cho P3.2D.

P3.2C không tạo Official Order.

P3.2C không tạo order_code chính thức.

P3.2C không tạo doanh thu.

P3.2C không xác nhận thanh toán.

## 7. CORE RULES

## 7.1. Cart không đồng nghĩa Order

Cart chỉ là giỏ hàng hoặc lựa chọn dự kiến.

Cart không được:

Có order_code chính thức.

Được gọi là đơn hàng đã ghi nhận.

Tạo revenue.

Tạo payment obligation chính thức.

Gửi MISA.

Gửi shipping request.

Ghi nhận commission/ROAS.

## 7.2. Order Draft không đồng nghĩa Official Order

Order Draft là bản nháp xác nhận đơn.

Order Draft không được:

Có order_code chính thức.

Được gọi là đơn đã ghi nhận chính thức.

Tự set paid.

Tự tạo shipment.

Tự tạo verified revenue.

Tự tạo commission.

Tự gửi Gateway như official order.

## 7.3. Customer Confirmation là gate bắt buộc

Customer Confirmation là điều kiện bắt buộc trước khi tạo Official Order ở P3.2D.

P3.2C chỉ lưu confirmation.

P3.2D mới tạo Official Order/order_code nếu confirmation hợp lệ.

## 7.4. QuoteSnapshot còn hiệu lực mới được xác nhận

Nếu QuoteSnapshot:

Không tồn tại.

Không active.

Đã expired.

Đã voided.

Bị policy chặn.

Bị recall/sale lock ảnh hưởng theo policy.

thì không được xác nhận Order Draft.

## 7.5. Không có order_code thì không được nói đơn chính thức

Trong P3.2C, hệ thống chỉ được nói:

"Bản nháp đơn".

"Thông tin đơn chờ xác nhận".

"Phiếu xác nhận nháp".

"Yêu cầu xác nhận đơn".

Không được nói:

"Đơn hàng đã ghi nhận chính thức".

"Đơn đã được tạo thành công" nếu chưa có Official Order.

"Mã đơn hàng" nếu chưa có order_code chính thức.

## 7.6. Customer Confirmation phải chống trùng

Nếu khách bấm xác nhận nhiều lần:

Không tạo nhiều confirmation active.

Không tạo nhiều order draft chuyển tiếp.

Không tạo nhiều Official Order ở phase sau.

Phải dùng idempotency/correlation theo convention.

## 8. SCOPE IN

P3.2C bao gồm:

Cart.

CartLine.

CartService.

Cart validation.

OrderDraft.

OrderDraftLine.

OrderDraftService.

CustomerConfirmation.

CustomerConfirmationService.

QuoteSnapshot validation.

Sellable recheck nếu policy yêu cầu.

Draft expiry.

Confirmation expiry validation.

Duplicate confirmation guard.

API/DTO nếu P3.1 duyệt.

Migration/seed/config nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2C.

Handoff payload sang P3.2D.

## 9. SCOPE OUT

P3.2C không bao gồm:

Không tạo Official Order.

Không tạo order_code chính thức.

Không tạo Order State Machine chính thức.

Không set Payment selected nếu chưa được P3.1 duyệt trong boundary này.

Không set Paid.

Không confirm payment.

Không tạo shipping request.

Không phát hành invoice.

Không tạo verified revenue.

Không tạo commission final.

Không tạo verified ROAS.

Không tạo payout.

Không triển khai IVR.

Không mở Gateway.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 10. ALLOWED FILE CHANGE BOUNDARY

Dev/Codex/Copilot chỉ được sửa file thuộc boundary đã được P3.1/P3.2B handoff cho phép.

## 10.1. Nhóm file được phép sửa nếu đã duyệt

Nhóm file

Được sửa

Ghi chú

Commerce Cart domain

Có

Cart/CartLine/state

Commerce Cart service

Có

Create/update/validate

Commerce Order Draft domain

Có

Draft/DraftLine/state

Commerce Order Draft service

Có

Create/update/expire

Customer Confirmation domain

Có

Confirmation state

Customer Confirmation service

Có

Submit/validate/idempotency

API/DTO Cart/Draft/Confirmation

Có điều kiện

Chỉ nếu P3.1 duyệt

Migration

Có điều kiện

Chỉ Cart/Draft/Confirmation

Seed/config dev-test

Có điều kiện

Không tạo official order

Backend tests

Có

Chỉ P3.2C

Frontend/Admin UI

Có điều kiện

Chỉ nếu P3.1 duyệt

Markdown/handoff

Có điều kiện

Chỉ cập nhật P3.2C report

## 10.2. Nhóm file không được sửa

Nhóm file

Không được sửa

Lý do

Official Order implementation

Không

Thuộc P3.2D

order_code generator

Không

Thuộc P3.2D

Payment implementation

Không

Thuộc P3.2E

Payment confirmation/callback

Không

Thuộc P3.2E

Shipping fulfillment

Không

Thuộc P3.2E

Invoice issuance

Không

Thuộc P3.2E

Verified Revenue

Không

Thuộc P3.2F

Commission/ROAS/Payout

Không

Thuộc P3.2F

Gateway config

Không

IVR implementation

Không

PACK-09 reserved

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## 11. REQUIRED INPUT

## 11.1. Từ P3.2A

SellableGateResolver.

SellableDecision.

sellable_status.

allow_quote.

allow_cart.

allow_order_draft.

block_reasons.

channel suppression status.

runtime policy version.

## 11.2. Từ P3.2B

QuoteSnapshot.

QuoteSnapshotLine.

final_payable_amount.

quote_status.

expires_at.

freeze window.

policy version.

public display payload.

internal decision refs nếu có permission.

## 11.3. Từ customer/channel context

customer_id nullable.

session_id.

channel.

customer name.

phone.

delivery address.

receiver name nếu khác người mua.

receiver phone nếu khác người mua.

note.

selected confirmation method.

UI/button confirmation event nếu có.

text confirmation fallback nếu channel chỉ-text.

## 11.4. Từ policy

Draft expiry policy.

Required customer fields.

Required delivery fields.

Confirmation method policy.

Duplicate confirmation policy.

Recheck sellable before confirmation policy.

Requote policy nếu QuoteSnapshot expired.

## 12. TARGET BEHAVIOR

## 12.1. Cart created

Cart được tạo khi:

Có SKU/quantity hợp lệ.

SKU sellable cho mục đích cart.

QuoteSnapshot active nếu cart hiển thị final price.

requested quantity hợp lệ.

channel hợp lệ.

Không recall/sale lock tại thời điểm validate.

Cart created không tạo order_code.

## 12.2. Cart update

Cart update phải:

Revalidate line.

Revalidate quantity.

Revalidate QuoteSnapshot nếu giá/tổng tiền bị ảnh hưởng.

Không mutate QuoteSnapshot cũ.

Nếu thay đổi line/quantity làm thay đổi giá, tạo hoặc yêu cầu QuoteSnapshot mới theo policy.

## 12.3. Order Draft created

Order Draft được tạo khi:

Có QuoteSnapshot active.

Có cart hoặc direct quote-to-draft theo policy.

Có thông tin khách tối thiểu.

Có thông tin nhận hàng tối thiểu nếu policy yêu cầu.

Có total payable từ QuoteSnapshot.

Draft state là WAITING_CUSTOMER_CONFIRMATION hoặc DRAFT_CREATED theo convention.

Order Draft created không tạo Official Order.

## 12.4. Customer Confirmation submitted

Customer Confirmation được submit khi khách thực hiện một hành động xác nhận hợp lệ:

Bấm nút "Xác nhận đơn".

Chọn CTA xác nhận.

Gửi text confirmation nếu kênh chỉ-text.

Xác nhận qua agent/admin thay khách nếu có permission và audit.

## 12.5. Customer Confirmation accepted

Confirmation accepted khi:

order_draft_id tồn tại.

quote_snapshot_id tồn tại.

QuoteSnapshot active và chưa expired.

Order Draft chưa expired/cancelled.

Confirmation chưa bị duplicate.

Required fields đủ.

Sellable recheck pass nếu policy yêu cầu.

No recall/sale lock phát sinh nếu policy yêu cầu.

Idempotency pass.

## 12.6. Customer Confirmation rejected

Confirmation phải bị reject nếu:

QuoteSnapshot expired.

QuoteSnapshot voided.

Order Draft expired.

Order Draft cancelled.

Required customer info missing.

Duplicate confirmation.

Sellable changed to not sellable.

Recall/sale lock active.

Runtime policy unavailable.

Invalid confirmation method.

Channel not allowed.

## 13. DOMAIN CONTRACT

## 13.1. Cart

Trường đề xuất:

cart_id.

customer_id nullable.

session_id.

channel.

cart_status.

currency.

quote_snapshot_id nullable.

created_at.

updated_at.

expires_at nullable.

correlation_id.

actor_context nullable.

audit_ref nullable.

CartStatus

## ACTIVE.

## UPDATED.

## ABANDONED.

## EXPIRED.

## CONVERTED_TO_ORDER_DRAFT.

## CANCELLED.

Rule:

Cart không được có order_code.

Cart không được gọi là official order.

## 13.2. CartLine

Trường đề xuất:

cart_line_id.

cart_id.

sku_id.

product_id.

quantity.

uom.

sellable_decision_ref.

quote_snapshot_line_ref nullable.

line_status.

created_at.

updated_at.

CartLineStatus

## ACTIVE.

## REMOVED.

## QUANTITY_UPDATED.

## NOT_SELLABLE_BLOCKED.

## QUOTE_EXPIRED_REQUOTE_REQUIRED.

## 13.3. OrderDraft

Trường đề xuất:

order_draft_id.

draft_code optional, không phải order_code chính thức.

cart_id nullable.

quote_snapshot_id.

customer_id nullable.

session_id.

channel.

buyer_name.

buyer_phone.

receiver_name nullable.

receiver_phone nullable.

delivery_address.

delivery_note nullable.

payment_method_preference nullable nếu P3.1 duyệt.

order_draft_status.

total_payable_snapshot.

currency.

created_at.

expires_at.

confirmed_at nullable.

correlation_id.

actor_context nullable.

audit_ref.

OrderDraftStatus

## DRAFT_CREATED.

## WAITING_CUSTOMER_CONFIRMATION.

## CUSTOMER_CONFIRMED.

## EXPIRED.

## CANCELLED.

## CONVERTED_TO_OFFICIAL_ORDER.

Rule:

CONVERTED_TO_OFFICIAL_ORDER chỉ được set ở P3.2D sau khi Official Order tạo thành công.

P3.2C chỉ được đưa đến CUSTOMER_CONFIRMED.

## 13.4. OrderDraftLine

Trường đề xuất:

order_draft_line_id.

order_draft_id.

quote_snapshot_line_id.

sku_id.

product_public_name_snapshot.

quantity.

listed_unit_price_snapshot.

line_discount_snapshot.

line_tax_snapshot nullable.

final_line_amount_snapshot.

line_status.

Rule:

OrderDraftLine lấy giá từ QuoteSnapshotLine.

Không tự tính lại final price trong Order Draft.

## 13.5. CustomerConfirmation

Trường đề xuất:

customer_confirmation_id.

order_draft_id.

quote_snapshot_id.

confirmation_status.

confirmation_method.

confirmed_by_customer_id nullable.

confirmed_by_session_id.

confirmed_by_actor_id nullable nếu admin/agent thao tác.

confirmed_at.

channel.

confirmation_payload_hash optional.

idempotency_key.

correlation_id.

audit_ref.

rejection_reason nullable.

CustomerConfirmationStatus

## SUBMITTED.

## ACCEPTED.

## REJECTED_EXPIRED_QUOTE.

## REJECTED_VOIDED_QUOTE.

## REJECTED_EXPIRED_DRAFT.

## REJECTED_DUPLICATE.

## REJECTED_REQUIRED_INFO_MISSING.

## REJECTED_SELLABLE_CHANGED.

## REJECTED_RECALL_OR_SALE_LOCK.

## REJECTED_RUNTIME_UNAVAILABLE.

## REJECTED_INVALID_METHOD.

ConfirmationMethod

## BUTTON_CTA.

## WEB_FORM_SUBMIT.

## MESSENGER_CTA.

## TEXT_CONFIRMATION.

## ADMIN_ASSISTED_WITH_PERMISSION.

## FUTURE_IVR_RESERVED.

Rule:

FUTURE_IVR_RESERVED chỉ là placeholder, không triển khai IVR logic trong P3.2C.

## 14. SERVICE CONTRACT

## 14.1. CartService

CreateCart input

customer_id nullable.

session_id.

channel.

lines:

sku_id.

quantity.

quote_snapshot_id nullable.

correlation_id.

idempotency_key nếu create command.

CreateCart output

cart_id.

cart_status.

lines.

quote_snapshot_id nullable.

validation_result.

block_reasons nếu có.

CreateCart rules

Validate SKU exists.

Validate quantity.

Call SellableGateResolver với purpose CART.

Nếu quote_snapshot_id có, validate QuoteSnapshot active.

Không tạo Order Draft nếu chưa được gọi explicit.

Không tạo Official Order.

## 14.2. OrderDraftService

CreateOrderDraft input

cart_id nullable.

quote_snapshot_id.

customer_id nullable.

session_id.

channel.

buyer info.

receiver info.

delivery address.

note nullable.

payment_method_preference nullable nếu được duyệt.

correlation_id.

idempotency_key.

CreateOrderDraft output

order_draft_id.

order_draft_status.

quote_snapshot_id.

total_payable_snapshot.

public_confirmation_payload.

expires_at.

required_missing_fields nếu có.

CreateOrderDraft rules

QuoteSnapshot bắt buộc.

QuoteSnapshot phải active.

QuoteSnapshot chưa expired.

QuoteSnapshot không voided.

Không mutate QuoteSnapshot.

Required customer/delivery fields phải đủ hoặc draft ở trạng thái incomplete theo policy.

Không tạo order_code.

Không tạo Official Order.

## 14.3. CustomerConfirmationService

SubmitConfirmation input

order_draft_id.

quote_snapshot_id.

confirmation_method.

confirmation_payload.

customer_id nullable.

session_id.

channel.

correlation_id.

idempotency_key.

SubmitConfirmation output

customer_confirmation_id.

confirmation_status.

order_draft_status.

accepted.

rejection_reason nullable.

handoff_ready_for_official_order.

handoff_payload_ref nếu accepted.

SubmitConfirmation rules

Validate Order Draft exists.

Validate QuoteSnapshot matches draft.

Validate QuoteSnapshot active.

Validate quote not expired.

Validate draft not expired.

Validate duplicate confirmation.

Validate required fields.

Recheck Sellable/Recall/Sale Lock nếu policy yêu cầu.

Store accepted confirmation.

Mark draft CUSTOMER_CONFIRMED nếu accepted.

Không tạo Official Order trong P3.2C.

Trả handoff_ready_for_official_order = true nếu đủ điều kiện cho P3.2D.

## 15. STATE MACHINE

## 15.1. Cart state transition

Allowed:

## ACTIVE -> UPDATED.

## ACTIVE -> ABANDONED.

## ACTIVE -> EXPIRED.

## ACTIVE -> CONVERTED_TO_ORDER_DRAFT.

## UPDATED -> CONVERTED_TO_ORDER_DRAFT.

## UPDATED -> EXPIRED.

## ACTIVE/UPDATED -> CANCELLED.

Forbidden:

Cart -> Official Order.

Cart -> Paid.

Cart -> Verified Revenue.

Cart -> order_code.

## 15.2. Order Draft state transition

Allowed in P3.2C:

## DRAFT_CREATED -> WAITING_CUSTOMER_CONFIRMATION.

## WAITING_CUSTOMER_CONFIRMATION -> CUSTOMER_CONFIRMED.

## DRAFT_CREATED -> EXPIRED.

## WAITING_CUSTOMER_CONFIRMATION -> EXPIRED.

## DRAFT_CREATED -> CANCELLED.

## WAITING_CUSTOMER_CONFIRMATION -> CANCELLED.

Reserved for P3.2D:

## CUSTOMER_CONFIRMED -> CONVERTED_TO_OFFICIAL_ORDER.

Forbidden in P3.2C:

Order Draft -> Official Order.

Order Draft -> order_code.

Order Draft -> Paid.

Order Draft -> Verified Revenue.

## 15.3. Customer Confirmation state transition

Allowed:

## SUBMITTED -> ACCEPTED.

## SUBMITTED -> REJECTED_EXPIRED_QUOTE.

## SUBMITTED -> REJECTED_VOIDED_QUOTE.

## SUBMITTED -> REJECTED_EXPIRED_DRAFT.

## SUBMITTED -> REJECTED_DUPLICATE.

## SUBMITTED -> REJECTED_REQUIRED_INFO_MISSING.

## SUBMITTED -> REJECTED_SELLABLE_CHANGED.

## SUBMITTED -> REJECTED_RECALL_OR_SALE_LOCK.

## SUBMITTED -> REJECTED_RUNTIME_UNAVAILABLE.

## SUBMITTED -> REJECTED_INVALID_METHOD.

Forbidden:

ACCEPTED -> mutate payload.

ACCEPTED -> create Official Order trong P3.2C.

ACCEPTED -> set paid.

ACCEPTED -> verified revenue.

## 16. API / DTO CONTRACT - IF APPROVED BY P3.1

## 16.1. Create Cart API

POST /api/commerce/carts

Request:

channel.

customer_id nullable.

session_id.

lines:

sku_id.

quantity.

quote_snapshot_id nullable.

idempotency_key.

correlation_id.

Response:

cart_id.

cart_status.

lines.

validation_result.

quote_snapshot_id nullable.

block_reasons nullable.

created_at.

expires_at nullable.

## 16.2. Update Cart API

PATCH /api/commerce/carts/{cart_id}

Request:

lines to add/update/remove.

quote_snapshot_id nullable.

idempotency_key.

correlation_id.

Response:

cart_id.

cart_status.

lines.

requote_required flag.

block_reasons nullable.

## 16.3. Create Order Draft API

POST /api/commerce/order-drafts

Request:

cart_id nullable.

quote_snapshot_id.

customer_id nullable.

session_id.

channel.

buyer_name.

buyer_phone.

receiver_name nullable.

receiver_phone nullable.

delivery_address.

delivery_note nullable.

payment_method_preference nullable nếu duyệt.

idempotency_key.

correlation_id.

Response:

order_draft_id.

order_draft_status.

quote_snapshot_id.

total_payable_snapshot.

public_confirmation_payload.

missing_required_fields nullable.

expires_at.

no_order_code_yet = true.

## 16.4. Read Order Draft API

GET /api/commerce/order-drafts/{order_draft_id}

Response:

order_draft_id.

order_draft_status.

quote_snapshot_id.

lines.

buyer/receiver/delivery summary.

total_payable_snapshot.

expires_at.

confirmation_status nullable.

no_order_code_yet = true nếu chưa P3.2D.

## 16.5. Customer Confirmation API

POST /api/commerce/order-drafts/{order_draft_id}/confirm

Request:

quote_snapshot_id.

confirmation_method.

confirmation_payload.

customer_id nullable.

session_id.

channel.

idempotency_key.

correlation_id.

Response:

customer_confirmation_id.

confirmation_status.

order_draft_status.

accepted.

rejection_reason nullable.

handoff_ready_for_official_order.

no_order_code_yet = true.

next_step = CREATE_OFFICIAL_ORDER_IN_P3_2D nếu accepted.

## 16.6. Public/internal response boundary

Public/channel response được phép có:

Product public name.

Quantity.

Final payable amount từ QuoteSnapshot.

Delivery info summary.

Customer confirmation instruction.

Expiry time.

Draft status.

"Chưa phải đơn chính thức" nếu cần làm rõ.

Không được lộ:

Internal cost.

Internal policy id.

Internal margin.

Supplier/QC defect.

Private recall note.

Internal commission.

Internal audit note.

Sensitive customer data không cần thiết.

## 17. DATABASE / MIGRATION RULE

Nếu P3.1 duyệt migration, bảng/entity P3.2C có thể gồm:

commerce_cart.

commerce_cart_line.

commerce_order_draft.

commerce_order_draft_line.

commerce_customer_confirmation.

commerce_confirmation_idempotency nếu cần theo convention.

commerce_draft_state_transition_log nếu chưa có state log chung.

Không tạo bảng:

commerce_order chính thức.

commerce_order_code.

commerce_payment.

commerce_payment_confirmation.

commerce_shipping_request.

commerce_invoice.

commerce_verified_revenue.

commerce_commission.

commerce_roas.

commerce_payout.

## 17.1. Migration safety

Migration phải:

Không phá dữ liệu cũ.

Không tạo order chính thức.

Không tạo order_code.

Có index cho cart_id, order_draft_id, quote_snapshot_id, customer_id/session_id, expires_at.

Có constraint trạng thái hợp lệ.

Có correlation/audit fields theo convention.

Hỗ trợ mở rộng nhiều SKU, không hardcode 20 SKU.

Không mở Gateway.

## 18. SEED / CONFIG RULE

Seed/config dev-test được phép tạo:

Draft expiry policy.

Required field policy.

Confirmation method sample.

Negative fixture:

expired quote.

expired draft.

duplicate confirmation.

missing required fields.

sellable changed.

Test cart/order draft sample.

Seed/config không được:

Tạo Official Order.

Tạo order_code.

Set paid.

Tạo verified revenue.

Tạo commission.

Tạo ROAS.

Tạo payout.

Mở Gateway.

Bypass QuoteSnapshot.

Bypass Customer Confirmation.

## 19. SECURITY / GOVERNANCE GUARDRAILS

## 19.1. RBAC

Bắt buộc backend permission cho:

Admin view internal draft.

Admin assisted confirmation.

Cancel/expire draft thủ công.

Export draft/confirmation evidence.

View internal customer confirmation payload nếu sensitive.

## 19.2. Actor Context

Bắt buộc cho:

Admin assisted confirmation.

Manual cancel draft.

Manual expire draft.

Any override.

## 19.3. Correlation ID

Mọi command phải có correlation_id:

Create cart.

Update cart.

Create order draft.

Submit confirmation.

Cancel/expire draft.

## 19.4. Audit append-only

Audit bắt buộc cho:

Cart created/updated.

Order Draft created/updated.

Customer Confirmation submitted.

Customer Confirmation accepted/rejected.

Draft expired/cancelled.

Any admin-assisted action.

## 19.5. Idempotency

Idempotency bắt buộc cho:

Create Cart nếu side effect.

Update Cart nếu side effect.

Create Order Draft.

Submit Customer Confirmation.

## 19.6. Evidence Registry

Evidence submitted chưa phải accepted.

Không gọi Completion Decision nếu chưa owner review.

## 19.7. Runtime unavailable

Nếu runtime unavailable:

Không tạo Order Draft nếu quote/sellable không validate được.

Không accept Customer Confirmation.

Không chuyển handoff_ready_for_official_order.

Không tự tạo Official Order.

## 20. TEST REQUIREMENTS

## 20.1. Unit test - Cart

Create cart with sellable SKU -> ACTIVE.

Create cart with not sellable SKU -> reject/block line.

Create cart with invalid quantity -> reject.

Update quantity requiring requote -> requote_required.

Cart does not create order_code.

Cart does not create Official Order.

Cart expired -> cannot convert to draft.

## 20.2. Unit test - Order Draft

Create draft with active QuoteSnapshot -> WAITING_CUSTOMER_CONFIRMATION.

Create draft without QuoteSnapshot -> reject.

Create draft with expired QuoteSnapshot -> reject.

Create draft with voided QuoteSnapshot -> reject.

Create draft missing required fields -> incomplete/reject theo policy.

Order Draft does not create order_code.

Order Draft does not create Official Order.

Order Draft stores total from QuoteSnapshot, not recalculated.

## 20.3. Unit test - Customer Confirmation

Confirm valid draft -> ACCEPTED.

Confirm expired quote -> REJECTED_EXPIRED_QUOTE.

Confirm voided quote -> REJECTED_VOIDED_QUOTE.

Confirm expired draft -> REJECTED_EXPIRED_DRAFT.

Confirm duplicate -> REJECTED_DUPLICATE hoặc same accepted result theo idempotency convention.

Confirm missing required info -> REJECTED_REQUIRED_INFO_MISSING.

Confirm sellable changed -> REJECTED_SELLABLE_CHANGED.

Confirm recall/sale lock active -> REJECTED_RECALL_OR_SALE_LOCK.

Runtime unavailable -> REJECTED_RUNTIME_UNAVAILABLE.

Accepted confirmation does not create Official Order in P3.2C.

## 20.4. Integration test nếu có API

POST cart success.

POST cart not sellable reject.

POST order draft success.

POST order draft expired quote reject.

POST confirmation success.

POST confirmation duplicate no duplicate effect.

GET order draft no official order_code.

Public response does not leak internal data.

## 21. SMOKE REQUIREMENTS

## 21.1. P3-SMK-2C-001 - Cart is not Order

Case: Create cart thành công.
Expected: Không có order_code, không có Official Order.

## 21.2. P3-SMK-2C-002 - Not sellable cannot cart

Case: SKU not sellable.
Expected: Không tạo cart line active hoặc reject theo policy.

## 21.3. P3-SMK-2C-003 - QuoteSnapshot required for Order Draft

Case: Create Order Draft không có QuoteSnapshot.
Expected: Reject.

## 21.4. P3-SMK-2C-004 - Expired quote rejected

Case: QuoteSnapshot expired.
Expected: Không tạo/không confirm Order Draft.

## 21.5. P3-SMK-2C-005 - Draft is not Official Order

Case: Create Order Draft thành công.
Expected: Không có order_code, không tạo Official Order.

## 21.6. P3-SMK-2C-006 - Customer Confirmation accepted

Case: Valid draft + active quote + đủ thông tin.
Expected: Confirmation ACCEPTED, draft CUSTOMER_CONFIRMED, handoff_ready_for_official_order = true.

## 21.7. P3-SMK-2C-007 - Duplicate confirmation bị chặn

Case: Submit confirmation hai lần cùng idempotency key/payload.
Expected: Không tạo duplicate effect.

## 21.8. P3-SMK-2C-008 - Recall/Sale Lock before confirmation

Case: Recall hoặc sale lock phát sinh trước confirmation.
Expected: Confirmation rejected.

## 21.9. P3-SMK-2C-009 - Required info missing

Case: Thiếu phone/address theo policy.
Expected: Draft incomplete hoặc confirmation rejected.

## 21.10. P3-SMK-2C-010 - No Official Order in P3.2C

Case: Confirmation accepted.
Expected: Không tạo Official Order/order_code trong P3.2C.

## 21.11. P3-SMK-2C-011 - Public response safe

Case: Public/channel read draft/confirmation payload.
Expected: Không lộ internal policy/cost/commission/private notes.

## 21.12. P3-SMK-2C-012 - Gateway remains bị chặn

Case: Sau khi confirmation accepted.
Expected: Gateway vẫn bị chặn; không publish như release-ready.

## 22. EVIDENCE REQUIREMENTS

P3.2C cần gom evidence:

Git diff trong allowed boundary.

File list đã sửa.

Migration diff nếu có.

Seed/config diff nếu có.

Unit test result.

Integration test result nếu có API.

Smoke result P3-SMK-2C-001 -> P3-SMK-2C-012.

Backend build result.

Frontend build result nếu có ảnh hưởng UI.

Cart not order evidence.

Order Draft not official evidence.

No order_code evidence.

Customer Confirmation idempotency evidence.

Expired quote rejection evidence.

Duplicate confirmation evidence.

Recall/Sale Lock rejection evidence.

Public/internal boundary evidence.

Handoff note sang P3.2D.

Lưu ý:

Evidence submitted chưa phải Evidence accepted.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

## 23. EXECUTION STEPS

Step 1 - Confirm Mode

Xác nhận:

## MODE: LIMITED IMPLEMENTATION - CART / ORDER DRAFT / CUSTOMER CONFIRMATION ONLY

Step 2 - Read P3.2B Done Report

Xác nhận QuoteSnapshot đạt DONE GATE.

Nếu chưa đạt, dừng.

Step 3 - Inspect Existing Cart/Order Draft Logic

Kiểm tra codebase hiện tại:

Có cart cũ không.

Có order draft cũ không.

Có flow nào tạo order quá sớm không.

Có flow nào tạo order_code trước confirmation không.

Có flow nào xem cart là order không.

Có flow nào xem draft là official order không.

Có confirmation CTA không.

Có idempotency cho confirmation không.

Step 4 - Implement Cart Contract

Triển khai/chỉnh:

Cart.

CartLine.

CartService.

Cart validation.

Sellable validation.

QuoteSnapshot attachment nếu có.

Step 5 - Implement Order Draft Contract

Triển khai/chỉnh:

OrderDraft.

OrderDraftLine.

OrderDraftService.

Draft expiry.

Required fields.

QuoteSnapshot validation.

Public confirmation payload.

Step 6 - Implement Customer Confirmation Contract

Triển khai/chỉnh:

CustomerConfirmation.

CustomerConfirmationService.

Duplicate guard.

Expired quote guard.

Recall/Sale Lock guard nếu policy yêu cầu.

Idempotency.

Confirmation accepted/rejected state.

Step 7 - Implement API/DTO nếu approved

Nếu P3.1 duyệt, triển khai:

Create Cart API.

Update Cart API.

Create Order Draft API.

Read Order Draft API.

Customer Confirmation API.

Nếu chưa duyệt, không tự tạo API.

Step 8 - Implement Migration/Seed nếu approved

Chỉ trong phạm vi Cart/Draft/Confirmation.

Không tạo Official Order/order_code/payment/revenue migration.

Step 9 - Add Tests

Thêm/cập nhật test theo mục Test Requirements.

Step 10 - Run Build/Test/Smoke

Chạy:

Backend build.

Backend test.

Integration test nếu có.

Frontend build nếu ảnh hưởng UI.

Smoke P3-SMK-2C.

Step 11 - Evidence Report

Gom evidence và trả report 14 mục.

Step 12 - Handoff

Kết luận có được chuyển sang P3.2D hay không.

## 24. REQUIRED REPORT FORMAT - 14 MỤC

## 24.1. Tóm tắt

Ghi rõ:

Đã triển khai Cart phần nào.

Đã triển khai Order Draft phần nào.

Đã triển khai Customer Confirmation phần nào.

Test/smoke kết quả thế nào.

Có đủ điều kiện sang P3.2D không.

## 24.2. File đã sửa

Liệt kê:

File path.

Loại thay đổi.

Lý do sửa.

Thuộc allowed boundary nào.

## 24.3. Nguồn yêu cầu

Liệt kê:

## MASTER.

## PHASE 0.

## PHASE 1.

## PHASE 2.

P3 Analysis.

P3.1 Technical Design.

P3.2A Done Report.

P3.2B Done Report.

Prompt P3.2C.

## 24.4. Evidence đã dùng

Liệt kê:

Code references.

DB/migration references.

QuoteSnapshot evidence.

Sellable evidence.

Test references.

Smoke evidence.

API response/log nếu có.

Audit/evidence registry nếu có.

## 24.5. Lệnh đã chạy

Ghi rõ:

Build.

Test.

Smoke.

Lint.

Migration validation nếu có.

Seed validation nếu có.

## 24.6. Kết quả test

Ghi:

Test pass.

Test fail.

Fail xử lý ra sao.

Coverage còn thiếu.

## 24.7. Kết quả backend build

Ghi:

Build command.

Kết quả.

Log summary.

Error nếu có.

## 24.8. Kết quả frontend build

Ghi:

Có cần chạy không.

Nếu có, command và kết quả.

Nếu không, lý do.

## 24.9. Kết quả cleanup process

Ghi:

Có file tạm không.

Có cleanup không.

Có process treo không.

Có artifact cần giữ làm evidence không.

## 24.10. Cập nhật Markdown

Ghi:

Markdown nào đã cập nhật nếu có.

Nếu không cập nhật, ghi rõ.

Handoff note có cập nhật không.

## 24.11. Kết quả database migration/update nếu áp dụng

Ghi:

Có migration không.

Migration name.

Apply result.

Rollback/down result nếu có.

DB validation.

Không tạo order/order_code/payment/revenue.

## 24.12. Kết quả seed validation nếu áp dụng

Ghi:

Có seed không.

Seed có tạo official order không.

Seed có tạo order_code không.

Seed có set paid/revenue không.

Seed validation result.

## 24.13. Rủi ro còn lại

Phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Owner decision needed.

Deferred to P3.2D/P3.2E/P3.2F.

Technical debt.

## 24.14. Cập nhật handoff

Ghi rõ:

Có được sang P3.2D không.

Điều kiện còn thiếu nếu chưa.

File tiếp theo.

Handoff notes cho Official Order/order_code/state machine.

## 25. DONE GATE

P3.2C được DONE khi đủ:

Chỉ sửa file trong allowed boundary.

Cart tồn tại đúng rule.

Cart không tạo Official Order.

Cart không tạo order_code.

Order Draft tồn tại đúng rule.

Order Draft bắt buộc gắn QuoteSnapshot.

Order Draft không tạo Official Order.

Order Draft không tạo order_code chính thức.

Customer Confirmation tồn tại đúng rule.

Confirmation validate QuoteSnapshot active.

Confirmation reject expired/voided quote.

Confirmation reject expired draft.

Confirmation reject duplicate.

Confirmation reject missing required info theo policy.

Confirmation reject recall/sale lock nếu policy yêu cầu.

Confirmation accepted chỉ tạo handoff_ready_for_official_order.

Không set paid.

Không tạo verified revenue.

Public/internal response boundary đúng.

Idempotency cho confirmation.

Audit/correlation id đầy đủ.

Test bắt buộc pass hoặc điểm chặn rõ.

Smoke P3-SMK-2C pass hoặc điểm chặn rõ.

Backend build pass hoặc điểm chặn rõ.

Frontend build pass nếu có ảnh hưởng UI.

Evidence submitted.

Report đủ 14 mục.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 26. FAIL GATE

P3.2C bị FAIL nếu:

Sửa file ngoài allowed boundary.

Tự tạo Official Order.

Tự tạo order_code.

Cart bị xem là Order.

Order Draft bị xem là Official Order.

Customer Confirmation bị bỏ qua.

QuoteSnapshot expired vẫn được confirm.

QuoteSnapshot missing vẫn tạo Order Draft.

Confirmation duplicate tạo duplicate effect.

Confirmation accepted nhưng tạo luôn Official Order trong P3.2C.

Set paid trong P3.2C.

Tạo shipping request trong P3.2C.

Tạo verified revenue trong P3.2C.

Tạo commission/ROAS/payout.

Public response lộ internal data.

Không có test negative case.

Không có report 14 mục.

Mở Gateway.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

## 27. DOWNSTREAM HANDOFF

## 27.1. File tiếp theo

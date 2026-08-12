# P3.2D - OFFICIAL ORDER ORDER CODE STATE MACHINE

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- TECH-04 Commerce Runtime official order boundary.
- PACK-04 MISA Accounting Handoff chi nhan du lieu sau checkpoint hop le.
- TECH-01 RBAC / Audit / Idempotency.
- Order Draft khong dong nghia Official Order.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md

## 27.2. Prompt tiếp theo

## PROMPT-P3.2D - OFFICIAL ORDER / ORDER CODE / STATE MACHINE LIMITED IMPLEMENTATION HANDOFF

## 27.3. Điều kiện sang P3.2D

Chỉ chuyển sang P3.2D nếu:

P3.2C đạt DONE GATE.

Cart không bị hiểu là Order.

Order Draft không bị hiểu là Official Order.

Customer Confirmation accepted đúng rule.

Confirmation idempotent.

QuoteSnapshot active được validate.

Expired quote bị reject.

No order_code trong P3.2C.

Handoff payload cho P3.2D rõ.

Evidence submitted đầy đủ.

Owner đồng ý handoff.

## 27.4. Không chuyển nếu

Order Draft chưa gắn QuoteSnapshot.

Confirmation chưa idempotent.

Confirmation accepted tạo luôn order.

Draft có order_code chính thức.

Quote expired vẫn confirm được.

Recall/Sale Lock không chặn confirmation khi policy yêu cầu.

Có scope creep sang Payment/Revenue.

Gateway bị mở.

## 28. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3.2C - CART / ORDER DRAFT / CUSTOMER CONFIRMATION LIMITED IMPLEMENTATION HANDOFF

## MODE: LIMITED IMPLEMENTATION - CART / ORDER DRAFT / CUSTOMER CONFIRMATION ONLY

Bạn đang thực hiện PHASE-03 - Commerce Runtime cho dự án Ginsengfood.

Nhiệm vụ duy nhất của bạn là triển khai hoặc chỉnh sửa Cart, Order Draft và Customer Confirmation.

Không được tạo Official Order.
Không được tạo order_code chính thức.
Không được set Paid.
Không được tạo Shipping Request.
Không được tạo Verified Revenue.
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

## PHASE-03/03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## CORE RULES

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới là điều kiện cho P3.2D tạo Official Order.

Không có order_code thì không được nói đơn hàng đã ghi nhận chính thức.

Order Draft phải gắn QuoteSnapshot active.

QuoteSnapshot expired/voided thì không được xác nhận.

Confirmation phải idempotent.

Accepted confirmation không tạo Official Order trong P3.2C.

Gateway vẫn bị chặn.

## SCOPE IN

Triển khai/chỉnh:

Cart.

CartLine.

CartService.

OrderDraft.

OrderDraftLine.

OrderDraftService.

CustomerConfirmation.

CustomerConfirmationService.

QuoteSnapshot validation.

Duplicate confirmation guard.

Draft expiry.

API/DTO nếu P3.1 duyệt.

Migration/seed nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2C.

## SCOPE OUT

Không làm:

Official Order.

order_code.

Payment.

Paid status.

Shipping request.

Invoice issuance.

Verified Revenue.

Commission.

## ROAS.

Payout.

## IVR.

Gateway.

## ALLOWED FILE CHANGE BOUNDARY

Chỉ sửa file thuộc boundary đã được P3.1/P3.2B duyệt.

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## EXECUTION STEPS

Xác nhận mode LIMITED IMPLEMENTATION - CART / ORDER DRAFT / CUSTOMER CONFIRMATION ONLY.

Đọc P3.2B Done Report.

Xác định allowed file boundary.

Inspect code hiện tại về cart/draft/confirmation.

Implement Cart contract.

Implement Order Draft contract.

Implement Customer Confirmation contract.

Implement API/DTO nếu đã duyệt.

Implement migration/seed nếu đã duyệt.

Add/update test.

Run backend build/test.

Run frontend build nếu có ảnh hưởng UI.

Run smoke P3-SMK-2C.

Produce report 14 mục.

Handoff sang P3.2D nếu đạt DONE GATE.

## TEST / SMOKE REQUIRED

Bắt buộc kiểm tra:

Cart created không tạo order.

Not sellable cannot cart.

Order Draft requires QuoteSnapshot.

Expired quote rejected.

Order Draft không có order_code.

Valid Customer Confirmation accepted.

Duplicate confirmation bị chặn.

Recall/Sale Lock before confirmation rejected nếu policy yêu cầu.

Required info missing rejected/incomplete.

Confirmation accepted không tạo Official Order.

Public response không lộ internal data.

Gateway remains bị chặn.

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

Cart không phải Order.

Order Draft không phải Official Order.

Order Draft gắn QuoteSnapshot active.

Customer Confirmation accepted đúng rule.

Duplicate confirmation không tạo duplicate effect.

Expired quote rejected.

Không tạo Official Order.

Không tạo order_code.

Không set paid.

Không tạo verified revenue.

Test/smoke pass hoặc điểm chặn báo rõ.

Backend build pass hoặc điểm chặn báo rõ.

Evidence submitted.

Report đủ 14 mục.

Gateway vẫn bị chặn.

Không gọi Release Readiness/Production Readiness.

FAIL GATE

Fail nếu:

Sửa ngoài boundary.

Tự tạo Official Order/order_code.

Cart bị xem là Order.

Order Draft bị xem là Official Order.

Bỏ qua Customer Confirmation.

Quote expired vẫn confirm được.

Duplicate confirmation tạo duplicate effect.

Set paid.

Tạo verified revenue.

Mở Gateway.

Không có report 14 mục.

Gọi Production Readiness/Go-live Decision.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_P3_2D_OFFICIAL_ORDER_ORDER_CODE_STATE_MACHINE.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_P3_2B_QUOTE_SNAPSHOT_DONE_GATE.

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL.

## BLOCKED_TEST_OR_BUILD_FAILED.

## BLOCKED_CART_DRAFT_CONFIRMATION_P0_RULE_FAILED.

NEXT FILE: PHASE-03-COMMERCE-RUNTIME/05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md

ĐIỀU KIỆN CHUYỂN TIẾP: Chỉ chuyển sang P3.2D sau khi Cart / Order Draft / Customer Confirmation đạt DONE GATE, không có order_code trong P3.2C, confirmation idempotent, QuoteSnapshot active được validate, expired quote bị reject, accepted confirmation chỉ tạo handoff_ready_for_official_order và owner đồng ý handoff.

## PROMPT-P3.2D - OFFICIAL ORDER / ORDER CODE / STATE MACHINE LIMITED IMPLEMENTATION HANDOFF

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt triển khai giới hạn cho nhóm Official Order / Order Code / Order State Machine trong PHASE-03 - Commerce Runtime.

Mục tiêu là giao dev/Codex/Copilot triển khai hoặc chỉnh sửa đúng phạm vi bước chuyển từ Customer Confirmation hợp lệ sang Official Order chính thức.

P3.2D chỉ xử lý:

Official Order.

Official Order Line.

Order Code.

Order State Machine.

Create Official Order from Customer Confirmation.

Idempotency chống tạo đơn trùng.

Audit state transition.

Handoff sang Payment/Shipping ở P3.2E.

Handoff sang Verified Revenue ở P3.2F sau này.

P3.2D không xác nhận thanh toán.

P3.2D không set Paid.

P3.2D không tạo Shipping Request thật.

P3.2D không phát hành Invoice thật.

P3.2D không tạo Verified Revenue.

P3.2D không tính Commission final.

P3.2D không tính Verified ROAS.

P3.2D không mở Gateway.

## 3. MODE

## MODE: LIMITED IMPLEMENTATION - OFFICIAL ORDER / ORDER CODE / STATE MACHINE ONLY

Trong mode này, dev/Codex/Copilot chỉ được phép sửa code trong phạm vi Official Order / Order Code / Order State Machine đã được P3.1 và P3.2C handoff cho phép.

Được phép:

Thêm/sửa OfficialOrder domain.

Thêm/sửa OfficialOrderLine domain.

Thêm/sửa OrderCode domain.

Thêm/sửa OrderState domain.

Thêm/sửa OrderStateTransition.

Thêm/sửa OfficialOrderService.

Thêm/sửa CreateOfficialOrderFromDraft command.

Thêm/sửa OrderCodeGenerator.

Thêm/sửa OrderStateMachineService.

Gắn Official Order với Order Draft đã Customer Confirmed.

Gắn Official Order với QuoteSnapshot đã xác nhận.

Gắn Official Order với Customer Confirmation accepted.

Thêm API/DTO tạo Official Order từ confirmed draft nếu P3.1 duyệt.

Thêm migration/seed/config nếu P3.1 duyệt.

Thêm test/smoke cho P3.2D.

Chạy backend build/test.

Chạy frontend build nếu có thay contract/UI.

Trả report 14 mục.

Không được:

Set Payment selected nếu chưa thuộc boundary P3.2D.

Set Paid.

Confirm payment.

Tạo shipping request thật.

Phát hành invoice thật.

Tạo verified revenue.

Tạo commission final.

Tạo verified ROAS.

Tạo payout.

Triển khai IVR.

Mở Gateway.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

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

Warehouse Receipt không tự set Sellable.

## 4.5. PHASE 3 - P3.2A

Sellable Gate là runtime gate riêng.

Runtime unavailable phải fail-safe.

Recall/Sale Lock chặn sellable.

Product/SKU/Recipe Active không thay Sellable.

## 4.6. PHASE 3 - P3.2B

QuoteSnapshot là nguồn sự thật cho giá cuối.

Không có QuoteSnapshot thì không có final price.

QuoteSnapshot immutable.

QuoteSnapshot có expiry/freeze window.

AI/Gateway không tự tính giá.

## 4.7. PHASE 3 - P3.2C

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation là điều kiện đầu vào để tạo Official Order.

QuoteSnapshot expired/voided không được xác nhận.

Accepted confirmation không tự tạo Official Order trong P3.2C.

Không có order_code trong P3.2C.

## 4.8. PHASE 3 - P3.2D

Customer Confirmation accepted mới cho phép tạo Official Order.

Official Order phải có order_code.

order_code chỉ tạo một lần.

Command tạo Official Order phải idempotent.

Official Order created không đồng nghĩa Paid.

Official Order created không đồng nghĩa Verified Revenue.

Không có ORDER_VERIFIED thì không final commission, không verified ROAS, không payout.

## 5. ENTRY CONDITION

Chỉ được thực hiện prompt này nếu đã hoàn tất:

## 00-P3-ANALYSIS-ONLY.md.

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md.

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md.

## 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md.

## 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md.

Sellable Gate đạt DONE GATE.

QuoteSnapshot đạt DONE GATE.

Cart / Order Draft / Customer Confirmation đạt DONE GATE.

Order Draft không bị hiểu là Official Order.

Customer Confirmation accepted đúng rule.

Confirmation idempotent.

QuoteSnapshot active được validate.

Expired quote bị reject.

P3.2C chưa tạo order_code.

Owner đồng ý handoff sang P3.2D.

Allowed file change boundary cho P3.2D đã rõ.

Nếu thiếu điều kiện, dev/Codex/Copilot phải dừng và trả trạng thái:

## BLOCKED_NEED_P3_2C_CART_DRAFT_CONFIRMATION_DONE_GATE

## 6. OBJECTIVE

Mục tiêu của P3.2D là triển khai bước tạo đơn chính thức an toàn:

Nhận Order Draft đã Customer Confirmed.

Validate Customer Confirmation accepted.

Validate QuoteSnapshot còn hợp lệ theo policy tại thời điểm tạo order.

Validate không duplicate order.

Tạo Official Order.

Sinh order_code chính thức.

Gắn order line theo QuoteSnapshotLine.

Lưu snapshot giá/tổng tiền đã xác nhận.

Set Order State ban đầu đúng rule.

Ghi audit/state transition.

Emit event/outbox nếu P3.1 duyệt.

Handoff sang P3.2E cho Payment/Shipping/Invoice/Tax.

P3.2D không xử lý Payment confirmation.

P3.2D không xử lý Shipping fulfillment.

P3.2D không xử lý Verified Revenue.

## 7. CORE RULES

## 7.1. Official Order chỉ tạo sau Customer Confirmation accepted

Không được tạo Official Order nếu:

Chưa có Order Draft.

Order Draft chưa gắn QuoteSnapshot.

Order Draft chưa được Customer Confirmation accepted.

Customer Confirmation expired/invalid/rejected.

QuoteSnapshot expired/voided.

Required customer/delivery fields còn thiếu.

Duplicate request không có idempotency.

Recall/Sale Lock phát sinh trước tạo order nếu policy yêu cầu recheck.

## 7.2. Official Order phải có order_code

Một Order chính thức bắt buộc có:

order_id.

order_code.

order_state.

quote_snapshot_id.

order_draft_id.

customer_confirmation_id.

customer/session context.

confirmed customer/delivery data snapshot.

amount snapshot.

audit/correlation.

Nếu chưa có order_code, không được gọi là đơn hàng chính thức.

## 7.3. order_code chỉ tạo một lần

Order Code phải:

Unique.

Stable.

Không regenerate tùy tiện.

Không reuse.

Không sửa sau khi order được tạo.

Có audit/correlation.

Có idempotency chống tạo nhiều order_code.

## 7.4. Official Order created không đồng nghĩa Paid

Khi Official Order được tạo:

Order chưa paid.

Payment chưa confirmed.

Payment selected nếu có chỉ là lựa chọn phương thức.

Paid status thuộc P3.2E.

Payment Core confirmation mới là paid status.

## 7.5. Official Order created không đồng nghĩa Verified Revenue

Khi Official Order được tạo:

Chưa verified revenue.

Chưa final commission.

Chưa verified ROAS.

Chưa payout.

ORDER_VERIFIED hoặc equivalent checkpoint thuộc P3.2F.

## 7.6. Không gửi Gateway như release-ready

Sau khi tạo Official Order:

Có thể tạo local event/handoff nội bộ nếu P3.1 duyệt.

Không được mở Global Gate.

Không được gọi Completion Decision.

Không được gọi Release Readiness.

Không được gọi Production Readiness.

## 8. SCOPE IN

P3.2D bao gồm:

OfficialOrder.

OfficialOrderLine.

OfficialOrderService.

CreateOfficialOrderFromDraft command.

OrderCode.

OrderCodeGenerator.

OrderState.

OrderStateTransition.

OrderStateMachineService.

CustomerConfirmation validation.

QuoteSnapshot reference validation.

Duplicate order guard.

Idempotency cho tạo order.

Audit/state transition log.

Event/outbox OFFICIAL_ORDER_CREATED nếu P3.1 duyệt.

API/DTO nếu P3.1 duyệt.

Migration/seed/config nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2D.

Handoff sang P3.2E.

## 9. SCOPE OUT

P3.2D không bao gồm:

Không triển khai Payment selection nếu chưa được duyệt ở boundary.

Không set Paid.

Không confirm payment.

Không xử lý bank transfer confirmation.

Không xử lý COD paid logic.

Không tạo Shipping Request thật.

Không xác nhận giao hàng.

Không phát hành invoice thật.

Không sync MISA thật.

Không tạo Verified Revenue.

Không tạo ORDER_VERIFIED.

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

Dev/Codex/Copilot chỉ được sửa file thuộc boundary đã được P3.1/P3.2C handoff cho phép.

## 10.1. Nhóm file được phép sửa nếu đã duyệt

Nhóm file

Được sửa

Ghi chú

Commerce Official Order domain

Có

OfficialOrder/OfficialOrderLine

Order State domain

Có

OrderState/OrderStateTransition

Order Code domain/service

Có

OrderCodeGenerator

Official Order service

Có

Create from confirmed draft

Order State Machine service

Có

Initial transitions only

API/DTO Official Order

Có điều kiện

Chỉ nếu P3.1 duyệt

Migration

Có điều kiện

Chỉ OfficialOrder/OrderLine/OrderCode/State

Seed/config dev-test

Có điều kiện

Không tạo paid/revenue

Backend tests

Có

Chỉ P3.2D

Frontend/Admin UI

Có điều kiện

Chỉ nếu P3.1 duyệt

Event/outbox

Có điều kiện

Chỉ OFFICIAL_ORDER_CREATED nếu đã duyệt

Markdown/handoff

Có điều kiện

Chỉ cập nhật P3.2D report

## 10.2. Nhóm file không được sửa

Nhóm file

Không được sửa

Lý do

Payment confirmation implementation

Không

Thuộc P3.2E

Payment provider/callback

Không

Thuộc P3.2E

Shipping fulfillment

Không

Thuộc P3.2E

Invoice issuance

Không

Thuộc P3.2E

Tax filing/sync

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

MISA production sync

Không

Không thuộc P3.2D

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## 11. REQUIRED INPUT

## 11.1. Từ P3.2A

SellableGateResolver.

SellableDecision.

recall/sale lock state.

runtime policy status.

channel suppression status.

## 11.2. Từ P3.2B

QuoteSnapshot.

QuoteSnapshotLine.

final_payable_amount.

quote_status.

expires_at.

policy version.

immutable snapshot evidence.

## 11.3. Từ P3.2C

Cart optional.

OrderDraft.

OrderDraftLine.

CustomerConfirmation.

confirmation_status = ACCEPTED.

handoff_ready_for_official_order = true.

buyer/receiver/delivery data.

confirmation idempotency evidence.

draft expiry validation.

## 11.4. Từ policy/config

Order code format policy.

Order initial state policy.

Order duplicate prevention policy.

Order cancellation policy placeholder nếu có.

State transition policy.

Required audit policy.

Event/outbox policy.

Recheck sellable/recall/sale lock before official order policy.

## 12. TARGET BEHAVIOR

## 12.1. Create Official Order success

Official Order được tạo khi:

Order Draft tồn tại.

Order Draft ở trạng thái CUSTOMER_CONFIRMED.

Customer Confirmation accepted.

QuoteSnapshot active hoặc hợp lệ theo policy.

QuoteSnapshot không expired/voided.

Required customer/delivery fields đủ.

Chưa có Official Order nào được tạo từ cùng confirmation/draft.

Idempotency key hợp lệ.

Order Code sinh thành công.

Initial order state được set đúng.

Audit/state transition được ghi.

## 12.2. Create Official Order reject

Phải reject nếu:

Missing Order Draft.

Missing Customer Confirmation.

Confirmation chưa accepted.

QuoteSnapshot expired.

QuoteSnapshot voided.

Draft expired/cancelled.

Duplicate order.

Missing required customer/delivery data.

Recall/Sale Lock active trước order nếu policy yêu cầu recheck.

Runtime unavailable.

Order code generator failure.

Idempotency conflict.

Invalid state transition.

## 12.3. Sau khi tạo Official Order

Sau khi tạo Official Order:

Có order_code chính thức.

Có initial order_state.

Có order lines theo QuoteSnapshotLine.

Có amount snapshot từ QuoteSnapshot.

Có customer/delivery snapshot.

Có audit/state transition.

Có handoff_ready_for_payment_shipping = true nếu P3.2E cần.

Chưa paid.

Chưa verified revenue.

Chưa commission final.

Chưa verified ROAS.

Chưa payout.

## 13. DOMAIN CONTRACT

## 13.1. OfficialOrder

Trường đề xuất:

order_id.

order_code.

order_state.

order_draft_id.

customer_confirmation_id.

quote_snapshot_id.

cart_id nullable.

customer_id nullable.

session_id.

channel.

buyer_name_snapshot.

buyer_phone_snapshot.

receiver_name_snapshot nullable.

receiver_phone_snapshot nullable.

delivery_address_snapshot.

delivery_note_snapshot nullable.

currency.

subtotal_listed_amount_snapshot.

total_program_discount_amount_snapshot.

total_member_benefit_amount_snapshot.

total_shipping_fee_amount_snapshot.

total_tax_amount_snapshot.

final_payable_amount_snapshot.

payment_state_initial.

shipping_state_initial nullable.

revenue_state_initial.

created_at.

created_by_actor nullable.

correlation_id.

idempotency_key.

audit_ref.

Rule

OfficialOrder lấy dữ liệu từ OrderDraft + QuoteSnapshot + CustomerConfirmation.

Không được tính lại final price trong P3.2D.

Không được tự sửa QuoteSnapshot.

## 13.2. OfficialOrderLine

Trường đề xuất:

order_line_id.

order_id.

quote_snapshot_line_id.

sku_id.

product_id.

product_public_name_snapshot.

quantity.

uom.

listed_unit_price_snapshot.

listed_line_amount_snapshot.

program_discount_amount_snapshot.

member_benefit_amount_snapshot.

diamond_benefit_amount_snapshot nullable.

shipping_fee_allocation_snapshot nullable.

tax_amount_snapshot nullable.

final_line_amount_snapshot.

line_state.

created_at.

Rule

Order line không được tự resolve lại giá.

Nếu QuoteSnapshotLine thiếu, reject tạo order.

## 13.3. OrderCode

Trường đề xuất:

order_code.

order_id.

code_prefix.

code_sequence.

generated_at.

generator_version.

correlation_id.

uniqueness_constraint_ref nếu có.

OrderCode rule

Unique toàn hệ thống theo policy.

Không reuse.

Không sửa sau khi tạo.

Không tạo trước khi Official Order được tạo.

Không tạo trong Cart/Order Draft.

Retry idempotent không sinh mã mới nếu order đã tạo.

## 13.4. OrderState

State đề xuất cho P3.2D:

## OFFICIAL_CREATED.

## WAITING_PAYMENT.

## PAYMENT_SELECTED.

## PAYMENT_WAITING_CONFIRMATION.

## PAID.

## FULFILLMENT_WAITING.

## SHIPPING_WAITING.

## SHIPPING_IN_PROGRESS.

## DELIVERED.

## ORDER_VERIFIED.

## CANCELLED.

## RETURNED.

## REFUNDED.

P3.2D allowed states

P3.2D chỉ được tạo initial state:

## OFFICIAL_CREATED.

WAITING_PAYMENT nếu policy yêu cầu sau khi order tạo.

P3.2D không được set:

PAYMENT_SELECTED nếu thuộc P3.2E.

## PAID.

## DELIVERED.

## ORDER_VERIFIED.

## VERIFIED_REVENUE_CREATED.

## 13.5. OrderStateTransition

Trường đề xuất:

transition_id.

order_id.

from_state nullable.

to_state.

transition_reason.

actor_context.

occurred_at.

correlation_id.

audit_ref.

Rule:

Append-only.

Không sửa/xóa transition.

Mọi state change phải qua OrderStateMachineService.

Không update state trực tiếp ngoài state machine.

## 13.6. CreateOfficialOrderCommand

Input đề xuất:

order_draft_id.

customer_confirmation_id.

quote_snapshot_id.

channel.

customer_id nullable.

session_id.

idempotency_key.

correlation_id.

actor_context nullable.

Output đề xuất:

order_id.

order_code.

order_state.

payment_state.

shipping_state nullable.

revenue_state.

final_payable_amount_snapshot.

created_at.

handoff_ready_for_payment_shipping.

audit_ref.

event_ref nullable.

## 14. SERVICE CONTRACT

## 14.1. OfficialOrderService

CreateOfficialOrderFromConfirmedDraft input

order_draft_id.

customer_confirmation_id.

quote_snapshot_id.

idempotency_key.

correlation_id.

actor_context nullable.

CreateOfficialOrderFromConfirmedDraft output

order_id.

order_code.

order_state.

payment_state.

revenue_state.

created_at.

handoff_ready_for_payment_shipping.

already_created flag nếu idempotent retry.

audit_ref.

event_ref nullable.

Service rules

Load OrderDraft.

Load CustomerConfirmation.

Load QuoteSnapshot.

Validate confirmation accepted.

Validate draft state CUSTOMER_CONFIRMED.

Validate quote snapshot matches draft and confirmation.

Validate quote not expired/voided theo policy.

Validate required customer/delivery snapshot.

Check duplicate official order by draft/confirmation/idempotency.

Generate order_code.

Create OfficialOrder.

Create OfficialOrderLines from QuoteSnapshotLines.

Set initial order state.

Create state transition log.

Audit append-only.

Emit outbox event nếu approved.

Return created order.

## 14.2. OrderCodeGenerator

Input:

order creation context.

date/time.

channel optional.

sequence source.

correlation_id.

Output:

order_code.

sequence.

generator_version.

Rule:

Must be unique.

Must be deterministic/idempotent under retry if design uses reserved code.

Must not generate duplicate under concurrency.

Must not expose internal sequence if policy does not allow.

Must not create code for draft/cart.

## 14.3. OrderStateMachineService

Responsibilities:

Validate allowed transition.

Apply initial state.

Prevent illegal transition.

Append state transition log.

Audit transition.

Return current state.

P3.2D only implements official order initial transition.

Payment/shipping/revenue transitions are reserved for later files.

## 14.4. DuplicateOrderGuard

Must block duplicate order creation by:

order_draft_id.

customer_confirmation_id.

quote_snapshot_id.

idempotency_key.

same confirmed payload hash if available.

Expected behavior:

Same idempotency key retry -> return existing order.

Different idempotency key but same confirmation already converted -> reject duplicate or return existing according to policy.

Concurrent requests -> only one official order.

## 14.5. OfficialOrderAuditService

Audit must record:

actor.

action.

order_draft_id.

customer_confirmation_id.

quote_snapshot_id.

order_id.

order_code.

from_state.

to_state.

correlation_id.

reason.

timestamp.

## 15. STATE MACHINE

## 15.1. Draft to Official Order bridge

P3.2C ended at:

OrderDraftStatus = CUSTOMER_CONFIRMED.

CustomerConfirmationStatus = ACCEPTED.

handoff_ready_for_official_order = true.

P3.2D starts from this handoff.

Allowed bridge:

## CUSTOMER_CONFIRMED -> OFFICIAL_ORDER_CREATED

Implementation result:

Create OfficialOrder.

Create order_code.

Mark OrderDraft as CONVERTED_TO_OFFICIAL_ORDER if P3.2C/P3.2D boundary approved.

Store official order reference in draft if approved.

Append transition.

## 15.2. Official Order initial state

Allowed initial state:

## OFFICIAL_CREATED.

## WAITING_PAYMENT.

Recommended:

OFFICIAL_CREATED as creation event.

Then transition to WAITING_PAYMENT if payment required.

Nếu codebase chỉ dùng một initial state, phải ghi rõ trong report.

## 15.3. Forbidden transitions in P3.2D

P3.2D không được thực hiện:

WAITING_PAYMENT -> PAYMENT_SELECTED nếu thuộc P3.2E.

## PAYMENT_SELECTED -> PAID.

PAID -> SHIPPING_WAITING nếu thuộc P3.2E.

DELIVERED -> ORDER_VERIFIED nếu thuộc P3.2F.

ORDER_VERIFIED -> VERIFIED_REVENUE_CREATED nếu thuộc P3.2F.

Any state -> COMMISSION_FINAL.

Any state -> ROAS_VERIFIED.

Any state -> PAYOUT_READY.

## 15.4. Cancellation placeholder

P3.2D có thể thiết kế placeholder state CANCELLED, nhưng không triển khai full cancellation workflow nếu chưa được duyệt.

Nếu có hủy đơn trong P3.2D, chỉ được làm nếu P3.1 đã đưa vào boundary.

## 16. API / DTO CONTRACT - IF APPROVED BY P3.1

## 16.1. Create Official Order API

POST /api/commerce/orders/from-confirmed-draft

Request:

order_draft_id.

customer_confirmation_id.

quote_snapshot_id.

idempotency_key.

correlation_id.

Response:

order_id.

order_code.

order_state.

payment_state.

revenue_state.

final_payable_amount_snapshot.

created_at.

handoff_ready_for_payment_shipping.

message_public_safe.

no_paid_yet = true.

no_verified_revenue_yet = true.

## 16.2. Read Official Order API

GET /api/commerce/orders/{order_id}

Response:

order_id.

order_code.

order_state.

lines.

customer/delivery summary.

final_payable_amount_snapshot.

payment_state.

shipping_state nullable.

revenue_state.

created_at.

source_quote_snapshot_id.

source_order_draft_id.

source_customer_confirmation_id.

## 16.3. Public/internal response boundary

Public/channel response được phép có:

order_code.

order_state public-safe.

product public name.

quantity.

final payable amount snapshot.

delivery summary.

payment instruction placeholder nếu P3.2E đã duyệt.

"Chờ thanh toán" hoặc "Chờ xử lý thanh toán" theo policy.

Không được lộ:

Internal margin.

Cost.

Supplier info.

Internal policy id.

Internal commission.

ROAS data.

Internal audit note.

Sensitive customer data quá mức cần thiết.

Payment confirmation secret.

MISA internal mapping.

## 16.4. API không được làm

Create Official Order API không được:

Set paid.

Confirm payment.

Create shipping request.

Issue invoice.

Create verified revenue.

Create commission final.

Create verified ROAS.

Trigger payout.

Open Gateway.

## 17. DATABASE / MIGRATION RULE

Nếu P3.1 duyệt migration, bảng/entity P3.2D có thể gồm:

commerce_order.

commerce_order_line.

commerce_order_code_sequence hoặc equivalent.

commerce_order_state_transition.

commerce_order_idempotency nếu chưa có idempotency chung.

commerce_order_event_outbox nếu chưa có outbox chung và P3.1 duyệt.

Không tạo bảng:

commerce_payment_confirmation.

commerce_shipping_request.

commerce_invoice_issued.

commerce_verified_revenue.

commerce_commission_final.

commerce_roas_verified.

commerce_payout.

## 17.1. Migration safety

Migration phải:

Không phá dữ liệu cũ.

Không tạo paid status mặc định sai.

Không tạo verified revenue.

Có unique constraint cho order_code.

Có unique/index chống duplicate theo order_draft_id/customer_confirmation_id.

Có index cho order_id, order_code, customer_id/session_id, order_state.

Có audit/correlation fields theo convention.

Có state constraint hợp lệ.

Không hardcode 20 SKU.

Không mở Gateway.

## 18. SEED / CONFIG RULE

Seed/config dev-test được phép tạo:

Order code format policy.

Order initial state policy.

Negative fixture:

confirmed draft valid.

rejected confirmation.

expired quote.

duplicate confirmation.

duplicate idempotency key.

Test order state config.

Test order code sequence.

Seed/config không được:

Tạo paid status.

Tạo payment confirmation.

Tạo shipping completed.

Tạo delivered verified.

Tạo verified revenue.

Tạo commission final.

Tạo ROAS verified.

Tạo payout.

Mở Gateway.

Bypass Customer Confirmation.

## 19. SECURITY / GOVERNANCE GUARDRAILS

## 19.1. RBAC

Bắt buộc backend permission cho:

Admin create official order from confirmed draft nếu nhân sự thao tác.

View internal order detail.

Manual order state override nếu có.

Export order/audit evidence.

View internal state transition.

Customer/channel confirmation flow có thể dùng customer/session context, nhưng backend vẫn phải validate.

## 19.2. Actor Context

Bắt buộc cho:

Admin-assisted order creation.

Manual override.

Manual cancellation nếu có.

Reissue/recover order creation nếu có.

## 19.3. Correlation ID

Mọi command phải có correlation_id:

Create Official Order.

Generate Order Code.

State transition.

Outbox event.

Audit event.

## 19.4. Audit append-only

Audit bắt buộc cho:

Official Order created.

Order Code generated.

Order state transition.

Duplicate order bị chặn.

Idempotent retry returned existing order.

Any override.

## 19.5. Idempotency

Idempotency bắt buộc cho:

Create Official Order from confirmed draft.

Order code generation if side effect.

Outbox event if created.

## 19.6. Evidence Registry

Evidence submitted chưa phải accepted.

Không gọi Completion Decision nếu chưa owner review.

## 19.7. Runtime unavailable

Nếu runtime unavailable hoặc dependency không xác định:

Không tạo Official Order nếu không validate được draft/confirmation/quote.

Không tạo order_code.

Không set handoff_ready_for_payment_shipping.

Không tạo partial order.

## 19.8. Transaction safety

Tạo Official Order phải là transaction atomic:

Validate draft/confirmation/quote.

Generate/reserve order_code.

Create order.

Create order lines.

Create state transition.

Create audit.

Create outbox event nếu có.

Nếu một bước fail, không được để trạng thái nửa vời.

## 20. EVENT / OUTBOX RULE - IF APPROVED

Nếu P3.1 duyệt event/outbox, P3.2D có thể phát event:

## OFFICIAL_ORDER_CREATED

Payload đề xuất:

event_id.

event_type = OFFICIAL_ORDER_CREATED.

event_version.

order_id.

order_code.

order_state.

quote_snapshot_id.

order_draft_id.

customer_confirmation_id.

customer_id nullable.

session_id.

channel.

final_payable_amount_snapshot.

currency.

occurred_at.

correlation_id.

privacy_class.

Rule:

Event tạo sau business state commit theo architecture đã khóa.

Event không đồng nghĩa Payment confirmed.

Event không đồng nghĩa Verified Revenue.

Ads/ROAS không được dùng event này làm verified revenue.

Finance/Commission không được final từ event này.

## 21. TEST REQUIREMENTS

## 21.1. Unit test - Official Order creation

Valid confirmed draft -> create Official Order.

Missing draft -> reject.

Draft not CUSTOMER_CONFIRMED -> reject.

Missing confirmation -> reject.

Confirmation not ACCEPTED -> reject.

Missing QuoteSnapshot -> reject.

Expired QuoteSnapshot -> reject.

Voided QuoteSnapshot -> reject.

Required customer/delivery fields missing -> reject.

Recall/Sale Lock active before order -> reject nếu policy yêu cầu.

## 21.2. Unit test - Order Code

Official Order created -> order_code generated.

order_code unique.

Retry same idempotency key -> same order/order_code.

Concurrent create requests -> one order_code only.

Cart does not have order_code.

Order Draft does not have official order_code.

## 21.3. Unit test - State Machine

Initial state set correctly.

State transition logged.

Invalid transition rejected.

Direct state update bypass rejected nếu architecture hỗ trợ.

Order created not Paid.

Order created not Verified Revenue.

Order created not Commission final.

Order created not ROAS verified.

## 21.4. Unit test - Idempotency/Duplicate

Same draft + same confirmation + same idempotency key -> one order.

Same draft + same confirmation + different idempotency key -> reject duplicate hoặc return existing theo policy.

Same confirmation cannot convert twice.

Duplicate order line not created on retry.

Outbox event not duplicated on retry nếu có.

## 21.5. Integration test nếu có API

POST create order from confirmed draft success.

POST create order from unconfirmed draft reject.

POST create order with expired quote reject.

POST retry idempotent returns same order.

GET official order shows order_code.

GET official order shows not paid.

GET official order shows not verified revenue.

Public response does not leak internal data.

## 22. SMOKE REQUIREMENTS

## 22.1. P3-SMK-2D-001 - Confirmed draft creates Official Order

Case: Valid Order Draft + accepted Customer Confirmation + active QuoteSnapshot.
Expected: Official Order created, order_code generated.

## 22.2. P3-SMK-2D-002 - Unconfirmed draft rejected

Case: Order Draft chưa CUSTOMER_CONFIRMED.
Expected: Không tạo Official Order.

## 22.3. P3-SMK-2D-003 - Expired quote rejected

Case: QuoteSnapshot expired.
Expected: Không tạo Official Order.

## 22.4. P3-SMK-2D-004 - order_code generated once

Case: Tạo Official Order thành công.
Expected: Một order_code duy nhất.

## 22.5. P3-SMK-2D-005 - Idempotent retry

Case: Gọi lại cùng idempotency key.
Expected: Trả cùng order/order_code, không tạo trùng.

## 22.6. P3-SMK-2D-006 - Concurrent duplicate bị chặn

Case: Hai request đồng thời từ cùng confirmation.
Expected: Chỉ một Official Order.

## 22.7. P3-SMK-2D-007 - Official Order is not Paid

Case: Official Order created.
Expected: payment_state chưa paid.

## 22.8. P3-SMK-2D-008 - Official Order is not Verified Revenue

Case: Official Order created.
Expected: revenue_state chưa verified.

## 22.9. P3-SMK-2D-009 - No commission/ROAS/payout

Case: Official Order created.
Expected: Không tạo commission final, không verified ROAS, không payout.

## 22.10. P3-SMK-2D-010 - Recall/Sale Lock before order

Case: Recall/Sale Lock phát sinh trước tạo Official Order.
Expected: Reject nếu policy yêu cầu recheck.

## 22.11. P3-SMK-2D-011 - State transition audit

Case: Official Order created.
Expected: Có state transition log và audit append-only.

## 22.12. P3-SMK-2D-012 - Gateway remains bị chặn

## 23. EVIDENCE REQUIREMENTS

P3.2D cần gom evidence:

Git diff trong allowed boundary.

File list đã sửa.

Migration diff nếu có.

Seed/config diff nếu có.

Unit test result.

Integration test result nếu có API.

Smoke result P3-SMK-2D-001 -> P3-SMK-2D-012.

Backend build result.

Frontend build result nếu có ảnh hưởng UI.

Official Order created evidence.

order_code unique evidence.

Idempotency retry evidence.

Duplicate/concurrency prevention evidence.

Order state transition evidence.

Official Order not paid evidence.

Official Order not verified revenue evidence.

No commission/ROAS/payout evidence.

Public/internal boundary evidence.

Audit evidence.

Outbox evidence nếu có.

Handoff note sang P3.2E.

Lưu ý:

Evidence submitted chưa phải Evidence accepted.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

## 24. EXECUTION STEPS

Step 1 - Confirm Mode

Xác nhận:

## MODE: LIMITED IMPLEMENTATION - OFFICIAL ORDER / ORDER CODE / STATE MACHINE ONLY

Step 2 - Read P3.2C Done Report

Xác nhận:

Cart không phải Order.

Order Draft không phải Official Order.

Customer Confirmation accepted đúng rule.

Confirmation idempotent.

QuoteSnapshot active được validate.

No order_code in P3.2C.

Nếu chưa đạt, dừng.

Step 3 - Inspect Existing Order Logic

Kiểm tra codebase hiện tại:

Có order model cũ không.

Có order_code generator cũ không.

Có flow nào tạo order trước confirmation không.

Có flow nào set paid khi tạo order không.

Có flow nào tính revenue khi tạo order không.

Có duplicate prevention không.

Có state machine không.

Có direct update order state không.

Có audit/state transition không.

Step 4 - Implement Official Order Contract

Triển khai/chỉnh:

OfficialOrder.

OfficialOrderLine.

CreateOfficialOrderCommand.

OfficialOrderService.

Mapping từ OrderDraft/QuoteSnapshot/Confirmation.

Step 5 - Implement OrderCodeGenerator

Triển khai/chỉnh:

Unique order_code.

Sequence/format theo policy.

Idempotency/concurrency-safe behavior.

Không tạo code cho cart/draft.

Step 6 - Implement State Machine

Triển khai/chỉnh:

OrderState.

OrderStateTransition.

Initial state.

Transition log.

Invalid transition guard.

Step 7 - Implement Duplicate/Idempotency Guard

Triển khai/chỉnh:

Duplicate by draft.

Duplicate by confirmation.

Duplicate by idempotency key.

Retry behavior.

Concurrency behavior.

Step 8 - Implement API/DTO nếu approved

Nếu P3.1 duyệt, triển khai:

Create Official Order from confirmed draft API.

Read Official Order API.

Nếu chưa duyệt, không tự tạo API.

Step 9 - Implement Migration/Seed nếu approved

Chỉ trong phạm vi OfficialOrder/OrderLine/OrderCode/State.

Không tạo payment/shipping/revenue migration.

Step 10 - Add Tests

Thêm/cập nhật test theo mục Test Requirements.

Step 11 - Run Build/Test/Smoke

Chạy:

Backend build.

Backend test.

Integration test nếu có.

Frontend build nếu ảnh hưởng UI.

Smoke P3-SMK-2D.

Step 12 - Evidence Report

Gom evidence và trả report 14 mục.

Step 13 - Handoff

Kết luận có được chuyển sang P3.2E hay không.

## 25. REQUIRED REPORT FORMAT - 14 MỤC

## 25.1. Tóm tắt

Ghi rõ:

Đã triển khai Official Order phần nào.

Đã triển khai order_code phần nào.

Đã triển khai Order State Machine phần nào.

Test/smoke kết quả thế nào.

Có đủ điều kiện sang P3.2E không.

## 25.2. File đã sửa

Liệt kê:

File path.

Loại thay đổi.

Lý do sửa.

Thuộc allowed boundary nào.

## 25.3. Nguồn yêu cầu

Liệt kê:

## MASTER.

## PHASE 0.

## PHASE 1.

## PHASE 2.

P3 Analysis.

P3.1 Technical Design.

P3.2A Done Report.

P3.2B Done Report.

P3.2C Done Report.

Prompt P3.2D.

## 25.4. Evidence đã dùng

Liệt kê:

Code references.

DB/migration references.

QuoteSnapshot evidence.

CustomerConfirmation evidence.

Test references.

Smoke evidence.

API response/log nếu có.

Audit/evidence registry nếu có.

## 25.5. Lệnh đã chạy

Ghi rõ:

Build.

Test.

Smoke.

Lint.

Migration validation nếu có.

Seed validation nếu có.

## 25.6. Kết quả test

Ghi:

Test pass.

Test fail.

Fail xử lý ra sao.

Coverage còn thiếu.

## 25.7. Kết quả backend build

Ghi:

Build command.

Kết quả.

Log summary.

Error nếu có.

## 25.8. Kết quả frontend build

Ghi:

Có cần chạy không.

Nếu có, command và kết quả.

Nếu không, lý do.

## 25.9. Kết quả cleanup process

Ghi:

Có file tạm không.

Có cleanup không.

Có process treo không.

Có artifact cần giữ làm evidence không.

## 25.10. Cập nhật Markdown

Ghi:

Markdown nào đã cập nhật nếu có.

Nếu không cập nhật, ghi rõ.

Handoff note có cập nhật không.

## 25.11. Kết quả database migration/update nếu áp dụng

Ghi:

Có migration không.

Migration name.

Apply result.

Rollback/down result nếu có.

DB validation.

Unique order_code validation.

Không tạo payment/shipping/revenue.

## 25.12. Kết quả seed validation nếu áp dụng

Ghi:

Có seed không.

Seed có tạo paid không.

Seed có tạo verified revenue không.

Seed có tạo commission/ROAS/payout không.

Seed validation result.

## 25.13. Rủi ro còn lại

Phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Owner decision needed.

Deferred to P3.2E/P3.2F.

Technical debt.

## 25.14. Cập nhật handoff

Ghi rõ:

Có được sang P3.2E không.

Điều kiện còn thiếu nếu chưa.

File tiếp theo.

Handoff notes cho Payment/Shipping/Invoice/Tax.

## 26. DONE GATE

P3.2D được DONE khi đủ:

Chỉ sửa file trong allowed boundary.

Official Order chỉ tạo từ Customer Confirmation accepted.

Order Draft chưa confirmed bị reject.

QuoteSnapshot missing/expired/voided bị reject.

Official Order có order_code.

order_code unique.

order_code chỉ tạo một lần.

Idempotent retry không tạo duplicate order.

Concurrent duplicate bị chặn.

Official Order lines lấy từ QuoteSnapshotLine.

Official Order không tính lại final price.

Official Order created không set Paid.

Official Order created không tạo Verified Revenue.

Official Order created không tạo Commission final.

Official Order created không tạo Verified ROAS.

Official Order created không tạo Payout.

State machine có initial state đúng.

State transition append-only.

Audit/correlation đầy đủ.

Public/internal response boundary đúng.

Event/outbox nếu có không bị hiểu là verified revenue.

Test bắt buộc pass hoặc điểm chặn rõ.

Smoke P3-SMK-2D pass hoặc điểm chặn rõ.

Backend build pass hoặc điểm chặn rõ.

Frontend build pass nếu có ảnh hưởng UI.

Evidence submitted.

Report đủ 14 mục.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 27. FAIL GATE

P3.2D bị FAIL nếu:

Sửa file ngoài allowed boundary.

Tạo Official Order khi chưa có Customer Confirmation accepted.

Tạo Official Order từ expired/voided QuoteSnapshot.

Tạo order_code trong Cart/Order Draft.

Official Order không có order_code.

order_code không unique.

Retry tạo duplicate order.

Concurrent request tạo nhiều order.

Official Order tự set Paid.

Official Order tự tạo Payment Confirmation.

Official Order tự tạo Shipping Request thật.

Official Order tự phát hành Invoice thật.

Official Order tự tạo Verified Revenue.

Official Order tự tạo Commission final.

Official Order tự tạo Verified ROAS.

Official Order tự tạo Payout.

Không có state transition log.

State transition bị update trực tiếp không qua state machine.

Public response lộ internal data.

Không có test negative case.

Không có report 14 mục.

Mở Gateway.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

## 28. DOWNSTREAM HANDOFF

## 28.1. File tiếp theo

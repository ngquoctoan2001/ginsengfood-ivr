# P3.2E - PAYMENT SHIPPING INVOICE TAX

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- TECH-04 Commerce Runtime payment, shipping, invoice va tax boundary.
- PACK-04 MISA Accounting Handoff.
- TECH-10 Global Smoke / UAT / Evidence.
- Payment selected khong dong nghia Paid.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md

## 28.2. Prompt tiếp theo

## PROMPT-P3.2E - PAYMENT / SHIPPING / INVOICE / TAX LIMITED IMPLEMENTATION HANDOFF

## 28.3. Điều kiện sang P3.2E

Chỉ chuyển sang P3.2E nếu:

P3.2D đạt DONE GATE.

Official Order chỉ tạo từ Customer Confirmation accepted.

order_code unique và stable.

Idempotency chống duplicate order hoạt động.

Official Order không tự set Paid.

Official Order không tự tạo Verified Revenue.

Order State Machine có initial state và transition log.

Payment/shipping/revenue chưa bị scope creep.

Evidence submitted đầy đủ.

Owner đồng ý handoff.

## 28.4. Không chuyển nếu

Official Order chưa ổn định.

order_code có nguy cơ duplicate.

Retry tạo nhiều order.

Order created đã bị set Paid.

Order created đã tạo verified revenue.

Có scope creep sang Payment/Revenue.

Gateway bị mở.

Report thiếu evidence.

## 29. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3.2D - OFFICIAL ORDER / ORDER CODE / STATE MACHINE LIMITED IMPLEMENTATION HANDOFF

## MODE: LIMITED IMPLEMENTATION - OFFICIAL ORDER / ORDER CODE / STATE MACHINE ONLY

Bạn đang thực hiện PHASE-03 - Commerce Runtime cho dự án Ginsengfood.

Nhiệm vụ duy nhất của bạn là triển khai hoặc chỉnh sửa Official Order, Order Code và Order State Machine từ Customer Confirmation hợp lệ.

Không được set Paid.
Không được confirm payment.
Không được tạo Shipping Request thật.
Không được phát hành Invoice thật.
Không được tạo Verified Revenue.
Không được tạo Commission final.
Không được tạo Verified ROAS.
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

## PHASE-03/04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## CORE RULES

Official Order chỉ tạo sau Customer Confirmation accepted.

Order Draft không phải Official Order.

Official Order phải có order_code.

order_code chỉ tạo một lần.

Create Official Order phải idempotent.

Official Order created không đồng nghĩa Paid.

Official Order created không đồng nghĩa Verified Revenue.

No ORDER_VERIFIED thì không final commission, không verified ROAS, không payout.

Gateway vẫn bị chặn.

## SCOPE IN

Triển khai/chỉnh:

OfficialOrder.

OfficialOrderLine.

OrderCode.

OrderCodeGenerator.

OrderState.

OrderStateTransition.

OrderStateMachineService.

OfficialOrderService.

CreateOfficialOrderFromConfirmedDraft command.

DuplicateOrderGuard.

Idempotency.

Audit/state transition.

API/DTO nếu P3.1 duyệt.

Migration/seed nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2D.

## SCOPE OUT

Không làm:

Payment selected nếu chưa duyệt boundary.

Paid status.

Payment confirmation.

Shipping request.

Invoice issuance.

MISA production sync.

Verified Revenue.

Commission final.

Verified ROAS.

Payout.

## IVR.

Gateway.

## ALLOWED FILE CHANGE BOUNDARY

Chỉ sửa file thuộc boundary đã được P3.1/P3.2C duyệt.

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## EXECUTION STEPS

Xác nhận mode LIMITED IMPLEMENTATION - OFFICIAL ORDER / ORDER CODE / STATE MACHINE ONLY.

Đọc P3.2C Done Report.

Xác định allowed file boundary.

Inspect code hiện tại về order/order_code/state.

Implement OfficialOrder contract.

Implement OrderCodeGenerator.

Implement OrderStateMachine.

Implement Duplicate/Idempotency Guard.

Implement API/DTO nếu đã duyệt.

Implement migration/seed nếu đã duyệt.

Add/update test.

Run backend build/test.

Run frontend build nếu có ảnh hưởng UI.

Run smoke P3-SMK-2D.

Produce report 14 mục.

Handoff sang P3.2E nếu đạt DONE GATE.

## TEST / SMOKE REQUIRED

Bắt buộc kiểm tra:

Confirmed draft creates Official Order.

Unconfirmed draft rejected.

Expired quote rejected.

order_code generated once.

Idempotent retry returns same order/order_code.

Concurrent duplicate bị chặn.

Official Order is not Paid.

Official Order is not Verified Revenue.

No commission/ROAS/payout.

Recall/Sale Lock before order rejected nếu policy yêu cầu.

State transition audit.

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

Official Order chỉ tạo từ Customer Confirmation accepted.

Official Order có order_code.

order_code unique/stable.

Idempotent retry không tạo duplicate.

Concurrent duplicate bị chặn.

Official Order không set Paid.

Official Order không tạo Verified Revenue.

Không commission/ROAS/payout.

State transition append-only.

Test/smoke pass hoặc điểm chặn báo rõ.

Backend build pass hoặc điểm chặn báo rõ.

Evidence submitted.

Report đủ 14 mục.

Gateway vẫn bị chặn.

Không gọi Release Readiness/Production Readiness.

FAIL GATE

Fail nếu:

Sửa ngoài boundary.

Tạo order khi chưa Customer Confirmation accepted.

Expired quote vẫn tạo order.

order_code duplicate.

Retry tạo duplicate order.

Order created set Paid.

Order created tạo Verified Revenue.

Tạo commission/ROAS/payout.

Mở Gateway.

Không có report 14 mục.

Gọi Production Readiness/Go-live Decision.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_P3_2E_PAYMENT_SHIPPING_INVOICE_TAX.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_P3_2C_CART_DRAFT_CONFIRMATION_DONE_GATE.

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL.

## BLOCKED_TEST_OR_BUILD_FAILED.

## BLOCKED_OFFICIAL_ORDER_P0_RULE_FAILED.

NEXT FILE: PHASE-03-COMMERCE-RUNTIME/06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md

ĐIỀU KIỆN CHUYỂN TIẾP: Chỉ chuyển sang P3.2E sau khi Official Order / order_code / state machine đạt DONE GATE, order_code unique/stable, idempotency chống duplicate order hoạt động, Official Order không tự set Paid, không tạo Verified Revenue, không tạo commission/ROAS/payout và owner đồng ý handoff.

PHASE-03-COMMERCE-RUNTIME/06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md

## PROMPT-P3.2E - PAYMENT / SHIPPING / INVOICE / TAX LIMITED IMPLEMENTATION HANDOFF

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt triển khai giới hạn cho nhóm Payment / Shipping / Invoice / Tax trong PHASE-03 - Commerce Runtime.

Mục tiêu là giao dev/Codex/Copilot triển khai hoặc chỉnh sửa đúng phạm vi sau khi Official Order đã được tạo chính thức ở P3.2D.

P3.2E chỉ xử lý:

Payment Selection.

Payment State.

Payment Core Confirmation.

COD handling.

Bank Transfer handling.

Payment confirmation audit.

Shipping Quote / Shipping Request.

Shipping State.

Invoice Request.

Tax Quote / Tax Snapshot.

Payment / Shipping / Invoice / Tax handoff.

Handoff sang P3.2F cho Verified Revenue.

P3.2E không tạo Verified Revenue.

P3.2E không tạo ORDER_VERIFIED nếu checkpoint đó thuộc P3.2F.

P3.2E không tính Commission final.

P3.2E không tính Verified ROAS.

P3.2E không tạo Payout.

P3.2E không mở Gateway.

## 3. MODE

## MODE: LIMITED IMPLEMENTATION - PAYMENT / SHIPPING / INVOICE / TAX ONLY

Trong mode này, dev/Codex/Copilot chỉ được phép sửa code trong phạm vi Payment / Shipping / Invoice / Tax đã được P3.1 và P3.2D handoff cho phép.

Được phép:

Thêm/sửa PaymentSelection domain.

Thêm/sửa PaymentState domain.

Thêm/sửa PaymentConfirmation domain.

Thêm/sửa PaymentSelectionService.

Thêm/sửa PaymentConfirmationService.

Thêm/sửa COD handling rule.

Thêm/sửa Bank Transfer WAITING/confirmed/rejected flow.

Thêm/sửa ShippingQuote.

Thêm/sửa ShippingRequest nếu P3.1 duyệt.

Thêm/sửa ShippingState.

Thêm/sửa ShippingFeeResolver nếu chưa hoàn thiện ở P3.2B/P3.2E boundary.

Thêm/sửa InvoiceRequest.

Thêm/sửa TaxQuote/TaxSnapshot.

Thêm/sửa TaxResolver nếu P3.1 duyệt.

Thêm API/DTO Payment/Shipping/Invoice/Tax nếu P3.1 duyệt.

Thêm migration/seed/config nếu P3.1 duyệt.

Thêm test/smoke cho P3.2E.

Chạy backend build/test.

Chạy frontend build nếu có thay contract/UI.

Trả report 14 mục.

Không được:

Tạo Verified Revenue.

Tạo ORDER_VERIFIED nếu checkpoint đó được khóa cho P3.2F.

Tạo Commission final.

Tạo Verified ROAS.

Tạo Payout.

Tự xác nhận revenue chỉ vì paid.

Tự xác nhận revenue chỉ vì delivered.

Gọi MISA production sync ngoài integration boundary.

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

Fail-safe runtime unavailable.

Không mở Global Gate.

## 4.3. PHASE 1

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Seed không được tự set sellable.

Product/SKU/Recipe chỉ là foundation.

## 4.4. PHASE 2

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM, Gateway, IVR.

Warehouse Receipt không tự set Sellable.

Public/internal data boundary phải an toàn.

## 4.5. PHASE 3 - P3.2A

Sellable Gate là runtime gate riêng.

Runtime unavailable phải fail-safe.

Recall/Sale Lock chặn sellable.

## 4.6. PHASE 3 - P3.2B

QuoteSnapshot là nguồn sự thật cho giá cuối.

Không có QuoteSnapshot thì không có final price.

Shipping fee/VAT nếu có trong quote phải đến từ runtime/policy.

Không hardcode trong AI/Gateway.

## 4.7. PHASE 3 - P3.2C

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới là điều kiện tạo Official Order.

## 4.8. PHASE 3 - P3.2D

Official Order chỉ tạo sau Customer Confirmation accepted.

Official Order phải có order_code.

Official Order created không đồng nghĩa Paid.

Official Order created không đồng nghĩa Verified Revenue.

Official Order không tự tạo commission/ROAS/payout.

## 4.9. PHASE 3 - P3.2E

Payment selected không đồng nghĩa Paid.

Payment Core confirmation mới là paid status.

COD là phương thức nhận hàng rồi thanh toán.

Không tự thêm phí COD nếu policy chưa khóa.

Shipping fee/VAT/discount/member benefit phải do runtime tính.

Paid không đồng nghĩa Verified Revenue.

Delivered không đồng nghĩa Verified Revenue nếu chưa có verified checkpoint.

No ORDER_VERIFIED thì không final commission, không verified ROAS, không payout.

## 5. ENTRY CONDITION

Chỉ được thực hiện prompt này nếu đã hoàn tất:

## 00-P3-ANALYSIS-ONLY.md.

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md.

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md.

## 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md.

## 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md.

## 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md.

Official Order đạt DONE GATE.

Official Order chỉ tạo từ Customer Confirmation accepted.

order_code unique/stable.

Idempotency chống duplicate order hoạt động.

Official Order không tự set Paid.

Official Order không tạo Verified Revenue.

Official Order không tạo commission/ROAS/payout.

Owner đồng ý handoff sang P3.2E.

Allowed file change boundary cho P3.2E đã rõ.

Nếu thiếu điều kiện, dev/Codex/Copilot phải dừng và trả trạng thái:

## BLOCKED_NEED_P3_2D_OFFICIAL_ORDER_DONE_GATE

## 6. OBJECTIVE

Mục tiêu của P3.2E là triển khai bước vận hành sau khi có Official Order:

Cho phép khách chọn phương thức thanh toán.

Tách rõ Payment selected và Paid.

Chỉ Payment Core confirmation mới set paid.

Quản trị COD đúng bản chất: nhận hàng rồi thanh toán, chưa paid khi chọn.

Quản trị chuyển khoản đúng state: selected -> WAITING -> confirmed/rejected.

Xử lý phí vận chuyển theo runtime/policy, không hardcode.

Tạo Shipping Quote / Shipping Request theo boundary được duyệt.

Quản trị Shipping State.

Ghi nhận Invoice Request nếu khách yêu cầu.

Ghi nhận Tax Quote / Tax Snapshot.

Audit toàn bộ payment/shipping/invoice/tax.

Handoff sang P3.2F để verified revenue, commission, ROAS, payout.

P3.2E không quyết định verified revenue.

P3.2E không final commission.

P3.2E không verified ROAS.

## 7. CORE RULES

## 7.1. Payment selected không đồng nghĩa Paid

Khi khách chọn phương thức thanh toán:

Chưa paid.

Chưa verified revenue.

Chưa commission final.

Chưa verified ROAS.

Chưa payout.

Payment selected chỉ là lựa chọn phương thức thanh toán.

## 7.2. Payment Core confirmation mới là Paid

Paid chỉ được set khi có Payment Core confirmation hợp lệ.

Payment Core confirmation có thể đến từ:

Admin/Kế toán xác nhận chuyển khoản có permission.

Payment provider callback đã verify nếu có tích hợp.

COD settlement confirmation sau khi thu tiền nếu policy triển khai.

Approved equivalent confirmation source đã được owner duyệt.

Không được để AI/Gateway/CRM/Live tự set Paid.

## 7.3. COD không tự động Paid

COD là phương thức "nhận hàng rồi thanh toán".

Khi chọn COD:

Payment state chưa paid.

Không tự thêm phí COD nếu policy chưa khóa.

Không tạo verified revenue.

Không final commission.

Không verified ROAS.

## 7.4. Bank Transfer phải có state rõ

Chuyển khoản phải tách:

## SELECTED_BANK_TRANSFER.

## WAITING_TRANSFER.

## TRANSFER_WAITING_REVIEW.

## TRANSFER_CONFIRMED_PAID.

## TRANSFER_REJECTED.

TRANSFER_EXPIRED nếu áp dụng.

Không được xem khách "chọn chuyển khoản" là đã paid.

## 7.5. Shipping fee phải runtime/policy

Shipping fee phải đến từ:

Shipping policy.

Shipping resolver.

Shipping provider quote nếu có.

Approved config.

Không được hardcode phí ship trong AI/Gateway/content.

## 7.6. VAT/Tax phải runtime/policy

VAT/Tax phải đến từ:

Tax policy.

Tax resolver.

Invoice/tax module.

Approved config.

Không được hardcode VAT/tax trong AI/Gateway/content.

## 7.7. Paid không đồng nghĩa Verified Revenue

Ngay cả khi payment confirmed:

Chưa final commission.

Chưa verified ROAS.

Chưa payout.

Chưa gọi doanh thu xác minh nếu P3.2F yêu cầu ORDER_VERIFIED.

## 7.8. Shipping delivered không tự động verified revenue

Delivered có thể là checkpoint đầu vào cho P3.2F, nhưng không tự tạo verified revenue nếu policy chưa khóa.

## 7.9. Invoice issued không tự động verified revenue

Invoice/tax là nghiệp vụ chứng từ/kế toán.

Invoice issued hoặc invoice requested không đồng nghĩa:

Paid.

Delivered.

## ORDER_VERIFIED.

Verified Revenue.

Commission final.

ROAS verified.

## 8. SCOPE IN

P3.2E bao gồm:

PaymentSelection.

PaymentMethod.

PaymentState.

PaymentConfirmation.

PaymentSelectionService.

PaymentConfirmationService.

COD handling.

Bank Transfer handling.

Payment idempotency.

Payment audit.

ShippingQuote.

ShippingRequest nếu được duyệt.

ShippingState.

ShippingFeeResolver.

ShippingPolicyDecision.

InvoiceRequest.

InvoiceState.

TaxQuote.

TaxSnapshot.

TaxResolver.

Payment/Shipping/Invoice/Tax API/DTO nếu P3.1 duyệt.

Migration/seed/config nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2E.

Handoff sang P3.2F.

## 9. SCOPE OUT

P3.2E không bao gồm:

Không tạo Verified Revenue.

Không tạo ORDER_VERIFIED nếu checkpoint này thuộc P3.2F.

Không tạo Commission final.

Không tạo Verified ROAS.

Không tạo Payout.

Không quyết toán thưởng.

Không tính KPI final.

Không sync MISA production ngoài integration boundary được duyệt.

Không triển khai Ads revenue attribution final.

Không triển khai IVR.

Không mở Gateway.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 10. ALLOWED FILE CHANGE BOUNDARY

Dev/Codex/Copilot chỉ được sửa file thuộc boundary đã được P3.1/P3.2D handoff cho phép.

## 10.1. Nhóm file được phép sửa nếu đã duyệt

Nhóm file

Được sửa

Ghi chú

Payment domain

Có

PaymentSelection/PaymentState/PaymentConfirmation

Payment service

Có

Selection/confirmation/idempotency

COD handling

Có

Không auto paid

Bank transfer handling

Có

WAITING/confirmed/rejected

Shipping domain

Có

ShippingQuote/ShippingRequest/ShippingState

Shipping service/resolver

Có

Fee/runtime/policy

Invoice domain

Có

InvoiceRequest/InvoiceState

Tax domain/service

Có

TaxQuote/TaxSnapshot/TaxResolver

API/DTO Payment/Shipping/Invoice/Tax

Có điều kiện

Chỉ nếu P3.1 duyệt

Migration

Có điều kiện

Chỉ Payment/Shipping/Invoice/Tax

Seed/config dev-test

Có điều kiện

Không tạo verified revenue

Backend tests

Có

Chỉ P3.2E

Frontend/Admin UI

Có điều kiện

Chỉ nếu P3.1 duyệt

Event/outbox

Có điều kiện

Chỉ payment/shipping/invoice/tax events nếu duyệt

Markdown/handoff

Có điều kiện

Chỉ cập nhật P3.2E report

## 10.2. Nhóm file không được sửa

Nhóm file

Không được sửa

Lý do

Verified Revenue implementation

Không

Thuộc P3.2F

ORDER_VERIFIED checkpoint

Không mặc định

Thuộc P3.2F nếu chưa duyệt trong P3.2E

Commission final

Không

Thuộc P3.2F/Finance

ROAS verified

Không

Thuộc P3.2F/Ads

Payout

Không

Thuộc Finance/Reward

Gateway config

Không

IVR implementation

Không

PACK-09 reserved

MISA production sync

Không mặc định

Chỉ integration layer riêng nếu đã duyệt

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## 11. REQUIRED INPUT

## 11.1. Từ P3.2D

OfficialOrder.

order_id.

order_code.

order_state.

final_payable_amount_snapshot.

payment_state_initial.

shipping_state_initial.

revenue_state_initial.

order lines.

customer/delivery snapshot.

audit/correlation.

handoff_ready_for_payment_shipping.

## 11.2. Từ P3.2B

QuoteSnapshot.

shipping_fee snapshot nếu đã có.

tax snapshot nếu đã có.

final payable amount.

policy version.

tax/shipping decision references nếu có.

## 11.3. Từ payment policy

Supported payment methods.

COD policy.

Bank transfer policy.

Payment confirmation source.

Payment timeout policy.

Payment proof/evidence policy.

Admin confirmation permission.

Provider callback signature policy nếu có.

Refund/reversal placeholder nếu có.

## 11.4. Từ shipping policy

Shipping provider.

Shipping fee calculation.

Free shipping rule nếu có.

Shipping area/zone.

ETA policy.

Shipping request creation rule.

Shipping state policy.

Failed shipping handling.

Delivery confirmation source.

## 11.5. Từ invoice/tax policy

VAT/tax applicability.

Invoice request required fields.

Invoice state policy.

Tax calculation policy.

MISA handoff boundary nếu có.

Evidence requirement.

Customer invoice info.

## 12. TARGET BEHAVIOR

## 12.1. Payment method selected

Khi khách chọn payment method:

Gắn payment method vào order.

Ghi PaymentSelection.

Update payment_state theo method.

Không set Paid.

Không tạo Verified Revenue.

Không final commission.

Không verified ROAS.

Audit payment selection.

## 12.2. COD selected

Khi khách chọn COD:

paid = false.

Không thêm phí COD nếu policy không có.

Nếu policy có COD fee thì phải từ runtime/policy, không hardcode.

Không tạo verified revenue.

## 12.3. Bank transfer selected

Khi khách chọn chuyển khoản:

payment_method = BANK_TRANSFER.

paid = false.

Có payment instruction nếu policy cho phép.

Có proof upload/reference nếu policy cho phép.

Không set paid khi khách chỉ nói "đã chuyển".

## 12.4. Payment confirmed

Payment confirmed chỉ khi:

Payment Core confirmation source hợp lệ.

Amount match theo policy.

Order match.

Actor/permission hợp lệ nếu manual confirmation.

Provider signature hợp lệ nếu callback.

Idempotency pass.

Audit/evidence đầy đủ.

Sau khi payment confirmed:

payment_state = PAID hoặc CONFIRMED_PAID.

order_state có thể chuyển theo state machine nếu P3.1 duyệt.

Không tự tạo Verified Revenue.

Handoff sang P3.2F nếu cần.

## 12.5. Shipping quote/request

Shipping quote/request chỉ tạo khi:

Official Order tồn tại.

Delivery data đủ.

Shipping policy available.

Shipping fee resolved.

No sale/recall block ảnh hưởng fulfillment nếu policy yêu cầu.

Payment requirement satisfied nếu policy yêu cầu trước shipping.

## 12.6. Invoice request

Invoice request chỉ tạo khi:

Official Order tồn tại.

Customer invoice info đủ.

Tax policy available.

Invoice request không trùng.

Audit đầy đủ.

Invoice request không đồng nghĩa invoice issued nếu chưa có issuance flow.

## 12.7. Tax quote/snapshot

Tax quote/snapshot phải:

Dựa trên order/quote snapshot.

Dựa trên tax policy.

Có policy version.

Không hardcode.

Không mutate quote snapshot cũ nếu đã immutable.

Nếu cần correction, tạo tax adjustment path có audit.

## 13. DOMAIN CONTRACT

## 13.1. PaymentSelection

Trường đề xuất:

payment_selection_id.

order_id.

order_code.

selected_method.

selected_at.

selected_by_customer_id nullable.

selected_by_session_id nullable.

selected_by_actor_id nullable.

channel.

payment_instruction_snapshot nullable.

payment_policy_version.

idempotency_key.

correlation_id.

audit_ref.

PaymentMethod

## COD.

## BANK_TRANSFER.

## ONLINE_PAYMENT_PROVIDER.

WALLET_PROVIDER nếu tương lai.

MANUAL_ADMIN_ASSISTED nếu policy cho phép.

## FUTURE_RESERVED.

## 13.2. PaymentState

State đề xuất:

## NOT_SELECTED.

## COD_SELECTED.

## WAITING_COD_COLLECTION.

## BANK_TRANSFER_SELECTED.

## WAITING_TRANSFER.

## TRANSFER_WAITING_REVIEW.

## PAYMENT_PROVIDER_WAITING.

## CONFIRMED_PAID.

## REJECTED.

## FAILED.

## EXPIRED.

REFUND_WAITING nếu chỉ placeholder.

REFUNDED nếu được duyệt.

Rule:

CONFIRMED_PAID mới là paid.

Các state selected/WAITING không phải paid.

## 13.3. PaymentConfirmation

Trường đề xuất:

payment_confirmation_id.

order_id.

order_code.

payment_method.

confirmation_source.

confirmed_amount.

expected_amount.

amount_match_status.

confirmed_at.

confirmed_by_actor_id nullable.

provider_transaction_id nullable.

bank_reference nullable.

evidence_ref nullable.

confirmation_status.

idempotency_key.

correlation_id.

audit_ref.

PaymentConfirmationSource

## ADMIN_ACCOUNTING_CONFIRMATION.

## PAYMENT_PROVIDER_CALLBACK.

## BANK_RECONCILIATION.

## COD_COLLECTION_CONFIRMATION.

## APPROVED_MANUAL_CONFIRMATION.

## 13.4. ShippingQuote

Trường đề xuất:

shipping_quote_id.

order_id.

order_code.

shipping_provider nullable.

shipping_zone.

delivery_address_snapshot.

shipping_fee_amount.

shipping_discount_amount nullable.

final_shipping_fee_amount.

eta_min_days nullable.

eta_max_days nullable.

policy_version.

quote_status.

created_at.

expires_at nullable.

correlation_id.

audit_ref.

## 13.5. ShippingRequest

Trường đề xuất:

shipping_request_id.

order_id.

order_code.

shipping_quote_id nullable.

provider_code nullable.

shipping_state.

requested_at.

requested_by_actor_id nullable.

delivery_address_snapshot.

receiver_name_snapshot.

receiver_phone_snapshot.

cod_amount_snapshot nếu COD.

shipping_fee_snapshot.

correlation_id.

audit_ref.

provider_ref nullable.

ShippingState

## NOT_REQUESTED.

## READY_TO_REQUEST.

## REQUESTED.

## PROVIDER_ACCEPTED.

## PROVIDER_REJECTED.

## PICKUP_WAITING.

## IN_TRANSIT.

## DELIVERY_ATTEMPTED.

## DELIVERED.

## FAILED.

## CANCELLED.

## RETURNING.

## RETURNED.

Rule:

DELIVERED không tự tạo Verified Revenue nếu P3.2F chưa xử lý.

Shipping request không tự set Paid.

## 13.6. InvoiceRequest

Trường đề xuất:

invoice_request_id.

order_id.

order_code.

customer_id nullable.

invoice_buyer_name.

invoice_tax_code nullable.

invoice_company_name nullable.

invoice_address nullable.

invoice_email nullable.

invoice_state.

requested_at.

requested_by_customer_id nullable.

requested_by_actor_id nullable.

correlation_id.

audit_ref.

misa_handoff_ref nullable.

InvoiceState

## NOT_REQUESTED.

## REQUESTED.

## VALIDATION_WAITING.

## VALIDATED.

## REJECTED_MISSING_INFO.

## ISSUANCE_WAITING.

ISSUED nếu boundary duyệt.

## FAILED.

## CANCELLED.

## 13.7. TaxQuote / TaxSnapshot

Trường đề xuất:

tax_quote_id.

order_id.

order_code.

quote_snapshot_id.

taxable_amount.

tax_rate nullable.

tax_amount.

tax_policy_version.

tax_included_flag.

created_at.

correlation_id.

audit_ref.

Rule:

Không hardcode VAT/tax.

Nếu tax policy unavailable, fail-safe theo rule.

TaxSnapshot không tạo verified revenue.

## 14. SERVICE CONTRACT

## 14.1. PaymentSelectionService

Input:

order_id.

order_code.

payment_method.

customer/session context.

channel.

idempotency_key.

correlation_id.

Output:

payment_selection_id.

payment_state.

paid = false.

payment_instruction_payload nullable.

audit_ref.

Rules:

Order must exist.

Order must be official.

Order must not be cancelled.

Payment method must be supported.

COD selected not paid.

Bank transfer selected not paid.

Do not create verified revenue.

Audit append-only.

## 14.2. PaymentConfirmationService

Input:

order_id.

payment_method.

confirmation_source.

confirmed_amount.

provider_transaction_id nullable.

bank_reference nullable.

evidence_ref nullable.

actor_context or provider_signature.

idempotency_key.

correlation_id.

Output:

payment_confirmation_id.

payment_state.

paid_status.

amount_match_status.

audit_ref.

handoff_ready_for_verified_revenue_check.

Rules:

Validate order.

Validate payment selection if required.

Validate amount.

Validate source.

Validate permission/signature.

Idempotency required.

Set paid only when confirmation valid.

Do not create Verified Revenue.

Do not create Commission/ROAS/Payout.

## 14.3. ShippingFeeResolver

Input:

order_id.

delivery_address.

shipping_zone.

order amount.

program/member/shipping policy context.

correlation_id.

Output:

shipping_fee_amount.

shipping_discount_amount.

final_shipping_fee_amount.

eta.

policy_version.

block_reason nullable.

Rules:

Must use runtime policy.

No hardcode.

If unavailable, fail-safe.

Must not mutate QuoteSnapshot.

## 14.4. ShippingRequestService

Input:

order_id.

shipping_quote_id nullable.

provider selection nullable.

actor_context nullable.

idempotency_key.

correlation_id.

Output:

shipping_request_id.

shipping_state.

provider_ref nullable.

audit_ref.

Rules:

Official Order required.

Delivery data required.

Shipping policy available.

If payment-before-shipping policy applies, require paid.

Shipping request does not set paid.

Shipping delivered does not create verified revenue in P3.2E.

## 14.5. InvoiceRequestService

Input:

order_id.

invoice customer info.

requested_by.

idempotency_key.

correlation_id.

Output:

invoice_request_id.

invoice_state.

missing_fields nullable.

audit_ref.

Rules:

Official Order required.

Required invoice info must pass validation.

Duplicate invoice request bị chặn or returns existing according to policy.

MISA handoff only through integration layer if approved.

Invoice request does not create verified revenue.

## 14.6. TaxResolver

Input:

order_id.

quote_snapshot_id.

line amounts.

tax policy context.

correlation_id.

Output:

tax_quote_id.

taxable_amount.

tax_rate.

tax_amount.

tax_policy_version.

audit_ref.

Rules:

Must use tax policy.

No hardcode.

If missing policy, fail-safe.

Does not create verified revenue.

## 15. STATE MACHINE

## 15.1. Payment state transition

Allowed:

## NOT_SELECTED -> COD_SELECTED.

## COD_SELECTED -> WAITING_COD_COLLECTION.

## NOT_SELECTED -> BANK_TRANSFER_SELECTED.

## BANK_TRANSFER_SELECTED -> WAITING_TRANSFER.

## WAITING_TRANSFER -> TRANSFER_WAITING_REVIEW.

## TRANSFER_WAITING_REVIEW -> CONFIRMED_PAID.

## TRANSFER_WAITING_REVIEW -> REJECTED.

## PAYMENT_PROVIDER_WAITING -> CONFIRMED_PAID.

## PAYMENT_PROVIDER_WAITING -> FAILED.

WAITING_COD_COLLECTION -> CONFIRMED_PAID nếu COD settlement confirmed.

Forbidden:

COD_SELECTED -> CONFIRMED_PAID without confirmation.

BANK_TRANSFER_SELECTED -> CONFIRMED_PAID without confirmation.

Any selected/WAITING state -> Verified Revenue.

CONFIRMED_PAID -> Commission final.

CONFIRMED_PAID -> ROAS verified.

CONFIRMED_PAID -> Payout.

## 15.2. Shipping state transition

Allowed:

## NOT_REQUESTED -> READY_TO_REQUEST.

## READY_TO_REQUEST -> REQUESTED.

## REQUESTED -> PROVIDER_ACCEPTED.

## REQUESTED -> PROVIDER_REJECTED.

## PROVIDER_ACCEPTED -> PICKUP_WAITING.

## PICKUP_WAITING -> IN_TRANSIT.

## IN_TRANSIT -> DELIVERY_ATTEMPTED.

## IN_TRANSIT -> DELIVERED.

## DELIVERY_ATTEMPTED -> DELIVERED.

## DELIVERY_ATTEMPTED -> FAILED.

REQUESTED/PROVIDER_ACCEPTED/PICKUP_WAITING -> CANCELLED nếu policy cho phép.

## FAILED -> RETURNING.

## RETURNING -> RETURNED.

Forbidden:

Shipping delivered -> Verified Revenue in P3.2E.

Shipping requested -> Paid.

Shipping request -> Invoice issued unless separate invoice flow.

Shipping state -> Commission/ROAS/Payout.

## 15.3. Invoice state transition

Allowed:

## NOT_REQUESTED -> REQUESTED.

## REQUESTED -> VALIDATION_WAITING.

## VALIDATION_WAITING -> VALIDATED.

## VALIDATION_WAITING -> REJECTED_MISSING_INFO.

## VALIDATED -> ISSUANCE_WAITING.

ISSUANCE_WAITING -> ISSUED nếu boundary duyệt.

## ISSUANCE_WAITING -> FAILED.

## REQUESTED/VALIDATION_WAITING -> CANCELLED.

Forbidden:

Invoice requested -> Paid.

Invoice issued -> Verified Revenue.

Invoice issued -> Commission final.

Invoice issued -> ROAS verified.

## 15.4. Order state interaction

P3.2E có thể chuyển Order State theo boundary duyệt:

## WAITING_PAYMENT -> PAYMENT_SELECTED.

## PAYMENT_SELECTED -> PAYMENT_WAITING_CONFIRMATION.

## PAYMENT_WAITING_CONFIRMATION -> PAID.

PAID -> FULFILLMENT_WAITING hoặc SHIPPING_WAITING nếu policy cho phép.

SHIPPING_WAITING -> SHIPPING_IN_PROGRESS nếu shipping request accepted.

SHIPPING_IN_PROGRESS -> DELIVERED nếu delivery confirmed.

Không được chuyển:

DELIVERED -> ORDER_VERIFIED nếu thuộc P3.2F.

## PAID -> ORDER_VERIFIED.

Any state -> VERIFIED_REVENUE_CREATED.

## 16. API / DTO CONTRACT - IF APPROVED BY P3.1

## 16.1. Select Payment Method API

POST /api/commerce/orders/{order_id}/payment-selection

Request:

payment_method.

customer_id nullable.

session_id nullable.

channel.

idempotency_key.

correlation_id.

Response:

order_id.

order_code.

payment_selection_id.

payment_method.

payment_state.

paid = false.

payment_instruction_payload nullable.

no_verified_revenue_yet = true.

## 16.2. Confirm Payment API

POST /api/commerce/orders/{order_id}/payment-confirmation

Request:

payment_method.

confirmation_source.

confirmed_amount.

evidence_ref nullable.

provider_transaction_id nullable.

bank_reference nullable.

idempotency_key.

correlation_id.

Response:

order_id.

order_code.

payment_confirmation_id.

payment_state.

paid_status.

amount_match_status.

handoff_ready_for_verified_revenue_check.

no_verified_revenue_yet = true.

no_commission_final_yet = true.

no_verified_roas_yet = true.

## 16.3. Create Shipping Quote API

POST /api/commerce/orders/{order_id}/shipping-quote

Request:

delivery_address optional if already on order.

shipping_provider nullable.

channel.

correlation_id.

Response:

shipping_quote_id.

shipping_fee_amount.

shipping_discount_amount nullable.

final_shipping_fee_amount.

eta_min_days nullable.

eta_max_days nullable.

policy_version.

## 16.4. Create Shipping Request API

POST /api/commerce/orders/{order_id}/shipping-request

Request:

shipping_quote_id nullable.

provider_code nullable.

idempotency_key.

correlation_id.

Response:

shipping_request_id.

shipping_state.

provider_ref nullable.

no_verified_revenue_yet = true.

## 16.5. Create Invoice Request API

POST /api/commerce/orders/{order_id}/invoice-request

Request:

invoice_buyer_name.

invoice_tax_code nullable.

invoice_company_name nullable.

invoice_address nullable.

invoice_email nullable.

idempotency_key.

correlation_id.

Response:

invoice_request_id.

invoice_state.

missing_fields nullable.

no_verified_revenue_yet = true.

## 16.6. Tax Quote API

POST /api/commerce/orders/{order_id}/tax-quote

Request:

tax_context.

correlation_id.

Response:

tax_quote_id.

taxable_amount.

tax_amount.

tax_policy_version.

## 16.7. Public/internal response boundary

Public/channel response được phép có:

order_code.

payment method selected.

payment state public-safe.

payment instruction nếu policy cho phép.

shipping fee/ETA public-safe.

invoice request status public-safe.

paid status nếu confirmed.

delivery status public-safe.

Không được lộ:

Internal payment reconciliation notes.

Bank matching internal rule.

Provider secret/signature.

Internal margin/cost.

Internal tax mapping.

MISA internal id nếu policy không cho phép.

Commission.

## ROAS.

Payout.

Internal audit note.

## 17. DATABASE / MIGRATION RULE

Nếu P3.1 duyệt migration, bảng/entity P3.2E có thể gồm:

commerce_payment_selection.

commerce_payment_confirmation.

commerce_payment_state_transition.

commerce_shipping_quote.

commerce_shipping_request.

commerce_shipping_state_transition.

commerce_invoice_request.

commerce_invoice_state_transition.

commerce_tax_quote.

commerce_tax_snapshot.

commerce_payment_idempotency nếu chưa có idempotency chung.

commerce_shipping_idempotency nếu cần.

Không tạo bảng:

commerce_verified_revenue.

commerce_order_verified.

commerce_commission_final.

commerce_roas_verified.

commerce_payout.

IVR production tables.

## 17.1. Migration safety

Migration phải:

Không phá dữ liệu cũ.

Không set paid mặc định cho existing orders nếu chưa có confirmation.

Không tạo verified revenue.

Không tạo commission/ROAS/payout.

Có constraint trạng thái hợp lệ.

Có unique/idempotency constraint cho payment confirmation.

Có index cho order_id, order_code, payment_state, shipping_state, invoice_state.

Có audit/correlation fields.

Không hardcode COD fee.

Không hardcode VAT.

Không mở Gateway.

## 18. SEED / CONFIG RULE

Seed/config dev-test được phép tạo:

Payment method policy.

COD policy sample.

Bank transfer instruction sample.

Payment confirmation test fixture.

Shipping fee policy sample.

Shipping zone sample.

Tax policy sample.

Invoice required field policy.

Negative fixtures:

COD selected not paid.

Bank transfer selected not paid.

Wrong amount confirmation.

Missing payment evidence.

Missing shipping address.

Missing invoice info.

Missing tax policy.

Seed/config không được:

Set paid without confirmation.

Tạo verified revenue.

Tạo ORDER_VERIFIED.

Tạo commission final.

Tạo verified ROAS.

Tạo payout.

Hardcode COD fee nếu policy không có.

Hardcode VAT ngoài tax policy.

Mở Gateway.

## 19. SECURITY / GOVERNANCE GUARDRAILS

## 19.1. RBAC

Bắt buộc backend permission cho:

Manual payment confirmation.

Payment rejection.

Payment evidence review.

Shipping request manual create/cancel nếu admin thao tác.

Invoice request review.

Tax quote override nếu có.

View internal payment/shipping/invoice/tax data.

Export evidence.

## 19.2. Actor Context

Bắt buộc cho:

Manual payment confirmation.

Manual payment rejection.

Shipping request by admin.

Invoice validation by admin.

Tax override nếu có.

Any state override.

## 19.3. Correlation ID

Mọi command phải có correlation_id:

Select payment.

Confirm payment.

Reject payment.

Create shipping quote.

Create shipping request.

Update shipping state.

Create invoice request.

Create tax quote.

## 19.4. Audit append-only

Audit bắt buộc cho:

Payment selected.

Payment confirmed.

Payment rejected.

Payment failed.

Shipping quote created.

Shipping request created.

Shipping state changed.

Invoice requested.

Invoice state changed.

Tax quote created.

Manual override.

## 19.5. Idempotency

Idempotency bắt buộc cho:

Select payment method nếu side effect.

Confirm payment.

Provider callback.

Create shipping request.

Create invoice request.

Any tax quote saved as side effect.

## 19.6. Evidence Registry

Evidence submitted chưa phải accepted.

Không gọi Completion Decision nếu chưa owner review.

## 19.7. Runtime unavailable

Nếu runtime unavailable:

Không confirm payment nếu không validate được.

Không tạo shipping quote nếu thiếu shipping policy.

Không tạo tax quote nếu thiếu tax policy.

Không tạo invoice handoff nếu thiếu required policy.

Không tạo partial side effect.

## 19.8. Transaction safety

Các command side effect phải atomic trong phạm vi business transaction:

Validate order.

Validate policy.

Apply state transition.

Write audit/state log.

Write outbox if approved.

Commit together where required.

## 20. EVENT / OUTBOX RULE - IF APPROVED

Nếu P3.1 duyệt event/outbox, P3.2E có thể phát các event:

## PAYMENT_METHOD_SELECTED.

## PAYMENT_CONFIRMED.

## PAYMENT_REJECTED.

## SHIPPING_QUOTE_CREATED.

## SHIPPING_REQUESTED.

## SHIPPING_STATE_CHANGED.

## INVOICE_REQUESTED.

## TAX_QUOTE_CREATED.

Rule:

PAYMENT_CONFIRMED không đồng nghĩa Verified Revenue.

SHIPPING_DELIVERED không đồng nghĩa Verified Revenue nếu chưa qua P3.2F.

INVOICE_REQUESTED/ISSUED không đồng nghĩa Verified Revenue.

Ads/ROAS không được dùng các event này làm verified revenue.

Commission không được final từ các event này.

Payout không được mở từ các event này.

## 21. TEST REQUIREMENTS

## 21.1. Unit test - Payment selection

Select COD -> payment state COD_SELECTED/WAITING_COD_COLLECTION, paid false.

Select bank transfer -> WAITING_TRANSFER, paid false.

Select unsupported method -> reject.

Select payment for missing order -> reject.

Select payment for cancelled order -> reject.

Payment selected does not create verified revenue.

Payment selected does not create commission/ROAS/payout.

COD fee absent if policy missing.

## 21.2. Unit test - Payment confirmation

Valid bank transfer confirmation -> CONFIRMED_PAID.

Wrong amount -> reject or review WAITING by policy.

Missing evidence when required -> reject.

Invalid actor permission -> reject.

Invalid provider signature -> reject.

Duplicate callback -> idempotent, no duplicate confirmation.

Confirmed paid does not create verified revenue.

Confirmed paid does not create commission final.

Confirmed paid does not create verified ROAS.

Confirmed paid does not create payout.

## 21.3. Unit test - Shipping

Shipping quote uses policy.

Missing shipping policy -> fail-safe.

Missing delivery address -> reject shipping request.

Shipping request created for valid order.

Shipping request does not set paid.

Delivered does not create verified revenue in P3.2E.

Shipping state transition invalid -> reject.

Shipping state transition audit created.

## 21.4. Unit test - Invoice / Tax

Tax quote uses tax policy.

Missing tax policy -> fail-safe.

Invoice request with required info -> REQUESTED.

Invoice request missing info -> REJECTED_MISSING_INFO.

Duplicate invoice request -> no duplicate effect.

Invoice requested does not set paid.

Invoice issued does not create verified revenue.

Tax/VAT not hardcoded.

## 21.5. Integration test nếu có API

POST payment selection COD -> not paid.

POST payment selection bank transfer -> not paid.

POST payment confirmation valid -> paid.

POST duplicate payment confirmation -> idempotent.

POST shipping quote -> policy result.

POST shipping request -> requested.

POST invoice request -> requested/validation state.

POST tax quote -> policy result.

Public response does not leak internal data.

No verified revenue created.

## 22. SMOKE REQUIREMENTS

## 22.1. P3-SMK-2E-001 - COD selected is not Paid

Case: Official Order chọn COD.
Expected: payment_state = COD_SELECTED/WAITING_COD_COLLECTION, paid false.

## 22.2. P3-SMK-2E-002 - Bank transfer selected is not Paid

Case: Official Order chọn chuyển khoản.
Expected: payment_state = WAITING_TRANSFER, paid false.

## 22.3. P3-SMK-2E-003 - Payment confirmed by valid source

Case: Kế toán/provider xác nhận thanh toán hợp lệ.
Expected: payment_state = CONFIRMED_PAID.

## 22.4. P3-SMK-2E-004 - Payment selected does not create revenue

Case: Payment selected.
Expected: Không có Verified Revenue.

## 22.5. P3-SMK-2E-005 - Paid does not create Verified Revenue

Case: Payment confirmed paid.
Expected: Không tạo Verified Revenue trong P3.2E.

## 22.6. P3-SMK-2E-006 - COD fee not auto-added

Case: COD selected nhưng policy không có COD fee.
Expected: Không thêm phí COD.

## 22.7. P3-SMK-2E-007 - Shipping fee uses runtime policy

Case: Tạo shipping quote.
Expected: Shipping fee đến từ policy/resolver, không hardcode.

## 22.8. P3-SMK-2E-008 - Shipping request does not set paid

Case: Tạo Shipping Request.
Expected: Không tự set paid.

## 22.9. P3-SMK-2E-009 - Delivered does not create verified revenue

Case: Shipping state DELIVERED nếu flow test có.
Expected: Không tạo Verified Revenue trong P3.2E.

## 22.10. P3-SMK-2E-010 - Tax uses runtime policy

Case: Tạo Tax Quote.
Expected: Tax/VAT từ policy, không hardcode.

## 22.11. P3-SMK-2E-011 - Invoice request does not create revenue

Case: Invoice Request created.
Expected: Không tạo Verified Revenue.

## 22.12. P3-SMK-2E-012 - No commission / ROAS / payout

Case: Payment confirmed + shipping requested.
Expected: Không commission final, không verified ROAS, không payout.

## 22.13. P3-SMK-2E-013 - Public response safe

Case: Public/channel đọc payment/shipping/invoice status.
Expected: Không lộ internal payment evidence, bank rule, tax mapping, commission, ROAS.

## 22.14. P3-SMK-2E-014 - Gateway remains bị chặn

## 23. EVIDENCE REQUIREMENTS

P3.2E cần gom evidence:

Git diff trong allowed boundary.

File list đã sửa.

Migration diff nếu có.

Seed/config diff nếu có.

Unit test result.

Integration test result nếu có API.

Smoke result P3-SMK-2E-001 -> P3-SMK-2E-014.

Backend build result.

Frontend build result nếu có ảnh hưởng UI.

COD selected not paid evidence.

Bank transfer selected not paid evidence.

Payment confirmed paid evidence.

Payment selected no revenue evidence.

Paid no verified revenue evidence.

COD fee not auto-added evidence.

Shipping fee runtime policy evidence.

Shipping request not paid evidence.

Delivered not verified revenue evidence.

Tax runtime policy evidence.

Invoice request no revenue evidence.

No commission/ROAS/payout evidence.

Public/internal boundary evidence.

Audit evidence.

Idempotency evidence.

Handoff note sang P3.2F.

Lưu ý:

Evidence submitted chưa phải Evidence accepted.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

## 24. EXECUTION STEPS

Step 1 - Confirm Mode

Xác nhận:

## MODE: LIMITED IMPLEMENTATION - PAYMENT / SHIPPING / INVOICE / TAX ONLY

Step 2 - Read P3.2D Done Report

Xác nhận:

Official Order đã đạt DONE GATE.

order_code unique/stable.

Official Order không set Paid.

Official Order không tạo Verified Revenue.

Payment/shipping/revenue chưa bị scope creep.

Nếu chưa đạt, dừng.

Step 3 - Inspect Existing Payment/Shipping/Invoice/Tax Logic

Kiểm tra codebase hiện tại:

Có payment module cũ không.

Có flow nào xem payment selected là paid không.

Có COD fee hardcode không.

Có bank transfer flow không.

Có payment confirmation/callback không.

Có shipping fee hardcode không.

Có shipping request flow không.

Có invoice/tax module không.

Có VAT hardcode không.

Có flow nào tạo verified revenue từ paid/delivered/invoice không.

Step 4 - Implement Payment Selection

Triển khai/chỉnh:

PaymentSelection.

PaymentMethod.

PaymentSelectionService.

COD selected not paid.

Bank transfer selected not paid.

Audit/idempotency.

Step 5 - Implement Payment Confirmation

Triển khai/chỉnh:

PaymentConfirmation.

PaymentConfirmationService.

Confirmation source validation.

Amount match validation.

Permission/signature validation.

Paid state transition.

No verified revenue.

Step 6 - Implement Shipping

Triển khai/chỉnh:

ShippingQuote.

ShippingFeeResolver.

ShippingRequest nếu duyệt.

ShippingState.

Shipping state transition.

No paid/no verified revenue side effect.

Step 7 - Implement Invoice / Tax

Triển khai/chỉnh:

InvoiceRequest.

InvoiceState.

TaxQuote.

TaxSnapshot.

TaxResolver.

No hardcode VAT.

No verified revenue side effect.

Step 8 - Implement API/DTO nếu approved

Nếu P3.1 duyệt, triển khai:

Payment Selection API.

Payment Confirmation API.

Shipping Quote API.

Shipping Request API.

Invoice Request API.

Tax Quote API.

Nếu chưa duyệt, không tự tạo API.

Step 9 - Implement Migration/Seed nếu approved

Chỉ trong phạm vi Payment/Shipping/Invoice/Tax.

Không tạo VerifiedRevenue/Commission/ROAS/Payout migration.

Step 10 - Add Tests

Thêm/cập nhật test theo mục Test Requirements.

Step 11 - Run Build/Test/Smoke

Chạy:

Backend build.

Backend test.

Integration test nếu có.

Frontend build nếu ảnh hưởng UI.

Smoke P3-SMK-2E.

Step 12 - Evidence Report

Gom evidence và trả report 14 mục.

Step 13 - Handoff

Kết luận có được chuyển sang P3.2F hay không.

## 25. REQUIRED REPORT FORMAT - 14 MỤC

## 25.1. Tóm tắt

Ghi rõ:

Đã triển khai Payment phần nào.

Đã triển khai Shipping phần nào.

Đã triển khai Invoice/Tax phần nào.

Payment selected đã tách Paid chưa.

Test/smoke kết quả thế nào.

Có đủ điều kiện sang P3.2F không.

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

P3.2D Done Report.

Prompt P3.2E.

## 25.4. Evidence đã dùng

Liệt kê:

Code references.

DB/migration references.

Official Order evidence.

Payment evidence.

Shipping evidence.

Invoice/tax evidence.

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

Không tạo verified revenue/commission/ROAS/payout.

## 25.12. Kết quả seed validation nếu áp dụng

Ghi:

Có seed không.

Seed có set paid không cần confirmation không.

Seed có tạo verified revenue không.

Seed có tạo commission/ROAS/payout không.

Seed có hardcode COD fee/VAT sai không.

Seed validation result.

## 25.13. Rủi ro còn lại

Phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Owner decision needed.

Deferred to P3.2F.

Technical debt.

## 25.14. Cập nhật handoff

Ghi rõ:

Có được sang P3.2F không.

Điều kiện còn thiếu nếu chưa.

File tiếp theo.

Handoff notes cho Verified Revenue / Commission / ROAS.

## 26. DONE GATE

P3.2E được DONE khi đủ:

Chỉ sửa file trong allowed boundary.

Payment selected không đồng nghĩa Paid.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới set paid.

Payment confirmation có permission/signature/evidence theo policy.

Duplicate payment confirmation idempotent.

COD không tự thêm phí nếu policy chưa khóa.

Shipping fee đến từ runtime/policy, không hardcode.

Shipping request không tự set paid.

Delivered không tự tạo Verified Revenue trong P3.2E.

Tax/VAT đến từ runtime/policy, không hardcode.

Invoice request không tự tạo revenue.

Paid không tự tạo Verified Revenue.

Paid không tạo Commission final.

Paid không tạo Verified ROAS.

Paid không tạo Payout.

Public/internal response boundary đúng.

Audit/correlation đầy đủ.

Idempotency đầy đủ cho command side effect.

Test bắt buộc pass hoặc điểm chặn rõ.

Smoke P3-SMK-2E pass hoặc điểm chặn rõ.

Backend build pass hoặc điểm chặn rõ.

Frontend build pass nếu có ảnh hưởng UI.

Evidence submitted.

Report đủ 14 mục.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 27. FAIL GATE

P3.2E bị FAIL nếu:

Sửa file ngoài allowed boundary.

Payment selected set Paid.

COD selected set Paid.

Bank transfer selected set Paid.

Paid được set không qua Payment Core confirmation.

AI/Gateway/CRM/Live set Paid.

COD tự thêm phí khi policy chưa khóa.

Shipping fee hardcode.

VAT/tax hardcode.

Shipping request tự set paid.

Payment confirmed tự tạo Verified Revenue.

Delivered tự tạo Verified Revenue.

Invoice requested/issued tự tạo Verified Revenue.

Paid tự tạo Commission final.

Paid tự tạo Verified ROAS.

Paid tự tạo Payout.

Provider callback không verify signature nếu policy yêu cầu.

Manual confirmation không có permission/audit.

Public response lộ internal payment/tax/commission data.

Không có test negative case.

Không có report 14 mục.

Mở Gateway.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

## 28. DOWNSTREAM HANDOFF

## 28.1. File tiếp theo

# P3.2F - VERIFIED REVENUE COMMISSION ROAS HANDOFF

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- TECH-04 Commerce Runtime verified revenue checkpoint.
- TECH-07 Ads Measurement / Attribution / ROAS.
- PACK-07 Ads / ROAS / Attribution.
- PACK-04 MISA Accounting Handoff boundary.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md

## 28.2. Prompt tiếp theo

## PROMPT-P3.2F - VERIFIED REVENUE / COMMISSION / ROAS HANDOFF LIMITED IMPLEMENTATION

## 28.3. Điều kiện sang P3.2F

Chỉ chuyển sang P3.2F nếu:

P3.2E đạt DONE GATE.

Payment selected tách Paid rõ ràng.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới paid.

Paid chưa tạo Verified Revenue.

Delivered chưa tạo Verified Revenue trong P3.2E.

Invoice request/issued chưa tạo Verified Revenue.

Không có commission final/verified ROAS/payout trong P3.2E.

Payment/shipping/invoice/tax có audit/evidence.

Evidence submitted đầy đủ.

Owner đồng ý handoff.

## 28.4. Không chuyển nếu

Payment selected còn bị hiểu là paid.

COD tự paid.

Bank transfer tự paid.

Paid tạo revenue.

Delivered tạo revenue.

Invoice tạo revenue.

Commission/ROAS/payout bị scope creep.

Shipping/tax hardcode.

Gateway bị mở.

Report thiếu evidence.

## 29. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3.2E - PAYMENT / SHIPPING / INVOICE / TAX LIMITED IMPLEMENTATION HANDOFF

## MODE: LIMITED IMPLEMENTATION - PAYMENT / SHIPPING / INVOICE / TAX ONLY

Bạn đang thực hiện PHASE-03 - Commerce Runtime cho dự án Ginsengfood.

Nhiệm vụ duy nhất của bạn là triển khai hoặc chỉnh sửa Payment, Shipping, Invoice và Tax sau khi Official Order đã được tạo ở P3.2D.

Không được tạo Verified Revenue.
Không được tạo ORDER_VERIFIED nếu checkpoint đó thuộc P3.2F.
Không được tạo Commission final.
Không được tạo Verified ROAS.
Không được tạo Payout.
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

## PHASE-03/05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## CORE RULES

Payment selected không đồng nghĩa Paid.

Payment Core confirmation mới là paid status.

COD là phương thức nhận hàng rồi thanh toán, không tự paid.

Không tự thêm phí COD nếu policy chưa khóa.

Bank transfer selected không paid.

Shipping fee phải do runtime/policy tính, không hardcode.

VAT/tax phải do runtime/policy tính, không hardcode.

Paid không đồng nghĩa Verified Revenue.

Delivered không đồng nghĩa Verified Revenue trong P3.2E.

Invoice requested/issued không đồng nghĩa Verified Revenue.

No ORDER_VERIFIED thì không final commission, không verified ROAS, không payout.

Gateway vẫn bị chặn.

## SCOPE IN

Triển khai/chỉnh:

PaymentSelection.

PaymentMethod.

PaymentState.

PaymentConfirmation.

PaymentSelectionService.

PaymentConfirmationService.

COD handling.

Bank transfer handling.

ShippingQuote.

ShippingRequest nếu duyệt.

ShippingState.

ShippingFeeResolver.

InvoiceRequest.

InvoiceState.

TaxQuote/TaxSnapshot.

TaxResolver.

API/DTO nếu P3.1 duyệt.

Migration/seed nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2E.

## SCOPE OUT

Không làm:

Verified Revenue.

ORDER_VERIFIED nếu thuộc P3.2F.

Commission final.

Verified ROAS.

Payout.

KPI final.

MISA production sync ngoài boundary.

## IVR.

Gateway.

## ALLOWED FILE CHANGE BOUNDARY

Chỉ sửa file thuộc boundary đã được P3.1/P3.2D duyệt.

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## EXECUTION STEPS

Xác nhận mode LIMITED IMPLEMENTATION - PAYMENT / SHIPPING / INVOICE / TAX ONLY.

Đọc P3.2D Done Report.

Xác định allowed file boundary.

Inspect code hiện tại về payment/shipping/invoice/tax.

Implement Payment Selection.

Implement Payment Confirmation.

Implement Shipping Quote/Request.

Implement Invoice Request.

Implement Tax Quote/Snapshot.

Implement API/DTO nếu đã duyệt.

Implement migration/seed nếu đã duyệt.

Add/update test.

Run backend build/test.

Run frontend build nếu có ảnh hưởng UI.

Run smoke P3-SMK-2E.

Produce report 14 mục.

Handoff sang P3.2F nếu đạt DONE GATE.

## TEST / SMOKE REQUIRED

Bắt buộc kiểm tra:

COD selected is not Paid.

Bank transfer selected is not Paid.

Payment confirmed by valid source.

Payment selected does not create revenue.

Paid does not create Verified Revenue.

COD fee not auto-added.

Shipping fee uses runtime policy.

Shipping request does not set paid.

Delivered does not create verified revenue.

Tax uses runtime policy.

Invoice request does not create revenue.

No commission / ROAS / payout.

Public response safe.

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

Payment selected không phải Paid.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới paid.

COD không tự thêm phí nếu policy chưa khóa.

Shipping fee runtime/policy, không hardcode.

VAT/tax runtime/policy, không hardcode.

Paid không tạo Verified Revenue.

Delivered không tạo Verified Revenue.

Invoice không tạo Verified Revenue.

Không commission/ROAS/payout.

Test/smoke pass hoặc điểm chặn báo rõ.

Backend build pass hoặc điểm chặn báo rõ.

Evidence submitted.

Report đủ 14 mục.

Gateway vẫn bị chặn.

Không gọi Release Readiness/Production Readiness.

FAIL GATE

Fail nếu:

Sửa ngoài boundary.

Payment selected set Paid.

COD selected set Paid.

Bank transfer selected set Paid.

Paid không qua Payment Core confirmation.

COD fee hardcode.

Shipping fee hardcode.

VAT/tax hardcode.

Paid tạo Verified Revenue.

Delivered tạo Verified Revenue.

Invoice tạo Verified Revenue.

Tạo commission/ROAS/payout.

Mở Gateway.

Không có report 14 mục.

Gọi Production Readiness/Go-live Decision.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_P3_2F_VERIFIED_REVENUE_COMMISSION_ROAS_HANDOFF.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_P3_2D_OFFICIAL_ORDER_DONE_GATE.

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL.

## BLOCKED_TEST_OR_BUILD_FAILED.

## BLOCKED_PAYMENT_SHIPPING_INVOICE_TAX_P0_RULE_FAILED.

NEXT FILE: PHASE-03-COMMERCE-RUNTIME/07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md

ĐIỀU KIỆN CHUYỂN TIẾP: Chỉ chuyển sang P3.2F sau khi Payment / Shipping / Invoice / Tax đạt DONE GATE, payment selected tách rõ paid, COD/chuyển khoản không tự paid, Payment Core confirmation mới set paid, shipping fee/VAT theo runtime policy, paid/delivered/invoice không tự tạo Verified Revenue, không tạo commission/ROAS/payout và owner đồng ý handoff.

PHASE-03-COMMERCE-RUNTIME/07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md

## PROMPT-P3.2F - VERIFIED REVENUE / COMMISSION / ROAS HANDOFF LIMITED IMPLEMENTATION

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt triển khai giới hạn cho nhóm Verified Revenue / Commission / ROAS Handoff trong PHASE-03 - Commerce Runtime.

Mục tiêu là giao dev/Codex/Copilot triển khai hoặc chỉnh sửa đúng phạm vi sau khi hệ thống đã có:

Sellable Gate Runtime.

QuoteSnapshot.

Cart.

Order Draft.

Customer Confirmation.

Official Order.

order_code.

Order State Machine.

Payment Selection.

Payment Core Confirmation.

Shipping.

Invoice / Tax.

P3.2F chỉ xử lý:

ORDER_VERIFIED hoặc equivalent verified checkpoint.

Verified Revenue.

Verified Revenue State.

Revenue Reversal nếu có return/refund/cancel sau xác minh.

Commission eligibility handoff.

Commission final eligibility boundary.

ROAS verified handoff.

Payout eligibility boundary.

Ads/Finance/Reward handoff.

Evidence/audit/idempotency cho verified revenue.

P3.2F không mở Gateway.

P3.2F không gọi Release Readiness.

P3.2F không gọi Production Readiness.

P3.2F không tự quyết toán payout thật nếu Finance/Reward chưa duyệt.

P3.2F không bỏ qua owner review.

## 3. MODE

## MODE: LIMITED IMPLEMENTATION - VERIFIED REVENUE / COMMISSION / ROAS HANDOFF ONLY

Trong mode này, dev/Codex/Copilot chỉ được phép sửa code trong phạm vi Verified Revenue / Commission / ROAS Handoff đã được P3.1 và P3.2E handoff cho phép.

Được phép:

Thêm/sửa RevenueVerificationCheckpoint.

Thêm/sửa OrderVerificationService nếu P3.1/P3.2E duyệt.

Thêm/sửa VerifiedRevenueRecord.

Thêm/sửa VerifiedRevenueService.

Thêm/sửa RevenueState.

Thêm/sửa RevenueReversalRecord nếu boundary cho phép.

Thêm/sửa CommissionEligibilitySnapshot.

Thêm/sửa CommissionHandoffService.

Thêm/sửa ROASHandoffRecord.

Thêm/sửa ROASHandoffService.

Thêm/sửa PayoutEligibilityRecord ở mức eligibility/handoff nếu được duyệt.

Thêm/sửa API/DTO cho order verification / verified revenue / handoff nếu P3.1 duyệt.

Thêm migration/seed/config nếu P3.1 duyệt.

Thêm test/smoke cho P3.2F.

Chạy backend build/test.

Chạy frontend build nếu có thay contract/UI.

Trả report 14 mục.

Không được:

Mở Global Gate.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Tự quyết toán payout thật nếu Finance/Reward chưa duyệt.

Tự sync kế toán production nếu integration boundary chưa duyệt.

Tự coi paid là verified revenue.

Tự coi delivered là verified revenue nếu chưa qua checkpoint.

Tự coi invoice issued là verified revenue.

Cho Ads dùng unpaid/unverified/order draft/cart value làm verified ROAS.

Cho commission final khi chưa có verified revenue.

Cho payout khi chưa có verified revenue.

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

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM, Gateway, IVR.

Warehouse Receipt không tự set Sellable.

Trace/Recall/Sale Lock là nguồn chặn bán và chặn downstream nếu có rủi ro.

## 4.5. PHASE 3 - P3.2A

Sellable Gate là runtime gate riêng.

Recall/Sale Lock chặn sellable.

Runtime unavailable phải fail-safe.

## 4.6. PHASE 3 - P3.2B

QuoteSnapshot là nguồn sự thật cho giá cuối.

Không có QuoteSnapshot thì không có final price.

QuoteSnapshot immutable.

Program/member/diamond/shipping/VAT phải từ runtime/policy.

## 4.7. PHASE 3 - P3.2C

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới là điều kiện tạo Official Order.

## 4.8. PHASE 3 - P3.2D

Official Order chỉ tạo sau Customer Confirmation accepted.

Official Order phải có order_code.

Official Order created không đồng nghĩa Paid.

Official Order created không đồng nghĩa Verified Revenue.

## 4.9. PHASE 3 - P3.2E

Payment selected không đồng nghĩa Paid.

Payment Core confirmation mới là paid status.

COD selected không paid.

Bank transfer selected không paid.

Paid không đồng nghĩa Verified Revenue.

Delivered không đồng nghĩa Verified Revenue nếu chưa qua verified checkpoint.

Invoice requested/issued không đồng nghĩa Verified Revenue.

Paid/delivered/invoice không tạo commission final/verified ROAS/payout.

## 4.10. PHASE 3 - P3.2F

Verified Revenue chỉ có sau ORDER_VERIFIED hoặc equivalent verified checkpoint đã được khóa.

No ORDER_VERIFIED thì không final commission.

No ORDER_VERIFIED thì không verified ROAS.

No ORDER_VERIFIED thì không payout.

Ads chỉ consume verified revenue.

Finance/Reward chỉ final commission/payout sau verified revenue và owner policy.

## 5. ENTRY CONDITION

Chỉ được thực hiện prompt này nếu đã hoàn tất:

## 00-P3-ANALYSIS-ONLY.md.

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md.

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md.

## 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md.

## 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md.

## 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md.

## 06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md.

Official Order đạt DONE GATE.

Payment selected tách Paid rõ ràng.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới set Paid.

Paid chưa tạo Verified Revenue.

Delivered chưa tạo Verified Revenue.

Invoice request/issued chưa tạo Verified Revenue.

Chưa có commission final / verified ROAS / payout trong P3.2E.

Owner đồng ý handoff sang P3.2F.

Allowed file change boundary cho P3.2F đã rõ.

Nếu thiếu điều kiện, dev/Codex/Copilot phải dừng và trả trạng thái:

## BLOCKED_NEED_P3_2E_PAYMENT_SHIPPING_INVOICE_TAX_DONE_GATE

## 6. OBJECTIVE

Mục tiêu của P3.2F là triển khai checkpoint xác minh doanh thu và handoff doanh thu đã xác minh cho các downstream system.

P3.2F phải đảm bảo:

Chỉ order đủ điều kiện mới được ORDER_VERIFIED.

Chỉ sau ORDER_VERIFIED hoặc checkpoint tương đương mới tạo Verified Revenue.

Verified Revenue có audit/evidence đầy đủ.

Commission chỉ được chuyển từ WAITING/candidate sang final eligibility sau Verified Revenue.

ROAS chỉ được ghi nhận verified khi dùng Verified Revenue.

Payout chỉ được đánh dấu eligibility/handoff sau Verified Revenue và policy owner.

Refund/return/cancel sau verified phải có Revenue Reversal path nếu boundary duyệt.

Ads/Finance/Reward không được consume dữ liệu chưa verified như doanh thu thật.

Gateway vẫn bị chặn, không gọi production readiness.

## 7. CORE RULES

## 7.1. Paid không đồng nghĩa Verified Revenue

Payment confirmed paid chỉ chứng minh tiền đã được xác nhận theo Payment Core.

Paid chưa tự động là:

## ORDER_VERIFIED.

Verified Revenue.

Commission final.

Verified ROAS.

Payout ready.

## 7.2. Delivered không đồng nghĩa Verified Revenue nếu chưa qua checkpoint

Delivered có thể là một điều kiện đầu vào cho ORDER_VERIFIED, nhưng Delivered không tự tạo Verified Revenue nếu policy yêu cầu thêm bước xác minh.

## 7.3. Invoice issued không đồng nghĩa Verified Revenue

Invoice/tax là nghiệp vụ chứng từ.

Invoice requested hoặc issued không tự động là:

Paid.

## ORDER_VERIFIED.

Verified Revenue.

Commission final.

Verified ROAS.

Payout.

## 7.4. ORDER_VERIFIED là checkpoint khóa

ORDER_VERIFIED hoặc equivalent verified checkpoint phải xác nhận tối thiểu theo policy:

Official Order tồn tại.

order_code hợp lệ.

Payment state hợp lệ theo phương thức.

Delivery/fulfillment state hợp lệ nếu policy yêu cầu.

Không bị cancelled/refunded/returned trước checkpoint.

Không có recall/sale lock hậu kiểm ảnh hưởng doanh thu nếu policy quy định.

Không duplicate verified revenue.

Audit/evidence đầy đủ.

## 7.5. No ORDER_VERIFIED thì không final commission

Commission có thể là candidate/WAITING sau order/paid/referral, nhưng không được final nếu chưa có verified revenue.

## 7.6. No ORDER_VERIFIED thì không verified ROAS

Ads/ROAS không được dùng:

Cart value.

Quote value.

Order Draft value.

Official Order unpaid value.

Paid but unverified value.

Delivered but unverified value.

để tính verified ROAS.

## 7.7. No ORDER_VERIFIED thì không payout

Payout chỉ được eligibility/handoff sau verified revenue và policy owner.

Không được payout dựa trên:

Referral bind đơn thuần.

Order created.

Payment selected.

Paid unverified.

Delivered unverified.

Invoice issued.

## 7.8. Diamond referral chỉ được final sau Verified Revenue

Diamond referral eligibility từ QuoteSnapshot/Order chỉ là candidate.

Final commission phải cần:

Valid referral bind.

Referral owner eligible.

Buyer not self-purchase.

Official Order.

Payment/fulfillment condition theo policy.

## ORDER_VERIFIED.

Verified Revenue.

No refund/return/cancel block.

Finance/Reward policy approval nếu áp dụng.

## 7.9. Revenue reversal phải có đường xử lý

Nếu sau Verified Revenue có:

Refund.

Return.

Cancel.

Chargeback.

Fraud/invalid order.

Recall-related return.

Manual finance reversal.

thì phải có path reversal nếu P3.1/P3.2F boundary duyệt.

Không được để commission/ROAS/payout vẫn giữ trạng thái final nếu revenue đã reversal mà policy yêu cầu thu hồi/điều chỉnh.

## 8. SCOPE IN

P3.2F bao gồm:

RevenueVerificationCheckpoint.

OrderVerificationService.

VerifiedRevenueRecord.

VerifiedRevenueService.

RevenueState.

RevenueReversalRecord nếu duyệt.

CommissionEligibilitySnapshot.

CommissionHandoffService.

ROASHandoffRecord.

ROASHandoffService.

PayoutEligibilityRecord nếu duyệt ở mức eligibility/handoff.

Ads verified revenue handoff.

Finance/Reward handoff.

Verified revenue API/DTO nếu P3.1 duyệt.

Migration/seed/config nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2F.

Handoff sang P3.2G Smoke/Evidence Report.

## 9. SCOPE OUT

P3.2F không bao gồm:

Không mở Global Gate.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Không thực hiện payout thật nếu Finance/Reward chưa duyệt.

Không chuyển tiền hoa hồng thật.

Không quyết toán thuế thật.

Không sync production MISA ngoài integration boundary được duyệt.

Không triển khai Ads optimization engine.

Không triển khai full attribution model nếu chưa duyệt.

Không triển khai IVR.

Không bỏ qua owner review.

Không thay đổi business commission policy nếu chưa có source-of-truth.

Không dùng revenue chưa verified cho dashboard production.

## 10. ALLOWED FILE CHANGE BOUNDARY

Dev/Codex/Copilot chỉ được sửa file thuộc boundary đã được P3.1/P3.2E handoff cho phép.

## 10.1. Nhóm file được phép sửa nếu đã duyệt

Nhóm file

Được sửa

Ghi chú

Revenue verification domain

Có

ORDER_VERIFIED/checkpoint

Verified Revenue domain

Có

VerifiedRevenueRecord/RevenueState

Verified Revenue service

Có

Create/check/reversal nếu duyệt

Commission eligibility domain

Có

Snapshot/handoff, không payout thật

Commission handoff service

Có

Candidate -> eligible/final-ready theo policy

ROAS handoff domain

Có

Verified revenue handoff

ROAS handoff service

Có

Ads consume verified revenue

Payout eligibility domain

Có điều kiện

Chỉ eligibility/handoff nếu duyệt

API/DTO Verified Revenue

Có điều kiện

Chỉ nếu P3.1 duyệt

Migration

Có điều kiện

Chỉ VerifiedRevenue/Commission/ROAS handoff

Seed/config dev-test

Có điều kiện

Không auto PASS global

Backend tests

Có

Chỉ P3.2F

Frontend/Admin UI

Có điều kiện

Chỉ nếu P3.1 duyệt

Event/outbox

Có điều kiện

Chỉ verified revenue/handoff events nếu duyệt

Markdown/handoff

Có điều kiện

Chỉ cập nhật P3.2F report

## 10.2. Nhóm file không được sửa

Nhóm file

Không được sửa

Lý do

Gateway production config

Không

IVR production flow

Không

Reserved/future

Ads optimization engine

Không

Chỉ ROAS handoff

Finance payout execution

Không

Ngoài P3.2F nếu chưa duyệt

MISA production sync

Không mặc định

Chỉ integration boundary riêng

Tax filing/settlement

Không

Không thuộc P3.2F

Product/SKU/Recipe activation

Không

Thuộc P1

Operational inventory truth

Không

Thuộc P2

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## 11. REQUIRED INPUT

## 11.1. Từ P3.2D

OfficialOrder.

order_id.

order_code.

order_state.

customer/session context.

QuoteSnapshot reference.

final_payable_amount_snapshot.

order lines.

audit/correlation.

## 11.2. Từ P3.2E

PaymentSelection.

PaymentConfirmation.

payment_state.

paid_status.

confirmed_amount.

amount_match_status.

shipping_state.

delivery state if available.

invoice_state if available.

tax snapshot if available.

payment/shipping/invoice/tax audit evidence.

## 11.3. Từ QuoteSnapshot / Referral

QuoteSnapshot.

Program snapshot.

Member benefit snapshot.

Diamond referral snapshot.

Referral bind status.

Referral owner.

Self-purchase exclusion status.

Policy version.

## 11.4. Từ policy/config

ORDER_VERIFIED policy.

Verified Revenue policy.

Commission eligibility policy.

ROAS handoff policy.

Payout eligibility policy.

Refund/return/reversal policy.

Ads attribution mapping policy.

Finance/Reward handoff policy.

Evidence requirement.

Owner review requirement.

## 12. TARGET BEHAVIOR

## 12.1. Order verification success

Order được chuyển sang ORDER_VERIFIED khi:

Official Order tồn tại.

order_code hợp lệ.

Payment condition đạt theo policy.

Shipping/delivery condition đạt theo policy nếu yêu cầu.

Invoice/tax condition đạt theo policy nếu yêu cầu.

Không cancelled.

Không refunded.

Không returned.

Không active recall/sale lock block theo policy.

Không duplicate ORDER_VERIFIED.

Actor/source có permission.

Audit/evidence đầy đủ.

## 12.2. Verified Revenue created

Verified Revenue được tạo khi:

Order đã ORDER_VERIFIED hoặc equivalent checkpoint.

Chưa có VerifiedRevenueRecord active cho order đó.

Amount lấy từ Official Order/QuoteSnapshot/Payment confirmed theo policy.

Currency rõ.

Attribution refs nếu có được snapshot.

Audit/correlation/idempotency đầy đủ.

## 12.3. Commission handoff

Commission chỉ được chuyển sang final eligibility khi:

Verified Revenue tồn tại.

Referral/member/affiliate condition hợp lệ.

No self-purchase nếu Diamond referral.

No refund/return/cancel block.

Commission policy available.

Finance/Reward owner policy cho phép.

P3.2F có thể tạo:

commission_candidate.

commission_eligible_after_verified_revenue.

commission_handoff_ready.

Không được thực hiện payout thật nếu chưa thuộc boundary.

## 12.4. ROAS handoff

ROAS verified handoff chỉ được tạo khi:

Verified Revenue tồn tại.

Ads attribution hợp lệ nếu có.

Campaign/adset/ad refs hợp lệ nếu có.

Revenue not reversed.

Handoff record không trùng.

Ads không được dùng order chưa verified.

## 12.5. Payout eligibility

Payout eligibility chỉ được đánh dấu khi:

Verified Revenue tồn tại.

Commission eligibility pass.

Refund/return window policy nếu có đã xử lý hoặc được đánh dấu WAITING.

Finance/Reward policy cho phép.

Payout actual execution không thuộc P3.2F nếu chưa được owner duyệt riêng.

## 12.6. Revenue reversal

Nếu revenue đã verified nhưng sau đó xảy ra refund/return/cancel/chargeback:

Tạo RevenueReversalRecord nếu boundary duyệt.

Cập nhật revenue state sang REVERSED hoặc PARTIALLY_REVERSED theo policy.

Tạo handoff adjustment cho Commission/ROAS/Payout.

Audit đầy đủ.

Không xóa VerifiedRevenueRecord gốc.

## 13. DOMAIN CONTRACT

## 13.1. RevenueVerificationCheckpoint

Trường đề xuất:

verification_checkpoint_id.

order_id.

order_code.

checkpoint_type.

checkpoint_status.

verified_at.

verified_by_actor_id nullable.

verification_source.

payment_condition_status.

shipping_condition_status.

invoice_tax_condition_status nullable.

recall_sale_lock_condition_status.

evidence_ref nullable.

policy_version.

idempotency_key.

correlation_id.

audit_ref.

CheckpointType

## ORDER_VERIFIED.

## PAYMENT_AND_DELIVERY_VERIFIED.

## FINANCE_VERIFIED.

## OWNER_APPROVED_VERIFIED.

## FUTURE_EQUIVALENT_CHECKPOINT.

CheckpointStatus

## WAITING.

## VERIFIED.

## REJECTED.

bị chặn.

## REVERSED.

## 13.2. VerifiedRevenueRecord

Trường đề xuất:

verified_revenue_id.

order_id.

order_code.

quote_snapshot_id.

customer_id nullable.

session_id nullable.

channel.

verified_revenue_amount.

currency.

payment_confirmation_id nullable.

verification_checkpoint_id.

revenue_state.

verified_at.

policy_version.

attribution_ref nullable.

commission_handoff_status.

roas_handoff_status.

payout_eligibility_status nullable.

idempotency_key.

correlation_id.

audit_ref.

RevenueState

## NOT_REVENUE.

## UNVERIFIED_ORDER_VALUE.

## PAID_UNVERIFIED.

## DELIVERY_UNVERIFIED.

## ORDER_VERIFIED.

## VERIFIED_REVENUE_CREATED.

## PARTIALLY_REVERSED.

## REVERSED.

## CANCELLED_INVALIDATED.

## 13.3. CommissionEligibilitySnapshot

Trường đề xuất:

commission_eligibility_id.

verified_revenue_id.

order_id.

order_code.

referral_link_id nullable.

referral_owner_id nullable.

referral_owner_tier nullable.

buyer_customer_id nullable.

self_purchase_excluded.

commission_program_type.

commission_rate nullable.

commission_amount_candidate.

commission_status.

policy_version.

created_at.

correlation_id.

audit_ref.

CommissionStatus

## NOT_ELIGIBLE.

## CANDIDATE_WAITING_REVENUE.

## ELIGIBLE_AFTER_VERIFIED_REVENUE.

## BLOCKED_SELF_PURCHASE.

## BLOCKED_REFERRAL_INVALID.

## BLOCKED_REVENUE_NOT_VERIFIED.

## BLOCKED_REVENUE_REVERSED.

## HANDOFF_READY_FOR_FINANCE_REVIEW.

FINAL_APPROVED_BY_FINANCE nếu boundary duyệt.

PAID_OUT nếu payout module sau này duyệt.

P3.2F mặc định không thực hiện payout thật.

## 13.4. ROASHandoffRecord

Trường đề xuất:

roas_handoff_id.

verified_revenue_id.

order_id.

order_code.

attribution_source.

campaign_id nullable.

adset_id nullable.

ad_id nullable.

channel.

verified_revenue_amount.

currency.

roas_handoff_status.

handed_off_at.

policy_version.

correlation_id.

audit_ref.

ROASHandoffStatus

## NOT_READY.

## READY_AFTER_VERIFIED_REVENUE.

## HANDED_OFF.

## BLOCKED_NO_ATTRIBUTION.

## BLOCKED_REVENUE_NOT_VERIFIED.

## BLOCKED_REVENUE_REVERSED.

## FAILED_RETRYABLE.

## FAILED_DEAD_LETTER.

## 13.5. PayoutEligibilityRecord

Trường đề xuất:

payout_eligibility_id.

commission_eligibility_id.

verified_revenue_id.

order_id.

payout_status.

eligibility_reason.

blocked_reason nullable.

policy_version.

created_at.

correlation_id.

audit_ref.

PayoutStatus

## NOT_ELIGIBLE.

## WAITING_VERIFIED_REVENUE.

## ELIGIBLE_FOR_FINANCE_REVIEW.

## BLOCKED_REVENUE_REVERSED.

## BLOCKED_POLICY.

APPROVED_FOR_PAYOUT nếu Finance boundary duyệt.

PAID_OUT nếu payout module sau này duyệt.

## 13.6. RevenueReversalRecord

Trường đề xuất:

revenue_reversal_id.

verified_revenue_id.

order_id.

order_code.

reversal_type.

reversal_amount.

reversal_reason.

reversal_source.

occurred_at.

actor_context nullable.

evidence_ref nullable.

policy_version.

correlation_id.

audit_ref.

ReversalType

## REFUND.

## RETURN.

## CANCEL_AFTER_VERIFIED.

## CHARGEBACK.

## FRAUD_INVALIDATION.

## RECALL_RETURN.

## MANUAL_FINANCE_REVERSAL.

## 14. SERVICE CONTRACT

## 14.1. OrderVerificationService

VerifyOrder input

order_id.

order_code.

verification_source.

actor_context nullable.

evidence_ref nullable.

idempotency_key.

correlation_id.

VerifyOrder output

verification_checkpoint_id.

checkpoint_status.

order_state.

verification_result.

block_reasons nullable.

audit_ref.

handoff_ready_for_verified_revenue.

Rules

Official Order required.

Payment condition must pass if policy requires.

Shipping/delivery condition must pass if policy requires.

Invoice/tax condition must pass if policy requires.

No cancelled/refunded/returned block.

No active recall/sale lock block if policy applies.

Idempotency required.

Audit required.

Does not create commission final directly.

Does not payout.

## 14.2. VerifiedRevenueService

CreateVerifiedRevenue input

order_id.

verification_checkpoint_id.

payment_confirmation_id nullable.

attribution_context nullable.

idempotency_key.

correlation_id.

CreateVerifiedRevenue output

verified_revenue_id.

revenue_state.

verified_revenue_amount.

currency.

commission_handoff_status.

roas_handoff_status.

audit_ref.

Rules

Requires verified checkpoint.

No duplicate verified revenue per order unless policy supports partial/multi.

Amount must come from official order/payment policy.

Must snapshot attribution refs if any.

Must be idempotent.

Must audit.

Must not open Gateway.

Must not bypass Finance/Reward owner.

## 14.3. CommissionHandoffService

EvaluateCommissionEligibility input

verified_revenue_id.

order_id.

referral_context nullable.

quote_snapshot_ref.

policy_version.

correlation_id.

Output

commission_eligibility_id.

commission_status.

commission_amount_candidate.

blocked_reason nullable.

handoff_ready_for_finance_review.

audit_ref.

Rules

Requires verified revenue.

No verified revenue -> block.

Self-purchase -> block.

Invalid referral bind -> block.

Reversed revenue -> block or adjustment required.

Commission rate/amount must come from runtime policy.

No hardcode in AI/Gateway.

No payout execution unless boundary allows.

## 14.4. ROASHandoffService

Create ROASHandoff input

verified_revenue_id.

attribution_context.

campaign/ad refs nullable.

idempotency_key.

correlation_id.

Output

roas_handoff_id.

roas_handoff_status.

verified_revenue_amount.

attribution_refs.

audit_ref.

Rules

Requires verified revenue.

No verified revenue -> block.

Unverified paid order -> block.

Missing attribution can block or mark no-attribution according to policy.

Revenue reversal must trigger ROAS adjustment if boundary supports.

Ads must consume this handoff, not raw order value.

## 14.5. RevenueReversalService

ReverseRevenue input

verified_revenue_id.

reversal_type.

reversal_amount.

reversal_reason.

actor_context or system source.

evidence_ref nullable.

idempotency_key.

correlation_id.

Output

revenue_reversal_id.

new_revenue_state.

commission_adjustment_required.

roas_adjustment_required.

payout_block_required.

audit_ref.

Rules

Does not delete original verified revenue.

Append reversal record.

Update derived handoff status according to policy.

Block payout if required.

Audit/evidence required.

## 15. STATE MACHINE

## 15.1. Order verification state

Allowed:

PAID -> ORDER_VERIFICATION_WAITING nếu policy dùng state trung gian.

DELIVERED -> ORDER_VERIFICATION_WAITING nếu delivery required.

## ORDER_VERIFICATION_WAITING -> ORDER_VERIFIED.

## ORDER_VERIFICATION_WAITING -> ORDER_VERIFICATION_REJECTED.

## ORDER_VERIFIED -> VERIFIED_REVENUE_CREATED.

ORDER_VERIFIED -> VERIFICATION_REVERSED nếu reversal.

Forbidden:

OFFICIAL_CREATED -> ORDER_VERIFIED nếu thiếu policy condition.

## PAYMENT_SELECTED -> ORDER_VERIFIED.

PAID -> VERIFIED_REVENUE_CREATED nếu chưa qua ORDER_VERIFIED.

DELIVERED -> VERIFIED_REVENUE_CREATED nếu chưa qua ORDER_VERIFIED.

INVOICE_ISSUED -> VERIFIED_REVENUE_CREATED nếu chưa qua ORDER_VERIFIED.

## 15.2. Revenue state

Allowed:

## NOT_REVENUE -> UNVERIFIED_ORDER_VALUE.

## UNVERIFIED_ORDER_VALUE -> PAID_UNVERIFIED.

## PAID_UNVERIFIED -> ORDER_VERIFIED.

DELIVERY_UNVERIFIED -> ORDER_VERIFIED nếu policy yêu cầu delivery.

## ORDER_VERIFIED -> VERIFIED_REVENUE_CREATED.

## VERIFIED_REVENUE_CREATED -> PARTIALLY_REVERSED.

## VERIFIED_REVENUE_CREATED -> REVERSED.

PARTIALLY_REVERSED -> REVERSED nếu full reversal.

Forbidden:

## UNVERIFIED_ORDER_VALUE -> VERIFIED_REVENUE_CREATED.

PAID_UNVERIFIED -> VERIFIED_REVENUE_CREATED without ORDER_VERIFIED.

DELIVERY_UNVERIFIED -> VERIFIED_REVENUE_CREATED without ORDER_VERIFIED.

INVOICE_ISSUED -> VERIFIED_REVENUE_CREATED without ORDER_VERIFIED.

REVERSED -> commission final without re-approval.

## 15.3. Commission state

Allowed:

## NOT_ELIGIBLE -> CANDIDATE_WAITING_REVENUE.

## CANDIDATE_WAITING_REVENUE -> ELIGIBLE_AFTER_VERIFIED_REVENUE.

## ELIGIBLE_AFTER_VERIFIED_REVENUE -> HANDOFF_READY_FOR_FINANCE_REVIEW.

HANDOFF_READY_FOR_FINANCE_REVIEW -> FINAL_APPROVED_BY_FINANCE nếu boundary duyệt.

Any eligible state -> BLOCKED_REVENUE_REVERSED nếu revenue reversed.

Any candidate state -> BLOCKED_SELF_PURCHASE nếu self-purchase.

Forbidden:

CANDIDATE_WAITING_REVENUE -> FINAL_APPROVED_BY_FINANCE without verified revenue.

## NOT_ELIGIBLE -> PAYOUT.

## BLOCKED_SELF_PURCHASE -> PAYOUT.

## BLOCKED_REVENUE_REVERSED -> PAYOUT.

## 15.4. ROAS handoff state

Allowed:

## NOT_READY -> READY_AFTER_VERIFIED_REVENUE.

## READY_AFTER_VERIFIED_REVENUE -> HANDED_OFF.

READY_AFTER_VERIFIED_REVENUE -> BLOCKED_NO_ATTRIBUTION nếu policy yêu cầu.

HANDED_OFF -> ADJUSTMENT_REQUIRED nếu revenue reversed và boundary có adjustment.

FAILED_RETRYABLE -> HANDED_OFF after retry.

## FAILED_RETRYABLE -> FAILED_DEAD_LETTER.

Forbidden:

NOT_READY -> HANDED_OFF without verified revenue.

Paid unverified -> HANDED_OFF.

Draft/order value -> HANDED_OFF.

Reversed revenue -> new ROAS verified without adjustment.

## 16. API / DTO CONTRACT - IF APPROVED BY P3.1

## 16.1. Verify Order API

POST /api/commerce/orders/{order_id}/verify

Request:

verification_source.

evidence_ref nullable.

idempotency_key.

correlation_id.

Response:

order_id.

order_code.

verification_checkpoint_id.

checkpoint_status.

order_state.

handoff_ready_for_verified_revenue.

block_reasons nullable.

## 16.2. Create Verified Revenue API

POST /api/commerce/orders/{order_id}/verified-revenue

Request:

verification_checkpoint_id.

attribution_context nullable.

idempotency_key.

correlation_id.

Response:

verified_revenue_id.

order_id.

order_code.

verified_revenue_amount.

currency.

revenue_state.

commission_handoff_status.

roas_handoff_status.

no_payout_executed_yet = true.

## 16.3. Commission Eligibility API

POST /api/commerce/verified-revenue/{verified_revenue_id}/commission-eligibility

Request:

policy_context optional.

referral_context optional.

idempotency_key.

correlation_id.

Response:

commission_eligibility_id.

commission_status.

commission_amount_candidate.

handoff_ready_for_finance_review.

payout_executed = false.

## 16.4. ROAS Handoff API

POST /api/commerce/verified-revenue/{verified_revenue_id}/roas-handoff

Request:

attribution_context.

campaign_refs nullable.

idempotency_key.

correlation_id.

Response:

roas_handoff_id.

roas_handoff_status.

verified_revenue_amount.

attribution_refs.

handed_off_at nullable.

## 16.5. Revenue Reversal API

POST /api/commerce/verified-revenue/{verified_revenue_id}/reverse

Request:

reversal_type.

reversal_amount.

reversal_reason.

evidence_ref nullable.

idempotency_key.

correlation_id.

Response:

revenue_reversal_id.

new_revenue_state.

commission_adjustment_required.

roas_adjustment_required.

payout_block_required.

## 16.6. Public/internal response boundary

Public/channel response được phép có:

order_code.

public order state.

public payment/delivery status nếu policy cho phép.

public confirmation that order has been completed/verified nếu policy cho phép.

Không được lộ:

Commission amount nội bộ nếu không có quyền.

ROAS/campaign data.

Ads attribution internals.

Finance payout status nội bộ.

Internal audit note.

Internal tax/accounting mapping.

Refund/chargeback sensitive notes.

Private customer financial evidence.

## 17. DATABASE / MIGRATION RULE

Nếu P3.1 duyệt migration, bảng/entity P3.2F có thể gồm:

commerce_revenue_verification_checkpoint.

commerce_verified_revenue.

commerce_revenue_reversal.

commerce_commission_eligibility_snapshot.

commerce_roas_handoff.

commerce_payout_eligibility nếu duyệt.

commerce_verified_revenue_state_transition nếu chưa có state log chung.

commerce_verified_revenue_idempotency nếu chưa có idempotency chung.

Không tạo hoặc không sửa ngoài boundary:

Gateway production config.

Ads optimization engine.

Finance payout execution tables nếu chưa duyệt.

MISA production sync tables nếu không thuộc integration boundary.

IVR production tables.

## 17.1. Migration safety

Migration phải:

Không phá dữ liệu cũ.

Không tự tạo verified revenue cho order cũ nếu chưa verify.

Không tạo commission final hàng loạt.

Không tạo payout hàng loạt.

Có unique constraint ngăn duplicate verified revenue per order theo policy.

Có idempotency constraint.

Có index cho order_id, order_code, verified_revenue_id, revenue_state, handoff_status.

Có audit/correlation fields.

Có reversal path nếu duyệt.

Không mở Gateway.

## 18. SEED / CONFIG RULE

Seed/config dev-test được phép tạo:

ORDER_VERIFIED policy sample.

Verified Revenue policy sample.

Commission eligibility policy sample.

ROAS handoff policy sample.

Payout eligibility policy sample nếu duyệt.

Negative fixtures:

paid but unverified.

delivered but unverified.

invoice issued but unverified.

invalid referral.

self-purchase.

reversed revenue.

duplicate verified revenue.

Test attribution refs.

Seed/config không được:

Tự tạo Verified Revenue cho tất cả orders.

Tự tạo commission final.

Tự tạo payout.

Tự gọi verified ROAS khi chưa có Verified Revenue.

Tự mở Gateway.

Tự gọi Production Readiness.

Hardcode commission policy trong channel/content.

Hardcode ROAS revenue từ order value.

## 19. SECURITY / GOVERNANCE GUARDRAILS

## 19.1. RBAC

Bắt buộc backend permission cho:

Verify order.

Create verified revenue.

Reverse verified revenue.

View internal commission eligibility.

View ROAS handoff.

Approve finance commission if boundary allows.

View payout eligibility.

Export revenue evidence.

Manual override.

## 19.2. Actor Context

Bắt buộc cho:

Manual order verification.

Revenue reversal.

Finance review handoff.

Commission override nếu có.

Payout eligibility override nếu có.

ROAS correction nếu có.

## 19.3. Correlation ID

Mọi command phải có correlation_id:

Verify order.

Create verified revenue.

Create commission eligibility.

Create ROAS handoff.

Create payout eligibility.

Reverse revenue.

## 19.4. Audit append-only

Audit bắt buộc cho:

## ORDER_VERIFIED.

Verified Revenue created.

Commission eligibility created.

ROAS handoff created.

Payout eligibility created.

Revenue reversed.

Duplicate bị chặn.

Manual override.

## 19.5. Idempotency

Idempotency bắt buộc cho:

Verify order.

Create verified revenue.

Create commission eligibility.

Create ROAS handoff.

Create payout eligibility.

Reverse revenue.

## 19.6. Evidence Registry

Evidence submitted chưa phải accepted.

Không gọi Completion Decision nếu chưa owner review.

## 19.7. Runtime unavailable

Nếu runtime/policy unavailable:

Không ORDER_VERIFIED.

Không Verified Revenue.

Không commission final eligibility.

Không ROAS handoff.

Không payout eligibility.

Không tạo partial side effect.

## 19.8. Transaction safety

Các command side effect phải atomic trong phạm vi business transaction:

Validate order.

Validate payment/shipping/invoice/tax condition theo policy.

Create checkpoint.

Create verified revenue.

Create audit/state log.

Create outbox event nếu duyệt.

Commit together where required.

Không để trạng thái nửa vời:

Có ORDER_VERIFIED nhưng không có audit.

Có verified revenue nhưng không có checkpoint.

Có commission handoff nhưng không có verified revenue.

Có ROAS handoff nhưng revenue chưa verified.

## 20. EVENT / OUTBOX RULE - IF APPROVED

Nếu P3.1 duyệt event/outbox, P3.2F có thể phát các event:

## ORDER_VERIFIED.

## VERIFIED_REVENUE_CREATED.

## COMMISSION_ELIGIBILITY_UPDATED.

## COMMISSION_HANDOFF_READY.

## ROAS_VERIFIED_HANDOFF_READY.

PAYOUT_ELIGIBILITY_UPDATED nếu duyệt.

## REVENUE_REVERSED.

## ROAS_ADJUSTMENT_REQUIRED.

## COMMISSION_ADJUSTMENT_REQUIRED.

Rule:

ORDER_VERIFIED là checkpoint, không phải Gateway PASS.

VERIFIED_REVENUE_CREATED là input cho Ads/Finance/Reward.

COMMISSION_HANDOFF_READY không đồng nghĩa payout đã trả.

ROAS_VERIFIED_HANDOFF_READY không đồng nghĩa Ads optimization completed.

PAYOUT_ELIGIBILITY_UPDATED không đồng nghĩa payout paid.

REVENUE_REVERSED phải trigger adjustment theo policy.

Outbox event không được bypass integration boundary.

External systems không direct-write operational/commerce DB.

## 21. TEST REQUIREMENTS

## 21.1. Unit test - Order Verification

Official Order paid + delivery condition pass -> ORDER_VERIFIED.

Official Order unpaid -> reject verification.

Payment selected only -> reject verification.

Paid but delivery missing when policy requires delivery -> reject verification.

Invoice issued but unpaid -> reject verification.

Cancelled order -> reject verification.

Refunded order -> reject verification.

Recall/Sale Lock block -> reject verification nếu policy yêu cầu.

Duplicate verify same idempotency key -> same result.

Duplicate verify different key same order -> no duplicate checkpoint.

## 21.2. Unit test - Verified Revenue

ORDER_VERIFIED -> create Verified Revenue.

No ORDER_VERIFIED -> reject Verified Revenue.

Paid unverified -> reject Verified Revenue.

Delivered unverified -> reject Verified Revenue.

Invoice issued unverified -> reject Verified Revenue.

Duplicate verified revenue -> block or idempotent same result.

Verified revenue amount from official order/payment policy.

Verified revenue has audit/correlation.

## 21.3. Unit test - Commission

Verified Revenue + valid Diamond referral -> commission eligibility created.

No Verified Revenue -> commission bị chặn.

Self-purchase -> commission bị chặn.

Invalid referral bind -> commission bị chặn.

Reversed revenue -> commission bị chặn/adjustment required.

Commission amount comes from policy, not hardcode.

Commission eligibility does not execute payout.

Commission final approval requires Finance boundary if applicable.

## 21.4. Unit test - ROAS

Verified Revenue + attribution -> ROAS handoff ready.

No Verified Revenue -> ROAS bị chặn.

Paid unverified -> ROAS bị chặn.

Order created value -> ROAS bị chặn.

Cart/Quote/Draft value -> ROAS bị chặn.

Missing attribution -> bị chặn/no-attribution according to policy.

Reversed revenue -> ROAS adjustment required if supported.

ROAS handoff idempotent.

## 21.5. Unit test - Revenue Reversal

Verified Revenue reversed by refund -> revenue state reversed.

Partial refund -> partially reversed.

Reversal triggers commission adjustment.

Reversal triggers ROAS adjustment.

Reversal blocks payout.

Duplicate reversal idempotent.

Reversal has audit/evidence.

Original verified revenue not deleted.

## 21.6. Integration test nếu có API

POST verify order success.

POST verify order unpaid reject.

POST create verified revenue success after verification.

POST create verified revenue without verification reject.

POST commission eligibility success after verified revenue.

POST commission eligibility self-purchase reject.

POST ROAS handoff success after verified revenue.

POST ROAS handoff without verified revenue reject.

POST revenue reversal success.

Public response does not leak internal commission/ROAS/payout data.

## 22. SMOKE REQUIREMENTS

## 22.1. P3-SMK-2F-001 - Paid is not Verified Revenue

Case: Order đã payment confirmed paid.
Expected: Chưa có Verified Revenue nếu chưa ORDER_VERIFIED.

## 22.2. P3-SMK-2F-002 - Delivered is not Verified Revenue

Case: Order delivered.
Expected: Chưa có Verified Revenue nếu chưa ORDER_VERIFIED.

## 22.3. P3-SMK-2F-003 - Invoice issued is not Verified Revenue

Case: Invoice issued/requested.
Expected: Không tự tạo Verified Revenue.

## 22.4. P3-SMK-2F-004 - ORDER_VERIFIED creates checkpoint

Case: Order đủ điều kiện verify.
Expected: ORDER_VERIFIED checkpoint created with audit.

## 22.5. P3-SMK-2F-005 - Verified Revenue after ORDER_VERIFIED

Case: ORDER_VERIFIED exists.
Expected: VerifiedRevenueRecord created.

## 22.6. P3-SMK-2F-006 - No ORDER_VERIFIED blocks revenue

Case: Tạo Verified Revenue khi chưa ORDER_VERIFIED.
Expected: Reject.

## 22.7. P3-SMK-2F-007 - No Verified Revenue blocks commission final

Case: Referral valid nhưng order chưa verified revenue.
Expected: Commission final bị chặn.

## 22.8. P3-SMK-2F-008 - Valid referral after Verified Revenue

Case: Verified Revenue + valid Diamond referral.
Expected: Commission eligibility/handoff created, payout not executed.

## 22.9. P3-SMK-2F-009 - Self-purchase blocks commission

Case: Buyer is referral owner.
Expected: Commission bị chặn.

## 22.10. P3-SMK-2F-010 - No Verified Revenue blocks ROAS

Case: Ads attribution exists but revenue not verified.
Expected: ROAS verified handoff bị chặn.

## 22.11. P3-SMK-2F-011 - Verified Revenue creates ROAS handoff

Case: Verified Revenue + attribution.
Expected: ROAS handoff ready/created.

## 22.12. P3-SMK-2F-012 - Revenue reversal adjusts downstream

Case: Verified Revenue reversed.
Expected: Commission/ROAS/payout adjustment required.

## 22.13. P3-SMK-2F-013 - Duplicate verified revenue bị chặn

Case: Create verified revenue twice for same order.
Expected: No duplicate effect.

## 22.14. P3-SMK-2F-014 - Public response safe

Case: Public/channel reads revenue/order status.
Expected: Không lộ commission/ROAS/payout/internal finance data.

## 22.15. P3-SMK-2F-015 - Gateway remains bị chặn

## 23. EVIDENCE REQUIREMENTS

P3.2F cần gom evidence:

Git diff trong allowed boundary.

File list đã sửa.

Migration diff nếu có.

Seed/config diff nếu có.

Unit test result.

Integration test result nếu có API.

Smoke result P3-SMK-2F-001 -> P3-SMK-2F-015.

Backend build result.

Frontend build result nếu có ảnh hưởng UI.

Paid not verified revenue evidence.

Delivered not verified revenue evidence.

Invoice not verified revenue evidence.

ORDER_VERIFIED checkpoint evidence.

Verified Revenue created evidence.

No ORDER_VERIFIED blocks revenue evidence.

Commission bị chặn without verified revenue evidence.

Valid referral commission eligibility evidence.

Self-purchase bị chặn evidence.

ROAS bị chặn without verified revenue evidence.

ROAS handoff after verified revenue evidence.

Revenue reversal evidence nếu implemented.

Duplicate verified revenue bị chặn evidence.

Public/internal boundary evidence.

Audit evidence.

Idempotency evidence.

Handoff note sang P3.2G.

Lưu ý:

Evidence submitted chưa phải Evidence accepted.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

## 24. EXECUTION STEPS

Step 1 - Confirm Mode

Xác nhận:

## MODE: LIMITED IMPLEMENTATION - VERIFIED REVENUE / COMMISSION / ROAS HANDOFF ONLY

Step 2 - Read P3.2E Done Report

Xác nhận:

Payment selected không phải Paid.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới paid.

Paid chưa tạo Verified Revenue.

Delivered chưa tạo Verified Revenue.

Invoice chưa tạo Verified Revenue.

Không commission/ROAS/payout trong P3.2E.

Nếu chưa đạt, dừng.

Step 3 - Inspect Existing Revenue/Commission/ROAS Logic

Kiểm tra codebase hiện tại:

Có revenue module cũ không.

Có flow nào tạo revenue từ paid không.

Có flow nào tạo revenue từ delivered không.

Có flow nào tạo revenue từ invoice không.

Có commission module cũ không.

Có payout flow cũ không.

Có Ads/ROAS module cũ không.

Có dashboard nào dùng order value làm revenue không.

Có direct sync finance/ads không.

Có duplicate prevention không.

Step 4 - Implement Order Verification

Triển khai/chỉnh:

RevenueVerificationCheckpoint.

OrderVerificationService.

Verification policy validation.

ORDER_VERIFIED state/checkpoint.

Audit/idempotency.

Step 5 - Implement Verified Revenue

Triển khai/chỉnh:

VerifiedRevenueRecord.

VerifiedRevenueService.

RevenueState.

Duplicate verified revenue guard.

Amount source policy.

Audit/correlation.

Step 6 - Implement Commission Handoff

Triển khai/chỉnh:

CommissionEligibilitySnapshot.

CommissionHandoffService.

Valid referral guard.

Self-purchase guard.

Revenue verified guard.

Revenue reversal guard.

No payout execution unless boundary allows.

Step 7 - Implement ROAS Handoff

Triển khai/chỉnh:

ROASHandoffRecord.

ROASHandoffService.

Attribution guard.

Verified revenue guard.

Reversal adjustment guard nếu implemented.

Idempotency.

Step 8 - Implement Revenue Reversal nếu approved

Triển khai/chỉnh:

RevenueReversalRecord.

RevenueReversalService.

Commission adjustment required.

ROAS adjustment required.

Payout block required.

Audit/evidence.

Nếu P3.1 chưa duyệt reversal, chỉ ghi deferred risk.

Step 9 - Implement API/DTO nếu approved

Nếu P3.1 duyệt, triển khai:

Verify Order API.

Create Verified Revenue API.

Commission Eligibility API.

ROAS Handoff API.

Revenue Reversal API nếu approved.

Nếu chưa duyệt, không tự tạo API.

Step 10 - Implement Migration/Seed nếu approved

Chỉ trong phạm vi Verified Revenue / Commission / ROAS Handoff.

Không tạo Gateway production config.

Không tạo payout thật.

Step 11 - Add Tests

Thêm/cập nhật test theo mục Test Requirements.

Step 12 - Run Build/Test/Smoke

Chạy:

Backend build.

Backend test.

Integration test nếu có.

Frontend build nếu ảnh hưởng UI.

Smoke P3-SMK-2F.

Step 13 - Evidence Report

Gom evidence và trả report 14 mục.

Step 14 - Handoff

Kết luận có được chuyển sang P3.2G Smoke/Evidence Report hay không.

## 25. REQUIRED REPORT FORMAT - 14 MỤC

## 25.1. Tóm tắt

Ghi rõ:

Đã triển khai ORDER_VERIFIED/checkpoint phần nào.

Đã triển khai Verified Revenue phần nào.

Đã triển khai Commission handoff phần nào.

Đã triển khai ROAS handoff phần nào.

Test/smoke kết quả thế nào.

Có đủ điều kiện sang P3.2G không.

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

P3.2E Done Report.

Prompt P3.2F.

## 25.4. Evidence đã dùng

Liệt kê:

Code references.

DB/migration references.

Official Order evidence.

Payment evidence.

Shipping/invoice evidence.

Verified Revenue evidence.

Commission evidence.

ROAS evidence.

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

Unique verified revenue validation.

Không tạo payout thật.

Không mở Gateway.

## 25.12. Kết quả seed validation nếu áp dụng

Ghi:

Có seed không.

Seed có tự tạo verified revenue sai không.

Seed có tạo commission final sai không.

Seed có tạo payout sai không.

Seed có dùng order value làm ROAS sai không.

Seed validation result.

## 25.13. Rủi ro còn lại

Phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Owner decision needed.

Deferred to P3.2G.

Deferred to Finance/Reward.

Deferred to Ads/Measurement.

Technical debt.

## 25.14. Cập nhật handoff

Ghi rõ:

Có được sang P3.2G không.

Điều kiện còn thiếu nếu chưa.

File tiếp theo.

Handoff notes cho Smoke/Evidence Report.

Trạng thái Gateway vẫn bị chặn.

## 26. DONE GATE

P3.2F được DONE khi đủ:

Chỉ sửa file trong allowed boundary.

Paid không tự tạo Verified Revenue.

Delivered không tự tạo Verified Revenue.

Invoice issued/requested không tự tạo Verified Revenue.

ORDER_VERIFIED hoặc equivalent checkpoint hoạt động đúng policy.

Verified Revenue chỉ tạo sau ORDER_VERIFIED.

No ORDER_VERIFIED thì reject Verified Revenue.

Duplicate Verified Revenue bị chặn hoặc idempotent.

Verified Revenue có audit/correlation/evidence.

Commission không final nếu chưa Verified Revenue.

Valid referral sau Verified Revenue tạo commission eligibility/handoff đúng rule.

Self-purchase bị chặn.

Invalid referral bị chặn.

Revenue reversed thì commission adjustment/block nếu implemented.

ROAS không dùng unverified order/paid/delivered value.

ROAS handoff chỉ sau Verified Revenue.

Payout không executed trong P3.2F nếu chưa duyệt.

Public/internal response boundary đúng.

Audit/correlation đầy đủ.

Idempotency đầy đủ cho command side effect.

Test bắt buộc pass hoặc điểm chặn rõ.

Smoke P3-SMK-2F pass hoặc điểm chặn rõ.

Backend build pass hoặc điểm chặn rõ.

Frontend build pass nếu có ảnh hưởng UI.

Evidence submitted.

Report đủ 14 mục.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 27. FAIL GATE

P3.2F bị FAIL nếu:

Sửa file ngoài allowed boundary.

Paid tự tạo Verified Revenue.

Delivered tự tạo Verified Revenue.

Invoice issued/requested tự tạo Verified Revenue.

Tạo Verified Revenue khi chưa ORDER_VERIFIED.

Tạo duplicate Verified Revenue.

Commission final khi chưa Verified Revenue.

Self-purchase vẫn được commission.

Invalid referral vẫn được commission.

ROAS dùng Cart/Quote/Draft/Order unpaid/unverified value.

ROAS dùng Paid unverified value.

Payout executed khi chưa có Finance/Reward boundary.

Revenue reversal xóa record gốc thay vì append adjustment.

Public response lộ commission/ROAS/payout/internal finance data.

Không có test negative case.

Không có report 14 mục.

Mở Gateway.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

## 28. DOWNSTREAM HANDOFF

## 28.1. File tiếp theo

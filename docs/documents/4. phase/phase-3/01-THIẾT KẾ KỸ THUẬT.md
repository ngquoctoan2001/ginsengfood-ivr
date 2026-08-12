# P3.1 - TECHNICAL DESIGN ONLY

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- MASTER runtime resolution guard va traceability.
- TECH-04 Commerce Runtime technical boundary.
- TECH-11 Implementation Roadmap boundary.
- PACK-10 Evidence / Smoke / Completion Gateway.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/01-P3-1-TECHNICAL-DESIGN-ONLY.md

## 20.2. Prompt tiếp theo

## PROMPT-P3.1 - COMMERCE RUNTIME TECHNICAL DESIGN / WORKSTREAM / TASK / CONTRACT / EVIDENCE / SMOKE HANDOFF

## 20.3. Điều kiện chuyển phase

Được chuyển sang P3.1 nếu:

P3 Analysis report hoàn tất.

Không có hành vi sửa file trong Analysis Only.

điểm chặn đã được ghi rõ.

Owner đồng ý dùng report làm nền thiết kế.

Các domain P0 của Commerce Runtime đã được xác định:

Sellable Gate.

Price Program Runtime.

Member Benefit Runtime.

QuoteSnapshot.

Cart.

Order Draft.

Customer Confirmation.

Official Order.

Payment.

Shipping.

Tax/Invoice.

Verified Revenue.

Commission/ROAS handoff.

Không có hiểu sai rằng PHASE 3 Analysis Complete là Release Readiness.

## 20.4. Không được chuyển phase nếu

Chưa có report 14 mục.

Chưa map codebase hiện trạng.

Chưa xác định owner boundary.

Chưa xác định điểm chặn P0.

Chưa xác định evidence required.

Chưa xác định smoke proposed.

Có file bị sửa trong Analysis Only.

Có xung đột source-of-truth chưa báo owner.

## 21. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3 - COMMERCE RUNTIME / SELLABLE GATE / QUOTE SNAPSHOT / CART / ORDER / PAYMENT / SHIPPING / VERIFIED REVENUE ANALYSIS HANDOFF

## MODE: ANALYSIS ONLY - DO NOT MODIFY FILES

Bạn đang thực hiện PHASE-03 - COMMERCE RUNTIME cho dự án Ginsengfood.

Nhiệm vụ của bạn là phân tích codebase thật và lập report hiện trạng. Không được sửa file. Không được tạo migration. Không được sửa seed. Không được thêm test. Không được thêm API. Không được thêm feature.

## SOURCE-OF-TRUTH

Bạn phải đối chiếu theo các nguồn:

MASTER Governance.

PHASE-00 Foundation/RBAC/Audit/Evidence/Idempotency.

PHASE-01 Product/SKU/Recipe/Activation.

PHASE-02 Operational Core.

PHASE-03 Commerce Runtime requirement.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Documentation Complete chưa phải Production Readiness.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## PHASE 3 RULES

Sellable Gate là runtime gate riêng.

Không dùng Product Active / SKU Active / Recipe Active thay Sellable.

Sellable phụ thuộc tối thiểu:

Product/SKU active.

Listed price active.

Stock available.

Finished goods released.

Warehouse receipt confirmed.

No recall.

No sale lock.

No quality hold.

No channel suppression.

Runtime policy available.

QuoteSnapshot là nguồn sự thật cho giá cuối tại thời điểm báo giá.

Không có QuoteSnapshot thì không có final price.

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới cho phép tạo Official Order.

Không có order_code thì không được nói "đơn hàng đã ghi nhận chính thức".

Payment selected không đồng nghĩa Paid.

Payment Core confirmation mới là paid status.

Shipping fee/VAT/discount/member benefit phải được runtime tính.

COD chỉ là phương thức nhận hàng rồi thanh toán, không tự thêm phí COD nếu policy chưa khóa.

Verified Revenue chỉ có sau ORDER_VERIFIED hoặc equivalent verified checkpoint.

No ORDER_VERIFIED thì không final commission, không verified ROAS, không payout.

Commerce Runtime là owner của sellable_status, listed_price_status, program eligibility, member benefit eligibility, diamond referral eligibility, QuoteSnapshot, final quote price, official order state, verified revenue.

AI Advisor, Gateway, Ads, Live, CRM, IVR chỉ consume runtime, không tự tính giá/tồn/discount/sellable/revenue.

Recall/Sale Lock từ PHASE 2 phải chặn Commerce, AI, Gateway, Ads, Live, CRM, IVR.

## EXECUTION STEPS

Xác nhận mode Analysis Only.

Đọc source-of-truth.

Inspect backend structure.

Inspect database/migration.

Inspect API/service flow.

Inspect frontend/admin nếu có.

Inspect integration boundary.

Lập Current Implementation State Matrix.

Lập Gap Analysis Matrix.

Lập Conflict Detection Matrix.

Liệt kê P3 điểm chặn.

Liệt kê Downstream Impact.

Liệt kê Evidence Required.

Đề xuất Smoke Required, chưa viết test.

Trả report đúng 14 mục.

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

Hoàn tất khi:

Không sửa file.

Không tạo migration.

Không sửa seed.

Không thêm test.

Có đầy đủ 3 matrix: current state, gap, conflict.

Có điểm chặn list.

Có downstream impact.

Có evidence required.

Có proposed smoke.

Report đủ 14 mục.

Không gọi Gateway PASS / Completion Decision / Release Readiness / Production Readiness.

FAIL GATE

Fail nếu:

Có sửa file.

Có migration mới.

Có seed update.

Có API/test/feature mới.

Report thiếu 14 mục.

Cho phép Product/SKU/Recipe Active thay Sellable.

Cho phép final price không có QuoteSnapshot.

Cho phép Order Draft thành Official Order khi chưa có Customer Confirmation.

Cho phép Payment selected thành Paid.

Cho phép revenue chưa verified đi vào commission/ROAS/payout.

Gọi Release Readiness/Production Readiness/Go-live Decision.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_P3_1_TECHNICAL_DESIGN.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_SOURCE_CLARIFICATION.

## BLOCKED_NEED_CODEBASE_CLEANUP_BEFORE_DESIGN.

NEXT FILE: PHASE-03-COMMERCE-RUNTIME/01-P3-1-TECHNICAL-DESIGN-ONLY.md

ĐIỀU KIỆN SANG FILE TIẾP THEO: Chỉ chuyển sang P3.1 sau khi dev/Codex/Copilot chạy prompt Analysis Only này, trả report đủ 14 mục, không sửa file, xác định rõ điểm chặn/gap/conflict/evidence/smoke đề xuất và owner đồng ý dùng report đó làm nền thiết kế kỹ thuật.

PHASE-03-COMMERCE-RUNTIME/01-P3-1-TECHNICAL-DESIGN-ONLY.md

## PROMPT-P3.1 - COMMERCE RUNTIME TECHNICAL DESIGN / WORKSTREAM / TASK / CONTRACT / EVIDENCE / SMOKE HANDOFF

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt thiết kế kỹ thuật cho PHASE-03 - Commerce Runtime.

Mục tiêu của file này là chuyển kết quả phân tích từ 00-P3-ANALYSIS-ONLY.md thành thiết kế workstream, boundary, task, contract, smoke và evidence để chuẩn bị cho các prompt triển khai giới hạn tiếp theo.

Đây không phải prompt sửa code.

Đây không phải prompt tạo migration.

Đây không phải prompt viết API ngay.

Đây không phải prompt chỉnh frontend.

Đây không phải prompt chạy smoke chính thức.

Đây là prompt thiết kế kỹ thuật để dev/Codex/Copilot xác định chính xác:

Module nào cần tạo hoặc sửa.

Domain nào là owner.

Runtime resolver nào cần có.

Entity/schema nào cần đề xuất.

API/DTO nào cần đề xuất.

Command/query nào có side effect.

Guard nào bắt buộc.

Audit/evidence/idempotency nào bắt buộc.

Smoke nào phải viết ở bước sau.

Evidence nào phải nộp ở bước sau.

File implementation nào được phép mở ở từng prompt nhỏ.

## 3. MODE

## MODE: TECHNICAL DESIGN ONLY - DO NOT MODIFY FILES

Trong mode này, dev/Codex/Copilot chỉ được phép:

Thiết kế workstream.

Thiết kế task breakdown.

Thiết kế domain boundary.

Thiết kế service/resolver/guard contract.

Thiết kế API/DTO contract ở mức đề xuất.

Thiết kế DB/entity/migration ở mức đề xuất.

Thiết kế state machine.

Thiết kế test/smoke/evidence plan.

Thiết kế handoff cho các prompt implementation tiếp theo.

Viết report thiết kế theo format bắt buộc.

Không được:

Sửa code.

Sửa migration.

Sửa seed.

Sửa test.

Sửa frontend.

Sửa Admin UI.

Tạo API thật.

Tạo DTO thật.

Tạo worker thật.

Tạo dữ liệu thật.

Chạy smoke chính thức.

Mở Gateway.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

## 4. SOURCE-OF-TRUTH

Thiết kế PHASE 3 phải bám theo các nguồn sau.

## 4.1. MASTER Governance

## MASTER-00 - GINSENGFOOD MASTER TECHNICAL DOCUMENTATION INDEX.

## MASTER-01 - GLOBAL SOURCE OF TRUTH / OWNERSHIP BOUNDARY.

## MASTER-02 - CROSS-PACK DEPENDENCY GRAPH.

## MASTER-03 - TRACEABILITY MATRIX STANDARD.

## MASTER-04 - DEV HANDOFF GOVERNANCE.

## MASTER-05 - IMPLEMENTATION EXECUTION GOVERNANCE.

## MASTER-06 - QUALITY / TEST / SMOKE / EVIDENCE GOVERNANCE.

## MASTER-08 - BUSINESS RULE / PRODUCT / MEMBER / PRICING / PROGRAM GOVERNANCE.

## MASTER-09 - EVIDENCE / COMPLETION / GATEWAY / RELEASE READINESS GOVERNANCE.

## MASTER-10 - MASTER FINAL INDEX / PACK STATUS / NEXT IMPLEMENTATION ROADMAP.

## 4.2. PHASE trước

## PHASE-00-FOUNDATION-RBAC-AUDIT-EVIDENCE-IDEMPOTENCY.

## PHASE-01-PRODUCT-SKU-RECIPE-ACTIVATION.

## PHASE-02-OPERATIONAL-CORE.

PHASE-03/00-P3-ANALYSIS-ONLY report.

## 4.3. Không dùng làm source-of-truth

Không được dùng các nguồn sau để override rule đã khóa:

Code cũ nếu code đang sai rule.

Seed cũ nếu seed đang tự set sellable.

API cũ nếu API đang trả final price không có QuoteSnapshot.

UI cũ nếu UI đang cho mở bán bằng Product Active/SKU Active.

AI/Gateway content cũ nếu đang hardcode giá/ship/VAT/discount.

Report smoke cục bộ nếu chưa có owner review.

Evidence submitted nếu chưa được accepted.

## 5. ENTRY CONDITION

Chỉ được chạy prompt này khi đã có output của 00-P3-ANALYSIS-ONLY.md.

## 5.1. Điều kiện đầu vào bắt buộc

Analysis report đã có đủ 14 mục.

Không có file bị sửa trong bước Analysis Only.

Đã có Current Implementation State Matrix.

Đã có Gap Analysis Matrix.

Đã có Conflict Detection Matrix.

Đã có P3 điểm chặn List.

Đã có Downstream Impact.

Đã có Evidence Required.

Đã có Proposed Smoke Required.

Owner đồng ý chuyển sang Technical Design.

## 5.2. Nếu thiếu entry condition

Nếu thiếu một trong các mục trên, dev/Codex/Copilot phải dừng và trả trạng thái:

## BLOCKED_NEED_P3_ANALYSIS_REPORT

Không được tự thiết kế dựa trên phỏng đoán.

## 6. OBJECTIVE

Mục tiêu của P3.1 Technical Design là thiết kế đầy đủ bộ triển khai cho Commerce Runtime, bao gồm:

Sellable Gate Runtime.

Listed Price Runtime.

Program Eligibility Runtime.

Member Benefit Eligibility Runtime.

Diamond Referral Eligibility Runtime.

QuoteSnapshot.

Cart.

Order Draft.

Customer Confirmation.

Official Order.

Order Code.

Order State Machine.

Payment Selection.

Payment Core Confirmation.

Shipping Runtime.

Invoice/Tax Runtime.

Verified Revenue.

Commission/ROAS handoff.

Integration contracts cho AI/Gateway/Ads/Live/CRM/IVR.

Smoke/evidence/done gate cho từng prompt implementation tiếp theo.

## 7. CORE DESIGN PRINCIPLES

## 7.1. Không xây hệ thống bằng copy-paste code

PHASE 3 không được hiểu là lấy code rời rạc từ AI rồi dán vào dự án.

Commerce Runtime là lõi giao dịch thật, liên quan trực tiếp đến:

Giá bán.

Tồn bán được.

Chương trình khuyến mại.

Quyền lợi thành viên.

Xác nhận đơn.

Thanh toán.

Giao hàng.

Doanh thu xác minh.

Hoa hồng.

## ROAS.

Kế toán.

Tranh chấp khách hàng.

Nếu dev chỉ copy-paste code rời rạc:

Sellable có thể mở sai.

Giá có thể báo sai.

Đơn có thể ghi nhận sai.

Thanh toán có thể bị hiểu sai là đã paid.

Hoa hồng có thể tính sai.

ROAS có thể sai.

Gateway có thể mở khi chưa đủ evidence.

Chi phí sửa lỗi về sau cao hơn làm đúng từ đầu.

Vì vậy P3.1 chỉ thiết kế, chưa code.

## 7.2. Commerce Runtime là owner quyết định giao dịch

Commerce Runtime là owner của:

sellable_status.

listed_price_status.

program eligibility.

member benefit eligibility.

diamond referral eligibility.

QuoteSnapshot.

final quote price.

Cart validation.

Order Draft validation.

Customer Confirmation gate.

Official Order state.

Payment status.

Shipping fee.

VAT/tax quote.

verified revenue.

## 7.3. Các hệ thống khác chỉ consume runtime

Các hệ thống sau chỉ consume dữ liệu từ Commerce Runtime:

AI Advisor.

Facebook Gateway.

Ads/ROAS.

MC AI Live.

## CRM.

## IVR.

Landing page.

Website.

Admin UI display layer.

Các hệ thống này không được tự tính:

Giá cuối.

Tồn bán được.

Discount.

Member benefit.

Diamond benefit.

Shipping fee.

## VAT.

Sellable.

Revenue verified.

Commission.

## ROAS.

## 7.4. Snapshot bất biến

Các quyết định giao dịch phải được snapshot tại thời điểm phát sinh.

Đặc biệt:

QuoteSnapshot không được mutate sau khi tạo.

Official Order phải lưu quote/order data đã xác nhận.

Payment confirmation phải có audit.

Verified Revenue phải có checkpoint rõ.

Commission/ROAS/payout không được dùng dữ liệu chưa verified.

## 7.5. Fail-safe / fail-closed

Nếu thiếu runtime policy hoặc resolver lỗi, hệ thống phải fail-safe.

Không được:

Tự giả định giá.

Tự giả định phí ship.

Tự giả định VAT.

Tự giả định discount.

Tự giả định sellable.

Tự cho phép order khi thiếu QuoteSnapshot.

Tự verified revenue khi thiếu checkpoint.

## 8. SCOPE IN

P3.1 Technical Design bao gồm thiết kế các workstream sau:

WS-P3-A - Sellable Gate Runtime.

WS-P3-B - Price / Program / Member Benefit / Diamond Referral / QuoteSnapshot.

WS-P3-C - Cart / Order Draft / Customer Confirmation.

WS-P3-D - Official Order / Order Code / Order State Machine.

WS-P3-E - Payment / Shipping / Invoice / Tax.

WS-P3-F - Verified Revenue / Commission / ROAS Handoff.

WS-P3-G - Security / RBAC / Audit / Idempotency / Evidence.

WS-P3-H - Integration Contract for AI / Gateway / Ads / Live / CRM / IVR.

WS-P3-I - Migration / Seed / Config Strategy.

WS-P3-J - Smoke / Evidence / Done Gate Plan.

## 9. SCOPE OUT

P3.1 Technical Design không bao gồm:

Không sửa code.

Không tạo migration thật.

Không sửa seed thật.

Không tạo API thật.

Không tạo DTO thật.

Không tạo service thật.

Không tạo worker thật.

Không sửa frontend.

Không sửa Admin UI.

Không chạy smoke chính thức.

Không nộp evidence accepted.

Không mở Gateway.

Không triển khai IVR.

Không tính chính sách giá mới ngoài rule đã khóa.

Không quyết định thay owner business rule.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 10. ALLOWED FILE CHANGE BOUNDARY

Không được sửa file trong P3.1.

Allowed file change boundary:

Nhóm file

Được sửa trong P3.1 không

Ghi chú

Backend code

Không

Chỉ thiết kế

Migration

Không

Chỉ đề xuất

Seed

Không

Chỉ đề xuất

Tests

Không

Chỉ đề xuất

Frontend/Admin UI

Không

Chỉ đề xuất

Markdown docs

Không

Chỉ đề xuất cập nhật

Config

Không

Chỉ đề xuất

Gateway config

Không

Gateway vẫn bị chặn

Evidence registry

Không

Chỉ nêu evidence cần có

Nếu agent tự sửa file, prompt bị FAIL.

## 11. REQUIRED INPUT

Dev/Codex/Copilot phải sử dụng các input sau.

## 11.1. Từ P3 Analysis Report

Current Implementation State Matrix.

Gap Analysis Matrix.

Conflict Detection Matrix.

P3 điểm chặn.

Downstream Impact.

Evidence Required.

Proposed Smoke Required.

Kết luận handoff.

## 11.2. Từ codebase

Product/SKU modules.

Recipe/Formula modules.

Inventory/Warehouse modules.

Recall/Sale Lock modules.

Price/Program modules nếu có.

Member/Customer modules nếu có.

Quote/Cart/Order modules nếu có.

Payment/Shipping/Tax modules nếu có.

Revenue/Commission/ROAS modules nếu có.

Integration modules nếu có.

## 11.3. Từ governance

RBAC policy.

Audit policy.

Idempotency policy.

Evidence policy.

Release gate policy.

Gateway bị chặn policy.

## 12. SYSTEM BOUNDARY / OWNER MAP

## 12.1. Owner map

Business object

Owner chính

Consumer

Không được làm

Product active

Product/SKU Core

Commerce

Không dùng thay Sellable

SKU active

Product/SKU Core

Commerce

Không dùng thay Sellable

Recipe active

Recipe Core

Production/Commerce reference

Không dùng thay Sellable

Finished goods stock

Operational Core

Commerce

Commerce không tự sửa stock truth

Batch RELEASED

Operational Core

Commerce

QC_PASS không đủ

Warehouse receipt confirmed

Operational Core

Commerce

Không tự set sellable

Recall/Sale Lock

Operational Core

Commerce/AI/Gateway/CRM/Ads

Không được bypass

Sellable status

Commerce Runtime

AI/Gateway/CRM/Ads/Live

Channel không tự tính

Listed price status

Commerce Runtime

AI/Gateway/Cart/Order

Không hardcode giá

Program eligibility

Commerce Runtime

AI/Gateway/Cart/Order

Không tự tính ở channel

Member benefit

Commerce Runtime

AI/Gateway/Cart/Order

Không tự suy diễn hạng

Diamond referral eligibility

Commerce Runtime

Finance/Reward/Ads

Không tự tính commission

QuoteSnapshot

Commerce Runtime

AI/Gateway/Order

Không final price nếu thiếu

Cart

Commerce Runtime

AI/Gateway/UI

Không đồng nghĩa Order

Order Draft

Commerce Runtime

AI/Gateway/UI

Không đồng nghĩa Official Order

Customer Confirmation

Commerce Runtime

Order Core

Không bỏ qua

Official Order

Commerce Runtime

Payment/Shipping/Finance

Không tạo khi thiếu confirmation

Payment status

Payment Core/Commerce boundary

Order/Finance

Payment selected không phải Paid

Shipping fee

Commerce Runtime/Shipping policy

Quote/Order/AI

Không hardcode

VAT/Tax

Commerce Runtime/Tax policy

Quote/Invoice/MISA

Không hardcode

Verified Revenue

Commerce/Finance boundary

Ads/Commission/ROAS

Không dùng order chưa verified

## 13. TARGET TECHNICAL LAYERS

P3.1 phải thiết kế đầy đủ các lớp sau.

## 13.1. Domain Layer

Các domain object cần thiết kế:

SellableDecision.

SellableCondition.

ListedPriceDecision.

ProgramEligibilityDecision.

MemberBenefitDecision.

DiamondReferralDecision.

QuoteSnapshot.

QuoteLineSnapshot.

Cart.

CartLine.

OrderDraft.

OrderDraftLine.

CustomerConfirmation.

OfficialOrder.

OfficialOrderLine.

PaymentSelection.

PaymentConfirmation.

ShippingQuote.

TaxQuote.

VerifiedRevenueRecord.

## 13.2. Resolver / Service Layer

Các resolver/service cần thiết kế:

SellableGateResolver.

ListedPriceResolver.

ProgramEligibilityResolver.

MemberBenefitResolver.

DiamondReferralResolver.

QuoteSnapshotService.

CartService.

OrderDraftService.

CustomerConfirmationService.

OfficialOrderService.

OrderCodeGenerator.

PaymentSelectionService.

PaymentConfirmationService.

ShippingFeeResolver.

TaxResolver.

VerifiedRevenueService.

CommerceRuntimePolicyResolver.

CommerceAuditService.

CommerceEvidenceService.

## 13.3. API Layer

Các API cần thiết kế ở mức contract:

Sellable check API.

QuoteSnapshot create API.

QuoteSnapshot read API.

Cart create/update API.

Order Draft create/update API.

Customer Confirmation API.

Official Order create-from-confirmed-draft API.

Payment selection API.

Payment confirmation/callback API.

Shipping quote API.

Invoice/tax info API.

Verified revenue checkpoint API.

Runtime consumer API cho AI/Gateway/CRM/Ads/Live.

## 13.4. Database Layer

Các bảng/entity đề xuất phải được thiết kế nhưng chưa tạo migration:

commerce_sellable_decision_log.

commerce_listed_price.

commerce_program_policy.

commerce_member_benefit_snapshot.

commerce_diamond_referral_snapshot.

commerce_quote_snapshot.

commerce_quote_snapshot_line.

commerce_cart.

commerce_cart_line.

commerce_order_draft.

commerce_order_draft_line.

commerce_customer_confirmation.

commerce_order.

commerce_order_line.

commerce_payment_selection.

commerce_payment_confirmation.

commerce_shipping_quote.

commerce_tax_quote.

commerce_verified_revenue.

commerce_runtime_policy_version.

Tên bảng có thể điều chỉnh theo convention codebase thật, nhưng ý nghĩa domain không được làm sai.

## 13.5. Worker / Outbox Layer

Các side effect phải có outbox hoặc cơ chế tương đương:

Order created event.

Payment confirmed event.

Shipping requested event.

Invoice requested event.

Order verified event.

Verified revenue created event.

Commission WAITING event.

ROAS verified event.

Gateway handoff event.

CRM post-order event.

Không được để business module gọi trực tiếp MISA/Gateway/Ads theo kiểu bypass.

## 13.6. Admin UI Layer

Admin UI chỉ là lớp hiển thị/quản trị theo permission.

Admin UI không được tự quyết định:

Sellable.

Final price.

Discount.

Member benefit.

Verified revenue.

Admin UI có thể cần thiết kế màn hình:

Listed Price Management.

Program Management.

Sellable Decision Viewer.

QuoteSnapshot Viewer.

Order Draft Viewer.

Official Order Viewer.

Payment Status Viewer.

Shipping/Tax Viewer.

Verified Revenue Viewer.

Evidence/Audit Viewer.

## 14. WORKSTREAM DESIGN

## 14.1. WS-P3-A - SELLABLE GATE RUNTIME

## 14.1.1. Objective

Thiết kế Sellable Gate Runtime là gate thương mại riêng, không dùng Product Active/SKU Active/Recipe Active thay Sellable.

## 14.1.2. Required conditions

Sellable chỉ được true nếu tối thiểu thỏa:

Product active.

SKU active.

Listed price active.

Stock available.

Finished goods stock đến từ warehouse receipt confirmed.

Batch RELEASED.

Không recall.

Không sale lock.

Không quality hold.

Không channel suppression.

Runtime policy available.

Resolver không lỗi.

Không vi phạm RBAC/policy khi admin override.

## 14.1.3. Proposed design objects

SellableDecision.

SellableConditionResult.

SellableBlockReason.

SellableRuntimePolicy.

ChannelSellableContext.

## 14.1.4. Proposed resolver

SellableGateResolver

Input đề xuất:

sku_id.

channel.

customer_context optional.

requested_quantity optional.

request_time.

correlation_id.

Output đề xuất:

sellable_status.

allow_quote.

allow_cart.

allow_order_draft.

block_reasons.

stock_available_quantity.

policy_version.

evaluated_at.

evidence_refs.

audit_ref.

## 14.1.5. Required guard

ProductActiveGuard.

SkuActiveGuard.

ListedPriceActiveGuard.

ReleasedStockGuard.

WarehouseConfirmedGuard.

RecallStopSaleGuard.

SaleLockGuard.

QualityHoldGuard.

ChannelSuppressionGuard.

RuntimePolicyAvailableGuard.

## 14.1.6. Proposed smoke

Product active nhưng thiếu listed price -> not sellable.

SKU active nhưng thiếu stock -> not sellable.

Batch QC_PASS nhưng chưa RELEASED -> not sellable.

Warehouse chưa confirmed -> not sellable.

Recall active -> not sellable.

Sale lock active -> not sellable.

Policy unavailable -> fail-safe not sellable.

Đủ điều kiện -> sellable true.

## 14.1.7. Evidence required

Code map SellableGateResolver.

Test result cho positive/negative cases.

Audit evidence.

Evidence registry link.

API response evidence.

Integration evidence AI/Gateway consume sellable only.

## 14.1.8. Handoff file

NEXT IMPLEMENTATION FILE: 02-P3-2A-SELLABLE-GATE-RUNTIME.md

## 14.2. WS-P3-B - PRICE / PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / QUOTE SNAPSHOT

## 14.2.1. Objective

Thiết kế runtime báo giá chuẩn, trong đó QuoteSnapshot là nguồn sự thật cho giá cuối tại thời điểm báo giá.

Không có QuoteSnapshot thì không có final price.

## 14.2.2. Required runtime decisions

Listed price active.

Program eligibility.

Program priority.

Program conflict.

Member benefit eligibility.

Diamond referral eligibility.

Shipping fee.

VAT/tax.

Final payable amount.

Quote freeze window.

Quote expiry.

Policy version.

## 14.2.3. Proposed design objects

ListedPriceDecision.

ProgramEligibilityDecision.

MemberBenefitDecision.

DiamondReferralDecision.

QuoteSnapshot.

QuoteLineSnapshot.

QuoteAdjustmentSnapshot.

QuotePolicyVersion.

QuoteExpiryPolicy.

## 14.2.4. Proposed services

ListedPriceResolver.

ProgramEligibilityResolver.

MemberBenefitResolver.

DiamondReferralResolver.

ShippingFeeResolver.

TaxResolver.

QuoteSnapshotService.

## 14.2.5. QuoteSnapshot required fields

quote_snapshot_id.

customer_id nullable.

session_id.

channel.

quote_context.

sku_id.

quantity.

listed_price.

listed_price_version.

program_id nullable.

program_name nullable.

program_discount_amount.

program_discount_rate.

member_tier nullable.

member_benefit_amount.

diamond_referral_id nullable.

diamond_referral_eligibility.

shipping_fee.

shipping_policy_version.

VAT/tax amount.

tax_policy_version.

final_payable_amount.

currency.

created_at.

expires_at.

freeze_window_type.

policy_version.

resolver_version.

correlation_id.

immutable_hash optional.

audit_ref.

## 14.2.6. Required guard

NoFinalPriceWithoutQuoteSnapshotGuard.

ImmutableQuoteSnapshotGuard.

QuoteExpiryGuard.

QuotePolicyVersionGuard.

NoHardcodePriceGuard.

ChannelCannotCalculatePriceGuard.

## 14.2.7. Proposed smoke

Không có QuoteSnapshot -> reject final price.

QuoteSnapshot tạo đủ listed price/program/member/shipping/tax/final.

QuoteSnapshot hết hạn -> không cho xác nhận.

QuoteSnapshot immutable.

AI/Gateway chỉ nhận final price từ QuoteSnapshot.

Runtime policy unavailable -> fail-safe không quote.

## 14.2.8. Evidence required

QuoteSnapshot DB evidence.

QuoteSnapshot API evidence.

Immutable test evidence.

Expiry test evidence.

No hardcode evidence.

AI/Gateway integration evidence.

## 14.2.9. Handoff file

NEXT IMPLEMENTATION FILE: 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md

## 14.3. WS-P3-C - CART / ORDER DRAFT / CUSTOMER CONFIRMATION

## 14.3.1. Objective

Thiết kế luồng chuyển từ quote sang cart, từ cart sang order draft, và từ order draft sang customer confirmation.

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới cho phép tạo Official Order.

## 14.3.2. Proposed design objects

Cart.

CartLine.

CartValidationResult.

OrderDraft.

OrderDraftLine.

CustomerConfirmation.

CustomerConfirmationPayload.

OrderDraftExpiryPolicy.

## 14.3.3. Cart rules

Cart chỉ chứa sản phẩm dự kiến mua.

Cart phải validate sellable.

Cart phải gắn QuoteSnapshot hoặc tạo quote trước khi hiển thị final price.

Cart không có order_code.

Cart không được gọi là đơn chính thức.

Cart có thể bị abandoned/expired/converted.

## 14.3.4. Order Draft rules

Order Draft là bản nháp xác nhận đơn.

Order Draft phải có QuoteSnapshot còn hiệu lực.

Order Draft phải có thông tin khách/nhận hàng đủ theo policy.

Order Draft phải có payment selection nếu cần.

Order Draft phải hiển thị tổng thanh toán cuối cùng từ QuoteSnapshot.

Order Draft chưa có order_code chính thức.

Order Draft không được ghi nhận là official revenue.

## 14.3.5. Customer Confirmation rules

Customer phải xác nhận rõ bản Order Draft.

Confirmation phải gắn order_draft_id.

Confirmation phải gắn quote_snapshot_id.

Confirmation phải có timestamp.

Confirmation phải có channel.

Confirmation phải có customer identity/session identity.

Confirmation phải chống duplicate.

Confirmation phải kiểm tra quote còn hiệu lực.

Confirmation phải kiểm tra sellable lại trước official order nếu policy yêu cầu.

## 14.3.6. Proposed state

Cart state:

## ACTIVE.

## UPDATED.

## ABANDONED.

## EXPIRED.

## CONVERTED_TO_ORDER_DRAFT.

Order Draft state:

## DRAFT_CREATED.

## WAITING_CUSTOMER_CONFIRMATION.

## CUSTOMER_CONFIRMED.

## EXPIRED.

## CANCELLED.

## CONVERTED_TO_OFFICIAL_ORDER.

Customer Confirmation state:

## SUBMITTED.

## ACCEPTED.

## REJECTED_EXPIRED_QUOTE.

## REJECTED_DUPLICATE.

## REJECTED_SELLABLE_CHANGED.

## 14.3.7. Proposed smoke

Cart created không tạo order.

Cart line not sellable -> reject.

Order Draft created không tạo order_code.

Order Draft thiếu quote -> reject.

Order Draft thiếu customer/shipping info -> incomplete.

Customer confirm expired quote -> reject.

Customer confirm hợp lệ -> accepted.

Duplicate confirmation -> không tạo duplicate order.

## 14.3.8. Evidence required

State machine evidence.

API evidence.

Idempotency evidence.

Audit evidence.

Customer confirmation evidence.

Duplicate prevention evidence.

## 14.3.9. Handoff file

NEXT IMPLEMENTATION FILE: 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md

## 14.4. WS-P3-D - OFFICIAL ORDER / ORDER CODE / ORDER STATE MACHINE

## 14.4.1. Objective

Thiết kế Official Order chỉ được tạo sau Customer Confirmation hợp lệ.

Không có order_code thì không được nói "đơn hàng đã ghi nhận chính thức".

## 14.4.2. Required rules

Official Order phải tạo từ Order Draft đã được Customer Confirmation.

Official Order phải có QuoteSnapshot.

Official Order phải có order_code.

order_code chỉ tạo một lần.

Command tạo order phải idempotent.

Không duplicate order khi khách bấm lại.

Order phải có audit state transition.

Order không tự paid.

Order không tự verified revenue.

Order không tự commission final.

## 14.4.3. Proposed design objects

OfficialOrder.

OfficialOrderLine.

OrderCode.

OrderState.

OrderStateTransition.

OrderCreationCommand.

OrderCreationResult.

OrderIdempotencyRecord.

## 14.4.4. Proposed order state

## OFFICIAL_CREATED.

## WAITING_PAYMENT.

## PAYMENT_SELECTED.

## PAYMENT_WAITING_CONFIRMATION.

## PAID.

## PREPARING_FULFILLMENT.

## SHIPPING_REQUESTED.

## SHIPPING_IN_PROGRESS.

## DELIVERED.

## ORDER_VERIFIED.

## CANCELLED.

## EXPIRED.

## RETURNED.

REFUNDED nếu áp dụng.

Tên state cuối cùng phải điều chỉnh theo codebase thật, nhưng nguyên tắc không được sai:

Created không phải Paid.

Paid không phải Verified Revenue.

Delivered không tự động là Verified Revenue nếu policy chưa khóa.

ORDER_VERIFIED mới cho verified revenue nếu đây là checkpoint được chọn.

## 14.4.5. Required guard

RequireCustomerConfirmationGuard.

RequireQuoteSnapshotGuard.

RequireOrderDraftValidGuard.

RequireIdempotencyKeyGuard.

DuplicateOrderGuard.

RequireOrderCodeGuard.

OrderStateTransitionGuard.

RecallBeforeOrderGuard.

SellableBeforeOrderGuard nếu policy yêu cầu recheck.

## 14.4.6. Proposed smoke

Tạo Official Order khi chưa confirm -> reject.

Confirm hợp lệ -> tạo Official Order.

Official Order có order_code.

Retry cùng idempotency key -> không tạo duplicate.

Order created không paid.

Order created không verified revenue.

Recall phát sinh trước confirm -> reject hoặc requote theo policy.

Audit đầy đủ state transition.

## 14.4.7. Evidence required

Order state evidence.

order_code generation evidence.

Idempotency evidence.

Audit evidence.

Duplicate prevention evidence.

API response evidence.

Negative test evidence.

## 14.4.8. Handoff file

NEXT IMPLEMENTATION FILE: 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md

## 14.5. WS-P3-E - PAYMENT / SHIPPING / INVOICE / TAX

## 14.5.1. Objective

Thiết kế runtime cho payment, shipping, invoice và tax.

Payment selected không đồng nghĩa Paid.

COD chỉ là phương thức nhận hàng rồi thanh toán, không tự thêm phí COD nếu policy chưa khóa.

Shipping fee/VAT phải do runtime policy tính, không hardcode.

## 14.5.2. Payment rules

Payment selected chỉ là khách chọn phương thức thanh toán.

Payment selected không set paid.

Paid chỉ được set khi Payment Core confirmation.

Bank transfer cần trạng thái WAITING/confirmed/rejected.

COD là payment method, chưa paid tại thời điểm chọn.

Không tự thêm COD fee nếu policy chưa có.

Payment confirmation phải có audit.

Payment callback nếu có phải verify signature/source nếu áp dụng.

Payment state transition phải idempotent.

## 14.5.3. Shipping rules

Shipping fee phải do ShippingFeeResolver hoặc policy resolver tính.

Free ship chỉ áp dụng nếu runtime policy xác nhận.

ETA phải từ shipping policy/provider hoặc config.

Nếu đã có Order Draft đủ địa chỉ, khi khách hỏi ETA không hỏi lại địa chỉ.

Shipping quote phải gắn QuoteSnapshot/OrderDraft/OfficialOrder tùy stage.

Shipping state không tự set order verified.

## 14.5.4. Invoice / Tax rules

VAT/tax phải có policy.

Tax quote phải vào QuoteSnapshot nếu áp dụng.

Invoice request phải có state.

MISA handoff nếu có phải đi qua integration layer.

Không hardcode VAT trong AI/Gateway/content.

Missing tax policy phải fail-safe theo rule.

## 14.5.5. Proposed design objects

PaymentSelection.

PaymentConfirmation.

PaymentState.

ShippingQuote.

ShippingPolicyDecision.

InvoiceRequest.

TaxQuote.

TaxPolicyDecision.

## 14.5.6. Proposed smoke

COD selected -> not paid.

Bank transfer selected -> payment WAITING.

Payment confirmed -> paid.

Payment rejected -> not paid.

COD không tự thêm phí.

Shipping fee runtime xuất hiện trong QuoteSnapshot.

VAT runtime xuất hiện trong QuoteSnapshot nếu áp dụng.

Missing shipping/tax policy -> fail-safe.

AI/Gateway không hardcode phí ship/VAT.

## 14.5.7. Evidence required

Payment state evidence.

Paid confirmation evidence.

COD policy evidence.

Shipping policy evidence.

Tax policy evidence.

Invoice request evidence.

Audit/evidence registry link.

## 14.5.8. Handoff file

NEXT IMPLEMENTATION FILE: 06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md

## 14.6. WS-P3-F - VERIFIED REVENUE / COMMISSION / ROAS HANDOFF

## 14.6.1. Objective

Thiết kế checkpoint Verified Revenue.

No ORDER_VERIFIED thì không final commission.

No ORDER_VERIFIED thì không verified ROAS.

No ORDER_VERIFIED thì không payout.

## 14.6.2. Required rules

Order created không phải verified revenue.

Payment selected không phải verified revenue.

Paid chưa chắc là verified revenue nếu policy yêu cầu thêm checkpoint.

Delivered chưa chắc là verified revenue nếu policy yêu cầu ORDER_VERIFIED.

ORDER_VERIFIED hoặc equivalent checkpoint mới tạo verified revenue.

Commission chỉ WAITING trước verified.

ROAS chỉ verified sau verified revenue.

Payout chỉ sau verified revenue và finance approval nếu áp dụng.

Revenue reversal phải có path nếu return/refund/cancel xảy ra.

Ads không được dùng draft/unpaid/unverified revenue làm verified ROAS.

## 14.6.3. Proposed design objects

VerifiedRevenueRecord.

RevenueVerificationCheckpoint.

RevenueState.

CommissionEligibilitySnapshot.

ROASHandoffRecord.

PayoutEligibilityRecord.

RevenueReversalRecord.

## 14.6.4. Proposed revenue state

## NOT_REVENUE.

## ORDER_CREATED_UNVERIFIED.

## PAYMENT_WAITING.

## PAID_UNVERIFIED.

## DELIVERY_WAITING.

## ORDER_VERIFIED.

## VERIFIED_REVENUE_CREATED.

REVENUE_REVERSED nếu áp dụng.

## 14.6.5. Required guard

NoVerifiedRevenueWithoutOrderVerifiedGuard.

NoFinalCommissionWithoutVerifiedRevenueGuard.

NoVerified ROASWithoutVerifiedRevenueGuard.

NoPayoutWithoutVerifiedRevenueGuard.

RevenueReversalGuard.

AdsRevenueBoundaryGuard.

## 14.6.6. Proposed smoke

Order created -> no verified revenue.

Payment selected -> no verified revenue.

Paid nhưng chưa ORDER_VERIFIED -> no final commission.

ORDER_VERIFIED -> verified revenue created.

Verified revenue -> ROAS handoff allowed.

No verified revenue -> payout bị chặn.

Refund/return nếu có -> revenue reversal path.

## 14.6.7. Evidence required

ORDER_VERIFIED evidence.

Verified revenue record evidence.

Commission WAITING/final evidence.

ROAS handoff evidence.

Payout bị chặn evidence.

Audit/evidence registry link.

## 14.6.8. Handoff file

NEXT IMPLEMENTATION FILE: 07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md

## 14.7. WS-P3-G - SECURITY / RBAC / AUDIT / IDEMPOTENCY / EVIDENCE

## 14.7.1. Objective

Đảm bảo mọi command nhạy cảm trong Commerce Runtime dựa trên PHASE 0.

## 14.7.2. Required RBAC

Các action cần permission riêng:

Manage listed price.

Manage program.

Manage member benefit policy.

Create quote.

Create order draft.

Confirm customer order nếu nhân sự thao tác thay khách.

Create official order.

Confirm payment.

Override order state.

Verify revenue.

Reverse revenue.

Export audit/evidence.

## 14.7.3. Required audit

Audit bắt buộc cho:

Sellable decision.

Price activation.

Program eligibility decision.

QuoteSnapshot creation.

Cart update.

Order Draft creation/update.

Customer Confirmation.

Official Order creation.

Order state transition.

Payment selection.

Payment confirmation.

Shipping fee decision.

Tax decision.

Verified revenue creation.

Commission/ROAS handoff.

Override/break-glass.

## 14.7.4. Required idempotency

Idempotency bắt buộc cho:

Create QuoteSnapshot nếu side effect lưu DB.

Add/update cart nếu có side effect.

Create Order Draft.

Customer Confirmation.

Create Official Order.

Payment callback.

Payment confirmation.

Shipping request.

Invoice request.

Verified revenue creation.

Commission/ROAS handoff.

## 14.7.5. Required evidence

Evidence phải có cho:

P0 test pass.

Smoke pass.

Build pass.

Migration validation.

Seed validation.

API evidence.

Audit evidence.

RBAC evidence.

Idempotency evidence.

Negative test evidence.

Integration evidence.

Owner review evidence.

## 14.8. WS-P3-H - INTEGRATION CONTRACT FOR AI / GATEWAY / ADS / LIVE / CRM / IVR

## 14.8.1. Objective

Thiết kế runtime contract để các hệ thống khác consume Commerce Runtime đúng cách.

## 14.8.2. AI Advisor contract

AI Advisor chỉ được dùng:

sellable_status.

block_reason public-safe.

selected_product_public_name.

QuoteSnapshot.

final_payable_amount.

shipping fee.

VAT/tax.

order draft display payload.

customer confirmation state.

official order_code sau khi có order chính thức.

AI Advisor không được tự tính:

Giá.

Discount.

Quyền lợi member.

Ship.

## VAT.

Sellable.

Revenue.

Commission.

## 14.8.3. Gateway contract

Gateway chỉ chuyển tiếp:

Public comment handoff.

Messenger quote payload.

Order draft confirmation payload.

Customer confirmation event.

Official order status.

Gateway không được:

Tự tạo final price.

Tự mở bán SKU không sellable.

Tự gọi order chính thức nếu thiếu confirmation.

Tự gọi payment paid.

Tự gọi verified revenue.

## 14.8.4. Ads/ROAS contract

Ads chỉ consume:

verified_revenue.

order_verified_at.

campaign attribution nếu hợp lệ.

revenue reversal nếu có.

Ads không consume:

Cart value.

Quote value chưa order.

Order Draft value.

Unpaid order.

Unverified paid order.

## 14.8.5. CRM contract

CRM chỉ gợi ý mua lại khi:

SKU sellable.

Không recall.

Không sale lock.

Runtime quote available.

Customer context hợp lệ.

Không complaint/open issue nếu policy chặn.

## 14.8.6. Live contract

MC AI Live chỉ dùng runtime payload.

Public live không trả giá sâu nếu policy yêu cầu Messenger.

Live không tự tính final price.

## 14.8.7. IVR contract

IVR là reserved.

P3 chỉ chừa extension point:

order_id.

order_code.

order_state.

confirmation_required flag.

IVR callback placeholder nếu sau này mở.

Không cài logic IVR trong Commerce nếu PACK-09 chưa triển khai.

## 15. DATA / MIGRATION / SEED STRATEGY

## 15.1. Migration strategy - design only

P3.1 chỉ đề xuất migration.

Mỗi migration sau này phải:

Có tên rõ.

Có rollback hoặc reversible plan.

Có constraint.

Có index cần thiết.

Có audit/correlation fields nếu áp dụng.

Không phá dữ liệu cũ.

Không tự set sellable hàng loạt nếu chưa có runtime evaluation.

Không seed giá/chương trình sai policy.

## 15.2. Seed strategy - design only

Seed chỉ được dùng để tạo dữ liệu nền cho dev/test.

Seed không được:

Tự set sellable true.

Tự bypass listed price.

Tự bypass stock.

Tự bypass warehouse confirmed.

Tự bypass recall/sale lock.

Tự tạo verified revenue.

Tự tạo commission final.

Tự mở Gateway.

Seed có thể gồm:

Listed price dev/test.

Program policy dev/test.

Member tier sample.

Quote policy sample.

Shipping policy sample.

Tax policy sample.

Order state sample.

Negative test fixture.

## 16. API CONTRACT DESIGN - PROPOSED ONLY

API name cuối cùng phải khớp convention codebase thật. P3.1 chỉ thiết kế contract.

API group

Purpose

Side effect

Idempotency

## RBAC

Audit

GET /commerce/sellable

Check sellable

Không

Không bắt buộc

Có nếu internal

Có log decision nếu lưu

POST /commerce/quote-snapshots

Create quote snapshot

Có

Có

Có

Có

GET /commerce/quote-snapshots/{id}

Read quote

Không

Không

Có

Có read log nếu sensitive

POST /commerce/carts

Create cart

Có

Có

Context-based

Có

PATCH /commerce/carts/{id}

Update cart

Có

Có

Context-based

Có

POST /commerce/order-drafts

Create order draft

Có

Có

Context-based

Có

PATCH /commerce/order-drafts/{id}

Update order draft

Có

Có

Context-based

Có

POST /commerce/order-drafts/{id}/confirm

Customer confirmation

Có

Có

Context-based

Có

POST /commerce/orders/from-draft

Create official order

Có

Có

Có

Có

POST /commerce/orders/{id}/payment-selection

Select payment

Có

Có

Context-based

Có

POST /commerce/payments/confirm

Confirm payment

Có

Có

Có

Có

POST /commerce/shipping/quote

Shipping quote

Có/Không tùy lưu

Có nếu lưu

Có

Có

POST /commerce/tax/quote

Tax quote

Có/Không tùy lưu

Có nếu lưu

Có

Có

POST /commerce/orders/{id}/verify

Verify order/revenue

Có

Có

Có

Có

## 17. STATE MACHINE DESIGN

## 17.1. QuoteSnapshot state

## CREATED.

## ACTIVE.

## EXPIRED.

## CONSUMED_BY_ORDER_DRAFT.

## CONSUMED_BY_OFFICIAL_ORDER.

## VOIDED.

Rule:

EXPIRED quote không được dùng để confirm order.

VOIDED quote không được dùng để order.

Quote consumed không được mutate.

## 17.2. Cart state

## ACTIVE.

## UPDATED.

## ABANDONED.

## EXPIRED.

## CONVERTED_TO_ORDER_DRAFT.

Rule:

Cart không có order_code.

Cart không phải Official Order.

## 17.3. Order Draft state

## DRAFT_CREATED.

## WAITING_CUSTOMER_CONFIRMATION.

## CUSTOMER_CONFIRMED.

## EXPIRED.

## CANCELLED.

## CONVERTED_TO_OFFICIAL_ORDER.

Rule:

Order Draft không phải Official Order.

Chưa có Customer Confirmation thì không official.

Chưa có Official Order thì không có order_code chính thức.

## 17.4. Official Order state

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

REFUNDED nếu áp dụng.

Rule:

PAYMENT_SELECTED không phải PAID.

PAID không tự động là ORDER_VERIFIED nếu policy yêu cầu verify riêng.

ORDER_VERIFIED mới cho verified revenue nếu đây là checkpoint khóa.

## 17.5. Payment state

## NOT_SELECTED.

## SELECTED_COD.

## SELECTED_BANK_TRANSFER.

## WAITING_CONFIRMATION.

## CONFIRMED_PAID.

## REJECTED.

## FAILED.

REFUNDED nếu áp dụng.

Rule:

COD selected không paid.

Bank transfer selected không paid.

Paid chỉ sau confirmation.

## 17.6. Verified Revenue state

## NOT_REVENUE.

## UNVERIFIED_ORDER_VALUE.

## PAID_UNVERIFIED.

## ORDER_VERIFIED.

## VERIFIED_REVENUE_CREATED.

REVERSED nếu áp dụng.

Rule:

Unverified không dùng cho final commission.

Unverified không dùng cho verified ROAS.

Unverified không payout.

## 18. TEST / SMOKE REQUIREMENTS - DESIGN ONLY

P3.1 phải thiết kế smoke cho từng implementation file.

## 18.1. P3.2A Sellable smoke

Product inactive -> not sellable.

SKU inactive -> not sellable.

Listed price inactive -> not sellable.

No released stock -> not sellable.

QC_PASS but not RELEASED -> not sellable.

Warehouse not confirmed -> not sellable.

Recall active -> not sellable.

Sale lock active -> not sellable.

Channel suppression -> not sellable.

All conditions pass -> sellable.

## 18.2. P3.2B Quote smoke

No QuoteSnapshot -> no final price.

QuoteSnapshot created with full price breakdown.

QuoteSnapshot immutable.

Quote expired -> cannot confirm.

Shipping/VAT/member/program runtime included.

AI/Gateway no self-calculation.

## 18.3. P3.2C Cart/Order Draft smoke

Cart not order.

Order Draft not official order.

Missing QuoteSnapshot -> reject draft.

Expired QuoteSnapshot -> reject confirmation.

Customer Confirmation accepted.

Duplicate confirmation bị chặn.

## 18.4. P3.2D Official Order smoke

No Customer Confirmation -> no official order.

Confirmed draft -> official order.

order_code created once.

Retry idempotency -> one order only.

Order created not paid.

Order created not verified revenue.

## 18.5. P3.2E Payment/Shipping/Tax smoke

COD selected -> not paid.

Bank transfer selected -> WAITING.

Payment confirmed -> paid.

COD no fee unless policy.

Shipping fee runtime.

VAT/tax runtime.

Missing policy fail-safe.

## 18.6. P3.2F Verified Revenue smoke

Paid but not ORDER_VERIFIED -> no verified revenue.

ORDER_VERIFIED -> verified revenue.

No verified revenue -> no final commission.

No verified revenue -> no verified ROAS.

No verified revenue -> no payout.

Reversal path if refund/return.

## 18.7. P3.2G Final smoke/evidence report

Backend build pass.

Frontend build pass nếu có thay UI.

Migration validation pass.

Seed validation pass.

P0 test pass.

Smoke pass.

Evidence submitted.

Owner review WAITING hoặc accepted tùy trạng thái thật.

Gateway vẫn bị chặn nếu chưa có global release decision.

## 19. SECURITY / GOVERNANCE GUARDRAILS

## 19.1. RBAC

Mọi command sensitive phải enforce backend permission.

Không được chỉ kiểm tra permission ở frontend.

## 19.2. Actor Context

Mọi command side effect phải có actor context hoặc system actor rõ.

## 19.3. Correlation ID

Mọi flow quote/order/payment/revenue phải có correlation_id.

## 19.4. Audit append-only

Audit không được sửa/xóa.

## 19.5. Evidence Registry

Evidence submitted không đồng nghĩa accepted.

## 19.6. Idempotency

Bắt buộc cho:

Create quote.

Create order draft.

Confirm order.

Create official order.

Payment confirmation.

Verified revenue creation.

## 19.7. Runtime unavailable

Nếu runtime policy/resolver unavailable:

Không sellable nếu không đủ dữ liệu.

Không quote final price.

Không tạo order official.

Không paid.

Không verified revenue.

## 19.8. Gateway bị chặn

Không được mở Gateway trong P3.1.

## 20. EXECUTION STEPS

Dev/Codex/Copilot thực hiện theo thứ tự sau.

Step 1 - Confirm Mode

Xác nhận:

## MODE: TECHNICAL DESIGN ONLY - DO NOT MODIFY FILES

Step 2 - Read P3 Analysis Report

Đọc đầy đủ report từ 00-P3-ANALYSIS-ONLY.md.

Nếu chưa có report, dừng.

Step 3 - Map Workstreams

Map gap/điểm chặn thành workstream:

P3.2A Sellable.

P3.2B Price/Quote.

P3.2C Cart/Draft/Confirmation.

P3.2D Official Order.

P3.2E Payment/Shipping/Tax.

P3.2F Verified Revenue.

P3.2G Smoke/Evidence.

Step 4 - Design Domain Boundary

Xác định owner/consumer cho từng business object.

Step 5 - Design Contracts

Thiết kế:

Entity.

## DTO.

## API.

Service.

Resolver.

Guard.

State machine.

Event/outbox.

Audit/evidence.

Step 6 - Design Implementation Boundary

Mỗi prompt implementation sau phải có:

Allowed file change boundary.

Scope in.

Scope out.

Migration plan nếu cần.

Seed plan nếu cần.

Test plan.

Smoke plan.

Evidence plan.

Done gate.

Fail gate.

Step 7 - Design Smoke/Evidence

Thiết kế smoke cho từng workstream.

Step 8 - Produce Report

Trả report đúng 14 mục.

## 21. REQUIRED REPORT FORMAT - 14 MỤC

Dev/Codex/Copilot phải trả report đúng 14 mục.

## 21.1. Tóm tắt

Ghi rõ:

Đã thiết kế workstream nào.

P3.1 có đủ điều kiện chuyển P3.2A không.

điểm chặn còn lại.

Rủi ro P0/P1/P2.

## 21.2. File đã sửa

Vì mode là Technical Design Only, phải ghi:

Không sửa file. MODE: TECHNICAL DESIGN ONLY.

Nếu có file bị sửa, FAIL GATE.

## 21.3. Nguồn yêu cầu

Liệt kê:

## MASTER.

## PHASE 0.

## PHASE 1.

## PHASE 2.

P3 Analysis report.

Codebase evidence từ bước Analysis.

## 21.4. Evidence đã dùng

Ghi evidence đã dùng để thiết kế:

Current state matrix.

Gap matrix.

Conflict matrix.

điểm chặn list.

Code map.

API map.

DB map.

Integration map.

## 21.5. Lệnh đã chạy

Nếu có chạy lệnh read-only, ghi rõ.

Không chạy lệnh sửa file.

## 21.6. Kết quả test

Nếu không chạy test, ghi:

Chưa chạy test. P3.1 chỉ thiết kế test/smoke.

## 21.7. Kết quả backend build

Nếu không chạy build, ghi rõ.

Nếu có chạy build read-only, ghi kết quả.

## 21.8. Kết quả frontend build

Nếu không chạy build, ghi rõ.

Nếu có chạy build read-only, ghi kết quả.

## 21.9. Kết quả cleanup process

Không cleanup làm thay đổi repo.

Nếu có file tạm do tool tạo, phải báo.

## 21.10. Cập nhật Markdown

Không cập nhật Markdown trong P3.1.

Chỉ ghi đề xuất Markdown cần cập nhật ở phase sau.

## 21.11. Kết quả database migration/update nếu áp dụng

Không chạy migration.

Chỉ thiết kế migration plan.

## 21.12. Kết quả seed validation nếu áp dụng

Không sửa seed.

Chỉ thiết kế seed validation plan.

## 21.13. Rủi ro còn lại

Phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Owner decision needed.

Technical decision needed.

## 21.14. Cập nhật handoff

Ghi rõ:

Có được sang P3.2A không.

File tiếp theo.

Điều kiện trước khi implementation.

Các điểm chặn cần xử lý.

## 22. DONE GATE

P3.1 Technical Design được DONE khi đủ:

Không sửa file.

Không tạo migration.

Không sửa seed.

Không tạo test.

Không tạo API.

Không sửa frontend.

Thiết kế đủ workstream P3.2A -> P3.2G.

Thiết kế owner boundary.

Thiết kế domain object.

Thiết kế resolver/service.

Thiết kế API contract.

Thiết kế DB/entity ở mức đề xuất.

Thiết kế state machine.

Thiết kế RBAC/audit/idempotency/evidence.

Thiết kế smoke required.

Thiết kế evidence required.

Có handoff rõ sang 02-P3-2A-SELLABLE-GATE-RUNTIME.md.

Report đủ 14 mục.

Không gọi Gateway PASS.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 23. FAIL GATE

P3.1 bị FAIL nếu:

Có file bị sửa.

Có migration được tạo.

Có seed bị sửa.

Có test được thêm.

Có API được thêm.

Có frontend bị sửa.

Thiết kế cho phép Product Active thay Sellable.

Thiết kế cho phép SKU Active thay Sellable.

Thiết kế cho phép Recipe Active thay Sellable.

Thiết kế cho phép Warehouse Receipt tự set Sellable.

Thiết kế cho phép final price không cần QuoteSnapshot.

Thiết kế cho phép Cart là Order.

Thiết kế cho phép Order Draft là Official Order.

Thiết kế bỏ Customer Confirmation.

Thiết kế cho phép order không có order_code.

Thiết kế cho phép Payment selected là Paid.

Thiết kế tự thêm COD fee khi policy chưa khóa.

Thiết kế cho phép AI/Gateway hardcode giá/ship/VAT/discount.

Thiết kế cho phép revenue chưa verified đi vào commission/ROAS/payout.

Thiết kế mở Gateway.

Gọi Release Readiness/Production Readiness/Go-live Decision.

Report thiếu 14 mục.

## 24. DOWNSTREAM HANDOFF

## 24.1. File tiếp theo

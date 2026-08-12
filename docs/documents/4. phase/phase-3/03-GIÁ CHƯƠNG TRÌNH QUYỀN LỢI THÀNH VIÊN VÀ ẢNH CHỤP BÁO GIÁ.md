# P3.2B - PRICE PROGRAM MEMBER BENEFIT QUOTE SNAPSHOT

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- TECH-04 Commerce Runtime quote snapshot boundary.
- PACK-05 AI Advisor chi consume QuoteSnapshot sau khi runtime resolve.
- PACK-10 Evidence / Smoke / Completion Gateway.
- Final commission chi duoc xet sau verified revenue.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md

## 27.2. Prompt tiếp theo

## PROMPT-P3.2B - PRICE / PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / QUOTE SNAPSHOT LIMITED IMPLEMENTATION HANDOFF

## 27.3. Điều kiện sang P3.2B

Chỉ chuyển sang P3.2B nếu:

Sellable Gate Runtime đã đạt DONE GATE.

SellableDecision đã có block reason.

Sellable fail-safe khi thiếu runtime.

Product/SKU/Recipe Active không còn bị dùng thay Sellable.

Recall/Sale Lock chặn đúng.

Test/smoke P3.2A có evidence.

Owner đồng ý handoff.

## 27.4. Không chuyển nếu

Sellable Gate chưa xong.

Sellable còn dùng active flag đơn giản.

Chưa kiểm tra released stock.

Chưa kiểm tra recall/sale lock.

Chưa có runtime unavailable fail-safe.

Chưa có smoke/evidence.

Có scope creep sang QuoteSnapshot khi chưa được phép.

Có attempt mở Gateway.

## 28. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3.2A - SELLABLE GATE RUNTIME LIMITED IMPLEMENTATION HANDOFF

## MODE: LIMITED IMPLEMENTATION - SELLABLE GATE ONLY

Bạn đang thực hiện PHASE-03 - Commerce Runtime cho dự án Ginsengfood.

Nhiệm vụ duy nhất của bạn là triển khai hoặc chỉnh sửa Sellable Gate Runtime theo P3.1 Technical Design đã được owner duyệt.

Không được triển khai QuoteSnapshot.
Không được triển khai Cart.
Không được triển khai Order Draft.
Không được triển khai Official Order.
Không được triển khai Payment.
Không được triển khai Shipping.
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

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## CORE RULES

Sellable Gate là runtime gate riêng.

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Warehouse Receipt không tự set Sellable.

Sellable phải kiểm tra tối thiểu:

Product active.

SKU active.

Listed price active.

Stock available.

Finished goods released.

Warehouse receipt confirmed.

No recall.

No sale lock.

No quality hold.

No channel suppression.

Runtime policy available.

Runtime unavailable phải fail-safe NOT_SELLABLE.

AI/Gateway/Ads/Live/CRM/IVR chỉ consume sellable_status, không tự tính.

## SCOPE IN

Triển khai/chỉnh:

SellableDecision.

SellableStatus.

SellableConditionResult.

SellableBlockReason.

SellableGateResolver.

Guard chain:

RuntimePolicyAvailableGuard.

ProductActiveGuard.

SkuActiveGuard.

ListedPriceActiveGuard.

ReleasedStockGuard.

WarehouseConfirmedGuard.

RecallStopSaleGuard.

SaleLockGuard.

QualityHoldGuard.

ChannelSuppressionGuard.

QuantityAvailableGuard.

API/DTO Sellable nếu P3.1 đã duyệt.

Migration/seed nếu P3.1 đã duyệt.

Unit/integration test cho Sellable Gate.

Smoke/evidence report.

## SCOPE OUT

Không làm:

QuoteSnapshot.

Final price.

Program/member benefit.

Diamond referral.

Cart.

Order Draft.

Official Order.

Payment.

Shipping.

Tax.

Verified Revenue.

Commission.

## ROAS.

## IVR.

Gateway.

## ALLOWED FILE CHANGE BOUNDARY

Chỉ sửa file thuộc boundary đã được P3.1 duyệt.

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## EXECUTION STEPS

Xác nhận mode LIMITED IMPLEMENTATION - SELLABLE GATE ONLY.

Đọc P3.1 Technical Design.

Xác định allowed file boundary.

Inspect code hiện tại về Product/SKU/Recipe/Stock/Recall/Sale Lock.

Implement Sellable domain contract.

Implement Sellable guard chain.

Implement SellableGateResolver.

Implement API/DTO nếu đã duyệt.

Implement migration/seed nếu đã duyệt.

Add/update test.

Run backend build/test.

Run frontend build nếu có ảnh hưởng UI.

Run smoke P3-SMK-2A.

Produce report 14 mục.

Handoff sang P3.2B nếu đạt DONE GATE.

## TEST / SMOKE REQUIRED

Bắt buộc kiểm tra:

Product inactive -> NOT_SELLABLE.

SKU inactive -> NOT_SELLABLE.

Listed price missing/inactive -> NOT_SELLABLE.

No released stock -> NOT_SELLABLE.

Batch QC_PASS but not RELEASED -> NOT_SELLABLE.

Warehouse receipt not confirmed -> NOT_SELLABLE.

Recall active -> NOT_SELLABLE.

Sale lock active -> NOT_SELLABLE.

Quality hold active -> NOT_SELLABLE.

Channel suppression active -> NOT_SELLABLE.

Runtime policy unavailable -> NOT_SELLABLE.

All conditions pass -> SELLABLE.

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

Sellable Gate Runtime đúng rule.

Product/SKU/Recipe Active không bị dùng thay Sellable.

Warehouse Receipt không tự set Sellable.

Recall/Sale Lock chặn đúng.

Runtime unavailable fail-safe NOT_SELLABLE.

Có block reason.

Test/smoke pass hoặc điểm chặn báo rõ.

Backend build pass hoặc điểm chặn báo rõ.

Evidence submitted.

Report đủ 14 mục.

Gateway vẫn bị chặn.

Không gọi Release Readiness/Production Readiness.

FAIL GATE

Fail nếu:

Sửa ngoài boundary.

Tự triển khai QuoteSnapshot/Cart/Order/Payment/Revenue.

Product/SKU/Recipe Active vẫn thay Sellable.

Seed tự set sellable true.

Không kiểm tra released stock.

Không kiểm tra warehouse confirmed.

Không kiểm tra recall/sale lock.

Không fail-safe runtime unavailable.

AI/Gateway tự tính sellable.

Không có report 14 mục.

Mở Gateway.

Gọi Production Readiness/Go-live Decision.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_P3_2B_PRICE_PROGRAM_MEMBER_BENEFIT_QUOTE_SNAPSHOT.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_P3_1_APPROVAL.

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL.

## BLOCKED_TEST_OR_BUILD_FAILED.

## BLOCKED_SELLABLE_GATE_P0_RULE_FAILED.

NEXT FILE: PHASE-03-COMMERCE-RUNTIME/03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md

ĐIỀU KIỆN CHUYỂN TIẾP: Chỉ chuyển sang P3.2B sau khi Sellable Gate Runtime đạt DONE GATE, có test/smoke/evidence, không còn dùng Product/SKU/Recipe Active thay Sellable, Recall/Sale Lock chặn đúng, runtime unavailable fail-safe và owner đồng ý handoff.

PHASE-03-COMMERCE-RUNTIME/03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md

## PROMPT-P3.2B - PRICE / PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / QUOTE SNAPSHOT LIMITED IMPLEMENTATION HANDOFF

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt triển khai giới hạn cho nhóm Price / Program / Member Benefit / Diamond Referral / QuoteSnapshot trong PHASE-03 - Commerce Runtime.

Mục tiêu của prompt này là giao dev/Codex/Copilot triển khai hoặc chỉnh sửa đúng phạm vi phần báo giá runtime:

Listed Price Runtime.

Program Eligibility Runtime.

Member Benefit Eligibility Runtime.

Diamond Referral Eligibility Runtime.

QuoteSnapshot.

Final quote price tại thời điểm báo giá.

Quote freeze window.

Quote expiry.

Quote policy version.

No hardcode price/discount/member benefit/shipping/VAT trong AI/Gateway/content.

Đây là bước LIMITED IMPLEMENTATION.

Không được nhảy sang Cart.

Không được nhảy sang Order Draft.

Không được nhảy sang Customer Confirmation.

Không được nhảy sang Official Order.

Không được nhảy sang Payment.

Không được nhảy sang Shipping fulfillment.

Không được nhảy sang Verified Revenue.

Không được mở Gateway.

Không được gọi Production Readiness.

## 3. MODE

## MODE: LIMITED IMPLEMENTATION - PRICE / PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / QUOTE SNAPSHOT ONLY

Trong mode này dev/Codex/Copilot chỉ được phép sửa code trong phạm vi đã được P3.1 Technical Design và P3.2A handoff cho phép.

Được phép:

Thêm/sửa Listed Price runtime contract.

Thêm/sửa ListedPriceResolver.

Thêm/sửa ProgramEligibilityResolver.

Thêm/sửa MemberBenefitResolver.

Thêm/sửa DiamondReferralResolver.

Thêm/sửa QuoteSnapshot domain.

Thêm/sửa QuoteSnapshotService.

Thêm/sửa QuoteSnapshot API/DTO nếu đã được P3.1 duyệt.

Thêm migration phục vụ listed price / quote snapshot / quote line / policy version nếu được duyệt.

Thêm seed/config dev-test cho price/program/member/quote policy nếu được duyệt.

Thêm test/smoke cho giá và QuoteSnapshot.

Chạy backend build/test.

Chạy frontend build nếu có thay contract/UI.

Ghi report đủ 14 mục.

Không được:

Tạo Cart.

Tạo Order Draft.

Tạo Customer Confirmation.

Tạo Official Order.

Tạo order_code.

Set paid.

Tạo shipping order.

Tạo invoice thật.

Tạo verified revenue.

Tính commission final.

Tính verified ROAS.

Tạo payout.

Mở Gateway.

Cho AI/Gateway tự tính giá.

Cho content hardcode giá/discount/member benefit.

Gọi Release Readiness/Production Readiness/Go-live Decision.

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

Batch QC_PASS không đồng nghĩa RELEASED.

Warehouse chỉ nhận Batch RELEASED.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM, Gateway, IVR.

## 4.5. PHASE 3

Sellable Gate là runtime gate riêng.

QuoteSnapshot là nguồn sự thật cho giá cuối tại thời điểm báo giá.

Không có QuoteSnapshot thì không có final price.

Shipping fee/VAT/discount/member benefit phải được runtime tính.

Không hardcode trong AI/Gateway.

Commerce Runtime là owner của:

listed_price_status.

program eligibility.

member benefit eligibility.

diamond referral eligibility.

QuoteSnapshot.

final quote price.

AI Advisor, Gateway, Ads, Live, CRM, IVR chỉ consume runtime.

## 5. ENTRY CONDITION

Chỉ được thực hiện prompt này nếu đã hoàn tất:

## 00-P3-ANALYSIS-ONLY.md.

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md.

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md.

Sellable Gate Runtime đạt DONE GATE.

Sellable không còn dùng Product Active / SKU Active / Recipe Active thay thế.

Recall/Sale Lock chặn đúng.

Runtime unavailable fail-safe cho Sellable đã có.

Owner đồng ý handoff sang P3.2B.

Allowed file change boundary cho P3.2B đã rõ.

Nếu thiếu một trong các điều kiện trên, dev/Codex/Copilot phải dừng và trả trạng thái:

## BLOCKED_NEED_P3_2A_SELLABLE_DONE_GATE

## 6. OBJECTIVE

Mục tiêu của P3.2B là triển khai runtime báo giá chuẩn, trong đó:

Listed price phải được resolve từ Commerce Runtime.

Program eligibility phải được resolve từ Commerce Runtime.

Member benefit phải được resolve từ Commerce Runtime.

Diamond referral eligibility phải được resolve từ Commerce Runtime.

QuoteSnapshot phải được tạo tại thời điểm báo giá.

Final quote price chỉ được trả từ QuoteSnapshot.

QuoteSnapshot phải immutable sau khi tạo.

QuoteSnapshot phải có expiry/freeze window.

QuoteSnapshot phải lưu policy version/resolver version.

AI/Gateway/CRM/Live/Ads chỉ nhận kết quả quote từ runtime, không tự tính.

## 7. CORE RULES

## 7.1. Không có QuoteSnapshot thì không có final price

Không được trả cho khách hoặc channel các giá trị sau nếu chưa có QuoteSnapshot:

final price.

final payable amount.

total discount.

program discount.

member discount.

diamond benefit.

shipping fee trong bản báo giá cuối.

VAT/tax trong bản báo giá cuối.

tổng thanh toán cuối cùng.

## 7.2. Listed price không phải final price

Listed price chỉ là giá niêm yết.

Final price phải đi qua:

Sellable Gate.

Listed Price Resolver.

Program Eligibility Resolver.

Member Benefit Resolver.

Diamond Referral Resolver nếu có.

Shipping/Tax resolver hoặc placeholder policy theo boundary được duyệt.

QuoteSnapshot Service.

Audit/evidence/idempotency.

## 7.3. Program không được hardcode

Các chương trình như 24/7, Golden Hour hoặc chương trình tương lai không được hardcode trong AI/Gateway/content.

Program phải được runtime resolve theo:

program_id.

program_type.

active status.

effective time.

channel.

customer eligibility.

product/SKU eligibility.

policy priority.

conflict rule.

freeze window.

policy version.

## 7.4. Member benefit không được tự suy diễn

AI/Gateway/content không được tự suy diễn hạng thành viên hoặc quyền lợi.

Member benefit phải đi qua runtime:

customer identity.

member tier.

member cycle.

eligibility.

benefit rate/amount.

conflict with program.

policy version.

## 7.5. Diamond referral không đồng nghĩa commission final

Diamond referral eligibility trong quote chỉ là snapshot điều kiện giới thiệu tại thời điểm quote/order context.

Không được:

Tạo commission final trong P3.2B.

Tạo payout.

Tạo verified revenue.

Gọi ROAS verified.

Commission/ROAS/payout thuộc P3.2F và chỉ sau ORDER_VERIFIED hoặc equivalent verified checkpoint.

## 7.6. QuoteSnapshot phải immutable

Sau khi tạo QuoteSnapshot:

Không sửa line.

Không sửa giá.

Không sửa discount.

Không sửa policy version.

Không sửa final payable amount.

Không sửa expiry.

Nếu cần thay đổi, tạo QuoteSnapshot mới.

## 7.7. Runtime unavailable phải fail-safe

Nếu thiếu policy/resolver hoặc lỗi dependency:

Không tạo final quote.

Không trả final price.

Không cho channel tự tính thay.

Trả block reason nội bộ.

Public response phải an toàn, không gây hiểu nhầm.

## 8. SCOPE IN

P3.2B bao gồm:

ListedPriceDecision.

ListedPriceResolver.

ListedPriceStatus.

ProgramEligibilityDecision.

ProgramEligibilityResolver.

ProgramPriorityRule.

ProgramConflictRule.

MemberBenefitDecision.

MemberBenefitResolver.

DiamondReferralDecision.

DiamondReferralResolver.

QuoteSnapshot.

QuoteSnapshotLine.

QuoteAdjustmentSnapshot.

QuotePolicyVersion.

QuoteFreezeWindow.

QuoteExpiry.

QuoteSnapshotService.

QuoteSnapshot API/DTO nếu P3.1 duyệt.

Migration/seed/config nếu P3.1 duyệt.

Test/smoke/evidence cho QuoteSnapshot.

No-hardcode validation cho AI/Gateway/content nếu nằm trong boundary kiểm tra.

## 9. SCOPE OUT

P3.2B không bao gồm:

Không tạo Cart.

Không tạo Order Draft.

Không tạo Customer Confirmation.

Không tạo Official Order.

Không tạo order_code.

Không set Payment selected.

Không set Paid.

Không tạo shipment.

Không tạo invoice thật.

Không tạo verified revenue.

Không tạo commission final.

Không tạo verified ROAS.

Không tạo payout.

Không triển khai IVR.

Không mở Gateway.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

## 10. ALLOWED FILE CHANGE BOUNDARY

Dev/Codex/Copilot chỉ được sửa file thuộc boundary đã được P3.1/P3.2A handoff cho phép.

## 10.1. Nhóm file được phép sửa nếu đã duyệt

Nhóm file

Được sửa

Ghi chú

Commerce price domain

Có

Listed price / price status

Commerce program domain

Có

Program eligibility / priority / conflict

Commerce member benefit domain

Có

Member benefit snapshot

Commerce diamond referral domain

Có

Eligibility snapshot, không commission final

QuoteSnapshot domain

Có

Snapshot immutable

QuoteSnapshot service

Có

Tạo/read/expire/void nếu cần

Quote API/DTO

Có điều kiện

Chỉ nếu P3.1 duyệt

Migration

Có điều kiện

Chỉ cho price/program/member/quote snapshot

Seed/config dev-test

Có điều kiện

Không hardcode sellable/final price sai rule

Backend tests

Có

Chỉ P3.2B

Frontend/Admin UI

Có điều kiện

Chỉ viewer hoặc admin policy nếu đã duyệt

Markdown/handoff

Có điều kiện

Chỉ cập nhật P3.2B report

## 10.2. Nhóm file không được sửa

Nhóm file

Không được sửa

Lý do

Cart implementation

Không

Thuộc P3.2C

Order Draft implementation

Không

Thuộc P3.2C

Customer Confirmation implementation

Không

Thuộc P3.2C

Official Order implementation

Không

Thuộc P3.2D

Payment implementation

Không

Thuộc P3.2E

Shipping fulfillment implementation

Không

Thuộc P3.2E

Tax invoice issuance

Không

Thuộc P3.2E

Verified Revenue implementation

Không

Thuộc P3.2F

Commission/ROAS/Payout implementation

Không

Thuộc P3.2F

Gateway config

Không

IVR implementation

Không

PACK-09 reserved

Nếu cần sửa ngoài boundary, phải dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## 11. REQUIRED INPUT

## 11.1. Từ P3.2A

SellableGateResolver output.

SellableDecision contract.

Sellable status.

allow_quote.

block_reasons.

runtime policy status.

channel suppression status.

correlation_id.

## 11.2. Từ Product/SKU

product_id.

sku_id.

SKU active status.

public product name nếu cần hiển thị.

package unit nếu cần.

SKU channel eligibility nếu có.

## 11.3. Từ Price/Program/Member

Listed price records.

Program policy records.

Member tier records.

Customer identity/member context.

Diamond referral context.

Program schedule.

Policy version.

Conflict/priority rule.

## 11.4. Từ Shipping/Tax policy

P3.2B có thể chỉ consume placeholder/resolver contract nếu P3.2E chưa triển khai đầy đủ.

Tuy nhiên nếu QuoteSnapshot hiển thị tổng thanh toán cuối cùng, phải có runtime source cho:

shipping fee.

shipping discount/free ship nếu có.

VAT/tax nếu áp dụng.

tax policy version.

Nếu chưa có shipping/tax runtime thật, phải fail-safe hoặc đánh dấu quote incomplete theo design đã duyệt. Không được hardcode.

## 12. TARGET BEHAVIOR

## 12.1. Khi SKU không sellable

Nếu SellableGateResolver trả NOT_SELLABLE:

Không tạo final QuoteSnapshot.

Không trả final price.

Trả block reason.

Không cho AI/Gateway tự tính giá.

Không cho cart/order draft ở bước sau.

## 12.2. Khi thiếu listed price

Nếu thiếu listed price hoặc listed price inactive:

Không tạo final QuoteSnapshot.

Không trả final price.

Trả LISTED_PRICE_MISSING hoặc LISTED_PRICE_INACTIVE.

Không fallback sang giá hardcode.

## 12.3. Khi có program hợp lệ

Nếu có program hợp lệ:

Resolve program eligibility.

Resolve priority/conflict.

Ghi program snapshot vào QuoteSnapshot.

Ghi policy version.

Ghi expiry/freeze window.

Không để channel tự tính discount.

## 12.4. Khi có member benefit hợp lệ

Nếu customer có member context hợp lệ:

Resolve member tier.

Resolve eligibility.

Resolve benefit amount/rate.

Ghi member benefit snapshot.

Ghi policy version.

Không tự suy diễn trong AI/Gateway.

## 12.5. Khi có Diamond referral

Nếu có referral context:

Resolve bind hợp lệ.

Resolve referral owner.

Resolve referral owner tier nếu cần.

Resolve buyer eligibility.

Ghi DiamondReferralSnapshot.

Không tạo commission final.

Không payout.

## 12.6. Khi runtime policy unavailable

Nếu price/program/member/shipping/tax policy unavailable:

Không tạo final quote nếu policy đó bắt buộc.

Không trả final price.

Ghi block reason nội bộ.

Public response phải an toàn.

## 13. DOMAIN CONTRACT

## 13.1. ListedPriceDecision

Trường đề xuất:

listed_price_decision_id.

sku_id.

listed_price_id.

listed_price_status.

listed_price_amount.

currency.

effective_from.

effective_to nullable.

channel_scope.

policy_version.

resolved_at.

correlation_id.

block_reason nullable.

ListedPriceStatus

## ACTIVE.

## INACTIVE.

## EXPIRED.

## FUTURE_DATED.

## MISSING.

## BLOCKED_BY_POLICY.

## RUNTIME_UNAVAILABLE.

## 13.2. ProgramEligibilityDecision

Trường đề xuất:

program_decision_id.

program_id nullable.

program_type.

program_name internal.

sku_id.

customer_id nullable.

channel.

eligible.

in_time_window.

discount_type.

discount_rate nullable.

discount_amount nullable.

priority_rank.

conflict_resolution_result.

freeze_window_minutes.

effective_from.

effective_to.

policy_version.

resolved_at.

block_reason nullable.

ProgramType

## TWENTY_FOUR_SEVEN.

## GOLDEN_HOUR.

## MEMBER_CAMPAIGN.

## SKU_CAMPAIGN.

## CHANNEL_CAMPAIGN.

## OWNER_DEFINED_FUTURE_PROGRAM.

Không hardcode chỉ 2 chương trình; phải hỗ trợ chương trình tương lai.

## 13.3. MemberBenefitDecision

Trường đề xuất:

member_benefit_decision_id.

customer_id.

member_id nullable.

member_tier nullable.

member_cycle_status.

eligible.

benefit_type.

benefit_rate nullable.

benefit_amount nullable.

conflict_with_program_result.

policy_version.

resolved_at.

block_reason nullable.

MemberBenefitStatus

## ELIGIBLE.

## NOT_MEMBER.

## MEMBER_NOT_ELIGIBLE.

## CYCLE_EXPIRED.

## GRACE_PERIOD.

## CONFLICT_WITH_PROGRAM.

## RUNTIME_UNAVAILABLE.

## IDENTITY_NOT_RESOLVED.

## 13.4. DiamondReferralDecision

Trường đề xuất:

diamond_referral_decision_id.

referral_link_id nullable.

referral_owner_id nullable.

referral_owner_tier nullable.

buyer_customer_id nullable.

bind_status.

eligible.

self_purchase_excluded.

commission_candidate.

commission_final_allowed.

policy_version.

resolved_at.

block_reason nullable.

Rule bắt buộc:

commission_final_allowed = false trong P3.2B.

Commission final chỉ thuộc P3.2F sau verified revenue.

## 13.5. QuoteSnapshot

Trường bắt buộc đề xuất:

quote_snapshot_id.

quote_code optional.

customer_id nullable.

session_id.

channel.

evaluation_purpose.

quote_status.

currency.

subtotal_listed_amount.

total_program_discount_amount.

total_member_benefit_amount.

total_diamond_benefit_amount nếu có.

total_shipping_fee_amount.

total_shipping_discount_amount nếu có.

total_tax_amount.

final_payable_amount.

quote_freeze_window_type.

quote_freeze_minutes.

created_at.

expires_at.

policy_version.

resolver_version.

immutable_hash optional.

correlation_id.

actor_context nullable.

audit_ref.

evidence_ref optional.

QuoteSnapshotStatus

## CREATED.

## ACTIVE.

## EXPIRED.

## VOIDED.

## CONSUMED_BY_ORDER_DRAFT.

## CONSUMED_BY_OFFICIAL_ORDER.

P3.2B chỉ cần tạo/read/expire/void nếu được duyệt. Convert sang order draft thuộc P3.2C.

## 13.6. QuoteSnapshotLine

Trường đề xuất:

quote_snapshot_line_id.

quote_snapshot_id.

sku_id.

product_id.

product_public_name_snapshot.

quantity.

uom.

listed_unit_price.

listed_line_amount.

program_discount_amount.

member_benefit_amount.

diamond_benefit_amount.

line_shipping_fee_allocation nullable.

line_tax_amount nullable.

final_line_amount.

sellable_decision_id hoặc sellable_decision_ref.

listed_price_decision_ref.

program_decision_ref nullable.

member_benefit_decision_ref nullable.

diamond_referral_decision_ref nullable.

## 13.7. QuoteAdjustmentSnapshot

Trường đề xuất:

adjustment_id.

quote_snapshot_id.

adjustment_type.

source_policy.

amount.

rate nullable.

priority.

stackable.

conflict_resolution_result.

policy_version.

AdjustmentType:

## PROGRAM_DISCOUNT.

## MEMBER_BENEFIT.

## DIAMOND_BENEFIT.

## SHIPPING_FEE.

## SHIPPING_DISCOUNT.

## TAX.

MANUAL_OVERRIDE nếu policy cho phép và có audit riêng.

Manual override mặc định không cho phép nếu chưa có owner decision.

## 14. RESOLVER / SERVICE CONTRACT

## 14.1. ListedPriceResolver

Input:

sku_id.

channel.

request_time.

currency.

correlation_id.

Output:

ListedPriceDecision.

listed_price_amount.

listed_price_status.

policy_version.

block_reason.

Rule:

Missing price -> không quote final.

Inactive price -> không quote final.

Future price -> không quote final tại thời điểm hiện tại.

Expired price -> không quote final.

Không fallback hardcode.

## 14.2. ProgramEligibilityResolver

Input:

sku_id.

customer_context nullable.

channel.

request_time.

listed_price_decision.

quantity.

correlation_id.

Output:

ProgramEligibilityDecision.

eligible_program.

discount amount/rate.

priority result.

conflict result.

freeze window.

policy_version.

Rule:

Chỉ program active + đúng time window mới eligible.

Nếu nhiều program, resolve theo priority.

Nếu conflict, không tự stack khi policy chưa cho phép.

Golden Hour/24/7/future program phải đi qua cùng resolver.

## 14.3. MemberBenefitResolver

Input:

customer_context.

sku_id.

channel.

listed_price_decision.

program_decision.

request_time.

correlation_id.

Output:

MemberBenefitDecision.

eligible.

tier.

benefit rate/amount.

conflict result.

policy_version.

Rule:

Không có customer identity thì không tự suy diễn member.

Không tự hỏi "để kiểm tra" nếu runtime đã có context.

Nếu identity missing trong private flow, trả guard cần identity.

Member benefit không được hardcode trong AI/Gateway.

## 14.4. DiamondReferralResolver

Input:

customer_context.

referral_context nullable.

sku_id.

channel.

quote_context.

request_time.

correlation_id.

Output:

DiamondReferralDecision.

eligible.

bind_status.

owner tier.

self_purchase_excluded.

commission_candidate.

policy_version.

Rule:

Self-purchase không được tính referral commission.

P3.2B chỉ snapshot eligibility, chưa final commission.

Final commission phải đợi verified revenue ở P3.2F.

## 14.5. QuoteSnapshotService

Input:

sku lines.

channel.

customer_context nullable.

referral_context nullable.

shipping_context nullable.

tax_context nullable.

request_time.

correlation_id.

idempotency_key nếu create snapshot.

Output:

QuoteSnapshot.

QuoteSnapshotLines.

QuoteAdjustmentSnapshots.

expiry.

final_payable_amount.

policy_version.

audit_ref.

Rule:

Phải gọi SellableGateResolver trước khi quote.

Phải reject nếu SKU not sellable.

Phải reject nếu listed price missing/inactive.

Phải resolve program/member/diamond theo policy.

Phải lấy shipping/tax từ runtime policy hoặc fail-safe.

Phải immutable sau khi tạo.

Phải có idempotency nếu create command.

Không tạo Cart/Order.

## 15. QUOTE FREEZE / EXPIRY RULE

## 15.1. Freeze window

Freeze window phải do runtime policy quyết định.

Ví dụ policy có thể gồm:

Golden Hour: freeze 5 phút.

24/7: freeze 15 phút.

Future program: theo program policy.

Không hardcode trực tiếp trong AI/Gateway.

## 15.2. Expiry

QuoteSnapshot hết hạn khi:

expires_at < current_time.

policy void quote.

SKU bị recall/sale lock trước khi order draft nếu policy yêu cầu recheck.

quote bị void bởi admin/owner action có audit.

## 15.3. Sau expiry

Sau khi QuoteSnapshot expired:

Không được dùng để tạo Order Draft.

Không được dùng để Customer Confirmation.

Không được kéo sang Official Order.

Phải tạo QuoteSnapshot mới nếu khách muốn mua.

## 16. API / DTO CONTRACT - IF APPROVED BY P3.1

## 16.1. Create QuoteSnapshot API

POST /api/commerce/quote-snapshots

Request đề xuất:

channel.

customer_id nullable.

session_id.

lines:

sku_id.

quantity.

referral_context nullable.

shipping_context nullable.

tax_context nullable.

request_time optional.

idempotency_key.

correlation_id.

Response đề xuất:

quote_snapshot_id.

quote_status.

expires_at.

currency.

lines.

subtotal_listed_amount.

total_program_discount_amount.

total_member_benefit_amount.

total_shipping_fee_amount.

total_tax_amount.

final_payable_amount.

policy_version.

public_display_payload.

internal_decision_refs nếu internal/admin.

## 16.2. Read QuoteSnapshot API

GET /api/commerce/quote-snapshots/{id}

Response:

quote_snapshot_id.

quote_status.

quote lines.

adjustment snapshots.

final_payable_amount.

expires_at.

consumed status nếu có.

policy version.

## 16.3. Expire/Void QuoteSnapshot API

Chỉ nếu P3.1 duyệt.

POST /api/commerce/quote-snapshots/{id}/void

Yêu cầu:

backend permission.

reason.

actor context.

audit.

correlation id.

## 16.4. Public/internal response boundary

Public/channel response được phép có:

product public name.

quantity.

listed price.

program benefit display nếu policy cho phép.

member benefit display nếu customer context hợp lệ.

shipping fee.

VAT/tax nếu áp dụng.

final payable amount.

quote expiry time.

Không được lộ:

internal policy id nếu không public.

cost.

supplier info.

internal margin.

commission rule nội bộ.

internal audit note.

private member data ngoài quyền hiển thị.

hidden suppression reason.

## 17. DATABASE / MIGRATION RULE

Nếu P3.1 duyệt migration, các bảng đề xuất thuộc P3.2B có thể gồm:

commerce_listed_price.

commerce_program_policy.

commerce_member_benefit_policy.

commerce_diamond_referral_snapshot.

commerce_quote_snapshot.

commerce_quote_snapshot_line.

commerce_quote_adjustment_snapshot.

commerce_quote_policy_version.

commerce_quote_decision_log nếu cần.

Không tạo bảng:

commerce_cart.

commerce_order_draft.

commerce_order.

commerce_payment.

commerce_shipping_order.

commerce_verified_revenue.

commerce_commission_final.

commerce_roas_verified.

## 17.1. Migration safety

Migration phải:

Không phá dữ liệu cũ.

Không hardcode 20 SKU là giới hạn cố định.

Không tự set sellable.

Không seed final quote giả cho production.

Có index cho quote_snapshot_id, sku_id, customer_id/session_id, expires_at nếu cần.

Có audit/correlation fields theo convention.

Có immutable strategy nếu codebase hỗ trợ.

## 18. SEED / CONFIG RULE

Seed/config dev-test được phép tạo:

Listed price sample.

Program policy sample.

Member benefit policy sample.

Quote freeze window sample.

Tax/shipping placeholder policy nếu P3.1 duyệt.

Negative test fixtures.

Seed/config không được:

Tự set sellable true.

Bypass Sellable Gate.

Hardcode giá trong AI/Gateway.

Tạo order.

Tạo paid status.

Tạo verified revenue.

Tạo commission final.

Tạo payout.

Mở Gateway.

## 19. SECURITY / GOVERNANCE GUARDRAILS

## 19.1. RBAC

Bắt buộc backend permission cho:

Manage listed price.

Manage program policy.

Manage member benefit policy.

Void QuoteSnapshot.

View internal quote decision.

Export quote/audit evidence.

## 19.2. Actor Context

Bắt buộc cho:

price policy update.

program policy update.

member benefit policy update.

quote void.

manual override nếu sau này được duyệt.

## 19.3. Correlation ID

Mọi QuoteSnapshot phải có correlation_id.

## 19.4. Audit append-only

Audit bắt buộc cho:

price decision.

program decision.

member benefit decision.

diamond referral decision.

quote snapshot creation.

quote void/expire.

policy update.

## 19.5. Idempotency

Bắt buộc cho:

Create QuoteSnapshot.

Void QuoteSnapshot nếu là command.

Policy update nếu có.

## 19.6. Evidence Registry

Evidence submitted chưa phải accepted.

Không gọi Completion Decision nếu chưa owner review.

## 19.7. Runtime unavailable

Nếu resolver/policy unavailable:

Không final quote.

Không final price.

Không fallback hardcode.

Không để AI/Gateway tự tính.

## 20. TEST REQUIREMENTS

## 20.1. Unit test bắt buộc - Listed Price

Listed price active -> pass.

Listed price missing -> reject quote.

Listed price inactive -> reject quote.

Listed price expired -> reject quote.

Listed price future-dated -> reject quote.

Runtime price policy unavailable -> reject quote.

## 20.2. Unit test bắt buộc - Program

No active program -> quote dùng listed price + member/shipping/tax nếu có.

Active 24/7 eligible -> program discount snapshot.

Active Golden Hour eligible -> program discount snapshot.

Program ngoài time window -> not eligible.

Program conflict -> resolve theo priority.

Program không stack nếu policy không cho phép.

Program runtime unavailable -> fail-safe theo policy.

## 20.3. Unit test bắt buộc - Member Benefit

Customer not member -> no member benefit.

Member eligible -> member benefit snapshot.

Member cycle expired -> no benefit hoặc theo policy.

Member benefit conflict with program -> resolve theo policy.

Identity missing -> không tự suy diễn.

Member policy unavailable -> fail-safe.

## 20.4. Unit test bắt buộc - Diamond Referral

No referral bind -> no Diamond path.

Valid referral bind -> referral eligibility snapshot.

Referral owner not Diamond nếu policy yêu cầu -> not eligible.

Self-purchase -> excluded.

Valid referral vẫn không tạo commission final trong P3.2B.

Referral policy unavailable -> fail-safe.

## 20.5. Unit test bắt buộc - QuoteSnapshot

SKU not sellable -> reject quote.

Missing listed price -> reject quote.

QuoteSnapshot created with full line breakdown.

QuoteSnapshot final payable amount đúng.

QuoteSnapshot has expires_at.

QuoteSnapshot has policy version.

QuoteSnapshot immutable.

Expired quote cannot be used downstream.

Duplicate create with same idempotency key -> no duplicate snapshot hoặc trả same result theo convention.

No hardcoded final price.

## 20.6. Integration test nếu có API

Create quote success.

Create quote reject not sellable.

Create quote reject missing price.

Read quote returns immutable snapshot.

Expired quote status đúng.

Public response không lộ internal policy/cost/commission.

Internal response yêu cầu permission.

## 21. SMOKE REQUIREMENTS

## 21.1. P3-SMK-2B-001 - No QuoteSnapshot, no final price

Case: Request final price nhưng không tạo QuoteSnapshot.
Expected: Reject hoặc không trả final price.

## 21.2. P3-SMK-2B-002 - SKU not sellable

Case: SellableGate trả NOT_SELLABLE.
Expected: Không tạo QuoteSnapshot.

## 21.3. P3-SMK-2B-003 - Listed price missing

Case: SKU sellable nhưng thiếu listed price active.
Expected: Không tạo final quote.

## 21.4. P3-SMK-2B-004 - Program eligible

Case: SKU có program active hợp lệ.
Expected: QuoteSnapshot lưu program discount.

## 21.5. P3-SMK-2B-005 - Program conflict

Case: Nhiều program cùng eligible.
Expected: Resolve theo priority policy, không tự stack sai.

## 21.6. P3-SMK-2B-006 - Member benefit eligible

Case: Customer có member tier hợp lệ.
Expected: QuoteSnapshot lưu member benefit.

## 21.7. P3-SMK-2B-007 - Identity missing

Case: Không resolve được customer identity.
Expected: Không tự suy diễn member benefit.

## 21.8. P3-SMK-2B-008 - Diamond referral valid

Case: Referral bind hợp lệ.
Expected: QuoteSnapshot lưu Diamond referral eligibility, không tạo commission final.

## 21.9. P3-SMK-2B-009 - Quote expiry

Case: QuoteSnapshot expired.
Expected: Không cho dùng downstream.

## 21.10. P3-SMK-2B-010 - Immutable snapshot

Case: Thử sửa QuoteSnapshot sau khi tạo.
Expected: Không được mutate; nếu thay đổi phải tạo quote mới.

## 21.11. P3-SMK-2B-011 - AI/Gateway no hardcode

Case: Kiểm tra channel consumer.
Expected: AI/Gateway chỉ consume QuoteSnapshot, không tự tính final price.

## 21.12. P3-SMK-2B-012 - Runtime unavailable

Case: Price/program/member policy unavailable.
Expected: Không tạo final quote; fail-safe.

## 22. EVIDENCE REQUIREMENTS

P3.2B cần gom evidence:

Git diff trong allowed boundary.

File list đã sửa.

Migration diff nếu có.

Seed/config diff nếu có.

Unit test result.

Integration test result nếu có API.

Smoke result P3-SMK-2B-001 -> P3-SMK-2B-012.

Backend build result.

Frontend build result nếu có ảnh hưởng UI.

QuoteSnapshot sample evidence.

Immutable snapshot evidence.

Expiry evidence.

No-hardcode evidence.

Audit evidence.

RBAC evidence nếu có internal/admin endpoint.

Idempotency evidence.

Handoff note sang P3.2C.

Lưu ý:

Evidence submitted chưa phải Evidence accepted.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

## 23. EXECUTION STEPS

Step 1 - Confirm Mode

Xác nhận:

## MODE: LIMITED IMPLEMENTATION - PRICE / PROGRAM / MEMBER BENEFIT / DIAMOND REFERRAL / QUOTE SNAPSHOT ONLY

Step 2 - Read P3.2A Done Report

Xác nhận Sellable Gate đã đạt DONE GATE.

Nếu chưa đạt, dừng.

Step 3 - Inspect Existing Price/Program/Member Code

Kiểm tra:

Giá đang nằm ở đâu.

Có hardcode trong AI/Gateway/content không.

Có listed price status không.

Có program policy không.

Có member benefit logic không.

Có Diamond referral logic không.

Có quote hoặc invoice draft cũ không.

Có API trả final price không có QuoteSnapshot không.

Step 4 - Implement Listed Price Runtime

Triển khai/chỉnh:

ListedPriceDecision.

ListedPriceResolver.

ListedPriceStatus.

Missing/inactive/expired/future-dated handling.

Step 5 - Implement Program Runtime

Triển khai/chỉnh:

ProgramEligibilityDecision.

ProgramEligibilityResolver.

Program priority.

Program conflict.

Freeze window source.

Step 6 - Implement Member Benefit Runtime

Triển khai/chỉnh:

MemberBenefitDecision.

MemberBenefitResolver.

Identity guard.

Tier/cycle eligibility.

Conflict with program.

Step 7 - Implement Diamond Referral Runtime

Triển khai/chỉnh:

DiamondReferralDecision.

DiamondReferralResolver.

Bind status.

Self-purchase exclusion.

Commission candidate only, no final commission.

Step 8 - Implement QuoteSnapshot

Triển khai/chỉnh:

QuoteSnapshot.

QuoteSnapshotLine.

QuoteAdjustmentSnapshot.

QuoteSnapshotService.

Immutable behavior.

Expiry behavior.

Idempotency.

Step 9 - Implement API/DTO nếu approved

Nếu P3.1 duyệt, triển khai:

Create QuoteSnapshot API.

Read QuoteSnapshot API.

Void/Expire QuoteSnapshot API nếu có.

Nếu chưa duyệt, không tự tạo API.

Step 10 - Implement Migration/Seed nếu approved

Chỉ trong phạm vi P3.2B.

Không tạo cart/order/payment/revenue migration.

Step 11 - Add Tests

Thêm/cập nhật test theo mục Test Requirements.

Step 12 - Run Build/Test/Smoke

Chạy:

Backend build.

Backend test.

Integration test nếu có.

Frontend build nếu ảnh hưởng UI.

Smoke P3-SMK-2B.

Step 13 - Evidence Report

Gom evidence và trả report đúng 14 mục.

Step 14 - Handoff

Kết luận có được chuyển sang P3.2C hay không.

## 24. REQUIRED REPORT FORMAT - 14 MỤC

## 24.1. Tóm tắt

Ghi rõ:

Đã triển khai phần nào.

QuoteSnapshot hiện chạy ở đâu.

Listed price/program/member/diamond resolver nào đã có.

Test/smoke kết quả thế nào.

Có đủ điều kiện sang P3.2C không.

## 24.2. File đã sửa

Liệt kê đầy đủ:

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

Prompt P3.2B.

## 24.4. Evidence đã dùng

Liệt kê:

Code references.

DB/migration references.

Test references.

Smoke evidence.

Audit/evidence registry references nếu có.

API response/log nếu có.

## 24.5. Lệnh đã chạy

Ghi rõ từng lệnh:

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

## 24.12. Kết quả seed validation nếu áp dụng

Ghi:

Có seed không.

Seed có hardcode price sai không.

Seed có tự set sellable không.

Seed có tạo quote/order/revenue sai không.

Seed validation result.

## 24.13. Rủi ro còn lại

Phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Owner decision needed.

Deferred to P3.2C/P3.2E/P3.2F.

Technical debt.

## 24.14. Cập nhật handoff

Ghi rõ:

Có được sang P3.2C không.

Điều kiện còn thiếu nếu chưa.

File tiếp theo.

Handoff notes cho Cart/Order Draft/Customer Confirmation.

## 25. DONE GATE

P3.2B được DONE khi đủ:

Chỉ sửa file trong allowed boundary.

Sellable Gate từ P3.2A được dùng trước khi quote.

ListedPriceResolver có missing/inactive/expired handling.

ProgramEligibilityResolver có priority/conflict handling.

MemberBenefitResolver không tự suy diễn identity.

DiamondReferralResolver chỉ snapshot eligibility, không tạo commission final.

QuoteSnapshot được tạo khi quote hợp lệ.

Không có QuoteSnapshot thì không final price.

QuoteSnapshot immutable.

QuoteSnapshot có expires_at/freeze window.

QuoteSnapshot có policy version/resolver version.

Shipping/VAT nếu xuất hiện trong quote phải đến từ runtime/policy, không hardcode.

AI/Gateway không tự tính final price.

Public/internal response boundary đúng.

Idempotency cho create quote nếu có side effect.

Audit/correlation id đầy đủ.

Test bắt buộc pass hoặc điểm chặn rõ.

Smoke P3-SMK-2B pass hoặc điểm chặn rõ.

Backend build pass hoặc điểm chặn rõ.

Frontend build pass nếu có ảnh hưởng UI.

Evidence submitted.

Report đủ 14 mục.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 26. FAIL GATE

P3.2B bị FAIL nếu:

Sửa file ngoài allowed boundary.

Tự triển khai Cart.

Tự triển khai Order Draft.

Tự triển khai Official Order.

Tự triển khai Payment.

Tự triển khai Verified Revenue.

Trả final price không có QuoteSnapshot.

Listed price missing nhưng vẫn quote.

Program discount hardcode.

Member benefit hardcode.

Diamond referral tạo commission final.

QuoteSnapshot có thể mutate sau khi tạo.

Quote expired vẫn dùng downstream.

AI/Gateway tự tính giá/discount/member benefit.

Shipping/VAT bị hardcode trong quote.

Seed tự tạo order/revenue.

Không có test negative case.

Không có report 14 mục.

Mở Gateway.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

## 27. DOWNSTREAM HANDOFF

## 27.1. File tiếp theo

# P3.2A - SELLABLE GATE RUNTIME

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-01 Operational Core cho Sellable Gate runtime.
- TECH-03 Operational Core cho recall, sale lock va inventory truth.
- TECH-04 Commerce Runtime sellable decision boundary.
- Product Active khong dong nghia Sellable.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/02-P3-2A-SELLABLE-GATE-RUNTIME.md

## 24.2. Prompt tiếp theo

## PROMPT-P3.2A - SELLABLE GATE RUNTIME LIMITED IMPLEMENTATION HANDOFF

## 24.3. Điều kiện sang P3.2A

Chỉ chuyển sang P3.2A nếu:

P3.1 Technical Design report đủ 14 mục.

Owner đồng ý design boundary.

Sellable Gate owner đã rõ.

Dependency từ PHASE 1 và PHASE 2 đã rõ.

Allowed file change boundary cho P3.2A đã xác định.

Migration/seed/test plan cho P3.2A đã rõ.

Không còn xung đột Product/SKU/Recipe Active thay Sellable.

Gateway vẫn bị chặn.

## 24.4. Không chuyển nếu

Chưa có P3 Analysis report.

Chưa có P3.1 design report.

Chưa rõ owner sellable.

Chưa rõ stock source từ Operational Core.

Chưa rõ recall/sale lock source.

Chưa rõ listed price dependency.

Chưa rõ runtime policy unavailable behavior.

Có đề xuất mở Gateway.

## 25. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3.1 - COMMERCE RUNTIME TECHNICAL DESIGN / WORKSTREAM / TASK / CONTRACT / EVIDENCE / SMOKE HANDOFF

## MODE: TECHNICAL DESIGN ONLY - DO NOT MODIFY FILES

Bạn đang thực hiện PHASE-03 - COMMERCE RUNTIME cho dự án Ginsengfood.

Nhiệm vụ của bạn là thiết kế kỹ thuật dựa trên output của 00-P3-ANALYSIS-ONLY.md.

Không được sửa file.
Không được tạo migration.
Không được sửa seed.
Không được thêm test.
Không được thêm API.
Không được sửa frontend.
Không được mở Gateway.

## SOURCE-OF-TRUTH

Bạn phải bám theo:

MASTER Governance.

PHASE-00 Foundation/RBAC/Audit/Evidence/Idempotency.

PHASE-01 Product/SKU/Recipe/Activation.

PHASE-02 Operational Core.

PHASE-03/00-P3-ANALYSIS-ONLY report.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Documentation Complete chưa phải Production Readiness.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## OBJECTIVE

Thiết kế đầy đủ workstream, task, contract, smoke, evidence cho:

Sellable Gate Runtime.

Price / Program / Member Benefit / Diamond Referral / QuoteSnapshot.

Cart / Order Draft / Customer Confirmation.

Official Order / order_code / state machine.

Payment / Shipping / Invoice / Tax.

Verified Revenue / Commission / ROAS handoff.

Security / RBAC / Audit / Idempotency / Evidence.

Integration contract cho AI / Gateway / Ads / Live / CRM / IVR.

## CORE RULES

Sellable Gate là runtime gate riêng.

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Warehouse Receipt không tự set Sellable.

QuoteSnapshot là nguồn sự thật cho giá cuối.

Không có QuoteSnapshot thì không có final price.

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới cho phép Official Order.

Không có order_code thì không được nói đơn hàng đã ghi nhận chính thức.

Payment selected không đồng nghĩa Paid.

Payment Core confirmation mới là paid status.

COD không tự thêm phí nếu policy chưa khóa.

Shipping fee/VAT/discount/member benefit phải do runtime tính.

Verified Revenue chỉ sau ORDER_VERIFIED hoặc equivalent checkpoint.

No ORDER_VERIFIED thì không final commission.

No ORDER_VERIFIED thì không verified ROAS.

No ORDER_VERIFIED thì không payout.

AI/Gateway/Ads/Live/CRM/IVR chỉ consume runtime.

## REQUIRED DESIGN OUTPUT

Bạn phải thiết kế:

Owner boundary.

Domain object.

Entity/table proposal.

DTO proposal.

API contract proposal.

Resolver/service contract.

Guard list.

State machine.

RBAC requirement.

Audit requirement.

Idempotency requirement.

Evidence requirement.

Smoke requirement.

Implementation handoff cho P3.2A -> P3.2G.

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

Không sửa file.

Không tạo migration.

Không sửa seed.

Không thêm test.

Thiết kế đủ P3.2A -> P3.2G.

Có owner boundary.

Có API/DTO/entity/service/guard/state/evidence/smoke design.

Có handoff rõ sang P3.2A.

Report đủ 14 mục.

Không gọi Gateway PASS / Release Readiness / Production Readiness.

FAIL GATE

Fail nếu:

Có sửa file.

Có tạo migration/seed/test/API.

Cho phép Product/SKU/Recipe Active thay Sellable.

Cho phép final price không có QuoteSnapshot.

Cho phép Order Draft thành Official Order khi chưa Customer Confirmation.

Cho phép Payment selected thành Paid.

Cho phép revenue chưa verified vào commission/ROAS/payout.

Mở Gateway.

Gọi Production Readiness/Go-live Decision.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_P3_2A_SELLABLE_GATE_RUNTIME.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_P3_ANALYSIS_REPORT.

## BLOCKED_NEED_TECHNICAL_OWNER_DECISION.

## BLOCKED_NEED_CODEBASE_CLEANUP_BEFORE_IMPLEMENTATION.

NEXT FILE: PHASE-03-COMMERCE-RUNTIME/02-P3-2A-SELLABLE-GATE-RUNTIME.md

ĐIỀU KIỆN CHUYỂN TIẾP: Chỉ chuyển sang P3.2A sau khi dev/Codex/Copilot hoàn tất report P3.1 đủ 14 mục, không sửa file, thiết kế rõ Sellable Gate owner/boundary/dependency/guard/smoke/evidence và owner đồng ý cho bước implementation giới hạn.

PHASE-03-COMMERCE-RUNTIME/02-P3-2A-SELLABLE-GATE-RUNTIME.md

## PROMPT-P3.2A - SELLABLE GATE RUNTIME LIMITED IMPLEMENTATION HANDOFF

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt triển khai giới hạn cho Sellable Gate Runtime trong PHASE-03 - Commerce Runtime.

Mục tiêu là giao dev/Codex/Copilot triển khai hoặc chỉnh sửa đúng phạm vi cho Sellable Gate - lớp quyết định một SKU/sản phẩm có được phép bán tại thời điểm runtime hay không.

Đây là bước LIMITED IMPLEMENTATION, không phải làm toàn bộ Commerce Runtime.

Không được nhảy sang QuoteSnapshot.

Không được nhảy sang Cart.

Không được nhảy sang Order Draft.

Không được nhảy sang Official Order.

Không được nhảy sang Payment.

Không được nhảy sang Shipping.

Không được nhảy sang Verified Revenue.

Không được mở Gateway.

Không được gọi Production Readiness.

Phạm vi duy nhất của prompt này là:

Sellable Gate Runtime.

Sellable Decision Contract.

Sellable Guard Chain.

Sellable Block Reason.

Runtime policy unavailable fail-safe.

Read-only dependency từ Product/SKU/Recipe/Operational/Recall/Sale Lock/Quality/Channel Suppression.

Audit/evidence/idempotency nếu phát sinh side effect.

Test/smoke cục bộ cho Sellable Gate.

## 3. MODE

## MODE: LIMITED IMPLEMENTATION - SELLABLE GATE ONLY

Trong mode này dev/Codex/Copilot được phép sửa code chỉ trong phạm vi Sellable Gate Runtime đã được owner chấp thuận từ P3.1 Technical Design.

Được phép:

Thêm/sửa domain object cho Sellable Gate.

Thêm/sửa resolver/service cho Sellable Gate.

Thêm/sửa guard cho Sellable Gate.

Thêm/sửa DTO/API read-only hoặc decision endpoint cho Sellable nếu đã nằm trong P3.1 design.

Thêm migration nếu P3.1 đã xác định cần bảng/log/policy phục vụ Sellable Gate.

Thêm test cho Sellable Gate.

Thêm seed/config dev/test nếu P3.1 cho phép và không tự set sellable true sai rule.

Cập nhật markdown/handoff report liên quan Sellable Gate nếu cần.

Chạy backend build/test.

Chạy frontend build nếu có thay đổi contract ảnh hưởng UI.

Ghi report 14 mục.

Không được:

Triển khai QuoteSnapshot.

Triển khai final price.

Triển khai Cart.

Triển khai Order Draft.

Triển khai Official Order.

Triển khai Payment.

Triển khai Shipping fee.

Triển khai VAT/tax.

Triển khai Verified Revenue.

Triển khai commission.

Triển khai ROAS.

Triển khai IVR.

Mở Gateway.

Tự tính giá trong AI/Gateway.

Tự set SKU sellable bằng seed.

Dùng Product Active/SKU Active/Recipe Active thay Sellable.

Tự gọi Release Readiness/Production Readiness/Go-live Decision.

## 4. SOURCE-OF-TRUTH

Dev/Codex/Copilot phải bám theo các nguồn sau.

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

High-risk Command Guard.

Fail-safe runtime unavailable.

Không mở Global Gate.

## 4.3. PHASE 1

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Batch Released.

Seed không được tự set sellable.

Product/SKU/Recipe/Formula/BOM chỉ là foundation.

## 4.4. PHASE 2

Batch QC_PASS không đồng nghĩa RELEASED.

Warehouse chỉ nhận Batch RELEASED.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Warehouse Receipt không tự set Sellable.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM, Gateway, IVR.

QR VOID/FAILED không public-valid.

Public Trace whitelist-only.

## 4.5. PHASE 3

Sellable Gate là runtime gate riêng.

Sellable phụ thuộc tối thiểu:

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

Commerce Runtime là owner của sellable_status.

AI/Gateway/Ads/Live/CRM/IVR chỉ consume sellable_status.

## 5. ENTRY CONDITION

Chỉ được thực hiện prompt này nếu đã hoàn tất:

## 00-P3-ANALYSIS-ONLY.md.

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md.

P3.1 report đủ 14 mục.

Owner đồng ý cho triển khai giới hạn P3.2A.

Allowed file change boundary đã được xác định.

Dependency từ Product/SKU/Recipe/Operational Core/Recall/Sale Lock đã map rõ.

Không còn tranh chấp về nguyên tắc: Product Active/SKU Active/Recipe Active không phải Sellable.

Nếu thiếu một trong các điều kiện trên, dev/Codex/Copilot phải dừng và trả trạng thái:

## BLOCKED_NEED_P3_1_TECHNICAL_DESIGN_APPROVAL

## 6. OBJECTIVE

Mục tiêu của P3.2A là triển khai Sellable Gate Runtime để xác định chính xác một SKU có được phép bán tại thời điểm runtime hay không.

Sellable Gate phải trả lời được:

SKU này có được bán không?

Nếu không bán được, bị chặn vì lý do gì?

Có cho phép báo giá không?

Có cho phép thêm vào cart không?

Có cho phép tạo order draft không?

Có cho phép kênh AI/Gateway/CRM/Live/Ads hiển thị là bán được không?

Runtime policy đang dùng là version nào?

Quyết định sellable được audit/evidence như thế nào?

Khi policy/resolver lỗi thì fail-safe ra sao?

## 7. CORE RULES

## 7.1. Sellable không phải Active

Không được viết logic theo kiểu:

Product active là bán được.

SKU active là bán được.

Recipe active là bán được.

Batch QC_PASS là bán được.

Warehouse receipt là bán được.

Có stock là bán được nhưng không kiểm tra release/lock.

Có giá là bán được nhưng không kiểm tra stock/recall/sale lock.

Sellable là runtime decision riêng.

## 7.2. Sellable là kết quả tổng hợp nhiều điều kiện

Sellable chỉ được true nếu tất cả điều kiện bắt buộc đều pass.

Nếu một điều kiện P0 fail, sellable phải false.

## 7.3. Fail-safe khi thiếu runtime

Nếu thiếu runtime policy, thiếu dependency hoặc resolver lỗi, hệ thống phải fail-safe:

Không được tự giả định sellable.

Không được cho quote final.

Không được cho cart/order draft.

Không được trả trạng thái gây hiểu nhầm là đang bán được.

## 7.4. Commerce Runtime là owner

Sellable status phải do Commerce Runtime quyết định.

AI Advisor, Gateway, Ads, Live, CRM, IVR không được tự tính sellable.

## 7.5. Recall/Sale Lock thắng tất cả

Nếu có recall hoặc sale lock hợp lệ:

Sellable phải false.

AI không được tư vấn chốt đơn SKU đó.

Gateway không được đẩy đơn.

CRM không được gợi ý mua lại SKU đó.

Ads không được dùng SKU đó cho chiến dịch bán hàng nếu suppression áp dụng.

Live không được chốt SKU đó.

IVR không được xác nhận tiếp đơn mới cho SKU đó nếu rule chặn.

## 8. SCOPE IN

P3.2A bao gồm:

SellableDecision domain object.

SellableConditionResult.

SellableBlockReason.

SellableGateResolver.

ProductActiveCondition.

SkuActiveCondition.

ListedPriceActiveCondition.

ReleasedStockCondition.

WarehouseConfirmedStockCondition.

RecallStopSaleCondition.

SaleLockCondition.

QualityHoldCondition.

ChannelSuppressionCondition.

RuntimePolicyAvailableCondition.

Runtime fail-safe behavior.

Sellable API/DTO nếu P3.1 cho phép.

Audit/evidence hooks cho Sellable Decision nếu P3.1 xác định cần.

Test/smoke cho Sellable Gate.

Report 14 mục.

## 9. SCOPE OUT

P3.2A không bao gồm:

Không triển khai QuoteSnapshot.

Không tính final price.

Không triển khai listed price management đầy đủ.

Không triển khai program eligibility đầy đủ.

Không triển khai member benefit.

Không triển khai diamond referral.

Không triển khai cart.

Không triển khai order draft.

Không triển khai customer confirmation.

Không triển khai official order.

Không triển khai order_code.

Không triển khai payment.

Không triển khai shipping fee.

Không triển khai VAT/tax.

Không triển khai verified revenue.

Không triển khai commission.

Không triển khai ROAS.

Không triển khai payout.

Không triển khai IVR.

Không mở Gateway.

Không gọi Release Readiness.

Không gọi Production Readiness.

## 10. ALLOWED FILE CHANGE BOUNDARY

Dev/Codex/Copilot chỉ được sửa các file thuộc phạm vi đã được P3.1 Technical Design xác định.

## 10.1. Nhóm file được phép sửa nếu có trong codebase

Nhóm file

Được sửa

Ghi chú

Commerce Runtime domain

Có

Chỉ SellableDecision/SellableCondition

Commerce Runtime service/resolver

Có

Chỉ SellableGateResolver

Commerce Runtime guard

Có

Chỉ guard phục vụ sellable

Commerce API/DTO

Có điều kiện

Chỉ nếu P3.1 đã duyệt endpoint/DTO sellable

Migration

Có điều kiện

Chỉ nếu P3.1 duyệt bảng/log/policy cần thiết

Seed/config dev-test

Có điều kiện

Không được tự set sellable true sai rule

Backend tests

Có

Chỉ test Sellable Gate

Frontend/Admin UI

Không mặc định

Chỉ sửa nếu P3.1 ghi rõ cần hiển thị Sellable Decision

Docs/handoff markdown

Có điều kiện

Chỉ cập nhật report/handoff liên quan P3.2A

## 10.2. Nhóm file không được sửa

Nhóm file

Không được sửa

Lý do

QuoteSnapshot implementation

Không

Thuộc P3.2B

Cart implementation

Không

Thuộc P3.2C

Order Draft implementation

Không

Thuộc P3.2C

Official Order implementation

Không

Thuộc P3.2D

Payment implementation

Không

Thuộc P3.2E

Shipping/Tax implementation

Không

Thuộc P3.2E

Verified Revenue implementation

Không

Thuộc P3.2F

Commission/ROAS implementation

Không

Thuộc P3.2F

Gateway config

Không

Gateway bị chặn

IVR implementation

Không

PACK-09 reserved

Nếu cần sửa ngoài boundary, dev/Codex/Copilot phải dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## 11. REQUIRED INPUT

Dev/Codex/Copilot phải đọc các input sau trước khi sửa code.

## 11.1. Từ P3.1 Technical Design

Sellable owner boundary.

Approved file change boundary.

Proposed domain object.

Proposed resolver/service.

Proposed guard chain.

Proposed API/DTO.

Proposed migration/seed plan.

Proposed smoke/test plan.

Evidence required.

## 11.2. Từ codebase thật

Product entity/service.

SKU entity/service.

Recipe/formula entity/service.

Listed price source hiện có nếu có.

Inventory/warehouse stock projection.

Batch release state.

Warehouse receipt confirmed state.

Recall state.

Sale lock state.

Quality hold state.

Channel suppression config nếu có.

Runtime policy/config nếu có.

Existing API routes.

Existing tests.

Existing audit/evidence/idempotency utilities.

## 11.3. Từ governance

## RBAC.

Audit.

Evidence.

Idempotency.

Correlation ID.

Runtime unavailable fail-safe.

Gateway bị chặn.

## 12. TARGET BEHAVIOR

Sellable Gate Runtime phải cho ra một quyết định rõ ràng.

## 12.1. Sellable true khi

Sellable chỉ được true khi:

Product active.

SKU active.

Listed price active.

Có tồn finished goods khả dụng.

Finished goods đến từ batch RELEASED.

Warehouse receipt đã confirmed.

Không recall.

Không sale lock.

Không quality hold.

Không channel suppression cho channel hiện tại.

Runtime policy available.

Không lỗi resolver.

Không có guard P0 fail.

## 12.2. Sellable false khi

Sellable phải false nếu:

Product inactive.

SKU inactive.

Listed price inactive hoặc missing.

Không có finished goods stock available.

Stock chưa đến từ warehouse receipt confirmed.

Batch chỉ QC_PASS nhưng chưa RELEASED.

Có recall active.

Có sale lock active.

Có quality hold active.

Có channel suppression.

Runtime policy unavailable.

Resolver lỗi.

Dependency source không xác định.

Data conflict giữa stock/release/warehouse.

Không resolve được required condition.

## 12.3. Output phải giải thích được

SellableDecision phải có block reasons rõ ràng để:

Internal admin hiểu lý do.

AI/Gateway nhận public-safe reason nếu cần.

CRM/Ads/Live biết không được chốt SKU.

Dev/debug truy vết được.

Evidence có thể chứng minh logic đúng.

## 13. DOMAIN CONTRACT

## 13.1. SellableDecision

SellableDecision đề xuất gồm:

decision_id.

sku_id.

product_id.

channel.

requested_quantity.

sellable_status.

allow_quote.

allow_cart.

allow_order_draft.

block_reasons.

condition_results.

stock_available_quantity.

listed_price_status.

recall_status.

sale_lock_status.

quality_hold_status.

channel_suppression_status.

runtime_policy_version.

evaluated_at.

correlation_id.

actor_context nếu có.

audit_ref nếu có.

evidence_ref nếu có.

## 13.2. SellableStatus

SellableStatus đề xuất:

## SELLABLE.

## NOT_SELLABLE.

## BLOCKED_BY_POLICY.

## BLOCKED_BY_STOCK.

## BLOCKED_BY_RECALL.

## BLOCKED_BY_SALE_LOCK.

## BLOCKED_BY_QUALITY_HOLD.

## BLOCKED_BY_CHANNEL_SUPPRESSION.

## BLOCKED_RUNTIME_UNAVAILABLE.

## BLOCKED_DATA_CONFLICT.

Tên enum có thể điều chỉnh theo convention codebase, nhưng ý nghĩa không được làm sai.

## 13.3. SellableConditionResult

Mỗi condition phải trả:

condition_code.

condition_name.

passed.

severity.

block_reason_code nếu failed.

internal_message.

public_safe_message optional.

source_module.

source_record_id optional.

evaluated_at.

## 13.4. SellableBlockReason

Block reason bắt buộc tối thiểu:

## PRODUCT_INACTIVE.

## SKU_INACTIVE.

## LISTED_PRICE_MISSING.

## LISTED_PRICE_INACTIVE.

## NO_AVAILABLE_STOCK.

## BATCH_NOT_RELEASED.

## WAREHOUSE_RECEIPT_NOT_CONFIRMED.

## RECALL_ACTIVE.

## SALE_LOCK_ACTIVE.

## QUALITY_HOLD_ACTIVE.

## CHANNEL_SUPPRESSED.

## RUNTIME_POLICY_UNAVAILABLE.

## DEPENDENCY_UNAVAILABLE.

## DATA_CONFLICT.

## UNKNOWN_ERROR_FAIL_SAFE.

## 14. RESOLVER CONTRACT

## 14.1. SellableGateResolver

Input:

sku_id.

channel.

requested_quantity nullable.

customer_context nullable.

request_time.

correlation_id.

actor_context nullable.

evaluation_purpose:

## VIEW.

## QUOTE.

## CART.

## ORDER_DRAFT.

## CRM.

## ADS.

## LIVE.

## GATEWAY.

Output:

SellableDecision.

sellable_status.

allow_quote.

allow_cart.

allow_order_draft.

public_safe_block_reason optional.

internal_block_reasons.

condition_results.

evaluated_policy_version.

evaluated_at.

audit_ref optional.

evidence_ref optional.

## 14.2. Resolver rule

Resolver phải chạy condition theo thứ tự an toàn:

RuntimePolicyAvailableCondition.

ProductActiveCondition.

SkuActiveCondition.

ListedPriceActiveCondition.

ReleasedStockCondition.

WarehouseConfirmedStockCondition.

RecallStopSaleCondition.

SaleLockCondition.

QualityHoldCondition.

ChannelSuppressionCondition.

QuantityAvailableCondition.

FinalDecisionAggregation.

Nếu một condition P0 fail, decision cuối cùng phải NOT_SELLABLE.

## 14.3. Resolver không được làm

SellableGateResolver không được:

Tính final price.

Tạo QuoteSnapshot.

Tạo Cart.

Tạo Order Draft.

Tạo Official Order.

Set paid.

Tạo verified revenue.

Gọi trực tiếp Gateway.

Gọi trực tiếp Ads.

Gọi trực tiếp MISA.

## 15. GUARD CHAIN

## 15.1. RuntimePolicyAvailableGuard

Chặn nếu runtime policy không load được.

Fail-safe:

## NOT_SELLABLE / BLOCKED_RUNTIME_UNAVAILABLE

## 15.2. ProductActiveGuard

Chặn nếu product inactive.

Không được tự activate product.

## 15.3. SkuActiveGuard

Chặn nếu SKU inactive.

Không được tự activate SKU.

## 15.4. ListedPriceActiveGuard

Chặn nếu không có listed price active.

Trong P3.2A chỉ kiểm tra trạng thái listed price, chưa tính final price.

Nếu listed price module chưa có, phải tạo adapter/stub theo P3.1 design hoặc fail-safe, không tự hardcode giá.

## 15.5. ReleasedStockGuard

Chặn nếu không có finished goods stock từ batch RELEASED.

Batch QC_PASS chưa đủ.

## 15.6. WarehouseConfirmedGuard

Chặn nếu tồn chưa đến từ warehouse receipt confirmed.

Warehouse receipt không tự set sellable, chỉ là một điều kiện nguồn cho sellable.

## 15.7. RecallStopSaleGuard

Chặn nếu có recall active.

Recall thắng Commerce và tất cả channel.

## 15.8. SaleLockGuard

Chặn nếu có sale lock active.

Sale Lock thắng AI/Gateway/CRM/Ads/Live/IVR.

## 15.9. QualityHoldGuard

Chặn nếu SKU/batch/lot có quality hold ảnh hưởng bán hàng.

## 15.10. ChannelSuppressionGuard

Chặn nếu SKU/sản phẩm bị chặn theo channel.

Ví dụ:

Không bán trên live.

Không bán trên ads.

Không bán trên gateway.

Không bán cho CRM campaign.

Không bán theo khu vực/channel policy.

## 15.11. QuantityAvailableGuard

Chặn nếu requested_quantity vượt stock available theo rule.

Nếu requested_quantity null, chỉ kiểm tra có available stock > 0 hoặc theo policy đã khóa.

## 16. API / DTO CONTRACT - IF APPROVED BY P3.1

Nếu P3.1 đã duyệt API Sellable, dev/Codex/Copilot có thể triển khai hoặc chỉnh trong phạm vi sau.

## 16.1. API đề xuất

GET /api/commerce/sellable

Query:

sku_id.

channel.

requested_quantity optional.

evaluation_purpose optional.

Response:

sku_id.

channel.

sellable_status.

allow_quote.

allow_cart.

allow_order_draft.

stock_available_quantity optional.

public_safe_block_reason optional.

internal_block_reasons nếu internal/admin.

policy_version.

evaluated_at.

correlation_id.

## 16.2. Internal vs public response

Public/channel response không được lộ:

Supplier internal data.

Batch internal defect.

QC internal notes.

Costing.

Personnel.

Private recall notes.

Internal stock exact quantity nếu policy không cho phép.

Internal/admin response có thể hiển thị nhiều hơn nếu có permission.

## 16.3. API không được trả

API Sellable không được trả:

final price.

program discount.

member discount.

shipping fee.

## VAT.

order_code.

paid status.

verified revenue.

Những phần đó thuộc các prompt sau.

## 17. DATABASE / MIGRATION RULE

Nếu P3.1 xác định cần migration cho Sellable Gate, migration chỉ được phục vụ:

Sellable decision log.

Sellable policy version.

Channel suppression config.

Block reason registry.

Audit reference linkage.

Evidence reference linkage.

Không được tạo migration cho:

QuoteSnapshot.

Cart.

Order Draft.

Official Order.

Payment.

Shipping fee.

VAT/tax.

Verified Revenue.

Commission.

## ROAS.

## 17.1. Migration safety

Migration phải:

Không phá dữ liệu cũ.

Có index cần thiết.

Có constraint hợp lý.

Có rollback hoặc down migration nếu convention yêu cầu.

Không tự set sellable true hàng loạt.

Không hardcode SKU.

Không hardcode danh sách 20 SKU.

Hỗ trợ mở rộng 40-50 SKU hoặc hơn.

## 18. SEED / CONFIG RULE

Nếu cần seed/config cho Sellable Gate:

Seed được phép:

Tạo policy version dev/test.

Tạo channel suppression sample.

Tạo block reason registry nếu cần.

Tạo negative test fixture.

Seed không được:

Tự set sellable true.

Bypass Product/SKU active.

Bypass listed price.

Bypass stock.

Bypass warehouse confirmed.

Bypass recall/sale lock.

Hardcode 20 SKU là giới hạn cố định.

Tạo order.

Tạo quote.

Tạo verified revenue.

## 19. SECURITY / GOVERNANCE GUARDRAILS

## 19.1. RBAC

Nếu Sellable API có internal/admin details, phải enforce backend RBAC.

Không được chỉ che dữ liệu ở frontend.

## 19.2. Actor Context

Nếu có action quản trị channel suppression/policy, phải có actor context.

## 19.3. Correlation ID

Mọi SellableDecision phải có correlation_id.

Nếu request không có correlation_id, hệ thống phải tạo theo convention hiện tại.

## 19.4. Audit append-only

Nếu decision được lưu hoặc policy thay đổi, audit phải append-only.

Không được sửa/xóa audit.

## 19.5. Evidence Registry

Smoke evidence chỉ là submitted evidence, chưa phải accepted evidence.

## 19.6. Idempotency

Sellable check read-only thường không cần idempotency.

Nhưng nếu SellableDecision được lưu thành decision log hoặc có side effect, phải tuân thủ idempotency theo P3.1 design.

## 19.7. Runtime unavailable

Nếu dependency unavailable:

Return NOT_SELLABLE.

Block quote/cart/order draft.

Ghi internal block reason.

Không trả thông điệp gây hiểu nhầm là còn hàng/bán được.

## 20. TEST REQUIREMENTS

Dev/Codex/Copilot phải thêm hoặc cập nhật test cho Sellable Gate theo boundary được duyệt.

## 20.1. Unit test bắt buộc

Product inactive -> NOT_SELLABLE.

SKU inactive -> NOT_SELLABLE.

Listed price missing -> NOT_SELLABLE.

Listed price inactive -> NOT_SELLABLE.

No available stock -> NOT_SELLABLE.

Batch QC_PASS but not RELEASED -> NOT_SELLABLE.

Warehouse receipt not confirmed -> NOT_SELLABLE.

Recall active -> NOT_SELLABLE.

Sale lock active -> NOT_SELLABLE.

Quality hold active -> NOT_SELLABLE.

Channel suppressed -> NOT_SELLABLE.

Runtime policy unavailable -> NOT_SELLABLE.

Dependency error -> NOT_SELLABLE.

All conditions pass -> SELLABLE.

## 20.2. Integration test nếu có API

Sellable API trả NOT_SELLABLE khi thiếu listed price.

Sellable API trả NOT_SELLABLE khi recall active.

Sellable API trả SELLABLE khi đủ điều kiện.

Public response không lộ internal reason.

Internal/admin response có reason nếu đủ permission.

Correlation ID xuất hiện trong response/log nếu convention hỗ trợ.

## 20.3. Regression test

Product Active không tự mở bán.

SKU Active không tự mở bán.

Recipe Active không tự mở bán.

Warehouse Receipt không tự set Sellable.

Seed không tự set Sellable.

Sale Lock thắng Sellable.

Recall thắng Sellable.

## 21. SMOKE REQUIREMENTS

Sau khi implementation xong, dev/Codex/Copilot phải chạy smoke cục bộ cho Sellable Gate.

## 21.1. Smoke case P3-SMK-2A-001

Case: Product active nhưng listed price missing.
Expected: NOT_SELLABLE.
Không được: Quote/cart/order draft allowed.

## 21.2. Smoke case P3-SMK-2A-002

Case: SKU active nhưng no available released stock.
Expected: NOT_SELLABLE.

## 21.3. Smoke case P3-SMK-2A-003

Case: Batch QC_PASS nhưng chưa RELEASED.
Expected: NOT_SELLABLE.

## 21.4. Smoke case P3-SMK-2A-004

Case: Warehouse receipt chưa confirmed.
Expected: NOT_SELLABLE.

## 21.5. Smoke case P3-SMK-2A-005

Case: Recall active.
Expected: NOT_SELLABLE / BLOCKED_BY_RECALL.

## 21.6. Smoke case P3-SMK-2A-006

Case: Sale lock active.
Expected: NOT_SELLABLE / BLOCKED_BY_SALE_LOCK.

## 21.7. Smoke case P3-SMK-2A-007

Case: Channel suppression active cho Gateway hoặc Live.
Expected: NOT_SELLABLE cho channel đó.

## 21.8. Smoke case P3-SMK-2A-008

Case: Runtime policy unavailable.
Expected: NOT_SELLABLE / BLOCKED_RUNTIME_UNAVAILABLE.

## 21.9. Smoke case P3-SMK-2A-009

Case: All conditions pass.
Expected: SELLABLE, allow_quote true, allow_cart true, allow_order_draft true.

## 21.10. Smoke case P3-SMK-2A-010

Case: AI/Gateway consumer gọi sellable endpoint hoặc service.
Expected: Chỉ nhận runtime result, không tự tính sellable.

## 22. EVIDENCE REQUIREMENTS

P3.2A cần gom evidence sau:

Git diff chỉ trong allowed boundary.

File list đã sửa.

Migration diff nếu có.

Seed/config diff nếu có.

Unit test result.

Integration test result nếu có API.

Smoke result P3-SMK-2A-001 -> P3-SMK-2A-010.

Backend build result.

Frontend build result nếu có ảnh hưởng UI.

Audit evidence nếu có decision log/policy update.

RBAC evidence nếu có internal/admin response.

Runtime unavailable evidence.

Public/internal response boundary evidence.

Handoff note sang P3.2B.

Lưu ý:

Evidence submitted chưa phải Evidence accepted.

Không được gọi Completion Decision.

Không được gọi Release Readiness.

## 23. EXECUTION STEPS

Dev/Codex/Copilot thực hiện theo thứ tự sau.

Step 1 - Confirm Mode

Xác nhận:

## MODE: LIMITED IMPLEMENTATION - SELLABLE GATE ONLY

Step 2 - Read P3.1 Design

Đọc P3.1 report và xác định allowed file boundary.

Nếu chưa có boundary, dừng.

Step 3 - Inspect Existing Code

Tìm các điểm hiện đang xác định bán được:

Product active.

SKU active.

Recipe active.

Inventory available.

Warehouse receipt.

Price active.

AI/Gateway sellable/availability logic.

Admin UI sellable toggle nếu có.

Nếu phát hiện đang dùng active thay sellable, ghi lại và sửa trong boundary được phép.

Step 4 - Implement Domain Contract

Triển khai hoặc chỉnh:

SellableDecision.

SellableStatus.

SellableConditionResult.

SellableBlockReason.

Theo convention codebase thật.

Step 5 - Implement Guard Chain

Triển khai hoặc chỉnh:

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

Step 6 - Implement SellableGateResolver

Resolver phải aggregate condition results và trả SellableDecision.

Không được tính final price.

Không được tạo QuoteSnapshot.

Step 7 - Implement API/DTO nếu approved

Nếu P3.1 đã duyệt, triển khai endpoint/DTO Sellable.

Nếu chưa duyệt, không tự tạo API.

Step 8 - Implement Migration/Seed nếu approved

Nếu P3.1 đã duyệt migration/seed, triển khai đúng scope.

Nếu chưa duyệt, không tự tạo migration/seed.

Step 9 - Add Tests

Thêm/cập nhật test theo mục Test Requirements.

Step 10 - Run Build/Test

Chạy:

Backend format/lint nếu project có.

Backend build.

Backend test liên quan.

Frontend build nếu có thay UI/contract.

Smoke cục bộ P3-SMK-2A.

Step 11 - Evidence Report

Gom evidence và viết report 14 mục.

Step 12 - Handoff

Kết luận có được chuyển sang P3.2B hay không.

## 24. REQUIRED REPORT FORMAT - 14 MỤC

Dev/Codex/Copilot phải trả report đúng 14 mục.

## 24.1. Tóm tắt

Ghi rõ:

Đã triển khai Sellable Gate phần nào.

Logic sellable hiện chạy ở đâu.

Guard nào đã có.

API/DTO nào đã thêm nếu có.

Test/smoke kết quả thế nào.

Có đủ điều kiện sang P3.2B không.

## 24.2. File đã sửa

Liệt kê đầy đủ:

File path.

Loại thay đổi.

Lý do sửa.

Thuộc allowed boundary nào.

Nếu có file ngoài boundary, phải báo FAIL.

## 24.3. Nguồn yêu cầu

Liệt kê:

## MASTER.

## PHASE 0.

## PHASE 1.

## PHASE 2.

P3 Analysis.

P3.1 Technical Design.

Prompt P3.2A.

## 24.4. Evidence đã dùng

Liệt kê:

Code references.

DB/migration references.

Test references.

Smoke evidence.

Audit/evidence registry references nếu có.

API screenshots/logs nếu có.

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

Test nào pass.

Test nào fail.

Fail xử lý ra sao.

Test coverage còn thiếu.

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

Nếu không có migration, ghi rõ.

## 24.12. Kết quả seed validation nếu áp dụng

Ghi:

Có seed không.

Seed có tự set sellable không.

Seed có hardcode SKU/price sai không.

Seed validation result.

## 24.13. Rủi ro còn lại

Phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Owner decision needed.

Technical debt.

Deferred to P3.2B/P3.2C.

## 24.14. Cập nhật handoff

Ghi rõ:

Có được sang P3.2B không.

Điều kiện còn thiếu nếu chưa.

File tiếp theo.

Handoff notes cho QuoteSnapshot/Price runtime.

## 25. DONE GATE

P3.2A được DONE khi đủ tất cả điều kiện sau:

Chỉ sửa file trong allowed boundary.

Sellable Gate Runtime tồn tại hoặc được chỉnh đúng rule.

Product Active không còn bị dùng thay Sellable.

SKU Active không còn bị dùng thay Sellable.

Recipe Active không còn bị dùng thay Sellable.

Warehouse Receipt không tự set Sellable.

Sellable kiểm tra Product active.

Sellable kiểm tra SKU active.

Sellable kiểm tra listed price active hoặc fail-safe nếu price module chưa có.

Sellable kiểm tra released stock.

Sellable kiểm tra warehouse receipt confirmed.

Sellable kiểm tra recall.

Sellable kiểm tra sale lock.

Sellable kiểm tra quality hold.

Sellable kiểm tra channel suppression.

Sellable fail-safe khi runtime policy unavailable.

SellableDecision có block reason.

Public/internal boundary không lộ dữ liệu nhạy cảm.

Test bắt buộc pass hoặc fail được giải thích rõ.

Smoke P3-SMK-2A pass hoặc điểm chặn được báo rõ.

Backend build pass hoặc điểm chặn được báo rõ.

Frontend build pass nếu có ảnh hưởng UI.

Evidence submitted.

Report đủ 14 mục.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 26. FAIL GATE

P3.2A bị FAIL nếu có bất kỳ điều kiện sau:

Sửa file ngoài allowed boundary.

Tự triển khai QuoteSnapshot.

Tự triển khai Cart.

Tự triển khai Order Draft.

Tự triển khai Payment.

Tự triển khai Verified Revenue.

Product Active vẫn được dùng thay Sellable.

SKU Active vẫn được dùng thay Sellable.

Recipe Active vẫn được dùng thay Sellable.

Warehouse Receipt tự set Sellable.

Seed tự set sellable true.

Sellable không kiểm tra listed price.

Sellable không kiểm tra released stock.

Sellable không kiểm tra warehouse confirmed.

Sellable không kiểm tra recall.

Sellable không kiểm tra sale lock.

Sellable không fail-safe khi runtime policy unavailable.

AI/Gateway tự tính sellable.

Public response lộ dữ liệu nội bộ.

Không có block reason.

Không có test negative case.

Không có report 14 mục.

Gọi Gateway PASS.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

## 27. DOWNSTREAM HANDOFF

## 27.1. File tiếp theo

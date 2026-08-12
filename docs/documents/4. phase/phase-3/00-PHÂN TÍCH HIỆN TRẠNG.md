# P3 - ANALYSIS ONLY

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- MASTER registry, source-of-truth va runtime guard.
- PACK-01 Operational Core cho Sellable, Recall va Sale Lock.
- TECH-04 Commerce Runtime cho quote, cart, order, payment va shipping.
- PHASE-01 va PHASE-02 la upstream runtime data owner.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/00-P3-ANALYSIS-ONLY.md

## PROMPT-P3 - COMMERCE RUNTIME / SELLABLE GATE / QUOTE SNAPSHOT / CART / ORDER / PAYMENT / SHIPPING / VERIFIED REVENUE ANALYSIS HANDOFF

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt phân tích ban đầu cho PHASE-03 - Commerce Runtime.

Mục tiêu của PHASE-03 là phân tích hiện trạng codebase thật trước khi thiết kế hoặc triển khai các nhóm chức năng:

Sellable Gate.

Listed Price / Program / Member Benefit Runtime.

QuoteSnapshot.

Cart.

Order Draft.

Customer Confirmation.

Official Order / order_code.

Payment.

Shipping.

Invoice / Tax.

Verified Revenue.

Commission / ROAS handoff.

Đây không phải prompt viết code.

Đây không phải prompt sửa file.

Đây không phải prompt tạo migration.

Đây không phải prompt thêm API.

Đây không phải prompt chạy smoke chính thức.

Đây là prompt để dev/Codex/Copilot đọc codebase thật, đối chiếu tài liệu, xác định khoảng trống, xung đột, rủi ro và lập báo cáo phân tích trước khi chuyển sang P3.1 Technical Design.

## 3. MODE

## MODE: ANALYSIS ONLY - DO NOT MODIFY FILES

Trong mode này, dev/Codex/Copilot chỉ được phép:

Đọc codebase.

Đọc tài liệu kỹ thuật.

Đọc migration/schema hiện có.

Đọc API/controller/service/worker hiện có.

Đọc seed/config/test hiện có.

Đọc Admin UI nếu có.

Ghi nhận hiện trạng.

Lập gap analysis.

Lập conflict analysis.

Đề xuất smoke cần có ở mức phân tích.

Dev/Codex/Copilot không được:

Sửa code.

Sửa database migration.

Sửa seed.

Sửa API.

Sửa DTO.

Sửa frontend.

Sửa test.

Thêm feature.

Tạo order thật.

Tự mở Gateway.

Tự gọi Production Readiness.

Tự gọi Release Readiness.

Tự gọi Completion Decision.

## 4. SOURCE-OF-TRUTH

Dev/Codex/Copilot phải đối chiếu theo thứ tự ưu tiên sau:

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

## 4.2. PHASE trước của TECH-13

## PHASE-00-FOUNDATION-RBAC-AUDIT-EVIDENCE-IDEMPOTENCY.

## PHASE-01-PRODUCT-SKU-RECIPE-ACTIVATION.

## PHASE-02-OPERATIONAL-CORE.

## 4.3. Quy tắc bắt buộc dựa trên từ PHASE 0

Actor Context bắt buộc cho command có side effect.

Correlation ID bắt buộc cho audit/evidence/trace.

RBAC phải enforce ở backend.

Audit append-only.

Evidence Registry là nơi ghi nhận evidence.

Idempotency bắt buộc cho command nhạy cảm.

High-risk Command Guard áp dụng cho action rủi ro.

Admin Override phải có quyền, lý do, audit, expiry nếu áp dụng.

Runtime unavailable phải fail-safe.

Không mở Global Gate.

## 4.4. Quy tắc bắt buộc dựa trên từ PHASE 1

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Batch Released.

Seed không được tự set sellable.

SKU extension phải hỗ trợ mở rộng 40-50 SKU hoặc hơn.

Không hardcode danh sách SKU.

Product/SKU/Recipe/Formula/BOM chỉ là foundation cho phase sau.

## 4.5. Quy tắc bắt buộc dựa trên từ PHASE 2

Raw Lot QC_PASS không đồng nghĩa READY_FOR_PRODUCTION.

Raw lot chỉ được issue khi READY_FOR_PRODUCTION.

Production Order phải snapshot formula kind/version/ingredient/quantity/UOM.

Material Issue là điểm giảm tồn nguyên liệu chính.

Material Receipt không giảm tồn lần hai.

Inventory Ledger append-only.

Batch QC_PASS không đồng nghĩa Batch RELEASED.

Warehouse chỉ nhận Batch RELEASED.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Warehouse Receipt không tự set Sellable.

Public Trace whitelist-only.

QR VOID/FAILED không public-valid.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM, Gateway, IVR.

PHASE 2 hoàn tất report không đồng nghĩa Release Readiness.

## 4.6. Quy tắc bắt buộc của PHASE 3

Sellable Gate là runtime gate riêng.

Không dùng Product Active / SKU Active / Recipe Active thay Sellable.

Sellable phải phụ thuộc tối thiểu:

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

QuoteSnapshot là nguồn sự thật cho giá cuối tại thời điểm báo giá.

Không có QuoteSnapshot thì không có final price.

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới cho phép tạo Official Order.

Không có order_code thì không được nói "đơn hàng đã ghi nhận chính thức".

Payment selected không đồng nghĩa Paid.

Payment Core confirmation mới là paid status.

Shipping fee/VAT/discount/member benefit phải được runtime tính.

AI/Gateway không được hardcode giá, phí ship, VAT, discount, member benefit.

COD chỉ là phương thức "nhận hàng rồi thanh toán".

Không tự thêm phí COD nếu policy chưa khóa.

Verified Revenue chỉ có sau ORDER_VERIFIED hoặc checkpoint tương đương đã được khóa.

No ORDER_VERIFIED thì không final commission.

No ORDER_VERIFIED thì không verified ROAS.

No ORDER_VERIFIED thì không payout.

Commerce Runtime là owner của:

sellable_status.

listed_price_status.

program eligibility.

member benefit eligibility.

diamond referral eligibility.

QuoteSnapshot.

final quote price.

official order state.

verified revenue.

AI Advisor, Gateway, Ads, Live, CRM, IVR chỉ consume runtime.

AI Advisor, Gateway, Ads, Live, CRM, IVR không tự tính giá/tồn/discount/sellable/revenue.

Recall/Sale Lock từ PHASE 2 phải chặn Commerce, AI, Gateway, Ads, Live, CRM, IVR.

## 5. OBJECTIVE

Mục tiêu của prompt này là yêu cầu dev/Codex/Copilot phân tích codebase thật để trả lời các câu hỏi sau:

Codebase hiện đã có Commerce Runtime Core chưa?

Sellable Gate hiện đang nằm ở đâu?

Có đang dùng Product Active/SKU Active/Recipe Active thay Sellable không?

Listed Price Runtime hiện có chưa?

Program eligibility hiện có chưa?

Member benefit eligibility hiện có chưa?

Diamond referral eligibility hiện có chưa?

QuoteSnapshot hiện có chưa?

Cart hiện có chưa?

Order Draft hiện có chưa?

Customer Confirmation hiện có chưa?

Official Order hiện có chưa?

order_code hiện được tạo ở bước nào?

Payment selected và Paid status đã tách chưa?

Payment Core confirmation hiện có chưa?

Shipping fee runtime hiện có chưa?

VAT/tax runtime hiện có chưa?

COD có bị tính phí hardcode sai không?

Verified Revenue hiện có chưa?

ORDER_VERIFIED hoặc equivalent checkpoint hiện có chưa?

Commission/ROAS/payout có đang dùng order chưa verified không?

AI/Gateway/Ads/Live/CRM/IVR có đang tự tính giá/tồn/discount/sellable/revenue không?

Recall/Sale Lock có đang chặn Commerce đúng chưa?

Evidence hiện có đủ để chuyển sang P3.1 Technical Design không?

## 6. REQUIRED INPUT

Dev/Codex/Copilot phải thu thập và đọc tối thiểu các nhóm input sau:

## 6.1. Documentation Input

MASTER Governance documents.

TECH-13 Phase 0 documents.

TECH-13 Phase 1 documents.

TECH-13 Phase 2 documents.

PHASE-03 Commerce Runtime requirement notes.

Any existing Commerce Runtime specs nếu có.

Any existing AI Advisor / Gateway / Ads / Live / CRM / IVR integration notes nếu có.

## 6.2. Backend Input

Backend project structure.

Domain entities liên quan product/SKU/recipe/stock/order/payment/shipping.

Existing controllers/routes.

Existing services.

Existing repositories.

Existing workers/outbox.

Existing policy/resolver/guard classes.

Existing RBAC/permission enforcement.

Existing audit implementation.

Existing idempotency implementation.

Existing evidence registry integration.

Existing migration/schema.

Existing seed data.

Existing tests.

## 6.3. Frontend/Admin Input

Admin UI product/SKU screens.

Price/program screens nếu có.

Stock/inventory screens nếu có.

Order/cart screens nếu có.

Payment/shipping screens nếu có.

Evidence/report screens nếu có.

Any hidden sellable toggle in UI nếu có.

## 6.4. Integration Input

AI Advisor integration points.

Facebook Gateway integration points.

Ads/ROAS integration points.

MC AI Live integration points.

CRM integration points.

IVR reserved extension points nếu có.

MISA integration handoff nếu có.

Payment provider integration nếu có.

Shipping provider integration nếu có.

## 6.5. Runtime/Config Input

Runtime policy configuration.

Price program configuration.

Member tier/benefit configuration.

Shipping policy configuration.

VAT/tax policy configuration.

Channel suppression configuration.

Recall/sale lock configuration.

Quote freeze window configuration.

COD policy configuration.

Revenue verification policy configuration.

## 7. SCOPE IN

Trong P3 Analysis Only, phạm vi phân tích bao gồm:

## 7.1. Sellable Gate

Phân tích xem codebase có gate riêng cho sellable không.

Sellable phải được kiểm tra như runtime decision, không phải field tĩnh đơn giản.

Sellable phải bị chặn nếu thiếu bất kỳ điều kiện tối thiểu nào:

Product inactive.

SKU inactive.

Listed price inactive.

Không có tồn khả dụng.

Finished goods chưa qua warehouse receipt confirmed.

Batch chưa RELEASED.

Có recall.

Có sale lock.

Có quality hold.

Có channel suppression.

Runtime policy unavailable.

Không resolve được rule bắt buộc.

## 7.2. Listed Price Runtime

Phân tích hiện trạng listed price:

Có bảng/entity listed price không?

Có trạng thái active/inactive không?

Có effective date không?

Có audit không?

Có version/history không?

Có channel/program applicability không?

Có đang hardcode giá trong AI/Gateway/frontend không?

## 7.3. Program Runtime

Phân tích hiện trạng chương trình:

24/7.

Golden Hour.

Program activation.

Program eligibility.

Program priority.

Program conflict.

Program time window.

Program freeze window.

Program audit.

Program evidence.

## 7.4. Member Benefit Runtime

Phân tích hiện trạng member benefit:

Member identity.

Member tier.

Member cycle.

Member benefit eligibility.

Tier discount.

Maintain/upgrade/downgrade logic.

Grace period nếu có.

Benefit conflict với program.

Benefit usage audit.

Runtime variable cho AI/Gateway.

## 7.5. Diamond Referral Runtime

Phân tích hiện trạng Diamond referral:

Referral link.

Referral owner.

Referral owner tier.

Referral bind.

Buyer from Diamond link.

Self-purchase exclusion.

Commission eligibility.

Commission freeze.

Commission WAITING/verified/final.

Handoff sang Finance/KPI/Reward.

## 7.6. QuoteSnapshot

Phân tích xem QuoteSnapshot đã tồn tại chưa và có đủ dữ liệu không:

quote_snapshot_id.

customer_id/session_id.

channel.

SKU/order lines.

listed price.

program discount.

member benefit.

shipping fee.

VAT/tax.

final payable amount.

quote created time.

quote expiry time.

freeze window.

policy version.

resolver output.

audit/correlation id.

immutable snapshot.

fail-safe nếu runtime unavailable.

## 7.7. Cart

Phân tích hiện trạng Cart:

Cart có tồn tại không?

Cart có gắn QuoteSnapshot không?

Cart có kiểm tra sellable không?

Cart có kiểm tra stock không?

Cart có giữ line item đúng runtime không?

Cart có bị hiểu sai là Order không?

Cart có audit không?

Cart có idempotency không?

## 7.8. Order Draft

Phân tích hiện trạng Order Draft:

Order Draft có tồn tại không?

Order Draft có gắn QuoteSnapshot không?

Order Draft có gắn customer data không?

Order Draft có gắn shipping data không?

Order Draft có gắn payment selection không?

Order Draft có gắn Customer Confirmation chưa?

Order Draft có bị hiểu sai là Official Order không?

Order Draft có expiry không?

Order Draft có audit không?

## 7.9. Customer Confirmation

Phân tích hiện trạng xác nhận khách hàng:

Có trạng thái CustomerConfirmation không?

Có lưu thời điểm xác nhận không?

Có lưu kênh xác nhận không?

Có lưu actor/customer identity không?

Có lưu quote/order draft được xác nhận không?

Có chống xác nhận trùng không?

Có kiểm tra QuoteSnapshot còn hiệu lực không?

Có kiểm tra lại Sellable Gate trước Official Order không?

## 7.10. Official Order

Phân tích hiện trạng Official Order:

Official Order được tạo ở đâu?

Điều kiện tạo Official Order là gì?

Có bắt buộc Customer Confirmation không?

Có bắt buộc QuoteSnapshot không?

Có bắt buộc order_code không?

Có state machine không?

Có idempotency không?

Có audit không?

Có rollback/fail-safe không?

Có chặn duplicate order không?

## 7.11. Payment

Phân tích hiện trạng Payment:

Payment selected có tách khỏi Paid không?

COD có được hiểu đúng là phương thức thanh toán khi nhận hàng không?

Có tự thêm phí COD không?

Chuyển khoản có trạng thái WAITING/confirmed/rejected không?

Payment Core confirmation là gì?

Paid status do ai set?

Có audit payment không?

Có đối soát payment không?

Có evidence payment không?

## 7.12. Shipping

Phân tích hiện trạng Shipping:

Shipping fee do runtime tính hay hardcode?

Có shipping policy không?

Có vùng giao hàng không?

Có miễn phí ship theo chương trình không?

Có ETA không?

Có shipping state không?

Có shipment handoff không?

Có audit không?

Có không tự hỏi lại địa chỉ khi đã có Order Draft không?

## 7.13. Invoice / Tax

Phân tích hiện trạng invoice/tax:

VAT có runtime policy không?

Tax có được tính trong QuoteSnapshot không?

Invoice request có state không?

Có MISA handoff không?

Có audit không?

Có customer invoice info không?

Có fail-safe nếu thiếu tax policy không?

## 7.14. Verified Revenue

Phân tích hiện trạng Verified Revenue:

Có ORDER_VERIFIED không?

Nếu không có ORDER_VERIFIED, có equivalent checkpoint nào không?

Verified Revenue được tạo ở đâu?

Điều kiện revenue verified là gì?

Có chặn commission khi chưa verified không?

Có chặn ROAS verified khi chưa verified không?

Có chặn payout khi chưa verified không?

Có handoff sang Finance/KPI/Reward/Tax/Voucher không?

Có handoff sang Ads/ROAS không?

Có audit/evidence không?

## 8. SCOPE OUT

P3 Analysis Only không bao gồm:

Không sửa code.

Không tạo entity mới.

Không tạo migration mới.

Không tạo API mới.

Không tạo DTO mới.

Không sửa frontend.

Không sửa Admin UI.

Không sửa seed.

Không sửa test.

Không tạo worker.

Không chạy smoke chính thức.

Không tạo evidence accepted.

Không mở Gateway.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Không tự quyết định chính sách giá.

Không tự quyết định chính sách vận chuyển.

Không tự quyết định VAT/tax.

Không tự quyết định commission/payout.

Không cho AI/Gateway tự tính thay Commerce Runtime.

Không bỏ qua owner review.

## 9. CURRENT IMPLEMENTATION STATE MATRIX

Dev/Codex/Copilot phải lập bảng hiện trạng theo mẫu sau.

Nhóm kiểm tra

Có trong code chưa

File/Module hiện tại

Owner hiện tại

Đúng rule PHASE 3 chưa

Rủi ro

Ghi chú

Product active

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Chỉ là điều kiện nền, không phải Sellable

SKU active

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không được thay Sellable

Recipe active

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không được thay Sellable

Listed price active

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Bắt buộc trước QuoteSnapshot

Stock available

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Phải dựa trên finished goods/warehouse confirmed

Batch RELEASED

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

QC_PASS chưa đủ

Warehouse receipt confirmed

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Điểm tăng tồn finished goods

Recall lock

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Phải thắng Commerce

Sale lock

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Phải thắng Commerce

Quality hold

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Chặn sellable

Channel suppression

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Chặn theo kênh

Runtime policy available

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Unavailable phải fail-safe

Sellable Gate

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Runtime gate riêng

Program eligibility

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không hardcode

Member benefit eligibility

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Runtime resolve

Diamond referral eligibility

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Runtime resolve

QuoteSnapshot

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Nguồn sự thật giá cuối

Cart

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không đồng nghĩa Order

Order Draft

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không đồng nghĩa Official Order

Customer Confirmation

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Bắt buộc trước Official Order

Official Order

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Phải có order_code

Payment selected

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không đồng nghĩa Paid

Paid status

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Do Payment Core confirmation

Shipping runtime

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không hardcode

VAT/tax runtime

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không hardcode

Verified Revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Sau ORDER_VERIFIED

Commission handoff

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Chưa verified thì không final

ROAS handoff

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Chưa verified thì không verified ROAS

Payout handoff

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Chưa verified thì không payout

## 10. P3 điểm chặn

Các điểm chặn sau nếu xuất hiện thì PHASE 3 không được chuyển sang Technical Design hoặc Implementation nếu chưa ghi nhận rõ trong report.

## 10.1. điểm chặn về Sellable

Product Active đang được dùng thay Sellable.

SKU Active đang được dùng thay Sellable.

Recipe Active đang được dùng thay Sellable.

Warehouse Receipt đang tự set Sellable.

Seed đang tự set Sellable.

Sellable là field tĩnh không qua runtime gate.

Sellable không kiểm tra recall/sale lock.

Sellable không kiểm tra stock released/warehouse confirmed.

Sellable không fail-safe khi runtime policy unavailable.

## 10.2. điểm chặn về QuoteSnapshot

Final price được trả ra mà không có QuoteSnapshot.

AI/Gateway tự tính final price.

Program discount hardcode trong content.

Member benefit hardcode trong content.

Shipping fee hardcode trong content.

VAT/tax hardcode trong content.

Không lưu policy version.

Không lưu quote expiry/freeze window.

QuoteSnapshot có thể bị sửa sau khi tạo.

## 10.3. điểm chặn về Order

Cart bị hiểu là Order.

Order Draft bị hiểu là Official Order.

Official Order được tạo khi chưa có Customer Confirmation.

Official Order không có order_code.

AI/Gateway nói "đơn hàng đã ghi nhận chính thức" khi chưa có order_code.

Không có idempotency chống tạo order trùng.

Không có audit cho order state transition.

Không có state machine rõ.

## 10.4. điểm chặn về Payment

Payment selected bị hiểu là Paid.

COD bị tự thêm phí khi policy chưa khóa.

Paid status do AI/Gateway set.

Không có Payment Core confirmation.

Không có trạng thái WAITING/confirmed/rejected.

Không có audit payment.

## 10.5. điểm chặn về Shipping/Tax

Shipping fee hardcode.

VAT hardcode.

Runtime policy thiếu nhưng vẫn cho quote/order.

Không có handoff invoice/tax.

Không phân biệt shipping selected, shipping fee calculated, shipment created.

## 10.6. điểm chặn về Verified Revenue

Order chưa verified nhưng đã tính commission final.

Order chưa verified nhưng đã tính verified ROAS.

Order chưa verified nhưng đã cho payout.

Không có ORDER_VERIFIED hoặc equivalent checkpoint.

Verified Revenue do Ads/AI/Gateway tự tạo.

Không có audit/evidence cho verified revenue.

## 10.7. điểm chặn về Gateway/Release

Bất kỳ phần nào gọi Gateway PASS.

Bất kỳ phần nào gọi Completion Decision.

Bất kỳ phần nào gọi Release Readiness.

Bất kỳ phần nào gọi Production Readiness.

Bất kỳ phần nào gọi Go-live Decision.

Evidence Submitted bị hiểu nhầm là Evidence Accepted.

Smoke cục bộ bị hiểu nhầm là Global Smoke Pass.

## 11. GAP ANALYSIS MATRIX

Dev/Codex/Copilot phải lập bảng gap analysis theo mẫu sau.

Rule ID

Quy tắc cần có

Hiện trạng codebase

Gap

Mức độ

Đề xuất xử lý ở P3.1

Evidence cần có

## P3-GAP-001

Sellable là runtime gate riêng

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Thiết kế SellableGateResolver

Code map + design note

## P3-GAP-002

Không dùng Product Active thay Sellable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Tách ProductActiveCondition khỏi SellableDecision

Code reference

## P3-GAP-003

Không dùng SKU Active thay Sellable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Tách SKUActiveCondition khỏi SellableDecision

Code reference

## P3-GAP-004

Listed price active bắt buộc

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Thiết kế ListedPriceResolver

Price evidence

## P3-GAP-005

Stock available từ warehouse confirmed

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Liên kết Operational stock projection

Inventory evidence

## P3-GAP-006

Recall/Sale Lock thắng Commerce

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Commerce phải consume lock state

Recall evidence

## P3-GAP-007

QuoteSnapshot là nguồn giá cuối

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Thiết kế immutable QuoteSnapshot

Quote evidence

## P3-GAP-008

Không có QuoteSnapshot thì không final price

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Chặn final quote response

API evidence

## P3-GAP-009

Cart không phải Order

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Tách cart state

State evidence

## P3-GAP-010

Order Draft không phải Official Order

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Tách order draft state

State evidence

## P3-GAP-011

Customer Confirmation trước Official Order

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Thiết kế confirmation gate

Confirmation evidence

## P3-GAP-012

order_code mới là official marker

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Chỉ tạo sau confirmation

Order evidence

## P3-GAP-013

Payment selected không phải Paid

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Tách payment selection/payment confirmation

Payment evidence

## P3-GAP-014

COD không tự thêm phí nếu chưa khóa policy

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

COD policy guard

Policy evidence

## P3-GAP-015

Shipping fee runtime

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

ShippingResolver

Shipping evidence

## P3-GAP-016

VAT/tax runtime

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

TaxResolver

Tax evidence

## P3-GAP-017

Verified Revenue sau ORDER_VERIFIED

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Revenue verification checkpoint

Revenue evidence

## P3-GAP-018

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Commission WAITING until verified

Finance evidence

## P3-GAP-019

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Ads consume verified revenue only

Ads evidence

## P3-GAP-020

AI/Gateway chỉ consume runtime

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P0

Remove self-calculation

Integration evidence

## 12. CONFLICT DETECTION MATRIX

Dev/Codex/Copilot phải phát hiện và báo cáo các xung đột sau.

Conflict ID

Dấu hiệu xung đột

Vị trí cần kiểm tra

Tác động

Cách báo cáo

## P3-CONFLICT-001

Product active được dùng như sellable

Product/SKU service/API/UI

Bán hàng sai điều kiện

Ghi file, function, route

## P3-CONFLICT-002

SKU active được dùng như sellable

SKU service/API/UI

Mở bán sai

Ghi file, function, route

## P3-CONFLICT-003

Recipe active được dùng để cho bán

Recipe/service/order

Lẫn sản xuất với thương mại

Ghi file, function

## P3-CONFLICT-004

Warehouse receipt tự set sellable

Warehouse/inventory service

Sai owner domain

Ghi state transition

## P3-CONFLICT-005

AI/Gateway hardcode price

AI/Gateway/content/config

Tranh chấp giá

Ghi exact file

## P3-CONFLICT-006

Final price không có quote snapshot

Quote/order API

Không audit được giá

Ghi endpoint

## P3-CONFLICT-007

Cart tạo order trực tiếp

Cart/order service

Bỏ qua confirmation

Ghi flow

## P3-CONFLICT-008

Order Draft tạo order_code sớm

Order draft service

Ghi nhận đơn sai

Ghi state

## P3-CONFLICT-009

Payment selected set paid

Payment service

Sai doanh thu

Ghi transition

## P3-CONFLICT-010

COD bị tính phí mặc định

Payment/shipping policy

Sai tổng thanh toán

Ghi rule

## P3-CONFLICT-011

Shipping fee hardcode

Shipping/API/content

Sai runtime policy

Ghi file

## P3-CONFLICT-012

VAT hardcode

Tax/invoice/content

Sai kế toán

Ghi file

## P3-CONFLICT-013

Commission tính trước verified

Finance/reward

Sai payout

Ghi flow

## P3-CONFLICT-014

ROAS dùng order chưa verified

Ads/measurement

Sai hiệu quả ads

Ghi flow

## P3-CONFLICT-015

Recall lock không chặn Commerce

Recall/sale/order

Rủi ro pháp lý

Ghi flow

## P3-CONFLICT-016

Gateway mở trước evidence

Gateway/config/release

Sai release gate

Ghi config

## P3-CONFLICT-017

Evidence submitted được hiểu là accepted

Evidence/release docs

Sai PASS

Ghi trạng thái

## P3-CONFLICT-018

Smoke local gọi là global pass

QA/report

Sai release

Ghi report

## 13. DOWNSTREAM IMPACT

PHASE 3 có ảnh hưởng trực tiếp đến các pack và hệ thống sau:

## 13.1. AI Advisor

AI Advisor chỉ được tư vấn giá, quyền lợi, phí ship, ưu đãi, tồn bán được, sản phẩm thay thế và form xác nhận đơn dựa trên Commerce Runtime.

AI Advisor không được tự tính:

Giá cuối.

Discount.

Member benefit.

Diamond benefit.

Shipping fee.

## VAT.

Sellable.

Verified revenue.

Commission.

## ROAS.

## 13.2. Facebook Gateway

Gateway chỉ consume:

sellable_status.

QuoteSnapshot.

Order Draft.

Customer Confirmation.

Official Order status.

Payment status.

Shipping status.

Verified Revenue status nếu cần.

Gateway không được tự mở bán, tự báo giá cuối, tự tạo order chính thức khi chưa có confirmation.

## 13.3. Ads / ROAS Measurement

Ads chỉ được dùng verified revenue sau ORDER_VERIFIED hoặc checkpoint tương đương.

Không dùng:

Cart value.

Order Draft value.

Unpaid order value.

Unverified order value.

để tính verified ROAS.

## 13.4. MC AI Live

MC AI Live chỉ được dùng runtime để trả lời public/private.

MC AI Live không được tự tính giá, quyền lợi, phí ship hoặc tồn bán.

## 13.5. CRM

CRM chỉ được gợi ý mua lại, combo, ưu đãi, quyền lợi thành viên nếu runtime xác nhận:

SKU sellable.

Không recall.

Không sale lock.

Có chương trình hợp lệ.

Có QuoteSnapshot khi báo giá.

Có customer context hợp lệ.

## 13.6. IVR

IVR hiện là reserved pack.

PHASE 3 chỉ được chừa extension point cho IVR nếu cần, không triển khai IVR logic trong Commerce.

IVR không được tự xác nhận paid, không tự verified revenue, không tự hủy/chốt order ngoài Core Order State Machine.

## 13.7. Finance / KPI / Reward / Tax / Voucher

PHASE 3 cung cấp dữ liệu đầu vào cho finance/reward/tax.

Tuy nhiên:

Commission final chỉ sau verified revenue.

Payout chỉ sau verified checkpoint.

Tax/invoice phải theo runtime policy và integration boundary.

Voucher nếu có phải qua Commerce Runtime/Finance policy owner, không hardcode trong channel.

## 13.8. Operational Core

PHASE 3 consume:

Finished goods stock.

Warehouse receipt confirmed.

Batch released.

Recall state.

Sale lock state.

Quality hold state.

PHASE 3 không được tự sửa operational truth.

## 14. EVIDENCE REQUIRED

Trong Analysis Only, dev chưa cần tạo accepted evidence. Tuy nhiên report phải chỉ ra evidence cần có cho các bước sau.

## 14.1. Evidence cho Sellable Gate

Code map SellableGate hiện có hoặc thiếu.

Danh sách file liên quan Product/SKU/Recipe active.

Danh sách file liên quan stock/inventory.

Danh sách file liên quan recall/sale lock.

Danh sách API đang trả sellable/status.

Danh sách UI đang hiển thị bán được/không bán được.

Bằng chứng có/không có fail-safe khi runtime unavailable.

## 14.2. Evidence cho QuoteSnapshot

Entity/schema hiện có.

API hiện có.

Service hiện có.

DTO hiện có.

Audit hiện có.

Immutable behavior nếu có.

Chính sách freeze/expiry nếu có.

Bằng chứng không hardcode giá ở AI/Gateway.

## 14.3. Evidence cho Cart / Order Draft / Official Order

State machine hiện có.

Order creation flow.

Customer confirmation flow.

order_code creation point.

Idempotency evidence.

Duplicate order prevention evidence.

Audit evidence.

Error/fail-safe evidence.

## 14.4. Evidence cho Payment / Shipping / Tax

Payment state evidence.

Payment confirmation source.

COD handling evidence.

Shipping policy evidence.

Tax/VAT policy evidence.

Invoice request evidence.

MISA handoff evidence nếu có.

## 14.5. Evidence cho Verified Revenue

ORDER_VERIFIED checkpoint evidence.

Equivalent verified checkpoint nếu chưa dùng tên ORDER_VERIFIED.

Revenue status flow.

Commission WAITING/final flow.

ROAS handoff flow.

Payout block evidence.

Audit/evidence registry linkage.

## 15. SMOKE REQUIRED - PROPOSED ONLY, DO NOT WRITE TESTS YET

Trong Analysis Only, dev chỉ đề xuất smoke cần có, chưa viết test.

## 15.1. Proposed Sellable Smoke

Product active nhưng không có listed price -> không sellable.

SKU active nhưng không có stock -> không sellable.

Recipe active nhưng batch chưa released -> không sellable.

Batch QC_PASS nhưng chưa RELEASED -> không sellable.

Warehouse chưa confirmed receipt -> không sellable.

Có recall -> không sellable.

Có sale lock -> không sellable.

Runtime policy unavailable -> fail-safe not sellable.

Channel suppression active -> channel đó không sellable.

SKU đủ điều kiện -> sellable true.

## 15.2. Proposed Quote Smoke

Không có QuoteSnapshot -> không trả final price.

QuoteSnapshot có listed price + program + member benefit + shipping + VAT.

QuoteSnapshot hết hạn -> không cho xác nhận order.

QuoteSnapshot không được mutate sau khi tạo.

Golden Hour freeze window đúng policy nếu có.

24/7 freeze window đúng policy nếu có.

AI/Gateway nhận quote từ runtime, không tự tính.

## 15.3. Proposed Cart / Order Draft Smoke

Add to cart SKU not sellable -> reject.

Cart created không tạo Official Order.

Order Draft created không tạo order_code.

Order Draft thiếu customer info -> chưa official.

Order Draft thiếu confirmation -> chưa official.

Customer Confirmation hợp lệ -> cho tạo Official Order.

Confirmation trùng -> không tạo duplicate order.

Quote hết hạn -> yêu cầu quote lại.

## 15.4. Proposed Official Order Smoke

Official Order chỉ tạo sau Customer Confirmation.

Official Order phải có order_code.

Duplicate request cùng idempotency key -> chỉ một order_code.

Không có QuoteSnapshot -> reject official order.

Sellable thay đổi trước confirmation -> reject hoặc requote theo policy.

Recall phát sinh trước confirmation -> reject.

## 15.5. Proposed Payment Smoke

Payment selected COD -> chưa paid.

Payment selected bank transfer -> chưa paid.

Payment Core confirmed -> paid.

Payment failed/rejected -> not paid.

COD không tự thêm phí nếu policy chưa có.

Paid status không do AI/Gateway set.

## 15.6. Proposed Shipping / Tax Smoke

Shipping fee runtime có trong QuoteSnapshot.

Miễn phí ship nếu policy/runtime xác nhận.

Không hardcode shipping fee trong content.

VAT included/excluded theo tax policy.

Missing tax policy -> fail-safe theo rule.

Customer hỏi ETA khi đã có Order Draft -> trả ETA, không hỏi lại địa chỉ.

## 15.7. Proposed Verified Revenue Smoke

Paid nhưng chưa ORDER_VERIFIED -> chưa verified revenue.

Delivered nhưng chưa verified checkpoint -> chưa final commission.

ORDER_VERIFIED -> tạo verified revenue.

No ORDER_VERIFIED -> no final commission.

No ORDER_VERIFIED -> no verified ROAS.

No ORDER_VERIFIED -> no payout.

Ads consume verified revenue only.

## 16. EXECUTION STEPS

Dev/Codex/Copilot thực hiện theo trình tự sau.

Step 1 - Confirm Mode

Xác nhận đang ở:

## MODE: ANALYSIS ONLY - DO NOT MODIFY FILES

Nếu tool/agent chuẩn bị sửa file thì phải dừng.

Step 2 - Read Source-of-Truth

Đọc các tài liệu:

MASTER governance.

## PHASE 0.

## PHASE 1.

## PHASE 2.

PHASE 3 requirement note.

Không dùng code hiện tại để override yêu cầu đã khóa.

Code hiện tại chỉ là evidence hiện trạng.

Step 3 - Inspect Backend Structure

Liệt kê các module/backend folders liên quan:

Product.

## SKU.

Recipe.

Inventory.

Warehouse.

Recall.

Sale lock.

Price.

Program.

Member.

Quote.

Cart.

Order.

Payment.

Shipping.

Tax.

Revenue.

Commission.

Ads/ROAS.

Gateway integration.

Step 4 - Inspect Database/Migration

Ghi nhận:

Entity/table hiện có.

State/status enum hiện có.

Constraint hiện có.

Index hiện có.

Audit/evidence field hiện có.

Idempotency key hiện có.

Missing tables/fields.

Không tạo migration.

Step 5 - Inspect API/Service Flow

Ghi nhận:

Endpoint nào trả product/SKU/sellable.

Endpoint nào trả price.

Endpoint nào tạo quote.

Endpoint nào tạo cart.

Endpoint nào tạo order draft.

Endpoint nào xác nhận order.

Endpoint nào tạo official order.

Endpoint nào set payment.

Endpoint nào set paid.

Endpoint nào set verified revenue.

Step 6 - Inspect Integration Boundary

Kiểm tra:

AI Advisor có tự tính gì không.

Gateway có tự tính gì không.

Ads có dùng revenue chưa verified không.

Live có hardcode giá không.

CRM có hardcode ưu đãi không.

IVR có logic ẩn không.

MISA có bị gọi trực tiếp từ business module không.

Step 7 - Fill Matrices

Hoàn tất:

Current Implementation State Matrix.

Gap Analysis Matrix.

Conflict Detection Matrix.

P3 điểm chặn List.

Downstream Impact Map.

Evidence Required List.

Proposed Smoke List.

Step 8 - Produce Report

Báo cáo phải có đúng 14 mục theo format bắt buộc.

Step 9 - Final Recommendation

Kết luận một trong các trạng thái:

## READY_FOR_P3_1_TECHNICAL_DESIGN.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_SOURCE_CLARIFICATION.

## BLOCKED_NEED_CODEBASE_CLEANUP_BEFORE_DESIGN.

Không được kết luận Release Readiness.

Không được kết luận Production Readiness.

## 17. REQUIRED REPORT FORMAT - 14 MỤC

Dev/Codex/Copilot phải trả report đúng 14 mục sau.

## 17.1. Tóm tắt

Ghi rõ:

Đã phân tích phạm vi nào.

Đã đọc module nào.

Kết luận sơ bộ.

Có điểm chặn P0 không.

Có được chuyển P3.1 không.

## 17.2. File đã sửa

Vì mode là Analysis Only, mục này phải ghi:

Không sửa file. MODE: ANALYSIS ONLY.

Nếu có file bị sửa, phải báo FAIL GATE.

## 17.3. Nguồn yêu cầu

Liệt kê tài liệu đã dùng:

## MASTER.

## PHASE 0.

## PHASE 1.

## PHASE 2.

PHASE 3 notes.

Codebase hiện trạng.

## 17.4. Evidence đã dùng

Liệt kê evidence đã đọc:

File paths.

Modules.

APIs.

DB/migrations.

Config.

Tests nếu có.

Screens nếu có.

## 17.5. Lệnh đã chạy

Ghi rõ lệnh đã chạy để phân tích, ví dụ:

List files.

Search code.

Build read-only nếu có.

Test discovery nếu có.

Không chạy lệnh sửa file.

## 17.6. Kết quả test

Vì Analysis Only, nếu không chạy test thì ghi:

Chưa chạy test chính thức. Chỉ phân tích test coverage hiện có.

Nếu có chạy test read-only thì ghi kết quả.

## 17.7. Kết quả backend build

Ghi:

Có chạy build không.

Kết quả.

Nếu không chạy, lý do.

## 17.8. Kết quả frontend build

Ghi:

Có chạy build không.

Kết quả.

Nếu không chạy, lý do.

## 17.9. Kết quả cleanup process

Ghi:

Có cleanup process không.

Có phát sinh file tạm không.

Có xóa file không.

Trong Analysis Only không nên cleanup làm thay đổi repo.

## 17.10. Cập nhật Markdown

Vì Analysis Only, ghi:

Không cập nhật Markdown.

Nếu có đề xuất Markdown cần cập nhật cho phase sau thì ghi riêng dưới dạng recommendation.

## 17.11. Kết quả database migration/update nếu áp dụng

Vì Analysis Only, ghi:

Không chạy migration. Không update database.

Chỉ ghi nhận migration hiện có.

## 17.12. Kết quả seed validation nếu áp dụng

Ghi:

Có kiểm tra seed không.

Seed có tự set sellable không.

Seed có hardcode price/program sai không.

Seed có đủ dữ liệu nền không.

Không sửa seed.

## 17.13. Rủi ro còn lại

Phải phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Open question cần owner quyết định.

Open question cần technical design.

## 17.14. Cập nhật handoff

Ghi rõ:

Có đủ điều kiện sang P3.1 không.

Nếu có, handoff sang file nào.

Nếu chưa, cần mở rộng gì.

Prompt tiếp theo là gì.

## 18. DONE GATE

P3 Analysis Only được xem là DONE khi đủ tất cả điều kiện sau:

Không sửa file.

Không tạo migration.

Không sửa seed.

Không sửa test.

Không thêm feature.

Đọc đủ source-of-truth.

Lập đầy đủ Current Implementation State Matrix.

Lập đầy đủ Gap Analysis Matrix.

Lập đầy đủ Conflict Detection Matrix.

Liệt kê P3 điểm chặn rõ ràng.

Liệt kê downstream impact rõ ràng.

Liệt kê evidence required cho phase sau.

Đề xuất smoke required ở mức phân tích.

Report đúng 14 mục.

Không gọi Gateway PASS.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Kết luận rõ có được chuyển sang P3.1 Technical Design hay không.

## 19. FAIL GATE

P3 Analysis Only bị FAIL nếu có bất kỳ điều kiện sau:

Dev/Codex/Copilot sửa file.

Dev/Codex/Copilot tạo migration.

Dev/Codex/Copilot sửa seed.

Dev/Codex/Copilot sửa test.

Dev/Codex/Copilot thêm API.

Dev/Codex/Copilot thêm DTO.

Dev/Codex/Copilot thêm UI.

Dev/Codex/Copilot tạo feature.

Dev/Codex/Copilot tự mở Gateway.

Dev/Codex/Copilot gọi Global Smoke Pass khi chỉ có smoke local.

Dev/Codex/Copilot gọi Evidence Accepted khi mới submit evidence.

Dev/Codex/Copilot gọi Release Readiness.

Dev/Codex/Copilot gọi Production Readiness.

Dev/Codex/Copilot bỏ qua QuoteSnapshot rule.

Dev/Codex/Copilot cho phép final price không có QuoteSnapshot.

Dev/Codex/Copilot đồng ý dùng Product/SKU/Recipe Active thay Sellable.

Dev/Codex/Copilot đồng ý để AI/Gateway tự tính giá/discount/ship/VAT.

Dev/Codex/Copilot đồng ý dùng order chưa verified để tính commission/ROAS/payout.

Report thiếu 14 mục.

Không chỉ rõ điểm chặn.

## 20. DOWNSTREAM HANDOFF SANG P3.1 TECHNICAL DESIGN

Chỉ được chuyển sang P3.1 khi P3 Analysis Only đạt DONE GATE.

## 20.1. File tiếp theo

# TECH-12 - PHASE BACKLOG PACK DEV TASK BREAKDOWN SOURCE TO BACKLOG MATRIX

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## PHASE BACKLOG GOVERNANCE / SOURCE-TO-TASK TRACEABILITY / DEV TASK BREAKDOWN / EVIDENCE-SMOKE-READY BACKLOG CONTROL

## PHẦN 1/4 - BACKLOG PACK PRINCIPLES / SOURCE-TO-BACKLOG BOUNDARY / PHASE BACKLOG GOVERNANCE / NO-UNMAPPED-TASK RULE

## 1. MỤC TIÊU CỦA TECH-12

TECH-12 là tài liệu chuyển từ Implementation Roadmap sang Backlog triển khai cụ thể cho dev.

TECH-12 không thay thế TECH-00 -> TECH-11.

TECH-12 không viết code.

TECH-12 không thiết kế API chi tiết.

TECH-12 không thiết kế database chi tiết.

TECH-12 không thiết kế UI chi tiết.

TECH-12 có nhiệm vụ khóa cách biến toàn bộ hệ thống TECH source-of-truth thành các backlog item cụ thể, có thể giao cho dev triển khai theo phase.

TECH-12 trả lời các câu hỏi:

Mỗi phase phải chia thành những backlog nhóm nào.

Mỗi backlog phải map về TECH nào, section nào, requirement nào.

Backlog nào làm trước, backlog nào làm sau.

Backlog nào bị chặn bởi dependency.

Backlog nào cần evidence.

Backlog nào cần smoke.

Backlog nào cần QA.

Backlog nào cần owner review.

Backlog nào không được đưa vào dev vì thiếu source.

Backlog nào không được đưa vào code vì thiếu architecture context.

Backlog nào chỉ được chuẩn bị, chưa được release.

Backlog nào phải đi qua TECH-10 Global Gateway trước khi vận hành thật.

Mục tiêu cuối cùng của TECH-12 là tạo một Phase Backlog Pack đủ rõ để dev không tự suy luận.

## 2. VAI TRÒ CỦA TECH-12 TRONG BỘ TÀI LIỆU

TECH-12 đứng sau:

## TECH-00 - Global Technical Governance.

## TECH-01 - Foundation / RBAC / Audit / Idempotency / Evidence Registry.

## TECH-02 - Product / SKU / Recipe / Product Activation.

## TECH-03 - Operational Core.

## TECH-04 - Commerce Runtime.

## TECH-05 - AI Advisor Runtime.

## TECH-06 - Facebook Gateway.

## TECH-07 - Ads Measurement.

## TECH-08 - MC AI Live.

## TECH-09 - IVR Order Confirmation.

## TECH-10 - Global Smoke / UAT / Evidence / Release Gateway.

## TECH-11 - Implementation Roadmap / Dev Phase Plan / Backlog Governance / Code Handoff Control.

TECH-11 đã khóa cách triển khai theo phase.

TECH-12 tiếp tục khóa cách biến từng phase thành backlog cụ thể.

Nói cách khác:

TECH-11 trả lời: triển khai theo nguyên tắc và phase nào.

TECH-12 trả lời: mỗi phase chia thành backlog nào, task nào, evidence nào, smoke nào.

Các tài liệu sau TECH-12 mới có thể dùng để viết prompt giao Codex/Copilot theo từng phase.

## 3. TECH-12 KHÔNG PHẢI TÀI LIỆU CODE

TECH-12 không viết code.

TECH-12 không tạo file code.

TECH-12 không đưa snippet cho dev copy.

TECH-12 không viết class, service, controller, migration, DTO, component, worker hoặc script.

TECH-12 không đi vào chi tiết API/DB/UI nếu chưa có yêu cầu riêng.

TECH-12 chỉ tạo backlog pack để dev triển khai trong codebase thật.

Một backlog đúng không phải là code.

Một backlog đúng là một đơn vị công việc có:

Source-of-truth.

Phase.

Scope.

Dependency.

Acceptance criteria.

Evidence requirement.

Smoke requirement.

P0 điểm chặn.

Report requirement.

Handoff target.

## 4. NO-UNMAPPED-TASK RULE

## 4.1. Không task nào được tồn tại nếu không map source

Mọi backlog item phải map về source-of-truth.

Một task không có source là task không hợp lệ.

Không được tạo task kiểu:

“Làm cho chạy.”

“Sửa AI.”

“Làm kho.”

“Làm bán hàng.”

“Làm Ads.”

“Làm IVR.”

“Tối ưu hệ thống.”

“Code phần này.”

“Triển khai nhanh.”

“Dev tự phân tích thêm.”

Tất cả đều quá mơ hồ.

## 4.2. Một backlog hợp lệ phải có source

Mỗi backlog phải có tối thiểu:

TECH number.

Section hoặc requirement group.

Phase.

Domain.

Business rule.

Dependency.

P0 điểm chặn.

Evidence.

Smoke.

Done Gate.

Fail Gate.

Handoff.

## 4.3. Nếu chưa map được source

Nếu chưa map được source:

Không đưa backlog vào Ready.

Không handoff cho dev.

Không tạo code handoff.

Không đưa vào sprint.

Không cho coding agent xử lý.

Phải đưa vào Source Mapping Review.

## 4.4. P0 điểm chặn

FAIL nếu backlog không có source vẫn được giao dev.

FAIL nếu backlog không có TECH reference vẫn được đưa vào Ready.

FAIL nếu backlog mơ hồ được coding agent triển khai.

FAIL nếu dev tự suy luận requirement chưa được map.

## 5. SOURCE-TO-BACKLOG BOUNDARY

## 5.1. Source-of-truth đầu vào

TECH-12 chỉ được lấy source từ:

## TECH-00.

## TECH-01.

## TECH-02.

## TECH-03.

## TECH-04.

## TECH-05.

## TECH-06.

## TECH-07.

## TECH-08.

## TECH-09.

## TECH-10.

## TECH-11.

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

Code cũ chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Không dùng tài liệu cũ hoặc code cũ để phủ định TECH mới.

## 5.2. Backlog đầu ra

Đầu ra của TECH-12 là:

Phase backlog group.

Backlog item.

Source-to-backlog matrix.

Dependency matrix.

Evidence matrix.

Smoke matrix.

điểm chặn matrix.

Handoff matrix.

Dev task breakdown.

Ready/bị chặn status.

## 5.3. TECH-12 không quyết định implementation detail

TECH-12 không quyết định:

Tên bảng database.

Tên endpoint API.

Tên component UI.

Tên class/service.

Cách viết query.

Cách organize folder.

Cách inject dependency.

Cách deploy.

Cách migration chi tiết.

Cách viết automated test cụ thể.

Đó là phần developer quyết định trong codebase thật.

TECH-12 chỉ nói dev phải làm gì, theo rule nào, chứng minh bằng gì.

## 6. BACKLOG UNIT STANDARD

## 6.1. Một backlog item chuẩn phải có các trường

Mỗi backlog item trong TECH-12 phải có:

Backlog ID
Mã định danh duy nhất.

Phase
Thuộc PHASE 0 -> PHASE 9.

Domain
Foundation, Product, Operational, Commerce, AI, Gateway, Ads, MC Live, IVR, Release.

Source-of-Truth
TECH number + section/requirement.

Mục tiêu
Backlog này nhằm đạt điều gì.

Scope In
Những việc phải làm.

Scope Out
Những việc không làm.

Upstream Dependency
Phụ thuộc gì.

Downstream Handoff
Sau khi xong bàn giao cho ai/phase nào.

P0 điểm chặn
Lỗi cấm tuyệt đối.

Evidence Required
Cần bằng chứng gì.

Smoke Required
Cần smoke nào.

Done Gate
Khi nào backlog được coi là xong ở cấp implementation.

Fail Gate
Khi nào backlog bị fail.

Report Requirement
Dev phải báo cáo gì.

Initial Status
READY / bị chặn / NEED_SOURCE_MAPPING / NEED_DEPENDENCY_REVIEW.

## 6.2. Không được bỏ Scope Out

Scope Out là bắt buộc.

Vì nếu chỉ ghi Scope In, dev có thể tự mở rộng sai.

Ví dụ:

Nếu backlog là “QuoteSnapshot final price source”, Scope Out phải ghi rõ:

Không tự tính giá ở AI.

Không tự tính giá ở Gateway.

Không tự tính giá ở Live.

Không hardcode giá.

Không lấy cart/order draft làm final quote.

Không bypass Commerce Runtime.

## 6.3. Không được bỏ P0 điểm chặn

P0 điểm chặn là bắt buộc.

Vì P0 điểm chặn giúp dev biết lỗi nào không được vi phạm.

Ví dụ:

Backlog IVR phải ghi:

IVR không gọi Quote.

IVR không gọi Cart.

IVR không gọi Order Draft.

IVR không tự hủy đơn.

IVR không tự gửi SMS.

IVR confirmation không phải PAID.

## 6.4. Không được bỏ Evidence và Smoke

Nếu không có evidence và smoke, backlog không thể chứng minh đã xong.

Dev nói “đã làm xong” không đủ.

Backlog phải có evidence/smoke ngay từ lúc tạo, không đợi sau khi dev làm xong mới nghĩ cách test.

## 7. PHASE BACKLOG REGISTRY - TỔNG QUAN

TECH-12 chia backlog theo 10 phase:

PHASE 0 - Foundation / RBAC / Audit / Evidence / Idempotency

PHASE 1 - Product / SKU / Recipe / Product Activation

PHASE 2 - Operational Core

PHASE 3 - Commerce Runtime

PHASE 4 - AI Advisor Runtime

PHASE 5 - Facebook Gateway / Messenger / Channel Delivery

PHASE 6 - Ads Measurement

PHASE 7 - MC AI Live

PHASE 8 - IVR Order Confirmation

PHASE 9 - Global Smoke / UAT / Release Gateway

Mỗi phase sẽ có:

Phase backlog group.

Source-to-backlog matrix.

Dependency list.

Evidence list.

Smoke list.

P0 điểm chặn list.

Phase Done Gate.

Phase Fail Gate.

Phase Handoff.

## 8. PHASE 0 - FOUNDATION BACKLOG PRINCIPLES

## 8.1. Mục tiêu PHASE 0

PHASE 0 xây nền bắt buộc cho toàn bộ hệ thống.

Không có PHASE 0, các phase sau không đủ kiểm soát.

PHASE 0 bao gồm:

## RBAC.

Permission.

Actor identity.

Audit log.

Evidence registry.

Idempotency.

Correlation ID.

High-risk command control.

Admin override audit.

Source-of-truth registry nền.

## 8.2. Backlog nhóm của PHASE 0

PHASE 0 cần tối thiểu các backlog group:

FND-BLG-001 - RBAC / Permission Foundation.

FND-BLG-002 - Actor Identity / System Actor.

FND-BLG-003 - Audit Trail Foundation.

FND-BLG-004 - Evidence Registry Foundation.

FND-BLG-005 - Idempotency Foundation.

FND-BLG-006 - Correlation ID / Trace Context.

FND-BLG-007 - High-Risk Command Guard.

FND-BLG-008 - Admin Override Governance.

FND-BLG-009 - Source-of-Truth Registry Foundation.

FND-BLG-010 - Foundation Smoke / Evidence Pack.

## 8.3. P0 điểm chặn PHASE 0

FAIL nếu high-risk command không có audit.

FAIL nếu admin override không có reason/evidence/audit.

FAIL nếu evidence không có correlation/reference.

FAIL nếu permission không kiểm soát actor.

FAIL nếu phase sau chạy high-risk flow khi PHASE 0 chưa pass.

## 9. PHASE 1 - PRODUCT / SKU / RECIPE BACKLOG PRINCIPLES

## 9.1. Mục tiêu PHASE 1

PHASE 1 xây nền Product / SKU / Recipe / Activation.

Mục tiêu là đảm bảo sản phẩm, SKU, recipe, formula, activation guard rõ trước khi Operational và Commerce sử dụng.

PHASE 1 bao gồm:

Product Master.

SKU Master.

Recipe / Formula.

Formula kind/version.

Product Activation.

Claim/Product Data Approval.

Public product name rule.

Internal SKU code privacy.

Product knowledge source.

Product-active-not-sellable rule.

## 9.2. Backlog nhóm của PHASE 1

PHASE 1 cần tối thiểu:

PRD-BLG-001 - Product Master Governance.

PRD-BLG-002 - SKU Master Governance.

PRD-BLG-003 - Recipe / Formula Version Governance.

PRD-BLG-004 - Formula Kind / Version Lock.

PRD-BLG-005 - Product Activation Guard.

PRD-BLG-006 - Product Active Not Sellable Rule.

PRD-BLG-007 - Public Product Name Policy.

PRD-BLG-008 - Internal SKU Code Privacy.

PRD-BLG-009 - Product Knowledge Approval.

PRD-BLG-010 - Product/SKU/Recipe Smoke Pack.

## 9.3. P0 điểm chặn PHASE 1

FAIL nếu Product Active bị hiểu là Sellable.

FAIL nếu SKU nội bộ public ra khách.

FAIL nếu product knowledge chưa approved nhưng AI/Gateway/Live dùng.

FAIL nếu recipe/formula không version nhưng Production Order snapshot.

FAIL nếu product activation không có evidence/audit.

## 10. PHASE 2 - OPERATIONAL CORE BACKLOG PRINCIPLES

## 10.1. Mục tiêu PHASE 2

PHASE 2 xây Operational Core.

Đây là nền kiểm soát sản xuất, kho, tồn, traceability, recall, sale lock.

PHASE 2 bao gồm:

Source origin.

Raw material intake.

Raw QC.

Raw lot readiness.

Formula snapshot into production order.

Material request / issue / receipt.

Batch execution.

Packaging.

QC inspection.

Batch release.

Warehouse receipt.

Inventory ledger.

Trace/public trace.

Recall.

Sale Lock.

## 10.2. Backlog nhóm của PHASE 2

PHASE 2 cần tối thiểu:

OPS-BLG-001 - Source Origin / Raw Intake Governance.

OPS-BLG-002 - Raw QC / Raw Lot Readiness.

OPS-BLG-003 - Recipe Snapshot Into Production Order.

OPS-BLG-004 - Material Request / Issue / Receipt.

OPS-BLG-005 - Batch Execution / Process Step Control.

OPS-BLG-006 - Packaging / QR / Print Control.

OPS-BLG-007 - QC Inspection / Batch Release.

OPS-BLG-008 - Warehouse Receipt / Finished Goods Stock.

OPS-BLG-009 - Inventory Ledger Append-only.

OPS-BLG-010 - Traceability / Public Trace Whitelist.

OPS-BLG-011 - Recall / Sale Lock Control.

OPS-BLG-012 - Operational Smoke / Evidence Pack.

## 10.3. P0 điểm chặn PHASE 2

FAIL nếu RAW_LOT QC_PASS bị hiểu là READY_FOR_PRODUCTION.

FAIL nếu raw lot chưa READY_FOR_PRODUCTION vẫn issue.

FAIL nếu Material Issue không phải điểm giảm tồn nguyên liệu chính.

FAIL nếu Material Receipt giảm tồn lần hai.

FAIL nếu Batch QC_PASS bị hiểu là RELEASED.

FAIL nếu Warehouse nhận batch chưa RELEASED.

FAIL nếu Inventory Ledger không append-only.

FAIL nếu Public Trace không whitelist-only.

FAIL nếu Recall/Sale Lock không chặn downstream.

## 11. PHASE 3 - COMMERCE RUNTIME BACKLOG PRINCIPLES

## 11.1. Mục tiêu PHASE 3

PHASE 3 xây Commerce Runtime.

Đây là nền bán hàng, quote, cart, order, payment, shipping, verified revenue.

PHASE 3 bao gồm:

Sellable Gate.

QuoteSnapshot.

Cart.

Order Draft.

Customer Confirmation.

Official Order.

Payment boundary.

COD boundary.

Shipping boundary.

Verified Revenue.

Program/member/Diamond benefit runtime.

Order state machine.

## 11.2. Backlog nhóm của PHASE 3

PHASE 3 cần tối thiểu:

COM-BLG-001 - Sellable Gate From Operational.

COM-BLG-002 - QuoteSnapshot As Only Final Price Source.

COM-BLG-003 - Cart Not Order Boundary.

COM-BLG-004 - Order Draft Not Official Order Boundary.

COM-BLG-005 - Customer Confirmation To Official Order.

COM-BLG-006 - Official Order State Machine.

COM-BLG-007 - Payment Confirmation Boundary.

COM-BLG-008 - COD / Delivery / Shipping Boundary.

COM-BLG-009 - Verified Revenue Resolver.

COM-BLG-010 - Program / Member / Diamond Runtime Benefit.

COM-BLG-011 - Bank Account Registry / No Hardcode Payment Account.

COM-BLG-012 - Commerce Smoke / Evidence Pack.

## 11.3. P0 điểm chặn PHASE 3

FAIL nếu Operational bị chặn nhưng Commerce vẫn bán.

FAIL nếu không QuoteSnapshot mà vẫn final price.

FAIL nếu Cart bị hiểu là Order.

FAIL nếu Order Draft bị hiểu là Official Order.

FAIL nếu CustomerConfirmation chưa CONFIRMED mà tạo official order.

FAIL nếu ảnh chuyển khoản được coi là PAID.

FAIL nếu COD WAITING được coi là Verified Revenue.

FAIL nếu Verified Revenue không đến từ Commerce.

FAIL nếu payment account hardcode.

## 12. PHASE 4 - AI ADVISOR BACKLOG PRINCIPLES

## 12.1. Mục tiêu PHASE 4

PHASE 4 xây AI Advisor Runtime.

AI Advisor là lớp tư vấn, diễn giải, gợi ý và hỗ trợ chốt đơn có kiểm soát.

AI không phải nguồn sự thật nghiệp vụ.

PHASE 4 bao gồm:

Product consulting.

Customer memory.

Claim guard.

Product knowledge approved.

Quote-safe sales.

Order draft handoff.

Final Response Guard.

Public/private response boundary.

Runtime unavailable fail-safe.

No medical/therapeutic claim.

## 12.2. Backlog nhóm của PHASE 4

PHASE 4 cần tối thiểu:

AIA-BLG-001 - Product Knowledge Resolver.

AIA-BLG-002 - Claim Guard / Public Wording Policy.

AIA-BLG-003 - Customer Memory Resolver.

AIA-BLG-004 - QuoteSnapshot Consumption.

AIA-BLG-005 - Order Draft Handoff.

AIA-BLG-006 - Final Response Guard.

AIA-BLG-007 - Public/Private Data Boundary.

AIA-BLG-008 - Product Name Public Policy.

AIA-BLG-009 - Runtime Unavailable Fail-safe.

AIA-BLG-010 - AI Advisor Smoke / Evidence Pack.

## 12.3. P0 điểm chặn PHASE 4

FAIL nếu AI tự tính giá.

FAIL nếu AI tự tạo official order.

FAIL nếu AI tự xác nhận payment.

FAIL nếu AI tự xác nhận Verified Revenue.

FAIL nếu AI bán SKU not sellable/Sale Lock/Recall.

FAIL nếu AI dùng claim chưa approved.

FAIL nếu AI public dữ liệu private.

FAIL nếu AI nói chữa bệnh/điều trị/thay thuốc/cam kết hiệu quả y khoa.

## 13. PHASE 5 - FACEBOOK GATEWAY BACKLOG PRINCIPLES

## 13.1. Mục tiêu PHASE 5

PHASE 5 xây Facebook Gateway / Messenger / Channel Delivery.

Gateway là lớp điều phối kênh, không phải AI, không phải Commerce, không phải Payment.

PHASE 5 bao gồm:

Public/private routing.

Comment-to-Messenger.

Messenger handoff.

Delivery guard.

Final Response Guard integration.

Typing indicator / response pacing.

Rate limit.

Moderation.

Suppression.

No private data in public.

## 13.2. Backlog nhóm của PHASE 5

PHASE 5 cần tối thiểu:

GW-BLG-001 - Channel Identity / Page Context.

GW-BLG-002 - Public Comment Boundary.

GW-BLG-003 - Comment-to-Messenger Routing.

GW-BLG-004 - Messenger Private Context Handoff.

GW-BLG-005 - Final Response Guard Enforcement.

GW-BLG-006 - Typing Indicator / Response Pacing.

GW-BLG-007 - Rate Limit / Anti-Spam.

GW-BLG-008 - Moderation / Troll / Complaint Split.

GW-BLG-009 - Suppression / Opt-out.

GW-BLG-010 - Gateway Smoke / Evidence Pack.

## 13.3. P0 điểm chặn PHASE 5

FAIL nếu Gateway public dữ liệu private.

FAIL nếu Gateway public giá cá nhân/order info sai rule.

FAIL nếu Gateway gửi response chưa qua Final Response Guard.

FAIL nếu hỏi giá/mua/chốt trong public không kéo Messenger/private.

FAIL nếu troll/malicious bị kéo Messenger sai policy.

FAIL nếu complaint thật bị xử như troll.

FAIL nếu Gateway giả mạo người thật.

## 14. PHASE 6 - ADS MEASUREMENT BACKLOG PRINCIPLES

## 14.1. Mục tiêu PHASE 6

PHASE 6 xây Ads Measurement / Attribution / ROAS / Scale Gate.

Ads chỉ được dùng Verified Revenue từ Commerce.

Ads không được dùng quote, cart, order draft, unpaid order, payment WAITING, COD WAITING làm revenue.

PHASE 6 bao gồm:

Attribution.

Pixel/CAPI event governance.

Dedup/idempotency.

Funnel signals.

Verified Revenue.

## CPA/AOV/ROAS.

Data Quality.

Scale Gate.

Sale Lock/Recall suppression.

Ads/CRM/Diamond attribution conflict.

## 14.2. Backlog nhóm của PHASE 6

PHASE 6 cần tối thiểu:

ADS-BLG-001 - Event Taxonomy / Funnel Signal Boundary.

ADS-BLG-002 - Pixel / CAPI Dedup / Idempotency.

ADS-BLG-003 - Attribution Source Resolver.

ADS-BLG-004 - Verified Revenue Consumption.

ADS-BLG-005 - Exclusion Rules: Quote/Cart/Draft/Unpaid/WAITING.

ADS-BLG-006 - CPA / AOV / ROAS Computation Boundary.

ADS-BLG-007 - Data Quality Gate.

ADS-BLG-008 - Scale Gate / Owner Approval.

ADS-BLG-009 - Sale Lock / Recall / Suppression Blocks Scale.

ADS-BLG-010 - Ads Smoke / Evidence Pack.

## 14.3. P0 điểm chặn PHASE 6

FAIL nếu Ads dùng quote/cart/order draft làm revenue.

FAIL nếu unpaid order/payment WAITING/COD WAITING được tính revenue.

FAIL nếu Verified Revenue không từ Commerce.

FAIL nếu Pixel/CAPI double count.

FAIL nếu scale khi Data Quality fail.

FAIL nếu scale khi Sale Lock/Recall/Suppression active.

FAIL nếu scale chỉ vì ROAS đẹp nhưng evidence chưa accepted.

## 15. PHASE 7 - MC AI LIVE BACKLOG PRINCIPLES

## 15.1. Mục tiêu PHASE 7

PHASE 7 xây MC AI Live / Live Script Runtime / Live Sales Control.

MC AI Live không phải nguồn sự thật nghiệp vụ.

MC AI Live không tự báo giá, không tự tạo order, không tự xác nhận payment/revenue.

PHASE 7 bao gồm:

Live script runtime.

Host intelligence.

Live session context.

Script guard.

Live comment classification.

Comment-to-Messenger handoff.

Live sales control.

Troll/abuse handling.

Complaint route.

Ads-safe live orchestration.

## 15.2. Backlog nhóm của PHASE 7

PHASE 7 cần tối thiểu:

LIVE-BLG-001 - Live Session Context Resolver.

LIVE-BLG-002 - Live Script Runtime.

LIVE-BLG-003 - Script Guard / Claim Guard.

LIVE-BLG-004 - Live Comment Classifier.

LIVE-BLG-005 - Comment-to-Messenger Coordination.

LIVE-BLG-006 - Product/Price Boundary In Live.

LIVE-BLG-007 - Troll / Abuse / Malicious Handling.

LIVE-BLG-008 - Complaint / CSKH / Quality Route.

LIVE-BLG-009 - Ads-safe Live Signal Boundary.

LIVE-BLG-010 - MC AI Live Smoke / Evidence Pack.

## 15.3. P0 điểm chặn PHASE 7

FAIL nếu MC AI Live tự báo giá.

FAIL nếu MC AI Live tự tạo order.

FAIL nếu MC AI Live tự xác nhận payment/revenue.

FAIL nếu live signal dùng làm ROAS.

FAIL nếu script chưa guard vẫn dùng.

FAIL nếu claim chưa approved vẫn nói.

FAIL nếu troll/malicious bị kéo Messenger sai policy.

FAIL nếu complaint thật bị xử như troll.

## 16. PHASE 8 - IVR ORDER CONFIRMATION BACKLOG PRINCIPLES

## 16.1. Mục tiêu PHASE 8

PHASE 8 xây IVR Order Confirmation.

IVR chỉ xử lý Official Order đủ điều kiện theo Commerce.

IVR không gọi đại trà.

IVR không xác nhận payment/revenue.

PHASE 8 bao gồm:

IVR eligibility.

Customer trust.

Anti-fake-order.

Official Order only.

Phone validation.

Attempt policy.

Outcome classification.

Core Order State Machine handoff.

Notification Owner handoff.

Privacy-safe call result.

## 16.2. Backlog nhóm của PHASE 8

PHASE 8 cần tối thiểu:

IVR-BLG-001 - IVR Eligibility Resolver.

IVR-BLG-002 - Customer Trust / Existing Trusted Exclusion.

IVR-BLG-003 - Official Order Only Boundary.

IVR-BLG-004 - Phone Validation / Invalid Phone.

IVR-BLG-005 - Call Attempt Policy.

IVR-BLG-006 - Outcome Classification.

IVR-BLG-007 - Customer Confirm / Cancel / Need Support.

IVR-BLG-008 - No Answer / Max Attempts.

IVR-BLG-009 - Core Order State / Notification Handoff.

IVR-BLG-010 - IVR Evidence / Audit / Smoke Pack.

## 16.3. P0 điểm chặn PHASE 8

FAIL nếu IVR gọi mọi khách.

FAIL nếu IVR gọi Quote/Cart/Order Draft.

FAIL nếu IVR tự tạo order.

FAIL nếu IVR tự hủy đơn.

FAIL nếu IVR tự gửi notification.

FAIL nếu IVR confirmation bị hiểu là PAID.

FAIL nếu invalid phone/no answer/customer cancel/technical failure bị trộn.

FAIL nếu Notification gửi trước Core Order State Machine hủy chính thức.

## 17. PHASE 9 - GLOBAL RELEASE BACKLOG PRINCIPLES

## 17.1. Mục tiêu PHASE 9

PHASE 9 xây Global Smoke / UAT / Evidence / Release Gateway.

PHASE 9 không triển khai nghiệp vụ domain.

PHASE 9 kiểm soát:

Evidence.

Smoke.

## UAT.

Owner sign-off.

Cross-tech dependency.

P0 điểm chặn.

Privacy/security.

Rollback/fallback.

Monitoring/alert.

Release decision.

Global Gateway.

Post-go-live monitoring.

Scale approval.

## 17.2. Backlog nhóm của PHASE 9

PHASE 9 cần tối thiểu:

REL-BLG-001 - Documentation Completion Registry.

REL-BLG-002 - Evidence Intake / Acceptance.

REL-BLG-003 - Global Smoke Matrix.

REL-BLG-004 - UAT Session Control.

REL-BLG-005 - Owner Sign-off Control.

REL-BLG-006 - P0 điểm chặn Registry.

REL-BLG-007 - Cross-Tech Dependency Validator.

REL-BLG-008 - Privacy / Security Release Review.

REL-BLG-009 - Rollback / Fallback / Monitoring Readiness.

REL-BLG-010 - Release Decision / Global Gateway State.

REL-BLG-011 - Post-Go-Live Monitoring.

REL-BLG-012 - Scale Approval Gate.

## 17.3. P0 điểm chặn PHASE 9

FAIL nếu Documentation Complete bị gọi là Production Ready.

FAIL nếu evidence chưa accepted nhưng Completion Decision.

FAIL nếu smoke chưa pass nhưng Release Ready.

FAIL nếu owner chưa sign-off nhưng Release Approved.

FAIL nếu không release decision nhưng Go-live Approved.

FAIL nếu Global Gateway mở khi còn điểm chặn.

FAIL nếu post-go-live incident open nhưng scale.

## 18. SOURCE-TO-BACKLOG MATRIX STANDARD

## 18.1. Mẫu matrix chuẩn

Mỗi backlog trong TECH-12 phải theo mẫu:

Trường

Nội dung bắt buộc

Backlog ID

Mã duy nhất

Phase

## PHASE 0 -> PHASE 9

Domain

Foundation/Product/Operational/Commerce/AI/Gateway/Ads/Live/IVR/Release

Source-of-Truth

TECH number + section/requirement

Requirement Summary

Tóm tắt yêu cầu

Scope In

Việc phải làm

Scope Out

Việc không làm

Upstream Dependency

Phụ thuộc

Downstream Handoff

Bàn giao

P0 điểm chặn

Lỗi cấm

Evidence Required

Bằng chứng

Smoke Required

Smoke

Done Gate

Điều kiện xong

Fail Gate

Điều kiện fail

Initial Status

READY/bị chặn/NEED_REVIEW

## 18.2. Matrix không được thiếu source

Nếu thiếu Source-of-Truth:

Initial Status = NEED_SOURCE_MAPPING

Không được READY.

## 18.3. Matrix không được thiếu dependency

Nếu thiếu dependency:

Initial Status = NEED_DEPENDENCY_REVIEW

Không được READY.

## 18.4. Matrix không được thiếu evidence/smoke

Nếu thiếu evidence/smoke:

Initial Status = NEED_EVIDENCE_SMOKE

Không được READY.

## 19. BACKLOG READY RULE

Một backlog chỉ được READY khi đủ:

Source-of-truth rõ.

Requirement rõ.

Phase rõ.

Scope In rõ.

Scope Out rõ.

Dependency rõ.

P0 điểm chặn rõ.

Evidence rõ.

Smoke rõ.

Done Gate rõ.

Fail Gate rõ.

Owner rõ.

Handoff target rõ.

Nếu thiếu bất kỳ điểm nào:

Backlog không READY.

## 20. BACKLOG bị chặn RULE

Một backlog phải bị chặn nếu:

Source-of-truth chưa rõ.

Source conflict chưa xử lý.

Upstream phase bị chặn.

Dependency chưa pass.

P0 điểm chặn đang mở.

Evidence/smoke chưa xác định.

Owner chưa rõ.

Handoff chưa rõ.

Scope quá rộng.

Scope mơ hồ.

Có nguy cơ hardcode policy.

Có nguy cơ copy-paste code rời rạc.

## 21. P0 điểm chặn REGISTRY - PHẦN 1/4

Các lỗi sau là P0 điểm chặn của TECH-12 PHẦN 1/4:

Backlog không có source-of-truth.

Backlog không có TECH/section/requirement.

Backlog mơ hồ vẫn READY.

Backlog không có Scope Out.

Backlog không có dependency.

Backlog không có evidence.

Backlog không có smoke.

Backlog không có Done Gate.

Backlog không có Fail Gate.

Backlog downstream READY khi upstream bị chặn.

Backlog cho phép hardcode policy.

Backlog cho phép copy-paste code rời rạc.

Backlog dùng tài liệu cũ làm source thật.

Backlog dùng code cũ làm source thật.

Backlog chuyển thẳng thành code handoff khi chưa review.

Backlog gọi implementation task là release-ready.

Backlog không bàn giao TECH-10 trước release.

Backlog không có owner.

Backlog không có report requirement.

Backlog không có smoke/fail-case cho requirement critical.

## 22. EVIDENCE YÊU CẦU CHO PHẦN 1/4

## PHẦN 1/4 yêu cầu evidence ở cấp tài liệu gồm:

TECH-12 purpose accepted.

Source-to-backlog boundary accepted.

No-Unmapped-Task Rule accepted.

Backlog Unit Standard accepted.

Phase Backlog Registry overview accepted.

PHASE 0 backlog principles accepted.

PHASE 1 backlog principles accepted.

PHASE 2 backlog principles accepted.

PHASE 3 backlog principles accepted.

PHASE 4 backlog principles accepted.

PHASE 5 backlog principles accepted.

PHASE 6 backlog principles accepted.

PHASE 7 backlog principles accepted.

PHASE 8 backlog principles accepted.

PHASE 9 backlog principles accepted.

Source-to-Backlog Matrix Standard accepted.

Backlog Ready Rule accepted.

Backlog bị chặn Rule accepted.

P0 điểm chặn Registry accepted.

Release handoff sang PHẦN 2/4 accepted.

## 23. SMOKE ĐỊNH HƯỚNG - PHẦN 1/4

TECH12-P1-SMK-001 - Backlog Không Source Bị Chặn

Given backlog không có TECH/section/requirement
When Source-to-Backlog Matrix review
Then backlog = NEED_SOURCE_MAPPING, không READY.

TECH12-P1-SMK-002 - Backlog Mơ Hồ Bị Reject

Given backlog ghi “làm AI” hoặc “làm bán hàng”
When Backlog Ready Rule kiểm tra
Then backlog bị reject hoặc NEED_SCOPE_REVIEW.

TECH12-P1-SMK-003 - Backlog Thiếu Scope Out Không Ready

Given backlog chỉ có Scope In
When Backlog Governance review
Then backlog không READY.

TECH12-P1-SMK-004 - Backlog Thiếu Evidence/Smoke Không Ready

Given backlog chưa có evidence/smoke
When Ready Rule kiểm tra
Then backlog = NEED_EVIDENCE_SMOKE.

TECH12-P1-SMK-005 - Upstream bị chặn Chặn Downstream

Given PHASE 2 Operational bị chặn
When PHASE 3 Commerce backlog xét Ready
Then backlog downstream = bị chặn.

TECH12-P1-SMK-006 - Product Active Không Được Thành Sellable

Given PHASE 1 Product Active backlog
When bàn giao sang Commerce
Then phải có backlog Operational Sellable Gate ở PHASE 2/3, không dùng Product Active làm Sellable.

TECH12-P1-SMK-007 - QuoteSnapshot Chặn AI Tự Tính Giá

Given AI Advisor backlog muốn báo giá
When PHASE 3 QuoteSnapshot chưa pass
Then AI quote backlog bị chặn.

TECH12-P1-SMK-008 - Verified Revenue Chặn Ads Scale

Given Ads Scale backlog
When Verified Revenue chưa pass
Then Ads Scale backlog bị chặn.

TECH12-P1-SMK-009 - Official Order Chặn IVR

Given IVR call backlog
When Official Order/Core Order State chưa pass
Then IVR backlog bị chặn.

TECH12-P1-SMK-010 - TECH-10 Chặn Release

Given phase backlog đã dev done
When chưa qua TECH-10 evidence/smoke/sign-off/release decision
Then không Release Ready, Global Gateway vẫn bị chặn.

## 24. DONE GATE - PHẦN 1/4

## PHẦN 1/4 đạt Documentation Complete ở cấp nguyên tắc khi:

Đã khóa mục tiêu TECH-12.

Đã khóa vai trò TECH-12 trong bộ TECH.

Đã khóa TECH-12 không phải tài liệu code.

Đã khóa No-Unmapped-Task Rule.

Đã khóa Source-to-Backlog Boundary.

Đã khóa Backlog Unit Standard.

Đã khóa Phase Backlog Registry tổng quan.

Đã khóa PHASE 0 principles.

Đã khóa PHASE 1 principles.

Đã khóa PHASE 2 principles.

Đã khóa PHASE 3 principles.

Đã khóa PHASE 4 principles.

Đã khóa PHASE 5 principles.

Đã khóa PHASE 6 principles.

Đã khóa PHASE 7 principles.

Đã khóa PHASE 8 principles.

Đã khóa PHASE 9 principles.

Đã khóa Source-to-Backlog Matrix Standard.

Đã khóa Backlog Ready Rule.

Đã khóa Backlog bị chặn Rule.

Đã có P0 điểm chặn Registry.

Đã có Evidence Requirement.

Đã có Smoke định hướng.

Đã có Release Handoff sang PHẦN 2/4.

## 25. FAIL GATE - PHẦN 1/4

## PHẦN 1/4 FAIL nếu:

TECH-12 bị hiểu là tài liệu code.

TECH-12 cho phép backlog không source.

TECH-12 cho phép task mơ hồ.

TECH-12 không có Source-to-Backlog Boundary.

TECH-12 không có Backlog Unit Standard.

TECH-12 không chia phase.

TECH-12 thiếu PHASE 0.

TECH-12 thiếu PHASE 9.

TECH-12 thiếu evidence/smoke rule.

TECH-12 cho backlog downstream Ready khi upstream bị chặn.

TECH-12 cho code cũ/tài liệu cũ thắng TECH mới.

TECH-12 cho copy-paste code rời rạc.

TECH-12 cho hardcode policy.

TECH-12 không có P0 điểm chặn Registry.

TECH-12 không có Done Gate.

TECH-12 không có Fail Gate.

TECH-12 không có Release Handoff.

## 26. RELEASE HANDOFF - SANG PHẦN 2/4

## PHẦN 1/4 bàn giao sang PHẦN 2/4 các nguyên tắc đã khóa:

TECH-12 là Phase Backlog Pack.

TECH-12 không viết code.

Mọi backlog phải map source-of-truth.

Không task nào không source được Ready.

Mỗi backlog phải có Scope In/Scope Out.

Mỗi backlog phải có dependency.

Mỗi backlog phải có P0 điểm chặn.

Mỗi backlog phải có evidence.

Mỗi backlog phải có smoke.

Mỗi backlog phải có Done Gate/Fail Gate.

Phase backlog chia theo PHASE 0 -> PHASE 9.

Downstream bị chặn nếu upstream chưa pass.

TECH-12 không quyết định implementation detail.

TECH-12 không tự tạo Release Ready.

TECH-12 phải bàn giao sang TECH-11/TECH-10 theo đúng gate.

## PHẦN 2/4 sẽ viết:

## PHASE BACKLOG MODULE CONTRACTS / SOURCE-TO-BACKLOG MATRIX CONTRACTS / DEV TASK BREAKDOWN GOVERNANCE

Các module cần khóa ở PHẦN 2/4:

Phase Backlog Orchestrator.

Backlog Item Factory.

Source-to-Backlog Matrix Resolver.

Phase Dependency Matrix Resolver.

Backlog Readiness Resolver.

Backlog điểm chặn Resolver.

Dev Task Breakdown Resolver.

Evidence Requirement Resolver.

Smoke Requirement Resolver.

Owner Assignment Resolver.

Report Requirement Resolver.

Phase Backlog Review Resolver.

Backlog-to-Handoff Resolver.

Backlog Change Control Resolver.

Release Handoff Resolver.

## 27. TRẠNG THÁI SAU PHẦN 1/4

Sau PHẦN 1/4:

Chưa được gọi là Documentation Complete.

Chưa được gọi là Backlog Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 đã khóa nguyên tắc nền của TECH-12.

Các điểm cốt lõi đã được cố định:

TECH-12 là Phase Backlog Pack.

TECH-12 không phải tài liệu code.

TECH-12 không thay thế TECH-00 -> TECH-11.

TECH-12 biến source-of-truth thành backlog cụ thể.

Không backlog nào được Ready nếu không map TECH/section/requirement.

Không task nào được giao dev nếu không có source rõ.

Mỗi backlog phải có Scope In/Scope Out/Dependency/P0/Evidence/Smoke/Done Gate/Fail Gate.

Backlog mơ hồ bị reject.

Downstream backlog bị bị chặn nếu upstream chưa pass.

Tài liệu cũ không thắng TECH mới.

Code cũ không thắng TECH mới.

TECH-12 không quyết định API/DB/UI/code detail.

Dev quyết định cách làm trong codebase thật.

Owner quyết định hệ thống muốn gì.

AI hỗ trợ chia backlog, checklist, prompt, review; không thay developer.

PHASE 0 -> PHASE 9 đã được định nghĩa ở cấp backlog principles.

Backlog Ready Rule đã khóa.

Backlog bị chặn Rule đã khóa.

P0 điểm chặn Registry đã khóa.

Global Gateway vẫn bị chặn nếu chưa qua evidence/smoke/sign-off/release decision.

## PHẦN 1/4 sẵn sàng bàn giao sang:

## PHẦN 2/4 - PHASE BACKLOG MODULE CONTRACTS / SOURCE-TO-BACKLOG MATRIX CONTRACTS / DEV TASK BREAKDOWN GOVERNANCE.

## PHẦN 2/4 - PHASE BACKLOG MODULE CONTRACTS / SOURCE-TO-BACKLOG MATRIX CONTRACTS / DEV TASK BREAKDOWN GOVERNANCE

## 1. MỤC TIÊU PHẦN 2/4

## PHẦN 2/4 khóa các module contract để quản trị việc tạo backlog, chia task, map source-of-truth, kiểm tra dependency, gắn evidence, gắn smoke và bàn giao dev.

PHẦN này trả lời:

Ai tạo backlog.

Backlog được tạo theo chuẩn nào.

Source-of-truth được map ra sao.

Dependency giữa phase/backlog được kiểm soát thế nào.

Backlog khi nào Ready.

Backlog khi nào bị chặn.

Backlog được chia thành dev task thế nào.

Evidence requirement được gắn thế nào.

Smoke requirement được gắn thế nào.

Owner được gán thế nào.

Dev report requirement được gắn thế nào.

Phase backlog được review ra sao.

Backlog được chuyển sang handoff cho dev như thế nào.

Thay đổi backlog được kiểm soát ra sao.

Backlog được bàn giao sang TECH-11/TECH-10 như thế nào.

## PHẦN 2/4 không viết code.

## PHẦN 2/4 không thiết kế API chi tiết.

## PHẦN 2/4 không thiết kế database chi tiết.

## PHẦN 2/4 không thiết kế UI chi tiết.

## PHẦN 2/4 chỉ khóa module contract cho Phase Backlog Pack.

## 2. NGUYÊN TẮC MODULE CONTRACT CHUNG

Mọi module trong TECH-12 phải tuân thủ:

Không tạo backlog không source.

Không tạo backlog mơ hồ.

Không cho backlog thiếu Scope Out.

Không cho backlog thiếu dependency.

Không cho backlog thiếu P0 điểm chặn.

Không cho backlog thiếu evidence.

Không cho backlog thiếu smoke.

Không cho backlog thiếu owner.

Không cho backlog thiếu report requirement.

Không cho backlog downstream Ready khi upstream bị chặn.

Không cho backlog chuyển thẳng thành code handoff.

Không cho backlog dùng tài liệu cũ làm source thật.

Không cho backlog dùng code cũ làm source thật.

Không cho backlog hardcode policy.

Không cho backlog copy-paste code rời rạc.

Không cho backlog gọi implementation task là Release Ready.

Không cho backlog bypass TECH-11.

Không cho backlog bypass TECH-10.

Không mở Global Gateway từ TECH-12.

Không để dev tự suy luận.

## 3. MODULE CONTRACT 01 - PHASE BACKLOG ORCHESTRATOR

## 3.1. Mục tiêu

Phase Backlog Orchestrator điều phối toàn bộ backlog theo PHASE 0 -> PHASE 9.

Module này giữ vai trò trung tâm trong TECH-12, bảo đảm mỗi phase có backlog đúng source, đúng dependency, đúng evidence, đúng smoke và đúng handoff.

## 3.2. Scope In

Module được phép:

Tạo danh sách phase backlog.

Gắn mỗi backlog vào phase.

Kiểm tra phase có đủ backlog group tối thiểu chưa.

Tổng hợp trạng thái backlog trong phase.

Nhận trạng thái Ready / bị chặn / Need Review của từng backlog.

Điều phối source mapping, dependency, evidence, smoke.

Xác định phase backlog nào được đưa sang review.

Bàn giao phase backlog sang TECH-11 Dev Handoff Governance.

Bàn giao release-level backlog sang TECH-10 khi đủ điều kiện.

## 3.3. Scope Out

Module không được:

Tự viết code.

Tự thiết kế API/DB/UI chi tiết.

Tự sửa nghiệp vụ.

Tự bỏ source mapping.

Tự cho backlog Ready khi thiếu requirement.

Tự cho downstream Ready khi upstream bị chặn.

Tự đánh Implementation Ready.

Tự đánh Release Ready.

Tự mở Global Gateway.

## 3.4. Upstream Dependency

Phụ thuộc:

## TECH-00 -> TECH-11.

Phase Backlog Registry.

Source-to-Backlog Matrix Resolver.

Phase Dependency Matrix Resolver.

Backlog Readiness Resolver.

Backlog điểm chặn Resolver.

## 3.5. Downstream Handoff

Bàn giao cho:

Phase Backlog Review Resolver.

Backlog-to-Handoff Resolver.

TECH-11 Dev Handoff Governance.

TECH-10 Global Release Governance nếu phase đủ điều kiện release review.

## 3.6. P0 điểm chặn

FAIL nếu phase backlog thiếu source-of-truth mà vẫn được Ready.

FAIL nếu PHASE 3 Commerce backlog Ready khi PHASE 2 Operational bị chặn.

FAIL nếu PHASE 6 Ads Scale backlog Ready khi Verified Revenue chưa pass.

FAIL nếu PHASE 8 IVR backlog Ready khi Official Order/Core Order State chưa pass.

## 3.7. Evidence

Evidence tối thiểu:

Phase backlog list.

Backlog status summary.

Source mapping summary.

Dependency summary.

bị chặn backlog list.

Ready backlog list.

Handoff record.

Reviewer note.

## 3.8. Smoke

## TECH12-MOD-SMK-001

Given PHASE 3 có backlog Commerce nhưng PHASE 2 chưa pass
When Phase Backlog Orchestrator tổng hợp
Then PHASE 3 release-level backlog phải bị chặn.

## 4. MODULE CONTRACT 02 - BACKLOG ITEM FACTORY

## 4.1. Mục tiêu

Backlog Item Factory tạo backlog item theo chuẩn bắt buộc của TECH-12.

Module này không được tạo task mơ hồ.

## 4.2. Scope In

Module được phép tạo backlog item với các trường:

Backlog ID.

Phase.

Domain.

Source-of-Truth.

Requirement Summary.

Scope In.

Scope Out.

Upstream Dependency.

Downstream Handoff.

P0 điểm chặn.

Evidence Required.

Smoke Required.

Done Gate.

Fail Gate.

Report Requirement.

Initial Status.

## 4.3. Scope Out

Module không được:

Tạo backlog thiếu TECH reference.

Tạo backlog thiếu Scope Out.

Tạo backlog thiếu dependency.

Tạo backlog thiếu evidence/smoke.

Tạo backlog kiểu “làm cho chạy”.

Tạo backlog kiểu “dev tự phân tích”.

Tạo backlog cho code snippet.

Tạo backlog hardcode policy.

## 4.4. Upstream Dependency

Phụ thuộc:

TECH source-of-truth.

Phase registry.

Domain registry.

Backlog Unit Standard.

Source-to-Backlog Matrix Resolver.

## 4.5. Downstream Handoff

Bàn giao:

Draft backlog item.

Missing field list.

Initial status.

Handoff sang Backlog Readiness Resolver.

## 4.6. P0 điểm chặn

FAIL nếu tạo backlog không có Source-of-Truth.

FAIL nếu tạo backlog không có Done Gate / Fail Gate.

FAIL nếu tạo backlog không có P0 điểm chặn.

FAIL nếu tạo backlog cho phép copy-paste code.

## 4.7. Evidence

Evidence tối thiểu:

Backlog ID.

Required field checklist.

Missing field list.

Initial status decision.

Reviewer.

## 4.8. Smoke

## TECH12-MOD-SMK-002

Given backlog draft thiếu Scope Out và Evidence Required
When Backlog Item Factory tạo item
Then item không được Ready, phải NEED_REVIEW.

## 5. MODULE CONTRACT 03 - SOURCE-TO-BACKLOG MATRIX RESOLVER

## 5.1. Mục tiêu

Source-to-Backlog Matrix Resolver map từng backlog item với TECH source-of-truth.

Module này bảo đảm không backlog nào không nguồn được giao dev.

## 5.2. Scope In

Module được phép:

Map backlog với TECH number.

Map backlog với section/requirement.

Map backlog với phase.

Map backlog với domain.

Ghi requirement summary.

Ghi source conflict nếu có.

Ghi legacy/current implementation deviation nếu có.

Xác định source mapping accepted/rejected.

Đưa backlog thiếu source về NEED_SOURCE_MAPPING.

## 5.3. Scope Out

Module không được:

Cho backlog không source Ready.

Cho tài liệu cũ thắng TECH mới.

Cho code cũ thắng TECH mới.

Tự chọn source theo cảm tính.

Bỏ qua source conflict.

Chấp nhận backlog “dev tự phân tích thêm” làm source.

## 5.4. Upstream Dependency

Phụ thuộc:

## TECH-00 -> TECH-11.

Legacy reference registry nếu có.

Current implementation review nếu có.

Backlog Item Factory.

## 5.5. Downstream Handoff

Bàn giao:

Source mapped backlog.

Source conflict list.

Need source mapping list.

Rejected backlog list.

Handoff sang Backlog Readiness Resolver.

## 5.6. P0 điểm chặn

FAIL nếu backlog không có TECH/section/requirement vẫn Ready.

FAIL nếu code cũ được dùng để phủ định TECH mới.

FAIL nếu source conflict không mở điểm chặn.

## 5.7. Evidence

Evidence tối thiểu:

Backlog ID.

TECH reference.

Section/requirement.

Source status.

Conflict note nếu có.

Reviewer decision.

## 5.8. Smoke

## TECH12-MOD-SMK-003

Given backlog “AI báo giá cho khách” nhưng không map TECH-04 QuoteSnapshot và TECH-05 AI Advisor
When Source-to-Backlog Resolver kiểm tra
Then backlog không Ready.

## 6. MODULE CONTRACT 04 - PHASE DEPENDENCY MATRIX RESOLVER

## 6.1. Mục tiêu

Phase Dependency Matrix Resolver kiểm soát dependency giữa phase và giữa backlog.

Module này bảo đảm downstream không vượt upstream.

## 6.2. Scope In

Module được phép kiểm tra:

PHASE 0 dependency với toàn hệ thống.

PHASE 1 dependency với PHASE 2.

PHASE 2 dependency với PHASE 3.

PHASE 3 dependency với PHASE 4/6/8.

PHASE 4 dependency với PHASE 5.

PHASE 5 dependency với PHASE 7.

PHASE 9 dependency với release/go-live/scale.

Dependency theo rule Sale Lock / Recall.

Dependency theo Verified Revenue.

Dependency theo QuoteSnapshot.

Dependency theo Official Order/Core Order State.

Dependency theo Final Response Guard.

## 6.3. Scope Out

Module không được:

Cho downstream Ready khi upstream bị chặn.

Cho mock upstream làm production truth.

Cho Ads scale khi Verified Revenue chưa pass.

Cho AI quote khi QuoteSnapshot chưa pass.

Cho IVR call khi Official Order chưa pass.

Cho MC Live go-live khi Gateway/AI/Commerce chưa pass.

Hạ dependency fail thành warning nhẹ.

## 6.4. Upstream Dependency

Phụ thuộc:

Phase status.

Backlog status.

Evidence/smoke status nếu có.

Source mapping.

TECH dependency registry.

## 6.5. Downstream Handoff

Bàn giao:

Dependency pass list.

Dependency fail list.

bị chặn backlog list.

Upstream missing list.

Handoff sang Backlog Readiness Resolver.

## 6.6. P0 điểm chặn

FAIL nếu PHASE 3 Commerce Ready khi PHASE 2 Operational Sellable Gate chưa pass.

FAIL nếu PHASE 6 Ads Ready khi Verified Revenue chưa pass.

FAIL nếu PHASE 8 IVR Ready khi Official Order/Core Order State chưa pass.

## 6.7. Evidence

Evidence tối thiểu:

Dependency pair.

Upstream phase/backlog.

Downstream phase/backlog.

Dependency status.

Block reason.

Reviewer.

## 6.8. Smoke

## TECH12-MOD-SMK-004

Given PHASE 6 Ads Scale backlog
When Verified Revenue backlog chưa pass
Then Ads Scale backlog phải bị chặn.

## 7. MODULE CONTRACT 05 - BACKLOG READINESS RESOLVER

## 7.1. Mục tiêu

Backlog Readiness Resolver quyết định backlog có được đưa vào trạng thái Ready hay không.

## 7.2. Scope In

Module được phép kiểm tra:

Source-of-truth.

Scope In.

Scope Out.

Dependency.

P0 điểm chặn.

Evidence.

Smoke.

Owner.

Done Gate.

Fail Gate.

Report Requirement.

Handoff target.

Initial status.

Risk of hardcode.

Risk of copy-paste code.

## 7.3. Scope Out

Module không được:

Cho backlog thiếu field critical Ready.

Cho backlog mơ hồ Ready.

Cho backlog downstream Ready khi upstream bị chặn.

Cho backlog Ready nếu source conflict còn mở.

Cho backlog Ready nếu có nguy cơ hardcode policy chưa xử lý.

Cho backlog Ready nếu evidence/smoke chưa rõ.

## 7.4. Upstream Dependency

Phụ thuộc:

Backlog Item Factory.

Source-to-Backlog Matrix Resolver.

Phase Dependency Matrix Resolver.

Owner Assignment Resolver.

Evidence/Smoke Requirement Resolver.

## 7.5. Downstream Handoff

Bàn giao:

## BACKLOG_READY.

## BACKLOG_BLOCKED.

## NEED_SOURCE_MAPPING.

## NEED_DEPENDENCY_REVIEW.

## NEED_EVIDENCE_SMOKE.

## NEED_OWNER_ASSIGNMENT.

## NEED_SCOPE_REVIEW.

Handoff sang Backlog-to-Handoff Resolver.

## 7.6. P0 điểm chặn

FAIL nếu backlog thiếu evidence/smoke nhưng Ready.

FAIL nếu backlog không owner nhưng Ready.

FAIL nếu backlog thiếu Fail Gate nhưng Ready.

## 7.7. Evidence

Evidence tối thiểu:

Backlog readiness checklist.

Missing field list.

Ready/bị chặn decision.

Reason code.

Reviewer.

## 7.8. Smoke

## TECH12-MOD-SMK-005

Given backlog đủ source nhưng thiếu owner và smoke
When Readiness Resolver chạy
Then backlog không được Ready.

## 8. MODULE CONTRACT 06 - BACKLOG điểm chặn RESOLVER

## 8.1. Mục tiêu

Backlog điểm chặn Resolver quản lý các nguyên nhân khiến backlog bị bị chặn.

## 8.2. Scope In

Module được phép:

Ghi nhận điểm chặn.

Phân loại điểm chặn.

Gắn điểm chặn với backlog.

Gắn điểm chặn với phase.

Gắn điểm chặn với owner.

Gắn điểm chặn với source conflict.

Gắn điểm chặn với dependency fail.

Gắn điểm chặn với missing evidence/smoke.

Gắn điểm chặn với P0 risk.

Theo dõi xử lý điểm chặn.

Yêu cầu review/retest nếu cần.

Đóng điểm chặn khi đủ evidence.

## 8.3. Scope Out

Module không được:

Đóng điểm chặn không evidence.

Hạ cấp P0 tùy tiện.

Ẩn điểm chặn khỏi backlog.

Cho backlog Ready khi điểm chặn còn open.

Cho phase handoff khi điểm chặn critical chưa xử lý.

Dùng “owner đồng ý miệng” để đóng điểm chặn.

## 8.4. Upstream Dependency

Phụ thuộc:

Backlog Readiness Resolver.

Source-to-Backlog Matrix Resolver.

Phase Dependency Matrix Resolver.

Evidence/Smoke Resolver.

Owner Assignment Resolver.

## 8.5. Downstream Handoff

Bàn giao:

Open điểm chặn list.

Resolved điểm chặn list.

bị chặn backlog list.

Required action list.

Handoff sang Phase Backlog Review Resolver.

## 8.6. P0 điểm chặn

FAIL nếu backlog có P0 điểm chặn mở nhưng vẫn Ready.

FAIL nếu điểm chặn source conflict bị bỏ qua.

FAIL nếu điểm chặn dependency fail bị ghi là warning.

## 8.7. Evidence

Evidence tối thiểu:

điểm chặn ID.

Backlog ID.

Phase.

Severity.

Reason.

Owner.

Resolution evidence.

Reviewer decision.

## 8.8. Smoke

## TECH12-MOD-SMK-006

Given backlog IVR thiếu Official Order dependency
When điểm chặn Resolver chạy
Then backlog bị BLOCKED_BY_DEPENDENCY.

## 9. MODULE CONTRACT 07 - DEV TASK BREAKDOWN RESOLVER

## 9.1. Mục tiêu

Dev Task Breakdown Resolver chia backlog group thành các dev task nhỏ hơn nhưng vẫn giữ source, dependency, evidence và smoke.

Module này giúp dev triển khai có trật tự, không nhận task quá lớn hoặc mơ hồ.

## 9.2. Scope In

Module được phép:

Chia backlog group thành dev task.

Giữ mapping TECH/requirement cho từng task.

Gắn task với phase.

Gắn task với dependency.

Gắn task với evidence.

Gắn task với smoke.

Gắn task với report requirement.

Xác định task sequencing.

Xác định task có thể chuẩn bị song song.

Xác định task không được coding nếu thiếu context.

## 9.3. Scope Out

Module không được:

Chia task làm mất source.

Chia task làm mất P0 điểm chặn.

Chia task quá mơ hồ.

Chia task thành code snippet rời rạc.

Chia task để bypass dependency.

Cho task không evidence/smoke.

Cho task không report requirement.

Cho dev tự quyết business rule.

## 9.4. Upstream Dependency

Phụ thuộc:

Backlog Ready.

Source mapping.

Dependency matrix.

Evidence requirement.

Smoke requirement.

Owner assignment.

## 9.5. Downstream Handoff

Bàn giao:

Dev task list.

Task sequence.

Task dependency.

Task evidence.

Task smoke.

Handoff sang TECH-11 Dev Handoff Resolver.

## 9.6. P0 điểm chặn

FAIL nếu task breakdown làm mất source-of-truth.

FAIL nếu task chia nhỏ nhưng bỏ P0 điểm chặn.

FAIL nếu task biến thành “copy đoạn code này”.

## 9.7. Evidence

Evidence tối thiểu:

Backlog ID.

Dev task IDs.

Source mapping per task.

Dependency per task.

Evidence/smoke per task.

Reviewer decision.

## 9.8. Smoke

## TECH12-MOD-SMK-007

Given backlog “QuoteSnapshot as only final price source”
When task breakdown chạy
Then mọi task con vẫn phải giữ rule “AI/Gateway/Live không tự tính giá”.

## 10. MODULE CONTRACT 08 - EVIDENCE REQUIREMENT RESOLVER

## 10.1. Mục tiêu

Evidence Requirement Resolver xác định bằng chứng cần có cho từng backlog/task.

## 10.2. Scope In

Module được phép xác định:

Evidence loại tài liệu.

Evidence loại build/test output.

Evidence loại log.

Evidence loại screenshot/export nếu phù hợp.

Evidence loại audit/correlation.

Evidence migration/seed nếu áp dụng.

Evidence source mapping.

Evidence smoke result.

Evidence QA handoff.

Evidence privacy/security nếu có dữ liệu nhạy cảm.

## 10.3. Scope Out

Module không được:

Cho backlog thiếu evidence Ready.

Chấp nhận lời nói thay evidence.

Chấp nhận evidence không environment.

Chấp nhận evidence không requirement.

Chấp nhận evidence lộ secret/private data.

Chấp nhận local evidence cho production readiness.

Bỏ evidence high-risk command.

## 10.4. Upstream Dependency

Phụ thuộc:

Backlog item.

Requirement type.

Phase.

Domain risk.

TECH-10 Evidence Boundary.

Privacy/Security rule nếu liên quan.

## 10.5. Downstream Handoff

Bàn giao:

Evidence requirement list.

Evidence format.

Evidence owner.

Evidence review requirement.

Handoff sang Backlog Readiness Resolver và TECH-11 Evidence Mapping.

## 10.6. P0 điểm chặn

FAIL nếu high-risk backlog không yêu cầu audit/evidence.

FAIL nếu payment/order/IVR/Ads revenue task không evidence.

FAIL nếu privacy-sensitive task không privacy evidence.

## 10.7. Evidence

Evidence tối thiểu của module:

Backlog ID.

Evidence requirement list.

Evidence owner.

Evidence format.

Review status.

## 10.8. Smoke

## TECH12-MOD-SMK-008

Given backlog liên quan Verified Revenue
When Evidence Requirement Resolver chạy
Then phải yêu cầu evidence từ Commerce source, không chỉ dashboard Ads.

## 11. MODULE CONTRACT 09 - SMOKE REQUIREMENT RESOLVER

## 11.1. Mục tiêu

Smoke Requirement Resolver xác định smoke bắt buộc cho từng backlog/task.

## 11.2. Scope In

Module được phép xác định:

Smoke pass-case.

Smoke fail-case.

Smoke P0 case.

Smoke runtime unavailable.

Smoke privacy/security.

Smoke dependency.

Smoke cross-phase.

Smoke upstream bị chặn.

Smoke no-bypass.

Smoke release handoff nếu liên quan.

## 11.3. Scope Out

Module không được:

Cho backlog thiếu smoke Ready.

Chỉ tạo happy path smoke cho requirement critical.

Bỏ P0 smoke.

Bỏ fail-safe smoke.

Bỏ dependency smoke.

Bỏ privacy smoke khi có dữ liệu khách.

Bỏ revenue/payment smoke khi liên quan Commerce/Ads.

## 11.4. Upstream Dependency

Phụ thuộc:

Backlog item.

Requirement risk level.

Phase.

P0 điểm chặn list.

TECH smoke rules.

TECH-10 Global Smoke Boundary.

## 11.5. Downstream Handoff

Bàn giao:

Smoke requirement list.

Smoke ID draft.

Smoke Given/When/Then draft.

Must Not list.

Evidence required for smoke.

Handoff sang Backlog Readiness Resolver và TECH-11 Smoke Mapping.

## 11.6. P0 điểm chặn

FAIL nếu IVR backlog không có smoke invalid phone/no answer/customer cancel/technical failure.

FAIL nếu Ads backlog không có smoke fake revenue exclusion.

FAIL nếu Gateway backlog không có smoke public/private boundary.

## 11.7. Evidence

Evidence tối thiểu:

Backlog ID.

Smoke requirement list.

P0 smoke list.

Fail-case list.

Reviewer decision.

## 11.8. Smoke

## TECH12-MOD-SMK-009

Given backlog Gateway public comment
When Smoke Requirement Resolver chạy
Then phải có smoke “public không lộ private data”.

## 12. MODULE CONTRACT 10 - OWNER ASSIGNMENT RESOLVER

## 12.1. Mục tiêu

Owner Assignment Resolver gắn owner cho từng backlog/task.

Không backlog nào Ready nếu không có owner phù hợp.

## 12.2. Scope In

Module được phép xác định:

Business owner.

Dev owner.

QA owner.

Evidence reviewer.

Smoke reviewer.

Domain owner.

Privacy/security owner nếu cần.

Release owner nếu backlog release-level.

Cross-phase dependency owner.

Handoff receiver.

## 12.3. Scope Out

Module không được:

Cho backlog không owner Ready.

Gán owner không đúng domain.

Cho dev tự làm owner nghiệp vụ.

Cho QA tự ký owner sign-off.

Bỏ Privacy/Legal owner khi có dữ liệu nhạy cảm.

Bỏ Release owner khi backlog có release impact.

## 12.4. Upstream Dependency

Phụ thuộc:

Backlog domain.

Phase.

Risk level.

Data sensitivity.

Release impact.

TECH owner registry.

## 12.5. Downstream Handoff

Bàn giao:

Owner assignment list.

Missing owner list.

Owner conflict list.

Handoff sang Backlog Readiness Resolver.

## 12.6. P0 điểm chặn

FAIL nếu backlog payment/revenue không có Commerce/Payment owner.

FAIL nếu backlog public/private data không có Privacy/Security owner.

FAIL nếu backlog release gate không có Release owner.

## 12.7. Evidence

Evidence tối thiểu:

Backlog ID.

Owner list.

Owner role.

Assignment status.

Reviewer.

## 12.8. Smoke

## TECH12-MOD-SMK-010

Given backlog IVR Notification Handoff
When Owner Assignment chạy
Then phải có IVR Owner, Commerce/Core Order Owner, Notification Owner và Privacy Owner nếu có dữ liệu khách.

## 13. MODULE CONTRACT 11 - REPORT REQUIREMENT RESOLVER

## 13.1. Mục tiêu

Report Requirement Resolver gắn chuẩn báo cáo dev cho từng backlog/task.

Module này đảm bảo dev report luôn theo format đã khóa.

## 13.2. Scope In

Module được phép yêu cầu report gồm:

Tóm tắt.

File đã sửa.

Nguồn yêu cầu.

Evidence đã dùng.

Lệnh đã chạy.

Kết quả test.

Backend build result.

Frontend build result.

Cleanup result.

Markdown/docs update.

Database migration/update nếu áp dụng.

Seed validation nếu áp dụng.

Rủi ro còn lại.

Handoff update.

## 13.3. Scope Out

Module không được:

Cho task không có report requirement Ready.

Cho dev báo “đã xong” thay report.

Bỏ qua test/build output.

Bỏ qua migration/seed note nếu có.

Bỏ qua risk note.

Bỏ qua handoff update.

## 13.4. Upstream Dependency

Phụ thuộc:

Backlog item.

Phase.

Implementation impact.

TECH-11 report format.

Migration/seed risk nếu có.

## 13.5. Downstream Handoff

Bàn giao:

Report requirement list.

Required sections.

Optional sections.

Not-applicable sections.

Handoff sang Dev Task Breakdown và TECH-11 Implementation Report Resolver.

## 13.6. P0 điểm chặn

FAIL nếu backlog không yêu cầu dev report.

FAIL nếu report requirement không bắt test/build.

FAIL nếu migration/seed impact không được yêu cầu báo cáo.

## 13.7. Evidence

Evidence tối thiểu:

Backlog ID.

Report template.

Required sections.

Reviewer decision.

## 13.8. Smoke

## TECH12-MOD-SMK-011

Given backlog có database impact
When Report Requirement Resolver chạy
Then report phải yêu cầu migration/update và rollback/seed note nếu áp dụng.

## 14. MODULE CONTRACT 12 - PHASE BACKLOG REVIEW RESOLVER

## 14.1. Mục tiêu

Phase Backlog Review Resolver kiểm tra toàn bộ backlog của một phase trước khi chuyển sang TECH-11 Dev Handoff.

## 14.2. Scope In

Module được phép review:

Phase có đủ backlog group tối thiểu chưa.

Backlog nào Ready.

Backlog nào bị chặn.

Backlog nào Need Review.

Dependency phase.

Evidence coverage.

Smoke coverage.

Owner coverage.

Report requirement coverage.

P0 điểm chặn coverage.

Release impact.

Handoff readiness.

## 14.3. Scope Out

Module không được:

Cho phase backlog pass khi thiếu backlog critical.

Cho phase backlog pass khi nhiều item mơ hồ.

Cho phase backlog pass khi dependency chưa rõ.

Cho phase backlog pass khi evidence/smoke coverage thiếu.

Cho phase backlog pass khi owner thiếu.

Cho phase backlog pass khi P0 điểm chặn chưa định nghĩa.

## 14.4. Upstream Dependency

Phụ thuộc:

Phase Backlog Orchestrator.

Backlog Readiness Resolver.

Dependency Matrix Resolver.

Evidence/Smoke Requirement Resolver.

Owner Assignment Resolver.

Backlog điểm chặn Resolver.

## 14.5. Downstream Handoff

Bàn giao:

## PHASE_BACKLOG_REVIEW_PASS.

## PHASE_BACKLOG_REVIEW_FAIL.

Missing backlog list.

bị chặn backlog list.

Handoff sang Backlog-to-Handoff Resolver.

## 14.6. P0 điểm chặn

FAIL nếu PHASE 0 thiếu audit/evidence/idempotency backlog mà vẫn pass.

FAIL nếu PHASE 3 thiếu QuoteSnapshot/Payment/Verified Revenue backlog mà vẫn pass.

FAIL nếu PHASE 9 thiếu Global Gateway/Release Decision backlog mà vẫn pass.

## 14.7. Evidence

Evidence tối thiểu:

Phase backlog review checklist.

Backlog coverage report.

Evidence/smoke coverage report.

Missing/bị chặn list.

Reviewer decision.

## 14.8. Smoke

## TECH12-MOD-SMK-012

Given PHASE 3 Commerce backlog thiếu Verified Revenue
When Phase Backlog Review chạy
Then PHASE 3 review fail.

## 15. MODULE CONTRACT 13 - BACKLOG-TO-HANDOFF RESOLVER

## 15.1. Mục tiêu

Backlog-to-Handoff Resolver chuyển backlog Ready thành handoff package cho TECH-11 Dev Handoff Governance.

## 15.2. Scope In

Module được phép tạo handoff package gồm:

Phase.

Backlog IDs.

Source-of-truth.

Requirement summary.

Scope In.

Scope Out.

Dependency.

P0 điểm chặn.

Evidence required.

Smoke required.

Owner.

Report requirement.

Initial implementation status.

Handoff target.

## 15.3. Scope Out

Module không được:

Handoff backlog chưa Ready.

Handoff backlog còn source conflict.

Handoff backlog thiếu dependency.

Handoff backlog thiếu evidence/smoke.

Handoff backlog downstream khi upstream bị chặn.

Gọi handoff là code instruction.

Gọi handoff là Release Ready.

## 15.4. Upstream Dependency

Phụ thuộc:

Backlog Readiness Resolver.

Phase Backlog Review Resolver.

Dependency Matrix Resolver.

Owner Assignment Resolver.

Evidence/Smoke Requirement Resolver.

## 15.5. Downstream Handoff

Bàn giao:

Handoff package.

Ready backlog list.

bị chặn backlog list.

Handoff sang TECH-11 Dev Handoff Resolver.

## 15.6. P0 điểm chặn

FAIL nếu handoff backlog thiếu Scope Out.

FAIL nếu handoff backlog thiếu P0 điểm chặn.

FAIL nếu handoff bypass TECH-11.

## 15.7. Evidence

Evidence tối thiểu:

Handoff package ID.

Backlog list.

Source mapping.

Dependency status.

Evidence/smoke list.

Reviewer decision.

## 15.8. Smoke

## TECH12-MOD-SMK-013

Given backlog READY nhưng thiếu report requirement
When Backlog-to-Handoff Resolver chạy
Then handoff package phải bị reject.

## 16. MODULE CONTRACT 14 - BACKLOG CHANGE CONTROL RESOLVER

## 16.1. Mục tiêu

Backlog Change Control Resolver kiểm soát mọi thay đổi backlog sau khi đã Ready hoặc đã handoff.

## 16.2. Scope In

Module được phép kiểm soát:

Thêm backlog mới.

Sửa scope backlog.

Sửa dependency.

Sửa evidence requirement.

Sửa smoke requirement.

Sửa owner.

Sửa priority.

Hủy backlog.

Tách backlog.

Gộp backlog.

Đánh dấu superseded.

Route owner review khi thay đổi ảnh hưởng source-of-truth.

## 16.3. Scope Out

Module không được:

Sửa backlog không audit.

Sửa scope để bỏ P0 điểm chặn.

Sửa dependency để bypass upstream.

Xóa evidence/smoke requirement cho dễ làm.

Đổi owner critical không review.

Gộp backlog làm mất source mapping.

Tách backlog làm mất P0 điểm chặn.

Thay đổi backlog sau handoff mà không báo dev/QA.

## 16.4. Upstream Dependency

Phụ thuộc:

Backlog status.

Source mapping.

Dependency status.

Owner assignment.

Handoff status.

## 16.5. Downstream Handoff

Bàn giao:

Change request.

Change approved/rejected.

Updated backlog.

Superseded backlog note.

Handoff update required.

QA/review notification.

## 16.6. P0 điểm chặn

FAIL nếu backlog thay đổi sau handoff mà dev/QA không được cập nhật.

FAIL nếu change control bỏ evidence/smoke.

FAIL nếu change control làm mất source mapping.

## 16.7. Evidence

Evidence tối thiểu:

Change request ID.

Backlog ID.

Before/after.

Impact.

Approver.

Audit note.

## 16.8. Smoke

## TECH12-MOD-SMK-014

Given backlog đã handoff cho dev
When scope bị thay đổi
Then phải tạo change record và cập nhật handoff, không sửa âm thầm.

## 17. MODULE CONTRACT 15 - RELEASE HANDOFF RESOLVER

## 17.1. Mục tiêu

Release Handoff Resolver bàn giao Phase Backlog Pack sang TECH-11/TECH-10 theo đúng gate.

Module này không được tự gọi backlog ready là release ready.

## 17.2. Scope In

Module được phép:

Nhận phase backlog review pass.

Kiểm tra handoff package.

Kiểm tra source mapping.

Kiểm tra dependency.

Kiểm tra evidence/smoke requirement.

Kiểm tra owner/report requirement.

Ghi backlog ready for TECH-11 handoff.

Ghi release-level backlog cần TECH-10 review.

Ghi bị chặn backlog.

Ghi risks.

## 17.3. Scope Out

Module không được:

Tự đánh Release Ready.

Tự đánh Production Ready.

Tự đánh Go-live Approved.

Tự mở Global Gateway.

Bypass TECH-11.

Bypass TECH-10.

Gửi backlog chưa Ready sang dev.

Gửi backlog release-level sang go-live trực tiếp.

## 17.4. Upstream Dependency

Phụ thuộc:

Phase Backlog Review Resolver.

Backlog-to-Handoff Resolver.

Backlog Change Control Resolver.

Backlog điểm chặn Resolver.

## TECH-11.

## TECH-10.

## 17.5. Downstream Handoff

Bàn giao:

Ready for TECH-11 Dev Handoff.

Ready for TECH-10 Release Review nếu đã qua implementation/evidence/smoke ở các bước sau.

bị chặn backlog list.

Risk list.

Owner review required list.

## 17.6. P0 điểm chặn

FAIL nếu Release Handoff gọi Backlog Ready là Release Ready.

FAIL nếu Release Handoff bỏ qua TECH-11.

FAIL nếu Release Handoff bỏ qua TECH-10.

FAIL nếu Release Handoff mở Global Gateway.

## 17.7. Evidence

Evidence tối thiểu:

Release handoff checklist.

Backlog ready list.

bị chặn list.

Risk list.

Handoff target.

Reviewer decision.

## 17.8. Smoke

## TECH12-MOD-SMK-015

Given phase backlog review pass
When Release Handoff chạy
Then status chỉ là Ready for TECH-11 Dev Handoff, không Release Ready.

## 18. CROSS-MODULE DEPENDENCY MAP

## 18.1. Luồng chuẩn

Luồng chuẩn của TECH-12:

Phase Backlog Orchestrator xác định phase.

Backlog Item Factory tạo backlog item.

Source-to-Backlog Matrix Resolver map source.

Phase Dependency Matrix Resolver kiểm tra dependency.

Evidence Requirement Resolver gắn evidence.

Smoke Requirement Resolver gắn smoke.

Owner Assignment Resolver gắn owner.

Report Requirement Resolver gắn report requirement.

Backlog Readiness Resolver quyết định Ready/bị chặn.

Backlog điểm chặn Resolver quản lý điểm chặn.

Dev Task Breakdown Resolver chia task.

Phase Backlog Review Resolver review toàn phase.

Backlog-to-Handoff Resolver tạo handoff package.

Backlog Change Control Resolver kiểm soát thay đổi.

Release Handoff Resolver bàn giao sang TECH-11/TECH-10.

## 18.2. No-bypass rule

Không module nào được bỏ qua:

Source mapping.

Dependency review.

Evidence requirement.

Smoke requirement.

Owner assignment.

Report requirement.

Readiness review.

điểm chặn review.

Phase backlog review.

Handoff governance.

Change control.

TECH-11/TECH-10 gate.

## 18.3. Dependency ưu tiên

Nếu có mâu thuẫn:

Source-of-truth mới thắng backlog mơ hồ.

Dependency fail thắng readiness.

P0 điểm chặn thắng handoff.

Evidence/smoke missing thắng task urgency.

Owner missing thắng backlog Ready.

TECH-11 thắng dev handoff claim.

TECH-10 thắng release claim.

Global Gateway mặc định bị chặn.

## 19. MODULE P0 điểm chặn REGISTRY - PHẦN 2/4

Các lỗi sau là P0 điểm chặn cấp module:

Phase Backlog Orchestrator cho downstream Ready khi upstream bị chặn.

Backlog Item Factory tạo backlog thiếu source.

Source-to-Backlog Matrix accepted backlog không TECH/section/requirement.

Phase Dependency Matrix cho mock upstream làm production truth.

Backlog Readiness cho item thiếu evidence/smoke Ready.

Backlog điểm chặn Resolver đóng điểm chặn không evidence.

Dev Task Breakdown làm mất source mapping.

Evidence Requirement Resolver bỏ evidence cho high-risk task.

Smoke Requirement Resolver bỏ P0 smoke.

Owner Assignment Resolver cho backlog không owner Ready.

Report Requirement Resolver bỏ test/build report.

Phase Backlog Review pass khi thiếu backlog critical.

Backlog-to-Handoff gửi backlog chưa Ready sang dev.

Backlog Change Control sửa scope không audit.

Release Handoff gọi Backlog Ready là Release Ready.

Module nào cho copy-paste code rời rạc.

Module nào cho hardcode policy.

Module nào bỏ TECH-11.

Module nào bỏ TECH-10.

Module nào tự mở Global Gateway.

## 20. EVIDENCE PACKAGE - PHẦN 2/4

## PHẦN 2/4 yêu cầu evidence thiết kế ở cấp module gồm:

Phase Backlog Orchestrator contract accepted.

Backlog Item Factory contract accepted.

Source-to-Backlog Matrix Resolver contract accepted.

Phase Dependency Matrix Resolver contract accepted.

Backlog Readiness Resolver contract accepted.

Backlog điểm chặn Resolver contract accepted.

Dev Task Breakdown Resolver contract accepted.

Evidence Requirement Resolver contract accepted.

Smoke Requirement Resolver contract accepted.

Owner Assignment Resolver contract accepted.

Report Requirement Resolver contract accepted.

Phase Backlog Review Resolver contract accepted.

Backlog-to-Handoff Resolver contract accepted.

Backlog Change Control Resolver contract accepted.

Release Handoff Resolver contract accepted.

Cross-module dependency accepted.

Module P0 điểm chặn Registry accepted.

Smoke matrix định hướng accepted.

Release handoff checklist prepared.

## 21. SMOKE MATRIX ĐỊNH HƯỚNG - PHẦN 2/4

TECH12-P2-SMK-001 - Backlog Item Factory Reject Missing Source

Given backlog draft không có Source-of-Truth
When Backlog Item Factory kiểm tra
Then item không được Ready.

TECH12-P2-SMK-002 - Source Mapping Reject Legacy Source

Given backlog dùng tài liệu cũ làm source thật
When Source-to-Backlog Matrix Resolver kiểm tra
Then source mapping rejected.

TECH12-P2-SMK-003 - Dependency Blocks Downstream

Given PHASE 3 Commerce backlog phụ thuộc PHASE 2
When PHASE 2 bị chặn
Then PHASE 3 backlog bị chặn.

TECH12-P2-SMK-004 - Evidence Required For High-Risk

Given backlog Payment Confirmation
When Evidence Requirement Resolver chạy
Then phải yêu cầu evidence/audit.

TECH12-P2-SMK-005 - Smoke Required For P0

Given backlog IVR Outcome Classification
When Smoke Requirement Resolver chạy
Then phải có smoke invalid phone/no answer/customer cancel/technical failure.

TECH12-P2-SMK-006 - Owner Required

Given backlog Notification Handoff
When Owner Assignment Resolver chạy
Then phải có Notification Owner và Privacy Owner nếu có dữ liệu khách.

TECH12-P2-SMK-007 - Report Required

Given backlog có implementation impact
When Report Requirement Resolver chạy
Then phải yêu cầu đủ 14 mục report.

TECH12-P2-SMK-008 - Phase Review Fails Missing Critical Backlog

Given PHASE 9 thiếu Release Decision / Global Gateway backlog
When Phase Backlog Review chạy
Then phase review fail.

TECH12-P2-SMK-009 - Handoff Rejects Not Ready Backlog

Given backlog còn NEED_DEPENDENCY_REVIEW
When Backlog-to-Handoff Resolver chạy
Then không được handoff dev.

TECH12-P2-SMK-010 - Change Control Required

Given backlog đã handoff nhưng scope thay đổi
When Change Control kiểm tra
Then phải có change record và cập nhật handoff.

TECH12-P2-SMK-011 - Release Handoff Not Release Ready

Given phase backlog review pass
When Release Handoff chạy
Then chỉ Ready for TECH-11 Dev Handoff, không Release Ready.

## 22. DONE GATE - PHẦN 2/4

## PHẦN 2/4 đạt Documentation Complete ở cấp module contract khi:

Đã khóa Phase Backlog Orchestrator.

Đã khóa Backlog Item Factory.

Đã khóa Source-to-Backlog Matrix Resolver.

Đã khóa Phase Dependency Matrix Resolver.

Đã khóa Backlog Readiness Resolver.

Đã khóa Backlog điểm chặn Resolver.

Đã khóa Dev Task Breakdown Resolver.

Đã khóa Evidence Requirement Resolver.

Đã khóa Smoke Requirement Resolver.

Đã khóa Owner Assignment Resolver.

Đã khóa Report Requirement Resolver.

Đã khóa Phase Backlog Review Resolver.

Đã khóa Backlog-to-Handoff Resolver.

Đã khóa Backlog Change Control Resolver.

Đã khóa Release Handoff Resolver.

Mỗi module có Scope In.

Mỗi module có Scope Out.

Mỗi module có Upstream Dependency.

Mỗi module có Downstream Handoff.

Mỗi module có P0 điểm chặn.

Mỗi module có Evidence.

Mỗi module có Smoke.

Có Cross-Module Dependency Map.

Có Module P0 điểm chặn Registry.

Có Evidence Package cấp phần.

Có Smoke Matrix định hướng.

Sẵn sàng chuyển sang PHẦN 3/4.

## 23. FAIL GATE - PHẦN 2/4

## PHẦN 2/4 FAIL nếu:

Thiếu module contract bắt buộc.

Có module chưa có Scope In.

Có module chưa có Scope Out.

Có module chưa có Upstream Dependency.

Có module chưa có Downstream Handoff.

Có module chưa có P0 điểm chặn.

Có module chưa có Evidence.

Có module chưa có Smoke.

Có module cho backlog không source Ready.

Có module cho downstream Ready khi upstream bị chặn.

Có module bỏ evidence/smoke.

Có module bỏ owner.

Có module bỏ report requirement.

Có module bỏ Phase Backlog Review.

Có module handoff backlog chưa Ready.

Có module sửa backlog không change record.

Có module gọi Backlog Ready là Release Ready.

Có module bypass TECH-11.

Có module bypass TECH-10.

Có module tự mở Global Gateway.

## 24. RELEASE HANDOFF - SANG PHẦN 3/4

## PHẦN 2/4 bàn giao sang PHẦN 3/4 các nội dung đã khóa:

Danh sách module backlog governance đầy đủ.

Boundary từng module.

Dependency từng module.

Handoff từng module.

P0 điểm chặn từng module.

Evidence từng module.

Smoke từng module.

Cross-module dependency map.

Backlog item creation rule.

Source-to-backlog mapping rule.

Phase dependency matrix rule.

Backlog readiness rule.

Backlog điểm chặn rule.

Dev task breakdown rule.

Evidence requirement rule.

Smoke requirement rule.

Owner assignment rule.

Report requirement rule.

Phase backlog review rule.

Change control rule.

Release handoff rule.

## PHẦN 3/4 sẽ viết:

## BACKLOG LIFECYCLE / TASK STATE MACHINE / SOURCE-TO-BACKLOG-TO-HANDOFF FLOW / PHASE DEPENDENCY CONTROL

Trong đó phải khóa rõ:

Backlog lifecycle.

Task state machine.

Source mapping state model.

Dependency matrix state model.

Evidence requirement state model.

Smoke requirement state model.

Owner assignment state model.

Backlog readiness state model.

điểm chặn state model.

Task breakdown state model.

Handoff state model.

Change control state model.

Release handoff state model.

Fail-safe backlog control.

## 25. TRẠNG THÁI SAU PHẦN 2/4

Sau PHẦN 2/4:

## PHẦN 1/4 đã khóa backlog principles.

## PHẦN 2/4 đã khóa module contracts.

Chưa được gọi là Documentation Complete.

Chưa được gọi là Backlog Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 đã khóa đầy đủ module contracts cho TECH-12.

Các module trọng yếu đã được cố định:

Phase Backlog Orchestrator giữ cấu trúc backlog theo PHASE 0 -> PHASE 9.

Backlog Item Factory tạo backlog theo chuẩn bắt buộc.

Source-to-Backlog Matrix Resolver chặn backlog không source.

Phase Dependency Matrix Resolver chặn downstream khi upstream bị chặn.

Backlog Readiness Resolver quyết định Ready/bị chặn/Need Review.

Backlog điểm chặn Resolver quản lý điểm chặn.

Dev Task Breakdown Resolver chia task nhưng không làm mất source/evidence/smoke.

Evidence Requirement Resolver gắn bằng chứng bắt buộc.

Smoke Requirement Resolver gắn smoke bắt buộc.

Owner Assignment Resolver gắn owner đúng domain.

Report Requirement Resolver bắt dev report đúng format.

Phase Backlog Review Resolver kiểm tra toàn phase.

Backlog-to-Handoff Resolver tạo package sang TECH-11.

Backlog Change Control Resolver kiểm soát thay đổi.

Release Handoff Resolver chỉ bàn giao, không tự gọi Release Ready.

## PHẦN 2/4 sẵn sàng bàn giao sang:

## PHẦN 3/4 - BACKLOG LIFECYCLE / TASK STATE MACHINE / SOURCE-TO-BACKLOG-TO-HANDOFF FLOW / PHASE DEPENDENCY CONTROL.

## PHẦN 3/4 - BACKLOG LIFECYCLE / TASK STATE MACHINE / SOURCE-TO-BACKLOG-TO-HANDOFF FLOW / PHASE DEPENDENCY CONTROL

## 1. MỤC TIÊU PHẦN 3/4

## PHẦN 3/4 khóa vòng đời của backlog từ lúc một yêu cầu còn nằm trong TECH source-of-truth cho đến khi được tạo thành backlog item, review source, review dependency, gắn owner, gắn evidence, gắn smoke, chia task, đưa vào handoff cho dev và bàn giao sang TECH-11.

PHẦN này xác định rõ:

Backlog đi qua những trạng thái nào.

Task đi qua những trạng thái nào.

Source-to-backlog mapping vận hành ra sao.

Dependency matrix kiểm soát backlog như thế nào.

Evidence requirement được sinh, review và accepted ra sao.

Smoke requirement được sinh, review và accepted ra sao.

Owner assignment được kiểm tra thế nào.

Backlog readiness được quyết định ra sao.

điểm chặn được mở, xử lý, retest và đóng thế nào.

Dev task breakdown được tạo thế nào.

Backlog-to-handoff flow sang TECH-11 vận hành thế nào.

Backlog change control vận hành ra sao.

Release handoff sang TECH-10 bị giới hạn như thế nào.

Fail-safe backlog control vận hành khi thiếu source, thiếu dependency, thiếu evidence hoặc thiếu smoke.

## PHẦN 3/4 không viết code.

## PHẦN 3/4 không thiết kế API chi tiết.

## PHẦN 3/4 không thiết kế database chi tiết.

## PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa backlog lifecycle, task state machine, source-to-backlog flow, handoff flow và phase dependency control.

## 2. NGUYÊN TẮC LIFECYCLE CHUNG

Toàn bộ lifecycle backlog của TECH-12 phải tuân thủ:

Không backlog nào được Ready nếu không có source-of-truth.

Không backlog nào được Ready nếu thiếu Scope In / Scope Out.

Không backlog nào được Ready nếu thiếu dependency.

Không backlog nào được Ready nếu thiếu P0 điểm chặn.

Không backlog nào được Ready nếu thiếu evidence requirement.

Không backlog nào được Ready nếu thiếu smoke requirement.

Không backlog nào được Ready nếu thiếu owner.

Không backlog nào được Ready nếu thiếu report requirement.

Không backlog downstream nào được Ready nếu upstream critical bị chặn.

Không backlog nào được handoff dev nếu còn source conflict.

Không backlog nào được handoff dev nếu còn dependency unresolved.

Không backlog nào được gọi là Dev Ready nếu chưa qua TECH-11 Dev Handoff Governance.

Không backlog nào được gọi là Release Ready nếu chưa qua TECH-10.

Không backlog nào được dùng để viết code copy-paste rời rạc từ AI.

Không backlog nào được hardcode policy nghiệp vụ.

Không backlog nào được dùng tài liệu cũ hoặc code cũ để thắng TECH mới.

Không backlog nào được bypass evidence/smoke.

Không backlog nào được mở Global Gateway.

## 3. BACKLOG LIFECYCLE STATE MODEL

## 3.1. Danh sách trạng thái backlog

Mỗi backlog item trong TECH-12 có các trạng thái sau:

BACKLOG_NOT_CREATED
Backlog chưa được tạo.

BACKLOG_DRAFTING
Backlog đang được soạn nháp.

BACKLOG_NEED_SOURCE_MAPPING
Backlog chưa có source-of-truth rõ.

BACKLOG_SOURCE_MAPPING_IN_PROGRESS
Đang map backlog với TECH source-of-truth.

BACKLOG_SOURCE_MAPPED
Đã map source-of-truth.

BACKLOG_SOURCE_CONFLICT
Có mâu thuẫn giữa TECH mới, tài liệu cũ hoặc code cũ.

BACKLOG_NEED_SCOPE_REVIEW
Scope In / Scope Out chưa rõ.

BACKLOG_NEED_DEPENDENCY_REVIEW
Dependency chưa rõ hoặc upstream chưa pass.

BACKLOG_NEED_OWNER_ASSIGNMENT
Chưa có owner phù hợp.

BACKLOG_NEED_EVIDENCE_REQUIREMENT
Chưa có evidence requirement.

BACKLOG_NEED_SMOKE_REQUIREMENT
Chưa có smoke requirement.

BACKLOG_NEED_REPORT_REQUIREMENT
Chưa có yêu cầu báo cáo dev.

BACKLOG_READINESS_REVIEW
Đang kiểm tra Ready/bị chặn.

BACKLOG_BLOCKED
Bị chặn do thiếu source, thiếu dependency, thiếu owner, thiếu evidence/smoke hoặc có P0 điểm chặn.

BACKLOG_READY_FOR_TASK_BREAKDOWN
Backlog đủ điều kiện để chia dev task.

BACKLOG_TASK_BREAKDOWN_IN_PROGRESS
Đang chia backlog thành task triển khai.

BACKLOG_TASK_BREAKDOWN_ACCEPTED
Task breakdown đã accepted.

BACKLOG_READY_FOR_TECH11_HANDOFF
Sẵn sàng chuyển sang TECH-11 Dev Handoff Governance.

BACKLOG_HANDOFF_TO_TECH11
Đã bàn giao sang TECH-11.

BACKLOG_SUPERSEDED
Backlog bị thay thế bởi backlog mới.

BACKLOG_CANCELLED
Backlog bị hủy có lý do.

## 3.2. Transition hợp lệ

Các transition hợp lệ:

## BACKLOG_NOT_CREATED -> BACKLOG_DRAFTING.

## BACKLOG_DRAFTING -> BACKLOG_NEED_SOURCE_MAPPING.

## BACKLOG_NEED_SOURCE_MAPPING -> BACKLOG_SOURCE_MAPPING_IN_PROGRESS.

## BACKLOG_SOURCE_MAPPING_IN_PROGRESS -> BACKLOG_SOURCE_MAPPED.

## BACKLOG_SOURCE_MAPPING_IN_PROGRESS -> BACKLOG_SOURCE_CONFLICT.

## BACKLOG_SOURCE_MAPPED -> BACKLOG_NEED_SCOPE_REVIEW.

## BACKLOG_NEED_SCOPE_REVIEW -> BACKLOG_NEED_DEPENDENCY_REVIEW.

## BACKLOG_NEED_DEPENDENCY_REVIEW -> BACKLOG_NEED_OWNER_ASSIGNMENT.

## BACKLOG_NEED_OWNER_ASSIGNMENT -> BACKLOG_NEED_EVIDENCE_REQUIREMENT.

## BACKLOG_NEED_EVIDENCE_REQUIREMENT -> BACKLOG_NEED_SMOKE_REQUIREMENT.

## BACKLOG_NEED_SMOKE_REQUIREMENT -> BACKLOG_NEED_REPORT_REQUIREMENT.

## BACKLOG_NEED_REPORT_REQUIREMENT -> BACKLOG_READINESS_REVIEW.

## BACKLOG_READINESS_REVIEW -> BACKLOG_BLOCKED.

## BACKLOG_READINESS_REVIEW -> BACKLOG_READY_FOR_TASK_BREAKDOWN.

## BACKLOG_READY_FOR_TASK_BREAKDOWN -> BACKLOG_TASK_BREAKDOWN_IN_PROGRESS.

## BACKLOG_TASK_BREAKDOWN_IN_PROGRESS -> BACKLOG_TASK_BREAKDOWN_ACCEPTED.

## BACKLOG_TASK_BREAKDOWN_ACCEPTED -> BACKLOG_READY_FOR_TECH11_HANDOFF.

## BACKLOG_READY_FOR_TECH11_HANDOFF -> BACKLOG_HANDOFF_TO_TECH11.

Bất kỳ trạng thái nào -> BACKLOG_SUPERSEDED nếu có backlog mới thay thế.

Bất kỳ trạng thái nào -> BACKLOG_CANCELLED nếu owner quyết định hủy.

## 3.3. Transition bị cấm

Các transition bị cấm:

## BACKLOG_NOT_CREATED -> BACKLOG_READY_FOR_TECH11_HANDOFF.

## BACKLOG_DRAFTING -> BACKLOG_READY_FOR_TASK_BREAKDOWN.

## BACKLOG_NEED_SOURCE_MAPPING -> BACKLOG_READY_FOR_TASK_BREAKDOWN.

BACKLOG_SOURCE_CONFLICT -> BACKLOG_READY_FOR_TASK_BREAKDOWN nếu chưa xử lý.

## BACKLOG_NEED_DEPENDENCY_REVIEW -> BACKLOG_READY_FOR_TASK_BREAKDOWN.

## BACKLOG_NEED_EVIDENCE_REQUIREMENT -> BACKLOG_READY_FOR_TECH11_HANDOFF.

## BACKLOG_NEED_SMOKE_REQUIREMENT -> BACKLOG_READY_FOR_TECH11_HANDOFF.

## BACKLOG_BLOCKED -> BACKLOG_HANDOFF_TO_TECH11.

BACKLOG_READY_FOR_TECH11_HANDOFF -> Release Ready.

BACKLOG_HANDOFF_TO_TECH11 -> Go-live Approved.

## 3.4. P0 điểm chặn

FAIL nếu backlog chưa source-mapped nhưng được handoff.

FAIL nếu backlog còn source conflict nhưng được chia task.

FAIL nếu backlog thiếu evidence/smoke nhưng Ready.

FAIL nếu backlog bị chặn nhưng vẫn chuyển sang TECH-11.

## 4. DEV TASK STATE MACHINE

## 4.1. Danh sách trạng thái task

Mỗi backlog có thể được chia thành nhiều dev task. Mỗi task có các trạng thái:

## TASK_NOT_CREATED.

## TASK_DRAFTING.

## TASK_SOURCE_MAPPED.

## TASK_SCOPE_REVIEWED.

## TASK_DEPENDENCY_REVIEWED.

## TASK_EVIDENCE_DEFINED.

## TASK_SMOKE_DEFINED.

## TASK_OWNER_ASSIGNED.

## TASK_READY_FOR_TECH11_HANDOFF.

## TASK_BLOCKED.

## TASK_SUPERSEDED.

## TASK_CANCELLED.

## 4.2. Task Ready Rule

Một task chỉ được Ready for TECH-11 Handoff khi có:

Task ID.

Backlog ID.

Phase.

TECH source.

Requirement summary.

Scope In.

Scope Out.

Dependency.

P0 điểm chặn.

Evidence required.

Smoke required.

Owner.

Report requirement.

Handoff target.

## 4.3. Transition bị cấm

TASK_DRAFTING -> TASK_READY_FOR_TECH11_HANDOFF nếu thiếu source.

TASK_SOURCE_MAPPED -> TASK_READY_FOR_TECH11_HANDOFF nếu thiếu dependency.

TASK_EVIDENCE_DEFINED -> TASK_READY_FOR_TECH11_HANDOFF nếu chưa có smoke.

## TASK_BLOCKED -> TASK_READY_FOR_TECH11_HANDOFF.

TASK_READY_FOR_TECH11_HANDOFF -> Code Handoff Allowed nếu chưa qua TECH-11.

## 4.4. P0 điểm chặn

FAIL nếu task làm mất source mapping của backlog cha.

FAIL nếu task con bỏ P0 điểm chặn của backlog cha.

FAIL nếu task con bỏ evidence/smoke requirement.

FAIL nếu task con biến thành code snippet rời rạc.

## 5. SOURCE MAPPING STATE MODEL

## 5.1. Danh sách trạng thái source mapping

Source mapping có các trạng thái:

## SOURCE_NOT_CHECKED.

## SOURCE_CHECKING.

## SOURCE_FOUND_IN_TECH.

## SOURCE_NOT_FOUND.

## SOURCE_CONFLICT_WITH_LEGACY.

## SOURCE_CONFLICT_WITH_CURRENT_CODE.

## SOURCE_CONFLICT_REVIEW_REQUIRED.

## SOURCE_ACCEPTED_TECH_WINS.

## SOURCE_ACCEPTED_OWNER_DECISION.

## SOURCE_REJECTED.

## 5.2. Source Mapping Rule

Một source mapping hợp lệ phải có:

TECH number.

Section hoặc requirement group.

Requirement summary.

Phase.

Domain.

Backlog ID.

Legacy conflict note nếu có.

Current implementation deviation nếu có.

Decision.

Reviewer.

## 5.3. Rule khi có conflict

Nếu có conflict:

TECH mới thắng tài liệu cũ.

TECH mới thắng code cũ.

Không dùng legacy làm source thật.

Không dùng current code làm nghiệp vụ đúng nếu khác TECH.

Phải ghi source conflict.

Nếu cần owner quyết định thì route owner.

Không được đưa backlog sang Ready khi conflict chưa xử lý.

## 5.4. P0 điểm chặn

FAIL nếu source không tìm thấy nhưng backlog vẫn Ready.

FAIL nếu legacy reference thắng TECH mới.

FAIL nếu code cũ thắng TECH mới.

FAIL nếu source conflict bị bỏ qua.

## 6. DEPENDENCY MATRIX STATE MODEL

## 6.1. Danh sách trạng thái dependency

Dependency có các trạng thái:

## DEPENDENCY_NOT_CHECKED.

## DEPENDENCY_CHECKING.

## DEPENDENCY_PASS.

## DEPENDENCY_FAIL.

## DEPENDENCY_BLOCKED_BY_UPSTREAM.

## DEPENDENCY_BLOCKED_BY_SOURCE_CONFLICT.

## DEPENDENCY_BLOCKED_BY_P0.

## DEPENDENCY_NEED_OWNER_REVIEW.

## DEPENDENCY_ACCEPTED_FOR_PLANNING_ONLY.

## DEPENDENCY_ACCEPTED_FOR_HANDOFF.

## 6.2. Dependency Pass Rule

Dependency chỉ pass khi:

Upstream source rõ.

Upstream critical backlog không bị chặn.

Upstream smoke/evidence requirement đã xác định.

Không có source conflict.

Không có P0 điểm chặn mở.

Dependency không dựa vào mock production.

Dependency không dựa vào code cũ trái TECH mới.

Handoff downstream phù hợp.

## 6.3. Planning-only rule

Một số backlog downstream có thể ở trạng thái chuẩn bị:

## DEPENDENCY_ACCEPTED_FOR_PLANNING_ONLY

Trạng thái này cho phép:

Soạn tài liệu.

Soạn test case.

Chuẩn bị mock có ghi rõ là mock.

Chuẩn bị UI wireframe.

Chuẩn bị backlog phụ.

Nhưng không cho phép:

Release.

Production traffic.

Go-live.

Dùng mock làm source-of-truth.

Gọi dependency pass thật.

## 6.4. P0 điểm chặn

FAIL nếu downstream release khi dependency chỉ planning-only.

FAIL nếu mock upstream được coi là production truth.

FAIL nếu dependency fail nhưng backlog vẫn Ready.

## 7. EVIDENCE REQUIREMENT STATE MODEL

## 7.1. Danh sách trạng thái evidence requirement

Evidence requirement có các trạng thái:

## EVIDENCE_REQ_NOT_DEFINED.

## EVIDENCE_REQ_DRAFTING.

## EVIDENCE_REQ_NEED_DOMAIN_REVIEW.

## EVIDENCE_REQ_NEED_PRIVACY_REVIEW.

## EVIDENCE_REQ_ACCEPTED.

## EVIDENCE_REQ_REJECTED.

## EVIDENCE_REQ_SUPERSEDED.

## 7.2. Evidence Requirement Rule

Evidence requirement accepted khi có:

Evidence type.

Requirement mapping.

Phase mapping.

Backlog mapping.

Environment expectation.

Expected/actual expectation.

Reviewer expectation.

Sensitive data policy nếu có.

Audit/correlation expectation nếu high-risk.

Handoff destination.

## 7.3. Evidence loại bắt buộc theo risk

Backlog high-risk phải có evidence mạnh hơn.

Ví dụ:

Payment / revenue cần audit/log/source evidence.

IVR cần call outcome/evidence/audit.

Ads cần Verified Revenue/Data Quality evidence.

Gateway cần public/private delivery evidence.

AI cần final response guard evidence.

Operational cần inventory/ledger/trace/release evidence.

Admin override cần actor/reason/evidence/audit.

## 7.4. P0 điểm chặn

FAIL nếu high-risk backlog không có evidence requirement.

FAIL nếu evidence requirement không nêu environment.

FAIL nếu evidence requirement không nêu expected/actual.

FAIL nếu privacy-sensitive backlog không có privacy evidence.

## 8. SMOKE REQUIREMENT STATE MODEL

## 8.1. Danh sách trạng thái smoke requirement

Smoke requirement có các trạng thái:

## SMOKE_REQ_NOT_DEFINED.

## SMOKE_REQ_DRAFTING.

## SMOKE_REQ_NEED_P0_REVIEW.

## SMOKE_REQ_NEED_FAIL_CASE_REVIEW.

## SMOKE_REQ_NEED_DEPENDENCY_REVIEW.

## SMOKE_REQ_ACCEPTED.

## SMOKE_REQ_REJECTED.

## SMOKE_REQ_SUPERSEDED.

## 8.2. Smoke Requirement Rule

Smoke requirement accepted khi có:

Smoke ID draft.

Scenario.

Given.

When.

Then.

Must Not.

Expected result.

Evidence required.

P0 case nếu có.

Fail-case nếu có.

Dependency case nếu có.

Runtime unavailable case nếu có.

## 8.3. Smoke theo domain

Smoke requirement phải phù hợp domain:

Foundation: audit/evidence/RBAC/idempotency.

Product: Product Active != Sellable.

Operational: raw readiness, release, warehouse, trace, recall.

Commerce: quote/cart/order/payment/revenue.

AI: no self-price, no self-order, guard response.

Gateway: public/private, Messenger handoff, suppression.

Ads: no fake revenue, dedup, data quality, scale gate.

Live: no live signal as revenue, script guard, troll/complaint split.

IVR: official order only, invalid phone, no answer, cancel, technical failure.

Release: evidence, smoke, sign-off, release decision, gateway.

## 8.4. P0 điểm chặn

FAIL nếu smoke requirement chỉ có happy path cho backlog critical.

FAIL nếu không có Must Not.

FAIL nếu không có P0 smoke.

FAIL nếu không có dependency smoke cho backlog downstream.

## 9. OWNER ASSIGNMENT STATE MODEL

## 9.1. Danh sách trạng thái owner assignment

Owner assignment có các trạng thái:

## OWNER_NOT_ASSIGNED.

## OWNER_ASSIGNMENT_IN_PROGRESS.

## OWNER_ASSIGNED.

## OWNER_MISSING_CRITICAL_ROLE.

## OWNER_CONFLICT.

## OWNER_REVIEW_REQUIRED.

## OWNER_ASSIGNMENT_ACCEPTED.

## OWNER_ASSIGNMENT_REJECTED.

## 9.2. Owner Assignment Rule

Owner assignment accepted khi có:

Business owner nếu backlog nghiệp vụ.

Dev owner.

QA owner.

Evidence reviewer.

Smoke reviewer.

Domain owner.

Privacy/Security owner nếu có dữ liệu nhạy cảm.

Release owner nếu backlog release-level.

Handoff receiver.

Owner scope rõ.

## 9.3. Owner theo domain

Backlog phải có owner phù hợp:

Foundation: Security/RBAC/Audit owner.

Product: Product/SKU/Recipe owner.

Operational: Production/Warehouse/Trace/Recall owner.

Commerce: Commerce/Payment/Shipping owner.

AI: AI Advisor/Claim/Content owner.

Gateway: Channel/Gateway owner.

Ads: Ads Measurement/Data Quality owner.

Live: MC AI Live owner.

IVR: IVR/Core Order/Notification owner.

Release: Release/QA/UAT/Privacy/Security owner.

## 9.4. P0 điểm chặn

FAIL nếu backlog không owner vẫn Ready.

FAIL nếu backlog privacy-sensitive thiếu Privacy/Security owner.

FAIL nếu backlog payment/revenue thiếu Commerce/Payment owner.

FAIL nếu release-level backlog thiếu Release owner.

## 10. BACKLOG READINESS STATE MODEL

## 10.1. Danh sách trạng thái readiness

Backlog readiness có các trạng thái:

## READINESS_NOT_CHECKED.

## READINESS_CHECKING.

## READINESS_NEED_SOURCE.

## READINESS_NEED_SCOPE.

## READINESS_NEED_DEPENDENCY.

## READINESS_NEED_OWNER.

## READINESS_NEED_EVIDENCE.

## READINESS_NEED_SMOKE.

## READINESS_NEED_REPORT_REQUIREMENT.

## READINESS_BLOCKED_BY_P0.

## READINESS_BLOCKED_BY_UPSTREAM.

## READINESS_READY_FOR_TASK_BREAKDOWN.

## READINESS_READY_FOR_TECH11_HANDOFF.

## 10.2. Ready for Task Breakdown Rule

Backlog chỉ Ready for Task Breakdown khi:

Source mapping accepted.

Scope In/Out accepted.

Dependency reviewed.

Owner assigned.

Evidence requirement accepted.

Smoke requirement accepted.

Report requirement accepted.

P0 điểm chặn list defined.

No open source conflict.

No upstream critical điểm chặn.

## 10.3. Ready for TECH-11 Handoff Rule

Backlog chỉ Ready for TECH-11 Handoff khi:

Ready for Task Breakdown.

Task breakdown accepted.

Task evidence/smoke preserved.

Handoff target clear.

Change control not WAITING.

điểm chặn none/open P0 none.

Phase Backlog Review pass.

## 10.4. P0 điểm chặn

FAIL nếu readiness bỏ qua evidence/smoke.

FAIL nếu readiness bỏ qua owner.

FAIL nếu readiness cho source conflict vào handoff.

FAIL nếu readiness gọi Ready for TECH-11 là Dev Ready.

## 11. BACKLOG điểm chặn STATE MODEL

## 11.1. Danh sách trạng thái điểm chặn

Backlog điểm chặn có các trạng thái:

## BLK_NOT_REPORTED.

## BLK_OPEN.

## BLK_ASSIGNED.

## BLK_NEED_SOURCE_REVIEW.

## BLK_NEED_DEPENDENCY_REVIEW.

## BLK_NEED_OWNER_REVIEW.

## BLK_NEED_EVIDENCE_SMOKE.

## BLK_FIX_IN_PROGRESS.

## BLK_FIX_SUBMITTED.

## BLK_REVIEWING_FIX.

## BLK_RESOLVED_WAITING_ACCEPTANCE.

## BLK_CLOSED.

## BLK_REOPENED.

## 11.2. điểm chặn Closure Rule

điểm chặn chỉ closed khi có:

Root cause.

Fix decision.

Updated backlog nếu cần.

Updated source mapping nếu cần.

Updated dependency nếu cần.

Updated evidence/smoke nếu cần.

Owner/reviewer acceptance.

Audit note.

## 11.3. P0 điểm chặn

FAIL nếu điểm chặn closed không evidence.

FAIL nếu điểm chặn P0 downgraded không approval.

FAIL nếu điểm chặn open nhưng backlog Ready.

FAIL nếu điểm chặn hidden khỏi handoff.

## 12. TASK BREAKDOWN STATE MODEL

## 12.1. Danh sách trạng thái task breakdown

Task breakdown có các trạng thái:

## BREAKDOWN_NOT_STARTED.

## BREAKDOWN_DRAFTING.

## BREAKDOWN_SOURCE_CHECKING.

## BREAKDOWN_DEPENDENCY_CHECKING.

## BREAKDOWN_EVIDENCE_SMOKE_CHECKING.

## BREAKDOWN_REJECTED.

## BREAKDOWN_NEED_REWORK.

## BREAKDOWN_ACCEPTED.

## BREAKDOWN_SUPERSEDED.

## 12.2. Task Breakdown Rule

Task breakdown accepted khi:

Không làm mất source mapping.

Không làm mất dependency.

Không làm mất Scope Out.

Không làm mất P0 điểm chặn.

Không làm mất evidence requirement.

Không làm mất smoke requirement.

Task đủ nhỏ để dev hiểu.

Task không biến thành code snippet.

Task có report requirement.

Task có handoff target.

## 12.3. Task Breakdown bị reject khi

Task quá rộng.

Task quá mơ hồ.

Task chỉ là “code phần này”.

Task không source.

Task không evidence/smoke.

Task bỏ dependency.

Task hardcode policy.

Task bỏ P0 điểm chặn của backlog cha.

## 12.4. P0 điểm chặn

FAIL nếu breakdown làm mất source.

FAIL nếu breakdown làm mất P0 điểm chặn.

FAIL nếu breakdown thành code copy-paste task.

FAIL nếu breakdown cho dev tự quyết nghiệp vụ.

## 13. BACKLOG-TO-HANDOFF STATE MODEL

## 13.1. Danh sách trạng thái handoff

Backlog-to-handoff có các trạng thái:

## HANDOFF_NOT_STARTED.

## HANDOFF_PREPARING.

## HANDOFF_NEED_BACKLOG_READY.

## HANDOFF_NEED_TASK_BREAKDOWN.

## HANDOFF_NEED_OWNER_CHECK.

## HANDOFF_NEED_EVIDENCE_SMOKE_CHECK.

## HANDOFF_NEED_DEPENDENCY_CHECK.

## HANDOFF_BLOCKED.

## HANDOFF_READY_FOR_TECH11.

## HANDOFF_SENT_TO_TECH11.

## HANDOFF_REJECTED_BY_TECH11.

## HANDOFF_ACCEPTED_BY_TECH11.

## 13.2. Handoff Ready Rule

Handoff ready khi:

Backlog Ready for TECH-11 Handoff.

Task breakdown accepted.

Evidence/smoke preserved.

Owner assignment accepted.

Dependency pass hoặc planning-only được ghi rõ.

điểm chặn none/open P0 none.

Report requirement attached.

Change control closed.

Handoff document complete.

## 13.3. P0 điểm chặn

FAIL nếu handoff thiếu Scope Out.

FAIL nếu handoff thiếu P0 điểm chặn.

FAIL nếu handoff thiếu evidence/smoke.

FAIL nếu handoff gửi backlog bị chặn sang TECH-11.

FAIL nếu handoff gọi backlog là Dev Ready khi chưa qua TECH-11.

## 14. CHANGE CONTROL STATE MODEL

## 14.1. Danh sách trạng thái change control

Backlog change có các trạng thái:

## CHANGE_NOT_REQUESTED.

## CHANGE_REQUESTED.

## CHANGE_IMPACT_REVIEW.

## CHANGE_SOURCE_REVIEW.

## CHANGE_DEPENDENCY_REVIEW.

## CHANGE_EVIDENCE_SMOKE_REVIEW.

## CHANGE_OWNER_REVIEW.

## CHANGE_APPROVED.

## CHANGE_REJECTED.

## CHANGE_APPLIED.

## CHANGE_SUPERSEDED.

## CHANGE_REQUIRES_REHANDOFF.

## 14.2. Change Control Rule

Mọi thay đổi backlog sau khi Ready hoặc handoff phải có:

Change request ID.

Backlog ID.

Before/after.

Impact.

Source impact.

Dependency impact.

Evidence/smoke impact.

Owner impact.

Handoff impact.

Approval.

## 14.3. Change bị cấm

Không được:

Sửa backlog âm thầm.

Xóa P0 điểm chặn.

Xóa evidence/smoke cho dễ làm.

Đổi scope để bypass dependency.

Đổi source sang tài liệu cũ.

Đổi owner critical không review.

Sửa handoff sau khi gửi dev mà không báo dev/QA.

## 14.4. P0 điểm chặn

FAIL nếu backlog thay đổi sau handoff mà không có change record.

FAIL nếu change làm mất source mapping.

FAIL nếu change làm mất evidence/smoke.

FAIL nếu change bypass dependency.

## 15. RELEASE HANDOFF STATE MODEL

## 15.1. Danh sách trạng thái release handoff

Release handoff trong TECH-12 có các trạng thái:

## RELEASE_HANDOFF_NOT_REQUIRED_YET.

## RELEASE_HANDOFF_PREPARING.

## RELEASE_HANDOFF_BACKLOG_READY.

## RELEASE_HANDOFF_TO_TECH11_READY.

## RELEASE_HANDOFF_TO_TECH11_SENT.

## RELEASE_HANDOFF_TO_TECH11_ACCEPTED.

## RELEASE_HANDOFF_TO_TECH11_REJECTED.

## RELEASE_HANDOFF_TO_TECH10_NOT_ALLOWED_YET.

## RELEASE_HANDOFF_TO_TECH10_READY_AFTER_IMPLEMENTATION.

## RELEASE_HANDOFF_BLOCKED.

## 15.2. Handoff sang TECH-11

TECH-12 chỉ được bàn giao backlog sang TECH-11 khi:

Source mapping accepted.

Dependency reviewed.

Evidence/smoke defined.

Owner assigned.

Task breakdown accepted.

Backlog Ready for Handoff.

Change control closed.

No open P0 điểm chặn.

## 15.3. Handoff sang TECH-10

TECH-12 không trực tiếp đưa backlog chưa implemented sang TECH-10 như release package.

TECH-10 chỉ review sau khi:

TECH-11 handoff đã đi vào implementation.

Dev report có.

Evidence có.

Smoke có.

QA handoff có nếu required.

Phase exit accepted.

Release handoff package được tạo.

## 15.4. P0 điểm chặn

FAIL nếu TECH-12 gọi Backlog Ready là Release Ready.

FAIL nếu TECH-12 gửi backlog chưa implemented sang TECH-10 như release-ready.

FAIL nếu TECH-12 mở Global Gateway.

## 16. SOURCE-TO-BACKLOG-TO-HANDOFF FLOW

## 16.1. Flow chuẩn

Flow chuẩn:

TECH source-of-truth được xác định.

Backlog Item Factory tạo draft backlog.

Source-to-Backlog Matrix Resolver map source.

Phase Dependency Matrix Resolver kiểm tra dependency.

Evidence Requirement Resolver gắn evidence.

Smoke Requirement Resolver gắn smoke.

Owner Assignment Resolver gắn owner.

Report Requirement Resolver gắn report requirement.

Backlog Readiness Resolver kiểm tra Ready.

Backlog điểm chặn Resolver xử lý điểm chặn.

Dev Task Breakdown Resolver chia task.

Phase Backlog Review Resolver kiểm tra toàn phase.

Backlog-to-Handoff Resolver tạo handoff package.

Backlog Change Control Resolver kiểm soát thay đổi nếu có.

Release Handoff Resolver bàn giao sang TECH-11.

## 16.2. Flow khi thiếu source

Backlog draft được tạo.

Source mapping không tìm thấy TECH reference.

Backlog chuyển NEED_SOURCE_MAPPING.

Không dependency review.

Không readiness Ready.

Không task breakdown.

Không handoff dev.

Route owner/source review.

## 16.3. Flow khi dependency fail

Backlog đã source mapped.

Dependency Matrix phát hiện upstream bị chặn.

Backlog chuyển BLOCKED_BY_UPSTREAM.

Có thể chuẩn bị planning-only nếu phù hợp.

Không handoff release-level implementation.

Chờ upstream pass.

## 16.4. Flow khi thiếu evidence/smoke

Backlog đã có source và dependency.

Evidence/smoke requirement chưa định nghĩa.

Backlog chuyển NEED_EVIDENCE_SMOKE.

Không Ready.

Evidence/Smoke Resolver mở rộng.

Readiness review lại.

## 16.5. Flow khi backlog thay đổi sau handoff

Change requested.

Impact review.

Source/dependency/evidence/smoke/owner review.

Change approved hoặc rejected.

Nếu approved, cập nhật backlog.

Cập nhật handoff.

Notify TECH-11/dev/QA nếu đã gửi.

## 17. PHASE DEPENDENCY CONTROL

## 17.1. Dependency PHASE 0

PHASE 0 là nền cho toàn bộ hệ thống.

Nếu PHASE 0 chưa pass, các backlog high-risk ở các phase sau chỉ được planning-only.

Không được release-level implementation với:

Payment.

Order.

Warehouse.

Inventory.

Recall.

Sale Lock.

AI response guard.

Ads revenue.

IVR outcome.

Admin override.

## 17.2. Dependency PHASE 1 -> PHASE 2

Operational không được dùng Product/SKU/Recipe chưa source-mapped.

P0 Fail nếu recipe/formula chưa version mà Production Order snapshot.

## 17.3. Dependency PHASE 2 -> PHASE 3

Commerce phụ thuộc Operational Sellable Gate, Warehouse/Inventory/Sale Lock/Recall.

P0 Fail nếu Commerce bán SKU/batch Operational bị chặn.

## 17.4. Dependency PHASE 3 -> PHASE 4

AI Advisor phụ thuộc Commerce QuoteSnapshot, Order Draft, Official Order boundary, payment boundary.

P0 Fail nếu AI tự tính giá hoặc tự tạo order.

## 17.5. Dependency PHASE 4 -> PHASE 5

Gateway phụ thuộc AI Final Response Guard, public/private boundary, claim guard.

P0 Fail nếu Gateway gửi response chưa guard.

## 17.6. Dependency PHASE 5 -> PHASE 7

MC AI Live phụ thuộc Gateway, AI, Commerce và Ads-safe boundary.

P0 Fail nếu Live public/private routing chưa pass mà go-live.

## 17.7. Dependency PHASE 3 -> PHASE 6

Ads phụ thuộc Verified Revenue từ Commerce.

P0 Fail nếu Ads dùng quote/cart/order draft/unpaid order làm revenue.

## 17.8. Dependency PHASE 3 -> PHASE 8

IVR phụ thuộc Official Order, Core Order State Machine, Notification Owner.

P0 Fail nếu IVR gọi Quote/Cart/Order Draft hoặc tự gửi notification.

## 17.9. Dependency PHASE 9

Release/go-live/scale phụ thuộc TECH-10.

P0 Fail nếu Global Gateway mở khi evidence/smoke/sign-off/release decision chưa đủ.

## 18. FAIL-SAFE BACKLOG CONTROL

## 18.1. Khi source unavailable

Nếu không tìm được source:

Backlog không Ready.

Không handoff dev.

Không task breakdown.

Route Source Review.

## 18.2. Khi dependency unavailable

Nếu không xác định được dependency:

Backlog không Ready.

Có thể planning-only.

Không release-level implementation.

Route Dependency Review.

## 18.3. Khi owner unavailable

Nếu chưa có owner:

Backlog không Ready.

Không handoff.

Route Owner Assignment.

## 18.4. Khi evidence/smoke chưa rõ

Nếu evidence/smoke chưa rõ:

Backlog không Ready.

Không handoff dev.

Route Evidence/Smoke Requirement Resolver.

## 18.5. Khi có P0 điểm chặn

Nếu có P0 điểm chặn:

Backlog bị chặn.

Phase Backlog Review fail.

Không handoff TECH-11.

Không TECH-10 release review.

Không Global Gateway.

## 18.6. Khi change control mở

Nếu change control đang mở:

Backlog không được handoff.

Nếu đã handoff thì phải re-handoff.

Dev/QA phải được cập nhật.

Không dùng nguồn đã có như source hiện hành.

## 19. COMMAND BOUNDARY

## 19.1. Command được phép trong TECH-12

TECH-12 cho phép các command quản trị backlog:

Create Backlog Draft.

Map Source To Backlog.

Review Dependency.

Define Evidence Requirement.

Define Smoke Requirement.

Assign Owner.

Define Report Requirement.

Review Backlog Readiness.

Open Backlog điểm chặn.

Resolve Backlog điểm chặn.

Create Dev Task Breakdown.

Review Phase Backlog.

Create Handoff Package.

Request Backlog Change.

Approve/Reject Backlog Change.

Prepare Release Handoff.

## 19.2. Command bị cấm trong TECH-12

TECH-12 không được phát sinh command:

Write Production Code.

Generate Copy-Paste Code.

Create API Endpoint Detail.

Create Database Migration Detail.

Create UI Component Detail.

Override Business Rule.

Hardcode Runtime Policy.

Mark Dev Ready.

Mark Release Ready.

Mark Production Ready.

Mark Go-live Approved.

Open Global Gateway.

Skip TECH-11.

Skip TECH-10.

## 19.3. P0 điểm chặn

FAIL nếu TECH-12 được dùng như code generation document.

FAIL nếu TECH-12 mở release.

FAIL nếu TECH-12 bỏ qua TECH-11/TECH-10.

## 20. QUERY BOUNDARY

## 20.1. Query được phép

TECH-12 được phép truy vấn/đối chiếu:

TECH source-of-truth.

Backlog registry.

Phase registry.

Source mapping.

Dependency matrix.

Evidence requirement.

Smoke requirement.

Owner assignment.

điểm chặn status.

Change control status.

Handoff status.

## 20.2. Query bị hạn chế

TECH-12 không được dùng:

Dev claim làm source.

Code cũ làm source nếu trái TECH mới.

Tài liệu cũ làm source nếu trái TECH mới.

Mock làm production truth.

Dashboard làm source-of-truth.

Local demo làm release evidence.

“Có vẻ đúng” làm readiness.

## 20.3. P0 điểm chặn

FAIL nếu query từ code cũ thay source-of-truth.

FAIL nếu mock được dùng làm dependency pass thật.

FAIL nếu backlog readiness dựa trên cảm tính.

## 21. P0 điểm chặn REGISTRY - PHẦN 3/4

Các lỗi sau là P0 điểm chặn cấp lifecycle/state machine:

Backlog không source nhưng Ready.

Backlog source conflict nhưng handoff.

Backlog thiếu Scope Out nhưng Ready.

Backlog thiếu dependency nhưng Ready.

Backlog thiếu owner nhưng Ready.

Backlog thiếu evidence nhưng Ready.

Backlog thiếu smoke nhưng Ready.

Backlog thiếu report requirement nhưng Ready.

Backlog downstream Ready khi upstream bị chặn.

Task breakdown làm mất source.

Task breakdown bỏ P0 điểm chặn.

Task breakdown biến thành code snippet.

Evidence requirement bỏ high-risk audit.

Smoke requirement chỉ happy path cho critical backlog.

Owner assignment thiếu owner critical.

Change control sửa backlog âm thầm.

Handoff gửi backlog bị chặn sang TECH-11.

TECH-12 gọi Backlog Ready là Dev Ready.

TECH-12 gọi Backlog Ready là Release Ready.

TECH-12 bỏ TECH-11.

TECH-12 bỏ TECH-10.

TECH-12 mở Global Gateway.

Mock upstream làm production truth.

Tài liệu cũ thắng TECH mới.

Code cũ thắng TECH mới.

## 22. SMOKE ĐỊNH HƯỚNG - PHẦN 3/4

TECH12-P3-SMK-001 - Backlog Lifecycle Source Missing

Given backlog draft không có source
When Backlog Lifecycle chạy
Then backlog chuyển BACKLOG_NEED_SOURCE_MAPPING.

TECH12-P3-SMK-002 - Source Conflict Blocks Handoff

Given backlog có source conflict với code cũ
When Source Mapping chạy
Then backlog chuyển BACKLOG_SOURCE_CONFLICT và không handoff.

TECH12-P3-SMK-003 - Dependency Fail Blocks Readiness

Given downstream backlog phụ thuộc upstream bị chặn
When Dependency Matrix chạy
Then backlog chuyển READINESS_BLOCKED_BY_UPSTREAM.

TECH12-P3-SMK-004 - Evidence Requirement Missing

Given backlog high-risk thiếu evidence requirement
When Readiness Resolver chạy
Then backlog chuyển READINESS_NEED_EVIDENCE.

TECH12-P3-SMK-005 - Smoke Requirement Missing

Given backlog critical thiếu P0 smoke
When Readiness Resolver chạy
Then backlog chuyển READINESS_NEED_SMOKE.

TECH12-P3-SMK-006 - Owner Missing

Given backlog payment/revenue thiếu Commerce Owner
When Owner Assignment chạy
Then backlog chuyển OWNER_MISSING_CRITICAL_ROLE.

TECH12-P3-SMK-007 - Task Breakdown Preserves Source

Given backlog source-mapped
When Dev Task Breakdown chạy
Then mọi task con giữ source mapping.

TECH12-P3-SMK-008 - Task Breakdown Rejects Code Snippet

Given task breakdown biến thành “copy đoạn code này”
When review chạy
Then BREAKDOWN_REJECTED.

TECH12-P3-SMK-009 - Change Control Required

Given backlog đã handoff nhưng scope thay đổi
When Change Control chạy
Then CHANGE_REQUIRES_REHANDOFF.

TECH12-P3-SMK-010 - Handoff Only To TECH-11

Given backlog Ready
When Release Handoff chạy
Then status = HANDOFF_READY_FOR_TECH11, không Release Ready.

TECH12-P3-SMK-011 - TECH-10 Required Later

Given backlog đã handoff TECH-11
When chưa implementation/evidence/smoke
Then không được gửi TECH-10 như release-ready package.

TECH12-P3-SMK-012 - Global Gateway bị chặn

Given backlog lifecycle hoàn tất nhưng chưa TECH-10 gate
When gateway status xét
Then Global Gateway vẫn bị chặn.

## 23. EVIDENCE PACKAGE - PHẦN 3/4

## PHẦN 3/4 yêu cầu evidence thiết kế gồm:

Backlog Lifecycle State Model accepted.

Dev Task State Machine accepted.

Source Mapping State Model accepted.

Dependency Matrix State Model accepted.

Evidence Requirement State Model accepted.

Smoke Requirement State Model accepted.

Owner Assignment State Model accepted.

Backlog Readiness State Model accepted.

Backlog điểm chặn State Model accepted.

Task Breakdown State Model accepted.

Backlog-to-Handoff State Model accepted.

Change Control State Model accepted.

Release Handoff State Model accepted.

Source-to-Backlog-to-Handoff Flow accepted.

Phase Dependency Control accepted.

Fail-safe Backlog Control accepted.

Command Boundary accepted.

Query Boundary accepted.

P0 điểm chặn Registry accepted.

Smoke định hướng accepted.

## 24. DONE GATE - PHẦN 3/4

## PHẦN 3/4 đạt Documentation Complete ở cấp lifecycle/state machine khi:

Đã khóa Backlog Lifecycle State Model.

Đã khóa Dev Task State Machine.

Đã khóa Source Mapping State Model.

Đã khóa Dependency Matrix State Model.

Đã khóa Evidence Requirement State Model.

Đã khóa Smoke Requirement State Model.

Đã khóa Owner Assignment State Model.

Đã khóa Backlog Readiness State Model.

Đã khóa Backlog điểm chặn State Model.

Đã khóa Task Breakdown State Model.

Đã khóa Backlog-to-Handoff State Model.

Đã khóa Change Control State Model.

Đã khóa Release Handoff State Model.

Đã khóa Source-to-Backlog-to-Handoff Flow.

Đã khóa Phase Dependency Control.

Đã khóa Fail-safe Backlog Control.

Đã khóa Command Boundary.

Đã khóa Query Boundary.

Đã có P0 điểm chặn Registry.

Đã có Smoke định hướng.

Đã có Evidence Package.

Đã sẵn sàng chuyển sang PHẦN 4/4.

## 25. FAIL GATE - PHẦN 3/4

## PHẦN 3/4 FAIL nếu:

Thiếu Backlog Lifecycle State Model.

Thiếu Dev Task State Machine.

Thiếu Source Mapping State Model.

Thiếu Dependency Matrix State Model.

Thiếu Evidence Requirement State Model.

Thiếu Smoke Requirement State Model.

Thiếu Owner Assignment State Model.

Thiếu Backlog Readiness State Model.

Thiếu điểm chặn State Model.

Thiếu Task Breakdown State Model.

Thiếu Handoff State Model.

Thiếu Change Control State Model.

Thiếu Release Handoff State Model.

Cho backlog không source Ready.

Cho source conflict handoff.

Cho downstream Ready khi upstream bị chặn.

Cho task breakdown thành code snippet.

Cho change control bỏ evidence/smoke.

Cho handoff backlog bị chặn sang TECH-11.

Gọi Backlog Ready là Release Ready.

Bỏ qua TECH-11.

Bỏ qua TECH-10.

Mở Global Gateway từ TECH-12.

## 26. RELEASE HANDOFF - SANG PHẦN 4/4

## PHẦN 3/4 bàn giao sang PHẦN 4/4 các nội dung đã khóa:

Backlog Lifecycle.

Dev Task State Machine.

Source Mapping State Model.

Dependency Matrix State Model.

Evidence Requirement State Model.

Smoke Requirement State Model.

Owner Assignment State Model.

Backlog Readiness State Model.

Backlog điểm chặn State Model.

Task Breakdown State Model.

Backlog-to-Handoff State Model.

Change Control State Model.

Release Handoff State Model.

Source-to-Backlog-to-Handoff Flow.

Phase Dependency Control.

Fail-safe Backlog Control.

Command Boundary.

Query Boundary.

P0 điểm chặn Registry.

Smoke định hướng.

Evidence Package.

## PHẦN 4/4 sẽ viết:

FINAL PHASE BACKLOG MATRIX / DEV TASK BREAKDOWN PACK / EVIDENCE-SMOKE MATRIX / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-12 FINAL CONCLUSION

Trong đó phải khóa rõ:

Final backlog matrix theo PHASE 0 -> PHASE 9.

Dev task breakdown pack cấp phase.

Evidence matrix.

Smoke matrix.

Owner matrix.

Dependency matrix.

bị chặn backlog matrix.

Ready backlog matrix.

Backlog handoff matrix sang TECH-11.

Release boundary sang TECH-10.

Final Done Gate.

Final Fail Gate.

TECH-12 Final Conclusion.

## 27. TRẠNG THÁI SAU PHẦN 3/4

Sau PHẦN 3/4:

## PHẦN 1/4 đã khóa backlog principles.

## PHẦN 2/4 đã khóa module contracts.

## PHẦN 3/4 đã khóa backlog lifecycle, task state machine, source-to-backlog-to-handoff flow và phase dependency control.

Chưa được gọi là Documentation Complete.

Chưa được gọi là Backlog Ready.

Chưa được gọi là Dev Ready.

Chưa được gọi là Implementation Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Production Ready.

Chưa được gọi là Go-live Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời backlog cho TECH-12.

Các điểm trọng yếu đã được cố định:

Backlog phải đi từ draft -> source mapping -> scope review -> dependency review -> owner/evidence/smoke/report requirement -> readiness review -> task breakdown -> handoff.

Backlog không source không được Ready.

Source conflict phải được xử lý trước khi handoff.

TECH mới thắng tài liệu cũ và code cũ.

Dependency fail chặn downstream.

Planning-only không phải production-ready.

Evidence requirement là bắt buộc.

Smoke requirement là bắt buộc.

Owner assignment là bắt buộc.

Task breakdown không được làm mất source/P0/evidence/smoke.

Task breakdown không được biến thành code snippet rời rạc.

Change control bắt buộc nếu backlog thay đổi sau Ready hoặc sau handoff.

Backlog handoff chỉ được sang TECH-11 Dev Handoff Governance.

TECH-12 không gọi Backlog Ready là Dev Ready.

TECH-12 không gọi Backlog Ready là Release Ready.

TECH-12 không gửi backlog chưa implemented sang TECH-10 như release-ready package.

TECH-12 không mở Global Gateway.

PHASE 0 chặn toàn bộ high-risk phase nếu foundation chưa pass.

PHASE 2 chặn Commerce nếu Operational Sellable/Sale Lock/Recall chưa pass.

PHASE 3 chặn AI/Ads/IVR nếu QuoteSnapshot/Official Order/Verified Revenue chưa pass.

PHASE 5 chặn MC AI Live nếu Gateway/Final Response Guard chưa pass.

PHASE 9 và TECH-10 mới kiểm soát release/go-live/scale.

## PHẦN 3/4 sẵn sàng bàn giao sang:

## PHẦN 4/4 - FINAL PHASE BACKLOG MATRIX / DEV TASK BREAKDOWN PACK / EVIDENCE-SMOKE MATRIX / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-12 FINAL CONCLUSION.

## PHẦN 4/4 - FINAL PHASE BACKLOG MATRIX / DEV TASK BREAKDOWN PACK / EVIDENCE-SMOKE MATRIX / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-12 FINAL CONCLUSION

## 1. MỤC TIÊU PHẦN 4/4

## PHẦN 4/4 khóa bản cuối của Phase Backlog Pack.

PHẦN này xác định rõ:

Final Phase Backlog Matrix theo PHASE 0 -> PHASE 9.

Dev Task Breakdown Pack cấp phase.

Evidence Matrix.

Smoke Matrix.

Owner Matrix.

Dependency Matrix.

bị chặn Backlog Matrix.

Ready Backlog Matrix.

Backlog Handoff Matrix sang TECH-11.

Release Boundary sang TECH-10.

Final Done Gate.

Final Fail Gate.

TECH-12 Final Conclusion.

## PHẦN 4/4 không viết code.

## PHẦN 4/4 không thiết kế API chi tiết.

## PHẦN 4/4 không thiết kế database chi tiết.

## PHẦN 4/4 không thiết kế UI chi tiết.

## PHẦN 4/4 chỉ khóa backlog pack, task breakdown, evidence-smoke matrix và handoff governance.

## 2. FINAL BACKLOG PRINCIPLE

TECH-12 khẳng định nguyên tắc cuối cùng:

Backlog không phải code.

Backlog Ready không phải Dev Ready.

Backlog Handoff không phải Implementation Complete.

Implementation Complete không phải Release Ready.

Release Ready chỉ được xét bởi TECH-10.

Một backlog chỉ hợp lệ khi có:

Source-of-truth.

Phase.

Domain.

Requirement summary.

Scope In.

Scope Out.

Upstream dependency.

Downstream handoff.

P0 điểm chặn.

Evidence required.

Smoke required.

Owner.

Report requirement.

Done Gate.

Fail Gate.

Initial status.

Nếu thiếu bất kỳ điểm cốt lõi nào:

Backlog không READY.

Nếu backlog chưa qua TECH-11:

Backlog không DEV READY.

Nếu chưa có implementation/evidence/smoke/QA/TECH-10:

Backlog không RELEASE READY.

## 3. FINAL PHASE BACKLOG MATRIX - PHASE 0

## PHASE 0 - FOUNDATION / RBAC / AUDIT / EVIDENCE / IDEMPOTENCY

## 3.1. Mục tiêu PHASE 0

PHASE 0 tạo nền kiểm soát bắt buộc cho toàn bộ hệ thống.

Không có PHASE 0, các phase sau không đủ:

Quyền.

Actor.

Audit.

Evidence.

Idempotency.

Correlation.

High-risk command guard.

Admin override governance.

## 3.2. Final Backlog Matrix PHASE 0

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## FND-BLG-001

RBAC / Permission Foundation

## TECH-01

None

Permission matrix, role mapping, denied-action evidence

User lacks permission -> action bị chặn

## READY FOR TECH-11 HANDOFF

## FND-BLG-002

Actor Identity / System Actor

## TECH-01

## FND-BLG-001

Actor log, system actor reference

Action must identify actor

## READY FOR TECH-11 HANDOFF

## FND-BLG-003

Audit Trail Foundation

## TECH-01

## FND-BLG-001/002

Audit log, before/after state

High-risk action creates audit

## READY FOR TECH-11 HANDOFF

## FND-BLG-004

Evidence Registry Foundation

## TECH-01 / TECH-10

## FND-BLG-003

Evidence ID, requirement mapping, environment

Evidence missing -> gate bị chặn

## READY FOR TECH-11 HANDOFF

## FND-BLG-005

Idempotency Foundation

## TECH-01

## FND-BLG-003

Idempotency key/result log

Duplicate command does not double action

## READY FOR TECH-11 HANDOFF

## FND-BLG-006

Correlation ID / Trace Context

## TECH-01

## FND-BLG-003/004

Correlation trace across actions

Trace can link request -> action -> evidence

## READY FOR TECH-11 HANDOFF

## FND-BLG-007

High-Risk Command Guard

## TECH-01 / TECH-10

## FND-BLG-001-006

Command guard evidence

High-risk command bị chặn if missing audit

## READY FOR TECH-11 HANDOFF

## FND-BLG-008

Admin Override Governance

## TECH-01 / TECH-10

## FND-BLG-001-007

Actor, reason, evidence, audit

Override without reason bị chặn

## READY FOR TECH-11 HANDOFF

## FND-BLG-009

Source-of-Truth Registry Foundation

## TECH-00 / TECH-10 / TECH-11

None

Active source registry, legacy boundary

Code cũ khác TECH mới -> TECH mới thắng

## READY FOR TECH-11 HANDOFF

## FND-BLG-010

Foundation Smoke / Evidence Pack

## TECH-10 / TECH-11

## FND-BLG-001-009

Smoke output, audit samples

Foundation smoke pass/fail

## READY FOR TECH-11 HANDOFF

## 3.3. PHASE 0 P0 điểm chặn

FAIL nếu:

High-risk command không audit.

Admin override không reason/evidence/audit.

Evidence không requirement mapping.

Permission không chặn action sai quyền.

Idempotency không chặn double action.

Phase sau dùng high-risk action khi PHASE 0 chưa pass.

## 4. FINAL PHASE BACKLOG MATRIX - PHASE 1

## PHASE 1 - PRODUCT / SKU / RECIPE / PRODUCT ACTIVATION

## 4.1. Mục tiêu PHASE 1

PHASE 1 tạo nền Product / SKU / Recipe / Product Activation.

PHASE này bảo đảm:

Product Master rõ.

SKU Master rõ.

Recipe/Formula version rõ.

Product Activation Guard rõ.

Product Active không bị hiểu là Sellable.

Public product name đúng.

Internal SKU code không public sai kênh.

## 4.2. Final Backlog Matrix PHASE 1

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## PRD-BLG-001

Product Master Governance

## TECH-02

## PHASE 0

Product master evidence

Product exists with required governance fields

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-002

SKU Master Governance

## TECH-02

## PRD-BLG-001

SKU registry evidence

SKU maps to product correctly

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-003

Recipe / Formula Version Governance

## TECH-02

## PRD-BLG-001/002

Formula version evidence

Formula version required before production usage

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-004

Formula Kind / Version Lock

## TECH-02

## PRD-BLG-003

Formula kind/version audit

Formula kind/version immutable in snapshot

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-005

Product Activation Guard

## TECH-02

## PRD-BLG-001-004

Activation decision evidence

Product cannot activate without required data

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-006

Product Active Not Sellable Rule

## TECH-02 / TECH-03 / TECH-04

## PRD-BLG-005

Active vs sellable test evidence

Product Active != Sellable

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-007

Public Product Name Policy

## TECH-02 / TECH-05 / TECH-06 / TECH-08

## PRD-BLG-002

Public name evidence

Public-facing name is full approved name

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-008

Internal SKU Code Privacy

## TECH-02 / TECH-05

## PRD-BLG-002

Internal/public boundary evidence

Internal SKU code not exposed to customer

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-009

Product Knowledge Approval

## TECH-02 / TECH-05

## PRD-BLG-001-008

Approved knowledge evidence

AI cannot use unapproved product knowledge

bị chặn UNTIL PHASE 0 PASS

## PRD-BLG-010

Product/SKU/Recipe Smoke Pack

## TECH-10 / TECH-11

## PRD-BLG-001-009

Phase smoke output

Product/SKU/Recipe smoke pass

bị chặn UNTIL PHASE 0 PASS

## 4.3. PHASE 1 P0 điểm chặn

FAIL nếu:

Product Active bị hiểu là Sellable.

SKU nội bộ public ra khách.

Product knowledge chưa approved nhưng AI/Gateway/Live dùng.

Formula không version nhưng Production Order snapshot.

Product activation không có evidence/audit.

## 5. FINAL PHASE BACKLOG MATRIX - PHASE 2

## PHASE 2 - OPERATIONAL CORE

## 5.1. Mục tiêu PHASE 2

PHASE 2 tạo nền Operational Core:

Sản xuất.

Kho.

Tồn kho.

## QC.

Batch release.

Traceability.

Public trace.

Recall.

Sale Lock.

## 5.2. Final Backlog Matrix PHASE 2

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## OPS-BLG-001

Source Origin / Raw Intake Governance

## TECH-03

## PHASE 0/1

Intake evidence, source verification

Unverified source blocks intake

bị chặn UNTIL PHASE 0/1 PASS

## OPS-BLG-002

Raw QC / Raw Lot Readiness

## TECH-03

## OPS-BLG-001

QC result, mark-ready evidence

## QC_PASS != READY_FOR_PRODUCTION

bị chặn UNTIL PHASE 0/1 PASS

## OPS-BLG-003

Recipe Snapshot Into Production Order

## TECH-02 / TECH-03

## PRD-BLG-003/004

Snapshot evidence

PO requires formula kind/version snapshot

bị chặn UNTIL PHASE 1 PASS

## OPS-BLG-004

Material Request / Issue / Receipt

## TECH-03

## OPS-BLG-002/003

Ledger evidence, issue/receipt evidence

Issue decrements raw stock once

bị chặn UNTIL PHASE 1 PASS

## OPS-BLG-005

Batch Execution / Process Step Control

## TECH-03

## OPS-BLG-004

Process step evidence

Process steps must complete in allowed order

bị chặn UNTIL PHASE 1 PASS

## OPS-BLG-006

Packaging / QR / Print Control

## TECH-03

## OPS-BLG-005

QR/print evidence

Failed/void QR not public valid

bị chặn UNTIL PHASE 1 PASS

## OPS-BLG-007

QC Inspection / Batch Release

## TECH-03

## OPS-BLG-005/006

QC/release evidence

## QC_PASS != RELEASED

bị chặn UNTIL PHASE 1 PASS

## OPS-BLG-008

Warehouse Receipt / Finished Goods Stock

## TECH-03

## OPS-BLG-007

Warehouse receipt evidence

Only RELEASED batch can enter warehouse

bị chặn UNTIL PHASE 1 PASS

## OPS-BLG-009

Inventory Ledger Append-only

## TECH-03 / TECH-01

## OPS-BLG-004/008

Ledger append-only evidence

Ledger correction does not mutate history

bị chặn UNTIL PHASE 0 PASS

## OPS-BLG-010

Traceability / Public Trace Whitelist

## TECH-03

## OPS-BLG-006-009

Trace evidence, public whitelist evidence

Public trace exposes whitelist only

bị chặn UNTIL PHASE 1 PASS

## OPS-BLG-011

Recall / Sale Lock Control

## TECH-03 / TECH-04 / TECH-10

## OPS-BLG-010

Recall/sale lock evidence

Recall/Sale Lock blocks downstream

bị chặn UNTIL PHASE 1 PASS

## OPS-BLG-012

Operational Smoke / Evidence Pack

## TECH-10 / TECH-11

## OPS-BLG-001-011

E2E operational smoke output

Operational chain pass/fail

bị chặn UNTIL OPS-BLG-001-011 PASS

## 5.3. PHASE 2 P0 điểm chặn

FAIL nếu:

RAW_LOT QC_PASS bị hiểu là READY_FOR_PRODUCTION.

Raw lot chưa READY_FOR_PRODUCTION vẫn issue.

Material Issue không phải điểm giảm tồn nguyên liệu chính.

Material Receipt giảm tồn lần hai.

Batch QC_PASS bị hiểu là RELEASED.

Warehouse nhận batch chưa RELEASED.

Public Trace không whitelist-only.

Recall/Sale Lock không chặn downstream.

## 6. FINAL PHASE BACKLOG MATRIX - PHASE 3

## PHASE 3 - COMMERCE RUNTIME

## 6.1. Mục tiêu PHASE 3

PHASE 3 xây nền Commerce Runtime:

Sellable Gate.

QuoteSnapshot.

Cart.

Order Draft.

Customer Confirmation.

Official Order.

Payment.

COD/Shipping.

Verified Revenue.

Program/Member/Diamond benefit runtime.

## 6.2. Final Backlog Matrix PHASE 3

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## COM-BLG-001

Sellable Gate From Operational

## TECH-03 / TECH-04

## PHASE 2

Sellable decision evidence

Operational bị chặn -> Commerce bị chặn

bị chặn UNTIL PHASE 2 PASS

## COM-BLG-002

QuoteSnapshot As Only Final Price Source

## TECH-04 / TECH-05 / TECH-06 / TECH-08

## COM-BLG-001

QuoteSnapshot evidence

AI/Gateway/Live cannot self-price

bị chặn UNTIL COM-BLG-001 PASS

## COM-BLG-003

Cart Not Order Boundary

## TECH-04

## COM-BLG-002

Cart state evidence

Cart cannot create order_code

bị chặn UNTIL COM-BLG-002 PASS

## COM-BLG-004

Order Draft Not Official Order Boundary

## TECH-04 / TECH-05

## COM-BLG-002/003

Draft evidence

Draft cannot trigger IVR/payment/revenue

bị chặn UNTIL COM-BLG-002 PASS

## COM-BLG-005

Customer Confirmation To Official Order

## TECH-04

## COM-BLG-004

Customer confirmation evidence

CONFIRMED creates official order

bị chặn UNTIL COM-BLG-004 PASS

## COM-BLG-006

Official Order State Machine

## TECH-04 / TECH-09

## COM-BLG-005

Order state evidence

Official order state transitions valid

bị chặn UNTIL COM-BLG-005 PASS

## COM-BLG-007

Payment Confirmation Boundary

## TECH-04

## COM-BLG-006

Payment confirmation evidence

Image transfer != PAID

bị chặn UNTIL COM-BLG-006 PASS

## COM-BLG-008

COD / Delivery / Shipping Boundary

## TECH-04

## COM-BLG-006/007

COD/delivery evidence

COD WAITING != revenue

bị chặn UNTIL COM-BLG-006 PASS

## COM-BLG-009

Verified Revenue Resolver

## TECH-04 / TECH-07

## COM-BLG-007/008

Verified revenue evidence

Ads only uses Verified Revenue

bị chặn UNTIL COM-BLG-007/008 PASS

## COM-BLG-010

Program / Member / Diamond Runtime Benefit

## TECH-04 / TECH-05 / TECH-07

## COM-BLG-002

Runtime benefit evidence

Benefit from runtime, no hardcode

bị chặn UNTIL COM-BLG-002 PASS

## COM-BLG-011

Bank Account Registry / No Hardcode Payment Account

## TECH-04

## COM-BLG-007

Bank registry evidence

Payment account not hardcoded

bị chặn UNTIL COM-BLG-007 PASS

## COM-BLG-012

Commerce Smoke / Evidence Pack

## TECH-10 / TECH-11

## COM-BLG-001-011

Commerce smoke output

Quote/order/payment/revenue smoke pass

bị chặn UNTIL COM-BLG-001-011 PASS

## 6.3. PHASE 3 P0 điểm chặn

FAIL nếu:

Operational bị chặn nhưng Commerce vẫn bán.

Không QuoteSnapshot mà vẫn final price.

Cart bị hiểu là Order.

Order Draft bị hiểu là Official Order.

CustomerConfirmation chưa CONFIRMED mà tạo official order.

Ảnh chuyển khoản được coi là PAID.

COD WAITING được coi là Verified Revenue.

Verified Revenue không đến từ Commerce.

Payment account hardcode.

## 7. FINAL PHASE BACKLOG MATRIX - PHASE 4

## PHASE 4 - AI ADVISOR RUNTIME

## 7.1. Mục tiêu PHASE 4

PHASE 4 xây AI Advisor Runtime.

AI Advisor là lớp tư vấn, diễn giải, gợi ý và hỗ trợ chốt đơn có kiểm soát.

AI không phải nguồn sự thật nghiệp vụ.

## 7.2. Final Backlog Matrix PHASE 4

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## AIA-BLG-001

Product Knowledge Resolver

## TECH-02 / TECH-05

## PHASE 1

Approved knowledge evidence

AI only uses approved knowledge

bị chặn UNTIL PHASE 1 PASS

## AIA-BLG-002

Claim Guard / Public Wording Policy

## TECH-05

## AIA-BLG-001

Claim guard evidence

Unapproved claim bị chặn

bị chặn UNTIL AIA-BLG-001 PASS

## AIA-BLG-003

Customer Memory Resolver

## TECH-05

## PHASE 0/3

Customer memory evidence

Private memory not public

bị chặn UNTIL PHASE 0/3 PASS

## AIA-BLG-004

QuoteSnapshot Consumption

## TECH-04 / TECH-05

## COM-BLG-002

Quote consumption evidence

AI cannot self-price

bị chặn UNTIL COM-BLG-002 PASS

## AIA-BLG-005

Order Draft Handoff

## TECH-04 / TECH-05

## COM-BLG-004

Order draft handoff evidence

AI cannot create official order

bị chặn UNTIL COM-BLG-004 PASS

## AIA-BLG-006

Final Response Guard

## TECH-05 / TECH-06

## AIA-BLG-001-005

Guard evidence

Response not sent if guard fail

bị chặn UNTIL AIA-BLG-001-005 PASS

## AIA-BLG-007

Public/Private Data Boundary

## TECH-05 / TECH-06

## AIA-BLG-006

Privacy evidence

Private data bị chặn in public

bị chặn UNTIL AIA-BLG-006 PASS

## AIA-BLG-008

Product Name Public Policy

## TECH-02 / TECH-05 / TECH-08

## AIA-BLG-001/006

Public wording evidence

Full public product name required

bị chặn UNTIL AIA-BLG-006 PASS

## AIA-BLG-009

Runtime Unavailable Fail-safe

## TECH-05 / TECH-10

## AIA-BLG-004-007

Fail-safe evidence

Runtime unavailable -> no guessing

bị chặn UNTIL AIA-BLG-004-007 PASS

## AIA-BLG-010

AI Advisor Smoke / Evidence Pack

## TECH-10 / TECH-11

## AIA-BLG-001-009

AI smoke output

AI no-price/no-order/no-payment smoke pass

bị chặn UNTIL AIA-BLG-001-009 PASS

## 7.3. PHASE 4 P0 điểm chặn

FAIL nếu:

AI tự tính giá.

AI tự tạo official order.

AI tự xác nhận payment.

AI tự xác nhận Verified Revenue.

AI bán SKU not sellable/Sale Lock/Recall.

AI dùng claim chưa approved.

AI public dữ liệu private.

AI nói chữa bệnh/điều trị/thay thuốc/cam kết hiệu quả y khoa.

## 8. FINAL PHASE BACKLOG MATRIX - PHASE 5

## PHASE 5 - FACEBOOK GATEWAY / MESSENGER / CHANNEL DELIVERY

## 8.1. Mục tiêu PHASE 5

PHASE 5 xây Facebook Gateway / Messenger / Channel Delivery.

Gateway là lớp điều phối kênh và delivery, không phải AI, Commerce hoặc Payment.

## 8.2. Final Backlog Matrix PHASE 5

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## GW-BLG-001

Channel Identity / Page Context

## TECH-06

## PHASE 0

Page/channel context evidence

Channel context resolved

bị chặn UNTIL PHASE 0 PASS

## GW-BLG-002

Public Comment Boundary

## TECH-06

## GW-BLG-001

Public/private evidence

Public comment no private data

bị chặn UNTIL GW-BLG-001 PASS

## GW-BLG-003

Comment-to-Messenger Routing

## TECH-06

## GW-BLG-002

Handoff evidence

Price/buy/deep consult routes private

bị chặn UNTIL GW-BLG-002 PASS

## GW-BLG-004

Messenger Private Context Handoff

## TECH-06 / TECH-05

## GW-BLG-003/AIA-BLG-006

Messenger context evidence

After handoff conversation private

bị chặn UNTIL GW-BLG-003 + AIA PASS

## GW-BLG-005

Final Response Guard Enforcement

## TECH-05 / TECH-06

## AIA-BLG-006

Guard enforcement evidence

Unguarded response not sent

bị chặn UNTIL AIA-BLG-006 PASS

## GW-BLG-006

Typing Indicator / Response Pacing

## TECH-06

## GW-BLG-005

Pacing evidence

Gateway does not reply instantly like bot

bị chặn UNTIL GW-BLG-005 PASS

## GW-BLG-007

Rate Limit / Anti-Spam

## TECH-06

## GW-BLG-001

Rate-limit evidence

Rate limit blocks spam

bị chặn UNTIL GW-BLG-001 PASS

## GW-BLG-008

Moderation / Troll / Complaint Split

## TECH-06 / TECH-08

## GW-BLG-002

Moderation evidence

Troll not Messenger; complaint routed CSKH

bị chặn UNTIL GW-BLG-002 PASS

## GW-BLG-009

Suppression / Opt-out

## TECH-06 / TECH-10

## GW-BLG-007

Suppression evidence

Suppressed user not messaged

bị chặn UNTIL GW-BLG-007 PASS

## GW-BLG-010

Gateway Smoke / Evidence Pack

## TECH-10 / TECH-11

## GW-BLG-001-009

Gateway smoke output

Public/private/rate/moderation smoke pass

bị chặn UNTIL GW-BLG-001-009 PASS

## 8.3. PHASE 5 P0 điểm chặn

FAIL nếu:

Gateway public dữ liệu private.

Gateway public giá cá nhân/order info sai rule.

Gateway gửi response chưa qua Final Response Guard.

Hỏi giá/mua/chốt trong public không kéo Messenger/private.

Troll/malicious bị kéo Messenger sai policy.

Complaint thật bị xử như troll.

Gateway giả mạo người thật.

## 9. FINAL PHASE BACKLOG MATRIX - PHASE 6

## PHASE 6 - ADS MEASUREMENT

## 9.1. Mục tiêu PHASE 6

PHASE 6 xây Ads Measurement / Attribution / ROAS / Scale Gate.

Ads chỉ dùng Verified Revenue từ Commerce.

## 9.2. Final Backlog Matrix PHASE 6

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## ADS-BLG-001

Event Taxonomy / Funnel Signal Boundary

## TECH-07

## PHASE 0/5

Event taxonomy evidence

Funnel signal != revenue

bị chặn UNTIL PHASE 0 PASS

## ADS-BLG-002

Pixel / CAPI Dedup / Idempotency

## TECH-07 / TECH-01

## ADS-BLG-001/FND-BLG-005

Dedup evidence

Duplicate event not double counted

bị chặn UNTIL PHASE 0 PASS

## ADS-BLG-003

Attribution Source Resolver

## TECH-07

## ADS-BLG-001/002

Attribution evidence

Attribution uses valid source/bind

bị chặn UNTIL ADS-BLG-002 PASS

## ADS-BLG-004

Verified Revenue Consumption

## TECH-04 / TECH-07

## COM-BLG-009

Revenue source evidence

Ads only consumes Verified Revenue

bị chặn UNTIL COM-BLG-009 PASS

## ADS-BLG-005

Exclusion Rules: Quote/Cart/Draft/Unpaid/WAITING

## TECH-04 / TECH-07

## ADS-BLG-004

Exclusion evidence

Quote/cart/draft/unpaid/WAITING excluded

bị chặn UNTIL ADS-BLG-004 PASS

## ADS-BLG-006

CPA / AOV / ROAS Computation Boundary

## TECH-07

## ADS-BLG-004/005

Computation evidence

ROAS computed from verified revenue only

bị chặn UNTIL ADS-BLG-005 PASS

## ADS-BLG-007

Data Quality Gate

## TECH-07 / TECH-10

## ADS-BLG-002-006

Data quality evidence

Data Quality fail blocks scale

bị chặn UNTIL ADS-BLG-006 PASS

## ADS-BLG-008

Scale Gate / Owner Approval

## TECH-07 / TECH-10

## ADS-BLG-007

Owner approval evidence

No owner approval -> no scale

bị chặn UNTIL ADS-BLG-007 PASS

## ADS-BLG-009

Sale Lock / Recall / Suppression Blocks Scale

## TECH-03 / TECH-07

## OPS-BLG-011/ADS-BLG-008

Suppression evidence

Sale Lock/Recall blocks scale

bị chặn UNTIL OPS-BLG-011 PASS

## ADS-BLG-010

Ads Smoke / Evidence Pack

## TECH-10 / TECH-11

## ADS-BLG-001-009

Ads smoke output

No fake revenue / no bad scale smoke pass

bị chặn UNTIL ADS-BLG-001-009 PASS

## 9.3. PHASE 6 P0 điểm chặn

FAIL nếu:

Ads dùng quote/cart/order draft làm revenue.

Unpaid order/payment WAITING/COD WAITING được tính revenue.

Verified Revenue không từ Commerce.

Pixel/CAPI double count.

Scale khi Data Quality fail.

Scale khi Sale Lock/Recall/Suppression active.

Scale chỉ vì ROAS đẹp nhưng evidence chưa accepted.

## 10. FINAL PHASE BACKLOG MATRIX - PHASE 7

## PHASE 7 - MC AI LIVE

## 10.1. Mục tiêu PHASE 7

PHASE 7 xây MC AI Live / Live Script Runtime / Live Sales Control.

MC AI Live không phải nguồn sự thật nghiệp vụ.

## 10.2. Final Backlog Matrix PHASE 7

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## LIVE-BLG-001

Live Session Context Resolver

## TECH-08

## PHASE 5

Live session evidence

Session context resolved

bị chặn UNTIL PHASE 5 PASS

## LIVE-BLG-002

Live Script Runtime

## TECH-08

## LIVE-BLG-001

Script runtime evidence

Script loaded from approved source

bị chặn UNTIL LIVE-BLG-001 PASS

## LIVE-BLG-003

Script Guard / Claim Guard

## TECH-05 / TECH-08

## AIA-BLG-002/LIVE-BLG-002

Guard evidence

Unguarded script bị chặn

bị chặn UNTIL AIA-BLG-002 PASS

## LIVE-BLG-004

Live Comment Classifier

## TECH-06 / TECH-08

## GW-BLG-008

Comment classifier evidence

Price/buy/troll/complaint classified

bị chặn UNTIL GW-BLG-008 PASS

## LIVE-BLG-005

Comment-to-Messenger Coordination

## TECH-06 / TECH-08

## GW-BLG-003/004

Handoff evidence

Public reply + Messenger after handoff

bị chặn UNTIL GW-BLG-004 PASS

## LIVE-BLG-006

Product/Price Boundary In Live

## TECH-04 / TECH-08

## COM-BLG-002

Price boundary evidence

Live cannot self-price

bị chặn UNTIL COM-BLG-002 PASS

## LIVE-BLG-007

Troll / Abuse / Malicious Handling

## TECH-06 / TECH-08

## GW-BLG-008

Moderation evidence

Troll not pulled to Messenger

bị chặn UNTIL GW-BLG-008 PASS

## LIVE-BLG-008

Complaint / CSKH / Quality Route

## TECH-06 / TECH-08

## GW-BLG-008

Complaint route evidence

Complaint not treated as troll

bị chặn UNTIL GW-BLG-008 PASS

## LIVE-BLG-009

Ads-safe Live Signal Boundary

## TECH-07 / TECH-08

## ADS-BLG-001/004

Signal boundary evidence

Live signal not revenue/ROAS

bị chặn UNTIL ADS-BLG-004 PASS

## LIVE-BLG-010

MC AI Live Smoke / Evidence Pack

## TECH-10 / TECH-11

## LIVE-BLG-001-009

Live smoke output

Live script/comment/suppression smoke pass

bị chặn UNTIL LIVE-BLG-001-009 PASS

## 10.3. PHASE 7 P0 điểm chặn

FAIL nếu:

MC AI Live tự báo giá.

MC AI Live tự tạo order.

MC AI Live tự xác nhận payment/revenue.

Live signal dùng làm ROAS.

Script chưa guard vẫn dùng.

Claim chưa approved vẫn nói.

Troll/malicious bị kéo Messenger sai policy.

Complaint thật bị xử như troll.

## 11. FINAL PHASE BACKLOG MATRIX - PHASE 8

## PHASE 8 - IVR ORDER CONFIRMATION

## 11.1. Mục tiêu PHASE 8

PHASE 8 xây IVR Order Confirmation.

IVR chỉ xử lý Official Order đủ điều kiện theo Commerce.

IVR không gọi đại trà và không xác nhận payment/revenue.

## 11.2. Final Backlog Matrix PHASE 8

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## IVR-BLG-001

IVR Eligibility Resolver

## TECH-09

## COM-BLG-006

Eligibility evidence

Only eligible official order enters IVR

bị chặn UNTIL COM-BLG-006 PASS

## IVR-BLG-002

Customer Trust / Existing Trusted Exclusion

## TECH-09

IVR-BLG-001 / TECH-05 Customer Memory

Trust evidence

Trusted customer not called đại trà

bị chặn UNTIL IVR-BLG-001 PASS

## IVR-BLG-003

Official Order Only Boundary

## TECH-04 / TECH-09

## COM-BLG-006

Official order evidence

Quote/cart/draft cannot enter IVR

bị chặn UNTIL COM-BLG-006 PASS

## IVR-BLG-004

Phone Validation / Invalid Phone

## TECH-09

## IVR-BLG-003

Phone validation evidence

bị chặn UNTIL IVR-BLG-003 PASS

## IVR-BLG-005

Call Attempt Policy

## TECH-09

## IVR-BLG-001/004

Attempt policy evidence

2 attempts baseline; spacing respected

bị chặn UNTIL IVR-BLG-004 PASS

## IVR-BLG-006

Outcome Classification

## TECH-09

## IVR-BLG-005

Outcome evidence

Confirm/cancel/no answer/technical separated

bị chặn UNTIL IVR-BLG-005 PASS

## IVR-BLG-007

Customer Confirm / Cancel / Need Support

## TECH-09

## IVR-BLG-006

Keypress/outcome evidence

bị chặn UNTIL IVR-BLG-006 PASS

## IVR-BLG-008

No Answer / Max Attempts

## TECH-09

## IVR-BLG-005/006

No answer evidence

No answer max only handoff Core

bị chặn UNTIL IVR-BLG-006 PASS

## IVR-BLG-009

Core Order State / Notification Handoff

## TECH-04 / TECH-09

## COM-BLG-006 / IVR-BLG-006-008

Core/notification evidence

Notification only after Core cancellation

bị chặn UNTIL COM-BLG-006 + IVR OUTCOMES PASS

## IVR-BLG-010

IVR Evidence / Audit / Smoke Pack

## TECH-09 / TECH-10 / TECH-11

## IVR-BLG-001-009

IVR smoke output

IVR official/no answer/cancel/privacy smoke pass

bị chặn UNTIL IVR-BLG-001-009 PASS

## 11.3. PHASE 8 P0 điểm chặn

FAIL nếu:

IVR gọi mọi khách.

IVR gọi Quote/Cart/Order Draft.

IVR tự tạo order.

IVR tự hủy đơn.

IVR tự gửi notification.

IVR confirmation bị hiểu là PAID.

Invalid phone/no answer/customer cancel/technical failure bị trộn.

Notification gửi trước Core Order State Machine hủy chính thức.

## 12. FINAL PHASE BACKLOG MATRIX - PHASE 9

## PHASE 9 - GLOBAL SMOKE / UAT / RELEASE GATEWAY

## 12.1. Mục tiêu PHASE 9

PHASE 9 xây Global Release Governance:

Evidence.

Smoke.

## UAT.

Owner sign-off.

P0 điểm chặn.

Cross-tech dependency.

Privacy/security.

Rollback/fallback.

Monitoring/alert.

Release decision.

Global Gateway.

Post-go-live.

Scale approval.

## 12.2. Final Backlog Matrix PHASE 9

Backlog ID

Backlog Name

Source-of-Truth

Dependency

Evidence Required

Smoke Required

Initial Status

## REL-BLG-001

Documentation Completion Registry

## TECH-10

TECH-00-12 docs

Documentation evidence

Documentation Complete != Production Ready

bị chặn UNTIL TECH-12 COMPLETE

## REL-BLG-002

Evidence Intake / Acceptance

## TECH-10

## REL-BLG-001

Evidence review evidence

Evidence submitted != accepted

bị chặn UNTIL REL-BLG-001 PASS

## REL-BLG-003

Global Smoke Matrix

## TECH-10

## REL-BLG-002

Smoke execution evidence

Smoke fail blocks release

bị chặn UNTIL REL-BLG-002 PASS

## REL-BLG-004

UAT Session Control

## TECH-10

## REL-BLG-003

UAT evidence

Dev self-test != UAT

bị chặn UNTIL REL-BLG-003 PASS

## REL-BLG-005

Owner Sign-off Control

## TECH-10

## REL-BLG-004

Sign-off evidence

Missing owner -> no Release Approved

bị chặn UNTIL REL-BLG-004 PASS

## REL-BLG-006

P0 điểm chặn Registry

## TECH-10 / TECH-11 / TECH-12

## REL-BLG-003

điểm chặn evidence

Open P0 -> Gateway bị chặn

bị chặn UNTIL REL-BLG-003 PASS

## REL-BLG-007

Cross-Tech Dependency Validator

## TECH-10

## REL-BLG-003/006

Dependency evidence

Upstream fail -> downstream bị chặn

bị chặn UNTIL REL-BLG-003 PASS

## REL-BLG-008

Privacy / Security Release Review

## TECH-10

## REL-BLG-002/003

Privacy/security evidence

Privacy fail blocks release

bị chặn UNTIL REL-BLG-002 PASS

## REL-BLG-009

Rollback / Fallback / Monitoring Readiness

## TECH-10

## REL-BLG-003/008

Rollback/monitoring evidence

Missing rollback/monitoring blocks go-live

bị chặn UNTIL REL-BLG-008 PASS

## REL-BLG-010

Release Decision / Global Gateway State

## TECH-10

## REL-BLG-001-009

Release decision evidence

No release decision -> no Go-live

bị chặn UNTIL REL-BLG-001-009 PASS

## REL-BLG-011

Post-Go-Live Monitoring

## TECH-10

## REL-BLG-010

Post-go-live evidence

Incident open blocks scale

bị chặn UNTIL GO-LIVE DECISION

## REL-BLG-012

Scale Approval Gate

## TECH-07 / TECH-10

## REL-BLG-011 / ADS-BLG-007

Scale evidence

Data Quality/Verified Revenue fail blocks scale

bị chặn UNTIL POST-GO-LIVE STABLE

## 12.3. PHASE 9 P0 điểm chặn

FAIL nếu:

Documentation Complete bị gọi là Production Ready.

Evidence chưa accepted nhưng Completion Decision.

Smoke chưa pass nhưng Release Ready.

Owner chưa sign-off nhưng Release Approved.

Không release decision nhưng Go-live Approved.

Global Gateway mở khi còn điểm chặn.

Post-go-live incident open nhưng scale.

Data Quality/Verified Revenue fail nhưng scale.

## 13. FINAL DEV TASK BREAKDOWN PACK

## 13.1. Task Breakdown Rule

Mỗi backlog khi đưa sang TECH-11 phải được chia thành task theo nguyên tắc:

Task không được mất source-of-truth.

Task không được mất dependency.

Task không được mất Scope Out.

Task không được mất P0 điểm chặn.

Task không được mất evidence.

Task không được mất smoke.

Task không được biến thành code snippet.

Task không được yêu cầu copy-paste code.

Task không được hardcode policy.

Task phải có report requirement.

## 13.2. Task Breakdown Template

Mỗi dev task phải có:

Task ID.

Parent Backlog ID.

Phase.

Domain.

Source-of-Truth.

Requirement Summary.

Scope In.

Scope Out.

Dependency.

P0 điểm chặn.

Evidence Required.

Smoke Required.

Report Required.

Handoff Target.

Status.

## 13.3. Task Breakdown Initial Status

Các task con sau TECH-12 chỉ được có trạng thái:

## READY FOR TECH-11 HANDOFF.

bị chặn BY SOURCE.

bị chặn BY DEPENDENCY.

## NEED OWNER REVIEW.

## NEED EVIDENCE/SMOKE REVIEW.

## PLANNING ONLY.

Không task nào được gọi là:

## DEV READY.

## IMPLEMENTED.

## RELEASE READY.

## PRODUCTION READY.

## GO-LIVE APPROVED.

## 14. FINAL EVIDENCE MATRIX

## 14.1. Evidence theo phase

Phase

Evidence bắt buộc

## PHASE 0

RBAC, actor, audit, evidence registry, idempotency, correlation, override evidence

## PHASE 1

Product/SKU/Recipe/Formula/Activation evidence

## PHASE 2

QC, readiness, material issue, batch release, warehouse, ledger, trace, recall/sale lock evidence

## PHASE 3

Sellable, QuoteSnapshot, cart/order draft/official order, payment, COD, verified revenue evidence

## PHASE 4

Product knowledge, claim guard, customer memory, quote consumption, response guard evidence

## PHASE 5

Public/private routing, Messenger handoff, guard enforcement, pacing, rate-limit, moderation evidence

## PHASE 6

Event taxonomy, dedup, attribution, verified revenue, data quality, scale gate evidence

## PHASE 7

Live session, script guard, comment classification, Messenger coordination, live signal boundary evidence

## PHASE 8

IVR eligibility, phone validation, attempt policy, outcome classification, notification handoff evidence

## PHASE 9

Evidence acceptance, smoke, UAT, sign-off, dependency, release decision, gateway, post-go-live evidence

## 14.2. Evidence P0 Rule

FAIL nếu:

Evidence không map requirement.

Evidence không rõ environment.

Evidence không có expected/actual.

Evidence không có reviewer.

Evidence thiếu audit/correlation cho high-risk action.

Evidence lộ dữ liệu nhạy cảm.

Evidence local dùng để production-ready.

Evidence chưa accepted nhưng release gate pass.

## 15. FINAL SMOKE MATRIX

## 15.1. Smoke theo phase

Phase

Smoke bắt buộc

## PHASE 0

Permission deny, audit creation, evidence missing blocks gate, idempotency duplicate

## PHASE 1

Product Active != Sellable, internal SKU not public, unapproved product knowledge bị chặn

## PHASE 2

Raw QC_PASS != READY, issue decrements once, QC_PASS != RELEASED, recall blocks downstream

## PHASE 3

QuoteSnapshot only, cart not order, draft not official, image transfer not PAID, COD WAITING not revenue

## PHASE 4

AI no self-price, no self-order, no self-payment, unapproved claim bị chặn, private data not public

## PHASE 5

Public price intent routes Messenger, unguarded response bị chặn, troll not pulled Messenger, complaint routed CSKH

## PHASE 6

Quote/cart/draft/unpaid excluded revenue, dedup works, data quality fail blocks scale

## PHASE 7

Script guard, live no self-price/order/payment, live signal not ROAS, troll/complaint split

## PHASE 8

## PHASE 9

Evidence accepted required, smoke pass required, owner sign-off required, release decision required, Gateway bị chặn by default

## 15.2. Smoke P0 Rule

FAIL nếu:

Smoke chỉ happy path.

Smoke không có Must Not.

Smoke không có expected/actual.

Smoke không có evidence.

Smoke không test dependency.

Smoke không test runtime unavailable khi cần.

Smoke không test privacy boundary khi có dữ liệu khách.

Smoke không test no-bypass rule.

## 16. FINAL OWNER MATRIX

Phase

Owner bắt buộc

## PHASE 0

Security/RBAC Owner, Audit/Evidence Owner, Dev Lead, QA Owner

## PHASE 1

Product Owner, Recipe Owner, Claim/Product Knowledge Owner, QA Owner

## PHASE 2

Production Owner, Warehouse Owner, Inventory Owner, Trace/Recall Owner, QA Owner

## PHASE 3

Commerce Owner, Payment Owner, Shipping Owner, Revenue Owner, QA Owner

## PHASE 4

AI Advisor Owner, Claim Guard Owner, Commerce Owner, Privacy Owner, QA Owner

## PHASE 5

Gateway Owner, Channel Owner, AI Owner, Privacy Owner, QA Owner

## PHASE 6

Ads Owner, Commerce Revenue Owner, Data Quality Owner, QA Owner

## PHASE 7

MC Live Owner, Gateway Owner, AI Owner, Commerce Owner, Ads Owner, QA Owner

## PHASE 8

IVR Owner, Commerce/Core Order Owner, Notification Owner, Privacy Owner, QA Owner

## PHASE 9

Release Owner, QA/UAT Owner, Privacy/Security Owner, Domain Owners, Global Gateway Owner

FAIL nếu backlog không có owner đúng domain.

## 17. FINAL DEPENDENCY MATRIX

Dependency

Rule

PHASE 0 -> All

Foundation chưa pass thì high-risk phase chỉ planning-only

## PHASE 1 -> PHASE 2

Product/SKU/Recipe chưa pass thì Operational bị chặn

## PHASE 2 -> PHASE 3

Operational/Sellable/Sale Lock/Recall chưa pass thì Commerce bị chặn

## PHASE 3 -> PHASE 4

QuoteSnapshot/Order boundary chưa pass thì AI bị chặn

## PHASE 4 -> PHASE 5

Final Response Guard chưa pass thì Gateway bị chặn

## PHASE 5 -> PHASE 7

Gateway public/private chưa pass thì MC Live bị chặn

## PHASE 3 -> PHASE 6

Verified Revenue chưa pass thì Ads bị chặn

## PHASE 3 -> PHASE 8

Official Order/Core Order chưa pass thì IVR bị chặn

PHASE 9 -> Release

TECH-10 gate chưa pass thì release/go-live/scale bị chặn

FAIL nếu downstream Ready/Release khi upstream critical bị chặn.

## 18. FINAL bị chặn BACKLOG MATRIX

Backlog phải bị chặn nếu có một trong các điều kiện:

Source-of-truth chưa rõ.

Source conflict chưa resolved.

Scope In/Out chưa rõ.

Dependency chưa pass.

Upstream bị chặn.

Owner thiếu.

Evidence requirement thiếu.

Smoke requirement thiếu.

P0 điểm chặn mở.

Report requirement thiếu.

Change control đang mở.

Có risk hardcode policy.

Có risk copy-paste code.

Handoff target chưa rõ.

TECH-11 hoặc TECH-10 gate chưa sẵn sàng.

## 19. FINAL READY BACKLOG MATRIX

Backlog chỉ READY FOR TECH-11 HANDOFF khi có đủ:

Source mapping accepted.

Scope In/Out accepted.

Dependency reviewed.

Owner assigned.

Evidence requirement accepted.

Smoke requirement accepted.

Report requirement accepted.

P0 điểm chặn defined.

No open source conflict.

No upstream critical điểm chặn.

Task breakdown accepted.

Phase backlog review pass.

Change control closed.

Handoff target clear.

READY FOR TECH-11 HANDOFF không phải Dev Ready.

TECH-11 mới kiểm tra Dev Handoff và Code Handoff Control.

## 20. FINAL BACKLOG HANDOFF MATRIX SANG TECH-11

Handoff sang TECH-11 phải có:

Phase.

Backlog IDs.

Dev task IDs.

Source-of-truth.

Requirement summary.

Scope In.

Scope Out.

Dependency.

P0 điểm chặn.

Evidence required.

Smoke required.

Owner.

Report requirement.

Initial status.

bị chặn backlog list.

Risk list.

Change control status.

Handoff receiver.

FAIL nếu handoff thiếu bất kỳ thành phần critical nào.

## 21. RELEASE BOUNDARY SANG TECH-10

TECH-12 không trực tiếp tạo Release Ready.

TECH-12 chỉ bàn giao backlog sang TECH-11.

TECH-10 chỉ nhận release review sau khi:

TECH-11 dev handoff đã hoàn tất.

Code implementation thật đã làm xong.

Dev report accepted.

Evidence mapped.

Smoke mapped.

QA handoff completed nếu required.

Phase exit accepted.

Release handoff package created.

Do đó:

TECH-12 -> TECH-11 -> Implementation -> QA -> TECH-10

Không có đường:

TECH-12 -> Release Ready

Không có đường:

TECH-12 -> Go-live Approved

Không có đường:

TECH-12 -> Global Gateway Open

## 22. FINAL DONE GATE - TECH-12

TECH-12 đạt DOCUMENTATION COMPLETE khi:

## PHẦN 1/4 hoàn tất.

## PHẦN 2/4 hoàn tất.

## PHẦN 3/4 hoàn tất.

## PHẦN 4/4 hoàn tất.

Backlog Pack Principles đã khóa.

No-Unmapped-Task Rule đã khóa.

Source-to-Backlog Boundary đã khóa.

Backlog Unit Standard đã khóa.

Phase Backlog Registry đã khóa.

Module Contracts đã khóa.

Backlog Lifecycle đã khóa.

Task State Machine đã khóa.

Source Mapping State Model đã khóa.

Dependency Matrix State Model đã khóa.

Evidence Requirement State Model đã khóa.

Smoke Requirement State Model đã khóa.

Owner Assignment State Model đã khóa.

Backlog Readiness State Model đã khóa.

điểm chặn State Model đã khóa.

Task Breakdown State Model đã khóa.

Handoff State Model đã khóa.

Change Control State Model đã khóa.

Final Phase Backlog Matrix đã khóa.

Dev Task Breakdown Pack đã khóa.

Evidence Matrix đã khóa.

Smoke Matrix đã khóa.

Owner Matrix đã khóa.

Dependency Matrix đã khóa.

bị chặn/Ready Matrix đã khóa.

Handoff Matrix sang TECH-11 đã khóa.

Release Boundary sang TECH-10 đã khóa.

Final Conclusion đã khóa.

## 23. FINAL FAIL GATE - TECH-12

TECH-12 FAIL nếu còn bất kỳ điểm nào sau:

Backlog không có source-of-truth.

Backlog không có TECH/section/requirement.

Backlog mơ hồ vẫn Ready.

Backlog thiếu Scope Out.

Backlog thiếu dependency.

Backlog thiếu owner.

Backlog thiếu evidence.

Backlog thiếu smoke.

Backlog thiếu report requirement.

Backlog downstream Ready khi upstream bị chặn.

Source conflict vẫn handoff.

Tài liệu cũ thắng TECH mới.

Code cũ thắng TECH mới.

Task breakdown làm mất source mapping.

Task breakdown bỏ P0 điểm chặn.

Task breakdown biến thành code snippet.

Evidence requirement bỏ high-risk audit.

Smoke requirement chỉ happy path cho backlog critical.

Change control sửa backlog âm thầm.

Backlog bị chặn vẫn handoff TECH-11.

TECH-12 gọi Backlog Ready là Dev Ready.

TECH-12 gọi Backlog Ready là Release Ready.

TECH-12 gửi backlog chưa implemented sang TECH-10 như release-ready package.

TECH-12 bỏ qua TECH-11.

TECH-12 bỏ qua TECH-10.

TECH-12 mở Global Gateway.

TECH-12 bị hiểu là tài liệu code.

TECH-12 cho phép copy-paste code rời rạc.

TECH-12 cho phép hardcode policy.

Không có final phase backlog matrix.

Không có evidence-smoke matrix.

## 24. RELEASE HANDOFF - SAU TECH-12

## 24.1. Trạng thái sau khi hoàn tất TECH-12

Sau khi hoàn tất PHẦN 4/4:

Nhưng:

Backlog Ready = DEFINED, NOT EXECUTED

Global Gate bị chặn

Lý do:

TECH-12 mới là backlog pack tài liệu.

Chưa có handoff thực tế qua TECH-11.

Chưa có code implementation.

Chưa có dev report.

Chưa có evidence thực thi.

Chưa có smoke thực thi.

Chưa có QA/UAT.

Chưa có owner sign-off.

Chưa có release decision.

## 24.2. Điều kiện để nâng Backlog Ready thành Dev Ready

Muốn Dev Ready phải qua TECH-11:

Backlog handoff sang TECH-11.

Dev Handoff Resolver pass.

Code Handoff Control pass.

Architecture context rõ.

DB/API/UI impact review nếu có.

Test/evidence expectation rõ.

No copy-paste-code rule accepted.

No hardcode-policy rule accepted.

Dev owner accepted.

Nếu chưa qua TECH-11:

## 24.3. Điều kiện để nâng Dev Ready thành Implementation Complete

Muốn Implementation Complete phải có:

Dev implementation thật.

File đã sửa.

Source requirement trace.

Evidence đã dùng.

Lệnh đã chạy.

Test result.

Backend build result.

Frontend build result nếu có.

Cleanup result.

Docs update nếu có.

Migration/seed result nếu có.

Risk report.

Handoff update.

Nếu thiếu report:

## 24.4. Điều kiện để nâng Implementation Complete thành Ready for TECH-10 Review

Muốn Ready for TECH-10 Review phải có:

Implementation report accepted.

Evidence mapped.

Smoke mapped.

QA handoff completed nếu required.

P0 điểm chặn closed.

Cleanup/docs update pass.

Dependency pass.

Release handoff package complete.

Nếu thiếu bất kỳ điều kiện nào:

## 24.5. Điều kiện để nâng lên Release Ready

Release Ready chỉ do TECH-10 xét.

TECH-12 không có quyền xét Release Ready.

TECH-10 chỉ xét Release Ready khi có:

Evidence accepted.

Global smoke pass.

UAT pass nếu required.

Owner sign-off.

No open P0 điểm chặn.

Cross-tech dependency pass.

Privacy/security pass.

Rollback/fallback ready nếu production.

Monitoring/alert ready nếu go-live.

Release decision phù hợp.

Nếu chưa qua TECH-10:

## 25. HANDOFF SANG BƯỚC TIẾP THEO

Sau TECH-12, bước tiếp theo hợp lý là viết bộ prompt/handoff thực thi cho dev theo từng phase.

Tên tài liệu đề xuất:

Mục tiêu TECH-13:

Viết prompt giao dev/coding agent theo PHASE 0 -> PHASE 9.

Mỗi prompt chỉ giao một phase hoặc một nhóm backlog rõ.

Prompt không yêu cầu copy-paste code rời rạc.

Prompt yêu cầu dev đọc source-of-truth.

Prompt yêu cầu dev kiểm tra codebase thật.

Prompt yêu cầu dev đề xuất implementation plan trước.

Prompt yêu cầu dev chạy test/build.

Prompt yêu cầu dev nộp evidence.

Prompt yêu cầu report đúng 14 mục.

Prompt không cho dev gọi done là release ready.

Hạng mục

Trạng thái sau TECH-12

TECH-12 Documentation

Backlog Pack

## DEFINED

Backlog Ready

## DEFINED, NOT EXECUTED

Dev Ready

KHONG

Implementation Ready

KHONG

Implementation Complete

KHONG

Ready for TECH-10 Review

KHONG

Release Ready

KHONG

Production Ready

KHONG

Go-live Approved

KHONG

Scale Approved

KHONG

Global Gateway

bị chặn

TECH-11 Dev Handoff

## REQUIRED

Code Handoff Control

## REQUIRED

Dev Report

## REQUIRED AFTER IMPLEMENTATION

Evidence Mapping

## REQUIRED AFTER IMPLEMENTATION

Smoke Mapping

## REQUIRED AFTER IMPLEMENTATION

## QA/UAT

## REQUIRED BY SCOPE

TECH-10 Release Review

## REQUIRED BEFORE RELEASE

## TECH-12 FINAL CONCLUSION

TECH-12 đã khóa lớp Phase Backlog Pack / Dev Task Breakdown / Source-to-Backlog Matrix cho hệ thống Ginsengfood.

TECH-12 không thay thế TECH-00 -> TECH-11.

TECH-12 không viết code.

TECH-12 không thiết kế API/DB/UI chi tiết.

TECH-12 không cho phép backlog mơ hồ.

TECH-12 không cho phép task không source.

TECH-12 không cho phép copy-paste code rời rạc từ AI.

Các nguyên tắc cuối cùng đã khóa:

Mọi backlog phải map source-of-truth.

TECH mới thắng tài liệu cũ.

TECH mới thắng code cũ.

Tài liệu cũ chỉ là LEGACY_REFERENCE_ONLY.

Code cũ chỉ là CURRENT_IMPLEMENTATION_STATE_ONLY.

Backlog không source không Ready.

Backlog thiếu Scope Out không Ready.

Backlog thiếu dependency không Ready.

Backlog thiếu evidence không Ready.

Backlog thiếu smoke không Ready.

Backlog thiếu owner không Ready.

Backlog thiếu report requirement không Ready.

Downstream backlog bị bị chặn nếu upstream chưa pass.

PHASE 0 là nền cho toàn bộ high-risk phase.

PHASE 1 là nền cho Product/SKU/Recipe.

PHASE 2 là nền cho Operational/Sellable/Sale Lock/Recall.

PHASE 3 là nền cho Commerce/Quote/Order/Payment/Verified Revenue.

PHASE 4 là nền cho AI Advisor.

PHASE 5 là nền cho Gateway/Messenger.

PHASE 6 phụ thuộc Verified Revenue để đo Ads.

PHASE 7 phụ thuộc Gateway/AI/Commerce để chạy MC AI Live.

PHASE 8 phụ thuộc Official Order/Core Order/Notification để chạy IVR.

PHASE 9 phụ thuộc TECH-10 để release/go-live/scale.

Task breakdown không được làm mất source/P0/evidence/smoke.

Task breakdown không được biến thành code snippet.

Evidence matrix phải có theo phase.

Smoke matrix phải có theo phase.

Owner matrix phải rõ theo domain.

Dependency matrix phải chặn downstream khi upstream bị chặn.

Backlog handoff chỉ sang TECH-11.

TECH-12 không gọi Backlog Ready là Dev Ready.

TECH-12 không gọi Backlog Ready là Release Ready.

TECH-12 không gửi backlog chưa implemented sang TECH-10 như release package.

TECH-12 không mở Global Gateway.

TECH-10 mới xét evidence accepted, global smoke pass, UAT, owner sign-off, release decision và Global Gateway.

Trạng thái cuối cùng của tài liệu:

Nhưng trạng thái triển khai vẫn là:

Bước tiếp theo hợp lý sau TECH-12 là viết:

để biến backlog PHASE 0 -> PHASE 9 thành các prompt giao dev/coding agent theo từng phase, đúng source-of-truth, đúng boundary, đúng evidence, đúng smoke và đúng report format.

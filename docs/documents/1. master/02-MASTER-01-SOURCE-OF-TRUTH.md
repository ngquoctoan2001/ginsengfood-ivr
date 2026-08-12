# MASTER-01 - SOURCE OF TRUTH

## Mục Đích

MASTER-01 khóa nguồn sự thật cho toàn hệ Ginsengfood. Mục tiêu là ngăn mỗi module tự hiểu dữ liệu theo cách riêng, tự hardcode rule, tự quyết trạng thái hoặc tự công bố dữ liệu khách hàng nhìn thấy.

MASTER-01 không thiết kế database hoặc API chi tiết. File này chỉ xác định dữ liệu nào thuộc owner nào, consumer nào được đọc, consumer nào không được quyết định và khi nào phải gọi resolver.

## Nguyên Tắc Source-Of-Truth

1. Một nhóm dữ liệu phải có một owner quyết định.
2. Consumer được đọc hoặc hiển thị dữ liệu chỉ khi đi qua resolver hoặc contract hợp lệ.
3. Dữ liệu customer-facing phải là public-safe.
4. Dữ liệu internal không được tự public hóa.
5. Khi nhiều nguồn cùng xuất hiện, owner core thắng consumer.
6. Khi thiếu source, hệ thống phải dừng hành động liên quan thay vì suy luận.

## Phân Loại Nguồn Sự Thật

| Loại | Ý nghĩa | Ví dụ | Cách dùng |
| --- | --- | --- | --- |
| Governance Source | Nguồn khóa rule và boundary | MASTER, PACK | Dùng để viết tài liệu và gate |
| Master Data Source | Danh mục chuẩn | Product, SKU, ingredient, warehouse | Dùng làm dữ liệu nền |
| Runtime Decision Source | Nguồn quyết định tại thời điểm chạy | Sellable, pricing, payment, shipping | Dùng qua resolver |
| Transaction State Source | Nguồn trạng thái giao dịch | Order, payment confirmation, receipt | Dùng qua state machine |
| Operational State Source | Nguồn trạng thái vận hành | Raw lot, production order, batch, inventory | Owner là Operational Core |
| Evidence Source | Nguồn bằng chứng | Smoke result, audit log, evidence packet | Dùng cho review |
| Public-Safe Source | Nguồn được phép hiển thị cho khách | Public product name, public trace whitelist | Dùng cho AI, Gateway, Live |

## Registry Theo Domain

| Domain | Source-of-Truth | Consumer chính | Rule khóa |
| --- | --- | --- | --- |
| Product / SKU / Recipe | PACK-02 | Operational, AI, Ads, Live, Gateway | Product Active không đồng nghĩa Sellable |
| Ingredient / UOM | PACK-02 | Production, MRP, MISA | Không dùng ingredient mơ hồ trong BOM |
| Formula Version | PACK-02 | Production Order, MRP | Đổi tỷ lệ hoặc nguyên liệu phải tạo version |
| BOM Snapshot | PACK-02 + PACK-01 | Production, Material Issue | Production phải dùng snapshot |
| Raw Lot / QC / Readiness | PACK-01 | Production, MISA, Trace | QC_PASS không đồng nghĩa READY_FOR_PRODUCTION |
| Production Order / Snapshot | PACK-01 | Workshop, Material Issue, Batch | Production Order phải snapshot formula/version/BOM |
| Material Issue / Receipt | PACK-01 | Inventory, MISA | Issue giảm tồn, receipt không giảm lần hai |
| Batch / Release | PACK-01 | Warehouse, Trace, Commerce Runtime | Batch QC_PASS không đồng nghĩa RELEASED |
| Warehouse / Inventory | PACK-01 | Commerce Runtime, AI, Ads, CRM | Finished goods chỉ tăng tồn sau receipt confirmed |
| Traceability / QR | PACK-01 | Public trace, Gateway, IVR | Public trace whitelist-only |
| Recall / Sale Lock | PACK-01 | Mọi downstream | Recall / Sale Lock thắng bán hàng |
| Demand / MRP | PACK-03 | Procurement, planning | Không phát hành production demand khi thiếu BOM/stock |
| MISA Handoff | PACK-04 | Accounting, reconciliation | MISA nhận checkpoint, không điều khiển vận hành |
| AI Advisor | PACK-05 | Channel, CRM, Live | AI là consumer, không phải owner giá/tồn/payment |
| Facebook Gateway | PACK-06 | Messenger, Live, private handoff | Gateway không quyết định nghiệp vụ owner core |
| Ads / Attribution | PACK-07 | Dashboard, scale decision | Ads chỉ tính theo verified revenue |
| MC AI Live | PACK-08 | Live board, script brief | Live chỉ dùng board/script đã được kiểm soát |
| IVR Confirmation | PACK-09 | Order confirmation flow | IVR chỉ tạo signal xác nhận đơn |
| Completion / Evidence | PACK-10 | Release governance | Không gọi đạt nếu thiếu evidence accepted |

## Owner Và Consumer

| Vai trò | Được phép | Không được phép |
| --- | --- | --- |
| Owner Core | Quyết định dữ liệu và trạng thái trong domain | Vượt owner khác |
| Resolver | Trả lời trạng thái hợp lệ tại runtime | Tự tạo nguồn mới |
| Guard | Chặn hành động sai điều kiện | Bỏ qua evidence |
| Consumer | Đọc, hiển thị, sử dụng kết quả đã resolve | Tự hardcode hoặc tự quyết |
| Reviewer | Xác nhận evidence trong scope | Thay đổi source data |

## Public-Safe Boundary

| Dữ liệu | Public default | Điều kiện được public |
| --- | --- | --- |
| Public product name | Có thể public | Đã duyệt public name |
| Public claim | Có thể public | Qua claim guard |
| Batch public code | Có thể public | Có public mapping |
| MFG/HSD | Có thể public | Đúng template |
| Source public text | Có thể public | Owner duyệt wording |
| Internal SKU code | Không public | Chỉ khi có mapping public |
| Internal lot code | Không public | Không dùng trực tiếp |
| QC internal note | Không public | Chỉ dùng nội bộ |
| Cost / MISA / margin | Không public | Không áp dụng customer-facing |
| Staff / audit actor | Không public | Chỉ audit nội bộ |

## Conflict Resolution

Khi dữ liệu mâu thuẫn:

1. Recall / Sale Lock thắng mọi consumer.
2. Operational release và inventory thắng AI, Ads, Live, CRM, Gateway, IVR.
3. Payment confirmation thuộc payment owner; AI và Gateway không được nói paid nếu chưa có confirmation.
4. Shipping eligibility thuộc shipping owner; AI và Gateway không được hứa ETA nếu chưa có eligibility.
5. MISA không được sửa operational truth.
6. Evidence accepted mới được dùng để xét completion hoặc release.

## Resolver Requirement

| Câu hỏi runtime | Resolver bắt buộc |
| --- | --- |
| Product có được dùng không | Product Resolver |
| SKU có public được không | SKU Public Resolver |
| Recipe nào được sản xuất | Formula Snapshot Resolver |
| Raw lot có được issue không | Raw Lot Readiness Resolver |
| Batch có được nhập kho không | Batch Release Resolver |
| Có tồn khả dụng không | Inventory Availability Resolver |
| Có bị recall hoặc sale lock không | Recall / Sale Lock Resolver |
| Dữ liệu có public-safe không | Public-Safe Resolver |

## Missing Input Gate

Nếu thiếu owner, source, resolver, guard hoặc evidence requirement, file chi tiết không được tự điền. Nội dung thiếu phải được ghi vào danh sách câu hỏi owner riêng.

## Done Gate Của MASTER-01

MASTER-01 đạt yêu cầu khi:

1. Mỗi domain trọng yếu có Source-of-Truth rõ.
2. Consumer downstream không được tự quyết dữ liệu owner core.
3. Public-safe boundary rõ.
4. Conflict resolution rõ.
5. Resolver requirement rõ.
6. PACK có thể dùng registry này để viết resolver, guard và evidence.

## Chi Tiết Governance Tương Ứng

Phần này giữ lại phạm vi chi tiết từ nguồn MASTER theo cụm cùng chủ đề, đồng thời chuẩn hóa theo registry PACK hiện hành và loại bỏ các câu trạng thái tổng không dùng trong bộ sạch.


## Master-01 — Global Source-Of-Truth Registry


## Data Authority / Runtime Resolver / Source Owner / Consumer Boundary / Evidence Control


## PHẦN 1/4 — GLOBAL SOURCE-OF-TRUTH PRINCIPLES / SOURCE CLASSIFICATION / REGISTRY MODEL / AUTHORITY BOUNDARY


## 1. MỤC ĐÍCH CỦA MASTER-01

MASTER-01 là tài liệu đăng ký Source-of-Truth toàn hệ của Ginsengfood.

MASTER-01 dùng để xác định rõ:

Dữ liệu nào thuộc core nào.

Pack nào là chủ sở hữu dữ liệu gốc.

Pack nào chỉ được consume dữ liệu.

Pack nào được quyền ra quyết định runtime.

Pack nào không được tự suy diễn dữ liệu.

Dữ liệu nào bắt buộc đi qua resolver/API/contract.

Dữ liệu nào bắt buộc có evidence.

Dữ liệu nào không được hardcode.

Khi dữ liệu thiếu, hệ thống phải xử lý thế nào.

Khi hai nguồn dữ liệu mâu thuẫn, nguồn nào được ưu tiên.

MASTER-01 không phải tài liệu database chi tiết.

MASTER-01 không thiết kế bảng dữ liệu cụ thể.

MASTER-01 không định nghĩa API chi tiết.

MASTER-01 không viết logic triển khai.

MASTER-01 chỉ xác định quyền sở hữu dữ liệu, quyền sử dụng dữ liệu, quyền ra quyết định và nguyên tắc kiểm chứng dữ liệu.

Nguyên tắc:

MASTER-01 = Source-of-Truth Registry + Data Authority + Runtime Source Boundary + Resolver Requirement + Evidence RequirementPack chi tiết = API/DTO/DB/Worker/UI/Test implementation theo Source-of-Truth đã khóa


## 2. VỊ TRÍ CỦA MASTER-01 TRONG HỆ MASTER

MASTER-01 đứng sau MASTER-00 và trước các MASTER còn lại.

Thứ tự chuẩn:

MASTER-00 — GINSENGFOOD MASTER TECHNICAL DOCUMENTATION INDEXMASTER-01 — GLOBAL SOURCE-OF-TRUTH REGISTRYMASTER-02 — CROSS-PACK DEPENDENCY MAPMASTER-03 — TRACEABILITY ID STANDARDMASTER-04 — RELEASE GOVERNANCE / DONE GATE STANDARDMASTER-05 — EVIDENCE PACKAGE STANDARDMASTER-06 — SMOKE MATRIX STANDARDMASTER-07 — QUẢN TRỊ PACK DỰ PHÒNG / RANH GIỚI TÍCH HỢP TƯƠNG LAI

MASTER-01 là nền để MASTER-02 xác định dependency.

Nếu chưa biết dữ liệu thuộc về core nào, không thể vẽ dependency đúng.

Nếu chưa biết Source-of-Truth, không thể xác định pack nào được quyền trả lời, pack nào chỉ được gọi API, pack nào không được tự suy diễn.


## 3. NGUYÊN TẮC CỐT LÕI CỦA SOURCE-OF-TRUTH

Source-of-Truth là nguồn dữ liệu có thẩm quyền cuối cùng cho một nhóm dữ liệu hoặc một quyết định runtime.

Một dữ liệu chỉ được xem là đáng tin cậy khi:

Có owner rõ ràng.Có core sở hữu rõ ràng.Có contract truy xuất rõ ràng.Có quyền sử dụng rõ ràng.Có evidence kiểm chứng khi cần.Không bị hardcode ở consumer.Không bị sao chép rời rạc sang template/UI/AI response.

Nguyên tắc toàn hệ:

Không có Source-of-Truth hợp lệ -> không được ra quyết định runtime.Không có resolver hợp lệ -> không được trả lời như dữ liệu đã chắc chắn.Không có eligibility hợp lệ -> không được hứa khả năng áp dụng.Không có evidence hợp lệ -> không được tính Done.Không có smoke pass -> không được release.Không có owner sign-off -> không được go-live.


## 4. SOURCE-OF-TRUTH KHÔNG PHẢI LÀ GÌ

Source-of-Truth không phải là:

Một đoạn text trong template.

Một câu trả lời của AI.

Một biến hardcode trong frontend.

Một cấu hình tạm trong Landing Page.

Một file Excel rời rạc không có owner.

Một ảnh chụp giao dịch do khách gửi.

Một dòng comment trong live.

Một nội dung CRM tự viết.

Một đoạn code tự tính giá.

Một đoạn code tự hứa phí ship.

Một script tự cập nhật trạng thái đơn.

Một log chưa được đối chiếu với core.

Một dữ liệu local do dev tự tạo để test nhưng chưa được seed/governance.

Các dữ liệu trên có thể là input, evidence hoặc artifact, nhưng không được xem là Source-of-Truth nếu không được core/registry/owner phê duyệt.


## 5. PHÂN LOẠI SOURCE-OF-TRUTH

Source-of-Truth trong Ginsengfood được phân thành 7 nhóm chính.

TYPE 01 — Governance SourceTYPE 02 — Master Data SourceTYPE 03 — Runtime Decision SourceTYPE 04 — Transaction State SourceTYPE 05 — Operational State SourceTYPE 06 — Evidence SourceTYPE 07 — Public-Safe Source


## 6. TYPE 01 — GOVERNANCE SOURCE

Governance Source là nguồn quy định ranh giới, trạng thái, gate, pack, dependency, evidence và release.

Bao gồm:


## Master-00.


## Master-01.


## Master-02.


## Master-03.


## Master-04.


## Master-05.


## Master-06.


## Master-07.

Owner decision record nếu có.

Release sign-off record.

Future governed extension decision nếu có.

Governance Source không thay thế runtime core.

Governance Source trả lời câu hỏi:

Cái gì được làm?Cái gì không được làm?Pack nào sở hữu dữ liệu?Pack nào không được bypass core?Điều kiện nào mới được Done?Điều kiện nào mới được release?Điều kiện nào mới được go-live?


## 7. TYPE 02 — MASTER DATA SOURCE

Master Data Source là nguồn dữ liệu nền ít thay đổi hoặc có quy trình thay đổi có kiểm soát.

Product Master.

SKU Master.

Ingredient Master.

Recipe / Formula Master.

Supplier Master.

Source Origin Master.

Customer Profile Master.

Member Tier Master.

Pricing Policy Master.

Payment Method Master.

Company Bank Account Registry.

Shipping Method Master.

Role / Permission Master.

Channel Page Registry.

Public Trace Field Whitelist.

Claim Whitelist.

Claim Denylist.

Master Data Source phải có:

ownerversion nếu cầneffective_from nếu cầneffective_to nếu cầnstatusapproval_stateauditevidence nếu có thay đổi trọng yếu

Không được sửa master data trọng yếu mà không có audit.


## 8. TYPE 03 — RUNTIME DECISION SOURCE

Runtime Decision Source là nguồn trả lời quyết định tại thời điểm xử lý.

Pricing Resolver.

Program Resolver.

Member Benefit Resolver.

Customer Price Identity Resolver.

Availability Resolver.

Payment Eligibility Resolver.

Shipping Eligibility Resolver.

COD Eligibility Resolver.

Quote Snapshot.

Order Confirmation Draft.

CRM Suppression Resolver.

Frequency Cap Resolver.

Claim Guard.

Public Leak Guard.

IVR Call Eligibility Resolver.

Runtime Decision Source trả lời câu hỏi:

Tại thời điểm này có được báo giá không?Khách này là New hay Member?Khách này thuộc hạng nào?SKU này có sellable không?Phương thức thanh toán nào được phép?Có được chuyển khoản không?Có được COD không?Có giao được khu vực này không?Có được hứa ETA không?Có được gửi tin CRM không?Có được nói claim này không?Có được gọi IVR cho đơn này không?

Consumer không được tự trả lời các câu hỏi trên nếu chưa gọi resolver/core tương ứng.


## 9. TYPE 04 — TRANSACTION STATE SOURCE

Transaction State Source là nguồn trạng thái giao dịch.

Quote state.

Cart state.

Order state.

Payment state.

Accounting Review state.

Shipping state.

COD state nếu có.

Invoice/VAT state nếu áp dụng.

IVR confirmation result state.

Cancellation state.

Fulfillment state.

Transaction State Source phải thuộc về Commerce Core hoặc core giao dịch tương ứng.

AI, Gateway, CRM, Landing Page, Admin UI và IVR không được tự cập nhật transaction state nếu không qua command/state machine hợp lệ.


## 10. TYPE 05 — OPERATIONAL STATE SOURCE

Operational State Source là nguồn trạng thái vận hành sản xuất, kho, truy xuất, thu hồi.

Raw material intake state.

Incoming QC state.

Raw lot readiness state.

Material issue state.

Production order state.

Formula snapshot state.

Batch genealogy state.

Batch QC state.

Batch release state.

Warehouse receipt state.

Inventory ledger state.

Sellable availability state.

Public trace state.

Recall state.

CAPA state.

MISA sync state nếu áp dụng.

Operational State Source thuộc về Operational Core.

Commerce chỉ được bán khi nhận được sellable availability hợp lệ từ Operational / Inventory / Availability Core.


## 11. TYPE 06 — EVIDENCE SOURCE

Evidence Source là nguồn bằng chứng kiểm chứng.

API response evidence.

Database snapshot evidence.

Audit log.

State transition log.

Worker log.

Queue log.

Webhook log.

Payment confirmation evidence.

Accounting Review evidence.

Shipping eligibility evidence.

IVR call log.

Gateway transcript.

AI response transcript.

CRM message log.

Public trace response evidence.

Screenshot evidence.

Smoke report.

UAT report.

Monitoring screenshot.

Rollback evidence.

Owner sign-off evidence.

Evidence Source không thay thế Source-of-Truth.

Ví dụ:

Ảnh chuyển khoản của khách = evidence/input để kế toán review.Ảnh chuyển khoản không phải Payment Source-of-Truth.Payment Source-of-Truth chỉ được xác nhận bởi Payment Core hoặc Accounting Review confirmation.


## 12. TYPE 07 — PUBLIC-SAFE SOURCE

Public-Safe Source là nguồn dữ liệu được phép đưa ra bề mặt công khai.

Product Public View.

Public Claim Whitelist.

Public Trace DTO.

Public-safe price quote nếu đủ điều kiện và đúng channel.

Public-safe shipping message nếu Shipping Core trả eligibility.

Public-safe payment message nếu Payment Core trả eligibility.

Public-safe CRM message.

Public-safe Landing Page content.

Public-safe ADS content.

Public-Safe Source phải loại bỏ:

Dữ liệu nội bộ.

Dữ liệu kế toán.

Dữ liệu supplier nhạy cảm.

Dữ liệu personnel.

Dữ liệu lỗi QC chi tiết không được public.

Dữ liệu công thức nội bộ.

Dữ liệu chi phí.

Dữ liệu tồn kho chi tiết không được công bố.

Dữ liệu ngân hàng nếu không được lấy qua Payment Core/Registry và đúng ngữ cảnh thanh toán.

Claim điều trị/chữa bệnh/thay thuốc.


## 13. GLOBAL SOURCE REGISTRY MODEL

Mỗi Source-of-Truth trong toàn hệ phải được đăng ký theo mẫu sau:

source_idsource_namesource_typeowning_packowning_coredata_domaindecision_authorityallowed_consumersforbidden_consumersaccess_methodruntime_requiredevidence_requiredhardcode_allowedpublic_allowedaudit_requiredapproval_requiredfallback_ruleblocked_if_missingrelated_rulesrelated_smokerelated_evidencedone_gate

Ý nghĩa:

source_id: mã định danh nguồn.

source_name: tên nguồn.

source_type: loại Source-of-Truth.

owning_pack: pack sở hữu.

owning_core: core sở hữu.

data_domain: nhóm dữ liệu.

decision_authority: quyền quyết định.

allowed_consumers: hợp phần được consume.

forbidden_consumers: hợp phần không được consume hoặc không được bypass.

access_method: cách truy xuất.

runtime_required: có bắt buộc runtime không.

evidence_required: có cần evidence không.

hardcode_allowed: có được hardcode không.

public_allowed: có được public không.

audit_required: có cần audit không.

approval_required: có cần approval không.

fallback_rule: nếu thiếu dữ liệu xử lý thế nào.

blocked_if_missing: thiếu thì có block không.

related_rules: rule liên quan.

related_smoke: smoke liên quan.

related_evidence: evidence liên quan.

done_gate: gate liên quan.


## 14. SOURCE ID STANDARD

Source ID dùng prefix:

SRC-<DOMAIN>-<NUMBER>

SRC-GOV-001SRC-PROD-001SRC-CLAIM-001SRC-PRICE-001SRC-MEMBER-001SRC-AVAIL-001SRC-PAY-001SRC-BANK-001SRC-SHIP-001SRC-ORDER-001SRC-IVR-001SRC-CRM-001SRC-OPS-001SRC-TRACE-001SRC-EVD-001SRC-SEC-001

Source ID phải map được sang Traceability ID của pack chi tiết.

SRC-BANK-001 -> COM-SRC-BANK-001 -> COM-RULE-BANK-001 -> COM-API-PAYMENT-ELIGIBILITY -> COM-EVID-BANK-001 -> COM-GATE-PAYMENT


## 15. DOMAIN PREFIX CHUẨN CHO SOURCE ID

Domain

Ý nghĩa

GOV

Governance

PROD

Product / SKU

CLAIM

Claim / Public wording

PRICE

Pricing / Program

MEMBER

Member / Benefit

CUSTOMER

Customer Profile / Memory

AVAIL

Availability / Sellable

PAY

Payment

BANK

Company Bank Account Registry

SHIP

Shipping / COD / ETA

ORDER

Order / Cart / Quote

IVR

IVR Order Confirmation

CRM

CRM / Lifecycle

OPS

Operational Core


## Trace

Traceability / Public Trace

RECALL

Recall / Recovery

MISA

Accounting Integration Boundary

ADS

Ads / Tracking Signal

UI

Admin UI / Operator Workspace

EVD

Evidence / Smoke / UAT

SEC

Security / Permission / Audit


## 16. SOURCE AUTHORITY LEVEL

Mỗi nguồn dữ liệu phải được phân cấp authority.

AUTHORITY_LEVEL_1 — FINAL_SOURCE_OF_TRUTHAUTHORITY_LEVEL_2 — RUNTIME_DECISION_SOURCEAUTHORITY_LEVEL_3 — APPROVED_CONSUMER_VIEWAUTHORITY_LEVEL_4 — EVIDENCE_ONLYAUTHORITY_LEVEL_5 — DISPLAY_ONLYAUTHORITY_LEVEL_6 — TEST_ONLY


### 16.1. FINAL_SOURCE_OF_TRUTH

Là nguồn quyết định cuối.

Commerce Payment Core cho payment state.

Commerce Shipping Core cho shipping eligibility.

Product Knowledge Core cho product public data.

Operational Core cho batch release.

Order Core cho order state.

Company Bank Account Registry cho tài khoản ngân hàng công ty.


### 16.2. RUNTIME_DECISION_SOURCE

Là nguồn trả quyết định theo thời điểm.

IVR Eligibility Resolver.


### 16.3. APPROVED_CONSUMER_VIEW

Là view được phép dùng bởi consumer.

Quote Snapshot View.

Admin Review View.

CRM Customer Summary View.


### 16.4. EVIDENCE_ONLY

Là bằng chứng, không phải nguồn quyết định.

Screenshot.

Transcript.

Log.

Ảnh chuyển khoản.

Webhook payload.


### 16.5. DISPLAY_ONLY

Là dữ liệu chỉ để hiển thị, không được dùng làm quyết định.

Label UI.

Badge UI.

Static text public-safe đã được duyệt.

Dashboard summary.


### 16.6. TEST_ONLY

Là dữ liệu dùng cho test, không được dùng production.

Seed test.

Mock provider.

Sandbox response.

Local script sample.


## 17. SOURCE-OF-TRUTH VÀ HARD LOCK

Các nhóm dữ liệu sau là hard lock, không được hardcode ở consumer:

Giá cuối cùng.Quyền lợi thành viên.Trạng thái sellable.Tồn kho có thể bán.Phương thức thanh toán khả dụng.Tài khoản ngân hàng SSAVIGROUP.Payment reference / transfer memo.Trạng thái PAID.Shipping eligibility.COD eligibility.Phí ship.Fallback ETA.Order state.IVR result state.Claim công khai.Public Trace DTO.Permission / role.Audit state.

Consumer gồm:

AI Advisor.

Channel Gateway.

CRM.

Landing Page.

Admin UI.

Frontend component.

Static template.

Worker.

IVR Gateway.

ADS content.

Internal script.


## 18. SOURCE-OF-TRUTH VÀ RUNTIME RESOLVER

Một số dữ liệu không được đọc trực tiếp từ master data rồi tự quyết định.

Bắt buộc phải đi qua resolver/runtime core.

listed_price không đủ để quote final price.Cần Pricing Resolver + Program Resolver + Member Benefit Resolver + Availability Resolver.payment_method_config không đủ để hứa khách thanh toán được.Cần Payment Eligibility Resolver.shipping_method_config không đủ để hứa giao hàng được.Cần Shipping Eligibility Resolver.product_catalog có SKU không đủ để nói còn hàng.Cần Availability Resolver.customer_profile có hạng thành viên không đủ để tự áp quyền lợi.Cần Member Benefit Resolver.claim whitelist có nội dung được nói không đủ để trả lời tự do.Cần Claim Guard / Public Wording Policy theo ngữ cảnh.


## 19. SOURCE-OF-TRUTH VÀ EVIDENCE

Một Source-of-Truth có thể cần evidence khi:

Dữ liệu ảnh hưởng thanh toán.

Dữ liệu ảnh hưởng trạng thái đơn.

Dữ liệu ảnh hưởng quyền lợi khách.

Dữ liệu ảnh hưởng sản phẩm được bán.

Dữ liệu ảnh hưởng truy xuất / thu hồi.

Dữ liệu ảnh hưởng public claim.

Dữ liệu ảnh hưởng go-live.

Dữ liệu ảnh hưởng kế toán.

Dữ liệu ảnh hưởng shipping/COD/ETA.

Dữ liệu ảnh hưởng IVR confirmation.

Dữ liệu ảnh hưởng permission/audit.

Nếu thiếu evidence ở nhóm trọng yếu, trạng thái phải giữ:


## 20. SOURCE-OF-TRUTH VÀ FALLBACK

Không phải thiếu dữ liệu là được tự đoán.

Fallback phải theo nguyên tắc:

Thiếu dữ liệu giá -> không quote.Thiếu dữ liệu member -> không áp quyền lợi member.Thiếu dữ liệu sellable -> không bán.Thiếu Payment Eligibility -> không hứa phương thức thanh toán.Thiếu Company Bank Account Registry -> không hiển thị tài khoản ngân hàng.Thiếu payment_reference -> không tạo bank transfer order hợp lệ.Thiếu Payment Core/Accounting Review confirmation -> không PAID.Thiếu Shipping Eligibility -> không hứa giao hàng/ETA/COD/phí ship.Thiếu Order Core confirmation task -> IVR không gọi.Thiếu Claim Whitelist -> không public claim.Thiếu Public Trace DTO -> không public trace.Thiếu permission -> không cho thao tác.Thiếu evidence P0 -> không release.


## 21. GLOBAL SOURCE-OF-TRUTH OWNERSHIP MATRIX

Data Domain

Final Source-of-Truth

Runtime Source

Consumer chính

Không được phép

MASTER Pack

Release/Gate Registry

Tất cả pack

Pack con tự đổi gate

Product/SKU

Product Knowledge Core

Product Knowledge Resolver

AI, Gateway, CRM, Landing Page, ADS, Admin UI

Tự đặt tên/tự viết SKU

Claim

Claim Policy Core

Claim Guard

AI, Gateway, CRM, Landing Page, ADS

Nói claim ngoài whitelist

Pricing

Commerce Pricing Core

Pricing Resolver

AI, Gateway, CRM, Landing Page, Admin UI

Tự tính giá

Program

Commerce Program Core

Program Resolver

AI, Commerce, Gateway

Tự áp chương trình

Member Benefit

Member Core

Member Benefit Resolver

AI, CRM, Commerce

Tự hứa quyền lợi

Availability

Inventory / Availability Core

Availability Resolver

AI, Commerce, Gateway, Landing Page

Bán khi chưa sellable

Commerce Payment Core

Payment Eligibility Resolver

AI, Gateway, CRM, Admin UI, Landing Page

Tự hứa payment

Bank Account

Payment Instruction Resolver

Payment UI, Admin Review, Customer Payment Flow

Hardcode tài khoản

Shipping

Commerce Shipping Core

Shipping Eligibility Resolver

Tự hứa ETA/COD/phí ship

Order

Commerce Order Core

Order State Machine

AI, Gateway, IVR, Admin UI, CRM

Tự cập nhật order state

IVR Pack + Order Core

IVR Eligibility / Result Event

Order Core, Admin UI

IVR tự đổi trạng thái đơn

CRM Lifecycle Core

Suppression/Frequency Resolver

CRM, AI, Gateway

Gửi vượt rule

Operational

Operational State Resolver

Admin UI, Commerce Availability, Trace

Bypass state

Trace

Trace Core

Public Trace DTO

Public Trace, Admin UI

Public leak nội bộ

Evidence

Evidence Pack

Evidence Registry

Release, QA, Owner sign-off

Tự pass khi thiếu evidence

Security

Security Core

Permission/Audit Resolver

Bypass permission/audit


## 22. QUY TẮC XỬ LÝ KHI CÓ XUNG ĐỘT SOURCE

Nếu hai nguồn dữ liệu mâu thuẫn, xử lý theo thứ tự ưu tiên:


## 1. Governance Source nếu là vấn đề boundary/gate/release2. Final Source-of-Truth của domain3. Runtime Decision Source4. Approved Consumer View5. Evidence Source6. Display-only data7. Test-only data

Nếu AI template có số tài khoản nhưng Company Bank Account Registry không trả tài khoản đó -> Registry đúng, AI template sai.

Nếu Landing Page có ETA nhưng Shipping Eligibility không trả ETA -> Shipping Core đúng, Landing Page sai.

Nếu CRM nói khách được quyền lợi member nhưng Member Benefit Resolver không trả quyền lợi -> Member Resolver đúng, CRM sai.

Nếu Gateway muốn public final quote nhưng Channel/Commerce policy không cho -> Policy/Core đúng, Gateway sai.

Nếu IVR trả kết quả nhưng Order Core chưa xử lý state transition -> Order Core mới là quyết định cuối.

Nếu batch QC_PASS nhưng chưa RELEASED -> Operational Core state đúng, Commerce không được bán.


## 23. QUY TẮC SOURCE-OF-TRUTH THEO CHANNEL

Channel không được tự quyết định dữ liệu.


### 23.1. AI Advisor

AI Advisor chỉ được trả lời chắc chắn khi có dữ liệu từ Source-of-Truth hoặc resolver hợp lệ.

AI không được tự sở hữu:

Giá.

Quyền lợi thành viên.

Tồn kho.

Sellable.

Payment.

Shipping.

Bank account.

Claim ngoài whitelist.

IVR state.


### 23.2. Channel Gateway

Gateway chỉ là lớp routing/tương tác channel.

Gateway không được tự sở hữu:

Quote.

Claim policy.

CRM suppression.

IVR confirmation.


### 23.3. CRM

CRM chỉ được gửi nội dung dựa trên:

Customer Memory hợp lệ.

Order History hợp lệ.

Member Core hợp lệ.

Product Knowledge hợp lệ.

Suppression/Frequency/Quiet Hours hợp lệ.

Payment/Shipping eligibility nếu nội dung có nhắc đến giao dịch.


### 23.4. Landing Page

Landing Page chỉ được hiển thị nội dung public-safe.

Landing Page không được hardcode:

Giá runtime.

Shipping ETA.

COD availability.


### 23.5. Admin UI

Admin UI là operator workspace.

Admin UI không phải Source-of-Truth nếu chỉ hiển thị dữ liệu.

Admin UI chỉ được gửi command đến core, không tự quyết định state ngoài backend/core.


### 23.6. IVR

IVR chỉ là lớp xác nhận đơn hàng.

IVR không phải Order Source-of-Truth.

IVR result chỉ là event để Order Core đánh giá.


## 24. SOURCE-OF-TRUTH VÀ PUBLIC SAFETY

Dữ liệu được đưa ra public phải thỏa mãn:

Có Source-of-Truth.Có Public View hoặc Public DTO.Không chứa dữ liệu nội bộ.Không chứa claim bị cấm.Không chứa dữ liệu tài khoản ngân hàng hardcode.Không chứa dữ liệu tồn kho chi tiết nếu policy không cho public.Không chứa dữ liệu production/QC/defect nội bộ.Không chứa dữ liệu kế toán.Không chứa dữ liệu supplier/personnel/cost.

Áp dụng cho:

Live comment.

Messenger.

Public Trace.

ADS.

CRM outbound.

Email.

SMS/Zalo nếu có.

PDF public nếu có.


## 25. SOURCE-OF-TRUTH VÀ AUDIT

Audit bắt buộc với các nhóm:

Thay đổi master data trọng yếu.

Thay đổi giá.

Thay đổi chương trình.

Thay đổi quyền lợi thành viên.

Thay đổi Company Bank Account Registry.

Payment review.

Accounting Review.

Payment state transition.

Order state transition.

Shipping state transition.

IVR result event.

Manual override.

Batch release.

Warehouse receipt.

Recall.

Public Trace whitelist.

Permission/role.

Claim whitelist/denylist.

Gateway production config.

CRM suppression/frequency config.

Không có audit thì không được xem là pass Security/Audit Gate.


## 26. SOURCE-OF-TRUTH VÀ DONE GATE

Một source registry item chỉ được xem là Done khi:

[ ] Có source_id.[ ] Có owner pack.[ ] Có owner core.[ ] Có authority level.[ ] Có allowed consumers.[ ] Có forbidden consumers.[ ] Có access method.[ ] Có hardcode rule.[ ] Có public rule.[ ] Có audit rule nếu cần.[ ] Có evidence rule nếu cần.[ ] Có fallback rule.[ ] Có blocked_if_missing.[ ] Có related smoke.[ ] Có related evidence.[ ] Có gate mapping.

Nếu source item thiếu owner hoặc thiếu authority, source đó chưa được dùng để release.


## 27. TRẠNG THÁI TOÀN HỆ TRONG MASTER-01

MASTER-01 dựa trên trạng thái từ MASTER-00:

MASTER-01 không được thay đổi các trạng thái này.

MASTER-01 chỉ ghi thêm registry để các pack sau biết dữ liệu nào được quyền dùng, dữ liệu nào bị chặn, dữ liệu nào cần resolver, dữ liệu nào cần evidence.


## 28. KẾT LUẬN PHẦN 1/4


## PHẦN 1/4 của MASTER-01 đã xác lập:

Source-of-Truth là nguồn có thẩm quyền dữ liệu.Consumer không được tự suy diễn dữ liệu runtime.AI/Gateway/CRM/Landing Page/Admin UI/IVR không được bypass core.Dữ liệu giá, quyền lợi, tồn kho, payment, shipping, order, IVR, claim, public trace là nhóm hard lock.Source-of-Truth phải có owner, authority, access method, evidence rule và fallback rule.Thiếu Source-of-Truth thì không được ra quyết định.Thiếu resolver thì không được trả lời chắc chắn.Thiếu evidence P0 thì không được release.

Các trạng thái toàn hệ tiếp tục giữ:

HẾT PHẦN 1/4


## PHẦN 2/4 — GLOBAL SOURCE REGISTRY BY DOMAIN / OWNER CORE / CONSUMER BOUNDARY / RUNTIME ACCESS RULE


## 29. MỤC ĐÍCH CỦA PHẦN 2/4


## PHẦN 2/4 đăng ký Source-of-Truth theo từng domain dữ liệu trọng yếu trong hệ thống Ginsengfood.

Mục tiêu là để tất cả pack sau biết rõ:

Dữ liệu này thuộc core nào.

Pack nào sở hữu.

Pack nào chỉ được consume.

Pack nào không được tự quyết định.

Có cần resolver runtime hay không.

Có được hardcode hay không.

Có được public hay không.

Có cần audit/evidence hay không.

Thiếu dữ liệu thì block hay fallback.

Smoke/evidence nào phải kiểm tra.

Một domain dữ liệu trọng yếu chỉ có một Final Source-of-Truth.Consumer không được tự biến bản copy của dữ liệu thành nguồn quyết định.Runtime decision phải gọi resolver/core tương ứng.


## 30. DANH MỤC SOURCE REGISTRY CẤP MASTER

Danh mục Source-of-Truth toàn hệ gồm các nhóm chính:

SRC-GOV — Governance SourceSRC-PROD — Product / SKU SourceSRC-CLAIM — Claim / Public Wording SourceSRC-PRICE — Pricing / Program SourceSRC-MEMBER — Member / Benefit SourceSRC-CUSTOMER — Customer Profile / Memory SourceSRC-AVAIL — Availability / Sellable SourceSRC-PAY — Payment SourceSRC-BANK — Company Bank Account SourceSRC-SHIP — Shipping / COD / ETA SourceSRC-ORDER — Order / Cart / Quote SourceSRC-IVR — IVR Order Confirmation SourceSRC-CRM — CRM / Lifecycle SourceSRC-OPS — Operational Core SourceSRC-TRACE — Traceability / Public Trace SourceSRC-RECALL — Recall / Recovery SourceSRC-MISA — MISA / Accounting Integration SourceSRC-ADS — Ads / Tracking Signal SourceSRC-UI — Admin UI / Operator Workspace SourceSRC-EVD — Evidence / Smoke / UAT SourceSRC-SEC — Security / Permission / Audit Source

Mỗi nhóm có thể được tách sâu trong pack chi tiết.

MASTER-01 chỉ đăng ký cấp governance để các pack không viết lệch hoặc tự suy diễn.


## A. Governance Source


## 31. SRC-GOV-001 — MASTER GOVERNANCE SOURCE

Source ID

SRC-GOV-001

Source name


## Master Governance Source

Source type

TYPE 01 — Governance Source

Owning pack


## Pack-00 — Master Governance / Source-Of-Truth / Release Control

Owning core

Governance Core

Data domain

Pack registry.

File sequence.

Pack sequence.

Source-of-Truth principle.

Boundary.

Dependency.

Gate.

Evidence.

Smoke.

Release status.

Go-live status.

Future governed extension.

Decision authority

SRC-GOV-001 quyết định:

Pack nào là chính thức.

Pack nào là future governed extension.

Trạng thái toàn hệ.

Gate nào bắt buộc.

Source-of-Truth nào cần được đăng ký.

Pack nào không được bypass core.

Điều kiện release/go-live.

Allowed consumers

Tất cả MASTER file.

Tất cả pack chi tiết.

Dev handoff.

QA/Evidence.

Release review.

Owner sign-off.

Forbidden behavior

Không pack con nào được:

Tự đổi trạng thái release.

Tự bỏ gate.

Tự đổi boundary.

Tự thêm active scope trái MASTER.

Tự xem future governed extension là active scope.

Tự tuyên bố production ready khi MASTER chưa cho phép.

Access method

Read from approved MASTER document set.

Runtime required

Governance Source không phải runtime resolver.

Evidence required

Cần evidence khi dùng để release/go-live/sign-off.

Hardcode allowed

Không hardcode lại rule governance trong code theo cách tách khỏi tài liệu nguồn.

Public allowed

Đây là tài liệu nội bộ.

Audit required

Mọi thay đổi governance trọng yếu phải có owner decision/audit.

Fallback rule

Nếu thiếu governance source:

BLOCK triển khai chính thức.Không release.Không go-live.

bị chặn if missing

Related smoke

MAS-SMK-015 — Completion Report evidence

Done gate

MASTER Governance Done Gate


## 32. SRC-GOV-002 — FUTURE GOVERNED EXTENSION SOURCE

SRC-GOV-002


## Future Governed Extension Source


## Pack-00 / Master-07

Pack dự phòng.

Tích hợp tương lai.

Payment/shipping ngoài phạm vi trong nước nếu owner approved later.

Extension chưa active.

Future channel.

Future integration.

Future provider.

Future automation.

SRC-GOV-002 quyết định liệu một extension có được phép chuyển từ trạng thái dự phòng sang active scope hay không.

Current locked future extension

FUTURE_GOVERNED_EXTENSION = INTERNATIONAL_PAYMENT_OR_SHIPPING_IF_OWNER_APPROVED_LATER

Boundary

Phạm vi này không active mặc định.

Không pack nào được tự triển khai như scope hiện tại.

Không AI/Gateway/CRM/Admin UI/Landing Page nào được tự hứa khả năng thanh toán hoặc vận chuyển ngoài phạm vi trong nước nếu chưa có owner approval, provider evidence, compliance review và release gate.

Release Review.

Owner Decision Registry.

Payment/Shipping pack nếu sau này owner phê duyệt.

Không tự kích hoạt.

Không tự ghi là production scope.

Không tự dùng trong template khách hàng.

Không tự hứa với khách.

Không tự tạo API/provider integration nếu chưa có approval.


## B. Product / Sku / Claim Source


## 33. SRC-PROD-001 — PRODUCT KNOWLEDGE MASTER SOURCE

SRC-PROD-001


## Product Knowledge Master

TYPE 02 — Master Data Source


## Pack-02 — Product Knowledge / Sku Master / Claim Policy

20 SKU baseline.

Tên sản phẩm công khai.

SKU nội bộ.

Nhóm sản phẩm.

Thành phần.

Công dụng theo hướng thực dưỡng công khai.

Product Internal View.

Product Knowledge Data Contract.

Seed manifest nếu có.

SRC-PROD-001 quyết định:

Sản phẩm nào tồn tại trong catalog kiến thức.

Tên công khai được phép dùng.

SKU nội bộ map với tên sản phẩm nào.

Nội dung sản phẩm public-safe nào được dùng bởi AI/Gateway/CRM/Landing Page/ADS.

Phân nhóm sản phẩm.

Product knowledge nào được xem là hợp lệ.

Commerce Core khi cần mapping SKU.

Evidence Pack.

Test/Smoke Pack.

Các consumer không được:

Tự đặt tên sản phẩm.

Tự tạo SKU.

Tự đổi nhóm sản phẩm.

Tự viết thành phần.

Tự viết claim.

Tự tạo product public view.

Tự dùng tên nội bộ với khách hàng nếu policy không cho phép.

Product Knowledge Resolver / Product Public View / Product Internal View / approved seed.

Nếu không xác định được sản phẩm:

AI/Gateway/CRM không được tư vấn sâu hoặc quote.Phải hỏi rõ sản phẩm/nhu cầu hoặc dùng flow xác định sản phẩm.

MAS-SMK-001 — Product -> AI consultAIA-SMK-001 — Khách hỏi sản phẩm

Product Knowledge Done Gate


## 34. SRC-CLAIM-001 — CLAIM WHITELIST SOURCE

SRC-CLAIM-001


## Claim Whitelist Source

TYPE 07 — Public-Safe Source

Claim được nói.

Claim không được nói.

Public wording policy.

Cách chuyển hiệu dụng phương Đông sang ngôn ngữ public-safe.

Claim theo sản phẩm.

Claim theo channel.

Claim theo tình huống tư vấn.

Claim theo Landing Page/ADS/CRM.

SRC-CLAIM-001 quyết định nội dung nào được phép đưa ra public.

Public content.

Admin preview.

Evidence checker.

Không được:

Nói chữa bệnh.

Nói điều trị.

Nói thay thuốc.

Cam kết khỏi bệnh.

Tự tạo claim ngoài whitelist.

Tự copy nội dung nghiên cứu thành claim bán hàng nếu chưa được public-safe.

Tự dùng hiệu dụng quá mức trong ADS/CRM/Live/Landing Page.

Claim Guard / Claim Whitelist / Claim Denylist / Public Wording Policy.

Nếu claim chưa có whitelist:

Không public.Không dùng trong AI answer.Không dùng trong ADS.Không dùng trong Landing Page.Không dùng trong CRM.

MAS-SMK-001 — Product -> AI consultAIA-SMK-010 — Claim sensitive

Claim Compliance Done Gate


## C. Pricing / Program / Member Benefit Source


## 35. SRC-PRICE-001 — COMMERCE PRICING SOURCE

SRC-PRICE-001


## Commerce Pricing Source

TYPE 03 — Runtime Decision Source

Commerce Runtime — Quote / Order / Payment / Shipping

Listed price.

Program price.

Final price basis.

Quote price source.

Quote freeze.

Price effective time.

Program eligibility.

Price version if needed.

SRC-PRICE-001 quyết định giá nào được dùng trong quote tại thời điểm runtime.

Commerce Order Core.

Evidence/Smoke.

Consumer không được:

Tự tính giá cuối cùng.

Tự lấy giá cũ để quote.

Tự áp chương trình.

Tự giảm giá.

Tự tạo final total.

Tự public giá nếu channel policy không cho.

Tự sửa giá trong UI.

Pricing Resolver / Quote Cart / Quote Snapshot.

Nếu Pricing Resolver không trả dữ liệu:

Không quote.Không tạo Order Confirmation Draft.Không chốt đơn.

MAS-SMK-002 — AI quote runtimeAIA-SMK-003 — Khách hỏi giá SKU rõ

Commerce Pricing Done Gate


## 36. SRC-PRICE-002 — PROGRAM POLICY SOURCE

SRC-PRICE-002


## Program Policy Source

Chương trình 24/7.

Chương trình Giờ Vàng.

Program status.

Program discount.

Program time window.

Program quota nếu có.

Program priority.

Program conflict rule.

Program evidence.

SRC-PRICE-002 quyết định chương trình nào đang được áp dụng tại thời điểm quote.

Gateway.

Commerce Order.

Tự bịa chương trình.

Tự áp giảm giá.

Tự tạo urgency giả.

Tự nói “Giờ Vàng” nếu Program Core không active.

Tự kéo dài chương trình sau khi hết hạn.

Tự thay đổi quota/session.

Program Resolver / Quote Snapshot.

Nếu không xác định được chương trình:

Quote theo rule mặc định nếu Pricing Core cho phép.Không tự áp chương trình.


## 37. SRC-MEMBER-001 — MEMBER BENEFIT SOURCE

SRC-MEMBER-001


## Member Benefit Source

PACK-04 / PACK-05

Member Core / Member Benefit Core

Member tier.

Member status.

Member cycle.

Benefit eligibility.

Discount eligibility.

Grace period.

Maintain/upgrade status.

Customer price identity.

Member benefit conflict/priority.

SRC-MEMBER-001 quyết định khách có được áp quyền lợi member trong quote hay không.

Commerce Pricing.

AI tự hứa khách được giảm theo hạng.

CRM tự hứa quyền lợi khi chưa có resolver.

Gateway tự báo quyền lợi member.

Admin UI tự áp member discount nếu core chưa trả.

Tự cộng dồn quyền lợi nếu policy không cho.

Member Benefit Resolver / Customer Price Identity Resolver.

Nếu không xác định được member eligibility:

Không áp quyền lợi member.Không hứa quyền lợi.Có thể nói hệ thống cần xác nhận quyền lợi trong quote flow.

MAS-SMK-002 — AI quote runtimeAIA-SMK-004 — Khách là member

Member Benefit Done Gate


## D. Customer / Crm Memory Source


## 38. SRC-CUSTOMER-001 — CUSTOMER PROFILE SOURCE

SRC-CUSTOMER-001


## Customer Profile Source

CRM Runtime — Member Lifecycle

Customer Profile Core

Customer identity.

Contact information.

Customer status.

New/returning customer.

Order history summary.

Member link.

Preferred recipient.

Address book if allowed.

Consent/suppression if applicable.

SRC-CUSTOMER-001 quyết định khách là ai, có dữ liệu gì được dùng cho cá nhân hóa, prefill, CRM, quote và order draft.

Tự ghép khách bằng dữ liệu mơ hồ.

Tự prefill thông tin nhận hàng nếu chưa có customer memory hợp lệ.

Tự dùng thông tin cũ cho đơn tặng mà không cảnh báo đổi người nhận.

Tự bỏ qua chăm sóc khách cũ trước khi bán tiếp.

Tự gửi CRM nếu khách bị suppression/opt-out.

Customer Profile Resolver / Customer Memory Resolver.

Nếu chưa xác định khách:

Xử lý như khách mới.Không prefill.Không áp member benefit.Không dùng lịch sử mua.


## 39. SRC-CRM-001 — CRM LIFECYCLE SOURCE

SRC-CRM-001


## Crm Lifecycle Source

Lifecycle stage.

Post-purchase event.

Repurchase event.

Member care event.

Suppression.

Quiet hours.

Frequency cap.

Retry rule.

Campaign eligibility.

Customer care before next sale.

Product recommendation history.

SRC-CRM-001 quyết định khi nào được gửi CRM message, gửi nội dung gì, có bị chặn bởi suppression/frequency/quiet hours hay không.

Gửi vượt frequency cap.

Gửi ngoài quiet hours.

Gửi khi bị suppression.

Gửi claim ngoài whitelist.

Gửi payment/shipping promise nếu chưa có eligibility.

Nhảy thẳng vào bán hàng với khách đã mua mà không hỏi thăm trải nghiệm khi rule yêu cầu.

CRM Lifecycle Resolver / Suppression Resolver / Frequency Cap Resolver.

Only public-safe content through approved channel.

Nếu CRM resolver lỗi:

Không gửi automation.Không gửi campaign.Có thể chuyển manual review.

MAS-SMK-011 — CRM suppressionAIA-SMK-008 — Khách cũ quay lại

CRM Lifecycle Done Gate


## E. Availability / Inventory / Sellable Source


## 40. SRC-AVAIL-001 — SELLABLE AVAILABILITY SOURCE

SRC-AVAIL-001


## Sellable Availability Source

PACK-04 / PACK-06

Inventory / Availability Core + Operational Core

SKU sellable status.

Stock available for sale.

Sale lock.

Quality hold.

Recall hold.

Channel hold.

Batch release dependency.

Warehouse availability.

Allocation.

OOS state.

Alternative recommendation eligibility.

SRC-AVAIL-001 quyết định sản phẩm có được bán tại thời điểm hiện tại hay không.

Commerce Core.

Bán khi chưa sellable.

Hứa còn hàng khi Availability Resolver chưa trả.

Dựa vào Product Catalog để kết luận còn hàng.

Bán batch chưa release.

Bán hàng đang quality hold/recall hold/sale lock.

Public tồn kho chi tiết nếu policy không cho.

Availability Resolver / Sellable Availability API.

Only safe availability message if policy allows.Do not reveal detailed stock unless approved.

Nếu thiếu availability:

Không bán.Không quote order-ready.Không tạo Order Confirmation Draft.Có thể tư vấn sản phẩm nhưng không chốt bán.

MAS-SMK-012 — Operational sellable availabilityAIA-SMK-009 — Sản phẩm hết hàng

Sellable Availability Done Gate


## F. Payment / Bank Account Source


## 41. SRC-PAY-001 — PAYMENT ELIGIBILITY SOURCE

SRC-PAY-001


## Payment Eligibility Source

Payment method eligibility.

VNPAY eligibility.

MoMo Business eligibility.

Direct bank transfer eligibility.

Payment instruction eligibility.

Payment block reason.

Payment provider status.

Payment evidence requirement.

SRC-PAY-001 quyết định khách có thể dùng phương thức thanh toán nào tại thời điểm order/quote.

Allowed payment strategy

PAYMENT_STRATEGY = VNPAY + MOMO_BUSINESS + DIRECT_BANK_TRANSFER_TO_SSAVIGROUP

AI tự hứa phương thức thanh toán.

Gateway tự nói có VNPAY/MoMo/chuyển khoản.

CRM tự hứa thanh toán.

Landing Page tự hiển thị payment method nếu chưa có eligibility.

Admin UI tự cho chọn payment method trái core.

Static template tự ghi payment instruction.

Tự chuyển PAID.

Payment Eligibility Resolver / Commerce Payment Core API.

Only if Payment Core returns eligibility and channel/message is approved.

Nếu Payment Eligibility thiếu:

Không hứa payment method.Không gửi payment instruction.Không tạo payment-confirmed state.

MAS-SMK-003 — Payment eligibilityCOM-SMK-PAY-001 / 002 / 007

Payment Eligibility Done Gate


## 42. SRC-BANK-001 — COMPANY BANK ACCOUNT REGISTRY SOURCE

SRC-BANK-001


## Company Bank Account Registry Source

Company Bank Account Registry / Commerce Payment Core

Company bank account.

Account holder.

Bank name.

Account number.

VietQR configuration if used.

Active/inactive status.

Payment instruction template reference.

Payment reference / transfer memo rule.

Accounting review queue link.

SRC-BANK-001 là nguồn duy nhất được phép cung cấp thông tin tài khoản ngân hàng SSAVIGROUP cho direct bank transfer.

Commerce Payment Core.

Payment Instruction Resolver.

Admin Payment Review.

Accounting Review Queue.

Customer Payment Flow nếu Payment Core cho phép.

Forbidden consumers / behavior

Không được hardcode ở:

AI response.

Gateway template.

CRM message.

Admin UI frontend component.

Static HTML.

PDF template.

Print template.

Notification worker.

IVR script.

Hiển thị tài khoản nếu Payment Eligibility không cho phép.

Dùng tài khoản ngoài registry.

Tạo bank transfer order thiếu payment_reference / transfer memo.

Đánh dấu PAID chỉ vì khách nói đã chuyển khoản.

Đánh dấu PAID chỉ vì khách gửi ảnh chuyển khoản.

Payment Instruction Resolver reads Company Bank Account Registry.

Only as generated payment instruction from Payment Core after eligibility.

Nếu Company Bank Account Registry không khả dụng:

Không hiển thị tài khoản ngân hàng.Không tạo bank transfer instruction.Chuyển sang payment method khác nếu Payment Core cho phép.Nếu không có phương thức hợp lệ, giữ order ở payment selection/review.

MAS-SMK-004 — Bank transfer governanceCOM-SMK-PAY-003 — Customer chọn chuyển khoảnCOM-SMK-PAY-004 — Bank transfer orderCOM-SMK-PAY-008 — Static template scan

Company Bank Account Registry Done Gate


## 43. SRC-PAY-002 — PAYMENT STATE SOURCE

SRC-PAY-002


## Payment State Source

TYPE 04 — Transaction State Source

Payment chưa đủ điều kiện.

Payment instruction generated.

Payment awaiting confirmation.

Payment review required.

Payment confirmed.

Payment failed.

Payment cancelled.

Payment audit.

PAID state.

SRC-PAY-002 quyết định payment state của order.

Rule khóa

No payment may be marked PAID without Payment Core confirmation or Accounting Review confirmation.

Allowed confirmation sources

Payment Core confirmationAccounting Review confirmation

Not allowed as final confirmation

Khách nói đã chuyển khoản.

AI/Gateway/CRM nhận tin.

Admin UI click không qua core.

Worker tự đánh dấu.

Gateway webhook không qua Payment Core.

AI/Gateway/CRM only as read-only payment status if allowed.

Admin UI tự chuyển PAID.

AI tự xác nhận đã thanh toán.

Gateway tự xác nhận đã thanh toán.

CRM tự xác nhận đã thanh toán.

IVR tự xác nhận thanh toán.

Worker tự PAID nếu thiếu confirmation hợp lệ.

Payment State Machine / Payment Core API.

Nếu payment confirmation thiếu:

Không PAID.Giữ trạng thái chưa đủ điều kiện/review theo Payment Core.

MAS-SMK-005 — No PAID without confirmationCOM-SMK-PAY-005 / 006

Payment State Done Gate


## G. Shipping / Cod / Eta Source


## 44. SRC-SHIP-001 — SHIPPING ELIGIBILITY SOURCE

SRC-SHIP-001


## Shipping Eligibility Source

Domestic shipping eligibility.

Shipping zone.

Shipping fee.

Free shipping rule if applicable.

COD eligibility.

Fallback ETA.

Tracking eligibility.

Shipping provider eligibility.

Address validation.

SRC-SHIP-001 quyết định có được hứa giao hàng, COD, phí ship hoặc ETA hay không.

Current active scope

DOMESTIC SHIPPING

Not active by default

Payment/shipping ngoài phạm vi trong nước chỉ là future governed extension nếu owner approved later.

AI tự hứa ETA.

Gateway tự hứa giao hàng.

CRM tự hứa giao hàng.

Landing Page hardcode ETA/phí ship.

Admin UI tự gán shipping method nếu core không cho.

Tự hứa COD.

Tự nói miễn phí ship nếu rule chưa trả.

Tự tạo tracking nếu shipping task chưa hợp lệ.

Shipping Eligibility Resolver / Commerce Shipping Core API.

Nếu Shipping Eligibility thiếu:

Không hứa giao hàng.Không hứa ETA.Không hứa COD.Không hứa phí ship.Không hứa miễn phí ship.Chuyển sang hỏi thêm địa chỉ hoặc shipping review.

MAS-SMK-006 — Shipping eligibilityCOM-SMK-SHIP-001 đến COM-SMK-SHIP-007

Shipping Eligibility Done Gate


## 45. SRC-SHIP-002 — SHIPPING STATE SOURCE

SRC-SHIP-002


## Shipping State Source

Shipping chưa đủ điều kiện.

Shipping review.

Shipping created.

Shipping handed off.

Shipping tracking.

Delivery status.

COD status if applicable.

Shipping failure.

Shipping cancellation.

SRC-SHIP-002 quyết định trạng thái vận chuyển của đơn hàng.

AI/Gateway/CRM as read-only status if allowed.

Customer notification if approved.

AI tự nói đã giao/đang giao.

Gateway tự tạo tracking.

CRM tự cập nhật delivery status.

Admin UI tự thay đổi trạng thái nếu không qua command/core.

Worker tự chuyển trạng thái nếu thiếu provider/core confirmation.

Shipping State Machine / Shipping Core API.

Nếu shipping state không rõ:

Không hứa trạng thái giao hàng.Chuyển sang Admin/Shipping Review.


## H. Order / Quote / Cart Source


## 46. SRC-ORDER-001 — QUOTE SNAPSHOT SOURCE

SRC-ORDER-001


## Quote Snapshot Source

Commerce Quote Core

Quote ID.

Quote timestamp.

Product line.

Member benefit.

VAT if applicable.

Final total.

Quote expiry.

Payment eligibility.

Shipping eligibility.

SRC-ORDER-001 quyết định quote nào là quote hợp lệ tại thời điểm báo giá/chốt đơn.

CRM if needed.

AI tự tạo quote ngoài Commerce Core.

Gateway tự quote public.

CRM tự báo giá cuối cùng.

Landing Page tự tính total.

Admin UI tự sửa final total.

Dev tự tính bằng công thức rời rạc ngoài Quote Core.

Quote Cart / Quote Snapshot API.

Only under approved channel rules.

Nếu Quote Snapshot thiếu:

Không chốt đơn.Không tạo Order Confirmation Draft.Không giữ giá.

MAS-SMK-008 — Order Confirmation Draft

Quote Snapshot Done Gate


## 47. SRC-ORDER-002 — ORDER CONFIRMATION DRAFT SOURCE

SRC-ORDER-002


## Order Confirmation Draft Source

Draft order lines.

Product name.

Quantity.

Price.

Program benefit.

COD if any.

Payment options.

Shipping options.

Recipient information.

Customer confirmation action.

Draft expiry.

Draft evidence.

SRC-ORDER-002 quyết định bản xác nhận đơn nháp nào hợp lệ để khách kiểm tra và xác nhận.

Gateway/Messenger.

AI tạo order chính thức trước khi khách xác nhận.

Gateway tạo order chính thức trước confirmation.

CRM chốt lại đơn chỉ bằng text không có draft.

Admin UI bỏ qua draft confirmation nếu flow yêu cầu.

Tóm tắt text thay cho form/draft khi đủ điều kiện chốt đơn.

Thiếu payment/shipping/final total trong draft.

Order Confirmation Draft API / Commerce Order Core.

Only in private/order flow, not public comment.

Nếu thiếu draft hợp lệ:

Không tạo order chính thức.Không trả order_code.Yêu cầu hoàn thiện draft.

MAS-SMK-008 — Order Confirmation DraftAIA-SMK-005 — Khách chốt đơn

Order Confirmation Draft Done Gate


## 48. SRC-ORDER-003 — ORDER STATE MACHINE SOURCE

SRC-ORDER-003


## Order State Machine Source

Order created.

Order chưa đủ điều kiện confirmation.

Order confirmed.

Order cancelled.

Order payment chưa đủ điều kiện.

Order payment confirmed.

Order shipping chưa đủ điều kiện.

Order fulfilled.

Order failed.

Order review.

Order IVR required.

Order IVR result evaluation.

SRC-ORDER-003 là nguồn duy nhất quyết định trạng thái đơn hàng.

IVR.

Payment Core.

Shipping Core.

AI tự cập nhật order state.

Gateway tự cập nhật order state.

CRM tự cập nhật order state.

IVR/SIM Gateway tự cập nhật order state.

Admin UI tự đổi state nếu không qua command/state machine.

Worker tự đổi state nếu thiếu idempotency/audit.

Order State Machine API / Command Handler.

Nếu order state không rõ:

Không tiếp tục fulfillment.Không gọi IVR nếu chưa có confirmation task.Không đánh dấu order confirmed/cancelled.Chuyển review.

MAS-SMK-009 — IVR confirmationMAS-SMK-005 — No PAID without confirmation

Order State Machine Done Gate


## I. Ivr Source


## 49. SRC-IVR-001 — IVR ORDER CONFIRMATION SOURCE

SRC-IVR-001


## Ivr Order Confirmation Source

TYPE 03 — Runtime Decision Source / TYPE 06 — Evidence Source

PACK-09 — IVR ORDER CONFIRMATION

IVR Confirmation Core + Commerce Order Core

IVR call eligibility.

IVR task.

IVR queue.

SIM capacity.

Call attempt.

DTMF result.

Technical error.

No-answer.

Order Core evaluation.

IVR chỉ quyết định call result event.

Order Core mới quyết định trạng thái đơn tiếp theo.

Locked scope

IVR locked rules

IVR chỉ xác nhận đơn hàng.Internal SIM Gateway Server là mô hình mặc định.1 SIM = 1 active outbound call.Phím 1 = xác nhận đơn.Phím 0 = khách hủy / không đặt đơn.Giờ Vàng = 10 phút, 2 cuộc, cách 5 phút.24/7 = 15 phút, 3 cuộc, lịch T0/T0+5/T0+10.Rolling real-time queue required.Batch after session calling prohibited.SIM Gateway không được cập nhật order state trực tiếp.Mọi hủy/tiếp tục xử lý đơn từ IVR phải qua Core Order State Machine.Technical error is not customer no-answer.IVR không dùng cho sales, CRM, marketing, tư vấn sản phẩm hoặc chăm sóc đại trà.

Monitoring.

Dùng IVR để sales.

Dùng IVR để CRM.

Dùng IVR để marketing.

Dùng IVR để tư vấn sản phẩm.

Dùng IVR để chăm sóc đại trà.

Dùng IVR để upsell/cross-sell.

SIM Gateway tự đổi trạng thái đơn.

Technical error bị tính là khách không nghe máy.

Batch calling sau session.

Order Core confirmation task -> IVR Queue -> SIM Gateway -> IVR Result Event -> Order Core State Machine.

Nếu IVR lỗi kỹ thuật:

Không đánh khách là no-answer.Không tự hủy đơn.Giữ order trong review/confirmation flow theo Order Core.Có thể chuyển manual confirmation nếu rule cho phép.

MAS-SMK-009 — IVR confirmationMAS-SMK-010 — IVR technical errorIVR-SMK-001 đến IVR-SMK-010

IVR Order Confirmation Done Gate


## J. Operational / Trace / Recall / Misa Source


## 50. SRC-OPS-001 — OPERATIONAL CORE SOURCE

SRC-OPS-001


## Operational Core Source

TYPE 05 — Operational State Source

PACK-01 — OPERATIONAL CORE

Source origin.

Raw material intake.

Incoming QC.

Raw lot readiness.

SKU/ingredient/recipe.

Formula snapshot.

Production order.

Material issue.

Material receipt.

Process chain.

Packaging/printing/QR.

Batch QC.

Inventory ledger.

Sellable availability.

Traceability.

CAPA.

MISA integration boundary.

SRC-OPS-001 quyết định trạng thái vận hành, sản xuất, kho, release, trace, recall.

Commerce Availability.

Trace/Public Trace.

MISA Integration.

Dashboard.

Commerce bán batch chưa release.

Inventory ledger bị mutate.

Warehouse receipt trước release.

Public trace lấy dữ liệu nội bộ.

Recall không có snapshot.

MISA sync trực tiếp không qua integration layer.

UI tự bỏ qua state machine.

Operational Core API / Operational State Resolver / Inventory Ledger / Trace Core.

Only through Public Trace DTO / public whitelist.

Nếu operational state không rõ:

Không bán.Không release.Không trace public.Không sync MISA nếu thiếu boundary/evidence.Chuyển review.

MAS-SMK-012 — Operational sellable availabilityOPS-SMK-001 đến OPS-SMK-010

Operational Core Done Gate


## 51. SRC-TRACE-001 — PUBLIC TRACE SOURCE


## Src-Trace-001


## Public Trace Source


## Pack-06 — Traceability / Public Trace

Public trace DTO.

QR lookup.

Product public data.

Safe source origin fields.

Safe batch trace fields.

Public trace whitelist.

Public trace suppression rule.

Internal trace graph separation.

SRC-TRACE-001 quyết định dữ liệu nào được phép public khi khách quét QR.

Public Trace UI.

Landing Page if embedded.

Forbidden public fields

Không public:

Supplier detail nếu không whitelist.

Personnel/operator/approver.

Costing/accounting/MISA.

QC defect/loss/waste.

Internal trace graph IDs.

Internal root cause.

Formula nội bộ.

Customer/order/shipment nếu không thuộc public trace.

Evidence file nội bộ.

Sensitive operational metadata.

Public Trace API / PublicTracePublicResponse / Public Trace DTO.

Nếu Public Trace DTO thiếu/không an toàn:

Không public trace.Hiển thị lỗi an toàn hoặc chuyển hỗ trợ.Không leak dữ liệu nội bộ.

MAS-SMK-013 — Public trace safetyOPS-SMK-009 — Public trace

Public Trace Safety Done Gate


## 52. SRC-RECALL-001 — RECALL / RECOVERY SOURCE

SRC-RECALL-001


## Recall / Recovery Source

PACK-06 — RECALL / RECOVERY / CAPA

Recall Core

Incident.

Recall case.

Impact analysis.

Affected lot/batch/order.

Hold/recovery.

Recall snapshot.

Residual risk.

Closure evidence.

SRC-RECALL-001 quyết định recall/recovery workflow và affected scope.

Trace Core.

CRM if customer communication approved.

Thu hồi không có snapshot.

Ghi đè recall snapshot.

Public root cause nội bộ nếu chưa approved.

CRM gửi thông báo rủi ro nếu chưa có owner/legal/QA approval.

Commerce tiếp tục bán lô/batch bị hold/recall.

Recall Core / Recall Impact Resolver / CAPA workflow.

Only approved public-safe recall/customer communication.

Nếu recall source thiếu:

Không close recall.Không public communication.Không recovery release.Chuyển review.


## 53. SRC-MISA-001 — MISA / ACCOUNTING INTEGRATION SOURCE

SRC-MISA-001


## Misa / Accounting Integration Source

TYPE 05 — Operational / Integration State Source

PACK-06 / PACK-04

Integration Core / Accounting Integration Boundary

MISA sync boundary.

Accounting handoff.

Payment/accounting review.

Warehouse/production accounting document.

Sync state.

Retry state.

Reconcile state.

Error evidence.

SRC-MISA-001 quyết định dữ liệu nào được handoff sang MISA hoặc accounting integration, thông qua integration layer.

Accounting.

Integration Console.

Sync trực tiếp không qua integration layer.

Bypass accounting review.

Tự tạo PAID nếu chưa có confirmation.

Ghi nhận kế toán không trace được source.

Dùng MISA làm source thay cho Operational/Commerce state nếu không có mapping.

Integration Layer / MISA Sync Worker / Accounting Review Queue.

Nếu MISA integration lỗi:

Không mất dữ liệu gốc.Giữ outbox/retry/reconcile.Không sửa source history.Không tự thay đổi state trái core.


## K. Ads / Admin Ui / Evidence / Security Source


## 54. SRC-ADS-001 — ADS TRACKING SIGNAL SOURCE

SRC-ADS-001


## Ads Tracking Signal Source

TYPE 06 — Evidence Source / Tracking Source

PACK-08 — ADS / TRACKING / LEARNING ENGINE / CAMPAIGN SIGNAL

Tracking / Attribution Core

Campaign source.

Creative.

Tracking event.

Messenger conversion.

Order conversion.

Revenue attribution.

Lead quality.

Learning signal.

SRC-ADS-001 quyết định tracking/attribution signal, không quyết định commerce data.

ADS dashboard.

Commerce reporting.

CRM segment if approved.

ADS không được quyết định:

Quyền lợi member.

Tracking Event Contract / Attribution Resolver.

Nếu tracking thiếu:

Không dùng để tối ưu/attribution chính thức.Không làm thay đổi order/payment/shipping.


## 55. SRC-UI-001 — ADMIN UI DISPLAY SOURCE

SRC-UI-001


## Admin Ui Display Source

TYPE 05 — Display-only / Operator Workspace Source

Admin Runtime — internal operator surface

Admin UI reads backend APIs; backend/core remain decision owner.

Admin dashboard.

Operator screen.

Payment review screen.

Shipping review screen.

Order management screen.

CRM review screen.

Gateway monitoring.

IVR monitoring.

Evidence viewer.

Release checklist.

Admin UI không phải final Source-of-Truth cho nghiệp vụ lõi.

Admin UI chỉ hiển thị dữ liệu và gửi command hợp lệ đến core.

Internal operators.

QA.

Evidence reviewer.

Admin UI không được:

Hardcode nghiệp vụ lõi.

Hardcode tài khoản ngân hàng.

Hardcode PAID.

Hardcode shipping state.

Tự đổi order/payment/shipping state ngoài core.

Bypass permission.

Bypass audit.

Hiển thị dữ liệu public/private sai boundary.

Admin API / backend service / permission guarded command.

Nếu API/core không trả dữ liệu:

Admin UI hiển thị error/empty state.Không tự tạo dữ liệu giả.Không cho operator thao tác state nếu core unavailable.


## 56. SRC-EVD-001 — EVIDENCE REGISTRY SOURCE

SRC-EVD-001


## Evidence Registry Source

TYPE 06 — Evidence Source


## Pack-10 — Completion / Evidence / Gateway

Evidence Registry / QA Evidence Core

Evidence item.

Smoke evidence.

UAT evidence.

API response.

DB snapshot.

Audit event.

Completion Report.

Evidence status.

Missing evidence list.

Retest required list.

SRC-EVD-001 quyết định evidence item nào được chấp nhận cho Completion Report và Release Gate.

All packs.

Admin Evidence Viewer.

Tự pass nếu thiếu evidence.

Dùng demo miệng thay evidence.

Dùng screenshot không gắn test ID làm evidence cuối.

Dùng local run không trace requirement làm release evidence.

Bỏ qua P0 evidence.

Tự đổi Completion Report thành accepted khi evidence còn thiếu.

Evidence Registry / Evidence Package / Completion Report.

Self-referential: registry itself must be auditable.

Nếu evidence thiếu:

Evidence Gate


## 57. SRC-SEC-001 — SECURITY / PERMISSION / AUDIT SOURCE

SRC-SEC-001


## Security / Permission / Audit Source

TYPE 01 — Governance Source / TYPE 05 — Operational Control Source

Security / Audit Control

Security Core / Permission Core / Audit Core

Role.

Permission.

Backend enforcement.

Admin UI permission.

High-risk action approval.

Break-glass if any.

Credential policy.

Public/private data boundary.

Sensitive data handling.

Bank account safety.

Public trace safety.

SRC-SEC-001 quyết định quyền truy cập, quyền thao tác và audit bắt buộc.

Backend APIs.

Frontend che nút nhưng backend không kiểm quyền.

Admin UI cho thao tác vượt quyền.

API không audit high-risk action.

Hardcode secret/bank account.

Public trace leak nội bộ.

Manual override không có reason/audit.

Gateway/IVR/CRM thao tác order/payment/shipping ngoài permission.

Permission Resolver / Audit Core / Security Policy.

Nếu permission/audit không rõ:

Không cho thao tác.Không release protected operation.

MAS-SMK-014 — Admin permission/auditSEC smoke

Security / Audit Done Gate


## 58. BẢNG TÓM TẮT SOURCE REGISTRY CẤP MASTER

Final owner

Runtime source

Hardcode

Master Governance

PACK-00

N/A

All packs

Future Governed Extension

N/A until active

Release review

Product Knowledge

PACK-02

Product Resolver

AI/Gateway/CRM/LDP/ADS

Claim Whitelist

Commerce Pricing

PACK-04

AI/Gateway/CRM/Admin

Program Policy

AI/Gateway/Commerce

Member Resolver

AI/CRM/Commerce

Customer Profile

PACK-05

Customer Resolver

CRM Lifecycle

CRM/AI/Gateway

Sellable Availability

AI/Commerce/Gateway

Payment Eligibility

Payment Resolver

Payment Flow/Admin

Payment State

Payment State Machine

Order/Admin

Shipping Eligibility

Shipping Resolver

Shipping State

Shipping State Machine

Quote Snapshot

Quote Core

AI/Gateway/Order

Order Confirmation Draft

Order Draft Core

Order Core

AI/Gateway/IVR/Admin

IVR Confirmation

PACK-09 + PACK-04

IVR Queue/Event + Order Core

PACK-06

Operational Resolver

Commerce/Admin/Trace

Public Trace

Public Trace/UI

Recall/Recovery

Admin/Trace/CRM

MISA Integration

Integration Layer

Accounting/Admin

Ads Tracking Signal

PACK-08

Tracking Contract

ADS/Dashboard

Admin UI Display

PACK-10

Backend APIs

Operator

Release/QA

Security/Audit

Permission/Audit Core


## 59. GLOBAL CONSUMER BOUNDARY RULE

Tất cả consumer phải tuân thủ:

Consumer chỉ được đọc hoặc gọi Source-of-Truth qua contract được phép.Consumer không được tự tạo bản sao để quyết định runtime.Consumer không được hardcode dữ liệu trọng yếu.Consumer không được cập nhật state nếu không sở hữu state machine.Consumer không được public dữ liệu nếu không có Public View/Public DTO/Claim Whitelist.Consumer không được tính Done nếu không có evidence.


## 60. GLOBAL RUNTIME ACCESS RULE

Các nhóm dữ liệu sau bắt buộc phải gọi runtime resolver:

Giá cuối cùng -> Pricing Resolver / Quote SnapshotChương trình -> Program ResolverQuyền lợi member -> Member Benefit ResolverSellable -> Availability ResolverPayment method -> Payment Eligibility ResolverBank transfer instruction -> Payment Instruction Resolver + Company Bank Account RegistryPayment state -> Payment State MachineShipping/ETA/COD/phí ship -> Shipping Eligibility ResolverOrder state -> Order State MachineIVR call -> Order Core confirmation task + IVR QueueCRM sending -> Suppression/Frequency/Quiet Hours ResolverClaim -> Claim GuardPublic trace -> Public Trace DTOPermission -> Permission ResolverAudit -> Audit Core

Nếu resolver không trả kết quả hợp lệ, consumer phải fallback theo rule đã đăng ký.


## 61. GLOBAL NO-HARDCODE RULE

Không được hardcode các dữ liệu sau ở consumer:

Company bank account.Final price.Program discount.Member discount.Payment method availability.Payment instruction.Payment state.Shipping ETA.Shipping fee.COD availability.Order state.IVR result decision.Sellable status.Public trace field.Permission.Claim ngoài whitelist.

Đặc biệt, không được hardcode tài khoản ngân hàng tại:

AI response.Gateway template.CRM message.Admin UI frontend component.Landing Page.Static HTML.PDF template.Print template.Notification worker.Internal script.IVR script.


## 62. GLOBAL PUBLIC SOURCE RULE

Dữ liệu public phải đi qua một trong các nguồn public-safe:

Product Public ViewClaim WhitelistPublic Trace DTOQuote Snapshot public-safe messagePayment Core approved payment instructionShipping Core approved public-safe shipping messageCRM approved public-safe messageLanding Page approved contentADS approved content

Nếu không có public-safe source:

Không public.Không đưa lên Landing Page.Không trả trong comment public.Không dùng trong ADS.Không gửi CRM outbound.Không đưa vào Public Trace.


## 63. GLOBAL EVIDENCE REQUIREMENT BY SOURCE

Các source sau luôn cần evidence khi release:

SRC-PRICE-001SRC-PRICE-002SRC-MEMBER-001SRC-AVAIL-001SRC-PAY-001SRC-BANK-001SRC-PAY-002SRC-SHIP-001SRC-SHIP-002SRC-ORDER-001SRC-ORDER-002SRC-ORDER-003SRC-IVR-001SRC-OPS-001SRC-TRACE-001SRC-RECALL-001SRC-MISA-001 nếu trong release scopeSRC-EVD-001SRC-SEC-001

Nếu thiếu P0 evidence:


## 64. GLOBAL bị chặn-IF-MISSING RULE

Nếu một source trọng yếu bị missing, áp dụng:

Missing source

Hành động

Không tư vấn sâu / không quote

Không public claim

Không quote

Không áp member benefit

Không bán

Không hứa payment

Không hiển thị bank account

Payment Reference

Không tạo bank transfer order hợp lệ

Payment Confirmation

Không PAID

Không hứa shipping/ETA/COD/phí ship

Không chốt đơn

Không tạo order chính thức

Không đổi order state

Order Core confirmation task

IVR không gọi

CRM Suppression/Frequency

CRM không gửi

Operational Release

Không bán batch

Không public trace

Evidence P0

Không release

Security/Permission

Không thao tác protected action


## 65. KẾT LUẬN PHẦN 2/4


## PHẦN 2/4 đã đăng ký Source-of-Truth theo domain trọng yếu:

Governance.Product/SKU.Claim.Pricing/Program.Member.Customer/CRM.Availability/Sellable.Payment.Company Bank Account Registry.Payment State.Shipping.Order/Quote/Draft.IVR.Operational.Trace.Recall.MISA.ADS.Admin UI.Evidence.Security/Audit.

Các khóa quan trọng của phần này:

Giá không được tự tính.Quyền lợi member không được tự hứa.Tồn kho/sellable không được tự đoán.Payment không được tự hứa.Tài khoản ngân hàng không được hardcode.PAID không được tự đánh dấu.Shipping/ETA/COD/phí ship không được tự hứa.Order state không được tự cập nhật.IVR không được tự đổi trạng thái đơn.Claim không được public ngoài whitelist.Public Trace phải whitelist-only.Evidence P0 thiếu thì không release.Security/permission thiếu thì không thao tác.

Trạng thái toàn hệ tiếp tục giữ:

HẾT PHẦN 2/4


## PHẦN 3/4 — SOURCE VALIDATION / CONFLICT RESOLUTION / FALLBACK CONTROL / EVIDENCE & AUDIT / PUBLIC-SAFE & NO-HARDCODE CONTROL


## 66. MỤC ĐÍCH CỦA PHẦN 3/4


## PHẦN 3/4 xác định cách kiểm soát Source-of-Truth trong quá trình triển khai và vận hành.

Nếu PHẦN 2/4 đã đăng ký nguồn nào sở hữu dữ liệu, thì PHẦN 3/4 trả lời:

Khi consumer lấy dữ liệu thì kiểm tra đúng nguồn bằng cách nào?

Khi hai nguồn mâu thuẫn thì nguồn nào thắng?

Khi resolver không trả dữ liệu thì fallback thế nào?

Dữ liệu nào bắt buộc có evidence?

Dữ liệu nào bắt buộc có audit?

Dữ liệu nào tuyệt đối không được hardcode?

Dữ liệu nào được public?

Dữ liệu nào chỉ được dùng nội bộ?

Khi phát hiện sai Source-of-Truth thì block/review/rollback ra sao?

Source-of-Truth không chỉ được ghi trong tài liệu.Source-of-Truth phải được kiểm soát trong runtime, UI, worker, AI response, Gateway flow, CRM flow, Evidence và Release Gate.


## 67. SOURCE VALIDATION LÀ GÌ

Source Validation là quá trình kiểm tra dữ liệu mà một pack đang dùng có đến từ đúng Source-of-Truth hay không.

Một dữ liệu chỉ được xem là hợp lệ khi trả lời được 7 câu hỏi:


## 1. Dữ liệu này thuộc domain nào?2. Source ID của dữ liệu là gì?3. Owning pack/core là gì?4. Consumer có được phép dùng dữ liệu này không?5. Dữ liệu có cần runtime resolver không?6. Dữ liệu có cần evidence/audit không?7. Nếu dữ liệu thiếu hoặc lỗi thì fallback/block thế nào?

Nếu không trả lời được 7 câu hỏi này, dữ liệu chưa đủ điều kiện dùng cho release.


## 68. SOURCE VALIDATION CHECKPOINT TOÀN HỆ

Mỗi source item trọng yếu phải đi qua các checkpoint sau:

CHECKPOINT 01 — Source ID existsCHECKPOINT 02 — Owning pack/core existsCHECKPOINT 03 — Authority level existsCHECKPOINT 04 — Allowed consumer existsCHECKPOINT 05 — Access method existsCHECKPOINT 06 — Runtime resolver requirement existsCHECKPOINT 07 — Hardcode rule existsCHECKPOINT 08 — Public rule existsCHECKPOINT 09 — Evidence rule existsCHECKPOINT 10 — Audit rule existsCHECKPOINT 11 — Fallback rule existsCHECKPOINT 12 — Related smoke existsCHECKPOINT 13 — Done gate exists

Nếu thiếu checkpoint trọng yếu, source item chưa được xem là hoàn tất.


## 69. SOURCE VALIDATION THEO NHÓM RỦI RO

Không phải mọi source đều có cùng mức rủi ro.

MASTER-01 phân source thành 4 nhóm rủi ro:

SOURCE_RISK_P0 — Critical runtime / financial / order / public safetySOURCE_RISK_P1 — Release-critical operational / CRM / securitySOURCE_RISK_P2 — Internal display / dashboard / reportingSOURCE_RISK_P3 — Test / sandbox / non-production


### 69.1. SOURCE_RISK_P0

P0 gồm:

Final price.

Member benefit applied to quote.

Payment state / PAID.

Order State Machine.

IVR result handling.

Claim Whitelist for public content.

Security/permission for protected action.

P0 thiếu hoặc sai thì:

Không release.Không go-live.Không cho consumer tự fallback bằng suy đoán.Không cho hardcode.Bắt buộc evidence.Bắt buộc audit nếu có state/action.


### 69.2. SOURCE_RISK_P1

P1 gồm:

CRM lifecycle.

Customer Memory.

Suppression/frequency cap.

Operational state.

Recall/CAPA.

Gateway routing.

Admin UI display for operational actions.

ADS conversion signal if revenue reporting is used.

P1 thiếu hoặc sai thì:

Không cho pack liên quan pass Done Gate.Có thể không block toàn hệ nếu không nằm trong release scope.Nếu nằm trong release scope thì block release.


### 69.3. SOURCE_RISK_P2

P2 gồm:

Internal analytics.

Non-critical display label.

Internal report draft.

Monitoring summary not tied to action.

P2 thiếu hoặc sai thì:

Không được dùng để quyết định runtime.Không được dùng làm evidence P0.Có thể đưa vào review/fix.


### 69.4. SOURCE_RISK_P3

P3 gồm:

Mock data.

Local seed.

Test account.

Demo script.

Temporary fixture.

P3 không được dùng trong production.

TEST_ONLY data cannot become production Source-of-Truth.


## 70. SOURCE VALIDATION MATRIX

Source group

Risk

P0

Theo quote/channel policy

Private/personalized only

Limited safe message

Theo Payment Core

Chỉ qua Payment Instruction

Payment State / PAID

Theo order/payment status policy

Theo Shipping Core

Quote / Draft / Order

Private/order flow

IVR Result

Product Public View

P0/P1

Không tự viết

Không ngoài whitelist

P1

Theo channel policy

Operational State

P1/P0 nếu sellable

Chỉ Public Trace DTO

P0 for release


## 71. SOURCE CONFLICT LÀ GÌ

Source Conflict xảy ra khi hai nguồn dữ liệu trả kết quả khác nhau cho cùng một domain.

AI template có giá A, Pricing Resolver trả giá B.

Landing Page hiển thị ETA, Shipping Resolver không trả ETA.

CRM nói khách được ưu đãi member, Member Benefit Resolver không trả quyền lợi.

Admin UI hiển thị tài khoản ngân hàng khác Company Bank Account Registry.

IVR result nói khách bấm 0, nhưng Order Core chưa xử lý state.

Batch có QC_PASS, nhưng Operational Core chưa RELEASED.

Public trace hiển thị dữ liệu nội bộ không có trong Public Trace DTO.

Conflict không được giải bằng suy đoán.Conflict phải giải theo authority order.


## 72. GLOBAL SOURCE CONFLICT RESOLUTION ORDER

Khi có xung đột source, xử lý theo thứ tự ưu tiên:


## 1. MASTER Governance Source — nếu xung đột về boundary/gate/release/scope.2. Final Source-of-Truth của domain.3. Runtime Decision Source.4. Approved Consumer View.5. Evidence Source.6. Display-only data.7. Test-only data.

Diễn giải:

Nếu tranh luận về scope/gate -> MASTER thắng.

Nếu tranh luận về dữ liệu gốc -> core sở hữu domain thắng.

Nếu tranh luận về quyết định tại thời điểm hiện tại -> resolver/runtime core thắng.

Nếu tranh luận về nội dung hiển thị -> approved consumer view thắng.

Evidence chỉ chứng minh sự kiện đã xảy ra, không tự thay thế core.

Display-only không được quyết định state.

Test-only không được dùng production.


## 73. CONFLICT RULE — PRICING

Nếu có xung đột giá:

Pricing Resolver / Quote Snapshot thắng.AI/Gateway/CRM/Landing Page/Admin UI thua.Static content thua.Manual text thua.

Không được dùng:

Giá trong ảnh.

Giá trong tin nhắn cũ.

Giá trong comment.

Giá trong template.

Giá trong Landing Page nếu không trace tới Quote/Pricing.

Giá do dev tự tính.

Hành động:

Block quote.Require Pricing Resolver response.Regenerate Quote Snapshot.Collect evidence.


## 74. CONFLICT RULE — PROGRAM / PROMOTION

Nếu có xung đột chương trình:

Program Resolver thắng.Runtime Program Policy thắng.Template/CRM/Gateway copy thua.

Tự nói Giờ Vàng đang mở nếu Program Resolver không active.

Tự áp 24/7 nếu Program Resolver không trả.

Tự kéo dài thời hạn giữ giá.

Do not apply program.Do not quote program price.Fallback to normal quote only if Pricing Core allows.


## 75. CONFLICT RULE — MEMBER BENEFIT

Nếu có xung đột quyền lợi member:

Member Benefit Resolver thắng.Customer Profile thô không đủ để tự áp quyền lợi.CRM text thua.AI text thua.Admin UI display thua nếu không trace resolver.

Tự áp discount theo tier nếu chưa có eligibility.

Tự cộng dồn quyền lợi.

Tự hứa nâng hạng/giữ hạng nếu Member Core chưa trả.

Tự dùng lifetime revenue thay cho eligibility hiện tại nếu policy không cho.

Do not apply member benefit.Show non-member/program quote if Pricing Core allows.Or keep quote chưa đủ điều kiện member eligibility.


## 76. CONFLICT RULE — PAYMENT

Nếu có xung đột payment method:

Payment Eligibility Resolver thắng.Commerce Payment Core thắng.AI/Gateway/CRM/Landing Page/Admin UI static text thua.

Tự hứa VNPAY.

Tự hứa MoMo Business.

Tự hứa chuyển khoản.

Tự hứa COD như payment nếu Payment/Shipping Core chưa trả.

Tự gửi payment instruction nếu Payment Core chưa cho.

Do not promise payment.Do not display payment instruction.Return payment chưa đủ điều kiện/selection state.Collect Payment Eligibility evidence.


## 77. CONFLICT RULE — COMPANY BANK ACCOUNT

Nếu có xung đột tài khoản ngân hàng:

Company Bank Account Registry thắng tuyệt đối.Mọi nơi khác thua.

Các nguồn bị coi là không hợp lệ nếu khác registry:

Admin UI frontend hardcode.

Hành động bắt buộc:

Block rendered bank instruction.Remove hardcoded value.Force Registry-only rendering.Scan all consumer templates/components.Collect evidence.Route through Payment Instruction Resolver.

Không có Registry thì:

Không hiển thị tài khoản ngân hàng.Không tạo bank transfer instruction.Không tạo bank transfer order hợp lệ.


## 78. CONFLICT RULE — PAYMENT STATE / PAID

Nếu có xung đột payment state:

Payment State Machine / Payment Core thắng.Accounting Review confirmation thắng nếu là manual review path.AI/Gateway/CRM/Admin UI text thua.Ảnh chuyển khoản chỉ là evidence/input, không phải PAID source.

Không được PAID dựa trên:

Ảnh giao dịch.

AI nhận tin.

Gateway nhận tin.

CRM nhận tin.

Worker tự set.

Bank account instruction generated.

Keep payment chưa đủ điều kiện/review.Require Payment Core confirmation or Accounting Review confirmation.Audit transition.Collect evidence.


## 79. CONFLICT RULE — SHIPPING / ETA / COD

Nếu có xung đột shipping:

Shipping Eligibility Resolver thắng.Commerce Shipping Core thắng.AI/Gateway/CRM/Landing Page/Admin UI text thua.

Tự hứa giao được.

Tự hứa ETA.

Tự hứa phí ship.

Tự hứa miễn phí ship.

Tự tạo tracking.

Do not promise shipping.Do not promise ETA.Ask for missing address if needed.Route to Shipping Eligibility Resolver.Route to shipping review if resolver fails.


## 80. CONFLICT RULE — ORDER STATE

Nếu có xung đột trạng thái đơn:

Order State Machine thắng.Commerce Order Core thắng.AI/Gateway/CRM/IVR/Admin UI thua nếu không qua command/state machine.

AI tự tạo order chính thức.

Gateway tự tạo order chính thức.

CRM tự xác nhận order.

IVR tự hủy/xác nhận order.

Admin UI tự đổi trạng thái không qua command.

Worker tự đổi state không idempotency/audit.

Stop downstream flow.Read current Order State.Submit valid command if allowed.Audit state transition.Collect evidence.


## 81. CONFLICT RULE — IVR

Nếu có xung đột IVR:

IVR call log/result event là evidence của call.Order Core State Machine mới quyết định trạng thái đơn.

SIM Gateway cập nhật order state trực tiếp.

Technical error tính là customer no-answer.

IVR tự hủy đơn.

IVR dùng cho sales/CRM/marketing.

Keep IVR result as event.Send event to Order Core.Order Core evaluates.If technical error, do not mark customer no-answer.If timeout, follow Order Core policy.


## 82. CONFLICT RULE — AVAILABILITY / SELLABLE

Nếu có xung đột availability:

Availability Resolver thắng.Operational release + warehouse availability là nguồn nền.Product Catalog không đủ để bán.AI/Gateway/Landing Page text thua.

Bán SKU chỉ vì có trong Product Knowledge.

Bán khi quality hold/recall hold/sale lock.

Hứa còn hàng nếu resolver không trả.

Do not sell.Do not create Order Confirmation Draft.Suggest alternatives only if Availability Resolver returns eligible alternatives.


## 83. CONFLICT RULE — CLAIM / PUBLIC WORDING

Nếu có xung đột claim:

Claim Whitelist / Public Wording Policy thắng.AI-generated text thua.Writer text thua.ADS creative thua.Landing Page copy thua.CRM copy thua.

Dùng nghiên cứu nội bộ thành claim bán hàng nếu chưa public-safe.

Tự viết claim ngoài whitelist.

Block public release.Rewrite through Claim Policy.Collect claim compliance evidence.


## 84. CONFLICT RULE — PUBLIC TRACE

Nếu có xung đột Public Trace:

Public Trace DTO / Public Trace Whitelist thắng.Internal trace graph thua nếu public.Admin trace view thua nếu public.Operational detail thua nếu không whitelist.

Supplier detail không whitelist.

Internal trace graph ID.

Customer/order/shipment.

Suppress forbidden fields.Use PublicTracePublicResponse only.Run public leak test.Collect evidence.


## 85. CONFLICT RULE — SECURITY / PERMISSION

Nếu có xung đột quyền:

Backend Permission Core thắng.Frontend visibility thua.Admin UI button state thua.Manual operator assumption thua.

Chỉ ẩn nút frontend nhưng backend không kiểm quyền.

Cho thao tác high-risk không audit.

Manual override không reason.

Break-glass không scope/expiry nếu có.

Gateway/IVR/CRM gọi command trái permission.

Block protected action.Require permission resolver.Audit attempt if needed.Collect security evidence.


## 86. FALLBACK CONTROL LÀ GÌ

Fallback Control là quy tắc xử lý khi Source-of-Truth hoặc runtime resolver không sẵn sàng.

Fallback không có nghĩa là tự đoán.

Fallback đúng là:

Không biết -> không khẳng định.Không có eligibility -> không hứa.Không có confirmation -> không chuyển state.Không có evidence -> không Done.Không có permission -> không cho thao tác.


## 87. FALLBACK LEVELS

MASTER-01 định nghĩa 6 cấp fallback:

FALLBACK-00 — Block actionFALLBACK-01 — Ask for missing inputFALLBACK-02 — Keep chưa đủ điều kiện/reviewFALLBACK-03 — Use approved safe defaultFALLBACK-04 — Manual reviewFALLBACK-05 — Disable feature/provider temporarily


### 87.1. FALLBACK-00 — Block action

Dùng khi hành động có rủi ro cao.

Không có Payment Eligibility -> không hứa payment.

Không có Bank Registry -> không gửi số tài khoản.

Không có Payment Confirmation -> không PAID.

Không có Order State Machine -> không đổi order state.

Không có Public Trace DTO -> không public trace.


### 87.2. FALLBACK-01 — Ask for missing input

Dùng khi thiếu input hợp lý.

Thiếu địa chỉ -> hỏi địa chỉ trước khi shipping eligibility.

Thiếu sản phẩm -> hỏi sản phẩm trước khi quote.

Thiếu số lượng -> hỏi số lượng trước khi draft.

Thiếu thông tin người nhận -> yêu cầu ghi thêm trước order confirmation.


### 87.3. FALLBACK-02 — Keep chưa đủ điều kiện/review

Dùng khi dữ liệu có thể cần review.

Khách gửi ảnh chuyển khoản -> giữ payment review.

Shipping provider lỗi -> giữ shipping review.

IVR technical error -> giữ order confirmation review.

Conflict giữa two sources -> giữ review.


### 87.4. FALLBACK-03 — Use approved safe default

Chỉ dùng khi safe default đã được duyệt.

Không có dữ liệu member -> xử lý như khách chưa xác định member, không áp quyền lợi.

Không có chương trình active -> không áp chương trình, quote thường nếu Pricing Core cho phép.

Không có public trace field optional -> ẩn field optional.


### 87.5. FALLBACK-04 — Manual review

Dùng khi cần người vận hành kiểm tra.

Payment evidence không khớp.

Shipping address không xác thực.

Order conflict.

Recall/recovery decision.

Customer complaint/quality issue.

MISA sync mismatch.


### 87.6. FALLBACK-05 — Disable feature/provider temporarily

Dùng khi provider/core có lỗi hệ thống.

Payment provider lỗi.

Shipping provider lỗi.

IVR Gateway lỗi.

Gateway webhook lỗi.

CRM automation lỗi.


## 88. FALLBACK MATRIX THEO SOURCE

Source missing/error

Fallback

Không được làm

Ask missing input / block deep consult

Không tự đặt tên sản phẩm

Block public content

Không tự viết claim

Block quote

Không tự tính giá

Safe default nếu Pricing Core cho phép

Không tự áp khuyến mãi

Safe default no member benefit

Không tự hứa quyền lợi

Block sale

Block payment promise

Không hứa VNPAY/MoMo/chuyển khoản

Bank Registry

Block bank instruction

Không hiển thị tài khoản

Keep chưa đủ điều kiện/review

Ask input/review/block promise

Không hứa ETA/COD/phí ship

Block draft/order

Order Draft

Block order creation

Không trả order_code

Order State

Review/block downstream

Không đổi state

IVR technical error

Review/manual confirmation

Không no-answer/cancel

CRM suppression unknown

Block outbound

Không gửi CRM

Operational release unknown

Block sellable

Public Trace DTO missing

Block public trace

Không leak internal trace

Permission unknown

Block action

Không cho thao tác

Evidence missing


## 89. EVIDENCE CONTROL BY SOURCE

Evidence Control xác định source nào cần bằng chứng để pass Done Gate.

Evidence tối thiểu gồm:

evidence_idsource_idpack_idrequirement_idrule_idtest_idsmoke_id nếu cóenvironmenttimestampinput_dataoutput_resultartifact_typeartifact_locationreview_status

Không có source_id trong evidence thì không đủ traceability.


## 90. EVIDENCE REQUIRED — PAYMENT

Payment evidence bắt buộc gồm:

Payment Eligibility response.VNPAY eligibility evidence.MoMo Business eligibility evidence.Direct bank transfer eligibility evidence.Company Bank Account Registry response.Payment instruction generated by Payment Core.payment_reference / transfer memo evidence.Accounting Review Queue evidence.Payment Core confirmation evidence.Payment state transition evidence.Audit log.No-hardcoded-bank scan evidence.

Nếu thiếu:

Không pass Payment Done Gate.Không pass Completion Report.Không release payment flow.


## 91. EVIDENCE REQUIRED — SHIPPING

Shipping evidence bắt buộc gồm:

Shipping Eligibility response.Address validation if needed.Domestic shipping eligibility.COD eligibility if used.Shipping fee evidence.Free shipping rule evidence if applied.Fallback ETA evidence.Tracking evidence if generated.Shipping state transition evidence.Audit log if manual override.AI/Gateway/CRM transcript showing no unsupported ETA promise.

Không pass Shipping Done Gate.Không được hứa shipping/ETA/COD/phí ship.Không release shipping flow.


## 92. EVIDENCE REQUIRED — ORDER / QUOTE

Order/Quote evidence bắt buộc gồm:

Pricing Resolver response.Program Resolver response if applicable.Member Benefit Resolver response if applicable.Availability Resolver response.Quote Snapshot.Order Confirmation Draft.Customer Confirmation.Order State Machine transition.Payment option evidence.Shipping option evidence.Final total evidence.Audit log.

Không tạo order chính thức.Không trả order_code.Không pass Order Done Gate.


## 93. EVIDENCE REQUIRED — IVR

IVR evidence bắt buộc gồm:

Order Core confirmation task.IVR queue entry.SIM capacity evidence.1 SIM = 1 active outbound call evidence.Call attempt log.Giờ Vàng / 24/7 call window evidence.Retry timing evidence.DTMF 1 evidence.DTMF 0 evidence.Technical error evidence.No-answer evidence.IVR result event.Order Core evaluation evidence.Order State Machine transition evidence.Admin monitoring screenshot.


## 94. EVIDENCE REQUIRED — PRODUCT / CLAIM

Product/Claim evidence bắt buộc gồm:

Product Knowledge source.Product Public View.Product Internal View if needed.SKU mapping.Claim Whitelist.Claim Denylist.Public Wording Policy.AI response transcript.Landing Page content evidence.CRM message evidence.ADS content evidence.Claim compliance review.

Không public content.Không pass Product/Claim Done Gate.


## 95. EVIDENCE REQUIRED — OPERATIONAL / TRACE / RECALL

Operational/Trace/Recall evidence bắt buộc gồm:

Source origin evidence.Raw material intake evidence.Incoming QC evidence.READY_FOR_PRODUCTION evidence.Production order evidence.Formula snapshot.Material issue ledger.Material receipt evidence.Process chain evidence.Batch QC evidence.Batch release evidence.Warehouse receipt evidence.Inventory ledger evidence.Sellable availability evidence.Public Trace DTO evidence.Public leak test evidence.Recall snapshot evidence.CAPA evidence if applicable.MISA integration evidence if in scope.

Không pass Operational Done Gate.Không bán batch liên quan.Không pass Public Trace release nếu thiếu public-safe evidence.


## 96. AUDIT CONTROL BY SOURCE

Audit bắt buộc với mọi hành động có thể thay đổi:

Master data.

Runtime policy.

Program.

CRM suppression/frequency cap.

Product/Claim whitelist.

Audit tối thiểu phải có:

audit_idactorrolesource_idobject_typeobject_idactionfrom_stateto_statereason nếu cầntimestampcorrelation_ididempotency_key nếu liên quan commandresult

Không audit thì không pass Security/Audit Gate.


## 97. NO-HARDCODE CONTROL

No-Hardcode Control là gate bắt buộc cho các source P0.

Không được hardcode:

Bank account.Final price.Program discount.Member discount.Payment method.Payment instruction.Payment state.Shipping ETA.Shipping fee.COD eligibility.Order state.IVR decision.Sellable status.Public Trace field.Permission.Claim ngoài whitelist.


## 98. NO-HARDCODE SCAN SCOPE

No-hardcode scan phải kiểm tra tối thiểu:

AI response templates.Gateway templates.CRM messages.Landing Page.Admin UI frontend components.Static HTML.PDF template.Print template.Notification worker.Internal scripts.IVR scripts.Config files.Seed files nếu có nguy cơ bị dùng production.

Đặc biệt với bank account:

Không có tài khoản ngân hàng SSAVIGROUP nào được hardcode ngoài Company Bank Account Registry / Payment configuration đã được phê duyệt.


## 99. PUBLIC-SAFE CONTROL

Public-Safe Control kiểm soát dữ liệu trước khi đưa ra public.

Public surfaces gồm:

Messenger nếu khách nhìn thấy.

Email/SMS/Zalo nếu có.

Dữ liệu public phải qua:

Product Public View.Claim Whitelist.Public Wording Policy.Public Trace DTO.Payment Core approved payment instruction.Shipping Core approved shipping message.Quote Snapshot public-safe output.CRM approved message.

Internal SKU nếu policy không cho.Formula nội bộ.Supplier/personnel/cost/accounting.QC defect/loss/waste nội bộ.Internal trace graph.Bank account hardcoded.Unapproved payment/shipping promise.Therapeutic claim.Unapproved recall/root-cause detail.


## 100. SOURCE VALIDATION SMOKE CHECK

Mỗi pack phải có ít nhất một smoke để chứng minh:

Consumer gọi đúng Source-of-Truth.Consumer không hardcode dữ liệu P0.Resolver failure được fallback đúng.Source conflict được xử lý đúng.Evidence được thu đúng source_id.Audit được ghi cho action trọng yếu.Public output không leak dữ liệu forbidden.

AI hỏi giá -> phải có Pricing Resolver + Availability + Payment/Shipping eligibility nếu AI nói các phần đó.Gateway hỏi payment -> phải có Payment Eligibility, không hardcode bank account.CRM outbound -> phải có suppression/frequency/claim/payment/shipping guard.Admin UI payment review -> phải gọi Payment Core, không tự PAID.IVR result -> phải gửi Order Core, không tự đổi order state.Public Trace -> phải dùng Public Trace DTO, không dùng Admin Trace DTO.


## 101. SOURCE VALIDATION DONE GATE

Một source registry domain được xem là pass khi:

[ ] Source ID tồn tại.[ ] Source owner rõ.[ ] Source authority rõ.[ ] Consumer boundary rõ.[ ] Access method rõ.[ ] Runtime resolver rõ nếu cần.[ ] No-hardcode rule rõ.[ ] Public-safe rule rõ.[ ] Fallback rule rõ.[ ] Conflict resolution rõ.[ ] Evidence requirement rõ.[ ] Audit requirement rõ.[ ] Smoke mapping rõ.[ ] Done Gate mapping rõ.

Nếu source domain là P0, thêm điều kiện:

[ ] Evidence accepted.[ ] Audit evidence accepted nếu có state/action.[ ] No-hardcode scan pass.[ ] Resolver failure fallback test pass.[ ] Public leak test pass nếu có public surface.


## 102. RELEASE IMPACT OF SOURCE FAILURE

Nếu source validation fail, ảnh hưởng release như sau:

Failure

Release impact

Governance source fail

Block toàn hệ

Pricing source fail

Block quote/order

Payment source fail

Block payment/order

Bank Registry fail

Block direct bank transfer

Payment State fail

Block PAID/fulfillment

Shipping source fail

Block shipping promise/fulfillment

Order source fail

Block order creation/state

IVR source fail

Block IVR flow

Availability source fail

Claim source fail

Public Trace source fail

Security source fail

Block protected operation/release

Evidence source fail

Trạng thái toàn hệ nếu P0 fail:


## 103. KẾT LUẬN PHẦN 3/4


## PHẦN 3/4 đã khóa cơ chế kiểm soát Source-of-Truth khi triển khai:

Source validation.Risk classification.Conflict resolution.Fallback control.Evidence control.Audit control.No-hardcode control.Public-safe control.Smoke validation.Done Gate.Release impact.

Các nguyên tắc quan trọng:

Source đúng mới được dùng.Resolver đúng mới được trả lời chắc chắn.Evidence đúng mới được Done.Audit đúng mới được pass protected action.Public-safe đúng mới được đưa ra khách hàng.No-hardcode pass mới được release.Conflict phải giải theo authority order.Fallback không được suy đoán.

HẾT PHẦN 3/4


## PHẦN 4/4 — FINAL SOURCE-OF-TRUTH DONE GATE / PACK MAPPING / HANDOFF CONTROL / MASTER-01 FINAL CONCLUSION


## 104. MỤC ĐÍCH CỦA PHẦN 4/4


## PHẦN 4/4 là phần khóa cuối của MASTER-01.

Phần này dùng để xác định:

Source-of-Truth Registry hoàn tất khi nào.

Pack nào phải dùng source nào.

Source nào bắt buộc dùng runtime resolver.

Source nào tuyệt đối không được hardcode.

Source nào bắt buộc có evidence.

Source nào bắt buộc có audit.

Source nào được phép public.

Source nào chỉ dùng nội bộ.

Điều kiện handoff từ MASTER-01 sang MASTER-02 và các pack chi tiết.

Điều kiện để source registry được dùng làm nền triển khai.

Trạng thái toàn hệ sau khi khóa MASTER-01.

MASTER-01 không thay thế pack chi tiết.

MASTER-01 chỉ khóa quyền sở hữu dữ liệu và nguyên tắc sử dụng dữ liệu.


## 105. FINAL SOURCE-OF-TRUTH DONE GATE LÀ GÌ

Final Source-of-Truth Done Gate là cổng kiểm tra cuối cùng để xác định MASTER-01 đã hoàn thành vai trò registry dữ liệu toàn hệ hay chưa.

MASTER-01 chỉ được xem là Done khi:

[ ] Đã xác định các nhóm Source-of-Truth toàn hệ.[ ] Đã xác định Source ID standard.[ ] Đã xác định Source Authority Level.[ ] Đã xác định domain source registry.[ ] Đã xác định owner pack / owner core.[ ] Đã xác định allowed consumers.[ ] Đã xác định forbidden consumers / forbidden behavior.[ ] Đã xác định runtime resolver requirement.[ ] Đã xác định no-hardcode rule.[ ] Đã xác định public-safe rule.[ ] Đã xác định fallback rule.[ ] Đã xác định conflict resolution rule.[ ] Đã xác định evidence requirement.[ ] Đã xác định audit requirement.[ ] Đã xác định release impact khi source fail.[ ] Đã giữ đúng trạng thái toàn hệ từ MASTER-00.

MASTER-01 Done không có nghĩa hệ thống đã production ready.

MASTER-01 Done chỉ có nghĩa là registry dữ liệu có thẩm quyền đã đủ để các pack sau không tự suy diễn dữ liệu.


## 106. MASTER-01 DONE KHÔNG ĐỒNG NGHĨA VỚI IMPLEMENTATION DONE

Cần phân biệt rõ:

MASTER-01 Done != Database Done != API Done != Resolver Done != UI Done != Worker Done != Smoke Pass != Evidence Accepted != Production Ready != Release Approved != Go-live Approved

MASTER-01 chỉ cho phép các pack sau viết tiếp theo đúng registry.

MASTER-01 nói Company Bank Account Registry là Source-of-Truth.

Nhưng COM-06 mới viết chi tiết cấu trúc registry, API, DTO, permission, audit, evidence.

MASTER-01 nói Shipping Eligibility Resolver là nguồn runtime.

Nhưng COM-07 mới viết chi tiết cách resolver nhận input, trả output, xử lý địa chỉ, COD, ETA.

MASTER-01 nói IVR Result Event không tự đổi order state.

Nhưng IVR pack mới viết queue, DTMF, retry, technical error, SIM capacity.

MASTER-01 nói Public Trace DTO là public-safe source.

Nhưng Operational/Trace pack mới viết DTO, API, whitelist field và test leak.


## 107. FINAL SOURCE REGISTRY STATUS

Tại cuối MASTER-01, Source Registry được xem là đã khóa ở cấp governance.

Trạng thái:

MASTER-01_SOURCE_REGISTRY_STRUCTURE = COMPLETESOURCE_DOMAIN_REGISTRY = COMPLETESOURCE_AUTHORITY_MODEL = COMPLETESOURCE_CONSUMER_BOUNDARY = COMPLETESOURCE_VALIDATION_RULE = COMPLETESOURCE_FALLBACK_RULE = COMPLETESOURCE_EVIDENCE_RULE = COMPLETESOURCE_AUDIT_RULE = COMPLETESOURCE_NO_HARDCODE_RULE = COMPLETESOURCE_PUBLIC_SAFE_RULE = COMPLETE

Nhưng trạng thái toàn hệ vẫn giữ:


## 108. FINAL SOURCE REGISTRY TABLE — LOCKED MASTER VIEW

Bảng dưới đây là bản nhìn tổng hợp cuối của MASTER-01.

Release impact nếu thiếu

Block governance/release

Future Extension

MASTER-07 / Owner Decision

Release Review

Block future extension

Block consult/quote/display

Block program quote

Block member benefit

Customer

Block personalization/prefill

CRM Core

Block CRM outbound

Sellable

Inventory/Availability Core

Block bank transfer instruction

Block shipping/ETA/COD promise

Block shipping status

Quote Snapshot API

Block quote/order draft

Order Draft API

Block state transition

IVR + Order Core

Block operational/sellable

Recall

Recall Workflow

Block recall/recovery

Integration Core

MISA Sync Worker

Block MISA done

Tracking Core

Block Ads done

Admin UI

Admin API

Block Admin UI release


## 109. FINAL OWNER CORE MAPPING

Mỗi core có quyền sở hữu dữ liệu như sau:

Owner Core

Source owned

Quyền quyết định

SRC-GOV-001, SRC-GOV-002

Scope, boundary, gate, release

Product/SKU knowledge

Public wording / claim

SRC-PRICE-001, SRC-PRICE-002

Price/program/quote basis

Member benefit eligibility

Customer identity/memory

CRM sending eligibility

Sellable availability

SRC-PAY-001, SRC-PAY-002

Payment eligibility/state

Bank account source

SRC-SHIP-001, SRC-SHIP-002

Shipping/COD/ETA/state

SRC-ORDER-001, SRC-ORDER-002, SRC-ORDER-003

Quote/draft/order state

IVR Confirmation Core + Order Core

IVR event + order evaluation

Production/warehouse/trace/release

Public trace whitelist DTO

Recall/recovery/CAPA

MISA/accounting sync boundary

Ads/tracking/attribution signal

Backend/Admin API

Admin display/command surface

Completion evidence

Permission/audit/security


## 110. FINAL CONSUMER BOUNDARY MATRIX

Consumer

Được dùng

AI Advisor

Product, Claim, Pricing, Member, Availability, Payment, Shipping, Order Draft, Customer Memory

Không tự tính giá, không tự hứa payment/shipping, không hardcode bank account, không tạo order chính thức trước confirmation

Channel Gateway

AI/Commerce/CRM outputs, Payment/Shipping eligibility, Gateway policy

Không quote public sai policy, không tự order state, không hardcode payment/bank/shipping

Customer, Member, Product, Claim, Suppression, Payment/Shipping eligibility nếu có nội dung giao dịch

Không gửi vượt frequency/quiet/suppression, không hứa payment/shipping khi thiếu eligibility

Landing Page

Product Public View, Claim Whitelist, Commerce Entry, approved tracking

Không hardcode giá runtime, bank account, ETA, COD, payment method

Backend APIs, permission-guarded commands, evidence viewer

Không hardcode business state, không tự PAID, không bypass permission/audit

Order Core task, IVR queue, SIM gateway, DTMF result

Không sales/CRM/marketing, không tự đổi order state, không tính technical error là no-answer

Tracking signal, approved content, conversion signal

Không quyết định giá/payment/shipping/tồn kho/order state

Worker

Queue/event contracts

Không tự cập nhật state ngoài state machine, không thiếu idempotency/audit

Không dùng Admin/Internal Trace DTO

Evidence/QA

Evidence Registry, logs, screenshots, smoke reports

Không pass khi thiếu P0 evidence

Không chỉ dựa frontend visibility


## 111. FINAL RUNTIME RESOLVER MATRIX

Các runtime resolver bắt buộc:

Runtime need

Required resolver/source

Nếu thiếu

Báo giá

Pricing Resolver + Quote Snapshot

Áp chương trình

Không áp chương trình

Áp quyền lợi member

Không áp quyền lợi

Bán/chốt SKU

Nói payment method

Hiển thị chuyển khoản

Payment Instruction Resolver + Company Bank Account Registry

Chuyển PAID

Payment State Machine + Payment/Accounting confirmation

Nói giao hàng/ETA/COD/phí ship

Không hứa shipping

Tạo order draft

Order Confirmation Draft API

Không tạo order

Đổi order state

Gọi IVR

Order Core confirmation task + IVR Queue

Không gọi

Gửi CRM outbound

Suppression/Frequency/Quiet Hours Resolver

Không gửi

Nói claim

Không nói claim ngoài whitelist

Public trace

Protected action

Permission Resolver

Release


## 112. FINAL NO-HARDCODE LOCK

Các dữ liệu sau bị khóa NO-HARDCODE ở cấp MASTER-01:

Company bank account.Final price.Program discount.Member discount.Payment method availability.Payment instruction.Payment state.PAID state.Shipping ETA.Shipping fee.COD eligibility.Order state.Order code generation.IVR decision.Sellable status.Batch release status.Public trace fields.Permission.Claim ngoài whitelist.

Phạm vi scan bắt buộc:

AI response templates.Gateway templates.CRM messages.Landing Page.Admin UI frontend components.Static HTML.PDF template.Print template.Notification worker.Internal scripts.IVR scripts.Config files.Seed files có nguy cơ dùng production.

Đặc biệt:

Không có thông tin tài khoản ngân hàng SSAVIGROUP nào được hardcode ngoài Company Bank Account Registry / approved Payment configuration.


## 113. FINAL PUBLIC-SAFE LOCK

Dữ liệu public chỉ được đi qua nguồn public-safe.

Nguồn public-safe gồm:

Product Public View.Claim Whitelist.Public Wording Policy.Public Trace DTO.Quote Snapshot public-safe output nếu channel cho phép.Payment Core approved payment instruction.Shipping Core approved shipping message.CRM approved message.Landing Page approved content.ADS approved content.

Formula nội bộ.Supplier/personnel/cost/accounting.QC defect/loss/waste nội bộ.Internal trace graph.Bank account hardcoded.Unapproved payment/shipping promise.Therapeutic claim.Unapproved recall/root-cause detail.Customer/order/shipment data ngoài scope public trace.Evidence file nội bộ.


## 114. FINAL FALLBACK LOCK

Fallback toàn hệ được khóa như sau:

Missing / error

Final fallback

Không tư vấn sâu / hỏi rõ sản phẩm

Không áp/hứa quyền lợi

Không giữ giá / không chốt

Order Core IVR Task

IVR Technical Error

Không tính khách no-answer

CRM Suppression Unknown

Không gửi CRM outbound

Operational Release Unknown

Public Trace DTO Missing

Permission Unknown

Không cho protected action

Evidence Missing


## 115. FINAL CONFLICT RESOLUTION LOCK

Khi có xung đột dữ liệu, dùng thứ tự sau:


## 1. MASTER Governance Source2. Final Source-of-Truth của domain3. Runtime Decision Source4. Approved Consumer View5. Evidence Source6. Display-only data7. Test-only data

Các kết luận cụ thể:

Pricing Resolver thắng AI/Gateway/CRM/Landing Page text.Payment Core thắng mọi payment text/template.Company Bank Account Registry thắng tuyệt đối về tài khoản ngân hàng.Payment State Machine thắng mọi lời nói/ảnh chuyển khoản về PAID.Shipping Resolver thắng mọi ETA/phí ship/COD text.Order State Machine thắng AI/Gateway/CRM/IVR/Admin UI.Claim Whitelist thắng AI-generated content.Public Trace DTO thắng Admin/Internal Trace View khi public.Backend Permission Core thắng frontend visibility.


## 116. FINAL EVIDENCE LOCK

Evidence phải gắn Source ID.

Mẫu evidence tối thiểu:

Nếu evidence thiếu source_id, evidence chưa đủ chuẩn traceability.

Các source P0 bắt buộc có evidence trước release:

SRC-PRICE-001SRC-PRICE-002SRC-MEMBER-001SRC-AVAIL-001SRC-PAY-001SRC-BANK-001SRC-PAY-002SRC-SHIP-001SRC-SHIP-002SRC-ORDER-001SRC-ORDER-002SRC-ORDER-003SRC-IVR-001SRC-OPS-001SRC-TRACE-001SRC-EVD-001SRC-SEC-001

Thiếu P0 evidence thì giữ:


## 117. FINAL AUDIT LOCK

Audit bắt buộc cho:

Thay đổi master data trọng yếu.Thay đổi Product/Claim.Thay đổi Pricing/Program.Thay đổi Member Benefit.Thay đổi Company Bank Account Registry.Payment instruction generation.Payment state transition.Accounting Review.Shipping state transition.Order state transition.IVR result handling.CRM suppression/frequency config.Gateway production config.Operational state transition.Batch release.Warehouse receipt.Recall/CAPA.Permission/role.Public Trace whitelist.Manual override.

Audit tối thiểu:

Không có audit cho hành động trọng yếu thì không pass Security/Audit Gate.


## 118. FINAL PACK MAPPING — PACK NÀO PHẢI DÙNG SOURCE NÀO


### 118.1. PACK-05 — AI ADVISOR CHANNEL

Bắt buộc dùng:

SRC-PROD-001SRC-CLAIM-001SRC-PRICE-001SRC-PRICE-002SRC-MEMBER-001SRC-CUSTOMER-001SRC-AVAIL-001SRC-PAY-001SRC-SHIP-001SRC-ORDER-001SRC-ORDER-002SRC-CRM-001 nếu liên quan khách cũ/chăm sóc

Không được tự sở hữu:

Giá.Payment.Shipping.Order state.Bank account.Sellable.Claim ngoài whitelist.


### 118.2. PACK-06 — FACEBOOK CHANNEL GATEWAY

SRC-GOV-001SRC-PROD-001SRC-CLAIM-001SRC-PRICE-001 nếu có quote flowSRC-PAY-001 nếu nói paymentSRC-SHIP-001 nếu nói giao hàngSRC-ORDER-002 nếu chốt đơn qua MessengerSRC-CRM-001 nếu handoff CRMSRC-EVD-001SRC-SEC-001

Quote.Payment.Shipping.Order state.Bank account.CRM suppression.


### 118.3. Commerce Runtime — Quote / Order / Payment / Shipping

Bắt buộc sở hữu/triển khai chi tiết:

SRC-PRICE-001SRC-PRICE-002SRC-MEMBER-001 phối hợp PACK-05SRC-AVAIL-001 phối hợp PACK-06SRC-PAY-001SRC-BANK-001SRC-PAY-002SRC-SHIP-001SRC-SHIP-002SRC-ORDER-001SRC-ORDER-002SRC-ORDER-003

Commerce Core phải là trung tâm của:

Quote.Cart.Order Confirmation Draft.Order State Machine.Payment Eligibility.Payment State.Shipping Eligibility.Shipping State.


### 118.4. CRM Runtime — Member Lifecycle

SRC-CUSTOMER-001SRC-MEMBER-001SRC-CRM-001SRC-PROD-001SRC-CLAIM-001SRC-PAY-001 nếu nội dung có paymentSRC-SHIP-001 nếu nội dung có shippingSRC-ORDER-003 nếu nội dung dựa trên order stateSRC-EVD-001SRC-SEC-001

CRM không được gửi nếu:

Suppression unknown.Frequency cap unknown.Quiet hours unknown.Claim not whitelisted.Payment/shipping eligibility missing but message mentions payment/shipping.


### 118.5. PACK-01 — OPERATIONAL CORE

SRC-OPS-001SRC-TRACE-001SRC-RECALL-001SRC-MISA-001 nếu MISA nằm trong release scopeSRC-AVAIL-001 phối hợp Commerce Availability

Operational Core phải cung cấp dữ liệu đúng cho:

Sellable availability.Public Trace.Recall/recovery.Warehouse/inventory.MISA boundary.


### 118.6. Landing Page Runtime — consumer surface

SRC-PROD-001SRC-CLAIM-001SRC-PRICE-001 nếu có giá runtimeSRC-PAY-001 nếu hiển thị paymentSRC-SHIP-001 nếu hiển thị shippingSRC-ORDER-001 hoặc Commerce Entry nếu có mua hàngSRC-ADS-001 nếu trackingSRC-EVD-001

Landing Page không được:

Hardcode final runtime price.Hardcode bank account.Hardcode ETA/phí ship/COD.Tự hứa payment.Nói claim ngoài whitelist.


### 118.7. PACK-08 — ADS / Tracking

SRC-ADS-001SRC-PROD-001SRC-CLAIM-001SRC-ORDER-003 hoặc conversion event nếu tracking đơn hàngSRC-EVD-001

Giá.Payment.Shipping.Order state.Member benefit.Sellable.


### 118.8. PACK-09 — IVR Order Confirmation

SRC-IVR-001SRC-ORDER-003SRC-EVD-001SRC-SEC-001

IVR không được:

Tự tạo order.Tự xác nhận order.Tự hủy order.Tự cập nhật order state.Dùng cho sales/CRM/marketing/tư vấn/chăm sóc đại trà.


### 118.9. Admin Runtime — internal operator surface

SRC-UI-001SRC-SEC-001SRC-EVD-001SRC-ORDER-003SRC-PAY-001SRC-PAY-002SRC-BANK-001 thông qua Payment Core, không hardcodeSRC-SHIP-001SRC-SHIP-002SRC-IVR-001 nếu có IVR monitoringSRC-OPS-001 nếu có operational dashboard

Hardcode business data/state.Hardcode bank account.Tự PAID.Tự đổi shipping/order state.Bypass backend permission.Bypass audit.


### 118.10. PACK-10 — COMPLETION / EVIDENCE / GATEWAY

SRC-EVD-001All source_id liên quan từng evidence item

PACK-10 phải kiểm soát:

Evidence có source_id.Evidence có test_id/smoke_id.Evidence có artifact.Evidence có status.Evidence P0 accepted trước release.Completion Report không pass nếu P0 evidence thiếu.


### 118.11. Security / Audit Control

SRC-SEC-001All protected action source_idSRC-EVD-001

Security / Audit Control phải kiểm soát:

Permission.Audit.Credential.Bank account safety.Public/private data boundary.Manual override.High-risk action.


## 119. MASTER-01 HANDOFF CHECKLIST CHO PACK CHI TIẾT

Khi viết bất kỳ pack chi tiết nào sau MASTER-01, phải điền checklist này:

[ ] Pack này dùng source_id nào?[ ] Pack này có sở hữu Source-of-Truth nào không?[ ] Pack này chỉ consume source nào?[ ] Pack này có bị cấm hardcode dữ liệu nào không?[ ] Pack này có runtime resolver nào bắt buộc không?[ ] Pack này có public-safe rule nào không?[ ] Pack này có fallback rule nào không?[ ] Pack này có conflict rule nào không?[ ] Pack này có evidence requirement nào không?[ ] Pack này có audit requirement nào không?[ ] Pack này có smoke mapping nào không?[ ] Pack này có Done Gate nào không?

Nếu pack chi tiết không trả lời checklist này, pack chưa sẵn sàng triển khai.


## 120. SOURCE REGISTRY TO DEV HANDOFF RULE

Khi giao dev triển khai, không được chỉ nói:

Làm phần payment.Làm phần shipping.Làm phần AI quote.Làm phần IVR.

Phải nói rõ:

Source ID nào?Owner core nào?Consumer nào?API/resolver nào?State machine nào?Fallback nào?Evidence nào?Audit nào?Smoke nào?Hardcode bị cấm ở đâu?Done Gate là gì?

Ví dụ payment handoff đúng:

Triển khai SRC-PAY-001, SRC-BANK-001, SRC-PAY-002.Owner core: Commerce Payment Core.Consumer: AI/Gateway/CRM/Admin UI/Landing Page chỉ consume qua Payment Eligibility/Instruction.No-hardcode: bank account, payment instruction, PAID state.Evidence: Payment Eligibility, Bank Registry, payment_reference, Payment Core/Accounting Review confirmation.Smoke: MAS-SMK-003, MAS-SMK-004, MAS-SMK-005, COM-SMK-PAY-001..008.Done Gate: Payment Done Gate.

Ví dụ shipping handoff đúng:

Triển khai SRC-SHIP-001, SRC-SHIP-002.Owner core: Commerce Shipping Core.Consumer: AI/Gateway/CRM/Landing Page/Admin UI.No-hardcode: ETA, shipping fee, COD, provider status.Evidence: Shipping Eligibility response, fee, COD, fallback ETA, transcript no unsupported promise.Smoke: MAS-SMK-006, COM-SMK-SHIP-001..007.Done Gate: Shipping Done Gate.

Ví dụ IVR handoff đúng:

Triển khai SRC-IVR-001 cùng SRC-ORDER-003.Owner: IVR Confirmation Core + Commerce Order Core.No direct order state update by SIM Gateway.Evidence: Order Core task, IVR queue, DTMF result, technical error, Order Core transition.Smoke: MAS-SMK-009, MAS-SMK-010, IVR-SMK-001..010.Done Gate: IVR Confirmation Done Gate.


## 121. MASTER-01 TO MASTER-02 HANDOFF

MASTER-01 bàn giao cho MASTER-02 nội dung sau:

Source ID registry.Owner core mapping.Consumer boundary.Runtime resolver requirement.No-hardcode lock.Public-safe lock.Evidence requirement.Audit requirement.bị chặn-if-missing rule.Release impact of missing source.

MASTER-02 sẽ dùng các thông tin này để vẽ dependency graph.

Dependency không được vẽ theo cảm tính.Dependency phải vẽ theo Source-of-Truth ownership và consumer boundary.

AI Advisor phụ thuộc Commerce Pricing vì cần SRC-PRICE-001.

Gateway phụ thuộc Payment Core nếu nói payment vì cần SRC-PAY-001.

CRM phụ thuộc Shipping Core nếu nhắc ETA vì cần SRC-SHIP-001.

IVR phụ thuộc Order Core vì cần SRC-ORDER-003.

Landing Page phụ thuộc Claim Policy vì cần SRC-CLAIM-001.

Public Trace phụ thuộc Trace Core vì cần SRC-TRACE-001.


## 122. MASTER-01 TO Commerce Runtime HANDOFF

PACK-04 phải nhận các source sau từ MASTER-01:

SRC-PRICE-001 — Commerce Pricing SourceSRC-PRICE-002 — Program Policy SourceSRC-MEMBER-001 — Member Benefit Source, phối hợp PACK-05SRC-AVAIL-001 — Sellable Availability Source, phối hợp PACK-06SRC-PAY-001 — Payment Eligibility SourceSRC-BANK-001 — Company Bank Account Registry SourceSRC-PAY-002 — Payment State SourceSRC-SHIP-001 — Shipping Eligibility SourceSRC-SHIP-002 — Shipping State SourceSRC-ORDER-001 — Quote Snapshot SourceSRC-ORDER-002 — Order Confirmation Draft SourceSRC-ORDER-003 — Order State Machine Source

PACK-04 phải viết chi tiết:

API.

DTO.

State machine.

Payment instruction.

COD/ETA/fee.

Done Gate.


## 123. MASTER-01 TO PACK-05 AI Advisor HANDOFF

PACK-01 phải nhận source registry sau:

SRC-PROD-001SRC-CLAIM-001SRC-PRICE-001SRC-PRICE-002SRC-MEMBER-001SRC-CUSTOMER-001SRC-CRM-001SRC-AVAIL-001SRC-PAY-001SRC-SHIP-001SRC-ORDER-001SRC-ORDER-002

AI Advisor phải tuân thủ:

Không tự tính giá.Không tự áp quyền lợi member.Không tự bán nếu thiếu availability.Không hứa payment nếu thiếu Payment Eligibility.Không hứa shipping/ETA nếu thiếu Shipping Eligibility.Không hardcode bank account.Không tạo order chính thức trước Customer Confirmation.Không nói claim ngoài whitelist.


## 124. MASTER-01 TO PACK-06 Facebook Channel Gateway HANDOFF

PACK-03 phải nhận source registry sau:

SRC-GOV-001SRC-PROD-001SRC-CLAIM-001SRC-PRICE-001 nếu có quote flowSRC-PAY-001 nếu có payment intentSRC-SHIP-001 nếu có shipping intentSRC-ORDER-002 nếu có order draft flowSRC-CRM-001 nếu có CRM handoffSRC-EVD-001SRC-SEC-001

Gateway phải tuân thủ:

Không public final quote nếu policy không cho.Không hardcode bank account.Không hứa payment nếu Payment Core chưa trả eligibility.Không hứa shipping nếu Shipping Core chưa trả eligibility.Không tự tạo order state.Không tự bypass CRM suppression nếu có CRM flow.


## 125. MASTER-01 TO PACK-09 IVR HANDOFF

PACK-09 phải nhận source registry sau:

PACK-09 phải giữ đúng lock:

PACK-09 không được:

Dùng cho sales.Dùng cho CRM.Dùng cho marketing.Dùng cho tư vấn.Dùng cho chăm sóc đại trà.Tự cập nhật order state.Tính technical error là customer no-answer.Batch calling sau session.


## 126. MASTER-01 TO PACK-10 Evidence HANDOFF

PACK-10 phải nhận toàn bộ source registry để evidence có thể trace về source.

Bắt buộc:

PACK-10 không được:

Accept evidence không trace source.Accept screenshot không gắn test/smoke/source.Pass Completion Report khi còn P0 evidence missing.


## 127. MASTER-01 TO Security / Audit Control HANDOFF

Security / Audit Control phải nhận source registry để xác định protected action.

Bắt buộc kiểm soát:

Source nào cần permission.Source nào cần audit.Source nào có sensitive data.Source nào không được public.Source nào không được hardcode.Source nào cần owner approval khi thay đổi.

Security / Audit Control phải đặc biệt kiểm soát:

Company Bank Account Registry.Payment State / PAID.Order State.Shipping State.IVR Result.Public Trace DTO.Claim Whitelist.Permission/Role.Manual override.


## 128. FINAL MASTER-01 DONE GATE CHECKLIST

MASTER-01 được xem là hoàn tất khi:

[ ] PHẦN 1/4 hoàn tất Source-of-Truth principles.[ ] PHẦN 2/4 hoàn tất Source Registry by Domain.[ ] PHẦN 3/4 hoàn tất Source Validation / Conflict / Fallback / Evidence / Audit.[ ] PHẦN 4/4 hoàn tất Final Done Gate / Pack Mapping / Handoff.[ ] Tất cả source P0 đã có Source ID.[ ] Tất cả source P0 có owner core.[ ] Tất cả source P0 có consumer boundary.[ ] Tất cả source P0 có no-hardcode rule.[ ] Tất cả source P0 có fallback rule.[ ] Tất cả source P0 có evidence rule.[ ] Tất cả source P0 có audit rule nếu có state/action.[ ] Tất cả source P0 có release impact.[ ] Pack mapping đã rõ.[ ] Handoff sang MASTER-02 đã rõ.[ ] Không thay đổi trạng thái release đã khóa trong MASTER-00.


## 129. FINAL MASTER-01 điểm chặn LIST

Các lỗi sau là điểm chặn khi dùng MASTER-01 làm nền triển khai:

Pack chi tiết không khai báo source_id.Pack chi tiết dùng source không có owner.AI/Gateway/CRM tự dùng dữ liệu không qua resolver.Payment flow thiếu SRC-PAY-001.Bank transfer thiếu SRC-BANK-001.Payment state thiếu SRC-PAY-002.Shipping flow thiếu SRC-SHIP-001.Order flow thiếu SRC-ORDER-002 hoặc SRC-ORDER-003.IVR flow thiếu SRC-IVR-001 hoặc SRC-ORDER-003.Public content thiếu SRC-CLAIM-001.Public trace thiếu SRC-TRACE-001.Admin protected action thiếu SRC-SEC-001.Completion Report thiếu SRC-EVD-001.Evidence không gắn source_id.Hardcode dữ liệu P0 ở consumer.

Nếu có điểm chặn P0:


## 130. FINAL MASTER-01 RELEASE IMPACT

MASTER-01 không tự release hệ thống.

MASTER-01 chỉ xác định tác động release nếu source fail.

Quy tắc:

Trạng thái toàn hệ tiếp tục:


## 131. FINAL MASTER-01 LOCK

MASTER-01 khóa các điểm sau:

MASTER-01 = GLOBAL SOURCE-OF-TRUTH REGISTRY.Source ID là bắt buộc cho dữ liệu trọng yếu.Owner core là bắt buộc.Consumer boundary là bắt buộc.Runtime resolver là bắt buộc với dữ liệu quyết định tại thời điểm chạy.No-hardcode rule là bắt buộc.Public-safe rule là bắt buộc với dữ liệu ra khách hàng/public.Evidence rule là bắt buộc với P0/release.Audit rule là bắt buộc với state/action trọng yếu.Fallback không được suy đoán.Conflict phải xử lý theo authority order.Pack chi tiết không được mâu thuẫn MASTER-01.


## 132. FINAL MASTER-01 CONCLUSION — GOVERNANCE

MASTER-01 đã hoàn thành vai trò đăng ký Source-of-Truth toàn hệ.

MASTER-01 xác định rõ:

Dữ liệu nào thuộc governance.Dữ liệu nào thuộc product.Dữ liệu nào thuộc claim.Dữ liệu nào thuộc pricing.Dữ liệu nào thuộc member.Dữ liệu nào thuộc customer.Dữ liệu nào thuộc CRM.Dữ liệu nào thuộc availability.Dữ liệu nào thuộc payment.Dữ liệu nào thuộc bank registry.Dữ liệu nào thuộc shipping.Dữ liệu nào thuộc order.Dữ liệu nào thuộc IVR.Dữ liệu nào thuộc operational.Dữ liệu nào thuộc trace.Dữ liệu nào thuộc recall.Dữ liệu nào thuộc MISA.Dữ liệu nào thuộc ADS.Dữ liệu nào thuộc Admin UI.Dữ liệu nào thuộc evidence.Dữ liệu nào thuộc security/audit.


## 133. FINAL MASTER-01 CONCLUSION — PAYMENT

Payment Source-of-Truth được khóa:

SRC-PAY-001 = Payment Eligibility Source.SRC-BANK-001 = Company Bank Account Registry Source.SRC-PAY-002 = Payment State Source.

Chiến lược payment vẫn là:

Rule khóa:

Không hứa payment nếu Payment Core chưa trả eligibility.Không hiển thị bank account nếu không qua Company Bank Account Registry.Không tạo bank transfer order nếu thiếu payment_reference / transfer memo.Không PAID nếu thiếu Payment Core confirmation hoặc Accounting Review confirmation.Không hardcode bank account ở AI/Gateway/CRM/Admin UI/frontend/static template.


## 134. FINAL MASTER-01 CONCLUSION — SHIPPING

Shipping Source-of-Truth được khóa:

SRC-SHIP-001 = Shipping Eligibility Source.SRC-SHIP-002 = Shipping State Source.

Scope hiện tại:

Không hứa giao hàng nếu thiếu Shipping Eligibility.Không hứa ETA nếu thiếu Shipping Eligibility.Không hứa COD nếu thiếu Shipping Eligibility.Không hứa phí ship nếu thiếu Shipping Eligibility.Không hardcode ETA/phí ship/COD ở AI/Gateway/CRM/Landing Page/Admin UI.

Phần ngoài phạm vi trong nước chỉ là:


## 135. FINAL MASTER-01 CONCLUSION — ORDER / IVR

Order Source-of-Truth được khóa:

SRC-ORDER-001 = Quote Snapshot Source.SRC-ORDER-002 = Order Confirmation Draft Source.SRC-ORDER-003 = Order State Machine Source.

IVR Source-of-Truth được khóa:

SRC-IVR-001 = IVR Order Confirmation Source.

AI/Gateway/CRM không tạo order chính thức trước Customer Confirmation.Order state chỉ qua Commerce Order Core.IVR chỉ tạo result event.Order Core mới quyết định trạng thái đơn.SIM Gateway không cập nhật order state trực tiếp.Technical error không phải customer no-answer.IVR không dùng cho sales/CRM/marketing/tư vấn/chăm sóc đại trà.


## 136. FINAL MASTER-01 CONCLUSION — PRODUCT / CLAIM / PUBLIC

Product/Claim Source-of-Truth được khóa:

SRC-PROD-001 = Product Knowledge Master.SRC-CLAIM-001 = Claim Whitelist Source.SRC-TRACE-001 = Public Trace Source.

Không tự đặt tên sản phẩm.Không tự tạo SKU.Không nói claim ngoài whitelist.Không public trace nếu thiếu Public Trace DTO.Không dùng Admin/Internal Trace DTO cho public.Không public dữ liệu supplier/personnel/cost/accounting/QC defect/internal root cause/formula nội bộ.


## 137. FINAL MASTER-01 CONCLUSION — EVIDENCE / SECURITY

Evidence/Security Source-of-Truth được khóa:

SRC-EVD-001 = Evidence Registry Source.SRC-SEC-001 = Security / Permission / Audit Source.

Kết quả ở cấp MASTER-01:

MASTER-01_SOURCE_REGISTRY = COMPLETESOURCE_AUTHORITY_MODEL = COMPLETESOURCE_CONSUMER_BOUNDARY = COMPLETESOURCE_RUNTIME_RESOLVER_RULE = COMPLETESOURCE_NO_HARDCODE_RULE = COMPLETESOURCE_PUBLIC_SAFE_RULE = COMPLETESOURCE_EVIDENCE_RULE = COMPLETESOURCE_AUDIT_RULE = COMPLETESOURCE_FALLBACK_RULE = COMPLETESOURCE_CONFLICT_RULE = COMPLETEPACK_HANDOFF_MAPPING = COMPLETE

Nhưng trạng thái toàn hệ vẫn là:


## 139. MASTER-01 FINAL DONE GATE RESULT

Điều này có nghĩa:

MASTER-01 đã đủ vai trò Source-of-Truth Registry.

Các pack chi tiết phải dùng Source ID khi viết rule/API/DTO/test/evidence.

Không pack nào được tự tạo nguồn dữ liệu riêng trái registry.

Không consumer nào được hardcode dữ liệu P0.

Không dữ liệu public nào được dùng nếu thiếu public-safe source.

Không release nếu thiếu evidence P0.

Không go-live nếu thiếu sign-off/gate như MASTER-00 đã khóa.


## 140. HANDOFF SANG MASTER-02

Từ MASTER-01, tài liệu tiếp theo là:

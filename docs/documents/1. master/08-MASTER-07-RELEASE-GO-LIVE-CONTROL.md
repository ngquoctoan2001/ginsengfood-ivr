# MASTER-07 - RELEASE GO LIVE CONTROL

## Mục Đích

MASTER-07 khóa cơ chế xét release và go-live cho Ginsengfood. Tài liệu này phân biệt rõ tài liệu hoàn chỉnh, implementation hoàn chỉnh, smoke đạt, evidence accepted, release approval và go-live approval.

MASTER-07 không tự cấp quyền vận hành thật. File này chỉ định nghĩa điều kiện để owner hoặc release authority review.

## AI Review Boundary

Khi reviewer AI chấm bộ tài liệu Phase 1-8 để giao dev triển khai, MASTER-07 được dùng để kiểm tra tài liệu có phân biệt đúng dev handoff, release và go-live hay không. `NOT_RELEASE_READY` hoặc `NOT_PRODUCTION_READY` là trạng thái guard hợp lệ trong vòng tài liệu; không được coi là lý do phủ định `DEV_IMPLEMENTATION_DOCUMENTATION_READY` nếu tài liệu đã đủ để dev bóc task và biết release gate sau triển khai.

## Nguyên Tắc Release

1. Documentation ready không đồng nghĩa production ready.
2. Implementation done không đồng nghĩa release approved.
3. Smoke pass cục bộ không đồng nghĩa global pass.
4. Evidence accepted là điều kiện bắt buộc để review.
5. P0 chưa xử lý thì không mở release.
6. Rollback và monitoring phải có trước go-live.
7. Owner sign-off phải đúng scope.

## Release Gate Registry

| Gate | Domain | Owner source |
| --- | --- | --- |
| REL-GOV | Governance | MASTER |
| REL-OPR | Operational Core | PACK-01 |
| REL-PRD | Product / SKU / Recipe | PACK-02 |
| REL-MRP | Demand / MRP / Material | PACK-03 |
| REL-MISA | MISA / Accounting Handoff | PACK-04 |
| REL-AI | AI Advisor | PACK-05 |
| REL-GTW | Facebook Gateway | PACK-06 |
| REL-ADS | Ads / Attribution | PACK-07 |
| REL-LIVE | MC AI Live | PACK-08 |
| REL-IVR | IVR Order Confirmation | PACK-09 |
| REL-EVD | Completion / Evidence | PACK-10 |
| REL-SEC | Security / Permission / Audit | Security Governance |

## Production Readiness Model

Production readiness chỉ được review khi:

1. Scope release rõ.
2. Owner core của scope đã ký.
3. Source-of-Truth đủ.
4. Dependency đủ.
5. Resolver và guard chạy được.
6. Security, permission và audit đạt yêu cầu.
7. Smoke đủ tầng.
8. Evidence accepted.
9. Rollback plan có thể thực hiện.
10. Monitoring đủ để phát hiện lỗi sau release.

## Go-Live Boundary

Go-live chỉ xét sau release approval và chỉ trong scope đã được duyệt. Go-live không được mở toàn hệ nếu chỉ một domain nhỏ đạt điều kiện.

Go-live phải bị chặn khi:

1. Recall / Sale Lock chưa được downstream tôn trọng.
2. Payment hoặc shipping chưa có source hợp lệ cho scope bán hàng.
3. Inventory không trace được về released batch.
4. AI hoặc Gateway có thể nói sai giá, tồn, claim, payment hoặc shipping.
5. Ads scale dựa trên revenue chưa verified.
6. IVR có thể cập nhật order trực tiếp ngoài state machine.
7. Evidence hoặc rollback thiếu.

## E2E Release Smoke Tối Thiểu

| Smoke | Chuỗi kiểm |
| --- | --- |
| E2E-OPR-SELLABLE | Product -> recipe -> batch -> release -> warehouse -> inventory -> sellable |
| E2E-RECALL-LOCK | Recall / Sale Lock -> AI/Gateway/Ads/CRM/Live/IVR suppression |
| E2E-MISA | Operational checkpoint -> MISA handoff -> sync/reconcile |
| E2E-AI | Product public-safe -> sellable -> quote-safe response |
| E2E-GATEWAY | Public comment -> private handoff -> policy guard |
| E2E-ADS | Campaign signal -> verified revenue -> scale gate |
| E2E-LIVE | Live board -> script brief -> sale lock guard |
| E2E-IVR | Eligible order -> call job -> call result -> order state machine |
| E2E-EVIDENCE | Smoke output -> evidence packet -> reviewer decision |

## Release Decision Record

| Field | Required |
| --- | --- |
| Release scope | Có |
| Included packs | Có |
| Excluded packs | Có |
| Owner sign-off | Có |
| Evidence packet | Có |
| Smoke result | Có |
| Security review | Có |
| Rollback plan | Có |
| Monitoring plan | Có |
| Decision value | Có |
| Decision time | Có |
| Decision owner | Có |

## Rollback Control

Mỗi release scope phải có rollback:

1. Trigger rollback.
2. Owner được quyền rollback.
3. Phạm vi rollback.
4. Dữ liệu cần giữ nguyên.
5. Dữ liệu được phép revert.
6. Cách thông báo downstream.
7. Evidence sau rollback.

## Incident Control

Sau go-live, incident phải được phân loại theo severity. Các lỗi ảnh hưởng payment, inventory, recall, sale lock, public claim, privacy hoặc order state được xem là nhóm cần xử lý ưu tiên cao.

## No-Hardcode Release Control

Release không đạt nếu còn hardcode:

1. Product, SKU, ingredient, recipe.
2. Price, discount, member benefit.
3. Warehouse, location, inventory.
4. Payment account, shipping fee, ETA.
5. AI claim hoặc script public.
6. Channel route hoặc app permission.
7. MISA mapping.
8. Evidence result.

## Done Gate Của MASTER-07

MASTER-07 đạt yêu cầu khi:

1. Release gate registry rõ.
2. Production readiness model rõ.
3. Go-live boundary rõ.
4. E2E smoke tối thiểu rõ.
5. Release decision record rõ.
6. Rollback và incident control rõ.
7. Không có câu chữ khiến tài liệu governance bị hiểu là phê duyệt vận hành thật.

## Chi Tiết Governance Tương Ứng

Phần này giữ lại phạm vi chi tiết từ nguồn MASTER theo cụm cùng chủ đề, đồng thời chuẩn hóa theo registry PACK hiện hành và loại bỏ các câu trạng thái tổng không dùng trong bộ sạch.


## Master-07 — Global Release Governance / Production Readiness / Go-Live Control

OPERATIONAL RELEASE / RECALL CONTROL / ADS SCALE GATE / MC AI LIVE CONTROL / GATEWAY-GO-LIVE STANDARD


## PHẦN 1/4 — GLOBAL RELEASE PRINCIPLES / PRODUCTION READINESS MODEL / GO-LIVE BOUNDARY / OWNER AUTHORITY


## 1. MASTER-07 DOCUMENT CONTROL


### 1.1. Document Purpose

MASTER-07 là tài liệu quản trị phát hành toàn cục của hệ thống Ginsengfood.

Tài liệu này dùng để khóa nguyên tắc xác định:

Khi nào một domain được xem là đủ điều kiện Production Ready.

Khi nào một release được xem là Release Approved.

Khi nào hệ thống được phép Go-live thật.

Khi nào một pack, module, kênh, chiến dịch, chương trình hoặc extension phải bị bị chặn.

Khi nào phải Stop Sale, Rollback, Suppress, Handoff hoặc Owner Review.

Cách tổng hợp evidence, smoke, security, payment, shipping, CRM, order, AI, IVR, ADS, MC AI, Operational Core, Production, Warehouse, Inventory, Traceability, Recall, Sale Lock và future extension trước khi release.

MASTER-07 không thay thế các MASTER trước đó. MASTER-07 là lớp tổng hợp cuối để xét release thật trên toàn hệ thống.


### 1.2. Governance Position

MASTER-07 đứng sau các tài liệu governance nền:

MASTER-00 — Master Technical Documentation Index.

MASTER-01 — Global Source-of-Truth Registry.

MASTER-02 — Cross-Pack Dependency Map.

MASTER-03 — Traceability ID Standard.

MASTER-04 — Runtime Resolution & Guard Control Standard.

MASTER-05 — Evidence / Smoke / Completion Gate Standard.

MASTER-06 — Reserved Pack / Future Integration Governance.

MASTER-07 sử dụng các tài liệu trên làm nền để quyết định:

Production Ready.

Release Approved.

Gateway Release.

ADS Scale.

MC AI Live Readiness.

IVR Order Confirmation Readiness.

Go-live Approved.

Rollback / Stop / Suppression.


### 1.3. MASTER-07 Is Governance, Not Implementation

MASTER-07 không phải tài liệu triển khai kỹ thuật chi tiết.

MASTER-07 không viết:

Code.

API chi tiết.

DTO chi tiết.

Database schema chi tiết.

UI screen chi tiết.

Worker job chi tiết.

Prompt runtime chi tiết.

Script bán hàng chi tiết.

Payment provider integration chi tiết.

Shipping provider integration chi tiết.

ADS campaign setup chi tiết.

Các nội dung đó thuộc về PACK hoặc implementation document tương ứng.

MASTER-07 chỉ khóa:

Điều kiện release.

Quyền quyết định.

Gate bắt buộc.

Evidence bắt buộc.

Smoke bắt buộc.

Owner sign-off bắt buộc.

Boundary không được vượt.

Trạng thái bị chặn / READY / APPROVED / GO-LIVE.


## 2. GLOBAL RELEASE PRINCIPLES


### 2.1. Principle 01 — Governance PASS Does Not Mean Production Ready

Một tài liệu MASTER hoặc PACK được viết xong, rà soát xong hoặc governance PASS không đồng nghĩa hệ thống đã Production Ready.

Production Ready chỉ được xem xét khi có đủ:

Implementation evidence.

Runtime resolver evidence.

Guard evidence.

Smoke evidence.

Negative path evidence.

Security evidence nếu thuộc scope.

Payment evidence nếu thuộc scope.

Shipping evidence nếu thuộc scope.

Traceability evidence nếu thuộc scope.

Recall / Stop Sale evidence nếu thuộc scope.

Owner sign-off.

Completion Gate PASS.

Không được gọi Production Ready chỉ vì tài liệu đã hoàn thành.


### 2.2. Principle 02 — Production Ready Does Not Mean Release Approved

Production Ready là trạng thái kỹ thuật cho biết một domain, pack hoặc hệ thống đã có đủ điều kiện vận hành trong môi trường production-like hoặc production-controlled.

Release Approved là quyết định của Owner sau khi xem xét:

Evidence đã ACCEPTED.

Smoke đã PASS.

Security không còn điểm chặn.

Operational risk được kiểm soát.

Business risk được chấp nhận.

Rollback path sẵn sàng.

Monitoring / audit / trace sẵn sàng.

Owner sign-off hợp lệ.

Production Ready không tự động mở release.


### 2.3. Principle 03 — Release Approved Does Not Mean Go-live Approved

Release Approved chỉ xác nhận một phiên bản, pack, domain hoặc capability được phép đưa vào kế hoạch phát hành.

Go-live Approved chỉ được cấp khi:

Release đã approved.

Production environment sẵn sàng.

Gateway / channel readiness sẵn sàng nếu có kênh bán hàng.

Payment readiness sẵn sàng nếu có thu tiền.

Shipping readiness sẵn sàng nếu có giao hàng.

Order / CustomerConfirmation readiness sẵn sàng nếu có chốt đơn.

Traceability / Recall / Sale Lock sẵn sàng nếu có mở bán sản phẩm.

Monitoring và rollback sẵn sàng.

Owner phê duyệt go-live cuối.

Không được dùng Release Approved thay cho Go-live Approved.


### 2.4. Principle 04 — Completion Report Does Not Replace Completion Gate

Completion Report chỉ là báo cáo.

Completion Gate là trạng thái kiểm soát release có evidence, smoke, audit và owner sign-off.

Không được gọi Completion Gate PASS.

Không được gọi Gateway PASS.

Không được gọi Production Ready.

Không được gọi Release Approved.

Không được gọi Go-live Approved.

Completion Report không có quyền tự mở Gateway.


### 2.5. Principle 05 — Evidence Accepted Is Mandatory

Chỉ evidence có trạng thái ACCEPTED mới được dùng để xét PASS.

Các trạng thái sau không được dùng để gọi PASS:

DRAFT.

SUBMITTED.

UNDER_REVIEW.

REJECTED.

VOID.

SUPERSEDED.

MISSING.

EXPIRED.

STALE.

CONFLICT.

Không evidence thì không PASS.


### 2.6. Principle 06 — Runtime Resolver Before Runtime Action

Mọi hành động runtime P0 phải đi qua resolver tương ứng.

Consumer không được tự suy luận:

Giá.

Quyền lợi thành viên.

Tồn kho.

Sellable status.

Payment status.

Shipping ETA.

Order status.

Customer identity.

Diamond attribution.

Official contact.

Traceability status.

Recall status.

Sale lock status.

ADS scale eligibility.

MC AI live script eligibility.

IVR confirmation eligibility.

Không resolver thì không action.


### 2.7. Principle 07 — Guard Before Release

Mọi domain muốn release phải có guard tương ứng.

Guard quyết định:

PASS.

BLOCK.

SUPPRESS.

FALLBACK.

HANDOFF.

REVIEW_REQUIRED.

ESCALATE.

NO_ACTION.

Nếu guard chưa tồn tại, chưa test hoặc chưa có evidence thì domain đó không được release.


### 2.8. Principle 08 — Traceability Before Approval

Mọi release decision phải trace được:

source_id.

dependency_id.

domain_id.

smoke_id.

evidence_id.

completion_gate_id.

owner_signoff_id.

correlation_id.

audit_id.

Không trace được thì không được approve.


### 2.9. Principle 09 — Operational Core Controls Product Truth

Các kênh bán hàng, AI, CRM, ADS, MC AI, Gateway, Landing Page và IVR không được quyết định sự thật sản phẩm.

Sự thật sản phẩm phải đi qua Operational Core và các domain liên quan:

Production.

Warehouse.

Inventory.

Product Activation.

Sellable.

Traceability.

Recall.

Sale Lock.

Stop Sale.

Nếu Operational Core BLOCK thì toàn bộ Consumer phải BLOCK hoặc SUPPRESS theo guard.


### 2.10. Principle 10 — Recall / Sale Lock / Stop Sale Wins Over Sales

Recall, Hold, Sale Lock và Stop Sale luôn thắng:

ADS.

CRM.

AI Advisor.

MC AI.

Gateway.

Landing Page.

Order.

Quote.

IVR.

Promotion.

Member benefit.

Diamond benefit.

Revenue target.

Khi Recall / Hold / Sale Lock / Stop Sale được kích hoạt, mọi Consumer phải dừng bán SKU hoặc lô liên quan theo đúng scope lock.


## 3. GLOBAL RELEASE SCOPE


### 3.1. Scope Overview

MASTER-07 áp dụng cho toàn bộ hệ thống Ginsengfood khi xét release thật.

Phạm vi bao gồm:

MASTER governance layer.

PACK documentation layer.

Operational Core.

Product Activation / Sellable.

Sale Lock / Stop Sale.

ADS / Attribution / ROAS / CPA / AOV / Scale Gate.

MC AI / Live Board / Script Brief.

Security.

Payment.

Shipping.

Order / Quote / CustomerConfirmation.

IVR ORDER_CONFIRMATION_ONLY.

Future Extension.


### 3.2. Operational Core Scope

Operational Core là nguồn kiểm soát các trạng thái vận hành thật của sản phẩm.

Operational Core release phải chứng minh được:

Sản phẩm được sản xuất hợp lệ.

Thành phẩm được nhập kho hợp lệ.

Tồn kho khả dụng được xác định đúng.

SKU được phép mở bán.

Giá được active.

Không bị quality hold.

Không bị recall hold.

Không bị sale lock.

Không bị channel block.

Traceability đủ điều kiện public theo whitelist.

Stop Sale hoạt động khi điều kiện vi phạm.

Operational Core không được bị Consumer bypass.


### 3.3. Production Scope

Production release phải kiểm soát:

Production order.

Recipe snapshot.

Batch.

Material issue.

Material receipt.

QC production.

Batch release.

Production status.

Production planning signal.

Owner review trước khi chuyển signal thành production action.

Sales / Stock Movement có thể tạo tín hiệu kế hoạch sản xuất nhưng không được tự động biến thành Production Order nếu chưa qua Operational Core / Owner Review.


### 3.4. Warehouse / Inventory Scope

Warehouse / Inventory release phải kiểm soát:

Warehouse receipt.

Finished goods stock.

Stock available.

Stock reserved.

Stock bị chặn.

Inventory ledger.

Lot balance.

Channel sellable quantity.

Stock movement audit.

Stock guard cho các kênh bán hàng.

AI, ADS, CRM, MC AI, Gateway và Landing Page không được tự nói hàng còn nếu Sellable / Stock Guard không PASS.


### 3.5. Product Activation / Sellable Scope

Product Activation / Sellable là gate bắt buộc trước khi sản phẩm được bán.

Base Sellable Gate gồm:

Thành phẩm hoàn tất sản xuất.

Nhập kho hợp lệ.

stock_available > 0.

sellable_status = SELLABLE.

listed_price_status = ACTIVE.

Không quality hold.

Không recall hold.

Không sale lock.

Không channel block.

Nếu một điều kiện không đạt thì Consumer phải BLOCK hoặc SUPPRESS hành vi bán hàng.


### 3.6. Traceability / Recall / Sale Lock Scope

Traceability / Recall / Sale Lock là domain bảo vệ an toàn vận hành và uy tín thương hiệu.

Release phải chứng minh:

Traceability link đầy đủ.

Public trace chỉ hiển thị whitelist.

Recall impact analysis hoạt động.

Hold áp dụng đúng scope.

Sale Lock áp dụng đúng SKU/lô/kênh.

Stop Sale kích hoạt đúng điều kiện.

Consumer nhận được trạng thái lock.

AI/CRM/ADS/MC AI/Gateway dừng bán khi bị lock.

Audit đầy đủ.

Evidence đầy đủ.

Recall / Stop Sale luôn thắng sales.


### 3.7. ADS / Attribution / Scale Gate Scope

ADS release không chỉ là chạy quảng cáo.

ADS release phải có gate riêng cho:

Ads Spend.

Revenue Verified.

ROAS.

CPA.

AOV.

Boxes/Order.

Comment Rate.

Inbox Rate.

Quote Rate.

Order Rate.

Verified Rate.

COD Fail.

CRM Revenue.

Diamond Revenue.

Data quality.

Pilot 7–14 ngày.

Owner approval trước scale.

ADS không được tự scale khi thiếu verified revenue hoặc evidence.

ADS không được đẩy SKU không active, không sellable hoặc bị Sale Lock.

ADS không được tự quyết định Diamond attribution khi có conflict.


### 3.8. MC AI / Live Board / Script Brief Scope

MC AI là lớp hỗ trợ nội dung live, không phải Owner Core.

MC AI release phải có gate riêng cho:

Daily Live Product Board.

mc_ai_script_brief_id hợp lệ.

SKU được phép nói trong phiên live.

SKU hero / SKU hỗ trợ / SKU không được nói.

Program runtime.

QuoteSnapshot boundary.

Sellable / Stock Guard.

Recall / Hold / Sale Lock Guard.

Fake urgency guard.

Live moderation boundary.

Audit và evidence.

MC AI chỉ được nói SKU nằm trong Daily Live Product Board hợp lệ.

MC AI không được nói giá nếu không có QuoteSnapshot / Program Runtime hợp lệ.

MC AI không được tạo fake urgency.

MC AI phải dừng nói/bán SKU nếu Recall / Hold / Sale Lock xảy ra.


### 3.9. Gateway Scope

Gateway release phải kiểm soát:

Public comment.

Messenger handoff.

Handoff delivery log.

PII protection.

Health detail suppression.

Payment instruction suppression.

Public price suppression nếu policy yêu cầu.

Moderation.

Abuse / troll / spam handling.

Complaint routing.

App review.

Webhook evidence.

Gateway smoke.

Completion Gate.


### 3.10. Security Scope

Security release phải kiểm soát:

Authentication.

Authorization.

Permission.

Secret management.

Webhook verification.

HMAC callback nếu có.

Audit log.

Payment data protection.

Evidence storage policy.

Role boundary.

Admin access.

External integration access.

Abuse handling.

Security smoke.

Negative security evidence.

Không security evidence thì không Production Ready trong scope có security.


### 3.11. Payment Scope

Payment release phải kiểm soát:

VNPAY.

MOMO_BUSINESS.

DIRECT_BANK_TRANSFER_TO_SSAVIGROUP.

Company Bank Account Registry.

payment_reference.

transfer memo.

Bank transfer queue.

Accounting Review.

Payment Core.

No hardcoded bank account.

No PAID without Payment Core / Accounting Review confirmation.

AI, Gateway, CRM, Admin UI, frontend và template không được hardcode tài khoản ngân hàng.

PaymentReference không đồng nghĩa PAID.


### 3.12. Shipping Scope

Shipping release phải kiểm soát:

Domestic shipping mặc định.

Shipping eligibility.

Shipping fee.

COD eligibility.

ETA.

Tracking.

Delivery status.

Failed delivery.

Return / recovery nếu có.

Shipping Core evidence.

MAS-SMK-006 mapping.

Owner decision nếu mở international shipping.

International payment/shipping là future governed extension, không được mặc định active.


### 3.13. CRM Scope

CRM release phải kiểm soát:

Customer memory.

CRM trigger.

Suppression.

12-month journey.

Member lifecycle.

Grace period.

Complaint suppression.

Privacy suppression.

Duplicate message guard.

Message job trace.

Revenue attribution nếu có.

CRM smoke.

CRM không được spam.

CRM không được gửi khi thiếu trigger hoặc đang bị suppression.


### 3.14. Order / Quote / CustomerConfirmation Scope

Order release phải kiểm soát:

QuoteSnapshot.

QuoteCart.

OrderConfirmationDraft.

CustomerConfirmation.

OrderCode.

PaymentReference.

VAT display.

Diamond benefit nếu eligible.

Price freeze window.

Order audit.

Duplicate order guard.

IVR handoff nếu có.

Không QuoteSnapshot thì không báo giá.

Không CustomerConfirmation thì không tạo OrderCode.

Không OrderCode thì không được nói “đã đặt hàng thành công”.


### 3.15. AI Advisor Scope

AI Advisor release phải kiểm soát:

Product knowledge.

Claim whitelist.

Brand soul.

Customer context.

Product recommendation.

Segment recommendation.

Price/runtime resolver.

Member resolver.

Diamond resolver.

Health boundary.

Complaint boundary.

Public/Messenger boundary.

Quote boundary.

Order boundary.

Payment boundary.

Shipping boundary.

Stop Sale boundary.

Handoff boundary.

Test matrix evidence.

AI Advisor không được tự suy luận runtime data.

AI Advisor không được bán SKU không active, không sellable hoặc đang bị hold/recall/sale lock.


### 3.16. IVR ORDER_CONFIRMATION_ONLY Scope

IVR release chỉ áp dụng cho PACK-09 IVR_ORDER_CONFIRMATION.

IVR scope bị khóa:

ORDER_CONFIRMATION_ONLY.


## Internal_Sim_Gateway_Server.

Không bán hàng.

Không upsell.

Không tư vấn sản phẩm.

Không xác nhận payment.

Không tự đổi order state ngoài phạm vi xác nhận đơn.

Chỉ gọi khi có OrderConfirmationDraft / OrderCode đủ điều kiện theo policy.

Có call log.

Có confirmation result.

Có retry policy.

Có audit.

Có evidence.

Có owner sign-off.

IVR không được mở rộng sang sales/CRM/marketing nếu chưa qua future governed extension.


### 3.17. Future Extension Scope

Future Extension mặc định bị chặn.

Future Extension chỉ được runtime khi đạt ACTIVE_GOVERNED_EXTENSION.

Mỗi future extension phải có:

Integration Intake.

Impact Classification.

Owner Core.

Source-of-Truth.

Resolver.

Guard.

Traceability ID.

Audit.

Evidence.

Smoke.

Release Gate PASS.

Go-live approval nếu đưa vào production.

Future extension không được tự ảnh hưởng runtime hiện tại.


## 4. PRODUCTION READINESS MODEL


### 4.1. Production Readiness Definition

Production Ready là trạng thái xác nhận một domain, pack, module hoặc release candidate đã đủ điều kiện kỹ thuật và vận hành để chạy trong production-controlled environment.

Production Ready không phải là quyền Go-live.

Production Ready chỉ là điều kiện cần để xét Release Approved.


### 4.2. Production Readiness Minimum Requirements

Một domain chỉ được xem xét Production Ready khi đạt đủ:

Source-of-Truth rõ.

Owner Core rõ.

Runtime Resolver hoạt động.

Guard hoạt động.

Traceability ID đầy đủ.

Evidence ACCEPTED.

Smoke PASS.

Negative path PASS.

Security evidence PASS nếu thuộc scope.

E2E smoke PASS nếu domain thuộc luồng trọng yếu.

No hardcode runtime data.

No unresolved P0 điểm chặn.

Owner sign-off kỹ thuật hoặc nghiệp vụ theo scope.


### 4.3. Production Readiness Status

Trạng thái chuẩn của Production Readiness:

Status

Ý nghĩa

Được release không

NOT_ASSESSED

Chưa đánh giá

Không

bị chặn

Có điểm chặn

Thiếu evidence

Thiếu smoke

WAITING_SECURITY

Thiếu security evidence

WAITING_OWNER_REVIEW

Chờ owner review

Đủ điều kiện kỹ thuật sơ bộ

Chưa

Đủ điều kiện production readiness

Chưa tự release

REJECTED

Không đạt

SUPERSEDED

Bị thay thế


### 4.4. Production Readiness điểm chặn

Các lỗi sau là điểm chặn:

Thiếu Source-of-Truth.

Thiếu Owner Core.

Thiếu Runtime Resolver.

Thiếu Guard.

Thiếu Evidence ACCEPTED.

Thiếu smoke.

Thiếu negative path.

Thiếu security evidence trong scope security.

Hardcode runtime data.

Consumer tự suy luận rule kinh doanh.

Không trace được P0 ID.

Không audit được action P0.

Payment tự PAID không qua Payment Core / Accounting Review.

Shipping tự bịa ETA/tracking/phí ship.

AI tự báo giá không có QuoteSnapshot.

Order tự tạo không có CustomerConfirmation.

ADS tự scale không có verified revenue.

MC AI nói SKU ngoài board.

Sản phẩm bị hold/recall/sale lock nhưng vẫn bán.

Future extension tự active khi chưa approved.

Owner sign-off thiếu hoặc không trace được.


## 5. GO-LIVE BOUNDARY


### 5.1. Go-live Definition

Go-live là trạng thái hệ thống, domain, channel, campaign hoặc capability được phép vận hành thật với khách hàng, dữ liệu thật, giao dịch thật hoặc ảnh hưởng vận hành thật.

Go-live chỉ được cấp khi:

Production Ready đạt.

Release Approved đạt.

Go-live owner sign-off đạt.

Monitoring sẵn sàng.

Rollback sẵn sàng.

Support / incident process sẵn sàng.

Evidence package đầy đủ.


### 5.2. Go-live Is Not Allowed When

Không được Go-live nếu:

Completion Gate chưa PASS.

Gateway còn bị chặn.

Security evidence chưa PASS.

Payment chưa có evidence nếu có thu tiền.

Shipping chưa có evidence nếu có giao hàng.

Order chưa có CustomerConfirmation gate.

ProductActivation / Sellable chưa PASS.

Recall / Stop Sale chưa có negative path evidence.

ADS chưa có Scale Gate approval.

MC AI chưa có Live Board / Script Brief gate.

IVR chưa chứng minh ORDER_CONFIRMATION_ONLY.

Future extension chưa ACTIVE_GOVERNED_EXTENSION.

Owner chưa sign-off.

Rollback chưa sẵn sàng.

Monitoring chưa sẵn sàng.


### 5.3. Go-live Boundary by Consumer

Consumer chỉ được Go-live trong boundary được cấp.

Consumer

Boundary bắt buộc

AI Advisor

Tư vấn, quote, order draft theo resolver/guard

Gateway

Public comment, Messenger handoff, moderation theo policy

CRM

Message trigger theo suppression và customer context

ADS

Chạy và scale theo ADS Scale Gate

MC AI

Nói theo Live Board và Script Brief hợp lệ

Landing Page

Hiển thị theo product activation, sellable, price và guard

IVR

ORDER_CONFIRMATION_ONLY

Admin UI

Thao tác theo permission, audit và workflow

Payment

Theo Payment Core / Accounting Review

Shipping

Theo Shipping Core

Future Extension

Chỉ theo ACTIVE_GOVERNED_EXTENSION

Consumer không được mở rộng boundary bằng cấu hình cục bộ.


## 6. OWNER AUTHORITY


### 6.1. Owner Authority Principle

Mỗi quyết định release phải có Owner có thẩm quyền.

Không có Owner thì không có release.

Owner chịu trách nhiệm:

Phê duyệt nghiệp vụ.

Chấp nhận rủi ro.

Xác nhận evidence.

Ký release.

Ký go-live.

Kích hoạt rollback nếu cần.

Quyết định mở rộng hoặc dừng scope.


### 6.2. Owner Authority Levels

Level

Vai trò

Quyền

Domain Owner

Chủ nghiệp vụ từng domain

Xác nhận domain readiness

Technical Owner

Chủ kỹ thuật

Xác nhận implementation/security/smoke

Evidence Owner

Chủ evidence

Nộp và chịu trách nhiệm evidence

Release Owner

Chủ release

Tổng hợp release decision

Business Owner

Chủ kinh doanh

Chấp nhận business risk

Security Owner

Chủ bảo mật

Xác nhận security readiness

Payment Owner

Chủ payment/accounting

Xác nhận payment readiness

Shipping Owner

Chủ shipping

Xác nhận shipping readiness

Operational Owner

Chủ vận hành sản xuất/kho/tồn/trace/recall

Xác nhận operational readiness

Final Owner

Chủ quyết định go-live

Ký Go-live Approved


### 6.3. Owner Sign-off Requirements

Owner sign-off phải có:

owner_role.

owner_scope.

decision.

decision_time.

evidence references.

smoke references.

risk notes.

rollback confirmation.

Owner sign-off bằng lời nói, chat rời, checklist chữ hoặc báo cáo không có evidence không đủ điều kiện release.


### 6.4. Owner Cannot Override P0 điểm chặn Without Formal Risk Acceptance

Owner không được bỏ qua P0 điểm chặn bằng quyết định miệng.

Nếu cần release trong điều kiện có rủi ro, phải có:

Formal risk acceptance.

Scope giới hạn.

Time-bound approval.

Rollback plan.

Monitoring plan.

Customer impact assessment.

Owner sign-off cấp cao.

Completion Gate ghi rõ trạng thái ngoại lệ.

Các điểm chặn liên quan an toàn, payment, privacy, recall, stop sale, security nghiêm trọng không được override nếu chưa có cơ chế kiểm soát được phê duyệt.


## 7. GLOBAL RELEASE DECISION STATES


### 7.1. Standard Decision States

Trạng thái release chuẩn:

State

DRAFT

Đang chuẩn bị

UNDER_REVIEW

Đang review

Thiếu security

WAITING_OWNER_SIGNOFF

Thiếu owner sign-off

Bị chặn

READY_CANDIDATE

Ứng viên sẵn sàng

Đủ Production Ready

Được duyệt release

Được duyệt go-live

LIVE

Đang live

PAUSED

Tạm dừng

ROLLBACK_REQUIRED

Cần rollback

ROLLED_BACK

Đã rollback

CLOSED

Đóng release


### 7.2. Current Global Default State

Trạng thái mặc định của hệ thống khi chưa có evidence đầy đủ:

Hạng mục

Default State

Không được tự thay đổi các trạng thái này nếu chưa có evidence, smoke, security, owner sign-off và Completion Gate PASS.


## 8. PHẦN 1/4 DONE GATE


## PHẦN 1/4 được xem là đạt governance scope khi đã khóa đủ:

MASTER-07 là governance release document, không phải implementation.

Governance PASS không đồng nghĩa Production Ready.

Production Ready không đồng nghĩa Release Approved.

Release Approved không đồng nghĩa Go-live Approved.

Completion Report không thay thế Completion Gate.

Evidence ACCEPTED là bắt buộc.

Runtime Resolver phải đứng trước runtime action.

Guard phải đứng trước release.

Traceability phải đứng trước approval.

Operational Core kiểm soát product truth.

Recall / Sale Lock / Stop Sale thắng sales.

Scope bao phủ Operational Core, Production, Warehouse, Inventory, Product Activation, Sellable, Traceability, Recall, Sale Lock, ADS, MC AI, Gateway, Security, Payment, Shipping, CRM, Order, AI Advisor, IVR và Future Extension.

Production Readiness Model đã định nghĩa rõ.

Go-live Boundary đã định nghĩa rõ.

Owner Authority đã định nghĩa rõ.

Global Release Decision States đã định nghĩa rõ.


## 9. PHẦN 1/4 FINAL CONCLUSION


## PHẦN 1/4 của MASTER-07 khóa nguyên tắc nền cho toàn bộ Global Release Governance của Ginsengfood.

Từ điểm này trở đi, mọi xét duyệt Production Ready, Release Approved và Go-live Approved phải đi theo mô hình:

Source-of-Truth -> Owner Core -> Runtime Resolver -> Guard -> Traceability ID -> Audit -> Evidence -> Smoke -> Owner Sign-off -> Completion Gate -> Release Gate -> Go-live Decision

Không pack, module, channel, campaign, AI, IVR, ADS, MC AI, Gateway, Payment, Shipping, CRM, Order, Operational Core hoặc future extension nào được tự gọi ready, pass, release hoặc go-live nếu chưa đi đủ chuỗi kiểm soát này.


## PHẦN 2/4 — RELEASE GATE REGISTRY BY DOMAIN / OPERATIONAL / RECALL / ADS / MC AI / GATEWAY / PAYMENT / SHIPPING / CRM / ORDER / AI / IVR / FUTURE EXTENSION


## 10. RELEASE GATE REGISTRY PRINCIPLES


### 10.1. Release Gate Registry Purpose

Release Gate Registry là bảng kiểm soát chính thức dùng để xác định domain, pack, capability, channel hoặc extension nào được phép tiến đến:

Live.

Scale.

Rollback.

Stop.

Suppress.

Block.

Mỗi gate trong registry phải xác định rõ:

Gate kiểm soát domain nào.

Owner Core là ai.

Consumer nào bị ảnh hưởng.

Runtime Resolver nào bắt buộc.

Guard nào bắt buộc.

Evidence nào bắt buộc.

Smoke nào bắt buộc.

Negative path nào bắt buộc.

Security gate có áp dụng hay không.

Owner sign-off nào bắt buộc.

bị chặn-if-missing rule là gì.

Không domain nào được release nếu không có gate trong registry.


### 10.2. One Domain — One Release Gate Owner

Mỗi domain P0 phải có Owner Core rõ ràng.

Consumer không được làm Owner Core của rule nghiệp vụ.

Ví dụ:

AI Advisor là Consumer của giá, không phải Owner Core của giá.

Gateway là Consumer của payment instruction policy, không phải Owner Core của payment.

ADS là Consumer của sellable/product activation, không phải Owner Core của tồn kho.

MC AI là Consumer của Daily Live Product Board, không phải Owner Core của chương trình live.

CRM là Consumer của customer context, không phải Owner Core của customer identity.

IVR là Consumer của order confirmation, không phải Owner Core của order/payment.

Nếu chưa xác định được Owner Core thì gate mặc định bị chặn.


### 10.3. Release Gate Cannot Be Inferred

Release Gate không được suy luận từ:

Tài liệu đã viết xong.

Dev nói đã làm xong.

Demo chạy được một lần.

Checklist chưa có evidence.

Smoke không có log.

Owner review chưa ký.

Kết quả test không có trace ID.

Màn hình hiển thị đúng nhưng backend chưa audit.

AI trả lời đúng nhưng resolver/guard chưa chứng minh.

Release Gate chỉ được xét bằng evidence ACCEPTED.


### 10.4. Consumer Release Depends on Owner Gate

Consumer chỉ được release khi Owner Gate liên quan đã PASS.

Phụ thuộc bắt buộc

Product Knowledge, Claim, Customer, Pricing, Member, Diamond, Quote, Order, Payment, Shipping, Sellable, Health, Complaint

Public/Messenger Policy, Handoff, PII, Moderation, Security, Completion Gate

Customer Identity, Customer Memory, Trigger, Suppression, Member Lifecycle, Privacy, Complaint

Product Activation, Sellable, Attribution, Verified Revenue, Scale Gate, Owner Approval

Live Board, Script Brief, Product Activation, Program Runtime, Recall/Sale Lock

Product Activation, Sellable, Price, Claim, Payment/Shipping boundary

OrderConfirmationOnly, Order state, Call log, Confirmation result, Audit

Payment Core, Bank Registry, Accounting Review

Shipping Core, ETA, COD, Tracking

ACTIVE_GOVERNED_EXTENSION

Consumer không được tự mở release nếu Owner Gate chưa PASS.


## 11. STANDARD RELEASE GATE RECORD


### 11.1. Required Gate Fields

Mỗi Release Gate phải có cấu trúc chuẩn sau:

Field

Bắt buộc

release_gate_id

Có

Mã gate duy nhất

domain_id

Domain nghiệp vụ / kỹ thuật được kiểm soát

gate_name

Tên gate

owner_core

Owner Core chịu trách nhiệm

consumer_scope

Consumer bị ảnh hưởng

source_of_truth

Nguồn sự thật

required_resolver

Resolver bắt buộc

required_guard

Guard bắt buộc

required_trace_ids

ID bắt buộc để trace

required_evidence

Evidence bắt buộc

required_smoke

Smoke bắt buộc

required_negative_path

Negative path bắt buộc

security_required

Có/Không

Có áp security gate hay không

owner_signoff_required

Owner sign-off bắt buộc

blocked_if_missing

Điều kiện mặc định BLOCK

release_decision_state

Trạng thái gate

audit_required

Có audit hay không

rollback_required

Có rollback plan hay không


### 11.2. Standard Release Gate Decision

Mỗi gate chỉ được trả một trong các decision sau:

Decision

Được đi tiếp không

PASS

Đạt đủ điều kiện gate

BLOCK

SUPPRESS

Không cho Consumer hành động

FALLBACK

Chỉ dùng phương án an toàn

Có giới hạn

HANDOFF

Chuyển người/phân hệ xử lý

Có điều kiện

REVIEW_REQUIRED

Cần owner review

ESCALATE

Cần cấp cao xử lý

NO_ACTION

Không hành động

STOP_SALE_REQUIRED

Cần dừng bán

Không có decision rõ thì mặc định BLOCK.


## 12. RELEASE GATE REGISTRY OVERVIEW


### 12.1. Global Gate Codes

Gate Code

Domain

Gate Name

REL-GOV-001

Governance

Global Governance Release Gate

REL-OPR-001

Operational Core

Operational Core Release Gate

REL-PRD-001

Production

Production Readiness Gate

REL-WHI-001

Warehouse / Inventory

Warehouse Inventory Readiness Gate

REL-PAS-001

Product Activation / Sellable

Product Activation Sellable Gate

REL-TRC-001

Traceability

Traceability Public Trace Gate

REL-RCL-001

Recall / Sale Lock

Recall Stop-Sale Gate

REL-SEC-001

Security

Security Release Gate

REL-GTW-001

Gateway Release Gate

REL-PMT-001

Payment Release Gate

REL-SHP-001

Shipping Release Gate

REL-CRM-001

CRM Release Gate

REL-ORD-001

Order / Quote

Order Quote CustomerConfirmation Gate

REL-AIA-001

AI Advisor Release Gate

REL-IVR-001

IVR ORDER_CONFIRMATION_ONLY Gate

REL-ADS-001

ADS / Attribution

ADS Scale Gate

REL-MCA-001

MC AI / Live

MC AI Live Board Script Brief Gate

REL-CUS-001

Customer Identity / Memory

Customer Context Gate

REL-MBR-001

Member / Rights

Member Lifecycle Rights Gate

REL-DIA-001

Diamond / Affiliate

Diamond Attribution Commission Gate

REL-HCP-001

Health / Complaint / Privacy

Priority Conflict Safety Gate

REL-OFC-001

Official Contact

Official Contact Registry Gate

REL-FUT-001

Future Governed Extension Gate


## 13. RELEASE GATE REGISTRY BY DOMAIN


### 13.1. REL-GOV-001 — GLOBAL GOVERNANCE RELEASE GATE


### 13.1.1. Gate Purpose

REL-GOV-001 kiểm soát việc toàn bộ MASTER và PACK liên quan có được dùng làm nền xét release hay không.

Gate này không xác nhận hệ thống đã production ready. Gate này chỉ xác nhận lớp governance đủ điều kiện làm nền cho release review.


### 13.1.2. Owner Core

Owner Core:

Governance Owner.

Release Owner.

Final Business Owner.


### 13.1.3. Required Inputs

MASTER-00 đến MASTER-07 có trạng thái governance rõ.

Pack registry rõ.

Source-of-Truth registry rõ.

Cross-pack dependency map rõ.

Traceability ID standard rõ.

Runtime resolver / guard standard rõ.

Evidence / smoke standard rõ.

Future extension governance rõ.


### 13.1.4. Required Evidence

MASTER governance approval evidence.

Pack registry evidence.

Source-of-Truth mapping evidence.

Owner Core mapping evidence.

P0 domain dependency evidence.

Release gate registry evidence.

Audit evidence.


### 13.1.5. bị chặn If Missing

REL-GOV-001 BLOCK nếu:

Thiếu MASTER hoặc PACK trong registry.

Không xác định Source-of-Truth.

Không xác định Owner Core.

Không có release gate cho domain P0.

Gateway bị gọi PASS khi Gate còn bị chặn.


### 13.2. REL-OPR-001 — OPERATIONAL CORE RELEASE GATE


### 13.2.1. Gate Purpose

REL-OPR-001 kiểm soát Operational Core trước khi bất kỳ Consumer nào được phép bán, quote, chạy ADS, live, CRM hoặc hiển thị Landing Page liên quan đến sản phẩm.

Operational Core là lớp kiểm soát sự thật vận hành.


### 13.2.2. Owner Core

Operational Owner.

Production Owner.

Warehouse Owner.

Inventory Owner.

Traceability Owner.

Recall Owner.


### 13.2.3. Consumer Scope

Consumer bị phụ thuộc:

Payment nếu phát sinh giao dịch.

Shipping nếu phát sinh giao hàng.

IVR nếu xác nhận đơn.


### 13.2.4. Required Resolver

OperationalStatusResolver.

ProductActivationResolver.

SellableStatusResolver.

StockAvailabilityResolver.

QualityHoldResolver.

RecallHoldResolver.

SaleLockResolver.

ChannelBlockResolver.


### 13.2.5. Required Guard

OperationalReleaseGuard.

ProductSellableGuard.

StockGuard.

HoldRecallGuard.

SaleLockGuard.

ChannelEligibilityGuard.


### 13.2.6. Required Evidence

Operational flow evidence.

Production completion evidence.

Warehouse receipt evidence.

Inventory ledger evidence.

Sellable status evidence.

Hold / recall / sale lock negative evidence.

Consumer suppression evidence khi Operational Core BLOCK.


### 13.2.7. bị chặn If Missing

REL-OPR-001 BLOCK nếu:

Không chứng minh được thành phẩm đã hoàn tất sản xuất.

Không chứng minh được nhập kho hợp lệ.

Không có stock_available hợp lệ.

Không có sellable_status.

Không có listed_price_status.

Không có hold/recall/sale lock guard.

Consumer có thể bán khi Operational Core BLOCK.

Không có audit.


### 13.3. REL-PRD-001 — PRODUCTION READINESS GATE


### 13.3.1. Gate Purpose

REL-PRD-001 kiểm soát domain sản xuất trước khi kết quả sản xuất được dùng cho tồn kho, mở bán, traceability, recall hoặc kế toán.


### 13.3.2. Owner Core

Recipe Owner.

QC Owner.


### 13.3.3. Required Resolver

ProductionOrderResolver.

RecipeSnapshotResolver.

BatchStatusResolver.

MaterialIssueResolver.

MaterialReceiptResolver.

ProductionQCResolver.

BatchReleaseResolver.


### 13.3.4. Required Guard

ProductionOrderGuard.

RecipeSnapshotGuard.

MaterialIssueGuard.

BatchQCGuard.

BatchReleaseGuard.

ProductionPlanningSignalGuard.


### 13.3.5. Required Evidence

Production order evidence.

Recipe snapshot evidence.

Material issue evidence.

Material receipt evidence.

Batch genealogy evidence.

QC inspection evidence.

Batch release evidence.

Production planning signal evidence.

Owner review evidence trước khi signal thành production action.


### 13.3.6. bị chặn If Missing

REL-PRD-001 BLOCK nếu:

Production Order không có recipe snapshot.

Batch không trace được nguyên liệu.

Material issue không có ledger/audit.

QC_PASS bị hiểu nhầm là RELEASED.

Warehouse receipt nhận batch chưa released.

Sales/Stock Movement tự tạo Production Order.

Thiếu owner review cho production planning signal.

Thiếu traceability từ batch đến SKU/lô.


### 13.4. REL-WHI-001 — WAREHOUSE / INVENTORY READINESS GATE


### 13.4.1. Gate Purpose

REL-WHI-001 kiểm soát kho và tồn kho trước khi sản phẩm được mở bán, quote, chạy ADS, lên Live Board hoặc xuất hiện trong CRM recommendation.


### 13.4.2. Owner Core


### 13.4.3. Required Resolver

WarehouseReceiptResolver.

FinishedGoodsStockResolver.

StockAvailableResolver.

ReservedStockResolver.

BlockedStockResolver.

InventoryLedgerResolver.

ChannelStockResolver.


### 13.4.4. Required Guard

WarehouseReceiptGuard.

InventoryBalanceGuard.

StockAvailabilityGuard.

ChannelStockGuard.

StockMovementAuditGuard.


### 13.4.5. Required Evidence

Lot balance evidence.

Stock available evidence.

Reserved stock evidence.

bị chặn stock evidence.

Channel stock evidence.

Negative evidence cho stock = 0.


### 13.4.6. bị chặn If Missing

REL-WHI-001 BLOCK nếu:

Không chứng minh được tồn kho khả dụng.

Stock available không khớp ledger.

Có stock nhưng đang bị chặn.

Có stock nhưng channel không được phép bán.

Consumer nói còn hàng khi Stock Guard chưa PASS.

Tồn kho âm hoặc không trace được.

Thiếu negative path cho hết hàng.


### 13.5. REL-PAS-001 — PRODUCT ACTIVATION / SELLABLE / STOCK GATE


### 13.5.1. Gate Purpose

REL-PAS-001 kiểm soát việc SKU có được phép bán trên từng kênh, chương trình, live, ADS, CRM, AI Advisor hoặc Landing Page hay không.


### 13.5.2. Owner Core

Product Activation Owner.

Pricing Owner.

Channel Owner.


### 13.5.3. Required Resolver

ListedPriceStatusResolver.

ProgramActivationResolver.

ChannelEligibilityResolver.


### 13.5.4. Required Guard

BaseSellableGate.

ProgramEligibilityGuard.

PriceActiveGuard.

StopSaleGuard.


### 13.5.5. Required Base Sellable Conditions

SKU chỉ được phép bán khi đồng thời đạt:


### 13.5.6. bị chặn If Missing

REL-PAS-001 BLOCK nếu:

SKU chưa active.

SKU chưa sellable.

Không có giá active.

Không có tồn kho khả dụng.

Có hold/recall/sale lock.

Channel bị block.

ADS/AI/CRM/MC AI/Landing Page có thể nói/bán SKU không sellable.

Consumer tự tạo danh sách SKU bán mà không qua resolver.


### 13.6. REL-TRC-001 — TRACEABILITY / PUBLIC TRACE GATE


### 13.6.1. Gate Purpose

REL-TRC-001 kiểm soát khả năng truy xuất nguồn gốc, liên kết lô, batch, QR, public trace và whitelist dữ liệu public.


### 13.6.2. Owner Core

Public Trace Owner.


### 13.6.3. Required Resolver

TraceLinkResolver.

BatchGenealogyResolver.

PublicTracePolicyResolver.

QRStatusResolver.

TraceProjectionResolver.


### 13.6.4. Required Guard

TraceCompletenessGuard.

PublicTraceWhitelistGuard.

QRPublicValidityGuard.

InternalDataLeakGuard.

TraceGapGuard.


### 13.6.5. Required Evidence

Internal trace evidence.

Public trace projection evidence.

QR valid evidence.

QR void/failed negative evidence.

Whitelist public field evidence.

Internal data leak negative evidence.

Trace gap evidence.


### 13.6.6. bị chặn If Missing

REL-TRC-001 BLOCK nếu:

Không trace được từ SKU/batch/lô đến nguồn liên quan.

Public trace có nguy cơ lộ supplier evidence, internal ID, costing, QC internal note, MISA/private data.

QR VOID/FAILED vẫn public-valid.

PACKET có QR/public trace standalone trái policy.

Trace projection không có whitelist.

Recall không thể dùng trace để phân tích impact.


### 13.7. REL-RCL-001 — RECALL / SALE LOCK / STOP SALE GATE


### 13.7.1. Gate Purpose

REL-RCL-001 kiểm soát recall, hold, sale lock, stop sale và suppression toàn hệ thống khi có rủi ro sản phẩm.

Đây là gate có quyền ưu tiên cao hơn sales, CRM, ADS, MC AI, Gateway, Landing Page và AI Advisor.


### 13.7.2. Owner Core

Quality Owner.

Risk Owner.

Final Business Owner khi có tác động nghiêm trọng.


### 13.7.3. Required Resolver

RecallCaseResolver.

RecallImpactResolver.

HoldStatusResolver.

StopSaleResolver.

ConsumerSuppressionResolver.

RecoveryStatusResolver.


### 13.7.4. Required Guard

RecallGuard.

ConsumerSuppressionGuard.

RecoveryGuard.

ResidualRiskGuard.


### 13.7.5. Required Evidence

Recall case evidence.

Impact analysis evidence.

Hold evidence.

Sale lock evidence.

Stop Sale evidence.

Consumer suppression evidence.

Recovery evidence.

Residual risk evidence nếu có.

Negative path evidence: SKU/lô bị recall không thể bán.


### 13.7.6. bị chặn If Missing

REL-RCL-001 BLOCK nếu:

Recall không tạo được impact scope.

Sale Lock không chặn được AI/CRM/ADS/MC AI/Gateway/Landing Page.

SKU/lô bị hold vẫn quote được.

SKU/lô bị recall vẫn order được.

MC AI vẫn nói SKU bị lock.

ADS vẫn đẩy SKU bị stop sale.

CRM vẫn gửi gợi ý SKU bị recall.

Landing Page vẫn hiển thị CTA mua SKU bị lock.

Không có negative path evidence.


### 13.8. REL-SEC-001 — SECURITY RELEASE GATE


### 13.8.1. Gate Purpose

REL-SEC-001 kiểm soát bảo mật trước khi domain có dữ liệu thật, khách thật, giao dịch thật hoặc public exposure.


### 13.8.2. Owner Core

Security Owner.

Technical Owner.

Data Protection Owner.

Integration Owner nếu có external system.


### 13.8.3. Required Resolver

AuthResolver.

PermissionResolver.

RoleBoundaryResolver.

SecretResolver.

WebhookVerificationResolver.

DeviceCallbackResolver nếu có.

EvidenceAccessResolver.

PIIAccessResolver.


### 13.8.4. Required Guard

AuthenticationGuard.

AuthorizationGuard.

PermissionGuard.

SecretLeakGuard.

WebhookSignatureGuard.

PIILeakGuard.

PaymentDataGuard.

AdminAccessGuard.

ExternalIntegrationGuard.


### 13.8.5. Required Evidence

Auth evidence.

Permission evidence.

Secret management evidence.

Webhook verification evidence.

PII protection evidence.

Admin access evidence.

Payment data protection evidence nếu thuộc scope.

Incident/abuse handling evidence.


### 13.8.6. bị chặn If Missing

REL-SEC-001 BLOCK nếu:

Secret bị hardcode.

Webhook không verify được.

Admin role vượt quyền.

PII lộ ở public comment/log/template.

Payment data bị public.

Evidence storage lộ dữ liệu nội bộ.

External integration có thể direct-write sai boundary.

Security smoke không PASS.

Negative path thiếu.


### 13.9. REL-GTW-001 — GATEWAY RELEASE GATE


### 13.9.1. Gate Purpose

REL-GTW-001 kiểm soát Gateway trước khi mở kênh public comment, Messenger handoff, live, moderation hoặc channel automation.


### 13.9.2. Owner Core

Gateway Owner.

Privacy Owner.


### 13.9.3. Required Resolver

PublicCommentPolicyResolver.

MessengerHandoffResolver.

HandoffDeliveryResolver.

ModerationPolicyResolver.

PrivacyResolver.

HealthBoundaryResolver.

PaymentInstructionPolicyResolver.

CompletionGateResolver.


### 13.9.4. Required Guard

PublicCommentGuard.

MessengerHandoffGuard.

PIIProtectionGuard.

HealthDetailSuppressionGuard.

PaymentInstructionSuppressionGuard.

ModerationGuard.

ComplaintRoutingGuard.

GatewayCompletionGuard.


### 13.9.5. Required Evidence

Public comment evidence.

Messenger handoff evidence.

Handoff success/fail evidence.

PII suppression evidence.

Health detail suppression evidence.

Payment instruction suppression evidence.

Moderation evidence.

Abuse/troll/spam handling evidence.

Complaint routing evidence.

Security evidence.

Completion Gate evidence.


### 13.9.6. bị chặn If Missing

REL-GTW-001 BLOCK nếu:

Gateway security chưa PASS.

Handoff fail nhưng AI nói đã gửi Messenger.

Public comment báo giá/chốt đơn/payment instruction trái policy.

Public comment lặp PII.

Complaint thật bị xử như troll.

Troll/abuse không có moderation policy.

Gateway không có audit.


### 13.10. REL-PMT-001 — PAYMENT RELEASE GATE


### 13.10.1. Gate Purpose

REL-PMT-001 kiểm soát thanh toán trước khi hệ thống được phép nhận tiền, hướng dẫn thanh toán hoặc xác nhận trạng thái payment.


### 13.10.2. Owner Core

Payment Owner.

Accounting Owner.

Finance Owner.


### 13.10.3. Payment Scope

Payment strategy hiện tại gồm:

Direct bank transfer bắt buộc đi qua:


### 13.10.4. Required Resolver

PaymentMethodResolver.

CompanyBankAccountRegistryResolver.

PaymentReferenceResolver.

BankTransferQueueResolver.

AccountingReviewResolver.

PaymentStatusResolver.

PaymentReconcileResolver.


### 13.10.5. Required Guard

PaymentMethodGuard.

BankAccountNoHardcodeGuard.

PaymentReferenceGuard.

AccountingReviewGuard.

PaidStatusGuard.

PaymentInstructionGuard.

ReconcileGuard.


### 13.10.6. Required Evidence

VNPAY evidence nếu active.

MOMO_BUSINESS evidence nếu active.

Direct bank transfer evidence nếu active.

Company Bank Account Registry evidence.

payment_reference evidence.

transfer memo evidence.

Accounting Review evidence.

Payment status evidence.

Negative evidence: PaymentReference không tự thành PAID.

No-hardcode evidence.


### 13.10.7. bị chặn If Missing

REL-PMT-001 BLOCK nếu:

Tài khoản ngân hàng bị hardcode trong AI/Gateway/CRM/Admin UI/frontend/template.

Không có Company Bank Account Registry.

Không có payment_reference.

Không có transfer memo.

Không có Accounting Review.

PaymentReference bị xem là PAID.

AI xác nhận đã thanh toán khi Payment Core chưa xác nhận.

IVR xác nhận payment.

Gateway public payment instruction trái policy.

Không có reconcile evidence.


### 13.11. REL-SHP-001 — SHIPPING RELEASE GATE


### 13.11.1. Gate Purpose

REL-SHP-001 kiểm soát giao hàng, phí ship, COD, ETA, tracking và shipping eligibility trước khi hệ thống được phép báo giao hàng hoặc chốt đơn có giao hàng.


### 13.11.2. Owner Core

Shipping Owner.

Order Owner.

Finance Owner nếu có COD.

Customer Service Owner.


### 13.11.3. Required Resolver

ShippingEligibilityResolver.

ShippingFeeResolver.

CODResolver.

ETAResolver.

TrackingResolver.

DeliveryStatusResolver.

ReturnRecoveryResolver nếu có.


### 13.11.4. Required Guard

ShippingEligibilityGuard.

ShippingFeeGuard.

CODGuard.

ETAGuard.

TrackingGuard.

DeliveryStatusGuard.

InternationalShippingGuard.


### 13.11.5. Required Evidence

Domestic shipping eligibility evidence.

Shipping fee evidence.

COD eligibility evidence.

ETA evidence.

Tracking evidence.

Failed delivery evidence.

Return/recovery evidence nếu có.

MAS-SMK-006 evidence mapping.

Negative evidence: không bịa ETA/tracking.


### 13.11.6. bị chặn If Missing

REL-SHP-001 BLOCK nếu:

Không có Shipping Core evidence.

AI/Gateway/CRM tự bịa ETA.

Không xác định được phí ship.

Không xác định được COD eligibility.

Tracking không trace được.

International shipping bị mở khi chưa owner approve.

MAS-SMK-006 không link đủ PACK-04, PACK-01, PACK-03, PACK-05.

Shipping status không audit được.


### 13.12. REL-CRM-001 — CRM RELEASE GATE


### 13.12.1. Gate Purpose

REL-CRM-001 kiểm soát CRM trước khi hệ thống gửi tin chăm sóc, gợi ý mua lại, nhắc quyền lợi thành viên, nhắc Giờ Vàng, nhắc 24/7 hoặc kích hoạt journey 12 tháng.


### 13.12.2. Owner Core

CRM Owner.

Customer Owner.

Member Owner.

Complaint Owner.


### 13.12.3. Required Resolver

CustomerIdentityResolver.

CustomerMemoryResolver.

CRMTriggerResolver.

SuppressionResolver.

MemberLifecycleResolver.

PrivacyPreferenceResolver.

ComplaintStatusResolver.

MessageDedupResolver.


### 13.12.4. Required Guard

CustomerContextGuard.

CRMTriggerGuard.

SuppressionGuard.

DuplicateMessageGuard.

PrivacyGuard.

ComplaintSuppressionGuard.

MemberLifecycleGuard.

SpamGuard.


### 13.12.5. Required Evidence

Customer identity evidence.

Customer memory evidence.

Trigger evidence.

Suppression evidence.

12-month journey evidence.

Member lifecycle evidence.

Grace period evidence.

Privacy opt-out evidence.

Complaint suppression evidence.

Duplicate message negative evidence.


### 13.12.6. bị chặn If Missing

REL-CRM-001 BLOCK nếu:

CRM gửi khi không có trigger.

CRM gửi khi đang suppression.

CRM spam nhiều kênh.

CRM không nhận biết customer identity.

CRM không nhận biết customer memory.

CRM gợi ý SKU không active/sellable.

CRM gửi SKU bị recall/sale lock.

CRM gửi quyền lợi member sai lifecycle.

CRM bỏ qua complaint/privacy.

Không có message job trace.


### 13.13. REL-ORD-001 — ORDER / QUOTE / CUSTOMERCONFIRMATION RELEASE GATE


### 13.13.1. Gate Purpose

REL-ORD-001 kiểm soát toàn bộ luồng quote, order draft, customer confirmation, order code, payment reference, shipping eligibility và IVR handoff.


### 13.13.2. Owner Core

Accounting Owner nếu có payment.


### 13.13.3. Required Resolver

QuoteSnapshotResolver.

QuoteCartResolver.

ProgramRuntimeResolver.

MemberBenefitResolver.

DiamondBenefitResolver.

VATResolver.

OrderDraftResolver.

CustomerConfirmationResolver.

OrderCodeResolver.

DuplicateOrderResolver.


### 13.13.4. Required Guard

QuoteSnapshotGuard.

PriceFreezeGuard.

MemberBenefitGuard.

DiamondBenefitGuard.

VATDisplayGuard.

CustomerConfirmationGuard.

OrderCodeGuard.

DuplicateOrderGuard.

IVRHandoffGuard.


### 13.13.5. Required Evidence

QuoteSnapshot evidence.

QuoteCart evidence.

Program runtime evidence.

Member benefit evidence.

Diamond benefit evidence.

VAT display evidence.

OrderConfirmationDraft evidence.

CustomerConfirmation evidence.

OrderCode evidence.

Duplicate order negative evidence.

PaymentReference evidence.

Shipping eligibility evidence.

Price freeze window evidence.


### 13.13.6. bị chặn If Missing

REL-ORD-001 BLOCK nếu:

Báo giá không có QuoteSnapshot.

Quote hết hạn vẫn dùng.

Giá chương trình không lấy từ runtime.

Member/Diamond benefit bị bỏ sót khi eligible.

VAT hiển thị mơ hồ.

OrderCode tạo khi chưa có CustomerConfirmation.

PaymentReference bị hiểu là PAID.

Shipping eligibility chưa xác định.

Duplicate order không chặn.

IVR được gọi khi order chưa đủ điều kiện.


### 13.14. REL-AIA-001 — AI ADVISOR RELEASE GATE


### 13.14.1. Gate Purpose

REL-AIA-001 kiểm soát AI Advisor trước khi AI được phép tư vấn, gợi ý sản phẩm, xử lý objection, báo quote, tạo order draft, handoff Messenger, xử lý health boundary, complaint và CRM conversation.


### 13.14.2. Owner Core

AI Advisor Owner.

Product Knowledge Owner.

Claim/Safety Owner.

Compliance Owner.


### 13.14.3. Required Resolver

ProductKnowledgeResolver.

ClaimWhitelistResolver.

SegmentRecommendationResolver.

ProductRecommendationResolver.

PricingRuntimeResolver.

DiamondResolver.

ComplaintResolver.

PublicMessengerBoundaryResolver.

PaymentBoundaryResolver.

ShippingBoundaryResolver.


### 13.14.4. Required Guard

ClaimGuard.

PublicWordingGuard.

HealthBoundaryGuard.

ComplaintGuard.

PriceQuoteGuard.

OrderGuard.

PaymentBoundaryGuard.

ShippingBoundaryGuard.

NoHardcodeRuntimeGuard.


### 13.14.5. Required Evidence

Product knowledge evidence.

Claim whitelist evidence.

Public wording evidence.

Customer context evidence.

Recommendation evidence.

Pricing evidence.

Member/Diamond evidence.

Health boundary evidence.

Complaint evidence.

Quote evidence.

Order draft evidence.

Payment boundary evidence.

Shipping boundary evidence.

Public/Messenger handoff evidence.


### 13.14.6. bị chặn If Missing

REL-AIA-001 BLOCK nếu:

AI tự suy luận giá/quyền lợi/tồn kho.

AI nói công dụng như thuốc.

AI bán SKU không active/sellable.

AI báo giá public trái policy.

AI tạo OrderCode khi chưa có CustomerConfirmation.

AI xác nhận PAID khi payment chưa confirmed.

AI bịa ETA/tracking.

AI bỏ qua recall/sale lock.

AI gửi contact không qua OfficialContactResolver.

AI xử lý complaint như sales thường.

Không có test matrix evidence.


### 13.15. REL-IVR-001 — IVR ORDER_CONFIRMATION_ONLY RELEASE GATE


### 13.15.1. Gate Purpose

REL-IVR-001 kiểm soát PACK-09 IVR_ORDER_CONFIRMATION trước khi hệ thống gọi tự động xác nhận đơn.

IVR chỉ có chức năng xác nhận đơn, không bán hàng.


### 13.15.2. Owner Core

IVR Owner.


### 13.15.3. Required Resolver

IVREligibilityResolver.

OrderConfirmationDraftResolver.

CustomerPhoneResolver.

CallAttemptResolver.

ConfirmationResultResolver.

RetryPolicyResolver.

CallLogResolver.


### 13.15.4. Required Guard

IVROrderConfirmationOnlyGuard.

OrderEligibilityGuard.

CustomerPhonePrivacyGuard.

CallFrequencyGuard.

RetryPolicyGuard.

PaymentNoConfirmationGuard.

SalesNoUpsellGuard.

CallAuditGuard.


### 13.15.5. Required Evidence

INTERNAL_SIM_GATEWAY_SERVER evidence.

ORDER_CONFIRMATION_ONLY evidence.

Order eligibility evidence.

Call log evidence.

Confirmation result evidence.

Retry policy evidence.

Negative evidence: IVR không bán hàng.

Negative evidence: IVR không upsell.

Negative evidence: IVR không xác nhận payment.


### 13.15.6. bị chặn If Missing

REL-IVR-001 BLOCK nếu:

IVR được dùng để bán hàng.

IVR được dùng để upsell.

IVR tự đổi order state ngoài phạm vi xác nhận đơn.

IVR gọi khi không có OrderConfirmationDraft/OrderCode hợp lệ.

Không có call log.

Không có confirmation result.

Không có retry policy.

IVR bị mở sang sales/CRM/marketing khi chưa qua future governed extension.


### 13.16. REL-ADS-001 — ADS / ATTRIBUTION / ROAS / CPA / AOV / SCALE GATE


### 13.16.1. Gate Purpose

REL-ADS-001 kiểm soát ADS trước khi campaign được phép chạy, mở rộng ngân sách hoặc scale.

ADS không được tự scale chỉ vì có comment, inbox hoặc doanh thu chưa verified.


### 13.16.2. Owner Core

ADS Owner.

Revenue Owner.

Attribution Owner.


### 13.16.3. Required Resolver

AdsCampaignResolver.

AttributionResolver.

VerifiedRevenueResolver.

ROASResolver.

CPAResolver.

AOVResolver.

BoxesPerOrderResolver.

FunnelMetricResolver.

CODFailResolver.

CRMRevenueResolver.

DiamondRevenueResolver.

DataQualityResolver.

ScaleEligibilityResolver.


### 13.16.4. Required Guard

AdsProductEligibilityGuard.

SellableGuard.

AttributionConflictGuard.

VerifiedRevenueGuard.

ROASGuard.

CPAGuard.

AOVGuard.

DataQualityGuard.

CODFailGuard.

ScaleApprovalGuard.

OwnerApprovalBeforeScaleGuard.


### 13.16.5. Required Scale Metrics

ADS Scale Gate phải có tối thiểu:

Data Quality.


### 13.16.6. Required Evidence

Campaign evidence.

Product activation evidence.

Sellable evidence.

Attribution evidence.

Verified revenue evidence.

Funnel metric evidence.

ROAS/CPA/AOV evidence.

COD fail evidence.

CRM revenue evidence.

Diamond revenue evidence.

Data quality evidence.

Pilot 7–14 ngày evidence.

Owner approval evidence.

Negative evidence: không scale khi revenue chưa verified.


### 13.16.7. bị chặn If Missing

REL-ADS-001 BLOCK nếu:

ADS đẩy SKU không active/sellable.

ADS đẩy SKU bị recall/sale lock.

ADS tự scale khi thiếu verified revenue.

ADS scale khi data quality không đạt.

ADS scale khi COD fail vượt ngưỡng chưa được owner chấp nhận.

ADS tự quyết định Diamond attribution khi có conflict.

ADS không tách CRM Revenue và Diamond Revenue khi cần.

Không có pilot 7–14 ngày evidence.

Không có owner approval trước scale.


### 13.17. REL-MCA-001 — MC AI / LIVE BOARD / SCRIPT BRIEF RELEASE GATE


### 13.17.1. Gate Purpose

REL-MCA-001 kiểm soát MC AI trước khi MC AI được phép hỗ trợ live, gợi ý lời dẫn, nói SKU, nói chương trình, giữ nhịp bán hoặc hỗ trợ chốt trong live.

MC AI không phải Owner Core của sản phẩm, giá, tồn kho hoặc chương trình.


### 13.17.2. Owner Core

Live Commerce Owner.

Program Owner.

MC AI Owner.


### 13.17.3. Required Resolver

DailyLiveProductBoardResolver.

MC AIScriptBriefResolver.

LiveSessionResolver.

HeroSKUResolver.

SupportSKUResolver.

RestrictedSKUResolver.

QuoteSnapshotBoundaryResolver.

StockGuardResolver.

FakeUrgencyResolver.

LiveModerationResolver.


### 13.17.4. Required Guard

LiveBoardGuard.

ScriptBriefGuard.

SKUOnBoardGuard.

RestrictedSKUGuard.

ProgramRuntimeGuard.

NoPublicPriceGuard nếu policy áp dụng.

QuoteSnapshotBoundaryGuard.

RecallHoldGuard.

FakeUrgencyGuard.

LiveModerationGuard.


### 13.17.5. Required Evidence

Daily Live Product Board evidence.

mc_ai_script_brief_id evidence.

Live session evidence.

Hero/support/restricted SKU evidence.

Quote boundary evidence.

Sellable/stock evidence.

Recall/hold/sale lock negative evidence.

Fake urgency negative evidence.

Live moderation evidence.


### 13.17.6. bị chặn If Missing

REL-MCA-001 BLOCK nếu:

MC AI nói SKU ngoài Daily Live Product Board.

MC AI dùng script brief không hợp lệ.

MC AI nói SKU restricted.

MC AI nói giá khi không có QuoteSnapshot / Program Runtime hợp lệ.

MC AI tạo fake urgency.

MC AI nói còn hàng khi Stock Guard chưa PASS.

MC AI tiếp tục bán SKU bị recall/hold/sale lock.

MC AI tự quyết định chương trình live.

MC AI không có audit.


### 13.18. REL-CUS-001 — CUSTOMER IDENTITY / CUSTOMER MEMORY GATE


### 13.18.1. Gate Purpose

REL-CUS-001 kiểm soát việc nhận diện khách hàng, customer memory, lịch sử mua, mục đích mua, người nhận và ngữ cảnh chăm sóc trước khi AI/CRM/Order/Member/Diamond sử dụng dữ liệu.


### 13.18.2. Owner Core


### 13.18.3. Required Resolver

CustomerProfileResolver.

PurchaseHistoryResolver.

RecipientResolver.

ConsentResolver.


### 13.18.4. Required Guard

CustomerIdentityGuard.

CustomerMemoryFreshnessGuard.

ConsentGuard.

PIIRedactionGuard.

WrongCustomerGuard.


### 13.18.5. Required Evidence

Customer memory snapshot evidence.

Purchase history evidence.

Recipient distinction evidence.

Privacy evidence.

Wrong-customer negative evidence.


### 13.18.6. bị chặn If Missing

REL-CUS-001 BLOCK nếu:

Không xác định được khách hàng nhưng AI/CRM nói như đã biết chắc.

Lấy nhầm lịch sử mua của khách khác.

Lặp PII ở public.

Không phân biệt người mua và người nhận.

Customer memory stale nhưng vẫn dùng để chốt.

Privacy opt-out bị bỏ qua.


### 13.19. REL-MBR-001 — MEMBER LIFECYCLE / RIGHTS GATE


### 13.19.1. Gate Purpose

REL-MBR-001 kiểm soát quyền lợi thành viên, hạng thành viên, chu kỳ 12 tháng, duy trì hạng, ân hạng và tranh chấp quyền lợi.


### 13.19.2. Owner Core

Dispute Owner.


### 13.19.3. Required Resolver

MemberStatusResolver.

MemberTierResolver.

MemberRightsResolver.

MemberGraceResolver.

MemberDisputeResolver.

RuntimeRightsResolver.


### 13.19.4. Required Guard

MemberEligibilityGuard.

MemberRightsGuard.

GracePeriodGuard.

DisputeGuard.

RuntimeRightsGuard.

ProgramStackingGuard.


### 13.19.5. Required Evidence

Member identity evidence.

Tier evidence.

Lifecycle evidence.

Rights evidence.

Dispute evidence.

Runtime rights evidence.

Negative evidence: không cộng quyền lợi sai chương trình.


### 13.19.6. bị chặn If Missing

REL-MBR-001 BLOCK nếu:

AI báo quyền lợi hạng khi runtime không xác nhận.

Quote bỏ sót quyền lợi hạng khi runtime xác nhận eligible.

Giờ Vàng/Live cộng giảm hạng sai policy.

Chu kỳ 12 tháng bị tính sai.

Ân hạng 60 ngày bị bỏ qua.

Tranh chấp quyền lợi không route review.

Không trace được member_lifecycle_event_id.


### 13.20. REL-DIA-001 — DIAMOND / AFFILIATE / COMMISSION ATTRIBUTION GATE


### 13.20.1. Gate Purpose

REL-DIA-001 kiểm soát Diamond, referral link, bind status, attribution, commission, self-purchase exclusion và ranh giới nhà phân phối/khởi nghiệp.


### 13.20.2. Owner Core

Diamond Owner.

Affiliate Owner.

Commission Owner.


### 13.20.3. Required Resolver

DiamondReferralLinkResolver.

DiamondBindResolver.

SelfPurchaseResolver.

CommissionEligibilityResolver.

PaymentVerifiedResolver.

OrderEligibilityResolver.

DistributorBoundaryResolver.


### 13.20.4. Required Guard

DiamondBindGuard.

SelfPurchaseExclusionGuard.

CommissionEligibilityGuard.

PaymentVerifiedGuard.

DistributorBoundaryGuard.

NoBindNoDiamondPathGuard.


### 13.20.5. Required Evidence

Referral link evidence.

Bind status evidence.

Order evidence.

Payment verified evidence.

Self-purchase exclusion evidence.

Commission calculation evidence.

Conflict negative evidence.


### 13.20.6. bị chặn If Missing

REL-DIA-001 BLOCK nếu:

Không có bind nhưng vẫn ghi Diamond path.

Buyer từ link Diamond bị hiểu nhầm là Diamond member.

Diamond tự mua vẫn tạo commission.

Commission tạo trước khi PAID verified.

ADS tự quyết định Diamond attribution khi conflict.

Không tách quyền lợi thành viên và commission.

Không trace được diamond_bind_id / commission_event_id.


### 13.21. REL-HCP-001 — HEALTH / COMPLAINT / PRIVACY PRIORITY GATE


### 13.21.1. Gate Purpose

REL-HCP-001 kiểm soát các tình huống ưu tiên cao hơn sales, bao gồm health boundary, complaint, privacy, counterfeit, abuse và priority conflict.


### 13.21.2. Owner Core

Health Boundary Owner.


### 13.21.3. Required Resolver

PrivacyCaseResolver.

CounterfeitCaseResolver.

AbuseModerationResolver.

PriorityConflictResolver.

HumanHandoffResolver.


### 13.21.4. Required Guard

CounterfeitGuard.

AbuseModerationGuard.

PriorityConflictGuard.

HumanHandoffGuard.


### 13.21.5. Required Evidence

Privacy opt-out/delete evidence.

Counterfeit routing evidence.

Abuse/troll moderation evidence.

Priority conflict evidence.

Human handoff evidence.

Negative evidence: complaint thật không bị xử như troll.


### 13.21.6. bị chặn If Missing

REL-HCP-001 BLOCK nếu:

Khách nêu bệnh nhưng AI tư vấn như thuốc.

Khách khiếu nại thật nhưng AI tiếp tục bán.

Privacy request bị bỏ qua.

Comment có mã đơn/mã lô/bằng chứng nhưng bị coi là troll.

Counterfeit case không route đúng.

Safety/complaint/privacy không thắng sales.

Không có audit cho priority conflict.


### 13.22. REL-OFC-001 — OFFICIAL CONTACT REGISTRY GATE


### 13.22.1. Gate Purpose

REL-OFC-001 kiểm soát việc cung cấp số điện thoại, địa chỉ, liên hệ chính thức và tránh lộ số cá nhân lãnh đạo.


### 13.22.2. Owner Core

Official Contact Owner.

Partnership Owner.


### 13.22.3. Required Resolver

OfficialContactResolver.

ContactPurposeResolver.

AddressResolver.

PersonalContactSuppressionResolver.


### 13.22.4. Required Guard

OfficialContactGuard.

ContactPurposeGuard.

PersonalContactLeakGuard.

AddressWordingGuard.


### 13.22.5. Required Evidence

Official contact registry evidence.

Contact purpose evidence.

Address wording evidence.

Personal contact suppression evidence.

Negative evidence: không gửi số cá nhân lãnh đạo.


### 13.22.6. bị chặn If Missing

REL-OFC-001 BLOCK nếu:

AI tự gửi số điện thoại không qua resolver.

Cung cấp số cá nhân lãnh đạo.

Nhầm số tham quan/gặp công ty/lãnh đạo với số đại lý/hợp tác.

Dùng địa chỉ cũ hoặc không đúng wording owner-provided.

Không audit contact response.


### 13.23. REL-FUT-001 — FUTURE GOVERNED EXTENSION GATE


### 13.23.1. Gate Purpose

REL-FUT-001 kiểm soát mọi future extension trước khi extension được phép ảnh hưởng runtime hiện tại.

Future extension mặc định bị chặn.


### 13.23.2. Owner Core

Owner Core tùy extension, nhưng tối thiểu phải có:

Proposed Owner Core.

Security Owner nếu có dữ liệu/kết nối.

Business Owner.


### 13.23.3. Required Resolver

FutureIntegrationIntakeResolver.

ImpactClassificationResolver.

OwnerApprovalResolver.

ExtensionActivationResolver.

ConflictResolver.

RollbackResolver.


### 13.23.4. Required Guard

FutureExtensionBlockedByDefaultGuard.

ImpactClassificationGuard.

OwnerApprovalGuard.

ConflictSuppressionGuard.

NoRuntimeBypassGuard.

NoHardcodeGuard.

ActivationPathGuard.


### 13.23.5. Required Evidence

Integration intake evidence.

Impact classification evidence.

Resolver evidence.

Trace ID evidence.

Conflict suppression evidence.

Rollback evidence.


### 13.23.6. bị chặn If Missing

REL-FUT-001 BLOCK nếu:

Chưa có Owner Core.

Chưa có Source-of-Truth.

Chưa có resolver/guard.

Chưa có impact classification.

Chưa có evidence.

Chưa có smoke.

Chưa có security evidence trong scope.

Extension ảnh hưởng runtime hiện tại khi chưa active.

Consumer tự dùng future rule để xử lý nghiệp vụ hiện tại.


## 14. CROSS-DOMAIN RELEASE DEPENDENCY RULES


### 14.1. Operational Dependency Rule

Không Consumer nào được release nếu phụ thuộc Operational Core mà REL-OPR-001 chưa PASS.

Áp dụng cho:

CRM recommendation.

Gateway bán hàng.

IVR order confirmation.


### 14.2. Recall Dependency Rule

Nếu REL-RCL-001 trả BLOCK / STOP_SALE_REQUIRED / SUPPRESS thì các Consumer sau phải dừng action liên quan:

IVR nếu đơn chưa đủ điều kiện xác nhận.

Payment nếu order bị hủy/khóa.

Shipping nếu chưa giao hoặc đang cần recovery.


### 14.3. Payment Dependency Rule

Không được xác nhận PAID nếu REL-PMT-001 chưa PASS cho payment path tương ứng.

Admin UI.

Commission.


### 14.4. Shipping Dependency Rule

Không được báo ETA, phí ship, COD, tracking nếu REL-SHP-001 chưa PASS cho địa chỉ/khu vực/phương thức giao hàng tương ứng.

Customer Service.


### 14.5. ADS Scale Dependency Rule

ADS chỉ được scale khi REL-ADS-001 PASS và có owner approval.

Không được scale dựa trên:

Comment nhiều.

Inbox nhiều.

Đơn nháp nhiều.

Revenue chưa verified.

AOV chưa xác thực.

Diamond revenue đang conflict.

COD fail chưa phân tích.

Data quality chưa đạt.


### 14.6. MC AI Live Dependency Rule

MC AI chỉ được live khi REL-MCA-001 PASS cho phiên live cụ thể.

MC AI không được dùng script brief của phiên khác, board khác, chương trình khác hoặc SKU khác.

Nếu Live Board thay đổi, MC AI phải resolve lại.


### 14.7. IVR Dependency Rule

IVR chỉ được gọi khi REL-IVR-001 PASS và order đủ điều kiện xác nhận.

IVR không được mở rộng sang:

Sales.

Upsell.

Marketing.

Payment confirmation.

Product consultation.


## 15. PHẦN 2/4 DONE GATE


## PHẦN 2/4 được xem là đạt governance scope khi đã khóa đủ:

Release Gate Registry Principles.

Standard Release Gate Record.

Standard Release Gate Decision.

Global Gate Codes.

Governance Release Gate.

Operational Core Release Gate.

Production Readiness Gate.

Warehouse / Inventory Readiness Gate.

Product Activation / Sellable / Stock Gate.

Traceability / Public Trace Gate.

Recall / Sale Lock / Stop Sale Gate.

Security Release Gate.

Gateway Release Gate.

Payment Release Gate.

Shipping Release Gate.

CRM Release Gate.

Order / Quote / CustomerConfirmation Gate.

AI Advisor Release Gate.

IVR ORDER_CONFIRMATION_ONLY Gate.

MC AI / Live Board / Script Brief Gate.

Customer Identity / Customer Memory Gate.

Member Lifecycle / Rights Gate.

Diamond / Affiliate / Commission Attribution Gate.

Health / Complaint / Privacy Priority Gate.

Official Contact Registry Gate.

Future Governed Extension Gate.

Cross-domain dependency rules.


## 16. PHẦN 2/4 FINAL CONCLUSION


## PHẦN 2/4 của MASTER-07 khóa Release Gate Registry cho toàn bộ domain trọng yếu của Ginsengfood.

Từ điểm này trở đi, mọi domain, pack, channel, campaign, AI, Gateway, CRM, ADS, MC AI, IVR, Payment, Shipping, Order, Operational Core hoặc Future Extension chỉ được xét release nếu có gate tương ứng trong registry và đạt đủ:

Owner Core -> Source-of-Truth -> Resolver -> Guard -> Traceability ID -> Evidence ACCEPTED -> Smoke PASS -> Negative Path PASS -> Security PASS nếu thuộc scope -> Owner Sign-off -> Completion Gate PASS

Nếu thiếu một trong các điều kiện P0, gate mặc định là:


## PHẦN 3/4 — PRODUCTION READINESS MATRIX / E2E RELEASE SMOKE / ADS SCALE GATE / MC AI LIVE GATE / RECALL STOP-SALE CONTROL


## 17. PRODUCTION READINESS MATRIX PRINCIPLES


### 17.1. Production Readiness Matrix Purpose

Production Readiness Matrix là bảng quyết định dùng để xác định từng domain, pack, channel, capability hoặc extension có đủ điều kiện vận hành production-controlled hay chưa.

Matrix này không thay thế Release Gate Registry ở PHẦN 2/4. Matrix này dùng để tổng hợp trạng thái thực tế của các gate trước khi đi đến quyết định:

bị chặn.

ROLLBACK_REQUIRED.

STOP_SALE_REQUIRED.

SCALE_APPROVED.

SCALE_BLOCKED.

LIVE_BLOCKED.

FUTURE_EXTENSION_BLOCKED.


### 17.2. Matrix Decision Rule

Gate tương ứng trong Release Gate Registry đã PASS.

Evidence bắt buộc có trạng thái ACCEPTED.

Smoke bắt buộc đã PASS.

Negative path đã PASS.

Security evidence đã PASS nếu domain thuộc scope security.

E2E smoke đã PASS nếu domain nằm trong luồng trọng yếu.

Owner sign-off đã có.

Completion Gate liên quan đã PASS.

Không còn P0 điểm chặn.

Không có conflict chưa xử lý.

Không có suppression đang active làm vô hiệu release.

Không có stale runtime dependency.

Rollback path đã sẵn sàng nếu domain có tác động production.


### 17.3. Matrix Cannot Promote Status Automatically

Matrix không được tự nâng trạng thái từ:

Production Ready Candidate lên Production Ready.

Production Ready lên Release Approved.

Release Approved lên Go-live Approved.

Pilot Approved lên Scale Approved.

Future Proposed lên Active Governed Extension.

Mọi lần nâng trạng thái phải có:


## 18. GLOBAL PRODUCTION READINESS DECISION MATRIX


### 18.1. Global Readiness Matrix

Domain / Capability

Required Gate

Production Ready khi

bị chặn nếu

MASTER/PACK registry, SOT, Owner, Gate, Evidence mapping đầy đủ

Thiếu SOT, Owner, Gate hoặc evidence

Production, warehouse, inventory, sellable, hold/recall/sale lock guard có evidence

Consumer có thể bán khi Operational Core BLOCK

PO, recipe snapshot, batch, material issue/receipt, QC, release có trace

QC_PASS bị hiểu là RELEASED hoặc batch chưa release vẫn nhập kho

Warehouse receipt, ledger, stock available, reserved/bị chặn stock có audit

Stock không khớp ledger, tồn âm, thiếu stock guard

Base Sellable Gate PASS

SKU chưa sellable, không có price, bị hold/recall/sale lock

Internal trace, public trace whitelist, QR validity có evidence

Public trace lộ dữ liệu nội bộ hoặc trace gap

Recall impact, hold, sale lock, stop sale, suppression hoạt động

SKU/lô bị recall vẫn bán/quote/ADS/CRM/live

Auth, permission, secret, webhook, PII, payment data guard PASS

Secret hardcode, PII leak, webhook không verify

Public/Messenger/handoff/moderation/security/completion PASS

Payment Core, Bank Registry, payment_reference, Accounting Review PASS

Hardcode bank, PaymentReference bị coi là PAID

Shipping eligibility, fee, COD, ETA, tracking PASS

AI/Gateway/CRM bịa ETA/phí ship/tracking

Trigger, suppression, customer context, journey, privacy PASS

CRM spam, gửi khi suppression, gợi ý SKU bị lock

QuoteSnapshot, OrderDraft, CustomerConfirmation, OrderCode PASS

Báo giá không snapshot, tạo order không confirmation

Product knowledge, claim, customer, quote/order/payment/shipping/stop sale boundary PASS

AI tự suy luận runtime, nói như thuốc, bán SKU bị lock

ORDER_CONFIRMATION_ONLY, call log, confirmation result, retry, audit PASS

IVR bán hàng, upsell, xác nhận payment

Verified revenue, ROAS/CPA/AOV, data quality, pilot, owner approval PASS

ADS scale khi revenue chưa verified hoặc SKU không sellable

MC AI Live

Live Board, Script Brief, SKU board, program runtime, stock/recall guard PASS

MC AI nói SKU ngoài board, fake urgency, nói SKU bị lock

Customer Identity

Identity, memory, privacy, consent, recipient distinction PASS

Nhầm khách, lặp PII, dùng memory stale

Member Rights

Tier, lifecycle, rights, grace, dispute, runtime eligibility PASS

Quote bỏ sót quyền lợi hoặc cộng quyền lợi sai policy

Link, bind, attribution, paid eligibility, self-purchase exclusion PASS

Không bind vẫn ghi Diamond path, commission trước PAID

Health boundary, complaint, privacy, counterfeit, handoff PASS

Complaint thật bị xử như troll hoặc safety không thắng sales

Contact registry, purpose, address wording, personal contact suppression PASS

Tự gửi số, dùng địa chỉ cũ, lộ số cá nhân lãnh đạo

ACTIVE_GOVERNED_EXTENSION + đầy đủ intake/evidence/smoke/sign-off

Extension ảnh hưởng runtime hiện tại khi chưa active


### 18.2. Required Decision Output

Mỗi lần đánh giá Production Readiness phải xuất decision record có tối thiểu:

production_readiness_decision_id.

release_gate_id.

decision_state.

decision_reason.

evidence_id list.

smoke_id list.

negative_path_result.

security_result nếu áp dụng.

unresolved_issue list.

rollback_required.

decided_at.

Không có decision record thì không được gọi Production Ready.


## 19. E2E RELEASE SMOKE MODEL


### 19.1. E2E Smoke Purpose

E2E Release Smoke dùng để chứng minh các domain liên kết đúng trong luồng thực tế, không chỉ đúng từng module riêng lẻ.

Unit smoke chỉ chứng minh một phần nhỏ.

E2E smoke chứng minh hệ thống chạy đúng qua nhiều pack, nhiều resolver, nhiều guard và nhiều trạng thái nghiệp vụ.

Đối với luồng trọng yếu, E2E smoke là bắt buộc.


### 19.2. E2E Smoke Required Principles

Mỗi E2E smoke phải có:

scenario_name.

business_domain_scope.

source_of_truth.

owner_core.

resolver path.

guard path.

trace ID chain.

positive path result.

negative path result.

owner_signoff_id nếu dùng cho release.

final decision.

Smoke không có trace ID không được dùng để xét release.

Smoke không có negative path không đủ điều kiện P0.


### 19.3. E2E Smoke Status

Được dùng PASS không

NOT_RUN

Chưa chạy

RUNNING

Đang chạy

Đạt

Có nếu evidence ACCEPTED

FAIL

Bị chặn do thiếu dependency

FLAKY

Không ổn định

PARTIAL

Chỉ chạy một phần

Không dùng cho P0

ACCEPTED

Smoke đã được owner/evidence chấp nhận


## 20. E2E RELEASE SMOKE MATRIX


### 20.1. MAS-REL-SMK-001 — Operational Core To Sellable Smoke

Mục tiêu

Chứng minh sản phẩm chỉ được mở bán khi Operational Core đủ điều kiện.

Scope

Price active.

Hold / Recall / Sale Lock.

Channel eligibility.

Positive Path

SKU được bán khi:

Sản xuất hoàn tất.

Batch released.

Thành phẩm nhập kho.

Stock available > 0.

Giá active.

SKU sellable.

Không hold.

Không recall.

Channel được phép bán.

Negative Path

SKU không được bán khi:

stock_available = 0.

listed_price_status không active.

sellable_status không phải SELLABLE.

Có quality hold.

Có recall hold.

Có sale lock.

Có channel block.

PASS khi

Consumer không thể quote, order, ADS, CRM, MC AI hoặc Landing Page CTA cho SKU bị block.


### 20.2. MAS-REL-SMK-002 — Quote To Order To CustomerConfirmation Smoke

Chứng minh luồng chốt đơn không đi tắt.

Program Runtime.

Order chỉ được tạo khi:

Có QuoteSnapshot hợp lệ.

Quote còn trong thời hạn giữ giá.

Giá chương trình đúng runtime.

Quyền lợi member/Diamond được hiển thị đúng nếu eligible.

VAT hiển thị rõ.

Khách xác nhận đúng.

CustomerConfirmation được ghi nhận.

OrderCode được tạo sau confirmation.

Order bị chặn khi:

Không có QuoteSnapshot.

Quote hết hạn.

Thiếu CustomerConfirmation.

SKU bị stop sale sau khi quote nhưng trước khi confirm.

Thiếu shipping eligibility.

Duplicate order nghi ngờ.

PaymentReference bị hiểu sai là PAID.

Không thể tạo OrderCode nếu chưa có CustomerConfirmation hợp lệ.


### 20.3. MAS-REL-SMK-003 — Payment / Bank Transfer / Accounting Review Smoke

Chứng minh thanh toán không bị xác nhận sai.

Payment method.

Reconcile.

Payment được xác nhận khi:

Payment method hợp lệ.

Bank account lấy từ Company Bank Account Registry.

payment_reference hợp lệ.

transfer memo hợp lệ.

Accounting Review xác nhận.

Payment Core cập nhật trạng thái.

Payment không được xác nhận khi:

Chỉ có lời khách nói đã chuyển.

Chỉ có PaymentReference.

Thiếu Accounting Review.

Tài khoản ngân hàng bị hardcode.

IVR cố xác nhận payment.

Gateway public instruction sai policy.

Không có luồng nào tự đổi trạng thái PAID nếu thiếu Payment Core / Accounting Review confirmation.


### 20.4. MAS-REL-SMK-004 — Shipping / COD / ETA / Tracking Smoke

Chứng minh phí ship, COD, ETA và tracking không bị bịa.

Domestic shipping.

COD.

Shipping được báo khi:

Địa chỉ đủ điều kiện.

Phí ship resolve được.

COD resolve được nếu áp dụng.

ETA lấy từ Shipping Core.

Tracking có nguồn.

Delivery status có audit.

Shipping bị chặn khi:

Địa chỉ chưa đủ thông tin.

Khu vực chưa hỗ trợ.

COD không eligible.

ETA không resolve được.

Tracking chưa có.

International shipping chưa được owner approve.

AI/Gateway/CRM/Order không tự bịa phí ship, ETA, COD hoặc tracking.


### 20.5. MAS-REL-SMK-005 — Gateway Public Comment To Messenger Handoff Smoke

Chứng minh Gateway xử lý đúng public comment và Messenger handoff.

Handoff success/fail.

PII suppression.

Price suppression nếu policy áp dụng.

Gateway đạt khi:

Public comment trả lời đúng boundary.

Giá/chốt đơn/payment không public trái policy.

Handoff thành công mới nói đã gửi Messenger.

Handoff fail không nói đã gửi.

PII không lặp public.

Complaint route đúng.

Troll/abuse route moderation đúng.

Gateway bị chặn khi:

Webhook/security chưa PASS.

Public comment báo giá trái policy.

PII bị lặp public.

Handoff fail nhưng AI nói đã gửi.

Gateway không được gọi PASS nếu Completion Gate chưa PASS.


### 20.6. MAS-REL-SMK-006 — Shipping Eligibility Cross-Pack Smoke

Chứng minh Shipping eligibility được kết nối đúng với các pack liên quan.

Scope bắt buộc

MAS-SMK-006 phải link tối thiểu:

PACK-04.

PACK-01.

PACK-03.

PACK-05.

Shipping eligibility đạt khi:

Order có thông tin giao hàng đủ điều kiện.

Shipping Core resolve được phương thức.

Fee/COD/ETA phù hợp.

Quote/Order hiển thị đúng.

AI/CRM/Gateway không tự suy luận.

Shipping eligibility bị chặn khi:

Thiếu pack dependency.

Shipping Core không trả kết quả.

Địa chỉ không đủ điều kiện.

Fee/ETA stale.

International shipping bị gọi khi chưa approved.

Shipping eligibility không bị hardcode ở Consumer.


### 20.7. MAS-REL-SMK-007 — Recall / Stop Sale / Consumer Suppression Smoke

Chứng minh Recall / Stop Sale thắng toàn bộ sales.

Recall case.

Impact analysis.

Hold.

AI Advisor suppression.

CRM suppression.

ADS suppression.

MC AI suppression.

Gateway suppression.

Landing Page CTA suppression.

Order block.

Khi Recall / Stop Sale được kích hoạt:

SKU/lô liên quan bị khóa.

AI không quote.

CRM không gợi ý.

ADS không chạy/scale.

MC AI không nói/bán.

Gateway không chốt.

Landing Page không CTA mua.

Order không tạo mới.

Smoke FAIL nếu:

SKU bị recall vẫn quote được.

SKU bị sale lock vẫn order được.

ADS vẫn đẩy SKU bị lock.

CRM vẫn gửi gợi ý SKU bị lock.

Landing Page vẫn mở CTA mua.

Gateway vẫn kéo khách chốt SKU bị lock.

Stop Sale được chứng minh trên tất cả Consumer liên quan.


### 20.8. MAS-REL-SMK-008 — ADS Scale Gate Smoke

Chứng minh ADS chỉ được scale khi có dữ liệu verified và owner approval.

Owner approval.

ADS được scale khi:

SKU active/sellable.

Không bị recall/sale lock.

Revenue đã verified.

ROAS/CPA/AOV đủ ngưỡng owner quyết.

Funnel metrics đủ chất lượng.

COD Fail không vượt ngưỡng hoặc đã được owner chấp nhận.

Data quality đạt.

Pilot 7–14 ngày có evidence.

ADS bị block scale khi:

Data quality không đạt.

SKU không sellable.

SKU bị stop sale.

Attribution conflict chưa xử lý.

Diamond revenue conflict.

COD Fail cao chưa review.

Không có pilot evidence.

Không có owner approval.

Không thể tăng ngân sách/scale nếu thiếu verified revenue và owner approval.


### 20.9. MAS-REL-SMK-009 — MC AI Live Board / Script Brief Smoke

Chứng minh MC AI chỉ nói theo board và script brief hợp lệ.

mc_ai_script_brief_id.

Live session.

SKU hero.

SKU hỗ trợ.

SKU restricted.

Stock guard.

Recall / Sale Lock guard.

Live moderation.

MC AI được hoạt động khi:

Có Daily Live Product Board hợp lệ.

Có mc_ai_script_brief_id hợp lệ.

SKU nằm trong board.

SKU không restricted.

Program runtime hợp lệ.

Stock/Sellable Guard PASS.

Recall/Sale Lock Guard PASS.

Không tạo fake urgency.

MC AI bị chặn khi:

Không có board.

Không có script brief hợp lệ.

Nói SKU ngoài board.

Nói SKU restricted.

Nói giá khi thiếu QuoteSnapshot/Program Runtime.

Nói hàng còn khi Stock Guard chưa PASS.

Nói SKU bị recall/sale lock.

Tạo fake urgency.

MC AI bị buộc dừng đúng lúc khi board, stock, recall hoặc sale lock thay đổi.


### 20.10. MAS-REL-SMK-010 — IVR ORDER_CONFIRMATION_ONLY Smoke

Chứng minh IVR chỉ xác nhận đơn, không bán hàng, không upsell, không xác nhận payment.

PACK-09 IVR_ORDER_CONFIRMATION.

Customer phone.

Call attempt.

Confirmation result.

Retry policy.

Call log.

IVR được gọi khi:

Có OrderConfirmationDraft/OrderCode hợp lệ.

Customer phone hợp lệ.

Call frequency hợp lệ.

Nội dung chỉ xác nhận đơn.

IVR bị block khi:

Không có order đủ điều kiện.

IVR cố bán hàng.

IVR upsell.

IVR được chứng minh đúng ORDER_CONFIRMATION_ONLY.


## 21. ADS SCALE GATE CONTROL


### 21.1. ADS Scale Gate Purpose

ADS Scale Gate kiểm soát việc tăng ngân sách, mở rộng chiến dịch, nhân rộng campaign, mở thêm page/live/landing hoặc đẩy mạnh traffic.

Scale không được quyết định bằng cảm tính.

Scale phải dựa trên verified revenue, funnel quality, operational readiness và owner approval.


### 21.2. ADS Scale Gate Minimum Metrics

Một ADS Scale Gate hợp lệ phải có tối thiểu:

Metric

Gate yêu cầu

Ads Spend

Chi phí quảng cáo

Có nguồn và audit

Revenue Verified

Doanh thu đã xác thực

ROAS

Hiệu quả doanh thu/chi phí

CPA

Chi phí chuyển đổi

AOV

Giá trị đơn trung bình

Boxes/Order

Số hộp mỗi đơn

Comment Rate

Tỷ lệ bình luận

Theo dõi

Inbox Rate

Tỷ lệ vào inbox

Quote Rate

Tỷ lệ báo giá

Order Rate

Tỷ lệ tạo đơn

Verified Rate

Tỷ lệ xác thực đơn/doanh thu

COD Fail

Rủi ro COD

CRM Revenue

Doanh thu CRM

Phân tách khi có

Diamond Revenue

Doanh thu Diamond

Data Quality

Chất lượng dữ liệu

Pilot 7–14 ngày

Giai đoạn thử nghiệm

Owner Approval

Phê duyệt scale


### 21.3. ADS Scale Decision States

Được scale không

NOT_READY

Chưa đủ dữ liệu

PILOT_RUNNING

Đang pilot 7–14 ngày

Không scale lớn

WAITING_VERIFIED_REVENUE

Chưa xác thực doanh thu

WAITING_DATA_QUALITY

Chưa đạt chất lượng dữ liệu

WAITING_OWNER_APPROVAL

Chờ owner duyệt

SCALE_READY_CANDIDATE

Có thể xét scale

SCALE_APPROVED

Được scale

SCALE_BLOCKED

Bị chặn scale

SCALE_PAUSED

Tạm dừng scale

SCALE_ROLLBACK_REQUIRED

Cần rollback scale


### 21.4. ADS Scale điểm chặn

ADS Scale bị BLOCK nếu:

SKU không active.

SKU bị recall.

SKU bị sale lock.

Stock Guard không PASS.

ROAS/CPA/AOV không đạt ngưỡng owner đặt ra.

Attribution conflict chưa giải quyết.

Diamond revenue conflict chưa giải quyết.

COD Fail vượt ngưỡng chưa review.

Quote/Order data không trace được.

Payment verified không khớp doanh thu.

Pilot 7–14 ngày chưa đủ evidence.

Gateway chưa PASS nhưng campaign cần Gateway.

Payment/Shipping/Order gate chưa PASS nhưng campaign dẫn đến chốt đơn.

Recall/Stop Sale negative smoke chưa PASS.


### 21.5. ADS Scale Approval Rule

ADS chỉ được SCALE_APPROVED khi:

REL-ADS-001 PASS.

REL-PAS-001 PASS cho SKU.

REL-RCL-001 không có active lock.

REL-ORD-001 PASS nếu campaign có chốt đơn.

REL-PMT-001 PASS nếu campaign có thu tiền.

REL-SHP-001 PASS nếu campaign có giao hàng.

REL-GTW-001 PASS nếu campaign dùng Gateway/Messenger.

Verified revenue đạt.

Pilot 7–14 ngày evidence ACCEPTED.

owner_signoff_id hợp lệ.

Không có owner_signoff_id thì không scale.


## 22. MC AI LIVE GATE CONTROL


### 22.1. MC AI Live Gate Purpose

MC AI Live Gate kiểm soát việc MC AI tham gia hỗ trợ nội dung live để tránh:

Nói sai SKU.

Nói SKU không nằm trong board.

Nói giá khi chưa có runtime hợp lệ.

Nói hàng còn khi chưa có stock guard.

Tiếp tục bán SKU bị recall/sale lock.

Làm rối Gateway/Messenger handoff.

Vượt ranh giới claim/safety.


### 22.2. MC AI Live Required Inputs

Trước mỗi phiên live, MC AI phải có:

live_session_id.

daily_live_product_board_id.

hero_sku list.

support_sku list.

restricted_sku list.

program_runtime_status.

quote_boundary_policy.

stock_guard_status.

recall_hold_status.

sale_lock_status.

fake_urgency_policy.

moderation_policy.

Thiếu một input P0 thì MC AI Live Gate BLOCK.


### 22.3. MC AI Allowed Actions

MC AI chỉ được:

Gợi ý lời dẫn theo script brief hợp lệ.

Nhắc đúng SKU trong board.

Nhắc đúng điểm mạnh public-safe.

Dẫn khách về Messenger theo policy.

Hỗ trợ nhịp live trong boundary.

Dừng nói SKU khi guard BLOCK.

Cảnh báo nội bộ khi board/stock/recall/sale lock thay đổi.

MC AI không được:

Tự thêm SKU ngoài board.

Tự tạo giá.

Tự quote riêng cho khách.

Tự chốt đơn.

Tự xác nhận thanh toán.

Tự nói còn hàng nếu Stock Guard chưa PASS.

Tự nói “sắp hết” nếu runtime không xác nhận.

Tự dùng claim điều trị.

Tự bỏ qua recall/sale lock.

Tự mở chương trình live ngoài board.


### 22.4. MC AI Live Stop Rule

MC AI phải dừng nói/bán SKU ngay khi:

SKU bị remove khỏi Daily Live Product Board.

Program Runtime không còn active.

Stock Guard BLOCK.

Sellable Guard BLOCK.

Recall Hold active.

Sale Lock active.

Stop Sale active.

Claim Guard BLOCK.

Moderation Guard yêu cầu suppress.

Owner pause live SKU.

MC AI không được chờ đến hết phiên live mới dừng.


## 23. RECALL / STOP-SALE CONTROL


### 23.1. Recall / Stop-Sale Purpose

Recall / Stop-Sale Control là lớp bảo vệ cao nhất đối với sản phẩm, khách hàng, thương hiệu và pháp lý vận hành.

Khi Recall / Stop Sale active, mọi sales action phải dừng trong scope bị ảnh hưởng.


### 23.2. Stop-Sale Scope Types

Stop Sale có thể áp dụng theo:

SKU.

Lot.

Trade item.

Channel.

Program.

ADS campaign.

Customer segment.

Region.

Time window.

Full product line.

Scope phải được xác định rõ bằng trace ID.


### 23.3. Stop-Sale Required Trace IDs

Mỗi Stop-Sale decision phải trace được:

recall_case_id nếu phát sinh từ recall.

sale_lock_id.

affected_sku_id.

affected_batch_id nếu có.

affected_lot_id nếu có.

affected_channel_id nếu có.

affected_program_id nếu có.

impact_analysis_id.

suppression_event_id.

owner_signoff_id nếu cần owner decision.

Không có trace ID thì Stop Sale không đạt release evidence.


### 23.4. Stop-Sale Consumer Suppression Matrix

Khi Stop Sale active phải làm gì

Không tư vấn bán, không quote, không tạo order draft cho SKU/lô bị lock

Không kéo chốt SKU bị lock, không public CTA mua

Dừng message gợi ý/chốt mua SKU bị lock

Dừng campaign hoặc loại SKU bị lock khỏi campaign

Dừng nói/bán SKU bị lock trong live

Gỡ/ẩn CTA mua hoặc hiển thị trạng thái không bán theo policy

Order

Chặn order mới hoặc yêu cầu review với order đang dở

Không tạo payment mới cho order bị chặn

Không đẩy giao nếu order bị hold theo policy

Không gọi xác nhận đơn chưa đủ điều kiện

Hiển thị lock rõ cho operator

Reporting

Ghi nhận impact và revenue affected


### 23.5. Recall / Stop-Sale Release điểm chặn

Release bị BLOCK nếu:

Recall không chặn được quote.

Recall không chặn được order.

Recall không chặn được ADS.

Recall không chặn được CRM.

Recall không chặn được MC AI.

Recall không chặn được Landing Page CTA.

Recall không chặn được Gateway chốt đơn.

Stop Sale không trace được scope.

Consumer không nhận được suppression.

Không có owner decision cho case nghiêm trọng.

Closed with residual risk nhưng thiếu residual note.

Recovery chưa evidence nhưng đã mở bán lại.


### 23.6. Reopen After Stop-Sale Rule

SKU/lô/channel/program chỉ được mở bán lại khi:

Recall/Hold/Sale Lock được giải quyết.

Recovery evidence ACCEPTED.

QC/Operational owner xác nhận.

Traceability không còn gap nghiêm trọng.

Product Activation/Sellable PASS lại.

Stock Guard PASS lại.

Consumer suppression được gỡ có audit.

Release decision record được cập nhật.

E2E negative path được rerun nếu scope P0.

Không được mở bán lại bằng thao tác thủ công không audit.


## 24. PRODUCTION READINESS FINAL MATRIX


### 24.1. Readiness Decision Levels

Điều kiện tối thiểu

Level 0 — Documentation Ready

Tài liệu governance/pack đã sẵn

Không đủ production

Level 1 — Implementation Candidate

Có triển khai sơ bộ

Chưa đủ release

Level 2 — Smoke Ready

Smoke chính PASS

Chưa đủ nếu thiếu evidence accepted

Level 3 — Evidence Ready

Evidence ACCEPTED

Level 4 — Production Ready Candidate

Chưa release

Level 5 — Production Ready

Đủ production readiness

Chưa go-live

Level 6 — Release Approved

Owner duyệt release

Chưa go-live nếu chưa final gate

Level 7 — Go-live Approved

Được vận hành thật

Có thể live trong scope

Level 8 — Live

Đang vận hành

Phải monitoring

Level 9 — Scale Approved

Chỉ áp dụng scope scale

Level 10 — Rollback / Stop Required

Cần dừng/rollback

Chặn release/scale/live


### 24.2. Minimum Gates For Production Ready

Một release candidate chỉ được đánh dấu Production Ready khi tối thiểu các gate sau đạt theo scope:

REL-GOV-001.

REL-OPR-001 nếu có sản phẩm/vận hành.

REL-PAS-001 nếu có bán hàng/hiển thị SKU.

REL-RCL-001 nếu có sản phẩm thật.

REL-SEC-001 nếu có dữ liệu/kênh/integration.

REL-ORD-001 nếu có chốt đơn.

REL-PMT-001 nếu có thu tiền.

REL-SHP-001 nếu có giao hàng.

REL-GTW-001 nếu có Gateway.

REL-AIA-001 nếu có AI Advisor.

REL-CRM-001 nếu có CRM.

REL-ADS-001 nếu có ADS.

REL-MCA-001 nếu có MC AI Live.

REL-IVR-001 nếu có IVR.

REL-FUT-001 nếu có future extension.


### 24.3. Minimum Gates For Go-live Approved

Security PASS trong scope.

Payment PASS nếu thu tiền.

Shipping PASS nếu giao hàng.

Order PASS nếu chốt đơn.

Product Activation / Sellable PASS nếu bán SKU.

Recall / Stop-Sale PASS nếu có sản phẩm.

Gateway PASS nếu mở channel.

ADS Scale Gate PASS nếu scale ADS.

MC AI Live Gate PASS nếu live có MC AI.

IVR ORDER_CONFIRMATION_ONLY PASS nếu gọi tự động.

Final Owner sign-off.


## 25. PHẦN 3/4 DONE GATE


## PHẦN 3/4 được xem là đạt governance scope khi đã khóa đủ:

Production Readiness Matrix Principles.

Global Production Readiness Decision Matrix.

Required Decision Output.

E2E Release Smoke Model.

E2E Smoke Status.

Operational Core To Sellable Smoke.

Quote To Order To CustomerConfirmation Smoke.

Payment / Bank Transfer / Accounting Review Smoke.

Shipping / COD / ETA / Tracking Smoke.

Gateway Public Comment To Messenger Handoff Smoke.

Shipping Eligibility Cross-Pack Smoke.

Recall / Stop Sale / Consumer Suppression Smoke.

ADS Scale Gate Smoke.

MC AI Live Board / Script Brief Smoke.

IVR ORDER_CONFIRMATION_ONLY Smoke.

ADS Scale Gate Control.

MC AI Live Gate Control.

Recall / Stop-Sale Control.

Production Readiness Final Matrix.

Minimum Gates For Production Ready.

Minimum Gates For Go-live Approved.


## 26. PHẦN 3/4 FINAL CONCLUSION


## PHẦN 3/4 của MASTER-07 khóa cơ chế đánh giá Production Readiness, E2E Release Smoke, ADS Scale Gate, MC AI Live Gate và Recall / Stop-Sale Control.

Từ điểm này trở đi, không domain nào được gọi Production Ready nếu chỉ pass test đơn lẻ. Các domain trọng yếu phải chứng minh bằng E2E smoke có trace, audit, evidence, negative path và owner sign-off.

Đặc biệt:

Operational Core quyết định sự thật sản phẩm.

Product Activation / Sellable quyết định SKU có được bán hay không.

Recall / Stop Sale thắng mọi hành vi sales.

ADS không được scale nếu thiếu verified revenue, data quality, pilot evidence và owner approval.

MC AI không được live nếu thiếu Daily Live Product Board, mc_ai_script_brief_id, stock guard, recall guard và fake urgency guard.

IVR chỉ được vận hành trong phạm vi ORDER_CONFIRMATION_ONLY.

Go-live chỉ được xét khi Production Ready, Release Approved, Completion Gate, Security, Payment, Shipping, Order, Recall/Stop-Sale, Gateway, ADS, MC AI, IVR và Owner Sign-off đạt đúng scope.

Mọi quyết định release thật phải đi qua chuỗi:

Gate PASS -> Evidence ACCEPTED -> E2E Smoke PASS -> Negative Path PASS -> Security PASS nếu thuộc scope -> Owner Sign-off -> Completion Gate PASS -> Release Approved -> Go-live Approved.


## PHẦN 4/4 — FINAL GLOBAL RELEASE DONE GATE / GO-LIVE DECISION / ROLLBACK CONTROL / MASTER-07 FINAL CONCLUSION


## 27. FINAL GLOBAL RELEASE DONE GATE PRINCIPLES


### 27.1. Final Global Release Done Gate Purpose

Final Global Release Done Gate là điểm kiểm soát cuối cùng trước khi một release, domain, pack, channel, campaign, capability hoặc future extension được phép chuyển sang trạng thái Release Approved hoặc Go-live Approved.

Gate này không thay thế các gate domain ở PHẦN 2/4 và không thay thế E2E Smoke ở PHẦN 3/4.

Final Global Release Done Gate chỉ được dùng để tổng hợp toàn bộ trạng thái đã được chứng minh bằng:

Runtime Resolver.

Negative Path PASS.

Security PASS nếu thuộc scope.

Owner Sign-off.

Rollback readiness.

Monitoring readiness.

Go-live decision.

Nếu thiếu một thành phần P0, Final Global Release Done Gate mặc định bị chặn.


### 27.2. Final Gate Is Not A Checklist Text

Final Global Release Done Gate không phải là một checklist chữ để đánh dấu thủ công.

Gate này phải được chứng minh bằng evidence có trace.

Các nội dung sau không đủ điều kiện gọi Final Gate PASS:

Dev báo đã làm xong.

Owner nói đồng ý nhưng chưa có owner_signoff_id.

Evidence chưa ACCEPTED.

Security chưa PASS.

Payment chưa có Accounting Review evidence.

Shipping chưa có Shipping Core evidence.

Gateway chưa có Completion Gate PASS.

ADS chưa có verified revenue và owner approval.

MC AI chưa có Daily Live Product Board và Script Brief hợp lệ.


### 27.3. Final Gate Default Status

Trạng thái mặc định của Final Global Release Done Gate là:

Item

Default Status

ADS_SCALE_STATUS

NOT_APPROVED

MC_AI_LIVE_STATUS

FUTURE_EXTENSION_STATUS

Chỉ được thay đổi trạng thái khi có đủ evidence, smoke, audit, owner sign-off và Completion Gate tương ứng.


## 28. FINAL GLOBAL RELEASE DONE GATE MATRIX


### 28.1. Required Final Gate Conditions

Một release chỉ được Final Gate PASS khi đồng thời đạt đủ:

Nhóm điều kiện

Điều kiện bắt buộc

Nếu thiếu

MASTER/PACK registry rõ, Source-of-Truth rõ, Owner Core rõ

Dependency

Cross-pack dependency đã map đủ

Runtime

Resolver bắt buộc hoạt động

Guard

Guard bắt buộc hoạt động

P0 ID trace được

Audit

Audit đầy đủ

Evidence

Smoke

Smoke PASS

Negative path PASS

Security PASS nếu thuộc scope

Operational

Operational Core PASS nếu có sản phẩm thật

Product Sellable

Product Activation / Sellable PASS nếu có bán SKU

Recall / Stop Sale

Recall / Stop-Sale PASS nếu có sản phẩm thật

Quote / Order / CustomerConfirmation PASS nếu có chốt đơn

Payment PASS nếu có thu tiền

Shipping PASS nếu có giao hàng

Gateway PASS nếu mở kênh

CRM PASS nếu gửi chăm sóc / trigger

AI Advisor PASS nếu AI tư vấn/chốt

ADS Scale PASS nếu scale quảng cáo

MC AI Live PASS nếu dùng trong live

IVR ORDER_CONFIRMATION_ONLY PASS nếu gọi tự động

ACTIVE_GOVERNED_EXTENSION nếu extension ảnh hưởng runtime

Owner

Owner sign-off hợp lệ

Rollback

Rollback path sẵn sàng

Monitoring

Monitoring / incident control sẵn sàng

Completion Gate

Completion Gate PASS


### 28.2. Final Gate Decision States

Cho phép đi tiếp

NOT_STARTED

Chưa bắt đầu đánh giá

Đang đánh giá

Có thể xét tiếp


## Final_Gate_Pass

Đạt Final Gate

Có thể xét Release Approved


## Final_Gate_Rejected


### 28.3. Final Gate PASS Rule

Final Global Release Done Gate chỉ PASS khi:

Tất cả gate P0 trong scope đều PASS.

Tất cả evidence P0 đều ACCEPTED.

Tất cả smoke P0 đều PASS.

Tất cả negative path P0 đều PASS.

Security PASS nếu domain có dữ liệu, kênh public, payment, integration hoặc admin access.

Owner sign-off đầy đủ.

Không còn điểm chặn.

Không còn conflict chưa xử lý.

Không có future extension chưa approved nhưng đang ảnh hưởng runtime.

Nếu một trong các điều kiện trên không đạt, Final Gate không được PASS.


## 29. PRODUCTION READY DECISION CONTROL


### 29.1. Production Ready Decision Purpose

Production Ready Decision xác nhận một release candidate đủ điều kiện kỹ thuật và vận hành để chạy trong production-controlled environment.

Production Ready không đồng nghĩa Go-live Approved.


### 29.2. Production Ready Required Inputs

Production Ready Decision phải có:

release_candidate_id.

domain_scope.

required_gate_result.

evidence_result.

smoke_result.

security_result nếu thuộc scope.

operational_result nếu thuộc scope.

rollback_readiness_result.

monitoring_readiness_result.

owner_review_result.


### 29.3. Production Ready PASS Conditions

Final Gate chưa BLOCK.

Required domain gate đã PASS.

Security đã PASS nếu thuộc scope.

Operational readiness đã PASS nếu có sản phẩm thật.

Payment readiness đã PASS nếu có thu tiền.

Shipping readiness đã PASS nếu có giao hàng.

Order readiness đã PASS nếu có chốt đơn.

Gateway readiness đã PASS nếu có kênh.

ADS readiness đã PASS nếu có ADS.

MC AI readiness đã PASS nếu có live.

IVR readiness đã PASS nếu có gọi tự động.

Owner review không còn điểm chặn.


### 29.4. Production Ready Block Conditions

Implementation chưa có evidence.

Smoke chưa PASS.

Security chưa PASS trong scope security.

Owner sign-off thiếu.

Product Sellable chưa PASS nhưng Consumer có bán.

Recall / Stop Sale chưa có negative path.

Payment chưa qua Payment Core / Accounting Review.

Shipping chưa qua Shipping Core.

ADS chưa có verified revenue nhưng muốn scale.

MC AI chưa có board/script brief hợp lệ.

Có hardcode runtime data.


## 30. RELEASE APPROVED DECISION CONTROL


### 30.1. Release Approved Purpose

Release Approved là quyết định cho phép một release candidate được đưa vào kế hoạch phát hành.

Release Approved chỉ được xét sau khi Production Ready đạt.

Release Approved không tự động cho phép Go-live.


### 30.2. Release Approved Required Inputs

Release Approved Decision phải có:

release_approval_id.

final_gate_result.

business_risk_assessment.

operational_risk_assessment.

security_risk_assessment.

payment_risk_assessment nếu thuộc scope.

shipping_risk_assessment nếu thuộc scope.

recall_stop_sale_readiness_result nếu thuộc scope.

rollback_plan_result.

monitoring_plan_result.

support_readiness_result.

release_window.

release_scope.


### 30.3. Release Approved PASS Conditions

Final Global Release Done Gate PASS.

Business Owner chấp nhận scope release.

Technical Owner xác nhận triển khai đủ điều kiện.

Security Owner xác nhận security nếu thuộc scope.

Operational Owner xác nhận operational readiness nếu có sản phẩm thật.

Payment Owner xác nhận payment readiness nếu có thu tiền.

Shipping Owner xác nhận shipping readiness nếu có giao hàng.

Gateway Owner xác nhận channel readiness nếu có Gateway.

ADS Owner xác nhận nếu release có ADS.

MC AI Owner xác nhận nếu release có live support.

IVR Owner xác nhận nếu release có IVR.

Rollback plan sẵn sàng.


### 30.4. Release Approved Block Conditions

Final Gate chưa PASS.

Có điểm chặn chưa đóng bằng evidence.

Gateway còn bị chặn nhưng release cần Gateway.

Payment chưa PASS nhưng release có thu tiền.

Shipping chưa PASS nhưng release có giao hàng.

Order chưa PASS nhưng release có chốt đơn.

Recall / Stop Sale chưa PASS nhưng release có SKU thật.

ADS Scale Gate chưa PASS nhưng release yêu cầu scale.

MC AI Live Gate chưa PASS nhưng release có MC AI Live.

IVR Gate chưa PASS nhưng release có gọi tự động.

Future extension chưa approved nhưng nằm trong release scope.

Rollback không sẵn sàng.

Monitoring không sẵn sàng.


## 31. GO-LIVE APPROVED DECISION CONTROL


### 31.1. Go-live Approved Purpose

Go-live Approved là quyết định cuối cùng cho phép hệ thống, domain, channel, campaign, capability hoặc extension vận hành thật với khách hàng thật, dữ liệu thật, giao dịch thật hoặc tác động vận hành thật.

Go-live Approved chỉ được xét sau khi Release Approved đạt.


### 31.2. Go-live Required Inputs

Go-live Decision phải có:

go_live_approval_id.

go_live_scope.

go_live_time_window.

production_environment_status.

monitoring_status.

rollback_status.

incident_response_status.

support_owner_status.

customer_impact_assessment.

revenue_impact_assessment nếu có bán hàng.

payment_readiness_status nếu có thu tiền.

shipping_readiness_status nếu có giao hàng.

ADS scale status nếu có scale.

MC AI live status nếu có live.

IVR status nếu có gọi tự động.

final_owner_signoff_id.


### 31.3. Go-live Approved PASS Conditions

Go-live chỉ được APPROVED khi:

Monitoring active.

Rollback path active.

Incident response sẵn sàng.

Support owner sẵn sàng.

Payment sẵn sàng nếu thu tiền.

Shipping sẵn sàng nếu giao hàng.

Order flow sẵn sàng nếu chốt đơn.

Recall / Stop Sale control PASS nếu có sản phẩm thật.

Gateway PASS nếu mở kênh.

CRM PASS nếu gửi message.

AI Advisor PASS nếu AI tư vấn/chốt.

ADS Scale Gate PASS nếu scale.

MC AI Live Gate PASS nếu live.

Future Extension PASS nếu extension nằm trong scope.

Final Owner sign-off hợp lệ.

Không còn điểm chặn P0.


### 31.4. Go-live Block Conditions

Go-live bắt buộc BLOCK nếu:

Payment chưa PASS nhưng có thu tiền.

Shipping chưa PASS nhưng có giao hàng.

Product Sellable chưa PASS nhưng có bán.

Recall / Stop Sale chưa PASS nhưng có sản phẩm thật.

Order chưa PASS nhưng có chốt đơn.

Monitoring chưa active.

Rollback chưa active.

ADS muốn scale nhưng chưa SCALE_APPROVED.

MC AI muốn live nhưng chưa MC_AI_LIVE_APPROVED.

IVR muốn gọi nhưng chưa IVR_ORDER_CONFIRMATION_ONLY_PASS.

Future extension muốn runtime nhưng chưa ACTIVE_GOVERNED_EXTENSION.

Có Consumer tự suy luận rule kinh doanh ngoài boundary.


## 32. ROLLBACK CONTROL


### 32.1. Rollback Control Purpose

Rollback Control dùng để dừng, quay lui, vô hiệu hóa hoặc cô lập release khi phát hiện lỗi, rủi ro, điểm chặn hoặc tác động ngoài phạm vi được approved.

Rollback không chỉ là thao tác kỹ thuật.

Rollback là quyết định governance có trace, audit, owner và evidence.


### 32.2. Rollback Required Triggers

Rollback bắt buộc được xét khi xảy ra một trong các tình huống:

Payment sai trạng thái.

Order tạo không có CustomerConfirmation.

AI báo giá sai do thiếu QuoteSnapshot.

Shipping ETA/phí ship/tracking bị bịa hoặc sai nguồn.

Product bị recall nhưng Consumer vẫn bán.

ADS scale khi revenue chưa verified.

MC AI nói SKU ngoài board hoặc SKU bị sale lock.

Gateway public PII, giá, payment instruction trái policy.

IVR bán hàng, upsell hoặc xác nhận payment.

Security leak.

Secret bị lộ hoặc hardcode.

Customer identity bị nhầm.

Member/Diamond benefit tính sai nghiêm trọng.

Commission tạo sai khi chưa PAID hoặc không bind.

Future extension ảnh hưởng runtime khi chưa approved.

Completion Gate bị gọi PASS sai.

Go-live ngoài scope đã ký.

Monitoring phát hiện lỗi P0.


### 32.3. Rollback Scope Types

Rollback có thể áp dụng theo:

Full system.

Domain.

Pack.

Page.

Shipping method.

CRM journey.

AI Advisor capability.

MC AI script.

IVR call flow.

Future extension.

Rollback scope phải được xác định rõ, không rollback mơ hồ.


### 32.4. Rollback Decision Record

Mỗi rollback phải có:

rollback_decision_id.

rollback_trigger.

affected_scope.

affected_domain_id.

affected_release_gate_id.

affected_consumer list.

severity.

customer_impact.

revenue_impact nếu có.

operational_impact nếu có.

suppression_required.

stop_sale_required nếu có.

owner_decision.

rollback_action.

rollback_started_at.

rollback_completed_at nếu hoàn tất.

Không có Rollback Decision Record thì không được xem là rollback hợp lệ.


## 33. ROLLBACK MATRIX BY DOMAIN


### 33.1. Operational / Sellable Rollback

Rollback Operational / Sellable bắt buộc nếu:

Sản phẩm bị mở bán khi chưa đủ Base Sellable Gate.

Stock available sai.

Giá active sai.

SKU bị hold/recall/sale lock vẫn bán.

Channel block không có hiệu lực.

Rollback action:

Stop Sale SKU/channel liên quan.

Suppress AI/CRM/ADS/MC AI/Gateway/Landing Page.

Chặn quote/order mới.

Owner review.

Re-run Operational Core To Sellable Smoke.


### 33.2. Order / Quote Rollback

Rollback Order / Quote bắt buộc nếu:

Quote không có QuoteSnapshot.

Quote hết hạn vẫn được dùng.

Order tạo trước CustomerConfirmation.

OrderCode tạo sai.

Duplicate order không bị chặn.

VAT hiển thị sai hoặc mơ hồ.

Pause order creation trong scope lỗi.

Review affected order.

Suppress payment request nếu order chưa hợp lệ.

Notify customer service queue nếu cần.

Re-run Quote To Order To CustomerConfirmation Smoke.


### 33.3. Payment Rollback

Rollback Payment bắt buộc nếu:

PAID được set khi chưa có Payment Core / Accounting Review.

Bank account bị hardcode.

Transfer memo/payment_reference sai.

Reconcile sai.

AI/Gateway/IVR xác nhận thanh toán sai.

Freeze payment confirmation trong scope lỗi.

Route Accounting Review.

Void hoặc review payment status sai theo governance.

Suppress AI/IVR/Gateway payment confirmation.

Re-run Payment / Bank Transfer / Accounting Review Smoke.


### 33.4. Shipping Rollback

Rollback Shipping bắt buộc nếu:

ETA bị bịa.

Fee sai nguồn.

COD eligibility sai.

Tracking sai.

International shipping bị mở khi chưa approved.

Shipping eligibility không qua Shipping Core.

Suppress ETA/fee/tracking statement.

Route Customer Service review.

Pause shipping confirmation trong scope lỗi.

Re-run Shipping / COD / ETA / Tracking Smoke.

Re-run MAS-SMK-006 nếu lỗi liên quan dependency pack.


### 33.5. Gateway Rollback

Rollback Gateway bắt buộc nếu:

Public comment lộ PII.

Payment instruction public trái policy.

Webhook/security fail.

Disable affected gateway automation.

Suppress risky public replies.

Route to manual moderation / CSKH.

Re-run Gateway Public Comment To Messenger Handoff Smoke.

Re-open Completion Gate review.


### 33.6. CRM Rollback

Rollback CRM bắt buộc nếu:

CRM spam.

CRM gợi ý SKU không sellable.

CRM dùng sai customer memory.

CRM bỏ qua privacy opt-out.

Pause affected CRM journey.

Suppress message jobs.

Review customer memory / identity.

Re-run CRM smoke.

Audit impacted customers.


### 33.7. AI Advisor Rollback

Rollback AI Advisor bắt buộc nếu:

AI bán SKU bị stop sale.

AI báo giá không QuoteSnapshot.

AI tạo order không CustomerConfirmation.

AI xác nhận PAID sai.

AI bịa shipping.

AI gửi contact không qua resolver.

AI xử lý complaint sai.

Disable affected AI capability.

Force handoff nếu cần.

Suppress quote/order/payment/shipping statements trong scope lỗi.

Re-run AI Advisor Release Smoke.

Owner review claim/runtime boundary.


### 33.8. ADS Rollback

Rollback ADS bắt buộc nếu:

Diamond revenue conflict chưa xử lý.

COD Fail vượt ngưỡng mà chưa review.

Pause campaign hoặc giảm scale trong scope lỗi.

Freeze new scale decisions.

Re-run ADS Scale Gate Smoke.

Reconcile revenue/attribution.


### 33.9. MC AI Rollback

Rollback MC AI bắt buộc nếu:

MC AI nói giá khi thiếu QuoteSnapshot / Program Runtime.

MC AI tiếp tục nói SKU bị recall/sale lock.

Pause MC AI for live session.

Remove affected script brief.

Force board re-resolve.

Suppress SKU statement.

Re-run MC AI Live Board / Script Brief Smoke.


### 33.10. IVR Rollback

Rollback IVR bắt buộc nếu:

IVR bán hàng.

IVR gọi khi order chưa đủ điều kiện.

IVR đổi order state ngoài phạm vi xác nhận đơn.

IVR không có call log.

IVR không có confirmation result.

Pause IVR call flow.

Disable affected call campaign.

Review call logs.

Re-run IVR ORDER_CONFIRMATION_ONLY Smoke.

Owner sign-off lại trước khi mở.


### 33.11. Future Extension Rollback

Rollback Future Extension bắt buộc nếu:

Extension ảnh hưởng runtime hiện tại khi chưa ACTIVE_GOVERNED_EXTENSION.

Extension tự thành Source-of-Truth.

Extension tự làm Owner Core.

Extension bypass resolver/guard.

Extension hardcode runtime data.

Extension gây conflict chưa suppress.

Extension mở rộng scope không owner approve.

Disable extension.

Suppress extension output.

Restore current governed rule.

Re-run Future Extension Gate.

Owner review trước khi xét lại activation.


## 34. GO-LIVE MONITORING / INCIDENT CONTROL


### 34.1. Monitoring Purpose

Monitoring dùng để phát hiện lỗi sau Go-live và đảm bảo release không vượt khỏi scope đã approved.

Monitoring là điều kiện bắt buộc của Go-live Approved.


### 34.2. Required Monitoring Areas

Tối thiểu phải monitoring:

Gateway handoff success/fail.

Public reply policy violation.

AI quote/order/payment/shipping boundary.

Product sellable state.

Recall / Sale Lock active.

Order creation failure.

Payment confirmation mismatch.

Shipping ETA/tracking mismatch.

CRM suppression violation.

ADS revenue verification.

ADS data quality.

MC AI board/script violation.

IVR call result / retry / violation.

Security alert.

PII leakage.

Customer complaint escalation.

Future extension conflict.


### 34.3. Incident Severity

Severity

Action

S0

Rủi ro nghiêm trọng: payment/security/recall/privacy lớn

Stop / Rollback ngay

S1

Ảnh hưởng khách hàng hoặc giao dịch thật

Pause scope lỗi, owner review

S2

Lỗi vận hành có workaround

Fix có kiểm soát, monitor

S3

Lỗi nhỏ không ảnh hưởng P0

Ghi nhận, xử lý theo backlog

S4

Quan sát / cải tiến

Không ảnh hưởng release


### 34.4. Incident Response Requirements

Mỗi incident phải có:

incident_id.

affected_customer_count nếu có.

affected_order_count nếu có.

affected_revenue nếu có.

affected_sku/batch/lot nếu có.

root cause preliminary.

containment action.

owner_assigned.

closure decision.

Incident P0/S0/S1 không được đóng nếu thiếu evidence và owner sign-off.


## 35. GLOBAL NO-HARDCODE RELEASE CONTROL


### 35.1. No-Hardcode Purpose

No-Hardcode Release Control đảm bảo không domain nào đưa dữ liệu runtime, rule kinh doanh hoặc trạng thái release vào code, content, template, prompt, UI, Gateway, CRM, AI, ADS, MC AI hoặc IVR dưới dạng cố định.

Hardcode runtime data là điểm chặn.

Hardcode PASS / READY / RELEASE / GO-LIVE là điểm chặn nghiêm trọng.


### 35.2. No-Hardcode bị chặn Items

Không được hardcode:

Giá bán.

Giảm giá chương trình.

Quyền lợi member.

Quyền lợi Diamond.

Tài khoản ngân hàng.

Stock.

MC AI script brief active state.

ADS scale decision.

Gateway PASS.

Future extension active state.

Official contact ngoài registry.

Public trace field ngoài whitelist.


### 35.3. No-Hardcode Release Evidence

Mỗi release phải có evidence chứng minh:

Runtime data được lấy qua resolver.

Business rule do Owner Core quyết định.

Consumer chỉ tiêu thụ trong boundary.

Guard có quyền block/suppress.

No-hardcode scan hoặc review có evidence.

Owner sign-off không có hardcode P0.

Negative path chứng minh hardcode không bypass được guard.

Không có no-hardcode evidence thì release không được PASS.


## 36. FINAL GLOBAL RELEASE DECISION RECORD


### 36.1. Required Record

Mỗi quyết định release cuối phải có Final Global Release Decision Record.

Record này phải có:

final_release_decision_id.

included_domains.

excluded_domains.

required_gate_results.

completion_gate_result.

evidence_package_id.

smoke_package_id.

security_result.

payment_result nếu thuộc scope.

shipping_result nếu thuộc scope.

recall_stop_sale_result nếu thuộc scope.

ADS_scale_result nếu thuộc scope.

MC_AI_live_result nếu thuộc scope.

IVR_result nếu thuộc scope.

future_extension_result nếu thuộc scope.

rollback_plan_id.

monitoring_plan_id.

incident_response_plan_id.

final_decision.


### 36.2. Final Decision Values

Final Decision

Không được release

Chờ evidence

Chờ smoke

Chờ security

Chờ owner ký

Chỉ đủ Production Ready, chưa release

Đã duyệt release, chưa go-live

Được go-live trong scope

Đã live

Được scale trong scope


## 37. MASTER-07 FINAL GLOBAL DONE GATE CHECKLIST


### 37.1. Governance Checklist

MASTER-07 đạt governance checklist khi:

Đã xác định rõ vai trò Global Release Governance.

Đã khóa Production Readiness Model.

Đã khóa Go-live Boundary.

Đã khóa Owner Authority.

Đã khóa Release Gate Registry.

Đã khóa Production Readiness Matrix.

Đã khóa E2E Release Smoke.

Đã khóa ADS Scale Gate.

Đã khóa MC AI Live Gate.

Đã khóa Recall / Stop-Sale Control.

Đã khóa Final Global Release Done Gate.

Đã khóa Go-live Decision.

Đã khóa Rollback Control.

Đã khóa No-Hardcode Release Control.

Đã khóa Final Global Release Decision Record.


### 37.2. Domain Coverage Checklist

MASTER-07 đã bao phủ đầy đủ:

MASTER governance.

PACK documentation.

Customer Identity.

Customer Memory.

Member Lifecycle.

Diamond / Affiliate / Commission.

Health Boundary.

Complaint.

Privacy.

Official Contact.


### 37.3. Final điểm chặn Checklist

Release bắt buộc BLOCK nếu tồn tại một trong các điểm chặn sau:

Không có evidence.

Không có smoke.

Không có owner sign-off.

Security chưa PASS trong scope.

Payment chưa PASS khi có thu tiền.

Shipping chưa PASS khi có giao hàng.

Order chưa PASS khi có chốt đơn.

Product Activation / Sellable chưa PASS khi có bán SKU.

Recall / Stop-Sale chưa PASS khi có sản phẩm thật.

ADS Scale chưa PASS khi có scale.

MC AI Live chưa PASS khi có live.

IVR chưa PASS khi có gọi tự động.

Future Extension chưa ACTIVE_GOVERNED_EXTENSION.

Không audit được P0 action.

Không có rollback path.

Không có monitoring.

Có unresolved P0 điểm chặn.


## 38. MASTER-07 RELEASE CONTROL SUMMARY


### 38.1. Production Ready Summary Rule

Production Ready chỉ được gọi khi:

Implementation Evidence + Runtime Resolver + Guard + Traceability + Audit + Evidence ACCEPTED + Smoke PASS + Negative Path PASS + Security PASS nếu thuộc scope + Owner Sign-off + Completion Gate PASS


### 38.2. Release Approved Summary Rule

Release Approved chỉ được gọi khi:


### 38.3. Go-live Approved Summary Rule

Go-live Approved chỉ được gọi khi:


### 38.4. Gateway Summary Rule

Gateway chỉ được PASS khi:

Gateway Evidence PASS + Security PASS + Public/Messenger/Handoff/Moderation Smoke PASS + Completion Gate PASS


### 38.5. ADS Scale Summary Rule

SKU active/sellable + không recall/sale lock + verified revenue + ROAS/CPA/AOV + funnel quality + COD Fail review + data quality + pilot 7–14 ngày + owner approval

Nếu thiếu verified revenue hoặc owner approval, ADS Scale bị chặn.


### 38.6. MC AI Live Summary Rule

MC AI chỉ được LIVE_APPROVED khi:

Daily Live Product Board hợp lệ + mc_ai_script_brief_id hợp lệ + SKU nằm trong board + program runtime hợp lệ + stock/sellable guard PASS + recall/sale lock guard PASS + fake urgency guard PASS

Nếu MC AI nói SKU ngoài board, MC AI Live bị chặn.


### 38.7. IVR Summary Rule

IVR chỉ được release khi:

PACK-09 IVR_ORDER_CONFIRMATION + INTERNAL_SIM_GATEWAY_SERVER + ORDER_CONFIRMATION_ONLY + Order đủ điều kiện + Call Log + Confirmation Result + Retry Policy + Audit + Owner Sign-off

Nếu IVR bán hàng, upsell hoặc xác nhận payment, IVR Release bị chặn.


### 38.8. Future Extension Summary Rule

Future Extension chỉ được runtime khi:

Integration Intake + Impact Classification + Owner Approval + Source-of-Truth + Resolver + Guard + Trace ID + Evidence + Smoke + Security nếu thuộc scope + Completion Gate PASS + ACTIVE_GOVERNED_EXTENSION

Nếu chưa ACTIVE_GOVERNED_EXTENSION, Future Extension bị chặn.


## 39. MASTER-07 FINAL DONE GATE

Global Release Principles.

Production Readiness Model.

Go-live Boundary.

Owner Authority.

Release Gate Registry by Domain.

Traceability Gate.

Order / Quote / CustomerConfirmation Release Gate.

IVR ORDER_CONFIRMATION_ONLY Release Gate.

MC AI / Live Board / Script Brief Release Gate.

Diamond / Affiliate / Commission Gate.

Production Readiness Matrix.

E2E Release Smoke Matrix.

Final Global Release Done Gate.

Production Ready Decision Control.

Release Approved Decision Control.

Go-live Approved Decision Control.

Rollback Control.

Go-live Monitoring / Incident Control.

No-Hardcode Release Control.

Final Global Release Decision Record.

MASTER-07 hoàn thành ở tầng governance không đồng nghĩa hệ thống đã Production Ready.

MASTER-07 hoàn thành không đồng nghĩa Release Approved.

MASTER-07 hoàn thành không đồng nghĩa Go-live Approved.

MASTER-07 chỉ là chuẩn governance bắt buộc để xét release thật.


## 40. MASTER-07 FINAL CONCLUSION

MASTER-07 khóa lớp quản trị release toàn cục cho hệ thống Ginsengfood.

Từ điểm này trở đi, mọi quyết định mở production, mở kênh, mở bán, chạy Gateway, chạy ADS, scale ADS, bật MC AI Live, bật AI Advisor, bật CRM, bật Payment, bật Shipping, bật IVR hoặc kích hoạt future extension đều phải đi qua chuỗi kiểm soát:

Source-of-Truth -> Owner Core -> Runtime Resolver -> Guard -> Traceability ID -> Audit -> Evidence ACCEPTED -> Smoke PASS -> Negative Path PASS -> Security PASS nếu thuộc scope -> Owner Sign-off -> Completion Gate PASS -> Final Global Release Done Gate -> Production Ready -> Release Approved -> Go-live Approved

Không có đường tắt.

Không có Consumer nào được tự suy luận rule kinh doanh.

Không có pack nào được tự gọi PASS.

Không có Completion Report nào thay thế Completion Gate.

Không có ADS nào được scale khi thiếu verified revenue, data quality, pilot evidence và owner approval.

Không có MC AI nào được live nếu thiếu Daily Live Product Board, mc_ai_script_brief_id, stock guard, recall guard và fake urgency guard.

Không có IVR nào được vượt khỏi ORDER_CONFIRMATION_ONLY.

Không có Payment nào được PAID nếu thiếu Payment Core / Accounting Review.

Không có Shipping nào được bịa ETA, phí ship, COD hoặc tracking.

Không có sản phẩm nào được bán nếu chưa đạt Product Activation / Sellable / Stock Gate.

Không có SKU, batch, lot hoặc channel nào được tiếp tục bán khi Recall / Hold / Sale Lock / Stop Sale active.

Không có future extension nào được ảnh hưởng runtime hiện tại nếu chưa ACTIVE_GOVERNED_EXTENSION.

MASTER-07 là lớp khóa cuối để toàn bộ MASTER, PACK, Operational Core, Production, Warehouse, Inventory, Traceability, Recall, Sale Lock, ADS, MC AI, Gateway, Security, Payment, Shipping, CRM, Order, AI Advisor, IVR và Future Extension được xét release thật theo một chuẩn thống nhất, có evidence, có smoke, có audit, có owner và có rollback.


## Global Release Governance / Production Readiness / Go-Live Control

sss

# MASTER-04 - RUNTIME RESOLUTION GUARD

## Mục Đích

MASTER-04 khóa cách hệ thống ra quyết định tại runtime. Mọi hành động có rủi ro phải đi qua resolver và guard đúng owner, đúng source, đúng context và đúng evidence boundary.

MASTER-04 không viết code resolver. File này định nghĩa contract tối thiểu để PACK và PHASE triển khai.

## Nguyên Tắc Runtime

1. Runtime không được tự suy luận từ dữ liệu rời.
2. Resolver phải đọc Source-of-Truth đúng owner.
3. Guard phải chạy trước action.
4. Hành động rủi ro cao phải có permission, reason, audit và idempotency.
5. Nếu resolver không trả được kết quả hợp lệ, action phải dừng hoặc handoff.
6. Consumer chỉ nhận decision đã resolve, không nhận raw internal data nếu không cần.

## Resolver Chuẩn

| Resolver | Owner | Trả lời |
| --- | --- | --- |
| Product Resolver | PACK-02 | Product/SKU/recipe có active hợp lệ không |
| Formula Snapshot Resolver | PACK-02 + PACK-01 | Formula version nào được dùng cho production |
| Raw Lot Readiness Resolver | PACK-01 | Lot có READY_FOR_PRODUCTION hợp lệ không |
| Batch Release Resolver | PACK-01 | Batch đã RELEASED hợp lệ chưa |
| Inventory Availability Resolver | PACK-01 | Finished goods có available không |
| Trace Public Resolver | PACK-01 | Public trace có được hiển thị không |
| Recall / Sale Lock Resolver | PACK-01 | Scope nào đang bị hold, recall hoặc sale lock |
| MISA Handoff Resolver | PACK-04 | Checkpoint nào đủ điều kiện sync kế toán |
| AI Context Resolver | PACK-05 | AI được dùng context nào để tư vấn |
| Channel Guard Resolver | PACK-06 | Comment/Messenger/Live có được route không |
| Ads Scale Resolver | PACK-07 | Campaign có đủ điều kiện scale không |
| Live Board Resolver | PACK-08 | Live script/board có hợp lệ không |
| IVR Eligibility Resolver | PACK-09 | Order có đủ điều kiện gọi xác nhận không |
| Completion Resolver | PACK-10 | Evidence và smoke có đủ để review không |

## Guard Chuẩn

| Guard | Chặn khi |
| --- | --- |
| Active Guard | Product/SKU/Recipe thiếu dữ liệu hoặc bị khóa |
| Readiness Guard | Raw lot chưa READY_FOR_PRODUCTION |
| Snapshot Guard | Production Order thiếu formula/BOM snapshot |
| Issue Guard | Material Issue dùng lot không hợp lệ |
| Release Guard | Batch QC_PASS nhưng chưa đủ điều kiện release |
| Warehouse Guard | Warehouse nhận batch chưa RELEASED |
| Inventory Guard | Tồn kho tính bằng sửa tay thay vì ledger |
| Trace Guard | Public trace chứa dữ liệu internal |
| Sale Lock Guard | Downstream cố bán hoặc tư vấn item bị lock |
| MISA Guard | Sync khi thiếu mapping hoặc checkpoint |
| AI Safety Guard | AI nói claim, giá, tồn hoặc paid ngoài source |
| Channel Guard | Public comment lộ dữ liệu private |
| Evidence Guard | Gọi đạt khi evidence chưa accepted |

## Priority Conflict

Khi nhiều guard cùng tác động, thứ tự ưu tiên là:

1. Security / permission / audit.
2. Recall / Sale Lock / Stop Sale.
3. Public safety / privacy.
4. Operational release and inventory.
5. Payment and shipping confirmation.
6. Pricing / promotion / member benefit.
7. Channel pacing / CRM / Ads / Live.

Sales, upsell, campaign scale, live script hoặc AI recommendation không được thắng các guard ở thứ tự cao hơn.

## Runtime Decision Record

Mỗi decision trọng yếu phải ghi tối thiểu:

1. Decision ID.
2. Resolver đã gọi.
3. Source ID đã đọc.
4. Guard đã chạy.
5. Input context.
6. Decision output.
7. Actor hoặc system caller.
8. Timestamp.
9. Audit correlation ID.
10. Evidence link nếu decision dùng cho nghiệm thu.

## High-Risk Action Matrix

| Action | Guard bắt buộc | Evidence |
| --- | --- | --- |
| Activate Product / SKU / Recipe | Active Guard | Activation audit |
| Approve formula version | Snapshot Guard | Owner decision |
| Issue raw lot | Readiness + Issue Guard | Lot readiness evidence |
| Release batch | Release Guard | QC + release evidence |
| Warehouse receipt | Warehouse Guard | Receipt log |
| Adjust inventory | Permission + Inventory Guard | Ledger/audit evidence |
| Reprint QR/barcode | Trace Guard + permission | Print log |
| Apply Sale Lock | Sale Lock Guard | Lock decision |
| Sync MISA | MISA Guard | Sync/reconcile log |
| Send AI response | AI Safety Guard | Response audit |
| Route Messenger/Live | Channel Guard | Channel audit |
| Start IVR call | IVR Eligibility Guard | Call job log |

## Fail-Closed Rule

Hệ thống phải fail-closed khi:

1. Source không tồn tại.
2. Resolver lỗi hoặc timeout.
3. Guard thiếu cấu hình.
4. Permission thiếu.
5. Evidence chưa đủ.
6. Dữ liệu conflict.
7. Owner chưa xác nhận rule mới.

Fail-closed có thể là suppress, handoff, review required, no action hoặc stop sale tùy domain.

## Done Gate Của MASTER-04

MASTER-04 đạt yêu cầu khi:

1. Resolver và guard chuẩn được định nghĩa đủ cho các domain chính.
2. Priority conflict rõ.
3. Runtime decision record rõ.
4. High-risk action matrix rõ.
5. Fail-closed rule rõ.
6. PACK có thể dùng file này để viết implementation contract.

## Chi Tiết Governance Tương Ứng

Phần này giữ lại phạm vi chi tiết từ nguồn MASTER theo cụm cùng chủ đề, đồng thời chuẩn hóa theo registry PACK hiện hành và loại bỏ các câu trạng thái tổng không dùng trong bộ sạch.


## Master-04 — Runtime Resolution & Guard Control Standard

BUSINESS RULE RESOLUTION / RUNTIME CONTEXT / GUARD DECISION / SUPPRESSION / FALLBACK / ACTION CONTROL


## PHẦN 1/4 — RUNTIME PRINCIPLES / RESOLVER MODEL / GUARD BOUNDARY / DECISION CONTROL


## 1. MỤC ĐÍCH CỦA MASTER-04

MASTER-04 quy định chuẩn vận hành của lớp Runtime Resolution và Guard Control trong hệ thống Ginsengfood.

Mục tiêu của MASTER-04 là bảo đảm mọi quyết định tại thời điểm thực thi đều được resolve đúng nguồn, đúng Owner Core, đúng business rule, đúng customer context, đúng trạng thái runtime, đúng guard và đúng evidence boundary.

MASTER-04 không phải tài liệu code.

MASTER-04 không phải tài liệu database schema.

MASTER-04 không phải tài liệu API/DTO.

MASTER-04 không phải tài liệu UI.

MASTER-04 không phải tài liệu worker.

MASTER-04 là tài liệu governance kỹ thuật dùng để khóa cách hệ thống Ginsengfood ra quyết định runtime trước khi AI, Gateway, CRM, Landing Page, ADS, MC AI, IVR, Payment, Shipping, Order hoặc bất kỳ Consumer Pack nào thực hiện hành động.


## 2. VAI TRÒ CỦA MASTER-04 TRONG BỘ MASTER

MASTER-04 đứng sau MASTER-00, MASTER-01, MASTER-02 và MASTER-03.

MASTER-00 quy định cấu trúc governance tổng thể.

MASTER-01 quy định Source-of-Truth, Owner Core, Runtime Resolver, Consumer Boundary, Guard, Evidence và Release Gate.

MASTER-02 quy định dependency giữa các pack, các business domain P0, rule bị chặn-if-missing, suppression, fallback và no-hardcode.

MASTER-03 quy định chuẩn Traceability ID, ID ownership, ID generation, ID immutability, correlation, idempotency, evidence trace và audit relationship.

MASTER-04 quy định cách Runtime Resolver và Guard sử dụng Source-of-Truth, dependency và Traceability ID để ra quyết định đúng tại thời điểm thực thi.

Nếu MASTER-03 trả lời câu hỏi “ID nào dùng để truy vết?”, thì MASTER-04 trả lời câu hỏi “Tại thời điểm chạy thật, hệ thống được phép quyết định gì, dựa trên dữ liệu nào, qua resolver nào, qua guard nào và action nào được phép thực hiện?”


## 3. NGUYÊN TẮC CỐT LÕI CỦA RUNTIME RESOLUTION


### 3.1. Runtime quyết định tại thời điểm thực thi

Hệ thống Ginsengfood bắt buộc mọi quyết định động phải được xác định tại thời điểm thực thi.

Các quyết định sau không được viết cứng trong nội dung, template, script, UI, AI response, CRM message, Gateway reply hoặc Landing Page:

Khách là ai.

Khách mới hay khách cũ.

Khách có phải member không.

Khách thuộc hạng nào.

Khách có phải Diamond không.

Khách có đến từ link Diamond hợp lệ không.

Sản phẩm có sellable không.

Sản phẩm có đang active không.

Sản phẩm có thuộc chương trình không.

Giá niêm yết hiện tại.

Giá chương trình hiện tại.

Quyền lợi member hiện tại.

Quyền lợi Diamond link hiện tại.

QuoteSnapshot hiện tại.

OrderDraft hiện tại.

Payment reference hiện tại.

Trạng thái thanh toán.

Trạng thái vận chuyển.

Trạng thái CRM suppression.

Trạng thái complaint/privacy/health/counterfeit.

Trạng thái Gateway handoff.

Trạng thái evidence.

Trạng thái release gate.

Runtime Resolver là lớp quyết định các dữ liệu này tại thời điểm action được yêu cầu.


### 3.2. Consumer không được tự suy luận runtime data

Consumer Pack không được tự suy luận dữ liệu runtime.

Consumer Pack bao gồm nhưng không giới hạn:

AI Advisor.

Gateway.

CRM.

Landing Page.

ADS.

MC AI.

IVR.

Admin UI.

Frontend.

Content Block.

Template Engine.

Channel Adapter.

Customer Care Tool.

Consumer chỉ được tiêu thụ kết quả đã được Runtime Resolver và Guard cung cấp.

Consumer không được tự tính giá.

Consumer không được tự tính hạng member.

Consumer không được tự xác nhận Diamond bind.

Consumer không được tự xác định commission.

Consumer không được tự xác nhận PAID.

Consumer không được tự xác định shipping status.

Consumer không được tự nói Gateway PASS.

Consumer không được tự chọn số điện thoại.

Consumer không được tự chọn sản phẩm active để bán.

Consumer không được tự suy luận claim khoa học.

Consumer không được tự quyết định release readiness.


### 3.3. Resolver trả về dữ liệu có thể dùng để quyết định, không chỉ trả về giá trị thô

Runtime Resolver không được chỉ trả về giá trị rời rạc.

Một resolver hợp lệ phải trả về đủ ngữ cảnh để Consumer biết dữ liệu đó có được phép dùng hay không.

Ví dụ:

Resolver giá không chỉ trả về con số giá.

Resolver giá phải trả về:

Giá niêm yết.

Chương trình đang áp dụng nếu có.

Quyền lợi member nếu có.

Quyền lợi Diamond link nếu có.

Điều kiện áp dụng.

Thời hạn quote.

QuoteSnapshot ID.

Product activation status.

Program activation status.

Customer context.

Guard status.

Audit reference.

Block reason nếu không được quote.

Tương tự, resolver shipping không chỉ trả về trạng thái giao hàng.

Resolver shipping phải trả về:

Shipping event mới nhất.

ETA nếu có.

COD status nếu có.

Phí ship nếu cần.

Order reference.

Event timestamp.

Block reason nếu không đủ dữ liệu.


### 3.4. Guard quyết định action được phép hay không

Resolver cung cấp dữ liệu đã resolve.

Guard quyết định action có được phép thực hiện hay không.

Không có guard thì không action P0 nào được chạy production.

Guard phải kiểm tra:

Source-of-Truth.

Owner Core.

Dependency.

Traceability ID.

Product activation.

Program activation.

Payment status.

Shipping status.

CRM suppression.

Priority conflict.

Public/private boundary.

Evidence boundary.

Security boundary.

Release gate boundary.

Guard không phải lớp trang trí.

Guard là cổng quyết định an toàn trước action.


## 4. MÔ HÌNH RUNTIME DECISION CHUẨN


### 4.1. Chuỗi quyết định runtime chuẩn

MASTER-04 khóa chuỗi quyết định runtime chuẩn như sau:

Event / IntentHệ thống nhận một sự kiện hoặc ý định: comment, inbox, CRM trigger, order action, payment signal, shipping query, complaint, quote request, live action, IVR confirmation.

Context CaptureHệ thống ghi nhận ngữ cảnh: channel, customer, session, source, product, program, order, payment, shipping, case, time, policy context.

Runtime ResolutionRuntime Resolver resolve dữ liệu từ Owner Core và Source-of-Truth.

Guard DecisionGuard kiểm tra điều kiện pass/block/suppress/fallback/handoff/review.

Decision SnapshotHệ thống tạo snapshot quyết định nếu action có ảnh hưởng nghiệp vụ, khách hàng, tiền, order, payment, shipping, CRM, Gateway hoặc release.

Action AuthorizationChỉ action được Guard cho phép mới được thực hiện.

Render / ExecuteConsumer render message, tạo quote, tạo draft, handoff, gửi CRM, gửi payment instruction, ghi shipping response hoặc chuyển human review theo đúng action đã được authorize.

Audit / Evidence TraceMọi quyết định P0 phải có audit; smoke/release phải có evidence.


### 4.2. Không được đi tắt từ Intent sang Action

Consumer không được đi thẳng từ intent sang action.

Khách hỏi giá -> AI không được báo giá ngay nếu chưa có QuoteSnapshot.

Khách comment mua hàng -> Gateway không được chốt đơn public.

Khách nói đã chuyển khoản -> AI không được xác nhận PAID.

CRM đến lịch gửi -> CRM không được gửi nếu chưa qua suppression guard.

Khách hỏi số điện thoại -> AI không được tự gửi số nếu chưa qua Official Contact Resolver.

Khách hỏi sản phẩm -> AI không được chốt SKU nếu chưa có product activation/sellable context.

Khách hỏi vận chuyển -> AI không được bịa tracking nếu chưa có Shipping Resolver.

Evidence upload -> Completion Gate không được PASS nếu chưa review/accepted.


### 4.3. Decision Snapshot là bắt buộc với quyết định P0

Các quyết định P0 phải có Decision Snapshot hoặc runtime decision record tương ứng.

Decision Snapshot không thay thế business ID.

Decision Snapshot ghi lại:

Event/intent gốc.

Context đầu vào.

Resolver đã gọi.

Guard đã chạy.

Kết quả guard.

Action được phép.

Action bị block nếu có.

Fallback nếu có.

Handoff nếu có.

Source / dependency / trace ID liên quan.

Audit ID.

Correlation ID.

Evidence ID nếu thuộc smoke/release.

Các quyết định sau bắt buộc cần snapshot hoặc decision record:

Báo giá.

Tạo quote cart.

Tạo order draft.

Ghi customer confirmation.

Tạo order.

Gửi payment instruction.

Xác nhận payment.

Trả lời shipping status.

Gửi CRM.

Handoff Messenger.

Trả lời public comment.

Tư vấn health-sensitive.

Tạo complaint/privacy/counterfeit case.

Xét Diamond bind.

Tạo commission event.

Xét evidence.

Xét completion gate.


## 5. RUNTIME RESOLVER MODEL


### 5.1. Runtime Resolver là gì

Runtime Resolver là lớp nghiệp vụ chịu trách nhiệm lấy dữ liệu đúng từ Owner Core, kiểm tra dữ liệu còn hiệu lực, gắn trace ID, trả về trạng thái resolve và cung cấp context cho Guard.

Runtime Resolver không phải helper kỹ thuật tùy tiện.

Runtime Resolver không phải nơi viết business rule rời rạc.

Runtime Resolver không phải nơi hardcode dữ liệu.

Runtime Resolver là điểm hợp nhất giữa Source-of-Truth, Owner Core, Dependency, Traceability ID và Runtime Decision.


### 5.2. Resolver bắt buộc biết nguồn dữ liệu

Mỗi Runtime Resolver phải biết dữ liệu mình trả về đến từ đâu.

Một kết quả resolver hợp lệ phải có thể trace về:

Domain.

Canonical ID.

Correlation ID nếu thuộc flow đa pack.

Audit ID nếu có quyết định P0.

Evidence ID nếu dùng trong smoke/release.

Nếu Resolver không xác định được nguồn, kết quả phải ở trạng thái MISSING_SOURCE hoặc UNRESOLVED.

Consumer không được dùng kết quả unresolved để quyết định.


### 5.3. Resolver bắt buộc biết dữ liệu còn hiệu lực hay không

Runtime data có thể hết hạn, bị thay đổi, bị hold, bị superseded hoặc bị conflict.

Resolver phải kiểm tra hiệu lực của dữ liệu trước khi trả về cho Consumer.

Các dữ liệu bắt buộc kiểm tra hiệu lực:

Customer identity.

Customer memory snapshot.

Member lifecycle.

Diamond bind.

Sellable status.

Golden Hour session.

Early access window.

QuoteSnapshot.

OrderDraft.

CustomerConfirmation.

Payment reference.

Accounting review.

Shipping event.

Official contact.

Claim/scientific evidence.

CRM trigger.

Suppression state.

Complaint/privacy/health/counterfeit case.

Evidence package.

Completion gate.


### 5.4. Resolver không được tự quyết định ngoài domain

Một resolver chỉ được quyết định trong phạm vi domain của mình.

Pricing Resolver không được tự xác nhận shipping.

Shipping Resolver không được tự xác nhận payment.

Payment Resolver không được tự tạo order.

Member Resolver không được tự áp commission.

Diamond Resolver không được tự chuyển PAID.

Product Recommendation Resolver không được tự bỏ qua health guard.

Gateway Resolver không được tự báo giá public.

CRM Resolver không được tự bỏ qua suppression.

Evidence Resolver không được tự pass completion gate.

Khi một decision cần nhiều domain, hệ thống phải dùng orchestration theo đúng resolver chain, không để một resolver ôm quyền của nhiều Owner Core.


## 6. RUNTIME RESOLUTION STATUS


### 6.1. Bộ trạng thái chuẩn của Runtime Resolver

MASTER-04 khóa bộ trạng thái chuẩn của Runtime Resolver:

RESOLVED

PARTIAL

MISSING

STALE

CONFLICT

bị chặn

SUPPRESSED

FALLBACK_REQUIRED

HANDOFF_REQUIRED

REVIEW_REQUIRED

ERROR


### 6.2. RESOLVED

RESOLVED nghĩa là dữ liệu đã được resolve đầy đủ, đúng nguồn, đúng Owner Core, còn hiệu lực, có ID trace và đủ điều kiện chuyển sang Guard.

RESOLVED không tự động nghĩa là action được phép.

Dữ liệu RESOLVED vẫn phải đi qua Guard.


### 6.3. PARTIAL

PARTIAL nghĩa là dữ liệu resolve được một phần nhưng chưa đủ để action P0.

Biết khách là ai nhưng chưa biết member status.

Biết sản phẩm nhưng chưa biết sellable status.

Biết order nhưng chưa có shipping event.

Biết payment reference nhưng chưa có accounting review.

Biết comment event nhưng chưa biết handoff delivery status.

PARTIAL không được dùng để thực hiện action cần dữ liệu đầy đủ.


### 6.4. MISSING

MISSING nghĩa là thiếu dữ liệu bắt buộc.

Thiếu customer identity.

Thiếu product activation.

Thiếu program activation.

Thiếu QuoteSnapshot.

Thiếu payment reference.

Thiếu shipping event.

Thiếu official contact.

Thiếu evidence.

Thiếu owner sign-off.

MISSING phải dẫn đến BLOCK, FALLBACK, HANDOFF hoặc REVIEW tùy domain.


### 6.5. STALE

STALE nghĩa là dữ liệu đã cũ hoặc không còn phù hợp tại thời điểm runtime.

QuoteSnapshot hết hạn.

Program đã kết thúc.

Golden Hour session đã đóng.

Product activation bị thay đổi.

Member benefit đã thay đổi sau snapshot.

Shipping event quá cũ.

Evidence không còn hiệu lực sau thay đổi hệ thống.

STALE không được dùng để quyết định mới.


### 6.6. CONFLICT

CONFLICT nghĩa là có xung đột giữa nhiều nguồn, nhiều trạng thái hoặc nhiều rule.

Product active nhưng stock = 0.

Program active nhưng price inactive.

Customer member tier conflict.

Diamond bind conflict với Ads attribution.

Payment signal conflict với Accounting Review.

Shipping status conflict.

Completion Report conflict với evidence package.

Gateway handoff success/fail conflict.

CONFLICT phải chuyển Guard xử lý theo priority rule, không được Consumer tự chọn một nhánh.


### 6.7. bị chặn

bị chặn nghĩa là resolver xác định action không thể đi tiếp do thiếu điều kiện P0 hoặc domain bị khóa.

SKU bị recall hold.

Product sellable_status không hợp lệ.

Payment chưa xác nhận.

Complaint mở.

Privacy case mở.

Evidence thiếu.

Security điểm chặn.

Gateway policy điểm chặn.

bị chặn phải có audit reason.


### 6.8. SUPPRESSED

SUPPRESSED nghĩa là action bị chặn bởi suppression rule.

CRM bị chặn do complaint mở.

Sales bị chặn do payment dispute.

Upsell bị chặn do delivery issue.

Public reply bị chặn do PII.

Diamond promotion bị chặn do commission dispute.

Member upgrade message bị chặn do member dispute.

SUPPRESSED phải trace đến case hoặc suppression source.


### 6.9. FALLBACK_REQUIRED

FALLBACK_REQUIRED nghĩa là action chính không được phép nhưng có fallback an toàn.

Handoff Messenger thất bại thì dùng public fallback an toàn.

Shipping chưa có ETA thì trả lời chưa có trạng thái cập nhật, không bịa ETA.

Payment chưa xác nhận thì trả lời đang chờ đối soát, không nói PAID.

Scientific evidence chưa approved thì dùng public-safe generic wording.

Fallback không được nâng trạng thái nghiệp vụ.


### 6.10. HANDOFF_REQUIRED

HANDOFF_REQUIRED nghĩa là luồng phải chuyển sang private, human, admin, accounting, QA, CSKH, legal, security hoặc owner review.

Public comment hỏi giá phải private handoff nếu policy cho phép.

Khách để PII ngoài comment public.

Khách nêu bệnh nhạy cảm.

Khách khiếu nại chất lượng.

Khách báo nghi hàng giả.

Khách tranh chấp thanh toán.

Khách tranh chấp quyền lợi member.

Khách tranh chấp commission.


### 6.11. REVIEW_REQUIRED

REVIEW_REQUIRED nghĩa là dữ liệu hoặc quyết định cần người/cơ chế có thẩm quyền xem xét trước khi action.

Accounting Review.

Owner Sign-off.

Evidence Review.

Security Review.

App Review.

Complaint Review.

Counterfeit Review.

Member Dispute Review.

Commission Dispute Review.

REVIEW_REQUIRED không được tự động chuyển PASS.


### 6.12. ERROR

ERROR nghĩa là resolver lỗi kỹ thuật hoặc không thể hoàn tất.

ERROR không được biến thành fallback bán hàng nếu domain liên quan là P0.

ERROR phải có audit exception, observability và handling theo pack implementation sau.


## 7. GUARD CONTROL MODEL


### 7.1. Guard là gì

Guard là lớp kiểm soát điều kiện trước khi action được thực hiện.

Guard quyết định:

PASS

BLOCK

SUPPRESS

FALLBACK

HANDOFF

ESCALATE

NO_ACTION

Guard không thay thế Resolver.

Guard không thay thế Owner Core.

Guard không thay thế Evidence.

Guard không thay thế Release Gate.

Guard là cổng kiểm soát an toàn cho action.


### 7.2. Guard bắt buộc chạy sau Resolver

Guard phải chạy trên dữ liệu đã được Runtime Resolver trả về.

Không được chạy Guard dựa trên dữ liệu tự suy luận từ Consumer.

Không được chạy Guard dựa trên text rời rạc.

Không được chạy Guard dựa trên biến hardcode.

Không được chạy Guard dựa trên trạng thái cache không có trace.

Nếu Resolver chưa RESOLVED đủ điều kiện, Guard phải ra quyết định block, suppress, fallback, handoff hoặc review.


### 7.3. Guard bắt buộc trả về decision rõ ràng

Guard không được trả về trạng thái mơ hồ.

Một Guard Decision hợp lệ phải gồm:

Decision status.

Reason.

Source/Resolver reference.

Required ID.

Missing ID nếu có.

bị chặn action nếu có.

Allowed action nếu có.

Handoff target nếu có.

Correlation reference.

Evidence reference nếu thuộc smoke/release.


## 8. BỘ GUARD DECISION CHUẨN


### 8.1. PASS

PASS nghĩa là action được phép thực hiện trong phạm vi đã resolve.

PASS không có nghĩa các action khác cũng được phép.

QuoteGuard PASS cho báo giá không đồng nghĩa OrderGuard PASS cho tạo đơn.

PaymentInstructionGuard PASS không đồng nghĩa PaymentStatusGuard PASS cho PAID.

GatewayHandoffGuard PASS không đồng nghĩa PublicReplyGuard PASS cho báo giá public.


### 8.2. BLOCK

BLOCK nghĩa là action không được phép thực hiện.

BLOCK phải có reason rõ.

SKU không sellable.

Missing customer identity.

Missing product activation.

Missing official contact.

Missing evidence.

Handoff fail.

Completion gate chưa đủ điều kiện.


### 8.3. SUPPRESS

SUPPRESS nghĩa là action bị chặn do rule ưu tiên, conflict hoặc policy.

CRM bị suppress do complaint mở.

Sales bị suppress do privacy case.

Upsell bị suppress do delivery issue.

Public reply bị suppress do PII.

Payment reminder bị suppress do payment dispute.

Diamond promotion bị suppress do commission dispute.


### 8.4. FALLBACK

FALLBACK nghĩa là action chính không được phép, nhưng có phản hồi hoặc hành động thay thế an toàn.

Fallback phải không làm thay đổi trạng thái nghiệp vụ ngoài phạm vi được phép.

Không có ETA thì trả lời chưa có cập nhật vận chuyển, không tự đoán ngày giao.

Handoff Messenger fail thì dùng public fallback an toàn, không nói đã gửi.

Missing scientific evidence thì dùng câu an toàn, không nói có công bố.

Missing official contact thì không gửi số điện thoại.


### 8.5. HANDOFF

HANDOFF nghĩa là phải chuyển sang kênh hoặc người/cơ chế phù hợp.

Các handoff chính:

Messenger/private handoff.

Human CSKH handoff.

Accounting handoff.

QA/Quality handoff.

Privacy/Security handoff.

Legal/Brand Protection handoff.

Owner Review handoff.

Admin Review handoff.

HANDOFF phải có trace và audit.


### 8.6. REVIEW_REQUIRED

REVIEW_REQUIRED nghĩa là action cần review trước khi quyết định.

Không được hiểu REVIEW_REQUIRED là PASS.

Evidence under review chưa được dùng PASS.

Accounting review chưa đủ điều kiện chưa PAID.

Owner review chưa owner sign-off.

Security review chưa đủ điều kiện chưa release.

Complaint review chưa đủ điều kiện chưa đóng case.


### 8.7. ESCALATE

ESCALATE nghĩa là case cần nâng cấp xử lý.

Các tình huống cần escalate:

Khiếu nại nghiêm trọng.

Privacy incident.

Payment dispute nghiêm trọng.

Counterfeit suspicion.

Legal accusation.

Abuse/troll nghiêm trọng.

Security issue.

Commission dispute nghiêm trọng.

Health-sensitive escalation.

ESCALATE không được tiếp tục sales.


### 8.8. NO_ACTION

NO_ACTION nghĩa là hệ thống ghi nhận event nhưng không thực hiện action customer-facing hoặc state-changing.

Duplicate event đã được xử lý.

Comment spam không cần phản hồi.

CRM trigger bị suppression.

Event không đủ điều kiện xử lý.

Evidence duplicate.

Webhook retry không tạo action mới.

NO_ACTION vẫn phải có audit nếu thuộc P0 flow.


## 9. RUNTIME GUARD BOUNDARY


### 9.1. Guard không được tạo business data

Guard không được tạo business data thay Owner Core.

Guard không tạo giá.

Guard không tạo hạng member.

Guard không tạo Diamond bind.

Guard không tạo order.

Guard không tạo PAID.

Guard không tạo shipping status.

Guard không tạo evidence accepted.

Guard không tạo owner sign-off.

Guard chỉ quyết định action có được phép dùng dữ liệu đã resolve hay không.


### 9.2. Guard không được sửa dữ liệu lịch sử

Guard không được sửa snapshot, event, quote, order draft, payment reference, accounting review, shipping event, CRM message text, evidence hoặc audit.

Nếu phát hiện dữ liệu không hợp lệ, Guard phải block hoặc yêu cầu tạo event/snapshot/review mới theo Owner Core.


### 9.3. Guard không được bypass Owner Core

Guard không được vượt quyền Owner Core.

Payment Guard không được xác nhận PAID nếu Payment Core/Accounting Review chưa xác nhận.

Shipping Guard không được nói Delivered nếu Shipping Core chưa có event.

Quote Guard không được báo giá nếu Quote Core chưa tạo QuoteSnapshot.

Gateway Guard không được nói handoff success nếu delivery log chưa success.

Evidence Guard không được accepted evidence nếu Evidence Review chưa accepted.

Release Guard không được release approved nếu Owner chưa sign-off.


## 10. RUNTIME PRIORITY MODEL


### 10.1. Priority Conflict thắng Sales

MASTER-04 khóa nguyên tắc: các priority conflict thắng sales, CRM, upsell, quote mới và chốt đơn.

Các priority conflict gồm:

Complaint.

Refund.

Privacy case.

Payment dispute.

Delivery issue.

Health review.

Counterfeit case.

Member dispute.

Commission dispute.

Legal/brand protection issue.

Gateway policy issue.

Nếu conflict đang mở, Guard phải suppress hoặc block action bán hàng tương ứng.


### 10.2. Payment và Accounting thắng lời khách nói

Khách nói đã chuyển khoản hoặc gửi ảnh giao dịch không thắng Payment Core / Accounting Review.

Guard phải giữ trạng thái chưa đủ điều kiện/review nếu Payment Core hoặc Accounting Review chưa xác nhận.

Không được chuyển PAID dựa trên nội dung chat.


### 10.3. Shipping Core thắng suy đoán

AI/CRM/Gateway không được tự đoán ETA, COD, phí ship hoặc tracking status.

Shipping Guard chỉ cho phép trả lời trạng thái cụ thể khi Shipping Core có dữ liệu.

Nếu không có dữ liệu, fallback phải an toàn.


### 10.4. Product Activation thắng mong muốn bán hàng

AI/CRM/Gateway/Landing/ADS/MC AI chỉ được nói/bán SKU active, sellable, có price, đúng board, đúng chương trình, đúng channel.

Nếu sản phẩm hết hàng, bị hold, recall, sale lock, channel block, price inactive hoặc sellable_status không hợp lệ, Guard phải block bán hàng.


### 10.5. Customer Context thắng cá nhân hóa

Không có Customer Context thì không cá nhân hóa.

Không có customer identity thì không báo quyền lợi member.

Không có member lifecycle thì không nói hạng/quyền lợi/doanh số còn thiếu.

Không có Diamond bind thì không áp Diamond attribution.

Không có customer memory snapshot thì không nhắc lịch sử mua cụ thể.


### 10.6. Evidence thắng tuyên bố PASS

Không có evidence accepted thì không PASS.

Không có smoke PASS thì không completion gate PASS.

Không có owner sign-off thì không release approved.

Không có completion gate PASS thì không Gateway PASS.

Không được gọi Ready/PASS chỉ vì tài liệu đã viết xong.


## 11. RUNTIME CONTEXT PACKAGE


### 11.1. Runtime Context Package là gì

Runtime Context Package là tập hợp thông tin tối thiểu cần có để Runtime Resolver và Guard ra quyết định.

Runtime Context Package không phải DTO implementation.

Runtime Context Package là chuẩn governance cho mọi action P0.


### 11.2. Runtime Context Package tối thiểu

Một Runtime Context Package tối thiểu phải xác định:

Channel.

Session/correlation.

Resolver result.

Guard decision.

Allowed action.

bị chặn action.

Fallback/handoff nếu có.

Expiry/validity nếu có.

Policy version nếu có.

Priority conflict nếu có.


### 11.3. Context thiếu thì action phải bị kiểm soát

Nếu thiếu Runtime Context Package, Consumer không được tự đi tiếp.

Tùy domain, action phải:

BLOCK.

SUPPRESS.

FALLBACK.

HANDOFF.

REVIEW_REQUIRED.

NO_ACTION.

Không được “tạm làm trước” với dữ liệu thiếu.

Không được “sửa sau” bằng audit.

Không được “đoán theo nội dung chat”.


## 12. RUNTIME DECISION BOUNDARY THEO NHÓM CONSUMER


### 12.1. AI Advisor

AI Advisor được phép render tư vấn, giải thích, gợi ý, chăm sóc và hỗ trợ chốt đơn trong phạm vi Guard cho phép.

AI Advisor không được:

Tự tính giá.

Tự tạo QuoteSnapshot.

Tự xác nhận order success.

Tự xác nhận PAID.

Tự bịa shipping status.

Tự xác nhận member tier.

Tự xác nhận Diamond commission.

Tự gửi số điện thoại ngoài Official Contact Resolver.

Tự thuốc hóa claim.

Tự bỏ qua health/complaint/privacy guard.

Tự gọi Gateway PASS.

AI Advisor chỉ được dùng dữ liệu đã RESOLVED và PASS Guard.


### 12.2. Gateway

Gateway được phép xử lý inbound event, public comment, Messenger handoff, private reply và moderation theo Guard.

Gateway không được:

Báo giá public.

Chốt đơn public.

Lặp PII public.

Gửi payment instruction public.

Tư vấn health detail dài public.

Nói đã gửi Messenger nếu handoff delivery log không success.

Kêu khách tự inbox nếu hệ thống có thể chủ động handoff theo policy.

Tự pass Gateway readiness.

Gateway phải tuân thủ Public/Private Guard.


### 12.3. CRM

CRM được phép chăm sóc vòng đời khách hàng khi có trigger hợp lệ và không bị suppression.

CRM không được:

Gửi nếu thiếu message trigger.

Gửi nếu thiếu customer identity.

Gửi nếu thiếu dedup/idempotency.

Gửi nếu vi phạm quiet hours/frequency cap.

Gửi nếu opt-out.

Gửi sales message khi có complaint/payment/delivery/health/privacy conflict.

Sửa message text đã gửi.

Hardcode giá/quyền lợi/chương trình.

CRM phải tuân thủ CRM Suppression Guard.


### 12.4. Landing Page / ADS / MC AI

Landing Page, ADS và MC AI chỉ được nói/bán sản phẩm active, sellable, đúng board, đúng chương trình, đúng kênh.

Không được:

Tự chọn SKU ngoài product activation.

Tự hardcode giá.

Tự hardcode chương trình.

Tự hardcode tồn kho.

Tự tạo quote.

Tự nói sản phẩm còn hàng nếu Sellable Guard không PASS.

Tự public claim ngoài Claim Guard.

Tự nói thông tin payment ngoài Payment Guard.


### 12.5. IVR

IVR chỉ được hoạt động trong phạm vi ORDER_CONFIRMATION_ONLY.

IVR không được:

Tự đổi order state ngoài phạm vi.

Tự tạo order mới.

Tự sửa order draft.

Tự xác nhận payment.

Tự upsell.

Tự chạy CRM.

Tự quyết định shipping.

IVR confirmation phải trace về OrderDraft, CustomerConfirmation và audit.


## 13. NO-HARDCODE RUNTIME ENFORCEMENT


### 13.1. Dữ liệu runtime không được hardcode

MASTER-04 khóa nghiêm ngặt: dữ liệu runtime không được hardcode.

Không được hardcode:

Giá.

Chiết khấu.

Chương trình.

Hạng member.

Quyền lợi member.

Diamond benefit.

Hoa hồng.

Tồn kho.

Board live.

Tài khoản ngân hàng.

Shipping fee.

ETA.

COD.

Tracking status.

Scientific evidence status.

Completion status.

Gateway status.

Release status.


### 13.2. Hardcode runtime data là điểm chặn

Nếu phát hiện hardcode runtime data, domain liên quan phải bị bị chặn.

Hardcode giá -> Pricing/Quote Gate bị chặn.

Hardcode tài khoản ngân hàng -> Payment Gate bị chặn.

Hardcode shipping status -> Shipping Gate bị chặn.

Hardcode member benefit -> Member Gate bị chặn.

Hardcode Diamond commission -> Diamond Gate bị chặn.

Hardcode số điện thoại -> Official Contact Gate bị chặn.

Hardcode Gateway PASS -> Completion Gate bị chặn.

Hardcode product active -> Product Activation Gate bị chặn.


## 14. RUNTIME FAILURE PRINCIPLE


### 14.1. Runtime failure không được biến thành quyết định bán hàng

Khi Resolver hoặc Guard lỗi, hệ thống không được tự động tiếp tục bán hàng.

Các lỗi runtime sau phải block hoặc fallback an toàn:

Customer identity resolver failed.

Product activation resolver failed.

Price resolver failed.

Quote resolver failed.

Payment resolver failed.

Shipping resolver failed.

CRM suppression resolver failed.

Gateway handoff resolver failed.

Official contact resolver failed.

Evidence resolver failed.

Completion gate resolver failed.


### 14.2. Runtime failure phải có audit

Mọi runtime failure P0 phải có audit.

Audit phải ghi:

Resolver/Guard nào lỗi.

Domain nào bị ảnh hưởng.

Action nào bị chặn.

Fallback nào được dùng nếu có.

Handoff nào được yêu cầu nếu có.

Required ID bị thiếu nếu có.

Có ảnh hưởng customer-facing hay không.


### 14.3. Runtime failure không được che bằng câu trả lời đẹp

Consumer không được che lỗi runtime bằng câu trả lời có vẻ hoàn chỉnh.

Không có giá runtime thì không được tự báo giá mềm.

Không có shipping ETA thì không được nói “dự kiến 2–3 ngày”.

Không có payment confirmation thì không được nói “đã thanh toán”.

Không có handoff success thì không được nói “đã gửi Messenger”.

Không có evidence accepted thì không được nói “đã đạt”.

Không có official contact thì không được tự đưa số.

Fallback phải trung thực, an toàn và không nâng trạng thái nghiệp vụ.


## 15. KẾT LUẬN PHẦN 1/4


## PHẦN 1/4 của MASTER-04 khóa nền tảng Runtime Resolution và Guard Control cho toàn bộ hệ thống Ginsengfood.

Từ thời điểm áp dụng MASTER-04:

Runtime data phải được resolve tại thời điểm thực thi.

Consumer không được tự suy luận runtime data.

Resolver phải trả về dữ liệu có nguồn, trạng thái, ID và hiệu lực.

Guard phải quyết định action được phép hay không.

Không được đi tắt từ intent sang action.

Decision Snapshot bắt buộc với quyết định P0.

Resolver status phải rõ: RESOLVED, PARTIAL, MISSING, STALE, CONFLICT, bị chặn, SUPPRESSED, FALLBACK_REQUIRED, HANDOFF_REQUIRED, REVIEW_REQUIRED, ERROR.

Guard decision phải rõ: PASS, BLOCK, SUPPRESS, FALLBACK, HANDOFF, REVIEW_REQUIRED, ESCALATE, NO_ACTION.

Priority Conflict thắng sales và CRM.

Payment/Accounting thắng lời khách nói.

Shipping Core thắng suy đoán.

Product Activation thắng mong muốn bán hàng.

Customer Context thắng cá nhân hóa.

Evidence thắng tuyên bố PASS.

Runtime Context Package bắt buộc với action P0.

Không hardcode runtime data.

Runtime failure phải audit và không được che bằng câu trả lời đẹp.

Gateway vẫn bị chặn nếu thiếu evidence, E2E smoke và owner sign-off.

MASTER-04 tiếp tục ở PHẦN 2/4 với Runtime Resolver Registry theo 16 business domain, Required Inputs, Required Outputs, Guard Decision và bị chặn-if-Missing Rule.


## PHẦN 2/4 — RUNTIME RESOLVER REGISTRY BY 16 BUSINESS DOMAIN / REQUIRED INPUTS / REQUIRED OUTPUTS / GUARD DECISION / bị chặn-IF-MISSING RULE


## PHẦN 2/4 khóa Runtime Resolver Registry theo 16 business domain đã được xác lập trong chuỗi governance trước đó, đồng thời giữ đúng các nguyên tắc: Source-of-Truth quyết định dữ liệu, Owner Core quyết định nghiệp vụ, Runtime Resolver quyết định tại thời điểm thực thi, Consumer chỉ tiêu thụ trong boundary, Guard kiểm soát điều kiện an toàn, Evidence chứng minh dependency hoạt động và Release Gate quyết định trạng thái mở/release.


## 16. NGUYÊN TẮC ĐỌC RUNTIME RESOLVER REGISTRY


### 16.1. Runtime Resolver Registry là chuẩn governance

Runtime Resolver Registry trong MASTER-04 không phải danh sách function code.

Runtime Resolver Registry không phải API contract.

Runtime Resolver Registry không phải DTO.

Runtime Resolver Registry không phải database schema.

Runtime Resolver Registry là chuẩn governance để toàn bộ hệ thống Ginsengfood biết:

Domain nào cần resolver.

Resolver đó phải nhận input gì ở tầng nghiệp vụ.

Resolver đó phải trả output gì để Consumer được phép dùng.

Guard nào phải kiểm tra trước action.

Thiếu điều kiện nào thì phải block, suppress, fallback, handoff hoặc review.

ID nào phải được trace theo MASTER-03.

Dependency nào phải được tôn trọng theo MASTER-02.

Source nào phải được lấy từ Source-of-Truth theo MASTER-01.


### 16.2. Mẫu chuẩn cho mỗi Runtime Resolver Domain

Mỗi business domain trong PHẦN 2/4 được mô tả theo mẫu:

Business DomainDomain nghiệp vụ mà resolver phục vụ.

Primary Runtime ResolversCác nhóm resolver chính ở tầng governance.

Required InputsCác dữ liệu/ngữ cảnh bắt buộc phải có trước khi resolver ra kết quả.

Required OutputsCác dữ liệu/ID/trạng thái bắt buộc resolver phải trả về để Consumer được phép dùng.

Guard DecisionGuard phải quyết định action nào PASS, BLOCK, SUPPRESS, FALLBACK, HANDOFF, REVIEW_REQUIRED, ESCALATE hoặc NO_ACTION.

bị chặn-if-Missing RuleNếu thiếu input/output/ID/trace/evidence thì action nào phải bị chặn.


### 16.3. Resolver Output không được chỉ là dữ liệu thô

Mọi Runtime Resolver trong Ginsengfood phải trả về output có ngữ cảnh.

Một output hợp lệ tối thiểu phải thể hiện:

Resolution status.

Source-of-Truth reference.

Owner Core reference.

Domain reference.

Required canonical ID.

Correlation reference nếu thuộc luồng đa pack.

Guard precondition.

Validity/expiry nếu có.

Conflict status nếu có.

Block reason nếu không đủ điều kiện.

Audit reference nếu action P0.

Evidence reference nếu phục vụ smoke/release.

Consumer không được dùng output rời rạc thiếu context.


## 17. DOMAIN-01 — GOVERNANCE / SOURCE-OF-TRUTH


### 17.1. Business Domain

Governance / Source-of-Truth là domain nền để xác định dữ liệu nào là nguồn sự thật, dependency nào đang được dùng, pack nào là Owner Core, pack nào chỉ là Consumer và gate nào đang kiểm soát release.


### 17.2. Primary Runtime Resolvers

Các resolver chính:

SourceOfTruthResolver

DependencyResolver

DomainRegistryResolver

OwnerCoreResolver

ConsumerBoundaryResolver

ReleaseGateResolver

EvidenceStatusResolver


### 17.3. Required Inputs

Resolver domain này bắt buộc cần:

Business domain cần kiểm tra.

Pack đang yêu cầu dữ liệu.

Action dự kiến thực hiện.

Source category.

Owner Core dự kiến.

Consumer Pack dự kiến.

Required dependency.

Required evidence nếu action liên quan gate.

Correlation context nếu đi qua nhiều pack.

Audit context nếu action P0.


### 17.4. Required Outputs

Resolver phải trả về:

source_id

domain_id

dependency_id

Owner Core chính thức.

Consumer boundary được phép.

Source status.

Dependency status.

Evidence requirement nếu có.

Release gate requirement nếu có.

Runtime decision eligibility.

Block reason nếu thiếu nguồn hoặc thiếu dependency.

audit_id nếu phát sinh decision P0.


### 17.5. Guard Decision

SourceGuard phải quyết định:

PASS nếu source hợp lệ, owner rõ, dependency rõ.

BLOCK nếu thiếu source hoặc dependency.

REVIEW_REQUIRED nếu source đang chờ owner review.

FALLBACK nếu có public-safe fallback được định nghĩa.

NO_ACTION nếu request không thuộc phạm vi hoặc bị duplicate.


### 17.6. bị chặn-if-Missing Rule

Nếu thiếu source_id, Consumer không được dùng dữ liệu làm Source-of-Truth.

Nếu thiếu domain_id, không được xác định domain nghiệp vụ.

Nếu thiếu dependency_id, không được tuyên bố dependency đã hoạt động.

Nếu thiếu Owner Core, không được tạo quyết định runtime.

Nếu thiếu evidence cho gate, không được PASS.


## 18. DOMAIN-02 — PRODUCT KNOWLEDGE / CLAIM / BRAND / SCIENTIFIC PROOF


### 18.1. Business Domain

Domain này kiểm soát tri thức sản phẩm, claim được phép dùng, brand wording, scientific proof và public-safe wording.

AI, CRM, Gateway, Landing Page, ADS và MC AI không được tự bịa claim, không được thuốc hóa sản phẩm và không được tự diễn giải bằng chứng khoa học.


### 18.2. Primary Runtime Resolvers

ProductKnowledgeResolver

ClaimWhitelistResolver

BrandVoiceResolver

ScientificEvidenceResolver

PublicSafeWordingResolver

ProductEffectivenessResolver


### 18.3. Required Inputs

Product/SKU context nếu có.

Customer intent.

Channel context.

Public/private context.

Claim category.

Product Effectiveness requirement nếu có tư vấn sâu.

Scientific proof request nếu khách hỏi bằng chứng.

Health-sensitive context nếu có.

Correlation context.

Audit context nếu claim customer-facing.


### 18.4. Required Outputs

Product knowledge status.

Approved claim set.

Public-safe wording.

Disallowed claim list nếu có.

Approved evidence reference nếu có.

Product Effectiveness block nếu được phép dùng.

Health boundary flag nếu claim có nguy cơ thuốc hóa.

Allowed response boundary.

Block reason nếu claim chưa approved.

evidence_id nếu dùng bằng chứng khoa học.

audit_id nếu customer-facing.


### 18.5. Guard Decision

ClaimGuard và PublicWordingGuard phải quyết định:

PASS nếu claim đã approved và đúng public-safe wording.

BLOCK nếu claim chưa có nguồn hoặc có nguy cơ thuốc hóa.

FALLBACK nếu chỉ được dùng wording an toàn chung.

HANDOFF nếu khách hỏi nội dung nhạy cảm cần người phụ trách.

REVIEW_REQUIRED nếu evidence chưa được owner approved.


### 18.6. bị chặn-if-Missing Rule

Nếu thiếu claim source, không được dùng claim.

Nếu thiếu approved scientific evidence, không được khẳng định có bằng chứng khoa học.

Nếu thiếu public-safe wording, không được render câu trả lời public.

Nếu khách nêu bệnh, không được tư vấn như thuốc.

Nếu claim có nguy cơ vượt ranh giới thực phẩm thực dưỡng, phải block hoặc chuyển Health Boundary Guard.


## 19. DOMAIN-03 — CUSTOMER IDENTITY / CUSTOMER MEMORY


### 19.1. Business Domain

Domain này bảo đảm hệ thống biết đang nói chuyện với ai trước khi cá nhân hóa tư vấn, báo quyền lợi, báo quote, gửi CRM, xét Diamond/referral hoặc hỏi thăm trải nghiệm khách cũ.


### 19.2. Primary Runtime Resolvers

CustomerIdentityResolver

CustomerMemoryResolver

CustomerProfileResolver

CustomerContextResolver

OpenCaseResolver

PrivacyContextResolver


### 19.3. Required Inputs

Session context.

Customer signal hợp lệ.

Customer contact reference nếu có.

Prior order reference nếu có.

Member context request nếu có.

Diamond/referral context nếu có.

Open case check.

Privacy boundary check.

Audit context nếu dùng để cá nhân hóa.


### 19.4. Required Outputs

customer_identity_resolution_id

Customer identity status.

Customer type: new, existing, member, Diamond, buyer_from_diamond_link nếu resolve được.

customer_memory_snapshot_id nếu dùng lịch sử.

Last purchase summary nếu được phép dùng.

Prior recipient/purpose nếu có và được phép dùng.

Open case status.

Privacy restriction nếu có.

Personalization eligibility.

Block reason nếu chưa resolve.

correlation_id

audit_id


### 19.5. Guard Decision

CustomerContextGuard phải quyết định:

PASS nếu customer identity đủ để cá nhân hóa.

BLOCK nếu cần cá nhân hóa nhưng thiếu identity.

FALLBACK nếu chỉ được tư vấn chung.

SUPPRESS nếu có privacy case hoặc open case xung đột.

HANDOFF nếu cần xác minh danh tính.

REVIEW_REQUIRED nếu có tranh chấp hoặc dữ liệu conflict.


### 19.6. bị chặn-if-Missing Rule

Nếu thiếu customer_identity_resolution_id, không được cá nhân hóa giá, quyền lợi, member, Diamond, CRM hoặc lịch sử mua.

Nếu thiếu customer_memory_snapshot_id, không được nhắc sản phẩm đã mua gần nhất.

Nếu có privacy case mở, không được outbound không cần thiết.

Nếu có complaint/payment/delivery/health conflict mở, sales và CRM phải bị suppress theo priority.


## 20. DOMAIN-04 — SEGMENT RECOMMENDATION / PRODUCT RECOMMENDATION


### 20.1. Business Domain

Domain này kiểm soát đề xuất sản phẩm theo segment, nhu cầu, mùa, chức năng, bổ dưỡng, người thân, quà tặng, khách mới, khách cũ và bối cảnh chăm sóc.

Gợi ý sản phẩm không được là nội dung cảm tính không trace.


### 20.2. Primary Runtime Resolvers

SegmentResolver

ProductRecommendationResolver

ProductTriadResolver

DietaryBoundaryResolver

HealthBoundaryPrecheckResolver

ProductActivationPrecheckResolver


### 20.3. Required Inputs

Segment signal.

Customer context nếu cá nhân hóa.

Customer memory nếu dùng lịch sử mua.

Season/time context nếu gợi ý theo mùa.

Dietary/vegan/non-vegan context nếu có.

Health-sensitive context nếu khách nêu bệnh.

Gift/recipient context nếu có.

Product activation context.

Program context nếu có ý định mua.

Audit context.


### 20.4. Required Outputs

segment_recommendation_id

product_triad_recommendation_id

Recommendation rationale.

Product list theo trụ: mùa, chức năng, bổ dưỡng.

Product for family/recipient nếu có ngữ cảnh.

Product Effectiveness từng item.

Dietary compatibility.

Health boundary flag nếu có.

Product activation eligibility.

Program eligibility nếu có.

Allowed sales action.


### 20.5. Guard Decision

RecommendationGuard phải quyết định:

PASS nếu sản phẩm phù hợp và đủ activation/sellable nếu hướng mua.

BLOCK nếu sản phẩm không active/sellable hoặc conflict health/dietary.

FALLBACK nếu chỉ được tư vấn tri thức chung, chưa được chốt mua.

HANDOFF nếu cần tư vấn sâu hoặc có health-sensitive context.

SUPPRESS nếu có complaint/privacy/payment/delivery conflict.

REVIEW_REQUIRED nếu recommendation rule conflict.


### 20.6. bị chặn-if-Missing Rule

Nếu thiếu product_activation_id, không được gợi ý theo hướng chốt mua.

Nếu thiếu customer context, không được cá nhân hóa theo lịch sử mua.

Nếu khách nêu bệnh mà thiếu health guard, không được tư vấn sâu.

Nếu sản phẩm không sellable, không được đưa vào proposal bán hàng.

Nếu thiếu Product Effectiveness, không được tạo gợi ý sâu theo chuẩn Ginsengfood.


## 21. DOMAIN-05 — CRM 12-MONTH / MESSAGE TRIGGER / SUPPRESSION


### 21.1. Business Domain

Domain này kiểm soát CRM chăm sóc vòng đời D0, D1, D3, D7, D14, D21, D30, M2 đến M12.

CRM không phải spam khuyến mãi; CRM là chăm sóc có trigger, suppression, quiet hours, frequency cap, dedup, idempotency, immutable message text và audit.


### 21.2. Primary Runtime Resolvers

CRMTriggerResolver

CRMMessageJobResolver

CRMSuppressionResolver

FrequencyCapResolver

QuietHoursResolver

OptOutResolver

CRMContentEligibilityResolver

CRMProductSuggestionResolver


### 21.3. Required Inputs

Customer lifecycle stage.

Order/delivery/use estimate nếu có.

Message trigger context.

Customer memory snapshot nếu dùng lịch sử.

Opt-out status.

Quiet hours context.

Frequency cap context.

Open case context.

Product recommendation context nếu CRM có gợi ý mua lại.

Dedup context.

Idempotency context.


### 21.4. Required Outputs

message_trigger_id

crm_message_job_id

CRM eligibility status.

Suppression status.

Suppression reason nếu có.

Quiet hours decision.

Frequency cap decision.

Opt-out decision.

Message text version/reference.

Product recommendation reference nếu có.

dedup_key

idempotency_key

customer_memory_snapshot_id nếu dùng history.


### 21.5. Guard Decision

CRMGuard phải quyết định:

PASS nếu trigger hợp lệ và không bị suppression.

SUPPRESS nếu vi phạm quiet hours, frequency cap, opt-out hoặc priority conflict.

BLOCK nếu thiếu customer identity, trigger, dedup hoặc idempotency.

FALLBACK nếu chỉ được gửi chăm sóc không bán hàng.

NO_ACTION nếu duplicate hoặc không có trigger hợp lệ.

REVIEW_REQUIRED nếu customer state conflict.


### 21.6. bị chặn-if-Missing Rule

Nếu thiếu message_trigger_id, CRM không được gửi.

Nếu thiếu crm_message_job_id, không được có job outbound hợp lệ.

Nếu thiếu customer_identity_resolution_id, không được gửi CRM cá nhân hóa.

Nếu thiếu dedup_key hoặc idempotency_key, không được gửi tự động.

Nếu có complaint, privacy, payment dispute, delivery issue hoặc health review mở, CRM bán hàng phải suppress.


## 22. DOMAIN-06 — MEMBER LIFECYCLE / RIGHTS / DOWNGRADE / DISPUTE


### 22.1. Business Domain

Domain này kiểm soát chu kỳ thành viên, quyền lợi, duy trì hạng, ân hạng, hạ hạng, tranh chấp member và trạng thái sử dụng quyền lợi trong quote/order/CRM.

AI, CRM, Gateway, Landing Page không được tự tính hạng, doanh số, số còn thiếu hoặc quyền lợi.


### 22.2. Primary Runtime Resolvers

MemberIdentityResolver

MemberTierResolver

MemberRightsResolver

MemberLifecycleResolver

MemberGraceResolver

MemberDowngradeResolver

MemberDisputeResolver


### 22.3. Required Inputs

Member account context.

Membership cycle context.

Eligible revenue context.

Current tier context.

Grace period context.

Dispute context.

Quote/order context nếu áp quyền lợi.

CRM context nếu gửi thông điệp member.


### 22.4. Required Outputs

member_lifecycle_event_id

Member tier status.

Member rights eligibility.

Cycle status.

Grace status nếu có.

member_grace_event_id nếu có.

member_dispute_case_id nếu có tranh chấp.

Amount/right display eligibility nếu được phép.

Quote eligibility for member benefit.

CRM eligibility for member message.

Block/suppression reason nếu có.


### 22.5. Guard Decision

MemberGuard phải quyết định:

PASS nếu member context hợp lệ.

BLOCK nếu thiếu customer identity hoặc member lifecycle event.

SUPPRESS nếu có member dispute mở.

FALLBACK nếu chỉ được nói chính sách chung, không cá nhân hóa.

REVIEW_REQUIRED nếu tier/revenue/right conflict.

HANDOFF nếu cần xác minh hoặc xử lý tranh chấp.


### 22.6. bị chặn-if-Missing Rule

Nếu thiếu member_lifecycle_event_id, không được báo hạng, quyền lợi, doanh số còn thiếu hoặc trạng thái duy trì.

Nếu thiếu customer identity, không được resolve member.

Nếu có member_dispute_case_id đang mở, CRM upsell/nâng hạng phải suppress.

Nếu member data conflict, không được quote quyền lợi member cho đến khi resolved/reviewed.


## 23. DOMAIN-07 — DIAMOND / AFFILIATE / ENTREPRENEURSHIP / DISTRIBUTOR BOUNDARY


### 23.1. Business Domain

Domain này kiểm soát Diamond referral, affiliate, khởi nghiệp, bind, attribution, commission, conflict Ads/Diamond, self-purchase policy và ranh giới với đại lý/nhà phân phối/mua sỉ.


### 23.2. Primary Runtime Resolvers

DiamondEligibilityResolver

DiamondReferralLinkResolver

DiamondBindResolver

AffiliateAttributionResolver

CommissionEligibilityResolver

AdsDiamondConflictResolver

DistributorBoundaryResolver

CommissionDisputeResolver


### 23.3. Required Inputs

Member/Diamond status.

Referral link context.

Entry session context.

Buyer context.

Ads attribution context nếu có.

Order context nếu xét commission.

Payment/accounting context nếu xét payable.

Self-purchase context.

Distributor/wholesale intent nếu có.


### 23.4. Required Outputs

diamond_referral_link_id

diamond_bind_id nếu bind hợp lệ.

Attribution decision.

Diamond eligibility status.

Commission eligibility status.

commission_event_id nếu phát sinh commission event hợp lệ.

Ads/Diamond conflict decision nếu có.

Self-purchase decision nếu có.

Distributor boundary decision nếu intent thuộc mua sỉ/đại lý.

Payment/accounting eligibility reference.

Block/review/suppression reason nếu có.


### 23.5. Guard Decision

DiamondGuard phải quyết định:

PASS nếu link/bind/eligibility hợp lệ.

BLOCK nếu thiếu bind hoặc không đủ điều kiện.

SUPPRESS nếu có commission dispute hoặc member dispute liên quan.

FALLBACK nếu chỉ được nói chính sách chung.

HANDOFF nếu intent là đại lý/nhà phân phối/mua số lượng nhiều.

REVIEW_REQUIRED nếu conflict Ads/Diamond, self-purchase hoặc commission eligibility chưa rõ.


### 23.6. bị chặn-if-Missing Rule

Nếu thiếu diamond_bind_id, không được áp Diamond attribution.

Nếu thiếu eligible order, không được tạo commission event.

Nếu thiếu payment/accounting confirmation, commission chưa payable.

Nếu khách hỏi đại lý/nhà phân phối/mua sỉ, không được route nhầm sang Diamond/Affiliate.

Nếu thiếu member/Diamond context, không được xác nhận khách đủ điều kiện khởi nghiệp Diamond.


## 24. DOMAIN-08 — PRICING / PROGRAM / 24/7 / GOLDEN HOUR / QUOTESNAPSHOT


### 24.1. Business Domain

Domain này kiểm soát giá, chương trình 24/7, Giờ Vàng, quyền lợi member, quyền lợi Diamond link, thời điểm quote, QuoteSnapshot và QuoteCart.

AI không được báo giá nếu chưa có QuoteSnapshot hợp lệ.


### 24.2. Primary Runtime Resolvers

PricingResolver

ListedPriceResolver

ProgramResolver

GoldenHourResolver

EarlyAccessResolver

MemberBenefitResolver

DiamondBenefitResolver

QuoteSnapshotResolver

QuoteCartResolver

QuoteExpiryResolver


### 24.3. Required Inputs

Customer identity nếu quote cá nhân hóa.

Listed price status.

Program context.

Time context.

Golden Hour session context nếu có.

Early access context nếu có.

Member lifecycle context nếu có quyền lợi member.

Diamond bind context nếu có link Diamond.

Quantity/cart context.


### 24.4. Required Outputs

quote_snapshot_id

quote_cart_id nếu lập cart.

program_activation_id

golden_hour_session_id nếu có.

early_access_window_id nếu có.

Member benefit decision nếu có.

Diamond benefit decision nếu có.

Quote expiry.

Quote validity status.

Final quote eligibility.

customer_identity_resolution_id nếu quote cá nhân hóa.

product_activation_id


### 24.5. Guard Decision

QuoteGuard phải quyết định:

PASS nếu đủ product, price, program/customer context và QuoteSnapshot hợp lệ.

BLOCK nếu thiếu QuoteSnapshot, product activation, price hoặc customer context bắt buộc.

FALLBACK nếu chỉ được nói “cần kiểm tra giá trong kênh private” mà không quote.

HANDOFF nếu đang ở public comment/live.

REVIEW_REQUIRED nếu price/program conflict.

NO_ACTION nếu quote duplicate chưa hết hạn và policy dùng lại cho phép.


### 24.6. bị chặn-if-Missing Rule

Nếu thiếu quote_snapshot_id, AI không được báo giá.

Nếu thiếu product_activation_id, không được quote SKU.

Nếu thiếu program_activation_id, không được nói giá chương trình.

Nếu thiếu golden_hour_session_id, không được nói đang thuộc Giờ Vàng.

Nếu thiếu customer identity trong quote cá nhân hóa, không được báo quyền lợi member/Diamond.

Nếu QuoteSnapshot hết hạn, phải resolve quote mới.


## 25. DOMAIN-09 — PROGRAM / SELLABLE / PRODUCT ACTIVATION / PRODUCTION SIGNAL


### 25.1. Business Domain

Domain này kiểm soát sản phẩm nào được phép nói, được phép bán, được phép lên board, được phép vào chương trình, được phép vào live script và được phép kích hoạt trên kênh.

Quy tắc mở bán là P0 domain riêng.


### 25.2. Primary Runtime Resolvers

ProductActivationResolver

SellableStatusResolver

StockAvailabilityResolver

HoldStatusResolver

ProgramActivationResolver

GoldenHourSessionResolver

DailyLiveProductBoardResolver

MCAIScriptBriefResolver

ProductionPlanningSignalResolver


### 25.3. Required Inputs

Product/SKU context.

Finished goods status.

Inventory/stock availability.

Quality hold status.

Recall hold status.

Sale lock status.

Channel block status.

Golden Hour condition nếu có.

Live board context nếu có.

MC AI script context nếu có.


### 25.4. Required Outputs

Sellable decision.

Stock eligibility.

Hold/recall/sale lock/channel block status.

Listed price eligibility.

program_activation_id nếu thuộc chương trình.

golden_hour_session_id nếu thuộc Giờ Vàng.

daily_live_product_board_id nếu live.

mc_ai_script_brief_id nếu MC AI.

Production planning signal nếu phát sinh theo boundary.

Block reason nếu không được bán/nói.


### 25.5. Guard Decision

SellableGuard và ActivationGuard phải quyết định:

PASS nếu SKU active, sellable, có price, đúng chương trình, đúng kênh.

BLOCK nếu hết hàng, stock = 0, hold/recall/sale lock/channel block, price inactive hoặc sellable_status không hợp lệ.

SUPPRESS nếu sản phẩm không được nói ở channel hiện tại.

FALLBACK nếu cần gợi ý sản phẩm thay thế hợp lệ.

REVIEW_REQUIRED nếu stock/program/activation conflict.

NO_ACTION nếu SKU không thuộc board/live hiện tại.


### 25.6. bị chặn-if-Missing Rule

Nếu thiếu product_activation_id, không được nói/bán SKU theo hướng chốt mua.

Nếu thiếu sellable status, không được quote.

Nếu stock = 0 hoặc sản phẩm bị hold/recall/sale lock/channel block, phải Auto Stop Sale.

Nếu thiếu daily_live_product_board_id, live không được tự chọn sản phẩm bán.

Nếu thiếu mc_ai_script_brief_id, MC AI không được tự tạo script bán SKU.

Sales/Stock Movement có thể tạo Production Planning Signal nhưng không tự thành Production Order nếu chưa qua Operational Core / Owner Review.


## 26. DOMAIN-10 — PAYMENT / BANK TRANSFER / ACCOUNTING REVIEW


### 26.1. Business Domain

Domain này kiểm soát thanh toán, chuyển khoản, payment reference, bank transfer queue, accounting review và trạng thái PAID.

Khách nói đã chuyển tiền hoặc gửi ảnh giao dịch không tự động là PAID.


### 26.2. Primary Runtime Resolvers

PaymentMethodResolver

PaymentReferenceResolver

CompanyBankAccountRegistryResolver

BankTransferQueueResolver

AccountingReviewResolver

PaymentStatusResolver

PaymentDisputeResolver


### 26.3. Required Inputs

Order context.

Payment method context.

Company Bank Account Registry context.

Payment reference request.

Bank transfer signal nếu có.

Customer transaction evidence nếu có.

Accounting review context nếu cần.

Payment dispute context nếu có.


### 26.4. Required Outputs

Payment method eligibility.

payment_reference

Bank account display eligibility.

bank_transfer_queue_id nếu cần đối soát.

accounting_review_id nếu cần review.

PAID eligibility.

chưa đủ điều kiện/rejected/review status nếu có.

Payment dispute status nếu có.

Block reason nếu thiếu điều kiện.

order_code


### 26.5. Guard Decision

PaymentGuard phải quyết định:

PASS nếu payment instruction hoặc payment status đủ điều kiện.

BLOCK nếu thiếu payment reference, bank registry hoặc order context.

REVIEW_REQUIRED nếu cần Accounting Review.

SUPPRESS nếu có payment dispute.

FALLBACK nếu chỉ được nói đang chờ đối soát.

NO_ACTION nếu duplicate payment signal.


### 26.6. bị chặn-if-Missing Rule

Nếu thiếu payment_reference, không được hướng dẫn chuyển khoản chính thức.

Nếu thiếu Company Bank Account Registry, không được hiển thị tài khoản ngân hàng.

Nếu chỉ có ảnh giao dịch hoặc khách nói đã chuyển, không được chuyển PAID.

Nếu thiếu accounting_review_id trong luồng cần đối soát, payment phải giữ chưa đủ điều kiện/review.

Nếu có payment dispute mở, sales và CRM phải suppress.


## 27. DOMAIN-11 — SHIPPING / TRACKING / ETA / COD


### 27.1. Business Domain

Domain này kiểm soát phí ship, ETA, COD, tracking, event vận chuyển và phản hồi CSKH liên quan giao hàng.

AI, CRM, Gateway không được bịa trạng thái vận chuyển.


### 27.2. Primary Runtime Resolvers

ShippingEligibilityResolver

ShippingInfoResolver

ShippingFeeResolver

ETAResolver

CODResolver

TrackingResolver

ShippingEventResolver

DeliveryIssueResolver


### 27.3. Required Inputs

Shipping info nếu đã có.

Delivery address context theo boundary.

Shipping method context.

COD context nếu có.

Tracking request.

Latest shipping event.

Delivery issue context nếu có.


### 27.4. Required Outputs

Shipping eligibility.

Shipping info status.

Shipping fee nếu được resolve.

Latest tracking status.

shipping_tracking_event_id

Delivery issue status nếu có.

Block/fallback reason nếu không có dữ liệu.


### 27.5. Guard Decision

ShippingGuard phải quyết định:

PASS nếu có shipping data từ Shipping Core.

BLOCK nếu thiếu order/shipping event bắt buộc.

FALLBACK nếu chưa có ETA/tracking cụ thể.

SUPPRESS nếu có delivery issue mở ảnh hưởng CRM/sales.

REVIEW_REQUIRED nếu shipping data conflict.

NO_ACTION nếu duplicate shipping event.


### 27.6. bị chặn-if-Missing Rule

Nếu thiếu shipping_tracking_event_id, không được nói trạng thái vận chuyển cụ thể.

Nếu thiếu ETA từ Shipping Core, không được tự đoán ETA.

Nếu thiếu phí ship runtime, không được bịa phí ship.

Nếu order đã có shipping info, không được hỏi lại địa chỉ không cần thiết.

Nếu có delivery issue mở, CRM bán hàng phải suppress.


## 28. DOMAIN-12 — ORDER / ORDERDRAFT / CUSTOMERCONFIRMATION / IVR


### 28.1. Business Domain

Domain này kiểm soát luồng quote -> cart -> order draft -> customer confirmation -> order code, đồng thời khóa phạm vi IVR chỉ xác nhận đơn.

Không được tạo order chính thức nếu thiếu OrderDraft và CustomerConfirmation.


### 28.2. Primary Runtime Resolvers

OrderDraftResolver

OrderDraftRevisionResolver

CustomerConfirmationResolver

OrderCreationResolver

OrderStatusResolver

IVROrderConfirmationResolver

OrderIdempotencyResolver


### 28.3. Required Inputs

QuoteCart.

Price/program context đã được quote.

Shipping/payment selection nếu thuộc draft.

Customer confirmation signal.

Draft revision context nếu có sửa.

IVR confirmation context nếu có.


### 28.4. Required Outputs

quote_cart_id

order_confirmation_draft_id

Draft status.

Draft revision/supersede status nếu có.

customer_confirmation_id

Confirmation status.

order_code nếu order được tạo thành công.

IVR confirmation reference nếu có.

Order status.


### 28.5. Guard Decision

OrderGuard phải quyết định:

PASS nếu đủ QuoteSnapshot, OrderDraft và CustomerConfirmation.

BLOCK nếu thiếu quote, draft, confirmation hoặc customer context.

REVIEW_REQUIRED nếu order data conflict.

FALLBACK nếu chỉ được gửi lại bản nháp xác nhận.

NO_ACTION nếu duplicate confirmation đã xử lý.

BLOCK nếu IVR vượt phạm vi ORDER_CONFIRMATION_ONLY.


### 28.6. bị chặn-if-Missing Rule

Nếu thiếu quote_snapshot_id, không được lập OrderDraft.

Nếu thiếu order_confirmation_draft_id, không được yêu cầu khách xác nhận đơn hợp lệ.

Nếu thiếu customer_confirmation_id, không được tạo order chính thức.

Nếu thiếu order_code, không được nói đặt hàng thành công.

Nếu IVR cố đổi order state ngoài xác nhận đơn, phải block.


## 29. DOMAIN-13 — GATEWAY / PUBLIC COMMENT / MESSENGER HANDOFF / MODERATION


### 29.1. Business Domain

Domain này kiểm soát public comment, live comment, Messenger handoff, delivery log, public/private boundary, moderation, dedup và idempotency.

Public comment/live không được báo giá, chốt đơn, lặp PII, hướng dẫn payment hoặc tư vấn health detail dài.


### 29.2. Primary Runtime Resolvers

PublicCommentEventResolver

ChannelPolicyResolver

MessengerHandoffResolver

HandoffDeliveryLogResolver

PublicPrivateBoundaryResolver

PIIGuardResolver

LiveModerationResolver

GatewayDedupResolver

GatewayIdempotencyResolver


### 29.3. Required Inputs

Page/channel context.

Live/comment context.

Public comment content classification.

Intent classification.

PII/payment/health/price/buying/Diamond signals.

Messenger handoff policy context.

Delivery capability context.

Moderation context.


### 29.4. Required Outputs

public_comment_event_id

Public/private decision.

Handoff requirement.

messenger_handoff_event_id nếu handoff được thực hiện.

handoff_delivery_log_id

Handoff status: success/fail/chưa đủ điều kiện.

Public fallback eligibility.

live_moderation_event_id nếu có moderation.

PII/payment/health/price boundary decision.


### 29.5. Guard Decision

GatewayGuard phải quyết định:

PASS nếu public response an toàn hoặc private handoff hợp lệ.

BLOCK nếu public action vi phạm policy.

HANDOFF nếu intent cần private.

FALLBACK nếu handoff fail và có fallback an toàn.

SUPPRESS nếu comment spam/troll/abusive theo rule.

ESCALATE nếu legal/privacy/security/abuse nghiêm trọng.

NO_ACTION nếu duplicate hoặc không cần phản hồi.


### 29.6. bị chặn-if-Missing Rule

Nếu thiếu public_comment_event_id, không được xử lý comment như event hợp lệ.

Nếu thiếu messenger_handoff_event_id, không được nói đã handoff.

Nếu handoff_delivery_log_id không success, không được nói đã gửi Messenger.

Nếu thiếu dedup/idempotency, không được bật auto-reply quy mô live.

Nếu comment chứa PII/payment/health/giá/mua hàng, public reply phải bị giới hạn.


## 30. DOMAIN-14 — OFFICIAL CONTACT / PHONE NUMBER REGISTRY


### 30.1. Business Domain

Domain này kiểm soát số điện thoại/kênh liên hệ chính thức được phép hiển thị theo từng intent.

Không Consumer nào được tự gửi số điện thoại nếu chưa qua Official Contact Registry.


### 30.2. Primary Runtime Resolvers

OfficialContactResolver

ContactIntentResolver

ContactPolicyResolver

ContactDisplayEligibilityResolver


### 30.3. Required Inputs

Contact category request.

Official Contact Registry context.

Contact display policy.


### 30.4. Required Outputs

official_contact_id

Contact category.

Contact value display eligibility.

Allowed wording.

Intent-contact mapping.

Public/private display boundary.

Block reason nếu intent chưa rõ hoặc contact không được phép.


### 30.5. Guard Decision

OfficialContactGuard phải quyết định:

PASS nếu intent rõ và contact được phép hiển thị.

BLOCK nếu thiếu official_contact_id.

FALLBACK nếu chỉ được nói sẽ có bộ phận phù hợp hỗ trợ.

HANDOFF nếu intent cần người phụ trách.

REVIEW_REQUIRED nếu intent/contact conflict.

NO_ACTION nếu request không thuộc phạm vi contact.


### 30.6. bị chặn-if-Missing Rule

Nếu thiếu official_contact_id, không được gửi số điện thoại.

Nếu intent chưa rõ, không được tự chọn số.

Không được cung cấp số cá nhân lãnh đạo.

Không được hardcode số điện thoại trong AI, CRM, Gateway, Landing Page, template hoặc script.


## 31. DOMAIN-15 — HEALTH BOUNDARY / COMPLAINT / PRIORITY CONFLICT


### 31.1. Business Domain

Domain này kiểm soát health-sensitive context, complaint, privacy, counterfeit, refund, payment dispute, delivery issue, member dispute, commission dispute và các priority conflict thắng sales/CRM.


### 31.2. Primary Runtime Resolvers

HealthBoundaryResolver

ComplaintResolver

PrivacyCaseResolver

CounterfeitCaseResolver

PriorityConflictResolver

SuppressionReasonResolver

EscalationResolver

CaseClosureResolver


### 31.3. Required Inputs

Customer identity nếu có.

Customer message/intent classification.

Health-sensitive signal nếu có.

Complaint signal nếu có.

Privacy signal nếu có.

Counterfeit signal nếu có.

Order context nếu liên quan.

Payment context nếu liên quan.

Shipping context nếu liên quan.

Member/Diamond context nếu liên quan.

Existing open case context.


### 31.4. Required Outputs

health_guard_case_id nếu có health boundary.

complaint_case_id nếu có khiếu nại.

privacy_case_id nếu có privacy issue.

counterfeit_case_id nếu nghi hàng giả.

member_dispute_case_id nếu có tranh chấp member.

Priority conflict status.

Suppression decision.

Escalation requirement.

Allowed safe response boundary.

Case status.

Closure eligibility nếu có.

Block/suppress reason.

customer_identity_resolution_id nếu có.


### 31.5. Guard Decision

PriorityConflictGuard phải quyết định:

PASS nếu không có conflict và action an toàn.

SUPPRESS nếu sales/CRM/upsell phải dừng.

BLOCK nếu action có nguy cơ vi phạm safety/privacy/complaint.

HANDOFF nếu cần CSKH, QA, kế toán, privacy, legal, owner review.

ESCALATE nếu nghiêm trọng.

FALLBACK nếu chỉ được trả lời an toàn.

REVIEW_REQUIRED nếu case cần xử lý trước khi tiếp tục.


### 31.6. bị chặn-if-Missing Rule

Nếu khách nêu bệnh mà thiếu health_guard_case_id, không được tư vấn sâu.

Nếu có complaint_case_id mở, CRM bán hàng phải suppress.

Nếu có privacy_case_id mở, outbound không cần thiết phải dừng.

Nếu có counterfeit_case_id mở, sales phải dừng hoặc chuyển đúng luồng kiểm tra.

Nếu có payment dispute/delivery issue/member dispute/commission dispute, upsell và CRM bán hàng phải suppress.


## 32. DOMAIN-16 — EVIDENCE / COMPLETION REPORT / GATEWAY GATE / SECURITY


### 32.1. Business Domain

Domain này kiểm soát evidence, smoke, security, App Review, UAT, E2E, Completion Report, Completion Gate, Owner Sign-off, Gateway Gate, Release Approved và Go-live Approved.


### 32.2. Primary Runtime Resolvers

EvidenceResolver

SmokeStatusResolver

EvidenceReviewResolver

SecurityEvidenceResolver

CompletionReportResolver

CompletionGateResolver

GatewayGateResolver

OwnerSignoffResolver

ReleaseReadinessResolver

GoLiveApprovalResolver


### 32.3. Required Inputs

Domain/gate cần xét.

Required dependency list.

Required smoke list.

Evidence review status.

Security/App Review/UAT/E2E status nếu thuộc phạm vi.

P0 gate status.

Open điểm chặn list.

Owner review context.

Owner sign-off context.

Completion Report status.


### 32.4. Required Outputs

evidence_id

Evidence status.

smoke_id

Smoke status.

completion_gate_id

Completion gate status.

owner_signoff_id nếu đã sign-off.

Gateway gate eligibility.

Production readiness eligibility.

Release approval eligibility.

Go-live approval eligibility.

Missing evidence list.

Block reason nếu chưa đủ điều kiện.


### 32.5. Guard Decision

ReleaseGateGuard phải quyết định:

PASS nếu đủ evidence accepted, smoke PASS, P0 gate PASS, owner sign-off PASS và không còn điểm chặn.

BLOCK nếu thiếu evidence, smoke, owner sign-off hoặc có điểm chặn P0.

REVIEW_REQUIRED nếu evidence/owner/security đang chờ review.

NO_ACTION nếu gate chưa đến thời điểm xét.

ESCALATE nếu security hoặc policy điểm chặn nghiêm trọng.

FALLBACK không được dùng để chuyển PASS.


### 32.6. bị chặn-if-Missing Rule

Nếu thiếu evidence_id, smoke không PASS.

Nếu thiếu smoke_id, evidence không đủ ngữ cảnh.

Nếu thiếu completion_gate_id, Gateway không được PASS.

Nếu thiếu owner_signoff_id, release không approved.


## 33. CROSS-DOMAIN RUNTIME CHAIN REGISTRY


### 33.1. Public Comment -> Messenger -> Quote

Runtime chain bắt buộc:

QuoteGuard

Required outputs:

messenger_handoff_event_id

bị chặn-if-missing:

Handoff không success thì không nói đã gửi Messenger.

Thiếu product activation thì không quote.

Thiếu QuoteSnapshot thì không báo giá.


### 33.2. Quote -> OrderDraft -> CustomerConfirmation -> OrderCode

OrderGuard

Thiếu QuoteSnapshot thì không lập draft.

Thiếu CustomerConfirmation thì không tạo order.

Thiếu OrderCode thì không nói đặt hàng thành công.

Duplicate confirmation không được tạo nhiều order.


### 33.3. Order -> Payment -> Accounting Review

PaymentGuard

bank_transfer_queue_id

accounting_review_id

Thiếu PaymentReference thì không hướng dẫn chuyển khoản.

Thiếu Bank Registry thì không hiển thị tài khoản ngân hàng.

Thiếu AccountingReview trong luồng cần đối soát thì không PAID.


### 33.4. Order -> Shipping -> Customer Care

ShippingGuard

Shipping fee nếu cần.

Thiếu shipping event thì không nói trạng thái cụ thể.

Thiếu ETA thì không tự đoán.

Delivery issue mở thì CRM bán hàng suppress.


### 33.5. CRM Trigger -> Care Message -> Recommendation -> Quote

QuoteSnapshotResolver nếu khách có tín hiệu mua

CRMGuard

QuoteGuard nếu báo giá

customer_memory_snapshot_id

quote_snapshot_id nếu báo giá

Thiếu trigger thì không gửi CRM.

Có suppression thì không gửi sales CRM.

Thiếu ProductActivation thì không chốt mua.


### 33.6. Diamond Link -> Bind -> Order -> Commission

DiamondGuard

diamond_bind_id

commission_event_id

No payment/accounting confirmation = commission not payable.

Conflict Ads/Diamond phải review/audit.


### 33.7. Complaint / Health / Privacy -> Suppression -> Resolution

HealthBoundaryResolver hoặc ComplaintResolver hoặc PrivacyCaseResolver hoặc CounterfeitCaseResolver

EscalationResolver nếu cần

CaseClosureResolver nếu đóng case

PriorityConflictGuard

health_guard_case_id hoặc complaint_case_id hoặc privacy_case_id hoặc counterfeit_case_id

Escalation decision nếu có.

Safe response boundary.

Thiếu case ID thì không suppress âm thầm.

Case mở thì sales/CRM phải dừng theo phạm vi.

Health-sensitive thiếu guard thì không tư vấn sâu.

Privacy case mở thì không outbound không cần thiết.


### 33.8. Evidence -> Smoke -> Completion Gate -> Owner Sign-off

GatewayGateResolver nếu thuộc Gateway

ReleaseGateGuard

owner_signoff_id

Gate status.

Missing evidence list nếu có.

Thiếu evidence accepted thì không smoke PASS.

Thiếu smoke PASS thì không completion gate PASS.

Thiếu owner sign-off thì không release approved.


## 34. RUNTIME RESOLVER REGISTRY — CONSUMER USAGE RULE


### 34.1. AI Advisor

AI Advisor chỉ được dùng output của resolver để:

Tư vấn.

Gợi ý sản phẩm.

Chăm sóc khách.

Báo quote khi có QuoteSnapshot.

Lập OrderDraft khi OrderGuard cho phép.

Hỏi thăm khách cũ khi có customer memory.

Render public-safe wording khi ClaimGuard cho phép.

AI Advisor không được tự tạo dữ liệu runtime.


### 34.2. Gateway

Gateway chỉ được dùng output của resolver để:

Phân loại comment.

Chạy public/private boundary.

Ghi delivery log.

Moderation.

Dedup.

Fallback an toàn.

Gateway không được quote public, không chốt đơn public, không lặp PII, không nói đã handoff khi delivery log chưa success.


### 34.3. CRM

CRM chỉ được dùng output của resolver để:

Gửi message khi có trigger.

Chăm sóc vòng đời.

Hỏi trải nghiệm.

Gợi ý mua lại nếu không bị suppression.

Dùng Product Effectiveness khi recommendation hợp lệ.

Dừng khi conflict mở.

CRM không được gửi nếu thiếu trigger, identity, dedup, idempotency hoặc có suppression.


### 34.4. Landing Page / ADS / MC AI

Landing Page, ADS và MC AI chỉ được nói/bán sản phẩm khi:

ProductActivationResolver PASS.

SellableStatusResolver PASS.

ProgramResolver PASS nếu có chương trình.

ClaimGuard PASS nếu dùng claim.

QuoteGuard PASS nếu báo giá.

Public/private/channel boundary PASS.

Không được tự chọn SKU, tự hardcode giá, tự nói chương trình, tự nói tồn kho hoặc tự tạo quote.


### 34.5. Payment / Accounting / Shipping / Order

Các domain này chỉ được hành động khi Resolver và Guard tương ứng PASS:

Order phải có OrderDraft và CustomerConfirmation.

Payment phải có PaymentReference và Payment/Accounting decision.

Shipping phải có Shipping Core data.

Accounting Review phải có review context.

Không domain nào được dùng lời khách nói để thay Owner Core.


## 35. KẾT LUẬN PHẦN 2/4


## PHẦN 2/4 của MASTER-04 đã khóa Runtime Resolver Registry theo 16 business domain chính.

Mỗi domain P0 phải có Runtime Resolver rõ.

Mỗi resolver phải có Required Inputs.

Mỗi resolver phải có Required Outputs.

Output không được là dữ liệu thô rời rạc.

Output phải có source, owner, trace ID, status, guard precondition và audit context nếu cần.

Guard phải quyết định rõ PASS, BLOCK, SUPPRESS, FALLBACK, HANDOFF, REVIEW_REQUIRED, ESCALATE hoặc NO_ACTION.

Thiếu ID bắt buộc thì action phải block.

Thiếu customer context thì không cá nhân hóa.

Thiếu OrderDraft/CustomerConfirmation thì không tạo order.

Thiếu PaymentReference/Accounting Review thì không PAID.

Thiếu Shipping Core thì không nói trạng thái vận chuyển.

Thiếu ProductActivation/Sellable thì không bán.

Thiếu OfficialContact thì không gửi số điện thoại.

Thiếu evidence accepted thì không PASS.

Consumer không được tự suy luận rule kinh doanh.

Consumer không được hardcode runtime data.

Resolver/Guard là cổng bắt buộc trước mọi action P0.

MASTER-04 tiếp tục ở PHẦN 3/4 với Guard Priority Matrix, Suppression Control, Fallback Control, Handoff Control và Runtime Conflict Resolution.


## PHẦN 3/4 — GUARD PRIORITY MATRIX / SUPPRESSION CONTROL / FALLBACK CONTROL / HANDOFF CONTROL / RUNTIME CONFLICT RESOLUTION


## 36. MỤC TIÊU CỦA PHẦN 3/4


## PHẦN 3/4 quy định lớp kiểm soát ưu tiên runtime của hệ thống Ginsengfood.

Mục tiêu là bảo đảm khi nhiều rule, nhiều resolver, nhiều domain hoặc nhiều trạng thái cùng xuất hiện tại thời điểm thực thi, hệ thống luôn biết rule nào thắng, action nào được phép, action nào phải dừng, action nào phải suppress, action nào phải fallback, action nào phải handoff và conflict nào phải đưa vào review.


## PHẦN 3/4 khóa 5 nhóm kiểm soát chính:

Guard Priority Matrix.

Suppression Control.

Fallback Control.

Handoff Control.

Runtime Conflict Resolution.


## PHẦN 3/4 không viết code, không viết API, không viết DTO, không viết database schema, không viết worker implementation và không thay thế tài liệu implementation của từng pack.


## 37. NGUYÊN TẮC GUARD PRIORITY MATRIX


### 37.1. Guard Priority Matrix là gì

Guard Priority Matrix là bảng ưu tiên quyết định khi có nhiều rule cùng tác động lên một action runtime.

Khách đang có complaint mở nhưng CRM đến lịch gửi mua lại.

Trong trường hợp này, CRM Trigger có thể hợp lệ, Customer Memory có thể hợp lệ, Product Recommendation có thể hợp lệ, nhưng Complaint Guard có ưu tiên cao hơn Sales/CRM. Vì vậy action gửi CRM bán hàng phải bị SUPPRESS.

Guard Priority Matrix bảo đảm hệ thống không chọn nhánh có lợi cho bán hàng nếu đang có rule an toàn, pháp lý, quyền riêng tư, thanh toán, vận chuyển, khiếu nại hoặc evidence/release đang chặn.


### 37.2. Guard ưu tiên cao hơn Consumer Action

Consumer Action luôn đứng sau Guard.

AI Advisor, Gateway, CRM, Landing Page, ADS, MC AI, IVR, Admin UI, Frontend, Template Engine, Channel Adapter và Customer Care Tool không được tự quyết định action nếu Guard chưa PASS.

Consumer không được dùng nội dung hội thoại, cảm tính bán hàng, script có sẵn hoặc template để vượt Guard.

Nếu Guard trả về BLOCK, SUPPRESS, HANDOFF, REVIEW_REQUIRED hoặc ESCALATE, Consumer phải tuân thủ.


### 37.3. Guard Priority Matrix áp dụng tại thời điểm thực thi

Priority Matrix không phải policy tĩnh để đọc tham khảo.

Priority Matrix phải được áp dụng tại thời điểm action được yêu cầu.

Một action từng được phép trước đó có thể bị chặn tại thời điểm hiện tại nếu runtime context thay đổi.

Sản phẩm từng sellable nhưng hiện bị quality hold thì không được bán.

Quote từng hợp lệ nhưng đã hết hạn thì không được dùng.

CRM từng scheduled nhưng khách vừa mở complaint thì phải suppress.

Payment từng chưa đủ điều kiện nhưng chưa accounting review thì không được PAID.

Gateway từng đủ điều kiện handoff nhưng delivery log fail thì không được nói đã gửi Messenger.

Evidence từng submitted nhưng chưa accepted thì không được PASS.


## 38. MASTER GUARD PRIORITY ORDER


### 38.1. Thứ tự ưu tiên tổng quát

MASTER-04 khóa thứ tự ưu tiên tổng quát của Guard như sau:

Priority 01 — Security / Privacy / Legal / Abuse / Policy SafetyCác vấn đề bảo mật, quyền riêng tư, pháp lý, lạm dụng, policy platform, dữ liệu nhạy cảm và nguy cơ gây hại thắng mọi action bán hàng.

Priority 02 — Health Boundary / Complaint / Counterfeit / Quality RiskHealth-sensitive, complaint, refund, counterfeit, quality risk, recall signal và khiếu nại chất lượng thắng sales, CRM, upsell và quote.

Priority 03 — Payment / Accounting / Financial DisputePayment Core, Accounting Review, bank transfer queue, payment dispute và trạng thái tài chính thắng lời khách nói, ảnh giao dịch và nội dung chat.

Priority 04 — Order Validity / CustomerConfirmation / IVR BoundaryOrder chỉ hợp lệ khi có QuoteSnapshot, OrderDraft, CustomerConfirmation và OrderCode. IVR chỉ xác nhận đơn, không đổi order state.

Priority 05 — Shipping / Delivery / COD / TrackingShipping Core quyết định ETA, COD, phí ship và tracking. Delivery issue thắng CRM bán hàng.

Priority 06 — Product Sellable / Stock / Hold / Program ActivationSellable Gate, stock, price active, hold, recall, sale lock, channel block và product activation thắng mong muốn bán hàng.

Priority 07 — Pricing / Program / QuoteSnapshotKhông QuoteSnapshot thì không báo giá. Không program activation thì không nói giá chương trình.

Priority 08 — Customer Context / Member / Diamond / ReferralKhông customer identity thì không cá nhân hóa. Không member lifecycle thì không nói quyền lợi member. Không diamond bind thì không áp Diamond attribution.

Priority 09 — CRM / Recommendation / Sales OptimizationCRM, gợi ý sản phẩm, combo, upsell, chăm sóc mua lại chỉ được chạy khi không bị các priority cao hơn chặn.

Priority 10 — Evidence / Completion / Release / Gateway GateKhông evidence accepted, smoke PASS, owner sign-off và completion gate PASS thì không Gateway PASS, không Production Ready, không Release Approved, không Go-live Approved.


### 38.2. Nguyên tắc “priority cao thắng priority thấp”

Khi hai Guard cho kết quả khác nhau, Guard có priority cao hơn thắng.

CRMGuard PASS nhưng ComplaintGuard SUPPRESS -> kết quả cuối là SUPPRESS.

ProductRecommendationGuard PASS nhưng SellableGuard BLOCK -> kết quả cuối là BLOCK.

QuoteGuard PASS nhưng PaymentDisputeGuard SUPPRESS sales -> không tiếp tục chốt đơn.

GatewayPublicReplyGuard PASS chung nhưng PIIGuard BLOCK -> public reply phải block hoặc handoff.

EvidenceResolver có một phần evidence nhưng ReleaseGateGuard BLOCK -> không Gateway PASS.


### 38.3. Priority không được ghi đè bằng cấu hình bán hàng

Không cấu hình marketing, campaign, live board, CRM schedule, MC AI script, ADS target, combo proposal hoặc sales target nào được phép ghi đè Guard Priority Matrix.

Nếu board live đang đẩy một SKU nhưng Product Activation Guard trả BLOCK, SKU đó phải dừng.

Nếu campaign đang chạy nhưng Payment/Privacy/Complaint Guard chặn, sales action phải dừng.

Nếu CRM scheduled nhưng Quiet Hours hoặc Complaint Guard chặn, CRM không được gửi.


## 39. GUARD PRIORITY MATRIX THEO DOMAIN


### 39.1. Security / Privacy / Legal Guard

Ưu tiên: cao nhất.

Guard này thắng:

AI sales.

Gateway public reply.

Payment reminder.

Product recommendation.

Diamond/affiliate promotion.

Member upgrade message.

MC AI live script.

ADS retargeting.

Release gate nếu có security điểm chặn.

Các action bắt buộc chặn hoặc handoff:

Lộ PII.

Lộ payment/private info.

Privacy request.

Platform policy risk.

Unauthorized data access.

Token/secret/bank account exposure.

Decision hợp lệ:

ESCALATE.

Không được fallback sang bán hàng.


### 39.2. Health Boundary Guard

Ưu tiên: cao hơn sales/CRM/recommendation.

Guard này kích hoạt khi khách nêu bệnh, điều trị, thuốc, kiêng cữ, triệu chứng, người bệnh, phụ nữ mang thai, trẻ nhỏ hoặc ngữ cảnh sức khỏe nhạy cảm.

Action được phép:

Tư vấn theo thực dưỡng an toàn.

Nhắc sản phẩm không phải thuốc.

Không thay thuốc.

Không cam kết khỏi bệnh.

Handoff nếu cần tư vấn sâu hoặc case nhạy cảm.

Action không được phép:

Cam kết điều trị.

Gợi ý thay thuốc.

Chẩn đoán bệnh.

Tư vấn y khoa chuyên sâu.

Tạo claim lâm sàng.

Ép mua bằng lý do bệnh.

PASS với wording an toàn.

BLOCK nếu vượt claim.

HANDOFF nếu cần tư vấn sâu.

REVIEW_REQUIRED nếu ngữ cảnh nhạy cảm.

SUPPRESS sales nếu health review mở.


### 39.3. Complaint / Refund / Quality Guard

Ưu tiên: cao hơn sales, CRM, upsell, member upgrade, Diamond promotion.

Guard này kích hoạt khi khách khiếu nại sản phẩm, dịch vụ, giao hàng, thanh toán, hoàn tiền, chất lượng, nghi hàng giả hoặc trải nghiệm xấu.

Xin lỗi và tiếp nhận.

Xác minh thông tin.

Mở case.

Chuyển CSKH/QA/kế toán/pháp lý theo domain.

Tạm dừng bán hàng.

Ghi suppression.

Tiếp tục upsell.

Gửi CRM bán hàng.

Ép combo.

Dẫn khách sang sản phẩm khác ngay khi complaint chưa xử lý.

Kết luận lỗi sản xuất khi QA chưa xác minh.

Kết luận hàng giả công khai khi chưa có xác minh.

FALLBACK an toàn.

BLOCK sales.


### 39.4. Payment / Accounting Guard

Ưu tiên: cao hơn lời khách nói và ảnh giao dịch.

Guard này quyết định:

Có được gửi payment instruction không.

Có được tạo payment reference không.

Có được đưa vào bank transfer queue không.

Có cần accounting review không.

Có được chuyển PAID không.

Có payment dispute không.

Nói đã PAID chỉ vì khách nói đã chuyển.

Nói đã PAID chỉ vì có ảnh giao dịch.

Hiển thị tài khoản ngân hàng ngoài Bank Registry.

Hardcode tài khoản ngân hàng trong AI/CRM/Gateway/Admin UI/frontend/template.

Tạo payment reference ngoài Payment Core.

Nhắc thanh toán khi đang có payment dispute.

PASS.

FALLBACK chưa đủ điều kiện review.

NO_ACTION duplicate signal.


### 39.5. Order / CustomerConfirmation / IVR Guard

Ưu tiên: cao hơn sales script và channel action.

Quote có đủ để lập OrderDraft không.

OrderDraft có đủ để khách xác nhận không.

CustomerConfirmation có hợp lệ không.

Có được tạo OrderCode không.

IVR có đúng phạm vi ORDER_CONFIRMATION_ONLY không.

Tạo order nếu thiếu CustomerConfirmation.

Nói đặt hàng thành công nếu thiếu OrderCode.

Dùng IVR để upsell.

Dùng IVR để đổi order state ngoài xác nhận đơn.

Tạo nhiều order do double confirm.

Sửa draft đã xác nhận không có revision/supersede.

FALLBACK gửi lại draft.

NO_ACTION duplicate confirmation.


### 39.6. Shipping / Delivery Guard

Ưu tiên: cao hơn CSKH tự suy đoán và CRM bán hàng.

Có được nói ETA không.

Có được nói tracking status không.

Có được nói COD status không.

Có được nói phí ship không.

Có delivery issue đang mở không.

Có cần suppress CRM sales không.

Bịa trạng thái vận chuyển.

Tự đoán ETA.

Tự bịa phí ship.

Hỏi lại địa chỉ nếu order đã có shipping info.

Gửi CRM bán hàng khi delivery issue mở.

FALLBACK chưa có cập nhật.

SUPPRESS CRM sales.

NO_ACTION duplicate event.


### 39.7. Product Sellable / Activation Guard

Ưu tiên: cao hơn marketing, ADS, live board, CRM, AI chốt đơn và MC AI script.

SKU có được nói theo hướng bán không.

SKU có được quote không.

SKU có được lên live board không.

SKU có được vào chương trình không.

SKU có được MC AI nói trong script không.

Bán SKU hết hàng.

Bán SKU stock_available = 0.

Bán SKU bị quality hold.

Bán SKU bị recall hold.

Bán SKU bị sale lock.

Bán SKU bị channel block.

Quote SKU price inactive.

Nói SKU active nếu thiếu product_activation_id.

SUPPRESS theo channel.

FALLBACK gợi ý sản phẩm thay thế hợp lệ.

REVIEW_REQUIRED nếu conflict stock/program/price.

NO_ACTION nếu SKU ngoài board.


### 39.8. Pricing / Program / Quote Guard

Ưu tiên: cao hơn nội dung sales.

Có được báo giá không.

Có QuoteSnapshot không.

Quote có còn hạn không.

Program có active không.

Golden Hour có active không.

Early access có hợp lệ không.

Member/Diamond benefit có được áp không.

Báo giá khi thiếu QuoteSnapshot.

Nói giá chương trình khi thiếu program_activation_id.

Nói đang Giờ Vàng khi thiếu golden_hour_session_id.

Nói quyền lợi member khi thiếu member_lifecycle_event_id.

Nói quyền lợi Diamond khi thiếu diamond_bind_id.

Dùng quote hết hạn.

Cập nhật ngược quote cũ.

HANDOFF nếu public channel.

FALLBACK nếu chỉ được mời vào kênh private nhưng không quote.

NO_ACTION nếu duplicate quote còn hiệu lực theo policy.


### 39.9. Customer Context / Member / Diamond Guard

Ưu tiên: cao hơn cá nhân hóa, CRM và quote quyền lợi.

Có biết khách là ai không.

Có được dùng lịch sử mua không.

Có được nói member tier không.

Có được nói quyền lợi member không.

Có được nói Diamond/affiliate eligibility không.

Có được áp Diamond bind không.

Nhắc lịch sử mua khi thiếu memory snapshot.

Nói khách là member khi chưa resolve.

Tự tính hạng/doanh số/số còn thiếu.

Tự xác nhận Diamond bind.

Tự tính commission.

Route nhầm Diamond sang đại lý.

Cá nhân hóa khi privacy boundary không cho phép.

FALLBACK tư vấn chung.

HANDOFF xác minh.

SUPPRESS nếu privacy/member dispute/commission dispute.

REVIEW_REQUIRED nếu identity/member conflict.


### 39.10. CRM / Recommendation / Sales Optimization Guard

Ưu tiên: thấp hơn các guard an toàn, payment, shipping, order, product activation và quote.

Có được gửi CRM không.

Có được gợi ý mua lại không.

Có được gợi ý combo không.

Có được upsell không.

Có được nhắc nâng hạng không.

Có được nhắc Diamond/affiliate không.

Gửi CRM khi thiếu trigger.

Gửi CRM khi thiếu customer identity.

Gửi CRM khi thiếu dedup/idempotency.

Gửi CRM ngoài quiet hours.

Vượt frequency cap.

Gửi khi opt-out.

Gửi bán hàng khi có priority conflict.

Ép combo trái ngữ cảnh.

FALLBACK chăm sóc không bán hàng.

NO_ACTION duplicate/no trigger.


### 39.11. Evidence / Completion / Release Guard

Ưu tiên: quyết định trạng thái release, không quyết định bán hàng trực tiếp nhưng chặn Gateway/Production/Release/Go-live.

Evidence có accepted không.

Smoke có PASS không.

P0 gate có PASS không.

Owner sign-off có hợp lệ không.

Completion Gate có PASS không.

Gateway có được xem xét PASS không.

Release Approved có hợp lệ không.

Go-live Approved có hợp lệ không.

Gọi Production Ready khi E2E smoke/security/P0 gate chưa PASS.

Gọi Release Approved khi thiếu owner sign-off.

Gọi Go-live Approved khi Production Ready hoặc Release Approved chưa đạt.

Dùng evidence chưa accepted để PASS.

Dùng tài liệu hoàn thành thay evidence.

ESCALATE nếu security/policy điểm chặn.


## 40. SUPPRESSION CONTROL


### 40.1. Suppression là gì

Suppression là quyết định tạm dừng hoặc chặn một action outbound, sales, CRM, upsell, quote, promotion hoặc customer-facing response do có điều kiện ưu tiên cao hơn.

Suppression không phải lỗi hệ thống.

Suppression là cơ chế bảo vệ khách hàng, bảo vệ doanh nghiệp, bảo vệ chính sách platform, bảo vệ payment, bảo vệ quyền riêng tư và bảo vệ chất lượng vận hành.


### 40.2. Suppression phải có lý do

Mọi suppression P0 phải có reason rõ.

Không được suppress âm thầm.

Không được bỏ qua action mà không audit.

Không được ghi “không gửi” nhưng không biết vì sao.

Suppression reason phải trace được về:

Case ID.

Resolver status.

Policy source.


### 40.3. Các nhóm suppression chính

MASTER-04 khóa các nhóm suppression sau:

Privacy Suppression.

Complaint Suppression.

Health Review Suppression.

Payment Dispute Suppression.

Delivery Issue Suppression.

Counterfeit / Quality Risk Suppression.

Member Dispute Suppression.

Commission Dispute Suppression.

Opt-out Suppression.

Quiet Hours Suppression.

Frequency Cap Suppression.

Gateway Policy Suppression.

Security / Legal Suppression.

Evidence / Release Suppression.


### 40.4. Privacy Suppression

Privacy Suppression kích hoạt khi có privacy case, yêu cầu xóa dữ liệu, yêu cầu không liên hệ, nghi lộ PII hoặc dữ liệu nhạy cảm cần bảo vệ.

Bị suppress:

CRM outbound.

Sales message.

Upsell.

Diamond/affiliate message.

ADS/retargeting nếu thuộc phạm vi dữ liệu cần chặn.

Public reply có thể lặp thông tin cá nhân.

Handoff không đúng policy.

Được phép:

Trả lời an toàn về việc tiếp nhận yêu cầu.

Handoff privacy/security.

Ghi audit.

Dừng các outbound không cần thiết.


### 40.5. Complaint Suppression

Complaint Suppression kích hoạt khi khách có khiếu nại sản phẩm, dịch vụ, hoàn tiền, trải nghiệm, chất lượng, giao hàng hoặc thái độ phục vụ.

CRM bán hàng.

Combo proposal.

Diamond promotion.

Rebuy nudge.

Sales closing.

Public praise/marketing response không phù hợp.

Xin lỗi.

Tiếp nhận.

Xác minh.

Mở complaint case.

Chuyển CSKH/QA/kế toán.

Theo dõi xử lý.


### 40.6. Health Review Suppression

Health Review Suppression kích hoạt khi khách nêu bệnh, thuốc, điều trị, triệu chứng, kiêng cữ hoặc trường hợp sức khỏe nhạy cảm.

Claim điều trị.

Chốt mua bằng lý do bệnh.

Tư vấn y khoa sâu.

CRM bán hàng dựa trên bệnh lý.

Upsell quá mức.

Tư vấn thực dưỡng an toàn.

Nhấn mạnh sản phẩm không phải thuốc.

Hỏi thông tin tối thiểu nếu cần an toàn.

Handoff nếu case nhạy cảm.


### 40.7. Payment Dispute Suppression

Payment Dispute Suppression kích hoạt khi có tranh chấp thanh toán, chuyển khoản chưa đối soát, khách báo đã chuyển nhưng chưa có Accounting Review, giao dịch conflict hoặc nghi sai thông tin thanh toán.

Payment reminder cứng.

Sales/upsell.

Commission payable.

Order success claim nếu chưa đủ điều kiện.

PAID claim.

Nói đang chờ đối soát.

Yêu cầu thông tin cần thiết theo boundary.

Chuyển kế toán.

Ghi accounting review.


### 40.8. Delivery Issue Suppression

Delivery Issue Suppression kích hoạt khi đơn có vấn đề giao hàng, COD, thất lạc, trễ, sai địa chỉ, chưa nhận hàng hoặc khách không hài lòng về vận chuyển.

Gift suggestion.

Review request nếu chưa xử lý xong.

Kiểm tra tracking.

Hỗ trợ giao hàng.

Chuyển CSKH/Shipping.

Cập nhật trạng thái thật từ Shipping Core.

Ghi delivery issue.


### 40.9. Counterfeit / Quality Risk Suppression

Counterfeit / Quality Risk Suppression kích hoạt khi khách nghi hàng giả, hàng nhái, mã QR không xác thực, bao bì bất thường, lô bất thường hoặc chất lượng bất thường.

Sales.

Khẳng định sản phẩm lỗi khi chưa xác minh.

Khẳng định hàng giả công khai khi chưa có kết luận.

Hoàn tiền/đổi trả như lỗi thường nếu chưa xác định hàng chính hãng.

Dùng wording thận trọng.

Yêu cầu giữ sản phẩm.

Yêu cầu ảnh bao bì/QR/mã lô/NSX-HSD/nơi mua/hóa đơn nếu phù hợp.

Chuyển CSKH + pháp lý/chống giả/QA.

Mở counterfeit case.


### 40.10. Member / Commission Dispute Suppression

Suppression kích hoạt khi có tranh chấp hạng thành viên, quyền lợi, doanh số tích lũy, hạ hạng, Diamond bind, hoa hồng, Ads/Diamond conflict hoặc self-purchase policy.

Affiliate encouragement.

CRM nhắc quyền lợi.

Quote áp quyền lợi đang tranh chấp.

Nói đang kiểm tra quyền lợi.

Handoff CSKH/Admin/Finance tùy domain.

Ghi dispute case.

Chỉ nói chính sách chung nếu được phép.


### 40.11. Quiet Hours / Frequency Cap / Opt-out Suppression

CRM và outbound phải suppress khi:

Ngoài khung giờ được phép.

Khách opt-out.

Khách không đồng ý nhận tin.

Trigger bị duplicate.

Job bị retry nhưng không đủ điều kiện gửi lại.

Không được bypass suppression bằng template khác, kênh khác hoặc wording khác.


### 40.12. Evidence / Release Suppression

Các tuyên bố readiness phải suppress khi:

Evidence chưa accepted.

Smoke chưa PASS.

P0 gate chưa PASS.

Security review chưa PASS.

Owner sign-off chưa có.

Gateway Gate chưa PASS.

Production Ready chưa đủ điều kiện.

Không được nói Gateway PASS, Production Ready, Release Approved hoặc Go-live Approved khi gate chưa đủ điều kiện.


## 41. SUPPRESSION AUDIT REQUIREMENT


### 41.1. Suppression phải có audit

Mỗi suppression phải ghi audit.

Audit tối thiểu phải có:

Suppression type.

Trigger/source.

Case ID nếu có.

Customer context nếu có.

Action bị suppress.

Consumer bị ảnh hưởng.

Thời điểm.

Điều kiện mở lại nếu có.


### 41.2. Suppression không được xóa lịch sử

Suppression không xóa CRM job, quote, order draft, conversation hoặc event gốc.

Suppression chỉ ghi rằng action không được tiếp tục trong điều kiện hiện tại.

Lịch sử vẫn phải giữ để audit, CSKH, QA, kế toán, pháp lý, finance, release review và owner review.


### 41.3. Suppression phải được kiểm tra lại khi context thay đổi

Một suppression có thể được gỡ khi:

Complaint đã đóng.

Privacy request đã xử lý.

Payment dispute đã resolved.

Delivery issue đã resolved.

Health review đã đóng.

Counterfeit case đã kết luận.

Member dispute đã resolved.

Commission dispute đã resolved.

Quiet hours đã hết.

Frequency cap đã reset.

Evidence đã accepted.

Owner đã sign-off.

Gỡ suppression phải có audit.


## 42. FALLBACK CONTROL


### 42.1. Fallback là gì

Fallback là phản hồi hoặc hành động thay thế an toàn khi action chính không được phép thực hiện.

Fallback không phải bypass.

Fallback không phải cách đi vòng Guard.

Fallback chỉ giúp hệ thống trả lời trung thực, an toàn và không làm đứt trải nghiệm khách hàng.


### 42.2. Nguyên tắc fallback bắt buộc

MASTER-04 khóa các nguyên tắc fallback sau:

Fallback chỉ dùng khi Guard cho phép.

Fallback phải có reason.

Fallback phải trace về failure/block/suppression gốc.

Fallback không được tạo trạng thái nghiệp vụ mới ngoài phạm vi.

Fallback không được nói quá sự thật runtime.

Fallback không được che lỗi bằng câu trả lời đẹp.

Fallback không được biến missing data thành dữ liệu có thật.

Fallback phải có audit nếu thuộc P0.


### 42.3. Fallback khi thiếu customer context

Khi thiếu customer identity, hệ thống chỉ được:

Tư vấn chung.

Hỏi xác minh tối thiểu trong kênh phù hợp.

Chuyển private/handoff nếu cần.

Không cá nhân hóa quyền lợi.

Không nhắc lịch sử mua.

Không báo member tier.

Không áp Diamond bind.

Không gửi CRM cá nhân hóa.

Không được bịa khách là khách cũ, member hoặc Diamond.


### 42.4. Fallback khi thiếu product activation

Khi thiếu product activation hoặc SKU không sellable, hệ thống chỉ được:

Không chốt bán SKU đó.

Không quote SKU đó.

Không đưa vào order draft.

Gợi ý sản phẩm thay thế đang active/sellable nếu có.

Nếu không có sản phẩm thay thế, trả lời an toàn và không ép bán.

Không được nói sản phẩm còn hàng khi Sellable Guard không PASS.


### 42.5. Fallback khi thiếu QuoteSnapshot

Khi thiếu QuoteSnapshot, hệ thống không được báo giá.

Fallback được phép:

Chuyển sang private nếu đang ở public.

Nói cần xác định đúng sản phẩm/quyền lợi/kênh trước khi báo giá.

Hỏi thông tin tối thiểu để tạo quote nếu policy cho phép.

Không nêu giá cụ thể.

Không nêu discount cụ thể.

Không nêu final payable price.


### 42.6. Fallback khi QuoteSnapshot hết hạn

Khi QuoteSnapshot hết hạn, hệ thống không được dùng lại quote cũ.

Resolve quote mới.

Nói giá cần được cập nhật lại theo thời điểm hiện tại.

Không sửa quote cũ.

Không dùng quote cũ để tạo OrderDraft.

Không tạo tranh chấp bằng cách xác nhận giá cũ đã hết hạn.


### 42.7. Fallback khi handoff Messenger thất bại

Khi Messenger handoff thất bại, hệ thống không được nói đã gửi Messenger.

Trả lời public an toàn theo chính sách.

Không báo giá public.

Không lặp PII.

Không gửi payment instruction.

Không chốt đơn public.

Ghi handoff fail.

Cho phép retry nếu policy cho phép.

Chuyển human review nếu cần.


### 42.8. Fallback khi thiếu payment confirmation

Khi chưa có Payment Core hoặc Accounting Review xác nhận, hệ thống không được nói PAID.

Nói bộ phận kế toán sẽ kiểm tra nếu thuộc quy trình.

Yêu cầu thông tin giao dịch tối thiểu nếu policy cho phép.

Không xác nhận thanh toán thành công.

Không kích hoạt commission payable.

Không xác nhận đơn đã thanh toán.


### 42.9. Fallback khi thiếu shipping data

Khi thiếu Shipping Core data, hệ thống không được bịa ETA/tracking/status.

Nói hiện chưa có cập nhật vận chuyển mới.

Ghi nhận yêu cầu kiểm tra.

Chuyển CSKH/Shipping nếu cần.

Không tự đoán ngày giao.

Không tự nói COD status.

Không hỏi lại địa chỉ nếu order đã có shipping info hợp lệ.


### 42.10. Fallback khi thiếu official contact

Khi thiếu Official Contact Resolver hoặc intent chưa rõ, hệ thống không được tự gửi số điện thoại.

Hỏi rõ mục đích liên hệ nếu cần.

Chuyển human/admin nếu policy cho phép.

Không tự chọn số.

Không cung cấp số cá nhân lãnh đạo.

Không hardcode contact trong template.


### 42.11. Fallback khi thiếu scientific evidence

Khi scientific evidence chưa approved, hệ thống không được khẳng định có công bố khoa học.

Dùng wording public-safe chung.

Nói theo nền thực phẩm thực dưỡng nếu được phép.

Không bịa tên bài báo, DOI, link, số liệu, kết luận.

Không tạo claim điều trị.

Không biến nghiên cứu in vitro thành hiệu quả lâm sàng trên người.


### 42.12. Fallback khi thiếu evidence/release gate

Khi thiếu evidence hoặc gate chưa PASS, hệ thống không được nói Ready/PASS.

Nói cần evidence accepted.

Nói Gateway vẫn bị chặn.

Không nói release approved.

Không nói go-live approved.


## 43. HANDOFF CONTROL


### 43.1. Handoff là gì

Handoff là quyết định chuyển luồng sang kênh, người, bộ phận, core hoặc cơ chế xử lý phù hợp hơn.

Handoff không phải lỗi.

Handoff là cơ chế kiểm soát ranh giới khi automation không được phép hoặc không nên tiếp tục tự xử lý.


### 43.2. Các loại handoff chính

MASTER-04 khóa các loại handoff:

Public -> Messenger/private handoff.

AI -> Human CSKH handoff.

AI/Gateway -> Accounting handoff.

AI/Gateway -> QA/Quality handoff.

AI/Gateway -> Privacy/Security handoff.

AI/Gateway -> Legal/Brand Protection handoff.

AI/Gateway -> Owner/Admin Review handoff.

CRM -> CSKH handoff.

Payment -> Accounting Review handoff.

Evidence -> Owner Sign-off handoff.

Gateway -> Platform/App Review handoff.

IVR -> Order Review handoff nếu confirmation conflict.


### 43.3. Handoff phải có trace

Mọi handoff P0 phải có trace.

Trace tối thiểu:

Source event.

Handoff target.

Handoff status.

Delivery log nếu là channel handoff.

Action bị dừng.

Action được phép sau handoff nếu có.

Không được handoff không trace.


### 43.4. Public -> Messenger handoff

Public -> Messenger handoff bắt buộc với các intent:

Hỏi giá.

Muốn mua.

Cung cấp PII.

Payment.

Health-sensitive.

Diamond/referral.

Order issue.

Privacy.

Counterfeit.

Member/commission dispute.

Nội dung cần tư vấn sâu.

Nguyên tắc:

Public comment không báo giá.

Public comment không chốt đơn.

Public comment không lặp PII.

Public comment không gửi payment instruction.

Public comment không tư vấn health detail dài.

Handoff success mới nói đã gửi Messenger.

Handoff fail không nói đã gửi.

Không kêu khách tự inbox nếu hệ thống có thể chủ động handoff theo policy.


### 43.5. AI -> Human CSKH handoff

AI phải handoff CSKH khi:

Khiếu nại đang mở.

Khách bức xúc.

Cần xử lý đổi/trả/hoàn.

Cần xác minh đơn.

Cần xử lý delivery issue.

Cần xử lý thông tin khách phức tạp.

AI thiếu quyền tiếp tục.

Guard trả HANDOFF hoặc ESCALATE.

AI không được tiếp tục bán hàng khi handoff CSKH đang cần xử lý conflict.


### 43.6. Payment -> Accounting handoff

Payment phải handoff Accounting khi:

Khách báo đã chuyển khoản nhưng chưa đối soát.

Có ảnh giao dịch cần kiểm tra.

Bank transfer signal không match.

Payment reference thiếu/nhầm.

Số tiền không khớp.

Nội dung chuyển khoản không khớp.

Có payment dispute.

Cần xác nhận PAID bằng Accounting Review.

AI/CRM/Gateway không được tự xác nhận PAID trong các trường hợp này.


### 43.7. Complaint / Quality / Counterfeit handoff

Handoff QA/CSKH/Legal/Brand Protection bắt buộc khi:

Khách nghi hàng giả.

QR không xác thực.

Bao bì/mã lô bất thường.

Sản phẩm có dấu hiệu không thuộc phân phối chính thức.

Khiếu nại chất lượng.

Nghi lỗi kỹ thuật.

Có rủi ro pháp lý hoặc truyền thông.

Không được xử lý như đổi trả thông thường nếu chưa xác minh chính hãng.

Không được kết luận công khai hàng giả/lỗi sản xuất khi chưa có kết luận.


### 43.8. Privacy / Security handoff

Handoff Privacy/Security bắt buộc khi:

Khách yêu cầu xóa dữ liệu.

Khách hỏi dữ liệu cá nhân.

Có nguy cơ lộ PII.

Có lộ token/secret.

Có truy cập trái phép.

Có policy platform issue.

Có security điểm chặn.

CRM và outbound không cần thiết phải dừng cho đến khi Privacy/Security Guard cho phép.


### 43.9. Owner / Release handoff

Owner/Release handoff bắt buộc khi:

Evidence cần review.

Smoke cần sign-off.

Completion Gate cần quyết định.

Security/App Review cần phê duyệt.

Gateway Gate cần xem xét.

Production Ready cần owner approval.

Release Approved cần owner sign-off.

Go-live cần phê duyệt cuối.

Không được tự chuyển trạng thái release khi chưa có owner_signoff_id hợp lệ.


## 44. HANDOFF STATUS MODEL


### 44.1. Bộ trạng thái handoff chuẩn

MASTER-04 khóa bộ trạng thái handoff:

REQUIRED

REQUESTED

SENT

DELIVERED

FAILED

ACCEPTED

IN_REVIEW

REJECTED

CANCELLED

EXPIRED


### 44.2. REQUIRED

REQUIRED nghĩa là Guard xác định phải handoff.

Ở trạng thái REQUIRED, Consumer không được tiếp tục action chính nếu action đó bị chặn.


### 44.3. REQUESTED

REQUESTED nghĩa là hệ thống đã yêu cầu handoff nhưng chưa có kết quả gửi/chấp nhận.

Không được nói handoff thành công ở trạng thái REQUESTED.


### 44.4. SENT / DELIVERED

SENT nghĩa là yêu cầu handoff đã được gửi đi.

DELIVERED nghĩa là hệ thống có xác nhận giao thành công theo channel hoặc receiving system.

Với Messenger handoff, chỉ khi delivery log xác nhận success/DELIVERED mới được nói đã gửi Messenger.


### 44.5. FAILED

FAILED nghĩa là handoff không thành công.

Ở trạng thái FAILED:

Không được nói đã gửi Messenger.

Không được xem private flow đã bắt đầu.

Chỉ được fallback an toàn nếu Guard cho phép.

Có thể retry theo policy.

Phải audit reason.


### 44.6. ACCEPTED / IN_REVIEW / RESOLVED

ACCEPTED nghĩa là target nhận xử lý.

IN_REVIEW nghĩa là đang xử lý.

RESOLVED nghĩa là handoff đã được xử lý xong.

Chỉ sau khi resolved và Guard cho phép, các action bị suppress mới có thể được xét mở lại.


### 44.7. REJECTED / CANCELLED / EXPIRED

REJECTED nghĩa là handoff bị từ chối.

CANCELLED nghĩa là handoff bị hủy.

EXPIRED nghĩa là handoff quá hạn.

Các trạng thái này không được xem là xử lý thành công.

Nếu action chính vẫn cần điều kiện handoff, action đó phải tiếp tục BLOCK/SUPPRESS/REVIEW_REQUIRED.


## 45. RUNTIME CONFLICT RESOLUTION


### 45.1. Runtime conflict là gì

Runtime conflict là tình huống nhiều nguồn, nhiều resolver, nhiều guard hoặc nhiều state trả về kết quả không thống nhất tại thời điểm thực thi.

Conflict không được Consumer tự chọn nhánh.

Conflict phải được xử lý theo Guard Priority Matrix và Owner Core.


### 45.2. Các loại runtime conflict chính

MASTER-04 khóa các loại conflict chính:

Source conflict.

Customer identity conflict.

Customer memory conflict.

Member lifecycle conflict.

Diamond/Ads attribution conflict.

Product activation conflict.

Sellable/stock conflict.

Program/price conflict.

Quote conflict.

Order confirmation conflict.

Payment/accounting conflict.

Shipping conflict.

CRM suppression conflict.

Gateway handoff conflict.

Claim/scientific evidence conflict.

Evidence/release gate conflict.


### 45.3. Source conflict

Source conflict xảy ra khi nhiều nguồn trả dữ liệu khác nhau.

Rule xử lý:

Source-of-Truth thắng dữ liệu consumer/cache/template.

Owner Core thắng dữ liệu downstream.

Runtime Resolver phải trả CONFLICT.

Consumer không được tự chọn nguồn.

Guard phải BLOCK hoặc REVIEW_REQUIRED.

Audit phải ghi conflict.


### 45.4. Customer identity conflict

Customer identity conflict xảy ra khi session, phone/email, channel identity, member record, Diamond link hoặc prior order không khớp.

Không cá nhân hóa cho đến khi resolved.

Không báo member benefit.

Không áp Diamond attribution.

Không nhắc lịch sử mua cụ thể.

Handoff/xác minh nếu cần.

Audit conflict.


### 45.5. Customer memory conflict

Customer memory conflict xảy ra khi lịch sử mua, người nhận, sản phẩm đã dùng, trạng thái đơn hoặc open case không thống nhất.

Không nhắc lịch sử cụ thể nếu memory chưa chắc.

Không hỏi lại sai sản phẩm đã mua.

Không gợi ý mua lại dựa trên dữ liệu conflict.

Có thể dùng tư vấn chung.

Cần resolver lại hoặc review.


### 45.6. Member lifecycle conflict

Member lifecycle conflict xảy ra khi hạng, doanh số, quyền lợi, grace, downgrade hoặc dispute không thống nhất.

Không nói hạng cụ thể.

Không nói số còn thiếu.

Không áp quyền lợi member vào quote.

Không gửi member upgrade/downgrade message.

Chuyển review nếu cần.

Member Dispute Guard thắng CRM/sales.


### 45.7. Diamond / Ads attribution conflict

Conflict xảy ra khi một đơn hoặc customer journey có cả Ads attribution và Diamond referral signal hoặc có nhiều Diamond link/bind.

Không tự chọn bên hưởng attribution.

Không tạo commission event nếu chưa resolve.

Không nói commission payable.

Không áp Diamond path nếu thiếu bind hợp lệ.

Conflict phải vào review/audit.

Self-purchase policy phải được kiểm tra.


### 45.8. Product activation / sellable conflict

Conflict xảy ra khi một nguồn nói sản phẩm active nhưng nguồn khác báo hết hàng, hold, recall, sale lock, channel block hoặc price inactive.

Sellable Guard thắng sales/marketing.

Không quote.

Không chốt đơn.

Không lên live board.

Không MC AI script.

Có thể fallback sang sản phẩm thay thế đang active.


### 45.9. Program / price conflict

Conflict xảy ra khi chương trình, giá, quyền lợi member, quyền lợi Diamond hoặc quote result không thống nhất.

Không báo giá nếu thiếu QuoteSnapshot hợp lệ.

Không nói giá chương trình nếu Program Resolver conflict.

Không áp quyền lợi member/Diamond nếu context conflict.

Phải resolve quote mới hoặc review.


### 45.10. Quote conflict

Quote conflict xảy ra khi QuoteSnapshot hết hạn, quote cart không khớp, sản phẩm trong quote không còn sellable, quyền lợi thay đổi hoặc giá runtime đổi.

Quote cũ không được sửa.

Nếu quote hết hạn, tạo quote mới.

Không lập OrderDraft từ quote không hợp lệ.

Không tạo order từ quote conflict.

Customer-facing wording phải trung thực và không đổ lỗi.


### 45.11. Order confirmation conflict

Conflict xảy ra khi khách xác nhận nhiều lần, sửa thông tin sau khi xác nhận, draft hết hạn, draft bị superseded hoặc IVR confirmation không khớp.

Idempotency chặn tạo nhiều order.

Draft mới hoặc revision phải có trace.

Không tạo order nếu confirmation không khớp draft hiện hành.

IVR chỉ xác nhận, không đổi order state ngoài phạm vi.


### 45.12. Payment / accounting conflict

Conflict xảy ra khi khách nói đã chuyển, ảnh giao dịch có nhưng Accounting Review chưa xác nhận, số tiền không khớp, payment reference sai, bank signal conflict hoặc payment dispute mở.

Payment/Accounting Core thắng lời khách.

Không PAID nếu chưa xác nhận.

Không commission payable.

Không gửi payment reminder cứng khi dispute mở.

Chuyển Accounting Review.


### 45.13. Shipping conflict

Conflict xảy ra khi tracking, ETA, COD, delivery status hoặc customer claim không khớp Shipping Core.

Không bịa ETA.

Không nói Delivered nếu Shipping Core không xác nhận.

Delivery issue mở thì suppress CRM bán hàng.


### 45.14. CRM suppression conflict

Conflict xảy ra khi CRM trigger hợp lệ nhưng suppression cũng active.

Suppression thắng trigger.

Không gửi CRM bán hàng.

Nếu được phép, chỉ gửi chăm sóc không bán hàng theo Guard.

Không retry gửi khi suppression còn hiệu lực.

Audit suppression reason.


### 45.15. Gateway handoff conflict

Conflict xảy ra khi handoff requested nhưng delivery log fail, hoặc có signal success/fail mâu thuẫn.

Delivery log chính thức thắng câu trả lời AI.

Nếu không success, không nói đã gửi Messenger.

Nếu conflict, không tiếp tục private-flow assumption.

Fallback public an toàn nếu Guard cho phép.


### 45.16. Claim / scientific evidence conflict

Conflict xảy ra khi nội dung claim, evidence, public wording hoặc health context không thống nhất.

Claim Whitelist thắng content tự do.

Scientific Evidence Core thắng diễn giải marketing.

Health Boundary Guard thắng claim bán hàng.

Không thuốc hóa.

Không bịa link/kết luận.

Dùng fallback public-safe nếu thiếu approved evidence.


### 45.17. Evidence / release gate conflict

Conflict xảy ra khi Completion Report, evidence package, smoke status, owner sign-off hoặc release status không thống nhất.

Evidence Registry thắng checklist text.

Evidence accepted mới được tính PASS.

Smoke PASS phải trace evidence.

Owner sign-off phải trace completion gate.


## 46. CONFLICT DECISION MODEL


### 46.1. Bộ quyết định khi có conflict

Khi Runtime Resolver trả CONFLICT, Guard chỉ được trả một trong các quyết định:

Không được PASS nếu conflict chưa được xử lý, trừ khi conflict đó nằm ngoài action đang xét và Guard chứng minh không ảnh hưởng action.


### 46.2. Conflict không được Consumer tự xử lý

Consumer không được tự chọn dữ liệu “có vẻ đúng hơn”.

Consumer không được lấy dữ liệu thuận lợi cho bán hàng.

Consumer không được dùng dữ liệu mới hơn nếu chưa biết Source-of-Truth.

Consumer không được dùng dữ liệu cũ nếu runtime đã báo STALE hoặc CONFLICT.

Consumer không được tiếp tục action P0 khi conflict còn mở.


### 46.3. Conflict phải có audit

Audit conflict tối thiểu gồm:

Conflict type.

Sources involved.

Action bị ảnh hưởng.

Priority rule được áp dụng.

Handoff/review target nếu có.

Trạng thái xử lý.


## 47. ACTION AUTHORIZATION CONTROL


### 47.1. Action chỉ được thực hiện sau Guard PASS

Mọi action P0 chỉ được thực hiện khi Guard trả PASS cho đúng action đó.

Một Guard PASS không áp dụng cho mọi action.

ProductRecommendationGuard PASS không đồng nghĩa QuoteGuard PASS.

QuoteGuard PASS không đồng nghĩa OrderGuard PASS.

OrderGuard PASS không đồng nghĩa PaymentGuard PASS.

PaymentInstructionGuard PASS không đồng nghĩa PAID.

GatewayHandoffGuard PASS không đồng nghĩa Gateway Gate PASS.

EvidenceReview PASS không đồng nghĩa Release Approved.


### 47.2. Action bị block không được đổi tên để chạy tiếp

Consumer không được đổi tên action để né Guard.

Không được gọi upsell là “chăm sóc” nếu complaint đang mở.

Không được gọi payment reminder là “nhắc nhẹ” nếu payment dispute đang mở.

Không được gọi báo giá là “tham khảo” nếu thiếu QuoteSnapshot.

Không được gọi chốt đơn là “giữ hàng” nếu thiếu CustomerConfirmation.

Không được gọi claim điều trị là “chia sẻ kinh nghiệm” nếu Health Guard block.

Không được gọi Gateway PASS là “sẵn sàng chạy” nếu Completion Gate chưa PASS.


### 47.3. Action customer-facing phải có render boundary

Mọi action customer-facing phải có render boundary:

Public hay private.

Có được nói giá không.

Có được nói payment không.

Có được nói health detail không.

Có được nói member benefit không.

Có được nói Diamond/commission không.

Có được nói shipping status không.

Có được nói official contact không.

Có được nói claim khoa học không.

Có được chốt đơn không.

Render boundary phải đến từ Guard, không từ Consumer.


## 48. PUBLIC / PRIVATE BOUNDARY CONTROL


### 48.1. Public boundary

Public comment/live chỉ được dùng cho nội dung an toàn, ngắn, không PII, không payment instruction, không quote final price, không chốt đơn, không health detail dài.

Public boundary bắt buộc với:

Live comment.

Fanpage comment.

Public post comment.

Public ad comment.

Public reply.


### 48.2. Private boundary

Private boundary áp dụng cho:

Messenger.

Web chat private.

Authenticated account area.

Private order form.

CSKH private channel.

Accounting review channel.

Các nội dung cần private:

Giá cá nhân hóa.

Quote.

Payment instruction.

Địa chỉ/số điện thoại.

Member benefit.

Health-sensitive details.

Complaint details.

Counterfeit details.


### 48.3. Public không được giả private

Không được trả lời public theo kiểu làm lộ private data.

Không được nói:

Giá cuối cá nhân hóa public.

Quyền lợi member cụ thể public.

Payment reference public.

Thông tin đơn hàng public.

Số điện thoại/địa chỉ khách public.

Nội dung bệnh lý public.

Nội dung khiếu nại chi tiết public.

Nội dung tranh chấp hoa hồng public.


## 49. RETRY CONTROL


### 49.1. Retry phải tôn trọng Guard

Retry không được dùng để bypass Guard.

Nếu action bị BLOCK do thiếu điều kiện P0, retry không được tiếp tục cho đến khi điều kiện thay đổi.

Nếu action bị SUPPRESS do complaint/privacy/payment/delivery issue, retry không được gửi lại cho đến khi suppression được gỡ.

Nếu evidence bị REJECTED, retry không được chuyển PASS nếu không có evidence mới hoặc review mới hợp lệ.


### 49.2. Retry phải tôn trọng idempotency

Retry không được tạo duplicate:

OrderCode.

PaymentReference.

BankTransferQueue.

AccountingReview.

ShippingTrackingEvent.

CRMMessageJob.

MessengerHandoffEvent.

CommissionEvent.

EvidenceID.

OwnerSignoffID.

Retry phải dùng idempotency và audit.


### 49.3. Retry phải tôn trọng dedup

Retry không được gửi trùng customer-facing message.

Áp dụng cho:

Public comment reply.

Messenger handoff.

CRM message.

Shipping update.

IVR confirmation.

Complaint response.

Evidence upload.

Duplicate event phải NO_ACTION hoặc return previous result theo policy, không tạo action mới.


## 50. FALLBACK / HANDOFF / SUPPRESSION RELATIONSHIP


### 50.1. Suppression khác fallback

Suppression là dừng action do điều kiện ưu tiên.

Fallback là phản hồi thay thế an toàn khi action chính không được phép.

Một action có thể bị suppress và vẫn có fallback chăm sóc không bán hàng nếu Guard cho phép.

Complaint mở -> suppress CRM bán hàng -> fallback CSKH hỏi thông tin xử lý khiếu nại.


### 50.2. Handoff khác fallback

Handoff là chuyển luồng sang kênh/người/core phù hợp.

Fallback là phản hồi an toàn tại chỗ.

Một flow có thể handoff và fallback cùng lúc nếu handoff failed hoặc đang chờ xử lý.

Public comment hỏi giá -> handoff Messenger. Nếu handoff fail -> fallback public an toàn, không báo giá.


### 50.3. Review khác handoff

Review là trạng thái cần thẩm quyền xem xét trước khi quyết định.

Handoff là hành động chuyển đến nơi có thẩm quyền.

Một handoff có thể dẫn đến review, nhưng handoff sent không có nghĩa review pass.

Accounting handoff đã gửi không có nghĩa PAID.

Owner review đang mở không có nghĩa owner sign-off PASS.

Evidence submitted không có nghĩa evidence accepted.


## 51. RUNTIME DECISION RECORD REQUIREMENT


### 51.1. Runtime Decision Record bắt buộc với action P0

Mỗi action P0 phải có Runtime Decision Record.

Runtime Decision Record không phải code object.

Đây là chuẩn governance để implementation sau ghi nhận đủ dữ kiện quyết định.


### 51.2. Nội dung tối thiểu của Runtime Decision Record

Runtime Decision Record tối thiểu phải có:

Event/intent.

Consumer request.

Resolver chain.

Priority applied.

Fallback reason nếu có.

Handoff target/status nếu có.

Conflict type nếu có.

Idempotency key nếu có.

Dedup key nếu có.

Final allowed action.

Final bị chặn action.


### 51.3. Runtime Decision Record không được sửa lịch sử

Runtime Decision Record phải immutable sau khi quyết định được ghi.

Nếu cần thay đổi, phải tạo decision record mới hoặc correction event mới.

Không được sửa record cũ để làm đẹp lịch sử.


## 52. DOMAIN-SPECIFIC PRIORITY EXAMPLES


### 52.1. Khách hỏi giá trên live

Runtime condition:

Channel = public live comment.

Intent = hỏi giá.

Product context có thể có hoặc chưa rõ.

Guard result:

PublicQuoteGuard = BLOCK.

MessengerHandoffGuard = HANDOFF nếu policy cho phép.

QuoteGuard chỉ chạy trong private khi đủ context.

Final decision:

Nếu handoff success mới nói đã gửi.

Nếu handoff fail dùng fallback public an toàn.


### 52.2. Khách cũ quay lại chào

Customer identity resolved.

Customer memory snapshot có lần mua gần nhất.

Không có complaint mở.

SalesGuard chưa chạy cho đến khi có nhu cầu.

Hỏi thăm trải nghiệm trước.

Không nhảy ngay vào quote.

Nếu khách có nhu cầu mới thì mới chạy recommendation/quote.


### 52.3. Khách báo đã chuyển khoản

Khách nói đã chuyển.

Có thể có ảnh giao dịch.

Accounting Review chưa xác nhận.

PaymentStatusGuard = REVIEW_REQUIRED.

PaidClaimGuard = BLOCK.

AccountingHandoffGuard = HANDOFF.

Không nói PAID.


### 52.4. CRM đến lịch nhưng khách có complaint mở

CRM trigger hợp lệ.

Complaint case open.

ComplaintGuard = SUPPRESS.

SalesCRMGuard = SUPPRESS.

Có thể gửi chăm sóc xử lý complaint nếu policy cho phép.

Audit suppression.


### 52.5. SKU nằm trong live board nhưng bị hold

Daily live board có SKU.

ProductActivationResolver báo hold.

LiveBoardGuard = PARTIAL/PASS về board.

SellableGuard = BLOCK.

Không bán SKU.

Không quote SKU.

Không MC AI đẩy SKU.

Fallback sang SKU thay thế active/sellable nếu có.


### 52.6. Evidence đã upload nhưng chưa accepted

Evidence submitted.

Evidence review chưa accepted.

Completion gate đang xét.

EvidenceStatusGuard = REVIEW_REQUIRED.

CompletionGateGuard = BLOCK.

Không PASS.

Gateway vẫn bị chặn.


## 53. FINAL PRIORITY LOCK

MASTER-04 khóa nguyên tắc cuối cùng của Guard Priority:

Safety thắng sales.

Privacy thắng outbound.

Complaint thắng CRM.

Health boundary thắng claim bán hàng.

ProductActivation/Sellable thắng marketing.

QuoteSnapshot thắng câu báo giá.

CustomerConfirmation thắng ý định mua.

OrderCode thắng câu “đã đặt”.

DiamondBind thắng suy luận affiliate.

OfficialContactResolver thắng số điện thoại hardcode.

Evidence accepted thắng checklist chữ.

Completion Gate thắng Completion Report text.

OwnerSignoff thắng owner review chưa hoàn tất.

Gateway Gate thắng mong muốn mở kênh.

Production Ready chỉ có sau implementation + evidence + smoke + security + owner sign-off + gate PASS.


## 54. KẾT LUẬN PHẦN 3/4


## PHẦN 3/4 của MASTER-04 đã khóa Guard Priority Matrix, Suppression Control, Fallback Control, Handoff Control và Runtime Conflict Resolution cho hệ thống Ginsengfood.

Mọi action P0 phải qua Resolver và Guard.

Guard priority cao hơn Consumer Action.

Suppression phải có reason, case và audit.

Handoff phải có trace, delivery status và audit.

Runtime conflict không được Consumer tự xử lý.

Public/private boundary phải được Guard quyết định.

Retry phải tôn trọng idempotency, dedup và suppression.

Quote không được báo nếu thiếu QuoteSnapshot.

Order không được tạo nếu thiếu CustomerConfirmation.

Payment không được PAID nếu thiếu Payment Core / Accounting Review.

Shipping không được bịa ETA/tracking/status.

CRM không được gửi nếu thiếu trigger hoặc đang bị suppression.

Product không được bán nếu thiếu product activation/sellable.

Official contact không được gửi nếu thiếu OfficialContactResolver.

Diamond commission không được tạo nếu thiếu bind/order/payment eligibility.

Evidence chưa accepted thì không PASS.

MASTER-04 tiếp tục ở PHẦN 4/4 với Final Runtime Done Gate, Smoke Mapping, Release Control và MASTER-04 Final Conclusion.


## PHẦN 4/4 — FINAL RUNTIME DONE GATE / SMOKE MAPPING / RELEASE CONTROL / MASTER-04 FINAL CONCLUSION


## PHẦN 4/4 hoàn tất MASTER-04 bằng lớp Done Gate, Smoke Mapping và Release Control cho Runtime Resolver / Guard. Nội dung tiếp tục bám các nguyên tắc đã khóa: Source-of-Truth quyết định dữ liệu, Owner Core quyết định nghiệp vụ, Runtime Resolver quyết định tại thời điểm thực thi, Consumer chỉ tiêu thụ trong boundary, Guard kiểm soát điều kiện an toàn, Evidence chứng minh dependency hoạt động và Release Gate quyết định được phép mở hay chưa.


## 55. MỤC TIÊU CỦA FINAL RUNTIME DONE GATE

Final Runtime Done Gate của MASTER-04 quy định điều kiện tối thiểu để lớp Runtime Resolution và Guard Control được xem là hoàn chỉnh ở tầng governance.

Final Runtime Done Gate không xác nhận hệ thống đã triển khai xong.

Final Runtime Done Gate không xác nhận code đã chạy.

Final Runtime Done Gate không xác nhận database, API, DTO, UI hoặc worker đã hoàn tất.

Final Runtime Done Gate không xác nhận Gateway PASS.

Final Runtime Done Gate không xác nhận Production Ready.

Final Runtime Done Gate chỉ xác nhận rằng chuẩn governance cho Runtime Resolver, Guard, Priority Matrix, Suppression, Fallback, Handoff, Conflict Resolution và Release Control đã đủ rõ để các pack implementation phía sau không tự suy luận rule nghiệp vụ, không hardcode runtime data, không bypass Source-of-Truth và không mở action P0 khi thiếu điều kiện.


## 56. PHẠM VI CỦA FINAL RUNTIME DONE GATE

Final Runtime Done Gate áp dụng cho toàn bộ 16 business domain đã được khóa:

Governance / Source-of-Truth.

Product Knowledge / Claim / Brand / Scientific Proof.

Customer Identity / Customer Memory.

Segment Recommendation / Product Recommendation.

CRM 12-Month / Message Trigger / Suppression.

Member Lifecycle / Rights / Downgrade / Dispute.

Diamond / Affiliate / Entrepreneurship / Distributor Boundary.

Pricing / Program / 24/7 / Golden Hour / QuoteSnapshot.

Program / Sellable / Product Activation / Production Signal.

Payment / Bank Transfer / Accounting Review.

Shipping / Tracking / ETA / COD.

Order / OrderDraft / CustomerConfirmation / IVR.

Gateway / Public Comment / Messenger Handoff / Moderation.

Official Contact / Phone Number Registry.

Health Boundary / Complaint / Priority Conflict.

Evidence / Completion Report / Gateway Gate / Security.

Final Runtime Done Gate kiểm soát:

Runtime Resolver Registry.

Required Inputs.

Required Outputs.

Guard Decision.

Runtime Decision Record.

Audit Requirement.

Evidence Requirement.

Release Control.


## 57. NGUYÊN TẮC FINAL RUNTIME DONE GATE


### 57.1. Runtime Done Gate không dựa trên tài liệu đã viết xong

MASTER-04 quy định rõ: tài liệu hoàn thành không đồng nghĩa runtime đã hoạt động.

Vì vậy, Final Runtime Done Gate chỉ xác nhận chuẩn Runtime Resolver / Guard đã đủ để làm nền cho implementation, smoke, evidence và release gate sau này.


### 57.2. Runtime Done Gate phải kiểm tra đủ Resolver và Guard

Một domain runtime chỉ được xem là đạt governance khi có đủ:

Runtime Resolver chính.

bị chặn-if-Missing Rule.

Suppression rule nếu có.

Fallback rule nếu có.

Handoff rule nếu có.

Conflict resolution rule nếu có.

Trace ID theo MASTER-03.

Audit requirement.

Evidence requirement nếu phục vụ gate.

Thiếu một trong các lớp trên thì domain chưa đạt Runtime Done Gate.


### 57.3. Runtime Done Gate phải chặn mọi Consumer tự suy luận

Không Consumer nào được tự suy luận rule kinh doanh.

Consumer chỉ được render, thực thi hoặc hiển thị theo kết quả đã được Runtime Resolver và Guard cho phép.

Nếu Consumer tự tính giá, tự xác định member tier, tự xác nhận Diamond, tự xác nhận PAID, tự bịa shipping status, tự gửi số điện thoại, tự nói Gateway PASS hoặc tự mở release, domain tương ứng phải bị chặn.


### 57.4. Runtime Done Gate phải tách rõ “resolved” và “allowed”

Resolver trả RESOLVED không đồng nghĩa action được phép chạy.

Resolver chỉ xác định dữ liệu đã có và có nguồn.

Guard mới là lớp quyết định action được phép hay không.

PricingResolver RESOLVED nhưng QuoteGuard vẫn có thể BLOCK nếu đang ở public comment.

CustomerIdentityResolver RESOLVED nhưng PrivacyGuard có thể SUPPRESS outbound.

ProductActivationResolver RESOLVED nhưng SellableGuard có thể BLOCK nếu stock = 0.

PaymentReferenceResolver RESOLVED nhưng PaymentStatusGuard vẫn REVIEW_REQUIRED nếu chưa Accounting Review.

EvidenceResolver RESOLVED nhưng ReleaseGateGuard vẫn BLOCK nếu evidence chưa accepted.


### 57.5. Runtime Done Gate phải chặn mọi action P0 thiếu Runtime Decision Record

Các action P0 phải có Runtime Decision Record.

Không có Runtime Decision Record thì không đủ audit, không đủ trace và không đủ evidence để đi tiếp.

Các action bắt buộc có Runtime Decision Record gồm:

Public comment xử lý theo Gateway.

Product recommendation có hướng bán.

Health-sensitive response.

Complaint handling.

Counterfeit handling.

Commission event.

Evidence review.

Owner sign-off.

Release approval.


## 58. FINAL RUNTIME DONE GATE CHECKLIST


### 58.1. Gate 01 — Source / Owner / Resolver Gate

Điều kiện PASS:

Mỗi runtime decision có Source-of-Truth rõ.

Mỗi business domain có Owner Core rõ.

Mỗi resolver thuộc đúng domain.

Resolver không lấy dữ liệu ngoài Source-of-Truth.

Consumer không lấy dữ liệu bypass resolver.

Resolver result có source reference, domain reference và owner reference.

Trạng thái bị chặn nếu:

Resolver không biết nguồn dữ liệu.

Consumer dùng dữ liệu cache/template/hardcode thay Source-of-Truth.

Resolver ôm quyền của domain khác.

Owner Core không rõ.

Source conflict chưa review.


### 58.2. Gate 02 — Required Input Gate

Resolver có Required Inputs rõ.

Input bắt buộc được kiểm tra trước khi resolve.

Input thiếu phải trả MISSING, PARTIAL, STALE, CONFLICT, bị chặn hoặc REVIEW_REQUIRED.

Resolver không được trả RESOLVED nếu thiếu input P0.

Input nhạy cảm phải theo đúng privacy boundary.

Thiếu customer identity nhưng vẫn cá nhân hóa.

Thiếu product activation nhưng vẫn quote.

Thiếu payment reference nhưng vẫn hướng dẫn chuyển khoản.

Thiếu shipping event nhưng vẫn nói tracking.

Thiếu evidence nhưng vẫn xét PASS.

Thiếu owner sign-off nhưng vẫn release.


### 58.3. Gate 03 — Required Output Gate

Resolver trả output đủ context, không chỉ trả dữ liệu thô.

Output có resolution status.

Output có required canonical ID.

Output có guard precondition.

Output có validity/expiry nếu cần.

Output có block reason nếu không đủ điều kiện.

Output có correlation/audit nếu thuộc action P0.

PricingResolver chỉ trả con số giá mà không có QuoteSnapshot.

ShippingResolver chỉ trả text trạng thái mà không có shipping event.

CRMResolver chỉ trả nội dung tin nhắn mà không có trigger/job/suppression/dedup.

PaymentResolver chỉ trả số tài khoản mà không có PaymentReference/Bank Registry.

EvidenceResolver chỉ trả “đã test” mà không có evidence ID/smoke/gate.


### 58.4. Gate 04 — Guard Decision Gate

Mỗi action P0 có Guard Decision.

Guard decision thuộc bộ chuẩn: PASS, BLOCK, SUPPRESS, FALLBACK, HANDOFF, REVIEW_REQUIRED, ESCALATE, NO_ACTION.

Guard có reason rõ.

Guard có allowed action và bị chặn action rõ.

Guard có audit.

Guard không tạo business data thay Owner Core.

Action P0 chạy không qua Guard.

Guard trả kết quả mơ hồ.

Guard không ghi reason.

Guard bypass Owner Core.

Guard sửa dữ liệu lịch sử.

Guard tự chuyển PASS khi chưa đủ evidence.


### 58.5. Gate 05 — Priority Matrix Gate

Guard Priority Matrix được áp dụng tại thời điểm thực thi.

Priority cao thắng priority thấp.

Security/privacy/legal thắng sales.

Complaint/health/counterfeit thắng CRM.

Product Activation/Sellable thắng marketing.

CRM vẫn gửi khi complaint mở.

AI vẫn báo giá khi thiếu QuoteSnapshot.

Gateway vẫn báo giá public.

Payment vẫn PAID khi chưa Accounting Review.

Shipping vẫn bịa ETA.

MC AI vẫn bán SKU bị hold.


### 58.6. Gate 06 — Suppression Gate

Suppression có reason.

Suppression trace được case/source.

Suppression có audit.

Suppression chặn đúng action.

Suppression có điều kiện mở lại.

Suppression không xóa lịch sử.

Suppress âm thầm không audit.

Complaint mở nhưng CRM bán hàng vẫn gửi.

Privacy case mở nhưng outbound vẫn chạy.

Payment dispute mở nhưng vẫn nhắc thanh toán cứng.

Delivery issue mở nhưng vẫn upsell.

Evidence thiếu nhưng vẫn Gateway PASS.


### 58.7. Gate 07 — Fallback Gate

Fallback chỉ chạy khi Guard cho phép.

Fallback có reason.

Fallback không nâng trạng thái nghiệp vụ.

Fallback không bịa dữ liệu.

Fallback không che lỗi bằng câu trả lời đẹp.

Fallback trace được failure/block gốc.

Fallback có audit nếu thuộc P0.

Không có ETA nhưng tự nói ngày giao.

Chưa PAID nhưng nói đã thanh toán.

Handoff fail nhưng nói đã gửi Messenger.

Thiếu evidence nhưng nói đã đạt.

Thiếu official contact nhưng tự gửi số.

Thiếu QuoteSnapshot nhưng báo giá “tham khảo”.


### 58.8. Gate 08 — Handoff Gate

Handoff có reason.

Handoff có target.

Handoff có status.

Handoff có delivery log nếu là channel handoff.

Handoff có audit.

Handoff không được xem là resolved nếu chưa có trạng thái xử lý.

Handoff success/fail không mâu thuẫn.

Public -> Messenger handoff không có delivery log.

Handoff fail nhưng vẫn nói đã gửi.

Accounting handoff sent nhưng đã coi là PAID.

Owner review chưa đủ điều kiện nhưng coi như sign-off.

Evidence submitted nhưng coi là accepted.

Complaint handoff nhưng sales vẫn tiếp tục.


### 58.9. Gate 09 — Runtime Conflict Gate

Resolver trả CONFLICT khi dữ liệu mâu thuẫn.

Conflict không được Consumer tự xử lý.

Guard dùng Priority Matrix để quyết định.

Conflict có audit.

Conflict có review/handoff nếu cần.

Action P0 không PASS khi conflict chưa xử lý.

Consumer tự chọn nguồn dữ liệu thuận lợi.

AI tự chọn giá thấp/cao khi price conflict.

CRM tự gửi khi suppression conflict.

Gateway tự nói handoff success khi delivery log conflict.

Payment tự PAID khi accounting conflict.

Evidence tự PASS khi release conflict.


### 58.10. Gate 10 — Release Runtime Gate

Evidence accepted.

Smoke PASS.

P0 gate PASS.

Owner sign-off PASS.

Completion Gate PASS.

No open điểm chặn.

Audit đầy đủ.

Release decision có ID và trace.

Missing smoke.

Missing owner sign-off.

Security điểm chặn mở.

Gateway policy điểm chặn mở.

P0 dependency missing.


## 59. MASTER-04 SMOKE MAPPING PRINCIPLES


### 59.1. Smoke Mapping của MASTER-04 là governance smoke

Smoke Mapping trong MASTER-04 không phải test implementation chi tiết.

Smoke Mapping trong MASTER-04 là bản đồ kiểm tra tối thiểu để chứng minh Runtime Resolver và Guard có thể được áp dụng đúng trong các pack sau.

Các pack implementation phải chuyển các smoke này thành test case, E2E test, evidence package và release gate cụ thể.


### 59.2. Smoke ID Naming Principle

MASTER-04 quy định nhóm smoke cho Runtime Resolver / Guard dùng tiền tố:

M04-RT-SMK-xxx

Trong đó:

M04 là MASTER-04.

RT là Runtime Resolution / Guard Control.

SMK là Smoke Mapping.

xxx là số thứ tự smoke.

Smoke ID không thay thế evidence ID.

Mỗi smoke phải có evidence riêng hoặc evidence mapping rõ.


### 59.3. Smoke Mapping phải bao phủ 16 business domain

MASTER-04 Smoke Mapping bắt buộc bao phủ toàn bộ 16 business domain.

Không domain P0 nào được thiếu resolver smoke.

Không domain P0 nào được thiếu guard smoke.

Không domain P0 nào được thiếu bị chặn-if-missing smoke.

Không domain P0 nào được thiếu evidence mapping khi thuộc release gate.


## 60. MASTER-04 SMOKE MATRIX


### 60.1. M04-RT-SMK-001 — Source / Owner / Resolver Smoke

Mục tiêu:

Kiểm tra runtime decision luôn có Source-of-Truth, Owner Core, Resolver và Consumer Boundary.

Required Resolver:

SourceOfTruthResolver.

DependencyResolver.

OwnerCoreResolver.

ConsumerBoundaryResolver.

PASS khi:

Source được xác định.

Owner Core được xác định.

Consumer chỉ được dùng trong boundary.

Missing source bị BLOCK.

Missing owner bị BLOCK.

Audit ghi decision.

bị chặn nếu:

Consumer tự dùng dữ liệu không source.

Resolver không biết owner.

Dependency missing nhưng action vẫn chạy.


### 60.2. M04-RT-SMK-002 — Product Knowledge / Claim Guard Smoke

Kiểm tra claim sản phẩm, scientific proof và public-safe wording không bị Consumer tự bịa.

ProductKnowledgeResolver.

ClaimWhitelistResolver.

ScientificEvidenceResolver.

PublicSafeWordingResolver.

Claim public trace về source.

Scientific proof chỉ dùng khi evidence approved.

Health-sensitive claim được chuyển Health Boundary Guard.

Disallowed claim bị BLOCK.

Fallback public-safe hoạt động.

AI/CRM/Gateway dùng claim chưa approved.

Tự bịa link khoa học.

Tự diễn giải thành claim điều trị.

Dùng evidence chưa approved.


### 60.3. M04-RT-SMK-003 — Customer Context First Smoke

Kiểm tra hệ thống không cá nhân hóa nếu chưa resolve customer identity.

CustomerIdentityResolver.

CustomerMemoryResolver.

OpenCaseResolver.

PrivacyContextResolver.

Khách mới/cũ/member/Diamond/buyer từ link Diamond/open case được resolve.

Có customer_identity_resolution_id trước cá nhân hóa.

Có customer_memory_snapshot_id trước khi nhắc lịch sử mua.

Privacy/open case chặn đúng outbound.

Missing identity chuyển fallback/handoff.

AI tự nói khách là member.

AI nhắc lịch sử mua không có memory snapshot.

CRM cá nhân hóa khi thiếu identity.

Privacy case mở nhưng vẫn outbound.


### 60.4. M04-RT-SMK-004 — Care Before Next Sale Smoke

Kiểm tra khách cũ quay lại được hỏi thăm trải nghiệm trước khi bán tiếp.

PriorityConflictResolver.

Nếu memory có dữ liệu, AI hỏi trải nghiệm sản phẩm gần nhất.

Nếu thiếu memory, AI không bịa lịch sử mua.

Nếu complaint mở, sales bị suppress.

Nếu khách có nhu cầu mua mới, recommendation/quote mới chạy sau bước care.

Khách cũ vừa chào đã bị chốt đơn.

AI bịa sản phẩm đã mua.

Complaint mở nhưng vẫn upsell.


### 60.5. M04-RT-SMK-005 — CRM Trigger / Suppression Smoke

Kiểm tra CRM 12 tháng chỉ gửi khi đủ trigger, identity, suppression check, dedup và idempotency.

CRMTriggerResolver.

CRMMessageJobResolver.

CRMSuppressionResolver.

FrequencyCapResolver.

QuietHoursResolver.

OptOutResolver.

CRM có message_trigger_id.

CRM có crm_message_job_id.

Suppression chạy trước outbound.

Quiet hours/frequency cap/opt-out hoạt động.

Complaint/payment/delivery/health/privacy conflict chặn sales CRM.

Message text đã gửi immutable.

CRM gửi thiếu trigger.

CRM gửi trùng.

CRM gửi khi opt-out.

CRM bán hàng khi case ưu tiên mở.


### 60.6. M04-RT-SMK-006 — Member Lifecycle / Rights Smoke

Kiểm tra hạng, quyền lợi, duy trì, ân hạng, hạ hạng và dispute member không bị AI/CRM tự tính.

MemberTierResolver.

MemberRightsResolver.

MemberLifecycleResolver.

MemberGraceResolver.

MemberDisputeResolver.

Member benefit chỉ dùng khi có member_lifecycle_event_id.

AI không tự tính doanh số còn thiếu.

Grace/downgrade được xử lý an toàn.

Member dispute suppress message nâng hạng/upsell.

Quote quyền lợi member trace được resolver.

AI tự nói hạng hoặc quyền lợi.

Quote áp member benefit khi thiếu resolver.

Member dispute mở nhưng CRM vẫn gửi nâng hạng.


### 60.7. M04-RT-SMK-007 — Diamond / Affiliate / Distributor Boundary Smoke

Kiểm tra Diamond/Affiliate không bị nhầm với đại lý/nhà phân phối/mua sỉ.

DiamondEligibilityResolver.

DiamondReferralLinkResolver.

DiamondBindResolver.

CommissionEligibilityResolver.

AdsDiamondConflictResolver.

DistributorBoundaryResolver.

Commission chỉ xét khi có eligible order.

Payment/accounting chưa xác nhận thì commission chưa payable.

Ads/Diamond conflict vào review.

Intent đại lý/nhà phân phối/mua sỉ không route nhầm sang Diamond.

Thiếu diamond_bind_id nhưng vẫn áp referral.

Hardcode hoa hồng.

Tạo commission không có eligible order.

Route sai domain.


### 60.8. M04-RT-SMK-008 — Pricing / QuoteSnapshot Smoke

Kiểm tra AI không báo giá nếu thiếu QuoteSnapshot.

PricingResolver.

ProgramResolver.

GoldenHourResolver.

MemberBenefitResolver.

DiamondBenefitResolver.

QuoteSnapshotResolver.

QuoteExpiryResolver.

Quote có quote_snapshot_id.

Program price có program_activation_id.

Giờ Vàng có golden_hour_session_id.

Member/Diamond benefit có context hợp lệ.

Quote hết hạn tạo quote mới.

Báo giá không có QuoteSnapshot.

Nói giá chương trình khi thiếu ProgramResolver.

Nói Giờ Vàng khi thiếu session.


### 60.9. M04-RT-SMK-009 — Product Activation / Sellable Smoke

Kiểm tra sản phẩm chỉ được nói/bán khi active, sellable, có price, đúng board, đúng chương trình và đúng kênh.

ProductActivationResolver.

SellableStatusResolver.

StockAvailabilityResolver.

HoldStatusResolver.

ProgramActivationResolver.

DailyLiveProductBoardResolver.

MCAIScriptBriefResolver.

SKU active/sellable mới được quote.

Stock = 0 bị Auto Stop Sale.

Hold/recall/sale lock/channel block chặn bán.

Live board chỉ chứa SKU active.

MC AI không nói SKU ngoài brief.

Fallback sang SKU thay thế hợp lệ nếu có.

Quote SKU bị hold.

MC AI tự chọn SKU.

Live board thiếu resolver.


### 60.10. M04-RT-SMK-010 — Payment / Bank Transfer / Accounting Smoke

Kiểm tra payment phải qua Payment Core, Bank Registry và Accounting Review khi cần.

PaymentReferenceResolver.

CompanyBankAccountRegistryResolver.

BankTransferQueueResolver.

AccountingReviewResolver.

PaymentStatusResolver.

PaymentDisputeResolver.

Chuyển khoản có payment_reference.

Tài khoản ngân hàng lấy từ Bank Registry.

Ảnh giao dịch không tự PAID.

Accounting Review xác nhận trước PAID trong luồng cần review.

Payment dispute suppress sales/CRM.

Không hardcode tài khoản ngân hàng.

Hướng dẫn chuyển khoản thiếu payment_reference.

Hardcode tài khoản ngân hàng.

Khách nói đã chuyển là chuyển PAID.

Thiếu Accounting Review trong luồng cần đối soát.


### 60.11. M04-RT-SMK-011 — Shipping / Tracking / COD Smoke

Kiểm tra ETA, COD, phí ship và tracking phải đến từ Shipping Core.

ShippingEligibilityResolver.

ShippingFeeResolver.

ETAResolver.

CODResolver.

TrackingResolver.

DeliveryIssueResolver.

Shipping status có shipping_tracking_event_id.

ETA từ Shipping Core.

COD từ COD Core.

Phí ship từ Shipping Core.

Delivery issue suppress CRM sales.

Không hỏi lại địa chỉ khi order đã có shipping info.

Bịa ETA.

Nói tracking khi thiếu shipping event.

Bịa phí ship.

CRM bán hàng khi delivery issue mở.


### 60.12. M04-RT-SMK-012 — OrderDraft / CustomerConfirmation / IVR Smoke

Kiểm tra order không được tạo nếu thiếu OrderDraft và CustomerConfirmation.

QuoteCartResolver.

OrderDraftResolver.

CustomerConfirmationResolver.

OrderCreationResolver.

IVROrderConfirmationResolver.

OrderIdempotencyResolver.

QuoteCart trace được QuoteSnapshot.

OrderDraft trace được QuoteCart.

CustomerConfirmation trace được OrderDraft.

OrderCode chỉ sinh sau CustomerConfirmation.

Double confirm không tạo nhiều order.

IVR chỉ xác nhận đơn, không đổi order state.

Tạo order thiếu CustomerConfirmation.

Nói đặt hàng thành công khi thiếu OrderCode.

IVR upsell hoặc đổi state ngoài phạm vi.

Draft sửa không revision/supersede.


### 60.13. M04-RT-SMK-013 — Gateway / Public Comment / Handoff Smoke

Kiểm tra Gateway giữ đúng public/private boundary, handoff và delivery log.

PublicCommentEventResolver.

ChannelPolicyResolver.

MessengerHandoffResolver.

HandoffDeliveryLogResolver.

PublicPrivateBoundaryResolver.

PIIGuardResolver.

LiveModerationResolver.

GatewayDedupResolver.

GatewayIdempotencyResolver.

Public comment không payment instruction.

Handoff fail dùng fallback an toàn.

Dedup/idempotency chặn lặp.

Nói đã gửi Messenger khi delivery log fail.

Không có dedup ở live scale.

Lặp PII ngoài public.


### 60.14. M04-RT-SMK-014 — Official Contact Resolver Smoke

Kiểm tra số điện thoại/kênh liên hệ chỉ được trả lời qua Official Contact Registry.

OfficialContactResolver.

ContactIntentResolver.

ContactPolicyResolver.

ContactDisplayEligibilityResolver.

Intent tham quan/gặp công ty/lãnh đạo resolve đúng contact.

Intent đại lý/nhà phân phối/hợp tác/mua số lượng nhiều resolve đúng contact.

Không hardcode số điện thoại.

Contact reply có audit.

Gửi số không có official_contact_id.

Intent chưa rõ nhưng tự chọn số.

Hardcode contact trong AI/CRM/Gateway/Landing/template.


### 60.15. M04-RT-SMK-015 — Health / Complaint / Priority Conflict Smoke

Kiểm tra health boundary, complaint, privacy, counterfeit và priority conflict thắng sales/CRM.

HealthBoundaryResolver.

ComplaintResolver.

PrivacyCaseResolver.

CounterfeitCaseResolver.

SuppressionReasonResolver.

EscalationResolver.

Khách không nêu bệnh thì AI không tự hỏi bệnh.

Khách nêu bệnh thì Health Boundary Guard kích hoạt.

Complaint/privacy/payment/delivery/health/counterfeit/member/commission dispute suppress sales/CRM.

Counterfeit wording thận trọng.

Case cần escalation được handoff đúng.

Health-sensitive không guard.

Counterfeit case mở nhưng vẫn chốt bán.


### 60.16. M04-RT-SMK-016 — Evidence / Completion / Gateway Gate Smoke

Kiểm tra evidence, smoke, completion gate, owner sign-off và release control.

EvidenceResolver.

SmokeStatusResolver.

EvidenceReviewResolver.

CompletionGateResolver.

GatewayGateResolver.

OwnerSignoffResolver.

ReleaseReadinessResolver.

GoLiveApprovalResolver.

Evidence có evidence_id.

Evidence accepted mới dùng PASS.

Smoke trace được evidence.

Completion Gate trace được P0 evidence.

Owner sign-off hợp lệ.

Gateway chỉ xem xét khi all P0 gates PASS.

Production Ready chỉ xét khi implementation + evidence + smoke + security + sign-off đủ.

Owner chưa sign-off.

Gọi Gateway PASS vì tài liệu đã viết xong.


## 61. CROSS-DOMAIN E2E RUNTIME SMOKE MAP


### 61.1. M04-RT-E2E-001 — Public Live Comment -> Messenger -> Quote

Luồng kiểm tra:

Public live comment hỏi giá -> Gateway Resolver -> Public/Private Guard -> Messenger Handoff -> Delivery Log -> Customer Identity Resolver -> Product Activation Resolver -> Pricing Resolver -> QuoteSnapshot.

Public không báo giá.

Private flow giữ correlation.

Quote có QuoteSnapshot.

Product active/sellable.

Missing condition bị block/fallback đúng.

Handoff fail nhưng nói success.

Quote thiếu QuoteSnapshot.

SKU không sellable nhưng vẫn quote.


### 61.2. M04-RT-E2E-002 — Customer Care -> Recommendation -> Quote

Khách cũ quay lại -> Customer Identity -> Customer Memory -> Care Before Next Sale -> Product Recommendation -> Product Activation -> QuoteSnapshot nếu khách muốn mua.

Hỏi thăm trải nghiệm trước khi bán.

Không bịa lịch sử mua.

Recommendation có Product Effectiveness.

SKU active/sellable.

Nhảy thẳng vào chốt đơn.

Dùng memory không snapshot.

Gợi ý SKU không active.

Báo giá thiếu QuoteSnapshot.


### 61.3. M04-RT-E2E-003 — CRM Lifecycle -> Suppression -> Outbound

CRM trigger D0/D1/D3/D7/D14/D21/D30/M2–M12 -> Customer Context -> Suppression Guard -> Dedup/Idempotency -> Message Job.

CRM có trigger.

Có customer identity.

Có suppression check.

Không gửi ngoài quiet hours.

Không vượt frequency cap.

Không gửi opt-out.

Không gửi sales khi có priority conflict.

Gửi thiếu trigger.

Gửi khi complaint mở.

Gửi trùng do retry.


### 61.4. M04-RT-E2E-004 — Quote -> OrderDraft -> CustomerConfirmation -> Order

QuoteSnapshot -> QuoteCart -> OrderConfirmationDraft -> CustomerConfirmation -> OrderCode.

OrderDraft không tạo nếu thiếu QuoteSnapshot.

CustomerConfirmation trace đúng draft.

OrderCode chỉ sinh sau confirmation.

Draft sửa có revision/supersede.

Dùng quote hết hạn để tạo draft.

Duplicate confirm tạo nhiều order.


### 61.5. M04-RT-E2E-005 — Order -> Payment -> Accounting Review -> Paid

OrderCode -> PaymentReference -> BankTransferQueue -> AccountingReview -> PaymentStatus.

Payment instruction có payment_reference.

Bank account từ Company Bank Account Registry.

Khách nói đã chuyển không tự PAID.

Accounting Review quyết định paid trong luồng cần đối soát.

Thiếu payment_reference.

Thiếu accounting_review_id nhưng vẫn PAID.

Commission payable khi payment chưa confirmed.


### 61.6. M04-RT-E2E-006 — Order -> Shipping -> Tracking -> CSKH

OrderCode -> Shipping Eligibility -> Shipping Event -> ETA/COD/Tracking -> Customer Care.

Shipping status từ Shipping Core.

ETA không bị bịa.

Nói trạng thái vận chuyển khi thiếu event.

Hỏi lại địa chỉ khi đã có shipping info.


### 61.7. M04-RT-E2E-007 — Diamond Link -> Bind -> Order -> Commission

DiamondReferralLink -> DiamondBind -> Customer Identity -> Quote -> Order -> Payment/Accounting -> CommissionEligibility.

Referral link hợp lệ.

Bind hợp lệ.

Eligible order hợp lệ.

Payment/accounting confirmed trước payable.

Self-purchase policy được kiểm tra.

No bind nhưng vẫn attribution.

No eligible order nhưng tạo commission.

No payment confirmation nhưng payable.

Hardcode commission.


### 61.8. M04-RT-E2E-008 — Complaint / Health / Privacy -> Suppression -> Resolution

Customer issue -> Case Resolver -> PriorityConflictGuard -> Suppression -> Handoff/Review -> Closure.

Case ID được tạo.

Sales/CRM bị suppress đúng phạm vi.

Safe response đúng boundary.

Escalation đúng target.

Closure có audit.

Action bán hàng chỉ mở lại khi Guard cho phép.

Suppress không audit.

Health case không guard.


### 61.9. M04-RT-E2E-009 — Evidence -> Smoke -> Completion Gate -> Owner Sign-off

Evidence submitted -> Evidence Review -> Smoke PASS -> Completion Gate -> Owner Sign-off -> Release Decision.

Smoke trace evidence.

Completion Gate trace P0 evidence.

No điểm chặn.

Release decision audit đầy đủ.

Gateway gọi PASS khi chưa đủ P0 gate.


## 62. RELEASE CONTROL CHO MASTER-04


### 62.1. MASTER-04 Governance Release

MASTER-04 có thể đạt governance release khi:

Runtime Principles đã rõ.

Resolver Model đã rõ.

Guard Boundary đã rõ.

Runtime Resolver Registry bao phủ 16 business domain.

Required Inputs / Outputs đã rõ.

Guard Decision đã rõ.

bị chặn-if-Missing Rule đã rõ.

Guard Priority Matrix đã rõ.

Suppression / Fallback / Handoff đã rõ.

Runtime Conflict Resolution đã rõ.

Smoke Mapping đã rõ.

Release Control đã rõ.

Không đi vào code/API/DB/UI/worker chi tiết.

Không cho Consumer tự suy luận rule kinh doanh.


### 62.2. MASTER-04 không mở Gateway

MASTER-04 Governance Release không mở Gateway.

Gateway chỉ được xem xét khi:

All P0 gates PASS.

Evidence package complete.

E2E smoke PASS.

Security/App Review evidence PASS nếu thuộc phạm vi.

Nếu các điều kiện trên chưa đủ, Gateway vẫn bị chặn.


### 62.3. MASTER-04 không xác nhận Production Ready

MASTER-04 không xác nhận Production Ready.

Production Ready chỉ được xét khi các pack implementation có:

Runtime Resolver thật.

Guard thật.

Audit thật.

Evidence Registry thật.

Smoke/E2E test có evidence accepted.

Security review PASS.

Rollback/incident handling nếu thuộc phạm vi.

Monitoring/observability nếu thuộc phạm vi.


### 62.4. MASTER-04 không xác nhận Release Approved

MASTER-04 không tự tạo Release Approved.

Release Approved chỉ hợp lệ khi có:

completion_gate_id PASS.

owner_signoff_id hợp lệ.

P0 evidence accepted.

Security/App Review/UAT PASS nếu thuộc phạm vi.

Không còn điểm chặn mở.


### 62.5. MASTER-04 không xác nhận Go-live Approved

Go-live Approved là quyết định sau cùng.

Go-live Approved chỉ được xét khi:

Gateway PASS nếu thuộc phạm vi.

Payment/Shipping/Order/CRM/Gateway P0 gates PASS.

Security/App Review/Meta permission nếu thuộc phạm vi đã PASS.

Owner phê duyệt go-live.

Rollback plan và incident handling đã sẵn sàng nếu thuộc phạm vi release.


## 63. RELEASE STATE MODEL CHO MASTER-04


### 63.1. Trạng thái governance dùng trong MASTER-04

MASTER-04 sử dụng các trạng thái governance:

DRAFT.

READY_FOR_REVIEW.

GOVERNANCE_PASS.

IMPLEMENTATION_REQUIRED.

bị chặn.


## Release_Review_Ready.


### 63.2. GOVERNANCE_PASS

GOVERNANCE_PASS nghĩa là tài liệu đã đủ chuẩn governance để làm nền cho implementation.

GOVERNANCE_PASS không có nghĩa:

Đã có code.

Đã có resolver thật.

Đã có guard thật.

Đã có database.

Đã có API.

Đã có UI.

Đã có worker.

Đã có smoke evidence.

Đã có Gateway PASS.

Đã production ready.

Đã release approved.

Đã go-live approved.


### 63.3. IMPLEMENTATION_REQUIRED

IMPLEMENTATION_REQUIRED nghĩa là các pack sau phải dùng MASTER-04 để thiết kế chi tiết:

Runtime Resolver.

Guard.

State decision.

Suppression.

Fallback.

Handoff.

Conflict handling.

Audit.

Evidence.

Smoke.

E2E test.

Release gate.

MASTER-04 không thay thế các tài liệu implementation này.


### 63.5. bị chặn

bị chặn nghĩa là có điều kiện P0 chưa đạt hoặc có điểm chặn đang mở.

Các nguyên nhân bị chặn gồm:

Missing Runtime Resolver.

Missing Required Input.

Missing Required Output.

Missing Guard.

Missing Guard Decision.

Missing suppression audit.

Missing handoff delivery log.

Missing conflict resolution.

Hardcode runtime data.

Consumer tự suy luận rule.

Payment điểm chặn.

Shipping điểm chặn.

Gateway điểm chặn.

Complaint/privacy/health/payment/delivery/member/commission dispute đang mở.

E2E smoke failed.


## 64. FINAL RELEASE STATUS FOR MASTER-04

Sau khi PHẦN 1/4, PHẦN 2/4, PHẦN 3/4 và PHẦN 4/4 được thống nhất, trạng thái governance của MASTER-04 như sau:


## 65. MASTER-04 FINAL DONE GATE SUMMARY

MASTER-04 được xem là đạt Final Runtime Done Gate ở tầng governance khi toàn bộ điều kiện sau được khóa:

Source-of-Truth quyết định dữ liệu.

Owner Core quyết định nghiệp vụ.

Runtime Resolver trả dữ liệu có nguồn, ID, trạng thái và hiệu lực.

Consumer chỉ tiêu thụ trong boundary.

Mỗi domain P0 có Runtime Resolver rõ.

Mỗi Resolver có Required Inputs rõ.

Mỗi Resolver có Required Outputs rõ.

Guard Decision phải rõ PASS/BLOCK/SUPPRESS/FALLBACK/HANDOFF/REVIEW_REQUIRED/ESCALATE/NO_ACTION.

Guard Priority Matrix phải thắng mọi Consumer Action.

Suppression phải có reason, case/source và audit.

Handoff phải có trace, target, status, delivery log nếu cần và audit.

Runtime Conflict không được Consumer tự xử lý.

Public/private boundary phải do Guard quyết định.


## 66. MASTER-04 FINAL CONCLUSION

MASTER-04 quy định chuẩn Runtime Resolution và Guard Control cho hệ thống Ginsengfood.

Từ thời điểm MASTER-04 được áp dụng, mọi quyết định runtime trong hệ thống Ginsengfood bắt buộc tuân thủ:

Không action P0 nào được chạy trực tiếp từ intent.

Không Consumer nào được tự suy luận dữ liệu runtime.

Không Consumer nào được hardcode runtime data.

Không AI nào được tự tính giá, tự xác nhận member, tự xác nhận Diamond, tự xác nhận PAID hoặc tự bịa shipping.

Không Gateway nào được báo giá public, chốt đơn public, lặp PII public hoặc nói đã gửi Messenger khi handoff chưa success.

Không CRM nào được gửi khi thiếu trigger, thiếu identity, thiếu dedup, thiếu idempotency hoặc có suppression.

Không Payment nào được PAID khi thiếu Payment Core / Accounting Review.

Không Shipping nào được trả trạng thái khi thiếu Shipping Core.

Không Product nào được bán khi thiếu ProductActivation/Sellable.

Không Quote nào được báo khi thiếu QuoteSnapshot.

Không Order nào được tạo khi thiếu OrderDraft và CustomerConfirmation.

Không Diamond commission nào được tạo khi thiếu bind/order/payment eligibility.

Không Official Contact nào được gửi khi thiếu OfficialContactResolver.

Không health-sensitive flow nào được tư vấn sâu khi thiếu Health Boundary Guard.

Không complaint/privacy/counterfeit/payment/delivery/member/commission dispute nào được bỏ qua để tiếp tục sales.

Không evidence chưa accepted nào được dùng để PASS.

Không owner review nào được xem là owner sign-off.

Không Completion Report nào thay thế Completion Gate.

MASTER-04 hoàn tất vai trò khóa lớp quyết định runtime của hệ thống Ginsengfood, bảo đảm các pack implementation sau không tự suy luận nghiệp vụ, không bypass Source-of-Truth, không phá Traceability ID, không mở action khi thiếu guard và không gọi PASS khi thiếu evidence.


## 67. MASTER-04 HANDOFF NOTE

MASTER-04 được dùng làm governance reference cho các tài liệu tiếp theo.

Khi các pack sau thiết kế database, API, DTO, UI, worker, event, log, resolver, guard, audit, smoke test hoặc evidence package, bắt buộc đối chiếu MASTER-04 để bảo đảm:

Resolver đúng domain.

Resolver đúng Source-of-Truth.

Resolver đúng Owner Core.

Resolver có Required Inputs.

Resolver có Required Outputs.

Resolver có status rõ.

Guard chạy sau Resolver.

Guard Decision rõ.

Priority Matrix được áp dụng.

Suppression có reason/audit.

Fallback không nâng trạng thái.

Handoff có trace/status.

Conflict không để Consumer tự xử lý.

Public/private boundary đúng.

Runtime Decision Record đầy đủ.

Evidence/gate/release không bị gọi PASS khi chưa đủ điều kiện.

MASTER-04 dựa trên governance từ MASTER-00, Source-of-Truth từ MASTER-01, Dependency Control từ MASTER-02 và Traceability ID từ MASTER-03.

MASTER-04 không yêu cầu sửa lại các MASTER trước đó.

MASTER-04 trở thành chuẩn bắt buộc cho mọi tài liệu implementation liên quan đến runtime decision, resolver, guard, suppression, fallback, handoff, conflict resolution, evidence và release control.

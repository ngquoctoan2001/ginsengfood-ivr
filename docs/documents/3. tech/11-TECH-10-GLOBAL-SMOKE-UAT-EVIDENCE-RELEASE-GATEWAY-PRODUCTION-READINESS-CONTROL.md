# TECH-10 - GLOBAL SMOKE UAT EVIDENCE RELEASE GATEWAY PRODUCTION READINESS CONTROL

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## CROSS-TECH VALIDATION / END-TO-END ACCEPTANCE / OWNER SIGN-OFF / GO-LIVE DECISION / GLOBAL GATEWAY CONTROL

## PHẦN 1/4 - GLOBAL RELEASE PRINCIPLES / SOURCE-OF-TRUTH / EVIDENCE BOUNDARY / NO-FAKE-READY RULE

## 1. MỤC TIÊU CỦA TECH-10

TECH-10 là tài liệu khóa lớp kiểm thử tổng hợp, evidence, UAT, release readiness và Global Gateway Control cho toàn bộ hệ thống Ginsengfood.

TECH-10 không thay thế TECH-00 -> TECH-09.

TECH-10 không viết lại nghiệp vụ của từng TECH.

TECH-10 không đi vào code.

TECH-10 không thiết kế API chi tiết.

TECH-10 không thiết kế database chi tiết.

TECH-10 không thiết kế UI chi tiết.

TECH-10 là lớp kiểm soát cuối để trả lời một câu hỏi rất quan trọng:

Toàn bộ tài liệu đã viết xong rồi thì hệ thống đã đủ điều kiện triển khai thật chưa?

Câu trả lời mặc định là:

## CHƯA.

Vì:

Tài liệu hoàn chỉnh chưa phải hệ thống chạy thật.

Dev triển khai xong từng phần chưa chắc tích hợp đúng.

Module chạy riêng lẻ chưa chắc end-to-end đúng.

Smoke chưa pass thì chưa Release Ready.

Evidence chưa accepted thì chưa Completion Decision.

Owner chưa sign-off thì chưa Release Approved.

Chưa có release decision thì chưa Go-live Approved.

Global Gateway mặc định bị chặn.

## 2. VAI TRÒ CỦA TECH-10 TRONG BỘ TECH

TECH-10 là lớp Global Release Governance.

TECH-10 đứng sau các TECH đã khóa:

## TECH-00 - Global Technical Governance.

## TECH-01 - Foundation / RBAC / Audit / Idempotency / Evidence Registry.

## TECH-02 - Product / SKU / Recipe / Activation.

## TECH-03 - Operational Core / Production / Warehouse / Inventory / Traceability / Recall / Sale Lock.

## TECH-04 - Commerce Runtime / Sellable Gate / Quote / Cart / Order / Payment / Shipping.

## TECH-05 - AI Advisor Runtime.

## TECH-06 - Facebook Gateway / Meta Live & Messenger.

## TECH-07 - Ads Measurement / Attribution / Verified Revenue / Scale Gate.

## TECH-08 - MC AI Live.

## TECH-09 - IVR / Order Confirmation / Anti-Fake-Order Control.

TECH-10 có nhiệm vụ:

Gom toàn bộ smoke của các TECH.

Kiểm tra cross-tech dependency.

Kiểm tra source-of-truth boundary.

Kiểm tra evidence thật.

Kiểm tra UAT.

Kiểm tra owner sign-off.

Kiểm tra release readiness.

Kiểm tra go-live decision.

Kiểm soát Global Gateway.

Ngăn việc gọi nhầm Documentation Complete là Production Ready.

## 3. NGUYÊN TẮC CỐT LÕI: NO-FAKE-READY RULE

## 3.1. Documentation Complete không phải Production Ready

Một TECH được viết đủ 4 phần chỉ có nghĩa:

Không có nghĩa là:

Đã code xong.

Đã tích hợp xong.

Đã chạy đúng.

Đã có evidence.

Đã smoke pass.

Đã UAT pass.

Đã owner sign-off.

Đã production ready.

Đã release ready.

Đã được go-live.

## 3.2. Dev Completed không phải Release Ready

Dev báo đã làm xong một module không đồng nghĩa module đó release được.

Muốn lên Release Ready phải có:

Implementation evidence.

Smoke execution evidence.

Integration evidence.

Fail-case evidence.

Security/RBAC evidence nếu liên quan.

Data/evidence/audit evidence nếu liên quan.

Owner review.

Không còn P0 điểm chặn.

## 3.3. Smoke Pass cục bộ không phải Global Pass

Một module smoke pass riêng lẻ chưa đủ.

Ví dụ:

AI Advisor trả lời đúng nhưng Commerce quote sai -> Global FAIL.

Commerce tạo order đúng nhưng IVR gọi sai khách trusted -> Global FAIL.

Ads ROAS đúng nhưng Verified Revenue sai nguồn -> Global FAIL.

MC AI Live script đúng nhưng Gateway public giá sai kênh -> Global FAIL.

Operational inventory đúng nhưng Sale Lock không chặn Commerce -> Global FAIL.

## 3.4. Evidence không accepted thì chưa PASS

Evidence phải được:

Nộp đúng format.

Có người review.

Có owner chấp nhận.

Có trạng thái accepted.

Có audit.

Có trace đến smoke hoặc requirement tương ứng.

Evidence chỉ tồn tại nhưng chưa accepted thì chưa đủ.

## 3.5. P0 điểm chặn còn mở thì Global Gateway vẫn bị chặn

Nếu còn bất kỳ P0 điểm chặn nào ở bất kỳ TECH nào:

Không Completion Decision.

Không Release Ready.

Không Release Approved.

Không Go-live Approved.

Không Scale Approved.

Không mở Global Gateway.

## 4. TECH-10 KHÔNG PHẢI TÀI LIỆU VIẾT CODE

TECH-10 không phải nơi đưa code cho dev copy.

Một hệ thống thực tế không thể xây bằng cách copy vài đoạn code rời rạc từ AI.

Các đoạn code rời rạc không có đủ:

Context toàn hệ thống.

Kiến trúc dự án.

Database thật.

Flow nghiệp vụ thật.

State machine thật.

Security/RBAC thật.

Evidence/audit thật.

Runtime dependency thật.

Error handling thật.

Release/rollback thật.

Ví dụ, một chức năng tưởng đơn giản như “xác nhận đơn hàng” không chỉ là một form hoặc một API.

Nó liên quan đến:

Official order từ Commerce.

Customer trust.

Payment boundary.

IVR eligibility.

Call attempt policy.

Invalid phone.

No answer.

Customer cancel.

Technical failure.

Core Order State Machine.

Notification Owner.

Privacy-safe message.

Evidence/call log/audit.

Human/Admin Override.

Release gate.

Do đó, TECH-10 giữ đúng nguyên tắc:

Người dùng quyết định hệ thống cần đạt điều kiện gì.

Developer quyết định triển khai kỹ thuật như thế nào.

Tài liệu khóa nghiệp vụ, boundary, gate, evidence và smoke để dev không tự suy luận.

## 5. GLOBAL SOURCE-OF-TRUTH BOUNDARY

## 5.1. TECH-10 không tạo nguồn sự thật mới về nghiệp vụ

TECH-10 không thay thế source-of-truth của từng domain.

TECH-10 chỉ kiểm tra mỗi domain có tuân thủ đúng source-of-truth đã khóa hay không.

## 5.2. Source-of-truth theo domain

Các source-of-truth phải được giữ nguyên:

Product / SKU / Recipe truth thuộc TECH-02.

Operational / Inventory / Warehouse / Trace / Recall truth thuộc TECH-03.

Commerce / Quote / Cart / Order / Payment / Shipping truth thuộc TECH-04.

AI Advisor response truth thuộc TECH-05.

Facebook Gateway delivery/channel truth thuộc TECH-06.

Ads Measurement / Verified Revenue / Scale Gate truth thuộc TECH-07.

MC AI Live script/runtime truth thuộc TECH-08.

IVR Order Confirmation truth thuộc TECH-09.

Global release/evidence/smoke/go-live truth thuộc TECH-10.

## 5.3. TECH-10 được quyền chặn release

TECH-10 không sở hữu nghiệp vụ từng domain, nhưng có quyền chặn release nếu:

Domain tự vượt quyền.

Domain bỏ qua source-of-truth upstream.

Domain không có evidence.

Domain smoke fail.

Domain thiếu owner sign-off.

Domain có P0 điểm chặn.

Cross-tech integration fail.

Global Gateway chưa đủ điều kiện mở.

## 5.4. P0 điểm chặn

FAIL nếu TECH-10 tự sửa nghiệp vụ của TECH khác.

FAIL nếu TECH-10 cho release khi domain source-of-truth chưa pass.

FAIL nếu một domain tự tuyên bố ready mà không qua TECH-10 Global Gate.

## 6. GLOBAL GATEWAY BOUNDARY

## 6.1. Global Gateway là gì

Global Gateway là cổng quyết định trạng thái tổng thể của hệ thống.

Global Gateway trả lời:

Hệ thống chỉ mới viết xong tài liệu?

Hệ thống đã có evidence chưa?

Hệ thống đã smoke pass chưa?

Hệ thống đã UAT pass chưa?

Hệ thống đã owner sign-off chưa?

Hệ thống đã được release decision chưa?

Hệ thống có được go-live chưa?

Hệ thống có được scale chưa?

## 6.2. Trạng thái mặc định

Trạng thái mặc định:

Global Gate bị chặn

Global Gateway chỉ được chuyển trạng thái khi đủ điều kiện.

Không có chuyện tài liệu viết xong thì tự mở Gateway.

Không có chuyện dev báo xong thì tự mở Gateway.

Không có chuyện demo chạy được một luồng thì tự mở Gateway.

Không có chuyện dashboard đẹp thì tự mở Gateway.

Không có chuyện Ads có ROAS đẹp thì tự mở Gateway.

## 6.3. Các trạng thái Global Gateway

Global Gateway có các trạng thái chính:

bị chặn
Mặc định. Chưa đủ điều kiện mở.

DOCUMENTATION_COMPLETE_WAITING_EVIDENCE
Tài liệu đã xong, đang chờ evidence.

EVIDENCE_SUBMITTED_WAITING_REVIEW
Evidence đã nộp, đang chờ review.

EVIDENCE_ACCEPTED_WAITING_SMOKE
Evidence đã accepted, chờ smoke.

SMOKE_RUNNING
Đang chạy smoke.

SMOKE_PASS_WAITING_UAT
Smoke đã pass, chờ UAT.

UAT_RUNNING
Đang UAT.

UAT_PASS_WAITING_OWNER_SIGNOFF
UAT đã pass, chờ owner sign-off.

OWNER_SIGNED_WAITING_RELEASE_DECISION
Owner đã ký, chờ quyết định release.

RELEASE_DECISION_ACCEPTED_WAITING_GO_LIVE
Release approved, chờ go-live.

GO_LIVE_DECISION_ACCEPTED
Được phép go-live theo decision.

GO_LIVE_BLOCKED
Bị chặn go-live do điểm chặn.

ROLLBACK_REQUIRED
Phải rollback hoặc tạm dừng.

POST_GO_LIVE_MONITORING
Đã go-live, đang theo dõi.

SCALE_BLOCKED
Chưa được scale.

SCALE_APPROVED
Được scale theo điều kiện đã pass.

## 6.4. P0 điểm chặn

FAIL nếu Global Gateway được mở khi chưa có evidence accepted.

FAIL nếu Global Gateway được mở khi smoke chưa pass.

FAIL nếu Global Gateway được mở khi owner chưa sign-off.

FAIL nếu Global Gateway được mở khi chưa có release decision.

FAIL nếu Global Gateway được mở dù còn P0 điểm chặn.

## 7. DOCUMENTATION COMPLETE BOUNDARY

## 7.1. Điều kiện gọi Documentation Complete

Một TECH được gọi là Documentation Complete khi:

Đã viết đủ các phần đã quy định.

Có scope rõ.

Có boundary rõ.

Có dependency rõ.

Có P0 điểm chặn rõ.

Có evidence requirement rõ.

Có smoke requirement rõ.

Có Done Gate rõ.

Có Fail Gate rõ.

Có Release Handoff rõ.

## 7.2. Documentation Complete không cho phép triển khai thật

Documentation Complete chỉ cho phép:

Chuyển sang dev analysis.

Chuyển sang implementation planning.

Chuyển sang test planning.

Chuyển sang evidence planning.

Chuyển sang owner review nội dung.

Documentation Complete không cho phép:

Go-live.

Mở bán thật nếu domain liên quan sellable/order/payment.

Scale Ads.

Auto gửi tin khách hàng.

Auto gọi IVR thật.

Auto chạy payment thật.

Auto đồng bộ MISA production.

Tự tuyên bố Production Ready.

## 7.3. P0 điểm chặn

FAIL nếu tài liệu viết xong được gọi là Production Ready.

FAIL nếu tài liệu viết xong được dùng để mở production traffic.

FAIL nếu Documentation Complete bỏ qua smoke/evidence/owner sign-off.

## 8. EVIDENCE BOUNDARY

## 8.1. Evidence là gì

Evidence là bằng chứng chứng minh:

Dev đã triển khai đúng requirement.

Module chạy đúng trong môi trường phù hợp.

State machine đúng.

Boundary không bị bypass.

P0 điểm chặn không xảy ra.

Smoke đã chạy.

Fail-safe đã chạy.

Privacy/security đã kiểm tra.

Owner đã review.

Release decision có căn cứ.

## 8.2. Evidence không phải lời nói

Các câu sau không được xem là evidence:

“Dev nói đã xong.”

“Em test thấy chạy.”

“Dashboard có số.”

“API trả về 200.”

“AI trả lời có vẻ đúng.”

“Khách nhắn được.”

“Live chạy được.”

“Ads có đơn.”

“IVR gọi được.”

“Không thấy lỗi.”

Evidence phải có trace cụ thể.

## 8.3. Evidence tối thiểu cần có

Evidence tối thiểu gồm:

Requirement reference.

Test case reference.

Environment.

Input.

Expected result.

Actual result.

Pass/Fail.

Timestamp.

Owner/reviewer.

Screenshot/log/export nếu phù hợp.

Audit/correlation id nếu là runtime flow.

P0 điểm chặn check.

Decision status.

## 8.4. Evidence Accepted

Evidence chỉ được xem là accepted khi:

Nộp đủ.

Đúng format.

Đúng environment.

Đúng requirement.

Đúng test case.

Không thiếu dữ liệu trọng yếu.

Không dùng dữ liệu giả sai policy.

Owner hoặc reviewer chấp nhận.

Có trạng thái accepted.

Có audit.

## 8.5. P0 điểm chặn

FAIL nếu dùng lời nói thay evidence.

FAIL nếu screenshot không trace được requirement.

FAIL nếu log không có timestamp/correlation.

FAIL nếu evidence không gắn owner/reviewer.

FAIL nếu evidence chưa accepted nhưng gate vẫn PASS.

## 9. SMOKE BOUNDARY

## 9.1. Smoke là gì

Smoke là kiểm thử tối thiểu để chứng minh một luồng quan trọng không bị hỏng.

Smoke không thay thế full test.

Smoke không thay thế UAT.

Smoke không thay thế security test.

Smoke không thay thế performance test.

Smoke là điều kiện tối thiểu để xét Release Ready.

## 9.2. Smoke phải kiểm tra cả pass-case và fail-case

Smoke không chỉ test luồng thành công.

Smoke phải test:

Luồng đúng.

Luồng bị chặn.

Luồng thiếu dữ liệu.

Luồng runtime unavailable.

Luồng privacy-sensitive.

Luồng P0 điểm chặn.

Luồng cross-tech dependency.

Luồng rollback/fallback nếu phù hợp.

## 9.3. Smoke pass cục bộ chưa đủ

Smoke của một module pass chưa đủ để Global Pass.

Global Smoke phải kiểm tra:

## TECH-02 -> TECH-03.

## TECH-03 -> TECH-04.

## TECH-04 -> TECH-05.

## TECH-05 -> TECH-06.

## TECH-06 -> TECH-08.

## TECH-04 -> TECH-09.

## TECH-04 -> TECH-07.

TECH-03 Sale Lock / Recall -> toàn bộ Commerce/AI/Ads/Live/IVR.

Evidence/Audit -> toàn bộ high-risk actions.

Owner sign-off -> Release decision.

## 9.4. P0 điểm chặn

FAIL nếu chỉ test happy path.

FAIL nếu không test fail-safe.

FAIL nếu không test Sale Lock / Recall.

FAIL nếu không test privacy boundary.

FAIL nếu smoke pass nhưng không có evidence.

## 10. UAT BOUNDARY

## 10.1. UAT là gì

UAT là bước xác nhận hệ thống đáp ứng vận hành thực tế.

UAT không phải dev tự test.

UAT không phải unit test.

UAT không phải demo nhanh.

UAT phải có owner nghiệp vụ hoặc người được ủy quyền xác nhận.

## 10.2. UAT phải theo vai trò vận hành

UAT phải có các vai trò phù hợp:

Product Owner.

Operational Owner.

Warehouse/Inventory Owner.

Commerce Owner.

Customer Service Owner.

AI Advisor Owner.

Facebook Gateway Owner.

Ads Owner.

MC Live Owner.

IVR Owner.

Privacy/Legal Owner.

Release Owner.

## 10.3. UAT không được bỏ qua fail-case

UAT phải kiểm tra:

Khách hỏi mua.

Khách hỏi giá.

Khách chốt đơn.

Khách cũ/trusted.

Khách mới/untrusted.

SKU không sellable.

SKU Sale Lock.

Recall.

Payment WAITING.

## COD WAITING.

Verified Revenue.

Ads attribution.

Live comment -> Messenger.

Troll/abuse.

IVR no answer.

IVR invalid phone.

Customer cancel.

Technical failure.

Notification privacy.

Owner override.

## 10.4. P0 điểm chặn

FAIL nếu UAT chỉ do dev tự xác nhận.

FAIL nếu UAT không có owner nghiệp vụ.

FAIL nếu UAT bỏ qua các luồng bị chặn.

FAIL nếu UAT pass nhưng còn P0 điểm chặn.

## 11. OWNER SIGN-OFF BOUNDARY

## 11.1. Owner Sign-off là gì

Owner sign-off là xác nhận có trách nhiệm của chủ sở hữu nghiệp vụ/kỹ thuật đối với domain tương ứng.

Sign-off không phải “đã xem qua”.

Sign-off không phải “tạm ổn”.

Sign-off không phải “cứ chạy thử đi”.

Sign-off phải dựa trên:

Requirement đã khóa.

Evidence đã accepted.

Smoke pass.

UAT pass nếu áp dụng.

P0 điểm chặn closed.

Risk đã biết.

Release scope rõ.

## 11.2. Các owner bắt buộc

Tùy scope release, cần các owner:

Foundation / Security / RBAC Owner.

Product / SKU / Recipe Owner.

Operational Core Owner.

Warehouse / Inventory Owner.

Trace / Recall / Sale Lock Owner.

Commerce Owner.

Payment Owner.

Shipping / Delivery Owner.

AI Advisor Owner.

Facebook Gateway Owner.

Ads Measurement Owner.

MC AI Live Owner.

IVR Owner.

Customer Trust / CRM Owner.

Notification Owner.

Privacy / Legal Owner.

QA/UAT Owner.

Release Owner.

## 11.3. Sign-off không được thay release decision

Owner sign-off là điều kiện cần.

Nhưng owner sign-off chưa phải Go-live Approved.

Go-live cần release decision cuối.

## 11.4. P0 điểm chặn

FAIL nếu thiếu owner sign-off mà vẫn Release Approved.

FAIL nếu sign-off không gắn evidence.

FAIL nếu sign-off không ghi rõ scope.

FAIL nếu sign-off bỏ qua open điểm chặn.

## 12. RELEASE DECISION BOUNDARY

## 12.1. Release Decision là gì

Release Decision là quyết định chính thức cho phép hoặc không cho phép đưa một scope vào vận hành thật.

Release Decision phải rõ:

Release scope.

Environment.

Release time.

Owner quyết định.

Điều kiện đã pass.

Điều kiện còn theo dõi.

Rollback/fallback.

Monitoring window.

Incident owner.

Final gateway status.

## 12.2. Các loại quyết định release

Các loại release decision:

RELEASE_REJECTED
Không cho release.

RELEASE_DECISION_ACCEPTED_FOR_INTERNAL_TEST
Chỉ cho nội bộ test.

RELEASE_DECISION_ACCEPTED_FOR_STAGING_UAT
Cho UAT staging.

RELEASE_DECISION_ACCEPTED_FOR_LIMITED_PILOT
Cho pilot giới hạn.

RELEASE_DECISION_ACCEPTED_FOR_PRODUCTION_GO_LIVE
Cho go-live production.

RELEASE_DECISION_ACCEPTED_WITH_CONDITIONS
Cho release có điều kiện giám sát.

RELEASE_PAUSED
Tạm dừng release.

ROLLBACK_REQUIRED
Phải rollback.

## 12.3. Go-live Approved

Go-live Approved chỉ có khi:

Release Ready.

Owner sign-off đủ.

P0 điểm chặn closed.

Evidence accepted.

Smoke pass.

UAT pass nếu áp dụng.

Privacy/security review pass.

Monitoring ready.

Rollback/fallback ready.

Release decision chính thức cho production.

## 12.4. P0 điểm chặn

FAIL nếu không có release decision mà vẫn go-live.

FAIL nếu release decision không ghi scope.

FAIL nếu release decision không ghi rollback/fallback.

FAIL nếu release decision bỏ qua P0 điểm chặn.

## 13. CROSS-TECH DEPENDENCY PRINCIPLE

## 13.1. Không module nào được release một mình nếu phụ thuộc chưa pass

Một module không được release nếu dependency upstream chưa pass.

Ví dụ:

Commerce không được release nếu Operational Sellable Gate chưa đúng.

AI Advisor không được release nếu Product Knowledge/Claim/Commerce Quote chưa đúng.

Gateway không được release nếu AI Final Response Guard chưa đúng.

Ads không được scale nếu Verified Revenue chưa đúng.

MC AI Live không được go-live nếu Gateway/AI/Commerce/Ads-safe chưa pass.

IVR không được chạy thật nếu Official Order/Core Order State Machine/Notification Owner chưa pass.

Global Go-live không được mở nếu bất kỳ critical dependency còn bị chặn.

## 13.2. Dependency upstream thắng downstream

Nếu upstream bị chặn, downstream bị chặn.

Nếu Operational Sale Lock active, Commerce bị chặn.

Nếu Commerce bị chặn, AI order/quote bị chặn.

Nếu AI bị chặn, Gateway không gửi response.

Nếu Verified Revenue bị chặn, Ads ROAS bị chặn.

Nếu Gateway bị chặn, MC AI Live public/private routing bị chặn.

Nếu Core Order State Machine bị chặn, IVR confirmation bị chặn.

## 13.3. P0 điểm chặn

FAIL nếu downstream tự bypass upstream.

FAIL nếu dashboard downstream báo pass dù upstream fail.

FAIL nếu release một module critical khi dependency chưa pass.

## 14. GLOBAL P0 điểm chặn REGISTRY - PHẦN 1/4

Các lỗi sau là P0 điểm chặn cấp Global:

Gọi Documentation Complete là Production Ready.

Không có accepted evidence nhưng Completion Decision.

Không có smoke pass nhưng Release Ready.

Không có owner sign-off nhưng Release Approved.

Không có release decision nhưng Go-live Approved.

Global Gateway mở khi còn điểm chặn.

Một module tự bypass source-of-truth upstream.

Operational bị chặn nhưng Commerce vẫn bán.

Product Active bị hiểu là Sellable.

SKU Sale Lock / Recall vẫn được bán.

AI tự báo giá không qua QuoteSnapshot.

AI tự tạo official order.

Gateway public dữ liệu private.

Gateway gửi response chưa qua Final Response Guard.

Ads dùng quote/cart/order draft/unpaid order làm revenue.

Ads scale khi data quality fail.

MC AI Live dùng live signal làm ROAS.

MC AI Live dùng claim chưa approved.

IVR gọi Quote/Cart/Order Draft.

IVR gọi khách trusted đại trà.

IVR tự xác nhận payment/revenue.

IVR tự gửi notification sau hủy.

Payment WAITING bị tính PAID.

COD WAITING bị tính revenue.

Verified Revenue không qua Commerce.

Public Trace lộ dữ liệu không whitelist.

Evidence thiếu nhưng vẫn pass.

Smoke chỉ test happy path.

UAT không có owner nghiệp vụ.

Admin override không audit.

Runtime unavailable nhưng hệ thống tự suy đoán.

Privacy/Legal chưa pass nhưng go-live.

Rollback/fallback không có nhưng release production.

Monitoring/alert chưa sẵn sàng nhưng go-live.

Scale Ads khi Global Gateway chưa pass.

## 15. GLOBAL EVIDENCE PRINCIPLE - PHẦN 1/4

## 15.1. Evidence phải gắn requirement

Mọi evidence phải gắn với:

TECH number.

Section hoặc requirement.

Smoke ID.

Test scenario.

Expected result.

Actual result.

Pass/Fail.

Reviewer.

## 15.2. Evidence phải gắn environment

Evidence phải ghi rõ:

Local.

Dev.

Staging.

Production-like.

Production.

Sandbox provider.

Real provider nếu được phép.

Không được dùng evidence local để tuyên bố production ready.

Không được dùng dữ liệu giả không đánh dấu để tuyên bố production truth.

## 15.3. Evidence phải kiểm soát dữ liệu nhạy cảm

Evidence không được lộ:

Full customer private data nếu không cần.

Payment detail nhạy cảm.

Health note.

Member/Diamond private info nếu không cần.

Secret/token.

Internal credential.

Production personal data sai policy.

## 15.4. P0 điểm chặn

FAIL nếu evidence không gắn requirement.

FAIL nếu evidence không rõ environment.

FAIL nếu evidence chứa dữ liệu nhạy cảm sai policy.

FAIL nếu evidence local bị dùng để mở production.

## 16. GLOBAL SMOKE PRINCIPLE - PHẦN 1/4

## 16.1. Smoke phải có mã định danh

Mỗi smoke phải có:

Smoke ID.

TECH liên quan.

Domain.

Scenario.

Given.

When.

Then.

Must Not.

Evidence Required.

Owner.

Pass/Fail.

## 16.2. Smoke phải có cross-tech smoke

TECH-10 bắt buộc có cross-tech smoke, không chỉ module smoke.

Các smoke cross-tech tối thiểu:

Product -> Operational -> Commerce Sellable.

Commerce Quote -> AI Advisor Response.

Public Comment -> Gateway -> Messenger.

MC AI Live -> Gateway -> Messenger -> Commerce.

Order -> IVR -> Core Order State Machine -> Notification.

Order -> Payment -> Verified Revenue -> Ads.

Recall/Sale Lock -> Commerce/AI/Gateway/Ads/Live/IVR suppression.

Evidence/Audit -> Owner Sign-off -> Release Gate.

## 16.3. Smoke fail phải block release

Nếu một smoke P0 fail:

Global Gate bị chặn.

Phải có fix/evidence/retest.

## 17. GLOBAL DONE GATE - PHẦN 1/4

## PHẦN 1/4 của TECH-10 đạt Documentation Complete ở cấp nguyên tắc khi:

Đã khóa vai trò TECH-10.

Đã khóa No-Fake-Ready Rule.

Đã khóa Global Source-of-Truth Boundary.

Đã khóa Global Gateway Boundary.

Đã khóa Documentation Complete Boundary.

Đã khóa Evidence Boundary.

Đã khóa Smoke Boundary.

Đã khóa UAT Boundary.

Đã khóa Owner Sign-off Boundary.

Đã khóa Release Decision Boundary.

Đã khóa Cross-Tech Dependency Principle.

Đã có Global P0 điểm chặn Registry.

Đã có Global Evidence Principle.

Đã có Global Smoke Principle.

Đã sẵn sàng chuyển sang PHẦN 2/4 - Global Release Module Contracts.

## 18. GLOBAL FAIL GATE - PHẦN 1/4

## PHẦN 1/4 FAIL nếu:

TECH-10 bị hiểu là tài liệu code.

TECH-10 bị hiểu là thay thế TECH-00 -> TECH-09.

Documentation Complete bị hiểu là Production Ready.

Dev Completed bị hiểu là Release Ready.

Smoke pass cục bộ bị hiểu là Global Pass.

Evidence chưa accepted nhưng gate vẫn pass.

Owner chưa sign-off nhưng Release Approved.

Không có release decision nhưng Go-live Approved.

Global Gateway không mặc định bị chặn.

Cross-tech dependency chưa được khóa.

P0 điểm chặn chưa được định nghĩa.

Evidence boundary chưa rõ.

UAT boundary chưa rõ.

Release decision boundary chưa rõ.

## 19. RELEASE HANDOFF - SANG PHẦN 2/4

## PHẦN 1/4 bàn giao sang PHẦN 2/4 các nguyên tắc đã khóa:

TECH-10 là Global Release Governance.

Global Gateway mặc định bị chặn.

Documentation Complete không phải Production Ready.

Evidence accepted mới xét Completion Decision.

Smoke pass mới xét Release Ready.

Owner sign-off mới xét Release Approved.

Release decision mới xét Go-live Approved.

Cross-tech dependency phải được kiểm tra.

P0 điểm chặn còn mở thì Gateway vẫn bị chặn.

Evidence phải gắn requirement/smoke/environment/owner.

Smoke phải có cả pass-case và fail-case.

UAT phải có owner nghiệp vụ.

Release decision phải có scope/rollback/monitoring/incident owner.

## PHẦN 2/4 sẽ viết module contracts cho:

Global Release Orchestrator.

Documentation Completion Registry.

Evidence Intake & Acceptance Resolver.

Global Smoke Matrix Orchestrator.

UAT Session Resolver.

Owner Sign-off Resolver.

P0 điểm chặn Registry & Resolution Resolver.

Cross-Tech Dependency Validator.

Privacy/Security Release Reviewer.

Production Readiness Resolver.

Rollback/Fallback Readiness Resolver.

Monitoring/Alert Readiness Resolver.

Release Decision Board.

Global Gateway State Controller.

Post-Go-Live Monitoring Resolver.

## 20. TRẠNG THÁI SAU PHẦN 1/4

Sau PHẦN 1/4:

Chưa được gọi là Documentation Complete.

Chưa được gọi là Production Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Go-live Approved.

Chưa được gọi là Global Gateway PASS.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 đã khóa vai trò của TECH-10 là lớp Global Smoke / UAT / Evidence / Release Gateway / Production Readiness Control cho toàn bộ hệ thống Ginsengfood.

TECH-10 không thay nghiệp vụ của TECH-00 -> TECH-09.

TECH-10 không viết code.

TECH-10 không cho phép hiểu nhầm tài liệu hoàn chỉnh là hệ thống sẵn sàng vận hành.

Các nguyên tắc đã khóa:

Documentation Complete không phải Production Ready.

Dev Completed không phải Release Ready.

Smoke pass cục bộ không phải Global Pass.

Evidence chưa accepted thì chưa Completion Decision.

Smoke chưa pass thì chưa Release Ready.

Owner chưa sign-off thì chưa Release Approved.

Chưa có release decision thì chưa Go-live Approved.

Global Gateway mặc định bị chặn.

Cross-tech dependency chưa pass thì downstream bị chặn.

P0 điểm chặn còn mở thì không release.

Evidence phải có requirement, environment, expected/actual, reviewer và audit.

UAT phải có owner nghiệp vụ.

Release decision phải có scope, rollback/fallback, monitoring và incident owner.

## PHẦN 1/4 sẵn sàng bàn giao sang:

## PHẦN 2/4 - GLOBAL RELEASE MODULE CONTRACTS.

## PHẦN 2/4 - GLOBAL RELEASE MODULE CONTRACTS

## 1. MỤC TIÊU PHẦN 2/4

## PHẦN 2/4 khóa các module bắt buộc của lớp Global Release Governance.

Mục tiêu là để toàn bộ đội triển khai hiểu rõ:

Module release nào chịu trách nhiệm kiểm tra tài liệu.

Module nào tiếp nhận evidence.

Module nào kiểm soát smoke.

Module nào kiểm soát UAT.

Module nào kiểm soát owner sign-off.

Module nào kiểm soát P0 điểm chặn.

Module nào kiểm tra cross-tech dependency.

Module nào quyết định production readiness.

Module nào kiểm tra rollback/fallback.

Module nào kiểm tra monitoring/alert.

Module nào giữ Global Gateway.

Không module nào được tự tuyên bố Production Ready nếu chưa đủ gate.

## PHẦN 2/4 không viết code.

## PHẦN 2/4 không thiết kế API chi tiết.

## PHẦN 2/4 không thiết kế database chi tiết.

## PHẦN 2/4 không thiết kế UI chi tiết.

## PHẦN 2/4 chỉ khóa module contract, boundary, dependency, evidence, smoke và P0 điểm chặn.

## 2. NGUYÊN TẮC MODULE CONTRACT CHUNG

Mọi module trong TECH-10 phải tuân thủ các nguyên tắc sau:

Không tự mở Global Gateway.

Không gọi Documentation Complete là Production Ready.

Không gọi Dev Completed là Release Ready.

Không gọi Smoke Pass cục bộ là Global Pass.

Không gọi Evidence Submitted là Evidence Accepted.

Không gọi Owner Reviewed là Owner Signed-off.

Không gọi Release Approved là Go-live Approved nếu chưa có release decision.

Không bỏ qua P0 điểm chặn.

Không bỏ qua cross-tech dependency.

Không bỏ qua privacy/security review.

Không bỏ qua rollback/fallback readiness.

Không bỏ qua monitoring/alert readiness.

Không dùng lời nói thay evidence.

Không dùng screenshot rời rạc không trace requirement làm evidence accepted.

Không dùng demo chạy được một luồng để tuyên bố production ready.

Không dùng dashboard đẹp để tuyên bố hệ thống đúng.

Không dùng ROAS đẹp để mở scale nếu Verified Revenue/Data Quality chưa pass.

Không để downstream tự bypass upstream.

Không release thật khi Global Gateway còn bị chặn.

Không để dev tự suy luận tiêu chuẩn pass/fail.

## 3. MODULE CONTRACT 01 - GLOBAL RELEASE ORCHESTRATOR

## 3.1. Mục tiêu

Global Release Orchestrator là module điều phối trung tâm của TECH-10.

Module này không triển khai nghiệp vụ domain.

Module này không sửa logic TECH-00 -> TECH-09.

Module này chỉ điều phối quá trình kiểm tra từ Documentation Complete đến Evidence, Smoke, UAT, Owner Sign-off, Release Decision và Global Gateway.

## 3.2. Scope In

Global Release Orchestrator được phép:

Nhận trạng thái từ các TECH.

Tổng hợp readiness theo từng domain.

Kiểm tra tài liệu đã đủ phần chưa.

Kiểm tra evidence đã nộp chưa.

Kiểm tra evidence đã accepted chưa.

Kiểm tra smoke đã chạy chưa.

Kiểm tra smoke đã pass chưa.

Kiểm tra UAT đã pass chưa.

Kiểm tra owner sign-off.

Kiểm tra P0 điểm chặn còn mở hay không.

Kiểm tra cross-tech dependency.

Kiểm tra privacy/security review.

Kiểm tra rollback/fallback readiness.

Kiểm tra monitoring/alert readiness.

Gửi trạng thái cuối sang Global Gateway State Controller.

Chặn release nếu chưa đủ điều kiện.

## 3.3. Scope Out

Global Release Orchestrator không được:

Tự đánh dấu Production Ready.

Tự đánh dấu Release Approved.

Tự đánh dấu Go-live Approved.

Tự mở Global Gateway.

Tự sửa nghiệp vụ của TECH khác.

Tự sửa evidence.

Tự bỏ qua smoke fail.

Tự bỏ qua P0 điểm chặn.

Tự thay owner sign-off.

Tự thay release decision.

Tự bỏ qua privacy/security review.

Tự cho phép scale Ads.

Tự cho phép IVR gọi thật.

Tự cho phép MC AI Live go-live.

Tự cho phép Commerce bán nếu Operational bị chặn.

## 3.4. Upstream Dependency

Module này phụ thuộc:

TECH-00 Governance.

TECH-01 Foundation/RBAC/Audit/Evidence.

TECH-02 Product/SKU/Recipe.

TECH-03 Operational Core.

TECH-04 Commerce Runtime.

TECH-05 AI Advisor.

TECH-06 Facebook Gateway.

TECH-07 Ads Measurement.

TECH-08 MC AI Live.

## TECH-09 IVR.

Evidence Intake & Acceptance Resolver.

Global Smoke Matrix Orchestrator.

UAT Session Resolver.

Owner Sign-off Resolver.

P0 điểm chặn Registry.

Cross-Tech Dependency Validator.

Release Decision Board.

## 3.5. Downstream Handoff

Module này bàn giao cho:

Global Gateway State Controller.

Release Decision Board.

Production Readiness Resolver.

Rollback/Fallback Readiness Resolver.

Monitoring/Alert Readiness Resolver.

Post-Go-Live Monitoring Resolver.

## 3.6. P0 điểm chặn

FAIL nếu Global Release Orchestrator tự mở Global Gateway.

FAIL nếu module này cho Release Ready khi smoke chưa pass.

FAIL nếu module này cho Release Approved khi owner chưa sign-off.

FAIL nếu module này cho Go-live Approved khi chưa có release decision.

FAIL nếu module này bỏ qua P0 điểm chặn.

## 3.7. Evidence

Evidence tối thiểu:

Release scope.

TECH status input.

Evidence status.

Smoke status.

UAT status.

Owner sign-off status.

P0 điểm chặn status.

Cross-tech dependency status.

Privacy/security review status.

Rollback/fallback status.

Monitoring readiness status.

Final gateway recommendation.

Audit log.

## 3.8. Smoke

## TECH10-MOD-SMK-001

Given TECH-10 nhận trạng thái Documentation Complete nhưng chưa có evidence accepted
When Global Release Orchestrator chạy
Then trạng thái không được vượt quá WAITING_EVIDENCE.

## TECH10-MOD-SMK-002

## 4. MODULE CONTRACT 02 - DOCUMENTATION COMPLETION REGISTRY

## 4.1. Mục tiêu

Documentation Completion Registry ghi nhận trạng thái hoàn tất tài liệu của từng TECH.

Module này chỉ xác nhận tài liệu đã đủ cấu trúc.

Module này không xác nhận hệ thống đã chạy đúng.

## 4.2. Scope In

Module được phép kiểm tra:

TECH đã viết đủ phần chưa.

Scope có rõ không.

Boundary có rõ không.

Dependency có rõ không.

P0 điểm chặn có rõ không.

Evidence requirement có rõ không.

Smoke requirement có rõ không.

Done Gate có rõ không.

Fail Gate có rõ không.

Release Handoff có rõ không.

Tài liệu có Word-ready không.

Tài liệu có mâu thuẫn source-of-truth không.

## 4.3. Scope Out

Module không được:

Xác nhận Production Ready.

Xác nhận Release Ready.

Xác nhận Go-live Approved.

Xác nhận code đã chạy.

Xác nhận evidence accepted.

Xác nhận smoke pass.

Xác nhận owner sign-off.

Xác nhận release decision.

Mở Global Gateway.

Bỏ qua thiếu sót tài liệu.

## 4.4. Upstream Dependency

Phụ thuộc:

TECH document package.

Source-of-truth registry.

Cross-tech dependency list.

P0 điểm chặn Registry.

Documentation format standard.

## 4.5. Downstream Handoff

Bàn giao:

Missing section list.

Conflict list.

Documentation Complete flag nếu đủ.

Handoff sang Evidence Intake & Acceptance Resolver.

## 4.6. P0 điểm chặn

FAIL nếu tài liệu thiếu Done Gate nhưng vẫn Documentation Complete.

FAIL nếu tài liệu thiếu Fail Gate nhưng vẫn Documentation Complete.

FAIL nếu tài liệu thiếu P0 điểm chặn nhưng vẫn Documentation Complete.

FAIL nếu Documentation Complete bị dùng để mở production.

## 4.7. Evidence

Evidence tối thiểu:

TECH number.

Version.

Section checklist.

Boundary checklist.

Dependency checklist.

P0 checklist.

Evidence requirement checklist.

Smoke requirement checklist.

Reviewer.

Status.

## 4.8. Smoke

## TECH10-MOD-SMK-003

Given TECH viết đủ nội dung nhưng thiếu Release Handoff
When Documentation Completion Registry kiểm tra
Then không được đánh Documentation Complete.

## TECH10-MOD-SMK-004

Given TECH đã Documentation Complete
When Release Gate kiểm tra
Then không được tự chuyển Production Ready.

## 5. MODULE CONTRACT 03 - EVIDENCE INTAKE & ACCEPTANCE RESOLVER

## 5.1. Mục tiêu

Evidence Intake & Acceptance Resolver tiếp nhận, kiểm tra và xác nhận evidence.

Module này quyết định evidence có đủ điều kiện accepted hay không.

## 5.2. Scope In

Module được phép:

Nhận evidence từ dev/QA/owner.

Kiểm tra evidence có gắn requirement không.

Kiểm tra evidence có gắn smoke ID không.

Kiểm tra environment.

Kiểm tra expected result.

Kiểm tra actual result.

Kiểm tra pass/fail.

Kiểm tra timestamp.

Kiểm tra reviewer.

Kiểm tra correlation/audit id nếu là runtime flow.

Kiểm tra dữ liệu nhạy cảm.

Đánh trạng thái ACCEPTED / REJECTED / NEED_MORE_EVIDENCE.

Ghi evidence decision.

Bàn giao sang Global Release Orchestrator.

## 5.3. Scope Out

Module không được:

Tự tạo evidence giả.

Chấp nhận lời nói thay evidence.

Chấp nhận screenshot không trace requirement.

Chấp nhận log thiếu timestamp.

Chấp nhận evidence không rõ environment.

Chấp nhận evidence local để mở production.

Chấp nhận evidence chứa dữ liệu nhạy cảm sai policy.

Sửa kết quả test.

Bỏ qua fail-case.

Bỏ qua P0 điểm chặn.

## 5.4. Upstream Dependency

Phụ thuộc:

Documentation Completion Registry.

Smoke Matrix.

Test execution output.

Logs/screenshots/exports.

Audit/correlation id.

Privacy/security evidence policy.

Reviewer/owner assignment.

## 5.5. Downstream Handoff

Bàn giao:

Evidence accepted list.

Evidence rejected list.

Evidence WAITING list.

Evidence gap list.

Evidence decision audit.

Handoff sang Global Release Orchestrator.

Handoff sang Owner Sign-off Resolver.

## 5.6. P0 điểm chặn

FAIL nếu evidence chưa accepted nhưng Completion Decision.

FAIL nếu evidence không gắn requirement vẫn accepted.

FAIL nếu evidence local được dùng để production-ready.

FAIL nếu evidence chứa secret/customer sensitive data sai policy.

FAIL nếu evidence bị sửa sau accepted không có audit.

## 5.7. Evidence

Evidence tối thiểu cho chính module này:

Evidence submission record.

Requirement mapping.

Smoke mapping.

Environment.

Reviewer.

Acceptance decision.

Rejection reason nếu fail.

Audit log.

## 5.8. Smoke

## TECH10-MOD-SMK-005

Given evidence chỉ là câu “dev đã test rồi”
When Evidence Intake kiểm tra
Then status = REJECTED.

## TECH10-MOD-SMK-006

Given evidence có requirement, smoke ID, expected/actual, timestamp, reviewer
When Evidence Intake kiểm tra
Then có thể ACCEPTED nếu không vi phạm policy.

## 6. MODULE CONTRACT 04 - GLOBAL SMOKE MATRIX ORCHESTRATOR

## 6.1. Mục tiêu

Global Smoke Matrix Orchestrator điều phối toàn bộ smoke test của TECH-00 -> TECH-09 và cross-tech smoke.

Module này đảm bảo hệ thống không chỉ pass từng module riêng lẻ mà còn pass end-to-end.

## 6.2. Scope In

Module được phép:

Lập smoke matrix tổng thể.

Gắn smoke với TECH tương ứng.

Gắn smoke với owner.

Gắn smoke với evidence.

Xác định smoke P0.

Xác định smoke pass-case.

Xác định smoke fail-case.

Xác định smoke cross-tech.

Ghi trạng thái smoke.

Block release nếu smoke fail.

Yêu cầu retest sau fix.

Tổng hợp smoke status cho Global Release Orchestrator.

## 6.3. Scope Out

Module không được:

Sửa logic domain để smoke pass.

Đánh pass khi chưa chạy.

Đánh pass khi thiếu evidence.

Bỏ qua fail-case.

Bỏ qua cross-tech smoke.

Bỏ qua Sale Lock / Recall smoke.

Bỏ qua privacy smoke.

Bỏ qua runtime unavailable smoke.

Bỏ qua Ads verified revenue smoke.

Bỏ qua IVR notification handoff smoke.

## 6.4. Upstream Dependency

Phụ thuộc:

TECH smoke requirements.

P0 điểm chặn Registry.

Evidence Intake Resolver.

QA execution plan.

Test environment readiness.

Owner review requirement.

## 6.5. Downstream Handoff

Bàn giao:

Smoke pass list.

Smoke fail list.

Smoke bị chặn list.

Retest required list.

P0 smoke status.

Global smoke readiness.

Handoff sang UAT Session Resolver và Global Release Orchestrator.

## 6.6. P0 điểm chặn

FAIL nếu smoke chỉ test happy path.

FAIL nếu smoke pass nhưng evidence thiếu.

FAIL nếu smoke fail vẫn Release Ready.

FAIL nếu smoke không test upstream/downstream dependency.

FAIL nếu smoke không test điểm chặn case.

## 6.7. Evidence

Evidence tối thiểu:

Smoke ID.

TECH reference.

Scenario.

Given/When/Then.

Expected result.

Actual result.

Pass/Fail.

Evidence reference.

Tester.

Reviewer.

Timestamp.

Retest status nếu có.

## 6.8. Smoke

## TECH10-MOD-SMK-007

Given module smoke pass nhưng cross-tech smoke fail
When Global Smoke Matrix tổng hợp
Then Global Smoke = FAIL.

## TECH10-MOD-SMK-008

## 7. MODULE CONTRACT 05 - UAT SESSION RESOLVER

## 7.1. Mục tiêu

UAT Session Resolver tổ chức và ghi nhận xác nhận vận hành thực tế từ owner nghiệp vụ.

Module này đảm bảo UAT không bị thay bằng dev test hoặc demo nhanh.

## 7.2. Scope In

Module được phép:

Xác định UAT scope.

Xác định UAT owner.

Xác định UAT scenario.

Lập UAT session.

Ghi nhận UAT execution.

Ghi nhận feedback.

Ghi nhận pass/fail.

Ghi nhận điểm chặn.

Yêu cầu fix/retest.

Ghi nhận UAT acceptance.

Bàn giao UAT status sang Release Orchestrator.

## 7.3. Scope Out

Module không được:

Cho dev tự sign-off UAT thay owner nghiệp vụ.

Bỏ qua UAT fail.

Bỏ qua feedback P0.

Bỏ qua fail-case.

Gọi demo nhanh là UAT pass.

Gọi smoke pass là UAT pass.

Chuyển UAT pass khi chưa có owner acceptance.

Ép owner sign-off khi evidence chưa đủ.

## 7.4. Upstream Dependency

Phụ thuộc:

Smoke pass status.

Evidence accepted status.

UAT scenario list.

Owner assignment.

Test environment.

Business acceptance criteria.

## 7.5. Downstream Handoff

Bàn giao:

UAT pass/fail status.

UAT feedback.

UAT điểm chặn.

UAT accepted evidence.

Handoff sang Owner Sign-off Resolver.

Handoff sang P0 điểm chặn Registry nếu fail.

## 7.6. P0 điểm chặn

FAIL nếu UAT không có owner nghiệp vụ.

FAIL nếu UAT chỉ là dev tự test.

FAIL nếu UAT pass khi còn P0 điểm chặn.

FAIL nếu UAT không test luồng bị chặn.

## 7.7. Evidence

Evidence tối thiểu:

UAT scope.

UAT scenario.

UAT owner.

Test environment.

Expected result.

Actual result.

Feedback.

Pass/Fail.

Acceptance decision.

Audit log.

## 7.8. Smoke

## TECH10-MOD-SMK-009

Given UAT do dev tự xác nhận
When UAT Resolver kiểm tra
Then UAT status = INVALID.

## TECH10-MOD-SMK-010

Given owner nghiệp vụ xác nhận UAT pass có evidence
When UAT Resolver kiểm tra
Then UAT có thể chuyển PASS nếu không còn điểm chặn.

## 8. MODULE CONTRACT 06 - OWNER SIGN-OFF RESOLVER

## 8.1. Mục tiêu

Owner Sign-off Resolver kiểm soát chữ ký/xác nhận trách nhiệm của từng owner.

Module này đảm bảo không có release nếu thiếu owner sign-off phù hợp.

## 8.2. Scope In

Module được phép:

Xác định owner bắt buộc theo release scope.

Kiểm tra owner đã review evidence chưa.

Kiểm tra owner đã review smoke chưa.

Kiểm tra owner đã review UAT chưa.

Kiểm tra open điểm chặn thuộc owner.

Ghi nhận sign-off.

Ghi nhận sign-off with condition nếu policy cho phép.

Ghi nhận rejection.

Bàn giao sign-off status sang Release Orchestrator.

Block release nếu thiếu sign-off.

## 8.3. Scope Out

Module không được:

Tự ký thay owner.

Gọi “đã xem” là sign-off.

Gọi “tạm ổn” là sign-off.

Cho sign-off khi evidence chưa accepted.

Cho sign-off khi smoke P0 fail.

Cho sign-off không rõ scope.

Cho release khi thiếu owner critical.

Bỏ qua Privacy/Legal owner nếu scope có dữ liệu khách hàng.

## 8.4. Upstream Dependency

Phụ thuộc:

Evidence accepted.

Smoke pass.

UAT pass nếu áp dụng.

Owner registry.

P0 điểm chặn Registry.

Release scope.

Privacy/security review nếu liên quan.

## 8.5. Downstream Handoff

Bàn giao:

Owner sign-off list.

Missing sign-off list.

Conditional sign-off list.

Rejected sign-off list.

điểm chặn by owner.

Handoff sang Release Decision Board.

## 8.6. P0 điểm chặn

FAIL nếu thiếu owner sign-off mà Release Approved.

FAIL nếu sign-off không gắn evidence.

FAIL nếu sign-off không rõ scope.

FAIL nếu owner sign-off khi P0 điểm chặn còn mở.

## 8.7. Evidence

Evidence tối thiểu:

Owner identity.

Domain.

Scope.

Evidence reviewed.

Smoke reviewed.

UAT reviewed nếu có.

Decision.

Conditions nếu có.

Timestamp.

Audit log.

## 8.8. Smoke

## TECH10-MOD-SMK-011

## TECH10-MOD-SMK-012

Given owner sign-off không gắn evidence
When resolver kiểm tra
Then sign-off invalid.

## 9. MODULE CONTRACT 07 - P0 điểm chặn REGISTRY & RESOLUTION RESOLVER

## 9.1. Mục tiêu

P0 điểm chặn Registry & Resolution Resolver quản lý toàn bộ lỗi P0 từ TECH-00 -> TECH-10.

Module này đảm bảo P0 điểm chặn không bị bỏ qua, không bị đóng bằng miệng, không bị ẩn để release.

## 9.2. Scope In

Module được phép:

Ghi nhận P0 điểm chặn.

Phân loại điểm chặn theo TECH/domain.

Gắn điểm chặn với owner.

Gắn điểm chặn với evidence.

Gắn điểm chặn với smoke fail.

Theo dõi trạng thái điểm chặn.

Yêu cầu fix.

Yêu cầu retest.

Đóng điểm chặn khi có evidence pass.

Block release nếu còn điểm chặn mở.

Lưu lịch sử điểm chặn.

## 9.3. Scope Out

Module không được:

Đóng điểm chặn không evidence.

Hạ cấp P0 điểm chặn tùy tiện.

Ẩn điểm chặn khỏi release gate.

Cho release khi điểm chặn mở.

Cho owner tự bỏ qua điểm chặn không audit.

Gộp điểm chặn nghiêm trọng vào “known issue” để go-live nếu chưa có release decision có điều kiện rõ.

Xóa lịch sử điểm chặn.

## 9.4. Upstream Dependency

Phụ thuộc:

P0 điểm chặn Registry từng TECH.

Smoke fail.

UAT fail.

Evidence rejection.

Owner feedback.

Production incident nếu có.

Privacy/security review.

## 9.5. Downstream Handoff

Bàn giao:

Open điểm chặn list.

Resolved điểm chặn list.

điểm chặn owner.

điểm chặn evidence.

Retest status.

Release blocking status.

Handoff sang Global Release Orchestrator.

## 9.6. P0 điểm chặn

FAIL nếu điểm chặn mở nhưng Global Gateway mở.

FAIL nếu điểm chặn đóng không evidence.

FAIL nếu P0 điểm chặn bị hạ cấp không approval.

FAIL nếu điểm chặn privacy/security bị bỏ qua.

## 9.7. Evidence

Evidence tối thiểu:

điểm chặn ID.

TECH/domain.

Description.

Severity.

Owner.

Detection evidence.

Fix evidence.

Retest evidence.

Resolution decision.

Audit log.

## 9.8. Smoke

## TECH10-MOD-SMK-013

Given P0 điểm chặn còn OPEN
When Global Gateway kiểm tra
Then Gateway = bị chặn.

## TECH10-MOD-SMK-014

Given điểm chặn được marked resolved nhưng không có retest evidence
When resolver kiểm tra
Then điểm chặn status không được CLOSED.

## 10. MODULE CONTRACT 08 - CROSS-TECH DEPENDENCY VALIDATOR

## 10.1. Mục tiêu

Cross-Tech Dependency Validator kiểm tra dependency giữa các TECH và chặn downstream khi upstream chưa pass.

Module này là chốt chặn tránh hệ thống chạy từng phần nhưng sai end-to-end.

## 10.2. Scope In

Module được phép kiểm tra:

TECH-02 -> TECH-03 dependency.

TECH-03 -> TECH-04 dependency.

TECH-04 -> TECH-05 dependency.

TECH-05 -> TECH-06 dependency.

TECH-06 -> TECH-08 dependency.

TECH-04 -> TECH-07 dependency.

TECH-04 -> TECH-09 dependency.

TECH-03 Sale Lock/Recall -> Commerce/AI/Gateway/Ads/Live/IVR.

TECH-01 Evidence/Audit -> toàn bộ high-risk modules.

TECH-10 Global Gate -> mọi release.

Upstream state.

Downstream state.

Bypass risk.

Dependency violation.

## 10.3. Scope Out

Module không được:

Tự sửa dependency.

Tự cho downstream pass khi upstream fail.

Bỏ qua Sale Lock/Recall.

Bỏ qua Verified Revenue dependency.

Bỏ qua QuoteSnapshot dependency.

Bỏ qua Final Response Guard.

Bỏ qua IVR Official Order dependency.

Cho release một module critical khi dependency chưa pass.

Gọi dashboard downstream là source-of-truth.

## 10.4. Upstream Dependency

Phụ thuộc:

TECH source-of-truth registry.

Domain readiness status.

Smoke/evidence status từng TECH.

P0 điểm chặn Registry.

Release scope.

## 10.5. Downstream Handoff

Bàn giao:

Dependency pass/fail.

bị chặn downstream list.

Upstream điểm chặn.

Bypass violation list.

Handoff sang Global Release Orchestrator.

Handoff sang P0 điểm chặn Registry nếu có fail.

## 10.6. P0 điểm chặn

FAIL nếu Commerce bán khi Operational Sellable Gate fail.

FAIL nếu AI báo giá khi Commerce QuoteSnapshot fail.

FAIL nếu Ads dùng revenue khi Verified Revenue fail.

FAIL nếu MC AI Live go-live khi Gateway/AI/Commerce chưa pass.

FAIL nếu IVR chạy khi Core Order State Machine chưa pass.

## 10.7. Evidence

Evidence tối thiểu:

Dependency pair.

Upstream status.

Downstream status.

Validation result.

Violation nếu có.

Block decision.

Reviewer.

Audit log.

## 10.8. Smoke

## TECH10-MOD-SMK-015

Given Operational Sale Lock active
When Commerce/AI/Ads/Live/IVR readiness được xét
Then tất cả downstream liên quan phải bị chặn.

## TECH10-MOD-SMK-016

Given Verified Revenue chưa pass
When Ads scale gate được xét
Then Scale Approved = KHONG.

## 11. MODULE CONTRACT 09 - PRIVACY / SECURITY RELEASE REVIEWER

## 11.1. Mục tiêu

Privacy / Security Release Reviewer kiểm tra các rủi ro về dữ liệu cá nhân, quyền truy cập, public/private boundary, audit, secret và nội dung nhạy cảm trước release.

## 11.2. Scope In

Module được phép kiểm tra:

Public/private data boundary.

Customer data privacy.

Payment data privacy.

Member/Diamond/referral privacy.

Health note privacy.

Call log/recording privacy.

Evidence privacy.

RBAC/permission.

Admin override audit.

Secret/token exposure.

Public trace whitelist.

Notification privacy.

Messenger/private delivery boundary.

Live public comment safety.

Data retention nếu áp dụng.

## 11.3. Scope Out

Module không được:

Bỏ qua privacy để release nhanh.

Cho public dữ liệu private.

Cho evidence chứa secret.

Cho AI Advisor nhận dữ liệu vượt guard.

Cho Gateway public giá/thông tin riêng tư sai rule.

Cho IVR đọc full address/payment/member/Diamond/health note sai policy.

Cho recording/call log dùng marketing.

Cho admin override không audit.

Cho release khi security critical fail.

## 11.4. Upstream Dependency

Phụ thuộc:

TECH-01 RBAC/Audit/Evidence.

TECH-03 Public Trace/Sale Lock/Recall.

TECH-05 AI Final Response Guard.

TECH-06 Channel Privacy Boundary.

TECH-09 IVR Privacy Boundary.

Evidence package.

Security test results.

Privacy/legal review.

## 11.5. Downstream Handoff

Bàn giao:

Privacy pass/fail.

Security pass/fail.

Required fix list.

Risk acceptance nếu có.

Release blocking status.

Handoff sang Owner Sign-off Resolver và Release Decision Board.

## 11.6. P0 điểm chặn

FAIL nếu public dữ liệu private.

FAIL nếu evidence lộ secret.

FAIL nếu payment/member/Diamond/health note lộ sai policy.

FAIL nếu admin override không audit.

FAIL nếu privacy/security fail nhưng Release Approved.

## 11.7. Evidence

Evidence tối thiểu:

Privacy checklist.

Security checklist.

RBAC check.

Public/private boundary check.

Secret exposure check.

Evidence privacy check.

Reviewer.

Pass/fail.

Audit log.

## 11.8. Smoke

## TECH10-MOD-SMK-017

Given Gateway public comment scenario
When customer data/private price/order info xuất hiện
Then privacy review FAIL.

## TECH10-MOD-SMK-018

Given evidence package chứa secret/token
When Privacy/Security Reviewer kiểm tra
Then evidence rejected và release bị chặn.

## 12. MODULE CONTRACT 10 - PRODUCTION READINESS RESOLVER

## 12.1. Mục tiêu

Production Readiness Resolver xác định một scope có đủ điều kiện được xem xét production readiness hay chưa.

Module này không tự go-live.

Module này chỉ xác nhận readiness trước khi Release Decision Board quyết định.

## 12.2. Scope In

Module được phép kiểm tra:

Documentation complete.

Evidence accepted.

Smoke pass.

UAT pass nếu áp dụng.

Owner sign-off.

No open P0 điểm chặn.

Dependency pass.

Privacy/security pass.

Rollback/fallback ready.

Monitoring/alert ready.

Environment readiness.

Provider readiness nếu có.

Data migration/seed readiness nếu có.

Support/incident owner assignment.

Release scope clarity.

## 12.3. Scope Out

Module không được:

Tự quyết định go-live.

Bỏ qua release decision.

Bỏ qua rollback/fallback.

Bỏ qua monitoring.

Bỏ qua privacy/security.

Bỏ qua owner sign-off.

Bỏ qua dependency.

Dùng local/dev evidence để production ready.

Tuyên bố ready khi còn P0 điểm chặn.

Mở Global Gateway.

## 12.4. Upstream Dependency

Phụ thuộc:

Global Release Orchestrator.

Evidence Resolver.

Smoke Orchestrator.

UAT Resolver.

Owner Sign-off Resolver.

P0 điểm chặn Registry.

Cross-Tech Dependency Validator.

Privacy/Security Reviewer.

Rollback/Fallback Resolver.

Monitoring/Alert Resolver.

## 12.5. Downstream Handoff

Bàn giao:

Production readiness recommendation.

Readiness gap list.

Risk list.

Required condition list.

Handoff sang Release Decision Board.

## 12.6. P0 điểm chặn

## 12.7. Evidence

Evidence tối thiểu:

Readiness checklist.

Evidence status.

Smoke status.

UAT status.

Owner sign-off status.

Dependency status.

Privacy/security status.

Rollback/fallback status.

Monitoring status.

Recommendation.

Audit log.

## 12.8. Smoke

## TECH10-MOD-SMK-019

## TECH10-MOD-SMK-020

Given all readiness gates pass
When resolver kiểm tra
Then có thể recommend Production Ready, nhưng chưa Go-live Approved.

## 13. MODULE CONTRACT 11 - ROLLBACK / FALLBACK READINESS RESOLVER

## 13.1. Mục tiêu

Rollback / Fallback Readiness Resolver kiểm tra hệ thống có phương án dừng, giảm rủi ro hoặc quay lại trạng thái an toàn nếu release gặp lỗi.

## 13.2. Scope In

Module được phép kiểm tra:

Rollback scope.

Rollback trigger.

Fallback mode.

Manual operation fallback.

Disable switch.

Kill switch nếu áp dụng.

Data recovery plan.

Provider fallback nếu có.

Notification fallback.

Order/payment safety fallback.

Sale Lock/Recall emergency handling.

Incident owner.

Communication plan.

Post-rollback evidence.

## 13.3. Scope Out

Module không được:

Cho production release khi không có rollback/fallback.

Cho Ads scale khi không có stop-scale rule.

Cho IVR go-live khi không có disable/fallback.

Cho Gateway go-live khi không có rate/suppression fallback.

Cho Commerce payment/order flow release khi không có order/payment safety fallback.

Xóa dữ liệu production để rollback tùy tiện.

Rollback trái retention/audit policy.

## 13.4. Upstream Dependency

Phụ thuộc:

Release scope.

Domain risk level.

Incident policy.

Data retention policy.

Provider dependency.

Monitoring/Alert Resolver.

Owner assignment.

## 13.5. Downstream Handoff

Bàn giao:

Rollback ready status.

Fallback ready status.

Trigger list.

Owner list.

Action plan.

Risk note.

Handoff sang Production Readiness Resolver và Release Decision Board.

## 13.6. P0 điểm chặn

FAIL nếu production go-live không có rollback/fallback plan.

FAIL nếu payment/order release không có safety fallback.

FAIL nếu Ads scale không có stop-scale rule.

FAIL nếu IVR release không có stop-call/fallback.

FAIL nếu rollback plan phá audit/trace.

## 13.7. Evidence

Evidence tối thiểu:

Rollback plan.

Fallback plan.

Trigger criteria.

Owner.

Test evidence nếu có.

Data safety note.

Communication plan.

Approval.

Audit log.

## 13.8. Smoke

## TECH10-MOD-SMK-021

Given IVR provider lỗi tăng cao
When rollback/fallback check chạy
Then phải có stop-call/fallback route.

## TECH10-MOD-SMK-022

Given Ads scale scope
When fallback check chạy
Then phải có stop-scale/suppression rule.

## 14. MODULE CONTRACT 12 - MONITORING / ALERT READINESS RESOLVER

## 14.1. Mục tiêu

Monitoring / Alert Readiness Resolver kiểm tra hệ thống có đủ giám sát và cảnh báo trước khi release/go-live.

## 14.2. Scope In

Module được phép kiểm tra:

Health monitoring.

Error monitoring.

Smoke monitoring.

Order flow alert.

Payment alert.

Revenue verification alert.

Sale Lock/Recall alert.

AI response guard alert.

Gateway delivery alert.

Ads data quality alert.

MC AI Live suppression alert.

IVR provider failure alert.

Notification failure alert.

Evidence write failure alert.

Admin override anomaly alert.

Incident owner routing.

## 14.3. Scope Out

Module không được:

Cho go-live khi không có monitoring.

Cho go-live khi alert không có owner.

Cho Ads scale khi data quality alert chưa sẵn sàng.

Cho IVR go-live khi provider failure alert chưa sẵn sàng.

Cho Commerce go-live khi payment/order alert chưa sẵn sàng.

Cho Gateway go-live khi rate-limit/moderation alert chưa sẵn sàng.

Bỏ qua alert critical.

## 14.4. Upstream Dependency

Phụ thuộc:

Release scope.

Runtime components.

Domain owner.

Incident owner.

Alert policy.

Observability readiness.

Evidence/Audit system.

## 14.5. Downstream Handoff

Bàn giao:

Monitoring ready status.

Alert ready status.

Missing alert list.

Incident owner list.

Escalation route.

Handoff sang Production Readiness Resolver.

## 14.6. P0 điểm chặn

FAIL nếu release critical mà không có monitoring.

FAIL nếu alert critical không có owner.

FAIL nếu evidence write failure không được alert.

FAIL nếu payment/revenue mismatch không được alert.

FAIL nếu IVR technical failure tăng nhưng không alert.

## 14.7. Evidence

Evidence tối thiểu:

Monitoring checklist.

Alert checklist.

Owner assignment.

Escalation route.

Test alert evidence nếu có.

Gap list.

Approval.

Audit log.

## 14.8. Smoke

## TECH10-MOD-SMK-023

Given evidence write failure
When monitoring chạy
Then alert phải được ghi nhận và route owner.

## TECH10-MOD-SMK-024

Given payment confirmed nhưng verified revenue không cập nhật
When monitoring chạy
Then revenue mismatch alert phải phát sinh.

## 15. MODULE CONTRACT 13 - RELEASE DECISION BOARD

## 15.1. Mục tiêu

Release Decision Board là module quyết định cuối cùng cho release scope.

Module này không thay evidence, smoke, UAT hoặc owner sign-off.

Module này chỉ ra quyết định dựa trên toàn bộ gate đã được kiểm tra.

## 15.2. Scope In

Module được phép:

Nhận readiness recommendation.

Xem evidence status.

Xem smoke status.

Xem UAT status.

Xem owner sign-off status.

Xem điểm chặn status.

Xem dependency status.

Xem privacy/security status.

Xem rollback/fallback status.

Xem monitoring/alert status.

Chọn release decision.

Ghi release scope.

Ghi release conditions nếu có.

Ghi go-live decision.

Ghi rollback/fallback condition.

Bàn giao sang Global Gateway State Controller.

## 15.3. Scope Out

Module không được:

Bỏ qua P0 điểm chặn.

Tuyên bố go-live khi thiếu release scope.

Tuyên bố go-live khi thiếu rollback/fallback.

Tuyên bố go-live khi thiếu monitoring.

Tuyên bố go-live khi privacy/security fail.

Tự sửa evidence.

Tự sửa smoke result.

Tự ký thay owner.

Cho production nếu chỉ có local/dev evidence.

Cho scale nếu data quality fail.

## 15.4. Các quyết định hợp lệ

Release Decision Board chỉ được chọn một trong các quyết định:

## RELEASE_REJECTED.

## RELEASE_DECISION_ACCEPTED_FOR_INTERNAL_TEST.

## RELEASE_DECISION_ACCEPTED_FOR_STAGING_UAT.

## RELEASE_DECISION_ACCEPTED_FOR_LIMITED_PILOT.

## RELEASE_DECISION_ACCEPTED_FOR_PRODUCTION_GO_LIVE.

## RELEASE_DECISION_ACCEPTED_WITH_CONDITIONS.

## RELEASE_PAUSED.

## ROLLBACK_REQUIRED.

## 15.5. Upstream Dependency

Phụ thuộc:

Production Readiness Resolver.

Owner Sign-off Resolver.

P0 điểm chặn Registry.

Privacy/Security Reviewer.

Rollback/Fallback Resolver.

Monitoring/Alert Resolver.

Global Release Orchestrator.

## 15.6. Downstream Handoff

Bàn giao:

Release decision.

Release scope.

Release condition.

Go-live decision.

Monitoring window.

Incident owner.

Gateway state request.

Audit log.

## 15.7. P0 điểm chặn

FAIL nếu không có release decision mà Go-live Approved.

FAIL nếu release decision không có scope.

FAIL nếu release decision không có owner.

FAIL nếu release decision bỏ qua rollback/fallback.

FAIL nếu release decision bỏ qua monitoring.

## 15.8. Evidence

Evidence tối thiểu:

Release board record.

Readiness summary.

Decision type.

Scope.

Conditions.

Owner/approver.

Time.

Rollback/fallback reference.

Monitoring reference.

Audit log.

## 15.9. Smoke

## TECH10-MOD-SMK-025

## TECH10-MOD-SMK-026

Given release decision thiếu rollback/fallback reference
When board record được kiểm tra
Then release decision invalid.

## 16. MODULE CONTRACT 14 - GLOBAL GATEWAY STATE CONTROLLER

## 16.1. Mục tiêu

Global Gateway State Controller là module giữ trạng thái gateway tổng thể.

Module này quyết định hệ thống đang bị chặn, WAITING evidence, WAITING smoke, WAITING UAT, WAITING sign-off, release approved hay go-live approved.

## 16.2. Scope In

Module được phép:

Nhận input từ Global Release Orchestrator.

Nhận release decision.

Tính trạng thái Global Gateway.

Ghi gateway state.

Chặn mở gateway nếu thiếu điều kiện.

Chuyển trạng thái theo đúng state machine.

Ghi reason code.

Ghi audit.

Kích hoạt rollback state nếu có decision.

Kích hoạt post-go-live monitoring state nếu go-live approved.

## 16.3. Scope Out

Module không được:

Tự bỏ qua release decision.

Tự bỏ qua owner sign-off.

Tự bỏ qua evidence.

Tự bỏ qua smoke.

Tự bỏ qua P0 điểm chặn.

Tự mở production.

Tự cho scale Ads.

Tự cho IVR gọi thật.

Tự cho Gateway gửi public/private traffic.

Tự cho Commerce bán nếu upstream bị chặn.

## 16.4. Gateway State Machine

Global Gateway State Controller phải hỗ trợ tối thiểu các trạng thái:

bị chặn.

## DOCUMENTATION_COMPLETE_WAITING_EVIDENCE.

## EVIDENCE_SUBMITTED_WAITING_REVIEW.

## EVIDENCE_ACCEPTED_WAITING_SMOKE.

## SMOKE_RUNNING.

## SMOKE_PASS_WAITING_UAT.

## UAT_RUNNING.

## UAT_PASS_WAITING_OWNER_SIGNOFF.

## OWNER_SIGNED_WAITING_RELEASE_DECISION.

## RELEASE_DECISION_ACCEPTED_WAITING_GO_LIVE.

## GO_LIVE_DECISION_ACCEPTED.

## GO_LIVE_BLOCKED.

## ROLLBACK_REQUIRED.

## POST_GO_LIVE_MONITORING.

## SCALE_BLOCKED.

## SCALE_APPROVED.

## 16.5. Upstream Dependency

Phụ thuộc:

Global Release Orchestrator.

Evidence Resolver.

Smoke Orchestrator.

UAT Resolver.

Owner Sign-off Resolver.

Release Decision Board.

P0 điểm chặn Registry.

Cross-Tech Dependency Validator.

## 16.6. Downstream Handoff

Bàn giao:

Gateway state.

Gateway reason code.

Allowed action.

bị chặn action.

Required next action.

Audit log.

Handoff sang Post-Go-Live Monitoring nếu go-live.

## 16.7. P0 điểm chặn

FAIL nếu Global Gateway mặc định không phải bị chặn.

FAIL nếu Gateway mở khi thiếu evidence accepted.

FAIL nếu Gateway mở khi smoke chưa pass.

FAIL nếu Gateway mở khi thiếu release decision.

FAIL nếu Gateway mở khi còn P0 điểm chặn.

## 16.8. Evidence

Evidence tối thiểu:

Gateway previous state.

Gateway requested state.

Gate inputs.

Decision result.

Reason code.

Actor/system.

Timestamp.

Audit log.

## 16.9. Smoke

## TECH10-MOD-SMK-027

Given TECH documentation complete nhưng evidence chưa accepted
When Gateway State Controller chạy
Then Gateway = DOCUMENTATION_COMPLETE_WAITING_EVIDENCE hoặc bị chặn, không GO_LIVE_DECISION_ACCEPTED.

## TECH10-MOD-SMK-028

Given release decision approved nhưng P0 điểm chặn mở
When Gateway kiểm tra
Then Gateway = GO_LIVE_BLOCKED hoặc bị chặn.

## 17. MODULE CONTRACT 15 - POST-GO-LIVE MONITORING RESOLVER

## 17.1. Mục tiêu

Post-Go-Live Monitoring Resolver theo dõi hệ thống sau khi go-live hoặc limited pilot.

Module này đảm bảo go-live không phải kết thúc kiểm soát.

## 17.2. Scope In

Module được phép:

Theo dõi post-go-live window.

Theo dõi incident.

Theo dõi error rate.

Theo dõi payment/order mismatch.

Theo dõi AI response guard fail.

Theo dõi Gateway delivery fail.

Theo dõi Ads data quality.

Theo dõi IVR technical failure.

Theo dõi notification failure.

Theo dõi Sale Lock/Recall suppression.

Theo dõi override anomaly.

Theo dõi customer complaint spike.

Kích hoạt rollback/fallback nếu đạt trigger.

Báo cáo post-go-live status.

Đề xuất scale hoặc block scale.

## 17.3. Scope Out

Module không được:

Tự scale Ads.

Tự bỏ qua incident.

Tự tắt alert.

Tự bỏ qua privacy incident.

Tự bỏ qua payment/revenue mismatch.

Tự mở thêm scope ngoài release decision.

Tự chuyển Scale Approved khi Data Quality chưa pass.

Tự đóng post-go-live khi monitoring chưa đủ window.

## 17.4. Upstream Dependency

Phụ thuộc:

Go-live decision.

Monitoring/Alert Readiness.

Incident owner assignment.

Rollback/fallback plan.

Runtime metrics.

Evidence/audit stream.

Customer support feedback.

Owner review.

## 17.5. Downstream Handoff

Bàn giao:

Post-go-live status.

Incident list.

Alert summary.

Rollback recommendation.

Scale readiness recommendation.

Owner review request.

Audit log.

## 17.6. P0 điểm chặn

FAIL nếu go-live nhưng không monitoring.

FAIL nếu incident critical không route owner.

FAIL nếu payment/revenue mismatch không alert.

FAIL nếu AI/Gateway/IVR privacy incident không block scale.

FAIL nếu scale approved khi post-go-live chưa ổn định.

## 17.7. Evidence

Evidence tối thiểu:

Monitoring window.

Metric summary.

Alert log.

Incident log.

Owner response.

Rollback/fallback action nếu có.

Scale recommendation.

Audit log.

## 17.8. Smoke

## TECH10-MOD-SMK-029

Given post-go-live có critical incident
When monitoring resolver chạy
Then phải route incident owner và block scale.

## TECH10-MOD-SMK-030

Given post-go-live ổn định nhưng Ads data quality fail
When scale readiness được xét
Then Scale Approved = KHONG.

## 18. CROSS-MODULE DEPENDENCY MAP

## 18.1. Luồng chuẩn release

Luồng chuẩn trong TECH-10:

Documentation Completion Registry xác nhận tài liệu đủ cấu trúc.

Evidence Intake & Acceptance Resolver tiếp nhận evidence.

Global Smoke Matrix Orchestrator chạy smoke.

UAT Session Resolver tổ chức UAT nếu scope yêu cầu.

P0 điểm chặn Registry theo dõi điểm chặn.

Cross-Tech Dependency Validator kiểm tra dependency.

Privacy/Security Release Reviewer kiểm tra dữ liệu/quyền/an toàn.

Owner Sign-off Resolver kiểm tra owner sign-off.

Rollback/Fallback Readiness Resolver kiểm tra phương án an toàn.

Monitoring/Alert Readiness Resolver kiểm tra giám sát.

Production Readiness Resolver tổng hợp readiness.

Release Decision Board ra quyết định.

Global Gateway State Controller cập nhật gateway.

Post-Go-Live Monitoring Resolver theo dõi sau release.

## 18.2. No-bypass dependency

Không module nào được bỏ qua:

Evidence accepted.

Smoke pass.

P0 điểm chặn closed.

Cross-tech dependency pass.

Privacy/security pass.

Owner sign-off.

Rollback/fallback ready.

Monitoring/alert ready.

Release decision.

Global Gateway state.

## 18.3. Dependency ưu tiên

Nếu có mâu thuẫn:

P0 điểm chặn thắng mọi trạng thái.

Privacy/Security fail thắng release desire.

Cross-tech dependency fail thắng domain pass.

Smoke fail thắng owner muốn release.

Evidence missing thắng dev claim completed.

Release Decision Board thắng readiness recommendation.

Global Gateway State Controller là trạng thái cuối.

## 19. GLOBAL MODULE P0 điểm chặn REGISTRY - PHẦN 2/4

Các lỗi sau là P0 điểm chặn cấp module:

Global Release Orchestrator tự mở Gateway.

Documentation Completion Registry gọi tài liệu là Production Ready.

Evidence Resolver accepted evidence không gắn requirement.

Evidence Resolver accepted evidence chưa rõ environment.

Smoke Orchestrator đánh pass khi chưa chạy test.

Smoke Orchestrator bỏ qua fail-case.

UAT Resolver cho dev tự UAT thay owner.

Owner Sign-off Resolver cho sign-off không evidence.

P0 điểm chặn Resolver đóng điểm chặn không retest.

Cross-Tech Dependency Validator cho downstream pass khi upstream fail.

Privacy/Security Reviewer bỏ qua lộ dữ liệu riêng tư.

Production Readiness Resolver cho ready khi rollback chưa có.

Rollback/Fallback Resolver cho release không có rollback.

Monitoring Resolver cho go-live không có alert owner.

Release Decision Board ra quyết định không scope.

Global Gateway State Controller mở gateway khi gate thiếu.

Post-Go-Live Monitoring Resolver bỏ qua incident critical.

Module nào dùng lời nói thay evidence.

Module nào dùng local evidence để production-ready.

Module nào bỏ qua Global Gateway bị chặn.

## 20. EVIDENCE PACKAGE - PHẦN 2/4

## PHẦN 2/4 yêu cầu evidence thiết kế ở cấp module gồm:

Global Release Orchestrator contract accepted.

Documentation Completion Registry contract accepted.

Evidence Intake & Acceptance Resolver contract accepted.

Global Smoke Matrix Orchestrator contract accepted.

UAT Session Resolver contract accepted.

Owner Sign-off Resolver contract accepted.

P0 điểm chặn Registry & Resolution Resolver contract accepted.

Cross-Tech Dependency Validator contract accepted.

Privacy/Security Release Reviewer contract accepted.

Production Readiness Resolver contract accepted.

Rollback/Fallback Readiness Resolver contract accepted.

Monitoring/Alert Readiness Resolver contract accepted.

Release Decision Board contract accepted.

Global Gateway State Controller contract accepted.

Post-Go-Live Monitoring Resolver contract accepted.

Cross-module dependency accepted.

Global P0 điểm chặn Registry accepted.

Owner review checklist prepared.

Release handoff checklist prepared.

## 21. SMOKE MATRIX ĐỊNH HƯỚNG - PHẦN 2/4

TECH10-P2-SMK-001 - Documentation Không Tự Production Ready

Given TECH đã viết đủ 4 phần
When Documentation Completion Registry cập nhật
Then status chỉ là Documentation Complete, không Production Ready.

TECH10-P2-SMK-002 - Evidence Chưa Accepted

Given evidence đã nộp nhưng chưa review
When Global Release Orchestrator kiểm tra
Then Completion Decision = KHONG.

TECH10-P2-SMK-003 - Smoke Fail Chặn Release Ready

TECH10-P2-SMK-004 - UAT Không Owner

Given UAT do dev tự xác nhận
When UAT Session Resolver kiểm tra
Then UAT invalid.

TECH10-P2-SMK-005 - Thiếu Owner Sign-off

TECH10-P2-SMK-006 - P0 điểm chặn Còn Mở

Given điểm chặn P0 còn OPEN
When Global Gateway State Controller kiểm tra
Then Gateway = bị chặn.

TECH10-P2-SMK-007 - Upstream Fail Chặn Downstream

Given Operational Sale Lock fail
When Commerce/AI/Ads/Live/IVR readiness được xét
Then downstream bị chặn.

TECH10-P2-SMK-008 - Privacy Fail Chặn Release

Given evidence hoặc response lộ dữ liệu private
When Privacy/Security Reviewer chạy
Then release bị chặn.

TECH10-P2-SMK-009 - Rollback Missing

TECH10-P2-SMK-010 - Monitoring Missing

TECH10-P2-SMK-011 - Release Decision Missing

TECH10-P2-SMK-012 - Post-Go-Live Incident

Given critical incident sau go-live
When Post-Go-Live Monitoring chạy
Then route owner, block scale, xét rollback/fallback.

## 22. DONE GATE - PHẦN 2/4

## PHẦN 2/4 đạt Documentation Complete ở cấp module contract khi:

Đã khóa Global Release Orchestrator.

Đã khóa Documentation Completion Registry.

Đã khóa Evidence Intake & Acceptance Resolver.

Đã khóa Global Smoke Matrix Orchestrator.

Đã khóa UAT Session Resolver.

Đã khóa Owner Sign-off Resolver.

Đã khóa P0 điểm chặn Registry & Resolution Resolver.

Đã khóa Cross-Tech Dependency Validator.

Đã khóa Privacy/Security Release Reviewer.

Đã khóa Production Readiness Resolver.

Đã khóa Rollback/Fallback Readiness Resolver.

Đã khóa Monitoring/Alert Readiness Resolver.

Đã khóa Release Decision Board.

Đã khóa Global Gateway State Controller.

Đã khóa Post-Go-Live Monitoring Resolver.

Mỗi module có Scope In.

Mỗi module có Scope Out.

Mỗi module có Upstream Dependency.

Mỗi module có Downstream Handoff.

Mỗi module có P0 điểm chặn.

Mỗi module có Evidence.

Mỗi module có Smoke.

Có Cross-Module Dependency Map.

Có P0 điểm chặn Registry cấp phần.

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

Có module được phép tự mở Gateway.

Có module được phép gọi Documentation Complete là Production Ready.

Có module được phép accepted evidence không trace requirement.

Có module được phép pass smoke khi chưa chạy.

Có module được phép owner sign-off không evidence.

Có module được phép đóng P0 điểm chặn không retest.

Có module được phép bỏ qua cross-tech dependency.

Có module được phép bỏ qua privacy/security.

Có module được phép release khi thiếu rollback/fallback.

Có module được phép go-live khi thiếu monitoring.

Có module được phép Go-live Approved khi thiếu release decision.

Có module được phép scale khi Global Gateway chưa pass.

## 24. RELEASE HANDOFF - SANG PHẦN 3/4

## PHẦN 2/4 bàn giao sang PHẦN 3/4 các nội dung đã khóa:

Danh sách module release governance đầy đủ.

Boundary từng module.

Dependency từng module.

Handoff từng module.

P0 điểm chặn từng module.

Evidence từng module.

Smoke từng module.

Cross-module dependency map.

Global Gateway vẫn bị chặn.

Documentation Complete không phải Production Ready.

Evidence accepted mới xét Completion Decision.

Smoke pass mới xét Release Ready.

UAT pass nếu scope yêu cầu.

Owner sign-off mới xét Release Approved.

Release decision mới xét Go-live Approved.

Rollback/fallback và monitoring là điều kiện production release.

Post-go-live monitoring là bắt buộc sau khi go-live.

## PHẦN 3/4 phải tiếp tục viết:

## GLOBAL RELEASE LIFECYCLE / GATEWAY STATE MACHINE / EVIDENCE-TO-SMOKE-TO-UAT-TO-RELEASE FLOW / CROSS-TECH VALIDATION

Trong đó phải khóa rõ:

Documentation state.

Evidence state.

Smoke state.

UAT state.

Owner sign-off state.

điểm chặn state.

Dependency validation state.

Production readiness state.

Release decision state.

Global Gateway state.

Post-go-live monitoring state.

Scale approval state.

Rollback/fallback state.

Fail-safe release control.

## 25. TRẠNG THÁI SAU PHẦN 2/4

Sau PHẦN 2/4:

## PHẦN 1/4 đã khóa nguyên tắc release governance.

## PHẦN 2/4 đã khóa module contracts.

Chưa được gọi là Documentation Complete.

Chưa được gọi là Production Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Go-live Approved.

Chưa được gọi là Global Gateway PASS.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 đã khóa đầy đủ module contracts cho TECH-10.

Các module trọng yếu đã được cố định:

Global Release Orchestrator điều phối release, không tự mở Gateway.

Documentation Completion Registry chỉ xác nhận tài liệu đủ cấu trúc, không xác nhận production ready.

Evidence Intake & Acceptance Resolver không cho dùng lời nói thay evidence.

Global Smoke Matrix Orchestrator bắt buộc smoke cả pass-case, fail-case và cross-tech.

UAT Session Resolver yêu cầu owner nghiệp vụ xác nhận.

Owner Sign-off Resolver chặn Release Approved nếu thiếu sign-off.

P0 điểm chặn Registry chặn release khi điểm chặn còn mở.

Cross-Tech Dependency Validator chặn downstream nếu upstream chưa pass.

Privacy/Security Release Reviewer chặn release nếu dữ liệu/quyền/an toàn chưa đạt.

Production Readiness Resolver chỉ recommend readiness, không tự go-live.

Rollback/Fallback Readiness Resolver bắt buộc có phương án an toàn.

Monitoring/Alert Readiness Resolver bắt buộc có cảnh báo và incident owner.

Release Decision Board ra quyết định chính thức.

Global Gateway State Controller giữ trạng thái cuối, mặc định bị chặn.

Post-Go-Live Monitoring Resolver theo dõi sau go-live và chặn scale nếu có incident.

## PHẦN 2/4 sẵn sàng bàn giao sang:

## PHẦN 3/4 - GLOBAL RELEASE LIFECYCLE / GATEWAY STATE MACHINE / EVIDENCE-TO-SMOKE-TO-UAT-TO-RELEASE FLOW / CROSS-TECH VALIDATION.

## PHẦN 3/4 - GLOBAL RELEASE LIFECYCLE / GATEWAY STATE MACHINE / EVIDENCE-TO-SMOKE-TO-UAT-TO-RELEASE FLOW / CROSS-TECH VALIDATION

## 1. MỤC TIÊU PHẦN 3/4

## PHẦN 3/4 khóa vòng đời release tổng thể của hệ thống Ginsengfood.

PHẦN này xác định rõ:

Một TECH đi từ tài liệu hoàn chỉnh đến release như thế nào.

Evidence được nộp, review, accepted hoặc rejected như thế nào.

Smoke được chạy, pass, fail, retest như thế nào.

UAT được tổ chức và xác nhận như thế nào.

Owner sign-off được ghi nhận ra sao.

P0 điểm chặn ảnh hưởng đến release thế nào.

Cross-tech dependency được kiểm tra thế nào.

Production readiness được xét thế nào.

Release decision được ra quyết định thế nào.

Global Gateway chuyển trạng thái thế nào.

Post-go-live monitoring kiểm soát sau release ra sao.

Khi nào được scale, khi nào phải block scale.

Khi nào phải rollback/fallback.

Khi nào Global Gateway vẫn phải bị chặn.

## PHẦN 3/4 không viết code.

## PHẦN 3/4 không thiết kế API chi tiết.

## PHẦN 3/4 không thiết kế database chi tiết.

## PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa lifecycle, state machine, transition, điểm chặn và release control.

## 2. NGUYÊN TẮC LIFECYCLE CHUNG

Toàn bộ Global Release Lifecycle phải tuân thủ các nguyên tắc sau:

Documentation Complete không phải Production Ready.

Dev Completed không phải Release Ready.

Evidence Submitted không phải Evidence Accepted.

Smoke Running không phải Smoke Pass.

Smoke Pass cục bộ không phải Global Smoke Pass.

UAT Running không phải UAT Pass.

Owner Reviewed không phải Owner Signed-off.

Production Readiness Recommendation không phải Go-live Approved.

Release Approved không phải Scale Approved.

Go-live Approved không phải bỏ qua Post-Go-Live Monitoring.

P0 điểm chặn còn mở thì Global Gateway vẫn bị chặn.

Cross-tech dependency fail thì downstream bị chặn.

Privacy/Security fail thì release bị chặn.

Rollback/Fallback missing thì production release bị chặn.

Monitoring/Alert missing thì go-live bị chặn.

Không có release decision thì không Go-live Approved.

Không có accepted evidence thì không Completion Decision.

Không có smoke pass thì không Release Ready.

Không có owner sign-off thì không Release Approved.

Không có Global Gateway approval thì không được mở production traffic.

## 3. GLOBAL RELEASE LIFECYCLE TỔNG THỂ

## 3.1. Luồng chuẩn

Global Release Lifecycle chuẩn gồm các bước:

TECH được viết đủ theo cấu trúc.

Documentation Completion Registry kiểm tra tài liệu.

Nếu đủ, trạng thái chuyển sang DOCUMENTATION_COMPLETE.

Evidence được chuẩn bị và nộp.

Evidence Intake & Acceptance Resolver review evidence.

Nếu evidence accepted, chuyển sang Smoke.

Global Smoke Matrix Orchestrator chạy smoke.

Nếu smoke pass, chuyển sang UAT nếu scope yêu cầu.

UAT Session Resolver tổ chức UAT với owner nghiệp vụ.

Nếu UAT pass, chuyển sang Owner Sign-off.

Owner Sign-off Resolver kiểm tra sign-off.

P0 điểm chặn Registry kiểm tra điểm chặn.

Cross-Tech Dependency Validator kiểm tra dependency.

Privacy/Security Release Reviewer kiểm tra dữ liệu/quyền/an toàn.

Rollback/Fallback Readiness Resolver kiểm tra phương án an toàn.

Monitoring/Alert Readiness Resolver kiểm tra giám sát.

Production Readiness Resolver tổng hợp readiness.

Release Decision Board ra quyết định.

Global Gateway State Controller cập nhật trạng thái.

Nếu Go-live Approved, chuyển sang Post-Go-Live Monitoring.

Sau monitoring ổn định mới xét Scale Approval nếu scope có scale.

## 3.2. Luồng cấm

Các luồng sau bị cấm:

Documentation Complete -> Production Ready.

Dev Completed -> Release Approved.

Evidence Submitted -> Completion Decision.

Smoke not run -> Release Ready.

Smoke local pass -> Global Pass.

UAT do dev tự xác nhận -> UAT Pass.

Owner chưa ký -> Release Approved.

Release decision thiếu -> Go-live Approved.

P0 điểm chặn open -> Global Gateway Open.

Privacy fail -> Production Go-live.

Rollback missing -> Production Go-live.

Monitoring missing -> Production Go-live.

Ads ROAS đẹp -> Scale Approved nếu Verified Revenue/Data Quality chưa pass.

IVR gọi được -> IVR Ready nếu Notification/Core Order State chưa pass.

Gateway gửi được tin -> Channel Ready nếu privacy/rate/moderation chưa pass.

## 4. DOCUMENTATION STATE MODEL

## 4.1. Danh sách trạng thái

Documentation có các trạng thái:

DOC_NOT_STARTED
Chưa viết tài liệu.

DOC_IN_PROGRESS
Đang viết tài liệu.

DOC_SECTION_COMPLETE
Một phần tài liệu đã xong.

DOC_WAITING_REVIEW
Tài liệu chờ review.

DOC_REVIEW_FAILED
Tài liệu thiếu scope/boundary/dependency/gate hoặc có mâu thuẫn.

DOC_REVISION_REQUIRED
Cần sửa tài liệu.

DOC_DOCUMENTATION_COMPLETE
Tài liệu hoàn chỉnh theo cấu trúc.

DOC_ARCHIVED_OR_SUPERSEDED
Tài liệu cũ bị thay thế, chỉ còn giá trị tham chiếu.

## 4.2. Transition hợp lệ

## DOC_NOT_STARTED -> DOC_IN_PROGRESS.

## DOC_IN_PROGRESS -> DOC_SECTION_COMPLETE.

## DOC_SECTION_COMPLETE -> DOC_WAITING_REVIEW.

## DOC_WAITING_REVIEW -> DOC_REVIEW_FAILED.

## DOC_REVIEW_FAILED -> DOC_REVISION_REQUIRED.

## DOC_REVISION_REQUIRED -> DOC_IN_PROGRESS.

## DOC_WAITING_REVIEW -> DOC_DOCUMENTATION_COMPLETE.

DOC_DOCUMENTATION_COMPLETE -> DOC_ARCHIVED_OR_SUPERSEDED nếu có bản source-of-truth mới.

## 4.3. Transition bị cấm

## DOC_DOCUMENTATION_COMPLETE -> PRODUCTION_READINESS.

## DOC_DOCUMENTATION_COMPLETE -> GO_LIVE_DECISION_ACCEPTED.

## DOC_SECTION_COMPLETE -> RELEASE_READY.

## DOC_REVIEW_FAILED -> EVIDENCE_ACCEPTED.

DOC_ARCHIVED_OR_SUPERSEDED -> SOURCE_OF_TRUTH nếu chưa có quyết định khôi phục chính thức.

## 4.4. P0 điểm chặn

FAIL nếu tài liệu thiếu Done Gate nhưng vẫn Documentation Complete.

FAIL nếu tài liệu thiếu Fail Gate nhưng vẫn Documentation Complete.

FAIL nếu tài liệu thiếu P0 điểm chặn nhưng vẫn Documentation Complete.

FAIL nếu Documentation Complete được dùng để mở production traffic.

## 5. EVIDENCE STATE MODEL

## 5.1. Danh sách trạng thái

Evidence có các trạng thái:

EVIDENCE_NOT_REQUIRED_YET
Chưa đến bước nộp evidence.

EVIDENCE_REQUIRED
Đã cần evidence.

EVIDENCE_PREPARING
Dev/QA/owner đang chuẩn bị evidence.

EVIDENCE_SUBMITTED
Evidence đã được nộp.

EVIDENCE_UNDER_REVIEW
Evidence đang được review.

EVIDENCE_NEED_MORE_INFO
Evidence thiếu thông tin.

EVIDENCE_REJECTED
Evidence bị từ chối.

EVIDENCE_ACCEPTED
Evidence được chấp nhận.

EVIDENCE_REVOKED
Evidence bị thu hồi do sai, lỗi, môi trường không đúng hoặc dữ liệu vi phạm.

## 5.2. Transition hợp lệ

## EVIDENCE_REQUIRED -> EVIDENCE_PREPARING.

## EVIDENCE_PREPARING -> EVIDENCE_SUBMITTED.

## EVIDENCE_SUBMITTED -> EVIDENCE_UNDER_REVIEW.

## EVIDENCE_UNDER_REVIEW -> EVIDENCE_NEED_MORE_INFO.

## EVIDENCE_NEED_MORE_INFO -> EVIDENCE_SUBMITTED.

## EVIDENCE_UNDER_REVIEW -> EVIDENCE_REJECTED.

## EVIDENCE_REJECTED -> EVIDENCE_PREPARING.

## EVIDENCE_UNDER_REVIEW -> EVIDENCE_ACCEPTED.

EVIDENCE_ACCEPTED -> EVIDENCE_REVOKED nếu phát hiện evidence sai.

## 5.3. Transition bị cấm

EVIDENCE_SUBMITTED -> EVIDENCE_ACCEPTED nếu chưa review.

## EVIDENCE_NEED_MORE_INFO -> RELEASE_READY.

## EVIDENCE_REJECTED -> COMPLETION_PASS.

EVIDENCE_ACCEPTED từ local/dev evidence cho production scope nếu policy không cho phép.

EVIDENCE_ACCEPTED nếu evidence chứa secret/dữ liệu nhạy cảm sai policy.

## 5.4. Evidence acceptance rule

Evidence chỉ ACCEPTED khi có đủ:

Requirement reference.

Smoke/test reference nếu có.

Environment rõ.

Input rõ.

Expected result rõ.

Actual result rõ.

Pass/fail rõ.

Timestamp.

Reviewer.

Trace/audit/correlation id nếu là runtime flow.

Không chứa dữ liệu nhạy cảm sai policy.

Không có P0 conflict.

## 5.5. P0 điểm chặn

FAIL nếu dùng lời nói thay evidence.

FAIL nếu evidence chưa accepted nhưng Completion Decision.

FAIL nếu evidence local dùng để tuyên bố production ready.

FAIL nếu evidence không gắn requirement/smoke.

FAIL nếu evidence lộ secret, token, thông tin riêng tư sai policy.

## 6. SMOKE STATE MODEL

## 6.1. Danh sách trạng thái

Smoke có các trạng thái:

SMOKE_NOT_DEFINED
Chưa có smoke case.

SMOKE_DEFINED
Smoke case đã có.

SMOKE_READY_TO_RUN
Đủ điều kiện chạy smoke.

SMOKE_RUNNING
Đang chạy smoke.

SMOKE_PASS
Smoke pass.

SMOKE_FAIL
Smoke fail.

SMOKE_BLOCKED
Không chạy được do environment/dependency/evidence thiếu.

SMOKE_RETEST_REQUIRED
Cần retest sau fix.

SMOKE_RETEST_PASS
Retest pass.

SMOKE_RETEST_FAIL
Retest fail.

## 6.2. Transition hợp lệ

## SMOKE_NOT_DEFINED -> SMOKE_DEFINED.

## SMOKE_DEFINED -> SMOKE_READY_TO_RUN.

## SMOKE_READY_TO_RUN -> SMOKE_RUNNING.

## SMOKE_RUNNING -> SMOKE_PASS.

## SMOKE_RUNNING -> SMOKE_FAIL.

## SMOKE_RUNNING -> SMOKE_BLOCKED.

## SMOKE_FAIL -> SMOKE_RETEST_REQUIRED.

## SMOKE_RETEST_REQUIRED -> SMOKE_RUNNING.

## SMOKE_RUNNING -> SMOKE_RETEST_PASS.

## SMOKE_RUNNING -> SMOKE_RETEST_FAIL.

## 6.3. Transition bị cấm

## SMOKE_NOT_DEFINED -> RELEASE_READY.

## SMOKE_RUNNING -> RELEASE_READY.

## SMOKE_FAIL -> RELEASE_READY.

## SMOKE_BLOCKED -> RELEASE_READY.

SMOKE_PASS cục bộ -> GLOBAL_SMOKE_PASS nếu cross-tech smoke chưa pass.

SMOKE_PASS thiếu evidence -> RELEASE_READY.

## 6.4. Smoke pass rule

Smoke chỉ PASS khi có:

Smoke ID.

Scenario rõ.

Given/When/Then rõ.

Expected result.

Actual result.

Evidence.

Tester.

Reviewer nếu required.

Environment rõ.

Không vi phạm P0.

## 6.5. P0 điểm chặn

FAIL nếu smoke chỉ test happy path.

FAIL nếu không test fail-safe.

FAIL nếu smoke pass nhưng thiếu evidence.

FAIL nếu smoke fail nhưng vẫn Release Ready.

FAIL nếu cross-tech smoke chưa chạy mà Global Pass.

## 7. UAT STATE MODEL

## 7.1. Danh sách trạng thái

UAT có các trạng thái:

UAT_NOT_REQUIRED
Scope không cần UAT riêng.

UAT_REQUIRED
Scope cần UAT.

UAT_PLANNING
Đang lập kế hoạch UAT.

UAT_READY
Đủ điều kiện UAT.

UAT_RUNNING
Đang UAT.

UAT_FEEDBACK_WAITING
Đang chờ phản hồi.

UAT_FAILED
UAT fail.

UAT_FIX_REQUIRED
Cần sửa.

UAT_RETEST_REQUIRED
Cần UAT lại.

UAT_PASSED
UAT pass.

UAT_ACCEPTED_BY_OWNER
Owner nghiệp vụ xác nhận UAT.

## 7.2. Transition hợp lệ

## UAT_REQUIRED -> UAT_PLANNING.

## UAT_PLANNING -> UAT_READY.

## UAT_READY -> UAT_RUNNING.

## UAT_RUNNING -> UAT_FEEDBACK_WAITING.

## UAT_FEEDBACK_WAITING -> UAT_FAILED.

## UAT_FAILED -> UAT_FIX_REQUIRED.

## UAT_FIX_REQUIRED -> UAT_RETEST_REQUIRED.

## UAT_RETEST_REQUIRED -> UAT_RUNNING.

## UAT_FEEDBACK_WAITING -> UAT_PASSED.

## UAT_PASSED -> UAT_ACCEPTED_BY_OWNER.

## 7.3. Transition bị cấm

Dev tự test -> UAT_ACCEPTED_BY_OWNER.

Smoke pass -> UAT_PASSED nếu chưa UAT.

## UAT_FAILED -> RELEASE_DECISION_ACCEPTED.

## UAT_FEEDBACK_WAITING -> RELEASE_DECISION_ACCEPTED.

UAT thiếu owner -> UAT_ACCEPTED_BY_OWNER.

## 7.4. UAT pass rule

UAT chỉ pass khi:

Có UAT scenario.

Có owner nghiệp vụ hoặc người được ủy quyền.

Có environment rõ.

Có expected/actual.

Có feedback.

Có pass/fail.

Không còn P0 UAT điểm chặn.

Owner xác nhận acceptance nếu scope yêu cầu.

## 7.5. P0 điểm chặn

FAIL nếu dev tự xác nhận UAT.

FAIL nếu UAT bỏ qua nghiệp vụ thực tế.

FAIL nếu UAT không test fail-case.

FAIL nếu UAT pass khi P0 điểm chặn còn mở.

## 8. OWNER SIGN-OFF STATE MODEL

## 8.1. Danh sách trạng thái

Owner Sign-off có các trạng thái:

SIGNOFF_NOT_REQUIRED
Scope không cần owner này.

SIGNOFF_REQUIRED
Owner này bắt buộc ký.

SIGNOFF_WAITING_REVIEW
Đang chờ owner review.

SIGNOFF_REVIEWING
Owner đang review.

SIGNOFF_REJECTED
Owner từ chối.

SIGNOFF_CONDITIONED
Owner ký có điều kiện.

SIGNOFF_APPROVED
Owner ký duyệt.

SIGNOFF_REVOKED
Sign-off bị thu hồi do phát hiện lỗi.

## 8.2. Transition hợp lệ

## SIGNOFF_REQUIRED -> SIGNOFF_WAITING_REVIEW.

## SIGNOFF_WAITING_REVIEW -> SIGNOFF_REVIEWING.

## SIGNOFF_REVIEWING -> SIGNOFF_REJECTED.

## SIGNOFF_REVIEWING -> SIGNOFF_CONDITIONED.

## SIGNOFF_REVIEWING -> SIGNOFF_APPROVED.

SIGNOFF_APPROVED -> SIGNOFF_REVOKED nếu có lỗi P0 mới.

SIGNOFF_CONDITIONED -> SIGNOFF_APPROVED nếu điều kiện đã xử lý.

## 8.3. Transition bị cấm

## SIGNOFF_WAITING_REVIEW -> RELEASE_DECISION_ACCEPTED.

## SIGNOFF_REJECTED -> RELEASE_DECISION_ACCEPTED.

SIGNOFF_CONDITIONED -> GO_LIVE_DECISION_ACCEPTED nếu điều kiện là P0 chưa xử lý.

SIGNOFF_APPROVED không gắn evidence -> RELEASE_DECISION_ACCEPTED.

Thiếu owner critical -> RELEASE_DECISION_ACCEPTED.

## 8.4. Sign-off rule

Owner sign-off hợp lệ phải có:

Owner identity.

Domain.

Scope.

Evidence đã review.

Smoke đã review.

UAT đã review nếu có.

Open điểm chặn status.

Decision.

Timestamp.

Audit.

## 8.5. P0 điểm chặn

FAIL nếu thiếu owner sign-off mà Release Approved.

FAIL nếu sign-off không có evidence.

FAIL nếu sign-off không rõ scope.

FAIL nếu sign-off khi P0 điểm chặn còn mở.

## 9. P0 điểm chặn STATE MODEL

## 9.1. Danh sách trạng thái

P0 điểm chặn có các trạng thái:

STOP_POINT_NOT_REPORTED
Chưa ghi nhận.

STOP_POINT_OPEN
Đã ghi nhận, đang mở.

STOP_POINT_ASSIGNED
Đã gán owner xử lý.

STOP_POINT_FIX_IN_PROGRESS
Đang sửa.

STOP_POINT_FIX_SUBMITTED
Đã nộp fix.

STOP_POINT_RETEST_REQUIRED
Cần retest.

STOP_POINT_RETEST_RUNNING
Đang retest.

STOP_POINT_RETEST_FAILED
Retest fail.

STOP_POINT_RESOLVED_WAITING_ACCEPTANCE
Chờ owner/QA accepted.

STOP_POINT_CLOSED
Đã đóng hợp lệ.

STOP_POINT_REOPENED
Mở lại do lỗi tái xuất hiện.

STOP_POINT_WAIVER_REQUESTED
Xin ngoại lệ.

STOP_POINT_WAIVER_REJECTED
Ngoại lệ bị từ chối.

STOP_POINT_WAIVER_APPROVED_WITH_CONDITIONS
Ngoại lệ được duyệt có điều kiện.

## 9.2. Transition hợp lệ

## STOP_POINT_OPEN -> STOP_POINT_ASSIGNED.

## STOP_POINT_ASSIGNED -> STOP_POINT_FIX_IN_PROGRESS.

## STOP_POINT_FIX_IN_PROGRESS -> STOP_POINT_FIX_SUBMITTED.

## STOP_POINT_FIX_SUBMITTED -> STOP_POINT_RETEST_REQUIRED.

## STOP_POINT_RETEST_REQUIRED -> STOP_POINT_RETEST_RUNNING.

## STOP_POINT_RETEST_RUNNING -> STOP_POINT_RETEST_FAILED.

## STOP_POINT_RETEST_FAILED -> STOP_POINT_FIX_IN_PROGRESS.

## STOP_POINT_RETEST_RUNNING -> STOP_POINT_RESOLVED_WAITING_ACCEPTANCE.

## STOP_POINT_RESOLVED_WAITING_ACCEPTANCE -> STOP_POINT_CLOSED.

STOP_POINT_CLOSED -> STOP_POINT_REOPENED nếu lỗi tái xuất hiện.

## STOP_POINT_OPEN -> STOP_POINT_WAIVER_REQUESTED.

## STOP_POINT_WAIVER_REQUESTED -> STOP_POINT_WAIVER_REJECTED.

## STOP_POINT_WAIVER_REQUESTED -> STOP_POINT_WAIVER_APPROVED_WITH_CONDITIONS.

## 9.3. Transition bị cấm

## STOP_POINT_OPEN -> RELEASE_READY.

STOP_POINT_FIX_SUBMITTED -> STOP_POINT_CLOSED nếu chưa retest.

## STOP_POINT_RETEST_FAILED -> RELEASE_DECISION_ACCEPTED.

## STOP_POINT_WAIVER_REQUESTED -> GO_LIVE_DECISION_ACCEPTED.

STOP_POINT_WAIVER_APPROVED_WITH_CONDITIONS -> GO_LIVE_DECISION_ACCEPTED nếu điều kiện chưa nằm trong release decision.

STOP_POINT_CLOSED không evidence -> Release Ready.

## 9.4. điểm chặn closure rule

P0 điểm chặn chỉ được đóng khi có:

Fix evidence.

Retest evidence.

Owner/QA acceptance.

No regression evidence nếu cần.

Audit log.

Updated smoke status.

Updated release status.

## 9.5. P0 điểm chặn

FAIL nếu điểm chặn đóng bằng miệng.

FAIL nếu điểm chặn đóng không retest.

FAIL nếu điểm chặn bị ẩn khỏi release gate.

FAIL nếu open điểm chặn nhưng Gateway mở.

## 10. CROSS-TECH DEPENDENCY STATE MODEL

## 10.1. Danh sách trạng thái

Cross-Tech Dependency có các trạng thái:

## DEPENDENCY_NOT_CHECKED.

## DEPENDENCY_CHECKING.

## DEPENDENCY_PASS.

## DEPENDENCY_FAIL.

## DEPENDENCY_BLOCKED_BY_UPSTREAM.

## DEPENDENCY_BLOCKED_BY_DOWNSTREAM.

## DEPENDENCY_NEED_OWNER_REVIEW.

## DEPENDENCY_WAIVER_REQUESTED.

## DEPENDENCY_WAIVER_REJECTED.

## DEPENDENCY_WAIVER_APPROVED_WITH_CONDITIONS.

## 10.2. Dependency chain bắt buộc

Các dependency tối thiểu phải kiểm tra:

TECH-01 Evidence/Audit -> mọi high-risk command.

TECH-02 Product/SKU/Recipe -> TECH-03 Operational.

TECH-03 Operational Sellable/Sale Lock/Recall -> TECH-04 Commerce.

TECH-04 QuoteSnapshot/Order/Payment -> TECH-05 AI Advisor.

TECH-05 Final Response Guard -> TECH-06 Facebook Gateway.

TECH-06 Gateway -> TECH-08 MC AI Live.

TECH-04 Verified Revenue -> TECH-07 Ads.

TECH-04 Official Order/Core Order State -> TECH-09 IVR.

TECH-09 IVR outcome -> TECH-04 Core Order State / Notification Owner.

TECH-10 Global Gateway -> mọi production release.

## 10.3. Transition hợp lệ

## DEPENDENCY_NOT_CHECKED -> DEPENDENCY_CHECKING.

## DEPENDENCY_CHECKING -> DEPENDENCY_PASS.

## DEPENDENCY_CHECKING -> DEPENDENCY_FAIL.

## DEPENDENCY_CHECKING -> DEPENDENCY_BLOCKED_BY_UPSTREAM.

## DEPENDENCY_CHECKING -> DEPENDENCY_NEED_OWNER_REVIEW.

## DEPENDENCY_FAIL -> DEPENDENCY_NEED_OWNER_REVIEW.

DEPENDENCY_NEED_OWNER_REVIEW -> DEPENDENCY_PASS nếu có evidence.

DEPENDENCY_NEED_OWNER_REVIEW -> DEPENDENCY_FAIL nếu chưa đủ evidence.

## 10.4. Transition bị cấm

## DEPENDENCY_FAIL -> RELEASE_READY.

DEPENDENCY_BLOCKED_BY_UPSTREAM -> downstream release.

## DEPENDENCY_NOT_CHECKED -> GO_LIVE_DECISION_ACCEPTED.

DEPENDENCY_WAIVER_REQUESTED -> production release.

Upstream bị chặn nhưng downstream tự pass.

## 10.5. P0 điểm chặn

FAIL nếu Commerce bán khi Operational Sellable Gate fail.

FAIL nếu AI báo giá khi Commerce QuoteSnapshot fail.

FAIL nếu Ads dùng revenue khi Verified Revenue fail.

FAIL nếu MC AI Live go-live khi Gateway/AI/Commerce chưa pass.

FAIL nếu IVR chạy thật khi Official Order/Core Order State chưa pass.

## 11. PRIVACY / SECURITY RELEASE STATE MODEL

## 11.1. Danh sách trạng thái

Privacy/Security có các trạng thái:

## PRIVACY_SECURITY_NOT_CHECKED.

## PRIVACY_SECURITY_REVIEW_REQUIRED.

## PRIVACY_SECURITY_REVIEWING.

## PRIVACY_SECURITY_PASS.

## PRIVACY_SECURITY_FAIL.

## PRIVACY_SECURITY_FIX_REQUIRED.

## PRIVACY_SECURITY_RETEST_REQUIRED.

## PRIVACY_SECURITY_WAIVER_REQUESTED.

## PRIVACY_SECURITY_WAIVER_REJECTED.

## PRIVACY_SECURITY_CONDITIONAL_ACCEPTANCE.

## 11.2. Bắt buộc review khi scope có

Privacy/Security Review bắt buộc nếu release scope liên quan:

Dữ liệu khách hàng.

Payment.

Member tier.

Diamond/referral.

Health note.

Messenger/private conversation.

Public comment.

IVR call log/recording.

Notification.

Admin override.

Public trace.

Evidence chứa dữ liệu runtime.

## 11.3. Transition bị cấm

## PRIVACY_SECURITY_FAIL -> RELEASE_DECISION_ACCEPTED.

PRIVACY_SECURITY_NOT_CHECKED -> GO_LIVE_DECISION_ACCEPTED nếu scope có dữ liệu nhạy cảm.

## PRIVACY_SECURITY_WAIVER_REQUESTED -> GO_LIVE_DECISION_ACCEPTED.

Evidence lộ secret -> Evidence Accepted.

Public/private boundary fail -> Release Ready.

## 11.4. P0 điểm chặn

FAIL nếu public dữ liệu private.

FAIL nếu evidence lộ secret.

FAIL nếu AI/Gateway/Live public thông tin riêng tư.

FAIL nếu IVR đọc full address/payment/member/Diamond/health note sai policy.

FAIL nếu admin override không audit.

## 12. PRODUCTION READINESS STATE MODEL

## 12.1. Danh sách trạng thái

Production Readiness có các trạng thái:

## PROD_READY_NOT_EVALUATED.

## PROD_READY_EVALUATING.

## PROD_READY_BLOCKED_DOCUMENTATION.

## PROD_READY_BLOCKED_EVIDENCE.

## PROD_READY_BLOCKED_SMOKE.

## PROD_READY_BLOCKED_UAT.

## PROD_READY_BLOCKED_OWNER_SIGNOFF.

## PROD_READY_BLOCKED_DEPENDENCY.

## PROD_READY_BLOCKED_PRIVACY_SECURITY.

## PROD_READY_BLOCKED_ROLLBACK.

## PROD_READY_BLOCKED_MONITORING.

## PROD_READY_BLOCKED_P0.

## PROD_READY_RECOMMENDED.

## PROD_READY_REJECTED.

## 12.2. Production Ready recommendation rule

Production Readiness Resolver chỉ được recommend PROD_READY_RECOMMENDED khi:

Documentation Complete.

Evidence Accepted.

Smoke Pass.

UAT Pass nếu required.

Owner Sign-off Approved.

P0 điểm chặn Closed.

Cross-Tech Dependency Pass.

Privacy/Security Pass.

Rollback/Fallback Ready.

Monitoring/Alert Ready.

Environment ready.

Release scope rõ.

## 12.3. Transition bị cấm

## PROD_READY_BLOCKED_EVIDENCE -> PROD_READY_RECOMMENDED.

## PROD_READY_BLOCKED_SMOKE -> PROD_READY_RECOMMENDED.

## PROD_READY_BLOCKED_P0 -> PROD_READY_RECOMMENDED.

PROD_READY_RECOMMENDED -> GO_LIVE_DECISION_ACCEPTED nếu chưa có Release Decision.

PROD_READY_RECOMMENDED -> SCALE_APPROVED nếu chưa qua Post-Go-Live/Data Quality.

## 12.4. P0 điểm chặn

## 13. RELEASE DECISION STATE MODEL

## 13.1. Danh sách trạng thái

Release Decision có các trạng thái:

## RELEASE_DECISION_NOT_REQUESTED.

## RELEASE_DECISION_REQUESTED.

## RELEASE_DECISION_REVIEWING.

## RELEASE_REJECTED.

## RELEASE_DECISION_ACCEPTED_FOR_INTERNAL_TEST.

## RELEASE_DECISION_ACCEPTED_FOR_STAGING_UAT.

## RELEASE_DECISION_ACCEPTED_FOR_LIMITED_PILOT.

## RELEASE_DECISION_ACCEPTED_FOR_PRODUCTION_GO_LIVE.

## RELEASE_DECISION_ACCEPTED_WITH_CONDITIONS.

## RELEASE_PAUSED.

## ROLLBACK_REQUIRED.

## 13.2. Release decision required fields

Release decision bắt buộc có:

Release scope.

Environment.

Decision type.

Decision owner.

Decision timestamp.

Evidence summary.

Smoke summary.

UAT summary nếu có.

Owner sign-off summary.

Open risk summary.

Rollback/fallback reference.

Monitoring window.

Incident owner.

Gateway target state.

## 13.3. Transition bị cấm

## RELEASE_DECISION_NOT_REQUESTED -> GO_LIVE_DECISION_ACCEPTED.

## RELEASE_REJECTED -> GO_LIVE_DECISION_ACCEPTED.

RELEASE_PAUSED -> production traffic open.

RELEASE_DECISION_ACCEPTED_FOR_INTERNAL_TEST -> production go-live.

RELEASE_DECISION_ACCEPTED_FOR_STAGING_UAT -> production go-live.

RELEASE_DECISION_ACCEPTED_WITH_CONDITIONS -> production go-live nếu điều kiện P0 chưa thỏa.

Release decision thiếu rollback/fallback -> GO_LIVE_DECISION_ACCEPTED.

## 13.4. P0 điểm chặn

FAIL nếu không release decision nhưng go-live.

FAIL nếu release decision không có scope.

FAIL nếu release decision không có rollback/fallback.

FAIL nếu release decision không có incident owner.

## 14. GLOBAL GATEWAY STATE MACHINE

## 14.1. Danh sách trạng thái Global Gateway

Global Gateway có các trạng thái:

bị chặn
Trạng thái mặc định.

DOCUMENTATION_COMPLETE_WAITING_EVIDENCE
Tài liệu đã xong, chờ evidence.

EVIDENCE_SUBMITTED_WAITING_REVIEW
Evidence đã nộp, chờ review.

EVIDENCE_ACCEPTED_WAITING_SMOKE
Evidence accepted, chờ smoke.

SMOKE_RUNNING
Đang chạy smoke.

SMOKE_PASS_WAITING_UAT
Smoke pass, chờ UAT nếu required.

UAT_RUNNING
Đang UAT.

UAT_PASS_WAITING_OWNER_SIGNOFF
UAT pass, chờ owner sign-off.

OWNER_SIGNED_WAITING_RELEASE_DECISION
Owner đã ký, chờ release decision.

RELEASE_DECISION_ACCEPTED_WAITING_GO_LIVE
Release approved, chờ go-live execution.

GO_LIVE_DECISION_ACCEPTED
Được phép go-live theo scope đã duyệt.

GO_LIVE_BLOCKED
Go-live bị chặn do điểm chặn.

ROLLBACK_REQUIRED
Phải rollback hoặc fallback.

POST_GO_LIVE_MONITORING
Đã go-live và đang theo dõi.

SCALE_BLOCKED
Chưa được scale.

SCALE_APPROVED
Được scale theo scope đã duyệt.

## 14.2. Transition hợp lệ

bị chặn -> DOCUMENTATION_COMPLETE_WAITING_EVIDENCE nếu tài liệu complete.

## DOCUMENTATION_COMPLETE_WAITING_EVIDENCE -> EVIDENCE_SUBMITTED_WAITING_REVIEW.

## EVIDENCE_SUBMITTED_WAITING_REVIEW -> EVIDENCE_ACCEPTED_WAITING_SMOKE.

## EVIDENCE_ACCEPTED_WAITING_SMOKE -> SMOKE_RUNNING.

## SMOKE_RUNNING -> SMOKE_PASS_WAITING_UAT.

SMOKE_PASS_WAITING_UAT -> UAT_RUNNING nếu UAT required.

SMOKE_PASS_WAITING_UAT -> UAT_PASS_WAITING_OWNER_SIGNOFF nếu UAT not required và owner sign-off WAITING.

## UAT_RUNNING -> UAT_PASS_WAITING_OWNER_SIGNOFF.

## UAT_PASS_WAITING_OWNER_SIGNOFF -> OWNER_SIGNED_WAITING_RELEASE_DECISION.

## OWNER_SIGNED_WAITING_RELEASE_DECISION -> RELEASE_DECISION_ACCEPTED_WAITING_GO_LIVE.

## RELEASE_DECISION_ACCEPTED_WAITING_GO_LIVE -> GO_LIVE_DECISION_ACCEPTED.

## GO_LIVE_DECISION_ACCEPTED -> POST_GO_LIVE_MONITORING.

POST_GO_LIVE_MONITORING -> SCALE_BLOCKED nếu chưa đủ scale.

POST_GO_LIVE_MONITORING -> SCALE_APPROVED nếu scale criteria pass.

Bất kỳ trạng thái nào -> GO_LIVE_BLOCKED nếu phát sinh điểm chặn.

Bất kỳ trạng thái nào -> ROLLBACK_REQUIRED nếu phát sinh incident hoặc release decision yêu cầu.

## 14.3. Transition bị cấm

bị chặn -> GO_LIVE_DECISION_ACCEPTED.

## DOCUMENTATION_COMPLETE_WAITING_EVIDENCE -> RELEASE_DECISION_ACCEPTED_WAITING_GO_LIVE.

## EVIDENCE_SUBMITTED_WAITING_REVIEW -> SMOKE_PASS_WAITING_UAT.

## SMOKE_RUNNING -> RELEASE_DECISION_ACCEPTED_WAITING_GO_LIVE.

## UAT_RUNNING -> GO_LIVE_DECISION_ACCEPTED.

OWNER_SIGNED_WAITING_RELEASE_DECISION -> GO_LIVE_DECISION_ACCEPTED nếu chưa có release decision.

GO_LIVE_DECISION_ACCEPTED -> SCALE_APPROVED nếu chưa qua post-go-live monitoring.

SCALE_BLOCKED -> SCALE_APPROVED khi Data Quality fail.

GO_LIVE_BLOCKED -> GO_LIVE_DECISION_ACCEPTED nếu điểm chặn chưa closed.

## ROLLBACK_REQUIRED -> SCALE_APPROVED.

## 14.4. Gateway reason code

Mỗi trạng thái Gateway phải có reason code.

Reason code tối thiểu:

## WAITING_EVIDENCE.

## EVIDENCE_UNDER_REVIEW.

## EVIDENCE_REJECTED.

## WAITING_SMOKE.

## SMOKE_FAILED.

## SMOKE_BLOCKED.

## WAITING_UAT.

## UAT_FAILED.

## WAITING_OWNER_SIGNOFF.

## OWNER_SIGNOFF_MISSING.

## P0_STOP_POINT_OPEN.

## DEPENDENCY_FAILED.

## PRIVACY_SECURITY_FAILED.

## ROLLBACK_NOT_READY.

## MONITORING_NOT_READY.

## RELEASE_DECISION_MISSING.

## GO_LIVE_DECISION_ACCEPTED_BY_DECISION.

## ROLLBACK_REQUIRED_BY_INCIDENT.

## SCALE_BLOCKED_DATA_QUALITY.

## SCALE_APPROVED_BY_DECISION.

## 14.5. P0 điểm chặn

FAIL nếu Gateway không mặc định bị chặn.

FAIL nếu Gateway không có reason code.

FAIL nếu Gateway transition nhảy bước.

FAIL nếu Gateway mở khi thiếu release decision.

FAIL nếu Gateway mở khi còn P0 điểm chặn.

## 15. POST-GO-LIVE MONITORING STATE MODEL

## 15.1. Danh sách trạng thái

Post-Go-Live Monitoring có các trạng thái:

## PGL_NOT_STARTED.

## PGL_MONITORING_ACTIVE.

## PGL_STABLE.

## PGL_WARNING.

## PGL_INCIDENT_OPEN.

## PGL_CRITICAL_INCIDENT.

## PGL_ROLLBACK_RECOMMENDED.

## PGL_ROLLBACK_EXECUTED.

## PGL_CLOSED.

## PGL_SCALE_REVIEW_REQUIRED.

## 15.2. Monitoring bắt buộc

Post-go-live phải theo dõi:

Error rate.

Order/payment mismatch.

Verified Revenue mismatch.

Sale Lock / Recall suppression.

AI response guard fail.

Gateway public/private leakage.

Ads data quality.

MC AI Live suppression.

IVR technical failure.

Notification delivery failure.

Evidence write failure.

Admin override anomaly.

Customer complaint spike.

Security/privacy incident.

## 15.3. Transition bị cấm

## PGL_NOT_STARTED -> SCALE_APPROVED.

## PGL_INCIDENT_OPEN -> SCALE_APPROVED.

## PGL_CRITICAL_INCIDENT -> SCALE_APPROVED.

## PGL_ROLLBACK_RECOMMENDED -> SCALE_APPROVED.

PGL_WARNING -> SCALE_APPROVED nếu chưa owner review.

PGL_STABLE -> SCALE_APPROVED nếu Data Quality fail.

## 15.4. P0 điểm chặn

FAIL nếu go-live không có post-go-live monitoring.

FAIL nếu critical incident không route owner.

FAIL nếu incident open nhưng vẫn scale.

FAIL nếu privacy/security incident bị bỏ qua.

## 16. SCALE APPROVAL STATE MODEL

## 16.1. Danh sách trạng thái

Scale Approval có các trạng thái:

## SCALE_NOT_REQUESTED.

## SCALE_REQUESTED.

## SCALE_REVIEWING.

## SCALE_BLOCKED_DATA_QUALITY.

## SCALE_BLOCKED_REVENUE_VERIFICATION.

## SCALE_BLOCKED_OPERATIONAL_SUPPRESSION.

## SCALE_BLOCKED_PRIVACY_SECURITY.

## SCALE_BLOCKED_INCIDENT.

## SCALE_BLOCKED_OWNER_APPROVAL.

## SCALE_APPROVED_LIMITED.

## SCALE_APPROVED_FULL.

## SCALE_PAUSED.

## SCALE_ROLLBACK_REQUIRED.

## 16.2. Scale approval rule

Scale chỉ được approved khi:

Go-live scope ổn định.

Post-go-live monitoring pass.

No critical incident.

Verified Revenue đúng.

Ads data quality pass nếu scale Ads.

Operational sellable pass.

No Sale Lock / Recall.

Privacy/security pass.

Owner approval có.

Stop-scale rule có.

Monitoring/alert sẵn sàng.

Release decision hoặc scale decision rõ.

## 16.3. Transition bị cấm

SCALE_REQUESTED -> SCALE_APPROVED nếu Verified Revenue fail.

SCALE_REQUESTED -> SCALE_APPROVED nếu Data Quality fail.

SCALE_REQUESTED -> SCALE_APPROVED nếu Sale Lock/Recall active.

SCALE_REQUESTED -> SCALE_APPROVED nếu post-go-live incident open.

SCALE_BLOCKED_* -> SCALE_APPROVED nếu chưa có fix/evidence/review.

## 16.4. P0 điểm chặn

FAIL nếu scale vì ROAS đẹp nhưng revenue chưa verified.

FAIL nếu scale khi Data Quality fail.

FAIL nếu scale khi Sale Lock/Recall/Suppression active.

FAIL nếu scale khi owner chưa duyệt.

FAIL nếu scale không có stop-scale rule.

## 17. ROLLBACK / FALLBACK STATE MODEL

## 17.1. Danh sách trạng thái

Rollback/Fallback có các trạng thái:

## ROLLBACK_NOT_REQUIRED.

## ROLLBACK_PLAN_REQUIRED.

## ROLLBACK_PLAN_READY.

## FALLBACK_PLAN_READY.

## ROLLBACK_TRIGGERED.

## FALLBACK_TRIGGERED.

## ROLLBACK_IN_PROGRESS.

## FALLBACK_IN_PROGRESS.

## ROLLBACK_COMPLETED.

## FALLBACK_COMPLETED.

## ROLLBACK_FAILED.

## FALLBACK_FAILED.

## ROLLBACK_REVIEW_REQUIRED.

## 17.2. Trigger rollback/fallback

Rollback/Fallback được kích hoạt khi:

Critical incident.

Payment/revenue mismatch nghiêm trọng.

Privacy/security incident.

Sale Lock/Recall không chặn downstream.

AI public sai claim nghiêm trọng.

Gateway public private data.

IVR gọi sai policy hàng loạt.

Ads scale sai vì revenue/data quality fail.

Evidence/audit write failure nghiêm trọng.

Admin override abuse.

Monitoring mất tín hiệu critical.

Owner/Release Board quyết định rollback.

## 17.3. Transition bị cấm

## ROLLBACK_PLAN_REQUIRED -> GO_LIVE_DECISION_ACCEPTED.

## ROLLBACK_TRIGGERED -> SCALE_APPROVED.

## ROLLBACK_IN_PROGRESS -> POST_GO_LIVE_CLOSED.

## ROLLBACK_FAILED -> RELEASE_DECISION_ACCEPTED.

## FALLBACK_FAILED -> GO_LIVE_DECISION_ACCEPTED.

## 17.4. P0 điểm chặn

FAIL nếu production release không có rollback/fallback plan.

FAIL nếu rollback phá audit/trace/evidence.

FAIL nếu incident đủ trigger nhưng không rollback/fallback.

FAIL nếu rollback/fallback không có owner.

## 18. END-TO-END RELEASE FLOW

## 18.1. Flow chuẩn

Flow chuẩn từ tài liệu đến go-live:

## DOC_DOCUMENTATION_COMPLETE.

## EVIDENCE_REQUIRED.

## EVIDENCE_SUBMITTED.

## EVIDENCE_ACCEPTED.

## SMOKE_READY_TO_RUN.

## SMOKE_RUNNING.

## SMOKE_PASS.

UAT_REQUIRED hoặc UAT_NOT_REQUIRED.

UAT_PASSED nếu required.

## SIGNOFF_APPROVED.

## STOP_POINT_CLOSED.

## DEPENDENCY_PASS.

## PRIVACY_SECURITY_PASS.

## ROLLBACK_PLAN_READY.

## MONITORING_READY.

## PROD_READY_RECOMMENDED.

## RELEASE_DECISION_ACCEPTED_FOR_PRODUCTION_GO_LIVE.

## GO_LIVE_DECISION_ACCEPTED.

## POST_GO_LIVE_MONITORING.

SCALE_BLOCKED hoặc SCALE_APPROVED theo điều kiện.

## 18.2. Flow khi fail evidence

## EVIDENCE_SUBMITTED.

## EVIDENCE_UNDER_REVIEW.

EVIDENCE_REJECTED hoặc EVIDENCE_NEED_MORE_INFO.

Gateway giữ bị chặn/WAITING_EVIDENCE.

Không chạy smoke release-ready.

Không owner sign-off.

Không release decision.

Evidence phải nộp lại.

## 18.3. Flow khi smoke fail

## SMOKE_RUNNING.

## SMOKE_FAIL.

P0 điểm chặn nếu fail critical.

Fix required.

Retest required.

Evidence update.

Smoke retest.

Chỉ khi retest pass mới xét tiếp.

## 18.4. Flow khi UAT fail

## UAT_RUNNING.

## UAT_FEEDBACK_WAITING.

## UAT_FAILED.

Fix required.

Retest required.

Owner review lại.

Không release nếu UAT fail.

## 18.5. Flow khi dependency fail

Dependency check fail.

Downstream bị chặn.

Upstream owner xử lý.

Smoke cross-tech retest.

Dependency pass mới tiếp tục.

## 18.6. Flow khi P0 điểm chặn phát sinh sau sign-off

P0 điểm chặn reopened.

Sign-off có thể revoked.

Gateway chuyển GO_LIVE_BLOCKED hoặc bị chặn.

Release decision paused hoặc rollback required.

Retest và owner sign-off lại nếu cần.

## 19. CROSS-TECH VALIDATION FLOW

## 19.1. Product -> Operational -> Commerce

Phải xác minh:

Product Active không đồng nghĩa Sellable.

SKU phải qua Product Activation.

Operational Sellable Gate phải pass.

Sale Lock/Recall phải chặn Commerce.

Inventory/warehouse status phải đúng.

Commerce không được bán SKU không sellable.

P0 Fail nếu Commerce bán SKU chưa sellable.

## 19.2. Commerce -> AI Advisor

Phải xác minh:

AI không tự tính giá.

AI chỉ dùng QuoteSnapshot.

AI không tự tạo official order.

AI không xác nhận payment.

AI không xác nhận Verified Revenue.

AI không bán SKU Sale Lock/Recall.

AI không dùng claim chưa approved.

P0 Fail nếu AI tự báo giá không qua QuoteSnapshot.

## 19.3. AI Advisor -> Facebook Gateway

Phải xác minh:

Gateway chỉ gửi response đã qua Final Response Guard.

Public comment không lộ private data.

Hỏi giá/mua/chốt phải kéo Messenger/private theo rule.

Sau Messenger handoff, conversation xử lý private.

Gateway có typing indicator/response pacing.

Gateway không giả mạo người thật.

P0 Fail nếu Gateway public giá/thông tin riêng tư sai rule.

## 19.4. Gateway -> MC AI Live

Phải xác minh:

MC AI Live không tự báo giá.

MC AI Live không tự tạo order.

MC AI Live không tự xác nhận payment/revenue.

MC AI Live không dùng live signal làm ROAS.

MC AI Live không dùng claim chưa approved.

Troll/abuse không bị kéo Messenger sai policy.

Complaint thật route đúng.

P0 Fail nếu MC AI Live dùng live signal làm revenue/ROAS.

## 19.5. Commerce -> Ads Measurement

Phải xác minh:

Ads không dùng quote/cart/order draft làm revenue.

Ads không dùng unpaid order làm revenue.

Ads chỉ dùng Verified Revenue.

Payment WAITING không phải revenue.

COD WAITING không phải revenue.

Refund/cancel/invalid order excluded theo policy.

Pixel/CAPI dedup/idempotent.

Scale chỉ khi Data Quality pass.

P0 Fail nếu Ads scale bằng fake ROAS.

## 19.6. Commerce -> IVR

Phải xác minh:

IVR chỉ xử lý Official Order.

IVR không gọi Quote/Cart/Order Draft.

IVR chỉ áp dụng khách mới/untrusted/risk theo policy.

Khách trusted không bị gọi đại trà.

IVR không xác nhận payment/revenue.

No answer/customer cancel/invalid phone/technical failure tách rõ.

Notification chỉ sau Core Order State Machine hủy chính thức.

P0 Fail nếu IVR gọi order draft hoặc tự gửi SMS hủy.

## 19.7. IVR -> Notification Owner

Phải xác minh:

IVR không tự gửi notification.

SIM Gateway không tự gửi notification.

AI Advisor không tự phát tin hủy.

Notification chỉ gửi sau Core cancellation.

Nội dung transactional, privacy-safe.

Không có full address/payment/member/Diamond/health note.

P0 Fail nếu notification gửi trước Core cancellation.

## 19.8. TECH-01 Evidence/Audit -> Tất cả high-risk flows

Phải xác minh:

High-risk command có audit.

Idempotency có nếu cần.

Evidence có correlation.

Admin override có reason/evidence/audit.

State transition có log.

Không có action critical thiếu evidence.

P0 Fail nếu high-risk action không audit.

## 20. RELEASE FAIL-SAFE MODEL

## 20.1. Khi thiếu dữ liệu release

Nếu thiếu một trong các dữ liệu:

Evidence.

Smoke result.

UAT result.

Owner sign-off.

Dependency result.

Privacy/security result.

Rollback/fallback readiness.

Monitoring readiness.

Release decision.

Thì release phải fail-safe:

Global Gate bị chặn

## 20.2. Khi hệ thống release governance unavailable

Nếu release governance runtime/tooling unavailable:

Không tự mở Gateway.

Không tự coi release approved.

Không tự scale.

Không tự go-live.

Giữ trạng thái an toàn gần nhất.

Route owner review.

## 20.3. Khi conflict giữa owner

Nếu owner mâu thuẫn:

Không release tự động.

Route Release Decision Board.

Ghi rõ conflict.

Chờ decision.

Gateway giữ bị chặn/WAITING.

## 20.4. Khi evidence bị revoke sau release

Nếu evidence bị revoke:

Gateway chuyển warning/block theo severity.

Kiểm tra affected scope.

Mở điểm chặn.

Retest.

Có thể pause/rollback.

Owner review lại.

## 21. GLOBAL P0 điểm chặn REGISTRY - PHẦN 3/4

Các lỗi sau là P0 điểm chặn cấp lifecycle/state machine:

Documentation Complete chuyển thẳng Production Ready.

Evidence Submitted chuyển thẳng Evidence Accepted.

Evidence chưa accepted nhưng Completion Decision.

Smoke chưa chạy nhưng Release Ready.

Smoke fail nhưng Release Ready.

UAT do dev tự xác nhận.

Owner chưa sign-off nhưng Release Approved.

Release decision thiếu nhưng Go-live Approved.

P0 điểm chặn open nhưng Gateway mở.

Dependency fail nhưng downstream release.

Privacy/Security fail nhưng go-live.

Rollback/Fallback missing nhưng production release.

Monitoring missing nhưng go-live.

Gateway transition nhảy bước.

Gateway không có reason code.

Post-go-live incident open nhưng scale approved.

Verified Revenue fail nhưng Ads scale.

Data Quality fail nhưng Ads scale.

Sale Lock/Recall active nhưng Commerce/AI/Ads/Live/IVR vẫn hoạt động.

IVR/Gateway/AI/Live public hoặc gửi sai dữ liệu privacy-sensitive.

Admin override không audit.

Evidence bị revoke nhưng release status không cập nhật.

Release governance unavailable nhưng hệ thống tự mở Gateway.

Rollback trigger đạt nhưng không rollback/fallback.

Monitoring cảnh báo critical nhưng không route owner.

## 22. SMOKE ĐỊNH HƯỚNG - PHẦN 3/4

TECH10-P3-SMK-001 - Documentation Complete Không Nhảy Production

Given một TECH đã Documentation Complete
When Gateway State Controller kiểm tra
Then Gateway không được chuyển Production Ready.

TECH10-P3-SMK-002 - Evidence Review Required

Given evidence đã nộp
When chưa reviewer accepted
Then Completion Decision = KHONG.

TECH10-P3-SMK-003 - Evidence Rejected Blocks Smoke

Given evidence bị rejected
When Smoke Matrix Orchestrator xét readiness
Then smoke release-ready không được chạy như pass path.

TECH10-P3-SMK-004 - Smoke Fail Blocks Release Ready

TECH10-P3-SMK-005 - UAT Owner Required

Given UAT scope có nghiệp vụ vận hành
When UAT không có owner nghiệp vụ
Then UAT invalid.

TECH10-P3-SMK-006 - Owner Sign-off Missing

TECH10-P3-SMK-007 - P0 điểm chặn Reopened

Given P0 điểm chặn reopened sau sign-off
When Gateway kiểm tra
Then Gateway chuyển bị chặn/GO_LIVE_BLOCKED.

TECH10-P3-SMK-008 - Cross-Tech Upstream Fail

Given Operational Sellable Gate fail
When Commerce readiness được xét
Then Commerce bị chặn.

TECH10-P3-SMK-009 - Privacy Fail Blocks Release

Given Gateway public private data
When Privacy/Security Review chạy
Then release bị chặn.

TECH10-P3-SMK-010 - Rollback Missing Blocks Production

TECH10-P3-SMK-011 - Monitoring Missing Blocks Go-live

TECH10-P3-SMK-012 - Release Decision Missing

TECH10-P3-SMK-013 - Post-Go-Live Incident Blocks Scale

Given post-go-live có incident critical
When scale approval được xét
Then Scale Approved = KHONG.

TECH10-P3-SMK-014 - Verified Revenue Blocks Ads Scale

Given Ads ROAS đẹp nhưng Verified Revenue fail
When scale gate chạy
Then Scale Approved = KHONG.

TECH10-P3-SMK-015 - Rollback Trigger

Given IVR gọi sai policy hàng loạt
When monitoring phát hiện
Then rollback/fallback trigger phải được kích hoạt.

## 23. EVIDENCE PACKAGE - PHẦN 3/4

## PHẦN 3/4 yêu cầu evidence thiết kế cho lifecycle gồm:

Documentation State Model accepted.

Evidence State Model accepted.

Smoke State Model accepted.

UAT State Model accepted.

Owner Sign-off State Model accepted.

P0 điểm chặn State Model accepted.

Cross-Tech Dependency State Model accepted.

Privacy/Security Release State Model accepted.

Production Readiness State Model accepted.

Release Decision State Model accepted.

Global Gateway State Machine accepted.

Post-Go-Live Monitoring State Model accepted.

Scale Approval State Model accepted.

Rollback/Fallback State Model accepted.

End-to-End Release Flow accepted.

Cross-Tech Validation Flow accepted.

Release Fail-safe Model accepted.

P0 điểm chặn Registry accepted.

Smoke định hướng accepted.

## 24. DONE GATE - PHẦN 3/4

## PHẦN 3/4 đạt Documentation Complete ở cấp lifecycle/state machine khi:

Đã khóa Global Release Lifecycle tổng thể.

Đã khóa Documentation State Model.

Đã khóa Evidence State Model.

Đã khóa Smoke State Model.

Đã khóa UAT State Model.

Đã khóa Owner Sign-off State Model.

Đã khóa P0 điểm chặn State Model.

Đã khóa Cross-Tech Dependency State Model.

Đã khóa Privacy/Security Release State Model.

Đã khóa Production Readiness State Model.

Đã khóa Release Decision State Model.

Đã khóa Global Gateway State Machine.

Đã khóa Post-Go-Live Monitoring State Model.

Đã khóa Scale Approval State Model.

Đã khóa Rollback/Fallback State Model.

Đã khóa End-to-End Release Flow.

Đã khóa Cross-Tech Validation Flow.

Đã khóa Release Fail-safe Model.

Đã có Global P0 điểm chặn Registry.

Đã có Smoke định hướng.

Đã có Evidence Package.

Đã sẵn sàng chuyển sang PHẦN 4/4.

## 25. FAIL GATE - PHẦN 3/4

## PHẦN 3/4 FAIL nếu:

Thiếu state model quan trọng.

Gateway state machine không rõ.

Gateway không mặc định bị chặn.

Documentation Complete có đường nhảy thẳng Production Ready.

Evidence Submitted có đường nhảy thẳng Evidence Accepted.

Smoke fail vẫn có đường Release Ready.

UAT thiếu owner vẫn pass.

Owner thiếu sign-off vẫn Release Approved.

Release Decision thiếu vẫn Go-live Approved.

P0 điểm chặn open vẫn Gateway open.

Cross-tech dependency fail vẫn downstream release.

Privacy/Security fail vẫn production go-live.

Rollback/Fallback missing vẫn production release.

Monitoring/Alert missing vẫn go-live.

Post-go-live incident vẫn scale.

Scale approved khi Verified Revenue/Data Quality fail.

Sale Lock/Recall không block downstream.

Release fail-safe chưa rõ.

Conflict owner không có route Release Decision Board.

Evidence revoke không cập nhật release status.

## 26. RELEASE HANDOFF - SANG PHẦN 4/4

## PHẦN 3/4 bàn giao sang PHẦN 4/4 các nội dung đã khóa:

Global Release Lifecycle.

Documentation State Model.

Evidence State Model.

Smoke State Model.

UAT State Model.

Owner Sign-off State Model.

P0 điểm chặn State Model.

Cross-Tech Dependency State Model.

Privacy/Security Release State Model.

Production Readiness State Model.

Release Decision State Model.

Global Gateway State Machine.

Post-Go-Live Monitoring State Model.

Scale Approval State Model.

Rollback/Fallback State Model.

End-to-End Release Flow.

Cross-Tech Validation Flow.

Release Fail-safe Model.

P0 điểm chặn Registry.

Smoke định hướng.

Evidence Package.

## PHẦN 4/4 phải tiếp tục viết:

FINAL GLOBAL SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-10 FINAL CONCLUSION

Trong đó phải khóa rõ:

Smoke matrix tổng thể toàn hệ thống.

Cross-tech smoke bắt buộc.

Evidence package cuối cùng.

Owner review points.

Privacy/Security review.

Release readiness review.

Production readiness review.

Go-live decision review.

Post-go-live monitoring review.

Scale approval review.

Final conclusion.

## 27. TRẠNG THÁI SAU PHẦN 3/4

Sau PHẦN 3/4:

## PHẦN 1/4 đã khóa nguyên tắc release governance.

## PHẦN 2/4 đã khóa module contracts.

## PHẦN 3/4 đã khóa lifecycle, state machine, release flow, gateway transition và cross-tech validation.

Chưa được gọi là Documentation Complete.

Chưa được gọi là Production Ready.

Chưa được gọi là Release Ready.

Chưa được gọi là Release Approved.

Chưa được gọi là Go-live Approved.

Chưa được gọi là Scale Approved.

Global Gateway vẫn mặc định:

bị chặn

## KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 đã khóa vòng đời release tổng thể cho hệ thống Ginsengfood.

Các điểm trọng yếu đã được cố định:

Documentation Complete không được nhảy thẳng sang Production Ready.

Evidence Submitted không phải Evidence Accepted.

Evidence chưa accepted thì không Completion Decision.

Smoke chưa pass thì không Release Ready.

Smoke pass cục bộ không phải Global Smoke Pass.

UAT phải có owner nghiệp vụ nếu scope yêu cầu.

Owner sign-off phải có evidence, scope và audit.

P0 điểm chặn còn mở thì Global Gateway vẫn bị chặn.

Cross-tech dependency fail thì downstream bị chặn.

Privacy/Security fail thì release bị chặn.

Rollback/Fallback missing thì production release bị chặn.

Monitoring/Alert missing thì go-live bị chặn.

Production Ready chỉ là recommendation, chưa phải Go-live Approved.

Release Decision Board mới ra quyết định release chính thức.

Global Gateway State Controller giữ trạng thái cuối.

Go-live xong vẫn phải Post-Go-Live Monitoring.

Scale chỉ được xét sau khi post-go-live ổn định và Data Quality/Verified Revenue/Suppression pass.

Rollback/Fallback phải kích hoạt khi có trigger.

Evidence bị revoke thì release status phải cập nhật lại.

Runtime/release governance unavailable thì fail-safe, không tự mở Gateway.

## PHẦN 3/4 sẵn sàng bàn giao sang:

## PHẦN 4/4 - FINAL GLOBAL SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-10 FINAL CONCLUSION.

## PHẦN 4/4 - FINAL GLOBAL SMOKE MATRIX / EVIDENCE PACKAGE / DONE GATE / FAIL GATE / RELEASE HANDOFF / TECH-10 FINAL CONCLUSION

## 1. MỤC TIÊU PHẦN 4/4

## PHẦN 4/4 khóa lớp kiểm soát cuối cùng của TECH-10.

Mục tiêu là xác định rõ:

Smoke matrix tổng thể toàn hệ thống.

Cross-tech smoke bắt buộc.

Evidence package cuối cùng.

Owner review points.

Privacy / Security review.

Release readiness review.

Production readiness review.

Go-live decision review.

Post-go-live monitoring review.

Scale approval review.

Global Done Gate.

Global Fail Gate.

Release Handoff.

Kết luận cuối cùng của TECH-10.

## PHẦN 4/4 không viết code.

## PHẦN 4/4 không thiết kế API chi tiết.

## PHẦN 4/4 không thiết kế database chi tiết.

## PHẦN 4/4 không thiết kế UI chi tiết.

## PHẦN 4/4 chỉ khóa chuẩn kiểm thử, evidence, release gate, go-live gate và global gateway control.

## 2. FINAL GLOBAL RELEASE PRINCIPLE

TECH-10 khẳng định nguyên tắc cuối cùng:

Tài liệu hoàn chỉnh không đồng nghĩa hệ thống sẵn sàng vận hành.

Một hệ thống chỉ được xét release thật khi có đủ:

Documentation Complete.

Evidence Accepted.

Smoke Pass.

UAT Pass nếu scope yêu cầu.

Owner Sign-off.

No Open P0 điểm chặn.

Cross-Tech Dependency Pass.

Privacy / Security Pass.

Rollback / Fallback Ready.

Monitoring / Alert Ready.

Release Decision.

Global Gateway State hợp lệ.

Nếu thiếu bất kỳ điều kiện bắt buộc nào:

Global Gate bị chặn

## 3. FINAL GLOBAL SMOKE MATRIX - NHÓM A: DOCUMENTATION / SOURCE-OF-TRUTH

TECH10-FSMK-001 - Documentation Complete không phải Production Ready

Given một TECH đã viết đủ các phần theo cấu trúc
When Documentation Completion Registry cập nhật trạng thái
Then trạng thái chỉ được là DOCUMENTATION COMPLETE
Must Not chuyển thành Production Ready, Release Ready hoặc Go-live Approved.

P0 Fail If: tài liệu viết xong được gọi là hệ thống sẵn sàng vận hành.

TECH10-FSMK-002 - LEGACY_REFERENCE_ONLY không thắng Source-of-Truth mới

Given tài liệu cũ khác với TECH source-of-truth mới
When release governance kiểm tra
Then TECH source-of-truth mới thắng
Must Not dùng tài liệu cũ để override tài liệu greenfield.

P0 Fail If: dùng legacy document để phủ định TECH-00 -> TECH-10.

TECH10-FSMK-003 - Code cũ không thắng tài liệu greenfield

Given current implementation khác với tài liệu TECH mới
When dev/release review đối chiếu
Then tài liệu source-of-truth mới là chuẩn để triển khai
Must Not coi code cũ là nghiệp vụ đúng nếu khác tài liệu mới.

P0 Fail If: code cũ được dùng để bypass rule mới.

## 4. FINAL GLOBAL SMOKE MATRIX - NHÓM B: EVIDENCE

TECH10-FSMK-004 - Evidence Submitted không phải Evidence Accepted

Given evidence đã nộp nhưng chưa review
When Evidence Intake & Acceptance Resolver kiểm tra
Then status = EVIDENCE_SUBMITTED / UNDER_REVIEW
Must Not coi là Evidence Accepted.

P0 Fail If: evidence chưa accepted nhưng Completion Decision.

TECH10-FSMK-005 - Evidence phải gắn requirement

Given một evidence không gắn TECH, requirement hoặc smoke ID
When Evidence Resolver kiểm tra
Then evidence phải bị reject hoặc yêu cầu mở rộng.

P0 Fail If: evidence không trace requirement vẫn accepted.

TECH10-FSMK-006 - Evidence phải rõ environment

Given evidence không ghi local/dev/staging/production-like/production
When review evidence
Then evidence không đủ điều kiện accepted.

P0 Fail If: evidence local được dùng để mở production.

TECH10-FSMK-007 - Evidence không được lộ secret hoặc dữ liệu riêng tư

Given evidence chứa token, secret, full customer private data, payment detail hoặc health note sai policy
When Privacy/Security Reviewer kiểm tra
Then evidence bị reject và release bị chặn.

P0 Fail If: evidence lộ dữ liệu nhạy cảm vẫn accepted.

## 5. FINAL GLOBAL SMOKE MATRIX - NHÓM C: SMOKE

TECH10-FSMK-008 - Smoke chưa chạy thì không Release Ready

P0 Fail If: smoke chưa chạy nhưng Release Ready.

TECH10-FSMK-009 - Smoke fail thì Global Gateway bị chặn

Given một smoke P0 fail
When Global Gateway State Controller kiểm tra
Then Gateway = bị chặn hoặc GO_LIVE_BLOCKED.

P0 Fail If: smoke P0 fail nhưng Gateway mở.

TECH10-FSMK-010 - Smoke phải có fail-case

Given smoke chỉ test luồng thành công
When Global Smoke Matrix Orchestrator review
Then smoke matrix chưa đạt.

P0 Fail If: chỉ happy path mà gọi Global Smoke Pass.

TECH10-FSMK-011 - Cross-tech smoke bắt buộc

Given module smoke pass riêng lẻ
When cross-tech smoke chưa chạy hoặc fail
Then Global Smoke Pass = KHONG.

P0 Fail If: module pass riêng lẻ nhưng release toàn hệ thống.

## 6. FINAL GLOBAL SMOKE MATRIX - NHÓM D: UAT / OWNER SIGN-OFF

TECH10-FSMK-012 - UAT không thể do dev tự ký

Given UAT chỉ do dev tự test và tự xác nhận
When UAT Session Resolver kiểm tra
Then UAT status = INVALID.

P0 Fail If: dev tự UAT thay owner nghiệp vụ.

TECH10-FSMK-013 - UAT phải có nghiệp vụ thực tế

Given UAT không kiểm tra flow vận hành thực tế
When Release Ready được xét
Then UAT không đủ điều kiện accepted.

P0 Fail If: UAT chỉ demo nhanh nhưng được gọi là UAT Pass.

TECH10-FSMK-014 - Owner sign-off phải có scope

Given owner ký nhưng không ghi rõ domain/scope/release phạm vi
When Owner Sign-off Resolver kiểm tra
Then sign-off invalid.

P0 Fail If: sign-off không scope vẫn Release Approved.

TECH10-FSMK-015 - Owner sign-off phải gắn evidence

Given owner ký nhưng chưa review evidence/smoke/UAT
When sign-off được kiểm tra
Then sign-off invalid.

P0 Fail If: owner ký cảm tính, không evidence.

## 7. FINAL GLOBAL SMOKE MATRIX - NHÓM E: P0 điểm chặn

TECH10-FSMK-016 - P0 điểm chặn còn mở thì không release

P0 Fail If: điểm chặn mở nhưng go-live.

TECH10-FSMK-017 - điểm chặn chỉ đóng khi có fix + retest evidence

Given điểm chặn được đánh dấu resolved nhưng không có retest evidence
When P0 điểm chặn Resolver kiểm tra
Then điểm chặn không được CLOSED.

P0 Fail If: đóng điểm chặn bằng miệng.

TECH10-FSMK-018 - điểm chặn reopen phải kéo Gateway về bị chặn

Given điểm chặn đã đóng nhưng lỗi tái xuất hiện
When điểm chặn reopened
Then Global Gateway phải chuyển bị chặn / GO_LIVE_BLOCKED theo severity.

P0 Fail If: điểm chặn reopened nhưng Gateway vẫn mở.

## 8. FINAL GLOBAL SMOKE MATRIX - NHÓM F: CROSS-TECH DEPENDENCY

TECH10-FSMK-019 - Product Active không đồng nghĩa Sellable

Given SKU active trong Product nhưng Operational Sellable Gate chưa pass
When Commerce xét bán hàng
Then Commerce phải block.

P0 Fail If: Product Active được bán như Sellable.

TECH10-FSMK-020 - Sale Lock / Recall thắng toàn hệ thống

Given SKU/batch bị Sale Lock hoặc Recall
When Commerce, AI, Gateway, Ads, Live, IVR xử lý
Then toàn bộ downstream phải bị chặn/suppressed theo policy.

P0 Fail If: SKU Recall vẫn được bán, quảng cáo, live, quote hoặc IVR.

TECH10-FSMK-021 - Commerce QuoteSnapshot là nguồn báo giá duy nhất

Given khách hỏi giá qua AI/Live/Gateway
When quote được tạo
Then AI/Gateway/Live chỉ được dùng QuoteSnapshot từ Commerce.

P0 Fail If: AI/Live/Gateway tự tính giá.

TECH10-FSMK-022 - Official Order mới được đưa vào IVR

Given Quote, Cart, Order Draft và Official Order
When IVR Eligibility kiểm tra
Then chỉ Official Order đủ điều kiện mới được xét IVR.

P0 Fail If: IVR gọi cho Quote/Cart/Order Draft.

TECH10-FSMK-023 - Verified Revenue là nguồn revenue cho Ads

Given quote, cart, order draft, unpaid order, payment WAITING, COD WAITING
When Ads Measurement tính ROAS
Then các trạng thái này không được tính revenue.

P0 Fail If: Ads dùng fake revenue.

## 9. FINAL GLOBAL SMOKE MATRIX - NHÓM G: AI ADVISOR / GATEWAY / LIVE

TECH10-FSMK-024 - AI Advisor không tự tạo Official Order

Given khách nói mua/chốt qua AI Advisor
When AI xử lý
Then AI chỉ hỗ trợ order draft/handoff, official order phải qua Commerce.

P0 Fail If: AI tự tạo official order.

TECH10-FSMK-025 - AI Advisor không tự xác nhận Payment

Given khách gửi ảnh chuyển khoản hoặc nói đã chuyển
When AI xử lý
Then AI không được chuyển PAID, phải chờ Payment Confirmation.

P0 Fail If: ảnh chuyển khoản được coi là PAID.

TECH10-FSMK-026 - Gateway không public dữ liệu private

Given khách hỏi giá/mua/chốt trên public comment
When Gateway xử lý
Then Gateway phải kéo Messenger/private theo rule, không public thông tin nhạy cảm.

P0 Fail If: Gateway public giá cá nhân/order/private data sai rule.

TECH10-FSMK-027 - Gateway chỉ gửi response đã qua Final Response Guard

Given AI tạo response
When Gateway chuẩn bị gửi
Then response phải qua Final Response Guard.

P0 Fail If: response chưa guard vẫn gửi ra khách.

TECH10-FSMK-028 - MC AI Live không dùng live signal làm revenue

Given live có comment mua hàng, inbox, quote hoặc order draft
When Ads/Live dashboard tính hiệu quả
Then không được coi các signal này là revenue.

P0 Fail If: live signal được tính ROAS.

TECH10-FSMK-029 - MC AI Live không kéo troll/malicious vào Messenger sai policy

Given comment chửi rủa, phá live, công kích thương hiệu có dấu hiệu malicious
When Live Moderation xử lý
Then không trả lời public, không kéo Messenger, flag/hide/block theo policy.

P0 Fail If: troll/malicious được tạo order hoặc kéo vào Messenger như khách thường.

TECH10-FSMK-030 - Complaint thật không bị xử như troll

Given khách có mã đơn/mã lô/bằng chứng khiếu nại thật
When moderation/anti-fake-order xử lý
Then route Complaint/CSKH/Quality, không auto blacklist.

P0 Fail If: complaint thật bị Purchase Block sai.

## 10. FINAL GLOBAL SMOKE MATRIX - NHÓM H: IVR

TECH10-FSMK-031 - IVR không gọi đại trà

Given khách trusted, lịch sử nhận hàng tốt, không có rủi ro mới
When IVR Eligibility chạy
Then khách không bị gọi đại trà.

P0 Fail If: mọi khách đều bị IVR gọi.

TECH10-FSMK-032 - Invalid phone tách khỏi No Answer

Given số điện thoại invalid
When IVR xử lý
Then outcome = INVALID_PHONE, không phải NO_ANSWER.

P0 Fail If: invalid phone bị ghi là khách không nghe.

TECH10-FSMK-033 - Customer Cancel tách khỏi No Answer

Given khách bấm hủy qua IVR
When outcome được phân loại
Then outcome = CUSTOMER_CANCELLED_VIA_IVR.

P0 Fail If: khách bấm hủy bị ghi no answer.

TECH10-FSMK-034 - Technical Failure không quy lỗi khách

Given provider lỗi hoặc call setup failed
When IVR xử lý
Then outcome = TECHNICAL_FAILURE / PROVIDER_FAILURE.

P0 Fail If: lỗi kỹ thuật bị ghi thành khách không nghe.

TECH10-FSMK-035 - No Answer Max chỉ bàn giao Core Order State Machine

Given khách không nghe đủ 2 attempts hợp lệ
When IVR đạt max attempts
Then IVR chỉ handoff Core Order State Machine, không tự hủy, không tự gửi SMS.

P0 Fail If: IVR tự hủy hoặc tự nhắn.

TECH10-FSMK-036 - Notification chỉ sau Core cancellation

Given Core Order State Machine chưa hủy chính thức
When Notification Handoff Resolver chạy
Then không được gửi thông báo hủy.

P0 Fail If: Notification gửi trước Core cancellation.

## 11. FINAL GLOBAL SMOKE MATRIX - NHÓM I: PAYMENT / REVENUE / ADS / SCALE

TECH10-FSMK-037 - Payment WAITING không phải PAID

Given đơn đang payment WAITING
When Commerce/Ads/AI kiểm tra
Then không được coi là PAID.

P0 Fail If: payment WAITING được tính paid revenue.

TECH10-FSMK-038 - COD WAITING không phải Verified Revenue

Given đơn COD chưa giao/thu thành công
When Ads Measurement tính revenue
Then COD WAITING không được tính Verified Revenue.

P0 Fail If: COD WAITING được tính ROAS.

TECH10-FSMK-039 - Verified Revenue phải đến từ Commerce

Given order, payment, delivery, refund/cancel status
When Ads Measurement nhận revenue
Then chỉ dùng Verified Revenue từ Commerce.

P0 Fail If: Ads dùng dashboard hoặc signal thay source-of-truth.

TECH10-FSMK-040 - Không scale khi Data Quality fail

Given Ads data quality fail hoặc dedup/idempotency fail
When Scale Gate được xét
Then Scale Approved = KHONG.

P0 Fail If: scale khi data quality fail.

TECH10-FSMK-041 - Không scale khi Sale Lock / Recall active

Given SKU/batch có Sale Lock/Recall/Suppression
When Ads scale được xét
Then Scale Approved = KHONG.

P0 Fail If: scale sản phẩm bị lock/recall.

## 12. FINAL GLOBAL SMOKE MATRIX - NHÓM J: PRIVACY / SECURITY / AUDIT

TECH10-FSMK-042 - Public Trace whitelist-only

Given Public Trace được gọi
When trace data trả ra
Then chỉ trả dữ liệu whitelist.

P0 Fail If: public trace lộ supplier, cost, QC defect, MISA, private/internal data.

TECH10-FSMK-043 - Admin Override phải có audit

Given admin override trạng thái quan trọng
When override được thực hiện
Then phải có actor, permission, reason, evidence, before/after state, audit.

P0 Fail If: override không audit.

TECH10-FSMK-044 - Evidence/Audit bắt buộc cho high-risk command

Given high-risk command như release batch, warehouse receipt, order cancel, payment confirm, IVR outcome, recall/sale lock
When command chạy
Then phải có evidence/audit/correlation.

P0 Fail If: high-risk command không audit.

TECH10-FSMK-045 - Runtime unavailable thì fail-safe

Given source-of-truth runtime unavailable
When downstream cần quyết định
Then downstream không tự suy đoán, phải fail-safe.

P0 Fail If: runtime lỗi nhưng hệ thống tự bán/tự confirm/tự scale/tự gọi.

## 13. FINAL GLOBAL SMOKE MATRIX - NHÓM K: ROLLBACK / FALLBACK / MONITORING

TECH10-FSMK-046 - Production release phải có rollback/fallback

Given release scope vào production
When Rollback/Fallback Resolver kiểm tra
Then phải có rollback/fallback plan.

P0 Fail If: production go-live không rollback/fallback.

TECH10-FSMK-047 - Monitoring/Alert phải có owner

Given go-live scope có runtime critical
When Monitoring Resolver kiểm tra
Then alert critical phải có owner/escalation route.

P0 Fail If: alert critical không có người chịu trách nhiệm.

TECH10-FSMK-048 - Incident critical kích hoạt rollback/fallback review

Given post-go-live xuất hiện incident critical
When monitoring phát hiện
Then route owner, mở điểm chặn, xét rollback/fallback.

P0 Fail If: incident critical bị bỏ qua.

TECH10-FSMK-049 - Post-go-live incident chặn scale

Given post-go-live còn incident open
When Scale Approval được xét
Then Scale Approved = KHONG.

P0 Fail If: incident open nhưng vẫn scale.

## 14. FINAL GLOBAL SMOKE MATRIX - NHÓM L: GATEWAY / RELEASE DECISION

TECH10-FSMK-050 - Global Gateway mặc định bị chặn

Given hệ thống chưa đủ evidence/smoke/sign-off/release decision
When Global Gateway kiểm tra
Then trạng thái mặc định = bị chặn.

P0 Fail If: Gateway mặc định mở.

TECH10-FSMK-051 - Không release nếu thiếu release decision

P0 Fail If: không release decision nhưng go-live.

TECH10-FSMK-052 - Release decision phải có scope

Given release decision không ghi scope/environment/owner/rollback/monitoring
When Gateway kiểm tra
Then decision invalid.

P0 Fail If: decision mơ hồ vẫn mở Gateway.

TECH10-FSMK-053 - Gateway không được nhảy bước

Given trạng thái đang DOCUMENTATION_COMPLETE_WAITING_EVIDENCE
When chưa evidence accepted/smoke pass/sign-off/decision
Then không được nhảy sang GO_LIVE_DECISION_ACCEPTED.

P0 Fail If: Gateway transition nhảy bước.

TECH10-FSMK-054 - Evidence revoke phải kéo Gateway xuống

Given evidence từng accepted bị revoke
When release status được cập nhật
Then Gateway phải chuyển về bị chặn/WAITING/review theo severity.

P0 Fail If: evidence revoke nhưng Gateway vẫn pass.

## 15. FINAL EVIDENCE PACKAGE - TECH-10

TECH-10 yêu cầu evidence package cuối cùng gồm các nhóm sau.

## 15.1. Documentation Evidence

TECH-00 Documentation Complete evidence.

TECH-01 Documentation Complete evidence.

TECH-02 Documentation Complete evidence.

TECH-03 Documentation Complete evidence.

TECH-04 Documentation Complete evidence.

TECH-05 Documentation Complete evidence.

TECH-06 Documentation Complete evidence.

TECH-07 Documentation Complete evidence.

TECH-08 Documentation Complete evidence.

TECH-09 Documentation Complete evidence.

TECH-10 Documentation Complete evidence.

Source-of-truth registry.

Legacy reference boundary.

Current implementation boundary.

P0 điểm chặn Registry.

## 15.2. Evidence Acceptance Evidence

Evidence submission log.

Evidence review record.

Evidence accepted/rejected/WAITING status.

Requirement mapping.

Smoke ID mapping.

Environment mapping.

Reviewer identity.

Timestamp.

Sensitive data check.

Evidence integrity check.

Evidence revoke log nếu có.

Audit log.

## 15.3. Smoke Execution Evidence

Smoke matrix.

Smoke execution result.

Pass/fail status.

Failed smoke reason.

Retest evidence.

Cross-tech smoke result.

P0 smoke result.

Fail-safe smoke result.

Privacy/security smoke result.

Release smoke summary.

## 15.4. UAT Evidence

UAT scope.

UAT owner.

UAT scenario.

UAT environment.

Expected result.

Actual result.

Owner feedback.

UAT pass/fail.

UAT retest nếu có.

UAT acceptance record.

## 15.5. Owner Sign-off Evidence

Owner identity.

Domain.

Release scope.

Evidence reviewed.

Smoke reviewed.

UAT reviewed nếu có.

Decision.

Condition nếu có.

Timestamp.

Audit log.

## 15.6. Cross-Tech Dependency Evidence

Dependency pair.

Upstream status.

Downstream status.

Validation result.

bị chặn downstream list.

Violation list nếu có.

Owner review.

Resolution evidence.

Audit log.

## 15.7. Privacy / Security Evidence

Privacy checklist.

Security checklist.

RBAC check.

Admin override audit check.

Public/private boundary check.

Evidence sensitive data check.

Secret/token exposure check.

Payment/member/Diamond/health data check.

IVR call script privacy check.

Notification privacy check.

Reviewer decision.

Audit log.

## 15.8. Rollback / Fallback Evidence

Rollback plan.

Fallback plan.

Trigger criteria.

Stop switch / suppression / kill switch nếu áp dụng.

Owner assignment.

Communication plan.

Data safety note.

Test evidence nếu có.

Approval.

Audit log.

## 15.9. Monitoring / Alert Evidence

Monitoring checklist.

Alert checklist.

Critical metric list.

Alert owner.

Escalation route.

Test alert evidence.

Incident routing evidence.

Evidence write failure alert.

Payment/revenue mismatch alert.

IVR/Gateway/AI/Ads/Live critical alert.

Audit log.

## 15.10. Release Decision Evidence

Release Decision Board record.

Release scope.

Environment.

Decision type.

Decision owner.

Evidence summary.

Smoke summary.

UAT summary nếu có.

Owner sign-off summary.

Open risk summary.

Rollback/fallback reference.

Monitoring reference.

Incident owner.

Gateway target state.

Audit log.

## 15.11. Post-Go-Live Evidence

Monitoring window.

Runtime metrics.

Alert log.

Incident log.

Owner response.

Customer complaint trend nếu scope liên quan.

Rollback/fallback action nếu có.

Stability assessment.

Scale readiness recommendation.

Audit log.

## 16. OWNER REVIEW POINTS

## 16.1. Global Release Owner

Global Release Owner phải xác nhận:

TECH-10 đủ 4 phần.

Global Gateway mặc định bị chặn.

Documentation Complete không phải Production Ready.

Evidence accepted mới xét Completion Decision.

Smoke pass mới xét Release Ready.

Owner sign-off mới xét Release Approved.

Release decision mới xét Go-live Approved.

Cross-tech dependency đã được kiểm tra.

P0 điểm chặn còn mở thì không release.

## 16.2. Foundation / Security / RBAC Owner

Owner này phải xác nhận:

RBAC đúng.

Audit đúng.

Evidence registry đúng.

Admin override có audit.

High-risk command có trace.

Secret/token không lộ.

Idempotency/risk command không bị double action.

## 16.3. Product / SKU / Recipe Owner

Owner này phải xác nhận:

Product Active không bị hiểu là Sellable.

SKU/Recipe source-of-truth đúng.

Claim/product data approved.

Không dùng SKU nội bộ sai kênh public.

Product activation gate không bị bypass.

Sale/AI/Live/Gateway không dùng dữ liệu chưa approved.

## 16.4. Operational / Warehouse / Trace / Recall Owner

Owner này phải xác nhận:

Operational Sellable Gate đúng.

Inventory/warehouse status đúng.

QC_PASS không bị hiểu là RELEASED.

Sale Lock/Recall chặn downstream.

Public Trace whitelist-only.

Warehouse receipt / inventory ledger / trace evidence đúng.

## 16.5. Commerce / Payment / Shipping Owner

Owner này phải xác nhận:

QuoteSnapshot là nguồn báo giá.

Cart không phải order.

Order Draft không phải Official Order.

Official Order không phải PAID.

Payment Confirmation mới được PAID.

COD WAITING không phải Verified Revenue.

Verified Revenue đúng source-of-truth.

Shipping/payment/order state không bị AI/Gateway/IVR override.

## 16.6. AI Advisor Owner

Owner này phải xác nhận:

AI không tự tính giá.

AI không tự tạo official order.

AI không tự xác nhận payment.

AI không public dữ liệu private.

AI không dùng claim chưa approved.

AI không bán SKU not sellable/Sale Lock/Recall.

Final Response Guard hoạt động.

## 16.7. Facebook Gateway Owner

Owner này phải xác nhận:

Public/private routing đúng.

Comment hỏi giá/mua/chốt kéo Messenger/private theo rule.

Gateway không gửi response chưa guard.

Gateway có pacing/typing indicator.

Gateway không giả mạo người thật.

Rate limit/moderation/suppression đúng.

Không public dữ liệu nhạy cảm.

## 16.8. Ads Measurement Owner

Owner này phải xác nhận:

Ads chỉ dùng Verified Revenue.

Quote/cart/order draft/unpaid không phải revenue.

Pixel/CAPI dedup đúng.

Attribution conflict xử lý đúng.

Data Quality pass.

Scale không mở khi evidence/smoke/Gateway chưa pass.

Sale Lock/Recall/Suppression chặn scale.

## 16.9. MC AI Live Owner

Owner này phải xác nhận:

MC AI Live không tự báo giá.

MC AI Live không tự tạo order.

MC AI Live không xác nhận payment/revenue.

Script đã guard.

Troll/abuse xử lý đúng.

Complaint thật route đúng.

Live signal không dùng làm ROAS.

## 16.10. IVR Owner

Owner này phải xác nhận:

IVR không gọi đại trà.

IVR chỉ gọi official order đủ điều kiện.

Khách trusted không gọi đại trà.

Invalid phone/no answer/customer cancel/technical failure tách rõ.

## MAX_ATTEMPT_PER_ORDER = PROGRAM_BASED: Golden Hour 2 attempts / 10 ph?t; 24/7 3 attempts / 15 ph?t.

IVR không tự hủy/tự nhắn.

Notification chỉ sau Core cancellation.

Call log/evidence/audit đầy đủ.

## 16.11. Notification / CRM Owner

Owner này phải xác nhận:

Notification transactional khác CRM/marketing.

Notification sau hủy không upsell.

SIM Gateway không tự gửi tin hủy.

AI Advisor không tự phát tin hủy.

Notification privacy-safe.

Opt-out/suppression/legal block được tôn trọng.

## 16.12. Privacy / Legal Owner

Owner này phải xác nhận:

Public/private boundary đúng.

Evidence không lộ dữ liệu nhạy cảm.

IVR không đọc full address/payment/member/Diamond/health note sai policy.

Notification không chứa dữ liệu cấm.

Call log/recording không dùng marketing.

Health note không lộ sai kênh.

Public Trace không lộ dữ liệu nội bộ.

## 16.13. QA / UAT Owner

Owner này phải xác nhận:

Smoke matrix đủ pass-case/fail-case.

Cross-tech smoke đủ.

UAT có owner nghiệp vụ.

Retest evidence đủ.

P0 điểm chặn closed hợp lệ.

Không có smoke/UAT pass bằng miệng.

## 17. FINAL REVIEW GATES

## 17.1. Documentation Review Gate

PASS khi:

TECH đủ phần.

Scope rõ.

Boundary rõ.

Dependency rõ.

P0 điểm chặn rõ.

Evidence rõ.

Smoke rõ.

Done Gate rõ.

Fail Gate rõ.

Release Handoff rõ.

FAIL nếu tài liệu còn thiếu gate hoặc mâu thuẫn source-of-truth.

## 17.2. Evidence Review Gate

PASS khi:

Evidence đủ.

Evidence đúng format.

Evidence đúng environment.

Evidence gắn requirement/smoke.

Evidence không lộ dữ liệu cấm.

Reviewer accepted.

Audit đầy đủ.

FAIL nếu evidence chỉ là lời nói, screenshot rời rạc hoặc log không trace được requirement.

## 17.3. Smoke Review Gate

PASS khi:

Smoke đã chạy.

Smoke pass.

P0 smoke pass.

Fail-case pass.

Cross-tech smoke pass.

Evidence đủ.

Retest pass nếu có fail trước đó.

FAIL nếu smoke chưa chạy, thiếu evidence hoặc chỉ test happy path.

## 17.4. UAT Review Gate

PASS khi:

UAT scope rõ.

Owner nghiệp vụ tham gia.

Scenario thực tế.

Feedback được xử lý.

UAT pass.

Owner accepted.

FAIL nếu dev tự ký UAT hoặc UAT thiếu nghiệp vụ thật.

## 17.5. Owner Sign-off Gate

PASS khi:

Đúng owner.

Đúng scope.

Evidence reviewed.

Smoke reviewed.

UAT reviewed nếu có.

Không còn P0 điểm chặn.

Sign-off có audit.

FAIL nếu thiếu owner critical hoặc sign-off cảm tính.

## 17.6. Cross-Tech Dependency Gate

PASS khi:

Upstream pass.

Downstream không bypass.

Sale Lock/Recall chặn đúng.

Verified Revenue đúng nguồn.

QuoteSnapshot đúng nguồn.

Final Response Guard đúng.

IVR official order boundary đúng.

FAIL nếu bất kỳ downstream nào tự vượt upstream.

## 17.7. Privacy / Security Gate

PASS khi:

Không lộ dữ liệu private.

Secret/token không lộ.

RBAC đúng.

Admin override audit đúng.

Evidence privacy đúng.

Notification/IVR/Gateway/AI public/private đúng.

Public Trace whitelist-only.

FAIL nếu privacy/security critical còn mở.

## 17.8. Production Readiness Gate

PASS khi:

Documentation complete.

Evidence accepted.

Smoke pass.

UAT pass nếu required.

Owner sign-off.

No P0 điểm chặn.

Dependency pass.

Privacy/security pass.

Rollback/fallback ready.

Monitoring/alert ready.

PASS ở gate này chỉ là Production Ready Recommendation, chưa phải Go-live Approved.

## 17.9. Release Decision Gate

PASS khi:

Release scope rõ.

Environment rõ.

Release decision rõ.

Owner quyết định rõ.

Rollback/fallback reference có.

Monitoring reference có.

Incident owner có.

Gateway target state có.

FAIL nếu không có release decision chính thức.

## 17.10. Go-live Gate

PASS khi:

Production readiness recommended.

Release decision approved for production go-live.

Global Gateway chuyển GO_LIVE_DECISION_ACCEPTED hợp lệ.

Monitoring window mở.

Rollback/fallback ready.

Incident owner ready.

FAIL nếu còn P0 điểm chặn, thiếu decision hoặc thiếu monitoring.

## 17.11. Post-Go-Live Gate

PASS khi:

Monitoring active.

Incident được theo dõi.

Alert route owner.

No critical incident open.

Evidence tiếp tục ghi nhận.

Release scope ổn định.

Owner review sau go-live nếu required.

FAIL nếu go-live xong không monitoring.

## 17.12. Scale Approval Gate

PASS khi:

Post-go-live ổn định.

Verified Revenue đúng.

Data Quality pass.

No Sale Lock / Recall / Suppression.

No privacy/security incident.

Owner approve.

Stop-scale rule có.

Scale decision rõ.

FAIL nếu scale vì ROAS đẹp nhưng dữ liệu chưa đạt.

## 18. FINAL DONE GATE - TECH-10

TECH-10 đạt DOCUMENTATION COMPLETE khi:

## PHẦN 1/4 hoàn tất.

## PHẦN 2/4 hoàn tất.

## PHẦN 3/4 hoàn tất.

## PHẦN 4/4 hoàn tất.

Global Release Principles đã khóa.

No-Fake-Ready Rule đã khóa.

Global Source-of-Truth Boundary đã khóa.

Global Gateway Boundary đã khóa.

Documentation Complete Boundary đã khóa.

Evidence Boundary đã khóa.

Smoke Boundary đã khóa.

UAT Boundary đã khóa.

Owner Sign-off Boundary đã khóa.

Release Decision Boundary đã khóa.

Cross-Tech Dependency Principle đã khóa.

Module Contracts đã khóa.

Lifecycle / State Machine đã khóa.

Final Global Smoke Matrix đã khóa.

Final Evidence Package đã khóa.

Owner Review Points đã khóa.

Final Review Gates đã khóa.

Final Done Gate đã khóa.

Final Fail Gate đã khóa.

Release Handoff đã khóa.

Final Conclusion đã khóa.

## 19. FINAL FAIL GATE - TECH-10

TECH-10 FAIL nếu còn bất kỳ điểm nào sau:

Documentation Complete bị gọi là Production Ready.

Dev Completed bị gọi là Release Ready.

Evidence Submitted bị gọi là Evidence Accepted.

Evidence chưa accepted nhưng Completion Decision.

Smoke chưa chạy nhưng Release Ready.

Smoke fail nhưng Release Ready.

Smoke chỉ test happy path.

Cross-tech smoke thiếu.

UAT do dev tự ký.

Owner sign-off không evidence.

Owner sign-off không rõ scope.

P0 điểm chặn còn mở nhưng release.

điểm chặn đóng không retest evidence.

Dependency upstream fail nhưng downstream release.

Privacy/security fail nhưng go-live.

Rollback/fallback missing nhưng production release.

Monitoring/alert missing nhưng go-live.

Release decision thiếu nhưng Go-live Approved.

Release decision không scope.

Global Gateway không mặc định bị chặn.

Gateway transition nhảy bước.

Gateway không có reason code.

Evidence bị revoke nhưng Gateway không cập nhật.

Post-go-live incident open nhưng scale.

Verified Revenue fail nhưng Ads scale.

Data Quality fail nhưng Ads scale.

Sale Lock/Recall active nhưng Commerce/AI/Ads/Live/IVR vẫn hoạt động.

AI tự báo giá/tạo order/xác nhận payment.

Gateway public dữ liệu private.

MC AI Live dùng live signal làm ROAS.

IVR gọi Quote/Cart/Order Draft.

IVR tự gửi notification hủy đơn.

Admin override không audit.

Runtime unavailable nhưng downstream tự suy đoán.

Evidence chứa secret/dữ liệu nhạy cảm sai policy.

Scale Approved khi Global Gateway chưa pass.

## 20. RELEASE HANDOFF - SAU TECH-10

## 20.1. Trạng thái sau khi hoàn tất TECH-10

Sau khi hoàn tất PHẦN 4/4:

Nhưng:

Global Gate bị chặn

## 20.2. Điều kiện nâng lên Completion Decision

Muốn nâng lên Completion Decision cần:

Evidence package được nộp.

Evidence được review.

Evidence accepted.

Evidence không vi phạm privacy/security.

Evidence gắn requirement/smoke/environment.

Không còn P0 điểm chặn ở cấp tài liệu/evidence.

Owner xác nhận evidence đủ cho scope.

Nếu chưa accepted evidence:

## 20.3. Điều kiện nâng lên Release Ready

Muốn nâng lên Release Ready cần:

Completion Decision.

Smoke execution đã chạy.

P0 smoke pass.

Cross-tech smoke pass.

Fail-case smoke pass.

Runtime unavailable smoke pass.

Privacy/security smoke pass.

Retest evidence pass nếu có lỗi trước đó.

Không còn điểm chặn mở.

Nếu smoke chưa pass:

## 20.4. Điều kiện nâng lên Release Approved

Muốn nâng lên Release Approved cần:

Release Ready.

Owner sign-off đủ theo scope.

Privacy/Legal sign-off nếu scope có dữ liệu khách hàng.

Security/RBAC sign-off nếu scope có quyền/audit.

Operational/Commerce/AI/Gateway/Ads/Live/IVR owner sign-off theo scope.

QA/UAT sign-off nếu áp dụng.

No open P0 điểm chặn.

Release Owner xác nhận.

Nếu owner chưa sign-off:

## 20.5. Điều kiện nâng lên Go-live Approved

Muốn nâng lên Go-live Approved cần:

Release Approved.

Production readiness recommended.

Rollback/fallback ready.

Monitoring/alert ready.

Incident owner assigned.

Release decision chính thức.

Global Gateway State Controller chuyển trạng thái hợp lệ.

Nếu chưa có release decision:

## 20.6. Điều kiện nâng lên Scale Approved

Muốn nâng lên Scale Approved cần:

Go-live Approved đã thực thi đúng scope.

Post-go-live monitoring ổn định.

No critical incident open.

Verified Revenue đúng.

Data Quality pass.

No Sale Lock / Recall / Suppression.

Ads/Commerce/Operational owner approval nếu scale Ads/commerce.

Stop-scale rule ready.

Scale decision rõ.

Nếu post-go-live chưa ổn định hoặc Data Quality fail:

## 21. HANDOFF SANG TRIỂN KHAI THỰC TẾ

TECH-10 bàn giao cho quá trình triển khai thực tế các yêu cầu sau:

## 21.1. Bàn giao cho Dev

Dev không được hiểu tài liệu là code.

Dev cần:

Phân tích source-of-truth.

Thiết kế kiến trúc triển khai.

Thiết kế database/API/UI theo giai đoạn được giao.

Bảo đảm dependency đúng.

Bảo đảm evidence/audit.

Bảo đảm smoke pass.

Không copy-paste code rời rạc từ AI.

Không tự hardcode chính sách nghiệp vụ.

Không tự bỏ qua gate.

## 21.2. Bàn giao cho QA

QA cần:

Lập smoke matrix theo TECH-10.

Lập cross-tech smoke.

Lập fail-case smoke.

Lập UAT scenario.

Gắn evidence với requirement.

Theo dõi P0 điểm chặn.

Retest sau fix.

Không pass bằng cảm tính.

## 21.3. Bàn giao cho Owner nghiệp vụ

Owner cần:

Review evidence.

Review smoke.

Tham gia UAT.

Ký sign-off đúng scope.

Không ký khi chưa đủ evidence.

Không ký khi còn P0 điểm chặn.

Xác nhận rủi ro nếu release có điều kiện.

## 21.4. Bàn giao cho Release Owner

Release Owner cần:

Kiểm tra toàn bộ gate.

Kiểm tra owner sign-off.

Kiểm tra rollback/fallback.

Kiểm tra monitoring/alert.

Kiểm tra release decision.

Điều khiển Global Gateway.

Không mở Gateway khi thiếu điều kiện.

Không gọi Documentation Complete là Production Ready.

## 21.5. Bàn giao cho Operations

Operations cần:

Chuẩn bị vận hành.

Chuẩn bị quy trình fallback.

Chuẩn bị người nhận alert.

Chuẩn bị xử lý incident.

Chuẩn bị CSKH/Complaint/IVR/Notification flow.

Theo dõi post-go-live.

Báo cáo bất thường.

Hạng mục

Trạng thái sau TECH-10

TECH-10 Documentation

Completion Decision

## WAITING_EVIDENCE

Evidence Accepted

KHONG

Smoke Pass

KHONG

UAT Pass

KHONG / NOT EVALUATED

Owner Sign-off

KHONG

Production Ready

KHONG

Release Ready

KHONG

Release Approved

KHONG

Go-live Approved

KHONG

Scale Approved

KHONG

Global Gateway

bị chặn

Rollback/Fallback Ready

## REQUIRED

Monitoring/Alert Ready

## REQUIRED

Release Decision

## REQUIRED

Post-Go-Live Monitoring

## REQUIRED AFTER GO-LIVE

## TECH-10 FINAL CONCLUSION

TECH-10 đã khóa lớp Global Smoke / UAT / Evidence / Release Gateway / Production Readiness Control cho toàn bộ hệ thống Ginsengfood.

TECH-10 không thay thế TECH-00 -> TECH-09.

TECH-10 không viết code.

TECH-10 không cho phép gọi tài liệu hoàn chỉnh là hệ thống đã sẵn sàng vận hành.

Các nguyên tắc cuối cùng đã khóa:

Documentation Complete không phải Production Ready.

Dev Completed không phải Release Ready.

Evidence Submitted không phải Evidence Accepted.

Evidence chưa accepted thì không Completion Decision.

Smoke chưa pass thì không Release Ready.

UAT phải có owner nghiệp vụ nếu scope yêu cầu.

Owner chưa sign-off thì không Release Approved.

Chưa có release decision thì không Go-live Approved.

P0 điểm chặn còn mở thì Global Gateway bị chặn.

Cross-tech dependency fail thì downstream bị chặn.

Privacy/Security fail thì release bị chặn.

Rollback/Fallback missing thì production release bị chặn.

Monitoring/Alert missing thì go-live bị chặn.

Post-go-live chưa ổn định thì chưa Scale Approved.

Verified Revenue/Data Quality fail thì Ads không được scale.

Sale Lock/Recall/Suppression thắng Commerce, AI, Ads, Live, Gateway và IVR.

Evidence phải gắn requirement, smoke, environment, expected/actual, reviewer và audit.

Smoke phải có pass-case, fail-case và cross-tech.

Owner sign-off phải rõ scope, evidence và trách nhiệm.

Release Decision Board là nơi ra quyết định chính thức.

Global Gateway State Controller là trạng thái cuối.

Runtime unavailable thì fail-safe, không tự suy đoán.

Evidence revoke thì release/gateway status phải cập nhật lại.

Critical incident sau go-live phải route owner và có thể kích hoạt rollback/fallback.

Scale chỉ được xét sau go-live ổn định, data quality pass, verified revenue đúng, owner approve và stop-scale ready.

Trạng thái cuối cùng của tài liệu:

Nhưng trạng thái vận hành vẫn là:

TECH-10 chỉ cho phép hệ thống tiến tới release thật khi có đủ:

Accepted evidence.

Smoke pass.

UAT pass nếu scope yêu cầu.

Owner sign-off.

No open P0 điểm chặn.

Cross-tech dependency pass.

Privacy/Security pass.

Rollback/Fallback ready.

Monitoring/Alert ready.

Release decision.

Global Gateway approval.

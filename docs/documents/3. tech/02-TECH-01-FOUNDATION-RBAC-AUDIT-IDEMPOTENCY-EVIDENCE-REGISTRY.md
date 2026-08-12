# TECH-01 - FOUNDATION RBAC AUDIT IDEMPOTENCY EVIDENCE REGISTRY

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## TECHNICAL FOUNDATION / ACCESS CONTROL / AUDIT TRAIL / IDEMPOTENT COMMAND / COMPLETION EVIDENCE BASE

## PHẦN 1/4 - FOUNDATION PRINCIPLES / CORE PLATFORM BOUNDARY / SECURITY BASELINE / NO-BYPASS FOUNDATION RULE

## 1. MỤC ĐÍCH CỦA TECH-01

TECH-01 là tài liệu kỹ thuật nền đầu tiên sau TECH-00.

TECH-01 khóa lớp Foundation của hệ thống Ginsengfood trước khi triển khai Product, Operational Core, Commerce Runtime, AI Advisor, Facebook Gateway, Ads Measurement, MC AI Live và IVR.

Foundation không phải một module phụ.
Foundation không phải phần có thể làm sau.
Foundation không phải chỉ gồm đăng nhập và phân quyền đơn giản.

Foundation là lớp nền giúp toàn hệ thống kiểm soát:

Ai được vào hệ thống.

Ai được làm hành động nào.

Hành động nào phải kiểm tra quyền.

Hành động nào phải ghi audit.

Hành động nào phải chống submit lặp.

Evidence được lưu và review thế nào.

P0 điểm chặn được ghi nhận thế nào.

Release status được quản lý thế nào.

Owner decision được ghi nhận thế nào.

Module nào chưa đủ điều kiện thì bị bị chặn thế nào.

Nếu Foundation sai, toàn bộ hệ thống phía sau sẽ sai:

Product có thể bị sửa sai.

Batch có thể release sai.

Inventory có thể lệch.

Payment có thể mark PAID sai.

AI có thể vượt quyền.

Facebook Gateway có thể tạo order sai.

Ads có thể lấy revenue sai.

IVR có thể hủy đơn sai.

Evidence có thể bị hợp thức hóa bằng tay.

Release có thể bị gọi PASS khi chưa đủ điều kiện.

Vì vậy TECH-01 là nền bắt buộc trước TECH-02 đến TECH-10.

## 2. VAI TRÒ CỦA FOUNDATION TRONG KIẾN TRÚC GINSENGFOOD

## 2.1. Foundation là lớp kiểm soát chung

Foundation cung cấp các năng lực nền cho toàn bộ hệ thống:

User.

Role.

Permission.

Authentication.

Authorization.

Audit trail.

Idempotency.

Runtime configuration base.

Evidence registry.

P0 điểm chặn registry.

Release status registry.

Owner decision record.

Security baseline.

Environment boundary.

System governance metadata.

Foundation không xử lý nghiệp vụ chi tiết của từng domain.

Foundation không tự quyết:

SKU nào được bán.

Batch nào được release.

Order nào được paid.

Revenue nào verified.

Claim nào được nói.

IVR result nào làm hủy đơn.

Ads nào được scale.

Những quyết định đó thuộc các domain tương ứng.

Foundation chỉ cung cấp lớp kiểm soát quyền, audit, evidence, điểm chặn và release status để các domain vận hành đúng.

## 2.2. Foundation là upstream bắt buộc

Các module sau đều phụ thuộc Foundation:

Module / Pack

Phụ thuộc Foundation

Product / SKU / Recipe

Role, permission, audit, version change tracking

Operational Core

Permission, audit, idempotency, scan/action evidence

Demand / MRP / Procurement

Approval, audit, điểm chặn, owner decision

Commerce Runtime

Permission, audit, idempotency, payment/revenue evidence

AI Advisor

Runtime access, guard evidence, no-hardcode evidence

Facebook Gateway

Channel security, audit, privacy evidence

Ads Measurement

Evidence, data quality, owner scale decision

MC AI Live

Script approval, live evidence, incident control

## IVR

Security, call result evidence, no-direct-order-update proof

Completion / Release

Evidence registry, điểm chặn registry, release status

Nếu Foundation chưa đạt, các domain phía sau không được production.

## 3. TECH-01 SOURCE-OF-TRUTH

## 3.1. Nguồn chính

TECH-01 dựa trên các nguồn đã khóa:

MASTER governance documents.

PACK-01 đến PACK-10.

## DEV EXECUTION COMMAND PACK.

## TECH-00 - Technical Implementation Master Plan.

Business Pack liên quan role, permission, audit, approval.

Functional Pack liên quan acceptance criteria.

Owner decision mới nếu có.

Không dùng tài liệu cũ làm nền.
Không dùng code cũ làm business truth.
Không dùng UI cũ để quyết định permission.
Không dùng dashboard cũ để quyết định evidence/release.

## 3.2. TECH-01 không thay thế domain pack

TECH-01 không thay:

PACK-01 Operational Core.

PACK-02 Product / SKU / Recipe.

PACK-03 Demand / MRP / Procurement.

Commerce Runtime.

PACK-06 Facebook Gateway.

PACK-05 AI Advisor.

PACK-07 Ads Measurement.

PACK-08 MC AI Live.

## PACK-09 IVR.

PACK-10 Completion / Evidence / Gateway / Release Readiness.

TECH-01 chỉ cung cấp nền kiểm soát chung.

Nếu có conflict giữa TECH-01 và pack domain, phải đưa vào:

## OWNER_REVIEW_REQUIRED

Không tự sửa, không tự chọn theo hướng dễ làm.

## 4. FOUNDATION CORE MODULES

TECH-01 quản trị 8 nhóm module nền.

## 4.1. User / Identity Module

Quản lý danh tính người dùng nội bộ, người dùng hệ thống, tài khoản tích hợp và tài khoản bên ngoài nếu scope có.

Bao gồm:

User identity.

Account status.

Login boundary.

Session/token boundary.

User profile nội bộ.

Link user với role.

Link user với organization/supplier nếu có.

Account lock/disable.

Module này không quyết định nghiệp vụ.
User có quyền hay không phải qua Role/Permission.

## 4.2. Role Module

Quản lý vai trò.

Vai trò không được viết tự do trong code.
Vai trò phải là cấu hình có kiểm soát, có thể audit và review.

Ví dụ nhóm vai trò:

System Admin.

Owner / Director.

Production Manager.

## QC.

Warehouse.

Sales Admin.

Commerce Admin.

Accounting / Payment Review.

Marketing / Ads.

AI Content / Claim Review.

Facebook Channel Operator.

MC Live Operator.

IVR Operator.

Supplier User nếu scope có.

Tên role cụ thể sẽ khóa ở phần sau, nhưng nguyên tắc là:

Role chỉ là nhóm quyền. Permission mới là điểm enforce hành động.

## 4.3. Permission Module

Permission quyết định hành động nào được phép thực hiện.

Không được chỉ dựa vào role name để xử lý logic quan trọng.

Ví dụ:

batch.release;

payment.confirm;

order.cancel;

evidence.accept;

release.approve;

sale_lock.apply;

recall.create;

ivr.call_control;

ads.scale_approve;

product.activate.

Permission phải được enforce ở backend/application layer, không chỉ ở UI.

## 4.4. Audit Module

Audit ghi nhận hành động quan trọng.

Audit không phải log kỹ thuật thông thường.
Audit là lịch sử nghiệp vụ/kiểm soát.

Audit bắt buộc cho:

State transition.

Approval/rejection.

Payment confirmation.

Batch release.

Recall.

Sale lock.

Order cancellation.

Evidence review.

Release decision.

Admin override.

Permission/role change.

IVR result ingestion.

MISA/payment sync outcome.

Public trace publish/unpublish.

AI/Claim guard exception nếu có.

Audit không được sửa lịch sử.

Nếu cần correction, phải tạo audit record mới.

## 4.5. Idempotency Module

Idempotency bảo đảm command rủi ro không tạo kết quả trùng khi retry.

Bắt buộc với các hành động:

Create quote.

Create order.

Confirm payment.

Cancel order.

Issue material.

Receive warehouse.

Release batch.

Apply recall.

Apply sale lock.

Submit IVR result.

Send notification.

Sync MISA.

Accept evidence.

Approve release.

Nếu không có idempotency, một lỗi retry có thể tạo trùng order, trừ kho nhiều lần, gửi tin nhiều lần hoặc ghi nhận payment sai.

## 4.6. Evidence Registry Module

Evidence Registry quản lý bằng chứng triển khai, test, smoke, security, owner review và release.

Evidence Registry không tự làm PASS domain nào.

Evidence Registry chỉ ghi nhận:

Evidence ID.

Pack/module liên quan.

Scope.

Environment.

Version/build/config.

Test/smoke case.

Result.

Submitted by.

Reviewed by.

Evidence status.

Owner decision nếu có.

Chỉ evidence có trạng thái ACCEPTED mới được dùng để xét PASS.

## 4.7. P0 điểm chặn Registry Module

P0 điểm chặn Registry quản lý các lỗi/rủi ro chặn release.

P0 điểm chặn không được xử lý bằng lời nói miệng.

Mỗi P0 điểm chặn phải có:

điểm chặn ID.

Module.

Pack/source rule.

Severity.

Impact.

Owner.

Status.

Fix evidence.

Retest evidence.

Review result.

Release impact.

Nếu P0 điểm chặn còn OPEN:

## 4.8. Release Status Registry Module

Release Status Registry quản lý trạng thái triển khai, test, smoke, evidence, release và go-live của từng module/scope.

Các trạng thái được phép dùng:

## NOT_STARTED.

## SPEC_COMPLETE.

## IMPLEMENTATION_WAITING.

## IMPLEMENTATION_COMPLETE_ONLY.

## TEST_WAITING.

## SMOKE_WAITING.

## EVIDENCE_WAITING.

## P0_BLOCKED.

## DEPENDENCY_BLOCKED.

## OWNER_REVIEW_REQUIRED.

## RELEASE_READY.

## RELEASE_DECISION_ACCEPTED.

## PRODUCTION_READINESS.

## GO_LIVE_DECISION_ACCEPTED.

## RELEASE_HOLD.

## ROLLBACK_REQUIRED.

## EXCLUDED_FROM_SCOPE.

Không dùng trạng thái mơ hồ như “gần xong”, “chạy được”, “pass miệng”.

## 5. FOUNDATION BOUNDARY

## 5.1. Foundation được phép làm

Foundation được phép:

Quản lý user/role/permission.

Enforce permission cho command quan trọng.

Ghi audit.

Cung cấp idempotency cho command rủi ro.

Quản lý evidence.

Quản lý P0 điểm chặn.

Quản lý release status.

Ghi owner decision.

Cung cấp security baseline.

Cung cấp environment boundary.

Cung cấp runtime config base.

Cung cấp metadata dùng chung.

## 5.2. Foundation không được làm

Foundation không được:

Quyết định SKU sellable.

Quyết định batch release.

Quyết định tồn kho khả dụng.

Quyết định quote price.

Quyết định order paid.

Quyết định verified revenue.

Quyết định claim AI được nói.

Quyết định Facebook handoff.

Quyết định Ads scale.

Quyết định MC Live script.

Quyết định IVR hủy đơn.

Quyết định MISA accounting truth.

Thay owner pack để PASS module.

Tự gọi production ready.

Foundation là lớp kiểm soát, không phải domain owner của mọi nghiệp vụ.

## 6. FOUNDATION SECURITY BASELINE

## 6.1. Nguyên tắc security nền

Foundation phải bảo đảm:

Không ai truy cập hệ thống nếu chưa xác thực.

Không ai thực hiện action nếu không có permission.

Không command rủi ro nào không audit.

Không command rủi ro nào thiếu idempotency.

Không evidence nào tự accepted.

Không P0 điểm chặn nào bị xóa im lặng.

Không release nào được approve bằng lời nói miệng.

Không role/permission nào được sửa không audit.

Không public dữ liệu nội bộ.

Không dùng dữ liệu thật ở môi trường chưa được duyệt.

## 6.2. Security-sensitive actions

Các hành động sau là security-sensitive:

Create/update/delete user.

Assign/remove role.

Grant/revoke permission.

Reset password / disable account.

Confirm payment.

Cancel order.

Release batch.

Adjust inventory.

Apply sale lock.

Apply recall.

Accept evidence.

Approve release.

Override state.

Export data.

Access customer/payment/private data.

Control IVR calling.

Publish public trace.

Sync MISA/payment.

Security-sensitive action phải có permission, audit và owner review nếu rủi ro cao.

## 7. RBAC PRINCIPLES

## 7.1. RBAC là nền, không phải danh sách role đơn giản

RBAC trong Ginsengfood phải xử lý:

User.

Role.

Permission.

Scope.

Environment.

Channel.

Supplier ownership nếu có.

Approval level.

Audit trail.

Separation of duties nếu cần.

Không chỉ là “admin/user”.

## 7.2. Permission phải ở cấp action

Không dùng role name để hardcode logic.

Sai:

Nếu role = Admin thì cho release batch.

Đúng:

User phải có permission batch.release và thỏa guard của Batch Release.

Role chỉ gom permission.
Permission mới enforce action.

## 7.3. Least privilege

Mỗi user chỉ được quyền vừa đủ.

Ví dụ:

QC có thể ghi kết quả QC nhưng không tự release batch nếu rule yêu cầu phê duyệt.

Warehouse có thể nhận hàng đã RELEASED nhưng không tự đổi QC_PASS thành RELEASED.

Ads user xem measurement nhưng không tự duyệt scale nếu không có quyền.

IVR operator xem call result nhưng không cập nhật order trực tiếp.

AI content reviewer duyệt content/claim nhưng không xác nhận payment.

Accounting xác nhận payment nhưng không sửa quote hoặc product formula.

## 8. AUDIT PRINCIPLES

## 8.1. Audit là bắt buộc cho hành động rủi ro

Không có audit thì không release production.

Audit phải chứng minh:

Ai làm.

Làm lúc nào.

Làm hành động gì.

Làm trên object nào.

Trạng thái trước là gì.

Trạng thái sau là gì.

Vì sao làm.

Kết quả là gì.

Có correlation/idempotency không.

Có thất bại không.

## 8.2. Audit không được sửa lịch sử

Nếu cần sửa, phải tạo correction record.

Không được:

Sửa audit cũ.

Xóa audit cũ.

Ghi đè audit cũ.

Làm sạch audit để hợp thức hóa release.

Sửa evidence review lịch sử.

Lịch sử phải append-only theo nguyên tắc quản trị.

## 9. IDEMPOTENCY PRINCIPLES

## 9.1. Vì sao idempotency là P0

Trong hệ thống có order, payment, kho, IVR, notification, MISA, nếu retry không được kiểm soát sẽ gây lỗi nghiêm trọng:

Tạo trùng đơn.

Gửi trùng tin.

Trừ kho nhiều lần.

Xác nhận payment nhiều lần.

Nhập kho nhiều lần.

Release batch nhiều lần.

Submit IVR result nhiều lần.

Sync MISA trùng.

Accept evidence trùng.

Approve release trùng.

Vì vậy idempotency là nền P0.

## 9.2. Idempotency không chỉ là technical retry

Idempotency phải gắn với business command.

Ví dụ:

submit cùng một IVR result không được tạo nhiều quyết định;

confirm cùng một payment không được mark paid nhiều lần;

issue material không được trừ tồn nhiều lần;

send cancellation notice không được gửi nhiều lần;

approve release không được tạo nhiều quyết định release mâu thuẫn.

## 10. EVIDENCE REGISTRY PRINCIPLES

## 10.1. Evidence Registry là nền để chứng minh Done

Không có evidence, không có PASS.

Evidence Registry phải phục vụ:

Implementation evidence.

Test evidence.

Smoke evidence.

Security evidence.

P0 fix evidence.

Retest evidence.

Owner review evidence.

Release decision evidence.

Go-live decision evidence.

Evidence không được là file rời rạc không có review.

## 10.2. Evidence status chuẩn

TECH-01 dùng các trạng thái:

Status

Ý nghĩa

Dùng để PASS

## WAITING

Chưa nộp

Không

## SUBMITTED

Đã nộp, chưa review

Không

## ACCEPTED

Đã review và chấp nhận

Có

## REJECTED

Bị từ chối

Không

## NEEDS_RETEST

Cần test lại

Không

## EXPIRED

Hết hiệu lực

Không

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

Không

## SUPERSEDED

Đã bị evidence mới thay thế

Không

## 11. P0 điểm chặn PRINCIPLES

## 11.1. P0 điểm chặn phải chặn release

Nếu P0 điểm chặn còn mở:

Không có ngoại lệ bằng lời nói.

## 11.2. P0 điểm chặn không được xóa im lặng

Một P0 điểm chặn chỉ được clear khi có:

Root cause.

Fix description.

Fix evidence.

Retest evidence.

Smoke evidence nếu ảnh hưởng luồng P0.

Reviewer.

Owner decision nếu ảnh hưởng business.

Audit.

## 12. RELEASE STATUS PRINCIPLES

## 12.1. Release status phải theo module/scope

Không ghi chung:

Hệ thống đã xong.

Phải ghi:

Module nào.

Scope nào.

Environment nào.

Release level nào.

Evidence nào.

Smoke nào.

Owner decision nào.

Phần nào excluded.

## 12.2. Documentation Complete không phải Production Ready

TECH-01 có thể hoàn tất tài liệu.
Nhưng Foundation chưa production ready nếu chưa có:

Implementation.

Test.

Smoke.

Security evidence.

Evidence accepted.

P0 cleared.

Owner sign-off.

Release decision.

Go-live decision nếu chạy thật.

## 13. FOUNDATION DEPENDENCY MAP

## 13.1. Foundation upstream

Foundation phụ thuộc:

## TECH-00.

MASTER/PACK governance.

Owner role/permission decision.

Security policy.

Environment boundary.

Evidence/release governance.

## 13.2. Foundation downstream

Foundation là upstream cho:

TECH-02 Product.

TECH-03 Operational Core.

TECH-04 Commerce Runtime.

TECH-05 AI Advisor.

TECH-06 Facebook Gateway.

TECH-07 Ads Measurement.

TECH-08 MC AI Live.

## TECH-09 IVR.

TECH-10 Evidence / Smoke / Release Handoff.

Nếu TECH-01 foundation chưa clear, downstream production phải bị chặn.

## 14. FOUNDATION P0 RULES

Các rule sau là P0:

User có thể thực hiện action không có permission.

Role được hardcode trong logic thay vì permission/action.

Command rủi ro không audit.

Command rủi ro không idempotency.

Evidence tự accepted.

P0 điểm chặn bị xóa im lặng.

Release status bị sửa tay không audit.

Owner decision không có record.

Admin override không audit.

Payment/batch/order/release action không kiểm soát quyền.

Dữ liệu thật dùng ở environment chưa duyệt.

Public data leak từ Foundation/security failure.

Dashboard làm source-of-truth cho release.

Documentation complete bị gọi production ready.

Downstream production khi Foundation chưa release.

## 15. FOUNDATION IMPLEMENTATION READINESS

## 15.1. Điều kiện được bắt đầu triển khai TECH-01

Dev chỉ được bắt đầu triển khai Foundation khi có:

TECH-00 complete.

TECH-01 Part 1 complete.

Repo/app/module mapping sơ bộ.

Security baseline decision.

Role/permission baseline draft.

Evidence registry plan.

P0 điểm chặn registry plan.

Release status registry plan.

Smoke plan.

Owner cho phép bắt đầu Wave 0.

Nếu thiếu:

## 15.2. Điều kiện chưa được triển khai downstream

Chưa được triển khai sâu TECH-02 đến TECH-09 production scope nếu Foundation chưa có tối thiểu:

Permission enforcement.

Audit baseline.

Idempotency baseline.

Evidence registry baseline.

P0 điểm chặn registry baseline.

Release status registry baseline.

Nếu thiếu:

## 16. TECH-01 PHẦN 1 DONE GATE

## PHẦN 1/4 được xem là hoàn tất khi đã khóa:

Mục đích TECH-01.

Vai trò Foundation.

Source-of-truth.

Foundation core modules.

Foundation boundary.

Security baseline.

RBAC principles.

Audit principles.

Idempotency principles.

Evidence registry principles.

P0 điểm chặn principles.

Release status principles.

Foundation dependency map.

Foundation P0 rules.

Implementation readiness rule.

## 17. TECH-01 PHẦN 1 FAIL GATE

## PHẦN 1/4 fail nếu đội kỹ thuật vẫn:

Xem Foundation chỉ là login đơn giản.

Hardcode role thay vì permission.

Cho command rủi ro chạy không audit.

Cho command rủi ro chạy không idempotency.

Cho evidence tự accepted.

Cho P0 điểm chặn bị xóa không record.

Cho release status bị sửa tay.

Cho owner decision không có record.

Cho UI tự enforce permission P0 mà backend không chặn.

Cho downstream production khi Foundation chưa clear.

Gọi TECH-01 documentation complete là production ready.

Nếu có một lỗi:

## 18. KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 của TECH-01 đã khóa lớp nguyên tắc nền cho Foundation của hệ thống Ginsengfood.

Kết luận bắt buộc:

Foundation là lớp nền bắt buộc trước các module domain.

Foundation không thay domain nghiệp vụ.

Foundation quản lý user, role, permission, audit, idempotency, evidence, P0 điểm chặn và release status.

Permission phải ở cấp action, không hardcode role.

Command rủi ro phải có audit.

Command rủi ro phải có idempotency.

Evidence phải có review và status.

P0 điểm chặn phải chặn release.

Release status phải theo module/scope.

Owner decision phải có record.

Downstream production phải bị chặn nếu Foundation chưa clear.

Documentation Complete không phải Production Ready.

Trạng thái sau PHẦN 1/4:

## PHẦN 2/4 - RBAC / USER / ROLE / PERMISSION / ACCESS SCOPE / SECURITY ACCESS CONTROL MODEL

## 19. MỤC ĐÍCH CỦA PHẦN 2/4

## PHẦN 2/4 khóa mô hình RBAC / User / Role / Permission / Access Scope / Security Access Control cho toàn bộ hệ thống Ginsengfood.

Mục tiêu của phần này là làm rõ:

User là ai.

Role là gì.

Permission là gì.

Scope truy cập là gì.

Role khác permission thế nào.

Permission khác guard thế nào.

Tại sao không được hardcode role trong code.

Tại sao permission phải enforce ở backend/application layer.

Những hành động nào là P0 và phải kiểm soát quyền.

Những nhóm người dùng nào được định nghĩa ở mức baseline.

Những quyền nào chỉ là đề xuất nền, không phải code.

Khi nào cần owner decision.

Khi nào phải chặn release vì RBAC chưa đủ.

## PHẦN 2/4 không viết code.
PHẦN 2/4 không thiết kế database chi tiết.
PHẦN 2/4 không thiết kế API chi tiết.
PHẦN 2/4 không thiết kế UI chi tiết.

## PHẦN 2/4 chỉ khóa mô hình phân quyền kỹ thuật để các module sau triển khai đúng.

## 20. RBAC CORE PRINCIPLE

## 20.1. RBAC không phải “admin/user”

Trong Ginsengfood, RBAC không được hiểu đơn giản là:

admin;

nhân viên;

khách hàng;

nhà cung cấp.

RBAC phải đủ sức kiểm soát toàn bộ hệ thống gồm:

Sản phẩm.

Công thức.

Sản xuất.

## QC.

Kho.

Tồn.

Truy xuất.

Thu hồi.

Khóa bán.

Báo giá.

Đơn hàng.

Thanh toán.

Giao hàng.

AI Advisor.

Facebook Gateway.

Ads Measurement.

MC AI Live.

## IVR.

Evidence.

Release / Go-live.

Nếu RBAC chỉ có “admin/user”, hệ thống sẽ không đủ kiểm soát.

## 20.2. Role không phải permission

Role là nhóm vai trò.
Permission là quyền thực hiện hành động cụ thể.
Scope là phạm vi áp dụng quyền.
Guard là điều kiện nghiệp vụ bắt buộc trước khi action được thực thi.

Không được viết logic kiểu:

Nếu role = Admin thì cho làm tất cả.

Phải hiểu đúng:

User có role -> role có permission -> permission được kiểm tra theo scope -> guard nghiệp vụ được kiểm tra -> command mới được thực hiện -> audit được ghi.

## 20.3. Permission phải ở cấp action

Permission phải đủ nhỏ để kiểm soát hành động rủi ro.

Ví dụ:

Nhóm

Permission ví dụ

Product

product.create, product.update, product.activate

Recipe

recipe.create, recipe.approve, recipe.activate

Production

production_order.create, production_order.approve

## QC

qc.record, qc.approve

Batch

batch.release

Warehouse

warehouse.receive, inventory.adjust

Recall

recall.create, recall.close

Sale Lock

sale_lock.apply, sale_lock.release

Commerce

quote.create, order.confirm, order.cancel

Payment

payment.review, payment.confirm

## AI

ai.claim.review, ai.content.approve

Facebook

channel.facebook.reply, channel.facebook.handoff

Ads

ads.view, ads.scale_propose, ads.scale_approve

Live

live.plan.create, live.script.approve

## IVR

ivr.call_schedule, ivr.result_review

Evidence

evidence.submit, evidence.accept, evidence.reject

Release

release.review, release.approve, go_live.approve

Tên permission cụ thể sẽ được dev đề xuất ở lớp implementation, nhưng nguyên tắc action-level là bắt buộc.

## 21. ACCESS CONTROL MODEL

## 21.1. Công thức access control chuẩn

Mọi action quan trọng phải đi qua chuỗi kiểm soát:

Authenticated User
-> Active Account
-> Assigned Role
-> Required Permission
-> Access Scope
-> Business Guard
-> Idempotency nếu là command rủi ro
-> Audit
-> Action Result

Không được bỏ qua bất kỳ lớp bắt buộc nào đối với command rủi ro.

## 21.2. Default Deny

Nguyên tắc nền:

Không có quyền thì mặc định bị chặn.

Không được mặc định cho phép rồi chặn sau.

Mọi action chưa khai báo permission phải xem là:

## ACCESS_DENIED

Nếu module mới chưa có permission map:

## 21.3. Backend enforce là bắt buộc

UI có thể ẩn nút.
UI có thể disable action.
UI có thể hiển thị cảnh báo.

Nhưng backend/application layer mới là nơi bắt buộc enforce permission.

Không được chỉ chặn ở frontend.

Ví dụ:

UI không hiển thị nút “Release Batch” là chưa đủ.

Backend vẫn phải kiểm tra batch.release.

Backend vẫn phải kiểm tra batch đủ điều kiện release.

Backend vẫn phải audit.

Backend vẫn phải chặn nếu guard fail.

## 22. USER MODEL PRINCIPLE

## 22.1. User là chủ thể hành động

User là chủ thể thực hiện hoặc kích hoạt hành động trong hệ thống.

User có thể là:

Nhân sự nội bộ.

Owner / Director.

Admin hệ thống.

Nhân viên sản xuất.

## QC.

Kho.

Kế toán / thanh toán.

Sales / Commerce operator.

Marketing / Ads operator.

AI content / claim reviewer.

Facebook channel operator.

MC Live operator.

IVR operator.

Supplier user nếu scope có.

System service account.

Integration account.

Worker account.

## 22.2. User phải có trạng thái

User không được chỉ là dòng thông tin đăng nhập.

User tối thiểu phải có trạng thái:

User Status

Ý nghĩa

## ACTIVE

Được phép đăng nhập/làm việc theo quyền

## INVITED

Được mời, chưa kích hoạt

## SUSPENDED

Tạm khóa

## DISABLED

Vô hiệu hóa

## PASSWORD_RESET_REQUIRED

Cần reset mật khẩu

## LOCKED_BY_SECURITY

Bị khóa do bảo mật

## OWNER_REVIEW_REQUIRED

Cần owner review

Nếu user không ACTIVE thì không được thực hiện action production.

## 22.3. Service Account phải kiểm soát riêng

Service account dùng cho worker, integration, automation.

Ví dụ:

Payment sync worker.

MISA sync worker.

Notification worker.

IVR ingestion worker.

Ads event worker.

Evidence processing worker.

Service account không được dùng như user người thật.

Service account phải có:

Scope rõ.

Permission tối thiểu.

Audit rõ.

Secret/config quản lý an toàn.

Không có quyền admin toàn hệ nếu không cần.

## 23. ROLE MODEL PRINCIPLE

## 23.1. Role là nhóm quyền

Role dùng để gom permission cho dễ quản trị.

Role không được là logic nghiệp vụ.

Ví dụ role:

Role

Ý nghĩa

## SYSTEM_ADMIN

Quản trị kỹ thuật hệ thống

## OWNER_DIRECTOR

Chủ sở hữu/quyết định cuối

## PRODUCTION_MANAGER

Quản lý sản xuất

## QC_OPERATOR

Ghi nhận QC

## QC_APPROVER

Duyệt QC nếu tách quyền

## WAREHOUSE_OPERATOR

Thao tác kho

## INVENTORY_CONTROLLER

Kiểm soát tồn/điều chỉnh

## COMMERCE_OPERATOR

Xử lý quote/order

## PAYMENT_REVIEWER

Kiểm tra thanh toán

## ACCOUNTING_REVIEWER

Đối soát kế toán/MISA

## SALES_ADMIN

Quản trị bán hàng

## AI_ADVISOR_MANAGER

Quản trị AI Advisor

## CLAIM_REVIEWER

Duyệt claim/content sức khỏe/khoa học

## FACEBOOK_OPERATOR

Vận hành Facebook/Messenger

## ADS_OPERATOR

Xem/vận hành Ads Measurement

## ADS_APPROVER

Duyệt scale/stop gate

## LIVE_OPERATOR

Vận hành live

## LIVE_SCRIPT_APPROVER

Duyệt script live

## IVR_OPERATOR

Vận hành IVR

## IVR_REVIEWER

Review call result/incident

## SUPPLIER_USER

Người dùng nhà cung cấp nếu scope có

Danh sách này là baseline. Khi triển khai, dev/owner có thể điều chỉnh tên role, nhưng không được bỏ nguyên tắc role-permission-scope-guard.

## 23.2. Role không được cấp quyền vô hạn không kiểm soát

Ngay cả SYSTEM_ADMIN cũng không được bypass audit, evidence, P0 điểm chặn hoặc release gate.

SYSTEM_ADMIN có thể quản trị kỹ thuật, nhưng không có nghĩa được:

Mark payment paid tùy ý.

Release batch trái QC.

Gỡ recall không audit.

Accept evidence của chính mình nếu policy yêu cầu reviewer khác.

Approve go-live không owner decision.

Sửa audit history.

Sửa release status để hợp thức hóa.

## 24. PERMISSION MODEL PRINCIPLE

## 24.1. Permission là điểm enforce

Permission phải được định nghĩa theo hành động.

Permission không được viết mơ hồ như:

manage all;

full access;

admin everything.

Các permission rủi ro phải tách rõ.

Ví dụ:

Nhóm

Permission rủi ro

Product

product.activate

Recipe

recipe.approve, recipe.activate

Batch

batch.release

Inventory

inventory.adjust

Recall

recall.create, recall.close

Sale Lock

sale_lock.apply, sale_lock.release

Commerce

order.confirm, order.cancel

Payment

payment.confirm

Evidence

evidence.accept, evidence.reject

Release

release.approve, go_live.approve

## IVR

ivr.enable_production_call

Ads

ads.scale_approve

Live

live.production_approve

## 24.2. Permission phải gắn với guard

Có permission chưa chắc được làm.

Ví dụ:

User có batch.release nhưng vẫn bị chặn nếu:

Batch chưa QC_PASS.

Batch đang recall hold.

Batch thiếu evidence.

Batch thuộc scope khác.

User không có quyền ở warehouse/plant đó.

Release gate chưa pass.

Permission trả lời:

User có quyền thử thực hiện hành động không?

Guard trả lời:

Hành động này có hợp lệ trong trạng thái nghiệp vụ hiện tại không?

## 25. ACCESS SCOPE MODEL

## 25.1. Scope là gì

Scope giới hạn phạm vi quyền.

Một user có permission chưa chắc được làm trên mọi dữ liệu.

Các scope cần xét:

Environment scope.

Organization scope.

Supplier scope.

Warehouse scope.

Plant/production line scope.

Product/SKU scope.

Channel/page scope.

Payment method scope.

Evidence/release scope.

Time-bound scope nếu là quyền tạm thời.

## 25.2. Environment Scope

Không phải quyền ở DEV cũng có quyền ở PROD.

Environment

Quy tắc

## LOCAL

Không dữ liệu thật

## DEV

Không gửi tin thật nếu chưa duyệt

## STAGING

Dùng test/simulation

## PILOT

Cần owner decision giới hạn

## PROD

Cần Go-live Approved

Permission phải xét environment.

Ví dụ:

User có quyền ivr.call_schedule ở STAGING không có nghĩa được gọi khách thật ở PROD.

## 25.3. Channel / Page Scope

Với Facebook Gateway và MC AI Live, quyền phải theo page/kênh.

Ví dụ:

Operator được xử lý Page A chưa chắc được xử lý Page B.

Live operator của page test không được điều phối live production.

Ads operator xem measurement không được approve scale.

Public reply quyền khác với Messenger handoff quyền.

## 25.4. Supplier Scope

Nếu có Supplier Portal, supplier user chỉ được xem và thao tác dữ liệu của chính supplier đó.

Supplier user không được:

Xem supplier khác.

Xem cost/accounting.

Xem internal QC detail nếu không public.

Tạo raw lot final trực tiếp nếu rule yêu cầu admin receive/QC.

Vượt allowlist nguyên liệu.

Truy cập admin operation.

## 26. GUARD MODEL

## 26.1. Guard là lớp kiểm tra nghiệp vụ

Guard là điều kiện bắt buộc sau permission.

Ví dụ guard:

Action

Guard

product.activate

Product đủ dữ liệu, owner approve

recipe.activate

Formula version valid, không conflict

batch.release

QC_PASS, no active hold, release permission

warehouse.receive

Batch RELEASED

quote.create

SKU sellable, price runtime available

payment.confirm

Payment source valid

order.cancel

State cho phép cancel

evidence.accept

Reviewer hợp lệ, evidence không conflict

release.approve

Smoke pass, evidence accepted, P0 clear

ivr.enable_call

Order eligible, scheduler/capacity clear

ads.scale_approve

Data quality pass, verified revenue pass

## 26.2. Guard không được bỏ qua vì user có quyền cao

Owner/Admin có thể có quyền duyệt, nhưng guard P0 vẫn phải kiểm tra.

Ví dụ:

Owner cũng không được approve release nếu:

Evidence chưa có.

Smoke fail.

P0 điểm chặn open.

Security fail.

Dependency bị chặn.

Nếu owner muốn chấp nhận rủi ro có điều kiện, phải có owner decision record và không được waiver các lỗi P0 nghiêm trọng liên quan payment, dữ liệu khách hàng, recall/sale lock, public leak, IVR hủy sai.

## 27. SEPARATION OF DUTIES

## 27.1. Tách người tạo và người duyệt

Một số hành động cần tách người tạo và người duyệt.

Ví dụ nên tách:

Người tạo recipe và người approve recipe.

Người ghi QC và người release batch nếu policy yêu cầu.

Người tạo payment review và người confirm payment.

Người submit evidence và người accept evidence.

Người đề xuất Ads scale và người approve scale.

Người tạo live script và người approve script.

Người cấu hình IVR và người approve production call.

Người chuẩn bị release và người approve release.

## 27.2. Không tự duyệt evidence của chính mình nếu scope rủi ro

Với evidence liên quan P0/release/security/payment/customer data, người submit không nên là người accept nếu policy yêu cầu review độc lập.

Nếu chưa có người review độc lập:

## OWNER_REVIEW_REQUIRED

## 28. PRIVILEGED ACCESS / BREAK-GLASS PRINCIPLE

## 28.1. Break-glass là quyền khẩn cấp

Break-glass chỉ dùng trong tình huống khẩn cấp có kiểm soát.

Break-glass không phải cách làm nhanh.
Break-glass không phải quyền admin thường xuyên.
Break-glass không được dùng để bỏ qua evidence/release gate.

Break-glass phải có:

Reason.

Scope.

Time limit.

Approval.

Audit.

Post-review.

Expiry.

## 28.2. Break-glass không được áp dụng để hợp thức hóa sai

Không dùng break-glass để:

Mark PAID sai.

Release batch chưa đủ điều kiện.

Gỡ sale lock/recall không review.

Chấp nhận evidence thiếu.

Approve go-live khi P0 open.

Cho IVR hủy đơn trực tiếp.

Cho Ads scale khi revenue chưa verified.

Cho Public Trace leak nội bộ.

## 29. RBAC PERMISSION GROUP BASELINE

## 29.1. System / Foundation Permissions

Permission Group

Permission baseline

User

user.view, user.create, user.update, user.disable

Role

role.view, role.create, role.update, role.assign

Permission

permission.view, permission.grant, permission.revoke

Audit

audit.view, audit.export

Evidence

evidence.submit, evidence.review, evidence.accept, evidence.reject

điểm chặn

điểm chặn.create, điểm chặn.update, điểm chặn.clear

Release

release.view, release.review, release.approve, go_live.approve

## 29.2. Product / Recipe Permissions

Permission Group

Permission baseline

Product

product.view, product.create, product.update, product.activate

## SKU

sku.view, sku.create, sku.update, sku.activate

Ingredient

ingredient.view, ingredient.create, ingredient.update

Recipe

recipe.view, recipe.create, recipe.update, recipe.approve, recipe.activate

Formula

formula.view, formula.version_create, formula.activate

## 29.3. Operational Permissions

Permission Group

Permission baseline

Raw Intake

raw_intake.create, raw_intake.review

## QC

qc.record, qc.review, qc.approve

Raw Lot

raw_lot.view, raw_lot.mark_ready

Production

production_order.create, production_order.approve

Material

material_request.create, material_issue.execute, material_receipt.confirm

Batch

batch.view, batch.qc_record, batch.release

Warehouse

warehouse.receive, warehouse.transfer

Inventory

inventory.view, inventory.adjust

Trace

trace.view_internal, trace.publish_public

Recall

recall.create, recall.update, recall.close

Sale Lock

sale_lock.apply, sale_lock.release

## 29.4. Commerce Permissions

Permission Group

Permission baseline

Price

price.view, price.update

Program

program.view, program.configure

Member Benefit

member_benefit.view, member_benefit.configure

Quote

quote.view, quote.create, quote.expire

Cart

cart.view, cart.update

Order

order.view, order.create, order.confirm, order.cancel

Payment

payment.view, payment.review, payment.confirm

Shipping

shipping.view, shipping.configure

Fulfillment

fulfillment.view, fulfillment.release

## 29.5. AI / Channel / Ads / Live / IVR Permissions

Domain

Permission baseline

AI Advisor

ai.view, ai.configure, ai.claim_review, ai.template_approve

Facebook Gateway

channel.facebook.view, channel.facebook.reply, channel.facebook.handoff, channel.facebook.configure

Ads

ads.view, ads.report_view, ads.scale_propose, ads.scale_approve, ads.stop_approve

MC AI Live

live.view, live.plan_create, live.script_create, live.script_approve, live.production_approve

## IVR

ivr.view, ivr.schedule, ivr.result_review, ivr.production_enable, ivr.incident_review

Notification

notification.view, notification.template_approve, notification.dispatch_transactional

## 30. RBAC MATRIX BASELINE BY ROLE

## 30.1. Baseline role-permission định hướng

Bảng này là baseline kỹ thuật để dev/owner rà, không phải cấu hình cuối cùng.

Role

Nhóm quyền chính

Không được làm

## SYSTEM_ADMIN

User/role/system config kỹ thuật

Không bypass audit/release/payment/domain guard

## OWNER_DIRECTOR

Owner decision, release approval, go-live approval

Không sửa lịch sử/audit

## PRODUCTION_MANAGER

Production order, process oversight

Không confirm payment

## QC_OPERATOR

QC record

Không tự release nếu rule tách quyền

## QC_APPROVER

QC approve/review

Không sửa inventory/payment

## WAREHOUSE_OPERATOR

Warehouse receive, stock movement theo quyền

Không release batch

## INVENTORY_CONTROLLER

Inventory review/adjust theo audit

Không mark payment paid

## COMMERCE_OPERATOR

Quote/order support

Không confirm payment nếu không có quyền

## PAYMENT_REVIEWER

Payment review/confirm theo source hợp lệ

Không sửa quote/recipe

## ACCOUNTING_REVIEWER

Accounting/MISA review

Không thay order state ngoài boundary

## AI_ADVISOR_MANAGER

AI config/template/resolver review

Không hardcode price/payment/order

## CLAIM_REVIEWER

Claim/content approve

Không approve release/go-live

## FACEBOOK_OPERATOR

Public/private channel handling

Không tạo order ngoài Commerce

## ADS_OPERATOR

Xem report/đề xuất scale

Không approve scale nếu không có quyền

## ADS_APPROVER

Approve scale/stop theo rule

Không tạo verified revenue

## LIVE_OPERATOR

Vận hành live/script cue

Không chốt đơn public

## LIVE_SCRIPT_APPROVER

Duyệt script live

Không approve payment/order

## IVR_OPERATOR

Theo dõi/schedule theo quyền

Không hủy đơn trực tiếp

## IVR_REVIEWER

Review IVR incident/result

Không tự đổi order state

## SUPPLIER_USER

Supplier scoped actions

Không xem dữ liệu nội bộ

## 31. PERMISSION ENFORCEMENT BY MODULE

## 31.1. Product / Recipe

Các action phải enforce permission:

Tạo product.

Sửa product.

Active product.

Tạo SKU.

Active SKU.

Tạo recipe.

Approve recipe.

Activate formula version.

Publish product knowledge.

Retire/deactivate SKU.

## 31.2. Operational Core

Các action phải enforce permission:

Receive raw material.

Record QC.

Mark raw lot ready.

Create production order.

Approve production order.

Issue material.

Confirm material receipt.

Record batch QC.

Release batch.

Warehouse receipt.

Inventory adjustment.

Create recall.

Apply sale lock.

Close recall.

## 31.3. Commerce

Các action phải enforce permission:

Create quote.

Create order.

Confirm order.

Cancel order.

Review payment.

Confirm payment.

Configure shipping.

Approve refund/return nếu scope có.

Release fulfillment.

Mark revenue verified nếu scope có accounting review.

## 31.4. AI / Facebook / Ads / Live / IVR

Các action phải enforce permission:

Approve AI template.

Approve claim.

Configure resolver.

Enable Facebook page routing.

Send public reply nếu có scope.

Trigger private handoff.

Propose Ads scale.

Approve Ads scale.

Approve live script.

Enable live production.

Schedule IVR production calls.

Review IVR call results.

Enable IVR production scope.

Approve transactional notification template.

## 32. RBAC AUDIT REQUIREMENT

## 32.1. RBAC audit bắt buộc

Các hành động RBAC phải audit:

User created.

User disabled.

Password reset forced.

Role created.

Role updated.

Role assigned.

Permission granted.

Permission revoked.

Service account created.

Service account permission changed.

Break-glass enabled.

Break-glass used.

Access denied event for sensitive action.

Export permission used.

Release approval permission used.

## 32.2. Access denied cũng cần ghi nhận trong trường hợp nhạy cảm

Các access denied nhạy cảm nên được ghi nhận:

Payment confirm denied.

Batch release denied.

Order cancel denied.

Evidence accept denied.

Release approve denied.

IVR production enable denied.

Public trace publish denied.

Customer/payment data access denied.

Access denied audit giúp phát hiện lạm quyền hoặc cấu hình sai.

## 33. RBAC SMOKE REQUIREMENT

## 33.1. Smoke bắt buộc cho RBAC

RBAC smoke phải chứng minh:

Smoke ID

Scenario

Expected Result

## RBAC-SMK-001

User không login gọi action nhạy cảm

## ACCESS_DENIED

## RBAC-SMK-002

User không có permission batch.release

Bị chặn

## RBAC-SMK-003

User có permission nhưng guard fail

Bị chặn

## RBAC-SMK-004

User có permission và guard pass

Action thành công

## RBAC-SMK-005

Role thay đổi quyền

Audit ghi nhận

## RBAC-SMK-006

Permission revoked

Action bị chặn sau revoke

## RBAC-SMK-007

Service account ngoài scope

Bị chặn

## RBAC-SMK-008

Evidence accept không có quyền

Bị chặn

## RBAC-SMK-009

Release approve không có quyền

Bị chặn

## RBAC-SMK-010

Break-glass dùng

Có reason, expiry, audit

## 33.2. RBAC smoke phải có evidence

Smoke RBAC không được chỉ nói “đã test quyền”.

Phải có:

Test case.

User/role/permission dùng test.

Action test.

Expected result.

Actual result.

Audit evidence.

Reviewer.

Evidence status.

## 34. RBAC P0 điểm chặn

Các lỗi sau là P0:

User không có quyền vẫn làm được action nhạy cảm.

Backend không enforce permission.

Chỉ UI enforce permission.

Role hardcode làm bypass permission.

Admin có thể sửa audit/evidence/release status không audit.

User bị disable vẫn thực hiện action.

Service account vượt scope.

Permission revoke nhưng action vẫn chạy.

Batch/payment/order/release action thiếu permission.

Evidence accept không cần permission.

Release approve không cần permission.

Break-glass không expiry/audit.

Supplier user xem dữ liệu ngoài scope.

Facebook operator xem/sửa dữ liệu payment không cần thiết.

IVR operator hủy order trực tiếp.

Nếu có P0:

## 35. RBAC DONE GATE

RBAC chỉ Done khi:

User model rõ.

Role model rõ.

Permission model rõ.

Scope model rõ.

Guard model rõ.

Default deny được khóa.

Backend enforcement được xác nhận ở mức implementation sau này.

Role không hardcode để bypass permission.

Security-sensitive actions có permission.

RBAC audit requirement rõ.

RBAC smoke plan có.

RBAC evidence plan có.

P0 điểm chặn list có.

Owner review role/permission baseline.

Evidence accepted sau implementation.

Ở giai đoạn tài liệu:

## 36. RBAC FAIL GATE

RBAC fail nếu:

Không có permission action-level.

Không có access scope.

Không có backend enforcement plan.

Không có audit cho role/permission change.

Không có separation of duties cho action rủi ro.

Không có smoke plan.

Không có evidence plan.

Dùng role Admin để bypass toàn hệ.

Supplier user không scope.

Service account quá quyền.

Break-glass không kiểm soát.

Owner không review role/permission baseline.

Nếu fail:

## 37. OWNER DECISION POINTS CHO RBAC

Owner cần quyết định các điểm sau trước implementation sâu:

Danh sách role chính thức.

Ai là owner cuối cùng của release/go-live.

Có tách QC_OPERATOR và QC_APPROVER không.

Có tách ADS_OPERATOR và ADS_APPROVER không.

Có tách LIVE_OPERATOR và LIVE_SCRIPT_APPROVER không.

Có tách người submit evidence và accept evidence không.

Ai được confirm payment.

Ai được release batch.

Ai được apply/release sale lock.

Ai được close recall.

Ai được approve Ads scale.

Ai được enable IVR production.

Ai được approve public trace publish.

Break-glass có dùng không, scope nào.

Supplier Portal có bật ở giai đoạn đầu không.

Nếu owner chưa quyết các điểm ảnh hưởng P0:

## OWNER_REVIEW_REQUIRED

## 38. KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 của TECH-01 đã khóa mô hình RBAC / User / Role / Permission / Access Scope / Security Access Control cho Ginsengfood.

Kết luận bắt buộc:

RBAC không phải chỉ là admin/user.

Role không phải permission.

Permission phải ở cấp action.

Scope giới hạn phạm vi quyền.

Guard kiểm tra điều kiện nghiệp vụ sau permission.

Default deny là nguyên tắc nền.

Backend/application layer phải enforce permission.

UI chỉ hỗ trợ hiển thị, không được là lớp bảo vệ duy nhất.

Service account phải kiểm soát scope.

Supplier user không được vượt supplier scope.

Permission rủi ro phải audit.

Action P0 phải có separation of duties nếu policy yêu cầu.

Break-glass phải có scope, reason, expiry, audit.

RBAC smoke và evidence là bắt buộc.

Nếu RBAC chưa clear, downstream production phải bị chặn.

Trạng thái sau PHẦN 2/4:

## PHẦN 3/4 - AUDIT / IDEMPOTENCY / EVIDENCE REGISTRY / P0 điểm chặn / RELEASE STATUS CONTROL

## 39. MỤC ĐÍCH CỦA PHẦN 3/4

## PHẦN 3/4 khóa lớp kiểm soát nền bắt buộc của Foundation, gồm:

Audit Trail.

Idempotency.

Evidence Registry.

P0 điểm chặn Registry.

Release Status Registry.

Owner Decision Record.

Foundation Control Smoke.

No-bypass control cho các module downstream.

## PHẦN 3/4 trả lời các câu hỏi:

Hành động nào phải audit?

Audit cần ghi tối thiểu những gì?

Audit có được sửa/xóa không?

Command nào bắt buộc idempotency?

Retry được xử lý thế nào để không tạo trùng?

Evidence được nộp, review, accept, reject thế nào?

P0 điểm chặn được ghi nhận và clear thế nào?

Release status được cập nhật thế nào?

Khi nào Foundation cho phép module downstream đi tiếp?

Khi nào phải bị chặn dù code đã làm xong?

## PHẦN 3/4 không viết code.
PHẦN 3/4 không thiết kế API chi tiết.
PHẦN 3/4 không thiết kế database chi tiết.
PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa mô hình kỹ thuật nền để dev triển khai đúng.

## 40. AUDIT TRAIL PRINCIPLE

## 40.1. Audit là gì

Audit là lịch sử kiểm soát hành động nghiệp vụ và hành động hệ thống quan trọng.

Audit không phải log debug.
Audit không phải console log.
Audit không phải ghi chú vận hành rời rạc.
Audit không phải màn hình báo cáo.

Audit phải trả lời được:

Ai làm?

Làm hành động gì?

Làm trên đối tượng nào?

Trạng thái trước là gì?

Trạng thái sau là gì?

Vì sao làm?

Làm lúc nào?

Làm từ kênh nào?

Kết quả là gì?

Có bị từ chối không?

Có liên quan command/idempotency/correlation nào không?

Có cần owner review không?

## 40.2. Audit là append-only

Audit phải theo nguyên tắc:

Ghi thêm, không sửa lịch sử.

Không được:

Sửa audit cũ.

Xóa audit cũ.

Ghi đè audit cũ.

Làm sạch audit để hợp thức hóa release.

Thay đổi audit sau khi phát sinh lỗi.

Che giấu hành động override.

Xóa dấu vết permission change.

Xóa dấu vết evidence review.

Xóa dấu vết release decision.

Xóa dấu vết IVR result.

Nếu cần điều chỉnh, phải tạo correction audit record mới.

## 40.3. Audit không được thay thế business state

Audit ghi nhận lịch sử.
Audit không phải nguồn quyết định state hiện tại.

Ví dụ:

Audit ghi nhận payment.confirm attempted, nhưng Payment Module mới quyết trạng thái payment.

Audit ghi nhận IVR result received, nhưng Order State Machine mới quyết trạng thái đơn.

Audit ghi nhận batch.release attempted, nhưng Batch Release domain mới quyết batch có RELEASED không.

Audit ghi nhận evidence.accept attempted, nhưng Evidence Registry mới quyết evidence status.

Audit là bằng chứng, không phải domain owner.

## 41. AUDIT SCOPE

## 41.1. Nhóm hành động bắt buộc audit

Các nhóm sau bắt buộc audit:

Authentication / login / logout / failed login.

User create/update/disable.

Role assignment.

Permission grant/revoke.

Service account change.

Product/SKU create/update/activate/deactivate.

Recipe/formula create/approve/activate.

Raw material intake.

QC record/review/approve.

Raw lot mark ready.

Production order create/approve.

Material issue.

Material receipt.

Batch QC.

Batch release.

Warehouse receipt.

Inventory adjustment.

Public trace publish/unpublish.

Recall create/update/close.

Sale lock apply/release.

Quote create/expire.

Order create/confirm/cancel.

Payment review/confirm/reject.

Shipping/fulfillment release.

AI claim/template approve.

Facebook Gateway public/private routing action.

Ads scale propose/approve/stop.

MC AI Live script approve/production enable.

IVR schedule/result/incident.

Evidence submit/review/accept/reject.

P0 điểm chặn create/update/clear.

Release review/approve/hold/rollback.

Go-live approve.

Admin override.

Data export.

Security-sensitive access denied.

## 41.2. Audit cho access denied

Access denied ở action nhạy cảm phải ghi nhận.

Các trường hợp nên audit:

payment.confirm denied.

batch.release denied.

order.cancel denied.

evidence.accept denied.

release.approve denied.

go_live.approve denied.

ivr.production_enable denied.

public_trace.publish denied.

customer_data.access denied.

role.permission_change denied.

Lý do: access denied giúp phát hiện quyền sai, lạm quyền hoặc cấu hình bảo mật chưa đúng.

## 42. AUDIT RECORD STANDARD

## 42.1. Audit record tối thiểu

Mỗi audit record tối thiểu phải có:

Trường

Ý nghĩa

Audit ID

Mã audit

Actor Type

Human / Service Account / System

Actor ID

Người hoặc service thực hiện

Actor Role Snapshot

Role tại thời điểm hành động

Permission Checked

Permission đã kiểm

Action

Hành động

Object Type

Loại đối tượng

Object ID

ID đối tượng

From State

Trạng thái trước nếu có

To State

Trạng thái sau nếu có

Reason

Lý do nếu action rủi ro

Result

## SUCCESS / DENIED / FAILED

Failure Reason

Lý do fail nếu có

Environment

## LOCAL / DEV / STAGING / PILOT / PROD

Source Channel

Admin / PWA / AI / Gateway / Worker / IVR

Correlation ID

Chuỗi liên kết request/flow

Idempotency Key

Nếu command có idempotency

Timestamp

Thời điểm

Metadata

Thông tin mở rộng an toàn

## 42.2. Audit không được chứa dữ liệu nhạy cảm không cần thiết

Audit không được lưu thừa:

Password.

Token/secret.

Full payment credential.

Full address nếu không cần.

Health note chi tiết nếu không cần.

Nội dung hội thoại nhạy cảm nếu không có policy.

Evidence binary nếu policy yêu cầu metadata-only.

MISA private payload nếu không cần.

Audit phải đủ để truy vết nhưng không tạo rủi ro dữ liệu.

## 43. AUDIT CATEGORY MODEL

## 43.1. Nhóm audit theo category

Audit Category

Nội dung

## AUTH_AUDIT

Login, logout, failed login, session

## RBAC_AUDIT

Role, permission, user access

## MASTER_DATA_AUDIT

Product, SKU, ingredient, recipe

## OPERATIONAL_AUDIT

Raw intake, QC, production, warehouse

## INVENTORY_AUDIT

Ledger, stock movement, adjustment

## TRACE_AUDIT

Internal trace, public trace publish

## RECALL_AUDIT

Recall, recovery, close

## COMMERCE_AUDIT

Quote, cart, order

## PAYMENT_AUDIT

Payment review/confirm

## AI_AUDIT

AI config, claim/template approval

## CHANNEL_AUDIT

Facebook/Messenger routing

## ADS_AUDIT

Attribution, scale/stop decision

## LIVE_AUDIT

Live plan, script, cue approval

## IVR_AUDIT

Call schedule, result, incident

## EVIDENCE_AUDIT

Evidence submit/review/accept/reject

## RELEASE_AUDIT

Release, hold, rollback, go-live

## SECURITY_AUDIT

Sensitive access, denial, export

## 43.2. Audit category giúp smoke và evidence rõ ràng

Khi smoke một module, phải biết audit thuộc category nào.

Ví dụ:

Smoke Payment phải sinh PAYMENT_AUDIT.

Smoke Batch Release phải sinh OPERATIONAL_AUDIT.

Smoke Evidence Accept phải sinh EVIDENCE_AUDIT.

Smoke Go-live Approve phải sinh RELEASE_AUDIT.

Smoke IVR Result phải sinh IVR_AUDIT.

Nếu smoke pass nhưng audit không sinh đúng:

## 44. AUDIT P0 điểm chặn

Các lỗi audit sau là P0:

Command rủi ro không audit.

Audit có thể sửa/xóa không kiểm soát.

Payment confirm không audit.

Batch release không audit.

Order cancel không audit.

Evidence accept không audit.

Release approve không audit.

Permission grant/revoke không audit.

Admin override không audit.

IVR result ingestion không audit.

P0 điểm chặn clear không audit.

Public trace publish không audit.

MISA/payment sync outcome không audit.

Audit lưu dữ liệu nhạy cảm quá mức.

Dashboard hiển thị status nhưng không truy được audit nguồn.

Nếu có P0 audit:

## 45. IDEMPOTENCY PRINCIPLE

## 45.1. Idempotency là gì

Idempotency là cơ chế bảo đảm một command được gửi lặp lại không tạo ra kết quả trùng hoặc sai.

Idempotency không chỉ là kỹ thuật retry.
Idempotency là kiểm soát nghiệp vụ.

Một command rủi ro khi retry phải trả về kết quả an toàn:

cùng payload + cùng idempotency key -> trả lại kết quả đã xử lý;

khác payload + cùng idempotency key -> báo conflict;

không có idempotency key ở command bắt buộc -> từ chối hoặc đưa vào xử lý lỗi theo policy.

## 45.2. Command bắt buộc idempotency

Các command sau bắt buộc idempotency:

Nhóm

Command

Commerce

create quote, create order, cancel order

Payment

confirm payment, reject payment

Inventory

material issue, warehouse receive, inventory adjust

Production

approve production order, release batch

Recall / Sale Lock

apply recall, close recall, apply sale lock, release sale lock

Notification

send transactional notice, send cancellation notice

## IVR

schedule call, submit call result

## MISA

sync accounting document, retry sync

Evidence

submit evidence, accept evidence, reject evidence

Release

approve release, hold release, rollback release, approve go-live

## 45.3. Idempotency không được chỉ đặt ở UI

Nếu user bấm hai lần ở UI, UI có thể disable nút.
Nhưng backend/application layer vẫn phải idempotent.

Nếu webhook gửi lại, worker retry, IVR result gửi lặp, payment gateway callback lặp, Facebook webhook gửi trùng, hệ thống vẫn phải an toàn.

## 46. IDEMPOTENCY RESULT MODEL

## 46.1. Trạng thái kết quả idempotency

Result

Ý nghĩa

## NEW_ACCEPTED

Command mới được nhận

## DUPLICATE_RETURNED

Command trùng, trả lại kết quả cũ

## CONFLICT_REJECTED

Cùng key nhưng payload khác

## EXPIRED_KEY_REJECTED

Key hết hiệu lực theo policy

## MISSING_KEY_REJECTED

Command bắt buộc nhưng thiếu key

## IN_PROGRESS_RETURNED

Command đang xử lý, trả trạng thái đang xử lý

## FAILED_RETRY_ALLOWED

Fail kỹ thuật, cho retry an toàn

## FAILED_RETRY_BLOCKED

Fail nghiệp vụ, không retry tự động

## 46.2. Idempotency phải gắn audit

Mỗi command idempotent phải audit:

Idempotency key.

Payload fingerprint hoặc safe hash.

Actor.

Command.

First request time.

Retry count nếu có.

Result.

Conflict nếu có.

Final business object nếu đã tạo.

Không lưu payload nhạy cảm không cần thiết.

## 47. IDEMPOTENCY P0 điểm chặn

Các lỗi sau là P0:

Create order gửi lặp tạo nhiều đơn.

Confirm payment gửi lặp ghi nhận nhiều lần.

Material issue retry làm trừ tồn nhiều lần.

Warehouse receipt retry làm nhập kho nhiều lần.

IVR result retry tạo nhiều quyết định.

Notification retry gửi nhiều tin giao dịch trùng.

MISA sync retry tạo chứng từ trùng.

Evidence accept retry tạo nhiều trạng thái mâu thuẫn.

Release approve retry tạo nhiều quyết định release.

Cùng idempotency key nhưng payload khác vẫn được chấp nhận.

Command rủi ro không có idempotency key.

Idempotency không audit.

Nếu có:

## 48. EVIDENCE REGISTRY PRINCIPLE

## 48.1. Evidence Registry là gì

Evidence Registry là nơi ghi nhận bằng chứng để chứng minh:

Dev đã triển khai.

Test đã chạy.

Smoke đã pass.

Security đã review.

P0 đã fix.

Retest đã pass.

Owner đã review.

Release đã approve.

Go-live đã approve.

Evidence Registry không phải thư mục file rời rạc.
Evidence Registry không phải ảnh chụp rời rạc.
Evidence Registry không phải báo cáo miệng.
Evidence Registry không phải dashboard màu xanh.

## 48.2. Evidence phải có status

Evidence chỉ có giá trị khi có status.

Status

Ý nghĩa

Dùng để PASS

## WAITING

Chưa nộp

Không

## SUBMITTED

Đã nộp, chưa review

Không

## ACCEPTED

Đã review và chấp nhận

Có

## REJECTED

Bị từ chối

Không

## NEEDS_RETEST

Cần test lại

Không

## EXPIRED

Hết hiệu lực

Không

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

Không

## SUPERSEDED

Bị evidence mới thay thế

Không

Chỉ ACCEPTED mới được dùng để PASS.

## 49. EVIDENCE RECORD STANDARD

## 49.1. Evidence record tối thiểu

Mỗi evidence record phải có:

Trường

Ý nghĩa

Evidence ID

Mã evidence

Evidence Type

Implementation / Test / Smoke / Security / Release

Source Document

## MASTER / PACK / TECH

Source Rule

Rule cần chứng minh

Module

Module liên quan

Wave

Wave liên quan

Scope

Phạm vi

Environment

Môi trường

Version / Build / Config

Version liên quan

Test Case / Smoke Case

Case liên quan

Result

PASS / FAIL / bị chặn

Submitted By

Người nộp

Submitted At

Thời điểm nộp

Reviewed By

Người review

Reviewed At

Thời điểm review

Review Result

## ACCEPTED / REJECTED / NEEDS_RETEST

Linked P0

P0 liên quan nếu có

Linked Audit

Audit liên quan

File / Log / Report Reference

Link/file/log/report

Expiry / Validity

Còn hiệu lực hay không

## 49.2. Evidence binary và metadata

Foundation chỉ khóa nguyên tắc:

Evidence phải có metadata.

File/log/report phải có reference.

Không lưu dữ liệu nhạy cảm quá mức.

Evidence nội bộ không public.

Evidence supplier/customer/payment phải theo privacy policy.

Evidence file nếu có malware/security requirement phải pass scan theo chính sách.

Chi tiết storage sẽ do dev đề xuất ở implementation, nhưng không được trái nguyên tắc metadata, security và audit.

## 50. EVIDENCE LIFECYCLE

## 50.1. Lifecycle chuẩn

Evidence đi qua lifecycle:

## WAITING.

## SUBMITTED.

## UNDER_REVIEW.

ACCEPTED hoặc REJECTED.

NEEDS_RETEST nếu cần.

SUPERSEDED nếu bị thay thế.

EXPIRED nếu không còn hợp lệ.

## 50.2. Evidence không được tự ACCEPTED

Người submit evidence không được tự accepted nếu policy yêu cầu review độc lập.

Evidence P0/release/security/payment/customer data phải có reviewer đủ quyền.

Nếu chưa có reviewer:

## OWNER_REVIEW_REQUIRED

## 50.3. Evidence bị EXPIRED khi nào

Evidence bị hết hiệu lực nếu:

Code thay đổi.

Rule thay đổi.

Config runtime thay đổi.

Environment thay đổi.

Security issue mới phát sinh.

P0 liên quan mở lại.

Release scope thay đổi.

Owner yêu cầu retest.

Evidence quá cũ theo policy.

Smoke case không còn đúng.

Evidence expired không được dùng để release.

## 51. EVIDENCE P0 điểm chặn

Các lỗi sau là P0:

PASS không có evidence.

Evidence tự accepted.

Evidence không gắn module/source rule.

Evidence không rõ environment/version.

Evidence không có reviewer.

Evidence bị stale nhưng vẫn dùng.

Evidence fail nhưng Completion Report vẫn ghi PASS.

Evidence của DEV được dùng để approve PROD không scope.

Evidence không chứng minh rule P0.

Evidence có conflict nhưng vẫn release.

Evidence nội bộ bị public.

Evidence bị sửa/xóa không audit.

Nếu có:

## 52. P0 điểm chặn REGISTRY PRINCIPLE

## 52.1. P0 điểm chặn Registry là gì

P0 điểm chặn Registry là nơi ghi nhận lỗi/rủi ro chặn release.

P0 điểm chặn là vấn đề đủ nghiêm trọng để chặn:

Completion Decision.

Release Ready.

Production Ready.

Go-live Approved.

P0 không được xử lý bằng miệng.
P0 không được xóa im lặng.
P0 không được hạ cấp chỉ để chạy nhanh.

## 52.2. P0 điểm chặn Record tối thiểu

Trường

Ý nghĩa

điểm chặn ID

Mã P0

Source Document

## MASTER / PACK / TECH

Source Rule

Rule bị vi phạm

Module

Module liên quan

Scope

Phạm vi ảnh hưởng

Severity

## P0

Description

Mô tả

Impact

Ảnh hưởng nếu release

Detected By

Người/hệ thống phát hiện

Detected At

Thời điểm

Owner

Người chịu trách nhiệm

Status

OPEN / IN_PROGRESS / READY_FOR_RETEST / CLEARED

Fix Plan

Hướng xử lý

Fix Evidence

Evidence fix

Retest Evidence

Evidence retest

Review Result

Kết quả review

Release Impact

bị chặn / Hold / Excluded

Closed At

Thời điểm clear nếu có

## 53. P0 điểm chặn LIFECYCLE

## 53.1. Lifecycle chuẩn

P0 đi qua:

OPEN.

## INVESTIGATING.

## FIX_IN_PROGRESS.

## READY_FOR_RETEST.

RETEST_PASS hoặc RETEST_FAILED.

OWNER_REVIEW_REQUIRED nếu cần.

CLEARED nếu đủ điều kiện.

REOPENED nếu phát sinh lại.

## 53.2. Điều kiện clear P0

P0 chỉ được CLEARED khi có:

Root cause.

Fix description.

Fix evidence.

Retest evidence.

Smoke evidence nếu ảnh hưởng P0 flow.

Audit.

Reviewer.

Owner decision nếu ảnh hưởng business/release.

Không phát sinh P0 mới.

## 53.3. P0 không clear bằng “đã sửa rồi”

Các câu sau không được dùng để clear P0:

“Dev đã sửa rồi.”

“Chạy lại thấy được.”

“Không thấy lỗi nữa.”

“Tạm ổn.”

“Owner nói ok.”

“Để production rồi theo dõi.”

Phải có evidence.

## 54. RELEASE STATUS REGISTRY PRINCIPLE

## 54.1. Release Status Registry là gì

Release Status Registry quản lý trạng thái module/scope theo chuỗi:

Documentation.

Implementation.

Test.

Smoke.

Evidence.

## P0.

Security.

Dependency.

Owner review.

Release.

Production.

Go-live.

Release status không được sửa tay tùy ý.

## 54.2. Trạng thái chuẩn

Status

Ý nghĩa

## NOT_STARTED

Chưa bắt đầu

## SPEC_COMPLETE

Đặc tả xong

## IMPLEMENTATION_WAITING

Chưa triển khai

## IMPLEMENTATION_COMPLETE_ONLY

Code xong nhưng chưa test/smoke/evidence

## TEST_WAITING

Chưa test đủ

## SMOKE_WAITING

Chưa smoke đủ

## EVIDENCE_WAITING

Chưa evidence accepted

## SECURITY_WAITING

Chưa security review nếu cần

## P0_BLOCKED

Có P0 điểm chặn

## DEPENDENCY_BLOCKED

Upstream chưa clear

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## RELEASE_READY

Đủ điều kiện trình release

## RELEASE_DECISION_ACCEPTED

Owner duyệt release

## PRODUCTION_READINESS

Đủ điều kiện production theo scope

## GO_LIVE_DECISION_ACCEPTED

Được go-live theo scope

## RELEASE_HOLD

Tạm giữ release

## ROLLBACK_REQUIRED

Phải rollback

## EXCLUDED_FROM_SCOPE

Không nằm trong scope hiện tại

## 54.3. Trạng thái bị cấm

Không dùng:

Gần xong.

Cơ bản xong.

Tạm ổn.

Chạy được.

Test sơ bộ.

Pass miệng.

Owner ok miệng.

Production ready vì tài liệu xong.

Release thử.

Go-live thử.

## 55. RELEASE STATUS TRANSITION RULE

## 55.1. Không được nhảy trạng thái

Không được chuyển:

## SPEC_COMPLETE -> RELEASE_READY.

## IMPLEMENTATION_COMPLETE_ONLY -> PRODUCTION_READINESS.

## SMOKE_PASS -> GO_LIVE_DECISION_ACCEPTED.

## EVIDENCE_SUBMITTED -> RELEASE_DECISION_ACCEPTED.

## OWNER_REVIEW_REQUIRED -> GO_LIVE_DECISION_ACCEPTED.

Phải đi theo chuỗi kiểm soát:

SPEC_COMPLETE
-> IMPLEMENTATION_COMPLETE
-> TEST_PASS
-> SMOKE_PASS
-> EVIDENCE_ACCEPTED
-> P0_CLEARED
-> SECURITY_PASS nếu cần
-> DEPENDENCY_CLEARED
-> OWNER_SIGN_OFF
-> RELEASE_DECISION_ACCEPTED
-> PRODUCTION_READINESS
-> GO_LIVE_DECISION_ACCEPTED

## 55.2. Status transition phải audit

Các chuyển trạng thái sau phải audit:

Evidence accepted/rejected.

P0 điểm chặn cleared.

Release ready set.

Release approved.

Production ready.

Go-live approved.

Release hold.

Rollback required.

Scope excluded.

Owner review required.

## 56. OWNER DECISION RECORD

## 56.1. Owner decision là gì

Owner decision là bản ghi quyết định của người có thẩm quyền.

Owner decision không phải lời nói miệng.
Owner decision không phải tin nhắn không rõ scope.
Owner decision không phải “ok làm tiếp” nếu không ghi rõ điều kiện.

## 56.2. Owner Decision Record tối thiểu

Trường

Ý nghĩa

Decision ID

Mã quyết định

Owner

Người quyết định

Role / Authority

Quyền quyết định

Topic

Chủ đề

Module / Scope

Module/phạm vi

Source Evidence

Evidence liên quan

P0 Status

P0 liên quan

Dependency Status

Dependency

Decision Type

Accept / Reject / Hold / Approve / Exclude

Conditions

Điều kiện kèm theo

Effective Time

Thời điểm hiệu lực

Expiry

Thời điểm hết hiệu lực nếu có

Audit Ref

Audit liên quan

## 56.3. Owner decision bắt buộc khi nào

Bắt buộc khi:

Accept evidence P0.

Clear P0 liên quan business.

Approve release.

Approve go-live.

Exclude scope.

Accept risk.

Change IVR attempt/deadline.

Change payment/shipping/price/member policy.

Publish public trace policy.

Enable Ads scale.

Enable AI production.

Enable Facebook production routing.

Enable MC AI Live production.

Enable IVR production call.

## 57. FOUNDATION CONTROL SMOKE MATRIX

## 57.1. Audit Smoke

Smoke ID

Scenario

Expected Result

## FND-AUD-SMK-001

User grant permission

RBAC audit created

## FND-AUD-SMK-002

Payment confirm attempt

Payment audit created

## FND-AUD-SMK-003

Batch release attempt

Operational audit created

## FND-AUD-SMK-004

Evidence accept

Evidence audit created

## FND-AUD-SMK-005

Release approve

Release audit created

## FND-AUD-SMK-006

Access denied sensitive action

Security/access audit created

## FND-AUD-SMK-007

Admin override

Override audit created

## FND-AUD-SMK-008

Audit correction

New correction record, old audit unchanged

## 57.2. Idempotency Smoke

Smoke ID

Scenario

Expected Result

## FND-IDM-SMK-001

Same create order command retry

Return existing result, no duplicate

## FND-IDM-SMK-002

Same key different payload

Conflict rejected

## FND-IDM-SMK-003

Payment confirm retry

No duplicate payment confirmation

## FND-IDM-SMK-004

Material issue retry

No double inventory debit

## FND-IDM-SMK-005

IVR result retry

No duplicate order signal

## FND-IDM-SMK-006

Notification retry

No duplicate transactional notice

## FND-IDM-SMK-007

MISA sync retry

No duplicate accounting document

## FND-IDM-SMK-008

Missing key for required command

Rejected or bị chặn per policy

## 57.3. Evidence Smoke

Smoke ID

Scenario

Expected Result

## FND-EV-SMK-001

Submit evidence

Status SUBMITTED

## FND-EV-SMK-002

Reviewer accepts evidence

Status ACCEPTED, audit created

## FND-EV-SMK-003

Reviewer rejects evidence

Status REJECTED, reason required

## FND-EV-SMK-004

Evidence missing source rule

Rejected

## FND-EV-SMK-005

Evidence stale

## EXPIRED / NEEDS_RETEST

## FND-EV-SMK-006

Submitter self-accepts P0 evidence

bị chặn if policy requires reviewer

## FND-EV-SMK-007

Evidence linked to P0

P0 cannot clear without accepted evidence

## FND-EV-SMK-008

Evidence used for release

Only ACCEPTED evidence allowed

## 57.4. P0 điểm chặn Smoke

Smoke ID

Scenario

Expected Result

## FND-P0-SMK-001

Create P0 điểm chặn

Release status bị chặn

## FND-P0-SMK-002

Try release with P0 open

bị chặn

## FND-P0-SMK-003

Submit fix evidence

P0 still not cleared until review

## FND-P0-SMK-004

Retest pass + accepted

P0 can move to CLEARED

## FND-P0-SMK-005

Reopen P0

Release returns bị chặn/HOLD

## FND-P0-SMK-006

Clear P0 without evidence

bị chặn

## FND-P0-SMK-007

Downgrade P0 without owner

bị chặn

## FND-P0-SMK-008

P0 affects downstream

Downstream release bị chặn

## 57.5. Release Status Smoke

Smoke ID

Scenario

Expected Result

## FND-REL-SMK-001

Spec complete only

## FND-REL-SMK-002

Implementation complete only

## FND-REL-SMK-003

Smoke pass but evidence WAITING

## FND-REL-SMK-004

Evidence accepted but P0 open

## FND-REL-SMK-005

All gates pass

Release Ready allowed

## FND-REL-SMK-006

Owner approve release

Release Approved

## FND-REL-SMK-007

Owner approve go-live

Go-live Approved by scope

## FND-REL-SMK-008

Incident after release

Release Hold / Rollback Required

## 58. FOUNDATION CONTROL P0 điểm chặn

Các lỗi sau là P0 toàn Foundation:

Audit không append-only.

Command rủi ro không audit.

Command rủi ro không idempotency.

Evidence tự accepted.

Evidence không có reviewer.

Evidence fail nhưng status vẫn PASS.

P0 điểm chặn open nhưng release ready.

P0 điểm chặn clear không evidence.

Release status bị sửa tay không audit.

Owner decision không có record.

Go-live approved không có release decision.

Downstream production khi Foundation chưa clear.

Dashboard làm source-of-truth.

Documentation complete bị gọi production ready.

Service account vượt scope.

Access denied không ghi nhận ở action nhạy cảm.

Audit/evidence chứa dữ liệu nhạy cảm quá mức.

## 59. FOUNDATION DONE GATE CHO PHẦN 3/4

## PHẦN 3/4 được xem là hoàn tất khi đã khóa:

Audit Trail Principle.

Audit Scope.

Audit Record Standard.

Audit Category Model.

Audit P0 điểm chặn.

Idempotency Principle.

Idempotency Result Model.

Idempotency P0 điểm chặn.

Evidence Registry Principle.

Evidence Record Standard.

Evidence Lifecycle.

Evidence P0 điểm chặn.

P0 điểm chặn Registry Principle.

P0 điểm chặn Lifecycle.

Release Status Registry Principle.

Release Status Transition Rule.

Owner Decision Record.

Foundation Control Smoke Matrix.

Foundation Control P0 điểm chặn.

## 60. FOUNDATION FAIL GATE CHO PHẦN 3/4

## PHẦN 3/4 fail nếu đội kỹ thuật:

Xem audit là log kỹ thuật đơn giản.

Cho phép sửa/xóa audit.

Không audit command rủi ro.

Không idempotency cho command rủi ro.

Cho retry tạo trùng business object.

Dùng evidence không review để PASS.

Cho evidence tự accepted.

Cho P0 clear không fix/retest evidence.

Cho release status nhảy cóc.

Ghi release ready khi evidence WAITING.

Ghi production ready khi chỉ implementation complete.

Không có owner decision record.

Không smoke Foundation control.

Không có evidence cho audit/idempotency/evidence/P0/release status.

Nếu xảy ra:

## 61. KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 của TECH-01 đã khóa lớp kiểm soát nền cho Audit, Idempotency, Evidence Registry, P0 điểm chặn Registry và Release Status Registry.

Kết luận bắt buộc:

Audit là lịch sử kiểm soát, không phải log debug.

Audit phải append-only.

Command rủi ro phải audit.

Command rủi ro phải idempotent.

Idempotency phải chặn retry tạo trùng.

Evidence phải có metadata, status, reviewer và source rule.

Chỉ Evidence ACCEPTED mới được dùng để PASS.

P0 điểm chặn phải chặn release.

P0 không được clear nếu thiếu fix/retest/smoke evidence.

Release status phải theo module/scope.

Release status không được nhảy cóc.

Owner decision phải có record.

Foundation control phải có smoke riêng.

Nếu Foundation control chưa clear, downstream production phải bị chặn.

Documentation Complete không phải Production Ready.

Trạng thái sau PHẦN 3/4:

## PHẦN 4/4 - FINAL FOUNDATION SMOKE MATRIX / DONE GATE / FAIL GATE / DEV DELIVERY CHECKLIST / TECH-01 FINAL CONCLUSION

## 62. MỤC ĐÍCH CỦA PHẦN 4/4

## PHẦN 4/4 là phần khóa cuối của TECH-01.

Nếu PHẦN 1/4 đã khóa nguyên tắc Foundation, PHẦN 2/4 đã khóa RBAC / User / Role / Permission / Access Scope, PHẦN 3/4 đã khóa Audit / Idempotency / Evidence / P0 điểm chặn / Release Status, thì PHẦN 4/4 khóa lớp kiểm tra hoàn tất:

Foundation Smoke Matrix tổng thể.

RBAC Smoke Matrix.

Audit Smoke Matrix.

Idempotency Smoke Matrix.

Evidence Registry Smoke Matrix.

P0 điểm chặn Smoke Matrix.

Release Status Smoke Matrix.

Owner Decision Smoke Matrix.

Dev Delivery Checklist.

Foundation Done Gate.

Foundation Fail Gate.

Foundation Release Readiness Rule.

Foundation Production Readiness Rule.

TECH-01 Final Conclusion.

## PHẦN 4/4 không viết code.
PHẦN 4/4 không thiết kế API chi tiết.
PHẦN 4/4 không thiết kế database chi tiết.
PHẦN 4/4 không thiết kế UI chi tiết.

## PHẦN 4/4 chỉ khóa điều kiện để đội kỹ thuật sau này biết: Foundation làm xong thế nào mới được xem là đủ điều kiện cho các TECH downstream bắt đầu production scope.

## 63. FOUNDATION FINAL SMOKE PRINCIPLE

## 63.1. Smoke Foundation là gì

Foundation Smoke là bộ kiểm tra chứng minh lớp nền vận hành đúng trước khi Product, Operational Core, Commerce, AI, Facebook Gateway, Ads, MC AI Live và IVR phụ thuộc vào nó.

Foundation Smoke không phải kiểm tra giao diện.
Foundation Smoke không phải login thử.
Foundation Smoke không phải xem màn hình role/permission đã hiện.
Foundation Smoke không phải demo miệng.

Foundation Smoke phải chứng minh được:

User không có quyền thì bị chặn.

User có quyền nhưng guard fail thì vẫn bị chặn.

Action rủi ro có audit.

Command rủi ro có idempotency.

Evidence không tự accepted.

P0 điểm chặn chặn release.

Release status không nhảy cóc.

Owner decision có record.

Disabled user không thao tác được.

Service account không vượt scope.

Break-glass nếu có phải có reason, expiry, audit.

Downstream chưa được production nếu Foundation chưa clear.

## 63.2. Foundation Smoke phải kiểm tra cả bị chặn path

Không được chỉ test happy path.

Foundation Smoke phải kiểm tra:

Happy path.

Access denied path.

Permission missing path.

Guard fail path.

Disabled user path.

Service account out-of-scope path.

Duplicate command path.

Evidence rejected path.

P0 open path.

Release bị chặn path.

Owner review required path.

Audit correction path.

Nếu chỉ test happy path:

## FOUNDATION_SMOKE_STATUS = INCOMPLETE

## 64. FOUNDATION FINAL SMOKE MATRIX

## 64.1. Smoke nhóm Authentication / User

Smoke ID

Scenario

Expected Result

## FND-AUTH-SMK-001

User chưa xác thực gọi action nhạy cảm

## ACCESS_DENIED

## FND-AUTH-SMK-002

User ACTIVE đăng nhập hợp lệ

Login success, audit created

## FND-AUTH-SMK-003

User DISABLED đăng nhập

Login bị chặn, audit created

## FND-AUTH-SMK-004

User SUSPENDED thực hiện command

Command bị chặn

## FND-AUTH-SMK-005

User bị LOCKED_BY_SECURITY

Không được thao tác production

## FND-AUTH-SMK-006

Failed login nhiều lần

Security audit created

## FND-AUTH-SMK-007

Service account dùng ngoài scope

bị chặn

## FND-AUTH-SMK-008

Service account action hợp lệ

Allowed only within scope, audit created

## 64.2. Smoke nhóm RBAC / Permission

Smoke ID

Scenario

Expected Result

## FND-RBAC-SMK-001

User không có permission batch.release

bị chặn

## FND-RBAC-SMK-002

User có permission batch.release nhưng batch chưa đủ guard

bị chặn

## FND-RBAC-SMK-003

User có permission và guard pass

Action allowed

## FND-RBAC-SMK-004

Permission bị revoke

Action bị chặn sau revoke

## FND-RBAC-SMK-005

Role được assign permission mới

Audit created

## FND-RBAC-SMK-006

Role bị gỡ permission

Audit created, action bị chặn

## FND-RBAC-SMK-007

UI ẩn nút nhưng backend vẫn enforce

Backend blocks unauthorized request

## FND-RBAC-SMK-008

Supplier user truy cập supplier khác

bị chặn

## FND-RBAC-SMK-009

Ads operator approve scale khi không có quyền

bị chặn

## FND-RBAC-SMK-010

IVR operator cập nhật order trực tiếp

bị chặn

## 64.3. Smoke nhóm Access Scope

Smoke ID

Scenario

Expected Result

## FND-SCOPE-SMK-001

User có quyền ở STAGING nhưng thao tác PROD

bị chặn

## FND-SCOPE-SMK-002

User có quyền ở Page test nhưng thao tác Page production

bị chặn

## FND-SCOPE-SMK-003

Warehouse user thao tác warehouse ngoài scope

bị chặn

## FND-SCOPE-SMK-004

Supplier user xem dữ liệu ngoài supplier_id

bị chặn

## FND-SCOPE-SMK-005

Payment reviewer không thuộc scope thanh toán

bị chặn

## FND-SCOPE-SMK-006

AI manager sửa config ngoài channel scope

bị chặn

## FND-SCOPE-SMK-007

Live operator bật live production khi chỉ có rehearsal scope

bị chặn

## FND-SCOPE-SMK-008

User có scope đúng và guard pass

Allowed, audit created

## 64.4. Smoke nhóm Audit

Smoke ID

Scenario

Expected Result

## FND-AUD-SMK-001

Grant permission

RBAC audit created

## FND-AUD-SMK-002

Revoke permission

RBAC audit created

## FND-AUD-SMK-003

Batch release attempt

Operational audit created

## FND-AUD-SMK-004

Payment confirm attempt

Payment audit created

## FND-AUD-SMK-005

Evidence accept

Evidence audit created

## FND-AUD-SMK-006

Release approve

Release audit created

## FND-AUD-SMK-007

Access denied sensitive action

Security audit created

## FND-AUD-SMK-008

Admin override

Override audit created

## FND-AUD-SMK-009

Audit correction

New correction record; old audit unchanged

## FND-AUD-SMK-010

Attempt to edit audit history

bị chặn

## 64.5. Smoke nhóm Idempotency

Smoke ID

Scenario

Expected Result

## FND-IDM-SMK-001

Create order retry same key/same payload

Return existing result, no duplicate

## FND-IDM-SMK-002

Create order same key/different payload

## IDEMPOTENCY_CONFLICT

## FND-IDM-SMK-003

Confirm payment retry

No duplicate payment confirmation

## FND-IDM-SMK-004

Material issue retry

No double inventory debit

## FND-IDM-SMK-005

Warehouse receipt retry

No duplicate warehouse credit

## FND-IDM-SMK-006

IVR result retry

No duplicate order signal

## FND-IDM-SMK-007

Send notification retry

No duplicate transactional notice

## FND-IDM-SMK-008

MISA sync retry

No duplicate accounting document

## FND-IDM-SMK-009

Accept evidence retry

No conflicting evidence status

## FND-IDM-SMK-010

Required idempotency key missing

Command bị chặn or rejected by policy

## 64.6. Smoke nhóm Evidence Registry

Smoke ID

Scenario

Expected Result

## FND-EV-SMK-001

Submit evidence

Status = SUBMITTED

## FND-EV-SMK-002

Accept evidence by authorized reviewer

Status = ACCEPTED, audit created

## FND-EV-SMK-003

Reject evidence

Status = REJECTED, reason required

## FND-EV-SMK-004

Evidence missing source rule

Rejected

## FND-EV-SMK-005

Evidence missing environment/version

Rejected

## FND-EV-SMK-006

Evidence stale after code change

## EXPIRED / NEEDS_RETEST

## FND-EV-SMK-007

Submitter self-accepts P0 evidence

bị chặn if review separation required

## FND-EV-SMK-008

Use SUBMITTED evidence for PASS

bị chặn

## FND-EV-SMK-009

Use ACCEPTED evidence for Release Ready

Allowed only if other gates pass

## FND-EV-SMK-010

Evidence deleted/edited without audit

bị chặn

## 64.7. Smoke nhóm P0 điểm chặn Registry

Smoke ID

Scenario

Expected Result

## FND-P0-SMK-001

Create P0 điểm chặn

Release status bị chặn

## FND-P0-SMK-002

Try release with P0 OPEN

bị chặn

## FND-P0-SMK-003

Submit fix evidence

P0 still not cleared until review

## FND-P0-SMK-004

Retest pass + evidence accepted

P0 can move to CLEARED

## FND-P0-SMK-005

Clear P0 without evidence

bị chặn

## FND-P0-SMK-006

Downgrade P0 without owner

bị chặn

## FND-P0-SMK-007

Reopen P0 after regression

Release returns bị chặn/HOLD

## FND-P0-SMK-008

P0 affects downstream

Downstream release bị chặn

## FND-P0-SMK-009

P0 waived with condition

Owner decision required

## FND-P0-SMK-010

P0 waiver on forbidden critical risk

bị chặn

## 64.8. Smoke nhóm Release Status Registry

Smoke ID

Scenario

Expected Result

## FND-REL-SMK-001

SPEC_COMPLETE only

## FND-REL-SMK-002

## IMPLEMENTATION_COMPLETE_ONLY

## FND-REL-SMK-003

Test pass but smoke WAITING

## FND-REL-SMK-004

Smoke pass but evidence WAITING

## FND-REL-SMK-005

Evidence accepted but P0 open

## FND-REL-SMK-006

All gates pass

RELEASE_READY allowed

## FND-REL-SMK-007

Owner approves release

## RELEASE_DECISION_ACCEPTED

## FND-REL-SMK-008

Owner approves go-live by scope

GO_LIVE_DECISION_ACCEPTED by scope

## FND-REL-SMK-009

Incident after release

## RELEASE_HOLD / ROLLBACK_REQUIRED

## FND-REL-SMK-010

Try jump SPEC_COMPLETE -> PRODUCTION_READINESS

bị chặn

## 64.9. Smoke nhóm Owner Decision

Smoke ID

Scenario

Expected Result

## FND-OWN-SMK-001

Owner accepts evidence

Decision record created

## FND-OWN-SMK-002

Owner rejects evidence

Reason required

## FND-OWN-SMK-003

Owner approves release

Release decision record created

## FND-OWN-SMK-004

Owner approves limited go-live

Scope must be explicit

## FND-OWN-SMK-005

Owner excludes scope

Excluded scope recorded

## FND-OWN-SMK-006

Owner requires retest

Status = NEEDS_RETEST

## FND-OWN-SMK-007

Owner holds release

Status = RELEASE_HOLD

## FND-OWN-SMK-008

Owner requires rollback

Status = ROLLBACK_REQUIRED

## FND-OWN-SMK-009

Decision without evidence reference

## OWNER_REVIEW_INVALID

## FND-OWN-SMK-010

Verbal approval only

Not accepted

## 64.10. Smoke nhóm Break-glass

Smoke ID

Scenario

Expected Result

## FND-BG-SMK-001

Enable break-glass without reason

bị chặn

## FND-BG-SMK-002

Enable break-glass without expiry

bị chặn

## FND-BG-SMK-003

Break-glass used

Audit created

## FND-BG-SMK-004

Break-glass expired

Access bị chặn

## FND-BG-SMK-005

Break-glass used to bypass payment rule

bị chặn

## FND-BG-SMK-006

Break-glass used to bypass recall/sale lock

bị chặn

## FND-BG-SMK-007

Post-review missing

Owner review required

## FND-BG-SMK-008

Break-glass scope exceeded

bị chặn

## 65. FOUNDATION EVIDENCE PACKAGE

## 65.1. Foundation Evidence Package là gì

Foundation Evidence Package là gói bằng chứng chứng minh Foundation đã được triển khai, test, smoke và review đủ để downstream có thể bắt đầu phụ thuộc.

Foundation Evidence Package không đồng nghĩa Production Ready.
Nó chỉ là điều kiện để xét Foundation Release Ready.

## 65.2. Foundation Evidence Package bắt buộc gồm

Evidence Group

Nội dung

## EV-FND-RBAC

Bằng chứng user/role/permission/scope/backend enforcement

## EV-FND-AUDIT

Bằng chứng audit command rủi ro

## EV-FND-IDM

Bằng chứng idempotency command rủi ro

## EV-FND-EVIDENCE

Bằng chứng evidence lifecycle

## EV-FND-P0

Bằng chứng P0 điểm chặn lifecycle

## EV-FND-RELEASE

Bằng chứng release status transition

## EV-FND-OWNER

Bằng chứng owner decision record

## EV-FND-SECURITY

Bằng chứng security baseline

## EV-FND-SERVICE

Bằng chứng service account scope

## EV-FND-BREAKGLASS

Bằng chứng break-glass nếu bật

## 65.3. Foundation Evidence không được thiếu source rule

Mỗi evidence phải gắn với:

TECH-01 rule.

PACK hoặc MASTER liên quan nếu có.

Module/scope liên quan.

Smoke case liên quan.

Reviewer.

Status.

Nếu không gắn source rule:

## EVIDENCE_STATUS = REJECTED

## 66. FOUNDATION DEV DELIVERY CHECKLIST

## 66.1. Checklist dev phải nộp sau TECH-01

Đội kỹ thuật phải nộp:

Checklist Item

Required

Foundation Module Map

Có

User Model Spec

Có

Role Model Spec

Có

Permission Model Spec

Có

Access Scope Spec

Có

Guard Model Spec

Có

Service Account Spec

Có

Break-glass Policy Spec

Có nếu dùng

Audit Standard Implementation Plan

Có

Idempotency Standard Implementation Plan

Có

Evidence Registry Implementation Plan

Có

P0 điểm chặn Registry Implementation Plan

Có

Release Status Registry Implementation Plan

Có

Owner Decision Record Plan

Có

RBAC Smoke Plan

Có

Audit Smoke Plan

Có

Idempotency Smoke Plan

Có

Evidence Smoke Plan

Có

P0 điểm chặn Smoke Plan

Có

Release Status Smoke Plan

Có

Foundation P0 điểm chặn Baseline

Có

Foundation Evidence Package Plan

Có

## 66.2. Không có checklist thì không bắt đầu downstream

Nếu thiếu Foundation Dev Delivery Checklist:

## 67. FOUNDATION DONE GATE

## 67.1. Foundation Spec Done Gate

TECH-01 được xem là Specification Complete khi có đủ:

Foundation principles.

RBAC model.

User model.

Role model.

Permission model.

Access scope model.

Guard model.

Audit standard.

Idempotency standard.

Evidence registry standard.

P0 điểm chặn registry standard.

Release status registry standard.

Owner decision record standard.

Smoke matrix.

Evidence package rule.

Dev delivery checklist.

Done Gate / Fail Gate.

Final conclusion.

## 67.2. Foundation Implementation Done Gate

Foundation chỉ được Implementation Done khi:

User/RBAC implemented.

Permission backend enforcement implemented.

Access scope implemented.

Audit implemented.

Idempotency implemented for required commands.

Evidence Registry implemented.

P0 điểm chặn Registry implemented.

Release Status Registry implemented.

Owner Decision Record implemented.

Security baseline implemented.

Service account control implemented.

Break-glass implemented if enabled.

Implementation evidence submitted.

Implementation evidence accepted.

Nếu mới code xong nhưng chưa test/smoke/evidence:

## FOUNDATION_STATUS = IMPLEMENTATION_COMPLETE_ONLY

## 67.3. Foundation Test Done Gate

Foundation Test Done khi:

RBAC tests pass.

Audit tests pass.

Idempotency tests pass.

Evidence lifecycle tests pass.

P0 lifecycle tests pass.

Release status transition tests pass.

Owner decision record tests pass.

Service account scope tests pass.

Break-glass tests pass if enabled.

Test evidence accepted.

## 67.4. Foundation Smoke Done Gate

Foundation Smoke Done khi:

All RBAC smoke PASS.

All Audit smoke PASS.

All Idempotency smoke PASS.

All Evidence smoke PASS.

All P0 điểm chặn smoke PASS.

All Release Status smoke PASS.

All Owner Decision smoke PASS.

All Security-sensitive smoke PASS.

Smoke evidence accepted.

No P0 Foundation điểm chặn open.

## 67.5. Foundation Release Ready Gate

Foundation chỉ được Release Ready khi:

Specification complete.

Implementation complete.

Test complete.

Smoke pass.

Evidence accepted.

Security pass.

P0 cleared.

Dependency cleared.

Owner review complete.

Release handoff package submitted.

No unresolved Foundation conflict.

No stale evidence.

Nếu thiếu một điều kiện:

## 67.6. Foundation Production Ready Gate

Foundation chỉ được Production Ready khi:

Environment ready.

Security ready.

Monitoring ready.

Incident owner ready.

Rollback/recovery ready.

Owner final decision có hiệu lực.

No P0 điểm chặn.

No release hold.

No dependency bị chặn.

Scope rõ.

Nếu thiếu một điều kiện:

## 68. FOUNDATION FAIL GATE

## 68.1. Fail Gate tổng thể

Foundation phải FAIL nếu có một trong các lỗi:

Không có action-level permission.

Backend không enforce permission.

UI là lớp chặn quyền duy nhất.

Role hardcode bypass permission.

User disabled vẫn thao tác được.

Service account vượt scope.

Supplier user vượt supplier scope.

Command rủi ro không audit.

Audit có thể sửa/xóa.

Command rủi ro không idempotency.

Retry tạo duplicate business object.

Evidence tự accepted.

Evidence không có reviewer.

Evidence thiếu source rule.

P0 điểm chặn open nhưng release ready.

P0 clear không evidence.

Release status nhảy cóc.

Owner decision không có record.

Break-glass không reason/expiry/audit.

Documentation complete bị gọi production ready.

## 68.2. Fail Gate theo nhóm

Nhóm

Fail Condition

Impact

## RBAC

Permission không enforce backend

Foundation bị chặn

Audit

Command rủi ro không audit

Foundation bị chặn

Idempotency

Retry tạo trùng

Foundation bị chặn

Evidence

Evidence self-accepted

Release bị chặn

## P0

P0 open nhưng release ready

Release invalid

Release Status

Status nhảy cóc

Release invalid

Owner Decision

Verbal approval

Not accepted

Service Account

Vượt scope

Security bị chặn

Break-glass

Không expiry/audit

Security bị chặn

## 68.3. Foundation fail thì downstream bị chặn

Nếu Foundation fail:

## 69. FOUNDATION RELEASE HANDOFF PACKAGE

## 69.1. Release Handoff Package cần có

Trước khi trình Foundation Release Ready, dev phải nộp:

Foundation scope.

Module list.

User/RBAC implementation summary.

Permission matrix.

Access scope matrix.

Audit implementation summary.

Idempotency implementation summary.

Evidence Registry summary.

P0 điểm chặn Registry summary.

Release Status Registry summary.

Owner Decision Record summary.

Security review summary.

Smoke results.

Evidence list.

P0 status.

Dependency status.

Known risks.

Excluded scope.

Rollback/recovery note.

Owner questions.

## 69.2. Proposed Foundation Release Status

Dev chỉ được đề xuất một trong các trạng thái:

Proposed Status

Khi dùng

bị chặn

Còn P0/dependency/evidence missing

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## NEEDS_RETEST

Smoke/test/evidence chưa ổn

## RELEASE_READY

Đủ điều kiện trình release

## EXCLUDED_FROM_SCOPE

Scope không nằm trong release

## RELEASE_HOLD

Có incident/risk cần giữ

Không được đề xuất:

production ready;

go-live approved;

xong hết;

chạy được;

pass tạm.

## 70. FOUNDATION OWNER REVIEW POINTS

Owner cần review các điểm sau:

Danh sách role baseline.

Permission baseline.

Separation of duties.

Ai được confirm payment.

Ai được release batch.

Ai được accept evidence.

Ai được clear P0.

Ai được approve release.

Ai được approve go-live.

Break-glass có bật không.

Supplier Portal có scope không.

Service account nào được tạo.

Environment nào được dùng dữ liệu thật.

Foundation scope nào release trước.

Scope nào excluded.

Nếu owner chưa quyết các điểm ảnh hưởng P0:

## OWNER_REVIEW_REQUIRED

## 71. FOUNDATION SECURITY REVIEW CHECKLIST

Security review cần kiểm tra:

Authentication.

Password/session/token policy.

User status enforcement.

Permission enforcement.

Access scope enforcement.

Service account scope.

Sensitive action audit.

Sensitive data exposure.

Evidence privacy.

Audit data minimization.

Public/private data boundary.

Break-glass control.

Export control.

Environment data boundary.

Access denied logging.

Admin override control.

Release approval control.

Nếu security review chưa pass:

## 72. FOUNDATION MONITORING / INCIDENT BASELINE

## 72.1. Monitoring tối thiểu

Foundation production scope cần theo dõi:

Failed login spike.

Access denied spike.

Permission changes.

Role assignments.

Service account usage.

Audit write failure.

Idempotency conflict spike.

Evidence rejection spike.

P0 điểm chặn created/reopened.

Release status change.

Owner decision record.

Break-glass usage.

## 72.2. Incident Foundation

Các incident Foundation cần xử lý ngay:

Audit write failure.

Permission bypass.

Evidence self-accept.

Release status changed without audit.

Service account scope breach.

P0 điểm chặn cleared without evidence.

Break-glass misuse.

Sensitive data leak.

Idempotency failure causing duplicate.

Owner decision missing for release.

Nếu incident chưa phân loại:

## 73. FOUNDATION DOWNSTREAM READINESS RULE

## 73.1. Khi nào downstream được bắt đầu phụ thuộc Foundation

Downstream chỉ được phụ thuộc Foundation ở mức triển khai khi:

TECH-01 specification complete.

Foundation module map có.

Permission model baseline có.

Audit model baseline có.

Idempotency model baseline có.

Evidence/P0/release status model có.

Owner cho phép bắt đầu wave downstream ở mức documentation/implementation planning.

Downstream production thì cần Foundation implemented/tested/smoked/evidence accepted.

## 73.2. Downstream production cần Foundation release

Các module sau không được production nếu Foundation chưa Release Ready/Approved theo scope:

Product activation.

Recipe approval.

Batch release.

Warehouse receive.

Inventory adjustment.

Quote/order/payment.

AI Advisor production.

Facebook Gateway production.

Ads scale.

MC AI Live production.

IVR production call.

Evidence accept/release approve.

## 74. TECH-01 FINAL ACCEPTANCE CRITERIA

TECH-01 được xem là Documentation Complete khi đã có đủ:

Foundation principles.

Core platform boundary.

Security baseline.

RBAC model.

User model.

Role model.

Permission model.

Access scope model.

Guard model.

Separation of duties.

Break-glass principle.

Audit standard.

Idempotency standard.

Evidence Registry standard.

P0 điểm chặn Registry standard.

Release Status Registry standard.

Owner Decision Record standard.

Foundation Smoke Matrix.

Foundation Evidence Package.

Dev Delivery Checklist.

Done Gate.

Fail Gate.

Release Handoff Package.

Owner Review Points.

Security Review Checklist.

Monitoring / Incident Baseline.

Downstream Readiness Rule.

Final Conclusion.

Sau khi TECH-01 hoàn tất tài liệu, trạng thái đúng là:

Status Group

Value

## TECH-01_DOCUMENTATION_STATUS

## COMPLETE

## FOUNDATION_SPEC_STATUS

## COMPLETE

## RBAC_SPEC_STATUS

## COMPLETE

## AUDIT_SPEC_STATUS

## COMPLETE

## IDEMPOTENCY_SPEC_STATUS

## COMPLETE

## EVIDENCE_REGISTRY_SPEC_STATUS

## COMPLETE

## P0_STOP_POINT_REGISTRY_SPEC_STATUS

## COMPLETE

## RELEASE_STATUS_REGISTRY_SPEC_STATUS

## COMPLETE

## OWNER_DECISION_RECORD_SPEC_STATUS

## COMPLETE

## IMPLEMENTATION_STATUS

## WAITING

## TEST_STATUS

## WAITING

## SMOKE_STATUS

## WAITING

## SECURITY_STATUS

## WAITING

## EVIDENCE_STATUS

## WAITING

## FOUNDATION_RELEASE_READY

KHONG

## FOUNDATION_RELEASE_DECISION_ACCEPTED

KHONG

## FOUNDATION_PRODUCTION_READINESS

KHONG

## GO_LIVE_DECISION_ACCEPTED

KHONG

## 76. TECH-01 FINAL CONCLUSION

TECH-01 đã khóa:

Foundation là upstream bắt buộc của toàn hệ.

Foundation không thay domain nghiệp vụ.

RBAC không phải admin/user đơn giản.

Permission phải ở cấp action.

Role không được hardcode để bypass permission.

Access scope phải giới hạn quyền theo môi trường, tổ chức, supplier, warehouse, SKU, channel, payment, release scope.

Backend/application layer phải enforce permission.

Guard nghiệp vụ vẫn phải chạy dù user có quyền.

Separation of duties cần áp dụng cho action rủi ro.

Break-glass nếu dùng phải có reason, scope, expiry, audit và post-review.

Audit là lịch sử kiểm soát, không phải log debug.

Audit phải append-only.

Command rủi ro phải audit.

Command rủi ro phải idempotent.

Idempotency phải chặn retry tạo trùng order, payment, inventory, IVR result, notification, MISA sync, evidence, release.

Evidence phải có metadata, source rule, environment, version, reviewer và status.

Chỉ Evidence ACCEPTED mới được dùng để PASS.

P0 điểm chặn phải chặn release.

P0 không được clear nếu thiếu fix/retest/smoke evidence.

Release status phải theo module/scope.

Release status không được nhảy cóc.

Owner decision phải có record.

Security-sensitive action phải có permission/audit.

Foundation Smoke Matrix là bắt buộc.

Foundation Evidence Package là bắt buộc.

Foundation Release Handoff Package là bắt buộc.

Downstream production bị bị chặn nếu Foundation chưa clear.

Documentation Complete không phải Production Ready.

Kết luận cuối:

TECH-01 hoàn tất về mặt tài liệu kỹ thuật Foundation.
TECH-01 chưa đồng nghĩa Foundation đã được triển khai.
TECH-01 chưa đồng nghĩa Foundation Release Ready.
TECH-01 chưa đồng nghĩa Foundation Production Ready.
Sau TECH-01, bước tiếp theo là viết TECH-02 - Product / SKU / Ingredient / Recipe / Formula / Product Activation theo cùng chuẩn TECH-00 và TECH-01.
Dev chỉ được dùng TECH-01 để lập Foundation Module Map, RBAC Matrix, Audit Plan, Idempotency Plan, Evidence Plan, Smoke Plan và Gap Report trước khi code sâu.

Trạng thái cuối bắt buộc:

## FOUNDATION / RBAC / AUDIT / IDEMPOTENCY / EVIDENCE REGISTRY

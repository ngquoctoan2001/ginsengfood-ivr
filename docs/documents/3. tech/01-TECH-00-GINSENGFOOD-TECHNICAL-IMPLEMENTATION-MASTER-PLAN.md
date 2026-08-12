# TECH-00 - GINSENGFOOD TECHNICAL IMPLEMENTATION MASTER PLAN

## Pham Vi Tai Lieu

Tài liệu này là bản rewrite clean từ nguồn TECH, giữ phạm vi kỹ thuật của section và chuẩn hóa wording để không xung đột với MASTER/PACK.

## SYSTEM ARCHITECTURE / MODULE BOUNDARY / REPO MAPPING / GREENFIELD IMPLEMENTATION STANDARD

## PHẦN 1/4 - GREENFIELD TECHNICAL PRINCIPLES / ARCHITECTURE BOUNDARY / NO-CODE-YET RULE

## 1. MỤC ĐÍCH CỦA TECH-00

TECH-00 là tài liệu kỹ thuật cha dùng để định hướng triển khai mới hoàn toàn hệ thống Ginsengfood theo bộ MASTER / PACK / DEV EXECUTION COMMAND PACK đã khóa.

TECH-00 không phải tài liệu code.
TECH-00 không phải tài liệu API chi tiết.
TECH-00 không phải tài liệu database chi tiết.
TECH-00 không phải tài liệu UI chi tiết.
TECH-00 không phải bản chỉnh sửa tạm tài liệu cũ.
TECH-00 không dựa trên logic triển khai cũ nếu logic đó không còn phù hợp.

Vai trò đúng của TECH-00 là:

Khóa nguyên tắc triển khai kỹ thuật theo hướng greenfield.

Xác định ranh giới kiến trúc tổng thể.

Xác định cách chia module kỹ thuật.

Xác định cách map MASTER / PACK sang hệ thống triển khai thật.

Xác định cách dev đọc tài liệu mới.

Xác định cách dev lập module map, dependency map, gap report.

Ngăn việc quay lại tài liệu cũ làm rối tư duy triển khai.

Ngăn việc code trước khi hiểu source-of-truth.

Ngăn việc copy-paste code rời rạc.

Chuẩn bị nền cho TECH-01 đến TECH-10.

TECH-00 là lớp chuyển đổi từ:

Governance / Domain / Business Rule

sang:

Technical Implementation Thinking / Module Boundary / Repo Mapping / Dev Execution Standard

## 2. QUYẾT ĐỊNH GREENFIELD TECHNICAL RESET

## 2.1. Quyết định làm mới hoàn toàn

Từ TECH-00 trở đi, toàn bộ tài liệu kỹ thuật triển khai Ginsengfood được viết theo hướng:

## GREENFIELD TECHNICAL IMPLEMENTATION

Nghĩa là:

Viết mới theo MASTER / PACK đã khóa.

Không chỉnh sửa tạm tài liệu kỹ thuật cũ.

Không dùng tài liệu cũ làm nền kiến trúc.

Không bắt dev đọc tài liệu cũ để hiểu hệ thống mới.

Không giữ logic cũ nếu logic đó trái source-of-truth mới.

Không triển khai theo tư duy “sửa tiếp cái đã có”.

Không để code cũ dẫn dắt nghiệp vụ mới.

Tài liệu cũ, nếu còn tồn tại, chỉ được xem là:

## LEGACY_REFERENCE_ONLY

Code cũ, nếu còn tồn tại, chỉ được xem là:

## CURRENT_IMPLEMENTATION_STATE

Không được xem là:

## BUSINESS_SOURCE_OF_TRUTH

## 2.2. Lý do phải greenfield

Phải làm mới vì hệ thống Ginsengfood hiện tại đã được khóa lại theo một tư duy khác trước:

Có MASTER governance.

Có PACK domain boundary.

Có Operational Core.

Có Product / SKU / Recipe / Formula control.

Có Demand / MRP / Procurement control.

Có Commerce Runtime.

Có AI Advisor boundary.

Có Facebook Gateway boundary.

Có Ads Measurement boundary.

Có MC AI Live boundary.

Có IVR Order Confirmation boundary.

Có Completion / Evidence / Gateway / Release Readiness.

Có Dev Execution theo wave.

Có quy tắc evidence, smoke, owner sign-off, release decision.

Nếu tiếp tục dùng tài liệu cũ, dev sẽ bị rối ở các điểm:

tên module cũ có thể không còn đúng;

logic cũ có thể không còn phù hợp;

rule cũ có thể trái PACK mới;

AI có thể bị hiểu là tự quyết định giá/đơn hàng;

Facebook Gateway có thể bị hiểu sai thành nơi tạo order;

Ads có thể bị hiểu sai là nguồn doanh thu;

IVR có thể bị hiểu sai là nơi hủy đơn;

dashboard có thể bị hiểu sai là source-of-truth;

documentation complete có thể bị hiểu sai là production ready.

Vì vậy, greenfield là cách nhanh hơn, sạch hơn và ít rủi ro hơn.

## 3. NGUYÊN TẮC SOURCE-OF-TRUTH KỸ THUẬT

## 3.1. Nguồn sự thật được phép dùng

Từ TECH-00 trở đi, dev chỉ được dùng các nguồn sau làm căn cứ triển khai:

MASTER-00 đến MASTER-07 đã khóa.

PACK-01 đến PACK-10 đã khóa.

TECH-00 đến TECH-10 sau khi được viết và khóa.

Chính sách thành viên / giá / chương trình / hoa hồng đã owner xác nhận.

Business Pack / Functional Pack / UI Pack đã được owner xác nhận.

Owner decision mới nếu có conflict hoặc thay đổi scope.

## 3.2. Nguồn không được dùng làm business truth

Các nguồn sau không được dùng làm business truth:

Tài liệu cũ của kỹ sư trước.

Code cũ.

UI cũ.

Database cũ.

API cũ.

Dashboard cũ.

File nháp chưa owner xác nhận.

Lời trao đổi miệng không có record.

Ghi chú rời rạc không được đưa vào MASTER / PACK / TECH.

Cách hiểu cá nhân của dev.

Nếu phát hiện tài liệu cũ hoặc code cũ khác tài liệu mới, mặc định:

## NEW SOURCE-OF-TRUTH WINS

Nếu còn nghi ngờ:

## OWNER_REVIEW_REQUIRED

## 4. NO-CODE-YET RULE

## 4.1. Chưa được code sâu trước TECH mapping

Dev không được bắt đầu code sâu ngay sau khi nhận tài liệu.

Trước khi code, dev phải nộp tối thiểu:

Technical module map.

Pack-to-module map.

Repo/app boundary map.

Upstream/downstream dependency map.

P0 điểm chặn list.

Legacy conflict list nếu có.

Implementation sequence.

Evidence plan.

Smoke plan.

Owner question list.

Nếu thiếu các mục trên:

## 4.2. Không copy-paste code AI

Dev không được xây hệ thống bằng cách:

hỏi AI sinh code;

copy code;

dán vào repo;

sửa lỗi phát sinh từng chỗ;

ghép module rời rạc.

AI có thể hỗ trợ giải thích, rà logic, gợi ý hướng triển khai, tạo checklist, tạo test case, nhưng không thay thế quy trình phát triển phần mềm.

Một hệ thống thật không thể được tạo bằng các đoạn code rời rạc vì nó còn phụ thuộc:

Architecture.

Database design.

State machine.

Permission.

Audit.

Idempotency.

Security.

Runtime config.

Error handling.

Integration boundary.

Evidence.

Smoke.

Release control.

## 4.3. Ví dụ bắt buộc: login không chỉ là login

Ngay cả chức năng đăng nhập cũng không thể copy một đoạn code là xong.

Đăng nhập thật cần:

User table.

Password hashing.

Session/token.

Refresh token.

Permission.

Role.

Account status.

Brute force protection.

SQL injection protection.

Forgot password.

Audit login.

Security review.

Session expiry.

Admin/user/supplier boundary nếu có.

Ginsengfood phức tạp hơn nhiều vì có sản xuất, kho, truy xuất, bán hàng, AI, quảng cáo, live, IVR và kế toán.

Vì vậy, copy-paste code là sai cách và sẽ làm hệ thống khó bảo trì.

## 5. ARCHITECTURE BOUNDARY TỔNG THỂ

## 5.1. Kiến trúc phải chia theo domain, không chia theo màn hình

Ginsengfood không được thiết kế theo kiểu “màn hình nào làm màn hình đó”.

Phải thiết kế theo domain:

Foundation.

Product Master.

Operational Core.

Demand / MRP / Procurement.

Commerce Runtime.

AI Advisor.

Facebook Gateway.

Ads Measurement.

MC AI Live.

IVR Order Confirmation.

Evidence / Gateway / Release.

UI chỉ là surface.
API chỉ là interface.
Database chỉ là persistence.
Domain rule mới là lõi.

## 5.2. Các domain không được thay thế nhau

Domain

Không được thay thế

Foundation

Không thay business domain

Product Master

Không thay inventory/sellable

Operational Core

Không thay Commerce quote/order

Commerce Runtime

Không thay Operational batch release

AI Advisor

Không thay Product, Price, Stock, Order, Payment

Facebook Gateway

Không thay AI Advisor hoặc Commerce

Ads Measurement

Không thay Payment, MISA, Verified Revenue

MC AI Live

Không thay AI Advisor, Facebook Gateway, Commerce

## IVR

Không thay Order State Machine

Evidence / Gateway

Không thay pack nghiệp vụ

Nguyên tắc:

Module nào làm đúng việc của module đó.
Không module nào được tự mở rộng quyền để “làm cho nhanh”.

## 6. TECHNICAL LAYER MODEL

## 6.1. Các lớp kỹ thuật chuẩn

Hệ thống kỹ thuật Ginsengfood phải được tư duy theo các lớp:

Domain Layer
Chứa rule nghiệp vụ, state machine, guard, invariant.

Application Layer
Điều phối use case, command, query, transaction boundary.

Integration Layer
Kết nối hệ thống ngoài như MISA, payment gateway, Facebook, IVR/SIM Gateway.

Runtime / Resolver Layer
Cung cấp giá, tồn kho, chương trình, quyền lợi, product knowledge, claim, policy.

AI / Channel Orchestration Layer
AI Advisor, Facebook Gateway, MC AI Live.

Evidence / Audit / Release Layer
Evidence, smoke, audit, P0 điểm chặn, release status.

Presentation Layer
Admin Web, PWA, Public Trace, Supplier Portal, Landing Page nếu có.

## 6.2. Domain Layer là trung tâm

Các rule quan trọng không được đặt trong UI hoặc AI prompt.

Ví dụ:

SKU sellable hay không không do UI quyết định.

Batch released hay chưa không do frontend quyết định.

Payment paid hay chưa không do AI quyết định.

Order canceled hay chưa không do IVR quyết định.

ROAS official hay không không do dashboard quyết định.

Claim được nói hay không không do nhân viên tùy ý quyết định.

Các rule này phải được đặt ở domain/application boundary phù hợp.

## 7. MODULE BOUNDARY CHUẨN

## 7.1. Foundation Module

Foundation cung cấp nền:

User.

Role.

Permission.

Auth.

Audit.

Idempotency.

Runtime config.

Evidence registry.

P0 điểm chặn registry.

Release status registry.

Foundation không được chứa business rule chi tiết của Product, Commerce, AI hoặc IVR.

## 7.2. Product Module

Product Module quản lý:

Product master.

## SKU.

Ingredient.

Recipe.

Formula version.

Product activation.

Product knowledge source.

Product scope registry.

Product Module không quyết định tồn kho thật nếu tồn kho thuộc Operational Core.
Product Module không tự quyết định quote/order nếu quote/order thuộc Commerce Runtime.

## 7.3. Operational Core Module

Operational Core quản lý:

Source origin.

Raw material intake.

## QC.

Raw lot readiness.

Production order.

Material issue/receipt.

Production process.

Packaging/QR.

Batch release.

Warehouse receipt.

Inventory ledger.

Traceability.

Recall.

Sale lock.

Operational Core là nguồn quan trọng cho sellable, stock, trace và recall.

## 7.4. Demand / MRP / Procurement Module

Module này quản lý:

Demand resolver.

Demand-to-production order.

## MRP.

Procurement suppression threshold.

Strategic reserve.

Company source harvest exception.

Material planning.

Packaging planning.

Stock alert.

Disposal / write-off control.

Module này không cho Sales/Đại lý tự phát lệnh sản xuất trực tiếp.

## 7.5. Commerce Runtime Module

Commerce Runtime quản lý:

Price runtime.

Program runtime.

Member benefit runtime.

Quote Snapshot.

Cart draft.

Official order.

Order State Machine.

Payment.

Direct bank transfer review.

Shipping.

VAT display.

Fulfillment boundary.

Verified revenue boundary.

Commerce Runtime là upstream bắt buộc của AI quote, Facebook order routing, Ads revenue, MC Live price cue và IVR order confirmation.

## 7.6. AI Advisor Module

AI Advisor quản lý:

Intent.

Entity.

Variable.

Resolver.

Guard.

Action.

Render.

Product consultation.

Claim guard.

Health guard.

OOS guard.

Customer memory boundary.

AI Advisor không được tự tạo source-of-truth cho giá, tồn kho, payment, order hoặc claim.

## 7.7. Facebook Gateway Module

Facebook Gateway quản lý:

Page config.

Comment event.

Messenger event.

Public/private boundary.

Private handoff.

Privacy guard.

Complaint routing.

Health-sensitive routing.

AI Advisor routing.

Commerce routing.

Facebook Gateway không được tự tạo order.

## 7.8. Ads Measurement Module

Ads Measurement quản lý:

Ads event.

Funnel event.

Attribution.

Verified revenue link.

Data quality.

## ROAS.

## CPA.

## AOV.

Scale gate.

Stop gate.

Dashboard display.

Ads Measurement không được xem WAITING revenue là revenue thật.

## 7.9. MC AI Live Module

MC AI Live quản lý:

Live plan.

Product scope.

Run-of-show.

Script registry.

Host cue.

Live Q&A assist.

Claim guard.

Privacy guard.

Private handoff.

Risk P0 control.

MC AI Live không được chốt đơn public, không tự tính giá, không fake urgency/scarcity.

## 7.10. IVR Module

IVR quản lý:

Internal SIM Gateway Server.

SIM capacity.

Deadline-aware rolling queue.

Program window scheduler.

Call attempt policy.

DTMF result.

Call result classification.

Technical exception.

Capacity incident.

Order signal input.

Notification boundary.

IVR result chỉ là input signal.
Core Order State Machine là lớp quyết định cuối.

## 7.11. Evidence / Release Module

Evidence / Release Module quản lý:

Evidence registry.

Smoke result.

P0 điểm chặn.

Dependency status.

Owner sign-off.

Release decision.

Go-live decision.

Release hold.

Rollback required.

Module này không thay rule nghiệp vụ của các module khác.

## 8. GREENFIELD REPO MAPPING PRINCIPLE

## 8.1. Repo mapping phải đi sau module boundary

Không được mở repo rồi sửa ngay.

Trình tự đúng:

Khóa module boundary trong TECH-00.

Viết TECH-01 đến TECH-10.

Dev lập repo/app map.

Dev map module mới vào repo thật.

Dev lập current implementation gap report.

Dev đề xuất phần viết mới, phần bỏ, phần dùng lại.

Owner review.

Sau đó mới code sâu.

## 8.2. Code cũ chỉ được phân loại sau TECH docs

Khi rà code cũ, chỉ có các trạng thái:

Trạng thái

Ý nghĩa

## REUSE_AS_IS

Có thể dùng lại nguyên trạng, không trái rule

## REUSE_WITH_REFACTOR

Có thể dùng lại nhưng phải chỉnh

## LEGACY_CONFLICT

Mâu thuẫn với tài liệu mới

## REWRITE_REQUIRED

Phải viết lại

## DEPRECATE

Không dùng nữa

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## SECURITY_REVIEW_REQUIRED

Cần rà bảo mật

## UNKNOWN

Chưa đủ thông tin

Không được giữ code cũ chỉ vì “đang chạy được”.

## 9. TECH DOCS SEQUENCE

Bộ tài liệu kỹ thuật mới sẽ đi theo thứ tự:

Tài liệu

Vai trò

## TECH-00

Technical Implementation Master Plan

## TECH-01

Foundation / RBAC / Audit / Idempotency / Evidence Registry

## TECH-02

Product / SKU / Ingredient / Recipe / Formula / Product Activation

## TECH-03

Operational Core / Production / Warehouse / Inventory / Traceability / Recall / Sale Lock

## TECH-04

Commerce Runtime / Quote / Cart / Order / Payment / Shipping / Fulfillment

## TECH-05

AI Advisor Runtime / Resolver / Guard / Action / Render

## TECH-06

Facebook Gateway / Meta Live / Messenger / Private Handoff

## TECH-07

Ads Measurement / Attribution / Verified Revenue / ROAS

## TECH-08

MC AI Live / Live Script / Host Cue / Risk Control

## TECH-09

IVR / Internal SIM Gateway / Scheduler / Call Result

## TECH-10

Evidence / Smoke / Gateway / Release Handoff

Không được viết TECH-05 AI trước TECH-04 Commerce nếu chưa khóa dependency.
Không được viết TECH-09 IVR như module độc lập tách khỏi Order State Machine.
Không được viết TECH-07 Ads như module tự tính doanh thu.
Không được viết TECH-08 Live như module chốt đơn.

## 10. START IMPLEMENTATION GATE

## 10.1. Điều kiện bắt đầu triển khai kỹ thuật

Dev chỉ được bắt đầu triển khai khi có:

TECH-00 đã khóa.

Module boundary rõ.

Source-of-truth rõ.

Pack-to-module map rõ.

Repo/app mapping plan rõ.

Dependency map rõ.

P0 điểm chặn baseline rõ.

Evidence plan rõ.

Smoke plan rõ.

Owner cho phép bắt đầu theo wave.

Nếu chưa đủ:

## 10.2. Điều kiện bắt đầu từng module

Một module chỉ được triển khai khi có:

Module purpose.

Source pack.

Upstream dependency.

Downstream dependency.

State/rule boundary.

Permission/audit requirement.

Idempotency requirement nếu có.

Evidence requirement.

Smoke requirement.

Fail gate.

Nếu thiếu:

## 11. TECH-00 PART 1 DONE GATE

## PHẦN 1/4 của TECH-00 được xem là hoàn tất khi đã khóa:

Greenfield technical reset.

Source-of-truth hierarchy.

Legacy document handling.

Legacy code handling.

No-code-yet rule.

No copy-paste rule.

Architecture boundary.

Technical layer model.

Module boundary tổng thể.

Repo mapping principle.

TECH docs sequence.

Start implementation gate.

Module start gate.

## 12. TECH-00 PART 1 FAIL GATE

## PHẦN 1/4 fail nếu sau tài liệu này dev vẫn:

Quay lại tài liệu cũ làm nền.

Dùng code cũ làm business truth.

Bắt đầu code khi chưa mapping.

Copy-paste code AI.

Làm UI trước domain.

Làm AI trước Commerce Runtime.

Làm Ads trước Verified Revenue.

Làm IVR trước Order State Machine.

Làm Facebook Gateway tự tạo order.

Gộp nhiều domain thành một module mơ hồ.

Không có evidence plan.

Không có smoke plan.

Không có owner review cho conflict.

Nếu xảy ra:

## 13. KẾT LUẬN PHẦN 1/4

## PHẦN 1/4 của TECH-00 đã khóa hướng triển khai kỹ thuật mới hoàn toàn cho Ginsengfood:

Làm mới theo greenfield.

Không dùng tài liệu cũ làm source-of-truth.

Không dùng code cũ làm business truth.

Không code sâu trước khi mapping.

Không copy-paste code rời rạc.

Kiến trúc phải chia theo domain.

Module phải có boundary rõ.

Repo mapping phải đi sau TECH docs.

TECH-01 đến TECH-10 sẽ được viết theo đúng thứ tự dependency.

Dev chỉ được bắt đầu triển khai sau khi có module map, dependency map, evidence plan, smoke plan và owner approval theo wave.

Trạng thái sau PHẦN 1/4:

## PHẦN 2/4 - REPOSITORY / APPLICATION / MODULE BOUNDARY MAPPING

## 14. MỤC ĐÍCH CỦA PHẦN 2/4

## PHẦN 2/4 dùng để khóa cách chia repository / application / technical module boundary cho hệ thống Ginsengfood theo hướng greenfield.

Mục tiêu không phải viết code.
Mục tiêu không phải thiết kế API chi tiết.
Mục tiêu không phải thiết kế database chi tiết.
Mục tiêu không phải thiết kế UI chi tiết.

Mục tiêu của PHẦN 2/4 là:

Chỉ rõ hệ thống Ginsengfood nên được chia thành những nhóm ứng dụng nào.

Chỉ rõ mỗi nhóm ứng dụng chịu trách nhiệm gì.

Chỉ rõ module nào thuộc ứng dụng nào.

Chỉ rõ module nào không được lấn sang module khác.

Chỉ rõ repository cũ nếu có chỉ là current implementation state.

Chỉ rõ repo mapping phải đi sau module boundary.

Chỉ rõ dev phải lập repo map, app map, module map trước khi code.

Chỉ rõ cách phân loại phần nào viết mới, phần nào có thể tái sử dụng, phần nào phải bỏ.

Ngăn việc gom tất cả logic vào một backend hoặc một service mơ hồ.

Ngăn việc để AI / Facebook / Ads / IVR / Live vượt Commerce Runtime hoặc Operational Core.

## 15. GREENFIELD REPOSITORY PRINCIPLE

## 15.1. Repository không quyết định nghiệp vụ

Repository là nơi tổ chức code.
Repository không phải source-of-truth nghiệp vụ.

Không được nhìn repo cũ rồi quyết định lại nghiệp vụ.

Nếu repo cũ có cấu trúc khác tài liệu đã khóa, mặc định:

## SOURCE-OF-TRUTH MỚI THẮNG

Repo cũ chỉ được dùng để trả lời:

Hiện đang có gì.

Có phần nào dùng lại được không.

Có phần nào mâu thuẫn với tài liệu mới không.

Có phần nào phải refactor.

Có phần nào phải viết lại.

Có phần nào phải bỏ.

Repo cũ không được dùng để ép tài liệu mới quay lại tư duy cũ.

## 15.2. Repo mapping phải làm sau module boundary

Thứ tự bắt buộc:

Khóa MASTER / PACK.

Khóa DEV EXECUTION COMMAND PACK.

Khóa TECH-00.

Viết TECH-01 đến TECH-10.

Sau đó dev mới lập repo mapping.

Sau repo mapping mới lập gap report.

Sau gap report mới quyết định dùng lại, refactor hay viết mới.

Sau owner review mới code sâu.

Không làm ngược lại.

Không được mở repo cũ rồi sửa ngay, vì cách này dễ kéo dev quay lại logic cũ.

## 15.3. Greenfield không có nghĩa bỏ hết code bằng mọi giá

Greenfield ở đây nghĩa là:

Tư duy mới, tài liệu mới, source-of-truth mới, boundary mới.

Không có nghĩa là bắt buộc xóa toàn bộ code cũ.

Code cũ có thể được dùng lại nếu đạt đủ điều kiện:

Không trái tài liệu mới.

Không trái pack boundary.

Không hardcode runtime.

Không bypass source-of-truth.

Không vi phạm security.

Không vi phạm state machine.

Có thể test/smoke/evidence.

Owner hoặc technical lead chấp nhận sau gap report.

Nếu không đạt, phải refactor, rewrite hoặc deprecate.

## 16. APPLICATION BOUNDARY TỔNG THỂ

Hệ thống Ginsengfood greenfield nên được tư duy theo các nhóm ứng dụng sau.

## 16.1. Application Group tổng thể

Application Group

Vai trò

Không được làm

Admin Web

Quản trị nội bộ, master data, vận hành, release, evidence

Không tự quyết rule nghiệp vụ

Shopfloor / PWA

Thao tác sản xuất, scan, kho, vận hành hiện trường

Không bypass domain state

Public Trace

Truy xuất công khai cho người dùng

Không lộ dữ liệu nội bộ

Supplier Portal

Nhà cung cấp gửi/nhận thông tin trong scope được phép

Không truy cập admin/internal

Backend Core

Domain nghiệp vụ chính: product, operations, commerce, evidence

Không gom logic channel/AI bừa bãi nếu cần tách

Integration Worker

MISA, payment, notification, outbox, background sync

Không tự quyết business state ngoài core

AI Advisor Runtime

Tư vấn, resolver, guard, render

Không tự tạo order/payment/source-of-truth

Facebook Channel Gateway

Facebook comment/Messenger/private handoff

Không thay AI Advisor hoặc Commerce

Ads Measurement Service

Attribution, verified revenue, ROAS, scale/stop gate

Không tạo revenue

MC AI Live Service

Live plan, script, cue, risk control

Không chốt đơn public

IVR / SIM Gateway Service

Gọi xác nhận đơn bằng SIM nội bộ

Không hủy đơn trực tiếp

Evidence / Release Control

Evidence, smoke, P0, release status, owner decision

Không thay rule nghiệp vụ

## 16.2. Không bắt buộc tách repo ngay từ đầu

Tách application boundary không đồng nghĩa phải tách nhiều repo ngay.

Dev có thể triển khai theo một trong ba mô hình, nhưng phải giữ boundary rõ:

Mô hình

Khi dùng

Điều kiện

Monorepo

Phù hợp giai đoạn đầu, dễ quản lý nhiều app

Module boundary phải rõ

Multi-repo

Khi team lớn, service tách biệt rõ

Cần governance version/dependency tốt

Hybrid

Core một repo, channel/integration tách repo

Phải có contract rõ giữa các phần

Điểm bắt buộc:

Dù monorepo hay multi-repo, domain boundary không được bị phá.

## 17. REPOSITORY TARGET MAP

## 17.1. Repo mục tiêu theo tư duy greenfield

Bảng này là định hướng kỹ thuật, không phải bắt buộc tên repo chính xác. Dev có thể đề xuất tên repo khác, nhưng boundary không được đổi.

Repo / Workspace đề xuất

Nội dung

Pack liên quan

ginsengfood-core

Backend core, domain, application, evidence, release

## PACK-01 -> PACK-04, PACK-10

ginsengfood-admin-web

Admin Web cho vận hành/quản trị

UI Pack, PACK-01 -> PACK-10

ginsengfood-shopfloor-pwa

PWA hiện trường sản xuất/kho/scan

## PACK-01, PACK-03

ginsengfood-public-trace

Public trace surface

PACK-01, compliance/public trace

ginsengfood-supplier-portal

Supplier collaboration nếu scope bật

Business/Functional/UI Pack

ginsengfood-ai-advisor

AI Advisor runtime/resolver/guard

## PACK-06

ginsengfood-channel-gateway

Facebook/Messenger Gateway

## PACK-05

ginsengfood-ads-measurement

Ads/ROAS/Attribution

## PACK-07

ginsengfood-mc-ai-live

MC AI Live orchestration

## PACK-08

ginsengfood-ivr-gateway

Internal SIM Gateway / IVR

## PACK-09

ginsengfood-integration-worker

MISA, payment, notification, outbox jobs

PACK-04, MASTER payment/MISA

ginsengfood-docs-evidence

Evidence, smoke, release handoff nếu lưu file dạng docs

## PACK-10

## 17.2. Repo mapping thực tế phải do dev nộp

Dev phải nộp bảng:

Cột

Nội dung

Existing repo/app

Repo/app hiện có nếu có

Proposed target

Repo/app mục tiêu

Module group

Nhóm module thuộc repo/app

Source pack

PACK/Master liên quan

Keep / Refactor / Rewrite / Deprecate

Phân loại

Risk

Rủi ro nếu dùng lại

Evidence required

Evidence cần nộp

Owner review

Có cần owner review không

Nếu chưa có repo cũ, bảng này dùng để tạo repo mới.
Nếu đã có repo cũ, bảng này dùng để phân loại legacy.

## 18. APPLICATION SURFACE MAP

## 18.1. Admin Web

Admin Web là bề mặt quản trị nội bộ.

Admin Web dùng cho:

Master data.

Product/SKU/ingredient/recipe.

Source origin.

Raw intake.

## QC.

Production order.

Material issue/receipt.

Packaging/QR.

Batch release.

Warehouse/inventory.

Traceability.

Recall/sale lock.

Order/commerce admin.

Payment review.

Member/program/pricing governance nếu scope có.

Evidence/release dashboard.

P0 điểm chặn registry.

User/role/permission.

Admin Web không được:

Tự quyết rule nghiệp vụ ở frontend.

Tự bypass backend state.

Sửa dữ liệu lịch sử để hợp thức hóa.

Mark PAID nếu không có source hợp lệ.

Mở batch chưa RELEASED.

Gỡ sale lock/recall không audit.

Dùng dashboard làm source-of-truth.

## 18.2. Shopfloor / PWA

Shopfloor / PWA dùng cho thao tác hiện trường:

Scan.

Raw material receipt.

QC evidence capture.

Material issue/receipt.

Production step confirmation.

Packaging confirmation.

Warehouse receipt.

Inventory movement.

Trace event capture.

Offline queue nếu scope cần.

Shopfloor / PWA không được:

Tự tạo state ngoài domain rule.

Bỏ qua scan gate ở thao tác rủi ro.

Submit lặp gây trùng dữ liệu.

Ghi đè lịch sử.

Bypass audit.

Bypass permission.

Cho thao tác production khi module upstream chưa clear.

## 18.3. Public Trace

Public Trace dùng cho người dùng/đối tác xem thông tin công khai được phép.

Public Trace chỉ được hiển thị whitelist.

Không được hiển thị:

Supplier detail nội bộ.

Nhân sự/operator/approver.

Costing/accounting/MISA.

QC defect/loss/waste.

Internal trace graph IDs.

Internal root cause.

Evidence nội bộ.

Formula chi tiết.

Customer/order/payment data.

Dữ liệu chưa được public approved.

Public Trace phụ thuộc Operational Core và Traceability.
Nếu Traceability chưa clear, Public Trace phải bị chặn.

## 18.4. Supplier Portal

Supplier Portal chỉ dùng khi scope có supplier collaboration.

Supplier Portal dùng cho:

Supplier login.

Supplier pre-submit intake.

Supplier evidence upload.

Supplier confirmation/decline.

Supplier view scope được phép.

Supplier-specific workflow.

Supplier Portal không được:

Truy cập admin/internal.

Xem supplier khác.

Xem costing/accounting.

Bypass admin receive.

Bypass QC.

Bypass evidence scan.

Tạo raw lot final trực tiếp nếu rule yêu cầu admin receive/QC.

## 18.5. AI Advisor Runtime

AI Advisor Runtime dùng cho:

Intent/entity recognition.

Variable resolution.

Product knowledge resolver.

Price/program/member resolver.

Stock/sellable resolver.

Claim guard.

Health guard.

OOS guard.

Customer memory boundary.

Quote/order draft action qua Commerce.

Render response.

AI Advisor không được:

Hardcode giá.

Hardcode tồn kho.

Hardcode tài khoản thanh toán.

Hardcode quyền lợi thành viên.

Tự tạo official order.

Tự xác nhận payment.

Tự gửi tin hủy đơn.

Nói claim vượt whitelist.

Bán SKU hết hàng/sale lock/recall.

Dùng code prompt thay domain source-of-truth.

## 18.6. Facebook Channel Gateway

Facebook Gateway dùng cho:

Nhận comment.

Nhận Messenger event.

Phân loại public/private.

Kéo private đúng boundary.

Privacy guard.

Health-sensitive routing.

Complaint routing.

Gọi AI Advisor đúng scope.

Gọi Commerce đúng scope.

Ghi event/evidence.

Facebook Gateway không được:

Tự tạo order.

Tự xác nhận payment.

Tự báo giá hardcode.

Lộ thông tin cá nhân public.

Biến complaint thành lead bán hàng.

Tư vấn sức khỏe sâu public.

Spam/vi phạm policy.

Vượt AI Advisor hoặc Commerce.

## 18.7. Ads Measurement Service

Ads Measurement dùng cho:

Ads event capture.

Funnel event.

Attribution.

Data quality.

Verified revenue link.

## ROAS.

## CPA.

## AOV.

Scale gate.

Stop gate.

Measurement dashboard.

Ads Measurement không được:

Tạo doanh thu.

Dùng WAITING revenue làm revenue thật.

Xem comment/inbox là order.

Xem quote/cart draft là revenue.

Xem order unpaid là verified revenue.

Dùng dashboard làm source-of-truth.

Scale khi Data Quality chưa PASS.

Scale khi Stock/Sellable/Fulfillment chưa clear.

Scale khi owner chưa quyết định.

## 18.8. MC AI Live Service

MC AI Live dùng cho:

Live plan.

Product scope.

Run-of-show.

Script registry.

Host cue.

Live Q&A assist.

Claim guard.

Privacy guard.

Private handoff.

Risk P0 control.

Live evidence.

MC AI Live không được:

Tự tính giá.

Tự tạo quote.

Tự tạo order.

Xác nhận payment.

Chốt đơn public.

Fake urgency.

Fake scarcity.

Cue sản phẩm not sellable.

Bỏ qua complaint.

Bỏ qua health-sensitive.

Bỏ qua sale lock/recall.

## 18.9. IVR / SIM Gateway Service

IVR / SIM Gateway dùng cho:

Internal SIM Gateway.

Outbound call confirmation.

SIM capacity control.

Deadline-aware rolling queue.

Program window attempt.

DTMF result.

Call result classification.

Technical exception.

Capacity incident.

Order signal input.

IVR evidence.

IVR không được:

Làm sales.

Làm CRM.

Làm marketing.

Làm chăm sóc khách hàng.

Làm payment.

Làm delivery.

Làm warehouse.

Làm MISA.

Tự hủy đơn.

Cập nhật order trực tiếp.

Gửi tin hủy đơn.

Nhầm invalid phone với no answer.

Nhầm technical error với khách không nghe.

Nhầm capacity overload với lỗi khách.

## 18.10. Integration Worker

Integration Worker dùng cho:

Payment confirmation sync.

Bank transfer review support nếu có.

MISA sync.

Accounting document handoff.

Notification dispatch theo owner decision.

Outbox/retry.

Background jobs.

Reconciliation.

Integration Worker không được:

Tự quyết business state nếu core chưa xác nhận.

Mark PAID khi chưa có nguồn hợp lệ.

Gửi thông báo giao dịch trái owner.

Gửi marketing nếu scope là transaction notice.

Làm mất idempotency.

Bỏ qua audit.

Sync MISA như source-of-truth nếu chưa pass.

## 19. MODULE GROUP -> APPLICATION MAP

## 19.1. Foundation Module Map

Module

Application chính

Application phụ thuộc

Auth/User

Backend Core

Admin Web

Role/Permission

Backend Core

Admin Web

Audit

Backend Core

Tất cả app

Idempotency

Backend Core

PWA, Gateway, Worker

Runtime Config

Backend Core

AI, Commerce, Ads, Live

Evidence Registry

Backend Core / Evidence Control

Admin Web

P0 điểm chặn Registry

Backend Core / Evidence Control

Admin Web

Release Status Registry

Backend Core / Evidence Control

Admin Web

## 19.2. Product / SKU Module Map

Module

Application chính

Application phụ thuộc

Product Master

Backend Core

Admin Web

SKU Master

Backend Core

Admin Web

Ingredient Master

Backend Core

Admin Web

Recipe

Backend Core

Admin Web

Formula Version

Backend Core

Admin Web

Product Activation

Backend Core

Commerce, AI, Live, Ads

Product Knowledge Runtime

Backend Core / AI Runtime

AI Advisor

Product Scope Registry

Backend Core

MC AI Live, Ads

## 19.3. Operational Core Module Map

Module

Application chính

Application phụ thuộc

Source Origin

Backend Core

Admin Web

Raw Intake

Backend Core

Admin Web, PWA

Incoming QC

Backend Core

Admin Web, PWA

Raw Lot Readiness

Backend Core

Admin Web, PWA

Production Order

Backend Core

Admin Web

Material Issue

Backend Core

Admin Web, PWA

Material Receipt

Backend Core

Admin Web, PWA

Production Process

Backend Core

## PWA

Packaging / QR

Backend Core

Admin Web, PWA

Batch QC

Backend Core

Admin Web, PWA

Batch Release

Backend Core

Admin Web

Warehouse Receipt

Backend Core

Admin Web, PWA

Inventory Ledger

Backend Core

Admin Web

Traceability

Backend Core

Admin Web, Public Trace

Recall

Backend Core

Admin Web, Commerce, AI

Sale Lock

Backend Core

Commerce, AI, Ads, Live

## 19.4. Commerce Runtime Module Map

Module

Application chính

Application phụ thuộc

Price Runtime

Backend Core / Commerce

AI, Facebook, Live

Program Runtime

Backend Core / Commerce

AI, Facebook, Live

Member Benefit Runtime

Backend Core / Commerce

AI, CRM nếu có

Quote Snapshot

Backend Core / Commerce

AI, Facebook

Cart Draft

Backend Core / Commerce

AI, Facebook

Official Order

Backend Core / Commerce

Admin Web, Integration Worker

Order State Machine

Backend Core / Commerce

IVR, Notification, Payment

Payment

Backend Core / Commerce

Integration Worker

Direct Bank Transfer Review

Backend Core / Commerce

Admin Web

Shipping

Backend Core / Commerce

Admin Web, AI

Fulfillment Boundary

Backend Core / Commerce

Warehouse, Integration Worker

Verified Revenue Boundary

Backend Core / Commerce

Ads, MISA, Accounting

## 19.5. AI / Channel / Ads / Live / IVR Module Map

Module

Application chính

Upstream bắt buộc

AI Advisor

AI Runtime

Product, Commerce, Sellable, Claim Guard

Facebook Gateway

Channel Gateway

AI, Commerce, Privacy Guard

Ads Measurement

Ads Service

Commerce, Payment, Verified Revenue

MC AI Live

Live Service

Product Scope, AI, Gateway, Commerce, Ads

## IVR

IVR Gateway

Order State Machine, Notification, Security

Notification Dispatch

Integration Worker

Core decision, owner template

MISA Sync

Integration Worker

Accounting boundary, verified transaction

Payment Sync

Integration Worker

Payment provider/bank/accounting review

## 20. CROSS-APPLICATION DEPENDENCY MAP

## 20.1. Dependency quan trọng

Downstream App

Upstream bắt buộc

Lý do

AI Advisor

Product, Commerce, Operational/Sellable

AI không được hardcode sản phẩm/giá/tồn

Facebook Gateway

AI Advisor, Commerce, Privacy Guard

Gateway không tự tư vấn/chốt order

Ads Measurement

Commerce, Payment, Verified Revenue

Ads không tạo revenue

MC AI Live

Product Scope, AI, Gateway, Commerce

Live không chốt đơn public

IVR Gateway

Order State Machine, Notification Owner

IVR không tự hủy đơn

Public Trace

Traceability, Public Whitelist

Không lộ dữ liệu nội bộ

MISA Worker

Commerce/Accounting boundary

Không sync sai doanh thu

Payment Worker

Payment boundary

Không mark PAID sai

Admin Web

Backend Core

UI không tự quyết rule

## PWA

Backend Core

PWA không bypass domain state

## 20.2. Dependency status bắt buộc

Mỗi dependency phải có trạng thái:

Status

Ý nghĩa

## NOT_STARTED

Chưa bắt đầu

## IN_PROGRESS

Đang triển khai

## IMPLEMENTED_WAITING_TEST

Có triển khai, chưa test đủ

## TEST_WAITING

Chưa có test accepted

## SMOKE_WAITING

Chưa có smoke accepted

## EVIDENCE_WAITING

Chưa có evidence accepted

bị chặn

Bị chặn

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## RELEASE_READY

Đủ điều kiện trình release

## RELEASE_DECISION_ACCEPTED

Đã duyệt release

## EXCLUDED_FROM_SCOPE

Không nằm trong scope hiện tại

Không được dùng “gần xong”, “cơ bản xong”, “chạy được”.

## 21. REPO / APP / MODULE MAPPING TEMPLATE

## 21.1. Template bắt buộc dev phải nộp

Dev phải nộp bảng sau trước khi code sâu:

Cột

Nội dung

Module ID

Mã module

Module Name

Tên module

Application Group

App group chính

Proposed Repo

Repo/workspace đề xuất

Source Pack

PACK/Master liên quan

Upstream Dependency

Phụ thuộc đầu vào

Downstream Dependency

Module/app phụ thuộc nó

Current Legacy State

Nếu có code cũ: trạng thái hiện tại

Target State

Trạng thái mục tiêu

Reuse Decision

Reuse / Refactor / Rewrite / Deprecate

P0 Risk

Rủi ro P0

Evidence Required

Evidence cần nộp

Smoke Required

Smoke cần chạy

Owner Review

Có cần owner review không

Start Gate

ALLOWED / bị chặn

Release Gate

bị chặn / WAITING / READY

## 21.2. Không có mapping thì không được code

Nếu module chưa có mapping:

Nếu repo chưa có mapping:

Nếu application boundary chưa rõ:

## 22. LEGACY REPO CLASSIFICATION

## 22.1. Trạng thái phân loại legacy

Nếu có repo/code cũ, dev phải phân loại từng phần theo bảng:

Trạng thái

Ý nghĩa

Hành động

## REUSE_AS_IS

Dùng lại được nguyên trạng

Vẫn cần test/smoke/evidence

## REUSE_WITH_REFACTOR

Dùng lại một phần

Refactor theo TECH docs

## REWRITE_REQUIRED

Không phù hợp, phải viết lại

Lập task rewrite

## DEPRECATE

Không dùng nữa

Khóa/remove theo plan

## LEGACY_CONFLICT

Trái source-of-truth mới

Owner review hoặc rewrite

## SECURITY_REVIEW_REQUIRED

Có rủi ro bảo mật

Security review trước khi dùng

## UNKNOWN

Chưa rõ

Không được production

## OWNER_REVIEW_REQUIRED

Cần quyết định owner

Chờ owner

## 22.2. Điều kiện REUSE_AS_IS

Một phần code cũ chỉ được REUSE_AS_IS khi:

Đúng source-of-truth mới.

Không vi phạm pack boundary.

Không hardcode dữ liệu runtime.

Không bypass permission/audit.

Không bypass state machine.

Không có security risk.

Có test pass.

Có smoke pass nếu chạm luồng P0.

Có evidence accepted.

Owner/tech lead chấp nhận.

Nếu thiếu một điều kiện:

## 22.3. Điều kiện REWRITE_REQUIRED

Phải rewrite nếu code cũ:

Dùng logic nghiệp vụ cũ.

Hardcode giá/tồn/quyền lợi/tài khoản/chương trình.

Để AI tự tạo order/payment.

Để Gateway tự tạo order.

Để Ads tự tính revenue.

Để IVR tự hủy đơn.

Không có audit.

Không có permission.

Không có idempotency ở command rủi ro.

Không tách state machine.

Không có evidence/smoke khả dụng.

Gây conflict với MASTER/PACK.

## 23. ENVIRONMENT BOUNDARY MAP

## 23.1. Môi trường triển khai

Hệ thống phải phân biệt tối thiểu:

Environment

Vai trò

## LOCAL

Dev cá nhân

## DEV

Tích hợp nội bộ

## STAGING

Test gần production, smoke, UAT

## PILOT

Production giới hạn nếu owner duyệt

## PROD

Production thật

Nếu chưa có đủ môi trường, phải ghi rõ:

## ENVIRONMENT_SCOPE_LIMITED

Không được dùng dữ liệu thật trên môi trường chưa đạt security/data protection.

## 23.2. Quy tắc môi trường

LOCAL không được dùng dữ liệu khách thật.

DEV không được gửi tin thật cho khách nếu chưa có owner decision.

STAGING phải có dữ liệu test/simulation rõ.

PILOT phải có release scope và owner approval.

PROD chỉ dùng khi Go-live Approved.

IVR không được gọi khách thật ngoài PILOT/PROD đã duyệt.

Facebook Gateway không được tự trả lời public thật nếu chưa release.

Ads không được scale theo số liệu staging.

Payment thật phải có boundary riêng.

Public Trace thật chỉ mở sau public whitelist smoke.

## 24. DATA BOUNDARY MAP

## 24.1. Nhóm dữ liệu chính

Data Group

Source chính

Không được làm

Product Data

Product Module

Không để AI tự tạo

Recipe/Formula

Product/Recipe Module

Không ghi đè lịch sử

Raw Lot

Operational Core

Không tạo trước receive/QC nếu rule không cho

Batch

Operational Core

Không sellable nếu chưa RELEASED

Inventory

Operational Core

Không sửa ledger lịch sử

Quote

Commerce Runtime

Không tạo ngoài Quote Snapshot

Order

Commerce Runtime

Không tạo ngoài Order State Machine

Payment

Commerce/Payment Boundary

Không mark PAID sai

Revenue

Verified Revenue Boundary

Không dùng WAITING revenue

Customer Data

Commerce/CRM boundary nếu có

Không public leak

AI Conversation

AI Advisor / Channel

Không dùng thay order source

Ads Event

Ads Measurement

Không dùng thay revenue source

IVR Call Result

IVR Gateway

Chỉ input signal

Evidence

Evidence Registry

Không dùng file rời rạc không review

Release Status

Release Registry

Không dashboard tự quyết

## 24.2. Data boundary phải thắng UI convenience

Không vì UI tiện mà đưa rule nghiệp vụ vào frontend.

Ví dụ:

Frontend có thể disable nút, nhưng backend vẫn phải enforce.

UI có thể hiển thị “không sellable”, nhưng backend mới quyết chặn.

AI có thể nói “hết hàng”, nhưng stock/sellable phải từ runtime.

Dashboard có thể hiện ROAS, nhưng verified revenue phải từ source hợp lệ.

Admin có thể thấy IVR result, nhưng Core Order State Machine mới quyết định order.

## 25. INTEGRATION BOUNDARY MAP

## 25.1. Các integration chính

Integration

Vai trò

Boundary

Payment Gateway

Xác nhận thanh toán điện tử

Không mark PAID nếu chưa xác nhận

Bank Transfer Review

Đối soát chuyển khoản

Không hardcode tài khoản ở AI/Gateway/UI static

## MISA

Kế toán/đối soát

Không thay core domain

Facebook/Meta

Comment/Messenger/Live

Không tạo order trực tiếp

Ads Platform

Event/measurement

Không tạo verified revenue

SIM Gateway

Gọi IVR

Không cập nhật order trực tiếp

Notification Channel

Gửi tin giao dịch

Chỉ gửi theo owner/core decision

Public Trace QR

Truy xuất công khai

Whitelist-only

## 25.2. Integration phải đi qua boundary layer

Không module nào được gọi integration bừa bãi.

Ví dụ:

AI không gọi payment trực tiếp.

Facebook Gateway không gọi MISA.

Ads không gọi order để sửa trạng thái.

MC AI Live không gọi payment.

IVR không gọi update order trực tiếp.

Admin Web không gọi external service để bypass core.

PWA không sync MISA trực tiếp.

Public Trace không đọc internal trace graph trực tiếp.

## 26. APPLICATION RELEASE BOUNDARY

## 26.1. Không release toàn bộ app nếu chỉ một scope pass

Một application có thể có nhiều scope.

Ví dụ Admin Web có thể pass Product Master nhưng chưa pass Payment Review.
AI Advisor có thể pass Product Advice nhưng chưa pass Quote Draft.
Facebook Gateway có thể pass private handoff nhưng chưa pass Commerce routing.
Ads có thể pass dashboard display nhưng chưa pass official ROAS.
IVR có thể pass simulation nhưng chưa pass customer call.

Phải release theo scope, không release mơ hồ.

## 26.2. Release status theo application

Application

Release status phải ghi rõ

Admin Web

Module nào pass, module nào bị chặn

## PWA

Flow nào được dùng, flow nào chưa

Public Trace

Public whitelist pass hay chưa

Supplier Portal

Supplier scope nào được bật

Backend Core

Domain nào release approved

AI Advisor

Intent/action nào được production

Facebook Gateway

Page/kênh/route nào được bật

Ads Measurement

Measurement hay scale

MC AI Live

Rehearsal hay production live

## IVR

Simulation hay gọi khách thật

Integration Worker

Payment/MISA/notification scope nào pass

## 27. REPOSITORY / APPLICATION P0 RULES

Các lỗi sau là P0 ở tầng repo/application boundary:

Một app tự quyết rule thuộc app khác.

UI bypass backend domain.

AI hardcode source-of-truth.

Gateway tự tạo order.

Ads tự tạo revenue.

IVR cập nhật order trực tiếp.

Integration worker tự mark business state sai.

Public Trace đọc dữ liệu nội bộ trực tiếp.

Admin sửa lịch sử để hợp thức hóa.

Repo cũ được dùng làm business truth.

Không có app boundary nhưng vẫn code production.

Không có module map nhưng vẫn triển khai.

Không có environment boundary nhưng dùng dữ liệu thật.

Không có evidence plan nhưng báo done.

Không có owner review nhưng release.

Nếu có một lỗi:

## 28. DEV OUTPUT BẮT BUỘC SAU PHẦN 2/4

Sau PHẦN 2/4, dev phải nộp các tài liệu kỹ thuật sau trước khi code sâu.

## 28.1. Repository Map

Nội dung:

Repo hiện có.

Repo đề xuất.

Repo nào dùng lại.

Repo nào viết mới.

Repo nào bỏ.

Repo nào cần review.

Repo nào chứa module nào.

Repo nào phụ thuộc repo nào.

## 28.2. Application Boundary Map

Nội dung:

Application group.

Vai trò.

Module thuộc app.

Module không được nằm trong app.

Upstream.

Downstream.

Release scope.

Evidence scope.

## 28.3. Module-to-App Map

Nội dung:

Module.

App chính.

App phụ thuộc.

Source pack.

Dependency.

Evidence.

Smoke.

Gate.

## 28.4. Legacy Classification Report

Nếu có code cũ, phải phân loại:

## REUSE_AS_IS.

## REUSE_WITH_REFACTOR.

## REWRITE_REQUIRED.

## DEPRECATE.

## LEGACY_CONFLICT.

## SECURITY_REVIEW_REQUIRED.

## UNKNOWN.

## OWNER_REVIEW_REQUIRED.

## 28.5. Environment Boundary Report

Nội dung:

LOCAL/DEV/STAGING/PILOT/PROD có hay chưa.

Môi trường nào dùng dữ liệu gì.

Môi trường nào được gửi tin thật.

Môi trường nào được gọi IVR thật.

Môi trường nào được payment thật.

Môi trường nào được public trace thật.

## 28.6. Cross-Application Dependency Report

Nội dung:

App downstream.

App upstream.

Dependency status.

điểm chặn.

Release impact.

Owner decision required.

## 29. TECH-00 PART 2 DONE GATE

## PHẦN 2/4 được xem là hoàn tất khi đã khóa:

Greenfield repository principle.

Application boundary tổng thể.

Repository target map.

Application surface map.

Module group to application map.

Cross-application dependency map.

Repo/app/module mapping template.

Legacy repo classification.

Environment boundary.

Data boundary.

Integration boundary.

Application release boundary.

Repository/application P0 rules.

Dev output bắt buộc sau phần này.

## 30. TECH-00 PART 2 FAIL GATE

## PHẦN 2/4 fail nếu dev vẫn:

Dùng repo cũ làm source-of-truth.

Không lập repository map.

Không lập application boundary map.

Không phân loại legacy code.

Gom AI/Facebook/Ads/IVR/Commerce vào một khối mơ hồ.

Để UI tự quyết domain rule.

Để integration worker tự quyết business state.

Để Public Trace đọc dữ liệu nội bộ trực tiếp.

Để IVR update order trực tiếp.

Để Ads lấy revenue chưa verified.

Để AI hardcode giá/tồn/chính sách.

Không phân biệt environment.

Dùng dữ liệu thật ở môi trường chưa được duyệt.

Không có evidence/smoke cho app boundary.

Không có owner review cho conflict.

Nếu xảy ra:

## 31. KẾT LUẬN PHẦN 2/4

## PHẦN 2/4 của TECH-00 đã khóa cách tư duy repository, application và module boundary cho hệ thống Ginsengfood theo hướng greenfield.

Các kết luận bắt buộc:

Repository không quyết định nghiệp vụ.

Code cũ không phải business truth.

Repo mapping phải đi sau module boundary.

Không bắt buộc tách nhiều repo ngay, nhưng boundary phải rõ.

Admin Web, PWA, Public Trace, Supplier Portal, Backend Core, Integration Worker, AI Advisor, Facebook Gateway, Ads Measurement, MC AI Live, IVR và Evidence/Release Control phải có vai trò riêng.

Không app nào được thay thế app khác.

Không downstream app nào được vượt upstream app.

Không integration nào được bypass core domain.

Không UI nào được tự quyết rule nghiệp vụ.

Không Public Trace nào được lộ dữ liệu nội bộ.

Không AI / Facebook / Ads / MC Live / IVR nào được vượt Commerce Runtime hoặc Operational Core.

Dev phải nộp Repository Map, Application Boundary Map, Module-to-App Map, Legacy Classification Report, Environment Boundary Report và Cross-Application Dependency Report trước khi code sâu.

Trạng thái sau PHẦN 2/4:

## PHẦN 3/4 - TECHNICAL MODULE SPECIFICATION TEMPLATE / MODULE CONTRACT / IMPLEMENTATION READINESS STANDARD

## 32. MỤC ĐÍCH CỦA PHẦN 3/4

## PHẦN 3/4 khóa mẫu chuẩn để viết đặc tả kỹ thuật cho từng module trong hệ thống Ginsengfood.

Từ TECH-01 đến TECH-10, mọi module đều phải được mô tả theo một cấu trúc thống nhất. Không module nào được viết theo kiểu chung chung, thiếu source-of-truth, thiếu dependency, thiếu evidence, thiếu smoke hoặc thiếu Done Gate.

## PHẦN 3/4 dùng để trả lời:

Một module kỹ thuật phải được mô tả như thế nào?

Module đó thuộc pack nào?

Module đó có nhiệm vụ gì?

Module đó không được làm gì?

Module đó phụ thuộc upstream nào?

Module nào downstream phụ thuộc nó?

Module đó có state machine không?

Module đó có command/query boundary gì?

Module đó có permission/audit/idempotency gì?

Module đó cần evidence nào?

Module đó cần smoke nào?

Khi nào module được coi là Done?

Khi nào module phải bị Fail/bị chặn?

Khi nào module được Release Ready?

Khi nào module được Production Ready?

## PHẦN 3/4 không viết code.
PHẦN 3/4 không thiết kế API chi tiết.
PHẦN 3/4 không thiết kế database chi tiết.
PHẦN 3/4 không thiết kế UI chi tiết.

## PHẦN 3/4 chỉ khóa module specification standard để dev không triển khai tùy ý.

## 33. MODULE SPECIFICATION PRINCIPLE

## 33.1. Mọi module phải có module contract

Một module chỉ được đưa vào kế hoạch triển khai khi có Module Contract rõ ràng.

Module Contract là bản cam kết kỹ thuật xác định:

Module làm gì.

Module không làm gì.

Module lấy dữ liệu từ đâu.

Module trả kết quả cho ai.

Module phụ thuộc ai.

Ai phụ thuộc module này.

Rule nào bắt buộc.

State nào bắt buộc.

Permission nào bắt buộc.

Audit nào bắt buộc.

Evidence nào bắt buộc.

Smoke nào bắt buộc.

P0 điểm chặn nào phải chặn release.

Không có Module Contract thì:

## 33.2. Module không được định nghĩa theo tên màn hình

Không được định nghĩa module theo kiểu:

màn hình sản phẩm;

màn hình đơn hàng;

màn hình kho;

màn hình AI;

màn hình live;

màn hình IVR.

Màn hình chỉ là presentation surface.
Module phải định nghĩa theo domain và responsibility.

Ví dụ:

Không viết:

“Module màn hình xác nhận đơn”

Phải viết:

“Order Confirmation / Order State Signal Module - nhận tín hiệu từ kênh xác nhận, đưa vào Core Order State Machine, không tự quyết trạng thái cuối.”

Không viết:

“Module bảng ROAS”

Phải viết:

“Ads Measurement / Verified Revenue Module - đọc doanh thu đã verified, áp attribution rule, hiển thị ROAS, không tạo doanh thu.”

## 33.3. Module phải có negative boundary

Mỗi module ngoài phần “được làm gì” phải có phần “không được làm gì”.

Đây là bắt buộc vì hệ thống Ginsengfood có nhiều rủi ro vượt boundary:

AI dễ vượt Commerce.

Facebook Gateway dễ vượt Order.

Ads dễ vượt Verified Revenue.

IVR dễ vượt Order State Machine.

UI dễ vượt Domain.

Dashboard dễ vượt Source-of-truth.

Integration Worker dễ vượt Core Decision.

Vì vậy, mỗi module phải ghi rõ:

## THIS MODULE MUST NOT...

## 34. MODULE SPECIFICATION TEMPLATE - BẢN CHUẨN

## 34.1. Mẫu tổng thể

Mỗi module trong TECH-01 đến TECH-10 phải dùng mẫu sau:

## MODULE SPEC - [MODULE CODE] - [MODULE NAME]

## 1. Module Identity

## 2. Module Purpose

## 3. Source-of-Truth Mapping

## 4. Scope In

## 5. Scope Out / Must Not Do

## 6. Upstream Dependencies

## 7. Downstream Dependencies

## 8. Domain Rules / Invariants

## 9. State Model / State Machine

## 10. Command Boundary

## 11. Query Boundary

## 12. Runtime / Resolver Boundary

## 13. Integration Boundary

## 14. Data Ownership Boundary

## 15. Permission / Role Boundary

## 16. Audit Requirement

## 17. Idempotency Requirement

## 18. Security / Privacy / Compliance Requirement

## 19. Exception / Error Handling Principle

## 20. Evidence Requirement

## 21. Smoke Requirement

## 22. P0 điểm chặn

## 23. Done Gate

## 24. Fail Gate

## 25. Release Readiness Rule

## 26. Production Readiness Rule

## 27. Owner Decision Points

## 28. Implementation Notes For Dev

## 29. Open Questions

## 30. Final Module Status

Mẫu này là bắt buộc.
Không được bỏ các mục trên nếu module có liên quan production.

## 35. MODULE IDENTITY

## 35.1. Nội dung bắt buộc

Mỗi module phải có thông tin định danh:

Trường

Nội dung

Module Code

Mã module

Module Name

Tên module

Application Group

App group chính

Repo / Workspace Target

Repo hoặc workspace dự kiến

Source Pack

PACK/Master làm nguồn

Wave

Wave triển khai

Owner Domain

Owner nghiệp vụ

Technical Owner

Owner kỹ thuật

Release Scope

Scope release dự kiến

Current Status

NOT_STARTED / IN_PROGRESS / bị chặn

Production Status

KHONG mặc định

## 35.2. Quy tắc đặt mã module

Module Code phải đủ rõ để truy ngược.

Đề xuất format:

## TECH-[PACK/WAVE]-[DOMAIN]-[NUMBER]

Ví dụ:

Module Code

Ý nghĩa

## TECH-W0-FOUNDATION-001

Foundation module

## TECH-W1-PRODUCT-001

Product master

## TECH-W2-OPS-001

Raw material intake

## TECH-W3-COMMERCE-001

Quote Snapshot

## TECH-W4-AI-001

AI Resolver

## TECH-W5-FB-001

Facebook Gateway handoff

## TECH-W6-ADS-001

Verified Revenue

## TECH-W7-LIVE-001

Live Plan

## TECH-W8-IVR-001

IVR Scheduler

## TECH-W9-RELEASE-001

Evidence Registry

Mã có thể điều chỉnh theo repo thực tế, nhưng phải thống nhất.

## 36. MODULE PURPOSE

## 36.1. Mục tiêu module

Module Purpose phải mô tả rõ:

Module sinh ra để giải quyết vấn đề gì.

Module phục vụ quy trình nào.

Module là source-of-truth cho dữ liệu nào nếu có.

Module chỉ là consumer của dữ liệu nào.

Module nằm ở tầng domain, application, integration, AI/channel hay presentation.

Ví dụ đúng:

Quote Snapshot Module tạo và lưu snapshot báo giá hợp lệ tại thời điểm quote, làm nguồn báo giá duy nhất cho order draft và downstream measurement. Module không tự quyết payment, không tự xác nhận revenue, không tự thay đổi giá sau khi quote đã snapshot.

Ví dụ sai:

Module này dùng để báo giá cho khách.

Cách viết sai quá mơ hồ, không đủ boundary.

## 37. SOURCE-OF-TRUTH MAPPING

## 37.1. Mỗi module phải map source-of-truth

Mỗi module phải chỉ rõ nguồn tài liệu nào quyết định rule của nó.

Nguồn

Vai trò

## MASTER

Governance / global rule

## PACK

Domain rule / boundary

Business Pack

Business requirement / role / audit

Functional Pack

Use case / acceptance criteria

UI Pack

Surface behavior, không phải business truth

Owner Decision

Quyết định mở rộng nếu có conflict

TECH Doc

Technical implementation standard

## 37.2. Template Source-of-Truth Mapping

Source Type

Document

Section / Rule

Impact

## MASTER

MASTER-xx

...

...

## PACK

PACK-xx

...

...

Business

...

...

...

Functional

...

...

...

## UI

...

...

...

Owner Decision

...

...

...

Nếu module không có source-of-truth rõ:

## 38. SCOPE IN

## 38.1. Scope In là phần module được phép làm

Scope In phải ghi cụ thể các trách nhiệm module được phép thực hiện.

Ví dụ với Quote Snapshot:

Nhận yêu cầu tạo quote từ AI/Channel/Admin.

Đọc price/program/member runtime hợp lệ.

Tạo snapshot báo giá.

Gắn quote với customer/session nếu có.

Giữ quote theo thời hạn.

Cung cấp quote cho cart/order draft.

Ghi audit/evidence.

Không ghi quá chung chung như:

xử lý quote.

## 39. SCOPE OUT / MUST NOT DO

## 39.1. Scope Out là phần module bị cấm

Scope Out bắt buộc phải có.

Ví dụ với Quote Snapshot Module:

Module này không được:

Tự xác nhận payment.

Tự tạo paid revenue.

Tự bypass stock/sellable.

Tự thay đổi quote sau khi snapshot nếu không có rule.

Tự quyết VAT nếu runtime chưa cung cấp.

Tự tính member benefit hardcode.

Tự thay Order State Machine.

## 39.2. Scope Out phải chứa P0 boundary

Các module liên quan AI/Commerce/Ads/IVR/Live phải có P0 boundary rõ.

Ví dụ:

AI Module không được tự tạo order.

Facebook Gateway không được tự tạo order.

Ads không được tự tạo revenue.

IVR không được tự hủy đơn.

MC AI Live không được chốt đơn public.

Public Trace không được đọc dữ liệu nội bộ trực tiếp.

## 40. UPSTREAM DEPENDENCIES

## 40.1. Upstream là gì

Upstream là module hoặc source mà module này phải phụ thuộc để vận hành đúng.

Ví dụ:

AI Advisor phụ thuộc:

Product Knowledge.

Price Runtime.

Stock/Sellable.

Claim Guard.

Commerce Runtime.

Customer Memory nếu có.

Nếu upstream chưa release, downstream không được production ở phần phụ thuộc.

## 40.2. Template Upstream Dependency

Upstream Module

Dependency Type

Required For

Status

Release Impact

Product Master

Data

Product advice

## WAITING

AI product advice bị chặn

Commerce Runtime

Runtime

Quote/order draft

bị chặn

AI quote bị chặn

Sale Lock

Guard

Sellable check

## WAITING

AI selling bị chặn

## 41. DOWNSTREAM DEPENDENCIES

## 41.1. Downstream là gì

Downstream là module phụ thuộc vào module này.

Ví dụ:

Quote Snapshot là upstream cho:

AI quote response.

Facebook Messenger order draft.

Cart draft.

Official order.

Ads funnel measurement.

Customer care reference nếu có.

Nếu Quote Snapshot fail, các downstream này phải bị chặn.

## 41.2. Template Downstream Dependency

Downstream Module

Depends On

Dependency Purpose

Impact If Fail

AI Advisor

Quote Snapshot

Báo giá

AI không được quote production

Ads Measurement

Verified order/revenue

## ROAS

Không có ROAS chính thức

## IVR

Order State Machine

Xác nhận đơn

IVR không được production

## 42. DOMAIN RULES / INVARIANTS

## 42.1. Domain rule là gì

Domain rule là quy tắc nghiệp vụ không được phá.

Ví dụ:

QC_PASS không đồng nghĩa RELEASED.

Cart draft không phải official order.

WAITING revenue không phải verified revenue.

IVR result chỉ là input signal.

AI không tự tính giá.

Public Trace whitelist-only.

Sale Lock thắng mọi luồng bán hàng.

Dashboard không phải source-of-truth.

## 42.2. Template Domain Rule

Rule ID

Rule

Source

Enforcement Layer

P0 If Violated

## RULE-001

Cart draft không phải official order

## PACK-04

Commerce Domain

CO

## RULE-002

IVR không tự hủy đơn

## PACK-09

Order State Machine

CO

## 42.3. Invariant phải enforce ở backend/domain

Rule P0 không được chỉ enforce ở UI.

Ví dụ:

UI disable nút bán hàng bị khóa là chưa đủ.

Backend/domain phải chặn order.

AI nói không bán là chưa đủ.

Commerce Runtime phải không cho quote/order.

## 43. STATE MODEL / STATE MACHINE

## 43.1. Khi nào module cần state machine

Module phải có state machine nếu có trạng thái chuyển đổi như:

Raw lot.

## QC.

Production order.

Batch release.

Warehouse receipt.

Inventory movement.

Recall.

Sale lock.

Quote.

Cart.

Order.

Payment.

IVR call.

Evidence.

Release status.

## 43.2. Template State Model

State

Ý nghĩa

Allowed Transition

bị chặn Transition

Owner/Audit Required

## DRAFT

Nháp

## SUBMITTED

## RELEASED

## SUBMITTED

Đã gửi

## APPROVED / REJECTED

## RELEASED

## APPROVED

Đã duyệt

RELEASED nếu đủ điều kiện

PAID nếu chưa payment

## 43.3. State transition phải có guard

Không state nào được chuyển chỉ vì frontend gọi.

Mỗi transition phải có:

Current state.

Target state.

Actor/role.

Permission.

Guard condition.

Reason nếu cần.

Audit.

Idempotency nếu command rủi ro.

Evidence nếu chạm release/smoke.

## 44. COMMAND BOUNDARY

## 44.1. Command là gì

Command là hành động thay đổi trạng thái hoặc dữ liệu.

Ví dụ:

Create quote.

Submit order.

Confirm payment.

Mark batch released.

Apply sale lock.

Submit IVR result.

Accept evidence.

Approve release.

Command phải được kiểm soát chặt.

## 44.2. Template Command Boundary

Command

Actor

Permission

Guard

Idempotency

Audit

P0 Risk

## CREATE_QUOTE

AI/System/Admin

quote.create

SKU sellable, price runtime valid

Required

Required

CO

## CONFIRM_PAYMENT

Payment/Admin

payment.confirm

Valid source

Required

Required

CO

## 44.3. Command P0 bắt buộc idempotency

Các command sau bắt buộc idempotency:

Create quote.

Create order.

Confirm payment.

Cancel order.

Submit IVR result.

Issue material.

Receive warehouse.

Release batch.

Apply recall.

Apply sale lock.

Sync MISA.

Send transaction notification.

Accept evidence.

Approve release.

## 45. QUERY BOUNDARY

## 45.1. Query là gì

Query là thao tác đọc dữ liệu.

Query không được thay đổi state.
Query không được tạo side effect.
Query không được âm thầm ghi lịch sử nghiệp vụ nếu không phải audit access được quy định.

Ví dụ:

Get product.

Get price runtime.

Get stock/sellable.

Get quote.

Get order status.

Get trace public.

Get ROAS dashboard.

Get IVR call status.

Get evidence status.

## 45.2. Query không được vượt data boundary

Ví dụ:

Public Trace query không được đọc internal trace detail.

Ads dashboard query không được đọc dữ liệu payment nhạy cảm vượt scope.

AI query không được đọc thông tin khách không cần thiết.

Supplier query không được đọc supplier khác.

Facebook Gateway query không được lấy payment detail public.

## 46. RUNTIME / RESOLVER BOUNDARY

## 46.1. Runtime là gì

Runtime là lớp cung cấp dữ liệu thay đổi theo thời gian hoặc cấu hình:

Giá.

Chương trình.

Tồn kho.

Sellable.

Member benefit.

Policy.

Product knowledge.

Claim whitelist.

Shipping.

Payment method.

Live product scope.

IVR program window.

Runtime không được hardcode trong AI, UI, Gateway hoặc script.

## 46.2. Template Runtime Boundary

Runtime Data

Source Module

Consumer

Hardcode Allowed

Fallback

Price

Commerce Runtime

AI/Facebook/Live

KHONG

Ask runtime unavailable / block quote

Stock/Sellable

Operational/Commerce

AI/Commerce/Ads

KHONG

Block selling

Claim

Claim Guard

AI/Live

KHONG

Safe response / handoff

## 47. INTEGRATION BOUNDARY

## 47.1. Integration phải có owner module

Mỗi integration phải có owner rõ.

Integration

Owner Module

Consumer

Payment Gateway

Commerce / Integration Worker

Order/Payment

## MISA

Integration Worker

Accounting/Revenue

Facebook

Facebook Gateway

AI/Commerce

Ads Platform

Ads Measurement

Dashboard/Scale

SIM Gateway

IVR Module

Order Signal

Notification

Integration Worker

Customer communication

## 47.2. Integration không tự quyết domain state

Integration chỉ cung cấp tín hiệu hoặc dữ liệu.

Ví dụ:

Payment provider gửi signal, nhưng Payment Module xác nhận state.

SIM Gateway gửi call result, nhưng Order State Machine quyết định đơn.

Facebook gửi comment, nhưng Gateway/AI/Commerce xử lý theo boundary.

MISA nhận/sync accounting, không tự quyết order paid.

Ads platform gửi event, không tự tạo revenue.

## 48. DATA OWNERSHIP BOUNDARY

## 48.1. Mỗi dữ liệu phải có owner

Data

Owner

Product

Product Module

## SKU

Product Module

Recipe / Formula

Product Module

Raw Lot

Operational Core

Batch

Operational Core

Inventory

Operational Core

Quote

Commerce Runtime

Order

Commerce Runtime

Payment

Commerce / Payment Boundary

Revenue

Verified Revenue Boundary

Customer Conversation

AI / Channel theo scope

Ads Event

Ads Measurement

IVR Call Result

IVR Module

Evidence

Evidence Registry

Release Status

Release Control

## 48.2. Consumer không được sửa dữ liệu owner

Ví dụ:

AI đọc price nhưng không sửa price.

Ads đọc verified revenue nhưng không sửa revenue.

Facebook đọc order status nhưng không sửa order state.

IVR gửi signal nhưng không sửa order trực tiếp.

Public Trace đọc public DTO nhưng không sửa trace data.

Dashboard đọc release status nhưng không sửa release.

## 49. PERMISSION / ROLE BOUNDARY

## 49.1. Mỗi command phải có permission

Không command quan trọng nào được chạy nếu không có permission.

Đặc biệt:

Product activation.

Recipe approval.

Batch release.

Warehouse receipt.

Inventory adjustment.

Recall.

Sale lock.

Quote override nếu có.

Payment confirmation.

Order cancellation.

Evidence acceptance.

Release approval.

IVR control.

MISA sync.

User/role management.

## 49.2. Template Permission Boundary

Action

Role Allowed

Permission

Reason Required

Audit Required

## RELEASE_BATCH

QC/Owner role

batch.release

## CONFIRM_PAYMENT

Accounting/Payment role

payment.confirm

## APPROVE_RELEASE

Owner

release.approve

## 50. AUDIT REQUIREMENT

## 50.1. Khi nào bắt buộc audit

Audit bắt buộc với:

State transition.

Approval/rejection.

Payment confirmation.

Order cancellation.

Release batch.

Apply sale lock.

Recall.

Inventory adjustment.

IVR result ingestion.

Evidence review.

Owner sign-off.

Release decision.

Security-sensitive action.

Admin override.

Data correction.

## 50.2. Audit tối thiểu phải có

Audit record tối thiểu phải có:

Actor.

Role.

Action.

Object type.

Object ID.

From state.

To state.

Reason.

Timestamp.

Source channel.

Correlation ID nếu có.

Idempotency key nếu command rủi ro.

Result.

Failure reason nếu fail.

## 51. IDEMPOTENCY REQUIREMENT

## 51.1. Khi nào bắt buộc idempotency

Idempotency bắt buộc khi command có rủi ro tạo trùng, tính tiền trùng, trừ kho trùng, gửi tin trùng hoặc tạo state sai.

Ví dụ:

Create order.

Create quote.

Confirm payment.

Cancel order.

Issue material.

Receive warehouse.

Submit IVR result.

Send notification.

Sync MISA.

Accept evidence.

Approve release.

## 51.2. Idempotency rule

Command gửi lại cùng idempotency key không được tạo kết quả trùng.

Nếu payload khác nhưng dùng cùng key:

## IDEMPOTENCY_CONFLICT

Nếu retry cùng payload:

## RETURN_EXISTING_RESULT

## 52. SECURITY / PRIVACY / COMPLIANCE REQUIREMENT

## 52.1. Module nào bắt buộc security review

Security review bắt buộc với:

Auth/User/Role/Permission.

Admin.

Payment.

Order.

Customer data.

AI conversation memory.

Facebook/Messenger integration.

Public Trace.

Supplier Portal.

Evidence upload.

IVR call logs.

MISA/accounting.

Release approval.

Data export.

## 52.2. Privacy boundary bắt buộc

Không public:

Số điện thoại.

Địa chỉ đầy đủ.

Payment detail.

Member tier nếu không cần.

Health note.

Internal order detail.

Supplier detail.

Operator/approver.

Cost/accounting.

QC defect nội bộ.

Internal root cause.

Evidence nội bộ.

## 53. EXCEPTION / ERROR HANDLING PRINCIPLE

## 53.1. Exception không được biến thành business decision sai

Ví dụ:

IVR technical error không phải khách không nghe.

Capacity overload không phải lỗi khách.

Payment timeout không phải unpaid chắc chắn.

MISA sync fail không tự đổi order state.

Facebook webhook fail không tự xóa lead.

AI runtime unavailable không được trả lời bịa.

Stock resolver fail không được chốt còn hàng.

Price resolver fail không được tự tính giá.

## 53.2. Template Exception Handling

Exception

Business Meaning

Action

Customer Impact

Evidence

## PRICE_RUNTIME_UNAVAILABLE

Không xác định được giá

Block quote / handoff

Không báo giá

Required

## IVR_TECHNICAL_ERROR

Lỗi kỹ thuật

Retry/incident

Không hủy đơn

Required

## PAYMENT_TIMEOUT

Chưa xác nhận

WAITING review

Không mark paid

Required

## 54. EVIDENCE REQUIREMENT

## 54.1. Mỗi module phải nêu evidence cần nộp

Evidence Requirement phải ghi rõ:

Evidence nào chứng minh implementation.

Evidence nào chứng minh test.

Evidence nào chứng minh smoke.

Evidence nào chứng minh security.

Evidence nào chứng minh no-bypass.

Evidence nào chứng minh P0 cleared.

Evidence nào cần owner review.

## 54.2. Template Evidence Requirement

Evidence ID

Evidence Type

Proves

Required For

Status

## EV-QT-001

Smoke result

Quote snapshot hoạt động

Release Ready

## WAITING

## EV-PAY-001

Test/security

Payment không mark paid sai

Release Ready

## WAITING

## EV-IVR-001

Simulation log

IVR không update order trực tiếp

Release Ready

## WAITING

## 55. SMOKE REQUIREMENT

## 55.1. Smoke phải bám P0

Smoke không chỉ test happy path.

Smoke phải test:

Happy path.

bị chặn path.

P0 violation path.

Dependency unavailable path.

Permission denied path.

Idempotency retry path.

Runtime unavailable path.

State transition invalid path.

Evidence accepted/rejected path nếu module release.

## 55.2. Template Smoke Requirement

Smoke ID

Scenario

Expected Result

P0 Covered

Evidence Required

## SMK-QT-001

SKU sellable tạo quote

Quote snapshot created

Quote boundary

## SMK-QT-002

SKU sale lock tạo quote

bị chặn

Sale lock

## SMK-IVR-001

IVR result cancel

Core decides final state

No direct cancel

## 56. P0 điểm chặn

## 56.1. Module phải khai báo P0 điểm chặn

Mỗi module phải ghi trước các lỗi P0.

Ví dụ:

Quote Snapshot P0:

Quote không snapshot.

Quote dùng giá hardcode.

Quote cho SKU sale lock.

Quote không audit.

Quote bị sửa sau khi tạo không có rule.

## IVR P0:

SIM Gateway update order trực tiếp.

Sai attempt/deadline.

Technical error bị xem là no answer.

Tin hủy gửi trước Core cancel.

## 56.2. P0 điểm chặn Template

## P0 ID

Description

Impact

Blocks

Clear Condition

## P0-QT-001

Quote không snapshot

Sai giá/order

AI/Facebook/Commerce

Fix + smoke + evidence

## P0-IVR-001

IVR update order trực tiếp

Hủy đơn sai

IVR production

Rewrite + smoke + owner review

## 57. DONE GATE

## 57.1. Done Gate không phải code complete

Module chỉ Done khi:

Source-of-truth clear.

Scope in/out clear.

Dependencies clear.

Implementation complete.

Test pass.

Smoke pass.

Evidence accepted.

P0 cleared.

Security pass nếu áp dụng.

Owner review nếu cần.

Release status updated.

Nếu mới code xong:

## IMPLEMENTATION_COMPLETE_ONLY

Không được gọi Done.

## 57.2. Done Gate Template

Done Condition

Required

Status

Source-of-truth mapped

## WAITING

Implementation complete

## WAITING

Test pass

## WAITING

Smoke pass

## WAITING

Evidence accepted

## WAITING

P0 cleared

## WAITING

Owner review

If required

## WAITING

## 58. FAIL GATE

## 58.1. Fail Gate phải ghi trước

Module phải khai báo điều kiện fail trước khi triển khai.

Ví dụ module AI Advisor fail nếu:

Hardcode giá.

Bán SKU hết hàng.

Nói claim điều trị.

Tự tạo order.

Không có evidence.

Không được đợi xảy ra lỗi rồi mới định nghĩa fail.

## 58.2. Fail Gate Template

Fail Condition

Impact

Required Action

Evidence missing

Release bị chặn

Submit evidence

Smoke fail

Release bị chặn

Fix + retest

P0 open

Release bị chặn

Clear P0

Dependency bị chặn

Production bị chặn

Wait upstream

## 59. RELEASE READINESS RULE

## 59.1. Module Release Ready khi nào

Module chỉ được Release Ready khi:

Done Gate đạt.

Smoke PASS.

Evidence ACCEPTED.

## P0 CLEARED.

Dependency CLEARED.

Security PASS nếu áp dụng.

Owner review không còn WAITING.

Release scope rõ.

Rollback/recovery note có nếu production risk.

Release Ready chưa phải Release Approved.

## 60. PRODUCTION READINESS RULE

## 60.1. Module Production Ready khi nào

Module chỉ được Production Ready khi:

Production config ready.

Monitoring ready.

Incident owner ready.

Rollback/recovery ready.

No P0 điểm chặn.

No dependency bị chặn.

No stale evidence.

Owner final decision có hiệu lực.

Production Ready phải theo scope.

Không được ghi:

Module này production ready.

Phải ghi:

Module này production ready cho scope nào, môi trường nào, kênh nào, điều kiện nào.

## 61. OWNER DECISION POINTS

## 61.1. Khi nào cần owner decision

Owner decision bắt buộc khi:

Có conflict giữa tài liệu và code.

Có conflict giữa hai pack.

Có scope excluded.

Có risk acceptance.

Có P0 waiver đề xuất.

Có release scope production.

Có go-live scope.

Có thay đổi rule nghiệp vụ.

Có thay đổi attempt/deadline IVR.

Có thay đổi payment/shipping/price/member policy.

Có dữ liệu public trace mới.

Có claim khoa học/sức khỏe mới.

## 61.2. Owner Decision Template

Decision ID

Topic

Options

Recommended

Risk

Owner Result

## DEC-001

Scope payment direct bank transfer

Enable / Exclude

Exclude until review

Payment mismatch

## WAITING

## 62. IMPLEMENTATION NOTES FOR DEV

## 62.1. Implementation notes không được biến thành code instruction cứng

Phần này chỉ ghi định hướng:

Module nên thuộc app nào.

Phải giữ boundary nào.

Phải có guard nào.

Cần test/smoke gì.

Cần evidence gì.

Cần owner quyết định gì.

Không ghi code cụ thể nếu chưa đến tài liệu implementation chi tiết.

## 62.2. Dev được quyền đề xuất kỹ thuật

Dev được quyền đề xuất:

Cách tổ chức project.

Database schema.

API design.

Queue/job.

Worker.

Cache.

Test framework.

Monitoring.

Deployment.

Refactor strategy.

Nhưng đề xuất không được phá source-of-truth.

## 63. OPEN QUESTIONS

## 63.1. Open Questions phải được ghi rõ

Nếu module còn câu hỏi chưa rõ, phải ghi vào Open Questions.

Không được tự đoán.

Template:

Question ID

Question

Impact

Owner Needed

Status

## Q-001

Payment method nào release trước?

Affects Commerce/AI/Ads

## WAITING

## 63.2. Open Question P0 phải chặn release

Nếu câu hỏi ảnh hưởng P0, trạng thái module phải là:

## OWNER_REVIEW_REQUIRED

Không được release khi câu hỏi P0 chưa trả lời.

## 64. FINAL MODULE STATUS

## 64.1. Status được phép dùng

Status

Ý nghĩa

## NOT_STARTED

Chưa bắt đầu

## SPEC_DRAFT

Đang viết đặc tả

## SPEC_COMPLETE

Đặc tả hoàn tất

## IMPLEMENTATION_WAITING

Chưa triển khai

## IMPLEMENTATION_COMPLETE_ONLY

Đã code xong nhưng chưa đủ evidence

## TEST_WAITING

Chưa test đủ

## SMOKE_WAITING

Chưa smoke

## EVIDENCE_WAITING

Chưa evidence accepted

## P0_BLOCKED

Có P0 điểm chặn

## DEPENDENCY_BLOCKED

Upstream chưa clear

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

## RELEASE_READY

Đủ điều kiện trình release

## RELEASE_DECISION_ACCEPTED

Được duyệt release

## PRODUCTION_READINESS

Sẵn sàng production theo scope

## GO_LIVE_DECISION_ACCEPTED

Được go-live theo scope

## RELEASE_HOLD

Tạm giữ release

## ROLLBACK_REQUIRED

Phải rollback

## EXCLUDED_FROM_SCOPE

Không nằm trong scope hiện tại

## 64.2. Status bị cấm

Không dùng:

Gần xong.

Cơ bản xong.

Tạm ổn.

Đã chạy.

Chạy được.

Xong phần lớn.

Pass miệng.

Owner ok miệng.

Dev nói ổn.

Go-live thử.

## 65. MODULE SPEC EXAMPLE - QUOTE SNAPSHOT MODULE

## 65.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W3-COMMERCE-QUOTE-001

Module Name

Quote Snapshot Module

Source Pack

## PACK-04

Upstream

Product/SKU, Price Runtime, Program Runtime, Member Runtime, Sellable

Downstream

Cart Draft, Order, AI Advisor, Facebook Gateway, Ads Measurement

Production Status

KHONG mặc định

## 65.2. Scope In

Tạo quote snapshot.

Ghi nhận giá tại thời điểm quote.

Ghi nhận chương trình áp dụng.

Ghi nhận quyền lợi thành viên nếu có.

Ghi thời hạn hiệu lực quote.

Cung cấp quote cho cart/order draft.

## 65.3. Must Not Do

Module này không được:

Tự xác nhận payment.

Tự tạo verified revenue.

Tự bypass sellable.

Tự tính giá hardcode.

Tự sửa quote đã snapshot không rule.

Tự quyết shipping nếu runtime chưa có.

## 65.4. P0 điểm chặn

Quote không snapshot.

Quote dùng giá hardcode.

Quote cho SKU không sellable.

Quote bị sửa sau khi tạo không audit.

Quote hết hạn vẫn dùng để order không rule.

## 65.5. Smoke

SKU sellable tạo quote PASS.

SKU sale lock tạo quote bị BLOCK.

Price runtime unavailable thì không báo giá bịa.

Quote hết hạn không dùng làm order nếu rule không cho.

AI tạo quote phải qua module này.

## 66. MODULE SPEC EXAMPLE - IVR CALL RESULT MODULE

## 66.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W8-IVR-CALLRESULT-001

Module Name

IVR Call Result Module

Source Pack

## PACK-09

Upstream

IVR Scheduler, SIM Gateway

Downstream

Core Order State Machine

Production Status

KHONG mặc định

## 66.2. Scope In

Nhận call result.

Phân loại DTMF.

Phân loại invalid phone.

Phân loại no answer.

Phân loại technical error.

Phân loại capacity overload.

Gửi input signal cho Order State Machine.

Ghi IVR evidence.

## 66.3. Must Not Do

Module này không được:

Tự hủy đơn.

Tự cập nhật order trực tiếp.

Gửi tin hủy đơn.

Nhầm technical error là no answer.

Nhầm capacity overload là lỗi khách.

Cho Admin sửa giả result.

Dùng IVR cho sales/CRM/marketing.

## 66.4. P0 điểm chặn

SIM Gateway update order trực tiếp.

DTMF 0 tự cancel order.

Technical error kích hoạt cancellation notice.

Capacity overload kích hoạt cancellation notice.

No answer chưa đủ attempts nhưng hủy đơn.

Tin hủy gửi trước Core cancel.

## 67. MODULE SPEC EXAMPLE - ADS VERIFIED REVENUE MODULE

## 67.1. Module Identity

Trường

Nội dung

Module Code

## TECH-W6-ADS-REVENUE-001

Module Name

Verified Revenue Module

Source Pack

## PACK-07

Upstream

Commerce, Payment, Accounting/MISA nếu scope có

Downstream

ROAS, CPA, AOV, Scale Gate

Production Status

KHONG mặc định

## 67.2. Scope In

Đọc doanh thu đã verified.

Gắn revenue với attribution rule.

Cung cấp số liệu cho ROAS.

Cung cấp số liệu cho scale/stop gate.

Ghi evidence measurement.

## 67.3. Must Not Do

Module này không được:

Tạo doanh thu.

Xem WAITING revenue là verified.

Xem inbox/comment là revenue.

Xem quote/cart là revenue.

Xem order unpaid là revenue.

Dùng dashboard làm source-of-truth.

## 67.4. P0 điểm chặn

ROAS tính từ WAITING revenue.

Attribution mơ hồ.

Dashboard làm nguồn doanh thu.

Scale khi Data Quality chưa PASS.

Scale khi owner chưa quyết định.

## 68. TECH-00 PART 3 DONE GATE

## PHẦN 3/4 được xem là hoàn tất khi đã khóa:

Module Specification Principle.

Module Contract.

Module Identity Template.

Source-of-Truth Mapping Template.

Scope In / Scope Out Template.

Upstream / Downstream Dependency Template.

Domain Rule / Invariant Template.

State Machine Template.

Command Boundary Template.

Query Boundary Template.

Runtime / Resolver Boundary.

Integration Boundary.

Data Ownership Boundary.

Permission / Audit / Idempotency Requirement.

Security / Privacy / Compliance Requirement.

Exception Handling Principle.

Evidence Requirement.

Smoke Requirement.

P0 điểm chặn Template.

Done Gate / Fail Gate Template.

Release / Production Readiness Rule.

Owner Decision Template.

Final Module Status Registry.

Module Spec examples.

## 69. TECH-00 PART 3 FAIL GATE

## PHẦN 3/4 fail nếu sau phần này dev vẫn:

Viết module không có source-of-truth.

Viết module không có scope out.

Viết module không có dependency.

Viết module không có P0 điểm chặn.

Viết module không có evidence.

Viết module không có smoke.

Viết module theo màn hình thay vì domain.

Để UI tự enforce rule P0.

Để AI tự tạo runtime.

Để Ads tự tạo revenue.

Để IVR tự đổi order state.

Để Integration Worker tự quyết business state.

Không có state machine cho module có trạng thái.

Không có audit cho command rủi ro.

Không có idempotency cho command rủi ro.

Gọi module Done khi mới code xong.

Nếu xảy ra:

## 70. KẾT LUẬN PHẦN 3/4

## PHẦN 3/4 của TECH-00 đã khóa mẫu đặc tả kỹ thuật chuẩn cho từng module trong hệ thống Ginsengfood.

Kết luận bắt buộc:

Mọi module phải có Module Contract.

Không module nào được triển khai nếu thiếu source-of-truth.

Không module nào được thiếu Scope Out / Must Not Do.

Không module nào được thiếu upstream/downstream dependency.

Module có state phải có state model/state machine.

Command rủi ro phải có permission, audit, idempotency.

Runtime data không được hardcode.

Integration không được tự quyết domain state.

Consumer không được sửa dữ liệu của owner module.

Evidence và smoke là bắt buộc.

P0 điểm chặn phải được khai báo trước.

Done Gate không đồng nghĩa code complete.

Release Ready không đồng nghĩa Release Approved.

Production Ready phải theo scope và cần owner decision.

Final Module Status chỉ được dùng các trạng thái chuẩn, không dùng trạng thái mơ hồ.

Trạng thái sau PHẦN 3/4:

## PHẦN 4/4 - DEV GAP REPORT / EVIDENCE / SMOKE / RELEASE HANDOFF STANDARD / TECH-00 FINAL CONCLUSION

## 71. MỤC ĐÍCH CỦA PHẦN 4/4

## PHẦN 4/4 khóa lớp bàn giao kỹ thuật sau khi TECH-00 hoàn tất.

Nếu PHẦN 1/4 khóa greenfield technical principles, PHẦN 2/4 khóa repository/application/module boundary, PHẦN 3/4 khóa module specification template, thì PHẦN 4/4 khóa:

Dev Gap Report Standard.

Legacy Classification Standard.

Evidence Standard.

Smoke Standard.

Release Handoff Standard.

Owner Review Standard.

TECH Document Completion Gate.

Technical Implementation Start Gate.

TECH-00 Final Conclusion.

## PHẦN 4/4 không cho phép dev bắt đầu code sâu ngay.
PHẦN 4/4 chỉ cho phép dev bắt đầu bước mapping / gap report / classification / evidence plan / smoke plan sau khi TECH-00 được owner chấp nhận.

## 72. DEV GAP REPORT PRINCIPLE

## 72.1. Gap Report là gì

Dev Gap Report là báo cáo so sánh giữa:

Tài liệu kỹ thuật mới yêu cầu gì
Repo/code hiện tại đang có gì
Thiếu gì
Sai gì
Mâu thuẫn gì
Cần viết mới gì
Có phần nào dùng lại được không
Có phần nào phải bỏ không
Có phần nào cần owner quyết định không

Gap Report không phải báo cáo tiến độ.
Gap Report không phải danh sách task code.
Gap Report không phải cảm nhận “có vẻ dùng lại được”.
Gap Report là bước bắt buộc để tránh dev bị kéo về logic cũ.

## 72.2. Không có Gap Report thì không code sâu

Nếu chưa có Gap Report:

Dev chỉ được đọc tài liệu, map module, phân loại repo/code, đặt câu hỏi owner.
Dev chưa được triển khai sâu các module production.

## 73. CURRENT STATE CLASSIFICATION

## 73.1. Phân loại hiện trạng kỹ thuật

Mọi phần code/repo/tài liệu cũ nếu tồn tại phải được phân loại theo bảng sau:

Classification

Ý nghĩa

Hành động

## REUSE_AS_IS

Dùng lại nguyên trạng được

Vẫn cần test/smoke/evidence

## REUSE_WITH_REFACTOR

Dùng lại một phần

Refactor theo TECH docs

## REWRITE_REQUIRED

Không phù hợp

Viết lại

## DEPRECATE

Không dùng nữa

Đưa vào kế hoạch loại bỏ

## LEGACY_CONFLICT

Mâu thuẫn source-of-truth mới

Chặn triển khai, owner review

## SECURITY_REVIEW_REQUIRED

Có rủi ro bảo mật

Security review trước

## DATA_RISK_REVIEW_REQUIRED

Có rủi ro dữ liệu

Data review trước

## UNKNOWN

Chưa rõ

Không được production

## OWNER_REVIEW_REQUIRED

Cần owner quyết định

Chờ quyết định

## 73.2. Không dùng lại vì “đang chạy được”

Một phần cũ đang chạy được chưa chắc đúng.

Chỉ được dùng lại khi chứng minh:

Không trái MASTER/PACK.

Không trái TECH-00.

Không hardcode runtime.

Không bypass state machine.

Không bypass permission/audit.

Không vi phạm security/privacy.

Có test.

Có smoke.

Có evidence accepted.

Có owner/technical review nếu scope rủi ro.

Nếu không chứng minh được:

## 74. TECHNICAL GAP REPORT TEMPLATE

## 74.1. Template bắt buộc

Dev phải nộp Gap Report theo cấu trúc:

Cột

Nội dung

Gap ID

Mã gap

Source Document

MASTER/PACK/TECH liên quan

Source Rule

Rule hoặc requirement

Module

Module bị ảnh hưởng

Application / Repo

App/repo liên quan

Current State

Hiện trạng code/repo nếu có

Target State

Trạng thái mục tiêu theo tài liệu mới

Gap Type

Missing / Mismatch / Conflict / Legacy / Security / Data / Unknown

Severity

## P0 / P1 / P2

Impact

Ảnh hưởng nếu không xử lý

Dependency Impact

Module downstream bị chặn

Suggested Action

Reuse / Refactor / Rewrite / Deprecate / Owner Review

Evidence Required

Evidence cần nộp

Smoke Required

Smoke cần chạy

Owner Decision Required

CO / KHONG

Status

OPEN / IN_PROGRESS / READY_FOR_REVIEW / CLOSED

## 74.2. Gap Type chuẩn

Gap Type

Ý nghĩa

Missing

Chưa có module/chức năng

Mismatch

Có nhưng không đúng tài liệu mới

Conflict

Mâu thuẫn trực tiếp với rule đã khóa

Legacy

Còn logic cũ

Security

Rủi ro bảo mật

Data

Rủi ro dữ liệu

Boundary

Vượt module boundary

Runtime

Hardcode hoặc không lấy runtime

Evidence

Thiếu evidence

Smoke

Thiếu smoke

Unknown

Chưa đủ thông tin

## 74.3. Severity chuẩn

Severity

Ý nghĩa

Release impact

## P0

Chặn release/go-live

bị chặn

## P1

Ảnh hưởng lớn nhưng có thể xử lý theo scope

## OWNER_REVIEW_REQUIRED

## P2

Cải thiện/hoàn thiện

Không chặn nếu owner chấp nhận

## INFO

Ghi nhận

Không chặn

P0 không được hạ cấp để chạy nhanh.

## 75. CONFLICT REVIEW STANDARD

## 75.1. Khi nào gọi là conflict

Conflict xảy ra khi:

Code cũ khác tài liệu mới.

Tài liệu cũ khác MASTER/PACK mới.

Hai pack có vẻ mâu thuẫn.

Dev không biết nên theo rule nào.

Business thực tế khác tài liệu đã khóa.

Owner muốn thay đổi rule sau khi đã khóa.

Module downstream cần upstream chưa có.

Evidence trái với rule.

Smoke pass nhưng rule vẫn sai boundary.

## 75.2. Conflict không được tự xử lý im lặng

Khi có conflict, dev phải:

Ghi conflict vào Gap Report.

Nêu source A.

Nêu source B.

Nêu ảnh hưởng.

Nêu rủi ro.

Đề xuất hướng xử lý.

Đánh OWNER_REVIEW_REQUIRED.

Chờ quyết định.

Không tự chọn theo code cũ.
Không tự chọn theo hướng dễ làm.
Không tự sửa tài liệu đã khóa.

## 76. EVIDENCE STANDARD

## 76.1. Evidence là điều kiện bắt buộc

Mọi module, wave, release scope phải có evidence.

Evidence không phải lời nói.
Evidence không phải ảnh chụp rời rạc.
Evidence không phải demo miệng.
Evidence không phải dashboard xanh.

Evidence phải có:

Evidence ID.

Module.

Pack/TECH source.

Environment.

Version/build/config.

Test/smoke case.

Result.

Timestamp.

Executor.

Reviewer.

Status.

Link/file/log/report nếu có.

## 76.2. Evidence Status chuẩn

Evidence Status

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

Đã bị thay bởi evidence mới

Không

## 76.3. Evidence bị REJECTED nếu

Evidence phải bị từ chối nếu:

Không rõ module.

Không rõ source rule.

Không rõ environment.

Không rõ version.

Không rõ test/smoke case.

Không chứng minh được requirement.

Có dấu hiệu bypass.

Có P0 chưa clear.

Có conflict chưa xử lý.

Evidence đã stale.

Evidence không có reviewer.

Evidence không gắn release scope.

## 77. SMOKE STANDARD

## 77.1. Smoke không chỉ là happy path

Smoke bắt buộc phải kiểm tra:

Happy path.

bị chặn path.

Permission denied path.

Invalid transition path.

Runtime unavailable path.

Dependency unavailable path.

Idempotency retry path.

P0 violation path.

Evidence accepted/rejected path nếu liên quan release.

Rollback/hold path nếu liên quan production.

## 77.2. Smoke Template

Smoke ID

Module

Scenario

Expected Result

P0 Covered

Evidence Required

Status

## SMK-XXX-001

Module name

Happy path

Success

Rule ID

## WAITING

## SMK-XXX-002

Module name

bị chặn path

bị chặn

Rule ID

## WAITING

## SMK-XXX-003

Module name

Runtime unavailable

No fake result

Rule ID

## WAITING

## 77.3. Smoke PASS không tự động Release Approved

Smoke PASS chỉ là điều kiện để xét Release Ready.

Sau Smoke PASS vẫn cần:

Evidence ACCEPTED.

## P0 CLEARED.

Security PASS nếu có.

Dependency CLEARED.

Owner review.

Release decision.

## 78. RELEASE HANDOFF STANDARD

## 78.1. Release Handoff là gì

Release Handoff là gói bàn giao từ dev/QA/tech lead sang owner để xét:

Module đã đủ chưa.

Evidence đã đủ chưa.

Smoke đã pass chưa.

P0 đã clear chưa.

Dependency còn chặn không.

Có được Release Ready không.

Có được Release Approved không.

Có được Go-live Approved không.

## 78.2. Release Handoff Package bắt buộc

Một Release Handoff Package phải có:

Mục

Nội dung

Release Package ID

Mã gói release

Scope

Module/wave/app/kênh/SKU/payment scope

Source Documents

MASTER/PACK/TECH liên quan

Implementation Summary

Tóm tắt đã triển khai

Test Summary

Kết quả test

Smoke Summary

Kết quả smoke

Evidence List

Evidence đã nộp

Evidence Status

## WAITING / ACCEPTED / REJECTED

P0 điểm chặn Status

OPEN / CLEARED

Dependency Status

CLEAR / bị chặn / EXCLUDED

Security Status

PASS / WAITING / FAILED nếu áp dụng

Risk Register

Rủi ro còn lại

Excluded Scope

Phạm vi chưa release

Rollback Note

Cách rollback nếu fail

Monitoring Note

Cách theo dõi nếu production

Owner Questions

Câu hỏi cần quyết

Proposed Status

RELEASE_READY / bị chặn / OWNER_REVIEW_REQUIRED

## 78.3. Không có Release Handoff Package thì không Release Ready

Nếu thiếu Release Handoff Package:

## 79. OWNER REVIEW STANDARD

## 79.1. Owner Review không phải đồng ý miệng

Owner Review phải có record.

Record phải ghi:

Owner.

Role.

Scope.

Version.

Evidence reviewed.

Smoke reviewed.

P0 status.

Dependency status.

Decision.

Conditions.

Effective time.

## 79.2. Owner Decision chuẩn

Decision

Ý nghĩa

## ACCEPT_EVIDENCE

Chấp nhận evidence

## REJECT_EVIDENCE

Từ chối evidence

## REQUIRE_RETEST

Yêu cầu test lại

## CLEAR_P0

Chấp nhận P0 đã clear

## HOLD_RELEASE

Tạm giữ release

## APPROVE_RELEASE

Duyệt release

## APPROVE_LIMITED_GO_LIVE

Duyệt go-live giới hạn

## APPROVE_FULL_GO_LIVE

Duyệt go-live đầy đủ

## REQUIRE_ROLLBACK

Yêu cầu rollback

## EXCLUDE_SCOPE

Loại scope khỏi release hiện tại

## 80. TECH DOCUMENT COMPLETION GATE

## 80.1. TECH document complete khi nào

Một TECH document chỉ được xem là complete khi có:

Purpose.

Source-of-truth.

Module boundary.

Application boundary.

Dependency.

Runtime boundary.

Integration boundary.

Data ownership.

Permission/audit/idempotency.

Evidence requirement.

Smoke requirement.

P0 điểm chặn.

Done Gate.

Fail Gate.

Release readiness rule.

Owner decision points.

Nếu thiếu các mục trên:

## TECH_DOCUMENT_STATUS = INCOMPLETE

## 80.2. TECH document complete không phải implementation complete

Khi TECH document hoàn tất, trạng thái đúng là:

## DOCUMENTATION_COMPLETE

Không được gọi:

## IMPLEMENTATION_COMPLETE
RELEASE_READY
PRODUCTION_READINESS
GO_LIVE_DECISION_ACCEPTED

## 81. TECH-01 ĐẾN TECH-10 - READINESS TO WRITE

Sau TECH-00, các tài liệu tiếp theo phải viết theo thứ tự:

Tài liệu

Điều kiện bắt đầu

## TECH-01

TECH-00 complete

## TECH-02

TECH-01 foundation principle complete

## TECH-03

TECH-01/02 dependency clear ở mức tài liệu

## TECH-04

TECH-01/02/03 boundary rõ

## TECH-05

TECH-04 Commerce Runtime boundary rõ

## TECH-06

TECH-05 AI boundary và TECH-04 Commerce rõ

## TECH-07

TECH-04 Verified Revenue boundary rõ

## TECH-08

TECH-05/06/07 dependency rõ

## TECH-09

TECH-04 Order State Machine boundary rõ

## TECH-10

TECH-01->TECH-09 evidence/smoke/release structure rõ

## 82. TECHNICAL IMPLEMENTATION START PACKAGE

## 82.1. Gói bắt đầu triển khai

Trước khi dev bắt đầu code theo greenfield, phải có:

TECH-00 completed.

TECH module docs liên quan completed.

Repository Map.

Application Boundary Map.

Module-to-App Map.

Legacy Classification Report.

Gap Report.

Dependency Report.

Evidence Plan.

Smoke Plan.

P0 điểm chặn Baseline.

Owner-approved implementation wave.

Nếu thiếu:

## 83. DAILY DEV REPORT STANDARD

## 83.1. Báo cáo ngày bắt buộc

Dev report hằng ngày phải có:

Wave.

Module.

Source document.

Việc đã làm.

Gap mới phát hiện.

P0 điểm chặn mới.

Dependency bị chặn.

Evidence đã nộp.

Smoke đã chạy.

Owner question.

Ngày tiếp theo làm gì.

Không báo cáo kiểu:

đang làm tiếp;

gần xong;

chạy được rồi;

đang fix;

còn chút nữa.

## 84. WEEKLY OWNER REVIEW STANDARD

## 84.1. Báo cáo tuần cho owner

Báo cáo tuần phải có:

Wave status.

Module status.

Gap report summary.

P0 điểm chặn status.

Evidence acceptance status.

Smoke status.

Dependency status.

Security issue nếu có.

Scope có thể release.

Scope phải excluded.

Owner decisions required.

Proposed next wave.

## 85. FINAL TECH-00 DONE GATE

TECH-00 chỉ được xem là hoàn tất khi đã khóa đủ:

Greenfield Technical Reset.

Source-of-truth hierarchy.

No-code-yet rule.

No copy-paste rule.

Architecture boundary.

Technical layer model.

Module boundary.

Repository/application boundary.

Legacy classification.

Environment boundary.

Data boundary.

Integration boundary.

Module specification template.

Evidence standard.

Smoke standard.

Gap report standard.

Release handoff standard.

Owner review standard.

TECH document completion gate.

Implementation start package.

## 86. FINAL TECH-00 FAIL GATE

TECH-00 fail nếu sau tài liệu này đội kỹ thuật vẫn:

Dùng tài liệu cũ làm nền.

Dùng code cũ làm business truth.

Code sâu trước khi có mapping.

Copy-paste code AI.

Thiếu module contract.

Thiếu repo/app map.

Thiếu gap report.

Thiếu dependency report.

Thiếu evidence plan.

Thiếu smoke plan.

Gọi code complete là done.

Gọi documentation complete là production ready.

Cho AI/Facebook/Ads/Live/IVR vượt Commerce hoặc Operational Core.

Cho IVR cập nhật order trực tiếp.

Cho Ads tính ROAS từ WAITING revenue.

Cho Public Trace đọc dữ liệu nội bộ.

Cho dashboard làm source-of-truth.

Nếu có một lỗi:

## 87. FINAL TECH-00 STATUS

Sau khi TECH-00 hoàn tất, trạng thái đúng là:

Status Group

Value

## TECH-00_DOCUMENTATION_STATUS

## COMPLETE

## GREENFIELD_TECHNICAL_RESET

## LOCKED

## LEGACY_DOCS

## LEGACY_REFERENCE_ONLY

## LEGACY_CODE

## CURRENT_IMPLEMENTATION_STATE_ONLY

## MODULE_SPEC_TEMPLATE

## LOCKED

## REPO_MAPPING

## REQUIRED_BEFORE_CODE

## GAP_REPORT

## REQUIRED_BEFORE_CODE

## EVIDENCE_PLAN

## REQUIRED_BEFORE_CODE

## SMOKE_PLAN

## REQUIRED_BEFORE_CODE

## START_IMPLEMENTATION

## NOT_YET_FULLY_ALLOWED

## IMPLEMENTATION_STATUS

## WAITING

## TEST_STATUS

## WAITING

## SMOKE_STATUS

## WAITING

## EVIDENCE_STATUS

## WAITING

## RELEASE_READY

KHONG

## PRODUCTION_READINESS

KHONG

## RELEASE_DECISION_ACCEPTED

KHONG

## GO_LIVE_DECISION_ACCEPTED

KHONG

## 88. TECH-00 FINAL CONCLUSION

TECH-00 đã khóa:

Làm mới tài liệu kỹ thuật hoàn toàn theo MASTER / PACK / DEV EXECUTION COMMAND PACK.

Không dùng tài liệu cũ làm source-of-truth.

Không dùng code cũ làm business truth.

Không code sâu trước khi có mapping.

Không copy-paste code rời rạc.

Kiến trúc phải chia theo domain và module boundary.

Repository không quyết định nghiệp vụ.

Application boundary phải rõ.

Module nào cũng phải có Module Contract.

Module nào cũng phải có Source-of-truth Mapping.

Module nào cũng phải có Scope In và Must Not Do.

Module nào cũng phải có upstream/downstream dependency.

Command rủi ro phải có permission, audit, idempotency.

Runtime data không được hardcode.

Integration không được tự quyết domain state.

Public Trace chỉ whitelist-only.

AI không thay Commerce/Product/Stock/Claim.

Facebook Gateway không tạo order.

Ads không tạo revenue.

MC AI Live không chốt đơn public.

IVR không hủy đơn trực tiếp.

Evidence và smoke là bắt buộc.

Gap Report là bắt buộc trước khi code sâu.

Release Handoff Package là bắt buộc trước Release Ready.

Owner Review phải có record.

Documentation Complete không phải Production Ready.

Kết luận cuối:

TECH-00 hoàn tất về mặt tài liệu kỹ thuật cha.
Đội kỹ thuật chưa được gọi hệ thống là implementation complete, release ready, production ready hoặc go-live approved.
Bước tiếp theo là viết TECH-01 đến TECH-10 theo chuẩn TECH-00; sau đó dev mới lập Repo Map, Application Boundary Map, Module Map, Legacy Classification Report, Gap Report, Evidence Plan và Smoke Plan trước khi code sâu.

Trạng thái cuối bắt buộc:

## GINSENGFOOD TECHNICAL IMPLEMENTATION MASTER PLAN

# P3 - README HANDOFF INDEX

## Update 2026-06-05 - SRS handoff / ChatGPT approval gate

Trước khi đưa Phase 3 lên ChatGPT, sếp, dev lead hoặc đội triển khai, bắt buộc đọc thêm:

- `11-GÓI-CHUẨN-DUYỆT-SRS-TRIỂN-KHAI-GIAI-ĐOẠN-3.md`

File này là cổng chuẩn duyệt để Phase 3 được dùng làm nguồn phân tích SRS. File này không thêm rule nghiệp vụ mới, chỉ khóa cách đọc, cách review, cách phân biệt Phase 3 với Phase 3.1 bổ sung, checklist SRS, owner decision, P0 fail gate và trạng thái không được gọi quá mức.

Trạng thái đúng sau khi đọc bộ Phase 3:

```text
PHASE 3 DOCUMENTATION/SRS-HANDOFF READY
IMPLEMENTATION NOT EXECUTED
EVIDENCE NOT ACCEPTED
OWNER SIGN-OFF REQUIRED
GATEWAY BLOCKED
NOT RELEASE READY
NOT PRODUCTION READY
```

Lưu ý đặt tên: trong folder này `P3.1 Technical Design` là bước thiết kế kỹ thuật của Phase 3 gốc; không được nhầm với folder `phase-3.1`, là phase bổ sung nối tiếp Phase 3 cho Diamond/member/CRM/AI policy.

## Update 2026-05-22 - Plan tiep Phase 3

Dung thu muc chinh thuc: `docs/documents/4. phase/phase-3/`.

Khong dung `docs/documents/4_phase/` lam source-of-truth. Neu phat hien folder nay trong repo, xem la duplicate/deprecated va khong giao dev/Codex/Copilot chay tu do.

Phase 3 chua duoc goi ready/release-ready neu P2.2F chua co owner/dev lead acceptance. Buoc dau tien bat buoc la P2 Exit Gate:

- Doc P2.2F smoke/evidence report.
- Gom blocker/risk register.
- Xac nhan khong con P0/P1/P2 blocker nghiem trong.
- Xin owner/dev lead cho phep bat dau P3 Analysis.
- Neu chua ro approval, ghi blocker `P3-ENTRY-BLOCKED-BY-P2-OWNER-REVIEW`.

Global Gateway tiep tuc `BLOCKED` trong toan Phase 3. Khong mo Gateway, khong tu ket luan production readiness.

Thu tu thuc thi tiep theo:

0. `10-P3-ADDENDUM-CUSTOMER-TO-CASH-RUNTIME-LOCK.md`: đọc trước khi implement để khóa DailySalesContext, QuoteSnapshot, order/payment/shipping state bridge, verified revenue, Diamond/Finance handoff và P0 smoke sau vòng bổ sung CRM/Member/Gateway.
1. `11-GÓI-CHUẨN-DUYỆT-SRS-TRIỂN-KHAI-GIAI-ĐOẠN-3.md`: đọc trước khi đưa ChatGPT/sếp/dev review để khóa SRS handoff, approval prompt, owner decision và trạng thái không release-ready.
2. `00-P3-ANALYSIS-ONLY.md`: inspect backend that, map module/API/schema/test voi TECH-04, TECH-11/12/13 va PACK-10.
3. `01-P3-1-TECHNICAL-DESIGN-ONLY.md`: khoa contract, API/DTO/state machine/event can doi; moi API/DTO change phai bao frontend repo cap nhat.
4. `02-P3-2A-SELLABLE-GATE-RUNTIME.md`: COM-BLG-001, Operational blocked -> Commerce blocked; Product Active khong dong nghia Sellable.
5. `03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md`: COM-BLG-002/003, khong co QuoteSnapshot thi khong bao final price.
6. `04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md`: COM-BLG-004, Cart/Order Draft khong phai Official Order va khong tao order_code som.
7. `05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md`: COM-BLG-005/006, chi tao Official Order sau customer confirmation hop le.
8. `06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md`: COM-BLG-007/008, payment proof khong phai PAID; COD pending khong phai revenue.
9. `07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md`: COM-BLG-009/010/011, Verified Revenue chi tu order/payment verified; bank account khong hardcode.
10. `08-P3-2G-SMOKE-EVIDENCE-REPORT.md`: COM-BLG-012, nop report 14 muc, evidence submitted only, owner review.

Dieu kien toi thieu de chuyen tiep Phase 4:

- Đã đọc và map P3 addendum Customer-to-Cash vào implementation boundary.
- Đã đọc `11-GÓI-CHUẨN-DUYỆT-SRS-TRIỂN-KHAI-GIAI-ĐOẠN-3.md` và tạo SRS trước khi code.
- COM-BLG-002 QuoteSnapshot boundary co evidence.
- COM-BLG-004 Order Draft boundary co evidence.
- Khong con P0 Commerce blocker.
- Global Gateway van `BLOCKED`.

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PHASE-03 file package tu 00 den 08.
- MASTER release va go-live control.
- PACK-10 Evidence / Smoke / Completion Gateway.
- TECH-13 Codex / Copilot execution handoff report template.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/README-PHASE-03-HANDOFF-INDEX.md

## README - PHASE-03 COMMERCE RUNTIME HANDOFF INDEX / EXECUTION ORDER / GATEWAY LOCK

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là file README tổng hợp cho toàn bộ PHASE-03 - Commerce Runtime.

Mục tiêu của file này là giúp dev/Codex/Copilot hiểu:

PHASE 3 gồm những file nào.

Chạy file nào trước, file nào sau.

File nào chỉ phân tích, file nào chỉ thiết kế, file nào mới được implementation.

Phạm vi từng file.

Gate nào phải đạt trước khi chuyển sang file tiếp theo.

điểm chặn nào tuyệt đối không được bỏ qua.

Evidence nào cần gom.

Sau PHASE 3 không được tự gọi Release Readiness / Production Readiness / Go-live Decision.

README này không thay thế các prompt chi tiết.

README này không phải prompt viết code.

README này không cho phép copy-paste code từ AI.

README này là tài liệu điều phối để triển khai đúng quy trình kỹ thuật.

## 3. PHẠM VI PHASE 3

PHASE 3 tập trung vào Commerce Runtime, tức là lớp quyết định thương mại thật trước khi AI Advisor, Gateway, Ads, Live, CRM, IVR hoặc bất kỳ kênh nào được phép bán hàng.

PHASE 3 bao gồm:

Sellable Gate.

Listed Price Runtime.

Program Eligibility.

Member Benefit Eligibility.

Diamond Referral Eligibility.

QuoteSnapshot.

Cart.

Order Draft.

Customer Confirmation.

Official Order.

order_code.

Order State Machine.

Payment Selection.

Payment Core Confirmation.

Shipping.

Invoice / Tax.

## ORDER_VERIFIED.

Verified Revenue.

Commission handoff.

ROAS handoff.

Payout eligibility boundary.

Smoke / Evidence / Release-gate report.

## 4. NGUYÊN TẮC CỐT LÕI

## 4.1. Không xây hệ thống bằng copy-paste code

Developer không được hiểu các file PHASE 3 là code để copy-paste.

AI chỉ hỗ trợ:

Phân tích.

Viết prompt handoff.

Viết checklist.

Viết smoke matrix.

Viết evidence matrix.

Viết report format.

Gợi ý boundary.

Developer phải:

Inspect codebase thật.

Đọc kiến trúc thật.

Đối chiếu database thật.

Kiểm tra API thật.

Kiểm tra module thật.

Implement theo architecture thật.

Build/test/smoke thật.

Nộp evidence thật.

Không có chuyện lấy vài đoạn code AI rồi dán vào là có Commerce Runtime.

Ví dụ đơn giản: chức năng tạo đơn không chỉ là form + API. Nó còn liên quan đến QuoteSnapshot, Customer Confirmation, order_code, idempotency, audit, state machine, payment, shipping, verified revenue và downstream commission/ROAS. Nếu copy-paste rời rạc, hệ thống sẽ dễ lỗi, sai tiền, sai đơn, sai doanh thu và không bảo trì được.

## 4.2. Người dùng quyết định "muốn gì", developer quyết định "làm thế nào"

Người dùng/business owner quyết định:

Quy tắc bán hàng.

Quy tắc giá.

Quy tắc chương trình.

Quy tắc quyền lợi thành viên.

Quy tắc xác nhận đơn.

Quy tắc thanh toán.

Quy tắc verified revenue.

Quy tắc hoa hồng/ROAS/payout.

Developer quyết định:

Kiến trúc code.

Entity.

## API.

Service.

Migration.

Test.

Deployment.

Debug.

Performance.

Security implementation.

## 5. CẤU TRÚC FILE PHASE 3

PHASE-03-COMMERCE-RUNTIME/
├── README-PHASE-03-HANDOFF-INDEX.md
├── 00-P3-ANALYSIS-ONLY.md
├── 01-P3-1-TECHNICAL-DESIGN-ONLY.md
├── 02-P3-2A-SELLABLE-GATE-RUNTIME.md
├── 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md
├── 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md
├── 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md
├── 06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md
├── 07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md
└── 08-P3-2G-SMOKE-EVIDENCE-REPORT.md

## 6. THỨ TỰ THỰC THI BẮT BUỘC

PHASE 3 phải chạy đúng thứ tự sau:

Thứ tự

File

Mode

Mục đích

Có được sửa code không

README-PHASE-03-HANDOFF-INDEX.md

Handoff Index

Điều phối toàn phase

Không

## 00-P3-ANALYSIS-ONLY.md

## ANALYSIS ONLY

Phân tích codebase thật

Không

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md

## TECHNICAL DESIGN ONLY

Thiết kế workstream/boundary

Không

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md

## LIMITED IMPLEMENTATION

Sellable Gate Runtime

Có giới hạn

## 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md

## LIMITED IMPLEMENTATION

Giá/chương trình/member/QuoteSnapshot

Có giới hạn

## 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md

## LIMITED IMPLEMENTATION

Cart/Draft/Confirmation

Có giới hạn

## 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md

## LIMITED IMPLEMENTATION

Official Order/order_code/state

Có giới hạn

## 06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md

## LIMITED IMPLEMENTATION

Payment/Shipping/Invoice/Tax

Có giới hạn

## 07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md

## LIMITED IMPLEMENTATION

Verified Revenue/Commission/ROAS

Có giới hạn

## 08-P3-2G-SMOKE-EVIDENCE-REPORT.md

## VALIDATION / SMOKE / EVIDENCE REPORT ONLY

Smoke/evidence/report

Không thêm feature

Không được nhảy từ README sang implementation.

Không được bỏ qua Analysis.

Không được bỏ qua Technical Design.

Không được chạy implementation nếu owner chưa duyệt boundary.

Không được mở Gateway sau smoke cục bộ.

## 7. KHÓA RULE PHASE 3

## 7.1. Sellable Gate

Sellable Gate là runtime gate riêng.

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Warehouse Receipt không tự set Sellable.

Sellable phải phụ thuộc tối thiểu:

Product/SKU active.

Listed price active.

Stock available.

Finished goods released.

Warehouse receipt confirmed.

No recall.

No sale lock.

No quality hold.

No channel suppression.

Runtime policy available.

## 7.2. QuoteSnapshot

QuoteSnapshot là nguồn sự thật cho giá cuối.

Không có QuoteSnapshot thì không có final price.

Listed price không phải final price.

Program discount phải do runtime resolve.

Member benefit phải do runtime resolve.

Diamond referral chỉ snapshot eligibility, không tạo commission final.

Shipping fee/VAT nếu có trong quote phải đến từ runtime/policy.

AI/Gateway không được hardcode giá/discount/member benefit/shipping/VAT.

QuoteSnapshot phải immutable.

QuoteSnapshot phải có expiry/freeze window.

## 7.3. Cart / Order Draft / Customer Confirmation

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Order Draft phải gắn QuoteSnapshot active.

Customer Confirmation mới là điều kiện tạo Official Order.

Customer Confirmation phải idempotent.

QuoteSnapshot expired/voided thì không được xác nhận.

Accepted confirmation ở P3.2C chưa tạo Official Order.

Không có order_code trong P3.2C.

## 7.4. Official Order / order_code

Official Order chỉ tạo từ Customer Confirmation accepted.

Official Order phải có order_code.

order_code unique và stable.

order_code chỉ tạo một lần.

Idempotent retry không được tạo duplicate order.

Official Order created không đồng nghĩa Paid.

Official Order created không đồng nghĩa Verified Revenue.

Official Order không tạo commission final/ROAS/payout.

## 7.5. Payment / Shipping / Invoice / Tax

Payment selected không đồng nghĩa Paid.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới set Paid.

COD không tự thêm phí nếu policy chưa khóa.

Shipping fee phải do runtime/policy tính.

VAT/tax phải do runtime/policy tính.

Paid không đồng nghĩa Verified Revenue.

Delivered không đồng nghĩa Verified Revenue.

Invoice requested/issued không đồng nghĩa Verified Revenue.

## 7.6. Verified Revenue / Commission / ROAS

Verified Revenue chỉ có sau ORDER_VERIFIED hoặc equivalent verified checkpoint.

No ORDER_VERIFIED thì không final commission.

No ORDER_VERIFIED thì không verified ROAS.

No ORDER_VERIFIED thì không payout.

Ads chỉ consume verified revenue.

Commission chỉ final sau verified revenue và policy owner.

Payout không executed nếu Finance/Reward chưa duyệt.

Revenue reversal phải append record, không xóa record gốc.

## 7.7. Gateway Lock

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Documentation Complete chưa phải Production Readiness.

Không được gọi Completion Decision.

Không được gọi Release Readiness.

Không được gọi Production Readiness.

Không được gọi Go-live Decision.

## 8. FILE INDEX CHI TIẾT

## 8.1. 00-P3-ANALYSIS-ONLY.md

Mode: ANALYSIS ONLY
Mục tiêu: Phân tích codebase thật, không sửa file.

Scope chính

Kiểm tra hiện trạng Commerce Runtime.

Map current implementation.

Tìm gap.

Tìm conflict.

Tìm điểm chặn.

Xác định evidence cần có.

Đề xuất smoke, chưa viết test.

Trả report 14 mục.

Done Gate

Không sửa file.

Có current state matrix.

Có gap matrix.

Có conflict matrix.

Có điểm chặn list.

Có evidence required.

Có proposed smoke.

Report đủ 14 mục.

Không được

Không sửa code.

Không tạo migration.

Không tạo API.

Không tạo test.

Không mở Gateway.

## 8.2. 01-P3-1-TECHNICAL-DESIGN-ONLY.md

Mode: TECHNICAL DESIGN ONLY
Mục tiêu: Thiết kế workstream, owner boundary, API/DTO/entity/service/guard/test/evidence.

Scope chính

Thiết kế workstream P3.2A đến P3.2G.

Thiết kế boundary.

Thiết kế domain object.

Thiết kế service/resolver.

Thiết kế API contract.

Thiết kế DB/migration proposal.

Thiết kế smoke/evidence plan.

Trả report 14 mục.

Done Gate

Không sửa file.

Có design đủ P3.2A -> P3.2G.

Có owner boundary.

Có allowed file change boundary cho implementation.

Report đủ 14 mục.

Không được

Không code.

Không migration.

Không seed.

Không test.

Không mở Gateway.

## 8.3. 02-P3-2A-SELLABLE-GATE-RUNTIME.md

Mode: LIMITED IMPLEMENTATION
Mục tiêu: Triển khai Sellable Gate Runtime.

Scope chính

SellableDecision.

SellableGateResolver.

Sellable guard chain.

Block reason.

Runtime unavailable fail-safe.

Public/internal response boundary.

Smoke P3.2A.

Done Gate

Sellable không dùng Product/SKU/Recipe Active thay thế.

Listed price active được kiểm tra.

Released stock được kiểm tra.

Warehouse receipt confirmed được kiểm tra.

Recall/Sale Lock chặn đúng.

Runtime unavailable fail-safe.

Có block reason.

Test/smoke/evidence đầy đủ.

Không được

Không QuoteSnapshot.

Không Cart.

Không Order.

Không Payment.

Không Verified Revenue.

Không Gateway.

## 8.4. 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md

Mode: LIMITED IMPLEMENTATION
Mục tiêu: Triển khai giá/runtime benefit/QuoteSnapshot.

Scope chính

ListedPriceResolver.

ProgramEligibilityResolver.

MemberBenefitResolver.

DiamondReferralResolver.

QuoteSnapshotService.

QuoteSnapshot immutable.

Quote expiry/freeze window.

No hardcode price/discount/member benefit.

Done Gate

Không có QuoteSnapshot thì không final price.

QuoteSnapshot immutable.

Program/member/diamond snapshot đúng.

Shipping/VAT nếu có phải từ runtime/policy.

AI/Gateway không tự tính giá.

Test/smoke/evidence đầy đủ.

Không được

Không Cart.

Không Order Draft.

Không Official Order.

Không Payment.

Không Verified Revenue.

Không Commission final.

## 8.5. 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md

Mode: LIMITED IMPLEMENTATION
Mục tiêu: Triển khai Cart / Order Draft / Customer Confirmation.

Scope chính

Cart.

CartLine.

OrderDraft.

OrderDraftLine.

CustomerConfirmation.

QuoteSnapshot validation.

Duplicate confirmation guard.

handoff_ready_for_official_order.

Done Gate

Cart không phải Order.

Order Draft không phải Official Order.

Order Draft gắn QuoteSnapshot active.

Expired quote bị reject.

Customer Confirmation idempotent.

Accepted confirmation không tạo Official Order.

Không order_code trong P3.2C.

Không được

Không Official Order.

Không order_code.

Không Paid.

Không Shipping Request.

Không Verified Revenue.

## 8.6. 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md

Mode: LIMITED IMPLEMENTATION
Mục tiêu: Tạo Official Order từ Customer Confirmation accepted.

Scope chính

OfficialOrder.

OfficialOrderLine.

order_code.

OrderCodeGenerator.

OrderState.

OrderStateTransition.

Duplicate order guard.

Idempotency.

State machine.

Done Gate

Official Order chỉ tạo từ confirmation accepted.

order_code unique/stable.

Retry không tạo duplicate.

Concurrent duplicate bị chặn.

Order created không paid.

Order created không verified revenue.

State transition append-only.

Evidence đầy đủ.

Không được

Không Payment confirmation.

Không Shipping Request thật.

Không Invoice thật.

Không Verified Revenue.

Không commission/ROAS/payout.

## 8.7. 06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md

Mode: LIMITED IMPLEMENTATION
Mục tiêu: Triển khai Payment / Shipping / Invoice / Tax.

Scope chính

PaymentSelection.

PaymentConfirmation.

COD handling.

Bank transfer handling.

ShippingQuote.

ShippingRequest nếu được duyệt.

InvoiceRequest.

TaxQuote / TaxSnapshot.

Payment Core confirmation.

Done Gate

Payment selected không phải Paid.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới paid.

COD không tự thêm phí nếu policy chưa khóa.

Shipping fee không hardcode.

VAT/tax không hardcode.

Paid/Delivered/Invoice không tạo Verified Revenue.

Không commission/ROAS/payout.

Không được

Không ORDER_VERIFIED nếu thuộc P3.2F.

Không Verified Revenue.

Không commission final.

Không verified ROAS.

Không payout.

## 8.8. 07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md

Mode: LIMITED IMPLEMENTATION
Mục tiêu: Triển khai ORDER_VERIFIED / Verified Revenue / Commission / ROAS Handoff.

Scope chính

RevenueVerificationCheckpoint.

## ORDER_VERIFIED.

VerifiedRevenueRecord.

CommissionEligibilitySnapshot.

ROASHandoffRecord.

PayoutEligibilityRecord nếu duyệt ở mức handoff.

RevenueReversalRecord nếu duyệt.

Done Gate

Paid không tạo Verified Revenue.

Delivered không tạo Verified Revenue.

Invoice không tạo Verified Revenue.

Verified Revenue chỉ sau ORDER_VERIFIED.

No ORDER_VERIFIED thì reject revenue.

Commission không final nếu chưa Verified Revenue.

ROAS không dùng unverified value.

Payout không executed nếu chưa duyệt.

Evidence đầy đủ.

Không được

Không Gateway production config.

Không payout thật nếu chưa Finance/Reward duyệt.

Không Ads optimization engine.

Không Release Readiness.

Không Production Readiness.

## 8.9. 08-P3-2G-SMOKE-EVIDENCE-REPORT.md

Mode: VALIDATION / SMOKE / EVIDENCE REPORT ONLY
Mục tiêu: Chạy smoke tổng hợp, gom evidence, lập report.

Scope chính

Build validation.

Test validation.

Smoke validation.

Migration validation.

Seed validation.

RBAC validation.

Audit validation.

Idempotency validation.

Runtime unavailable validation.

Public/internal response validation.

Evidence package.

Handoff sang Owner Review.

Done Gate

Không thêm feature.

Không sửa code ngoài report/evidence boundary.

Build/test/smoke rõ trạng thái.

Evidence package submitted.

P0 điểm chặn không còn hoặc bị chặn rõ.

Gateway vẫn bị chặn.

Không gọi Evidence Accepted.

Không gọi Global Smoke Pass.

Không gọi Release Readiness/Production Readiness.

Không được

Không chỉnh sửa tạm lỗi trong smoke report.

Không sửa seed để pass giả.

Không tạo migration chỉnh sửa tạm nóng.

Không mở Gateway.

Không gọi Go-live Decision.

## 9. REPORT 14 MỤC BẮT BUỘC

Mọi file execution report trong PHASE 3 đều phải có đúng 14 mục:

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

Nếu report thiếu 14 mục, không được chuyển phase.

## 10. điểm chặn P0 TOÀN PHASE 3

Các lỗi sau là P0 điểm chặn:

Product Active được dùng thay Sellable.

SKU Active được dùng thay Sellable.

Recipe Active được dùng thay Sellable.

Warehouse Receipt tự set Sellable.

Final price trả ra không có QuoteSnapshot.

QuoteSnapshot mutable.

AI/Gateway hardcode giá/discount/member benefit/shipping/VAT.

Cart bị hiểu là Order.

Order Draft bị hiểu là Official Order.

Customer Confirmation bị bỏ qua.

Official Order không có order_code.

order_code duplicate.

Payment selected bị set Paid.

COD selected bị set Paid.

Bank transfer selected bị set Paid.

Paid tạo Verified Revenue trực tiếp.

Delivered tạo Verified Revenue trực tiếp.

Invoice tạo Verified Revenue trực tiếp.

Verified Revenue tạo khi chưa ORDER_VERIFIED.

Commission final trước Verified Revenue.

ROAS dùng unverified value.

Payout executed ngoài boundary.

Evidence Submitted bị gọi là Evidence Accepted.

Smoke cục bộ bị gọi là Global Smoke Pass.

Gateway bị mở.

Release Readiness bị gọi khi chưa owner review.

Production Readiness bị gọi khi chưa release decision.

## 11. EVIDENCE PACKAGE TỐI THIỂU

PHASE 3 cần gom evidence package gồm:

P3 analysis report.

P3 technical design report.

P3.2A Sellable evidence.

P3.2B QuoteSnapshot evidence.

P3.2C Cart/Draft/Confirmation evidence.

P3.2D Official Order/order_code evidence.

P3.2E Payment/Shipping/Tax evidence.

P3.2F Verified Revenue/Commission/ROAS evidence.

P3.2G smoke matrix.

Backend build logs.

Frontend build logs nếu applicable.

Unit test logs.

Integration test logs.

Smoke logs.

Migration validation logs.

Seed validation logs.

RBAC evidence.

Audit evidence.

Idempotency evidence.

Runtime unavailable evidence.

Public/internal response evidence.

Gateway bị chặn evidence.

Owner review checklist.

Evidence package chỉ được gọi là:

## EVIDENCE_SUBMITTED

Không được gọi là:

## EVIDENCE_ACCEPTED

cho đến khi owner/reviewer xác nhận.

## 12. DEV EXECUTION PROTOCOL

Khi giao dev/Codex/Copilot, bắt buộc chạy theo protocol sau:

Step 1 - Đọc README này

Dev phải đọc file README này để hiểu thứ tự và gate.

Step 2 - Chạy 00-P3-ANALYSIS-ONLY

Chỉ phân tích.

Không sửa file.

Step 3 - Chạy 01-P3-1-TECHNICAL-DESIGN-ONLY

Chỉ thiết kế.

Không sửa file.

Step 4 - Owner duyệt boundary

Không có owner approval thì không được implementation.

Step 5 - Chạy từng prompt implementation

Chạy tuần tự:

## P3.2A.

## P3.2B.

## P3.2C.

## P3.2D.

## P3.2E.

## P3.2F.

Mỗi prompt phải có report 14 mục.

Step 6 - Chạy P3.2G

Chỉ smoke/evidence/report.

Không sửa feature.

Step 7 - Owner Review

Sau P3.2G mới sang Owner Review.

Step 8 - Release decision riêng

Release decision không nằm trong PHASE 3 implementation.

## 13. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối dưới đây để giao dev/Codex/Copilot khi bắt đầu PHASE 3.

## PHASE-03 COMMERCE RUNTIME - DEV EXECUTION HANDOFF

Bạn đang thực hiện PHASE-03 - Commerce Runtime cho dự án Ginsengfood.

Không được hiểu nhiệm vụ này là copy-paste code từ AI.

Bạn phải inspect codebase thật, database thật, API thật, migration thật, seed thật, test thật và triển khai theo architecture thật.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Documentation Complete chưa phải Production Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision nếu chưa có accepted evidence, smoke, owner review và release decision.

## EXECUTION ORDER

Chạy đúng thứ tự:

## 00-P3-ANALYSIS-ONLY.md

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md

## 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md

## 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md

## 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md

## 06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md

## 07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md

## 08-P3-2G-SMOKE-EVIDENCE-REPORT.md

Không được nhảy phase.

Không được bỏ qua Analysis/Design.

Không được implementation nếu chưa có approved boundary.

## CORE RULES

Sellable Gate là runtime gate riêng.

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Không có QuoteSnapshot thì không có final price.

Cart không đồng nghĩa Order.

Order Draft không đồng nghĩa Official Order.

Customer Confirmation mới cho phép tạo Official Order.

Không có order_code thì không được nói đơn hàng đã ghi nhận chính thức.

Payment selected không đồng nghĩa Paid.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới là Paid.

Paid không đồng nghĩa Verified Revenue.

Delivered không đồng nghĩa Verified Revenue.

Invoice không đồng nghĩa Verified Revenue.

No ORDER_VERIFIED thì không final commission.

No ORDER_VERIFIED thì không verified ROAS.

No ORDER_VERIFIED thì không payout.

AI Advisor, Gateway, Ads, Live, CRM, IVR chỉ consume runtime, không tự tính giá/tồn/discount/sellable/revenue.

Recall/Sale Lock chặn Commerce và toàn bộ downstream channel.

## REPORT FORMAT

Mọi report phải có 14 mục:

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

## FINAL RULE

Nếu phát hiện điểm chặn, không chỉnh sửa tạm lén, không bỏ qua, không sửa seed/test để xanh giả.

Phải báo điểm chặn và quay lại đúng prompt liên quan.

## 14. DOWNSTREAM SAU PHASE 3

Sau PHASE 3, không tự động sang production.

Thứ tự đúng là:

Owner Review.

Evidence Acceptance.

Security Review nếu áp dụng.

Finance/Reward Review nếu liên quan commission/payout.

Ads/ROAS Review nếu liên quan revenue feed.

Gateway Decision.

Release Decision.

Sau đó mới có thể bàn đến PHASE 4.

## 15. PHASE TIẾP THEO ĐỀ XUẤT

Sau khi PHASE 3 được owner review, phase tiếp theo hợp lý là:

## PHASE-04 - AI ADVISOR / CHANNEL GATEWAY RUNTIME CONSUMER

PHASE 4 sẽ khóa cách các hệ thống bên ngoài consume Commerce Runtime:

AI Advisor.

Facebook Gateway.

Live comment -> Messenger.

## CRM.

Ads/ROAS consumer.

IVR reserved.

Public/private response.

Customer Memory.

Runtime variable binding.

No self-calculation policy.

PHASE 4 chỉ được bắt đầu khi PHASE 3 đã có:

P3.2G report.

Evidence submitted.

Owner review.

Các P0 điểm chặn đã xử lý hoặc được phân loại rõ.

Gateway vẫn bị chặn cho đến khi có quyết định riêng.

NEXT ACTION: Giao dev/Codex/Copilot đọc README này, sau đó bắt đầu từ 00-P3-ANALYSIS-ONLY.md.

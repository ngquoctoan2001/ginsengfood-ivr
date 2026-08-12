# P3.2G - SMOKE EVIDENCE REPORT

## Nguon Bat Buoc

File nay duoc rewrite cho PHASE theo ranh gioi MASTER/PACK/TECH hien hanh.

- PACK-10 Evidence / Smoke / Completion Gateway.
- TECH-10 Global Smoke / UAT / Evidence.
- P3.2A den P3.2F reports va evidence.
- Recall / Sale Lock thang downstream.

## Noi Dung Rewrite

PHASE-03-COMMERCE-RUNTIME/08-P3-2G-SMOKE-EVIDENCE-REPORT.md

## 28.2. Prompt tiếp theo

## PROMPT-P3.2G - COMMERCE RUNTIME SMOKE / EVIDENCE / RELEASE-GATE REPORT ONLY

## 28.3. Điều kiện sang P3.2G

Chỉ chuyển sang P3.2G nếu:

P3.2F đạt DONE GATE.

Paid không tự tạo Verified Revenue.

Delivered không tự tạo Verified Revenue.

Invoice không tự tạo Verified Revenue.

ORDER_VERIFIED checkpoint đúng.

Verified Revenue chỉ sau ORDER_VERIFIED.

Commission/ROAS/Payout guard đúng.

Evidence submitted đầy đủ.

Owner đồng ý handoff.

## 28.4. Không chuyển nếu

Verified Revenue còn dùng Paid làm nguồn trực tiếp.

Delivered tạo revenue trực tiếp.

Invoice tạo revenue trực tiếp.

Commission final trước verified revenue.

ROAS dùng unverified value.

Payout bị scope creep.

Gateway bị mở.

Report thiếu evidence.

## 29. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3.2F - VERIFIED REVENUE / COMMISSION / ROAS HANDOFF LIMITED IMPLEMENTATION

## MODE: LIMITED IMPLEMENTATION - VERIFIED REVENUE / COMMISSION / ROAS HANDOFF ONLY

Bạn đang thực hiện PHASE-03 - Commerce Runtime cho dự án Ginsengfood.

Nhiệm vụ duy nhất của bạn là triển khai hoặc chỉnh sửa ORDER_VERIFIED / Verified Revenue / Commission Handoff / ROAS Handoff.

Không được mở Gateway.
Không được gọi Release Readiness.
Không được gọi Production Readiness.
Không được payout thật nếu Finance/Reward chưa duyệt.
Không được dùng Paid/Delivered/Invoice làm Verified Revenue nếu chưa qua ORDER_VERIFIED hoặc equivalent checkpoint.

## SOURCE-OF-TRUTH

Bám theo:

MASTER Governance.

PHASE-00 Foundation/RBAC/Audit/Evidence/Idempotency.

PHASE-01 Product/SKU/Recipe/Activation.

PHASE-02 Operational Core.

## PHASE-03/00-P3-ANALYSIS-ONLY.

## PHASE-03/01-P3-1-TECHNICAL-DESIGN-ONLY.

## PHASE-03/02-P3-2A-SELLABLE-GATE-RUNTIME.

## PHASE-03/03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.

## PHASE-03/04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.

## PHASE-03/05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.

## PHASE-03/06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## CORE RULES

Paid không đồng nghĩa Verified Revenue.

Delivered không đồng nghĩa Verified Revenue nếu chưa qua checkpoint.

Invoice issued/requested không đồng nghĩa Verified Revenue.

Verified Revenue chỉ có sau ORDER_VERIFIED hoặc equivalent verified checkpoint.

No ORDER_VERIFIED thì không final commission.

No ORDER_VERIFIED thì không verified ROAS.

No ORDER_VERIFIED thì không payout.

Ads chỉ consume verified revenue.

Commission/payout phải qua Finance/Reward policy.

Revenue reversal phải append record, không xóa record gốc.

Gateway vẫn bị chặn.

## SCOPE IN

Triển khai/chỉnh:

RevenueVerificationCheckpoint.

OrderVerificationService.

VerifiedRevenueRecord.

VerifiedRevenueService.

RevenueState.

RevenueReversalRecord nếu duyệt.

CommissionEligibilitySnapshot.

CommissionHandoffService.

ROASHandoffRecord.

ROASHandoffService.

PayoutEligibilityRecord nếu duyệt ở mức eligibility/handoff.

API/DTO nếu P3.1 duyệt.

Migration/seed nếu P3.1 duyệt.

Test/smoke/evidence cho P3.2F.

## SCOPE OUT

Không làm:

Gateway production config.

Release Readiness.

Production Readiness.

Payout execution thật nếu chưa duyệt.

Ads optimization engine.

Finance settlement thật nếu chưa duyệt.

MISA production sync ngoài boundary.

## IVR.

## ALLOWED FILE CHANGE BOUNDARY

Chỉ sửa file thuộc boundary đã được P3.1/P3.2E duyệt.

Nếu cần sửa ngoài boundary, dừng và báo:

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL

## EXECUTION STEPS

Xác nhận mode LIMITED IMPLEMENTATION - VERIFIED REVENUE / COMMISSION / ROAS HANDOFF ONLY.

Đọc P3.2E Done Report.

Xác định allowed file boundary.

Inspect code hiện tại về revenue/commission/ROAS/payout.

Implement Order Verification.

Implement Verified Revenue.

Implement Commission Handoff.

Implement ROAS Handoff.

Implement Revenue Reversal nếu approved.

Implement API/DTO nếu approved.

Implement migration/seed nếu approved.

Add/update test.

Run backend build/test.

Run frontend build nếu có ảnh hưởng UI.

Run smoke P3-SMK-2F.

Produce report 14 mục.

Handoff sang P3.2G nếu đạt DONE GATE.

## TEST / SMOKE REQUIRED

Bắt buộc kiểm tra:

Paid is not Verified Revenue.

Delivered is not Verified Revenue.

Invoice issued is not Verified Revenue.

ORDER_VERIFIED creates checkpoint.

Verified Revenue after ORDER_VERIFIED.

No ORDER_VERIFIED blocks revenue.

No Verified Revenue blocks commission final.

Valid referral after Verified Revenue creates commission eligibility/handoff.

Self-purchase blocks commission.

No Verified Revenue blocks ROAS.

Verified Revenue creates ROAS handoff.

Revenue reversal adjusts downstream nếu implemented.

Duplicate verified revenue bị chặn.

Public response safe.

Gateway remains bị chặn.

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

Chỉ sửa file trong allowed boundary.

Paid không tạo Verified Revenue.

Delivered không tạo Verified Revenue.

Invoice không tạo Verified Revenue.

Verified Revenue chỉ sau ORDER_VERIFIED.

No ORDER_VERIFIED thì reject revenue.

Commission không final nếu chưa Verified Revenue.

ROAS không dùng unverified value.

Payout không executed nếu chưa duyệt.

Test/smoke pass hoặc điểm chặn báo rõ.

Backend build pass hoặc điểm chặn báo rõ.

Evidence submitted.

Report đủ 14 mục.

Gateway vẫn bị chặn.

Không gọi Release Readiness/Production Readiness.

FAIL GATE

Fail nếu:

Sửa ngoài boundary.

Paid tạo Verified Revenue.

Delivered tạo Verified Revenue.

Invoice tạo Verified Revenue.

Revenue tạo khi chưa ORDER_VERIFIED.

Commission final trước Verified Revenue.

ROAS dùng unverified value.

Payout executed ngoài boundary.

Mở Gateway.

Không có report 14 mục.

Gọi Production Readiness/Go-live Decision.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_P3_2G_SMOKE_EVIDENCE_REPORT.

READY_WITH_điểm chặn_FOR_OWNER_DECISION.

## BLOCKED_NEED_P3_2E_PAYMENT_SHIPPING_INVOICE_TAX_DONE_GATE.

## BLOCKED_SCOPE_EXPANSION_REQUIRED_OWNER_APPROVAL.

## BLOCKED_TEST_OR_BUILD_FAILED.

## BLOCKED_VERIFIED_REVENUE_COMMISSION_ROAS_P0_RULE_FAILED.

NEXT FILE: PHASE-03-COMMERCE-RUNTIME/08-P3-2G-SMOKE-EVIDENCE-REPORT.md

ĐIỀU KIỆN CHUYỂN TIẾP: Chỉ chuyển sang P3.2G sau khi Verified Revenue / Commission / ROAS Handoff đạt DONE GATE, paid/delivered/invoice không tự tạo Verified Revenue, Verified Revenue chỉ sau ORDER_VERIFIED, commission/ROAS/payout bị chặn nếu chưa Verified Revenue, reversal path rõ nếu implemented, evidence submitted đầy đủ và owner đồng ý handoff.

PHASE-03-COMMERCE-RUNTIME/08-P3-2G-SMOKE-EVIDENCE-REPORT.md

## PROMPT-P3.2G - COMMERCE RUNTIME SMOKE / EVIDENCE / RELEASE-GATE REPORT ONLY

## 1. PHASE MARKER

## 2. HEADER

Tài liệu này là prompt tổng hợp kiểm thử, smoke, evidence và release-gate report cho toàn bộ PHASE-03 - Commerce Runtime.

P3.2G không phải prompt viết thêm feature.

P3.2G không phải prompt sửa code.

P3.2G không phải prompt mở Gateway.

P3.2G không phải prompt gọi Production Readiness.

P3.2G chỉ làm các việc sau:

Chạy build.

Chạy test.

Chạy smoke cục bộ.

Kiểm tra migration.

Kiểm tra seed.

Kiểm tra cleanup.

Gom evidence.

Đối chiếu Done Gate / Fail Gate.

Lập báo cáo 14 mục.

Kết luận phase đạt local smoke hay còn điểm chặn.

Handoff sang Owner Review.

P3.2G phải kiểm tra toàn bộ chuỗi Commerce Runtime:

Sellable Gate.

Price / Program / Member Benefit / Diamond Referral / QuoteSnapshot.

Cart / Order Draft / Customer Confirmation.

Official Order / order_code / State Machine.

Payment / Shipping / Invoice / Tax.

Verified Revenue / Commission / ROAS Handoff.

## 3. MODE

## MODE: VALIDATION / SMOKE / EVIDENCE REPORT ONLY - DO NOT ADD FEATURE

Trong mode này, dev/Codex/Copilot chỉ được phép:

Chạy test.

Chạy smoke.

Chạy build.

Chạy migration validation.

Chạy seed validation.

Kiểm tra git diff.

Kiểm tra file boundary.

Kiểm tra evidence.

Kiểm tra audit/log/outbox nếu có.

Lập report 14 mục.

Cập nhật report/handoff nếu P3.1 đã cho phép cập nhật tài liệu.

Không được:

Thêm feature.

Sửa business logic.

Sửa Sellable Gate.

Sửa QuoteSnapshot.

Sửa Cart/Order Draft.

Sửa Official Order.

Sửa Payment/Shipping.

Sửa Verified Revenue.

Sửa Commission/ROAS.

Tạo migration mới để chỉnh sửa tạm lỗi ngay trong P3.2G nếu chưa có owner approval.

Tạo seed mới để che lỗi.

Mở Gateway.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Nếu smoke/test phát hiện lỗi, phải báo điểm chặn và handoff lại đúng file liên quan:

Lỗi Sellable -> quay lại P3.2A.

Lỗi QuoteSnapshot -> quay lại P3.2B.

Lỗi Cart/Draft/Confirmation -> quay lại P3.2C.

Lỗi Official Order/order_code -> quay lại P3.2D.

Lỗi Payment/Shipping/Invoice/Tax -> quay lại P3.2E.

Lỗi Verified Revenue/Commission/ROAS -> quay lại P3.2F.

## 4. SOURCE-OF-TRUTH

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

High-risk Guard.

Runtime unavailable fail-safe.

Không mở Global Gate.

## 4.3. PHASE 1

Product Active không đồng nghĩa Sellable.

SKU Active không đồng nghĩa Sellable.

Recipe Active không đồng nghĩa Sellable.

Seed không được tự set sellable.

Product/SKU/Recipe/Formula/BOM chỉ là foundation.

## 4.4. PHASE 2

Batch QC_PASS không đồng nghĩa RELEASED.

Finished goods chỉ tăng tồn khi warehouse receipt confirmed.

Warehouse Receipt không tự set Sellable.

Recall/Sale Lock thắng Commerce, AI, Ads, Live, CRM, Gateway, IVR.

Operational smoke không đồng nghĩa Release Readiness.

## 4.5. PHASE 3 implementation files

## 00-P3-ANALYSIS-ONLY.md.

## 01-P3-1-TECHNICAL-DESIGN-ONLY.md.

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md.

## 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md.

## 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md.

## 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md.

## 06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md.

## 07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md.

## 5. ENTRY CONDITION

Chỉ được chạy P3.2G nếu đã có DONE report hoặc handoff report cho:

P3.2A - Sellable Gate Runtime.

P3.2B - Price / Program / Member Benefit / Diamond Referral / QuoteSnapshot.

P3.2C - Cart / Order Draft / Customer Confirmation.

P3.2D - Official Order / order_code / State Machine.

P3.2E - Payment / Shipping / Invoice / Tax.

P3.2F - Verified Revenue / Commission / ROAS Handoff.

Nếu thiếu một report, phải dừng và trả trạng thái:

## BLOCKED_NEED_PREVIOUS_PHASE_DONE_REPORT

Nếu có report nhưng chưa có owner cho phép chạy smoke tổng hợp, phải trả trạng thái:

## BLOCKED_NEED_OWNER_APPROVAL_FOR_AGGREGATE_SMOKE

## 6. OBJECTIVE

Mục tiêu của P3.2G là xác minh toàn bộ PHASE 3 ở mức local smoke/evidence:

Đảm bảo từng workstream đã được test.

Đảm bảo build backend pass.

Đảm bảo frontend build pass nếu có thay đổi UI/contract.

Đảm bảo migration không lỗi.

Đảm bảo seed không tạo trạng thái sai.

Đảm bảo không có scope creep.

Đảm bảo không gọi Release Readiness/Production Readiness.

Đảm bảo có evidence cho từng chuỗi nghiệp vụ.

Đảm bảo điểm chặn được phân loại rõ.

Đảm bảo handoff sang Owner Review đúng trạng thái.

## 7. CORE RULES

## 7.1. Smoke cục bộ không phải Global Smoke Pass

P3.2G có thể kết luận:

Local smoke pass.

Module smoke pass.

Phase smoke submitted.

Evidence submitted.

Nhưng không được kết luận:

Global Smoke Pass.

Completion Decision.

Release Readiness.

Production Readiness.

Go-live Decision.

## 7.2. Evidence Submitted chưa phải Evidence Accepted

Evidence do dev/Codex/Copilot nộp trong P3.2G chỉ là:

## EVIDENCE_SUBMITTED

Chỉ owner/reviewer có quyền chuyển sang:

## EVIDENCE_ACCEPTED

## 7.3. Dev Complete chưa phải Release Readiness

Ngay cả khi test/build/smoke pass, vẫn không được gọi Release Readiness nếu chưa có:

Evidence accepted.

Owner review.

Security review nếu áp dụng.

Release decision.

Gateway decision.

Production readiness decision.

## 7.4. Gateway vẫn bị chặn

P3.2G không được:

Enable Gateway.

Enable production channel.

Enable Ads production revenue feed.

Enable Live production order flow.

Enable CRM production campaign.

Enable IVR production confirmation.

Enable payout production.

Enable MISA production sync nếu chưa có release decision.

## 7.5. Không chỉnh sửa tạm lỗi trong report phase

Nếu phát hiện lỗi trong P3.2G:

Không chỉnh sửa tạm lén.

Không tạo migration chỉnh sửa tạm nóng.

Không sửa seed để test pass.

Không chỉnh test để xanh giả.

Không bỏ qua case fail.

Phải báo đúng điểm chặn và quay lại prompt implementation tương ứng.

## 8. SCOPE IN

P3.2G bao gồm:

Build validation.

Unit test validation.

Integration test validation.

Smoke validation.

Migration validation.

Seed validation.

Security/RBAC validation.

Audit validation.

Idempotency validation.

Runtime unavailable validation.

Public/internal response boundary validation.

Outbox/event validation nếu có.

Evidence registry validation.

File boundary validation.

No scope creep validation.

Report 14 mục.

Handoff sang Owner Review.

## 9. SCOPE OUT

P3.2G không bao gồm:

Không thêm feature.

Không sửa business rule.

Không tạo API mới.

Không tạo DTO mới.

Không tạo migration mới nếu chưa được owner duyệt.

Không tạo seed mới để né lỗi.

Không sửa frontend để chỉnh sửa tạm flow.

Không thay đổi gateway config.

Không mở Ads/ROAS production feed.

Không mở CRM/Live/IVR production flow.

Không sync MISA production nếu chưa có release decision.

Không payout thật.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

## 10. ALLOWED FILE CHANGE BOUNDARY

## 10.1. Mặc định

Trong P3.2G:

Không được sửa code.

Chỉ được tạo hoặc cập nhật tài liệu report/evidence nếu P3.1/P3.2G boundary cho phép.

## 10.2. Được phép nếu được duyệt

Nhóm file

Được sửa

Ghi chú

Smoke report markdown/doc

Có điều kiện

Chỉ ghi kết quả, không sửa code

Evidence manifest

Có điều kiện

Chỉ nộp evidence

Handoff note

Có điều kiện

Chỉ cập nhật trạng thái

Test output artifact

Có

Artifact sinh ra từ lệnh test/build

Coverage report

Có

Artifact sinh ra từ lệnh test

Build log

Có

Artifact evidence

Migration validation log

Có

Artifact evidence

## 10.3. Không được sửa

Nhóm file

Không được sửa

Lý do

Domain code

Không

P3.2G chỉ report

Service code

Không

Không chỉnh sửa tạm trong smoke

## API/DTO

Không

Không thêm feature

Migration source

Không

Không tạo migration mới

Seed source

Không

Không sửa seed để pass

Frontend code

Không

Không chỉnh sửa tạm UI trong report

Gateway config

Không

Production config

Không

Chưa release decision

Payout config

Không

Không payout thật

Nếu cần sửa code, trả trạng thái:

## BLOCKED_NEED_RETURN_TO_IMPLEMENTATION_PROMPT

## 11. REQUIRED INPUT

Dev/Codex/Copilot phải thu thập đầy đủ:

P3.2A report.

P3.2B report.

P3.2C report.

P3.2D report.

P3.2E report.

P3.2F report.

Git diff.

Build logs.

Test logs.

Smoke logs.

Migration logs.

Seed validation logs.

API response samples nếu có.

Audit/evidence samples nếu có.

Outbox/event samples nếu có.

RBAC test evidence nếu có.

Idempotency test evidence.

Negative test evidence.

Public/internal response evidence.

Gateway bị chặn evidence.

## 12. AGGREGATE SMOKE MATRIX

Dev/Codex/Copilot phải lập bảng tổng hợp smoke theo mẫu.

Smoke ID

Workstream

Case

Expected

Actual

Evidence Ref

Status

điểm chặn

## P3-SMK-2A-001

Sellable

Product active nhưng missing listed price

## NOT_SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2A-002

Sellable

SKU active nhưng no released stock

## NOT_SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2A-003

Sellable

Batch QC_PASS chưa RELEASED

## NOT_SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2A-004

Sellable

Warehouse receipt chưa confirmed

## NOT_SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2A-005

Sellable

Recall active

## NOT_SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2A-006

Sellable

Sale lock active

## NOT_SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2A-007

Sellable

Channel suppression active

## NOT_SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2A-008

Sellable

Runtime policy unavailable

## NOT_SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2A-009

Sellable

All conditions pass

## SELLABLE

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-001

Quote

No QuoteSnapshot

No final price

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-002

Quote

SKU not sellable

No QuoteSnapshot

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-003

Quote

Listed price missing

Reject final quote

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-004

Quote

Program eligible

Snapshot discount

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-005

Quote

Program conflict

Resolve priority

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-006

Quote

Member eligible

Snapshot benefit

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-007

Quote

Identity missing

No member inference

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-008

Quote

Diamond referral valid

Eligibility snapshot only

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-009

Quote

Quote expired

Cannot use downstream

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2B-010

Quote

Immutable snapshot

Cannot mutate

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2C-001

Cart/Draft

Cart created

No order_code

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2C-002

Cart/Draft

Not sellable cannot cart

Reject/block

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2C-003

Cart/Draft

Draft without QuoteSnapshot

Reject

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2C-004

Cart/Draft

Expired quote

Reject

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2C-005

Cart/Draft

Draft created

No Official Order

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2C-006

Confirmation

Valid confirmation

Accepted

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2C-007

Confirmation

Duplicate confirmation

No duplicate

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2C-008

Confirmation

Recall/Sale Lock before confirmation

Reject

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2D-001

Order

Confirmed draft

Official Order + order_code

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2D-002

Order

Unconfirmed draft

Reject

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2D-003

Order

Expired quote

Reject

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2D-004

Order

order_code generated once

Unique/stable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2D-005

Order

Idempotent retry

Same order

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2D-006

Order

Concurrent duplicate

One order only

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2D-007

Order

Official Order created

Not Paid

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2D-008

Order

Official Order created

Not Verified Revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-001

Payment

COD selected

Not Paid

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-002

Payment

Bank transfer selected

Not Paid

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-003

Payment

Valid payment confirmation

Paid

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-004

Payment

Payment selected

No revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-005

Payment

Paid

No Verified Revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-006

Payment

COD policy no fee

No COD fee auto-added

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-007

Shipping

Shipping fee

Runtime policy

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-008

Shipping

Shipping request

Does not set paid

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-009

Shipping

Delivered

No Verified Revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-010

Tax

Tax quote

Runtime policy

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2E-011

Invoice

Invoice request

No revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-001

Revenue

Paid

Not Verified Revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-002

Revenue

Delivered

Not Verified Revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-003

Revenue

Invoice issued

Not Verified Revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-004

Revenue

## ORDER_VERIFIED

Checkpoint created

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-005

Revenue

ORDER_VERIFIED exists

Verified Revenue created

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-006

Revenue

No ORDER_VERIFIED

Revenue bị chặn

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-007

Commission

No Verified Revenue

Commission final bị chặn

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-008

Commission

Valid referral + Verified Revenue

Eligibility/handoff only

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-009

Commission

Self-purchase

bị chặn

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-010

## ROAS

No Verified Revenue

ROAS bị chặn

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-011

## ROAS

Verified Revenue + attribution

ROAS handoff

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## P3-SMK-2F-012

Reversal

Revenue reversed

Adjust downstream

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## 13. BUILD VALIDATION MATRIX

Build item

Command

Expected

Actual

Evidence Ref

Status

điểm chặn

Backend restore

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Backend build

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Backend test

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Backend lint/format

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Pass hoặc documented

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Frontend install

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Pass nếu applicable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Frontend build

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Pass nếu applicable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Frontend test

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Pass nếu applicable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

OpenAPI generation

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Pass nếu applicable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Worker build

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Pass nếu applicable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Migration validation

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Seed validation

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## 14. MIGRATION VALIDATION MATRIX

Migration area

Expected

Actual

Evidence Ref

Status

điểm chặn

Sellable migration

No auto sellable true

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

QuoteSnapshot migration

Immutable snapshot support

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Cart/Draft migration

No official order/order_code

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Official Order migration

Unique order_code

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Payment migration

Selected != Paid

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Shipping migration

No revenue side effect

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Tax/Invoice migration

No revenue side effect

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Verified Revenue migration

Requires ORDER_VERIFIED

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Commission migration

No final without Verified Revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

ROAS migration

No unverified value

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Payout migration

No payout execution unless approved

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## 15. SEED VALIDATION MATRIX

Seed area

Forbidden behavior

Actual

Evidence Ref

Status

điểm chặn

Product/SKU seed

Auto sellable true

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Price seed

Hardcoded final price

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Program seed

Hardcoded channel discount outside policy

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Quote seed

Fake final quote without QuoteSnapshot

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Cart/Draft seed

Official order created early

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Order seed

Paid by default

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Payment seed

Payment selected set paid

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

COD seed

COD fee auto-added without policy

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Shipping seed

Delivered creates revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Invoice seed

Invoice creates revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Revenue seed

Verified revenue without ORDER_VERIFIED

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Commission seed

Final commission without verified revenue

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

ROAS seed

ROAS from unverified order value

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Payout seed

Payout executed

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Gateway seed/config

Gateway opened

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## 16. SECURITY / GOVERNANCE VALIDATION

P3.2G phải kiểm tra các guardrail sau.

## 16.1. RBAC validation

Kiểm tra backend permission cho:

Manage listed price.

Manage program policy.

View internal quote.

Admin-assisted confirmation.

Create official order if admin-assisted.

Confirm payment manually.

Create shipping request manually.

View invoice/tax internal data.

Verify order.

Create verified revenue.

Create commission/ROAS handoff.

Reverse revenue.

## 16.2. Audit validation

Audit append-only phải có cho:

Sellable decision nếu lưu decision.

QuoteSnapshot creation.

Order Draft creation.

Customer Confirmation.

Official Order creation.

order_code generation.

Order state transition.

Payment selection.

Payment confirmation.

Shipping state change.

Invoice request.

Tax quote.

## ORDER_VERIFIED.

Verified Revenue.

Commission handoff.

ROAS handoff.

Revenue reversal nếu có.

## 16.3. Idempotency validation

Idempotency phải có cho:

Create QuoteSnapshot.

Create Cart nếu side effect.

Create Order Draft.

Submit Customer Confirmation.

Create Official Order.

Payment Confirmation.

Shipping Request.

Invoice Request.

## ORDER_VERIFIED.

Create Verified Revenue.

Commission handoff.

ROAS handoff.

Revenue reversal nếu có.

## 16.4. Runtime unavailable validation

Runtime unavailable phải fail-safe tại:

Sellable Gate.

Price resolver.

Program resolver.

Member benefit resolver.

Diamond referral resolver.

QuoteSnapshot.

Order Draft confirmation.

Payment policy.

Shipping policy.

Tax policy.

Verified Revenue policy.

Commission/ROAS policy.

## 16.5. Public/internal response validation

Public response không được lộ:

Supplier internal data.

Internal QC notes.

Internal recall notes.

Cost/margin.

Internal policy id nếu không public.

Payment reconciliation notes.

Bank/provider secrets.

Tax/MISA internal mapping.

Commission amount nếu không có quyền.

ROAS/campaign internal data.

Payout status nội bộ.

Audit internal notes.

## 17. RELEASE-GATE VALIDATION

P3.2G phải xác định rõ từng gate.

Gate

Expected in P3.2G

Actual

Status

Note

Local build

Có thể PASS/FAIL

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không đồng nghĩa Release Readiness

Local unit test

Có thể PASS/FAIL

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không đồng nghĩa Global Smoke

Local integration test

Có thể PASS/FAIL

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không đồng nghĩa Production Readiness

Local smoke

Có thể PASS/FAIL

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không đồng nghĩa Global Smoke Pass

Evidence submitted

Có thể SUBMITTED

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Chưa phải ACCEPTED

Evidence accepted

Không tự set

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Owner/reviewer quyết định

Owner review

## WAITING

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Bắt buộc

Security review

WAITING nếu applicable

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Bắt buộc nếu có sensitive flow

Release decision

## WAITING

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không tự gọi

Gateway status

bị chặn

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không mở trong P3.2G

Production Readiness

KHONG

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không gọi

Go-live Decision

KHONG

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

## CHỜ DEV ĐIỀN BẰNG EVIDENCE THẬT KHI CHẠY

Không gọi

## 18. EVIDENCE PACKAGE REQUIRED

P3.2G phải gom evidence package gồm:

## P3_EVIDENCE_INDEX.

Git diff summary.

Build logs.

Test logs.

Smoke logs.

Migration validation logs.

Seed validation logs.

API response samples nếu có.

Audit log samples.

Idempotency samples.

RBAC samples.

Runtime unavailable samples.

Public/internal response boundary samples.

Outbox/event samples nếu có.

Gateway bị chặn evidence.

Known điểm chặn list.

Owner review checklist.

Handoff decision note.

Evidence package chỉ được đánh dấu:

## SUBMITTED_FOR_OWNER_REVIEW

Không được đánh dấu:

## ACCEPTED

trừ khi owner/reviewer thật đã xác nhận.

## 19. điểm chặn CLASSIFICATION

P3.2G phải phân loại điểm chặn theo cấp.

## 19.1. P0 điểm chặn

P0 là điểm chặn không được handoff release nếu còn tồn tại:

Sellable dùng Product/SKU/Recipe Active thay Sellable.

Quote trả final price không có QuoteSnapshot.

QuoteSnapshot mutable.

Cart bị hiểu là Order.

Order Draft bị hiểu là Official Order.

Customer Confirmation bị bỏ qua.

Official Order không có order_code.

order_code duplicate.

Payment selected set Paid.

COD selected set Paid.

Bank transfer selected set Paid.

Paid tạo Verified Revenue trực tiếp.

Delivered tạo Verified Revenue trực tiếp.

Invoice tạo Verified Revenue trực tiếp.

Verified Revenue không cần ORDER_VERIFIED.

Commission final trước Verified Revenue.

ROAS dùng unverified order value.

Payout executed ngoài boundary.

Gateway bị mở.

Evidence bị gọi nhầm là Accepted.

Local smoke bị gọi nhầm là Global Smoke Pass.

Dev Complete bị gọi nhầm là Release Readiness.

## 19.2. P1 Risk

P1 là rủi ro cần owner quyết định trước release:

Test coverage chưa đủ negative case.

Một số API chưa có RBAC granular.

Audit chưa đủ chi tiết nhưng không phá flow.

Some outbox retry chưa đủ.

Reversal flow chưa triển khai nhưng đã ghi deferred rõ.

Shipping/tax policy cần owner mở rộng rule.

Ads attribution thiếu một số mapping.

Finance payout boundary chưa khóa.

## 19.3. P2 Improvement

P2 là cải tiến sau:

Tối ưu performance.

Thêm dashboard viewer.

Thêm report filter.

Thêm admin UI tiện dụng.

Thêm advanced attribution.

Thêm analytics.

Thêm automation reconciliation.

## 20. EXECUTION STEPS

Step 1 - Confirm Mode

Xác nhận:

## MODE: VALIDATION / SMOKE / EVIDENCE REPORT ONLY - DO NOT ADD FEATURE

Step 2 - Verify Previous Reports

Đọc và xác nhận:

P3.2A report.

P3.2B report.

P3.2C report.

P3.2D report.

P3.2E report.

P3.2F report.

Nếu thiếu report, dừng.

Step 3 - Check Git Diff

Kiểm tra:

File đã sửa.

File ngoài boundary.

Feature scope creep.

Migration/seed unexpected changes.

Gateway config changes.

Production config changes.

Step 4 - Run Build

Chạy build backend/frontend/worker theo project convention.

Ghi đầy đủ command và result.

Step 5 - Run Tests

Chạy:

Unit tests.

Integration tests.

Regression tests.

Security/RBAC tests nếu có.

Idempotency tests nếu có.

Step 6 - Run Aggregate Smoke

Chạy smoke matrix từ P3.2A đến P3.2F.

Không bỏ case fail.

Step 7 - Validate Migration

Kiểm tra:

Migration apply.

Migration rollback nếu convention yêu cầu.

Constraint.

Index.

No destructive change.

No auto sellable/revenue/payout.

Step 8 - Validate Seed

Kiểm tra seed không vi phạm:

No auto sellable.

No final price without QuoteSnapshot.

No official order without confirmation.

No paid without confirmation.

No verified revenue without ORDER_VERIFIED.

No commission/ROAS/payout without verified revenue.

Step 9 - Validate Evidence

Kiểm tra evidence package đầy đủ.

Step 10 - Validate Gateway Status

Kiểm tra:

Gateway config vẫn bị chặn.

Production flags không bật.

Ads/CRM/Live/IVR production flow không mở.

Release status không bị set sai.

Step 11 - Produce 14-Item Report

Trả report đúng 14 mục.

Step 12 - Final Handoff

Kết luận một trong các trạng thái:

## READY_FOR_OWNER_REVIEW_WITH_EVIDENCE_SUBMITTED.

## READY_WITH_P1_RISKS_FOR_OWNER_DECISION.

## BLOCKED_P0_SMOKE_FAILED.

## BLOCKED_BUILD_FAILED.

## BLOCKED_TEST_FAILED.

## BLOCKED_MIGRATION_FAILED.

## BLOCKED_SEED_VALIDATION_FAILED.

## BLOCKED_GATE_STATUS_VIOLATION.

## BLOCKED_SCOPE_CREEP_DETECTED.

## 21. REQUIRED REPORT FORMAT - 14 MỤC

## 21.1. Tóm tắt

Ghi rõ:

Đã chạy validation phạm vi nào.

Build pass/fail.

Test pass/fail.

Smoke pass/fail.

Evidence submitted chưa.

Có P0 điểm chặn không.

Gateway status.

Có được sang Owner Review không.

## 21.2. File đã sửa

Trong P3.2G, nếu không sửa code phải ghi:

Không sửa code. MODE: VALIDATION / SMOKE / EVIDENCE REPORT ONLY.

Nếu có report/evidence file được tạo, liệt kê riêng.

Nếu có code file bị sửa, phải báo FAIL.

## 21.3. Nguồn yêu cầu

Liệt kê:

MASTER Governance.

## PHASE 0.

## PHASE 1.

## PHASE 2.

P3 Analysis.

P3.1 Technical Design.

## P3.2A.

## P3.2B.

## P3.2C.

## P3.2D.

## P3.2E.

## P3.2F.

Prompt P3.2G.

## 21.4. Evidence đã dùng

Liệt kê:

Git diff.

Build logs.

Test logs.

Smoke logs.

Migration logs.

Seed logs.

API responses.

Audit samples.

Idempotency samples.

RBAC samples.

Runtime unavailable samples.

Public/internal response samples.

Gateway bị chặn evidence.

## 21.5. Lệnh đã chạy

Ghi đầy đủ:

Backend build command.

Backend test command.

Frontend build command nếu có.

Frontend test command nếu có.

Worker build/test command nếu có.

Migration command.

Seed validation command.

Smoke command.

Cleanup command nếu có.

## 21.6. Kết quả test

Ghi rõ:

Tổng số test.

Số pass.

Số fail.

Số skip.

Fail thuộc module nào.

Fail có phải P0 không.

Link/evidence log.

## 21.7. Kết quả backend build

Ghi:

Command.

Result.

Warning.

Error.

Evidence ref.

## 21.8. Kết quả frontend build

Ghi:

Có frontend affected không.

Command nếu có.

Result.

Warning/error.

Evidence ref.

## 21.9. Kết quả cleanup process

Ghi:

Có file tạm không.

Có process treo không.

Có log/artifact cần giữ không.

Có cleanup an toàn không.

Có xóa nhầm evidence không.

## 21.10. Cập nhật Markdown

Ghi:

Report nào đã cập nhật.

Evidence manifest nào đã cập nhật.

Handoff note nào đã cập nhật.

Không cập nhật code docs ngoài boundary.

## 21.11. Kết quả database migration/update nếu áp dụng

Ghi:

Migration apply result.

Migration rollback/down result nếu áp dụng.

Constraint validation.

Index validation.

No destructive change.

No auto sellable/revenue/payout.

Evidence ref.

## 21.12. Kết quả seed validation nếu áp dụng

Ghi:

Seed run result.

No auto sellable.

No quote without QuoteSnapshot.

No order without confirmation.

No paid without Payment Core confirmation.

No verified revenue without ORDER_VERIFIED.

No commission/ROAS/payout without verified revenue.

Evidence ref.

## 21.13. Rủi ro còn lại

Phân loại:

P0 điểm chặn.

P1 risk.

P2 improvement.

Owner decision needed.

Security review needed.

Finance/Reward decision needed.

Ads/ROAS decision needed.

Gateway release decision needed.

## 21.14. Cập nhật handoff

Ghi rõ:

Trạng thái cuối.

Có được sang Owner Review không.

Evidence submitted ở đâu.

Gateway vẫn bị chặn hay không.

Điều kiện để owner chuyển Evidence Accepted.

Điều kiện để future release decision.

Nếu bị chặn, quay lại file nào.

## 22. DONE GATE

P3.2G được DONE khi đủ:

Không thêm feature.

Không sửa code ngoài report/evidence boundary.

Đã kiểm tra P3.2A -> P3.2F.

Backend build pass hoặc điểm chặn rõ.

Frontend build pass nếu applicable hoặc điểm chặn rõ.

Tests pass hoặc điểm chặn rõ.

Smoke matrix đầy đủ.

Migration validation đầy đủ.

Seed validation đầy đủ.

RBAC/audit/idempotency validation đầy đủ.

Runtime unavailable validation đầy đủ.

Public/internal response validation đầy đủ.

Evidence package submitted.

P0 điểm chặn = none hoặc ghi bị chặn rõ.

Không gọi Evidence Accepted.

Không gọi Global Smoke Pass.

Không gọi Completion Decision.

Không gọi Release Readiness.

Không gọi Production Readiness.

Không gọi Go-live Decision.

Report đủ 14 mục.

Handoff sang Owner Review rõ.

## 23. FAIL GATE

P3.2G bị FAIL nếu:

Sửa code trong phase report.

Thêm feature mới.

Bỏ qua smoke fail.

Xóa test fail để pass.

Sửa seed để che lỗi.

Tạo migration chỉnh sửa tạm lỗi không được duyệt.

Không chạy build.

Không chạy test.

Không chạy smoke.

Không validate migration.

Không validate seed.

Không gom evidence.

Thiếu report 14 mục.

Gọi Evidence Accepted khi chưa owner review.

Gọi Global Smoke Pass khi chỉ smoke cục bộ.

Gọi Completion Decision.

Gọi Release Readiness.

Gọi Production Readiness.

Gọi Go-live Decision.

Gateway bị mở.

Ads/CRM/Live/IVR production flow bị mở.

Payout production bị mở.

MISA production sync bị mở ngoài boundary.

P0 điểm chặn tồn tại nhưng vẫn handoff release.

## 24. DOWNSTREAM HANDOFF

## 24.1. Next step

## OWNER REVIEW / ACCEPTED EVIDENCE / RELEASE DECISION

P3.2G không tự tạo file implementation tiếp theo.

Sau P3.2G, chỉ có thể chuyển sang:

Owner Review.

Evidence Acceptance Review.

Security Review nếu áp dụng.

Finance/Reward Review nếu áp dụng.

Ads/ROAS Review nếu áp dụng.

Release Decision.

Gateway Decision.

## 24.2. Điều kiện sang Owner Review

Chỉ sang Owner Review nếu:

P3.2G report đủ 14 mục.

Evidence package submitted.

P0 điểm chặn không còn hoặc ghi rõ bị chặn.

Smoke matrix đầy đủ.

Build/test/migration/seed status rõ.

Gateway vẫn bị chặn.

Không gọi Release Readiness.

## 24.3. Không được chuyển sang release nếu

Evidence chưa accepted.

Owner chưa review.

Security review chưa xong nếu applicable.

Gateway decision chưa có.

Có P0 điểm chặn.

Có smoke fail chưa xử lý.

Có build/test fail chưa xử lý.

Có migration/seed fail chưa xử lý.

Có payment/revenue/commission/ROAS guard fail.

Có Gateway opened violation.

## 24.4. Nếu bị chặn thì quay lại

điểm chặn

Quay lại file

Sellable điểm chặn

## 02-P3-2A-SELLABLE-GATE-RUNTIME.md

Quote điểm chặn

## 03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.md

Cart/Draft/Confirmation điểm chặn

## 04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.md

Official Order điểm chặn

## 05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.md

Payment/Shipping/Tax điểm chặn

## 06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.md

Revenue/Commission/ROAS điểm chặn

## 07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.md

Governance/Gateway điểm chặn

Owner Review / MASTER-09 governance

## 25. PROMPT COPY GIAO DEV / CODEX / COPILOT

Sao chép nguyên khối prompt dưới đây để giao dev/Codex/Copilot.

## PROMPT-P3.2G - COMMERCE RUNTIME SMOKE / EVIDENCE / RELEASE-GATE REPORT ONLY

## MODE: VALIDATION / SMOKE / EVIDENCE REPORT ONLY - DO NOT ADD FEATURE

Bạn đang thực hiện PHASE-03 - Commerce Runtime cho dự án Ginsengfood.

Nhiệm vụ duy nhất của bạn là chạy validation, smoke, build, test, migration validation, seed validation, gom evidence và viết report 14 mục.

Không được thêm feature.
Không được sửa business logic.
Không được tạo migration mới để chỉnh sửa tạm lỗi.
Không được sửa seed để che lỗi.
Không được mở Gateway.
Không được gọi Completion Decision.
Không được gọi Release Readiness.
Không được gọi Production Readiness.
Không được gọi Go-live Decision.

## SOURCE-OF-TRUTH

Bám theo:

MASTER Governance.

PHASE-00 Foundation/RBAC/Audit/Evidence/Idempotency.

PHASE-01 Product/SKU/Recipe/Activation.

PHASE-02 Operational Core.

## PHASE-03/00-P3-ANALYSIS-ONLY.

## PHASE-03/01-P3-1-TECHNICAL-DESIGN-ONLY.

## PHASE-03/02-P3-2A-SELLABLE-GATE-RUNTIME.

## PHASE-03/03-P3-2B-PRICE-PROGRAM-MEMBER-BENEFIT-QUOTE-SNAPSHOT.

## PHASE-03/04-P3-2C-CART-ORDER-DRAFT-CUSTOMER-CONFIRMATION.

## PHASE-03/05-P3-2D-OFFICIAL-ORDER-ORDER-CODE-STATE-MACHINE.

## PHASE-03/06-P3-2E-PAYMENT-SHIPPING-INVOICE-TAX.

## PHASE-03/07-P3-2F-VERIFIED-REVENUE-COMMISSION-ROAS-HANDOFF.

## GLOBAL LOCK

Evidence Submitted chưa phải Evidence Accepted.

Smoke cục bộ chưa phải Global Smoke Pass.

Documentation Complete chưa phải Production Readiness.

Dev Complete chưa phải Release Readiness.

Không được gọi Completion Decision / Release Readiness / Production Readiness / Go-live Decision.

## CORE VALIDATION RULES

Product/SKU/Recipe Active không được thay Sellable.

Không có QuoteSnapshot thì không final price.

Cart không phải Order.

Order Draft không phải Official Order.

Customer Confirmation mới cho phép tạo Official Order.

Không có order_code thì không được nói đơn chính thức.

Payment selected không phải Paid.

COD selected không paid.

Bank transfer selected không paid.

Payment Core confirmation mới paid.

Paid không phải Verified Revenue.

Delivered không phải Verified Revenue nếu chưa ORDER_VERIFIED.

Invoice không phải Verified Revenue.

No ORDER_VERIFIED thì không final commission.

No ORDER_VERIFIED thì không verified ROAS.

No ORDER_VERIFIED thì không payout.

Gateway vẫn bị chặn.

## SCOPE IN

Thực hiện:

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

Report 14 mục.

Handoff sang Owner Review.

## SCOPE OUT

Không làm:

Feature mới.

Code fix.

Business logic change.

Migration mới.

Seed chỉnh sửa tạm lỗi.

Gateway enable.

Release Readiness.

Production Readiness.

Go-live Decision.

## EXECUTION STEPS

Xác nhận mode VALIDATION / SMOKE / EVIDENCE REPORT ONLY.

Đọc P3.2A -> P3.2F reports.

Kiểm tra git diff và boundary.

Chạy backend build.

Chạy frontend build nếu applicable.

Chạy tests.

Chạy aggregate smoke.

Validate migration.

Validate seed.

Validate RBAC/audit/idempotency.

Validate public/internal response boundary.

Validate Gateway remains bị chặn.

Gom evidence package.

Viết report 14 mục.

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

Không thêm feature.

Không sửa code ngoài report/evidence boundary.

Build/test/smoke/migration/seed status rõ.

Smoke matrix đầy đủ.

Evidence package submitted.

P0 điểm chặn không còn hoặc bị chặn rõ.

Report đủ 14 mục.

Gateway vẫn bị chặn.

Không gọi Evidence Accepted.

Không gọi Global Smoke Pass.

Không gọi Release Readiness/Production Readiness.

FAIL GATE

Fail nếu:

Sửa code trong report phase.

Bỏ qua smoke fail.

Sửa seed/test để pass giả.

Thiếu evidence.

Thiếu report 14 mục.

Gọi Evidence Accepted.

Gọi Global Smoke Pass.

Mở Gateway.

Gọi Release Readiness/Production Readiness/Go-live Decision.

Có P0 điểm chặn nhưng vẫn handoff release.

## FINAL OUTPUT

Kết luận một trong các trạng thái:

## READY_FOR_OWNER_REVIEW_WITH_EVIDENCE_SUBMITTED.

## READY_WITH_P1_RISKS_FOR_OWNER_DECISION.

## BLOCKED_P0_SMOKE_FAILED.

## BLOCKED_BUILD_FAILED.

## BLOCKED_TEST_FAILED.

## BLOCKED_MIGRATION_FAILED.

## BLOCKED_SEED_VALIDATION_FAILED.

## BLOCKED_GATE_STATUS_VIOLATION.

## BLOCKED_SCOPE_CREEP_DETECTED.

## NEXT STEP: OWNER REVIEW / EVIDENCE ACCEPTANCE / RELEASE DECISION.

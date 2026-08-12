# 08 - KIỂM THỬ / BẰNG CHỨNG / BÁO CÁO GIAI ĐOẠN 3.1

## 1. Mục tiêu

File này khóa smoke, evidence và report standard cho toàn bộ Phase 3.1.

Nó không tự xác nhận release. Nó chỉ định nghĩa cách chứng minh implementation đúng.

## 2. Entry gate

Chỉ chạy sau khi các workstream 02 đến 07 có implementation report hoặc blocker rõ.

Nếu thiếu report, ghi:

`P3_1-SMOKE-BLOCKED-BY-MISSING-WORKSTREAM-REPORT`

## 3. Scope In

- Tổng hợp smoke.
- Tổng hợp evidence.
- Tổng hợp blocker.
- Kiểm tra source coverage.
- Kiểm tra no-conflict.
- Kiểm tra no-release-claim.
- Chuẩn hóa report 14 mục.
- Chuẩn bị owner review.

## 4. Scope Out

- Không sửa business logic.
- Không đổi policy.
- Không accepted evidence thay owner.
- Không gọi release-ready.
- Không mở Gateway.

## 5. Source coverage checklist

| Source | Bắt buộc chứng minh đã map vào file nào |
| --- | --- |
| Master Rule Lock V2.1 | File 06 + file tổng + file 01 |
| Supplementary Situation 076-096 | File 06 + file 07 |
| Bổ sung tin nhắn chăm sóc | File 04 + file 05 |
| Chính sách thành viên/hoa hồng | File 02 + file 03 |
| CRM 12-Month Lifecycle | File 05 |
| Phụ lục hệ thống tin nhắn | File 04 |

FAIL nếu còn source không map.

## 6. Smoke matrix tổng

| Area | Smoke ID | Scenario | Expected |
| --- | --- | --- | --- |
| Member | P3_1-MEM-001 | Silver đủ runtime | Benefit 5% nếu program allow. |
| Member | P3_1-MEM-002 | Hết chu kỳ chưa đạt 50% | Grace 60 ngày. |
| Member | P3_1-MEM-003 | Hết grace chưa đạt | Downgrade safe. |
| Program | P3_1-PRG-001 | 24/7 count 49 | Discount ngày sau 9%. |
| Program | P3_1-PRG-002 | 24/7 count 50 | Discount ngày sau 5%. |
| Golden Hour | P3_1-GH-001 | Stock 299 | Không mở phiên. |
| Golden Hour | P3_1-GH-002 | Stock 300 | Mở 1 phiên tối 20h00 - 20h45. |
| Golden Hour | P3_1-GH-003 | Diamond phiên 2 | Early access 19h30. |
| Quote | P3_1-QTE-001 | Thiếu QuoteSnapshot | Không báo final price. |
| Diamond | P3_1-DIA-001 | Verified 24/7 referral | Commission 15%. |
| Diamond | P3_1-DIA-002 | Verified Golden Hour referral | Commission 10%. |
| Diamond | P3_1-DIA-003 | Refund after commission | Hold/reverse. |
| Messaging | P3_1-MSG-001 | Gửi sai hạng | Suppress. |
| Messaging | P3_1-MSG-002 | Duplicate same month | Suppress. |
| Messaging | P3_1-MSG-003 | ORDER_SUCCESS with valid email | Email sent/required. |
| CRM | P3_1-CRM-001 | Trigger 22:00 | Reschedule. |
| CRM | P3_1-CRM-002 | Open case | Suppress sales. |
| CRM | P3_1-CRM-003 | Missing runtime member amount | Không bịa. |
| AI | P3_1-AI-001 | Khách cũ mua tiếp | Hỏi thăm trước khi bán. |
| AI | P3_1-AI-002 | Health-sensitive | Guard pass, no medical claim. |
| AI | P3_1-AI-003 | Vegan | Chỉ SKU vegan sellable. |
| AI | P3_1-AI-004 | Public price intent | Route private, no final quote public. |
| AI | P3_1-AI-005 | Troll/malicious | Hide/no reply/no Messenger nếu policy. |
| Payment | P3_1-PAY-001 | Bank transfer selected | Payment reference unique. |
| Payment | P3_1-PAY-002 | Proof image only | Not PAID. |
| Payment | P3_1-PAY-003 | Duplicate transaction | Block/audit. |
| Shipping | P3_1-SHP-001 | Tracking available | Return tracking state/link. |
| Shipping | P3_1-SHP-002 | Missing tracking miền Bắc | Fallback 5-7 ngày. |
| IVR | P3_1-IVR-001 | New high-risk COD | IVR required, no official order_code before pass. |
| IVR | P3_1-IVR-002 | IVR timeout Golden Hour | Release quota. |
| Privacy | P3_1-PRI-001 | Opt-out | Stop CRM, log suppression. |
| Refund | P3_1-RFD-001 | Product issue | Open case, no sales. |
| Fake goods | P3_1-FKG-001 | QR/source unknown | Trace/CSKH/legal, no unsupported conclusion. |

## 7. Evidence checklist

Mỗi smoke phải có:

- Source requirement.
- Workstream.
- Environment.
- Input.
- Expected.
- Actual.
- Pass/fail.
- File/log/test reference.
- Decision envelope hoặc audit ref nếu high-risk.
- PII/payment masking note.
- Reviewer nếu có.

## 8. Evidence package theo workstream

| Workstream | Evidence bắt buộc |
| --- | --- |
| 02 Member/price/program | MemberLifecycleSnapshot, ProgramPriceDecision, GoldenHourDecision, QuoteSnapshot. |
| 03 Diamond | ReferralSnapshot, BuyerBenefitDecision, VerifiedRevenueRef, CommissionDecision, Reversal/Dispute log. |
| 04 Messaging | Template registry, Trigger registry, DeliveryDecision, Dedup/Suppression/Email log. |
| 05 CRM lifecycle | CRMEventDecision, MemberLifecycleEvent, RecommendationDecision, OutcomeLog. |
| 06 AI Advisor | SituationRegistry, ResolverTrace, GuardTrace, FinalResponseEvidence, ProductRecommendationDecision. |
| 07 Payment/shipping/IVR | PaymentReference, AccountingReviewDecision, ShippingETAResult, IVRRequiredDecision, Order/Refund/VAT audit. |

## 9. Report 14 mục bắt buộc

Mỗi implementation report phải có:

1. Tóm tắt.
2. File đã sửa.
3. Nguồn yêu cầu.
4. Evidence đã dùng.
5. Lệnh đã chạy.
6. Kết quả test.
7. Kết quả backend build.
8. Kết quả frontend build.
9. Kết quả cleanup process.
10. Cập nhật Markdown.
11. Kết quả database migration/update nếu áp dụng.
12. Kết quả seed validation nếu áp dụng.
13. Rủi ro còn lại.
14. Cập nhật handoff.

Không đủ 14 mục thì report chưa đạt.

## 10. No-conflict checklist

Trước khi báo Phase 3.1 handoff complete phải kiểm:

- Không conflict với Phase 1 Product/SKU.
- Không conflict với Phase 2 Operational/sellable/recall/sale lock.
- Không conflict với Phase 3 Commerce owner truth.
- Không conflict với Phase 4 AI consumer boundary.
- Không conflict với Phase 5 Gateway delivery boundary.
- Không conflict với Phase 8 IVR order-confirmation-only.
- Không claim release.
- Không hardcode policy.

## 11. P0 fail gate

Phase 3.1 FAIL nếu:

- Source coverage thiếu.
- Smoke chỉ happy path.
- Evidence thiếu expected/actual.
- Payment/shipping/PII evidence không mask.
- P0 fail nhưng vẫn báo done.
- Build/test fail nhưng giấu.
- Report gọi Release Ready/Production Ready.
- Gateway mở.
- Owner decision missing nhưng status ghi complete.

## 12. Owner decision cần track

| ID | Decision |
| --- | --- |
| P3_1-OD-001 | Owner cuối cùng của Member Core/CRM Lifecycle Core. |
| P3_1-OD-002 | Policy priority member benefit vs Diamond buyer benefit. |
| P3_1-OD-003 | Commission reversal policy. |
| P3_1-OD-004 | Message template registry owner. |
| P3_1-OD-005 | Company Bank Account Registry/config owner và masking rule. |
| P3_1-OD-006 | IVR high-risk threshold. |
| P3_1-OD-007 | International shipping status. |
| P3_1-OD-008 | Product pillar canonical source cho 20 SKU. |

## 13. Final status template

```text
PHASE-03.1 STATUS
Documentation: COMPLETE | NEED_UPDATE
Implementation: NOT_STARTED | PARTIAL | COMPLETE_FOR_REVIEW
Smoke: NOT_RUN | PARTIAL | PASS_LOCAL | FAIL
Evidence: MISSING | COLLECTED | REVIEW_PENDING | ACCEPTED_BY_OWNER
P0 Blockers: NONE | OPEN
Owner Sign-off: MISSING | RECEIVED
Release State: NOT_RELEASE_READY
Gateway State: BLOCKED
```

## 14. Done gate

File 08 DONE khi:

- Smoke matrix phủ đủ workstream.
- Evidence checklist rõ.
- Report 14 mục rõ.
- Owner decisions list rõ.
- No-conflict checklist rõ.
- Không release claim.

## 15. Final status

`GIAI ĐOẠN 3.1 KIỂM THỬ/BẰNG CHỨNG/BÀN GIAO HOÀN TẤT - CHỜ TRIỂN KHAI/BẰNG CHỨNG/OWNER REVIEW - GATEWAY BLOCKED`

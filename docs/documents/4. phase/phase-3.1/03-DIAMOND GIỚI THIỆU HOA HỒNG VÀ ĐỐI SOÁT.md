# 03 - DIAMOND / GIỚI THIỆU / HOA HỒNG / ĐỐI SOÁT

## 1. Mục tiêu

File này khóa toàn bộ runtime Diamond cho PHASE-03.1:

- Điều kiện Diamond.
- Link giới thiệu.
- Buyer benefit.
- Commission 24/7 và Giờ Vàng.
- Commission reversal.
- Dispute escalation.
- Message triggers.
- Audit/evidence.

Đây là nguồn triển khai chính cho workstream P3.1-2B.

## 2. Source đã gom

| Source | Nội dung đã gom |
| --- | --- |
| `5. bo sung/4. chính sách và hoa hồng thành viên.md` | Diamond threshold, buyer benefit 24/7, early access, commission 15%/10%. |
| `5. bo sung/1. BẢN TỔNG HỢP SAU TEST THỰC CHIẾN + CORE POLICY + CRM MESSAGING + MEMBER LIFECYCLE.md` | Affiliate/Diamond tách đại lý/mua sỉ, runtime-first, member dispute escalation, outcome log. |
| `5. bo sung/3. bổ sung tin nhắn chăm sóc khác hàng.md` | DIAMOND_REFERRAL, DIAMOND_COMMISSION message. |
| `5. bo sung/6. PHỤ LỤC HỆ THỐNG TIN NHẮN.md` | Snapshot, trigger, variables, immutable text. |
| PHASE-03 | Verified Revenue, official order, payment/order state. |

## 3. Boundary

Diamond runtime không phải đại lý/nhà phân phối/mua sỉ.

| Intent khách | Route đúng |
| --- | --- |
| Hỏi làm Diamond/affiliate/nhà đồng hành khởi nghiệp | Diamond/member lifecycle flow. |
| Hỏi đại lý, nhà phân phối, mua sỉ, hợp tác số lượng lớn để kinh doanh | Route agency/distributor/bulk theo contact owner-approved, không tính Diamond commission. |
| Mua quà số lượng lớn nhưng không làm đại lý | Corporate gift/bulk quote flow, không ép đại lý. |
| Buyer mua qua link Diamond | Buyer benefit flow nếu link/bind valid. |

## 4. Điều kiện Diamond

Member là Diamond khi runtime xác nhận:

- Doanh số hợp lệ trong chu kỳ >= 8.000.000đ.
- Doanh số đến từ order hợp lệ.
- Member status còn active hoặc grace/policy cho phép.
- Không có dispute hoặc hold làm vô hiệu quyền lợi.

AI/CRM không được tự nói khách là Diamond nếu runtime chưa trả `member_tier = DIAMOND`.

## 5. Link giới thiệu Diamond

Link giới thiệu hợp lệ phải có:

- `diamond_referral_link_id`.
- Owner member id.
- Owner tier snapshot = Diamond tại thời điểm xét.
- Bind status hợp lệ.
- Policy version.
- Created/active status.
- Attribution source nếu có.

Không tính benefit hoặc commission khi:

- Link inactive.
- Bind invalid.
- Owner không phải Diamond.
- Link bị dispute/hold.
- Order không đi qua attribution hợp lệ.
- Order duplicate/test/fraud.

## 6. Buyer benefit qua link Diamond

### 6.1. 24/7

Buyer qua link Diamond hợp lệ được hưởng giảm 5% trên giá chương trình 24/7 nếu Runtime Core cho phép.

Điều kiện:

- Link hợp lệ.
- Bind hợp lệ.
- Buyer phát sinh order qua link.
- Program type = 24/7.
- QuoteSnapshot ghi nhận buyer benefit.
- Policy priority cho phép.

### 6.2. Giờ Vàng

Buyer qua link Diamond hợp lệ được quyền mua sớm 5 phút nếu:

- Phiên Giờ Vàng active.
- SKU thuộc phiên còn sellable/quota.
- Link/bind hợp lệ.
- Runtime xác nhận buyer early access eligible.

Buyer qua link Diamond không tự động nhận discount Giờ Vàng nếu policy không nêu.

## 7. Commission Diamond

| Program phát sinh order | Commission |
| --- | ---: |
| 24/7 | 15% |
| Giờ Vàng Tri Ân | 10% |

Commission chỉ ghi nhận khi:

- Order chính thức đã tạo qua Order Core.
- Order được xác nhận thành công.
- Payment/COD/verification đạt trạng thái hợp lệ.
- Order không hủy.
- Order không hoàn.
- Order không test.
- Order không duplicate.
- Order không phải self-purchase của chính Diamond owner hoặc tài khoản liên kết bị policy coi là self-purchase.
- Runtime xác nhận đủ điều kiện.
- Verified Revenue hoặc equivalent Commerce signal đã có.

## 8. Commission lifecycle

Đề xuất state:

| State | Ý nghĩa |
| --- | --- |
| `PENDING_VERIFICATION` | Có order qua link nhưng chưa đủ verified revenue. |
| `ELIGIBLE` | Đủ điều kiện tính commission. |
| `APPROVED` | Được duyệt theo policy payout. |
| `HELD` | Tạm giữ do refund/dispute/fraud/checkpoint. |
| `REJECTED` | Không đủ điều kiện. |
| `REVERSED` | Đã ghi nhận nhưng phải đảo do cancel/refund/reversal policy. |

Không được nhảy trực tiếp từ order draft sang commission approved.

Self-purchase phải trả `REJECTED` hoặc `HELD_FOR_REVIEW` theo `DIAMOND_SELF_PURCHASE_POLICY_V2026_06_CANONICAL_001`; không được tạo final payable hay MISA payout cho case này.

## 9. Reversal và dispute

### 9.1. Reversal

Commission phải được hold hoặc reversed nếu:

- Order bị cancel.
- Order refund toàn phần hoặc một phần theo policy.
- Order duplicate.
- Payment bị chargeback/reconciliation failed.
- Referral attribution bị dispute.
- Fraud signal.

### 9.2. Dispute

Mở case/human review khi:

- Member nói sai hạng.
- Sai doanh số tích lũy.
- Mất quyền lợi.
- Sai commission.
- Hạ hạng tranh chấp.
- Buyer không nhận benefit từ link.
- Link bind sai người.

AI/CRM không tranh luận với khách. Chỉ nói sẽ ghi nhận và chuyển kiểm tra.

## 10. Message triggers

### 10.1. DIAMOND_REFERRAL

Chỉ gửi khi:

- Member tier = Diamond.
- Có referral event hợp lệ.
- Trigger đúng `DIAMOND_REFERRAL`.
- Template registry active.
- Suppression pass.

Snapshot bắt buộc:

- diamond_snapshot.
- tier_snapshot.
- link_snapshot.
- policy_version_id.

### 10.2. DIAMOND_COMMISSION

Chỉ gửi khi:

- Commission được ghi nhận thành công.
- Trigger đúng `DIAMOND_COMMISSION`.
- Commission rate/amount từ runtime.
- Suppression pass.

Snapshot bắt buộc:

- diamond_snapshot.
- link_snapshot.
- commission_decision.
- policy_version_id.

## 11. Contract tối thiểu

| Contract | Fields |
| --- | --- |
| DiamondReferralSnapshot | referral_link_id, owner_member_id, owner_tier, bind_status, policy_version_id, active_status |
| DiamondBuyerBenefitDecision | buyer_customer_id, link_id, program_type, benefit_type, benefit_value, allow_apply, deny_reason |
| DiamondCommissionDecision | order_id, verified_revenue_id, member_id, rate, amount, status, reason_code, policy_version |
| DiamondCommissionLedger | ledger_id, commission_decision_id, debit_credit, amount, status, source_ref |
| DiamondDisputeCase | case_id, dispute_type, source_ref, customer_id, member_id, status, owner_review_required |

## 12. API/DB/UI impact

Phải review:

- Referral link registry.
- Referral bind table.
- Commission ledger.
- Commission reversal table/log.
- Member tier snapshot.
- Verified revenue reference.
- Admin commission review page.
- Member Diamond dashboard.
- Message trigger integration.

Không tự tạo UI nếu chưa có owner design. Nhưng nếu implementation có Admin UI, phải có audit và permission.

## 13. P0 blocker

- Commission tính từ quote/cart/order draft.
- Commission tính khi payment chưa verified.
- Commission không reverse khi refund/cancel.
- Gold/Platinum nhận Diamond commission.
- Link invalid vẫn tính benefit.
- Buyer benefit tự cộng với member benefit.
- Đại lý/mua sỉ bị xử như Diamond affiliate.
- Message Diamond gửi sai hạng.
- Commission amount public sai người.

## 14. Smoke tối thiểu

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P3_1-2B-001 | Diamond link tạo order 24/7 verified | Commission 15% eligible. |
| P3_1-2B-002 | Diamond link tạo order Golden Hour verified | Commission 10% eligible. |
| P3_1-2B-003 | Order refund sau commission | Commission hold/reverse theo policy. |
| P3_1-2B-004 | Owner link không còn Diamond | Không tính commission. |
| P3_1-2B-005 | Buyer vừa là member vừa qua Diamond link | PolicyPriorityResolver quyết định benefit. |
| P3_1-2B-006 | Khách hỏi đại lý/mua sỉ | Route distributor/bulk, không Diamond commission. |
| P3_1-2B-007 | Sai commission dispute | Open case/human review. |
| P3_1-2B-008 | Trigger Diamond gửi cho Gold | Suppress. |

## 15. Evidence bắt buộc

- DiamondReferralSnapshot.
- BuyerBenefitDecision.
- VerifiedRevenueRef.
- CommissionDecision.
- CommissionLedger/Reversal log nếu có.
- Dispute case audit nếu có.
- Message delivery decision nếu có gửi.
- Correlation id.
- Smoke output.

## 16. Done gate

Workstream 03 DONE khi:

- Diamond eligibility, referral, buyer benefit, commission, reversal, dispute đã rõ.
- Không còn nhầm Diamond với đại lý/mua sỉ.
- Commission phụ thuộc Verified Revenue.
- Message trigger đúng hạng và đúng suppression.
- Evidence đủ.
- Không release claim.

## 17. Final status

`P3.1-2B HOÀN TẤT BÀN GIAO - CHỜ TRIỂN KHAI/BẰNG CHỨNG - GATEWAY BLOCKED`

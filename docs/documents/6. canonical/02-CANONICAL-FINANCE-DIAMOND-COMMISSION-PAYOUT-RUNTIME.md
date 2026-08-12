# Canonical Finance / Diamond Commission / Payout Runtime

Ngày lập: 2026-06-05
Trạng thái: `CANONICAL_RUNTIME_LOCK_POLICY_SELECTED`
Production payout: `NOT_ALLOWED`

## 1. Mục đích

Tài liệu này tách rõ quyền lợi khách mua qua Diamond, điều kiện hoa hồng Diamond, verified revenue, finance payable checkpoint và MISA handoff. Đây là canonical thay cho việc dev đọc trực tiếp các bản gôm Diamond/hoa hồng phân tán hoặc file Phase 3.1 chưa đủ nội dung.

Policy refs đã chọn:

| Policy | Selected option |
| --- | --- |
| `DIAMOND_COMMISSION_POLICY_V2026_06_CANONICAL_001` | 24/7 = 15%, Golden Hour = 10%, chỉ eligible sau verified revenue + referral bind + policy pass. |
| `DIAMOND_SELF_PURCHASE_POLICY_V2026_06_CANONICAL_001` | Self-purchase không eligible commission. |
| `MISA_PAYOUT_POLICY_V2026_06_CANONICAL_001` | PIT withholding 10%; payout/MISA handoff chỉ sau Finance payable approval + MISA mapping active + evidence accepted. |

## 2. Boundary lock

| Domain | Được quyết định | Không được quyết định |
| --- | --- | --- |
| Commerce | QuoteSnapshot, buyer benefit, order/payment/verified revenue handoff | Payout thật, MISA sync thật |
| Diamond | Referral bind, eligibility snapshot, candidate commission | Final payable nếu chưa có Finance checkpoint |
| Finance | Payable approval, tax/PIT, reversal adjustment, payout batch | Tự sửa order/revenue source |
| MISA | Accounting handoff theo PACK-04 và owner mapping | Tự tạo policy hoa hồng |
| CRM | Message thông báo trạng thái public-safe | Không nói "đã thanh toán hoa hồng" nếu Finance chưa confirmed |

## 3. Diamond lifecycle

```yaml
DiamondReferralLifecycle:
  REFERRAL_LINK_CREATED:
    owner: Diamond
  BUYER_LINK_CLICKED:
    owner: Channel/Gateway attribution
  REFERRAL_BIND_CANDIDATE:
    owner: Diamond
  REFERRAL_BIND_CONFIRMED:
    owner: Diamond policy
  QUOTE_WITH_BUYER_BENEFIT:
    owner: Commerce QuoteSnapshot
  ORDER_VERIFIED:
    owner: Commerce/Payment checkpoint
  COMMISSION_ELIGIBILITY_CANDIDATE:
    owner: Diamond
  COMMISSION_ELIGIBLE:
    owner: Diamond + verified revenue + policy
  FINANCE_PAYABLE_REVIEW:
    owner: Finance
  PAYOUT_APPROVED:
    owner: Finance/Reward
  MISA_HANDOFF_READY:
    owner: Finance/MISA mapping
```

Không state nào được nhảy từ `REFERRAL_BIND_CANDIDATE` sang payout.

## 4. Buyer benefit vs commission

| Nội dung | Applies to | Source |
| --- | --- | --- |
| Buyer referral discount/benefit | Người mua qua link Diamond | QuoteSnapshot / Commerce |
| Diamond commission candidate | Diamond giới thiệu | Diamond eligibility snapshot |
| Commission eligible | Diamond sau verified revenue + policy pass | Diamond + Commerce + Finance pre-check |
| Payout approved | Finance review | Finance canonical / owner sign-off |

Buyer benefit không đồng nghĩa Diamond được hoa hồng. Commission candidate không đồng nghĩa payable.

## 5. Eligibility rules fail-closed

Commission phải `PENDING_REVIEW` hoặc `INELIGIBLE` nếu:

- Thiếu verified revenue.
- Thiếu referral bind confirmed.
- Thiếu policy version.
- Đơn bị cancel/refund/reversal.
- Self-purchase.
- Buyer/order/channel không eligible theo policy.
- Diamond account bị suspended/dispute.
- Thiếu tax/PIT/payable checkpoint nếu đã đến bước Finance.
- Thiếu MISA mapping khi cần official accounting handoff.

## 6. Finance / MISA handoff

```yaml
FinanceCommissionHandoff:
  commission_record_id: required
  source_order_id: required
  verified_revenue_record_id: required
  diamond_member_id: required
  policy_version: required
  commission_rate: required
  commission_amount: required
  reversal_status: required
  tax_policy_ref: DIAMOND_PIT_10_PERCENT
  finance_review_status: pending | approved | rejected | blocked
  misa_mapping_status: not_required | pending | active | blocked
  payout_status: not_started | pending_review | approved | paid | reversed
```

Không sync MISA thật nếu mapping chưa `active` hoặc Finance payable chưa `approved`. Không ghi production payout trong tài liệu nếu chưa có evidence thật.

## 7. Message wording lock

CRM/Gateway chỉ được nói public-safe:

| State | Được nói | Không được nói |
| --- | --- | --- |
| Candidate | "Đơn giới thiệu đang được kiểm tra theo chính sách." | "Hoa hồng đã ghi nhận." |
| Eligible | "Hoa hồng đủ điều kiện theo chính sách hiện hành." | "Đã chi trả." |
| Finance review | "Bộ phận phụ trách đang rà soát." | MISA, accounting, audit details. |
| Paid | Chỉ nói khi Finance payout confirmed. | Số liệu nội bộ ngoài biến approved. |
| Reversal | Nói mềm, có lý do public-safe nếu policy cho phép. | Đổ lỗi, tranh luận, lộ internal. |

## 8. P0 smoke bắt buộc

| Test ID | Scenario | Expected |
| --- | --- | --- |
| FIN-DIA-P0-001 | Referral bind thiếu verified order | No commission eligible. |
| FIN-DIA-P0-002 | Verified revenue có reversal | Commission blocked/adjusted. |
| FIN-DIA-P0-003 | Self-purchase | Ineligible commission, no payable. |
| FIN-DIA-P0-004 | Buyer benefit applied | QuoteSnapshot ghi buyer benefit, không tự tạo commission final. |
| FIN-DIA-P0-005 | Finance checkpoint missing | No payout approved. |
| FIN-DIA-P0-006 | MISA mapping pending | No official MISA sync. |
| FIN-DIA-P0-007 | CRM nói hoa hồng đã ghi nhận khi candidate | Language/state guard block. |
| FIN-DIA-P0-008 | Refund after eligible | Append reversal/adjustment, không xóa record gốc. |

## 9. Done gate

Finance/Diamond chỉ đủ handoff khi:

1. Buyer benefit, referral eligibility, commission eligibility và payout tách state.
2. Verified revenue là input bắt buộc.
3. Self-purchase, PIT/tax, payout schedule và MISA mapping dùng policy refs đã chọn ở tài liệu này.
4. CRM/Member message dùng wording theo state.
5. Evidence có order, revenue, referral, policy, finance review và reversal trace.
6. Không có production payout/MISA sync nếu Finance approval, mapping active hoặc evidence accepted còn thiếu.

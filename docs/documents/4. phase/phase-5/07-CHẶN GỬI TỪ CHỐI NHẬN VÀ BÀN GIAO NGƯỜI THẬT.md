# P5.2F - Suppression / Opt-out / Human Handoff

**Backlog:** `GW-BLG-009`  
**Mapped FB scope:** CRM suppression, human handoff, Ads/Diamond attribution guard  
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Mục tiêu

Đảm bảo Gateway tôn trọng consent, opt-out, quiet hours, open case, recall/sale lock, human handoff và attribution boundary. Gateway không được tự gửi CRM, không tự tính Diamond commission và không public Ads/ROAS/Diamond payout.

## 2. Suppression contract

```yaml
GatewaySuppressionDecision:
  customer_ref: required
  channel_identity: required
  consent_status: OPTED_IN | OPTED_OUT | UNKNOWN
  quiet_hour_status: IN_WINDOW | OUTSIDE_WINDOW
  message_fatigue_status: PASS | EXCEEDED
  open_case_status: NONE | QUALITY | COUNTERFEIT | PRIVACY | PAYMENT | DELIVERY
  recall_or_sale_lock_status: PASS | BLOCKED
  selected_action: SEND | SUPPRESS | HUMAN_HANDOFF | SAFE_ACK_ONLY
  suppression_reasons: list
  evidence_ref: required
```

Suppression thắng AI, Gateway, CRM, Ads, Live và IVR sales flow.

## 3. CRM outbound boundary

Gateway chỉ được enqueue hoặc deliver CRM command đã được CRM/AI runtime guard pass. Gateway không tự chọn lifecycle message.

CRM frequency locks:

- Sau mua: tối đa 1 tin/ngày/khách.
- Gợi ý mua lại/đổi vị: tối đa 1 tin/3 ngày/khách.
- Win-back: tối đa 1 tin/7 ngày/khách.
- Campaign: tối đa 1 campaign/ngày/khách, nếu có consent và policy cho phép.
- Quiet hour: 22h00-08h00.

CRM không gửi khi:

- Opt-out.
- Open quality/counterfeit/privacy/payment/delivery issue.
- Message fatigue exceeded.
- SKU recall/sale lock/not sellable.
- Thiếu Product Effectiveness nếu gợi ý sản phẩm.

## 4. Human handoff

Human bắt buộc khi:

- Complaint thật.
- Hàng giả/chất lượng/privacy/payment/delivery issue.
- Bệnh nền phức tạp, thai kỳ, trẻ nhỏ, dùng thuốc chống đông, suy thận/lọc thận, dị ứng nặng.
- Khách yêu cầu cam kết y khoa/thay thuốc/chữa bệnh.
- AI không đủ dữ liệu để lọc an toàn.
- Handoff/delivery/quote/order runtime fail mà không có safe fallback.

## 5. Ads/ROAS internal-only guard

Gateway được preserve attribution, không public:

- campaign_id.
- adset_id.
- ad_id.
- attribution_id.
- ROAS.
- CPA.
- Ads spend.

ROAS chỉ tính từ `ORDER_VERIFIED`. Quote, inbox, comment, order_created không là revenue.

## 6. Diamond/referral attribution guard

Gateway được preserve:

- referral_link_id.
- diamond_id.
- source_page_id.
- live_session_id.
- order_attribution_snapshot.

Gateway không được:

- Tự tính commission.
- Nói payout cho khách thường.
- Tính commission trước ORDER_VERIFIED.
- Bỏ qua self-purchase policy.
- Ghi duplicate commission ledger khi retry.

Commission baseline chỉ là business input:

- 24/7: 15%.
- Giờ Vàng Tri Ân: 10%.
- PIT: 10% trên hoa hồng.

Runtime/canonical Finance-Diamond mới là owner tính toán.

## 7. Acceptance criteria

- Opt-out user không nhận proactive message.
- Open case suppress sales/CRM.
- Quiet hour được enforce.
- Recall/sale lock chặn sales reply.
- Ads/ROAS chỉ internal.
- Diamond attribution preserve nhưng không tính payout ở Gateway.
- Human handoff có reason/evidence.

## 8. Smoke

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P5.2F-SMK-001 | User opt-out | No outbound. |
| P5.2F-SMK-002 | Quiet hour CRM | Suppressed/deferred. |
| P5.2F-SMK-003 | Open complaint | Human route, no sales. |
| P5.2F-SMK-004 | Ads-origin customer response | No campaign/ad/ROAS public. |
| P5.2F-SMK-005 | Diamond referral before verified order | No commission calculation. |

## 9. Final status

`LIMITED_IMPLEMENTATION_REPORT_ONLY`

`SUPPRESSION_AND_ATTRIBUTION_GUARD_REQUIRED`

## 10. SRS hardening addendum - Suppression, Opt-out, Human Handoff, Attribution

### 10.1. Suppression precedence

Suppression wins over delivery in this order:

1. Legal/privacy/platform block.
2. Human takeover active.
3. Customer opt-out or consent missing.
4. Open complaint/refund/payment/privacy case.
5. Sale lock/recall/quality hold.
6. App Review/permission/token unavailable.
7. Quiet hour/frequency cap/fatigue.
8. Duplicate/replay/rate-limit.
9. AI guard fail.
10. Owner policy unknown.

If any item returns `blocked|unknown` where evidence is required, Gateway must not send automated sales/customer-facing content.

### 10.2. Suppression decision contract

```yaml
SuppressionDecision:
  suppression_decision_id: required
  decision_id: required
  customer_channel_ref: required_redacted
  page_registry_id: required
  opt_out_status: opted_in|opted_out|unknown
  consent_status: valid|missing|expired|unknown
  quiet_hour_status: pass|defer|unknown
  frequency_cap_status: pass|blocked|unknown
  open_case_status: none|complaint|refund|payment|privacy|unknown
  sale_lock_status: pass|blocked|unknown
  recall_status: pass|blocked|unknown
  human_takeover_status: none|active|required
  final_status: pass|blocked|deferred|human_required
  blocked_reasons: list
  evidence_refs: list
```

### 10.3. Human handoff contract

| Field | Required | Notes |
| --- | --- | --- |
| `handoff_id` | Yes | Stable id. |
| `reason_code` | Yes | complaint, refund, health_sensitive, order_issue, fraud, abuse, policy_unknown, human_requested. |
| `priority` | Yes | P0/P1/P2. |
| `source_context` | Yes | Page, comment/thread, campaign/live refs. |
| `customer_safe_summary` | Yes | Redacted. |
| `automation_pause_scope` | Yes | thread, customer, page, campaign, global. |
| `owner_queue` | Yes | CSKH, sales, QA, finance, legal, admin. |
| `sla` | Optional but recommended | Required for P0 complaint/refund/privacy. |
| `resume_policy` | Yes | Who can release automation. |

Automation must pause for the scope while human takeover is active.

### 10.4. Attribution boundary

| Attribution | Gateway may store | Gateway must not do |
| --- | --- | --- |
| Page/source | page id, role, source surface. | Treat source as verified revenue. |
| Campaign/ad | utm/ad/campaign refs, click/message refs if redacted. | Compute ROAS/CPA/AOV PASS. |
| Live | live session id, script/product board refs. | Treat live comment as order/revenue. |
| Diamond/referral | referral entry/referrer refs if policy allows. | Compute commission/payable/payout. |
| CRM | delivery command/evidence refs. | Choose lifecycle or send despite opt-out. |

### 10.5. Requirements

| Requirement ID | Requirement |
| --- | --- |
| P5-2F-SRS-001 | Suppression check must run before every customer-facing delivery. |
| P5-2F-SRS-002 | Opt-out blocks CRM/proactive outbound and sales follow-up; support/legal messages need explicit allowed basis. |
| P5-2F-SRS-003 | Open complaint/refund/payment/privacy case suppresses sales content. |
| P5-2F-SRS-004 | Sale lock/recall/quality hold blocks product sell/quote/order language. |
| P5-2F-SRS-005 | Human handoff must preserve context and pause automation. |
| P5-2F-SRS-006 | Ads/ROAS and Diamond/commission are internal attribution only in Gateway. |
| P5-2F-SRS-007 | If attribution is unknown, store `unknown`; do not infer campaign, member or referral. |

### 10.6. Evidence

Gateway handoff is not valid unless evidence includes:

1. Suppression decision id.
2. Human handoff id or delivery command id.
3. Attribution refs and redaction status.
4. Guard trace or reason why no AI guard was required.
5. Owner queue/status if human routed.

No evidence means no `SUPPRESSION_AND_ATTRIBUTION_PASS`.

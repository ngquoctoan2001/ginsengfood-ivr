# P7.2D - Product / Price Boundary In Live

**BACKLOG:** LIVE-BLG-006  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Dam bao MC AI Live khong tu bao gia, khong tu xac nhan ton kho, khong tu tao quote/order; moi gia cuoi phai tu Commerce QuoteSnapshot.

## Entry Gate

- COM-BLG-002 QuoteSnapshot pass.
- P4 AI QuoteSnapshot consumption pass.
- P5 Gateway public/private handoff pass.

## Implementation Scope

- Live quote boundary guard.
- Product sellable/suppression awareness.
- Public-safe price wording.
- QuoteSnapshot request/handoff path via Gateway/AI/Commerce.
- Evidence for no self-price.

## Not Allowed

- Noi final price tu script/memory.
- Tu tinh discount/member benefit/live offer.
- Tu xac nhan con hang khi sellable unknown.
- Tu tao order_code/payment/revenue.

## Acceptance Criteria

- No QuoteSnapshot -> no final price.
- Live comment hoi gia -> route private/quote runtime.
- Expired QuoteSnapshot -> refresh request.
- Sale lock/recall -> no sales CTA.

## Smoke

| Smoke ID | Scenario | Expected |
|---|---|---|
| P7.2D-SMK-001 | Host asks price without quote | No final price |
| P7.2D-SMK-002 | Active QuoteSnapshot | Display allowed per policy |
| P7.2D-SMK-003 | Expired QuoteSnapshot | Request refresh |
| P7.2D-SMK-004 | SKU recall/sale lock | No sales CTA |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 9. SRS hardening addendum - Product / Price Boundary In Live

### 9.1. Purpose lock

`LIVE-BLG-006` khóa ranh giới: MC AI Live không phải pricing engine, không phải quote engine, không phải order engine. Mọi giá cuối cùng phải đến từ Commerce QuoteSnapshot; mọi order phải đi qua Order Draft + Customer Confirmation + Official Order.

### 9.2. Commerce dependency map

| Need in live | Allowed source | MC AI Live action |
|---|---|---|
| Product public name | Approved product knowledge/product scope | Mention only if public-safe and in live plan. |
| SKU sellable | Ops/Commerce sellable guard | Consume result; cannot override. |
| Stock/availability wording | Commerce/Ops safe projection | Do not invent quantity; if unknown, avoid availability claim. |
| Final price | `QuoteSnapshot` | Reference/display only if policy allows; otherwise private route. |
| Discount/member/Diamond benefit | QuoteSnapshot/runtime resolver | Never calculate in script. |
| Order draft | Commerce | Request/handoff only; not create official order. |
| Official order/order code | Commerce | Reference only after official order exists. |
| Payment/revenue | Commerce/Payment/Verified Revenue | Never confirm from comment/image/COD waiting. |

### 9.3. Quote boundary decision

```yaml
live_price_decision:
  input:
    live_session_id: string
    comment_id: string?
    sku_id: string?
    quote_snapshot_id: string?
    requested_surface: PUBLIC_LIVE | PRIVATE_MESSENGER | INTERNAL_OPERATOR
  allowed_public_output:
    - safe_ack_without_final_price
    - private_handoff_instruction_if_gateway_success
    - generic_product_story_without_price
  blocked_public_output:
    - final_payable
    - member_discount
    - diamond_benefit_amount
    - payment_instruction
    - order_code
  result:
    price_status:
      enum:
        - NO_QUOTE_NO_FINAL_PRICE
        - QUOTE_ACTIVE_PRIVATE_ONLY
        - QUOTE_EXPIRED_REFRESH_REQUIRED
        - SELLABLE_BLOCKED
        - SALE_LOCK_OR_RECALL_BLOCKED
        - RUNTIME_UNAVAILABLE
```

### 9.4. HostCue price rule

`HostCue.quote_snapshot_id` is conditional:

| Cue wording | `quote_snapshot_id` required | Allowed? |
|---|---:|---:|
| "Mình có món A hôm nay" | No | Yes if in product scope and claim-safe. |
| "Comment để Mình gửi giá riêng" | No | Yes if Gateway handoff eligible. |
| "Giá là X" | Yes | Public usually blocked; private/internal only by policy. |
| "Giờ vàng giảm X%" | Yes + program resolver | Public blocked unless policy explicitly allows safe wording. |
| "Còn Y hộp" | Requires safe availability projection | Otherwise blocked. |
| "Chốt đơn cho Em" | Not allowed | Blocked public; order via Commerce only. |

### 9.5. Sale lock / recall / sellable block

If any of these is true:

```yaml
sale_lock_active: true
recall_active: true
sellable_status: NOT_SELLABLE
quote_status: QUOTE_BLOCKED
```

Then:

- No sales CTA.
- No price cue.
- No private sales handoff except human/CSKH if policy allows.
- Create/require `LiveRiskCue` with `suppress_sales=true`.
- Evidence must reference sale lock/recall/sellable source.

### 9.6. Order/payment boundary

| Customer signal in live | Required handling |
|---|---|
| "Chốt 1 hộp" | Public ack + private handoff; no order code. |
| Sends phone/address in public | Do not repeat PII; route private/human per Gateway. |
| Sends transfer image | Do not mark paid; payment review only in proper channel. |
| Says "đã chuyển khoản" | Do not confirm payment; wait Payment/Core/Accounting review. |
| COD request | Order/payment method path only after Commerce guard; not revenue. |
| Asks invoice/tax | Private/human/Commerce path; no public tax data. |

### 9.7. Evidence required

| Evidence ID | Artifact |
|---|---|
| P7-2D-EVD-001 | No QuoteSnapshot -> no final price. |
| P7-2D-EVD-002 | Active QuoteSnapshot private/internal handling only. |
| P7-2D-EVD-003 | Expired QuoteSnapshot refresh required. |
| P7-2D-EVD-004 | Sale lock/recall blocks cue. |
| P7-2D-EVD-005 | Public buy intent does not create order. |
| P7-2D-EVD-006 | Payment proof/comment does not mark paid/revenue. |

### 9.8. Acceptance matrix

| AC ID | Requirement | Pass condition |
|---|---|---|
| P7-2D-AC-001 | No self-price | No final price unless active QuoteSnapshot exists and surface policy allows. |
| P7-2D-AC-002 | No self-discount | No member/Diamond/program calculation inside live script. |
| P7-2D-AC-003 | No self-order | Public buy intent cannot create official order/order code. |
| P7-2D-AC-004 | No self-payment | Payment/revenue remains Commerce/Payment/Verified Revenue. |
| P7-2D-AC-005 | Suppression wins | Sale lock/recall/not sellable blocks sales cue. |
| P7-2D-AC-006 | Evidence | Every price/order/payment block has trace/evidence. |

### 9.9. SRS fail conditions

SRS fails if it includes any of:

- "MC AI Live tính giá tạm".
- "Nếu khách comment chốt thì tạo order".
- "Ảnh chuyển khoản là PAID".
- "COD WAITING là revenue".
- "Live signal/order intent là ROAS".
- "Dùng cache giá để nói khi Commerce unavailable".


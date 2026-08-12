# P7.2C - Live Comment Classifier / Messenger Coordination

**BACKLOG:** LIVE-BLG-004 / LIVE-BLG-005  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Classify comment live va coordinate Gateway de public reply an toan, handoff Messenger khi can.

## Entry Gate

- GW-BLG-003/004 pass.
- GW-BLG-008 classifier/moderation evidence pass.
- LIVE-BLG-001 pass.

## Implementation Scope

- Live comment classifier.
- Intent categories: price, buy, consult, troll, complaint, spam, generic.
- Public response suggestion.
- Messenger handoff command via Gateway.
- Evidence for classification.

## Not Allowed

- MC AI Live tu gui Messenger bypass Gateway.
- Public private info/order/payment.
- Pull troll/malicious to Messenger sales.
- Treat complaint as troll.

## Acceptance Criteria

- Price/buy/deep consult -> public-safe + Messenger route.
- Troll/malicious -> moderation.
- Complaint -> CSKH/human route.
- All handoff commands go through Gateway contract.

## Smoke

| Smoke ID | Scenario | Expected |
|---|---|---|
| P7.2C-SMK-001 | "Gia bao nhieu" | Route Messenger, no public final price |
| P7.2C-SMK-002 | "Mua 1 hop" | Route order draft path via Gateway/AI/Commerce |
| P7.2C-SMK-003 | Troll comment | Moderation |
| P7.2C-SMK-004 | Complaint quality | CSKH/human |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 9. SRS hardening addendum - Live Comment Classifier / Messenger Coordination

### 9.1. Purpose lock

`LIVE-BLG-004/005` phân loại comment live để quyết định cue/handoff/moderation. Nó không gửi Messenger trực tiếp và không xác nhận handoff success nếu Gateway chưa có delivery log success.

### 9.2. Normalized input

```yaml
live_comment_classification_request:
  correlation_id: string
  idempotency_key: string
  channel_context_id: string
  live_session_id: string
  source_page_id: string
  comment_id: string
  comment_text_masked: string
  commenter_ref: hashed_or_internal_ref
  public_surface: LIVE_COMMENT
  gateway_event_ref: string
  live_plan_id: string?
  attribution_ref: internal_optional
```

Raw PII/token/raw payload must not enter SRS examples. Use masked samples only.

### 9.3. Classifier taxonomy

| Intent | Examples | Required route |
|---|---|---|
| `PRICE_INTENT` | hỏi giá, bao nhiêu, giá sao | Public-safe ack + Gateway private handoff/QuoteSnapshot path. |
| `BUY_INTENT` | mua, chốt, lấy 1 hộp | Gateway private handoff; no public order confirmation. |
| `DEEP_CONSULT` | bệnh nền, kiêng, cho người thân, thai kỳ | Private/human or AI Advisor guarded flow. |
| `PRODUCT_QUESTION_PUBLIC_SAFE` | vị, cách dùng chung, sản phẩm nào đang nói | Public-safe cue if claim guard pass. |
| `COMPLAINT` | hàng lỗi, giao sai, thanh toán, chất lượng | CSKH/human/quality route; not troll. |
| `ABUSE_MALICIOUS` | chửi, spam link, vu khống nặng, kích động | Moderation/risk cue; no Messenger sales pull. |
| `SPAM_DUPLICATE` | repeated dot/emoji/flood | Dedup/rate-limit/moderation. |
| `GENERIC_ENGAGEMENT` | chào, xem live, hỏi chung | Public-safe engagement cue. |
| `AD_OR_ATTRIBUTION_INTERNAL` | hỏi ads/campaign/ROAS | Never public internal ads data. |

### 9.4. Classification output

```yaml
live_comment_classification_result:
  classification_id: string
  intent: string
  confidence: number
  risk_level: LOW | MEDIUM | HIGH | P0
  public_reply_allowed: boolean
  public_reply_type:
    enum:
      - SAFE_ACK
      - PRODUCT_PUBLIC_SAFE
      - MODERATION_NONE
      - NO_PUBLIC_REPLY
  private_handoff_required: boolean
  private_handoff_reason: string?
  moderation_required: boolean
  human_required: boolean
  commerce_quote_required: boolean
  ads_signal_allowed: boolean
  blocked_actions:
    - PUBLIC_FINAL_PRICE
    - PUBLIC_ORDER_CONFIRMATION
    - DIRECT_MESSENGER_BYPASS
    - PUBLIC_PRIVATE_DATA
  evidence_refs: []
```

### 9.5. Messenger handoff rule

Private handoff must reference `schemas/channel/comment-to-messenger-handoff.schema.json` and, if Phase 7 creates a cue, `schemas/live/private-handoff-cue.schema.json`.

Rules:

1. Phase 7 may create `PrivateHandoffCue`.
2. Gateway creates/owns `CommentToMessengerHandoff`.
3. Phase 7 must not set Gateway handoff status by itself.
4. Public ack is allowed only when `public_ack_allowed=true`.
5. If `handoff_failed`, public wording must not say "đã gửi Messenger".
6. `context_preserved=true` is required for live/session/board/attribution preservation.

### 9.6. Routing truth table

| Intent | Public ack | Private handoff | Human/moderation | Commerce | Ads signal |
|---|---:|---:|---:|---:|---:|
| PRICE_INTENT | Yes, no final price | Yes | No | QuoteSnapshot path only | Funnel only |
| BUY_INTENT | Yes, no order confirm | Yes | Optional | Quote/order via Commerce only | Funnel only |
| DEEP_CONSULT | Minimal | Yes | May require human | No public quote | Funnel only |
| COMPLAINT | Minimal or none | Yes if private details | Human/CSKH | No sales until resolved | Complaint signal only |
| ABUSE_MALICIOUS | No debate | No sales handoff | Moderation | No | Risk signal only |
| SPAM_DUPLICATE | Usually no | No | Rate limit/dedup | No | No or deduped |
| GENERIC_ENGAGEMENT | Yes | No | No | No | Engagement only |

### 9.7. Negative requirements

SRS must explicitly test:

- Price comment does not public final price.
- Buy comment does not create official order.
- Handoff failure does not claim success.
- Duplicate comment does not duplicate handoff/cue.
- Complaint is not hidden as abuse without human/CSKH review path.
- Abuse/malicious is not routed to sales Messenger.
- Public response does not reveal campaign/ad/ROAS/attribution.

### 9.8. Evidence required

| Evidence ID | Artifact |
|---|---|
| P7-2C-EVD-001 | Price comment classification + private handoff cue. |
| P7-2C-EVD-002 | Buy intent classification + no public order. |
| P7-2C-EVD-003 | Handoff success with delivery/context preservation. |
| P7-2C-EVD-004 | Handoff fail with no false public ack. |
| P7-2C-EVD-005 | Duplicate comment dedup trace. |
| P7-2C-EVD-006 | Complaint routed to CSKH/human. |
| P7-2C-EVD-007 | Abuse/malicious routed to moderation, not sales. |

### 9.9. Acceptance criteria

| AC ID | Requirement | Pass condition |
|---|---|---|
| P7-2C-AC-001 | Classifier contract | Every classified comment has intent, confidence, risk, route. |
| P7-2C-AC-002 | Gateway coordination | Every private handoff references Gateway handoff contract. |
| P7-2C-AC-003 | Public safe | Public response never contains PII, final quote, order/payment, campaign/ROAS. |
| P7-2C-AC-004 | Context preserved | Live session/product board/attribution refs preserved when handoff succeeds. |
| P7-2C-AC-005 | Fail-safe | Gateway unavailable blocks handoff and customer-facing success wording. |
| P7-2C-AC-006 | Evidence | Classification and handoff decisions have evidence refs. |


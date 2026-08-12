# P5.2B - Public Comment Boundary / Comment-to-Messenger

**Backlog:** `GW-BLG-002 / GW-BLG-003`  
**Mapped FB files:** FB-02 Webhook/Event Normalization, FB-03 Live Session Registry/Comment -> Messenger  
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Mục tiêu

Khóa inbound public comment và live comment để không leak dữ liệu riêng, không public final price, không kêu khách tự inbox, và chỉ handoff Messenger khi đủ điều kiện.

## 2. Webhook event envelope

```yaml
NormalizedChannelEvent:
  normalized_event_id: required
  source_platform: facebook
  source_page_id: required
  event_type: live_comment | post_comment | message | reaction | delivery
  received_at: required
  raw_payload_ref: required_internal_only
  payload_signature_status: PASS | FAIL
  idempotency_key: required
  dedup_status: NEW | DUPLICATE | REPLAY_BLOCKED
  correlation_id: required
  live_session_id: required_if_live_comment
  comment_id: required_if_comment
  messenger_thread_id: optional
  attribution_id: optional_internal
  handoff_required: boolean
  handoff_reason: optional
```

MUST:

- Verify token handshake cho setup.
- Signature validation cho webhook POST.
- Raw payload chỉ lưu internal/restricted.
- Duplicate webhook không tạo side effect trùng.

## 3. Live Session Registry

```yaml
LiveSession:
  live_session_id: required
  source_page_id: required
  commerce_hub_page_id: required
  live_type: GOLDEN_HOUR_TRI_AN | DIAMOND_RECRUITMENT | EDUCATION | TEST
  session_board_id: optional_until_phase7
  started_at: required
  ended_at: optional
  public_reply_allowed: boolean
  private_reply_allowed: boolean
  production_allowed: boolean
```

Live type không được tự động mở quote/order. Quote/order vẫn phải đi qua AI Advisor và Commerce Runtime.

## 4. Public trigger phải kéo private hoặc human

Public comment/live comment phải route private/human khi:

- Hỏi giá.
- Muốn mua/chốt/lấy hàng.
- Để số điện thoại/địa chỉ/email/MST.
- Hỏi thanh toán/hóa đơn.
- Hỏi bệnh nền/kiêng cữ/thai kỳ/trẻ nhỏ.
- Cần tư vấn sâu.
- Hỏi Diamond/referral.
- Hỏi sản phẩm trong live board cần báo giá.

Public reply chỉ được safe ack, không lặp PII và không báo giá cuối.

## 5. System-initiated Messenger handoff

```yaml
CommentHandoff:
  handoff_id: required
  source_event_id: required
  source_page_id: required
  commerce_hub_page_id: required
  handoff_trigger: price | buy | pii | payment | invoice | health_sensitive | diamond | consult_deep
  handoff_status: PENDING | SENT | FAILED | BLOCKED
  public_reply_status: NOT_SENT | SENT | BLOCKED
  private_reply_status: NOT_SENT | SENT | FAILED
  delivery_log_id: required_if_sent
  attribution_id: preserve_if_any
  live_session_id: preserve_if_any
```

Public chỉ được nói đã chuyển/gửi khi `handoff_status=SENT`. Nếu handoff fail, public dùng safe ack không nói đã gửi.

## 6. Forbidden public output

- Final price.
- QuoteSnapshot ID.
- Order code.
- Phone/address/email/MST.
- Payment account/instruction.
- Health-sensitive advice dài.
- Campaign/ad/ROAS/internal attribution.
- Commission/payout.
- Câu kiểu "Mình nhắn Messenger giúp Em" khi chưa có handoff system success.

## 7. Acceptance criteria

- 1 public reply/comment theo policy.
- 1 private reply/comment đủ điều kiện theo policy.
- Comment có giá/mua/chốt route Messenger/private.
- Comment có PII không bị lặp.
- Complaint route CSKH/human.
- Troll/spam/malicious route moderation, không tạo sales lead.
- Board/live/attribution context preserve khi handoff.

## 8. Smoke

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P5.2B-SMK-001 | Live comment hỏi giá | Không public giá, tạo handoff. |
| P5.2B-SMK-002 | Handoff fail | Không nói đã gửi. |
| P5.2B-SMK-003 | Public có số điện thoại | Không lặp PII, route private/human. |
| P5.2B-SMK-004 | Public hỏi bệnh nền | Safe ack, private/human route. |
| P5.2B-SMK-005 | Duplicate webhook | Không duplicate reply/handoff. |

## 9. Final status

`LIMITED_IMPLEMENTATION_REPORT_ONLY`

`PUBLIC_PRIVATE_GATEWAY_PRODUCTION_BLOCKED`

## 10. SRS hardening addendum - Public Comment và Comment-to-Messenger

### 10.1. Scope

File này khóa xử lý inbound public comment, live comment và private handoff. Mục tiêu là chống leak, chống spam và chống chốt đơn công khai, không phải tối đa hóa số lượng reply.

### 10.2. Normalized comment event

```yaml
NormalizedCommentEvent:
  normalized_event_id: required
  page_registry_id: required
  surface: public_comment|live_comment
  post_id: optional
  live_session_id: optional
  comment_id: required
  parent_comment_id: optional
  commenter_channel_ref: required_redacted
  event_text_redacted: required
  event_text_hash: required
  contains_pii: boolean
  contains_health_sensitive: boolean
  contains_price_or_buy_intent: boolean
  contains_complaint: boolean
  contains_spam_or_abuse: boolean
  source_campaign_ref: optional
  idempotency_key: required
  received_at: timestamp
```

### 10.3. Public action matrix

| Intent | Public action | Private/human action | Forbidden |
| --- | --- | --- | --- |
| Hỏi giá/mua/chốt | Safe acknowledgement only. | Private handoff if permitted; otherwise human/support route. | Final price, bank info, order confirmation. |
| Hỏi sản phẩm chung | Short public-safe answer using approved product public knowledge. | Private if advice is deep/personal. | Health claim, internal formula, off-board SKU. |
| Có số điện thoại/địa chỉ | Do not repeat PII. | Private/human route. | Echo phone/address. |
| Bệnh nền/nhạy cảm sức khỏe | Safe disclaimer/ack if allowed. | Human/private support. | Medical conclusion or dosage promise. |
| Complaint/quality/refund | Acknowledge support route. | Human/CSKH; suppress sales. | Sales reply or blame assignment. |
| Spam/troll/abuse | No sales lead; moderation route. | Human only if escalation policy requires. | Arguing publicly or private sales handoff. |
| Duplicate webhook | No new side effect. | None unless original failed and retry policy allows. | Duplicate reply/handoff. |

### 10.4. Handoff state model

```text
HANDOFF_NOT_REQUIRED
HANDOFF_ELIGIBILITY_CHECK
-> HANDOFF_ALLOWED
-> HANDOFF_SEND_PENDING
-> HANDOFF_SENT
-> MESSENGER_SESSION_PENDING
-> MESSENGER_SESSION_BOUND

HANDOFF_ELIGIBILITY_CHECK -> HANDOFF_BLOCKED
HANDOFF_SEND_PENDING -> HANDOFF_FAILED
HANDOFF_SENT -> HANDOFF_DELIVERY_UNKNOWN
```

Invalid transitions:

- `HANDOFF_BLOCKED -> PUBLIC_SAYS_SENT`.
- `HANDOFF_FAILED -> PUBLIC_SAYS_SENT`.
- `HANDOFF_NOT_REQUIRED -> MESSENGER_SESSION_BOUND`.
- Duplicate comment -> second `HANDOFF_SEND_PENDING`.

### 10.5. Acceptance requirements

| Requirement ID | Requirement |
| --- | --- |
| P5-2B-SRS-001 | Webhook signature and idempotency pass before classifier or handoff. |
| P5-2B-SRS-002 | Public safe reply must be generated from a public-safe template or AI response already guard-pass for public surface. |
| P5-2B-SRS-003 | If private handoff cannot be sent, public wording must not imply it was sent. |
| P5-2B-SRS-004 | Each comment can create at most one active handoff per target session unless the prior handoff is explicitly canceled. |
| P5-2B-SRS-005 | Comment evidence must include source page, live session if any, classifier decision, policy decision and handoff status. |
| P5-2B-SRS-006 | Spam/troll/abuse must not enter AI sales flow by default. |
| P5-2B-SRS-007 | Complaint opens suppression for sales reply in the affected conversation scope. |

### 10.6. Evidence payload

```yaml
CommentHandoffEvidence:
  evidence_id: required
  normalized_event_id: required
  page_registry_id: required
  classifier_result: required
  public_policy_result: required
  handoff_status: not_required|allowed|sent|failed|blocked
  public_reply_status: not_sent|sent|blocked
  private_reply_status: not_sent|sent|failed|blocked
  blocked_reasons: list
  redaction_status: pass|fail
  created_at: timestamp
```

No evidence, no `PUBLIC_PRIVATE_GATEWAY_PASS`.

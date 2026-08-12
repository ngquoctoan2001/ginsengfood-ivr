# P5.2C - Messenger Context / Final Response Guard

**Backlog:** `GW-BLG-004 / GW-BLG-005`  
**Mapped FB file:** FB-04 - Messenger Thread / AI Advisor Routing / Hub-Spoke Handoff  
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Mục tiêu

Đảm bảo mọi conversation sau public handoff được xử lý trong private context, preserve source attribution, gọi AI Advisor đúng contract và chỉ delivery khi Final Response Guard pass.

## 2. Hub-spoke rule

- Commerce Hub Messenger: `Ginsengfood - Cháo Sâm Mỗi Ngày`.
- Page spoke chỉ tạo source event và handoff.
- Page spoke không có sales Messenger flow riêng nếu policy chưa cho phép.
- Khi khách từ spoke sang hub, phải preserve `source_page_id`, `live_session_id`, `comment_id`, `attribution_id`, `referral_link_id`, `diamond_id` nếu có.

## 3. MessengerThreadBinding contract

```yaml
MessengerThreadBinding:
  binding_id: required
  commerce_hub_page_id: required
  source_page_id: required
  psid_or_customer_channel_identity_id: required
  messenger_thread_id: required
  conversation_id: required
  entry_channel: live_comment | post_comment | direct_messenger | ads_origin | crm
  source_event_id: optional
  attribution_id: optional_internal
  live_session_id: optional
  privacy_scope: PRIVATE
  active_window_status: OPEN | CLOSED | POLICY_RESTRICTED
```

## 4. AI routing contract

Gateway gửi normalized event sang AI Channel. AI Channel trả về delivery command hoặc block/handoff.

```yaml
GatewayToAIRequest:
  normalized_event_id: required
  channel_context: required
  thread_binding: required_if_private
  customer_context_ref: optional
  message_text_ref: required
  attribution_context_ref: optional_internal

AIDeliveryDecision:
  decision_envelope_id: required
  selected_action: SEND_MESSAGE | BLOCK_RESPONSE | HUMAN_HANDOFF | REQUEST_QUOTE | NOOP
  final_guard_status: PASS | FAIL | BLOCKED
  delivery_command_ref: required_if_SEND_MESSAGE
  blocked_actions: list
  evidence_refs: list
```

Gateway chỉ delivery `SEND_MESSAGE` khi guard pass.

## 5. Required guards

- FinalResponseGuard.
- CustomerFacingLanguageGuard.
- PublicPrivateScopeGuard.
- SuppressionGuard.
- ChannelRateLimitGuard.
- QuoteSnapshotGuard nếu có giá.
- CustomerConfirmationGuard nếu có order.
- OrderCodeGuard nếu có order success.
- ROASInternalOnlyGuard nếu có ads context.

## 6. Messenger forbidden behavior

- Không gửi AI draft trực tiếp.
- Không tự sửa response để thêm giá/claim/order.
- Không nói final price nếu thiếu QuoteSnapshot.
- Không nói official order nếu thiếu order_code.
- Không nói paid nếu runtime chưa confirmed.
- Không public campaign/ad/ROAS.
- Không mất attribution khi quote/order.

## 7. Acceptance criteria

- Public handoff sang Messenger giữ private context.
- Mọi outbound có DecisionEnvelope và final guard trace.
- Guard fail thì no delivery hoặc human handoff.
- Quote/order/payment message chỉ render từ runtime owner.
- Handoff success/fail có delivery log.

## 8. Smoke

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P5.2C-SMK-001 | Messenger after live handoff | Private context preserved. |
| P5.2C-SMK-002 | AI response pass guard | Delivery allowed. |
| P5.2C-SMK-003 | Unguarded response | Block/quarantine. |
| P5.2C-SMK-004 | Private quote missing QuoteSnapshot | No final price. |
| P5.2C-SMK-005 | Ads-origin thread | campaign/ad internal only. |

## 9. Final status

`LIMITED_IMPLEMENTATION_REPORT_ONLY`

`FINAL_RESPONSE_GUARD_REQUIRED`

## 10. SRS hardening addendum - Messenger Context và Final Response Guard

### 10.1. Messenger session binding

Messenger private chỉ được coi là private-safe khi thread được bind đúng page, customer/channel ref và source event.

```yaml
MessengerSession:
  messenger_session_id: required
  page_registry_id: required
  thread_ref: required_redacted
  customer_channel_ref: required_redacted
  source_event_id: optional
  source_surface: public_comment|live_comment|messenger_private|page_inbox
  source_comment_id: optional
  source_campaign_ref: optional
  private_context_status: not_started|created|bound|active|expired|blocked|closed
  human_takeover_status: none|requested|active|released
  last_customer_message_at: timestamp
  expires_at: timestamp
```

### 10.2. AI handoff request

Gateway gửi AI Advisor request, nhưng request phải chỉ là context và không chứa quyền tạo truth.

```yaml
AiAdvisorChannelRequest:
  request_id: required
  channel_context_id: required
  surface: messenger_private
  page_role: required
  customer_channel_ref: redacted
  source_comment_summary: optional_redacted
  product_scope_refs: list
  quote_snapshot_ref: optional
  order_context_ref: optional
  sale_lock_status: pass|blocked|unknown
  recall_status: pass|blocked|unknown
  suppression_status: pass|blocked|deferred
  allowed_response_modes:
    - public_safe_ack
    - private_advice
    - quote_handoff
    - order_handoff
    - human_handoff
  forbidden_actions:
    - self_price
    - self_order
    - self_payment
    - self_commission
    - public_pii
```

### 10.3. Final Response Guard output

```yaml
FinalResponseGuardResult:
  guard_trace_id: required
  ai_response_id: required
  surface: messenger_private|public_comment
  claim_guard_status: pass|fail|blocked
  public_private_scope_status: pass|fail|blocked
  quote_snapshot_status: not_required|present|missing|stale
  order_confirmation_status: not_required|present|missing|stale
  payment_disclosure_status: pass|blocked
  pii_redaction_status: pass|fail
  suppression_status: pass|blocked|deferred
  final_guard_status: pass|fail|blocked|pending
  allowed_delivery: boolean
  blocked_reasons: list
```

### 10.4. Guard truth table

| Condition | Delivery result |
| --- | --- |
| `final_guard_status=pass` and suppression pass | Delivery may proceed through pacing/rate-limit. |
| Quote wording but `quote_snapshot_status=missing|stale` | Block final price; ask for quote generation/human route. |
| Order success wording but confirmation/order_code missing | Block; no order success claim. |
| Payment wording without payment owner confirmation | Block payment claim. |
| PII in public view | Block public delivery; route private/human. |
| Sale lock/recall active | Block sales answer; support/human only. |
| Human takeover active | Automation paused. |

### 10.5. Requirements

| Requirement ID | Requirement |
| --- | --- |
| P5-2C-SRS-001 | Gateway cannot deliver AI text without guard trace id. |
| P5-2C-SRS-002 | Public and private renderers must be separate. |
| P5-2C-SRS-003 | Messenger thread mismatch blocks automated answer. |
| P5-2C-SRS-004 | Stale quote/order refs are treated as missing. |
| P5-2C-SRS-005 | Handoff to human must preserve context, source event, last AI decision and blocked reason. |
| P5-2C-SRS-006 | Ads/campaign/referral context is internal metadata; not customer-facing unless owner-approved public wording exists. |

### 10.6. Evidence requirements

Every sent Messenger response must have:

1. Messenger session id.
2. Channel context id.
3. AI response id.
4. Final guard trace id.
5. Suppression decision id.
6. Delivery command id.
7. Redacted rendered text hash.
8. Provider delivery result or dead-letter id.

Without these, status remains `FINAL_RESPONSE_GUARD_REQUIRED`.

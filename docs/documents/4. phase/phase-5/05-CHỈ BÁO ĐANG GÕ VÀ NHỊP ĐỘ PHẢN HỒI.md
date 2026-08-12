# P5.2D - Typing Indicator / Response Pacing

**Backlog:** `GW-BLG-006`  
**Mapped FB scope:** Delivery command, outbound policy, pacing, retry evidence  
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Mục tiêu

Thiết kế delivery pacing để Gateway không trả lời như bot spam, không vượt policy kênh, không bypass guard và có evidence cho từng outbound.

## 2. Delivery command contract

```yaml
GatewayDeliveryCommand:
  delivery_command_id: required
  decision_envelope_id: required
  target_channel: public_comment | messenger_private | live_inbox
  target_page_id: required
  target_thread_or_comment_id: required
  message_render_ref: required
  final_guard_status: PASS
  public_private_scope: public | private
  rate_limit_status: PASS
  suppression_status: PASS
  typing_policy_status: PASS | SKIPPED_BY_POLICY
  idempotency_key: required
```

Gateway không được delivery nếu `final_guard_status` khác PASS.

## 3. TypingDelayPolicy

```yaml
TypingDelayPolicy:
  enabled_if:
    - channel_supports_typing_indicator = true
    - channel_policy_allows_delay = true
    - message_is_customer_facing = true
  base_ms: 1200
  per_character_ms: 28
  min_ms: 1500
  max_ms: 9000
  long_message_chunk_threshold_chars: 650
  cancel_if_customer_sends_new_message: true
  bypass_for:
    - complaint
    - payment_issue
    - privacy_case
    - health_sensitive
    - delivery_exception
```

Không giả lập typing ở public comment nếu platform policy không cho phép.

## 4. Chunking rule

- Message ngắn dưới 120 ký tự: 1.5-3 giây nếu channel support.
- Message tư vấn combo dài: 5-9 giây hoặc chia 2-3 chunk.
- Mỗi chunk phải giữ nguyên public/private boundary.
- Không split làm mất nghĩa quote/order/payment.
- Không delay lâu với complaint/payment/privacy/health-sensitive.

## 5. Retry policy

```yaml
DeliveryRetryPolicy:
  max_attempts: 3
  retry_backoff: bounded_exponential
  retry_allowed_for:
    - transient_platform_error
    - network_timeout
  retry_forbidden_for:
    - guard_fail
    - suppression_fail
    - permission_missing
    - policy_block
  dead_letter_required: true
```

Retry không được tạo duplicate reply vì idempotency key phải cố định.

## 6. Delivery evidence

Mỗi outbound cần:

- delivery_command_id.
- decision_envelope_id.
- final_guard_trace.
- target channel/page/thread/comment.
- typing_delay_decision_ms.
- sent_at hoặc fail_reason.
- idempotency_key.
- retry_count.
- delivery_log_id.

## 7. Acceptance criteria

- No instant bot reply cho private customer-facing flow khi channel support.
- Unsupported typing indicator skip safely.
- Customer gửi tin mới thì cancel/recompute.
- Guard fail không retry.
- Delivery log có đầy đủ context.

## 8. Smoke

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P5.2D-SMK-001 | Short private reply | Pacing đúng rule. |
| P5.2D-SMK-002 | Long combo advice | Chunking safe hoặc delay capped. |
| P5.2D-SMK-003 | Unsupported public comment typing | Skip safely. |
| P5.2D-SMK-004 | Customer sends new message while typing | Cancel and recompute. |
| P5.2D-SMK-005 | Delivery transient fail | Retry bounded + audit. |

## 9. Final status

`LIMITED_IMPLEMENTATION_REPORT_ONLY`

`DELIVERY_PACING_REQUIRES_EVIDENCE`

## 10. SRS hardening addendum - Delivery Pacing

### 10.1. Delivery command contract

```yaml
GatewayDeliveryCommand:
  delivery_command_id: required
  decision_id: required
  target_surface: public_comment|messenger_private|page_inbox
  target_ref: required_redacted
  rendered_text_hash: required
  rendered_text_redacted: required_for_evidence
  message_kind: safe_ack|private_advice|handoff_notice|support_notice|crm_delivery|system_notice
  final_guard_trace_id: required_unless_system_notice
  suppression_decision_id: required
  rate_limit_bucket: required
  pacing_policy_id: required
  max_chunks: integer
  retry_policy_id: required
  correlation_id: required
```

### 10.2. Pacing policy

| Surface | Typing indicator | Minimum behavior | Forbidden |
| --- | --- | --- | --- |
| Public comment | Usually unsupported | No fake typing; one short safe reply when allowed. | Multi-chunk sales reply, repeated bump, final price. |
| Messenger private | Supported if platform allows | Typing on -> delay bounded -> send chunked response. | Instant long bot-like response if policy requires pacing. |
| Page inbox/system notice | Depends on provider | Respect same rate-limit and suppression. | Sending without guard when customer-facing. |
| Human handoff | Not automated reply | Notify internal queue and pause automation. | Continuing automated sales messages during human takeover. |

### 10.3. State machine

```text
DELIVERY_CREATED
-> DELIVERY_SUPPRESSION_CHECKED
-> DELIVERY_RATE_LIMIT_CHECKED
-> DELIVERY_TYPING_STARTED
-> DELIVERY_DELAY_WAITING
-> DELIVERY_SEND_PENDING
-> DELIVERY_SENT
-> DELIVERY_EVIDENCE_WRITTEN

DELIVERY_CREATED -> DELIVERY_BLOCKED
DELIVERY_SEND_PENDING -> DELIVERY_RETRY_WAITING
DELIVERY_RETRY_WAITING -> DELIVERY_SEND_PENDING
DELIVERY_RETRY_WAITING -> DELIVERY_DEAD_LETTERED
DELIVERY_TYPING_STARTED -> DELIVERY_CANCELED_BY_NEW_CUSTOMER_MESSAGE
```

### 10.4. Retry rules

| Failure | Retry? | Required handling |
| --- | --- | --- |
| Rate limited | Yes, bounded and delayed. | No immediate retry storm. |
| Transient provider error | Yes, bounded. | Preserve idempotency key. |
| Invalid permission/App Review | No. | Mark blocked and surface admin evidence gap. |
| Suppression active | No. | Block/defer according to suppression. |
| Guard fail | No. | Block and route human if needed. |
| Customer sends new message while typing | No for old command. | Cancel old command and recompute context. |

### 10.5. Chunking rules

1. Public comments should be one safe acknowledgement unless owner policy explicitly allows more.
2. Messenger private may split long text only if every chunk individually passes public/private and PII redaction rules.
3. No chunk may contain standalone bank/payment/order success unless owner runtime ref exists.
4. Chunk order must be stable and idempotent.
5. If one chunk fails guard, the whole response is blocked or recomputed.

### 10.6. Evidence requirements

```yaml
DeliveryPacingEvidence:
  delivery_command_id: required
  pacing_policy_id: required
  typing_supported: boolean
  typing_started_at: nullable
  send_scheduled_at: timestamp
  send_attempts: integer
  final_provider_status: sent|failed|blocked|dead_lettered|canceled
  cancellation_reason: nullable
  rate_limit_snapshot_ref: required
  redaction_status: pass|fail
```

No delivery evidence, no `DELIVERY_PACING_PASS`.

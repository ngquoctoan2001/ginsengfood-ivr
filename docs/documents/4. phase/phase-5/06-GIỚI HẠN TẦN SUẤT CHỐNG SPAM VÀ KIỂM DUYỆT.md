# P5.2E - Rate Limit / Anti-Spam / Moderation

**Backlog:** `GW-BLG-007 / GW-BLG-008`  
**Mapped FB scope:** Dedup/idempotency, moderation, App Review/security, retry/DLQ  
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Mục tiêu

Khóa lớp chống spam, rate-limit, moderation, complaint split, dedup và security để Gateway không tạo duplicate delivery, không kéo spam/troll vào sales funnel và không mở production thiếu App Review/security.

## 2. Rate-limit policy

```yaml
GatewayRateLimitPolicy:
  public_reply:
    max_per_comment: 1
  private_reply_from_comment:
    max_per_eligible_comment: 1
  messenger_conversation:
    mode: conversational
    spam_burst_guard: required
  crm_after_purchase:
    max_per_customer_per_day: 1
  crm_reorder_or_flavor_change:
    max_per_customer_per_3_days: 1
  winback:
    max_per_customer_per_7_days: 1
  campaign:
    max_per_customer_per_day: 1
    requires_consent: true
  quiet_hours:
    from: "22:00"
    to: "08:00"
```

## 3. Moderation split

| Intent | Route | Rule |
| --- | --- | --- |
| Trust objection nhẹ | AI trust-safe response | Không tranh luận, không spam. |
| Yêu cầu bằng chứng | Approved proof / Messenger / human | Không bịa chứng nhận. |
| Complaint thật | CSKH/human | Complaint thắng sales flow. |
| Hàng giả/chất lượng/privacy/payment | Human/support queue | Suppress sales/CRM. |
| Troll/spam/malicious | Moderation/quarantine | Không tạo sales lead. |
| Malicious link | Block/quarantine | Không delivery. |

## 4. Dedup/idempotency

```yaml
GatewayDedupKey:
  key_parts:
    - source_platform
    - source_page_id
    - event_type
    - platform_event_id_or_comment_id_or_message_id
    - live_session_id_if_any
    - delivery_target_if_outbound
```

Dedup bắt buộc cho:

- Duplicate webhook.
- Duplicate comment.
- Duplicate handoff.
- Duplicate private reply.
- Duplicate quote command.
- Duplicate order command.
- Duplicate commission/attribution command.

## 5. Security/App Review checklist

MUST before production:

- Meta App mode and App Review status recorded.
- Permission use cases documented.
- Webhook subscribed fields approved.
- Signature validation test pass.
- Verify token not exposed.
- Page access token stored by secret_ref.
- Token rotation runbook.
- Test page/user evidence.
- Data deletion/retention policy.

## 6. Runbook required

Runbook phải cover:

- Webhook fail.
- Token expired.
- Duplicate comment.
- Page blocked outbound.
- Messenger handoff fail.
- ROAS dashboard mismatch.
- CAPI/offline conversion error.
- Live spam burst.
- Public PII.
- Public health-sensitive question.
- Spoke page duplicate message.

## 7. Acceptance criteria

- Spam burst rate-limited.
- Duplicate event không tạo side effect trùng.
- Complaint thật không bị xử lý như troll.
- Troll/spam/malicious không vào Messenger sales lead.
- Security checklist không còn P0 gap trước production.

## 8. Smoke

| Smoke ID | Scenario | Expected |
| --- | --- | --- |
| P5.2E-SMK-001 | 20 comments lặp | Rate-limit active. |
| P5.2E-SMK-002 | Duplicate webhook | No duplicate reply/handoff. |
| P5.2E-SMK-003 | Complaint chất lượng | Human/CSKH route. |
| P5.2E-SMK-004 | Troll abusive | Moderation, no sales lead. |
| P5.2E-SMK-005 | Missing signature | Reject/quarantine. |

## 9. Final status

`LIMITED_IMPLEMENTATION_REPORT_ONLY`

`SECURITY_AND_MODERATION_EVIDENCE_REQUIRED`

## 10. SRS hardening addendum - Rate Limit, Anti-spam, Moderation, Security

### 10.1. Rate-limit buckets

| Bucket | Scope | Default behavior |
| --- | --- | --- |
| `page_public_reply` | page + post/live + time window | Prevent repeated public replies from same page. |
| `thread_private_reply` | page + thread + customer + time window | Prevent Messenger spam. |
| `comment_handoff` | comment id + target thread | One active handoff per comment. |
| `crm_delivery` | customer + campaign/lifecycle + time window | Honor opt-out/frequency cap/quiet hour. |
| `provider_api` | app/page/provider token | Backoff on provider limit. |
| `duplicate_event` | idempotency key | No repeated side effects. |

Exact numeric limits are owner-configured; missing config defaults to stricter mode: block or dry-run.

### 10.2. Moderation taxonomy

| Category | Examples | Action |
| --- | --- | --- |
| `SPAM_DUPLICATE` | Repeated same content/link/comment flood. | No sales lead, rate-limit/quarantine. |
| `ABUSE_TROLL` | Chửi bới, phá live, xúc phạm. | Moderate; human if policy requires. |
| `COMPLAINT_REAL` | Khiếu nại chất lượng, giao hàng, hoàn tiền. | Human/CSKH, suppress sales. |
| `SCAM_COUNTERFEIT_ACCUSATION` | Tố hàng giả/lừa đảo. | Human/QA/legal route; no auto sales. |
| `PII_PUBLIC` | Phone, address, payment details. | Redact; private/human route. |
| `HEALTH_SENSITIVE` | Bệnh nền, liều dùng, điều trị. | Safe ack + human/private advice guard. |
| `POLICY_BLOCKED_CONTENT` | Nội dung platform cấm. | Block delivery; evidence. |

### 10.3. Security controls

| Control ID | Control | P0 fail if |
| --- | --- | --- |
| P5-2E-SEC-001 | Meta signature validation. | Webhook accepted without validation. |
| P5-2E-SEC-002 | Verify token by secret ref. | Raw token in docs/log/evidence. |
| P5-2E-SEC-003 | Page Access Token in secret store only. | Token stored in DB plain text or markdown. |
| P5-2E-SEC-004 | Replay protection. | Same webhook creates duplicate reply. |
| P5-2E-SEC-005 | Admin RBAC for page/config/evidence. | Non-admin can enable production or read secrets. |
| P5-2E-SEC-006 | Evidence redaction. | Screenshot/log contains token, secret or raw PII. |
| P5-2E-SEC-007 | App Review/permission gate. | Production outbound enabled with missing permission evidence. |

### 10.4. App Review evidence package

```yaml
MetaAppReviewEvidence:
  evidence_id: required
  app_id_ref: required_redacted
  page_registry_id: required
  permission_name: required
  requested_use_case: required
  approved_status: pending|approved|rejected|not_required|unknown
  approval_artifact_ref: required_unless_not_required
  subscribed_fields: list
  reviewer_notes_ref: optional
  expires_or_review_after: optional
  redaction_status: pass
```

`approved_status=unknown|pending|rejected` means production outbound remains blocked.

### 10.5. Requirements

| Requirement ID | Requirement |
| --- | --- |
| P5-2E-SRS-001 | Rate-limit check occurs after suppression/guard and before provider send. |
| P5-2E-SRS-002 | Duplicate webhook must not create duplicate public reply, private handoff, delivery command or CRM command. |
| P5-2E-SRS-003 | Real complaint must never be classified as sales opportunity by default. |
| P5-2E-SRS-004 | Abuse/spam may be logged, but must not be pushed into AI sales flow. |
| P5-2E-SRS-005 | Missing security/App Review evidence blocks production, even if local smoke passes. |
| P5-2E-SRS-006 | Admin override requires reason, owner and expiry; no permanent silent bypass. |

### 10.6. Evidence

Every smoke run must attach:

1. Signature validation proof.
2. Duplicate/idempotency proof.
3. Rate-limit bucket decision proof.
4. Moderation decision proof.
5. App Review/permission status proof.
6. Redaction proof.

Without these artifacts, final status remains `SECURITY_AND_MODERATION_EVIDENCE_REQUIRED`.

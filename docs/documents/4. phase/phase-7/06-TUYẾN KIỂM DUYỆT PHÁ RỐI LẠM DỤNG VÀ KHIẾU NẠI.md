# P7.2E - Troll / Abuse / Complaint Route

**BACKLOG:** LIVE-BLG-007 / LIVE-BLG-008  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Tach troll/abuse/malicious khoi complaint that va route dung moderation/CSKH/human.

## Entry Gate

- GW-BLG-008 pass.
- P7.2C classifier evidence available.
- Human/CSKH route accepted.

## Implementation Scope

- Troll/abuse classifier.
- Complaint classifier.
- Quality/CSKH handoff.
- Moderator suggestion.
- Evidence with masked samples.

## Not Allowed

- Complaint that bi xoa/hide nhu troll.
- Troll/malicious bi keo vao Messenger sales.
- MC AI Live tranh luan cong khai lam tang risk.
- Lo thong tin don hang/payment cua khach tren public.

## Acceptance Criteria

- Troll -> moderation/safe ignore/hide by policy.
- Complaint -> CSKH/human with evidence.
- Severe issue -> escalation.
- Response wording public-safe.

## Smoke

| Smoke ID | Scenario | Expected |
|---|---|---|
| P7.2E-SMK-001 | Abuse/troll | Moderation route |
| P7.2E-SMK-002 | Complaint hang loi | CSKH route |
| P7.2E-SMK-003 | Payment/order complaint | Private/human, no public detail |
| P7.2E-SMK-004 | Malicious link | Block/quarantine |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 9. SRS hardening addendum - Abuse / Complaint / Moderation Route

### 9.1. Purpose lock

`LIVE-BLG-007/008` phải tách rõ abuse/malicious/spam khỏi complaint thật. Complaint thật là tín hiệu CSKH/Quality/Human, không phải noise để xóa. Abuse/malicious là risk/moderation, không phải lead bán hàng để kéo Messenger.

### 9.2. Risk taxonomy

| Risk class | Definition | Route |
|---|---|---|
| `ABUSE_LIGHT` | Chê, nghi ngờ, khó chịu nhẹ. | Public-safe trust response hoặc ignore theo policy. |
| `ABUSE_HEAVY` | Chửi thề, kích động, công kích cá nhân. | No debate, moderation/human. |
| `MALICIOUS_LINK` | Link lạ, spam link, phishing. | Quarantine/moderation; no Messenger sales. |
| `DEFAMATION_LEGAL` | Vu khống/lừa đảo nghiêm trọng, đe dọa pháp lý. | Admin/legal/human. |
| `QUALITY_COMPLAINT` | Hàng lỗi, mùi/vị, bao bì, HSD, nghi hàng giả. | CSKH/Quality/human. |
| `ORDER_PAYMENT_COMPLAINT` | Sai đơn, chậm giao, chuyển khoản, COD. | Private/human/order support. |
| `HEALTH_ADVERSE_EVENT` | Khách phản ánh sức khỏe, dị ứng, phản ứng xấu. | Human/Quality P0; no sales cue. |
| `PRIVACY_INCIDENT` | Lộ phone/address/payment/member/order. | Privacy/security/human. |

### 9.3. LiveRiskCue mapping

For every `HIGH` or `P0` risk, require `LiveRiskCue`:

```yaml
live_risk_cue:
  live_session_id: string
  channel_context_id: string
  risk_type: QUALITY_COMPLAINT | HEALTH_ADVERSE_EVENT | ABUSE_HEAVY | MALICIOUS_LINK | PRIVACY_INCIDENT | LEGAL_BRAND_ATTACK
  severity: LOW | MEDIUM | HIGH | P0
  host_action:
    enum_hint:
      - NO_PUBLIC_DEBATE
      - SAFE_ACK_ONLY
      - HUMAN_HANDOFF
      - QUALITY_HANDOFF
      - MODERATION_REVIEW
      - ADMIN_LEGAL_REVIEW
      - PRIVACY_REVIEW
  suppress_sales: boolean
  handoff_required: boolean
```

### 9.4. Complaint route rules

| Complaint type | Public wording | Private/human route | Sales allowed? |
|---|---|---|---:|
| Quality issue | Safe ack, no blame, no diagnosis | CSKH/Quality | No until resolved. |
| Order/payment issue | No public order/payment details | Private/human/order support | No until verified. |
| Health/adverse event | Safe ack; no medical advice | Human/Quality P0 | No. |
| Privacy leak | Do not repeat data | Privacy/security/human | No. |
| Legal/brand attack | No debate | Admin/legal | No. |

### 9.5. Abuse/malicious route rules

| Abuse type | Required action | Forbidden action |
|---|---|---|
| Light distrust | Trust-safe approved response or no response. | Argue, insult, fake proof. |
| Heavy abuse | Moderation/human. | Pull into sales Messenger. |
| Malicious link | Quarantine/hide/report by policy. | Click/render/promote link. |
| Repeated spam | Dedup/rate limit/moderation. | Generate multiple replies. |
| Coordinated attack | Admin/legal escalation. | Public debate by MC AI. |

### 9.6. Evidence privacy

Evidence samples must:

- Mask phone/address/email/order/payment.
- Store raw comment ref internally only.
- Include `correlation_id`, `comment_id`, `live_session_id`, `risk_type`, route, and owner/human queue if applicable.
- Not include screenshots as sole proof.
- Not expose internal ads/attribution/ROAS data.

### 9.7. Acceptance matrix

| AC ID | Requirement | Pass condition |
|---|---|---|
| P7-2E-AC-001 | Complaint not abuse | Complaint route is CSKH/Quality/human, not hide/delete by default. |
| P7-2E-AC-002 | Abuse not lead | Abuse/malicious does not create sales Messenger handoff. |
| P7-2E-AC-003 | Health risk | Health/adverse event suppresses sales and escalates. |
| P7-2E-AC-004 | Privacy | Public response never repeats PII/order/payment. |
| P7-2E-AC-005 | No debate | MC AI Live does not argue publicly. |
| P7-2E-AC-006 | Evidence | Risk/complaint has masked evidence and route trace. |

### 9.8. Smoke extension

| Smoke ID | Scenario | Expected |
|---|---|---|
| P7.2E-SMK-005 | Complaint with phone/order in public | Mask/route private human, no repeated PII. |
| P7.2E-SMK-006 | Health adverse event comment | Sales suppressed, quality/human P0 route. |
| P7.2E-SMK-007 | Spam link repeated | Quarantine/dedup, no host sales cue. |
| P7.2E-SMK-008 | Legal/brand attack | Admin/legal route, no public argument. |

### 9.9. SRS fail conditions

SRS fails if:

- Complaint thật bị gộp vào abuse và bị bỏ qua.
- Abuse/malicious được kéo Messenger để bán.
- MC AI Live tranh luận/đôi co công khai.
- Evidence chứa raw PII.
- Sales cue vẫn xuất hiện trong complaint/health/adverse event.


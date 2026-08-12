# P5.2G - Gateway Smoke / Evidence Report

**Backlog:** `GW-BLG-010`  
**Mode:** `SMOKE_EVIDENCE_REPORT_ONLY`  
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Mục tiêu

Chạy smoke và gom evidence cho Facebook Gateway / Meta Live / Messenger. File này không sửa code, không tạo token, không bật production, không gọi release-ready.

## 2. Entry gate

Chỉ chạy smoke khi:

- P5.2A đến P5.2F có implementation/evidence report.
- AI Advisor Facebook Completion Report `PASS` hoặc smoke được đánh dấu pre-production/staging only.
- Evidence Package Schema đã có.
- Test Page/Sandbox/Staging rõ.
- Secret/token đã mask.

Nếu thiếu, ghi:

`P5-SMOKE-BLOCKED-BY-MISSING_AI_COMPLETION_OR_GATEWAY_IMPLEMENTATION`

## 3. Gateway P0 smoke matrix

| Test ID | Scenario | Expected |
| --- | --- | --- |
| P5-GSMK-001 | Known Commerce Hub event | Resolve Page Registry đúng. |
| P5-GSMK-002 | Unknown page | Reject/quarantine. |
| P5-GSMK-003 | Live comment hỏi giá | Không public final price, tạo handoff. |
| P5-GSMK-004 | Handoff fail | Public không nói đã gửi. |
| P5-GSMK-005 | Public có PII | Không lặp PII. |
| P5-GSMK-006 | Messenger private quote | Bắt buộc AI guard pass và QuoteSnapshot. |
| P5-GSMK-007 | Order success | Bắt buộc CustomerConfirmation và order_code. |
| P5-GSMK-008 | Typing/pacing | Delay/chunk đúng policy hoặc skip safely. |
| P5-GSMK-009 | Duplicate webhook | Không duplicate reply/handoff/order. |
| P5-GSMK-010 | Spam burst | Rate-limit active. |
| P5-GSMK-011 | Complaint thật | Human/CSKH route, no sales. |
| P5-GSMK-012 | Opt-out CRM | Suppressed. |
| P5-GSMK-013 | Ads context | No campaign/ad/ROAS customer-facing. |
| P5-GSMK-014 | ORDER_VERIFIED mapping | ROAS chỉ tính verified revenue. |
| P5-GSMK-015 | Diamond referral | Preserve attribution, no commission before verified order. |
| P5-GSMK-016 | Missing signature | Reject/quarantine. |
| P5-GSMK-017 | Token/secret evidence | Không lộ secret/token. |

## 4. E2E smoke pack

### Smoke 01 - Live comment -> handoff -> Messenger quote

Flow:

1. Live comment hỏi giá.
2. Gateway nhận webhook và signature PASS.
3. Normalized event created.
4. AI Channel nhận event.
5. PublicPrivateSurfaceGuard blocks price.
6. HandoffDecision required.
7. Gateway handoff SENT.
8. Public reply safe ack.
9. Messenger private continuation.
10. QuoteSnapshot created.
11. Private quote rendered.

Expected: không public giá, không kêu khách tự inbox, handoff success mới nói đã chuyển, QuoteSnapshot tồn tại.

### Smoke 02 - Messenger quote -> order

Flow:

1. Customer selects product.
2. Sales session created.
3. Quote cart created.
4. QuoteSnapshot created.
5. Order confirmation draft rendered.
6. Customer confirms.
7. Order created from confirmed quote.
8. order_code returned.

Expected: Order chỉ tạo sau xác nhận, order success chỉ khi có order_code, duplicate command không tạo 2 đơn.

### Smoke 03 - ORDER_VERIFIED -> ROAS / Diamond / CRM

Flow:

1. Order created.
2. Order verified.
3. Attribution mapping created.
4. Diamond commission eligibility checked if referral exists.
5. CRMEligibility checked.
6. ROAS projection updated.

Expected: ROAS chỉ từ ORDER_VERIFIED, Diamond commission chỉ sau ORDER_VERIFIED, CRM suppression đúng.

## 5. Evidence required

Mỗi P0 test cần:

- commit/build/environment.
- masked webhook sample.
- normalized event.
- DecisionEnvelope nếu liên quan AI.
- resolver trace.
- guard trace.
- handoff delivery log nếu handoff.
- QuoteSnapshot nếu báo giá.
- CustomerConfirmation và order_code nếu order.
- ROAS mapping nếu measurement.
- idempotency key nếu state change.
- retry/DLQ log nếu fail/retry.
- owner/QA note nếu manual review.

Evidence không được chứa raw PII, Page Access Token, App Secret, Verify Token, API key, payment secret hoặc cookie/session.

## 6. Dashboard / scale gate metrics

Dashboard phải đo:

- Ads Spend.
- Revenue Verified.
- ROAS.
- CPA.
- AOV.
- Boxes per Order.
- Comment Rate.
- Inbox Rate.
- Quote Rate.
- Order Rate.
- Verified Rate.
- COD Fail Rate.
- Cancel/Return Rate.
- CRM Revenue.
- Diamond Revenue.

Scale gate tối thiểu:

- QuoteSnapshot hoạt động đúng.
- Không leak PII.
- Không spam.
- AOV >= 2 hộp/đơn.
- CPA trong ngưỡng từng giai đoạn.
- ORDER_VERIFIED tracking đúng.
- ROAS đo thật và cao hơn break-even.
- COD fail <= 5%, hoàn/hủy <= 5% hoặc owner accepted blocker rõ.

## 7. Pilot rule

Pilot 7-14 ngày, không scale mạnh ngay.

Theo dõi:

- CPM.
- CPC.
- Comment rate.
- Inbox rate.
- Quote rate.
- Order rate.
- AOV.
- Hộp/đơn.
- CPA.
- ROAS.
- COD fail.
- Hủy/hoàn.
- Diamond lead rate.

## 8. Rollback flags

```yaml
gateway_rollback_flags:
  disable_live_public_reply: true
  disable_private_reply: true
  disable_messenger_handoff: true
  disable_crm_delivery: true
  disable_order_creation_bridge: true
  disable_ads_external_conversion_send: true
```

Rollback không được xóa evidence, không rewrite quote/order history và không sửa log để làm PASS giả.

## 9. Report 14 mục

1. Scope/environment.
2. Source-of-truth checked.
3. Phase entry evidence.
4. Page Registry result.
5. Webhook/signature/normalization result.
6. Public/private boundary result.
7. Messenger handoff result.
8. Final Response Guard enforcement.
9. Delivery pacing/rate-limit/moderation result.
10. Suppression/opt-out/CRM result.
11. Ads/ROAS/Diamond attribution result.
12. Smoke matrix pass/fail/blocked.
13. Blocker/risk/owner decision register.
14. Release verdict.

Release verdict tối đa khi chưa đủ PASS:

`EVIDENCE_SUBMITTED_ONLY__GATEWAY_PRODUCTION_BLOCKED`

## 10. Final status

`PHASE_5_VALIDATION_REPORT_ONLY`

`NOT_RELEASE_READY`

## 11. SRS hardening addendum - Smoke and Evidence Gate

### 11.1. Evidence verdict vocabulary

| Verdict | Meaning | Can claim release? |
| --- | --- | --- |
| `NOT_RUN` | Smoke not executed. | No. |
| `BLOCKED` | Missing dependency/config/evidence. | No. |
| `FAIL` | Executed and failed. | No. |
| `PASS_LOCAL_ONLY` | Local/staging pass, no production proof. | No. |
| `PASS_WITH_EVIDENCE` | Required artifact exists and redacted. | Still not release unless global gate accepts. |
| `ACCEPTED_BY_OWNER` | Owner signed specific evidence and risk. | Only for scoped gate. |

No Phase 5 local file may call `GLOBAL_SMOKE_PASS`, `COMPLETION_PASS`, `RELEASE_READY` or `PRODUCTION_READY` alone.

### 11.2. P0 smoke matrix

| Smoke ID | Scenario | Required proof |
| --- | --- | --- |
| P5-GW-P0-001 | Unknown Page ID inbound event | Event quarantined, no reply. |
| P5-GW-P0-002 | Missing/invalid webhook signature | Rejected, no side effect. |
| P5-GW-P0-003 | Duplicate webhook retry | One normalized event/effect only. |
| P5-GW-P0-004 | Public comment asks price | Safe ack/private handoff, no final price public. |
| P5-GW-P0-005 | Public comment contains phone/address | PII not repeated; private/human route. |
| P5-GW-P0-006 | Handoff fails | Public does not say sent. |
| P5-GW-P0-007 | Messenger thread mismatch/stale | Block/human route. |
| P5-GW-P0-008 | AI response lacks guard trace | No delivery. |
| P5-GW-P0-009 | Quote missing QuoteSnapshot | No final price. |
| P5-GW-P0-010 | Order success missing order_code/confirmation | No order success claim. |
| P5-GW-P0-011 | Sale lock/recall active | Sales reply blocked. |
| P5-GW-P0-012 | Customer opt-out | CRM/proactive outbound blocked. |
| P5-GW-P0-013 | Complaint/refund case open | Human/CSKH route; sales suppressed. |
| P5-GW-P0-014 | Provider rate-limit | Bounded retry/backoff, no spam. |
| P5-GW-P0-015 | App Review/permission pending | Production blocked. |
| P5-GW-P0-016 | Token/secret in evidence attempt | Evidence rejected/redacted. |
| P5-GW-P0-017 | Ads-origin lead | Attribution preserved; no ROAS PASS from lead. |
| P5-GW-P0-018 | Diamond referral before verified order | Referral preserved; no commission calculation. |
| P5-GW-P0-019 | Human takeover active | Automation paused. |
| P5-GW-P0-020 | Kill-switch active | No outbound. |

### 11.3. Evidence package schema

```yaml
GatewaySmokeEvidencePackage:
  package_id: required
  phase: phase-5
  environment: local|staging|pilot|production
  run_started_at: timestamp
  run_finished_at: timestamp
  source_revision_ref: required
  config_snapshot_ref: required_redacted
  page_registry_snapshot_ref: required
  app_review_snapshot_ref: required
  smoke_results:
    - smoke_id: required
      result: pass|fail|blocked|not_run
      artifact_refs: list
      blocker_reason: optional
      redaction_status: pass|fail
  p0_summary:
    pass: integer
    fail: integer
    blocked: integer
    not_run: integer
  owner_decisions: list
  final_verdict: NOT_RELEASE_READY|EVIDENCE_SUBMITTED_ONLY|SCOPED_PASS_WITH_BLOCKERS
```

### 11.4. Artifact requirements

| Artifact | Required for | Redaction rule |
| --- | --- | --- |
| Webhook signature/idempotency logs | P5-GW-P0-001..003 | No raw token/signature secret. |
| Page Registry snapshot | Page identity and permission tests | No token. |
| Public/private rendered text sample | Public/private boundary tests | No raw PII. |
| Handoff state trace | Comment-to-Messenger tests | Redacted thread/customer refs. |
| AI guard trace | AI/Messenger tests | No hidden prompt/secrets. |
| Suppression decision trace | Opt-out/open case/sale lock tests | Redacted customer refs. |
| Rate-limit trace | Anti-spam tests | No provider secret. |
| Owner decision log | Any accepted risk | Decision id, owner, scope, expiry. |

### 11.5. Release verdict rules

`PHASE_5_VALIDATION_REPORT_ONLY` remains the maximum verdict unless:

1. All P0 smoke tests pass with redacted artifacts.
2. App Review/permission status is approved or explicitly not required with evidence.
3. Production page set is scoped and owner-approved.
4. Global evidence gate accepts the package.
5. Release/go-live owner signs the release decision.

If any item is missing, final remains `NOT_RELEASE_READY`.

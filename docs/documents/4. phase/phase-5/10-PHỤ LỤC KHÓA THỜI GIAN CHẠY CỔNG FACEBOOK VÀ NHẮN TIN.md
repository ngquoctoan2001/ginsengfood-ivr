# P5 - PHỤ LỤC KHÓA RUNTIME GATEWAY FACEBOOK VÀ NHẮN TIN

**Ngày viết lại:** 2026-06-05  
**Trạng thái:** `ADDENDUM_REQUIRED_BEFORE_IMPLEMENTATION`  
**Gateway:** `PRODUCTION_BLOCKED`

## 1. Mục đích

Khóa Phase 5 thành Facebook Channel Gateway technical/dev pack. Gateway là lớp channel delivery runtime, không phải AI Advisor, Commerce, Payment, Ads optimizer, CRM worker hoặc Finance/Diamond runtime.

File đọc đầy đủ trước phụ lục này là `BẢNG GÔM GIAI ĐOẠN 5.md`. Nếu phụ lục này và Bảng Gôm có khác nhau về mức chi tiết, Bảng Gôm thắng.

## 2. Production block

Gateway production không được mở nếu thiếu bất kỳ điều kiện nào:

- AI Advisor Facebook Completion Report `PASS`.
- Evidence Package `PASS`.
- All P0 AI Channel gates PASS.
- Live-to-order E2E smoke PASS.
- Owner sign-off PASS.
- Rollback and monitoring READY.

Mặc định: `PRODUCTION_BLOCKED`.

## 3. Boundary matrix

| Việc | Owner | Gateway được làm |
| --- | --- | --- |
| Product/sellable/recall | Phase 1/2 runtime | Preserve event/context, không tự quyết sản phẩm bán được. |
| Giá cuối/QuoteSnapshot | Phase 3 Commerce | Forward/render response đã guard, không tự tính. |
| Order/order_code | Phase 3 Commerce | Handoff confirmed quote, không tự tạo order_code. |
| Payment/paid | Payment Core | Hiển thị public-safe state nếu runtime xác nhận. |
| AI response | Phase 4 AI Advisor | Chỉ delivery command có FinalGuard PASS. |
| CRM lifecycle | CRM canonical/runtime | Enqueue/deliver command đã eligible, không tự chọn lifecycle. |
| Ads/ROAS | Ads + Verified Revenue | Preserve attribution, ROAS chỉ từ ORDER_VERIFIED. |
| Diamond commission | Finance/Diamond canonical | Preserve referral, không tự tính payout. |

## 4. Page Registry lock

```yaml
PageRegistryEntry:
  page_id: required
  page_name: required
  page_role: commerce_hub | live_spoke | ads_spoke | audience_spoke | test
  commerce_hub_page_id: required
  live_allowed: boolean
  messenger_allowed: boolean
  crm_allowed: boolean
  public_reply_allowed: boolean
  private_reply_allowed: boolean
  production_allowed: boolean
  app_id_ref: optional
  page_token_secret_ref: required_if_outbound
  registry_version: required
```

Chỉ Page `Ginsengfood - Cháo Sâm Mỗi Ngày` được xác định là Commerce Hub, Messenger và CRM page theo owner decision hiện tại.

## 5. Channel identity lock

```yaml
ChannelContext:
  source_page_id: required
  commerce_hub_page_id: required
  platform: facebook | messenger | live_comment | post_comment
  page_role: required
  thread_id: required_if_private
  comment_id: required_if_public_comment
  live_session_id: required_if_live
  customer_channel_identity_id: required
  production_allowed: boolean
  page_registry_version: required
```

Nếu `production_allowed=false`, Gateway chỉ dry-run/evidence.

## 6. Public/private response lock

| Channel | Allowed | Forbidden |
| --- | --- | --- |
| Public comment/live | Safe ack, handoff status nếu success, public-safe trust response. | Final price, PII, order, payment, invoice, health-sensitive detail, campaign/ad/ROAS, payout. |
| Messenger/private | Tư vấn sâu và quote/order nếu AI guard + runtime pass. | Tự tính giá/order/paid/commission. |
| CRM outbound | Delivery command đã eligible. | Gửi khi opt-out/open case/quiet hour/fatigue. |

## 7. Handoff lock

Handoff phải là system-initiated action có delivery log. Không dùng câu public kiểu khách tự nhắn Messenger để thay cho handoff thật.

```yaml
HandoffAllowedToSaySent:
  required:
    - handoff_status = SENT
    - delivery_log_id exists
    - public_reply_status = SENT
```

Nếu handoff fail, public reply chỉ safe ack, không nói đã gửi.

## 8. Final Response Guard lock

Gateway chỉ delivery khi có:

1. `FinalResponseGuard.status = PASS`.
2. `CustomerFacingLanguageGuard.status = PASS`.
3. `PublicPrivateScopeGuard.status = PASS`.
4. `SuppressionGuard.status = PASS`.
5. `ChannelRateLimitGuard.status = PASS`.
6. `EvidenceWrite.status = PASS_OR_DEFERRED_BY_POLICY`.

Nếu thiếu guard hoặc guard unavailable, response bị `BLOCKED_OR_HUMAN_HANDOFF`.

## 9. Ads / ROAS / Diamond lock

MUST:

- Preserve campaign/adset/ad/source_page/live_session/comment/thread/quote/order attribution.
- ROAS chỉ tính từ ORDER_VERIFIED.
- COD_FAILED, CANCELLED, RETURNED, TEST_ORDER, DUPLICATE_ORDER bị loại khỏi revenue.
- Diamond referral chỉ preserve đến Finance/Diamond runtime.

MUST NOT:

- Public campaign/ad/ROAS/CPA.
- Tính revenue từ quote/inbox/comment/order_created.
- Tính commission trong Gateway.
- Tính commission trước ORDER_VERIFIED.

## 10. Security lock

- Không ghi Page Access Token, App Secret, Verify Token, API key trong docs/evidence.
- Webhook POST phải signature validation.
- Verify token handshake chỉ dùng cho setup.
- Token storage bằng secret_ref.
- App Review/permission/subscribed fields phải có evidence.
- Token rotation và incident runbook bắt buộc trước production.

## 11. P0 smoke bắt buộc

| Test ID | Scenario | Expected |
| --- | --- | --- |
| P5-ADD-P0-001 | Public comment hỏi giá | Không final quote; handoff private. |
| P5-ADD-P0-002 | Handoff fail | Không nói đã gửi. |
| P5-ADD-P0-003 | Private hỏi giá thiếu QuoteSnapshot | No final price. |
| P5-ADD-P0-004 | Duplicate webhook | Một response/evidence. |
| P5-ADD-P0-005 | Opt-out customer | Suppress outbound. |
| P5-ADD-P0-006 | Complaint thật | Human/support route. |
| P5-ADD-P0-007 | Page registry chưa production | Dry-run only. |
| P5-ADD-P0-008 | ORDER_VERIFIED mapping | ROAS chỉ verified revenue. |
| P5-ADD-P0-009 | Diamond referral before verified order | No commission calculation. |
| P5-ADD-P0-010 | Evidence contains token/secret | FAIL. |

## 12. Done gate

Phase 5 chỉ đủ handoff khi:

1. Page Registry normalized và production_allowed per page.
2. Public/private guard có exact P0 evidence.
3. Gateway không tự quote/order/payment/CRM/commission.
4. Dedup/suppression/moderation pass.
5. Typing/pacing có policy và cancel behavior.
6. Ads/ROAS/Diamond attribution chỉ internal và đo bằng ORDER_VERIFIED.
7. Evidence packet có channel context, guard trace và idempotency trace.
8. Gateway vẫn `PRODUCTION_BLOCKED` đến khi Completion Report + Evidence Package + E2E smoke + owner sign-off PASS.

## 13. Final status

`P5_GATEWAY_RUNTIME_LOCK_REWRITTEN`

`PRODUCTION_BLOCKED`

## 14. SRS hardening addendum - Runtime invariants

### 14.1. Absolute invariants

| Invariant ID | Invariant |
| --- | --- |
| P5-LOCK-001 | Gateway never creates business truth. |
| P5-LOCK-002 | Public surface never leaks private data, final quote, payment, order state, health-sensitive detail, internal SKU, ROAS or commission. |
| P5-LOCK-003 | Messenger private never bypasses AI Final Response Guard, QuoteSnapshot, suppression or owner runtime checks. |
| P5-LOCK-004 | Any active sale lock, recall, opt-out, open complaint, human takeover, App Review block or rate-limit block wins over delivery. |
| P5-LOCK-005 | Duplicate webhook/event never creates duplicate side effects. |
| P5-LOCK-006 | Handoff fail never produces public wording that says handoff was sent. |
| P5-LOCK-007 | Evidence with token/secret/raw PII is invalid evidence. |
| P5-LOCK-008 | Documentation complete is not production ready. |

### 14.2. Production status state machine

```text
SPEC_DRAFT
-> SPEC_REVIEWED
-> IMPLEMENTATION_WAITING
-> IMPLEMENTATION_IN_PROGRESS
-> IMPLEMENTATION_COMPLETE_ONLY
-> LOCAL_SMOKE_WAITING
-> LOCAL_SMOKE_PASS_WITH_BLOCKERS
-> STAGING_EVIDENCE_WAITING
-> STAGING_EVIDENCE_ACCEPTED
-> PILOT_OWNER_REVIEW
-> PILOT_ALLOWED
-> PRODUCTION_REVIEW_WAITING
-> PRODUCTION_APPROVED_SCOPED
```

Forbidden jumps:

- `SPEC_REVIEWED -> PRODUCTION_APPROVED_SCOPED`.
- `LOCAL_SMOKE_PASS_WITH_BLOCKERS -> PRODUCTION_APPROVED_SCOPED`.
- `PILOT_ALLOWED -> GLOBAL_PRODUCTION_READY`.

### 14.3. Kill-switch matrix

| Flag | Default | Effect |
| --- | --- | --- |
| `facebook_gateway_enabled` | false | Blocks all runtime processing except admin/dry-run. |
| `facebook_gateway_webhook_ingest_enabled` | false | Blocks Meta webhook side effects. |
| `facebook_gateway_public_reply_enabled` | false | Blocks public replies. |
| `facebook_gateway_comment_to_messenger_enabled` | false | Blocks private handoff. |
| `facebook_gateway_messenger_delivery_enabled` | false | Blocks Messenger delivery. |
| `facebook_gateway_ai_handoff_enabled` | false | Blocks AI calls. |
| `facebook_gateway_crm_delivery_enabled` | false | Blocks CRM/proactive delivery. |
| `facebook_gateway_ads_attribution_enabled` | false | Blocks ads attribution writes beyond raw event quarantine. |
| `facebook_gateway_diamond_attribution_enabled` | false | Blocks referral attribution writes. |
| `facebook_gateway_production_allowed` | false | Blocks production status regardless of sub-flags. |

### 14.4. Conflict resolution ladder

When rules disagree:

1. Legal/privacy/platform/App Review block wins.
2. Master release/evidence gate wins.
3. Canonical CRM/Finance/Evidence wins for CRM/commission/release.
4. Commerce owner wins for quote/order/payment/verified revenue.
5. Ops/Product/Sale Lock wins for sellable/recall/sale lock.
6. PACK-05/TECH-05 wins for AI behavior and Final Response Guard.
7. PACK-06/TECH-06 wins for Gateway/channel mechanics.
8. Supplementary business/ads/live files inform but do not override runtime owners.

### 14.5. Done vs release

| Status | Allowed meaning |
| --- | --- |
| `SRS_COMPLETE` | Documents are strict enough for dev analysis. |
| `IMPLEMENTATION_COMPLETE_ONLY` | Code path exists, not necessarily smoke/evidence pass. |
| `LOCAL_SMOKE_PASS_WITH_BLOCKERS` | Local tests pass but external/app/evidence gates pending. |
| `PILOT_ALLOWED` | Scoped owner approval for pilot only. |
| `PRODUCTION_APPROVED_SCOPED` | Only a release/go-live owner can set, with accepted evidence. |
| `PRODUCTION_READY` | Not claimable by Phase 5 local docs alone. |

### 14.6. Final lock

The safe final statement for this documentation pack is:

```text
PHASE_5_SRS_READY_FOR_DEV_ANALYSIS
GATEWAY_PRODUCTION_BLOCKED
NOT_RELEASE_READY
```

Any stronger statement requires runtime implementation, smoke, App Review/permission evidence, production-scope owner decision and global evidence acceptance.

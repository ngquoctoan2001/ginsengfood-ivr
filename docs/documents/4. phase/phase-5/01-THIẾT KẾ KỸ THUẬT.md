# P5.1 - Technical Design Only

**Mode:** `TECHNICAL_DESIGN_ONLY`  
**Gateway:** `PRODUCTION_BLOCKED`  
**Ngày viết lại:** 2026-06-05

## 1. Design objective

Khóa thiết kế Facebook Channel Gateway trước implementation. Gateway là channel infrastructure: nhận Meta webhook, normalize event, dedup, kiểm Page policy, route vào AI Advisor, nhận delivery command đã guard pass và ghi delivery/evidence.

Gateway không phải chatbot riêng, không phải AI Advisor, không phải Commerce Runtime, không phải Ads optimizer, không phải CRM worker và không phải Diamond payout engine.

## 2. Gateway boundary

Gateway được làm:

- Meta webhook verify/signature intake.
- Page Registry and channel identity resolution.
- Event normalization and idempotency.
- Live comment ingestion.
- Comment-to-Messenger handoff.
- Messenger thread binding.
- Delivery command execution theo platform policy.
- Delivery log, retry, dead-letter queue.
- Ads/live/page attribution preservation.
- Evidence for Gateway transition.

Gateway bị cấm:

- Tự chọn sản phẩm.
- Tự tính giá/ship/VAT/final payable.
- Tự tạo QuoteSnapshot.
- Tự tạo official order/order_code.
- Tự xác nhận payment/paid.
- Tự gửi CRM campaign.
- Tự tính Diamond commission.
- Bypass AI Advisor Final Response Guard.

## 3. FB-00 đến FB-08 map vào Phase 5

| FB file | Nội dung | File Phase 5 chứa |
| --- | --- | --- |
| FB-00 | README / source-of-truth / Gateway boundary | `09-P5-CHỈ MỤC BÀN GIAO.md`, `10-P5-PHỤ LỤC KHÓA RUNTIME GATEWAY FACEBOOK VÀ NHẮN TIN.md`, file này |
| FB-01 | Meta App / Business / Page Registry / Token Governance | `02-P5-2A` |
| FB-02 | Webhook / Event Normalization / Dedup / Idempotency | `03-P5-2B`, `06-P5-2E` |
| FB-03 | Live Session Registry / Comment -> Messenger Handoff | `03-P5-2B` |
| FB-04 | Messenger Thread / AI Advisor Routing / Hub-Spoke Handoff | `04-P5-2C` |
| FB-05 | Ads Attribution / Pixel / CAPI / Offline Conversion / ORDER_VERIFIED | file này, `08-P5-2G` |
| FB-06 | Diamond / Referral / Commission Attribution | file này, `07-P5-2F`, `08-P5-2G` |
| FB-07 | ROAS / CPA / AOV Dashboard / Scale Gate | file này, `08-P5-2G` |
| FB-08 | Security / App Review / Test / Pilot / Release / Rollback | `06-P5-2E`, `08-P5-2G`, `10-P5-PHỤ LỤC KHÓA RUNTIME GATEWAY FACEBOOK VÀ NHẮN TIN.md` |

## 4. API contract cần khóa

```yaml
gateway_public_apis:
  GET /api/channel/meta/webhook/verify:
    purpose: Meta verify token handshake
  POST /api/channel/meta/webhook:
    purpose: receive Meta webhook with signature validation

gateway_admin_apis:
  POST /api/admin/facebook/pages:
    purpose: create/update page registry entry
  POST /api/admin/facebook/pages/{id}/subscribe:
    purpose: subscribe page webhook fields
  POST /api/admin/facebook/live-sessions:
    purpose: register live session
  GET /api/admin/facebook/events:
    purpose: inspect normalized events
  GET /api/admin/facebook/attribution:
    purpose: inspect attribution chain

internal_integration_apis:
  POST /api/internal/ai-channel/gateway/normalized-event:
    owner: AI Channel
  POST /api/internal/facebook/messenger/handoff:
    owner: Gateway
  POST /api/internal/facebook/delivery-commands:
    owner: Gateway
```

Hard rule: Gateway không có API tạo quote/order/payment/CRM directly.

## 5. Database object contract cần khóa

```yaml
gateway_db_objects:
  - facebook_page_registry
  - meta_app_registry
  - page_token_secret_ref
  - meta_webhook_event
  - normalized_channel_event
  - live_session
  - comment_handoff
  - messenger_thread_binding
  - ads_attribution_context
  - ads_measurement_event
  - order_attribution_snapshot
  - diamond_referral_attribution
  - gateway_outbound_log
  - gateway_dedup_key
  - gateway_retry_queue
  - gateway_dead_letter_event
```

Các object chứa PII hoặc token chỉ lưu restricted/secret_ref. Không lưu token thật trong DB/log rộng.

## 6. Measurement event contract

Event tối thiểu:

- LIVE_VIEW.
- LIVE_COMMENT.
- MESSENGER_STARTED.
- AI_PROPOSAL_SENT.
- QUOTE_CREATED.
- QUOTE_SENT.
- ORDER_CONFIRMATION_SENT.
- ORDER_CREATED.
- ORDER_VERIFIED.
- CRM_REORDER.
- DIAMOND_LEAD_CREATED.

Revenue event chính: `ORDER_VERIFIED`.

Không tính revenue từ:

- comment.
- inbox.
- quote.
- order_created.
- test order.
- duplicate order.
- COD_FAILED.
- CANCELLED.
- RETURNED.

## 7. Pixel / CAPI / Offline Conversion design contract

Chưa được implementation nếu chưa có owner-approved contract:

```yaml
conversion_mapping_contract:
  event_name: required
  destination: internal_only | pixel | capi | offline_conversion
  revenue_source: ORDER_VERIFIED_only
  dedup_key: required
  customer_match_fields_hashed: required_if_external
  retry_policy: required
  failure_log: required
  privacy_review_required: true
```

## 8. Security / App Review design contract

Cần checklist trước production:

- App mode dev/live.
- Permission cần xin và use case từng permission.
- Test user/page.
- Webhook subscribed fields.
- Screencast/evidence App Review.
- Data retention and deletion policy.
- Token rotation.
- Signature validation test.
- Secret storage by secret_ref.

## 9. Feature flags

```yaml
p5_gateway_feature_flags:
  facebook_gateway_webhook_enabled: false
  facebook_gateway_page_registry_enabled: false
  facebook_gateway_live_comment_enabled: false
  facebook_gateway_messenger_handoff_enabled: false
  facebook_gateway_private_reply_enabled: false
  facebook_gateway_delivery_enabled: false
  facebook_gateway_ads_attribution_enabled: false
  facebook_gateway_pixel_capi_enabled: false
  facebook_gateway_crm_bridge_enabled: false
  facebook_gateway_diamond_attribution_enabled: false
  facebook_gateway_production_allowed: false
```

## 10. Done gate

Technical design chỉ DONE khi:

- API/DB/event/feature-flag design có owner review.
- Page Registry design có production_allowed per page.
- Gateway không bypass AI Advisor.
- Ads/ROAS chỉ tính ORDER_VERIFIED.
- Rollback flags có trong design.
- Gateway production vẫn BLOCKED cho đến Completion Report PASS.

## 11. Final status tối đa

`P5_TECHNICAL_DESIGN_HANDOFF_COMPLETED__NOT_RELEASE_READY`

## 12. SRS-grade implementation contract

Phần này là contract kỹ thuật để dev tách SRS và implementation backlog. Nếu có xung đột giữa phần này và các mục trước, mục chặt hơn và fail-closed hơn thắng.

### 12.1. Bounded context

| Context | Owner | Phase 5 được làm | Phase 5 bị cấm |
| --- | --- | --- | --- |
| Meta/Facebook channel adapter | Phase 5 | Webhook intake, Page Registry, channel context, permission state, delivery adapter. | Lưu token thật trong docs/log, bypass App Review, gửi production khi status blocked. |
| AI Advisor | Phase 4 / PACK-05 / TECH-05 | Gọi AI bằng request envelope, nhận response candidate, kiểm Final Response Guard trace. | Tự sinh reasoning/prompt/memory/claim guard riêng. |
| Commerce | Phase 3 | Forward quote/order handoff request đã đủ context. | Tự tính giá, tạo QuoteSnapshot, tạo order_code, xác nhận paid. |
| Ops/Product/Sale Lock | Phase 1/2 | Consume sellable/sale-lock/recall/suppression status. | Tự mở bán SKU khi upstream chặn. |
| CRM/Member | Canonical CRM | Deliver command đã eligible nếu channel policy cho phép. | Tự chọn lifecycle, tự gửi khi opt-out/open case/quiet hour. |
| Ads/ROAS | PACK-07 / TECH-07 | Preserve utm/ad/campaign identifiers and handoff raw funnel evidence. | Tự tính ROAS từ comment/inbox/quote/order draft. |
| Finance/Diamond | Canonical Finance | Preserve referral/channel attribution. | Tự tính commission/payable/payout. |

### 12.2. Logical modules

| Module ID | Module | Responsibilities | Required failure behavior |
| --- | --- | --- | --- |
| P5-M01 | Gateway Orchestrator | Điều phối thứ tự Page -> webhook -> context -> AI -> guard -> delivery -> evidence. | Fail closed, no outbound. |
| P5-M02 | Meta App Registry | Lưu app/business/page role, permission, app review status, secret references. | Unknown/quarantine. |
| P5-M03 | Webhook Intake | Verify token, signature, parse, idempotency key, replay block. | Reject/quarantine; no side effect. |
| P5-M04 | Channel Context Resolver | Resolve page, live session, comment, messenger thread, source campaign. | Unknown context -> no production delivery. |
| P5-M05 | Public Comment Policy | Classify intent and allowed public action. | PII/price/payment/order/health-sensitive -> private/human. |
| P5-M06 | Comment-to-Messenger Handoff | Create private handoff only when policy and permission pass. | If fail, public must not claim message was sent. |
| P5-M07 | Messenger Runtime | Bind thread/customer/session and route to AI/human. | Mismatch/stale thread -> human/block. |
| P5-M08 | AI Handoff Client | Call AI Advisor with complete channel context. | AI unavailable -> no fabricated reply. |
| P5-M09 | Final Response Guard Consumer | Enforce AI response guard, public-safe view and owner runtime references. | Guard fail/pending -> block delivery. |
| P5-M10 | Delivery Adapter | Send public reply/private message with pacing, retry and audit. | Rate-limited/transient fail -> bounded retry/dead-letter. |
| P5-M11 | Moderation and Anti-spam | Detect spam/abuse/complaint/duplicate/unsafe content. | No sales lead from spam/abuse. |
| P5-M12 | Suppression Resolver | Merge opt-out, open case, sale lock, recall, quiet hour, fatigue. | Any active blocker wins over send. |
| P5-M13 | Attribution Recorder | Store source/referral/ad/live identifiers as evidence. | Do not compute revenue/commission. |
| P5-M14 | Evidence Runtime | Write trace for decisions, delivery, guards and smoke. | Missing evidence -> no PASS. |

### 12.3. Canonical data contracts

```yaml
FacebookPageRegistryEntry:
  page_registry_id: required
  meta_object_id: required
  meta_object_type: page|profile|business|app|unknown
  display_name: required
  business_id_ref: optional_redacted
  app_id_ref: optional_redacted
  token_secret_ref: required_if_delivery_enabled
  page_role: commerce_hub|live_spoke|cskh_spoke|audience_spoke|test|unknown
  live_allowed: boolean
  public_reply_allowed: boolean
  private_reply_allowed: boolean
  crm_delivery_allowed: boolean
  production_status: blocked|pilot|enabled|disabled
  permission_status: missing|pending|approved|rejected|expired|unknown
  app_review_status: not_required|pending|approved|rejected|unknown
  evidence_refs: list
  last_reviewed_at: timestamp

NormalizedMetaEvent:
  normalized_event_id: required
  source_platform: facebook
  event_type: page_comment|live_comment|messenger_message|postback|delivery_receipt|unknown
  raw_event_ref: required_redacted
  event_time: timestamp
  received_at: timestamp
  page_registry_id: required
  user_channel_ref: required_redacted
  comment_id: optional
  parent_comment_id: optional
  messenger_thread_id: optional
  live_session_id: optional
  campaign_ref: optional
  idempotency_key: required
  replay_status: first_seen|duplicate|replay_blocked
  normalization_status: normalized|quarantined|rejected

GatewayDecisionEnvelope:
  decision_id: required
  normalized_event_id: required
  channel_context_id: required
  surface: public_comment|live_comment|messenger_private|page_inbox|internal_test
  intent_class: price|buy|product_advice|complaint|spam|abuse|pii|health_sensitive|support|unknown
  public_policy: safe_ack_allowed|private_required|human_required|blocked
  ai_handoff_status: not_required|requested|succeeded|failed|blocked
  final_response_guard_status: not_required|pass|fail|pending|blocked
  suppression_status: pass|blocked|deferred
  delivery_decision: send|defer|human_route|block|dry_run
  blocked_reasons: list
  evidence_refs: list
```

### 12.4. API surface SRS

| API | Auth | Purpose | Hard rule |
| --- | --- | --- | --- |
| `GET /api/channel/meta/webhook/verify` | Verify token only | Meta verification challenge. | Token compared by secret ref; never log raw token. |
| `POST /api/channel/meta/webhook` | Meta signature | Receive events and enqueue normalization. | No outbound side effect before signature/idempotency pass. |
| `GET /api/admin/facebook/pages` | Admin | List registry + permission/app review status. | Redact secrets. |
| `POST /api/admin/facebook/pages` | Admin | Create/update page registry. | Default `production_status=blocked`. |
| `POST /api/admin/facebook/pages/{id}/subscribe` | Admin | Register subscribed fields. | Requires permission/app review evidence reference. |
| `POST /api/admin/facebook/live-sessions` | Admin | Bind live session to page/product/script context. | Live session cannot open SKU outside upstream scope. |
| `GET /api/admin/facebook/events` | Admin/support | Inspect normalized events and quarantine. | Redact PII and raw payload secrets. |
| `POST /api/internal/facebook/comment-handoffs` | Internal | Create comment-to-Messenger handoff. | Only after public/private policy pass. |
| `POST /api/internal/facebook/delivery-commands` | Internal | Execute guarded delivery command. | Requires DecisionEnvelope and guard/suppression pass. |
| `GET /api/admin/facebook/evidence/{id}` | Admin/audit | Read evidence package. | No token/secret/raw PII exposure. |

Gateway APIs must not expose create quote/order/payment/commission endpoints.

### 12.5. Database object SRS

| Object | Required uniqueness | Retention/security | Notes |
| --- | --- | --- | --- |
| `facebook_page_registry` | `meta_object_id` + `meta_object_type` | secret refs only | Production blocked by default. |
| `facebook_permission_evidence` | page/app/permission/version | evidence refs, no tokens | Stores App Review and permission proof. |
| `facebook_normalized_events` | `idempotency_key` | raw ref redacted | Duplicate-safe source of normalized events. |
| `facebook_channel_contexts` | event/thread/session scoped | PII minimized | Links page, live, comment, messenger, campaign. |
| `facebook_comment_handoffs` | `comment_id` + `target_thread_id` | audit required | Tracks pending/sent/failed/blocked. |
| `facebook_messenger_sessions` | page + psid/thread | privacy scoped | Stale/mismatch sessions block. |
| `facebook_delivery_commands` | `decision_id` | immutable decision snapshot | Command must be derived from guard pass. |
| `facebook_delivery_logs` | provider message id or command id | redact payload | Includes retry/dead-letter. |
| `facebook_suppression_snapshots` | decision id | immutable | Proves blocker state at decision time. |
| `facebook_gateway_evidence` | evidence id | no secrets | Smoke and handoff evidence. |

### 12.6. State transitions

```text
META_EVENT_RECEIVED
-> SIGNATURE_VERIFIED
-> NORMALIZED
-> CHANNEL_CONTEXT_RESOLVED
-> POLICY_CLASSIFIED
-> AI_HANDOFF_REQUESTED
-> FINAL_RESPONSE_GUARD_EVALUATED
-> SUPPRESSION_EVALUATED
-> DELIVERY_SCHEDULED
-> DELIVERY_ATTEMPTED
-> DELIVERY_SENT | DELIVERY_FAILED | DELIVERY_BLOCKED | HUMAN_ROUTED | DEAD_LETTERED
```

Invalid transitions:

- `META_EVENT_RECEIVED -> DELIVERY_SENT`.
- `NORMALIZED -> ORDER_CREATED`.
- `POLICY_CLASSIFIED(public) -> FINAL_PRICE_PUBLIC`.
- `AI_HANDOFF_REQUESTED -> DELIVERY_SENT` without guard pass.
- `SUPPRESSION_BLOCKED -> DELIVERY_SENT`.
- `DELIVERY_FAILED -> PUBLIC_SAYS_SENT`.

### 12.7. Non-functional requirements

| NFR ID | Requirement |
| --- | --- |
| P5-NFR-001 | Idempotency must make duplicate webhook delivery side-effect-free. |
| P5-NFR-002 | Signature/replay failure must be observable but must not expose payload secrets. |
| P5-NFR-003 | PII redaction applies to logs, screenshots, evidence and admin search. |
| P5-NFR-004 | Delivery retry must be bounded and rate-limit aware. |
| P5-NFR-005 | Public reply latency may be optimized only after safety and platform policy pass. |
| P5-NFR-006 | Manual human takeover must pause automation for the affected thread/scope. |
| P5-NFR-007 | All production flags default false/blocked in new environment. |
| P5-NFR-008 | Observability must include correlation id, decision id, event id and evidence id. |

### 12.8. Implementation definition of done

`P5_TECHNICAL_DESIGN_HANDOFF_COMPLETED__NOT_RELEASE_READY` chỉ được chuyển sang implementation-ready khi dev tạo được:

1. API contract hoặc OpenAPI delta cho các endpoint nội bộ/admin cần có.
2. DB migration plan với secret refs, unique keys và redaction boundaries.
3. Event/schema contract cho normalized event, decision envelope, delivery command, evidence.
4. Service ownership map cho P5-M01 đến P5-M14.
5. Unit/integration/smoke plan cho P0 gates.
6. Rollback flags và kill-switch.
7. Owner decision register cho Page/App/permission/App Review.
8. Evidence template không chứa token/secret/PII thật.


# P7.1 - Technical Design Only

**PROMPT-P7.1 - MC AI LIVE TECHNICAL DESIGN HANDOFF**

## Mode

TECHNICAL DESIGN ONLY. Khong implement live runtime, khong bat live auto-reply, khong tao script production.

## Input Bat Buoc

- P7 Analysis report.
- P5 Gateway evidence.
- P4 AI/Final Response Guard evidence.
- P3 Commerce evidence.
- P6 Ads signal/Verified Revenue boundary evidence neu dung ads-safe orchestration.

Neu thieu Gateway evidence, ghi `BLOCKED_NEED_PHASE5_GATEWAY_EVIDENCE`.

## Design Boundary

MC AI Live la live orchestration assistant. No chi:

- Resolve live session context.
- Goi y script/nhiep noi/CTA.
- Goi y public-safe comment handling.
- Coordinate Messenger handoff qua Gateway.
- Respect Product Knowledge / Claim Guard / Final Response Guard.
- Respect Commerce QuoteSnapshot / Order / Payment boundary.
- Respect Ads signal boundary.

MC AI Live khong:

- Tu tinh gia/discount/ton kho.
- Tu tao order/payment/revenue.
- Tu noi claim y te chua approved.
- Tu dung live signal lam revenue/ROAS.

## Workstreams

| Slice | Backlog | Muc tieu |
|---|---|---|
| P7.2A | LIVE-BLG-001 | Live Session Context Resolver |
| P7.2B | LIVE-BLG-002/003 | Live Script Runtime + Script/Claim Guard |
| P7.2C | LIVE-BLG-004/005 | Comment Classifier + Messenger Coordination |
| P7.2D | LIVE-BLG-006 | Product/Price Boundary In Live |
| P7.2E | LIVE-BLG-007/008 | Troll/Abuse + Complaint/CSKH Route |
| P7.2F | LIVE-BLG-009 | Ads-safe Live Signal Boundary |
| P7.2G | LIVE-BLG-010 | Smoke / Evidence Pack |

## Contract Can Khoa

- Live session context.
- Live product board reference.
- Script runtime request/response.
- Claim guard decision.
- Comment classification result.
- Messenger handoff command.
- Commerce quote boundary.
- Ads-safe signal event.
- Suppression decision.
- Evidence record.

## API/DTO Impact

Moi API/DTO/event/OpenAPI thay doi phai ghi FE/operator console/channel/ads dashboard impact.

## P0 Design Blockers

- Live tu bao gia.
- Live tu tao order/payment/revenue.
- Script chua guard van duoc dung.
- Claim chua approved van noi.
- Live signal dung lam ROAS.
- Troll/malicious keo Messenger sai policy.
- Complaint that bi xu nhu troll.

## Final Status

**P7 TECHNICAL DESIGN HANDOFF COMPLETED - NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 10. SRS-grade implementation contract

### 10.1. Bounded context

Phase 7 chỉ thiết kế `MC AI Live` trong business-platform. Nó nhận live/session/comment/signal context từ Gateway và các runtime guard, tạo cue an toàn cho host/operator hoặc tạo yêu cầu handoff/private review. Nó không phát ngôn trực tiếp ra khách nếu chưa qua Gateway và Final Response Guard.

```yaml
bounded_context: MC_AI_LIVE
runtime_role: host_operator_assistant
may_create:
  - LivePlan
  - HostCue
  - LiveRiskCue
  - PrivateHandoffCue
  - LiveMeasurementEvent
may_request:
  - GatewayCommentToMessengerHandoff
  - CommerceQuoteSnapshot
  - HumanTakeover
  - EvidenceRecord
must_not_create:
  - QuoteSnapshot
  - OfficialOrder
  - PaymentConfirmation
  - VerifiedRevenue
  - AdsScaleDecision
  - GatewayDeliveryMessage
```

### 10.2. Module contract map

| Module | SRS ID | Contract/API chính | Owner boundary |
|---|---|---|---|
| Live Session Context Resolver | P7-M01 | `LivePlan`, live session context | TECH-08 + Gateway |
| Live Readiness Guard | P7-M02 | `LivePlan.release_status`, evidence refs | TECH-10/PACK-10 |
| Product Scope Resolver | P7-M03 | `LivePlan.product_scope_refs` | Product/Commerce/Ops |
| Host Cue Generator | P7-M04 | `HostCue` | TECH-08 |
| Script/Claim Guard | P7-M05 | `HostCue.claim_guard_result_id`, `LiveRiskCue` | TECH-05/TECH-08 |
| Public Response Suggestion | P7-M06 | Host cue only, not delivery | TECH-06 Gateway |
| Comment Intent Awareness | P7-M07 | Channel comment + classifier result | TECH-06 |
| Private Handoff Cue | P7-M08 | `PrivateHandoffCue`, `CommentToMessengerHandoff` | TECH-06 |
| Commerce-safe Live Selling Guard | P7-M09 | `quote_snapshot_id` reference only | TECH-04 |
| Moderation/Risk Cue | P7-M10 | `LiveRiskCue` | Gateway/CSKH/Quality |
| Ads-safe Signal Handoff | P7-M11 | `LiveMeasurementEvent` | TECH-07 |
| Evidence Collector | P7-M12 | `EvidenceRef`, `EvidenceRegistry` | TECH-10 |
| Rehearsal/UAT Gate | P7-M13 | Smoke report | Owner/QA |
| Release Status Guard | P7-M14 | Release status ref | Master/TECH-10 |

### 10.3. API surface được phép

| Operation | Path | SRS usage | Hard block |
|---|---|---|---|
| `createLivePlanV1` | `POST /v1/live/plans` | Tạo live plan bounded bởi product scope, approved claims, guard readiness. | Không tạo plan nếu thiếu live session/page/product scope/evidence. |
| `createHostCueV1` | `POST /v1/live/host-cues` | Tạo cue cho host/operator. | Không tạo cue có final price nếu thiếu `quote_snapshot_id`; không cue SKU locked/recalled. |
| `createLiveRiskCueV1` | `POST /v1/live/risk-cues` | Ghi risk cue khi claim, privacy, complaint, recall, sale lock, fake urgency risk xuất hiện. | Không dùng risk cue để tranh luận public hoặc bán tiếp. |
| `createPrivateHandoffCueV1` | `POST /v1/live/private-handoff-cues` | Tạo cue kéo về private theo Gateway. | Không nhắn riêng bypass Gateway; không claim handoff success nếu Gateway delivery fail. |

Nếu dev cần endpoint khác, phải ghi `CONTRACT_MISSING` và sửa OpenAPI/schema trước khi implement.

### 10.4. Data contract references

| Object | Path | Bắt buộc trong SRS |
|---|---|---|
| Live Plan | `schemas/live/live-plan.schema.json` | `live_plan_id`, `live_session_id`, `product_scope_refs`, guard pass flags, evidence refs. |
| Host Cue | `schemas/live/host-cue.schema.json` | `host_cue_id`, `live_plan_id`, `cue_type`, `cue_text`, `sku_id`, `quote_snapshot_id`, `allowed_to_present`. |
| Live Risk Cue | `schemas/live/live-risk-cue.schema.json` | `risk_type`, `severity`, `suppress_sales`, `handoff_required`. |
| Private Handoff Cue | `schemas/live/private-handoff-cue.schema.json` | `source_comment_id`, `comment_to_messenger_handoff_id`, `status`, `public_safe`. |
| Live Measurement Event | `schemas/live/live-measurement-event.schema.json` | `not_revenue_by_itself=true`, optional `verified_revenue_link_id`. |
| Comment-to-Messenger Handoff | `schemas/channel/comment-to-messenger-handoff.schema.json` | `handoff_id`, `status`, `private_handoff_status`, `context_preserved`. |
| Quote Snapshot | `schemas/commerce/quote-snapshot.schema.json` | Reference only; source for final price. |
| Verified Revenue | `schemas/commerce/verified-revenue.schema.json` | Reference only; source for ROAS. |
| Sale Lock | `schemas/ops/sale-lock.schema.json` | Blocks live sales cue. |

### 10.5. State model đề xuất cho SRS

```yaml
live_plan_state:
  DRAFT:
    next: [READY_FOR_GUARD_CHECK, CANCELLED]
  READY_FOR_GUARD_CHECK:
    next: [GUARD_PASS, GUARD_FAIL, EVIDENCE_WAITING]
  GUARD_PASS:
    next: [REHEARSAL_READY, EVIDENCE_WAITING]
  GUARD_FAIL:
    next: [FIX_REQUIRED, CANCELLED]
  REHEARSAL_READY:
    next: [REHEARSAL_PASSED, REHEARSAL_FAILED]
  REHEARSAL_PASSED:
    next: [OWNER_REVIEW_REQUIRED]
  OWNER_REVIEW_REQUIRED:
    next: [EVIDENCE_ACCEPTED, EVIDENCE_REJECTED]
  EVIDENCE_ACCEPTED:
    next: [LIVE_READY_REVIEW_ONLY]
  LIVE_READY_REVIEW_ONLY:
    invariant: "không phải production go-live nếu Global Gateway BLOCKED"
```

`LIVE_READY_REVIEW_ONLY` không được render thành `LIVE_READY=true`; nó chỉ là trạng thái review nội bộ.

### 10.6. Guard chain bắt buộc

Mọi cue/script/comment action phải đi qua chain sau, theo thứ tự:

1. `LiveSessionResolver`.
2. `LivePlanResolver`.
3. `ProductScopeResolver`.
4. `SellableGuard`.
5. `RecallSaleLockGuard`.
6. `ClaimGuard`.
7. `PrivacyPIIGuard`.
8. `FakeUrgencyScarcityGuard`.
9. `CommerceQuoteBoundaryGuard` nếu có giá/mua/chốt.
10. `GatewayPublicPrivateGuard` nếu liên quan public/private.
11. `AdsSignalBoundaryGuard` nếu tạo measurement signal.
12. `EvidenceGateGuard` trước khi claim completion/release.

Guard fail phải tạo `blocked_actions` và reason code; không được im lặng fallback sang lời bịa.

### 10.7. Persistence/audit requirement

SRS phải yêu cầu lưu tối thiểu:

| Record | Required fields |
|---|---|
| `live_plan_record` | `live_plan_id`, `version`, `live_session_id`, `plan_status`, `product_scope_refs`, guard pass flags, evidence refs, audit refs. |
| `host_cue_record` | `host_cue_id`, `live_plan_id`, `cue_type`, `cue_text`, `allowed_to_present`, guard result, source refs. |
| `risk_cue_record` | `live_risk_cue_id`, `risk_type`, `severity`, `suppress_sales`, `handoff_required`, evidence refs. |
| `handoff_cue_record` | `private_handoff_cue_id`, `source_comment_id`, `comment_to_messenger_handoff_id`, status, public ack status. |
| `measurement_event_record` | `live_measurement_event_id`, `event_type`, `funnel_event_id`, `not_revenue_by_itself`, evidence refs. |

Không lưu token Meta, App Secret, raw PII rộng, số điện thoại/địa chỉ không mask trong evidence package.

### 10.8. Acceptance baseline

Một SRS Phase 7 được xem là đủ để dev triển khai khi có đủ:

- Sơ đồ module P7-M01 -> P7-M14.
- Contract refs cho mọi DTO/API.
- State model live plan, host cue, handoff cue, risk cue, measurement event.
- Negative path cho no QuoteSnapshot, sale lock, recall, claim fail, handoff fail, runtime unavailable.
- Smoke matrix P0 từ `P7-GSMK-001` trở lên.
- Evidence schema và owner decision IDs.
- Final verdict vẫn là `NOT RELEASE READY` và `GLOBAL GATEWAY: BLOCKED`.

### 10.9. SRS no-conflict checklist

| Check | PASS rule |
|---|---|
| Price | Không có câu nào cho MC AI Live tự tính/báo final price. |
| Order | Không có câu nào cho MC AI Live tạo official order hoặc order code. |
| Payment | Không có câu nào cho MC AI Live xác nhận paid/revenue. |
| Ads | Không có câu nào dùng live signal làm ROAS/revenue. |
| Gateway | Không có câu nào bypass Gateway để gửi public/private. |
| Claim | Không có câu nào bypass ClaimGuard/FinalGuard. |
| Privacy | Không có public PII/order/payment/member/ad internal data. |
| Release | Không có `LIVE_READY`, `RELEASE_READY`, `PRODUCTION_READY` nếu thiếu accepted evidence. |


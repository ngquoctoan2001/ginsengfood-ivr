# P7.2A - Live Session Context Resolver

**BACKLOG:** LIVE-BLG-001  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Khoa live session context de MC AI Live biet live nao, page nao, product board nao, host/operator nao va suppression nao dang active.

## Entry Gate

- P7.1 design accepted.
- P5 Gateway channel/page/session evidence pass.
- Live board/source accepted.

## Implementation Scope

- Live session identity.
- Page/channel binding.
- Live product board binding.
- Host/operator context.
- Suppression context.
- Evidence logging.

## Not Allowed

- Hardcode live session/product board.
- Dung live session unknown de tao script.
- Bo qua recall/sale lock/suppression.
- Public internal board/private data.

## Acceptance Criteria

- Moi live command co live_session_id/page/channel/context.
- Unknown session bi reject.
- Product board co source/evidence.
- Suppression state duoc resolve truoc script/comment suggestion.

## Smoke

| Smoke ID | Scenario | Expected |
|---|---|---|
| P7.2A-SMK-001 | Known live session | Context resolved |
| P7.2A-SMK-002 | Unknown session | Reject/quarantine |
| P7.2A-SMK-003 | Missing board | No script generation |
| P7.2A-SMK-004 | Sale lock active | Sales suggestion blocked |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 9. SRS hardening addendum - Live Session Context Resolver

### 9.1. Purpose lock

`LIVE-BLG-001` phải khóa được một câu hỏi duy nhất: một live command/cue/comment có đang thuộc một live session hợp lệ, đúng page/channel, đúng live plan, đúng product scope, và không bị suppression hay runtime hold hay không.

Không có context hợp lệ thì Phase 7 không được sinh script, host cue, handoff cue, measurement event hoặc public response suggestion.

### 9.2. Input contract

```yaml
live_session_context_request:
  correlation_id: string
  idempotency_key: string
  source_platform: FACEBOOK
  source_page_id: string
  channel_context_id: string
  live_session_id: string
  live_plan_id: string?
  comment_id: string?
  operator_id: string?
  host_id: string?
  requested_action:
    enum:
      - RESOLVE_LIVE_CONTEXT
      - CREATE_HOST_CUE
      - CLASSIFY_LIVE_COMMENT
      - CREATE_PRIVATE_HANDOFF_CUE
      - CREATE_LIVE_MEASUREMENT_SIGNAL
  source_refs:
    - type: LIVE_PLAN | PRODUCT_SCOPE | GATEWAY_EVENT | OWNER_DECISION
      ref_id: string
```

Required validation:

| Field | Rule |
|---|---|
| `correlation_id` | Bắt buộc để trace sang Gateway/Commerce/Ads/evidence. |
| `idempotency_key` | Bắt buộc để không tạo cue/handoff duplicate. |
| `source_page_id` | Phải map Page Registry/Gateway config đã accepted. |
| `live_session_id` | Không được unknown hoặc stale. |
| `live_plan_id` | Bắt buộc nếu action tạo cue/script/measurement. |
| `requested_action` | Phải nằm trong allowlist; không có action gửi tin trực tiếp. |

### 9.3. Output contract

```yaml
live_session_context_result:
  context_status:
    enum:
      - RESOLVED
      - UNKNOWN_SESSION
      - PAGE_NOT_ALLOWED
      - LIVE_PLAN_MISSING
      - LIVE_PLAN_NOT_READY
      - PRODUCT_SCOPE_MISSING
      - SUPPRESSION_ACTIVE
      - SALE_LOCK_OR_RECALL_ACTIVE
      - RUNTIME_UNAVAILABLE
      - OWNER_REVIEW_REQUIRED
  live_session_id: string
  live_plan_id: string?
  page_binding_status: PASS | FAIL | WAITING
  product_scope_status: PASS | FAIL | WAITING
  gateway_status: PASS | FAIL | WAITING | BLOCKED
  suppression_summary:
    sale_lock_active: boolean
    recall_active: boolean
    gateway_suppression_active: boolean
    claim_or_privacy_hold_active: boolean
  allowed_actions:
    - CREATE_HOST_CUE
    - CREATE_PRIVATE_HANDOFF_CUE
    - CREATE_RISK_CUE
    - CREATE_MEASUREMENT_SIGNAL
  blocked_actions:
    - action: string
      reason_code: string
  evidence_refs: []
  audit_refs: []
```

### 9.4. LivePlan binding

Resolver phải đọc/ghi theo `schemas/live/live-plan.schema.json`:

| LivePlan field | SRS rule |
|---|---|
| `live_plan_id` | Không được null khi tạo cue. |
| `live_session_id` | Phải khớp request context. |
| `product_scope_refs` | Phải có ít nhất một ref accepted nếu cue bán hàng. |
| `stock_guard_pass` | `false` thì không có CTA bán hàng. |
| `recall_sale_lock_guard_pass` | `false` thì không có CTA bán hàng. |
| `claim_guard_result_id` | Bắt buộc trước script/product story cue. |
| `privacy_guard_pass` | `false` thì không public/private cue. |
| `fake_urgency_guard_pass` | `false` thì block host cue. |
| `no_fake_urgency` | Luôn phải là `true`. |

### 9.5. Readiness decision

```yaml
readiness_decision:
  READY_FOR_CONTEXT_USE:
    allowed_when:
      - live_session_id known
      - page_binding PASS
      - live_plan exists
      - product_scope exists for sales cue
      - no active suppression for requested scope
  HOLD_FOR_MISSING_PLAN:
    when:
      - live_plan_id missing
      - live_plan status not acceptable
  HOLD_FOR_RUNTIME:
    when:
      - Gateway unavailable
      - Commerce unavailable when price/order intent is involved
      - ClaimGuard unavailable when script/claim is involved
  BLOCK_FOR_SALE_LOCK_RECALL:
    when:
      - recall active
      - sale lock active
      - sellable false
```

### 9.6. Negative path requirements

| Scenario | Required output |
|---|---|
| Unknown live session | `context_status=UNKNOWN_SESSION`, block all cue/script/handoff/measurement creation. |
| Known session but missing plan | `LIVE_PLAN_MISSING`, allow only owner/operator review, no host cue. |
| Plan exists but product scope missing | Block sales cue; allow non-sales operational cue only if public-safe. |
| Sale lock active | `SALE_LOCK_OR_RECALL_ACTIVE`, block CTA, create `LiveRiskCue` if needed. |
| Gateway unavailable | `RUNTIME_UNAVAILABLE`, no public/private action. |
| Commerce unavailable and price intent | No QuoteSnapshot request/cue; safe handoff only if Gateway permits. |

### 9.7. Evidence required

| Evidence ID | Required artifact |
|---|---|
| P7-2A-EVD-001 | Known live session context result with page/live/plan binding. |
| P7-2A-EVD-002 | Unknown live session rejection trace. |
| P7-2A-EVD-003 | Missing live plan hold trace. |
| P7-2A-EVD-004 | Sale lock/recall active suppression trace. |
| P7-2A-EVD-005 | Runtime unavailable fail-safe trace. |
| P7-2A-EVD-006 | Idempotency duplicate command trace. |

### 9.8. Acceptance matrix

| AC ID | Requirement | Pass condition |
|---|---|---|
| P7-2A-AC-001 | Context resolution | Every action has `correlation_id`, `idempotency_key`, `live_session_id`, `source_page_id`. |
| P7-2A-AC-002 | Plan binding | Cue/script action requires valid `live_plan_id`. |
| P7-2A-AC-003 | Product scope | Sales/product cue requires accepted product scope. |
| P7-2A-AC-004 | Suppression | Sale lock/recall/suppression blocks sales cue. |
| P7-2A-AC-005 | No direct delivery | Resolver never sends public reply or Messenger message. |
| P7-2A-AC-006 | Evidence | Every block/pass decision has evidence/audit refs. |

### 9.9. SRS handoff output

Dev handoff for this slice must include:

- Current code map or `RUNTIME_UNKNOWN`.
- Contract impact list.
- Any needed schema/OpenAPI delta.
- Resolver pseudocode.
- Negative-path smoke.
- Evidence fixture examples.
- Final verdict: `LIMITED IMPLEMENTATION REPORT ONLY`, `NOT RELEASE READY`, `GLOBAL GATEWAY: BLOCKED`.


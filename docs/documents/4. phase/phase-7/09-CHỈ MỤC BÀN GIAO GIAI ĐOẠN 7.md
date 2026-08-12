# P7 - README HANDOFF INDEX

## Update 2026-05-22 - Phase 7 Plan

Dung thu muc chinh thuc: `docs/documents/4. phase/phase-7/`.

Phase 7 la MC AI Live / Live Script Runtime / Live Sales Control / Ads-safe Live Orchestration. MC AI Live la assistant cho host/operator, khong phai source-of-truth ve san pham, gia, ton, order, payment, revenue hay ROAS.

## Entry Gate

Chi duoc bat dau Phase 7 khi:

- Phase 5 Gateway public/private, comment classifier va Messenger handoff co evidence.
- Phase 4 AI Advisor / Final Response Guard co evidence.
- Phase 3 QuoteSnapshot / Order / Payment boundary co evidence.
- Phase 6 Ads signal / Verified Revenue boundary co evidence neu dung ads-safe live orchestration.

Neu chua du, ghi `P7-ENTRY-BLOCKED-BY-GATEWAY-AI-COMMERCE`.

**Global Gateway:** `BLOCKED` den TECH-10.

## Source Of Truth

- TECH-08 MC AI Live.
- PACK-08 MC AI Live.
- TECH-06 Facebook Gateway.
- TECH-07 Ads Measurement.
- TECH-05 AI Advisor.
- TECH-04 Commerce Runtime.
- TECH-11/12 phase governance/backlog.

## Execution Order

1. `00-P7-ANALYSIS-ONLY.md`: inspect live/script/comment/runtime code that.
2. `01-P7-1-TECHNICAL-DESIGN-ONLY.md`: khoa MC AI Live contract.
3. `02-P7-2A-LIVE-SESSION-CONTEXT-RESOLVER.md`: LIVE-BLG-001.
4. `03-P7-2B-LIVE-SCRIPT-RUNTIME-CLAIM-GUARD.md`: LIVE-BLG-002/003.
5. `04-P7-2C-LIVE-COMMENT-CLASSIFIER-MESSENGER-COORDINATION.md`: LIVE-BLG-004/005.
6. `05-P7-2D-PRODUCT-PRICE-BOUNDARY-IN-LIVE.md`: LIVE-BLG-006.
7. `06-P7-2E-TROLL-COMPLAINT-MODERATION-ROUTE.md`: LIVE-BLG-007/008.
8. `07-P7-2F-ADS-SAFE-LIVE-SIGNAL-BOUNDARY.md`: LIVE-BLG-009.
9. `08-P7-2G-SMOKE-EVIDENCE-REPORT.md`: LIVE-BLG-010.

## Core Rules

- MC AI Live khong tu bao gia, khong tu tinh discount, khong tu xac nhan ton.
- MC AI Live khong tu tao order/order_code.
- MC AI Live khong tu xac nhan payment/revenue.
- No QuoteSnapshot -> no final price.
- Script/claim phai qua guard.
- Public/private boundary do Gateway enforce.
- Live signal khong phai revenue/ROAS.
- Sale Lock / Recall / Suppression thang live script/CTA.

## P0 Blockers

- MC AI Live tu bao gia.
- MC AI Live tu tao order.
- MC AI Live tu xac nhan payment/revenue.
- Live signal dung lam ROAS.
- Script chua guard van dung.
- Claim chua approved van noi.
- Troll/malicious keo Messenger sai policy.
- Complaint that bi xu nhu troll.

## Downstream

Phase 8 IVR chi nen bat dau sau khi Commerce Official Order boundary va channel handoff du evidence. Phase 7 smoke pass khong mo Gateway va khong goi production readiness.

**NOT LIVE READY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 9. SRS hardening addendum - Dev handoff index

### 9.1. Required reading order for dev

Dev must read in this order:

1. `00-P7-ANALYSIS-ONLY.md`.
2. `01-P7-1-TECHNICAL-DESIGN-ONLY.md`.
3. `docs/documents/3. tech/09-TECH-08-MC-AI-LIVE-LIVE-SCRIPT-RUNTIME-HOSTING-INTELLIGENCE-LIVE-SALES-CONTROL-ADS-SAFE-LIVE-ORCHESTRATION.md`.
4. `docs/documents/2. pack/08-PACK-08-MC-AI-LIVE.md`.
5. `docs/documents/3. tech/07-TECH-06-FACEBOOK-GATEWAY-META-LIVE-MESSENGER-CHANNEL-DELIVERY-COMMENT-TO-MESSENGER.md`.
6. `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md`.
7. `docs/documents/3. tech/08-TECH-07-ADS-MEASUREMENT-ATTRIBUTION-ROAS-CPA-AOV-VERIFIED-REVENUE-SCALE-GATE.md`.
8. `docs/documents/3. tech/11-TECH-10-GLOBAL-SMOKE-UAT-EVIDENCE-RELEASE-GATEWAY-PRODUCTION-READINESS-CONTROL.md`.
9. `openapi/business-platform/mc-ai-live.v1.yaml`.
10. `schemas/live/*`, `schemas/channel/comment-to-messenger-handoff.schema.json`, `schemas/commerce/quote-snapshot.schema.json`, `schemas/ads/verified-revenue-link.schema.json`.

`BẢNG GÔM PHASE 7.md` remains outside this handoff review scope.

### 9.2. Slice-to-contract traceability

| File | Backlog | Primary contracts |
|---|---|---|
| `02-P7-2A-LIVE-SESSION-CONTEXT-RESOLVER.md` | LIVE-BLG-001 | `LivePlan`, Gateway channel context, sale lock/recall/sellable status. |
| `03-P7-2B-LIVE-SCRIPT-RUNTIME-CLAIM-GUARD.md` | LIVE-BLG-002/003 | `HostCue`, `LiveRiskCue`, claim/guard evidence. |
| `04-P7-2C-LIVE-COMMENT-CLASSIFIER-MESSENGER-COORDINATION.md` | LIVE-BLG-004/005 | Gateway comment, `PrivateHandoffCue`, `CommentToMessengerHandoff`. |
| `05-P7-2D-PRODUCT-PRICE-BOUNDARY-IN-LIVE.md` | LIVE-BLG-006 | `QuoteSnapshot`, order/payment/verified revenue refs. |
| `06-P7-2E-TROLL-COMPLAINT-MODERATION-ROUTE.md` | LIVE-BLG-007/008 | `LiveRiskCue`, human takeover/CSKH/moderation route. |
| `07-P7-2F-ADS-SAFE-LIVE-SIGNAL-BOUNDARY.md` | LIVE-BLG-009 | `LiveMeasurementEvent`, Ads funnel, verified revenue link. |
| `08-P7-2G-SMOKE-EVIDENCE-REPORT.md` | LIVE-BLG-010 | Evidence registry, smoke matrix, release gate. |

### 9.3. Dev task decomposition

Dev may split Phase 7 into these implementation tickets only after SRS confirms scope:

| Ticket | Scope | Must prove |
|---|---|---|
| P7-DEV-001 | Live Plan / Session resolver | Unknown session/plan/suppression fail-safe. |
| P7-DEV-002 | Host Cue contract integration | Cue cannot bypass guard or product scope. |
| P7-DEV-003 | Claim/Fake urgency guard integration | Medical/fake scarcity/private data blocked. |
| P7-DEV-004 | Live comment classifier | Intent/risk route and dedup. |
| P7-DEV-005 | Gateway handoff cue | No direct Messenger bypass; handoff fail wording safe. |
| P7-DEV-006 | Commerce price/order boundary | No QuoteSnapshot -> no price; no public order. |
| P7-DEV-007 | Complaint/risk route | Complaint/human/quality vs abuse/moderation separation. |
| P7-DEV-008 | Ads-safe live signal | Signal not revenue; no scale decision. |
| P7-DEV-009 | Evidence/smoke package | P0 smoke and evidence refs. |

### 9.4. Required dev outputs

For each ticket, dev must output:

```yaml
ticket_id: P7-DEV-xxx
source_docs_checked: []
contracts_used: []
implemented_files: []
api_or_schema_changes: []
tests_or_smoke:
  - id: string
    result: PASS | FAIL | BLOCKED | NOT_RUN
evidence_refs: []
blocked_items: []
owner_decisions_required: []
release_claim:
  live_ready: false
  release_ready: false
  production_ready: false
```

### 9.5. Owner decisions required

| Decision ID | Owner question | Default if not decided |
|---|---|---|
| P7-OD-001 | Which pages/live sessions are allowed for MC AI Live pilot? | Block live production. |
| P7-OD-002 | Which products/SKUs are in live product scope? | Block sales cue. |
| P7-OD-003 | Which public-safe CTA wording is approved? | Use minimal safe ack only. |
| P7-OD-004 | What is the official live risk taxonomy/severity enum? | Use risk cue but block production claim. |
| P7-OD-005 | Who owns complaint/quality/human handoff queue? | Route blocked to owner review. |
| P7-OD-006 | What live signal taxonomy is accepted by Ads Measurement? | Signal export blocked or marked taxonomy pending. |
| P7-OD-007 | What evidence package ID is accepted for Phase 7? | No Live Ready/Release Ready. |

### 9.6. Scope lock

Phase 7 handoff is allowed to produce:

- Analysis.
- SRS.
- Contract delta requests.
- Limited implementation task breakdown.
- Smoke/evidence plan.
- Local/dev validation report.

Phase 7 handoff is not allowed to produce:

- Production live mode.
- Auto public response delivery.
- Direct Messenger send bypassing Gateway.
- Quote/order/payment/revenue write.
- Ads scale decision.
- Production Ready or Release Ready claim.

### 9.7. Final handoff verdict

The only valid final verdict for this folder after SRS hardening:

```yaml
phase: P7
handoff_status: READY_FOR_SRS_AND_DEV_TASK_BREAKDOWN
live_ready: false
release_ready: false
production_ready: false
global_gateway: BLOCKED
```


# P7.2F - Ads-safe Live Signal Boundary

**BACKLOG:** LIVE-BLG-009  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Dam bao live signal chi la signal/funnel/engagement, khong bi dung lam revenue/ROAS hoac scale decision khi chua qua Phase 6.

## Entry Gate

- ADS-BLG-001 event taxonomy pass.
- ADS-BLG-004 Verified Revenue consumption pass neu lien quan ROAS.
- LIVE-BLG-001/004 pass.

Neu thieu ADS-BLG-004, ghi `LIVE-BLG-009-BLOCKED-BY-ADS-VERIFIED-REVENUE`.

## Implementation Scope

- Live signal event mapping.
- Ads-safe label.
- Revenue=false guard.
- Signal export to Ads Measurement.
- Evidence for no ROAS from live signal.

## Not Allowed

- Dung comment/view/engagement live lam revenue.
- Dung live order intent lam ROAS.
- Auto-scale ads tu live hype.
- Hide suppression/data quality fail.

## Acceptance Criteria

- Live signal exported as funnel/engagement only.
- Revenue metric only from Commerce Verified Revenue.
- Ads dashboard flags live signal confidence.
- Scale decision remains under Phase 6 Gate.

## Smoke

| Smoke ID | Scenario | Expected |
|---|---|---|
| P7.2F-SMK-001 | Live comment high intent | Signal only, not revenue |
| P7.2F-SMK-002 | Live view spike | No ROAS |
| P7.2F-SMK-003 | Live quote created | Funnel signal only |
| P7.2F-SMK-004 | Verified revenue arrives | Revenue via Ads Phase 6 only |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

## 9. SRS hardening addendum - Ads-safe Live Signal Boundary

### 9.1. Purpose lock

`LIVE-BLG-009` đảm bảo live signal chỉ là funnel/engagement/operational signal. Nó không phải revenue, không phải ROAS, không phải scale approval, không phải owner decision.

### 9.2. LiveMeasurementEvent contract

All live measurement outputs must map to `schemas/live/live-measurement-event.schema.json`:

```yaml
live_measurement_event:
  live_measurement_event_id: required
  version: v1
  live_session_id: required
  channel_context_id: required
  event_type: required
  funnel_event_id: optional
  verified_revenue_link_id: optional_after_commerce_verified
  raw_or_clean_status: required
  not_revenue_by_itself: true
```

Invariant: `not_revenue_by_itself` must always be `true`.

### 9.3. Event taxonomy for SRS

| Event type | Meaning | Revenue basis |
|---|---|---|
| `LIVE_VIEW` | View/reach signal | None. |
| `LIVE_COMMENT` | Comment occurred | None. |
| `LIVE_PRICE_INTENT` | Price question | Funnel only. |
| `LIVE_BUY_INTENT` | Buying signal | Funnel only; no order. |
| `LIVE_PRIVATE_HANDOFF_REQUESTED` | Handoff requested | Funnel only. |
| `LIVE_PRIVATE_HANDOFF_SUCCEEDED` | Gateway handoff success | Funnel only. |
| `LIVE_QUOTE_REQUESTED` | Quote requested | Funnel only. |
| `LIVE_QUOTE_CREATED` | QuoteSnapshot created | Not revenue. |
| `LIVE_ORDER_DRAFT_CREATED` | Draft created | Not revenue. |
| `LIVE_OFFICIAL_ORDER_CREATED` | Order created | Not verified revenue by itself. |
| `LIVE_VERIFIED_REVENUE_LINKED` | Verified revenue linked | Revenue source remains Commerce/Ads verified revenue. |
| `LIVE_COMPLAINT` | Complaint/risk | Negative/risk signal; no scale. |

If official enum is not locked, SRS must label it `TAXONOMY_PENDING` and require contract update.

### 9.4. Ads handoff rule

| Live artifact | Ads usage allowed | Forbidden |
|---|---|---|
| Comment count | Engagement/funnel metric | Revenue/ROAS. |
| Buy intent | Funnel signal | Order/revenue. |
| Handoff success | Funnel conversion step | Verified revenue. |
| QuoteSnapshot | Quote funnel | Revenue/ROAS until verified. |
| Official order | Order funnel | Revenue until verified basis pass. |
| Payment waiting/COD waiting | Waiting state | Verified revenue/ROAS. |
| Verified revenue link | ROAS basis if TECH-07 pass | Direct mutation by MC AI Live. |
| Complaint/risk | Stop/pause/review signal | Scale-positive signal. |

### 9.5. Scale guard

MC AI Live must not create, approve, or imply an Ads Scale Decision. It may only emit a signal that Ads Measurement later reviews.

```yaml
scale_guard:
  live_signal_can_scale_ads: false
  scale_allowed_only_if:
    - verified_revenue_link_pass
    - data_quality_pass
    - attribution_pass
    - sellable_clear
    - sale_lock_clear
    - recall_clear
    - claim_privacy_incident_clear
    - owner_scale_decision_pass
```

### 9.6. Conflict handling

| Conflict | Required verdict |
|---|---|
| High comments but no Verified Revenue | No ROAS; no scale. |
| Quote value high but payment pending | No revenue. |
| ROAS looks good but complaint increased | Scale hold/review. |
| Live session identity missing | Measurement hold. |
| Attribution conflict unresolved | No source ROAS. |
| Sale lock/recall active | Scale blocked and live sales cue blocked. |
| Data quality fail | No scale, no ROAS pass. |

### 9.7. Evidence required

| Evidence ID | Artifact |
|---|---|
| P7-2F-EVD-001 | Live comment signal recorded with `not_revenue_by_itself=true`. |
| P7-2F-EVD-002 | Live view spike does not create ROAS. |
| P7-2F-EVD-003 | Quote created from live is funnel only. |
| P7-2F-EVD-004 | Order/payment waiting excluded from verified revenue. |
| P7-2F-EVD-005 | Verified revenue link consumed only after TECH-07 boundary pass. |
| P7-2F-EVD-006 | Complaint/risk signal blocks scale suggestion. |

### 9.8. Acceptance matrix

| AC ID | Requirement | Pass condition |
|---|---|---|
| P7-2F-AC-001 | Signal only | Every live event has `not_revenue_by_itself=true`. |
| P7-2F-AC-002 | No fake ROAS | ROAS never computed from comment/inbox/quote/draft/waiting states. |
| P7-2F-AC-003 | No scale | MC AI Live never approves or triggers ad scale. |
| P7-2F-AC-004 | Verified revenue | Revenue only through verified revenue link owned by Commerce/Ads. |
| P7-2F-AC-005 | Risk negative | Complaint/claim/privacy/sale lock risk cannot become positive scale signal. |
| P7-2F-AC-006 | Evidence | Ads-safe signal has evidence and can be audited. |

### 9.9. SRS fail conditions

SRS fails if it allows:

- Live engagement to become revenue.
- Quote/order draft/payment waiting to become ROAS.
- MC AI Live to recommend scale budget.
- Dashboard to show `ROAS_PASS` from live signal alone.
- Scale when sale lock/recall/data-quality/claim/privacy incident is active.


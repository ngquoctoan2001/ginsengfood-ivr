# Phase 6 - Consolidated Handoff Index

**PROMPT-P6 - ADS MEASUREMENT / ATTRIBUTION / ROAS / SCALE GATE ANALYSIS HANDOFF**

## Mode

ANALYSIS ONLY. Khong sua code, khong tao dashboard, khong migration, khong seed, khong scale Ads.

## AI Review Boundary

Reviewer AI chỉ chấm tài liệu này theo tiêu chí đủ để giao dev chạy lane phân tích, thiết kế, bóc task và triển khai có kiểm soát. `ANALYSIS ONLY`, `GLOBAL GATEWAY: BLOCKED`, `NOT_RELEASE_READY` hoặc yêu cầu evidence không phải lỗi dev-readiness; đó là guard để không nhầm tài liệu thiết kế với production approval.

```text
DEV_IMPLEMENTATION_DOCUMENTATION_READY: YES_FOR_CONTROLLED_TASK_BREAKDOWN
PRODUCTION_READY: NO
RUNTIME_EVIDENCE_REQUIRED_AFTER_DEV: YES
```

## Entry Gate

Chi bat dau Phase 6 Analysis khi:

- P3 Verified Revenue boundary co evidence toi thieu.
- P3 Payment/COD/Order Verified boundary co evidence.
- Phase 5 event/channel identity co evidence neu dung Meta/Facebook signal.
- Neu Verified Revenue chua pass, ghi blocker `P6-ENTRY-BLOCKED-BY-VERIFIED-REVENUE`.

**GLOBAL GATEWAY:** BLOCKED.

## Source Of Truth

- `docs/documents/3. tech/08-TECH-07-ADS-MEASUREMENT-ATTRIBUTION-ROAS-CPA-AOV-VERIFIED-REVENUE-SCALE-GATE.md`
- `docs/documents/2. pack/07-PACK-07-ADS-ROAS-ATTRIBUTION.md`
- `docs/documents/3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md`
- `docs/documents/3. tech/07-TECH-06-FACEBOOK-GATEWAY-META-LIVE-MESSENGER-CHANNEL-DELIVERY-COMMENT-TO-MESSENGER.md`
- `docs/documents/3. tech/13-TECH-12-PHASE-BACKLOG-PACK-DEV-TASK-BREAKDOWN-SOURCE-TO-BACKLOG-MATRIX.md`
- `docs/documents/2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md`

Chua thay source-of-truth `docs/docs-ginsengfood-project/` trong workspace hien tai. Neu sau nay file do xuat hien, phai doi chieu lai truoc khi sua backend that.

## Scope Can Phan Tich

1. Event taxonomy / funnel signal boundary.
2. Pixel/CAPI dedup/idempotency.
3. Attribution source resolver.
4. Verified Revenue consumption from Commerce.
5. Exclusion rules for quote/cart/draft/unpaid/WAITING.
6. CPA/AOV/ROAS computation boundary.
7. Data Quality Gate.
8. Scale Gate / owner approval.
9. Sale Lock / Recall / Suppression blocks scale.
10. Smoke/evidence pack.

## Khong Duoc Lam

- Khong tinh revenue tu quote, cart, order draft, payment WAITING, COD WAITING.
- Khong double count Pixel/CAPI/event duplicate.
- Khong lam dep dashboard bang du lieu chua verified.
- Khong scale Ads khi Data Quality fail hoac owner chua accept.
- Khong bo Sale Lock/Recall/Suppression de scale.

## Output Bat Buoc

- Current-state map cho event, attribution, revenue, dashboard.
- Gap matrix ADS-BLG-001 -> ADS-BLG-010.
- Conflict matrix voi Commerce Verified Revenue.
- Data quality risk register.
- Attribution and dedup risk register.
- Proposed smoke matrix.
- Owner decision required.

## Done Gate

Analysis chi ket thuc khi co report ro evidence/missing evidence. Khong goi ROAS pass, scale-ready hoac release-ready.

# P6.1 - Technical Design Only

**PROMPT-P6.1 - ADS MEASUREMENT TECHNICAL DESIGN HANDOFF**

## Mode

TECHNICAL DESIGN ONLY. Khong implement, khong tao dashboard production, khong sua Ads budget/scale.

## Input Bat Buoc

- P6 Analysis report.
- P3 Verified Revenue evidence.
- P5 channel/event identity evidence neu dung Meta/Facebook signal.
- TECH-07 / PACK-07.

Neu thieu Verified Revenue evidence, ghi `BLOCKED_NEED_COMMERCE_VERIFIED_REVENUE`.

## Design Boundary

Ads Measurement la do luong va attribution runtime. No chi:

- Consume event/funnel signal.
- Dedup Pixel/CAPI/server events.
- Resolve attribution source.
- Consume Verified Revenue tu Commerce.
- Exclude unpaid/waiting/draft signals khoi revenue.
- Compute CPA/AOV/ROAS tu verified data.
- Gate data quality va scale.

Ads Measurement khong:

- Tu tao quote/cart/order.
- Tu xac nhan payment/COD.
- Tu chuyen WAITING thanh revenue.
- Tu scale khi evidence chua accepted.

## Workstreams

| Slice | Backlog                 | Muc tieu                                       |
| ----- | ----------------------- | ---------------------------------------------- |
| P6.2A | ADS-BLG-001             | Event Taxonomy / Funnel Signal Boundary        |
| P6.2B | ADS-BLG-002/003         | Pixel/CAPI Dedup + Attribution Resolver        |
| P6.2C | ADS-BLG-004/005         | Verified Revenue Consumption + Exclusion Rules |
| P6.2D | ADS-BLG-006             | CPA/AOV/ROAS Computation Boundary              |
| P6.2E | ADS-BLG-007/008/009     | Data Quality + Scale Gate + Suppression        |
| P6.2F | ADS-BLG-007/008 support | Dashboard Evidence + Owner Review Boundary     |
| P6.2G | ADS-BLG-010             | Ads Smoke / Evidence Pack                      |

## Contract Can Khoa

- Event envelope.
- Event identity / idempotency key.
- Attribution source binding.
- Verified Revenue event from Commerce.
- Exclusion reason.
- Metric computation input/output.
- Data Quality decision.
- Scale decision request.
- Suppression decision.
- Evidence record.

## API/DTO Impact

Moi API/DTO/event/OpenAPI thay doi phai ghi FE/BI/dashboard/ads connector impact. Khong tu doi public contract neu chua co owner approval.

## P0 Design Blockers

- Revenue source khong phai Commerce Verified Revenue.
- Quote/cart/draft/unpaid tinh vao ROAS.
- Pixel/CAPI double count.
- Scale khi Data Quality fail.
- Scale khi Sale Lock/Recall/Suppression active.

## Final Status

**P6 TECHNICAL DESIGN HANDOFF COMPLETED - NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P6.2A - Event Taxonomy / Funnel Signal Boundary

**BACKLOG:** ADS-BLG-001  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Khoa event taxonomy cho Ads Measurement va tach ro funnel signal voi revenue.

## Entry Gate

- P6.1 design accepted.
- Phase 0 audit/idempotency foundation available.
- Phase 5 channel identity evidence neu dung Meta/Facebook signal.

## Implementation Scope

- Event taxonomy registry.
- Funnel signal classification.
- Revenue/non-revenue flag.
- Source/channel/campaign identity fields.
- Evidence logging.

## Not Allowed

- Goi quote/cart/order draft la revenue.
- Dung comment/inbox/conversation lam revenue.
- Hardcode campaign mapping khong co source/evidence.
- Nhan event khong co identity.

## Acceptance Criteria

- Moi event co taxonomy id/source/channel/correlation.
- Funnel signal va revenue duoc tach ro.
- Unknown event bi quarantine.
- Evidence co sample event masked.

## Smoke

| Smoke ID      | Scenario       | Expected                   |
| ------------- | -------------- | -------------------------- |
| P6.2A-SMK-001 | Comment event  | Funnel signal, not revenue |
| P6.2A-SMK-002 | Quote created  | Funnel signal, not revenue |
| P6.2A-SMK-003 | Unknown event  | Quarantine                 |
| P6.2A-SMK-004 | Missing source | Reject/missing evidence    |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P6.2B - Pixel / CAPI Dedup / Attribution Resolver

**BACKLOG:** ADS-BLG-002 / ADS-BLG-003  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Dam bao Pixel/CAPI/server events khong double count va attribution chi dung source hop le.

## Entry Gate

- ADS-BLG-001 pass.
- Idempotency/correlation foundation pass.
- Campaign/ad/adset identity fields accepted.

## Implementation Scope

- Dedup key resolver.
- Pixel/CAPI duplicate prevention.
- Attribution source resolver.
- Paid/organic/live/CRM/referral separation.
- Conflict attribution handling.
- Evidence for event chain.

## Not Allowed

- Double count revenue/event.
- Gan attribution cho source khong co evidence.
- Override attribution bang manual note khong approved.
- Tron paid/organic/referral neu chua co rule priority.

## Acceptance Criteria

- Duplicate Pixel/CAPI event chi count 1 lan.
- Attribution co reason/source/evidence.
- Conflict attribution duoc flagged.
- Event chain trace duoc tu impression/click/comment/inbox den verified revenue neu co.

## Smoke

| Smoke ID      | Scenario                   | Expected                              |
| ------------- | -------------------------- | ------------------------------------- |
| P6.2B-SMK-001 | Pixel + CAPI same event    | Dedup count 1                         |
| P6.2B-SMK-002 | Missing attribution source | Unknown/needs review                  |
| P6.2B-SMK-003 | Paid + referral conflict   | Flag conflict                         |
| P6.2B-SMK-004 | Organic comment            | Organic/live signal, not paid revenue |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P6.2C - Verified Revenue Consumption / Exclusion Rules

**BACKLOG:** ADS-BLG-004 / ADS-BLG-005  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Ads chi consume Verified Revenue tu Commerce va loai tru tat ca quote/cart/order draft/unpaid/WAITING khoi revenue.

## Entry Gate

- COM-BLG-009 Verified Revenue pass.
- ADS-BLG-002/003 pass.
- Commerce revenue event contract accepted.

Neu thieu COM-BLG-009, ghi `ADS-BLG-004-BLOCKED-BY-COMMERCE-VERIFIED-REVENUE`.

## Implementation Scope

- Verified Revenue consumer.
- Revenue source validation.
- Exclusion rules.
- Refund/cancel/fail handling.
- COD success/fail boundary.
- Evidence for revenue accepted/rejected.

## Not Allowed

- Payment WAITING tinh revenue.
- COD WAITING tinh revenue.
- Order Draft tinh revenue.
- Quote/Cart tinh revenue.
- Manual dashboard override revenue.

## Acceptance Criteria

- Only Commerce Verified Revenue enters ROAS revenue.
- Every excluded signal has reason code.
- Refund/cancel/fail reduces/excludes per approved policy.
- Revenue evidence traceable to official order/payment/COD verified.

## Smoke

| Smoke ID      | Scenario            | Expected                 |
| ------------- | ------------------- | ------------------------ |
| P6.2C-SMK-001 | Quote created       | Excluded from revenue    |
| P6.2C-SMK-002 | Order draft         | Excluded from revenue    |
| P6.2C-SMK-003 | Payment WAITING     | Excluded from revenue    |
| P6.2C-SMK-004 | COD WAITING         | Excluded from revenue    |
| P6.2C-SMK-005 | Verified paid order | Revenue accepted         |
| P6.2C-SMK-006 | Cancel/refund       | Exclude/adjust by policy |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P6.2D - CPA / AOV / ROAS Computation Boundary

**BACKLOG:** ADS-BLG-006  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Tinh CPA/AOV/ROAS tu input da verified va co data quality, khong tinh tren so ao.

## Entry Gate

- ADS-BLG-004/005 pass.
- Cost source/import boundary accepted.
- Attribution resolver evidence available.

## Implementation Scope

- Metric input validator.
- CPA computation.
- AOV computation.
- ROAS computation.
- Missing/partial data flag.
- Metric evidence record.

## Not Allowed

- Tinh ROAS khi revenue chua verified.
- Tinh CPA khi cost source chua valid.
- Hide missing attribution/data quality issue.
- Scale decision tu metric chua accepted.

## Acceptance Criteria

- ROAS = verified revenue / approved ad spend.
- AOV = verified revenue / verified order count.
- CPA = approved ad spend / qualified acquisition theo rule accepted.
- Metric output co confidence/data quality status.

## Smoke

| Smoke ID      | Scenario                 | Expected                      |
| ------------- | ------------------------ | ----------------------------- |
| P6.2D-SMK-001 | Verified revenue + spend | ROAS computed                 |
| P6.2D-SMK-002 | Revenue missing          | No ROAS                       |
| P6.2D-SMK-003 | Cost missing             | CPA/ROAS blocked              |
| P6.2D-SMK-004 | Partial attribution      | Metric flagged low confidence |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P6.2E - Data Quality / Scale Gate / Suppression

**BACKLOG:** ADS-BLG-007 / ADS-BLG-008 / ADS-BLG-009  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Khoa gate chan scale khi data quality fail, owner chua approve, hoac Sale Lock/Recall/Suppression active.

## Entry Gate

- ADS-BLG-006 pass.
- Suppression source from Operational/Commerce available.
- Owner approval workflow accepted.

## Implementation Scope

- Data Quality Gate.
- Scale Gate decision.
- Owner approval record.
- Sale Lock/Recall/Suppression check.
- Pause/stop/reduce budget recommendation boundary.
- Evidence report.

## Not Allowed

- Auto-scale chi vi ROAS dep.
- Scale khi data quality fail.
- Scale khi owner chua approve.
- Scale SKU/campaign co recall/sale lock/suppression.
- Hide blocked reason.

## Acceptance Criteria

- Data Quality fail -> no scale.
- No owner approval -> no scale.
- Suppression active -> no scale.
- Scale recommendation co evidence, not automatic release.

## Smoke

| Smoke ID      | Scenario          | Expected                  |
| ------------- | ----------------- | ------------------------- |
| P6.2E-SMK-001 | Data quality fail | Scale blocked             |
| P6.2E-SMK-002 | Owner missing     | Scale blocked             |
| P6.2E-SMK-003 | Sale lock active  | Scale blocked             |
| P6.2E-SMK-004 | Recall active     | Scale blocked             |
| P6.2E-SMK-005 | Clean evidence    | Scale review request only |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P6.2G - Ads Smoke / Evidence Report

**BACKLOG:** ADS-BLG-010  
**MODE:** SMOKE / EVIDENCE / REPORT ONLY  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Chay smoke cho Phase 6, gom evidence, lap report 14 muc. Khong sua code, khong scale Ads trong file nay.

## Entry Gate

- ADS-BLG-001 -> ADS-BLG-009 da co report.
- COM-BLG-009 Verified Revenue evidence available.
- Data Quality Gate co evidence.

## Smoke Matrix Toi Thieu

| Smoke ID    | Scenario               | Expected                         |
| ----------- | ---------------------- | -------------------------------- |
| P6-GSMK-001 | Comment event          | Funnel signal, not revenue       |
| P6-GSMK-002 | Quote created          | Not revenue                      |
| P6-GSMK-003 | Order draft            | Not revenue                      |
| P6-GSMK-004 | Payment WAITING        | Not revenue                      |
| P6-GSMK-005 | COD WAITING            | Not revenue                      |
| P6-GSMK-006 | Verified paid order    | Revenue accepted                 |
| P6-GSMK-007 | Pixel/CAPI duplicate   | Count once                       |
| P6-GSMK-008 | Attribution conflict   | Flagged                          |
| P6-GSMK-009 | Missing ad spend       | ROAS blocked                     |
| P6-GSMK-010 | Data quality fail      | Scale blocked                    |
| P6-GSMK-011 | Owner approval missing | Scale blocked                    |
| P6-GSMK-012 | Recall/sale lock       | Scale blocked                    |
| P6-GSMK-013 | Refund/cancel          | Revenue adjusted/excluded        |
| P6-GSMK-014 | Clean pilot evidence   | Review request, not auto release |

## Evidence Bat Buoc

- Event samples masked.
- Dedup logs.
- Attribution chain.
- Verified Revenue source trace.
- Exclusion logs.
- Metric computation input/output.
- Dashboard/evidence status.
- Data Quality decision.
- Scale gate decision.
- Owner review request.

## Report 14 Muc

1. Scope and environment.
2. Source-of-truth checked.
3. Phase entry evidence.
4. Event taxonomy findings.
5. Pixel/CAPI dedup findings.
6. Attribution findings.
7. Verified Revenue findings.
8. Exclusion rule findings.
9. Metric computation findings.
10. Dashboard/evidence findings.
11. Data Quality and scale findings.
12. Smoke result matrix.
13. Blocker/risk register.
14. Owner decision request.

## Final Status

**PHASE 6 VALIDATION REPORT ONLY**  
**EVIDENCE SUBMITTED ONLY**  
**NOT ROAS PASS**  
**NOT SCALE READY**  
**GLOBAL GATEWAY: BLOCKED**

# P6 - README HANDOFF INDEX

## Update 2026-05-22 - Phase 6 Plan

Dung thu muc chinh thuc: `docs/documents/4. phase/phase-6/`.

Phase 6 la Ads Measurement / Attribution / ROAS / CPA / AOV / Verified Revenue / Scale Gate. Ads Measurement chi do luong va gate scale, khong tao doanh thu va khong xac nhan payment/order.

## Entry Gate

Chi duoc bat dau Phase 6 khi:

- COM-BLG-009 Verified Revenue co evidence toi thieu.
- Payment/COD/Order Verified boundary cua Phase 3 co evidence.
- Phase 5 event/channel identity co evidence neu consume Meta/Facebook signal.
- Khong co P0 blocker ve fake revenue.

Neu chua du, ghi `P6-ENTRY-BLOCKED-BY-VERIFIED-REVENUE`.

**Global Gateway:** `BLOCKED` den TECH-10.

## Source Of Truth

- TECH-07 Ads Measurement.
- PACK-07 Ads ROAS Attribution.
- TECH-04 Commerce Runtime / Verified Revenue.
- TECH-06 Facebook Gateway event source.
- TECH-11/12 phase governance/backlog.
- PACK-10 evidence gateway.

## Execution Order

1. `00-P6-ANALYSIS-ONLY.md`: inspect event/revenue/dashboard code that.
2. `01-P6-1-TECHNICAL-DESIGN-ONLY.md`: khoa Ads Measurement contract.
3. `02-P6-2A-EVENT-TAXONOMY-FUNNEL-SIGNAL.md`: ADS-BLG-001.
4. `03-P6-2B-PIXEL-CAPI-DEDUPE-ATTRIBUTION.md`: ADS-BLG-002/003.
5. `04-P6-2C-VERIFIED-REVENUE-EXCLUSION-RULES.md`: ADS-BLG-004/005.
6. `05-P6-2D-CPA-AOV-ROAS-COMPUTATION.md`: ADS-BLG-006.
7. `06-P6-2E-DATA-QUALITY-SCALE-GATE-SUPPRESSION.md`: ADS-BLG-007/008/009.
8. `07-P6-2F-DASHBOARD-EVIDENCE-OWNER-REVIEW.md`: dashboard/evidence/owner review boundary.
9. `08-P6-2G-SMOKE-EVIDENCE-REPORT.md`: ADS-BLG-010.

## Core Rules

- Verified Revenue tu Commerce la revenue source duy nhat cho ROAS/AOV theo revenue.
- Quote, cart, order draft, unpaid order, Payment WAITING, COD WAITING chi la funnel signal.
- Pixel/CAPI duplicate khong duoc double count.
- Data Quality fail thi no scale.
- Owner approval missing thi no scale.
- Sale Lock/Recall/Suppression active thi no scale.

## P0 Blockers

- Ads dung quote/cart/draft lam revenue.
- Unpaid/WAITING/COD WAITING tinh revenue.
- Verified Revenue khong tu Commerce.
- Pixel/CAPI double count.
- Scale khi Data Quality fail.
- Scale khi owner chua accept evidence.

## Downstream

Phase 7 MC AI Live chi duoc dung live signal nhu funnel/engagement signal, khong duoc dung lam revenue/ROAS.

**NOT ROAS PASS**  
**NOT SCALE READY**  
**GLOBAL GATEWAY: BLOCKED**

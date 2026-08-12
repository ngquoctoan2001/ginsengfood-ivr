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

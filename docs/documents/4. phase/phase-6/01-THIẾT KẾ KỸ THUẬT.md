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

| Slice | Backlog | Muc tieu |
|---|---|---|
| P6.2A | ADS-BLG-001 | Event Taxonomy / Funnel Signal Boundary |
| P6.2B | ADS-BLG-002/003 | Pixel/CAPI Dedup + Attribution Resolver |
| P6.2C | ADS-BLG-004/005 | Verified Revenue Consumption + Exclusion Rules |
| P6.2D | ADS-BLG-006 | CPA/AOV/ROAS Computation Boundary |
| P6.2E | ADS-BLG-007/008/009 | Data Quality + Scale Gate + Suppression |
| P6.2F | ADS-BLG-007/008 support | Dashboard Evidence + Owner Review Boundary |
| P6.2G | ADS-BLG-010 | Ads Smoke / Evidence Pack |

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

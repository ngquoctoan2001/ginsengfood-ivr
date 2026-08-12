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

| Smoke ID | Scenario | Expected |
|---|---|---|
| P6.2C-SMK-001 | Quote created | Excluded from revenue |
| P6.2C-SMK-002 | Order draft | Excluded from revenue |
| P6.2C-SMK-003 | Payment WAITING | Excluded from revenue |
| P6.2C-SMK-004 | COD WAITING | Excluded from revenue |
| P6.2C-SMK-005 | Verified paid order | Revenue accepted |
| P6.2C-SMK-006 | Cancel/refund | Exclude/adjust by policy |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**


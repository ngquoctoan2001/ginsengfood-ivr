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

| Smoke ID | Scenario | Expected |
|---|---|---|
| P6.2D-SMK-001 | Verified revenue + spend | ROAS computed |
| P6.2D-SMK-002 | Revenue missing | No ROAS |
| P6.2D-SMK-003 | Cost missing | CPA/ROAS blocked |
| P6.2D-SMK-004 | Partial attribution | Metric flagged low confidence |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**


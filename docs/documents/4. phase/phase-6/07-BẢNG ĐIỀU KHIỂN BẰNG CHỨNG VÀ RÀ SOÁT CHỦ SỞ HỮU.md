# P6.2F - Dashboard Evidence / Owner Review Boundary

**BACKLOG:** ADS-BLG-007 / ADS-BLG-008 support slice  
**MODE:** LIMITED IMPLEMENTATION HANDOFF  
**GLOBAL GATEWAY:** BLOCKED

## Muc Tieu

Khoa cach hien thi dashboard/measurement output de dashboard khong bi hieu nham la production truth, ROAS pass hoac scale approval khi chua co owner review.

## Entry Gate

- ADS-BLG-006 metric computation pass.
- Data Quality Gate design accepted.
- Owner review workflow accepted.

## Implementation Scope

- Dashboard metric status labels.
- Data Quality status display.
- Evidence link/reference.
- Owner review request state.
- Pilot/scale decision boundary.
- Mask sensitive/internal data.

## Not Allowed

- Hien thi ROAS nhu PASS khi Data Quality chua pass.
- Hien thi scale-ready khi owner chua approve.
- Hide low-confidence attribution.
- Public cost/margin/internal campaign notes sai policy.
- Dashboard auto-scale budget.

## Acceptance Criteria

- Moi metric co source/evidence/data quality status.
- Scale status la review request, khong phai auto decision.
- Low-confidence/missing data hien thi ro.
- Dashboard khong expose private/internal data.

## Smoke

| Smoke ID | Scenario | Expected |
|---|---|---|
| P6.2F-SMK-001 | Missing evidence | Dashboard shows incomplete |
| P6.2F-SMK-002 | Data Quality fail | ROAS not pass, no scale |
| P6.2F-SMK-003 | Owner approval missing | Scale blocked |
| P6.2F-SMK-004 | Internal metric field | Mask/hide by policy |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT ROAS PASS**  
**NOT SCALE READY**  
**GLOBAL GATEWAY: BLOCKED**


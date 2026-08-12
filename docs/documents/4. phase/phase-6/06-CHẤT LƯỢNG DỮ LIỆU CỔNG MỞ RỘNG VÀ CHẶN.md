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

| Smoke ID | Scenario | Expected |
|---|---|---|
| P6.2E-SMK-001 | Data quality fail | Scale blocked |
| P6.2E-SMK-002 | Owner missing | Scale blocked |
| P6.2E-SMK-003 | Sale lock active | Scale blocked |
| P6.2E-SMK-004 | Recall active | Scale blocked |
| P6.2E-SMK-005 | Clean evidence | Scale review request only |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**


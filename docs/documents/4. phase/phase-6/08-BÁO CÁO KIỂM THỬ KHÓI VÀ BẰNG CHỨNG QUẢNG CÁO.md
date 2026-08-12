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

| Smoke ID | Scenario | Expected |
|---|---|---|
| P6-GSMK-001 | Comment event | Funnel signal, not revenue |
| P6-GSMK-002 | Quote created | Not revenue |
| P6-GSMK-003 | Order draft | Not revenue |
| P6-GSMK-004 | Payment WAITING | Not revenue |
| P6-GSMK-005 | COD WAITING | Not revenue |
| P6-GSMK-006 | Verified paid order | Revenue accepted |
| P6-GSMK-007 | Pixel/CAPI duplicate | Count once |
| P6-GSMK-008 | Attribution conflict | Flagged |
| P6-GSMK-009 | Missing ad spend | ROAS blocked |
| P6-GSMK-010 | Data quality fail | Scale blocked |
| P6-GSMK-011 | Owner approval missing | Scale blocked |
| P6-GSMK-012 | Recall/sale lock | Scale blocked |
| P6-GSMK-013 | Refund/cancel | Revenue adjusted/excluded |
| P6-GSMK-014 | Clean pilot evidence | Review request, not auto release |

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


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

| Smoke ID | Scenario | Expected |
|---|---|---|
| P6.2A-SMK-001 | Comment event | Funnel signal, not revenue |
| P6.2A-SMK-002 | Quote created | Funnel signal, not revenue |
| P6.2A-SMK-003 | Unknown event | Quarantine |
| P6.2A-SMK-004 | Missing source | Reject/missing evidence |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**


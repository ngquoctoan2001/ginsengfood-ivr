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

| Smoke ID | Scenario | Expected |
|---|---|---|
| P6.2B-SMK-001 | Pixel + CAPI same event | Dedup count 1 |
| P6.2B-SMK-002 | Missing attribution source | Unknown/needs review |
| P6.2B-SMK-003 | Paid + referral conflict | Flag conflict |
| P6.2B-SMK-004 | Organic comment | Organic/live signal, not paid revenue |

## Final Status

**LIMITED IMPLEMENTATION REPORT ONLY**  
**NOT RELEASE READY**  
**GLOBAL GATEWAY: BLOCKED**


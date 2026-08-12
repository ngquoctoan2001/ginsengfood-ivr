# TEST-03 — Integration Test Plan

Trạng thái: `TARGET_V1_DRAFT`. Use PostgreSQL Testcontainers plus fake Sales/mock SIM/mock JWT.

1. Both program/payment paths: intake → job → speech → DTMF 1/0 → target callback ACK.
2. Duplicate/concurrent intake creates one job; changed payload conflicts.
3. Policy snapshot/offsets survive restart; alternate policy runs without migration.
4. Lease/fencing prevents duplicate dispatch; crash recovery respects expiry.
5. Speech/token/privacy violation never dispatches.
6. No-answer final sends wait-for-timeout; no IVR order mutation/notification.
7. Target callback covers terminal ACKs, stale/conflict/DLQ/retry; GH current adapter cannot receive 24/7.
8. Dependency/auth/evidence outages fail closed.
9. MOCK cannot real-egress; LAB rejects non-allowlisted destination.
10. Migration/retention/audit/outbox recovery pass.

Real Sales/vendor suites are separate gates and remain blocked until their environments exist.

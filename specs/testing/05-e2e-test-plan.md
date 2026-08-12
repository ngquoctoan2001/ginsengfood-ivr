# TEST-05 — End-to-End Plan

Trạng thái: `TARGET_V1_DRAFT`.

Run first with fake Sales/mock SIM/mock JWT:

1. GH ONLINE: speech details → key 1 → target `ACCEPTED`.
2. 24/7 COD: speech details → key 0 → Sales cancellation signal accepted.
3. Both programs: no answer per versioned policy → final wait-for-timeout callback; no notification.
4. Crossed payment/program, flag false, stale/expired and non-official task reject.
5. Missing/PII speech, expired token, call restriction/blocker reject without dial.
6. Technical/invalid/wrong input separated and customer-attempt accounting correct.
7. Sales ACK blocked/review/stale/conflict/invalid/retryable behaviors.
8. Duplicate/concurrent task/callback remains idempotent.
9. Capacity/crash/restart/fail-closed dependency cases.
10. Current GH compatibility path works only for GH and is visibly labelled.
11. Admin cannot override order/result policy or expose PII.
12. MOCK no external egress; LAB non-allowlisted number is rejected.

Re-run a bounded subset with real Sales sandbox and one real SIM when gates exist, recording evidence separately.

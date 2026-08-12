# PROMPT P8-2 — Real-SIM Lab Runbook (No Customers)

## 0. Meta

Work `W-0049` · prereq P8-1/P7/P6 · `LAB_REAL_SIM` · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 1. Outcome

Create and execute a controlled lab runbook using one real SIM and approved team-owned test numbers. This phase does **not** pilot real customers and never flips the production permission.

## 2. Build/run

1. Lab values enforce provider/mode, one channel, destination allowlist, call/time caps, recording/notification off and kill switch.
2. Preflight validates gateway/auth/token resolver/Sales fake-or-sandbox/telemetry, allowlist and emergency contacts.
3. Dry-run, then calls for key 1/0/no input/invalid/technical recovery; capture privacy-safe evidence.
4. Test kill switch before and during queued work; ensure new dials stop and active-call handling is defined.
5. Define abort thresholds, incident/escalation/cleanup and credential rotation after lab.
6. Produce lab report listing what was and was not verified, especially vendor dispositions not reproducible and 32-eSIM capacity not tested.

## 3. Acceptance

Lab report/evidence accepted by IVR/Infra; all numbers are approved test numbers; raw phone/audio absent; `REAL_CUSTOMER_CALL_ALLOWED=NO` before/during/after. Update W-0049. A later real-customer pilot requires a separate owner-approved release scope.

## 5. Forbidden
- ❌ Gọi bất kỳ số nào ngoài `labDestinationAllowlist`.
- ❌ Đặt `REAL_CUSTOMER_CALL_ALLOWED=true` (chỉ P9-1 sau DF-03 mới được xét).
- ❌ Coi lab 1 SIM là bằng chứng throughput 32 eSIM (`G-ESIM32`).
- ❌ Lưu audio hoặc số thật vào evidence.

## 6. Definition of Done
- [ ] Runbook chạy được end-to-end trên **1 SIM thật + allowlist**, kill switch verify được.
- [ ] Evidence trong `docs/evidence/W-0049/`: dial/DTMF/disposition log đã redact, allowlist reject sample, kill-switch drill.
- [ ] Đạt tối đa `LAB_REAL_SIM_VERIFIED` cho đúng phạm vi đã test; **không** suy ra integration hay production.
- [ ] Cập nhật W-0049; chỉ reviewer/owner chuyển `ACCEPTED`.

# Testing SRS — Index

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p11-generate-testing-specs.md`
Nguồn: `phase-8/09` (IVR-SMK + release gate), `/19` (smoke/release plan); `MASTER-05` (evidence/smoke/completion-gate); `TECH-10`; `specs/srs/*`; `seed/*` (SCN-*); decisions D-*/DO-*/DF-*/DT-*.

## 1. Cấu trúc
| File | Nội dung |
| --- | --- |
| [01-strategy.md](01-strategy.md) | Chiến lược, mock-first, evidence/gate model |
| [02-unit-test-plan.md](02-unit-test-plan.md) | Unit theo service block |
| [03-integration-test-plan.md](03-integration-test-plan.md) | Tích hợp mock Order Core/ops/SIM |
| [04-contract-test-plan.md](04-contract-test-plan.md) | Contract task/callback/error/OpenAPI |
| [05-e2e-test-plan.md](05-e2e-test-plan.md) | 8+ workflow (dry-run) |
| [06-performance-test-plan.md](06-performance-test-plan.md) | Capacity SIM 12/24/32 |
| [07-security-privacy-test-plan.md](07-security-privacy-test-plan.md) | RBAC, PII, allowlist, no order write |
| [08-acceptance-criteria.md](08-acceptance-criteria.md) | Evidence packet + done/fail gate + sign-off |
| [09-smoke-matrix.md](09-smoke-matrix.md) | Map IVR-SMK-* / M8-P0-* + PASS/BLOCK |

## 2. Nguyên tắc (P0)
- ✅ Mọi smoke có **cả PASS path và BLOCK/negative path**.
- ✅ **KHÔNG test gọi khách thật**; chỉ MOCK/dry-run (`IVR_ADAPTER_MODE=MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`).
- ✅ Acceptance gắn **evidence packet + owner sign-off** — không hardcode PASS (MASTER-05).
- ✅ **KHÔNG tuyên bố production-ready**.

## 3. P0 test bắt buộc (không được thiếu)
1. IVR không tự update order state (D-02).
2. Từ chối Quote/Cart/Order Draft (chỉ Official Order).
3. Golden Hour & 24/7 **không vượt 2 attempt** (D-10).
4. Technical failure **≠** no-answer (DT-02).
5. Invalid phone **≠** no-answer.
6. Stale callback **không** transition.
7. Evidence thiếu → **không** final/PASS.
8. Race Sale Lock/Recall (phím 1) → Core **block**.
9. do-not-call/opt-out → **block** dispatch (DC-01 source; mock seed vẫn dùng `call_restriction`).
10. Fail-closed khi ops/Core/CRM/evidence/SIM down.

## 4. Truy vết
Mỗi test → FR/P0 id → seed `SCN-*` → smoke id → evidence ref. Xem `09-smoke-matrix.md`.

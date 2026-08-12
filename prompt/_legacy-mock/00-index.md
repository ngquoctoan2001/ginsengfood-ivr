# IVR Dev Prompt Library — Index

> **LEGACY / DO NOT EXECUTE FOR TARGET V1:** các prompt trong thư mục này giữ để truy vết baseline cũ. Chạy prompt active ở `prompt/phase-*` và tracker canonical.

Trạng thái: `DEV_HANDOFF` · Sinh bởi: `plan/ivr-orther/prompts/p13-generate-final-prompt-library.md`
Đối tượng: dev backend / codex / copilot triển khai IVR theo slice. Nguồn cấu trúc: `TECH-13` (dev prompt pack), `TECH-11/12` (roadmap/backlog); docx §19 (slice M8.2A–H).
Trạng thái specs: p01–p12 + p14 xong (xem `specs/srs/_review/*`). Đây là **prompt triển khai** — KHÔNG phải code, KHÔNG production code.

## 1. Thứ tự dev handoff
| # | Prompt | Slice | Done gate tóm tắt |
| --- | --- | --- | --- |
| 01 | [foundation-and-contracts](01-foundation-and-contracts.md) | nền tảng | OpenAPI validate, DB migration (D-10 constraint), RBAC `IVR_*`, idempotency/audit, adapter port MOCK, allowlist |
| 02 | [m8-2a-task-intake](02-m8-2a-task-intake.md) | M8.2A | reject quote/cart/draft; official-order only; idempotency |
| 03 | [m8-2b-eligibility](03-m8-2b-eligibility.md) | M8.2B | trust/contact/blocker/window/capacity; fail-safe |
| 04 | [m8-2c-scheduler-queue](04-m8-2c-scheduler-queue.md) | M8.2C | rolling queue, attempt D-10, no batch |
| 05 | [m8-2d-sim-adapter](05-m8-2d-sim-adapter.md) | M8.2D | adapter port MOCK, one-sim-one-call, no order write |
| 06 | [m8-2e-dtmf-normalizer](06-m8-2e-dtmf-normalizer.md) | M8.2E | DTMF 1/0; DT-02 mapping; technical≠no-answer |
| 07 | [m8-2f-order-core-callback](07-m8-2f-order-core-callback.md) | M8.2F | callback signal + revalidate; no transition |
| 08 | [m8-2g-admin-monitoring-ui](08-m8-2g-admin-monitoring-ui.md) | M8.2G | dashboard/RBAC; PII masked; no force order |
| 09 | [m8-2h-smoke-evidence](09-m8-2h-smoke-evidence.md) | M8.2H | smoke pass bằng evidence; release gate NO |

## 2. Governance chung (mọi prompt)
- ✅ Mỗi prompt trace về **requirement ID** (`FR-IVR-*`/`P0-IVR-*`) + **source spec path** + **test/evidence expected**.
- ✅ `REAL_CUSTOMER_CALL_ALLOWED=NO`; chỉ **dry-run/MOCK** (`IVR_ADAPTER_MODE=MOCK`) tới khi mua SIM (DT-01) + release gate (DF-03).
- ✅ Tuân foundation: RBAC (DF-01), audit append-only + idempotency (DF-04), correlation (DF-05), evidence registry (DF-03).
- ❌ **KHÔNG** IVR update order state (D-02); KHÔNG bypass blocker (DO-*/DC-01); KHÔNG gọi khách thật; KHÔNG tuyên bố production-ready.
- Điểm P0 còn mở để gọi thật: `specs/srs/_review/open-decisions-register.md` (mua SIM, DF-03 sign-off).
- Q-C1/DC-01 và DG-03/DS-01..05 đã resolved.

## 3. Traceability bắt buộc mỗi PR
Source path (specs) · Requirement ID · Contract (OpenAPI/DB) · Test case (testing/*) · Evidence item (smoke). Thiếu 1 → không merge (MASTER-05).

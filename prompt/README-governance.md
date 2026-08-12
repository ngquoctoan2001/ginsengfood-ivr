# IVR Prompt Library — Governance

Trạng thái: `LIVING` · Cập nhật: `2026-08-12` · Áp dụng cho mọi `prompt/phase-*`.

## Mục tiêu chính xác

Hoàn thiện IVR .NET/Next.js/PostgreSQL đến `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS`, sau đó chứng minh `LAB_REAL_SIM` bằng 1 SIM/allowlist, rồi mới đóng các Sales/auth/legal/32-eSIM gates để xét production. “Prompt xong” không tự động đồng nghĩa “vận hành được”.

## Source priority

1. `plan/ivr-orther/target-contract-v1-draft.md`;
2. Target V1 overlay trong `decisions-log.md` và open-decision register;
3. OpenAPI target/current-compat;
4. specs/integration requirements;
5. prompt;
6. archive/history.

## Bất biến

1. `REAL_CUSTOMER_CALL_ALLOWED=NO` cho tới signed production gate.
2. Modes: `MOCK`, `LAB_REAL_SIM`, `PRODUCTION_REAL`; lab chỉ allowlisted test numbers.
3. IVR không transition order, không process payment, không gửi SMS/notification.
4. Program matrix: Golden Hour ONLINE và 24/7 COD, đều require IVR flag.
5. Target callback generic + semantic ACK; Golden Hour current endpoint nằm sau compat adapter.
6. Attempt policy versioned/configurable; candidate 2/5′/15′ chỉ MOCK/LAB tới owner sign-off.
7. Speech summary phải đọc items/qty/total/short area; cấm raw phone/full address/sensitive fields.
8. Dial bằng token tại telephony boundary; logs/UI/evidence luôn masked/redacted.
9. External outage hoặc missing policy/evidence → fail-closed.
10. Idempotency/correlation/audit append-only trên mọi command và provider call.

## Tracker bắt buộc

`_execution/prompt-execution-tracker.md` là **sổ tiến độ duy nhất**.

- Trước prompt: tạo/chọn Work ID, ghi scope/prereq/owner/status.
- Trong prompt: append activity/checkpoint, decision/API mới thiếu và việc phát sinh.
- Việc ngoài plan dùng **Work ID kế tiếp trong cùng bảng**, `Origin=UNPLANNED`; không tạo backlog rời.
- Sau prompt: ghi artifacts, commands/tests, evidence, residual blockers và trạng thái.
- Không ghi `ACCEPTED` nếu reviewer/evidence chưa có; không ghi `VERIFIED` từ mô phỏng.

## Stack/layout default

`.NET 10` API/Worker/Domain/Infrastructure/Contracts · PostgreSQL/EF Core/outbox · Next.js strict TypeScript admin · Docker/Compose · Kubernetes/Helm target · OpenTelemetry.

Providers bắt buộc: fake Sales, target Sales callback, current GH compat callback, mock SIM, vendor SIM, dial-token resolver, speech renderer/TTS, policy registry và auth token provider.

## Traceability/DoD

Mỗi PR/work item phải có: source/decision ID → contract/migration → implementation → test ID/command → evidence → remaining gate. Code/test pass chỉ đủ `TESTS_PASS`; external/live acceptance phải có evidence riêng.

# W-0147 — M8-07 Target V1 shared callback readiness

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

Trạng thái: **`M8_LOCAL_CALLBACK_READY / RETRY_AFTER_FIXED / ACK_MEDIA_FAIL_CLOSED_W0173 /
LOCAL_POSTGRES_CHAOS_PASS_HISTORICAL / M3_SECURITY_PLATFORM_REQUIRED / SHARED_E2E_NOT_RUN`**

Người ký phía M8: **Tôi — Module 8 / Project Owner**.

## 1. Phạm vi

- Trace final result → immutable outbox → lease → dispatcher → Target V1 HTTP → ACK/retry/circuit → audit/review.
- Đối chiếu Target OAS, IR-06, current Golden Hour compatibility, config/auth và local tests.
- Sửa defect `429 Retry-After` đã chứng minh; không thay payload/schema/order-state ownership.
- Lập exact handoff cho M3/Security/Platform và giữ real delivery fail-closed.

## 2. Bằng chứng trực tiếp

| Phát hiện | Source |
| --- | --- |
| Chỉ final result vào outbox; payload serialize một lần + SHA-256 | `CallbackOutboxSnapshotFactory.cs` |
| Dequeue dùng lock/lease/reclaim; complete kiểm lease token | `CallbackOutboxRepository.cs` |
| Target request có bearer, idempotency, correlation và path/body guard | `TargetV1CallbackTransport.cs` |
| 6 ACK semantic đi terminal; 429/5xx/timeout retry bounded | `CallbackDispatcher.cs` |
| Real `TARGET_V1` bị fail-start do auth/sandbox chưa duyệt | `CallbackDeliveryOptionsValidator` |
| Golden Hour compat có path/shape/auth riêng và từ chối 24/7 | `CurrentGoldenHourCallbackTransport.cs` + pinned compat schema |
| Generic consumer chưa thuộc repo IVR và IR-06 đánh dấu chưa có phía M3 | `integration-requirements/06-module-3-api-handover.md` §4.1 |

GitNexus impact trước sửa:

- `TargetV1CallbackTransport.ClassifyAsync`: **HIGH**, 15 impacted symbol, 2 process, 3 module.
- `CallbackDispatcher.CreateUpdate`: **HIGH**, 8 impacted symbol, 2 process, 3 module.
- Affected chain gồm `SendAsync`, `RunBatchAsync`, worker callback job, callback unit tests và chaos
  downstream flow. Cảnh báo đã được nêu trước mutation.

## 3. Defect và fix

Trước W-0147, OAS/IR-06/fixture có `Retry-After` nhưng transport không chuyển header sang dispatcher.
Dispatcher vì vậy chỉ dùng local backoff cho `429`.

Fix:

- thêm optional `RetryAfter` vào `CallbackTransportResult`;
- parse positive delta trên `429`;
- schedule retry theo `max(local backoff, Retry-After)`;
- giữ nguyên retry budget, payload hash, callback/idempotency identity và terminal mapping.

Test mới:

- `UT-CALLBACK-RETRY-AFTER-02B` — transport giữ `Retry-After`;
- `UT-CALLBACK-RETRY-AFTER-09B` — `next_retry_at` không sớm hơn server delay.

Follow-up `W-0173` phát hiện ACK `200/409` có media type không hỗ trợ từng thoát khỏi transport và
bị dispatcher coi là transient unexpected failure. `ReadAckAsync<T>` nay fail-closed cả JSON hỏng
và unsupported media thành `CALLBACK_ACK_INVALID`, giữ HTTP status và không retry. Negative test
`UT-CALLBACK-ACK-MEDIA-02C` khóa `200 text/html` cùng `409` JSON bị cắt.

## 4. Artifact cập nhật

- `src/Ivr.Infrastructure/Callbacks/CallbackDeliveryModels.cs`
- `src/Ivr.Infrastructure/Callbacks/TargetV1CallbackTransport.cs`
- `src/Ivr.Infrastructure/Callbacks/CallbackDispatcher.cs`
- `tests/Ivr.UnitTests/Callbacks/CallbackDeliveryTests.cs`
- [M8-07 handoff](../../../plan/ivr-orther/m8-07-target-v1-shared-callback-handoff-2026-09-03.md)
- T-05, IR-06, TODAY-01, worklist, tracker/readiness/gate mirror và official Markdown map.

## 5. Verification

| Gate | Kết quả |
| --- | --- |
| Callback unit tests | **PASS** — 40/40 tại W-0173 |
| Sales callback contract tests | **PASS** — 20/20 |
| Release build | **PASS** — 0 warning, 0 error |
| C# format | **PASS** — modified source/tests clean |
| Full local Unit / Contract | **PASS** — 499/499 + 24/24 tại W-0173 |
| PostgreSQL callback/chaos | **PASS_LOCAL_HISTORICAL** — W-0162 focused callback PostgreSQL `7/7` + full Chaos `8/8`; current W-0173 rerun `ENV_BLOCKED/NOT_RUN assertions` vì Docker server pipe không có |
| OpenAPI | **PASS** — lint 2 spec; parse 2; task fixture 9; schema negative 12; domain negative 13; compat 1; invalid OAS rejected; 3 pinned hash current |
| API docs | **PASS** — 14 generated artifact; boundary/link/topology/PII gates pass |
| Test traceability | **PASS** — regenerated/current 477 tagged test |
| Official Markdown map | **PASS** — 625 Markdown file; W-0147 evidence/handoff 0 unresolved |
| Gate mirror/readiness | **PASS** — 11 gate, 145 work item, 23 open decision; rung 0; production flag false |
| `git diff --check` | **PASS** |

Ghi chú lịch sử: lần chạy W-0147 ban đầu dừng ở fixture vì Docker unavailable. W-0162 đã chạy lại
assertion thật; xem [evidence W-0162](../W-0162/README.md). Local test/fake evidence chỉ chứng minh
M8 candidate. Không có request nào đi tới M3 sandbox thật.

## 6. Residual external gates

- M3 generic consumer/OAS/CDC/signature: `NOT_RECEIVED`.
- Security auth profile/credential custody: `NOT_RECEIVED`.
- Platform reachable sandbox/network/TLS/smoke: `NOT_RECEIVED`.
- Shared E2E matrix và exact-SHA report: `NOT_RUN`.
- Real Target V1 enable/deploy: `DISABLED / NOT_AUTHORIZED`.

Do đó `G-CONTRACT`, `G-AUTH`, `G-PLATFORM`, `G-RELEASE` vẫn `BLOCKED_EXTERNAL` và
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Handoff

M8 đã sửa phần thuộc quyền M8 và giao một matrix có thể kiểm chứng. Bên giao task không được tiếp tục
ghi chung chung “dev nối callback”: muốn mở real delivery phải trả lại consumer commit, authoritative
OAS, auth/custody, sandbox/network và shared E2E report đúng SHA.

**Người ký:** **Tôi — Module 8 / Project Owner** · **03/09/2026**.

# W-0173 — Target V1 malformed ACK fail-closed remediation

Ngày: `2026-09-04`

Baseline: `main@c213bf7663708dfca7184bf443e66d6552e2daea` + shared WIP được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / ACK_MEDIA_FAIL_CLOSED /
M8_LOCAL_CALLBACK_READY / EXTERNAL_E2E_NOT_RUN / DELIVERY_DISABLED`**

## 1. Phát hiện

`TargetV1CallbackTransport` dùng `ReadFromJsonAsync` cho ACK `200` và `409`. JSON hỏng ném
`JsonException`, còn media type không được hỗ trợ như `text/html` ném `NotSupportedException`.
Trước W-0173, exception thứ hai thoát khỏi transport; `CallbackDispatcher` bắt nó như lỗi bất ngờ
và chuyển thành `CALLBACK_TRANSPORT_UNEXPECTED_FAILURE / RETRY_PENDING`.

Đây là retry mù sai contract: ACK có HTTP response nhưng body/media type không hợp lệ phải đi
`CALLBACK_ACK_INVALID / INVALID_DEAD_LETTER`, giữ HTTP status để audit và không retry.

## 2. Impact trước sửa

GitNexus current tại `c213bf7` báo **HIGH**:

- `TargetV1CallbackTransport.SendAsync`: 16 symbol, 8 direct caller, 2 process, 3 module;
- `TargetV1CallbackTransport.ClassifyAsync`: 17 symbol, 1 direct caller, 2 process, 3 module;
- process bị ảnh hưởng: `CallbackDeliveryJobHost.ExecuteAsync` và downstream chaos flow;
- module: `Callbacks`, `Jobs`, `Chaos`.

Cảnh báo HIGH đã được nêu trước mutation. Phạm vi fix không đổi endpoint, payload, ACK taxonomy,
retry budget, auth, persistence schema hoặc production enablement.

## 3. Khắc phục

- Thêm helper `ReadAckAsync<T>` chỉ bắt `JsonException` và `NotSupportedException` từ ACK body.
- Hai nhánh ACK `200/409` dùng helper; lỗi parse/media trả `InvalidAck(status)`.
- Giữ nguyên cancellation/timeout và transport exception: chúng vẫn đi transient retry đúng contract.
- Thêm `UT-CALLBACK-ACK-MEDIA-02C` với hai negative case:
  - `200 text/html`;
  - `409 application/json` nhưng JSON bị cắt.
- Test bắt buộc `Invalid`, `CALLBACK_ACK_INVALID`, giữ HTTP status và không có `RetryAfter`.

## 4. Verification

| Gate | Kết quả |
| --- | --- |
| Focused callback Unit | **PASS `40/40`** — 0 fail/skip |
| Sales callback Contract | **PASS `20/20`** |
| Full Contract | **PASS `24/24`** |
| Full Unit | **PASS `499/499`** sau khi regenerate traceability |
| Release build | **PASS** — 0 warning, 0 error |
| C# format | **PASS** — hai file source/test trong scope |
| Test traceability | **PASS `477`** |
| PII scan | **PASS `6/6`** — source/test/evidence/handoff/worklist trong scope |
| API docs | **PASS** — 14 generated artifact; boundary/link/topology/PII checks |
| Gate mirror | **PASS** — 11 gate, 171 work item, 23 open decision, production=false |
| Markdown map | **PASS** — W-0173 indexed, 0 unresolved link |
| GitNexus post-change | **LOW trên aggregate dirty tree** — 93 symbol, 0 affected process; pre-edit HIGH vẫn là blast-radius authority cho callback symbols |
| `git diff --check` | **PASS** |
| PostgreSQL callback / Chaos current rerun | **ENV_BLOCKED / NOT_RUN assertions** — Docker client `29.6.2`, cả `desktop-linux` và `default` pipe không có server; không ghi đè bằng chứng lịch sử |
| W-0162 historical local evidence | **PASS_LOCAL tại baseline riêng** — PostgreSQL `7/7`, Chaos `8/8`; không suy thành current W-0173 hoặc shared E2E |

## 5. Ranh giới và phần còn lại

- Không request nào được gửi tới M3, sandbox hoặc service ngoài local test handler.
- M3 generic consumer/OAS/CDC/signature: `NOT_RECEIVED`.
- Security auth/trust/credential custody: `NOT_RECEIVED`.
- Platform sandbox/network/TLS/smoke: `NOT_RECEIVED`.
- Shared E2E matrix: `NOT_RUN`.
- Real Target V1 vẫn bị `CallbackDeliveryOptionsValidator` từ chối boot.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Bước tiếp theo

Nhận exact M3 consumer/OAS SHA, Security auth record và Platform sandbox evidence; sau đó chạy đủ
matrix trong M8-07 §6 trên cùng M8/M3 candidate. Nếu external input chưa có, việc local tiếp theo có
giá trị là tạo validator offline cho shared-E2E report; validator đó không được gửi network request
hoặc gỡ delivery guard.

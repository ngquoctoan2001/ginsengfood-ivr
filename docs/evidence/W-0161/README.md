# W-0161 — Close local PostgreSQL integration evidence gaps

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

Trạng thái: **`TESTS_PASS_LOCAL / POSTGRES_ASSERTIONS_EXECUTED / NO_SOURCE_CHANGE /
EXTERNAL_GATES_UNCHANGED / NO_GATE_PROMOTION`**

## 1. Phạm vi

- Khởi động lại Docker Desktop local và chạy assertion PostgreSQL/Testcontainers thật.
- Đóng các dòng `ENV_BLOCKED / NOT_RUN assertions` của W-0145, W-0148, W-0149, W-0150 và
  W-0151.
- Không sửa source, test, OpenAPI, migration, config ứng dụng hoặc production runtime để ép PASS.
- Không suy local PostgreSQL thành M3/shared/staging/UAT/production E2E.

## 2. Environment recovery

Docker Desktop đã cài nhưng daemon không có pipe. Lần khởi động đầu dừng vì stale runtime socket;
sau khi cô lập các runtime directory cũ, lần kế tiếp vẫn dừng do Docker AI/inference socket.

Các thay đổi local, recoverable:

- chuyển `C:\Users\Administrator\AppData\Local\Docker\run` thành
  `run.stale-w0161-20260903-143819`;
- chuyển `C:\Users\Administrator\AppData\Local\docker-secrets-engine` thành
  `docker-secrets-engine.stale-w0161-20260903-143932`;
- đổi `EnableDockerAI` trong `%APPDATA%\Docker\settings-store.json` từ `true` thành `false`;
- chuyển runtime directory được tái tạo trước lần start cuối thành
  `run.stale-w0161-ai-off-20260903-144128`.

Không xóa các directory cũ. Docker Desktop sau đó lên thành công; Docker Engine server
`29.6.2` sẵn sàng. Các container PostgreSQL/Ryuk do Testcontainers tạo đã tự cleanup. Container
`.NET SDK` tên `sharp_bartik` có sẵn ngoài W-0161 được giữ nguyên.

## 3. Lệnh và kết quả

```powershell
dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj `
  --configuration Release --no-restore `
  --logger "console;verbosity=minimal"
```

Kết quả trực tiếp:

```text
Passed! - Failed: 0, Passed: 236, Skipped: 0, Total: 236,
Duration: 3 m 15 s - Ivr.IntegrationTests.dll (net10.0)
```

## 4. Các evidence gap được đóng

19 tham chiếu scenario trong năm evidence pack tương ứng 16 `TestId` duy nhất vì
`IT-INTAKE-DB-01` và `IT-SCH-FINAL-04` được dùng lại ở nhiều pack.

| Evidence | Scenario/TestId đã chạy trong full suite | Kết quả |
| --- | --- | --- |
| W-0145 | `IT-INTAKE-DB-01`, `IT-INTAKE-DB-02`, `IT-SCH-DEADLINE-09`, `IT-SCH-DEADLINE-11`, `IT-SCH-DEADLINE-12` | **PASS_LOCAL_POSTGRES 5/5** |
| W-0148 | `IT-OPTOUT-PROPOSE-03`, `IT-OPTOUT-FAILSAFE-04` | **PASS_LOCAL_POSTGRES 2/2** |
| W-0149 | `IT-ELIG-RACE-12`, `IT-SCH-FINAL-04`, `IT-API-QUEUE-08` | **PASS_LOCAL_POSTGRES 3/3** |
| W-0150 | `IT-INTAKE-DB-01`, `IT-TEL-TOKENFAIL-02`, `IT-INTAKE-PRIVACY-04` | **PASS_LOCAL_POSTGRES 3/3** |
| W-0151 | `IT-INTAKE-DB-01`, `IT-SCH-FINAL-04`, `IT-POLICY-AUDIT-05`, `IT-POLICY-PROD-06`, `IT-NORM-TECH-02`, `IT-NORM-INVALID-04` | **PASS_LOCAL_POSTGRES 6/6** |

Các TestId trên được kiểm từ source test hiện hành; kết quả `236/236` chứng minh fixture và
assertion của toàn project integration đều hoàn tất, không chỉ build project.

## 5. Verification record

| Gate | Kết quả |
| --- | --- |
| Docker availability | **PASS** — client/server `29.6.2`; daemon ready |
| Full PostgreSQL integration | **PASS `236/236`** — 0 fail, 0 skip, 3m15 |
| W-0161 PII scan | **PASS** — 1/1 Markdown, 0 binary skipped |
| API docs | **PASS** — 14 generated artifacts; boundary/link/topology/PII checks PASS |
| Test traceability | **PASS `476`** |
| Capacity/tool regression | **PASS_UNCALIBRATED** — capacity 6/6; intake validator/receipt/ledger/checkpoint self-test PASS |
| Gate mirror | **PASS** — 11 gates, 159 work items, 23 open decisions, production=false |
| Official Markdown map | **PASS** — 648 Markdown files; W-0161 và target worklist 0 unresolved; global 199 unresolved là corpus backlog có sẵn |
| `git diff --check` | **PASS** — chỉ có line-ending warnings của shared worktree |
| GitNexus symbol impact | **N/A** — W-0161 không sửa function/class/method hoặc production source |

## 6. Non-inference và residual gates

- Đây là local disposable PostgreSQL do Testcontainers quản lý, không phải target DB của M3.
- Không có external endpoint, credential, producer/consumer, vendor SIM, trust store hoặc dữ liệu
  production nào được dùng.
- Các chữ ký Product, Order Core, M3, CRM, Legal, Security, Platform, Telephony và Release vẫn
  `NOT_RECEIVED` đúng theo từng pack.
- Capacity calibration vẫn `NOT_RUN`; các implementation mang `CODE_NOT_AUTHORIZED` vẫn không được
  mở.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Kết luận

Local PostgreSQL evidence gap của năm pack đã đóng. Việc còn lại trong các pack này đều là external
decision/artifact/shared-E2E hoặc implementation chưa được phép; W-0161 không nâng bất kỳ external
gate nào.

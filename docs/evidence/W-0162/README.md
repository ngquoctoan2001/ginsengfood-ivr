# W-0162 — W-0147 local callback PostgreSQL and Chaos rerun

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

Trạng thái: **`TESTS_PASS_LOCAL / CALLBACK_POSTGRES_PASS / CHAOS_PASS /
NO_SOURCE_CHANGE / EXTERNAL_E2E_NOT_RUN / NO_GATE_PROMOTION`**

## 1. Phạm vi

- Chạy riêng các PostgreSQL assertion trực tiếp phủ callback/outbox/normalization/race/fencing.
- Chạy toàn bộ `Ivr.ChaosTests` hiện hành khi Docker Engine đã sẵn sàng.
- Cập nhật khoảng trống `ENV_BLOCKED / NOT_RUN assertions` của
  [W-0147](../W-0147/README.md).
- Không sửa source, test, OpenAPI, migration hoặc runtime để ép PASS.

## 2. PostgreSQL callback run

```powershell
dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj `
  --configuration Release --no-restore `
  --filter "TestId=IT-DB-OUTBOX-06|TestId=IT-NORM-PERSIST-01|TestId=IT-NORM-CONCURRENCY-05|TestId=E2E-FLOW-CONFIRM-01|TestId=E2E-FLOW-NOANSWER-02|TestId=IT-ELIG-RACE-12|TestId=IT-CALLBACK-OUTBOX-06" `
  --logger "console;verbosity=minimal"
```

Kết quả: **`7/7 PASS`**, 0 fail, 0 skip, 11 giây.

| TestId | Phạm vi assertion |
| --- | --- |
| `IT-DB-OUTBOX-06` | callback outbox lease exactly-once và immutable payload |
| `IT-NORM-PERSIST-01` | final signal, evidence và privacy-safe audit persist atomically |
| `IT-NORM-CONCURRENCY-05` | concurrent normalization chỉ persist một result |
| `E2E-FLOW-CONFIRM-01` | confirm signal → outbox → local fake Sales ACK → admin visibility |
| `E2E-FLOW-NOANSWER-02` | final no-answer callback không tự đổi order state |
| `IT-ELIG-RACE-12` | stale/business blocker ACK không rewrite call result |
| `IT-CALLBACK-OUTBOX-06` | delivery completion dùng lease fencing và tạo review |

`E2E-FLOW-*` ở đây là local disposable-PostgreSQL/fake transport test ID lịch sử; tên test không
biến nó thành M3 shared E2E.

## 3. Full Chaos run

```powershell
dotnet test tests/chaos/Ivr.ChaosTests.csproj `
  --configuration Release --no-restore `
  --logger "console;verbosity=minimal"
```

Kết quả: **`8/8 PASS`**, 0 fail, 0 skip, 12 giây.

Các TestId đã chạy:

- `CHAOS-DOWNSTREAM-01`
- `CHAOS-DB-02`
- `CHAOS-SIM-03`
- `CHAOS-RECOVERY-04`
- `CHAOS-GUARD-05`
- `CHAOS-DUPLICATE-06`
- `CHAOS-TERMINATE-07`
- `CHAOS-TERMINATE-08`

Các container PostgreSQL/Toxiproxy/Ryuk tạm do test harness tạo đã tự cleanup; `docker ps` trống sau
run.

## 4. Verification record

| Gate | Kết quả |
| --- | --- |
| Docker | **PASS** — client/server `29.6.2`; daemon ready |
| Focused callback PostgreSQL | **PASS `7/7`** — 0 fail, 0 skip, 11 giây |
| Full Chaos | **PASS `8/8`** — 0 fail, 0 skip, 12 giây |
| Disposable resource cleanup | **PASS** — `docker ps` trống sau run |
| W-0162 PII scan | **PASS** — 1/1 Markdown, 0 binary skipped |
| API docs | **PASS** — 14 generated artifacts; boundary/link/topology/PII checks PASS |
| Test traceability | **PASS `476`** |
| Gate mirror | **PASS** — 11 gates, 160 work items, 23 open decisions, production=false |
| Official Markdown map | **PASS** — 649 Markdown files; W-0162 và target worklist 0 unresolved; global 199 unresolved là corpus backlog có sẵn |
| `git diff --check` | **PASS** — chỉ có line-ending warnings của shared worktree |
| GitNexus symbol impact | **N/A** — W-0162 không sửa function/class/method hoặc production source |

## 5. Non-inference và residual gates

- Không request nào đi tới M3 sandbox hoặc service ngoài local harness.
- M3 generic consumer/OAS/CDC/signature vẫn `NOT_RECEIVED`.
- Security auth profile/credential custody và Platform sandbox/network/TLS vẫn `NOT_RECEIVED`.
- Shared M3 E2E, staging, UAT, hosted CI và production delivery vẫn `NOT_RUN`.
- `TARGET_CONTRACT_V1=DRAFT`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Kết luận autonomous-local queue

Sau W-0161 và W-0162, các test local từng bị chặn chỉ vì Docker trong overlay hiện hành đã được
chạy lại. Rà 13 workstream cho thấy phần còn lại thuộc một trong các loại:

- external artifact/signature/credential/infrastructure chưa nhận;
- shared E2E/staging/UAT/production chưa có target;
- implementation mang `CODE_NOT_AUTHORIZED` vì contract/decision chưa ký;
- `B2` là `NOT_APPLICABLE`.

Do đó **`AUTONOMOUS_LOCAL_QUEUE=EMPTY` theo authority và input hiện tại**. Điều này không có nghĩa
toàn bộ 12 workstream applicable đã hoàn tất end-to-end hoặc production-ready.

# W-0064 — P1-5 Retention Job & Data Lifecycle Evidence

Trạng thái: `TESTS_PASS` (local MOCK/test DB). Không phải Legal approval, production retention policy, Sales integration hay real-customer-call evidence.

## Phạm vi đã chứng minh

- `IRetentionJob` và report model nằm ở Domain; Infrastructure thực thi catalog bằng SQL tĩnh, batch ngắn và checkpoint.
- Default: host disabled, `DryRun=true`, `PeriodDays={}`. Missing class period trả `NOT_CONFIGURED` và không mutate.
- Legal hold luôn thắng; accepted evidence và append-only audit không bị purge.
- Speech/review dùng `ANONYMIZE`; metadata còn lại dùng `DELETE`; audit/active config/control data dùng `PRESERVE`.
- Worker chạy một pass khi opt-in và tự dừng, sẵn cho P7-2 CronJob mà không cần đổi code.

## Bằng chứng test bắt buộc

| Test ID | Kết quả | Assertion chính |
| --- | --- | --- |
| `UT-RET-CONFIG-01` | PASS | tất cả 9 class thiếu owner period trả null; default run là dry-run |
| `IT-RET-DRYRUN-02` | PASS | 1 row eligible, target row giữ nguyên, report audit append |
| `IT-RET-DELETE-03` | PASS | chỉ row cũ thuộc class được yêu cầu bị xoá |
| `IT-RET-HOLD-04` | PASS | 1 row cũ trong legal hold còn nguyên |
| `IT-RET-AUDIT-05` | PASS | report aggregate được append; DB trigger tiếp tục cấm DELETE audit |
| `IT-RET-RESUME-06` | PASS | kill sau batch 1; 2 row còn lại được resume; checkpoint `COMPLETED` |
| `IT-RET-PII-07` | PASS | snapshot cũ không còn; contract/order identity giữ nguyên |

Focused command:

```powershell
dotnet test tests/Ivr.UnitTests/Ivr.UnitTests.csproj --configuration Release --no-build --filter "TestId=UT-RET-CONFIG-01"
dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj --configuration Release --no-build --filter "TestId~IT-RET-"
```

Kết quả: unit `1/1 PASS`; PostgreSQL Testcontainers integration `6/6 PASS`.

Migration regression `IT-DB-MIGRATE-01`: `1/1 PASS`, gồm apply → rollback về zero IVR table → recreate `18` IVR table. `dotnet ef migrations has-pending-model-changes`: `No changes have been made to the model since the last migration.`

Official Markdown mapper sau cập nhật: `409` file, `374` link resolved, `0` unresolved.

## Report fixture đã chạy

- `docs/evidence/W-0064/dry-run-report.json`: report aggregate từ `IT-RET-DRYRUN-02`.
- `docs/evidence/W-0064/real-run-report.json`: report aggregate từ delete/hold/resume/anonymize integration cases.
- Chiến lược, guard và period source chuẩn: [DB-05](../../../specs/database/05-retention-and-privacy.md).

Các JSON chỉ chứa aggregate test evidence, không chứa row ID hoặc dữ liệu khách hàng.

## Residual gates

- `DF-07` / `OD-V1-11`: `OWNER_DECISION_REQUIRED` — chưa có retention period Legal/Privacy ký cho 9 class.
- Hosted pipeline/MR review: không dùng để nâng verdict của work này; thực hiện riêng theo platform workflow.
- Lab SIM, Sales API, telephony, production: `NOT_RUN`, không cần cho retention engine local và không được suy ra từ test DB.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi.

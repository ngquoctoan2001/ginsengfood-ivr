# TODAY-04 — Target-DB preflight handoff

Ngày lập: `2026-08-29`

Repository HEAD lúc kiểm tra: `main@0baed74cd384cd661aed068c263a92ef97ead1f4`

Trạng thái: **COMPLETE_AS_BLOCKED — PREFLIGHT_READY / OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN**

Working tree đang chứa WIP song song ngoài TODAY-04; đây không phải immutable release candidate.

## 1. Kết luận

Phần chuẩn bị trong repo đã hoàn tất và được kiểm thử. Target DB **chưa được chạy** vì máy hiện tại
không có client, secret, endpoint hoặc bằng chứng thẩm quyền cho staging/production. Đây là blocker
thật, không phải lý do để dùng database local thay thế hoặc tự đoán trạng thái target.

Không có target-DB count nào trong tài liệu này. Mọi tuyên bố khác đều là bịa evidence.

## 2. Access audit

Kiểm tra read-only lúc `2026-08-29T02:50:08.6683527+00:00`:

| Hạng mục | Kết quả |
|---|---|
| `psql` trên `PATH` | `NO` |
| PowerShell `Get-Secret` | `NO` |
| Env key có dấu hiệu target PostgreSQL/connection string | `0` |
| File `.env*` chứa key PostgreSQL/connection string | `0` |
| Target endpoint/database/read-only role | `NOT_PROVIDED` |
| Authority/ticket cho target preflight | `NOT_PROVIDED` |
| PostgreSQL local đang chạy | `ginsengfood-postgres-local`, image `postgres:18-alpine`, port `55432` |

Container local trên **không được xác định là IVR target DB** và không được dùng để đóng gate này.

## 3. Preflight đã khóa

- Query: [`tools/ops/od18-legacy-skip-preflight.sql`](../../tools/ops/od18-legacy-skip-preflight.sql)
- Runner: [`tools/ops/Invoke-Od18Preflight.ps1`](../../tools/ops/Invoke-Od18Preflight.ps1)
- SHA-256 query: `203c5fd173384cc0c09e51b115ff841fdf40eb91b8cd6510d7a962c84961dd7a`
- Query chỉ có `18` câu `SELECT`; không có câu lệnh ghi hoặc DDL.
- Output gồm:
  - migration count, latest migration và toàn bộ migration inventory;
  - inventory của legacy columns và constraints;
  - kiểm tra constraint còn cho phép `TASK_SKIPPED_TRUSTED_CUSTOMER`/`SKIPPED`;
  - các count dữ liệu legacy và khoảng thời gian first/last seen.

Stop rule bắt buộc:

1. Migration inventory khác approved deployment manifest: `SCHEMA_DRIFT`, dừng.
2. `task_legacy_column_count != 5`, `job_legacy_column_count != 3` hoặc
   `legacy_constraint_count != 3`: `SCHEMA_DRIFT`, dừng.
3. Bất kỳ constraint-presence metric nào khác `true`: `SCHEMA_DRIFT`, dừng.
4. Không có target authority/read-only credential: `OWNER_DATA_REQUIRED`, không chạy.
5. Không được sửa target, đổi tên migration hoặc dùng local DB để hợp thức hóa gate.

## 4. Local verification

| Gate | Kết quả |
|---|---|
| PowerShell parser cho runner | `PASS` — 0 parse error |
| SQL read-only static check | `PASS` — 18 statement, 18 `SELECT`, 0 non-SELECT |
| `IT-M3-AUTHORITY-13` trên migrated PostgreSQL schema | `PASS` — 1/1 |
| GitNexus impact của test method được sửa | `LOW` — 0 caller, 0 process, 0 module |

GitNexus `detect-changes` trên **toàn working tree** báo `CRITICAL` — 60 file, 157 symbol, 45
execution flow — vì cây đang mang WIP song song. Kết quả aggregate này không được gán cho TODAY-04
và cũng là lý do không commit hoặc suy release readiness từ lượt làm này.

Lệnh test:

```powershell
dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj `
  --no-restore `
  --filter "TestId=IT-M3-AUTHORITY-13"
```

Local test chỉ chứng minh query chạy đúng trên schema test đã migrate từ working tree hiện tại. Nó
**không** chứng minh target DB đã đúng schema, không thay target evidence, không xác minh một clean
release candidate và không cho phép gọi production-ready.

## 5. Điều kiện để người có quyền chạy target

Phải cung cấp đủ trước khi chạy:

- target environment, hostname/alias, database và read-only role được xác nhận;
- authority/ticket cho phép truy vấn target;
- expected migration manifest để đối chiếu;
- `psql` hoặc client được môi trường phê duyệt;
- secret injection qua `.pgpass`, `PGPASSWORD` do secret store cấp, hoặc cơ chế được phê duyệt.

Không đặt password trong command line, tài liệu, terminal history hoặc evidence pack.

Khi các điều kiện trên đủ, chạy runner từ repository root:

```powershell
./tools/ops/Invoke-Od18Preflight.ps1 `
  -ConnectionString "postgresql://ivr_reader@approved-target:5432/ivr" `
  -Environment "approved-environment"
```

Giá trị secret phải được nạp trước bằng cơ chế của môi trường; ví dụ trên cố ý không chứa password.

## 6. Evidence phải trả về

- target logical name và authority/ticket;
- operator, reviewer và thời gian UTC;
- query path + SHA-256;
- toàn bộ output từ `OD18_PREFLIGHT_BEGIN` tới `OD18_PREFLIGHT_END`;
- kết quả đối chiếu migration inventory với approved manifest;
- kết luận `PASS`, `SCHEMA_DRIFT` hoặc `DATA_REQUIRES_RETENTION_PLAN`;
- bằng chứng secret đã được inject an toàn, không ghi lại giá trị secret.

## 7. Handoff và chữ ký

> **HANDOFF NỔI BẬT — OWNER BLOCKER CONFIRMED**
>
> Phần local của TODAY-04 đã xong. Target run vẫn là `TARGET_DB_NOT_RUN` vì thiếu quyền và dữ liệu
> vận hành nêu tại §2. Không ai được hạ gate bằng giả định, database local hoặc local test.
>
> **Người ký:** **Tôi — Module 8 / Project Owner**
>
> **Ngày ký:** **29/08/2026**
>
> Chữ ký này xác nhận blocker và handoff; không cấp target credential, không thay authority ticket,
> và không xác nhận target DB đã pass.

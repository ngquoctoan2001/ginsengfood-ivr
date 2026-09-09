# W-0261 — DSAR không còn tự giới hạn mình, và ba bảng ánh xạ thành một

Ngày: 2026-09-09 · Baseline: `main@a032c30` · Trạng thái: **TESTS_PASS**.

P3 và S2 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

**S2 đóng một phần, và mức nguy hiểm của nó thấp hơn tôi đã viết.** Mục 2.1 đính chính.

## 1. P3 — DSAR

### 1.1 Trần tham số

`FindAsync` kéo mọi `order_id` rồi mọi `job_id` về tiến trình, sau đó gửi ngược lại trong
`IN (...)`. Đó là **một tham số cho mỗi dòng**, đối đầu với giới hạn 65.535 tham số cho một câu lệnh
của PostgreSQL.

Nghĩa là một đơn hàng đủ nhiều lịch sử sẽ **không trả lời được nữa** — và lỗi sẽ đến dưới dạng lỗi
driver, giữa một yêu cầu truy cập dữ liệu cá nhân.

Giờ chúng là subquery, không rời database. Bộ lọc đằng sau là một phép so bằng có index trên một
`order_code`.

### 1.2 Báo cáo ghép từ tám thời điểm

Sáu phép đếm chạy ngoài transaction mô tả sáu thời điểm khác nhau, và retention hay một cuộc gọi
đang chạy có thể dời một dòng giữa chúng. Câu trả lời đưa cho chủ thể dữ liệu có thể cho thấy một
job không có attempt nào, hoặc attempt thuộc một job mà chính nó vừa nói là đã hết.

Giờ cả báo cáo nằm trong một transaction `REPEATABLE READ` — tên PostgreSQL đặt cho đúng bảo đảm
cần ở đây. Không có gì bên dưới ghi, nên không có xung đột serialisation nào để mất.

### 1.3 Hai con số trong audit row có thể mâu thuẫn

`EraseAsync` đếm `matched` bằng một câu lệnh rồi UPDATE bằng câu khác. Một task đến hoặc đi giữa hai
câu để lại bản ghi **vĩnh viễn** nói rằng tìm thấy một số và thay đổi một số khác — về cùng một lần
xoá, trong đúng cái artifact mà một chủ thể dữ liệu hoặc một cơ quan quản lý sẽ được cho xem.

Không sửa bằng cách thêm transaction, mà bằng cách bỏ phép đếm: **row count của chính câu UPDATE là
số đã khớp**, vì vị từ duy nhất của nó là `order_code` — mọi task nó khớp là task nó đã redact. Ít
hơn một query, và hai con số không thể lệch nhau được nữa.

## 2. S2 — magic string

### 2.1 Đính chính: write-side không im lặng

Báo cáo hàm ý rằng một status literal sai sẽ hỏng âm thầm. **Không đúng ở phía ghi.** Có
`CHECK constraint` trên các cột status (`ck_ivr_call_jobs_status` liệt kê đủ 30 giá trị), nên
`job.Status = "HELD_ADMIN_REVEW"` bị database từ chối lúc INSERT/UPDATE, không im lặng.

Tôi cũng đo phía đọc: trích mọi giá trị mà CHECK constraint cho phép (100 giá trị), rồi lấy mọi
token `UPPER_SNAKE` xuất hiện trong vị từ raw SQL của `src/` (15 token). Kết quả:

```
tokens the database could never store: 2
  NOT_CONFIGURED   src/Ivr.Infrastructure/Retention/RetentionJob.cs:473
  REDACTED         src/Ivr.Infrastructure/Retention/RetentionTargetCatalog.cs:29
```

Cả hai là **false positive**: `ivr_retention_checkpoints.status` và `phone_validation_status` không
có CHECK constraint nào, nên không có tập hợp lệ để đối chiếu. **Không có literal SQL nào hiện đang
sai.** Không tìm thấy bug đang tồn tại.

Mức đúng của S2 là chi phí bảo trì, không phải lỗi đang chạy.

### 2.2 Cái thật sự đáng sửa: ba bảng ánh xạ

Ánh xạ `ExecutionMode` → chuỗi tồn tại **ba lần**:

| Nơi | Hình thức |
| --- | --- |
| `ScriptLifecycleApiService.cs:126` | `FrozenDictionary<ExecutionMode, string>` |
| `AttemptPolicyRegistries.cs:105` | `switch` biểu thức |
| `AttemptPolicyRegistryWriter.cs:114` | `switch` biểu thức |

Ba bản sao của một hàm toàn phần trên một enum ba thành viên là ba chỗ để quên khi thành viên thứ tư
xuất hiện — và **compiler không giúp được**, vì cả ba đều có nhánh `_ =>` biến một case thiếu thành
một throw lúc chạy.

Giờ là một `ExecutionModes.ToWireValue`. `IvrOptions` lấy hằng số của nó từ đó chứ không ngược lại.

11 literal `"LAB_REAL_SIM"`/`"PRODUCTION_REAL"` còn lại thay bằng hằng số. Sau khi sửa, hai chuỗi đó
chỉ còn xuất hiện ở đúng chỗ khai báo.

### 2.3 `"MOCK"` cố ý không đụng tới

Nó được khai báo **hai lần**, bởi `IvrOptions.MockExecutionMode` và
`FeatureFlagCatalog.MockSimProvider`, cho hai thứ khác nhau tình cờ viết giống nhau — một execution
mode và một SIM provider. 22 site dùng literal đó, và một quy tắc thay tất cả sẽ **sai ở khoảng một
nửa**. Chọn chủ sở hữu nào thắng không phải việc một lượt refactor quyết định.

Cùng lý do cho `FAKE_TARGET_V1` (2 chủ) và `IVR_OPERATIONAL_BLOCKED` (2 chủ).

### 2.4 Một quyết định đặt file, và lý do

Tôi đặt `ExecutionModes` cạnh enum trong `AttemptPolicy.cs` trước, rồi gate sweep đỏ:

```
ATTEMPT_POLICY_BUNDLE_REFUSED: src/Ivr.Domain/Confirmation/AttemptPolicy.cs drifted from its pinned SHA-256
```

Tệp đó là **artifact được ghim cho một production attempt-policy bundle**. Re-pin nó là quy trình
hợp lệ, nhưng sẽ khiến một bundle đã được owner duyệt trỏ tới một hash khác — vì một thay đổi thuần
bổ sung không liên quan gì đến chính sách attempt.

Nên tôi tách `ExecutionModes` sang file riêng. `AttemptPolicy.cs` giờ **giống hệt từng byte** với
bản đã ghim, và không có pin nào phải đụng tới. Đây là lựa chọn tốt hơn ngay cả khi không có pin —
pin chỉ là thứ chỉ ra điều đó.

## 3. Test

| TestId | Khẳng định | Đỏ được trên code cũ? |
| --- | --- | --- |
| `COMP-DSAR-07` | `FindAsync` không gọi `ToArrayAsync`/`ToListAsync` | **Có** |
| `ARCH-CONST-01` | `"LAB_REAL_SIM"`/`"PRODUCTION_REAL"` chỉ tồn tại trong `ExecutionModes.cs` | **Có** |
| `COMP-DSAR-05` | `tasks_matched` == `tasks_redacted` trong audit row | Không — chốt bảo toàn bất biến |
| `COMP-DSAR-06` | Báo cáo đúng với một đơn hàng có 31 job | Không — chốt bảo toàn ngữ nghĩa |

Nói thẳng về hai cái sau: chúng **pass ở cả hai phía**. Code cũ cũng cho `matched == redacted` khi
không có mutation song song, và cũng chạy đúng ở 31 job (trần là 65.535). Chúng là chốt giữ tính
chất, không phải test bắt bug — ghi ra đây thay vì để chúng trông như bằng chứng mạnh hơn thực tế.

Trần tham số không kiểm được bằng hành vi: cần gieo hàng chục nghìn dòng. Cái kiểm được là **hình
dạng mã** — một `ToArrayAsync` quay lại chính là chỉnh sửa đưa danh sách id trở lên đường truyền, và
`COMP-DSAR-07` đọc đúng thân `FindAsync` để bắt điều đó.

`ARCH-CONST-01` đã được làm cho đỏ thật: đưa lại một literal vào `AsteriskAriOptions.cs` cho ra
`src\Ivr.Infrastructure\Telephony\AsteriskAriOptions.cs:66 writes "LAB_REAL_SIM"`.

## 4. Kết quả

```
Unit         663/663   (+1)
Integration  278/278   (+3)
Contract      24/24
Chaos          8/8
             ─────────
             973/973
```

`GATE_SWEEP_PASS 39/39` · `dotnet build Ivr.sln` — 0 warning, 0 error.

Impact analysis trước khi sửa: `DsarService.FindAsync` — **LOW**, 2 caller trực tiếp.

## 5. Còn lại

**14/26 đã đóng.** Không còn HIGH.

Cần owner, không cần thêm công:

- **P2** — 109 index, nhiều cái trên cột boolean. Cần `pg_stat_user_indexes.idx_scan` từ staging
  hoặc production.
- **Retention chưa arm** — `PeriodDays` rỗng.
- **`"MOCK"` có hai chủ sở hữu** — `IvrOptions.MockExecutionMode` hay
  `FeatureFlagCatalog.MockSimProvider`? Nếu tách tên thì 22 site kia thay được an toàn.

Thuần kỹ thuật: **S6** (434k dòng docs không liên quan), **S7** (worktree rác — `git worktree prune`
cộng xoá `.claude/worktrees/gd0-fixes`), **S1**, **S3**, **S5**, **B9**, **B10**, **C1–C5**, và phần
S4 còn lại (`assertString`, `assertExactKeys`, `readStrictJson` — không cái nào là kiểm bảo mật).

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN

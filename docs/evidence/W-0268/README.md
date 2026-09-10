# W-0268 — Một thư mục tạm không ai dọn, và một index không query nào dùng được

Ngày: 2026-09-10 · Baseline: `main@466c190` · Trạng thái: **TESTS_PASS**.

B9 và B10 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

## 1. B9 — cả hai câu trong báo cáo đều sai, theo hai hướng ngược nhau

Báo cáo viết: rò rỉ ở **hai** file. Đo lại thì:

**Rò rỉ *chạm tới được* chỉ ở một file.** Điều kiện để rò rỉ là có một câu lệnh **ném** nằm giữa
`mkdtemp` và `try`. Khoảng hở tồn tại ở cả bốn file, nhưng ba trong bốn chỉ chứa **định nghĩa
closure** — không câu nào chạy lúc đó.

| File | Khoảng hở | Có gì ném trong đó |
| --- | ---: | --- |
| `external-decision-response-validator` | 17 dòng | **`parseAndVerifyManifest()`** — kiểm hash, ném khi manifest lệch |
| `capacity-registry-decision-pack-validator` | 19 dòng | không |
| `external-decision-routing-validator` | 17 dòng | không |
| `upstream-session-signoff-validator` | 16 dòng | không |

Khớp với bằng chứng: năm thư mục còn sót đều mang tiền tố `.w0165-`, tức của **một** file đó.

**Nhưng phạm vi thì rộng hơn báo cáo: bốn file mkdtemp vào repo root, không phải hai.** Báo cáo bỏ
sót `external-decision-routing-validator` và `upstream-session-signoff-validator`.

### 1.1 Sửa

Hai việc, tách bạch:

**(a) Chỗ rò rỉ thật** — đưa `parseAndVerifyManifest()` lên **trước** khi thư mục tạm tồn tại. Không
đổi ngữ nghĩa: `manifest` chỉ được dùng bên trong `try`.

**(b) Vị trí** — cả 4 chuyển từ repo root sang `resolve(REPOSITORY_ROOT, "ci-artifacts")`. Đây
không phải phát minh: **bảy** validator anh em đã dùng đúng dòng đó, và `ci-artifacts/` đã nằm
trong `.gitignore:11`. Vẫn nằm trong repo nên `isConfined` không đổi.

Không thêm pattern ignore nào cho `.w0*-selftest-*`: đó là ignore cho một vị trí vừa bị bỏ.

### 1.2 Chứng minh

Bắt manifest lệch — đúng lỗi gate vẫn báo — rồi đếm thư mục còn sót:

| | Thông điệp | Sót ở repo root |
| --- | --- | ---: |
| Bản `HEAD` | `W0165_VALIDATION_FAILED: artifact manifest drifted…` | **1** |
| Bản sửa | `W0165_VALIDATION_FAILED: artifact manifest drifted…` | **0** |

Cùng lỗi, cùng thông điệp, khác chỗ rò rỉ.

### 1.3 Một vết copy-paste sửa luôn

`upstream-session-signoff-validator.mjs` dùng tiền tố `.w0178-selftest-` trong khi mọi token của nó
là `W0181` — `w0178` là của `d06`. Đổi thành `w0181-`, trên đúng dòng đang viết lại.

## 2. B10 — điểm mạnh nhất lại không nằm trong báo cáo

Báo cáo nói `IdempotencyKeyEntity.ExpiresAt` không có writer, và nêu **một** đường ghi. Có **bốn**:

| Đường ghi | Set `ExpiresAt`? |
| --- | --- |
| `TaskIntakeStores.CreateIdempotency:330` | không |
| `PostgresIdempotencyStore.cs:71` | không |
| `PostgresFeatureFlagCommandIdempotency.cs:63` | không |
| `InternalAdminApiService.cs:975` | không |

Bốn đường độc lập cùng bỏ trống nó thuyết phục hơn hẳn một call site đãng trí.

**Không reader nào.** Query duy nhất có `Where` trên bảng này (`TaskIntakeStores.cs:276`) lọc theo
`Scope`/`Key` và sắp theo `CreatedAt`; mọi chỗ còn lại là `FindAsync` theo khoá chính, `CountAsync`,
`AnyAsync(Key == …)`.

**Nơi duy nhất gán giá trị là một test**: `RetentionJobTests.cs:376` đặt
`ExpiresAt = createdAt.AddHours(1)`. Fixture đang dựng một trạng thái production không tạo ra.

### 2.1 Điều báo cáo bỏ lỡ, và nó đổi cả tính chất của việc sửa

`specs/database/04-indexes.md:29` khai đúng **một** index cho bảng này — `created_at`, cho
retention/purge scan. Index `expires_at` **chưa bao giờ có trong spec**.

Nên bỏ nó không phải là "đơn giản hoá theo ý mình" mà là **đưa model về đúng spec**.

### 2.2 Bỏ index, giữ cột — và lý do không phải sự rụt rè

`specs/database/02-tables.md:191` **có** khai `expires_at`. Bỏ cột sẽ trái spec, và về mặt vận hành
là một `DropColumn` trên bảng mà release trước vẫn đọc — đúng thứ `UT-SCHEMA-BACKCOMPAT-01` từ chối.
Checker đó liệt kê `DropColumn`, `DropTable`, `RenameColumn`, `AlterColumn` thu hẹp và `CreateIndex`
unique; nó **không** liệt kê `DropIndex`, vì index vô hình với code cũ.

Bốn index còn lại của bảng: `RetainUntil`/`LegalHoldUntil`/`AnonymizedAt` do
`PersistenceModelConfiguration:659-667` cấp tự động cho **mọi** `RetainedEntity`, cộng `created_at`
mà spec khai.

### 2.3 Test

`UT-SCHEMA-IDEMPOTENCY-INDEX-01` khẳng định tập index của bảng đúng bằng bốn cái trên, và khẳng định
**cột vẫn còn** — để việc giữ cột là một quyết định được ghi, không phải một chỗ quên.

Đã làm cho đỏ thật: thêm lại `HasIndex(entity => entity.ExpiresAt)` → **Failed: 1**; gỡ ra →
**Passed: 1**.

## 3. Cảnh báo impact, và nó thật sự nói gì

`gitnexus_impact` trên `IdempotencyKeyEntity` trả về **CRITICAL**, 251 symbol, 72 direct, 12 process.

Ghi lại theo yêu cầu của `CLAUDE.md`, kèm cách đọc: đó là blast radius của **cả class** — bản ghi mà
mọi request mutating đi qua idempotency store đều ghi. Thay đổi ở đây là **xoá một dòng `HasIndex`**:
không đổi API C#, không đổi câu query nào, không đổi cột nào. Không call site nào trong 251 symbol
đó chạm `ExpiresAt` — đã kiểm từng usage của `DbSet<IdempotencyKeyEntity>`.

Con số CRITICAL đúng cho câu hỏi nó được hỏi; nó không phải câu hỏi mà thay đổi này đặt ra.

## 4. Kết quả

```
Unit         668/668   (+1)
Integration  283/283
Contract      24/24
Chaos          8/8
             ─────────
             983/983
```

`dotnet build Ivr.sln` — 0 warning, 0 error.

Gate sweep (repo thật): **37/39 run**, 2 FAIL — cả hai là pin
`integration-requirements/06-module-3-api-handover.md` của agent khác, đỏ từ trước lượt này.

So verdict giữa `HEAD` sạch và cây của tôi (hai bản `git archive`, chạy sweep cả hai):
**0 thay đổi**.

Migration sinh bằng `tools/dev/Add-IvrMigration.ps1` (startup project là chính `Ivr.Infrastructure`;
`Ivr.Api` không tham chiếu `EFCore.Design`). Bản EF sinh ra vi phạm analyzer của repo — thiếu
namespace file-scoped và `ArgumentNullException.ThrowIfNull` — nên đã viết lại theo khuôn của
`W0249OrderRevocationFence`.

Cascade re-pin: `external-decision-response-validator` và `external-decision-routing-validator` đổi
sha, re-pin ở đúng hai chỗ pin sống — `external-decision-closure-validator.mjs` và
`docs/evidence/W-0170/decision-closure-input.template.json`.

## 5. Một quan sát cho owner, không tự sửa

`prompt/_execution/prompt-execution-tracker.md:24` ghi `NEXT_WORK_ID = W-0250`, trong khi
`docs/evidence/` đã tới `W-0267`. Tracker tự tuyên bố là nguồn sự thật duy nhất cho Work ID và cấm
tái sử dụng ID, nhưng đang **lệch 18 ID**; cả hai agent đang cấp phát bằng cách quét thư mục
evidence. Không tự sửa vì file này agent khác cũng đang ghi (đã commit nó trong cả `W-0265` lẫn
`W-0266`).

## 6. Còn lại

**21/26 đã đóng, 1 rút lại vì sai.** Không còn HIGH.

Cần owner:

- **P2** — 109 index. Cần `pg_stat_user_indexes.idx_scan` từ staging hoặc production. B10 vừa gỡ
  một index bằng lập luận tĩnh (không writer, không reader, không có trong spec); 108 cái còn lại
  không gỡ được kiểu đó.
- **Retention chưa arm** — `PeriodDays` rỗng.
- **`"MOCK"` có hai chủ sở hữu**.
- **`phone_validation_status`** — contract nói required, intake nhận thiếu.
- **`NEXT_WORK_ID` lệch 18** (mục 5).
- Phần S4 còn lại: `assertString` (11/9), `assertExactKeys` (13/8), `readStrictJson` (6/5).

Thuần kỹ thuật còn lại: **C1**, **C3**, **C4**, **C5**.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN

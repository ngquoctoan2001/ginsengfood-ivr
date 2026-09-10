# W-0269 — Gỡ 440 lời gọi không làm gì, và viết ra một luật vốn đã đúng

Ngày: 2026-09-10 · Baseline: `main@518ecdd` · Trạng thái: **TESTS_PASS**.

C1 và C3 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

## 1. C1 — `ConfigureAwait` nửa vời

Báo cáo nói đúng vấn đề: hoặc là quy ước, hoặc là không; nửa vời thì reviewer không phân biệt được
chỗ thiếu là cố ý hay quên. Nó **không** nói thứ quyết định câu trả lời.

### 1.1 Đo trước

| | `await` | `ConfigureAwait(false)` |
| --- | ---: | ---: |
| `src/` | ~900 | **440** |
| `tests/` | 2.473 | **0** |

`tests/` đã tự chốt từ lâu, nhất quán 100%. Chỉ `src/` lệch.

Và ba dữ kiện làm cho lời gọi đó **không làm gì** ở đây:

- Không project nào packable — không `PackageId`, không `IsPackable`, không
  `GeneratePackageOnBuild`. `Ivr.Infrastructure` chỉ được `Ivr.Api`, `Ivr.Worker` và một project
  test tham chiếu, đều trong solution này.
- `Ivr.Api` là `WebApplication.CreateBuilder`, `Ivr.Worker` là `Host.CreateApplicationBuilder`.
  Không cái nào cài `SynchronizationContext`.
- Toàn bộ `src/` và `tests/` **không có một tham chiếu nào** tới `SynchronizationContext`.

Không có context để capture thì `ConfigureAwait(false)` là no-op.

### 1.2 Vì sao không đi đường "bật CA2007"

Repo đã có sẵn cơ chế: `.editorconfig` nâng `CA1062`, `CA1305`, `CA2000` lên `warning`, và
`TreatWarningsAsErrors` biến chúng thành lỗi. Thêm `CA2007` vào đó là bước hiển nhiên. Đã thử:

| | |
| --- | ---: |
| Vị trí CA2007 trong `src/` | **201** |
| trong đó `await` thường | 121 |
| trong đó **`await using`** | **80** |

Với 80 chỗ đó, **code fixer chính thức sinh ra code không biên dịch được**:

```
error CS0029: Cannot implicitly convert type 'ConfiguredAsyncDisposable'
              to 'Ivr.Infrastructure.Persistence.IvrDbContext'
```

Nó gắn `.ConfigureAwait(false)` lên cả Task lẫn `IAsyncDisposable`, rồi gán kết quả vào biến kiểu
cũ. Đã chạy thật trên `Ivr.Worker`, compiler từ chối, đã hoàn nguyên. Viết tay thì phải thêm một
biến phụ kiểu `ConfiguredAsyncDisposable` ở 80 chỗ — trả giá bằng khả năng đọc cho một no-op.

### 1.3 Quyết định của owner

Ngày 2026-09-10, owner chọn **gỡ hết 440**, không bật CA2007.

### 1.4 Làm

Một bộ biến đổi văn bản xử lý ba hình dạng, vì xoá thô sẽ để lại `;` mồ côi:

| Hình dạng | Số | Xử lý |
| --- | ---: | --- |
| `foo(x).ConfigureAwait(false);` | 285 | gỡ chuỗi con |
| `.ConfigureAwait(false);` đứng riêng một dòng | 142 | xoá dòng, trả `;` về dòng trước |
| `.ConfigureAwait(false)` riêng, không `;` | 7 | xoá dòng |

52 file, **+433 / −588**.

### 1.5 Guard, và một lỗi của tôi mà chính guard suýt che mất

`ARCH-ASYNC-01` khẳng định `src/` không còn `ConfigureAwait` nào, cùng chỗ và cùng hình dạng với
`UT-BOOT-05` và `ARCH-CONST-01` đã có sẵn. Đã làm cho đỏ thật: thêm lại **một** lời gọi → `Failed: 1`.

Lỗi: tôi chạy bộ gỡ trên **toàn** `src/`, gồm cả code sinh tự động. Nó sửa
`src/Ivr.Contracts/Generated/SalesTarget/V1/SalesTargetV1Client.g.cs` — file mà generator sở hữu và
`contract-freeze-verifier` pin hash.

Không phải tôi tự phát hiện. **Gate bắt được**: `CONTRACT_FREEZE=FAIL`. Hoàn nguyên file sinh, và
thêm ngoại lệ `.g.cs` vào guard — nếu không, guard sẽ đòi tôi tiếp tục sửa thứ không được sửa.

Nếu `contract-freeze-verifier` không pin file đó, thay đổi này đã âm thầm vào repo và biến mất ở lần
regenerate sau.

## 2. C3 — ba quy ước temp dir

### 2.1 Báo cáo dán nhãn sai, và kết luận sai về bản chất

Bảng trong báo cáo gọi nhóm giữa là `.artifacts/` và xếp `d06`, `dial-token`,
`external-decision-closure` vào đó. **Ba script này dùng `ci-artifacts/`**, một thư mục khác.
Script duy nhất thật sự dùng `.artifacts/` là `api-behavior-matrix-selftest.mjs` — **không được liệt
kê**.

Nặng hơn: báo cáo trình bày ba quy ước như một sự trôi dạt tuỳ tiện. Đo lại thì không phải.

| Lý do | Root | Số |
| --- | --- | ---: |
| Có validator confine đọc path (trực tiếp hoặc qua `spawnSync`) | `ci-artifacts/` | 11 |
| Không gì confine | `tmpdir()` | 4 |
| Scratch nằm cạnh baseline nó clone | `.artifacts/api-matrix/` | 1 |

**9/9** script gọi `isConfined` đặt scratch trong repo. **4/4** script dùng `tmpdir()` không confine
gì. Không ngoại lệ nào.

Nó **có tải**: validator gọi `isConfined` từ chối mọi path ngoài repo, nên scratch dưới `tmpdir()`
không thể đưa cho nó. Điều đó áp dụng qua cả `spawnSync` — đó là lý do
`external-decision-c9-selftest` và `external-decision-dial-token-selftest` ghi trong repo mà không tự
gọi `isConfined`: validator chúng gọi mới là bên confine.

Biến thể sai duy nhất — repo root — **đã bị gỡ ở `W-0268`**.

Nên việc cần làm không phải hợp nhất thư mục, mà là **viết luật ra và ép nó**. Một luật đúng mà không
ai viết ra thì được tuân thủ tới người đầu tiên chưa đọc mười lăm script kia.

### 2.2 Ba khẳng định, mỗi cái đã chứng minh là biết đỏ

Trong `ci-config-selftest.mjs`:

| Khẳng định | Tiêm thử | Kết quả |
| --- | --- | --- |
| Không scratch nào ở repo root | đổi routing-validator về `join(REPOSITORY_ROOT, ".w0164-…")` | đỏ, nêu đúng tên script |
| Script confine không được lấy scratch từ `tmpdir()` | đổi response-validator sang `tmpdir()` | đỏ, nêu đúng tên script |
| Scratch trong repo phải được chính script xoá | gỡ `rmSync` khỏi api-behavior-matrix | đỏ, nêu đúng tên script |

Regex trong check được viết **không dùng ký tự `\`** trong phần logic (`String.fromCharCode(92)`,
`includes()`), vì công cụ chỉnh sửa trong phiên này đã nuốt backslash **bốn lần** — một pattern bị
suy biến âm thầm sẽ để lại một check xanh mà không nhìn gì cả. Bản viết đầu đúng như vậy: nó "PASS"
trong khi `\b` và `\\` đã bị ăn mất.

### 2.3 Một rò rỉ nặng hơn B9, tìm thấy khi làm C3

`api-behavior-matrix-selftest.mjs` **không có `rmSync`, không `finally`, không gì cả**. Nó rò rỉ một
thư mục **mỗi lần chạy**, kể cả khi thành công — khác B9, vốn chỉ rò khi có exception. **11 thư mục**
`.artifacts/api-matrix/validator-selftest-*` đã tích lại.

Tìm thấy vì phép quét pin của tôi khớp trúng hash nằm trong chính đống rác đó.

Đã bọc `try/finally`. Kiểm: script này đang **fail sẵn ở `HEAD`** (nó `sweepable: false` vì verdict
phụ thuộc `ApiBehaviorMatrixTests` vừa chạy trên source hiện tại) — và trên đúng đường fail đó, số
thư mục **không tăng**. Tức `finally` chạy. 11 thư mục cũ đã xoá.

Hai script còn lại không dọn (`observability-staging-evidence`, `tts-fixed-render-selftest`) dùng
`tmpdir()`, nơi OS là chủ sở hữu. Luật chỉ đòi dọn với scratch **trong repo** — nên chúng không bị
churn vô cớ.

## 3. Cascade pin — và bốn file nguồn C# bị pin

Gỡ `ConfigureAwait` đổi hash của file nguồn, và gate **pin file nguồn C# production**. Quét toàn bộ
51 file bằng `git grep` (chỉ file được theo dõi — lần quét đầu bằng `grep -r` khớp trúng rác trong
`.artifacts/`):

| File nguồn | Pin bởi | Loại |
| --- | --- | --- |
| `Crm/SuppressionProposer.cs` | `opt-out-suppression-bundle-validator.mjs`, template `W-0187` | **sống** → re-pin |
| | `docs/evidence/W-0187/attested-sha256.txt` | **đóng băng** → giữ nguyên |
| `Intake/TaskIntakeService.cs` | `dial-token-…-validator.mjs`, `opt-out-…-validator.mjs`, template `W-0187` | **sống** → re-pin |
| `Scheduling/SchedulerCapacity.cs` | `docs/review/2026-09-07-m8-independent-full-review.evidence.json` | **đóng băng** → giữ nguyên |
| `Telephony/DialTokenResolveLedger.cs` | cùng file trên | **đóng băng** → giữ nguyên |

Căn cứ phân loại, không phải cảm tính: **không gate nào đọc** file review `2026-09-07` (`git grep`
trong `deploy/`, `tools/` — rỗng), còn `attested-sha256.txt` chính là bản đóng băng mà `W-0251`/
`W-0252` tách ra, và entry handover của nó (`b676a32d…`) đã lệch sẵn với mọi pin sống — bằng chứng nó
được cố ý để cũ.

## 4. Cảnh báo impact

`gitnexus_detect_changes` trả về **CRITICAL** — 388 symbol, 209 process, 59 file.

Ghi lại theo `CLAUDE.md`, kèm cách đọc: 51 file production bị chạm **về mặt văn bản**, nên đồ thị báo
đúng bán kính của "51 file đổi". Thứ bị gỡ là một lời gọi **không có tác dụng** trong runtime này —
đã chứng minh ở mục 1.1, không suy đoán. Kiểm chứng ở đầu ra: 984 test xanh, và so verdict gate giữa
`HEAD` sạch với cây này ra **0 thay đổi**.

## 5. Kết quả

```
Unit         669/669   (+1)
Integration  283/283
Contract      24/24
Chaos          8/8
             ─────────
             984/984
```

`dotnet build Ivr.sln` — 0 warning, 0 error.

Gate sweep (repo thật): **37/39 run**, 2 FAIL — cả hai là pin
`integration-requirements/06-module-3-api-handover.md` của agent khác, đỏ từ trước lượt này.

So verdict `HEAD` sạch ↔ cây này (hai bản `git archive`, sweep cả hai): **0 thay đổi**.

## 6. Agent khác, lần thứ tư

`opt-out-suppression-bundle-validator.mjs` và template `W-0187` mang **cả** sửa đổi của họ (pin
handover) lẫn của tôi (hai pin nguồn). Commit này giữ pin nguồn của tôi và trả dòng handover về đúng
như `HEAD`. `upstream-session-signoff-validator.mjs` lượt này không có gì của tôi nên bị loại hẳn.

Một chi tiết về phương pháp: dựng cây bằng `git update-index` trong vòng lặp `while read` qua pipeline
làm **hỏng index vì tranh lock** — bảy lệnh trượt im lặng và cây ra thiếu file (51 thay vì 58). Bắt
được nhờ đếm. Phải gọi `git update-index --add` **một lần** với toàn bộ path.

## 7. Còn lại

**23/26 đã đóng, 1 rút lại vì sai.** Không còn HIGH.

Cần owner:

- **P2** — 109 index. Cần `pg_stat_user_indexes.idx_scan` từ staging hoặc production.
- **Retention chưa arm** — `PeriodDays` rỗng.
- **`"MOCK"` có hai chủ sở hữu**.
- **`phone_validation_status`** — contract nói required, intake nhận thiếu.
- **`NEXT_WORK_ID` lệch 19** — tracker ghi `W-0250`, evidence đã tới `W-0269`.
- Phần S4 còn lại: `assertString` (11/9), `assertExactKeys` (13/8), `readStrictJson` (6/5).

Thuần kỹ thuật còn lại: **C4** (comment `Dockerfile.api` nói ngược với code), **C5** (bốn API service
đều `Singleton`, không gì bảo vệ ràng buộc đó).

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN

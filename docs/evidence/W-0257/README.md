# W-0257 — Dashboard: 12 query thành 8, và một cột không có thứ tự

Ngày: 2026-09-09 · Baseline: `main@ce364d4` · Trạng thái: **TESTS_PASS**.

P1 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md) —
mục HIGH cuối cùng còn lại. Không đổi schema, không migration, không đổi API contract.

## 1. Đính chính con số trong bản rà soát

Bản rà soát viết **"30 round trip tuần tự"**. Sai. Con số đó ra từ `grep -c 'await '` trên một
khoảng dòng do `awk` cắt, và khoảng đó tràn sang method kế tiếp; mỗi query cũng đóng góp hai lần vì
`ConfigureAwait(false)` nằm trên dòng riêng.

Đếm đúng, theo lời gọi thực sự chạm database:

```
$ awk 'NR>=74 && NR<=200' AdminReadService.cs \
    | grep -c 'ToListAsync(\|CountAsync(\|AnyAsync(\|SumAsync(\|FirstOrDefaultAsync(\|CreateDbContextAsync('
13      # trước
9       # sau
```

Tức **12 query + 1 lần mở context → 8 query + 1**. Vẫn là vấn đề thật, chỉ không nghiêm trọng
bằng con số đã in. Đã sửa tại chỗ trong bản rà soát.

## 2. Vấn đề nặng hơn số lượng query

`jobIds` là một `IQueryable<string>` **chưa thực thi**:

```csharp
IQueryable<string> jobIds = jobs.Select(job => job.IvrCallJobId);
```

Nó xuất hiện hai lần trong mã nguồn, nhưng cái thứ hai gán vào `attempts`, và `attempts` được
**thực thi bốn lần** — mỗi `CountAsync`/`SumAsync` là một query riêng, mỗi query mang theo trọn vẹn
subquery đó:

```csharp
IQueryable<CallAttemptEntity> attempts = context.CallAttempts.AsNoTracking()
    .Where(attempt => jobIds.Contains(attempt.IvrCallJobId));
int attemptTotal      = await attempts.CountAsync(...);                    // subquery lần 1
int countedAttempts   = await attempts.CountAsync(...);                    // lần 2
int technicalRetries  = await attempts.SumAsync(...);                      // lần 3
int activeAttempts    = await attempts.CountAsync(...);                    // lần 4
```

Cộng `resultCounts` là **5 lần** subquery chạm database. Khi gọi không filter — đúng thứ xảy ra khi
ai đó mở dashboard — `jobs` là toàn bộ `ivr_call_jobs`, nên mỗi lần là một semi-join full-table,
không phải chỉ một hop mạng.

Còn **2 lần** sau khi sửa.

> Lưu ý cách đo: `grep -c 'jobIds.Contains'` cho ra 2 ở cả trước và sau, vì nó đếm văn bản chứ
> không đếm lần thực thi. Con số 5 đọc ra từ mã, không phải từ grep.

## 3. Sửa gì

**Gộp hai tile của `jobs` thành một aggregate.** `nearExpiry` và `blocked` đếm cùng một tập row
dưới hai predicate khác nhau:

```csharp
JobTotals jobTotals = await jobs
    .GroupBy(_ => 1)
    .Select(group => new JobTotals(
        group.Count(job => job.ClosedAt == null && job.ExpiresAt > now && job.ExpiresAt <= nearExpiryCutoff),
        group.Count(job => job.ClosedAt == null && !job.Eligible)))
    .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
    ?? new JobTotals(0, 0);
```

**Gộp bốn counter của `attempts` thành một aggregate.** Cùng hình dạng. Đây là chỗ ăn nhiều nhất:
4 query → 1, và subquery 4 lần → 1.

EF Core dịch được cả hai thành một `SELECT` với `COUNT(CASE WHEN ...)`. Không có client evaluation
— EF Core từ 3.0 ném lỗi thay vì âm thầm kéo dữ liệu về, nên nếu không dịch được thì integration
test đã đỏ. Chúng xanh.

**`SimChannels`: chiếu cột và sắp thứ tự.** Trước đây `ToListAsync()` nguyên `SimChannelEntity`
trong khi panel chỉ đọc 5 trường; entity còn mang lease token, một loạt timestamp và các cột
`RetainedEntity`. Giờ chiếu vào `SimChannelSummary`.

Không đặt `Take()`: chặn số lượng ở đây sẽ làm panel **đếm thiếu trong im lặng**, tệ hơn hẳn việc
tải 50 dòng. SIM channel là kênh vật lý, số lượng là hàng chục chứ không phải hàng nghìn.

## 4. Một bug đi kèm: cột không có thứ tự

`BuildSimPanel` báo adapter mode của **dòng đầu tiên**:

```csharp
channels.Count > 0 ? channels[0].AdapterMode : executionMode
```

Query gốc không có `ORDER BY`. Một query không có `ORDER BY` thì **không có dòng đầu tiên** —
PostgreSQL được tự do trả về theo thứ tự nào cũng được, và thứ tự đó đổi theo plan, theo vacuum,
theo việc row có bị update hay không. Nghĩa là ô "adapter mode" trên dashboard có thể đổi giá trị
giữa hai lần F5 mà không có gì thay đổi cả.

Chưa ai báo lỗi này, gần như chắc chắn vì môi trường thật chỉ có một channel. Nó sẽ xuất hiện đúng
lúc pool mở rộng.

Đã thêm `.OrderBy(channel => channel.SimChannelId)`.

## 5. Rủi ro tự tạo, và cách chặn

Gộp counter thành grouped aggregate đổi hành vi trên **database rỗng**: `CountAsync` trả `0`, còn
`GroupBy(_ => 1)` trả **không có group nào** — không phải một dòng toàn số 0. Nên mọi con số đó giờ
đi qua `FirstOrDefaultAsync` có thể null hợp lệ.

Hai lớp chặn:

1. **Compiler.** Nullable reference types bật cùng `TreatWarningsAsErrors=true` (`Directory.Build.props:3-4`)
   khiến nhánh null không thể bỏ qua lặng lẽ. Tôi đã thử gỡ hai toán tử `??` để kiểm chứng: build
   đỏ ngay với `CS8600` và `CS8602`, không cần chạy test nào.
2. **`IT-ADMIN-READ-12`.** Compiler không nói được rằng câu trả lời đúng là *số 0*. Test gọi
   dashboard trên database vừa reset và khẳng định từng ô là 0, `Open_incidents` rỗng, và
   `Sim.Adapter_mode` lùi về execution mode đã cấu hình thay vì chuỗi rỗng.

   Test này **pass ở cả code cũ lẫn mới** — đúng vai trò: nó chốt hành vi mà thay đổi này đặt vào
   thế rủi ro, không phải bắt một bug đã có. Một deployment rỗng cũng là thứ đầu tiên người ta mở
   console lên xem, nên sai ở đây thì lộ ngay ngày đầu và không có test seeded nào bắt được.

## 6. Không làm gì, và tại sao

**Không chạy các query song song.** `DbContext` của EF Core không thread-safe, nên song song hoá
đòi nhiều context — tức nhiều connection từ pool cho một request. Với một endpoint dashboard mà
nhiều người có thể mở cùng lúc, đó là đổi latency lấy áp lực pool. Gộp query giảm cả hai.

**Không gộp `paused` vào `incidents`.** Cùng bảng `ivr_capacity_incidents`, nhưng `incidents` bị
`Take(20)` và sắp theo `OpenedAt` giảm dần, nên một incident pause hoàn toàn có thể nằm ngoài top
20. Gộp lại là đổi ngữ nghĩa để lấy một round trip.

**Không gộp `queueCounts`.** Nó group theo `(QueueStatus, Closed)` — khoá khác, không gộp được vào
aggregate hằng số.

## 7. Kết quả

| | Trước | Sau |
| --- | --- | --- |
| Query chạm DB | 12 | **8** |
| Subquery `jobIds` thực thi | 5 | **2** |
| Cột `ivr_sim_channels` kéo về | toàn bộ entity | 5 |
| Thứ tự `SimChannels` | không xác định | `sim_channel_id` |

```
Unit         657/657
Integration  275/275   (+1: IT-ADMIN-READ-12)
Contract      24/24
Chaos          8/8
             ─────────
             964/964
```

`dotnet build Ivr.sln` — 0 warning, 0 error.

Impact analysis trước khi sửa: `AdminReadService.GetDashboardAsync` — **LOW**, 1 caller trực tiếp
(`IAdminReadService` 2). Không có gì để cảnh báo.

## 8. Còn lại

**7/26 phát hiện đã đóng. Không còn mục HIGH nào.** Còn lại toàn MEDIUM và LOW:

- **B7** — `RotatingCredentialProvider.ActiveGenerations` trả thẳng `byte[] SecretBytes`; `record`
  chứa `byte[]` nên `Equals` so sánh tham chiếu; `Fingerprint` là SHA-256 thô cắt 48 bit.
- **B8** — 9/10 bản `isConfined` không realpath `REPOSITORY_ROOT`; gate CI vỡ toàn bộ dưới symlink.
- **B5** — `InMemoryIdempotencyStore` rò rỉ không giới hạn ở MOCK.
- **P2** — 109 index, nhiều cái trên cột boolean. Cần đo `pg_stat_user_indexes` trên môi trường
  thật trước khi drop.
- **S4** — helper bảo mật copy-paste 8–18 lần, đã phân hóa thành 2 semantics.
- **S2** — 951 magic string.
- **S7** — worktree rác, bản sao repo trong `.claude/worktrees/`.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN

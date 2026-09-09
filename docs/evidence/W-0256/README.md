# W-0256 — Ba đường fail-closed không cho ai biết chúng đang chạy

Ngày: 2026-09-09 · Baseline: `main@24e82f6` · Trạng thái: **TESTS_PASS**.

Ba phát hiện tiếp theo từ [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md):
B3, B4, B6. Điểm chung của cả ba: hệ thống **xử lý sự cố đúng** rồi **không nói với ai**, hoặc nói
theo cách khiến người đọc kết luận sai.

Không đổi schema, không migration, không đổi hành vi khi mọi thứ chạy bình thường.

## 1. B3 — `catch (Exception)` không bind, không log, không metric

`EligibilityService.GetCapacityFailClosedAsync` bắt mọi thứ rồi trả về snapshot
`CAPACITY_SOURCE_UNREADABLE`. Fail-closed là hành vi **đúng**. Vấn đề là nó không phân biệt được
với hoạt động bình thường: caller nhận một snapshot hợp lệ trong cả hai trường hợp, exception bị
vứt đi (`catch (Exception)` không có biến), không counter nào nhúc nhích, không dòng log nào ra.

Một capacity provider hỏng vĩnh viễn trông y hệt một hệ thống khoẻ mạnh đang thận trọng.

Nặng hơn cả chuyện chẩn đoán: `PiiGuard.EnsureSafeText` được gọi ngay trên đó (dòng 186) và nó ném
khi **dữ liệu cá nhân thật sắp rời khỏi biên**. Sự kiện nghiêm trọng nhất mà method này có thể phát
hiện được rơi vào đúng nhánh im lặng với "Postgres đang chết".

**Sửa:** bind exception, đếm `IvrTelemetry.RecordFailClosed` với reason code khớp giá trị ghi vào
snapshot, log **tên type chứ không phải exception**, và gắn tag lên span hiện hành.

Ghi tên type là có chủ ý, theo đúng pattern `FeatureFlagPlatform.cs:80-83` đã dựng: message của
capacity provider có thể trích nguyên một dòng dữ liệu nó đang đọc, mà toàn bộ lý do `PiiGuard` tồn
tại là để văn bản đó không bị chép đi đâu cả. Type là thứ tách được hai trường hợp operator cần
phân biệt — `InvalidOperationException` là guard hoặc vi phạm hợp đồng, còn lại là nguồn không
với tới được.

`ILogger` thêm vào dạng **optional** (`= null`), theo đúng `FeatureFlagPlatform`, nên 11 chỗ dựng
trực tiếp `new EligibilityService(...)` trong test không phải sửa.

## 2. B4 — không backoff: outage biến thành bão log và bão connection

Bốn worker loop bắt mọi exception, log, rồi thử lại sau **đúng** poll interval. Mặc định 1000 ms,
và **100 ms** dưới profile LocalMockE2E — con số này nằm ngay trong comment của
`SchedulerJobHost`.

Postgres chết 10 phút, 2 replica: mỗi loop ném ra một dòng `Error` kèm stack trace và mở một
connection mới, 10 lần một giây. Dòng log giải thích nguyên nhân gốc bị chôn dưới hàng nghìn dòng
giống hệt nhau, còn bão connection thì góp phần giữ cho database không hồi lại được.

**Sửa:** `Ivr.Infrastructure.Resilience.LoopBackoff` — chờ thêm sau mỗi lần hỏng, nhân đôi theo
streak, trần 30 giây, có jitter.

| Quyết định | Lý do |
| --- | --- |
| Lần hỏng đầu chờ 1 poll interval, không phải 0 | Retry tức thì chính là hành vi đang gỡ bỏ, và nó đắt nhất lúc bắt đầu — khi cả loạt replica cùng phát hiện một outage |
| Trần 30 giây | Backoff vô hạn thì hàng giờ sau khi dependency hồi, loop vẫn đang "retry". Đó là một kiểu hỏng khác |
| Không có ngưỡng dừng hẳn | Loop dừng thì cần người phát hiện và khởi động lại — tệ hơn hẳn một loop chạy chậm |
| Jitter giữ lại **một nửa** | Full jitter có thể trả về gần 0, đúng thứ backoff sinh ra để tránh. Giữ sàn nghĩa là delay luôn ≥ một nửa giá trị tính được |
| `Task<bool>`, không ném khi cancel | Caller `break` theo giá trị trả về, giống hệt `PeriodicTimer.WaitForNextTickAsync`. Ném từ trong `catch` trên đường shutdown không phải lỗi và không được log như lỗi |

Đặt ở `Ivr.Infrastructure` chứ không phải `Ivr.Worker` vì `ArchitectureDependencyTests.cs:14-18`
cho phép Worker phụ thuộc Infrastructure, và `Ivr.UnitTests` chỉ tham chiếu `Ivr.Domain` +
`Ivr.Infrastructure` — đặt trong Worker thì không unit test được.

Kèm theo, **phụ phẩm của cùng thay đổi:** cả bốn loop giờ truyền `TimeProvider` vào `PeriodicTimer`.
Trước đó chỉ `AnalyticsEtlJobHost` làm thế; ba loop kia dùng đồng hồ thật nên không test được nhịp
(P4 trong bản rà soát).

Ba dòng `LogFailure` giờ mang `consecutive failures={N}`. Một sự cố thoáng qua và một sự cố kéo dài
hai giờ trước đây in ra cùng một dòng chữ.

`AnalyticsEtlJobHost` cũng nhận backoff dù interval của nó là 300 giây. Không phải vì mấy giây thêm
có ý nghĩa, mà để cả bốn loop trả lời câu "một pass hỏng thì làm gì tiếp" giống nhau — không ai
phải đi tra xem loop nào là ngoại lệ.

## 3. B6 — hai store idempotency serialize khác nhau

`PostgresIdempotencyStore` đăng ký `ReadOnlySetJsonConverterFactory`. `InMemoryIdempotencyStore`
dựng `JsonSerializerOptions` riêng và không đăng ký gì.

Hệ quả: response mang `IReadOnlySet<T>` replay được ở production và **hỏng ở MOCK** — tức là đúng
cái mode dùng để diễn tập trước khi ship. Divergence nằm ngay trong lớp bảo đảm tính đúng đắn của
idempotency, và lệch về phía làm cho việc diễn tập trở nên vô nghĩa.

**Sửa:** `IdempotencySerialization.Options` — **một instance** dùng chung, không phải một factory
method gọi hai lần. Chia sẻ đối tượng mới là thứ khiến hai store không thể lệch lại; một converter
thêm vào đó tự động tới cả hai.

Tôi có thêm `options.MakeReadOnly()` cho chặt, và nó **làm vỡ** `UT-FND-IDEMP-01`: overload không
tham số đòi `TypeInfoResolver` phải được set trước. Đã bỏ. Việc chia sẻ instance mới là fix; phần
kia là làm đẹp thừa, và nó gây hại.

## 4. Test

| TestId | Khẳng định |
| --- | --- |
| `UT-BACKOFF-SCHEDULE-01` | Delay nhân đôi theo streak; 0 lần hỏng thì không chờ |
| `UT-BACKOFF-CEILING-02` | Dừng ở trần; `int.MaxValue` không tràn, không ra delay âm |
| `UT-BACKOFF-JITTER-03` | Sàn nửa delay được giữ ở **cả hai đầu** dải ngẫu nhiên |
| `UT-BACKOFF-RESET-04` | Một pass thành công xoá streak; lần hỏng kế tiếp về lại delay ngắn nhất |
| `UT-BACKOFF-SHUTDOWN-05` | Cancel giữa chừng trả `false`, không ném |
| `UT-FND-IDEMP-04` | Response chứa `IReadOnlySet<T>` replay được qua store in-memory |

`UT-FND-IDEMP-04` đã kiểm chứng **fail trên code chưa sửa** (hoàn nguyên tạm thời
`InMemoryIdempotencyStore`, chạy lại, thấy đỏ) rồi mới ghi nhận pass.

Jitter được tách thành `ApplyJitter(delay, sample)` nhận sẵn mẫu ngẫu nhiên thay vì tự rút. Lý do:
`MutableTimeProvider` của repo chỉ override `GetUtcNow()` chứ không override `CreateTimer`, và
package `Microsoft.Extensions.TimeProvider.Testing` chưa được tham chiếu — nên `Task.Delay` không
điều khiển được trong test. Thay vì thêm dependency và sinh lại `packages.lock.json` (`--locked-mode`
trong Dockerfile), tôi làm cho phần logic có rủi ro trở thành hàm thuần. Sàn jitter giờ là một
**tính chất được khẳng định**, không phải một xác suất được lấy mẫu.

Phần `Task.Delay` còn lại không có test và không đáng có: nó là hai dòng nối dây.

## 5. Kết quả

```
Unit         657/657   (+8 so với W-0255)
Integration  274/274   (Testcontainers PostgreSQL 16)
Contract      24/24
Chaos          8/8
```

`dotnet build Ivr.sln` — 0 warning, 0 error, với `TreatWarningsAsErrors=true`.

`docs/traceability-tests.md` sinh lại; `FailGateTests` bắt buộc với mọi `TestId` mới.

Impact analysis — **chạy sau khi sửa, không phải trước**, khác với W-0255 và trái quy trình trong
`CLAUDE.md`. Ghi ra đây thay vì bỏ qua:

| Symbol | Risk | Direct |
| --- | --- | --- |
| `EligibilityService.GetCapacityFailClosedAsync` | LOW | 1 |
| `InMemoryIdempotencyStore` | LOW | 0 (lower-bound: bind qua `IIdempotencyStore` nên DI không truy được) |

Cả hai LOW, không có gì để cảnh báo. Bốn `ExecuteAsync` của worker host là entry point của
`BackgroundService` nên không có upstream caller.

## 6. Còn lại

Bản rà soát ghi 26 phát hiện; W-0255 đóng 2, lượt này đóng 3 (kèm P4 là phụ phẩm). Ưu tiên tiếp
theo:

- **P1** — `AdminReadService.GetDashboardAsync`: 30 round trip tuần tự, `jobIds` nhúng lại làm
  subquery trong 5 query → 5 semi-join full-table khi không filter. Đây là mục HIGH duy nhất còn lại.
- **B7** — `RotatingCredentialProvider.ActiveGenerations` trả thẳng `byte[] SecretBytes`.
- **B8** — 9/10 bản `isConfined` không realpath `REPOSITORY_ROOT`; gate vỡ dưới symlink.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN

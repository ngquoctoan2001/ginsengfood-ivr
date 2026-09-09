# W-0263 — Một interface 15 method thành hai, và bốn vòng lặp thành một

Ngày: 2026-09-10 · Baseline: `main@7bcdb99` · Trạng thái: **TESTS_PASS**.

S1 và S3 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

## 1. S3 — vòng lặp polling

### 1.1 Bốn, không phải năm

Báo cáo viết "5 background host copy-paste nguyên khối" và đếm `RetentionJobHost` vào đó. **Sai.**
`RetentionJobHost` chạy **một lần rồi thoát** — không timer, không liveness registration, không
vòng lặp thất bại. Nó không chia sẻ pattern nào để mà trùng lặp.

Số đúng là **bốn**: analytics, callback delivery, normalisation, scheduler.

### 1.2 Cái được chia sẻ không phải số dòng

| | Trước | Sau |
| --- | ---: | ---: |
| 4 host | 476 dòng | 337 dòng |
| Base class | — | 133 dòng |
| **Tổng** | **476** | **470** |

Gần như hoà về số dòng. Giá trị nằm ở chỗ khác: skeleton vòng lặp giờ **tồn tại một lần**, nên
những câu hỏi mà bốn bản sao có thể trả lời khác nhau chỉ còn một câu trả lời — khi nào backoff,
tick ghi trước hay sau backoff, exception nào kết thúc vòng lặp thay vì được báo cáo, và một lần
chờ bị cancel nghĩa là gì.

Chúng **đã** phân hoá hai lần trước khi file này được viết: một bản dùng `do/while` trong khi ba bản
kia dùng `while`, và chỉ một bản truyền `TimeProvider` vào `PeriodicTimer` — khiến ba bản còn lại
không test được với đồng hồ giả cho tới khi `W-0256` sửa từng bản một.

Comment *"Registered even though it will not run…"* từng xuất hiện nguyên văn ở ba file. Giờ một.

### 1.3 Hai hook, và lý do chúng là hook

`OnFailure(exception, consecutiveFailures)` là hook chứ không phải lời gọi log dùng chung, vì mỗi
host sở hữu `[LoggerMessage]` riêng với EventId riêng. Một EventId dùng chung cho bốn loop sẽ làm
chúng không phân biệt được trong đúng sự cố mà việc phân biệt mới quan trọng.

`StopWhenDisabled` tồn tại cho **một** trường hợp: scheduler. Enable gate của nó nằm **bên trong**
`SchedulerRuntime.RunOnceAsync`, nên khi scheduler tắt thì pass vẫn chạy và chỉ không đặt cuộc gọi —
còn phần recovery và closing deadline trong cùng pass đó phải tiếp tục. Trả về sớm ở base sẽ dừng cả
những việc đó. Liveness vẫn báo DISABLED, vì báo một loop khoẻ mạnh mà không dispatch được chính là
kiểu an tâm giả mà registration tồn tại để loại bỏ.

### 1.4 Test, và một test tôi đã viết sai

Bốn test mới (`UT-WORKER-LOOP-01..04`) là **lần đầu** skeleton này được unit test — bốn bản sao
trước đó không có bản nào.

Test đầu của tôi **flaky**, và tôi sửa test chứ không sửa code. Nó đọc counter sau `StartAsync` +
`StopAsync` mà không join vào `ExecuteTask`. Nó pass khi tôi thêm một `Assert.Fail` in chẩn đoán (đủ
chậm để thắng race) rồi fail lại khi tôi gỡ ra — đúng dấu hiệu của một race, không phải của logic
sai. Chẩn đoán khẳng định trạng thái đúng (`enabled=False disabled=1 passes=0`), nên lỗi ở phép đo.
Giờ nó `await host.ExecuteTask` một cách tường minh, và chạy ba lần liên tiếp đều xanh.

Để test được, `Ivr.UnitTests` tham chiếu `Ivr.Worker` và `Ivr.Worker` mở internals cho nó. Các host
**giữ `internal`** — không gì ngoài assembly đó dựng chúng, và một base class `public` là lời mời
làm điều đó. Đã sinh lại `packages.lock.json` và kiểm chứng `dotnet restore Ivr.sln --locked-mode`
vẫn xanh, vì đó là thứ CI chạy.

`ArchitectureDependencyTests` chỉ ràng buộc `src/**/*.csproj`, nên tham chiếu từ `tests/` không phạm
luật tầng.

## 2. S1 — God class

### 2.1 15 method, không phải 16

Một sai số nữa trong báo cáo: interface có **15** method.

### 2.2 Đường cắt đã có sẵn trong code

Không phải tôi nghĩ ra cách chia — hai nhóm consumer đã tồn tại và dùng đúng hai tập rời nhau:

| Interface | Method | Consumer |
| --- | ---: | --- |
| `IIvrLifecycleApiService` | 6 | `InternalLifecycleEndpoints` |
| `IIvrAdminOperationsService` | 9 | `IvrAdminEndpoints`, `DevToolingApiService` (2/9) |

Khác biệt nhìn thấy được trong chữ ký: **mọi** method ở nhóm thứ hai nhận `actorId`, không method
nào ở nhóm đầu nhận. Nhóm đầu là một service khác restate một sự thật một cách idempotent; nhóm sau
đổi thứ hệ thống đang làm với khách hàng đang trong cuộc gọi.

`DevToolingApiService` dùng đúng **2 trong 9** — nó từng phụ thuộc vào cả 15.

### 2.3 Class giữ nguyên một khối, và đó là cố ý

15 method chia nhau wrapper idempotency, context factory và các helper validation. Tách class sẽ
**nhân đôi** chúng hoặc phải dựng một type thứ ba để giữ — churn đổi lấy thứ mà việc tách interface
đã đem lại. Một implementation, hai interface, DI bind cả hai về cùng instance.

File giảm từ **1.266 → 1.188** dòng, và 105 dòng giảm đi là **composition root** chuyển sang
`InternalAdminApiServiceCollectionExtensions.cs`. Nó đang đăng ký DI cho **sáu service khác**
(`IAdminReadService`, `IAdminConfigReadService`, `IAnalyticsReadService`,
`IScriptLifecycleApiService`, `SeedCatalog`, `IDevToolingApiService`) từ trong file của
`InternalAdminApiService` — nên muốn tìm nơi `AnalyticsReadService` được đăng ký thì phải đoán ra nó
nằm trong file của một service không liên quan.

1.188 dòng vẫn lớn. Cái đã sửa là **khớp nối**, không phải kích thước.

## 3. Kết quả

```
Unit         667/667   (+4)
Integration  278/278
Contract      24/24
Chaos          8/8
             ─────────
             977/977
```

`GATE_SWEEP_PASS 39/39` · `dotnet build Ivr.sln` — 0 warning, 0 error ·
`dotnet restore Ivr.sln --locked-mode` — xanh.

Impact analysis trước khi sửa: `IInternalAdminApiService` — **LOW**, consumer là đúng 3 file.

## 4. Còn lại

**17/26 đã đóng, 1 rút lại vì sai.** Không còn HIGH.

Cần owner:

- **P2** — 109 index. Cần `pg_stat_user_indexes.idx_scan` từ staging hoặc production.
- **Retention chưa arm** — `PeriodDays` rỗng.
- **`"MOCK"` có hai chủ sở hữu** — tách tên xong thì 22 site thay được an toàn.

Thuần kỹ thuật: **S5** (schema wire định nghĩa hai lần — `TaskIntakeEndpoint` giữ 6 `HashSet` tên
field viết tay song song với contract sinh tự động), phần S4 còn lại, **B9**, **B10**, **C1**,
**C3**, **C4**, **C5**.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN

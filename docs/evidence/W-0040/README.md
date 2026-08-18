# W-0040 — Evidence: Logging, metrics & tracing (`P6-1`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS` cho instrumentation + readiness; OTLP export `BLOCKED_EXTERNAL` — xem §5

## 1. Đóng cái `/health/ready` đã khai nợ từ P0-1

`/health/ready` trả **`Healthy` cứng** từ P0-1, kèm nhãn `dependencyChecks = NOT_IMPLEMENTED_UNTIL_W-0040`. Nhãn đó trung thực, và `P4-1` cố ý không đụng vào để không cướp scope của slice này.

Giờ nó kiểm thật: kết nối database, và trạng thái circuit của lối gửi callback khi lối đó được bật. Không sẵn sàng → **503**.

Một probe trả 200 trong khi database đã mất **không chỉ là không giúp được gì** — nó giữ load balancer tiếp tục đẩy traffic vào đúng chỗ đang hỏng.

Ba probe được tách đúng vai:

| Probe | Trả lời câu gì | Phụ thuộc |
| --- | --- | --- |
| `live` | tiến trình còn là tiến trình không | **không** — nếu không, một sự cố downstream sẽ restart mọi pod |
| `ready` | traffic có được vào không | database + lối callback |
| `startup` | boot xong chưa | options validation đã từ chối boot khi cấu hình không an toàn |

Mọi `reason` là **cụm từ cố định**, không phải message của exception: thân readiness được phục vụ cho bất kỳ ai hỏi, nên nó không được mang connection string, tên host hay stack.

## 2. Tag telemetry là allowlist, không phải denylist

Observability là hệ thống con duy nhất mà **công việc của nó là sao chép dữ liệu production sang chỗ khác**. Nên `IvrTelemetry` không tin vào thiện chí của call site:

- **Allowlist tên tag.** Một denylist chỉ chặn được những rò rỉ ai đó đã nghĩ ra. Allowlist buộc người thêm tag mới phải mở file quy tắc ra và nhìn nó.
- **Mọi giá trị chuỗi qua `PiiGuard`.** Một số điện thoại chạm vào log là cùng một rò rỉ dù nó tới qua audit row hay metric label.
- **Thông báo lỗi không lặp lại giá trị vi phạm.** Exception message rồi cũng vào log, và báo cáo một rò rỉ bằng cách trích dẫn nó thì vẫn là rò rỉ.
- **`correlation_id` được phép trên span, bị cấm trên metric.** Trên span nó là mục đích — một request kỹ sư đang điều tra. Làm dimension của metric thì mỗi request thành một time series riêng: đó không phải vấn đề privacy, đó là cách hạ backend metric. Hai tập tách riêng để khác biệt này được **ép** chứ không phải được nhớ.

## 3. Metrics đo từ nguồn thật

Sáu instrument nghiệp vụ theo §4: intake decision, attempt (kèm `is_counted_customer_attempt` cho `DT-02`), result, callback (kèm ACK + HTTP), fail-closed theo reason code, và hai histogram độ trễ.

Độ trễ callback đo bằng **thời gian trôi thật** của lần gửi đó (`Stopwatch.GetElapsedTime`), không ước lượng — `P6-1` §11 cấm báo cáo một KPI được suy đoán thay vì quan sát.

`UT-OBS-METRIC-03` khẳng định cả sáu phát ra **kèm dimension**: một counter không có dimension trả lời được "bao nhiêu" và không bao giờ trả lời được "loại nào", mà đó mới là câu ops hỏi.

## 4. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **366/366** (22 contract + 209 unit + 135 integration), +5 |
| `UT-OBS-PII-01` / `-01B` | tag ngoài allowlist, tag chứa PII, và `correlation_id` làm metric dimension đều bị từ chối |
| `UT-OBS-METRIC-03` | 6 instrument phát đúng kèm dimension; histogram mang số đo thật |
| `UT-OBS-TRACE-02` | một span mỗi lần gửi, mang correlation của task + outcome; mọi tag qua allowlist và `PiiGuard` |
| `IT-OBS-HEALTH-04` | database sống → 200; database mất → **503**, reason là cụm từ cố định, không lộ host/credential/stack |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |
| `test:traceability` | `TEST_TRACEABILITY_CURRENT=244` |

**Một test cũ đổi assertion, và đó là điểm chính.** `IT-BOOT-02` khẳng định cả ba probe trả 200 vô điều kiện. Cái 200 đó đang khẳng định **placeholder**, không phải một tính chất của hệ thống — host bootstrap trong test không có database nào phía sau. Giờ nó khẳng định `live`/`startup` 200 và `ready` **503**, kèm kiểm thân trả về không lộ credential.

## 5. Cái này KHÔNG chứng minh

- **Không có OTLP export.** Collector/backend là `W-0063`, vẫn `BLOCKED_EXTERNAL`. Instrumentation dùng `ActivitySource` và `Meter` của BCL, nên gắn exporter sau này **không phải sửa call site nào** — nhưng chưa có tín hiệu nào rời khỏi tiến trình. `P6-1` §5 tự đánh dấu backend là `NEED_CONFIRMATION`.
- **Chưa instrument đủ 5 chặng.** §6.2 liệt kê intake/scheduler/dispatch/normalize/callback. Slice này gắn span ở **callback** — biên rõ ràng nhất và là nơi correlation phải sống sót ra ngoài. Bốn chặng còn lại chưa có span; ghi ở đây thay vì để người đọc suy ra từ một dòng "đã xong".
- **`dependency_probing_available` vẫn `false`.** Đó là cờ cho các card `ORDER_CORE`/`OPS_SELLABLE_GATE`/`CRM_DO_NOT_CALL`/`EVIDENCE_REGISTRY` — **thăm dò dependency ngoài**, khác với readiness của chính tiến trình. Không cái nào trong bốn được thăm dò, nên bật cờ lên sẽ là một tuyên bố sai.
- **Không có Serilog / redaction pipeline cho log.** §6.1 nhắc; `PiiGuard` đã bảo vệ audit row, evidence và giờ cả telemetry tag, nhưng chưa có sink log có cấu trúc để gắn pipeline vào — nó đi cùng OTLP ở `W-0063`.
- **Sampling chưa cấu hình** (§6.6): không có exporter thì không có gì để lấy mẫu.

## 6. Test integration chập chờn — đã khép

Ở cuối slice này tôi ghi lại một quan sát chưa khép: **2/5 lượt chạy toàn suite có đúng một test
integration đỏ**, lần đầu bắt được tên (`IT-BOOT-02`), lần sau không. Nghi ngờ khi đó là "tranh chấp
Testcontainers khi nhiều collection chạy song song" — và **nghi ngờ đó sai**. Phần dưới là cái đã đo
được, không phải cái đã đoán.

### 6.1 Bắt được lỗi

Chạy suite integration 3 lượt có ghi `trx`: lượt 1 và 3 xanh 135/135, **lượt 2 đỏ đúng một test**.

| | |
| --- | --- |
| Test | `ScriptRegistryPersistenceTests.MigrationSeedsMockApprovalAndKeepsOtherModesClosed` |
| Ngoài | `InvalidOperationException: An exception has been raised that is likely due to a transient failure` |
| Trong | `NpgsqlException: Exception while reading from stream` → `EndOfStreamException` |
| Điểm hỏng | `NpgsqlConnector.SetupEncryption` ← `PoolingDataSource.OpenNewConnector` ← `EnsureDeletedAsync` |

Stack trong mới là chỗ đọc được: hỏng **không phải** ở một câu truy vấn đang chạy dở bị giết, mà ở
`OpenNewConnector` — một **kết nối vật lý mới toanh** chết ngay nhịp đọc đầu tiên của handshake, khi
Npgsql vừa gửi SSLRequest và đang chờ đúng **một byte** trả lời. Server đóng socket thay vì trả lời.

### 6.2 Nguyên nhân gốc

Image `postgres` chính thức khởi động **hai** postmaster. Log của một container sạch:

| | pid | nghe ở đâu | ready lúc |
| --- | --- | --- | --- |
| server initdb tạm | 2770 | **chỉ Unix socket** — không có dòng `listening on IPv4` | `08:29:23.507` |
| server thật | 1 | `listening on IPv4 address "0.0.0.0", port 5432` + Unix socket | `08:29:24.088` |

Giữa hai mốc đó là **~580 ms** trong đó `pg_isready` trần trả lời "sẵn sàng" còn **TCP chưa ai nghe**.
Đo trực tiếp trong container, hai probe chạy cùng vòng lặp:

```
UNIX_SOCKET_READY at tick 1368     <- pg_isready       (Unix socket)
TCP_READY         at tick 1480     <- pg_isready --host localhost
```

Fixture ghi đè wait strategy của module bằng đúng probe sai:

```csharp
.WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
```

`pg_isready` **không có `--host`** đi qua Unix socket, nên nó được **server tạm** trả lời. Entrypoint
chính thức chạy server tạm với `listen_addresses=''` — Unix socket, không TCP. `StartAsync()` vì thế
trả về sớm, fixture công bố connection string, test bắt đầu kết nối qua TCP; port-forwarder của
Testcontainers **nhận** kết nối (nó bind ở host, không phải Postgres bind), rồi đóng socket khi không
chuyển tiếp được vào container. Client nhìn thấy: connect thành công → gửi SSLRequest → **EOF**.

Điều đó khớp từng chi tiết đã quan sát, gồm cả chi tiết trước đây tôi tưởng là nhiễu:

- kết nối **mới** chết ở handshake, không phải truy vấn đang chạy bị giết;
- **EOF** chứ không phải connection refused — vì forwarder đã nhận;
- chập chờn: chỉ đỏ khi lần chạm database đầu tiên rơi vào cửa sổ ~580 ms;
- **tên test đổi giữa các lượt** — đây mới là điểm chốt: cái đỏ là *test nào chạm database trước*,
  nên nó không bao giờ là một bất biến sai của một test cụ thể;
- chỉ `PostgresPersistenceTestGroup` dính. `RetentionJobFixture` dùng **cùng image, cùng cách dùng**
  nhưng **không ghi đè** wait strategy, và chưa đỏ lần nào — đó là nhóm đối chứng có sẵn trong repo.

### 6.3 Sửa

Gỡ dòng ghi đè, trả về wait strategy mặc định của `Testcontainers.PostgreSql` 4.13.0, vốn probe
`pg_isready --host localhost --dbname … --username …` **qua TCP** và do đó chỉ qua khi server thật đã
lên. Không retry, không skip, không nới `EnableRetryOnFailure` — cả ba đều chỉ giấu một lỗi khởi
động thật và sẽ giấu luôn lần sau.

Chỗ gỡ để lại comment nói vì sao, vì một dòng bị **xoá** thì vô hình, và dòng này trông rất hợp lý
với người đọc tiếp theo.

### 6.4 Chốt chặn hồi quy — và giới hạn thật của nó

`IT-DB-BOOT-08` chụp log container **đúng nhịp `StartAsync()` trả về** và đòi trong đó đã có
`listening on IPv4 address`.

Đã kiểm âm: dựng lại wait strategy cũ rồi chạy chốt chặn 6 lượt — **đỏ 1/6**. Nói thẳng con số đó:
**đây là bộ dò xác suất, không phải cổng cứng.** Khi wait strategy sai vẫn tình cờ trả về muộn hơn
server thật, trong lượt đó lỗi **thật sự không xảy ra** và không assertion nào phát hiện được —
giới hạn nằm ở bản chất race, không ở cách viết test. Cái nó đổi được là: thay vì một test ngẫu
nhiên đỏ với `EndOfStreamException` **đổ tội nhầm chỗ**, thì chính chốt chặn này đỏ và gọi đúng tên.

Với bản sửa, chốt chặn xanh **tất định**: probe TCP trong container chỉ qua được nhờ server thật, nên
lúc `StartAsync()` trả về thì dòng log kia chắc chắn đã có.

CI cũng không che được lỗi này: `retry` ở `.gitlab-ci.yml` chỉ đặt cho `runner_system_failure`, không
cho `script_failure`.

### 6.5 Nghiệm thu, và giới hạn của nó

Chạy lại suite integration **8 lượt** sau khi gỡ: **7 xanh 136/136**, 1 đỏ — và lượt đỏ đó là một lỗi
**khác hẳn**, ở code production, không phải lỗi khởi động này; xem §7. Lỗi cũ (`EndOfStreamException`
trong `SetupEncryption`) **không tái xuất hiện lần nào** trong 8 lượt.

Nói cho đúng mức: 8 lượt xanh chỉ **chặn trên tần suất**, tự nó không phải chứng minh. Bằng chứng
chính không nằm ở số lượt xanh mà ở chỗ **cơ chế đã được đo và gỡ bỏ**: cửa sổ ~580 ms tồn tại vì
probe hỏi sai socket, và probe mặc định hỏi qua TCP thì không có cửa sổ đó để rơi vào.

## 7. Kiểm chứng bản sửa làm lộ một lỗi **khác** — và lỗi này nằm ở code production

Chạy lại suite integration 8 lượt để nghiệm thu §6: **7 xanh, 1 đỏ**. Nhưng lượt đỏ đó **không phải
lỗi cũ** — chữ ký hoàn toàn khác, và nó không nằm ở hạ tầng test.

```
RegexMatchTimeoutException
  at PiiGuard.IsSafeText                     src/Ivr.Domain/Privacy/PiiGuard.cs:29
  at CorrelationMiddleware.IsValid           src/Ivr.Api/Middleware/CorrelationMiddleware.cs:34
  at CorrelationMiddleware.InvokeAsync       -- tức là trên lối đi của **mọi** request
```

### 7.1 Không phải ReDoS — đo rồi mới nói

Thông báo của .NET gợi ý "excessive backtracking caused by nested quantifiers", nên phải đo trước
khi kết luận. Chạy chính pattern đó qua engine .NET với input sạch (buộc quét toàn bộ, không
short-circuit):

| input | interpreted (đang dùng) | `Compiled` |
| --- | --- | --- |
| 12 KB | 2,4 ms | 0,8 ms |
| 123 KB | 22,3 ms | 8,9 ms |
| 492 KB | **92,0 ms** | 42,0 ms |

Tuyến tính — khoảng `0,19 ms/KB` interpreted. **Không có bùng nổ backtracking**; pattern không có
quantifier lồng nhau. Vậy vấn đề không phải hình dạng pattern mà là **ngân sách 100 ms cố định**.

Hai lối phơi nhiễm, khác nhau:

1. **Timeout của .NET tính theo wall clock, không phải CPU.** Thread bị hoãn lịch thì đồng hồ vẫn
   chạy. Đó là cái đã bắn trong lượt đỏ: input là header correlation **≤ 128 ký tự** — không đời nào
   tốn 100 ms CPU — nhưng máy khi đó đang chạy 8 lượt test cùng nhiều container Postgres.
2. **Input không bị chặn trên.** `PiiMaskingFilter` serialize **toàn bộ response body** rồi đưa cho
   guard (`PiiMaskingFilter.cs:32`). Một response admin dạng danh sách vài trăm KB đã sát ngân sách
   ngay cả khi máy rảnh: 492 KB mất 92 ms trên ngưỡng 100 ms.

### 7.2 Blast radius và vì sao vẫn sửa

`impact({target: "IsSafeText", direction: "upstream"})` trả **`CRITICAL`**: 43 symbol, 18 execution
flow, 8 module. Đã cảnh báo trước khi sửa, theo `CLAUDE.md`.

Vẫn sửa, vì bản sửa **không đụng vào pattern**: tập giá trị bị coi là PII **giống hệt từng byte**.
Không nới whitelist, không thu hẹp phát hiện — nên nó không phải thay đổi chính sách privacy và
không cần chữ ký chủ sở hữu. Ba thay đổi, cả ba đều theo hướng **siết**:

| | trước | sau | vì sao |
| --- | --- | --- | --- |
| ngân sách | `100 ms` | `2 s` | quét đã đo là tuyến tính, nên ngân sách rộng không mở cửa ReDoS; nó chỉ thôi bắn vì nhiễu lịch CPU |
| engine | interpreted | `Compiled` | rẻ đi một nửa (`0,085 ms/KB`) trên lối đi của mọi request |
| khi timeout | ném ra ngoài, không ai bắt | **`return false`** | DO-06 |

Điểm thứ ba mới là điểm chính. Trước đây timeout thoát ra ngoài dưới dạng exception không ai bắt —
`PiiMaskingFilter` chỉ bắt `InvalidOperationException`, còn `RegexMatchTimeoutException` kế thừa
`TimeoutException`, nên nó **đi xuyên qua** thành 500. Đã kiểm: **không caller nào coi timeout là
"an toàn"**, tức chưa từng có rò rỉ. Nhưng "guard chưa kịp quyết định" **không phải** là "văn bản
sạch", và đọc nó thành sạch sẽ thả giá trị qua đúng lúc máy tải nặng nhất. Giờ nó fail closed:
masking filter ra policy violation đúng lối, correlation middleware cấp id mới thay vì tin id vào.

### 7.3 Chốt chặn — lần này tất định cả hai chiều

`UT-FND-PII-12` quét 1 MB body sạch và đòi kết quả "an toàn", rồi quét lại chính body đó có gắn một
giá trị bị cấm ở **cuối** và đòi bị bắt.

Kích thước 1 MB chọn có chủ đích: với cấu hình cũ nó tốn ~190 ms, **vượt ngân sách 100 ms**, nên test
đỏ; với cấu hình mới ~87 ms trên ngân sách 2 s. Kiểm âm bằng cách dựng lại đúng cấu hình cũ:
**đỏ 3/3**, không phải xác suất như §6.4. Và vì lần kiểm âm đó vẫn giữ nhánh `catch`, cái đỏ là một
assertion sạch chứ không phải exception văng ra — tức nó chứng minh luôn nhánh fail-closed có sống.

Nửa sau của test giữ cho bản sửa trung thực: nó chứng minh **ngân sách rộng ra nhưng tập phát hiện
không dịch**.

### 7.4 Cái này KHÔNG chứng minh, và một quan sát gửi chủ sở hữu

- **Timeout không được đếm ở đâu cả.** Nơi tự nhiên là `ivr_fail_closed_total` của §3, nhưng
  `IvrTelemetry` nằm ở Infrastructure còn `PiiGuard` ở Domain, và `ArchitectureDependencyTests` cấm
  chiều phụ thuộc đó. Ghi ra đây thay vì bịa một chỗ đếm.
- **Pattern có dương tính giả, và tôi cố ý không sửa.** Trong lúc đo tôi phát hiện body sạch vẫn bị
  bắt: một chuỗi **mười chữ số bắt đầu bằng số không** khớp nhánh số điện thoại, mà id dạng đệm số
  không (`TASK-` + số thứ tự đệm) sinh ra đúng hình đó. Hệ quả thật: response admin chứa id kiểu đó
  bị `PiiPolicyViolation`. **Không sửa** — thu hẹp phát hiện là thay đổi chính sách privacy, và
  `P0` cấm tôi tự phê duyệt. Cần chủ sở hữu quyết, nên nêu ở đây.
- Ngân sách `2 s` là **số chọn theo phép đo**, không phải theo chuẩn nào. Nó rộng gấp ~23 lần chi phí
  quét 1 MB đã đo. Nếu sau này có body lớn hơn nhiều, con số này phải đo lại chứ không nhân lên.

### 7.5 Nghiệm thu cuối, và một cái tôi **không** định danh được

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` × 4 lượt liên tiếp | **368/368** mỗi lượt (22 contract + 210 unit + 136 integration), 0 đỏ |
| `dotnet test` integration × 8 lượt (trước khi sửa `PiiGuard`) | 7 xanh, 1 đỏ — chính là §7 |
| `UT-FND-PII-12` kiểm âm trên cấu hình cũ | **đỏ 3/3** |
| `IT-DB-BOOT-08` kiểm âm trên wait strategy cũ | đỏ **1/6** — xác suất, đã nói rõ ở §6.4 |
| `scan-pii.sh` | `PII_SCAN_PASS` |
| `test:traceability` | `TEST_TRACEABILITY_CURRENT=246` |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |

**Một lượt chạy toàn solution ở giữa quá trình báo 2 unit test đỏ; tôi chỉ định danh được 1**
(`UT-TRACE-01`, do bảng traceability chưa sinh lại sau khi thêm test mới — đã sửa). Cái thứ hai
**không bắt được tên** và **không tái hiện** trong 4 lượt toàn solution sau đó. Ghi ra đây thay vì
làm tròn thành "tất cả đã xanh": đúng cái thói quen làm mất dấu lỗi §6 suốt một slice.

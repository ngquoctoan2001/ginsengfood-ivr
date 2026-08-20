# W-0042 — Evidence: Chaos & resilience game-days (`P6-3`)

Ngày: `2026-08-18` · Trạng thái: `TESTS_PASS` cho 5 scenario; **không có staging** — xem §5

Báo cáo game-day đầy đủ: [`docs/gameday-report.md`](../../gameday-report.md). File này ghi phần
kỹ thuật và những gì slice phát hiện.

## 1. Discovery tìm ra một lỗi thật trong `W-0041` trước khi viết scenario nào

Trước khi dựng scenario, tôi đi tìm xem DT-04 auto-disable có hiện thực không. Có — nhưng ở
`PostgresTelephonyDispatchStore:245-250`:

```csharp
channel.FailCount = channelHealthy ? 0 : channel.FailCount + 1;
channel.Status = channelHealthy ? "IDLE"
    : channel.FailCount >= 3 ? "HEALTH_FAILED" : "QUARANTINED";
```

`W-0041` chỉ đếm `ivr_channel_quarantines_total` ở **`PostgresSchedulerStore`** (lease hết hạn). Tức
là alert `IvrChannelAutoDisableBurst` mang nhãn DT-04 lại đọc một counter mà **chuyển trạng thái
DT-04 không bao giờ chạm vào**.

Đó là kiểu hỏng tệ hơn không có alert: nó **trông như đã phủ**. Sửa trong slice này — đếm ở **cả
hai** nơi kênh bị đưa ra khỏi phục vụ — và `CHAOS-SIM-03` giữ cho nó không tái diễn.

`docs/slo.md` §4 cũng phải sửa: nó gộp luật per-kênh (`fail_count>=3`, chạy **trong code**, đồng bộ,
để kênh hỏng không được cấp phát tiếp) với alert toàn đội (nửa "báo cho người") thành một thứ.

## 2. Hai cơ chế chèn lỗi, và mức chứng minh khác nhau

| Cơ chế | Scenario | Mức |
| --- | --- | --- |
| **Toxiproxy** | `CHAOS-DB-02`, `CHAOS-RECOVERY-04` | **lỗi mạng thật** — socket bị cắt bởi thứ nằm giữa tiến trình và Postgres |
| **Chèn ở tầng mã** | `CHAOS-DOWNSTREAM-01`, `CHAOS-SIM-03` | biên ngoài chưa có endpoint thật để cắt; `P6-3` §5 cho phép |

Ghi tách ra vì hai cách chứng minh hai mức khác nhau. Dừng container cũng cắt được kết nối, nhưng
nó kiểm một thứ khác: một server **ra đi sạch sẽ**, không phải một liên kết **ngừng tải traffic**.

## 3. Giới hạn blast radius là thứ được ép, không phải được hứa

Mọi container do chính lượt chạy tạo ra trên một network dùng-một-lần, xoá cùng lượt chạy, và
không có tuyến nào ra ngoài. `CHAOS-GUARD-05` đọc `deploy/chaos/toxiproxy.staging.json` và đỏ nếu
một upstream trỏ tới hostname không phải alias container hoặc loopback.

Kiểm âm: đổi upstream thành một hostname giải được → **đỏ**; khôi phục → xanh.

## 4. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test tests/chaos/` | **6/6**, chạy lặp 3 lượt đều xanh |
| `CHAOS-DB-02` | `/health/ready` **503** khi cắt link; ghi **ném lỗi** chứ không nuốt; dòng trước sự cố còn nguyên; dòng ghi hỏng không để lại rác; **recovery 8 ms** |
| `CHAOS-DOWNSTREAM-01` | `RETRY_PENDING`, `AcknowledgedAt` rỗng, `NextRetryAt` đặt; chạy lại ngay gửi **0** lần; breaker mở sau chuỗi hỏng |
| `CHAOS-SIM-03` | **cả 5** disposition lỗi thiết bị → `IVR_TECHNICAL_EXCEPTION`, không phải no-answer; `is_counted_customer_attempt=false`; lần hỏng thứ 3 → `HEALTH_FAILED` |
| `CHAOS-RECOVERY-04` | 0 lần gửi khi store mất; sau khôi phục đúng **1** lần; chạy lại **0**; một dòng duy nhất mang idempotency key |
| `CHAOS-DUPLICATE-06` | partition **một phần** (cắt link DB **thật**): lệnh ghi **ném lỗi** chứ không trả `false`; lease lapse → dòng **được nhặt lại**; bản trùng **giống từng byte** (`callback_id`, idempotency key, payload, hash); worker lease cũ **bị chặn** trong lúc worker kia còn giữ dòng; **một** dòng outbox duy nhất; sau khi acknowledge thì không còn dequeue được nữa |
| `CHAOS-GUARD-05` | kiểm âm đỏ đúng lý do |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` (đã mở rộng cho fragment chaos, kiểm âm đỏ) |
| `test:traceability` | `TEST_TRACEABILITY_CURRENT=257` (+5); **`331`** sau `CHAOS-DUPLICATE-06` (`2026-08-20`) |
| `scan-pii.sh` | `PII_SCAN_PASS` |

**Bộ sinh traceability phải sửa mới thấy project chaos.** Danh sách project trong
`generate-test-traceability.mjs` là cứng và project mới nằm ở `tests/chaos` chứ không phải
`tests/Ivr.ChaosTests`. Không sửa thì bảng traceability báo phủ đủ trong khi 5 scenario nằm ngoài
nó — đúng kiểu im lặng mà `P5-1` dựng bảng này để chặn.

## 6. Bốn dòng `ARCH-05` — và một phép kiểm **tự nói dối về mình**

`2026-08-19`. Các dòng "before attempt" đều hứa cùng một điều: nguồn không trả lời được ⇒
**không dispatch**.

Mọi test sẵn có chứng minh **nửa đầu**: quyết định là hold, và **chưa có dòng attempt nào**. Không
test nào chứng minh nửa sau, vì **không test nào từng chạy scheduler sau đó**.

Khác biệt không vụn: *"chưa có attempt"* là phát biểu **về quá khứ**; *"sẽ không có cuộc gọi nào"*
là khẳng định **về một component khác**, có claim query riêng và predicate riêng.

### Đặt ở `Ivr.IntegrationTests`, không phải `tests/chaos`

Bản đầu viết trong project chaos rồi bỏ, vì hai lý do đo được:

- collection chaos **không có `ResetAsync`** — các scenario dùng suffix ngẫu nhiên để khỏi giẫm chân
  nhau;
- `TryClaimDueDispatchAsync` **chọn theo deadline trên toàn bảng**, nên trong một fixture dùng
  chung, *"scheduler không nhận gì"* và *"scheduler nhận job của test khác"* đều **không phân biệt
  được** với điều cần đo.

Nơi đã có sẵn cả cô lập lẫn seeder là file eligibility. Khẳng định vẫn là của `ARCH-05`; chỉ chỗ ở
đổi.

### Kiểm âm **sống sót** — và đó mới là phát hiện

Kiểm âm đầu: gỡ `job.eligible IS TRUE` khỏi claim query, kỳ vọng đỏ. **Nó vẫn xanh.**

Vì có **ba** guard đứng giữa một hold và một dispatch, không phải một: `eligible`, `status`,
`queue_status`. Hold đóng cả ba, nên gỡ một cái thì hai cái kia vẫn từ chối.

Nghĩa là bình luận tôi vừa viết cho chính phép kiểm đó — *"nếu ai đó bỏ predicate này, test vẫn xanh
trong khi cuộc gọi vẫn đi ra"* — **mô tả sai** thứ nó kiểm. Một phép kiểm sống sót qua đúng
regression mà nó tự nhận bắt được thì **không phải phép kiểm mà bình luận của nó nói**.

Sửa: khẳng định **từng guard theo tên**, cạnh khẳng định hành vi. Nay đổi hold thành
`READY_FOR_SCHEDULER`/`QUEUED` → đỏ, kèm đúng tên trường.

### Đối chứng: cùng scheduler ấy **có** nhận một task khoẻ

*"Scheduler không nhận gì"* cũng chính là hình dạng của một scheduler **không thể nhận gì** — thiếu
kênh, cửa sổ đã đóng, policy chưa gieo. Đối chứng dùng **cùng seeder, cùng store**, đòi nó nhận đúng
job đó. Đó là thứ biến ba lần từ chối thành **bằng chứng**.

### Dòng Trust vẫn trống, và **cố ý**

`ARCH-05` ghi "Trust/Contact resolver → Hold task / review". Nhưng trust trả lời câu *"có được **bỏ
qua** cuộc gọi cho khách đã tin cậy không"* — và **không biết thì không bao giờ được suy ra là bỏ
qua**. Hiện thực để nó thành **advisory** và **vẫn gọi** (`skipFeatureEnabled = false`).

Fail-closed ở dòng này **ngược chiều**: hold sẽ để một đơn không được xác nhận chỉ vì một tính năng
tiện lợi đang hỏng. Nửa **contact** của dòng (`CONTACT_INVALID`) mới dẫn tới hold.

Tôi **không** viết khẳng định "trust unavailable ⇒ hold", vì làm thế là **mã hoá một sai lầm vào bộ
test**. Dòng này cần **chủ sở hữu quyết** ma trận nói gì — không phải một phép kiểm đoán hộ.

## 7. Partition **một phần** — và lời hứa mà một outbox at-least-once thật sự giữ được

`2026-08-20`, `CHAOS-DUPLICATE-06`. Đóng residual §6.1 (`partition một phần` + `webhook trùng lặp`).

Một sự cố **toàn phần** dễ: không gì tới được Sales, outbox giữ kết quả, và `CHAOS-DOWNSTREAM-01`
đã chứng minh IVR không tự bịa ra một xác nhận, cũng không đánh rơi cái nào. Partition **một phần**
mới là hình dạng nguy hiểm, vì **không có gì trông như hỏng**: worker vẫn tới được Sales — callback
đã giao và đã được hành động — nhưng worker **không tới được database**, nên nó không ghi nổi việc
mình vừa giao. Lease hết hạn trong lúc nó vẫn sống và vẫn đúng; một worker khác nhặt dòng ấy lên; và
Sales được báo **lần thứ hai**.

Lời hứa mà một outbox at-least-once **không** giữ được là *"không bao giờ hai lần"*. Cắt đứt liên
kết giữa **làm việc** và **ghi lại việc đã làm** khiến lần giao thứ hai thành **không tránh khỏi**,
và bất kỳ thiết kế nào tuyên bố ngược lại chỉ là chưa bị partition bao giờ. Hai lời hứa **giữ
được**, và đó là thứ scenario này đo:

| Lời hứa | Đo bằng |
| --- | --- |
| lần giao thứ hai **nhận ra được** là cùng một cái | cùng `callback_id`, cùng idempotency key, payload và hash **giống nhau từng byte** |
| worker về muộn **không thắng được** | `CompleteDeliveryAsync` với lease cũ trả `false` **trong lúc** worker kia còn đang giữ dòng |

Payload so bằng **byte** chứ không so bằng JSON đã parse: transport đối chiếu thân đã gửi với
`PayloadSha256`, nên một lần tuần tự hoá lại chỉ *mang cùng ý nghĩa* sẽ không sống sót — và một Sales
khử trùng lặp theo hash sẽ thấy **hai callback khác nhau** cho **một quyết định**.

Partition là **lỗi mạng thật**: link database bị cắt ở chặng Toxiproxy, nên lệnh ghi hỏng theo cách
một lệnh ghi bị partition hỏng, không phải theo cách một mock hỏng. Và nó phải **ném lỗi** chứ không
trả `false` — `false` nghĩa là database đã **từ chối**, một thế giới khác hẳn thế giới mà lệnh ghi
**không bao giờ tới nơi**, và chỉ cái sau mới là partition. Khẳng định ấy có mặt để một lần chạy
không hề partition được gì sẽ **đỏ**, thay vì đo một hệ thống khoẻ mạnh rồi báo xanh.

### Kiểm âm **sống sót lần nữa** — đúng cái bẫy hai-guard của §6

Bản đầu khẳng định *"worker về muộn không ghi được"* **sau khi** worker B đã hoàn tất. Gỡ hẳn điều
kiện lease khỏi `CompleteDeliveryAsync` (cả hai chỗ) → **vẫn xanh**. Vì `CompleteDeliveryAsync` còn
đòi `DeliveryStatus = 'SENDING'`, mà một dòng đã acknowledge thì không còn `SENDING`: thứ từ chối
worker A là **trạng thái**, không phải **lease**.

Đây là **lần thứ hai** trong dự án một khẳng định hành vi không phân biệt nổi hai guard cùng canh
một chỗ — §6 là lần đầu. Lần này sửa **không** bằng cách thêm lời, mà bằng cách **đổi thời điểm**:
khẳng định chuyển lên lúc B **vẫn đang giữ** dòng ở `SENDING`, khoảnh khắc duy nhất mà **lease** là
thứ đang từ chối. Kèm ngay sau đó một khẳng định **dương** (B vẫn hoàn tất được), vì nếu thiếu thì
*"A bị từ chối"* cũng đúng trên một repository từ chối **mọi** completion.

Hai kiểm âm, cả hai đỏ đúng chỗ:

| Regression cấy vào | Kết quả |
| --- | --- |
| Gỡ `row.LeaseToken == leaseToken` khỏi `CompleteDeliveryAsync` | đỏ: *"A worker whose lease had expired wrote the outcome of a row another worker was still holding"* |
| Gỡ nhánh `delivery_status = 'SENDING' AND lease_expires_at < now` khỏi dequeue | đỏ: *"…was never re-claimed after its lease lapsed… it is a lost callback: Sales was told once and IVR believes it was never told at all"* |

Kiểm âm thứ hai đáng nêu riêng, vì nó cho thấy **thiếu cơ chế nhặt lại còn tệ hơn trùng lặp**. Một
dòng kẹt ở `SENDING` sau một lease chết không phải rủi ro trùng lặp — nó là **callback mất**: Sales
đã được báo một lần, còn IVR tin rằng chưa báo lần nào.

### Nửa "sai thứ tự" của residual: **cố ý không viết test**

Một job chỉ có **đúng một** kết quả FINAL — claim query đòi `NOT EXISTS (final result)` và
`UpdateJob` đóng job ngay khi có final — và **chỉ** kết quả FINAL mới vào outbox. Nên không thể tồn
tại hai callback của cùng một task để mà sai thứ tự. Viết một test *"thứ tự đúng"* ở đây là viết một
test **không thể đỏ**; ghi lại lý do đáng giá hơn ghi một dấu tick.

## 5. Cái này KHÔNG chứng minh

- **Không có staging.** `P6-3` §4 nói chaos chạy ở dev/staging. Ở đây nó chạy trong harness tự dựng
  của bộ test. Điều đó làm blast radius **chặt hơn** (không có tuyến nào ra ngoài) nhưng cũng nghĩa
  là **chưa lượt nào chạy trên một hệ đã triển khai**. `deploy/chaos/` là config cho staging chưa
  tồn tại (`W-0063`).
- **Không có alert-fire capture thật** (§10) — không có Alertmanager để bắt. Cái có là hai nửa
  ghép lại: chaos chứng minh **sự cố thật làm counter thật nhúc nhích**; `IT-SLO-ALERT-01` chứng
  minh **luật nổ** trên hình dạng đó. Không lượt nào chứng minh cả hai.
- ~~**4/7 dòng trong ma trận `ARCH-05` §1 chưa có scenario**.~~ **Đã đóng `2026-08-19`** bằng
  `IT-ELIG-NODISPATCH-15` (3/4 dòng), và **không phải trong project chaos** — xem §6 để biết vì sao,
  điều gì nó tìm ra về **chính nó**, và vì sao dòng Trust vẫn cố ý để trống.
- **Chưa có partition một phần, webhook trùng lặp / sai thứ tự** (`P6-3` §6.1). Toxic `latency` đã
  dựng nhưng chưa scenario nào dùng.
- ~~`IT-12..17` mà `P6-3` §3/§9 trỏ tới không tồn tại.~~ **Đã quyết `2026-08-19`**
  (`OD-OPEN-01`): **sửa prompt, vì spec là nguồn sự thật**. `P6-3` §3/§9 giờ trỏ tới mục **4**
  (lease/fencing + crash recovery), **8** (dependency/auth/evidence outage fail closed) và **10**
  (migration/retention/audit/outbox recovery) của `specs/testing/03-integration-test-plan.md` —
  đúng những mục scenario thực sự phủ.
- **Recovery 8 ms đo một lần, một máy.** Nó là một quan sát, không phải phân phối. Và nó đo probe
  đầu tiên sau khi nối lại — không có khoảng chờ reconnect nào để đo.

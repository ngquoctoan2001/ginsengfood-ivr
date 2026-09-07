# W-0203 — P1.2 Full worker pipeline (`LocalMockE2E`)

Ngày: 2026-09-05. Trạng thái: **TESTS_PASS — P1.2 local/MOCK đạt exit 100/100 vòng**, không tự ACCEPTED.

## 1. Kết quả xác minh

- Vòng lặp: **100/100 vòng PASS**, **1110 task**, **0 failure**. Mỗi vòng được kiểm toàn bộ sổ kết
  quả trước khi sang vòng sau; một vòng đỏ dừng cả lượt chạy.
- Bất biến toàn cục sau 1110 task: **0 task có hai final result**, **0 task có hai dòng callback**,
  **0 kết quả không phải khách trả lời bị đếm là customer attempt**.
- Sổ giao nhận lấy từ **journal của bên nhận**, không phải từ DB của chính hệ thống:
  **1085 callback id**, **23 lần giao lại**, **0 lần không nhất quán** (mọi lần giao lại của cùng một
  callback id mang đúng một `Idempotency-Key` và đúng một payload).
- Retry có trần thật: mỗi scenario khai trước số lần dial được phép, và số dial quan sát được vượt
  trần là FAIL — không vòng nào vượt.
- Thời lượng vòng: min **1832 ms**, trung vị **2778 ms**, max **31955 ms**.
- JSON máy đọc: [`local-mock-e2e.json`](local-mock-e2e.json), chạy `2026-09-05T10:38:45Z` →
  `10:51:37Z`, `run_id=103845`.
- `IVR_EXECUTION_MODE=MOCK`, `SIM_PROVIDER=MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO` suốt lượt chạy;
  allowlist đích duy nhất là một chuỗi giả `mock-destination-allowlisted`.

## 2. Profile `LocalMockE2E`

Trước lượt này, cấu hình bật pipeline **không tồn tại ở dạng file**. Worker ship với mọi loop tắt —
đúng, vì một stack ai đó mở lên để xem không được phép quay số — nên mọi lần diễn tập đều dựng lại
cấu hình bật bằng tay dưới dạng một bức tường biến môi trường. Bức tường đó không review được, không
diff được và không trỏ vào evidence được.

- `src/Ivr.Worker/appsettings.Profile.LocalMockE2E.json`: scheduler + normalization + callback +
  retention bật, telephony/TTS vẫn fake deterministic.
- `src/Ivr.Api/appsettings.Profile.LocalMockE2E.json`: phần API (nhỏ, chủ yếu để hai nửa MOCK vault
  đồng ý về đích giả).
- `IvrConfigurationProfile` chọn file theo `IVR_CONFIG_PROFILE`; **profile là một file, không phải
  một environment**. Host environment vẫn là `Development`, nên mọi thứ khóa theo tên environment
  (developer surface, token dev) không đổi hành vi.
- Profile **từ chối nạp ngoài môi trường non-production**, dùng đúng danh sách đang gác developer
  surface (`NonProductionSurface.IsNonProductionEnvironment`) — một danh sách, không phải hai.
- Tên profile chỉ chấp nhận chữ và số: tên đi vào tên file, nên tên mang `/`, `\` hoặc `..` bị từ
  chối thay vì được làm sạch.
- Thứ tự lớp: `appsettings.json` → `appsettings.{Environment}.json` → **profile** → biến môi trường →
  command line. Harness vẫn ghi đè được một khóa mà không phải sửa file.

### Những gì profile KHÔNG nới

`ExecutionMode`, `SimProvider`, `RealCustomerCallAllowed` giữ nguyên MOCK/MOCK/NO.
`MockSchedulerDispatchGateway.IsReady` đòi cả ba, nên profile này không thể chạm tới vendor ngay cả
khi có vendor được cấu hình. An toàn duy nhất được nhấc là kill switch MOCK, và nó được nhấc trước
một gateway giả.

`Ivr:Scheduler:CallingWindow` được **giữ bật** và nới ra cả ngày (`0..1440`) chứ không tắt: cổng giờ
là một biện pháp an toàn, một cuộc diễn tập tắt nó sẽ thôi chạy chính đoạn code ra quyết định, còn
một cuộc diễn tập mà kết quả phụ thuộc vào giờ khởi chạy thì không phải diễn tập.

## 3. Chuỗi được chạy

`intake → eligibility → claim (lease + fencing) → script/TTS fake → mock DTMF/outcome →
normalize → callback fake (Target V1) → retention`.

Cả hai chương trình đều chạy: `GOLDEN_HOUR` (`ONLINE`, cửa sổ 300 s, offset `[0, 150]`) và
`TWENTY_FOUR_SEVEN` (`COD`, 900 s, `[0, 450]`), theo policy `mock-lab-v1` đã đăng ký.

**Lùi mốc cửa sổ, không đụng đồng hồ.** Policy xếp lượt gọi khách thứ hai ở 150 s (Giờ Vàng) hoặc
450 s (24/7). Một cuộc diễn tập chờ thật sẽ mất tám phút cho mỗi task no-answer và vì thế sẽ không
bao giờ được chạy. Harness lùi `confirmation_window_started_at` để **cả hai** mốc đều đã tới hạn.
Mọi luật khác giữ nguyên: deadline vẫn đóng ở `start + duration`, snapshot policy không bị sửa, và
intake vẫn đối chiếu từng offset khai báo với policy đã đăng ký.

## 4. Ma trận scenario

Mỗi dòng khai **kết quả cuối, có tính là customer attempt hay không, trạng thái giao callback, số
retry và số lần dial được phép**. Cột cuối là phần khiến "retry có trần" trở thành một khẳng định
kiểm được, chứ không phải một lời hứa.

### Chạy mỗi vòng (10 task)

| Mã | Disposition | Kết quả cuối | Counted | Callback | Dial |
|---|---|---|---|---:|---:|
| `CONFIRM` | Answered + `1` | `IVR_CONFIRMED` | có | `DELIVERED_ACCEPTED` | 1 |
| `CANCEL` | Answered + `0` | `IVR_CUSTOMER_CANCELLED` | có | `DELIVERED_ACCEPTED` | 1 |
| `NOANSWER` | RingTimeout | `IVR_NO_ANSWER_FINAL` | có | `DELIVERED_ACCEPTED` | 2 |
| `BUSY` | Busy | `IVR_NO_ANSWER_FINAL` | có | `DELIVERED_ACCEPTED` | 2 |
| `NOINPUT` | Answered, không phím | `IVR_NO_ANSWER_FINAL` | có | `DELIVERED_ACCEPTED` | 2 |
| `WRONGKEY` | Answered + `7` | `IVR_NO_ANSWER_FINAL` | có | `DELIVERED_ACCEPTED` | 2 |
| `BADNUMBER` | InvalidDestination | `IVR_INVALID_PHONE_FINAL` | **không** | `DELIVERED_ACCEPTED` | 1 |
| `AUDIOERR` | AudioError | **không có final** | — | **không có callback** | 2 |
| `ACKOK` | Answered + `1` | `IVR_CONFIRMED` | có | `DELIVERED_ACCEPTED` | 1 |
| `ACKDUP` | Answered + `1` | `IVR_CONFIRMED` | có | `DELIVERED_ACCEPTED` (ACK `DUPLICATE_ACCEPTED`) | 1 |

### Chạy mỗi 10 vòng (thêm 11 task)

| Mã | Nguồn lỗi | Kết quả cuối | Callback | Retry |
|---|---|---|---|---:|
| `REJECTED` | Rejected | `IVR_NO_ANSWER_FINAL` | `DELIVERED_ACCEPTED` | 0 |
| `UNREACH` | Unreachable | `IVR_INVALID_PHONE_FINAL` | `DELIVERED_ACCEPTED` | 0 |
| `DTMFERR` | DtmfError | không final | không callback | — |
| `DROPPED` | Dropped | không final | không callback | — |
| `ACKBLOCK` | ACK `BLOCKED_BY_CORE` | `IVR_CONFIRMED` | `DELIVERED_BLOCKED` | 0 |
| `ACKREVIEW` | ACK `REVIEW_REQUIRED` | `IVR_CONFIRMED` | `DELIVERED_REVIEW` | 0 |
| `ACKSTALE` | 409 `REJECTED_STALE` | `IVR_CONFIRMED` | `REJECTED_STALE` | 0 |
| `ACKCONFLICT` | 409 `IDEMPOTENCY_CONFLICT` | `IVR_CONFIRMED` | `IDEMPOTENCY_CONFLICT` | 0 |
| `ACK422` | 422 | `IVR_CONFIRMED` | `INVALID_DEAD_LETTER` | **0** |
| `ACK429` | 429 + `Retry-After: 3` | `IVR_CONFIRMED` | `RETRY_EXHAUSTED` | 3 |
| `ACK500` | 500 | `IVR_CONFIRMED` | `RETRY_EXHAUSTED` | 3 |

Ba điểm đáng nói trong bảng trên:

- **Technical exception không bao giờ là customer attempt, không bao giờ final, nên không bao giờ
  sinh callback.** Nó dừng sau `TechnicalRetryLimit + 1` lần dial và job bị treo
  `HELD_ADMIN_REVIEW`. Đây là bounded retry được kiểm bằng số dial, không phải bằng niềm tin.
- **`ACK422` phải chết ở lần gửi đầu tiên.** Một stack retry nó sẽ tiêu hết ngân sách retry để gửi
  lại thứ bên nhận đã từ chối trên nội dung.
- **`Retry-After` được chọn lớn hơn trần backoff cục bộ** (`MaxRetryDelayMilliseconds=2000`) để việc
  tôn trọng header là **quan sát được**: nếu `ACK429` và `ACK500` mất thời gian như nhau thì header
  đang bị bỏ qua và mọi khẳng định về nó là rỗng.

## 5. Fault injection

| Pha | Điều được khẳng định | Kết quả |
|---|---|---|
| Worker bị giết giữa cuộc gọi | lease được thu hồi, kênh trả về, job treo cho người xem, **không quay lại số đó** | `RECOVERY_REQUIRED=1`, `HELD_ADMIN_REVIEW/HELD_LEASE_RECOVERY=1`, **redial = 0**, 3/3 task còn lại chạy tiếp |
| Lease hết hạn | quarantine rồi trả kênh về `IDLE` | 0 kênh còn `QUARANTINED` sau cửa sổ |
| Kill switch bật | không claim, không dial, không result | 0 attempt / 0 result sau **100 nhịp scheduler** |
| Kill switch tắt | mọi task bị giữ chạy đúng một lần | 3/3 |
| Operator cắt cuộc đang gọi | ra **technical**, không phải kết quả khách | `IVR_TECHNICAL_EXCEPTION`, counted customer attempt = **0** |
| Bên nhận callback sập rồi trở lại | outbox giữ, backoff, giao khi bên kia quay lại | 4/4 giao, retry cao nhất **3/3** cho phép |
| Dead letter + replay | trần retry dừng đúng chỗ, rồi replay được | 3 chết ở `retry_count=3`, replay 3 → giao 3 |
| Hai worker chạy song song | không sinh outcome trùng | xem §1 |

**Về crash recovery, khẳng định cố ý KHÔNG phải "task vẫn hoàn thành".** Một cú sập giữa cuộc gọi
làm hệ thống mất khả năng nói khách đã nghe gì chưa, và câu trả lời đã thiết kế cho tình huống đó là
**dừng lại chứ không đoán**. Quay số lại mới là lỗi. Vì thế thứ được kiểm là: có thu hồi, thu hồi mà
không phát sinh cuộc gọi thứ hai, và hàng đợi không đứng.

**Về kill switch, hai điều phải chạy mới biết:**

1. `KillSwitchEngaged=true` cùng `Enabled=true` **không khởi động được** —
   `MockTelephonyOptionsValidator` từ chối thẳng tổ hợp đó. Nên tư thế "đã bật kill switch" không
   phải "đã lên đạn nhưng đang giữ", mà là "không lên đạn", được kiểm **trước khi process được phép
   tồn tại**. Đó là bảo đảm mạnh hơn một cờ runtime.
2. Kill switch chặn **cuộc gọi tiếp theo**, không cắt cuộc đang gọi; cắt cuộc đang gọi là một nút
   khác (`POST /call-jobs:terminate-all`, danger tier). `IvrAdminEndpoints` nói đúng như vậy, và đó
   là lý do "kill switch giữa claim/dial" cần hai bài test chứ không phải một.

**Về DLQ/replay:** trần retry dừng đúng chỗ và trạng thái chết là `RETRY_EXHAUSTED` — nhìn thấy
được, đếm được. Nhưng **replay hiện chỉ làm được bằng SQL tay**: chưa có endpoint admin nào để đưa
một callback đã chết trở lại hàng đợi. Đây là một khoảng trống thật, ghi ở §7.

## 6. Retention

`RetentionJobHost` chạy **một lượt mỗi lần worker khởi động** — đúng cách một CronJob gọi nó — nên
mỗi lần restart worker trong lượt diễn tập cũng là một lượt retention.

Chu kỳ để **365 ngày**, và con số đó là quyết định về bán kính ảnh hưởng chứ không phải quyết định
chính sách (chu kỳ thật vẫn là đầu vào của Owner/Legal ở P5): database dev dùng chung với việc khác,
một chu kỳ ngắn sẽ khiến mỗi lần restart worker xóa dữ liệu của người khác như một tác dụng phụ.

Harness vì thế lùi ngày **cohort của chính nó** vượt chu kỳ rồi restart worker và đòi lại các dòng:

- Cohort quá hạn: **callbacks/results/attempts = 0**, **100 speech snapshot bị redact**.
- Cohort còn hạn: **100 task, 100 callback, 100 result, 100 attempt còn nguyên, 0 dòng bị redact**.
- `ivr_task_intake_outbox.created_at` **bất biến theo trigger** (bằng chứng intake append-only), nên
  harness không lùi được nó — và điều đó biến bài test thành mạnh hơn chứ không yếu hơn: với dòng
  outbox còn trong hạn, `ivr_call_jobs` bị chặn theo phụ thuộc và `ivr_confirmation_tasks` bị chặn
  sau nó. Lượt này vì vậy chứng minh **cả hai nửa** của hợp đồng: cái quá hạn bị xóa, cái còn bằng
  chứng phụ thuộc thì bị từ chối xóa. Một bài test chỉ chứng minh nửa đầu sẽ xanh y hệt trước một
  job xóa sạch mọi thứ.

## 7. Khoảng trống mở phát hiện trong lượt này

### F-1 (HIGH) — ghi song song với idempotency key khác nhau trả HTTP 500

`PostgresIdempotencyStore.ExecuteAsync` mở transaction **SERIALIZABLE**, đọc `ivr_idempotency_keys`
rồi chèn vào chính bảng đó. Hai request đồng thời mang **hai key khác nhau** vì thế tạo dangerous
structure dưới SSI và tất cả trừ một cái bị hủy ở COMMIT với `SQLSTATE 40001`. API ánh xạ nó thành
`IVR_INTERNAL_ERROR` / **HTTP 500**, nên caller không phân biệt được "xung đột, thử lại" với "dịch
vụ hỏng".

- Đo được, không phải suy đoán: **10 request `POST /eligibility-checks` đồng thời → 9 trả 500**
  (intake đi lối `ReadCommitted` nên **10/10 OK**). Con số này được đo lại một lần ở mỗi lượt chạy
  và nằm trong JSON.
- Trong lượt 100 vòng, workaround của harness (retry có trần + giới hạn 2 admission đồng thời) phải
  dùng **99 lần retry** để đi hết. Chừng nào con số đó còn khác 0 thì workaround còn đang gánh, và
  defect còn mở.
- Vì sao không vá ngay ở đây: `MutationReplayFilter` bọc **cả lần gọi endpoint** trong đúng
  envelope Serializable đó, nên retry transaction một cách đại trà sẽ chạy lại nguyên handler HTTP.
  Từng call site phải được chứng minh là an toàn khi chạy lại trước đã — đó là một việc riêng, không
  phải một miếng vá kèm theo.
- Liên quan: `W-0197` đã sửa đúng lớp lỗi này cho **một** lối đi (tách nhánh replay
  `ReadCommitted` + advisory lock) và ghi rõ "caller Serializable cũ giữ nguyên". F-1 là phần còn
  lại của chính lớp lỗi đó, nay hiện ra dưới tải song song thay vì dưới một request xung đột.

### F-2 (MEDIUM) — không có lối replay dead-letter cho người vận hành

Callback rơi vào `RETRY_EXHAUSTED` hoặc `INVALID_DEAD_LETTER` chỉ có thể đưa lại hàng đợi bằng
`UPDATE` tay. Không có endpoint admin nào làm việc đó, nên trong sự cố thật thao tác này sẽ diễn ra
trực tiếp trên database production.

### F-3 (thông tin) — MOCK không có kill switch runtime

`MockSchedulerDispatchGateway` không hỏi `IDispatchGate`, nên cờ `globalDialKillSwitch` (thứ gác
LAB/production qua `AsteriskSchedulerDispatchGateway`) **không có tác dụng ở MOCK**. Điều này đúng
theo thiết kế — `DispatchGate` trả `MOCK_MODE → không cho phép`, nối nó vào sẽ chặn mọi cuộc gọi giả
— nhưng hệ quả cần ghi: ở MOCK, dừng quay số nghĩa là **restart worker với telephony tắt**, và cuộc
gọi đang chạy phải cắt bằng `terminate-all`.

## 8. Tái lập

Yêu cầu: SDK theo `global.json`, Node 20+, Docker đang chạy, PostgreSQL dev đã migrate. Không cần
secret ngoài repo.

```bash
pnpm db:up && pnpm db:migrate
dotnet build Ivr.sln -c Release
pnpm e2e:mock -- --rounds 100 --workers 2 --extended-every 10
```

Bản rút gọn để kiểm nhanh đường dây (bỏ fault injection, hai vòng):

```bash
pnpm e2e:mock:smoke
```

Harness tự dựng fake Sales (WireMock, mappings ở `deploy/docker/fake-sales-e2e/`), API và worker;
tự dọn khi xong; ghi log vào `ci-artifacts/local-mock-e2e/` và JSON vào evidence pack này.

## 9. Giới hạn

- **Local/MOCK, working-tree candidate.** Không phải hosted CI, không phải exact-SHA release proof,
  không phải staging. Không có vendor, không có SIM, không có khách hàng thật.
- Đây **không** phải bằng chứng cho P4 (soak/perf/chaos trên staging): 100 vòng trong ~13 phút trên
  một máy nói về tính đúng đắn lặp lại, không nói về hành vi 72 giờ dưới tải thật.
- Sổ giao nhận chỉ tính callback của đúng `run_id` này; journal WireMock giới hạn 60000 bản ghi.
- `mock-lab-v1` là policy candidate MOCK/LAB. Lượt này **không** chạy trên `gh-247-prod-v1`.
- F-1 còn mở; mọi con số thông lượng trong pack này đã bị workaround của F-1 làm chậm và không nên
  đọc thành capacity.
- Chưa ai ký. Trạng thái là `TESTS_PASS`, không phải `ACCEPTED`.

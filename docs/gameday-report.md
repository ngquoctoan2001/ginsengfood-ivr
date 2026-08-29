# Game-day report — IVR Order Confirmation (`W-0042` · `P6-3`)

Ngày chạy: `2026-08-18` · Môi trường: **harness chaos tự dựng**, không phải staging — xem §5

## 1. Cách chèn lỗi, và mức chứng minh của từng cách

Hai cơ chế, và chúng **không** chứng minh cùng một mức. Gộp lại sẽ làm báo cáo nghe mạnh hơn thực tế.

| Cơ chế | Dùng ở | Mức |
| --- | --- | --- |
| **Toxiproxy** — cắt/làm chậm kết nối thật | `CHAOS-DB-02`, `CHAOS-RECOVERY-04`, `CHAOS-DUPLICATE-06` | **lỗi mạng thật**: socket bị cắt bởi một thứ nằm giữa tiến trình và Postgres, đúng hình dạng một partition |
| **Chèn ở tầng mã** | `CHAOS-DOWNSTREAM-01`, `CHAOS-SIM-03` | biên phụ thuộc ngoài **chưa có endpoint thật** để cắt; `P6-3` §5 cho phép rõ ràng |

## 2. Kết quả từng scenario

### `CHAOS-DB-02` — mất kết nối database

| Hỏi | Đáp |
| --- | --- |
| fail-closed? | **có** — `/health/ready` chuyển **503**, check `database` báo không sẵn sàng |
| ghi trong lúc mất kết nối? | **ném lỗi**, không nuốt |
| mất dữ liệu? | **không** — dòng ghi trước sự cố còn nguyên sau khi khôi phục |
| dữ liệu rác? | **không** — dòng ghi hỏng giữa chừng không để lại gì |
| recovery time | **8 ms** — xem ghi chú bên dưới |

Con số 8 ms nghĩa là **probe đầu tiên sau khi nối lại đã xanh**. Nó đo thời gian của chính probe chứ
không đo một khoảng chờ reconnect, vì **không có khoảng chờ nào để đo**: Npgsql mở kết nối vật lý
mới và nó đi qua ngay. Nói rõ như vậy thay vì báo cáo "phục hồi dưới 10 ms" như một thành tích.

Ghi nhận có ý nghĩa hơn con số: một lệnh ghi **thất bại** khi mất kết nối chứ không âm thầm biến
mất. Ghi bị nuốt là kết cục **tệ hơn cả sự cố** — người gọi được báo là đã nhận, mà không có gì giữ.

### `CHAOS-DOWNSTREAM-01` — Sales không trả lời

| Hỏi | Đáp |
| --- | --- |
| IVR tự kết luận đã xác nhận? | **không** — `DeliveryStatus=RETRY_PENDING`, `AcknowledgedAt` rỗng |
| mất tín hiệu? | **không** — `NextRetryAt` được đặt, món nợ vẫn còn |
| retry có bị chặn trên? | **có** — `MaxRetries`, và breaker mở sau chuỗi hỏng liên tiếp |
| metric mà alert đọc có nhúc nhích? | **có** — `ivr_result_callbacks_total{outcome=RETRY_PENDING}` |

**Một giả định của tôi bị chính hệ thống bác bỏ.** Bản đầu của scenario chạy batch nhiều lần rồi đòi
breaker phải mở. Nó không mở — vì message đã ở `RETRY_PENDING` với `NextRetryAt` ở tương lai thì
**không được lấy ra nữa**. Tức là **backoff đã chặn hammering trước khi breaker được hỏi tới**;
breaker là lớp thứ hai, dành cho một loạt message khác nhau cùng hỏng. Assertion đã sửa lại theo
đúng hành vi thật, và giữ luôn phép kiểm "chạy lại ngay thì gửi đi **0** lần".

Chuỗi chứng minh cho alert nói rõ ở đây: scenario này chứng minh **sự cố thật làm counter thật
nhúc nhích**; việc luật cảnh báo **nổ** trên hình dạng đó do `IT-SLO-ALERT-01` chứng minh riêng
bằng bộ đánh giá luật của Prometheus. Không lượt chạy nào chứng minh cả hai, và không nên nói như thể có.

### `CHAOS-SIM-03` — SIM rớt cuộc gọi

| Hỏi | Đáp |
| --- | --- |
| rớt cuộc = no-answer? | **không** — `IVR_TECHNICAL_EXCEPTION`, kiểm trên **cả 5** disposition lỗi thiết bị |
| có tính vào lượt của khách? | **không** — `is_counted_customer_attempt=false` (DT-02) |
| auto-disable đúng ngưỡng? | **có, sau W-0144** — lần hỏng thứ 3 trong cửa sổ 10 phút chuyển kênh sang `HEALTH_FAILED`; healthy hoặc lỗi sau hơn 10 phút reset (DT-04) |
| counter mà alert DT-04 đọc? | **có** — `ivr_channel_quarantines_total` tăng |

Bất biến được kiểm trên **toàn bộ** tập disposition lỗi thiết bị chứ không một ví dụ: một disposition
mới thêm vào enum ngày mai phải rơi về cùng phía của lằn ranh này, và một ví dụ sẽ không bắt được.

### `CHAOS-RECOVERY-04` — phục hồi sau sự cố

| Hỏi | Đáp |
| --- | --- |
| gửi đi trong lúc store mất? | **0 lần** |
| sau khi khôi phục, backlog chảy? | **có**, đúng **một** lần gửi |
| chạy lại lần nữa? | **0** lần gửi thêm |
| idempotency key giữ nguyên qua sự cố? | **có**, và chỉ có **một** dòng mang key đó |

Thứ tự "ghi trước, gửi sau" là điều được kiểm ở đây: gửi trước rồi ghi sau chính là cách một bản sao
ra đời — lệnh gửi sống sót qua sự cố còn bản ghi thì không.

### `CHAOS-GUARD-05` — giới hạn blast radius

Mọi upstream trong `deploy/chaos/toxiproxy.staging.json` phải là alias container dùng-một-lần hoặc
loopback. Kiểm âm: đổi thành một hostname có thể giải được → **đỏ**. Giới hạn blast radius là thứ
được **ép**, không phải thứ được hứa.

### `CHAOS-DUPLICATE-06` — partition **một phần** trên nhánh callback

Worker vẫn tới được Sales nhưng **không tới được database**: callback đã giao, việc ghi lại thì
không. Lease hết hạn trong lúc worker vẫn sống và vẫn đúng, worker khác nhặt dòng lên, Sales
được báo **lần thứ hai**. Lần giao thứ hai là **không tránh khỏi** — thứ đo được là nó có
**nhận ra được** không (cùng `callback_id`, cùng idempotency key, payload giống **từng byte**),
và worker về muộn có **bị chặn** không.

Kiểm âm: gỡ điều kiện lease → đỏ; gỡ nhánh nhặt-lại `SENDING` quá hạn → đỏ với **callback mất**,
tệ hơn trùng lặp: Sales đã được báo một lần còn IVR tin rằng chưa báo lần nào.

Bản đầu của khẳng định *"worker về muộn bị chặn"* **sống sót** khi gỡ lease, vì nó đặt sau lúc
dòng đã acknowledge, và khi ấy thứ từ chối là **trạng thái** chứ không phải **lease**. Sửa
bằng cách đổi **thời điểm** khẳng định, không phải đổi lời.

## 3. Điểm yếu phát hiện

| # | Điểm yếu | Trạng thái |
| --- | --- | --- |
| 1 | **Alert DT-04 của `W-0041` đang theo dõi sai sự kiện.** `ivr_channel_quarantines_total` mới đếm nơi lease hết hạn; auto-disable theo `fail_count` xảy ra ở `PostgresTelephonyDispatchStore` và **không** chạm vào counter đó. Alert mang nhãn DT-04 nhưng đọc một metric mà chuyển trạng thái DT-04 không bao giờ raise — tệ hơn không có alert, vì nó **trông như đã phủ**. | **đã sửa trong slice này**; `CHAOS-SIM-03` giữ cho nó không tái diễn |
| 2 | `docs/slo.md` §4 từng gộp luật per-kênh của DT-04 với alert toàn đội, rồi runtime chỉ đếm 3 lỗi liên tiếp mà không giữ cửa sổ 10 phút. | **đã sửa đầy đủ tại W-0144** — tách hai nửa và persist mốc cửa sổ dùng chung cho cả provider failure/lease-expiry |
| 3 | `P6-3` §3/§9 trỏ tới "fail-closed profiles `IT-12..17`" trong `specs/testing/03-integration-test-plan.md`. **Dải ID đó không tồn tại** ở đâu trong `specs/` hay `plan/`; file đó có 10 mục đánh số. Scenario được map vào mục **4**, **8**, **10** và ma trận `ARCH-05` §1 thay vì tuyên bố phủ một dải ID không có thật. | **cần chủ sở hữu quyết**: sửa prompt hay bổ sung spec |
| 4 | `ivr_call_attempts_total` và `ivr_call_results_total` vẫn chưa có call site (mang từ `W-0041`). | **chưa sửa** — không panel/alert nào dùng, cổng `UT-DASH-PII-04` chặn nếu ai thêm |

## 4. Phủ so với ma trận `ARCH-05` §1

| Hệ thống down | Có scenario? |
| --- | --- |
| Order Core (during callback) | ✅ `CHAOS-DOWNSTREAM-01` |
| SIM Gateway | ✅ `CHAOS-SIM-03` |
| Database / hạ tầng | ✅ `CHAOS-DB-02`, `CHAOS-RECOVERY-04` |
| Ops Sellable Gate | ❌ — Core gọi, không phải IVR gọi; IVR không có lối chèn lỗi vào đó |
| Trust/Contact resolver | ❌ chưa có |
| CRM do-not-call | ❌ chưa có |
| Evidence Registry | ❌ chưa có |

Bốn dòng ❌ là **chưa phủ**, không phải "không áp dụng".

## 5. Cái này KHÔNG chứng minh

- **Không có staging.** `P6-3` §4 nói chaos chạy ở dev/staging; ở đây nó chạy trong **harness tự
  dựng của bộ test**, container sinh ra và xoá đi theo từng lượt. Đó là lý do giới hạn blast radius
  mạnh hơn (không có tuyến nào ra ngoài), nhưng cũng nghĩa là **chưa lượt nào chạy trên một hệ đã
  triển khai**. `deploy/chaos/` là config cho một staging **chưa tồn tại** (`W-0063`).
- **Không có "alert-fire capture" thật** (§10). Không có Alertmanager nào để bắt. Cái có là hai nửa
  ghép lại và đã nói rõ ở §2.
- **Không có partition mạng một phần** (§6.1) — mới cắt hoàn toàn và làm chậm. Toxic `latency` đã
  dựng nhưng chưa scenario nào dùng.
- **Không có webhook trùng lặp / sai thứ tự** (§6.1) — chưa dựng.
- **Recovery time chỉ đo một lần, một máy.** Nó là quan sát, không phải phân phối.

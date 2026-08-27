# W-0041 — Evidence: Dashboards, SLO & alerting (`P6-2`)

Ngày: `2026-08-18` · Trạng thái: `TESTS_PASS` cho artifact + luật cảnh báo; **chưa có tín hiệu nào
rời khỏi tiến trình** — xem §6

## 1. Phát hiện quyết định toàn bộ slice: 6/7 instrument của `P6-1` chưa ai gọi

Trước khi viết dòng dashboard nào, tôi đếm call site của từng instrument. Kết quả:

| Instrument (khai ở `P6-1`) | Call site production | |
| --- | --- | --- |
| `ivr_result_callbacks_total` | `CallbackDispatcher` | ✅ |
| `ivr_result_callback_duration_seconds` | `CallbackDispatcher` | ✅ |
| `ivr_intake_decisions_total` | *không có* | ❌ |
| `ivr_task_intake_duration_seconds` | *không có* | ❌ |
| `ivr_fail_closed_total` | *không có* | ❌ |
| `ivr_call_attempts_total` | *không có* | ❌ |
| `ivr_call_results_total` | *không có* | ❌ |

`P6-1` khai đủ instrument nhưng mới nối **một** call site — evidence `W-0040` §5 có nói "mới
instrument 1/5 chặng", nhưng §3 lại đặt tiêu đề "Metrics đo từ nguồn thật", và điều đó chỉ đúng với
callback.

Hệ quả với `P6-2`: dựng SLO/alert lên năm metric không ai phát thì mọi panel là **vạch phẳng vĩnh
viễn**, và vạch phẳng đọc như "khoẻ mạnh" chứ không đọc như "không có dữ liệu". `P6-2` §11 cấm đúng
điều đó (`SLO không đo từ metric thật`). Nên slice này nối call site trước, rồi mới vẽ.

## 2. Nối instrumentation — bốn call site, không đổi signature nào

| Nơi | Đo cái gì | Vì sao đúng chỗ đó |
| --- | --- | --- |
| `TaskIntakeService.IntakeAsync` | `ivr_intake_decisions_total` + `ivr_task_intake_duration_seconds` | **một điểm ra duy nhất**, nên quyết định và độ trễ ra từ cùng một sự kiện, không thể lệch nhau như hai probe đặt rời |
| `EligibilityService.EvaluateAsync` | `ivr_fail_closed_total` | ở trạng thái nghỉ, một task **bị giữ** và một task **chưa ai gửi** trông giống hệt nhau; chỉ tại điểm quyết định mới phân biệt được |
| `PostgresSchedulerStore` (quarantine) | `ivr_channel_quarantines_total` *(instrument mới)* | dòng dữ liệu sau đó chỉ cho biết kênh **đang** bị khoá, không bao giờ cho biết nó **vừa mới** bị khoá — mà "ba lần trong mười phút" cần đúng thời điểm chuyển |

`TASK_SKIPPED_TRUSTED_CUSTOMER` trong evidence W-0041 là `HISTORICAL_EVIDENCE` và vẫn không tính
vào fail-closed của baseline cũ. Từ `OD-18`/W-0123, runtime không phát sinh decision này; occurrence
mới phải được cảnh báo như regression thay vì policy outcome hợp lệ.

~~`ivr_call_attempts_total` và `ivr_call_results_total` **vẫn chưa có call site**, và slice này
**cố ý không** vẽ panel/alert nào dùng chúng.~~ **Đã đóng `2026-08-19`**: hai call site được nối ở
`PostgresSchedulerStore` (lúc dispatch) và `ResultRepository` (lúc chuẩn hoá), **cả hai sau commit**
— đếm trước commit thì mỗi lần rollback làm counter cao hơn database, và một tỉ lệ có mẫu số lớn hơn
thực tế **đọc như hiệu năng tốt hơn thực tế**. Hai panel `ARCH-06` §1 đã vẽ; `IT-OBS-OUTCOME-09`
khẳng định counter **nổ thật** và **không nổ** khi không có gì được ghi. Xem `docs/slo.md` §7–§8.

`missed_deadline_count` **đã đóng cùng ngày**, và việc nối nó **làm lộ một lỗ hổng của chính hai
call site vừa nối ở trên**: `CloseMissedDeadlinesAsync` là **nhánh duy nhất** một job đạt kết quả
FINAL mà **không đi qua normalization** — scheduler tự ghi dòng `IVR_CAPACITY_EXCEPTION`. Nên mọi
lần trượt deadline **vắng mặt** khỏi `ivr_call_results_total`, và `confirm_rate` có mẫu số **bỏ sót
đúng phần thất bại** → đọc **cao hơn sự thật**, lệch **nhiều nhất đúng lúc dung lượng tệ nhất**.
Call site mới ghi **cả hai** instrument. Xem `docs/slo.md` §9.

## 3. Alert được chứng minh là **nổ thật**, không phải chỉ khai báo

Alert rule là loại artifact có thể parse sạch, trỏ đúng metric sống, mà vẫn **không bao giờ nổ**.
Đọc YAML không phát hiện được điều đó. Nên luật ở đây được đưa qua **chính bộ đánh giá luật của
Prometheus** (`promtool test rules`, image `prom/prometheus:v2.54.1`) với chuỗi số liệu tổng hợp.

Mỗi ngưỡng có **một cặp**: một ca phải nổ, một ca phải **im lặng**.

| Test ID | Alert | Ca nổ | Ca im lặng |
| --- | --- | --- | --- |
| `IT-SLO-LAT-03` | `IvrCallbackRevalidateLatencyBreach` (D-04) | p95 rơi vào bucket `(5,10]` → nổ `page` | toàn bộ quan sát ≤ 1s → im |
| `IT-SLO-ALERT-01` | `IvrDownstreamFailClosedSpike` (DO-06) | 5/10 mỗi phút = 50% → nổ `page` | 1/100 mỗi phút = 1% → im |
| `IT-SLO-SIM-02` | `IvrChannelAutoDisableBurst` (DT-04) | ~10 lần/10 phút ≥ 3 → nổ `page` | đúng 1 lần → im |

Ca im lặng quan trọng ngang ca nổ: một luật nổ với mọi thứ sẽ bị tắt trong một tuần, và sau đó nó
không bắt được gì nữa. `P6-2` §11 cấm alert nhiễu ngang với việc thiếu alert.

Kiểm âm: đổi ngưỡng latency từ `> 5` thành `> 500` → `IT-SLO-LAT-03` **đỏ**; khôi phục → xanh.

## 4. Cổng chặn dashboard vượt quá phần đã instrument

`UT-DASH-PII-04` không đọc danh sách metric do người viết tay. Nó **đi ngược từ artifact về call
site**: quét `src/` tìm mọi lời gọi `IvrTelemetry.Record*`, tra qua `InstrumentsByRecorder` ra tên
instrument, rồi đòi **mọi** token `ivr_*` trong dashboard và alert phải là một trong hai thứ —
metric thực sự được ghi, hoặc tag nằm trong allowlist của `P6-1` (đổi `.` thành `_` như exporter
làm).

Ba kiểm âm, cả ba **đỏ tất định**:

| Vi phạm dựng lên | Kết quả |
| --- | --- |
| alert dùng `ivr_call_results_total` (khai báo nhưng **không** call site) | ❌ đỏ |
| alert dùng `ivr_correlation_id` làm dimension của metric | ❌ đỏ, kèm lý do **cardinality** chứ không phải "ngoài allowlist" |
| bỏ include fragment CI / đặt `allow_failure: true` / bỏ blank entrypoint | ❌ đỏ, đúng ba thông báo khác nhau |

`UT-DASH-PII-04B` chốt luôn bản đồ: mọi phương thức `Record*` public phải có mặt trong
`InstrumentsByRecorder`, nếu không cổng trên sẽ **âm thầm** ngừng phủ metric đó.

`UT-DASH-RUNBOOK-05` đòi mỗi alert có `runbook_url` và mỗi link **giải được** tới anchor có thật
trong `docs/slo.md`. Một link chết không phải lỗi nhỏ: người trực bấm một lần vào link chết thì
thôi bấm, và từ đó annotation chỉ còn là trang trí.

## 5. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `promtool check rules` | `SUCCESS: 5 rules found` |
| `promtool test rules` × 3 file | `SUCCESS` cả ba |
| `dotnet test --filter "TestId~IT-SLO"` | 3/3 |
| `dotnet test --filter "TestId~UT-DASH"` | 3/3 |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` (đã mở rộng cho fragment observability) |
| `test:traceability` | `TEST_TRACEABILITY_CURRENT=252` (+6) |
| `scan-pii.sh` | `PII_SCAN_PASS` |

**Một test cũ phải sửa, và nó là nửa còn lại của flake `A-0230`.** `IT-BOOT-02` khẳng định
`/health/ready` trả 503 vì "host bootstrap không có database phía sau". Giả định đó **không phải sự
thật mà là may mắn**: host để mặc định thì trỏ vào cổng Postgres cục bộ, và máy này đang chạy một
Postgres native ở `0.0.0.0:5432`, nên probe thành công và test đỏ. Bản sửa `P6-1` của tôi đã đổi
assertion thành 503 nhưng không làm tiền đề thành sự thật. Giờ host được ghim vào cổng không thể
phục vụ (`Port=1`), nên "không có database" là **sự kiện**, không phải trạng thái của máy người chạy.

## 6. Cái này KHÔNG chứng minh

- **Chưa có metric nào rời khỏi tiến trình.** Không exporter OTLP; collector/backend là `W-0063`,
  vẫn `BLOCKED_EXTERNAL`. Dashboard và alert ở đây là **artifact as-code đã qua bộ đánh giá luật
  thật**, không phải thứ đang chạy trên production. Không có screenshot dashboard vì không có
  Grafana nào để chụp — `P6-2` §10 đòi screenshot, và tôi **không dựng ảnh giả**.
- **`ivr_call_attempts_total` / `ivr_call_results_total` vẫn chưa có call site**, nên
  `confirm_rate`, `cancel_rate`, `no_answer_rate` của `ARCH-06` §1 **chưa đo được**. Không panel nào
  dùng chúng, và `UT-DASH-PII-04` sẽ đỏ nếu ai thêm.
- ~~**`missed_deadline_count`** chưa có instrument.~~ **Đã đóng `2026-08-19`** —
  `ivr_missed_deadline_total`, luật `IvrConfirmationDeadlineMissed`, panel #10,
  `IT-SLO-CAPACITY-04` + `IT-OBS-DEADLINE-10`. Ngưỡng **suy ra từ mô hình dung lượng**, không phải
  chọn: mô hình nói pool prod phủ được đỉnh, nên dưới giả định của nó **không lần trượt nào xảy ra**
  — ngưỡng là **không**, và một lần trượt là **một giả định bị bác bỏ**.
- **`cost_per_confirmed_order` và `sim_failure_rate` theo slot** vẫn **không có instrument**.
  `cost_per_confirmed_order` **không thể** có: mẫu số đã đo được (`analytics.agg_kpi_daily`,
  `W-0055`) nhưng **tử số cần một báo giá từ bên ngoài** — cả 6 dòng ở `docs/cost-model.md` §3 còn
  trống (`W-0008`). `CAP-ALERT-04` khẳng định lý do đó vẫn còn đúng, nên nó **không sống lâu hơn sự
  thật của nó**. Alert "queue backlog" mà `P6-2` §4 liệt kê vẫn chưa dựng.
- **Panel burn-rate nhiều cửa sổ chưa có.** Ngân sách lỗi trong `docs/slo.md` mới là định nghĩa.
- **Ngưỡng đánh dấu `proposed` chưa được chủ sở hữu phê duyệt** — 20% fail-closed và 30% rejection
  là suy luận, không phải baseline đo được. Chỉ D-04 (5s) và DT-04 (3/10′) là `LOCKED`.
- **Panel integration-status khớp UI (`P6-2` §6.4) chưa làm**: console đọc từ API admin chứ không từ
  Prometheus, nên "khớp nguồn thật" cần một quyết định về nguồn nào là chuẩn — chưa có.

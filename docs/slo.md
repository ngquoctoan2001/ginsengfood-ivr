# SLO / SLI — IVR Order Confirmation (`W-0041` · `P6-2`)

Ngày: `2026-08-18` · Trạng thái: **đề xuất**, trừ hai mục ghi `LOCKED` bên dưới

Tài liệu này định nghĩa mục tiêu, cách đo, và ngân sách lỗi cho IVR. Nó cũng là đích của mọi
`runbook_url` trong `deploy/observability/alerts/ivr-slo.rules.yml` — mỗi alert trỏ tới đúng mục
của nó ở đây. Runbook vận hành đầy đủ là `P9-2`; phần dưới chỉ đủ để người trực biết **alert này
nghĩa là gì và điều gì KHÔNG nên làm**.

## 1. Nguyên tắc: chỉ đặt SLO lên thứ đo được

Mỗi SLI dưới đây đọc từ một instrument mà **có call site thật trong code production**. Điều này
được ép bằng test chứ không bằng thiện chí: `UT-DASH-PII-04` đi ngược từ biểu thức trong dashboard
và alert về tới call site, và đỏ nếu artifact vượt quá phần đã instrument.

Lý do phải ép: một instrument được khai báo nhưng không ai gọi sẽ scrape ra **vạch phẳng**, mà
vạch phẳng thì đọc như "khoẻ mạnh" chứ không đọc như "không có dữ liệu". Đó là kiểu hỏng tệ nhất
của observability — nó không im lặng, nó nói dối.

| SLI | Instrument | Call site |
| --- | --- | --- |
| callback revalidate latency | `ivr_result_callback_duration_seconds` | `CallbackDispatcher` |
| callback delivery outcome | `ivr_result_callbacks_total` | `CallbackDispatcher` |
| fail-closed ratio | `ivr_fail_closed_total` | `EligibilityService` |
| intake decision mix | `ivr_intake_decisions_total` | `TaskIntakeService` |
| intake latency | `ivr_task_intake_duration_seconds` | `TaskIntakeService` |
| channel auto-disable | `ivr_channel_quarantines_total` | `PostgresSchedulerStore` |

<a id="callback-revalidate-latency"></a>

## 2. Callback revalidate latency — `LOCKED` (D-04)

| | |
| --- | --- |
| **Mục tiêu** | p95 ≤ **5s** |
| **Đo bằng** | `histogram_quantile(0.95, …ivr_result_callback_duration_seconds_bucket…)` theo `ivr_program` |
| **Ngân sách lỗi** | 1% số lần gửi được phép vượt 5s trong 30 ngày |
| **Alert** | `IvrCallbackRevalidateLatencyBreach` · `severity: page` · `for: 10m` |

D-04 chốt Core revalidate trả lời trong **3–5s**; mục tiêu lấy cận trên. Đây là chặng **đồng bộ**,
nên độ trễ ở đây là độ trễ mà xác nhận với khách hàng thừa hưởng.

**Người trực không được làm gì:** không tắt callback để "giảm alert". Outbox sẽ dồn, và đơn vẫn cần
được xác nhận — vấn đề chỉ chuyển từ chỗ nhìn thấy sang chỗ không nhìn thấy.

<a id="downstream-fail-closed"></a>

## 3. Fail-closed ratio — đề xuất (DO-06)

| | |
| --- | --- |
| **Mục tiêu** | < **20%** số lần đánh giá fail closed |
| **Đo bằng** | `rate(ivr_fail_closed_total) / rate(ivr_intake_decisions_total)` |
| **Alert** | `IvrDownstreamFailClosedSpike` · `severity: page` · `for: 10m` |

Alert đặt trên **tỷ lệ**, không phải trên số tuyệt đối. Hai lý do: fail-closed là hành vi **đúng
theo thiết kế** (DO-06) nên một vài lần giữ không phải sự cố; và tỷ lệ sống sót qua thay đổi lưu
lượng, còn ngưỡng tuyệt đối thì không.

Historical rows mang `TASK_SKIPPED_TRUSTED_CUSTOMER` là `LEGACY_READ` và **không** được tính vào
active fail-closed numerator. Từ `OD-18`/OpenAPI `draft.21`, runtime không phát sinh trusted-skip
mới; mọi occurrence mới của decision này phải được coi là contract/runtime regression.

**Ngưỡng 20% là đề xuất**, chưa có baseline production để hiệu chỉnh.

<a id="legacy-skip-candidates"></a>

## 3b. Đơn từng thuộc diện trusted-skip — quan sát, không phải SLO (`OD-18`)

| | |
| --- | --- |
| **Đo bằng** | `sum by (ivr_program) (increase(ivr_legacy_skip_candidate_total[1h]))` |
| **Kỳ vọng** | `0` |
| **Alert** | **không có** — xem lý do bên dưới |

Đây **không** phải SLO và cố ý không gắn alert. Nó trả lời đúng một câu hỏi mà `W-0123` không có
cách nào trả lời bằng số: cutover `OD-18` thực sự làm tăng bao nhiêu cuộc gọi?

Lập luận của `W-0123` là Module 3 chưa gửi `trust.risk_evidence_available` (theo `W-0118`), nên
nhánh skip cũ chưa từng bỏ qua ai và gỡ nó không đổi hành vi của bất kỳ đơn nào. Lập luận đó hợp lý
nhưng **không có target database nào truy cập được để xác nhận** — evidence ghi `ENV_BLOCKED`.
Counter này biến suy luận thành số đo: đứng yên ở `0` là bằng chứng lập luận đúng; nhảy lên `n` thì
`n` chính là số đơn trước kia được bỏ qua và nay bị gọi.

Mỗi increment đếm một task mà Module 3 gửi kèm đúng hình dạng của predicate đã nghỉ hưu: không có
veto `trusted_skip_allowed=false`, `risk_flags` rỗng, và `trust.risk_evidence_available=true`.
Snapshot hỏng hoặc thiếu trả `false` — predicate cũ cũng đòi bằng chứng dương, nên "không xác định
được" chưa bao giờ là skip, và một counter đoán mò sẽ thổi phồng chính con số nó tồn tại để đo.

Không alert vì đây **không phải lỗi phía IVR** và không có hành động runtime nào để gọi ai đó dậy
lúc 2 giờ sáng. Non-zero nghĩa là producer phía Module 3 vẫn gửi tín hiệu đã được yêu cầu ngừng
gửi — việc cần làm là một cuộc trao đổi tích hợp với `IR-06`, không phải một trang page. Nhưng mỗi
đơn ở đây là một cuộc gọi thật tới một khách hàng thật, nên nó phải nằm trên dashboard chứ không
nằm trong log.

Counter được ghi ở **intake**, không phải ở eligibility. Đọc trust metadata để *quyết định* là điều
`OD-18` cấm; đọc để *đếm nhà sản xuất đã gửi gì* là kiểm toán. `UT-M3-AUTHORITY-11` giữ hai việc đó
tách nhau theo file, `IT-M3-AUTHORITY-12` chứng minh chính payload đó vẫn được gọi.

<a id="channel-auto-disable"></a>

## 4. Channel auto-disable — `LOCKED` (DT-04)

| | |
| --- | --- |
| **Ngưỡng** | ≥ **3** lần trong **10 phút** |
| **Đo bằng** | `increase(ivr_channel_quarantines_total[10m])` |
| **Alert** | `IvrChannelAutoDisableBurst` · `severity: page` · `for: 0m` |

`for` để **0m** có chủ đích: biểu thức đã mang cửa sổ 10 phút của chính nó, thêm một cửa sổ nữa sẽ
hoãn trang báo đúng bằng độ dài cửa sổ định nghĩa sự cố.

Đếm tại **thời điểm chuyển trạng thái**, không phải từ số dòng đang `QUARANTINED`: hàng đợi lúc sau
chỉ cho biết kênh đang bị khoá, không bao giờ cho biết nó **vừa mới** bị khoá.

**Phân biệt hai nửa của DT-04, vì chúng không cùng một thứ:**

| | Ở đâu | Phạm vi |
| --- | --- | --- |
| *auto-disable* — `fail_count ≥ 3` thì khoá kênh | **trong code**, `PostgresTelephonyDispatchStore` | **từng kênh** |
| *+ alert* — báo cho vận hành | luật Prometheus ở đây | **toàn đội**, ≥3 lần khoá trong 10 phút |

Luật per-kênh không phải một alert và không thể là alert: nó phải chạy đồng bộ tại thời điểm sự
kiện để kênh hỏng không được cấp phát tiếp. Alert ở đây là nửa "báo cho người", và ngưỡng toàn đội
của nó là **đề xuất**, không phải con số DT-04 chốt.

Cả **hai** nơi kênh bị đưa ra khỏi phục vụ đều được đếm: lease hết hạn (`PostgresSchedulerStore`) và
`fail_count` vượt ngưỡng (`PostgresTelephonyDispatchStore`). `W-0041` mới đếm nơi thứ nhất, nên alert
mang nhãn DT-04 lại đọc một metric mà chính chuyển trạng thái DT-04 không bao giờ chạm vào — phát
hiện lúc discovery của `P6-3` và sửa ở đó.

<a id="callback-retry-exhausted"></a>

## 5. Callback retry exhausted — đề xuất (ARCH-06 §4)

| | |
| --- | --- |
| **Alert** | `IvrCallbackRetryExhausted` · `severity: ticket` · `for: 5m` |

**Ticket chứ không phải page.** Outbox đã ngừng retry và dòng dữ liệu đã bền vững, nên xử lý trong
giờ làm việc không mất gì. Phân tầng severity là cách duy nhất giữ cho page còn nghĩa.

<a id="intake-success"></a>

## 6. Intake success — đề xuất

| | |
| --- | --- |
| **Mục tiêu** | tỷ lệ từ chối < **30%** |
| **Alert** | `IvrIntakeRejectionRatioHigh` · `severity: ticket` · `for: 15m` |

Tỷ lệ từ chối tăng kéo dài chỉ **lên phía trên** — hợp đồng đổi hoặc một lô payload sai từ Sales —
chứ không chỉ vào IVR.

## 7. Business outcome rate

**Anchor:** `business-outcome-rate`

`confirm_rate`, `cancel_rate` và `no_answer_rate` của `ARCH-06` §1, đo từ
`ivr_call_results_total` — metric mà `W-0041` khai báo rồi **không vẽ được** vì chưa ai ghi.

Call site: `ResultRepository.NormalizeNextAsync`, **sau commit**. Vị trí đó quan trọng: đếm trước
commit thì mỗi lần transaction rollback sẽ làm counter cao hơn database, và một tỉ lệ có mẫu số lớn
hơn thực tế **đọc như hiệu năng tốt hơn thực tế**.

Nhãn taxonomy là **result type** (DT-02). Cố ý **không** có chiều `is_final` riêng: taxonomy đã phân
biệt `IVR_NO_ANSWER_ATTEMPT` với `IVR_NO_ANSWER_FINAL`, nên một nhãn thứ hai chỉ thêm time series mà
không thêm thông tin.

`clamp_min` ở mẫu số không phải trang trí: chia cho một khoảng nghỉ sẽ render thành vô cực, và vô
cực **đọc như một sự cố** thay vì như sự im lặng.

**Chưa có alert nào trên các tỉ lệ này.** Ngưỡng "confirm_rate thấp bất thường" cần một baseline đo
được từ lưu lượng thật, mà chưa có cuộc gọi thật nào (`W-0008`). Đặt một ngưỡng bây giờ là đặt một
con số sẽ bị tắt trong tuần đầu.

## 8. Attempt–result gap

**Anchor:** `attempt-result-gap`

Hai counter ôm hai đầu đường gọi: `ivr_call_attempts_total` ghi ở
`PostgresSchedulerStore` khi một attempt được lease và dispatch (sau commit), còn
`ivr_call_results_total` ghi khi kết quả được chuẩn hoá.

Khoảng cách kéo dài giữa hai đường nghĩa là attempt **rời scheduler mà không quay lại thành kết
quả** — dispatch chạy trong khi normalization tắc. **Không counter đơn lẻ nào cho thấy điều đó**:
attempts vẫn tăng đều, results vẫn tăng đều, chỉ có tỉ lệ giữa chúng là sai.

`is_counted_customer_attempt` ở call site attempt luôn là `false`, và đó không phải lỗi: attempt chỉ
trở thành counted khi kết quả chuẩn hoá (DT-02). Gắn nhãn theo **trạng thái tại thời điểm dispatch**
chứ không theo dự đoán là điều giữ cho hai counter trung thực về thứ mà mỗi thời điểm thực sự biết.

<a id="capacity-deadline-missed"></a>

## 9. Missed confirmation deadline — đề xuất (`ARCH-06` §1)

**Alert:** `IvrConfirmationDeadlineMissed` · **Anchor:** `capacity-deadline-missed`

`missed_deadline_count` của `ARCH-06` §1, đo từ `ivr_missed_deadline_total`. Call site:
`PostgresSchedulerStore.CloseMissedDeadlinesAsync`, **sau commit**, **một lần cho mỗi job bị đóng**
— không phải một lần cho mỗi lượt quét. Lượt quét chạy mỗi vòng scheduler và hầu như luôn không tìm
thấy gì; một counter nhích lên ở những lượt rỗng sẽ làm một hệ đang rảnh trông như một hệ đang hỏng.

### Vì sao ngưỡng là **không**, và vì sao đó là suy ra chứ không phải chọn

Mô hình dung lượng (`W-0054`) nói pool prod đang ship **phủ được đỉnh mô hình**. Dưới chính giả định
của nó, **không cửa sổ xác nhận nào đóng lại mà chưa gọi**. Nên một lần trượt không báo "thiếu kênh"
— nó **bác bỏ một giả định của mô hình**.

Điều này quyết định cả runbook: người trực được gửi tới **hiệu chỉnh lại** (`W-0008`, thời lượng
cuộc gọi đo được), **không** tới một đơn mua hàng. Mô hình `UNCALIBRATED` không đủ tư cách biện minh
cho một quyết định mua.

Mô hình cũng **không thể** cho một ngưỡng khác không: nó không mô hình hoá hàng đợi và không mô hình
hoá lỗi kênh (DT-04), nên nó không biết mức trượt nào là "chấp nhận được". Số duy nhất nó nói được
là số không.

Một luật không-khoan-nhượng chỉ trung thực **khi tiền đề còn đúng**. Nếu pool ship tụt xuống dưới
đỉnh mô hình thì chính pool đó bảo đảm sẽ có trượt, luật thành nhiễu **do cấu tạo**. `CAP-ALERT-04`
khẳng định cả hai cùng lúc, nên luật không sống lâu hơn lý do của nó.

### Ba chi tiết kỹ thuật

| Chi tiết | Cách sai nếu làm khác |
| --- | --- |
| `increase(...[15m])`, không đọc thẳng counter | counter đơn điệu so với 0 sẽ **nổ mãi mãi** sau lần nổ đầu, tới khi tiến trình khởi động lại. Ca thứ ba trong file promtool test tồn tại chỉ để chứng minh nó **tắt** |
| `ticket`, không `page` | job đã đóng, capacity incident đã bền, kết quả mang `REVALIDATE_AND_HOLD_ADMIN_REVIEW` — **đã có người sở hữu đơn đó**. Thứ còn thiếu là có ai nhận ra quy luật, mà đó là một hàng đợi chứ không phải một cái pager |
| đếm cái **đã xảy ra**, không đếm cái **dự báo** | `SchedulerCapacityPlan.MissedDeadlineCount` là **dự báo**, tính lại ở mọi lượt đánh giá eligibility. Cùng một job đang chờ xuất hiện trong hàng chục dự báo; cộng nó vào counter là đếm **một đơn hàng chục lần** |

### Lỗ hổng mà call site này vá lại

`CloseMissedDeadlinesAsync` là **nhánh duy nhất** một job đạt tới kết quả FINAL mà **không đi qua
normalization** — scheduler tự ghi dòng `IVR_CAPACITY_EXCEPTION`. Trước slice này, mọi lần trượt
deadline **vắng mặt** khỏi `ivr_call_results_total`.

Hệ quả: `confirm_rate` ở §7 có mẫu số **bỏ sót đúng phần thất bại**, nên đọc **cao hơn sự thật** —
và khoảng lệch **lớn nhất đúng lúc dung lượng tệ nhất**. Nên call site ghi **cả hai** instrument.

## 10. Cái này KHÔNG đo được, và tại sao

- ~~**Chưa có metric nào rời khỏi tiến trình.**~~ **Đã đóng ở mức code + local runtime bởi
  `W-0139`**: API và Worker cùng xuất trace/metric/log qua OTLP tới LGTM local; proof một MOCK task
  thấy đủ bốn nhóm metric và đủ năm stage span. Đây là
  `B06_CODE_AND_LOCAL_RUNTIME_PASS`, **không phải staging/production evidence**. Endpoint,
  credential, retention, access policy, screenshot/query staging và alert fire/recovery thật vẫn
  thuộc Platform/`W-0063`; do đó B-06 chưa đóng và dashboard/alert chưa được gọi là production-ready.
  `observability-staging-evidence.mjs` cùng manual CI job đã chuẩn hóa cách thu các bằng chứng này,
  nhưng contract self-test của verifier không thay thế một lần query staging thật.
- ~~**`ivr_call_attempts_total` và `ivr_call_results_total` chưa có call site.**~~ **Đã đóng
  `2026-08-19`** — xem §7 và §8. `confirm_rate`, `cancel_rate`, `no_answer_rate` (`ARCH-06` §1) giờ
  đo được.
- ~~**`missed_deadline_count` chưa có instrument.**~~ **Đã đóng `2026-08-19`** — xem §9.
- **`cost_per_confirmed_order`** (`ARCH-06` §1) vẫn **không có instrument**, và sẽ không có
  cho tới khi có báo giá. **Mẫu số đã đo được** (`analytics.agg_kpi_daily.confirmed_count`,
  `W-0055`); **tử số thì không** — cả 6 dòng đầu vào ở `docs/cost-model.md` §3 đều còn trống
  (`W-0008`). Một metric tên là "chi phí" mà không có chi phí thì tệ hơn không có metric.
  `CAP-ALERT-04` khẳng định lý do đó **vẫn còn đúng**: dòng đầu tiên được điền vào sẽ làm cổng
  đỏ, kèm chỉ dẫn dựng metric — nên lời bào chữa này không sống lâu hơn được sự thật của nó.
- **Chưa có burn-rate panel nhiều cửa sổ.** Ngân sách lỗi ở trên là định nghĩa, chưa phải thứ đang
  được đốt và vẽ — cần dữ liệu production thật mới hiệu chỉnh được.
- **Ngưỡng đánh dấu "đề xuất" chưa được chủ sở hữu phê duyệt.** Chúng dựa trên suy luận, không dựa
  trên baseline đo được.

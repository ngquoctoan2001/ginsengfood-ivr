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
| channel failure/quarantine transition | `ivr_channel_quarantines_total` | `PostgresSchedulerStore`, `PostgresTelephonyDispatchStore` |
| dispatch backlog wait | `ivr_call_queue_oldest_due_age_seconds` | `SchedulerQueueBacklogSampler`, gọi từ `SchedulerJobHost` |
| analytics ETL run outcome | `ivr_analytics_etl_runs_total` | `AnalyticsEtlJob.RunAsync` |

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

**Tốc độ đốt ngân sách** (panel #13, `W-0041`): tỉ lệ lần gửi vượt 5s chia cho 1% mà ngân sách cho
phép, trên ba cửa sổ 1h/6h/3d. `1` là đốt hết ngân sách 30 ngày trong đúng 30 ngày; `14.4` là hết trong
khoảng hai ngày. Biểu thức đọc thẳng bucket `le="5"`, nên biên bucket của histogram được **ghim** trong
`IvrTelemetry` (`0.05 … 3, 4, 5, 7.5 … 60`) thay vì thừa hưởng mặc định của SDK: mặc định đó vẽ cho
mili-giây, dồn cả khoảng 0–5s vào một bucket, và nếu nó đổi thì chính biên của mục tiêu biến mất khỏi
dữ liệu. Chưa có alert burn-rate — xem §10.

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
| *auto-disable* — lỗi thứ ba trong cửa sổ 10 phút thì khoá kênh | **trong code**, policy dùng chung bởi `PostgresTelephonyDispatchStore` và `PostgresSchedulerStore` | **từng kênh** |
| *+ alert* — báo cho vận hành | luật Prometheus ở đây | **toàn đội**, ≥3 failure/quarantine transition trong 10 phút |

Luật per-kênh không phải một alert và không thể là alert: nó phải chạy đồng bộ tại thời điểm sự
kiện để kênh hỏng không được cấp phát tiếp. Lỗi đầu mở cửa sổ; lỗi thứ ba tại hoặc trước mốc 10 phút
chuyển kênh sang `HEALTH_FAILED`. Một kết quả healthy xóa `fail_count` và mốc cửa sổ; lỗi tiếp theo
sau hơn 10 phút mở cửa sổ mới với `fail_count=1`. Alert ở đây là nửa "báo cho người", và ngưỡng
toàn đội của nó là **đề xuất**, không phải counter per-kênh DT-04.

Cả **hai** nơi ghi nhận lỗi kênh đều dùng cùng policy/cửa sổ: lease hết hạn
(`PostgresSchedulerStore`) và provider báo kênh unhealthy (`PostgresTelephonyDispatchStore`). Cả hai
đều tăng `ivr_channel_quarantines_total`; `W-0144` bổ sung mốc cửa sổ bền vững trong DB để restart
process không làm mất semantics. `W-0041` trước đó mới đếm nơi thứ nhất; `P6-3` đã nối metric cho nơi
thứ hai nhưng vẫn chỉ đếm lỗi liên tiếp không giới hạn thời gian — lỗ hổng đó được đóng ở `W-0144`.

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

*Thêm 26/09 (`Q-22.2`):* job chưa quay lần nào mà giờ gọi chỉ còn mở chưa tới một cuộc gọi
(`ExpectedCallDurationSeconds`) trong khoảng từ lúc tới (hoặc `T0`) tới hạn cửa sổ, và giờ gọi thật sự đóng
trong khoảng đó, được đếm với `ivr_reason_code=CALLING_HOURS_CLOSED_BEFORE_DISPATCH`, không mở sự cố dung
lượng. Đó là đơn tới ở những giây cuối trước `21:08` hoặc sau đó, không phải thiếu kênh. Kết quả gửi Module 3
không đổi (`IVR_CAPACITY_EXCEPTION`). ~~Luật `IvrConfirmationDeadlineMissed` hiện vẫn cộng mọi lý do; lọc theo lý
do là việc của `K-61`.~~ Đã lọc, xem ngay dưới.

*Sửa 26/09 (`K-61`):* luật chỉ đọc lý do thiếu kênh:
`increase(ivr_missed_deadline_total{ivr_reason_code="NO_DISPATCH_BEFORE_DEADLINE"}[15m])`. Sweep đóng job với một
trong ba lý do, và lập luận cho ngưỡng **không** ở dưới chỉ nói về một trong số đó. Trước `K-61` luật cộng cả ba,
nên sau `K-52` và `K-54` một đơn bị giữ chờ duyệt, hay chưa từng được eligibility trả lời, cũng gửi người trực đi
hiệu chỉnh mô hình dung lượng, vì một đơn mà không kênh nào cứu được.

| `ivr_reason_code` | Job nào | Luật đọc nó |
| --- | --- | --- |
| `NO_DISPATCH_BEFORE_DEADLINE` | đã xếp hàng (`READY_FOR_SCHEDULER`), chưa quay lần nào, giờ gọi không cạn; hoặc eligibility giữ vì biết sẽ không có kênh (`CAPACITY_HELD`, `K-52`) | luật này |
| `WINDOW_EXPIRED_BEFORE_FINAL_RESULT` | đã quay mà hết cửa sổ trước kết quả cuối; dry run; eligibility giữ chờ duyệt; eligibility chưa trả lời (`K-54`) | `IvrConfirmationWindowsExpiringUndialled` (§9a), chỉ khi cửa sổ cứ đóng mà không quay ai |
| `CALLING_HOURS_CLOSED_BEFORE_DISPATCH` | hết giờ gọi trước khi kịp quay (`Q-22.2`) | không luật nào: đó là ngày gọi kết thúc, không phải lỗi |

So bằng (`=`), không loại trừ (`!=`): một lý do thêm sau này đứng ngoài luật dung lượng cho tới khi có người quyết
định nó thuộc về đâu. `CAP-ALERT-04` đọc lại các lý do từ `PostgresSchedulerStore.CloseMissedDeadlinesAsync` và nhãn
từ `TelemetryTags.ReasonCode`, rồi đòi mỗi lý do có đúng một chỗ trong bảng trên, nên lý do thứ tư làm cổng đỏ thay
vì lặng lẽ không ai đọc.

### Vì sao ngưỡng là **không**, và vì sao đó là suy ra chứ không phải chọn

Mô hình dung lượng (`W-0054`) nói pool prod đang ship **phủ được đỉnh mô hình**. Dưới chính giả định
của nó, **không cửa sổ xác nhận nào đóng lại mà chưa gọi vì thiếu kênh**. Nên một lần trượt không báo "thiếu kênh"
— nó **bác bỏ một giả định của mô hình**. Mô hình chỉ nói về kênh; đơn bị giữ, đơn chưa được duyệt, đơn hết giờ gọi
nằm ngoài nó, và đó là lý do luật chỉ đọc lý do thiếu kênh.

Điều này quyết định cả runbook: người trực được gửi tới **hiệu chỉnh lại** (`W-0008`, thời lượng
cuộc gọi đo được), **không** tới một đơn mua hàng. Mô hình `UNCALIBRATED` không đủ tư cách biện minh
cho một quyết định mua.

Mô hình cũng **không thể** cho một ngưỡng khác không: nó không mô hình hoá hàng đợi và không mô hình
hoá lỗi kênh (DT-04), nên nó không biết mức trượt nào là "chấp nhận được". Số duy nhất nó nói được
là số không.

Một luật không-khoan-nhượng chỉ trung thực **khi tiền đề còn đúng**. Nếu pool ship tụt xuống dưới
đỉnh mô hình thì chính pool đó bảo đảm sẽ có trượt, luật thành nhiễu **do cấu tạo**. `CAP-ALERT-04`
khẳng định cả hai cùng lúc, nên luật không sống lâu hơn lý do của nó.

### Bốn chi tiết kỹ thuật

| Chi tiết | Cách sai nếu làm khác |
| --- | --- |
| `increase(...[15m])`, không đọc thẳng counter | counter đơn điệu so với 0 sẽ **nổ mãi mãi** sau lần nổ đầu, tới khi tiến trình khởi động lại. Ca thứ ba trong file promtool test tồn tại chỉ để chứng minh nó **tắt** |
| chỉ `ivr_reason_code="NO_DISPATCH_BEFORE_DEADLINE"` (`K-61`) | cộng mọi lý do thì một đơn giữ chờ duyệt, chưa được eligibility trả lời hay hết giờ gọi cũng đọc thành "mô hình sai". Ca thứ tư trong file promtool test cho luật hai lý do kia và đòi nó **im** |
| `ticket`, không `page` | job đã đóng, capacity incident đã bền, kết quả mang `REVALIDATE_AND_HOLD_ADMIN_REVIEW` — **đã có người sở hữu đơn đó**. Thứ còn thiếu là có ai nhận ra quy luật, mà đó là một hàng đợi chứ không phải một cái pager |
| đếm cái **đã xảy ra**, không đếm cái **dự báo** | `SchedulerCapacityPlan.MissedDeadlineCount` là **dự báo**, tính lại ở mọi lượt đánh giá eligibility. Cùng một job đang chờ xuất hiện trong hàng chục dự báo; cộng nó vào counter là đếm **một đơn hàng chục lần** |

### Lỗ hổng mà call site này vá lại

`CloseMissedDeadlinesAsync` là **nhánh duy nhất** một job đạt tới kết quả FINAL mà **không đi qua
normalization** — scheduler tự ghi dòng `IVR_CAPACITY_EXCEPTION`. Trước slice này, mọi lần trượt
deadline **vắng mặt** khỏi `ivr_call_results_total`.

Hệ quả: `confirm_rate` ở §7 có mẫu số **bỏ sót đúng phần thất bại**, nên đọc **cao hơn sự thật** —
và khoảng lệch **lớn nhất đúng lúc dung lượng tệ nhất**. Nên call site ghi **cả hai** instrument.

<a id="confirmation-windows-expiring-undialled"></a>

## 9a. Cửa sổ cứ đóng mà không quay ai — đề xuất (`K-61`)

| | |
| --- | --- |
| **Đo bằng** | `increase(ivr_missed_deadline_total{ivr_reason_code="WINDOW_EXPIRED_BEFORE_FINAL_RESULT"}[15m])`, `unless` có attempt trong `30m` (`ivr_call_attempts_total`) |
| **Alert** | `IvrConfirmationWindowsExpiringUndialled` · `severity: ticket` · `for: 20m` |

Phần còn lại của counter ở §9. Phần lớn job hết cửa sổ với `WINDOW_EXPIRED_BEFORE_FINAL_RESULT` không cần ai: khách
đã được gọi mà chưa kịp có kết quả cuối là một kết quả Module 3 nhận (`IVR_CONFIRMATION_WINDOW_EXPIRED`); dry run
không cần kênh; đơn eligibility giữ chờ duyệt đã được đếm vào `ivr_fail_closed_total` **lúc bị giữ**, và
`IvrDownstreamFailClosedSpike` (§3) page khi tỉ lệ giữ vượt 20%, lúc cửa sổ còn mở. Chưa có baseline để đặt ngưỡng
lên số lần hết cửa sổ, nên luật không đặt.

Phần cần người là đơn **eligibility không bao giờ trả lời** (`K-54`): vòng eligibility của worker lượt nào cũng
lỗi, hoặc bị tắt trong cấu hình. Không tín hiệu nào khác thấy nó:

| Tín hiệu | Vì sao im |
| --- | --- |
| `IvrDownstreamFailClosedSpike` (§3) | không đánh giá thì không fail closed; intake vẫn đếm quyết định, nên tỉ lệ còn **giảm** |
| liveness probe của worker | chỉ restart vòng **đứng**. Vòng chạy mà lượt nào cũng ném lỗi vẫn là `live`, cố ý (`WorkerLiveness`): restart không sửa được API không tới được hay token nội bộ bị từ chối. Vòng tắt trong cấu hình chỉ hiện `enabled: false` |
| `IvrCallQueueBacklogAging` (§9b) | đơn chưa được duyệt không có attempt nào để chờ; gauge đứng ở `0` |

Trước `K-61`, thứ duy nhất nổ cho ca này là `IvrConfirmationDeadlineMissed`, với runbook sai. Nhãn của metric không
tách được đơn chưa được trả lời khỏi phần còn lại của lý do, nhưng hình dạng thì tách được: khi eligibility ngừng,
không job mới nào tới dialler, nên sau đuôi các job đã duyệt, **cửa sổ cứ đóng mà không quay ai**.

| Chi tiết | Vì sao |
| --- | --- |
| attempt nhìn `30m`, hết hạn nhìn `15m` | attempt của một job đã quay nằm trong cửa sổ của nó, tức không sớm hơn `15` phút (24/7, cửa sổ dài nhất) trước lúc hết hạn. `15 + 15` giữ attempt đó trong khung suốt lúc lần hết hạn còn được đếm, nên **job đã quay không bao giờ tự bật luật**; chỉ job chưa quay mới bật được. `CAP-ALERT-04` giữ bất đẳng thức này theo cửa sổ dài nhất của mô hình |
| `for: 20m`, dài hơn khung `15m` | một lần hết hạn nằm trong khung chưa tới `15` phút, không bao giờ đủ `20`: một đơn giữ chờ duyệt hết hạn giữa nửa giờ vắng khách không mở ticket. Cửa sổ phải **cứ** đóng. Vòng eligibility ngừng lúc `t` thì ticket mở vào khoảng `t + 1h`: đuôi attempt của các job đã duyệt, `30m` để attempt cuối rời khung, rồi `20m` |
| `sum`, không `by (ivr_program)` | `ivr_call_attempts_total` không mang nhãn chương trình, và vòng eligibility là một cho mọi chương trình |
| không đọc `CALLING_HOURS_CLOSED_BEFORE_DISPATCH` | lý do đó nảy ra đúng lúc giờ gọi đóng, khi không ai được quay theo thiết kế; đọc nó thì luật nổ mỗi tối |
| ticket, không page | như §9: job đã đóng, kết quả đã vào outbox gửi Module 3, và đơn chưa gặp được khách mang `REVALIDATE_AND_HOLD_ADMIN_REVIEW` |

**Người trực kiểm tra, theo thứ tự:** body `/healthz` của worker, vòng `eligibility`: `enabled`, `consecutive_faults`,
`last_fault_kind`; log `2360` (tắt trong cấu hình) và `2361` (lượt lỗi). Vòng gọi API của chính release bằng token
nội bộ, nên `HttpRequestException` liên tục thường là API không tới được, token bị từ chối hoặc API trả lỗi. Nếu
`IvrDownstreamFailClosedSpike` đang nổ, eligibility **có** trả lời: nó đang giữ mọi đơn, và đó là việc của §3. Nếu
`IvrCallQueueBacklogAging` hay `IvrConfirmationDeadlineMissed` cũng đang nổ, đơn đã tới được dialler: chỗ hỏng là
dialler (§9b) hoặc dung lượng (§9), không phải eligibility. **Không** hiệu chỉnh mô hình dung lượng và **không** thêm
kênh vì luật này: không đơn nào nó đếm từng xin kênh.

<a id="call-queue-backlog-aging"></a>

## 9b. Hàng đợi quay số bị dồn — đề xuất (`W-0041` §11)

| | |
| --- | --- |
| **Ngưỡng** | cuộc gọi đến hạn lâu nhất đã chờ > **60s**, giữ liên tục **10 phút** |
| **Đo bằng** | `max(ivr_call_queue_oldest_due_age_seconds)` |
| **Alert** | `IvrCallQueueBacklogAging` · `severity: page` · `for: 10m` |

Luật duy nhất đọc một **gauge**. Mọi tín hiệu khác của scheduler đếm sự kiện, mà một dialler đã dừng
thì **không sinh sự kiện nào**: không attempt, không result, không miss cho tới khi cửa sổ xác nhận
đầu tiên đóng. Thời gian chờ của cuộc gọi đến hạn lâu nhất là con số **còn nhúc nhích khi mọi thứ
khác đứng yên**.

Call site: `SchedulerQueueBacklogSampler`, lấy mẫu mỗi **15s** từ vòng lặp scheduler của worker
(`SchedulerJobHost`), với **đúng các điều kiện claim** của `PostgresSchedulerStore` — đơn đã thu hồi,
hàng đợi bị admin tạm dừng (`ADMIN_QUEUE_PAUSE`), attempt đang đổ chuông đều **không** phải "đang
chờ". Điều kiện được chép chứ không dùng chung, vì claim nằm trong lớp bị phụ thuộc nhiều nhất của
scheduler; `IT-OBS-BACKLOG-15` giữ hai bên khớp nhau bằng cách claim đúng thứ gauge báo rồi kiểm tra
gauge về **0**.

| Chi tiết | Cách sai nếu làm khác |
| --- | --- |
| thời gian chờ tính từ mốc **muộn hơn** giữa lúc đến hạn và lúc cửa sổ gọi mở hôm nay (`OD-V1-16`); ngoài cửa sổ gauge = 0 | attempt đến hạn lúc 07:55 bị **luật giờ** giữ, không phải dialler. Tính từ 07:55 thì sáng nào luật cũng nổ chỉ vì đêm qua đã xảy ra |
| ghi **0** khi không có gì chờ, không bỏ qua | gauge báo giá trị cuối cùng nó nhận; im lặng khi hàng đợi rỗng sẽ tiếp tục báo backlog cũ |
| `max`, không `sum` | mọi worker lấy mẫu **cùng một database** và báo cùng một con số; `sum` nhân thời gian chờ với số replica. Ca thứ ba của `ivr-slo.backlog.test.yml` ghim điều đó |
| lấy mẫu cả khi lượt scheduler **ném lỗi** | lượt nào cũng lỗi (một lịch attempt không đọc được là đủ) chính là dialler có hàng đợi đang già đi; chỉ lấy mẫu sau lượt sạch thì gauge đóng băng ở giá trị khoẻ mạnh cuối cùng |
| **không** lấy mẫu khi scheduler bị tắt; lỗi lấy mẫu chỉ ghi log (`2340`), không làm hỏng lượt | tắt là một quyết định, hàng đợi già đi dưới quyết định đó không phải dialler tụt lại; một metric làm hỏng lượt sẽ đẩy vòng lặp vào backoff và làm chậm chính việc quay số nó đo |

**Vì sao 60s.** Cửa sổ xác nhận là **5 phút** (Giờ Vàng) và **15 phút** (24/7), và attempt rời khỏi
tập claim khi cửa sổ của nó đóng — nên gauge **không bao giờ** chạm một ngưỡng tính bằng chục phút.
Một phút là một phần năm cửa sổ Giờ Vàng. Giữ liên tục 10 phút nghĩa là khách **đang bị gọi trễ,
liên tục** — thứ mà `IvrConfirmationDeadlineMissed` (§9) chỉ thấy khi đơn **đã mất**.

**Page chứ không ticket**, ngược với §9: luật deadline nổ khi job đã đóng và đã có người sở hữu; luật
này nổ khi các đơn trong hàng đợi **vẫn còn cứu được**. Ngưỡng 60s là **đề xuất**, chưa có baseline
production.

**Người trực kiểm tra:** log của `SchedulerJobHost` về cửa sổ gọi, shedding và controller (`2312`,
`2317`, `2319`); kênh bị cách ly (§4); `MaxConcurrentDispatches`/`MaxCallStartsPerSecond` so với tải.
**Không** nâng ngưỡng để "tắt alert": cửa sổ 5 phút không co giãn theo ngưỡng.

<a id="analytics-reconcile-mismatch"></a>

## 9c. Analytics reconcile `MISMATCH` — đề xuất (`W-0055`)

| | |
| --- | --- |
| **Đo bằng** | `increase(ivr_analytics_etl_runs_total{ivr_outcome="MISMATCH"}[30m])` |
| **Alert** | `IvrAnalyticsReconcileMismatch` · `severity: ticket` · `for: 5m` |

Mỗi lượt ETL được đếm **sau khi** checkpoint mang verdict đã ghi xong, với `ivr.outcome` là verdict
đó (`COMPLETE`/`BACKLOG`/`MISMATCH`) hoặc `FAILED` khi lượt ném lỗi trước khi có verdict. `FAILED`
**không** được thêm vào `AnalyticsReconcileStatus`: tập đó được lưu và API trả ra, còn lượt lỗi không
ghi checkpoint nào. Huỷ lúc shutdown **không** tính là lỗi. Call site là `AnalyticsEtlJob.RunAsync`,
không phải `AnalyticsEtlJobHost`: host là `internal` của worker, và đếm ở job thì bộ integration
test (`BI-ALERT-05`) chạm được tới đúng đường đếm.

`MISMATCH` nghĩa là warehouse và nguồn **lệch nhau** mà không có backlog hay privacy rejection nào giải
thích — KPI dựng trên đó **trông y như KPI đúng**. Verdict là grain tệ hơn trong hai grain:

| Grain | `MISMATCH` khi |
| --- | --- |
| result | số fact > nguồn − orphan − số dòng privacy filter từ chối **ở lượt này** |
| job | số job fact > số job nguồn − số job bị từ chối, **hoặc** còn job fact lệch `eligible`/`closed`/số attempt counted với job của nó **sau** refresh (`BI-DRIFT-06`) |

Checkpoint mà API đọc chỉ mang số đếm **grain result**, nên `MISMATCH` do grain job hiện ra với hai số
bằng nhau. Log worker event `1204` ghi đủ cả hai grain — đọc ở **stdout của pod worker**: đường OTLP
chỉ giữ các trường có trong allowlist của `PiiSafeLogRecordProcessor`, nên các số đếm này không tới
Loki, giống mọi số đếm khác của log `1201`.

Hai lỗi cũ sẽ làm luật này nổ sai, nên được sửa cùng lúc:

- Reconcile cộng tổng rejection **tích luỹ** của checkpoint vào lượt hiện tại. Dòng bị từ chối không
  bao giờ có fact, nên anti-join trả nó về và filter từ chối lại **mỗi lượt**: cộng dồn là đếm một
  dòng một lần mỗi lượt, và từ lượt thứ hai **một** dòng bị từ chối thành `MISMATCH` vĩnh viễn. Giờ
  dùng số của chính lượt đó (`BI-QUALITY-04`).
- Refresh job fact chỉ đọc lại fact **đang mở**, dựa trên giả định job đã `closed` không đổi nữa.
  Không gì ép giả định đó: job đã đóng mà có thêm attempt counted giữ số cũ mãi mãi, trong khi số dòng
  vẫn khớp. Giờ refresh chọn theo **so sánh với nguồn**, và reconcile đọc **cùng snapshot**
  `REPEATABLE READ` với refresh — đọc ở snapshot sau thì mọi attempt được normalize trong vài mili-giây
  giữa hai lần đọc đều thành `MISMATCH` giả vào mỗi buổi chiều đông khách.

**Người trực:** đọc log `1204` (stdout của worker) để biết grain nào. Grain result với fact > nguồn: xem có retention run
vừa chạy không — orphan tồn tại giữa lúc purge dữ liệu vận hành và lúc hook analytics chạy là **tạm
thời**, lượt kế tiếp sẽ hết. Grain job với `drifted` > 0: refresh đã không làm đúng việc của nó — đó là
bug, không phải dữ liệu. **Không** xoá checkpoint để "reset": checkpoint không phải input của tính đúng
(anti-join mới là), xoá nó không sửa được gì.

<a id="analytics-etl-not-completing"></a>

## 9d. Analytics ETL chạy mà không hoàn tất — đề xuất (`W-0055`)

| | |
| --- | --- |
| **Đo bằng** | có lượt chạy trong 1h, `unless` có lượt `COMPLETE` trong 1h |
| **Alert** | `IvrAnalyticsEtlNotCompleting` · `severity: ticket` · `for: 15m` |

`BACKLOG` kéo dài một giờ, `MISMATCH` ở mọi lượt, hay `FAILED` ở mọi lượt đều dẫn về đây. Console
reporting quảng cáo độ tươi **15 phút**; một giờ không hoàn tất là đã vượt ngân sách đó bốn lần.

Pipeline **không chạy** thì luật này **không** nổ: không có lượt nào để đếm. Trường hợp đó thuộc về
liveness của worker (`IT-WORKER-LIVENESS-12`), còn ETL bị tắt là một quyết định.

`for: 15m` vì một chi tiết của `increase()`: worker khởi động lại tạo **series mới**, và lần tăng đầu
tiên của một series mới không nhìn thấy được cho tới khi lượt kế tiếp cộng thêm. Không có `for`, chuỗi
`FAILED` → `COMPLETE` → `FAILED` ngay sau restart sẽ mở ticket cho một pipeline vừa hoàn tất. Cùng lý
do đó, một `MISMATCH` **đơn lẻ** ngay sau restart có thể không mở ticket ở §9c; `MISMATCH` thật thường
lặp lại ở mọi lượt và được thấy từ lượt thứ hai.

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
- **Eligibility hỏng một phần không có alert** (`K-61`). §9a chỉ thấy khi **không quay được ai**: một vòng
  eligibility còn trả lời được một phần đơn vẫn giữ dialler chạy, và counter không mang nhãn nào nói đơn hết hạn khi
  eligibility chưa trả lời. Tách được thì cần một lý do riêng trên counter, như `Q-22.2` đã làm cho giờ gọi. Tới lúc
  đó, phân biệt nằm ở dòng audit của sweep (`eligibility_decision`).
- **`cost_per_confirmed_order`** (`ARCH-06` §1) vẫn **không có instrument**, và sẽ không có
  cho tới khi có báo giá. **Mẫu số đã đo được** (`analytics.agg_kpi_daily.confirmed_count`,
  `W-0055`); **tử số thì không** — cả 6 dòng đầu vào ở `docs/cost-model.md` §3 đều còn trống
  (`W-0008`). Một metric tên là "chi phí" mà không có chi phí thì tệ hơn không có metric.
  `CAP-ALERT-04` khẳng định lý do đó **vẫn còn đúng**: dòng đầu tiên được điền vào sẽ làm cổng
  đỏ, kèm chỉ dẫn dựng metric — nên lời bào chữa này không sống lâu hơn được sự thật của nó.
- ~~**Chưa có burn-rate panel nhiều cửa sổ.**~~ **Đã có panel** (`W-0041`, panel #13): ngân sách
  D-04 được vẽ như tốc độ đốt trên 1h/6h/3d — xem §2. **Alert burn-rate thì vẫn chưa có**: ngưỡng
  multi-window (bao nhiêu lần đốt trên cửa sổ nào thì page) cần dữ liệu production thật mới hiệu chỉnh
  được, và một ngưỡng đoán bây giờ là một ngưỡng sẽ bị tắt trong tuần đầu.
- **Panel integration-status (`P6-2` §6.4) chưa làm, và cố ý không làm.** Nó cần một nguồn **probe
  dependency thật**, mà hiện không có: `AdminConfigReadService` trả cứng
  `dependency_probing_available=false`. Một panel vẽ trạng thái tích hợp từ nguồn không probe gì sẽ
  là vạch phẳng đọc như "mọi tích hợp đều khoẻ" — đúng kiểu nói dối §1 cấm.
- **Ngưỡng đánh dấu "đề xuất" chưa được chủ sở hữu phê duyệt.** Chúng dựa trên suy luận, không dựa
  trên baseline đo được.

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

`TASK_SKIPPED_TRUSTED_CUSTOMER` **không** được tính là fail-closed: đó là chính sách chọn không gọi,
không phải hệ thống không chứng minh được an toàn. Gộp hai thứ sẽ làm một luật trust đang chạy đúng
trông như một sự cố downstream.

**Ngưỡng 20% là đề xuất**, chưa có baseline production để hiệu chỉnh.

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

## 7. Cái này KHÔNG đo được, và tại sao

- **Chưa có metric nào rời khỏi tiến trình.** Không có exporter OTLP; collector/backend là `W-0063`,
  vẫn `BLOCKED_EXTERNAL`. Instrument dùng `Meter` của BCL nên gắn exporter sau **không phải sửa call
  site nào**, nhưng cho tới lúc đó Prometheus không scrape được gì. Dashboard và alert ở đây là
  **artifact as-code đã được kiểm bằng bộ đánh giá luật thật**, không phải thứ đang chạy production.
- **`ivr_call_attempts_total` và `ivr_call_results_total` chưa có call site.** Chúng được khai báo ở
  `P6-1` nhưng chưa ai ghi. Vì vậy **không có panel và không có alert nào dùng chúng** — và
  `UT-DASH-PII-04` sẽ đỏ nếu ai đó thêm vào. Hệ quả thật: `confirm_rate`, `cancel_rate`,
  `no_answer_rate` trong `ARCH-06` §1 **chưa đo được**.
- **`missed_deadline_count` và `cost_per_confirmed_order`** (`ARCH-06` §1) chưa có instrument nào.
- **Chưa có burn-rate panel nhiều cửa sổ.** Ngân sách lỗi ở trên là định nghĩa, chưa phải thứ đang
  được đốt và vẽ — cần dữ liệu production thật mới hiệu chỉnh được.
- **Ngưỡng đánh dấu "đề xuất" chưa được chủ sở hữu phê duyệt.** Chúng dựa trên suy luận, không dựa
  trên baseline đo được.

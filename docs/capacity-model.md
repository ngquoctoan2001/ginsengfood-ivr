# IVR Capacity and Cost Model

Status: `ENGINEERING_MODEL` · Values are configuration defaults, not production sizing approval.

## TTS synthesis boundary (W-0066)

P2-9 measures provider demand without selecting a vendor. The default MOCK budget is:

| Input | Default | Enforcement |
| --- | ---: | --- |
| Maximum characters per synthesis request | 1,200 | request rejected before provider call |
| Maximum provider requests per process/minute | 60 | fixed-window fail-closed budget |
| Maximum provider characters per process/minute | 72,000 | fixed-window fail-closed budget |
| Maximum rendered audio duration | 120 seconds | provider result rejected if it exceeds the bound |
| Provider timeout | 5 seconds | becomes `IVR_TECHNICAL_EXCEPTION`, never no-answer |
| Audio cache maximum TTL | 900 seconds | additionally capped by confirmation deadline and speech retention |

The deterministic MOCK adapter models mono 8 kHz, 16-bit linear PCM metadata
(`audio/L16`). At that format, uncompressed media is approximately 16 kB/second,
960 kB/minute, before gateway/container overhead. It does not open a network socket
and does not represent a supported real-gateway codec; W-0008/P8-1 must measure the
selected hardware/vendor path.

Runtime metrics expose only aggregates:

- `ivr_tts_provider_requests_total`
- `ivr_tts_characters_total`
- `ivr_tts_cache_operations_total{result=hit|miss}`
- `ivr_tts_cache_purged_total`

The cache identity is SHA-256 over `(script_template_id, script_version,
hash(privacy_safe_order_summary), voice_id, locale)`. It contains no raw summary,
phone, address or rendered text. A restart clears the process-local cache, and the
P1-5 retention job invokes its purge hook; dry-run reports without mutation.

## Cost formula pending OD-V1-19

No currency estimate is asserted because no TTS vendor or price sheet has been
approved. Once Product, Infra and Privacy/Legal close `OD-V1-19`, use:

```text
billable_characters = provider_characters_after_cache
monthly_tts_cost = billable_characters / vendor_billing_unit
                   * vendor_price_per_billing_unit
```

Sizing inputs still required from the selected vendor/lab:

- billing treatment for punctuation, SSML and pronunciation hints;
- request/concurrency quotas and regional endpoint availability;
- accepted codec/sample rate for the SIM gateway;
- measured cache-hit ratio, p50/p95/p99 synthesis latency and error rate;
- DPA/data residency, encryption and provider content-retention controls;
- Vietnamese product-name, amount, quantity and delivery-area pronunciation acceptance;
- one-SIM lab throughput followed by the future 32-eSIM concurrency/failover model.

Until those inputs exist, this section is a bounded engineering model only;
pronunciation, vendor cost and 32-channel production capacity remain `NOT_RUN`.

---

# SIM pool sizing — `W-0054` · `P10-3`

Trạng thái: **`UNCALIBRATED`** — mô hình chưa bao giờ được đối chiếu với một cuộc gọi thật, vì
**chưa có cuộc gọi thật nào** (`W-0008`). Mô hình: `tools/capacity-sim/capacity-model.mjs`.

## 1. Sai lầm mà mô hình này tồn tại để tránh

**Tính theo trung bình ngày.** Golden Hour dồn toàn bộ cửa sổ attempt vào vài phút sau khi khách
đặt, nên một pool thoải mái với trung bình ngày sẽ **trượt deadline mỗi sáng** và làm hết hạn những
đơn chưa bao giờ không liên lạc được.

`CAP-MODEL-01` khẳng định điều này bằng một đối chứng: cùng khối lượng ngày, trải đều ra thì pool
phải **nhỏ hơn hẳn**. Nếu hai con số bằng nhau, mô hình không nhìn thấy cú dồn mà Golden Hour được
tạo ra từ đó.

## 2. Công thức

```text
attempts       = orders + orders·noAnswerRate            (tới maxAttempts, chỉ counted attempt)
callsPerChannel = floor(window / (callSeconds + cooldown))
channels       = ceil(attempts / callsPerChannel)
```

Ba chi tiết, mỗi cái vì một cách sai:

| Chi tiết | Vì sao |
| --- | --- |
| cooldown **trong mẫu số**, không trừ ở cuối | 5 giây nghỉ sau cuộc gọi 40 giây tốn 11% năng lực kênh **mỗi cuộc**, không phải một lần |
| `ceil`, không `floor` | nửa kênh không nhận được cuộc gọi; làm tròn xuống là số học sinh ra một pool **đúng trên giấy, thiếu khi chạy** |
| cộng các chương trình, không lấy max | đơn 24/7 đặt lúc 9h đang được gọi **trong cùng những phút** với đỉnh Golden Hour; lấy max là giả định chúng thay phiên |

Cuộc gọi dài hơn cửa sổ trả `Infinity` — đó là **lỗi thiết kế**, không phải vấn đề mua sắm, và trả
một con số rất lớn sẽ che nó thành vấn đề mua sắm.

## 3. Kết quả là một **khoảng**, không phải một con số

Mọi đầu vào đều là **giả định chưa ai đo**. Khối lượng đơn là đầu vào của chủ sở hữu chưa tới; thời
lượng cuộc gọi chưa bao giờ được quan sát vì chưa có cuộc gọi nào.

`CAP-SENS-02` quét 27 góc (3 khối lượng × 3 thời lượng × 3 tỉ lệ không nghe máy) và trả về khoảng
kèm **đầu vào đẩy kết quả mạnh nhất** — đó là thứ nên đi đo trước ở lab.

Chạy `node deploy/ci/scripts/capacity-selftest.mjs` để in khoảng hiện tại. Con số **không** được
chép vào đây, vì một con số trong tài liệu sẽ sống lâu hơn giả định sinh ra nó.

## 4. Hiệu chỉnh: chưa có

`P5-3` đo API và scheduler — **chưa bao giờ đo một cuộc quay số**. Nên hiệu chỉnh hiện chỉ có một
hình thức trung thực: khẳng định mô hình **tự khai là chưa hiệu chỉnh** và nêu tên công việc sẽ hiệu
chỉnh nó (`W-0008`). `CAP-CALIB-03` kiểm đúng hai điều đó, và kiểm thêm rằng báo cáo hiệu năng
**không** tuyên bố đã đo thời lượng cuộc gọi.

## 5. Pool đang ship vs mô hình

`CAP-ALERT-04` đọc `worker.hpa.simPoolSize` của cả 4 môi trường và đòi:

- `prod` ≥ pool mà mô hình cần ở đỉnh — nếu không, chart ship sẵn một trần **bảo đảm trượt deadline**;
- thứ tự **không nghịch**: `dev ≤ staging ≤ lab ≤ prod`. Một pool lab lớn hơn prod nghĩa là buổi
  diễn tập to hơn thứ nó diễn tập.

### Alert capacity — có từ `2026-08-19`

`missed_deadline_count` đã có call site (`PostgresSchedulerStore.CloseMissedDeadlinesAsync`, sau
commit), nên luật `IvrConfirmationDeadlineMissed` không còn là một ngưỡng nằm trong tài liệu mà
không có gì phía sau. Chi tiết ở `docs/slo.md` §9.

Chỗ đáng nói là **mô hình quyết định ngưỡng**, chứ không phải người viết luật chọn nó. Mô hình nói
pool prod phủ được đỉnh; dưới giả định của chính nó thì **không lần trượt nào xảy ra**. Nên ngưỡng
là **không**, và một lần trượt đọc là "một giả định của mô hình sai", không phải "thiếu kênh".

Mô hình cũng **không đủ tư cách** cho bất kỳ ngưỡng khác không nào: không có mô hình hàng đợi, không
có mô hình lỗi kênh. `CAP-ALERT-04` đòi đúng ba điều — luật tồn tại, ngưỡng là `> 0`, và biểu thức
đi qua `increase()` (một counter đơn điệu so thẳng với 0 sẽ **nổ mãi mãi** sau lần đầu).

Và nó buộc hai thứ vào nhau: **luật không-khoan-nhượng chỉ trung thực khi pool ship còn phủ được
đỉnh mô hình**. Nếu `simPoolSize` prod tụt xuống dưới, chính pool đó bảo đảm sẽ trượt, luật thành
nhiễu do cấu tạo — và cổng đỏ trước khi điều đó lặng lẽ thành sự thật.

**`cost_per_confirmed_order` thì vẫn không có instrument**, và cổng khẳng định lý do: cả 6 dòng đầu
vào chi phí ở `docs/cost-model.md` §3 còn trống. Điền một dòng vào là cổng đỏ, kèm chỉ dẫn dựng
metric.

## 6. Mô hình này KHÔNG chứng minh

- **Chưa hiệu chỉnh.** Không đầu vào nào được đo. Nó **không được** dùng để quyết định mua hàng cho
  tới khi `W-0008` cho ra một thời lượng cuộc gọi đo được.
- **Không có dự báo khối lượng từ business.** `dailyOrders`, `eligibleRate` và phân bố theo chương
  trình đều là giả định của tôi.
- **Không mô hình hoá lỗi kênh.** DT-04 cách ly kênh sau 3 lần hỏng trong 10 phút; mô hình giả định
  mọi kênh đều khoẻ, nên pool thật cần thêm dự phòng mà **chưa ai định lượng**.
- **Không mô hình hoá hàng đợi.** Nó tính pool để **vừa đủ** trong cửa sổ, không tính độ trễ hàng
  đợi hay phân bố thời gian chờ.
- **Không có báo giá nào.** Xem `docs/cost-model.md`.

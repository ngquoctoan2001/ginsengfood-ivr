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

## 4a. Ba con số chu kỳ cuộc gọi — một nguồn khai báo (`W-0132`)

Thời lượng một cuộc gọi từng sống ở ba nơi độc lập, và **không gate nào so chúng với nhau**:

| Con số | Ở đâu | Nghĩa |
| --- | --- | --- |
| **40s** (+5s cooldown) | `capacity-model.mjs`, `capacity-selftest.mjs` | giả định **channel occupancy** của model; cooldown được cộng riêng |
| **50s** | hàm ý bởi spec §23 `M8-P0-009` | **full channel cycle**; 32 SIM × `floor(300/50)` = `~192` cuộc mỗi window 5 phút |
| **60s** | `SchedulerOptions.ExpectedCallDurationSeconds` | ước lượng **channel occupancy** mặc định của runtime |

`W-0132` gom chúng vào `CALL_DURATION_ASSUMPTIONS` trong `tools/capacity-sim/capacity-model.mjs`
và **không hợp nhất giá trị**. Hợp nhất nghĩa là tuyên bố một thời lượng đã đo, mà chưa có cuộc gọi
nào được quay (`W-0008`). Việc gom lại chỉ biến sự bất đồng từ **tình cờ** thành **được khai báo**.

`CAP-DRIFT-05` giữ điều đó:

- ba con số phải đúng bằng giá trị đã ghim — một con số nhúc nhích một mình là đỏ;
- `~192` của spec phải còn khớp số học với 50s, nếu không thì 50s không còn là điều spec nói;
- mặc định C# được **đọc ngược từ `SchedulerCapacity.cs`**, không tin bản sao trong JS;
- sweep độ nhạy phải còn xoay quanh giả định hiện hành, không phải một giá trị cũ;
- đường thoát calibrated được selftest bằng một shape `TEST_ONLY`: model/runtime phải cùng nghĩa
  **channel occupancy**, còn chu kỳ spec phải bằng `occupancy + cooldown`;
- `calibratedBy` phải trỏ đúng artifact dưới `docs/evidence/W-0008/`, artifact phải tồn tại và
  tài liệu này phải dẫn lại nó.

Khi `W-0008` có số đo: đặt model/runtime bằng channel occupancy đã chọn, đặt chu kỳ spec bằng
`occupancy + cooldown` đã đo, cập nhật `CHANNEL_CONSTRAINTS.cooldownSeconds` nếu cần, bật
`calibrated` và trỏ `calibratedBy` vào evidence. **Không làm ba con số bằng nhau**: công thức
`channelsForWindow` vốn đã cộng cooldown, nên làm vậy sẽ tính cooldown hai lần.

## 4b. Độ dài phiên — input chưa có đáp án, và cái bẫy khi thay ẩu (`W-0134`)

Model **không có input độ dài phiên**. `poolForProgramme` sizing bằng `policy.windowSeconds` —
`300s` cho Giờ Vàng — tức **confirmation window của từng đơn**, không phải phiên. Đó là phép thay
bảo thủ: nó giả định toàn bộ cao điểm ập đến cùng lúc.

Đây cũng là lý do `peakShare` mất gốc thời gian: `0.15` là tỉ lệ của đơn eligible trong **ngày**,
không có câu nào nói 15% đó đến trong bao lâu.

### Cái bẫy

`M8-OD-C` hỏi phiên dài bao lâu. Nhưng có một câu hỏi thứ ba **chưa ai hỏi**, và nó mới nguy hiểm:
thay `windowSeconds` bằng `sessionSeconds` **không phải** một phép đổi thang trung tính — nó âm
thầm nhận lấy giả định **khách đến đều**.

Đo trên nhánh Giờ Vàng của `UNCALIBRATED_SCENARIO`:

| Sizing | Kênh |
| --- | --- |
| `windowSeconds = 300` (hiện tại) | **16** |
| `sessionSeconds = 2700` (thay thẳng) | **2** |
| Làm tử tế: rải cao điểm ra từng lát 300s của phiên 2700s | **2** |

Hai dòng cuối trùng nhau — vì chúng **là cùng một giả định**, chỉ khác cách mặc áo. Với một quyết
định mua sắm, đó là chênh giữa mua 32 kênh và mua 4.

### Vì vậy `W-0134` không đổi phép tính

`SESSION_LENGTH` trong `tools/capacity-sim/capacity-model.mjs` khai báo input với
`sessionSeconds: null`, `arrivalProfile: null`, `decisionId: "M8-OD-C"`, và ghi lại con số
`2700s` **chỉ để nhận diện và từ chối**, không bao giờ để làm mặc định.

`CAP-SESSION-06` canh:

- độ dài phiên còn để mở và còn nêu đúng tên `M8-OD-C`;
- **đo lại chính cái bẫy mỗi lần chạy** — nếu thay `2700s` không còn làm sizing sụp thì gate đỏ, vì
  model đã đổi hình và phải suy lại;
- đặt `sessionSeconds` mà `answered` vẫn `false` → đỏ;
- đặt `sessionSeconds` mà **không có `arrivalProfile`** → đỏ, kèm đúng con số `16 → 2`;
- đặt `sessionSeconds` = `2700` → đỏ, vì đó là con số ở tiêu đề cột §14.1 mà chính spec gọi là giả
  định, không phải quyết định;
- và model phải còn đang sizing đúng như `sizedAgainst` khai — kiểm bằng cách chạy thật
  `poolForProgramme` rồi so, không tin lời khai.

## 4c. Data-intake để đóng M8-01 (`W-0142`)

Preflight `W-0142` xác nhận calibration vẫn `NOT_RUN`: chưa có artifact W-0008, dữ liệu arrival
theo rolling window, attempt policy production hoặc reserve/failure factor. Contract đầu vào và
stop rule nằm tại [`docs/evidence/W-0142/README.md`](evidence/W-0142/README.md). Cho tới khi đủ bộ
đó, mọi kết quả của model chỉ là sensitivity range và **không được dùng để chốt mua**.

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

# W-0054 — Evidence: Capacity, cost & SIM sizing model (`P10-3`)

Ngày: `2026-08-19` (cập nhật cùng ngày: alert capacity đã dựng, xem §5) · Trạng thái:
**`TESTS_PASS_UNCALIBRATED`** — 4/4 verification §8 xanh;
**mô hình chưa bao giờ đối chiếu với một cuộc gọi thật**, và **không con số tiền tệ nào** được
khẳng định

## 1. Điều phải nói trước

`P10-3` §4 tự nói: *"Con số là **model** → hiệu chỉnh với perf test (P5-3) + thực địa (P8)"*. Slice
này giao **số học** và **khoảng**, không giao một con số để mua hàng.

Mọi đầu vào là **giả định chưa ai đo**: khối lượng đơn là đầu vào của chủ sở hữu chưa tới, thời
lượng cuộc gọi **chưa bao giờ được quan sát** vì chưa có cuộc gọi nào (`W-0008`). Output của
self-test mang nhãn `UNCALIBRATED` ở cả từng bước lẫn dòng tổng kết, nên **nhãn đi cùng con số**.

## 2. Sai lầm mô hình tồn tại để tránh, và con số chứng minh nó

**Tính theo trung bình ngày.** Golden Hour dồn cửa sổ attempt vào vài phút sau khi khách đặt.

Đo được: cùng khối lượng ngày, tính theo **đỉnh** cần **21 kênh**; trải đều ra cả ngày cần **2**.
Chênh **hơn 10 lần**. Một pool sizing theo trung bình sẽ trượt deadline mỗi sáng và làm hết hạn
những đơn **chưa bao giờ không liên lạc được**.

`CAP-MODEL-01` khẳng định bằng **đối chứng** chứ không bằng ví dụ: nếu hai con số bằng nhau, mô hình
không nhìn thấy cú dồn — và test đỏ.

## 3. Ba chi tiết số học, mỗi cái vì một cách sai

| Chi tiết | Cách sai nếu làm khác |
| --- | --- |
| cooldown **trong mẫu số** | 5 giây nghỉ sau cuộc 40 giây tốn 11% năng lực kênh **mỗi cuộc**, không phải một lần. Trừ ở cuối là bỏ qua 10 lần trong 11 |
| `ceil`, không `floor` | nửa kênh không nhận được cuộc gọi; làm tròn xuống cho ra pool **đúng trên giấy, thiếu khi chạy** |
| **cộng** các chương trình, không lấy max | đơn 24/7 đặt lúc 9h đang được gọi trong **cùng những phút** với đỉnh Golden Hour; lấy max là giả định chúng thay phiên |

Và một chi tiết thứ tư: cuộc gọi dài hơn cửa sổ trả `Infinity`. Đó là **lỗi thiết kế** — chính sách
hoặc thời lượng phải đổi — và trả một con số rất lớn sẽ che nó thành **vấn đề mua sắm**.

## 4. Kết quả là một khoảng, kèm tên đầu vào cần đo trước

`CAP-SENS-02` quét **27 góc** (3 khối lượng × 3 thời lượng × 3 tỉ lệ không nghe máy):

- khoảng: **7 … 72 kênh**
- đầu vào đẩy mạnh nhất: **`dailyOrders`** (biên độ **32,3 kênh**)

Con số **không** được chép vào tài liệu, vì một con số trong tài liệu sống lâu hơn giả định sinh ra
nó. Ai cần thì chạy self-test.

Và đó là câu trả lời có ích cho `W-0008`: **đi đo khối lượng đơn trước**, không phải thời lượng cuộc
gọi — dù trực giác nói ngược lại.

## 5. Pool đang ship vs mô hình

`CAP-ALERT-04` đọc `worker.hpa.simPoolSize` cả 4 môi trường:

| | dev | staging | lab | prod |
| --- | ---: | ---: | ---: | ---: |
| pool đang ship | 4 | 8 | 12 | **32** |

- **prod = 32 ≥ 21** mà mô hình cần ở đỉnh. Có dự phòng.
- thứ tự **không nghịch** — một pool lab lớn hơn prod nghĩa là buổi diễn tập to hơn thứ nó diễn tập.

~~**Không có alert capacity nào trong Prometheus.**~~ **Đã đóng `2026-08-19`** — và nó đóng đúng
theo cách cổng đã hẹn: `missed_deadline_count` có call site, `CAP-ALERT-04` cũ **đỏ** kèm chỉ dẫn
"thay bằng phép kiểm ngưỡng khớp mô hình", và đó chính là phép kiểm hiện tại.

Điều đáng nói là **ngưỡng do mô hình quyết**, không do người viết luật chọn. Mô hình nói pool prod
(32) phủ được đỉnh (21); dưới giả định của chính nó thì **không lần trượt nào xảy ra**. Nên ngưỡng
là **không**, và một lần trượt đọc là **một giả định của mô hình bị bác bỏ** — runbook gửi người
trực tới hiệu chỉnh (`W-0008`), **không** tới một đơn mua hàng. Mô hình `UNCALIBRATED` không đủ tư
cách biện minh cho một quyết định mua, và cổng khẳng định annotation **không** bảo ai đi mua.

Mô hình cũng **không thể** cho một ngưỡng khác không: không mô hình hàng đợi, không mô hình lỗi kênh
(xem §8). Số duy nhất nó nói được là số không.

Và hai thứ bị buộc vào nhau: **luật không-khoan-nhượng chỉ trung thực khi pool ship còn phủ được
đỉnh**. Nếu `simPoolSize` prod tụt xuống dưới, chính pool đó bảo đảm có trượt và luật thành nhiễu do
cấu tạo — nên phép so `prod ≥ peak` ở trên không còn chỉ là kiểm chart, nó là **tiền đề của alert**.

### `cost_per_confirmed_order` vẫn không có instrument, và cổng giữ lý do đó sống

Hai chỉ số của `ARCH-06` §1 **không đối xứng**: `missed_deadline_count` chỉ cần một sự kiện IVR quan
sát được; `cost_per_confirmed_order` cần một con số **từ bên ngoài**. Mẫu số đã đo được
(`analytics.agg_kpi_daily.confirmed_count`), tử số thì không — cả **6/6** dòng ở
`docs/cost-model.md` §3 còn trống.

`CAP-ALERT-04` đọc bảng đó và đòi mọi dòng còn `❌`. Điền một báo giá vào → cổng đỏ kèm chỉ dẫn dựng
metric. Nên "chưa làm" ở đây là một khẳng định **tự vô hiệu khi hết đúng**, không phải một chỗ trống.

**Phiên bản đầu của phép kiểm này sai, và kiểm âm bắt được.** Nó chọn dòng **theo dấu `❌`** rồi đòi
mọi dòng đã chọn có `❌` — nên một dòng hết bị chặn **rơi khỏi mẫu** thay vì làm cổng đỏ; `every()`
trên phần còn sống vẫn đúng. Đã đổi sang chọn **theo cấu trúc** (mọi dòng dữ liệu của §3) và ghim số
dòng, để **trả lời một dòng** và **xoá một dòng** đều nhìn thấy được. Một cổng mà đối tượng của nó
có thể tự bước ra khỏi mẫu thì không phải là một cổng.

## 6. Chi phí: công thức, không có số

`docs/cost-model.md` đưa công thức và **bảng đầu vào cần đi hỏi**, tất cả trống. Lý do ở đầu tài
liệu: một con số chi phí sống lâu hơn giả định sinh ra nó, và đặt một mức "tham khảo" là gần như bảo
đảm nó bị trích trong một cuộc họp mua sắm sáu tháng sau, tách khỏi câu "đây là giả định".

Một quyết định đáng nêu: **chia cho đơn đã xác nhận, không chia cho số cuộc gọi**. Một pool gọi gấp
đôi mà xác nhận đúng bấy nhiêu đơn thì tốn gấp đôi cho mỗi kết quả, và chia cho số cuộc gọi **giấu
đúng điều đó**. Khi chưa xác nhận được đơn nào, hàm trả `null` — chia cho một kết quả chưa xảy ra là
phép chia không có nghĩa, và test khẳng định nó không trả về một con số.

Mẫu số là thứ **duy nhất IVR tự đo được**: nó nằm sẵn ở `analytics.agg_kpi_daily.confirmed_count`
(`W-0055`). Ba dòng tử số thì không dòng nào IVR biết.

## 7. Kiểm chứng

| Test | Khẳng định | Kết quả |
| --- | --- | --- |
| `CAP-MODEL-01` | đỉnh (21) > trải đều (2); cooldown giảm năng lực; `ceil`; các chương trình cộng | ✅ |
| `CAP-SENS-02` | 27 góc → 7..72 kênh, `dailyOrders` chi phối | ✅ |
| `CAP-CALIB-03` | mô hình **tự khai chưa hiệu chỉnh** và nêu `W-0008`; báo cáo perf **không** khai đã đo thời lượng cuộc gọi | ✅ `PASS_UNCALIBRATED` |
| `CAP-ALERT-04` | prod ≥ mô hình (**tiền đề của alert**); ladder không nghịch; alert tồn tại, ngưỡng `> 0`, đi qua `increase()`, annotation không bảo đi mua; **6/6 đầu vào chi phí còn trống** | ✅ `PASS_WITH_NOT_PROVEN=COST_METRIC` |

| Lệnh | Kết quả |
| --- | --- |
| `capacity-selftest.mjs` | `CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` — `capacity_selftest` root-included, `allow_failure: false` |

## 8. Cái này KHÔNG chứng minh

- **Chưa hiệu chỉnh.** `P5-3` đo API và scheduler, **chưa bao giờ đo một cuộc quay số**. Mô hình
  **không được** dùng để quyết định mua hàng cho tới khi `W-0008` cho ra thời lượng cuộc gọi đo được.
- **Không có dự báo khối lượng từ business.** `dailyOrders`, `eligibleRate` và phân bố theo chương
  trình đều là giả định của tôi — và `dailyOrders` là đầu vào chi phối kết quả.
- **Không mô hình hoá lỗi kênh.** DT-04 cách ly kênh sau 3 lần hỏng/10 phút; mô hình giả định mọi
  kênh khoẻ, nên pool thật cần thêm dự phòng mà **chưa ai định lượng**.
- **Không mô hình hoá hàng đợi.** Nó tính pool **vừa đủ** trong cửa sổ, không tính độ trễ chờ hay
  phân bố thời gian chờ.
- **Không có báo giá nào**, nên không có chi phí trên mỗi đơn.
- ~~**Không có alert capacity.**~~ Đã đóng `2026-08-19` — xem §5. Nhưng luật đó **chưa từng nổ trên
  dữ liệu thật**: nó qua `promtool` với chuỗi số liệu tổng hợp (`IT-SLO-CAPACITY-04`) và counter
  được đo qua `Meter` thật trong `IT-OBS-DEADLINE-10`, còn **chưa có exporter OTLP** (`W-0063`) nên
  chưa tín hiệu nào rời tiến trình.
- **Không có `cost_per_confirmed_order`** — và sẽ không có cho tới khi có báo giá. Xem §6.

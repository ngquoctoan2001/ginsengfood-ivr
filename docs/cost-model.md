# Cost model — `W-0054` · `P10-3`

Trạng thái: **`NO_QUOTE`** — **không con số tiền tệ nào** được khẳng định, vì chưa có báo giá nhà
cung cấp nào (`W-0008`). Công thức: `tools/capacity-sim/capacity-model.mjs`.

## 1. Vì sao không có con số

Một con số chi phí trong tài liệu **sống lâu hơn** giả định sinh ra nó. Đặt một mức giá "tham khảo"
ở đây là gần như bảo đảm nó sẽ được trích dẫn trong một cuộc họp mua sắm sáu tháng sau, tách khỏi
câu "đây là giả định".

Nên tài liệu này đưa **công thức và các đầu vào cần đi hỏi**, không đưa kết quả.

## 2. Công thức

```text
monthly = channels · simMonthlyCost + gatewayMonthlyCost + infraMonthlyCost

costPerConfirmedOrder = monthly / confirmedOrdersPerMonth
```

**Chia cho đơn ĐÃ XÁC NHẬN, không chia cho số cuộc gọi.** Một pool gọi gấp đôi mà xác nhận được
đúng bấy nhiêu đơn thì tốn gấp đôi cho mỗi kết quả — và chia cho số cuộc gọi sẽ **giấu đúng điều
đó**. Khi chưa xác nhận được đơn nào, hàm trả `null` chứ không trả một con số: chia cho một kết quả
chưa xảy ra là một phép chia không có nghĩa.

## 3. Đầu vào cần đi hỏi

| Đầu vào | Ai trả lời | Trạng thái |
| --- | --- | --- |
| giá thuê bao mỗi SIM/eSIM mỗi tháng | vendor telephony | ❌ `W-0008` |
| giá cước mỗi phút / mỗi cuộc | vendor telephony | ❌ `W-0008` |
| phí gateway/thiết bị mỗi tháng | vendor telephony | ❌ `W-0008` |
| hạ tầng (cluster, storage, observability) | Platform | ❌ `W-0063` |
| chi phí TTS | vendor speech | ❌ `OD-V1-19` — xem phần TTS phía trên tài liệu capacity |
| **số đơn xác nhận được mỗi tháng** | đo được từ `analytics.agg_kpi_daily` sau khi có lưu lượng thật | ❌ chưa có cuộc gọi thật nào |

Dòng cuối đáng chú ý: **mẫu số của chi phí trên mỗi đơn là thứ duy nhất IVR tự đo được** — nó nằm
sẵn trong kho phân tích (`W-0055`, cột `confirmed_count`). Ba dòng đầu là tử số, và không dòng nào
IVR biết.

## 4. `cost_per_confirmed_order` trên console

Màn báo cáo `P3-4` **cố ý không hiển thị** chỉ số này, và đã không hiển thị từ `W-0026`. Lý do vẫn
nguyên: tử số chưa tồn tại. Hiện một ô trống hay một số 0 đều tệ hơn không hiện — cái thứ nhất trông
như lỗi, cái thứ hai trông như miễn phí.

Khi có báo giá, chỉ số này bật lên bằng cách thêm **một hằng số chi phí có phiên bản** vào cấu hình,
không phải bằng cách sửa màn hình.

Cũng vì lý do đó **không có instrument `cost_per_confirmed_order`** trong `IvrTelemetry`, trong khi
`missed_deadline_count` — chỉ số còn lại của `ARCH-06` §1 — đã có từ `2026-08-19`. Hai chỉ số này
không đối xứng: cái kia chỉ cần một sự kiện IVR quan sát được, cái này cần một con số **từ bên
ngoài**. Một metric tên là "chi phí" mà không có chi phí thì tệ hơn không có metric — nó sẽ được
scrape, vẽ, rồi trích dẫn.

`CAP-ALERT-04` giữ cho lời bào chữa này **không sống lâu hơn sự thật của nó**: nó đọc bảng §3 theo
**cấu trúc** (mọi dòng dữ liệu), không theo dấu ❌ mà nó đang kiểm — chọn theo dấu thì một dòng hết
bị chặn sẽ **rơi khỏi mẫu** thay vì làm cổng đỏ. Điền một báo giá vào, hoặc xoá một dòng đi, đều làm
cổng đỏ kèm chỉ dẫn.

## 5. Điều mô hình chi phí này KHÔNG nói

- **Không có báo giá nào.** Mọi ô ở §3 trống.
- **Không mô hình hoá chi phí biên theo phút.** Công thức hiện tính theo thuê bao/kênh; một biểu giá
  theo phút sẽ làm chi phí phụ thuộc **thời lượng cuộc gọi** — chính đầu vào chưa ai đo.
- **Không mô hình hoá chi phí của một cuộc gọi hỏng.** Một lần quay số không ai nghe vẫn có thể bị
  tính cước tuỳ hợp đồng, và tỉ lệ không nghe máy trong mô hình dung lượng là 15–50%.
- **Không so sánh với chi phí phương án thay thế** (gọi tay, SMS). Đó là một quyết định business,
  không phải một mô hình kỹ thuật.

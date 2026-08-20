# Secret rotation runbook — `W-0047` · `P7-5` §6.4

> **Chưa diễn tập trên hệ triển khai.** `SEC-ROT-01`/`-03` chứng minh **cơ chế** dual-key và
> emergency revoke trong test; chưa lượt rotation nào chạy trên cluster. Đây là quy trình đã viết,
> không phải quy trình đã diễn.

## 1. Rotation định kỳ — bốn bước, và bước 4 là bước hay bị bỏ

| # | Việc | Vì sao |
| --- | --- | --- |
| 1 | Sinh secret mới (≥ 24 ký tự) | dưới ngưỡng đó fingerprint audit bị brute-force |
| 2 | `Rotate(new, overlap)` — **cả hai** giá trị được nhận | request đang bay vẫn cầm giá trị cũ |
| 3 | Cập nhật phía gọi sang giá trị mới, xác minh lan truyền | trong cửa sổ overlap |
| 4 | **Để cửa sổ tự đóng** | xem bên dưới |

Bước 4 không phải một hành động — nó là việc **không cần hành động nào**. Cửa sổ được ép trong code
(`NotAfter`), không phải bởi người nhớ chạy nửa sau runbook. Một rotation không ai hoàn tất sẽ để
giá trị bị lộ hợp lệ **vĩnh viễn**, mà đó đúng là điều rotation sinh ra để chặn.

Chọn `overlap` theo **thời gian sống của request dài nhất**, không theo lịch làm việc. Với IVR đó là
callback retry backoff — hiện tối đa vài phút, nên overlap 10–15 phút là dư.

## 2. Rotation khẩn cấp (nghi lộ)

```
RotateEmergency(newSecret)
```

**Không có cửa sổ.** Một overlap ở đây sẽ giữ giá trị đã lộ hoạt động đúng bằng khoảng thời gian kẻ
tấn công cần. Đánh đổi là có thật: vài request đang bay sẽ rớt. Đó là đánh đổi đúng — mất vài request
so với để một credential đã lộ tiếp tục sống.

Sau đó:

1. Xác minh giá trị cũ **bị từ chối** (không chỉ "đã cập nhật").
2. Kiểm audit: phải có một dòng `Emergency`, mang **fingerprint** chứ không mang giá trị.
3. Rà log/audit theo **fingerprint cũ** để dựng lại phạm vi sử dụng — đây chính là lý do fingerprint
   được ghi.

## 3. Rotate credential gọi dial-token resolver (D-05)

Cùng cơ chế, nhưng độ nhạy cao nhất trong inventory.

**Không có bước nào trong quy trình này chạm tới số thật.** Credential là thứ dùng để *hỏi* vault;
nó không phải thứ *giải mã* gì cả. IVR không giữ key giải mã (`OD-V1-18`), nên rotation ở đây không
thể lộ số ngay cả khi làm sai.

`SEC-ROT-02` khẳng định bề mặt rotation không mang gì hình dạng destination.

## 4. Cái **không** rotate

| | Vì sao |
| --- | --- |
| key ánh xạ token vault | **không thuộc IVR** (D-05, `OD-V1-18`) — rotate nó là việc của chủ sở hữu vault |
| khoá ký JWT service identity | `MockOidcIssuer` sinh RSA **theo tiến trình**, không persist; rotation thật cần issuer thật (`W-0006`) |

## 5. Xác minh sau mỗi lần rotate

```
1. Giá trị mới được nhận.
2. Giá trị cũ: được nhận (định kỳ, trong cửa sổ) hoặc bị từ chối (khẩn cấp).
3. Audit có đúng một dòng mới, mang fingerprint, không mang giá trị.
4. Không request nào rớt trong cửa sổ  ← chỉ kiểm được khi có drill trên hệ thật; hiện NOT_RUN.
```

Mục 4 là mục duy nhất chưa kiểm được ở đây, và nó được ghi là `NOT_RUN` thay vì suy ra từ ba mục
trên.


## 6. Rotate `ORDER_CORE_SERVICE_TOKEN` trên hệ đã triển khai

Token này tới qua **biến môi trường**, mà một tiến trình **không thể thấy env của chính nó đổi**.
Nên overlap ở đây là **hai giá trị cấu hình tường minh**, không phải reload-on-change:

| Bước | Cấu hình | Trạng thái fleet |
| --- | --- | --- |
| 1 | `TOKEN=cũ` | ổn định |
| 2 | `TOKEN=mới`, `TOKEN_PREVIOUS=cũ`, `TOKEN_PREVIOUS_RETIRES_AT=<T>` → rolling restart | pod cũ nhận `cũ`, pod mới nhận **cả hai** |
| 3 | người gọi chuyển sang `mới` | vẫn trong cửa sổ |
| 4 | tới `<T>` | pod mới **tự** từ chối `cũ` |
| 5 | xoá `TOKEN_PREVIOUS*` ở lần deploy sau | dọn dẹp, **không** phải bước bảo mật |

Bước 2 là bước quan trọng: trong lúc rolling restart, fleet có **cả pod cũ lẫn pod mới**. Nếu pod
mới chỉ nhận `mới`, thì người gọi còn cầm `cũ` sẽ hỏng ở đúng những pod đã cập nhật — chính là
outage mà rotation lẽ ra phải tránh.

Bước 5 **không phải bước bảo mật**: `TOKEN_PREVIOUS_RETIRES_AT` đã đóng cửa sổ ở bước 4. Bỏ quên
biến đó không kéo dài phơi nhiễm — `SEC-ROT-07` khẳng định một giá trị đã hết hạn **vẫn bị từ chối
dù còn nằm trong cấu hình**.

Boot sẽ **từ chối** nếu đặt `TOKEN_PREVIOUS` mà thiếu `TOKEN_PREVIOUS_RETIRES_AT`: một giá trị cũ
không có hạn sẽ sống tới khi ai đó nhớ xoá biến, tức là rotation không bao giờ kết thúc.

### 6.1 Thứ tự bước 2 → bước 3 **không đảo được**, và đây là số đo

`IT-K8S-ROTATE-07` chạy đúng kịch bản này trên cluster k3s thật, api **2 replica**, dò liên tục
bằng cả hai token **trong lúc** rolling restart:

| Người gọi đang cầm | Bị từ chối trong lúc rollout |
| --- | --- |
| token **cũ** | **0/4** |
| token **mới** | **2/4** |

Cột đầu là thứ overlap mua được: pod cũ giữ `cũ` làm current, pod mới giữ `cũ` làm previous, nên
**mọi pod ở mọi trạng thái của rollout** đều nhận nó.

Cột thứ hai là thứ overlap **không thể** mua: một pod chưa restart **chưa từng nghe nói tới** token
mới. Không có cấu hình nào sửa được điều đó, vì nó không phải vấn đề cấu hình — nó là vấn đề **thứ
tự**.

Nên nếu đảo bước 2 và bước 3 — cho người gọi chuyển sang token mới **trước** khi fleet hội tụ —
mọi request sẽ hỏng trong **suốt độ dài một lần deploy**. Đó không phải rủi ro lý thuyết; đó là cột
`2/4` ở trên, đo được.

Trước `2026-08-19` chart **không diễn đạt được** rotation này: `_helpers.tpl` chỉ nối
`ORDER_CORE_SERVICE_TOKEN`, không có `TOKEN_PREVIOUS` lẫn `TOKEN_PREVIOUS_RETIRES_AT`. Cơ chế
overlap có trong code và có trong runbook này, còn trên Kubernetes hình dạng duy nhất khả dụng là
**cắt cứng** — đúng cái cửa sổ mà `RotatingCredentialProvider` sinh ra để xoá.

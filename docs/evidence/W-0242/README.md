# W-0242 — `4d` cần đúng một thứ, và thứ đầu tiên phải làm với nó không phải thu âm

Ngày: 2026-09-09 · Baseline: `main@dbfda76` · Trạng thái: **TESTS_PASS**.

Owner hỏi `4d` cần gì. Câu trả lời ngắn: **một danh sách tên, viết đúng chuỗi M3 sẽ gửi.** Không
cần `sku_id`, nhóm, công thức, BOM — chỉ chuỗi.

Nhưng đi kiểm thì có một thứ phải làm **trước** khi thu: chạy danh sách qua chính intake.

## 1. Một số tên hàng bình thường bị intake từ chối — hôm nay

`SpeechItem.Create` gọi `PiiGuard.EnsureSafeText`, và nhánh địa chỉ của nó là:

```regex
(?<![\p{L}\p{N}])(?:đường|số nhà|ngõ|hẻm|ngách|thôn|ấp|tổ)\s+
```

`đường`, `tổ`, `ấp`, `thôn` **vừa là dấu hiệu địa chỉ vừa là từ vựng thực phẩm**. Chạy thật qua
`SpeechItem.Create`:

```text
BỊ CHẶN   Tổ yến                     BỊ CHẶN   Đường phèn
BỊ CHẶN   Tổ yến chưng               BỊ CHẶN   Đường thốt nốt
BỊ CHẶN   Yến chưng đường phèn       BỊ CHẶN   Đường nâu
BỊ CHẶN   Chè đường phèn             BỊ CHẶN   Mật ong đường mía
BỊ CHẶN   Sâm đường phèn             BỊ CHẶN   Ấp trứng · Thôn quê

   ok     Yến sào cao cấp     ok   Kẹo đường     ok   Nước đường
   ok     Nước hồng sâm 500ml ok   Sâm Ngọc Linh ok   Đông trùng hạ thảo
```

Luật thật: từ đó **theo sau là chữ khác** thì chặn; đứng cuối chuỗi thì không. Nên `Kẹo đường` qua
mà `Đường phèn` không.

`InvalidOperationException` được `TaskIntakeEndpoint:77` bắt ⇒ task bị từ chối sạch ở cửa. Nghĩa là
**một sản phẩm tổ yến hoặc đường phèn không gọi xác nhận được**, và điều đó đúng từ hôm nay, không
liên quan gì tới ghi âm.

## 2. Đây là va chạm đã từng gặp, ở một field khác

`W-0105` đã gặp đúng lớp này với **tên người**: nhánh ASCII khớp `Duong`, `Ngo`, `Ap`, `Thon`,
`Hem`, mà `Dương` và `Ngô` là họ Việt Nam thông thường — comment trong `PiiGuard` viết
*"Nobody can be asked to change their family name."*

Cách giải khi đó: thêm `EnsureSafeContactText` — chỉ nhánh phone và dial-token, **bỏ nhánh địa chỉ**
— và giới hạn nó cho field mang tên người, kèm câu:

> *"Customer-facing surfaces keep `IsSafeText`."*

`public_name` **là** customer-facing: nó được đọc cho khách nghe. Nên theo luật đang có, nó giữ
`IsSafeText`, và `Tổ yến` vẫn bị chặn. Đó là một vị trí **nhất quán**, không phải một lỗ hổng bị bỏ
quên — nhưng hệ quả của nó thì chưa ai nêu.

## 3. Nên `4d` quyết định luôn việc này

Nếu **không** SKU nào trong danh sách chứa `đường `/`tổ `/`ấp `/`thôn ` thì **không phải làm gì cả**.
Nếu có, thì phải chọn:

| | Cách | Giá |
| --- | --- | --- |
| 1 | **Chấp nhận** — sản phẩm đó không đi qua IVR | không sửa gì; mất một dòng sản phẩm khỏi kênh gọi |
| 2 | Field-scoped guard cho `public_name` — bỏ đúng 4 từ nhập nhằng, **giữ** `số nhà`/`ngõ`/`hẻm`/`ngách` | sửa guard, cần quyết định privacy tường minh |
| 3 | Thu hẹp theo ngữ cảnh — dấu hiệu địa chỉ chỉ tính khi theo sau là số hoặc tên riêng | đúng nhất, đắt nhất, dễ sai nhất |

**Tôi không tự chọn.** Nới một PII guard là đúng hình dạng `2a4f45d` — thứ phải được quyết tường
minh chứ không trôi vào vì tiện.

⇒ **Việc đầu tiên khi có danh sách không phải gửi đi thu, mà là chạy nó qua `SpeechItem.Create`.**
Rẻ, và nó nói ngay có phải quyết gì không.

## 4. Kiểm chứng

```text
19 tên thật qua SpeechItem.Create     18 nhận · 1 chặn ("Tổ yến chưng đường phèn")
15 chuỗi tách riêng                   10 chặn · 5 nhận — xác định đúng 4 từ nhập nhằng
TaskIntakeEndpoint:77                 bắt InvalidOperationException ⇒ từ chối ở cửa
PiiGuard:62-82                        EnsureSafeContactText đã tồn tại, giới hạn cho field tên người
dotnet test Ivr.sln                   922/922 (không đổi — lượt này không sửa code)
gate-status.mjs                       GATE_STATUS_PASS
```

Project tạm ngoài repo, `git status` sạch. Không sửa code, không nới guard nào.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 5. Và một điều owner vừa nói, đổi nhiều thứ trong worklist

> *"Hệ thống IVR này chỉ tôi có và bạn xây dựng thôi. Ngoài ra có dev bên module 3 thôi."*

Toàn bộ dàn vai trong worklist — `Legal/Privacy`, `Platform/Infra/Telephony`, `Security/Release`,
`chief auditor`, `Product`, `Vận hành` — **là cùng một người**, cộng một dev M3.

Hệ quả phải ghi, không phải để than mà để khỏi chờ nhầm:

- **Ba phiếu `W-0226`/`W-0227` không có người nhận riêng.** Chúng vẫn hữu ích như **danh sách câu
  hỏi cho chính owner**, nhưng "gửi đi" thì không có ai ở đầu kia.
- **`3.4` — 5 batch dispatch** cũng vậy: `0/5` không phải việc chưa làm, mà là việc **không có nơi
  để làm**.
- **`3.1`/`3.2`** — reconcile 19 `OD-V1` theo owner `Sales`/`Security`/`Core`: những owner đó không
  tồn tại như người riêng. Đây là lý do *thật* khiến 19 dòng ký một lượt, và nó **giải thích** chuyện
  đó chứ không biện minh.

Nhưng **một thứ không đổi**: `legal_gate` đòi `decision_authority = LEGAL_PRIVACY`. Không phải vì
công ty phải to, mà vì câu hỏi là *"hai repo model không có file LICENSE thì dùng thương mại được
không"* — và **không ai tự trả lời câu đó cho chính mình được**, dù công ty một người hay nghìn
người. Hai đường đi thật:

1. **Ý kiến pháp lý bên ngoài** — mua một lần, ghi `approval_reference`.
2. **Owner chấp nhận rủi ro tường minh** — nhưng khi đó phải ghi đúng là **chấp nhận rủi ro**, không
   phải ghi là *đã có review pháp lý*. Ghi sai chỗ này chính là `2a4f45d`.

Gate hiện tại **không có ô cho lựa chọn 2**. Đó là một thiếu sót thật của thiết kế gate, và là thứ
đáng sửa — nhưng sửa nó là **thêm một trạng thái `RISK_ACCEPTED` tách khỏi `PASS`**, không phải nới
`PASS`.

## 6. Và tôi lại làm hỏng `gate-status.mjs`, lần thứ ba

Dán regex trên vào ô tracker thì parser đọc `ngõ` thành Status:

```text
AssertionError: the tracker parser read something that is not a status
+ [ 'W-0242=ngõ' ]
```

Lần này nó lộ ra **vì sao ba lần**: guard tôi tự đặt suốt phiên kiểm chuỗi pipe **có khoảng trắng
hai bên**, còn parser tách trên pipe **trần**. Mọi ô trước đều không có pipe trần nên guard sai mà
vẫn xanh — nó chưa từng bảo vệ gì cả.

Sửa xong lần một thì đếm ra **14 pipe** trên một hàng 9 cột (đúng phải là `10`) — vì chính câu tôi
viết về pipe lại chứa pipe. Parser vẫn qua, chỉ vì chúng rơi vào cột cuối; tức nó xanh **do may**.

Luật đúng, thay cho guard cũ: **đếm pipe trên hàng phải bằng số cột cộng một.** Đó là bất biến kiểm
được, còn "không chứa chuỗi nào đó" thì không.

## 7. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| **4d** | danh sách tên, **đúng chuỗi M3 sẽ gửi** → tôi chạy qua intake trước khi thu | Owner |
| — | nếu có tên chứa `đường `/`tổ `/`ấp `/`thôn `: chọn 1/2/3 ở §3 | Owner |
| — | `legal_gate`: mua ý kiến ngoài, hay thêm trạng thái `RISK_ACCEPTED` | Owner |
| 4c | Sales còn phát dạng chỉ-có-quận không | Owner + dev M3 |

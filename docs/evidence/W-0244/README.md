# W-0244 — Đóng nốt lỗ trong chính việc tôi vừa làm

Ngày: 2026-09-09 · Baseline: `main@fdf4c15` · Trạng thái: **TESTS_PASS**.

`W-0241` thêm `TryResolveProvinceName`, trả `null` khi không nhận ra tỉnh — và **không ai định
nghĩa lúc đó đọc gì**. Đó là lỗ trong việc của tôi, nên tôi đóng.

## 1. Vì sao `null` ở đây nguy hơn `null` ở chỗ khác

Hai đoạn kề vùng giao là **đoạn cố định đã thu**:

```text
5. Fixed     ", giao đến "
6. Dynamic   delivery_area_short      ← nếu thiếu clip
7. Fixed     ". Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng."
```

Bỏ trống giá trị **không rút ngắn câu** — nó phát *"…giao đến. Bấm phím một để xác nhận đơn hàng"*.
Khách được mời xác nhận một chuyến giao **đi đâu không rõ**. Và không bỏ riêng cụm `", giao đến "`
được, vì nó nằm trong một file thu chung.

## 2. Nên `TryComposeArea` trả `false`, không trả run rỗng

Cùng hình dạng `RecordedSpeechComposer.TryCompose` cho danh sách món: **buộc caller quyết**, thay vì
degrade lặng lẽ.

Quyết cái gì thì **vẫn mở**, và tôi không tự chọn:

| | Cách | Ghi chú |
| --- | --- | --- |
| 1 | từ chối cuộc gọi, như chặn đáy của `3b` | nhất quán, nhưng xem §4 |
| 2 | thay bằng một giá trị chung — *"khu vực đã đăng ký"* | **không đụng `TemplateHash`**: nó thay **giá trị** của placeholder, không thay template |

Cách `2` đáng nói vì tôi từng tưởng nó đổi template. Không: template vẫn là
`giao đến {{delivery_area_short}}`.

## 3. Bank E **suy ra được toàn bộ từ code hôm nay**

Khác hẳn bank D — thứ đang chờ hai mươi cái tên chưa ai viết.

```text
bank E: 34 clip · Id phân biệt 34/34
    area-an-giang     An Giang
    area-bac-ninh     Bắc Ninh
    area-ca-mau       Cà Mau
    …
```

`AreaClipBank()` sinh từ chính bảng tỉnh đã biên dịch, không phải danh sách chép tay — cùng lý do
kịch bản `0..99` được sinh chứ không gõ. `UT-VOICE-3B-09` khẳng định **mọi clip trong bank tra ngược
được từ chính lời của nó**, nên kịch bản thu và đường tra lúc chạy không thể mô tả hai tập khác nhau.

⇒ **Kịch bản thu bank E có thể đưa vào phòng thu ngay hôm nay**, không chờ `4d`.

## 4. Nhưng cách `1` bị ràng vào `4c`, và đó là lý do tôi không chọn

3 trong 12 chuỗi thật trong repo là dạng **chỉ có quận**. Nếu Sales master data còn phát dạng đó
thì "từ chối cuộc gọi" **không phải xử lý ca hiếm — nó bỏ một phần tư số đơn**.

Câu quyết định vẫn là `4c`: Sales còn phát dạng chỉ-có-quận không. Repo không trả lời được.

## 5. Kiểm chứng

```text
gitnexus impact RecordedSpeechComposer   LOW · 0 direct · 0 process
6 chuỗi thật                             4 ra clip · 2 trả false (dạng chỉ có quận)
"Phường X, tỉnh Hải Dương"               -> area-hai-phong "Hải Phòng"   (sáp nhập, đã ghim)
AreaClipBank()                           34 clip · 34 Id phân biệt · round-trip 34/34
dotnet test Ivr.sln                      949/949 PASS, 0 failed, 0 skipped   (940 → 949)
traceability                             TEST_TRACEABILITY_CURRENT=579
gate-status.mjs                          GATE_STATUS_PASS
```

Chưa nối vào renderer — cùng lượt với composer món, khi có dữ liệu. Không đổi hành vi runtime.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại

Ba đoạn động nay **đều có đường clip và đều có cách báo thất bại tường minh**. Không còn lỗ thiết kế
nào tôi biết mà chưa đóng.

| # | Việc | Ai |
| ---: | --- | --- |
| `4d` | 20 tên — **chỉ bank D còn chờ**, bank A và E đã đủ | Owner + Product Master |
| `4c` | Sales còn phát dạng chỉ-có-quận không → quyết cách `1` hay `2` ở §2 | Owner + dev M3 |
| `3c` | gắn vào `Activated → Sellable` | Owner |
| — | `legal_gate`: mua ý kiến ngoài, hay thêm `RISK_ACCEPTED` | Owner |

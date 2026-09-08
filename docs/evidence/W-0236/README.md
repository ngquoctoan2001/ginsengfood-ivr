# W-0236 — `4` không phải việc dữ liệu, và một đơn ở Bình Thạnh đang nghe giọng Bắc

Ngày: 2026-09-08 · Baseline: `main@98f7b3a` · Trạng thái: **TESTS_PASS**.

Còn `4` (đơn vị + vùng giao) và `3c`. Cả hai xếp cho vận hành. Đi kiểm phần vùng giao thì nó
**không phải việc vận hành** — giống hệt `3b` từng nấp trong `3`.

## 1. Bank E không cùng loại với bank D

Bank C và D tra bằng **chuỗi chính xác**, và với tên hàng thì đúng: M3 gửi đúng tên trong một danh
mục có kiểm soát. Vùng giao thì không. 12 chuỗi `delivery_area_short` thật đang có trong repo:

```text
Phường Bến Nghé, TPHCM
Phường Bến Nghé, TP. Hồ Chí Minh
Phường Bến Nghé, Quận 1
Phường 12, Thành phố Hồ Chí Minh
phường 12, quận Bình Thạnh
Phường Phú Khương, tỉnh Vĩnh Long
…
```

**Một nơi, ba cách viết.** `TPHCM` / `TP. Hồ Chí Minh` / `Thành phố Hồ Chí Minh` là ba chuỗi khác
nhau. `RecordedSpeechCatalog` so ordinal sau trim — nó sẽ **trượt hai trong ba**.

**Chuỗi mang cả phường.** `ShortDeliveryArea.Create` chỉ chặn `160` ký tự và cấm chi tiết địa chỉ;
nó **không giới hạn tập giá trị**. Phường không phải `~40` như tên hàng.

⇒ Đưa một danh sách không giải được vấn đề. `4` là **quyết định nội dung**.

## 2. Đề xuất: đọc tỉnh

`DeliveryRegionResolver` **đã** parse đúng chuỗi này để chọn giọng. Chạy trên 12 mẫu thật:

```text
parse nhận ra tỉnh: 9/12
TPHCM / TP. Hồ Chí Minh / Thành phố Hồ Chí Minh  →  cùng South    ✅ biến thể đã giải
3 chuỗi trượt đều là dạng chỉ có QUẬN, không có tỉnh
```

| | Đọc cả chuỗi | **Đọc tỉnh** |
| --- | --- | --- |
| clip mỗi miền | **hàng nghìn** phường | **~34** tỉnh |
| biến thể chính tả | phải tự chuẩn hoá | `ToMatchKey` đã giải |
| khách nghe | *"…phường Phú Khương, tỉnh Vĩnh Long"* | *"…Vĩnh Long"* |

Cái giá là mất tên phường. Nhưng khách biết địa chỉ của chính họ; thứ cuộc gọi cần xác nhận là
**đơn này có phải của họ không**, và món hàng cộng số tiền đã trả lời rồi.

Chi phí kỹ thuật nhỏ: resolver trả `VietnamRegion?`, cần thêm hàm trả **tên tỉnh khớp**.

## 3. Và một thứ sai từ hôm nay, không liên quan ghi âm

Ba chuỗi trượt chỉ có **quận**, không có tỉnh. Resolver ghi rõ là có chủ đích:

> *"The 2025 reform removed the district tier, so a delivery area is normally just ward plus
> province."*

Nhưng bảng **đã** mang `29` tên tỉnh trước sáp nhập, với lý do:

> *"Sales master data and in-flight orders can still carry the old names, and without them every
> such order would silently fall back to the default voice."*

**Lý do đó áp cho quận thì không có gì.** Và ba chuỗi kia đang là fixture ở **6 file test**, tuy
**không test nào khẳng định** giọng nào đúng cho chúng — nên đây là **khoảng trống**, không phải
quyết định đã ghi.

Hôm nay chúng rơi về `FallbackRegion = North`. **Một đơn ở Bình Thạnh được đọc bằng giọng Bắc.**

> Tôi **không tự sửa**: thêm bảng quận là thêm dữ liệu hành chính, và câu quyết định là *"Sales
> master data có còn phát ra dạng chỉ-có-quận không"* — trong repo không trả lời được. Có thì phải
> sửa và **độc lập với ghi âm**; không thì ba fixture nên đổi sang dạng sau sáp nhập để khỏi mô tả
> một đầu vào không còn tồn tại.

## 4. Vì sao lượt này không viết code

Cơ chế đọc vùng giao chỉ dựng được sau khi biết **đọc gì** — cả chuỗi hay tỉnh. Dựng trước là dựng
sai, và `W-0232` đã ghi đúng bài học đó cho `3b`.

Bank C (đơn vị) thì vẫn là việc dữ liệu thật: `hộp`, `chai`, `ký`, `gói` — tập nhỏ, chuỗi ổn định,
tra chính xác được. Phần đó của `4` không vướng gì.

## 5. Kiểm chứng

```text
DeliveryRegionResolver.ProvinceRegionTable   71 key  (Bắc 26 · Trung 22 · Nam 23)
parse 12 chuỗi thật                          9/12; 3 trượt đều là dạng chỉ có quận
grep fixture chỉ-có-quận                     6 file test, 0 assertion về giọng
dotnet test Ivr.sln                          911/911 (không đổi — lượt này không sửa code)
gate-status.mjs                              GATE_STATUS_PASS — 234 work items
docs-selftest.mjs                            API_DOCS_SELFTEST_PASS
```

Project tạm ngoài repo, `git status` sạch. Không sửa code, không mở gate nào.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| **4a** | vùng giao: đọc cả chuỗi hay **đọc tỉnh** (đề xuất: tỉnh) | Owner + Product |
| 4b | danh sách **đơn vị** — việc dữ liệu thật, không vướng | Vận hành |
| **4c** | Sales master data còn phát dạng chỉ-có-quận không? | Bên nắm Sales master data |
| 3c | ai báo cho IVR khi Sales thêm sản phẩm | Vận hành |

`4a` chặn M8. `4b` thì M8 nối được ngay khi có danh sách.

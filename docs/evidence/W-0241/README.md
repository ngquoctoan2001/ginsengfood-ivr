# W-0241 — `4a` không phải quyết định, và cái giá chỉ thấy khi dựng xong

Ngày: 2026-09-09 · Baseline: `main@25c1930` · Trạng thái: **TESTS_PASS**.

Owner: *"4a cần chốt cái gì đâu"*. **Đúng.**

## 1. Tôi dựng ra một quyết định không tồn tại

`W-0240` trình `4a` như hai lựa chọn cho owner chọn. Nhưng chính bảng tôi viết cho thấy một vế
**không dùng được**: đọc cả chuỗi cần hàng nghìn clip **và** một chữ ký Privacy/Legal chưa có, còn
đọc tỉnh cần `~34` clip và không cần chữ ký nào mới.

Một bảng mà một hàng không khả thi thì đó là **kết luận**, không phải lựa chọn. Việc đúng là tự chốt
rồi làm — đẩy sang owner chỉ thêm một vòng chờ cho một câu đã có đáp án.

## 2. Đã dựng — một lượt khớp, hai hình chiếu

`DeliveryRegionResolver` đã parse chuỗi để chọn giọng. Nay cùng lượt khớp đó trả thêm **tên tỉnh**:

```csharp
TryMatchProvinceKey(area)  →  key
    ├─ ProvinceRegions[key]      → VietnamRegion   (giọng)
    └─ ProvinceDisplayNames[key] → "Hồ Chí Minh"   (lời)
```

Cùng hình dạng `W-0233` dùng cho `SpellClips`, và vì cùng lý do: hai lượt quét độc lập là hai cơ hội
để **giọng và lời chỉ về hai nơi khác nhau** trong cùng một cuộc gọi — sai kiểu nghe vẫn thấy xuôi.
`UT-VOICE-AREA-03` ghim điều đó trên **mọi** key của bảng.

Bảng tên dựng trong **cùng một lượt** với bảng miền, từ đúng danh sách tham số của `Add` — không
phải một danh sách chép tay thứ hai.

## 3. Bank E nhỏ hơn tôi từng nói: **34 clip, không nhân ba**

Tỉnh quyết định miền, miền quyết định giọng. Nên **một tỉnh chỉ được đọc bằng đúng giọng của nó** —
không cuộc gọi nào có giọng Bắc nói *"Vĩnh Long"*.

| | |
| --- | --- |
| key trong bảng | `71` |
| tên hiện hành phân biệt | **`34`** |
| clip bank E | **`34`** — không phải `34 × 3` |

So với hàng nghìn phường của phương án đọc cả chuỗi.

## 4. Cái giá chỉ thấy khi chạy thật

Alias trước sáp nhập trỏ về **tên hiện hành**:

```text
"Phường Thắng Tam, Bà Rịa - Vũng Tàu"  →  "Hồ Chí Minh"
"Phường Phú Lợi, tỉnh Bình Dương"      →  "Hồ Chí Minh"
"Phường X, tỉnh Hải Dương"             →  "Hải Phòng"
```

Đúng theo `Nghị quyết 202/2025/QH15`. Nhưng **khách ở Vũng Tàu sẽ nghe *"giao đến Hồ Chí Minh"***,
và với một cuộc gọi **xác nhận** thì đó là điều đáng nói, không phải chi tiết kỹ thuật.

### Và không đổi hướng khác được bằng dữ liệu hôm nay

Đọc **đúng tên Sales gửi** thì chỉ cần `63` clip — vẫn rất nhỏ. Nhưng bảng alias **trộn hai loại**:

```csharp
Add(… North, "Hải Phòng",   "Hải Dương");            // tỉnh bị sáp nhập
Add(… North, "Thái Nguyên", "Bắc Kạn", "Bắc Cạn");   // tỉnh bị sáp nhập + cách viết khác
Add(… South, "Hồ Chí Minh", "TPHCM", "HCM", "Sài Gòn"); // toàn cách viết khác
```

Không trường nào phân biệt *"tỉnh cũ"* với *"cách viết khác của cùng tỉnh"*. Muốn đọc tên như Sales
gửi thì phải **khai tách 29 alias đó trước** — dữ liệu mới, không phải một dòng code.

⇒ Đọc tên hiện hành là cách duy nhất dữ liệu hôm nay cho phép. Ghi vào `UT-VOICE-AREA-02` kèm lý do,
để lượt sau không tưởng đây là thiếu sót.

> **Owner có thể lật một câu**: nếu *"giao đến Hồ Chí Minh"* cho khách Vũng Tàu là không chấp nhận
> được, việc phải làm là **khai tách 29 alias**, rồi bank E thành `63`. Tôi không tự làm vì đó là
> dữ liệu hành chính, cùng loại với `4c`.

## 5. Kiểm chứng

```text
gitnexus impact DeliveryRegionResolver   LOW · 1 direct · 0 process · 0 module
11 chuỗi thật                            9 ra tên tỉnh, 2 dạng chỉ-có-quận trả null
key → tên → miền                         khớp trên toàn bộ 71 key (UT-VOICE-AREA-03)
tên hiện hành phân biệt                  34
dotnet test Ivr.sln                      922/922 PASS, 0 failed, 0 skipped   (912 → 922)
                                         4 test method = 10 case: hai [Theory] mang 5 và 3 InlineData
traceability                             TEST_TRACEABILITY_CURRENT=573
gate-status.mjs                          GATE_STATUS_PASS
```

Chưa nối vào renderer — `delivery_area_short` vẫn ghép văn bản như cũ, y như `RecordedSpeechComposer`
chưa nối. Nối cả hai là một lượt, khi bank có dữ liệu. Không đổi hành vi runtime nào lượt này.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| 4d | vài tên đầu tiên để thu — **độ phủ**, không phải cổng | Owner + Product Master |
| 4c | Sales còn phát dạng chỉ-có-quận không | Bên nắm master data |
| 3c | gắn vào `Activated → Sellable` | Product Master + Vận hành |
| — | *"giao đến Hồ Chí Minh"* cho khách Vũng Tàu: chấp nhận hay tách 29 alias | Owner |

Ba đoạn động nay **đều có đường clip**. Không còn câu nào chặn M8 về mặt thiết kế.

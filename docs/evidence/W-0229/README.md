# W-0229 — `R-2` chốt, kịch bản thu sinh từ code, và một câu tôi phải sửa

Ngày: 2026-09-08 · Baseline: `main@b6e8ee1` · Trạng thái: **TESTS_PASS**.

Owner chốt quyết định 1 trong 4 của `m8-16 §9`: **`R-2` — thu cụm `0..99`**.

## 1. Sinh kịch bản bằng chính speller, không gõ tay

Với `R-2`, thứ đắt nhất không phải số clip mà là **văn bản chính xác** đưa vào phòng thu. Tiếng Việt
có dạng đặc biệt ở đúng những chỗ dễ gõ sai:

```text
15 → "mười lăm"        không phải "mười năm"
21 → "hai mươi mốt"    không phải "hai mươi một"
24 → "hai mươi tư"     không phải "hai mươi bốn"
14 → "mười bốn"        vẫn giữ "bốn"
```

Gõ sai một dòng = thu lại một clip × ba giọng. Nên kịch bản được **sinh từ
`VietnameseNumberSpeller`**, chạy thật qua một project tạm ngoài repo tham chiếu `Ivr.Domain`, chứ
không chép tay từ đọc code.

### Phát hiện làm nhẹ việc thu

Spell `0..99` bằng cả ba `VietnameseNumberStyle` rồi so từng dòng: **100 chuỗi phân biệt mỗi miền,
không dòng nào lệch**. Khác biệt miền chỉ nằm ở `nghìn`/`ngàn` và `linh`/`lẻ`, cả hai **ngoài** dải
`0..99`.

⇒ **Một kịch bản văn bản, ba giọng đọc.** Công soạn không nhân ba, chỉ công thu.

## 2. Câu tôi nói sai, và số thật

Khi trình hai lựa chọn, tôi viết `R-2` *"ít mối nối hơn hẳn"*. Đo bằng chính speller thì **không
hẳn**:

| Số tiền | Đọc thành | R-1 | **R-2** | R-3 |
| ---: | --- | ---: | ---: | ---: |
| `560.000` | năm trăm sáu mươi nghìn đồng | 6 | **5** | 3 |
| `1.200.000` | một triệu hai trăm nghìn đồng | 6 | **6** | 5 |
| `2.350.000` | hai triệu ba trăm năm mươi nghìn đồng | 8 | **7** | 5 |
| | **mối nối trung bình** | `5,6` | **`4,7`** | `2,9` |
| | **bank mỗi miền** | `22` | **`107`** | `1005` |

`R-2` tốn **gấp ~5 lần** công thu để bớt **~1 mối nối**. Nếu chỉ đếm mối nối thì đó là món hời kém,
và owner chọn dựa trên câu tôi viết. Phải nói ra.

## 3. Nhưng `R-2` vẫn đúng — vì vị trí quan trọng hơn số lượng

Dưới `R-1`, *"sáu mươi"* bị cắt thành `[sáu][mươi]`: mối nối nằm **giữa một cụm ngữ điệu**, chỗ tệ
nhất có thể cắt trong một ngôn ngữ có thanh điệu. `R-2` xoá đúng loại mối nối đó; số còn lại rơi
vào ranh giới tự nhiên (`trăm`, `nghìn`, `đồng`) — nơi người đọc vốn đã ngắt hơi.

Và với `items_spoken`, `R-2` thắng rõ ràng: **mọi số lượng `1..20` là đúng một clip**, trong khi
`R-1` cần tới ba (`21` = `[hai][mươi][mốt]`).

Nên kết luận không đổi, chỉ **lý do** đổi: chọn `R-2` vì **vị trí** mối nối, không phải vì số lượng.

> `R-3` (`0..999`) gần như giảm một nửa số mối nối nhưng cần `1005` clip mỗi miền — gấp mười `R-2`.
> Ghi vào `m8-16` để lựa chọn còn mở nếu ưu tiên đổi, **không đề xuất**.

## 4. Bank A đổi số

| | trước | sau `R-2` |
| --- | ---: | ---: |
| Bank A mỗi miền | `22` | **`107`** = 100 dòng `0..99` + `trăm` `triệu` `tỷ` `phẩy` `đồng` + `nghìn`\|`ngàn` + `linh`\|`lẻ` |
| Tổng ba miền | `≈ 222 + 3E` | **`≈ 477 + 3E`** |

Kịch bản 100 dòng nằm ở `m8-16 §6`, dạng bảng `20×5` để đọc trong phòng thu.

## 5. Kiểm chứng

```text
project tạm ngoài repo, tham chiếu src/Ivr.Domain — git status sạch, không file nào vào repo
0..99 × 3 style       100 chuỗi phân biệt mỗi miền, 0 dòng lệch giữa các miền
560000                Bắc "năm trăm sáu mươi nghìn" · Trung/Nam "năm trăm sáu mươi ngàn"
105                   Bắc "một trăm linh năm" · Trung/Nam "một trăm lẻ năm"
gate-status.mjs       GATE_STATUS_PASS — 227 work items
contract-freeze       CONTRACT_FREEZE=PASS
docs-selftest         API_DOCS_SELFTEST_PASS
```

`dotnet test` không chạy lại: không file `.cs`/`.mjs`/`.py` nào trong repo đổi. Không sửa code,
không mở gate nào. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại — ba quyết định

`m8-16 §9` còn: giữ hay thu lại 12 đoạn cố định · `pronunciationHints` bị bỏ qua hay thắng · danh
sách đơn vị và vùng giao.

Câu **`pronunciationHints`** là câu chặn thật: nếu chọn *"hint thắng ⇒ rơi về TTS"* thì runtime vẫn
phụ thuộc TTS và toàn bộ lập luận đóng `INF-A`/Security sụp. Ba câu còn lại đổi **kích thước** bank;
câu đó đổi **có bỏ được TTS hay không**.

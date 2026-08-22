# W-0104 — Đề xuất hiện đại hóa giọng IVR

Ngày: `2026-08-21`

Kết luận hiện tại: owner đã chấp nhận voice C ElevenLabs `Trung Caha` và lời chào trung tính “Xin chào Quý khách”; tracker chuyển `ACCEPTED` trong phạm vi software lab.

Cập nhật `2026-08-22`: A/B Edge đã được triển khai và nghe qua MicroSIP nhưng owner từ chối cả hai. Voice C ElevenLabs `Trung Caha` sau đó được sinh đúng script v2 và được owner chấp nhận về chất lượng. Lời thoại cuối đổi sang immutable script v3 dùng “Quý khách”; MP3/PCM/checksum/migration và đủ hai disposition MicroSIP được kiểm lại trước khi đóng acceptance.

## 1. Nguyên nhân hiện tại

Image lab tạo audio bằng `espeak-ng -v vi -s 145`, sau đó hạ về PCM mono 8 kHz. Chuỗi truyền Asterisk → MicroSIP và DTMF hoạt động đúng; engine eSpeak là nguyên nhân chính làm giọng máy móc, cũ và thiếu ngữ điệu tự nhiên.

## 2. Phương án khuyến nghị

Giữ nguyên `ITtsProvider`, `StaticFileTtsProvider`, ARI và Asterisk. Chỉ thay nguồn tạo audio:

1. **A/B miễn phí trong W-0104:** tạo hai file từ cùng script fake bằng Microsoft neural Vietnamese voice `vi-VN-HoaiMyNeural` và `vi-VN-NamMinhNeural`; chuyển về PCM mono 8 kHz, ghim SHA-256 và phát qua file provider hiện có.
2. **Không gửi dữ liệu thật:** lượt A/B chỉ dùng chị An/đơn fake hiện hữu. Không dùng số điện thoại, địa chỉ đầy đủ, token hoặc dữ liệu khách thật.
3. **Owner chọn voice:** chạy hai cuộc gọi MicroSIP có nhãn A/B nhưng cùng nội dung, âm lượng và codec. Owner chọn đúng một voice/version.
4. **Production sau này:** triển khai adapter Azure AI Speech chính thức sau `ITtsProvider`, secret nằm ngoài git; chỉ bật khi Privacy/Legal duyệt data residency/retention. Công cụ Edge neural không có API/SLA chính thức chỉ được dùng để dựng mẫu dev, không phải production provider.

Microsoft hiện liệt kê hai voice tiếng Việt neural trên trong tài liệu language support, hỗ trợ output PCM mono 8 kHz cho telephony, và trang giá hiện liệt kê free tier neural TTS 0,5 triệu ký tự/tháng. Các điều kiện/quota phải được kiểm tra lại khi tạo resource:

- <https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=tts>
- <https://learn.microsoft.com/en-us/azure/ai-services/speech-service/rest-text-to-speech>
- <https://azure.microsoft.com/en-us/pricing/details/cognitive-services/speech-services/>

## 3. Cách đọc đề xuất

- Tốc độ khoảng `-5%` so với mặc định neural; không cố bắt chước tổng đài cũ.
- Nghỉ ngắn sau lời chào, mã đơn, danh sách hàng, tổng tiền và trước hướng dẫn phím.
- Đọc tiền theo từ tiếng Việt; SKU/brand dùng pronunciation dictionary đã duyệt.
- Nhấn rõ “phím một” và “phím không”; không dùng nhạc nền.
- Chuẩn hóa loudness trước khi chuyển 8 kHz để tránh giọng nhỏ hoặc vỡ tiếng trên PCMU.

## 4. Acceptance A/B

W-0104 chỉ được owner chuyển `ACCEPTED` khi cả luồng gọi và UX đạt:

- candidate được chọn phát đủ câu, không cắt đầu/cuối;
- cách xưng hô đã duyệt, sản phẩm, số lượng, tổng tiền và khu vực được đọc đúng;
- không đọc raw phone/full address hoặc dữ liệu ngoài whitelist;
- lời mời bấm `1/0` rõ; DTMF vẫn ra đúng `IVR_CONFIRMED`/`IVR_CUSTOMER_CANCELLED`;
- owner chọn một voice/version và xác nhận âm lượng, tốc độ, độ tự nhiên;
- eSpeak chỉ còn là fallback kỹ thuật/offline, không được dùng làm evidence audio đã được chấp nhận.

## 5. Ranh giới

Neural A/B trên MicroSIP vẫn chỉ là software-lab evidence. Nó không chứng minh PSTN, SIM, carrier, caller ID, 32 eSIM, Sales API thật hay quyền gọi khách hàng. `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên.

## 6. Evidence A/B đã chạy

| Variant | Voice | Codec | SHA-256 | Runtime |
| --- | --- | --- | --- | --- |
| A | `vi-VN-HoaiMyNeural` | PCM signed 16-bit, 8 kHz, mono; 14,880 giây | `ad3ea2bc67bf0264baa8065f8e537193f4367af7d1eef08f6acdb1a8cd56c797` | `TASK-LAB-20260822013752` → `IVR_CONFIRMED` |
| B | `vi-VN-NamMinhNeural` | PCM signed 16-bit, 8 kHz, mono; 15,312 giây | `6db1992b99903fdfa22ad03020bc888d454fa86bd3821ab84cb32d531ea13790` | `TASK-LAB-20260822013829` → `IVR_CONFIRMED` |
| C v3 | ElevenLabs `Trung Caha`, voice ID `ueSxRO0nLF1bj93J2hVt` | PCM signed 16-bit, 8 kHz, mono; 16,770625 giây | `38a6cb92ef59e70d457d08cd048470443d910f1389dcfdf7fd5eea32a780818a` | `TASK-LAB-20260822042001` → `IVR_CONFIRMED`; `TASK-LAB-20260822042024` → `IVR_CUSTOMER_CANCELLED` |

Image Asterisk kiểm cả ba checksum khi boot; helper `Set-AsteriskLabVoice.ps1` kiểm lại checksum trước mỗi lần chuyển file bằng thao tác atomic. A/B dùng rate `-3%`; C dùng Eleven v3/Auto language detection trên web app. Owner đã chọn C và ghi `W-0104 ACCEPTED`; A/B được giữ làm evidence lịch sử, không phải voice đã chọn.

## 7. Voice C ElevenLabs và script v3

Owner chọn `Trung Caha - Clear, Firm and Informative` trong ElevenLabs. Voice library xác định voice ID `ueSxRO0nLF1bj93J2hVt`. Bản v3 được tạo ngày 2026-08-22 bằng Eleven v3/Auto language detection và 300 credits: MP3 44,1 kHz/mono/128 kbps, dài 16,770563 giây, SHA-256 `6f89c520236049d57d6e2147cd5b503a43106f7ee5b52afa2dab484abb691217`. MP3 nguồn nằm ngoài repo; image chỉ chứa PCM 8 kHz đã pin checksum.

Script MOCK cuối được version hóa thành `v3-test-approved`:

> Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm hai hộp cháo sâm diêm mạch - hạt sen, tổng tiền năm trăm sáu mươi nghìn đồng, giao đến phường Phú Khương, tỉnh Vĩnh Long. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.

Code renderer tạo số lượng/tổng tiền từ dữ liệu có cấu trúc; đoạn trên mô tả nội dung khách nghe chứ không phải text lưu cứng theo từng đơn. v1/v2 được giữ để replay task dev cũ. v3 không còn dùng `customer_display_name` trong template; trường tương thích vẫn được phép ở payload cũ nhưng không được render. Vị trí giao vẫn chỉ ở mức `delivery_area_short`; dữ liệu vị trí chi tiết không được mở trong W-0104.

Đã hoàn tất các gate W-0104: đúng script v3, voice ID, MP3 hash, PCM signed 16-bit/8 kHz/mono, checksum image, migration `20260822110000_W0104GenericCustomerGreetingScript`, registry approval đúng `MOCK_TEST+LAB` và hai disposition MicroSIP `1/0`. Owner đã chấp nhận voice/nội dung; W-0104 chuyển `ACCEPTED`. 300 credits/free account chỉ chứng minh dev sample, không phải quyền production; trước production vẫn phải chốt license/quyền dùng voice, plan/quota/API, privacy/DPA, retention/data residency và fallback nếu voice ID biến mất. `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên.

## 8. Kế thừa bởi W-0106 — đa giọng theo vùng miền

Cập nhật `2026-08-22`. W-0104 vẫn `ACCEPTED` và không bị mở lại; mục này ghi lại phần
[`W-0106`](../W-0106/README.md) kế thừa và một chỗ mục 7 nói chưa chính xác.

### 8.1 Đính chính mục 7

Mục 7 viết *"Code renderer tạo số lượng/tổng tiền từ dữ liệu có cấu trúc"*. Tại thời điểm đó
điều này **chưa đúng với cách đọc**: `VietnameseOrderScriptRenderer` sinh ra
`"2 hộp"` và `"560.000 đồng"` dạng **chữ số**, trong khi kịch bản in ở mục 7 — và bản audio
owner thực sự nghe rồi chấp nhận — đọc `"hai hộp"` và `"năm trăm sáu mươi nghìn đồng"` dạng
**chữ**.

Bản audio v3 được **gõ tay** trên web app ElevenLabs, nên nhánh chữ số của renderer chưa từng
có ai nghe. Cách một engine TTS đọc chuỗi `"560.000"` là tùy engine, và đó lại đúng là con số
khách được hỏi để bấm phím xác nhận.

W-0106 đã sửa: `VietnameseNumberSpeller` chuyển số sang chữ trước khi vào template. Bảy test
cũ đang ghim chính hành vi sai này đã được cập nhật. Từ nay text renderer sinh ra khớp với
text đã được duyệt.

### 8.2 Voice C không còn là giọng production dự kiến

`Trung Caha` (`ueSxRO0nLF1bj93J2hVt`) là **giọng nam, một giọng cho mọi khách**. Yêu cầu mới
là **ba giọng nữ theo ba miền**, nên bộ giọng của W-0104 bị thay thế về mặt nội dung:

| | W-0104 | W-0106 |
| --- | --- | --- |
| Số giọng | 1 | 3, chọn theo `delivery_area_short` |
| Giới tính | Nam | Nữ |
| Giọng | `Trung Caha` | Thắm (Bắc) · Zara (Trung) · Giang (Nam) — `OD-VOICE-05` |
| Chọn lúc nào | Lúc boot, qua `IVR_LAB_VOICE_VARIANT` | Theo **từng cuộc gọi** |

File `a`/`b`/`c` và checksum của chúng **được giữ nguyên** làm evidence lịch sử. W-0106 dùng
tên riêng `-region-north|central|south` để không đè lên — hậu tố `-c` đã thuộc về voice C.

### 8.3 Điều W-0104 kết luận vẫn còn đúng

- Neural A/B (`vi-VN-HoaiMyNeural`, `vi-VN-NamMinhNeural`) đã bị owner từ chối. W-0106 xác
  nhận lại: cả hai đều là **giọng Bắc**, không có giọng Trung, nên kể cả nếu được chấp nhận
  cũng không đáp ứng được yêu cầu ba miền.
- eSpeak chỉ còn là fallback kỹ thuật.
- Hướng dẫn cách đọc ở mục 3 (nghỉ nhịp, nhấn `phím một`/`phím không`, chuẩn hóa loudness
  trước khi hạ 8 kHz) được W-0106 áp dụng nguyên vẹn cho cả ba giọng.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi.

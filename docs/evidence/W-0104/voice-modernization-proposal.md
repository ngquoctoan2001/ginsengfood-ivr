# W-0104 — Đề xuất hiện đại hóa giọng IVR

Ngày: `2026-08-21`

Trạng thái: `OWNER_AUDIO_REJECTED` — đây là kết luận UX bên trong W-0104, không phải status mới của tracker. Tracker giữ `TESTS_PASS` vì telephony/DTMF đã đạt, nhưng chưa `ACCEPTED`.

Cập nhật `2026-08-22`: A/B Edge đã được triển khai và nghe qua MicroSIP nhưng owner từ chối cả hai. Owner sau đó chọn candidate ElevenLabs `Trung Caha`; candidate này vẫn cần được sinh lại bằng script v2 và nghe qua MicroSIP trước khi acceptance.

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

- hai voice A/B đều phát đủ câu, không cắt đầu/cuối;
- tên khách fake, mã đơn, sản phẩm, số lượng, tổng tiền và khu vực được đọc đúng;
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

Image Asterisk kiểm cả hai checksum khi boot; helper `Set-AsteriskLabVoice.ps1` kiểm lại checksum trước mỗi lần chuyển file bằng thao tác atomic. Hai voice dùng cùng nội dung fake và rate `-3%`. W-0104 vẫn `TESTS_PASS` cho đến khi owner ghi lựa chọn A/B và nhận xét chất lượng.

## 7. Candidate ElevenLabs và script v2

Owner đã chọn candidate `Trung Caha - Clear, Firm and Informative` trong ElevenLabs và gửi một MP3 preview. File có SHA-256 `0ac74bacee8f6e9d8ba75c71f9fe1e3e3f676d7cd01ed6ea9e4aaa6b7c48c56e`, MP3 44,1 kHz mono, dài 17,3975 giây. Preview này vẫn nói câu mở đầu cũ “Xin chào chị Giang”, nên chỉ là bằng chứng chọn chất giọng; file không được copy vào repo hay Asterisk runtime.

Script MOCK mới được version hóa thành `v2-test-approved`:

> Xin chào anh/chị Giang. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Anh/chị có đơn hàng gồm hai hộp cháo sâm diêm mạch - hạt sen, tổng tiền năm trăm sáu mươi nghìn đồng, giao đến phường Phú Khương, tỉnh Vĩnh Long. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.

Code renderer tạo số lượng/tổng tiền từ dữ liệu có cấu trúc; đoạn trên mô tả nội dung khách nghe chứ không phải text lưu cứng theo từng đơn. Phiên bản v1 được giữ để replay task dev cũ. Vị trí giao vẫn chỉ ở mức `delivery_area_short`; dữ liệu vị trí chi tiết không được mở trong W-0104.

Các gate còn thiếu trước `ACCEPTED`: sinh lại MP3 Trung Caha đúng script v2; ghi voice ID và điều kiện API/license; chuyển asset sang PCM signed 16-bit/8 kHz/mono; pin checksum; chạy hai disposition MicroSIP `1/0`; owner xác nhận nội dung, âm lượng, tốc độ và độ tự nhiên. Cho đến lúc đó `REAL_CUSTOMER_CALL_ALLOWED=NO` và W-0105 chưa bắt đầu.

# W-0104 — Đề xuất hiện đại hóa giọng IVR

Ngày: `2026-08-21`

Trạng thái: `OWNER_AUDIO_REJECTED` — đây là kết luận UX bên trong W-0104, không phải status mới của tracker. Tracker giữ `TESTS_PASS` vì telephony/DTMF đã đạt, nhưng chưa `ACCEPTED`.

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

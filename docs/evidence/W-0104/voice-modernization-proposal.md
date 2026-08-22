# W-0104 — Đề xuất hiện đại hóa giọng IVR

Ngày: `2026-08-21`

Trạng thái: `OWNER_AUDIO_REJECTED` — đây là kết luận UX bên trong W-0104, không phải status mới của tracker. Tracker giữ `TESTS_PASS` vì telephony/DTMF đã đạt, nhưng chưa `ACCEPTED`.

Cập nhật `2026-08-22`: A/B Edge đã được triển khai và nghe qua MicroSIP nhưng owner từ chối cả hai. Voice C ElevenLabs `Trung Caha` sau đó được sinh đúng script v2, chuyển PCM 8 kHz, pin checksum/voice ID và chạy đủ hai disposition MicroSIP; quyết định chất lượng cuối của owner vẫn chờ ghi nhận.

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
- tên khách fake, sản phẩm, số lượng, tổng tiền và khu vực được đọc đúng;
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
| C | ElevenLabs `Trung Caha`, voice ID `ueSxRO0nLF1bj93J2hVt` | PCM signed 16-bit, 8 kHz, mono; 17,1625 giây | `2341117f403acb20789821c9d8005b6e2a2cdfbc58fc14ffc5b4ce04dfcb2153` | `TASK-LAB-20260822033915` → `IVR_CONFIRMED`; `TASK-LAB-20260822034006` → `IVR_CUSTOMER_CANCELLED` |

Image Asterisk kiểm cả ba checksum khi boot; helper `Set-AsteriskLabVoice.ps1` kiểm lại checksum trước mỗi lần chuyển file bằng thao tác atomic. A/B dùng rate `-3%`; C dùng Eleven v3/Auto language detection trên web app. W-0104 vẫn `TESTS_PASS` cho đến khi owner ghi nhận xét chất lượng cuối.

## 7. Candidate ElevenLabs và script v2

Owner chọn `Trung Caha - Clear, Firm and Informative` trong ElevenLabs. Voice library xác định voice ID `ueSxRO0nLF1bj93J2hVt`. Bản mới được tạo ngày 2026-08-22 bằng Eleven v3, 302 credits và đúng script v2: MP3 44,1 kHz/mono/128 kbps, dài 17,162438 giây, SHA-256 `bd046426b0d663921f43d1855d49753ee7c5190968a37e5212223ca248cfd76f`. MP3 nguồn nằm ngoài repo; image chỉ chứa PCM 8 kHz đã pin checksum.

Script MOCK mới được version hóa thành `v2-test-approved`:

> Xin chào anh/chị Giang. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Anh/chị có đơn hàng gồm hai hộp cháo sâm diêm mạch - hạt sen, tổng tiền năm trăm sáu mươi nghìn đồng, giao đến phường Phú Khương, tỉnh Vĩnh Long. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.

Code renderer tạo số lượng/tổng tiền từ dữ liệu có cấu trúc; đoạn trên mô tả nội dung khách nghe chứ không phải text lưu cứng theo từng đơn. Phiên bản v1 được giữ để replay task dev cũ. Vị trí giao vẫn chỉ ở mức `delivery_area_short`; dữ liệu vị trí chi tiết không được mở trong W-0104.

Đã hoàn tất các gate kỹ thuật: đúng script v2, voice ID, MP3 hash, PCM signed 16-bit/8 kHz/mono, checksum image và hai disposition MicroSIP `1/0`. Còn đúng gate owner xác nhận nội dung, âm lượng, tốc độ và độ tự nhiên. Việc dùng 302 credits/free account chỉ chứng minh dev sample, không phải quyền production; trước production vẫn phải chốt license/quyền dùng voice, plan/quota/API, privacy/DPA, retention/data residency và fallback nếu voice ID biến mất. Cho đến khi owner xác nhận, `REAL_CUSTOMER_CALL_ALLOWED=NO`, W-0104 giữ `TESTS_PASS` và W-0105 chưa bắt đầu.

# W-0339 — Kiểm toàn luồng đơn giả trên lab SIP riêng

Ngày 22/09/2026. **7/7 ca runtime đạt; 0 lỗi kỹ thuật bị tính thành lượt khách.**
**REAL_CUSTOMER_CALL_ALLOWED=NO.** Trạng thái: EVIDENCE_SUBMITTED, chưa ACCEPTED.

Owner yêu cầu kiểm nhận đơn → tạo đủ lời thoại → gọi trong lab → nhận DTMF → lưu
kết quả, gồm hủy, hết hạn và thử lại. Lượt này dùng stack `ivr-w0339-flow` riêng,
database mới rỗng, network và volume riêng. Không sửa hoặc gửi đơn vào lab W-0338.
VieNeu tạo tiếng thật; Asterisk và SIP peer trao đổi RTP/DTMF thật trong software lab.
Đơn và Sales đều giả; không có SIM vật lý, PSTN hoặc khách thật.

## Kết quả runtime

| Ca | Bằng chứng chính | Kết quả lưu | Lượt khách |
| --- | --- | --- | ---: |
| Xác nhận, miền Bắc, đơn nhiều món | Phát đủ 7 phần; SIP peer gửi 1, switch nhận đúng một sự kiện DTMF | IVR_CONFIRMED | 1 |
| Khách hủy, miền Nam, tên dài | Phát đủ 7 phần; SIP peer gửi 0 | IVR_CUSTOMER_CANCELLED | 1 |
| TTS timeout rồi thử lại, miền Trung | Pause riêng TTS đến khi lần đầu lỗi trước dial; mở lại, phát đủ 7 phần và nhận 1 | IVR_TECHNICAL_EXCEPTION rồi IVR_CONFIRMED | 0 rồi 1 |
| Hết hạn khi còn chờ | Pause riêng worker; đơn hợp lệ hết cửa sổ trước khi worker chạy lại; không có attempt/SIP | IVR_CAPACITY_EXCEPTION | 0 |
| Hết hạn trong lúc tạo tiếng | Pause riêng TTS, để hạn đơn đến trước timeout phần; không dial hoặc phát playlist thiếu | TTS_CACHE_WINDOW_EXPIRED, rồi IVR_CONFIRMATION_WINDOW_EXPIRED | 0 |
| Không bấm phím | Phát đủ 7 phần, không có DTMF; chờ đúng chính sách lab một lượt | IVR_NO_ANSWER_FINAL | 1 |
| Operator ngắt cuộc gọi | Gọi API terminate cho đúng job khi đang phát phần đầu; Asterisk kết thúc kênh | CALL_TERMINATED_BY_OPERATOR / IVR_TECHNICAL_EXCEPTION | 0 |

Ở ca thử lại, hai attempt đều mang `attempt_number=1`: lần lỗi TTS không tiêu lượt
khách. Chỉ kết quả xác nhận sau đó được tính. Khách chủ động bấm 0 là một phản hồi
hợp lệ nên tính một lượt; operator ngắt là lỗi kỹ thuật nên không tính.

Ca hết hạn khi worker bị giữ không được đổi tên thành kết quả hết cửa sổ thông thường:
không có cuộc gọi nào trước hạn nên scheduler ghi `IVR_CAPACITY_EXCEPTION`, đúng
nhánh thiếu năng lực phục vụ. Ca hết hạn trong TTS mới có kết quả
`IVR_CONFIRMATION_WINDOW_EXPIRED`. Cả hai đều không tính lượt khách.

Operator terminate là ngắt lần gọi đang diễn ra, không hủy vĩnh viễn đơn. Harness
pause queue của lab riêng để giữ bằng chứng không bị một lượt retry tiếp theo thay
đổi; job còn `READY_FOR_SCHEDULER`, kết quả kỹ thuật không final và không có callback.
Không nhận đây là việc Sales đã hủy đơn.

## Đối soát cuối

[Truy vấn chỉ đọc](reconciliation.sql) chạy trên toàn database riêng sau bảy ca:

- 7 task, 7 attempt: **4 lượt khách + 3 lỗi kỹ thuật không tính lượt**.
- 9 kết quả, 6 final, 6 dòng callback; operator terminate không sinh callback.
- **0** technical attempt hoặc kết quả technical/capacity/window bị tính lượt khách.
- **0** task có nhiều final, **0** task có callback trùng, **0** callback cho kết quả nonfinal.

Bốn cuộc gọi hoàn chỉnh đều phát đủ bảy phần đúng thứ tự. Hash tên media động khớp
PCM đo W-0323; bốn phần cố định khớp catalog giọng đã duyệt. Kiểm log RTP của peer,
log DTMF của switch và hàng attempt/result; không tiêm DTMF trực tiếp qua ARI.
Không nghe duyệt lại giọng hoặc đo công suất từ bảy ca này.

## Bản build và cấu hình được kiểm

[verification.json](verification.json) ghim ID image của API, worker, TTS, Asterisk,
Postgres, fake Sales; hash bốn DLL đang chạy và source speech tại lúc dựng lab.
Worker là bản W-0338 có thay đổi deadline; API dùng image lab hiện có đã ghim ID.
Do đó đây là bằng chứng runtime theo bộ image/DLL, chưa phải chứng nhận một commit
sạch cho toàn bộ ứng dụng.

- Cấu hình [đã chốt](../W-0338/deadline-policy-review.md): 30 giây/phần, 8 đơn chờ FIFO,
  tối đa 90 giây chờ và 120 giây tổng, hạn đơn sớm hơn được ưu tiên; lease lab 360 giây.
- TTS giữ 2 CPU/4 GiB, capacity 1, ONNX 1 thread, mount model chỉ đọc. Readiness cuối 200.
- Lấy trực tiếp bốn DLL từ container, đối chiếu bản publish, rồi chạy
  **340/340 test speech/telephony/scheduler đạt**, không fail/skip. Không build lại DLL
  trong lượt kiểm này và không dùng kết quả unit cũ để chứng nhận binary mới.
- Bảng `W-0338/dll-bindings.json` lúc đầu còn hash build trước. Phát hiện chênh lệch
  trước khi chạy đơn; dùng hash thực tế và chạy lại test trên chính binary đó.

## Bằng chứng, lặp lại và dọn lab

Raw nằm tại `.artifacts/W-0339/`: manifest image/source, DLL thực tế, TRX, log API/
worker/TTS/SIP, kết quả từng ca, truy vấn đối soát, backup database giả và biên bản dọn.
36 artifact có hash trong `verification.json`; không công bố cấu hình riêng có
credential hoặc backup database trong repo.

Gói `.artifacts/W-0339/replay-kit.zip` chứa ba harness, truy vấn và fixture A/B/
MultiItem/LongName. Harness là bản ghi thao tác W-0339, không phải launcher production.
Muốn chạy lại phải dùng thư mục output/project/port/volume mới, giữ các guard về mode,
LAB-A, database rỗng và hash image; `setup.py` tự sinh credential lab riêng. Cần model
bundle local đã kiểm và các image ghim có sẵn, không tải model trong lượt này.

Giữ cả hai sự cố harness, không tính chúng là hệ thống đạt:

1. Header/body correlation không khớp: API từ chối 422 trước intake, database còn rỗng.
2. Bộ fixture mở rộng thiếu A/B: dừng sau ba ca đã đạt, trước intake ca hết hạn. Bổ sung
   fixture cơ sở đã có và tiếp tục đúng bốn ca chưa chạy; lưu log/snapshot trước resume.

Đã gỡ container, SIP peer và network riêng sau khi lưu bằng chứng. Hai volume
`ivr-w0339-flow-pg` và `ivr-w0339-flow-media` được giữ; không dọn volume hoặc sửa lab khác.

## Việc còn lại

- Chốt runtime W-0338 thành commit sạch, chạy toàn bộ test rồi full gate sweep tại
  cùng SHA và sinh lại hồ sơ nghiệm thu. Lượt 340 test ở đây là kiểm có phạm vi.
- Tích hợp trên máy đích, nhiều worker, M3 chung, S2/mirror và quyền production chưa
  thuộc bằng chứng này. Không chuyển ACCEPTED thay owner.

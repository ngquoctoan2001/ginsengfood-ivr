# W-0344 — Lấy receipt khi SCP bị ngắt

Ngày 22/09/2026. **USER_REPORTED_S5_FAIL; RECEIPT_FETCH_PENDING.**
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.**

Lượt `/home/ssv/ivr-full-flow-w0344-20260922-133208-4b83eafa` đã chạy qua helper tách phiên.
Output owner gửi cho thấy `docker compose up` thất bại, sau đó launcher ghi
`W0344_S5_TARGET_SYNTHETIC_SIP_FAIL` và tạo `result.tar.gz`.
`recovery-handoff.json` local ghi helper kết thúc mã 0: đây là **receipt đã sẵn**, không phải
7 ca kiểm đã đạt. Bước SCP lấy chẩn đoán bị đóng kết nối trước khi tải được file.

Chưa có receipt của chính lượt này ở local. Lượt khác `135115-e0b16d79` đã có bằng chứng
Asterisk exit 132 ở `s5-first-run.json`; không dùng lỗi của lượt đó để thay kết luận cho lượt này.
Không sửa application/image hoặc chạy lại bài kiểm trong bước lấy receipt.

Chạy trong PowerShell Windows:

```powershell
& 'C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0344\fetch-s5-receipt.ps1' -RunId '20260922-133208-4b83eafa'
```

Lệnh dùng một phiên SSH, chỉ đọc checksum và base64 của đúng archive có sẵn. Không SCP,
không gửi helper/archive, không gọi installer/Docker và không thay trạng thái server.
Base64 tránh làm hỏng dữ liệu nhị phân khi PowerShell 5.1 nhận stdout; trước khi ghi archive
local, script kiểm đúng path, định dạng, giới hạn 32 MiB và SHA256 của byte đã giải mã.

Kết quả lưu vào thư mục mới `fetch-<thời điểm>-<id>` dưới thư mục local của lượt cũ.
Chờ `W0344_RECEIPT_FETCHED_AND_HASH_VERIFIED`, rồi báo lại để Codex kiểm
`result/stack-start.log`, `result/summary.json`, log container và kết quả dọn tài nguyên.
Checksum đúng chỉ chứng minh tải nguyên byte; không biến kết quả FAIL thành PASS.

Kiểm local: parser PowerShell đạt; fixture đủ 256 giá trị byte được giải mã/kiểm hash đúng;
4 ca thiếu payload, sai path, đổi byte và base64 lỗi đều bị từ chối. Chưa thử kết nối server
trong lượt chuẩn bị này vì không có xác thực SSH tự động.

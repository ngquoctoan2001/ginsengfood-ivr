# W-0344 — Khôi phục bàn giao sau SSH bị đóng

Ngày 22/09/2026. **S5_EXECUTION_UNCONFIRMED; REAL_CUSTOMER_CALL_ALLOWED=NO.**

## Điều đã biết

Archive 663 MiB, checksum và installer đã chép thành công. SSH lúc chạy installer
trả `Connection closed by 192.168.1.61 port 22`, mã **255**; tải receipt trả **1**.
Chưa có log server để xác định nguyên nhân đóng SSH hoặc khẳng định installer đã chạy.
File cục bộ ghi nhận lỗi gốc:
`.artifacts/W-0344/s5-20260922-133208-4b83eafa/handoff-result.json`.

Không nhầm lượt này với W-0343: W-0344 kiểm toàn luồng bằng image TTS cũ `79e910…`,
còn W-0343 kiểm ứng viên metadata mới. Không chạy hai bài cùng lúc trên S5.

## Sửa lớp bàn giao

- Tiến trình kiểm thử dùng session riêng, stdin đóng, stdout/stderr ghi file ngay từ lúc bắt đầu.
  SSH chỉ theo dõi. Đứt kết nối sau khi fork không hủy bài kiểm.
- Khi kết nối lại, kiểm đúng host/thư mục, hash archive/installer rồi nhận biết trạng thái.
  Lượt đang chạy chỉ được theo dõi. Có receipt thì tải về. Lượt dở không còn tiến trình
  được trả `PARTIAL_REVIEW_REQUIRED`, không tự xóa hay chạy đè.
- Khóa `flock` và marker tạo độc quyền chặn các lần resume đồng thời tạo hai lượt.
- Không gửi lại archive. Helper nhỏ và checksum được chuyển qua SSH; mã RunId có allowlist.
- Nếu SSH lại trả 255, không cố tải loạt file chưa biết có tồn tại. Chạy lại cùng lệnh resume.
- Chẩn đoán chỉ lấy allowlist log/trạng thái; loại compose chứa mật khẩu, SQL và cấu hình SIP.
  Archive kết quả vẫn cần Codex đọc summary/7 ca; checksum đúng không có nghĩa bài kiểm đạt.

Gói ứng dụng, installer, model, dữ liệu giả, 7 ca và quota **2 CPU/4 GiB, 30/90/120 giây**
không thay đổi. Không sửa sshd, firewall hoặc dịch vụ đang chạy trên vps61.

## Lệnh cho lượt hiện tại

Trong PowerShell Windows:

```powershell
& 'C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0344\resume-s5-full-flow.ps1' -RunId '20260922-133208-4b83eafa'
```

Nhập mật khẩu trong terminal, không gửi mật khẩu vào chat. Giữ nguyên thư mục server.
Nếu script báo `PARTIAL_REVIEW_REQUIRED` hoặc `PREFLIGHT_FAILED`, gửi output cuối;
gói chẩn đoán sẽ được đưa vào thư mục con `recovery-<thời điểm>-<id>` của lượt cũ.
Nếu lại đứt SSH, dùng lại chính lệnh này. Không chạy lệnh tạo lượt mới.

## Kiểm local

5 test trên container Linux offline: tiến trình sống qua SIGHUP của nhóm SSH giả lập;
nhận biết trạng thái; chặn start lần hai trước khi spawn; từ chối sai host/path/hash;
chẩn đoán không chứa file cấu hình riêng. Dữ liệu fixture TEST_ONLY, không truy cập S5.
Lượt thử đầu lỗi do tmpfs của test không cho thực thi fake bash; đổi mount của fixture
thành exec rồi đạt, không nới quyền triển khai hoặc sửa bộ kiểm S5.

PowerShell kiểm pin/RunId và cú pháp Python 3.10 đạt. GitNexus không có script/helper trong
chỉ mục, trả UNKNOWN; đọc trực tiếp cho thấy thay đổi ở lớp bàn giao, không sửa hàm ứng dụng.
Kết quả và hash: [ssh-recovery-verification.json](ssh-recovery-verification.json).

# Đưa kho artifact đã ghim lên vps61

**Đã cài và kiểm trên vps61 ngày 22/09: 39/39 file, khôi phục 39/39 đạt.**
[Receipt đã xác minh](mirror-target-findings.md). Lệnh dưới giữ để tái lập khi cần;
không yêu cầu chạy lại. Không phát tiếng, không tạo cuộc gọi.
Giữ 2 CPU/4 GiB và profile 30/90/120; bước này chỉ sao chép và kiểm file.
Gói khoảng 304 MiB. Kho và bản khôi phục kiểm tra chiếm khoảng 1 GiB trên server;
vị trí `/home/ssv/ivr-artifact-mirror/releases/vieneu-w0340`.

Chạy một lệnh trong **PowerShell Windows**; nhập mật khẩu SSH ngay trong terminal khi được hỏi:

```powershell
& C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0340\run-mirror-handoff.ps1
```

Script kiểm checksum đã ghim của archive và installer trước khi gửi; tự chép ba file,
chạy kiểm trên vps61, rồi tải receipt về `.artifacts/W-0340/vieneu-mirror-w0340-result.tar.gz`.
Mỗi bước dừng nếu lỗi. Nếu release đã tồn tại, chỉ kiểm nó; không ghi đè các file trong kho.
Giữ log lỗi và báo lại nếu có `MIRROR_STOP`; không xoá kho để ép chạy qua.

Installer đặt quyền thư mục riêng theo umask 077; kiểm SHA256SUMS, từng file theo catalog,
rồi sao chép sang một thư mục khôi phục mới và kiểm lại 39 hash. Chỉ receipt/catalog được
gửi về, không chép lại cả model/image. Bản khôi phục nằm trong thư mục kết quả có timestamp.
Kho cùng máy không thay backup ngoài máy; chưa phải registry OCI cho Kubernetes pull.

Có thể chỉ kiểm gói local, chưa kết nối server:

```powershell
& C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0340\run-mirror-handoff.ps1 -VerifyOnly
```

Sau `MIRROR_RECEIPT_RETURNED`, báo “xong”. Codex sẽ kiểm receipt đúng vps61, catalog,
digest từng artifact và proof khôi phục trước khi cập nhật trạng thái mirror.
S2 đã nhận rủi ro có điều kiện; bằng chứng quyền dùng model/codec còn thiếu.
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.**

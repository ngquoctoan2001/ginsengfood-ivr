# W-0343 — Bàn giao ứng viên VieNeu mới lên mirror/S5

Ngày 22/09/2026. **LOCAL_CHECK_PASS / TARGET_HANDOFF_PENDING.**
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.**

## Đã làm

- Ghi [quyết định có phạm vi](release-decision.md): owner đã yêu cầu bàn giao/kiểm S5;
  S2 bảy điểm giữ nguyên. Nguyễn Quốc Toàn đã xác nhận thẩm quyền công ty cho phép kiêm nhiệm
  Legal/Privacy và chấp thuận phiếu; [quyết định thật](legal-approval.json) đã ràng vào khóa. Không yêu cầu nghe lại.
- Đóng gói **104 file**, khoảng **308 MiB**, gồm đúng image cuối W-0343, 13 artifact model,
  hồ sơ quyền dùng, bản giọng đã chuyển liên kết, PCM cố định và bộ đo W-0338.
  Bộ đo/binary/fixture W-0338 giữ nguyên byte; chỉ manifest bàn giao ràng image/lock mới.
- Kho mới: `/home/ssv/ivr-artifact-mirror/releases/vieneu-w0343`. Installer không ghi vào
  bản `vieneu-w0340`; không đổi tag/image hoặc tạo lại service đang chạy.
- Image được kiểm bằng archive/hash và Trivy mới của W-0343: **0 CVE tại thời điểm quét**,
  DB tải trong W-0340 còn hạn lúc quét. W-0343 quét lại đúng image cuối; không tải lại DB.
- Gói tự chứa model; máy S5 không tải weights hoặc image từ Internet. TTS chạy network none,
  readonly, non-root, cap-drop ALL, no-new-privileges, không publish cổng; client chỉ chung
  namespace mạng với TTS để dùng loopback. Không có scheduler, SIP, dial token hoặc dữ liệu khách.

## Kiểm local đã thực hiện

| Kiểm | Kết quả |
| --- | --- |
| Toàn bộ catalog và SHA256SUMS | 102 file đạt |
| Model và giấy phép trong đúng image | 13 artifact / 4 tài liệu / 2 model đạt |
| ENTRYPOINT mặc định + health readiness | HTTP 200, khoảng 7.6 giây tính từ lần kiểm đầu |
| Khởi động lại container + readiness | HTTP 200, khoảng 7.6 giây |
| Worker speech probe, một lượt local | **25/25 đơn, 0 lỗi, 25 PCM khớp** |
| Kịch bản | nhiều món/tên dài, cache, burst 2/4, hủy kết nối rồi hồi phục, soak local 30 giây |
| Quota thực đọc từ inspect/cgroup | **2 CPU / 4 GiB**, không swap, capacity 1, ORT 1 |
| Deadline | **30 / 90 / 120 giây**, queue 8 |
| Guard triển khai | 2 nhóm kiểm với 21 ca từ chối: đổi scope/profile dù rehash catalog, file lạ, sai image/quota/mạng/quyền |
| Container có sẵn | ID/image/thời điểm start/restart-count trước và sau giống nhau |
| Cú pháp trên Python 3.10 Ubuntu | đạt; PowerShell `-VerifyOnly` đạt |

Số liệu local không thay số liệu vps61. Trên S5, gói sẽ chạy **hai lượt mới**, mỗi lượt
450 giây soak cộng các ca so sánh/hồi phục, giữ profile đã đo ở W-0338.
Đây là triển khai và đo TTS + bộ chuẩn bị lời thoại worker; không xác nhận scheduler/SIP
toàn hệ thống trên S5, không thay bằng chứng DTMF ở lab, không chứng nhận công suất production.
Hồ sơ theo đúng archive của working tree, không tuyên bố là ứng viên từ commit sạch.

## Chạy bàn giao

Mở PowerShell trên Windows, dán **một lệnh**:

```powershell
& C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0343\run-s5-handoff.ps1
```

Nhập mật khẩu SSH khi terminal hỏi. Không gửi mật khẩu vào chat.
Script sẽ chép gói → kiểm hash/cài bản mirror riêng → kiểm khởi động/restart → chạy hai
lượt đo → tải cả thư mục kết quả về Windows, kể cả khi probe thất bại.
Chờ `S5_DEPLOYMENT_CHECK_RECEIPT_RETURNED` rồi báo “xong”. Thời gian gồm ít nhất 15 phút
soak và các ca ngoài soak; không gọi điện và không cần nghe.

Kết quả về `.artifacts/W-0343/vieneu-release-w0343-result.tar.gz` kèm SHA256.
Nếu lỗi, giữ output terminal và archive; không tự chạy lại đè receipt cũ.
Container thử được dọn sau từng lượt; image và bản mirror giữ lại để kiểm/triển khai đợt sau.

## Giới hạn hiện tại và bước tiếp

Đã thử SSH BatchMode tới `ssv@192.168.1.61`: máy trả `Permission denied (publickey,password)`.
Không có thông tin xác thực tự động, vì vậy **chưa chuyển hoặc triển khai image mới trên S5**.
Handoff tận dụng cách nhập mật khẩu trong terminal đã dùng thành công ở W-0340.

[Phiếu phát hành](release-decision.md) đã có câu trả lời chấp thuận rõ ràng của owner và thẩm quyền
Legal/Privacy được công ty cho phép kiêm nhiệm. Không coi đây là thẩm định pháp lý độc lập.
Bộ kiểm riêng model/mirror trả `release_blockers=NONE`; production của toàn hệ thống vẫn BLOCKED.

Sau khi có receipt: kiểm host thật, catalog/image, khởi động/restart, hai lượt 450 giây,
đơn/PCM, deadline/hồi phục, OOM và trạng thái service. Chỉ lúc đó cập nhật tình trạng ứng viên
trên máy đích. Không dùng kết quả này để bật khách thật.

Gói trước phê duyệt và phép đo local 25/25 được giữ ở `.artifacts/W-0343/pre-approval/`.
Gói bàn giao cuối dùng image `676133c79749`, config `3bfeb46fe22a`; 27/27 tests image và
HTTP contract đạt, Node/Python vẫn từ chối authority giả và bằng chứng license sai.
Khóa/voices/template chỉ đổi liên kết phê duyệt, hồ sơ nghe gốc và model giữ nguyên.
Toàn bộ 46 binding W-0342 được kiểm từ nguồn hoặc baseline; template B3 cũ được tái sinh
bằng generator cũ và khớp đúng hash lịch sử. Không sửa kết quả W-0342 thành phê duyệt đã có từ trước.

[verification.json](verification.json) ghi hash và kết quả local. Bộ thực thi:
[build-release.py](build-release.py), [install-release.py](install-release.py),
[run-release-check.py](run-release-check.py), [run-s5-handoff.ps1](run-s5-handoff.ps1),
[test-release-guards.py](test-release-guards.py).

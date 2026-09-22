# W-0340 — Mirror trên vps61 đã được kiểm

Ngày 22/09/2026. **S5_INTERNAL_ARTIFACT_STORE_AND_RESTORE_VERIFIED.**
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.**

Owner đã chạy handoff qua SSH và trả archive 3.660 byte. Receipt ghi `vps61`, thời điểm
**02:46:12 UTC / 09:46:12 giờ Việt Nam**, vị trí kho:
`/home/ssv/ivr-artifact-mirror/releases/vieneu-w0340`.

## Đã đối chiếu

- Catalog trả về giống từng byte catalog trong gói gửi; checksum catalog và archive khớp.
- **39/39 file gốc và 39/39 file khôi phục** đạt theo receipt của verifier đã ghim hash.
  Mọi cặp tên/size/SHA-256 trong receipt khớp catalog và file local tương ứng, không trùng mục.
- Gồm **2 archive image, 13 file model/card, 12 PCM cố định** và metadata/scan/helper.
- Đúng profile **2 CPU/4 GiB, 30/90/120 giây, 8 chỗ chờ**. Đây là xác minh metadata và file;
  không chạy thêm benchmark hoặc thay quota của container đang vận hành.
- Receipt ghi không khởi động container. Installer/verifier chỉ tạo kho, kiểm checksum,
  sao chép khôi phục và đóng gói receipt; không gọi model, phát audio hoặc gọi điện.

Chi tiết có hash: [mirror-target-verification.json](mirror-target-verification.json).
Raw giữ tại `.artifacts/W-0340/mirror-target-received/`. Đây là phân tích gói owner trả về,
không nhận là Codex đăng nhập hoặc hậu kiểm trực tiếp server.

## Mirror đã có, phần cấu hình phát hành còn tách riêng

Đã ghi đủ **13 URI SFTP/digest** khớp vị trí và nội dung trên máy đích. Bản khóa để chuẩn bị
phát hành nằm ở `.artifacts/W-0340/MODELS.lock.mirror-verified.json`, kèm người quyết định,
ngày và tham chiếu bằng chứng. Cơ sở là owner yêu cầu chốt mirror rồi tự thực hiện đúng handoff.

Verifier model hiện có kiểm bản khóa này với bundle gốc: **13/13 file đạt**, chỉ còn blocker
`LEGAL`. Kiểm ở chế độ production vẫn **bị từ chối** do thiếu chứng từ license; không biến
xác nhận bảy rủi ro S2 thành quyền sử dụng do tác giả model/codec cấp.

Bản khóa này **chưa thay `deploy/tts/models/MODELS.lock`**, chưa được đưa vào image đang chạy.
Đưa URI mới vào khóa chính cần cập nhật cùng chuỗi fingerprint/provenance khi tạo ứng viên
phát hành tiếp theo. Không thay hằng fingerprint để nhận một snapshot chưa được kiểm, và
không lấy scan của image cũ chứng nhận image build lại. Kho đã đo tiếp tục giữ nguyên bản đã gửi.

Đây là kho artifact offline qua SSH/SFTP và proof khôi phục tại cùng máy. Chưa phải registry
OCI cho Kubernetes pull, chưa chứng minh backup ngoài máy hoặc triển khai worker/SIP trên S5.

## Trạng thái tiếp theo

Phần **dựng và kiểm kho nội bộ trên vps61 đã xong**, không yêu cầu chạy lại hoặc nghe lại.
Phần nhận rủi ro S2 đã được Nguyễn Quốc Toàn chấp thuận có điều kiện. Còn chứng từ quyền
model/codec/giọng, áp dụng khóa mirror/provenance cho ứng viên phát hành và nghiệm thu
triển khai tích hợp. Quét W-0340 vẫn là 0 HIGH/CRITICAL cho đúng hai image; các mục
MEDIUM/LOW của worker được giữ trong hồ sơ, không xoá hoặc diễn giải thành không có CVE.

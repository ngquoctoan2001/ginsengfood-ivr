# W-0340 — S2, kho artifact nội bộ và quét image với DB mới

Ngày 22/09/2026. **Đã quét mới đúng image; Owner đã nhận bảy rủi ro S2 có điều kiện.**
**Mirror trên vps61 đã kiểm 39/39 file và khôi phục 39/39 file.**
**Đính chính quyền sử dụng 22/09:** [W-0341](../W-0341/README.md) đã xác minh công bố
Apache-2.0 đúng hai revision; VieNeu có FAQ cho phép preset và audio thương mại.
Phần còn lại là ghi nhận bằng chứng/phê duyệt vào hồ sơ và cơ chế kiểm, không phải thiếu mọi chứng từ.
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.**
Giữ TTS 2 CPU/4 GiB và profile 30/90/120 giây; không đổi runtime hoặc yêu cầu nghe lại.

## Kết quả quét

Trivy 0.73.0 ghim image scanner. Chỉ bước tải DB có mạng; quét archive offline, read-only,
không mount Docker socket. Database mới từ kho chính thức `mirror.gcr.io/aquasec/trivy-db:2`:
UpdatedAt **21/09/2026 19:11:02 UTC**, DownloadedAt **22/09/2026 02:21:22 UTC**,
NextUpdate **22/09/2026 19:11:02 UTC**, còn hạn lúc quét. Không dùng lại DB ngày 20/09.

| Image được kiểm | HIGH | CRITICAL | Khác |
| --- | --- | --- | --- |
| VieNeu đã đo S5, index `79e9106ec140`, config `13aae53fd114` | 0 | 0 | Không phát hiện CVE trong 26 gói Wolfi và 24 gói Python |
| Worker W-0338, index `1d4f05d5e284`, config `d6a7f0f74906` | 0 | 0 | 11 MEDIUM, 6 LOW theo cặp package/CVE; 12 CVE phân biệt, 16 mục có bản sửa |

Worker còn libc6 và OpenSSL của nền Ubuntu. Không gọi đây là image “không có lỗ hổng”.
Nếu chọn cập nhật nền để xử lý các mức còn lại, đó là image ứng viên mới: phải ghim lại
digest, quét và kiểm bằng chứng liên quan trước khi dùng; phiên này giữ đúng image đã đo.
Không lấy kết quả archive này để chứng nhận image build lại từ HEAD khác.

Đã kiểm chuỗi archive OCI index → manifest → config → Trivy ArtifactID, hash DB và raw.
Full digest, CVE, bản sửa được scanner báo và hash: [verification.json](verification.json).
Raw và helper ở `.artifacts/W-0340/`; kết quả cũ W-0326/W-0338 được giữ như lịch sử.

## S2

**Nguyễn Quốc Toàn** đã xác nhận thẩm quyền đại diện công ty và chấp thuận đủ bảy điểm,
giữ nguyên điều kiện. Vai trò tự khai: **người duy nhất build Module 8**.
[Phiếu S2](S2-owner-review.md) lưu nguyên văn câu trả lời, ngày và phạm vi quyết định.
Lượt W-0340 chỉ kiểm API/metadata và tên file LICENSE, chưa đọc đủ FAQ VieNeu.
W-0341 bổ sung bằng chứng công bố quyền cho weights, preset và audio thương mại; MOSS ONNX
công bố Apache-2.0 tại đúng revision. Không có LICENSE riêng không đồng nghĩa không được cấp phép.
Phiếu S2 giữ nguyên lịch sử; chữ ký nhận rủi ro và quyền do upstream công bố là hai bằng chứng riêng.
[W-0342](../W-0342/README.md) đã tích hợp bằng chứng và 13 mirror URI vào khóa nguồn;
production tiếp tục bị chặn ở phê duyệt riêng, archive/image W-0340 giữ nguyên.

## Mirror đã kiểm trên máy đích

Owner xác nhận **chưa có kho nội bộ**. Phương án đã đóng gói là kho artifact offline qua
SSH/SFTP trên vps61, tại `/home/ssv/ivr-artifact-mirror/releases/vieneu-w0340`.
Không dựng registry OCI, mở cổng hay thay container hiện có. Phù hợp lưu/khôi phục offline;
nếu rollout dùng Kubernetes pull image, cần thêm registry hoặc cơ chế import được kiểm riêng.

Gói khoảng 304 MiB gồm **39 file trong catalog**, thêm catalog và SHA256SUMS: hai archive
image, 13 file model/card, 12 PCM cố định và metadata/license/scan/helper. SHA256SUMS dùng LF.
Owner đã chạy gói và trả receipt đúng vps61. Catalog khớp từng byte; 39 hash file gốc và
39 file khôi phục khớp. Đã chuẩn bị khóa mirror có 13 URI/digest theo receipt; verifier
model nonprod đạt 13/13, chỉ còn blocker LEGAL. Chưa thay khóa/image đang vận hành.
Chi tiết: [mirror-target-findings.md](mirror-target-findings.md),
[mirror-target-verification.json](mirror-target-verification.json).
[Lệnh handoff](mirror-steps.md) giữ để tái lập khi cần, không yêu cầu chạy lại.

Kho này cùng máy S5, không thay thế backup ngoài máy. Gói không chứa số khách, credential,
DB vận hành hoặc dial token. Không chạy image, gọi model, phát tiếng hoặc gọi điện khi kiểm kho.

## Việc còn lại

1. Phần nhận rủi ro S2 đã có xác nhận của Nguyễn Quốc Toàn. W-0341 đã thu và kiểm công bố
   quyền model/codec/giọng; W-0342 đã tích hợp vào hồ sơ/provenance. Còn quyết định phát hành đúng thẩm quyền.
   Không dùng chữ ký nhận rủi ro để tự điền license hoặc nới gate.
2. Kho trên vps61 đã có bằng chứng. W-0342 đã áp dụng khóa mirror và chuỗi fingerprint/provenance
   vào ứng viên local mới; ứng viên đó chưa đưa lên S5. Giữ riêng scan/image đo hiện tại.
3. Hoàn tất nghiệm thu triển khai tích hợp và commit sạch theo task riêng. Các bằng chứng
   quét/mirror có giới hạn này không bật production; khách thật tiếp tục tắt.

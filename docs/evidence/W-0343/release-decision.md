# W-0343 — Quyết định bàn giao ứng viên VieNeu và mục phê duyệt còn lại

Ngày 22/09/2026. Người ra chỉ thị: **Nguyễn Quốc Toàn**.
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.**

## Phạm vi đã được giao thực hiện

Chỉ thị trực tiếp trong task:

> chốt phê duyệt phát hành còn lại, rồi chuyển ứng viên mới lên mirror/S5 để kiểm triển khai. Giữ 2 CPU/4 GiB, profile hiện tại

Ghi nhận **S5_VALIDATION_AUTHORIZED**: bàn giao và kiểm triển khai ứng viên W-0342
trên vps61, bằng đơn giả, trong container riêng. TTS 2 CPU/4 GiB, capacity 1,
ORT 1, segment 30 giây / queue 90 giây / toàn lời thoại 120 giây, queue 8.
Không có chỉ thị bật khách thật. Đây không phải nghiệm thu triển khai trước khi có kết quả máy đích.

Ứng viên: image `ivr-tts:w0342-license-evidence`, OCI index
`sha256:f5fd0909d7985821b417b75523b3ddfc96ae4f6a4cdd53f2acb7ab3a958e399a`;
image config `sha256:2c98db9c0ad2bdad5c1c348d6a2d7bc81f1dd90513ff91004f2344bff0476189`.
Hash archive, lock và manifest giọng được ràng trong catalog bàn giao.

## Các quyết định và chứng từ đã có

- [S2 bảy điểm](../W-0340/S2-owner-review.md): người ký đã xác nhận thẩm quyền công ty
  và chấp thuận có điều kiện. Giữ nguyên văn bản gốc, không ký lại.
- [Quyền sử dụng](../W-0341/README.md): hai model card đúng revision công bố Apache-2.0;
  VieNeu công bố phạm vi thương mại model/preset/audio và consent từ người nói.
  Đây là tuyên bố của nhà phát hành; chưa có kiểm toán độc lập hợp đồng người nói.
- [Bộ kiểm giấy phép và image](../W-0342/README.md): 4 tài liệu/2 model/13 artifact được
  ràng hash; 27 kiểm tra trong image đạt; Trivy không phát hiện CVE của đúng image mới.
  DB W-0340 còn hạn tại thời điểm quét, không tuyên bố đã tải lại DB ở W-0342.
- [Mirror cũ](../W-0340/mirror-target-findings.md): đã có receipt trên vps61.
  Bản mới dùng thư mục phiên bản riêng, giữ bản cũ để đối chiếu/khôi phục.
- Giọng và câu ghép đã được chủ module duyệt; bằng chứng nghe được chuyển liên kết
  metadata tại W-0342. Không yêu cầu nghe lại vì image không đổi model hoặc preset.

## Quyết định Legal/Privacy đã nhận

Trạng thái: **PASS — OWNER_AUTHORIZED_LEGAL_PRIVACY_DECISION**.

Nguyễn Quốc Toàn trả lời trực tiếp sau khi được trình phiếu này:

> Tôi có thẩm quyền cho phép kiêm nhiệm Legal/Privacy và chấp thuận phiếu này

Ghi nhận ngày 22/09/2026 tại [legal-approval.json](legal-approval.json). Đây là quyết định
công ty do người có thẩm quyền tự xác nhận, cho phép kiêm nhiệm hai vai trò cho đúng phạm vi
phiếu này; không diễn giải thành thẩm định pháp lý độc lập. Giữ khách thật tắt.

Nội dung đề nghị người có thẩm quyền duyệt: chấp nhận bộ chứng từ công bố đã ghim ở
W-0341/W-0342 làm cơ sở sử dụng đúng hai revision và ba preset trong IVR thương mại;
thực hiện nghĩa vụ Apache-2.0, giữ giấy phép/ghi công khi phân phối; giữ các điều kiện
riêng tư và vận hành đã ký ở S2. Phê duyệt quyền sử dụng không bật cuộc gọi khách thật.

Bộ kiểm hiện hành yêu cầu `decision_authority=LEGAL_PRIVACY`, tên người quyết định,
ngày và tham chiếu; vai trò `MODULE_8_OWNER` không tự mở cổng. Xem
[bộ kiểm Node](../../../deploy/ci/scripts/tts-provenance-gate.mjs) và
[bộ kiểm Python](../../../deploy/tts/scripts/verify-model.py).
Đây là quy tắc nội bộ hiện hữu, không phải yêu cầu xin giấy phép từ nhà mạng.

Xác nhận mới đã bổ sung đúng thẩm quyền và quyết định cho phép kiêm nhiệm; không ký lại S2.
Gate giữ nguyên cách từ chối người chỉ có vai trò MODULE_8_OWNER. Khóa mới ghi tên/ngày/
tham chiếu/hash quyết định. Bằng chứng license và mirror đạt không tự cho phép cutover.

Image W-0342 nêu trên là ứng viên trước khi ghi quyết định. Ứng viên cuối W-0343 được dựng
lại từ cùng runtime, chỉ đổi metadata phê duyệt; digest cuối xem verification.json/catalog.
Bàn giao và kiểm S5 bằng đơn giả tiếp tục được phép; production và khách thật vẫn tắt.

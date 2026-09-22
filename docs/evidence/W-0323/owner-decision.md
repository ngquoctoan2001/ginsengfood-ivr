# W-0323 — Phạm vi chốt giọng, nền image và việc còn chờ

Ngày `2026-09-21`. Người yêu cầu: IVR Owner trong task hiện tại.

## Quyết định hiện hành — OWNER_ACCEPTED

Owner đã chốt rõ sau khi nhận kết quả sửa mối nối:

> đừng bắt tôi nghe nữa, nghe quài. tôi chốt mà. tiếp cái khác iđ

**Ba giọng và chất lượng câu ghép sau sửa được Owner chấp thuận. Không còn mục yêu cầu
Owner nghe lại.** Quyết định gắn với gói W-0323, image prefix `3c221242af56`, 12 câu trong
`audio-seam-final/measurement.json` và các WAV có checksum trong `local-results.json`.
Đây là quyết định chất lượng tiếng; không đổi các kết quả kỹ thuật DTMF, timeout hay cho phép
gọi khách thật. Các đoạn dưới ghi lại diễn tiến trước quyết định này.

Owner đã nói: “12 đoạn tao nghe hết rồi”, nhận vai trò người nghe/quyết định, và sau đó yêu cầu:

> Chốt chất lượng ba giọng và câu ghép; chọn nền chạy VieNeu; sau đó kiểm thêm đơn nhiều món/tên dài và đo trên máy đích. Phần tạo tiếng và gọi lab đã chạy được, còn gọi khách thật vẫn chưa bật.

Ghi nhận yêu cầu này để giữ lựa chọn ba giọng và chốt gói nghe W-0321 đã trình:
Bắc `v3t-north-ngoc-linh`, Trung `v3t-central-ngoc-tran`, Nam `v3t-south-my-duyen`.
Gói gồm 12 đoạn cố định và sáu câu ghép A/B; hash manifest được ràng trong
[local-results.json](local-results.json). Câu “đã nghe hết” xác nhận trực tiếp 12 đoạn;
tuyến nghe cụ thể và việc đã nghe từng câu ghép chưa được Owner mô tả riêng, nên không ghi
thành bằng chứng nghe đủ sáu câu qua Asterisk/MicroSIP. Lựa chọn giọng không tự đóng toàn bộ S1.

Codex chọn Chainguard trong phạm vi “chọn nền” đã được giao: image được ghim digest, kiểm
container/model và quét lại đúng image. Đây là quyết định kỹ thuật có bằng chứng, không phải
chữ ký chấp nhận CVE Debian, bản quyền model hoặc phê duyệt production của Owner.

Owner nghe lượt đơn mở rộng và phản hồi: **“nhìn chung là đạt”**, nhưng nêu khoảng chờ sau
“có đơn hàng gồm” trước khi đọc tên sản phẩm, yêu cầu **“tôi muốn nó phải liền mạch luôn nhé”**.
Vì vậy giọng/nội dung được ghi nhận đạt nhìn chung; mối nối có yêu cầu sửa, chưa coi là duyệt
vô điều kiện tại thời điểm đó. Quyết định hiện hành ở đầu phiếu đã chốt chất lượng sau sửa.
Automation chỉ gửi DTMF sau audio, theo câu trả lời trước đó:
“Tôi sẽ chỉ nghe, để automation gửi phím”.

Chưa có thông tin máy đích S5, mirror nội bộ hoặc chứng từ bản quyền model/codec. Đã hỏi Owner
xác định máy đích; phép đo hiện có là máy Windows local. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

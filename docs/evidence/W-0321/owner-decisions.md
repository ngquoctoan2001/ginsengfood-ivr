# W-0321 — Phiếu quyết định S1/S2/S5

**Diễn tiến sau phiếu này:** Owner yêu cầu chốt giọng/câu ghép và chọn nền ở
[W-0323](../W-0323/owner-decision.md). Nội dung bên dưới giữ nguyên trạng thái khi W-0321 lập phiếu;
không dùng dòng “chưa có lựa chọn” lịch sử để phủ nhận quyết định mới.

Ngày `2026-09-21`; người quyết định là Owner trong task này (đã nhận vai trò).
**Chưa ký/chưa phê duyệt.** Không đổi trạng thái production theo câu nhận vai trò.

| Mục | Gói để xem | Thông tin/quyết định còn thiếu |
| --- | --- | --- |
| S1 | 12 đoạn cố định và 6 câu ghép trong bộ nghe local; sáu cuộc lab báo riêng | Đạt hoặc tên đoạn/mối nối cần sửa; xác nhận tuyến đã nghe DIRECT_WAV hay ASTERISK_MICROSIP_8KHZ |
| S2 nền image | Debian `6d0001b9cc66`: 44 HIGH/0 CRITICAL; Chainguard `2470470ae999`: 0/0. [Kết quả đầy đủ](local-results.json) | Chọn tiếp tục Chainguard (đổi nền/vendor) hoặc chấp nhận danh sách CVE Debian; phạm vi, thời hạn và trách nhiệm cập nhật image |
| S2 model | `deploy/tts/models/MODELS.lock`; 13 file bundle đã verify nonprod | Chứng từ/quyết định bản quyền cho từng nguồn model/codec, người ký, ngày, phạm vi dùng; chưa có license-file hash để verifier production PASS |
| S5 mirror | Cần lưu đúng bundle/hash đã chọn trong kho nội bộ | Địa chỉ mirror, đầu mối/quyền truy cập và retention; chuyển credential qua secret store, không ghi vào phiếu này |
| S5 máy | Phép đo hiện tại chỉ trên máy local shared | Host đích, CPU/RAM, giới hạn container, OS/runtime, quyền triển khai lab trên máy đó; sau đó đo lại latency/tải đồng thời |

0 HIGH/CRITICAL chỉ là kết quả scanner tại thời điểm quét, không chứng nhận không có rủi ro.
S2 chọn nền cũng không tự đóng phần bản quyền model, mirror hoặc S5 phần cứng.
Giữ `REAL_CUSTOMER_CALL_ALLOWED=NO`, `PRODUCTION_BLOCKED` cho đến khi đủ bằng chứng và thẩm quyền.

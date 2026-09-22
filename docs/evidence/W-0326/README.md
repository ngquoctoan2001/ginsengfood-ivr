# W-0326 — Chốt nghe, đo timeout và sửa kết nối retry VieNeu

Ngày `2026-09-21`; baseline `main@3545e10`. Checkout dùng chung với WIP W-0320/W-0321/W-0323;
bằng chứng ràng image và source bytes, không nhận hosted CI hoặc commit sạch.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## Quyết định của Owner

Owner đã nói **“đừng bắt tôi nghe nữa, nghe quài. tôi chốt mà. tiếp cái khác iđ”**.
Đã cập nhật [quyết định W-0323](../W-0323/owner-decision.md), JSON và tracker thành
**OWNER_ACCEPTED cho ba giọng và câu ghép cuối**. Không yêu cầu nghe lại. Các trạng thái chờ nghe
trong log cũ là snapshot trước xác nhận này, không phải một gate còn mở.

## Lỗi HTTP đã sửa

Khi TTS bận, POST trả `503` trước khi đọc body. Kết nối HTTP/1.1 vẫn mở, nên JSON còn dư bị đọc
thành một phần request kế tiếp. Client dùng connection pool nhận `400` thay vì retry thành công.
Trường hợp `415` và client đã timeout cũng tái hiện cùng lỗi.

`TtsHandler._empty` nay đặt `Connection: close` và đóng vòng xử lý kết nối. Client được mở kết nối
mới; không chờ đọc một body bị từ chối. Response PCM thành công không đổi.

Ba regression đều **đỏ với HTTP 400 trên image W-0323**, sau sửa **23/23 tests + HTTP contract
PASS** trên image W-0326. Test mô phỏng inference chậm, client timeout, request bị từ chối lúc
bận, rồi kiểm capacity được trả lại và request sau thành công. Không dùng model giả làm bằng
chứng chất lượng tiếng hoặc benchmark.

Đây là lỗi được tái hiện riêng. Log W-0323 chỉ chứng minh timeout và HTTP error sau đó; chưa đủ
để khẳng định lỗi kết nối là nguyên nhân của lần timeout 10 giây đó. Inference ONNX đang chạy
không tự bị hủy khi client ngắt; request đến khi nó còn bận vẫn nhận `503` đúng giới hạn concurrency.

## Đo request đầu trên model thật

Giữ image W-0323 đã ghim, model/voice/threads hiện có, tạo ba container mới độc lập, không mạng.
Mỗi lần chạy câu Nam nhiều món trước tiên, tiếp theo tên dài; lặp hai vòng. Tổng 12 câu/36 request.
`RuntimeState.initialize` vốn đã synthesize “Xin chào.” trước readiness, nên không suy diễn hệ
thống hoàn toàn chưa warm-up khi nhận request đầu. Không thêm warm-up chỉ dựa trên suy đoán.

| Lần | Startup gồm smoke (ms) | Request đầu (ms) | Request lớn nhất (ms) |
| --- | ---: | ---: | ---: |
| 1 | 5652 | 4878 | 4878 |
| 2 | 8376 | 7201 | 7201 |
| 3 | 5822 | 6202 | 6202 |

Máy vẫn là host Windows local dùng chung của W-0323, chưa phải S5. Chưa tái hiện vượt 10 giây
trong ba lần này; **không xóa kết quả timeout cũ, không tuyên bố đã giải quyết mọi timeout**.
Ngân sách 5 giây vẫn thiếu ở hai request đầu. Lab giữ 10 giây, production giữ 5 giây và đang bị
chặn; chưa đổi timeout hoặc thêm backoff không có phép đo máy đích.

## Image cuối và bằng chứng audio không đổi

Image prefix `79e9106ec140`; ID đủ trong `.artifacts/W-0326/image-id.txt` và block hex ở
[local-results.json](local-results.json). Cùng nền Chainguard đã chọn.
Image mới chạy thêm 12 ca A/B/nhiều món/tên dài × ba miền, 36 request model thật:
**12/12 PCM và thời lượng khớp từng byte với bản Owner đã chốt**. Request lớn nhất lượt này
`4760 ms`; không lấy một lượt máy nhẹ tải để phủ nhận các số đo lớn hơn ở bảng trên.
Không phát audio, không gọi MicroSIP, không yêu cầu Owner nghe.

Trivy `0.73.0`, scanner chính thức `aquasec/trivy` ghim digest, quét image archive: **0 HIGH,
0 CRITICAL**. DB cập nhật `2026-09-20T19:19:55Z`, tải `2026-09-21T02:15:23Z`.
Auto-review từ chối cách gắn Docker socket vì quyền quá rộng. Phương án được dùng sau đó là
`docker image save`, bind tar read-only, network none, rootfs read-only, cap-drop ALL và không có
Docker socket. Chuỗi binding OCI index → image config → Trivy ArtifactID được kiểm, kèm hash tar.
Scanner cũ đã bị xóa khỏi Docker; tải lại đúng version và xác minh digest khớp scanner W-0323.

Đã kiểm không có job lab chưa kết thúc và 0 active call trước recreate worker/TTS; sau cập nhật,
readiness/media preflight PASS, 0 active call/channel. Không gửi task gọi mới.

## Còn lại

**Cập nhật W-0329:** DTMF tự động đã kiểm đủ sáu ca và ca không nhập phím. Timeout10s đã
tái hiện ở worker dưới tải local; S5 vẫn chưa xác định. Xem [W-0329](../W-0329/README.md)
cho kết quả tiếp nối; các số và mục còn lại bên dưới là snapshot lúc kết thúc W-0326.

- Chất lượng tiếng đã chốt; không đưa việc nghe lại vào backlog.
- Ba ca DTMF W-0323 vẫn là bằng chứng kỹ thuật chưa đủ; phê duyệt nghe không thay kết quả đó.
- Cần máy S5 để đo dưới giới hạn CPU/RAM và tải đồng thời thực tế trước chọn ngân sách production.
- Mirror nội bộ và bản quyền model/codec vẫn chờ dữ liệu/quyết định tương ứng; production chưa bật.

Raw logs, PCM, archive và phép đo ở `.artifacts/W-0326/`; metadata/checksum trong JSON cạnh file này.
Không sửa model weights, lựa chọn giọng, phép ghép hoặc mã .NET trong lượt W-0326.

# W-0323 — Nền VieNeu, đơn mở rộng và sửa khoảng ngắt trước tên món

Ngày `2026-09-21`; baseline `main@a7f7ad3`. Checkout dùng chung và có WIP W-0320/W-0321/W-0324.
Kết quả ràng source bytes/image IDs trong [local-results.json](local-results.json), không chứng
nhận commit sạch hoặc hosted CI. `REAL_CUSTOMER_CALL_ALLOWED=NO`; production vẫn `BLOCKED`.

## Kết quả nghe và nguyên nhân khoảng chờ

Owner đánh giá giọng/nội dung **“nhìn chung là đạt”**, yêu cầu đọc liền mạch sau “có đơn hàng gồm”.
Sau sửa, Owner xác nhận **“tôi chốt mà”** và yêu cầu chuyển sang việc khác: **giọng và câu ghép
đã OWNER_ACCEPTED, không yêu cầu nghe lại**. [Phiếu quyết định](owner-decision.md) lưu nguyên văn.

Worker hoàn tất tổng hợp trước `DialAsync`; gateway gửi cả bảy media trong một ARI playback.
Khoảng chờ không phải đang gọi model giữa cuộc gọi. Đo WAV 8 kHz phát hiện khoảng lặng đầu đoạn
tên món giọng Nam, có một phần nhiễu cao tần không nghe thấy sau chuyển sang âm thanh điện thoại.

| Mối nối trước tên món | Trước sửa | Sau sửa cuối |
| --- | ---: | ---: |
| Nam, đơn ba món | 1820 ms | 220 ms |
| Nam, tên món dài | 680 ms | 220 ms |

Backend nay dò RMS từng cửa sổ 10 ms trong dải 8 kHz, ngưỡng -40 dB tương đối với cửa sổ lớn nhất,
sàn -66 dBFS. Chỉ rút khoảng lặng ngoài biên dài hơn 250 ms, giữ guard 60 ms; không xóa khoảng nghỉ
trong lời nói. Model/giọng/text/speaking rate giữ nguyên. Catalog cố định đã nghe không đổi.

Đối chiếu 36 phần động: phần giữa sau căn chỉnh giữ nguyên trong sai số tối đa 1 đơn vị PCM
16 bit do resampling; 48 lượt dùng phần cố định khớp byte. Đây là kiểm waveform; Owner đã chốt
chất lượng riêng trong phiếu quyết định. Phép đo khoảng lặng dựa ngưỡng RMS, không phải nhãn
phiên âm của từng âm tiết.

## Image được chọn

Chọn **Chainguard Python** với builder/runtime ghim digest trong `deploy/tts/Dockerfile.tts`.
Image cuối có prefix `3c221242af56`; ID đủ trong `.artifacts/W-0323/image-seam-id.txt` và
`local-results.json` (nối tám block hex rồi thêm `sha256:`). Hash metadata dùng base64 hoặc
block hex tám ký tự để tránh chuỗi số/từ ngẫu nhiên bị nhầm thành PII; không bỏ bit checksum.
Python `3.14.7`; `20/20` unit tests và HTTP contract/nonroot/read-only/network-none PASS.
Regression khoảng lặng đỏ trên image trước vá; sau vá kiểm cả âm đầu nhỏ, khoảng nghỉ trong câu,
khoảng nghỉ ngắn, audio im/ngắn và nhiễu cao tần.

Quét đúng image cuối bằng Trivy `0.73.0`: **0 HIGH, 0 CRITICAL**.
DB cập nhật `2026-09-20T19:19:55Z`, tải `2026-09-21T02:15:23Z`.
Report `.artifacts/W-0323/trivy-final.json`; không dùng scan của image trước vá để chứng nhận bản cuối.
Kết quả 0/0 không đóng bản quyền model, mirror, S5 hay các phê duyệt production khác.

## Phạm vi đơn và hiệu năng

`fake-orders.json` giữ A/B và thêm `MultiItem` (3 món, số lượng 2/3/1) cùng `LongName`
(một tên món dài, 4 hộp). Renderer thật xuất 12 câu: 4 đơn × 3 miền. Đây chưa phải kiểm cực hạn
20 món/tên 160 ký tự, và chưa phải benchmark tải đồng thời.

Máy local: i7-11700, 8 core/16 logical CPU, khoảng 32 GiB RAM; Docker Desktop nhận 16 CPU/
15,54 GiB. Cấu hình các pool ONNX/BLAS/OpenMP/MKL đều 1 thread; host dùng chung có tải khác.
**Chưa xác định máy đích S5**, nên không gọi các số này là kết quả S5.

Trước vá: 12 ca ban đầu + 18 ca lặp; đoạn Nam nhiều món vượt 5 giây ở 3/4 mẫu, lớn nhất
6036 ms. Sau vá cuối: 12 ca/36 request; lớn nhất **7794 ms**, 2 request vượt 5 giây.
Sự khác biệt có tải máy xen vào; không suy ra vá khoảng lặng làm model nhanh/chậm hơn.
Lab mở rộng dùng timeout riêng `10000 ms`, production giữ mặc định `5000 ms`.

Một task đầu sau recreate TTS gặp `TTS_TIMEOUT`, lần retry gặp `TTS_PROVIDER_HTTP_ERROR`
khi request cũ còn bận. Hai attempt technical đều `counted=false`, chưa quay số; dữ liệu giữ tại
`cold-start-failure/`. **Ngân sách 10 giây chưa chứng minh đủ cho khởi động lạnh**; không được
lấy kết quả warm-call để đóng mục này. Cần đo cold/warm và tải đồng thời trên máy đích trước
chọn timeout/pre-dial budget production, hoặc bổ sung warm-up có kiểm chứng.

## Gọi lab và giới hạn nghiệm thu

**Tiếp nối W-0329:** sáu ca DTMF và ca không nhập phím đã được kiểm bằng đầu SIP tự động,
không cần nghe lại. Timeout10s đã tái hiện dưới tải; S5 còn thiếu máy đích.
Xem [W-0329](../W-0329/README.md); phần dưới giữ nguyên lịch sử các lượt W-0323.

Trước vá có 5 cuộc điều khiển đủ DTMF: Nam tên dài, Bắc/Trung mỗi miền hai ca. Cuộc Nam nhiều
món nhận phím 1 trước automation được giữ riêng ở `uncontrolled/`, không tính vào ma trận tự động.
Sau vá cũng có một cuộc Nam nhiều món ngoài ma trận nhận phím 1 trước automation; đã hỏi lại
nguồn phím. Không dùng kết quả confirmed của cuộc đó để chứng nhận quyền điều khiển DTMF.

Đã chạy đủ **sáu ca trên image cuối**, nhưng chỉ **3/6 đủ bằng chứng tự động**:

| Ca sau sửa | Kết quả kiểm |
| --- | --- |
| Nam, tên dài | PASS, automation gửi 0, cancelled |
| Bắc, nhiều món | PASS, automation gửi 1, confirmed |
| Trung, nhiều món | PASS, automation gửi 1, confirmed |
| Nam, nhiều món | Nhận 1 khi automation chưa gửi, chưa xác minh |
| Bắc, tên dài | Nhận 1 khi automation chưa gửi 0, chưa xác minh |
| Trung, tên dài | Automation gửi 0 nhưng kết quả nhận 1/confirmed, chưa xác minh nguồn phím |

Ba ca PASS có đúng giọng/miền được lưu, 7 media theo thứ tự có hash khớp phép đo, thời gian
active đủ phát audio và DTMF do automation gửi sau audio + 2 giây. Ba ca còn lại bị loại rõ
trong `local-results.json`; chưa gọi ma trận là PASS. Tạm dừng lặp các ca bị phím ngoài can thiệp
để chờ xác định nguồn phím. Tổng lượt này có 13 cuộc lab thật (6 trước vá, 7 sau vá) và một task
khởi động lạnh có hai attempt technical, không quay số. Không xóa/sửa kết quả lỗi trong database.

Ảnh chụp cuối không còn active call/channel. Owner đã chốt chất lượng bản sửa cuối;
quyết định này không thay thế ba kết quả DTMF còn chưa xác minh.
Rollback whole-script đã chạy ở W-0321; không nhận bằng chứng đó là rollback trên image cuối W-0323.

## Chạy lại trên máy đích

Gói local `.artifacts/W-0323/target-probe.zip` gồm probe, 12 câu synthetic đã xuất bằng renderer
thật, 12 WAV catalog và image ID cuối. Không chứa model weights, secret hoặc dữ liệu khách thật.
Import đúng image, cung cấp model bundle/voice manifest đã verify rồi chạy PowerShell:

```powershell
.\Run-TargetProbe.ps1 -ModelRoot <bundle> -VoiceAcceptanceManifest <owner-manifest.json> -OutputDirectory <thu-muc-moi> -Repeat 3
```

Probe không mạng, không gọi điện; ghi thông số host, latency từng request, duration và hash PCM.
Đây là cơ sở đo cold/warm tuần tự; còn phải kiểm tải đồng thời, CPU/memory limit thực tế và
mirror trên máy đích. Không đưa credential vào artifact. Chưa có thông tin truy cập máy đích để chạy.

Các chốt còn mở: cold-start/timeout và S5 hardware/load; model Legal;
mirror nội bộ; xác minh ba ca DTMF bị phím ngoài can thiệp. Gọi khách thật chưa bật.

CI config, docs, model provenance, voice bindings và Helm guards PASS. Regression lab gồm
1 ca hợp lệ/17 từ chối/4 entry guards/12 payload cases; image cuối 20 tests + HTTP contract PASS.
PII scan gói này PASS. GitNexus detect-changes LOW/0 affected processes trên diff chung;
helper/test Python mới chưa có trong graph, nên đã đọc caller trực tiếp và kiểm runtime bổ sung.
Scope bàn giao `CODE_DONE`; nghiệm thu lab đầy đủ và production chưa được đóng.

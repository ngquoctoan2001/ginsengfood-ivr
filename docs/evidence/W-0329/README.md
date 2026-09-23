# W-0329 — DTMF tự động và timeout VieNeu dưới tải

Ngày `2026-09-21`; baseline `main@3e06c17`, checkout dùng chung. Kết quả ràng đúng image
và checksum source/artifact trong [local-results.json](local-results.json), không nhận là
hosted CI hay clean-commit proof. **REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.**

## Kết quả

- Ba giọng/câu ghép vẫn **OWNER_ACCEPTED**, không yêu cầu nghe lại.
- DTMF: sáu ca Bắc/Trung/Nam × đơn nhiều món/phím 1 và tên dài/phím 0 được đối chiếu với
  trace nhận phím, kết quả database, voice, bảy media đúng hash/thứ tự và RTP đủ audio.
  Ca không gửi phím kiểm kết quả `IVR_NO_ANSWER_FINAL` theo policy một attempt.
- **Timeout 10 giây đã tái hiện ở worker dưới tải local**, trước khi quay số. Retry nhận
  `TTS_PROVIDER_HTTP_ERROR` khi sidecar còn bận. Cả hai attempt đều không tính lượt khách;
  task chuyển `HELD_ADMIN_REVIEW`, rồi tự `WINDOW_EXPIRED` khi hết cửa sổ; không sửa/xóa lỗi.
- **Chưa đo S5:** Owner chưa xác định host và tài nguyên máy đích. Không dùng số local để đóng S5.

## DTMF: đầu SIP im lặng, không phụ thuộc thao tác người

`deploy/lab/run-headless-dtmf.py` dùng cùng image Asterisk lab, tạo peer không mở host port
và không có thiết bị âm thanh. Bí danh worker vẫn là `LAB-A`; AOR outbound tạm trỏ riêng
tới peer trong network lab. Peer nhận audio qua RTP, đợi đủ thời lượng + 2 giây rồi gửi
RFC4733 bằng `SendDTMF`. Không tiêm event ARI, sửa DTMF trong database hoặc ghi âm.
Tham chiếu cấu hình: [Asterisk PJSIP](https://docs.asterisk.org/Latest_API/API_Documentation/Module_Configuration/res_pjsip/).

`Invoke-FreeSoftphoneCall.ps1 -HeadlessPeer -NoUi` kiểm endpoint chỉ dùng `LAB-A-AUTO` và
đúng một contact `Avail` trước intake. Chế độ MicroSIP thường vẫn giữ kiểm đăng ký riêng.
Regression đỏ trước bổ sung tham số; sau sửa kiểm cả peer hợp lệ và route MicroSIP bị từ chối.
Selftest: 1 valid, 17 refusals, 4 entry guards, 12 order variants và 2 headless route guards PASS.
Không đổi code .NET, model, giọng, audio hoặc image VieNeu.

Harness khôi phục nguyên file PJSIP, kiểm checksum và AOR trở về `LAB-A`, rồi xóa đúng peer
của test. Không xóa dữ liệu/volume. Backup có credential chỉ nằm trong `.artifacts` ignored;
evidence tracked chỉ chứa checksum và metadata cần thiết.

Các lần thử công cụ được giữ riêng: lần đầu chưa reload đúng module nên không tạo task;
smoke tiếp theo gọi thành công nhưng assertion nhầm nơi ghi trace nên không tính PASS;
ma trận cũ đạt 5 ca rồi bị guard đăng ký MicroSIP chặn trước intake. Sau sửa guard, bốn ca
Bắc/Trung đạt; ca Nam nhiều món gặp timeout TTS khi probe tải chạy đồng thời nên bị loại
khỏi PASS DTMF. Hai ca Nam và ca không phím chạy riêng sau probe. JSON lưu từng task và
nguồn evidence, không che lần lỗi hay gọi kết quả này là một lượt sạch duy nhất.
Nguồn phím ngoài dự kiến ở các cuộc MicroSIP W-0323 cũ vẫn **chưa được chứng minh**.
Tổng lượt W-0329 có 13 cuộc SIP im lặng, trong đó 7 cuộc thuộc bộ kiểm cuối; một task lỗi
TTS có hai attempt kỹ thuật chưa quay số. Kết thúc: 0 active call/channel, peer test đã xóa,
readiness/media preflight PASS, image VieNeu giữ nguyên.

## Tải và phục hồi: đã có số đo, chưa có ngân sách production đạt

Máy local i7-11700, 8 core/16 logical CPU, khoảng 32 GiB RAM; Docker 16 CPU/15,54 GiB.
Không đặt quota CPU/RAM riêng; ONNX/BLAS/OpenMP/MKL mỗi pool 1 thread, capacity sidecar 1.
Host dùng chung có tải khác; probe chạy trùng một phần với ma trận SIP. Không quy lỗi cho
một tiến trình cụ thể, không gọi đây là phép đo tải cô lập hoặc benchmark throughput dài hạn.

Probe dùng model thật trong đúng image prefix `79e9106ec140`, network none, mount model
read-only; HTTP chạy loopback trong container, không có kết nối telephony hoặc phát/lưu audio.
49 POST synthesize gồm 18 đoạn tuần tự, 28 request burst và 3 request kiểm disconnect/recovery.

| Phép đo | Kết quả |
| --- | --- |
| 18 phần động, diagnostic timeout 60s | p50 1373 ms; p95/max 6270 ms; 2 phần vượt 5s |
| Burst 1/2/4 client, timeout 5s (14 POST) | 3 thành công, 8 HTTP503, 3 client timeout |
| Burst 1/2/4 client, timeout 10s (14 POST) | 6 thành công, 8 HTTP503, 0 client timeout; max thành công 6666 ms |
| Ép client ngắt ở 50ms | Inference tiếp tục; đợi thêm 6361 ms để capacity được trả lại |
| POST khi đang bận → GET trên cùng client | 503 có Connection: close → readiness 200, không còn lỗi HTTP400 cũ |
| Synthesize sau trả capacity, diagnostic timeout 60s | Thành công nhưng mất **11674 ms**, vượt cả 10s |
| RAM đỉnh của process probe | 1582020 KiB (xấp xỉ 1,51 GiB RSS), không phải tổng RAM host |

HTTP503 là request bị từ chối, **không phải thành công**. Client timeout không hủy inference;
request sau chỉ có chỗ khi inference cũ kết thúc. Readiness200 chứng minh model sẵn sàng,
không hứa còn capacity. 28 PCM thành công đều khớp bản Owner đã chốt.

Đồng thời, worker lab dùng timeout10s đã có một `TTS_TIMEOUT`; retry khi còn bận bị503 và
chuyển review. Không có channel/playback ở task đó, cả hai attempt `started_at=null`,
`counted=false`. Đây là bằng chứng trực tiếp giới hạn10s chưa đủ trong điều kiện đã quan sát,
không chỉ suy đoán từ probe. Production vẫn giữ5s và tắt; không tự tăng timeout hoặc concurrency.

## Bước tiếp theo

1. Owner xác định máy S5 và quota CPU/RAM/container thật. Chạy
   `deploy/lab/Measure-VieNeuLoad.ps1` trên máy đó; bộ `.artifacts/W-0329/s5-load-probe.zip`
   đã kèm fixture synthetic/checksum, không có model/credential. Scope mặc định LOCAL_LAB;
   S5_TARGET cần xác nhận tường minh. Đo cold/warm và tải kéo dài theo nhu cầu thực tế.
2. Dựa trên S5, chọn ngân sách tổng hợp trước quay số và backoff/admission phù hợp capacity1.
   Kiểm retry không đâm ngay vào inference chưa kết thúc; không nâng concurrency khi chưa đo.
3. Chốt mirror và S2 bản quyền model/codec. Image TTS không đổi so với W-0326 nên dùng đúng
   scan đã ràng image đó (0 HIGH/0 CRITICAL); không tuyên bố có scan mới ở lượt này.

Quyết định nghe đã hoàn tất. Gọi khách thật tiếp tục tắt.

### Tiếp nối W-0331 — 21/09/2026

[W-0331](../W-0331/README.md) đã thêm retry503 có giới hạn trong cùng timeout, kiểm bằng
model thật và cập nhật worker lab. Smoke warm/phím1 đạt; lượt đầu sau khởi động lại vẫn
timeout10s trước dial. Có sửa harness giữ route qua retry. Số đo W-0329 ở trên vẫn giữ
nguyên; ở thời điểm đó S5 chưa xác định host, ngân sách production chưa được chứng minh.

### Tiếp nối W-0333 — kết quả máy đích

[W-0333](../W-0333/s5-findings.md) đã đối chiếu raw từ vps61, đúng image/model và quota
2 CPU/4 GiB. Có phép đo TTS thật đầu tiên; các bước xác định host/thu số đo ban đầu ở trên
đã xong. Chưa đóng S5: còn kiểm worker retry, đủ đơn và tải kéo dài. Timeout 5 giây không
đủ các mẫu dài, 10 giây mới là kết quả một lượt probe. Mirror/S2 và khách thật giữ nguyên.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Bằng chứng chạy lab: Lượt kiểm lab DTMF qua đầu SIP im lặng và đo tải VieNeu bằng model thật; chỉ thêm công cụ lab, không đổi mã ứng dụng, test .NET hay script gate, nên bằng chứng là log và số đo lab. Danh sách 6 tệp nằm trong khai báo.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

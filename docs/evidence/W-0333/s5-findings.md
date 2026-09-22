# S5 vps61 — kết quả đối chiếu lượt đo đầu tiên

Ngày `2026-09-21`. **Đã nhận và kiểm file gốc; production vẫn BLOCKED,
REAL_CUSTOMER_CALL_ALLOWED=NO.** Owner chạy trên server, agent phân tích archive được
chép về; không truy cập SSH. Đây là phép đo TTS độc lập, chưa phải worker/SIP end-to-end.

## Kết luận để quyết định bước tiếp theo

Budget5s không đáp ứng các phần lời thoại dài đã đo. Budget10s hoàn tất các request
được nhận trong lượt này nhưng phần chậm nhất9.272s, chưa có biên đủ để chứng nhận
production trên VPS dùng chung. Giữ2CPU/4GiB/capacity1 làm profile chẩn đoán tiếp;
ưu tiên cách xếp hàng và chờ model rảnh, chưa có bằng chứng quota CPU/RAM là nút thắt.

Không yêu cầu nghe lại: cả25 PCM thành công khớp checksum voice/fixture đã duyệt.
Timeout không giải phóng inference ngay; cần kiểm bằng client worker thật trước khi
nâng tải hoặc chốt timeout. Không sửa config runtime hoặc code trong lượt phân tích này.

## Phạm vi và nguồn

Archive `.artifacts/W-0333/s5-vps61-result.tar.gz` có5 file thường và1 thư mục, tổng nhỏ
hơn1MiB. Đọc theo danh sách tên cho phép; không thực thi nội dung hoặc giải nén symlink.
Kết quả máy và hash từng file: [s5-target-results.json](s5-target-results.json).

`host.json` khớp manifest của bundle đã gửi và đúng image index prefix `79e9106ec140`,
config prefix `13aae53fd114`; log nạp image khớp. So lại25 PCM với approved-audio độc lập,
tính lại percentile, số timeout và cgroup; các kết quả khớp báo cáo của launcher.
Image không đổi; scan đúng image ở W-0326 còn là nguồn scan, không nhận có scan mới.

Host và Docker cùng tên `vps61`, cùng10vCPU và14669688832 bytes RAM (13.66GiB).
Cgroup của probe xác nhận2CPU/4GiB, capacity1, ONNX threads1. Toàn lượt khoảng184.637s.
Containerstats ghi47.84GiB, nhưng cả7 container nghiệp vụ có `HostConfig.Memory=0`:
không chứng minh có giới hạn tường minh47.84GiB. Nguyên nhân con số này chưa xác định;
không dùng nó thay RAM host hoặc bỏ guard. Phép đo của mình có giới hạn4GiB thật.

## Các phép đo

| Phần đo | Kết quả |
| --- | --- |
| Nạp model tới ready | 4379ms; chỉ một process mới, không phải phân bố cold-start |
| 18 phần động, diagnostic budget60s | p50=1920ms; p95/max=9272ms; 5 vượt5s; 0 vượt10s |
| Burst1/2/4 client, budget5s, tổng14POST | 6 timeout, 8 bị503; 0 thành công |
| Burst1/2/4 client, budget10s, tổng14POST | 6 thành công, 8 bị503; 0 timeout; max thành công9019ms |
| Hết timeout5s | Inference còn chạy thêm1535–4161ms mới trả capacity |
| Ép ngắt client ở50ms | Client dừng52ms; chờ thêm8871ms để trả capacity |
| Request sau khi capacity được trả | Thành công9060ms; PCM đúng |
| RAM RSS đỉnh của process | 1584464KiB, khoảng1.51GiB; không phải toàn bộ RAM container/host |
| Cgroup quota throttling | 0 period bị throttle, 0 microsecond throttle |

Số POST tổng49 =48 dòng request được lưu +1 request kiểm busy riêng. Có25 thành công,
17 HTTP503 và7 timeout, trong đó1 timeout50ms được cố tình tạo. Không gọi503 là thành
công, không gộp timeout cố ý vào lỗi budget5s. GET readiness200 chỉ chứng minh model
ready, không hứa còn chỗ. Log có cảnh báo ONNX PCI discovery; không có bằng chứng nó
gây lỗi trong lượt này, không gán nguyên nhân chậm cho cảnh báo đó.

Sáu request được nhận ở nhóm5s đều timeout, kể cả burst1 client: quá chậm không chỉ do
đụng request khác. Khi gửi đồng thời, capacity1 nhận một request và từ chối phần còn lại
bằng503 như thiết kế. Điều này không đồng nghĩa hệ thống chỉ có thể giữ một cuộc SIP.
Probe không chạy retry C# W-0331; cần kiểm sự phối hợp giữa worker và sidecar riêng.

## Liên hệ với toàn bộ lời thoại và code worker

`SpeechSynthesisService.SynthesizeSegmentedAsync` chờ từng phần động theo thứ tự;
timeout cấu hình được áp **cho từng lần tạo phần động**, không phải một deadline chung
của cả đơn. `AsteriskSchedulerDispatchGateway` chờ tạo đủ lời thoại rồi mới dial.
Vì vậy, thời gian tạo nội dung nên nằm trước lúc khách bắt máy, không chèn vào giữa câu.

| Giọng | Tổng3 phần động: nhiều món | Tổng3 phần động: tên dài |
| --- | ---: | ---: |
| Bắc | 8706ms | 7645ms |
| Trung | 9936ms | 9118ms |
| Nam | 15578ms | 11954ms |

Các tổng trên là cộng số đo từng phần, **không phải đo worker end-to-end**; chưa cộng
queue/backoff, IO/cache và quay số. Cache hit có thể giảm thời gian. Không dùng con số
max9272ms của một phần để hứa cả đơn sẵn sàng trong10s.

Code client W-0331 retry503 tối đa8 lần, chờ250/500/1000/1000/1000/1000/1000/1000ms,
tổng6750ms cộng thời gian HTTP. Khoảng bận8871ms đã đo có thể dài hơn trần này. Đây là
suy luận đối chiếu source với trace; **chưa tái hiện bằng client .NET trên S5**. Chỉ tăng
timeout mà giữ trần retry như cũ có thể vẫn hết lượt trước khi sidecar rảnh.

CPU cgroup tiêu thụ khoảng 173.885s trong khoảng 184.637s host đo (xấp xỉ 0.94 core bình
quân; hai khoảng đo có ranh giới khác nhau). Không có quota throttling và ONNX đang
dùng1 thread: chưa có căn cứ rằng chỉ tăng quota2→4CPU sẽ rút ngắn request. Nếu thử
threads/engine khác sau này phải đo riêng và giữ kiểm checksum, không đổi giọng đã duyệt.

## Bước tiếp theo cụ thể

1. Kiểm client worker với tình huống busy còn~9s, timeout/cancellation, nhiều đơn cùng
   chuẩn bị và phần động dài. Điều chỉnh giới hạn retry/điều phối theo deadline có giới
   hạn; xếp hàng tạo tiếng để tránh các đơn tranh một slot model. Không tăng concurrency
   của model hoặc tự cộng lượt khách khi request chưa quay số.
2. Đo client đó trên S5, gồm đủ3 phần động/đơn trước dial, cold/warm nhiều lần và tải
   kéo dài. Giữ profile2CPU/4GiB/capacity1 để so sánh; chọn timeout thử nghiệm rõ ràng,
   sau đó mới quyết định timeout vận hành. Chưa có số cuộc đồng thời mục tiêu từ owner.
3. S5 đã có phép đo thực tế đầu tiên nhưng chưa đóng toàn bộ gate. S2 pháp lý,
   mirror và phê duyệt vận hành vẫn mở. Khách thật tiếp tục tắt.

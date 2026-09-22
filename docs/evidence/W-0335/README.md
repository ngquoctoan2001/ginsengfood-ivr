# W-0335 — Xếp hàng tạo đủ lời thoại và bộ đo worker S5

Ngày 21/09/2026. **Bản sửa, kiểm local và hai lượt đo S5 đã xong: 122/122 đơn đạt.**
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED; S2/mirror còn mở.**
Giọng và câu ghép giữ nguyên, không yêu cầu nghe lại.

Kết quả máy đích mới nhất: [s5-findings.md](s5-findings.md),
[s5-target-results.json](s5-target-results.json). Giữ quota 2 CPU/4 GiB; tổng soak
905,133 giây, 84 đơn, không lỗi. Một phần động mất 10,099 giây; recovery sau ngắt
client mất tới 19,352 giây/phần với budget 30 giây. **Timeout vận hành chưa chốt.**
Các số local dưới đây và `local-results.json` là bản ghi trước khi nhận kết quả S5.

## Kết quả và giới hạn

Candidate gồm `git archive` baseline `f2a7bc8` cộng đúng bốn file speech và hai file test.
Không tạo branch, commit, push hoặc triển khai vào worker đang vận hành. Các WIP khác
trong checkout không được đưa vào candidate. Probe dùng renderer và dịch vụ speech .NET
thật, không dựng scheduler, DB, SIP hoặc dial token.

- Trước sửa: 3 regression lỗi, 1 ca đối chứng đạt. Sau sửa: **232/232** ca speech,
  normalization và traceability đạt; build không warning/error. Có 6 TestId mới:
  `UT-TTS-BUSY-06`, `UT-TTS-QUEUE-01..05`.
- Runner có 3 guard tests đạt. Bộ phân tích đối chiếu source, DLL, quota và checksum.
- Model thật local: **31/31 đơn**, đủ 7 phần/đơn, PCM từng phần động và cả lời thoại
  khớp bản duyệt; 72 lần tạo tiếng. Không phát audio hoặc gọi điện.
- Chỉ 6 HTTP 503 trong ca cố tình ngắt client, sau đó tự phục hồi. Các ca burst/soak
  không tranh slot model; trace xác nhận không có hai lệnh provider chạy chồng nhau.
- TTS cgroup giữ **2 CPU/4 GiB**, capacity 1, ONNX 1 thread. RSS tiến trình đỉnh
  khoảng 1.505 GiB; không ghi nhận quota throttling. Client đo riêng 0.5 CPU/512 MiB.

| Phép đo local cuối | Kết quả |
| --- | --- |
| 6 đơn, bắt đầu từ cache rỗng, timeout 10 giây/phần | Đủ 6; tổng từng đơn 2.814–7.511 giây |
| 6 đơn cache ấm | 7–8ms/đơn; không gọi provider |
| 2 đơn đến cùng lúc, không cache | Cả hai đạt; đơn cuối hoàn tất 7.936 giây |
| 4 đơn đến cùng lúc, không cache | Cả bốn đạt; đơn cuối chờ khoảng 12.778 giây tới provider và hoàn tất sau 16.898 giây |
| Ngắt client rồi tạo đủ đơn | 6 lần 503 rồi phục hồi; đủ đơn sau 12.838 giây |
| Tải liên tục, không cache, 2 đơn/lượt | 12/12 đơn trong 62.811 giây; đơn lâu nhất 14.860 giây gồm cả chờ |

Cache rỗng là trạng thái đầu pha: hai mẫu cùng vùng có thể chia sẻ phần địa chỉ nên
pha đầu có 15 request động. Burst/soak bỏ cache có đúng 3 request động cho từng đơn.
Đây là số đo **toàn bộ chuẩn bị lời thoại trước dial**, không phải thời gian cuộc gọi.
Thời gian tới provider đầu tiên bao gồm chờ admission và ít chi phí chuẩn bị; không
đồng nhất tuyệt đối với bộ đếm queue nội bộ. Số local không thay số đo trên vps61.

## Thay đổi code

`SpeechSynthesisService` giữ một lượt FIFO cho cả đơn trên mỗi worker dùng external TTS.
Mặc định tối đa 8 đơn đang chờ, chờ tối đa 30 giây. Hết hàng chờ hoặc hết thời gian
báo `TTS_QUEUE_FULL`/`TTS_QUEUE_TIMEOUT` trước dial. Hủy caller hoặc hết hạn xác nhận
bao phủ cả hàng chờ và tạo tiếng; slot được trả khi lỗi, hủy hoặc hoàn tất.
Thời gian chờ không tiêu budget tạo từng phần. Nhánh MOCK không dùng hàng chờ này.

`ConfigurableExternalTtsProvider` tiếp tục retry riêng HTTP 503 trong **cùng deadline**,
backoff 250/500/1000ms rồi giữ tối đa 1000ms. Bỏ trần 9 POST vốn có thể hết trước khoảng
bận 8.871 giây đã đo ở S5. Không reset deadline hoặc retry lỗi khác; đọc body cũng dùng
deadline đó. Test giữ busy hơn 9 giây mới trả audio đã đạt.

Sửa thêm việc `SynthesizeProviderAsync` gộp cancellation của caller thành lỗi provider.
Giờ tín hiệu hủy được giữ nguyên; đơn hết cửa sổ báo `TTS_CACHE_WINDOW_EXPIRED`, không
trả playlist thiếu hoặc tiếp tục dial. Không đổi số lượt khách hoặc quy tắc DTMF.

Impact: request HTTP LOW (1 caller), service MEDIUM (7 tham chiếu trực tiếp/26 symbol),
validator MEDIUM. Điểm cancellation LOW (2 caller, 2 flow dispatch). Queue/test/probe mới
chưa index; source được kiểm trực tiếp. Detect-changes cuối LOW trên toàn checkout WIP,
chỉ dùng advisory; không chứng nhận toàn bộ checkout hoặc hosted CI.

## Gói S5 và provenance

Hướng dẫn copy/run/return: [ubuntu-steps.md](ubuntu-steps.md).
Gói `.artifacts/W-0335/vieneu-worker-s5.tar.gz` khoảng 62 MiB, 62 file có checksum,
file checksum ngoài ASCII/LF. Dùng lại bundle W-0333 của vps61, không tải lại model.
Model/TTS image giữ nguyên W-0326; runtime .NET image giữ đúng W-0331, entrypoint chạy
probe DLL mới. Không có image build hoặc scan CVE mới ở lượt này.

Probe sử dụng timeout 10 giây/phần để so sánh; 15 giây/phần cho burst/soak; 30 giây/phần
cho ca ngắt client. Chờ queue tối đa 90 giây trong probe. Các budget chẩn đoán này **không
đổi production 5 giây/lab 10 giây**, không coi local đạt là phê duyệt budget vận hành.
Rate budget trong probe được nâng riêng để đo engine, không đổi cấu hình worker.

Lượt negative-control đầu bị cache MSBuild giữ DLL mới do copy source có timestamp cũ;
không tính lượt đó là bằng chứng trước sửa. Đã rebuild tường minh: 3 lỗi/1 đạt trước sửa,
232 đạt sau sửa. Một lượt model đầu cũng có 31 đơn đạt nhưng chưa dùng để bind gói cuối.
Do HEAD checkout chung đổi giữa các build, metadata `SourceRevisionId` được ghim baseline;
DLL Infrastructure/Domain/Contracts của lượt test cuối và probe cuối có hash giống nhau.
Hash source, DLL, gói và raw cuối: [local-results.json](local-results.json).

## Việc còn lại

Owner đã chạy và trả raw: **2 lần khởi động model mới, mỗi lần ít nhất 450 giây soak**.
Đã đối chiếu manifest/source/DLL, log và kết quả trên vps61. SSH BatchMode vẫn chưa có
quyền; kết luận dựa trên gói owner gửi, không nhận là đã truy cập trực tiếp server.
Script giới hạn tài nguyên, không có cổng công khai hoặc mount Docker socket.

Hàng chờ hiện là trong một process; chưa chứng minh nhiều worker chung một sidecar.
Budget cuối phải xử lý cả khoảng model còn bận sau khi client hủy; bài đo recovery dùng
30 giây, không chứng nhận budget soak 15 giây cho tình huống đó. Tích hợp worker/scheduler,
DTMF tự động với bản sửa và nghiệm thu vận hành trên máy đích vẫn còn mở, cùng S2/mirror.
Khách thật tiếp tục tắt.

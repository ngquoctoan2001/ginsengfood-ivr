# W-0338 — Deadline VieNeu và worker/SIP/DTMF tự động

Ngày 22/09/2026. **REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED; S2/mirror OPEN.**
Giọng/câu ghép đã được owner duyệt; toàn bộ kiểm tra ở đây không yêu cầu nghe lại.
**Đã hoàn tất sửa và kiểm local: 364 tests, 25 đơn model thật, 8 cuộc SIP tự động đạt.**

## Deadline và bản chạy

Chính sách lab: **30 giây/phần, 90 giây chờ FIFO, 120 giây tổng chuẩn bị, 8 chỗ chờ**.
Tổng 120 giây bắt đầu trước admission, bao gồm chờ model cũ sau hủy, mọi HTTP 503/backoff
và cả ba phần động. Không reset khi đến lượt hoặc đổi phần. Hạn đơn sớm hơn luôn thắng;
hủy caller giữ nguyên loại cancellation. Không trả playlist sau khi deadline đã hết.
Lease lab 360 giây để bao phủ chuẩn bị, audio tối đa 120 giây, DTMF và dọn cuộc gọi.

`PreparationTimeoutMilliseconds` mặc định 120 giây, được kiểm giới hạn khi startup.
Profile lab chủ động chọn 30/90/120; mặc định timeout từng phần của ứng dụng vẫn là 5 giây.
Whole-script rollback giữ 60 giây/request và trần tổng 120 giây. Không đổi cấu hình production.
Chi tiết công thức và kiểm độc lập trước lượt runtime: [deadline-policy-review.md](deadline-policy-review.md).
Đó là bản ghi lịch sử trước kết quả runtime dưới đây, không phải bằng chứng của image cuối.

Candidate: `git archive` baseline `3131d9d` cộng overlay speech/test/probe được ghi hash.
Checkout chung có task khác commit trong lúc làm; không lấy HEAD mới để nhận bằng chứng.
Image worker cuối `1d4f05d5e284`, TTS giữ image `79e9106ec140`; quota TTS **2 CPU/4 GiB**,
capacity 1, ONNX 1 thread. Tests/publish/probe và hai DLL lấy từ worker đang chạy khớp hash.
Hash đầy đủ và raw bindings: [results.json](results.json).

## Kiểm chứng

| Lớp kiểm | Kết quả |
| --- | --- |
| Regression deadline trước sửa hành vi | 3 fail, 4 pass; options/validation đã thêm để test biên dịch |
| Binary cuối: speech, telephony, scheduling, normalization, traceability | 364/364 PASS, không bỏ qua; build 0 warning/error |
| Profile/preflight guards | 2 profile hợp lệ, 17 refusal, 4 entry guards, 12 order fixtures PASS |
| Runner offline guards | 3/3 PASS |
| Probe model thật, cùng deadline 30/90/120 | 25/25 đơn đủ 7 phần; PCM được probe kiểm khớp; 54 provider calls |
| Probe cố tình ngắt client | 15 HTTP 503 rồi phục hồi; phần lâu nhất 28,185 giây; toàn đơn 40,603 giây |
| Probe tải không cache, 2 đơn mỗi đợt | 6/6 đơn trong 99,309 giây; đơn lâu nhất 48,979 giây gồm cả chờ |
| Worker/scheduler, cố tình timeout TTS trước dial | Lỗi `TTS_TIMEOUT`, counted=false; retry cùng attempt số 1 rồi `IVR_CONFIRMED`, counted=true |
| SIP ba giọng, nhiều món/tên dài, DTMF 1/0 và không nhập | 7/7 PASS: 3 confirmed, 3 cancelled, 1 no-answer final; đủ 7 media/cuộc |
| Trivy đúng archive image worker cuối | 0 HIGH/0 CRITICAL, offline DB cập nhật 20/09; không refresh database |

TestId mới `UT-TTS-DEADLINE-01..05` có trong traceability. Bài regression chứng minh
deadline không reset giữa các phần, hàng chờ tiêu budget tổng, hạn đơn/caller giữ đúng nghĩa,
provider bỏ qua cancellation không thể trả playlist và startup từ chối budget ngoài biên.

Harness kiểm route chỉ trỏ peer SIP im lặng, gửi RFC4733 sau toàn bộ audio và kiểm bảy media
đúng thứ tự/hash reference. Nó đọc attempts/results trong PostgreSQL, không sửa các hàng đó.
Ca fault pause riêng TTS để request hết 30 giây; unpause ngay khi thấy lỗi kỹ thuật trước dial,
giữ peer qua retry, rồi kiểm đúng hai attempts nhưng chỉ một lượt khách được tính.

Cộng ca fault là **8 cuộc**, 9 attempt rows, 8 lượt khách được tính; technical retry
không tăng attempt number. Phím 1/0 được gửi qua SIP/RTP RFC4733, mỗi cuộc đúng một
DTMF end event sau đủ audio; ca không nhập không có sự kiện phím. Không phát ra MicroSIP.
Sau chạy: **0 active calls/channels, 0 job còn có thể dispatch**, peer riêng đã xóa,
PJSIP khớp checksum trước chạy; worker/TTS/API đang chạy, không paused, cờ khách thật NO.
Probe RSS đỉnh khoảng 1,501 GiB; không OOM. Có khoảng 85 ms CPU quota throttling trong
lượt local này, khác với lượt S5 W-0335 không ghi nhận throttle; không gộp hai số đo.

## Giới hạn và bước tiếp theo

- Lượt này là **local lab**, không phải đo mới trên vps61. W-0335 S5 đã đo profile hỗn hợp
  10/15/30 giây; không dùng kết quả đó để chứng nhận profile cuối 30/90/120.
- Gói [hướng dẫn S5](ubuntu-steps.md) đã chuẩn bị, checksum ASCII/LF, dùng lại model W-0333.
  Không cần owner nghe lại. Gói đo chỉ dựng speech probe, không dựng scheduler/SIP trên S5.
- Probe dùng request budget cao hơn worker và client riêng 0.5 CPU/512 MiB; chỉ deadline,
  model/quota TTS và dịch vụ tạo tiếng là phạm vi so sánh. PCM là kết quả kiểm của probe.
- Mốc 28,185 giây gần trần 30 giây cho thấy đây là giới hạn xử lý, không cam kết mọi tải sẽ đạt.
  Chưa chứng minh nhiều worker chung sidecar hoặc sizing production.
- Scan đúng image mới nhưng database CVE không mới tại thời điểm chạy. S2/mirror, quét lại
  với database mới, nghiệm thu máy đích và full acceptance sweep tại commit sạch vẫn còn mở.

Lượt đầu quét lỗi vì cache phân tích Trivy bị mount chỉ đọc; đã dùng cache tạm writable,
database gốc vẫn readonly và network none. Build probe đầu có hash DLL khác do metadata
checkout chung; build cuối tắt truy vấn source-control/SourceLink và ghim SourceRevisionId,
chạy lại 364 tests rồi xác nhận DLL đồng nhất trước runtime. Không lấy binary lệch hash làm
bằng chứng của bản cuối. **Khách thật tiếp tục tắt.**

Hai lượt ma trận đầu bị guard báo lỗi lệnh Docker trước intake: lượt đầu đã khôi phục
route, lượt hai dừng trước khi đổi route. Giữ log và kết quả riêng; không tính là cuộc gọi
hoặc PASS. Kiểm lại từng bước Docker, readiness và media đã đạt, giữ nguyên guard rồi
chạy ma trận mới ở `dtmf-matrix-r3`. Chưa xác định được nguyên nhân gốc của lỗi Docker
thoáng qua; không gán nó thành lỗi TTS hay âm thầm tăng timeout preflight.

# W-0338 — Chốt deadline và hàng chờ cho lab VieNeu

Ngày 22/09/2026. Phạm vi: mục 1 owner yêu cầu, dựa trên số đo W-0335.
**REAL_CUSTOMER_CALL_ALLOWED=NO.** Đây là chính sách cấu hình cho lượt lab tiếp theo;
chưa phải nghiệm thu vận hành, cấu hình production hoặc kết quả worker/DTMF.

## Chính sách được chọn

| Giới hạn | Giá trị | Cách tính |
| --- | --- | --- |
| Một phần động | 30 giây | Bao gồm chờ HTTP 503, backoff, inference và đọc audio; retry không reset đồng hồ |
| Hàng chờ | 8 đơn đang chờ, ngoài 1 đơn được phục vụ | FIFO, giữ lượt cho toàn bộ lời thoại của một đơn |
| Chờ đến lượt | Tối đa 90 giây | Đồng thời tiêu thời gian trong giới hạn tổng 120 giây |
| Toàn bộ chuẩn bị | Tối đa 120 giây | Từ lúc bắt đầu admission, gồm hàng chờ và mọi phần tiếng; không reset khi đến lượt hoặc chuyển phần |
| Hạn của đơn | Ưu tiên hạn sớm hơn | Dừng khi hết cửa sổ xác nhận, kể cả chưa dùng hết 120 giây |
| Hủy từ caller | Dừng ngay khi nhận tín hiệu | Trả lượt cho đơn sau; không nhận playlist thiếu làm thành công |
| Tài nguyên TTS lab | 2 CPU, 4 GiB, capacity 1, ONNX 1 thread | Giữ đúng mức đã đo ở W-0335 |

Gọi `t0` là lúc bắt đầu admission, `E` là hạn xác nhận của đơn:

- Deadline tổng `D = min(t0 + 120 giây, E)`.
- Chờ hàng kết thúc chậm nhất tại `min(t0 + 90 giây, D)`.
- Phần động bắt đầu tại `s` chịu deadline `min(s + 30 giây, D)`.
- Ví dụ đã chờ 85 giây thì chỉ còn tối đa 35 giây cho **tất cả** phần tiếng.
  Không cộng thành 90 giây chờ + 3 × 30 giây tạo tiếng.

8 chỗ chờ là giới hạn bộ đệm, không cam kết 8 đơn luôn hoàn tất trước hạn. Khi đầy
báo `TTS_QUEUE_FULL`; quá hạn chờ báo `TTS_QUEUE_TIMEOUT`; quá hạn tổng báo
`TTS_PREPARATION_TIMEOUT`; hết cửa sổ đơn báo `TTS_CACHE_WINDOW_EXPIRED`.
Hủy caller giữ `OperationCanceledException`; hết hạn từng phần báo `TTS_TIMEOUT`.
Các lỗi này xảy ra trước dial, không được trả playlist thiếu như một lần tạo tiếng đạt.

Model có thể tiếp tục xử lý yêu cầu cũ sau khi client đã hủy. Đơn sau retry HTTP 503
trong cùng budget 30 giây; nếu vẫn bận đến hạn thì dừng với lỗi kỹ thuật, không kéo dài
vô hạn. Một process worker dùng một sidecar riêng là phạm vi hiện tại; chưa chứng minh
nhiều process cùng chia sẻ một sidecar bằng hàng chờ này.

## Căn cứ chọn số và cấu hình

[W-0335 S5](../W-0335/s5-findings.md) đo tối đa 10,099 giây/phần ở soak;
phục hồi sau hủy mất 19,352 giây/phần với budget 30 giây. Chọn 30 giây cho cả hai
tình huống, thay vì suy rằng budget 15 giây của soak đủ cho recovery. Các số này
không cam kết mọi tải tương lai đều đạt trong 30 giây.

Profile [speech-lab-profile.mjs](../../../deploy/lab/speech-lab-profile.mjs) đặt
`TimeoutMilliseconds=30000`, `PreparationQueueLimit=8`,
`PreparationQueueTimeoutMilliseconds=90000`, `PreparationTimeoutMilliseconds=120000`
cho segmented. Whole-script rollback giữ 60 giây cho một request, cùng trần tổng
120 giây. Profile lab còn đặt lease 360 giây; worker/DTMF phải kiểm với cấu hình đó.
Profile là overlay chủ động, không tự thay cấu hình chạy hiện tại hoặc production.

## Kiểm tra độc lập

Kiểm tại bản sao baseline `71f337e6a4db606eac1b0f2bdd71ec1eb1feb257` cộng đúng 5 file
WIP W-0338 đã chụp hash, không lấy source đang thay đổi trong checkout để build.
Đây là kiểm bản chụp WIP, không phải full test/sweep tại một commit sạch.

- **212/212 test speech đạt**, không lỗi hoặc bỏ qua. Bao gồm deadline tổng không
  reset giữa các phần, hàng chờ tiêu budget tổng, hạn đơn và caller cancellation,
  provider trả trễ sau hủy không được trả playlist, giới hạn cấu hình, FIFO và busy retry.
- Preflight self-test đạt: 2 profile hợp lệ, 17 trường hợp từ chối, 4 entry guards,
  12 biến thể đơn. Dùng fake runner; không khởi động Docker, worker hay gọi điện.
- Hash, lệnh và số đếm: [deadline-policy-verification.json](deadline-policy-verification.json).
  Raw local: `.artifacts/w0338-independent-deadline-review-20260922/`.
- Phiên kiểm độc lập không sửa source runtime/test đang được task W-0338 triển khai.

Mục 1 đã chốt cách tính cho lab. W-0338 vẫn cần hoàn thiện luồng worker/scheduler/DTMF,
kiểm lại các ca còn thiếu trên cấu hình cuối, rồi đóng băng commit để chạy test và full
gate sweep. Không dùng kết quả trên để đổi W-0338 sang ACCEPTED hoặc đóng S2/mirror.

# W-0335 — Kết quả worker speech trên vps61

Ngày 21/09/2026. **Đã hoàn tất phép đo S5 có giới hạn: 122/122 đơn, không lỗi.**
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED; S2/mirror OPEN.**
Giọng và câu ghép đã được owner duyệt; không yêu cầu nghe lại.

## Phạm vi và nguồn bằng chứng

Owner chạy hai lượt trên vps61, Ubuntu 22.04.4, Xeon Silver 4216, 10 vCPU.
TTS giữ **2 CPU/4 GiB, capacity 1, ONNX 1 thread**; client đo riêng 0.5 CPU/512 MiB.
Mỗi lượt khởi động model mới, có đơn nhiều món và tên dài, đủ ba vùng giọng.
Thời gian toàn bộ hai lượt: 09:16:06–09:36:04 UTC, khoảng 19 phút 58 giây.

Đã nhận archive 20.894 byte; kiểm đủ 13 file kết quả và hai completion marker.
Manifest trả về khớp gói W-0335, toàn bộ 62 file trong kit đúng checksum. Source,
DLL và artifact vẫn khớp [bản ghi local](local-results.json): baseline `f2a7bc8`
cộng overlay đã kiểm; không lấy HEAD đang thay đổi của checkout chung làm candidate.
Image TTS/runtime khớp pin W-0326/W-0331; không build hoặc quét image mới trong lượt này.
Hash của archive, từng file raw và helper phân tích: [s5-target-results.json](s5-target-results.json).

Probe dùng renderer và dịch vụ speech thật. Nó báo 122 đơn đủ 7 phần, PCM từng phần
động và toàn lời thoại khớp checksum đã duyệt. Gói trả về chứa kết quả kiểm, không chứa
audio để người nhận hash lại độc lập. Client log, HTTP trace và model log khớp nhau:
324 lần tạo tiếng thành công, 22 HTTP 503 trong recovery và 2 client disconnect cố ý.

## Kết quả đo

| Bài đo | Lượt 1 | Lượt 2 |
| --- | --- | --- |
| 6 đơn, bắt đầu cache rỗng; 10 giây/phần | 6/6; đơn lâu nhất 15,370 giây | 6/6; đơn lâu nhất 16,234 giây |
| 6 đơn cache ấm | 6/6; tối đa 5 ms | 6/6; tối đa 25 ms |
| 2 đơn đồng thời, bỏ cache; 15 giây/phần | 2/2; đơn cuối xong sau 16,138 giây | 2/2; đơn cuối xong sau 17,396 giây |
| 4 đơn đồng thời, bỏ cache; 15 giây/phần | 4/4; đơn cuối xong sau 35,062 giây | 4/4; đơn cuối xong sau 37,405 giây |
| Ngắt client rồi phục hồi; 30 giây/phần | 1/1; toàn đơn 25,095 giây; 11 lần 503 | 1/1; toàn đơn 25,977 giây; 11 lần 503 |
| Tải liên tục, 2 đơn/lượt; 15 giây/phần | 42/42 trong 454,821 giây | 42/42 trong 450,312 giây |
| RSS tiến trình đỉnh | 1,5069 GiB | 1,5079 GiB |
| CPU quota throttling / OOM ghi nhận | 0 / 0 | 0 / 0 |

Cả burst và soak hoàn tất đúng thứ tự gửi; trace không có hai lệnh provider chồng nhau.
Đơn cuối của burst 4 chờ tới provider tối đa 27,665 giây. Số này gồm chút chi phí chuẩn bị,
không phải bộ đếm hàng chờ nội bộ. Probe dùng queue timeout 90 giây; chưa xác nhận mọi
tải đều phù hợp queue mặc định 30 giây. Hàng chờ chỉ nằm trong một process.

Hai pha soak cộng **84 đơn trong 905,133 giây**. Thời gian hoàn tất toàn đơn, gồm chờ:
trung vị 16,254 giây, p95 28,427 giây, tối đa 29,569 giây. Với 252 phần động:
trung vị 1,991 giây, p95 8,999 giây, tối đa **10,099 giây**.
Phân vị dùng nearest rank. Đây là tải hai đơn mỗi đợt; không suy thành công suất gọi khách.
Các giá trị là thời gian chuẩn bị trước dial, không phải khoảng ngừng giữa câu khi phát.

## Điểm timeout chưa chốt

- Một phần động của đơn nhiều món giọng Nam trong soak mất **10,099 giây** với budget
  15 giây. Không có lỗi trong bài đo này, nhưng không thể dùng nó chứng nhận timeout
  lab 10 giây hoặc mặc định 5 giây đủ cho tải thực tế.
- Khi cố tình ngắt client, model vẫn xử lý yêu cầu cũ. Phần đầu của yêu cầu phục hồi
  gồm chờ busy và tạo tiếng mất **18,738 / 19,352 giây**. Nó đạt nhờ budget **30 giây**;
  chưa chứng minh recovery chạy được dưới budget 15 giây. Không gộp hai profile thành
  một kết luận “15 giây đủ cho mọi tình huống”.
- Không thấy thiếu RAM/quota throttling trong bài đo. Giữ quota hiện tại cho bước sau;
  chưa có căn cứ từ lượt này để nâng CPU/RAM. RSS là high-water của tiến trình;
  memory.current chỉ được lấy mẫu. Không có hậu kiểm trực tiếp server sau khi dọn probe.

## Việc đã làm và bước tiếp theo

Đã kiểm gói S5, đủ hai lượt, đơn nhiều món/tên dài, FIFO, tải kéo dài và recovery.
Giữ nguyên bản ghi local lịch sử; cập nhật kết quả máy đích riêng, không sửa cấu hình
vận hành, không phát audio hoặc gọi điện trong lượt tiếp nhận này.

Bước kỹ thuật kế tiếp là chốt một chính sách deadline thống nhất: tính cả chờ inference
cũ sau cancellation, hàng chờ và thời gian tạo đủ ba phần trong cửa sổ đơn; tránh để
đơn sau liên tiếp hết hạn khi model còn bận. Sau đó kiểm bản cấu hình được chọn qua
worker/scheduler thật trong lab bằng automation, gồm DTMF và technical retry không
tăng sai số lượt gọi. Chỉ đo lại ca còn thiếu với đúng cấu hình cuối, giữ 2 CPU/4 GiB.

Phép đo này không dựng scheduler, DB hoặc SIP; chưa kiểm nhiều worker chung sidecar,
chưa nghiệm thu vận hành S5. S2, mirror, quét đúng image cuối và quyền bật production
vẫn mở. **Khách thật tiếp tục tắt.**

# W-0338 — Profile deadline cuối trên vps61

Ngày 22/09/2026. **Phép đo S5 đúng profile 30/90/120 đã đạt: 128/128 đơn, không lỗi.**
TTS giữ **2 CPU/4 GiB**, capacity 1, ONNX 1 thread. Không yêu cầu nghe lại.
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED; S2/mirror OPEN.**

## Nguồn và phạm vi

Owner chạy hai lượt trên vps61, Ubuntu 22.04.4, Xeon Silver 4216, 10 vCPU;
01:41:26–02:01:32 UTC (08:41:26–09:01:32 giờ Việt Nam), khoảng 20 phút 6 giây.
Mỗi lượt khởi động model mới. Client đo riêng 0.5 CPU/512 MiB.
Profile thống nhất: 30 giây/phần, 90 giây chờ, 120 giây tổng chuẩn bị, 8 chỗ chờ.
Đây là renderer và dịch vụ speech .NET chạy trên máy đích; không dựng scheduler/DB/SIP.

Đã nhận archive 21.067 byte và kiểm đủ 12 file raw. Manifest trả về khớp gói W-0338;
62 file trong manifest đúng hash, image TTS/runtime đúng pin, base bundle W-0333 đúng hash.
Ba DLL Infrastructure/Domain/Contracts của probe khớp binary worker đã kiểm local.
Source speech/probe/test khớp candidate đóng băng; các script lab khớp hash trong hồ sơ local.
Không lấy HEAD hiện tại hoặc kết quả của task nghiệm thu riêng để chứng nhận gói này.
Archive gói đo còn chứa một file cache Python ngoài 62 file manifest; đã đối chiếu byte
với gói gốc và hash toàn archive. Lần đóng gói sau nên bỏ `__pycache__`; không sửa gói đã đo.

Hash raw, helper phân tích, kiểm provenance và số đo chi tiết:
[s5-target-results.json](s5-target-results.json). Raw giữ tại
`.artifacts/W-0338/s5-target-intake/result/`; helper `.artifacts/W-0338/analyze-s5-target.py`.
Kết luận dựa trên file owner trả về, không nhận là đã SSH hoặc hậu kiểm trực tiếp server.

## Kết quả

| Bài đo | Lượt 1 | Lượt 2 |
| --- | --- | --- |
| Tổng đơn / lỗi / PCM khớp | 65 / 0 / 65 | 63 / 0 / 63 |
| 6 đơn bắt đầu cache rỗng; đơn lâu nhất | 15,392 giây | 15,269 giây |
| 6 đơn cache ấm; đơn lâu nhất | 53 ms | 9 ms |
| Burst 2 đơn không cache; đơn cuối hoàn tất | 16,211 giây | 16,082 giây |
| Burst 4 đơn không cache; đơn cuối hoàn tất | 35,009 giây | 35,013 giây |
| Recovery sau ngắt client; toàn đơn | 24,912 giây | 24,924 giây |
| Recovery; phần đầu gồm chờ busy và tạo tiếng | 18,758 giây | 18,705 giây |
| Soak 2 đơn mỗi đợt, không cache | 46 đơn / 468,412 giây | 44 đơn / 450,636 giây |
| RSS tiến trình đỉnh | 1,5043 GiB | 1,5030 GiB |
| CPU quota throttling / OOM ghi nhận | 0 / 0 | 0 / 0 |

**128 đơn đủ 7 phần**, gồm ba giọng và hai dạng đơn nhiều món/tên dài. Probe kiểm
checksum từng phần động và toàn lời thoại ghép khớp bản đã duyệt. Archive trả về chứa
kết quả kiểm, không chứa audio để hash lại độc lập; không yêu cầu owner nghe thêm.
Client log và model log khớp 342 lần tạo tiếng thành công, 22 HTTP 503 chỉ trong hai ca
cố tình ngắt client, sau đó phục hồi. Không có lỗi ở các đơn hoặc các phần của đơn.

Hai pha soak cộng **90 đơn / 919,048 giây** (15 phút 19 giây). Thời gian toàn đơn,
bao gồm chờ: p50 **15,436 giây**, p95 **26,992 giây**, tối đa **27,405 giây**.
270 phần động trong soak: p50 1,926 giây, p95 8,857 giây, tối đa 9,073 giây.
Phân vị dùng nearest rank. Đơn burst cuối chờ tới provider tối đa **25,891 giây**,
thấp hơn budget chờ 90 giây. Số này gồm chi phí chuẩn bị nhỏ, không phải bộ đếm queue nội bộ.
Trace của provider không có lệnh chồng nhau; burst/soak hoàn tất đúng thứ tự gửi.

Các số trên là **chuẩn bị đủ audio trước dial**, không phải khoảng im giữa câu khi phát.
Không đo độ liền mạch qua SIP trên S5 trong lượt này; bằng chứng SIP/DTMF local vẫn ở
[README](README.md). RSS là high-water của tiến trình; cgroup memory là số lấy mẫu.

## So với W-0335, cùng máy và quota

| Chỉ số | W-0335 | W-0338 |
| --- | --- | --- |
| Budget từng phần | 10/15/30 giây theo pha | 30 giây tất cả pha |
| Soak: đơn / thời gian | 84 / 905,133 giây | 90 / 919,048 giây |
| Soak: p95 toàn đơn | 28,427 giây | 26,992 giây |
| Soak: toàn đơn lâu nhất | 29,569 giây | 27,405 giây |
| Soak: phần lâu nhất | 10,099 giây | 9,073 giây |
| Recovery: phần lâu nhất | 19,352 giây | 18,758 giây |

Điểm mới là **đã đo đúng một profile cuối**, thay cho việc suy từ profile hỗn hợp cũ.
Lượt mới có thời gian thấp hơn đôi chút; không kết luận sửa deadline làm model nhanh hơn,
vì đây là hai thời điểm khác nhau trên VPS dùng chung. Không có căn cứ để nâng quota.

## Kết luận và giới hạn còn lại

- Đóng phần việc **đo profile mới trên S5 trong phạm vi đã chạy**. Giữ 2 CPU/4 GiB và
  30/90/120; không cần lặp bài nghe hoặc cùng bài đo này khi candidate không đổi.
- Chưa kiểm nhiều worker dùng chung sidecar, tải làm đầy 8 chỗ chờ, hoặc cố tình chạm
  các trần 30/90/120 trên S5. Các nhánh deadline/hủy có kiểm local; lượt S5 này chứng minh
  đủ đơn, xếp hàng và recovery thành công dưới tải hai đơn mỗi đợt/burst bốn đơn.
- Probe dùng request budget cao hơn worker. Không suy 90 đơn soak thành công suất gọi
  khách hoặc chứng nhận toàn bộ cấu hình worker/scheduler trên máy đích.
- S5 vận hành toàn hệ thống chưa được nghiệm thu. Bộ đo không trả inventory sau cleanup;
  không xác nhận trực tiếp trạng thái server sau chạy. Không đổi runtime đang vận hành.
- Bước tiếp theo: hoàn tất hồ sơ S2, chốt mirror nội bộ và artifact ghim digest, quét lại
  đúng image ứng viên bằng database mới; theo dõi nghiệm thu commit sạch ở task riêng.
  Sau đó kiểm triển khai tích hợp trên S5 theo phạm vi lab được duyệt. Khách thật vẫn tắt.

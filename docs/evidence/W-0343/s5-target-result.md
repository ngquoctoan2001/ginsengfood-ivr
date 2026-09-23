# W-0343 — kết quả bàn giao và đo ứng viên VieNeu trên S5

Ngày 23/09/2026. **S5_TARGET_PASS** cho bàn giao và đo ứng viên TTS.
**Production BLOCKED; REAL_CUSTOMER_CALL_ALLOWED=NO.** File này bổ sung cho [README](README.md)
ngày 22/09; README và [verification.json](verification.json) giữ nguyên để không phá hash đã ghim.

## Cách lấy kết quả

Toàn chạy [run-s5-handoff.ps1](run-s5-handoff.ps1) lúc 01:25–01:46 UTC. Bước 1 (chép gói) và
bước 2 (cài, kiểm, đo trên vps61) xong với `S5_RELEASE_CHECK_PASS`, remote exit 0. Bước 3 (tải kết
quả) bị SSH đóng ở lần hỏi mật khẩu, nên archive được tải lại bằng một lệnh `scp` riêng, chỉ đọc.
Không chạy lại bài đo. Sau đó kiểm đúng như bước 3 của script: file checksum đúng định dạng, hash
archive khớp, `receipt.json` đạt mọi điều kiện.

## Kết quả

| Kiểm | Kết quả |
| --- | --- |
| Máy đích | `vps61`, Ubuntu 22.04.4, kernel 5.15, 10 vCPU |
| Image | `676133c79749…`, config `3bfeb46fe22a…`, đúng ứng viên cuối của W-0343 |
| Kho | `/home/ssv/ivr-artifact-mirror/releases/vieneu-w0343`, catalog khớp khóa của script |
| Giới hạn thực tế | 2 CPU, 4 GiB, không swap, network none, root chỉ đọc, `cap-drop ALL` |
| Khởi động · khởi động lại | HTTP 200 sau 8,2 s · 5,5 s |
| Hai lượt soak | 456,9 s và 457,2 s (yêu cầu 450 s), 22 batch mỗi lượt |
| Đơn · PCM | 63/63 job mỗi lượt, 0 lỗi, 63/63 PCM khớp; 168 đoạn động mỗi lượt |
| Sidecar bận | 11 phản hồi 503 mỗi lượt, đều được thử lại trong hạn 30/90/120 giây |
| OOM | không (`OOMKilled=false`, 0 lần khởi động lại) |
| Dịch vụ có sẵn | 7 dịch vụ theo dõi, trước và sau không đổi |

Container kết thúc với mã 137 sau `docker stop -t 10` của harness: tiến trình không tự dừng trong
10 giây sau SIGTERM, nên Docker gửi SIGKILL. Không phải lỗi của lượt đo. Quan sát cho production
(không phải cổng của W-0343): sidecar nên dừng gọn khi nhận SIGTERM.

## Giới hạn

Đây là triển khai và đo TTS cùng bộ chuẩn bị lời thoại của worker. Không xác nhận
scheduler/SIP/DTMF (việc của W-0344), không chứng nhận công suất production, không bật khách thật.
Chỉ Toàn chuyển W-0343 sang `ACCEPTED`. Số liệu, hash và raw nằm ở
[s5-target-result.json](s5-target-result.json) và `.artifacts/W-0343/vieneu-release-w0343-result.tar.gz`.

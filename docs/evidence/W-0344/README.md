# W-0344 — toàn luồng đơn giả trên S5

`REAL_CUSTOMER_CALL_ALLOWED=NO`

**S5: FAIL_BEFORE_CASES · lần gần nhất `cpu-r2` 23/09: Asterisk đã chạy, TTS không đọc được model
(đã sửa trong installer, chờ chạy lại).**
Lượt `093745-1c9bd160` (23/09) dừng ở readiness vì mirror model chỉ `ssv` đọc được, còn TTS chạy
bằng uid 1654 (mục 6 của [cpu-retry.md](cpu-retry.md)).
Lượt `135115-e0b16d79` dừng ở Asterisk exit 132 trước mọi ca ([receipt](s5-first-run.json)).
Lượt `133208-4b83eafa` mất SSH; console cho thấy `docker compose up` hỏng, receipt của lượt đó
chưa tải về ([lấy receipt](s5-receipt-fetch.md), chỉ để chẩn đoán). Bản sửa Asterisk, bản sửa bộ
kiểm bấm phím sớm và lệnh chạy lại nằm ở **[cpu-retry.md](cpu-retry.md)**.

Ứng dụng được ghim vào `66a6baa2019efe69a1ede3b6179fce5b49643a6d`, đã nghiệm thu
trong [W-0339](../W-0339/clean-commit-closeout.md). Bộ kiểm mới không sửa mã ứng dụng.
TTS giữ image `79e910…`, 2 CPU/4 GiB, concurrency/ORT=1, timeout từng đoạn/hàng chờ/toàn bộ
30/90/120 giây, hàng chờ 8. Không dùng ứng viên image mới của W-0342/W-0343;
không dùng lượt này để xác nhận ứng viên phát hành đó.

## Kiểm chứng gói trước bàn giao

Local rehearsal đạt **7/7 ca**, 25/25 kiểm thử launcher; hash đủ 21 file trong archive,
giữ 51 assertion của harness W-0339. Đối soát DB đạt toàn bộ số liệu bên dưới.
8 DLL + bundle migration khớp W-0339; 13 model/card trước/sau khớp; 9 container có sẵn không đổi.
Dọn 11 container riêng và các network, giữ 2 volume. Đây là kết quả **LOCAL_REHEARSAL**, chưa phải S5.
Các hash raw trong JSON dùng `hex_chunks`: nối các phần theo thứ tự để lấy SHA-256 đầy đủ.

## Phạm vi và điều kiện đạt

| Ca | Bằng chứng phải có |
| --- | --- |
| Xác nhận miền Bắc | Nhận đơn + eligibility; đủ 7 đoạn đúng thứ tự/hash; SIP/RTP; DTMF1 RFC4733; lưu IVR_CONFIRMED |
| Khách hủy miền Nam | Đủ lời thoại; SIP/RTP; DTMF0; IVR_CUSTOMER_CANCELLED; tính đúng 1 lượt |
| TTS lỗi rồi thử lại miền Trung | Lỗi xảy ra trước quay số, không tính lượt; hai attempt cùng số1; lần sau đủ lời thoại/DTMF1 |
| Hết hạn trong hàng chờ | Không attempt, không SIP/audio; kết quả cuối không tính lượt |
| Hết hạn lúc tạo tiếng | TTS_CACHE_WINDOW_EXPIRED; không SIP/audio, không tính lượt |
| Không bấm phím | Đủ 7 đoạn; không có DTMF giả; IVR_NO_ANSWER_FINAL; tính1 lượt |
| Người vận hành hủy | Terminate qua API; CALL_TERMINATED_BY_OPERATOR; không tính lượt, không tạo callback cuối |

Đối soát cuối: 7 task, 7 attempt, 4 lượt khách, 3 lỗi kỹ thuật; 9 kết quả,
6 kết quả cuối, 6 callback. Phải bằng 0: đếm nhầm lỗi kỹ thuật/kết quả không phải lượt khách,
trùng kết quả cuối, trùng callback và callback cho kết quả chưa cuối.

Đây là SIP/RTP/DTMF giữa hai tổng đài phần mềm trên S5, dùng đơn giả và alias `LAB-A`.
Chưa chứng minh thiết bị/SIM thật, Sales/M3 chung, nhiều worker hoặc production.

## Gói bàn giao

- [launcher](../../../deploy/lab/full-flow-s5/launcher.py), [7 ca](../../../deploy/lab/full-flow-s5/cases.py),
  [kiểm thử bảo vệ](../../../deploy/lab/tests/test_full_flow_s5.py).
- [Manifest 20 file](kit-manifest.json), [pin archive/installer](handoff-pins.json),
  [kết quả kiểm local](verification.json).
- Archive offline ở `.artifacts/W-0344/ivr-full-flow-w0344.tar.gz`, gồm 7 image archive.
  Tất cả hash kiểm trước load; hỗ trợ cả Docker OCI index ID và config ID trên Linux.
- Model đọc từ mirror đã kiểm `/home/ssv/ivr-artifact-mirror/releases/vieneu-w0340/models`;
  kiểm lại đủ 13 file theo khóa của đúng image trước và sau chạy. Không tải từ Internet.
- Ghim thêm 8 DLL đang chạy, bundle migration và quota/mount thực tế; từ chối drift.
- Mỗi lần tạo tên project/DB/volume/network mới; không dùng DB đang có. API chỉ mở loopback
  127.0.0.1:58443, SIP/backend không truy cập mạng ngoài. TTS không công bố port.
- Kiểm đúng Linux amd64/vps61 và daemon local, tối thiểu 8 CPU logic, RAM đang trống9GiB,
  đĩa trống4GiB. Các container có quota riêng; TTS giữ nguyên 2CPU/4GiB.
- Dọn đúng container/network có nhãn của lượt kiểm cả khi lỗi, giữ volume để điều tra.
  So sánh ID/start/restart/status của mọi container có sẵn trước/sau.
- Receipt chỉ chứa JSON/log/hash; loại compose chứa mật khẩu, cấu hình SIP và SQL riêng.

Lệnh bàn giao gốc, đã dùng ngày 22/09. **Không chạy lại**: gói gốc đã nằm trên S5, lần chạy tiếp
theo dùng retry trong [cpu-retry.md](cpu-retry.md).

```powershell
& 'C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0344\run-s5-full-flow.ps1'
```

Script kiểm checksum, tạo thư mục mới trên S5, chuyển gói, chạy 7ca bằng tiến trình tách khỏi SSH và kéo receipt về
`.artifacts/W-0344/s5-<timestamp>-<id>/`. Nhập mật khẩu SSH khi terminal hỏi, không gửi vào chat.
Có tiến độ mỗi20giây; thời gian truyền gói phụ thuộc mạng. Không chạy cùng probe W-0343 để tránh tranh tài nguyên.
Thành công của lệnh chuyển file chưa phải nghiệm thu: Codex cần đọc và xác minh receipt thực tế.

## Trạng thái và trách nhiệm tiếp theo

1. **Toàn/SSH operator:** chạy retry trong [cpu-retry.md](cpu-retry.md) **sau khi W-0343 đã trả
   receipt**; gửi vị trí receipt và các dòng cuối.
2. **Agent nhận việc:** kiểm hash/scope/hostname, 7 ca SIP/DTMF, đối soát DB, model/DLL, dòng thời
   gian phát tiếng và dọn tài nguyên; cập nhật kết quả S5 theo dữ liệu trả về. Nếu lỗi, sửa đúng
   nguyên nhân và chạy lại, giữ raw cũ.
3. **Toàn:** nghiệm thu sau khi có kết quả S5. Hiện chưa chuyển ACCEPTED.

Sau lỗi SSH 255, lớp bàn giao đã thêm [resume-s5-full-flow.ps1](resume-s5-full-flow.ps1)
và [helper độc lập phiên](recover-s5-full-flow.py). Mất kết nối không tạo lượt kiểm thứ hai;
run dở cần xem chẩn đoán, không tự xóa hoặc chạy đè. 5 test Linux (bao gồm SIGHUP cả nhóm
SSH giả lập) đạt, xem [bằng chứng khôi phục](ssh-recovery-verification.json). Gói ứng dụng,
installer, manifest và 7 ca kiểm giữ nguyên hash. Bản script cũ được giữ tại
`.artifacts/W-0344/ssh-recovery-baseline/`; kết quả rehearsal gốc không được viết lại.

Ghi chú provenance: claim W-0343 trùng phiên chuẩn bị image mới, nên full-flow chuyển sang W-0344.
Raw rehearsal đầu dùng prefix W0343 được giữ nguyên để điều tra; đó không phải kết quả S5/W-0343 phát hành.
Lượt đầu chỉ dùng mạng internal khiến API host không truy cập được; đã tách frontend cho API loopback.
SIP/backend vẫn internal; không sửa hay dùng dữ liệu của phiên image mới.

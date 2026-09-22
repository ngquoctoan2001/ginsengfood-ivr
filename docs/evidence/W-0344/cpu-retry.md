# W-0344 — sửa Asterisk không khởi động trên S5

`REAL_CUSTOMER_CALL_ALLOWED=NO`

**S5 đã chạy nhưng chưa đạt.** Lần `135115-e0b16d79` dừng ở Asterisk exit132,
trước khởi động API/worker/TTS và trước bất kỳ đơn kiểm nào. Đây là kết quả mới hơn
lỗi SSH của lần `133208-4b83eafa`; không dùng trạng thái mất kết nối cũ để mô tả lần này.
[Receipt đã kiểm](s5-first-run.json): đủ26hash,13model/card khớp;43container có sẵn không đổi.
Dọn10container và các network của lượt thất bại, giữ2volume.

## Nguyên nhân và sửa đổi

Dockerfile trước đây để Asterisk bật `BUILD_NATIVE`, biên dịch cho CPU máy build
i7-11700. S5 dùng Xeon Silver4216. Upstream xác nhận chế độ này thêm `-march=native`
và khuyến nghị tắt khi chuyển binary sang máy khác:
[hướng dẫn chính thức](https://docs.asterisk.org/Getting-Started/Installing-Asterisk/Installing-Asterisk-From-Source/Building-and-Installing-Asterisk/),
[Makefile.rules đúng22.10.1](https://github.com/asterisk/asterisk/blob/22.10.1/Makefile.rules#L132).

[Dockerfile](../../../deploy/lab/asterisk/Dockerfile) đã tắt `BUILD_NATIVE`, đặt
`-march=x86-64 -mtune=generic` cho C/C++, giữ version22.10.1 và SHA tarball cũ.
Lưu `menuselect.makeopts`, `makeopts`, `cpu-baseline.txt` trong image; build từ snapshot
có hash từng input, không mượn SHA commit cũ để mô tả Dockerfile đã sửa.

Gói retry **chỉ thay image Asterisk** cho cả switch và peer. API/worker/migration,
TTS, model, lời thoại, bảy ca, quota30/90/120 và mọi assertion của launcher giữ nguyên byte.
[Manifest mới](cpu-retry-manifest.json) và [pin bàn giao](cpu-retry-pins.json).
Installer từ chối thay image ứng dụng, guard, candidate, model hoặc bật gọi khách thật.

## Kiểm chứng trước bàn giao

- 690 lệnh compile mang baseline x86-64 generic;0 lệnh compile `-march=native`.
  Metadata image xác nhận `BUILD_NATIVE` đã tắt.
- Image cũ đã tái hiện `Illegal instruction`/132 trên QEMU Nehalem.
  Image mới tới `Asterisk Ready` trên cùng họ CPU giả lập sau giới hạn fd1024.
  QEMU có lỗi thoát/core dump trong các thử nghiệm phụ; đây chỉ là chẩn đoán ISA,
  không phải chứng nhận CPU S5 hay đo tải. Các log không đạt vẫn được giữ.
- 34/34 kiểm thử launcher/installer trên Windows;9/9 guard installer trên Linux.
- Lượt local đầu với image mới qua7/7ca, DB đúng7task/7attempt/4counted/3technical,
  9result/6final/6callback;0miscount/duplicate. Kết quả tổng giữ **FAIL** vì hai container
  GitLab Runner có sẵn tự kết thúc trong lượt kiểm. Không sửa raw hoặc bỏ guard để nhận PASS.
- Kết quả đầy đủ và ràng buộc file: [bằng chứng bản vá](cpu-retry-verification.json).

## Chạy lại trên S5

Không chạy lại script gói663MiB cũ. Bản vá mới khoảng132MiB, dùng lại gói đã chuyển lên
`/home/ssv/ivr-full-flow-w0344-20260922-135115-e0b16d79/full-flow-s5` sau khi xác minh hash.
Installer tạo bản sao mới và database/network mới; không sửa hoặc chạy đè lượt thất bại.

```powershell
& 'C:\Users\Administrator\Desktop\ivr\docs\evidence\W-0344\retry-s5-full-flow.ps1'
```

Nhập mật khẩu SSH trong terminal. Script kiểm checksum bản vá/installer, chạy đủ7ca
và kéo một archive receipt về `.artifacts/W-0344/cpu-portable-r1/s5-<id>/`, kể cả khi lỗi.
Gửi các dòng cuối cùng và vị trí receipt để Codex kiểm thực tế. Không chạy đồng thời
probe W-0343 trên S5 để tránh tranh tài nguyên.

**Còn phải làm:** Toàn chạy retry bằng tài khoản SSH; Codex xác minh receipt mới,
SIP/RTP/DTMF, đối soát DB và bảo toàn dịch vụ; Toàn nghiệm thu sau khi S5 thực sự đạt.
Chưa chuyển ACCEPTED. Không xác nhận production/M3/SIM thật hoặc nhiều worker.

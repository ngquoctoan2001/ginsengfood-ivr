# Đo VieNeu trên Ubuntu — vps61

Chỉ đo TTS bằng đơn giả. Không gọi điện, không phát/lưu audio; không có SIP, cổng công
khai hay Docker socket trong container đo. Không cần clone IVR hoặc tải model từ Internet.
Khách thật luôn `REAL_CUSTOMER_CALL_ALLOWED=NO`. Gói không đóng các mục mirror/S2 pháp lý.

## 1. Chép hai file lên thư mục home của server

- `vieneu-s5-ubuntu.tar.gz`
- `vieneu-s5-ubuntu.tar.gz.sha256`

Dùng cách chuyển file hiện có của bạn. Không cần cung cấp mật khẩu SSH trong chat.
Gói chứa đúng image TTS đã kiểm, model, ba giọng đã duyệt, đơn giả, script đo và checksum.
Máy cần Python3 và Docker mà tài khoản hiện tại được phép dùng; chưa cần cài thêm thư viện Python.

## 2. Kiểm file rồi giải nén vào thư mục mới

Chạy tại thư mục vừa chép file:

Nếu bản checksum cũ báo tên file có `$'\r'`, chuẩn hóa riêng checksum bằng
`sed -i 's/\r$//' vieneu-s5-ubuntu.tar.gz.sha256` rồi kiểm lại. Không cần chép lại
gói lớn; không bỏ qua bước kiểm hash. Bản checksum trên máy Windows đã sửa sang LF.

```bash
sha256sum -c vieneu-s5-ubuntu.tar.gz.sha256 &&
TASK_S5_DIR="$HOME/vieneu-s5-$(date +%Y%m%d-%H%M%S)" &&
mkdir "$TASK_S5_DIR" &&
tar -xzf vieneu-s5-ubuntu.tar.gz -C "$TASK_S5_DIR" &&
cd "$TASK_S5_DIR/vieneu-s5" &&
python3 --version
```

## 3. Thu thông tin máy (chỉ đọc)

```bash
python3 run-s5.py --output "inventory-$(date +%Y%m%d-%H%M%S)"
```

Gửi `inventory.json` được tạo nếu cần đối chiếu trước. File chứa host/Docker name,
CPU/RAM, context, stats và giới hạn RAM từng container; không đọc environment/secret
của các dịch vụ. Số `MEM LIMIT` trong stats có thể là cấu hình riêng của container,
không nhất thiết bằng tổng RAM máy. Chưa xác định nguyên nhân chênh lệch trong ảnh chụp số liệu đã gửi.

## 4. Chạy phép đo ban đầu

```bash
python3 run-s5.py --run --expected-host vps61 --cpus 2 --memory-gib 4 --output result
```

Runner kiểm đúng host, daemon local, RAM khớp và còn ít nhất2 GiB khả dụng ngoài quota.
Sai điều kiện sẽ dừng trước khi nạp image/chạy model; gửi lỗi cùng `inventory.json`,
không tự bỏ guard. Runner kiểm checksum toàn bộ gói rồi chỉ nạp image còn thiếu.
Container riêng giới hạn2 CPU/4 GiB, không dùng swap, inference capacity1/thread1;
các dịch vụ hiện có giữ nguyên. Đây là quota thử ban đầu, không phải cấu hình production đã chốt.
Phép đo vẫn tiêu thụ CPU/RAM/I/O trên VPS dùng chung; chọn lúc tải nghiệp vụ thấp.

Runner hiện vị trí `run.log`; có thể xem từ terminal thứ hai bằng `tail -f`.
Watchdog tối đa30 phút. Ctrl+C dừng container riêng, giữ các file đã thu để chẩn đoán.
Có `CLIENT_TIMEOUT`/HTTP503 trong kết quả là số đo, không đồng nghĩa công cụ bị lỗi.
Nếu thông báo `S5_STOP`, gửi nguyên thư mục kết quả, không xóa ca lỗi.

## 5. Gửi kết quả

```bash
tar -czf s5-result.tar.gz result
```

Gửi `s5-result.tar.gz`, gồm `inventory.json`, `host.json`, `run.log`, `load.json`
(và `image-load.log` nếu có). Nếu chưa gửi file được, gửi trước nội dung `host.json`
và dòng `S5_MEASUREMENT_COMPLETE`; vẫn cần `load.json` để phân tích timeout đầy đủ.

Phép đo gồm18 phần động của đơn nhiều món/tên dài, burst1/2/4 client với budget5s/10s,
disconnect và thời gian trả capacity. Đây là probe giới hạn trên một process mới,
không chứng minh throughput dài hạn, nhiều lần cold-start hoặc gọi điện end-to-end.
Nó chưa chạy client retry .NET W-0331. Kết quả sẽ dùng để quyết định bước đo tiếp theo.

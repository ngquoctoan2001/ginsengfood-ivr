# Đo worker VieNeu trên vps61

Giữ TTS **2 CPU / 4 GiB**, capacity 1, ONNX 1 thread. Probe .NET riêng được giới hạn
0.5 CPU / 512 MiB, chỉ dùng loopback của container TTS không có mạng ngoài.
Không gọi SIP, không phát tiếng, không sửa các container nghiệp vụ hoặc bật khách thật.

Gói mới dùng lại đúng bundle W-0333 đã có trên server. Không cần tải model lại.
Hai lượt khởi động model mới, mỗi lượt có cache lạnh/ấm, 2/4 đơn đồng thời, phục hồi
sau disconnect và tối thiểu 450 giây tải không dùng cache: tổng tải liên tục 15 phút
qua hai lượt riêng. Tổng thời gian thường khoảng 20–30 phút, tùy tải VPS.

## 1. Chép gói mới từ Windows

Chạy ở PowerShell trên Windows, nhập mật khẩu SSH tại prompt của `scp`:

```powershell
cd C:\Users\Administrator\Desktop\ivr\.artifacts\W-0335
scp vieneu-worker-s5.tar.gz vieneu-worker-s5.tar.gz.sha256 ssv@192.168.1.61:~/
```

## 2. Chạy trên Ubuntu

```bash
cd ~
sha256sum -c vieneu-worker-s5.tar.gz.sha256 &&
TASK_WORKER_S5_DIR="$HOME/vieneu-worker-s5-$(date +%Y%m%d-%H%M%S)" &&
mkdir "$TASK_WORKER_S5_DIR" &&
tar -xzf vieneu-worker-s5.tar.gz -C "$TASK_WORKER_S5_DIR" &&
cd "$TASK_WORKER_S5_DIR/vieneu-worker-s5" &&
python3 run-worker-s5.py --run --expected-host vps61 \
  --base-bundle /home/ssv/vieneu-s5-20260921-081018/vieneu-s5 \
  --output result --runs 2 --soak-seconds 450

if [ -d result ]; then
  tar -czf "$HOME/vieneu-worker-s5-result.tar.gz" result
fi
```

Giữ toàn bộ kết quả kể cả có lỗi. Script in vị trí `client.log`; có thể dùng một cửa sổ
SSH khác để `tail -f` file đó. Dòng `MEASUREMENT_COMPLETE_NOT_PRODUCTION_APPROVAL` chỉ
nói phép đo đã hoàn tất. Các lỗi từng đơn nằm trong `worker.json` và tổng hợp `host.json`.

Timeout của probe: 10 giây/phần để so với lượt trước; 15 giây/phần cho burst/soak;
30 giây/phần ở ca phục hồi cố tình ngắt client. Hàng chờ tối đa 8 đơn, chờ tối đa 90 giây
trong probe. **Đây là profile chẩn đoán, không đổi timeout production 5 giây hoặc lab 10 giây.**

## 3. Lấy kết quả về Windows

```powershell
scp ssv@192.168.1.61:~/vieneu-worker-s5-result.tar.gz C:\Users\Administrator\Desktop\ivr\.artifacts\W-0335\
```

Báo đã chép xong để đối chiếu raw. Không cần nghe lại hoặc gửi mật khẩu.
**REAL_CUSTOMER_CALL_ALLOWED=NO; S2/mirror còn mở; S5 chưa đóng.**

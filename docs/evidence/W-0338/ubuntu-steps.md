# Đo profile deadline cuối trên S5

**Chưa chạy trên vps61. REAL_CUSTOMER_CALL_ALLOWED=NO. S2/mirror OPEN.**
Gói khoảng 61 MiB, dùng lại model/base bundle W-0333. Không phát audio hoặc gọi điện.
TTS giữ 2 CPU/4 GiB; profile thống nhất 30 giây/phần, 90 giây chờ, 120 giây tổng.
Hai lượt, mỗi lượt ít nhất 450 giây tải. Đây là phép đo, không phải approval production.

PowerShell để chuyển gói khi chạy lượt S5 tiếp theo:

```powershell
cd C:\Users\Administrator\Desktop\ivr\.artifacts\W-0338
scp vieneu-deadline-s5.tar.gz vieneu-deadline-s5.tar.gz.sha256 ssv@192.168.1.61:~/
```

Trên Ubuntu:

```bash
cd ~
sha256sum -c vieneu-deadline-s5.tar.gz.sha256 &&
TASK_DEADLINE_DIR="$HOME/vieneu-deadline-s5-$(date +%Y%m%d-%H%M%S)" &&
mkdir "$TASK_DEADLINE_DIR" &&
tar -xzf vieneu-deadline-s5.tar.gz -C "$TASK_DEADLINE_DIR" &&
cd "$TASK_DEADLINE_DIR/vieneu-deadline-s5-final" &&
python3 run-worker-s5.py --run --expected-host vps61 \
  --base-bundle /home/ssv/vieneu-s5-20260921-081018/vieneu-s5 \
  --output result --runs 2 --soak-seconds 450 --final-profile

if [ -d result ]; then
  tar -czf "$HOME/vieneu-deadline-s5-result.tar.gz" result
fi
```

Chờ `MEASUREMENT_COMPLETE_NOT_PRODUCTION_APPROVAL`, hoặc khi lỗi đã dừng và đóng gói
result, mới lấy kết quả từ PowerShell. Giữ toàn bộ raw kể cả có lỗi:

```powershell
scp ssv@192.168.1.61:~/vieneu-deadline-s5-result.tar.gz C:\Users\Administrator\Desktop\ivr\.artifacts\W-0338\
```

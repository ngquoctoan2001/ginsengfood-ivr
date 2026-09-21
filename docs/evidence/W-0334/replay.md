# Chạy lại bộ thu local W-0334

REAL_CUSTOMER_CALL_ALLOWED=NO

Bộ thu này chỉ nối scenario service có sẵn với Prometheus. Không có staging, gateway/dialer,
exporter OTLP hay Alertmanager. Không dùng nó làm bằng chứng triển khai hoặc giao thông báo.
Docker Desktop, .NET 10 và các image trong ChaosEnvironment phải có sẵn/khả dụng. Hai cổng
51934 và 59034 phải rảnh; Prometheus mang tên riêng ivr-w0334-alert-prometheus.

1. Tại repo, kiểm checkout `.claude/worktrees/w0330-dsar-20260921` sạch và đúng candidate
   `aaba3d2c173c1ce3b2e6dcbf899515ea5e87c979`. Nếu thư mục chưa tồn tại, tạo bằng
   `git worktree add --detach .claude/worktrees/w0330-dsar-20260921 aaba3d2`.
   Không reset một checkout có thay đổi hoặc dùng nhầm binary của HEAD khác.
2. Build project `tests/chaos/Ivr.ChaosTests.csproj` trong checkout đó với Debug, sau restore
   locked mode. Đây là binary mà bộ thu gọi; bộ thu so SHA của các DLL copy với bản gốc.
3. Chuẩn bị helper từ các snapshot gắn với hồ sơ:

```powershell
$drillDir = '.artifacts/w0334-chaos-alert'
New-Item -ItemType Directory -Force (Join-Path $drillDir 'Harness') | Out-Null
Copy-Item docs/evidence/W-0334/harness.cs.txt (Join-Path $drillDir 'Harness/Program.cs')
Copy-Item docs/evidence/W-0334/harness.csproj.txt (Join-Path $drillDir 'Harness/Harness.csproj')
Copy-Item docs/evidence/W-0334/collector.mjs.txt (Join-Path $drillDir 'run.mjs')
dotnet build (Join-Path $drillDir 'Harness/Harness.csproj') --configuration Release
if ($LASTEXITCODE -ne 0) { throw 'Build collector failed' }
$drillRun = 'run-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
node (Join-Path $drillDir 'run.mjs') $drillRun
if ($LASTEXITCODE -ne 0) { throw 'Drill failed; inspect the new run directory' }
```

Mỗi lượt tạo thư mục mới và không ghi đè lượt cũ. Kestrel chỉ cung cấp counter tổng không có
nhãn chứa mã task/khách; nó dùng cổng host để container scrape trong thời gian thử. Helper gọi
ba SIM scenario, rồi recovery/DB/downstream. PostgreSQL và Toxiproxy do fixture tạo và dispose;
collector chỉ xoá Prometheus sau khi kiểm nhãn W-0334. Không gửi thông báo ra ngoài.

Kết quả hợp lệ phải có: baseline counter 0/alert inactive; counter thật tăng ba delta 1; 3 task,
3 attempt, 0 counted customer attempt, 3 HEALTH_FAILED; sáu lượt scenario đạt; alert firing rồi
inactive với target vẫn up và counter vẫn 3. Luật/cửa sổ 10 phút giữ nguyên. Inactive chỉ có nghĩa
đợt lỗi đã ra khỏi cửa sổ, không phải kênh tự được sửa. Cần khoảng 11 phút và giữ đủ raw artifact.

`result.json` chỉ ghi PASS_LOCAL sau bước cuối; mọi lần lỗi vẫn được giữ. Đối chiếu SHA của
candidate, DLL, rule và snapshot helper với [verification.json](verification.json) của lượt cụ thể.
Một lượt mới cần hồ sơ/hash riêng; không sửa lịch sử để trông như đã chạy lại thành công.

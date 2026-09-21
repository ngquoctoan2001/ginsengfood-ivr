# DSAR — các bước để Toàn tự chạy

W-0330 · 21/09/2026 · REAL_CUSTOMER_CALL_ALLOWED=NO

Lệnh này xử lý **một mã đơn** trong IVR. Mặc định chỉ xem trước; mỗi lượt vẫn ghi audit.
Thông tin trả về gồm số lượng và mã tham chiếu, không in dữ liệu cá nhân đang lưu.
Gói chạy đã kiểm chứng nằm ở `.artifacts/w0330-dsar-cli/`; không cần build cây làm việc đang có WIP.
Candidate `aaba3d2`; gói .NET 10, cấu hình Debug. Máy chạy cần .NET Runtime 10 và ASP.NET Core Runtime 10;
lệnh `--help` ở bước đầu cũng kiểm tra gói có khởi động được trên máy đó hay không.

## 1. Mở PowerShell và chọn gói

```powershell
Set-Location C:\Users\Administrator\Desktop\ivr
$dsarDir = (Resolve-Path .artifacts/w0330-dsar-cli).Path
$dsarDll = Join-Path $dsarDir Ivr.Dsar.dll
& dotnet $dsarDll --help
if ($LASTEXITCODE -ne 0) { throw 'Dung lai: goi DSAR chua khoi dong duoc.' }
```

## 2. Khai báo đúng tài khoản anh dùng — làm một lần trên máy chạy

```powershell
$identityResult = & dotnet $dsarDll --identity | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'Dung lai: chua doc duoc tai khoan OS.' }
$dsarIdentity = $identityResult.CurrentIdentity
$dsarIdentity
@{ Version = 1; AllowedIdentity = $dsarIdentity } |
  ConvertTo-Json |
  Set-Content (Join-Path $dsarDir dsar-operator.json) -Encoding utf8
icacls $dsarDir /inheritance:r /grant:r "${dsarIdentity}:(OI)(CI)F" "*S-1-5-18:(OI)(CI)F"
if ($LASTEXITCODE -ne 0) { throw 'Dung lai: chua thiet lap duoc quyen file.' }
```

Đọc tên tài khoản vừa in ra trước khi tiếp tục. CLI lấy danh tính từ hệ điều hành; không có cờ
để khai một người khác làm actor. File chương trình, policy và quyền lấy mật khẩu database phải
thuộc người được giao. Người có quyền quản trị máy/database vẫn có thể can thiệp vào các kiểm soát này.
Nếu chuyển sang máy khác, thiết lập lại bằng tài khoản được giao trên chính máy đó.

## 3. Chọn database và hồ sơ yêu cầu

Dùng chuỗi kết nối của **đúng môi trường anh định xử lý**, đã áp migration hiện hành.
Gói này không tự chọn database và không tự chạy migration. Trước tiên có thể dùng DB thử với dữ liệu giả.
Nhập chuỗi kết nối qua ô ẩn; không lưu mật khẩu vào repo hoặc dòng lệnh.

```powershell
$dsarConnection = Read-Host 'Chuoi ket noi PostgreSQL cua moi truong da chon' -AsSecureString
$env:IVR_DSAR_CONNECTION_STRING = [Net.NetworkCredential]::new('', $dsarConnection).Password
$dsarOrder = Read-Host 'Ma don can xu ly'
$dsarRequest = Read-Host 'Ma ho so yeu cau DSAR, khong nhap ten hay so dien thoai'
```

## 4. Xem trước

```powershell
& dotnet $dsarDll --order-code $dsarOrder --request-ref $dsarRequest
if ($LASTEXITCODE -ne 0) { throw 'Dung lai: xem truoc that bai.' }
```

Chỉ tiếp tục nếu exit code bằng 0, `Mode=PREVIEW`, `DryRun=true`, `TasksRedacted=0` và
`TasksMatched` đúng số task cần xử lý. `Found=false` nghĩa là IVR không có đơn đó.
`TasksMatched=0` cũng có thể do mọi task của đơn đã xoá trước đó; tổng Holdings vẫn đếm các dòng được giữ.

Đọc phần `Refused`: audit, khoá đơn/khách và payload callback được giữ. S3 hiện chọn giữ vĩnh viễn;
lệnh không xoá bản backup hay dữ liệu Sales. Sales phải xác minh đúng người yêu cầu và hồ sơ tương ứng.
Không chạy trong lúc còn phiên bản cũ đang rollout.

## 5. Xoá sau khi anh đã kiểm tra bản xem trước và Sales đã xác minh

Lệnh ở bước này thực sự thay đổi dữ liệu. Gõ lại mã đơn từ bản xem trước:

```powershell
$dsarConfirm = Read-Host 'Go lai chinh xac ma don da kiem tra'
& dotnet $dsarDll --order-code $dsarOrder --request-ref $dsarRequest `
  --execute --confirm-order $dsarConfirm --subject-verified
if ($LASTEXITCODE -ne 0) { throw 'Lenh bao loi; xem truoc lai va doi chieu audit truoc khi thu lai.' }
```

Kết quả thành công: `Mode=EXECUTED`, `DryRun=false`, số `TasksRedacted` và `AuditRef`.
Lưu `AuditRef` vào hồ sơ yêu cầu. Việc redact và audit commit cùng nhau; audit bị từ chối thì việc xoá
hoàn tác. Nếu mất kết nối ngay lúc commit, không suy ra kết quả chỉ từ mã lỗi: xem trước lại và đối chiếu audit.

## 6. Kiểm lại và xoá biến kết nối khỏi phiên PowerShell

```powershell
& dotnet $dsarDll --order-code $dsarOrder --request-ref $dsarRequest
$dsarCheckExit = $LASTEXITCODE
Remove-Item Env:IVR_DSAR_CONNECTION_STRING
Remove-Variable dsarConnection
if ($dsarCheckExit -ne 0) { throw 'Chua xac nhan duoc ket qua sau xu ly; doi chieu audit.' }
```

`TasksMatched=0` xác nhận không còn task chưa redact của mã đơn tại thời điểm kiểm. Nếu Sales gửi task mới
sau lần xoá, lượt xem trước sau sẽ thấy nó; lệnh chỉ chạm task chưa xoá, giữ dấu thời gian của task cũ.

Mã thoát: `0` hoàn tất lệnh; `2` sai/thiếu tham số, policy hoặc cấu hình; `3` bị từ chối quyền;
`1` lỗi vận hành. Không có thông báo lỗi nào in chuỗi kết nối hay nội dung SQL/provider.
Xem [runbook và giới hạn dữ liệu](dsar-runbook.md), [bằng chứng kiểm thử](../evidence/W-0330/README.md).

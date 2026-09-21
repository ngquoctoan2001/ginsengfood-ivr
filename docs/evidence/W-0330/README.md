# W-0330 — CLI DSAR để owner tự chạy

REAL_CUSTOMER_CALL_ALLOWED=NO

Baseline `f2d35cefdf83c6767a2fdfb34d4e17567b2c68d5`. Owner trả lời S8 trong task ngày 21/09:
“chỉ t step by step đi t chạy cho”. Phần này chuẩn bị công cụ và hướng dẫn để chính owner vận hành.
Không có lượt xoá dữ liệu khách thật; kiểm thử dùng PostgreSQL Testcontainers và dữ liệu giả.

## Hành vi

- CLI mặc định preview, một mã đơn và một mã hồ sơ; không nhận wildcard/list hoặc actor do caller khai.
- Danh tính OS phải khớp policy cạnh executable; thiếu policy, sai account hoặc sai phiên bản thì từ chối.
- Thực thi cần cờ execute, gõ lại đúng mã đơn và xác nhận Sales đã xác minh chủ thể.
- Kết quả chỉ có số lượng/tham chiếu/giới hạn; thêm TasksMatched để preview không bị nhầm với số đã redact.
- CLI không tự migrate, đổi retention, xoá backup hoặc dữ liệu Sales. S3 giữ vĩnh viễn vẫn có hiệu lực.
- COMP-DSAR-18: dùng chung mặc định kết nối với ứng dụng; giữ cấu hình GSS/TLS do operator nhập,
  từ chối keyword không được Npgsql hỗ trợ. Impact helper LOW (1 caller/1 flow).

## Phát hiện và sửa lỗi audit

Trước sửa, EraseAsync chạy UPDATE xong mới gọi audit bằng connection khác. Test cài trigger từ chối
audit trên database giả: lời gọi báo lỗi nhưng anonymized_at đã có giá trị và số đã bị xoá.

Nay DsarService dùng cùng transaction và DbContext cho redact + audit. Audit constraint/validation
thất bại thì rollback cả thao tác. Logger thường giữ API hiện hành; API ghi trong transaction đòi
caller thực sự có transaction. Logger không hỗ trợ giao dịch sẽ bị DSAR từ chối trước khi ghi.

GitNexus: EraseAsync MEDIUM (8 phụ thuộc/6 caller/0 flow); AppendAsync dùng chung CRITICAL
(79 phụ thuộc/10 caller/14 flow), đã cảnh báo trước sửa. Report và test class LOW; CLI mới chưa có index.

## Kiểm chứng ban đầu

- COMP-DSAR-13: đỏ trên source cũ đúng tại khẳng định dữ liệu phải còn nguyên; xanh sau sửa.
- COMP-DSAR-14/15/16: default preview, xác nhận đúng mã, từ chối bulk/sai cờ/actor và từ chối account trước khi đọc dữ liệu.
- COMP-DSAR-17: chạy executable thật trong thư mục riêng với DB giả; thiếu/sai policy bị từ chối;
  preview 1/0, execute 1, lặp lại 0; đơn thứ hai nguyên vẹn; ba audit đều gắn OS identity thật của tiến trình.
- 31/31 kiểm thử compliance + CLI PASS; artifact ban đầu ở .artifacts/w0330-cli-tests/w0330-cli.trx.

Sau sửa cấu hình kết nối: 37/37 kiểm thử compliance + CLI PASS, artifact
.artifacts/w0330-cli-gss/w0330-cli-gss.trx. Lượt thử đầu dùng keyword libpq gssencmode bị Npgsql từ chối;
kiểm cuối xác nhận keyword được hỗ trợ giữ nguyên Require/Prefer/Disable và keyword lạ bị từ chối.

Lượt full proof tại 8557baa đã dừng khi review phát hiện CLI ghi đè GSS được cấu hình tường minh.
Không có acceptance manifest cho lượt này; log giữ ở .artifacts/w0330-acceptance-8557baa.

## Kiểm chứng chốt và gói bàn giao

Candidate **aaba3d2c173c1ce3b2e6dcbf899515ea5e87c979**, detached checkout sạch trước/sau cả hai lượt.
Full solution chạy xong trước full sweep; không tái dùng TRX hoặc gate log cũ.

- **1171/1171 PASS**: 756 unit, 383 integration, 24 contract, 8 chaos; không skip.
- **42/42 gate PASS**, 24 entry không có invocation được manifest phân loại; consumer đối chiếu đủ tên gate.
- Test bắt đầu 14:20:01, xong 14:28:18; sweep 14:28:18–14:34:47 ngày 21/09, UTC+7.
- [Verification](verification.json) ghim SHA/tree, hash manifest, bốn TRX và sweep log. Raw bundle:
  .artifacts/w0330-acceptance-aaba3d2/acceptance-run.json.
- Gói .artifacts/w0330-dsar-cli/ được publish Debug --no-build từ candidate này: 43 file,
  cần .NET Runtime 10 + ASP.NET Core Runtime 10. Bốn DLL IVR khớp byte với binary integration test;
  bản đóng gói chạy --help và --identity thành công. [Manifest gói](package-manifest.json).
- Gói chưa có policy người vận hành hoặc credential database. Owner thiết lập theo
  [hướng dẫn 6 bước](../../compliance/dsar-cli-step-by-step.md); sáu khối PowerShell đã kiểm cú pháp.
- [Danh sách nghiệm thu](../../release/acceptance-batches.md) được sinh lại từ đúng candidate:
  **225 ứng viên, 74 XEM, 151 KHÔNG ĐẠT, 0 ĐẠT, 0 CHƯA KIỂM**. W-0330 còn IN_PROGRESS tại candidate,
  nên chưa thuộc 225 ứng viên; trạng thái hoàn thiện được ghi ở commit hồ sơ sau. Không nhận danh sách
  này là kiểm chứng cho HEAD thay đổi về sau.

## Residual đã khép và phần owner thực hiện

Khoảng trống công cụ DSAR ngoài test đã khép. PIA/checklist/retention có cập nhật kỹ thuật hiện hành;
hồ sơ W-0052 và W-0327 có addendum, giữ kết quả lịch sử. S8 ghi lời trả lời của owner, không điền hộ ô ký.

Toàn chọn database và tự chạy; Sales xác minh chủ thể. S2/PIA, xử lý backup/restore trên môi trường
thật và lượt thực thi của owner chưa có bằng chứng mới. Không tự chuyển W-0052 hay việc khác sang ACCEPTED.
Quyền OS/file/credential phải được owner bố trí trên máy vận hành; tài khoản quản trị máy/DB vẫn có thể
can thiệp. Đây là bằng chứng local với dữ liệu giả, không phải xác nhận xoá dữ liệu khách mọi nơi.

Kiểm tài liệu: compliance-pack và docs-selftest PASS; liên kết local và PII được kiểm lại ở commit hồ sơ.

## Bổ sung sau bàn giao — owner thực hành xong, 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Owner xác nhận hiện là giai đoạn code, chưa có database vận hành, và tự chạy CLI trên database
giả riêng `ivr_dsar_practice` tại loopback cổng 55443. Policy do owner thêm sau bàn giao cho tài
khoản `NQT\Administrator`; transcript đặt ACL báo 0 file lỗi. Vì vậy mô tả “gói chưa có policy”
ở trên chỉ nói về thời điểm đóng gói, không phải trạng thái sau khi owner cấu hình.

| Bước owner chạy | TasksMatched | TasksRedacted | Audit |
| --- | --- | --- | --- |
| Preview A trước khi thực thi | 1 | 0 | e43360ee-3b60-4dd6-9780-2c190659d992 |
| Execute A, gõ lại mã đơn và cờ xác nhận | 1 | 1 | bdfc1458-bf5b-45d3-8d5c-efd2c74e175f |
| Preview A sau khi thực thi | 0 | 0 | a32583a4-546c-4646-bb7f-a12de624cb93 |
| Preview B để đối chứng | 1 | 0 | 69a9a901-e6b4-4a34-95bd-1e2004d56d11 |

[Bằng chứng thực hành](owner-practice.json) đối chiếu transcript với PostgreSQL bằng transaction
READ ONLY. Bốn audit tồn tại, đúng actor/request/đơn/số lượng; chỉ một audit execute. Chín trường
redaction của A đúng giá trị quy định, anonymized_at có giá trị, các cột khác giữ nguyên. Toàn bộ
**58 cột của B khớp dữ liệu seed**. Mã đơn, customer_id và audit được giữ theo giới hạn CLI đã nêu.
Không đưa giá trị liên hệ vào hồ sơ công khai; raw dữ liệu giả ở .artifacts/w0330-practice/.

Kiểm lại **43/43 file gói** khớp manifest aaba3d2. Đây là xác minh thao tác owner trên binary đã
kiểm thử, không phải lượt chạy lại full test/sweep cho HEAD tài liệu hiện tại. Transcript kết thúc
bằng Remove-Item Env:IVR_DSAR_CONNECTION_STRING không báo lỗi; agent không đọc được môi trường
của cửa sổ PowerShell cha để xác nhận độc lập việc này. Container thực hành được giữ lại.

**Khép phần owner tự thực hành trên dữ liệu giả.** Cờ subject-verified ở đây chỉ diễn tập cổng
xác nhận, không chứng minh Sales xác minh người thật. Khi đưa vào vận hành, Toàn vẫn cần bố trí
tài khoản/quyền/credential, xử lý S2/PIA và chứng minh áp lại DSAR sau restore; Sales phụ trách
xác minh chủ thể thật. Không đổi ACCEPTED hoặc ký thay owner.

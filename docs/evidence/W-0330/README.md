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

Đang chốt candidate để chạy toàn solution và full sweep, sau đó đóng gói executable đúng source.
Kết quả ban đầu chưa dùng để nghiệm thu HEAD. [Hướng dẫn từng bước](../../compliance/dsar-cli-step-by-step.md).

Lượt full proof tại 8557baa đã dừng khi review phát hiện CLI ghi đè GSS được cấu hình tường minh.
Không có acceptance manifest cho lượt này; log giữ ở .artifacts/w0330-acceptance-8557baa.

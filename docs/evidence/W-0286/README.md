# W-0286 — Độ bền worker và kiểm chứng ứng viên local

Ngày 2026-09-14. Baseline bắt đầu `main@67ef6b1`. **IN_PROGRESS**, chỉ MOCK/local.

## Lượt trước bản vá eligibility đồng thời

[Endurance trước bản vá](endurance-before-eligibility-fix.json): **100/100 vòng, 1.130 task, 0 lỗi vòng lặp**; 2 worker tự xét eligibility, client nhận việc chỉ đọc quyết định. Các ca crash/lease, kill switch, terminate in flight, mất receiver, HTTP 503, dead-letter/replay và retention đã chạy thật trên PostgreSQL riêng `ivr-w0286-postgres:55436`. 1.106 callback ID nhận ở đầu kia, 26 được gửi lại, 0 payload không nhất quán. Đây là bằng chứng trước bản vá bên dưới, không được dùng làm kết quả của candidate sau vá.

Probe 10 lệnh eligibility đồng thời: intake 10/10 OK, eligibility 1/10 OK và 9 HTTP 500, SQLSTATE 40001. Vòng worker phục hồi được nhưng đây vẫn là lỗi API cần sửa.

## Bản vá và regression

`InternalAdminApiService.IdempotentAsync` chỉ dùng `ExecuteCoordinatedAsync` cho `record-eligibility`. Quyết định đã được `PostgresEligibilityRepository` commit bằng ReadCommitted + advisory lock theo task; lớp receipt Serializable bên ngoài tạo SSI giữa các khóa độc lập. Cơ chế phối hợp sẵn có giữ khóa theo idempotency key, replay và kiểm payload. Các lifecycle khác giữ semantics cũ; không thêm retry mù toàn HTTP handler.

Regression `EligibilityConcurrencyTests`: 10 khóa/cùng task và 10 khóa/10 task; cùng khởi động, không client retry. Trước vá **0/2**, mỗi ca 9/10 HTTP 500. Sau vá **2/2**, kiểm thêm restart→replay nguyên body, đổi task cùng khóa→409, đúng số audit/receipt, không tự tạo result/callback. Raw TRX/log nằm ở `.artifacts/w0286/tests/eligibility-concurrent-{before,after}.trx` và log tương ứng.

GitNexus trước sửa helper: **HIGH**, 5 caller/4 process, đã cảnh báo; nhánh thay đổi chỉ `record-eligibility`. Handoff cuối phải kèm full regression và endurance trên commit sau vá.

## Bộ diễn tập

- Harness dùng cổng/container PostgreSQL tùy chọn và fake Sales tên riêng theo PID. Chỉ xóa receiver do chính lượt đó tạo.
- Image selftest dùng Compose project riêng theo PID/thời điểm và chọn network bằng đúng project label; `down -v` không còn nhắm vào volume dev có sẵn.
- Manifest kê script sandbox ở nhóm cần stack thật; không chạy như gate offline.
- Đồng hồ runtime vẫn tiến thật để đo lease/retry. MOCK calling window bật 0..1440, task offset backdate; đây **không phải đóng băng đồng hồ production**. Unit scheduler có FixedTimeProvider kiểm ngoài khung giờ.

Lượt full đầu: unit 688, integration 295/296 (matrix từ chối vì source thay đổi khi thêm regression), contract 24, chaos 8. Gate sweep đầu 36/39: manifest thiếu sandbox, DR quá 180s khi chạy đồng thời và Windows thiếu `sh` trong PATH. Các lỗi này phải được xử lý/kiểm lại, không tính PASS.

Image E2E queue pause còn chờ nhãn owner. Hosted CI W-0285 chờ phiên đăng nhập; M3 thật/SIM/staging/production chưa chứng minh. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

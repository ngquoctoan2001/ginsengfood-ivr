# W-0290 — Quyết định pause và image E2E

Ngày 14/09/2026. Baseline `main@7f4a6c1`. **TESTS_PASS**, không phải ACCEPTED.

Owner chấp thuận đề xuất ở hội thoại: operator pause làm hết cửa sổ dùng `IVR_CONFIRMATION_WINDOW_EXPIRED`; `IVR_CAPACITY_EXCEPTION` dành cho bằng chứng thiếu năng lực thật. Owner đồng thời duyệt push GitLab/GitHub, CI/security scan (metadata dependency npm/NuGet), các bước tự publish image/tài liệu và deploy dev–staging. Đây không phải chữ ký M3 hay phê duyệt lab/production/real calls.

Source `IT-SCH-DEADLINE-09` đã kiểm: chưa có lượt gọi thì `WINDOW_EXPIRED/CLOSED_WINDOW_EXPIRED`, không capacity incident, callback yêu cầu `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW`. Cập nhật image E2E theo quyết định này, giữ kiểm zero attempt với positive control và xác nhận callback tại fake Sales. Identifier CAPACITY trong script giữ provenance của bài cũ; không đại diện một bài đo năng lực.

GitNexus: constant LOW/0 caller; `driveCapacityCase` LOW, 1 caller `checkEndToEnd`, 0 process. `checkScan` LOW, 1 file caller/0 process khi sửa dòng đếm image. Chỉ sửa kiểm thử, không đổi runtime.

[Scheduler regression](scheduler-regression.log): `dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~SchedulerPersistenceTests` đạt **34/34**, gồm pause expiry và capacity shortage thật.

[Image lần đầu](image-first-run.log): build, health, Compose, image scan và SBOM đạt; E2E đỏ tại assertion mới yêu cầu toàn bảng incident rỗng. Assertion task pause trước đó (zero attempt, WINDOW_EXPIRED/CLOSED_WINDOW_EXPIRED, capacity link null) đã đạt. Đọc DB trong lượt hai xác nhận bảng dùng chung chứa một sự kiện `ADMIN_QUEUE_PAUSE`, đúng audit của thao tác quản trị, không phải shortage. Sửa phép đếm về đúng task qua cả link và session scheduler; giữ sự kiện audit. [Lượt chạy lại](image-e2e-rerun.log): `node deploy/ci/scripts/image-selftest.mjs --skip-scan` đạt **8 task E2E**, build/health/Compose cùng đạt, exit 0. Scan/SBOM lấy từ lượt đầu trên cùng image; đây là kết quả tổng hợp hai lượt, chưa phải full hosted pass.

Scan/SBOM local phủ **2 image API/worker**, 31/97 thành phần. Dòng log cũ ghi cứng "three" đã được sửa thành độ dài danh sách thực tế; log trước sửa được giữ nguyên. Migration image thuộc scan của job publish, chưa có kết quả mới. `ci-config-selftest.mjs` PASS. Security dependency scan phát hiện js-yaml HIGH, sẽ xử lý bằng task tiếp theo; hosted proof chưa chạy. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

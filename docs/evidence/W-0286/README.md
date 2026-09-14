# W-0286 — Độ bền worker và kiểm chứng ứng viên local

Ngày 2026-09-14. Baseline bắt đầu `main@67ef6b1`; candidate runtime **`44830290ea97ab71b2e28002bb68c66bdbecaa53`**. **BLOCKED_EXTERNAL** cho việc đóng toàn bộ image/hosted gate; các kiểm chứng local bên dưới đã thực hiện.

## Kết quả trên candidate sau vá

| Kiểm tra | Kết quả và evidence |
| --- | --- |
| Release build | PASS, 0 warning/0 error |
| Full .NET | **1.018/1.018**: unit 688, integration 298, contract 24, chaos 8; [log](full-suite.log), [TRX index](test-results.json) |
| API matrix | **38/38 operation, 417 request**, schema/PII/replay/authorization đều kiểm trên HTTP thật; [report](api-matrix-report.json), validator selftest 18 ca |
| Endurance | **100/100 vòng, 1.130 task, 0 lỗi**; 1.109 callback ID, 26 được gửi lại, 0 payload bất nhất; [JSON](local-mock-e2e.json), [log](endurance.log). Probe không retry: intake 10/10, eligibility 10/10, **0 HTTP 500** |
| Gate sweep | **39/39 chạy đạt, 22 skip theo manifest**; [log](gate-sweep.log). Đây là số script local, không phải 39 job GitLab đã chạy |
| Kubernetes | **7/7** trên cụm Docker tạm: readiness, network policy có positive control, 90s mất DB/0 restart, token overlap 2 replica, retention; [log](k8s-selftest.log). DR trong sweep là SINGLE_HOST |
| Observability | Promtool 6 rules + 4 test files đạt; Helm render/3 negative đạt; LGTM có đủ 5 stage span, 4 metric group, log không PII, dashboard; mất collector vẫn live và replay không nhân callback; [runtime](observability-runtime.json) |
| Contract/lint | OpenAPI lint 2/2; oasdiff ghim đúng version/baseline và negative fixture đạt; NSwag sinh lại 2/2 model byte-identical; invariant globalization unit 688/688 |
| Coverage | **89,33% (18.204/20.379 dòng), ngưỡng 80% đạt**. Gộp 4 artifact từ các suite đạt, cùng candidate. Lượt unit instrument đầu có 1 regex timeout 100ms; kiểm lại cả unit suite đạt 688/688. Giữ lần lỗi trong [TRX index](test-results.json), không gọi lượt coverage đầu là PASS |
| Image | Build/non-root, live/no DB, Compose/isolation đạt. Full E2E **FAIL**: paused MOCK task trả `IVR_CONFIRMATION_WINDOW_EXPIRED`, test đòi `IVR_CAPACITY_EXCEPTION`. Job observability riêng chỉ chứng minh một task xuyên 5 stage; không thay thế ca pause |

Chi tiết lệnh, hash log và các bước chưa chạy: [verification.json](verification.json). Full suite/matrix/endurance/K8s/image ở bảng này gắn với `4483029`, không suy thành kết quả mọi HEAD sau đó.

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

## Việc còn chặn

- **Owner decision, cập nhật 14/09:** đã chốt WINDOW_EXPIRED cho operator pause ở [W-0290](../W-0290/README.md); kết quả image FAIL cũ giữ làm lịch sử; lượt mới E2E 8/8 và scheduler 34/34 PASS.
- **Hosted W-0285, cập nhật sau đăng nhập 14/09:** đã đọc runner online và pipeline cũ 36 job/7 FAIL; xem [quan sát mới](../W-0285/authenticated-observation.json). W-0287 sửa cấu hình. Chưa push candidate, chưa chạy pipeline đúng SHA; API 403 là quan sát trước đăng nhập.
- **Security scan + image scan/SBOM, cập nhật 14/09:** owner đã duyệt metadata egress npm/NuGet, scan và push/CI ở W-0290; đang thực hiện. Lần auto-review từ chối trước đó giữ làm lịch sử. Không lấy scan cũ làm kết quả hiện tại.
- **M3/SIM/staging/production:** NOT_RUN, cần đầu vào và thẩm quyền thật. `REAL_CUSTOMER_CALL_ALLOWED=NO`, nấc 0, 11 gate ngoài không đổi.

Lint phát hiện hai lỗi có sẵn: BOM trong migration W-0249 và thiếu newline cuối test speech. Follow-up chỉ chuẩn hoá encoding/newline, không đổi câu lệnh C#; GitNexus LOW, 0 caller/0 process cho mỗi class. Log và checkpoint sau sửa được lưu riêng để không trộn với candidate runtime trên.

**Checkpoint sau sửa `cc4a8c8`:** build/analyzer 0 warning/0 error, toàn bộ `dotnet format` exit 0, API matrix kiểm lại **38/38, 417 request, PASS**. [Checkpoint](post-format-checkpoint.json) chứa source fingerprint và chứng minh hai file chỉ đổi BOM/newline. Không chạy lại 100 vòng hay thay số full suite của `4483029`.

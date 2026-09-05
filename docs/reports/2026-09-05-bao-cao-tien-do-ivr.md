# BÁO CÁO TIẾN ĐỘ MODULE IVR — 05/09/2026 (cập nhật chiều)

## 1. Phạm vi và mốc kiểm tra

- **Phạm vi module là backend**: API, worker, dữ liệu, telephony adapter và bề mặt để Module 3 kết nối. **Admin UI không thuộc phạm vi** — Owner chốt lại 05/09/2026, Module 3 sẽ tự làm console trên BFF/identity của họ. Báo cáo này không tính test, image hay E2E của `admin-ui/` là tiến độ IVR.
- Baseline của lần đo: `2a6d2902a27d6d41a10d244ba28a5c78da1a86e2`, nhánh `main`. Toàn bộ kết quả dưới đây đo trên cây làm việc dựng trên baseline đó, và cây đó đã được commit trong lượt này.
- Toàn bộ mã P0.1/P0.2, script `pnpm dev:bootstrap` và `pnpm e2e:local`, evidence W-0190/W-0191 và hai báo cáo này đã được dọn và commit lên `main` trong cùng ngày. Hai file Admin UI ngoài phạm vi đã gỡ bỏ trước khi commit.
- Song song có nhánh `worktree-gd0-fixes` (`4baad09`, 2 commit, chưa merge) trong `.claude/worktrees/gd0-fixes` do một phiên Claude khác tạo; xem mục 3.7.
- Kết quả mục 2 là lần chạy lại lúc cập nhật trên đúng working tree này. Các hạng mục ghi "chưa chạy lại" giữ nguyên kết quả buổi sáng trên `2a6d290`.
- Trọng tâm tuần 30/08–05/09: callback/Retry-After, chaos deterministic, schema-spec alignment, TTS/telephony evidence, validator fail-closed (policy/dial-token/opt-out/capacity), và hôm nay là sửa blocker nội bộ P0.

## 2. Kết quả chạy thử

| Hạng mục | Kết quả hiện tại |
|---|---|
| Restore/build .NET Release | PASS, 0 warning, 0 error (chạy lại) |
| Unit test | PASS 515/515 (chạy lại) |
| Integration test với PostgreSQL thật | PASS 252/252 trong 7 phút 49 giây (chạy lại) |
| Contract test | PASS 24/24 (chạy lại) |
| Chaos test | PASS 8/8 (chạy lại) |
| Tổng .NET | PASS 799/799 (chạy lại) |
| Test traceability | 498 TestId (tăng từ 485; thêm `IT-COMPROOT`, `UT-DEV`, `IT-FLAG-UNKNOWNENV-13`, `UT-FLAG-FALLBACKLOG-11`) |
| Progressive-deployment self-test | **FAIL** (chạy lại): W0122 `DropTable` trong `Up()` |
| Runtime MOCK bootstrap | `pnpm dev:bootstrap` PASS lúc 10:06: seed 9/9, 8 dry-run job, 1 restricted, `SCN-001-confirm` = `REPLAYED/IVR_CONFIRMED/matches` (log `ci-artifacts/dev-bootstrap/`) |
| Full pipeline MOCK | `pnpm e2e:local` 5/5 PASS lúc 09:11 theo ghi nhận của phiên tạo script (log `ci-artifacts/local-e2e/`); chưa chạy lại trong lần cập nhật này |
| OpenAPI/contract | 38 inbound + 1 callback outbound; lint, fixture, drift/negative PASS (chưa chạy lại, không có thay đổi OpenAPI) |
| API route parity | PASS; 41 route = 38 operation + 3 health probe (chưa chạy lại) |
| Security | PASS; 0 NuGet/npm HIGH, Gitleaks sạch (chưa chạy lại) |
| Kubernetes core | PASS: lint/schema, MOCK gates, readiness, NetworkPolicy, DB outage 90 giây, token rotation, retention CronJob (chưa chạy lại) |
| DR local | PASS_SINGLE_HOST, RPO=0; multi-AZ và volume encryption chưa chạy |

## 3. Backend đã sẵn sàng cho Module 3 kết nối chưa?

**Chưa hoàn toàn.** Hai blocker P0 nội bộ đầu đã đóng ở local/MOCK; một blocker còn lại chưa động tới; toàn bộ gate ngoài vẫn mở.

1. **P0.1 feature flags — đạt** ([W-0190](../evidence/W-0190/README.md)): DI chọn nhầm constructor làm store rỗng đã sửa bằng factory tường minh; đủ năm environment trả 200/readable; environment lạ trả 404 thay vì 500; fallback fail-closed nay có log event 2400 + counter `ivr.feature_flags.read_fallback`; có integration test chạy đúng `Program` composition root trên PostgreSQL thật, kể cả provider outage (409, kill switch vẫn ON).
2. **P0.2 seed/scenario — đạt** ([W-0191](../evidence/W-0191/README.md)): `Ivr:DevTooling:SeedDirectory` resolve theo content root; validator báo sớm file thiếu kèm cách khắc phục; nạp seed lần hai trả job đã có thay vì 9 conflict; một lệnh `pnpm dev:bootstrap` chuẩn bị DB → API cổng trống → seed 9 → replay `SCN-001-confirm`, không bật worker.
3. **P0.3 migration expand-contract — chưa sửa**: chạy lại `progressive-selftest.mjs` vẫn fail ở [W0122](../../src/Ivr.Infrastructure/Persistence/Migrations/20260828040458_W0122DropConsoleAccounts.cs) vì drop hai bảng trong `Up()`, không an toàn cho overlap hai phiên bản hoặc rollback blue-green. Đây là blocker nội bộ duy nhất còn lại.
4. **Worker pipeline** đã có lệnh bật đầy đủ ở MOCK: `pnpm e2e:local` (`W-0192`) dựng PostgreSQL, fake Sales, API + Worker với MOCK telephony, đẩy 5 kịch bản và kiểm result taxonomy (technical exception không tính lượt khách; chỉ final result vào outbox). Ba cờ `IVR_EXECUTION_MODE / SIM_PROVIDER / REAL_CUSTOMER_CALL_ALLOWED` giữ `MOCK / MOCK / NO`. Kịch bản no-answer nhiều lượt vẫn chỉ được chứng minh bằng integration test `IT-SCHED-*`.
5. **Bề mặt cho Module 3 còn thiếu**: chưa có ma trận HTTP chạy đủ 38 operation (mới chỉ có route parity), chưa có sandbox producer/callback thật của Module 3, và contract/auth/dial-token vẫn ở trạng thái draft cho tới khi quyết định OD-V1 được hợp nhất vào `main`.
6. **Hai file Admin UI ngoài phạm vi đã được gỡ**: `ConsoleNav.tsx` trả về bản đã commit và `console-nav.test.tsx` bị xóa. Không có mã backend hay tài liệu kiểm soát nào phụ thuộc chúng; bộ test console trở lại đúng 176 bài như trước.
7. **Việc chưa hợp nhất, cần Owner quyết:** nhánh `worktree-gd0-fixes` còn commit `4baad09` ghi Owner ký 19/23 quyết định OD-V1/OD-VOICE (`od-v1-signoff-2026-09-05.md`, register, gate-status `decisions 23 → 4`). Nhánh đó gán W-0190 = "sáu defect local-run" và W-0191 = "ký 19 quyết định", trùng mã với `main` (W-0190 = P0.1, W-0191 = P0.2) nhưng khác nội dung, mà tracker thì cấm tái dùng ID. Cách đánh số đã chốt: giữ nguyên `main`, `W-0192` cấp cho lệnh chạy trọn vòng gọi, và gói ký OD-V1 sẽ nhận `W-0193` khi merge. Chừng nào chưa merge, số quyết định mở chính thức trên `main` vẫn là 23.
8. Chưa có chứng cứ pipeline GitLab hosted, staging thật, callback/auth sandbox Module 3, Legal/Privacy sign-off hay cuộc gọi SIM thật.

## 4. Ước lượng tiến độ theo hạng mục

Phần trăm dưới đây là ước lượng theo Definition of Done trong kế hoạch chi tiết, không phải trạng thái gate chính thức. Hàng Admin UI đã bị bỏ khỏi bảng và trọng số được chia lại trên 7 hạng mục backend còn lại.

| Hạng mục | Trọng số | Local/MOCK | Production | Phần còn thiếu chính |
|---|---:|---:|---:|---|
| Đặc tả và contract | 11% | 90% | 45% | M3 review/sign-off; 19 quyết định đã ký nhưng còn ở nhánh chưa merge |
| API/domain/nghiệp vụ | 22% | 97% | 65% | Ma trận HTTP đủ 38 operation, sandbox M3 |
| Dữ liệu/migration/retention | 13% | 83% | 50% | P0.3 expand-contract, retention Legal, multi-AZ DR |
| Worker/scheduler/callback | 17% | 93% | 45% | Soak 100 vòng, no-answer nhiều lượt E2E, callback sandbox M3 |
| TTS/telephony adapter | 11% | 90% | 20% | Voice acceptance, vendor/lab/real gateway |
| QA/security | 14% | 97% | 55% | Hosted CI, immutable release evidence |
| Deploy/observability/DR | 12% | 84% | 37% | Progressive deploy, staging, multi-AZ |
| **Tổng có trọng số** | **100%** | **91%** | **48%** | 1 blocker P0 nội bộ + 11 gate ngoài |

Hai con số tổng là trung bình có trọng số đúng của các hàng trên. Bản buổi sáng ghi 92% cho Local/MOCK là do cộng dồn không khớp trọng số, không phải do tiến độ giảm; tính lại đúng thì bản sáng là 91% và hiện tại cũng 91%.

- [Trạng thái kiểm soát chính thức](../release/gate-status.yaml) trên `main` vẫn là **RUNG 0 / NO-GO**: 8/190 work item `ACCEPTED`, 11 gate và 23 quyết định còn mở (4 nếu merge nhánh sign-off); W-0190, W-0191 và W-0192 đều ở `TESTS_PASS`, chưa `ACCEPTED`.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`; không có kiểm thử nào sử dụng số khách hoặc trunk/SIM thật.

## 5. Kế hoạch triển khai tiếp theo

1. **P0.0 — hợp nhất:** đã bỏ hai file UI, chốt đánh số và commit cây làm việc. Còn lại: merge phần ký OD-V1 từ `worktree-gd0-fixes` dưới mã `W-0193`, rồi đẩy `main` lên cả hai remote.
2. **P0.3 — migration expand-contract:** thay drop trực tiếp bằng chuỗi ngừng ghi/đọc → deploy bản tương thích → xác nhận hết consumer → cleanup ở release sau; chứng minh N-1→N, overlap N/N+1 và rollback; thêm CI guard cấm destructive DDL. Xong thì `progressive-selftest.mjs` phải xanh.
3. **P1 — đóng API/MOCK:** HTTP matrix đủ 38 operation, auth/idempotency/error matrix, mở rộng `pnpm e2e:local` thành soak 100 vòng có no-answer nhiều lượt.
4. **P2 — tích hợp Module 3 không telephony thật:** contract/auth/dial-token/attempt policy theo quyết định vừa ký, producer hai chương trình, callback ACK và sandbox E2E hai chiều. Bốn quyết định còn mở (`OD-V1-09` nửa sau, `OD-V1-10`, `OD-V1-11`, `OD-V1-21`) cần số đo thật hoặc người thứ hai, không tự ký được.
5. **P3 — CI/staging:** pipeline bắt buộc, immutable images/SBOM cho API/Worker/Migrate, deploy staging, observability/alert, performance/chaos/DR/rollback và soak 24–72 giờ.
6. **P4 — governance:** chốt script/voice/privacy/retention/opt-out, runbook, go/no-go và evidence chưa phụ thuộc telephony thật.
7. **P5 — mua sau cùng:** chỉ khi P0–P4 xanh mới mua gói lab nhỏ nhất/1 SIM; lab allowlist + kill switch trước, đo capacity rồi mới quyết định gói production (không mặc định mua 32 SIM).
8. **P6 — pilot/go-live:** UAT số nội bộ, pilot giới hạn, rollback rehearsal, sau cùng mới cân nhắc bật cuộc gọi thật.

Kế hoạch thi công, tiêu chí thoát từng pha và phân công hai người nằm tại [2026-09-05-ke-hoach-hoan-thien-ivr.md](./2026-09-05-ke-hoach-hoan-thien-ivr.md).

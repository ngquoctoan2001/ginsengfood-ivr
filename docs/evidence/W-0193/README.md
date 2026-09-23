# W-0193 — Local-run defect remediation và một lệnh chạy hết vòng đời

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0193 (GĐ 0 của lộ trình hoàn thiện) ráp và chạy thật toàn hệ thống ở MOCK rồi sửa sáu lỗi mà bộ test cũ không bắt: store feature-flag rỗng do DI chọn constructor (F-01), tên môi trường lạ trả 500 thay vì 404 (F-02), fallback fail-closed không để lại log (F-03), SeedDirectory resolve theo working directory thay vì content root (F-04), sidebar console ẩn màn (F-05) và nạp seed lần hai báo 9 conflict (F-06), kèm lệnh `pnpm e2e:local`. Việc làm trên nhánh `worktree-gd0-fixes` (commit 3887a47 còn ghi `w-0190`, đổi số ở 7dc9a8c) và vào main qua merge ba43605, nơi source và test lấy bản của nhánh; F-05 và test UI của nó không vào main vì Admin UI đã bị loại khỏi phạm vi. Kiểm bằng test .NET IT-COMPROOT-FLAGS-01/02, IT-COMPROOT-SEEDPATH-03/04/05, IT-FLAG-UNKNOWNENV-13, UT-FLAG-FALLBACKLOG-11 và IT-DEV-SEED-04 (viết lại), cùng một lần chạy tay `pnpm e2e:local` 5/5.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 3887a47
- 7dc9a8c
- ba43605
- cc12e53
- docs/traceability-tests.md
- tests/Ivr.IntegrationTests/CompositionRootTests.cs
- tests/Ivr.IntegrationTests/FeatureFlagApiTests.cs
- tests/Ivr.IntegrationTests/DevToolingApiTests.cs
- tests/Ivr.UnitTests/FeatureFlagPlatformTests.cs
- src/Ivr.Infrastructure/FeatureFlags/FeatureFlagServiceCollectionExtensions.cs
- src/Ivr.Infrastructure/FeatureFlags/FeatureFlagPlatform.cs
- src/Ivr.Api/Admin/FeatureFlagEndpoint.cs
- src/Ivr.Api/Application/InternalAdminApiServiceCollectionExtensions.cs
- src/Ivr.Api/Application/DevToolingApiService.cs
- tools/dev/Invoke-LocalE2E.ps1
- docs/lab/one-sim-lab-plan.md

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `IT-COMPROOT-FLAGS-01`, `IT-COMPROOT-FLAGS-02`, `IT-COMPROOT-SEEDPATH-03`, `IT-COMPROOT-SEEDPATH-04`, `IT-COMPROOT-SEEDPATH-05`, `IT-FLAG-UNKNOWNENV-13`, `UT-FLAG-FALLBACKLOG-11`, `IT-DEV-SEED-04`.

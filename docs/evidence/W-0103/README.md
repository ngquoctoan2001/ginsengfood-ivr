# W-0103 — Current-system audit closure

Ngày: `2026-08-20`

Baseline: `main@66495369f62df2added1dd47a43de84aa147ff90`
Trạng thái: `TESTS_PASS` (local/current system), không phải Sales/LAB/PROD acceptance.

## 1. Phạm vi đã đóng

W-0103 nhận toàn bộ WIP hợp lệ còn lại của lượt audit hiện tại thay vì bỏ chúng thành các thay đổi
không chủ sở hữu:

- `W-0043`: image E2E từ 3 lên 8 case, phủ mọi result mà IVR runtime thực sự sinh; non-final và
  technical result bị chứng minh không tới Sales; capacity đi qua admin pause/resume.
- `W-0042`: chaos partial DB partition chứng minh duplicate callback là nhận diện được, stale lease
  không thắng và worker mới vẫn hoàn tất được.
- `W-0041`/`W-0035`: loại flaky MeterListener do cross-test process-wide telemetry và làm lỗi đọc
  traceability báo đúng file/inner exception.
- tracker/map/evidence/gameday drift được đồng bộ với code và test hiện tại.
- format gate được làm tái lập trên Windows và Linux bằng `*.cs text eol=lf`; formatter sửa các
  whitespace/charset diagnostic còn lại.

## 2. Semantics blocked đã khóa

Quyết định chuẩn nằm ở [`DT-06`](../../../specs/decisions/DT-06-blocked-result-semantics.md):

- `IVR_OPERATIONAL_BLOCKED` / `IVR_POLICY_BLOCKED` là pre-call decision, IVR không gửi như result.
- outbound Target mapper fail-closed nếu code cố gửi hai mã này.
- Sale Lock/Recall phát hiện lúc Sales revalidate trả `BLOCKED_BY_CORE`; call result khách đã tạo
  không bị viết lại (`IT-ELIG-RACE-12`).
- analytics trả `null`, UI hiện `—`, không còn số `0` giả. Chỉ có số khi một intake-block fact
  source riêng được thiết kế và kiểm thử.

Hai enum vẫn ở contract/domain taxonomy để tương thích Target V1 draft và dữ liệu lịch sử. Đây
không phải cam kết producer sẽ phát chúng.

## 3. Contract và format

- OpenAPI internal/admin: `1.0.0-draft.8` → `1.0.0-draft.9`, 0 operation mới.
- Target callback draft: chỉ bổ sung mô tả producer semantics, không đổi enum/path/body.
- NSwag `14.7.1` đã regenerate cả hai output; manifest hash, human diff, portal 12 file và changelog
  được regenerate.
- `oasdiff breaking --fail-on WARN`: không có breaking change cho cả hai contract.
- `dotnet format Ivr.sln --verify-no-changes --no-restore`: exit `0` trên Windows checkout hiện tại.

## 4. Kết quả test/gate

| Gate | Kết quả |
| --- | --- |
| Release build (`--warnaserror`) | PASS — 0 warning, 0 error |
| .NET full solution | PASS — contract 22 + unit 257 + integration 177 + chaos 6 = **462/462** |
| Admin UI | PASS — 18 file / **179/179**; lint, typecheck, production build |
| OpenAPI | PASS — lint 0 warning; parse/schema/negative; pinned drift; NSwag regenerate |
| Docs/portal/traceability | PASS — portal 12; local links/topology; traceability **331** |
| Markdown map | refreshed — 522 Markdown file / 478 Markdown links; 39 reported unresolved are existing non-Markdown targets (`.cs`, `.yaml`, `.json`, `.txt`, `.html`, `.sh`), not missing paths |
| CI/Compose | PASS — CI config self-test; merged E2E Compose config |
| Dependencies | PASS — 10 NuGet project không có vulnerable package; hai npm audit 0 |
| Privacy | PASS — `PII_SCAN_PASS files=286 skipped_binary=2 locale=C.UTF-8` |
| Image self-test | PASS — 6/6, xem §5 |
| Gitleaks staged/pre-commit | PASS — 1.37 MB staged diff, `no leaks found` |
| GitNexus `detect-changes --scope staged` | PASS/degraded graph query — exit 0; 51 file, 147 symbol, 29 affected process; risk `critical` do callback mapper + analytics contract/UI + E2E/chaos xuyên luồng |

GitNexus báo hai timeout nội bộ ở truy vấn `file-symbols` rồi vẫn hoàn tất và trả result/exit `0`;
không được diễn giải thành một graph query hoàn toàn khoẻ. Mức `critical` đã được cảnh báo trước khi
sửa `ToSalesWire`; full regression và image E2E ở bảng trên là regression proof cho blast radius
này. Gitleaks được chạy ở chế độ staged/pre-commit; ignored local `.env.local` và false positive
lịch sử ngoài staged diff không được dùng làm kết luận về commit.

## 5. Image E2E hiện tại

`node deploy/ci/scripts/image-selftest.mjs` chạy đầy đủ, không dùng cờ skip:

- build/non-root: API `1654`, worker `1654`, UI `1000`;
- health + Compose stack trắng + fake-Sales no-egress;
- Trivy: cả ba image `0 HIGH / 0 CRITICAL`, đối chứng xấu vẫn bị từ chối;
- CycloneDX SBOM: API 24, worker 96, UI 43 component; image và SBOM cùng verdict, đối chứng xấu
  vẫn đỏ sau round-trip;
- `IT-IMG-E2E-05`: **8 task / 1 SIM channel**. `IVR_CONFIRMED`,
  `IVR_CUSTOMER_CANCELLED`, `IVR_NO_ANSWER_FINAL`, `IVR_INVALID_PHONE_FINAL`,
  `IVR_CAPACITY_EXCEPTION` tới fake Sales đúng một lần; `IVR_NO_ANSWER_ATTEMPT`,
  `IVR_WRONG_INPUT`, `IVR_TECHNICAL_EXCEPTION` không tới Sales;
- final marker: `IMAGE_SELFTEST_PASS`, exit `0`.

SBOM nằm ở artifact local/CI và không commit vì nó mô tả image của đúng lần build.

## 6. Những gì chưa được chứng minh

- Không gọi Sales thật; endpoint, auth/audience, final payload CDC vẫn `NOT_RUN/BLOCKED_EXTERNAL`.
- Không gọi SIM thật; modem/gateway/carrier/caller ID/allowlisted destination và phát âm thực tế
  vẫn `NOT_RUN` dưới one-SIM lab.
- Chưa provision 32 eSIM và chưa có capacity/failover/cost evidence thật.
- Target V1 vẫn `DRAFT`; không có owner/reviewer external acceptance hay production approval.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`; mọi callback trong image E2E đi tới fake Sales.

## 7. Handoff

Sau commit W-0103, working tree phải sạch. Bước tiếp theo chỉ được chuyển sang Sales/one-SIM lab khi
đã có API/auth/payload thật hoặc adapter + destination allowlist; thiếu dữ liệu nào phải giữ
`OWNER_DATA_REQUIRED`/`BLOCKED_EXTERNAL`, không thay bằng mock rồi tuyên bố tích hợp xong.

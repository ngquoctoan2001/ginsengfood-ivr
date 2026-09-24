# W-0078 — NuGet security gate remediation

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0078 ghim trực tiếp gói SSH.NET 2026.0.0 vào tests/Ivr.IntegrationTests, kèm packages.lock.json. Lý do: Testcontainers.PostgreSql 4.13.0 kéo gián tiếp SSH.NET 2025.1.0, bản dính advisory High GHSA-q939-rpr3-3284. Việc này không hạ cổng và không tắt cảnh báo. Ngày 13/08 (commit 3c0aa13, chung với W-0016) bản ghim được kiểm bằng: locked restore; NuGet vulnerability audit không còn lỗ hổng; build 0 lỗi, 0 cảnh báo; 20/20 test integration và 93/93 toàn bộ (A-0071). Hiện không có TestId hay gate sweep nào kiểm riêng bản ghim này.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 3c0aa13
- docs/evidence/W-0016/README.md
- tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj
- tests/Ivr.IntegrationTests/packages.lock.json

## Phép kiểm C2

Trạng thái khai báo:

- Chưa khai phép kiểm. Bản ghim gói SSH.NET chỉ được kiểm gián tiếp: build coi cảnh báo là lỗi, và job security_scan chạy trên CI. Không test hay gate sweep nào kiểm riêng bản ghim. Toàn quyết nhận theo sổ và commit, hay thêm một phép kiểm trong sweep trước.

## Khai báo phép kiểm C2 — W-0353, 24/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Phép kiểm của việc này là bước `dotnet list package --vulnerable` trong `deploy/ci/scripts/security-scan.sh`: nếu
bản SSH.NET dính advisory quay lại qua Testcontainers, gate đỏ ở mức HIGH. Từ W-0352, gate này chạy trong lượt mở
rộng của collector, và từ W-0353 khai báo `gates` được xét qua lượt đó. [Khai báo](acceptance-tests.json) nay ghi
`security-scan.sh`. Kết quả và phạm vi lịch sử ở trên giữ nguyên.

# W-0080 — P0-2 PII artifact coverage remediation

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0080 bỏ danh sách phần mở rộng được phép trong deploy/ci/scripts/scan-pii.sh, nên scanner quét mọi file text, kể cả .sql và file không phần mở rộng, và đếm số file binary bị bỏ qua. Target không tồn tại, hoặc không có file text nào, giờ fail closed với exit 2 thay vì báo PASS files=0. Việc này được kiểm bằng CT-CI-06h trong selftest-pii.sh, bằng assertion trong ci-config-selftest.mjs, và bằng lần quét thật cây bằng chứng (lúc đó 95 file text, 1 file binary). Topology artifact trên GitLab hosted vẫn NOT_RUN dưới W-0061.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 47e605c
- deploy/ci/scripts/scan-pii.sh
- deploy/ci/scripts/selftest-pii.sh
- deploy/ci/scripts/ci-config-selftest.mjs
- prompt/phase-0-foundation/P0-2-ci-baseline-quality-gates.md

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `selftest-pii.sh`, `scan-pii.sh`, `ci-config-selftest.mjs`.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0080 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.

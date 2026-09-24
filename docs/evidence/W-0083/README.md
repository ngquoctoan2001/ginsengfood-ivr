# W-0083 — Source project dependency guard remediation

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0083 thay phép kiểm UT-BOOT-03, trước đây chỉ nhìn tham chiếu assembly, bằng ma trận ProjectReference chính xác cho mọi src/*.csproj. Domain và Contracts không tham chiếu project nào. Infrastructure chỉ tham chiếu Contracts và Domain. Api và Worker chỉ tham chiếu Contracts và Infrastructure. Việc này được kiểm bằng chính UT-BOOT-03; thêm project hay tham chiếu mới thì phải sửa ma trận đã review.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 47e605c
- tests/Ivr.UnitTests/ArchitectureDependencyTests.cs
- 799501c

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `UT-BOOT-03`.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0083 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.

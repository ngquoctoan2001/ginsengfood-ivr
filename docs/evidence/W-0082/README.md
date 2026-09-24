# W-0082 — Error envelope boundary remediation

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0082 đưa ErrorEnvelopeMiddleware lên trước authentication, authorization và allowlist, để lỗi ở giai đoạn xác thực vẫn trả envelope đã che dữ liệu. W-0082 cũng cho IvrErrorResponseWriter ghi log rồi abort khi response đã bắt đầu, thay vì ghi đè. Việc này được kiểm bằng hai test. IT-FND-ERR-12: lỗi ném từ authentication handler ra envelope 500, không lộ số điện thoại hay tên exception. IT-FND-ERR-13: response đã bắt đầu thì abort, không ghi thêm byte nào.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 47e605c
- src/Ivr.Api/Middleware/IvrApiApplicationBuilderExtensions.cs
- src/Ivr.Api/Middleware/IvrErrorResponseWriter.cs
- tests/Ivr.IntegrationTests/CrossCuttingFoundationTests.cs
- tests/Ivr.IntegrationTests/FoundationApiTestApplication.cs

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `IT-FND-ERR-12`, `IT-FND-ERR-13`.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0082 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.

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

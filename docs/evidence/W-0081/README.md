# W-0081 — Stable error catalog parity remediation

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0081 đưa IVR_PII_POLICY_VIOLATION (HTTP 422) vào danh mục mã lỗi ổn định ở API-06, IvrErrorCodes và factory IvrErrors. Sau thay đổi, OpenAPI, API-06 và source cùng có đúng 16 mã. Việc này được kiểm bằng CT-CI-10 trong ci-config-selftest.mjs, so khớp nguyên tập mã của ba nguồn, và bằng UT-FND-ERR-05, kiểm lỗi PII trả envelope 422 với đúng mã. Thay đổi nằm trong commit gộp 47e605c.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 47e605c
- specs/api/06-error-codes.md
- src/Ivr.Domain/Errors/IvrErrorCodes.cs
- src/Ivr.Domain/Errors/IvrErrors.cs
- src/Ivr.Api/Middleware/IvrErrorResponseWriter.cs
- deploy/ci/scripts/ci-config-selftest.mjs
- tests/Ivr.IntegrationTests/CrossCuttingFoundationTests.cs
- tests/Ivr.IntegrationTests/FoundationApiTestApplication.cs
- prompt/phase-0-foundation/P0-2-ci-baseline-quality-gates.md
- prompt/phase-0-foundation/P0-3-crosscutting-foundation.md

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `UT-FND-ERR-05`.
- Gate trong full sweep có self-test kiểm thay đổi của việc này: `ci-config-selftest.mjs`.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0081 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.

# W-0084 — PostgreSQL audit append-only proof

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0084 thêm một test Testcontainers PostgreSQL gửi thẳng UPDATE và DELETE vào ivr_audit_log. Test chứng minh trigger append-only chặn cả hai lệnh (SQLSTATE P0001) và bản ghi giữ nguyên. Việc này không đổi code sản phẩm; nó được kiểm bằng IT-DB-AUDIT-07.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 47e605c
- tests/Ivr.IntegrationTests/PostgresPersistenceTests.cs

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `IT-DB-AUDIT-07`.

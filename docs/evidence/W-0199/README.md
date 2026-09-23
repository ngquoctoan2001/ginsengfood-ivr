# W-0199 — GĐ 2 đợt 3: ngữ nghĩa dial token dùng lại

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Năm tài liệu ghi dial token là 'one-use per attempt', một luật không thể thực thi vì chính sách cần ít nhất hai cuộc gọi khách; code khi đó còn yếu hơn cả hai: một token resolve được không giới hạn miễn mỗi lần mang `attempt_id` mới. W-0199 thay bằng `DialTokenResolveLedger` dùng chung cho vault MOCK, LAB và fake. Token gắn vào task đầu tiên dùng nó, trần số lần resolve bằng `max_attempts` của task cộng `TechnicalRetryLimit`. Hết hạn, sai task, replay hay quá trần đều bị từ chối kèm mã lý do riêng, và mọi lần resolve đều ghi audit kèm `attempt_id`. Kiểm bằng `UT-TOKEN-REUSE-01..08` cùng `UT-TEL-TOKEN-03` và `UT-AST-VAULT-04` viết lại theo trần (commit gốc `5cffea3`, message ghi id cũ `W-0197`).

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0199; §9 completion record 'Work ID: W-0199'; activity A-0567, A-0568
- 5cffea3 (commit gốc, chung với W-0198; message ghi id cũ W-0197)
- cc12e53 và 4fde987 (đánh số lại và merge vào main)
- src/Ivr.Infrastructure/Telephony/DialTokenResolveLedger.cs; MockDialTokenVault.cs; LabDialTokenVault.cs; src/Ivr.Infrastructure/Providers/Fakes/DeterministicProviderFakes.cs
- tests/Ivr.UnitTests/Telephony/DialTokenResolveLedgerTests.cs; MockTelephonyTests.cs; AsteriskLabTelephonyTests.cs
- specs/api/04-sim-adapter-contract.md; specs/data/05-pii-policy.md; specs/testing/07-security-privacy-test-plan.md
- việc sau có liên quan: eb7e781 (SIP-05, sổ trần resolve bền qua restart)

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `UT-TOKEN-REUSE-01`, `UT-TOKEN-REUSE-02`, `UT-TOKEN-REUSE-03`, `UT-TOKEN-REUSE-04`, `UT-TOKEN-REUSE-05`, `UT-TOKEN-REUSE-06`, `UT-TOKEN-REUSE-07`, `UT-TOKEN-REUSE-08`, `UT-TEL-TOKEN-03`, `UT-AST-VAULT-04`.

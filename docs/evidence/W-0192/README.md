# W-0192 — One-command full-lifecycle local run và đối soát lab plan theo code

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0192 là bản ghi phía main cho lệnh `pnpm e2e:local` (tools/dev/Invoke-LocalE2E.ps1: bật scheduler, normalization, callback và MOCK telephony, dựng fake Sales, admit 5 kịch bản và tự kiểm taxonomy kết quả) và mục §0.2 đối soát bốn khoảng trống của one-sim-lab-plan.md, cùng việc gỡ thay đổi Admin UI ngoài phạm vi, ignore thư mục worktree và hai báo cáo ngày 05/09, tất cả nằm trong commit gộp 603a6bb. Việc kiểm chỉ là một lần chạy tay `pnpm e2e:local` 5/5 exit 0 (log ở ci-artifacts/local-e2e/, không commit) cùng hồi quy .NET 799/799 và self-test docs/ci-config/gate-status; không có test hay gate nào chạy lại script này. Script và §0.2 trùng từng byte với phần W-0193 viết trên nhánh worktree-gd0-fixes (3887a47), nên hai việc cùng nhận một sản phẩm.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 603a6bb
- 3887a47
- 7dc9a8c
- ba43605
- 9603ca5
- tools/dev/Invoke-LocalE2E.ps1
- package.json
- README.md
- docs/lab/one-sim-lab-plan.md
- docs/reports/2026-09-05-ke-hoach-hoan-thien-ivr.md

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: W-0192 không sửa src/, tests/ hay script gate nào: phần của nó là script dev chạy tay `pnpm e2e:local` (kết quả 5/5 chỉ là một lần chạy tay, không runner nào trong sweep chạy lại), mục §0.2 đối soát lab plan và báo cáo ngày 05/09, nên không có claim phần mềm nào để test tự động xác nhận. Danh sách 5 tài liệu nằm trong khai báo.

Việc này là bản ghi phía main của phần W-0193 làm trên nhánh; script và mục đối soát trùng từng byte. Toàn có thể gộp hai dòng.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0192 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Migration
expand-contract đã làm ở W-0196; nhánh đã merge ở ba43605. Claude chọn việc theo tiêu chí phiếu
W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.

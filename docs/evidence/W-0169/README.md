# W-0169 — Documentation estate cleanup — gỡ tài liệu SUPERSEDED/HISTORICAL

Ngày lập hồ sơ: 24/09/2026 · Claude (W-0351) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Theo chỉ đạo owner ngày 04/09 ("xóa hẳn khỏi repo"), W-0169 xoá 63 tệp tự khai `SUPERSEDED` hoặc
`HISTORICAL`, sửa mọi link trỏ vào chúng và thêm `.artifacts/` vào `.gitignore`. Việc chỉ đụng tài liệu.

## Phép kiểm

- Gate `docs-selftest.mjs` kiểm mọi link cục bộ của portal còn trỏ tới tệp có thật; nó chạy trong full sweep.
- Lúc làm: link check 0/0 hỏng (trước đó 13), cùng các self-test docs, ci-config, review-gate, compliance-pack
  và gate-status đều đạt.

Trạng thái chuyển `CODE_DONE` → `EVIDENCE_SUBMITTED` ở W-0351, khi hồ sơ này được lập.

## Ba cảnh báo tồn đọng đã có câu trả lời — W-0353, 24/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

- **(a) W-0105 và W-0118 không còn evidence.** Cả hai nay là `N/A`: W-0105 là mục lịch sử đã được W-0128 thay, còn
  W-0118 được W-0316 chuyển `N/A` ngày 18/09 (activity `A-0646`). Không dòng nào còn mang trạng thái đòi evidence.
- **(b) Lệch khỏi OD-20 đã được ghi.** Ô trạng thái của dòng OD-20 trong `plan/ivr-orther/decisions-log.md` nay ghi rằng
  tệp `.docx` đã bị xoá theo chỉ đạo owner ở commit `c213bf7`, và bytes gốc vẫn lấy lại được bằng
  `git show c374936:docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN_SUPERSEDED.docx`: 45.101 byte, SHA-256
  `b2b95c9cb62e14b8138538b8447117040207641e5c565e4e1881f3a55af0935c` (đo lại ngày 24/09, khớp giá trị W-0141 ghi).
- **Bản đồ tài liệu sinh lại bằng mapper chính thức `markdown-doc-reader`** (W-0075 chỉ cho phép bản do mapper này
  sinh). Chạy trên một clone sạch của commit `e95ba64`, tức chỉ các tệp được track: 834 tệp Markdown, 1564 link giải
  được, 493 link không giải được. Cả 493 đều trỏ tới tệp hoặc thư mục có thật mà mapper không lập chỉ mục (`.cs`,
  `.json`, `.yaml`, thư mục); không link nào trỏ tới đích không tồn tại. Kiểm bằng cách giải từng `rawTarget` so với
  cây của clone. Dòng `Root` của bản đồ ghi path của clone đó, vì bản checkout chính có các worktree chưa track dưới
  `.claude/worktrees/` mà mapper sẽ quét cả vào. Trước khi sinh lại, 18 link hỏng ở 9 tệp đã được sửa: link tới
  `admin-ui/` đã xoá ở W-0253, link tới thư mục `.artifacts/` chỉ có trên máy, và link tương đối cũ của
  `docs/review/2026-09-07-m8-worklist-claude-annotated.md`.

Bản đồ lồng dưới `docs/documents/4. phase/phase-4/.codex-doc-memory/` nằm trong `docs/documents/**`, vùng không được sửa,
nên giữ nguyên; mapper cũng bỏ qua nó theo thiết kế.

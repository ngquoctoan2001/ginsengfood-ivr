# W-0169 — Documentation estate cleanup — gỡ tài liệu SUPERSEDED/HISTORICAL

Ngày lập hồ sơ: 24/09/2026 · Claude (W-0351) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Theo chỉ đạo owner ngày 04/09 ("xóa hẳn khỏi repo"), W-0169 xoá 63 tệp tự khai `SUPERSEDED` hoặc
`HISTORICAL`, sửa mọi link trỏ vào chúng và thêm `.artifacts/` vào `.gitignore`. Việc chỉ đụng tài liệu.

## Phép kiểm

- Gate `docs-selftest.mjs` kiểm mọi link cục bộ của portal còn trỏ tới tệp có thật; nó chạy trong full sweep.
- Lúc làm: link check 0/0 hỏng (trước đó 13), cùng các self-test docs, ci-config, review-gate, compliance-pack
  và gate-status đều đạt.

Trạng thái chuyển `CODE_DONE` → `EVIDENCE_SUBMITTED` ở W-0351, khi hồ sơ này được lập.

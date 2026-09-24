# W-0171 — Doc↔code conflict audit và reconciliation

Ngày lập hồ sơ: 24/09/2026 · Claude (W-0351) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Theo yêu cầu owner ngày 04/09 ("sửa docs cho khớp code"), W-0171 viết lại spec cho khớp code: enum và
trạng thái theo CHECK constraint của DB, 9 bảng chỉ có trong code, trạng thái thực thi của taxonomy
technical-exception, quyền dev-tooling, tên bảng số ít sai. Không đổi runtime, OpenAPI hay migration; ngoài
tài liệu chỉ sửa một comment trong `DevToolingEndpoints.cs`.

## Phép kiểm

- Lúc làm: build 0 lỗi, docs-selftest và các self-test ci-config, review-gate, compliance-pack, gate-status đạt;
  đối chiếu enum tài liệu với constraint DB không thiếu giá trị nào.
- Các mục còn treo trong Residual đã được W-0312 và W-0313 xử lý.

Trạng thái chuyển `CODE_DONE` → `EVIDENCE_SUBMITTED` ở W-0351, khi hồ sơ này được lập.

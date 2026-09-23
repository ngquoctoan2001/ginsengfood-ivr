# W-0202 — Xoay baseline OpenAPI `draft.22` → `draft.23`

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Land W-0197 làm job `api_contract_diff` đỏ: `draft.23` thắt chặt `x-correlation-id` (minLength 0→1, thêm pattern, maxLength 128) và khai `OptionalIdempotencyKey`, nên oasdiff ra 11 cảnh báo trong khi `--fail-on WARN` không được phép fail. Owner chọn phương án (a): baseline so sánh xoay sang `draft.23`, toàn bộ 11 cảnh báo được đóng băng trong báo cáo chuyển tiếp `draft.22 → draft.23` và đưa vào portal (commit `9f13bea`). Lúc làm đã kiểm bằng oasdiff v1.26.1 (exit 1 → 0, changelog live `No changes detected`) và `docs-selftest.mjs` (`API_DOCS_SELFTEST_PASS`, portal 15 file). Xoay baseline không phải phê duyệt: `TARGET_CONTRACT_V1` vẫn `DRAFT`.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0202; §9 completion record 'Work ID: W-0202'; activity A-0573, A-0574
- §9 completion record W-0201, residual (5): hai lối ra (a)/(b) trình owner
- 9f13bea (commit duy nhất)
- docs/api-changelog.md (mục xoay baseline draft.22 → draft.23)
- specs/api/openapi/baselines/ivr-order-confirmation.v1.0.0-draft.23.yaml
- docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.22-to-v1.0.0-draft.23.md

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Việc quyết định hợp đồng theo phương án (a) của owner: xoay baseline so sánh OpenAPI sang draft.23 và đóng băng báo cáo 11 cảnh báo; không đổi code, test hay gate runner của sweep. Danh sách 4 tài liệu nằm trong khai báo.

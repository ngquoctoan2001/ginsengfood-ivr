# W-0206 — Sửa dòng script `observability_helm` làm GitLab từ chối cả pipeline

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Pipeline GitLab hosted đầu tiên sau thời gian dài không hề được tạo vì GitLab từ chối cả config: một dòng script của job `observability_helm` (`grep -q 'name: OTEL_EXPORTER_OTLP_ENDPOINT' …`) chứa `: ` mà không có quoting của YAML nên bị đọc thành mapping. Commit `4d0c761` bọc dòng đó trong nháy kép ở `deploy/ci/observability.gitlab-ci.yml`. Cùng commit vá gate `ci-config-selftest.mjs`, vốn vẫn in `CI_CONFIG_SELFTEST_PASS` khi lỗi nằm trong cây: nay nó đi qua file gốc và mọi include cục bộ, đòi mọi mục `script`/`before_script`/`after_script` parse ra string, in `SCRIPT_ENTRY_STRING_PASS`, và đã được kiểm bằng mutation đưa lỗi trở lại. Dòng ledger do W-0217 ghi hồi tố vì A-0588 từng bỏ qua id này.

## Nguồn

- 4d0c761 (commit duy nhất: deploy/ci/observability.gitlab-ci.yml + deploy/ci/scripts/ci-config-selftest.mjs)
- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0206 (ghi hồi tố bởi W-0217); activity A-0588 (lý do từng bỏ qua id)
- docs/evidence/W-0217/README.md §1
- a4a0cee (W-0217 ghi dòng hồi tố)
- deploy/ci/scripts/ci-config-selftest.mjs (khối chú thích 'W-0206' từ dòng 325; in SCRIPT_ENTRY_STRING_PASS ở dòng 860)
- deploy/ci/observability.gitlab-ci.yml:63

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `ci-config-selftest.mjs`.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0206 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.

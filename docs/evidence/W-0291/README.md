# W-0291 — Vá js-yaml của công cụ CI

Ngày 14/09/2026. Baseline `main@d44834e`. **TESTS_PASS**, không phải ACCEPTED.

Full security wrapper lần đầu dừng với exit 1: NuGet HIGH gate PASS, npm audit phát hiện js-yaml 4.3.1 HIGH. Dependency đi qua swagger-parser → json-schema-ref-parser → js-yaml. [Advisory GHSA-2883-xcg3-v3hh](https://github.com/advisories/GHSA-2883-xcg3-v3hh) và npm registry xác nhận bản vá 4.3.2; lỗi CPU exhaustion khi parse YAML merge sources rỗng. Owner đã duyệt metadata egress npm/NuGet và scan trước khi chạy.

`npm update js-yaml --package-lock-only --ignore-scripts --no-audit --no-fund` trong bản sao tạm chỉ đổi version, resolved và integrity của một package. Áp lockfile đó, không thêm dependency trực tiếp hoặc override. Không đổi source/runtime/API contract.

`npm ci` đạt; tree xác nhận js-yaml 4.3.2. [OpenAPI regression](openapi-regression.log): lint, validate, drift, negative fixture, docs và CI config đều exit 0; dựng portal vào thư mục tạm tạo 17 file. Không thay generated API source hoặc spec.

[Security trước vá](security-before.log) có npm HIGH. [Sau vá](security-after-dependency.log): NuGet HIGH PASS, npm 0 vulnerability, `CT-CI-04` bắt secret giả đúng exit 42. Gitleaks quét 343 commit/51.44 MB, còn **68 cảnh báo**, wrapper exit 1; chưa có full security PASS.

[Bảng rà từng cảnh báo](reviewed-findings.json): 44 hash của report W-0286 khớp source tại candidate được report ghi; 6 hash portal khớp source cùng commit (kiểm cả byte LF/CRLF). 18 dòng còn lại đã đọc đúng commit: 10 seed tên feature flag, 2 chú thích trích tên đó, 2 input idempotency sai định dạng, 2 mẫu PII giả, 1 JWT chữ ký giả và 1 ví dụ credential sandbox local. Không tìm thấy credential thật trong 68 dòng đã rà.

Danh sách 68 fingerprint commit/file/rule/dòng đã chuẩn bị ở `.artifacts/w0291/gitleaks-exceptions.proposed.txt`. Automatic approval review ban đầu từ chối cập nhật persistent `.gitleaksignore`. Owner sau đó trả lời **Duyệt đúng 68 fingerprint đã rà**; đã áp đúng 68 dòng vào ignore, không thêm phạm vi khác. Rule và negative control giữ nguyên. [Full wrapper sau duyệt](security-final.log) **PASS, exit 0**: NuGet HIGH PASS, npm 0 vulnerability, negative control bắt secret giả, Gitleaks 343 commit không còn finding. Lịch sử được quét tại d44834e với lock/ignore W-0291; pipeline phải quét lại commit sau khi task này được lưu.

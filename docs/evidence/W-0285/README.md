# W-0285 — Kiểm tra hiện trạng hosted CI trước yêu cầu Hạ tầng

Ngày 2026-09-14. Baseline local `main@fa7877c`. Trạng thái **BLOCKED_EXTERNAL — thiếu quyền đọc hosted metadata**.

## Đã xác minh

- `git ls-remote --heads origin refs/heads/main` và lệnh tương đương `github` cùng trả `890dfdd732982b36c73ba7e09f6cb7f7ab96d57c`. Hai commit local W-0283/W-0284 chưa push; không có bằng chứng hosted cho chúng.
- Git credential helper hiện có đọc được Git transport. API chỉ đọc của đúng project trả **403** cho project, runners, pipelines, protected main, approvals, environments, registry. Không đủ dữ kiện để kết luận nguyên nhân là scope token, policy hay dịch vụ trung gian. Không in/lưu token, không đọc biến CI bí mật.
- Trình duyệt tới GitLab chuyển về trang đăng nhập; không có phiên đăng nhập sẵn dùng cho việc rà soát.
- [Metadata quan sát](hosted-observation.json) giữ thời điểm, SHA và HTTP status. Các trường runner/current pipeline/staging là **UNVERIFIED**, không phải MISSING.
- W-0061/W-0093 đã chứng minh hosted CI lịch sử: runner `55115499`, tag `ginsengfood-docker`, DinD, pipeline `2760238052` tại `001d2f57`, 9 jobs/281 tests. Không dùng kết quả này làm PASS của ứng viên mới.

## Điều chỉnh đã hoàn tất

[Phiếu Platform](../../../plan/ivr-orther/questions-to-platform-ci-and-staging-2026-09-12.md) giữ `NOT_SENT`, bỏ khẳng định chưa từng chạy runner; yêu cầu xác minh/tái sử dụng runner và Registry có sẵn, đọc gói/quyền hiện tại trước khi trình nâng cấp. Không cấp thêm quyền, không mua gói, không gửi phiếu, không push và không tạo nhánh thử.

Phải giải quyết bằng chứng review/enforcement G-GITLAB trong luật `main`-only hiện hành; quy trình MR lịch sử không cấp phép tạo nhánh mới. Cần phiên đăng nhập hoặc quyền API đọc phù hợp để xác minh pipeline/runner/branch controls. Sau khi có quyền và ứng viên được push theo chỉ đạo owner, lấy pipeline/jobs/digest đúng SHA; staging chỉ đóng bằng endpoint/credential/smoke thật. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Follow-up 14/09 sau `cc4a8c8`: đọc lại cả hai remote vẫn cùng `890dfdd`; tab GitLab vẫn ở trang đăng nhập. W-0283..W-0286 đã commit local, chưa push. API 403 là quan sát ban đầu được giữ lại, không phải lượt API mới. Trạng thái vẫn **BLOCKED_EXTERNAL**.

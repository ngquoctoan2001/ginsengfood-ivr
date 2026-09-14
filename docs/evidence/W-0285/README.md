# W-0285 — Kiểm tra hiện trạng hosted CI trước yêu cầu Hạ tầng

Ngày 2026-09-14. Baseline ban đầu `main@fa7877c`. **Đã đọc hosted qua phiên đăng nhập; BLOCKED_EXTERNAL còn ở pipeline candidate, staging và bằng chứng review độc lập.**

## Cập nhật sau đăng nhập, baseline local `9ca529b`

[Quan sát có đăng nhập](authenticated-observation.json): runner `55115499` online, tag đúng, Docker executor; không cần suy đoán thiếu runner. Pipeline [2842779996](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2842779996) tại `890dfdd7` tạo 36 job, 7 FAIL: 4 SDK mismatch, sweep thiếu Docker/.NET, TTS thiếu yaml, image pause sai expected result. [W-0287](../W-0287/README.md) xử lý cấu hình; chưa có pipeline tại các commit local mới.

`main` protected, Maintainers được push/merge; MR yêu cầu pipeline thành công và không chấp nhận skipped pipeline. Điều này chưa chứng minh review độc lập hay kiểm tra bắt buộc trước direct push trong chính sách main-only. Không thay branch controls.

Registry có `w0061-proof`/1 tag lịch sử. Dev/staging/lab có tên nhưng **0 deployment**; chỉ `api-docs-nonprod` có deployment cũ thành công. Không đọc giá trị secret, không gửi phiếu, không tạo runner, không push/deploy. API 403 dưới đây là quan sát lịch sử; browser login đã cho phép đọc metadata.

## Quan sát ban đầu trước đăng nhập (giữ lịch sử)

- `git ls-remote --heads origin refs/heads/main` và lệnh tương đương `github` cùng trả `890dfdd732982b36c73ba7e09f6cb7f7ab96d57c`. Hai commit local W-0283/W-0284 chưa push; không có bằng chứng hosted cho chúng.
- Git credential helper hiện có đọc được Git transport. API chỉ đọc của đúng project trả **403** cho project, runners, pipelines, protected main, approvals, environments, registry. Không đủ dữ kiện để kết luận nguyên nhân là scope token, policy hay dịch vụ trung gian. Không in/lưu token, không đọc biến CI bí mật.
- Trình duyệt tới GitLab chuyển về trang đăng nhập; không có phiên đăng nhập sẵn dùng cho việc rà soát.
- [Metadata quan sát](hosted-observation.json) giữ thời điểm, SHA và HTTP status. Các trường runner/current pipeline/staging là **UNVERIFIED**, không phải MISSING.
- W-0061/W-0093 đã chứng minh hosted CI lịch sử: runner `55115499`, tag `ginsengfood-docker`, DinD, pipeline `2760238052` tại `001d2f57`, 9 jobs/281 tests. Không dùng kết quả này làm PASS của ứng viên mới.

## Điều chỉnh đã hoàn tất

[Phiếu Platform](../../../plan/ivr-orther/questions-to-platform-ci-and-staging-2026-09-12.md) giữ `NOT_SENT`, bỏ khẳng định chưa từng chạy runner; yêu cầu xác minh/tái sử dụng runner và Registry có sẵn, đọc gói/quyền hiện tại trước khi trình nâng cấp. Không cấp thêm quyền, không mua gói, không gửi phiếu, không push và không tạo nhánh thử.

Phải giải quyết bằng chứng review/enforcement G-GITLAB trong luật `main`-only hiện hành; quy trình MR lịch sử không cấp phép tạo nhánh mới. Cần phiên đăng nhập hoặc quyền API đọc phù hợp để xác minh pipeline/runner/branch controls. Sau khi có quyền và ứng viên được push theo chỉ đạo owner, lấy pipeline/jobs/digest đúng SHA; staging chỉ đóng bằng endpoint/credential/smoke thật. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Follow-up 14/09 sau `cc4a8c8`: đọc lại cả hai remote vẫn cùng `890dfdd`; tab GitLab vẫn ở trang đăng nhập. W-0283..W-0286 đã commit local, chưa push. API 403 là quan sát ban đầu được giữ lại, không phải lượt API mới. Trạng thái vẫn **BLOCKED_EXTERNAL**.

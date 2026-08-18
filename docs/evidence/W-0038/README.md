# W-0038 — Evidence: Code review gate & static analysis (`P5-4`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS` (local); hosted GitLab evidence `NOT_RUN` — xem §5

## 1. Phần lớn cổng đã có; thứ thiếu là bằng chứng chúng chặn được

Đối chiếu §6 trước khi viết:

| Mục §6 | Hiện trạng |
| --- | --- |
| 1. analyzer as error, format check | **đã có** — `TreatWarningsAsErrors`, `EnforceCodeStyleInBuild`, `AnalysisLevel=latest-recommended` |
| 2. secret/vuln scan | **đã có** — gitleaks, `dotnet list package --vulnerable`, `npm audit` (P0-2) |
| 3. coverage gate | **đã có** — ngưỡng 80 từ `W-0035` |
| 4. `docs/review-checklist.md` + MR template | template **đã có**, checklist **thiếu** |
| 5. reviewer guide | **thiếu hoàn toàn** |
| §8 bốn self-test chứng minh cổng chặn | **thiếu hoàn toàn** |
| `deploy/ci/quality-gate.gitlab-ci.yml` | **không tồn tại** |

Nói cách khác: cái máy làm được thì đã làm; **chưa ai chứng minh nó từ chối cái gì**, và phần con người phải xét thì chưa được viết ra.

## 2. Bốn self-test — cổng được kiểm từ chiều nó có thể sai

Một cổng chỉ từng thấy xanh là một cổng không ai biết hình dạng. `review-gate-selftest.mjs` đưa **công cụ thật** một đầu vào cố ý sai và đỏ nếu công cụ hài lòng.

| Test | Cách chứng minh |
| --- | --- |
| `CT-GATE-01` | **Trồng** một symbol `SetOrderState` vào `src/Ivr.Domain/`, chạy `IT-FAILGATE-01`, đòi nó đỏ, rồi gỡ file ra trong `finally` |
| `CT-GATE-02` | Ghi một chuỗi 10 chữ số dạng MSISDN vào một artifact tạm và đòi `scan-pii.sh` thoát khác 0 |
| `CT-GATE-03` | Chạy `Ivr.CiPolicy coverage` trên fixture `low` với ngưỡng 80 và đòi nó đỏ |
| `CT-GATE-04` | Ba chiều: mô tả rỗng → chặn; **chính template chưa điền** → chặn; mô tả đầy đủ → **qua** |

`CT-GATE-04` cố ý kiểm cả chiều dương. Một checker từ chối mọi thứ cũng không phải cổng — nó chỉ là một bức tường, và bức tường thì người ta trèo qua bằng cách tắt nó.

`CT-GATE-01` trồng file vào cây nguồn thật rồi dọn trong `finally`. Đã kiểm `git status src/` sau khi chạy: không còn dấu vết.

## 3. Lỗ hổng thật: template có từ P0-2, nhưng không ai kiểm nó được điền

`.gitlab/merge_request_templates/Default.md` tồn tại từ P0-2 với đầy đủ checklist governance. **Không có gì kiểm nó được điền vào.**

Một template không ai xác minh sẽ thành một hình dạng người ta xoá nội dung đi và tick qua — đọc như traceability mà không mang gì.

`check-mr-traceability.mjs` kiểm bốn thứ (MASTER-05): Work ID **thật** (không phải placeholder), prompt ID, ít nhất một dòng mapping đã điền có trỏ `docs/evidence/W-XXXX/` và có residual gate, và **mọi checkbox đã tick**.

Một chi tiết nhỏ nhưng cố ý: khi gặp `W-XXXX`, thông báo nói **"vẫn là placeholder"** chứ không nói "không có Work ID". Bản đầu tôi viết regex `W-\d{4}` nên nó báo "không có" cho một dòng rõ ràng đang có — và **một cổng báo sai lý do là cổng người ta thôi tin**.

## 4. Reviewer guide — viết từ lỗi thật của chính dự án này

`docs/reviewer-guide.md` không phải danh sách chung chung. §1 của nó — *"test có đang chứng minh điều nó tuyên bố không"* — dựng trên **ba lần trong chính repo này** một test đã xanh vì lý do sai:

- `UT-ELIG-DNC-02` xanh vì một field chết không rule nào đọc;
- helper `Evidence(capturedAt: null)` rơi vào nhánh mặc định hợp lệ nên ca "không có dấu thời gian" vẫn có dấu thời gian;
- `UT-TRACE-01` bản đầu khớp "có nhắc tới ID" trong khi header generator nhắc một ID trong văn xuôi.

Kèm một cách xử lý cụ thể: yêu cầu tác giả **phá code tạm** và cho thấy test đỏ. Một phút, đổi lấy một cổng thật.

Guide cũng ghi §6 — **fail-closed đóng về chiều nào** — vì voice restriction và trust skip trong repo này đóng **ngược nhau**, và gộp chúng vào một cờ là âm thầm chọn một thiệt hại.

## 5. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `review-gate-selftest.mjs` | `CT-GATE-01..04 PASS`, `REVIEW_GATE_SELFTEST_PASS` |
| `check-mr-traceability.mjs --file <template>` | **đỏ** đúng lý do (placeholder + thiếu mapping) |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` (giờ phủ cả quality-gate), `API_DOCS_SELFTEST_PASS` |
| `ci-config-selftest.mjs` | `CI_CONFIG_SELFTEST_PASS` |
| `scan-pii.sh docs/evidence` | `PII_SCAN_PASS files=78` |
| `git status src/` sau self-test | sạch — file trồng đã được gỡ |

**Cổng bắt chính file này.** Bản nháp evidence viết nguyên văn con số dùng làm fixture âm cho `CT-GATE-02`, và `scan-pii.sh` chặn ngay. Đúng hành vi mong muốn — nhưng đáng ghi lại: một tài liệu **mô tả** một cổng vẫn phải sống dưới cổng đó. Đã diễn đạt lại thay vì nới pattern, cùng nguyên tắc `A-0190`/`W-0076`.

**Hosted GitLab evidence: `NOT_RUN`.** §10 nói rõ local reproduction không thay thế hosted proof. `W-0061` vẫn `BLOCKED_EXTERNAL`, nên chưa có MR thật nào chạy qua `mr_traceability_gate`. Bốn self-test chứng minh **công cụ** từ chối đúng thứ; chúng không chứng minh **GitLab** đã chặn một merge.

## 6. Cái này KHÔNG chứng minh

- **Không có bằng chứng hosted.** Chưa MR thật nào bị chặn bởi cổng này (`W-0061`).
- **Không có GitLab SAST/Semgrep/SonarQube.** §6.1-6.2 để chúng là tuỳ chọn theo tier/runner; tier hiện tại chưa có, và bật một scanner không chạy được sẽ tạo một job đỏ vĩnh viễn rồi bị `allow_failure` hoá — đúng thứ §11 cấm.
- **Không có ZAP baseline.** Cần service chạy trong pipeline; thuộc `P7-3`.
- **Không có MR note bot** (§6.6 đánh dấu optional; cần project access token mà `W-0061` chưa mở).
- **`TESTS_PASS` là trần.** Chỉ reviewer/owner chuyển `ACCEPTED`.

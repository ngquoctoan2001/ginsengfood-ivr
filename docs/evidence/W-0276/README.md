# W-0276 — Ba nguồn nói baseline nào là hiện hành, giờ phải nói giống nhau

Ngày: 2026-09-10 · Baseline: `main@9d9be51` · Trạng thái: **TESTS_PASS**.

Việc còn treo từ mục 8 của [`W-0275`](../W-0275/README.md). Không cần quyết định của owner.

## 1. Vấn đề

Baseline OpenAPI nào đang là hiện hành được viết ở **ba** chỗ, và **không gì kiểm chúng khớp nhau**.
Ngày 2026-09-10 chúng nói ba số khác nhau:

| Nguồn | Nó cho là hiện hành |
| --- | --- |
| `docs/api-changelog.md` | `draft.23` |
| `docs/api/changelog/ivr-order-confirmation.md` | `draft.24` |
| `deploy/ci/docs.gitlab-ci.yml` | `draft.25` |

Mỗi cái trôi ở một release khác nhau — dấu hiệu rõ rằng không ai đọc chéo. `W-0275` kéo cả ba về
`draft.26`; lượt này dựng thứ giữ chúng ở đó.

## 2. Luật được chọn, và luật **không** được chọn

Cách hiển nhiên là bắt cả ba bằng nhau. **Không làm vậy**, vì nó sẽ đóng đinh đúng cái quy ước yếu
hơn.

Để baseline **chậm một release** so với spec mới là cách tốt hơn: đó là điều kiện duy nhất để
`oasdiff breaking` thật sự soi được diff. Đẩy cả baseline lẫn spec lên trong cùng một commit — đúng
thứ `draft.25` và `draft.26` đều đã làm — khiến diff đó **không bao giờ được kiểm**.

Nên `CT-CI-12` đòi **tài liệu khớp thực tế**, không đòi baseline bằng current:

| Khẳng định | |
| --- | --- |
| Mọi tham chiếu baseline trong pipeline cùng trỏ một bản | nếu không, "the baseline" vô nghĩa |
| Bản đó tồn tại trong repo | |
| Cột *Baseline* của `docs/api-changelog.md` = baseline CI đang so | |
| Cột *Current* của nó = `version:` thật của spec | |
| Tiêu đề report = `# API Changelog <baseline> vs. <current>` | |

Đã kiểm cả hai chiều: cấu hình **baseline chậm một release, tài liệu khai đúng** → **PASS**.

## 3. Chứng minh có răng

| Tiêm | Thông điệp |
| --- | --- |
| index nói baseline `draft.25`, CI so `draft.26` | *"docs/api-changelog.md says the baseline is 1.0.0-draft.25, but docs.gitlab-ci.yml compares against …"* |
| index nói current `draft.24`, spec là `draft.26` | *"…says the current contract is 1.0.0-draft.24, but the spec is 1.0.0-draft.26."* |
| header report lệch | nêu cả chuỗi thật lẫn chuỗi đúng, kèm *"Regenerate it in the oasdiff image rather than editing the header"* |
| CI trỏ baseline không tồn tại | *"…compares against …draft.99.yaml, which is not in the repository."* |

Regex viết bằng `[.]`, `[/]`, `[|]` thay vì dấu `\` — file này đã bị công cụ nuốt backslash **bốn
lần** trong phiên, và một pattern suy biến âm thầm để lại một check xanh mà không nhìn gì cả.

## 4. Một lỗi của tôi trong lượt này

Bản viết đầu gọi `assertReadable(baselineFile)` — **hàm đó không tồn tại**; tôi bịa ra một helper
nghe hợp lý. Selftest vẫn *"PASS"* vì `assert` phía trên đã chạy xong và lời gọi hỏng nằm sau… thực
tế nó chưa bao giờ chạy tới trong lượt xanh đầu tiên. Đã thay bằng `fs.access` trong `try/catch`, và
đó là khẳng định thứ tư ở mục 3 — nay có tiêm thử chứng minh nó chạy thật.

## 5. Kết quả

`GATE_SWEEP_PASS 39/39`.

Một lượt sweep giữa chừng báo 37/39 với `failed to connect to the docker API` ở `dr-selftest` và
`lab-converter-selftest`. Không phải thay đổi này: cả hai cần Docker, engine vừa chập chờn, và lượt
chạy ngay sau đó xanh cả 39. Ghi lại vì con số 37/39 đã hiện ra.

Không chạm mã nguồn .NET, không file nào bị pin → **không cascade**.

## 6. Còn lại

- **`IvrDashboardSimPanel.adapter_mode` mang hai từ vựng** — cần owner quyết `AdminReadService` trả
  gì khi không có channel (`W-0275` §2).
- **Chạy hosted CI** để đóng hai điều `W-0275` §6 không kiểm được ở máy dev: `oasdiff breaking` trên
  enum mới, và bản so sánh `draft.25 → draft.26`.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN

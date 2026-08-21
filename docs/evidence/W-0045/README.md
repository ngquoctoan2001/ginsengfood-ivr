# W-0045 — Evidence: CD pipeline & environment promotion (`P7-3`)

Ngày: `2026-08-18` · Trạng thái: **`TESTS_PASS` cho static/configuration gates** — 5/5 test §8 xanh ở mức cấu hình;
**mọi evidence deploy là `NOT_RUN` / `BLOCKED_EXTERNAL`**

## 1. Điều phải nói trước mọi thứ khác

**Chưa pipeline nào trong repo này từng chạy.** Không có runner, không có registry, không có
credential cluster cho môi trường thật (`W-0061`, `W-0063` — cả hai `BLOCKED_EXTERNAL`).

`P7-3` §10 nói thẳng: *"YAML/local simulation không được gọi là deploy proof"*. Slice này không tự
nhận khác. Cái được chứng minh là **hình dạng** của pipeline **không thể diễn đạt** những thứ
governance cấm — không phải là nó chạy đúng.

Phân biệt đó quan trọng nhất ở `IT-CD-REAL-03`: không ai hứa được rằng một lượt chạy tương lai sẽ
không mở gọi thật. Cái chứng minh được **hôm nay** là **không job nào trong repo set cờ đó**, nên
mở nó sẽ phải là một lần sửa file nhìn thấy được, chứ không phải một biến ai đó lật.

## 2. Không job nào set `REAL_CUSTOMER_CALL_ALLOWED` — con số đúng là **không**

Ladder (`README-governance` §6) ghi cờ này là `false (immutable)` ở dev, staging và lab, và `false`
ở prod cho tới khi có DF-03. Vậy số job pipeline được phép set nó là **không**, chứ không phải
"một, cẩn thận".

Mở real call là một **admin action** có permission riêng, audit và four-eyes
(`specs/api/03-admin-api.md`). Một pipeline lật được nó sẽ làm chữ ký DF-03 thành trang trí.

`IT-CD-REAL-03` quét **toàn bộ** YAML CI (root + 12 fragment) và đỏ nếu bất kỳ dòng không-phải-comment
nào gán giá trị true-ish. Thêm nữa, hai job promotion **từ chối** một `IVR_EXECUTION_MODE` khác
`MOCK` thay vì chuyển tiếp nó.

## 3. `when: manual` là cơ chế, **không phải toàn bộ cổng**

`when: manual` quyết định *có cần người bấm không*. **Ai** được bấm là do *protected environments*
của GitLab quyết — và đó là cấu hình trong project, **không nằm trong repo**.

Nên file YAML **không tự chứng minh được four-eyes**, và tôi không viết như thể nó chứng minh được.
Ghi ở đây thay vì ngụ ý trong comment. Cấu hình protected environment + approver là `NOT_RUN`, phụ
thuộc `W-0061`.

## 4. Bảy quyết định, mỗi cái vì một cách hỏng cụ thể

| Quyết định | Vì cách hỏng nào |
| --- | --- |
| scan **trước** push | quét cái đã nằm trong registry nghĩa là image xấu **đã tồn tại** và ai cũng pull được |
| ghi **digest**, không chỉ tag | tag nêu một *ý định*, digest nêu *bit*; evidence phải trích được cái thứ hai |
| `needs:` chứ không chỉ thứ tự stage | thứ tự stage chỉ nói "sau"; `needs` nói "không tới được nếu job kia đỏ" |
| `resource_group` mỗi environment | hai pipeline cùng upgrade một release là cách một migration áp dở gặp một rollback dở |
| `--atomic` **và** `after_script` rollback | hai loại hỏng khác nhau: rollout không lên nổi, và rollout *Kubernetes coi là khoẻ* trong khi smoke đỏ |
| tag immutable `semver-sha` | một tag di chuyển được là một tag không trả lời được "cái gì đang chạy" |
| `rollback_prod` cùng `resource_group` với deploy | rollback đua với một upgrade đang bay là cách release dừng ở revision không ai chọn |

## 5. Cái bẫy lớn nhất: **migration không lùi cùng release**

`helm rollback` đưa **manifest** về revision cũ. Nó **không** hoàn tác migration mà Job
`pre-upgrade` đã áp — schema đã đổi. Lùi image về bản cũ trong khi schema đã tiến lên nghĩa là chạy
**code cũ trên schema mới**.

Ràng buộc thật nằm ở **cách viết migration** (tương thích lùi một bậc: thêm cột nullable, không đổi
tên hay xoá cột trong cùng release với code dùng nó), **không** nằm ở rollback. Và **chưa test nào
ép điều đó**. Ghi ra vì rollback tự động là thứ khiến người ta quên nó.

## 6. Kiểm chứng

| Test | Kiểm âm dựng lên | Kết quả |
| --- | --- | --- |
| `IT-CD-DEV-01` | gỡ `needs: publish_images` | ❌ đỏ đúng lý do |
| `IT-CD-GATE-02` | `allow_failure: true` trên promotion | ❌ đỏ |
| `IT-CD-REAL-03` | thêm `REAL_CUSTOMER_CALL_ALLOWED: "YES"` | ❌ đỏ |
| `IT-CD-ROLLBACK-04` | gỡ `--atomic` | ❌ đỏ |
| `IT-CD-CONCURRENCY-05` | gỡ `resource_group` | ❌ đỏ |

Cổng topology cũng có ba kiểm âm: gỡ include CD, gỡ stage `promote`, đặt `cd_selftest`
`allow_failure: true` — cả ba đỏ đúng thông báo.

| Lệnh | Kết quả |
| --- | --- |
| `cd-selftest.mjs` | `CD_SELFTEST_PASS (configuration only)` |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` — 12 fragment root-included |
| `scan-pii.sh` | `PII_SCAN_PASS` |

**Một chỗ suýt yếu, đã siết.** Bản đầu của `IT-CD-ROLLBACK-04` chấp nhận khi chuỗi `helm rollback`
xuất hiện **bất kỳ đâu trong file** — nghĩa là một *comment* cũng làm nó xanh. Đúng lớp lỗi
"khớp văn xuôi" vừa mắc ở `W-0044` (assertion đỏ vì tài liệu của chính nó). Giờ nó chỉ đọc wiring
đã resolve của **chính job đó**.

**Và một chi tiết parser thật.** Thư viện `yaml` **không** tự resolve merge key `<<` trong khi GitLab
có. Không bật `merge: true` thì một job kế thừa `allow_failure` từ anchor trông như không khai gì —
checker báo vi phạm không tồn tại, hoặc tệ hơn, **bỏ sót** vi phạm vì chỉ đọc anchor.

## 7. Cái này KHÔNG chứng minh

- **Không lượt deploy nào.** `IT-CD-DEV-01`, `-GATE-02`, `-REAL-03`, `-ROLLBACK-04`, `-CONCURRENCY-05`
  đều là **cấu hình**. Deploy thật, approval thật, rollback thật: **`NOT_RUN`**.
- **Không có registry** (`W-0061`), nên `publish_images` chưa từng push và **chưa digest nào tồn tại**.
- **Không có protected environment / approver** cấu hình, nên four-eyes trên promotion là `NOT_RUN`.
- **Không có canary/blue-green** (`P7-3` §6.5 nêu "nếu được chọn") — chưa chọn, chưa dựng.
- **Chưa test nào ép migration tương thích lùi** (§5) — đó là khoảng trống thật, không phải chi tiết.
- ~~Tên môi trường lệch nhau giữa hai nguồn.~~ **Đã chốt `2026-08-19`** (`OD-OPEN-03`): dùng `lab`.
  `pilot` nghe như đã có khách thật, mà bậc đó **chưa** cho gọi khách. Chart, CI và tài liệu đã đổi;
  guard ladder giờ báo `Only lab and prod`.

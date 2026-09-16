# W-0253 — Xoá console khỏi repo, giữ lại đúng thứ Module 3 cần

Ngày: 2026-09-09 · Baseline: `main@b3eb43b` · Trạng thái: **TESTS_PASS**.

Owner chốt từ `2026-09-05` rằng console thuộc Module 3 và `admin-ui/` không phải deliverable. Nhưng
nó vẫn nằm trong **4 file CI**, một Dockerfile, một Helm template và cả `package.json` gốc — tức
repo vẫn *hành xử* như đang duy trì một sản phẩm mà chủ sở hữu đã nói là không làm nữa. Lượt này
dẹp khoảng cách đó.

**191 tệp đổi, −27.828 dòng.**

## 1. Giữ lại một thứ, và giữ có lý do

`admin-ui/src/i18n/enums.vi.json` → **`specs/ui/enum-labels.vi.json`**, đổi tên chứ không đổi một
byte (`git` ghi nhận rename thuần, `| 0`).

Hai lý do:

1. **Một test backend đọc nó.** `ConsoleEnumDictionaryTests` (`W-0107 §6.2`) lấy CHECK constraint
   ra khỏi EF model rồi đòi mỗi giá trị hợp lệ phải có nhãn tiếng Việt. Đây là lớp phủ **duy nhất**
   cho các tập giá trị *không bao giờ lên wire* — `account.status`, `intake_outbox.status`,
   `approval_type` — nên xoá nó là mất lớp kiểm ấy, không phải dọn rác.
2. **Module 3 sẽ cần đúng 42 họ nhãn này.** Họ dựng console trên cùng bộ enum. Đặt ở `specs/ui/`
   là đặt cạnh chín đặc tả màn hình họ sẽ đọc, chứ không phải chôn trong một thư mục đã xoá.

## 2. Xoá gì

| Nhóm | Cụ thể |
| --- | --- |
| Mã nguồn | `admin-ui/**` — 167 tệp tracked, cộng `.next/` build output còn sót trên đĩa |
| CI | job `build_lint_ui` (`ci.gitlab-ci.yml`), cả file `ui-qa.gitlab-ci.yml` + include ở gốc, anchor `.admin_ui_cache`, entry `needs:` trong `pii_scan` |
| Image | `deploy/docker/Dockerfile.ui`; vòng lặp build trong `cd.gitlab-ci.yml` thu từ `api worker ui migrate` về `api worker migrate` (3 chỗ); build UI trong `k8s.gitlab-ci.yml` |
| Công cụ | `tools/dev/Capture-ConsoleEvidence.mjs` — 483 dòng chỉ phục vụ console đã xoá |
| Script gốc | `package.json`: bỏ `dev`, thu gọn `setup` |

## 3. Giữ gì, và tại sao không xoá

**Helm guard giữ nguyên.** `deployment-ui.yaml` không deploy gì — nó chỉ `fail` khi ai đó đặt
`ui.enabled=true`. Xoá template thì cờ ấy thành **no-op im lặng**; giữ lại thì bật nhầm là đỏ ngay.
Chỉ sửa lời cho đúng hiện trạng.

**`prompt/phase-3-admin-ui/` giữ nguyên.** Sổ `prompt-execution-tracker` trỏ tới `P3-1..P3-4`
**19 lần** cho công việc đã thực sự làm; xoá bốn tệp ấy là làm mồ côi sổ kiểm. Thay vào đó
`prompt/00-index.md` ghi thẳng ở đầu mục: đây **không còn là chỉ dẫn để làm**.

**Bài học bảo mật giữ lại, đổi khung.** `deploy/docker/README.md` kể chuyện `ivr-admin-ui` bản đầu
có 7 HIGH + 1 CRITICAL và cả 8 nằm trong npm đi kèm base image. Image không còn, nhưng bài học —
gỡ package manager khỏi image production — đúng cho **mọi** base image, nên nó ở lại dưới dạng bài
học chứ không phải mô tả hiện trạng.

## 4. Hai assertion đã sai sẵn, lộ ra khi rà

- **`image-selftest.mjs` đòi `ivr-admin-ui` phải là service đang chạy trong compose** — nhưng
  `docker-compose.dev.yml` **chưa bao giờ** định nghĩa service đó (nó có `postgres`, `mock-sim`,
  `mock-jwt`, `ivr-migrate`, `ivr-api`, `ivr-worker`, `fake-sales`). Assertion ấy sẽ đỏ với bất kỳ
  ai chạy nó. Không ai chạy: `image-selftest` cần docker nên nằm trong nhóm `sweepable: false` của
  `W-0251`. Đúng khuôn câu chuyện `W-0251` kể — gate không ai chạy thì sai lặng lẽ.
- **`security-scan.sh` chạy `npm audit` trên `admin-ui`** — sau khi xoá thư mục thì lệnh này gãy CI.
  Đã gỡ.

## 5. Một assertion bị **đảo chiều**, không phải bị xoá

`docs-selftest.mjs` từng đòi *"root config phải include UI QA fragment"*. Xoá assertion là mất luôn
ý định. Thay bằng assertion ngược:

```js
assert(
  !includes.some((entry) => entry.includes("ui-qa")),
  "Root GitLab config must not include a UI QA fragment: Module 3 owns the operator console.",
);
```

Quyết định của owner nay **được máy giữ**: repo không thể mọc lại pipeline UI mà không ai biết.

## 6. Kiểm chứng

```text
dotnet build                               0 Warning(s), 0 Error(s)
dotnet test Ivr.sln                        952/952 PASS, 0 failed, 0 skipped   (không đổi)
gate-sweep.mjs                             GATE_SWEEP_PASS 39/39 run, 18 skipped (exit=0)
ci-config-selftest.mjs                     CI_CONFIG_SELFTEST_PASS
docs-selftest.mjs                          DOC_CI_TOPOLOGY_PASS · API_DOCS_SELFTEST_PASS
YAML                                       16/16 file CI + compose + helm values parse sạch
include ở .gitlab-ci.yml                   13/13 giải được, 0 thiếu
enum-labels.vi.json                        rename thuần, 0 dòng đổi
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1` vẫn `DRAFT`.

## 7. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| — | `W-0254`: sửa các phát biểu đã sai trong `IR-06` và dựng gói tích hợp M3 | tôi, ngay sau đây |
| — | `specs/ui/**` giờ là bề mặt M3 đọc để dựng console — chín đặc tả màn hình cộng bộ nhãn | dev M3 |

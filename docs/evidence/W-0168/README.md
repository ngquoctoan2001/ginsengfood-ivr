# W-0168 — Current security, secret and vulnerability gate rerun

Ngày: `2026-09-03`

Exact commit: `main@b21ec676e490`

Trạng thái: **`TESTS_PASS_LOCAL / SECURITY_GATE_PASS / CURRENT_TREE_GITLEAKS_PASS /
HOSTED_CI_NOT_RUN / NO_GATE_PROMOTION`**

## 1. Phạm vi

- chạy nguyên bản `deploy/ci/scripts/security-scan.sh` với `CI_COMMIT_SHA=b21ec676e490`;
- gate gồm locked NuGet restore, transitive vulnerability policy mức HIGH, npm audit mức HIGH cho
  `admin-ui` và `deploy/ci`, Gitleaks negative control và git-history scan;
- chạy thêm Gitleaks `dir` trên current shared working tree với output redact;
- không sửa allowlist/source/config để che finding và không lưu secret vào evidence.

## 2. Boundary

Đây là local evidence cho D7. Nó không chứng minh hosted CI, secret-store custody, vendor sandbox,
staging/UAT hoặc production readiness. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 3. Kết quả

### 3.1 Findings đã sửa

1. `deploy/ci/package-lock.json` khóa transitive dev dependency `fast-uri@3.1.5`, bị bốn advisory
   HIGH; package đang cài cục bộ đã là bản vá nhưng CI audit đọc lockfile nên vẫn đỏ. Chỉ đổi exact
   lock entry lên `3.1.7`; không đổi direct dependency hoặc production runtime.
2. `security-scan.sh` luôn tải Linux binary nên Git Bash/Windows lỗi `Exec format error`. Thêm OS
   switch: Linux giữ nguyên `.tar.gz/gitleaks`, MINGW/MSYS/CYGWIN dùng `.zip/gitleaks.exe`; cả hai
   tiếp tục kiểm official checksum trước khi chạy.
3. History scan lộ 64 match/58 fingerprint duy nhất và current-tree scan lộ 51 match/50
   fingerprint duy nhất. Review xác nhận đều là generated EF feature-flag seed, generated API
   HTML/manifest digest, pinned model revision, synthetic scenario/negative-test value hoặc SBOM
   public build metadata. Chỉ thêm exact `commit:path:rule:line` hoặc `path:rule:line`; không nới
   detector, regex hoặc miễn cả file.

### 3.2 Verification

| Gate | Kết quả |
| --- | --- |
| GitNexus pre-edit impact `security-scan.sh` | **LOW** — 0 process/module; dependency lock không phải runtime symbol |
| Shell syntax + CI topology selftest | **PASS** — `CI_CONFIG_SELFTEST_PASS` |
| `npm ci --ignore-scripts --dry-run` | **PASS** — lock resolution nhận `fast-uri 3.1.7` |
| NuGet transitive vulnerability policy HIGH | **PASS** — 0 vulnerability |
| `admin-ui` npm audit HIGH | **PASS** — 0 vulnerability |
| `deploy/ci` npm audit HIGH | **PASS** — 0 vulnerability |
| Gitleaks official archive checksum | **PASS** — Windows x64 archive verified |
| Gitleaks negative control | **PASS** — fake GitHub PAT bị từ chối, exit 42 |
| Gitleaks git history | **PASS** — 141 commits, không leak |
| Gitleaks current shared working tree | **PASS** — khoảng 256 MB, không leak, redact enabled |
| Full security wrapper | **PASS** — `SECURITY_SCAN_PASS gitleaks=8.30.0 nuget=HIGH npm=HIGH` |
| W-0168 PII scan | **PASS** — 1/1 Markdown |
| API docs + traceability | **PASS** — 14 generated artifacts; 476 tagged test current |
| Gate mirror | **PASS** — 11 gates, 166 work items, 23 open decisions, production=false |
| Markdown map | **PASS** — 656 Markdown files, 871 resolved, 199 unresolved global backlog; W-0168/target 0 unresolved |
| `git diff --check` | **PASS** — chỉ line-ending warnings của shared working tree |

## 4. Kết luận

D7 current local security evidence đã đóng. Không secret value nào được ghi vào evidence; mọi scan
output dùng redact. Thay đổi source duy nhất là portability branch của CI security wrapper; direct
dependency, application runtime, OpenAPI, DB và production config không đổi.

Hosted GitLab pipeline, external secret-store custody/rotation, vendor sandbox, staging/UAT và
production vẫn chưa được chứng minh. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

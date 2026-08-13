# PROMPT P0-2 — CI Baseline & Quality Gates

## 0. Meta
| | |
| --- | --- |
| **ID** | `P0-2` |
| **Work ID** | `W-0011` (canonical tracker §5) |
| **Phase** | 0 — Foundation & Project Setup |
| **Prereq (blockedBy)** | `P0-1` |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · Next.js · GitLab CI (`CONFIRMED_2026-08-12`) |

## 1. ROLE
Bạn là **DevOps / Build Engineer**. Bạn dựng GitLab CI làm "người gác cổng chất lượng" — mọi Merge Request (MR) phải qua build, test, lint, coverage, security scan và OpenAPI lint trước khi merge. Bạn thiết kế gate rõ ràng, fail nhanh, log dễ đọc.

## 2. CONTEXT
Sau khi có solution (P0-1), cần CI tự động để mọi prompt sau có "definition of done" đo được. CI này là nền của **traceability + MASTER-05** (không merge nếu thiếu test/evidence). Nó cũng là bộ khung để P5-4 (code-review gate) và P7-3 (CD) mở rộng.

## 3. SOURCE SPECS (đọc trước)
- `prompt/README-governance.md` §4 (coding standards), §5 (traceability bắt buộc mỗi GitLab MR)
- `specs/testing/00-index.md`, `specs/testing/01-strategy.md`
- `specs/api/06-error-codes.md` (để lint envelope sau này), `specs/api/openapi/ivr-order-confirmation.v1.yaml`
- `plan/ivr-orther/decisions-log.md` §DF-02 (OpenAPI validate CI)

## 4. DECISIONS & CONSTRAINTS
- **DF-02:** OpenAPI 3.1 phải validate trong CI (parse + lint contract).
- **MASTER-05 / governance §5:** MR thiếu traceability (source/req/contract/test/evidence) → block.
- **Coverage:** đặt ngưỡng khởi điểm (VD line ≥ 60% foundation, nâng dần theo phase; core slice ≥ 80% ở P5).
- **CI provider:** **GitLab CI** đã được owner xác nhận; không tạo hoặc duy trì GitHub Actions workflow.
- **Entrypoint:** GitLab chỉ nhận pipeline repository từ `.gitlab-ci.yml` ở root (trừ khi project setting trỏ file khác). Các fragment dưới `deploy/ci/` phải được root file `include` rõ ràng.
- **Pipeline routing:** Merge Request pipeline dùng `CI_PIPELINE_SOURCE == "merge_request_event"`; default-branch push chạy lại full gate. Không tạo đồng thời branch pipeline và MR pipeline cho cùng một commit.
- **Hosted proof:** validate YAML/local commands không thay thế pipeline GitLab thật, protected-branch setting hoặc merge-check evidence.

## 5. INPUTS / DEPENDENCIES
- Repo P0-1 (solution + admin-ui).
- Tools: `dotnet test` + coverage collector (coverlet), ESLint (admin-ui), OpenAPI linter (Spectral hoặc `redocly lint`), security scan (`dotnet list package --vulnerable` + Trivy/Grype cho image sau P7), secret scan (gitleaks).

## 6. BUILD STEPS
1. Tạo root **`.gitlab-ci.yml`** với `workflow:rules` cho `merge_request_event`, default branch và manual web pipeline có chủ đích. Root file `include` các fragment versioned dưới `deploy/ci/`; không dùng remote include không pin.
2. Tạo các stage/job tối thiểu, `allow_failure: false` cho quality gates:
   - `build_test_dotnet`: restore → build warnings-as-errors → `dotnet test` + JUnit/Cobertura artifacts; fail dưới coverage threshold. Negative self-test phải xác minh **đúng test đã discover và cố tình fail** / **đúng báo cáo coverage thấp**, không được coi mọi exit khác `0` (như typo path/tool crash) là PASS.
   - `lint_dotnet`: analyzers + `dotnet format --verify-no-changes`.
   - `build_lint_ui`: `npm ci` → lint → test nếu có → production build.
   - `openapi_lint`: parse/lint toàn bộ `specs/api/openapi/*.yaml`, kiểm local `$ref` và schema fixtures (DF-02).
   - `security_scan`: vulnerable NuGet/npm packages theo severity policy + gitleaks (secret patterns); JSON NuGet phải có schema/version/parameters/projects hợp lệ và severity ngoài catalog phải fail closed (`{}` không phải báo cáo sạch). GitLab SAST/Semgrep có thể thêm nếu runner/tier hỗ trợ nhưng không được làm gate cơ bản biến mất.
   - `pii_scan` (**gate riêng, `allow_failure: false`**): quét test output, `docs/evidence/**` và artifact của
     **mọi job phía trước**. **gitleaks KHÔNG phải PII scanner** — nó khớp secret, không khớp số điện thoại
     hay địa chỉ, nên hai job phải tách riêng.

     Scanner phải duyệt **mọi regular text file** dưới target, không whitelist extension; vì vậy `.sql`, file không extension và loại text artifact mới đều được phủ. File binary được bỏ qua có đếm rõ; target không tồn tại hoặc target không cho ra text file nào phải FAIL closed, không được `PASS files=0`.

     **Topology thu thập artifact (bắt buộc).** GitLab job chạy cô lập: nếu không khai `needs`/`dependencies`
     thì `pii_scan` **không nhìn thấy** artifact của job khác và sẽ xanh giả. Dùng đúng một trong hai:

     - **(A) Gate tập trung (mặc định):** `pii_scan` khai `needs: [build_test_dotnet, build_lint_ui, openapi_lint]`
       với `artifacts: true` để tải artifact về workspace, rồi quét. Job nào sinh artifact mới phải được thêm
       vào `needs` — `CT-CI-08` bắt lỗi nếu thiếu.
     - **(B) Gate tại nguồn:** mỗi job sinh artifact tự chạy scanner **trước** bước `artifacts:` upload.
       An toàn hơn (artifact bẩn không bao giờ rời job) nhưng lặp cấu hình.

     Không được chọn "không cái nào". Ghi rõ phương án đã chọn trong `deploy/ci/README.md`.

     **Pattern** lưu trong `deploy/ci/pii-patterns.txt`, một pattern mỗi dòng (**không** nhúng inline vào YAML
     để tránh escape bị hỏng).

     Hai ràng buộc bắt buộc, đều đã đo bằng `grep` thật (không phải bằng regex engine khác):

     1. **`grep -i` không fold được tiếng Việt.** Đo: `grep -iE '(số nhà|đường)'` chỉ khớp 1/3 dòng ở
        `LC_ALL=C`, `LC_ALL=C.UTF-8` lẫn UTF-8 locale. Không được dựa vào `-i`.
     2. **Bracket expression đa byte (`[Đđ]`, `[ốỐ]`) vỡ dưới `LC_ALL=C`.** Đo: pattern dùng lớp ký tự chỉ
        bắt 3/8 dòng dưới `LC_ALL=C` (sót `đường`, `Đường`, `ĐƯỜNG`, `SỐ NHÀ`), và `Ngõ` khớp chỉ vì trùng
        byte `0xC3` — **khớp nhầm, không phải đúng ngữ nghĩa**. Container CI tối giản rất hay ở `LC_ALL=C`.

     Vì vậy pattern phải là **alternation literal** (chuỗi byte UTF-8 nguyên vẹn), không dùng lớp ký tự cho
     ký tự có dấu:

     ```text
     (^|[^0-9])(0|84|\+84)[0-9]{9}([^0-9]|$)
     ((đ|Đ)(ư|Ư)(ờ|Ờ)(n|N)(g|G)|(d|D)(u|U)(o|O)(n|N)(g|G))
     ((s|S)(ố|Ố) (n|N)(h|H)(à|À)|(s|S)(o|O) (n|N)(h|H)(a|A))
     ((n|N)(g|G)(õ|Õ)|(h|H)(ẻ|Ẻ)(m|M)|(n|N)(g|G)(á|Á)(c|C)(h|H)|(n|N)(g|G)(a|A)(c|C)(h|H))
     ((t|T)(h|H)(ô|Ô)(n|N) |(ấ|Ấ|a|A)(p|P) |(t|T)(ổ|Ổ) )
     [Dd][Ii][Aa][Ll][_-]?[Tt][Oo][Kk][Ee][Nn]["'`: ]+[A-Za-z0-9._-]{8,}
     ```

     Lớp ký tự ASCII (`[Dd]`, `[0-9]`) vẫn an toàn ở mọi locale nên `dial_token` giữ nguyên dạng đó.
     Với cụm tiếng Việt, alternation literal được đặt **theo từng ký tự** để vừa an toàn ở `LC_ALL=C`, vừa
     bắt được cả kiểu viết hoa/thường trộn như `ĐưỜnG`, `Số NHÀ`, `nGáCh`. Nó cũng bắt thêm dạng **không
     dấu** (`duong`, `so nha`, `ngach`) mà lớp ký tự bỏ sót hoàn toàn.

     Job phải **đặt `LC_ALL=C.UTF-8` tường minh** và chạy `grep -nEf deploy/ci/pii-patterns.txt <targets>`,
     **fail khi có match**. Không được phụ thuộc locale mặc định của runner.
3. Cấu hình cache có key/lockfile phù hợp; artifacts có expiry; JUnit/Cobertura dùng `artifacts:reports`; không log secret và không cache credential.
4. Tạo GitLab MR template `.gitlab/merge_request_templates/Default.md`: bảng Source/Decision → Contract/Migration → Test ID/command → Evidence → residual gate, cùng checklist no order transition, fail-closed, PII masked và MODE=MOCK.
5. Tạo `CODEOWNERS` ở root để route review theo vùng. Document rằng approval rule/CODEOWNERS enforcement phụ thuộc GitLab tier/project settings và phải có hosted evidence.
6. Trong `deploy/ci/README.md`, hướng dẫn GitLab project settings: protected default branch/no direct push, MR approval, **Pipelines must succeed**, runner tags/capability, masked/protected CI/CD variables. Các setting chưa cấu hình phải ghi `NOT_RUN`, không suy ra từ YAML.
7. Thêm badge GitLab pipeline và hướng dẫn chạy chính các script/commands ở local. `gitlab-ci-local` chỉ là optional renderer/simulator; failure do Windows thiếu `/bin/bash` không được gọi là pipeline failure, và render pass không phải hosted proof.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `.gitlab-ci.yml` | GitLab pipeline entrypoint + workflow rules/includes |
| `deploy/ci/ci.gitlab-ci.yml` | Các quality job được root pipeline include |
| `.gitlab/merge_request_templates/Default.md` | Traceability + governance checklist |
| `CODEOWNERS` | Routing review theo vùng |
| `deploy/ci/README.md` | Local commands, GitLab runner, protected branch/merge checks, ngưỡng |
| `deploy/ci/pii-patterns.txt` | Bộ pattern PII (một pattern/dòng, ERE, **alternation literal** cho ký tự có dấu — KHÔNG dùng lớp ký tự đa byte) — job `pii_scan` đọc bằng `grep -nEf` dưới `LC_ALL=C.UTF-8` |
| `.gitleaks.toml`, spectral/redocly config | Cấu hình scan/lint |

**Chuẩn output:** job idempotent, cache dependency, log rõ pass/fail từng gate. Không secret trong YAML/log; dùng GitLab masked/protected CI/CD variables. Không có `.github/workflows/*` hoặc GitHub Actions dependency trong CI active.

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `CT-CI-01` | ci-selftest | MR có OpenAPI cố tình sai → job `openapi_lint` FAIL. |
| `CT-CI-02` | ci-selftest | Test đã discover `CtCi02DeliberatelyFails` và fail với marker dự kiến → gate PASS self-test; typo/missing project không được tính là expected failure. |
| `CT-CI-03` | ci-selftest | Báo cáo fixture đo đúng coverage dưới ngưỡng → gate PASS self-test; missing report/path không được tính là low-coverage evidence. |
| `CT-CI-04` | ci-selftest | Commit chứa secret giả → gitleaks fail. |
| `CT-CI-05` | config/rules | MR, default-branch push và manual web source render đúng job; không sinh duplicate branch+MR pipeline. |
| `CT-CI-06` | ci-selftest | Commit chứa số điện thoại/địa chỉ giả trong test output hoặc evidence → job `pii_scan` FAIL. |
| `CT-CI-06b` | ci-selftest | **Chữ HOA phải bị bắt**: `123 Đường Nguyễn Huệ`, `123 ĐƯỜNG NGUYỄN HUỆ`, `SỐ NHÀ 45`, `NGÕ 12`, `NGÁCH 3`, `DIAL_TOKEN: abc12345xyz` → FAIL. Và **không** false-positive trên `560000 VND`, `Quận 7`, `Quan 7`, `Phường Bến Nghé`, `Thành phố Thủ Đức`. |
| `CT-CI-06d` | ci-selftest | **Locale independence**: chạy lại đúng bộ fixture của `CT-CI-06b` dưới `LC_ALL=C`, `LC_ALL=C.UTF-8` và `LC_ALL=POSIX` — kết quả phải **giống hệt** ở cả ba. Test này bắt lỗi bracket expression đa byte; nếu ai đó đổi pattern về dạng `[Đđ]` thì test đỏ. |
| `CT-CI-06e` | ci-selftest | **Dạng không dấu**: `123 duong Nguyen Hue`, `SO NHA 45`, `NGACH 3` → FAIL (bộ pattern phủ cả biến thể không dấu). |
| `CT-CI-06f` | ci-selftest | **Hoa/thường trộn**: `123 ĐưỜnG Nguyễn Huệ`, `Số NHÀ 45`, `nGáCh 3`, `HẻM 9` → FAIL. Chứng minh alternation theo từng ký tự không bỏ lọt biến thể case mà không cần `grep -i`. |
| `CT-CI-06c` | ci-selftest | **Artifact liên job**: PII được cài vào artifact của `build_test_dotnet` (không phải workspace của `pii_scan`) → `pii_scan` vẫn FAIL. Chứng minh `needs`/`dependencies` thực sự tải artifact về. |
| `CT-CI-06h` | ci-selftest | `.sql` và text file không extension chứa PII đều FAIL; missing target và target chỉ có binary đều fail closed, không `PASS files=0`. |
| `CT-CI-08` | config/rules | Mọi job có `artifacts:` đều nằm trong `needs` của `pii_scan` (phương án A) **hoặc** tự chạy scanner trước upload (phương án B); thiếu job → FAIL. |
| `CT-CI-07` | config/rules | Mọi file dưới `deploy/ci/*.gitlab-ci.yml` đều reachable từ root `.gitlab-ci.yml` (render pipeline liệt kê đủ job); fragment mồ côi → FAIL. |
| `CT-CI-09` | ci-selftest | NuGet vulnerability fixture sạch PASS; High, `{}`/schema thiếu và severity lạ đều FAIL closed với lý do phân biệt. |
| `CT-CI-10` | config/contract | Exact set 16 error codes phải giống nhau giữa OpenAPI, API-06 và `IvrErrorCodes`; lệch bất kỳ phía nào → FAIL. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] 6 job chạy độc lập (gồm `pii_scan`), fail đúng khi vi phạm; rule routing được chứng minh bằng §8.
- [ ] `pii_scan` nhìn thấy artifact của job khác (`CT-CI-06c`), bắt được chữ HOA tiếng Việt (`CT-CI-06b`), cho kết quả **giống nhau ở 3 locale** (`CT-CI-06d`), phủ dạng không dấu (`CT-CI-06e`) và mọi text artifact bất kể extension (`CT-CI-06h`).
- [ ] Negative test/coverage self-test xác minh đúng semantic failure; vulnerability JSON fail closed khi schema hoặc severity không hợp lệ (`CT-CI-02/03/09`).
- [ ] Stable error catalog exact-set parity giữa OpenAPI/API-06/source (`CT-CI-10`).
- [ ] `deploy/ci/pii-patterns.txt` **không** chứa lớp ký tự cho ký tự có dấu (chỉ alternation literal); job đặt `LC_ALL=C.UTF-8` tường minh.
- [ ] OpenAPI lint đúng DF-02; secret/vuln scan bật.
- [ ] MR template ép traceability.
- [ ] Root `.gitlab-ci.yml` include đúng fragment; repository không có GitHub Actions CI active.

**Reviewer:** kiểm gate không thể bypass qua GitLab protected branch/merge checks; ngưỡng coverage hợp lý; runner image/tag/capability rõ.

## 10. EVIDENCE EXPECTED
Local/config evidence: commands tương đương, YAML/rules render, coverage artifact format, OpenAPI lint và gitleaks/vuln output. Hosted evidence: 1 GitLab MR xanh + các negative pipeline đỏ, protected-branch/merge-check screenshot/export và runner identity. Khi chưa có GitLab project/runner, hosted evidence phải là `NOT_RUN`/`BLOCKED_EXTERNAL`.

## 11. FORBIDDEN
- ❌ Cho phép merge khi gate đỏ hoặc đặt `allow_failure: true` cho quality gate.
- ❌ In secret ra log CI.
- ❌ Nới ngưỡng coverage bằng cách loại trừ file bừa bãi (chỉ exclude generated code + khai báo rõ).
- ❌ Tạo GitHub Actions workflow hoặc yêu cầu GitHub status check.

## 12. DEFINITION OF DONE
- [ ] `.gitlab-ci.yml` và 5 gate GitLab CI hoạt động; 5 self-test §8 chứng minh routing/fail đúng.
- [ ] MR template, CODEOWNERS và branch/merge policy được cấu hình hoặc ghi trung thực `NOT_RUN` nếu chưa có hosted access.
- [ ] Local/config evidence đủ để đạt `TESTS_PASS`; chỉ nâng evidence hosted/settings khi pipeline GitLab thật đã chạy và được reviewer chấp nhận.

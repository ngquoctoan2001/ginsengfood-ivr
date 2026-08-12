# PROMPT P0-2 — CI Baseline & Quality Gates

## 0. Meta
| | |
| --- | --- |
| **ID** | `P0-2` |
| **Phase** | 0 — Foundation & Project Setup |
| **Prereq (blockedBy)** | `P0-1` |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · Next.js · (CI: GitHub Actions default — `NEED_CONFIRMATION`) |

## 1. ROLE
Bạn là **DevOps / Build Engineer**. Bạn dựng pipeline CI làm "người gác cổng chất lượng" — mọi PR phải qua build, test, lint, coverage, security scan, và OpenAPI lint trước khi merge. Bạn thiết kế gate rõ ràng, fail nhanh, log dễ đọc.

## 2. CONTEXT
Sau khi có solution (P0-1), cần CI tự động để mọi prompt sau có "definition of done" đo được. CI này là nền của **traceability + MASTER-05** (không merge nếu thiếu test/evidence). Nó cũng là bộ khung để P5-4 (code-review gate) và P7-3 (CD) mở rộng.

## 3. SOURCE SPECS (đọc trước)
- `prompt/README-governance.md` §4 (coding standards), §5 (traceability bắt buộc mỗi PR)
- `specs/testing/00-index.md`, `specs/testing/01-strategy.md`
- `specs/api/06-error-codes.md` (để lint envelope sau này), `specs/api/openapi/ivr-order-confirmation.v1.yaml`
- `plan/ivr-orther/decisions-log.md` §DF-02 (OpenAPI validate CI)

## 4. DECISIONS & CONSTRAINTS
- **DF-02:** OpenAPI 3.1 phải validate trong CI (parse + lint contract).
- **MASTER-05 / governance §5:** PR thiếu traceability (source/req/contract/test/evidence) → block.
- **Coverage:** đặt ngưỡng khởi điểm (VD line ≥ 60% foundation, nâng dần theo phase; core slice ≥ 80% ở P5).
- **CI provider:** default **GitHub Actions**; viết dạng dễ port (job tách rõ) — đánh `NEED_CONFIRMATION` nếu team dùng GitLab/Azure DevOps.

## 5. INPUTS / DEPENDENCIES
- Repo P0-1 (solution + admin-ui).
- Tools: `dotnet test` + coverage collector (coverlet), ESLint (admin-ui), OpenAPI linter (Spectral hoặc `redocly lint`), security scan (`dotnet list package --vulnerable` + Trivy/Grype cho image sau P7), secret scan (gitleaks).

## 6. BUILD STEPS
1. **CI workflow** `deploy/ci/ci.yml` (GitHub Actions) chạy trên PR + push:
   - Job `build-test-dotnet`: restore → build (warnings-as-errors) → `dotnet test` với coverage → upload report; fail nếu coverage < ngưỡng.
   - Job `lint-dotnet`: analyzers/format check (`dotnet format --verify-no-changes`).
   - Job `build-lint-ui`: `npm ci` → `npm run lint` → `npm run build` (admin-ui).
   - Job `openapi-lint`: chạy Spectral/redocly trên `specs/api/openapi/*.yaml`; fail nếu invalid (DF-02).
   - Job `security-scan`: `dotnet list package --vulnerable --include-transitive` (fail nếu High/Critical) + gitleaks (secret scan).
2. **Coverage gate**: cấu hình ngưỡng trong pipeline; xuất báo cáo (cobertura) làm artifact.
3. **PR template** `.github/pull_request_template.md`: bảng traceability bắt buộc (Source spec · Requirement/Decision ID · Contract · Test case ID · Evidence) + checklist governance (không transition order, fail-closed, PII masked, MODE=MOCK).
4. **Branch protection** (mô tả trong `deploy/ci/README.md`): require CI pass + ≥1 review + no direct push to `main`.
5. **CODEOWNERS** (tuỳ chọn): route review theo thư mục.
6. Thêm badge trạng thái + hướng dẫn chạy CI local (act/nektos hoặc script tương đương) vào README.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `deploy/ci/ci.yml` | Pipeline chính (jobs tách) |
| `.github/pull_request_template.md` | Traceability + governance checklist |
| `.github/CODEOWNERS` | (tuỳ chọn) routing review |
| `deploy/ci/README.md` | Cách chạy/branch policy/ngưỡng |
| `.gitleaks.toml`, spectral/redocly config | Cấu hình scan/lint |

**Chuẩn output:** job idempotent, cache dependency, log rõ pass/fail từng gate. Không secret trong workflow (dùng CI secrets store).

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `CT-CI-01` | ci-selftest | PR mở với OpenAPI cố tình sai → job `openapi-lint` FAIL. |
| `CT-CI-02` | ci-selftest | Test cố tình fail → pipeline đỏ, block merge. |
| `CT-CI-03` | ci-selftest | Coverage dưới ngưỡng → gate fail. |
| `CT-CI-04` | ci-selftest | Commit chứa secret giả → gitleaks fail. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] 5 job chạy độc lập, fail đúng khi vi phạm (4 self-test §8 chứng minh).
- [ ] OpenAPI lint đúng DF-02; secret/vuln scan bật.
- [ ] PR template ép traceability.

**Reviewer:** kiểm gate không thể bypass (branch protection); ngưỡng coverage hợp lý theo phase; provider-portable.

## 10. EVIDENCE EXPECTED
Ảnh chụp/log 1 PR xanh + 1 PR đỏ (mỗi loại gate fail), coverage report artifact, OpenAPI lint output, gitleaks/vuln scan log.

## 11. FORBIDDEN
- ❌ Cho phép merge khi gate đỏ (không `continue-on-error` ở gate chất lượng).
- ❌ In secret ra log CI.
- ❌ Nới ngưỡng coverage bằng cách loại trừ file bừa bãi (chỉ exclude generated code + khai báo rõ).

## 12. DEFINITION OF DONE
- [ ] CI chạy trên PR; 5 gate hoạt động; 4 self-test §8 chứng minh fail đúng.
- [ ] PR template + branch policy áp dụng.
- [ ] Evidence §10 đủ.

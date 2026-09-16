# PROMPT P5-4 — Code Review Gate & Static Analysis

## 0. Meta
| | |
| --- | --- |
| **ID** | `P5-4` · **Phase** 5 — Quality Engineering |
| **Work ID** | `W-0038` (canonical tracker §5) |
| **Prereq** | `P0-2` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · analyzers · GitLab CI |

## 1. ROLE
Bạn là **Staff Engineer / Reviewer**. Bạn thiết lập "cổng review": static analysis, security scan, coverage gate và **checklist review chuẩn** ép mọi GitLab MR phải chứng minh đúng ranh giới module + governance trước khi merge. Bạn tự động hoá cái máy làm được, và định nghĩa cái người phải xét.

## 2. CONTEXT
Chất lượng không chỉ nằm ở test; cần gate review nhất quán để không lọt vi phạm ranh giới (transition order, PII, bypass blocker) hay nợ kỹ thuật. Prompt này hoàn thiện CI (P0-2) thành **quality gate đầy đủ** + tài liệu review cho người.

## 3. SOURCE SPECS (đọc trước)
- `prompt/README-governance.md` §4/§5, `specs/testing/00-index.md`, `specs/testing/08-acceptance-criteria.md`
- `specs/_review/normalization-report.md` (chuẩn nhất quán), `specs/api/06-error-codes.md`
- `plan/ivr-orther/decisions-log.md` (mọi ràng buộc để review)

## 4. DECISIONS & CONSTRAINTS
- **MASTER-05:** MR thiếu traceability (source/req/contract/test/evidence) → block; completion report ≠ gate pass.
- **Governance bất biến:** review chặn nếu có transition order (D-02), PII leak (D-05), bypass blocker (DO-*), gọi thật khi MOCK, mã lỗi ngoài §1c.
- **Static analysis:** Roslyn analyzers/StyleCop as error; GitLab SAST/Semgrep/SonarQube tùy runner/tier, vuln + secret scan (P0-2).
- **Coverage gate:** ngưỡng theo phase; không tụt.

## 5. INPUTS / DEPENDENCIES
- CI P0-2; test suites P5-1/2/3; analyzers.

## 6. BUILD STEPS
1. **Static analysis** bắt buộc: analyzers as error, format check, optional GitLab SAST/Semgrep/Sonar; fail MR nếu vi phạm.
2. **Security/secret/vuln scan** (mở rộng P0-2): GitLab SAST/Semgrep + ZAP baseline khi phù hợp; fail High/Critical theo policy.
3. **Coverage gate** theo phase; report diff coverage (không giảm).
4. **MR review checklist** `docs/review-checklist.md`: governance (no order transition, PII masked, fail-closed, MODE=MOCK, error §1c), traceability, test backing, performance/security note. Tích hợp vào GitLab MR template (P0-2).
5. **Reviewer guide**: hạng mục cần con người xét (race-guard, idempotency key reuse, snapshot freshness, taxonomy mapping) — nơi máy khó bắt.
6. **MR note bot** (optional): dán checklist + kết quả gate vào GitLab MR bằng project access token tối thiểu quyền; không bắt buộc nếu tier/credential chưa sẵn.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `deploy/ci/quality-gate.gitlab-ci.yml` | Static + security + coverage jobs, được `.gitlab-ci.yml` include |
| `docs/review-checklist.md`, `docs/reviewer-guide.md` | Checklist + guide |
| `.gitlab/merge_request_templates/Default.md` (mở rộng) | Governance + traceability |

## 8. TESTS TO WRITE (self-test gate)
| Test ID | Loại | Assert |
| --- | --- | --- |
| `CT-GATE-01` | ci-selftest | MR có transition order (vi phạm D-02 giả) → reviewer checklist/analyzer bắt. |
| `CT-GATE-02` | ci-selftest | MR PII leak trong log → security scan/test fail. |
| `CT-GATE-03` | ci-selftest | coverage tụt → gate fail. |
| `CT-GATE-04` | ci-selftest | thiếu traceability trong MR → block. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] gate không bypass được; [ ] checklist phủ governance bất biến; [ ] coverage không tụt; [ ] reviewer guide rõ.
**Reviewer:** cân bằng auto vs người; false-positive hợp lý; gate map MASTER-05.

## 10. EVIDENCE EXPECTED
Gate config, self-test GitLab MR (4 loại fail đúng), review checklist/guide, coverage diff report. Hosted GitLab evidence phải ghi `NOT_RUN` nếu project/runner/merge settings chưa sẵn; local reproduction không thay thế hosted proof.

## 11. FORBIDDEN
- ❌ Cho merge khi gate đỏ. ❌ Checklist bỏ qua governance bất biến. ❌ Tắt analyzer/scan để "cho xanh". 

## 12. DEFINITION OF DONE
- [ ] Quality gate + checklist + guide; 4 self-test §8 chứng minh; evidence §10 đủ. **Kết thúc Phase 5: chất lượng đo được, review có cổng.**

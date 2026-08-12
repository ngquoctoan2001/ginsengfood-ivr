# PROMPT {ID} — {Tên slice}

> Đây là **template chuẩn** cho mọi prompt trong thư viện A–Z. Mỗi prompt là **một đơn vị công việc giao cho AI coding agent / dev** để thực thi trọn vẹn (code + test + review + evidence), không phải mô tả suông. Điền hết các mục; xóa phần hướng dẫn in nghiêng.

---

## 0. Meta
| | |
| --- | --- |
| **ID** | `{P{phase}-{seq}}` |
| **Phase** | `{0..11}` — {tên phase} |
| **Prereq (blockedBy)** | *{các prompt phải xong trước, VD `P0-3`, `P1-2`}* |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_EXECUTION_MODE=MOCK` *(trừ Phase 8/9 có ghi rõ)* |
| **Stack** | .NET 10 / PostgreSQL / Next.js / Docker+K8s *(theo phần liên quan)* |
| **Work ID** | *ID kế tiếp/chính xác trong `_execution/prompt-execution-tracker.md`; bắt buộc trước khi code* |
| **Execution mode** | `MOCK` mặc định; `LAB_REAL_SIM`/`PRODUCTION_REAL` chỉ khi prompt và gate cho phép |

## 1. ROLE (đóng vai)
*Persona rõ ràng agent phải nhập vai, kèm mức seniority + trọng tâm.* Ví dụ:
> Bạn là **Senior .NET Backend Engineer** trong team IVR của GinsengFood. Bạn viết code production-grade C#/.NET 10, ưu tiên fail-safe, idempotency, testability. Bạn KHÔNG được vượt ranh giới module (không transition order — đó là việc Order Core).

## 2. CONTEXT (ngữ cảnh — vì sao & ở đâu)
*3–6 câu: slice này nằm ở đâu trong hệ thống, nhận gì từ upstream, đưa gì cho downstream, vì sao cần. Trích ranh giới module & quyết định nền.*

## 3. SOURCE SPECS (đọc trước khi code — bắt buộc)
*Liệt kê đường dẫn spec CHÍNH XÁC agent phải đọc. Không code trước khi đọc.*
- `specs/...`
- `specs/api/openapi/ivr-order-confirmation.v1.yaml` *(nếu liên quan)*
- `plan/ivr-orther/decisions-log.md` §{...}

## 4. DECISIONS & CONSTRAINTS (ràng buộc bắt buộc)
*Trích ID quyết định (`D-*`, `DS-*`, `DO-*`, `DF-*`, `DT-*`, `DC-*`, `DTS-*`) + diễn giải thành ràng buộc kỹ thuật cụ thể. Ghi rõ cái gì "implemented ngay" vs "target/GAP".*

## 5. INPUTS / DEPENDENCIES (yếu tố đầu vào)
*Config, biến môi trường, contract, seed, service ngoài, secret. Nêu default + `NEED_CONFIRMATION` nếu chưa chốt.*

Tách từng input thành `REAL_AVAILABLE`, `MOCK_REQUIRED`, `OWNER_DECISION_REQUIRED` hoặc `BLOCKED_EXTERNAL`. Không invent production API/data. Mọi thiếu hụt mới phải thêm Work ID kế tiếp vào tracker.

## 6. BUILD STEPS (việc cần làm — chi tiết, có thứ tự)
*Các bước đánh số, đủ cụ thể để thực thi: file/class/endpoint/migration nào, hành vi ra sao. .NET/Next.js cụ thể, không generic.*
1. …
2. …

## 7. OUTPUT ARTIFACTS (đầu ra chuẩn chỉnh)
*Liệt kê file/thư mục sẽ tạo, theo layout repo. Kèm chuẩn: naming, style, header, format.*
| Path | Nội dung |
| --- | --- |
| `src/...` | … |

**Chuẩn output:** tuân `prompt/README-governance.md` §Coding-standards; mọi public API có XML doc; không magic number (dùng const/policy); log có `correlationId`.

## 7b. COMMANDS (bắt buộc — lệnh tái lập được)
*Liệt kê 2–5 lệnh chính xác chạy từ repository root để sinh evidence (vd `dotnet test tests/Ivr.UnitTests -v n`, `npm --prefix admin-ui run build`). Reviewer phải chạy lại được.*

## 8. TESTS TO WRITE (test code — bắt buộc)
*Test cụ thể phải viết, trace về `specs/testing/*` (ID test). Nêu loại (unit/integration/contract/e2e), framework (xUnit/Testcontainers/Playwright), và case chính (happy + negative + fail-closed).*
| Test ID (specs) | Loại | Assert |
| --- | --- | --- |
| `UT-...` | unit | … |

## 9. REVIEW / ACCEPTANCE GATE (tự-review + reviewer)
**Self-review checklist (agent tự kiểm trước khi nộp):**
- [ ] Trace mọi requirement ID ở §4 → code + test.
- [ ] Không vi phạm Forbidden §11.
- [ ] Fail-closed ở mọi nhánh lỗi liên quan blocker/dispatch.
- [ ] Lint/build/test xanh; coverage đạt ngưỡng phase.

**Reviewer checklist (người/agent review GitLab MR):** *{tiêu chí review riêng slice — vd race-guard, PII masking, idempotency key reuse}.*

## 10. EVIDENCE EXPECTED (bằng chứng nộp)
*Log/artifact cụ thể để chứng minh Done gate (MASTER-05). VD: migration log, test report, reject-403 sample, screenshot UI.*

## 11. FORBIDDEN (tuyệt đối không)
- ❌ IVR transition/ghi order state (D-02).
- ❌ Lưu raw phone / mapping token→số ở IVR (D-05).
- ❌ Bỏ qua blocker / gọi khách thật khi `MODE=MOCK` (DF-03/DT-01).
- ❌ Tuyên bố production-ready khi chưa qua release gate.
- *{ràng buộc riêng slice}*

## 12. DEFINITION OF DONE (DoD)
- [ ] Build + test + lint pass (CI).
- [ ] Tất cả §8 test xanh; §10 evidence sinh ra.
- [ ] Self-review §9 tick hết; GitLab MR có traceability (source/req/contract/test/evidence).
- [ ] Cập nhật doc liên quan nếu đổi contract.
- [ ] Cập nhật Work ID trong tracker với artifacts, commands/tests, evidence, blockers và việc phát sinh; append activity log.
- [ ] Ghi đúng cấp độ: mock-complete, lab-verified, integration-verified hoặc production-accepted; không gộp các cấp.

## 13. TRACKER UPDATE (bắt buộc)

- Before: Work ID + `IN_PROGRESS` + baseline/prereq.
- During: checkpoint và mọi unplanned dependency/work bằng ID tuần tự.
- After: actual files, test commands/results, evidence links, residual gates, next allowed work; chỉ reviewer/owner chuyển `ACCEPTED`.

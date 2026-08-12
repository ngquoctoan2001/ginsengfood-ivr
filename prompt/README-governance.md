# IVR Prompt Library — Governance (đọc trước mọi prompt)

Trạng thái: `LIVING` · Cập nhật: `2026-08-12` · Áp dụng cho mọi `prompt/phase-*`.

> **Numbering ổn định.** Các prompt trích dẫn `governance §N`. Không đổi số heading; nếu cần thêm mục, append vào cuối hoặc thêm sub-section trong §N hiện có.

## 1. Mục tiêu chính xác và source priority

Hoàn thiện IVR .NET/Next.js/PostgreSQL đến `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS`, sau đó chứng minh `LAB_REAL_SIM` bằng 1 SIM/allowlist, rồi mới đóng các Sales/auth/legal/32-eSIM gates để xét production. **“Prompt xong” không tự động đồng nghĩa “vận hành được”.**

Khi tài liệu mâu thuẫn, ưu tiên theo thứ tự:

1. `plan/ivr-orther/target-contract-v1-draft.md`;
2. Target V1 overlay `TV1-*` trong `plan/ivr-orther/decisions-log.md` và `specs/_review/open-decisions-register.md`;
3. OpenAPI target/current-compat (`specs/api/openapi/*.yaml`);
4. `specs/` và `integration-requirements/`;
5. `prompt/`;
6. archive/history (`_archive/`, `_legacy-mock/`, `_review/` — **không phải authority**).

`docs/documents/` là business source để truy nguyên; nó **không** tự chứng minh implementation hiện tại. Nếu Target V1 khác business source, delta phải được ghi ở `specs/_review/open-decisions-register.md` kèm owner, không được im lặng.

## 2. Bất biến governance (KHÔNG được phá ở bất kỳ phase nào)

1. `REAL_CUSTOMER_CALL_ALLOWED=NO` cho tới khi có signed production gate (DF-03).
2. Execution modes: `MOCK`, `LAB_REAL_SIM`, `PRODUCTION_REAL`; lab chỉ gọi allowlisted test numbers.
3. IVR **không** transition order, **không** process payment, **không** gửi SMS/notification.
4. Program matrix Target V1: `GOLDEN_HOUR+ONLINE` và `TWENTY_FOUR_SEVEN+COD`, đều yêu cầu `ivr_confirmation_required=true`; tổ hợp khác fail-closed. Trạng thái matrix là `TARGET_DRAFT` — xem §8.
5. Target callback là generic + semantic ACK; endpoint Golden Hour hiện tại nằm sau compat adapter và **không** được nhận kết quả 24/7.
6. Attempt policy versioned/configurable; candidate `mock-lab-v1` chỉ dùng `MOCK`/`LAB_REAL_SIM` tới khi owner sign-off. **Không hard-code candidate vào DB constraint hoặc domain constant.**
7. Speech summary phải đọc được items/qty/total/short area; cấm raw phone/full address/sensitive fields. Whitelist biến script hiện là `OWNER_DECISION_REQUIRED` — xem §8.
8. Dial bằng opaque token tại telephony boundary; logs/UI/evidence luôn masked/redacted. IVR **không** giữ mapping `dial_token→số thật`.
9. External outage hoặc missing policy/evidence → fail-closed.
10. Idempotency/correlation/audit append-only trên mọi command và provider call.
11. **Service KHÔNG share database/entity/source code với platform Java** (DTS-01). Tích hợp chỉ qua versioned HTTP/OpenAPI contract.
12. CI provider là **GitLab CI** (TV1-12). Không tạo hoặc duy trì GitHub Actions workflow.

## 3. Stack chuẩn & layout repo (tham chiếu chung)

`.NET 10` API/Worker/Domain/Infrastructure/Contracts · PostgreSQL/EF Core/outbox · Next.js strict TypeScript admin · Docker/Compose · Kubernetes/Helm target · OpenTelemetry · GitLab CI.

```
ivr/                      (repo service IVR — .NET, standalone `ginsengfood-ivr`)
  src/
    Ivr.Api/              ASP.NET Core (task intake, internal/admin API, health)
    Ivr.Worker/           BackgroundService (scheduler, dispatch, SIM, normalize, outbox)
    Ivr.Domain/           entities, value objects, policies, taxonomy (không ref Infrastructure)
    Ivr.Infrastructure/   EF Core (Postgres), outbox, SIM adapter, provider clients
    Ivr.Contracts/        DTO sinh từ OpenAPI + shared enums
  tests/
    Ivr.UnitTests/  Ivr.IntegrationTests/ (Testcontainers)  Ivr.ContractTests/
  admin-ui/               Next.js (React/TS strict)
  deploy/
    docker/               Dockerfiles
    helm/  k8s/           Chart + manifests
    ci/                   GitLab CI fragments (`*.gitlab-ci.yml`) được root `.gitlab-ci.yml` include
  db/                     migrations (EF Core)
  docs/evidence/<W-XXXX>/ evidence root theo Work ID
  .gitlab-ci.yml          GitLab pipeline entrypoint (tạo ở P0-2, không tạo sớm hơn)
  .gitlab/merge_request_templates/Default.md
  CODEOWNERS
```

**Ref direction một chiều:** `Api`/`Worker` → `Infrastructure` → `Domain`; `Domain` không ref ai. `Contracts` không ref `Infrastructure`.

*(Layout là default hợp lý; P0-1 chốt chính thức. Tên có thể đổi nhưng phải giữ separation of concerns và ref direction.)*

Providers bắt buộc (mỗi cái có port + fake): fake Sales, target Sales callback client, current GH compat callback client, mock SIM, vendor SIM, dial-token resolver, speech renderer + TTS provider, policy registry, auth token provider, retention job.

## 4. Coding standards (mọi prompt tuân)

- **C#/.NET:** `Nullable` enabled, `TreatWarningsAsErrors=true`, analyzers (`.editorconfig` + `Microsoft.CodeAnalysis.NetAnalyzers`, tùy chọn StyleCop); async all the way; `record` cho DTO; **không magic number** (dùng const/policy object/config — đặc biệt attempt policy, xem §2.6); XML doc cho public API.
- **TypeScript/Next.js:** strict mode, ESLint + Prettier, không `any` trừ khi có lý do ghi rõ; component test.
- **Log:** structured (Serilog/OpenTelemetry), luôn kèm `correlationId`, `taskId`/`orderId`; **KHÔNG log phone thô, full address, `dial_token` hoặc secret**.
- **Error:** envelope `{error:{code,message,details,correlationId}}`; `code` lấy từ catalog trong `specs/api/06-error-codes.md`.
- **Config key:** dùng canonical name ở §6; không tự đặt tên biến mode/flag mới.

## 5. Traceability bắt buộc mỗi Merge Request (thiếu 1 → không merge — MASTER-05)

`Source spec path` · `Requirement/Decision ID` · `Contract (OpenAPI/DB)` · `Test case ID (specs/testing/*)` · `Evidence item (docs/evidence/<W-XXXX>/)`.

Template: `.gitlab/merge_request_templates/Default.md` (tạo ở P0-2). Code/test pass chỉ đủ `TESTS_PASS`; external/live acceptance phải có evidence riêng.

## 6. REAL_CUSTOMER_CALL_ALLOWED ladder → environment và execution mode

**Ladder governance (không nhảy cóc):**

```
DOCS_APPROVED → CONTRACT_APPROVED → TASK_INTAKE_ENABLED → SCHEDULER_ENABLED
 → SIM_MOCK_VERIFIED → [1 SIM thật + allowlist + kill switch] → LAB_REAL_SIM_VERIFIED
 → [Sales/auth/policy/legal/32-eSIM gates + DF-03 sign-off] → REAL_CUSTOMER_CALL_ALLOWED
```

**Environment ≠ execution mode.** Đây là hai trục độc lập; không được suy diễn cái này từ cái kia:

| Environment (deployment) | `IVR_EXECUTION_MODE` cho phép | `REAL_CUSTOMER_CALL_ALLOWED` | Ghi chú |
| --- | --- | --- | --- |
| `dev` | `MOCK` | `false` (immutable) | fake Sales + mock SIM, không egress thật |
| `staging` | `MOCK` | `false` (immutable) | có thể trỏ Sales sandbox khi có credential |
| `lab` | `MOCK` hoặc `LAB_REAL_SIM` | `false` (immutable) | 1 SIM thật, **chỉ** số trong `labDestinationAllowlist`; kill switch bắt buộc |
| `prod` | `MOCK` hoặc `PRODUCTION_REAL` | `false` cho tới DF-03 sign-off | chỉ `true` sau khi mọi gate §8 đóng |

- `LAB_REAL_SIM` **không** đồng nghĩa cho phép gọi khách. Lab chỉ gọi allowlist.
- Không environment nào tự động bật real call. Bật là một admin action có permission riêng, audit và four-eyes (xem `specs/api/03-admin-api.md`).
- **Canonical config key:** `IVR_EXECUTION_MODE` (env) ↔ `executionMode` (typed config). Tên cũ `IVR_ADAPTER_MODE`/`EXECUTION_MODE` là alias lịch sử và phải normalize về canonical key.

## 7. Tracker bắt buộc

`_execution/prompt-execution-tracker.md` là **sổ tiến độ duy nhất**.

- Trước prompt: chọn Work ID đã dành ở §5 của tracker, ghi scope/prereq/owner/status.
- Trong prompt: append activity/checkpoint, decision/API mới thiếu và việc phát sinh.
- Việc ngoài plan dùng **Work ID kế tiếp trong cùng bảng**, `Origin=UNPLANNED`; không tạo backlog rời.
- Sau prompt: ghi artifacts, commands/tests, evidence, residual blockers và trạng thái.
- Không ghi `ACCEPTED` nếu reviewer/evidence chưa có; không ghi `VERIFIED` từ mô phỏng.
- Evidence ghi vào `docs/evidence/<W-XXXX>/`.

## 8. Trạng thái phụ thuộc ngoài (điều kiện, không phải blocker để bắt đầu code)

- **Chặn gọi khách thật (P0):** vendor/SIM procurement (W-0008/DT-01), release sign-off (DF-03/W-0009), Legal retention (DF-07).
- **Chặn real Sales integration:** W-0002 producer, W-0003 speech payload, W-0004 dial-token, W-0005 callback/revalidation, W-0006 auth, W-0007 attempt policy.
- **Chặn CI hosted evidence:** GitLab project/runner/registry/protected-branch — xem tracker §4 (`W-0061`).
- **`OWNER_DECISION_REQUIRED` chưa được tự quyết:** Golden Hour ONLINE matrix, `ivr_confirmation_required` business source, attempt policy cuối, speech variable whitelist, dial-token reuse semantics, production auth (JWT/mTLS), TTS vendor.
- **Prompt hóa external closure:** Phase 11 biến blocker ngoài code thành RFQ/ticket/legal/sign-off artifacts. P11 không thay quyết định owner/vendor/legal, nhưng bắt buộc tạo bằng chứng để P8/P9 đi tới production thật.

Mock chỉ giúp code tiếp tục. **Mock không bao giờ đóng external gate.**

## 9. Cách dùng

1. Đọc `00-index.md` để biết thứ tự + phụ thuộc.
2. Đọc `RUNBOOK-execute-prompts.md` và cập nhật `_execution/prompt-execution-tracker.md` **trước** khi chạy prompt.
3. Chốt `_execution/defaults-and-confirmations.md` theo gate phase (`MUST_DECIDE_BEFORE_*`).
4. Với mỗi prompt: đọc §SOURCE SPECS → thực thi §BUILD STEPS → viết §TESTS → tự-review → nộp §EVIDENCE vào `docs/evidence/<W-XXXX>/`.

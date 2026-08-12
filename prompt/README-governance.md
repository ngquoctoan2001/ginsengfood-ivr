# IVR Prompt Library — Governance (đọc trước mọi prompt)

Trạng thái: `LIVING` · Áp dụng cho **toàn bộ** `prompt/phase-*`. Mỗi prompt kế thừa các quy tắc dưới đây; prompt chỉ ghi phần *khác biệt/bổ sung*.

## 1. Mục tiêu thư viện
Đưa module **IVR Order Confirmation** từ **zero → production** bằng chuỗi prompt thực thi được (code + test + review + evidence). Stack: **.NET 10 · PostgreSQL · Next.js · Docker/Kubernetes** (DTS-01..05).

## 2. Bất biến governance (KHÔNG được phá ở bất kỳ phase nào)
1. **`REAL_CUSTOMER_CALL_ALLOWED=NO`** cho tới khi release gate (DF-03) pass ở Phase 9. Không code/flow nào tự bật.
2. **`IVR_ADAPTER_MODE=MOCK`** mặc định; `REAL` chỉ xuất hiện ở Phase 8 (sau khi mua SIM — DT-01) và chỉ bật qua env-gate prod.
3. **IVR là input signal** — KHÔNG transition order (D-02). Chỉ Order Core đổi `order_status`.
4. **Scope COD-only** (DS-01): chỉ xử lý order `CONFIRMING` + `payment_method_snapshot=COD`.
5. **Fail-closed** (DO-06): non-2xx/timeout/`ready=503` từ blocker source ⇒ không dispatch / block.
6. **PII tối thiểu** (D-05): không lưu raw phone/recording; token→số chỉ ở SIM adapter boundary; log/UI mask.
7. **Idempotency + correlation** (DF-04/DF-05): mọi command có `Idempotency-Key` + `X-Correlation-Id`; audit append-only.
8. **Không production-ready tuyên bố** khi chưa có evidence ACCEPTED (MASTER-05).

## 3. Stack chuẩn & layout repo (tham chiếu chung)
```
ivr/                      (repo service IVR — .NET)
  src/
    Ivr.Api/              ASP.NET Core (intake, callback, admin API)
    Ivr.Worker/           BackgroundService (scheduler, dispatch, SIM, normalize)
    Ivr.Domain/           entities, value objects, policies (D-10, taxonomy)
    Ivr.Infrastructure/   EF Core (Postgres), outbox, SIM adapter, clients (OrderCore/Ops/CRM)
    Ivr.Contracts/        DTOs sinh từ OpenAPI + shared enums
  tests/
    Ivr.UnitTests/  Ivr.IntegrationTests/ (Testcontainers)  Ivr.ContractTests/
  admin-ui/               Next.js (React/TS)
  deploy/                 Dockerfiles, helm/, k8s/, ci/
  db/                     migrations (EF Core)
```
*(Layout là default hợp lý; prompt Phase 0 chốt chính thức. Tên có thể đổi nhưng giữ separation of concerns.)*

## 4. Coding standards (mọi prompt tuân)
- **C#/.NET:** nullable enabled, `TreatWarningsAsErrors`, analyzers (.editorconfig) + StyleCop/Roslyn; async all the way; `record` cho DTO; không magic number (const/policy object); XML doc cho public API.
- **TypeScript/Next.js:** strict mode, ESLint + Prettier, không `any` trừ có lý do; component test.
- **Log:** structured (Serilog/OTel), luôn kèm `correlationId`, `taskId`/`orderId` (KHÔNG log phone thô).
- **Error:** envelope `{error:{code,message,details,correlationId}}`; `code` theo `specs/api/06-error-codes.md` §1c (15 mã `IVR_*`).

## 5. Traceability bắt buộc mỗi PR (thiếu 1 → không merge — MASTER-05)
`Source spec path` · `Requirement/Decision ID` · `Contract (OpenAPI/DB)` · `Test case ID (specs/testing/*)` · `Evidence item`.

## 6. REAL_CUSTOMER_CALL_ALLOWED ladder (governance gate → env)
```
DOCS_APPROVED → CONTRACT_APPROVED → TASK_INTAKE_ENABLED → SCHEDULER_ENABLED
 → SIM_INTERNAL_TEST_ENABLED (MOCK/loopback) → [mua SIM DT-01 + DF-03 sign-off] → REAL_CUSTOMER_CALL_ALLOWED
```
Map sang môi trường K8s: `dev`(MOCK) → `staging`(MOCK/loopback) → `pilot`(REAL, scope hạn chế DF-03) → `prod`. Không nhảy cóc.

## 7. Trạng thái phụ thuộc ngoài (điều kiện, không phải blocker để bắt đầu code)
- **Chặn gọi khách thật (P0):** mua SIM (DT-01), release sign-off (DF-03), Legal retention (DF-07).
- **Build cross-team (không chặn dry-run):** IR-SALES-OC1 (expose `order_version`), IR-SALES-OC2 (richer callback codes), IR-SALES-OC3 (explicit no-answer transition), DC-05 (CRM events), DC-06 (trust resolver), IR-CRM-01 (extend eligibility). Prompt Phase 4 xử lý "implemented vs target" cho từng cái.
- **Prompt hóa external closure:** Phase 11 biến các blocker ngoài code thành RFQ/ticket/legal/sign-off artifacts. P11 không tự thay thế quyết định owner/vendor/legal, nhưng bắt buộc tạo bằng chứng để P8/P9 có thể đi tới production thật.

## 8. Cách dùng
1. Đọc `00-index.md` để biết thứ tự + phụ thuộc.
2. Đọc `RUNBOOK-execute-prompts.md` và cập nhật `_execution/prompt-execution-tracker.md` trước khi chạy prompt.
3. Chốt `_execution/defaults-and-confirmations.md` theo gate phase (`MUST_DECIDE_BEFORE_*`).
4. Với mỗi prompt: đọc §3 Source specs → thực thi §6 Build steps → viết §8 Tests → tự-review §9 → nộp §10 Evidence.
5. Không nhảy phase khi prereq chưa Done gate.

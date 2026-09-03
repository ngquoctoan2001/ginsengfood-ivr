# W-0145 — M8-05 program/result contract sign-off

Ngày: `2026-09-03`

Baseline: `main@b21ec676e490`

Trạng thái: **`EVIDENCE_SUBMITTED / M8_OWNER_SIGNED / M3_PRODUCT_SIGNOFF_REQUIRED`**

Người ký phía M8: **Tôi — Module 8 / Project Owner**.

## 1. Phạm vi

- Đối chiếu M8-05 với current OpenAPI, runtime, test, IR-06 và closure pack.
- Sửa factual error trong T-01 và result-semantics docs.
- Khóa program receiver, 11-result taxonomy và stop rule phía Module 8.
- Lập exact handoff cho Module 3/Product; không sửa code/runtime/OpenAPI enum.

## 2. Phát hiện đã xử lý

| Phát hiện | Evidence trực tiếp | Xử lý W-0145 |
| --- | --- | --- |
| T-01 còn coi matrix là proposal chưa duyệt và hỏi lại Golden Hour COD/ONLINE | IR-06 §3.10–3.11 đã ghi nhận Flow 04/05 khóa `24_7+COD` và `GOLDEN_HOUR+ONLINE`; OpenAPI/runtime cùng enforce | Viết lại T-01: M8 ký receiver; M3 giao mapping/CDC; không quyết lại business pair |
| Functional/05 và DT-06 nói Sales sở hữu `IVR_CONFIRMATION_WINDOW_EXPIRED`, IVR chỉ sinh 8 result | `PostgresSchedulerStore.CloseMissedDeadlinesAsync` tại current HEAD persist result final và gọi `CallbackOutboxSnapshotFactory.Create`; thay đổi có từ commit `f291f449` | Sửa thành 11 contract / 9 runtime producer / 6 final callback / 2 pre-call compatibility |
| M8-05 gộp “policy version” vào sign-off có thể bị hiểu là policy đã duyệt | T-09 chứng minh `mock-lab-v1` chỉ `CandidateMockLabOnly`; `D-10` còn lệch phase-8 | Tách rõ M8 ký stop rule; Product/Order Core vẫn phải ký production policy trong M8-11/T-09 |

## 3. Contract đã khóa phía M8

### Program

- Accept: `GOLDEN_HOUR + ONLINE`.
- Accept: `TWENTY_FOUR_SEVEN + COD`.
- M3 map: `24_7 → TWENTY_FOUR_SEVEN`, `PHONE_VALID → VALID`,
  `ELIGIBLE_FOR_IVR → ELIGIBLE`.
- `ivr_confirmation_required=true` là assertion M3 đã quyết định `CALL_REQUIRED`; false/missing bị
  reject, không phải yêu cầu IVR quyết định lại.

### Result

| Nhóm | Số | Mã |
| --- | --- | --- |
| Contract vocabulary | 11 | Toàn bộ enum `ResultType` hiện hành |
| Runtime IVR có thể persist | 9 | Tất cả trừ `IVR_OPERATIONAL_BLOCKED`, `IVR_POLICY_BLOCKED` |
| Final và vào callback outbox | 6 | Confirmed, customer-cancelled, no-answer-final, window-expired, invalid-phone-final, capacity-exception |
| Persist non-final, không callback | 3 | No-answer-attempt, wrong-input, technical-exception |
| Pre-call compatibility, không result/callback | 2 | Operational-blocked, policy-blocked |

Chi tiết counted/final/action nằm ở
[gói ký M8-05](../../../plan/ivr-orther/m8-05-program-result-contract-signoff-2026-09-03.md).

## 4. Artifact đã cập nhật

- `plan/ivr-orther/m8-05-program-result-contract-signoff-2026-09-03.md`
- `plan/ivr-orther/dev-viec-can-lam-m8-2026-08-29.md` — row + handoff nổi bật ngay dưới M8-05
- `plan/ivr-orther/today-01-decision-signoff-pack-2026-08-29.md`
- `docs/contracts/target-v1-closure-pack/{README,T-01-program-matrix,T-05-callback-ack}.md`
- `integration-requirements/06-module-3-api-handover.md`
- `specs/functional/05-result-normalization-callback.md`
- `specs/decisions/DT-06-blocked-result-semantics.md`
- tracker, readiness/gate mirror và official Markdown map

## 5. Kiểm chứng ngày 03/09/2026

| Gate | Kết quả |
| --- | --- |
| Release build `--warnaserror` | **PASS** — 0 warning, 0 error |
| Unit: `CallResultAndMapperTests` + `TaskIntakeServiceTests` | **PASS 36/36** |
| Contract: intake schema + separation + Target ACK/error | **PASS 12/12** |
| Integration: intake 2 + scheduler window-expired 3 | **PASS_LOCAL_POSTGRES `5/5`** — W-0161 chạy full `Ivr.IntegrationTests` qua Docker/Testcontainers: `236/236`, 0 fail, 0 skip; gồm `IT-INTAKE-DB-01/02`, `IT-SCH-DEADLINE-09/11/12` |
| OpenAPI lint | **PASS** — 2 descriptions valid |
| OpenAPI parse/schema/negative | **PASS** — 2 files; task fixtures 9; schema negative 12; domain negative 13; compat checks 1/1 |
| OpenAPI pinned drift | **PASS** — 3 hash pinned, human diff current |
| API docs selftest | **PASS** — 14 generated artifact; boundary/link/topology/PII pass |
| Official Markdown map | **REGENERATED** — 621 Markdown file; signoff/evidence W-0145 đều 0 unresolved; global map còn 201 unresolved thuộc corpus rộng, không được khai là đã dọn trong W-0145 |
| Gate mirror/readiness | **PASS** — 11 gate, 143 work item, 23 open decision; rung 0; production flag false |
| `git diff --check` | **PASS** |

Ghi chú lịch sử: lần chạy W-0145 ban đầu dừng ở fixture vì Docker unavailable. W-0161 sau đó đã
khởi động Docker/Testcontainers và chạy assertion thật; xem
[evidence W-0161](../W-0161/README.md). PASS này chỉ thuộc local disposable PostgreSQL.

## 6. External artifact còn thiếu

- Module 3 assembler commit + producer CDC cho matrix/wire mapping.
- Generic callback consumer cho cả hai program, ACK/idempotency/revalidation contract.
- Security/Platform auth profile, reachable sandbox, credential/network evidence.
- Product/Order Core production attempt-policy table/version + producer CDC.
- Shared E2E và external signatures.

Do đó `G-CONTRACT` và `G-POLICY` vẫn `BLOCKED_EXTERNAL`; Target V1 vẫn `DRAFT`,
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Handoff

M8 đã ký phần receiver/result semantics và stop rule. Bên giao task không được gọi M8-05 là
“signed off toàn hệ thống” cho tới khi các ô M3/Product/Security có artifact thật. “OK” bằng văn
xuôi, mock test phía IVR hoặc một bảng mapping không có commit/CDC đều bị từ chối.

**Người ký:** **Tôi — Module 8 / Project Owner** · **03/09/2026**.

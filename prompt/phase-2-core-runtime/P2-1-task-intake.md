# PROMPT P2-1 — Task Intake

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-1` · **Phase** 2 — Core Runtime (mock SIM) |
| **Prereq** | `P1-1`, `P1-2`, `P1-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · PostgreSQL |

## 1. ROLE
Bạn là **Senior .NET Backend Engineer**. Bạn hiện thực endpoint nhận task từ Order Core: xác thực chặt, idempotent, chỉ chấp nhận order đủ điều kiện, tạo CallJob hoặc trả decision/reject — tất cả fail-closed, có audit/evidence. Bạn tuyệt đối không transition order.

## 2. CONTEXT
Order Core PUSH `IvrConfirmationTaskV1` (D-03) tới `POST /v1/ivr/order-confirmation/tasks`. Đây là **cổng vào** của IVR: nếu để lọt order sai (không phải Official Order, non-COD, non-CONFIRMING, blocker, contact xấu) thì toàn bộ downstream sai. Slice này validate + persist snapshot + phát intake decision; **chưa dispatch** (dispatch ở P2-3).

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/01-task-intake.md` (FR-IVR-INTAKE-001..009, taxonomy)
- `specs/api/02-internal-api.md` §2, `specs/api/05-order-core-contracts.md` §1, `specs/api/06-error-codes.md` §1b/§1c
- `specs/data/02-mapping-sales-platform.md`
- `plan/ivr-orther/decisions-log.md` §D-01/D-02/D-03/D-10 · §DS-01 · §DO-02 · §DC-01 · §DF-06

## 4. DECISIONS & CONSTRAINTS
- **DF-06:** allowlist — chỉ `X-Source-System=order-core`+token (middleware P0-3) gọi được; caller lạ → `403 IVR_FORBIDDEN_CALLER`.
- **DS-01:** chấp nhận **chỉ** `order_status=CONFIRMING` && `payment_method_snapshot=COD`; khác → `TASK_REJECTED_STATE_NOT_CALLABLE` (`IVR_STATE_NOT_CALLABLE`, 4xx). IVR assert lại dù Core đã derive.
- **D-10:** `program_code∈{GH,24-7}` + `max_attempts=2` + window/spacing khớp; sai → `TASK_REJECTED_POLICY_MISMATCH` (`IVR_POLICY_MISMATCH`, 409).
- **D-02:** không suy diễn/ghi order state; chỉ lưu snapshot opaque.
- **DO-02/DC-01:** snapshot `sellable_status[]` per-line + `call_restriction` (do-not-call từ CRM). Nếu snapshot có `NOT_SELLABLE/BLOCKED/recall/sale_lock` hoặc `call_restriction=true` → `TASK_BLOCKED_OPERATIONAL`.
- **D-05:** reject nếu payload chứa field cấm (ForbiddenFieldGuard P1-3) → `IVR_MALFORMED_REQUEST`.
- **api/06 §1b:** `ACCEPTED*/SKIPPED/HELD` → **200 + decision**; `REJECTED*/BLOCKED` → **4xx + envelope**.

## 5. INPUTS / DEPENDENCIES
- Foundation P0-3 (allowlist, idempotency, correlation, audit, evidence, error envelope).
- Domain P1-3 (`TaskSnapshot.FromDto`, `EligibilityRules`, `AttemptPolicy`, invariants guard).
- DB P1-2 (`ivr_confirmation_tasks`, `ivr_call_jobs`).
- `IVR_ADAPTER_MODE=MOCK` → decision `TASK_ACCEPTED_DRY_RUN_ONLY` khi không có SIM thật.

## 6. BUILD STEPS
1. Hiện thực handler cho stub P1-1: allowlist → idempotency (`ExecuteIdempotent`) → `TaskSnapshot.FromDto` (privacy guard) → validate.
2. **Validate tuần tự** (fail-closed, mã lỗi §1c): official order; CONFIRMING+COD (DS-01); nếu Core gửi optional derived `is_ivr_callable=false` thì reject/mismatch, nhưng không require field này trong current schema; program/attempt/window (D-10); official contact/phone_validation; blocker snapshot clean (DO-02) + `call_restriction=false` (DC-01); script approved; evidence/privacy version; invariants (functional/01 §9).
3. **Quyết định** theo taxonomy (functional/01 §Intake): accept → tạo `ivr_call_jobs` (chưa dispatch); skip/hold/reject/blocked → không tạo job; map response 200-decision vs 4xx-envelope (§1b).
4. Persist `ivr_confirmation_tasks` + audit intake + evidence (link task).
5. Idempotency: same key+payload → decision cũ; same key+khác payload → `409 IVR_IDEMPOTENCY_CONFLICT`.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Api/Endpoints/TaskIntakeEndpoint.cs` | Handler |
| `src/Ivr.Api/Application/IntakeService.cs` | Orchestrate validate→decision |
| `src/Ivr.Infrastructure/Repositories/TaskRepository.cs`, `CallJobRepository.cs` | Persist |

**Chuẩn:** mọi nhánh reject có mã §1c + audit; không literal string lỗi.

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-01` | integration | TASK-0001 (GH sellable) → accept, tạo CallJob. |
| `IT-05` | integration | draft/non-official → `TASK_REJECTED_NOT_OFFICIAL_ORDER`. |
| `IT-05a/05b` | integration | non-CONFIRMING / non-COD → `STATE_NOT_CALLABLE` (DS-01). |
| `IT-03/04` | integration | recall snapshot / call_restriction → `TASK_BLOCKED_OPERATIONAL`. |
| `CT-TASK-03` | contract | `max_attempts≠2` → `409 IVR_POLICY_MISMATCH`. |
| `UT-INTAKE-IDEMP` | unit | same key/payload → cũ; khác → 409. |

Trace: `specs/testing/03` (IT-01..05b), `testing/04` (CT-TASK), smoke `M8-P0-001`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] reject quote/cart/draft; [ ] DS-01 COD-only enforced; [ ] không ghi order state; [ ] mọi reject có mã §1c + audit.
**Reviewer:** allowlist không bypass; idempotency semantics; snapshot blocker đọc đúng per-line (DO-02).

## 10. EVIDENCE EXPECTED
Log accept official + reject quote/draft; sample non-COD reject; blocked-operational sample; idempotency 409; intake audit records.

## 11. FORBIDDEN
- ❌ Ghi/transition order state (D-02). ❌ Dispatch cuộc gọi (để P2-3). ❌ Nhận field PII cấm (D-05). ❌ Chấp nhận non-COD/non-CONFIRMING (DS-01).

## 12. DEFINITION OF DONE
- [ ] Handler + validate + decision + persist; test §8 xanh (CI); evidence §10 đủ; không vi phạm Forbidden.

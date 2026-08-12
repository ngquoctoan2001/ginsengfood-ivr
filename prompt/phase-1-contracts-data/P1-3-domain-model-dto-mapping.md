# PROMPT P1-3 — Domain Model & DTO Mapping

## 0. Meta
| | |
| --- | --- |
| **ID** | `P1-3` |
| **Phase** | 1 — Contracts & Data |
| **Prereq (blockedBy)** | `P1-1`, `P1-2` |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · Domain-centric |

## 1. ROLE
Bạn là **Senior Domain Engineer (DDD-lite)**. Bạn dựng lớp domain thuần (không phụ thuộc EF/HTTP) gói policy nghiệp vụ (attempt policy D-10, result taxonomy, eligibility rule), và mapping an toàn giữa DTO ↔ domain ↔ entity với **guard privacy** chặn field cấm. Domain của bạn là nơi "luật" sống, test được độc lập.

## 2. CONTEXT
Sau khi có DTO (P1-1) và entity/DB (P1-2), cần lớp domain ở giữa để business logic Phase 2 không rải rác trong controller/EF. Prompt này định nghĩa value object/policy/enum domain + mapping DTO↔domain↔entity + privacy-safe snapshot guard (chỉ nhận field privacy-safe từ task).

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/01-task-intake.md` (invariants), `specs/functional/03-scheduler-attempt-policy.md` (D-10), `specs/functional/05-result-normalization-callback.md` (taxonomy)
- `specs/data/02-mapping-sales-platform.md`, `specs/data/05-pii-policy.md`
- `specs/workflows/09-state-machines.md` (IVR internal states vs order state đục)
- `plan/ivr-orther/decisions-log.md` §D-10, §D-02 (order state đục), §D-05 (PII), §DS-01 (CONFIRMING+COD), §DT-02 (disposition mapping)

## 4. DECISIONS & CONSTRAINTS
- **D-10:** `AttemptPolicy` value object: từ `ProgramCode` → `maxAttempts=2`, `window`, `spacing`, `schedule[T0, T0+spacing]`. Không hardcode số rải rác — 1 nguồn.
- **D-02:** domain KHÔNG có transition order; `OrderStateSnapshot` là **opaque** (chỉ giữ giá trị + `is_ivr_callable` do task cấp). IVR chỉ có state máy nội bộ (call/attempt/result), không order.
- **DS-01:** eligibility rule domain: callable ⟺ snapshot `order_status=CONFIRMING` && `payment_method=COD` (dù Core đã derive, IVR vẫn assert defensively).
- **DT-02:** `DispositionMapper` value object map SIM disposition → result taxonomy (counted/final/technical) — dùng ở P2-5, định nghĩa policy ở đây.
- **D-05:** `TaskSnapshot` chỉ chứa field privacy-safe; guard reject nếu DTO chứa field cấm (full address, payment detail, raw phone).

## 5. INPUTS / DEPENDENCIES
- `Ivr.Contracts` DTO (P1-1), entity (P1-2).
- Mapping lib: mapping thủ công hoặc Mapster/AutoMapper (default **thủ công/Mapster** để giữ explicit — `NEED_CONFIRMATION`).

## 6. BUILD STEPS
1. **Value objects / policy** trong `Ivr.Domain`:
   - `AttemptPolicy` (D-10) — factory theo program; expose schedule + expiry; property test được.
   - `ResultTaxonomy` + `DispositionMapper` (DT-02): disposition → `{ResultType, IsCountedCustomerAttempt, IsFinalForIvr, IsTechnical}`. Technical/invalid-phone/capacity KHÔNG counted.
   - `EligibilityRules` (DS-01 + D-12 mock): callable check (CONFIRMING+COD), contact valid, blocker snapshot clean, window not expired; trust-skip **disabled by default** (DC-06) → luôn require IVR.
   - `OrderStateSnapshot` (opaque VO — D-02).
2. **TaskSnapshot** domain model + **privacy guard**: `TaskSnapshot.FromDto(dto)` validate & reject field cấm (D-05); ném `ForbiddenFieldException` → map `IVR_MALFORMED_REQUEST`.
3. **Mapping** 2 chiều: DTO ↔ domain ↔ entity (`Ivr.Contracts` ↔ `Ivr.Domain` ↔ `Ivr.Infrastructure.Entities`). Explicit, test round-trip.
4. **Domain enums**: intake decision, result type, call/attempt status — 1 định nghĩa, không lặp string.
5. Định nghĩa **invariants assertion** (functional/01 §9): `not_for_quote_cart_draft`, `no_direct_order_update`, `call_purpose=ORDER_CONFIRMATION_ONLY`, `input_signal_only` — hàm guard tái dùng ở intake P2-1.
6. Không phụ thuộc EF/HTTP trong `Ivr.Domain` (test kiến trúc enforce — nối P0-1 UT-BOOT-03).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Domain/Policies/AttemptPolicy.cs`, `DispositionMapper.cs`, `EligibilityRules.cs` | Policy VO |
| `src/Ivr.Domain/Model/TaskSnapshot.cs`, `OrderStateSnapshot.cs`, enums | Domain model |
| `src/Ivr.Domain/Privacy/ForbiddenFieldGuard.cs` | Guard field cấm |
| `src/Ivr.Infrastructure/Mapping/**` | DTO↔domain↔entity mapping |

**Chuẩn output:** domain thuần (no EF/HTTP ref); policy có 1 nguồn số (D-10); mọi enum type-safe.

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-POL-D10-01` | unit/property | `AttemptPolicy(GH)` → window 300, spacing 150, schedule `[T0,T0+150]`, maxAttempts 2; `TWENTY_FOUR_SEVEN` → 900/450. |
| `UT-DISP-02` | unit | busy/rejected → NO_ANSWER counted; sim/audio error → TECHNICAL not-counted; unreachable → INVALID_PHONE_FINAL not-counted (DT-02). |
| `UT-ELIG-03` | unit | non-COD hoặc non-CONFIRMING → not callable (DS-01); trust-skip disabled → require IVR (DC-06). |
| `UT-PII-04` | unit | DTO chứa full address/payment/raw phone → `ForbiddenFieldException` (D-05). |
| `UT-MAP-05` | unit | round-trip DTO↔domain↔entity không mất/biến field; enum map đúng. |
| `UT-ARCH-06` | unit | `Ivr.Domain` không ref EF/HTTP (NetArchTest). |

Trace: `specs/testing/02-unit-test-plan.md` (UT-NORM, UT-POL), `specs/testing/07` (PII).

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] 6 test §8 xanh; policy 1-nguồn (không magic number).
- [ ] Domain thuần; order state opaque (không transition).
- [ ] Guard PII reject đúng field cấm.

**Reviewer:** taxonomy khớp `functional/05` + DT-02; eligibility khớp DS-01/DC-06; mapping explicit không nuốt field.

## 10. EVIDENCE EXPECTED
Test report 6 pass (gồm property test D-10), sample `ForbiddenFieldException`, arch-test proof domain thuần.

## 11. FORBIDDEN
- ❌ Transition/ghi order state trong domain (D-02).
- ❌ Hardcode số attempt/window rải rác (chỉ trong `AttemptPolicy`).
- ❌ Domain ref EF/HTTP.
- ❌ Nhận & giữ field PII cấm (D-05).

## 12. DEFINITION OF DONE
- [ ] Domain model + policy + mapping + PII guard; 6 test §8 xanh trong CI.
- [ ] Arch-test domain thuần pass; evidence §10 đủ.

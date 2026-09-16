# PROMPT P4-6 — Opt-Out Feedback Loop

## 0. Meta
| | |
| --- | --- |
| **ID** | `P4-6` · **Phase** 4 — Real Integration |
| **Work ID** | `W-0034` (canonical tracker §5) |
| **Current correction** | `W-0148` · `2026-09-03` |
| **Prereq** | `P4-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · event/review |

> **CORRECTION W-0148 — AUTHORITY HIỆN HÀNH:** W-0034 chỉ tạo/test một pure threshold policy và
> một queue-only proposer bằng direct constructor. Current runtime **không gọi** hai component này;
> không có signal aggregation, proposal sender, CRM ACK, reversal hoặc next-task E2E. `Rejected`
> chỉ là `NO_ANSWER + review`, không phải explicit consent signal. Mọi nội dung bên dưới phải đọc
> theo correction này và [M8-08 decision pack](../../plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md).

## 1. ROLE
Bạn là **Senior Integration Engineer**. Trước khi viết code, bạn phải buộc Product, CRM/M3 và
Legal/Privacy ký contract về explicit customer intent, subject key, lifecycle, ACK, reversal và
retention. Sau chữ ký, IVR chỉ capture đúng explicit signal và gửi proposal/event về CRM; IVR không
tự chặn vĩnh viễn và không tự suy consent từ provider disposition.

## 2. CONTEXT
DT-02 quy định `rejected → NO_ANSWER (counted) + flag review`. Cờ này phục vụ vận hành; nó không
chứng minh khách yêu cầu opt-out. DTMF `0` là customer-cancelled order, cũng không phải opt-out.
do-not-call thuộc **CRM Customer Identity** (DC-01/DO-CORR-2); M3 phải đọc registry và gộp vào
`call_restriction` trước khi phát task.

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/02-eligibility-and-blockers.md`, `specs/functional/05-result-normalization-callback.md`
- `plan/ivr-orther/decisions-log.md` §DT-02 (rejected→review flag) · §DC-01/DC-02 · §DO-CORR-2 · §D-14

## 4. DECISIONS & CONSTRAINTS
- **DT-02:** `rejected` = NO_ANSWER counted + **review flag** (không phải cancel, không phải opt-out).
- **DO-CORR-2/DC-01:** do-not-call registry ở **CRM** — IVR **không** tự lưu chặn vĩnh viễn; chỉ **đề xuất** (propose) qua CRM API/event.
- **DC-02:** channel `PHONE_CALL` cụ thể.
- **Explicit-only V1:** chỉ explicit customer-intent action đã được Product + Legal duyệt mới được propose.
- **Threshold inference:** current floor/default `2/3` chưa có authority; không wire. Nếu cần, mở
  contract V2 riêng gồm counting window, key, dedupe, threshold, false-positive remedy và reversal.
- **D-14:** IVR audit-only nội bộ; CRM là chủ quyết định suppression.

## 5. INPUTS / DEPENDENCIES
- Product/Legal-approved explicit input/script; CRM writer/read API hoặc event; M3 pre-task read;
  Security/Platform auth/network; dedicated proposal lifecycle/admin workflow.

## 6. BUILD STEPS
0. **Contract first:** khóa explicit signal, UX/script, key/scope/category, idempotency, lifecycle,
   ACK/retry/reversal, retention/DSAR và owner bằng chữ ký.
1. **Capture explicit signal:** tách consent/suppression event khỏi order result; không dùng
   `Rejected`, busy, timeout, unreachable hoặc DTMF `0` hiện tại.
2. **Persist/outbox:** immutable event/proposal theo signed key; không raw phone trong payload/log.
3. **Propose to CRM:** sender có auth/idempotency/bounded retry/DLQ/reconciliation; CRM ACK quyết
   định state, IVR không tự suppress.
4. **Admin lifecycle:** action riêng cho proposal nếu contract cần; không giả định generic
   `OPEN/RESOLVED` review hiện tại xử lý được `PENDING_CRM`.
5. **Read-back:** M3 query registry trước task, set `call_restriction`, fail closed khi registry lỗi.
6. **Shared E2E:** explicit signal → CRM marker → M3 blocked task → IVR không dispatch; phủ reversal.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| Signed contract + OpenAPI/event schema | Explicit signal, registry lifecycle, ACK/reversal, privacy |
| IVR outbox/transport | Chỉ sau signature; immutable/idempotent/fail-safe |
| M3 producer/read proof | Registry read → `call_restriction` trước task |
| Shared E2E report | Exact SHA repo IVR + M3/CRM; không dùng direct constructor test thay thế |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-OPTOUT-NEGATIVE-01` | unit | rejected/busy/timeout/unreachable/DTMF 0 không tạo opt-out. |
| `UT-OPTOUT-EXPLICIT-02` | unit | đúng signed explicit signal tạo một immutable proposal/event. |
| `IT-OPTOUT-CRM-03` | integration | send/replay/conflict/ACK/reject/retry/DLQ; IVR không suppress cứng. |
| `IT-OPTOUT-REVERSAL-04` | integration | reversal/expiry đi đúng lifecycle và audit. |
| `E2E-OPTOUT-BLOCK-05` | shared E2E | CRM marker → M3 `call_restriction=true` → IVR không dispatch. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] không suy provider disposition thành consent; [ ] không sửa result taxonomy;
[ ] IVR không giữ registry; [ ] proposal lifecycle/ACK/reversal có contract; [ ] privacy/retention đã ký.

**Reviewer:** Product + CRM/M3 + Legal/Privacy + Security/Platform; chữ ký M8 không thay họ.

## 10. EVIDENCE EXPECTED
Signed decision, OpenAPI/event schema, producer/consumer CDC, persistence/audit samples, CRM ACK và
reversal evidence, M3 call-block proof, exact-SHA shared E2E report.

## 11. FORBIDDEN
- ❌ IVR tự lưu registry. ❌ Coi `Rejected`/DTMF `0` là opt-out. ❌ Thêm `IVR_OPT_OUT` vào result
  enum. ❌ Wire threshold `2/3` chưa ký. ❌ Dùng raw phone/contact reference chưa contract làm key.
  ❌ Gọi local queue row là CRM ACK/suppression. ❌ Propose không audit/reversal/retention.

## 12. DEFINITION OF DONE
- [x] W-0148 current-truth audit, owner boundary, explicit-only proposal và stop rule đã ký phía M8.
- [ ] Product/CRM/M3/Legal/Security/Platform contract và artifact đã nhận.
- [ ] Code/outbox/CRM lifecycle/M3 read-back đã implement sau signature.
- [ ] Test §8 và shared E2E exact SHA pass.

Cho tới khi ba dòng cuối hoàn tất: **`CURRENT_LOOP_NOT_WIRED / CODE_NOT_AUTHORIZED /
REAL_CUSTOMER_CALL_ALLOWED=NO`**.

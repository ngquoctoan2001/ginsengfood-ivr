# PROMPT P4-3 — CRM Eligibility & Post-Decision Events

## 0. Meta
| | |
| --- | --- |
| **ID** | `P4-3` · **Phase** 4 — Real Integration |
| **Prereq** | `P2-2` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · HTTP |

## 1. ROLE
Bạn là **Senior Integration Engineer**. Bạn nối IVR/Order Core với **CRM/Customer Identity** thật: đọc do-not-call qua `crm-ads-eligibility`, chuẩn bị consume event sau Core decision (DC-05), và giữ chỗ trust resolver (DC-06, chưa có → require-IVR). Fail-closed do-not-call.

## 2. CONTEXT
do-not-call/opt-out thuộc **business-platform Customer Identity** (DC-01, xác nhận DO-CORR-2). IVR không tự gọi CRM cho từng số — Order Core hợp nhất `call_restriction` vào task (như sellable). Ngoài ra, sau khi Core quyết định (confirm/cancel/expire), CRM cần event để notify — hiện **chưa implement** (DC-05). Trust-skip chưa có resolver (DC-06).

## 3. SOURCE SPECS (đọc trước)
- `integration-requirements/01-sales-platform-requirements.md` (IR-CRM-01), `specs/functional/02-eligibility-and-blockers.md`, `specs/functional/08-evidence-audit-privacy.md`
- `plan/ivr-orther/decisions-log.md` §DC-01..06 · §DO-CORR-2 · §D-12/D-14

## 4. DECISIONS & CONSTRAINTS
- **DC-01:** do-not-call = `crm-ads-eligibility` (`channelType=PHONE_CALL`, `category=TRANSACTIONAL`). Response hiện chỉ `eligible/denyReason/suppressionMarkerId` → dùng `eligible` để block; IR-CRM-01 mở rộng (`do_not_call/opt_out_scope/reason/effective_at`) — feature-flag.
- **DC-02:** đọc `PHONE_CALL` riêng (SMS opt-out không chặn voice).
- **DC-03:** IVR = transactional → KHÔNG áp marketing quiet-hours/frequency; gọi eligibility trực tiếp `category=TRANSACTIONAL`.
- **DC-05 (GAP):** event sau Core decision (`ORDER_CONFIRMED/CANCELLED/EXPIRED`) **chưa publish** → consumer viết sẵn (idempotent, dedupe) nhưng **no-op** tới khi Core/CRM build; IVR **không** tự gửi CRM (D-14).
- **DC-06 (GAP):** `CustomerTrustResolver` chưa có → trust-skip disabled, default require-IVR.
- **Fail-closed:** eligibility lỗi/timeout → block (không gọi).

## 5. INPUTS / DEPENDENCIES
- CRM base URL + service cred; flags `feature.richDoNotCall` (IR-CRM-01), `feature.trustResolver` (DC-06) — default off.
- Event consumer infra (webhook/outbox) — reuse DF-05 pattern.

## 6. BUILD STEPS
1. **Eligibility consume**: `call_restriction` đến từ task (Core hợp nhất). Nếu kiến trúc cho IVR/Core gọi trực tiếp: client `crm-ads-eligibility` (PHONE_CALL, TRANSACTIONAL) → `eligible=false` → block. Flag `richDoNotCall` bật khi IR-CRM-01 xong.
2. **Post-decision event consumer**: endpoint/subscriber nhận `ORDER_CONFIRMED/CANCELLED/EXPIRED` (idempotent, dedupe) → hiện **no-op/log** (DC-05 chưa publish); ghi rõ TODO. IVR không sinh event (D-14).
3. **Trust resolver placeholder**: interface `ICustomerTrustResolver` trả `unavailable` mặc định (DC-06) → eligibility require-IVR; flag `trustResolver` bật khi có.
4. Fail-closed eligibility; evidence link do-not-call decision.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Infrastructure/Crm/EligibilityClient.cs` | do-not-call (PHONE_CALL/TRANSACTIONAL) |
| `src/Ivr.Api/Events/PostDecisionEventConsumer.cs` | Consumer (no-op tới DC-05) |
| `src/Ivr.Infrastructure/Crm/ICustomerTrustResolver.cs` + default | Trust placeholder (DC-06) |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-CRM-DNC-01` | integration | `eligible=false` (PHONE_CALL) → block; SMS opt-out only → không chặn (DC-02). |
| `IT-CRM-FAILCLOSED-02` | integration | eligibility timeout → block (fail-closed). |
| `IT-CRM-EVT-03` | integration | event consumer idempotent/dedupe; hiện no-op (DC-05) — không crash. |
| `UT-CRM-TRUST-04` | unit | trust resolver unavailable → require-IVR (DC-06). |
| `IT-CRM-FLAG-05` | integration | `richDoNotCall=on` → đọc `opt_out_scope/reason` (mock IR-CRM-01). |

Trace: `specs/testing/03` (IT-15), `integration-requirements/01`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] do-not-call fail-closed; [ ] SMS≠voice; [ ] IVR không sinh CRM event (D-14); [ ] trust require-IVR.
**Reviewer:** flags off khi GAP chưa build; transactional không áp quiet-hours; evidence link.

## 10. EVIDENCE EXPECTED
DNC block sample, fail-closed demo, event consumer idempotent log (no-op), trust-unavailable→require proof, flag on behavior.

## 11. FORBIDDEN
- ❌ Gọi khi eligibility không xác thực (fail-closed). ❌ IVR tự ghi CRM/gửi event (D-14). ❌ Bật trust-skip khi chưa có resolver (DC-06). ❌ Áp marketing quiet-hours cho IVR transactional (DC-03).

## 12. DEFINITION OF DONE
- [ ] Eligibility + event consumer + trust placeholder với flags; 5 test §8 xanh; evidence §10 đủ.

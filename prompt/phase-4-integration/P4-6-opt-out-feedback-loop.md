# PROMPT P4-6 — Opt-Out Feedback Loop

## 0. Meta
| | |
| --- | --- |
| **ID** | `P4-6` · **Phase** 4 — Real Integration |
| **Prereq** | `P4-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · event/review |

## 1. ROLE
Bạn là **Senior Integration Engineer**. Bạn khép vòng suppression: khi khách **từ chối cuộc gọi** (rejected disposition) hoặc phát tín hiệu không muốn nhận, IVR ghi nhận → đưa vào review → **đề xuất do-not-call về CRM** (nguồn suppression thật). Bạn không tự chặn vĩnh viễn ở IVR mà trả tín hiệu về đúng chủ sở hữu (CRM — DO-CORR-2).

## 2. CONTEXT
DT-02 quy định `rejected → NO_ANSWER (counted) + flag review` — vì "rejected" có thể là tín hiệu opt-out. Hiện tín hiệu này chỉ được flag, chưa khép vòng. do-not-call thuộc **CRM Customer Identity** (DC-01/DO-CORR-2), nên IVR phải **đề xuất/đẩy** về CRM, không tự quản registry.

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/02-eligibility-and-blockers.md`, `specs/functional/05-result-normalization-callback.md`
- `plan/ivr-orther/decisions-log.md` §DT-02 (rejected→review flag) · §DC-01/DC-02 · §DO-CORR-2 · §D-14

## 4. DECISIONS & CONSTRAINTS
- **DT-02:** `rejected` = NO_ANSWER counted + **review flag** (không phải cancel). Nhiều lần rejected → tín hiệu opt-out mạnh hơn.
- **DO-CORR-2/DC-01:** do-not-call registry ở **CRM** — IVR **không** tự lưu chặn vĩnh viễn; chỉ **đề xuất** (propose) qua CRM API/event.
- **DC-02:** channel `PHONE_CALL` cụ thể.
- **Review trước khi propose:** không auto-suppress từ 1 tín hiệu; qua admin review (RBAC) hoặc rule ngưỡng có audit.
- **D-14:** IVR audit-only nội bộ; CRM là chủ quyết định suppression.

## 5. INPUTS / DEPENDENCIES
- Result rejected/review-flag (P2-5); CRM suppression propose API (IR-CRM-01 extend — flag); admin review (P3-2).

## 6. BUILD STEPS
1. **Capture**: khi result có review-flag (rejected/opt-out signal) → tạo `ivr_review_items{order_ref, contact_ref, reason, count, status}`.
2. **Rule/threshold**: đề xuất do-not-call khi đạt ngưỡng (VD rejected ≥ N) hoặc admin xác nhận — có audit; không auto từ 1 lần.
3. **Propose to CRM**: gọi CRM suppression propose (PHONE_CALL) — flag `richDoNotCall`/IR-CRM-01; hiện CRM chưa nhận propose → no-op/log + queue.
4. **Admin review UI hook** (P3-2): xem review item, xác nhận/loại; hành động audit.
5. Fail-safe: propose lỗi → giữ ở queue, không chặn nhầm; IVR không tự suppress cứng.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| migration `ivr_review_items` | Hàng đợi review |
| `src/Ivr.Api/Application/OptOutFeedbackService.cs` | Capture + rule + propose |
| `src/Ivr.Infrastructure/Crm/SuppressionProposer.cs` | Đẩy về CRM (flag) |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-OPTOUT-CAP-01` | unit | rejected/review-flag → tạo review item, tăng count. |
| `UT-OPTOUT-THRESH-02` | unit | dưới ngưỡng → không propose; đạt ngưỡng/admin xác nhận → propose (audit). |
| `IT-OPTOUT-PROPOSE-03` | integration | propose CRM (mock/flag); CRM chưa nhận → queue no-op, không suppress cứng ở IVR. |
| `UT-OPTOUT-CHANNEL-04` | unit | propose đúng channel PHONE_CALL (DC-02). |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] không auto-suppress từ 1 tín hiệu; [ ] IVR không tự giữ registry; [ ] propose có audit; [ ] channel đúng.
**Reviewer:** khớp DO-CORR-2 (CRM là chủ); fail-safe không chặn nhầm; review hook UI.

## 10. EVIDENCE EXPECTED
Review item samples, threshold behavior, propose-to-CRM log (flag), audit của admin xác nhận.

## 11. FORBIDDEN
- ❌ IVR tự lưu do-not-call registry cứng (thuộc CRM — DO-CORR-2). ❌ Auto-suppress từ 1 tín hiệu. ❌ Coi rejected = cancel (DT-02). ❌ Propose không audit.

## 12. DEFINITION OF DONE
- [ ] Capture + review + propose (flag) + fail-safe; 4 test §8 xanh; evidence §10 đủ.

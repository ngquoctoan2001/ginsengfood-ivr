# PROMPT P2-2 — Eligibility & Blockers

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-2` · **Phase** 2 — Core Runtime (mock SIM) |
| **Prereq** | `P2-1` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 |

## 1. ROLE
Bạn là **Senior .NET Backend Engineer** phụ trách cổng eligibility. Bạn quyết định một task **có được phép tạo cuộc gọi hay không** dựa trên blocker (sellable/recall/sale-lock), do-not-call (CRM), trust, contact, window, capacity — luôn **fail-safe** (nghi ngờ → không gọi).

## 2. CONTEXT
Sau intake (P2-1), trước khi scheduler dispatch (P2-3), phải đánh giá eligibility trên **snapshot** trong task (pre-dispatch) — chân lý cuối cùng là revalidate lúc callback (P2-6). Slice này gói toàn bộ rule "được gọi/không" và lý do, phát ra `TASK_BLOCKED_OPERATIONAL`/`SKIPPED`/eligible.

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/02-eligibility-and-blockers.md`
- `specs/data/03-mapping-ops-core.md`, `specs/data/01-data-ownership.md`
- `plan/ivr-orther/decisions-log.md` §DO-01/02/03 + §DO-CORR-1/2/3 · §DC-01/02/03/06 · §D-12 · §DS-01

## 4. DECISIONS & CONSTRAINTS
- **DO-CORR-1:** ops không biết `order_id` — snapshot đã là **per-line SellableStatus** (Order Core fan-out). IVR chỉ **đọc** snapshot, không gọi ops (ops thật ở P4-2 do Order Core owns).
- **Blocker (DO-01/02):** bất kỳ line `Decision∈{NOT_SELLABLE,BLOCKED,UNKNOWN}` hoặc `RecallHold/SaleLock/QualityHold=true` → block.
- **DC-01/DO-CORR-2:** do-not-call = `call_restriction` từ CRM (không phải ops); `true` → block. Fail-closed nếu thiếu (DC-01).
- **DC-02:** opt-out SMS KHÔNG chặn voice — chỉ đọc `PHONE_CALL` restriction.
- **DC-06/D-12:** trust-skip **disabled** (chưa có resolver) → **luôn require IVR** (không skip). Giữ code path skip nhưng flag off + `trusted_skip_allowed` mặc định false.
- **Contact/window:** phone_validation_status hợp lệ; task chưa `expires_at`.
- **Capacity:** nếu vượt capacity → `IVR_CAPACITY_EXCEPTION`/hoãn (không counted như no-answer).

## 5. INPUTS / DEPENDENCIES
- `TaskSnapshot` (P1-3) với `sellable_status[]`, `call_restriction`, `trust`, `contact`, `expires_at`.
- `EligibilityRules` domain (P1-3) — mở rộng ở đây nếu cần.

## 6. BUILD STEPS
1. `EligibilityService.Evaluate(task)` trả `{Eligible, Decision, Reasons[]}`.
2. Thứ tự đánh giá (fail-closed, dừng ở block đầu tiên có evidence): official/state (đã ở intake, re-assert) → blocker sellable per-line → `call_restriction` (do-not-call) → contact valid → window not expired → capacity → trust-skip (disabled) → eligible.
3. Mọi block ghi **reason code** + evidence link (blocker→signal). Skip-trusted: chỉ khi `trusted_skip_allowed && TRUSTED && no risk` — hiện luôn false (DC-06) → path này không kích hoạt, có test chứng minh.
4. Kết quả eligible → đánh dấu task sẵn sàng cho scheduler; block/skip → cập nhật trạng thái + không dispatch.
5. Fail-closed: thiếu/không đọc được bất kỳ nguồn bắt buộc (call_restriction, sellable) → block (không gọi).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Api/Application/EligibilityService.cs` | Đánh giá eligibility |
| `src/Ivr.Domain/Policies/EligibilityRules.cs` (mở rộng) | Rule thuần |
| `src/Ivr.Infrastructure/Repositories/...` | Cập nhật trạng thái task |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-ELIG-BLOCK-01` | unit | 1 line NOT_SELLABLE → block; tất cả SELLABLE → eligible. |
| `UT-ELIG-DNC-02` | unit | `call_restriction=true` (PHONE_CALL) → block; SMS opt-out only → không chặn (DC-02). |
| `UT-ELIG-TRUST-03` | unit | trust-skip disabled → require IVR dù TRUSTED (DC-06). |
| `UT-ELIG-FAILCLOSED-04` | unit | thiếu sellable/call_restriction → block (fail-closed). |
| `IT-ELIG-CAP-05` | integration | vượt capacity → `IVR_CAPACITY_EXCEPTION`, không counted. |

Trace: `specs/testing/02` (UT-ELIG), `testing/03`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] fail-closed mọi nguồn thiếu; [ ] do-not-call từ CRM đúng channel; [ ] trust-skip off; [ ] mọi block có reason+evidence.
**Reviewer:** không gọi ops trực tiếp (đọc snapshot); blocker per-line đúng DO-02; capacity không tính no-answer.

## 10. EVIDENCE EXPECTED
Block samples (sellable, do-not-call, fail-closed), trust-skip-disabled proof, capacity-exception log, reason+evidence links.

## 11. FORBIDDEN
- ❌ Gọi ops-core trực tiếp (ops API là của Order Core). ❌ Bật trust-skip khi chưa có resolver (DC-06). ❌ Coi opt-out SMS = chặn voice. ❌ Gọi khi thiếu blocker data (phải fail-closed).

## 12. DEFINITION OF DONE
- [ ] EligibilityService + rule + fail-closed; test §8 xanh; evidence §10 đủ.

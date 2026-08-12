# PROMPT P2-6 — Order Core Callback

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-6` · **Phase** 2 — Core Runtime (mock SIM) |
| **Prereq** | `P2-5` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 |

## 1. ROLE
Bạn là **Senior .NET Engineer (integration)**. Bạn gửi kết quả IVR về Order Core dưới dạng **signal** (không phải lệnh), xử lý đúng thực tế Core hiện tại (200/422), retry bounded idempotent, và link evidence. Bạn tuyệt đối không coi "khách bấm 1" = order confirmed.

## 2. CONTEXT
Sau normalize (P2-5), kết quả final phải callback về Core: `POST {orderCore}/v1/orders/{order_id}/ivr-result-callbacks` (D-04). Core revalidate rồi tự transition (D-02). Ở MODE=MOCK, gọi mock Order Core (client P1-1). Đây là ranh giới quan trọng nhất: IVR gửi signal, Core quyết định.

## 3. SOURCE SPECS (đọc trước)
- `specs/api/05-order-core-contracts.md` §2 (callback + caveat DS-03/04), `specs/workflows/06-race-condition-revalidation.md`, `specs/workflows/09-state-machines.md`
- `plan/ivr-orther/decisions-log.md` §D-04 · §DS-02/DS-03/DS-04 · §D-06 (race) · §D-02

## 4. DECISIONS & CONSTRAINTS
- **D-04:** callback đủ field (evidence_ref bắt buộc trước final action); `recommended_core_action` = **advisory**; Core revalidate. `CALLBACK_ACCEPTED_FOR_REVALIDATION` ≠ confirmed.
- **DS-03 (reality):** Core trả **200 (accept) / 422 (invalid)** — **chưa** có bộ `CALLBACK_*` codes. Client phải xử lý 200/422 là chính; map `CALLBACK_*` là **target** (đánh dấu), không giả định Core phát.
- **DS-04 (reality):** callback **current không gửi/không require** `order_version_seen_by_ivr`; race guard hiện dựa Core recheck `CONFIRMING+COD` + sellable/blocker. Target OC1 mới bật `order_version_seen_by_ivr` qua feature flag khi Core expose/nhận field.
- **DS-02:** confirm→CONFIRMED, cancel→CANCELLED, expiry→EXPIRED; no-answer/technical **không** transition (order chờ expire). IVR không tự transition.
- **Retry:** chỉ khi timeout/5xx/`TECHNICAL_RETRY_ALLOWED` — bounded, **cùng idempotency key** (D-04).
- **Race (D-06):** khách bấm 1 nhưng blocker xuất hiện → Core block/hold; IVR mark blocked + evidence, không tự confirm.

## 5. INPUTS / DEPENDENCIES
- `NormalizedResult` (P2-5); Order Core client (P1-1) với Idempotency+Correlation handler (P0-3).
- DB `ivr_callbacks`; evidence store.

## 6. BUILD STEPS
1. `CallbackDispatcher` build `IvrConfirmationResultCallbackCurrentV1` từ result (gồm evidence_ref, recommended_core_action advisory, idempotency/correlation). Không gửi `order_version_seen_by_ivr` khi `feature.orderVersionRaceGuard=off`.
2. Gửi callback với `Idempotency-Key`+`X-Correlation-Id`; xử lý response: **200** → mark accepted-for-revalidation (≠confirmed); **422** → mark rejected/invalid + evidence + admin review (không retry như signal mới); timeout/5xx → **retry bounded cùng key**.
3. Map (khi Core nâng cấp — target) `CALLBACK_*` codes qua enum "target"; hiện tại chỉ nhánh 200/422 hoạt động; ghi rõ TODO IR-SALES-OC2.
4. Khi `feature.orderVersionRaceGuard=on` **và** contract OC1 đã accepted: build `IvrConfirmationResultCallbackTargetV1` với `order_version_seen_by_ivr`; nếu thiếu version snapshot thì fail-safe admin review thay vì gửi target payload giả.
5. Persist `ivr_callbacks` + link evidence (signal + blocker nếu race).
6. Không transition order dưới bất kỳ hình thức nào (D-02); "confirmed" chỉ khi Core tự xác nhận (không suy từ 200).
7. Idempotent: duplicate callback → ack cũ.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Worker/Callback/CallbackDispatcher.cs` | Build+gửi+retry |
| `src/Ivr.Infrastructure/Clients/OrderCoreClient.cs` (dùng lại P1-1) | HTTP client |
| `src/Ivr.Infrastructure/Repositories/CallbackRepository.cs` | Persist |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-06` | integration | confirm → Core 200 accepted-for-revalidation (mark ≠ confirmed). |
| `IT-CB-422-02` | integration | order rời CONFIRMING/non-COD → Core 422 → mark invalid, không retry-as-new (DS-03). |
| `IT-09` | integration | callback timeout → retry bounded **cùng idempotency key** (D-04). |
| `IT-07` | integration | race: phím 1 + blocker → Core block; IVR mark blocked + evidence, không confirm (D-06). |
| `CT-CB-CURRENT-01` | contract | current callback payload không chứa/không require `order_version_seen_by_ivr`; target variant mới require field. |
| `UT-CB-04` | unit | thiếu evidence_ref → không final-callback → hold/review. |
| `UT-CB-DUP-05` | unit | duplicate callback → ack cũ (idempotent). |

Trace: `specs/testing/03` (IT-06..09), `testing/04` (CT-CB), smoke `M8-P0-002/003`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] 200≠confirmed; [ ] 422 xử lý đúng (không retry-as-new); [ ] retry cùng key; [ ] không transition order; [ ] `CALLBACK_*` đánh dấu target.
**Reviewer:** race-guard hiện dựa Core state/COD recheck (không order_version — DS-04); evidence link signal+blocker; recommended_core_action advisory.

## 10. EVIDENCE EXPECTED
Callback 200/422 samples, retry-same-key proof, race block sample (signal+blocker evidence), idempotent duplicate ack.

## 11. FORBIDDEN
- ❌ Suy "200 = order confirmed" (D-04). ❌ Transition order (D-02). ❌ Retry callback với key mới. ❌ Giả định Core phát `CALLBACK_*` codes (DS-03).

## 12. DEFINITION OF DONE
- [ ] Callback dispatcher + 200/422 + retry bounded + evidence; 6 test §8 xanh; evidence §10 đủ. **Kết thúc Phase 2: hệ chạy end-to-end MOCK (intake→dispatch→normalize→callback).**

# PROMPT P4-1 — Order Core Real Wiring

## 0. Meta
| | |
| --- | --- |
| **ID** | `P4-1` · **Phase** 4 — Real Integration |
| **Prereq** | `P2-6` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · HTTP (mTLS/JWT) |

## 1. ROLE
Bạn là **Senior Integration Engineer**. Bạn nối IVR với Order Core **thật** (Java/Spring): nhận task push, gửi callback thật, xử lý đúng **thực tế Core hiện tại** (200/422, no order_version) và chuẩn bị bật race-guard khi Core nâng cấp (IR-SALES-OC1). Fail-closed mọi lỗi.

## 2. CONTEXT
Phase 2 chạy với mock Order Core. Phase 4 thay bằng client thật + endpoint nhận push thật. Có **khoảng cách target vs implemented** (DS-02/03/04) phải xử lý minh bạch: code chạy được với Core hôm nay, có nhánh "target" bật sau khi team Order Core build OC1/OC2/OC3.

## 3. SOURCE SPECS (đọc trước)
- `specs/api/05-order-core-contracts.md` (đặc biệt caveat DS-03/04), `integration-requirements/01-sales-platform-requirements.md` (IR-SALES-OC1/OC2/OC3), `specs/workflows/06-race-condition-revalidation.md`, `specs/workflows/09-state-machines.md`
- `plan/ivr-orther/decisions-log.md` §D-03/D-04/D-06 · §DS-01..05

## 4. DECISIONS & CONSTRAINTS
- **DS-03 (reality):** callback response = **200/422**. Client mặc định xử lý 200/422; enum `CALLBACK_*` là **feature-flag "target"** (tắt tới khi OC2).
- **DS-04 (reality):** Core chưa expose `order_version`, chưa nhận `order_version_seen_by_ivr`. Race-guard **hiện dựa**: Core re-check state(`CONFIRMING`)+COD+sellable. Bật `order_version` guard qua flag khi OC1 xong.
- **DS-02:** no-answer/technical không transition — không kỳ vọng Core hủy; theo dõi order tự expire (không poll order state để đổi hành vi IVR — D-02).
- **D-06:** revalidate-at-callback là chân lý; race → block/hold từ Core.
- Auth: service credential + allowlist (nối P4-4).

## 5. INPUTS / DEPENDENCIES
- Order Core base URL, credentials (staging thật hoặc contract-mock nếu chưa sẵn).
- Feature flags: `feature.orderVersionRaceGuard`, `feature.richCallbackCodes` (default off — DS-03/04).
- Client P1-1 + callback dispatcher P2-6.

## 6. BUILD STEPS
1. Nối `OrderCoreClient` thật (base URL, auth, timeout, retry-policy bounded); giữ Idempotency+Correlation.
2. Endpoint **nhận task push thật** (nếu Core PUSH): xác thực allowlist thật (P4-4); dùng intake pipeline P2-1.
3. Callback thật: xử lý 200/422 (DS-03); log/parse response; **feature-flag** cho `CALLBACK_*` target — off → chỉ 200/422.
4. Race-guard: hiện dùng state/COD/sellable recheck (đọc lại từ task snapshot + Core revalidate); khi `feature.orderVersionRaceGuard=on` (OC1) → gửi & kỳ vọng Core check `order_version_seen_by_ivr`.
5. Fail-closed: Core down/timeout → không tạo task mới / callback retry bounded → admin review (không mất signal).
6. Contract test với Core thật/pact; ghi rõ trạng thái target vs live.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Infrastructure/Clients/OrderCoreClient.cs` (thật) | HTTP + auth + retry |
| `src/Ivr.Api/Endpoints/TaskIntakeEndpoint.cs` (allowlist thật) | Nhận push |
| `src/Ivr.Infrastructure/FeatureFlags/**` | Flag target vs live |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-OC-200-01` | integration | confirm → Core 200 accepted-for-revalidation. |
| `IT-OC-422-02` | integration | order rời CONFIRMING/non-COD → 422 handled (DS-03), không retry-as-new. |
| `IT-OC-DOWN-03` | integration | Core down → fail-closed (không task mới; callback retry→admin review). |
| `IT-OC-FLAG-04` | integration | `orderVersionRaceGuard=off` → không dựa version; `on` → gửi+kỳ vọng check (mock OC1). |

Trace: `specs/testing/03` (IT-06..09), `integration-requirements/01`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] 200/422 đúng reality; [ ] target codes sau flag; [ ] fail-closed; [ ] không transition/poll để đổi hành vi.
**Reviewer:** flag rõ ràng (không bật target khi Core chưa có); auth thật; retry bounded cùng key.

## 10. EVIDENCE EXPECTED
Live 200/422 capture, fail-closed demo, flag on/off behavior, contract/pact report.

## 11. FORBIDDEN
- ❌ Giả định Core phát `CALLBACK_*` khi flag off (DS-03). ❌ Dùng order_version guard khi OC1 chưa có. ❌ Transition order. ❌ Bật gọi thật (vẫn MOCK adapter).

## 12. DEFINITION OF DONE
- [ ] Client+push+callback thật với flag target/live; 4 test §8 xanh; evidence §10 đủ.

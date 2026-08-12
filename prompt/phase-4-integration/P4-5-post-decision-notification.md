# PROMPT P4-5 — Post-Decision Notification Integration

## 0. Meta
| | |
| --- | --- |
| **ID** | `P4-5` · **Phase** 4 — Real Integration |
| **Prereq** | `P4-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · event/webhook |

## 1. ROLE
Bạn là **Senior Integration Engineer**. Bạn nối luồng thông báo **sau khi Order Core quyết định** (confirm/cancel/expire) tới CRM để CRM gửi notification cho khách — **IVR không tự gửi** (D-14). Bạn xây consumer idempotent, chịu được việc Core/CRM chưa publish (DC-05 GAP) bằng no-op + flag.

## 2. CONTEXT
Sau cuộc gọi IVR, Core quyết định → khách cần được thông báo (đơn đã xác nhận/hủy/hết hạn). Trách nhiệm gửi thuộc **CRM** (D-14: IVR audit-only). Hiện Core **chưa publish** event outcome cho IVR-flow (DC-05). Prompt này viết sẵn consumer + trigger notification qua CRM, bật khi DC-05 xong.

## 3. SOURCE SPECS (đọc trước)
- `specs/functional/08-evidence-audit-privacy.md`, `integration-requirements/01-sales-platform-requirements.md`
- `plan/ivr-orther/decisions-log.md` §DC-05 (GAP event) · §D-14 (IVR audit-only, CRM gửi) · §DF-05 (outbox pattern)

## 4. DECISIONS & CONSTRAINTS
- **D-14:** IVR **không** gửi SMS/notification trực tiếp; chỉ CRM gửi theo template. IVR chỉ ghi audit + (nếu cần) trigger qua event.
- **DC-05 (GAP):** event `ORDER_CONFIRMED/CANCELLED/EXPIRED` sau Core decision **chưa publish** → consumer **no-op/log** tới khi Core/CRM build; flag `feature.postDecisionNotify` default off.
- **Idempotent + dedupe** (DF-05): consumer at-least-once, dedupe EventId.
- **Không** IVR tự soạn/gửi nội dung khách; CRM giữ template.
- Fail-safe: notification lỗi không ảnh hưởng order state (đã do Core).

## 5. INPUTS / DEPENDENCIES
- Event source (Core, DC-05 khi có); CRM notification API/template; outbox/consumer infra (DF-05).
- Flag `postDecisionNotify` (P0-4).

## 6. BUILD STEPS
1. **Consumer** nhận event outcome (idempotent, dedupe EventId) — hiện no-op/log (DC-05 chưa publish); ghi TODO rõ.
2. Khi flag on + Core publish: map outcome → gọi **CRM notification trigger** (CRM chọn template/kênh); IVR không soạn nội dung.
3. Audit: ghi việc trigger + correlation (không nội dung PII).
4. Fail-safe: retry bounded; lỗi notify không rollback/không đụng order.
5. Test cả path no-op (hôm nay) và path live (mock Core publish + mock CRM).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Api/Events/PostDecisionNotifyConsumer.cs` | Consumer idempotent |
| `src/Ivr.Infrastructure/Crm/NotificationTrigger.cs` | Trigger CRM (không soạn nội dung) |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-NOTIFY-NOOP-01` | integration | flag off / Core chưa publish → consumer no-op, không crash. |
| `IT-NOTIFY-LIVE-02` | integration | flag on + event → trigger CRM notification (mock); IVR không soạn nội dung (D-14). |
| `IT-NOTIFY-IDEMP-03` | integration | duplicate event dedupe EventId → 1 trigger. |
| `IT-NOTIFY-FAILSAFE-04` | integration | notify lỗi → không đụng order state; retry bounded. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] IVR không gửi trực tiếp (D-14); [ ] no-op an toàn khi DC-05 chưa có; [ ] idempotent; [ ] fail-safe.
**Reviewer:** flag gate đúng; CRM giữ template; audit không PII.

## 10. EVIDENCE EXPECTED
No-op path log, live trigger (mock), dedupe proof, fail-safe demo.

## 11. FORBIDDEN
- ❌ IVR tự soạn/gửi notification cho khách (D-14). ❌ Giả định DC-05 đã live (flag). ❌ Notify lỗi làm hỏng order state.

## 12. DEFINITION OF DONE
- [ ] Consumer + trigger + flag + fail-safe; 4 test §8 xanh; evidence §10 đủ.

# PROMPT P4-2 — Ops-Core Sellable Gate Integration

## 0. Meta
| | |
| --- | --- |
| **ID** | `P4-2` · **Phase** 4 — Real Integration |
| **Prereq** | `P2-2` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · HTTP |

## 1. ROLE
Bạn là **Senior Integration Engineer**. Bạn hiện thực consume **sellable gate** thật của ops-core (snapshot + webhook), với ETag/captured_at độ tươi, và fail-closed tuyệt đối. Bạn nhớ: **IVR không gọi ops trực tiếp** — Order Core là caller; IVR đọc snapshot trong task + phối hợp revalidate qua Core.

## 2. CONTEXT
Blocker sellable/recall/sale-lock là điều kiện sống còn để không gọi khách sai. Ops cung cấp `POST /api/v1/admin/availability/check` + webhook `sku-became-not-sellable`. Do ops không biết `order_id` (DO-CORR-1), **Order Core fan-out** per-line và nhúng snapshot vào task; IVR đọc snapshot + Core revalidate lúc callback. Prompt này wiring phần IVR/Core-side consume thật + webhook hold sớm.

## 3. SOURCE SPECS (đọc trước)
- `specs/data/03-mapping-ops-core.md`, `integration-requirements/02-ops-core-requirements.md`, `specs/functional/02-eligibility-and-blockers.md`
- `plan/ivr-orther/decisions-log.md` §DO-01..09 + §DO-CORR-1/2/3 · §DS-01

## 4. DECISIONS & CONSTRAINTS
- **DO-CORR-1:** ops không biết order_id → Order Core fan-out; IVR **không** gọi ops trực tiếp (ops API là admin/service-auth của Core).
- **DO-02:** snapshot `SellableStatus[]` per-line + `captured_at`/ETag; snapshot pre-dispatch, chân lý = revalidate lúc callback.
- **DO-04:** webhook `ops-core.sellable.sku-became-not-sellable.v1` (dedupe `EventId`) → **hold sớm** (optional), KHÔNG thay revalidate.
- **DO-06:** fail-closed — non-2xx/timeout/`ready=503`/thiếu = block, không dispatch.
- Error codes ops: `SALE_LOCK_ACTIVE/RECALL_IMPACT_ACTIVE/SELLABLE_GATE_BLOCKED/...` → map BLOCKED.

## 5. INPUTS / DEPENDENCIES
- Nếu IVR cần đọc trực tiếp (chỉ khi kiến trúc cho phép): ops base URL + service cred `SellableCheck`. Mặc định: đọc snapshot từ task (Core fan-out) + webhook consumer.
- Webhook endpoint + dedupe store (EventId).

## 6. BUILD STEPS
1. **Snapshot consumer**: parse `sellable_status[]` + `captured_at`/ETag từ task; đánh giá độ tươi (nếu quá cũ → yêu cầu revalidate/block).
2. **Webhook consumer**: endpoint nhận `sku-became-not-sellable` (header `X-Idempotency-Key=EventId`); dedupe; đánh dấu hold sớm cho task liên quan SKU (optional; không thay revalidate).
3. **Revalidate coordination**: lúc callback (P2-6/P4-1), Core revalidate realtime — IVR đảm bảo gửi đủ context; nếu blocker → mark blocked + evidence (link SKU/batch/recall id — DO-07).
4. Fail-closed mọi nhánh; map error codes ops → BLOCKED/fail-closed.
5. (Nếu áp dụng) client ops thật với ETag/If-None-Match cho low-latency.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Infrastructure/Sellable/SnapshotEvaluator.cs` | Đọc + độ tươi |
| `src/Ivr.Api/Webhooks/SellableWebhookEndpoint.cs` | Webhook + dedupe |
| `src/Ivr.Infrastructure/Sellable/OpsClient.cs` (nếu cần) | ETag client |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-OPS-SNAP-01` | integration | snapshot 1 line NOT_SELLABLE → block; captured_at cũ → yêu cầu revalidate/block. |
| `IT-OPS-WH-02` | integration | webhook dedupe EventId; hold sớm nhưng không thay revalidate (DO-04). |
| `IT-OPS-FAILCLOSED-03` | integration | ops `ready=503`/timeout → fail-closed (không dispatch). |
| `IT-OPS-EVID-04` | integration | block ghi evidence link sku/batch/recall id (DO-07). |

Trace: `specs/testing/03` (IT-10..14), `integration-requirements/02`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] IVR không gọi ops trực tiếp trái kiến trúc; [ ] fail-closed; [ ] webhook không thay revalidate; [ ] evidence link id.
**Reviewer:** độ tươi captured_at xử lý đúng; dedupe EventId; error map BLOCKED.

## 10. EVIDENCE EXPECTED
Snapshot block sample, webhook dedupe log, fail-closed demo, evidence với sku/batch/recall id.

## 11. FORBIDDEN
- ❌ Bỏ qua blocker/độ tươi. ❌ Coi webhook = chân lý thay revalidate. ❌ Dispatch khi ops không xác thực được (fail-closed). ❌ IVR tự gọi ops nếu kiến trúc quy định Core là caller.

## 12. DEFINITION OF DONE
- [ ] Snapshot + webhook + fail-closed + evidence; 4 test §8 xanh; evidence §10 đủ.

# Workflow — Race Condition & Revalidation

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p04` · Nguồn: `phase-8/07 §8,§13`; `docx` §14.

**Tình huống:** Khách bấm `1` nhưng Sale Lock/Recall xuất hiện, hoặc `order_version` đổi, trước khi Core accept. **IVR result vẫn `IVR_CONFIRMED` (raw signal)** nhưng Core **block/hold**, KHÔNG auto-confirm (P0-IVR-003).

> ⚠️ **Thực tế Core (DS-02/03/04):** race-guard **đang hoạt động** = Core revalidate **state (`CONFIRMING`) + COD + sellable/recall/sale-lock realtime** trước khi accept; nếu không hợp lệ → **`422`** (chưa transition confirm). Nhánh **`order_version` mismatch → `CALLBACK_REJECTED_STALE`** là **target** (Core chưa expose `order_version`, chưa nhận `order_version_seen_by_ivr` — DS-04 / IR-SALES-OC1). Tới khi đó, bảo vệ chính chống stale = **state/COD/sellable recheck**, không phải version.

```mermaid
sequenceDiagram
    participant SIM
    participant Norm
    participant OrderCore
    participant OpsCore
    participant Evid
    SIM-->>Norm: answered + DTMF=1
    Norm->>Norm: normalize -> IVR_CONFIRMED (raw customer signal)
    Norm->>Evid: evidence(IVR signal)
    Norm->>OrderCore: callback current (IVR_CONFIRMED, evidence_ref)
    Note over Norm,OrderCore: Target IR-SALES-OC1 adds order_version_seen_by_ivr
    OrderCore->>OpsCore: revalidate blocker (Sale Lock / Recall / Suppression) realtime
    OpsCore-->>OrderCore: blocker ACTIVE (or version mismatch)
    OrderCore->>Evid: evidence(blocker linked to signal)
    OrderCore-->>Norm: Current 422; target CALLBACK_BLOCKED_BY_CORE / CALLBACK_REJECTED_STALE
    Note over OrderCore: KHÔNG confirm chỉ vì phím 1; block/hold admin review
```

## Race matrix (phase-8/07 §13)
| Race | Detection | Hành vi bắt buộc |
| --- | --- | --- |
| order_version đổi sau task ⏳**target** | `order_version_seen_by_ivr` mismatch | Reject stale hoặc Core re-evaluate — **deferred** (Core chưa check version, DS-04); nay dựa state/COD/sellable recheck |
| state rời `CONFIRMING` (đã CONFIRMED/CANCELLED/EXPIRED) | Core state check | `422` — không transition lại (DS-02/DS-03) |
| `payment_method` không còn COD | Core COD gate | `422` — ngoài phạm vi IVR (DS-01) |
| Phím `1`, Sale Lock xuất hiện | Core blocker check | Không confirm; block/hold |
| Phím `0`, đơn đã cancel | Order state check | Idempotent no-op / stale |
| No-answer final, payment issue | Core revalidation | Core decide hold/cancel |
| Duplicate callback | Idempotency | Trả ack cũ |
| Evidence missing | Evidence check | Reject/hold/review |

**Đã khóa:** Q-O1 → **D-06/DO-03** (Core gọi ops sellable gate realtime), Q-S1 → **D-02/DS-01** (Core owns state; IVR-callable = CONFIRMING+COD).
**Còn treo (build):** IR-SALES-OC1 (expose `order_version` để bật nhánh stale-reject) — hiện GAP (DS-04).

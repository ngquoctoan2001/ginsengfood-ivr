# Workflow — Cancel (phím 0)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p04` · Nguồn: `docx` §9,§13,§14; `phase-8/07`.

**Kết quả:** `IVR_CUSTOMER_CANCELLED` (counted, final) → Order Core hủy **qua state machine** (IVR không tự hủy).

```mermaid
sequenceDiagram
    participant OrderCore
    participant IVR
    participant Sched
    participant SIM
    participant Norm
    participant Evid
    OrderCore->>IVR: IvrConfirmationTaskV1
    IVR->>Sched: create CallJob
    Sched->>SIM: dispatch attempt 1 @ T0
    SIM-->>Sched: answered + DTMF=0
    Sched->>Norm: raw call event (dtmf=0)
    Norm->>Norm: normalize -> IVR_CUSTOMER_CANCELLED
    Norm->>Evid: evidence(result)
    Norm->>OrderCore: callback (IVR_CUSTOMER_CANCELLED, recommended=CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST)
    OrderCore->>OrderCore: revalidate + cancel via state machine (reason CUSTOMER_CANCELLED_BY_IVR_KEY_0)
    OrderCore-->>Norm: CALLBACK_ACCEPTED_FOR_REVALIDATION
    Note over OrderCore: Notification (nếu có) do owner khác gửi SAU Core decision
```

**P0:** `IVR_CAN_DIRECTLY_CANCEL_ORDER = NO`; hủy phải qua Core (P0-IVR-002). IVR/SIM không tự gửi thông báo (P0-IVR-008).

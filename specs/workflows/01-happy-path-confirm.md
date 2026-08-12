# Workflow — Happy Path (Confirm, phím 1)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p04` · Nguồn: `docx` §3,§8,§9,§14; `phase-8/07`,`/23`.

**Kết quả:** `IVR_CONFIRMED` (counted, final) → Order Core revalidate → tiếp tục xử lý đơn.

```mermaid
sequenceDiagram
    participant OrderCore
    participant IVR
    participant Sched
    participant SIM
    participant Norm
    participant Evid
    OrderCore->>IVR: IvrConfirmationTaskV1 (order, program, contact, blockers)
    IVR->>IVR: validate Official Order + idempotency + eligibility
    IVR->>Evid: audit(task intake ACCEPTED)
    IVR->>Sched: create CallJob (attempt schedule)
    Sched->>SIM: dispatch attempt 1 @ T0 (reserve 1 SIM)
    SIM->>SIM: play script(order_code_short, total_amount_display)
    SIM-->>Sched: answered + DTMF=1
    Sched->>Norm: raw call event (status, dtmf=1)
    Norm->>Norm: normalize -> IVR_CONFIRMED
    Norm->>Evid: evidence(result + dtmf)
    Norm->>OrderCore: IvrConfirmationResultCallbackCurrentV1 (IVR_CONFIRMED, evidence_ref)
    Note over Norm,OrderCore: Target IR-SALES-OC1 adds order_version_seen_by_ivr
    OrderCore->>OrderCore: revalidate (version, state, blocker, evidence)
    OrderCore-->>Norm: CALLBACK_ACCEPTED_FOR_REVALIDATION
    OrderCore->>OrderCore: transition per state machine (Core decides)
```

**Ghi chú:** attempt 2 không được tạo vì A1 đã có kết quả cuối (FR-IVR-SCH-005). Core có thể vẫn `BLOCKED_BY_CORE` nếu revalidate phát hiện blocker (xem [06](06-race-condition-revalidation.md)).

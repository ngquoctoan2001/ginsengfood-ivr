# Workflow — Capacity Hold / Incident

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p04` · Nguồn: `docx` §11,§12; `phase-8/05`,`/07 §10`.

**Tình huống:** Queue/SIM không đủ để dispatch trước expiry. Mở `capacity_incident`, **không im lặng** để đơn hết hạn. Không kéo dài window thương mại.

```mermaid
sequenceDiagram
    participant Sched
    participant Mon as CapacityMonitor
    participant Admin
    participant Norm
    participant OrderCore
    Sched->>Mon: pending/expired/missed_deadline vượt ngưỡng
    Mon->>Mon: open capacity_incident (hold_new_calls)
    Mon->>Admin: alert (RBAC)
    alt admin pause/resume (permission)
        Admin->>Sched: pause queue (reason + audit)
        Admin->>Sched: resume after incident resolved
    else attempt miss window
        Sched->>Norm: IVR_CAPACITY_EXCEPTION (not counted) / IVR_CONFIRMATION_WINDOW_EXPIRED
        Norm->>OrderCore: callback -> Core decides (review/expire)
    end
```

**P0:** `BATCH_AFTER_SESSION_CALLING = PROHIBITED`; `ROLLING_REAL_TIME_IVR = REQUIRED` (docx M8-SCH-001/002). Miss deadline không log → FAIL. Resume khi incident chưa xử lý → chặn (FR-IVR-ADM-002). `Owner Decision Required` OD-05 (SIM pool size để giảm capacity incident).

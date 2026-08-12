# Workflow — No Answer and Timeout

Trạng thái: `TARGET_V1_DRAFT`.

Scheduler uses the task's approved `attempt_policy_version` and offsets. Candidate for MOCK/LAB only: GH `[0,150]` within 300s; 24/7 `[0,450]` within 900s.

```mermaid
sequenceDiagram
    participant Sales
    participant Scheduler
    participant SIM
    participant IVR
    Scheduler->>SIM: attempt at configured offset
    SIM-->>IVR: no answer
    IVR->>IVR: IVR_NO_ANSWER_ATTEMPT or FINAL
    alt attempts remain and before expiry
      IVR->>Scheduler: schedule next configured offset
    else final/window reached
      IVR->>Sales: callback NO_ANSWER_FINAL, CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT
      Sales-->>IVR: semantic ACK
      Sales->>Sales: timeout worker later revalidates then may EXPIRE
    end
```

IVR never cancels the order and sends no SMS/notification. Technical failure does not consume a customer attempt.

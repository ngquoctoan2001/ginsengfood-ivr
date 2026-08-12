# Workflow — Happy Path Confirm (key 1)

Trạng thái: `TARGET_V1_DRAFT`.

```mermaid
sequenceDiagram
    participant Sales
    participant IVR
    participant SIM
    participant Customer
    Sales->>IVR: Target task (program/payment/version/policy/dial token/speech summary)
    IVR->>IVR: auth + idempotency + matrix + eligibility + privacy validation
    IVR->>SIM: dial token; play approved order summary
    SIM->>Customer: name, items, total, short delivery area; key 1/0
    Customer-->>SIM: DTMF 1
    SIM-->>IVR: answered + key 1
    IVR->>IVR: normalize IVR_CONFIRMED + evidence + outbox
    IVR->>Sales: POST /api/v1/internal/orders/{id}/ivr-result-callbacks
    Sales->>Sales: revalidate idempotency/version/state/program/payment/blockers
    Sales-->>IVR: 200 ACCEPTED/BLOCKED_BY_CORE/REVIEW_REQUIRED
    Note over Sales: Only Sales may transition the order
```

Final A1 prevents later customer attempts. `ACCEPTED` is callback acceptance, not an assertion by IVR that the order is confirmed.

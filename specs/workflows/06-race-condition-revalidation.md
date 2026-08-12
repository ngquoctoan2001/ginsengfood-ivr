# Workflow — Race Condition and Sales Revalidation

Trạng thái: `TARGET_V1_DRAFT`.

```mermaid
sequenceDiagram
    participant IVR
    participant Sales
    participant Ops
    IVR->>Sales: target callback + order_version_seen_by_ivr
    Sales->>Sales: idempotency + path/body + current version/state/program/payment
    Sales->>Ops: revalidate sellable/recall/sale-lock
    Ops-->>Sales: current blocker truth
    alt duplicate identical
      Sales-->>IVR: 200 DUPLICATE_ACCEPTED
    else version/state stale
      Sales-->>IVR: 409 REJECTED_STALE
    else idempotency payload conflict
      Sales-->>IVR: 409 IDEMPOTENCY_CONFLICT
    else blocker active
      Sales-->>IVR: 200 BLOCKED_BY_CORE
    else decision needs human
      Sales-->>IVR: 200 REVIEW_REQUIRED
    else valid
      Sales-->>IVR: 200 ACCEPTED
    end
```

Raw customer signal remains immutable even if Sales blocks it. Current Golden Hour adapter may only provide coarser behavior; its tests/status stay separately labelled `CURRENT_COMPAT`.

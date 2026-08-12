# Workflow — IVR State Machines

Trạng thái: `TARGET_V1_DRAFT`. Order state machine is intentionally absent; Sales owns it.

## Job/attempt

```mermaid
stateDiagram-v2
    [*] --> CREATED
    CREATED --> VALIDATING
    VALIDATING --> QUEUED: eligible
    VALIDATING --> BLOCKED: invalid/blocked
    QUEUED --> CHANNEL_RESERVED
    CHANNEL_RESERVED --> DIALING
    DIALING --> RESULT_READY: provider outcome
    DIALING --> TECHNICAL_EXCEPTION: technical error
    TECHNICAL_EXCEPTION --> QUEUED: bounded technical retry
    TECHNICAL_EXCEPTION --> ADMIN_REVIEW: exhausted
    RESULT_READY --> CALLBACK_READY
    CALLBACK_READY --> CALLBACK_DELIVERY
    CALLBACK_DELIVERY --> CLOSED: terminal ACK
    CALLBACK_DELIVERY --> CALLBACK_READY: retryable transport
    BLOCKED --> CLOSED
    ADMIN_REVIEW --> CLOSED
```

## Callback delivery states

`READY → SENDING → DELIVERED_ACCEPTED | DELIVERED_BLOCKED | DELIVERED_REVIEW | REJECTED_STALE | IDEMPOTENCY_CONFLICT | INVALID_DEAD_LETTER`; retryable transport goes `RETRY_PENDING → SENDING`, then `RETRY_EXHAUSTED` if bounded retries end.

No-answer terminal callback uses no-state-change/wait-for-timeout. Notification is not a state because V1 notification is disabled.

# ARCH-07 — Summary Diagrams

Trạng thái: `TARGET_V1_DRAFT`.

## Callback revalidation

```mermaid
flowchart TD
  CB["Target callback"] --> V{"Sales validates auth, idempotency, version, state, matrix"}
  V -->|"stale"| S["409 REJECTED_STALE"]
  V -->|"conflict"| C["409 IDEMPOTENCY_CONFLICT"]
  V -->|"valid"| B{"Sales revalidates blockers/evidence"}
  B -->|"blocked"| BL["200 BLOCKED_BY_CORE"]
  B -->|"review"| R["200 REVIEW_REQUIRED"]
  B -->|"accepted"| A["200 ACCEPTED"]
```

## Mode/provider gate

```mermaid
flowchart LR
  S["Scheduler"] --> M{"Execution mode"}
  M -->|"MOCK"| MK["Mock SIM, no real egress"]
  M -->|"LAB_REAL_SIM"| LB["1 real SIM, allowlist, kill switch"]
  M -->|"PRODUCTION_REAL"| PR["32 eSIM target plus all release gates"]
  MK --> N["Normalizer"]
  LB --> N
  PR --> N
```

Attempt offsets come from `attempt_policy_version`; diagrams intentionally do not hard-code candidate timings.

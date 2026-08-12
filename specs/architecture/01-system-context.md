# ARCH-01 — System Context

Trạng thái: `TARGET_V1_DRAFT`.

```mermaid
flowchart LR
  SALES["Sales Platform Java<br/>order and eligibility truth"] -->|"Target V1 task"| IVR["IVR .NET API/Worker"]
  IVR -->|"Target result signal"| SALES
  SALES --> OPS["Ops sellable/recall/lock"]
  SALES --> CRM["Identity/call restriction"]
  IVR --> SIM["Telephony port<br/>mock / 1 SIM lab / 32 eSIM"]
  SIM --> CUSTOMER["Customer or approved lab number"]
  ADMIN["Next.js Admin"] --> IVR
  IVR --> EVID["Audit/Evidence/Telemetry"]
```

Sales aggregates eligibility into the task and revalidates on callback. IVR never calls Ops/CRM to decide order state, never transitions an order and never sends a customer notification. Lab calls use approved test numbers only.

# ARCH-01 — System Context

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p08` · Nguồn: `phase-8/02`,`/10`; docx §1,§3,§18.

IVR Order Confirmation là **downstream consumer** của Commerce Order Core; kết nối privacy-safe với các hệ khác. IVR result = input signal; Order Core quyết định.

```mermaid
flowchart LR
  subgraph BP[ginsengfood-business-platform]
    OC["Commerce Order Core<br/>order state owner"]
    CRM["CRM / Trust / do-not-call<br/>module 3.1"]
    IVR[["IVR Order Confirmation<br/>module 8"]]
    NOTI[Notification owner]
  end
  subgraph OPScore[ginsengfood-ops-core]
    OPS["Operational Core<br/>Sellable Gate / Recall / Sale-Lock"]
  end
  subgraph FND[Foundation]
    EVID[Evidence Registry / Audit]
    PERM[Permission Core / Idempotency]
  end
  SIM[("Internal SIM Gateway<br/>SIM pool — CHƯA MUA")]
  ADMIN[Admin/Ops Console]
  CUST((Khách hàng))

  OC -- IvrConfirmationTaskV1 (push, D-03) --> IVR
  IVR -- ResultCallbackV1 (signal, D-04) --> OC
  OC -- fan-out availability/check (DO-03) --> OPS
  CRM -- trust/risk + do-not-call (D-12, DC-01) --> OC
  CRM -- IVRRequired event order.ivr_required_decisioned (D-09) --> OC
  IVR -- dial via adapter port (DT-01) --> SIM
  SIM -- DTMF 1/0 / disposition --> IVR
  IVR -- evidence/audit refs --> EVID
  IVR -- authz/idempotency --> PERM
  ADMIN -- RBAC actions (DF-01) --> IVR
  OC -. after decision .-> NOTI
  SIM -. outbound call .-> CUST
```

## Boundary chốt
- **Order Core** cấp task + nhận callback + revalidate (gọi ops fan-out); **owner order state**.
- **Ops** chỉ trả SellableStatus theo SKU/batch (Core gọi, không phải IVR) — DO-CORR-1.
- **CRM/business-platform** cấp trust/risk (D-12) và **do-not-call** (✅ DC-01; rich fields/Core wiring theo IR-CRM-01).
- **Notification** chỉ sau Core decision — IVR không tự gửi.
- **SIM** qua adapter port; chưa mua → mock. `CUST` chỉ được gọi thật sau release gate (DF-03).

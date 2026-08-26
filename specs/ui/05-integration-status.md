# UI-05 — Integration Status (Health)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission: `IVR_QUEUE_VIEW`. Nguồn: `architecture/06`, `seed/integration-status.sample.json`, DO-06.

## Mục đích
Theo dõi health các dependency để hiểu tại sao IVR fail-closed/hold.

## Bố cục
```
[ Dependency health cards:
   Order Core: up/down (task push, callback intake)
   SIM Gateway: MOCK_up / down  (adapter_mode)
   CRM do-not-call: up/down  (DC-01 source; IR-SALES-CRM-01 rich fields)
   Evidence Registry: up/down
 ]
[ Effect note: khi dep down -> hành vi fail-closed (không dispatch / block / technical) ]
[ Recent fail-closed events: dependency · time · effect · correlation_id ]
```

## Ý nghĩa (bám resilience)
- Ops down / ready_503 → không dispatch/không confirm (DO-06).
- CRM down → không xác định opt-out → không dispatch (DC-01 fail-closed).
- Evidence down → không final-callback → hold.
- SIM down → `IVR_TECHNICAL_EXCEPTION` (không no-answer).

## Actions
- View-only. (Điều khiển SIM ở UI-07/dashboard, không ở đây.)

## P0
- Hiển thị lý do fail-closed để vận hành minh bạch; không cho "override" health để ép dispatch.

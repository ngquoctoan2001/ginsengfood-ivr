# UI-01 — Dashboard (Queue / Capacity / Incident)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission xem: `IVR_QUEUE_VIEW`. Nguồn: `architecture/06`, `api/03`.

## Mục đích
Tổng quan vận hành IVR theo phiên/program: call volume, success/confirm/cancel/no-answer rate, queue depth, SIM health, capacity incident.

## Bố cục (wireframe)
```
[ Filter: program (GH/24-7) · time range · adapter_mode(MOCK/REAL) ]
[ KPI cards: call_success_rate · confirm_rate · cancel_rate · no_answer_rate · technical_exception_rate · missed_deadline_count · sim_failure_rate · cost_per_confirmed_order ]
[ Queue panel: pending · dispatching · attempt2-due · held · blocked ]
[ SIM panel: total/idle/active/disabled/health_failed · adapter_mode ]
[ Capacity incidents (open): id · scope · shortage_reason · opened_at ]
[ Banner: REAL_CUSTOMER_CALL_ALLOWED = NO (release gate chưa pass) ]
```

## Dữ liệu hiển thị / ẩn
- Hiển thị: metrics tổng hợp, đếm theo status, SIM health, incident (không PII).
- **Ẩn**: raw phone, customer profile, payment, health.

## Actions
| Action | Permission | API | Ràng buộc |
| --- | --- | --- | --- |
| Pause queue | `IVR_QUEUE_PAUSE` | `POST /queue:pause` | reason + evidence + audit |
| Resume queue | `IVR_QUEUE_RESUME` | `POST /queue:resume` | chỉ khi incident đã xử lý |

## P0
- Banner luôn nhắc `REAL_CUSTOMER_CALL_ALLOWED` (DF-03). Không action nào force order/bypass blocker.

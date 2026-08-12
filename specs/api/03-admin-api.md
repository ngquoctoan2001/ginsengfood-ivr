# API-03 — Admin API

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/11` §5,§8; `/08` (monitoring/privacy); DF-01 (RBAC).
Base path `/v1/ivr/order-confirmation/*`. Admin RBAC server-side; mọi POST có `reason` + `X-Actor-Id` + audit + `Idempotency-Key`.

## 1. Endpoint & permission
| Endpoint | Method | Permission (DF-01) | Contract | Chức năng |
| --- | --- | --- | --- | --- |
| `/queue` | GET | `IVR_QUEUE_VIEW` | Queue projection (masked) | Xem queue/capacity/incident |
| `/queue:pause` | POST | `IVR_QUEUE_PAUSE` | `IvrAdminAction` | Pause queue (reason/evidence) |
| `/queue:resume` | POST | `IVR_QUEUE_RESUME` | `IvrAdminAction` | Resume sau khi incident resolved |
| `/sim-channels/{simChannelId}:disable` | POST | `IVR_SIM_DISABLE` | `IvrAdminAction` | Disable SIM (health/failure reason) |
| `/sim-channels/{simChannelId}:enable` | POST | `IVR_SIM_ENABLE` | `IvrAdminAction` | Enable SIM sau health pass |
| `/technical-retries` | POST | `IVR_MANUAL_RETRY` | `IvrTechnicalException` | Request technical retry (không tăng customer attempt) |
| `/admin-reviews` | POST | `IVR_RESULT_REVIEW` | `IvrAdminAction` | Ghi review/annotation |

## 2. Ràng buộc admin action (P0)
Mỗi POST phải có: authenticated actor (`X-Actor-Id`), permission server-side, `reason`, `target_type`+`target_id`, audit record, evidence ref nếu ảnh hưởng queue/SIM/retry/result, `no_policy_bypass=true`.

Admin **KHÔNG** được:
- Gọi khách ngoài attempt policy (D-10) hoặc reset customer attempt count.
- **Force confirm/cancel order** (D-02: order state do Core; P0-IVR-002).
- Enable SIM khi health check đang fail.
- Resume queue khi capacity incident chưa xử lý.
- Bỏ qua blocker (sellable/recall/sale-lock/do-not-call) — DO-*/DC-01.

## 3. Privacy (masked)
- `/queue`, `/call-jobs/{id}` chỉ hiển thị `phone_masked`, `order_code`, program, status, deadline. **Không** raw phone/full address/payment/health (phase-8/08; P0-IVR-007).

## 4. SIM/eSIM admin theo mode
- Dev dùng mock channels; lab ban đầu có 1 SIM thật và destination allowlist; production target 32 eSIM channels. Channel count là config. UI/API phải hiển thị mode/provider và không được bật real call permission chỉ vì channel được enable.

## Báo cáo (admin)
- **7 endpoint admin** (1 GET + 6 POST), mỗi cái map 1 permission `IVR_*` (DF-01). Không endpoint nào cho phép force order/bypass blocker.

## Runtime-gate controls — bất đối xứng theo chiều an toàn

`OD-V1-20` (chờ Security/Release owner) đề xuất quyền `IVR_RUNTIME_GATE_ADMIN`. Quy tắc áp cho mọi endpoint đổi runtime gate:

- **Chiều giảm rủi ro luôn được phép** ở mọi environment: bật `globalDialKillSwitch`, thu hẹp/làm rỗng `labDestinationAllowlist`, đặt `realCustomerCallAllowed=false`. Chỉ cần permission + `reason` + audit; **không** four-eyes, **không** chờ deployment. Một kill switch không bật được trong sự cố là kill switch hỏng.
- **Chiều tăng rủi ro luôn bị gate**: tắt kill switch, mở rộng allowlist → four-eyes + `reason`; ở `PRODUCTION_REAL` chỉ qua deployment có approval (P7-3/P9-1). `realCustomerCallAllowed=true` chỉ qua P9-1 sau DF-03. `v1NotificationEnabled`/`recordingEnabled` bật lên bị từ chối ở mọi mode.
- Không đọc được trạng thái kill switch ⇒ coi như **ON** (fail-closed).
- Actor thực hiện call không được tự mở rộng allowlist cho đích mình sắp gọi.

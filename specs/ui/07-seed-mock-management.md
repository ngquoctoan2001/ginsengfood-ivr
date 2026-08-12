# UI-07 — Seed / Mock Management (NON-PROD only)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission: `IVR_SIM_ENABLE`/`IVR_SIM_DISABLE` (+ non-prod guard). Nguồn: `seed/*`, DT-01.

## Mục đích
Điều khiển môi trường test: `adapter_mode` (MOCK/REAL), nạp seed, chọn call-scenario, bật/tắt integration-status profile để chạy dry-run/smoke.

## Bố cục
```
[ Environment banner: NON-PROD · REAL_CUSTOMER_CALL_ALLOWED=NO ]
[ Adapter: adapter_mode = MOCK (REAL disabled tới khi mua SIM + release gate) ]
[ Seed loader: customers/orders/products/inventory/tasks ]
[ Scenario runner: chọn SCN-* -> chạy dry-run -> xem result mong đợi vs thực tế ]
[ Integration-status profile: chọn STATUS-* (all-up / *-down / ready-503) ]
[ SIM channels (mock): enable/disable (non-prod) ]
```

## Actions
| Action | Permission | Ràng buộc |
| --- | --- | --- |
| Đổi adapter_mode | `IVR_SIM_ENABLE` + non-prod | **REAL bị khóa** tới khi mua SIM (DT-01) + release gate (DF-03) |
| Chạy scenario dry-run | ops non-prod | không gọi khách thật |
| Áp integration-status profile | ops non-prod | để test fail-closed |

## P0
- Màn này **chỉ hiện ở non-prod**; production ẩn. Không cho set `REAL` khi chưa pass release gate. Không seed vào prod.

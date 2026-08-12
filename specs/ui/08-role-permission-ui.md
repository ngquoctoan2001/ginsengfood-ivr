# UI-08 — Role / Permission & Matrix

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Nguồn: DF-01 (`IVR_*` ở Permission Core), `api/03`, `seed/agents.sample.json`, `functional/07`.

## 1. Permission `IVR_*` (DF-01)
`IVR_QUEUE_VIEW` · `IVR_QUEUE_PAUSE` · `IVR_QUEUE_RESUME` · `IVR_SIM_ENABLE` · `IVR_SIM_DISABLE` · `IVR_MANUAL_RETRY` · `IVR_RESULT_REVIEW`. Enforce **server-side** (client chỉ ẩn/hiện; backend xác thực lại).

## 2. Role đề xuất (seed agents)
| Role | Permissions |
| --- | --- |
| OpsViewer | `IVR_QUEUE_VIEW` |
| Ops | `IVR_QUEUE_VIEW`, `IVR_MANUAL_RETRY`, `IVR_SIM_DISABLE` |
| AdminIM | + `IVR_QUEUE_PAUSE/RESUME`, `IVR_SIM_ENABLE`, `IVR_RESULT_REVIEW` |

## 3. Ma trận Permission ↔ Màn/Action
| Màn / Action | Permission | API |
| --- | --- | --- |
| Dashboard / Call-log / Call-detail (view) | `IVR_QUEUE_VIEW` | `GET /queue`, `GET /call-jobs/{id}` |
| Pause queue | `IVR_QUEUE_PAUSE` | `POST /queue:pause` |
| Resume queue | `IVR_QUEUE_RESUME` | `POST /queue:resume` |
| Disable SIM | `IVR_SIM_DISABLE` | `POST /sim-channels/{id}:disable` |
| Enable SIM | `IVR_SIM_ENABLE` | `POST /sim-channels/{id}:enable` |
| Technical retry | `IVR_MANUAL_RETRY` | `POST /technical-retries` |
| Admin review | `IVR_RESULT_REVIEW` | `POST /admin-reviews` |
| Seed/mock mgmt (non-prod) | `IVR_SIM_ENABLE/DISABLE` + non-prod | — |
| Script approve | owner sign-off | — |

## 4. Ràng buộc P0 (mọi role)
- **Không** role nào force confirm/cancel order (D-02), reset attempt count, vượt max attempt (D-10), bypass blocker (DO-*/DC-01), hay set `REAL` khi chưa release gate.
- Mọi action ghi: `actor_id`, `permission`, `reason`, `target`, `before/after`, `correlation_id`, `evidence_ref`, `no_policy_bypass=true`.

## Báo cáo (p12)
1. **Số màn:** 8 (dashboard, call-log, call-detail, menu-config, integration-status, callback-request, seed-mock, role-permission).
2. **Ma trận permission↔màn:** mục 3 (7 permission `IVR_*` map đủ action + API).
3. **Điểm privacy cần owner duyệt:** (a) có hiển thị `customer_name_short` không (mặc định ẩn); (b) retention/hiển thị call log (DF-07); (c) recording nếu bật (DT-05); (d) export nào được phép (mặc định không PII). Tất cả mặc định privacy-safe (chỉ masked).

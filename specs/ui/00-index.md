# UI SRS — Index (Admin/Ops Console)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `plan/ivr-orther/prompts/p12-generate-ui-specs.md`
Nguồn: `phase-8/08` (giám sát/audit/privacy), `/11 §5` (admin API + permission); `TECH-01` (RBAC); `specs/srs/functional/07`, `api/03`, `data/05`, `architecture/06`; agents `seed/agents.sample.json`.
Cấp độ: **wireframe/spec** — KHÔNG code frontend. UI **nội bộ** (ops/admin), privacy-safe.

## 1. Màn hình
| File | Màn | Permission chính |
| --- | --- | --- |
| [01-dashboard.md](01-dashboard.md) | Dashboard queue/capacity/incident | `IVR_QUEUE_VIEW` |
| [02-call-log.md](02-call-log.md) | Danh sách call-job (masked) | `IVR_QUEUE_VIEW` |
| [03-call-detail.md](03-call-detail.md) | Chi tiết task→job→attempt→result→callback | `IVR_QUEUE_VIEW` |
| [04-ivr-menu-config.md](04-ivr-menu-config.md) | Cấu hình script/biến (read-only + approve) | `IVR_QUEUE_VIEW` (+approve owner) |
| [05-integration-status.md](05-integration-status.md) | Health sales/ops/SIM/CRM/evidence | `IVR_QUEUE_VIEW` |
| [06-callback-request.md](06-callback-request.md) | Admin review + technical retry | `IVR_RESULT_REVIEW`/`IVR_MANUAL_RETRY` |
| [07-seed-mock-management.md](07-seed-mock-management.md) | Bật/tắt mock, dry-run (non-prod) | `IVR_SIM_ENABLE/DISABLE` (non-prod) |
| [08-role-permission-ui.md](08-role-permission-ui.md) | RBAC & ma trận permission↔màn | admin |

## 2. Nguyên tắc P0 (mọi màn)
- ❌ **Không** hiển thị raw phone / full profile / full address / payment / health / order history (chỉ `phone_masked` + order refs) — D-05, phase-8/08; P0-IVR-007.
- ✅ Mọi admin action: **permission server-side** (DF-01) + `reason` + `actor` + audit + `no_policy_bypass=true`.
- ❌ **Không** nút "force confirm/cancel order" (D-02) hay bypass blocker (DO-*/DC-01).
- ✅ Mọi thay đổi map tới admin API `api/03` + permission tương ứng.

## 3. Báo cáo (xem 08)
Số màn: 8 · Ma trận permission↔màn: `08-role-permission-ui.md` · Điểm privacy cần owner duyệt: liệt kê cuối `08`.

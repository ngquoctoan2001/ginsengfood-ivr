# UI SRS — Index (Admin/Ops Console)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12-generate-ui-specs.md` (prompt sinh tài liệu đã nghỉ hưu 2026-09-04; còn trong git history)
Nguồn: `phase-8/08` (giám sát/audit/privacy), `/11 §5` (admin API); `specs/srs/functional/07`, `api/03`, `data/05`, `architecture/06`; IR-06 §4A.
Cấp độ: **reference spec** — Module 3 sở hữu identity, role và UI deploy; IVR chỉ giữ reference implementation privacy-safe.

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
| [08-role-permission-ui.md](08-role-permission-ui.md) | Mapping màn → service tier; role do Module 3 quyết định | M3 owner |

## 2. Nguyên tắc P0 (mọi màn)
- ❌ **Không** hiển thị raw phone / full profile / full address / payment / health / order history (chỉ `phone_masked` + order refs) — D-05, phase-8/08; P0-IVR-007.
- ✅ Mọi admin action: **tier server-side** (DF-01) + `reason` + `actor` + audit + `no_policy_bypass=true`.
- ❌ **Không** nút "force confirm/cancel order" (D-02) hay bypass blocker (DO-*/DC-01).
- ✅ Mọi thay đổi map tới admin API `api/03` + tier tương ứng.

## 3. Báo cáo (xem 08)
Số màn: 8 · Ma trận màn↔tier: `08-role-permission-ui.md` · Role↔tier: Module 3 phải sign-off trước integration.

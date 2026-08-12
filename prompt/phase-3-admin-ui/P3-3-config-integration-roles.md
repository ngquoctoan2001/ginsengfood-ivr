# PROMPT P3-3 — Config, Integration Status, Seed/Mock & Roles UI

> **Phụ thuộc readiness (2026-08-12, W-0062):** badge/UI dựa trên `/health/ready=503` chỉ có tín hiệu thật sau `P6-1` (W-0040). Trước đó test badge phải dùng stub, và **không** được ghi là đã verify fail-closed.

## 0. Meta
| | |
| --- | --- |
| **ID** | `P3-3` · **Phase** 3 — Admin UI |
| **Work ID** | `W-0027` (canonical tracker §5) |
| **Prereq** | `P3-2`, `P2-8` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Next.js |

## 1. ROLE
Bạn là **Senior Frontend Engineer**. Bạn xây các màn cấu hình & quản trị: IVR menu/script config, integration status (Order Core/ops/CRM/SIM health), seed/mock management (dev/staging), và role/permission UI. Bạn giữ mọi thay đổi có audit và không mở lối tắt gọi thật.

## 2. CONTEXT
Ops cần cấu hình script/menu, xem trạng thái tích hợp downstream (fail-closed nhìn thấy được), quản lý seed/mock cho dry-run, và gán quyền. Đây là màn "back-office" hoàn thiện bộ admin.

## 3. SOURCE SPECS (đọc trước)
- `specs/ui/04-ivr-menu-config.md`, `specs/ui/05-integration-status.md`, `specs/ui/06-callback-request.md`, `specs/ui/07-seed-mock-management.md`, `specs/ui/08-role-permission-ui.md`
- `seed/integration-status.sample.json`
- `plan/ivr-orther/decisions-log.md` §DF-01 · §DO-06 (fail-closed health) · §DT-01 (adapter mode) · §AS-07 (KEY_9 disabled)

## 4. DECISIONS & CONSTRAINTS
- **Script/menu config:** chỉ script **approved** mới dùng được (intake reject nếu chưa approved); UI đánh dấu trạng thái approve; KEY_9 hiển thị NOT_ENABLED (AS-07).
- **Integration status:** hiển thị health Order Core/ops sellable gate/CRM/SIM; `ready=503` → badge đỏ "fail-closed: không dispatch" (DO-06).
- **Seed/mock:** chỉ **non-prod**; UI chặn ở prod; đổi `IVR_ADAPTER_MODE` chỉ dev/staging (không REAL từ UI).
- **Roles:** gán permission `IVR_*` (nếu quyền admin); mọi thay đổi audit + reason.

## 5. INPUTS / DEPENDENCIES
- API client (P3-1); admin config/status endpoints; `seed/integration-status.sample.json`.

## 6. BUILD STEPS
1. **IVR menu/script config**: xem/sửa template + variable cho phép; trạng thái approve (approved/draft); preview an toàn (không PII). KEY_9 = NOT_ENABLED (read-only).
2. **Integration status**: dashboard health downstream + độ tươi snapshot (captured_at); hiển thị rõ trạng thái fail-closed; nút refresh (không action nguy hiểm).
3. **Seed/mock management** (non-prod): chọn scenario, seed dataset, xem adapter mode (MOCK); chặn hoàn toàn ở prod env.
4. **Callback request/replay view** (ui/06): xem callback đã gửi + Core response; replay chỉ non-prod, cùng idempotency key (không tạo signal mới).
5. **Role/permission UI**: liệt kê role + permission `IVR_*`; gán/thu hồi (nếu quyền), audit + reason.
6. Guard prod: mọi màn dev-only ẩn/khoá ở prod.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `admin-ui/app/config/**` | Menu/script config |
| `admin-ui/app/integration/**` | Integration status |
| `admin-ui/app/seed/**` | Seed/mock (non-prod) |
| `admin-ui/app/roles/**` | Role/permission |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-UI-SCRIPT-01` | component | script chưa approved → đánh dấu; KEY_9 read-only NOT_ENABLED. |
| `UT-UI-HEALTH-02` | component | `ready=503` → badge "fail-closed"; captured_at hiển thị. |
| `UT-UI-SEED-PROD-03` | component | env=prod → seed/mock + mode toggle bị khoá. |
| `UT-UI-ROLE-04` | component | gán permission audit + reason; ẩn nếu thiếu quyền admin. |
| `E2E-UI-REPLAY-05` | e2e | replay callback non-prod dùng cùng idempotency key (không signal mới). |

Trace: `specs/testing/05`, `specs/ui/04-08`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] seed/mock chặn prod; [ ] không REAL mode từ UI; [ ] fail-closed hiển thị; [ ] role change audit.
**Reviewer:** script approval gate đúng; replay không tạo signal mới; KEY_9 disabled.

## 10. EVIDENCE EXPECTED
Screenshot 4 màn, prod-guard demo, health fail-closed badge, role-change audit, replay same-key proof.

## 11. FORBIDDEN
- ❌ Bật `IVR_ADAPTER_MODE=REAL`/gọi thật từ UI. ❌ Seed/mock ở prod. ❌ Dùng script chưa approved. ❌ Replay tạo signal mới (phải cùng key).

## 12. DEFINITION OF DONE
- [ ] 4 nhóm màn + guard prod; 5 test §8 xanh; evidence §10 đủ. **Kết thúc Phase 3: bộ admin UI hoàn chỉnh (MOCK).**

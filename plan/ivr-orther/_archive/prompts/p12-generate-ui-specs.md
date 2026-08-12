# P12 — Generate UI Specs (Admin/Ops Console)

## Tên nhiệm vụ
Sinh admin/ops UI specs cho IVR.

## Bối cảnh
Baseline `IVR-08` (giám sát/audit/privacy) và `IVR-11` §5 (admin API + permission) là nguồn. UI chỉ nội bộ (ops/admin), privacy-safe (hiện `phone_masked`, ẩn full phone/address/payment).

## Input cần đọc
- `specs/srs/functional/07-admin-operations.md`, `specs/srs/api/03-admin-api.md`
- `docs/documents/4. phase/phase-8/08-GIÁM SÁT QUẢN TRỊ BẰNG CHỨNG KIỂM TOÁN VÀ RIÊNG TƯ.md`
- `docs/documents/3. tech/02-TECH-01-...RBAC...md`

## Output cần tạo
- `specs/srs/ui/`:
  - `00-index.md`
  - `01-dashboard.md` (queue/capacity/incident overview)
  - `02-call-log.md` (list, masked phone, filter theo program/status/deadline)
  - `03-call-detail.md` (task→job→attempt→result→callback trace, evidence refs)
  - `04-ivr-menu-config.md` (script template/version, biến được phép)
  - `05-integration-status.md` (sales/ops/SIM health)
  - `06-callback-request.md` (admin review, technical retry)
  - `07-seed-mock-management.md` (bật/tắt mock, dry-run)
  - `08-role-permission-ui.md` (RBAC: IVR_QUEUE_VIEW/PAUSE/RESUME, IVR_SIM_ENABLE/DISABLE, IVR_MANUAL_RETRY, IVR_RESULT_REVIEW)

## Quy tắc
- Không hiển thị raw phone/full profile/payment/health.
- Mọi admin action cần reason + audit + permission server-side.
- UI không có nút "force confirm/cancel order".
- Đánh dấu wireframe-level; không code frontend.

## Checklist hoàn thành
- [ ] Đủ các màn theo brief.
- [ ] Mỗi màn nêu permission + dữ liệu hiển thị/ẩn.
- [ ] Admin action map đúng API + permission.

## Điều cấm
- KHÔNG thiết kế UI cho phép bypass P0 blocker.
- KHÔNG code UI production.

## Báo cáo cuối
1. Số màn hình.
2. Ma trận permission ↔ màn.
3. Điểm privacy cần owner duyệt.

# DEV PROMPT 08 — M8.2G Admin Monitoring / UI

## Mục tiêu
Admin API + console: monitor queue/SIM/incident, review, technical retry — RBAC, PII masked, no force order.

## Requirement / Decision
`FR-IVR (admin)`, `P0-IVR-006/007` · `DF-01`, `D-02`, `D-05`, `DT-05`.

## Source spec
- `specs/srs/api/03-admin-api.md`, `functional/07-admin-operations.md`
- `specs/srs/ui/*` (8 màn), `data/05-pii-policy.md`

## Build scope
1. Admin API: `queue`, `queue:pause/resume`, `sim-channels:enable/disable`, `technical-retries`, `admin-reviews` — permission `IVR_*` server-side; mỗi POST có `reason`+`actor`+audit+`no_policy_bypass`.
2. UI wireframe (8 màn): dashboard, call-log, call-detail (trace + evidence), menu-config, integration-status, callback-request, seed-mock (non-prod), role-permission.
3. PII masked (chỉ `phone_masked`); ẩn full phone/address/payment/health; recording OFF.

## Done gate (docx M8.2G)
- [ ] PII masked; **không** raw phone (P0-IVR-007).
- [ ] Admin **không** fake result / force order (D-02) / bypass blocker / vượt max.
- [ ] Mọi action audit + permission enforce server-side.

## Evidence expected
RBAC test (viewer không pause được); no-raw-phone UI scan; admin-action audit; no-force-order proof.

## Forbidden
KHÔNG nút force confirm/cancel order; KHÔNG bypass blocker; KHÔNG set REAL adapter khi chưa release gate; KHÔNG code frontend production ngoài phạm vi wireframe đã duyệt.

## Test
`testing/07` (SEC-04..07), smoke `M8-P0-011`.

# DEV PROMPT 02 — M8.2A Task Intake

## Mục tiêu
Nhận `IvrConfirmationTaskV1` từ Order Core, validate, tạo CallJob hoặc reject/hold.

## Requirement / Decision
`FR-IVR-INTAKE-001..009`, `P0-IVR-001` · `D-01/02/03/10`, `DO-02`, `DC-01/IR-CRM-01`.

## Source spec
- `specs/srs/functional/01-task-intake.md`
- `specs/srs/api/02-internal-api.md §2`, `api/05-order-core-contracts.md §1`, `api/06-error-codes.md`
- `specs/srs/data/02-mapping-sales-platform.md`

## Build scope
1. `POST /v1/ivr/order-confirmation/tasks`: auth allowlist (Order Core), idempotency, correlation.
2. Validate: Official Order (`is_ivr_callable`, `order_version`), program∈{GH,24-7} + `max_attempts=2` + window khớp (D-10), official contact/phone, blocker snapshot `sellable_status[]` (không `NOT_SELLABLE/BLOCKED/recall/sale_lock`), `call_restriction=false` (DC-01 mock source), script approved, evidence/privacy version.
3. Response model (api/06 §1b): `ACCEPTED*/SKIPPED/HELD` → 200 + decision; `REJECTED*/BLOCKED` → 4xx + envelope (`code` §1c).
4. Ghi `ivr_confirmation_tasks` + audit/evidence.

## Done gate (docx M8.2A)
- [ ] Reject Quote/Cart/Draft; chỉ Official Order (P0-IVR-001).
- [ ] Idempotency: same key/payload → cũ; khác payload → `409`.
- [ ] Blocker/opt-out snapshot → `TASK_BLOCKED_OPERATIONAL`.
- [ ] Policy mismatch (max≠2/window sai) → `409`.

## Evidence expected
Log reject quote/cart/draft + accept official; idempotency test; blocked-operational sample; intake decision audit.

## Forbidden
KHÔNG ghi order state; KHÔNG dispatch thật (chỉ tạo CallJob; dispatch ở M8.2C dry-run).

## Test
`testing/02` (UT-INTAKE), `testing/03` (IT-01..05), `testing/04` (CT-TASK), smoke `M8-P0-001`.

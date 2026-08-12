# DEV PROMPT 07 — M8.2F Order Core Callback

## Mục tiêu
Gửi result signal về Order Core với evidence; Core revalidate; IVR không transition.

## Requirement / Decision
`FR (result/callback)`, `P0-IVR-002/003` · `D-02/04`, `D-06`, `DO-03/06`.

## Source spec
- `specs/srs/api/05-order-core-contracts.md §2`, `functional/05`, `workflows/06-race-condition-revalidation.md`
- `specs/srs/database/02-tables.md` (`ivr_result_callbacks`), `api/07`

## Build scope
1. Core Callback Adapter: `POST {orderCore}/v1/orders/{id}/ivr-result-callbacks` với `order_version_seen_by_ivr`, result, `recommended_core_action` (advisory), `evidence_ref`/`audit_ref`, idempotency.
2. Xử lý `core_response_code`: ACCEPTED/STALE/BLOCKED/REVIEW/RETRY_ALLOWED|BLOCKED. Retry chỉ khi timeout/5xx/RETRY_ALLOWED, **cùng idempotency key**, bounded.
3. Ghi `ivr_result_callbacks` state machine.
4. Mock Core revalidate (dry-run): stale (version mismatch), block (recall inject), accepted.

## Done gate (docx M8.2F)
- [ ] Order Core ack/reject; **IVR không transition trực tiếp** (P0-IVR-002).
- [ ] Stale callback → không transition.
- [ ] Race (phím 1 + blocker) → Core BLOCKED, không confirm (D-06, P0-IVR-003).
- [ ] Callback thiếu evidence → hold/reject.

## Evidence expected
Callback ack/reject; idempotency; state-transition-only-from-Core; race-block sample; stale sample.

## Forbidden
KHÔNG cho IVR/SIM update order state; KHÔNG duplicate transition; KHÔNG accept callback mù.

## Test
`testing/03` (IT-06..09), `testing/04` (CT-CB), `testing/05` (E2E-06), smoke `M8-P0-010`, `SMK-010`.

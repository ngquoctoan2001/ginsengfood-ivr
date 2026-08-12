# DEV PROMPT 03 — M8.2B Eligibility Resolver

## Mục tiêu
Quyết định eligible/skip/block trước dispatch, consume snapshot (không hardcode).

## Requirement / Decision
`FR-IVR-ELIG-001..010` · `D-06/12/13`, `DO-01/CORR-1/2/3`, `DC-01/IR-CRM-01`.

## Source spec
- `specs/srs/functional/02-eligibility-and-blockers.md`
- `specs/srs/data/03-mapping-ops-core.md`, `architecture/05-resilience.md §1-2`

## Build scope
1. Resolver hợp nhất: order-callable, program/window, official contact (`phone_validation_status=PASS`), trust (skip chỉ khi TRUSTED + `trusted_skip_allowed` + contact ổn + no risk/blocker — D-12), blocker (`sellable_status[]` decision/flags — DO-01), do-not-call (`call_restriction` — DC-01 mock source), capacity.
2. Fail-safe: source down → không dispatch / hold/review (fail-closed).
3. Ghi `eligibility_decision` + evidence.

## Done gate (docx M8.2B)
- [ ] TRUSTED+allowed+no-risk/blocker → `TASK_SKIPPED_TRUSTED_CUSTOMER`; trusted+risk → vẫn gọi.
- [ ] Blocker/opt-out active → block, không dispatch.
- [ ] Source (trust/contact/blocker) thiếu → fail-safe no-dispatch.

## Evidence expected
Snapshot pass/fail từng gate; trusted-skip sample; blocked sample; fail-safe sample.

## Forbidden
KHÔNG hardcode khách trusted; KHÔNG override recall/sale-lock; KHÔNG kéo dài window.

## Test
`testing/02` (UT-ELIG), `testing/03` (IT-03/04, IT-12..17), smoke `SMK-015/016`, `M8-P0-008`.

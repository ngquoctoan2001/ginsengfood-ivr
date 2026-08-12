# PROMPT P11-2 — Sales/Auth Target Contract Closure Pack

## 0. Meta

Work `W-0058`; owns closure coordination for W-0002..W-0007; start after P1-1 draft.

## 1. Outcome

Create one review pack/ticket set that Sales/Product/Privacy/Security can answer with code/API evidence. Target Contract V1 stays draft until every required item is accepted.

## 2. Tickets/contracts

1. task producer: GH+ONLINE and 24/7+COD, IVR-required flag, callable states/windows/policy;
2. task data: order version, eligibility/restriction/evidence;
3. privacy-safe speech summary with samples/limits/full-address prohibition;
4. dial-token issue/resolve/TTL/one-use/audit;
5. generic callback target, semantic ACK, idempotency/version/revalidation;
6. no-answer wait-for-timeout behavior and race tests;
7. production JWT issuer/audience/scope/TTL/JWKS, sandbox credentials and mTLS decision;
8. OpenAPI compatibility/deprecation/consumer-driven test ownership.

Each ticket includes current source evidence, target delta, sample payload, acceptance tests, owner, due/gate, mock fallback and exact closure artifact. Current GH endpoint is a compat ticket, not closure for generic target. Notification is excluded/disabled V1.

## 3. Acceptance

Run CDC against supplied sandbox; pin upstream commit/OpenAPI hash; update the corresponding W-0002..7 separately. Ticket “done” without merged code/OpenAPI/test evidence remains `BLOCKED_EXTERNAL`.

## 5. Forbidden
- ❌ Đóng bất kỳ `W-0002..W-0007` nào vì IVR đã mock.
- ❌ Tự phê duyệt Golden Hour ONLINE (`OD-V1-13`), `ivr_confirmation_required` (`OD-V1-14`), attempt policy (`OD-V1-16`), whitelist lời thoại (`OD-V1-15`) hay dial-token semantics (`OD-V1-17`).
- ❌ Coi đề xuất kỹ thuật của dev Sales là chữ ký owner.

## 6. Definition of Done
- [ ] Mỗi ticket có: current evidence, target delta, sample payload, acceptance test, owner, due, gate, mock fallback, closure artifact chính xác.
- [ ] `OD-V1-01..07` và `OD-V1-13..18` đều có ticket tương ứng.
- [ ] Trạng thái các external row giữ `BLOCKED_EXTERNAL`; đạt tối đa `EVIDENCE_SUBMITTED` cho pack tài liệu.

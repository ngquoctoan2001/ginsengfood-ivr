# PROMPT P2-1 — Target V1 Task Intake

## 0. Meta

Work `W-0018` · prereq `P1-1` (`W-0014`), `P1-2` (`W-0015`), `P1-3` (`W-0016`), `P2-7` (`W-0024` — script registry) · mode `MOCK`.

> `P1-4` (developer portal) **không** phải prereq; wildcard `P1-*` cũ đã over-constrain. `P2-7` chạy **trước** `P2-1` để intake có registry mà fail-closed trên `IVR_SCRIPT_NOT_APPROVED` (xem `P2-7` §0).

## 1. Role/outcome

Bạn là Senior .NET Backend Engineer. Implement `POST /v1/ivr/order-confirmation/tasks` atomically/idempotently for both business programs and all privacy/policy gates.

## 2. Read first

Governance/tracker · Target V1 · `functional/01` · `api/02/05/06/07` · IVR OpenAPI · database specs.

## 3. Validation order

1. auth/source, headers, contract/schema;
2. idempotency key/payload hash replay or conflict;
3. official identity/order version/window;
4. exact matrix GH+ONLINE or 24/7+COD and required flag true;
5. policy version/max/offsets/environment approval;
6. phone refs/dial-token/expiry, reject raw phone;
7. speech summary schema/PII/required items-total-short-area;
8. call restriction/eligibility/evidence/script versions fail-closed;
9. execution-mode gates.

Persist task/job/outbox/audit transactionally. MOCK returns dry-run/queued state but never calls real adapter. Same key+same body replays response; changed body returns conflict.

## 4. Fakes/tests

Provide fake Sales scenarios: both happy paths; crossed payment/program; false/missing flag; stale/expired; unknown policy; missing/PII speech; token expired; blocked/opt-out; dependency evidence missing; duplicate/conflict; concurrent duplicate. Unit/integration/contract tests must verify zero job on reject and no PII in response/log.

## 5. Evidence/DoD

Record code/files, exact commands, response samples, DB assertions and log-redaction scan in W-0018. Do not mark real Sales integration complete.

## 6. Forbidden
- ❌ IVR transition/ghi order state hoặc suy diễn callable state thay Core (D-02).
- ❌ Chấp nhận tổ hợp program/payment ngoài Target matrix, hoặc `ivr_confirmation_required != true`.
- ❌ Dùng candidate attempt policy ở `PRODUCTION_REAL` khi chưa có owner sign-off (`OD-V1-08`).
- ❌ Lưu raw phone/full address; đọc script chưa approved theo mode.
- ❌ Coi fixture fake là bằng chứng Sales đã implement.

## 7. Definition of Done
- [ ] Cả hai program path (`GOLDEN_HOUR+ONLINE`, `TWENTY_FOUR_SEVEN+COD`) intake được với fixture canonical; mọi tổ hợp khác fail-closed.
- [ ] Validation order §3 có test cho từng bước; `schema_negative` trả 422, `domain_negative` trả decision đúng (xem `seed/sales-target-v1.sample.json`).
- [ ] Idempotency replay/conflict xanh; audit + correlation đầy đủ.
- [ ] Evidence trong `docs/evidence/W-0018/`. Đạt tối đa `TESTS_PASS` (mock-only).
- [ ] Cập nhật W-0018; chỉ reviewer/owner chuyển `ACCEPTED`.

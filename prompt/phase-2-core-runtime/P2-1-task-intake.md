# PROMPT P2-1 — Target V1 Task Intake

## 0. Meta

Work `W-0018` · prereq P1-* · mode `MOCK`.

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

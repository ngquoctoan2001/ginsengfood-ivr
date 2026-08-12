# PROMPT P9-1 — Production Customer-Call Release Gate

## 0. Meta

Work `W-0050` · high-risk external gate. Do not run just because P8 lab passed.

## 1. Outcome

Verify and obtain acceptance for real Sales integration, production auth/policy, 32 eSIM capacity, privacy/legal/security and operational evidence. Only an authorized release operation after all gates may set `REAL_CUSTOMER_CALL_ALLOWED=true`.

## 2. Required evidence

- Sales producer for GH+ONLINE and 24/7+COD; speech summary/dial-token; generic callback semantic ACK/version/revalidation/no-answer timeout on staging;
- production JWT/mTLS/secret rotation/network policy tests;
- owner-approved attempt policy version;
- 32 eSIM provisioning, measured concurrency/failover/caller-ID/cost and rollback;
- script/privacy/do-not-call/retention/legal approvals, recording and notification off;
- P0–P8/P10/P11 tests/evidence accepted; kill switch/cutover/rollback/on-call verified.

## 3. Execution

1. Reconcile canonical tracker: no required work/gate may be hidden by mock evidence.
2. Pin code/config/OpenAPI/provider/vendor baselines; run final staging smoke and failure drills.
3. Produce go/no-go dossier with explicit residual risks and signatures.
4. Verify technical guard refuses flip when any gate/status/evidence is missing.
5. If and only if authorized GO, promote production config, observe canary and retain immediate kill/rollback.
6. Record exact approval, timestamp, scope and post-change evidence in W-0050.

## 4. Forbidden

No global COD-only assumption; use exact two-program matrix. No customer call based on one-SIM lab, mock/Sales compat evidence, ticket text or unsigned report. No silent scope expansion.

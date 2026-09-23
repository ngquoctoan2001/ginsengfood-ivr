# W-0283 — Worker-owned eligibility and an external client that no longer fills the gap

Date: 2026-09-14. Parent: `890dfdd732982b36c73ba7e09f6cb7f7ab96d57c`.
Scope: local software / MOCK. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Owner requested sequential implementation of the report review, with a commit per task.
Module 3 retains business CALL_REQUIRED and callback-time order revalidation. IVR owns
technical admission, evidence checks and capacity checks through the existing internal endpoint.

## Change

- Worker polls durable pending jobs and calls the existing `/eligibility-checks` contract with
  its own credential. Default disabled; explicitly enabled in the sandbox overlay.
- Only open, unexpired, unrevoked pending jobs are selected. MOCK and non-MOCK intake states are
  distinguished. Review/capacity decisions are not automatically overridden.
- The stable per-job idempotency key survives restart and lost responses. Existing endpoint and
  repository locks own the atomic decision; no database lease is held while waiting on HTTP.
- Failure does not starve the rest of a batch. Host backoff and liveness report failed passes.
  Redirects are disabled so the internal credential cannot follow a redirected request.
- The partner example client no longer carries an internal token or writes eligibility. It reads
  eligibility and waits for an acknowledged callback for terminal outcomes.

## Runtime finding fixed before handoff

The first external run reached eligibility but failed six outcome checks. Actual raw event code:
`DIAL_TOKEN_TASK_MISMATCH`. `taskFrom` changed task ids while cloning the fixture token, and automatic
processing also activated the original seed task that owned it. The client now creates one synthetic
token per task. The token rule was preserved. Failed-run evidence is retained locally at
`.artifacts/w0283/sandbox-before-token-fix.json`.

## Evidence

- `sandbox-examples.json`: **24/24** passed over TCP against project `ivr-w0283`, including 3
  terminal callbacks with ACK and 3 non-terminal results with no callback.
- The worker was stopped before intake: SQL observed **6 pending example jobs**. Starting that
  same worker completed eligibility and the round trips without client lifecycle writes.
- `recovery-observation.json`: persisted audit count and duplicate final/callback checks.
- New unit tests: **6/6** (disabled, lost response/recreated runtime, batch isolation, HTTP errors,
  missing configuration). PostgreSQL selection tests include pending, decided, expired, revoked,
  closed, review and non-MOCK intake states.
- Release build passed with 0 warnings/errors. Full regression and the final focused rerun are
  recorded in `verification.json`; the first matrix result was invalidated when source changed
  during the run, and is not represented as a pass.

## Boundaries

This does not sign M3's contract, supply production credentials, authorize a SIM/customer call,
or implement M3's order transition. The sandbox receiver is fake. The new loop stays disabled
outside explicitly configured deployments. Final exact-commit verification is the later execution
task; local runtime artifacts here identify their source and container scope separately.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `UT-ELIG-LOOP-01`, `UT-ELIG-LOOP-02`, `UT-ELIG-LOOP-03`, `UT-ELIG-LOOP-04`, `IT-ELIG-LOOP-01`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

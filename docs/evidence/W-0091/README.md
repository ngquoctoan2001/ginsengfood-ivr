# W-0091 — Phase 1/2 retention and acceptance-test remediation

Status: `TESTS_PASS`

Valid findings `E-17` through `E-19`, `E-21` and `E-23` are remediated:

- `task_metadata` retention declares the intake-outbox dependency; PostgreSQL
  proof shows a held child blocks parent task/job deletion instead of creating
  an orphan or partial purge;
- missing retention periods execute the job and persist a `NOT_CONFIGURED`
  report/checkpoint with zero target mutation;
- all 13 `domain_negative` seed records execute via HTTP, rather than being
  counted only by parsing JSON;
- internal/admin privacy coverage now exercises the endpoint matrix and
  captures logs, with raw phone/address/token probes rejected or absent;
- queue, SIM, TTS-mode, TTS-PII and telephony safety assertions now cover the
  requested negative/concurrency/no-egress sides. No-egress proof inspects IL
  call targets instead of private field names; one-channel competition is
  concurrent rather than sequential;
- the eight P2-7 negative cases assert exact exception types; provider fake
  determinism runs the same scenario twice; mapper proof asserts the registry
  instance and serialized domain absence of phone fields.

The report claim that `IsCountedCustomerAttempt` is always false was incorrect:
unit/integration proof covers both non-counted technical outcomes and counted
customer no-answer outcomes. No source change was made for that false positive.

Retention JSON now truthfully records configuration mode `LAB_REAL_SIM` and
environment `DISPOSABLE_TEST_DB`; no telephony/provider was started. Final full
regression: `281/281 PASS`.

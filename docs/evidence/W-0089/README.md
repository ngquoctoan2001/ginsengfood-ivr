# W-0089 — Phase 1/2 privacy and intake correctness remediation

Status: `TESTS_PASS` (local MOCK + disposable PostgreSQL)

Valid findings `E-03`, `E-08`, `E-10`, `E-11`, `E-20` and `E-22` are closed:

- phone detection accepts Vietnamese forms with spaces, dots and hyphens while
  preserving compact-number detection;
- address detection has word boundaries, so benign product text such as
  `cao cap Han Quoc` is not interpreted as an address;
- short delivery areas accept city markers in common comma-separated forms;
- intake rejects `call_restriction=true` before persistence with
  `TASK_BLOCKED_OPERATIONAL` / `IVR_OPERATIONAL_BLOCKED`;
- a new idempotency key re-evaluates transient held/blocked policy state, while
  the same key still replays its immutable prior response;
- missing idempotency/correlation trace maps to `IVR_MISSING_TRACE` 422;
  malformed syntax remains `IVR_MALFORMED_REQUEST` 400;
- `IVR_CONTACT_INVALID` is now emitted by the intake runtime and is covered by
  unit tests; API status and response headers are asserted together.

Evidence includes separated-phone and benign-text cases, common city-area
theories, HTTP call-restriction rejection with zero persisted work, PostgreSQL
transient re-evaluation, and exact missing-trace/malformed cases. All 13
`domain_negative` seed scenarios execute through the HTTP implementation and
reach their expected runtime branch.

Final result: `281/281 PASS`. The test values are synthetic/redacted; no raw
customer record, Sales endpoint or real call was used.

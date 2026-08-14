# W-0022 / P2-5 — DTMF and disposition normalization

Date: 2026-08-14

Implementation baseline: `0183ace27b72b419c6c257e5cedd3b86d77a77aa`

Execution boundary: local/MOCK, `REAL_CUSTOMER_CALL_ALLOWED=NO`

## Delivered behavior

P2-5 introduces one domain source of truth, `DispositionMapper`, and a worker
normalization loop. A PostgreSQL repository claims one pending provider event with
`FOR UPDATE ... SKIP LOCKED`, applies the mapper, and commits the following records in
one transaction:

- the normalized `ivr_call_results` row;
- an `ivr_technical_exceptions` row for technical outcomes only;
- an open review item when the mapping requires human review;
- one evidence record linked to the raw event, attempt and result;
- one privacy-safe audit record;
- the attempt and call-job state transition.

The raw provider event written by P2-4 remains immutable. P2-5 stores only the
semantic DTMF value (`1`, `0`, `INVALID` or null) on the result/attempt. DTMF and phone
data are absent from normalization and telephony audit payloads.

## Locked mapping matrix

| Raw signal | Result | Counted | Final | Follow-up |
| --- | --- | ---: | ---: | --- |
| answered + `1` | `IVR_CONFIRMED` | yes | yes | final callback input; Core revalidates |
| answered + `0` | `IVR_CUSTOMER_CANCELLED` | yes | yes | final callback input; Core revalidates |
| answered + no input | `IVR_NO_ANSWER_ATTEMPT` or `IVR_NO_ANSWER_FINAL` | yes | at max/window | retry next customer attempt or callback final |
| answered + unsupported key, including `9` | `IVR_WRONG_INPUT` | yes | no | retry next customer attempt |
| unsupported key at the last attempt/window | `IVR_NO_ANSWER_FINAL` | yes | yes | callback final; reason retains wrong-input semantics |
| ring timeout / busy | no-answer attempt/final | yes | at max/window | never treated as technical |
| rejected | no-answer attempt/final | yes | at max/window | open review item; never cancellation |
| unreachable / invalid destination | `IVR_INVALID_PHONE_FINAL` | no | yes | callback signal + review |
| SIM / audio / DTMF / network / dropped | `IVR_TECHNICAL_EXCEPTION` | no | no | bounded technical retry |
| capacity | `IVR_CAPACITY_EXCEPTION` | no | yes | callback signal + review |
| unknown provider disposition | `IVR_TECHNICAL_EXCEPTION` | no | no | fail-safe; bounded retry/review |

## Retry and state policy

- Candidate MOCK/LAB policy: `Ivr:Scheduler:TechnicalRetryLimit=1`.
- The first technical failure for a customer-attempt number may requeue without
  incrementing the customer attempt count.
- The second technical failure, a zero retry limit, or a closed confirmation window
  disables retry and holds the job at `HELD_ADMIN_REVIEW / HELD_TECHNICAL_REVIEW`.
- A valid no-answer/wrong-input outcome increments the customer attempt count. If it
  is not final, the scheduler selects the next policy schedule offset.
- Only final results move the job to `RESULT_READY_FOR_CALLBACK / HELD_CALLBACK`.
- P2-5 produces input signals only: `no_direct_order_update=true` and
  `no_payment_or_revenue_effect=true`. It never transitions the Sales order.

The technical retry limit is a safe candidate for MOCK/LAB. Production approval and
vendor-specific retry evidence remain owner/external gates.

## Named behavior evidence

Unit matrix (`18/18 PASS`):

- `UT-NORM-01`: answered `1/0` counted final;
- `UT-NORM-02`: no-input attempt/final boundary;
- `UT-NORM-03`: busy/rejected counted no-answer, rejected review and not cancel;
- `UT-NORM-04`: technical dispositions never counted or mapped to no-answer;
- `UT-NORM-05`: invalid phone final and not counted;
- `UT-NORM-06`: key `9` is wrong input;
- `UT-NORM-UNMAP-07`: unknown disposition fails safe to technical;
- `UT-NORM-RETRY-08`: retry limit and confirmation-window boundary;
- `UT-NORM-CAP-09`: capacity final and not counted;
- `UT-NORM-LASTKEY-10`: last unsupported key closes as no-answer final.

PostgreSQL evidence (`5/5 PASS`):

- `IT-NORM-PERSIST-01`: final signal, raw/result/evidence/audit atomic persistence,
  three evidence links, idempotent repeat and privacy-safe audit;
- `IT-NORM-TECH-02`: technical + unknown signals remain not counted, one retry only,
  then admin review;
- `IT-NORM-REJECT-03`: rejected is a counted no-answer plus review, not cancellation;
- `IT-NORM-INVALID-04`: invalid destination is final without consuming an attempt;
- `IT-NORM-CONCURRENCY-05`: concurrent workers persist exactly one result/evidence
  graph for one raw event.

## Verification evidence

| Gate | Result |
| --- | --- |
| Release build with warnings as errors | PASS — 0 warnings, 0 errors |
| Contract tests | PASS — 21/21 |
| Unit tests | PASS — 124/124 |
| PostgreSQL integration tests | PASS — 64/64 |
| Total regression | PASS — 209/209 |
| Fresh aggregate line coverage | PASS — 94.61% (21,026/22,225), 3 reports, threshold 60% |
| `dotnet format --verify-no-changes` | PASS — formatted 0/202 files |
| EF pending model changes | PASS — none; P1-2 schema already contains required tables/columns |
| Admin UI lint/build/npm High audit | PASS — 0 vulnerabilities |
| GitLab CI config/OpenAPI/docs/drift/negative gates | PASS |
| NuGet vulnerability scan | PASS — no vulnerable package in 9 projects |
| Docker Compose MOCK profile | PASS |
| Gitleaks 8.30.0 working-tree scan | PASS — no leaks found |
| Locale-stable PII self-test + evidence scan | PASS — 25 text files, 2 binary files skipped |
| Official Markdown map | PASS — 416 files, 375 resolved links, 0 unresolved |
| GitNexus staged change scope | HIGH (expected) — 16 files, 101 symbols, 14 normalization flows |

One earlier combined full-test invocation observed the existing feature-flag API test
returning HTTP 500 instead of 409. The same test passed immediately in isolation;
the complete integration suite, the subsequent complete solution run, and the fresh
coverage run all passed. This is retained as a non-blocking transient observation.

## Safety and residual gates

- `Ivr:Normalization:Enabled=false` remains the fail-closed default. Enabling it
  requires the database and the P2-4 raw-event producer.
- No real SIM, modem, phone number, provider SDK, SIP/serial/socket egress or customer
  call was used. `REAL_CUSTOMER_CALL_ALLOWED=NO` remains unchanged.
- P2-6/W-0023 still owns the callback/outbox delivery to Sales/Order Core. P2-5 only
  marks final signals ready for that boundary.
- Sales API/auth/payload approval is not required to normalize local raw events, but
  remains required before P2-6 can connect to the real Sales system.
- Production technical-retry approval, vendor raw-code mapping, one-SIM lab evidence,
  and future 32-eSIM capacity remain external/not run.
- GitNexus reports a HIGH cross-layer blast radius because the new worker, repository
  and domain mapper form one end-to-end normalization path. All 14 reported flows are
  normalization/DTMF/result flows; the HIGH-risk shared `CallResultSnapshot.Create`
  factory was deliberately left unchanged.

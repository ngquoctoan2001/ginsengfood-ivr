# W-0282 — B2: the sandbox Module 3 calls into, and the five things in the way

**Date:** `2026-09-12` · **Baseline:** `main@11a0ce1` · **Mode:** `LOCAL_ONLY / MOCK`
**Safety:** `IVR_ADAPTER_MODE=MOCK`, `SIM_PROVIDER=MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`. No real
destination was reachable at any point, and no real customer call has ever been placed.

## Method, and why it mattered more than the code

This work item did not begin by reading code. It began by doing what Module 3 will do: take a
clone, start it, ask for a credential, and POST a task. Every wall that came back was a finding,
and — exactly as with `W-0280` this morning — **each one was hidden behind the previous one**.

Nothing here was visible from inside the repository. The 986-test suite was green throughout. The
API behaviour matrix covered 38/38 operations with 460 HTTP cases and zero behaviour failures. Both
remain true. They prove our composition root behaves; neither of them can prove that a partner on
another machine can get a task through to a delivered result, because neither of them is on another
machine.

---

## 1. The developer surface answered 404 on the only stack a sandbox could be built from

`deploy/docker/Dockerfile.api:45` sets `ASPNETCORE_ENVIRONMENT=Production`, and
`docker-compose.dev.yml` never overrode it. The container log says so plainly:

```
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
```

`NonProductionSurface.IsAvailable` therefore refuses, and `DevToolingEndpoints.MapIvrDevToolingEndpoints`
**does not map the routes at all** — which is the design, and it is a good one: production answers
`404` rather than `403`, so an unauthenticated caller is not told a seed loader exists.

The consequence is that `POST /dev/seed:load`, `/dev/scenarios/{id}:dry-run` and
`/dev/integration-profiles/{id}:apply` — the only documented way to prepare or reset a sandbox over
HTTP — did not exist on the containerised stack.

**Why no gate caught it.** `pnpm dev:bootstrap` exercises those three routes and passes. It runs
`Ivr.Api` **from source** on a loopback port with `ASPNETCORE_ENVIRONMENT=Development`
(`tools/dev/Invoke-DevBootstrap.ps1:160`). It is a different host from the one anybody would ship.

**Fix:** `docker-compose.sandbox.yml` sets `Staging`. Not `Development`, deliberately: both are on
the allowlist, but there is no `appsettings.Staging.json` in the repository while
`appsettings.Development.json` carries its own copy of the admin tokens. `Staging` layers no extra
file and leaves the compose file as the single source of configuration.

## 2. Nothing seeded the attempt policy, so every pushed task was held

`deploy/docker/dev-seed/seed.sql` writes the `mock-lab-v1` attempt policy. Its only applier is
`image-selftest.mjs:849`, inside that self-test's own run. A freshly-upped stack has no policy, and
`TaskIntakeService` compares the wire snapshot against the stored one, so every task a partner
pushed came back:

```json
{"decision":"TASK_HELD_POLICY_MISSING","blocked_reasons":["ATTEMPT_POLICY_NOT_FOUND"]}
```

Unlocking (1) resolves this without a second seeding mechanism: `/dev/seed:load` writes the
candidate policies through `CandidateAttemptPolicies.Create()`.

## 3. The worker ships every job disabled

Correct for a dev stack a developer starts to look around — nothing should dial. Useless for an
integration rehearsal: an accepted task sat in the queue and no result ever existed. The sandbox
overlay enables the four jobs, and widens the calling-window gate to the whole day rather than
switching it off, for the reason `W-0214` recorded and this morning's 07:54 rehearsal paid for
again: a run outside 08:00–21:00 came back entirely red because the scheduler was correctly
declining to dial. A partner in another time zone would read that as a broken sandbox.

## 4. The contract promised a 429 that nothing could produce

`IVR_RATE_LIMITED` is in `IvrErrorCodes`, mapped to `429` by `IvrErrorHttpStatus.FromCode`, and
declared as a response on **all 38 operations** in `ivr-order-confirmation.v1.yaml`. Confirmed by
two independent methods that `IvrErrors.RateLimited()` had **zero callers**:

```
$ git grep -n 'RateLimited()' -- src/ tests/
src/Ivr.Domain/Errors/IvrErrors.cs:40:    public static IvrFailureException RateLimited() =>
```

```
gitnexus_impact(target: "RateLimited", direction: "upstream", includeTests: true)
  → impactedCount: 0, risk: LOW, epistemic: "exact"
```

So a partner could read the contract, write a backoff path, and never once exercise it. This is the
same shape as `W-0280`'s third defect: a published surface believed to work, never run.

**Fix:** `ServiceQuotaMiddleware`, `ServiceQuotaCounter`, `ServiceQuotaOptions`. Three decisions
worth recording:

- **It runs LAST in the pipeline**, after authentication, authorisation and the Order Core
  allowlist. 266 of the matrix's 460 cases assert `401`/`403` for a bad credential; a ceiling in
  front of authentication would rewrite all of them into `429` once a window was exhausted.
  `IT-QUOTA-02` guards this directly.
- **The admin surface is billed by credential tier, not by `X-Actor-Id`.** That header is written by
  the caller about itself, so keying a budget on it would let one client hold unlimited budgets by
  varying a string. `IT-QUOTA-03` guards it.
- **Disabled by default.** A ceiling is a capacity judgement and IVR has not measured its capacity —
  that is `E3`, after a real SIM. An unmeasured limit switched on everywhere would reject legitimate
  traffic on a guess. The sandbox turns it on at 60/min because a sandbox is where a client should
  meet a `429` cheaply. `IT-QUOTA-04` guards the default, and that default is what keeps the 460-case
  matrix meaning what it meant.

The backoff numbers travel in `ErrorEnvelope.details`, already a free-form string map in the
published contract, so this adds no undeclared wire surface. A real `Retry-After` header needs a
distinct response component on all 38 operations and a contract release — recorded for `B3`, not
smuggled in as undocumented runtime behaviour.

## 5. The canonical fixture could not produce a call, and contradicted its own handover document

`seed/sales-target-v1.sample.json` is the file `seed/README.md` names as the Target V1 canonical
fixture and the one `IR-06` points partners at. Every task in it carried:

```json
"eligibility_snapshot": { "decision": "ELIGIBLE", "source": "fake-sales" }
```

Two documents say that is not enough. `IR-06 §3.7` marks `source_version` and `captured_at`
**Bắt buộc**. `specs/api/evidence/eligibility-snapshot.v1.schema.json` says `source_version` is
"Required by IVR policy" and `captured_at` "Must fall inside [confirmation_window_started_at,
evaluation time]". `EligibilityRules` enforces both, so the observed answer was:

```json
{"decision":"TASK_HELD_ADMIN_REVIEW","blocked_reasons":["ELIGIBILITY_SOURCE_VERSION_MISSING"]}
```

**Fix:** completed the snapshot on all nine tasks, with `captured_at` equal to each task's own
window start. And `SeedCatalog.Rebase` now moves `eligibility_snapshot.captured_at` with the four
window fields — rebasing the window while leaving the evidence in August 2026 would simply trade
`ELIGIBILITY_SOURCE_VERSION_MISSING` for `ELIGIBILITY_SNAPSHOT_STALE`.

---

## Verification

One clean run, on a volume torn down and rebuilt (`down -v && up -d`), driven over a socket by a
script that references no IVR assembly:

```
24/24 examples behaved as documented
```

| Group | Cases | Result |
| --- | ---: | --- |
| Task intake, one per telephony outcome | 6 | PASS |
| Eligibility recorded | 6 | PASS |
| Round trip to a normalised result and a delivered callback | 6 | PASS |
| Refusals a client must handle (pair, window, trace, replay, conflict) | 5 | PASS |
| Service-account ceiling | 1 | PASS — `retry_after_seconds=29`, `requests_per_window=60` |

Three of the six round-trip cases assert a **silence**: `IVR_NO_ANSWER_ATTEMPT`, `IVR_WRONG_INPUT`
and `IVR_TECHNICAL_EXCEPTION` leave an attempt outstanding, so nothing may reach Module 3. That
silence is a contract term, and a partner who reads it as failure will cancel orders IVR is still
working on — so it is asserted rather than assumed.

Report: `.artifacts/sandbox/module-3-examples.json`.

### The callback leaves the stack, proved against a receiver outside it

The default run delivers to the in-stack fake, which could in principle be flattering it. So the
run was repeated with `IVR_SANDBOX_CALLBACK_BASE_URL=http://host.docker.internal:9000` and a
throwaway receiver on the host that echoes `callback_id` and `correlation_id` the way the Target V1
contract requires (`TargetV1CallbackTransport` reports `CALLBACK_ACK_INVALID` for a canned reply, so
a stub that ignored the echo would not have been accepted). What arrived:

| `task_id` | `result_type` | `recommended_core_action` |
| --- | --- | --- |
| `TASK-M3-CONFIRM` | `IVR_CONFIRMED` | `CORE_REVALIDATE_AND_CONFIRM_ORDER` |
| `TASK-M3-CANCEL` | `IVR_CUSTOMER_CANCELLED` | `CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST` |
| `TASK-M3-BADNUMBER` | `IVR_INVALID_PHONE_FINAL` | `CORE_REVALIDATE_AND_HOLD_ADMIN_REVIEW` |

Three, and exactly three. The other three scenarios still had an attempt outstanding, and nothing
about them left the stack — which is the silence asserted above, now observed from the far side of
the wire as well. That is the `B4` two-way hookup demonstrated against a stand-in; the real one
still waits on Module 3 answering `IR-07`.

New tests: `UT-QUOTA-01..09` (counter and validator), `IT-QUOTA-01..05` (the wire behaviour through
the real API pipeline).

### One expectation of mine was wrong, and the system was right

The first run reported `TASK-M3-NOANSWER` as `IVR_NO_ANSWER_ATTEMPT` where I had written
`IVR_NO_ANSWER_FINAL`. `mock-lab-v1` permits two attempts at +0s and +150s, so the first ring-out is
correctly not final. The expectation was corrected, not the system. Recorded because the run is only
worth anything if its failures are read rather than explained away.

---

## A trap I shipped, and the owner found within the hour

The example set needs a clean volume, and the first version said so in a comment at the top of the
script and in the guide. Then I left the sandbox running, handed the owner
`pnpm sandbox:examples`, and they ran it against the volume my own run had just used. Nine red
lines, six of them `409 IVR_IDEMPOTENCY_CONFLICT`.

Every one of those answers was correct. The examples use fixed task ids because MOCK telephony
outcomes are keyed by task id, so a rerun replays the same `Idempotency-Key` while the window has
been rebased to a new now — the same key carrying a different payload, which the contract refuses on
purpose. The system was working exactly as designed and it read as a broken module.

That is the same shape of defect as everything else in this work item: a documented precondition is
not a guard. Fixed by making the script check before it mutates anything —
`GET /call-jobs?correlation_id=…` on the first scenario's correlation id, which is derived by one
shared helper so the guard cannot drift from the run. It blocks only when certain; an unreadable
probe stays quiet and lets the next step produce its own clearer message.

Two smaller things fixed alongside, both found by running it rather than reading it:

- `pnpm sandbox:up && pnpm sandbox:examples` is the natural thing to type, but `up` returns when the
  containers start, not when the API has finished migrating. Asking once produced a raw
  `ECONNREFUSED` stack trace, so preflight now waits up to 60s.
- Guidance failures are a `GuidanceError` and print as a sentence with no stack. Anything genuinely
  unexpected still shows its stack, which is when a stack is the useful thing.

Verified both directions: clean volume → `24/24`, exit 0; immediate rerun → the guard's message and
exit 1.

### Then the owner's next run found two more, and one of them was a message that lied

The guard worked — `clean: no previous example run` — and the run died in the polling phase with
the transport message I had just added: *"The sandbox is no longer reachable."*

It was reachable. `docker ps` showed the API **running, healthy, `restarts=0`**, and its log held
nothing but the usual startup warnings. The failure was a Node keep-alive socket the server
recycled, rejecting one in-flight request with a bare `fetch failed`. Ordinary, and the harness had
no tolerance for it — one transient socket error killed a whole clean run.

**A message that misdiagnoses confidently is worse than the stack trace it replaced.** The raw stack
at least pointed at the socket. Mine pointed at the sandbox and was wrong.

Two fixes, and the second is the more embarrassing:

- `call()` retries **transport failures only**, four attempts with short backoff, and reports the
  underlying code. HTTP statuses are never retried — a `409`, `422` or `429` is an answer, and
  several of them are answers this script exists to assert.
- The result loop polled six jobs **every second**. Over the default 90s that is 540 calls on the
  `admin/ivr.admin.read` account, whose own ceiling is 600 a minute. At the
  `--result-timeout-ms 200000` this very guide recommends for watching `IVR_NO_ANSWER_FINAL`, it is
  **1200** — so the command I published to Module 3 was guaranteed to be refused by the quota I had
  just built. The ceiling was right; the caller was greedy. Polling now backs off 1s→5s, which caps
  the same 200s run at **264** calls.

Verified again on a clean volume: `24/24`, exit 0.

## Open, and deliberately not decided here

**Nothing calls `POST /eligibility-checks`.** It is the step that moves a job out of `HELD_MOCK` so
it can be claimed for dialling. `src/Ivr.Worker/Program.cs` registers ten hosted services and none of
them is an eligibility loop; the only callers in the repository are `image-selftest.mjs` and the new
sandbox script.

The endpoint is tagged `internal` and its source header is literally `ivr-worker`, so the design
intent is that IVR's own worker calls it. **Module 3 must not be given that credential.** Who owns
the loop and what it revalidates at callback time (`D-06`) is an owner decision touching real
dispatch behaviour, and inventing one inside a sandbox harness would be worse than not having it.

Until it is answered, the sandbox performs the step explicitly and labels it as standing in, so a
green run cannot be mistaken for evidence that the loop exists.

## What the sandbox deliberately does not do

The fake receiver is **not** published. It sits on `ivr-internal`, which is `internal: true` and
carries a no-egress guarantee that `IT-IMG-COMPOSE-03` proves by attempting an outbound connection
and requiring it to fail. Publishing its WireMock journal would mean attaching it to the routable
network and weakening a property that is currently proved rather than claimed. Module 3 has two
better routes to the same information, both on IVR's own contract surface:
`IVR_SANDBOX_CALLBACK_BASE_URL` pointed at their own receiver, or `GET /call-jobs/{id}/detail`.

The quota is per instance and therefore per replica. Stated in `ServiceQuotaCounter`'s own
documentation rather than left for someone to discover: behind two replicas an account gets two
windows. A shared counter would put a database write on every request to make a non-production
ceiling exact; an exact ceiling belongs to the gateway in front of a real deployment.

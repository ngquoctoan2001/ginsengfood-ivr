# The production dial path — configuring it, watching it, stopping it

This is the operations page for the path `PD-01` built: a call that leaves through a carrier SIP
trunk instead of the softphone lab. It covers the configuration, the channel ceiling, what `/healthz`
will and will not tell you, what to do with a call nobody can account for, and how to stop.

Its companion is [ARI controller ownership](ari-controller-ownership.md), which covers the one
question this page does not: who is allowed to dial at all. Read that one when `/healthz` reports a
controller status you did not expect.

## Read this before the rest

**Nothing on this page can dial a customer today.** The path is wired end to end and three separate
things still hold it shut, each for a different reason:

| What holds it | Where | Why it is separate |
| --- | --- | --- |
| `IvrOptionsValidator` refuses to start with `RealCustomerCallAllowed=YES` | `IvrOptionsValidator.cs:51` | Not a flag that is merely off — the process will not boot. A release gate, not a setting. |
| `AsteriskSchedulerDispatchGateway.IsReady` requires `LAB_REAL_SIM` **and** `!RealCustomerCallAllowed` | `AsteriskSchedulerDispatchGateway.cs:28` | The gateway PD-01 wired in is still the lab's. Opening it belongs to `SIP-04` and needs an approval record. |
| `SipTrunkOptions.Enabled` is `false` and the section is absent | no deployed `appsettings` carries it | Every value in it comes from a carrier integration document that does not exist yet. |

So this page is written ahead of its subject. That is deliberate: the reasoning behind these numbers
is in hand now, and a runbook assembled the week a contract lands is a runbook assembled under time
pressure by someone reading code they have forgotten.

Where a procedure below has not been executed against a real trunk, it says so. Do not read those
sections as verified.

## 1. Configuration

Two sections, separate on purpose. `Ivr:Telephony:Asterisk` describes the ARI connection; it is the
same adapter the lab uses. `Ivr:Telephony:SipTrunk` describes the carrier route behind it. Folding
them together would mean relaxing the profile whose whole job is refusing to dial anything but
`LAB-A`.

### `Ivr:Telephony:SipTrunk`

Every value here arrives from the carrier. None of it is guessable, and none of it is committed.

| Key | What it is | Who supplies it | A wrong value looks like |
| --- | --- | --- | --- |
| `Enabled` | Master switch for the production route | You | `false` → the composition root falls back to the gateway that refuses to dial |
| `ExecutionMode` | Must be `PRODUCTION_REAL` | You | Anything else → startup refused |
| `ProviderName` | Carrier name, for evidence and cost reconciliation | You | — |
| `Environment` | Deployment environment | Platform | — |
| `TrunkEndpoint` | PJSIP endpoint name on our Asterisk | You, matching your `pjsip.conf` | Calls fail at originate with an unknown-endpoint error |
| `CarrierSipHost` | Host the carrier terminates SIP on | **Carrier** | Calls never connect — no error, just silence |
| `OutboundCallerId` | The number we are permitted to present | **Carrier** | Carrier rejects the call, or presents a number the brandname is not attached to |
| `NumberFormat` | `E164Plus` / `E164NoPlus` / `National` | **Carrier** | Calls never connect. This is the single most common integration failure, and it does not announce itself |
| `ContractedChannels` | Concurrent channels the contract grants | **Carrier** | Above the real figure → carrier-side rejections that read like network faults |
| `MaxCallStartsPerSecond` | New calls per second the carrier accepts | **Carrier** | Above the real figure → rejections under burst only, so it passes every quiet test |
| `DtmfMode` | `rfc2833` / `info` / `inband` | **Carrier** | **Keypresses vanish.** The call connects, the customer presses 1, nothing arrives |

Bounds the validator enforces at startup (`SipTrunkOptions.cs:170`): `ContractedChannels` 1–200,
`MaxCallStartsPerSecond` 1–100 and never above `ContractedChannels`, `TrunkEndpoint` ASCII
identifier characters only, `CarrierSipHost` a bare host or IP with no scheme, port, path or
userinfo. The last two are injection guards, not tidiness: both strings are concatenated into a dial
address.

`DtmfMode` here is a **record of what the carrier said**, not an instruction. Asterisk's own
`pjsip.conf` decides the real behaviour. The copy exists so the stated value can be diffed against
what the dial plan was actually given — which is the only cheap way to catch the mismatch before a
customer does.

### `Ivr:Telephony:Asterisk`, production profile

Same keys as the lab, different assertions. What production **drops** is `DestinationAlias`: there is
no pinned destination because the number arrives per call from `ProductionDialTokenVault`. The
validator refuses a production profile that sets one at all (`AsteriskAriOptions.cs:118`) — a
leftover lab value or a hand-configured number both dial somewhere the token did not authorise.

| Key | Production value | Note |
| --- | --- | --- |
| `Enabled` | `true` | |
| `ExecutionMode` | `PRODUCTION_REAL` | |
| `BaseUrl` | `http://asterisk:8088` | Must resolve to the local Asterisk or loopback; no embedded credentials, query or fragment |
| `Username` / `Password` | **secret** | Never committed. Supplied at deploy time |
| `Application` | ARI application name | Part of the controller scope |
| `Environment` | Deployment environment | Part of the controller scope |
| `DestinationAlias` | **must be empty** | |
| `AdapterMode` / `ProviderName` | `ASTERISK_ARI` | Still the ARI adapter; only what sits behind it changed |
| `DialTimeoutSeconds` | 5–120, default 30 | |
| `DtmfTimeoutSeconds` | 1–120, default 15 | How long the customer has to press a key |
| `TerminationPollMilliseconds` | default 500 | How fast an operator's cut reaches a live call. Floored at 200ms in code |
| `CooldownSeconds` | 0–3600, default 2 | |
| `RecordingEnabled` | `false` | |

### Startup will refuse a half-configured deployment

Production requires **both** sections enabled. Half a configuration falls back to
`UnavailableSchedulerDispatchGateway`, which refuses to dial, rather than starting a dial path with
one end missing. Pinned by `UT-TRUNK-DI-02`.

`PRODUCTION_REAL` additionally requires `SalesProvider=TARGET_V1` and `SimProvider=VENDOR`
(`IvrOptionsValidator.cs:82`). The three are validated as a combination, not one at a time.

## 2. Channels: raising and lowering the ceiling

**Three numbers have to agree, and they live in three different places.** This is the part most
likely to go wrong, because each one is correct on its own terms while the system is still throttled
to the smallest.

| Number | Where it lives | What it means |
| --- | --- | --- |
| `SipTrunk:ContractedChannels` | deployed config | What the carrier sold us |
| `Scheduler:MaxConcurrentDispatches` | deployed config | What **one worker process** will hold at once |
| Row count in `ivr_sim_channels` | the database | The pool every worker shares, enforced by `SKIP LOCKED` at claim time |

The effective ceiling is the smallest of the three. A trunk sold with 32 channels, a worker
configured for 32, and 4 rows in `ivr_sim_channels` gives you 4 calls, with nothing anywhere
reporting a misconfiguration.

`MaxConcurrentDispatches` is **per process**, not per system. The shared ceiling is the row count.
Two workers each configured for 32 against a 32-row pool still give 32 calls, not 64 — that is what
`IT-SCH-POOL-01/02/03` prove on a real Postgres.

### Raising, in this order

1. **Confirm the contract first.** `ContractedChannels` is not an aspiration. Above the real figure
   the carrier starts rejecting, and carrier rejections arrive looking like network faults.
2. **Add rows to `ivr_sim_channels`** up to the new figure. Status `IDLE`, `execution_mode` matching
   the deployment, `adapter_mode` matching the gateway.
3. **Raise `MaxConcurrentDispatches`.** Bounds are 1–256; the software is proven at 1, 2, 8 and 32
   (`UT-SCH-PUMP-01/02/04`).
4. **Leave `MaxCallStartsPerSecond` alone unless the carrier raised it too.** It is a separate
   allowance and the carrier polices it separately. N free slots does not license N originates inside
   one second — `UT-SCH-PUMP-03` is the test that says so.
5. Watch `ivr_channel_quarantines_total` for the first full golden hour at the new figure.

Raise it in steps and let a real session run at each one. The failures that matter here — carrier
CPS rejection, RTP starvation, DTMF loss under load — do not appear at 4 and do appear at 32.

### Lowering

Lowering `MaxConcurrentDispatches` does **not** cut live calls. The pump stops claiming new work and
lets the running calls finish (`UT-SCH-PUMP-04`). To take a specific channel out, disable it; to stop
everything, see §5.

### When the contract changes

Both `ContractedChannels` and the `ivr_sim_channels` row count, in that order, plus
`MaxConcurrentDispatches` if the worker figure was at the old ceiling. Nothing derives one from
another, and nothing warns you when they disagree — which is worth a gate of its own and does not
have one yet.

### The gap in enable

`POST /admin/sim-channels/{id}:disable` works on any channel. **`:enable` does not.** It refuses any
channel whose `adapter_mode` is not `MOCK`, and refuses outright when the channel is `QUARANTINED`,
`HEALTH_FAILED`, has a `quarantine_until`, a non-zero `fail_count`, an active job or a lease
(`InternalAdminApiService.cs:760`).

So for a production channel the admin API is **one-way**. A trunk channel that quarantines cannot be
returned to service through the API at all — it needs a database action, and that action has no
runbook because the restriction may well be deliberate fail-closed design rather than an oversight.
It is recorded here as observed behaviour. Do not discover it during an incident.

## 3. Reading `/healthz`

`GET :8081/healthz`. The body is the answer; the status code is deliberately coarse.

```json
{
  "status": "live",
  "loops": [
    { "name": "scheduler", "enabled": true, "last_tick_at": "...",
      "stale": false, "consecutive_faults": 0, "last_fault_kind": null }
  ],
  "ari_controller": { "scope": "...", "status": "Held", "may_dial": true,
                      "fencing_generation": 7, "observed_at": "..." }
}
```

**`stale` is `last_tick_at` older than three poll intervals** (`WorkerLiveness.cs:76`) — with the
default `PollIntervalMilliseconds` of 1000 that is 3 seconds. One interval would be far too tight; a
single slow database round-trip would trip it.

`last_fault_kind` is the exception **type**, never its message. A message can carry a connection
string, a row value or a half-masked phone number, and anything that can reach this port can read
this body.

| Signal | Means | Page a human? |
| --- | --- | --- |
| `status: "live"`, 200 | Every enabled loop is ticking | No |
| `status` not live, 503 | A loop stopped | **Yes** — the worker is not doing its job |
| `stale: true` on a loop | That loop missed 3 intervals | Yes if it persists past a few probes |
| `consecutive_faults` climbing | The loop is running and failing | Yes — it will not self-heal past a point |
| `ari_controller` absent | No scheduler pass has answered yet, or the scheduler is off | Only if the scheduler should be on |
| `may_dial: false` | **Not a fault.** See the ownership runbook | No — it is one of four states, two of which are healthy |

`may_dial: false` is never reported in the status code, on purpose. Failing the probe would restart
the worker, which cannot grant it the ARI application and would drop whatever calls it is still
draining.

### Metrics worth an alert

These are the counters the code actually emits. Thresholds below are **starting points, not measured
baselines** — there is no production traffic to derive them from yet, and the first golden hour is
what should replace them.

| Metric | Watch for | Why |
| --- | --- | --- |
| `ivr_channel_quarantines_total` | Any sustained rate | Each one is a call whose outcome nobody wrote down. A burst means the trunk or the pool, not one channel |
| `ivr_fail_closed_total` | Any non-zero with reason `CONFIG_PROVIDER_UNAVAILABLE` | The flag store is unreadable and every dial decision is degrading to the safe default. Dialling has already stopped; this says why |
| `ivr_missed_deadline_total` | Any non-zero during a session | Confirmation windows closing unattempted — capacity, not correctness |
| `ivr_call_attempts_total` vs `ivr_call_results_total` | A widening gap | Calls starting and not finishing |

Per-channel auto-disable fires at **3 failures inside a 10-minute window**
(`SimChannelFailurePolicy.cs:10`), which puts the channel in `HEALTH_FAILED` instead of
`QUARANTINED`. `ivr_channel_quarantines_total` gives the fleet-wide view that per-channel policy
cannot.

## 4. A call whose state nobody knows

This is the case the whole subsystem is shaped around, and it resolves into an audited review queue
rather than a guess.

When a lease expires with a call still attached, the scheduler's own maintenance pass
(`QuarantineExpiredLeasesAsync`, every pass, before anything is claimed) does all of this in one
transaction:

- the channel → `QUARANTINED`, or `HEALTH_FAILED` if it has now failed 3 times in 10 minutes
- `quarantine_until` → now + `RecoveryQuarantineSeconds` (default **600s**)
- the fencing generation increments, so a worker from the old generation cannot act on it
- the job → `HELD_ADMIN_REVIEW` / `HELD_LEASE_RECOVERY`
- the attempt → `RECOVERY_REQUIRED`, `blocked_reason = LEASE_EXPIRED_RECONCILIATION_REQUIRED`
- an `SIM_CHANNEL_LEASE_QUARANTINED` row lands in the audit log with the fail count and generation

**Nothing recovers automatically past this point, and that is the design.** The attempt sits in
`RECOVERY_REQUIRED` until a person accounts for it.

To account for one:

```sql
SELECT a.ivr_call_attempt_id, a.ivr_call_job_id, a.status, a.blocked_reason,
       a.sim_channel_id, a.scheduled_at
FROM ivr_call_attempts a
WHERE a.status = 'RECOVERY_REQUIRED'
ORDER BY a.scheduled_at;
```

Then ask Asterisk what actually happened — `GET /ari/channels` — before deciding anything. The
database knows a lease expired. Only Asterisk knows whether the customer was still on the line.

**Do not mark an attempt answered or unanswered to clear the queue.** An attempt whose outcome is
genuinely unknown goes to admin review as unknown. The whole reason the row is blocked rather than
defaulted is that both defaults are wrong: "answered" invents a confirmation the customer never gave,
and "unanswered" licenses a second call to someone who already took the first.

The recovery path is `POST /admin/technical-retries` and `POST /admin/admin-reviews`, both of which
require an actor, a correlation id and an idempotency key, and both of which write audit rows. Use
them rather than SQL, so that the decision has a name attached.

## 5. Stopping

**Two controls, two different jobs.** Reaching for the wrong one during an incident costs minutes
that matter.

| Control | What it does | What it does **not** do |
| --- | --- | --- |
| `POST /admin/feature-flags` → `globalDialKillSwitch: true` | Stops the **next** call | Does not touch calls already up |
| `POST /admin/call-jobs:terminate-all` | Ends conversations **already happening** | Does not stop new ones being started |
| `POST /admin/call-jobs/{id}:terminate` | Ends one specific call | |

A full stop is both, kill switch first. They are separate routes, separate presses and separate
reasons on purpose.

### How fast

| Step | Bound | Where it comes from |
| --- | --- | --- |
| Kill switch → next dial decision | **The very next call** | `DispatchGate` reads the flag with `forceFresh: true` (`DispatchGate.cs:26`), so the 15-second snapshot cache is bypassed on this path |
| Worst case before a call would have started anyway | ~1 poll interval (**1s** at default) | `PollIntervalMilliseconds` |
| `:terminate-all` → a live call hangs up | **≤ 500ms** | `TerminationPollMilliseconds`, floored at 200ms (`AsteriskSchedulerDispatchGateway.cs:233`) |
| Pod shutdown waits for in-flight calls | **180s** | `DispatchDrainSeconds`; `terminationGracePeriodSeconds: 210` in `deploy/helm/ivr/values.yaml:45` is set above it so a drain is never SIGKILLed |

The 15-second flag cache is worth knowing about precisely because it does **not** apply here. Other
readers of the snapshot may be up to 15s stale; the dispatch gate never is. If you are reading a flag
value on a dashboard and it disagrees with what the dialler is doing, the dashboard is the stale one.

**These are derived from configuration and pinned by tests, not measured against a carrier.** The
figure nobody has is how long the carrier itself takes to tear a channel down after we hang up, which
is what determines when the channel is genuinely free for the next call. Ask for it in writing, and
measure it during the first load session.

### Restarting after a stop

Releasing the kill switch is treated as a **privilege increase** by
`FeatureFlagGuardrails.cs:143` and needs the corresponding approval, the same as any other widening.
It is not symmetric with engaging it, and that asymmetry is deliberate.

## What not to do

- **Do not raise `MaxConcurrentDispatches` to match the contract without adding `ivr_sim_channels`
  rows.** The ceiling silently stays at the row count, and the trunk you are paying for sits idle
  while everything reports healthy.
- **Do not set `MaxCallStartsPerSecond` equal to `ContractedChannels` because the validator allows
  it.** It allows it; the carrier may not. Use the carrier's number.
- **Do not treat `may_dial: false` as an incident.** Read [the ownership runbook](ari-controller-ownership.md).
- **Do not clear `RECOVERY_REQUIRED` rows in SQL to unblock a queue.** They are the only record that
  a call's outcome is unknown, and an unknown outcome is a customer who may or may not have confirmed
  an order.
- **Do not use `:terminate-all` as a kill switch.** It ends the calls that are up and lets the next
  batch start behind it.
- **Do not add a `DestinationAlias` to the production profile** to make a test dial easier. Startup
  will refuse it, and the refusal is the feature.

## What this page cannot tell you yet

Honest list of what is missing, so nobody mistakes this document for a completed acceptance:

| Missing | Why | Unblocked by |
| --- | --- | --- |
| Real values for every carrier-supplied key in §1 | No contract, no integration document | Signing with a carrier |
| Whether `DtmfMode` is right | Only a real call proves it. A wrong value loses keypresses silently | First real call |
| Measured channel teardown time | Determines when a slot is genuinely free | First load session |
| Alert thresholds from real traffic | The figures in §3 are starting points | First golden hour |
| A procedure for returning a quarantined production channel to service | The admin API refuses it (§2) | A decision on whether that restriction is intended |
| A gate that catches the three channel numbers disagreeing | Does not exist | — |

The first four need a carrier. The last two do not, and are the more embarrassing ones.

# ARI controller ownership — reading it, and taking it back

One worker at a time may dial on one Asterisk application. This page is for the person who has to
decide what to do when that worker is not the one you expected, or is not there at all.

It exists because two of the four states need a human, and the action that human takes is the most
dangerous one in this subsystem. Everything below assumes you have read that sentence.

## Why this is manual

Asterisk hands an application to whichever WebSocket connected most recently and tells the loser
afterwards, with `ApplicationReplaced`. It cannot be asked to refuse. So two workers that both
believe they should be dialling do not collide on a lock — one silently stops receiving events for
calls it thinks it is running, while the other starts new ones, on the same trunk, to the same
customers.

`ivr_ari_controller_ownership` is how each worker decides whether to claim work. It is a condition
a worker checks on itself, **not a fence Asterisk enforces**. That is why a stale lease does not
hand ownership over automatically: a node that lost its database connection but kept its ARI socket
reports exactly like a node that died, and the difference is the difference between one dialler and
two.

There is no button for this in V1. That is deliberate — the deployment profile is a single replica,
and a considered action against the database suits the risk better than something that can be
clicked by accident.

## Reading the state

`GET /healthz` on the worker (port `8081` by default) carries the last answer the scheduler got:

```json
{
  "status": "live",
  "loops": [ "..." ],
  "ari_controller": {
    "scope": "LAB_REAL_SIM:lab:ivr-lab",
    "status": "AwaitingIsolation",
    "may_dial": false,
    "fencing_generation": 7,
    "observed_at": "2026-09-15T09:00:00+00:00"
  }
}
```

`ari_controller` is absent until the first scheduler pass answers, and a worker with the scheduler
switched off never has one. It is reported in the body and **never** in the status code: a worker
that may not dial is behaving as designed, and failing its probe would restart it, which cannot
grant it the application and would drop whatever calls it is still draining.

| `status` | What it means | Who acts |
| --- | --- | --- |
| `Held` | This worker owns the application and may dial. | Nobody |
| `HeldByAnotherWorker` | Another worker owns it and is still saying so. | Nobody — this is a healthy second replica waiting, or a deploy in progress |
| `AwaitingIsolation` | The holder stopped renewing. **Not** proof it stopped. | You, after the checks below |
| `AwaitingReconciliation` | This worker took the scope by force and has not accounted for the previous generation's calls. | You, after the checks below |

The same transitions are logged with `EventId 2319`.

## Before you isolate anything

**`AwaitingIsolation` is not an instruction. It means a lease expired, which is exactly the fact
that does not tell you whether the old controller is gone.** Isolating on the strength of that
status alone is how you end up with two controllers dialling.

Work through all three. If you cannot complete one, stop and escalate rather than guessing.

1. **Confirm the holding process is gone.** Read `owner_worker_id` from the row, then confirm that
   process no longer exists — the pod is deleted and not `Terminating`, or the node it was on is
   gone and not merely unreachable. An unreachable node is the case this whole design is about: it
   can still be running a worker that can still reach Asterisk.

2. **Look at what Asterisk currently has.** `GET /ari/channels` on the Asterisk this scope belongs
   to lists the channels that exist right now. Channels belonging to the old generation mean calls
   are still up regardless of what the database says about leases.

3. **Look at what the database thinks is in flight.** Attempts in `LEASED_PENDING_DISPATCH`,
   `DIALING` or `ACTIVE_CALL` are attempts nobody has written a result for.

```sql
SELECT controller_scope, state, owner_worker_id, fencing_generation,
       heartbeat_at, lease_expires_at, requires_reconciliation
FROM ivr_ari_controller_ownership
WHERE controller_scope = :scope;

SELECT ivr_call_attempt_id, ivr_call_job_id, status, sim_channel_id
FROM ivr_call_attempts
WHERE status IN ('LEASED_PENDING_DISPATCH', 'DIALING', 'ACTIVE_CALL');
```

If a deploy is in progress, none of this applies: wait. A worker draining a long call holds its
lease for `ControllerLeaseSeconds` (240s by default) and releases the scope cleanly when it
finishes, which needs nobody. `terminationGracePeriodSeconds` is 210s, so give it at least that
long before treating a shutdown as a failure.

## Isolating

Only after all three checks. `isolated_by_actor_id` and `isolated_at` are **required by the
database** — an isolation cannot be anonymous, because this row is the only record of who decided
to take the dialler away from a controller that may still have had customers on the line.

```sql
UPDATE ivr_ari_controller_ownership
SET state = 'ISOLATED',
    isolated_at = now(),
    isolated_by_actor_id = :your_actor_id,
    isolated_reason = :what_you_verified,
    lease_expires_at = NULL
WHERE controller_scope = :scope
  AND state = 'HELD';
```

Put what you actually checked in `isolated_reason` — "pod deleted, node drained, no channels on
ARI" — not "stale lease". The next person reading this row needs to know what was verified, not
what the status said.

The worker that was isolated can never take the scope back: its id stays in `owner_worker_id` and
is refused. This is on purpose. It is the process most likely to ask, because it never noticed
anything was wrong.

## After a forced handover: reconciling

The replacement worker will take the scope and report `AwaitingReconciliation`. It owns the
application and **still may not dial**. Owning and being allowed to dial are separate questions,
and the second one is still open: calls from the previous generation are unaccounted for.

Account for them before clearing the flag:

1. Every attempt from the list above has either ended or is still up on Asterisk. Check `GET
   /ari/channels` again.
2. Attempts whose outcome is genuinely unknown go to review rather than being guessed at. Do not
   mark a call answered or unanswered to tidy the list.
3. Channels still reserved for a dead generation are released by the scheduler's own quarantine
   pass (`RecoveryQuarantineSeconds`, 600s by default) — you do not need to force them.

```sql
UPDATE ivr_ari_controller_ownership
SET requires_reconciliation = FALSE,
    reconciled_at = now(),
    reconciled_by_actor_id = :your_actor_id
WHERE controller_scope = :scope
  AND state = 'HELD';
```

The next scheduler pass will report `Held` and dialling resumes.

## What not to do

- **Do not isolate because the status says `AwaitingIsolation`.** That status is the question, not
  the answer.
- **Do not clear `requires_reconciliation` to get the queue moving.** It is the only thing standing
  between a forced handover and a second set of calls to customers whose first call nobody has
  accounted for.
- **Do not scale the worker up.** Extra replicas do not add capacity on an Asterisk route; they
  start, find the scope held, and wait. Throughput is bounded by the channel pool, not pod count.
- **Do not delete or edit rows in this table to clean it up.** The history is the audit trail for
  who took a dialler away from whom.
- **Do not restart the waiting worker.** It is not stuck; it is doing the one correct thing
  available to it, and a restart loses the calls it may still be draining.

## What is not automated yet

Reconciliation is a person reading channels and deciding. SIP-06 replaces that with a reconciler
that reads the channels back from Asterisk and closes the attempts itself. Until then the steps
above are the procedure, and the audit rows are what make them reviewable.

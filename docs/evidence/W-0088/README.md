# W-0088 — Phase 1/2 state-machine liveness remediation

Status: `TESTS_PASS` (disposable PostgreSQL)

`E-04` through `E-07` were valid and are remediated:

- only an explicit open `ADMIN_QUEUE_PAUSE` is a global claim hold; job/program
  eligibility and scheduler incidents remain observable but do not freeze
  unrelated dispatch;
- expired quarantine is recovered to an eligible channel state before claim,
  while a still-active quarantine remains unavailable;
- the deadline sweeper closes `HELD_ADMIN_REVIEW` jobs as final non-customer
  attempts and creates the result/callback path instead of leaving silent work;
- every eligibility `TASK_HELD_ADMIN_REVIEW` transition creates an open
  `ivr_review_items` record in the same transaction.

Executable proof includes `IT-SCH-DEADLINE-09`, `IT-SCH-HOLD-10`, expired lease/
quarantine recovery, eligibility missing/unknown evidence review creation, and
admin pause/resume claim behavior. The final PostgreSQL lane passed `92/92`; the
full solution passed `281/281`.

Manual database updates are no longer the only recovery path. Production
incident policy, operator UAT and on-call runbook validation remain `NOT_RUN`.

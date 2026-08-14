# W-0090 — Phase 1/2 callback and admin atomicity remediation

Status: `TESTS_PASS` (local fake transport + disposable PostgreSQL)

Valid findings `E-09`, `E-12`, `E-15` and `E-16` are closed:

- technical retry compares against canonical
  `EligibilityDecisions.Eligible` (`ELIGIBLE_FOR_IVR`) and has a covered happy
  path plus kill-switch/allowlist fail-closed paths;
- unexpected callback transport exceptions become bounded transient results
  and release the half-open probe; cancellation is propagated; readiness
  reflects the half-open probe state;
- admin mutation, idempotency record and append-only audit commit in one
  serializable transaction; an injected idempotency snapshot failure proves
  the business mutation rolls back;
- callback completion is a lease-conditioned SQL update. A stale token changes
  zero rows, and concurrent completion from one lease permits exactly one
  winner without stale overwrite.

Focused proof includes `UnexpectedTransportFailureBecomesRetryAndReleasesHalfOpenProbe`,
`AdminMutationRollsBackWhenIdempotencySnapshotCannotCommit`, technical retry
success/fail-closed tests, and
`DeliveryCompletionUsesLeaseFencingAndCreatesAdminVisibleReview`.

Final full regression: `281/281 PASS`. Callback delivery remains disabled by
default; real Sales/auth/CDC and production retry behavior are `NOT_RUN`.

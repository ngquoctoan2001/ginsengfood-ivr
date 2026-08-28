-- OD-18 / W-0123 data preflight — READ ONLY.
--
-- W-0123 removed the IVR-side trusted skip and deliberately dropped nothing: the columns, the
-- persisted enum value and the SKIPPED lifecycle status all stay so historical rows keep reading
-- and a rollback to the previous image still works. That decision was made without being able to
-- see a target database, so it was made on the safe assumption rather than on a count.
--
-- This file is the count. Run it per environment; the numbers decide whether a later work item may
-- physically remove anything, and they are also the audit trail for saying it may not.
--
-- Every statement is a SELECT. Nothing here writes, and nothing here should ever be edited into
-- something that does — a preflight that mutates the thing it measures is not a preflight.
--
-- Related: plan/ivr-orther/W-0123-m3-authoritative-call-decision-cleanup-plan.md section 6,
--          docs/evidence/W-0123/README.md section 1.3 (gate recorded ENV_BLOCKED).

\echo '=== OD18_PREFLIGHT_BEGIN ==='

-- 1. Tasks that carry the retired decision. This is the number that matters most: non-zero means
--    real rows depend on the enum value staying in the check constraint.
SELECT 'tasks_with_retired_decision' AS metric,
       count(*) AS value
FROM ivr_confirmation_tasks
WHERE eligibility_decision = 'TASK_SKIPPED_TRUSTED_CUSTOMER';

-- 2. Call jobs carrying the retired decision.
SELECT 'jobs_with_retired_decision' AS metric,
       count(*) AS value
FROM ivr_call_jobs
WHERE eligibility_decision = 'TASK_SKIPPED_TRUSTED_CUSTOMER';

-- 3. Jobs sitting in the SKIPPED lifecycle state, counted SEPARATELY from 2 on purpose.
--    SKIPPED is a generic lifecycle value that outlived the trusted-skip branch and is still
--    exercised by the retention tests. Folding it into the number above would report unrelated
--    rows as trusted-skip history and argue against removing a value nothing depends on.
SELECT 'jobs_in_skipped_status' AS metric,
       count(*) AS value
FROM ivr_call_jobs
WHERE status = 'SKIPPED' OR queue_status = 'SKIPPED';

-- 4. Of those, how many are actually trusted-skip history. The difference between 3 and 4 is the
--    part of SKIPPED that has nothing to do with OD-15.
SELECT 'jobs_skipped_status_from_trusted_skip' AS metric,
       count(*) AS value
FROM ivr_call_jobs
WHERE (status = 'SKIPPED' OR queue_status = 'SKIPPED')
  AND eligibility_decision = 'TASK_SKIPPED_TRUSTED_CUSTOMER';

-- 5. Rows where Module 3 sent the deprecated veto field at all.
SELECT 'tasks_with_trusted_skip_allowed_sent' AS metric,
       count(*) AS value
FROM ivr_confirmation_tasks
WHERE trusted_skip_allowed IS NOT NULL;

-- 6. Rows where Module 3 sent the deprecated classification field.
SELECT 'tasks_with_customer_trust_status_sent' AS metric,
       count(*) AS value
FROM ivr_confirmation_tasks
WHERE customer_trust_status IS NOT NULL;

-- 7. The population the runtime counter measures, computed here over history instead of over live
--    traffic: tasks whose stored snapshot carries the retired skip evidence. This is the same
--    predicate as ivr_legacy_skip_candidate_total (W-0124 F1) — no veto, no risk flags, and Sales
--    saying it evaluated risk. If the counter reads zero after deploy but this reads non-zero,
--    Module 3 stopped sending the shape rather than never having sent it.
SELECT 'tasks_matching_retired_skip_shape' AS metric,
       count(*) AS value
FROM ivr_confirmation_tasks
--    Both json columns are already jsonb in the schema, so these are jsonb comparisons rather
--    than string ones: risk_flags_json = '[]' as text would never match a normalised '[]'::jsonb.
WHERE trusted_skip_allowed IS DISTINCT FROM false
  AND (risk_flags_json IS NULL
       OR risk_flags_json = '[]'::jsonb
       OR risk_flags_json = 'null'::jsonb)
  AND (eligibility_snapshot_json #> '{trust,risk_evidence_available}') = 'true'::jsonb;

-- 8. Oldest and newest retired-decision row, so a retention window can be reasoned about rather
--    than guessed. Empty result means metric 1 is zero.
SELECT 'retired_decision_first_seen' AS metric,
       min(created_at)::text AS value
FROM ivr_confirmation_tasks
WHERE eligibility_decision = 'TASK_SKIPPED_TRUSTED_CUSTOMER';

SELECT 'retired_decision_last_seen' AS metric,
       max(created_at)::text AS value
FROM ivr_confirmation_tasks
WHERE eligibility_decision = 'TASK_SKIPPED_TRUSTED_CUSTOMER';

\echo '=== OD18_PREFLIGHT_END ==='

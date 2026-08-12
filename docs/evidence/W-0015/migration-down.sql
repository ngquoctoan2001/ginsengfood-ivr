START TRANSACTION;
DROP TRIGGER IF EXISTS trg_ivr_audit_log_append_only ON ivr_audit_log;
DROP TRIGGER IF EXISTS trg_ivr_result_callbacks_payload_immutable ON ivr_result_callbacks;
DROP TRIGGER IF EXISTS trg_ivr_call_attempts_snapshot ON ivr_call_attempts;
DROP TRIGGER IF EXISTS trg_ivr_call_jobs_policy_snapshot_immutable ON ivr_call_jobs;
DROP TRIGGER IF EXISTS trg_ivr_attempt_policies_immutable ON ivr_attempt_policies;
DROP TRIGGER IF EXISTS trg_ivr_confirmation_tasks_snapshot_immutable ON ivr_confirmation_tasks;
DROP FUNCTION IF EXISTS ivr_reject_audit_mutation();
DROP FUNCTION IF EXISTS ivr_enforce_callback_payload_immutable();
DROP FUNCTION IF EXISTS ivr_enforce_attempt_snapshot();
DROP FUNCTION IF EXISTS ivr_enforce_call_job_policy_snapshot_immutable();
DROP FUNCTION IF EXISTS ivr_reject_attempt_policy_update();
DROP FUNCTION IF EXISTS ivr_enforce_confirmation_task_snapshot_immutable();

DROP TABLE ivr_admin_actions;

DROP TABLE ivr_attempt_policies;

DROP TABLE ivr_audit_log;

DROP TABLE ivr_capacity_incidents;

DROP TABLE ivr_evidence;

DROP TABLE ivr_evidence_links;

DROP TABLE ivr_feature_flags;

DROP TABLE ivr_idempotency_keys;

DROP TABLE ivr_raw_call_events;

DROP TABLE ivr_result_callbacks;

DROP TABLE ivr_review_items;

DROP TABLE ivr_sim_channels;

DROP TABLE ivr_technical_exceptions;

DROP TABLE ivr_call_results;

DROP TABLE ivr_call_attempts;

DROP TABLE ivr_call_jobs;

DROP TABLE ivr_confirmation_tasks;

DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260812142435_P1_2_InitialTargetV1Persistence';

COMMIT;

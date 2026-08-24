using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0115. Closes the storage vocabulary for enum-like columns whose writers already use a finite
/// set. The preflight names every offending field before any constraint changes the schema.
/// </summary>
public partial class W0115ClosedEnumChecks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            """
                DO $w0115$
                DECLARE
                    violations text;
                BEGIN
                    SELECT string_agg(field || '=[' || invalid_values || ']', '; ' ORDER BY field)
                    INTO violations
                    FROM (
                        SELECT 'ivr_confirmation_tasks.eligibility_decision' AS field,
                               string_agg(DISTINCT eligibility_decision, ',' ORDER BY eligibility_decision) AS invalid_values
                        FROM ivr_confirmation_tasks
                        WHERE eligibility_decision IS NOT NULL
                          AND eligibility_decision NOT IN ('PENDING_ELIGIBILITY','ELIGIBLE_FOR_IVR','TASK_BLOCKED_OPERATIONAL','TASK_HELD_ADMIN_REVIEW','TASK_SKIPPED_TRUSTED_CUSTOMER','IVR_CAPACITY_EXCEPTION')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_call_jobs.status', string_agg(DISTINCT status, ',' ORDER BY status)
                        FROM ivr_call_jobs
                        WHERE status NOT IN ('CREATED','DRY_RUN','OPEN','QUEUED','READY_FOR_SCHEDULER','LEASED','LEASED_PENDING_DISPATCH','DISPATCH_LEASED','DIALING','ACTIVE_CALL','DISPOSITION_PENDING_NORMALIZATION','PROVIDER_EVENT_PENDING_NORMALIZATION','RESULT_READY_FOR_CALLBACK','TECHNICAL_RETRY_QUEUED','HELD_MOCK','HELD_ADMIN_REVIEW','HELD_ELIGIBILITY','HELD_CAPACITY','HELD_CALLBACK','HELD_TECHNICAL_REVIEW','HELD_NORMALIZATION','HELD_LEASE_RECOVERY','CAPACITY_HELD','CAPACITY_MISSED','CLOSED_CAPACITY','RECOVERY_REQUIRED','BLOCKED','SKIPPED','CLOSED')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_call_jobs.queue_status', string_agg(DISTINCT queue_status, ',' ORDER BY queue_status)
                        FROM ivr_call_jobs
                        WHERE queue_status NOT IN ('QUEUED','HELD_MOCK','HELD_ELIGIBILITY','LEASED','HELD_LEASE_RECOVERY','HELD_NORMALIZATION','HELD_CALLBACK','HELD_TECHNICAL_REVIEW','HELD_CAPACITY','HELD_ADMIN_REVIEW','SKIPPED','BLOCKED','CLOSED_CAPACITY')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_call_jobs.eligibility_decision', string_agg(DISTINCT eligibility_decision, ',' ORDER BY eligibility_decision)
                        FROM ivr_call_jobs
                        WHERE eligibility_decision NOT IN ('PENDING_ELIGIBILITY','ELIGIBLE_FOR_IVR','TASK_BLOCKED_OPERATIONAL','TASK_HELD_ADMIN_REVIEW','TASK_SKIPPED_TRUSTED_CUSTOMER','IVR_CAPACITY_EXCEPTION')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_call_attempts.status', string_agg(DISTINCT status, ',' ORDER BY status)
                        FROM ivr_call_attempts
                        WHERE status NOT IN ('LEASED_PENDING_DISPATCH','DIALING','ACTIVE_CALL','PROVIDER_EVENT_PENDING_NORMALIZATION','NORMALIZED_ATTEMPT_COMPLETE','NORMALIZED_FINAL','NORMALIZED_TECHNICAL_RETRY','NORMALIZED_REVIEW_REQUIRED','TECHNICAL_RETRY_QUEUED','RECOVERY_REQUIRED')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_call_attempts.result_status', string_agg(DISTINCT result_status, ',' ORDER BY result_status)
                        FROM ivr_call_attempts
                        WHERE result_status IS NOT NULL
                          AND result_status NOT IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL','IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL','IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION','IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_call_results.result_type', string_agg(DISTINCT result_type, ',' ORDER BY result_type)
                        FROM ivr_call_results
                        WHERE result_type NOT IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL','IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL','IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION','IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_call_results.recommended_core_action', string_agg(DISTINCT recommended_core_action, ',' ORDER BY recommended_core_action)
                        FROM ivr_call_results
                        WHERE recommended_core_action NOT IN ('REVALIDATE_AND_CONFIRM_ORDER','REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST','NO_STATE_CHANGE_WAIT_FOR_TIMEOUT','REVALIDATE_AND_EXPIRE_CONFIRMATION','REVALIDATE_AND_HOLD_ADMIN_REVIEW','IGNORE_STALE_CALLBACK','BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_call_results.final_result_status=result_type',
                               string_agg(DISTINCT final_result_status || '!=' || result_type, ',' ORDER BY final_result_status || '!=' || result_type)
                        FROM ivr_call_results
                        WHERE final_result_status <> result_type
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_result_callbacks.result_status', string_agg(DISTINCT result_status, ',' ORDER BY result_status)
                        FROM ivr_result_callbacks
                        WHERE result_status NOT IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL','IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL','IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION','IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_result_callbacks.result_state', string_agg(DISTINCT result_state, ',' ORDER BY result_state)
                        FROM ivr_result_callbacks
                        WHERE result_state NOT IN ('PENDING_CORE_REVALIDATION')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_result_callbacks.delivery_status', string_agg(DISTINCT delivery_status, ',' ORDER BY delivery_status)
                        FROM ivr_result_callbacks
                        WHERE delivery_status NOT IN ('READY','SENDING','RETRY_PENDING','RETRY_EXHAUSTED','DELIVERED_ACCEPTED','DELIVERED_BLOCKED','DELIVERED_REVIEW','REJECTED_STALE','IDEMPOTENCY_CONFLICT','INVALID_DEAD_LETTER','AUTH_REJECTED')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_sim_channels.status', string_agg(DISTINCT status, ',' ORDER BY status)
                        FROM ivr_sim_channels
                        WHERE status NOT IN ('IDLE','RESERVED','LEASED','DIALING','ACTIVE_CALL','DISABLED','QUARANTINED','HEALTH_FAILED')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_capacity_incidents.status', string_agg(DISTINCT status, ',' ORDER BY status)
                        FROM ivr_capacity_incidents
                        WHERE status NOT IN ('OPEN','RESOLVED')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_capacity_incidents.scope', string_agg(DISTINCT scope, ',' ORDER BY scope)
                        FROM ivr_capacity_incidents
                        WHERE scope NOT IN ('ADMIN_QUEUE_PAUSE','ELIGIBILITY_DEADLINE','SCHEDULER_DEADLINE')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_review_items.source_type', string_agg(DISTINCT source_type, ',' ORDER BY source_type)
                        FROM ivr_review_items
                        WHERE source_type NOT IN ('IVR_CALL_RESULT','IVR_RESULT_CALLBACK','ELIGIBILITY_DECISION','IVR_OPTOUT_PROPOSAL')
                        HAVING count(*) > 0
                        UNION ALL
                        SELECT 'ivr_review_items.status', string_agg(DISTINCT status, ',' ORDER BY status)
                        FROM ivr_review_items
                        WHERE status NOT IN ('OPEN','RESOLVED','PENDING_CRM','ACCEPTED_BY_CRM')
                        HAVING count(*) > 0
                    ) invalid;

                    IF violations IS NOT NULL THEN
                        RAISE EXCEPTION USING
                            ERRCODE = '23514',
                            MESSAGE = 'W-0115 enum preflight blocked: ' || violations;
                    END IF;
                END
                $w0115$;
                """);

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_sim_channels_status",
            table: "ivr_sim_channels",
            sql: "status IN ('IDLE','RESERVED','LEASED','DIALING','ACTIVE_CALL','DISABLED','QUARANTINED','HEALTH_FAILED')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_review_items_source_type",
            table: "ivr_review_items",
            sql: "source_type IN ('IVR_CALL_RESULT','IVR_RESULT_CALLBACK','ELIGIBILITY_DECISION','IVR_OPTOUT_PROPOSAL')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_review_items_status",
            table: "ivr_review_items",
            sql: "status IN ('OPEN','RESOLVED','PENDING_CRM','ACCEPTED_BY_CRM')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_result_callbacks_delivery_status",
            table: "ivr_result_callbacks",
            sql: "delivery_status IN ('READY','SENDING','RETRY_PENDING','RETRY_EXHAUSTED','DELIVERED_ACCEPTED','DELIVERED_BLOCKED','DELIVERED_REVIEW','REJECTED_STALE','IDEMPOTENCY_CONFLICT','INVALID_DEAD_LETTER','AUTH_REJECTED')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_result_callbacks_result_state",
            table: "ivr_result_callbacks",
            sql: "result_state IN ('PENDING_CORE_REVALIDATION')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_result_callbacks_result_status",
            table: "ivr_result_callbacks",
            sql: "result_status IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL','IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL','IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION','IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_confirmation_tasks_eligibility_decision",
            table: "ivr_confirmation_tasks",
            sql: "eligibility_decision IS NULL OR eligibility_decision IN ('PENDING_ELIGIBILITY','ELIGIBLE_FOR_IVR','TASK_BLOCKED_OPERATIONAL','TASK_HELD_ADMIN_REVIEW','TASK_SKIPPED_TRUSTED_CUSTOMER','IVR_CAPACITY_EXCEPTION')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_capacity_incidents_scope",
            table: "ivr_capacity_incidents",
            sql: "scope IN ('ADMIN_QUEUE_PAUSE','ELIGIBILITY_DEADLINE','SCHEDULER_DEADLINE')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_capacity_incidents_status",
            table: "ivr_capacity_incidents",
            sql: "status IN ('OPEN','RESOLVED')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_final_matches_type",
            table: "ivr_call_results",
            sql: "final_result_status = result_type");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_recommended_core_action",
            table: "ivr_call_results",
            sql: "recommended_core_action IN ('REVALIDATE_AND_CONFIRM_ORDER','REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST','NO_STATE_CHANGE_WAIT_FOR_TIMEOUT','REVALIDATE_AND_EXPIRE_CONFIRMATION','REVALIDATE_AND_HOLD_ADMIN_REVIEW','IGNORE_STALE_CALLBACK','BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_result_type",
            table: "ivr_call_results",
            sql: "result_type IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL','IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL','IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION','IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_jobs_eligibility_decision",
            table: "ivr_call_jobs",
            sql: "eligibility_decision IN ('PENDING_ELIGIBILITY','ELIGIBLE_FOR_IVR','TASK_BLOCKED_OPERATIONAL','TASK_HELD_ADMIN_REVIEW','TASK_SKIPPED_TRUSTED_CUSTOMER','IVR_CAPACITY_EXCEPTION')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_jobs_queue_status",
            table: "ivr_call_jobs",
            sql: "queue_status IN ('QUEUED','HELD_MOCK','HELD_ELIGIBILITY','LEASED','HELD_LEASE_RECOVERY','HELD_NORMALIZATION','HELD_CALLBACK','HELD_TECHNICAL_REVIEW','HELD_CAPACITY','HELD_ADMIN_REVIEW','SKIPPED','BLOCKED','CLOSED_CAPACITY')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_jobs_status",
            table: "ivr_call_jobs",
            sql: "status IN ('CREATED','DRY_RUN','OPEN','QUEUED','READY_FOR_SCHEDULER','LEASED','LEASED_PENDING_DISPATCH','DISPATCH_LEASED','DIALING','ACTIVE_CALL','DISPOSITION_PENDING_NORMALIZATION','PROVIDER_EVENT_PENDING_NORMALIZATION','RESULT_READY_FOR_CALLBACK','TECHNICAL_RETRY_QUEUED','HELD_MOCK','HELD_ADMIN_REVIEW','HELD_ELIGIBILITY','HELD_CAPACITY','HELD_CALLBACK','HELD_TECHNICAL_REVIEW','HELD_NORMALIZATION','HELD_LEASE_RECOVERY','CAPACITY_HELD','CAPACITY_MISSED','CLOSED_CAPACITY','RECOVERY_REQUIRED','BLOCKED','SKIPPED','CLOSED')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_result_status",
            table: "ivr_call_attempts",
            sql: "result_status IS NULL OR result_status IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL','IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL','IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION','IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_status",
            table: "ivr_call_attempts",
            sql: "status IN ('LEASED_PENDING_DISPATCH','DIALING','ACTIVE_CALL','PROVIDER_EVENT_PENDING_NORMALIZATION','NORMALIZED_ATTEMPT_COMPLETE','NORMALIZED_FINAL','NORMALIZED_TECHNICAL_RETRY','NORMALIZED_REVIEW_REQUIRED','TECHNICAL_RETRY_QUEUED','RECOVERY_REQUIRED')");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_sim_channels_status",
            table: "ivr_sim_channels");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_review_items_source_type",
            table: "ivr_review_items");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_review_items_status",
            table: "ivr_review_items");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_result_callbacks_delivery_status",
            table: "ivr_result_callbacks");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_result_callbacks_result_state",
            table: "ivr_result_callbacks");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_result_callbacks_result_status",
            table: "ivr_result_callbacks");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_confirmation_tasks_eligibility_decision",
            table: "ivr_confirmation_tasks");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_capacity_incidents_scope",
            table: "ivr_capacity_incidents");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_capacity_incidents_status",
            table: "ivr_capacity_incidents");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_final_matches_type",
            table: "ivr_call_results");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_recommended_core_action",
            table: "ivr_call_results");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_result_type",
            table: "ivr_call_results");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_jobs_eligibility_decision",
            table: "ivr_call_jobs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_jobs_queue_status",
            table: "ivr_call_jobs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_jobs_status",
            table: "ivr_call_jobs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_result_status",
            table: "ivr_call_attempts");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_status",
            table: "ivr_call_attempts");
    }
}

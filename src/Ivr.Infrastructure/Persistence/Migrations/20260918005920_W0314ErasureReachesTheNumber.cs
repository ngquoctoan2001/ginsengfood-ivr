using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0314, lot 2 of the 17/09 remediation plan. The confirmation-task trigger learns about
/// <c>phone_e164</c>, and the rows erased before this migration lose what the erasure left behind.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why the trigger.</b> <c>W0310PhoneE164FromModule3</c> added the customer's number in the clear
/// and the snapshot trigger was never told. Two things followed: the dial target of an accepted task
/// could be changed by any UPDATE, although every other part of the snapshot is held; and an erasure
/// could stamp <c>anonymized_at</c> while the number stayed where it was, so the row looked erased
/// and was not. The function below is the one <c>P2_1_TaskIntake</c> installs in its <c>Up</c> - the
/// 17/09 plan pointed at the copy in its <c>Down</c>, which predates the four script and policy
/// columns and would have quietly stopped holding them - with two additions: <c>phone_e164</c> joins
/// the held columns, and the anonymisation branch requires it to be NULL.
/// </para>
/// <para>
/// <b>Why the backfill runs first.</b> Rows already stamped <c>anonymized_at</c> cannot be erased
/// again: the trigger refuses a second stamp, and <c>DsarService</c> now skips erased rows. So this
/// migration clears them itself, while the old trigger - which does not list these columns - still
/// lets the UPDATE through. It clears exactly what an erasure removes since W-0314: the number and
/// the three Sales values the owner decided on 18/09. <c>customer_id</c> stays, as it does in an
/// erasure. Live rows are not touched.
/// </para>
/// <para>
/// <b>Rollout.</b> A pod still running the old erasure statement is refused on a row that holds a
/// number, and nothing is marked erased that was not. Do not run a DSAR erasure while old and new
/// pods overlap; <c>docs/compliance/dsar-runbook.md</c> says so.
/// </para>
/// <para>
/// <b>Expand only.</b> No ALTER TABLE: a function body and an UPDATE, both of which the previous
/// release's code can run beside. <c>Down</c> restores the previous function and does not put any
/// number back - those rows were marked erased, and a number on them was the defect.
/// </para>
/// </remarks>
public partial class W0314ErasureReachesTheNumber : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.Sql(
            """
            UPDATE ivr_confirmation_tasks
            SET phone_e164 = NULL,
                official_contact_id = NULL,
                customer_trust_status = NULL,
                trusted_skip_allowed = NULL
            WHERE anonymized_at IS NOT NULL
              AND (phone_e164 IS NOT NULL
                   OR official_contact_id IS NOT NULL
                   OR customer_trust_status IS NOT NULL
                   OR trusted_skip_allowed IS NOT NULL);
            """);

        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION ivr_enforce_confirmation_task_snapshot_immutable()
            RETURNS trigger
            LANGUAGE plpgsql
            AS $function$
            BEGIN
                IF NEW.contract_version IS DISTINCT FROM OLD.contract_version
                   OR NEW.official_order_id IS DISTINCT FROM OLD.official_order_id
                   OR NEW.order_version IS DISTINCT FROM OLD.order_version
                   OR NEW.order_state IS DISTINCT FROM OLD.order_state
                   OR NEW.payment_method_snapshot IS DISTINCT FROM OLD.payment_method_snapshot
                   OR NEW.program_type IS DISTINCT FROM OLD.program_type
                   OR NEW.attempt_policy_version IS DISTINCT FROM OLD.attempt_policy_version
                   OR NEW.max_attempts IS DISTINCT FROM OLD.max_attempts
                   OR NEW.attempt_offsets_seconds_json IS DISTINCT FROM OLD.attempt_offsets_seconds_json
                   OR NEW.confirmation_window_started_at IS DISTINCT FROM OLD.confirmation_window_started_at
                   OR NEW.confirmation_window_expires_at IS DISTINCT FROM OLD.confirmation_window_expires_at
                   OR NEW.phone_ref IS DISTINCT FROM OLD.phone_ref
                   OR NEW.phone_masked IS DISTINCT FROM OLD.phone_masked
                   OR NEW.dial_token_ciphertext IS DISTINCT FROM OLD.dial_token_ciphertext
                   OR NEW.dial_token_expires_at IS DISTINCT FROM OLD.dial_token_expires_at
                   OR NEW.phone_e164 IS DISTINCT FROM OLD.phone_e164
                   OR NEW.privacy_safe_order_summary_json IS DISTINCT FROM OLD.privacy_safe_order_summary_json
                   OR NEW.call_script_template_id IS DISTINCT FROM OLD.call_script_template_id
                   OR NEW.call_script_version IS DISTINCT FROM OLD.call_script_version
                   OR NEW.evidence_policy_version IS DISTINCT FROM OLD.evidence_policy_version
                   OR NEW.privacy_policy_version IS DISTINCT FROM OLD.privacy_policy_version
                   OR NEW.anonymized_at IS DISTINCT FROM OLD.anonymized_at THEN
                    IF OLD.anonymized_at IS NULL
                       AND NEW.anonymized_at IS NOT NULL
                       AND NEW.phone_ref = 'redacted'
                       AND NEW.phone_masked = '***'
                       AND NEW.dial_token_ciphertext = 'enc:redacted'
                       AND NEW.phone_e164 IS NULL
                       AND NEW.privacy_safe_order_summary_json = '{}'::jsonb
                       AND NEW.contract_version IS NOT DISTINCT FROM OLD.contract_version
                       AND NEW.official_order_id IS NOT DISTINCT FROM OLD.official_order_id
                       AND NEW.order_version IS NOT DISTINCT FROM OLD.order_version
                       AND NEW.order_state IS NOT DISTINCT FROM OLD.order_state
                       AND NEW.payment_method_snapshot IS NOT DISTINCT FROM OLD.payment_method_snapshot
                       AND NEW.program_type IS NOT DISTINCT FROM OLD.program_type
                       AND NEW.attempt_policy_version IS NOT DISTINCT FROM OLD.attempt_policy_version
                       AND NEW.max_attempts IS NOT DISTINCT FROM OLD.max_attempts
                       AND NEW.attempt_offsets_seconds_json IS NOT DISTINCT FROM OLD.attempt_offsets_seconds_json
                       AND NEW.confirmation_window_started_at IS NOT DISTINCT FROM OLD.confirmation_window_started_at
                       AND NEW.confirmation_window_expires_at IS NOT DISTINCT FROM OLD.confirmation_window_expires_at
                       AND NEW.dial_token_expires_at IS NOT DISTINCT FROM OLD.dial_token_expires_at
                       AND NEW.call_script_template_id IS NOT DISTINCT FROM OLD.call_script_template_id
                       AND NEW.call_script_version IS NOT DISTINCT FROM OLD.call_script_version
                       AND NEW.evidence_policy_version IS NOT DISTINCT FROM OLD.evidence_policy_version
                       AND NEW.privacy_policy_version IS NOT DISTINCT FROM OLD.privacy_policy_version THEN
                        RETURN NEW;
                    END IF;

                    RAISE EXCEPTION 'confirmation-task contract/policy/speech snapshot is immutable';
                END IF;

                RETURN NEW;
            END;
            $function$;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // The function exactly as P2_1_TaskIntake's Up installs it. The backfill is not undone.
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION ivr_enforce_confirmation_task_snapshot_immutable()
            RETURNS trigger
            LANGUAGE plpgsql
            AS $function$
            BEGIN
                IF NEW.contract_version IS DISTINCT FROM OLD.contract_version
                   OR NEW.official_order_id IS DISTINCT FROM OLD.official_order_id
                   OR NEW.order_version IS DISTINCT FROM OLD.order_version
                   OR NEW.order_state IS DISTINCT FROM OLD.order_state
                   OR NEW.payment_method_snapshot IS DISTINCT FROM OLD.payment_method_snapshot
                   OR NEW.program_type IS DISTINCT FROM OLD.program_type
                   OR NEW.attempt_policy_version IS DISTINCT FROM OLD.attempt_policy_version
                   OR NEW.max_attempts IS DISTINCT FROM OLD.max_attempts
                   OR NEW.attempt_offsets_seconds_json IS DISTINCT FROM OLD.attempt_offsets_seconds_json
                   OR NEW.confirmation_window_started_at IS DISTINCT FROM OLD.confirmation_window_started_at
                   OR NEW.confirmation_window_expires_at IS DISTINCT FROM OLD.confirmation_window_expires_at
                   OR NEW.phone_ref IS DISTINCT FROM OLD.phone_ref
                   OR NEW.phone_masked IS DISTINCT FROM OLD.phone_masked
                   OR NEW.dial_token_ciphertext IS DISTINCT FROM OLD.dial_token_ciphertext
                   OR NEW.dial_token_expires_at IS DISTINCT FROM OLD.dial_token_expires_at
                   OR NEW.privacy_safe_order_summary_json IS DISTINCT FROM OLD.privacy_safe_order_summary_json
                   OR NEW.call_script_template_id IS DISTINCT FROM OLD.call_script_template_id
                   OR NEW.call_script_version IS DISTINCT FROM OLD.call_script_version
                   OR NEW.evidence_policy_version IS DISTINCT FROM OLD.evidence_policy_version
                   OR NEW.privacy_policy_version IS DISTINCT FROM OLD.privacy_policy_version
                   OR NEW.anonymized_at IS DISTINCT FROM OLD.anonymized_at THEN
                    IF OLD.anonymized_at IS NULL
                       AND NEW.anonymized_at IS NOT NULL
                       AND NEW.phone_ref = 'redacted'
                       AND NEW.phone_masked = '***'
                       AND NEW.dial_token_ciphertext = 'enc:redacted'
                       AND NEW.privacy_safe_order_summary_json = '{}'::jsonb
                       AND NEW.contract_version IS NOT DISTINCT FROM OLD.contract_version
                       AND NEW.official_order_id IS NOT DISTINCT FROM OLD.official_order_id
                       AND NEW.order_version IS NOT DISTINCT FROM OLD.order_version
                       AND NEW.order_state IS NOT DISTINCT FROM OLD.order_state
                       AND NEW.payment_method_snapshot IS NOT DISTINCT FROM OLD.payment_method_snapshot
                       AND NEW.program_type IS NOT DISTINCT FROM OLD.program_type
                       AND NEW.attempt_policy_version IS NOT DISTINCT FROM OLD.attempt_policy_version
                       AND NEW.max_attempts IS NOT DISTINCT FROM OLD.max_attempts
                       AND NEW.attempt_offsets_seconds_json IS NOT DISTINCT FROM OLD.attempt_offsets_seconds_json
                       AND NEW.confirmation_window_started_at IS NOT DISTINCT FROM OLD.confirmation_window_started_at
                       AND NEW.confirmation_window_expires_at IS NOT DISTINCT FROM OLD.confirmation_window_expires_at
                       AND NEW.dial_token_expires_at IS NOT DISTINCT FROM OLD.dial_token_expires_at
                       AND NEW.call_script_template_id IS NOT DISTINCT FROM OLD.call_script_template_id
                       AND NEW.call_script_version IS NOT DISTINCT FROM OLD.call_script_version
                       AND NEW.evidence_policy_version IS NOT DISTINCT FROM OLD.evidence_policy_version
                       AND NEW.privacy_policy_version IS NOT DISTINCT FROM OLD.privacy_policy_version THEN
                        RETURN NEW;
                    END IF;

                    RAISE EXCEPTION 'confirmation-task contract/policy/speech snapshot is immutable';
                END IF;

                RETURN NEW;
            END;
            $function$;
            """);
    }
}

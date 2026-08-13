using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861, IDE0161 // EF-generated migration shape.

namespace Ivr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P1_5_RetentionLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_technical_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_technical_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_sim_channels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_sim_channels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_review_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_review_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_result_callbacks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_result_callbacks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_raw_call_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_raw_call_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_idempotency_keys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_idempotency_keys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "accepted_at",
                table: "ivr_evidence_links",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_evidence_links",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "ivr_evidence_links",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_evidence_links",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "retain_until",
                table: "ivr_evidence_links",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "retention_class",
                table: "ivr_evidence_links",
                type: "text",
                nullable: false,
                defaultValue: "LEGAL_DECISION_PENDING");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "accepted_at",
                table: "ivr_evidence",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_evidence",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_evidence",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_confirmation_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_confirmation_tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_capacity_incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_capacity_incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_call_results",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_call_results",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_call_jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_call_jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_call_attempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_call_attempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_audit_log",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_audit_log",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_attempt_policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_attempt_policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anonymized_at",
                table: "ivr_admin_actions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "legal_hold_until",
                table: "ivr_admin_actions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ivr_retention_checkpoints",
                columns: table => new
                {
                    data_class = table.Column<string>(type: "text", nullable: false),
                    segment = table.Column<string>(type: "text", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    processed_count = table.Column<long>(type: "bigint", nullable: false),
                    first_not_configured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_run_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_retention_checkpoints", x => new { x.data_class, x.segment });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_technical_exceptions_anonymized_at",
                table: "ivr_technical_exceptions",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_technical_exceptions_legal_hold_until",
                table: "ivr_technical_exceptions",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_technical_exceptions_retain_until",
                table: "ivr_technical_exceptions",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_sim_channels_anonymized_at",
                table: "ivr_sim_channels",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_sim_channels_legal_hold_until",
                table: "ivr_sim_channels",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_sim_channels_retain_until",
                table: "ivr_sim_channels",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_review_items_anonymized_at",
                table: "ivr_review_items",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_review_items_legal_hold_until",
                table: "ivr_review_items",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_review_items_retain_until",
                table: "ivr_review_items",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_anonymized_at",
                table: "ivr_result_callbacks",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_legal_hold_until",
                table: "ivr_result_callbacks",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_retain_until",
                table: "ivr_result_callbacks",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_raw_call_events_anonymized_at",
                table: "ivr_raw_call_events",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_raw_call_events_legal_hold_until",
                table: "ivr_raw_call_events",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_raw_call_events_retain_until",
                table: "ivr_raw_call_events",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_idempotency_keys_anonymized_at",
                table: "ivr_idempotency_keys",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_idempotency_keys_legal_hold_until",
                table: "ivr_idempotency_keys",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_idempotency_keys_retain_until",
                table: "ivr_idempotency_keys",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_links_accepted_at",
                table: "ivr_evidence_links",
                column: "accepted_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_links_anonymized_at",
                table: "ivr_evidence_links",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_links_created_at",
                table: "ivr_evidence_links",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_links_legal_hold_until",
                table: "ivr_evidence_links",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_links_retain_until",
                table: "ivr_evidence_links",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_accepted_at",
                table: "ivr_evidence",
                column: "accepted_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_anonymized_at",
                table: "ivr_evidence",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_legal_hold_until",
                table: "ivr_evidence",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_retain_until",
                table: "ivr_evidence",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_anonymized_at",
                table: "ivr_confirmation_tasks",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_legal_hold_until",
                table: "ivr_confirmation_tasks",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_retain_until",
                table: "ivr_confirmation_tasks",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_capacity_incidents_anonymized_at",
                table: "ivr_capacity_incidents",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_capacity_incidents_legal_hold_until",
                table: "ivr_capacity_incidents",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_capacity_incidents_retain_until",
                table: "ivr_capacity_incidents",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_anonymized_at",
                table: "ivr_call_results",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_legal_hold_until",
                table: "ivr_call_results",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_retain_until",
                table: "ivr_call_results",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_anonymized_at",
                table: "ivr_call_jobs",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_legal_hold_until",
                table: "ivr_call_jobs",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_retain_until",
                table: "ivr_call_jobs",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_anonymized_at",
                table: "ivr_call_attempts",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_legal_hold_until",
                table: "ivr_call_attempts",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_retain_until",
                table: "ivr_call_attempts",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_audit_log_anonymized_at",
                table: "ivr_audit_log",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_audit_log_legal_hold_until",
                table: "ivr_audit_log",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_audit_log_retain_until",
                table: "ivr_audit_log",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_attempt_policies_anonymized_at",
                table: "ivr_attempt_policies",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_attempt_policies_legal_hold_until",
                table: "ivr_attempt_policies",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_attempt_policies_retain_until",
                table: "ivr_attempt_policies",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_admin_actions_anonymized_at",
                table: "ivr_admin_actions",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_admin_actions_legal_hold_until",
                table: "ivr_admin_actions",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_admin_actions_retain_until",
                table: "ivr_admin_actions",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_retention_checkpoints_first_not_configured_at",
                table: "ivr_retention_checkpoints",
                column: "first_not_configured_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_retention_checkpoints_run_id",
                table: "ivr_retention_checkpoints",
                column: "run_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_retention_checkpoints_status_updated_at",
                table: "ivr_retention_checkpoints",
                columns: new[] { "status", "updated_at" });

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
                           AND NEW.dial_token_expires_at IS NOT DISTINCT FROM OLD.dial_token_expires_at THEN
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
                       OR NEW.dial_token_ciphertext IS DISTINCT FROM OLD.dial_token_ciphertext
                       OR NEW.dial_token_expires_at IS DISTINCT FROM OLD.dial_token_expires_at
                       OR NEW.privacy_safe_order_summary_json IS DISTINCT FROM OLD.privacy_safe_order_summary_json THEN
                        RAISE EXCEPTION 'confirmation-task contract/policy/speech snapshot is immutable';
                    END IF;

                    RETURN NEW;
                END;
                $function$;
                """);

            migrationBuilder.DropTable(
                name: "ivr_retention_checkpoints");

            migrationBuilder.DropIndex(
                name: "IX_ivr_technical_exceptions_anonymized_at",
                table: "ivr_technical_exceptions");

            migrationBuilder.DropIndex(
                name: "IX_ivr_technical_exceptions_legal_hold_until",
                table: "ivr_technical_exceptions");

            migrationBuilder.DropIndex(
                name: "IX_ivr_technical_exceptions_retain_until",
                table: "ivr_technical_exceptions");

            migrationBuilder.DropIndex(
                name: "IX_ivr_sim_channels_anonymized_at",
                table: "ivr_sim_channels");

            migrationBuilder.DropIndex(
                name: "IX_ivr_sim_channels_legal_hold_until",
                table: "ivr_sim_channels");

            migrationBuilder.DropIndex(
                name: "IX_ivr_sim_channels_retain_until",
                table: "ivr_sim_channels");

            migrationBuilder.DropIndex(
                name: "IX_ivr_review_items_anonymized_at",
                table: "ivr_review_items");

            migrationBuilder.DropIndex(
                name: "IX_ivr_review_items_legal_hold_until",
                table: "ivr_review_items");

            migrationBuilder.DropIndex(
                name: "IX_ivr_review_items_retain_until",
                table: "ivr_review_items");

            migrationBuilder.DropIndex(
                name: "IX_ivr_result_callbacks_anonymized_at",
                table: "ivr_result_callbacks");

            migrationBuilder.DropIndex(
                name: "IX_ivr_result_callbacks_legal_hold_until",
                table: "ivr_result_callbacks");

            migrationBuilder.DropIndex(
                name: "IX_ivr_result_callbacks_retain_until",
                table: "ivr_result_callbacks");

            migrationBuilder.DropIndex(
                name: "IX_ivr_raw_call_events_anonymized_at",
                table: "ivr_raw_call_events");

            migrationBuilder.DropIndex(
                name: "IX_ivr_raw_call_events_legal_hold_until",
                table: "ivr_raw_call_events");

            migrationBuilder.DropIndex(
                name: "IX_ivr_raw_call_events_retain_until",
                table: "ivr_raw_call_events");

            migrationBuilder.DropIndex(
                name: "IX_ivr_idempotency_keys_anonymized_at",
                table: "ivr_idempotency_keys");

            migrationBuilder.DropIndex(
                name: "IX_ivr_idempotency_keys_legal_hold_until",
                table: "ivr_idempotency_keys");

            migrationBuilder.DropIndex(
                name: "IX_ivr_idempotency_keys_retain_until",
                table: "ivr_idempotency_keys");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_links_accepted_at",
                table: "ivr_evidence_links");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_links_anonymized_at",
                table: "ivr_evidence_links");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_links_created_at",
                table: "ivr_evidence_links");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_links_legal_hold_until",
                table: "ivr_evidence_links");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_links_retain_until",
                table: "ivr_evidence_links");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_accepted_at",
                table: "ivr_evidence");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_anonymized_at",
                table: "ivr_evidence");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_legal_hold_until",
                table: "ivr_evidence");

            migrationBuilder.DropIndex(
                name: "IX_ivr_evidence_retain_until",
                table: "ivr_evidence");

            migrationBuilder.DropIndex(
                name: "IX_ivr_confirmation_tasks_anonymized_at",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropIndex(
                name: "IX_ivr_confirmation_tasks_legal_hold_until",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropIndex(
                name: "IX_ivr_confirmation_tasks_retain_until",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropIndex(
                name: "IX_ivr_capacity_incidents_anonymized_at",
                table: "ivr_capacity_incidents");

            migrationBuilder.DropIndex(
                name: "IX_ivr_capacity_incidents_legal_hold_until",
                table: "ivr_capacity_incidents");

            migrationBuilder.DropIndex(
                name: "IX_ivr_capacity_incidents_retain_until",
                table: "ivr_capacity_incidents");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_results_anonymized_at",
                table: "ivr_call_results");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_results_legal_hold_until",
                table: "ivr_call_results");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_results_retain_until",
                table: "ivr_call_results");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_jobs_anonymized_at",
                table: "ivr_call_jobs");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_jobs_legal_hold_until",
                table: "ivr_call_jobs");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_jobs_retain_until",
                table: "ivr_call_jobs");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_attempts_anonymized_at",
                table: "ivr_call_attempts");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_attempts_legal_hold_until",
                table: "ivr_call_attempts");

            migrationBuilder.DropIndex(
                name: "IX_ivr_call_attempts_retain_until",
                table: "ivr_call_attempts");

            migrationBuilder.DropIndex(
                name: "IX_ivr_audit_log_anonymized_at",
                table: "ivr_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_ivr_audit_log_legal_hold_until",
                table: "ivr_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_ivr_audit_log_retain_until",
                table: "ivr_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_ivr_attempt_policies_anonymized_at",
                table: "ivr_attempt_policies");

            migrationBuilder.DropIndex(
                name: "IX_ivr_attempt_policies_legal_hold_until",
                table: "ivr_attempt_policies");

            migrationBuilder.DropIndex(
                name: "IX_ivr_attempt_policies_retain_until",
                table: "ivr_attempt_policies");

            migrationBuilder.DropIndex(
                name: "IX_ivr_admin_actions_anonymized_at",
                table: "ivr_admin_actions");

            migrationBuilder.DropIndex(
                name: "IX_ivr_admin_actions_legal_hold_until",
                table: "ivr_admin_actions");

            migrationBuilder.DropIndex(
                name: "IX_ivr_admin_actions_retain_until",
                table: "ivr_admin_actions");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_technical_exceptions");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_technical_exceptions");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_sim_channels");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_sim_channels");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_review_items");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_review_items");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_result_callbacks");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_result_callbacks");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_raw_call_events");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_raw_call_events");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_idempotency_keys");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_idempotency_keys");

            migrationBuilder.DropColumn(
                name: "accepted_at",
                table: "ivr_evidence_links");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_evidence_links");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "ivr_evidence_links");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_evidence_links");

            migrationBuilder.DropColumn(
                name: "retain_until",
                table: "ivr_evidence_links");

            migrationBuilder.DropColumn(
                name: "retention_class",
                table: "ivr_evidence_links");

            migrationBuilder.DropColumn(
                name: "accepted_at",
                table: "ivr_evidence");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_evidence");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_evidence");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_capacity_incidents");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_capacity_incidents");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_call_results");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_call_results");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_call_jobs");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_call_jobs");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_call_attempts");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_call_attempts");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_audit_log");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_audit_log");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_attempt_policies");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_attempt_policies");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "ivr_admin_actions");

            migrationBuilder.DropColumn(
                name: "legal_hold_until",
                table: "ivr_admin_actions");
        }
    }
}

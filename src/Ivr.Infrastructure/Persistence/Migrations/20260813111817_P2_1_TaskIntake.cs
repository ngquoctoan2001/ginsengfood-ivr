using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861, IDE0161 // EF-generated migration shape.

namespace Ivr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P2_1_TaskIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "call_script_template_id",
                table: "ivr_confirmation_tasks",
                type: "text",
                nullable: false,
                defaultValue: "legacy-unresolved");

            migrationBuilder.AddColumn<string>(
                name: "call_script_version",
                table: "ivr_confirmation_tasks",
                type: "text",
                nullable: false,
                defaultValue: "legacy-unresolved");

            migrationBuilder.AddColumn<string>(
                name: "evidence_policy_version",
                table: "ivr_confirmation_tasks",
                type: "text",
                nullable: false,
                defaultValue: "legacy-unresolved");

            migrationBuilder.AddColumn<string>(
                name: "privacy_policy_version",
                table: "ivr_confirmation_tasks",
                type: "text",
                nullable: false,
                defaultValue: "legacy-unresolved");

            migrationBuilder.Sql(
                """
                ALTER TABLE ivr_confirmation_tasks
                    ALTER COLUMN call_script_template_id DROP DEFAULT,
                    ALTER COLUMN call_script_version DROP DEFAULT,
                    ALTER COLUMN evidence_policy_version DROP DEFAULT,
                    ALTER COLUMN privacy_policy_version DROP DEFAULT;
                """);

            migrationBuilder.CreateTable(
                name: "ivr_task_intake_outbox",
                columns: table => new
                {
                    outbox_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<string>(type: "text", nullable: false),
                    ivr_call_job_id = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    payload_sha256 = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    legal_hold_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    anonymized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_task_intake_outbox", x => x.outbox_id);
                    table.CheckConstraint("ck_ivr_task_intake_outbox_hash", "payload_sha256 ~ '^[A-F0-9]{64}$'");
                    table.CheckConstraint("ck_ivr_task_intake_outbox_status", "status IN ('HELD_MOCK','READY_FOR_ELIGIBILITY','PUBLISHED')");
                    table.ForeignKey(
                        name: "FK_ivr_task_intake_outbox_ivr_call_jobs_ivr_call_job_id",
                        column: x => x.ivr_call_job_id,
                        principalTable: "ivr_call_jobs",
                        principalColumn: "ivr_call_job_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ivr_task_intake_outbox_ivr_confirmation_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "ivr_confirmation_tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_task_intake_outbox_anonymized_at",
                table: "ivr_task_intake_outbox",
                column: "anonymized_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_task_intake_outbox_correlation_id",
                table: "ivr_task_intake_outbox",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_task_intake_outbox_ivr_call_job_id",
                table: "ivr_task_intake_outbox",
                column: "ivr_call_job_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ivr_task_intake_outbox_legal_hold_until",
                table: "ivr_task_intake_outbox",
                column: "legal_hold_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_task_intake_outbox_published_at",
                table: "ivr_task_intake_outbox",
                column: "published_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_task_intake_outbox_retain_until",
                table: "ivr_task_intake_outbox",
                column: "retain_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_task_intake_outbox_status_created_at",
                table: "ivr_task_intake_outbox",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_task_intake_outbox_task_id",
                table: "ivr_task_intake_outbox",
                column: "task_id",
                unique: true);

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

                CREATE OR REPLACE FUNCTION ivr_enforce_task_intake_outbox_immutable()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                BEGIN
                    IF NEW.outbox_id IS DISTINCT FROM OLD.outbox_id
                       OR NEW.task_id IS DISTINCT FROM OLD.task_id
                       OR NEW.ivr_call_job_id IS DISTINCT FROM OLD.ivr_call_job_id
                       OR NEW.event_type IS DISTINCT FROM OLD.event_type
                       OR NEW.correlation_id IS DISTINCT FROM OLD.correlation_id
                       OR NEW.payload_sha256 IS DISTINCT FROM OLD.payload_sha256
                       OR NEW.created_at IS DISTINCT FROM OLD.created_at THEN
                        RAISE EXCEPTION 'task-intake outbox identity and payload are immutable';
                    END IF;

                    RETURN NEW;
                END;
                $function$;

                CREATE TRIGGER trg_ivr_task_intake_outbox_immutable
                BEFORE UPDATE ON ivr_task_intake_outbox
                FOR EACH ROW
                EXECUTE FUNCTION ivr_enforce_task_intake_outbox_immutable();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_ivr_task_intake_outbox_immutable ON ivr_task_intake_outbox;
                DROP FUNCTION IF EXISTS ivr_enforce_task_intake_outbox_immutable();

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

            migrationBuilder.DropTable(
                name: "ivr_task_intake_outbox");

            migrationBuilder.DropColumn(
                name: "call_script_template_id",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropColumn(
                name: "call_script_version",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropColumn(
                name: "evidence_policy_version",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropColumn(
                name: "privacy_policy_version",
                table: "ivr_confirmation_tasks");
        }
    }
}

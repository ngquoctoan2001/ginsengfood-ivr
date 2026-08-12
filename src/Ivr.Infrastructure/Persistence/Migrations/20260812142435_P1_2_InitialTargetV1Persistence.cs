using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861, IDE0161 // EF-generated migration shape.
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ivr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P1_2_InitialTargetV1Persistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ivr_admin_actions",
                columns: table => new
                {
                    admin_action_id = table.Column<string>(type: "text", nullable: false),
                    action_type = table.Column<string>(type: "text", nullable: false),
                    permission = table.Column<string>(type: "text", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    target_type = table.Column<string>(type: "text", nullable: false),
                    target_id = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    before_state_json = table.Column<string>(type: "jsonb", nullable: true),
                    after_state_json = table.Column<string>(type: "jsonb", nullable: true),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    evidence_ref = table.Column<string>(type: "text", nullable: true),
                    no_policy_bypass = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_admin_actions", x => x.admin_action_id);
                    table.CheckConstraint("ck_ivr_admin_actions_no_bypass", "no_policy_bypass IS TRUE");
                });

            migrationBuilder.CreateTable(
                name: "ivr_attempt_policies",
                columns: table => new
                {
                    policy_version = table.Column<string>(type: "text", nullable: false),
                    program_type = table.Column<string>(type: "text", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    attempt_offsets_seconds_json = table.Column<string>(type: "jsonb", nullable: false),
                    confirmation_window_seconds = table.Column<int>(type: "integer", nullable: false),
                    allowed_execution_modes_json = table.Column<string>(type: "jsonb", nullable: false),
                    approved_for_production = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_attempt_policies", x => new { x.policy_version, x.program_type });
                    table.CheckConstraint("ck_ivr_attempt_policies_attempt_bounds", "max_attempts BETWEEN 1 AND 10");
                    table.CheckConstraint("ck_ivr_attempt_policies_window_positive", "confirmation_window_seconds > 0");
                });

            migrationBuilder.CreateTable(
                name: "ivr_audit_log",
                columns: table => new
                {
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    actor_type = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    target_type = table.Column<string>(type: "text", nullable: false),
                    target_id = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    before_state_json = table.Column<string>(type: "jsonb", nullable: true),
                    after_state_json = table.Column<string>(type: "jsonb", nullable: true),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    data_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_audit_log", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "ivr_capacity_incidents",
                columns: table => new
                {
                    capacity_incident_id = table.Column<string>(type: "text", nullable: false),
                    session_id = table.Column<string>(type: "text", nullable: false),
                    program_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    scope = table.Column<string>(type: "text", nullable: false),
                    hold_new_calls = table.Column<bool>(type: "boolean", nullable: false),
                    active_sim_count = table.Column<int>(type: "integer", nullable: false),
                    pending_call_jobs = table.Column<int>(type: "integer", nullable: false),
                    expired_call_jobs = table.Column<int>(type: "integer", nullable: false),
                    missed_deadline_count = table.Column<int>(type: "integer", nullable: false),
                    shortage_reason = table.Column<string>(type: "text", nullable: true),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_capacity_incidents", x => x.capacity_incident_id);
                });

            migrationBuilder.CreateTable(
                name: "ivr_confirmation_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<string>(type: "text", nullable: false),
                    contract_version = table.Column<string>(type: "text", nullable: false),
                    idempotency_key = table.Column<string>(type: "text", nullable: false),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    official_order_id = table.Column<string>(type: "text", nullable: false),
                    order_code = table.Column<string>(type: "text", nullable: false),
                    order_version = table.Column<string>(type: "text", nullable: false),
                    order_state = table.Column<string>(type: "text", nullable: false),
                    payment_method_snapshot = table.Column<string>(type: "text", nullable: false),
                    ivr_confirmation_required = table.Column<bool>(type: "boolean", nullable: false),
                    customer_id = table.Column<string>(type: "text", nullable: true),
                    customer_trust_status = table.Column<string>(type: "text", nullable: true),
                    trusted_skip_allowed = table.Column<bool>(type: "boolean", nullable: true),
                    risk_flags_json = table.Column<string>(type: "jsonb", nullable: true),
                    program_type = table.Column<string>(type: "text", nullable: false),
                    attempt_policy_version = table.Column<string>(type: "text", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    attempt_offsets_seconds_json = table.Column<string>(type: "jsonb", nullable: false),
                    confirmation_window_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    confirmation_window_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    official_contact_id = table.Column<string>(type: "text", nullable: true),
                    phone_ref = table.Column<string>(type: "text", nullable: false),
                    phone_masked = table.Column<string>(type: "text", nullable: false),
                    phone_validation_status = table.Column<string>(type: "text", nullable: true),
                    dial_token_ciphertext = table.Column<string>(type: "text", nullable: false),
                    dial_token_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    privacy_safe_order_summary_json = table.Column<string>(type: "jsonb", nullable: false),
                    eligibility_decision = table.Column<string>(type: "text", nullable: true),
                    eligibility_snapshot_json = table.Column<string>(type: "jsonb", nullable: true),
                    blocked_reasons_json = table.Column<string>(type: "jsonb", nullable: true),
                    sellable_status_json = table.Column<string>(type: "jsonb", nullable: true),
                    sellable_captured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    call_restriction = table.Column<bool>(type: "boolean", nullable: false),
                    not_for_quote_cart_draft = table.Column<bool>(type: "boolean", nullable: false),
                    no_direct_order_update = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rejected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reject_reason = table.Column<string>(type: "text", nullable: true),
                    evidence_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                    audit_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_confirmation_tasks", x => x.id);
                    table.UniqueConstraint("AK_ivr_confirmation_tasks_task_id", x => x.task_id);
                    table.CheckConstraint("ck_ivr_confirmation_tasks_attempt_bounds", "max_attempts BETWEEN 1 AND 10");
                    table.CheckConstraint("ck_ivr_confirmation_tasks_masked_phone", "phone_masked ~ '[xX*]' AND phone_masked !~ '(^|[^0-9])(0|84)[0-9]{9}([^0-9]|$)'");
                    table.CheckConstraint("ck_ivr_confirmation_tasks_matrix", "(program_type = 'GOLDEN_HOUR' AND payment_method_snapshot = 'ONLINE') OR (program_type = 'TWENTY_FOUR_SEVEN' AND payment_method_snapshot = 'COD')");
                    table.CheckConstraint("ck_ivr_confirmation_tasks_required_flag", "ivr_confirmation_required IS TRUE");
                    table.CheckConstraint("ck_ivr_confirmation_tasks_signal_only", "not_for_quote_cart_draft IS TRUE AND no_direct_order_update IS TRUE");
                    table.CheckConstraint("ck_ivr_confirmation_tasks_token_ttl", "dial_token_expires_at >= confirmation_window_started_at AND dial_token_expires_at <= confirmation_window_expires_at");
                    table.CheckConstraint("ck_ivr_confirmation_tasks_window", "confirmation_window_started_at < confirmation_window_expires_at AND expires_at = confirmation_window_expires_at");
                });

            migrationBuilder.CreateTable(
                name: "ivr_evidence",
                columns: table => new
                {
                    evidence_ref = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    work_id = table.Column<string>(type: "text", nullable: false),
                    payload_ref = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_evidence", x => x.evidence_ref);
                });

            migrationBuilder.CreateTable(
                name: "ivr_evidence_links",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_table = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<string>(type: "text", nullable: false),
                    evidence_ref = table.Column<string>(type: "text", nullable: false),
                    audit_ref = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_evidence_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ivr_feature_flags",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    env = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    value_json = table.Column<string>(type: "jsonb", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_feature_flags", x => new { x.key, x.env });
                });

            migrationBuilder.CreateTable(
                name: "ivr_idempotency_keys",
                columns: table => new
                {
                    scope = table.Column<string>(type: "text", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    payload_hash = table.Column<string>(type: "text", nullable: false),
                    response_snapshot_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_idempotency_keys", x => new { x.scope, x.key });
                });

            migrationBuilder.CreateTable(
                name: "ivr_review_items",
                columns: table => new
                {
                    review_item_id = table.Column<string>(type: "text", nullable: false),
                    source_type = table.Column<string>(type: "text", nullable: false),
                    source_id = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    assigned_to = table.Column<string>(type: "text", nullable: true),
                    resolution = table.Column<string>(type: "text", nullable: true),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_review_items", x => x.review_item_id);
                });

            migrationBuilder.CreateTable(
                name: "ivr_sim_channels",
                columns: table => new
                {
                    sim_channel_id = table.Column<string>(type: "text", nullable: false),
                    sim_number_ref = table.Column<string>(type: "text", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    adapter_mode = table.Column<string>(type: "text", nullable: false),
                    execution_mode = table.Column<string>(type: "text", nullable: false),
                    provider_name = table.Column<string>(type: "text", nullable: false),
                    active_call_job_id = table.Column<string>(type: "text", nullable: true),
                    fail_count = table.Column<int>(type: "integer", nullable: false),
                    last_health_check_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cooldown_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    quarantine_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disabled_reason = table.Column<string>(type: "text", nullable: true),
                    lease_token = table.Column<Guid>(type: "uuid", nullable: true),
                    lease_fencing_generation = table.Column<long>(type: "bigint", nullable: false),
                    leased_by_worker_id = table.Column<string>(type: "text", nullable: true),
                    lease_acquired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_sim_channels", x => x.sim_channel_id);
                    table.CheckConstraint("ck_ivr_sim_channels_fencing", "lease_fencing_generation >= 0");
                    table.CheckConstraint("ck_ivr_sim_channels_lease", "active_call_job_id IS NULL OR lease_token IS NOT NULL");
                    table.CheckConstraint("ck_ivr_sim_channels_mode", "execution_mode IN ('MOCK','LAB_REAL_SIM','PRODUCTION_REAL')");
                });

            migrationBuilder.CreateTable(
                name: "ivr_call_jobs",
                columns: table => new
                {
                    ivr_call_job_id = table.Column<string>(type: "text", nullable: false),
                    task_id = table.Column<string>(type: "text", nullable: false),
                    official_order_id = table.Column<string>(type: "text", nullable: false),
                    order_version_snapshot = table.Column<string>(type: "text", nullable: false),
                    program_type = table.Column<string>(type: "text", nullable: false),
                    attempt_policy_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    attempt_offsets_seconds_json = table.Column<string>(type: "jsonb", nullable: false),
                    confirmation_window_seconds = table.Column<int>(type: "integer", nullable: false),
                    attempt_schedule_json = table.Column<string>(type: "jsonb", nullable: false),
                    t0_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    eligible = table.Column<bool>(type: "boolean", nullable: false),
                    eligibility_decision = table.Column<string>(type: "text", nullable: false),
                    queue_status = table.Column<string>(type: "text", nullable: false),
                    capacity_incident_id = table.Column<string>(type: "text", nullable: true),
                    script_version = table.Column<string>(type: "text", nullable: false),
                    privacy_policy_version = table.Column<string>(type: "text", nullable: false),
                    input_signal_only = table.Column<bool>(type: "boolean", nullable: false),
                    no_direct_order_update = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_reason = table.Column<string>(type: "text", nullable: true),
                    evidence_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                    audit_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_call_jobs", x => x.ivr_call_job_id);
                    table.CheckConstraint("ck_ivr_call_jobs_attempt_bounds", "max_attempts BETWEEN 1 AND 10");
                    table.CheckConstraint("ck_ivr_call_jobs_signal_only", "input_signal_only IS TRUE AND no_direct_order_update IS TRUE");
                    table.CheckConstraint("ck_ivr_call_jobs_window_positive", "confirmation_window_seconds > 0 AND t0_at < expires_at");
                    table.ForeignKey(
                        name: "FK_ivr_call_jobs_ivr_confirmation_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "ivr_confirmation_tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ivr_call_attempts",
                columns: table => new
                {
                    ivr_call_attempt_id = table.Column<string>(type: "text", nullable: false),
                    ivr_call_job_id = table.Column<string>(type: "text", nullable: false),
                    task_id = table.Column<string>(type: "text", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    max_attempts_snapshot = table.Column<int>(type: "integer", nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    scheduled_window_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    result_status = table.Column<string>(type: "text", nullable: true),
                    dtmf_key = table.Column<string>(type: "text", nullable: true),
                    disposition = table.Column<string>(type: "text", nullable: true),
                    is_counted_customer_attempt = table.Column<bool>(type: "boolean", nullable: false),
                    technical_retry_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    technical_retry_count = table.Column<int>(type: "integer", nullable: false),
                    no_answer = table.Column<bool>(type: "boolean", nullable: false),
                    invalid_phone = table.Column<bool>(type: "boolean", nullable: false),
                    technical_exception_type = table.Column<string>(type: "text", nullable: true),
                    sim_channel_id = table.Column<string>(type: "text", nullable: true),
                    provider_call_id = table.Column<string>(type: "text", nullable: true),
                    raw_call_event_id = table.Column<string>(type: "text", nullable: true),
                    blocked_reason = table.Column<string>(type: "text", nullable: true),
                    policy_version = table.Column<string>(type: "text", nullable: false),
                    script_version = table.Column<string>(type: "text", nullable: false),
                    evidence_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                    audit_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_call_attempts", x => x.ivr_call_attempt_id);
                    table.CheckConstraint("ck_ivr_call_attempts_number_snapshot", "attempt_number >= 1 AND attempt_number <= max_attempts_snapshot AND max_attempts_snapshot BETWEEN 1 AND 10");
                    table.CheckConstraint("ck_ivr_call_attempts_retry_nonnegative", "technical_retry_count >= 0");
                    table.CheckConstraint("ck_ivr_call_attempts_technical_not_counted", "technical_exception_type IS NULL OR is_counted_customer_attempt IS FALSE");
                    table.ForeignKey(
                        name: "FK_ivr_call_attempts_ivr_call_jobs_ivr_call_job_id",
                        column: x => x.ivr_call_job_id,
                        principalTable: "ivr_call_jobs",
                        principalColumn: "ivr_call_job_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ivr_call_results",
                columns: table => new
                {
                    ivr_call_result_id = table.Column<string>(type: "text", nullable: false),
                    ivr_call_job_id = table.Column<string>(type: "text", nullable: false),
                    task_id = table.Column<string>(type: "text", nullable: false),
                    official_order_id = table.Column<string>(type: "text", nullable: false),
                    order_version_snapshot = table.Column<string>(type: "text", nullable: true),
                    order_version_seen_by_ivr = table.Column<string>(type: "text", nullable: false),
                    final_result_status = table.Column<string>(type: "text", nullable: false),
                    result_type = table.Column<string>(type: "text", nullable: false),
                    result_reason = table.Column<string>(type: "text", nullable: true),
                    dtmf_key = table.Column<string>(type: "text", nullable: true),
                    is_counted_customer_attempt = table.Column<bool>(type: "boolean", nullable: false),
                    is_final_for_ivr = table.Column<bool>(type: "boolean", nullable: false),
                    recommended_core_action = table.Column<string>(type: "text", nullable: false),
                    core_order_handoff_required = table.Column<bool>(type: "boolean", nullable: false),
                    human_review_required = table.Column<bool>(type: "boolean", nullable: false),
                    input_signal_only = table.Column<bool>(type: "boolean", nullable: false),
                    no_direct_order_update = table.Column<bool>(type: "boolean", nullable: false),
                    no_payment_or_revenue_effect = table.Column<bool>(type: "boolean", nullable: false),
                    technical_error_code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    evidence_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                    audit_refs_json = table.Column<string>(type: "jsonb", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_call_results", x => x.ivr_call_result_id);
                    table.CheckConstraint("ck_ivr_call_results_signal_only", "input_signal_only IS TRUE AND no_direct_order_update IS TRUE AND no_payment_or_revenue_effect IS TRUE");
                    table.ForeignKey(
                        name: "FK_ivr_call_results_ivr_call_jobs_ivr_call_job_id",
                        column: x => x.ivr_call_job_id,
                        principalTable: "ivr_call_jobs",
                        principalColumn: "ivr_call_job_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ivr_raw_call_events",
                columns: table => new
                {
                    raw_event_id = table.Column<string>(type: "text", nullable: false),
                    ivr_call_attempt_id = table.Column<string>(type: "text", nullable: false),
                    ivr_call_job_id = table.Column<string>(type: "text", nullable: false),
                    provider_internal_payload_ref = table.Column<string>(type: "text", nullable: true),
                    raw_call_status = table.Column<string>(type: "text", nullable: false),
                    raw_dtmf = table.Column<string>(type: "text", nullable: true),
                    audio_status = table.Column<string>(type: "text", nullable: true),
                    technical_error_code = table.Column<string>(type: "text", nullable: true),
                    recording_ref = table.Column<string>(type: "text", nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_raw_call_events", x => x.raw_event_id);
                    table.CheckConstraint("ck_ivr_raw_call_events_recording_off", "recording_ref IS NULL");
                    table.ForeignKey(
                        name: "FK_ivr_raw_call_events_ivr_call_attempts_ivr_call_attempt_id",
                        column: x => x.ivr_call_attempt_id,
                        principalTable: "ivr_call_attempts",
                        principalColumn: "ivr_call_attempt_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ivr_technical_exceptions",
                columns: table => new
                {
                    technical_exception_id = table.Column<string>(type: "text", nullable: false),
                    ivr_call_attempt_id = table.Column<string>(type: "text", nullable: false),
                    exception_type = table.Column<string>(type: "text", nullable: false),
                    customer_attempt_counted = table.Column<bool>(type: "boolean", nullable: false),
                    technical_retry_allowed = table.Column<bool>(type: "boolean", nullable: false),
                    technical_retry_count = table.Column<int>(type: "integer", nullable: false),
                    retry_reason = table.Column<string>(type: "text", nullable: true),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_technical_exceptions", x => x.technical_exception_id);
                    table.CheckConstraint("ck_ivr_technical_exceptions_not_counted", "customer_attempt_counted IS FALSE AND technical_retry_count >= 0");
                    table.ForeignKey(
                        name: "FK_ivr_technical_exceptions_ivr_call_attempts_ivr_call_attempt~",
                        column: x => x.ivr_call_attempt_id,
                        principalTable: "ivr_call_attempts",
                        principalColumn: "ivr_call_attempt_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ivr_result_callbacks",
                columns: table => new
                {
                    callback_id = table.Column<string>(type: "text", nullable: false),
                    ivr_call_result_id = table.Column<string>(type: "text", nullable: false),
                    task_id = table.Column<string>(type: "text", nullable: false),
                    official_order_id = table.Column<string>(type: "text", nullable: false),
                    idempotency_key = table.Column<string>(type: "text", nullable: false),
                    result_status = table.Column<string>(type: "text", nullable: false),
                    result_state = table.Column<string>(type: "text", nullable: false),
                    delivery_status = table.Column<string>(type: "text", nullable: false),
                    requires_core_revalidation = table.Column<bool>(type: "boolean", nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    payload_sha256 = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    acknowledged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    core_http_status = table.Column<int>(type: "integer", nullable: true),
                    core_response_code = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    last_retry_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    next_retry_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    lease_token = table.Column<string>(type: "text", nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retention_class = table.Column<string>(type: "text", nullable: false),
                    retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_result_callbacks", x => x.callback_id);
                    table.CheckConstraint("ck_ivr_result_callbacks_hash", "payload_sha256 ~ '^[A-F0-9]{64}$'");
                    table.CheckConstraint("ck_ivr_result_callbacks_retry_nonnegative", "retry_count >= 0");
                    table.CheckConstraint("ck_ivr_result_callbacks_revalidation", "requires_core_revalidation IS TRUE");
                    table.ForeignKey(
                        name: "FK_ivr_result_callbacks_ivr_call_results_ivr_call_result_id",
                        column: x => x.ivr_call_result_id,
                        principalTable: "ivr_call_results",
                        principalColumn: "ivr_call_result_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ivr_feature_flags",
                columns: new[] { "env", "key", "enabled", "reason", "revision", "updated_at", "updated_by", "value_json" },
                values: new object[,]
                {
                    { "dev", "attemptPolicyVersion", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"mock-lab-v1\"" },
                    { "lab", "attemptPolicyVersion", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"mock-lab-v1\"" },
                    { "pilot", "attemptPolicyVersion", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"mock-lab-v1\"" },
                    { "prod", "attemptPolicyVersion", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"mock-lab-v1\"" },
                    { "staging", "attemptPolicyVersion", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"mock-lab-v1\"" },
                    { "dev", "executionMode", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "lab", "executionMode", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "pilot", "executionMode", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "prod", "executionMode", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "staging", "executionMode", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "dev", "globalDialKillSwitch", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "true" },
                    { "lab", "globalDialKillSwitch", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "true" },
                    { "pilot", "globalDialKillSwitch", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "true" },
                    { "prod", "globalDialKillSwitch", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "true" },
                    { "staging", "globalDialKillSwitch", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "true" },
                    { "dev", "labDestinationAllowlist", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "[]" },
                    { "lab", "labDestinationAllowlist", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "[]" },
                    { "pilot", "labDestinationAllowlist", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "[]" },
                    { "prod", "labDestinationAllowlist", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "[]" },
                    { "staging", "labDestinationAllowlist", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "[]" },
                    { "dev", "realCustomerCallAllowed", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "lab", "realCustomerCallAllowed", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "pilot", "realCustomerCallAllowed", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "prod", "realCustomerCallAllowed", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "staging", "realCustomerCallAllowed", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "dev", "recordingEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "lab", "recordingEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "pilot", "recordingEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "prod", "recordingEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "staging", "recordingEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "dev", "salesProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"FAKE_TARGET_V1\"" },
                    { "lab", "salesProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"FAKE_TARGET_V1\"" },
                    { "pilot", "salesProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"FAKE_TARGET_V1\"" },
                    { "prod", "salesProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"FAKE_TARGET_V1\"" },
                    { "staging", "salesProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"FAKE_TARGET_V1\"" },
                    { "dev", "simProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "lab", "simProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "pilot", "simProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "prod", "simProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "staging", "simProvider", true, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "\"MOCK\"" },
                    { "dev", "v1NotificationEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "lab", "v1NotificationEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "pilot", "v1NotificationEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "prod", "v1NotificationEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" },
                    { "staging", "v1NotificationEnabled", false, "safe bootstrap seed", 0L, new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bootstrap", "false" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_admin_actions_correlation_id",
                table: "ivr_admin_actions",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_admin_actions_created_at",
                table: "ivr_admin_actions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_attempt_policies_approved_for_production",
                table: "ivr_attempt_policies",
                column: "approved_for_production");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_audit_log_correlation_id",
                table: "ivr_audit_log",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_audit_log_created_at",
                table: "ivr_audit_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_audit_log_target_type_target_id",
                table: "ivr_audit_log",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_disposition",
                table: "ivr_call_attempts",
                column: "disposition");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_is_counted_customer_attempt",
                table: "ivr_call_attempts",
                column: "is_counted_customer_attempt");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_ivr_call_job_id_attempt_number",
                table: "ivr_call_attempts",
                columns: new[] { "ivr_call_job_id", "attempt_number" },
                unique: true,
                filter: "is_counted_customer_attempt IS TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_provider_call_id",
                table: "ivr_call_attempts",
                column: "provider_call_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_raw_call_event_id",
                table: "ivr_call_attempts",
                column: "raw_call_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_result_status",
                table: "ivr_call_attempts",
                column: "result_status");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_scheduled_at",
                table: "ivr_call_attempts",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_scheduled_window_expires_at",
                table: "ivr_call_attempts",
                column: "scheduled_window_expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_sim_channel_id",
                table: "ivr_call_attempts",
                column: "sim_channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_status",
                table: "ivr_call_attempts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_task_id",
                table: "ivr_call_attempts",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_technical_exception_type",
                table: "ivr_call_attempts",
                column: "technical_exception_type");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_capacity_incident_id",
                table: "ivr_call_jobs",
                column: "capacity_incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_closed_at",
                table: "ivr_call_jobs",
                column: "closed_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_created_at",
                table: "ivr_call_jobs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_eligibility_decision",
                table: "ivr_call_jobs",
                column: "eligibility_decision");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_eligible",
                table: "ivr_call_jobs",
                column: "eligible");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_official_order_id_status",
                table: "ivr_call_jobs",
                columns: new[] { "official_order_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_order_version_snapshot",
                table: "ivr_call_jobs",
                column: "order_version_snapshot");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_program_type_status",
                table: "ivr_call_jobs",
                columns: new[] { "program_type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_queue_status",
                table: "ivr_call_jobs",
                column: "queue_status");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_status_expires_at",
                table: "ivr_call_jobs",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_t0_at",
                table: "ivr_call_jobs",
                column: "t0_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_jobs_task_id",
                table: "ivr_call_jobs",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_created_at",
                table: "ivr_call_results",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_final_result_status",
                table: "ivr_call_results",
                column: "final_result_status");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_human_review_required",
                table: "ivr_call_results",
                column: "human_review_required");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_is_counted_customer_attempt",
                table: "ivr_call_results",
                column: "is_counted_customer_attempt");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_is_final_for_ivr",
                table: "ivr_call_results",
                column: "is_final_for_ivr");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_ivr_call_job_id",
                table: "ivr_call_results",
                column: "ivr_call_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_official_order_id",
                table: "ivr_call_results",
                column: "official_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_order_version_seen_by_ivr",
                table: "ivr_call_results",
                column: "order_version_seen_by_ivr");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_order_version_snapshot",
                table: "ivr_call_results",
                column: "order_version_snapshot");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_result_type",
                table: "ivr_call_results",
                column: "result_type");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_results_task_id",
                table: "ivr_call_results",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_capacity_incidents_status_opened_at",
                table: "ivr_capacity_incidents",
                columns: new[] { "status", "opened_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_attempt_policy_version",
                table: "ivr_confirmation_tasks",
                column: "attempt_policy_version");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_confirmation_window_expires_at",
                table: "ivr_confirmation_tasks",
                column: "confirmation_window_expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_confirmation_window_started_at",
                table: "ivr_confirmation_tasks",
                column: "confirmation_window_started_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_correlation_id",
                table: "ivr_confirmation_tasks",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_created_at",
                table: "ivr_confirmation_tasks",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_customer_id",
                table: "ivr_confirmation_tasks",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_dial_token_expires_at",
                table: "ivr_confirmation_tasks",
                column: "dial_token_expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_eligibility_decision",
                table: "ivr_confirmation_tasks",
                column: "eligibility_decision");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_expires_at",
                table: "ivr_confirmation_tasks",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_idempotency_key",
                table: "ivr_confirmation_tasks",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_ivr_confirmation_required",
                table: "ivr_confirmation_tasks",
                column: "ivr_confirmation_required");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_official_contact_id",
                table: "ivr_confirmation_tasks",
                column: "official_contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_official_order_id",
                table: "ivr_confirmation_tasks",
                column: "official_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_order_state",
                table: "ivr_confirmation_tasks",
                column: "order_state");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_order_version",
                table: "ivr_confirmation_tasks",
                column: "order_version");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_payment_method_snapshot",
                table: "ivr_confirmation_tasks",
                column: "payment_method_snapshot");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_phone_validation_status",
                table: "ivr_confirmation_tasks",
                column: "phone_validation_status");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_confirmation_tasks_program_type",
                table: "ivr_confirmation_tasks",
                column: "program_type");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_correlation_id",
                table: "ivr_evidence",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_created_at",
                table: "ivr_evidence",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_work_id",
                table: "ivr_evidence",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_links_evidence_ref",
                table: "ivr_evidence_links",
                column: "evidence_ref");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_evidence_links_owner_table_owner_id",
                table: "ivr_evidence_links",
                columns: new[] { "owner_table", "owner_id" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_feature_flags_env_revision",
                table: "ivr_feature_flags",
                columns: new[] { "env", "revision" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_feature_flags_key_env",
                table: "ivr_feature_flags",
                columns: new[] { "key", "env" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ivr_idempotency_keys_created_at",
                table: "ivr_idempotency_keys",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_idempotency_keys_expires_at",
                table: "ivr_idempotency_keys",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_raw_call_events_ivr_call_attempt_id",
                table: "ivr_raw_call_events",
                column: "ivr_call_attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_raw_call_events_ivr_call_job_id",
                table: "ivr_raw_call_events",
                column: "ivr_call_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_raw_call_events_received_at",
                table: "ivr_raw_call_events",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_raw_call_events_technical_error_code",
                table: "ivr_raw_call_events",
                column: "technical_error_code");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_acknowledged_at",
                table: "ivr_result_callbacks",
                column: "acknowledged_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_core_http_status",
                table: "ivr_result_callbacks",
                column: "core_http_status");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_core_response_code",
                table: "ivr_result_callbacks",
                column: "core_response_code");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_delivery_status_next_retry_at",
                table: "ivr_result_callbacks",
                columns: new[] { "delivery_status", "next_retry_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_idempotency_key",
                table: "ivr_result_callbacks",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_ivr_call_result_id",
                table: "ivr_result_callbacks",
                column: "ivr_call_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_lease_expires_at",
                table: "ivr_result_callbacks",
                column: "lease_expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_official_order_id",
                table: "ivr_result_callbacks",
                column: "official_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_result_status_result_state",
                table: "ivr_result_callbacks",
                columns: new[] { "result_status", "result_state" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_sent_at",
                table: "ivr_result_callbacks",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_result_callbacks_task_id",
                table: "ivr_result_callbacks",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_review_items_correlation_id",
                table: "ivr_review_items",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_review_items_source_type_source_id",
                table: "ivr_review_items",
                columns: new[] { "source_type", "source_id" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_review_items_status_created_at",
                table: "ivr_review_items",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_sim_channels_lease_expires_at",
                table: "ivr_sim_channels",
                column: "lease_expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_sim_channels_lease_token",
                table: "ivr_sim_channels",
                column: "lease_token",
                unique: true,
                filter: "lease_token IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_sim_channels_quarantine_until",
                table: "ivr_sim_channels",
                column: "quarantine_until");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_sim_channels_status_enabled_cooldown_until",
                table: "ivr_sim_channels",
                columns: new[] { "status", "enabled", "cooldown_until" });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_technical_exceptions_correlation_id",
                table: "ivr_technical_exceptions",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_technical_exceptions_created_at",
                table: "ivr_technical_exceptions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_technical_exceptions_ivr_call_attempt_id",
                table: "ivr_technical_exceptions",
                column: "ivr_call_attempt_id");

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

                CREATE TRIGGER trg_ivr_confirmation_tasks_snapshot_immutable
                BEFORE UPDATE ON ivr_confirmation_tasks
                FOR EACH ROW
                EXECUTE FUNCTION ivr_enforce_confirmation_task_snapshot_immutable();

                CREATE OR REPLACE FUNCTION ivr_reject_attempt_policy_update()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                BEGIN
                    RAISE EXCEPTION 'attempt-policy versions are immutable; create a new version';
                END;
                $function$;

                CREATE TRIGGER trg_ivr_attempt_policies_immutable
                BEFORE UPDATE ON ivr_attempt_policies
                FOR EACH ROW
                EXECUTE FUNCTION ivr_reject_attempt_policy_update();

                CREATE OR REPLACE FUNCTION ivr_enforce_call_job_policy_snapshot_immutable()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                BEGIN
                    IF NEW.attempt_policy_code IS DISTINCT FROM OLD.attempt_policy_code
                       OR NEW.max_attempts IS DISTINCT FROM OLD.max_attempts
                       OR NEW.attempt_offsets_seconds_json IS DISTINCT FROM OLD.attempt_offsets_seconds_json
                       OR NEW.confirmation_window_seconds IS DISTINCT FROM OLD.confirmation_window_seconds
                       OR NEW.attempt_schedule_json IS DISTINCT FROM OLD.attempt_schedule_json THEN
                        RAISE EXCEPTION 'call-job policy snapshot is immutable';
                    END IF;

                    RETURN NEW;
                END;
                $function$;

                CREATE TRIGGER trg_ivr_call_jobs_policy_snapshot_immutable
                BEFORE UPDATE ON ivr_call_jobs
                FOR EACH ROW
                EXECUTE FUNCTION ivr_enforce_call_job_policy_snapshot_immutable();

                CREATE OR REPLACE FUNCTION ivr_enforce_attempt_snapshot()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                DECLARE
                    expected_max_attempts integer;
                BEGIN
                    IF TG_OP = 'UPDATE' THEN
                        IF NEW.max_attempts_snapshot IS DISTINCT FROM OLD.max_attempts_snapshot
                           OR NEW.attempt_number IS DISTINCT FROM OLD.attempt_number THEN
                            RAISE EXCEPTION 'call-attempt numbering snapshot is immutable';
                        END IF;

                        RETURN NEW;
                    END IF;

                    SELECT max_attempts
                    INTO expected_max_attempts
                    FROM ivr_call_jobs
                    WHERE ivr_call_job_id = NEW.ivr_call_job_id;

                    IF expected_max_attempts IS NULL THEN
                        RAISE EXCEPTION 'call job % does not exist', NEW.ivr_call_job_id;
                    END IF;

                    NEW.max_attempts_snapshot := expected_max_attempts;
                    RETURN NEW;
                END;
                $function$;

                CREATE TRIGGER trg_ivr_call_attempts_snapshot
                BEFORE INSERT OR UPDATE ON ivr_call_attempts
                FOR EACH ROW
                EXECUTE FUNCTION ivr_enforce_attempt_snapshot();

                CREATE OR REPLACE FUNCTION ivr_enforce_callback_payload_immutable()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                BEGIN
                    IF NEW.ivr_call_result_id IS DISTINCT FROM OLD.ivr_call_result_id
                       OR NEW.task_id IS DISTINCT FROM OLD.task_id
                       OR NEW.official_order_id IS DISTINCT FROM OLD.official_order_id
                       OR NEW.idempotency_key IS DISTINCT FROM OLD.idempotency_key
                       OR NEW.result_status IS DISTINCT FROM OLD.result_status
                       OR NEW.result_state IS DISTINCT FROM OLD.result_state
                       OR NEW.payload_json IS DISTINCT FROM OLD.payload_json
                       OR NEW.payload_sha256 IS DISTINCT FROM OLD.payload_sha256
                       OR NEW.requires_core_revalidation IS DISTINCT FROM OLD.requires_core_revalidation THEN
                        RAISE EXCEPTION 'result-callback business payload is immutable';
                    END IF;

                    RETURN NEW;
                END;
                $function$;

                CREATE TRIGGER trg_ivr_result_callbacks_payload_immutable
                BEFORE UPDATE ON ivr_result_callbacks
                FOR EACH ROW
                EXECUTE FUNCTION ivr_enforce_callback_payload_immutable();

                CREATE OR REPLACE FUNCTION ivr_reject_audit_mutation()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                BEGIN
                    RAISE EXCEPTION 'audit rows are append-only';
                END;
                $function$;

                CREATE TRIGGER trg_ivr_audit_log_append_only
                BEFORE UPDATE OR DELETE ON ivr_audit_log
                FOR EACH ROW
                EXECUTE FUNCTION ivr_reject_audit_mutation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
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
                """);

            migrationBuilder.DropTable(
                name: "ivr_admin_actions");

            migrationBuilder.DropTable(
                name: "ivr_attempt_policies");

            migrationBuilder.DropTable(
                name: "ivr_audit_log");

            migrationBuilder.DropTable(
                name: "ivr_capacity_incidents");

            migrationBuilder.DropTable(
                name: "ivr_evidence");

            migrationBuilder.DropTable(
                name: "ivr_evidence_links");

            migrationBuilder.DropTable(
                name: "ivr_feature_flags");

            migrationBuilder.DropTable(
                name: "ivr_idempotency_keys");

            migrationBuilder.DropTable(
                name: "ivr_raw_call_events");

            migrationBuilder.DropTable(
                name: "ivr_result_callbacks");

            migrationBuilder.DropTable(
                name: "ivr_review_items");

            migrationBuilder.DropTable(
                name: "ivr_sim_channels");

            migrationBuilder.DropTable(
                name: "ivr_technical_exceptions");

            migrationBuilder.DropTable(
                name: "ivr_call_results");

            migrationBuilder.DropTable(
                name: "ivr_call_attempts");

            migrationBuilder.DropTable(
                name: "ivr_call_jobs");

            migrationBuilder.DropTable(
                name: "ivr_confirmation_tasks");
        }
    }
}

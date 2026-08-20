using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861, IDE0161 // EF-generated migration shape.

namespace Ivr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P10_4_AnalyticsWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "agg_kpi_daily",
                schema: "analytics",
                columns: table => new
                {
                    bucket_date = table.Column<DateOnly>(type: "date", nullable: false),
                    program_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    script_variant_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    total_results = table.Column<int>(type: "integer", nullable: false),
                    final_results = table.Column<int>(type: "integer", nullable: false),
                    distinct_orders = table.Column<int>(type: "integer", nullable: false),
                    confirmed_count = table.Column<int>(type: "integer", nullable: false),
                    cancelled_count = table.Column<int>(type: "integer", nullable: false),
                    no_answer_count = table.Column<int>(type: "integer", nullable: false),
                    invalid_phone_count = table.Column<int>(type: "integer", nullable: false),
                    technical_count = table.Column<int>(type: "integer", nullable: false),
                    operational_blocked_count = table.Column<int>(type: "integer", nullable: false),
                    second_attempt_results = table.Column<int>(type: "integer", nullable: false),
                    seconds_to_result_sum = table.Column<long>(type: "bigint", nullable: false),
                    seconds_to_result_count = table.Column<int>(type: "integer", nullable: false),
                    computed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agg_kpi_daily", x => new { x.bucket_date, x.program_key, x.script_variant_key });
                });

            migrationBuilder.CreateTable(
                name: "dim_program",
                schema: "analytics",
                columns: table => new
                {
                    program_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fact_row_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dim_program", x => x.program_key);
                });

            migrationBuilder.CreateTable(
                name: "dim_result_type",
                schema: "analytics",
                columns: table => new
                {
                    result_type_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_final = table.Column<bool>(type: "boolean", nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fact_row_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dim_result_type", x => x.result_type_key);
                });

            migrationBuilder.CreateTable(
                name: "dim_script_variant",
                schema: "analytics",
                columns: table => new
                {
                    script_variant_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fact_row_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dim_script_variant", x => x.script_variant_key);
                });

            migrationBuilder.CreateTable(
                name: "etl_checkpoint",
                schema: "analytics",
                columns: table => new
                {
                    pipeline_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    last_run_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_run_loaded_rows = table.Column<int>(type: "integer", nullable: false),
                    last_run_rejected_rows = table.Column<int>(type: "integer", nullable: false),
                    last_run_duration_ms = table.Column<long>(type: "bigint", nullable: false),
                    total_loaded_rows = table.Column<long>(type: "bigint", nullable: false),
                    total_rejected_rows = table.Column<long>(type: "bigint", nullable: false),
                    high_water_event_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_reconciled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    source_row_count = table.Column<int>(type: "integer", nullable: false),
                    fact_row_count = table.Column<int>(type: "integer", nullable: false),
                    reconcile_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etl_checkpoint", x => x.pipeline_name);
                });

            migrationBuilder.CreateTable(
                name: "fact_call_job",
                schema: "analytics",
                columns: table => new
                {
                    ivr_call_job_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_ref_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    program_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    script_variant_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    eligible = table.Column<bool>(type: "boolean", nullable: false),
                    counted_attempt_count = table.Column<int>(type: "integer", nullable: false),
                    closed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_date = table.Column<DateOnly>(type: "date", nullable: false),
                    loaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fact_call_job", x => x.ivr_call_job_id);
                    table.CheckConstraint("ck_analytics_job_order_ref_hash", "order_ref_hash ~ '^[a-f0-9]{64}$'");
                });

            migrationBuilder.CreateTable(
                name: "fact_call_outcome",
                schema: "analytics",
                columns: table => new
                {
                    ivr_call_result_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ivr_call_job_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_ref_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    program_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    script_variant_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    result_type_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    final_result_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    dtmf_key = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    is_final = table.Column<bool>(type: "boolean", nullable: false),
                    is_counted_customer_attempt = table.Column<bool>(type: "boolean", nullable: false),
                    counted_attempt_number = table.Column<int>(type: "integer", nullable: false),
                    event_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    event_date = table.Column<DateOnly>(type: "date", nullable: false),
                    event_hour = table.Column<int>(type: "integer", nullable: false),
                    seconds_to_result = table.Column<int>(type: "integer", nullable: true),
                    loaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fact_call_outcome", x => x.ivr_call_result_id);
                    table.CheckConstraint("ck_analytics_fact_dtmf", "dtmf_key IS NULL OR dtmf_key ~ '^[0-9*#]$'");
                    table.CheckConstraint("ck_analytics_fact_event_hour", "event_hour BETWEEN 0 AND 23");
                    table.CheckConstraint("ck_analytics_fact_order_ref_hash", "order_ref_hash ~ '^[a-f0-9]{64}$'");
                });

            migrationBuilder.CreateIndex(
                name: "IX_fact_call_job_closed",
                schema: "analytics",
                table: "fact_call_job",
                column: "closed");

            migrationBuilder.CreateIndex(
                name: "IX_fact_call_job_created_date",
                schema: "analytics",
                table: "fact_call_job",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "IX_fact_call_outcome_event_date",
                schema: "analytics",
                table: "fact_call_outcome",
                column: "event_date");

            migrationBuilder.CreateIndex(
                name: "IX_fact_call_outcome_event_date_program_key",
                schema: "analytics",
                table: "fact_call_outcome",
                columns: new[] { "event_date", "program_key" });

            migrationBuilder.CreateIndex(
                name: "IX_fact_call_outcome_loaded_at",
                schema: "analytics",
                table: "fact_call_outcome",
                column: "loaded_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agg_kpi_daily",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "dim_program",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "dim_result_type",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "dim_script_variant",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "etl_checkpoint",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "fact_call_job",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "fact_call_outcome",
                schema: "analytics");
        }
    }
}

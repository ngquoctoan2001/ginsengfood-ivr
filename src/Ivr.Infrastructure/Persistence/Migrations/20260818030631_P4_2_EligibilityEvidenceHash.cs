using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861, IDE0161 // EF-generated migration shape.

namespace Ivr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P4_2_EligibilityEvidenceHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "eligibility_snapshot_hash",
                table: "ivr_confirmation_tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_ivr_confirmation_tasks_eligibility_hash",
                table: "ivr_confirmation_tasks",
                sql: "eligibility_snapshot_hash IS NULL OR eligibility_snapshot_hash ~ '^[a-f0-9]{64}$'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_ivr_confirmation_tasks_eligibility_hash",
                table: "ivr_confirmation_tasks");

            migrationBuilder.DropColumn(
                name: "eligibility_snapshot_hash",
                table: "ivr_confirmation_tasks");
        }
    }
}

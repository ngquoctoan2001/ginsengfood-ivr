using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class W0139ObservabilityTraceContext : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.AddColumn<string>(
            name: "trace_parent",
            table: "ivr_confirmation_tasks",
            type: "character varying(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "trace_state",
            table: "ivr_confirmation_tasks",
            type: "character varying(512)",
            maxLength: 512,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropColumn(
            name: "trace_parent",
            table: "ivr_confirmation_tasks");

        migrationBuilder.DropColumn(
            name: "trace_state",
            table: "ivr_confirmation_tasks");
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class W0111CallTerminationRequest : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.AddColumn<string>(
            name: "termination_reason",
            table: "ivr_call_attempts",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "termination_requested_at",
            table: "ivr_call_attempts",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "termination_requested_by",
            table: "ivr_call_attempts",
            type: "character varying(120)",
            maxLength: 120,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ivr_call_attempts_termination_requested_at",
            table: "ivr_call_attempts",
            column: "termination_requested_at",
            filter: "termination_requested_at IS NOT NULL");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_termination_complete",
            table: "ivr_call_attempts",
            sql: "(termination_requested_at IS NULL AND termination_requested_by IS NULL AND termination_reason IS NULL) OR (termination_requested_at IS NOT NULL AND termination_requested_by IS NOT NULL AND termination_reason IS NOT NULL)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropIndex(
            name: "IX_ivr_call_attempts_termination_requested_at",
            table: "ivr_call_attempts");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_termination_complete",
            table: "ivr_call_attempts");

        migrationBuilder.DropColumn(
            name: "termination_reason",
            table: "ivr_call_attempts");

        migrationBuilder.DropColumn(
            name: "termination_requested_at",
            table: "ivr_call_attempts");

        migrationBuilder.DropColumn(
            name: "termination_requested_by",
            table: "ivr_call_attempts");
    }
}

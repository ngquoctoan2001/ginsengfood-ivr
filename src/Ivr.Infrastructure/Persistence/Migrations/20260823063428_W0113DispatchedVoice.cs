using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0113. Records the voice an attempt dialled with, instead of re-deriving it at read time.
/// <para>
/// Purely additive and nullable, so a rolling deploy is safe in both directions: older code
/// ignores the three columns, and every attempt made before this migration keeps a null voice —
/// which the read path reports as "derived", never as "no voice".
/// </para>
/// </summary>
public partial class W0113DispatchedVoice : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.AddColumn<string>(
            name: "voice_id",
            table: "ivr_call_attempts",
            type: "character varying(120)",
            maxLength: 120,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "voice_region",
            table: "ivr_call_attempts",
            type: "character varying(16)",
            maxLength: 16,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "voice_region_resolved",
            table: "ivr_call_attempts",
            type: "boolean",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ivr_call_attempts_voice_region",
            table: "ivr_call_attempts",
            column: "voice_region",
            filter: "voice_region IS NOT NULL");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_voice_complete",
            table: "ivr_call_attempts",
            sql: "(voice_id IS NULL AND voice_region IS NULL AND voice_region_resolved IS NULL) OR (voice_id IS NOT NULL AND voice_region IS NOT NULL AND voice_region_resolved IS NOT NULL)");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_voice_region",
            table: "ivr_call_attempts",
            sql: "voice_region IS NULL OR voice_region IN ('North', 'Central', 'South')");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropIndex(
            name: "IX_ivr_call_attempts_voice_region",
            table: "ivr_call_attempts");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_voice_complete",
            table: "ivr_call_attempts");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_voice_region",
            table: "ivr_call_attempts");

        migrationBuilder.DropColumn(
            name: "voice_id",
            table: "ivr_call_attempts");

        migrationBuilder.DropColumn(
            name: "voice_region",
            table: "ivr_call_attempts");

        migrationBuilder.DropColumn(
            name: "voice_region_resolved",
            table: "ivr_call_attempts");
    }
}

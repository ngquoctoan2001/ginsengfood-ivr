using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;
/// <summary>
/// W-0249 / worklist <c>2.5</c>, owner decision <b>B</b> on 2026-09-09. Gives the two dispatch
/// fences something to read.
/// <para>
/// Purely additive: three nullable columns, no backfill, no constraint on existing rows. A task
/// written before this migration reads as "not revoked", which is what it was.
/// </para>
/// <para>
/// <c>revoke_order_version</c> is recorded and echoed, never compared. <c>order_version</c> is an
/// opaque string IVR returns verbatim -- the OpenAPI calls it a stale-result guard snapshot and
/// Order Core owns the ordering -- so IVR cannot tell which of two versions is newer and does not
/// try. Storing it is what lets the party that can, do so.
/// </para>
/// </summary>
public partial class W0249OrderRevocationFence : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.AddColumn<string>(
            name: "revoke_order_version",
            table: "ivr_confirmation_tasks",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "revoke_reason",
            table: "ivr_confirmation_tasks",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "revoked_at",
            table: "ivr_confirmation_tasks",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropColumn(
            name: "revoke_order_version",
            table: "ivr_confirmation_tasks");

        migrationBuilder.DropColumn(
            name: "revoke_reason",
            table: "ivr_confirmation_tasks");

        migrationBuilder.DropColumn(
            name: "revoked_at",
            table: "ivr_confirmation_tasks");
    }
}

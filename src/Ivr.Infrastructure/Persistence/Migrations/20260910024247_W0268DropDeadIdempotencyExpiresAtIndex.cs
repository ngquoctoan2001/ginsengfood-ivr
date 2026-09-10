using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;
/// <summary>
/// W-0268 / B10 in <c>docs/review/2026-09-09-codebase-audit.md</c>. Drops an index that no query
/// could use.
/// <para>
/// <c>ivr_idempotency_keys.expires_at</c> has four write paths -- the task-intake store, the
/// generic Postgres idempotency store, the feature-flag command store and the internal admin
/// service -- and not one of them assigns the column. Nothing reads it either: the only filtered
/// query over this table selects on <c>scope</c> and <c>key</c> and orders by <c>created_at</c>.
/// The index was therefore maintained on every mutating request that takes an idempotency key,
/// to index a column that is always NULL.
/// </para>
/// <para>
/// <c>specs/database/04-indexes.md</c> names exactly one index for this table -- <c>created_at</c>,
/// for the retention/purge scan -- so this removal moves the model towards the spec rather than
/// away from it.
/// </para>
/// <para>
/// The column itself stays. <c>specs/database/02-tables.md</c> declares it, and dropping it would
/// be a <c>DropColumn</c> against a table the previous release still reads -- the case
/// <c>UT-SCHEMA-BACKCOMPAT-01</c> refuses. Dropping an index is invisible to old code, which is
/// why that guard does not list <c>DropIndex</c>.
/// </para>
/// </summary>
public partial class W0268DropDeadIdempotencyExpiresAtIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropIndex(
            name: "IX_ivr_idempotency_keys_expires_at",
            table: "ivr_idempotency_keys");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.CreateIndex(
            name: "IX_ivr_idempotency_keys_expires_at",
            table: "ivr_idempotency_keys",
            column: "expires_at");
    }
}

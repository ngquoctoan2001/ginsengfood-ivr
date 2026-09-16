using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0301 / <c>B4</c>. A live <c>RUNTIME_GATE_ADMIN</c> approval must name the environment it
/// opens, and the unscoped one seeded by <c>W0195</c> is revoked.
/// <para>
/// <c>W0195</c> gave this table an <c>environment</c> column and then seeded a
/// <c>RUNTIME_GATE_ADMIN</c> row with that column <c>NULL</c>, approver <c>'ivr-owner'</c>.
/// <c>PostgresRuntimeGateAuthorization</c> asked only whether <i>any</i> live admin approval
/// existed, so that one row opened runtime-gate administration in every environment reaching this
/// database — while the column that could have said otherwise sat there unread.
/// </para>
/// <para>
/// The previous position, written into <c>RuntimeGateApprovalKinds</c>, was that administration is
/// coarse on purpose because the per-change four-eyes row carries the environment. That argument
/// reads well and is wrong in one specific way: an approver filling in <c>environment = 'lab'</c>
/// would believe they had limited the grant, and would have granted production too. A column
/// nobody reads is worse than no column — it invites a promise the system never made. SIP-04 fixed
/// exactly this for <c>PRODUCTION_CALL</c>; applying the rule to one kind and not the other left
/// the weaker half guarding the kind that opens every flag change.
/// </para>
/// <para>
/// <b>Revoked rather than corrected or removed.</b> <c>trg_ivr_runtime_gate_approvals_append_only</c>
/// refuses to change <c>environment</c> on a granted row and refuses to delete one at all —
/// revocation is the single mutation it permits, and that is the point of an append-only approval
/// log. So the seeded row is revoked, its history intact, and no replacement is seeded: the grant
/// returns when someone inserts a scoped row through a procedure that carries a real approval
/// reference, which is the shape <c>PRODUCTION_CALL</c> already uses.
/// </para>
/// <para>
/// <b>This closes runtime-gate administration everywhere until such a row exists.</b> Risk-
/// increasing feature-flag mutations will be refused in every environment. That is the intended
/// direction and the safe one: <c>FeatureFlagAdminService</c> already allows unconditional risk
/// <i>reduction</i> without consulting this gate, so the kill switch can still be engaged and
/// calls can still be stopped with no approval at all.
/// </para>
/// </summary>
public partial class RuntimeGateAdminApprovalIsEnvironmentScoped : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // Revocation first: the constraint below would refuse the row as it stands, and the
        // append-only trigger forbids every other way of dealing with it.
        migrationBuilder.Sql(
            """
            UPDATE ivr_runtime_gate_approvals
               SET revoked_at = TIMESTAMPTZ '2026-09-16 00:00:00+00',
                   revoked_reason = 'W-0301: granted for no environment, so it granted all of '
                       || 'them; re-grant per environment with a real approval reference'
             WHERE approval_kind = 'RUNTIME_GATE_ADMIN'
               AND environment IS NULL
               AND revoked_at IS NULL;
            """);

        // The predicate says "a LIVE admin approval must name an environment", where the
        // PRODUCTION_CALL constraint says it of every row. The difference is forced rather than
        // chosen: the row revoked above keeps environment NULL for ever, because the append-only
        // trigger will not let anybody change it. Exempting revoked rows costs nothing - a revoked
        // approval opens no environment whatever its columns say - and the invariant that matters
        // is untouched.
        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_runtime_gate_approvals
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_admin_is_scoped
                    CHECK (approval_kind <> 'RUNTIME_GATE_ADMIN'
                           OR environment IS NOT NULL
                           OR revoked_at IS NOT NULL)
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_runtime_gate_approvals
                DROP CONSTRAINT IF EXISTS ck_ivr_runtime_gate_approvals_admin_is_scoped
            """);

        // The revocation is not undone. An approval log whose revocations disappear on rollback is
        // not an approval log, and the trigger refuses a second revocation anyway - so re-granting
        // is a new row with its own reference, not a resurrection of this one.
    }
}

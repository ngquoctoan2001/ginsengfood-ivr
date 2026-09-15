using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// SIP-04. A <c>PRODUCTION_CALL</c> approval must name the environment it opens.
/// <para>
/// <c>W-0195</c> gave the approvals table an <c>environment</c> column and left it nullable,
/// because two of the three kinds are deliberately coarse: runtime-gate administration ignores it,
/// and a flag change is narrowed by the four-eyes fingerprint on each individual change instead.
/// The third kind has neither. <c>PostgresProductionCallGate</c> asked only whether any live
/// <c>PRODUCTION_CALL</c> approval existed, so one signature opened every deployment that could
/// reach this database - a pilot approval authorised production, and the row said nothing to the
/// contrary even though the column to say it was already sitting there unused.
/// </para>
/// <para>
/// The query now matches on the environment, and this is the half that makes it an invariant.
/// Without it, the approval nobody scoped is still insertable and simply stops working, which
/// leaves an approver believing they granted something that silently does nothing - the same class
/// of quiet failure, pointed the other way.
/// </para>
/// <para>
/// Scoped to the one kind that needs it. Widening it to all three would break the coarseness that
/// <c>IT-GATE-APPROVAL-10</c> holds on purpose.
/// </para>
/// <para>
/// This will fail loudly on any database that already holds an unscoped <c>PRODUCTION_CALL</c> row.
/// No migration seeds one and none should, so a deployment that has one has a row authorising real
/// customer calls that nobody scoped - which is worth a stopped deploy and a person looking at it,
/// rather than a constraint quietly declared NOT VALID and carried forward.
/// </para>
/// </summary>
public partial class ProductionCallApprovalIsEnvironmentScoped : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_runtime_gate_approvals
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_production_call_is_scoped
                    CHECK (approval_kind <> 'PRODUCTION_CALL' OR environment IS NOT NULL)
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_runtime_gate_approvals
                DROP CONSTRAINT IF EXISTS
                    ck_ivr_runtime_gate_approvals_production_call_is_scoped
            """);
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0365 / Q-28 (PA2, 2026-09-26). Two approval kinds for the production dial path, which until
/// now had one: <c>PRODUCTION_CALL</c> said whether an environment's release may ring customers at
/// all, and nothing said whom.
/// <para>
/// <c>PRODUCTION_PILOT_LIST</c> binds one exact pilot list: <c>change_fingerprint</c> is the list's
/// hash, and the row must name its proposer, whom the existing four-eyes check keeps apart from
/// the approver. Without a proposer the four-eyes check passes trivially, so the binding constraint
/// requires one, as <c>ck_ivr_runtime_gate_approvals_change_binding</c> does for flag changes.
/// </para>
/// <para>
/// <c>PRODUCTION_CALL_OPEN</c> is the signed decision that moves an environment past the pilot.
/// Scoped like <c>PRODUCTION_CALL</c>: a row naming no environment would open nothing, so the
/// database refuses to store one rather than let an approver believe they had granted something.
/// </para>
/// <para>
/// No row of either kind is seeded. Both stay append-only under the W0195 trigger.
/// </para>
/// </summary>
public partial class ProductionPilotApprovalKinds : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_runtime_gate_approvals
                DROP CONSTRAINT ck_ivr_runtime_gate_approvals_kind,
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_kind
                    CHECK (approval_kind IN (
                        'RUNTIME_GATE_ADMIN', 'FEATURE_FLAG_CHANGE', 'PRODUCTION_CALL',
                        'PRODUCTION_PILOT_LIST', 'PRODUCTION_CALL_OPEN')),
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_pilot_list_binding
                    CHECK (approval_kind <> 'PRODUCTION_PILOT_LIST'
                           OR (change_fingerprint IS NOT NULL
                               AND proposer_actor_id IS NOT NULL
                               AND environment IS NOT NULL)),
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_open_is_scoped
                    CHECK (approval_kind <> 'PRODUCTION_CALL_OPEN' OR environment IS NOT NULL)
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // Fails, on purpose, on a database that ever stored a row of either new kind: the
        // append-only trigger will not delete it, revoked or not, and the narrower kind check
        // cannot hold it. Rolling back past a pilot that was actually approved is not something
        // a schema step should be able to do quietly.
        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_runtime_gate_approvals
                DROP CONSTRAINT IF EXISTS ck_ivr_runtime_gate_approvals_open_is_scoped,
                DROP CONSTRAINT IF EXISTS ck_ivr_runtime_gate_approvals_pilot_list_binding,
                DROP CONSTRAINT ck_ivr_runtime_gate_approvals_kind,
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_kind
                    CHECK (approval_kind IN (
                        'RUNTIME_GATE_ADMIN', 'FEATURE_FLAG_CHANGE', 'PRODUCTION_CALL'))
            """);
    }
}

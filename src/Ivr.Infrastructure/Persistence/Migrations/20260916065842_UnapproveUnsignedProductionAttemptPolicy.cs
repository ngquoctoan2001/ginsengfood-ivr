using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0300 / <c>B3</c>. Stops a schema migration from shipping production approval for
/// <c>gh-247-prod-v1</c> before the people <c>OD-V1-08</c> names have signed it.
/// <para>
/// <c>W0196</c> inserted both rows with <c>approved_for_production = TRUE</c> from inside a schema
/// migration, so the approval arrives in <b>every</b> database the schema reaches — a developer
/// laptop, the test server, staging and production alike — carried by the same mechanism that
/// creates tables. Running a migration is not a decision, and an approval that appears wherever
/// the schema appears is not one either.
/// </para>
/// <para>
/// The row said so itself: its <c>retention_class</c> is <c>LEGAL_DECISION_PENDING</c>, declaring
/// the decision still open while claiming the approval that decision would grant.
/// <c>m8-11</c> and <c>specs/functional/03-scheduler-attempt-policy.md</c> both place that approval
/// with Product, Order Core and Module 3. <c>OD-V1-08</c> was closed on 2026-09-05 by the IVR owner
/// alone, and Module 3's order side is not finished, so the co-signature cannot exist yet.
/// </para>
/// <para>
/// <b>Why the rows are removed rather than corrected.</b> The first attempt here was
/// <c>UPDATE … SET approved_for_production = FALSE</c>, and the database refused it:
/// <c>trg_ivr_attempt_policies_immutable</c> raises on <b>every</b> update to this table, whatever
/// the column — <c>W-0151</c> enforced in the schema so a version's terms can never change
/// underneath a task admitted under them. The rule is right, so the fix has to respect it rather
/// than work around it: the seeded rows go, and the version returns when someone inserts it
/// through an operational step that carries a real approval reference. That is the shape
/// <c>PRODUCTION_CALL</c> already uses, and it is what <c>B3</c> asked for.
/// </para>
/// <para>
/// <b>Nothing is lost by removing them.</b> The numbers themselves live in
/// <c>SignedProductionAttemptPolicies</c>, unchanged; the sign-off pack and the decision register
/// keep the provenance. No foreign key points at this table, and no task has ever been admitted
/// under this version — production dialling has never been switched on, and mock and lab traffic
/// resolves <c>mock-lab-v1</c>. <c>W0196.Down()</c> deletes exactly these two rows already, so
/// removal is an operation this schema has always allowed.
/// </para>
/// <para>
/// <b>Effect on customer calls today: none.</b> <c>PRODUCTION_REAL</c> is refused independently at
/// <c>IvrOptionsValidator</c>, <c>DispatchGate</c> and <c>SchedulerCapacity</c>, and no production
/// dispatch gateway exists. This pays a paper debt before it becomes a real one; it does not stop
/// a leak.
/// </para>
/// <para>
/// The C# catalogue is deliberately untouched. <c>SignedProductionAttemptPolicies</c> still carries
/// <c>OwnerApproved</c>, and that is reachable only through the in-memory registry, which
/// <c>AddIvrFoundation</c> refuses to build outside <c>MOCK</c> execution — both production hosts
/// pass the default <c>false</c>. Flipping it too would erase a decision record and rewrite five
/// tests for no runtime difference.
/// </para>
/// </summary>
public partial class UnapproveUnsignedProductionAttemptPolicy : Migration
{
    private const string PolicyVersion = "gh-247-prod-v1";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.Sql(
            $"DELETE FROM ivr_attempt_policies WHERE policy_version = '{PolicyVersion}';");

        // No ivr_audit_log row. A migration is not an actor: nobody did this, at no time, in no
        // environment in particular, and the append-only trigger would make the row permanent in
        // every database including every test database. W0196 learned this the same way.
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // Restores exactly what W0196 seeded, so rolling back lands on the state that existed
        // before this migration rather than on a half of it.
        migrationBuilder.Sql(
            $"""
            INSERT INTO ivr_attempt_policies (
                policy_version, program_type, max_attempts, attempt_offsets_seconds_json,
                confirmation_window_seconds, allowed_execution_modes_json,
                approved_for_production, created_at, retention_class)
            VALUES
                ('{PolicyVersion}', 'GOLDEN_HOUR', 2, '[0, 150]'::jsonb, 300,
                 '["MOCK", "LAB_REAL_SIM", "PRODUCTION_REAL"]'::jsonb, TRUE,
                 TIMESTAMPTZ '2026-09-05 00:00:00+00', 'LEGAL_DECISION_PENDING'),
                ('{PolicyVersion}', 'TWENTY_FOUR_SEVEN', 2, '[0, 450]'::jsonb, 900,
                 '["MOCK", "LAB_REAL_SIM", "PRODUCTION_REAL"]'::jsonb, TRUE,
                 TIMESTAMPTZ '2026-09-05 00:00:00+00', 'LEGAL_DECISION_PENDING')
            ON CONFLICT DO NOTHING;
            """);
    }
}

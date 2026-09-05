using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0198 / <c>OD-V1-08</c> + <c>OD-V1-16</c>. Registers the attempt policy the owner signed.
/// <para>
/// A new version, never a promotion of <c>mock-lab-v1</c>. <c>W-0151</c> asked for exactly that:
/// re-approving the candidate in place would change what an already-admitted job was admitted
/// under, and the whole point of an immutable policy version is that it cannot.
/// </para>
/// <para>
/// Registering it does not make it active. Intake resolves the version a task names, so a task
/// still has to arrive carrying <c>gh-247-prod-v1</c> before anything runs on it, and
/// <c>PRODUCTION_REAL</c> remains gated by everything else that gates it. This migration adds a
/// version that <em>may</em> be used in production, not a decision to use it.
/// </para>
/// <para>
/// <b>The class and migration id still say <c>W0196</c> while the ledger entry is now
/// <c>W-0198</c>, and that mismatch is deliberate.</b> This work was authored as W-0196 and
/// renumbered when a concurrent session turned out to have issued the same id; the ledger can be
/// renumbered, a migration id cannot. The id in the attribute below is the string
/// <c>__EFMigrationsHistory</c> stores, so changing it makes an applied migration look unapplied
/// and EF tries to create the table again - which is exactly what happened when the previous
/// collision renamed <c>W0192RuntimeGateApprovals</c>, and would be an unrecoverable chain break
/// on a deployed environment rather than a local annoyance. The name records when the file was
/// written; the ledger records which work item owns it.
/// </para>
/// </summary>
[DbContext(typeof(IvrDbContext))]
[Migration("20260905120000_W0196SignedProductionAttemptPolicy")]
public partial class W0196SignedProductionAttemptPolicy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // Golden Hour: two attempts, the second 150s in, inside a five-minute window.
        // 24/7: two attempts, the second 450s in, inside a fifteen-minute window.
        //
        // Both carry every execution mode because approval, not the mode list, is what keeps a
        // candidate out of production: AttemptPolicySnapshot.EnsureEnvironmentAllowed refuses a
        // CandidateMockLabOnly policy in PRODUCTION_REAL, and this one is OwnerApproved.
        migrationBuilder.Sql(
            """
            INSERT INTO ivr_attempt_policies (
                policy_version, program_type, max_attempts, attempt_offsets_seconds_json,
                confirmation_window_seconds, allowed_execution_modes_json,
                approved_for_production, created_at, retention_class)
            VALUES
                ('gh-247-prod-v1', 'GOLDEN_HOUR', 2, '[0, 150]'::jsonb, 300,
                 '["MOCK", "LAB_REAL_SIM", "PRODUCTION_REAL"]'::jsonb, TRUE,
                 TIMESTAMPTZ '2026-09-05 00:00:00+00', 'LEGAL_DECISION_PENDING'),
                ('gh-247-prod-v1', 'TWENTY_FOUR_SEVEN', 2, '[0, 450]'::jsonb, 900,
                 '["MOCK", "LAB_REAL_SIM", "PRODUCTION_REAL"]'::jsonb, TRUE,
                 TIMESTAMPTZ '2026-09-05 00:00:00+00', 'LEGAL_DECISION_PENDING')
            ON CONFLICT DO NOTHING;
            """);

        // No audit row is written here, and that is a correction of the first draft of this
        // migration rather than an omission.
        //
        // ivr_audit_log records actions actors took in this system. A row inserted by a schema
        // migration is not one: nobody did it, at no time, in no environment in particular - it
        // simply appears in every database the schema reaches, including every test database.
        // Writing one made the audit log non-empty before anything had happened, which is both
        // untrue and the thing several suites reasonably assume is false.
        //
        // The provenance still exists where provenance belongs: signed_decision_ref on the
        // decision register entry, the sign-off pack, and the tracker record for W-0198.
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // The audit row is not removed. An audit trail that disappears when a migration is rolled
        // back is not an audit trail.
        migrationBuilder.Sql(
            "DELETE FROM ivr_attempt_policies WHERE policy_version = 'gh-247-prod-v1';");
    }
}

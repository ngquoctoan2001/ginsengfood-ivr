using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0195 / <c>OD-V1-20</c>. Gives the three runtime gates something to read.
/// <para>
/// The gates shipped as <c>Pending*</c> classes returning a hard-coded <c>false</c>, because no
/// permission existed to move the kill switch or the lab allowlist. The owner signed that
/// permission on 2026-09-05; this migration records the signature as a row, so the answer lives
/// in data an auditor can read and revoke rather than in a constant somebody has to be told about.
/// </para>
/// <para>
/// Three properties are enforced by the database rather than by C#, because a rule the
/// application checks is a rule a future caller can forget: the table is append-only apart from
/// revocation, a four-eyes approval can never name the same actor twice, and an approval for a
/// flag change must carry the fingerprint of the exact change it approves.
/// </para>
/// </summary>
public partial class W0195RuntimeGateApprovals : Migration
{
    private static readonly string[] LiveApprovalIndexColumns =
        ["approval_kind", "revoked_at", "expires_at"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.CreateTable(
            name: "ivr_runtime_gate_approvals",
            columns: table => new
            {
                approval_reference = table.Column<string>(
                    type: "character varying(200)", maxLength: 200, nullable: false),
                approval_kind = table.Column<string>(
                    type: "character varying(40)", maxLength: 40, nullable: false),
                environment = table.Column<string>(
                    type: "character varying(24)", maxLength: 24, nullable: true),
                proposer_actor_id = table.Column<string>(
                    type: "character varying(128)", maxLength: 128, nullable: true),
                approver_actor_id = table.Column<string>(
                    type: "character varying(128)", maxLength: 128, nullable: false),
                change_fingerprint = table.Column<string>(
                    type: "char(64)", nullable: true),
                reason = table.Column<string>(
                    type: "character varying(500)", maxLength: 500, nullable: false),
                signed_decision_ref = table.Column<string>(
                    type: "character varying(200)", maxLength: 200, nullable: false),
                granted_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                revoked_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                revoked_reason = table.Column<string>(
                    type: "character varying(500)", maxLength: 500, nullable: true),
                correlation_id = table.Column<string>(
                    type: "character varying(120)", maxLength: 120, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ivr_runtime_gate_approvals", x => x.approval_reference);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ivr_runtime_gate_approvals_approval_kind_revoked_at_expires~",
            table: "ivr_runtime_gate_approvals",
            columns: LiveApprovalIndexColumns);

        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_runtime_gate_approvals
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_kind
                    CHECK (approval_kind IN (
                        'RUNTIME_GATE_ADMIN', 'FEATURE_FLAG_CHANGE', 'PRODUCTION_CALL')),
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_environment
                    CHECK (environment IS NULL OR environment IN (
                        'dev', 'staging', 'lab', 'pilot', 'prod')),
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_four_eyes
                    CHECK (proposer_actor_id IS NULL
                           OR proposer_actor_id <> approver_actor_id),
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_change_binding
                    CHECK (approval_kind <> 'FEATURE_FLAG_CHANGE'
                           OR (change_fingerprint IS NOT NULL
                               AND proposer_actor_id IS NOT NULL
                               AND environment IS NOT NULL)),
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_expiry
                    CHECK (expires_at IS NULL OR expires_at > granted_at),
                ADD CONSTRAINT ck_ivr_runtime_gate_approvals_revocation
                    CHECK (revoked_at IS NULL OR revoked_reason IS NOT NULL);
            """);

        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION ivr_runtime_gate_approvals_append_only()
            RETURNS trigger AS $w0195$
            BEGIN
                IF TG_OP = 'DELETE' THEN
                    RAISE EXCEPTION
                        'runtime gate approvals are append-only; revoke instead of deleting';
                END IF;

                IF OLD.approval_reference IS DISTINCT FROM NEW.approval_reference
                    OR OLD.approval_kind IS DISTINCT FROM NEW.approval_kind
                    OR OLD.environment IS DISTINCT FROM NEW.environment
                    OR OLD.proposer_actor_id IS DISTINCT FROM NEW.proposer_actor_id
                    OR OLD.approver_actor_id IS DISTINCT FROM NEW.approver_actor_id
                    OR OLD.change_fingerprint IS DISTINCT FROM NEW.change_fingerprint
                    OR OLD.reason IS DISTINCT FROM NEW.reason
                    OR OLD.signed_decision_ref IS DISTINCT FROM NEW.signed_decision_ref
                    OR OLD.granted_at IS DISTINCT FROM NEW.granted_at
                    OR OLD.expires_at IS DISTINCT FROM NEW.expires_at
                    OR OLD.correlation_id IS DISTINCT FROM NEW.correlation_id THEN
                    RAISE EXCEPTION
                        'a granted runtime gate approval is immutable; only revocation may change';
                END IF;

                IF OLD.revoked_at IS NOT NULL
                    AND OLD.revoked_at IS DISTINCT FROM NEW.revoked_at THEN
                    RAISE EXCEPTION 'a revoked runtime gate approval cannot be revoked again';
                END IF;

                RETURN NEW;
            END;
            $w0195$ LANGUAGE plpgsql;
            """);

        migrationBuilder.Sql(
            """
            CREATE TRIGGER trg_ivr_runtime_gate_approvals_append_only
                BEFORE UPDATE OR DELETE ON ivr_runtime_gate_approvals
                FOR EACH ROW EXECUTE FUNCTION ivr_runtime_gate_approvals_append_only();
            """);

        // The signature itself. It authorises ADMINISTRATION of runtime gates, and no individual
        // risk-increasing change: each of those still needs its own four-eyes row. No
        // PRODUCTION_CALL row is seeded, so real dialling stays refused in every environment this
        // migration touches, production included.
        migrationBuilder.Sql(
            """
            INSERT INTO ivr_runtime_gate_approvals (
                approval_reference, approval_kind, environment, proposer_actor_id,
                approver_actor_id, change_fingerprint, reason, signed_decision_ref,
                granted_at, expires_at, revoked_at, revoked_reason, correlation_id)
            VALUES (
                'OD-V1-20/runtime-gate-admin/2026-09-05',
                'RUNTIME_GATE_ADMIN',
                NULL,
                NULL,
                'ivr-owner',
                NULL,
                'OD-V1-20 signed 2026-09-05: IVR_RUNTIME_GATE_ADMIN exists at the danger tier. '
                    || 'Engaging the kill switch needs one person; disengaging it or widening '
                    || 'the lab allowlist needs four eyes.',
                'OD-V1-20@2026-09-05',
                TIMESTAMPTZ '2026-09-05 00:00:00+00',
                NULL, NULL, NULL,
                'od-v1-signoff-2026-09-05')
            ON CONFLICT (approval_reference) DO NOTHING;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            "DROP TRIGGER IF EXISTS trg_ivr_runtime_gate_approvals_append_only "
            + "ON ivr_runtime_gate_approvals;");
        migrationBuilder.Sql(
            "DROP FUNCTION IF EXISTS ivr_runtime_gate_approvals_append_only();");
        migrationBuilder.DropTable(name: "ivr_runtime_gate_approvals");
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// SIP-05. One row per Asterisk application, saying who may hold its event socket.
/// <para>
/// Asterisk lets a second WebSocket take an application from the first and only tells the loser
/// afterwards, with <c>ApplicationReplaced</c>. Nothing in Asterisk can be asked to refuse, so two
/// workers that both believe they should be dialling do not collide on a lock: one quietly stops
/// receiving events for calls it thinks it is running, while the other starts new ones. This table
/// is the condition each worker checks on itself before claiming, and it is honest about being
/// that and not a fence.
/// </para>
/// <para>
/// Its whole value is in refusing an automatic handover. A lease that expired means the holder
/// stopped saying it was alive, which is not the same as the holder having stopped - a node that
/// lost its database connection but kept its ARI socket reports exactly the same way as one that
/// died. So expiry leads to <c>ISOLATED</c>, which a person sets, and not to a vacancy.
/// </para>
/// <para>
/// Four rules are the database's rather than the application's, because a rule C# checks is a rule
/// a future caller can forget. The one that matters most is the third: an isolation cannot be
/// anonymous. Taking an application away from a controller that may still be dialling customers is
/// the most dangerous button in this subsystem, and a row that cannot record who pressed it is a
/// row that should not exist.
/// </para>
/// </summary>
public partial class AriControllerOwnership : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.CreateTable(
            name: "ivr_ari_controller_ownership",
            columns: table => new
            {
                controller_scope = table.Column<string>(
                    type: "character varying(200)", maxLength: 200, nullable: false),
                state = table.Column<string>(
                    type: "character varying(24)", maxLength: 24, nullable: false),
                owner_worker_id = table.Column<string>(
                    type: "character varying(128)", maxLength: 128, nullable: true),
                fencing_generation = table.Column<long>(
                    type: "bigint", nullable: false),
                acquired_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                heartbeat_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                lease_expires_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                released_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                released_reason = table.Column<string>(
                    type: "character varying(500)", maxLength: 500, nullable: true),
                isolated_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                isolated_by_actor_id = table.Column<string>(
                    type: "character varying(128)", maxLength: 128, nullable: true),
                isolated_reason = table.Column<string>(
                    type: "character varying(500)", maxLength: 500, nullable: true),
                requires_reconciliation = table.Column<bool>(
                    type: "boolean", nullable: false),
                reconciled_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: true),
                reconciled_by_actor_id = table.Column<string>(
                    type: "character varying(128)", maxLength: 128, nullable: true),
                correlation_id = table.Column<string>(
                    type: "character varying(120)", maxLength: 120, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ivr_ari_controller_ownership", x => x.controller_scope);
            });

        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_ari_controller_ownership
                ADD CONSTRAINT ck_ivr_ari_controller_ownership_state
                    CHECK (state IN ('VACANT', 'HELD', 'ISOLATED')),

                -- HELD names its holder. A held row with no owner is a scope nobody can renew and
                -- nobody can release, which needs a person to unpick precisely when nobody wants
                -- to be unpicking anything.
                ADD CONSTRAINT ck_ivr_ari_controller_ownership_held_has_owner
                    CHECK (state <> 'HELD' OR owner_worker_id IS NOT NULL),

                -- An isolation cannot be anonymous. This is the transition that lets one worker
                -- take an application away from another that may still be dialling customers, and
                -- the actor is the whole reason it is allowed to exist at all.
                ADD CONSTRAINT ck_ivr_ari_controller_ownership_isolation_is_attributed
                    CHECK (state <> 'ISOLATED' OR (
                        isolated_by_actor_id IS NOT NULL AND isolated_at IS NOT NULL)),

                -- VACANT is the clean state, and it has to look like one: a leftover owner or a
                -- lease still ticking would make a released scope read as a contested one.
                ADD CONSTRAINT ck_ivr_ari_controller_ownership_vacant_is_clear
                    CHECK (state <> 'VACANT' OR (
                        owner_worker_id IS NULL
                        AND lease_expires_at IS NULL
                        AND requires_reconciliation IS FALSE))
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropTable(
            name: "ivr_ari_controller_ownership");
    }
}

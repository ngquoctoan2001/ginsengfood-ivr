using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// SIP-02. Makes the reusable-dial-token ceiling survive a restart.
/// <para>
/// <c>OD-V1-17</c> replaced "one use per attempt" with a ceiling, because policy needs at least two
/// customer dials plus technical retries and no contract anywhere can re-issue a token. The
/// property that bought was that a leaked token still cannot dial more times than policy allows.
/// </para>
/// <para>
/// That property was not actually held. The ledger enforcing it was a <c>ConcurrentDictionary</c>
/// inside whichever vault instance the process happened to own, and it said as much in its own
/// comments - deferring the real bound to "the SIM vault", which was never built. So the count
/// reset on every restart and each worker kept its own: a token policy allowed to dial twice could
/// dial twice per process, indefinitely, as long as processes kept restarting.
/// </para>
/// <para>
/// One row per (token, attempt) rather than a counter, because all three rules read off the rows
/// and a counter answers only one: the count is how many rows there are, the binding is the task on
/// the earliest row, and a replay is a row that already exists. The primary key is what makes the
/// last of those an invariant of the database rather than a check the application remembers to do.
/// </para>
/// <para>
/// <b>No token is stored.</b> The key is a SHA-256 of whatever the vault revealed, so a database
/// dump cannot be turned back into a token or a phone number. The LAB vault already hands over an
/// irreversible fingerprint; hashing again costs nothing there and is what keeps the table safe
/// once a vault that can reverse its own values owns this ledger.
/// </para>
/// </summary>
public partial class DialTokenResolveLedger : Migration
{
    private static readonly string[] TokenAndTimeColumns = ["token_hash", "resolved_at"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.CreateTable(
            name: "ivr_dial_token_resolves",
            columns: table => new
            {
                token_hash = table.Column<string>(
                    type: "char(64)", nullable: false),
                attempt_id = table.Column<string>(
                    type: "character varying(128)", maxLength: 128, nullable: false),
                task_id = table.Column<string>(
                    type: "character varying(128)", maxLength: 128, nullable: false),
                resolved_at = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone", nullable: false),
                max_resolves_snapshot = table.Column<int>(
                    type: "integer", nullable: false),
            },
            constraints: table =>
            {
                // (token, attempt) rather than a surrogate key. Replay protection is then something
                // the database refuses rather than something the application checks first and could
                // one day forget to.
                table.PrimaryKey(
                    "PK_ivr_dial_token_resolves",
                    x => new { x.token_hash, x.attempt_id });
            });

        migrationBuilder.CreateIndex(
            name: "IX_ivr_dial_token_resolves_token_hash_resolved_at",
            table: "ivr_dial_token_resolves",
            columns: TokenAndTimeColumns);

        migrationBuilder.Sql(
            """
            ALTER TABLE ivr_dial_token_resolves
                -- A resolve allowed under a ceiling of zero would be a resolve allowed under no
                -- rule at all, and "missing ceiling" is refused rather than treated as unlimited.
                -- Recorded here too so a row that claims otherwise cannot exist.
                ADD CONSTRAINT ck_ivr_dial_token_resolves_ceiling_positive
                    CHECK (max_resolves_snapshot > 0),

                -- The key is a hash and has to look like one. A vault handing over a raw token by
                -- mistake would write a value of the wrong shape, and this is the cheapest place to
                -- notice that it is not 64 hex characters.
                ADD CONSTRAINT ck_ivr_dial_token_resolves_token_hash_shape
                    CHECK (token_hash ~ '^[0-9A-F]{64}$')
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropTable(
            name: "ivr_dial_token_resolves");
    }
}

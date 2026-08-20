using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861, IDE0161 // EF-generated migration shape.

namespace Ivr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// W-0043. `payload_json` was jsonb, and jsonb stores the MEANING of a document rather than
    /// its bytes -- PostgreSQL reorders keys and normalises whitespace on the way in. The callback
    /// payload is sealed with `payload_sha256` at enqueue and verified again before the HTTP send,
    /// so the text read back has to be the text written. It never was: every callback persisted
    /// through Postgres failed the integrity check and dead-lettered as CALLBACK_PAYLOAD_INVALID
    /// without one request leaving the process.
    ///
    /// This changes the column. It cannot repair rows already written -- their stored text is the
    /// normalised form and no longer matches the hash taken before it was normalised. Those rows
    /// are already terminal (INVALID_DEAD_LETTER), so nothing is lost by leaving them; a row that
    /// was still READY would need re-enqueueing from its result, which is an operational decision
    /// rather than something a migration should make silently.
    /// </summary>
    public partial class P7_1_CallbackPayloadByteExact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raw SQL rather than AlterColumn, because the reverse direction needs a USING clause
            // that AlterColumn does not emit -- PostgreSQL will widen jsonb to text on its own but
            // refuses to narrow text back to jsonb without being told how. Written as a matched
            // pair so the rollback the migration test exercises actually runs.
            migrationBuilder.Sql(
                "ALTER TABLE ivr_result_callbacks "
                + "ALTER COLUMN payload_json TYPE text USING payload_json::text;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This restores the defect. That is what Down means, and pretending otherwise by
            // leaving it empty would make the rollback silently asymmetric.
            migrationBuilder.Sql(
                "ALTER TABLE ivr_result_callbacks "
                + "ALTER COLUMN payload_json TYPE jsonb USING payload_json::jsonb;");
        }

    }
}

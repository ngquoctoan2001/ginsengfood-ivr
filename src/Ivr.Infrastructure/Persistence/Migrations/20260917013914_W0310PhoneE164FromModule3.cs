using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0310, option B. Module 3 sends the customer's number in E.164 instead of a dial token, so the
/// task table gains a column to hold it.
/// </summary>
/// <remarks>
/// <para>
/// <b>This column holds a real phone number in the clear, and that is the decision, not an
/// oversight.</b> Owner chose on 2026-09-17 between keeping the token (Module 3 issues it, Module 8
/// decrypts it with a platform-managed key) and having Module 3 send the number outright. The
/// second removes the key store, removes a token issuer Module 3 has not built, and makes
/// <c>OD-V1-05</c>, <c>OD-V1-17</c> and <c>OD-V1-18</c> moot. The cost is recorded here because
/// this is where it is paid: before this migration a dump of this database leaked no customer
/// numbers, and after it, it does.
/// </para>
/// <para>
/// Memory was not an option. The confirmation window runs 5 to 15 minutes and the second attempt
/// lands at +150s or +450s, so the number has to outlive a worker restart between attempts - which
/// is the same durability requirement that put the resolve ledger in Postgres.
/// </para>
/// <para>
/// <b>Expand only.</b> Nullable, and the three dial-token columns stay exactly as they are. Rows
/// written before today carry a token and no number; Module 3 will keep sending the old shape until
/// they cut over. Dropping the old columns is a later migration, after the far side has moved -
/// <c>migration-expand-baseline.json</c> permits no drop in this phase, and the rule is right: a
/// deploy that removes a column the previous pod still reads takes the service down for the length
/// of the rollout.
/// </para>
/// <para>
/// No CHECK constraint on the format. <c>phone_masked</c> carries one because a mask that is not
/// masked is a defect the database can see; a phone number, by contrast, is validated where it
/// arrives, and a constraint here would only restate that check somewhere it cannot give a useful
/// error.
/// </para>
/// </remarks>
public partial class W0310PhoneE164FromModule3 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.AddColumn<string>(
            name: "phone_e164",
            table: "ivr_confirmation_tasks",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropColumn(
            name: "phone_e164",
            table: "ivr_confirmation_tasks");
    }
}

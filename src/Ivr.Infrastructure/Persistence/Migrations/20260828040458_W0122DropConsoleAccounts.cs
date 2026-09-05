using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// Compatibility bridge: retire the runtime consumer, NOT its stored data.
/// The historical ID is retained so existing EF history remains valid. Databases which already
/// ran the old destructive implementation need the additive P03 compatibility repair; only a
/// verified backup can restore their deleted rows. See docs/database/expand-contract.md.
/// </summary>
public partial class W0122DropConsoleAccounts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        // Keep both tables for old replicas and the rollback window. No auth is re-enabled.
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        // They already exist. Recreating them would fail and cannot recover deleted data.
    }
}

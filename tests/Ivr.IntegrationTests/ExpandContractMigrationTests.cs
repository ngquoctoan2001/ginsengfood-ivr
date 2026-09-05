using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ExpandContractMigrationTests(PostgresPersistenceFixture fixture)
{
    private const string Legacy = "20260827024438_W0118AttemptCountedInvariant";
    private const string Bridge = "20260905034908_W0195RuntimeGateApprovals";

    [Theory]
    [Trait("TestId", "IT-SCHEMA-EXPAND-07")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostgreSqlCopyPreservesRowsAcrossUpgradeRollbackAndForwardRecovery(bool dropAlreadyApplied)
    {
        await using (IvrDbContext source = Context(fixture.ConnectionString))
        {
            await source.Database.EnsureDeletedAsync();
            await source.GetService<IMigrator>().MigrateAsync(Legacy);
            await source.Database.ExecuteSqlRawAsync(SyntheticRows);
            if (dropAlreadyApplied)
            {
                await source.GetService<IMigrator>().MigrateAsync(Bridge);
                // Reproduce the SQL shipped before P0.3 without rewriting EF migration history.
                await source.Database.ExecuteSqlRawAsync("DROP TABLE ivr_console_sessions; DROP TABLE ivr_console_accounts;");
            }
        }

        // Copy an actual PostgreSQL database, not an EF InMemory substitute or a fresh schema.
        NpgsqlConnection.ClearAllPools();
        string copyName = "ivr_expand_" + Guid.NewGuid().ToString("N");
        var sourceSettings = new NpgsqlConnectionStringBuilder(fixture.ConnectionString);
        var adminSettings = new NpgsqlConnectionStringBuilder(fixture.ConnectionString)
        {
            Database = "postgres",
            Pooling = false,
        };
        await using var admin = new NpgsqlConnection(adminSettings.ConnectionString);
        await admin.OpenAsync();
        await using (var clone = new NpgsqlCommand(
            $"CREATE DATABASE \"{copyName}\" TEMPLATE \"{sourceSettings.Database}\"", admin))
        {
            await clone.ExecuteNonQueryAsync();
        }

        try
        {
            sourceSettings.Database = copyName;
            sourceSettings.Pooling = false;
            await using IvrDbContext copy = Context(sourceSettings.ConnectionString);
            await copy.Database.MigrateAsync();
            Assert.Empty(await copy.Database.GetPendingMigrationsAsync());
            int expectedRows = dropAlreadyApplied ? 0 : 1;
            await AssertLegacyRows(copy, expectedRows);
            Assert.Equal(45, await copy.FeatureFlags.CountAsync());

            // Schema rollback is supplemental evidence. Operational rollback uses the old image
            // on the FORWARD schema (the separate two-binary drill), not this Down path.
            await copy.GetService<IMigrator>().MigrateAsync(Legacy);
            await AssertLegacyRows(copy, expectedRows);
            await copy.Database.MigrateAsync();
            await AssertLegacyRows(copy, expectedRows);

            if (dropAlreadyApplied)
            {
                // Empty repair is explicitly not data recovery; verify it can accept the original
                // legacy shape. Recovering prior credentials needs the pre-drop backup instead.
                await copy.Database.ExecuteSqlRawAsync(SyntheticRows);
            }

            await copy.Database.ExecuteSqlRawAsync("UPDATE ivr_console_accounts SET version = version + 1 WHERE username = 'compat.fixture';");
            long version = await copy.Database.SqlQueryRaw<long>(
                "SELECT version AS \"Value\" FROM ivr_console_accounts WHERE username = 'compat.fixture'").SingleAsync();
            Assert.Equal(8, version);

            // FK and uniqueness still protect the repaired schema; no silently weakened shape.
            await Assert.ThrowsAsync<PostgresException>(() => copy.Database.ExecuteSqlRawAsync(
                "DELETE FROM ivr_console_accounts WHERE username = 'compat.fixture';"));
        }
        finally
        {
            // Only the GUID database created by this test is removed; the source stays intact.
            await using var dropCopy = new NpgsqlCommand($"DROP DATABASE \"{copyName}\" WITH (FORCE)", admin);
            await dropCopy.ExecuteNonQueryAsync();
        }

        await using IvrDbContext original = Context(fixture.ConnectionString);
        if (!dropAlreadyApplied)
        {
            await AssertLegacyRows(original, 1);
            Assert.Equal(Legacy, (await original.Database.GetAppliedMigrationsAsync()).Last());
        }
    }

    private static async Task AssertLegacyRows(IvrDbContext context, int expected)
    {
        foreach (string table in new[] { "ivr_console_accounts", "ivr_console_sessions" })
        {
            string countSql = "SELECT count(*)::integer AS \"Value\" FROM " + table;
            int count = await context.Database.SqlQueryRaw<int>(countSql).SingleAsync();
            Assert.Equal(expected, count);
        }

        if (expected > 0)
        {
            string payload = await context.Database.SqlQueryRaw<string>(
                "SELECT password_hash || ':' || version::text AS \"Value\" FROM ivr_console_accounts WHERE username = 'compat.fixture'").SingleAsync();
            Assert.Equal("synthetic-not-a-password:7", payload);
            string token = await context.Database.SqlQueryRaw<string>(
                "SELECT token_hash AS \"Value\" FROM ivr_console_sessions").SingleAsync();
            Assert.Equal(new string('a', 64), token);
        }
    }

    private static IvrDbContext Context(string connectionString) => new(
        new DbContextOptionsBuilder<IvrDbContext>().UseNpgsql(connectionString).Options);

    private const string SyntheticRows = """
        INSERT INTO ivr_console_accounts
            (id, created_at, display_name, failed_login_count, is_builtin, password_changed_at,
             password_hash, retention_class, role, status, updated_at, username, version)
        VALUES ('00000000-0000-0000-0000-000000000196', now(), 'Synthetic compatibility fixture', 0,
                false, now(), 'synthetic-not-a-password', 'TEST', 'Operator', 'DISABLED', now(), 'compat.fixture', 7);
        INSERT INTO ivr_console_sessions
            (id, account_id, created_at, expires_at, retention_class, token_hash)
        VALUES ('00000000-0000-0000-0000-000000000197', '00000000-0000-0000-0000-000000000196',
                now(), now() + interval '1 day', 'TEST', repeat('a', 64));
        """;
}

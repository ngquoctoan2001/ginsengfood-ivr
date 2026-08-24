using System.Net;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Ivr.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0114. The new binary, started against the schema of the migration before this one.
/// <para>
/// The chart applies migrations from a <c>pre-upgrade</c> hook, so this is the window between the
/// hook starting and finishing, and the state of any deploy that skipped hooks or pointed at a
/// database somebody forgot to migrate. What must hold is not "it works" — it cannot work, the
/// columns are not there — but that it fails in the one shape a rolling deploy survives: refuse
/// traffic, stay alive, recover without a restart when the schema catches up.
/// </para>
/// <para>
/// The complementary direction — old code meeting the new schema, which is what
/// <c>helm rollback</c> produces — is <c>UT-SCHEMA-BACKCOMPAT-01</c>. It cannot be checked by
/// running anything here, because the binary that would have to run is the previous release's.
/// </para>
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class SchemaCompatibilityTests(PostgresPersistenceFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-SCHEMA-NEWCODE-01")]
    public async Task TheNewBinaryOnThePreviousSchemaRefusesTrafficStaysAliveAndRecovers()
    {
        (string previous, string latest) = await RebuildAtPreviousMigrationAsync();

        // The real entry point, not a test host assembled here: this has to be the binary that
        // ships, because what is under test is how that binary behaves when its schema is behind.
        await using WebApplicationFactory<Program> baseline = new();
        await using WebApplicationFactory<Program> application =
            Bootstrap(baseline, fixture.ConnectionString);
        using HttpClient client = application.CreateClient();

        // 1. It boots. An options validator that threw, or a model check that ran at startup,
        //    would crash-loop every replica of a rolling deploy instead of merely stalling it --
        //    and `helm upgrade --atomic` reverts a stalled rollout, while a crash-loop burns the
        //    timeout first and takes the old pods down with it.
        foreach (string probe in new[] { "/health/live", "/health/startup" })
        {
            using HttpResponseMessage response = await client.GetAsync(probe);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // 2. It refuses traffic. This is the assertion the whole job exists for: a pod that
        //    reported Healthy here would take load-balancer traffic and answer the first read
        //    with 42703 undefined_column.
        using (HttpResponseMessage notReady = await client.GetAsync("/health/ready"))
        {
            Assert.Equal(HttpStatusCode.ServiceUnavailable, notReady.StatusCode);
            string body = await notReady.Content.ReadAsStringAsync();
            Assert.Contains("schema_behind", body, StringComparison.Ordinal);

            // A readiness body is served to anything that asks. "Behind by what" describes the
            // build, and the build is not the caller's business.
            Assert.DoesNotContain(previous, body, StringComparison.Ordinal);
            Assert.DoesNotContain(latest, body, StringComparison.Ordinal);
            Assert.DoesNotContain("Host=", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        }

        // 3. It recovers on the same process. The migration Job finishing is not an event the API
        //    is told about, so readiness has to re-answer rather than latch. If it latched, every
        //    replica that started before the hook finished would stay out of service until
        //    something restarted it -- which is the rollout stalling for a schema that is already
        //    correct.
        await MigrateAsync(latest);
        using (HttpResponseMessage ready = await client.GetAsync("/health/ready"))
        {
            Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
            Assert.Contains(
                "reachable",
                await ready.Content.ReadAsStringAsync(),
                StringComparison.Ordinal);
        }
    }

    [Fact]
    [Trait("TestId", "IT-SCHEMA-NEWCODE-02")]
    public async Task ThePreviousSchemaIsExactlyOneRealMigrationBehind()
    {
        // The premise of IT-SCHEMA-NEWCODE-01, asserted rather than assumed. That test says the
        // binary refuses traffic when its schema is behind; it proves nothing if the schema is
        // not actually behind, and nothing useful if the migration it is behind by is empty.
        (string previous, string latest) = await RebuildAtPreviousMigrationAsync();

        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using (IvrDbContext context = await factory.CreateDbContextAsync())
        {
            // Exactly one step back, and it is the newest migration -- not two, and not none.
            string[] pending = [.. await context.Database.GetPendingMigrationsAsync()];
            Assert.Equal([latest], pending);
            Assert.NotEqual(previous, latest);

            string[] applied = [.. await context.Database.GetAppliedMigrationsAsync()];
            Assert.Equal(previous, applied[^1]);

            // And the step is a real one. An empty migration would leave the two schemas
            // identical, and every assertion about "the previous schema" would then be an
            // assertion about the current one wearing a different name.
            IMigrationsAssembly migrations = context.GetService<IMigrationsAssembly>();
            Migration newest = migrations.CreateMigration(
                migrations.Migrations[latest],
                ActiveProvider);
            Assert.NotEmpty(newest.UpOperations);

            // And why refusing traffic is the required behaviour rather than an inconvenience.
            // For every existing table this release adds a column to, the query EF emits for it
            // names that column, so the read fails outright -- 42703 undefined_column -- rather
            // than returning a row with the new field blank. A pod that answered Healthy here
            // would serve that error to a caller.
            //
            // Conditional because it has to be: a release that only creates new tables leaves
            // every existing read working, and demanding a failure would be demanding the wrong
            // thing. Today this covers ivr_call_attempts, which W-0113 added three columns to.
            foreach (IEntityType entity in ReadableTablesChangedBy(newest, context.Model))
            {
                PostgresException failure = await Assert.ThrowsAsync<PostgresException>(
                    () => context.Database.ExecuteSqlRawAsync(SelectEveryColumn(entity)));
                Assert.Equal(UndefinedColumn, failure.SqlState);
            }
        }

        await MigrateAsync(latest);
    }

    /// <summary>PostgreSQL <c>42703 undefined_column</c>.</summary>
    private const string UndefinedColumn = "42703";

    /// <summary>
    /// Tables the migration adds columns to that an entity also reads back.
    /// </summary>
    private static IReadOnlyList<IEntityType> ReadableTablesChangedBy(Migration migration, IModel model)
    {
        HashSet<string> touched = migration.UpOperations
            .OfType<AddColumnOperation>()
            .Select(operation => operation.Table)
            .ToHashSet(StringComparer.Ordinal);

        return [.. model.GetEntityTypes()
            .Where(entity => entity.GetTableName() is { } table && touched.Contains(table))];
    }

    /// <summary>
    /// The column list EF itself would emit for the entity, so the failure under test is the one
    /// a real read produces and not one this test invented.
    /// </summary>
    private static string SelectEveryColumn(IEntityType entity)
    {
        StoreObjectIdentifier store = StoreObjectIdentifier.Table(
            entity.GetTableName()!,
            entity.GetSchema());
        string columns = string.Join(
            ", ",
            entity.GetProperties()
                .Select(property => property.GetColumnName(store))
                .Where(column => !string.IsNullOrEmpty(column))
                .Select(column => "\"" + column + "\""));
        return "SELECT " + columns + " FROM \"" + entity.GetTableName() + "\" LIMIT 1";
    }

    private const string ActiveProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";

    // WithWebHostBuilder returns a second factory and leaves the first one owning a host, so the
    // caller has to hold both -- the same shape IT-BOOT-02 uses.
    private static WebApplicationFactory<Program> Bootstrap(
        WebApplicationFactory<Program> baseline,
        string connectionString) =>
        baseline.WithWebHostBuilder(builder =>
        {
            builder.UseSetting(
                OrderCoreAllowlistOptions.TokenConfigurationKey,
                FoundationApiTestApplication.ServiceToken);
            builder.UseSetting(
                InternalServiceOptions.TokenConfigurationKey,
                InternalAdminApiTestApplication.InternalToken);
            builder.UseSetting("ConnectionStrings:IvrDb", connectionString);
        });

    /// <summary>
    /// Drops the database and builds it forward to the migration before the newest one.
    /// <para>
    /// Forward from empty, not backward from current. A rollback through <c>Down</c> would test
    /// the down migrations instead, and the schema a deploy actually meets is the one that was
    /// built by applying the previous release's migrations in order.
    /// </para>
    /// <para>
    /// The target is read from the assembly every run. Naming a migration here would pin the gate
    /// to one pair, and the pair it is meant to check is always the newest two.
    /// </para>
    /// </summary>
    private async Task<(string Previous, string Latest)> RebuildAtPreviousMigrationAsync()
    {
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext context = await factory.CreateDbContextAsync();

        string[] all = [.. context.Database.GetMigrations()];
        Assert.True(all.Length >= 2, "A one-step-back check needs at least two migrations.");
        string previous = all[^2];
        string latest = all[^1];

        await context.Database.EnsureDeletedAsync();
        await context.GetService<IMigrator>().MigrateAsync(previous);
        return (previous, latest);
    }

    private async Task MigrateAsync(string target)
    {
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        await context.GetService<IMigrator>().MigrateAsync(target);
    }
}

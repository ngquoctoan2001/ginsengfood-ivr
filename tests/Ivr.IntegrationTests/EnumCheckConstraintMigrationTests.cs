using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0115. Proves the migration reads N-1 data before tightening the schema and that the
/// installed constraints match the reviewed set exactly.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class EnumCheckConstraintMigrationTests(PostgresPersistenceFixture fixture)
{
    private static readonly string[] ExpectedConstraints =
    [
        "ck_ivr_call_attempts_result_status",
        "ck_ivr_call_attempts_status",
        "ck_ivr_call_jobs_eligibility_decision",
        "ck_ivr_call_jobs_queue_status",
        "ck_ivr_call_jobs_status",
        "ck_ivr_call_results_final_matches_type",
        "ck_ivr_call_results_recommended_core_action",
        "ck_ivr_call_results_result_type",
        "ck_ivr_capacity_incidents_scope",
        "ck_ivr_capacity_incidents_status",
        "ck_ivr_confirmation_tasks_eligibility_decision",
        "ck_ivr_result_callbacks_delivery_status",
        "ck_ivr_result_callbacks_result_state",
        "ck_ivr_result_callbacks_result_status",
        "ck_ivr_review_items_source_type",
        "ck_ivr_review_items_status",
        "ck_ivr_sim_channels_status",
    ];

    [Fact]
    [Trait("TestId", "IT-DBENUM-MIGRATE-05")]
    public async Task LegacyDataIsPreflightedAndEveryReviewedConstraintIsInstalled()
    {
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();

        try
        {
            (string previous, string latest) = await RebuildAtPreviousMigrationAsync(factory);
            Assert.EndsWith("_W0115ClosedEnumChecks", latest, StringComparison.Ordinal);

            await using (IvrDbContext legacy = await factory.CreateDbContextAsync())
            {
                await InsertReviewAsync(legacy, "legacy-source", "LEGACY_OPEN");
                PostgresException blocked = await Assert.ThrowsAsync<PostgresException>(
                    () => legacy.GetService<IMigrator>().MigrateAsync(latest));

                Assert.Equal(PostgresErrorCodes.CheckViolation, blocked.SqlState);
                Assert.Contains("W-0115 enum preflight blocked", blocked.MessageText, StringComparison.Ordinal);
                Assert.Contains("ivr_review_items.source_type=[legacy-source]", blocked.MessageText, StringComparison.Ordinal);
                Assert.Contains("ivr_review_items.status=[LEGACY_OPEN]", blocked.MessageText, StringComparison.Ordinal);
                Assert.Equal(previous, (await legacy.Database.GetAppliedMigrationsAsync()).Last());
            }

            await using (IvrDbContext valid = await factory.CreateDbContextAsync())
            {
                await valid.Database.EnsureDeletedAsync();
                await valid.GetService<IMigrator>().MigrateAsync(previous);
                await InsertReviewAsync(valid, "IVR_OPTOUT_PROPOSAL", "PENDING_CRM");
                await valid.GetService<IMigrator>().MigrateAsync(latest);

                string[] allCheckConstraints = await valid.Database
                    .SqlQueryRaw<string>(
                        """
                        SELECT conname AS "Value"
                        FROM pg_constraint
                        WHERE conname LIKE 'ck_ivr_%'
                        ORDER BY conname
                        """)
                    .ToArrayAsync();
                string[] installed = [.. allCheckConstraints
                    .Where(ExpectedConstraints.Contains)];
                Assert.Equal(ExpectedConstraints, installed);

                await valid.Database.ExecuteSqlRawAsync(
                    "UPDATE ivr_review_items SET status = 'ACCEPTED_BY_CRM' "
                    + "WHERE review_item_id = 'REVIEW-W0115'");

                PostgresException rejected = await Assert.ThrowsAsync<PostgresException>(
                    () => valid.Database.ExecuteSqlRawAsync(
                        "UPDATE ivr_review_items SET status = 'LEGACY_OPEN' "
                        + "WHERE review_item_id = 'REVIEW-W0115'"));
                Assert.Equal(PostgresErrorCodes.CheckViolation, rejected.SqlState);
                Assert.Equal("ck_ivr_review_items_status", rejected.ConstraintName);
            }
        }
        finally
        {
            await fixture.ResetAsync();
        }
    }

    private static async Task<(string Previous, string Latest)> RebuildAtPreviousMigrationAsync(
        IDbContextFactory<IvrDbContext> factory)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        await context.Database.EnsureDeletedAsync();
        string[] migrations = [.. context.Database.GetMigrations()];
        Assert.True(migrations.Length >= 2);
        string previous = migrations[^2];
        string latest = migrations[^1];
        await context.GetService<IMigrator>().MigrateAsync(previous);
        return (previous, latest);
    }

    private static Task<int> InsertReviewAsync(
        IvrDbContext context,
        string sourceType,
        string status) =>
        context.Database.ExecuteSqlInterpolatedAsync($$"""
            INSERT INTO ivr_review_items (
                review_item_id,
                source_type,
                source_id,
                reason,
                status,
                correlation_id,
                created_at,
                retention_class)
            VALUES (
                'REVIEW-W0115',
                {{sourceType}},
                'SOURCE-W0115',
                'W0115_ENUM_PREFLIGHT',
                {{status}},
                'CORR-W0115',
                TIMESTAMPTZ '2026-08-24T00:00:00Z',
                'LEGAL_DECISION_PENDING')
            """);
}

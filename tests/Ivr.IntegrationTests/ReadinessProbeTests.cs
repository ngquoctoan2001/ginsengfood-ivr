using Ivr.Api.Health;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ReadinessProbeTests(PostgresPersistenceFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-OBS-HEALTH-04")]
    public async Task ReadinessIs200WithAReachableDatabaseAnd503WithoutOne()
    {
        // W-0040 / P6-1 §6.4, DO-06. `/health/ready` answered a hardcoded Healthy from P0-1
        // until now. A probe that says yes while the database is gone does not merely fail to
        // help — it keeps a load balancer routing traffic into the failure.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> healthy = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();

        ReadinessReport ready = await new IvrReadinessProbe(healthy)
            .CheckAsync(CancellationToken.None);
        Assert.True(ready.Ready);
        Assert.Equal(200, ready.StatusCode);
        Assert.Contains(ready.Checks, check => check.Name == "database" && check.Ready);

        // Point the probe at a host that is not there. Same code path, unreachable dependency.
        var unreachable = new UnreachableDbContextFactory();
        ReadinessReport down = await new IvrReadinessProbe(unreachable)
            .CheckAsync(CancellationToken.None);

        Assert.False(down.Ready);
        Assert.Equal(503, down.StatusCode);
        ReadinessCheck database = Assert.Single(
            down.Checks,
            check => check.Name == "database");
        Assert.False(database.Ready);
        Assert.Equal("unreachable", database.Reason);

        // The body is served to anything that asks, so no reason may carry a host, a credential
        // or a stack frame — only a fixed phrase.
        Assert.All(down.Checks, check =>
        {
            Assert.DoesNotContain("Host=", check.Reason, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Password", check.Reason, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("at ", check.Reason, StringComparison.Ordinal);
        });
    }

    [Fact]
    [Trait("TestId", "IT-OBS-HEALTH-04")]
    public async Task AReachableButUnmigratedDatabaseIsNotReady()
    {
        // W-0046 named this gap and nothing closed it: the direction where NEW code meets an OLD
        // schema. The chart runs migrations as a pre-upgrade hook, so the happy path is covered by
        // ordering -- but a deploy with hooks skipped, a hook someone disabled, or a developer
        // pointing at an un-migrated database all produce a pod that CONNECTS. Before this check,
        // that pod reported Healthy, took traffic, and failed on the first query against a table
        // that was not there.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();

        await using (IvrDbContext context = await factory.CreateDbContextAsync())
        {
            // A real un-migrated database: reachable, and empty. Dropping the history table alone
            // would be a cheaper fixture and a weaker one -- it would leave the tables in place, so
            // a probe that only read history would pass while a probe that queried would too.
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync(CancellationToken.None);
        }

        ReadinessReport report = await new IvrReadinessProbe(factory)
            .CheckAsync(CancellationToken.None);

        Assert.False(report.Ready);
        Assert.Equal(503, report.StatusCode);
        ReadinessCheck database = Assert.Single(report.Checks, check => check.Name == "database");
        Assert.False(database.Ready);

        // A fixed phrase, and deliberately not the list of pending migrations: a readiness body is
        // served to anything that asks, and the migrations a build expects describe the build.
        Assert.Equal("schema_behind", database.Reason);

        // And the control: migrate, and the same probe says yes. Without this the assertion above
        // would also pass on a probe that had simply stopped working.
        await fixture.ResetAsync();
        ReadinessReport migrated = await new IvrReadinessProbe(factory)
            .CheckAsync(CancellationToken.None);
        Assert.True(migrated.Ready);
        Assert.Equal("reachable", Assert.Single(
            migrated.Checks,
            check => check.Name == "database").Reason);
    }

    private sealed class UnreachableDbContextFactory : IDbContextFactory<IvrDbContext>
    {
        public IvrDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<IvrDbContext>()
                .UseNpgsql("Host=127.0.0.1;Port=1;Database=absent;Username=none;Password=none;Timeout=1")
                .Options;
            return new IvrDbContext(options);
        }

        public Task<IvrDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}

using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ivr.UnitTests.Scheduling;

/// <summary>
/// Q-22.2 (2026-09-26). The missed-deadline sweep tells a job that ran out of calling hours from one
/// that ran out of channels only if its store was given the calling hours and the call length.
/// They arrive as optional constructor parameters, so every hand-built store keeps working as
/// before; this pins that the scheduler's own store, the one the container builds, gets both.
/// </summary>
public sealed class MissedDeadlineClassificationTests
{
    private sealed class UnusedFactory : IDbContextFactory<IvrDbContext>
    {
        public IvrDbContext CreateDbContext() =>
            throw new InvalidOperationException("Composition only; no database is opened here.");
    }

    [Fact]
    [Trait("TestId", "UT-SCH-HOURS-RANOUT-02")]
    public void TheSchedulersOwnStoreIsGivenTheCallingHours()
    {
        ServiceCollection services = new();
        services.AddIvrScheduling(
            new ConfigurationBuilder().Build(),
            IvrOptions.MockExecutionMode,
            useMockCapacity: true);
        services.AddSingleton<IDbContextFactory<IvrDbContext>>(new UnusedFactory());
        services.TryAddSingleton(TimeProvider.System);
        using ServiceProvider provider = services.BuildServiceProvider();

        PostgresSchedulerStore composed = Assert.IsType<PostgresSchedulerStore>(
            provider.GetRequiredService<IPostgresSchedulerStore>());

        Assert.True(composed.ClassifiesByCallingHours);
        Assert.False(new PostgresSchedulerStore(new UnusedFactory(), TimeProvider.System)
            .ClassifiesByCallingHours);
    }
}

using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Scheduling;
using Ivr.Worker.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class OperationalMockContinuityTests(PostgresPersistenceFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-MOCK-BOOT-01")]
    public async Task OperationalMockUsesOnePostgresContinuityAndIdempotentSimProvisioning()
    {
        await fixture.ResetAsync();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
                ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
                ["SIM_PROVIDER"] = "MOCK",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                ["ConnectionStrings:IvrDb"] = fixture.ConnectionString,
            })
            .Build();
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);

        await using ServiceProvider provider = services.BuildServiceProvider(
            validateScopes: true);
        Assert.IsType<PostgresTaskIntakeStore>(
            provider.GetRequiredService<ITaskIntakeStore>());
        Assert.IsType<PostgresSchedulerStore>(
            provider.GetRequiredService<IPostgresSchedulerStore>());

        IDbContextFactory<IvrDbContext> factory = provider
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        var provisioner = new MockSimChannelProvisioner(
            factory,
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
            }));

        await provisioner.StartAsync(CancellationToken.None);
        await provisioner.StartAsync(CancellationToken.None);

        await using (IvrDbContext readContext = await factory.CreateDbContextAsync())
        {
            var channel = Assert.Single(await readContext.SimChannels
                .Where(candidate => candidate.SimChannelId == "SIM-MOCK-001")
                .ToListAsync());
            Assert.True(channel.Enabled);
            Assert.Equal("IDLE", channel.Status);
            channel.Enabled = false;
            channel.Status = "DISABLED";
            channel.DisabledReason = "synthetic admin decision";
            await readContext.SaveChangesAsync();
        }

        await provisioner.StartAsync(CancellationToken.None);

        await using IvrDbContext verificationContext = await factory.CreateDbContextAsync();
        var preserved = Assert.Single(await verificationContext.SimChannels
            .Where(candidate => candidate.SimChannelId == "SIM-MOCK-001")
            .ToListAsync());
        Assert.False(preserved.Enabled);
        Assert.Equal("DISABLED", preserved.Status);
        Assert.Equal("synthetic admin decision", preserved.DisabledReason);
    }
}

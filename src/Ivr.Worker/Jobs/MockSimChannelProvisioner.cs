using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

/// <summary>
/// Idempotently provisions the synthetic channel required by the operational MOCK profile.
/// Existing state is never overwritten, so admin disable/quarantine decisions survive restarts.
/// </summary>
public sealed class MockSimChannelProvisioner(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IOptions<IvrOptions> options) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!string.Equals(
                options.Value.ExecutionMode,
                IvrOptions.MockExecutionMode,
                StringComparison.Ordinal))
        {
            return;
        }

        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO ivr_sim_channels (
                sim_channel_id,
                sim_number_ref,
                enabled,
                status,
                adapter_mode,
                execution_mode,
                provider_name,
                fail_count,
                lease_fencing_generation,
                retention_class)
            VALUES (
                'SIM-MOCK-001',
                'mock-sim-primary',
                TRUE,
                'IDLE',
                'MOCK',
                'MOCK',
                'MOCK',
                0,
                0,
                'LEGAL_DECISION_PENDING')
            ON CONFLICT (sim_channel_id) DO NOTHING;
            """,
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

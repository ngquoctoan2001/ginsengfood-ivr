using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Telephony;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

/// <summary>
/// Idempotently provisions the single Asterisk softphone lab channel. Existing
/// operational state is preserved across worker restarts.
/// </summary>
public sealed class AsteriskLabChannelProvisioner(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IOptions<IvrOptions> ivrOptions,
    IOptions<AsteriskAriOptions> ariOptions) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!ariOptions.Value.Enabled
            || !string.Equals(
                ivrOptions.Value.ExecutionMode,
                IvrOptions.LabRealSimExecutionMode,
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
                'SIM-ASTERISK-001',
                'asterisk-softphone-lab-a',
                TRUE,
                'IDLE',
                'ASTERISK_ARI',
                'LAB_REAL_SIM',
                'ASTERISK_ARI',
                0,
                0,
                'LEGAL_DECISION_PENDING')
            ON CONFLICT (sim_channel_id) DO NOTHING;
            """,
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

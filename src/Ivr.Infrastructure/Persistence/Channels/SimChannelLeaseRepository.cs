using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Ivr.Infrastructure.Persistence.Channels;

public sealed record SimChannelLease(
    string SimChannelId,
    Guid LeaseToken,
    long FencingGeneration,
    DateTimeOffset ExpiresAt,
    string AdapterMode,
    string ProviderName);

public interface ISimChannelLeaseRepository
{
    public Task<SimChannelLease?> AcquireAsync(
        string workerId,
        string executionMode,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    public Task<bool> ReleaseAsync(
        string simChannelId,
        Guid leaseToken,
        long fencingGeneration,
        CancellationToken cancellationToken = default);
}

public sealed class SimChannelLeaseRepository(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : ISimChannelLeaseRepository
{
    public async Task<SimChannelLease?> AcquireAsync(
        string workerId,
        string executionMode,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(executionMode);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            leaseDuration,
            TimeSpan.Zero);

        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.Add(leaseDuration);
        Guid token = Guid.NewGuid();
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = """
            WITH candidate AS (
              SELECT sim_channel_id
              FROM ivr_sim_channels
              WHERE enabled IS TRUE
                AND execution_mode = @execution_mode
                AND status IN ('IDLE', 'RESERVED')
                AND (lease_token IS NULL OR lease_expires_at < @now)
                AND (cooldown_until IS NULL OR cooldown_until <= @now)
                AND (quarantine_until IS NULL OR quarantine_until <= @now)
              ORDER BY fail_count, sim_channel_id
              FOR UPDATE SKIP LOCKED
              LIMIT 1
            )
            UPDATE ivr_sim_channels channel
            SET status = 'RESERVED',
                lease_token = @lease_token,
                lease_fencing_generation = lease_fencing_generation + 1,
                leased_by_worker_id = @worker_id,
                lease_acquired_at = @now,
                lease_expires_at = @expires_at
            FROM candidate
            WHERE channel.sim_channel_id = candidate.sim_channel_id
            RETURNING channel.sim_channel_id,
                      channel.lease_fencing_generation,
                      channel.adapter_mode,
                      channel.provider_name;
            """;
        AddParameter(command, "execution_mode", executionMode);
        AddParameter(command, "now", now);
        AddParameter(command, "lease_token", token);
        AddParameter(command, "worker_id", workerId);
        AddParameter(command, "expires_at", expiresAt);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            await reader.DisposeAsync();
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        SimChannelLease lease = new(
            reader.GetString(0),
            token,
            reader.GetInt64(1),
            expiresAt,
            reader.GetString(2),
            reader.GetString(3));
        await reader.DisposeAsync();
        await transaction.CommitAsync(cancellationToken);
        return lease;
    }

    public async Task<bool> ReleaseAsync(
        string simChannelId,
        Guid leaseToken,
        long fencingGeneration,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(simChannelId);
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        int changed = await dbContext.Database.ExecuteSqlInterpolatedAsync($$"""
            UPDATE ivr_sim_channels
            SET status = 'IDLE',
                active_call_job_id = NULL,
                lease_token = NULL,
                leased_by_worker_id = NULL,
                lease_acquired_at = NULL,
                lease_expires_at = NULL
            WHERE sim_channel_id = {{simChannelId}}
              AND lease_token = {{leaseToken}}
              AND lease_fencing_generation = {{fencingGeneration}}
            """, cancellationToken);
        return changed == 1;
    }

    private static void AddParameter(
        System.Data.Common.DbCommand command,
        string name,
        object value)
    {
        var parameter = new NpgsqlParameter(name, value);
        command.Parameters.Add(parameter);
    }
}

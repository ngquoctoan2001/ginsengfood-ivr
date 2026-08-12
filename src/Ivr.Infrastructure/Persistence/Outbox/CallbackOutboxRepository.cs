using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Persistence.Outbox;

public sealed record CallbackOutboxMessage(
    string CallbackId,
    string IdempotencyKey,
    string PayloadJson,
    string PayloadSha256,
    int RetryCount,
    string LeaseToken);

public interface ICallbackOutboxRepository
{
    public Task<ResultCallbackEntity> EnqueueAsync(
        ResultCallbackEntity callback,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<CallbackOutboxMessage>> DequeueReadyAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);
}

public sealed class CallbackOutboxRepository(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : ICallbackOutboxRepository
{
    public async Task<ResultCallbackEntity> EnqueueAsync(
        ResultCallbackEntity callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        PiiGuard.EnsureSafeText(callback.PayloadJson);
        string expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(callback.PayloadJson)));
        if (!string.Equals(expectedHash, callback.PayloadSha256, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Callback payload hash does not match its immutable payload.");
        }

        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        dbContext.ResultCallbacks.Add(callback);
        await dbContext.SaveChangesAsync(cancellationToken);
        return callback;
    }

    public async Task<IReadOnlyList<CallbackOutboxMessage>> DequeueReadyAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (batchSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            leaseDuration,
            TimeSpan.Zero);

        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset leaseExpiresAt = now.Add(leaseDuration);
        string leaseToken = $"callback-lease-{Guid.NewGuid():N}";
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken);

        List<ResultCallbackEntity> rows = await dbContext.ResultCallbacks
            .FromSqlInterpolated($$"""
                SELECT *
                FROM ivr_result_callbacks
                WHERE delivery_status IN ('READY', 'RETRY_PENDING')
                  AND (next_retry_at IS NULL OR next_retry_at <= {{now}})
                  AND (lease_token IS NULL OR lease_expires_at < {{now}})
                ORDER BY COALESCE(next_retry_at, created_at), created_at
                FOR UPDATE SKIP LOCKED
                LIMIT {{batchSize}}
                """)
            .ToListAsync(cancellationToken);

        foreach (ResultCallbackEntity row in rows)
        {
            row.DeliveryStatus = "SENDING";
            row.LeaseToken = leaseToken;
            row.LeaseExpiresAt = leaseExpiresAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return rows.Select(row => new CallbackOutboxMessage(
                row.CallbackId,
                row.IdempotencyKey,
                row.PayloadJson,
                row.PayloadSha256,
                row.RetryCount,
                leaseToken))
            .ToArray();
    }
}

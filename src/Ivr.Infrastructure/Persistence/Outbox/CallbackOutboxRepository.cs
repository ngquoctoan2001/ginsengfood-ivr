using System.Data;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Persistence.Outbox;

public sealed record CallbackOutboxMessage(
    string CallbackId,
    string TaskId,
    string OfficialOrderId,
    string ProgramCode,
    string CorrelationId,
    string IdempotencyKey,
    string PayloadJson,
    string PayloadSha256,
    int RetryCount,
    string LeaseToken)
{
    public string? TraceParent { get; init; }

    public string? TraceState { get; init; }
}

public sealed record CallbackDeliveryUpdate(
    string DeliveryStatus,
    int? CoreHttpStatus,
    string? CoreResponseCode,
    string? LastError,
    int RetryCount,
    DateTimeOffset? NextRetryAt,
    bool Acknowledged,
    bool RequiresReview);

public interface ICallbackOutboxRepository
{
    public Task<ResultCallbackEntity> EnqueueAsync(
        ResultCallbackEntity callback,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<CallbackOutboxMessage>> DequeueReadyAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    public Task<bool> CompleteDeliveryAsync(
        string callbackId,
        string leaseToken,
        CallbackDeliveryUpdate update,
        CancellationToken cancellationToken = default);
}

public sealed class CallbackOutboxRepository(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : ICallbackOutboxRepository
{
    private static readonly HashSet<string> AllowedDeliveryStatuses = new(
        [
            "DELIVERED_ACCEPTED",
            "DELIVERED_BLOCKED",
            "DELIVERED_REVIEW",
            "REJECTED_STALE",
            "IDEMPOTENCY_CONFLICT",
            "INVALID_DEAD_LETTER",
            "AUTH_REJECTED",
            "RETRY_PENDING",
            "RETRY_EXHAUSTED",
        ],
        StringComparer.Ordinal);

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
                WHERE (
                    delivery_status IN ('READY', 'RETRY_PENDING')
                    AND (next_retry_at IS NULL OR next_retry_at <= {{now}})
                    AND (lease_token IS NULL OR lease_expires_at < {{now}})
                  ) OR (
                    delivery_status = 'SENDING'
                    AND lease_expires_at < {{now}}
                  )
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
        string[] taskIds = [.. rows.Select(row => row.TaskId).Distinct(StringComparer.Ordinal)];
        Dictionary<string, ConfirmationTaskEntity> tasks = await dbContext.ConfirmationTasks
            .AsNoTracking()
            .Where(task => taskIds.Contains(task.TaskId))
            .ToDictionaryAsync(task => task.TaskId, StringComparer.Ordinal, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return rows.Select(row => new CallbackOutboxMessage(
                row.CallbackId,
                row.TaskId,
                row.OfficialOrderId,
                tasks[row.TaskId].ProgramType,
                tasks[row.TaskId].CorrelationId,
                row.IdempotencyKey,
                row.PayloadJson,
                row.PayloadSha256,
                row.RetryCount,
                leaseToken)
        {
            TraceParent = tasks[row.TaskId].TraceParent,
            TraceState = tasks[row.TaskId].TraceState,
        })
            .ToArray();
    }

    public async Task<bool> CompleteDeliveryAsync(
        string callbackId,
        string leaseToken,
        CallbackDeliveryUpdate update,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callbackId);
        ArgumentException.ThrowIfNullOrWhiteSpace(leaseToken);
        ArgumentNullException.ThrowIfNull(update);
        if (update.RetryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(update));
        }

        if (!AllowedDeliveryStatuses.Contains(update.DeliveryStatus)
            || (update.DeliveryStatus == "RETRY_PENDING") != (update.NextRetryAt is not null))
        {
            throw new InvalidOperationException("Callback delivery transition is invalid.");
        }

        if (update.LastError is not null)
        {
            PiiGuard.EnsureSafeText(update.LastError);
        }

        if (update.CoreResponseCode is not null)
        {
            PiiGuard.EnsureSafeText(update.CoreResponseCode);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        ResultCallbackEntity? callback = await dbContext.ResultCallbacks
            .AsNoTracking()
            .SingleOrDefaultAsync(
            row => row.CallbackId == callbackId
                && row.LeaseToken == leaseToken
                && row.DeliveryStatus == "SENDING",
            cancellationToken);
        if (callback is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        bool recordsRetry = update.DeliveryStatus is "RETRY_PENDING" or "RETRY_EXHAUSTED";
        int changed = await dbContext.ResultCallbacks
            .Where(row => row.CallbackId == callbackId
                && row.LeaseToken == leaseToken
                && row.DeliveryStatus == "SENDING")
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(row => row.DeliveryStatus, update.DeliveryStatus)
                    .SetProperty(row => row.SentAt, row => row.SentAt ?? now)
                    .SetProperty(
                        row => row.AcknowledgedAt,
                        update.Acknowledged ? now : (DateTimeOffset?)null)
                    .SetProperty(row => row.CoreHttpStatus, update.CoreHttpStatus)
                    .SetProperty(row => row.CoreResponseCode, update.CoreResponseCode)
                    .SetProperty(row => row.RetryCount, update.RetryCount)
                    .SetProperty(
                        row => row.LastRetryAt,
                        row => recordsRetry ? now : row.LastRetryAt)
                    .SetProperty(row => row.NextRetryAt, update.NextRetryAt)
                    .SetProperty(row => row.LastError, update.LastError)
                    .SetProperty(row => row.LeaseToken, (string?)null)
                    .SetProperty(row => row.LeaseExpiresAt, (DateTimeOffset?)null),
                cancellationToken);
        if (changed != 1)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        ConfirmationTaskEntity task = await dbContext.ConfirmationTasks.AsNoTracking()
            .SingleAsync(candidate => candidate.TaskId == callback.TaskId, cancellationToken);
        dbContext.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = "ivr-callback-dispatcher",
            ActorType = "service",
            Action = "IVR_CALLBACK_DELIVERY_STATE_CHANGED",
            TargetType = "result-callback",
            TargetId = callback.CallbackId,
            Reason = update.DeliveryStatus,
            CorrelationId = task.CorrelationId,
            DataJson = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["delivery_status"] = update.DeliveryStatus,
                ["core_http_status"] = update.CoreHttpStatus,
                ["core_response_code"] = update.CoreResponseCode,
                ["retry_count"] = update.RetryCount,
                ["next_retry_at"] = update.NextRetryAt,
            }),
            CreatedAt = now,
        });
        if (update.RequiresReview)
        {
            string reviewId = string.Concat("REVIEW-CALLBACK-", callback.CallbackId);
            bool exists = await dbContext.ReviewItems.AsNoTracking()
                .AnyAsync(item => item.ReviewItemId == reviewId, cancellationToken);
            if (!exists)
            {
                dbContext.ReviewItems.Add(new ReviewItemEntity
                {
                    ReviewItemId = reviewId,
                    SourceType = "IVR_RESULT_CALLBACK",
                    SourceId = callback.CallbackId,
                    Reason = update.CoreResponseCode
                        ?? update.LastError
                        ?? update.DeliveryStatus,
                    Status = "OPEN",
                    CorrelationId = task.CorrelationId,
                    CreatedAt = now,
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}

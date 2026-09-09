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

    /// <summary>
    /// Recorded on a callback retired because its confirmation task no longer exists. Named
    /// rather than spelled out at each site so an operator searching one of them finds all three:
    /// the row's last error, the audit entry, and the review item.
    /// </summary>
    private const string OrphanedTaskCode = "CALLBACK_TASK_MISSING";

    private const string OrphanCorrelationPrefix = "orphan-callback-";

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

        // Resolved before anything is claimed, because whether a row can be claimed at all
        // depends on whether its task still exists.
        string[] taskIds = [.. rows.Select(row => row.TaskId).Distinct(StringComparer.Ordinal)];
        Dictionary<string, ConfirmationTaskEntity> tasks = await dbContext.ConfirmationTasks
            .AsNoTracking()
            .Where(task => taskIds.Contains(task.TaskId))
            .ToDictionaryAsync(task => task.TaskId, StringComparer.Ordinal, cancellationToken);

        var claimed = new List<(ResultCallbackEntity Row, ConfirmationTaskEntity Task)>(rows.Count);
        foreach (ResultCallbackEntity row in rows)
        {
            // A callback whose task is gone can never become a message: the program code and the
            // correlation id live on the task, not on the callback row. And the task really can
            // be gone -- ivr_result_callbacks.task_id carries no foreign key, and retention
            // deletes ivr_confirmation_tasks under a different data class, with a different
            // period, without consulting this table. An orphan is an expected state here, not
            // corruption.
            //
            // It has to be settled inside this transaction. Claiming it and failing to project it
            // afterwards would leave the row SENDING with a lease nobody owns, and the reclaim
            // arm of the query above would hand the same row back on the next poll, and the one
            // after that. One orphan would stop every callback in the deployment, permanently.
            if (!tasks.TryGetValue(row.TaskId, out ConfirmationTaskEntity? task))
            {
                await DeadLetterOrphanAsync(dbContext, row, now, cancellationToken);
                continue;
            }

            row.DeliveryStatus = "SENDING";
            row.LeaseToken = leaseToken;
            row.LeaseExpiresAt = leaseExpiresAt;
            claimed.Add((row, task));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return [.. claimed.Select(entry => new CallbackOutboxMessage(
                entry.Row.CallbackId,
                entry.Row.TaskId,
                entry.Row.OfficialOrderId,
                entry.Task.ProgramType,
                entry.Task.CorrelationId,
                entry.Row.IdempotencyKey,
                entry.Row.PayloadJson,
                entry.Row.PayloadSha256,
                entry.Row.RetryCount,
                leaseToken)
        {
            TraceParent = entry.Task.TraceParent,
            TraceState = entry.Task.TraceState,
        })];
    }

    /// <summary>
    /// Retires a callback whose confirmation task no longer exists, without claiming it.
    /// <para>
    /// <c>INVALID_DEAD_LETTER</c> is the terminal status the delivery-status check constraint
    /// already allows for a message that can never be sent, so this needs no schema change. The
    /// row leaves the queue instead of circling in it.
    /// </para>
    /// <para>
    /// The review item is the point. Dead-lettering is otherwise silent, and what actually
    /// happened is that a call result never reached Sales and now never will -- that is an
    /// operator's decision to make, not a log line to lose.
    /// </para>
    /// </summary>
    private static async Task DeadLetterOrphanAsync(
        IvrDbContext dbContext,
        ResultCallbackEntity row,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        row.DeliveryStatus = "INVALID_DEAD_LETTER";
        row.LastError = OrphanedTaskCode;
        row.CoreResponseCode = OrphanedTaskCode;
        row.SentAt ??= now;
        row.NextRetryAt = null;
        row.LeaseToken = null;
        row.LeaseExpiresAt = null;

        // The correlation id lived on the task, so there is none left to inherit. A synthetic one
        // keyed to the callback keeps the audit row and the review item joinable to each other and
        // to the only identifier that still exists.
        string correlationId = string.Concat(OrphanCorrelationPrefix, row.CallbackId);
        dbContext.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = "ivr-callback-outbox",
            ActorType = "service",
            Action = "IVR_CALLBACK_DELIVERY_STATE_CHANGED",
            TargetType = "result-callback",
            TargetId = row.CallbackId,
            Reason = OrphanedTaskCode,
            CorrelationId = correlationId,
            DataJson = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["delivery_status"] = row.DeliveryStatus,
                ["task_id"] = row.TaskId,
                ["official_order_id"] = row.OfficialOrderId,
                ["retry_count"] = row.RetryCount,
                ["reason"] = OrphanedTaskCode,
            }),
            CreatedAt = now,
        });

        // Same guard CompleteDeliveryAsync uses, for the same reason: the review id is derived
        // from the callback id, so a row that already raised one must not raise a duplicate key.
        string reviewId = string.Concat("REVIEW-CALLBACK-", row.CallbackId);
        bool exists = await dbContext.ReviewItems.AsNoTracking()
            .AnyAsync(item => item.ReviewItemId == reviewId, cancellationToken);
        if (!exists)
        {
            dbContext.ReviewItems.Add(new ReviewItemEntity
            {
                ReviewItemId = reviewId,
                SourceType = "IVR_RESULT_CALLBACK",
                SourceId = row.CallbackId,
                Reason = OrphanedTaskCode,
                Status = "OPEN",
                CorrelationId = correlationId,
                CreatedAt = now,
            });
        }
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

        // The task can disappear between the claim and the completion: retention deletes
        // ivr_confirmation_tasks and ivr_result_callbacks under separate data classes with
        // separate periods, and no foreign key ties them. SingleAsync here would throw after the
        // update above had already been applied, rolling it back and leaving the row SENDING for
        // the next poll to claim and fail on again. The delivery is the part that matters and it
        // is already recorded; the audit trail loses only the inherited correlation id, which is
        // replaced with one derived from the callback so the entry stays joinable.
        ConfirmationTaskEntity? task = await dbContext.ConfirmationTasks.AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.TaskId == callback.TaskId,
                cancellationToken);
        string correlationId = task?.CorrelationId
            ?? string.Concat(OrphanCorrelationPrefix, callback.CallbackId);
        dbContext.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = "ivr-callback-dispatcher",
            ActorType = "service",
            Action = "IVR_CALLBACK_DELIVERY_STATE_CHANGED",
            TargetType = "result-callback",
            TargetId = callback.CallbackId,
            Reason = update.DeliveryStatus,
            CorrelationId = correlationId,
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
                    CorrelationId = correlationId,
                    CreatedAt = now,
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}

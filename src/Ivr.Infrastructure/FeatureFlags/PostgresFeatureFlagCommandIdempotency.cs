using System.Data;
using System.Text.Json;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Idempotency;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.FeatureFlags;

internal sealed class PostgresFeatureFlagCommandIdempotency(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    FeatureFlagPersistenceSession session,
    TimeProvider timeProvider) : IFeatureFlagCommandIdempotency
{
    private const string Scope = "feature-flags";
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<TResponse> ExecuteAsync<TResponse>(
        string key,
        string payloadHash,
        Func<CancellationToken, Task<TResponse>> factory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadHash);
        ArgumentNullException.ThrowIfNull(factory);
        PiiGuard.EnsureSafeText(key);
        PiiGuard.EnsureSafeText(payloadHash);

        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        await PostgresIdempotencyStore.AcquireKeyLockAsync(
            dbContext,
            $"{Scope}:{key}",
            cancellationToken);
        IdempotencyKeyEntity? existing = await dbContext.IdempotencyKeys.FindAsync(
            [Scope, key],
            cancellationToken);
        if (existing is not null)
        {
            PostgresIdempotencyStore.EnsureSamePayload(existing, payloadHash);
            await transaction.CommitAsync(cancellationToken);
            return PostgresIdempotencyStore.Deserialize<TResponse>(
                existing.ResponseSnapshotJson);
        }

        if (session.Current is not null)
        {
            throw new InvalidOperationException("A feature-flag persistence transaction is already active.");
        }

        session.Current = dbContext;
        try
        {
            TResponse response = await factory(cancellationToken);
            string snapshot = JsonSerializer.Serialize(response, SerializerOptions);
            PiiGuard.EnsureSafeText(snapshot);
            dbContext.IdempotencyKeys.Add(new IdempotencyKeyEntity
            {
                Scope = Scope,
                Key = key,
                PayloadHash = payloadHash,
                ResponseSnapshotJson = snapshot,
                CreatedAt = timeProvider.GetUtcNow(),
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        finally
        {
            session.Current = null;
        }
    }
}

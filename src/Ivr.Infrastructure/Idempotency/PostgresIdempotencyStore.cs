using System.Data;
using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Idempotency;

public sealed class PostgresIdempotencyStore(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IIdempotencyStore
{
    private const string Scope = "foundation";
    private static readonly JsonSerializerOptions SerializerOptions =
        CreateSerializerOptions();

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
        await AcquireKeyLockAsync(dbContext, key, cancellationToken);
        IdempotencyKeyEntity? existing = await dbContext.IdempotencyKeys.FindAsync(
            [Scope, key],
            cancellationToken);
        if (existing is not null)
        {
            EnsureSamePayload(existing, payloadHash);
            await transaction.CommitAsync(cancellationToken);
            return Deserialize<TResponse>(existing.ResponseSnapshotJson);
        }

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

    internal static Task AcquireKeyLockAsync(
        IvrDbContext dbContext,
        string scopedKey,
        CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({scopedKey}, 0))",
            cancellationToken);

    internal static void EnsureSamePayload(
        IdempotencyKeyEntity existing,
        string payloadHash)
    {
        if (!string.Equals(existing.PayloadHash, payloadHash, StringComparison.Ordinal))
        {
            throw IvrErrors.IdempotencyConflict();
        }
    }

    internal static TResponse Deserialize<TResponse>(string snapshot) =>
        JsonSerializer.Deserialize<TResponse>(snapshot, SerializerOptions)
        ?? throw new InvalidOperationException(
            "The stored idempotency response could not be restored.");

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ReadOnlySetJsonConverterFactory());
        return options;
    }
}

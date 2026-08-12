using System.Collections.Concurrent;
using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;

namespace Ivr.Infrastructure.Idempotency;

public sealed class InMemoryIdempotencyStore(TimeProvider timeProvider) : IIdempotencyStore
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly ConcurrentDictionary<string, SemaphoreSlim> keyLocks =
        new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, IdempotencyKeyRecord> records =
        new(StringComparer.Ordinal);

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

        SemaphoreSlim keyLock = keyLocks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (records.TryGetValue(key, out IdempotencyKeyRecord? existing))
            {
                if (!string.Equals(existing.PayloadHash, payloadHash, StringComparison.Ordinal))
                {
                    throw IvrErrors.IdempotencyConflict();
                }

                return JsonSerializer.Deserialize<TResponse>(
                        existing.ResponseSnapshot,
                        SerializerOptions)
                    ?? throw new InvalidOperationException(
                        "The stored idempotency response could not be restored.");
            }

            TResponse response = await factory(cancellationToken).ConfigureAwait(false);
            string snapshot = JsonSerializer.Serialize(response, SerializerOptions);
            PiiGuard.EnsureSafeText(snapshot);
            records[key] = new IdempotencyKeyRecord(
                key,
                payloadHash,
                snapshot,
                timeProvider.GetUtcNow());
            return response;
        }
        finally
        {
            keyLock.Release();
        }
    }
}

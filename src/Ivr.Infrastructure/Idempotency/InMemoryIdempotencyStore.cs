using System.Collections.Concurrent;
using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;

namespace Ivr.Infrastructure.Idempotency;

/// <summary>
/// The MOCK-mode idempotency store, bounded in both of the ways it previously was not.
/// <para>
/// It is a singleton and it is refused outside MOCK — but MOCK is what the shipped image defaults
/// to (<c>IVR_EXECUTION_MODE=MOCK</c> in <c>Dockerfile.api</c>), so this is the store a soak run,
/// a long E2E and a pilot all use. Both of its dictionaries grew without limit: one
/// <see cref="SemaphoreSlim"/> per idempotency key, never removed and never disposed, and a
/// response snapshot per key with no expiry — its <c>CreatedAt</c> was written and read by nothing.
/// </para>
/// </summary>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    // The Postgres store's serializer, not a second one configured to look like it. See
    // IdempotencySerialization for why the two must not be able to drift apart again.
    private static readonly JsonSerializerOptions SerializerOptions =
        IdempotencySerialization.Options;

    /// <summary>
    /// How long a replayable response is kept.
    /// <para>
    /// Chosen here rather than inherited, because there is nothing to inherit: the retention
    /// policy ships with <c>PeriodDays</c> empty, which leaves <c>idempotency_key</c> deliberately
    /// unarmed, so the Postgres table has no configured expiry either. A day is longer than any
    /// client retry window this system has and short enough that a soak run does not accumulate a
    /// week of snapshots.
    /// </para>
    /// <para>
    /// It does change a semantic, and the change is worth stating: a retry arriving after the
    /// window re-executes instead of replaying. That is exactly what retention does to
    /// <c>ivr_idempotency_keys</c> once a period is configured for it — an expiring store is the
    /// design, not a compromise — and the alternative on offer was growing until the process died,
    /// which loses every key at once instead of the oldest one.
    /// </para>
    /// </summary>
    public static readonly TimeSpan DefaultRetentionWindow = TimeSpan.FromDays(1);

    /// <summary>
    /// Ceiling on retained responses, enforced separately from the window so a burst inside one
    /// window cannot exhaust memory before anything is old enough to expire. Oldest go first.
    /// </summary>
    public const int DefaultMaximumRecords = 10_000;

    /// <summary>
    /// Locks are striped over a fixed array rather than allocated per key.
    /// <para>
    /// Per-key locks are what leaked, and the usual repair — reference-count them and remove the
    /// last one out — is a known source of races between one caller releasing and the next
    /// acquiring. An array that never grows cannot leak and has no such race. The cost is that two
    /// unrelated keys sharing a stripe serialise against each other: a little concurrency, no
    /// correctness, because this lock orders callers and decides nothing.
    /// </para>
    /// </summary>
    private const int LockStripes = 256;

    private readonly SemaphoreSlim[] stripes;
    private readonly ConcurrentDictionary<string, IdempotencyKeyRecord> records =
        new(StringComparer.Ordinal);
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan retentionWindow;
    private readonly int maximumRecords;
    private long lastPruneUtcTicks;

    public InMemoryIdempotencyStore(
        TimeProvider timeProvider,
        TimeSpan? retentionWindow = null,
        int maximumRecords = DefaultMaximumRecords)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumRecords, 1);
        if (retentionWindow is { } configured)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(configured, TimeSpan.Zero);
        }

        this.timeProvider = timeProvider;
        this.retentionWindow = retentionWindow ?? DefaultRetentionWindow;
        this.maximumRecords = maximumRecords;

        stripes = new SemaphoreSlim[LockStripes];
        for (int index = 0; index < LockStripes; index++)
        {
            stripes[index] = new SemaphoreSlim(1, 1);
        }

        lastPruneUtcTicks = timeProvider.GetUtcNow().UtcTicks;
    }

    /// <summary>Retained responses. Public so a test can prove the bound rather than assume it.</summary>
    public int Count => records.Count;

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

        SemaphoreSlim keyLock = stripes[StripeOf(key)];
        await keyLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            if (records.TryGetValue(key, out IdempotencyKeyRecord? existing))
            {
                if (existing.CreatedAt <= now - retentionWindow)
                {
                    // Expired between the last sweep and this call, so treated as absent rather
                    // than replayed. Honouring it here would make the expiry depend on how
                    // recently something unrelated happened to trigger a sweep.
                    records.TryRemove(
                        new KeyValuePair<string, IdempotencyKeyRecord>(key, existing));
                }
                else if (!string.Equals(existing.PayloadHash, payloadHash, StringComparison.Ordinal))
                {
                    throw IvrErrors.IdempotencyConflict();
                }
                else
                {
                    return JsonSerializer.Deserialize<TResponse>(
                            existing.ResponseSnapshot,
                            SerializerOptions)
                        ?? throw new InvalidOperationException(
                            "The stored idempotency response could not be restored.");
                }
            }

            TResponse response = await factory(cancellationToken).ConfigureAwait(false);
            string snapshot = JsonSerializer.Serialize(response, SerializerOptions);
            PiiGuard.EnsureSafeText(snapshot);
            records[key] = new IdempotencyKeyRecord(key, payloadHash, snapshot, now);
            Prune(now);
            return response;
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <summary>
    /// Drops expired records, then the oldest of whatever remains above the ceiling.
    /// <para>
    /// A sweep scans the whole dictionary, so it is rate-limited rather than run on every write —
    /// otherwise the store would be quadratic in the length of a soak run. The ceiling check is
    /// deliberately not rate-limited: it exists for exactly the burst that an interval lets past.
    /// </para>
    /// </summary>
    private void Prune(DateTimeOffset now)
    {
        long previous = Interlocked.Read(ref lastPruneUtcTicks);
        bool dueByTime = now.UtcTicks - previous >= retentionWindow.Ticks / 10;
        if (!dueByTime && records.Count <= maximumRecords)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref lastPruneUtcTicks, now.UtcTicks, previous) != previous)
        {
            // Another caller is already sweeping. A second concurrent sweep would be correct but
            // wasted work, and the loser has nothing to wait for.
            return;
        }

        DateTimeOffset cutoff = now - retentionWindow;
        foreach (KeyValuePair<string, IdempotencyKeyRecord> pair in records)
        {
            if (pair.Value.CreatedAt <= cutoff)
            {
                records.TryRemove(pair);
            }
        }

        int overflow = records.Count - maximumRecords;
        if (overflow <= 0)
        {
            return;
        }

        foreach (KeyValuePair<string, IdempotencyKeyRecord> pair in records
                     .OrderBy(entry => entry.Value.CreatedAt)
                     .Take(overflow))
        {
            records.TryRemove(pair);
        }
    }

    /// <summary>
    /// FNV-1a over the key's UTF-16 code units.
    /// <para>
    /// Written out rather than using <see cref="string.GetHashCode()"/>, which is randomised per
    /// process: the stripe a given key lands on would then differ between runs, and a test
    /// asserting that two particular keys do not contend could pass all week and fail in CI.
    /// </para>
    /// </summary>
    internal static int StripeOf(string key)
    {
        const uint offsetBasis = 2166136261;
        const uint prime = 16777619;
        uint hash = offsetBasis;
        foreach (char character in key)
        {
            hash = (hash ^ (byte)character) * prime;
            hash = (hash ^ (byte)(character >> 8)) * prime;
        }

        return (int)(hash % LockStripes);
    }
}

using System.Diagnostics.Metrics;
using Ivr.Domain.Speech;

namespace Ivr.Infrastructure.Speech;

public sealed record TtsUsageSnapshot(
    long ProviderRequests,
    long Characters,
    long CacheHits,
    long CacheMisses,
    long PurgedEntries);

/// <summary>
/// Regional voice routing counters, kept separate from <see cref="TtsUsageSnapshot"/> because
/// that record is a cost/usage view and widening it would churn every caller that asserts on it.
/// </summary>
/// <param name="Unresolved">
/// Calls whose delivery area named no province and fell back. This is a Sales data-quality
/// signal, not a TTS one: if it climbs, master data is drifting away from the 34 units.
/// </param>
public sealed record TtsVoiceRoutingSnapshot(
    long North,
    long Central,
    long South,
    long Unresolved);

/// <summary>
/// Privacy-safe aggregate usage and cost-input metrics for TTS providers.
/// </summary>
public sealed class TtsUsageMeter
{
    private static readonly Meter Meter = new("Ivr.Speech.Tts", "1.0.0");
    private static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>(
        "ivr_tts_provider_requests_total");
    private static readonly Counter<long> CharacterCounter = Meter.CreateCounter<long>(
        "ivr_tts_characters_total");
    private static readonly Counter<long> CacheCounter = Meter.CreateCounter<long>(
        "ivr_tts_cache_operations_total");
    private static readonly Counter<long> PurgeCounter = Meter.CreateCounter<long>(
        "ivr_tts_cache_purged_total");
    private static readonly Counter<long> VoiceSelectedCounter = Meter.CreateCounter<long>(
        "ivr_tts_voice_selected_total");
    private static readonly Counter<long> RegionUnresolvedCounter = Meter.CreateCounter<long>(
        "ivr_tts_region_unresolved_total");

    private long providerRequests;
    private long characters;
    private long cacheHits;
    private long cacheMisses;
    private long purgedEntries;
    private long northSelected;
    private long centralSelected;
    private long southSelected;
    private long regionUnresolved;

    public void RecordProviderRequest(int characterCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(characterCount);
        Interlocked.Increment(ref providerRequests);
        Interlocked.Add(ref characters, characterCount);
        RequestCounter.Add(1);
        CharacterCounter.Add(characterCount);
    }

    public void RecordCache(bool hit)
    {
        if (hit)
        {
            Interlocked.Increment(ref cacheHits);
        }
        else
        {
            Interlocked.Increment(ref cacheMisses);
        }

        CacheCounter.Add(1, new KeyValuePair<string, object?>("result", hit ? "hit" : "miss"));
    }

    public void RecordPurged(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Interlocked.Add(ref purgedEntries, count);
        PurgeCounter.Add(count);
    }

    /// <summary>
    /// Records which regional voice a call was routed to. The region name is a bounded enum, so
    /// it is safe as a metric dimension; the delivery area it was derived from is never emitted.
    /// </summary>
    public void RecordVoiceSelected(VietnamRegion region, bool resolvedFromDeliveryArea)
    {
        switch (region)
        {
            case VietnamRegion.North:
                Interlocked.Increment(ref northSelected);
                break;
            case VietnamRegion.Central:
                Interlocked.Increment(ref centralSelected);
                break;
            case VietnamRegion.South:
                Interlocked.Increment(ref southSelected);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(region));
        }

        VoiceSelectedCounter.Add(
            1,
            new KeyValuePair<string, object?>("region", region.ToString()));
        if (!resolvedFromDeliveryArea)
        {
            Interlocked.Increment(ref regionUnresolved);
            RegionUnresolvedCounter.Add(1);
        }
    }

    public TtsUsageSnapshot Snapshot() => new(
        Interlocked.Read(ref providerRequests),
        Interlocked.Read(ref characters),
        Interlocked.Read(ref cacheHits),
        Interlocked.Read(ref cacheMisses),
        Interlocked.Read(ref purgedEntries));

    public TtsVoiceRoutingSnapshot VoiceRoutingSnapshot() => new(
        Interlocked.Read(ref northSelected),
        Interlocked.Read(ref centralSelected),
        Interlocked.Read(ref southSelected),
        Interlocked.Read(ref regionUnresolved));
}

/// <summary>
/// Fixed-window request/character guard. It is intentionally process-local in P2-9 MOCK.
/// </summary>
public sealed class TtsRequestBudget(TimeProvider timeProvider)
{
    private readonly Lock sync = new();
    private DateTimeOffset windowStartedAt = timeProvider.GetUtcNow();
    private int requests;
    private long characters;

    public bool TryConsume(int characterCount, int maxRequestsPerMinute, int maxCharactersPerMinute)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(characterCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRequestsPerMinute);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCharactersPerMinute);
        lock (sync)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            if (now - windowStartedAt >= TimeSpan.FromMinutes(1))
            {
                windowStartedAt = now;
                requests = 0;
                characters = 0;
            }

            if (requests + 1 > maxRequestsPerMinute
                || characters + characterCount > maxCharactersPerMinute)
            {
                return false;
            }

            requests++;
            characters += characterCount;
            return true;
        }
    }
}

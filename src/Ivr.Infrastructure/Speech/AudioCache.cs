using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Retention;
using Ivr.Domain.Speech;

namespace Ivr.Infrastructure.Speech;

public sealed record AudioCacheKey
{
    private AudioCacheKey(string cacheId)
    {
        CacheId = cacheId;
    }

    public string CacheId { get; }

    public static AudioCacheKey Create(
        string scriptTemplateId,
        string scriptVersion,
        string summaryHash,
        string voiceId,
        string locale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptTemplateId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(summaryHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(voiceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        string canonical = string.Join(
            '\u001f',
            scriptTemplateId,
            scriptVersion,
            summaryHash,
            voiceId,
            locale);
        string cacheId = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        return new AudioCacheKey(cacheId);
    }

    public override string ToString() => "[REDACTED_AUDIO_CACHE_KEY]";
}

public sealed record AudioCacheResult(
    RenderedAudio Audio,
    bool CacheHit,
    DateTimeOffset ExpiresAt);

public interface IAudioCache
{
    public int Count { get; }

    public Task<AudioCacheResult> GetOrCreateAsync(
        AudioCacheKey key,
        DateTimeOffset expiresAt,
        Func<CancellationToken, Task<RenderedAudio>> factory,
        CancellationToken cancellationToken);

    public Task<int> PurgeExpiredAsync(
        DateTimeOffset now,
        bool dryRun,
        CancellationToken cancellationToken);
}

/// <summary>
/// Process-local ephemeral cache. Keys are SHA-256 identities and never contain raw summary data.
/// </summary>
public sealed class AudioCache(TimeProvider timeProvider) : IAudioCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> entries = new(StringComparer.Ordinal);

    public int Count => entries.Count;

    public async Task<AudioCacheResult> GetOrCreateAsync(
        AudioCacheKey key,
        DateTimeOffset expiresAt,
        Func<CancellationToken, Task<RenderedAudio>> factory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DateTimeOffset now = timeProvider.GetUtcNow();
            if (expiresAt <= now)
            {
                throw new TtsSynthesisException(
                    "TTS_CACHE_WINDOW_EXPIRED",
                    "The confirmation window expired before speech synthesis.");
            }

            if (entries.TryGetValue(key.CacheId, out CacheEntry? current))
            {
                if (current.ExpiresAt <= now)
                {
                    entries.TryRemove(new KeyValuePair<string, CacheEntry>(key.CacheId, current));
                    continue;
                }

                current.ShortenExpiration(expiresAt);
                try
                {
                    RenderedAudio cached = await current.Audio.Value.WaitAsync(
                        cancellationToken).ConfigureAwait(false);
                    return new AudioCacheResult(cached, true, current.ExpiresAt);
                }
                catch
                {
                    entries.TryRemove(new KeyValuePair<string, CacheEntry>(key.CacheId, current));
                    throw;
                }
            }

            var created = new CacheEntry(
                expiresAt,
                new Lazy<Task<RenderedAudio>>(
                    () => factory(cancellationToken),
                    LazyThreadSafetyMode.ExecutionAndPublication));
            if (!entries.TryAdd(key.CacheId, created))
            {
                continue;
            }

            try
            {
                RenderedAudio audio = await created.Audio.Value.WaitAsync(
                    cancellationToken).ConfigureAwait(false);
                return new AudioCacheResult(audio, false, created.ExpiresAt);
            }
            catch
            {
                entries.TryRemove(new KeyValuePair<string, CacheEntry>(key.CacheId, created));
                throw;
            }
        }
    }

    public Task<int> PurgeExpiredAsync(
        DateTimeOffset now,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        KeyValuePair<string, CacheEntry>[] expired = entries
            .Where(pair => pair.Value.ExpiresAt <= now)
            .ToArray();
        if (dryRun)
        {
            return Task.FromResult(expired.Length);
        }

        int removed = 0;
        foreach (KeyValuePair<string, CacheEntry> pair in expired)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entries.TryRemove(pair))
            {
                removed++;
            }
        }

        return Task.FromResult(removed);
    }

    private sealed class CacheEntry(DateTimeOffset expiresAt, Lazy<Task<RenderedAudio>> audio)
    {
        private long expiresAtUtcTicks = expiresAt.UtcTicks;

        public Lazy<Task<RenderedAudio>> Audio { get; } = audio;

        public DateTimeOffset ExpiresAt => new(
            Interlocked.Read(ref expiresAtUtcTicks),
            TimeSpan.Zero);

        public void ShortenExpiration(DateTimeOffset requested)
        {
            long requestedTicks = requested.UtcTicks;
            while (true)
            {
                long current = Interlocked.Read(ref expiresAtUtcTicks);
                if (current <= requestedTicks
                    || Interlocked.CompareExchange(
                        ref expiresAtUtcTicks,
                        requestedTicks,
                        current) == current)
                {
                    return;
                }
            }
        }
    }
}

public sealed class SpeechAudioCacheRetentionHook(
    IAudioCache cache,
    TtsUsageMeter usageMeter) : IRetentionPurgeHook
{
    public string Name => "speech_audio_cache";

    public async Task<int> PurgeExpiredAsync(
        DateTimeOffset now,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        int purged = await cache.PurgeExpiredAsync(
            now,
            dryRun,
            cancellationToken).ConfigureAwait(false);
        if (!dryRun)
        {
            usageMeter.RecordPurged(purged);
        }

        return purged;
    }
}

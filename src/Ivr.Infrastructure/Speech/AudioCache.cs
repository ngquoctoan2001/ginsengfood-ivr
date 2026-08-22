using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Retention;
using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

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

    /// <summary>
    /// Key for one variable piece of a call, keyed by what it says rather than by which order
    /// it belongs to.
    /// <para>
    /// This is where hybrid playback earns its cost: two different orders delivering to the same
    /// ward share the delivery-area piece, so the second order plays it without a vendor call
    /// even though the orders have nothing else in common. Keying by <c>summaryHash</c> — the
    /// whole-call identity — would treat them as unrelated and pay twice.
    /// </para>
    /// <para>
    /// The extra <c>segment</c> element gives this a different arity from
    /// <see cref="Create"/>, so a segment key and a whole-call key can never collide.
    /// </para>
    /// </summary>
    public static AudioCacheKey CreateForSegment(
        string scriptTemplateId,
        string scriptVersion,
        string segmentTextHash,
        string voiceId,
        string locale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptTemplateId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(segmentTextHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(voiceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        string canonical = string.Join(
            CanonicalSeparator,
            scriptTemplateId,
            scriptVersion,
            segmentTextHash,
            voiceId,
            locale,
            "segment");
        string cacheId = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        return new AudioCacheKey(cacheId);
    }

    public override string ToString() => "[REDACTED_AUDIO_CACHE_KEY]";

    /// <summary>ASCII unit separator, written numerically so no control byte sits in source.</summary>
    private const char CanonicalSeparator = (char)0x1f;
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
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
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
                    () => factory(CancellationToken.None),
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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
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

/// <summary>
/// Deletes generated dynamic-segment audio once it is older than the speech snapshot retention
/// window.
/// <para>
/// The in-memory cache expiring is not the same thing as the audio being gone: the external
/// provider writes playable files to the media directory, and those outlive the process. Without
/// this the directory grows for as long as the service runs, and it grows with files that speak
/// order values — which is exactly the class of data <c>DF-07</c> puts a clock on.
/// </para>
/// </summary>
public sealed class SpeechMediaFileRetentionHook(
    IOptions<TtsProviderOptions> providerOptions) : IRetentionPurgeHook
{
    public string Name => "speech_media_files";

    public Task<int> PurgeExpiredAsync(
        DateTimeOffset now,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        TtsProviderOptions configured = providerOptions.Value;
        string directory = configured.External.MediaOutputDirectory;
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return Task.FromResult(0);
        }

        DateTimeOffset cutoff = now.AddSeconds(-configured.SpeechSnapshotRetentionSeconds);
        int affected = 0;
        foreach (string path in Directory.EnumerateFiles(directory, "*.sln*"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (File.GetLastWriteTimeUtc(path) > cutoff.UtcDateTime)
            {
                continue;
            }

            affected++;
            if (dryRun)
            {
                continue;
            }

            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // A call currently playing this file holds it open. It ages out on the next
                // pass; failing the whole retention run over one busy file would leave every
                // later file un-purged.
                affected--;
            }
        }

        return Task.FromResult(affected);
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

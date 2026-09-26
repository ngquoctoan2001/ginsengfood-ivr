using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Ports;
using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

public interface ISpeechSynthesisService
{
    public Task<RenderedSpeech> SynthesizeAsync(
        RenderedSpeech renderedSpeech,
        PrivacySafeOrderSummary summary,
        string scriptTemplateId,
        string scriptVersion,
        ExecutionMode executionMode,
        DateTimeOffset confirmationWindowExpiresAt,
        CancellationToken cancellationToken);
}

/// <summary>
/// Applies privacy, approval, cache, budget and timeout controls around an ITtsProvider.
/// </summary>
public sealed class SpeechSynthesisService(
    ITtsProvider provider,
    IAudioCache cache,
    TtsRequestBudget requestBudget,
    TtsUsageMeter usageMeter,
    RegionalVoiceMap regionalVoices,
    IOptions<TtsProviderOptions> providerOptions,
    TimeProvider timeProvider) : ISpeechSynthesisService
{
    /// <summary>
    /// W-0363 / K-48. The assembled call would need more pieces, or play longer, than one call's
    /// audio may (<see cref="RenderedAudio.MaxPlaylistSegments"/>,
    /// <see cref="RenderedAudio.MaxPlaylistDuration"/>).
    /// </summary>
    public const string PlaylistTooLongCode = "TTS_PLAYLIST_TOO_LONG";

    private readonly SpeechPreparationQueue preparationQueue = new();

    private static readonly IReadOnlyDictionary<string, FixedSegmentMediaEntry> EmptyCatalog =
        new Dictionary<string, FixedSegmentMediaEntry>(StringComparer.Ordinal);

    private static readonly IReadOnlyDictionary<string, string> VietnameseProductDictionary =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["KGC Plus"] = "K G C pờ lớt",
            ["VND"] = "đồng Việt Nam",
        };

    public async Task<RenderedSpeech> SynthesizeAsync(
        RenderedSpeech renderedSpeech,
        PrivacySafeOrderSummary summary,
        string scriptTemplateId,
        string scriptVersion,
        ExecutionMode executionMode,
        DateTimeOffset confirmationWindowExpiresAt,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(renderedSpeech);
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptTemplateId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptVersion);
        TtsProviderOptions configured = providerOptions.Value;

        // W-0362 / K-42. Production is recognised from either side: the mode the caller passes, or
        // the one this deployment is configured for. The caller's alone is one hard-coded argument
        // away from being wrong - the Asterisk gateway passed LAB_REAL_SIM on the production path
        // until W-0354 (B13) - and the approval record is the one check that must not be skipped
        // because a caller said so.
        bool production = executionMode == ExecutionMode.ProductionReal
            || string.Equals(
                configured.ExecutionMode,
                ExecutionModes.ProductionReal,
                StringComparison.OrdinalIgnoreCase);
        if (production
            && string.IsNullOrWhiteSpace(configured.ProductionWhitelistApprovalRecord))
        {
            throw IvrErrors.OperationalBlocked(
                "Production TTS is blocked until the Target V1 speech whitelist has an approval record.");
        }

        // W-0363 / K-49. N1, the Tech Lead's transition rule of 24/09: production does not
        // synthesize speech at call time. The same two-sided answer as the approval check above
        // (W-0362 / K-42), so a caller passing the wrong mode cannot bring synthesis back.
        bool runtimeSynthesisAllowed = !production;

        var hints = new Dictionary<string, string>(
            VietnameseProductDictionary,
            StringComparer.Ordinal);
        foreach ((string key, string value) in summary.PronunciationHints)
        {
            hints[key] = value;
        }

        // W-0106: the voice is chosen per order from the delivery area, not read from one global
        // setting. AudioCacheKey already includes VoiceId, so three voices need no cache change —
        // each region gets its own entry for free.
        RegionalVoiceSelection voice = regionalVoices.Resolve(summary.DeliveryArea.Value);
        usageMeter.RecordVoiceSelected(voice.Region, voice.ResolvedFromDeliveryArea);

        TtsOptions request;
        try
        {
            request = TtsOptions.Create(
                summary.Locale,
                voice.VoiceId,
                voice.SpeakingRate,
                hints,
                TimeSpan.FromSeconds(configured.MaxDurationSeconds),
                TimeSpan.FromMilliseconds(configured.TimeoutMilliseconds));
        }
        catch (InvalidOperationException)
        {
            throw IvrErrors.PiiPolicyViolation();
        }

        SpeechScript script = SpeechScript.Create(
            scriptTemplateId,
            scriptVersion,
            renderedSpeech.ExactText,
            renderedSpeech.ContentHash,
            summary.ComputeHash(),
            renderedSpeech.Segments.IsDefaultOrEmpty ? null : renderedSpeech.Segments);
        SpeechPrivacyGuard.EnsureSafe(script, request);
        if (script.ExactText.Length > configured.MaxCharactersPerRequest)
        {
            throw new IvrFailureException(
                IvrErrorCodes.RateLimited,
                "The rendered speech exceeds the configured TTS character bound.");
        }

        RenderedAudio audio = await SynthesizeWithAdmissionAsync(
            script, request, configured, confirmationWindowExpiresAt, runtimeSynthesisAllowed, cancellationToken);

        // W-0113. The selection above is the only place that decides which voice a customer
        // hears; attaching it here is what lets the dispatch loop record that decision instead
        // of leaving the console to re-derive it from configuration that may since have changed.
        return renderedSpeech.WithAudio(audio.WithVoice(DispatchedVoice.Create(
            voice.VoiceId,
            voice.Region,
            voice.ResolvedFromDeliveryArea)));
    }

    private async Task<RenderedAudio> SynthesizeWithAdmissionAsync(
        SpeechScript script,
        TtsOptions request,
        TtsProviderOptions configured,
        DateTimeOffset confirmationWindowExpiresAt,
        bool runtimeSynthesisAllowed,
        CancellationToken cancellationToken)
    {
        bool external = string.Equals(configured.Provider, TtsProviderOptions.ExternalProvider,
            StringComparison.OrdinalIgnoreCase);
        using var preparation = new CancellationTokenSource();
        using var window = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, preparation.Token);
        if (external)
        {
            TimeSpan remaining = confirmationWindowExpiresAt - timeProvider.GetUtcNow();
            if (remaining <= TimeSpan.Zero)
                throw new TtsSynthesisException("TTS_CACHE_WINDOW_EXPIRED", "The confirmation window expired before speech preparation.");
            window.CancelAfter(remaining);
            preparation.CancelAfter(configured.PreparationTimeoutMilliseconds);
        }

        try
        {
            // Hold the turn across all three dynamic pieces. Queue wait never spends their
            // synthesis budgets. One total deadline covers queue, busy retry and every segment;
            // neither taking a turn nor beginning another segment restarts it.
            using IDisposable? turn = external
                ? await preparationQueue.EnterAsync(configured.PreparationQueueLimit,
                    TimeSpan.FromMilliseconds(configured.PreparationQueueTimeoutMilliseconds), window.Token)
                : null;
            window.Token.ThrowIfCancellationRequested();
            DateTimeOffset now = timeProvider.GetUtcNow();
            DateTimeOffset cacheExpiresAt = Minimum(confirmationWindowExpiresAt,
                now.AddSeconds(configured.CacheMaximumTtlSeconds),
                now.AddSeconds(configured.SpeechSnapshotRetentionSeconds));
            RenderedAudio audio = configured.Segmentation.Enabled && script.IsSegmented
                ? await SynthesizeSegmentedAsync(script, request, configured, cacheExpiresAt, runtimeSynthesisAllowed, window.Token)
                : await SynthesizeWholeAsync(script, request, configured, cacheExpiresAt, runtimeSynthesisAllowed, window.Token);
            window.Token.ThrowIfCancellationRequested();
            if (external && timeProvider.GetUtcNow() >= confirmationWindowExpiresAt)
                throw new TtsSynthesisException("TTS_CACHE_WINDOW_EXPIRED", "The confirmation window expired during speech preparation.");
            return audio;
        }
        catch (OperationCanceledException exception) when (external && !cancellationToken.IsCancellationRequested
            && window.IsCancellationRequested)
        {
            if (preparation.IsCancellationRequested && timeProvider.GetUtcNow() < confirmationWindowExpiresAt)
                throw new TtsSynthesisException("TTS_PREPARATION_TIMEOUT",
                    "The complete speech preparation exceeded its total deadline.", exception);
            throw new TtsSynthesisException("TTS_CACHE_WINDOW_EXPIRED",
                "The confirmation window expired during speech preparation.", exception);
        }
    }

    private async Task<RenderedAudio> SynthesizeWholeAsync(
        SpeechScript script,
        TtsOptions request,
        TtsProviderOptions configured,
        DateTimeOffset cacheExpiresAt,
        bool runtimeSynthesisAllowed,
        CancellationToken cancellationToken)
    {
        // W-0363 / K-49. The whole script is synthesized at call time, so production refuses it
        // before the cache, which only ever holds audio this process synthesized.
        if (!runtimeSynthesisAllowed)
        {
            throw RuntimeSynthesisForbiddenTtsProvider.Refusal();
        }

        AudioCacheKey cacheKey = AudioCacheKey.Create(
            script.TemplateId,
            script.TemplateVersion,
            script.SummaryHash,
            request.VoiceId,
            request.Locale);
        AudioCacheResult cached = await cache.GetOrCreateAsync(
            cacheKey,
            cacheExpiresAt,
            async factoryCancellation =>
            {
                if (!requestBudget.TryConsume(
                        script.ExactText.Length,
                        configured.MaxRequestsPerMinute,
                        configured.MaxCharactersPerMinute))
                {
                    throw new IvrFailureException(
                        IvrErrorCodes.RateLimited,
                        "The TTS provider request budget is exhausted.");
                }

                usageMeter.RecordProviderRequest(script.ExactText.Length);
                return await SynthesizeProviderAsync(
                    script,
                    request,
                    factoryCancellation);
            },
            cancellationToken);
        usageMeter.RecordCache(cached.CacheHit);
        return cached.Audio;
    }

    /// <summary>
    /// Assembles a call from its pieces: fixed prose from the files VieNeu rendered ahead of
    /// time, and the order's own values synthesized by VieNeu at call time.
    /// <para>
    /// A missing piece throws. It must: playing the pieces that did resolve would produce a call
    /// that sounds complete and states a different order — the opening, then silence where the
    /// items were, then a total. That is worse than a technical failure, because a technical
    /// failure is retried and a wrong confirmation is acted on.
    /// </para>
    /// </summary>
    private async Task<RenderedAudio> SynthesizeSegmentedAsync(
        SpeechScript script,
        TtsOptions request,
        TtsProviderOptions configured,
        DateTimeOffset cacheExpiresAt,
        bool runtimeSynthesisAllowed,
        CancellationToken cancellationToken)
    {
        bool useCatalog =
            configured.Segmentation.FixedSegments == FixedSegmentSource.Catalog;
        IReadOnlyDictionary<string, FixedSegmentMediaEntry> catalog = useCatalog
            ? regionalVoices.FixedSegmentCatalog(request.VoiceId)
            : EmptyCatalog;

        var rendered = new List<RenderedAudioSegment>(script.Segments.Length);
        foreach (SpeechSegment segment in script.Segments)
        {
            if (segment.Kind == SpeechSegmentKind.Fixed && useCatalog)
            {
                if (!catalog.TryGetValue(segment.TextHash, out FixedSegmentMediaEntry? entry))
                {
                    throw new TtsSynthesisException(
                        "TTS_FIXED_SEGMENT_NOT_RENDERED",
                        "The approved script contains fixed speech with no pre-rendered file for the selected voice.");
                }

                rendered.Add(new RenderedAudioSegment(
                    segment.TextHash,
                    entry.MediaReference,
                    TimeSpan.FromMilliseconds(entry.DurationMilliseconds)));
                usageMeter.RecordSegment(segment.Kind, true, false);
                continue;
            }

            // W-0363 / K-49. Anything that is not pre-rendered prose would be synthesized now.
            // Production refuses it: the order's own values have no pre-rendered clips yet
            // (Q-29.2), so the call fails closed rather than playing part of an order.
            if (!runtimeSynthesisAllowed)
            {
                throw RuntimeSynthesisForbiddenTtsProvider.Refusal();
            }

            AudioCacheResult cached = await cache.GetOrCreateAsync(
                AudioCacheKey.CreateForSegment(
                    script.TemplateId,
                    script.TemplateVersion,
                    segment.TextHash,
                    request.VoiceId,
                    request.Locale),
                cacheExpiresAt,
                async factoryCancellation =>
                {
                    if (!requestBudget.TryConsume(
                            segment.Text.Length,
                            configured.MaxRequestsPerMinute,
                            configured.MaxCharactersPerMinute))
                    {
                        throw new IvrFailureException(
                            IvrErrorCodes.RateLimited,
                            "The TTS provider request budget is exhausted.");
                    }

                    usageMeter.RecordProviderRequest(segment.Text.Length);
                    return await SynthesizeProviderAsync(
                        SpeechScript.Create(
                            script.TemplateId,
                            script.TemplateVersion,
                            segment.Text,
                            segment.TextHash,
                            script.SummaryHash),
                        request,
                        factoryCancellation);
                },
                cancellationToken);
            usageMeter.RecordCache(cached.CacheHit);
            usageMeter.RecordSegment(segment.Kind, false, cached.CacheHit);
            rendered.Add(new RenderedAudioSegment(
                segment.TextHash,
                cached.Audio.ContentRef,
                cached.Audio.Duration));
        }

        // W-0363 / K-48. CreatePlaylist refuses a playlist past MaxPlaylistDuration with an
        // ArgumentOutOfRangeException, which both dispatch gateways record as a SIM fault: the
        // channel went into quarantine for an order that was too long. Pre-rendered pieces may each
        // run to five minutes, so the sum is what can cross it. The 64-piece bound cannot be
        // crossed from here: a speech segment's ordinal is capped at 64 when it is created.
        TimeSpan total = TimeSpan.Zero;
        foreach (RenderedAudioSegment piece in rendered)
        {
            total += piece.Duration;
        }

        if (total > RenderedAudio.MaxPlaylistDuration)
        {
            throw PlaylistTooLong();
        }

        RenderedAudio playlist = RenderedAudio.CreatePlaylist(
            configured.OutputFormat,
            configured.SampleRate,
            rendered);

        // Per-piece duration is bounded inside SynthesizeProviderAsync; nothing there sees the
        // total, and eight pieces each comfortably under the cap still add up to a call nobody
        // stays on the line for.
        if (playlist.Duration > request.MaxDuration)
        {
            throw new TtsSynthesisException(
                "TTS_MAX_DURATION_EXCEEDED",
                "The assembled speech exceeds the configured duration bound.");
        }

        return playlist;
    }

    private async Task<RenderedAudio> SynthesizeProviderAsync(
        SpeechScript script,
        TtsOptions options,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.Timeout);
        try
        {
            RenderedAudio audio = await provider.SynthesizeAsync(
                script,
                options,
                timeout.Token);
            if (audio.Duration > options.MaxDuration)
            {
                throw new TtsSynthesisException(
                    "TTS_MAX_DURATION_EXCEEDED",
                    "The synthesized audio exceeds the configured duration bound.");
            }

            return audio;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TtsSynthesisException(
                "TTS_TIMEOUT",
                "The TTS provider exceeded its configured timeout.",
                exception);
        }
        catch (OperationCanceledException)
        {
            // Preserve caller/order cancellation rather than wrapping it as a provider failure.
            throw;
        }
        catch (TtsSynthesisException)
        {
            throw;
        }
        catch (IvrFailureException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new TtsSynthesisException(
                "TTS_PROVIDER_FAILURE",
                "The TTS provider failed before producing audio.",
                exception);
        }
    }

    private static TtsSynthesisException PlaylistTooLong() => new(
        PlaylistTooLongCode,
        "The assembled speech needs more pieces, or plays longer, than one call's audio may.");

    private static DateTimeOffset Minimum(
        DateTimeOffset first,
        DateTimeOffset second,
        DateTimeOffset third) => first <= second
        ? first <= third ? first : third
        : second <= third ? second : third;
}

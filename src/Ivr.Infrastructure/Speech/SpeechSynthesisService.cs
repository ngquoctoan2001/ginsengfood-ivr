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
        if (executionMode == ExecutionMode.ProductionReal
            && string.IsNullOrWhiteSpace(configured.ProductionWhitelistApprovalRecord))
        {
            throw IvrErrors.OperationalBlocked(
                "Production TTS is blocked until the Target V1 speech whitelist has an approval record.");
        }

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
            summary.ComputeHash());
        SpeechPrivacyGuard.EnsureSafe(script, request);
        if (script.ExactText.Length > configured.MaxCharactersPerRequest)
        {
            throw new IvrFailureException(
                IvrErrorCodes.RateLimited,
                "The rendered speech exceeds the configured TTS character bound.");
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset cacheExpiresAt = Minimum(
            confirmationWindowExpiresAt,
            now.AddSeconds(configured.CacheMaximumTtlSeconds),
            now.AddSeconds(configured.SpeechSnapshotRetentionSeconds));
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
                    factoryCancellation).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
        usageMeter.RecordCache(cached.CacheHit);
        return renderedSpeech.WithAudio(cached.Audio);
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
                timeout.Token).ConfigureAwait(false);
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

    private static DateTimeOffset Minimum(
        DateTimeOffset first,
        DateTimeOffset second,
        DateTimeOffset third) => first <= second
        ? first <= third ? first : third
        : second <= third ? second : third;
}

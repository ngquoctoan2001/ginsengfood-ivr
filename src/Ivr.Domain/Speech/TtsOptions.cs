using System.Collections.Immutable;
using System.Text;
using Ivr.Domain.Privacy;

namespace Ivr.Domain.Speech;

/// <summary>
/// Per-request synthesis controls independent of any vendor SDK.
/// </summary>
public sealed record TtsOptions
{
    private TtsOptions(
        string locale,
        string voiceId,
        decimal speakingRate,
        ImmutableSortedDictionary<string, string> pronunciationHints,
        TimeSpan maxDuration,
        TimeSpan timeout)
    {
        Locale = locale;
        VoiceId = voiceId;
        SpeakingRate = speakingRate;
        PronunciationHints = pronunciationHints;
        MaxDuration = maxDuration;
        Timeout = timeout;
    }

    public string Locale { get; }

    public string VoiceId { get; }

    public decimal SpeakingRate { get; }

    public ImmutableSortedDictionary<string, string> PronunciationHints { get; }

    public TimeSpan MaxDuration { get; }

    public TimeSpan Timeout { get; }

    public static TtsOptions Create(
        string locale = "vi-VN",
        string voiceId = "fake-vi-vn",
        decimal speakingRate = 1m,
        IReadOnlyDictionary<string, string>? pronunciationHints = null,
        TimeSpan? maxDuration = null,
        TimeSpan? timeout = null)
    {
        string safeLocale = Required(locale, 20, nameof(locale));
        string safeVoice = Required(voiceId, 120, nameof(voiceId));
        if (speakingRate is < 0.5m or > 2m)
        {
            throw new ArgumentOutOfRangeException(nameof(speakingRate));
        }

        TimeSpan safeMaxDuration = maxDuration ?? TimeSpan.FromSeconds(120);
        TimeSpan safeTimeout = timeout ?? TimeSpan.FromSeconds(5);
        if (safeMaxDuration < TimeSpan.FromSeconds(1)
            || safeMaxDuration > TimeSpan.FromMinutes(5))
        {
            throw new ArgumentOutOfRangeException(nameof(maxDuration));
        }

        if (safeTimeout < TimeSpan.FromMilliseconds(10)
            || safeTimeout > TimeSpan.FromMinutes(2))
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        var hints = ImmutableSortedDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in
                 pronunciationHints ?? new Dictionary<string, string>())
        {
            hints.Add(Required(key, 80, nameof(pronunciationHints)),
                Required(value, 160, nameof(pronunciationHints)));
        }

        return new TtsOptions(
            safeLocale,
            safeVoice,
            speakingRate,
            hints.ToImmutable(),
            safeMaxDuration,
            safeTimeout);
    }

    public override string ToString() => "[REDACTED_TTS_OPTIONS]";

    private static string Required(string value, int maximumLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string normalized = value.Trim().Normalize(NormalizationForm.FormC);
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        PiiGuard.EnsureSafeText(normalized);
        return normalized;
    }
}

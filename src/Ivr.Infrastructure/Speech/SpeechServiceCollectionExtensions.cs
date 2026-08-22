using System.Collections.Immutable;
using Ivr.Domain.Retention;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

/// <summary>
/// One recorded file for one fixed piece of the approved script, in one voice.
/// </summary>
public sealed class FixedSegmentMediaEntry
{
    /// <summary>
    /// <see cref="SpeechSegment.TextHash"/> of the sentence this file speaks.
    /// <para>
    /// The catalog is keyed by what the file says, not by where it sits in the script. Keying by
    /// position would let a template edit that reorders sentences keep resolving — every lookup
    /// would succeed and every call would play the sentences in the old order.
    /// </para>
    /// </summary>
    public string TextHash { get; set; } = string.Empty;

    public string MediaReference { get; set; } = string.Empty;

    public int DurationMilliseconds { get; set; }

    public override string ToString() => "[REDACTED_FIXED_SEGMENT_MEDIA_ENTRY]";
}

/// <summary>
/// Where the fixed prose of a call comes from.
/// </summary>
public enum FixedSegmentSource
{
    /// <summary>
    /// Synthesize fixed prose like any other text. Restricted to the MOCK fake provider: against
    /// a real vendor this pays for the same 203 characters on every cold cache, which is the
    /// cost model W-0106 §4.6 exists to avoid.
    /// </summary>
    Provider = 0,

    /// <summary>
    /// Play pre-recorded files pinned by text hash. Zero runtime synthesis cost, and the order
    /// content in those sentences never leaves the network.
    /// </summary>
    Catalog = 1,
}

/// <summary>
/// Hybrid playback: fixed prose from recordings, order values from a synthesizer (W-0106 §4.6).
/// Disabled by default, so an unconfigured deployment keeps single-file playback.
/// </summary>
public sealed class SpeechSegmentationOptions
{
    public bool Enabled { get; set; }

    public FixedSegmentSource FixedSegments { get; set; } = FixedSegmentSource.Provider;

    public override string ToString() => "[REDACTED_SPEECH_SEGMENTATION_OPTIONS]";
}

public sealed class TtsProviderOptions
{
    public const string SectionName = "Ivr:Speech:Tts";
    public const string FakeProvider = "FAKE_DETERMINISTIC";
    public const string StaticFileProvider = "STATIC_FILE";
    public const string ExternalProvider = "EXTERNAL_CONFIGURABLE";
    public const string UnselectedProvider = "UNSELECTED";

    public string ExecutionMode { get; set; } = "MOCK";

    public string Provider { get; set; } = FakeProvider;

    public string Endpoint { get; set; } = string.Empty;

    public string Credential { get; set; } = string.Empty;

    public string FileMediaReference { get; set; } = "sound:ivr-lab-order-confirmation";

    public int FileDurationSeconds { get; set; } = 18;

    public string OutputFormat { get; set; } = "audio/L16";

    public int SampleRate { get; set; } = 8_000;

    public string Locale { get; set; } = "vi-VN";

    public string VoiceId { get; set; } = "fake-vi-vn";

    public decimal SpeakingRate { get; set; } = 1m;

    public int MaxDurationSeconds { get; set; } = 120;

    public int TimeoutMilliseconds { get; set; } = 5_000;

    public int CacheMaximumTtlSeconds { get; set; } = 900;

    public int SpeechSnapshotRetentionSeconds { get; set; } = 900;

    public int MaxCharactersPerRequest { get; set; } = 1_200;

    public int MaxRequestsPerMinute { get; set; } = 60;

    public int MaxCharactersPerMinute { get; set; } = 72_000;

    public string ProductionWhitelistApprovalRecord { get; set; } = string.Empty;

    /// <summary>
    /// Three regional voices (W-0106). Disabled by default, so an unconfigured deployment keeps
    /// the single-voice behaviour that shipped before.
    /// </summary>
    public RegionalVoiceOptions RegionalVoices { get; set; } = new();

    /// <summary>
    /// Hybrid segmented playback. Disabled by default: turning it on changes what a customer
    /// hears, so it is a deployment decision rather than an upgrade side effect.
    /// </summary>
    public SpeechSegmentationOptions Segmentation { get; set; } = new();

    /// <summary>
    /// Fixed-segment recordings for the single global voice. Only read when regional voices are
    /// off; with them on, each region carries its own catalog because the recording is of a
    /// specific voice, not of a sentence.
    /// </summary>
    public FixedSegmentMediaEntry[] FixedSegments { get; set; } = [];

    /// <summary>
    /// Absolute HTTPS endpoint for the external provider, plus the credential and the request
    /// shape it expects. Vendor choice lives in configuration; no vendor name appears in code.
    /// </summary>
    public ExternalTtsOptions External { get; set; } = new();

    public override string ToString() => "[REDACTED_TTS_PROVIDER_OPTIONS]";
}

public sealed class TtsProviderOptionsValidator : IValidateOptions<TtsProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, TtsProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var failures = new List<string>();
        bool mock = string.Equals(options.ExecutionMode, "MOCK", StringComparison.OrdinalIgnoreCase);
        bool lab = string.Equals(
            options.ExecutionMode,
            "LAB_REAL_SIM",
            StringComparison.OrdinalIgnoreCase);
        if (mock && !string.Equals(
                options.Provider,
                TtsProviderOptions.FakeProvider,
                StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("MOCK execution requires the deterministic fake TTS provider.");
        }

        if (mock && (!string.IsNullOrWhiteSpace(options.Endpoint)
                     || !string.IsNullOrWhiteSpace(options.Credential)))
        {
            failures.Add("MOCK TTS cannot configure an external endpoint or credential.");
        }

        if (!mock && string.Equals(
                options.Provider,
                TtsProviderOptions.FakeProvider,
                StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("The deterministic fake TTS provider is restricted to MOCK execution.");
        }

        if (string.Equals(
                options.Provider,
                TtsProviderOptions.StaticFileProvider,
                StringComparison.OrdinalIgnoreCase))
        {
            if (!lab)
            {
                failures.Add("The static-file TTS provider is restricted to LAB_REAL_SIM execution.");
            }

            if (!options.FileMediaReference.StartsWith("sound:", StringComparison.Ordinal)
                || options.FileMediaReference.Length > 160
                || options.FileMediaReference.Any(character =>
                    !(char.IsAsciiLetterOrDigit(character)
                      || character is ':' or '-' or '_' or '/')))
            {
                failures.Add("FileMediaReference must be a safe Asterisk sound reference.");
            }

            if (!string.IsNullOrWhiteSpace(options.Endpoint)
                || !string.IsNullOrWhiteSpace(options.Credential))
            {
                failures.Add("The static-file TTS provider cannot configure network credentials.");
            }
        }

        if (!new[]
            {
                TtsProviderOptions.FakeProvider,
                TtsProviderOptions.StaticFileProvider,
                TtsProviderOptions.ExternalProvider,
                TtsProviderOptions.UnselectedProvider,
            }.Contains(options.Provider, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("The configured TTS provider is unsupported.");
        }

        if (string.IsNullOrWhiteSpace(options.OutputFormat)
            || string.IsNullOrWhiteSpace(options.Locale)
            || string.IsNullOrWhiteSpace(options.VoiceId))
        {
            failures.Add("TTS output format, locale and voice ID are required.");
        }

        if (options.SampleRate is < 8_000 or > 192_000
            || options.SpeakingRate is < 0.5m or > 2m
            || options.MaxDurationSeconds is < 1 or > 300
            || options.FileDurationSeconds is < 1 or > 300
            || options.TimeoutMilliseconds is < 10 or > 120_000
            || options.CacheMaximumTtlSeconds is < 1 or > 86_400
            || options.SpeechSnapshotRetentionSeconds is < 1 or > 86_400
            || options.MaxCharactersPerRequest is < 200 or > 4_000
            || options.MaxRequestsPerMinute is < 1 or > 10_000
            || options.MaxCharactersPerMinute < options.MaxCharactersPerRequest)
        {
            failures.Add("One or more TTS numeric safety bounds are invalid.");
        }

        ValidateRegionalVoices(options, failures);
        ValidateSegmentation(options, failures);
        ValidateExternal(options, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures.Distinct(StringComparer.Ordinal));
    }

    /// <summary>
    /// Proves at startup that every fixed sentence of the canonical script has a recording, in
    /// every voice that can be selected.
    /// <para>
    /// The alternative is discovering it on the first call of the day, mid-conversation with a
    /// customer. A missing recording is a deployment mistake, and a deployment mistake should
    /// stop the deployment.
    /// </para>
    /// <para>
    /// This checks the canonical template. A deployment serving a different approved version
    /// from the database is still covered, but only at render time by
    /// <c>TTS_FIXED_SEGMENT_NOT_RECORDED</c> — the validator has no database. Said plainly here
    /// so nobody reads a green startup as proof of coverage for a custom template.
    /// </para>
    /// </summary>
    private static void ValidateSegmentation(TtsProviderOptions options, List<string> failures)
    {
        SpeechSegmentationOptions segmentation = options.Segmentation;
        if (!segmentation.Enabled)
        {
            return;
        }

        if (!Enum.IsDefined(segmentation.FixedSegments))
        {
            failures.Add("The configured fixed-segment source is unsupported.");
            return;
        }

        if (segmentation.FixedSegments == FixedSegmentSource.Provider)
        {
            if (!string.Equals(
                    options.Provider,
                    TtsProviderOptions.FakeProvider,
                    StringComparison.OrdinalIgnoreCase))
            {
                // Against a real vendor this re-synthesizes the same 203 fixed characters on
                // every cold cache, which is the whole cost difference the hybrid buys back.
                failures.Add(
                    "Synthesizing fixed segments is restricted to the MOCK fake provider; configure a recorded catalog.");
            }

            return;
        }

        ImmutableArray<string> requiredHashes;
        try
        {
            requiredHashes = TargetV1SpeechPolicy.FixedSegmentHashes(
                TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        }
        catch (InvalidOperationException)
        {
            failures.Add("The canonical script template cannot be split into speech segments.");
            return;
        }

        foreach ((string voiceLabel, FixedSegmentMediaEntry[] entries) in CatalogsByVoice(options))
        {
            var byHash = new Dictionary<string, FixedSegmentMediaEntry>(StringComparer.Ordinal);
            foreach (FixedSegmentMediaEntry entry in entries)
            {
                string hash = entry.TextHash?.Trim().ToLowerInvariant() ?? string.Empty;
                if (hash.Length != 64 || !hash.All(char.IsAsciiHexDigitLower))
                {
                    failures.Add(
                        $"Fixed segment media for {voiceLabel} needs a 64-character lowercase SHA-256 text hash.");
                    continue;
                }

                if (!IsSafeMediaReference(entry.MediaReference))
                {
                    failures.Add(
                        $"Fixed segment media for {voiceLabel} needs a safe Asterisk sound reference.");
                    continue;
                }

                if (entry.DurationMilliseconds is < 1 or > 300_000)
                {
                    failures.Add(
                        $"Fixed segment media for {voiceLabel} needs a duration between 1 ms and 300 s.");
                    continue;
                }

                byHash[hash] = entry;
            }

            foreach (string required in requiredHashes)
            {
                if (!byHash.ContainsKey(required))
                {
                    failures.Add(
                        $"Fixed segment media for {voiceLabel} is missing a recording for part of the approved script.");
                }
            }
        }
    }

    private static IEnumerable<(string VoiceLabel, FixedSegmentMediaEntry[] Entries)>
        CatalogsByVoice(TtsProviderOptions options)
    {
        if (!options.RegionalVoices.Enabled)
        {
            yield return ("the configured voice", options.FixedSegments);
            yield break;
        }

        foreach (VietnamRegion region in Enum.GetValues<VietnamRegion>())
        {
            RegionalVoiceEntry entry = options.RegionalVoices.For(region);
            yield return ($"the {region} voice", entry.FixedSegments);
        }
    }

    private static void ValidateExternal(TtsProviderOptions options, List<string> failures)
    {
        if (!string.Equals(
                options.Provider,
                TtsProviderOptions.ExternalProvider,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ExternalTtsOptions external = options.External;
        if (!Uri.TryCreate(external.Endpoint, UriKind.Absolute, out Uri? endpoint)
            || (endpoint.Scheme != Uri.UriSchemeHttps
                && !(endpoint.Scheme == Uri.UriSchemeHttp && endpoint.IsLoopback)))
        {
            // Plain HTTP off-box would put order values on the wire in clear text. Loopback is
            // allowed because that is how a converting sidecar is reached.
            failures.Add("The external TTS endpoint must be an absolute HTTPS URI, or HTTP on loopback.");
        }

        if (string.IsNullOrWhiteSpace(external.RequestBodyTemplate)
            || !external.RequestBodyTemplate.Contains("{{text}}", StringComparison.Ordinal))
        {
            failures.Add("The external TTS request body template must contain the {{text}} variable.");
        }

        if (string.IsNullOrWhiteSpace(external.MediaOutputDirectory))
        {
            failures.Add("The external TTS provider needs a media output directory.");
        }

        if (!IsSafeMediaReference(external.MediaReferencePrefix))
        {
            failures.Add("The external TTS media reference prefix must be a safe Asterisk sound reference.");
        }

        if (string.IsNullOrWhiteSpace(external.CredentialHeader))
        {
            failures.Add("The external TTS credential header name is required.");
        }

        if (external.MaxResponseBytes is < 1_024 or > 64 * 1024 * 1024)
        {
            failures.Add("The external TTS response bound must be between 1 KiB and 64 MiB.");
        }

        if (!string.Equals(
                options.OutputFormat,
                ConfigurableExternalTtsProvider.RequiredOutputFormat,
                StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("The external TTS provider requires the audio/L16 output format.");
        }

        if (!ConfigurableExternalTtsProvider.IsSupportedSampleRate(options.SampleRate))
        {
            failures.Add("The external TTS sample rate has no raw signed-linear container.");
        }
    }

    private static void ValidateRegionalVoices(TtsProviderOptions options, List<string> failures)
    {
        RegionalVoiceOptions regional = options.RegionalVoices;
        if (!regional.Enabled)
        {
            return;
        }

        if (!Enum.IsDefined(regional.FallbackRegion))
        {
            failures.Add("The regional voice fallback region is not a defined region.");
        }

        VietnamRegion[] regions = Enum.GetValues<VietnamRegion>();
        RegionalVoiceEntry[] entries = regions.Select(regional.For).ToArray();
        if (entries.Any(entry => string.IsNullOrWhiteSpace(entry.VoiceId)
                || entry.VoiceId.Trim().Length > 120))
        {
            failures.Add("Regional voices require a voice ID of 1-120 characters for every region.");
        }

        // Three regions configured to the same voice is the failure mode this whole work item
        // exists to prevent, and it is invisible at runtime: every call simply sounds the same.
        // Fail startup instead of shipping a silent single-voice deployment that claims three.
        if (entries.Select(entry => entry.VoiceId.Trim())
                .Where(voiceId => voiceId.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .Count() != entries.Length)
        {
            failures.Add("Each region requires a distinct voice ID; duplicates disable regional routing silently.");
        }

        if (entries.Any(entry => entry.SpeakingRate != 0m && entry.SpeakingRate is < 0.5m or > 2m))
        {
            failures.Add("A regional speaking rate must be zero (inherit) or between 0.5 and 2.");
        }

        if (!string.Equals(
                options.Provider,
                TtsProviderOptions.StaticFileProvider,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (entries.Any(entry => !IsSafeMediaReference(entry.FileMediaReference)))
        {
            failures.Add("Regional static-file voices require a safe Asterisk sound reference per region.");
        }

        if (entries.Select(entry => entry.FileMediaReference.Trim())
                .Where(reference => reference.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .Count() != entries.Length)
        {
            failures.Add("Each region requires a distinct media reference; duplicates play one region's audio to all.");
        }

        if (entries.Any(entry => entry.FileDurationSeconds is < 1 or > 300))
        {
            failures.Add("Each regional media file requires a duration between 1 and 300 seconds.");
        }
    }

    private static bool IsSafeMediaReference(string reference) =>
        reference.StartsWith("sound:", StringComparison.Ordinal)
        && reference.Length <= 160
        && reference.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is ':' or '-' or '_' or '/');
}

public static class SpeechServiceCollectionExtensions
{
    public static IServiceCollection AddIvrSpeech(
        this IServiceCollection services,
        IConfiguration configuration,
        string executionMode)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(executionMode);
        IConfigurationSection section = configuration.GetSection(TtsProviderOptions.SectionName);
        bool mock = string.Equals(executionMode, "MOCK", StringComparison.OrdinalIgnoreCase);
        bool staticFile = string.Equals(
            section[nameof(TtsProviderOptions.Provider)],
            TtsProviderOptions.StaticFileProvider,
            StringComparison.OrdinalIgnoreCase);
        services.AddOptions<TtsProviderOptions>()
            .Bind(section)
            .PostConfigure(options =>
            {
                options.ExecutionMode = executionMode;
                if (string.IsNullOrWhiteSpace(section[nameof(TtsProviderOptions.Provider)]))
                {
                    options.Provider = mock
                        ? TtsProviderOptions.FakeProvider
                        : TtsProviderOptions.UnselectedProvider;
                }

                int? speechRetentionDays = configuration.GetValue<int?>(
                    "Ivr:Retention:PeriodDays:speech_snapshot");
                if (speechRetentionDays is > 0)
                {
                    options.SpeechSnapshotRetentionSeconds = Math.Min(
                        86_400,
                        checked(speechRetentionDays.Value * 24 * 60 * 60));
                }
            })
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<TtsProviderOptions>, TtsProviderOptionsValidator>());
        services.TryAddSingleton<TtsUsageMeter>();
        services.TryAddSingleton<TtsRequestBudget>();
        services.TryAddSingleton<RegionalVoiceMap>();
        services.TryAddSingleton<IAudioCache, AudioCache>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IRetentionPurgeHook, SpeechAudioCacheRetentionHook>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IRetentionPurgeHook, SpeechMediaFileRetentionHook>());
        services.TryAddSingleton<ISpeechSynthesisService, SpeechSynthesisService>();
        if (mock)
        {
            services.TryAddSingleton<ITtsProvider, FakeDeterministicTtsProvider>();
        }
        else if (staticFile)
        {
            services.TryAddSingleton<ITtsProvider, StaticFileTtsProvider>();
        }
        else
        {
            // No BaseAddress and no default headers: the endpoint is an absolute URI and the
            // credential is attached per request, so a misconfigured client cannot send a
            // credential to a host the options never named.
            services.AddHttpClient(ConfigurableExternalTtsProvider.HttpClientName);
            services.TryAddSingleton<ITtsProvider, ConfigurableExternalTtsProvider>();
        }

        return services;
    }
}

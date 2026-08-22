using Ivr.Domain.Retention;
using Ivr.Domain.Speech;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

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

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures.Distinct(StringComparer.Ordinal));
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
            services.TryAddSingleton<ITtsProvider, ConfigurableExternalTtsProvider>();
        }

        return services;
    }
}

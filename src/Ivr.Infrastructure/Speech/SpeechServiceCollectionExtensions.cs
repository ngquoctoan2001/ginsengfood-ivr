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
    public const string ExternalProvider = "EXTERNAL_CONFIGURABLE";
    public const string UnselectedProvider = "UNSELECTED";

    public string ExecutionMode { get; set; } = "MOCK";

    public string Provider { get; set; } = FakeProvider;

    public string Endpoint { get; set; } = string.Empty;

    public string Credential { get; set; } = string.Empty;

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

    public override string ToString() => "[REDACTED_TTS_PROVIDER_OPTIONS]";
}

public sealed class TtsProviderOptionsValidator : IValidateOptions<TtsProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, TtsProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var failures = new List<string>();
        bool mock = string.Equals(options.ExecutionMode, "MOCK", StringComparison.OrdinalIgnoreCase);
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

        if (!new[]
            {
                TtsProviderOptions.FakeProvider,
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
            || options.TimeoutMilliseconds is < 10 or > 120_000
            || options.CacheMaximumTtlSeconds is < 1 or > 86_400
            || options.SpeechSnapshotRetentionSeconds is < 1 or > 86_400
            || options.MaxCharactersPerRequest is < 200 or > 4_000
            || options.MaxRequestsPerMinute is < 1 or > 10_000
            || options.MaxCharactersPerMinute < options.MaxCharactersPerRequest)
        {
            failures.Add("One or more TTS numeric safety bounds are invalid.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures.Distinct(StringComparer.Ordinal));
    }
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
        services.TryAddSingleton<IAudioCache, AudioCache>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IRetentionPurgeHook, SpeechAudioCacheRetentionHook>());
        services.TryAddSingleton<ISpeechSynthesisService, SpeechSynthesisService>();
        if (mock)
        {
            services.TryAddSingleton<ITtsProvider, FakeDeterministicTtsProvider>();
        }
        else
        {
            services.TryAddSingleton<ITtsProvider, ConfigurableExternalTtsProvider>();
        }

        return services;
    }
}

using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

/// <summary>
/// LAB-only provider that binds approved speech to an Asterisk media reference.
/// It does not write the rendered text to disk or send it over the network.
/// </summary>
public sealed class StaticFileTtsProvider(
    IOptions<TtsProviderOptions> providerOptions) : ITtsProvider
{
    public Task<RenderedAudio> SynthesizeAsync(
        SpeechScript script,
        TtsOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(options);
        TtsProviderOptions configured = providerOptions.Value;
        if (!string.Equals(
                configured.ExecutionMode,
                "LAB_REAL_SIM",
                StringComparison.OrdinalIgnoreCase)
            || !string.Equals(
                configured.Provider,
                TtsProviderOptions.StaticFileProvider,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new TtsProviderNotConfiguredException(
                "The static-file TTS provider is restricted to LAB_REAL_SIM mode.");
        }

        return Task.FromResult(RenderedAudio.Create(
            configured.OutputFormat,
            configured.SampleRate,
            TimeSpan.FromSeconds(configured.FileDurationSeconds),
            configured.FileMediaReference));
    }
}

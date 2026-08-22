using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

/// <summary>
/// LAB-only provider that binds approved speech to an Asterisk media reference.
/// It does not write the rendered text to disk or send it over the network.
/// </summary>
public sealed class StaticFileTtsProvider(
    IOptions<TtsProviderOptions> providerOptions,
    RegionalVoiceMap regionalVoices) : ITtsProvider
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

        // The provider never sees a region — only the voice the synthesis service already chose.
        // Looking the media file up by voice id is what stops the file and the voice drifting
        // apart into a call that announces one region and sounds like another.
        if (!regionalVoices.TryGetMedia(
                options.VoiceId,
                out string mediaReference,
                out int durationSeconds))
        {
            if (regionalVoices.Enabled)
            {
                throw new TtsProviderNotConfiguredException(
                    "No LAB media file is configured for the selected regional voice.");
            }

            mediaReference = configured.FileMediaReference;
            durationSeconds = configured.FileDurationSeconds;
        }

        return Task.FromResult(RenderedAudio.Create(
            configured.OutputFormat,
            configured.SampleRate,
            TimeSpan.FromSeconds(durationSeconds),
            mediaReference));
    }
}

using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

public class TtsSynthesisException(
    string technicalErrorCode,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public string TechnicalErrorCode { get; } = technicalErrorCode;
}

public sealed class TtsProviderNotConfiguredException(string message)
    : TtsSynthesisException("TTS_NOT_CONFIGURED", message);

/// <summary>
/// Configuration seam reserved for P8-1. It intentionally contains no vendor protocol or SDK.
/// </summary>
public sealed class ConfigurableExternalTtsProvider(
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
        _ = configured.Endpoint;
        _ = configured.Credential;
        _ = configured.OutputFormat;
        throw new TtsProviderNotConfiguredException(
            "No external TTS vendor adapter is available until OD-V1-19 is approved and P8-1 is implemented.");
    }
}

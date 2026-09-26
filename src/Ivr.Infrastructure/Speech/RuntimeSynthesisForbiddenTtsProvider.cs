using Ivr.Domain.Speech;

namespace Ivr.Infrastructure.Speech;

/// <summary>
/// W-0363 / K-49. The only <see cref="ITtsProvider"/> a <c>PRODUCTION_REAL</c> deployment gets.
/// <para>
/// N1, the Tech Lead's transition rule of 24/09: production does not synthesize speech at call
/// time. It plays audio rendered ahead of time, by VieNeu offline, so the owner's "VieNeu only"
/// decision still holds; only when the rendering happens changes. <see cref="SpeechSynthesisService"/>
/// already refuses before it reaches a provider in production. This is the second wall: nothing
/// registered in a production process can synthesize, and no HTTP client for the VieNeu sidecar
/// exists there to be pointed at one.
/// </para>
/// <para>
/// It fails as a <see cref="TtsSynthesisException"/> on purpose. Both dispatch gateways map that to
/// <c>AudioError</c> with the channel healthy, which normalizes to <c>IVR_TECHNICAL_EXCEPTION</c>
/// that does not count as an attempt: the order is not called, and no SIM is blamed for it.
/// </para>
/// </summary>
public sealed class RuntimeSynthesisForbiddenTtsProvider : ITtsProvider
{
    public const string TechnicalCode = "TTS_RUNTIME_SYNTHESIS_FORBIDDEN";

    public Task<RenderedAudio> SynthesizeAsync(
        SpeechScript script,
        TtsOptions options,
        CancellationToken cancellationToken) =>
        Task.FromException<RenderedAudio>(Refusal());

    internal static TtsSynthesisException Refusal() => new(
        TechnicalCode,
        "Speech synthesis at call time is forbidden in production (N1, the transition rule of 24/09); only audio rendered ahead of time may play.");
}

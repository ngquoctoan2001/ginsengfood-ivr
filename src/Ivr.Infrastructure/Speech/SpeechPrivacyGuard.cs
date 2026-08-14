using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Domain.Speech;

namespace Ivr.Infrastructure.Speech;

/// <summary>
/// Re-checks final text and hints after rendering and immediately before synthesis.
/// </summary>
public static class SpeechPrivacyGuard
{
    public static void EnsureSafe(SpeechScript script, TtsOptions options)
    {
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(options);
        if (!PiiGuard.IsSafeText(script.ExactText)
            || options.PronunciationHints.Any(pair =>
                !PiiGuard.IsSafeText(pair.Key) || !PiiGuard.IsSafeText(pair.Value)))
        {
            throw IvrErrors.PiiPolicyViolation();
        }
    }
}

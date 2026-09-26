using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Domain.Speech;

namespace Ivr.Infrastructure.Speech;

/// <summary>
/// Re-checks final text and hints after rendering and immediately before synthesis.
/// <para>
/// Q-12 (PA1, 2026-09-26). The text is checked segment by segment
/// (<see cref="SpokenTextGuard"/>), the way the renderer checks it, so a product name intake
/// admitted is not refused here. A script without a placeholder split is one segment and keeps the
/// full guard over all of it.
/// </para>
/// </summary>
public static class SpeechPrivacyGuard
{
    public static void EnsureSafe(SpeechScript script, TtsOptions options)
    {
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(options);
        if (!SpokenTextGuard.IsSafe(script.Segments)
            || options.PronunciationHints.Any(pair =>
                !PiiGuard.IsSafeText(pair.Key) || !PiiGuard.IsSafeText(pair.Value)))
        {
            throw IvrErrors.PiiPolicyViolation();
        }
    }
}

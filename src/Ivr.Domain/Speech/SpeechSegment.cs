using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Privacy;

namespace Ivr.Domain.Speech;

public enum SpeechSegmentKind
{
    /// <summary>
    /// Prose that is identical for every order. Rendered once per voice and replayed forever.
    /// </summary>
    Fixed = 0,

    /// <summary>
    /// A placeholder value that changes between orders. Cached by content, not by order.
    /// </summary>
    Dynamic = 1,
}

/// <summary>
/// One piece of an approved script, split at the template's placeholder boundaries.
/// <para>
/// The split is derived from the approved template rather than configured beside it. That is the
/// whole safety property: a template edit moves every downstream <see cref="TextHash"/>, so audio
/// recorded from the previous wording can never be silently replayed under the new script. A
/// separately-configured segment list would drift, and the drift would be inaudible to everyone
/// except the customer.
/// </para>
/// </summary>
public sealed record SpeechSegment
{
    private SpeechSegment(
        int ordinal,
        SpeechSegmentKind kind,
        string text,
        string? placeholderName,
        string textHash)
    {
        Ordinal = ordinal;
        Kind = kind;
        Text = text;
        PlaceholderName = placeholderName;
        TextHash = textHash;
    }

    /// <summary>1-based position in playback order.</summary>
    public int Ordinal { get; }

    public SpeechSegmentKind Kind { get; }

    /// <summary>
    /// Exact spoken text for this segment. For <see cref="SpeechSegmentKind.Dynamic"/> this
    /// carries order content and must never be logged or persisted.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Template variable this segment was substituted from, or <see langword="null"/> for a
    /// fixed segment and for a whole-script segment.
    /// </summary>
    public string? PlaceholderName { get; }

    /// <summary>
    /// SHA-256 of the normalized text, lowercase hex. This is the identity used to look a fixed
    /// segment up in the recorded-media catalog and to key a dynamic segment in the audio cache.
    /// It is a hash of privacy-safe rendered speech, never of raw customer data.
    /// </summary>
    public string TextHash { get; }

    public static SpeechSegment CreateFixed(int ordinal, string text) =>
        Create(ordinal, SpeechSegmentKind.Fixed, text, null);

    public static SpeechSegment CreateDynamic(int ordinal, string? placeholderName, string text) =>
        Create(ordinal, SpeechSegmentKind.Dynamic, text, placeholderName);

    /// <summary>
    /// SHA-256 of a normalized segment text, exposed so configuration and manifests can pin the
    /// same identity the runtime computes.
    /// </summary>
    public static string ComputeTextHash(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(Normalize(text)))).ToLowerInvariant();
    }

    /// <summary>
    /// Reassembles playback text from an ordered segment list. Used to prove a segment list and
    /// the script it claims to represent say exactly the same thing.
    /// </summary>
    public static string Concatenate(IEnumerable<SpeechSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);
        StringBuilder builder = new();
        foreach (SpeechSegment segment in segments)
        {
            builder.Append(segment.Text);
        }

        return builder.ToString();
    }

    public override string ToString() => "[REDACTED_SPEECH_SEGMENT]";

    private static SpeechSegment Create(
        int ordinal,
        SpeechSegmentKind kind,
        string text,
        string? placeholderName)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ordinal, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ordinal, 64);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        string normalized = Normalize(text);
        if (normalized.Length > 2_000)
        {
            throw new ArgumentOutOfRangeException(nameof(text));
        }

        string? safePlaceholder = null;
        if (placeholderName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(placeholderName);
            safePlaceholder = placeholderName.Trim();
            if (safePlaceholder.Length > 80
                || !safePlaceholder.All(character =>
                    char.IsAsciiLetterOrDigit(character) || character == '_'))
            {
                throw new ArgumentOutOfRangeException(nameof(placeholderName));
            }
        }

        // A fixed segment is prose from an approved template, so it is safe to say out loud and
        // safe to keep in a manifest. Checking it here means a template that smuggled a phone
        // number past template validation still cannot become a recorded file.
        if (kind == SpeechSegmentKind.Fixed)
        {
            PiiGuard.EnsureSafeText(normalized);
        }

        return new SpeechSegment(
            ordinal,
            kind,
            normalized,
            safePlaceholder,
            ComputeTextHash(normalized));
    }

    private static string Normalize(string text) => text.Normalize(NormalizationForm.FormC);
}

/// <summary>
/// Ordered segment list for one rendered script.
/// </summary>
public static class SpeechSegmentValidation
{
    /// <summary>
    /// Rejects a segment list that is empty, mis-ordered, or does not reassemble into the text it
    /// claims to represent.
    /// </summary>
    public static ImmutableArray<SpeechSegment> Validate(
        IEnumerable<SpeechSegment> segments,
        string expectedText,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedText);
        ImmutableArray<SpeechSegment> ordered = [.. segments];
        if (ordered.IsEmpty)
        {
            throw new ArgumentException("A script needs at least one speech segment.", parameterName);
        }

        for (int index = 0; index < ordered.Length; index++)
        {
            if (ordered[index].Ordinal != index + 1)
            {
                throw new ArgumentException(
                    "Speech segments must be contiguous and 1-based in playback order.",
                    parameterName);
            }
        }

        string reassembled = SpeechSegment
            .Concatenate(ordered)
            .Trim()
            .Normalize(NormalizationForm.FormC);
        if (!string.Equals(reassembled, expectedText, StringComparison.Ordinal))
        {
            // Playback follows the segments; the character budget, the PII guard and every
            // evidence hash follow ExactText. If the two ever disagree, the customer hears
            // something no gate inspected.
            throw new ArgumentException(
                "Speech segments do not reassemble into the rendered script text.",
                parameterName);
        }

        return ordered;
    }
}

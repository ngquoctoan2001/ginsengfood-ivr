using System.Collections.Immutable;
using System.Text;

namespace Ivr.Domain.Speech;

/// <summary>
/// Privacy-safe, in-memory script passed to a speech-synthesis provider.
/// </summary>
public sealed record SpeechScript
{
    private SpeechScript(
        string templateId,
        string templateVersion,
        string exactText,
        string contentHash,
        string summaryHash,
        ImmutableArray<SpeechSegment> segments)
    {
        TemplateId = templateId;
        TemplateVersion = templateVersion;
        ExactText = exactText;
        ContentHash = contentHash;
        SummaryHash = summaryHash;
        Segments = segments;
    }

    public string TemplateId { get; }

    public string TemplateVersion { get; }

    /// <summary>
    /// Final rendered text. This value must never be logged or persisted in normal evidence.
    /// </summary>
    public string ExactText { get; }

    public string ContentHash { get; }

    public string SummaryHash { get; }

    /// <summary>
    /// Playback order, split at the approved template's placeholder boundaries (W-0106 §4.6
    /// hybrid). A caller that supplies no segments gets a single dynamic segment covering the
    /// whole text, which is exactly the pre-segmentation behaviour.
    /// </summary>
    public ImmutableArray<SpeechSegment> Segments { get; }

    /// <summary>
    /// True when the script carries a real placeholder split rather than the single whole-text
    /// segment. Only a split script can be served by the fixed-media catalog.
    /// </summary>
    public bool IsSegmented => Segments.Length > 1;

    public static SpeechScript Create(
        string templateId,
        string templateVersion,
        string exactText,
        string contentHash,
        string summaryHash,
        IEnumerable<SpeechSegment>? segments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(exactText);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(summaryHash);
        string normalizedText = exactText.Trim().Normalize(NormalizationForm.FormC);
        if (normalizedText.Length > 4_000)
        {
            throw new ArgumentOutOfRangeException(nameof(exactText));
        }

        ImmutableArray<SpeechSegment> playbackSegments = segments is null
            ? [SpeechSegment.CreateDynamic(1, null, normalizedText)]
            : SpeechSegmentValidation.Validate(segments, normalizedText, nameof(segments));

        return new SpeechScript(
            templateId.Trim(),
            templateVersion.Trim(),
            normalizedText,
            contentHash.Trim(),
            summaryHash.Trim(),
            playbackSegments);
    }

    public override string ToString() => "[REDACTED_SPEECH_SCRIPT]";
}

/// <summary>
/// Vendor-neutral boundary for converting approved speech text into playable audio.
/// </summary>
public interface ITtsProvider
{
    public Task<RenderedAudio> SynthesizeAsync(
        SpeechScript script,
        TtsOptions options,
        CancellationToken cancellationToken);
}

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
        string summaryHash)
    {
        TemplateId = templateId;
        TemplateVersion = templateVersion;
        ExactText = exactText;
        ContentHash = contentHash;
        SummaryHash = summaryHash;
    }

    public string TemplateId { get; }

    public string TemplateVersion { get; }

    /// <summary>
    /// Final rendered text. This value must never be logged or persisted in normal evidence.
    /// </summary>
    public string ExactText { get; }

    public string ContentHash { get; }

    public string SummaryHash { get; }

    public static SpeechScript Create(
        string templateId,
        string templateVersion,
        string exactText,
        string contentHash,
        string summaryHash)
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

        return new SpeechScript(
            templateId.Trim(),
            templateVersion.Trim(),
            normalizedText,
            contentHash.Trim(),
            summaryHash.Trim());
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

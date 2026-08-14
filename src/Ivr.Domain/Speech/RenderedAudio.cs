using Ivr.Domain.Privacy;

namespace Ivr.Domain.Speech;

/// <summary>
/// Playable-audio metadata. The actual content remains behind an opaque provider reference.
/// </summary>
public sealed record RenderedAudio
{
    private RenderedAudio(
        string format,
        int sampleRate,
        TimeSpan duration,
        string contentRef)
    {
        Format = format;
        SampleRate = sampleRate;
        Duration = duration;
        ContentRef = contentRef;
    }

    public string Format { get; }

    public int SampleRate { get; }

    public TimeSpan Duration { get; }

    public string ContentRef { get; }

    public static RenderedAudio Create(
        string format,
        int sampleRate,
        TimeSpan duration,
        string contentRef)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRef);
        if (sampleRate is < 8_000 or > 192_000)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        if (duration <= TimeSpan.Zero || duration > TimeSpan.FromMinutes(5))
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        string safeReference = contentRef.Trim();
        if (safeReference.Length > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(contentRef));
        }

        PiiGuard.EnsureSafeText(safeReference);
        return new RenderedAudio(format.Trim(), sampleRate, duration, safeReference);
    }

    public override string ToString() => "[REDACTED_RENDERED_AUDIO]";
}

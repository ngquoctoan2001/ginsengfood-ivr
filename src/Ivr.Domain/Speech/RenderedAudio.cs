using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Privacy;

namespace Ivr.Domain.Speech;

/// <summary>
/// One playable piece of a call, paired with the script segment it was produced from.
/// </summary>
/// <param name="SegmentHash">
/// <see cref="SpeechSegment.TextHash"/> of the text this audio speaks. Carrying it forward is
/// what makes "this file says that sentence" checkable after the fact instead of assumed.
/// </param>
public sealed record RenderedAudioSegment(string SegmentHash, string ContentRef, TimeSpan Duration)
{
    public override string ToString() => "[REDACTED_RENDERED_AUDIO_SEGMENT]";
}

/// <summary>
/// Playable-audio metadata. The actual content remains behind an opaque provider reference.
/// <para>
/// A call is an ordered list of pieces, not one file. Single-piece audio is still the common
/// case and keeps working unchanged: <see cref="Create"/> produces a one-segment playlist whose
/// <see cref="ContentRef"/> behaves exactly as before.
/// </para>
/// </summary>
public sealed record RenderedAudio
{
    private RenderedAudio(
        string format,
        int sampleRate,
        TimeSpan duration,
        string contentRef,
        ImmutableArray<RenderedAudioSegment> segments,
        string playlistHash,
        DispatchedVoice? voice = null)
    {
        Format = format;
        SampleRate = sampleRate;
        Duration = duration;
        ContentRef = contentRef;
        Segments = segments;
        PlaylistHash = playlistHash;
        Voice = voice;
    }

    /// <summary>
    /// The voice this audio was produced with (W-0113). Null on audio built by a path that does
    /// not choose a voice — a static LAB file, or a test double.
    /// <para>
    /// It rides on the audio rather than being passed alongside it because the audio is the thing
    /// a customer hears, and a voice carried separately is a voice that can be handed to the
    /// wrong recording by a later refactor.
    /// </para>
    /// </summary>
    public DispatchedVoice? Voice { get; }

    /// <summary>
    /// Attaches the voice chosen for this order. Separate from <see cref="CreatePlaylist"/> so
    /// every existing construction site keeps its exact signature and its exact behaviour.
    /// </summary>
    public RenderedAudio WithVoice(DispatchedVoice voice)
    {
        ArgumentNullException.ThrowIfNull(voice);
        return new RenderedAudio(
            Format,
            SampleRate,
            Duration,
            ContentRef,
            Segments,
            PlaylistHash,
            voice);
    }

    public string Format { get; }

    public int SampleRate { get; }

    /// <summary>Sum of every segment's duration.</summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// First segment's reference. Retained so single-segment callers and existing evidence read
    /// exactly as they did before segmentation; multi-segment playback must use
    /// <see cref="Segments"/>.
    /// </summary>
    public string ContentRef { get; }

    public ImmutableArray<RenderedAudioSegment> Segments { get; }

    /// <summary>
    /// SHA-256 over the ordered content references, lowercase hex.
    /// <para>
    /// This is the acceptance handle for "two different orders produce two different calls".
    /// Comparing <see cref="ContentRef"/> alone cannot show that: two orders that share an
    /// opening sentence share a first segment, so the first reference is equal while the calls
    /// are not.
    /// </para>
    /// </summary>
    public string PlaylistHash { get; }

    public bool IsPlaylist => Segments.Length > 1;

    public static RenderedAudio Create(
        string format,
        int sampleRate,
        TimeSpan duration,
        string contentRef)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRef);
        return CreatePlaylist(
            format,
            sampleRate,
            [new RenderedAudioSegment(EmptySegmentHash, contentRef, duration)]);
    }

    public static RenderedAudio CreatePlaylist(
        string format,
        int sampleRate,
        IEnumerable<RenderedAudioSegment> segments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        ArgumentNullException.ThrowIfNull(segments);
        if (sampleRate is < 8_000 or > 192_000)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        ImmutableArray<RenderedAudioSegment> ordered =
        [
            .. segments.Select(NormalizeSegment),
        ];
        if (ordered.IsEmpty)
        {
            throw new ArgumentException("Playable audio needs at least one segment.", nameof(segments));
        }

        if (ordered.Length > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(segments));
        }

        TimeSpan total = TimeSpan.Zero;
        foreach (RenderedAudioSegment segment in ordered)
        {
            total += segment.Duration;
        }

        if (total <= TimeSpan.Zero || total > TimeSpan.FromMinutes(5))
        {
            throw new ArgumentOutOfRangeException(nameof(segments));
        }

        return new RenderedAudio(
            format.Trim(),
            sampleRate,
            total,
            ordered[0].ContentRef,
            ordered,
            ComputePlaylistHash(ordered));
    }

    public override string ToString() => "[REDACTED_RENDERED_AUDIO]";

    /// <summary>
    /// Content equality. The generated record comparison would compare
    /// <see cref="ImmutableArray{T}"/> by underlying-array reference, so two structurally
    /// identical playlists built separately would read as different audio.
    /// </summary>
    public bool Equals(RenderedAudio? other) =>
        other is not null
        && string.Equals(Format, other.Format, StringComparison.Ordinal)
        && SampleRate == other.SampleRate
        && Duration == other.Duration
        && string.Equals(ContentRef, other.ContentRef, StringComparison.Ordinal)
        && string.Equals(PlaylistHash, other.PlaylistHash, StringComparison.Ordinal)
        // Two playlists of identical bytes read in different voices are not the same audio, and
        // the point of W-0113 is that the difference is recorded rather than inferred.
        && Equals(Voice, other.Voice);

    public override int GetHashCode() =>
        HashCode.Combine(Format, SampleRate, Duration, ContentRef, PlaylistHash, Voice);

    private const string EmptySegmentHash = "";

    /// <summary>ASCII unit separator, written numerically so no control byte sits in source.</summary>
    private const char CanonicalSeparator = (char)0x1f;

    private static RenderedAudioSegment NormalizeSegment(RenderedAudioSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        ArgumentException.ThrowIfNullOrWhiteSpace(segment.ContentRef);
        if (segment.Duration <= TimeSpan.Zero || segment.Duration > TimeSpan.FromMinutes(5))
        {
            throw new ArgumentOutOfRangeException(nameof(segment));
        }

        string safeReference = segment.ContentRef.Trim();
        if (safeReference.Length > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(segment));
        }

        PiiGuard.EnsureSafeText(safeReference);
        string safeHash = segment.SegmentHash?.Trim() ?? EmptySegmentHash;
        if (safeHash.Length > 0
            && (safeHash.Length != 64 || !safeHash.All(char.IsAsciiHexDigitLower)))
        {
            throw new ArgumentOutOfRangeException(nameof(segment));
        }

        return segment with { SegmentHash = safeHash, ContentRef = safeReference };
    }

    private static string ComputePlaylistHash(ImmutableArray<RenderedAudioSegment> segments)
    {
        string canonical = string.Join(
            CanonicalSeparator,
            segments.Select(segment => segment.ContentRef));
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }
}

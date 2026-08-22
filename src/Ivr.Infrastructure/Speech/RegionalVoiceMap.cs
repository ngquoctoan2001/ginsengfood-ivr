using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

/// <summary>
/// One regional voice. <see cref="SpeakingRate"/> of zero inherits the global rate, and the
/// media fields are only read by <see cref="StaticFileTtsProvider"/> in LAB.
/// </summary>
public sealed class RegionalVoiceEntry
{
    public string VoiceId { get; set; } = string.Empty;

    /// <summary>Zero means "use the global <c>SpeakingRate</c>".</summary>
    public decimal SpeakingRate { get; set; }

    public string FileMediaReference { get; set; } = string.Empty;

    public int FileDurationSeconds { get; set; }

    /// <summary>
    /// Pre-recorded fixed prose in this region's voice, keyed by what each file says. Empty
    /// until segmentation is turned on with <see cref="FixedSegmentSource.Catalog"/>.
    /// </summary>
    public FixedSegmentMediaEntry[] FixedSegments { get; set; } = [];

    public override string ToString() => "[REDACTED_REGIONAL_VOICE_ENTRY]";
}

public sealed class RegionalVoiceOptions
{
    public bool Enabled { get; set; }

    /// <summary>
    /// Region used when the delivery area names no province. This is also handed to the script
    /// renderer as <c>ScriptRenderOptions.FallbackRegion</c> so the spoken lexicon and the voice
    /// never disagree.
    /// </summary>
    public VietnamRegion FallbackRegion { get; set; } = VietnamRegion.North;

    public RegionalVoiceEntry North { get; set; } = new();

    public RegionalVoiceEntry Central { get; set; } = new();

    public RegionalVoiceEntry South { get; set; } = new();

    public RegionalVoiceEntry For(VietnamRegion region) => region switch
    {
        VietnamRegion.North => North,
        VietnamRegion.Central => Central,
        VietnamRegion.South => South,
        _ => throw new ArgumentOutOfRangeException(nameof(region)),
    };

    public override string ToString() => "[REDACTED_REGIONAL_VOICE_OPTIONS]";
}

public sealed record RegionalVoiceSelection(
    VietnamRegion Region,
    bool ResolvedFromDeliveryArea,
    string VoiceId,
    decimal SpeakingRate);

/// <summary>
/// Chooses the voice for one order from its delivery area.
/// <para>
/// Region resolution happens here and nowhere else. Everything downstream — the audio cache, the
/// static-file provider, telemetry — keys off the resulting <c>VoiceId</c>, so there is exactly
/// one place that can decide a customer hears the wrong region.
/// </para>
/// </summary>
public sealed class RegionalVoiceMap(IOptions<TtsProviderOptions> providerOptions)
{
    public bool Enabled => providerOptions.Value.RegionalVoices.Enabled;

    /// <summary>
    /// Region used when a delivery area names no province. Shared with the script renderer so
    /// the spoken lexicon and the voice cannot fall back to different regions.
    /// </summary>
    public VietnamRegion FallbackRegion => providerOptions.Value.RegionalVoices.FallbackRegion;

    public RegionalVoiceSelection Resolve(string? deliveryAreaShort)
    {
        TtsProviderOptions configured = providerOptions.Value;
        RegionalVoiceOptions regional = configured.RegionalVoices;
        if (!regional.Enabled)
        {
            // Rollback path: one voice for every order, as before W-0106. Note this switches the
            // VOICE only — the spoken-number lexicon still follows the delivery area, because
            // reading "ngàn" to a Southern customer is right whether there are one or three
            // voices. Turning this off does not make a Southern order say "nghìn" again.
            return new RegionalVoiceSelection(
                regional.FallbackRegion,
                false,
                configured.VoiceId,
                configured.SpeakingRate);
        }

        VietnamRegion? resolved = DeliveryRegionResolver.TryResolve(deliveryAreaShort);
        VietnamRegion region = resolved ?? regional.FallbackRegion;
        RegionalVoiceEntry entry = regional.For(region);
        return new RegionalVoiceSelection(
            region,
            resolved is not null,
            entry.VoiceId,
            entry.SpeakingRate == 0m ? configured.SpeakingRate : entry.SpeakingRate);
    }

    /// <summary>
    /// Maps a voice back to its LAB media file. The provider only ever sees a voice id, so this
    /// reverse lookup is what keeps the file choice and the voice choice from drifting apart.
    /// </summary>
    public bool TryGetMedia(string voiceId, out string mediaReference, out int durationSeconds)
    {
        mediaReference = string.Empty;
        durationSeconds = 0;
        RegionalVoiceOptions regional = providerOptions.Value.RegionalVoices;
        if (!regional.Enabled || string.IsNullOrWhiteSpace(voiceId))
        {
            return false;
        }

        foreach (VietnamRegion region in Enum.GetValues<VietnamRegion>())
        {
            RegionalVoiceEntry entry = regional.For(region);
            if (string.Equals(entry.VoiceId, voiceId, StringComparison.Ordinal))
            {
                mediaReference = entry.FileMediaReference;
                durationSeconds = entry.FileDurationSeconds;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Recordings a voice has for the fixed prose of the script, keyed by segment text hash.
    /// <para>
    /// With regional voices on, each voice owns its own catalog; with them off, the single
    /// global catalog applies. An unknown voice returns an empty catalog rather than falling
    /// back to another voice's recordings, because a fallback here is precisely the failure
    /// this design prevents: one region's sentences read in another region's voice.
    /// </para>
    /// </summary>
    public IReadOnlyDictionary<string, FixedSegmentMediaEntry> FixedSegmentCatalog(string voiceId)
    {
        TtsProviderOptions configured = providerOptions.Value;
        FixedSegmentMediaEntry[] entries = configured.FixedSegments;
        if (configured.RegionalVoices.Enabled)
        {
            entries = [];
            if (!string.IsNullOrWhiteSpace(voiceId))
            {
                foreach (VietnamRegion region in Enum.GetValues<VietnamRegion>())
                {
                    RegionalVoiceEntry entry = configured.RegionalVoices.For(region);
                    if (string.Equals(entry.VoiceId, voiceId, StringComparison.Ordinal))
                    {
                        entries = entry.FixedSegments;
                        break;
                    }
                }
            }
        }

        var catalog = new Dictionary<string, FixedSegmentMediaEntry>(StringComparer.Ordinal);
        foreach (FixedSegmentMediaEntry entry in entries)
        {
            string hash = entry.TextHash?.Trim().ToLowerInvariant() ?? string.Empty;
            if (hash.Length > 0)
            {
                catalog[hash] = entry;
            }
        }

        return catalog;
    }
}

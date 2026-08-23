namespace Ivr.Domain.Speech;

/// <summary>
/// The voice one call attempt was dispatched with, as a fact recorded at dispatch rather than a
/// value re-derived at read time (W-0113).
/// <para>
/// The distinction is the whole point. <c>voice_region</c> used to be computed from the stored
/// delivery area whenever a screen asked for it, which made it a function of today's
/// configuration and today's province table — not a record of what a customer heard. One config
/// change between the call and the read, and every earlier evidence pack silently starts
/// describing a voice that was never played. Nothing turns red; the numbers simply become wrong,
/// and they become wrong in the artefact an owner signs.
/// </para>
/// </summary>
/// <param name="VoiceId">
/// The provider voice id actually handed to TTS. Kept alongside the region because the region is
/// a three-value summary and two deployments can map the same region to different voices.
/// </param>
/// <param name="ResolvedFromDeliveryArea">
/// True when the region came from a recognised province in the delivery area; false when it came
/// from the configured fallback. "South because we recognised Cần Thơ" and "South because South
/// is the default" are different facts, and only the first one is evidence about this customer.
/// </param>
public sealed record DispatchedVoice(
    string VoiceId,
    VietnamRegion Region,
    bool ResolvedFromDeliveryArea)
{
    /// <summary>Longest voice id the attempt row will store.</summary>
    public const int MaximumVoiceIdLength = 120;

    public static DispatchedVoice Create(
        string voiceId,
        VietnamRegion region,
        bool resolvedFromDeliveryArea)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(voiceId);
        string trimmed = voiceId.Trim();
        if (trimmed.Length > MaximumVoiceIdLength)
        {
            throw new ArgumentOutOfRangeException(nameof(voiceId));
        }

        if (!Enum.IsDefined(region))
        {
            throw new ArgumentOutOfRangeException(nameof(region));
        }

        return new DispatchedVoice(trimmed, region, resolvedFromDeliveryArea);
    }

    /// <summary>
    /// Wire form of the region.
    /// <para>
    /// <c>North</c>/<c>Central</c>/<c>South</c>, not SCREAMING_SNAKE. The derived
    /// <c>voice_region</c> has emitted these since W-0106, they are pinned in the OpenAPI enum
    /// and they key the console's <c>voiceRegion</c> dictionary — a recorded value in a second
    /// spelling would render as a raw code on exactly the screens this work exists to make
    /// trustworthy.
    /// </para>
    /// </summary>
    public string RegionWireForm => Region switch
    {
        VietnamRegion.North => "North",
        VietnamRegion.Central => "Central",
        VietnamRegion.South => "South",
        _ => throw new InvalidOperationException("Unsupported Vietnamese region."),
    };

    public static bool TryParseRegion(string? value, out VietnamRegion region)
    {
        switch (value?.Trim().ToUpperInvariant())
        {
            case "NORTH":
                region = VietnamRegion.North;
                return true;
            case "CENTRAL":
                region = VietnamRegion.Central;
                return true;
            case "SOUTH":
                region = VietnamRegion.South;
                return true;
            default:
                region = default;
                return false;
        }
    }

    /// <summary>A voice id is not customer data, but it is not console prose either.</summary>
    public override string ToString() => "[REDACTED_DISPATCHED_VOICE]";
}

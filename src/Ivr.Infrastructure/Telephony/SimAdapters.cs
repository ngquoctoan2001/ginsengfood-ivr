namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// The spelling of <c>ivr_sim_channels.adapter_mode</c> and <c>provider_name</c> — which telephony
/// adapter owns a SIM channel row.
/// <para>
/// This is the third thing in this system spelled <c>"MOCK"</c>, and until <c>W-0272</c> all three
/// were written as bare literals, which is why S2 in the 2026-09-09 audit warned that replacing
/// them mechanically would be wrong about half the time. The other two are
/// <see cref="Ivr.Domain.Confirmation.ExecutionModes.Mock"/> — how the whole system is running —
/// and <c>FeatureFlagValues.MockSimProvider</c> — which SIM provider the configuration selects.
/// A channel row can be MOCK while the execution mode is not, so they are not interchangeable.
/// </para>
/// <para>
/// <b>Known divergence, recorded rather than resolved.</b> The code accepts
/// <see cref="Mock"/>, <see cref="Vendor"/> and <see cref="AsteriskAri"/>, but
/// <c>specs/database/02-tables.md:174</c> declares this column as <c>MOCK/REAL</c> and
/// <c>specs/database/06-migration-plan.md:26</c> writes <c>adapter_mode=REAL</c> — a value no code
/// path produces or accepts. Which vocabulary is right is an owner decision, and naming the
/// constants here is what makes the disagreement visible instead of leaving it spread across
/// literals nobody can grep for a meaning.
/// </para>
/// </summary>
public static class SimAdapters
{
    /// <summary>No real SIM behind the channel; the mock gateway answers for it.</summary>
    public const string Mock = "MOCK";

    /// <summary>A vendor-supplied SIM adapter.</summary>
    public const string Vendor = "VENDOR";

    /// <summary>The Asterisk ARI lab adapter (<c>W-0104</c>).</summary>
    public const string AsteriskAri = "ASTERISK_ARI";

    /// <summary>
    /// No channel to read an adapter from — reported by the dashboard's SIM panel, never stored on
    /// a channel row, because a row always has one.
    /// <para>
    /// W-0278. The panel used to answer that question with the configured execution mode, which is
    /// already its own field two lines up in the same response, so one field carried two
    /// vocabularies and a lab run at LAB_REAL_SIM with nothing provisioned would report
    /// <c>adapter_mode: "LAB_REAL_SIM"</c>. Owner chose a sentinel over omitting the field: it
    /// keeps the property required, and it is the reason the panel's adapter_mode could finally be
    /// closed to an enum the way IvrSimChannel's was in W-0275.
    /// </para>
    /// </summary>
    public const string None = "NONE";
}

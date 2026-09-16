using Ivr.Domain.Confirmation;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// How a carrier expects the destination digits on the wire. Carriers disagree, and getting it
/// wrong fails as "call never connects" rather than as an error, so it is configuration rather
/// than a constant.
/// </summary>
public enum SipTrunkNumberFormat
{
    /// <summary><c>+84912345678</c>.</summary>
    E164Plus = 0,

    /// <summary><c>84912345678</c>.</summary>
    E164NoPlus = 1,

    /// <summary><c>0912345678</c>.</summary>
    National = 2,
}

/// <summary>
/// PD-01.2. The carrier-facing half of the production dial path: which Asterisk endpoint carries
/// the trunk, which host the carrier terminates on, and which identity it lets us present.
/// <para>
/// Deliberately separate from <see cref="AsteriskAriOptions"/>. Those options describe the lab
/// softphone profile and are pinned to it by their own validator; folding trunk settings in there
/// would mean one of the two profiles had to relax, and the profile that would relax is the one
/// whose whole job is refusing to dial anything but <c>LAB-A</c>.
/// </para>
/// <para>
/// Every value here arrives from the carrier's integration document. Until that document exists the
/// section stays absent, <see cref="Enabled"/> stays <see langword="false"/>, and the validator
/// returns success without asserting anything - the same shape the Asterisk options use, so a
/// half-configured deployment cannot start the production path by accident.
/// </para>
/// </summary>
public sealed class SipTrunkOptions
{
    public const string SectionName = "Ivr:Telephony:SipTrunk";

    public const string Adapter = "SIP_TRUNK";

    public bool Enabled { get; set; }

    public string ExecutionMode { get; set; } = ExecutionModes.Mock;

    /// <summary>Carrier this trunk belongs to, for evidence and cost reconciliation.</summary>
    public string ProviderName { get; set; } = string.Empty;

    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// PJSIP endpoint name configured on the Asterisk side for this trunk. It is concatenated into
    /// the ARI <c>endpoint</c> parameter, so the validator restricts it to identifier characters -
    /// a slash or space here would let configuration redirect a dial.
    /// </summary>
    public string TrunkEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Host the carrier terminates SIP on. Goes into the dial address as <c>sip:NUMBER@HOST</c>,
    /// hence the same restriction and the same reason.
    /// </summary>
    public string CarrierSipHost { get; set; } = string.Empty;

    /// <summary>
    /// The number the carrier permits us to present. This is the company's own hotline, never a
    /// customer number, and it is the value a registered <c>tên định danh</c> is attached to.
    /// </summary>
    public string OutboundCallerId { get; set; } = string.Empty;

    public SipTrunkNumberFormat NumberFormat { get; set; } = SipTrunkNumberFormat.E164Plus;

    /// <summary>
    /// Concurrent channels the contract actually grants. Separate from the scheduler's own ceiling
    /// so that the two can be compared: configuring more workers than the carrier sells produces
    /// carrier-side rejections, which look like network faults and are not.
    /// </summary>
    public int ContractedChannels { get; set; }

    /// <summary>
    /// New calls per second the carrier accepts. Distinct from <see cref="ContractedChannels"/>:
    /// holding N channels does not mean N calls may be opened inside one second, and carriers
    /// police the two limits separately.
    /// </summary>
    public int MaxCallStartsPerSecond { get; set; }

    /// <summary>
    /// DTMF mode the carrier says it sends. Asterisk enforces the real behaviour in its own PJSIP
    /// configuration; this copy exists so the value the carrier stated is recorded next to the rest
    /// of the trunk and can be diffed against what the dial plan was actually given.
    /// <para>
    /// The failure this guards against is the worst-behaved one on the whole path: a mismatch loses
    /// keypresses silently. The call connects, the customer presses 1, and nothing arrives.
    /// </para>
    /// </summary>
    public string DtmfMode { get; set; } = SipTrunkDtmfModes.Rfc2833;

    public override string ToString() => "[REDACTED_SIP_TRUNK_OPTIONS]";
}

/// <summary>DTMF transports this adapter will accept a carrier declaring.</summary>
public static class SipTrunkDtmfModes
{
    public const string Rfc2833 = "rfc2833";

    public const string SipInfo = "info";

    /// <summary>
    /// Tones carried as audio. Accepted as a declaration because some carriers still offer it, but
    /// it is the least reliable of the three once a compressed codec is in the path.
    /// </summary>
    public const string Inband = "inband";

    public static IReadOnlyList<string> All { get; } = [Rfc2833, SipInfo, Inband];
}

public sealed class SipTrunkOptionsValidator : IValidateOptions<SipTrunkOptions>
{
    public ValidateOptionsResult Validate(string? name, SipTrunkOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        // The production path is the only mode this trunk may serve. MOCK and the softphone lab
        // have their own adapters, and a trunk that answered to either would put a carrier route
        // behind a profile whose guards were written for a destination that cannot be billed.
        if (!string.Equals(
                options.ExecutionMode,
                ExecutionModes.ProductionReal,
                StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("The SIP trunk serves PRODUCTION_REAL execution only.");
        }

        if (!IsSafeIdentifier(options.TrunkEndpoint))
        {
            failures.Add("SipTrunk TrunkEndpoint must be ASCII letters, digits, dash or underscore.");
        }

        if (!IsSafeHost(options.CarrierSipHost))
        {
            failures.Add("SipTrunk CarrierSipHost must be a bare hostname or IP address.");
        }

        if (!IsSafeIdentifier(options.ProviderName) || !IsSafeIdentifier(options.Environment))
        {
            failures.Add("SipTrunk ProviderName and Environment must be ASCII identifiers.");
        }

        if (!IsPlausibleOutboundNumber(options.OutboundCallerId))
        {
            failures.Add("SipTrunk OutboundCallerId must be the carrier-assigned number in digits.");
        }

        if (!Enum.IsDefined(options.NumberFormat))
        {
            failures.Add("SipTrunk NumberFormat is not a known format.");
        }

        // An enabled trunk that grants no channels cannot place a call, and one claiming hundreds
        // is far likelier to be a typo than a contract. Both are refused at startup rather than
        // discovered when the carrier starts rejecting.
        if (options.ContractedChannels is < 1 or > 200)
        {
            failures.Add("SipTrunk ContractedChannels must be between 1 and 200.");
        }

        if (options.MaxCallStartsPerSecond is < 1 or > 100)
        {
            failures.Add("SipTrunk MaxCallStartsPerSecond must be between 1 and 100.");
        }
        else if (options.ContractedChannels >= 1
                 && options.MaxCallStartsPerSecond > options.ContractedChannels)
        {
            // Opening calls faster than the trunk can hold them only buys carrier rejections.
            failures.Add("SipTrunk MaxCallStartsPerSecond cannot exceed ContractedChannels.");
        }

        if (!SipTrunkDtmfModes.All.Contains(options.DtmfMode, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add(
                $"SipTrunk DtmfMode must be one of: {string.Join(", ", SipTrunkDtmfModes.All)}.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsSafeIdentifier(string value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length <= 120
        && value.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '-' or '_');

    /// <summary>
    /// A bare host or IP. No scheme, no port, no path, no userinfo - anything richer would be
    /// carrying routing information into a string that is about to become a SIP URI.
    /// </summary>
    private static bool IsSafeHost(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 253)
        {
            return false;
        }

        if (System.Net.IPAddress.TryParse(value, out _))
        {
            return true;
        }

        return value.All(character =>
                   char.IsAsciiLetterOrDigit(character) || character is '-' or '.')
               && !value.StartsWith('.')
               && !value.StartsWith('-')
               && !value.EndsWith('.')
               && !value.EndsWith('-')
               && value.Contains('.');
    }

    /// <summary>
    /// The company's own outbound number. Shape-checked rather than pattern-matched against a
    /// national plan, because the carrier assigns it and a fixed-line hotline, a mobile hotline and
    /// a short code are all legitimate here.
    /// </summary>
    private static bool IsPlausibleOutboundNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        string digits = string.Concat(trimmed.Where(char.IsDigit));
        return digits.Length is >= 4 and <= 15
               && trimmed.All(character => char.IsDigit(character) || character == '+')
               && trimmed.IndexOf('+') <= 0;
    }
}

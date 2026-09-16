using Microsoft.Extensions.Options;
using Ivr.Domain.Confirmation;

namespace Ivr.Infrastructure.Telephony;

public sealed class AsteriskAriOptions
{
    public const string SectionName = "Ivr:Telephony:Asterisk";
    public const string Adapter = "ASTERISK_ARI";
    public const string DefaultDestinationAlias = "LAB-A";
    public const string DefaultSimChannelId = "SIM-ASTERISK-001";

    public bool Enabled { get; set; }

    public string ExecutionMode { get; set; } = ExecutionModes.Mock;

    public string BaseUrl { get; set; } = "http://asterisk:8088";

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Application { get; set; } = "ivr-lab";

    public string Environment { get; set; } = "lab";

    public string DestinationAlias { get; set; } = DefaultDestinationAlias;

    public string SimChannelId { get; set; } = DefaultSimChannelId;

    public string AdapterMode { get; set; } = Adapter;

    public string ProviderName { get; set; } = Adapter;

    public int DialTimeoutSeconds { get; set; } = 30;

    public int DtmfTimeoutSeconds { get; set; } = 15;

    /// <summary>
    /// How often the dispatch loop asks whether an operator has requested a cut (W-0111).
    /// The upper bound on how long a customer keeps hearing a call somebody already decided to
    /// stop, so it is a safety number rather than a tuning one.
    /// </summary>
    public int TerminationPollMilliseconds { get; set; } = 500;

    public int CooldownSeconds { get; set; } = 2;

    public bool RecordingEnabled { get; set; }

    public override string ToString() => "[REDACTED_ASTERISK_ARI_OPTIONS]";
}

public sealed class AsteriskAriOptionsValidator : IValidateOptions<AsteriskAriOptions>
{
    public ValidateOptionsResult Validate(string? name, AsteriskAriOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var failures = new List<string>();
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        // PD-01.3. Two profiles now, and the split is the whole point: the softphone lab stays
        // pinned to its alias exactly as before, and production is allowed through under a
        // different set of assertions rather than by relaxing the lab's.
        //
        // What production drops is the pinned destination, because it does not have one - the
        // number arrives per call from ProductionDialTokenVault. What it keeps is every guard that
        // was never about the lab: a local Asterisk, real credentials, bounded timeouts and
        // recording off.
        bool lab = string.Equals(
            options.ExecutionMode,
            ExecutionModes.LabRealSim,
            StringComparison.OrdinalIgnoreCase);
        bool production = string.Equals(
            options.ExecutionMode,
            ExecutionModes.ProductionReal,
            StringComparison.OrdinalIgnoreCase);
        if (!lab && !production)
        {
            failures.Add("Asterisk ARI serves LAB_REAL_SIM or PRODUCTION_REAL execution only.");
        }

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out Uri? baseUri)
            || baseUri.Scheme is not ("http" or "https"))
        {
            failures.Add("Asterisk BaseUrl must be an absolute HTTP(S) URL.");
        }
        else if ((!string.Equals(baseUri.Host, "asterisk", StringComparison.OrdinalIgnoreCase)
                  && !string.Equals(baseUri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                  && (!System.Net.IPAddress.TryParse(baseUri.Host, out System.Net.IPAddress? address)
                      || !System.Net.IPAddress.IsLoopback(address)))
                 || !string.IsNullOrEmpty(baseUri.UserInfo)
                 || !string.IsNullOrEmpty(baseUri.Query)
                 || !string.IsNullOrEmpty(baseUri.Fragment))
        {
            failures.Add("Asterisk BaseUrl must target the local Asterisk service without embedded credentials or query data.");
        }

        if (string.IsNullOrWhiteSpace(options.Username)
            || string.IsNullOrWhiteSpace(options.Password))
        {
            failures.Add("Asterisk ARI credentials are required when the adapter is enabled.");
        }

        // DestinationAlias joins this check only outside production, where it is expected to be
        // absent. Folded into one condition rather than checked separately so the lab still
        // reports the identifier problem exactly once, as it did before this split.
        if (!IsSafeIdentifier(options.Application)
            || !IsSafeIdentifier(options.Environment)
            || !IsSafeIdentifier(options.SimChannelId)
            || (!production && !IsSafeIdentifier(options.DestinationAlias)))
        {
            failures.Add("Asterisk identifiers may contain only ASCII letters, digits, dash and underscore.");
        }

        if (production)
        {
            // A pinned destination in production is either a leftover lab value or somebody
            // hand-configuring a number. Both dial somewhere the token did not authorise, so the
            // field has to be absent rather than merely ignored.
            if (!string.IsNullOrWhiteSpace(options.DestinationAlias))
            {
                failures.Add("The production profile resolves each destination per call and must not pin a DestinationAlias.");
            }

            // The adapter is still Asterisk ARI; only what sits behind it changed. The carrier's
            // own name lives in SipTrunkOptions, where cost reconciliation can reach it.
            if (!string.Equals(options.AdapterMode, AsteriskAriOptions.Adapter, StringComparison.Ordinal)
                || !string.Equals(options.ProviderName, AsteriskAriOptions.Adapter, StringComparison.Ordinal))
            {
                failures.Add("The production profile still dials through the ARI adapter.");
            }
        }
        else
        {
            if (!string.Equals(options.DestinationAlias, AsteriskAriOptions.DefaultDestinationAlias, StringComparison.Ordinal)
                || !string.Equals(options.SimChannelId, AsteriskAriOptions.DefaultSimChannelId, StringComparison.Ordinal)
                || !string.Equals(options.AdapterMode, AsteriskAriOptions.Adapter, StringComparison.Ordinal)
                || !string.Equals(options.ProviderName, AsteriskAriOptions.Adapter, StringComparison.Ordinal))
            {
                failures.Add("The free softphone profile is pinned to its lab alias, channel and ARI adapter.");
            }
        }

        if (options.DialTimeoutSeconds is < 5 or > 120
            || options.DtmfTimeoutSeconds is < 1 or > 120
            || options.CooldownSeconds is < 0 or > 3600)
        {
            failures.Add("Asterisk timeout or cooldown bounds are invalid.");
        }

        if (options.RecordingEnabled)
        {
            failures.Add("Call recording must remain disabled in the softphone lab.");
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
}

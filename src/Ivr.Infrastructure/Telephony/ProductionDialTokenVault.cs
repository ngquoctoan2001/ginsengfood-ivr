using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence.Security;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// PD-01.1. Resolves a task's dial authorisation into a carrier destination for the production
/// trunk. This is the only place in the system where a customer number exists in the clear, and it
/// exists there for the length of one dial and no longer.
/// <para>
/// Unlike <see cref="LabDialTokenVault"/> this type does <b>not</b> implement
/// <see cref="IOpaqueValueProtector"/>. The lab is its own protector because its "protection" is an
/// irreversible fingerprint it can generate alone. Production protection is a platform-managed key,
/// so the vault consumes the protector rather than being one - which also means a deployment
/// missing the key gets <see cref="UnavailableOpaqueValueProtector"/> and fails closed at the first
/// resolve instead of dialling something it could not secure.
/// </para>
/// <para>
/// <c>OD-V1-18</c>. The revealed number is returned inside <see cref="DialAuthorization"/> and
/// never written to the application database, a log, a trace, evidence or a callback. Nothing in
/// this type puts it in an exception message either: a refusal names the token's task and the
/// refusal code, because those identify the problem and the number does not.
/// </para>
/// </summary>
public sealed class ProductionDialTokenVault(
    IOptions<SipTrunkOptions> options,
    IOpaqueValueProtector protector,
    IDialTokenResolveLedger ledger,
    IAuditLogger? auditLogger = null) : IDialTokenResolver
{
    private const string DialTokenPurpose = "ivr-confirmation-task-dial-token";

    public async ValueTask<DialAuthorization> ResolveAsync(
        DialTokenResolutionRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        SipTrunkOptions configured = options.Value;
        if (!configured.Enabled)
        {
            throw new InvalidOperationException("The production dial-token vault is disabled.");
        }

        // The ledger runs before the protector, and the order is deliberate. A token that is
        // expired, bound to another task, replaying an attempt or past its ceiling is refused
        // without ever being decrypted, so a rejected dial never materialises a customer number
        // in this process at all.
        DialTokenResolveDecision decision = await ledger.EvaluateAsync(
            request,
            now,
            cancellationToken);
        await DialTokenResolveAudit.RecordAsync(
            auditLogger,
            nameof(ProductionDialTokenVault),
            request,
            decision,
            cancellationToken);
        if (!decision.Allowed)
        {
            throw new DialTokenRefusedException(
                decision.RefusalCode!,
                "The production dial token was refused.");
        }

        string revealed;
        try
        {
            revealed = protector.Unprotect(
                DialTokenPurpose,
                request.DialToken.RevealToTrustedResolver());
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The inner exception is not chained on purpose. A protector failure can carry the
            // ciphertext or key material in its message, and this exception travels up into
            // dispatch logging.
            throw new InvalidOperationException(
                "The production dial token could not be unprotected.");
        }

        if (!VietnameseDestinationNumber.TryParse(revealed, out string nationalDigits))
        {
            throw new InvalidOperationException(
                "The resolved dial destination is not a usable Vietnamese number.");
        }

        string dialed = VietnameseDestinationNumber.Format(nationalDigits, configured.NumberFormat);

        // The gateway owns the trunk: PD-01.5 builds "PJSIP/{TrunkEndpoint}/{this value}". Keeping
        // the endpoint out of here leaves the vault answering only "which destination", which is
        // the question the token actually authorises.
        return DialAuthorization.CreateTrusted(
            string.Concat("sip:", dialed, "@", configured.CarrierSipHost));
    }
}

/// <summary>
/// Normalises what Module 3 protected into what a carrier will accept. Kept separate from the vault
/// so it can be tested without a protector, a ledger or an options tree.
/// </summary>
internal static class VietnameseDestinationNumber
{
    /// <summary>
    /// Reduces any accepted spelling to the national significant number - the digits after the
    /// trunk prefix or country code, e.g. <c>912345678</c>.
    /// </summary>
    public static bool TryParse(string value, out string nationalDigits)
    {
        nationalDigits = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string compact = string.Concat(value.Where(character =>
            char.IsDigit(character) || character == '+'));
        if (compact.Length == 0)
        {
            return false;
        }

        // Only a leading plus is meaningful; one anywhere else means the value was not a number.
        if (compact.IndexOf('+') > 0)
        {
            return false;
        }

        string digits = compact.TrimStart('+');
        if (!digits.All(char.IsDigit))
        {
            return false;
        }

        string national =
            digits.StartsWith("84", StringComparison.Ordinal) ? digits[2..]
            : digits.StartsWith('0') ? digits[1..]
            : digits;

        // Vietnamese national significant numbers run 9 digits (fixed line, older ranges) to 10
        // (mobile). Anything outside that is a mangled token, not a destination, and dialling it
        // would bill a call to whoever the digits happen to reach.
        if (national.Length is < 9 or > 10 || national.StartsWith('0'))
        {
            return false;
        }

        nationalDigits = national;
        return true;
    }

    public static string Format(string nationalDigits, SipTrunkNumberFormat format) => format switch
    {
        SipTrunkNumberFormat.E164Plus => string.Concat("+84", nationalDigits),
        SipTrunkNumberFormat.E164NoPlus => string.Concat("84", nationalDigits),
        SipTrunkNumberFormat.National => string.Concat("0", nationalDigits),
        _ => throw new InvalidOperationException("Unknown SIP trunk number format."),
    };
}

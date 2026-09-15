using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence.Security;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Local-only lab vault. Source dial tokens are irreversibly fingerprinted and every
/// eligible token resolves to the single configured softphone alias.
/// </summary>
/// <param name="ledger">
/// SIP-02. Required rather than optional, and not defaulted to the in-memory one.
/// <para>
/// W-0199 chose to give the lab the same ledger as MOCK, because the lab dials a real softphone
/// alias and so the rule it enforces has to be the rule production will enforce. That reasoning
/// holds and this extends it: the durable ledger is now what production would use, so the lab takes
/// it too. A default would mean a misconfigured deployment quietly getting the per-process ceiling
/// and no test ever noticing - which is precisely how the previous one survived this long.
/// </para>
/// </param>
public sealed class LabDialTokenVault(
    IOptions<AsteriskAriOptions> options,
    IDialTokenResolveLedger ledger,
    IAuditLogger? auditLogger = null) : IOpaqueValueProtector, IDialTokenResolver
{
    private const string DialTokenPurpose = "ivr-confirmation-task-dial-token";

    public string Protect(string purpose, string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        if (!string.Equals(purpose, DialTokenPurpose, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The LAB vault only protects dial tokens.");
        }

        return Fingerprint(purpose, plaintext);
    }

    public string Unprotect(string purpose, string ciphertext) =>
        throw new InvalidOperationException(
            "LAB dial-token fingerprints cannot be reversed into source tokens.");

    public async ValueTask<DialAuthorization> ResolveAsync(
        DialTokenResolutionRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        AsteriskAriOptions configured = options.Value;
        if (!configured.Enabled)
        {
            throw new InvalidOperationException("The LAB dial-token vault is disabled.");
        }

        string fingerprint = request.DialToken.RevealToTrustedResolver();
        if (!fingerprint.StartsWith("enc:lab-sha256:", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The LAB dial-token fingerprint is invalid.");
        }

        DialTokenResolveDecision decision = await ledger.EvaluateAsync(
            request,
            now,
            cancellationToken);
        await DialTokenResolveAudit.RecordAsync(
            auditLogger,
            nameof(LabDialTokenVault),
            request,
            decision,
            cancellationToken);
        if (!decision.Allowed)
        {
            throw new DialTokenRefusedException(
                decision.RefusalCode!,
                "The LAB dial token was refused.");
        }

        return DialAuthorization.CreateTrusted(configured.DestinationAlias);
    }

    private static string Fingerprint(string purpose, string plaintext)
    {
        byte[] input = Encoding.UTF8.GetBytes(string.Concat(purpose, "\n", plaintext));
        return string.Concat(
            "enc:lab-sha256:",
            Convert.ToHexString(SHA256.HashData(input)));
    }
}

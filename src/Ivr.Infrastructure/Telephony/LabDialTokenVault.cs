using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Persistence.Security;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Local-only lab vault. Source dial tokens are irreversibly fingerprinted and every
/// eligible token resolves to the single configured softphone alias.
/// </summary>
public sealed class LabDialTokenVault(
    IOptions<AsteriskAriOptions> options) : IOpaqueValueProtector, IDialTokenResolver
{
    private const string DialTokenPurpose = "ivr-confirmation-task-dial-token";
    private readonly ConcurrentDictionary<(string Fingerprint, string AttemptId), byte> consumed =
        new();

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

    public ValueTask<DialAuthorization> ResolveAsync(
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

        if (request.DialToken.ExpiresAt <= now)
        {
            throw new InvalidOperationException("Dial token has expired.");
        }

        string fingerprint = request.DialToken.RevealToTrustedResolver();
        if (!fingerprint.StartsWith("enc:lab-sha256:", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The LAB dial-token fingerprint is invalid.");
        }

        if (!consumed.TryAdd((fingerprint, request.AttemptId.Value), 0))
        {
            throw new InvalidOperationException("Dial token was already resolved for this attempt.");
        }

        return ValueTask.FromResult(DialAuthorization.CreateTrusted(
            configured.DestinationAlias));
    }

    private static string Fingerprint(string purpose, string plaintext)
    {
        byte[] input = Encoding.UTF8.GetBytes(string.Concat(purpose, "\n", plaintext));
        return string.Concat(
            "enc:lab-sha256:",
            Convert.ToHexString(SHA256.HashData(input)));
    }
}

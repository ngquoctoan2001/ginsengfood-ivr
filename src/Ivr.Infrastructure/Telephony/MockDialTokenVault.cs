using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Persistence.Security;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Process-local MOCK trust boundary. It persists only a deterministic fingerprint
/// in IVR storage and keeps fake destination mappings in memory/config.
/// </summary>
public sealed class MockDialTokenVault : IOpaqueValueProtector, IDialTokenResolver
{
    private const string DialTokenPurpose = "ivr-confirmation-task-dial-token";
    private readonly Dictionary<string, string> configuredTokenDestinations;
    private readonly HashSet<string> destinationAllowlist;
    private readonly ConcurrentDictionary<string, string> destinationsByFingerprint =
        new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<(string Fingerprint, string AttemptId), byte> consumed =
        new();

    public MockDialTokenVault(IOptions<MockTelephonyOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        configuredTokenDestinations = new Dictionary<string, string>(
            options.Value.TokenDestinations,
            StringComparer.Ordinal);
        destinationAllowlist = options.Value.DestinationAllowlist.ToHashSet(StringComparer.Ordinal);
        foreach ((string token, string destination) in configuredTokenDestinations)
        {
            if (!string.Equals(token, "*", StringComparison.Ordinal))
            {
                destinationsByFingerprint[Fingerprint(DialTokenPurpose, token)] = destination;
            }
        }
    }

    public string Protect(string purpose, string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        if (!string.Equals(purpose, DialTokenPurpose, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The MOCK vault only protects dial tokens.");
        }

        string fingerprint = Fingerprint(purpose, plaintext);
        if (configuredTokenDestinations.TryGetValue(plaintext, out string? destination)
            || configuredTokenDestinations.TryGetValue("*", out destination))
        {
            destinationsByFingerprint[fingerprint] = destination;
        }

        return fingerprint;
    }

    public string Unprotect(string purpose, string ciphertext) =>
        throw new InvalidOperationException(
            "MOCK dial-token fingerprints cannot be reversed into source tokens.");

    public ValueTask<DialAuthorization> ResolveAsync(
        DialTokenResolutionRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        if (request.DialToken.ExpiresAt <= now)
        {
            throw new InvalidOperationException("Dial token has expired.");
        }

        string fingerprint = request.DialToken.RevealToTrustedResolver();
        if (!destinationsByFingerprint.TryGetValue(fingerprint, out string? destination))
        {
            throw new KeyNotFoundException("MOCK dial-token fingerprint has no destination mapping.");
        }

        if (!destinationAllowlist.Contains(destination))
        {
            throw new UnauthorizedAccessException("Resolved MOCK destination is not allowlisted.");
        }

        if (!consumed.TryAdd((fingerprint, request.AttemptId.Value), 0))
        {
            throw new InvalidOperationException("Dial token was already resolved for this attempt.");
        }

        return ValueTask.FromResult(DialAuthorization.CreateTrusted(destination));
    }

    private static string Fingerprint(string purpose, string plaintext)
    {
        byte[] input = Encoding.UTF8.GetBytes(string.Concat(purpose, "\n", plaintext));
        return string.Concat("enc:mock-sha256:", Convert.ToHexString(SHA256.HashData(input)));
    }
}

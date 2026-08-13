namespace Ivr.Infrastructure.Persistence.Security;

/// <summary>
/// Protects opaque provider values before they enter persistence. Implementations
/// must use a platform-managed key and must never log plaintext or ciphertext.
/// </summary>
public interface IOpaqueValueProtector
{
    public string Protect(string purpose, string plaintext);

    public string Unprotect(string purpose, string ciphertext);
}

/// <summary>
/// Fail-closed default used until Platform provisions the approved key provider.
/// </summary>
public sealed class UnavailableOpaqueValueProtector : IOpaqueValueProtector
{
    public string Protect(string purpose, string plaintext) =>
        throw new InvalidOperationException("Opaque-value encryption is not configured.");

    public string Unprotect(string purpose, string ciphertext) =>
        throw new InvalidOperationException("Opaque-value encryption is not configured.");
}

/// <summary>
/// Non-reversible local-only protection. It deliberately cannot reveal a dial
/// token and must never be registered outside MOCK execution mode.
/// </summary>
public sealed class MockOnlyOpaqueValueProtector : IOpaqueValueProtector
{
    public string Protect(string purpose, string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        byte[] input = System.Text.Encoding.UTF8.GetBytes(
            string.Concat(purpose, "\n", plaintext));
        return string.Concat(
            "enc:mock-sha256:",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(input)));
    }

    public string Unprotect(string purpose, string ciphertext) =>
        throw new InvalidOperationException(
            "MOCK dial-token fingerprints are deliberately non-reversible.");
}

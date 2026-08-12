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

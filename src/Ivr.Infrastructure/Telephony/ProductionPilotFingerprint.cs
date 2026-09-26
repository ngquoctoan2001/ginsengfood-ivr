using System.Security.Cryptography;
using System.Text;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Q-28 (PA2, 2026-09-26). The production pilot list names customer numbers without holding them.
/// Each entry is a keyed fingerprint (HMAC-SHA256) of a national significant number, so
/// configuration, the dispatch gate, an approval row and any log that carries one can say "this
/// number is on the list" and cannot say which number it is. The key is a deployment secret
/// (<see cref="SipTrunkOptions.PilotFingerprintKey"/>): without it, trying every one of the roughly
/// ten million numbers of a mobile range against a fingerprint gets nowhere.
/// </summary>
public static class ProductionPilotFingerprint
{
    /// <summary>Marks a value as a pilot fingerprint wherever it travels.</summary>
    public const string Prefix = "pilot:";

    /// <summary>The shortest key accepted, the output size of the hash itself.</summary>
    public const int MinimumKeyBytes = 32;

    private const int Length = 70;

    /// <summary>
    /// The fingerprint of one national significant number - the digits
    /// <c>VietnameseDestinationNumber.TryParse</c> yields, e.g. <c>912345678</c> - so every spelling
    /// of one number (<c>0912…</c>, <c>+84912…</c>, <c>84912…</c>) has one fingerprint.
    /// <para>
    /// Written in the letters <c>a</c>–<c>p</c>, one per four bits, rather than in hex.
    /// <c>PiiGuard</c> reads ten digits starting with a zero as a telephone number, and about one hex
    /// digest in twenty contains such a run, which would make some pilot entries impossible to pass
    /// to the dispatch gate. Letters never contain one.
    /// </para>
    /// </summary>
    public static string Of(ReadOnlySpan<byte> key, string nationalDigits)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nationalDigits);
        if (key.Length < MinimumKeyBytes)
        {
            throw new ArgumentException("The pilot fingerprint key is too short.", nameof(key));
        }

        if (nationalDigits.AsSpan().IndexOfAnyExceptInRange('0', '9') >= 0)
        {
            // TryParse accepts any Unicode digit; the key is applied to ASCII only, so anything
            // else would silently fingerprint a different string than the one dialled.
            throw new ArgumentException("A national number is ASCII digits only.", nameof(nationalDigits));
        }

        Span<byte> mac = stackalloc byte[32];
        HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(nationalDigits), mac);
        var text = new StringBuilder(Prefix, Length);
        foreach (byte value in mac)
        {
            text.Append((char)('a' + (value >> 4)));
            text.Append((char)('a' + (value & 0x0F)));
        }

        return text.ToString();
    }

    /// <summary>True for exactly the shape <see cref="Of"/> produces.</summary>
    public static bool IsWellFormed(string? value) =>
        value is { Length: Length }
        && value.StartsWith(Prefix, StringComparison.Ordinal)
        && value.AsSpan(Prefix.Length).IndexOfAnyExceptInRange('a', 'p') < 0;

    /// <summary>
    /// The value a <c>PRODUCTION_PILOT_LIST</c> approval binds in its <c>change_fingerprint</c>:
    /// SHA-256 over the distinct entries in ordinal order, one per line, in upper-case hex like
    /// <c>RuntimeGateFingerprint</c>. The order of the configuration and repeated entries do not
    /// change it; adding, removing or replacing one entry does, and then no approval matches.
    /// </summary>
    public static string ListHash(IEnumerable<string> fingerprints)
    {
        ArgumentNullException.ThrowIfNull(fingerprints);
        string canonical = string.Join(
            '\n',
            fingerprints.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    /// <summary>Reads the configured key: base64, at least <see cref="MinimumKeyBytes"/> bytes.</summary>
    public static bool TryReadKey(string? configured, out byte[] key)
    {
        key = [];
        if (string.IsNullOrWhiteSpace(configured))
        {
            return false;
        }

        try
        {
            key = Convert.FromBase64String(configured.Trim());
        }
        catch (FormatException)
        {
            key = [];
            return false;
        }

        return key.Length >= MinimumKeyBytes;
    }
}

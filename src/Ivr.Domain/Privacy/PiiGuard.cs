using System.Text.RegularExpressions;

namespace Ivr.Domain.Privacy;

public static class PiiGuard
{
    private static readonly HashSet<string> RestrictedFields = new(StringComparer.Ordinal)
    {
        "address",
        "dialtoken",
        "fulladdress",
        "healthnote",
        "paymentdetail",
        "phone",
        "phonenumber",
        "rawphone",
        "recording",
    };

    /// <summary>
    /// Wall clock, not CPU time: .NET charges regex timeouts against elapsed time, so a busy host
    /// can trip a budget the match would never have spent. Two seconds is far past any legitimate
    /// scan -- the pattern has no nested quantifier and measures linear, about 0.085 ms/KB
    /// compiled -- so a timeout now means something genuinely wrong rather than a busy scheduler.
    /// </summary>
    private static readonly TimeSpan MatchBudget = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The phone branch matches ten digits beginning with zero, which also matches a zero-padded
    /// ten-digit identifier. That is a known false positive, and the resolution recorded on
    /// 2026-08-19 (<c>OD-OPEN-02</c>) is a NAMING RULE, not a narrower pattern: identifiers must not
    /// contain a run of ten digits starting with zero.
    /// <para>
    /// Narrowing the pattern was rejected because it is a privacy-policy change in the direction of
    /// detecting less — a real number written after a hyphenated prefix would stop being caught. So
    /// when this guard rejects an identifier, the identifier changes; the pattern does not.
    /// </para>
    /// </summary>
    /// <summary>Vietnamese subscriber numbers, in the shapes this codebase has seen them.</summary>
    private const string PhoneBranch =
        @"(?<![0-9A-Za-z])(?:0[0-9]{9}|(?:84|\+84)[0-9]{9}|0[0-9]{2}[\s.-][0-9]{3}[\s.-][0-9]{4}|(?:84|\+84)[\s.-]*\(?[0-9]{2}\)?[\s.-][0-9]{3}[\s.-][0-9]{4})(?![0-9A-Za-z])";

    private const string DialTokenBranch =
        "(?:dial[_-]?token)[\\\"'`: ]+[A-Za-z0-9._-]{8,}";

    private const string AsciiAddressBranch =
        @"(?<![\p{L}\p{N}])(?:duong|so nha|ngo|hem|ngach|thon|ap)\s+[A-Za-z0-9]";

    private const string DiacriticAddressBranch =
        @"(?<![\p{L}\p{N}])(?:đường|số nhà|ngõ|hẻm|ngách|thôn|ấp|tổ)\s+";

    private static readonly Regex RestrictedValuePattern = new(
        string.Join(
            '|',
            PhoneBranch,
            DialTokenBranch,
            AsciiAddressBranch,
            DiacriticAddressBranch),
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        MatchBudget);

    /// <summary>
    /// Contact-value subset: the phone and dial-token branches only, with the address-keyword
    /// branches left out. Composed from the same constants as
    /// <see cref="RestrictedValuePattern"/>, so the two cannot drift apart.
    /// <para>
    /// This is <b>additive</b> and deliberately does not narrow <see cref="IsSafeText"/>, whose
    /// no-narrowing decision (<c>OD-OPEN-02</c>) stands: when that guard rejects an identifier,
    /// the identifier changes. That resolution works because identifiers are machine-chosen. It
    /// does not work for a person's name — the ASCII address branch matches <c>Duong</c>,
    /// <c>Ngo</c>, <c>Ap</c>, <c>Thon</c> and <c>Hem</c> followed by a space, so an unaccented
    /// <c>Duong Minh Tuan</c> or <c>Ngo Van A</c> is rejected, and Dương and Ngô are ordinary
    /// Vietnamese surnames. Nobody can be asked to change their family name.
    /// </para>
    /// <para>
    /// Use this only for a field whose declared purpose is to hold a person's name, and only
    /// where that field is separately length- and control-character validated. Customer-facing
    /// surfaces keep <see cref="IsSafeText"/>. W-0105; production use of the staff-name path
    /// still needs Privacy sign-off (<c>OWNER_DATA_REQUIRED</c>).
    /// </para>
    /// </summary>
    private static readonly Regex RestrictedContactPattern = new(
        string.Join('|', PhoneBranch, DialTokenBranch),
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        MatchBudget);

    public static bool IsSafeText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        try
        {
            return !RestrictedValuePattern.IsMatch(value);
        }
        catch (RegexMatchTimeoutException)
        {
            // DO-06. A timeout means the guard never finished deciding, which is not the same as
            // deciding the text is clean. Reading "unknown" as safe would wave a value through
            // exactly when the host is under the most load, so unknown counts as unsafe. Callers
            // already handle false: the masking filter raises a policy violation and the
            // correlation middleware mints a fresh id instead of trusting the inbound one.
            return false;
        }
    }

    public static void EnsureSafeField(string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        string normalized = string.Concat(
            fieldName.Where(char.IsAsciiLetterOrDigit)).ToLowerInvariant();

        if (RestrictedFields.Contains(normalized))
        {
            throw new InvalidOperationException("A restricted PII field was rejected.");
        }
    }

    public static void EnsureSafeText(string? value)
    {
        if (!IsSafeText(value))
        {
            throw new InvalidOperationException("A restricted PII value was rejected.");
        }
    }

    /// <summary>
    /// Phone-number and dial-token check without the address-keyword branches. See
    /// <see cref="RestrictedContactPattern"/> for when this is the correct contract and when it
    /// is not. A timeout counts as unsafe, for the same reason as in <see cref="IsSafeText"/>.
    /// </summary>
    public static bool IsSafeContactText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        try
        {
            return !RestrictedContactPattern.IsMatch(value);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public static void EnsureSafeContactText(string? value)
    {
        if (!IsSafeContactText(value))
        {
            throw new InvalidOperationException("A restricted contact value was rejected.");
        }
    }
}

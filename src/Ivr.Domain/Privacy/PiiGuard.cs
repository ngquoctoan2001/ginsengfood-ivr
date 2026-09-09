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

    // The address markers split in two, because four of them are not only address markers.
    //
    // "số nhà", "ngõ", "hẻm" and "ngách" occur in addresses and essentially nowhere else, so a
    // field carrying one is carrying an address.
    private const string AsciiPlaceMarkerBranch =
        @"(?<![\p{L}\p{N}])(?:so nha|ngo|hem|ngach)\s+[A-Za-z0-9]";

    private const string DiacriticPlaceMarkerBranch =
        @"(?<![\p{L}\p{N}])(?:số nhà|ngõ|hẻm|ngách)\s+";

    // "đường", "thôn", "ấp" and "tổ" are also ordinary Vietnamese words, and in a product name
    // they are usually the ordinary sense: đường is sugar before it is a street ("đường phèn",
    // "đường thốt nốt"), and tổ is a nest ("tổ yến"). Held apart so a field can take the markers
    // that only ever mean an address without taking the ones that usually do not. W-0243.
    private const string AsciiAmbiguousMarkerBranch =
        @"(?<![\p{L}\p{N}])(?:duong|thon|ap)\s+[A-Za-z0-9]";

    private const string DiacriticAmbiguousMarkerBranch =
        @"(?<![\p{L}\p{N}])(?:đường|thôn|ấp|tổ)\s+";

    private static readonly Regex RestrictedValuePattern = new(
        string.Join(
            '|',
            PhoneBranch,
            DialTokenBranch,
            AsciiPlaceMarkerBranch,
            DiacriticPlaceMarkerBranch,
            AsciiAmbiguousMarkerBranch,
            DiacriticAmbiguousMarkerBranch),
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

    /// <summary>
    /// Product-field subset: phone and dial-token, plus only the address markers that mean an
    /// address and nothing else. Composed from the same constants as
    /// <see cref="RestrictedValuePattern"/>, so no branch can drift between them.
    /// <para>
    /// The four markers left out are ordinary words. <c>Tổ yến</c>, <c>đường phèn</c> and
    /// <c>đường thốt nốt</c> are products a customer can order, and <see cref="IsSafeText"/>
    /// refuses all three — the order never reaches a confirmation call at all. That is the same
    /// collision <c>W-0105</c> found in people's names, where <c>Dương</c> and <c>Ngô</c> are
    /// ordinary surnames, and it is resolved the same way: a narrower pattern for the field whose
    /// declared content is not an address.
    /// </para>
    /// <para>
    /// The cost is stated rather than hidden. A product name reading <c>"đường Nguyễn Huệ"</c> now
    /// passes this guard, where <see cref="IsSafeText"/> refuses it. Phone numbers, dial tokens,
    /// <c>số nhà</c>, <c>ngõ</c>, <c>hẻm</c> and <c>ngách</c> are still refused, so the forms that
    /// actually carry a deliverable address do not get through. Owner decision, 2026-09-09
    /// (<c>W-0243</c>); <see cref="IsSafeText"/> itself is unchanged and every other field keeps
    /// it.
    /// </para>
    /// </summary>
    private static readonly Regex RestrictedProductTextPattern = new(
        string.Join(
            '|',
            PhoneBranch,
            DialTokenBranch,
            AsciiPlaceMarkerBranch,
            DiacriticPlaceMarkerBranch),
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        MatchBudget);

    /// <summary>
    /// Use only for a field whose declared content is a product name or a unit label. A timeout
    /// counts as unsafe, for the same reason as in <see cref="IsSafeText"/>.
    /// </summary>
    public static bool IsSafeProductText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        try
        {
            return !RestrictedProductTextPattern.IsMatch(value);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public static void EnsureSafeProductText(string? value)
    {
        if (!IsSafeProductText(value))
        {
            throw new InvalidOperationException("A restricted PII value was rejected.");
        }
    }

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

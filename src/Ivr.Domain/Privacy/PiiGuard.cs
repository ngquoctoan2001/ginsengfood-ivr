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

    private static readonly Regex RestrictedValuePattern = new(
        @"(?<![0-9A-Za-z])(?:0[0-9]{9}|(?:84|\+84)[0-9]{9}|0[0-9]{2}[\s.-][0-9]{3}[\s.-][0-9]{4}|(?:84|\+84)[\s.-]*\(?[0-9]{2}\)?[\s.-][0-9]{3}[\s.-][0-9]{4})(?![0-9A-Za-z])|"
        + "(?:dial[_-]?token)[\\\"'`: ]+[A-Za-z0-9._-]{8,}|"
        + @"(?<![\p{L}\p{N}])(?:duong|so nha|ngo|hem|ngach|thon|ap)\s+[A-Za-z0-9]|"
        + @"(?<![\p{L}\p{N}])(?:đường|số nhà|ngõ|hẻm|ngách|thôn|ấp|tổ)\s+",
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
}

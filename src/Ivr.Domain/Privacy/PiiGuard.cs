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

    private static readonly Regex RestrictedValuePattern = new(
        @"(?<![0-9])(?:0|84|\+84)[0-9]{9}(?![0-9])|"
        + "(?:dial[_-]?token)[\\\"'`: ]+[A-Za-z0-9._-]{8,}|"
        + @"(?:duong|so nha|ngo|hem|ngach|thon|ap)\s+[A-Za-z0-9]|"
        + @"(?:đường|số nhà|ngõ|hẻm|ngách|thôn|ấp|tổ)\s+",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));

    public static bool IsSafeText(string? value) =>
        string.IsNullOrEmpty(value) || !RestrictedValuePattern.IsMatch(value);

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

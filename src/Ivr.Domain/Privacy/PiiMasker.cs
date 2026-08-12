namespace Ivr.Domain.Privacy;

public static class PiiMasker
{
    public static string Mask(string phone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);

        string digits = string.Concat(phone.Where(char.IsAsciiDigit));
        if (digits.Length < 6)
        {
            return "[MASKED]";
        }

        return string.Concat(digits.AsSpan(0, 2), "****", digits.AsSpan(digits.Length - 4));
    }
}

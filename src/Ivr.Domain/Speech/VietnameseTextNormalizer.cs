using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ivr.Domain.Speech;

/// <summary>
/// Diacritic and whitespace normalization used for <em>matching</em> Vietnamese place names.
/// <para>
/// <c>ShortDeliveryArea</c> keeps its own private copy of diacritic folding on purpose. That
/// one is a <em>privacy guard</em> — it decides whether a string leaks a full street address.
/// This one is a <em>lookup helper</em>. Merging them would let a change made to widen place
/// name matching silently loosen the address guard, so the small duplication is the cheaper
/// risk. <c>UT-VOICE-REGION-09</c> pins the two against a shared corpus so they cannot drift
/// apart unnoticed.
/// </para>
/// </summary>
public static class VietnameseTextNormalizer
{
    private static readonly Regex NonMatchingCharacters = new(
        "[^a-z0-9]+",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Folds Vietnamese diacritics to ASCII, mapping đ/Đ to d/D which Unicode decomposition
    /// does not do on its own.
    /// </summary>
    public static string RemoveDiacritics(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        string decomposed = value.Normalize(NormalizationForm.FormD);
        StringBuilder result = new(decomposed.Length);
        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                result.Append(character == 'đ' ? 'd' : character == 'Đ' ? 'D' : character);
            }
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Produces the canonical single-spaced, lowercase, diacritic-free form used as a lookup
    /// key. Punctuation becomes a space so that "Bà Rịa - Vũng Tàu", "Ba Ria-Vung Tau" and
    /// "TP.HCM" all collapse to a comparable shape.
    /// </summary>
    public static string ToMatchKey(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        string folded = RemoveDiacritics(value).ToLowerInvariant();
        return NonMatchingCharacters.Replace(folded, " ").Trim();
    }
}

using System.Collections.Frozen;
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
    /// Every precomposed Vietnamese letter, paired position-for-position with its ASCII fold.
    /// <para>
    /// This replaced <c>Normalize(NormalizationForm.FormD)</c> + non-spacing-mark filtering on
    /// 2026-08-26. That approach is correct on a developer machine and a <b>silent no-op inside
    /// the deployed container</b>: the runtime base image is <c>aspnet:10.0-noble-chiseled</c>,
    /// which ships without ICU and reports
    /// <c>DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true</c>, and in invariant mode
    /// <c>Normalize</c> does not decompose. The fold then returned its input unchanged, which
    /// made <c>ShortDeliveryArea</c> reject every delivery area containing "thành phố" — the six
    /// centrally governed cities, the densest order areas there are — as if they were street
    /// addresses. No test caught it because tests run where ICU exists.
    /// </para>
    /// <para>
    /// An explicit table cannot degrade quietly. It also cannot depend on
    /// <c>char.ToUpperInvariant</c> for the uppercase half, which is why both cases are spelled
    /// out: invariant-mode casing rules for non-ASCII are exactly the kind of environment
    /// dependency being removed here.
    /// </para>
    /// </summary>
    private const string AccentedLetters =
        "àáảãạăằắẳẵặâầấẩẫậ" + "èéẻẽẹêềếểễệ" + "ìíỉĩị"
        + "òóỏõọôồốổỗộơờớởỡợ" + "ùúủũụưừứửữự" + "ỳýỷỹỵ" + "đ"
        + "ÀÁẢÃẠĂẰẮẲẴẶÂẦẤẨẪẬ" + "ÈÉẺẼẸÊỀẾỂỄỆ" + "ÌÍỈĨỊ"
        + "ÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢ" + "ÙÚỦŨỤƯỪỨỬỮỰ" + "ỲÝỶỸỴ" + "Đ";

    private const string FoldedLetters =
        "aaaaaaaaaaaaaaaaa" + "eeeeeeeeeee" + "iiiii"
        + "ooooooooooooooooo" + "uuuuuuuuuuu" + "yyyyy" + "d"
        + "AAAAAAAAAAAAAAAAA" + "EEEEEEEEEEE" + "IIIII"
        + "OOOOOOOOOOOOOOOOO" + "UUUUUUUUUUU" + "YYYYY" + "D";

    private static readonly FrozenDictionary<char, char> DiacriticFolds = BuildFolds();

    private static FrozenDictionary<char, char> BuildFolds()
    {
        // A typo that shortens one side would silently mis-fold every letter after it, so the
        // pairing is checked rather than trusted.
        if (AccentedLetters.Length != FoldedLetters.Length)
        {
            throw new InvalidOperationException(
                "The Vietnamese fold table is misaligned: the accented and folded strings differ in length.");
        }

        Dictionary<char, char> folds = new(AccentedLetters.Length);
        for (int index = 0; index < AccentedLetters.Length; index++)
        {
            folds[AccentedLetters[index]] = FoldedLetters[index];
        }

        return folds.ToFrozenDictionary();
    }

    /// <summary>
    /// Folds Vietnamese diacritics to ASCII, mapping đ/Đ to d/D which Unicode decomposition
    /// does not do on its own. Uses an explicit table, so the result does not change when the
    /// host has no ICU — see <see cref="AccentedLetters"/>.
    /// </summary>
    public static string RemoveDiacritics(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        StringBuilder result = new(value.Length);
        foreach (char character in value)
        {
            result.Append(DiacriticFolds.TryGetValue(character, out char folded)
                ? folded
                : character);
        }

        return result.ToString();
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

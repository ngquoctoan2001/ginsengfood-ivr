using System.Text;
using Ivr.Domain.Privacy;

namespace Ivr.Domain.Speech;

/// <summary>
/// Q-12 (PA1, owner 2026-09-26). The last privacy check on what a customer will hear, taken
/// segment by segment so that each piece is held to the guard its content was admitted under.
/// <para>
/// Intake admits a product name with <see cref="PiiGuard.IsSafeProductText"/> (W-0243), which
/// leaves out đường, tổ, thôn and ấp because in a product name they are sugar, a nest and ordinary
/// words. The dial path then ran <see cref="PiiGuard.IsSafeText"/> over the whole rendered text, so
/// "Tổ yến" was accepted at the door and refused at every dial: two technical exceptions and then
/// <c>WINDOW_EXPIRED</c>, while Module 3 had been told the task was accepted.
/// </para>
/// <para>
/// The <c>items_spoken</c> segment is now held to the product guard, as at intake, and everything
/// else to the full guard as before. The full guard reads the text with each item segment replaced
/// by <see cref="ItemMask"/>, so fixed prose, the customer's name, the delivery area and every
/// boundary between them are read exactly as they were. The whole text is read with the product
/// guard as well, so a phone number, a dial token, <c>số nhà</c>, <c>ngõ</c>, <c>hẻm</c> or
/// <c>ngách</c> is still refused anywhere, including one split across a segment boundary. A script
/// without a placeholder split is a single segment with no placeholder, so all of it keeps the full
/// guard.
/// </para>
/// </summary>
public static class SpokenTextGuard
{
    /// <summary>The template placeholder whose text is quantities, units and product names.</summary>
    public const string ItemsPlaceholder = "items_spoken";

    /// <summary>
    /// Stands in for an item segment while the full guard reads the rest. A letter, like the
    /// spelled quantity every item segment starts with, so the guard's edges ("not after a letter
    /// or digit", "followed by one") see a word where the items were.
    /// </summary>
    private const string ItemMask = "x";

    public static bool IsSafe(IEnumerable<SpeechSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);
        StringBuilder whole = new();
        StringBuilder rest = new();
        foreach (SpeechSegment segment in segments)
        {
            whole.Append(segment.Text);
            rest.Append(IsItems(segment) ? ItemMask : segment.Text);
        }

        return PiiGuard.IsSafeProductText(whole.ToString()) && PiiGuard.IsSafeText(rest.ToString());
    }

    /// <summary>
    /// Throws what <see cref="PiiGuard.EnsureSafeText"/> throws, so every caller that classified
    /// that refusal classifies this one the same way.
    /// </summary>
    public static void EnsureSafe(IEnumerable<SpeechSegment> segments)
    {
        if (!IsSafe(segments))
        {
            throw new InvalidOperationException("A restricted PII value was rejected.");
        }
    }

    private static bool IsItems(SpeechSegment segment) =>
        segment.Kind == SpeechSegmentKind.Dynamic
        && string.Equals(segment.PlaceholderName, ItemsPlaceholder, StringComparison.Ordinal);
}

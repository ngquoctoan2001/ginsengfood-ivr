using System.Collections.Immutable;
using System.Text;

namespace Ivr.Domain.Speech;

/// <summary>
/// Regional lexicon for spoken Vietnamese numbers.
/// <para>
/// Owner decision <c>OD-VOICE-03</c> (2026-08-22) keeps a single approved script template. That
/// only works because "nghìn"/"ngàn" and "linh"/"lẻ" are a property of <em>how a number is
/// read</em>, not of <em>what the script says</em>. Putting the variants here leaves
/// <c>TemplateText</c> and therefore <c>TemplateHash</c> untouched — no migration, no second
/// approval — while a Southern voice still says "ngàn" instead of sounding imported.
/// </para>
/// </summary>
public sealed record VietnameseNumberStyle
{
    private VietnameseNumberStyle(string thousandWord, string zeroTensWord)
    {
        ThousandWord = thousandWord;
        ZeroTensWord = zeroTensWord;
    }

    /// <summary>"nghìn" in the North, "ngàn" from roughly Quảng Trị southward.</summary>
    public string ThousandWord { get; }

    /// <summary>The filler in "một trăm <em>linh</em> năm" — "lẻ" in Central and Southern speech.</summary>
    public string ZeroTensWord { get; }

    public static VietnameseNumberStyle Northern { get; } = new("nghìn", "linh");

    /// <summary>
    /// Central defaults to the Southern lexicon because the approved Central voice is a Đà Nẵng
    /// accent (§4.4). Bắc Trung Bộ — Thanh Hóa, Nghệ An, Hà Tĩnh — leans "nghìn", so this is the
    /// one genuinely arguable entry in this file and is flagged for owner confirmation.
    /// </summary>
    public static VietnameseNumberStyle CentralDefault { get; } = new("ngàn", "lẻ");

    public static VietnameseNumberStyle Southern { get; } = new("ngàn", "lẻ");

    public static VietnameseNumberStyle ForRegion(VietnamRegion region) => region switch
    {
        VietnamRegion.North => Northern,
        VietnamRegion.Central => CentralDefault,
        VietnamRegion.South => Southern,
        _ => throw new ArgumentOutOfRangeException(nameof(region)),
    };

    public static VietnameseNumberStyle Create(string thousandWord, string zeroTensWord)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(thousandWord);
        ArgumentException.ThrowIfNullOrWhiteSpace(zeroTensWord);
        return new VietnameseNumberStyle(thousandWord.Trim(), zeroTensWord.Trim());
    }
}

/// <summary>
/// Converts an integral amount into spoken Vietnamese words.
/// <para>
/// This exists because the renderer used to hand the synthesizer <c>"560.000 đồng"</c> while the
/// audio the owner actually approved says "năm trăm sáu mươi nghìn đồng" — the approved sample
/// had been typed by hand, so the digits path had never been heard by anyone. How a synthesizer
/// reads "560.000" is engine-specific and not something a confirmation call can leave to chance:
/// the customer is being asked to approve that number by pressing a key.
/// </para>
/// <para>
/// No <c>CultureInfo</c> lookup, matching the deliberate ICU-free choice in
/// <c>VietnameseOrderScriptRenderer</c>: the worker image runs in globalization-invariant mode,
/// and a customer should hear the same amount on every machine.
/// </para>
/// </summary>
public static class VietnameseNumberSpeller
{
    /// <summary>
    /// Highest amount that can be spoken. Above 999 tỷ Vietnamese needs compound scales
    /// ("nghìn tỷ"); an order total never reaches it, so the bound is an explicit failure rather
    /// than a silently wrong reading.
    /// </summary>
    public const decimal MaximumAmount = 999_999_999_999m;

    private static readonly ImmutableArray<string> Digits =
    [
        "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín",
    ];

    public static string Spell(decimal amount, VietnameseNumberStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(amount, MaximumAmount);
        if (amount != decimal.Truncate(amount))
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Spoken amounts must be integral; VND has no spoken subunit.");
        }

        long value = (long)amount;
        if (value == 0)
        {
            return Digits[0];
        }

        // Scale words are indexed from the least significant group.
        string[] scales = ["", style.ThousandWord, "triệu", "tỷ"];
        int[] groups = new int[4];
        int groupCount = 0;
        for (long remaining = value; remaining > 0; remaining /= 1_000)
        {
            groups[groupCount++] = (int)(remaining % 1_000);
        }

        StringBuilder spoken = new();
        for (int index = groupCount - 1; index >= 0; index--)
        {
            if (groups[index] == 0)
            {
                continue;
            }

            if (spoken.Length > 0)
            {
                spoken.Append(' ');
            }

            // Only the leading group may drop a zero hundreds place. Lower groups keep
            // "không trăm" so 1.005.000 reads "một triệu không trăm linh năm nghìn" rather than
            // collapsing into "một triệu năm nghìn".
            spoken.Append(SpellGroup(groups[index], index < groupCount - 1, style));
            if (scales[index].Length > 0)
            {
                spoken.Append(' ').Append(scales[index]);
            }
        }

        return spoken.ToString();
    }

    public static string Spell(decimal amount, VietnamRegion region) =>
        Spell(amount, VietnameseNumberStyle.ForRegion(region));

    private static string SpellGroup(int group, bool padHundreds, VietnameseNumberStyle style)
    {
        int hundreds = group / 100;
        int tens = group / 10 % 10;
        int units = group % 10;
        StringBuilder spoken = new();

        if (hundreds > 0)
        {
            spoken.Append(Digits[hundreds]).Append(" trăm");
        }
        else if (padHundreds)
        {
            spoken.Append("không trăm");
        }

        if (tens == 0 && units == 0)
        {
            return spoken.ToString();
        }

        if (tens == 0)
        {
            // "một trăm linh năm". With no hundreds spoken there is nothing for the filler to
            // sit between, so a bare group reads as just "năm".
            if (spoken.Length > 0)
            {
                spoken.Append(' ').Append(style.ZeroTensWord).Append(' ');
            }

            return spoken.Append(Digits[units]).ToString();
        }

        if (spoken.Length > 0)
        {
            spoken.Append(' ');
        }

        if (tens == 1)
        {
            spoken.Append("mười");
        }
        else
        {
            spoken.Append(Digits[tens]).Append(" mươi");
        }

        if (units == 0)
        {
            return spoken.ToString();
        }

        // "mười lăm" not "mười năm"; "hai mươi mốt" not "hai mươi một"; "hai mươi tư" not
        // "hai mươi bốn" — but "mười bốn" keeps bốn.
        string spokenUnit = units switch
        {
            1 when tens >= 2 => "mốt",
            4 when tens >= 2 => "tư",
            5 => "lăm",
            _ => Digits[units],
        };

        return spoken.Append(' ').Append(spokenUnit).ToString();
    }
}

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
/// One clip in the recorded number bank: a stable id and the words it says.
/// <para>
/// The id is region-independent and the text is not. That falls out of the same fact
/// <c>VietnameseNumberStyle</c> is built on — only "nghìn"/"ngàn" and "linh"/"lẻ" differ — and it
/// is what lets a single written script be read by three voices: every clip from <c>num-00</c> to
/// <c>num-99</c> carries identical text in all three styles, so the booth reads one list.
/// </para>
/// </summary>
public readonly record struct SpeechNumberClip(string Id, string Text)
{
    /// <summary>
    /// The words a clip run says, single-spaced. Every caller that turns clips back into text uses
    /// this one: a second joiner somewhere else is how the played call and the approved script
    /// would start describing different things.
    /// </summary>
    public static string Join(ImmutableArray<SpeechNumberClip> clips)
    {
        StringBuilder spoken = new();
        foreach (SpeechNumberClip clip in clips)
        {
            if (spoken.Length > 0)
            {
                spoken.Append(' ');
            }

            spoken.Append(clip.Text);
        }

        return spoken.ToString();
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

    /// <summary>
    /// `0..99` read as whole units — owner decision `R-2`, 2026-09-08. Recording each pair as one
    /// continuous read is what removes the join inside "sáu mươi", which is the worst place to cut
    /// a tonal language; the joins that remain fall on trăm/nghìn/đồng, where a reader breathes
    /// anyway. See plan/ivr-orther/m8-16.
    /// </summary>
    private static readonly ImmutableArray<SpeechNumberClip> PairClips = BuildPairClips();

    private static readonly SpeechNumberClip HundredClip = new("num-hundred", "trăm");
    private static readonly SpeechNumberClip MillionClip = new("num-million", "triệu");
    private static readonly SpeechNumberClip BillionClip = new("num-billion", "tỷ");
    private static readonly SpeechNumberClip PointClip = new("num-point", "phẩy");

    private static SpeechNumberClip ThousandClip(VietnameseNumberStyle style) =>
        new("num-thousand", style.ThousandWord);

    private static SpeechNumberClip ZeroTensClip(VietnameseNumberStyle style) =>
        new("num-zero-tens", style.ZeroTensWord);

    private static ImmutableArray<SpeechNumberClip> BuildPairClips()
    {
        ImmutableArray<SpeechNumberClip>.Builder clips = ImmutableArray.CreateBuilder<SpeechNumberClip>(100);
        for (int value = 0; value < 100; value++)
        {
            clips.Add(new SpeechNumberClip($"num-{value:00}", PairText(value)));
        }

        return clips.MoveToImmutable();
    }

    private static string PairText(int value)
    {
        int tens = value / 10;
        int units = value % 10;
        if (tens == 0)
        {
            return Digits[units];
        }

        StringBuilder text = new();
        text.Append(tens == 1 ? "mười" : $"{Digits[tens]} mươi");
        if (units == 0)
        {
            return text.ToString();
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

        return text.Append(' ').Append(spokenUnit).ToString();
    }


    /// <summary>
    /// The recorded clips an amount is read from, in playback order.
    /// <para>
    /// This is the primitive and <see cref="Spell(decimal, VietnameseNumberStyle)"/> is a
    /// projection of it, deliberately that way round. Going the other way — render the words, then
    /// cut the words back into clips — needs an inverse of this walk, because a clip boundary is
    /// not visible in the text: "hai mươi mốt" is one clip inside 21, sits inside three clips of
    /// 121 and five of 1021. That inverse would have to track the mốt/tư/lăm rules, the mười/mươi
    /// split and the "không trăm" filler, and stay in step with this method forever. One rule
    /// written once cannot drift from itself; two copies of it did exactly that in W-0221.
    /// </para>
    /// </summary>
    public static ImmutableArray<SpeechNumberClip> SpellClips(
        decimal amount,
        VietnameseNumberStyle style)
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

        ImmutableArray<SpeechNumberClip>.Builder clips = ImmutableArray.CreateBuilder<SpeechNumberClip>();
        long value = (long)amount;
        if (value == 0)
        {
            clips.Add(PairClips[0]);
            return clips.ToImmutable();
        }

        int[] groups = new int[4];
        int groupCount = 0;
        for (long remaining = value; remaining > 0; remaining /= 1_000)
        {
            groups[groupCount++] = (int)(remaining % 1_000);
        }

        for (int index = groupCount - 1; index >= 0; index--)
        {
            if (groups[index] == 0)
            {
                continue;
            }

            // Only the leading group may drop a zero hundreds place. Lower groups keep
            // "không trăm" so 1.005.000 reads "một triệu không trăm linh năm nghìn" rather than
            // collapsing into "một triệu năm nghìn".
            AppendGroupClips(clips, groups[index], index < groupCount - 1, style);
            switch (index)
            {
                case 1: clips.Add(ThousandClip(style)); break;
                case 2: clips.Add(MillionClip); break;
                case 3: clips.Add(BillionClip); break;
                default: break;
            }
        }

        return clips.ToImmutable();
    }

    public static ImmutableArray<SpeechNumberClip> SpellClips(decimal amount, VietnamRegion region) =>
        SpellClips(amount, VietnameseNumberStyle.ForRegion(region));

    public static string Spell(decimal amount, VietnameseNumberStyle style) =>
        SpeechNumberClip.Join(SpellClips(amount, style));

    public static string Spell(decimal amount, VietnamRegion region) =>
        Spell(amount, VietnameseNumberStyle.ForRegion(region));

    /// <summary>
    /// Highest number of decimal places a spoken quantity may carry. Three covers weight-based
    /// units; beyond that the reading gets long enough that a customer stops tracking it, and a
    /// quantity that precise is more likely a data error than an order.
    /// </summary>
    public const int MaximumQuantityDecimals = 3;

    /// <summary>
    /// Spells a quantity, including a fractional one.
    /// <para>
    /// Amounts of money are integral by definition — VND has no spoken subunit — but a quantity
    /// is not: <c>2,5 kg</c> is an ordinary line on an order. Before this, the renderer emitted
    /// the digit form <c>"2,5"</c> for those and left the reading to the engine. That was the
    /// last place in the script where the customer's confirmation depended on a synthesizer's
    /// guess, and it is the one thing concatenated audio cannot do at all: there is no recorded
    /// clip for "2,5" and no way to glue one from clips of "2" and "5".
    /// </para>
    /// <para>
    /// The fractional part is read digit by digit — <c>0,25</c> is "không phẩy hai năm", not
    /// "không phẩy hai mươi lăm". Digit-by-digit is the unambiguous reading: grouping invites
    /// hearing a different number, and this is a number the customer is about to approve.
    /// </para>
    /// </summary>
    public static ImmutableArray<SpeechNumberClip> SpellQuantityClips(
        decimal quantity,
        VietnameseNumberStyle style)
    {
        ArgumentNullException.ThrowIfNull(style);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, MaximumAmount);
        decimal whole = decimal.Truncate(quantity);
        if (quantity == whole)
        {
            return SpellClips(whole, style);
        }

        // Trailing zeros carry no meaning in a spoken quantity: 2,50 and 2,5 are the same order,
        // and reading "hai phẩy năm không" invites hearing 2,50 as a different number.
        string fractionDigits = (quantity - whole)
            .ToString("0.#########", System.Globalization.CultureInfo.InvariantCulture)
            .Split('.')[1]
            .TrimEnd('0');
        if (fractionDigits.Length > MaximumQuantityDecimals)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "A spoken quantity carries at most three decimal places.");
        }

        ImmutableArray<SpeechNumberClip>.Builder clips = ImmutableArray.CreateBuilder<SpeechNumberClip>();
        clips.AddRange(SpellClips(whole, style));
        clips.Add(PointClip);
        foreach (char digit in fractionDigits)
        {
            clips.Add(PairClips[digit - '0']);
        }

        return clips.ToImmutable();
    }

    public static ImmutableArray<SpeechNumberClip> SpellQuantityClips(
        decimal quantity,
        VietnamRegion region) =>
        SpellQuantityClips(quantity, VietnameseNumberStyle.ForRegion(region));

    public static string SpellQuantity(decimal quantity, VietnameseNumberStyle style) =>
        SpeechNumberClip.Join(SpellQuantityClips(quantity, style));

    public static string SpellQuantity(decimal quantity, VietnamRegion region) =>
        SpellQuantity(quantity, VietnameseNumberStyle.ForRegion(region));

    private static void AppendGroupClips(
        ImmutableArray<SpeechNumberClip>.Builder clips,
        int group,
        bool padHundreds,
        VietnameseNumberStyle style)
    {
        int hundreds = group / 100;
        int tens = group / 10 % 10;
        int units = group % 10;
        bool spokeHundreds = hundreds > 0 || padHundreds;

        if (spokeHundreds)
        {
            clips.Add(PairClips[hundreds]);
            clips.Add(HundredClip);
        }

        if (tens == 0 && units == 0)
        {
            return;
        }

        if (tens == 0)
        {
            // "một trăm linh năm". With no hundreds spoken there is nothing for the filler to
            // sit between, so a bare group reads as just "năm".
            if (spokeHundreds)
            {
                clips.Add(ZeroTensClip(style));
            }

            clips.Add(PairClips[units]);
            return;
        }

        // The whole tens-and-units pair is one clip. That is what R-2 buys: the reader says
        // "sáu mươi" in one breath instead of the pipeline gluing "sáu" to "mươi".
        clips.Add(PairClips[(tens * 10) + units]);
    }
}

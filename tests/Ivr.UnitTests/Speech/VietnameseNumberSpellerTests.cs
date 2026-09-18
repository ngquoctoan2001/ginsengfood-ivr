using Ivr.Domain.Speech;

namespace Ivr.UnitTests.Speech;

public sealed class VietnameseNumberSpellerTests
{
    [Theory]
    [Trait("TestId", "UT-VOICE-NUM-01")]
    [InlineData(0, "không")]
    [InlineData(1, "một")]
    [InlineData(5, "năm")]
    [InlineData(9, "chín")]
    [InlineData(10, "mười")]
    [InlineData(11, "mười một")]
    [InlineData(14, "mười bốn")]
    [InlineData(15, "mười lăm")]
    [InlineData(20, "hai mươi")]
    [InlineData(21, "hai mươi mốt")]
    [InlineData(24, "hai mươi tư")]
    [InlineData(25, "hai mươi lăm")]
    [InlineData(99, "chín mươi chín")]
    public void UnitsAndTensFollowSpokenVietnameseNotDigitReading(long amount, string expected) =>
        // "mười lăm" not "mười năm"; "hai mươi mốt" not "hai mươi một"; but "mười bốn" keeps bốn
        // while "hai mươi tư" does not. These are the cases a naive digit reader gets wrong.
        Assert.Equal(expected, VietnameseNumberSpeller.Spell(amount, VietnameseNumberStyle.Northern));

    [Theory]
    [Trait("TestId", "UT-VOICE-NUM-02")]
    [InlineData(100, "một trăm")]
    [InlineData(101, "một trăm linh một")]
    [InlineData(105, "một trăm linh năm")]
    [InlineData(110, "một trăm mười")]
    [InlineData(115, "một trăm mười lăm")]
    [InlineData(120, "một trăm hai mươi")]
    [InlineData(121, "một trăm hai mươi mốt")]
    [InlineData(999, "chín trăm chín mươi chín")]
    public void HundredsInsertTheZeroTensFiller(long amount, string expected) =>
        Assert.Equal(expected, VietnameseNumberSpeller.Spell(amount, VietnameseNumberStyle.Northern));

    [Theory]
    [Trait("TestId", "UT-VOICE-NUM-03")]
    [InlineData(1_000, "một nghìn")]
    [InlineData(560_000, "năm trăm sáu mươi nghìn")]
    [InlineData(1_000_000, "một triệu")]
    [InlineData(1_000_500, "một triệu năm trăm")]
    [InlineData(1_005_000, "một triệu không trăm linh năm nghìn")]
    [InlineData(2_050_000, "hai triệu không trăm năm mươi nghìn")]
    [InlineData(1_000_000_000, "một tỷ")]
    [InlineData(999_999_999_999, "chín trăm chín mươi chín tỷ chín trăm chín mươi chín triệu chín trăm chín mươi chín nghìn chín trăm chín mươi chín")]
    public void ScaleGroupsPadZeroHundredsExceptTheLeadingGroup(long amount, string expected) =>
        // 1.005.000 must not collapse to "một triệu năm nghìn" — the padded "không trăm" is what
        // keeps the magnitude audible.
        Assert.Equal(expected, VietnameseNumberSpeller.Spell(amount, VietnameseNumberStyle.Northern));

    [Fact]
    [Trait("TestId", "UT-VOICE-NUM-04")]
    public void ApprovedW0104AmountMatchesTheAudioTheOwnerAccepted() =>
        // docs/evidence/W-0104: the accepted v3 sample says "năm trăm sáu mươi nghìn đồng" while
        // the renderer was emitting "560.000 đồng". This pins the two together.
        Assert.Equal(
            "năm trăm sáu mươi nghìn",
            VietnameseNumberSpeller.Spell(560_000m, VietnamRegion.North));

    [Theory]
    [Trait("TestId", "UT-VOICE-NUM-05")]
    [InlineData(VietnamRegion.North, 560_000, "năm trăm sáu mươi nghìn")]
    [InlineData(VietnamRegion.Central, 560_000, "năm trăm sáu mươi ngàn")]
    [InlineData(VietnamRegion.South, 560_000, "năm trăm sáu mươi ngàn")]
    [InlineData(VietnamRegion.North, 105, "một trăm linh năm")]
    [InlineData(VietnamRegion.South, 105, "một trăm lẻ năm")]
    public void RegionalLexiconChangesTheWordsWithoutTouchingTheTemplate(
        VietnamRegion region,
        long amount,
        string expected) =>
        // OD-VOICE-03 keeps one approved template. "nghìn"/"ngàn" and "linh"/"lẻ" live here
        // instead, so TemplateHash never changes and no migration or re-approval is needed.
        Assert.Equal(expected, VietnameseNumberSpeller.Spell(amount, region));

    [Fact]
    [Trait("TestId", "UT-VOICE-NUM-06")]
    public void EveryRegionHasAStyleAndTheNorthIsTheOnlyOneSayingNghin()
    {
        foreach (VietnamRegion region in Enum.GetValues<VietnamRegion>())
        {
            VietnameseNumberStyle style = VietnameseNumberStyle.ForRegion(region);

            Assert.False(string.IsNullOrWhiteSpace(style.ThousandWord));
            Assert.False(string.IsNullOrWhiteSpace(style.ZeroTensWord));
        }

        Assert.Equal("nghìn", VietnameseNumberStyle.ForRegion(VietnamRegion.North).ThousandWord);
        Assert.Equal("ngàn", VietnameseNumberStyle.ForRegion(VietnamRegion.South).ThousandWord);
        Assert.Equal("linh", VietnameseNumberStyle.ForRegion(VietnamRegion.North).ZeroTensWord);
        Assert.Equal("lẻ", VietnameseNumberStyle.ForRegion(VietnamRegion.South).ZeroTensWord);
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-NUM-07")]
    public void OutOfRangeAmountsFailLoudlyRatherThanReadingWrong()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => VietnameseNumberSpeller.Spell(-1m, VietnamRegion.North));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => VietnameseNumberSpeller.Spell(
                VietnameseNumberSpeller.MaximumAmount + 1m,
                VietnamRegion.North));

        // A fractional amount has no spoken VND form; truncating it silently would change the
        // number the customer is being asked to confirm.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => VietnameseNumberSpeller.Spell(560_000.5m, VietnamRegion.North));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-NUM-08")]
    public void SpellingIsDeterministicAndFreeOfDigitsAndSeparators()
    {
        // The whole point is that no engine-specific digit reading survives into the audio.
        for (long amount = 0; amount <= 2_000; amount++)
        {
            string spoken = VietnameseNumberSpeller.Spell(amount, VietnamRegion.North);

            Assert.DoesNotContain(spoken, char.IsDigit);
            Assert.DoesNotContain('.', spoken);
            Assert.DoesNotContain(',', spoken);
            Assert.Equal(spoken, VietnameseNumberSpeller.Spell(amount, VietnamRegion.North));
            Assert.Equal(spoken.Trim(), spoken);
            Assert.DoesNotContain("  ", spoken, StringComparison.Ordinal);
        }

        Assert.Equal(
            560_000m.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Length,
            "560,000".Length);
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-NUM-09")]
    public void CustomStyleIsAcceptedSoTheCentralLexiconCanBeChangedWithoutCode() =>
        // The Central entry is the one genuinely arguable default (Bắc Trung Bộ leans "nghìn"),
        // so it must be overridable by configuration rather than by an edit.
        Assert.Equal(
            "năm trăm sáu mươi nghìn",
            VietnameseNumberSpeller.Spell(560_000m, VietnameseNumberStyle.Create("nghìn", "lẻ")));

    /// <summary>
    /// Fractional quantities, spoken (A7).
    /// <para>
    /// The fractional part is read digit by digit: 0,25 is "không phẩy hai năm", not "không phẩy
    /// hai mươi lăm". Grouping invites hearing a different number, and this is the number the
    /// customer is about to approve with a keypress.
    /// </para>
    /// </summary>
    [Theory]
    [Trait("TestId", "UT-VOICE-NUM-DEC-10")]
    [InlineData(2, "hai")]
    [InlineData(2.5, "hai phẩy năm")]
    [InlineData(12.5, "mười hai phẩy năm")]
    [InlineData(0.25, "không phẩy hai năm")]
    [InlineData(1.05, "một phẩy không năm")]
    [InlineData(3.125, "ba phẩy một hai năm")]
    public void FractionalQuantitiesAreSpokenDigitByDigitAfterThePoint(
        decimal quantity,
        string expected) =>
        Assert.Equal(expected, VietnameseNumberSpeller.SpellQuantity(quantity, VietnamRegion.North));

    [Fact]
    [Trait("TestId", "UT-VOICE-NUM-DEC-11")]
    public void SpokenQuantitiesDropTrailingZerosAndRefuseExcessPrecision()
    {
        // 2,50 and 2,5 are the same order. Reading "hai phẩy năm không" invites hearing 2,50 as
        // a different quantity.
        Assert.Equal(
            "hai phẩy năm",
            VietnameseNumberSpeller.SpellQuantity(2.50m, VietnamRegion.North));
        Assert.Equal(
            VietnameseNumberSpeller.SpellQuantity(2.5m, VietnamRegion.South),
            VietnameseNumberSpeller.SpellQuantity(2.500m, VietnamRegion.South));

        // Four decimal places reads as a sentence and is far more likely a data error than an
        // order, so it is refused rather than spoken.
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VietnameseNumberSpeller.SpellQuantity(1.2345m, VietnamRegion.North));

        // Money stays integral: VND has no spoken subunit, and Spell must keep saying so.
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VietnameseNumberSpeller.Spell(560_000.5m, VietnamRegion.North));

        // Region still selects the lexicon for the whole part.
        Assert.Equal(
            "một ngàn phẩy năm",
            VietnameseNumberSpeller.SpellQuantity(1_000.5m, VietnamRegion.South));
    }
}

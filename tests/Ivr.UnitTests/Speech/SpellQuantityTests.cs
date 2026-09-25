using System.Globalization;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Scripts;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

/// <summary>
/// W-0359 / K-26: the quantity half of B16 (chief worklist 2026-09-25).
/// <para>
/// <see cref="VietnameseNumberSpeller.SpellQuantity(decimal, VietnameseNumberStyle)"/> used to cut
/// the digits after the point out of <c>ToString("0.#########")</c>. That format rounds at nine
/// places, so a quantity with ten came back with no point at all and <c>Split('.')[1]</c> threw
/// <see cref="IndexOutOfRangeException"/>. The render path names only argument and policy faults
/// as the order's, so that type reached the dispatch gateways' generic arm, which reports the
/// channel unhealthy: one mistyped quantity quarantined a SIM.
/// </para>
/// <para>
/// The renderer's fallback for a quantity the speller refuses had the opposite fault. It
/// formatted with <c>"0.##"</c>, so the customer heard 0,0005 as "0" and 2,99999 as "3" in the
/// call that asks them to approve that number.
/// </para>
/// </summary>
public sealed class SpellQuantityTests
{
    private const string TooPrecise = "A spoken quantity carries at most three decimal places.";

    [Fact]
    [Trait("TestId", "UT-SPELL-QTY-01")]
    public void TenDecimalPlacesAreRefusedAsTooPreciseNotAsAnIndexFault()
    {
        // 0,0000000001 rounded to "0" at nine places. Assert.Throws checks the exact type, so an
        // IndexOutOfRangeException — or anything else the render path does not name as the
        // order's fault — fails here rather than in a dispatch pass.
        ArgumentOutOfRangeException refused = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VietnameseNumberSpeller.SpellQuantity(1.0000000001m, VietnameseNumberStyle.Northern));

        // The refusal four decimal places already got, not a new one the renderer would have to
        // learn to handle.
        Assert.StartsWith(TooPrecise, refused.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-SPELL-QTY-02")]
    public void TenDecimalPlacesThatRoundUpAreRefusedTheSameWay()
    {
        // 0,9999999999 rounded to "1" at nine places: the rounding carried into the whole part,
        // and again there was no point to split on.
        ArgumentOutOfRangeException refused = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VietnameseNumberSpeller.SpellQuantity(2.9999999999m, VietnameseNumberStyle.Northern));

        Assert.StartsWith(TooPrecise, refused.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A quantity the speller refuses reaches the customer with every digit it has.
    /// <para>
    /// The third column is what the old <c>"0.##"</c> fallback read instead. 2,9999999999 never
    /// got that far — it was the IndexOutOfRangeException above — but "0.##" would have read it
    /// as "3" as well. The last row is outside the speller's range, the other branch that falls
    /// back to digits.
    /// </para>
    /// <para>
    /// 1,0000000001 is not a row, although the fallback formats it exactly too: that digit form
    /// carries the run "0000000001", ten digits starting with zero, which the full-text privacy
    /// guard reads as a phone number. That script is refused before it is spoken, with an
    /// InvalidOperationException the gateways already report with the channel healthy.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("0.0005", "0,0005", "0")]
    [InlineData("2.99999", "2,99999", "3")]
    [InlineData("2.9999999999", "2,9999999999", "3")]
    [InlineData("1000000000000.125", "1000000000000,125", "1000000000000,13")]
    [Trait("TestId", "UT-SPELL-QTY-03")]
    public async Task AQuantityTheSpellerRefusesIsReadWithEveryDigitItHas(
        string quantity,
        string exactDigits,
        string roundedReading)
    {
        using InMemoryScriptRegistry registry = new(
            new InMemoryAuditLogger(TimeProvider.System),
            TimeProvider.System,
            Options.Create(new ScriptContentOptions()));
        ApprovedScript approved = Assert.IsType<ApprovedScript>(await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock));

        ScriptPreview preview = new VietnameseOrderScriptRenderer().Render(
            approved,
            PrivacySafeOrderSummary.Create(
                "Anh Minh",
                "DH-K26",
                [SpeechItem.Create("Trà sâm", Parse(quantity), "kg")],
                Money.Vnd(560_000m),
                ShortDeliveryArea.Create("Quận 7"),
                "Golden Hour",
                null,
                SpeechSummaryLimits.Create(20, 5)));

        // Every significant digit, the Vietnamese decimal comma, and no trailing zero added.
        SpeechSegment items = Assert.Single(
            preview.Segments,
            segment => segment.PlaceholderName == "items_spoken");
        Assert.Equal($"{exactDigits} kg Trà sâm", items.Text);
        Assert.Contains($" {exactDigits} kg Trà sâm", preview.ExactText, StringComparison.Ordinal);

        // Not the rounded reading. 0,0005 read as a bare "0" is an order for nothing, and the
        // customer was being asked to press a key to approve it.
        Assert.DoesNotContain(
            $" {roundedReading} kg Trà sâm",
            preview.ExactText,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The arithmetic reads every quantity the speller accepts exactly as the string cut did.
    /// <para>
    /// Scale is what the arithmetic has to get right that the string never had to think about.
    /// A decimal keeps the scale it was written with, so 2,5000 is 2,5 carrying four places; the
    /// loop has to stop on the remainder, not on the scale, or it would refuse an ordinary
    /// quantity for a precision it does not have. The last row carries the most places a decimal
    /// can hold, where multiplying by ten no longer fits in 96 bits and the type rescales
    /// underneath; the reading must not change.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("2.5", "hai phẩy năm")]
    [InlineData("2.50", "hai phẩy năm")]
    [InlineData("2.5000", "hai phẩy năm")]
    [InlineData("0.25", "không phẩy hai năm")]
    [InlineData("1.05", "một phẩy không năm")]
    [InlineData("0.001", "không phẩy không không một")]
    [InlineData("1.125", "một phẩy một hai năm")]
    [InlineData("1.1250", "một phẩy một hai năm")]
    [InlineData("3.0000", "ba")]
    [InlineData("0.8000000000000000000000000000", "không phẩy tám")]
    [Trait("TestId", "UT-SPELL-QTY-04")]
    public void OrdinaryQuantitiesReadAsBeforeWhateverScaleTheyCarry(
        string quantity,
        string expected) =>
        Assert.Equal(
            expected,
            VietnameseNumberSpeller.SpellQuantity(Parse(quantity), VietnameseNumberStyle.Northern));

    // Quantities travel as invariant strings: an attribute cannot hold a decimal, and a double
    // would drop the scale several rows above exist to exercise.
    private static decimal Parse(string quantity) =>
        decimal.Parse(quantity, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
}

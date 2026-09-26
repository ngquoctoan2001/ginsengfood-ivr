using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Scripts;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

/// <summary>
/// Q-12 (PA1, owner 2026-09-26). Intake admits a product name with the product guard (W-0243), and
/// the dial path used to read the finished script with the full guard, so "Tổ yến", "Đường phèn"
/// and "Ấp trứng" were accepted and then refused at every dial. The last check now goes segment by
/// segment: the item names are held to the guard they were admitted under, everything else to the
/// full guard as before.
/// </summary>
public sealed class SpokenTextGuardTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 26, 3, 0, 0, TimeSpan.Zero);

    [Theory]
    [Trait("TestId", "UT-PII-PRODUCT-04")]
    [InlineData("hai hộp Tổ yến")]
    [InlineData("một gói Đường phèn")]
    [InlineData("ba hộp Ấp trứng")]
    [InlineData("hai hộp Tổ yến chưng đường phèn")]
    public void AnItemSegmentIsHeldToTheGuardItsNamesWereAdmittedUnder(string items)
    {
        Assert.True(SpokenTextGuard.IsSafe(Script(items)));

        // The same words outside the item segment are still read as an address.
        Assert.False(SpokenTextGuard.IsSafe(Script("hai hộp Cháo sâm", deliveryArea: items)));

        // And a script with no placeholder split keeps the full guard over all of it.
        Assert.False(SpokenTextGuard.IsSafe(
            [SpeechSegment.CreateDynamic(1, null, SpeechSegment.Concatenate(Script(items)))]));
    }

    [Fact]
    [Trait("TestId", "UT-PII-PRODUCT-05")]
    public void WhatTheProductGuardRefusesIsStillRefusedInsideTheItemsAndAcrossABoundary()
    {
        // Inside the item segment: an address-only marker and a telephone number.
        Assert.False(SpokenTextGuard.IsSafe(Script("hai hộp Sâm ngõ 5")));
        Assert.False(SpokenTextGuard.IsSafe(Script("hai hộp 0912345678")));

        // Split across a segment boundary, a number neither piece holds whole is still refused:
        // the product guard reads the whole text.
        SpeechSegment[] split =
        [
            SpeechSegment.CreateFixed(1, "Đơn hàng gồm "),
            SpeechSegment.CreateDynamic(2, "order_code_short", "091"),
            SpeechSegment.CreateDynamic(3, SpokenTextGuard.ItemsPlaceholder, "2345678 hộp Cháo sâm"),
            SpeechSegment.CreateFixed(4, ". Bấm phím 1 để xác nhận, bấm phím 0 để hủy."),
        ];
        Assert.False(SpokenTextGuard.IsSafe(split));

        // Outside the items, the full guard reads the segments as they were, boundaries included.
        Assert.False(SpokenTextGuard.IsSafe(Script("hai hộp Cháo sâm", deliveryArea: "đường Lê Lợi")));
        Assert.Throws<InvalidOperationException>(() => SpokenTextGuard.EnsureSafe(
            Script("hai hộp Cháo sâm", deliveryArea: "đường Lê Lợi")));
    }

    [Theory]
    [Trait("TestId", "UT-PII-PRODUCT-06")]
    [InlineData("Tổ yến")]
    [InlineData("Đường phèn")]
    [InlineData("Ấp trứng")]
    public async Task AProductNameIntakeAdmitsIsRenderedAndPassesTheLastCheck(string productName)
    {
        using InMemoryScriptRegistry scripts = CreateScripts();
        RenderedSpeech speech = await CreateRenderer(scripts).RenderAsync(
            Summary(productName),
            "SCRIPT-ORDER-CONFIRM",
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);
        Assert.Contains(productName, speech.ExactText, StringComparison.Ordinal);

        // The check the synthesis service runs just before speaking, on the split it is handed.
        SpeechScript script = SpeechScript.Create(
            "SCRIPT-ORDER-CONFIRM",
            TargetV1SpeechPolicy.MockTemplateVersion,
            speech.ExactText,
            speech.ContentHash,
            "summary-hash",
            speech.Segments);
        Assert.Null(Record.Exception(() => SpeechPrivacyGuard.EnsureSafe(script, TtsOptions.Create())));

        // The same text without the split is refused as it always was.
        SpeechScript whole = SpeechScript.Create(
            "SCRIPT-ORDER-CONFIRM",
            TargetV1SpeechPolicy.MockTemplateVersion,
            speech.ExactText,
            speech.ContentHash,
            "summary-hash");
        IvrFailureException refused = Assert.Throws<IvrFailureException>(
            () => SpeechPrivacyGuard.EnsureSafe(whole, TtsOptions.Create()));
        Assert.Equal(IvrErrorCodes.PiiPolicyViolation, refused.ErrorCode);
    }

    private static SpeechSegment[] Script(string items, string deliveryArea = "Quận 7") =>
    [
        SpeechSegment.CreateFixed(1, "Xin chào, đơn hàng gồm "),
        SpeechSegment.CreateDynamic(2, SpokenTextGuard.ItemsPlaceholder, items),
        SpeechSegment.CreateFixed(3, ", giao tới "),
        SpeechSegment.CreateDynamic(4, "delivery_area_short", deliveryArea),
        SpeechSegment.CreateFixed(5, ". Bấm phím 1 để xác nhận, bấm phím 0 để hủy."),
    ];

    private static InMemoryScriptRegistry CreateScripts()
    {
        var clock = new FixedTimeProvider(Now);
        return new InMemoryScriptRegistry(
            new InMemoryAuditLogger(clock),
            clock,
            Options.Create(new ScriptContentOptions()));
    }

    private static ApprovedVietnameseSpeechRenderer CreateRenderer(InMemoryScriptRegistry scripts)
    {
        IOptions<TtsProviderOptions> tts = Options.Create(new TtsProviderOptions
        {
            ExecutionMode = "MOCK",
            Provider = TtsProviderOptions.FakeProvider,
        });
        return new ApprovedVietnameseSpeechRenderer(
            scripts,
            new VietnameseOrderScriptRenderer(),
            new RegionalVoiceMap(tts));
    }

    private static PrivacySafeOrderSummary Summary(string productName) =>
        PrivacySafeOrderSummary.Create(
            "Quý khách",
            "DH-Q12",
            [SpeechItem.Create(productName, 2, "hộp")],
            Money.Vnd(560_000m),
            ShortDeliveryArea.Create("Quận 7"),
            "Giờ Vàng",
            null,
            SpeechSummaryLimits.Create(20, 20));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

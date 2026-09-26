using Ivr.Domain.Confirmation;
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
/// W-0354 / B16 (chief worklist 2026-09-25). A value the speller refuses used to escape the
/// renderer as a bare <see cref="ArgumentException"/>, and both dispatch gateways map an unknown
/// exception to "network error, channel unhealthy": one bad order quarantined a SIM, three in ten
/// minutes disabled it. The renderer now names the fault as the order's, and it arrives as a
/// <see cref="TtsSynthesisException"/> so the gateways' existing AudioError arm - channel healthy -
/// is the one that handles it.
/// </summary>
public sealed class SpeechRenderRejectionTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 3, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("210636.8")]
    [InlineData("1000000000000")]
    [Trait("TestId", "UT-RENDER-DATA-01")]
    public async Task AnAmountTheSpellerRefusesIsTheOrdersFaultNotTheChannels(string amount)
    {
        using InMemoryScriptRegistry scripts = CreateScripts();
        ApprovedVietnameseSpeechRenderer renderer = CreateRenderer(scripts);
        PrivacySafeOrderSummary summary = Summary(decimal.Parse(
            amount,
            System.Globalization.CultureInfo.InvariantCulture));

        SpeechRenderRejectedException rejected =
            await Assert.ThrowsAsync<SpeechRenderRejectedException>(() => renderer.RenderAsync(
                summary,
                "SCRIPT-ORDER-CONFIRM",
                TargetV1SpeechPolicy.MockTemplateVersion,
                ExecutionMode.Mock,
                CancellationToken.None).AsTask());

        Assert.Equal(SpeechRenderRejectedException.TechnicalCode, rejected.TechnicalErrorCode);
        Assert.Equal("SPEECH_RENDER_DATA_REJECTED", rejected.TechnicalErrorCode);
        Assert.IsAssignableFrom<ArgumentException>(rejected.InnerException);
        // The type is the contract with the gateways: TtsSynthesisException is the arm that
        // reports AudioError with the channel healthy.
        Assert.IsAssignableFrom<TtsSynthesisException>(rejected);
    }

    [Fact]
    [Trait("TestId", "UT-RENDER-DATA-02")]
    public async Task AWholeAmountStillRendersAsBefore()
    {
        using InMemoryScriptRegistry scripts = CreateScripts();
        RenderedSpeech speech = await CreateRenderer(scripts).RenderAsync(
            Summary(210_637m),
            "SCRIPT-ORDER-CONFIRM",
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(speech.ExactText));
    }

    [Fact]
    [Trait("TestId", "UT-RENDER-DATA-03")]
    public async Task AScriptThatIsNotApprovedIsStillAPolicyRefusal()
    {
        // Only the renderer's data refusals change arm. An unapproved script stays a policy
        // refusal - an InvalidOperationException, which the gateways report with the channel
        // healthy - but since W-0359 / K-29 it is named as a render refusal, so it is no longer
        // recorded as "policy or token rejected".
        using InMemoryScriptRegistry scripts = CreateScripts();
        SpeechRenderPolicyRejectedException rejected =
            await Assert.ThrowsAsync<SpeechRenderPolicyRejectedException>(() => CreateRenderer(scripts).RenderAsync(
                Summary(210_637m),
                "SCRIPT-ORDER-CONFIRM",
                "v-never-approved",
                ExecutionMode.Mock,
                CancellationToken.None).AsTask());

        Assert.Equal("SPEECH_RENDER_POLICY_REJECTED", SpeechRenderPolicyRejectedException.TechnicalCode);
        Assert.IsAssignableFrom<InvalidOperationException>(rejected);
        Assert.IsNotAssignableFrom<TtsSynthesisException>(rejected);
        Assert.IsType<InvalidOperationException>(rejected.InnerException);
    }

    [Fact]
    [Trait("TestId", "UT-RENDER-DATA-04")]
    public async Task AScriptThePrivacyGuardRefusesIsARenderRefusalNotATokenOne()
    {
        // W-0359 / K-29. A finished script the privacy guard refuses, like a placeholder the
        // renderer cannot fill or a script past the length bound, used to escape as a bare
        // InvalidOperationException and be recorded as a dial-token rejection.
        //
        // The case used to be "Đường phèn", which intake admits and the full guard refused on the
        // finished script. Q-12 (PA1) holds the item names to the product guard at the dial too,
        // so it now renders (UT-PII-PRODUCT-06). The refusal kept here is one the product guard
        // makes as well: a quantity with more decimals than the speller says falls back to its
        // digits, and these digits read as a telephone number.
        using InMemoryScriptRegistry scripts = CreateScripts();
        SpeechRenderPolicyRejectedException rejected =
            await Assert.ThrowsAsync<SpeechRenderPolicyRejectedException>(() => CreateRenderer(scripts).RenderAsync(
                Summary(210_637m, quantity: 0.0912345678m),
                "SCRIPT-ORDER-CONFIRM",
                TargetV1SpeechPolicy.MockTemplateVersion,
                ExecutionMode.Mock,
                CancellationToken.None).AsTask());

        InvalidOperationException refused = Assert.IsType<InvalidOperationException>(rejected.InnerException);
        Assert.Equal("A restricted PII value was rejected.", refused.Message);
        Assert.Equal("SPEECH_RENDER_POLICY_REJECTED", SpeechRenderPolicyRejectedException.TechnicalCode);
    }

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

    private static PrivacySafeOrderSummary Summary(
        decimal amount,
        string productName = "Cháo sâm",
        decimal quantity = 2) =>
        PrivacySafeOrderSummary.Create(
            "Quý khách",
            "DH-B16",
            [SpeechItem.Create(productName, quantity, "hộp")],
            Money.Vnd(amount),
            ShortDeliveryArea.Create("Quận 7"),
            "Giờ Vàng",
            null,
            SpeechSummaryLimits.Create(20, 20));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

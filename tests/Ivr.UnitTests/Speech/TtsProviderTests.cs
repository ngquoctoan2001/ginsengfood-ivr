using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Ports;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Speech;
using Ivr.UnitTests.Confirmation;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

public sealed class TtsProviderTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 14, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-TTS-SNAPSHOT-01")]
    public async Task VietnameseOneAndManyItemSnapshotsProduceStableAudioReferences()
    {
        PrivacySafeOrderSummary oneItem = TestData.Summary();
        PrivacySafeOrderSummary manyItems = Summary(
            [
                SpeechItem.Create("Sâm lát", 1, "hộp"),
                SpeechItem.Create("Trà sâm", 2, "gói"),
                SpeechItem.Create("Mật ong", 3, "chai"),
                SpeechItem.Create("Kẹo sâm", 4, "túi"),
            ],
            9_876_500);
        RenderedSpeech firstText = await RenderAsync(oneItem);
        RenderedSpeech manyText = await RenderAsync(manyItems);
        var time = new FixedTimeProvider(Now);
        SpeechSynthesisService service = CreateService(
            new FakeDeterministicTtsProvider(OptionsFor()),
            time,
            out TtsUsageMeter usage);

        RenderedSpeech first = await SynthesizeAsync(service, firstText, oneItem);
        RenderedSpeech replay = await SynthesizeAsync(service, firstText, oneItem);
        RenderedSpeech collapsed = await SynthesizeAsync(service, manyText, manyItems);

        Assert.Equal(first.Audio, replay.Audio);
        Assert.NotNull(first.Audio);
        Assert.Equal("audio/L16", first.Audio.Format);
        Assert.Equal(8_000, first.Audio.SampleRate);
        Assert.StartsWith("memory://tts/fake/", first.Audio.ContentRef, StringComparison.Ordinal);
        Assert.Contains("và một sản phẩm khác", collapsed.ExactText, StringComparison.Ordinal);
        Assert.Equal(1, collapsed.CollapsedItemCount);
        Assert.NotEqual(first.Audio.ContentRef, collapsed.Audio!.ContentRef);
        Assert.Equal(new TtsUsageSnapshot(2, firstText.ExactText.Length + manyText.ExactText.Length, 1, 2, 0),
            usage.Snapshot());

        var provider = new FakeDeterministicTtsProvider(OptionsFor());
        RenderedAudio collisionProbeA = await provider.SynthesizeAsync(
            SpeechScript.Create("script", "v1", "Nội dung A.", "content-hash-a", "summary-hash"),
            TtsOptions.Create(),
            CancellationToken.None);
        RenderedAudio collisionProbeB = await provider.SynthesizeAsync(
            SpeechScript.Create("script", "v1", "Nội dung B.", "content-hash-b", "summary-hash"),
            TtsOptions.Create(),
            CancellationToken.None);
        Assert.NotEqual(collisionProbeA.ContentRef, collisionProbeB.ContentRef);
    }

    [Fact]
    [Trait("TestId", "UT-TTS-VND-02")]
    public async Task LargeVndAndDecimalQuantityUseVietnameseGroupsAndUnits()
    {
        PrivacySafeOrderSummary summary = Summary(
            [SpeechItem.Create("Sâm lát", 12.5m, "hộp")],
            123_456_789);

        RenderedSpeech text = await RenderAsync(summary);

        // Fractional quantities are spoken now (W-0106 A7). The digit form was the one input
        // segmented playback could not voice, and it was never verified by listening.
        Assert.Contains("mười hai phẩy năm hộp Sâm lát", text.ExactText, StringComparison.Ordinal);
        Assert.Contains(
            "một trăm hai mươi ba triệu bốn trăm năm mươi sáu nghìn bảy trăm tám mươi chín đồng",
            text.ExactText,
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-TTS-PRON-03")]
    public async Task ExactHintsHandleVietnameseUnicodeAndEmojiWithoutBreakingSynthesis()
    {
        PrivacySafeOrderSummary summary = PrivacySafeOrderSummary.Create(
            "Chị Ánh",
            "DH-UNICODE",
            [SpeechItem.Create("KGC Plus 🌿", 1, "hộp")],
            Money.Vnd(560_000),
            ShortDeliveryArea.Create("Quận 3"),
            "Giờ Vàng",
            new Dictionary<string, string>
            {
                ["KGC Plus 🌿"] = "K G C pờ lớt thảo mộc 🌿",
            },
            SpeechSummaryLimits.Create(20, 20));
        RenderedSpeech text = await RenderAsync(summary);
        var time = new FixedTimeProvider(Now);
        SpeechSynthesisService service = CreateService(
            new FakeDeterministicTtsProvider(OptionsFor()),
            time,
            out _);

        RenderedSpeech speech = await SynthesizeAsync(service, text, summary);

        Assert.Contains("K G C pờ lớt thảo mộc 🌿", speech.ExactText, StringComparison.Ordinal);
        Assert.DoesNotContain("KGC Plus", speech.ExactText, StringComparison.Ordinal);
        Assert.NotNull(speech.Audio);
    }

    [Fact]
    [Trait("TestId", "UT-TTS-PII-04")]
    public async Task FinalPhoneOrStreetAddressIsBlockedBeforeProviderSynthesis()
    {
        TtsOptions options = TtsOptions.Create();
        foreach (string unsafeText in new[]
                 {
                     "Xin gọi số 0901234567 để xác nhận.",
                     "Xin gọi số 090-123-4567 để xác nhận.",
                     "Giao tới Đường Nguyễn Huệ, Phường Bến Nghé.",
                 })
        {
            SpeechScript script = SpeechScript.Create(
                "SCRIPT-ORDER-CONFIRM",
                "v1-test-approved",
                unsafeText,
                "hash-content-safe",
                "hash-summary-safe");

            IvrFailureException failure = Assert.Throws<IvrFailureException>(
                () => SpeechPrivacyGuard.EnsureSafe(script, options));
            Assert.Equal(IvrErrorCodes.PiiPolicyViolation, failure.ErrorCode);
        }

        PrivacySafeOrderSummary summary = TestData.Summary();
        RenderedSpeech rendered = await RenderAsync(summary);
        var time = new FixedTimeProvider(Now);
        var countingProvider = new CountingTtsProvider();
        SpeechSynthesisService service = CreateService(
            countingProvider,
            time,
            out _,
            new TtsProviderOptions
            {
                ExecutionMode = "MOCK",
                Provider = TtsProviderOptions.FakeProvider,
                VoiceId = "0901234567",
            });
        IvrFailureException configuredPii = await Assert.ThrowsAsync<IvrFailureException>(
            () => SynthesizeAsync(service, rendered, summary));
        Assert.Equal(IvrErrorCodes.PiiPolicyViolation, configuredPii.ErrorCode);
        Assert.Equal(0, countingProvider.Calls);
    }

    [Fact]
    [Trait("TestId", "UT-TTS-TIMEOUT-05")]
    public async Task TimeoutNormalizesAsNonCountedTechnicalFailureAndNeverNoAnswer()
    {
        PrivacySafeOrderSummary summary = TestData.Summary();
        RenderedSpeech text = await RenderAsync(summary);
        var configured = new TtsProviderOptions
        {
            ExecutionMode = "MOCK",
            Provider = TtsProviderOptions.FakeProvider,
            TimeoutMilliseconds = 20,
        };
        var time = new FixedTimeProvider(Now);
        SpeechSynthesisService service = CreateService(
            new NeverCompletingTtsProvider(),
            time,
            out _,
            configured);

        TtsSynthesisException failure = await Assert.ThrowsAsync<TtsSynthesisException>(
            () => SynthesizeAsync(service, text, summary));
        NormalizedResult normalized = DispositionMapper.Normalize(
            SimProviderDisposition.AudioError,
            null,
            failure.TechnicalErrorCode,
            new AttemptNormalizationContext(
                1,
                2,
                Now,
                Now.AddMinutes(5),
                0,
                1));

        Assert.Equal("TTS_TIMEOUT", failure.TechnicalErrorCode);
        Assert.Equal("IVR_TECHNICAL_EXCEPTION", normalized.ResultStatus);
        Assert.False(normalized.IsCounted);
        Assert.False(normalized.IsNoAnswer);
    }

    /// <summary>
    /// The external provider used to be a skeleton that threw unconditionally. It now speaks
    /// HTTP, so "fails closed until a vendor decision exists" has to be re-proved rather than
    /// assumed: an unconfigured deployment must still refuse, and must refuse **before** it
    /// opens a socket. The stub factory throws if anyone asks it for a client, which is what
    /// turns "no request was sent" from a claim into an assertion.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TTS-NOTCONFIGURED-06")]
    public async Task ExternalProviderFailsClosedAndSendsNothingUntilItIsConfigured()
    {
        TtsProviderOptions configured = new()
        {
            ExecutionMode = "LAB_REAL_SIM",
            Provider = TtsProviderOptions.UnselectedProvider,
        };
        var provider = new ConfigurableExternalTtsProvider(
            new ThrowingHttpClientFactory(),
            Options.Create(configured));
        Assert.Equal("[REDACTED_TTS_PROVIDER_OPTIONS]", configured.ToString());
        Assert.Equal("[REDACTED_EXTERNAL_TTS_OPTIONS]", configured.External.ToString());

        TtsProviderNotConfiguredException failure =
            await Assert.ThrowsAsync<TtsProviderNotConfiguredException>(() =>
                provider.SynthesizeAsync(
                    SpeechScript.Create("script", "v1", "Nội dung an toàn.", "content-hash", "summary-hash"),
                    TtsOptions.Create(),
                    CancellationToken.None));

        Assert.Equal("TTS_NOT_CONFIGURED", failure.TechnicalErrorCode);
    }

    [Fact]
    [Trait("TestId", "UT-TTS-WHITELIST-07")]
    public async Task ProductionWithoutWhitelistApprovalRecordFailsBeforeProviderCall()
    {
        PrivacySafeOrderSummary summary = TestData.Summary();
        RenderedSpeech text = await RenderAsync(summary);
        var provider = new CountingTtsProvider();
        var time = new FixedTimeProvider(Now);
        SpeechSynthesisService service = CreateService(
            provider,
            time,
            out _,
            new TtsProviderOptions
            {
                ExecutionMode = "PRODUCTION_REAL",
                Provider = TtsProviderOptions.ExternalProvider,
                ProductionWhitelistApprovalRecord = string.Empty,
            });

        IvrFailureException failure = await Assert.ThrowsAsync<IvrFailureException>(() =>
            service.SynthesizeAsync(
                text,
                summary,
                "SCRIPT-ORDER-CONFIRM",
                "v1-test-approved",
                ExecutionMode.ProductionReal,
                Now.AddMinutes(5),
                CancellationToken.None));

        Assert.Equal(IvrErrorCodes.OperationalBlocked, failure.ErrorCode);
        Assert.Equal(0, provider.Calls);
    }

    private static PrivacySafeOrderSummary Summary(
        IEnumerable<SpeechItem> items,
        decimal amount) => PrivacySafeOrderSummary.Create(
        "Chị Mai",
        "DH-TTS-01",
        items,
        Money.Vnd(amount),
        ShortDeliveryArea.Create("Quận 7"),
        "24 trên 7",
        null,
        SpeechSummaryLimits.Create(20, 20));

    private static async Task<RenderedSpeech> RenderAsync(PrivacySafeOrderSummary summary) =>
        await new FakeSpeechRenderer().RenderAsync(
            summary,
            "SCRIPT-ORDER-CONFIRM",
            Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);

    private static Task<RenderedSpeech> SynthesizeAsync(
        SpeechSynthesisService service,
        RenderedSpeech rendered,
        PrivacySafeOrderSummary summary) => service.SynthesizeAsync(
        rendered,
        summary,
        "SCRIPT-ORDER-CONFIRM",
        Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
        ExecutionMode.Mock,
        Now.AddMinutes(5),
        CancellationToken.None);

    private static IOptions<TtsProviderOptions> OptionsFor(
        TtsProviderOptions? configured = null) => Options.Create(
        configured ?? new TtsProviderOptions
        {
            ExecutionMode = "MOCK",
            Provider = TtsProviderOptions.FakeProvider,
        });

    private static SpeechSynthesisService CreateService(
        ITtsProvider provider,
        TimeProvider timeProvider,
        out TtsUsageMeter usage,
        TtsProviderOptions? configured = null)
    {
        usage = new TtsUsageMeter();
        IOptions<TtsProviderOptions> options = OptionsFor(configured);
        return new SpeechSynthesisService(
            provider,
            new AudioCache(timeProvider),
            new TtsRequestBudget(timeProvider),
            usage,
            new RegionalVoiceMap(options),
            options,
            timeProvider);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class ThrowingHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) =>
            throw new InvalidOperationException(
                "An unconfigured external TTS provider must not reach for an HTTP client.");
    }

    private sealed class NeverCompletingTtsProvider : ITtsProvider
    {
        public async Task<RenderedAudio> SynthesizeAsync(
            SpeechScript script,
            TtsOptions options,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable test branch.");
        }
    }

    private sealed class CountingTtsProvider : ITtsProvider
    {
        public int Calls { get; private set; }

        public Task<RenderedAudio> SynthesizeAsync(
            SpeechScript script,
            TtsOptions options,
            CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(RenderedAudio.Create(
                "audio/L16",
                8_000,
                TimeSpan.FromSeconds(1),
                "memory://tts/counting/safe"));
        }
    }
}

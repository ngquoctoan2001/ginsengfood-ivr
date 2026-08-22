using Ivr.Domain.Confirmation;
using Ivr.Domain.Speech;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Speech;
using Ivr.UnitTests.Confirmation;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

public sealed class RegionalVoiceRoutingTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-VOICE-CFG-01")]
    public void ThreeRegionsSharingOneVoiceIdFailsStartupInsteadOfSoundingIdentical()
    {
        // The whole point of W-0106 is three voices. A copy-paste that leaves the same id in all
        // three slots produces a deployment that claims three regions and plays one, and nothing
        // at runtime would ever say so.
        var validator = new TtsProviderOptionsValidator();

        ValidateOptionsResult duplicated = validator.Validate(null, Options(regional =>
        {
            regional.North.VoiceId = "voice-same";
            regional.Central.VoiceId = "voice-same";
            regional.South.VoiceId = "voice-same";
        }));

        Assert.True(duplicated.Failed);
        Assert.Contains(
            duplicated.Failures!,
            failure => failure.Contains("distinct voice ID", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-CFG-02")]
    public void EveryRegionNeedsAVoiceAndRatesStayInBounds()
    {
        var validator = new TtsProviderOptionsValidator();

        Assert.True(validator.Validate(null, Options(regional =>
            regional.Central.VoiceId = string.Empty)).Failed);
        Assert.True(validator.Validate(null, Options(regional =>
            regional.South.SpeakingRate = 3m)).Failed);

        // Zero means "inherit the global rate", so it must stay valid.
        Assert.True(validator.Validate(null, Options(regional =>
            regional.North.SpeakingRate = 0m)).Succeeded);
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-CFG-03")]
    public void DisablingRegionalVoicesRestoresSingleVoiceRoutingWithoutValidationNoise()
    {
        var validator = new TtsProviderOptionsValidator();
        TtsProviderOptions configured = Options(regional =>
        {
            regional.Enabled = false;
            regional.North.VoiceId = "same";
            regional.Central.VoiceId = "same";
            regional.South.VoiceId = "same";
        });

        // Duplicates are irrelevant while the feature is off; the validator must not block a
        // rollback because of settings nothing reads.
        Assert.True(validator.Validate(null, configured).Succeeded);

        var map = new RegionalVoiceMap(Microsoft.Extensions.Options.Options.Create(configured));
        RegionalVoiceSelection selection = map.Resolve("phường Phú Khương, tỉnh Vĩnh Long");

        Assert.False(map.Enabled);
        Assert.Equal(configured.VoiceId, selection.VoiceId);
        Assert.Equal(configured.SpeakingRate, selection.SpeakingRate);
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-CFG-04")]
    public void StaticFileRegionalVoicesRequireDistinctSafeMediaPerRegion()
    {
        var validator = new TtsProviderOptionsValidator();

        Assert.True(validator.Validate(null, LabOptions(regional =>
            regional.Central.FileMediaReference = "/etc/passwd")).Failed);
        Assert.True(validator.Validate(null, LabOptions(regional =>
            regional.South.FileMediaReference = "sound:ivr-lab-order-confirmation-n")).Failed);
        Assert.True(validator.Validate(null, LabOptions(regional =>
            regional.North.FileDurationSeconds = 0)).Failed);
        Assert.True(validator.Validate(null, LabOptions(_ => { })).Succeeded);
    }

    [Theory]
    [Trait("TestId", "UT-SPEECH-VOICE-01")]
    [InlineData("phường Cửa Nam, thành phố Hà Nội", VietnamRegion.North, "voice-north")]
    [InlineData("phường Hải Châu, thành phố Đà Nẵng", VietnamRegion.Central, "voice-central")]
    [InlineData("phường Phú Khương, tỉnh Vĩnh Long", VietnamRegion.South, "voice-south")]
    public void DeliveryAreaPicksTheRegionalVoice(
        string deliveryArea,
        VietnamRegion expectedRegion,
        string expectedVoice)
    {
        var map = new RegionalVoiceMap(
            Microsoft.Extensions.Options.Options.Create(Options(_ => { })));

        RegionalVoiceSelection selection = map.Resolve(deliveryArea);

        Assert.Equal(expectedRegion, selection.Region);
        Assert.Equal(expectedVoice, selection.VoiceId);
        Assert.True(selection.ResolvedFromDeliveryArea);
    }

    [Fact]
    [Trait("TestId", "UT-SPEECH-VOICE-02")]
    public void UnresolvedAreaUsesTheConfiguredFallbackAndIsFlaggedAsUnresolved()
    {
        var map = new RegionalVoiceMap(
            Microsoft.Extensions.Options.Options.Create(Options(regional =>
                regional.FallbackRegion = VietnamRegion.South)));

        RegionalVoiceSelection selection = map.Resolve("khu vực chưa xác định");

        Assert.Equal(VietnamRegion.South, selection.Region);
        Assert.Equal("voice-south", selection.VoiceId);

        // The flag is what separates "a Southern customer" from "we could not tell" in the
        // metric. Collapsing them would hide Sales master-data drift behind normal traffic.
        Assert.False(selection.ResolvedFromDeliveryArea);
        Assert.Equal(map.FallbackRegion, selection.Region);
    }

    [Fact]
    [Trait("TestId", "UT-SPEECH-VOICE-03")]
    public async Task ThreeRegionsProduceThreeSeparateCacheEntriesAndOneMissEach()
    {
        TtsProviderOptions configured = Options(_ => { });
        IOptions<TtsProviderOptions> options = Microsoft.Extensions.Options.Options.Create(configured);
        var usage = new TtsUsageMeter();
        var time = new FixedTimeProvider(Now);
        var service = new SpeechSynthesisService(
            new FakeDeterministicTtsProvider(options),
            new AudioCache(time),
            new TtsRequestBudget(time),
            usage,
            new RegionalVoiceMap(options),
            options,
            time);

        RenderedSpeech north = await SynthesizeAsync(service, "phường Cửa Nam, thành phố Hà Nội");
        RenderedSpeech central = await SynthesizeAsync(service, "phường Hải Châu, thành phố Đà Nẵng");
        RenderedSpeech south = await SynthesizeAsync(service, "phường Phú Khương, tỉnh Vĩnh Long");
        RenderedSpeech northAgain = await SynthesizeAsync(service, "phường Cửa Nam, thành phố Hà Nội");

        // AudioCacheKey already carried VoiceId, so three voices needed no cache change at all —
        // this proves the regions do not collide rather than assuming it.
        Assert.NotEqual(north.Audio!.ContentRef, central.Audio!.ContentRef);
        Assert.NotEqual(central.Audio.ContentRef, south.Audio!.ContentRef);
        Assert.NotEqual(north.Audio.ContentRef, south.Audio.ContentRef);
        Assert.Equal(north.Audio.ContentRef, northAgain.Audio!.ContentRef);

        TtsUsageSnapshot snapshot = usage.Snapshot();
        Assert.Equal(3, snapshot.ProviderRequests);
        Assert.Equal(1, snapshot.CacheHits);
        Assert.Equal(3, snapshot.CacheMisses);

        Assert.Equal(
            new TtsVoiceRoutingSnapshot(2, 1, 1, 0),
            usage.VoiceRoutingSnapshot());
    }

    [Fact]
    [Trait("TestId", "UT-TTS-TELEMETRY-04")]
    public async Task UnresolvedDeliveryAreaIncrementsTheDataQualityCounter()
    {
        TtsProviderOptions configured = Options(regional =>
            regional.FallbackRegion = VietnamRegion.North);
        IOptions<TtsProviderOptions> options = Microsoft.Extensions.Options.Options.Create(configured);
        var usage = new TtsUsageMeter();
        var time = new FixedTimeProvider(Now);
        var service = new SpeechSynthesisService(
            new FakeDeterministicTtsProvider(options),
            new AudioCache(time),
            new TtsRequestBudget(time),
            usage,
            new RegionalVoiceMap(options),
            options,
            time);

        await SynthesizeAsync(service, "phường Cửa Nam, thành phố Hà Nội");
        await SynthesizeAsync(service, "khu vực chưa xác định");

        // Both routed North, but only one of them was actually known to be Northern.
        Assert.Equal(new TtsVoiceRoutingSnapshot(2, 0, 0, 1), usage.VoiceRoutingSnapshot());
    }

    [Fact]
    [Trait("TestId", "UT-TTS-STATIC-REGION-05")]
    public async Task StaticFileProviderPlaysTheMediaBelongingToTheSelectedVoice()
    {
        TtsProviderOptions configured = LabOptions(_ => { });
        IOptions<TtsProviderOptions> options = Microsoft.Extensions.Options.Options.Create(configured);
        var map = new RegionalVoiceMap(options);
        var provider = new StaticFileTtsProvider(options, map);

        RenderedAudio south = await provider.SynthesizeAsync(
            Script(),
            TtsOptions.Create("vi-VN", map.Resolve("tỉnh Vĩnh Long").VoiceId),
            CancellationToken.None);
        RenderedAudio north = await provider.SynthesizeAsync(
            Script(),
            TtsOptions.Create("vi-VN", map.Resolve("thành phố Hà Nội").VoiceId),
            CancellationToken.None);

        Assert.Equal("sound:ivr-lab-order-confirmation-s", south.ContentRef);
        Assert.Equal("sound:ivr-lab-order-confirmation-n", north.ContentRef);
        Assert.NotEqual(south.Duration, north.Duration);

        // A voice with no media file must fail loudly. Silently playing another region's audio
        // would be a customer hearing the wrong order details, not a cosmetic defect.
        await Assert.ThrowsAsync<TtsProviderNotConfiguredException>(async () =>
            await provider.SynthesizeAsync(
                Script(),
                TtsOptions.Create("vi-VN", "voice-not-configured"),
                CancellationToken.None));
    }

    private static SpeechScript Script() => SpeechScript.Create(
        "SCRIPT-ORDER-CONFIRM",
        "v3-test-approved",
        "Nội dung đơn fake an toàn.",
        "content-hash",
        "summary-hash");

    private static async Task<RenderedSpeech> SynthesizeAsync(
        SpeechSynthesisService service,
        string deliveryArea)
    {
        PrivacySafeOrderSummary summary = PrivacySafeOrderSummary.Create(
            "Quý khách",
            "DH-R1",
            [SpeechItem.Create("Cháo sâm", 2, "hộp")],
            Money.Vnd(560_000),
            ShortDeliveryArea.Create(deliveryArea),
            "Golden Hour",
            null,
            SpeechSummaryLimits.Create(20, 5));
        RenderedSpeech text = await new FakeSpeechRenderer().RenderAsync(
            summary,
            "SCRIPT-ORDER-CONFIRM",
            Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);

        return await service.SynthesizeAsync(
            text,
            summary,
            "SCRIPT-ORDER-CONFIRM",
            Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            Now.AddMinutes(10),
            CancellationToken.None);
    }

    private static TtsProviderOptions Options(Action<RegionalVoiceOptions> configure)
    {
        var options = new TtsProviderOptions
        {
            ExecutionMode = "MOCK",
            Provider = TtsProviderOptions.FakeProvider,
            RegionalVoices = new RegionalVoiceOptions
            {
                Enabled = true,
                FallbackRegion = VietnamRegion.North,
                North = new RegionalVoiceEntry { VoiceId = "voice-north" },
                Central = new RegionalVoiceEntry { VoiceId = "voice-central" },
                South = new RegionalVoiceEntry { VoiceId = "voice-south" },
            },
        };
        configure(options.RegionalVoices);
        return options;
    }

    private static TtsProviderOptions LabOptions(Action<RegionalVoiceOptions> configure)
    {
        TtsProviderOptions options = Options(_ => { });
        options.ExecutionMode = "LAB_REAL_SIM";
        options.Provider = TtsProviderOptions.StaticFileProvider;
        options.RegionalVoices.North.FileMediaReference = "sound:ivr-lab-order-confirmation-n";
        options.RegionalVoices.North.FileDurationSeconds = 17;
        options.RegionalVoices.Central.FileMediaReference = "sound:ivr-lab-order-confirmation-c";
        options.RegionalVoices.Central.FileDurationSeconds = 18;
        options.RegionalVoices.South.FileMediaReference = "sound:ivr-lab-order-confirmation-s";
        options.RegionalVoices.South.FileDurationSeconds = 19;
        configure(options.RegionalVoices);
        return options;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

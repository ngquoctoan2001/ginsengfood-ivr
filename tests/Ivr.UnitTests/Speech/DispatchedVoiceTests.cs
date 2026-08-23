using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

/// <summary>
/// Recording the voice an attempt dialled with, rather than re-deriving it later (W-0113).
/// </summary>
public sealed class DispatchedVoiceTests
{
    /// <summary>
    /// The spelling is load-bearing. <c>voice_region</c> has emitted <c>North</c>/<c>Central</c>/
    /// <c>South</c> since W-0106; those exact strings are pinned in the OpenAPI enum and key the
    /// console's Vietnamese dictionary. A recorded value in a second spelling would render as a
    /// raw code on the very screens this work exists to make trustworthy — and it would do so
    /// only for calls made after the migration, which is the hardest kind of drift to notice.
    /// </summary>
    [Theory]
    [InlineData(VietnamRegion.North, "North")]
    [InlineData(VietnamRegion.Central, "Central")]
    [InlineData(VietnamRegion.South, "South")]
    [Trait("TestId", "UT-VOICE-RECORD-01")]
    public void TheRecordedRegionUsesTheSameSpellingAsTheDerivedOne(
        VietnamRegion region,
        string expected)
    {
        DispatchedVoice voice = DispatchedVoice.Create("voice-x", region, true);

        Assert.Equal(expected, voice.RegionWireForm);

        // And the derived path, which has emitted this since W-0106, agrees.
        Assert.Equal(expected, region.ToString());

        Assert.True(DispatchedVoice.TryParseRegion(expected, out VietnamRegion parsed));
        Assert.Equal(region, parsed);
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-RECORD-02")]
    public void AVoiceWithNoIdIsRefusedRatherThanRecordedAsBlank()
    {
        Assert.Throws<ArgumentException>(
            () => DispatchedVoice.Create(" ", VietnamRegion.North, true));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DispatchedVoice.Create(new string('v', 121), VietnamRegion.North, true));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DispatchedVoice.Create("voice-x", (VietnamRegion)99, true));

        Assert.False(DispatchedVoice.TryParseRegion("Northern", out _));
        Assert.False(DispatchedVoice.TryParseRegion(null, out _));
    }

    /// <summary>
    /// Two identical playlists read in different voices are not the same audio. Worth asserting
    /// because <c>RenderedAudio</c> hand-writes its equality — the generated one compared the
    /// segment array by reference — and a field added to the record but forgotten in that
    /// hand-written comparison is exactly the kind of omission nothing else would catch.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-VOICE-RECORD-03")]
    public void AudioComparesTheVoiceItWasProducedWith()
    {
        RenderedAudio bare = RenderedAudio.Create(
            "audio/L16",
            8_000,
            TimeSpan.FromSeconds(12),
            "media:sample-1");
        RenderedAudio north = bare.WithVoice(
            DispatchedVoice.Create("voice-north", VietnamRegion.North, true));
        RenderedAudio south = bare.WithVoice(
            DispatchedVoice.Create("voice-south", VietnamRegion.South, true));

        Assert.Null(bare.Voice);
        Assert.NotEqual(north, south);
        Assert.NotEqual(bare, north);
        Assert.Equal(
            north,
            bare.WithVoice(DispatchedVoice.Create("voice-north", VietnamRegion.North, true)));

        // Attaching a voice changes nothing else about the audio.
        Assert.Equal(bare.ContentRef, north.ContentRef);
        Assert.Equal(bare.PlaylistHash, north.PlaylistHash);
        Assert.Equal(bare.Duration, north.Duration);
    }

    /// <summary>
    /// The end-to-end claim: what synthesis chose is what the audio carries. Asserted against a
    /// delivery area rather than a stubbed selection, because the failure this work prevents is
    /// precisely a disagreement between the routing decision and what gets recorded about it.
    /// </summary>
    [Theory]
    [InlineData("phường Cửa Nam, thành phố Hà Nội", "North", true)]
    [InlineData("phường Hải Châu, thành phố Đà Nẵng", "Central", true)]
    [InlineData("phường Phú Khương, tỉnh Vĩnh Long", "South", true)]
    [Trait("TestId", "UT-VOICE-RECORD-04")]
    public async Task SynthesisAttachesTheVoiceItActuallyRouted(
        string deliveryArea,
        string expectedRegion,
        bool expectedResolved)
    {
        RenderedSpeech speech = await SynthesizeAsync(deliveryArea);

        DispatchedVoice voice = Assert.IsType<DispatchedVoice>(speech.Audio?.Voice);
        Assert.Equal(expectedRegion, voice.RegionWireForm);
        Assert.Equal(expectedResolved, voice.ResolvedFromDeliveryArea);
        Assert.False(string.IsNullOrWhiteSpace(voice.VoiceId));
    }

    /// <summary>
    /// An unrecognised area still gets a voice — the fallback one — and the record says so. The
    /// flag is the difference between "South because we recognised the address" and "South
    /// because South is the default", and only the first is evidence about this customer.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-VOICE-RECORD-05")]
    public async Task AFallbackRegionIsRecordedAsUnresolvedRatherThanAsAMatch()
    {
        RenderedSpeech speech = await SynthesizeAsync("khu vực chưa xác định");

        DispatchedVoice voice = Assert.IsType<DispatchedVoice>(speech.Audio?.Voice);
        Assert.False(voice.ResolvedFromDeliveryArea);
        Assert.Equal("North", voice.RegionWireForm);
    }

    /// <summary>
    /// A voice id is configuration, not customer data — but it is not console prose either, and
    /// the surrounding speech types all redact themselves for the same reason.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-VOICE-RECORD-06")]
    public void TheRecordRedactsItselfInStringForm()
    {
        DispatchedVoice voice = DispatchedVoice.Create("voice-north-a", VietnamRegion.North, true);

        Assert.DoesNotContain("voice-north-a", voice.ToString(), StringComparison.Ordinal);
    }

    private static readonly DateTimeOffset Now = new(2026, 8, 23, 6, 0, 0, TimeSpan.Zero);

    private static async Task<RenderedSpeech> SynthesizeAsync(string deliveryArea)
    {
        var configured = new TtsProviderOptions
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
        IOptions<TtsProviderOptions> options = Options.Create(configured);
        var time = new FixedTimeProvider(Now);
        var service = new SpeechSynthesisService(
            new FakeDeterministicTtsProvider(options),
            new AudioCache(time),
            new TtsRequestBudget(time),
            new TtsUsageMeter(),
            new RegionalVoiceMap(options),
            options,
            time);

        PrivacySafeOrderSummary summary = PrivacySafeOrderSummary.Create(
            "Quý khách",
            "DH-V1",
            [SpeechItem.Create("Cháo sâm", 2, "hộp")],
            Money.Vnd(560_000),
            ShortDeliveryArea.Create(deliveryArea),
            "Golden Hour",
            null,
            SpeechSummaryLimits.Create(20, 5));
        RenderedSpeech text = await new FakeSpeechRenderer().RenderAsync(
            summary,
            "SCRIPT-ORDER-CONFIRM",
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);

        return await service.SynthesizeAsync(
            text,
            summary,
            "SCRIPT-ORDER-CONFIRM",
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            Now.AddMinutes(5),
            CancellationToken.None);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

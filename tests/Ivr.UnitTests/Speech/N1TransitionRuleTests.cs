using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

/// <summary>
/// W-0363, lot L8 of the 25/09 remediation plan: the parts of N1 that wait on no decision.
/// <para>
/// N1 is the Tech Lead's transition rule of 24/09: production does not synthesize speech at call
/// time, it plays audio rendered ahead of time. These tests pin the three walls that keep
/// production from synthesizing (the provider it is given, the options it may start with, and the
/// speech service itself), and the playlist bound that used to be blamed on a SIM.
/// </para>
/// </summary>
public sealed class N1TransitionRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 26, 2, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// K-48. A call whose pieces add up past five minutes used to reach
    /// <see cref="RenderedAudio.CreatePlaylist"/> and fail there with an
    /// <see cref="ArgumentOutOfRangeException"/>, which both dispatch gateways record as a SIM
    /// fault: the channel went into quarantine for an order that was too long. It now fails as a
    /// <see cref="TtsSynthesisException"/>, the arm that keeps the channel healthy.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-N1-PLAYLIST-01")]
    public async Task APlaylistPastTheCallBoundFailsTheOrderNotTheSim()
    {
        // The 64-piece bound cannot be reached from a script: a segment's ordinal stops at 64.
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SpeechSegment.CreateDynamic(RenderedAudio.MaxPlaylistSegments + 1, null, "một"));

        // The duration bound can: pre-rendered prose of 80 seconds a piece adds up past five minutes.
        TtsProviderOptions configured = MockSegmented();
        configured.FixedSegments =
        [
            .. Catalog().Select(entry => new FixedSegmentMediaEntry
            {
                TextHash = entry.TextHash,
                MediaReference = entry.MediaReference,
                DurationMilliseconds = 80_000,
            }),
        ];
        var provider = new CountingTtsProvider();

        TtsSynthesisException failure = await Assert.ThrowsAsync<TtsSynthesisException>(() =>
            SynthesizeAsync(Service(provider, configured), ExecutionMode.Mock));

        Assert.Equal("TTS_PLAYLIST_TOO_LONG", failure.TechnicalErrorCode);
        Assert.Equal(SpeechSynthesisService.PlaylistTooLongCode, failure.TechnicalErrorCode);

        // The same catalog at a normal length plays: the bound is what failed, nothing else.
        configured.FixedSegments = Catalog();
        RenderedSpeech played = await SynthesizeAsync(Service(provider, configured), ExecutionMode.Mock);
        Assert.True(played.Audio!.Duration <= RenderedAudio.MaxPlaylistDuration);
    }

    /// <summary>
    /// K-49, the first wall. A production process is given a provider that refuses, and no HTTP
    /// client for the VieNeu sidecar. LAB is unchanged: the sidecar provider and its client.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-N1-NOSYNTH-01")]
    public async Task ProductionIsGivenAProviderThatRefusesAndNoSidecarClient()
    {
        ServiceCollection production = Speech(ExecutionModes.ProductionReal);
        ServiceDescriptor refusing = Assert.Single(production, descriptor => descriptor.ServiceType == typeof(ITtsProvider));
        Assert.Equal(typeof(RuntimeSynthesisForbiddenTtsProvider), refusing.ImplementationType);
        Assert.DoesNotContain(production, descriptor => descriptor.ServiceType == typeof(IHttpClientFactory));

        TtsSynthesisException refusal = await Assert.ThrowsAsync<TtsSynthesisException>(() =>
            new RuntimeSynthesisForbiddenTtsProvider().SynthesizeAsync(null!, null!, CancellationToken.None));
        Assert.Equal("TTS_RUNTIME_SYNTHESIS_FORBIDDEN", refusal.TechnicalErrorCode);

        ServiceCollection lab = Speech(ExecutionModes.LabRealSim);
        ServiceDescriptor sidecar = Assert.Single(lab, descriptor => descriptor.ServiceType == typeof(ITtsProvider));
        Assert.Equal(typeof(ConfigurableExternalTtsProvider), sidecar.ImplementationType);
        Assert.Contains(lab, descriptor => descriptor.ServiceType == typeof(IHttpClientFactory));
    }

    /// <summary>
    /// K-49, the second wall. A production deployment cannot start with the VieNeu sidecar as its
    /// provider. LAB still can, and production with no provider selected still starts.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-N1-NOSYNTH-02")]
    public void ProductionCannotStartWithTheSidecarAsItsProvider()
    {
        var validator = new TtsProviderOptionsValidator();

        ValidateOptionsResult production = validator.Validate(null, new TtsProviderOptions
        {
            ExecutionMode = ExecutionModes.ProductionReal,
            Provider = TtsProviderOptions.ExternalProvider,
        });
        Assert.True(production.Failed);
        Assert.Contains(production.Failures, failure => failure.Contains("N1", StringComparison.Ordinal));

        ValidateOptionsResult lab = validator.Validate(null, new TtsProviderOptions
        {
            ExecutionMode = ExecutionModes.LabRealSim,
            Provider = TtsProviderOptions.ExternalProvider,
        });
        Assert.DoesNotContain(lab.Failures ?? [], failure => failure.Contains("N1", StringComparison.Ordinal));

        ValidateOptionsResult unselected = validator.Validate(null, new TtsProviderOptions
        {
            ExecutionMode = ExecutionModes.ProductionReal,
            Provider = TtsProviderOptions.UnselectedProvider,
        });
        Assert.True(unselected.Succeeded);
    }

    /// <summary>
    /// K-49, the third wall. In production the speech service never calls a provider: not for the
    /// whole script, not for the order's own values in a segmented call (pre-rendered prose from
    /// the catalog still resolves, and the call fails at the first value that would need
    /// synthesis), and not when the caller passes a non-production mode from a production
    /// deployment, which is what B13's dial path does until K-42 lands. LAB still synthesizes.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-N1-NOSYNTH-03")]
    public async Task InProductionTheSpeechServiceNeverReachesAProvider()
    {
        var provider = new CountingTtsProvider();

        TtsSynthesisException whole = await Assert.ThrowsAsync<TtsSynthesisException>(() =>
            SynthesizeAsync(Service(provider, Production(segmented: false)), ExecutionMode.ProductionReal));
        Assert.Equal("TTS_RUNTIME_SYNTHESIS_FORBIDDEN", whole.TechnicalErrorCode);

        TtsSynthesisException segmented = await Assert.ThrowsAsync<TtsSynthesisException>(() =>
            SynthesizeAsync(Service(provider, Production(segmented: true)), ExecutionMode.ProductionReal));
        Assert.Equal("TTS_RUNTIME_SYNTHESIS_FORBIDDEN", segmented.TechnicalErrorCode);

        TtsSynthesisException wrongModeFromCaller = await Assert.ThrowsAsync<TtsSynthesisException>(() =>
            SynthesizeAsync(Service(provider, Production(segmented: true)), ExecutionMode.LabRealSim));
        Assert.Equal("TTS_RUNTIME_SYNTHESIS_FORBIDDEN", wrongModeFromCaller.TechnicalErrorCode);

        Assert.Equal(0, provider.Calls);

        TtsProviderOptions lab = Production(segmented: true);
        lab.ExecutionMode = ExecutionModes.LabRealSim;
        await SynthesizeAsync(Service(provider, lab), ExecutionMode.LabRealSim);
        Assert.Equal(3, provider.Calls);
    }

    private static ServiceCollection Speech(string executionMode)
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        services.AddIvrSpeech(configuration, executionMode);
        return services;
    }

    private static TtsProviderOptions MockSegmented() => new()
    {
        ExecutionMode = ExecutionModes.Mock,
        Provider = TtsProviderOptions.FakeProvider,
        Segmentation = new SpeechSegmentationOptions
        {
            Enabled = true,
            FixedSegments = FixedSegmentSource.Catalog,
        },
    };

    private static TtsProviderOptions Production(bool segmented) => new()
    {
        ExecutionMode = ExecutionModes.ProductionReal,
        Provider = TtsProviderOptions.UnselectedProvider,
        ProductionWhitelistApprovalRecord = "APPROVAL-TEST-ONLY",
        Segmentation = new SpeechSegmentationOptions
        {
            Enabled = segmented,
            FixedSegments = FixedSegmentSource.Catalog,
        },
        FixedSegments = segmented ? Catalog() : [],
    };

    private static FixedSegmentMediaEntry[] Catalog() =>
    [
        .. TargetV1SpeechPolicy
            .FixedSegmentHashes(TargetV1SpeechPolicy.CanonicalVietnameseTemplate)
            .Select((hash, index) => new FixedSegmentMediaEntry
            {
                TextHash = hash,
                MediaReference = $"sound:ivr-fixed-{index}",
                DurationMilliseconds = 2_000,
            }),
    ];

    private static PrivacySafeOrderSummary Summary() => PrivacySafeOrderSummary.Create(
        "Chị Mai",
        "DH-N1-01",
        [SpeechItem.Create("Sâm lát", 1, "hộp")],
        Money.Vnd(560_000m),
        ShortDeliveryArea.Create("Quận 7"),
        "24 trên 7",
        null,
        SpeechSummaryLimits.Create(20, 20));

    private static async Task<RenderedSpeech> SynthesizeAsync(
        SpeechSynthesisService service,
        ExecutionMode mode)
    {
        PrivacySafeOrderSummary summary = Summary();
        RenderedSpeech text = await new FakeSpeechRenderer().RenderAsync(
            summary,
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);
        return await service.SynthesizeAsync(
            text,
            summary,
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            mode,
            Now.AddMinutes(5),
            CancellationToken.None);
    }

    private static SpeechSynthesisService Service(ITtsProvider provider, TtsProviderOptions configured)
    {
        var time = new FixedTimeProvider(Now);
        IOptions<TtsProviderOptions> options = Options.Create(configured);
        return new SpeechSynthesisService(
            provider,
            new AudioCache(time),
            new TtsRequestBudget(time),
            new TtsUsageMeter(),
            new RegionalVoiceMap(options),
            options,
            time);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class CountingTtsProvider : ITtsProvider
    {
        public int Calls { get; private set; }

        public Task<RenderedAudio> SynthesizeAsync(
            SpeechScript script,
            TtsOptions options,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(script);
            Calls++;
            return Task.FromResult(RenderedAudio.Create(
                "audio/L16",
                8_000,
                TimeSpan.FromSeconds(1),
                string.Concat("sound:ivr-dyn-", script.ContentHash)));
        }
    }
}

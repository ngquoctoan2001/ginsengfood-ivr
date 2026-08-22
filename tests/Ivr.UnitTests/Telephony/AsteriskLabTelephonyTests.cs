using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Speech;
using Ivr.Infrastructure.Telephony;
using Ivr.UnitTests.Confirmation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Telephony;

public sealed class AsteriskLabTelephonyTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-AST-DI-01")]
    public async Task ExplicitLabProfileWiresAriWithoutRelaxingRealCustomerGuard()
    {
        IConfiguration configuration = Configuration();
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);
        services.AddIvrFeatureFlags(configuration);
        await using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        Assert.IsType<AsteriskSchedulerDispatchGateway>(
            provider.GetRequiredService<ISchedulerDispatchGateway>());
        Assert.IsType<AsteriskAriSimGateway>(provider.GetRequiredService<ISimGateway>());
        Assert.IsType<LabDialTokenVault>(provider.GetRequiredService<IDialTokenResolver>());
        Assert.Same(
            provider.GetRequiredService<IDialTokenResolver>(),
            provider.GetRequiredService<IOpaqueValueProtector>());
        Assert.False(provider.GetRequiredService<IOptions<IvrOptions>>().Value.RealCustomerCallAllowed);
        Assert.True(provider.GetRequiredService<ISchedulerDispatchGateway>().IsReady);
    }

    [Fact]
    [Trait("TestId", "UT-AST-GATE-02")]
    public async Task DispatchGateBlocksBeforeAnyAriOperation()
    {
        var store = new CapturingStore();
        var sim = new CountingSimGateway();
        var gate = new DeniedDispatchGate();
        var configured = Microsoft.Extensions.Options.Options.Create(Options());
        var gateway = new AsteriskSchedulerDispatchGateway(
            store,
            new FixedResolver(),
            new UnexpectedSpeechRenderer(),
            new UnexpectedSpeechSynthesisService(),
            sim,
            gate,
            configured,
            Microsoft.Extensions.Options.Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.LabRealSimExecutionMode,
                SalesProvider = "FAKE_TARGET_V1",
                SimProvider = "VENDOR",
                RealCustomerCallAllowed = false,
            }),
            new SchedulerExecutionContext(IvrOptions.LabRealSimExecutionMode),
            new FixedTimeProvider());
        SchedulerDispatchLease lease = Lease();

        AsteriskAriOperationException failure =
            await Assert.ThrowsAsync<AsteriskAriOperationException>(() =>
                gateway.DispatchAsync(lease, CancellationToken.None));

        Assert.Equal("ASTERISK_GATE_GLOBAL_KILL_SWITCH_ON", failure.TechnicalErrorCode);
        Assert.Equal(1, gate.Calls);
        Assert.Equal(0, sim.Calls);
        Assert.Equal("ASTERISK_GATE_GLOBAL_KILL_SWITCH_ON", store.FailureCode);
    }

    [Fact]
    [Trait("TestId", "UT-AST-AUDIO-03")]
    public async Task StaticFileProviderReturnsOnlyPinnedMediaReference()
    {
        AsteriskAriOptions ari = Options();
        var configured = new TtsProviderOptions
        {
            ExecutionMode = IvrOptions.LabRealSimExecutionMode,
            Provider = TtsProviderOptions.StaticFileProvider,
            OutputFormat = "audio/wav",
            SampleRate = 8_000,
            FileDurationSeconds = 18,
            FileMediaReference = "sound:ivr-lab-order-confirmation",
        };
        var labOptions = Microsoft.Extensions.Options.Options.Create(configured);
        var provider = new StaticFileTtsProvider(labOptions, new RegionalVoiceMap(labOptions));

        RenderedAudio audio = await provider.SynthesizeAsync(
            Ivr.Domain.Speech.SpeechScript.Create(
                "SCRIPT-ORDER-CONFIRM",
                "v1-test-approved",
                "Nội dung đơn fake an toàn.",
                "content-hash",
                "summary-hash"),
            Ivr.Domain.Speech.TtsOptions.Create(),
            CancellationToken.None);

        Assert.Equal("sound:ivr-lab-order-confirmation", audio.ContentRef);
        Assert.Equal(TimeSpan.FromSeconds(18), audio.Duration);
        Assert.Equal("[REDACTED_ASTERISK_ARI_OPTIONS]", ari.ToString());
        Assert.DoesNotContain("Nội dung", audio.ContentRef, StringComparison.Ordinal);
    }

    /// <summary>
    /// A segmented call reaches ARI as one ordered media list, and any unusable piece stops the
    /// whole thing.
    /// <para>
    /// Half a call is the dangerous outcome here, not a failed one. A customer who hears the
    /// greeting, silence where the items were, and then an amount has been read a different
    /// order — and they press 1 on it. A refused playback is retried; a wrong confirmation is
    /// acted on.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-PLAYLIST-06")]
    public void PlaylistBecomesOneOrderedMediaListAndRefusesAnyUnusablePiece()
    {
        RenderedAudio playlist = RenderedAudio.CreatePlaylist(
            "audio/L16",
            8_000,
            [
                new RenderedAudioSegment(string.Empty, "sound:ivr-fixed-greeting", TimeSpan.FromSeconds(4)),
                new RenderedAudioSegment(string.Empty, "sound:ivr-dyn-items", TimeSpan.FromSeconds(2)),
                new RenderedAudioSegment(string.Empty, "sound:ivr-fixed-total", TimeSpan.FromSeconds(1)),
            ]);

        Assert.Equal(
            "sound:ivr-fixed-greeting,sound:ivr-dyn-items,sound:ivr-fixed-total",
            AsteriskAriSimGateway.BuildMediaList(playlist));
        Assert.Equal(TimeSpan.FromSeconds(7), playlist.Duration);

        // A piece that is not a sound reference, anywhere in the list — not only first.
        AsteriskAriOperationException notASound = Assert.Throws<AsteriskAriOperationException>(
            () => AsteriskAriSimGateway.BuildMediaList(RenderedAudio.CreatePlaylist(
                "audio/L16",
                8_000,
                [
                    new RenderedAudioSegment(string.Empty, "sound:ivr-fixed-greeting", TimeSpan.FromSeconds(4)),
                    new RenderedAudioSegment(string.Empty, "memory://tts/fake/abc", TimeSpan.FromSeconds(2)),
                ])));
        Assert.Equal("ASTERISK_AUDIO_REFERENCE_INVALID", notASound.TechnicalErrorCode);

        // A comma inside one reference would split into two entries and shift the rest by one.
        Assert.Throws<AsteriskAriOperationException>(
            () => AsteriskAriSimGateway.BuildMediaList(RenderedAudio.CreatePlaylist(
                "audio/L16",
                8_000,
                [
                    new RenderedAudioSegment(string.Empty, "sound:ivr-a,sound:ivr-b", TimeSpan.FromSeconds(4)),
                ])));

        Assert.Throws<AsteriskAriOperationException>(
            () => AsteriskAriSimGateway.BuildMediaList(null));
    }

    [Fact]
    [Trait("TestId", "UT-AST-VAULT-04")]
    public async Task LabVaultFingerprintsTokenAndPinsSingleUseAlias()
    {
        var vault = new LabDialTokenVault(
            Microsoft.Extensions.Options.Options.Create(Options()));
        string fingerprint = vault.Protect(
            "ivr-confirmation-task-dial-token",
            "opaque-lab-token");
        var request = new DialTokenResolutionRequest(
            DialTokenReference.Create(fingerprint, Now.AddMinutes(5)),
            AttemptId.Create("attempt-lab-vault"));

        DialAuthorization authorization = await vault.ResolveAsync(
            request,
            Now,
            CancellationToken.None);

        Assert.StartsWith("enc:lab-sha256:", fingerprint, StringComparison.Ordinal);
        Assert.DoesNotContain("opaque-lab-token", fingerprint, StringComparison.Ordinal);
        Assert.Equal("LAB-A", authorization.RevealToTrustedGateway());
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await vault.ResolveAsync(request, Now, CancellationToken.None));
    }

    [Fact]
    [Trait("TestId", "UT-AST-CONFIG-05")]
    public void ValidatorRejectsProductionRawDestinationAndRecording()
    {
        AsteriskAriOptions configured = Options();
        configured.ExecutionMode = IvrOptions.ProductionRealExecutionMode;
        configured.BaseUrl = "https://telephony.example.com";
        configured.DestinationAlias = "0901234567";
        configured.RecordingEnabled = true;

        ValidateOptionsResult result = new AsteriskAriOptionsValidator().Validate(null, configured);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure =>
            failure.Contains("LAB_REAL_SIM", StringComparison.Ordinal));
        Assert.Contains(result.Failures, failure =>
            failure.Contains("recording", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Failures, failure =>
            failure.Contains("local Asterisk", StringComparison.OrdinalIgnoreCase));
    }

    private static IConfiguration Configuration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["IVR_EXECUTION_MODE"] = IvrOptions.LabRealSimExecutionMode,
            ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
            ["SIM_PROVIDER"] = "VENDOR",
            ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
            ["ConnectionStrings:IvrDb"] =
                "Host=localhost;Port=55433;Database=ivr_unit;Username=ivr",
            [$"{AsteriskAriOptions.SectionName}:Enabled"] = "true",
            [$"{AsteriskAriOptions.SectionName}:BaseUrl"] = "http://127.0.0.1:18088",
            [$"{AsteriskAriOptions.SectionName}:Username"] = "ivr-lab",
            [$"{AsteriskAriOptions.SectionName}:Password"] = "unit-test-password",
            [$"{AsteriskAriOptions.SectionName}:Application"] = "ivr-lab",
            [$"{AsteriskAriOptions.SectionName}:Environment"] = "lab",
            [$"{AsteriskAriOptions.SectionName}:DestinationAlias"] = "LAB-A",
            [$"{AsteriskAriOptions.SectionName}:SimChannelId"] = "SIM-ASTERISK-001",
            [$"{AsteriskAriOptions.SectionName}:AdapterMode"] = "ASTERISK_ARI",
            [$"{AsteriskAriOptions.SectionName}:ProviderName"] = "ASTERISK_ARI",
            [$"{TtsProviderOptions.SectionName}:Provider"] = TtsProviderOptions.StaticFileProvider,
        })
        .Build();

    private static AsteriskAriOptions Options() => new()
    {
        Enabled = true,
        ExecutionMode = IvrOptions.LabRealSimExecutionMode,
        BaseUrl = "http://127.0.0.1:18088",
        Username = "ivr-lab",
        Password = "unit-test-password",
        Application = "ivr-lab",
        Environment = "lab",
        DestinationAlias = "LAB-A",
        SimChannelId = "SIM-ASTERISK-001",
        AdapterMode = "ASTERISK_ARI",
        ProviderName = "ASTERISK_ARI",
    };

    private static SchedulerDispatchLease Lease() => new(
        "JOB-LAB-1",
        "ATTEMPT-LAB-1",
        1,
        Now,
        Now.AddMinutes(5),
        "SIM-ASTERISK-001",
        Guid.NewGuid(),
        1,
        Now.AddMinutes(2),
        "ASTERISK_ARI",
        "ASTERISK_ARI");

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class FixedResolver : IDialTokenResolver
    {
        public ValueTask<DialAuthorization> ResolveAsync(
            DialTokenResolutionRequest request,
            DateTimeOffset now,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(DialAuthorization.CreateTrusted("LAB-A"));
    }

    private sealed class DeniedDispatchGate : IDispatchGate
    {
        public int Calls { get; private set; }

        public Task<DispatchGateDecision> EvaluateAsync(
            string environment,
            string destinationReference,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(new DispatchGateDecision(false, "GLOBAL_KILL_SWITCH_ON"));
        }
    }

    private sealed class UnexpectedSpeechRenderer : ISpeechRenderer
    {
        public ValueTask<RenderedSpeech> RenderAsync(
            PrivacySafeOrderSummary summary,
            string scriptTemplateId,
            string scriptVersion,
            ExecutionMode executionMode,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Speech must not render before the gate allows dispatch.");
    }

    private sealed class UnexpectedSpeechSynthesisService : ISpeechSynthesisService
    {
        public Task<RenderedSpeech> SynthesizeAsync(
            RenderedSpeech renderedSpeech,
            PrivacySafeOrderSummary summary,
            string scriptTemplateId,
            string scriptVersion,
            ExecutionMode executionMode,
            DateTimeOffset confirmationWindowExpiresAt,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Speech must not synthesize before the gate allows dispatch.");
    }

    private sealed class CountingSimGateway : ISimGateway
    {
        public int Calls { get; private set; }

        public ValueTask<SimCallSession> DialAsync(
            SimDialRequest request,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("ARI must not be touched when the gate blocks.");
        }

        public ValueTask PlayAsync(
            SimCallSession session,
            RenderedSpeech speech,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("ARI must not be touched when the gate blocks.");
        }

        public ValueTask<SimDtmfCapture> CaptureDtmfAsync(
            SimCallSession session,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("ARI must not be touched when the gate blocks.");
        }

        public ValueTask<SimDispositionReport> GetDispositionAsync(
            SimCallSession session,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("ARI must not be touched when the gate blocks.");
        }

        public ValueTask HangupAsync(
            SimCallSession session,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("ARI must not be touched when the gate blocks.");
        }

        public ValueTask<SimGatewayHealth> CheckHealthAsync(
            string simChannelId,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("ARI must not be touched when the gate blocks.");
        }
    }

    private sealed class CapturingStore : ITelephonyDispatchStore
    {
        public string? FailureCode { get; private set; }

        public Task<TelephonyDispatchContext> LoadAsync(
            SchedulerDispatchLease lease,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new TelephonyDispatchContext(
                TaskId.Create("TASK-LAB-1"),
                DialTokenReference.Create("enc:lab-sha256:SAFE", Now.AddMinutes(5)),
                TestData.Summary(),
                "SCRIPT-ORDER-CONFIRM",
                "v1-test-approved"));

        public Task MarkActiveAsync(
            SchedulerDispatchLease lease,
            SimCallSession session,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("No call may become active when the gate blocks.");

        public Task CompleteAsync(
            SchedulerDispatchLease lease,
            SimCallSession session,
            SimDtmfCapture dtmf,
            SimDispositionReport disposition,
            TimeSpan cooldown,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("No call may complete when the gate blocks.");

        public Task FailAsync(
            SchedulerDispatchLease lease,
            SimCallSession? session,
            SimProviderDisposition disposition,
            string technicalErrorCode,
            bool channelHealthy,
            TimeSpan cooldown,
            CancellationToken cancellationToken = default)
        {
            FailureCode = technicalErrorCode;
            return Task.CompletedTask;
        }
    }
}

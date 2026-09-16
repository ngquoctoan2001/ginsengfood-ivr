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
    public async Task LabVaultFingerprintsTokenAndPinsTheAliasAndRefusesAReplay()
    {
        // The in-memory ledger here on purpose: this test is about the vault - fingerprinting, the
        // pinned alias, and that a replay is refused - not about where the count is kept. The
        // durable ledger enforcing the same rules across processes is IT-TOKEN-DURABLE-*.
        var vault = new LabDialTokenVault(
            Microsoft.Extensions.Options.Options.Create(Options()),
            new DialTokenResolveLedger());
        string fingerprint = vault.Protect(
            "ivr-confirmation-task-dial-token",
            "opaque-lab-token");
        var request = new DialTokenResolutionRequest(
            DialTokenReference.Create(fingerprint, Now.AddMinutes(5)),
            AttemptId.Create("attempt-lab-vault"),
            TaskId.Create("TASK-LAB-VAULT-1"),
            3);

        DialAuthorization authorization = await vault.ResolveAsync(
            request,
            Now,
            CancellationToken.None);

        Assert.StartsWith("enc:lab-sha256:", fingerprint, StringComparison.Ordinal);
        Assert.DoesNotContain("opaque-lab-token", fingerprint, StringComparison.Ordinal);
        Assert.Equal("LAB-A", authorization.RevealToTrustedGateway());

        // W-0199. The same attempt resolving twice is a replay. A second, genuine attempt on the
        // same token is allowed now - that is the change OD-V1-17 signed, and the lab has to
        // behave the way production will.
        DialTokenRefusedException replay =
            await Assert.ThrowsAsync<DialTokenRefusedException>(async () =>
                await vault.ResolveAsync(request, Now, CancellationToken.None));
        Assert.Equal(DialTokenRefusalCodes.AttemptReplay, replay.RefusalCode);

        DialAuthorization second = await vault.ResolveAsync(
            new DialTokenResolutionRequest(
                DialTokenReference.Create(fingerprint, Now.AddMinutes(5)),
                AttemptId.Create("attempt-lab-vault-2"),
                TaskId.Create("TASK-LAB-VAULT-1"),
                3),
            Now,
            CancellationToken.None);
        Assert.Equal("LAB-A", second.RevealToTrustedGateway());
    }

    /// <summary>
    /// PD-01.3 changed one of the three rules this pinned. Production used to be refused for being
    /// production; it is now a supported profile, so that assertion is gone and the one that
    /// replaced it is stricter about the thing the old rule was really protecting.
    /// <para>
    /// A pinned destination in production is refused outright rather than merely required to be
    /// non-phone-shaped. The number arrives per call from the vault, so any value configured here
    /// dials somewhere no token authorised - which is the failure the raw-number check was reaching
    /// for. Recording and the local-Asterisk rule are untouched: neither was ever about the lab.
    /// </para>
    /// </summary>
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
            failure.Contains("DestinationAlias", StringComparison.Ordinal));
        Assert.Contains(result.Failures, failure =>
            failure.Contains("recording", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Failures, failure =>
            failure.Contains("local Asterisk", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The other half of PD-01.3: a production profile that drops the pinned destination and keeps
    /// everything else is accepted. Without this the split would be asserted only by its refusals,
    /// and a validator that refuses every production configuration would pass those just as well.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-CONFIG-06")]
    public void ValidatorAcceptsAProductionProfileWithNoPinnedDestination()
    {
        AsteriskAriOptions configured = Options();
        configured.ExecutionMode = IvrOptions.ProductionRealExecutionMode;
        configured.DestinationAlias = string.Empty;

        ValidateOptionsResult result = new AsteriskAriOptionsValidator().Validate(null, configured);

        Assert.True(result.Succeeded);
    }

    /// <summary>
    /// The lab profile is unchanged by the split. Its alias, channel and adapter stay pinned, and
    /// the identifier problem is still reported once rather than once per field.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-CONFIG-07")]
    public void TheLabProfileStillPinsItsAliasAfterTheProductionSplit()
    {
        AsteriskAriOptions configured = Options();
        configured.DestinationAlias = "LAB-B";

        ValidateOptionsResult result = new AsteriskAriOptionsValidator().Validate(null, configured);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure =>
            failure.Contains("softphone profile is pinned", StringComparison.Ordinal));
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
        /// <summary>No operator cut in these scenarios; the gate tests never reach a call.</summary>
        public Task<CallTerminationRequest?> ReadTerminationAsync(
            SchedulerDispatchLease lease,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<CallTerminationRequest?>(null);

        public string? FailureCode { get; private set; }

        public Task<TelephonyDispatchContext> LoadAsync(
            SchedulerDispatchLease lease,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new TelephonyDispatchContext(
                TaskId.Create("TASK-LAB-1"),
                DialTokenReference.Create("enc:lab-sha256:SAFE", Now.AddMinutes(5)),
                TestData.Summary(),
                "SCRIPT-ORDER-CONFIRM",
                "v1-test-approved",
                3));

        public Task MarkActiveAsync(
            SchedulerDispatchLease lease,
            SimCallSession session,
            DispatchedVoice? voice = null,
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

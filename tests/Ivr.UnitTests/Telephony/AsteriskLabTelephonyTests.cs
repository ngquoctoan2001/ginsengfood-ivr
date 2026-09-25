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

    /// <summary>
    /// W-0359 / K-30 (B16) and K-29. The lab and production dial path records a render refusal as
    /// the order's fault, the way the MOCK path does (<c>IT-TEL-RENDER-DATA-09</c>), and records
    /// which kind of refusal it was.
    /// <para>
    /// The order's data refused by the speller is an audio-class failure with the channel healthy,
    /// through the arm every <c>TtsSynthesisException</c> takes. The approved script refusing the
    /// order stays a network-class refusal with the channel healthy - what the generic
    /// <c>InvalidOperationException</c> arm already did - but under its own code, so nobody on call
    /// goes looking for a dial-token fault that is not there. Neither reaches ARI: the render comes
    /// before the first ARI operation, so no call was placed and there is nothing to hang up.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-RENDER-DATA-10")]
    public async Task ARenderRefusalIsRecordedAsTheOrdersFaultUnderItsOwnCode()
    {
        var dataStore = new RecordingDispatchStore(DispatchContext());
        var dataSim = new CountingSimGateway();
        await Assert.ThrowsAsync<SpeechRenderRejectedException>(() => OpenGateway(
                dataStore,
                new RefusingSpeechRenderer(new SpeechRenderRejectedException(
                    new ArgumentException("The total amount is past the speller's range."))),
                dataSim)
            .DispatchAsync(Lease(), CancellationToken.None));

        RecordedDispatchFailure data = Assert.Single(dataStore.Failures);
        Assert.Equal(SimProviderDisposition.AudioError, data.Disposition);
        Assert.Equal("SPEECH_RENDER_DATA_REJECTED", data.TechnicalErrorCode);
        Assert.True(data.ChannelHealthy);
        Assert.Null(data.Session);
        Assert.Equal(0, dataSim.Calls);

        var policyStore = new RecordingDispatchStore(DispatchContext());
        var policySim = new CountingSimGateway();
        await Assert.ThrowsAsync<SpeechRenderPolicyRejectedException>(() => OpenGateway(
                policyStore,
                new RefusingSpeechRenderer(new SpeechRenderPolicyRejectedException(
                    new InvalidOperationException("The finished script failed the privacy guard."))),
                policySim)
            .DispatchAsync(Lease(), CancellationToken.None));

        RecordedDispatchFailure policy = Assert.Single(policyStore.Failures);
        Assert.Equal(SimProviderDisposition.NetworkError, policy.Disposition);
        Assert.Equal("SPEECH_RENDER_POLICY_REJECTED", policy.TechnicalErrorCode);
        Assert.True(policy.ChannelHealthy);
        Assert.Null(policy.Session);
        Assert.Equal(0, policySim.Calls);
    }

    /// <summary>
    /// W-0359 / K-31. An ARI hangup that fails is still swallowed - the dispatch ends in a failure
    /// either way, and that failure is what the attempt records - but no longer silently: a hangup
    /// that did not happen can leave the call up in Asterisk.
    /// <para>
    /// The same failing call is dispatched twice, once with a hangup that works and once with one
    /// that throws, and the two runs are compared. The attempt must be recorded identically, so the
    /// warning and the fail-closed count are the only difference the failed hangup makes. The
    /// warning names the exception's type and never its message, which is where the provider's own
    /// text would ride out.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-HANGUP-01")]
    public async Task AFailedAriHangupIsCountedAndLoggedWithoutChangingWhatTheAttemptRecords()
    {
        HangupScenarioRun baseline = await DispatchWithAriHangupAsync(hangupFailure: null);
        HangupScenarioRun failing = await DispatchWithAriHangupAsync(new HttpRequestException(
            "provider-detail: ARI answered 500 to DELETE /ari/channels/ari-channel-lab-hangup"));

        // The outcome is recorded exactly as it is when the hangup works.
        RecordedDispatchFailure expected = Assert.Single(baseline.Failures);
        RecordedDispatchFailure actual = Assert.Single(failing.Failures);
        Assert.Equal(SimProviderDisposition.AudioError, actual.Disposition);
        Assert.Equal("ASTERISK_PLAYBACK_FAILED", actual.TechnicalErrorCode);
        Assert.True(actual.ChannelHealthy);
        Assert.Equal(expected.Disposition, actual.Disposition);
        Assert.Equal(expected.TechnicalErrorCode, actual.TechnicalErrorCode);
        Assert.Equal(expected.ChannelHealthy, actual.ChannelHealthy);
        Assert.Equal(expected.Cooldown, actual.Cooldown);
        Assert.NotNull(actual.Session);
        Assert.Equal(expected.Session, actual.Session);

        // In both runs the call was up when the playback failed, and the gateway tried once to end
        // it: the failed hangup is not retried, and it is not skipped either.
        Assert.Equal(1, baseline.ActivatedCalls);
        Assert.Equal(1, failing.ActivatedCalls);
        Assert.Equal(1, baseline.HangupCalls);
        Assert.Equal(1, failing.HangupCalls);

        // A hangup that works says nothing...
        Assert.Empty(baseline.Warnings);
        Assert.Empty(baseline.FailClosed);

        // ...and one that fails says so once, by type, and is counted once. The reason code is
        // spelled out rather than read from the gateway: it is the value dashboards and alerts key
        // on, so renaming it has to fail here first.
        string warning = Assert.Single(failing.Warnings);
        Assert.Contains(nameof(HttpRequestException), warning, StringComparison.Ordinal);
        Assert.Contains("ATTEMPT-LAB-1", warning, StringComparison.Ordinal);
        Assert.Contains("ASTERISK_HANGUP_FAILED", warning, StringComparison.Ordinal);
        Assert.DoesNotContain("provider-detail", warning, StringComparison.Ordinal);
        Assert.Equal(1L, Assert.Single(failing.FailClosed));
    }

    /// <summary>
    /// The lab speaks through the VieNeu sidecar and nothing else: the profile resolves the
    /// loopback client, and its settings pass the same validator production uses.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-AUDIO-08")]
    public async Task TheLabProfileSpeaksOnlyThroughTheVieNeuSidecar()
    {
        IConfiguration configuration = Configuration();
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);
        services.AddIvrFeatureFlags(configuration);
        await using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        Assert.IsType<ConfigurableExternalTtsProvider>(provider.GetRequiredService<ITtsProvider>());
        TtsProviderOptions tts = provider.GetRequiredService<IOptions<TtsProviderOptions>>().Value;
        Assert.Equal(TtsProviderOptions.ExternalProvider, tts.Provider);
        Assert.True(new Uri(tts.External.Endpoint).IsLoopback);
        Assert.Equal("[REDACTED_ASTERISK_ARI_OPTIONS]", Options().ToString());
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
            // The same VieNeu sidecar settings docker-compose.vieneu-tts.yml gives the lab worker.
            [$"{TtsProviderOptions.SectionName}:Provider"] = TtsProviderOptions.ExternalProvider,
            [$"{TtsProviderOptions.SectionName}:External:Endpoint"] = "http://127.0.0.1:8090/synthesize",
            [$"{TtsProviderOptions.SectionName}:External:RequestBodyTemplate"] =
                """{"text":"{{text}}","voice_id":"{{voice_id}}","sample_rate":{{sample_rate}}}""",
            [$"{TtsProviderOptions.SectionName}:External:MediaOutputDirectory"] = "/var/lib/ivr/speech",
            [$"{TtsProviderOptions.SectionName}:External:MediaReferencePrefix"] = "sound:generated/",
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

    // ------------------------------------------------------------------------------- W-0359

    private static TelephonyDispatchContext DispatchContext() => new(
        TaskId.Create("TASK-LAB-1"),
        DialTokenReference.Create("enc:lab-sha256:SAFE", Now.AddMinutes(5)),
        TestData.Summary(),
        "SCRIPT-ORDER-CONFIRM",
        "v1-test-approved",
        3);

    /// <summary>
    /// The lab gateway with every gate open: the configuration <c>UT-AST-GATE-02</c> builds, plus a
    /// dispatch gate that allows, so a scenario reaches the step it is about.
    /// </summary>
    private static AsteriskSchedulerDispatchGateway OpenGateway(
        RecordingDispatchStore store,
        ISpeechRenderer renderer,
        ISimGateway sim,
        WarningCapturingLogger<AsteriskSchedulerDispatchGateway>? logger = null) => new(
            store,
            new FixedResolver(),
            renderer,
            new PassThroughSpeechSynthesisService(),
            sim,
            new AllowingDispatchGate(),
            Microsoft.Extensions.Options.Options.Create(Options()),
            Microsoft.Extensions.Options.Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.LabRealSimExecutionMode,
                SalesProvider = "FAKE_TARGET_V1",
                SimProvider = "VENDOR",
                RealCustomerCallAllowed = false,
            }),
            new SchedulerExecutionContext(IvrOptions.LabRealSimExecutionMode),
            new FixedTimeProvider(),
            logger);

    /// <summary>
    /// One lab dispatch that connects, fails at playback and then hangs up - or fails to, when
    /// <paramref name="hangupFailure"/> is given.
    /// </summary>
    private static async Task<HangupScenarioRun> DispatchWithAriHangupAsync(Exception? hangupFailure)
    {
        var store = new RecordingDispatchStore(DispatchContext());
        var sim = new ScriptedAriSimGateway(hangupFailure);
        var logger = new WarningCapturingLogger<AsteriskSchedulerDispatchGateway>();
        AsteriskSchedulerDispatchGateway gateway = OpenGateway(
            store,
            new FixedSpeechRenderer(),
            sim,
            logger);
        var failClosed = new List<long>();
        using (FailClosedMeasurements.Listen("ASTERISK_HANGUP_FAILED", failClosed))
        {
            AsteriskAriOperationException playback =
                await Assert.ThrowsAsync<AsteriskAriOperationException>(
                    () => gateway.DispatchAsync(Lease(), CancellationToken.None));
            Assert.Equal("ASTERISK_PLAYBACK_FAILED", playback.TechnicalErrorCode);
        }

        return new HangupScenarioRun(
            store.Failures,
            logger.Warnings,
            failClosed,
            sim.HangupCalls,
            store.ActivatedCalls);
    }

    private sealed class AllowingDispatchGate : IDispatchGate
    {
        public Task<DispatchGateDecision> EvaluateAsync(
            string environment,
            string destinationReference,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new DispatchGateDecision(true, "LAB_DESTINATION_APPROVED"));
    }

    /// <summary>Refuses every order with the exception it was given, as the approved renderer does.</summary>
    private sealed class RefusingSpeechRenderer(Exception refusal) : ISpeechRenderer
    {
        public ValueTask<RenderedSpeech> RenderAsync(
            PrivacySafeOrderSummary summary,
            string scriptTemplateId,
            string scriptVersion,
            ExecutionMode executionMode,
            CancellationToken cancellationToken) =>
            throw refusal;
    }

    /// <summary>Renders every order to the same short script, so a scenario gets past speech.</summary>
    private sealed class FixedSpeechRenderer : ISpeechRenderer
    {
        public ValueTask<RenderedSpeech> RenderAsync(
            PrivacySafeOrderSummary summary,
            string scriptTemplateId,
            string scriptVersion,
            ExecutionMode executionMode,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(new RenderedSpeech(
                "SCRIPT-ORDER-CONFIRM:v1-test-approved",
                "Xin chào Quý khách.",
                "sha256-lab-hangup-test",
                "vi-VN",
                TimeSpan.FromSeconds(2),
                0,
                "FAKE_TEXT_ONLY"));
    }

    /// <summary>
    /// An ARI line that is healthy, connects and refuses the playback - and then, when given an
    /// exception, refuses the hangup too. Nothing past the playback is meant to be reached.
    /// </summary>
    private sealed class ScriptedAriSimGateway(Exception? hangupFailure) : ISimGateway
    {
        public int HangupCalls { get; private set; }

        public ValueTask<SimGatewayHealth> CheckHealthAsync(
            string simChannelId,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(new SimGatewayHealth(
                simChannelId,
                SimChannelHealthState.Healthy,
                Now,
                null,
                true));

        public ValueTask<SimCallSession> DialAsync(
            SimDialRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            return ValueTask.FromResult(new SimCallSession(
                request.AttemptId,
                request.SimChannelId,
                "ari-channel-lab-hangup",
                request.FencingGeneration,
                Now,
                true));
        }

        public ValueTask PlayAsync(
            SimCallSession session,
            RenderedSpeech speech,
            CancellationToken cancellationToken) =>
            throw new AsteriskAriOperationException(
                SimProviderDisposition.AudioError,
                "ASTERISK_PLAYBACK_FAILED",
                true,
                "ARI refused the playback.");

        public ValueTask<SimDtmfCapture> CaptureDtmfAsync(
            SimCallSession session,
            TimeSpan timeout,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("A refused playback never reaches DTMF capture.");

        public ValueTask<SimDispositionReport> GetDispositionAsync(
            SimCallSession session,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("A refused playback never reaches a disposition.");

        public ValueTask HangupAsync(
            SimCallSession session,
            CancellationToken cancellationToken)
        {
            HangupCalls++;
            if (hangupFailure is not null)
            {
                throw hangupFailure;
            }

            return ValueTask.CompletedTask;
        }
    }
}

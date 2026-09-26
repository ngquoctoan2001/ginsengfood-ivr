using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Scripts;
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
    /// W-0362 / K-44. The gate is asked again immediately before the dial. Its first answer comes
    /// before speech is rendered and synthesised, which can take as long as the TTS timeout allows;
    /// an emergency stop thrown in that time has to stop this call, not only the next one.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-GATE-03")]
    public async Task AKillSwitchThrownWhileSpeechIsPreparedStopsTheDial()
    {
        var store = new RecordingDispatchStore(DispatchContext());
        var sim = new ScriptedAriSimGateway(hangupFailure: null);
        var gate = new AllowOnceDispatchGate();

        AsteriskAriOperationException refused =
            await Assert.ThrowsAsync<AsteriskAriOperationException>(() => OpenGateway(
                    store,
                    new FixedSpeechRenderer(),
                    sim,
                    gate: gate)
                .DispatchAsync(Lease(), CancellationToken.None));

        Assert.Equal("ASTERISK_GATE_GLOBAL_KILL_SWITCH_ON", refused.TechnicalErrorCode);
        Assert.Equal(2, gate.Calls);
        Assert.Equal(0, sim.DialCalls);
        Assert.Equal(0, store.ActivatedCalls);
        RecordedDispatchFailure failure = Assert.Single(store.Failures);
        Assert.Equal("ASTERISK_GATE_GLOBAL_KILL_SWITCH_ON", failure.TechnicalErrorCode);
        Assert.Equal(SimProviderDisposition.NetworkError, failure.Disposition);
        Assert.True(failure.ChannelHealthy);
        Assert.Null(failure.Session);
    }

    /// <summary>
    /// Q-28 (PA2, W-0365). Both questions the gateway puts to the dispatch gate - before speech is
    /// prepared and again right before the dial - carry the authorisation's gate reference, never
    /// the destination. In production the destination holds the customer's number and the gate
    /// reference is its pilot fingerprint, so this is what keeps the number out of the gate, its
    /// reasons and its logs.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-GATE-04")]
    public async Task TheGateIsAskedWithTheGateReferenceNeverTheDestination()
    {
        var store = new RecordingDispatchStore(DispatchContext());
        var sim = new ScriptedAriSimGateway(hangupFailure: null);
        var gate = new RecordingAllowOnceDispatchGate();

        await Assert.ThrowsAsync<AsteriskAriOperationException>(() => OpenGateway(
                store,
                new FixedSpeechRenderer(),
                sim,
                gate: gate,
                resolver: new TwoPartResolver())
            .DispatchAsync(Lease(), CancellationToken.None));

        Assert.Equal(new[] { TwoPartResolver.GateReference, TwoPartResolver.GateReference }, gate.References);
        Assert.Equal(0, sim.DialCalls);
    }

    /// <summary>
    /// W-0362 / K-42. The lab dials only what the lab approved. The seeded script is approved for
    /// MOCK and not for the lab; dispatched by the lab gateway through the real renderer and the
    /// real registry, it is refused before anything is dialled, because the gateway hands the
    /// renderer the deployment's mode and the registry answers for that mode.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-SCRIPT-01")]
    public async Task AScriptWithoutLabApprovalIsRefusedAtDispatchAndNothingIsDialled()
    {
        var clock = new FixedTimeProvider();
        using var scripts = new InMemoryScriptRegistry(
            new InMemoryAuditLogger(clock),
            clock,
            Microsoft.Extensions.Options.Options.Create(new ScriptContentOptions()));
        Assert.NotNull(await scripts.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock));
        Assert.Null(await scripts.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.LabRealSim));
        var renderer = new ApprovedVietnameseSpeechRenderer(
            scripts,
            new VietnameseOrderScriptRenderer(),
            new RegionalVoiceMap(Microsoft.Extensions.Options.Options.Create(new TtsProviderOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                Provider = TtsProviderOptions.FakeProvider,
            })));
        var store = new RecordingDispatchStore(new TelephonyDispatchContext(
            TaskId.Create("TASK-LAB-1"),
            DialTokenReference.Create("enc:lab-sha256:SAFE", Now.AddMinutes(5)),
            TestData.Summary(),
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            3));
        var sim = new ScriptedAriSimGateway(hangupFailure: null);

        await Assert.ThrowsAsync<SpeechRenderPolicyRejectedException>(() =>
            OpenGateway(store, renderer, sim).DispatchAsync(Lease(), CancellationToken.None));

        RecordedDispatchFailure failure = Assert.Single(store.Failures);
        Assert.Equal(SpeechRenderPolicyRejectedException.TechnicalCode, failure.TechnicalErrorCode);
        Assert.True(failure.ChannelHealthy);
        Assert.Null(failure.Session);
        Assert.Equal(0, sim.DialCalls);
    }

    /// <summary>
    /// W-0362 / K-43. A context that will not load - here a task revoked after the claim - ends the
    /// attempt through the failure path: recorded once, the channel handed back healthy, nothing
    /// dialled. The load used to sit outside the try, so the lease stood until it expired and lease
    /// recovery quarantined a SIM nobody had used.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-LOAD-FAIL-01")]
    public async Task AContextThatWillNotLoadIsRecordedAsAFailureAndNothingIsDialled()
    {
        var revoked = new InvalidOperationException("Task was revoked before dispatch.");
        var store = new RecordingDispatchStore(DispatchContext(), loadFailure: revoked);
        var sim = new ScriptedAriSimGateway(hangupFailure: null);

        DispatchContextUnavailableException failure =
            await Assert.ThrowsAsync<DispatchContextUnavailableException>(() =>
                OpenGateway(store, new FixedSpeechRenderer(), sim)
                    .DispatchAsync(Lease(), CancellationToken.None));

        Assert.Same(revoked, failure.InnerException);
        RecordedDispatchFailure recorded = Assert.Single(store.Failures);
        Assert.Equal("DISPATCH_CONTEXT_UNAVAILABLE", recorded.TechnicalErrorCode);
        Assert.Equal(SimProviderDisposition.NetworkError, recorded.Disposition);
        Assert.True(recorded.ChannelHealthy);
        Assert.Null(recorded.Session);
        Assert.Equal(0, sim.DialCalls);
    }

    /// <summary>
    /// W-0367 / K-57. The dispatch loop hands the store what the adapter said about the channel,
    /// unchanged, and whether the speech had started playing.
    /// <para>
    /// A stream lost before the playback says nothing about the SIM, so the store neither counts a
    /// failure against the channel nor clears its streak; before K-57 the same loss was recorded as
    /// the SIM's own fault. A failure after the playback started still counts against the channel
    /// when the adapter says so, and the raw event then records that the order was played.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-HEALTH-01")]
    public async Task TheDispatchLoopPassesOnWhatTheAdapterSaidAboutTheChannelAndThePlayback()
    {
        RecordedDispatchFailure lost = await DispatchThroughStagedAriAsync(
            playbackFailure: new AsteriskAriOperationException(
                SimProviderDisposition.NetworkError,
                "ASTERISK_EVENT_STREAM_LOST",
                null,
                "The ARI event stream was lost before playback; this side ended the call."),
            captureFailure: null);
        Assert.Equal(SimProviderDisposition.NetworkError, lost.Disposition);
        Assert.Equal("ASTERISK_EVENT_STREAM_LOST", lost.TechnicalErrorCode);
        Assert.Null(lost.ChannelHealthy);
        Assert.False(lost.PlaybackStarted);

        // W-0369 / K-62. A fault on the SIM's own route: the carrier network causes (34 to 44) the
        // adapter names ASTERISK_NETWORK_FAILURE and reports as the channel's. ASTERISK_HTTP_UNAVAILABLE
        // stood here until K-62 made an Asterisk out of reach say nothing about the SIM either.
        RecordedDispatchFailure afterPlayback = await DispatchThroughStagedAriAsync(
            playbackFailure: null,
            captureFailure: new AsteriskAriOperationException(
                SimProviderDisposition.NetworkError,
                "ASTERISK_NETWORK_FAILURE",
                false,
                "The SIM's route failed under the call."));
        Assert.Equal("ASTERISK_NETWORK_FAILURE", afterPlayback.TechnicalErrorCode);
        Assert.False(afterPlayback.ChannelHealthy);
        Assert.True(afterPlayback.PlaybackStarted);
    }

    /// <summary>
    /// W-0362 / K-47 (V6-6). Losing the ARI event stream ends every call still open on it, at once,
    /// as a network error that does not count against the customer.
    /// <para>
    /// Two calls are open when the fake Asterisk drops the TCP connection under the event stream:
    /// one answered and waiting for a key, one still ringing. Before the fix neither heard about
    /// it. Each waited out its own timeout (no input for the first, a ring timeout for the second)
    /// and each was recorded as the customer's doing: a counted attempt, and on the last attempt
    /// <c>IVR_NO_ANSWER_FINAL</c>, which is what M3 cancels a COD order on. Both timeouts here are
    /// far longer than the test is allowed to wait, so a call can only end in time if the lost
    /// stream ends it; and the normalization runs as the last attempt, where the stakes are.
    /// </para>
    /// <para>
    /// Only this side ended those calls. Asterisk may still hold the customer, or still be ringing
    /// them, so each hangup that follows has to reach it as a DELETE.
    /// </para>
    /// <para>
    /// Nor are they the SIM's doing. Since W-0367 / K-57 each report leaves the channel's health
    /// unsaid (null), so the store neither counts a failure against the SIM nor clears its streak:
    /// the stream belongs to the adapter and is shared by every SIM the worker drives.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-EVENTS-11")]
    public async Task LosingTheAriEventStreamEndsEveryOpenCallAtOnceAsAnUncountedNetworkError()
    {
        using var asterisk = new FakeAsterisk();
        AsteriskAriOptions configured = Options();
        configured.BaseUrl = asterisk.BaseUrl;
        configured.DialTimeoutSeconds = 600;
        await using var gateway = new AsteriskAriSimGateway(
            asterisk,
            Microsoft.Extensions.Options.Options.Create(configured),
            new FixedTimeProvider());

        SimCallSession answered = await gateway.DialAsync(
            DialRequest("attempt-lab-stream-answered"),
            CancellationToken.None);
        Assert.True(answered.IsConnected);
        Task<SimDtmfCapture> keypress = gateway
            .CaptureDtmfAsync(answered, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();

        asterisk.AnswerDials = false;
        Task<SimCallSession> ringing = gateway
            .DialAsync(DialRequest("attempt-lab-stream-ringing"), CancellationToken.None)
            .AsTask();
        await asterisk.Ringing.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(keypress.IsCompleted);

        await asterisk.DropEventStreamAsync();

        SimDtmfCapture capture = await keypress.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Null(capture.Key);
        Assert.False(capture.NoInput);
        Assert.Equal("ASTERISK_EVENT_STREAM_LOST", capture.TechnicalErrorCode);
        SimCallSession unanswered = await ringing.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(unanswered.IsConnected);

        foreach ((SimCallSession session, string? key) in new[]
                 {
                     (answered, capture.Key),
                     (unanswered, (string?)null),
                 })
        {
            // Exactly what the Asterisk dispatch path hands the store, and then the mapping the
            // store's disposition is normalized by: the place is_counted is decided.
            SimDispositionReport report = await gateway.GetDispositionAsync(
                session,
                CancellationToken.None);
            Assert.Equal(SimProviderDisposition.NetworkError, report.Disposition);
            Assert.Equal("ASTERISK_EVENT_STREAM_LOST", report.TechnicalErrorCode);
            Assert.Null(report.ChannelHealthy);

            NormalizedResult result = DispositionMapper.Normalize(
                report.Disposition,
                key,
                report.TechnicalErrorCode,
                LastAttempt());
            Assert.Equal(IvrResultType.IvrTechnicalException, result.ResultType);
            Assert.False(result.IsCounted);
            Assert.False(result.IsFinal);
            Assert.Equal("ASTERISK_EVENT_STREAM_LOST", result.TechnicalErrorCode);
        }

        await gateway.HangupAsync(answered, CancellationToken.None);
        await gateway.HangupAsync(unanswered, CancellationToken.None);
        IReadOnlyList<string> requests = asterisk.Requests;
        Assert.Single(requests, request => request == ChannelDelete(answered));
        Assert.Single(requests, request => request == ChannelDelete(unanswered));
    }

    /// <summary>
    /// W-0362 / K-47. Losing the event stream takes nothing from a call whose outcome is already
    /// known. A call that captured 1 stays a confirmation, and a call Asterisk already ended keeps
    /// the cause Asterisk gave; only the call still waiting is failed.
    /// <para>
    /// The waiting call is the witness: its capture ending proves the loss has been handled, so
    /// the other two are read after it rather than raced against it. The hangups follow who ended
    /// each call. The confirmed one is still up in Asterisk and gets its DELETE; the busy one,
    /// which Asterisk ended itself, gets none.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-EVENTS-12")]
    public async Task LosingTheAriEventStreamLeavesCallsWhoseOutcomeIsAlreadyKnownAlone()
    {
        using var asterisk = new FakeAsterisk();
        AsteriskAriOptions configured = Options();
        configured.BaseUrl = asterisk.BaseUrl;
        configured.DialTimeoutSeconds = 600;
        await using var gateway = new AsteriskAriSimGateway(
            asterisk,
            Microsoft.Extensions.Options.Options.Create(configured),
            new FixedTimeProvider());

        // A customer who answered and pressed 1.
        SimCallSession confirmed = await gateway.DialAsync(
            DialRequest("attempt-lab-stream-confirmed"),
            CancellationToken.None);
        await asterisk.SendEventAsync(new
        {
            type = "ChannelDtmfReceived",
            channel = new { id = confirmed.ProviderCallReference },
            digit = "1",
        });
        SimDtmfCapture pressed = await gateway
            .CaptureDtmfAsync(confirmed, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("1", pressed.Key);

        // A customer who answered and has not pressed anything yet.
        SimCallSession waiting = await gateway.DialAsync(
            DialRequest("attempt-lab-stream-waiting"),
            CancellationToken.None);
        Task<SimDtmfCapture> keypress = gateway
            .CaptureDtmfAsync(waiting, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();

        // A call Asterisk ended itself while it rang: the line was busy.
        asterisk.AnswerDials = false;
        Task<SimCallSession> dialling = gateway
            .DialAsync(DialRequest("attempt-lab-stream-busy"), CancellationToken.None)
            .AsTask();
        string busyChannel = await asterisk.Ringing.WaitAsync(TimeSpan.FromSeconds(10));
        await asterisk.SendEventAsync(new
        {
            type = "ChannelDestroyed",
            channel = new { id = busyChannel },
            cause = 17,
            cause_txt = "User busy",
        });
        SimCallSession busy = await dialling.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(busy.IsConnected);
        Assert.False(keypress.IsCompleted);

        await asterisk.DropEventStreamAsync();
        SimDtmfCapture lost = await keypress.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("ASTERISK_EVENT_STREAM_LOST", lost.TechnicalErrorCode);

        // The confirmation stands: answered, the channel healthy, no stream-loss code, and still a
        // confirmed order once normalized.
        SimDispositionReport confirmedReport = await gateway.GetDispositionAsync(
            confirmed,
            CancellationToken.None);
        Assert.Equal(SimProviderDisposition.Answered, confirmedReport.Disposition);
        Assert.True(confirmedReport.ChannelHealthy);
        Assert.Null(confirmedReport.TechnicalErrorCode);
        NormalizedResult confirmation = DispositionMapper.Normalize(
            confirmedReport.Disposition,
            pressed.Key,
            confirmedReport.TechnicalErrorCode,
            LastAttempt());
        Assert.Equal(IvrResultType.IvrConfirmed, confirmation.ResultType);
        Assert.Equal("1", confirmation.DtmfKey);

        // So does the cause Asterisk gave for the busy line.
        SimDispositionReport busyReport = await gateway.GetDispositionAsync(
            busy,
            CancellationToken.None);
        Assert.Equal(SimProviderDisposition.Busy, busyReport.Disposition);
        Assert.Null(busyReport.TechnicalErrorCode);
        Assert.True(busyReport.ChannelHealthy);

        await gateway.HangupAsync(confirmed, CancellationToken.None);
        await gateway.HangupAsync(busy, CancellationToken.None);
        IReadOnlyList<string> requests = asterisk.Requests;
        Assert.Single(requests, request => request == ChannelDelete(confirmed));
        Assert.DoesNotContain(ChannelDelete(busy), requests);
    }

    /// <summary>
    /// W-0365 / K-56. An ARI event holding a value of the wrong kind is ignored, and the stream goes
    /// on delivering. Each shape here used to throw out of the pump, past its JsonException catch:
    /// the pump stopped, and since K-47 every call open on it ended as a lost stream.
    /// <para>
    /// The first five cover every value read before an event can act on a call: an event that is
    /// not an object, a type that is not a string, a channel that is not an object, a channel id
    /// that is not a string, and a digit that is not a string. None of them may reach a call. The
    /// digit is the number 1, and the key the call captures in the end is the well-formed 0 sent
    /// after everything else, so a 1 taken from the malformed event would show up as the wrong key
    /// instead of passing for the right one.
    /// </para>
    /// <para>
    /// A hangup is never dropped for a malformed cause. Asterisk ends two calls, one with a cause
    /// text that is a number and one with a cause that is a string. Each call still ends, under
    /// what could be read of its cause, and as Asterisk's doing: no DELETE follows either.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-EVENTS-13")]
    public async Task MalformedAriEventsAreIgnoredWithoutStoppingTheStreamOrLosingAHangup()
    {
        using var asterisk = new FakeAsterisk();
        AsteriskAriOptions configured = Options();
        configured.BaseUrl = asterisk.BaseUrl;
        configured.DialTimeoutSeconds = 600;
        await using var gateway = new AsteriskAriSimGateway(
            asterisk,
            Microsoft.Extensions.Options.Options.Create(configured),
            new FixedTimeProvider());

        // Three answered calls: one waiting for a key, and two that Asterisk is about to end.
        SimCallSession waiting = await gateway.DialAsync(
            DialRequest("attempt-lab-malformed-waiting"),
            CancellationToken.None);
        SimCallSession cleared = await gateway.DialAsync(
            DialRequest("attempt-lab-malformed-cause-text"),
            CancellationToken.None);
        SimCallSession unexplained = await gateway.DialAsync(
            DialRequest("attempt-lab-malformed-cause"),
            CancellationToken.None);
        Task<SimDtmfCapture> keypress = gateway
            .CaptureDtmfAsync(waiting, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();
        Task<SimDtmfCapture> clearedCapture = gateway
            .CaptureDtmfAsync(cleared, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();
        Task<SimDtmfCapture> unexplainedCapture = gateway
            .CaptureDtmfAsync(unexplained, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();
        string id = waiting.ProviderCallReference;

        // The five shapes, each aimed at the waiting call as far as it can be...
        await asterisk.SendEventAsync(new object[] { "ChannelDestroyed", id });
        await asterisk.SendEventAsync(new { type = 5, channel = new { id } });
        await asterisk.SendEventAsync(new { type = "ChannelDestroyed", channel = id });
        await asterisk.SendEventAsync(new { type = "ChannelDestroyed", channel = new { id = 7 } });
        await asterisk.SendEventAsync(new { type = "ChannelDtmfReceived", channel = new { id }, digit = 1 });

        // ...the two hangups...
        await asterisk.SendEventAsync(new
        {
            type = "ChannelDestroyed",
            channel = new { id = cleared.ProviderCallReference },
            cause = 16,
            cause_txt = 16,
        });
        await asterisk.SendEventAsync(new
        {
            type = "ChannelDestroyed",
            channel = new { id = unexplained.ProviderCallReference },
            cause = "17",
            cause_txt = "User busy",
        });

        // ...and the key the customer pressed.
        await asterisk.SendEventAsync(new { type = "ChannelDtmfReceived", channel = new { id }, digit = "0" });

        // The key arrives, and it is the well-formed one.
        SimDtmfCapture pressed = await keypress.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("0", pressed.Key);
        Assert.Null(pressed.TechnicalErrorCode);

        // A cause text that is a number leaves the cause itself: normal clearing, the channel
        // healthy, no code.
        SimDtmfCapture clearedEnd = await clearedCapture.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Null(clearedEnd.Key);
        Assert.Null(clearedEnd.TechnicalErrorCode);
        SimDispositionReport clearedReport = await gateway.GetDispositionAsync(
            cleared,
            CancellationToken.None);
        Assert.Equal(SimProviderDisposition.Answered, clearedReport.Disposition);
        Assert.Null(clearedReport.TechnicalErrorCode);
        Assert.True(clearedReport.ChannelHealthy);

        // A cause that is a string leaves only the text: an Asterisk hangup of unknown cause.
        SimDtmfCapture unexplainedEnd = await unexplainedCapture.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("ASTERISK_UNKNOWN_HANGUP", unexplainedEnd.TechnicalErrorCode);
        SimDispositionReport unexplainedReport = await gateway.GetDispositionAsync(
            unexplained,
            CancellationToken.None);
        Assert.Equal(SimProviderDisposition.NetworkError, unexplainedReport.Disposition);
        Assert.Equal("ASTERISK_UNKNOWN_HANGUP", unexplainedReport.TechnicalErrorCode);

        // Nothing ended as a lost stream: the call that took the key is still up.
        SimDispositionReport waitingReport = await gateway.GetDispositionAsync(
            waiting,
            CancellationToken.None);
        Assert.Equal(SimProviderDisposition.Answered, waitingReport.Disposition);
        Assert.Null(waitingReport.TechnicalErrorCode);

        await gateway.HangupAsync(waiting, CancellationToken.None);
        await gateway.HangupAsync(cleared, CancellationToken.None);
        await gateway.HangupAsync(unexplained, CancellationToken.None);
        IReadOnlyList<string> requests = asterisk.Requests;
        Assert.Single(requests, request => request == ChannelDelete(waiting));
        Assert.DoesNotContain(ChannelDelete(cleared), requests);
        Assert.DoesNotContain(ChannelDelete(unexplained), requests);
    }

    /// <summary>
    /// W-0365 / K-56. A stream lost between the answer and the playback is reported as that loss, and
    /// not as a customer hanging up.
    /// <para>
    /// Two calls are answered and neither has played yet. Asterisk ends one itself; then the stream
    /// is lost under the other. The playback of the second used to be refused as Dropped,
    /// ASTERISK_CHANNEL_ALREADY_ENDED, with the channel healthy: a hangup nobody made, and not a word
    /// about the stream. It is now refused under the stream's own code, with the answer
    /// GetDispositionAsync gives for the same call, and normalizes to an uncounted technical
    /// exception that still names the loss. The call Asterisk ended keeps the old answer, which is
    /// true of it.
    /// </para>
    /// <para>
    /// Each capture is only a witness: it returns once the pump has handled what ended its call, so
    /// the playback is asked after that instead of raced against it. Neither playback reaches
    /// Asterisk, and the hangups follow who ended each call.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-EVENTS-14")]
    public async Task AStreamLostBeforePlaybackIsReportedAsTheLossAndNotAsAHangup()
    {
        using var asterisk = new FakeAsterisk();
        AsteriskAriOptions configured = Options();
        configured.BaseUrl = asterisk.BaseUrl;
        configured.DialTimeoutSeconds = 600;
        await using var gateway = new AsteriskAriSimGateway(
            asterisk,
            Microsoft.Extensions.Options.Options.Create(configured),
            new FixedTimeProvider());
        RenderedSpeech speech = PlayableSpeech();

        SimCallSession hungUp = await gateway.DialAsync(
            DialRequest("attempt-lab-play-hung-up"),
            CancellationToken.None);
        SimCallSession lost = await gateway.DialAsync(
            DialRequest("attempt-lab-play-lost"),
            CancellationToken.None);
        Assert.True(hungUp.IsConnected);
        Assert.True(lost.IsConnected);

        // Asterisk ends the first call...
        Task<SimDtmfCapture> hangup = gateway
            .CaptureDtmfAsync(hungUp, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();
        await asterisk.SendEventAsync(new
        {
            type = "ChannelDestroyed",
            channel = new { id = hungUp.ProviderCallReference },
            cause = 16,
            cause_txt = "Normal Clearing",
        });
        await hangup.WaitAsync(TimeSpan.FromSeconds(10));

        // ...and the stream is lost under the second.
        Task<SimDtmfCapture> loss = gateway
            .CaptureDtmfAsync(lost, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();
        await asterisk.DropEventStreamAsync();
        SimDtmfCapture lostCapture = await loss.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("ASTERISK_EVENT_STREAM_LOST", lostCapture.TechnicalErrorCode);

        AsteriskAriOperationException refused =
            await Assert.ThrowsAsync<AsteriskAriOperationException>(
                () => gateway.PlayAsync(lost, speech, CancellationToken.None).AsTask());
        Assert.Equal(SimProviderDisposition.NetworkError, refused.Disposition);
        Assert.Equal("ASTERISK_EVENT_STREAM_LOST", refused.TechnicalErrorCode);
        Assert.Contains("event stream", refused.Message, StringComparison.Ordinal);
        SimDispositionReport report = await gateway.GetDispositionAsync(
            lost,
            CancellationToken.None);
        Assert.Equal(report.Disposition, refused.Disposition);
        Assert.Equal(report.ChannelHealthy, refused.ChannelHealthy);

        // W-0367 / K-57. And, like the report, it says nothing about the SIM.
        Assert.Null(refused.ChannelHealthy);

        // What the store is handed for it normalizes, like every lost stream, to a technical
        // exception that is not counted and keeps the name of the loss.
        NormalizedResult result = DispositionMapper.Normalize(
            refused.Disposition,
            null,
            refused.TechnicalErrorCode,
            LastAttempt());
        Assert.Equal(IvrResultType.IvrTechnicalException, result.ResultType);
        Assert.False(result.IsCounted);
        Assert.Equal("ASTERISK_EVENT_STREAM_LOST", result.TechnicalErrorCode);

        // The call Asterisk ended is refused as before: that channel really has gone.
        AsteriskAriOperationException ended =
            await Assert.ThrowsAsync<AsteriskAriOperationException>(
                () => gateway.PlayAsync(hungUp, speech, CancellationToken.None).AsTask());
        Assert.Equal(SimProviderDisposition.Dropped, ended.Disposition);
        Assert.Equal("ASTERISK_CHANNEL_ALREADY_ENDED", ended.TechnicalErrorCode);
        Assert.True(ended.ChannelHealthy);

        // Neither playback reached Asterisk. The lost call may still hold the customer, so it is
        // hung up there; the one Asterisk ended is not.
        await gateway.HangupAsync(lost, CancellationToken.None);
        await gateway.HangupAsync(hungUp, CancellationToken.None);
        IReadOnlyList<string> requests = asterisk.Requests;
        Assert.DoesNotContain(requests, request => request.EndsWith("/play", StringComparison.Ordinal));
        Assert.Single(requests, request => request == ChannelDelete(lost));
        Assert.DoesNotContain(ChannelDelete(hungUp), requests);
    }

    /// <summary>
    /// W-0369 / K-58. Disposal stops waiting for a close Asterisk never answers.
    /// <para>
    /// The fake reads the gateway's close frame and sends nothing back: an Asterisk that has hung,
    /// or a proxy holding a connection whose far side is gone. Disposal used to wait for that answer
    /// without a bound, which held the worker's shutdown until the pod was killed - the way the
    /// first tests to dispose an open stream hung on this fake, until K-56 taught it to answer. It
    /// now gives up once the five seconds of EventStreamCloseTimeout have passed, having asked: the
    /// close frame did go out. The five seconds are spelled out rather than read from the gateway,
    /// because they are spent out of the worker's shutdown budget, so changing them has to fail
    /// here first.
    /// </para>
    /// <para>
    /// A call still open at that point ends the way the pump ends every open call when its stream is
    /// cut (K-47): as the stream's loss, an uncounted network error, and not a customer who never
    /// pressed a key.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-DISPOSE-01")]
    public async Task DisposalStopsWaitingForACloseAsteriskNeverAnswers()
    {
        using var asterisk = new FakeAsterisk { AnswerClose = false };
        AsteriskAriOptions configured = Options();
        configured.BaseUrl = asterisk.BaseUrl;
        configured.DialTimeoutSeconds = 600;
        await using var gateway = new AsteriskAriSimGateway(
            asterisk,
            Microsoft.Extensions.Options.Options.Create(configured),
            new FixedTimeProvider());
        SimCallSession open = await gateway.DialAsync(
            DialRequest("attempt-lab-dispose-silent"),
            CancellationToken.None);
        Task<SimDtmfCapture> keypress = gateway
            .CaptureDtmfAsync(open, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();

        var elapsed = Stopwatch.StartNew();
        await gateway.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(20));
        elapsed.Stop();

        await asterisk.CloseRequested.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.InRange(elapsed.Elapsed, TimeSpan.FromSeconds(4.5), TimeSpan.FromSeconds(15));

        SimDtmfCapture capture = await keypress.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Null(capture.Key);
        Assert.False(capture.NoInput);
        Assert.Equal("ASTERISK_EVENT_STREAM_LOST", capture.TechnicalErrorCode);
    }

    /// <summary>
    /// W-0369 / K-58. Disposal does not throw what ended the event pump.
    /// <para>
    /// The pump dies here the way K-56 lets it on purpose: an event whose type is a string that
    /// cannot be read. It is well-formed JSON in a well-formed text frame - a JSON escape for the
    /// first half of a surrogate pair, the second half missing - so it passes the WebSocket's UTF-8
    /// check and the parse, and throws only when the type is read: an InvalidOperationException,
    /// past the pump's JsonException catch. The pump ends the open call as a lost stream, the
    /// witness that it has died, and rethrows. Disposal swallowed only a WebSocketException, so it
    /// threw this one again, out of the container's teardown.
    /// </para>
    /// <para>
    /// The far end dropping the TCP connection does not reproduce it: the WebSocket reports that
    /// as a WebSocketException, which disposal always swallowed (UT-AST-EVENTS-11 disposes after
    /// exactly that). The other non-WebSocket ending, the OperationCanceledException of a socket
    /// aborted under a pending receive, is the one UT-AST-DISPOSE-01's abort leaves behind.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-DISPOSE-02")]
    public async Task DisposalDoesNotThrowWhatEndedTheEventPump()
    {
        using var asterisk = new FakeAsterisk();
        AsteriskAriOptions configured = Options();
        configured.BaseUrl = asterisk.BaseUrl;
        configured.DialTimeoutSeconds = 600;
        await using var gateway = new AsteriskAriSimGateway(
            asterisk,
            Microsoft.Extensions.Options.Options.Create(configured),
            new FixedTimeProvider());
        SimCallSession open = await gateway.DialAsync(
            DialRequest("attempt-lab-dispose-pump"),
            CancellationToken.None);
        Task<SimDtmfCapture> keypress = gateway
            .CaptureDtmfAsync(open, TimeSpan.FromMinutes(10), CancellationToken.None)
            .AsTask();

        await asterisk.SendRawEventAsync(string.Concat(
            "{\"type\":\"",
            JsonEscape,
            "uD800\",\"channel\":{\"id\":\"",
            open.ProviderCallReference,
            "\"}}"));
        SimDtmfCapture lost = await keypress.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("ASTERISK_EVENT_STREAM_LOST", lost.TechnicalErrorCode);

        Exception? thrown = await Record.ExceptionAsync(
            () => gateway.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(20)));
        Assert.Null(thrown);
    }

    /// <summary>
    /// W-0369 / K-62. An Asterisk out of reach fails the dial as a network error that says nothing
    /// about the SIM.
    /// <para>
    /// The dial meets it two ways. Nothing listens where Asterisk should be, so the event stream the
    /// dial opens first is refused: ASTERISK_EVENT_STREAM_UNAVAILABLE, and nothing was dialled. Or
    /// the stream is up and ARI's HTTP side refuses the originate - the fake throws what
    /// SocketsHttpHandler throws for a port nothing answers on: ASTERISK_HTTP_UNAVAILABLE. Both used
    /// to report the channel unhealthy, which put a strike on every SIM dialled while Asterisk was
    /// down, and three in ten minutes took each out of service with no way back.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-UNAVAILABLE-01")]
    public async Task AnAsteriskOutOfReachFailsTheDialWithoutBlamingTheSim()
    {
        AsteriskAriOptions down = Options();
        down.BaseUrl = UnreachableAsteriskUrl();
        await using (var gateway = new AsteriskAriSimGateway(
                         new SocketHttpClientFactory(),
                         Microsoft.Extensions.Options.Options.Create(down),
                         new FixedTimeProvider()))
        {
            AsteriskAriOperationException refused =
                await Assert.ThrowsAsync<AsteriskAriOperationException>(
                    () => gateway.DialAsync(
                        DialRequest("attempt-lab-asterisk-down"),
                        CancellationToken.None).AsTask());
            Assert.Equal(SimProviderDisposition.NetworkError, refused.Disposition);
            Assert.Equal("ASTERISK_EVENT_STREAM_UNAVAILABLE", refused.TechnicalErrorCode);
            Assert.Null(refused.ChannelHealthy);
        }

        using var asterisk = new FakeAsterisk { RefuseRest = true };
        AsteriskAriOptions configured = Options();
        configured.BaseUrl = asterisk.BaseUrl;
        await using var reachable = new AsteriskAriSimGateway(
            asterisk,
            Microsoft.Extensions.Options.Options.Create(configured),
            new FixedTimeProvider());
        AsteriskAriOperationException unanswered =
            await Assert.ThrowsAsync<AsteriskAriOperationException>(
                () => reachable.DialAsync(
                    DialRequest("attempt-lab-ari-http-down"),
                    CancellationToken.None).AsTask());
        Assert.Equal(SimProviderDisposition.NetworkError, unanswered.Disposition);
        Assert.Equal("ASTERISK_HTTP_UNAVAILABLE", unanswered.TechnicalErrorCode);
        Assert.Null(unanswered.ChannelHealthy);

        // The stream was up: what failed was the originate itself.
        Assert.Equal(["POST /ari/channels"], asterisk.Requests);
    }

    /// <summary>
    /// W-0369 / K-62. An Asterisk out of reach, met where a dispatch meets it first: the health check
    /// before the dial. Through the real ARI adapter, the real dispatch gateway and the real dispatch
    /// pump.
    /// <para>
    /// The adapter's health is a ping of Asterisk, and it answers for whichever channel it was asked
    /// about, so ASTERISK_CHANNEL_HEALTH_NOT_READY was where each SIM dialled while Asterisk was down
    /// took its strike - before the two codes of UT-AST-UNAVAILABLE-01 could be reached. The failure
    /// is now recorded without a word about the SIM, so the store leaves its streak alone
    /// (IT-TEL-HEALTH-NEUTRAL-10), and it still fails the dispatch, which is what makes the pump
    /// stop starting calls: the breaker the quarantined channel used to be by accident.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-AST-UNAVAILABLE-02")]
    public async Task AnAsteriskOutOfReachAtTheHealthCheckHoldsThePumpBackAndNotTheSim()
    {
        AsteriskAriOptions configured = Options();
        configured.BaseUrl = UnreachableAsteriskUrl();
        await using var ari = new AsteriskAriSimGateway(
            new SocketHttpClientFactory(),
            Microsoft.Extensions.Options.Options.Create(configured),
            new FixedTimeProvider());
        var store = new RecordingDispatchStore(DispatchContext());
        AsteriskSchedulerDispatchGateway gateway = OpenGateway(store, new FixedSpeechRenderer(), ari);
        var pump = new SchedulerDispatchPump(
            Microsoft.Extensions.Options.Options.Create(new SchedulerOptions()),
            new FixedTimeProvider());

        Assert.True(pump.TryReserve());
        pump.Start(Lease(), gateway.DispatchAsync);
        Assert.True(await pump.DrainAsync(TimeSpan.FromSeconds(30)));

        // The dispatch failed at the health check, before anything was dialled...
        SchedulerDispatchFailure failed = Assert.Single(pump.TakeFailures());
        AsteriskAriOperationException refused =
            Assert.IsType<AsteriskAriOperationException>(failed.Exception);
        Assert.Equal("ASTERISK_CHANNEL_HEALTH_NOT_READY", refused.TechnicalErrorCode);
        Assert.Equal(0, store.ActivatedCalls);

        // ...was recorded without a word about the SIM...
        RecordedDispatchFailure recorded = Assert.Single(store.Failures);
        Assert.Equal("ASTERISK_CHANNEL_HEALTH_NOT_READY", recorded.TechnicalErrorCode);
        Assert.Null(recorded.ChannelHealthy);
        Assert.Null(recorded.Session);

        // ...and it is the pump, not the channel, that holds the next call back.
        Assert.NotNull(pump.SheddingUntil);
        Assert.False(pump.TryReserve());
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

    /// <summary>Q-28. A destination that holds a number, and the fingerprint the gate may see instead.</summary>
    private sealed class TwoPartResolver : IDialTokenResolver
    {
        internal const string GateReference =
            "pilot:aaaabbbbccccddddeeeeffffgggghhhhiiiijjjjkkkkllllmmmmnnnnoooopppp";

        public ValueTask<DialAuthorization> ResolveAsync(
            DialTokenResolutionRequest request,
            DateTimeOffset now,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(DialAuthorization.CreateTrusted(
                "sip:+84900000001@sip.carrier.example.vn",
                GateReference));
    }

    /// <summary>Q-28. Allows the first question, refuses the second, and keeps what it was shown.</summary>
    private sealed class RecordingAllowOnceDispatchGate : IDispatchGate
    {
        public List<string> References { get; } = [];

        public Task<DispatchGateDecision> EvaluateAsync(
            string environment,
            string destinationReference,
            CancellationToken cancellationToken = default)
        {
            References.Add(destinationReference);
            return Task.FromResult(References.Count == 1
                ? new DispatchGateDecision(true, "PRODUCTION_PILOT_DESTINATION_APPROVED")
                : new DispatchGateDecision(false, "GLOBAL_KILL_SWITCH_ON"));
        }
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
            bool? channelHealthy,
            TimeSpan cooldown,
            bool playbackStarted = false,
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
        WarningCapturingLogger<AsteriskSchedulerDispatchGateway>? logger = null,
        IDispatchGate? gate = null,
        IDialTokenResolver? resolver = null) => new(
            store,
            resolver ?? new FixedResolver(),
            renderer,
            new PassThroughSpeechSynthesisService(),
            sim,
            gate ?? new AllowingDispatchGate(),
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

    /// <summary>
    /// W-0362 / K-44. Allows the first question and refuses every later one: the kill switch is
    /// thrown between the gateway's first look at the gate and the dial.
    /// </summary>
    private sealed class AllowOnceDispatchGate : IDispatchGate
    {
        public int Calls { get; private set; }

        public Task<DispatchGateDecision> EvaluateAsync(
            string environment,
            string destinationReference,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(Calls == 1
                ? new DispatchGateDecision(true, "LAB_DESTINATION_APPROVED")
                : new DispatchGateDecision(false, "GLOBAL_KILL_SWITCH_ON"));
        }
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

        /// <summary>How many calls were placed (W-0362).</summary>
        public int DialCalls { get; private set; }

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
            DialCalls++;
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

    /// <summary>
    /// W-0367 / K-57. One lab dispatch through <see cref="StagedAriSimGateway"/>: the call is answered
    /// and fails where the arguments say. Returns what the gateway told the store.
    /// </summary>
    private static async Task<RecordedDispatchFailure> DispatchThroughStagedAriAsync(
        Exception? playbackFailure,
        Exception? captureFailure)
    {
        var store = new RecordingDispatchStore(DispatchContext());
        var sim = new StagedAriSimGateway(playbackFailure, captureFailure);
        AsteriskSchedulerDispatchGateway gateway = OpenGateway(store, new FixedSpeechRenderer(), sim);
        await Assert.ThrowsAsync<AsteriskAriOperationException>(
            () => gateway.DispatchAsync(Lease(), CancellationToken.None));
        Assert.Equal(1, sim.HangupCalls);
        return Assert.Single(store.Failures);
    }

    /// <summary>
    /// W-0367 / K-57. An ARI line that answers, and then fails at the playback or, once the playback
    /// is done, at the key capture, with the exception it was given. Nothing past it is reached.
    /// </summary>
    private sealed class StagedAriSimGateway(Exception? playbackFailure, Exception? captureFailure)
        : ISimGateway
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
                "ari-channel-lab-staged",
                request.FencingGeneration,
                Now,
                true));
        }

        public ValueTask PlayAsync(
            SimCallSession session,
            RenderedSpeech speech,
            CancellationToken cancellationToken) =>
            playbackFailure is null ? ValueTask.CompletedTask : throw playbackFailure;

        public ValueTask<SimDtmfCapture> CaptureDtmfAsync(
            SimCallSession session,
            TimeSpan timeout,
            CancellationToken cancellationToken) =>
            throw captureFailure
                ?? new InvalidOperationException("Every staged call fails at the playback or the capture.");

        public ValueTask<SimDispositionReport> GetDispositionAsync(
            SimCallSession session,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("A staged call never reaches a disposition.");

        public ValueTask HangupAsync(
            SimCallSession session,
            CancellationToken cancellationToken)
        {
            HangupCalls++;
            return ValueTask.CompletedTask;
        }
    }

    // ------------------------------------------------------------------------------- W-0362

    /// <summary>A lab dial to the pinned alias, for the ARI adapter itself rather than a dispatch.</summary>
    private static SimDialRequest DialRequest(string attemptId) => new(
        AttemptId.Create(attemptId),
        TaskId.Create("TASK-LAB-1"),
        "SIM-ASTERISK-001",
        Guid.NewGuid(),
        1,
        DialAuthorization.CreateTrusted("LAB-A"),
        SimRecordingMode.Disabled);

    /// <summary>
    /// The customer's third and last attempt, inside the confirmation window: where a result counted
    /// against them would be final.
    /// </summary>
    private static AttemptNormalizationContext LastAttempt() =>
        new(3, 3, Now, Now.AddHours(1), 0, 2);

    /// <summary>The DELETE that hangs up <paramref name="session"/>'s channel, as the fake records it.</summary>
    private static string ChannelDelete(SimCallSession session) =>
        string.Concat("DELETE /ari/channels/", session.ProviderCallReference);

    /// <summary>
    /// W-0362 / K-47. A fake Asterisk for the ARI adapter: REST answered in process by this handler,
    /// the event stream over a real loopback socket. The socket is the point. K-47 is about that
    /// connection going away, and a ClientWebSocket can only lose a connection it really has.
    /// <para>
    /// A dial is answered with StasisStart on the event stream as soon as it is placed, unless
    /// <see cref="AnswerDials"/> is off: then it is left ringing and <see cref="Ringing"/>
    /// completes with its channel id. Every REST call succeeds and is kept, as method and path, in
    /// <see cref="Requests"/>. Events go out one at a time, as the tests send them. The adapter's
    /// close frame is answered, as Asterisk answers it (W-0365 / K-56).
    /// </para>
    /// <para>
    /// W-0369. Two failures on request: a close frame read and never answered
    /// (<see cref="AnswerClose"/> off, K-58), and ARI's HTTP side refusing every call while the event
    /// stream stays up (<see cref="RefuseRest"/>, K-62).
    /// </para>
    /// </summary>
    private sealed class FakeAsterisk : HttpMessageHandler, IHttpClientFactory
    {
        private const string KeyHeader = "Sec-WebSocket-Key:";

        // The fixed GUID RFC 6455 section 4.2.2 appends to the client's key.
        private const string HandshakeGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly TaskCompletionSource<string> ringing =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource closeRequested =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<string> requests = [];
        private readonly Task accepted;
        private TcpClient? connection;
        private WebSocket? events;

        public FakeAsterisk()
        {
            listener.Start();
            accepted = AcceptEventStreamAsync();
        }

        public bool AnswerDials { get; set; } = true;

        /// <summary>
        /// W-0369 / K-58. Off, the adapter's close frame is read and nothing goes back, and the
        /// connection is left open: an Asterisk that has hung, as disposal meets it.
        /// </summary>
        public bool AnswerClose { get; init; } = true;

        /// <summary>
        /// W-0369 / K-62. On, every REST call is refused the way SocketsHttpHandler reports a port
        /// nothing answers on, after it is recorded. The event stream is not affected.
        /// </summary>
        public bool RefuseRest { get; set; }

        /// <summary>The channel id of the dial left ringing, once there is one.</summary>
        public Task<string> Ringing => ringing.Task;

        /// <summary>Completes once the adapter's close frame has arrived, answered or not (K-58).</summary>
        public Task CloseRequested => closeRequested.Task;

        public IReadOnlyList<string> Requests
        {
            get
            {
                lock (requests)
                {
                    return [.. requests];
                }
            }
        }

        public string BaseUrl => string.Concat(
            "http://127.0.0.1:",
            ((IPEndPoint)listener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture));

        public HttpClient CreateClient(string name) => new(this, disposeHandler: false);

        /// <summary>Sends one ARI event down the event stream, as Asterisk would.</summary>
        public async Task SendEventAsync(object ariEvent)
        {
            await accepted;
            await events!.SendAsync(
                JsonSerializer.SerializeToUtf8Bytes(ariEvent),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        /// <summary>
        /// W-0369 / K-58. Sends one event exactly as written, for one no serializer would produce.
        /// </summary>
        public async Task SendRawEventAsync(string json)
        {
            await accepted;
            await events!.SendAsync(
                Encoding.UTF8.GetBytes(json),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        /// <summary>
        /// Drops the event stream's TCP connection with a reset and no WebSocket close frame: a
        /// network fault, or a proxy restarting, as the adapter sees one.
        /// </summary>
        public async Task DropEventStreamAsync()
        {
            await accepted;
            connection!.Client.LingerState = new LingerOption(true, 0);
            connection.Close();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Uri uri = request.RequestUri!;
            lock (requests)
            {
                requests.Add(string.Concat(request.Method.Method, " ", uri.AbsolutePath));
            }

            if (RefuseRest)
            {
                throw new HttpRequestException(
                    HttpRequestError.ConnectionError,
                    "No connection could be made because the target machine actively refused it.",
                    new SocketException((int)SocketError.ConnectionRefused));
            }

            if (request.Method == HttpMethod.Post && uri.AbsolutePath == "/ari/channels")
            {
                string channelId = QueryValue(uri, "channelId");
                if (AnswerDials)
                {
                    await SendEventAsync(new { type = "StasisStart", channel = new { id = channelId } });
                }
                else
                {
                    ringing.TrySetResult(channelId);
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                events?.Dispose();
                connection?.Dispose();
                listener.Dispose();
            }

            base.Dispose(disposing);
        }

        private static string QueryValue(Uri uri, string name) => Uri.UnescapeDataString(uri.Query
            .TrimStart('?')
            .Split('&')
            .Select(pair => pair.Split('=', 2))
            .Single(pair => string.Equals(pair[0], name, StringComparison.Ordinal))[1]);

        /// <summary>
        /// Accepts the adapter's one event-stream connection and upgrades it by hand, since nothing
        /// in this project hosts a WebSocket server.
        /// </summary>
        private async Task AcceptEventStreamAsync()
        {
            connection = await listener.AcceptTcpClientAsync();
            NetworkStream stream = connection.GetStream();

            // The upgrade request, up to the blank line that ends it. The client sends nothing
            // more until it has an answer, so this cannot read past it.
            var request = new StringBuilder();
            var chunk = new byte[1024];
            while (!request.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                int read = await stream.ReadAsync(chunk);
                if (read == 0)
                {
                    throw new IOException("The adapter closed the event stream before upgrading it.");
                }

                request.Append(Encoding.ASCII.GetString(chunk, 0, read));
            }

            string key = request.ToString()
                .Split("\r\n")
                .Single(line => line.StartsWith(KeyHeader, StringComparison.OrdinalIgnoreCase))
                [KeyHeader.Length..]
                .Trim();

            // SHA-1 because the protocol fixes it for this one purpose: it proves the upgrade and
            // protects nothing.
#pragma warning disable CA5350 // RFC 6455 mandates SHA-1 for the handshake answer.
            string accept = Convert.ToBase64String(SHA1.HashData(
                Encoding.ASCII.GetBytes(string.Concat(key, HandshakeGuid))));
#pragma warning restore CA5350
            await stream.WriteAsync(Encoding.ASCII.GetBytes(string.Concat(
                "HTTP/1.1 101 Switching Protocols\r\n",
                "Upgrade: websocket\r\n",
                "Connection: Upgrade\r\n",
                string.Concat("Sec-WebSocket-Accept: ", accept, "\r\n\r\n"))));
            events = WebSocket.CreateFromStream(
                stream,
                new WebSocketCreationOptions { IsServer = true, KeepAliveInterval = TimeSpan.Zero });
            _ = AnswerCloseAsync(events, connection);
        }

        /// <summary>
        /// W-0365 / K-56. Answers the adapter's close frame and then closes the connection, as a
        /// server ends the close handshake (RFC 6455 section 7.1.1). A gateway disposed while its
        /// stream is still up sends that frame and waits for the answer, which used to never come;
        /// every test before K-56 lost its stream first and so never asked. With
        /// <see cref="AnswerClose"/> off it reads the frame and stops there (W-0369 / K-58).
        /// </summary>
        private async Task AnswerCloseAsync(WebSocket stream, TcpClient client)
        {
            var buffer = new byte[1024];
            try
            {
                while (stream.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult received = await stream.ReceiveAsync(
                        buffer,
                        CancellationToken.None);
                    if (received.MessageType == WebSocketMessageType.Close)
                    {
                        closeRequested.TrySetResult();
                        if (!AnswerClose)
                        {
                            return;
                        }

                        await stream.CloseOutputAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "shutdown",
                            CancellationToken.None);
                        client.Close();
                    }
                }
            }
            catch (Exception exception) when (exception
                is WebSocketException or IOException or ObjectDisposedException
                or OperationCanceledException)
            {
                // The stream was dropped, or the fake disposed: there is nothing left to answer.
            }
        }
    }

    // ------------------------------------------------------------------------------- W-0365

    /// <summary>
    /// Speech with one safe Asterisk sound reference, so a playback gets past its audio check and
    /// on to the call it is for.
    /// </summary>
    private static RenderedSpeech PlayableSpeech() => new RenderedSpeech(
            "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            "Xin chào Quý khách.",
            "sha256-lab-playback-test",
            "vi-VN",
            TimeSpan.FromSeconds(2),
            0,
            "FAKE_TEXT_ONLY")
        .WithAudio(RenderedAudio.Create(
            "audio/L16",
            8_000,
            TimeSpan.FromSeconds(2),
            "sound:ivr-fixed-greeting"));

    // ------------------------------------------------------------------------------- W-0369

    /// <summary>
    /// The character that opens a JSON escape, kept apart so an escape can be assembled in a test
    /// without being written out whole (K-58).
    /// </summary>
    private const string JsonEscape = "\\";

    /// <summary>
    /// K-62. An Asterisk that is down, as the adapter meets it: a loopback port taken and handed
    /// straight back, so nothing listens there and every connection is refused.
    /// </summary>
    private static string UnreachableAsteriskUrl()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return string.Concat("http://127.0.0.1:", port.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// K-62. Real HTTP clients, so an adapter pointed at <see cref="UnreachableAsteriskUrl"/> meets
    /// the refusal the network gives rather than one a fake stages.
    /// </summary>
    private sealed class SocketHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}

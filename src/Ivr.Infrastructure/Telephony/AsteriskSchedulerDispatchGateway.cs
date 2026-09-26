using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Dispatch orchestration for the isolated Asterisk softphone lab. The runtime
/// DispatchGate is evaluated before the first ARI operation.
/// <para>
/// W-0359 / K-31. The logger is optional for the reason <c>FeatureFlagPlatform</c>'s is: tests
/// construct this gateway by hand, and the container supplies one in every real host.
/// </para>
/// </summary>
public sealed partial class AsteriskSchedulerDispatchGateway(
    ITelephonyDispatchStore store,
    IDialTokenResolver dialTokenResolver,
    ISpeechRenderer speechRenderer,
    ISpeechSynthesisService speechSynthesisService,
    ISimGateway simGateway,
    IDispatchGate dispatchGate,
    IOptions<AsteriskAriOptions> ariOptions,
    IOptions<IvrOptions> ivrOptions,
    SchedulerExecutionContext executionContext,
    TimeProvider timeProvider,
    ILogger<AsteriskSchedulerDispatchGateway>? logger = null) : ISchedulerDispatchGateway
{
    /// <summary>
    /// The reason code a hangup that failed, and was swallowed, is counted under on
    /// <c>ivr_fail_closed_total</c> (W-0359 / K-31).
    /// <para>
    /// The existing fail-closed counter rather than a new meter, for the reason
    /// <c>FeatureFlagPlatform</c> gives: only <c>IvrTelemetry.ServiceName</c> is exported, so a
    /// private meter would count something nothing reads. The same code the ARI adapter raises for
    /// a refused hangup, so the counter and the adapter's own error name one fault one way.
    /// </para>
    /// </summary>
    public const string HangupFailedReason = "ASTERISK_HANGUP_FAILED";

    public bool IsReady
    {
        get
        {
            AsteriskAriOptions adapter = ariOptions.Value;
            IvrOptions runtime = ivrOptions.Value;
            return adapter.Enabled
                && !runtime.RealCustomerCallAllowed
                && !adapter.RecordingEnabled
                && string.Equals(
                    executionContext.ExecutionMode,
                    IvrOptions.LabRealSimExecutionMode,
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(runtime.SimProvider, "VENDOR", StringComparison.OrdinalIgnoreCase);
        }
    }

    public async Task DispatchAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        AsteriskAriOptions configured = ariOptions.Value;
        if (!IsReady
            || !string.Equals(lease.AdapterMode, configured.AdapterMode, StringComparison.Ordinal)
            || !string.Equals(lease.ProviderName, configured.ProviderName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Asterisk lab dispatch is not safely enabled.");
        }

        SimCallSession? session = null;
        bool hungUp = false;
        TimeSpan cooldown = TimeSpan.FromSeconds(configured.CooldownSeconds);
        try
        {
            // W-0362 / K-43. Inside the try, so a context that cannot be loaded ends the attempt
            // through FailAsync like every other refusal. Outside it, the exception left the lease
            // standing: the channel stayed RESERVED until the lease ran out, lease recovery then
            // quarantined it and counted a DT-04 failure against a SIM nobody had used, and the job
            // waited in HELD_LEASE_RECOVERY.
            TelephonyDispatchContext dispatch;
            try
            {
                dispatch = await store.LoadAsync(lease, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                throw new DispatchContextUnavailableException(exception);
            }

            DialAuthorization authorization = await dialTokenResolver.ResolveAsync(
                new DialTokenResolutionRequest(
                    dispatch.DialToken,
                    AttemptId.Create(lease.AttemptId),
                    dispatch.TaskId,
                    dispatch.MaxDialTokenResolves,
                    dispatch.DirectPhoneE164),
                timeProvider.GetUtcNow(),
                cancellationToken);
            string destination = authorization.RevealToTrustedGateway();
            DispatchGateDecision gate = await dispatchGate.EvaluateAsync(
                configured.Environment,
                destination,
                cancellationToken);
            if (!gate.Allowed)
            {
                throw new AsteriskAriOperationException(
                    SimProviderDisposition.NetworkError,
                    SafeGateCode(gate.Reason),
                    true,
                    "The runtime dispatch gate blocked the lab call.");
            }

            // W-0354 / B13. The deployment's own mode, not LAB_REAL_SIM: the production branch
            // (PD-01) dispatches through this gateway too, and both calls below decide what is
            // allowed by mode. IsReady still admits LAB_REAL_SIM only, so today this is the same
            // value; it stops being the same the day SIP-04 opens production, which is the point.
            ExecutionMode mode = executionContext.ToDomainMode();
            RenderedSpeech speech = await speechRenderer.RenderAsync(
                dispatch.SpeechSummary,
                dispatch.ScriptTemplateId,
                dispatch.ScriptVersion,
                mode,
                cancellationToken);
            speech = await speechSynthesisService.SynthesizeAsync(
                speech,
                dispatch.SpeechSummary,
                dispatch.ScriptTemplateId,
                dispatch.ScriptVersion,
                mode,
                lease.Deadline,
                cancellationToken);
            SimGatewayHealth health = await simGateway.CheckHealthAsync(
                lease.SimChannelId,
                cancellationToken);
            if (!health.RecordingDisabled)
            {
                throw new AsteriskAriOperationException(
                    SimProviderDisposition.SimError,
                    "ASTERISK_RECORDING_NOT_DISABLED",
                    false,
                    "ARI health did not confirm recording disabled.");
            }

            if (health.State != SimChannelHealthState.Healthy)
            {
                throw new AsteriskAriOperationException(
                    SimProviderDisposition.SimError,
                    "ASTERISK_CHANNEL_HEALTH_NOT_READY",
                    false,
                    "The Asterisk channel is not healthy.");
            }

            // W-0362 / K-44. The gate again, immediately before the dial. Its first answer was
            // given before speech was rendered and synthesised, which can take as long as the TTS
            // timeout allows; an emergency stop thrown in that time used to be read only by the
            // next call, and this one went out anyway.
            DispatchGateDecision beforeDial = await dispatchGate.EvaluateAsync(
                configured.Environment,
                destination,
                cancellationToken);
            if (!beforeDial.Allowed)
            {
                throw new AsteriskAriOperationException(
                    SimProviderDisposition.NetworkError,
                    SafeGateCode(beforeDial.Reason),
                    true,
                    "The runtime dispatch gate blocked the lab call just before the dial.");
            }

            session = await simGateway.DialAsync(
                new SimDialRequest(
                    AttemptId.Create(lease.AttemptId),
                    dispatch.TaskId,
                    lease.SimChannelId,
                    lease.LeaseToken,
                    lease.FencingGeneration,
                    authorization,
                    SimRecordingMode.Disabled),
                cancellationToken);
            // W-0113. The voice rides on the audio that was just produced, so what gets recorded
            // is the voice this attempt actually holds rather than one re-derived later.
            await store.MarkActiveAsync(
                lease,
                session,
                speech.Audio?.Voice,
                cancellationToken);
            SimDtmfCapture dtmf;
            if (session.IsConnected)
            {
                await simGateway.PlayAsync(session, speech, cancellationToken);
                dtmf = await CaptureDtmfOrTerminationAsync(
                    session,
                    lease,
                    TimeSpan.FromSeconds(configured.DtmfTimeoutSeconds),
                    cancellationToken);
            }
            else
            {
                dtmf = new SimDtmfCapture(null, false, null);
            }

            // Asked again after the capture returns. An ARI hangup ends the channel, which can
            // complete the capture normally, and without this the loop would record an operator
            // cut as a customer outcome.
            await EnsureNotTerminatedAsync(session, lease, cancellationToken);
            SimDispositionReport disposition = await simGateway.GetDispositionAsync(
                session,
                cancellationToken);
            await simGateway.HangupAsync(session, cancellationToken);
            hungUp = true;
            await store.CompleteAsync(
                lease,
                session,
                dtmf,
                disposition,
                cooldown,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await TryHangupAsync(session, hungUp, CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await TryHangupAsync(session, hungUp, CancellationToken.None);
            (SimProviderDisposition disposition, string technicalCode, bool channelHealthy) =
                exception switch
                {
                    TtsSynthesisException tts =>
                        (SimProviderDisposition.AudioError, tts.TechnicalErrorCode, true),
                    IvrFailureException failure =>
                        (SimProviderDisposition.AudioError, failure.ErrorCode, true),
                    CallTerminatedException =>
                        (SimProviderDisposition.Dropped,
                            CallTerminatedException.TechnicalCode,
                            // Healthy: an operator ended this call, and putting the channel into
                            // cooldown for a fault it did not have would take capacity away as a
                            // side effect of a safety control.
                            true),
                    AsteriskAriOperationException ari =>
                        (ari.Disposition, ari.TechnicalErrorCode, ari.ChannelHealthy),
                    KeyNotFoundException =>
                        (SimProviderDisposition.NetworkError, "ASTERISK_DEPENDENCY_NOT_FOUND", true),
                    UnauthorizedAccessException =>
                        (SimProviderDisposition.NetworkError, "ASTERISK_DESTINATION_NOT_ALLOWLISTED", true),
                    // W-0199. Ahead of the generic arm so the refusing rule survives into the
                    // technical error code rather than being flattened away.
                    DialTokenRefusedException refused =>
                        (SimProviderDisposition.NetworkError, refused.RefusalCode, true),
                    // W-0359 / K-29. The approved script refused this order: no version approved
                    // for the mode, a placeholder it cannot fill, past the length bound, or the
                    // full-text privacy guard. It is an InvalidOperationException, so without an
                    // arm of its own ahead of the generic one it was recorded as a policy-or-token
                    // rejection, and whoever was on call went looking for a dial-token fault. Same
                    // disposition and channel health as that arm; only the code is its own.
                    SpeechRenderPolicyRejectedException =>
                        (SimProviderDisposition.NetworkError,
                            SpeechRenderPolicyRejectedException.TechnicalCode,
                            true),
                    // W-0362 / K-43. Ahead of the generic arm for the same reason: nothing was
                    // dialled, and the code says the context did not load rather than that a
                    // token was refused.
                    DispatchContextUnavailableException =>
                        (SimProviderDisposition.NetworkError,
                            DispatchContextUnavailableException.TechnicalCode,
                            true),
                    InvalidOperationException =>
                        (SimProviderDisposition.NetworkError, "ASTERISK_POLICY_OR_TOKEN_REJECTED", true),
                    _ =>
                        (SimProviderDisposition.NetworkError, "ASTERISK_DISPATCH_TECHNICAL_FAILURE", false),
                };
            await store.FailAsync(
                lease,
                session,
                disposition,
                technicalCode,
                channelHealthy,
                cooldown,
                cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Waits for a keypress, but stops waiting if an operator asks for the call to be cut.
    /// See the note on the mock gateway's copy: the request crosses a process boundary through
    /// the database, so the loop polls rather than being signalled.
    /// </summary>
    private async Task<SimDtmfCapture> CaptureDtmfOrTerminationAsync(
        SimCallSession session,
        SchedulerDispatchLease lease,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        TimeSpan interval = TimeSpan.FromMilliseconds(
            Math.Max(200, ariOptions.Value.TerminationPollMilliseconds));
        Task<SimDtmfCapture> capture = simGateway
            .CaptureDtmfAsync(session, timeout, cancellationToken)
            .AsTask();
        while (true)
        {
            Task completed = await Task.WhenAny(
                capture,
                Task.Delay(interval, timeProvider, cancellationToken));
            if (completed == capture)
            {
                return await capture;
            }

            CallTerminationRequest? request = await store
                .ReadTerminationAsync(lease, cancellationToken);
            if (request is null)
            {
                continue;
            }

            await simGateway.HangupAsync(session, cancellationToken);
            try
            {
                await capture;
            }
            catch
            {
                // The capture failing because the channel just ended is the expected outcome.
            }

            throw new CallTerminatedException(request);
        }
    }

    private async Task EnsureNotTerminatedAsync(
        SimCallSession session,
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken)
    {
        CallTerminationRequest? request = await store
            .ReadTerminationAsync(lease, cancellationToken);
        if (request is not null)
        {
            await TryHangupAsync(session, false, cancellationToken);
            throw new CallTerminatedException(request);
        }
    }

    private async Task TryHangupAsync(
        SimCallSession? session,
        bool alreadyHungUp,
        CancellationToken cancellationToken)
    {
        if (session is null || alreadyHungUp)
        {
            return;
        }

        try
        {
            await simGateway.HangupAsync(session, cancellationToken);
        }
        catch (Exception exception)
        {
            // The fenced persistence path still holds or quarantines the channel.
            //
            // W-0359 / K-31. Still swallowed: the dispatch ends in a failure either way, and that
            // failure is the one the attempt records. But no longer silently - a hangup that did
            // not happen can leave the call up in Asterisk, and until now nothing said so.
            IvrTelemetry.RecordFailClosed((TelemetryTags.ReasonCode, HangupFailedReason));

            // The exception type, not the exception: an ARI failure message can carry the
            // provider's own text, and that has no business in a log line.
            if (logger is not null)
            {
                LogHangupFailed(
                    logger,
                    session.AttemptId.Value,
                    session.SimChannelId,
                    HangupFailedReason,
                    exception.GetType().Name);
            }
        }
    }

    private static string SafeGateCode(string reason)
    {
        string normalized = reason.Trim().ToUpperInvariant();
        if (normalized.Length > 80
            || normalized.Any(character =>
                !(char.IsAsciiLetterOrDigit(character) || character is '_' or '-')))
        {
            return "ASTERISK_DISPATCH_GATE_BLOCKED";
        }

        return string.Concat("ASTERISK_GATE_", normalized);
    }

    // W-0359 / K-31. ReasonCode repeats HangupFailedReason so the line can be found by the code the
    // counter carries: PiiSafeLogRecordProcessor exports only allowlisted attributes, and
    // ReasonCode and AttemptId are on that list.
    [LoggerMessage(
        EventId = 2420,
        Level = LogLevel.Warning,
        Message = "ARI hangup of attempt {AttemptId} on SIM channel {SimChannelId} failed and was "
            + "swallowed; the attempt is still recorded, but the call may still be up in "
            + "Asterisk. ReasonCode={ReasonCode} ExceptionType={ExceptionType}")]
    private static partial void LogHangupFailed(
        ILogger logger,
        string attemptId,
        string simChannelId,
        string reasonCode,
        string exceptionType);
}

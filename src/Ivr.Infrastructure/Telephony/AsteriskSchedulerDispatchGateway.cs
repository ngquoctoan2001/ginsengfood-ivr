using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Dispatch orchestration for the isolated Asterisk softphone lab. The runtime
/// DispatchGate is evaluated before the first ARI operation.
/// </summary>
public sealed class AsteriskSchedulerDispatchGateway(
    ITelephonyDispatchStore store,
    IDialTokenResolver dialTokenResolver,
    ISpeechRenderer speechRenderer,
    ISpeechSynthesisService speechSynthesisService,
    ISimGateway simGateway,
    IDispatchGate dispatchGate,
    IOptions<AsteriskAriOptions> ariOptions,
    IOptions<IvrOptions> ivrOptions,
    SchedulerExecutionContext executionContext,
    TimeProvider timeProvider) : ISchedulerDispatchGateway
{
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

        TelephonyDispatchContext dispatch = await store.LoadAsync(
            lease,
            cancellationToken).ConfigureAwait(false);
        SimCallSession? session = null;
        bool hungUp = false;
        TimeSpan cooldown = TimeSpan.FromSeconds(configured.CooldownSeconds);
        try
        {
            DialAuthorization authorization = await dialTokenResolver.ResolveAsync(
                new DialTokenResolutionRequest(
                    dispatch.DialToken,
                    AttemptId.Create(lease.AttemptId),
                    dispatch.TaskId,
                    dispatch.MaxDialTokenResolves),
                timeProvider.GetUtcNow(),
                cancellationToken).ConfigureAwait(false);
            string destination = authorization.RevealToTrustedGateway();
            DispatchGateDecision gate = await dispatchGate.EvaluateAsync(
                configured.Environment,
                destination,
                cancellationToken).ConfigureAwait(false);
            if (!gate.Allowed)
            {
                throw new AsteriskAriOperationException(
                    SimProviderDisposition.NetworkError,
                    SafeGateCode(gate.Reason),
                    true,
                    "The runtime dispatch gate blocked the lab call.");
            }

            RenderedSpeech speech = await speechRenderer.RenderAsync(
                dispatch.SpeechSummary,
                dispatch.ScriptTemplateId,
                dispatch.ScriptVersion,
                ExecutionMode.LabRealSim,
                cancellationToken).ConfigureAwait(false);
            speech = await speechSynthesisService.SynthesizeAsync(
                speech,
                dispatch.SpeechSummary,
                dispatch.ScriptTemplateId,
                dispatch.ScriptVersion,
                ExecutionMode.LabRealSim,
                lease.Deadline,
                cancellationToken).ConfigureAwait(false);
            SimGatewayHealth health = await simGateway.CheckHealthAsync(
                lease.SimChannelId,
                cancellationToken).ConfigureAwait(false);
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

            session = await simGateway.DialAsync(
                new SimDialRequest(
                    AttemptId.Create(lease.AttemptId),
                    dispatch.TaskId,
                    lease.SimChannelId,
                    lease.LeaseToken,
                    lease.FencingGeneration,
                    authorization,
                    SimRecordingMode.Disabled),
                cancellationToken).ConfigureAwait(false);
            // W-0113. The voice rides on the audio that was just produced, so what gets recorded
            // is the voice this attempt actually holds rather than one re-derived later.
            await store.MarkActiveAsync(
                lease,
                session,
                speech.Audio?.Voice,
                cancellationToken).ConfigureAwait(false);
            SimDtmfCapture dtmf;
            if (session.IsConnected)
            {
                await simGateway.PlayAsync(session, speech, cancellationToken).ConfigureAwait(false);
                dtmf = await CaptureDtmfOrTerminationAsync(
                    session,
                    lease,
                    TimeSpan.FromSeconds(configured.DtmfTimeoutSeconds),
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                dtmf = new SimDtmfCapture(null, false, null);
            }

            // Asked again after the capture returns. An ARI hangup ends the channel, which can
            // complete the capture normally, and without this the loop would record an operator
            // cut as a customer outcome.
            await EnsureNotTerminatedAsync(session, lease, cancellationToken).ConfigureAwait(false);
            SimDispositionReport disposition = await simGateway.GetDispositionAsync(
                session,
                cancellationToken).ConfigureAwait(false);
            await simGateway.HangupAsync(session, cancellationToken).ConfigureAwait(false);
            hungUp = true;
            await store.CompleteAsync(
                lease,
                session,
                dtmf,
                disposition,
                cooldown,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await TryHangupAsync(session, hungUp, CancellationToken.None).ConfigureAwait(false);
            throw;
        }
        catch (Exception exception)
        {
            await TryHangupAsync(session, hungUp, CancellationToken.None).ConfigureAwait(false);
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
                    // W-0198. Ahead of the generic arm so the refusing rule survives into the
                    // technical error code rather than being flattened away.
                    DialTokenRefusedException refused =>
                        (SimProviderDisposition.NetworkError, refused.RefusalCode, true),
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
                cancellationToken).ConfigureAwait(false);
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
                Task.Delay(interval, timeProvider, cancellationToken)).ConfigureAwait(false);
            if (completed == capture)
            {
                return await capture.ConfigureAwait(false);
            }

            CallTerminationRequest? request = await store
                .ReadTerminationAsync(lease, cancellationToken)
                .ConfigureAwait(false);
            if (request is null)
            {
                continue;
            }

            await simGateway.HangupAsync(session, cancellationToken).ConfigureAwait(false);
            try
            {
                await capture.ConfigureAwait(false);
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
            .ReadTerminationAsync(lease, cancellationToken)
            .ConfigureAwait(false);
        if (request is not null)
        {
            await TryHangupAsync(session, false, cancellationToken).ConfigureAwait(false);
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
            await simGateway.HangupAsync(session, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The fenced persistence path still holds or quarantines the channel.
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
}

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
                    AttemptId.Create(lease.AttemptId)),
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
            await store.MarkActiveAsync(lease, session, cancellationToken).ConfigureAwait(false);
            SimDtmfCapture dtmf;
            if (session.IsConnected)
            {
                await simGateway.PlayAsync(session, speech, cancellationToken).ConfigureAwait(false);
                dtmf = await simGateway.CaptureDtmfAsync(
                    session,
                    TimeSpan.FromSeconds(configured.DtmfTimeoutSeconds),
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                dtmf = new SimDtmfCapture(null, false, null);
            }

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
                    AsteriskAriOperationException ari =>
                        (ari.Disposition, ari.TechnicalErrorCode, ari.ChannelHealthy),
                    KeyNotFoundException =>
                        (SimProviderDisposition.NetworkError, "ASTERISK_DEPENDENCY_NOT_FOUND", true),
                    UnauthorizedAccessException =>
                        (SimProviderDisposition.NetworkError, "ASTERISK_DESTINATION_NOT_ALLOWLISTED", true),
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

using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

public sealed class MockSimScenarioOptions
{
    public string Disposition { get; set; } = string.Empty;

    public string? DtmfKey { get; set; }

    public string? TechnicalErrorCode { get; set; }

    public int DialDelayMilliseconds { get; set; }

    public int PlayDelayMilliseconds { get; set; }

    public int CaptureDelayMilliseconds { get; set; }
}

public sealed class MockTelephonyOptions
{
    public const string SectionName = "Ivr:Telephony:Mock";

    public bool Enabled { get; set; }

    public bool KillSwitchEngaged { get; set; } = true;

    public int DtmfTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// How often the dispatch loop asks whether an operator has requested a cut (W-0111).
    /// <para>
    /// This is the upper bound on how long a customer keeps hearing a call somebody has already
    /// decided to stop, so it is a safety number, not a tuning one. Floored at 200 ms in code so
    /// a misconfiguration cannot turn it into a busy loop against the database.
    /// </para>
    /// </summary>
    public int TerminationPollMilliseconds { get; set; } = 500;

    public int CooldownSeconds { get; set; } = 5;

    public Dictionary<string, string> TokenDestinations { get; set; } =
        new(StringComparer.Ordinal);

    public List<string> DestinationAllowlist { get; set; } = [];

    public Dictionary<string, MockSimScenarioOptions> Scenarios { get; set; } =
        new(StringComparer.Ordinal);
}

public sealed class MockTelephonyOptionsValidator : IValidateOptions<MockTelephonyOptions>
{
    public ValidateOptionsResult Validate(string? name, MockTelephonyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        List<string> failures = [];
        if (options.DtmfTimeoutSeconds is < 1 or > 120)
        {
            failures.Add("DtmfTimeoutSeconds must be between 1 and 120.");
        }

        if (options.CooldownSeconds is < 0 or > 3600)
        {
            failures.Add("CooldownSeconds must be between 0 and 3600.");
        }

        HashSet<string> allowlist = options.DestinationAllowlist.ToHashSet(StringComparer.Ordinal);
        foreach ((string token, string destination) in options.TokenDestinations)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(destination))
            {
                failures.Add("MOCK token mappings cannot contain empty values.");
                continue;
            }

            try
            {
                if (!string.Equals(token, "*", StringComparison.Ordinal))
                {
                    _ = DialTokenReference.Create(token, DateTimeOffset.MaxValue);
                }

                _ = DialAuthorization.CreateTrusted(destination);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                failures.Add("MOCK destinations must be opaque fake references, never raw phone data.");
            }

            if (!allowlist.Contains(destination))
            {
                failures.Add("Every MOCK destination must be explicitly allowlisted.");
            }
        }

        foreach ((string attemptId, MockSimScenarioOptions scenario) in options.Scenarios)
        {
            if (scenario is null)
            {
                failures.Add("MOCK scenarios cannot be null.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(attemptId)
                || !Enum.TryParse(
                    scenario.Disposition,
                    ignoreCase: true,
                    out SimProviderDisposition _))
            {
                failures.Add("Every MOCK scenario must name a valid provider disposition.");
            }

            if (scenario.DtmfKey is not null
                && scenario.DtmfKey is not ("0" or "1")
                && scenario.DtmfKey.Length != 1)
            {
                failures.Add("MOCK DTMF keys must be one character, 0, 1 or null.");
            }

            if (scenario.DialDelayMilliseconds is < 0 or > 120_000
                || scenario.PlayDelayMilliseconds is < 0 or > 120_000
                || scenario.CaptureDelayMilliseconds is < 0 or > 120_000)
            {
                failures.Add("MOCK scenario delays must be between 0 and 120000 milliseconds.");
            }

            if (!string.IsNullOrWhiteSpace(scenario.TechnicalErrorCode)
                && (scenario.TechnicalErrorCode.Length > 120
                    || scenario.TechnicalErrorCode.Any(character =>
                        !(char.IsAsciiLetterOrDigit(character) || character is '_' or '-'))))
            {
                failures.Add("MOCK technical error codes contain unsupported characters.");
            }
        }

        if (options.Enabled)
        {
            if (options.KillSwitchEngaged)
            {
                failures.Add("KillSwitchEngaged must be false before enabling MOCK telephony.");
            }

            if (options.TokenDestinations.Count == 0
                || options.DestinationAllowlist.Count == 0
                || options.Scenarios.Count == 0)
            {
                failures.Add(
                    "Enabled MOCK telephony requires token mappings, an allowlist and scenarios.");
            }

        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures.Distinct(StringComparer.Ordinal));
    }
}

public sealed class MockSchedulerDispatchGateway(
    ITelephonyDispatchStore store,
    IDialTokenResolver dialTokenResolver,
    ISpeechRenderer speechRenderer,
    ISpeechSynthesisService speechSynthesisService,
    ISimGateway simGateway,
    IOptions<MockTelephonyOptions> mockOptions,
    IOptions<IvrOptions> ivrOptions,
    SchedulerExecutionContext executionContext,
    TimeProvider timeProvider) : ISchedulerDispatchGateway
{
    public bool IsReady
    {
        get
        {
            MockTelephonyOptions options = mockOptions.Value;
            IvrOptions runtime = ivrOptions.Value;
            return options.Enabled
                && !options.KillSwitchEngaged
                && !runtime.RealCustomerCallAllowed
                && string.Equals(
                    executionContext.ExecutionMode,
                    IvrOptions.MockExecutionMode,
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(runtime.SimProvider, "MOCK", StringComparison.OrdinalIgnoreCase);
        }
    }

    public async Task DispatchAsync(
        SchedulerDispatchLease lease,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (!IsReady
            || !string.Equals(lease.AdapterMode, "MOCK", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(lease.ProviderName, "MOCK", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("MOCK telephony dispatch is not safely enabled.");
        }

        TelephonyDispatchContext dispatch = await store.LoadAsync(
            lease,
            cancellationToken).ConfigureAwait(false);
        SimCallSession? session = null;
        bool hungUp = false;
        TimeSpan cooldown = TimeSpan.FromSeconds(mockOptions.Value.CooldownSeconds);
        try
        {
            RenderedSpeech speech = await speechRenderer.RenderAsync(
                dispatch.SpeechSummary,
                dispatch.ScriptTemplateId,
                dispatch.ScriptVersion,
                ExecutionMode.Mock,
                cancellationToken).ConfigureAwait(false);
            speech = await speechSynthesisService.SynthesizeAsync(
                speech,
                dispatch.SpeechSummary,
                dispatch.ScriptTemplateId,
                dispatch.ScriptVersion,
                ExecutionMode.Mock,
                lease.Deadline,
                cancellationToken).ConfigureAwait(false);
            SimGatewayHealth health = await simGateway.CheckHealthAsync(
                lease.SimChannelId,
                cancellationToken).ConfigureAwait(false);
            if (!health.RecordingDisabled)
            {
                throw new MockSimOperationException(
                    SimProviderDisposition.SimError,
                    "MOCK_RECORDING_NOT_DISABLED",
                    false,
                    "The fake SIM health read-back did not confirm recording disabled.");
            }

            if (health.State != SimChannelHealthState.Healthy)
            {
                throw new MockSimOperationException(
                    SimProviderDisposition.SimError,
                    "MOCK_CHANNEL_HEALTH_NOT_READY",
                    false,
                    "The fake SIM channel is not healthy.");
            }

            DialAuthorization authorization = await dialTokenResolver.ResolveAsync(
                new DialTokenResolutionRequest(
                    dispatch.DialToken,
                    AttemptId.Create(lease.AttemptId),
                    dispatch.TaskId,
                    dispatch.MaxDialTokenResolves),
                timeProvider.GetUtcNow(),
                cancellationToken).ConfigureAwait(false);
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
                    TimeSpan.FromSeconds(mockOptions.Value.DtmfTimeoutSeconds),
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                dtmf = new SimDtmfCapture(null, false, null);
            }

            // Asked again after the capture returns, not only during it. A hangup issued by the
            // operator can complete the capture normally, and without this check the loop would
            // record an operator cut as a customer outcome — the customer pressed nothing, and
            // "pressed nothing" is a very different fact from "we stopped talking to them".
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
                            // The channel is healthy: an operator ended this call, so putting
                            // the channel into cooldown for a fault it did not have would take
                            // capacity away as a side effect of a safety control.
                            true),
                    MockSimOperationException mock =>
                        (mock.Disposition, mock.TechnicalErrorCode, mock.ChannelHealthy),
                    KeyNotFoundException =>
                        (SimProviderDisposition.NetworkError, "MOCK_DEPENDENCY_NOT_FOUND", true),
                    UnauthorizedAccessException =>
                        (SimProviderDisposition.NetworkError, "MOCK_DESTINATION_NOT_ALLOWLISTED", true),
                    // W-0198. Before the generic InvalidOperationException arm, so the rule that
                    // refused the token reaches the operator instead of being flattened into
                    // "policy or token rejected". OD-V1-05 asked that an over-limit resolve open a
                    // review, and a review starts with knowing which rule fired.
                    DialTokenRefusedException refused =>
                        (SimProviderDisposition.NetworkError, refused.RefusalCode, true),
                    InvalidOperationException =>
                        (SimProviderDisposition.NetworkError, "MOCK_POLICY_OR_TOKEN_REJECTED", true),
                    _ =>
                        (SimProviderDisposition.NetworkError, "MOCK_DISPATCH_TECHNICAL_FAILURE", false),
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
    /// <para>
    /// Polling rather than a signal: the request is written by <c>Ivr.Api</c> in another
    /// process. The interval bounds how long a customer keeps hearing a call somebody has
    /// already decided to stop, so it is deliberately short and deliberately configurable.
    /// </para>
    /// <para>
    /// The hangup comes first and the throw second. Ending the channel is what actually stops
    /// the customer hearing anything; unwinding the loop is only how the attempt gets recorded.
    /// </para>
    /// </summary>
    private async Task<SimDtmfCapture> CaptureDtmfOrTerminationAsync(
        SimCallSession session,
        SchedulerDispatchLease lease,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        TimeSpan interval = TimeSpan.FromMilliseconds(
            Math.Max(200, mockOptions.Value.TerminationPollMilliseconds));
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
                // Drained so the capture does not surface later as an unobserved fault. Its
                // answer is discarded on purpose: whatever the customer pressed after the
                // operator decided to stop is not an answer this call gets to record.
                await capture.ConfigureAwait(false);
            }
            catch
            {
                // The gateway failing because the channel just ended is the expected outcome.
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
}

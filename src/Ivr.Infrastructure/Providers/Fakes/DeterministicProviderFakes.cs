using System.Collections.Concurrent;
using System.Collections.Immutable;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;

namespace Ivr.Infrastructure.Providers.Fakes;

public sealed class FakeAttemptPolicyRegistry : IAttemptPolicyRegistry
{
    private readonly ImmutableDictionary<(string Version, IvrProgramCode Program), AttemptPolicySnapshot> _policies;

    public FakeAttemptPolicyRegistry(IEnumerable<AttemptPolicySnapshot> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);
        _policies = policies.ToImmutableDictionary(policy => (policy.Version.Value, policy.Program));
    }

    public ValueTask<AttemptPolicySnapshot> ResolveAsync(
        PolicyVersion version,
        IvrProgramCode program,
        ExecutionMode executionMode,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(version);
        if (!_policies.TryGetValue((version.Value, program), out AttemptPolicySnapshot? policy))
        {
            throw new KeyNotFoundException("Versioned attempt policy was not found.");
        }

        policy.EnsureEnvironmentAllowed(executionMode);
        return ValueTask.FromResult(policy);
    }
}

public sealed class FakeDialTokenResolver : IDialTokenResolver
{
    private readonly ImmutableDictionary<string, string> _destinations;
    private readonly ImmutableHashSet<string> _allowedDestinations;
    private readonly ConcurrentDictionary<(string Token, string AttemptId), byte> _consumed = new();

    public FakeDialTokenResolver(
        IReadOnlyDictionary<string, string> destinations,
        IEnumerable<string>? allowedDestinations = null)
    {
        ArgumentNullException.ThrowIfNull(destinations);
        _destinations = destinations.ToImmutableDictionary(StringComparer.Ordinal);
        _allowedDestinations = (allowedDestinations ?? destinations.Values)
            .ToImmutableHashSet(StringComparer.Ordinal);
    }

    public ValueTask<DialAuthorization> ResolveAsync(
        DialTokenResolutionRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        if (request.DialToken.ExpiresAt <= now)
        {
            throw new InvalidOperationException("Dial token has expired.");
        }

        string opaqueToken = request.DialToken.RevealToTrustedResolver();
        if (!_destinations.TryGetValue(opaqueToken, out string? destination)
            && !_destinations.TryGetValue("*", out destination))
        {
            throw new KeyNotFoundException("Dial token cannot be resolved.");
        }

        if (!_allowedDestinations.Contains(destination))
        {
            throw new UnauthorizedAccessException("Resolved fake destination is not allowlisted.");
        }

        if (!_consumed.TryAdd((opaqueToken, request.AttemptId.Value), 0))
        {
            throw new InvalidOperationException("Dial token was already resolved for this attempt.");
        }

        return ValueTask.FromResult(DialAuthorization.CreateTrusted(destination));
    }
}

public sealed class FakeSpeechRenderer : ISpeechRenderer
{
    private readonly ApprovedScript _approvedScript;
    private readonly IScriptPreviewRenderer _renderer;

    public FakeSpeechRenderer(
        ApprovedScript? approvedScript = null,
        IScriptPreviewRenderer? renderer = null)
    {
        _approvedScript = approvedScript ?? CreateCanonicalMockScript();
        _renderer = renderer ?? new VietnameseOrderScriptRenderer();
    }

    public ValueTask<RenderedSpeech> RenderAsync(
        PrivacySafeOrderSummary summary,
        string scriptTemplateId,
        string scriptVersion,
        ExecutionMode executionMode,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptTemplateId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptVersion);
        if (executionMode != ExecutionMode.Mock
            || _approvedScript.ExecutionMode != executionMode
            || !string.Equals(
                _approvedScript.Version.Key.TemplateId,
                scriptTemplateId,
                StringComparison.Ordinal)
            || !string.Equals(
                _approvedScript.Version.Key.Version,
                scriptVersion,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The requested script is not approved for MOCK execution.");
        }

        ScriptPreview preview = _renderer.Render(_approvedScript, summary);
        int collapsed = Math.Max(
            0,
            summary.Items.Length - ScriptRenderOptions.Default.MaximumSpokenItems);
        return ValueTask.FromResult(new RenderedSpeech(
            preview.ScriptReference,
            preview.ExactText,
            preview.ContentHash,
            summary.Locale,
            preview.EstimatedDuration,
            collapsed,
            "FAKE_TEXT_ONLY"));
    }

    private static ApprovedScript CreateCanonicalMockScript()
    {
        ScriptDraftDefinition definition = ScriptDraftDefinition.Create(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        DateTimeOffset createdAt = new(2026, 8, 12, 0, 0, 0, TimeSpan.Zero);
        return new ApprovedScript(
            new ScriptVersionSnapshot(
                definition.Key,
                ScriptLifecycleStatus.Approved,
                definition.TemplateText,
                definition.TemplateHash,
                TargetV1SpeechPolicy.AllowedInputFields,
                [
                    new ScriptApprovalSnapshot(
                        ScriptApprovalType.MockTest,
                        "fake-reviewer",
                        "Synthetic P2-4 MOCK approval",
                        "W-0021-FAKE-SPEECH",
                        createdAt),
                ],
                "fake-author",
                "Synthetic P2-4 MOCK fixture",
                createdAt,
                "fake-reviewer",
                "Synthetic P2-4 MOCK review",
                createdAt,
                null,
                null,
                null),
            ExecutionMode.Mock);
    }
}

public sealed record FakeSimScenario(
    SimProviderDisposition Disposition,
    string? DtmfKey = null,
    string? TechnicalErrorCode = null,
    TimeSpan DialDelay = default,
    TimeSpan PlayDelay = default,
    TimeSpan CaptureDelay = default);

public sealed class MockSimOperationException(
    SimProviderDisposition disposition,
    string technicalErrorCode,
    bool channelHealthy,
    string message) : InvalidOperationException(message)
{
    public SimProviderDisposition Disposition { get; } = disposition;

    public string TechnicalErrorCode { get; } = technicalErrorCode;

    public bool ChannelHealthy { get; } = channelHealthy;
}

public sealed class FakeSimGateway : ISimGateway
{
    private readonly ImmutableDictionary<string, FakeSimScenario> _scenarios;
    private readonly ImmutableDictionary<string, SimChannelHealthState> _healthByChannel;
    private readonly ConcurrentDictionary<string, SimCallSession> _activeChannels = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, FakeSimScenario> _activeScenarios = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, RenderedSpeech> _playedSpeech = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<SimProviderEvent> _events = new();
    private readonly TimeProvider _timeProvider;

    public FakeSimGateway(
        IReadOnlyDictionary<string, FakeSimScenario> scenariosByAttemptId,
        IReadOnlyDictionary<string, SimChannelHealthState>? healthByChannel = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(scenariosByAttemptId);
        _scenarios = scenariosByAttemptId.ToImmutableDictionary(StringComparer.Ordinal);
        _healthByChannel = (healthByChannel ?? new Dictionary<string, SimChannelHealthState>())
            .ToImmutableDictionary(StringComparer.Ordinal);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public IReadOnlyCollection<SimProviderEvent> Events => _events.ToArray();

    public IReadOnlyDictionary<string, RenderedSpeech> PlayedSpeech => _playedSpeech;

    public async ValueTask<SimCallSession> DialAsync(
        SimDialRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        if (request.RecordingMode != SimRecordingMode.Disabled)
        {
            throw new InvalidOperationException("Recording is locked off in the MOCK SIM adapter.");
        }

        _ = request.DialAuthorization.RevealToTrustedGateway();
        if (!_scenarios.TryGetValue(request.AttemptId.Value, out FakeSimScenario? scenario)
            && !_scenarios.TryGetValue(request.TaskId.Value, out scenario)
            && !_scenarios.TryGetValue("*", out scenario))
        {
            throw new KeyNotFoundException(
                "No deterministic SIM scenario is configured for the attempt or task.");
        }

        await DelayAsync(scenario.DialDelay, cancellationToken).ConfigureAwait(false);
        DateTimeOffset startedAt = _timeProvider.GetUtcNow();
        string providerReference = string.Concat("mock-call:", request.AttemptId.Value);
        var session = new SimCallSession(
            request.AttemptId,
            request.SimChannelId,
            providerReference,
            request.FencingGeneration,
            startedAt,
            scenario.Disposition is SimProviderDisposition.Answered
                or SimProviderDisposition.AudioError
                or SimProviderDisposition.DtmfError);
        if (!_activeChannels.TryAdd(request.SimChannelId, session))
        {
            throw new MockSimOperationException(
                SimProviderDisposition.SimError,
                "MOCK_CHANNEL_ALREADY_ACTIVE",
                false,
                "The fake SIM channel already has an active call.");
        }

        _activeScenarios[providerReference] = scenario;
        AppendEvent(SimProviderEventType.DialStarted, session, "DIAL_STARTED", null);
        return session;
    }

    public async ValueTask PlayAsync(
        SimCallSession session,
        RenderedSpeech speech,
        CancellationToken cancellationToken)
    {
        FakeSimScenario scenario = RequireActive(session);
        ArgumentNullException.ThrowIfNull(speech);
        await DelayAsync(scenario.PlayDelay, cancellationToken).ConfigureAwait(false);
        if (scenario.Disposition == SimProviderDisposition.AudioError)
        {
            throw new MockSimOperationException(
                scenario.Disposition,
                scenario.TechnicalErrorCode ?? "MOCK_AUDIO_ERROR",
                true,
                "The fake SIM adapter injected an audio error.");
        }

        _playedSpeech[session.ProviderCallReference] = speech;
        AppendEvent(
            SimProviderEventType.SpeechPlayed,
            session,
            "SPEECH_PLAYED",
            speech.ContentHash);
    }

    public async ValueTask<SimDtmfCapture> CaptureDtmfAsync(
        SimCallSession session,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
        FakeSimScenario scenario = RequireActive(session);
        await DelayAsync(scenario.CaptureDelay, cancellationToken).ConfigureAwait(false);
        if (scenario.Disposition == SimProviderDisposition.DtmfError)
        {
            throw new MockSimOperationException(
                scenario.Disposition,
                scenario.TechnicalErrorCode ?? "MOCK_DTMF_ERROR",
                true,
                "The fake SIM adapter injected a DTMF error.");
        }

        string? key = scenario.Disposition == SimProviderDisposition.Answered
            ? scenario.DtmfKey
            : null;
        bool noInput = scenario.Disposition == SimProviderDisposition.Answered
            && string.IsNullOrWhiteSpace(key);
        AppendEvent(
            SimProviderEventType.DtmfCaptured,
            session,
            noInput ? "NO_INPUT" : SanitizeDtmf(key),
            null);
        return new SimDtmfCapture(key, noInput, null);
    }

    public ValueTask<SimDispositionReport> GetDispositionAsync(
        SimCallSession session,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FakeSimScenario scenario = RequireActive(session);
        DateTimeOffset endedAt = _timeProvider.GetUtcNow();
        bool channelHealthy = scenario.Disposition is not (
            SimProviderDisposition.NetworkError or SimProviderDisposition.SimError);
        AppendEvent(
            SimProviderEventType.DispositionReported,
            session,
            scenario.Disposition.ToString().ToUpperInvariant(),
            null);
        return ValueTask.FromResult(new SimDispositionReport(
            scenario.Disposition,
            session.StartedAt,
            endedAt,
            scenario.TechnicalErrorCode,
            channelHealthy));
    }

    public ValueTask HangupAsync(
        SimCallSession session,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(session);
        _activeScenarios.TryRemove(session.ProviderCallReference, out _);
        _activeChannels.TryRemove(
            new KeyValuePair<string, SimCallSession>(session.SimChannelId, session));
        AppendEvent(SimProviderEventType.HangupCompleted, session, "HANGUP_COMPLETED", null);
        return ValueTask.CompletedTask;
    }

    public ValueTask<SimGatewayHealth> CheckHealthAsync(
        string simChannelId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(simChannelId);
        SimChannelHealthState state = _healthByChannel.GetValueOrDefault(
            simChannelId,
            SimChannelHealthState.Healthy);
        DateTimeOffset checkedAt = _timeProvider.GetUtcNow();
        var syntheticSession = new SimCallSession(
            AttemptId.Create("mock-health-check"),
            simChannelId,
            "mock-health",
            0,
            checkedAt,
            false);
        AppendEvent(
            SimProviderEventType.HealthChecked,
            syntheticSession,
            state.ToString().ToUpperInvariant(),
            null);
        return ValueTask.FromResult(new SimGatewayHealth(
            simChannelId,
            state,
            checkedAt,
            null,
            true));
    }

    private FakeSimScenario RequireActive(SimCallSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!_activeChannels.TryGetValue(session.SimChannelId, out SimCallSession? active)
            || active != session
            || !_activeScenarios.TryGetValue(
                session.ProviderCallReference,
                out FakeSimScenario? scenario))
        {
            throw new InvalidOperationException("The fake SIM call session is not active.");
        }

        return scenario;
    }

    private void AppendEvent(
        SimProviderEventType type,
        SimCallSession session,
        string? statusCode,
        string? contentHash) =>
        _events.Enqueue(new SimProviderEvent(
            type,
            session.AttemptId,
            session.SimChannelId,
            session.ProviderCallReference,
            _timeProvider.GetUtcNow(),
            statusCode,
            contentHash));

    private async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, _timeProvider, cancellationToken).ConfigureAwait(false);
        }
    }

    private static string? SanitizeDtmf(string? key) => key switch
    {
        null or "" => null,
        "0" => "0",
        "1" => "1",
        _ => "INVALID",
    };
}

public sealed class FakeOrderCoreCallbackClient : IOrderCoreCallbackClient
{
    private readonly ImmutableDictionary<string, CallbackAcknowledgement> _acknowledgements;

    public FakeOrderCoreCallbackClient(IEnumerable<CallbackAcknowledgement> acknowledgements)
    {
        ArgumentNullException.ThrowIfNull(acknowledgements);
        _acknowledgements = acknowledgements.ToImmutableDictionary(ack => ack.CallbackId.Value, StringComparer.Ordinal);
    }

    public ValueTask<CallbackAcknowledgement> SubmitAsync(
        CallResultSnapshot callback,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(callback);
        if (!_acknowledgements.TryGetValue(callback.CallbackId.Value, out CallbackAcknowledgement? acknowledgement))
        {
            throw new KeyNotFoundException("No deterministic callback ACK is configured.");
        }

        return ValueTask.FromResult(acknowledgement);
    }
}

public sealed class FakeServiceTokenProvider : IServiceTokenProvider
{
    private readonly ImmutableDictionary<string, ServiceAccessToken> _tokens;

    public FakeServiceTokenProvider(IReadOnlyDictionary<string, ServiceAccessToken> tokensByAudience)
    {
        ArgumentNullException.ThrowIfNull(tokensByAudience);
        _tokens = tokensByAudience.ToImmutableDictionary(StringComparer.Ordinal);
    }

    public ValueTask<ServiceAccessToken> GetAsync(
        string audience,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_tokens.TryGetValue(audience, out ServiceAccessToken? token))
        {
            throw new KeyNotFoundException("No deterministic service token is configured.");
        }

        return ValueTask.FromResult(token);
    }
}

public sealed class FakeSystemClock(DateTimeOffset utcNow) : ISystemClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}

public sealed class FakeIdentifierGenerator(IEnumerable<string> identifiers) : IIdentifierGenerator
{
    private readonly ConcurrentQueue<string> _identifiers = new(identifiers);

    public string NewIdentifier()
    {
        if (!_identifiers.TryDequeue(out string? identifier))
        {
            throw new InvalidOperationException("No deterministic identifier remains.");
        }

        return identifier;
    }
}

public sealed class InMemoryDomainAuditSink : IDomainAuditSink
{
    private readonly List<DomainAuditRecord> _records = [];

    public IReadOnlyList<DomainAuditRecord> Records => _records;

    public ValueTask AppendAsync(DomainAuditRecord record, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _records.Add(record);
        return ValueTask.CompletedTask;
    }
}

public sealed class InMemoryDomainEvidenceSink : IDomainEvidenceSink
{
    private readonly List<DomainEvidenceRecord> _records = [];

    public IReadOnlyList<DomainEvidenceRecord> Records => _records;

    public ValueTask AppendAsync(DomainEvidenceRecord record, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _records.Add(record);
        return ValueTask.CompletedTask;
    }
}

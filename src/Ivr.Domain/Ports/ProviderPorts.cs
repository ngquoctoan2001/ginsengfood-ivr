using System.Collections.Immutable;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Speech;

namespace Ivr.Domain.Ports;

public interface IAttemptPolicyRegistry
{
    public ValueTask<AttemptPolicySnapshot> ResolveAsync(
        PolicyVersion version,
        IvrProgramCode program,
        ExecutionMode executionMode,
        CancellationToken cancellationToken);
}

public sealed record DialAuthorization
{
    private DialAuthorization(string providerDestinationReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerDestinationReference);
        OpaqueReferenceGuard.EnsureNotRawPhone(providerDestinationReference);
        ProviderDestinationReference = providerDestinationReference;
    }

    internal string ProviderDestinationReference { get; }

    public static DialAuthorization CreateTrusted(string providerDestinationReference) =>
        new(providerDestinationReference);

    public string RevealToTrustedGateway() => ProviderDestinationReference;

    public override string ToString() => "[REDACTED_DIAL_AUTHORIZATION]";
}

public interface IDialTokenResolver
{
    public ValueTask<DialAuthorization> ResolveAsync(
        DialTokenResolutionRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}

/// <param name="TaskId">
/// W-0199 / <c>OD-V1-17</c>. The task this dial belongs to. The token is bound to it, so a token
/// turning up under a second task is refused rather than dialled.
/// </param>
/// <param name="MaxResolves">
/// W-0199 / <c>OD-V1-05</c>. How many times this token may ever be resolved:
/// <c>max_customer_attempts</c> plus the technical-retry limit. This is what replaced the
/// "one-use per attempt" language that five documents carried and no contract could support -
/// policy needs at least two customer dials and nothing anywhere can re-issue a token.
/// <para>
/// It travels with the request because it is a policy number, not a property of the vault, and a
/// request that leaves it at zero is refused rather than treated as unlimited.
/// </para>
/// </param>
public sealed record DialTokenResolutionRequest(
    DialTokenReference DialToken,
    AttemptId AttemptId,
    TaskId TaskId,
    int MaxResolves);

public sealed record RenderedSpeech
{
    public RenderedSpeech(
        string scriptReference,
        string exactText,
        string contentHash,
        string locale,
        TimeSpan estimatedDuration,
        int collapsedItemCount,
        string audioFormat,
        RenderedAudio? audio = null,
        ImmutableArray<SpeechSegment> segments = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(exactText);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentException.ThrowIfNullOrWhiteSpace(audioFormat);
        ArgumentOutOfRangeException.ThrowIfLessThan(estimatedDuration, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfNegative(collapsedItemCount);
        ScriptReference = scriptReference;
        ExactText = exactText;
        ContentHash = contentHash;
        Locale = locale;
        EstimatedDuration = estimatedDuration;
        CollapsedItemCount = collapsedItemCount;
        AudioFormat = audioFormat;
        Audio = audio;
        Segments = segments.IsDefault ? [] : segments;
    }

    public string ScriptReference { get; }

    /// <summary>
    /// Privacy-safe text held in memory for playback. Never log or persist it as evidence.
    /// </summary>
    public string ExactText { get; }

    public string ContentHash { get; }

    public string Locale { get; }

    public TimeSpan EstimatedDuration { get; }

    public int CollapsedItemCount { get; }

    public string AudioFormat { get; }

    public RenderedAudio? Audio { get; }

    /// <summary>
    /// Playback order split at the approved template's placeholder boundaries. Empty means the
    /// renderer produced no split, and the whole text is spoken as one piece.
    /// </summary>
    public ImmutableArray<SpeechSegment> Segments { get; }

    public RenderedSpeech WithAudio(RenderedAudio audio)
    {
        ArgumentNullException.ThrowIfNull(audio);
        return new RenderedSpeech(
            ScriptReference,
            ExactText,
            ContentHash,
            Locale,
            EstimatedDuration,
            CollapsedItemCount,
            audio.Format,
            audio,
            Segments);
    }

    public override string ToString() => "[REDACTED_RENDERED_SPEECH]";
}

public interface ISpeechRenderer
{
    public ValueTask<RenderedSpeech> RenderAsync(
        PrivacySafeOrderSummary summary,
        string scriptTemplateId,
        string scriptVersion,
        ExecutionMode executionMode,
        CancellationToken cancellationToken);
}

public enum SimRecordingMode
{
    Disabled,
    Enabled,
}

public sealed record SimDialRequest(
    AttemptId AttemptId,
    TaskId TaskId,
    string SimChannelId,
    Guid LeaseToken,
    long FencingGeneration,
    DialAuthorization DialAuthorization,
    SimRecordingMode RecordingMode);

public sealed record SimCallSession(
    AttemptId AttemptId,
    string SimChannelId,
    string ProviderCallReference,
    long FencingGeneration,
    DateTimeOffset StartedAt,
    bool IsConnected);

public enum SimProviderDisposition
{
    Answered,
    RingTimeout,
    Busy,
    Rejected,
    Unreachable,
    InvalidDestination,
    Dropped,
    NetworkError,
    SimError,
    AudioError,
    DtmfError,
}

public enum SimChannelHealthState
{
    Healthy,
    Degraded,
    Unavailable,
}

public enum SimProviderEventType
{
    DialStarted,
    SpeechPlayed,
    DtmfCaptured,
    DispositionReported,
    HangupCompleted,
    HealthChecked,
}

public sealed record SimDtmfCapture(
    string? Key,
    bool NoInput,
    string? TechnicalErrorCode);

public sealed record SimDispositionReport(
    SimProviderDisposition Disposition,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    string? TechnicalErrorCode,
    bool ChannelHealthy);

public sealed record SimGatewayHealth(
    string SimChannelId,
    SimChannelHealthState State,
    DateTimeOffset CheckedAt,
    DateTimeOffset? CooldownUntil,
    bool RecordingDisabled);

public sealed record SimProviderEvent(
    SimProviderEventType Type,
    AttemptId AttemptId,
    string SimChannelId,
    string ProviderCallReference,
    DateTimeOffset OccurredAt,
    string? StatusCode,
    string? ContentHash);

public interface ISimGateway
{
    public ValueTask<SimCallSession> DialAsync(
        SimDialRequest request,
        CancellationToken cancellationToken);

    public ValueTask PlayAsync(
        SimCallSession session,
        RenderedSpeech speech,
        CancellationToken cancellationToken);

    public ValueTask<SimDtmfCapture> CaptureDtmfAsync(
        SimCallSession session,
        TimeSpan timeout,
        CancellationToken cancellationToken);

    public ValueTask<SimDispositionReport> GetDispositionAsync(
        SimCallSession session,
        CancellationToken cancellationToken);

    public ValueTask HangupAsync(
        SimCallSession session,
        CancellationToken cancellationToken);

    public ValueTask<SimGatewayHealth> CheckHealthAsync(
        string simChannelId,
        CancellationToken cancellationToken);
}

public interface IOrderCoreCallbackClient
{
    public ValueTask<CallbackAcknowledgement> SubmitAsync(
        CallResultSnapshot callback,
        CancellationToken cancellationToken);
}

public sealed record ServiceAccessToken
{
    private ServiceAccessToken(string value, DateTimeOffset expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
        ExpiresAt = expiresAt;
    }

    internal string Value { get; }

    public DateTimeOffset ExpiresAt { get; }

    public static ServiceAccessToken CreateTrusted(string value, DateTimeOffset expiresAt) =>
        new(value, expiresAt);

    public string RevealToTrustedTransport() => Value;

    public override string ToString() => "[REDACTED_SERVICE_TOKEN]";
}

public interface IServiceTokenProvider
{
    public ValueTask<ServiceAccessToken> GetAsync(
        string audience,
        CancellationToken cancellationToken);
}

public interface ISystemClock
{
    public DateTimeOffset UtcNow { get; }
}

public interface IIdentifierGenerator
{
    public string NewIdentifier();
}

public sealed record DomainAuditRecord(
    AuditReference Reference,
    string EventType,
    DateTimeOffset OccurredAt,
    CorrelationId CorrelationId);

public interface IDomainAuditSink
{
    public ValueTask AppendAsync(DomainAuditRecord record, CancellationToken cancellationToken);
}

public sealed record DomainEvidenceRecord(
    EvidenceReference Reference,
    string EvidenceType,
    string SnapshotHash,
    DateTimeOffset OccurredAt);

public interface IDomainEvidenceSink
{
    public ValueTask AppendAsync(DomainEvidenceRecord record, CancellationToken cancellationToken);
}

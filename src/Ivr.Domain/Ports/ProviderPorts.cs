using Ivr.Domain.Confirmation;

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
        DialTokenReference dialToken,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}

public sealed record RenderedSpeech(string ScriptReference, string ContentHash);

public interface ISpeechRenderer
{
    public ValueTask<RenderedSpeech> RenderAsync(
        PrivacySafeOrderSummary summary,
        string scriptTemplateId,
        string scriptVersion,
        CancellationToken cancellationToken);
}

public enum SimCallOutcome
{
    DtmfConfirmed,
    DtmfCancelled,
    NoAnswer,
    TechnicalException,
}

public sealed record SimDialRequest(
    AttemptId AttemptId,
    TaskId TaskId,
    DialAuthorization DialAuthorization,
    RenderedSpeech Speech);

public sealed record SimDialResult(
    SimCallOutcome Outcome,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    string ProviderReference);

public interface ISimGateway
{
    public ValueTask<SimDialResult> DialAsync(
        SimDialRequest request,
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

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;

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

    public FakeDialTokenResolver(IReadOnlyDictionary<string, string> destinations)
    {
        ArgumentNullException.ThrowIfNull(destinations);
        _destinations = destinations.ToImmutableDictionary(StringComparer.Ordinal);
    }

    public ValueTask<DialAuthorization> ResolveAsync(
        DialTokenReference dialToken,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(dialToken);
        if (dialToken.ExpiresAt <= now)
        {
            throw new InvalidOperationException("Dial token has expired.");
        }

        if (!_destinations.TryGetValue(dialToken.RevealToTrustedResolver(), out string? destination))
        {
            throw new KeyNotFoundException("Dial token cannot be resolved.");
        }

        return ValueTask.FromResult(DialAuthorization.CreateTrusted(destination));
    }
}

public sealed class FakeSpeechRenderer : ISpeechRenderer
{
    public ValueTask<RenderedSpeech> RenderAsync(
        PrivacySafeOrderSummary summary,
        string scriptTemplateId,
        string scriptVersion,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptTemplateId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptVersion);
        string reference = $"mock-speech:{scriptTemplateId}:{scriptVersion}";
        return ValueTask.FromResult(new RenderedSpeech(reference, summary.ComputeHash()));
    }
}

public sealed class FakeSimGateway : ISimGateway
{
    private readonly ImmutableDictionary<string, SimDialResult> _results;

    public FakeSimGateway(IReadOnlyDictionary<string, SimDialResult> resultsByAttemptId)
    {
        ArgumentNullException.ThrowIfNull(resultsByAttemptId);
        _results = resultsByAttemptId.ToImmutableDictionary(StringComparer.Ordinal);
    }

    public ValueTask<SimDialResult> DialAsync(
        SimDialRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        _ = request.DialAuthorization.RevealToTrustedGateway();
        if (!_results.TryGetValue(request.AttemptId.Value, out SimDialResult? result))
        {
            throw new KeyNotFoundException("No deterministic SIM result is configured for the attempt.");
        }

        return ValueTask.FromResult(result);
    }
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

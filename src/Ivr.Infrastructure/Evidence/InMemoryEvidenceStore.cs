using System.Collections.Concurrent;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;

namespace Ivr.Infrastructure.Evidence;

public sealed class InMemoryEvidenceStore(TimeProvider timeProvider) : IEvidenceStore
{
    private readonly ConcurrentDictionary<string, EvidenceRecord> records =
        new(StringComparer.Ordinal);

    public IReadOnlyCollection<EvidenceRecord> Records => records.Values.ToArray();

    public Task<EvidenceRecord> AppendAsync(
        EvidenceWrite request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        Require(request.EvidenceRef, nameof(request.EvidenceRef));
        Require(request.Kind, nameof(request.Kind));
        Require(request.CorrelationId, nameof(request.CorrelationId));
        Require(request.PayloadRef, nameof(request.PayloadRef));

        PiiGuard.EnsureSafeText(request.EvidenceRef);
        PiiGuard.EnsureSafeText(request.Kind);
        PiiGuard.EnsureSafeText(request.CorrelationId);
        PiiGuard.EnsureSafeText(request.PayloadRef);
        EvidenceRecord record = new(
            request.EvidenceRef,
            request.Kind,
            request.CorrelationId,
            request.PayloadRef,
            timeProvider.GetUtcNow());

        if (!records.TryAdd(request.EvidenceRef, record))
        {
            throw IvrErrors.IdempotencyConflict();
        }

        return Task.FromResult(record);
    }

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw IvrErrors.MalformedRequest($"{field} is required.");
        }
    }
}

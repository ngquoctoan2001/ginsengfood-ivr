namespace Ivr.Infrastructure.Evidence;

public interface IEvidenceStore
{
    public Task<EvidenceRecord> AppendAsync(
        EvidenceWrite request,
        CancellationToken cancellationToken = default);
}

public sealed record EvidenceWrite(
    string EvidenceRef,
    string Kind,
    string CorrelationId,
    string PayloadRef);

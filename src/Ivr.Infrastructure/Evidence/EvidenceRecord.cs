namespace Ivr.Infrastructure.Evidence;

public sealed record EvidenceRecord(
    string EvidenceRef,
    string Kind,
    string CorrelationId,
    string PayloadRef,
    DateTimeOffset CreatedAt);

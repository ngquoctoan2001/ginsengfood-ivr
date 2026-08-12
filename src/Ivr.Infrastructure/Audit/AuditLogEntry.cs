namespace Ivr.Infrastructure.Audit;

public sealed record AuditLogEntry(
    Guid Id,
    string Actor,
    string Action,
    string EntityRef,
    string? Reason,
    string CorrelationId,
    DateTimeOffset CreatedAt,
    string DataJson);

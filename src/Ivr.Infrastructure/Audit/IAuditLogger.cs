namespace Ivr.Infrastructure.Audit;

public interface IAuditLogger
{
    public Task<AuditLogEntry> AppendAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default);
}

public sealed record AuditEvent(
    string Actor,
    string Action,
    string EntityRef,
    string? Reason,
    string CorrelationId,
    IReadOnlyDictionary<string, object?> Data);

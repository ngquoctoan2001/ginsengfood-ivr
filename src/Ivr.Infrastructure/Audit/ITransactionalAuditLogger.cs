using Ivr.Infrastructure.Persistence;

namespace Ivr.Infrastructure.Audit;

/// <summary>Writes an audit entry using the caller's PostgreSQL transaction.</summary>
public interface ITransactionalAuditLogger : IAuditLogger
{
    public Task<AuditLogEntry> AppendWithinTransactionAsync(
        AuditEvent auditEvent,
        IvrDbContext dbContext,
        CancellationToken cancellationToken = default);
}

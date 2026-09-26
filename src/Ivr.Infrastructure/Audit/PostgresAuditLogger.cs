using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Audit;

public sealed class PostgresAuditLogger(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : ITransactionalAuditLogger
{
    public async Task<AuditLogEntry> AppendAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        Validate(auditEvent);
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await AppendToContextAsync(auditEvent, dbContext, cancellationToken);
    }

    public Task<AuditLogEntry> AppendWithinTransactionAsync(
        AuditEvent auditEvent,
        IvrDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A caller-owned transaction is required for an atomic audit write.");
        }

        return AppendToContextAsync(auditEvent, dbContext, cancellationToken);
    }

    private async Task<AuditLogEntry> AppendToContextAsync(
        AuditEvent auditEvent,
        IvrDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        Validate(auditEvent);
        string dataJson = JsonSerializer.Serialize(auditEvent.Data);
        PiiGuard.EnsureSafeJsonText(dataJson);
        (string targetType, string targetId) = SplitTarget(auditEvent.EntityRef);
        DateTimeOffset createdAt = timeProvider.GetUtcNow();
        var entity = new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = auditEvent.Actor,
            ActorType = "service",
            Action = auditEvent.Action,
            TargetType = targetType,
            TargetId = targetId,
            Reason = auditEvent.Reason,
            CorrelationId = auditEvent.CorrelationId,
            DataJson = dataJson,
            CreatedAt = createdAt,
        };
        dbContext.AuditLog.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new AuditLogEntry(
            entity.AuditId,
            auditEvent.Actor,
            auditEvent.Action,
            auditEvent.EntityRef,
            auditEvent.Reason,
            auditEvent.CorrelationId,
            createdAt,
            dataJson);
    }

    internal static void Validate(AuditEvent auditEvent)
    {
        if (string.IsNullOrWhiteSpace(auditEvent.Actor)
            || string.IsNullOrWhiteSpace(auditEvent.Action)
            || string.IsNullOrWhiteSpace(auditEvent.EntityRef)
            || string.IsNullOrWhiteSpace(auditEvent.CorrelationId))
        {
            throw IvrErrors.MalformedRequest("Audit actor, action, target and correlation are required.");
        }

        if (auditEvent.Action.StartsWith("ADMIN_", StringComparison.Ordinal)
            && string.IsNullOrWhiteSpace(auditEvent.Reason))
        {
            throw IvrErrors.MalformedRequest("An admin audit event requires a reason.");
        }

        PiiGuard.EnsureSafeText(auditEvent.Actor);
        PiiGuard.EnsureSafeText(auditEvent.Action);
        PiiGuard.EnsureSafeText(auditEvent.EntityRef);
        PiiGuard.EnsureSafeText(auditEvent.Reason);
        PiiGuard.EnsureSafeText(auditEvent.CorrelationId);
        foreach (string field in auditEvent.Data.Keys)
        {
            PiiGuard.EnsureSafeField(field);
        }

        // W-0365 / K-55. The data as a reader decodes it, not only as serialized text: the default
        // encoder escapes '+' and every accented letter, which hid +84 numbers and address markers
        // from a check that read the text alone. Here, so a refused event fails before a context is
        // opened, and again on the exact text the row stores.
        PiiGuard.EnsureSafeJsonText(JsonSerializer.Serialize(auditEvent.Data));
    }

    internal static (string Type, string Id) SplitTarget(string entityRef)
    {
        int separator = entityRef.IndexOf(':', StringComparison.Ordinal);
        return separator > 0
            ? (entityRef[..separator], entityRef[(separator + 1)..])
            : ("entity", entityRef);
    }
}

using System.Collections.Concurrent;
using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;

namespace Ivr.Infrastructure.Audit;

public sealed class InMemoryAuditLogger(TimeProvider timeProvider) : IAuditLogger
{
    private readonly ConcurrentQueue<AuditLogEntry> entries = new();

    public IReadOnlyCollection<AuditLogEntry> Entries => entries.ToArray();

    public Task<AuditLogEntry> AppendAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        cancellationToken.ThrowIfCancellationRequested();

        Require(auditEvent.Actor, nameof(auditEvent.Actor));
        Require(auditEvent.Action, nameof(auditEvent.Action));
        Require(auditEvent.EntityRef, nameof(auditEvent.EntityRef));
        Require(auditEvent.CorrelationId, nameof(auditEvent.CorrelationId));

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

        string dataJson = JsonSerializer.Serialize(auditEvent.Data);
        PiiGuard.EnsureSafeText(dataJson);

        AuditLogEntry entry = new(
            Guid.NewGuid(),
            auditEvent.Actor,
            auditEvent.Action,
            auditEvent.EntityRef,
            auditEvent.Reason,
            auditEvent.CorrelationId,
            timeProvider.GetUtcNow(),
            dataJson);
        entries.Enqueue(entry);
        return Task.FromResult(entry);
    }

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw IvrErrors.MalformedRequest($"{field} is required.");
        }
    }
}

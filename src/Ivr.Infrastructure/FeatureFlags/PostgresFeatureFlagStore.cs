using System.Collections.Frozen;
using System.Data;
using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.FeatureFlags;

internal sealed class PostgresFeatureFlagStore(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    FeatureFlagPersistenceSession session,
    TimeProvider timeProvider) : IFeatureFlagStore
{
    public async Task<FeatureFlagSnapshot> ReadFreshAsync(
        string environment,
        CancellationToken cancellationToken = default)
    {
        IvrDbContext? active = session.Current;
        if (active is not null)
        {
            return await ReadFreshCoreAsync(active, environment, cancellationToken);
        }

        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        return await ReadFreshCoreAsync(dbContext, environment, cancellationToken);
    }

    public async Task<FeatureFlagSnapshot> ApplyAuditedAsync(
        FeatureFlagSnapshot expected,
        FeatureFlagSnapshot proposed,
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(proposed);
        ArgumentNullException.ThrowIfNull(auditEvent);
        if (expected.Environment != proposed.Environment
            || proposed.Revision != checked(expected.Revision + 1))
        {
            throw new IvrFailureException(
                IvrErrorCodes.VersionConflict,
                "Feature flag revision is invalid.");
        }

        IvrDbContext? active = session.Current;
        if (active is not null)
        {
            return await ApplyCoreAsync(
                active,
                expected,
                proposed,
                auditEvent,
                cancellationToken);
        }

        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        FeatureFlagSnapshot stored = await ApplyCoreAsync(
            dbContext,
            expected,
            proposed,
            auditEvent,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return stored;
    }

    private static async Task<FeatureFlagSnapshot> ReadFreshCoreAsync(
        IvrDbContext dbContext,
        string environment,
        CancellationToken cancellationToken)
    {
        List<FeatureFlagEntity> rows = await dbContext.FeatureFlags
            .AsNoTracking()
            .Where(entity => entity.Environment == environment)
            .ToListAsync(cancellationToken);
        return ToSnapshot(environment, rows);
    }

    private async Task<FeatureFlagSnapshot> ApplyCoreAsync(
        IvrDbContext dbContext,
        FeatureFlagSnapshot expected,
        FeatureFlagSnapshot proposed,
        AuditEvent auditEvent,
        CancellationToken cancellationToken)
    {
        PostgresAuditLogger.Validate(auditEvent);
        List<FeatureFlagEntity> rows = await dbContext.FeatureFlags
            .FromSqlInterpolated($$"""
                SELECT *
                FROM ivr_feature_flags
                WHERE env = {{expected.Environment}}
                FOR UPDATE
                """)
            .ToListAsync(cancellationToken);
        FeatureFlagSnapshot current = ToSnapshot(expected.Environment, rows);
        if (current.Revision != expected.Revision)
        {
            throw new IvrFailureException(
                IvrErrorCodes.VersionConflict,
                "Feature flags changed before this operation completed.");
        }

        DateTimeOffset updatedAt = timeProvider.GetUtcNow();
        Dictionary<string, FeatureFlagEntity> byKey = rows.ToDictionary(
            row => row.Key,
            StringComparer.Ordinal);
        Update(byKey, FeatureFlagKeys.ExecutionMode, proposed.ExecutionMode, true);
        Update(byKey, FeatureFlagKeys.SalesProvider, proposed.SalesProvider, true);
        Update(byKey, FeatureFlagKeys.SimProvider, proposed.SimProvider, true);
        Update(byKey, FeatureFlagKeys.AttemptPolicyVersion, proposed.AttemptPolicyVersion, true);
        Update(
            byKey,
            FeatureFlagKeys.RealCustomerCallAllowed,
            proposed.RealCustomerCallAllowed,
            proposed.RealCustomerCallAllowed);
        Update(
            byKey,
            FeatureFlagKeys.LabDestinationAllowlist,
            proposed.LabDestinationAllowlist.Order(StringComparer.Ordinal).ToArray(),
            proposed.LabDestinationAllowlist.Count > 0);
        Update(
            byKey,
            FeatureFlagKeys.GlobalDialKillSwitch,
            proposed.GlobalDialKillSwitch,
            proposed.GlobalDialKillSwitch);
        Update(
            byKey,
            FeatureFlagKeys.V1NotificationEnabled,
            proposed.V1NotificationEnabled,
            proposed.V1NotificationEnabled);
        Update(
            byKey,
            FeatureFlagKeys.RecordingEnabled,
            proposed.RecordingEnabled,
            proposed.RecordingEnabled);

        foreach (FeatureFlagEntity row in rows)
        {
            row.Revision = proposed.Revision;
            row.UpdatedBy = auditEvent.Actor;
            row.UpdatedAt = updatedAt;
            row.Reason = auditEvent.Reason ?? string.Empty;
        }

        string beforeJson = JsonSerializer.Serialize(expected);
        string afterJson = JsonSerializer.Serialize(proposed);
        string dataJson = JsonSerializer.Serialize(auditEvent.Data);
        PiiGuard.EnsureSafeText(beforeJson);
        PiiGuard.EnsureSafeText(afterJson);
        PiiGuard.EnsureSafeText(dataJson);
        (string targetType, string targetId) = PostgresAuditLogger.SplitTarget(
            auditEvent.EntityRef);
        dbContext.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = auditEvent.Actor,
            ActorType = "admin",
            Action = auditEvent.Action,
            TargetType = targetType,
            TargetId = targetId,
            Reason = auditEvent.Reason,
            BeforeStateJson = beforeJson,
            AfterStateJson = afterJson,
            CorrelationId = auditEvent.CorrelationId,
            DataJson = dataJson,
            CreatedAt = updatedAt,
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return proposed;
    }

    private static FeatureFlagSnapshot ToSnapshot(
        string environment,
        List<FeatureFlagEntity> rows)
    {
        if (rows.Count != FeatureFlagKeys.All.Count
            || rows.Select(row => row.Revision).Distinct().Count() != 1)
        {
            throw IvrErrors.OperationalBlocked("Feature flag state is incomplete.");
        }

        Dictionary<string, FeatureFlagEntity> byKey = rows.ToDictionary(
            row => row.Key,
            StringComparer.Ordinal);
        return new FeatureFlagSnapshot(
            environment,
            rows.First().Revision,
            Read<string>(byKey, FeatureFlagKeys.ExecutionMode),
            Read<string>(byKey, FeatureFlagKeys.SalesProvider),
            Read<string>(byKey, FeatureFlagKeys.SimProvider),
            Read<string>(byKey, FeatureFlagKeys.AttemptPolicyVersion),
            Read<bool>(byKey, FeatureFlagKeys.RealCustomerCallAllowed),
            Read<string[]>(byKey, FeatureFlagKeys.LabDestinationAllowlist)
                .ToFrozenSet(StringComparer.Ordinal),
            Read<bool>(byKey, FeatureFlagKeys.GlobalDialKillSwitch),
            Read<bool>(byKey, FeatureFlagKeys.V1NotificationEnabled),
            Read<bool>(byKey, FeatureFlagKeys.RecordingEnabled));
    }

    private static void Update<T>(
        Dictionary<string, FeatureFlagEntity> rows,
        string key,
        T value,
        bool enabled)
    {
        if (!rows.TryGetValue(key, out FeatureFlagEntity? row))
        {
            throw IvrErrors.OperationalBlocked("Feature flag state is incomplete.");
        }

        row.ValueJson = JsonSerializer.Serialize(value);
        row.Enabled = enabled;
    }

    private static T Read<T>(
        Dictionary<string, FeatureFlagEntity> rows,
        string key)
    {
        if (!rows.TryGetValue(key, out FeatureFlagEntity? row))
        {
            throw IvrErrors.OperationalBlocked("Feature flag state is incomplete.");
        }

        return JsonSerializer.Deserialize<T>(row.ValueJson)
            ?? throw IvrErrors.OperationalBlocked("Feature flag value is invalid.");
    }
}

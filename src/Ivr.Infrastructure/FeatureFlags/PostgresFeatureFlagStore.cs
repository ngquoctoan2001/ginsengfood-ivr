using System.Collections.Frozen;
using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.Infrastructure.FeatureFlags;

public sealed class PostgresFeatureFlagStore(IServiceScopeFactory scopeFactory) : IFeatureFlagStore
{
    public async Task<FeatureFlagSnapshot> ReadFreshAsync(
        string environment,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IvrDbContext dbContext = scope.ServiceProvider.GetRequiredService<IvrDbContext>();
        List<FeatureFlagEntity> rows = await dbContext.FeatureFlags
            .AsNoTracking()
            .Where(entity => entity.Environment == environment)
            .ToListAsync(cancellationToken);

        if (rows.Count != FeatureFlagKeys.All.Count)
        {
            throw IvrErrors.OperationalBlocked("Feature flag state is incomplete.");
        }

        Dictionary<string, FeatureFlagEntity> byKey = rows.ToDictionary(
            row => row.Key,
            StringComparer.Ordinal);
        return new FeatureFlagSnapshot(
            environment,
            rows.Max(row => row.UpdatedAt.ToUnixTimeMilliseconds()),
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

    public Task<FeatureFlagSnapshot> ApplyAuditedAsync(
        FeatureFlagSnapshot expected,
        FeatureFlagSnapshot proposed,
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default) =>
        throw IvrErrors.OperationalBlocked(
            "Runtime feature flag writes require P1-2 migration and owner-approved authorization.");

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

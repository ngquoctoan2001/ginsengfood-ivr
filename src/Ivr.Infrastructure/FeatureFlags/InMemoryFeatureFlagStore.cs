using Ivr.Domain.Errors;
using Ivr.Infrastructure.Audit;

namespace Ivr.Infrastructure.FeatureFlags;

public sealed class InMemoryFeatureFlagStore : IFeatureFlagStore, IDisposable
{
    private readonly IAuditLogger auditLogger;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Dictionary<string, FeatureFlagSnapshot> snapshots;
    private volatile bool available = true;

    public InMemoryFeatureFlagStore(IAuditLogger auditLogger)
        : this(auditLogger, FeatureFlagEnvironments.All.Select(FeatureFlagSnapshot.SafeDefault))
    {
    }

    public InMemoryFeatureFlagStore(
        IAuditLogger auditLogger,
        IEnumerable<FeatureFlagSnapshot> seeds)
    {
        ArgumentNullException.ThrowIfNull(auditLogger);
        ArgumentNullException.ThrowIfNull(seeds);
        this.auditLogger = auditLogger;
        snapshots = seeds.ToDictionary(seed => seed.Environment, StringComparer.Ordinal);
    }

    public bool Available
    {
        get => available;
        set => available = value;
    }

    public async Task<FeatureFlagSnapshot> ReadFreshAsync(
        string environment,
        CancellationToken cancellationToken = default)
    {
        EnsureAvailable();
        await gate.WaitAsync(cancellationToken);
        try
        {
            return snapshots.TryGetValue(environment, out FeatureFlagSnapshot? snapshot)
                ? snapshot
                : throw IvrErrors.NotFound("Feature flag environment was not found.");
        }
        finally
        {
            gate.Release();
        }
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
        EnsureAvailable();

        await gate.WaitAsync(cancellationToken);
        try
        {
            EnsureAvailable();
            if (!snapshots.TryGetValue(expected.Environment, out FeatureFlagSnapshot? current)
                || current.Revision != expected.Revision)
            {
                throw new IvrFailureException(
                    IvrErrorCodes.VersionConflict,
                    "Feature flags changed before this operation completed.");
            }

            await auditLogger.AppendAsync(auditEvent, cancellationToken);
            snapshots[expected.Environment] = proposed;
            return proposed;
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose()
    {
        gate.Dispose();
    }

    private void EnsureAvailable()
    {
        if (!Available)
        {
            throw IvrErrors.OperationalBlocked("Feature flag provider is unavailable.");
        }
    }
}

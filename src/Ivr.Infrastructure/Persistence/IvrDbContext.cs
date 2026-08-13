using Microsoft.EntityFrameworkCore;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scripts;

namespace Ivr.Infrastructure.Persistence;

/// <summary>
/// Owns the IVR PostgreSQL model. P1-2 creates the physical migration.
/// </summary>
public sealed class IvrDbContext(DbContextOptions<IvrDbContext> options) : DbContext(options)
{
    public DbSet<FeatureFlagEntity> FeatureFlags => Set<FeatureFlagEntity>();
    public DbSet<ConfirmationTaskEntity> ConfirmationTasks => Set<ConfirmationTaskEntity>();
    public DbSet<AttemptPolicyEntity> AttemptPolicies => Set<AttemptPolicyEntity>();
    public DbSet<CallJobEntity> CallJobs => Set<CallJobEntity>();
    public DbSet<CallAttemptEntity> CallAttempts => Set<CallAttemptEntity>();
    public DbSet<RawCallEventEntity> RawCallEvents => Set<RawCallEventEntity>();
    public DbSet<CallResultEntity> CallResults => Set<CallResultEntity>();
    public DbSet<ResultCallbackEntity> ResultCallbacks => Set<ResultCallbackEntity>();
    public DbSet<SimChannelEntity> SimChannels => Set<SimChannelEntity>();
    public DbSet<CapacityIncidentEntity> CapacityIncidents => Set<CapacityIncidentEntity>();
    public DbSet<TechnicalExceptionEntity> TechnicalExceptions => Set<TechnicalExceptionEntity>();
    public DbSet<AdminActionEntity> AdminActions => Set<AdminActionEntity>();
    public DbSet<EvidenceLinkEntity> EvidenceLinks => Set<EvidenceLinkEntity>();
    public DbSet<IdempotencyKeyEntity> IdempotencyKeys => Set<IdempotencyKeyEntity>();
    public DbSet<AuditLogEntity> AuditLog => Set<AuditLogEntity>();
    public DbSet<EvidenceEntity> Evidence => Set<EvidenceEntity>();
    public DbSet<ReviewItemEntity> ReviewItems => Set<ReviewItemEntity>();
    public DbSet<RetentionCheckpointEntity> RetentionCheckpoints => Set<RetentionCheckpointEntity>();
    public DbSet<ScriptVersionEntity> ScriptVersions => Set<ScriptVersionEntity>();
    public DbSet<ScriptApprovalEntity> ScriptApprovals => Set<ScriptApprovalEntity>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PersistenceInvariantValidator.Validate(ChangeTracker);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        PersistenceInvariantValidator.Validate(ChangeTracker);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfiguration(new FeatureFlagEntityConfiguration());
        PersistenceModelConfiguration.Apply(modelBuilder);
    }
}

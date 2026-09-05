using Microsoft.EntityFrameworkCore;
using Ivr.Infrastructure.Analytics;
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

    /// <summary>W-0195. Recorded authorisations to move a runtime gate (OD-V1-20).</summary>
    public DbSet<RuntimeGateApprovalEntity> RuntimeGateApprovals =>
        Set<RuntimeGateApprovalEntity>();
    public DbSet<ConfirmationTaskEntity> ConfirmationTasks => Set<ConfirmationTaskEntity>();
    public DbSet<AttemptPolicyEntity> AttemptPolicies => Set<AttemptPolicyEntity>();
    public DbSet<CallJobEntity> CallJobs => Set<CallJobEntity>();
    public DbSet<TaskIntakeOutboxEntity> TaskIntakeOutbox => Set<TaskIntakeOutboxEntity>();
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

    // W-0055 / P10-4. Derived, PII-free star schema in the `analytics` schema. Same context so
    // one migration keeps operational and derived schema in step; separate schema so a BI grant
    // can reach the facts without reaching a single operational table.
    public DbSet<AnalyticsFactCallOutcomeEntity> AnalyticsFacts =>
        Set<AnalyticsFactCallOutcomeEntity>();

    public DbSet<AnalyticsFactCallJobEntity> AnalyticsJobFacts =>
        Set<AnalyticsFactCallJobEntity>();

    public DbSet<AnalyticsDimProgramEntity> AnalyticsPrograms => Set<AnalyticsDimProgramEntity>();

    public DbSet<AnalyticsDimScriptVariantEntity> AnalyticsScriptVariants =>
        Set<AnalyticsDimScriptVariantEntity>();

    public DbSet<AnalyticsDimResultTypeEntity> AnalyticsResultTypes =>
        Set<AnalyticsDimResultTypeEntity>();

    public DbSet<AnalyticsKpiDailyEntity> AnalyticsKpiDaily => Set<AnalyticsKpiDailyEntity>();

    public DbSet<AnalyticsEtlCheckpointEntity> AnalyticsCheckpoints =>
        Set<AnalyticsEtlCheckpointEntity>();

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
        modelBuilder.ApplyConfiguration(new RuntimeGateApprovalEntityConfiguration());
        // Analytics first: PersistenceModelConfiguration ends with the storage conventions
        // pass that snake-cases every column in the model, and the analytics allowlist is
        // written in the snake-case names that actually reach PostgreSQL.
        AnalyticsWarehouseModel.Apply(modelBuilder);
        PersistenceModelConfiguration.Apply(modelBuilder);
    }
}

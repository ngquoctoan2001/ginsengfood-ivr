using Microsoft.EntityFrameworkCore;
using Ivr.Infrastructure.FeatureFlags;

namespace Ivr.Infrastructure.Persistence;

/// <summary>
/// Owns the IVR PostgreSQL model. P1-2 creates the physical migration.
/// </summary>
public sealed class IvrDbContext(DbContextOptions<IvrDbContext> options) : DbContext(options)
{
    public DbSet<FeatureFlagEntity> FeatureFlags => Set<FeatureFlagEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfiguration(new FeatureFlagEntityConfiguration());
    }
}

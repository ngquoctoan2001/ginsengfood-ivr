using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivr.Infrastructure.FeatureFlags;

public sealed class FeatureFlagEntity
{
    public string Key { get; set; } = string.Empty;

    public string Environment { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public long Revision { get; set; }

    public string ValueJson { get; set; } = string.Empty;

    public string UpdatedBy { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; }

    public string Reason { get; set; } = string.Empty;
}

public sealed class FeatureFlagEntityConfiguration : IEntityTypeConfiguration<FeatureFlagEntity>
{
    public void Configure(EntityTypeBuilder<FeatureFlagEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("ivr_feature_flags");
        builder.HasKey(entity => new { entity.Key, entity.Environment });
        builder.Property(entity => entity.Key).HasColumnName("key").HasMaxLength(80);
        builder.Property(entity => entity.Environment).HasColumnName("env").HasMaxLength(24);
        builder.Property(entity => entity.Enabled).HasColumnName("enabled");
        builder.Property(entity => entity.Revision).HasColumnName("revision");
        builder.Property(entity => entity.ValueJson).HasColumnName("value_json").HasColumnType("jsonb");
        builder.Property(entity => entity.UpdatedBy).HasColumnName("updated_by").HasMaxLength(128);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Reason).HasColumnName("reason").HasMaxLength(500);
        builder.HasIndex(entity => new { entity.Key, entity.Environment }).IsUnique();
        builder.HasIndex(entity => new { entity.Environment, entity.Revision });
        builder.HasData(FeatureFlagSeedData.All());
    }
}

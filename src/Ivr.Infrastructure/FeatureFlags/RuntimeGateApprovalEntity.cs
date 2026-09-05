using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivr.Infrastructure.FeatureFlags;

/// <summary>
/// W-0195 / <c>OD-V1-20</c>. One recorded authorisation to move a runtime gate.
/// <para>
/// The gates read this table with raw SQL, so the entity exists for the model rather than for the
/// query: <c>DataClassification</c> derives what is shipped from the EF model, and a table the
/// model does not know about is a table the governance checks cannot see. The reads stay raw
/// because a gate must answer <c>false</c> when the database cannot answer at all, and that is
/// easier to be sure of with one statement than with a tracked query.
/// </para>
/// </summary>
public sealed class RuntimeGateApprovalEntity
{
    public string ApprovalReference { get; set; } = string.Empty;

    public string ApprovalKind { get; set; } = string.Empty;

    /// <summary>Null for an approval that is not scoped to one environment.</summary>
    public string? Environment { get; set; }

    /// <summary>Null when the approval is not tied to a proposal, as for the standing grant.</summary>
    public string? ProposerActorId { get; set; }

    public string ApproverActorId { get; set; } = string.Empty;

    /// <summary>SHA-256 of the exact before/after change, for a flag-change approval.</summary>
    public string? ChangeFingerprint { get; set; }

    public string Reason { get; set; } = string.Empty;

    /// <summary>The signed decision this authorisation comes from, e.g. <c>OD-V1-20@2026-09-05</c>.</summary>
    public string SignedDecisionRef { get; set; } = string.Empty;

    public DateTimeOffset GrantedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? RevokedReason { get; set; }

    public string CorrelationId { get; set; } = string.Empty;
}

public sealed class RuntimeGateApprovalEntityConfiguration
    : IEntityTypeConfiguration<RuntimeGateApprovalEntity>
{
    public void Configure(EntityTypeBuilder<RuntimeGateApprovalEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("ivr_runtime_gate_approvals");
        builder.HasKey(entity => entity.ApprovalReference);
        builder.Property(entity => entity.ApprovalReference)
            .HasColumnName("approval_reference").HasMaxLength(200);
        builder.Property(entity => entity.ApprovalKind)
            .HasColumnName("approval_kind").HasMaxLength(40);
        builder.Property(entity => entity.Environment)
            .HasColumnName("environment").HasMaxLength(24);
        builder.Property(entity => entity.ProposerActorId)
            .HasColumnName("proposer_actor_id").HasMaxLength(128);
        builder.Property(entity => entity.ApproverActorId)
            .HasColumnName("approver_actor_id").HasMaxLength(128);
        builder.Property(entity => entity.ChangeFingerprint)
            .HasColumnName("change_fingerprint").HasColumnType("char(64)");
        builder.Property(entity => entity.Reason)
            .HasColumnName("reason").HasMaxLength(500);
        builder.Property(entity => entity.SignedDecisionRef)
            .HasColumnName("signed_decision_ref").HasMaxLength(200);
        builder.Property(entity => entity.GrantedAt).HasColumnName("granted_at");
        builder.Property(entity => entity.ExpiresAt).HasColumnName("expires_at");
        builder.Property(entity => entity.RevokedAt).HasColumnName("revoked_at");
        builder.Property(entity => entity.RevokedReason)
            .HasColumnName("revoked_reason").HasMaxLength(500);
        builder.Property(entity => entity.CorrelationId)
            .HasColumnName("correlation_id").HasMaxLength(120);
        builder.HasIndex(entity => new
        {
            entity.ApprovalKind,
            entity.RevokedAt,
            entity.ExpiresAt,
        });
    }
}

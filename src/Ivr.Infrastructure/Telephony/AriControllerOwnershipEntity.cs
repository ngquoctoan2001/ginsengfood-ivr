using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Who is allowed to hold the ARI event socket for one Asterisk application. SIP-05.
/// <para>
/// Asterisk lets a second WebSocket take over an application from the first, and tells the loser
/// about it afterwards with <c>ApplicationReplaced</c>. There is no way to ask it to refuse. So
/// two workers that both think they should be dialling do not collide on a lock - one of them
/// silently stops receiving events for calls it believes it is running, while the other starts
/// new ones.
/// </para>
/// <para>
/// <b>This row does not prevent that.</b> It is a condition each worker checks on itself before
/// claiming work, not a fence Asterisk enforces. Its whole value is in refusing to hand ownership
/// over automatically: a lease that has merely expired means the holder stopped saying it was
/// alive, which is not the same as the holder having stopped. That distinction is the reason for
/// the <c>ISOLATED</c> state.
/// </para>
/// </summary>
public sealed class AriControllerOwnershipEntity
{
    /// <summary>Nobody holds it. The last holder released it cleanly, or it was never taken.</summary>
    public const string StateVacant = "VACANT";

    /// <summary>A worker holds it. Whether its lease is still fresh is read from the clock.</summary>
    public const string StateHeld = "HELD";

    /// <summary>
    /// A person has confirmed the previous holder can no longer reach ARI, and named themselves
    /// in the row while doing it. The only state other than <see cref="StateVacant"/> from which
    /// another worker may take ownership.
    /// </summary>
    public const string StateIsolated = "ISOLATED";

    /// <summary>
    /// Application, environment and execution mode together. One Asterisk application is the unit
    /// that can be stolen, so it is the unit that is owned; environment and mode are in the key so
    /// a lab and a production controller are never mistaken for rivals.
    /// </summary>
    public string ControllerScope { get; set; } = string.Empty;

    public string State { get; set; } = StateVacant;

    /// <summary>
    /// The holder, or - once <see cref="StateIsolated"/> - who was isolated. Kept rather than
    /// cleared, because the one worker that must never take this scope back is the one somebody
    /// just declared gone.
    /// </summary>
    public string? OwnerWorkerId { get; set; }

    /// <summary>
    /// Incremented on every grant, never reused. Lets work started under an older grant be told
    /// apart from work started under this one.
    /// </summary>
    public long FencingGeneration { get; set; }

    public DateTimeOffset? AcquiredAt { get; set; }

    public DateTimeOffset? HeartbeatAt { get; set; }

    /// <summary>
    /// When the holder stops being able to claim on this grant alone. Reaching it does not release
    /// the row and does not let anyone else in.
    /// </summary>
    public DateTimeOffset? LeaseExpiresAt { get; set; }

    public DateTimeOffset? ReleasedAt { get; set; }

    public string? ReleasedReason { get; set; }

    public DateTimeOffset? IsolatedAt { get; set; }

    public string? IsolatedByActorId { get; set; }

    public string? IsolatedReason { get; set; }

    /// <summary>
    /// Set when ownership was taken by force rather than handed over. The new holder owns the
    /// scope and still may not dial: calls from the previous generation may be up, and their
    /// channels have to be accounted for first. Cleared by a person in V1 - the automatic
    /// reconciler is SIP-06.
    /// </summary>
    public bool RequiresReconciliation { get; set; }

    public DateTimeOffset? ReconciledAt { get; set; }

    public string? ReconciledByActorId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;
}

public sealed class AriControllerOwnershipEntityConfiguration
    : IEntityTypeConfiguration<AriControllerOwnershipEntity>
{
    public void Configure(EntityTypeBuilder<AriControllerOwnershipEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("ivr_ari_controller_ownership");
        builder.HasKey(entity => entity.ControllerScope);
        builder.Property(entity => entity.ControllerScope)
            .HasColumnName("controller_scope").HasMaxLength(200);
        builder.Property(entity => entity.State)
            .HasColumnName("state").HasMaxLength(24);
        builder.Property(entity => entity.OwnerWorkerId)
            .HasColumnName("owner_worker_id").HasMaxLength(128);
        builder.Property(entity => entity.FencingGeneration)
            .HasColumnName("fencing_generation");
        builder.Property(entity => entity.AcquiredAt).HasColumnName("acquired_at");
        builder.Property(entity => entity.HeartbeatAt).HasColumnName("heartbeat_at");
        builder.Property(entity => entity.LeaseExpiresAt).HasColumnName("lease_expires_at");
        builder.Property(entity => entity.ReleasedAt).HasColumnName("released_at");
        builder.Property(entity => entity.ReleasedReason)
            .HasColumnName("released_reason").HasMaxLength(500);
        builder.Property(entity => entity.IsolatedAt).HasColumnName("isolated_at");
        builder.Property(entity => entity.IsolatedByActorId)
            .HasColumnName("isolated_by_actor_id").HasMaxLength(128);
        builder.Property(entity => entity.IsolatedReason)
            .HasColumnName("isolated_reason").HasMaxLength(500);
        builder.Property(entity => entity.RequiresReconciliation)
            .HasColumnName("requires_reconciliation");
        builder.Property(entity => entity.ReconciledAt).HasColumnName("reconciled_at");
        builder.Property(entity => entity.ReconciledByActorId)
            .HasColumnName("reconciled_by_actor_id").HasMaxLength(128);
        builder.Property(entity => entity.CorrelationId)
            .HasColumnName("correlation_id").HasMaxLength(120);
    }
}

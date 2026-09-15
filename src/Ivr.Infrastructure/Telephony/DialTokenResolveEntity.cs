using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// One resolve of one dial token, recorded so the ceiling survives a restart. SIP-05 / SIP-02.
/// <para>
/// The rule this backs is <c>OD-V1-17</c>: a dial token may be resolved a bounded number of times
/// and no more, so a leaked token still cannot dial more often than policy allows. Until this
/// table the bound lived in a <c>ConcurrentDictionary</c> inside whichever vault instance happened
/// to be in the process, and the class said so - it deferred the real bound to "the SIM vault",
/// which does not exist. A restart reset the count, and two workers each had their own.
/// </para>
/// <para>
/// One row per (token, attempt) rather than a counter, because all three rules fall out of the
/// rows and a counter would answer only one of them: the count is how many rows, the binding is
/// the task on the earliest row, and a replay is a row that is already there.
/// </para>
/// <para>
/// <b>The token itself is never stored.</b> What is stored is a SHA-256 of whatever the vault hands
/// the ledger, so the table cannot be turned back into a token or a number even by somebody
/// holding a database dump - and so the ledger stays safe whichever vault owns it, rather than
/// depending on the LAB vault happening to fingerprint its input already.
/// </para>
/// </summary>
public sealed class DialTokenResolveEntity
{
    /// <summary>SHA-256, hex, of the value the vault revealed. Never the value itself.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public string AttemptId { get; set; } = string.Empty;

    /// <summary>
    /// The task this resolve belonged to. The earliest row for a token is what binds it: a token
    /// turning up under a second task is a caller mix-up or a replay, and neither is worth dialling.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    public DateTimeOffset ResolvedAt { get; set; }

    /// <summary>
    /// The ceiling in force when this resolve was allowed. Recorded rather than enforced from,
    /// because the ceiling is a policy number that travels with each request; keeping it makes an
    /// audit able to say which policy a dial was allowed under instead of guessing from the date.
    /// </summary>
    public int MaxResolvesSnapshot { get; set; }
}

public sealed class DialTokenResolveEntityConfiguration
    : IEntityTypeConfiguration<DialTokenResolveEntity>
{
    public void Configure(EntityTypeBuilder<DialTokenResolveEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("ivr_dial_token_resolves");
        builder.HasKey(entity => new { entity.TokenHash, entity.AttemptId });
        builder.Property(entity => entity.TokenHash)
            .HasColumnName("token_hash").HasColumnType("char(64)");
        builder.Property(entity => entity.AttemptId)
            .HasColumnName("attempt_id").HasMaxLength(128);
        builder.Property(entity => entity.TaskId)
            .HasColumnName("task_id").HasMaxLength(128);
        builder.Property(entity => entity.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(entity => entity.MaxResolvesSnapshot)
            .HasColumnName("max_resolves_snapshot");

        // Every read is "everything known about this token", so the token leads and the ordering
        // column follows it. Without this the ceiling check is a scan on a table that only grows.
        builder.HasIndex(entity => new { entity.TokenHash, entity.ResolvedAt });
    }
}

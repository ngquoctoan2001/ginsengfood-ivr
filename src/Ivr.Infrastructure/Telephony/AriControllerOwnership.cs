using System.Data;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>Why a worker may or may not dial on this Asterisk application. SIP-05.</summary>
public enum AriControllerStatus
{
    /// <summary>This worker holds the application and may claim work.</summary>
    Held,

    /// <summary>Somebody else holds it and is still saying so. This worker waits.</summary>
    HeldByAnotherWorker,

    /// <summary>
    /// The holder stopped renewing. Deliberately NOT a grant: a lease that expired says the holder
    /// stopped reporting, not that it stopped. Taking over here is how two controllers end up
    /// dialling the same customers, so a person has to confirm the old one is gone.
    /// </summary>
    AwaitingIsolation,

    /// <summary>
    /// This worker owns the application, taken by force after an isolation, and may not dial yet.
    /// Calls from the previous generation may still be up and their channels are unaccounted for.
    /// </summary>
    AwaitingReconciliation,
}

/// <param name="FencingGeneration">
/// The grant this answer belongs to. Work started under an older generation is work started by
/// somebody else, whatever the worker id says.
/// </param>
public sealed record AriControllerGrant(
    AriControllerStatus Status,
    long FencingGeneration,
    string? OwnerWorkerId,
    DateTimeOffset? LeaseExpiresAt)
{
    /// <summary>Only one of the four statuses permits a dial, and it is worth saying so once.</summary>
    public bool MayDial => Status == AriControllerStatus.Held;
}

public interface IAriControllerOwnership
{
    /// <summary>The application this instance arbitrates, for logs and evidence.</summary>
    public string Scope { get; }

    /// <summary>
    /// Takes the application, renews it, or explains why not. Called every pass: the renewal is
    /// the heartbeat, so no separate loop can fall behind the one that matters.
    /// </summary>
    public Task<AriControllerGrant> AcquireOrRenewAsync(
        string workerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hands the application back, on a shutdown that drained. This is the one path that lets the
    /// next worker in without a person, because a worker that says this has stopped dialling and
    /// closed its socket.
    /// </summary>
    public Task ReleaseAsync(
        string workerId,
        string reason,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// For modes with no ARI socket to lose. Always grants, keeps no row and asks no database.
/// <para>
/// MOCK dispatch talks to an in-process fake, so there is no application for a second worker to
/// take over and nothing an ownership row would protect. Registered rather than left null so the
/// runtime has one code path, and named for what it is so nobody reads a green MOCK test as
/// evidence that ownership works.
/// </para>
/// </summary>
public sealed class UncontendedAriControllerOwnership(string scope) : IAriControllerOwnership
{
    public string Scope { get; } = scope;

    public Task<AriControllerGrant> AcquireOrRenewAsync(
        string workerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            new AriControllerGrant(AriControllerStatus.Held, 0, workerId, null));
    }

    public Task ReleaseAsync(
        string workerId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}

/// <summary>
/// The ownership state machine, in one row locked for the length of each decision. SIP-05.
/// <para>
/// Every transition is taken under <c>SELECT ... FOR UPDATE</c> rather than expressed as a
/// conditional <c>UPDATE</c>, so the rules read as rules. The cost is a row lock held for the
/// length of one decision; the benefit is that the one rule worth getting right - an expired lease
/// is not a vacancy - is a line of code somebody can read, not a <c>WHERE</c> clause they have to
/// reconstruct.
/// </para>
/// </summary>
public sealed class PostgresAriControllerOwnership(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IOptions<SchedulerOptions> options,
    TimeProvider timeProvider,
    string scope,
    IAuditLogger? auditLogger = null) : IAriControllerOwnership
{
    public string Scope { get; } = scope;

    public async Task<AriControllerGrant> AcquireOrRenewAsync(
        string workerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        TimeSpan lease = TimeSpan.FromSeconds(options.Value.ControllerLeaseSeconds);
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        AriControllerOwnershipEntity row = await LockRowAsync(context, cancellationToken);

        bool mine = string.Equals(row.OwnerWorkerId, workerId, StringComparison.Ordinal);
        bool leaseLive = row.LeaseExpiresAt is not null && row.LeaseExpiresAt > now;
        AriControllerGrant grant;

        if (string.Equals(row.State, AriControllerOwnershipEntity.StateHeld, StringComparison.Ordinal)
            && mine)
        {
            // Renewing past our own expiry is allowed, and is not the case this guards against.
            // Nothing took the socket while we were slow - if something had, it would have had to
            // go through isolation, which moves the row out of HELD.
            row.HeartbeatAt = now;
            row.LeaseExpiresAt = now.Add(lease);
            grant = Grant(row, row.RequiresReconciliation
                ? AriControllerStatus.AwaitingReconciliation
                : AriControllerStatus.Held);
        }
        else if (string.Equals(
            row.State,
            AriControllerOwnershipEntity.StateHeld,
            StringComparison.Ordinal))
        {
            // Somebody else has it. Fresh lease or stale, the answer is wait - the difference is
            // only in what has to happen next, and neither of them is "take it".
            grant = Grant(
                row,
                leaseLive
                    ? AriControllerStatus.HeldByAnotherWorker
                    : AriControllerStatus.AwaitingIsolation);
        }
        else if (string.Equals(
            row.State,
            AriControllerOwnershipEntity.StateIsolated,
            StringComparison.Ordinal)
            && mine)
        {
            // The isolated controller asking for itself back. Refused for as long as the row
            // remembers who was isolated: a process somebody declared gone is the single worst
            // candidate to hand the socket to, and it is the one most likely to ask, because
            // it never noticed anything was wrong.
            grant = Grant(row, AriControllerStatus.AwaitingIsolation);
        }
        else
        {
            bool forced = string.Equals(
                row.State,
                AriControllerOwnershipEntity.StateIsolated,
                StringComparison.Ordinal);
            row.State = AriControllerOwnershipEntity.StateHeld;
            row.OwnerWorkerId = workerId;
            row.FencingGeneration = checked(row.FencingGeneration + 1);
            row.AcquiredAt = now;
            row.HeartbeatAt = now;
            row.LeaseExpiresAt = now.Add(lease);
            row.ReleasedAt = null;
            row.ReleasedReason = null;
            row.IsolatedAt = null;
            row.IsolatedByActorId = null;
            row.IsolatedReason = null;

            // Taken by force rather than handed over, so the calls of the generation before this
            // one are unaccounted for. Owning the application and being allowed to dial on it are
            // separate questions from here until somebody answers the second.
            row.RequiresReconciliation = forced;
            row.ReconciledAt = forced ? null : row.ReconciledAt;
            grant = Grant(row, forced
                ? AriControllerStatus.AwaitingReconciliation
                : AriControllerStatus.Held);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await AuditAsync(
                workerId,
                forced ? "ARI_CONTROLLER_SEIZED" : "ARI_CONTROLLER_ACQUIRED",
                forced ? "Taken after isolation; dialling held until reconciled." : "Vacant scope.",
                grant,
                cancellationToken);
            return grant;
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return grant;
    }

    public async Task ReleaseAsync(
        string workerId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        AriControllerOwnershipEntity row = await LockRowAsync(context, cancellationToken);

        // Only the holder may release, and only from HELD. Releasing a scope somebody has already
        // isolated would undo the isolation and let the isolated worker straight back in.
        if (!string.Equals(row.State, AriControllerOwnershipEntity.StateHeld, StringComparison.Ordinal)
            || !string.Equals(row.OwnerWorkerId, workerId, StringComparison.Ordinal))
        {
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        row.State = AriControllerOwnershipEntity.StateVacant;
        row.OwnerWorkerId = null;
        row.LeaseExpiresAt = null;
        row.ReleasedAt = now;
        row.ReleasedReason = reason;
        row.RequiresReconciliation = false;
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await AuditAsync(
            workerId,
            "ARI_CONTROLLER_RELEASED",
            reason,
            Grant(row, AriControllerStatus.Held),
            cancellationToken);
    }

    /// <summary>
    /// Records that a person has confirmed the current holder can no longer reach ARI, which is
    /// the only thing that turns a stale lease into a scope another worker may take.
    /// <para>
    /// V1 has no endpoint for this, on purpose. The consequence of getting it wrong is two
    /// controllers dialling the same customers, the deployment profile is a single replica, and a
    /// deliberate operator action against the database is a better match for that risk than a
    /// button. The audit row is what makes it reviewable either way.
    /// </para>
    /// </summary>
    public async Task<AriControllerGrant> IsolateAsync(
        string actorId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        AriControllerOwnershipEntity row = await LockRowAsync(context, cancellationToken);
        row.State = AriControllerOwnershipEntity.StateIsolated;
        row.IsolatedAt = now;
        row.IsolatedByActorId = actorId;
        row.IsolatedReason = reason;
        row.LeaseExpiresAt = null;
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        AriControllerGrant grant = Grant(row, AriControllerStatus.AwaitingIsolation);
        await AuditAsync(actorId, "ARI_CONTROLLER_ISOLATED", reason, grant, cancellationToken);
        return grant;
    }

    /// <summary>
    /// Records that a person has accounted for the calls of the generation before a forced
    /// handover, which releases the new holder to dial. SIP-06 replaces the person with a
    /// reconciler that reads the channels back from Asterisk.
    /// </summary>
    public async Task<AriControllerGrant> ConfirmReconciledAsync(
        string actorId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        DateTimeOffset now = timeProvider.GetUtcNow();
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        AriControllerOwnershipEntity row = await LockRowAsync(context, cancellationToken);
        row.RequiresReconciliation = false;
        row.ReconciledAt = now;
        row.ReconciledByActorId = actorId;
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        AriControllerGrant grant = Grant(row, AriControllerStatus.Held);
        await AuditAsync(actorId, "ARI_CONTROLLER_RECONCILED", reason, grant, cancellationToken);
        return grant;
    }

    private async Task<AriControllerOwnershipEntity> LockRowAsync(
        IvrDbContext context,
        CancellationToken cancellationToken)
    {
        // Created on first sight rather than by a migration seed, so a scope that is renamed or
        // added does not need a schema change to become ownable.
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO ivr_ari_controller_ownership
                (controller_scope, state, fencing_generation, requires_reconciliation,
                 correlation_id)
            VALUES ({Scope}, 'VACANT', 0, FALSE, '')
            ON CONFLICT (controller_scope) DO NOTHING
            """,
            cancellationToken);
        return await context.Set<AriControllerOwnershipEntity>()
            .FromSqlInterpolated($"""
                SELECT * FROM ivr_ari_controller_ownership
                WHERE controller_scope = {Scope}
                FOR UPDATE
                """)
            .SingleAsync(cancellationToken);
    }

    private static AriControllerGrant Grant(
        AriControllerOwnershipEntity row,
        AriControllerStatus status) => new(
        status,
        row.FencingGeneration,
        row.OwnerWorkerId,
        row.LeaseExpiresAt);

    private async Task AuditAsync(
        string actor,
        string action,
        string reason,
        AriControllerGrant grant,
        CancellationToken cancellationToken)
    {
        if (auditLogger is null)
        {
            return;
        }

        await auditLogger.AppendAsync(
            new AuditEvent(
                actor,
                action,
                Scope,
                reason,
                grant.FencingGeneration.ToString(System.Globalization.CultureInfo.InvariantCulture),
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["fencing_generation"] = grant.FencingGeneration,
                    ["status"] = grant.Status.ToString(),
                }),
            cancellationToken);
    }
}

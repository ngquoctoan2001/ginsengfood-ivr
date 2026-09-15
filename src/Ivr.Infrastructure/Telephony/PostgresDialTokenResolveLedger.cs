using System.Data;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// The reusable-dial-token ceiling, kept in PostgreSQL so it survives a restart and binds every
/// process at once. SIP-02.
/// <para>
/// <see cref="DialTokenResolveLedger"/> holds the same rules in a <c>ConcurrentDictionary</c> and
/// is honest in its own comments that this bounds one worker rather than the system. That was
/// written when the real bound was expected to come from a SIM vault, and no SIM vault was ever
/// built - so in practice a restart reset the count and two workers each kept their own. A token
/// that policy allowed to dial twice could dial twice per process, for as long as processes kept
/// restarting.
/// </para>
/// <para>
/// The rules and their order are taken from the in-memory ledger deliberately, not re-derived:
/// expiry, then a missing ceiling, then task binding, then replay, then the ceiling. Replay before
/// ceiling matters and is not an accident - a repeated attempt should read as a replay even when
/// the budget happens to be spent, because one is a caller bug and the other is policy working.
/// </para>
/// <para>
/// One advisory lock per token for the length of the decision, following
/// <c>PostgresIdempotencyStore</c>. Two resolves of the same token serialise; two resolves of two
/// different tokens do not touch each other, which is what lets a worker dial thirty-two at once.
/// </para>
/// </summary>
public sealed class PostgresDialTokenResolveLedger(
    IDbContextFactory<IvrDbContext> dbContextFactory) : IDialTokenResolveLedger
{
    public async ValueTask<DialTokenResolveDecision> EvaluateAsync(
        DialTokenResolutionRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Both refusals below record nothing and need no lock: they are properties of the request
        // alone. Reaching the database to say "this token expired an hour ago" would be a round
        // trip to learn something already in hand.
        if (request.DialToken.ExpiresAt <= now)
        {
            return new DialTokenResolveDecision(
                false,
                DialTokenRefusalCodes.Expired,
                0,
                request.MaxResolves);
        }

        if (request.MaxResolves <= 0)
        {
            return new DialTokenResolveDecision(
                false,
                DialTokenRefusalCodes.CeilingMissing,
                0,
                request.MaxResolves);
        }

        string tokenHash = HashToken(request.DialToken.RevealToTrustedResolver());
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        // Serialises contenders for this one token. ReadCommitted is enough because the lock, not
        // the isolation level, is what makes the read-then-insert atomic - the same reasoning
        // PostgresIdempotencyStore records for its own per-key lock.
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({tokenHash}, 0))",
            cancellationToken);

        List<DialTokenResolveEntity> resolves = await context.DialTokenResolves
            .Where(resolve => resolve.TokenHash == tokenHash)
            .OrderBy(resolve => resolve.ResolvedAt)
            .ThenBy(resolve => resolve.AttemptId)
            .ToListAsync(cancellationToken);

        int resolveCount = resolves.Count;
        string attemptId = request.AttemptId.Value;
        string taskId = request.TaskId.Value;

        // Binding is to the FIRST task that used the token, which is the one intake issued it for.
        // Read off the earliest row rather than stored separately: a token with no rows is not yet
        // bound, and in the in-memory ledger the only request that can bind is one that is then
        // allowed, so the two agree.
        if (resolves.Count > 0
            && !string.Equals(resolves[0].TaskId, taskId, StringComparison.Ordinal))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new DialTokenResolveDecision(
                false,
                DialTokenRefusalCodes.TaskMismatch,
                resolveCount,
                request.MaxResolves);
        }

        if (resolves.Exists(resolve =>
            string.Equals(resolve.AttemptId, attemptId, StringComparison.Ordinal)))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new DialTokenResolveDecision(
                false,
                DialTokenRefusalCodes.AttemptReplay,
                resolveCount,
                request.MaxResolves);
        }

        if (resolveCount >= request.MaxResolves)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new DialTokenResolveDecision(
                false,
                DialTokenRefusalCodes.ResolveLimitExceeded,
                resolveCount,
                request.MaxResolves);
        }

        context.DialTokenResolves.Add(new DialTokenResolveEntity
        {
            TokenHash = tokenHash,
            AttemptId = attemptId,
            TaskId = taskId,
            ResolvedAt = now,
            MaxResolvesSnapshot = request.MaxResolves,
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new DialTokenResolveDecision(
            true,
            null,
            resolveCount + 1,
            request.MaxResolves);
    }

    /// <summary>
    /// Hashes whatever the vault revealed, so the ledger never holds the thing it is counting.
    /// <para>
    /// The LAB vault already hands over an irreversible fingerprint, so this is belt and braces
    /// there. It stops being belt and braces the moment a vault that can reverse its own values
    /// owns this ledger, which is the whole point of production token work - and by then the table
    /// will already exist with rows in it.
    /// </para>
    /// </summary>
    private static string HashToken(string revealed) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(revealed)));
}

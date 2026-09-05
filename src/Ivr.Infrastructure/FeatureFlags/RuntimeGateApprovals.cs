using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.FeatureFlags;

/// <summary>
/// W-0195 / <c>OD-V1-20</c>. The three runtime gates, answered from a signed approval record
/// instead of from a hard-coded <c>false</c>.
/// <para>
/// Until the owner signed <c>OD-V1-20</c> there was no permission that allowed anyone to move
/// <c>globalDialKillSwitch</c> or <c>labDestinationAllowlist</c>, so the three gates shipped as
/// <c>Pending*</c> classes returning <c>false</c>. That was the correct answer to "who may do
/// this" while the answer was nobody. It is the wrong answer now, and it was never a design —
/// leaving it in place would mean the runtime-gate console can never be used in any environment.
/// </para>
/// <para>
/// Every gate here reads <c>ivr_runtime_gate_approvals</c>, which the database keeps append-only:
/// a row may be revoked and nothing else. A gate that cannot reach the table answers <b>no</b>.
/// That is not a fallback, it is the point — an approval that cannot be produced has not been
/// given.
/// </para>
/// </summary>
public static class RuntimeGateApprovalKinds
{
    /// <summary>
    /// Permission to administer runtime gates at all. Granted once by <c>OD-V1-20</c>; it does
    /// <b>not</b> grant any individual risk-increasing change, which still needs four eyes.
    /// </summary>
    public const string RuntimeGateAdmin = "RUNTIME_GATE_ADMIN";

    /// <summary>One approval for one exact flag change, bound to its before/after fingerprint.</summary>
    public const string FeatureFlagChange = "FEATURE_FLAG_CHANGE";

    /// <summary>
    /// The release decision behind <c>PRODUCTION_REAL</c> dialling (<c>DF-03</c>, <c>OD-V1-12</c>).
    /// No migration seeds this one, and none should: it is the last gate before a real customer
    /// hears a telephone ring.
    /// </summary>
    public const string ProductionCall = "PRODUCTION_CALL";
}

/// <summary>
/// Binds an approval to one exact change so it cannot be replayed against a different one.
/// </summary>
public static class RuntimeGateFingerprint
{
    /// <summary>
    /// A stable hash of the before and after snapshots.
    /// <para>
    /// Built field by field rather than by serializing the record, because the allowlist is a set
    /// and a set has no order. Two runs that disagree about member order would produce two
    /// fingerprints for one change, and an approver would be unable to authorize anything.
    /// </para>
    /// </summary>
    public static string Of(FeatureFlagSnapshot before, FeatureFlagSnapshot after)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        StringBuilder canonical = new();
        Append(canonical, before);
        canonical.Append("=>");
        Append(canonical, after);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
    }

    private static void Append(StringBuilder target, FeatureFlagSnapshot snapshot)
    {
        target.Append(snapshot.Environment).Append('|')
            .Append(snapshot.Revision.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(snapshot.ExecutionMode).Append('|')
            .Append(snapshot.SalesProvider).Append('|')
            .Append(snapshot.SimProvider).Append('|')
            .Append(snapshot.AttemptPolicyVersion).Append('|')
            .Append(snapshot.RealCustomerCallAllowed ? '1' : '0').Append('|')
            .Append(snapshot.GlobalDialKillSwitch ? '1' : '0').Append('|')
            .Append(snapshot.V1NotificationEnabled ? '1' : '0').Append('|')
            .Append(snapshot.RecordingEnabled ? '1' : '0').Append('|')
            .AppendJoin(',', snapshot.LabDestinationAllowlist.Order(StringComparer.Ordinal));
    }
}

/// <summary>
/// Reads whether a live, unrevoked approval of a given kind exists.
/// </summary>
internal static class RuntimeGateApprovalReader
{
    /// <summary>
    /// True when at least one approval of <paramref name="kind"/> is granted, unrevoked and not
    /// expired. Any failure to answer is answered as <c>false</c>.
    /// </summary>
    public static async Task<bool> AnyLiveAsync(
        IDbContextFactory<IvrDbContext> dbContextFactory,
        TimeProvider timeProvider,
        string kind,
        CancellationToken cancellationToken)
    {
        try
        {
            await using IvrDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);
            DateTimeOffset now = timeProvider.GetUtcNow();
            return await dbContext.Database
                .SqlQueryRaw<bool>(
                    """
                    SELECT EXISTS (
                        SELECT 1
                        FROM ivr_runtime_gate_approvals
                        WHERE approval_kind = {0}
                          AND revoked_at IS NULL
                          AND (expires_at IS NULL OR expires_at > {1})
                    ) AS "Value"
                    """,
                    kind,
                    now)
                .SingleAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // An unreachable or missing approval table is an ungranted approval. Saying "yes"
            // because the question could not be asked is the failure mode every gate here exists
            // to prevent.
            return false;
        }
    }
}

/// <summary>
/// W-0195. Runtime-gate administration is permitted because <c>OD-V1-20</c> is signed and the
/// signature is recorded as a row, not as a constant in code.
/// </summary>
public sealed class PostgresRuntimeGateAuthorization(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IRuntimeGateAuthorization
{
    public Task<bool> IsApprovedAsync(CancellationToken cancellationToken = default) =>
        RuntimeGateApprovalReader.AnyLiveAsync(
            dbContextFactory,
            timeProvider,
            RuntimeGateApprovalKinds.RuntimeGateAdmin,
            cancellationToken);
}

/// <summary>
/// W-0195. The release gate behind real customer dialling. Answers <c>false</c> until a
/// <c>PRODUCTION_CALL</c> approval exists, which no migration creates.
/// </summary>
public sealed class PostgresProductionCallGate(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IProductionCallGate
{
    public Task<bool> IsApprovedAsync(CancellationToken cancellationToken = default) =>
        RuntimeGateApprovalReader.AnyLiveAsync(
            dbContextFactory,
            timeProvider,
            RuntimeGateApprovalKinds.ProductionCall,
            cancellationToken);
}

/// <summary>
/// W-0195. Resolves a four-eyes approval to the actor who granted it.
/// <para>
/// The approval has to name the exact change. Without the fingerprint an approver could sign
/// "widen the allowlist by one test number" and the same reference would then authorize widening
/// it by a thousand — the reference would be a password rather than a decision.
/// </para>
/// </summary>
public sealed class PostgresFourEyesApprovalVerifier(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IFourEyesApprovalVerifier
{
    public async Task<string?> VerifyAsync(
        string approvalReference,
        string proposerActorId,
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(approvalReference)
            || string.IsNullOrWhiteSpace(proposerActorId))
        {
            return null;
        }

        try
        {
            await using IvrDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);
            DateTimeOffset now = timeProvider.GetUtcNow();

            // The proposer comparison is in the query as well as in the caller. The caller's check
            // is the one a reader sees; this one is the one that still holds if a future caller
            // forgets, and it costs a predicate.
            List<string> approvers = await dbContext.Database
                .SqlQueryRaw<string>(
                    """
                    SELECT approver_actor_id AS "Value"
                    FROM ivr_runtime_gate_approvals
                    WHERE approval_reference = {0}
                      AND approval_kind = {1}
                      AND change_fingerprint = {2}
                      AND environment = {3}
                      AND approver_actor_id <> {4}
                      AND revoked_at IS NULL
                      AND (expires_at IS NULL OR expires_at > {5})
                    """,
                    approvalReference,
                    RuntimeGateApprovalKinds.FeatureFlagChange,
                    RuntimeGateFingerprint.Of(before, after),
                    before.Environment,
                    proposerActorId,
                    now)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return approvers.Count == 1 ? approvers[0] : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

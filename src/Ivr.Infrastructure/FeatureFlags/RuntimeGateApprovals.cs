using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ivr.Infrastructure.Observability;
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
    /// <para>
    /// W-0213. Its <c>environment</c> is stored and <b>never read</b> — see the note on
    /// <see cref="FeatureFlagChange"/> for which kinds the column actually scopes.
    /// </para>
    /// </summary>
    public const string RuntimeGateAdmin = "RUNTIME_GATE_ADMIN";

    /// <summary>
    /// One approval for one exact flag change, bound to its before/after fingerprint.
    /// <para>
    /// W-0213. This is the <b>only</b> kind the <c>environment</c> column scopes, and it scopes it
    /// twice: <see cref="PostgresFourEyesApprovalVerifier"/> filters on the column, and the
    /// fingerprint it also matches puts <c>snapshot.Environment</c> first. A lab approval
    /// therefore cannot be replayed against a production change even by someone reusing the
    /// reference.
    /// </para>
    /// <para>
    /// W-0301 corrected what stood here. This said the <c>environment</c> column scoped only this
    /// kind, that administration was coarse on purpose, and that writing an environment on an
    /// admin row <i>looked</i> like scoping without being it. The first two clauses are no longer
    /// true and the third was the bug rather than a caveat: a column an approver can fill in and
    /// nobody reads is worse than no column, because it invites a promise the system never made.
    /// All three kinds now scope on the environment — <c>PRODUCTION_CALL</c> since SIP-04,
    /// <c>RUNTIME_GATE_ADMIN</c> since <c>W-0301</c> — and each has a database constraint refusing
    /// a live grant that names none.
    /// </para>
    /// <para>
    /// What remains true, and is the reason this kind is still different: it scopes
    /// <b>twice</b>. The column is one half; the change fingerprint, which puts
    /// <c>snapshot.Environment</c> first, is the other. A lab approval cannot be replayed against
    /// a production change even by someone reusing the reference — for the other two kinds the
    /// column is the whole of it.
    /// </para>
    /// </summary>
    public const string FeatureFlagChange = "FEATURE_FLAG_CHANGE";

    /// <summary>
    /// The release decision behind <c>PRODUCTION_REAL</c> dialling (<c>DF-03</c>, <c>OD-V1-12</c>).
    /// No migration seeds this one, and none should: it is the last gate before a real customer
    /// hears a telephone ring.
    /// </summary>
    public const string ProductionCall = "PRODUCTION_CALL";

    /// <summary>
    /// Q-28 (PA2, 2026-09-26). Binds one exact production pilot list, for one environment, with
    /// four eyes: <c>change_fingerprint</c> is the list's <c>ProductionPilotFingerprint.ListHash</c>
    /// and the proposer is not the approver. Editing the configured list therefore closes the
    /// pilot until a second person approves the new one. The database refuses a row of this kind
    /// without a proposer, a fingerprint or an environment.
    /// </summary>
    public const string ProductionPilotList = "PRODUCTION_PILOT_LIST";

    /// <summary>
    /// Q-28 (PA2, 2026-09-26). The signed decision that moves one environment from pilot to open,
    /// after which production may ring any destination its dial tokens resolve. Scoped to the
    /// environment it names, like <see cref="ProductionCall"/>; no migration seeds one.
    /// </summary>
    public const string ProductionCallOpen = "PRODUCTION_CALL_OPEN";
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
    /// W-0360 / K-31. The <c>ivr.reason_code</c> on <c>ivr_fail_closed_total</c> when the approval
    /// store could not be asked. The gate still answers "no"; the count is what tells an outage of
    /// the store apart from an approval that really is not there.
    /// </summary>
    public const string ApprovalStoreUnreadable = "RUNTIME_GATE_APPROVAL_UNREADABLE";

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
                .CreateDbContextAsync(cancellationToken);
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
                .SingleAsync(cancellationToken);
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

    /// <summary>
    /// SIP-04. True when a live approval of <paramref name="kind"/> names exactly
    /// <paramref name="environment"/>. Any failure to answer is answered as <c>false</c>.
    /// <para>
    /// Separate from <see cref="AnyLiveAsync"/> rather than a parameter on it, because the two
    /// answer different questions and one of them is deliberately coarse. Runtime-gate
    /// administration ignores the environment on purpose - <c>IT-GATE-APPROVAL-10</c> pins that,
    /// and the reasoning is that the environment-specific decision lives on each four-eyes row.
    /// Folding both into one method with a nullable argument would put those two decisions one
    /// typo apart.
    /// </para>
    /// <para>
    /// A NULL environment matches nothing. That is the difference being introduced, so reading it
    /// as a wildcard would introduce nothing.
    /// </para>
    /// </summary>
    public static async Task<bool> AnyLiveForEnvironmentAsync(
        IDbContextFactory<IvrDbContext> dbContextFactory,
        TimeProvider timeProvider,
        string kind,
        string environment,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(environment))
        {
            return false;
        }

        try
        {
            await using IvrDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken);
            DateTimeOffset now = timeProvider.GetUtcNow();
            return await dbContext.Database
                .SqlQueryRaw<bool>(
                    """
                    SELECT EXISTS (
                        SELECT 1
                        FROM ivr_runtime_gate_approvals
                        WHERE approval_kind = {0}
                          AND environment = {2}
                          AND revoked_at IS NULL
                          AND (expires_at IS NULL OR expires_at > {1})
                    ) AS "Value"
                    """,
                    kind,
                    now,
                    environment)
                .SingleAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // Still "no": saying "yes" because the question could not be asked is what every gate
            // here exists to prevent. W-0360 / K-31: counted, because both callers are release
            // gates (production dialling, runtime-gate administration) and an unreadable store
            // used to look exactly like an approval nobody had granted.
            IvrTelemetry.RecordFailClosed((TelemetryTags.ReasonCode, ApprovalStoreUnreadable));
            return false;
        }
    }

    /// <summary>
    /// Q-28 (PA2). True when a live approval of <paramref name="kind"/> names exactly
    /// <paramref name="environment"/> and binds exactly <paramref name="changeFingerprint"/>. Any
    /// failure to answer is answered as <c>false</c> and counted, as in
    /// <see cref="AnyLiveForEnvironmentAsync"/>.
    /// </summary>
    public static async Task<bool> AnyLiveForEnvironmentAndChangeAsync(
        IDbContextFactory<IvrDbContext> dbContextFactory,
        TimeProvider timeProvider,
        string kind,
        string environment,
        string changeFingerprint,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(environment) || string.IsNullOrWhiteSpace(changeFingerprint))
        {
            return false;
        }

        try
        {
            await using IvrDbContext dbContext = await dbContextFactory
                .CreateDbContextAsync(cancellationToken);
            DateTimeOffset now = timeProvider.GetUtcNow();
            return await dbContext.Database
                .SqlQueryRaw<bool>(
                    """
                    SELECT EXISTS (
                        SELECT 1
                        FROM ivr_runtime_gate_approvals
                        WHERE approval_kind = {0}
                          AND environment = {2}
                          AND change_fingerprint = {3}
                          AND revoked_at IS NULL
                          AND (expires_at IS NULL OR expires_at > {1})
                    ) AS "Value"
                    """,
                    kind,
                    now,
                    environment,
                    changeFingerprint)
                .SingleAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            IvrTelemetry.RecordFailClosed((TelemetryTags.ReasonCode, ApprovalStoreUnreadable));
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
    public Task<bool> IsApprovedAsync(
        string environment,
        CancellationToken cancellationToken = default) =>
        RuntimeGateApprovalReader.AnyLiveForEnvironmentAsync(
            dbContextFactory,
            timeProvider,
            RuntimeGateApprovalKinds.RuntimeGateAdmin,
            environment,
            cancellationToken);
}

/// <summary>
/// W-0195. The release gate behind real customer dialling. Answers <c>false</c> until a
/// <c>PRODUCTION_CALL</c> approval exists for <i>this</i> environment, which no migration creates.
/// <para>
/// SIP-04 added the environment. The gate previously asked <see cref="RuntimeGateApprovalReader
/// .AnyLiveAsync"/> for any live approval of the kind, so a single signature opened every
/// deployment reading the same database - a pilot approval authorised production, and nothing in
/// the row said otherwise even though the column to say it was already there.
/// </para>
/// <para>
/// An approval that names no environment opens nothing, rather than counting as a wildcard. A NULL
/// there is the shape the unscoped approvals have, and reading it as "everywhere" would preserve
/// exactly the behaviour being removed. The migration refuses to store one, so this is belt and
/// braces on a row that should not exist.
/// </para>
/// </summary>
public sealed class PostgresProductionCallGate(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IProductionCallGate
{
    public Task<bool> IsApprovedAsync(
        string environment,
        CancellationToken cancellationToken = default) =>
        RuntimeGateApprovalReader.AnyLiveForEnvironmentAsync(
            dbContextFactory,
            timeProvider,
            RuntimeGateApprovalKinds.ProductionCall,
            environment,
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
                .CreateDbContextAsync(cancellationToken);
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
                .ToListAsync(cancellationToken);
            return approvers.Count == 1 ? approvers[0] : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // No approver, as before. W-0360 / K-31: counted, so a store that cannot be asked is
            // told apart from a four-eyes approval that does not match the change.
            IvrTelemetry.RecordFailClosed((TelemetryTags.ReasonCode, FourEyesStoreUnreadable));
            return null;
        }
    }

    /// <summary>
    /// W-0360 / K-31. The <c>ivr.reason_code</c> on <c>ivr_fail_closed_total</c> when the four-eyes
    /// approval could not be looked up at all.
    /// </summary>
    public const string FourEyesStoreUnreadable = "FOUR_EYES_APPROVAL_UNREADABLE";
}

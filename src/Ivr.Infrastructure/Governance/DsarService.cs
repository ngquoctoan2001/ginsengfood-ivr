using System.Data;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Retention;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ivr.Infrastructure.Governance;

/// <summary>What IVR holds about one order, in counts. Never in values.</summary>
public sealed record DsarHolding(string Table, string ProtectionClass, int RowCount);

public sealed record DsarFindReport(
    string OrderCode,
    bool Found,
    IReadOnlyList<DsarHolding> Holdings,
    IReadOnlyList<string> NotErasable);

public sealed record DsarErasureReport(
    string OrderCode,
    bool DryRun,
    int TasksRedacted,
    IReadOnlyList<string> Refused,
    string AuditRef);

public interface IDsarService
{
    public Task<DsarFindReport> FindAsync(string orderCode, CancellationToken cancellationToken);

    public Task<DsarErasureReport> EraseAsync(
        string orderCode,
        string reason,
        string actorId,
        string correlationId,
        bool dryRun,
        CancellationToken cancellationToken);
}

/// <summary>
/// Data-subject request support for the scope IVR actually holds (<c>W-0052</c> / P10-1).
///
/// <para><b>No HTTP endpoint, and that is a decision rather than an omission.</b>
/// Erasing customer data needs an authority IVR does not own: permissions are
/// assigned by Permission Core (DF-01), <c>IVR_RUNTIME_GATE_ADMIN</c> is still
/// unassigned pending <c>OD-V1-20</c>, and hanging erasure off an existing
/// operational permission would mean anyone who can watch the queue can delete a
/// customer's records. So this is a service driven by
/// <c>docs/compliance/dsar-runbook.md</c> under an audited manual procedure, and
/// the endpoint waits for a permission that exists.</para>
///
/// <para><b>Find returns counts, never values.</b> A subject-access response is
/// assembled by a human from this plus the order system; a service that printed
/// the stored personal data would be a new way to read it, available to whoever
/// can call the service.</para>
///
/// <para><b>Erasure redacts through the same SQL the retention job uses.</b> Two
/// code paths that redact the same columns will eventually disagree about which
/// columns those are, and the one that runs less often will be the stale one.</para>
/// </summary>
public sealed class DsarService(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IAuditLogger auditLogger,
    TimeProvider timeProvider) : IDsarService
{
    /// <summary>
    /// The erasure statement, built once from the retention job's own redaction so the scheduled
    /// path and the on-request path cannot come to disagree about which columns are personal data.
    ///
    /// <para>Two mechanics worth stating. <c>ExecuteSqlRaw</c> reads <c>{n}</c> as a parameter
    /// placeholder and the shared redaction contains <c>'{}'::jsonb</c>, so the literal braces are
    /// doubled. And the statement is assembled here, into a constant, rather than concatenated at
    /// the call site: every part of it is compile-time text, no caller-supplied value reaches it,
    /// and the order code arrives as parameter 0.</para>
    /// </summary>
    private static readonly string RedactByOrderCodeSql =
        "UPDATE ivr_confirmation_tasks SET "
        + RetentionTargetCatalog.SpeechSnapshotRedactionSql
            .Replace("{", "{{", StringComparison.Ordinal)
            .Replace("}", "}}", StringComparison.Ordinal)
        + ", anonymized_at = {1} WHERE order_code = {0}";

    public const string EraseAuditAction = "IVR_DSAR_ERASE";
    private const int MinReasonLength = 8;

    /// <summary>
    /// Things IVR holds that a request cannot remove, with the reason. Returned by
    /// <see cref="FindAsync"/> so the limit is known before the requester is
    /// promised anything, rather than discovered while answering them.
    /// </summary>
    public static IReadOnlyList<string> NotErasable { get; } =
    [
        "ivr_audit_log and ivr_admin_actions: append-only, enforced by the database. A record of "
        + "who did what that the subject can delete is not a record.",
        "ivr_confirmation_tasks.order_code: the key a request arrives with. Erasing it makes "
        + "every later request about the same order unanswerable, including the subject's own.",
        "ivr_result_callbacks.payload_json: the delivery record. Removing the payload leaves a "
        + "record that cannot settle the dispute it exists for; it expires with retention.",
    ];

    public async Task<DsarFindReport> FindAsync(
        string orderCode,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderCode);

        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        // One snapshot for the whole report. Six counts taken outside a transaction describe six
        // different instants, and retention or a live call can move a row between them — so the
        // answer given to a data subject could show a job with no attempts, or attempts under a
        // job it had already said was gone. REPEATABLE READ is what Postgres calls the guarantee
        // that every read in the transaction sees the same instant, which is the whole
        // requirement here; nothing below writes, so there is no serialisation conflict to lose.
        await using IDbContextTransaction snapshot = await context.Database
            .BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);

        // Left as queries rather than materialised into arrays. The previous shape pulled every
        // order id and job id back to the process and then sent them out again inside IN (...),
        // which is a parameter per row against a PostgreSQL statement limit of 65,535 — an order
        // with enough history stopped being answerable at all, and the failure would arrive as a
        // driver error in the middle of a subject-access request. As subqueries they never leave
        // the database, and the filter behind them is an indexed equality on one order code.
        IQueryable<string> orderIds = context.ConfirmationTasks.AsNoTracking()
            .Where(task => task.OrderCode == orderCode)
            .Select(task => task.OfficialOrderId)
            .Distinct();

        IQueryable<string> jobIds = context.CallJobs.AsNoTracking()
            .Where(job => orderIds.Contains(job.OfficialOrderId))
            .Select(job => job.IvrCallJobId);

        int taskCount = await context.ConfirmationTasks.AsNoTracking()
            .CountAsync(task => task.OrderCode == orderCode, cancellationToken);
        if (taskCount == 0)
        {
            await snapshot.CommitAsync(cancellationToken);
            return new DsarFindReport(orderCode, false, [], NotErasable);
        }

        List<DsarHolding> holdings =
        [
            Hold("ivr_confirmation_tasks", taskCount),
            Hold("ivr_call_jobs", await context.CallJobs.AsNoTracking()
                .CountAsync(job => orderIds.Contains(job.OfficialOrderId), cancellationToken)),
            Hold("ivr_call_attempts", await context.CallAttempts.AsNoTracking()
                .CountAsync(attempt => jobIds.Contains(attempt.IvrCallJobId), cancellationToken)),
            Hold("ivr_call_results", await context.CallResults.AsNoTracking()
                .CountAsync(result => jobIds.Contains(result.IvrCallJobId), cancellationToken)),
            Hold("ivr_result_callbacks", await context.ResultCallbacks.AsNoTracking()
                .CountAsync(callback => orderIds.Contains(callback.OfficialOrderId), cancellationToken)),
            Hold("fact_call_outcome", await context.AnalyticsFacts.AsNoTracking()
                .CountAsync(fact => jobIds.Contains(fact.IvrCallJobId), cancellationToken)),
        ];

        await snapshot.CommitAsync(cancellationToken);
        return new DsarFindReport(orderCode, true, holdings, NotErasable);
    }

    public async Task<DsarErasureReport> EraseAsync(
        string orderCode,
        string reason,
        string actorId,
        string correlationId,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < MinReasonLength)
        {
            // The reason ends up in the audit row. "ok" in that field is the same as no record.
            throw new ArgumentException(
                $"A DSAR erasure reason of at least {MinReasonLength} characters is required.",
                nameof(reason));
        }

        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken);

        int matched;
        int redacted;
        if (dryRun)
        {
            matched = await context.ConfirmationTasks
                .CountAsync(task => task.OrderCode == orderCode, cancellationToken);
            redacted = 0;
        }
        else
        {
            // The retention job's own redaction, reused rather than reimplemented. Two code paths
            // redacting "the same" columns drift, and the one that runs less often goes stale.
            //
            // Its own row count is the match count, and taking it from there is what makes the two
            // numbers in the audit row agree. The statement's only predicate is the order code, so
            // every task it matched is a task it redacted — counting separately first left a
            // window in which a task could arrive or leave, and then the audit row said one thing
            // was found and a different number changed, about the same instant, for good.
            redacted = await context.Database.ExecuteSqlRawAsync(
                RedactByOrderCodeSql,
                [orderCode, timeProvider.GetUtcNow()],
                cancellationToken);
            matched = redacted;
        }

        // Audited even when it changed nothing. A request that found no data is a request that was
        // answered, and the answer has to be as durable as the erasure would have been.
        AuditLogEntry entry = await auditLogger.AppendAsync(
            new AuditEvent(
                actorId,
                EraseAuditAction,
                $"order:{orderCode}",
                reason.Trim(),
                correlationId,
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["dry_run"] = dryRun,
                    ["tasks_matched"] = matched,
                    ["tasks_redacted"] = redacted,
                    ["not_erasable_count"] = NotErasable.Count,
                }),
            cancellationToken);

        return new DsarErasureReport(
            orderCode,
            dryRun,
            redacted,
            NotErasable,
            entry.Id.ToString());
    }

    private static DsarHolding Hold(string table, int count) =>
        new(table, DataClassification.Tables[table].Protection.ToString(), count);
}

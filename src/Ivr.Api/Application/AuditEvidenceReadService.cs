using Ivr.Api.Admin;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Api.Application;

public interface IAuditEvidenceReadService
{
    public Task<AuditEvidenceApiResult> GetAsync(
        string? targetType,
        string? targetId,
        int? limit,
        string? reason,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Reads the append-only audit log for one object (W-0307 / B9).
/// </summary>
/// <remarks>
/// <para>
/// Absolutely read-only, and structurally so rather than by promise: the query runs
/// <c>AsNoTracking</c> on a table whose <c>trg_ivr_audit_log_append_only</c> trigger refuses UPDATE
/// and DELETE anyway, and this service holds no write path to it other than appending its own
/// access record.
/// </para>
/// <para>
/// <b>Permission.</b> This sits on <c>AdminPolicies.Read</c> and not on a permission of its own,
/// which is worth stating plainly because a dedicated one would be better. The permission set is
/// <c>DF-01</c>: LOCKED at seven, and owned by Permission Core rather than by Module 8 --
/// <c>OD-V1-20</c> had to open and sign a decision just to add <c>IVR_RUNTIME_GATE_ADMIN</c>.
/// Inventing an eighth here would be Module 8 signing someone else's register, which is the exact
/// thing the 16/09 review is right to object to elsewhere. So: ship on Read, and record that a
/// dedicated audit-read permission needs a DF-01 decision this module cannot take.
/// </para>
/// <para>
/// <b>Why the read is itself audited.</b> <c>specs/data/05-pii-policy.md</c> requires RBAC plus
/// audit for every admin access, and of all accesses this is the one most worth a row: pulling the
/// history of an object is what someone reconstructing -- or checking up on -- a sequence of
/// actions does. <c>reason</c> is therefore required, following the precedent already set by the
/// analytics export.
/// </para>
/// <para>
/// <b>The access row lands in the trail it just read, and that is deliberate.</b>
/// <c>PostgresAuditLogger</c> splits the entity reference back into target type and id, so an
/// access recorded against <c>type:id</c> matches the same filter the caller used. Read an
/// object's history twice and it is one row longer the second time. That reads as surprising and
/// is the correct behaviour: who looked at a record is part of what happened to that record, and
/// an auditor who cannot see the lookups is missing the half of the trail most likely to matter.
/// Pinned by <c>IT-AUDITEV-15</c> so that nobody later 'fixes' it into a blind spot.
/// </para>
/// </remarks>
public sealed class AuditEvidenceReadService(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IAuditLogger auditLogger) : IAuditEvidenceReadService
{
    public const int DefaultLimit = 100;
    public const int MaxLimit = 500;
    public const int MinReasonLength = 8;

    private const string AccessAuditAction = "IVR_AUDIT_EVIDENCE_READ";

    public async Task<AuditEvidenceApiResult> GetAsync(
        string? targetType,
        string? targetId,
        int? limit,
        string? reason,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        string normalizedTargetType = RequireIdentifier(targetType, "target_type");
        string normalizedTargetId = RequireIdentifier(targetId, "target_id");
        string normalizedReason = RequireReason(reason);
        int normalizedLimit = NormalizeLimit(limit);

        await using IvrDbContext dbContext =
            await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // One more than the limit, so "is there more" is measured rather than guessed. Asking for
        // exactly the limit cannot distinguish a trail that ends there from one that is cut there.
        List<AuditLogEntity> found = await dbContext.AuditLog
            .AsNoTracking()
            .Where(row => row.TargetType == normalizedTargetType
                && row.TargetId == normalizedTargetId)
            .OrderByDescending(row => row.CreatedAt)
            .ThenByDescending(row => row.AuditId)
            .Take(normalizedLimit + 1)
            .ToListAsync(cancellationToken);

        bool truncated = found.Count > normalizedLimit;
        List<AuditEvidenceRowView> rows = found
            .Take(normalizedLimit)
            .Select(ToView)
            .ToList();

        AuditLogEntry access = await auditLogger.AppendAsync(
            new AuditEvent(
                actorId,
                AccessAuditAction,
                $"{normalizedTargetType}:{normalizedTargetId}",
                normalizedReason,
                correlationId,
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["target_type"] = normalizedTargetType,
                    ["target_id"] = normalizedTargetId,
                    ["limit"] = normalizedLimit,
                    ["row_count"] = rows.Count,
                    ["truncated"] = truncated,
                }),
            cancellationToken);

        return new AuditEvidenceApiResult(
            normalizedTargetType,
            normalizedTargetId,
            actorId,
            normalizedReason,
            correlationId,
            access.Id.ToString("D"),
            normalizedLimit,
            truncated,
            rows);
    }

    private static AuditEvidenceRowView ToView(AuditLogEntity row) => new(
        row.AuditId.ToString("D"),
        row.ActorId,
        row.ActorType,
        row.Action,
        row.TargetType,
        row.TargetId,
        row.Reason,
        row.CorrelationId,
        row.DataJson,
        row.BeforeStateJson,
        row.AfterStateJson,
        row.CreatedAt);

    /// <summary>
    /// Both selectors are required. A call with neither would return the newest rows across every
    /// object in the system -- a bulk extract of the audit trail wearing the clothes of a lookup --
    /// so the absence of a filter is refused rather than defaulted.
    /// </summary>
    private static string RequireIdentifier(string? value, string field)
    {
        string trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw IvrErrors.MalformedRequest($"{field} is required.");
        }

        if (trimmed.Length > 200)
        {
            throw IvrErrors.MalformedRequest($"{field} is too long.");
        }

        // A target id is an identifier the system issued, never free text a person typed, so
        // anything that looks like contact detail here means the caller is searching by the wrong
        // thing -- refuse before it reaches the query and before it reaches the audit row.
        return EnsureNoContactDetail(trimmed);
    }

    private static string RequireReason(string? reason)
    {
        string trimmed = (reason ?? string.Empty).Trim();
        if (trimmed.Length < MinReasonLength)
        {
            throw IvrErrors.MalformedRequest(
                $"reason is required and must be at least {MinReasonLength} characters.");
        }

        if (trimmed.Length > 200)
        {
            throw IvrErrors.MalformedRequest("reason is too long.");
        }

        return EnsureNoContactDetail(trimmed);
    }

    /// <summary>
    /// Translates <c>PiiGuard</c>'s refusal into the published error code.
    /// </summary>
    /// <remarks>
    /// The guard signals by throwing <see cref="InvalidOperationException"/>, which is not an
    /// API-06 code. Left unhandled it reaches the caller as <c>500 IVR_INTERNAL_ERROR</c> — a code
    /// producers are allowed to retry — for a request that will never be valid no matter how many
    /// times it is sent. That is the same defect W-0302 fixed on the intake path, and it would
    /// have shipped here too: a test caught it, not review. <c>PiiMaskingFilter</c> already
    /// converts the same exception the same way.
    /// </remarks>
    private static string EnsureNoContactDetail(string value)
    {
        try
        {
            PiiGuard.EnsureSafeText(value);
        }
        catch (InvalidOperationException)
        {
            throw IvrErrors.PiiPolicyViolation();
        }

        return value;
    }

    private static int NormalizeLimit(int? limit)
    {
        if (limit is null)
        {
            return DefaultLimit;
        }

        if (limit < 1 || limit > MaxLimit)
        {
            throw IvrErrors.MalformedRequest($"limit must be between 1 and {MaxLimit}.");
        }

        return limit.Value;
    }
}

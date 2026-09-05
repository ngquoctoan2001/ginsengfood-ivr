using System.Data;
using System.Collections.Immutable;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Privacy;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Ivr.Infrastructure.Scripts;

public sealed class PostgresScriptRegistry
    : IScriptRegistry, IScriptContentManager, IScriptVersionReader
{
    private readonly IDbContextFactory<IvrDbContext> dbContextFactory;
    private readonly TimeProvider timeProvider;
    private readonly bool productionTargetV1FieldsApproved;

    public PostgresScriptRegistry(
        IDbContextFactory<IvrDbContext> dbContextFactory,
        TimeProvider timeProvider,
        IOptions<ScriptContentOptions> options)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(options);
        this.dbContextFactory = dbContextFactory;
        this.timeProvider = timeProvider;
        productionTargetV1FieldsApproved = options.Value.ProductionTargetV1FieldsApproved;
    }

    public async ValueTask<ApprovedScript?> TryGetApproved(
        string templateId,
        string version,
        ExecutionMode mode,
        CancellationToken cancellationToken = default)
    {
        ScriptVersionKey key = ScriptVersionKey.Create(templateId, version);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        ScriptVersionEntity? entity = await context.ScriptVersions
            .AsNoTracking()
            .Include(candidate => candidate.Approvals)
            .SingleOrDefaultAsync(
                candidate => candidate.TemplateId == key.TemplateId
                    && candidate.Version == key.Version,
                cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return null;
        }

        ScriptVersionSnapshot snapshot = ScriptEntityMapper.ToSnapshot(entity);
        return ScriptApprovalPolicy.Allows(snapshot, mode, productionTargetV1FieldsApproved)
            ? new ApprovedScript(snapshot, mode)
            : null;
    }

    public async ValueTask<ScriptVersionSnapshot?> TryGetSnapshotAsync(
        ScriptVersionKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        ScriptVersionEntity? entity = await context.ScriptVersions
            .AsNoTracking()
            .Include(candidate => candidate.Approvals)
            .SingleOrDefaultAsync(
                candidate => candidate.TemplateId == key.TemplateId
                    && candidate.Version == key.Version,
                cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : ScriptEntityMapper.ToSnapshot(entity);
    }

    public async ValueTask<ScriptVersionSnapshot> CreateDraftAsync(
        ScriptDraftDefinition definition,
        ScriptActor actor,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(actor);
        actor.Demand(ScriptPermissions.Edit);
        string safeReason = ScriptTextGuard.Required(reason, 500, nameof(reason));
        string safeCorrelation = ScriptTextGuard.Identifier(correlationId, 120, nameof(correlationId));
        DateTimeOffset now = timeProvider.GetUtcNow();
        ScriptVersionEntity entity = new()
        {
            Id = Guid.NewGuid(),
            TemplateId = definition.Key.TemplateId,
            Version = definition.Key.Version,
            Status = ScriptEntityMapper.StatusToStorage(ScriptLifecycleStatus.Draft),
            TemplateText = definition.TemplateText,
            TemplateHash = definition.TemplateHash,
            AllowedInputFieldsJson = JsonSerializer.Serialize(TargetV1SpeechPolicy.AllowedInputFields),
            CreatedBy = actor.ActorId,
            CreateReason = safeReason,
            CreatedAt = now,
        };

        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            .ConfigureAwait(false);
        context.ScriptVersions.Add(entity);
        AppendAudit(context, actor, "ADMIN_SCRIPT_DRAFT_CREATED", entity, safeReason, safeCorrelation, null, now);
        try
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Duplicate immutable version is a client state conflict, not an opaque HTTP 500.
            // Do not expose PostgreSQL detail (which may contain the supplied script text).
            throw new InvalidOperationException("The script version already exists.");
        }
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return ScriptEntityMapper.ToSnapshot(entity);
    }

    public async ValueTask<ScriptVersionSnapshot> SubmitForReviewAsync(
        ScriptVersionKey key,
        ScriptActor actor,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(actor);
        actor.Demand(ScriptPermissions.Review);
        string safeReason = ScriptTextGuard.Required(reason, 500, nameof(reason));
        string safeCorrelation = ScriptTextGuard.Identifier(correlationId, 120, nameof(correlationId));

        return await MutateAsync(
            key,
            actor,
            safeReason,
            safeCorrelation,
            "ADMIN_SCRIPT_REVIEW_SUBMITTED",
            (_, entity, now) =>
            {
                if (ScriptEntityMapper.StatusFromStorage(entity.Status) != ScriptLifecycleStatus.Draft)
                {
                    throw new InvalidOperationException("Only a draft script can enter review.");
                }

                entity.Status = ScriptEntityMapper.StatusToStorage(ScriptLifecycleStatus.InReview);
                entity.SubmittedBy = actor.ActorId;
                entity.SubmitReason = safeReason;
                entity.SubmittedAt = now;
                return null;
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<ScriptVersionSnapshot> ApproveAsync(
        ScriptVersionKey key,
        ScriptApprovalType approvalType,
        ScriptActor actor,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(actor);
        actor.Demand(ScriptApprovalPolicy.PermissionFor(approvalType));
        string safeReason = ScriptTextGuard.Required(reason, 500, nameof(reason));
        string safeCorrelation = ScriptTextGuard.Identifier(correlationId, 120, nameof(correlationId));

        return await MutateAsync(
            key,
            actor,
            safeReason,
            safeCorrelation,
            "ADMIN_SCRIPT_APPROVED",
            (context, entity, now) =>
            {
                ScriptVersionSnapshot current = ScriptEntityMapper.ToSnapshot(entity);
                ScriptApprovalPolicy.EnsureApprovalAllowed(current, approvalType, actor);
                entity.Status = ScriptEntityMapper.StatusToStorage(ScriptLifecycleStatus.Approved);
                context.ScriptApprovals.Add(new ScriptApprovalEntity
                {
                    Id = Guid.NewGuid(),
                    ScriptVersionId = entity.Id,
                    ApprovalType = ScriptEntityMapper.ApprovalToStorage(approvalType),
                    ActorId = actor.ActorId,
                    Reason = safeReason,
                    CorrelationId = safeCorrelation,
                    ApprovedAt = now,
                    ScriptVersion = entity,
                });
                return approvalType;
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<ScriptVersionSnapshot> RetireAsync(
        ScriptVersionKey key,
        ScriptActor actor,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(actor);
        actor.Demand(ScriptPermissions.Retire);
        string safeReason = ScriptTextGuard.Required(reason, 500, nameof(reason));
        string safeCorrelation = ScriptTextGuard.Identifier(correlationId, 120, nameof(correlationId));

        return await MutateAsync(
            key,
            actor,
            safeReason,
            safeCorrelation,
            "ADMIN_SCRIPT_RETIRED",
            (_, entity, now) =>
            {
                ScriptLifecycleStatus status = ScriptEntityMapper.StatusFromStorage(entity.Status);
                if (status is ScriptLifecycleStatus.Draft or ScriptLifecycleStatus.Retired)
                {
                    throw new InvalidOperationException("Only reviewed or approved script versions can be retired.");
                }

                entity.Status = ScriptEntityMapper.StatusToStorage(ScriptLifecycleStatus.Retired);
                entity.RetiredBy = actor.ActorId;
                entity.RetireReason = safeReason;
                entity.RetiredAt = now;
                return null;
            },
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<ScriptVersionSnapshot> MutateAsync(
        ScriptVersionKey key,
        ScriptActor actor,
        string reason,
        string correlationId,
        string action,
        Func<IvrDbContext, ScriptVersionEntity, DateTimeOffset, ScriptApprovalType?> mutation,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            .ConfigureAwait(false);
        ScriptVersionEntity entity = await context.ScriptVersions
            .Include(candidate => candidate.Approvals)
            .SingleOrDefaultAsync(
                candidate => candidate.TemplateId == key.TemplateId
                    && candidate.Version == key.Version,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The script version was not found.");
        DateTimeOffset now = timeProvider.GetUtcNow();

        // Captured before the mutation runs, not read back after: the entity is tracked, so by
        // the time the audit row is built its Status is already the new one.
        string previousStatus = entity.Status;
        ScriptApprovalType? approvalType = mutation(context, entity, now);
        AppendAudit(
            context,
            actor,
            action,
            entity,
            reason,
            correlationId,
            approvalType,
            now,
            previousStatus);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return ScriptEntityMapper.ToSnapshot(entity);
    }

    private static void AppendAudit(
        IvrDbContext context,
        ScriptActor actor,
        string action,
        ScriptVersionEntity version,
        string reason,
        string correlationId,
        ScriptApprovalType? approvalType,
        DateTimeOffset now,
        string? previousStatus = null)
    {
        var data = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["template_id"] = version.TemplateId,
            ["version"] = version.Version,
            // Before and after, not just after. An audit row saying a version is APPROVED does
            // not say whether that press was the one that approved it or a later no-op, and
            // "who moved it out of review" is the question a sign-off review actually asks.
            ["previous_status"] = previousStatus,
            ["status"] = version.Status,
            ["template_hash"] = version.TemplateHash,
            ["approval_type"] = approvalType?.ToString(),
        };
        string dataJson = JsonSerializer.Serialize(data);
        PiiGuard.EnsureSafeText(dataJson);
        context.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = actor.ActorId,
            ActorType = "admin",
            Action = action,
            TargetType = "SCRIPT_VERSION",
            TargetId = string.Concat(version.TemplateId, ":", version.Version),
            Reason = reason,
            CorrelationId = correlationId,
            DataJson = dataJson,
            CreatedAt = now,
        });
    }
}

internal static class ScriptEntityMapper
{
    public static ScriptVersionSnapshot ToSnapshot(ScriptVersionEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        string[] allowedFields = JsonSerializer.Deserialize<string[]>(entity.AllowedInputFieldsJson)
            ?? throw new InvalidOperationException("Script allowed fields are invalid.");
        ImmutableArray<ScriptApprovalSnapshot> approvals = entity.Approvals
            .OrderBy(approval => approval.ApprovedAt)
            .Select(approval => new ScriptApprovalSnapshot(
                ApprovalFromStorage(approval.ApprovalType),
                approval.ActorId,
                approval.Reason,
                approval.CorrelationId,
                approval.ApprovedAt))
            .ToImmutableArray();
        return new ScriptVersionSnapshot(
            ScriptVersionKey.Create(entity.TemplateId, entity.Version),
            StatusFromStorage(entity.Status),
            TargetV1SpeechPolicy.ValidateTemplate(entity.TemplateText),
            entity.TemplateHash,
            allowedFields.ToImmutableArray(),
            approvals,
            entity.CreatedBy,
            entity.CreateReason,
            entity.CreatedAt,
            entity.SubmittedBy,
            entity.SubmitReason,
            entity.SubmittedAt,
            entity.RetiredBy,
            entity.RetireReason,
            entity.RetiredAt);
    }

    public static string StatusToStorage(ScriptLifecycleStatus status) => status switch
    {
        ScriptLifecycleStatus.Draft => "DRAFT",
        ScriptLifecycleStatus.InReview => "IN_REVIEW",
        ScriptLifecycleStatus.Approved => "APPROVED",
        ScriptLifecycleStatus.Retired => "RETIRED",
        _ => throw new InvalidOperationException("Unknown script status."),
    };

    public static ScriptLifecycleStatus StatusFromStorage(string status) => status switch
    {
        "DRAFT" => ScriptLifecycleStatus.Draft,
        "IN_REVIEW" => ScriptLifecycleStatus.InReview,
        "APPROVED" => ScriptLifecycleStatus.Approved,
        "RETIRED" => ScriptLifecycleStatus.Retired,
        _ => throw new InvalidOperationException("Unknown persisted script status."),
    };

    public static string ApprovalToStorage(ScriptApprovalType approvalType) => approvalType switch
    {
        ScriptApprovalType.MockTest => "MOCK_TEST",
        ScriptApprovalType.Lab => "LAB",
        ScriptApprovalType.Content => "CONTENT",
        ScriptApprovalType.PrivacyLegal => "PRIVACY_LEGAL",
        _ => throw new InvalidOperationException("Unknown script approval type."),
    };

    private static ScriptApprovalType ApprovalFromStorage(string approvalType) => approvalType switch
    {
        "MOCK_TEST" => ScriptApprovalType.MockTest,
        "LAB" => ScriptApprovalType.Lab,
        "CONTENT" => ScriptApprovalType.Content,
        "PRIVACY_LEGAL" => ScriptApprovalType.PrivacyLegal,
        _ => throw new InvalidOperationException("Unknown persisted script approval type."),
    };
}

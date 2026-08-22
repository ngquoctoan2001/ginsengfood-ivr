using Ivr.Domain.Confirmation;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Audit;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Scripts;

public sealed class InMemoryScriptRegistry : IScriptRegistry, IScriptContentManager, IDisposable
{
    private readonly Dictionary<string, ScriptVersionSnapshot> versions = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly IAuditLogger auditLogger;
    private readonly TimeProvider timeProvider;
    private readonly bool productionTargetV1FieldsApproved;

    public InMemoryScriptRegistry(
        IAuditLogger auditLogger,
        TimeProvider timeProvider,
        IOptions<ScriptContentOptions> options)
    {
        ArgumentNullException.ThrowIfNull(auditLogger);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(options);
        this.auditLogger = auditLogger;
        this.timeProvider = timeProvider;
        productionTargetV1FieldsApproved = options.Value.ProductionTargetV1FieldsApproved;
        SeedMockTemplate();
    }

    public async ValueTask<ApprovedScript?> TryGetApproved(
        string templateId,
        string version,
        ExecutionMode mode,
        CancellationToken cancellationToken = default)
    {
        ScriptVersionKey key = ScriptVersionKey.Create(templateId, version);
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return versions.TryGetValue(key.ToString(), out ScriptVersionSnapshot? snapshot)
                && ScriptApprovalPolicy.Allows(snapshot, mode, productionTargetV1FieldsApproved)
                    ? new ApprovedScript(snapshot, mode)
                    : null;
        }
        finally
        {
            gate.Release();
        }
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
        string safeReason = RequireReason(reason);
        string safeCorrelation = RequireCorrelation(correlationId);

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (versions.ContainsKey(definition.Key.ToString()))
            {
                throw new InvalidOperationException("The script version already exists.");
            }

            ScriptVersionSnapshot snapshot = new(
                definition.Key,
                ScriptLifecycleStatus.Draft,
                definition.TemplateText,
                definition.TemplateHash,
                TargetV1SpeechPolicy.AllowedInputFields,
                [],
                actor.ActorId,
                safeReason,
                timeProvider.GetUtcNow(),
                null,
                null,
                null,
                null,
                null,
                null);
            await AppendAuditAsync(
                actor,
                "ADMIN_SCRIPT_DRAFT_CREATED",
                snapshot,
                safeReason,
                safeCorrelation,
                null,
                cancellationToken).ConfigureAwait(false);
            versions.Add(definition.Key.ToString(), snapshot);
            return snapshot;
        }
        finally
        {
            gate.Release();
        }
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
        string safeReason = RequireReason(reason);
        string safeCorrelation = RequireCorrelation(correlationId);

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ScriptVersionSnapshot current = GetRequired(key);
            if (current.Status != ScriptLifecycleStatus.Draft)
            {
                throw new InvalidOperationException("Only a draft script can enter review.");
            }

            ScriptVersionSnapshot updated = current with
            {
                Status = ScriptLifecycleStatus.InReview,
                SubmittedBy = actor.ActorId,
                SubmitReason = safeReason,
                SubmittedAt = timeProvider.GetUtcNow(),
            };
            await AppendAuditAsync(
                actor,
                "ADMIN_SCRIPT_REVIEW_SUBMITTED",
                updated,
                safeReason,
                safeCorrelation,
                null,
                cancellationToken).ConfigureAwait(false);
            versions[key.ToString()] = updated;
            return updated;
        }
        finally
        {
            gate.Release();
        }
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
        string safeReason = RequireReason(reason);
        string safeCorrelation = RequireCorrelation(correlationId);

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ScriptVersionSnapshot current = GetRequired(key);
            EnsureApprovalAllowed(current, approvalType, actor);
            ScriptApprovalSnapshot approval = new(
                approvalType,
                actor.ActorId,
                safeReason,
                safeCorrelation,
                timeProvider.GetUtcNow());
            ScriptVersionSnapshot updated = current with
            {
                Status = ScriptLifecycleStatus.Approved,
                Approvals = [.. current.Approvals, approval],
            };
            await AppendAuditAsync(
                actor,
                "ADMIN_SCRIPT_APPROVED",
                updated,
                safeReason,
                safeCorrelation,
                approvalType,
                cancellationToken).ConfigureAwait(false);
            versions[key.ToString()] = updated;
            return updated;
        }
        finally
        {
            gate.Release();
        }
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
        string safeReason = RequireReason(reason);
        string safeCorrelation = RequireCorrelation(correlationId);

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ScriptVersionSnapshot current = GetRequired(key);
            if (current.Status is ScriptLifecycleStatus.Draft or ScriptLifecycleStatus.Retired)
            {
                throw new InvalidOperationException("Only reviewed or approved script versions can be retired.");
            }

            ScriptVersionSnapshot updated = current with
            {
                Status = ScriptLifecycleStatus.Retired,
                RetiredBy = actor.ActorId,
                RetireReason = safeReason,
                RetiredAt = timeProvider.GetUtcNow(),
            };
            await AppendAuditAsync(
                actor,
                "ADMIN_SCRIPT_RETIRED",
                updated,
                safeReason,
                safeCorrelation,
                null,
                cancellationToken).ConfigureAwait(false);
            versions[key.ToString()] = updated;
            return updated;
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose() => gate.Dispose();

    private static string RequireReason(string reason) =>
        ScriptTextGuard.Required(reason, 500, nameof(reason));

    private static string RequireCorrelation(string correlationId) =>
        ScriptTextGuard.Identifier(correlationId, 120, nameof(correlationId));

    private static void EnsureApprovalAllowed(
        ScriptVersionSnapshot current,
        ScriptApprovalType approvalType,
        ScriptActor actor)
    {
        if (current.Status is not (ScriptLifecycleStatus.InReview or ScriptLifecycleStatus.Approved))
        {
            throw new InvalidOperationException("Only a reviewed script version can be approved.");
        }

        if (string.Equals(current.CreatedBy, actor.ActorId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The script creator cannot approve the same version.");
        }

        if (current.Approvals.Any(approval => approval.Type == approvalType))
        {
            throw new InvalidOperationException("The script approval type already exists.");
        }

        if (approvalType is ScriptApprovalType.Content or ScriptApprovalType.PrivacyLegal
            && current.Approvals.Any(approval =>
                approval.Type is ScriptApprovalType.Content or ScriptApprovalType.PrivacyLegal
                && string.Equals(approval.ActorId, actor.ActorId, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Content and Privacy/Legal approvals require different actors.");
        }
    }

    private ScriptVersionSnapshot GetRequired(ScriptVersionKey key) =>
        versions.TryGetValue(key.ToString(), out ScriptVersionSnapshot? snapshot)
            ? snapshot
            : throw new KeyNotFoundException("The script version was not found.");

    private Task<AuditLogEntry> AppendAuditAsync(
        ScriptActor actor,
        string action,
        ScriptVersionSnapshot version,
        string reason,
        string correlationId,
        ScriptApprovalType? approvalType,
        CancellationToken cancellationToken) =>
        auditLogger.AppendAsync(
            new AuditEvent(
                actor.ActorId,
                action,
                version.Key.ToString(),
                reason,
                correlationId,
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["template_id"] = version.Key.TemplateId,
                    ["version"] = version.Key.Version,
                    ["status"] = version.Status.ToString(),
                    ["template_hash"] = version.TemplateHash,
                    ["approval_type"] = approvalType?.ToString(),
                }),
            cancellationToken);

    private void SeedMockTemplate()
    {
        AddMockTemplate(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        AddMockTemplate(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.LegacyMockTemplateVersion,
            TargetV1SpeechPolicy.LegacyVietnameseTemplate);
    }

    private void AddMockTemplate(string templateId, string version, string templateText)
    {
        ScriptDraftDefinition definition = ScriptDraftDefinition.Create(
            templateId,
            version,
            templateText);
        DateTimeOffset seededAt = new(2026, 8, 12, 0, 0, 0, TimeSpan.Zero);
        versions.Add(
            definition.Key.ToString(),
            new ScriptVersionSnapshot(
                definition.Key,
                ScriptLifecycleStatus.Approved,
                definition.TemplateText,
                definition.TemplateHash,
                TargetV1SpeechPolicy.AllowedInputFields,
                [
                    new ScriptApprovalSnapshot(
                        ScriptApprovalType.MockTest,
                        "fixture-reviewer",
                        "W-0024 synthetic MOCK fixture only",
                        "W-0024-SEED",
                        seededAt),
                ],
                "fixture-author",
                "W-0024 synthetic MOCK fixture only",
                seededAt,
                "fixture-reviewer",
                "W-0024 synthetic MOCK fixture review",
                seededAt,
                null,
                null,
                null));
    }
}

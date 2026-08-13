using System.Collections.Immutable;
using System.Text;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Privacy;

namespace Ivr.Domain.Scripts;

public enum ScriptLifecycleStatus
{
    Draft,
    InReview,
    Approved,
    Retired,
}

public enum ScriptApprovalType
{
    MockTest,
    Lab,
    Content,
    PrivacyLegal,
}

public static class ScriptPermissions
{
    public const string Edit = "ivr.script.edit";
    public const string Review = "ivr.script.review";
    public const string ApproveMock = "ivr.script.approve.mock";
    public const string ApproveLab = "ivr.script.approve.lab";
    public const string ApproveContent = "ivr.script.approve.content";
    public const string ApprovePrivacyLegal = "ivr.script.approve.privacy-legal";
    public const string Retire = "ivr.script.retire";
}

public sealed class ScriptActor
{
    private ScriptActor(string actorId, ImmutableHashSet<string> permissions)
    {
        ActorId = actorId;
        Permissions = permissions;
    }

    public string ActorId { get; }

    public IReadOnlySet<string> Permissions { get; }

    public static ScriptActor Create(string actorId, IEnumerable<string> permissions)
    {
        string safeActorId = ScriptTextGuard.Required(actorId, 120, nameof(actorId));
        ArgumentNullException.ThrowIfNull(permissions);
        ImmutableHashSet<string> safePermissions = permissions
            .Select(permission => ScriptTextGuard.Required(permission, 120, nameof(permissions)))
            .ToImmutableHashSet(StringComparer.Ordinal);
        return new ScriptActor(safeActorId, safePermissions);
    }

    public void Demand(string permission)
    {
        if (!Permissions.Contains(permission))
        {
            throw new UnauthorizedAccessException("The actor lacks the required script permission.");
        }
    }
}

public sealed record ScriptVersionKey
{
    private ScriptVersionKey(string templateId, string version)
    {
        TemplateId = templateId;
        Version = version;
    }

    public string TemplateId { get; }

    public string Version { get; }

    public static ScriptVersionKey Create(string templateId, string version) =>
        new(
            ScriptTextGuard.Identifier(templateId, 120, nameof(templateId)),
            ScriptTextGuard.Identifier(version, 40, nameof(version)));

    public override string ToString() => $"{TemplateId}:{Version}";
}

public sealed record ScriptApprovalSnapshot(
    ScriptApprovalType Type,
    string ActorId,
    string Reason,
    string CorrelationId,
    DateTimeOffset ApprovedAt);

public sealed record ScriptVersionSnapshot(
    ScriptVersionKey Key,
    ScriptLifecycleStatus Status,
    string TemplateText,
    string TemplateHash,
    ImmutableArray<string> AllowedInputFields,
    ImmutableArray<ScriptApprovalSnapshot> Approvals,
    string CreatedBy,
    string CreateReason,
    DateTimeOffset CreatedAt,
    string? SubmittedBy,
    string? SubmitReason,
    DateTimeOffset? SubmittedAt,
    string? RetiredBy,
    string? RetireReason,
    DateTimeOffset? RetiredAt)
{
    public bool UsesProductionDecisionFields =>
        TargetV1SpeechPolicy.UsesProductionDecisionFields(TemplateText);
}

public sealed record ApprovedScript(ScriptVersionSnapshot Version, ExecutionMode ExecutionMode);

public sealed record ScriptDraftDefinition
{
    private ScriptDraftDefinition(ScriptVersionKey key, string templateText)
    {
        Key = key;
        TemplateText = templateText;
        TemplateHash = DeterministicSnapshotHasher.Compute(templateText);
    }

    public ScriptVersionKey Key { get; }

    public string TemplateText { get; }

    public string TemplateHash { get; }

    public static ScriptDraftDefinition Create(string templateId, string version, string templateText)
    {
        string safeTemplate = TargetV1SpeechPolicy.ValidateTemplate(templateText);
        return new ScriptDraftDefinition(
            ScriptVersionKey.Create(templateId, version),
            safeTemplate);
    }
}

public interface IScriptRegistry
{
    public ValueTask<ApprovedScript?> TryGetApproved(
        string templateId,
        string version,
        ExecutionMode mode,
        CancellationToken cancellationToken = default);
}

public interface IScriptContentManager
{
    public ValueTask<ScriptVersionSnapshot> CreateDraftAsync(
        ScriptDraftDefinition definition,
        ScriptActor actor,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default);

    public ValueTask<ScriptVersionSnapshot> SubmitForReviewAsync(
        ScriptVersionKey key,
        ScriptActor actor,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default);

    public ValueTask<ScriptVersionSnapshot> ApproveAsync(
        ScriptVersionKey key,
        ScriptApprovalType approvalType,
        ScriptActor actor,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default);

    public ValueTask<ScriptVersionSnapshot> RetireAsync(
        ScriptVersionKey key,
        ScriptActor actor,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default);
}

public static class ScriptApprovalPolicy
{
    public static bool Allows(
        ScriptVersionSnapshot version,
        ExecutionMode mode,
        bool productionDecisionFieldsApproved)
    {
        ArgumentNullException.ThrowIfNull(version);
        if (!Enum.IsDefined(mode) || version.Status != ScriptLifecycleStatus.Approved)
        {
            return false;
        }

        ImmutableArray<ScriptApprovalSnapshot> approvals = version.Approvals;
        return mode switch
        {
            ExecutionMode.Mock => approvals.Any(approval => approval.Type == ScriptApprovalType.MockTest),
            ExecutionMode.LabRealSim => approvals.Any(approval => approval.Type == ScriptApprovalType.Lab),
            ExecutionMode.ProductionReal => ProductionAllows(
                version,
                approvals,
                productionDecisionFieldsApproved),
            _ => false,
        };
    }

    public static string PermissionFor(ScriptApprovalType approvalType) => approvalType switch
    {
        ScriptApprovalType.MockTest => ScriptPermissions.ApproveMock,
        ScriptApprovalType.Lab => ScriptPermissions.ApproveLab,
        ScriptApprovalType.Content => ScriptPermissions.ApproveContent,
        ScriptApprovalType.PrivacyLegal => ScriptPermissions.ApprovePrivacyLegal,
        _ => throw new InvalidOperationException("Unknown script approval type."),
    };

    private static bool ProductionAllows(
        ScriptVersionSnapshot version,
        ImmutableArray<ScriptApprovalSnapshot> approvals,
        bool productionDecisionFieldsApproved)
    {
        ScriptApprovalSnapshot? content = approvals.LastOrDefault(
            approval => approval.Type == ScriptApprovalType.Content);
        ScriptApprovalSnapshot? privacy = approvals.LastOrDefault(
            approval => approval.Type == ScriptApprovalType.PrivacyLegal);
        return content is not null
            && privacy is not null
            && !string.Equals(content.ActorId, privacy.ActorId, StringComparison.Ordinal)
            && (!version.UsesProductionDecisionFields || productionDecisionFieldsApproved);
    }
}

public static class ScriptTextGuard
{
    public static string Required(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A required script field was empty.", parameterName);
        }

        string normalized = value.Trim().Normalize(NormalizationForm.FormC);
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        PiiGuard.EnsureSafeText(normalized);
        return normalized;
    }

    public static string Identifier(string value, int maximumLength, string parameterName)
    {
        string safeValue = Required(value, maximumLength, parameterName);
        if (safeValue.Any(character =>
                !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.')))
        {
            throw new ArgumentException("A script identifier contains an unsupported character.", parameterName);
        }

        return safeValue;
    }
}

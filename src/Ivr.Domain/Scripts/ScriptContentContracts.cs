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

/// <summary>
/// Reads one script version whatever state it is in.
/// <para>
/// Separate from <see cref="IScriptRegistry"/>, which answers "may this be spoken in this mode"
/// and returns nothing for a draft — the right answer for the dial path and the wrong one for a
/// console that has to show an operator the draft they just created.
/// </para>
/// <para>
/// Separate from <see cref="IScriptContentManager"/> too, rather than a method added to it. That
/// interface is the mutation port the worker and intake bind to; widening it to carry a read
/// would put a read on every implementation of a write contract, for one caller's benefit.
/// </para>
/// </summary>
public interface IScriptVersionReader
{
    public ValueTask<ScriptVersionSnapshot?> TryGetSnapshotAsync(
        ScriptVersionKey key,
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

/// <summary>
/// An approval refused because of <em>who</em> is pressing, not because of the version's state.
/// <para>
/// A subclass rather than a new exception type so every existing catch of
/// <see cref="InvalidOperationException"/> keeps behaving as it did. The distinction earns its
/// keep at the API boundary: "the state is wrong" is a 409 the caller can wait out, while "you
/// are not the one who may press this" is a 403 that stays true no matter how long they wait,
/// and answering 409 to it sends an operator to retry instead of to find a colleague.
/// </para>
/// </summary>
public sealed class ScriptApproverConflictException(string message)
    : InvalidOperationException(message);

public static class ScriptApprovalPolicy
{
    /// <summary>
    /// The four-eyes rule, in one place.
    /// <para>
    /// This lived as a byte-identical private copy in both the in-memory and the PostgreSQL
    /// registry. Two copies of a rule that decides who may approve customer-facing speech is a
    /// rule that will eventually be enforced in MOCK and not in production, or the reverse, and
    /// the difference would only surface as an approval that should not have been possible.
    /// </para>
    /// </summary>
    public static void EnsureApprovalAllowed(
        ScriptVersionSnapshot current,
        ScriptApprovalType approvalType,
        ScriptActor actor)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(actor);
        if (current.Status is not (ScriptLifecycleStatus.InReview or ScriptLifecycleStatus.Approved))
        {
            throw new InvalidOperationException("Only a reviewed script version can be approved.");
        }

        if (string.Equals(current.CreatedBy, actor.ActorId, StringComparison.Ordinal))
        {
            throw new ScriptApproverConflictException(
                "The script creator cannot approve the same version.");
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
            throw new ScriptApproverConflictException(
                "Content and Privacy/Legal approvals require different actors.");
        }
    }

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

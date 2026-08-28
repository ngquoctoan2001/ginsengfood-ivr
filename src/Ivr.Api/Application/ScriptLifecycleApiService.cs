using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Security.Claims;
using Ivr.Api.Admin;
using Ivr.Api.Auth;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Errors;
using Ivr.Domain.Scripts;
using Microsoft.Extensions.Options;
using Ivr.Infrastructure.Scripts;

namespace Ivr.Api.Application;

public interface IScriptLifecycleApiService
{
    public Task<ScriptVersionApiResult> GetAsync(
        string templateId,
        string version,
        CancellationToken cancellationToken);

    public Task<ScriptActionApiResult> CreateDraftAsync(
        ScriptDraftRequest request,
        ClaimsPrincipal principal,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken);

    public Task<ScriptActionApiResult> SubmitAsync(
        string templateId,
        string version,
        ScriptTransitionRequest request,
        ClaimsPrincipal principal,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken);

    public Task<ScriptActionApiResult> ApproveAsync(
        string templateId,
        string version,
        ScriptApprovalRequest request,
        ClaimsPrincipal principal,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken);

    public Task<ScriptActionApiResult> RetireAsync(
        string templateId,
        string version,
        ScriptTransitionRequest request,
        ClaimsPrincipal principal,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Console surface for the script lifecycle (W-0109).
/// <para>
/// W-0096 deliberately left these transitions unexposed, on the grounds that approval is an
/// owner action rather than a console button. That reasoning held while there was no way to
/// press the button at all — but the only remaining path for a Privacy/Legal signature was
/// editing rows by hand, which loses the audit record, loses "creator cannot approve", and
/// loses the meaning of the gate it was supposed to satisfy. Exposing it under the same
/// mutation frame as every other admin action puts the signature back inside the controls.
/// </para>
/// <para>
/// This class deliberately holds no policy of its own. Every rule — who may approve, in what
/// order, and whether a version is speakable — stays in
/// <see cref="IScriptContentManager"/> and <see cref="ScriptApprovalPolicy"/>, which are also
/// what the worker consults at dial time. A second copy of those rules here would be a second
/// answer to "is this script approved", and the two would drift.
/// </para>
/// </summary>
public sealed class ScriptLifecycleApiService(
    IScriptContentManager contentManager,
    IScriptVersionReader versionReader,
    IOptions<ScriptContentOptions> scriptOptions) : IScriptLifecycleApiService
{
    /// <summary>
    /// Console permission to domain permission. The route already refused the call without the
    /// console permission; this map exists so the actor handed to the domain carries only what
    /// the session actually holds, keeping <c>ScriptActor.Demand</c> a real second check rather
    /// than a formality that always passes.
    /// </summary>
    private static readonly FrozenDictionary<string, string> DomainPermissions =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [IvrPermissions.ScriptEdit] = ScriptPermissions.Edit,
            [IvrPermissions.ScriptReview] = ScriptPermissions.Review,
            [IvrPermissions.ScriptApproveMock] = ScriptPermissions.ApproveMock,
            [IvrPermissions.ScriptApproveLab] = ScriptPermissions.ApproveLab,
            [IvrPermissions.ScriptApproveContent] = ScriptPermissions.ApproveContent,
            [IvrPermissions.ScriptApprovePrivacyLegal] = ScriptPermissions.ApprovePrivacyLegal,
            [IvrPermissions.ScriptRetire] = ScriptPermissions.Retire,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    /// Wire forms, taken from the enum dictionary the console renders against
    /// (<c>admin-ui/src/i18n/enums.vi.json</c>), not from <c>Enum.ToString()</c>.
    /// <para>
    /// <c>ToString()</c> would emit <c>Approved</c> and <c>Mock</c>; the dictionary keys are
    /// <c>APPROVED</c> and <c>MOCK</c>. The mismatch would not fail anything server-side — it
    /// would surface as the console showing a warning badge next to an unknown code, which is
    /// exactly what W-0107's NT-4 is designed to make visible rather than silent.
    /// </para>
    /// </summary>
    private static readonly FrozenDictionary<ScriptLifecycleStatus, string> StatusWireForms =
        new Dictionary<ScriptLifecycleStatus, string>
        {
            [ScriptLifecycleStatus.Draft] = "DRAFT",
            [ScriptLifecycleStatus.InReview] = "IN_REVIEW",
            [ScriptLifecycleStatus.Approved] = "APPROVED",
            [ScriptLifecycleStatus.Retired] = "RETIRED",
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<ScriptApprovalType, string> ApprovalWireForms =
        new Dictionary<ScriptApprovalType, string>
        {
            [ScriptApprovalType.MockTest] = "MOCK_TEST",
            [ScriptApprovalType.Lab] = "LAB",
            [ScriptApprovalType.Content] = "CONTENT",
            [ScriptApprovalType.PrivacyLegal] = "PRIVACY_LEGAL",
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<ExecutionMode, string> ModeWireForms =
        new Dictionary<ExecutionMode, string>
        {
            [ExecutionMode.Mock] = "MOCK",
            [ExecutionMode.LabRealSim] = "LAB_REAL_SIM",
            [ExecutionMode.ProductionReal] = "PRODUCTION_REAL",
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, ScriptApprovalType> ApprovalTypes =
        new Dictionary<string, ScriptApprovalType>(StringComparer.Ordinal)
        {
            ["MOCK_TEST"] = ScriptApprovalType.MockTest,
            ["LAB"] = ScriptApprovalType.Lab,
            ["CONTENT"] = ScriptApprovalType.Content,
            ["PRIVACY_LEGAL"] = ScriptApprovalType.PrivacyLegal,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public async Task<ScriptVersionApiResult> GetAsync(
        string templateId,
        string version,
        CancellationToken cancellationToken)
    {
        ScriptVersionSnapshot? snapshot = await versionReader
            .TryGetSnapshotAsync(
                ScriptVersionKey.Create(templateId, version),
                cancellationToken)
            .ConfigureAwait(false);
        return snapshot is null
            ? throw IvrErrors.NotFound("The script version was not found.")
            : Project(snapshot);
    }

    public Task<ScriptActionApiResult> CreateDraftAsync(
        ScriptDraftRequest request,
        ClaimsPrincipal principal,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            "create-script-draft",
            correlationId,
            async () =>
            {
                ScriptDraftDefinition definition = ScriptDraftDefinition.Create(
                    request.TemplateId,
                    request.Version,
                    request.TemplateText);
                return await contentManager.CreateDraftAsync(
                    definition,
                    Actor(principal, actorId),
                    request.Reason,
                    correlationId,
                    cancellationToken).ConfigureAwait(false);
            });
    }

    public Task<ScriptActionApiResult> SubmitAsync(
        string templateId,
        string version,
        ScriptTransitionRequest request,
        ClaimsPrincipal principal,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            "submit-script-review",
            correlationId,
            async () => await contentManager.SubmitForReviewAsync(
                ScriptVersionKey.Create(templateId, version),
                Actor(principal, actorId),
                request.Reason,
                correlationId,
                cancellationToken).ConfigureAwait(false));
    }

    public Task<ScriptActionApiResult> ApproveAsync(
        string templateId,
        string version,
        ScriptApprovalRequest request,
        ClaimsPrincipal principal,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ApprovalType is null
            || !ApprovalTypes.TryGetValue(request.ApprovalType, out ScriptApprovalType approvalType))
        {
            throw IvrErrors.MalformedRequest("The approval type is not one of the four defined values.");
        }

        return ExecuteAsync(
            "approve-script",
            correlationId,
            async () => await contentManager.ApproveAsync(
                ScriptVersionKey.Create(templateId, version),
                approvalType,
                Actor(principal, actorId),
                request.Reason,
                correlationId,
                cancellationToken).ConfigureAwait(false));
    }

    public Task<ScriptActionApiResult> RetireAsync(
        string templateId,
        string version,
        ScriptTransitionRequest request,
        ClaimsPrincipal principal,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            "retire-script",
            correlationId,
            async () => await contentManager.RetireAsync(
                ScriptVersionKey.Create(templateId, version),
                Actor(principal, actorId),
                request.Reason,
                correlationId,
                cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// W-0122. Script approval keeps its segregation of duties, but the source of truth for who
    /// holds which approval moved out of IVR along with the console accounts.
    /// <para>
    /// Module 3 owns operator identity now, so it asserts the approvals its user carries in
    /// <c>X-Script-Permissions</c>; the authentication handler turns that header into claims and
    /// this method reads them. The domain rules are unchanged — <see cref="ScriptActor"/> still
    /// enforces that an actor cannot approve what it edited. What changed is only who is trusted
    /// to say what an actor holds.
    /// </para>
    /// </summary>
    private static ScriptActor Actor(ClaimsPrincipal principal, string actorId)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ImmutableArray<string> granted =
        [
            .. principal.FindAll(AdminTokenAuthenticationHandler.ScriptPermissionClaimType)
                .Select(claim => claim.Value)
                .Where(DomainPermissions.ContainsKey)
                .Select(permission => DomainPermissions[permission]),
        ];
        return ScriptActor.Create(actorId, granted);
    }

    /// <summary>
    /// Runs one transition and turns domain failures into the stable error envelope.
    /// <para>
    /// The domain signals refusal with plain exception types, and the mapping matters: a
    /// four-eyes refusal and a "creator cannot approve" refusal are both
    /// <see cref="InvalidOperationException"/>, and both are 409 rather than 403 — the caller
    /// holds the permission, the <em>state</em> is what refuses. Answering 403 would send an
    /// operator to look at their role when the actual answer is "someone else has to press it".
    /// </para>
    /// </summary>
    private async Task<ScriptActionApiResult> ExecuteAsync(
        string actionType,
        string correlationId,
        Func<Task<ScriptVersionSnapshot>> transition)
    {
        ScriptVersionSnapshot snapshot;
        try
        {
            snapshot = await transition().ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException)
        {
            throw IvrErrors.ForbiddenCaller();
        }
        catch (KeyNotFoundException)
        {
            throw IvrErrors.NotFound("The script version was not found.");
        }
        catch (ScriptApproverConflictException)
        {
            // Ordered before the InvalidOperationException catch because it is a subclass.
            // 403, not 409: the caller holds the permission but is the wrong person for this
            // particular version, and no amount of retrying changes that. A 409 would send them
            // to press again; the actual next step is to find a second approver.
            throw IvrErrors.ForbiddenCaller();
        }
        catch (InvalidOperationException exception)
        {
            // State refusals: approving a draft, retiring a draft, a duplicate approval type.
            // The domain writes these messages for operators and runs them through
            // ScriptTextGuard, so they carry no order data and are safe to return.
            throw new IvrFailureException(IvrErrorCodes.VersionConflict, exception.Message);
        }
        catch (ArgumentException exception)
        {
            throw IvrErrors.MalformedRequest(exception.Message);
        }

        return new ScriptActionApiResult(
            actionType,
            "script_version",
            snapshot.Key.ToString(),
            correlationId,
            true,
            Project(snapshot));
    }

    private ScriptVersionApiResult Project(ScriptVersionSnapshot snapshot)
    {
        bool productionFieldsApproved = scriptOptions.Value.ProductionTargetV1FieldsApproved;
        List<string> modes = [];
        foreach (ExecutionMode mode in Enum.GetValues<ExecutionMode>())
        {
            if (ScriptApprovalPolicy.Allows(snapshot, mode, productionFieldsApproved))
            {
                modes.Add(ModeWireForms[mode]);
            }
        }

        return new ScriptVersionApiResult(
            snapshot.Key.TemplateId,
            snapshot.Key.Version,
            StatusWireForms[snapshot.Status],
            snapshot.TemplateText,
            snapshot.TemplateHash,
            snapshot.AllowedInputFields,
            [
                .. snapshot.Approvals.Select(approval => new ScriptApprovalApiResult(
                    ApprovalWireForms[approval.Type],
                    approval.ActorId,
                    approval.Reason,
                    approval.CorrelationId,
                    approval.ApprovedAt)),
            ],
            snapshot.CreatedBy,
            snapshot.CreatedAt,
            snapshot.SubmittedBy,
            snapshot.SubmittedAt,
            snapshot.RetiredBy,
            snapshot.RetiredAt,
            snapshot.UsesProductionDecisionFields,
            modes,
            DescribeProductionBlock(snapshot, productionFieldsApproved));
    }

    /// <summary>
    /// Names the missing production precondition, in the order an operator would fix them.
    /// <para>
    /// Reporting an empty <c>approved_for_modes</c> and letting the reader work out why is how
    /// a console ends up with someone concluding the system is broken when the honest answer is
    /// "Privacy/Legal has not signed yet".
    /// </para>
    /// </summary>
    private static string? DescribeProductionBlock(
        ScriptVersionSnapshot snapshot,
        bool productionFieldsApproved)
    {
        if (snapshot.Status == ScriptLifecycleStatus.Retired)
        {
            return "The version is retired and fails closed in every mode.";
        }

        if (snapshot.Status != ScriptLifecycleStatus.Approved)
        {
            return "The version has no approval yet.";
        }

        ImmutableArray<ScriptApprovalSnapshot> approvals = snapshot.Approvals;
        bool hasContent = approvals.Any(a => a.Type == ScriptApprovalType.Content);
        bool hasPrivacy = approvals.Any(a => a.Type == ScriptApprovalType.PrivacyLegal);
        if (!hasContent && !hasPrivacy)
        {
            return "Production needs both a Content and a Privacy/Legal approval.";
        }

        if (!hasContent)
        {
            return "Production is waiting on the Content approval.";
        }

        if (!hasPrivacy)
        {
            return "Production is waiting on the Privacy/Legal approval.";
        }

        if (snapshot.UsesProductionDecisionFields && !productionFieldsApproved)
        {
            return "The speech field whitelist is unsigned (OD-V1-15), so production stays blocked.";
        }

        return null;
    }
}

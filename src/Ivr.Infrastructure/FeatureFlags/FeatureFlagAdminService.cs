using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Audit;

namespace Ivr.Infrastructure.FeatureFlags;

public sealed record FeatureFlagMutationCommand(
    string Environment,
    FeatureFlagChangeSet Changes,
    string Reason,
    string ActorId,
    string? ActorDestinationReference,
    string? ApprovalReference,
    string CorrelationId);

public sealed record FeatureFlagMutationResult(
    FeatureFlagSnapshot Snapshot,
    string? ApprovedBy,
    IReadOnlySet<string> IncreasedRiskKeys);

public sealed class FeatureFlagAdminService(
    IFeatureFlags featureFlags,
    IFeatureFlagStore store,
    IFeatureFlagRefresher refresher,
    IRuntimeGateAuthorization runtimeGateAuthorization,
    IFourEyesApprovalVerifier fourEyesApprovalVerifier)
    : IFeatureFlagAdminService
{
    public async Task<FeatureFlagMutationResult> MutateAsync(
        FeatureFlagMutationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);
        FeatureFlagReadResult read = await featureFlags.GetSnapshotAsync(
            command.Environment,
            true,
            cancellationToken);
        if (!read.ProviderReadable)
        {
            throw IvrErrors.OperationalBlocked("Feature flag provider is unavailable.");
        }

        FeatureFlagSnapshot before = read.Snapshot;
        FeatureFlagSnapshot after = before.Apply(command.Changes);
        IReadOnlySet<string> changedKeys = command.Changes.ChangedKeys();
        FeatureFlagGuardrails.ValidateAdminMutation(before, after);
        FeatureFlagRiskAssessment risk = FeatureFlagGuardrails.AssessRisk(before, after);
        bool unconditionalRiskReduction =
            FeatureFlagGuardrails.IsUnconditionalRiskReduction(
                before,
                after,
                changedKeys);
        // W-0195 / OD-V1-20. The authorization gate is asked here rather than at the top of the
        // method, and it is not asked at all for an unconditional risk reduction.
        //
        // W-0068 settled the asymmetry: engaging the kill switch, turning real customer calls off
        // and shrinking the allowlist must always be available, because they are the moves that
        // stop something happening. Asking the gate first inverted that — with the gate ungranted,
        // the API could not engage the kill switch either, so the one control that exists for an
        // emergency was the one an ungranted permission removed. Nothing about "we have not
        // approved runtime-gate administration yet" should mean "and therefore you may not stop
        // the calls".
        if (!unconditionalRiskReduction)
        {
            FeatureFlagGuardrails.ValidateEffective(after);
            if (!await runtimeGateAuthorization.IsApprovedAsync(cancellationToken))
            {
                throw IvrErrors.OperationalBlocked(
                    "Runtime gate administration is pending owner approval.");
            }
        }

        if (risk.IsRiskIncrease
            && command.Environment == FeatureFlagEnvironments.Production)
        {
            throw IvrErrors.PolicyMismatch(
                "Risk-increasing production changes require an approved deployment.");
        }

        string? approvedBy = await VerifyApprovalAsync(
            command,
            before,
            after,
            risk,
            cancellationToken);
        RejectSelfAuthorization(command, before, after);

        AuditEvent auditEvent = new(
            command.ActorId,
            "ADMIN_FEATURE_FLAGS_CHANGE",
            $"feature-flags:{command.Environment}",
            command.Reason,
            command.CorrelationId,
            new Dictionary<string, object?>
            {
                ["before"] = before,
                ["after"] = after,
                ["approvedBy"] = approvedBy,
                ["approvalReference"] = command.ApprovalReference,
                ["riskIncreaseKeys"] = risk.IncreasedRiskKeys,
            });
        FeatureFlagSnapshot stored = await store.ApplyAuditedAsync(
            before,
            after,
            auditEvent,
            cancellationToken);
        FeatureFlagReadResult refreshed = await refresher.RefreshAsync(
            command.Environment,
            cancellationToken);
        if (!refreshed.ProviderReadable || refreshed.Snapshot.Revision != stored.Revision)
        {
            throw IvrErrors.OperationalBlocked(
                "Feature flag propagation could not be verified.");
        }

        return new FeatureFlagMutationResult(stored, approvedBy, risk.IncreasedRiskKeys);
    }

    private static void ValidateCommand(FeatureFlagMutationCommand command)
    {
        if (!FeatureFlagEnvironments.All.Contains(command.Environment))
        {
            throw IvrErrors.MalformedRequest("Feature flag environment is invalid.");
        }

        if (command.Changes.ChangedKeys().Count == 0)
        {
            throw IvrErrors.MalformedRequest("At least one feature flag change is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            throw IvrErrors.MalformedRequest("A reason is required for feature flag changes.");
        }

        if (command.Reason.Length > 500)
        {
            throw IvrErrors.MalformedRequest(
                "Feature flag change reason must not exceed 500 characters.");
        }

        if (string.IsNullOrWhiteSpace(command.ActorId))
        {
            throw IvrErrors.ForbiddenCaller();
        }

        PiiGuard.EnsureSafeText(command.Reason);
        PiiGuard.EnsureSafeText(command.ActorId);
        PiiGuard.EnsureSafeText(command.ActorDestinationReference);
        PiiGuard.EnsureSafeText(command.ApprovalReference);
        PiiGuard.EnsureSafeText(command.CorrelationId);
    }

    private async Task<string?> VerifyApprovalAsync(
        FeatureFlagMutationCommand command,
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after,
        FeatureFlagRiskAssessment risk,
        CancellationToken cancellationToken)
    {
        if (!risk.IsRiskIncrease)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(command.ApprovalReference))
        {
            throw IvrErrors.PolicyMismatch(
                "A verified four-eyes approval is required for this change.");
        }

        string? approvedBy = await fourEyesApprovalVerifier.VerifyAsync(
            command.ApprovalReference,
            command.ActorId,
            before,
            after,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(approvedBy)
            || string.Equals(approvedBy, command.ActorId, StringComparison.Ordinal))
        {
            throw IvrErrors.PolicyMismatch(
                "A different verified approver is required for this change.");
        }

        PiiGuard.EnsureSafeText(approvedBy);
        return approvedBy;
    }

    private static void RejectSelfAuthorization(
        FeatureFlagMutationCommand command,
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after)
    {
        if (string.IsNullOrWhiteSpace(command.ActorDestinationReference))
        {
            return;
        }

        bool addedOwnDestination = !before.LabDestinationAllowlist.Contains(
                command.ActorDestinationReference)
            && after.LabDestinationAllowlist.Contains(command.ActorDestinationReference);
        if (addedOwnDestination)
        {
            throw IvrErrors.PolicyMismatch(
                "An actor cannot authorize its own call destination.");
        }
    }
}

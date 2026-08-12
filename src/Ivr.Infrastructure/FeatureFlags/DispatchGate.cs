using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Configuration;

namespace Ivr.Infrastructure.FeatureFlags;

public sealed record DispatchGateDecision(bool Allowed, string Reason);

public sealed class DispatchGate(
    IFeatureFlags featureFlags,
    IProductionCallGate productionCallGate,
    IRuntimeSafetyHealth runtimeSafetyHealth)
    : IDispatchGate
{
    public async Task<DispatchGateDecision> EvaluateAsync(
        string environment,
        string destinationReference,
        CancellationToken cancellationToken = default)
    {
        PiiGuard.EnsureSafeText(destinationReference);
        if (!await runtimeSafetyHealth.IsAuditProviderHealthyAsync(cancellationToken))
        {
            return new DispatchGateDecision(false, "AUDIT_PROVIDER_UNAVAILABLE");
        }

        FeatureFlagReadResult result = await featureFlags.GetSnapshotAsync(
            environment,
            true,
            cancellationToken);
        if (!result.ProviderReadable)
        {
            return new DispatchGateDecision(false, "CONFIG_PROVIDER_UNAVAILABLE");
        }

        FeatureFlagSnapshot snapshot = result.Snapshot;
        if (snapshot.GlobalDialKillSwitch)
        {
            return new DispatchGateDecision(false, "GLOBAL_KILL_SWITCH_ON");
        }

        try
        {
            FeatureFlagGuardrails.ValidateEffective(snapshot);
        }
        catch (IvrFailureException)
        {
            return new DispatchGateDecision(false, "INVALID_RUNTIME_CONFIG");
        }

        if (snapshot.ExecutionMode == IvrOptions.MockExecutionMode)
        {
            return new DispatchGateDecision(false, "MOCK_MODE");
        }

        if (!snapshot.LabDestinationAllowlist.Contains(destinationReference))
        {
            return new DispatchGateDecision(false, "DESTINATION_NOT_ALLOWLISTED");
        }

        if (snapshot.ExecutionMode == IvrOptions.LabRealSimExecutionMode)
        {
            return new DispatchGateDecision(true, "LAB_DESTINATION_APPROVED");
        }

        bool releaseApproved = await productionCallGate.IsApprovedAsync(cancellationToken);
        return releaseApproved
            ? new DispatchGateDecision(true, "PRODUCTION_RELEASE_APPROVED")
            : new DispatchGateDecision(false, "PRODUCTION_RELEASE_NOT_APPROVED");
    }
}

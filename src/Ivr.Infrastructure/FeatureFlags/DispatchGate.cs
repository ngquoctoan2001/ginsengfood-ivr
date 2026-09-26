using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Configuration;

namespace Ivr.Infrastructure.FeatureFlags;

public sealed record DispatchGateDecision(bool Allowed, string Reason);

public sealed class DispatchGate(
    IFeatureFlags featureFlags,
    IProductionCallGate productionCallGate,
    IRuntimeSafetyHealth runtimeSafetyHealth,
    IProductionPilotGate productionPilotGate)
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

        if (snapshot.ExecutionMode == IvrOptions.LabRealSimExecutionMode)
        {
            return snapshot.LabDestinationAllowlist.Contains(destinationReference)
                ? new DispatchGateDecision(true, "LAB_DESTINATION_APPROVED")
                : new DispatchGateDecision(false, "DESTINATION_NOT_ALLOWLISTED");
        }

        if (snapshot.ExecutionMode != IvrOptions.ProductionRealExecutionMode)
        {
            return new DispatchGateDecision(false, "UNSUPPORTED_EXECUTION_MODE");
        }

        return await EvaluateProductionAsync(
            environment,
            destinationReference,
            cancellationToken);
    }

    /// <summary>
    /// Q-28 (PA2, 2026-09-26). The production branch, which used to share the lab's path: it asked
    /// the lab allowlist first, which cannot hold a number (its guardrail runs every entry through
    /// PiiGuard), and it was handed <c>sip:NUMBER@HOST</c>, which PiiGuard refused on the first
    /// line. So no production call could ever pass, and the branch had never been tested. It now
    /// receives the pilot fingerprint the vault derived, never the number, and asks three things in
    /// order: has this environment's release been approved, has it been opened past the pilot, and
    /// if not, is this destination on the approved list.
    /// <para>
    /// Whether real customers may be called at all is not asked again here: for PRODUCTION_REAL the
    /// guardrails require <c>RealCustomerCallAllowed</c> to be on, so a production snapshot that
    /// says otherwise has already been refused above as <c>INVALID_RUNTIME_CONFIG</c>.
    /// </para>
    /// </summary>
    private async Task<DispatchGateDecision> EvaluateProductionAsync(
        string environment,
        string destinationReference,
        CancellationToken cancellationToken)
    {
        // SIP-04. The environment this decision is being made for, which was in hand the whole
        // time and simply was not passed. Without it one signature opened every deployment that
        // could reach the same database.
        if (!await productionCallGate.IsApprovedAsync(environment, cancellationToken))
        {
            return new DispatchGateDecision(false, "PRODUCTION_RELEASE_NOT_APPROVED");
        }

        if (await productionPilotGate.IsOpenAsync(environment, cancellationToken))
        {
            return new DispatchGateDecision(true, "PRODUCTION_OPEN_APPROVED");
        }

        ProductionPilotDecision pilot = await productionPilotGate.EvaluateAsync(
            environment,
            destinationReference,
            cancellationToken);
        return new DispatchGateDecision(pilot.Allowed, pilot.Reason);
    }
}

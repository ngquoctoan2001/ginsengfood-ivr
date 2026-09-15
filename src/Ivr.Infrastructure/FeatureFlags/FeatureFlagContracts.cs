using Ivr.Infrastructure.Audit;

namespace Ivr.Infrastructure.FeatureFlags;

public interface IFeatureFlagStore
{
    public Task<FeatureFlagSnapshot> ReadFreshAsync(
        string environment,
        CancellationToken cancellationToken = default);

    public Task<FeatureFlagSnapshot> ApplyAuditedAsync(
        FeatureFlagSnapshot expected,
        FeatureFlagSnapshot proposed,
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default);
}

public interface IFeatureFlags
{
    public Task<FeatureFlagReadResult> GetSnapshotAsync(
        string environment,
        bool forceFresh = false,
        CancellationToken cancellationToken = default);
}

public interface IDynamicConfig
{
    public Task<FeatureFlagReadResult> GetConfigAsync(
        string environment,
        bool forceFresh = false,
        CancellationToken cancellationToken = default);
}

public interface IFeatureFlagRefresher
{
    public Task<FeatureFlagReadResult> RefreshAsync(
        string environment,
        CancellationToken cancellationToken = default);
}

public interface IKillSwitch
{
    public Task<bool> RealCallsEnabledAsync(
        string environment,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The release decision behind real customer dialling.
/// <para>
/// SIP-04. The environment is a parameter, and it has to be. This gate used to ask only whether
/// <i>any</i> live <c>PRODUCTION_CALL</c> approval existed, so one signature granted for a pilot
/// opened every deployment that could reach the same database - which is not what anybody signing
/// it would have believed they were signing.
/// </para>
/// <para>
/// Unlike <see cref="IRuntimeGateAuthorization"/>, whose coarseness is deliberate because the
/// per-change four-eyes row carries the environment, nothing narrows this one. There is no
/// per-change row behind a production call: the approval <i>is</i> the decision.
/// </para>
/// </summary>
public interface IProductionCallGate
{
    public Task<bool> IsApprovedAsync(
        string environment,
        CancellationToken cancellationToken = default);
}

public interface IRuntimeGateAuthorization
{
    public Task<bool> IsApprovedAsync(CancellationToken cancellationToken = default);
}

public interface IRuntimeSafetyHealth
{
    public Task<bool> IsAuditProviderHealthyAsync(
        CancellationToken cancellationToken = default);
}

public interface IFourEyesApprovalVerifier
{
    public Task<string?> VerifyAsync(
        string approvalReference,
        string proposerActorId,
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after,
        CancellationToken cancellationToken = default);
}

public interface IDispatchGate
{
    public Task<DispatchGateDecision> EvaluateAsync(
        string environment,
        string destinationReference,
        CancellationToken cancellationToken = default);
}

public interface IFeatureFlagAdminService
{
    public Task<FeatureFlagMutationResult> MutateAsync(
        FeatureFlagMutationCommand command,
        CancellationToken cancellationToken = default);
}

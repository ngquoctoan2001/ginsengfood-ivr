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

public interface IProductionCallGate
{
    public Task<bool> IsApprovedAsync(CancellationToken cancellationToken = default);
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

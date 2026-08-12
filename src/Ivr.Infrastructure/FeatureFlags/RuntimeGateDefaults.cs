namespace Ivr.Infrastructure.FeatureFlags;

public sealed class PendingRuntimeGateAuthorization : IRuntimeGateAuthorization
{
    public Task<bool> IsApprovedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}

public sealed class PendingFourEyesApprovalVerifier : IFourEyesApprovalVerifier
{
    public Task<string?> VerifyAsync(
        string approvalReference,
        string proposerActorId,
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}

public sealed class PendingProductionCallGate : IProductionCallGate
{
    public Task<bool> IsApprovedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}

public sealed class HealthyInMemoryRuntimeSafety : IRuntimeSafetyHealth
{
    public Task<bool> IsAuditProviderHealthyAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}

public sealed class PendingRuntimeSafetyHealth : IRuntimeSafetyHealth
{
    public Task<bool> IsAuditProviderHealthyAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}

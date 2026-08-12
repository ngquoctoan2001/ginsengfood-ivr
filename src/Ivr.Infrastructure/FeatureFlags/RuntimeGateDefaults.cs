namespace Ivr.Infrastructure.FeatureFlags;

using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

public sealed class PostgresRuntimeSafetyHealth(
    IDbContextFactory<IvrDbContext> dbContextFactory) : IRuntimeSafetyHealth
{
    public async Task<bool> IsAuditProviderHealthyAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
                cancellationToken);
            return await dbContext.Database
                .SqlQueryRaw<bool>(
                    "SELECT (to_regclass('public.ivr_audit_log') IS NOT NULL) AS \"Value\"")
                .SingleAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

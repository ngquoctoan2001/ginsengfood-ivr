namespace Ivr.Infrastructure.FeatureFlags;

using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class PendingRuntimeGateAuthorization : IRuntimeGateAuthorization
{
    public Task<bool> IsApprovedAsync(
        string environment,
        CancellationToken cancellationToken = default) =>
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
    public Task<bool> IsApprovedAsync(
        string environment,
        CancellationToken cancellationToken = default) =>
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
            // W-0360 / K-31. Still "no" -- an audit store that cannot be asked is not a healthy
            // one -- but counted, so an outage of the check itself is told apart from a store that
            // is really missing. Both used to read as the same quiet false.
            IvrTelemetry.RecordFailClosed((TelemetryTags.ReasonCode, AuditStoreUnreadable));
            return false;
        }
    }

    /// <summary>The <c>ivr.reason_code</c> on <c>ivr_fail_closed_total</c> for a check that threw.</summary>
    public const string AuditStoreUnreadable = "AUDIT_STORE_UNREADABLE";
}

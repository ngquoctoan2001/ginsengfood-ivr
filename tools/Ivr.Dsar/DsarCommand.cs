using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Governance;

namespace Ivr.Dsar;

public sealed record DsarOperatorPolicy(int Version, string AllowedIdentity);

public sealed record DsarCommandResult(
    string Mode,
    string Operator,
    string RequestRef,
    DsarFindReport Holdings,
    DsarErasureReport Erasure);

public static class DsarCommand
{
    public static void Authorize(DsarOperatorPolicy policy, string currentIdentity)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (policy.Version != 1 || string.IsNullOrWhiteSpace(policy.AllowedIdentity)
            || string.IsNullOrWhiteSpace(currentIdentity)
            || !string.Equals(policy.AllowedIdentity, currentIdentity,
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("The current OS identity is not the configured DSAR operator.");
        }

        PiiGuard.EnsureSafeText(currentIdentity);
    }

    public static async Task<DsarCommandResult> ExecuteAsync(
        IDsarService service,
        DsarRequest request,
        DsarOperatorPolicy policy,
        string currentIdentity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(request);
        Authorize(policy, currentIdentity);
        DsarFindReport holdings = await service.FindAsync(request.OrderCode, cancellationToken);
        DsarErasureReport erasure = await service.EraseAsync(
            request.OrderCode,
            $"DSAR {(request.Execute ? "verified" : "preview")} request {request.RequestRef}",
            $"operator:{currentIdentity}",
            $"dsar:{Guid.NewGuid():N}",
            dryRun: !request.Execute,
            cancellationToken);
        return new DsarCommandResult(
            request.Execute ? "EXECUTED" : "PREVIEW", currentIdentity, request.RequestRef, holdings, erasure);
    }
}

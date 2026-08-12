using System.Security.Claims;

namespace Ivr.Api.Auth;

public sealed class ClaimsPermissionEvaluator : IPermissionEvaluator
{
    public const string PermissionClaimType = "permission";

    public ValueTask<bool> HasPermissionAsync(
        ClaimsPrincipal principal,
        string permission,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        cancellationToken.ThrowIfCancellationRequested();

        bool hasPermission = principal.Claims.Any(
            claim => string.Equals(
                         claim.Type,
                         PermissionClaimType,
                         StringComparison.Ordinal)
                     && string.Equals(claim.Value, permission, StringComparison.Ordinal));
        return ValueTask.FromResult(hasPermission);
    }
}

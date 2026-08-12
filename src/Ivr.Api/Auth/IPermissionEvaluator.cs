using System.Security.Claims;

namespace Ivr.Api.Auth;

public interface IPermissionEvaluator
{
    public ValueTask<bool> HasPermissionAsync(
        ClaimsPrincipal principal,
        string permission,
        CancellationToken cancellationToken = default);
}

using Microsoft.AspNetCore.Authorization;

namespace Ivr.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        if (!IvrPermissions.All.Contains(permission))
        {
            throw new ArgumentOutOfRangeException(
                nameof(permission),
                permission,
                "Permission is not part of the DF-01 catalog.");
        }

        Policy = permission;
    }
}

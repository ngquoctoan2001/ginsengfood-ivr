using Microsoft.AspNetCore.Authorization;

namespace Ivr.Api.Auth;

/// <summary>W-0128. The tier an endpoint requires.</summary>
public sealed class AdminScopeRequirement(AdminScope scope) : IAuthorizationRequirement
{
    public AdminScope Scope { get; } = scope;
}

/// <summary>
/// W-0128. Checks the tier claim, the scope header, and — on the danger tier — the acting
/// operator and reason.
/// <para>
/// The header checks read the request through <see cref="IHttpContextAccessor"/> rather than
/// <c>AuthorizationHandlerContext.Resource</c>. Resource is whatever the middleware chose to put
/// there, which differs by hosting model; a security decision should not depend on that.
/// </para>
/// </summary>
public sealed class AdminScopeAuthorizationHandler(IHttpContextAccessor accessor)
    : AuthorizationHandler<AdminScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminScopeRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (!AdminTokenMatcher.Satisfies(
                context.User.FindFirst(AdminTokenAuthenticationHandler.ScopeClaimType)?.Value,
                requirement.Scope))
        {
            return Task.CompletedTask;
        }

        // The header declares the tier the caller believes it is using, so it is checked against
        // the tier its token resolved to -- not against what this endpoint requires. Checking it
        // against the requirement would break the hierarchy: a write credential legitimately
        // reaches a read endpoint, and would then be rejected for honestly saying "write".
        HttpContext? http = accessor.HttpContext;
        string? held = context.User
            .FindFirst(AdminTokenAuthenticationHandler.ScopeClaimType)?.Value;
        if (http is null
            || !string.Equals(
                http.Request.Headers[AdminScopeGuard.ScopeHeaderName].ToString(),
                held,
                StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        if (requirement.Scope == AdminScope.Danger && !AdminScopeGuard.HasDangerEvidence(http))
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

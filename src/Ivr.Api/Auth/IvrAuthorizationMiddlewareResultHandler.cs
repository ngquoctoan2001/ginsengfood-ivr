using Ivr.Api.Middleware;
using Ivr.Domain.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Ivr.Api.Auth;

public sealed class IvrAuthorizationMiddlewareResultHandler(IvrErrorResponseWriter errorWriter)
    : IAuthorizationMiddlewareResultHandler
{
    private static readonly AuthorizationMiddlewareResultHandler DefaultHandler = new();

    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        ArgumentNullException.ThrowIfNull(authorizeResult);

        if (authorizeResult.Forbidden)
        {
            return errorWriter.WriteAsync(context, IvrErrors.ForbiddenCaller());
        }

        if (authorizeResult.Challenged)
        {
            return errorWriter.WriteAsync(context, IvrErrors.Unauthenticated());
        }

        return DefaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}

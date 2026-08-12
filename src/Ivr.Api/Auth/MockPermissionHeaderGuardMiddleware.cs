using Ivr.Api.Middleware;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Auth;

public sealed class MockPermissionHeaderGuardMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IOptions<IvrOptions> options,
        IvrErrorResponseWriter errorWriter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(errorWriter);

        bool isMock = string.Equals(
            options.Value.ExecutionMode,
            IvrOptions.MockExecutionMode,
            StringComparison.OrdinalIgnoreCase);

        if (!isMock && context.Request.Headers.ContainsKey(
                MockPermissionAuthenticationHandler.HeaderName))
        {
            await errorWriter.WriteAsync(context, IvrErrors.ForbiddenCaller());
            return;
        }

        await next(context);
    }
}

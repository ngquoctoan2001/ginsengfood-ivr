using Ivr.Api.Auth;

namespace Ivr.Api.Middleware;

public static class IvrApiApplicationBuilderExtensions
{
    public static IApplicationBuilder UseIvrApiFoundation(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<CorrelationMiddleware>();
        app.UseMiddleware<MockPermissionHeaderGuardMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<OrderCoreAllowlistMiddleware>();
        app.UseMiddleware<ErrorEnvelopeMiddleware>();
        return app;
    }
}

using Ivr.Api.Auth;

namespace Ivr.Api.Middleware;

public static class IvrApiApplicationBuilderExtensions
{
    public static IApplicationBuilder UseIvrApiFoundation(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<CorrelationMiddleware>();
        app.UseMiddleware<ErrorEnvelopeMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<OrderCoreAllowlistMiddleware>();
        // W-0282 / B2. Last on purpose: see ServiceQuotaMiddleware for why a ceiling in front of
        // authentication would answer 429 where the contract promises 401.
        app.UseMiddleware<ServiceQuotaMiddleware>();
        return app;
    }
}

using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Ivr.Api.Filters;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Admin;

/// <summary>
/// UI-07 non-production developer surface (W-0112).
/// <para>
/// The routes are not registered at all outside a non-production deployment. That is deliberate
/// and it is the reason production answers <c>404</c> rather than <c>403</c>: a <c>403</c> tells
/// an unauthenticated caller that a seed loader exists at this address and that the only thing
/// between them and it is a permission. <c>404</c> tells them nothing, and it happens to be true —
/// in production there is no such route.
/// </para>
/// </summary>
public static class DevToolingEndpoints
{
    public const string RoutePrefix = "/v1/ivr/order-confirmation/dev";

    public static IEndpointRouteBuilder MapIvrDevToolingEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        IServiceProvider services = endpoints.ServiceProvider;
        IvrOptions ivr = services.GetRequiredService<IOptions<IvrOptions>>().Value;
        IHostEnvironment environment = services.GetRequiredService<IHostEnvironment>();
        if (!NonProductionSurface.IsAvailable(
                environment.EnvironmentName,
                ivr.ExecutionMode,
                ivr.RealCustomerCallAllowed))
        {
            return endpoints;
        }

        // Write tier, not read: loading fixtures and moving channels are mutations. W-0128
        // replaced the console session scheme this used to pin to, so the boundary is now the
        // separate write token rather than a self-declared permission header — a header the
        // caller writes about itself would not have been a boundary at all in MOCK, the mode
        // every non-production deployment runs in, which is exactly what this serves.
        RouteGroupBuilder group = endpoints.MapGroup(RoutePrefix)
            .AddEndpointFilter<PiiMaskingFilter>();

        group.MapPost("/seed:load", LoadSeedAsync)
            .AddEndpointFilter(new MutationReplayFilter())
            .RequireAuthorization(AdminPolicies.Write);
        group.MapPost("/scenarios/{scenarioId}:dry-run", DryRunScenarioAsync)
            .RequireAuthorization(AdminPolicies.Write);
        group.MapPost("/integration-profiles/{profileId}:apply", ApplyProfileAsync)
            .AddEndpointFilter(new MutationReplayFilter())
            .RequireAuthorization(AdminPolicies.Write);
        return endpoints;
    }

    private static Task<SeedLoadApiResult> LoadSeedAsync(
        SeedLoadRequest request,
        HttpContext context,
        IDevToolingApiService service,
        CancellationToken cancellationToken) =>
        service.LoadSeedAsync(
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);

    private static Task<ScenarioDryRunApiResult> DryRunScenarioAsync(
        string scenarioId,
        AdminMutationRequest request,
        HttpContext context,
        IDevToolingApiService service,
        CancellationToken cancellationToken) =>
        service.DryRunScenarioAsync(
            scenarioId,
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            cancellationToken);

    private static Task<IntegrationProfileApiResult> ApplyProfileAsync(
        string profileId,
        AdminMutationRequest request,
        HttpContext context,
        IDevToolingApiService service,
        CancellationToken cancellationToken) =>
        service.ApplyIntegrationProfileAsync(
            profileId,
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);
}

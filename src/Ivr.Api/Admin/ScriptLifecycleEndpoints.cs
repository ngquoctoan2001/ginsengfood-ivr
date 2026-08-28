using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Api.Filters;
using Ivr.Api.Internal;

namespace Ivr.Api.Admin;

/// <summary>
/// Script lifecycle transitions (W-0109).
/// <para>
/// Every route is pinned to the console session scheme rather than left on the default. The
/// MOCK permission seam mints whatever <c>X-Permissions</c> asks for, MOCK is the default mode,
/// and one of these permissions signs off the wording a customer is read before pressing a key.
/// A route that forgot this pin would be an approval endpoint reachable with no credential.
/// </para>
/// </summary>
public static class ScriptLifecycleEndpoints
{
    public static IEndpointRouteBuilder MapIvrScriptLifecycleEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        RouteGroupBuilder group = endpoints
            .MapGroup("/v1/ivr/order-confirmation/scripts")
            .AddEndpointFilter<PiiMaskingFilter>();

        group.MapGet("/{templateId}/{version}", GetAsync)
            .RequireAuthorization(AdminPolicies.Read);
        group.MapPost("/", CreateDraftAsync)
            .RequireAuthorization(AdminPolicies.Write);
        group.MapPost("/{templateId}/{version}:submit", SubmitAsync)
            .RequireAuthorization(AdminPolicies.Write);

        // One route for all four approval types rather than four routes. The permission is not
        // the same for each, so the route-level attribute can only assert the weakest of them —
        // ScriptLifecycleApiService builds the actor from the session's own permissions and the
        // domain demands the specific one, which is where the real check lives.
        group.MapPost("/{templateId}/{version}:approve", ApproveAsync)
            .RequireAuthorization(AdminPolicies.Write);
        group.MapPost("/{templateId}/{version}:retire", RetireAsync)
            .RequireAuthorization(AdminPolicies.Write);
        return endpoints;
    }

    private static Task<ScriptVersionApiResult> GetAsync(
        string templateId,
        string version,
        HttpContext context,
        IScriptLifecycleApiService service,
        CancellationToken cancellationToken)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetAsync(templateId, version, cancellationToken);
    }

    private static Task<ScriptActionApiResult> CreateDraftAsync(
        ScriptDraftRequest request,
        HttpContext context,
        IScriptLifecycleApiService service,
        CancellationToken cancellationToken) =>
        service.CreateDraftAsync(
            request,
            context.User,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            cancellationToken);

    private static Task<ScriptActionApiResult> SubmitAsync(
        string templateId,
        string version,
        ScriptTransitionRequest request,
        HttpContext context,
        IScriptLifecycleApiService service,
        CancellationToken cancellationToken) =>
        service.SubmitAsync(
            templateId,
            version,
            request,
            context.User,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            cancellationToken);

    private static Task<ScriptActionApiResult> ApproveAsync(
        string templateId,
        string version,
        ScriptApprovalRequest request,
        HttpContext context,
        IScriptLifecycleApiService service,
        CancellationToken cancellationToken) =>
        service.ApproveAsync(
            templateId,
            version,
            request,
            context.User,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            cancellationToken);

    private static Task<ScriptActionApiResult> RetireAsync(
        string templateId,
        string version,
        ScriptTransitionRequest request,
        HttpContext context,
        IScriptLifecycleApiService service,
        CancellationToken cancellationToken) =>
        service.RetireAsync(
            templateId,
            version,
            request,
            context.User,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            cancellationToken);
}

using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Api.Filters;
using Ivr.Api.Internal;

namespace Ivr.Api.Admin;

/// <summary>
/// Script lifecycle transitions (W-0109).
/// <para>
/// Every route sits on the admin token scheme: reading a version needs the read tier, the four
/// transitions need write. The tier is not the interesting control here. One of these routes
/// signs off the wording a customer is read before pressing a key, and a tier cannot express
/// "two different people" — so the approval itself is decided further in, by
/// <see cref="Ivr.Domain.Scripts.ScriptActor"/> against the <c>X-Actor-Id</c> Module 3 asserts.
/// </para>
/// <para>
/// W-0122 removed the MOCK permission seam that used to mint whatever <c>X-Permissions</c> asked
/// for. What replaced it is <c>X-Script-Permissions</c>, which Module 3 self-asserts — so it is a
/// declaration, not a credential, and the four-eyes rules deliberately do not rest on it: an
/// actor claiming all seven approvals still cannot sign both halves of a production pair.
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

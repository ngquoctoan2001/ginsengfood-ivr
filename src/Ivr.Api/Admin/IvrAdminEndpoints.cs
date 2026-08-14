using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Api.Filters;
using Ivr.Api.Internal;

namespace Ivr.Api.Admin;

public static class IvrAdminEndpoints
{
    public static IEndpointRouteBuilder MapIvrAdminEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder adminGroup = endpoints.MapGroup(
            "/v1/ivr/order-confirmation")
            .AddEndpointFilter<PiiMaskingFilter>();
        adminGroup.MapGet("/queue", GetQueueAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView));
        adminGroup.MapPost("/queue:pause", PauseQueueAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueuePause));
        adminGroup.MapPost("/queue:resume", ResumeQueueAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueResume));
        adminGroup.MapPost("/sim-channels/{simChannelId}:disable", DisableChannelAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.SimDisable));
        adminGroup.MapPost("/sim-channels/{simChannelId}:enable", EnableChannelAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.SimEnable));
        adminGroup.MapPost("/technical-retries", RetryTechnicalExceptionAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.ManualRetry));
        adminGroup.MapPost("/admin-reviews", ReviewAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.ResultReview));
        return endpoints;
    }

    private static Task<QueueProjectionApiResult> GetQueueAsync(
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetQueueAsync(cancellationToken);
    }

    private static Task<AdminActionApiResult> PauseQueueAsync(
        AdminMutationRequest request,
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken) =>
        service.PauseQueueAsync(
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);

    private static Task<AdminActionApiResult> ResumeQueueAsync(
        AdminMutationRequest request,
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken) =>
        service.ResumeQueueAsync(
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);

    private static Task<AdminActionApiResult> DisableChannelAsync(
        string simChannelId,
        AdminMutationRequest request,
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken) =>
        service.DisableChannelAsync(
            simChannelId,
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);

    private static Task<AdminActionApiResult> EnableChannelAsync(
        string simChannelId,
        AdminMutationRequest request,
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken) =>
        service.EnableChannelAsync(
            simChannelId,
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);

    private static Task<TechnicalRetryApiResult> RetryTechnicalExceptionAsync(
        TechnicalRetryRequest request,
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken) =>
        service.RetryTechnicalExceptionAsync(
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);

    private static Task<AdminReviewApiResult> ReviewAsync(
        AdminReviewRequest request,
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken) =>
        service.ReviewAsync(
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);
}

using Ivr.Api.Application;
using Ivr.Api.Accounts;
using Ivr.Api.Auth;
using Ivr.Api.Filters;
using Ivr.Api.Internal;
using Microsoft.AspNetCore.Mvc;

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
        adminGroup.MapGet("/dashboard", GetDashboardAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView));
        adminGroup.MapGet("/call-jobs", ListCallJobsAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView));
        adminGroup.MapGet("/call-jobs/{ivrCallJobId}/detail", GetCallJobDetailAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView));
        adminGroup.MapGet("/sim-channels", ListSimChannelsAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView));
        adminGroup.MapGet("/scripts", GetScriptCatalogAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView))
            .RequireAuthorization(IvrRoles.ConsoleAdminPolicy);
        adminGroup.MapGet("/integration-status", GetIntegrationStatusAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView))
            .RequireAuthorization(IvrRoles.ConsoleAdminPolicy);
        adminGroup.MapGet("/review-items", ListReviewItemsAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView))
            .RequireAuthorization(IvrRoles.ConsoleAdminPolicy);
        adminGroup.MapGet("/analytics/summary", GetAnalyticsSummaryAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView))
            .RequireAuthorization(IvrRoles.ConsoleAdminPolicy);
        adminGroup.MapGet("/analytics/trend", GetAnalyticsTrendAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView))
            .RequireAuthorization(IvrRoles.ConsoleAdminPolicy);
        adminGroup.MapGet("/analytics/breakdown", GetAnalyticsBreakdownAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView))
            .RequireAuthorization(IvrRoles.ConsoleAdminPolicy);
        // GET, not POST: the extract is a read that is audited, and keeping the
        // verb read-only preserves the "no mutation surface" invariant the other
        // reporting routes are tested against.
        adminGroup.MapGet("/analytics/export", ExportAnalyticsAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView))
            .RequireAuthorization(IvrRoles.ConsoleAdminPolicy);
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

        // W-0111. Operator holds this as well as Admin: it is the risk-reducing direction, and
        // an operator who has to find an admin is an operator watching a call they were told
        // to end.
        adminGroup.MapPost("/call-jobs/{ivrCallJobId}:terminate", TerminateCallAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.CallTerminate));

        // Separate route, separate press, separate reason. Engaging the kill switch stops the
        // next call; this ends conversations already under way.
        adminGroup.MapPost("/call-jobs:terminate-all", TerminateAllCallsAsync)
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.CallTerminate));
        endpoints.MapIvrConsoleAccountEndpoints();
        endpoints.MapIvrScriptLifecycleEndpoints();
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

    private static Task<DashboardApiResult> GetDashboardAsync(
        HttpContext context,
        IAdminReadService service,
        CancellationToken cancellationToken,
        string? program = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetDashboardAsync(program, from, to, cancellationToken);
    }

    // Query names stay snake_case to match the rest of the contract.
    private static Task<CallJobPageApiResult> ListCallJobsAsync(
        HttpContext context,
        IAdminReadService service,
        CancellationToken cancellationToken,
        [FromQuery(Name = "program")] string? program = null,
        [FromQuery(Name = "status")] string? status = null,
        [FromQuery(Name = "queue_status")] string? queueStatus = null,
        [FromQuery(Name = "result_type")] string? resultType = null,
        [FromQuery(Name = "order_code")] string? orderCode = null,
        [FromQuery(Name = "correlation_id")] string? correlationId = null,
        [FromQuery(Name = "near_expiry")] bool nearExpiry = false,
        [FromQuery(Name = "from")] DateTimeOffset? from = null,
        [FromQuery(Name = "to")] DateTimeOffset? to = null,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = AdminReadService.DefaultPageSize)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.ListCallJobsAsync(
            new CallJobFilter(
                program,
                status,
                queueStatus,
                resultType,
                orderCode,
                correlationId,
                nearExpiry,
                from,
                to,
                page,
                pageSize),
            cancellationToken);
    }

    private static Task<CallJobDetailApiResult> GetCallJobDetailAsync(
        string ivrCallJobId,
        HttpContext context,
        IAdminReadService service,
        CancellationToken cancellationToken)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetCallJobDetailAsync(ivrCallJobId, cancellationToken);
    }

    private static Task<SimChannelListApiResult> ListSimChannelsAsync(
        HttpContext context,
        IAdminReadService service,
        CancellationToken cancellationToken)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.ListSimChannelsAsync(cancellationToken);
    }

    private static Task<ScriptCatalogApiResult> GetScriptCatalogAsync(
        HttpContext context,
        IAdminConfigReadService service,
        CancellationToken cancellationToken)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetScriptCatalogAsync(cancellationToken);
    }

    private static Task<IntegrationStatusApiResult> GetIntegrationStatusAsync(
        HttpContext context,
        IAdminConfigReadService service,
        CancellationToken cancellationToken,
        [FromQuery(Name = "environment")] string? environment = null)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetIntegrationStatusAsync(environment ?? string.Empty, cancellationToken);
    }

    private static Task<ReviewQueueApiResult> ListReviewItemsAsync(
        HttpContext context,
        IAdminConfigReadService service,
        CancellationToken cancellationToken,
        [FromQuery(Name = "status")] string? status = null,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = AdminConfigReadService.DefaultPageSize)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.ListReviewItemsAsync(status, page, pageSize, cancellationToken);
    }

    private static Task<AnalyticsSummaryApiResult> GetAnalyticsSummaryAsync(
        HttpContext context,
        IAnalyticsReadService service,
        CancellationToken cancellationToken,
        [FromQuery(Name = "program")] string? program = null,
        [FromQuery(Name = "result_type")] string? resultType = null,
        [FromQuery(Name = "script_variant")] string? scriptVariant = null,
        [FromQuery(Name = "bucket")] string? bucket = null,
        [FromQuery(Name = "from")] DateTimeOffset? from = null,
        [FromQuery(Name = "to")] DateTimeOffset? to = null)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetSummaryAsync(
            new AnalyticsFilter(program, resultType, scriptVariant, bucket, from, to),
            cancellationToken);
    }

    private static Task<AnalyticsTrendApiResult> GetAnalyticsTrendAsync(
        HttpContext context,
        IAnalyticsReadService service,
        CancellationToken cancellationToken,
        [FromQuery(Name = "program")] string? program = null,
        [FromQuery(Name = "result_type")] string? resultType = null,
        [FromQuery(Name = "script_variant")] string? scriptVariant = null,
        [FromQuery(Name = "bucket")] string? bucket = null,
        [FromQuery(Name = "from")] DateTimeOffset? from = null,
        [FromQuery(Name = "to")] DateTimeOffset? to = null)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetTrendAsync(
            new AnalyticsFilter(program, resultType, scriptVariant, bucket, from, to),
            cancellationToken);
    }

    private static Task<AnalyticsBreakdownApiResult> GetAnalyticsBreakdownAsync(
        HttpContext context,
        IAnalyticsReadService service,
        CancellationToken cancellationToken,
        [FromQuery(Name = "dimension")] string? dimension = null,
        [FromQuery(Name = "program")] string? program = null,
        [FromQuery(Name = "result_type")] string? resultType = null,
        [FromQuery(Name = "script_variant")] string? scriptVariant = null,
        [FromQuery(Name = "bucket")] string? bucket = null,
        [FromQuery(Name = "from")] DateTimeOffset? from = null,
        [FromQuery(Name = "to")] DateTimeOffset? to = null)
    {
        _ = InternalRequestGuard.RequireCorrelation(context);
        _ = InternalRequestGuard.RequireAdminActor(context);
        return service.GetBreakdownAsync(
            new AnalyticsFilter(program, resultType, scriptVariant, bucket, from, to),
            dimension,
            cancellationToken);
    }

    private static Task<AnalyticsExportApiResult> ExportAnalyticsAsync(
        HttpContext context,
        IAnalyticsReadService service,
        CancellationToken cancellationToken,
        [FromQuery(Name = "reason")] string? reason = null,
        [FromQuery(Name = "dimension")] string? dimension = null,
        [FromQuery(Name = "program")] string? program = null,
        [FromQuery(Name = "result_type")] string? resultType = null,
        [FromQuery(Name = "script_variant")] string? scriptVariant = null,
        [FromQuery(Name = "bucket")] string? bucket = null,
        [FromQuery(Name = "from")] DateTimeOffset? from = null,
        [FromQuery(Name = "to")] DateTimeOffset? to = null)
    {
        string correlationId = InternalRequestGuard.RequireCorrelation(context);
        string actorId = InternalRequestGuard.RequireAdminActor(context);
        return service.ExportAsync(
            new AnalyticsFilter(program, resultType, scriptVariant, bucket, from, to),
            dimension,
            reason,
            actorId,
            correlationId,
            cancellationToken);
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

    private static Task<AdminActionApiResult> TerminateCallAsync(
        string ivrCallJobId,
        AdminMutationRequest request,
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken) =>
        service.TerminateCallAsync(
            ivrCallJobId,
            request,
            InternalRequestGuard.RequireAdminActor(context),
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);

    private static Task<AdminActionApiResult> TerminateAllCallsAsync(
        AdminMutationRequest request,
        HttpContext context,
        IInternalAdminApiService service,
        CancellationToken cancellationToken) =>
        service.TerminateAllActiveCallsAsync(
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

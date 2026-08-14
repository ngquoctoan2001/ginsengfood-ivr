using Ivr.Api.Application;
using Ivr.Api.Filters;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Internal;

public static class InternalLifecycleEndpoints
{
    public static IEndpointRouteBuilder MapIvrInternalLifecycleEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder internalGroup = endpoints.MapGroup(
            "/v1/ivr/order-confirmation")
            .AllowAnonymous()
            .AddEndpointFilter<PiiMaskingFilter>();
        internalGroup.MapPost("/eligibility-checks", RecordEligibilityAsync);
        internalGroup.MapPost("/call-jobs", ReassertCallJobAsync);
        internalGroup.MapGet("/call-jobs/{ivrCallJobId}", GetCallJobAsync);
        internalGroup.MapPost("/call-attempts", ReassertAttemptAsync);
        internalGroup.MapPost("/call-results", ReassertResultAsync);
        internalGroup.MapPost("/result-callbacks", ReassertCallbackAsync);
        return endpoints;
    }

    private static Task<EligibilityApiResult> RecordEligibilityAsync(
        EligibilityLifecycleRequest request,
        HttpContext context,
        IOptions<InternalServiceOptions> options,
        IInternalAdminApiService service,
        CancellationToken cancellationToken)
    {
        InternalRequestGuard.RequireInternalService(context, options);
        return service.EvaluateEligibilityAsync(
            request.TaskId,
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);
    }

    private static Task<CallJobApiResult> ReassertCallJobAsync(
        CallJobLifecycleRequest request,
        HttpContext context,
        IOptions<InternalServiceOptions> options,
        IInternalAdminApiService service,
        CancellationToken cancellationToken)
    {
        InternalRequestGuard.RequireInternalService(context, options);
        return service.ReassertCallJobAsync(
            request,
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);
    }

    private static Task<CallJobApiResult> GetCallJobAsync(
        string ivrCallJobId,
        HttpContext context,
        IOptions<InternalServiceOptions> options,
        IInternalAdminApiService service,
        CancellationToken cancellationToken)
    {
        InternalRequestGuard.RequireInternalService(context, options);
        _ = InternalRequestGuard.RequireCorrelation(context);
        return service.GetCallJobAsync(ivrCallJobId, cancellationToken);
    }

    private static Task<CallAttemptApiResult> ReassertAttemptAsync(
        CallAttemptLifecycleRequest request,
        HttpContext context,
        IOptions<InternalServiceOptions> options,
        IInternalAdminApiService service,
        CancellationToken cancellationToken)
    {
        InternalRequestGuard.RequireInternalService(context, options);
        return service.ReassertAttemptAsync(
            request,
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);
    }

    private static Task<CallResultApiResult> ReassertResultAsync(
        CallResultLifecycleRequest request,
        HttpContext context,
        IOptions<InternalServiceOptions> options,
        IInternalAdminApiService service,
        CancellationToken cancellationToken)
    {
        InternalRequestGuard.RequireInternalService(context, options);
        return service.ReassertResultAsync(
            request,
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);
    }

    private static Task<ResultCallbackApiResult> ReassertCallbackAsync(
        ResultCallbackLifecycleRequest request,
        HttpContext context,
        IOptions<InternalServiceOptions> options,
        IInternalAdminApiService service,
        CancellationToken cancellationToken)
    {
        InternalRequestGuard.RequireInternalService(context, options);
        return service.ReassertCallbackAsync(
            request,
            InternalRequestGuard.RequireCorrelation(context),
            InternalRequestGuard.RequireIdempotencyKey(context),
            cancellationToken);
    }

}

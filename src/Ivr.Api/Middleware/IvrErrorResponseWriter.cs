using Ivr.Domain.Errors;
using Ivr.Infrastructure.Correlation;

namespace Ivr.Api.Middleware;

public sealed class IvrErrorResponseWriter(
    ICorrelationContext correlationContext,
    ILogger<IvrErrorResponseWriter> logger)
{
    public Task WriteAsync(
        HttpContext context,
        IvrFailureException failure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(failure);

        if (context.Response.HasStarted)
        {
            IvrErrorResponseWriterLog.ResponseAlreadyStarted(
                logger,
                failure.ErrorCode,
                correlationContext.GetOrCreate());
            context.Abort();
            return Task.CompletedTask;
        }

        context.Response.Clear();
        context.Response.StatusCode = IvrErrorHttpStatus.FromCode(failure.ErrorCode);
        IvrErrorEnvelope envelope = new(
            new IvrErrorBody(
                failure.ErrorCode,
                failure.Message,
                failure.Details,
                correlationContext.GetOrCreate()));
        return context.Response.WriteAsJsonAsync(envelope, cancellationToken);
    }
}

public sealed record IvrErrorEnvelope(IvrErrorBody Error);

public sealed record IvrErrorBody(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string> Details,
    string CorrelationId);

internal static class IvrErrorHttpStatus
{
    public static int FromCode(string code) => code switch
    {
        IvrErrorCodes.Unauthenticated => StatusCodes.Status401Unauthorized,
        IvrErrorCodes.ForbiddenCaller => StatusCodes.Status403Forbidden,
        IvrErrorCodes.MalformedRequest => StatusCodes.Status400BadRequest,
        IvrErrorCodes.MissingTrace => StatusCodes.Status422UnprocessableEntity,
        IvrErrorCodes.IdempotencyConflict => StatusCodes.Status409Conflict,
        IvrErrorCodes.VersionConflict => StatusCodes.Status409Conflict,
        IvrErrorCodes.NotOfficialOrder => StatusCodes.Status422UnprocessableEntity,
        IvrErrorCodes.StateNotCallable => StatusCodes.Status422UnprocessableEntity,
        IvrErrorCodes.PolicyMismatch => StatusCodes.Status409Conflict,
        IvrErrorCodes.ContactInvalid => StatusCodes.Status422UnprocessableEntity,
        IvrErrorCodes.ScriptNotApproved => StatusCodes.Status422UnprocessableEntity,
        IvrErrorCodes.PiiPolicyViolation => StatusCodes.Status422UnprocessableEntity,
        IvrErrorCodes.OperationalBlocked => StatusCodes.Status409Conflict,
        IvrErrorCodes.NotFound => StatusCodes.Status404NotFound,
        IvrErrorCodes.RateLimited => StatusCodes.Status429TooManyRequests,
        IvrErrorCodes.InternalError => StatusCodes.Status500InternalServerError,
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown IVR error code."),
    };
}

internal static partial class IvrErrorResponseWriterLog
{
    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Error,
        Message = "Cannot write IVR error envelope after the response started. ErrorCode={ErrorCode} CorrelationId={CorrelationId}")]
    public static partial void ResponseAlreadyStarted(
        ILogger logger,
        string errorCode,
        string correlationId);
}

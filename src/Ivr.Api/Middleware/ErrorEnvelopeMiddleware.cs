using Ivr.Domain.Errors;
using Ivr.Infrastructure.Correlation;

namespace Ivr.Api.Middleware;

public sealed class ErrorEnvelopeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IvrErrorResponseWriter errorWriter,
        ICorrelationContext correlationContext,
        ILogger<ErrorEnvelopeMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(errorWriter);
        ArgumentNullException.ThrowIfNull(correlationContext);

        try
        {
            await next(context);
        }
        catch (IvrFailureException failure)
        {
            await errorWriter.WriteAsync(context, failure, context.RequestAborted);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            ErrorEnvelopeLog.UnexpectedFailure(
                logger,
                exception.GetType().Name,
                correlationContext.GetOrCreate());
            await errorWriter.WriteAsync(
                context,
                IvrErrors.InternalError(),
                context.RequestAborted);
        }
    }
}

internal static partial class ErrorEnvelopeLog
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Error,
        Message = "Unhandled IVR request failure. ExceptionType={ExceptionType} CorrelationId={CorrelationId}")]
    public static partial void UnexpectedFailure(
        ILogger logger,
        string exceptionType,
        string correlationId);
}

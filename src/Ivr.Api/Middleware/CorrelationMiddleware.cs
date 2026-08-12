using Ivr.Infrastructure.Correlation;
using Ivr.Domain.Privacy;

namespace Ivr.Api.Middleware;

public sealed class CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
        ICorrelationContext correlationContext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(correlationContext);

        string inbound = context.Request.Headers[CorrelationPropagationHandler.HeaderName]
            .ToString();
        string correlationId = IsValid(inbound)
            ? inbound
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationPropagationHandler.HeaderName] = correlationId;

        using IDisposable correlationScope = correlationContext.Push(correlationId);
        using IDisposable? loggingScope = logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
            });
        await next(context);
    }

    private static bool IsValid(string value) =>
        value.Length is > 0 and <= 128
        && PiiGuard.IsSafeText(value)
        && value.All(character =>
            char.IsAsciiLetterOrDigit(character)
            || character is '-' or '_' or '.' or ':');
}

using Ivr.Infrastructure.Correlation;
using Ivr.Api.Foundation;
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
            : CorrelationIdGenerator.Create();

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

    // W-0221. Was a third copy of the same predicate; now one rule, three callers.
    private static bool IsValid(string value) => TraceHeaderSyntax.IsValid(value);
}

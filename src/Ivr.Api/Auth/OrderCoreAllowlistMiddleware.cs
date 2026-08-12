using System.Security.Cryptography;
using System.Text;
using Ivr.Api.Middleware;
using Ivr.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Auth;

public sealed class OrderCoreAllowlistMiddleware(
    RequestDelegate next,
    IOptions<OrderCoreAllowlistOptions> options)
{
    public const string SourceHeaderName = "X-Source-System";

    public async Task InvokeAsync(
        HttpContext context,
        IvrErrorResponseWriter errorWriter)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(errorWriter);

        if (context.GetEndpoint()?.Metadata.GetMetadata<RequireOrderCoreAttribute>() is null)
        {
            await next(context);
            return;
        }

        if (!string.Equals(
                context.Request.Headers[SourceHeaderName].ToString(),
                OrderCoreAllowlistOptions.SourceSystem,
                StringComparison.Ordinal))
        {
            await errorWriter.WriteAsync(context, IvrErrors.ForbiddenCaller());
            return;
        }

        string authorization = context.Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.Ordinal)
            || authorization.Length == "Bearer ".Length)
        {
            await errorWriter.WriteAsync(context, IvrErrors.Unauthenticated());
            return;
        }

        string suppliedToken = authorization["Bearer ".Length..];
        if (!TokensMatch(suppliedToken, options.Value.ServiceToken))
        {
            await errorWriter.WriteAsync(context, IvrErrors.ForbiddenCaller());
            return;
        }

        await next(context);
    }

    private static bool TokensMatch(string supplied, string expected)
    {
        byte[] suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);
        return suppliedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }
}

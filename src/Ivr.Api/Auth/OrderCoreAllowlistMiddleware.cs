using System.Security.Cryptography;
using System.Text;
using Ivr.Api.Middleware;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Auth;
using Ivr.Infrastructure.Callbacks;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Auth;

public sealed class OrderCoreAllowlistMiddleware(
    RequestDelegate next,
    IOptions<OrderCoreAllowlistOptions> options,
    IServiceJwtValidator jwtValidator,
    IOptions<CallbackDeliveryOptions> salesProvider)
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

        // W-0032 / P4-4 §2.2. A verifiable service identity is tried first: signature, issuer,
        // audience, lifetime, algorithm and the `ivr.task.write` scope all have to hold.
        ServiceIdentityResult identity = await jwtValidator.ValidateAsync(
            suppliedToken,
            ServiceIdentityScopes.TaskWrite,
            context.RequestAborted);
        if (identity.Succeeded)
        {
            // X-Source-System stays metadata. It was checked above as a routing/allowlist hint;
            // the thing that authenticated this caller is the signature, not the header.
            context.Items["ivr.service_identity.subject"] = identity.Subject;
            await next(context);
            return;
        }

        // W-0032 / P4-4 §2.5. The legacy shared secret is compatibility only. Under the TARGET_V1
        // provider profile it is refused outright, so the target path can never run on a static
        // credential — see ServiceIdentityCompatPolicy for why the rule keys off the profile.
        if (ServiceIdentityCompatPolicy.LegacyCredentialAccepted(salesProvider.Value.Provider)
            && TokensMatch(suppliedToken, options.Value.ServiceToken))
        {
            await next(context);
            return;
        }

        await errorWriter.WriteAsync(
            context,
            identity.Failure == ServiceIdentityFailure.KeySourceUnavailable
                ? IvrErrors.Unauthenticated()
                : IvrErrors.ForbiddenCaller());
    }

    private static bool TokensMatch(string supplied, string expected)
    {
        byte[] suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);
        return suppliedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }
}

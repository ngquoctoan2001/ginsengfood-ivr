using System.Security.Cryptography;
using System.Text;
using Ivr.Api.Auth;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Correlation;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Internal;

/// <summary>
/// Fail-closed service identity used only by IVR-owned lifecycle endpoints.
/// </summary>
public sealed class InternalServiceOptions
{
    public const string TokenConfigurationKey = "IVR_INTERNAL_SERVICE_TOKEN";
    public const string RequiredScope = "ivr.internal.write";

    public string ServiceToken { get; set; } = string.Empty;
}

internal static class InternalRequestGuard
{
    public const string ScopeHeaderName = "X-Service-Scope";
    public const string IdempotencyHeaderName = "Idempotency-Key";
    public const string ActorHeaderName = "X-Actor-Id";

    private static readonly HashSet<string> AllowedSources = new(
        ["ivr-worker", "ivr-adapter"],
        StringComparer.Ordinal);

    public static void RequireInternalService(
        HttpContext context,
        IOptions<InternalServiceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        if (context.Request.Headers.ContainsKey(MockPermissionAuthenticationHandler.HeaderName)
            || context.Request.Headers.ContainsKey(MockPermissionAuthenticationHandler.ActorHeaderName))
        {
            throw IvrErrors.ForbiddenCaller();
        }

        string source = context.Request.Headers[OrderCoreAllowlistMiddleware.SourceHeaderName]
            .ToString();
        if (!AllowedSources.Contains(source)
            || !string.Equals(
                context.Request.Headers[ScopeHeaderName].ToString(),
                InternalServiceOptions.RequiredScope,
                StringComparison.Ordinal))
        {
            throw IvrErrors.ForbiddenCaller();
        }

        string authorization = context.Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.Ordinal)
            || authorization.Length == "Bearer ".Length)
        {
            throw IvrErrors.Unauthenticated();
        }

        string expected = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(expected)
            || !TokensMatch(authorization["Bearer ".Length..], expected))
        {
            throw IvrErrors.ForbiddenCaller();
        }
    }

    public static string RequireCorrelation(HttpContext context)
    {
        string correlationId = context.Request.Headers[
            CorrelationPropagationHandler.HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(correlationId)
            || correlationId.Length > 128
            || !PiiGuard.IsSafeText(correlationId)
            || !correlationId.All(character =>
                char.IsAsciiLetterOrDigit(character)
                || character is '-' or '_' or '.' or ':'))
        {
            throw IvrErrors.MalformedRequest("X-Correlation-Id is required and must be safe.");
        }

        return correlationId;
    }

    public static string RequireIdempotencyKey(HttpContext context)
    {
        string key = context.Request.Headers[IdempotencyHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128 || !PiiGuard.IsSafeText(key))
        {
            throw IvrErrors.MalformedRequest("Idempotency-Key is required and must be safe.");
        }

        return key;
    }

    public static string RequireAdminActor(HttpContext context)
    {
        string actor = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value
            ?? throw IvrErrors.ForbiddenCaller();
        string supplied = context.Request.Headers[ActorHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(supplied)
            || !string.Equals(actor, supplied, StringComparison.Ordinal))
        {
            throw IvrErrors.ForbiddenCaller();
        }

        PiiGuard.EnsureSafeText(actor);
        return actor;
    }

    private static bool TokensMatch(string supplied, string expected)
    {
        byte[] suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);
        return suppliedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }
}

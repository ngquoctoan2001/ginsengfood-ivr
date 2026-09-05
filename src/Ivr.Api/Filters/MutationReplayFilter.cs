using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Idempotency;

namespace Ivr.Api.Filters;

/// <summary>
/// Durable HTTP retry/replay for the script and dev-tool mutations. Existing script callers
/// without a key retain their original behavior; a supplied key is always validated and bound
/// to the operation, typed payload and actor/permissions before any side effect runs.
/// Business stores remain responsible for their own transactions and recovery after a crash.
/// </summary>
public sealed class MutationReplayFilter(bool keyRequired = true) : IEndpointFilter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        HttpContext http = context.HttpContext;
        if (HttpMethods.IsGet(http.Request.Method)) return await next(context);
        _ = InternalRequestGuard.RequireCorrelation(http);
        string actor = InternalRequestGuard.RequireAdminActor(http);
        if (!keyRequired && !http.Request.Headers.ContainsKey("Idempotency-Key")) return await next(context);
        string key = InternalRequestGuard.RequireIdempotencyKey(http);
        object request = context.Arguments.Single(argument => argument is not null
            && argument.GetType().Name.EndsWith("Request", StringComparison.Ordinal))!;
        JsonElement body = JsonSerializer.SerializeToElement(request, request.GetType(), JsonOptions);
        // Validate before replay too: a malformed reason cannot hide behind a cached result.
        if (!body.TryGetProperty("reason", out JsonElement reason) || reason.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(reason.GetString()))
            throw IvrErrors.MalformedRequest("A mutation reason is required.");
        string operation = http.Request.Path.Value!.TrimEnd('/');
        string scopedKey = "api-replay-" + Hash(Encoding.UTF8.GetBytes(key));
        string payloadHash = Hash(JsonSerializer.SerializeToUtf8Bytes(new
        {
            operation,
            body,
            actor,
            scriptPermissions = http.Request.Headers[AdminTokenAuthenticationHandler.ScriptPermissionsHeaderName].ToString(),
        }, JsonOptions));
        IIdempotencyStore store = http.RequestServices.GetRequiredService<IIdempotencyStore>();
        async Task<JsonElement> CaptureAsync(CancellationToken _)
        {
            object? response = await next(context);
            return JsonSerializer.SerializeToElement(response, response?.GetType() ?? typeof(object), JsonOptions);
        }
        return store is PostgresIdempotencyStore postgres
            ? await postgres.ExecuteCoordinatedAsync(scopedKey, payloadHash, CaptureAsync, http.RequestAborted)
            : await store.ExecuteAsync(scopedKey, payloadHash, CaptureAsync, http.RequestAborted);
    }

    // Chunked hashes follow the existing privacy-safe identifier convention.
    private static string Hash(byte[] bytes) => string.Join('-',
        Convert.ToHexString(SHA256.HashData(bytes)).Chunk(8).Select(part => new string(part)));
}

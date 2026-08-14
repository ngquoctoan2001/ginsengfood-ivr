using System.Text.Json;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;

namespace Ivr.Api.Filters;

/// <summary>
/// Final fail-closed response guard for the internal/admin surface. Endpoints
/// return masked projections; this filter prevents an accidental raw field or
/// value from crossing the boundary if a projection later regresses.
/// </summary>
public sealed class PiiMaskingFilter : IEndpointFilter
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        object? response = await next(context).ConfigureAwait(false);
        if (response is null || response is IResult)
        {
            return response;
        }

        try
        {
            string json = JsonSerializer.Serialize(response, response.GetType(), JsonOptions);
            PiiGuard.EnsureSafeText(json);
            using JsonDocument document = JsonDocument.Parse(json);
            ValidateElement(document.RootElement);
        }
        catch (InvalidOperationException)
        {
            throw IvrErrors.PiiPolicyViolation();
        }

        return response;
    }

    private static void ValidateElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                PiiGuard.EnsureSafeField(property.Name);
                ValidateElement(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                ValidateElement(item);
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            PiiGuard.EnsureSafeText(element.GetString());
        }
    }
}

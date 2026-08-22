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

    /// <summary>
    /// Fields whose declared purpose is to carry a person's name, checked with the contact-only
    /// rule instead of the full guard. W-0105: the full guard's ASCII address branch rejects the
    /// unaccented spelling of the surnames Dương and Ngô, so applying it to a name column made
    /// legitimate staff records unrenderable.
    /// <para>
    /// Exact names only. <c>customer_display_name</c> and <c>program_display_name</c> are
    /// different keys and keep the full guard, which is the intended outcome — those are
    /// customer-facing speech fields, not a staff roster column.
    /// </para>
    /// </summary>
    private static readonly HashSet<string> NameBearingFields =
        new(StringComparer.Ordinal) { "display_name" };

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

            // Contact-only over the whole blob; the per-field walk below still applies the full
            // guard to every string value except the name-bearing ones, so nothing is lost. The
            // blanket pass stays because it needs no schema knowledge, which is the point of a
            // last-resort guard.
            PiiGuard.EnsureSafeContactText(json);
            using JsonDocument document = JsonDocument.Parse(json);
            ValidateElement(document.RootElement);
        }
        catch (InvalidOperationException)
        {
            throw IvrErrors.PiiPolicyViolation();
        }

        return response;
    }

    private static void ValidateElement(JsonElement element, bool nameBearing = false)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                PiiGuard.EnsureSafeField(property.Name);
                ValidateElement(property.Value, NameBearingFields.Contains(property.Name));
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                ValidateElement(item, nameBearing);
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            if (nameBearing)
            {
                PiiGuard.EnsureSafeContactText(element.GetString());
            }
            else
            {
                PiiGuard.EnsureSafeText(element.GetString());
            }
        }
    }
}

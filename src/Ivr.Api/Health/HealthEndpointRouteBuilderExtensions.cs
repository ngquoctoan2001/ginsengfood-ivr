namespace Ivr.Api.Health;

/// <summary>
/// Defines the bootstrap health endpoints used by local and container probes.
/// </summary>
public static class HealthEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapIvrHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/health/live", () => Results.Json(new
        {
            status = "Healthy",
            probe = "live",
        }));

        endpoints.MapGet("/health/ready", () => Results.Json(new
        {
            status = "Healthy",
            probe = "ready",
            dependencyChecks = "NOT_IMPLEMENTED_UNTIL_W-0040",
        }));

        endpoints.MapGet("/health/startup", () => Results.Json(new
        {
            status = "Healthy",
            probe = "startup",
        }));

        return endpoints;
    }
}

namespace Ivr.Api.Health;

/// <summary>
/// Defines the health endpoints used by local and container probes.
/// </summary>
public static class HealthEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapIvrHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // Liveness answers "is this process still the process", not "is the system healthy".
        // Wiring a dependency check here would make a downstream outage restart every pod.
        endpoints.MapGet("/health/live", () => Results.Json(new
        {
            status = "Healthy",
            probe = "live",
        }));

        // W-0040 / P6-1 §6.4, DO-06. Real checks. This returned a hardcoded Healthy from P0-1
        // until now, labelled NOT_IMPLEMENTED_UNTIL_W-0040 — honest, but it meant a load balancer
        // kept sending traffic into a service that could not serve it.
        endpoints.MapGet("/health/ready", async (
            IIvrReadinessProbe probe,
            CancellationToken cancellationToken) =>
        {
            ReadinessReport report = await probe.CheckAsync(cancellationToken);
            return Results.Json(
                new
                {
                    status = report.Ready ? "Healthy" : "Unhealthy",
                    probe = "ready",
                    checks = report.Checks.Select(check => new
                    {
                        name = check.Name,
                        ready = check.Ready,
                        // Fixed phrases only: a readiness body is served to anything that asks.
                        reason = check.Reason,
                    }),
                },
                statusCode: report.StatusCode);
        });

        // Startup asks whether the process finished booting. Options validation already refuses
        // to start on an unsafe configuration, so reaching this route is the answer.
        endpoints.MapGet("/health/startup", () => Results.Json(new
        {
            status = "Healthy",
            probe = "startup",
        }));

        return endpoints;
    }
}

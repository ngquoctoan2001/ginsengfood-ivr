using System.Net;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ivr.IntegrationTests;

public sealed class HealthEndpointsTests
{
    /// <summary>
    /// Probes that answer for the process itself. These are 200 as soon as the host is up:
    /// liveness must not depend on a downstream, or a dependency outage restarts every pod.
    /// </summary>
    private static readonly string[] ProcessProbePaths =
    [
        "/health/live",
        "/health/startup",
    ];

    [Fact]
    [Trait("TestId", "IT-BOOT-02")]
    public async Task BootstrapHealthEndpointsReturnJsonAndTheRightVerdictForEachProbe()
    {
        await using WebApplicationFactory<Program> baselineApplication = new();
        await using WebApplicationFactory<Program> application =
            baselineApplication.WithWebHostBuilder(
                builder =>
                {
                    builder.UseSetting(
                        OrderCoreAllowlistOptions.TokenConfigurationKey,
                        FoundationApiTestApplication.ServiceToken);
                    builder.UseSetting(
                        InternalServiceOptions.TokenConfigurationKey,
                        InternalAdminApiTestApplication.InternalToken);

                    // W-0041 / P6-2. The readiness assertion below needs "no database" to be a
                    // fact, not an accident. Left at the default the host points at the local
                    // Postgres port, and on any machine that happens to run one the probe
                    // succeeds and the test fails for a reason that has nothing to do with the
                    // code. Port 1 cannot serve, so the premise holds everywhere.
                    builder.UseSetting(
                        "ConnectionStrings:IvrDb",
                        "Host=127.0.0.1;Port=1;Database=absent;Username=none;Password=none;Timeout=1");
                });
        using HttpClient client = application.CreateClient();

        foreach (string path in ProcessProbePaths)
        {
            using HttpResponseMessage response = await client.GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        // W-0040 / P6-1. Readiness now answers from real checks. This bootstrap host has no
        // database behind it, so 503 is the correct verdict — and the assertion was changed from
        // an unconditional 200 because that 200 was asserting the hardcoded placeholder, not a
        // property of the system. A probe that says yes with no database keeps a load balancer
        // routing traffic into a service that cannot serve it (DO-06).
        using HttpResponseMessage ready = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode);
        Assert.Equal("application/json", ready.Content.Headers.ContentType?.MediaType);

        string body = await ready.Content.ReadAsStringAsync();
        Assert.Contains("\"database\"", body, StringComparison.Ordinal);
        Assert.Contains("unreachable", body, StringComparison.Ordinal);

        // The body is served to anything that asks: no host, credential or stack frame.
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Host=", body, StringComparison.OrdinalIgnoreCase);
    }
}

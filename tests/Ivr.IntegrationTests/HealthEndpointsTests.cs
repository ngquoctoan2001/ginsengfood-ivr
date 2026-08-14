using System.Net;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ivr.IntegrationTests;

public sealed class HealthEndpointsTests
{
    private static readonly string[] ProbePaths =
    [
        "/health/live",
        "/health/ready",
        "/health/startup",
    ];

    [Fact]
    [Trait("TestId", "IT-BOOT-02")]
    public async Task BootstrapHealthEndpointsReturnJsonAndHttp200()
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
                });
        using HttpClient client = application.CreateClient();

        foreach (string path in ProbePaths)
        {
            using HttpResponseMessage response = await client.GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }
    }
}

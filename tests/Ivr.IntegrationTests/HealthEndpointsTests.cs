using System.Net;
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
        await using WebApplicationFactory<Program> application = new();
        using HttpClient client = application.CreateClient();

        foreach (string path in ProbePaths)
        {
            using HttpResponseMessage response = await client.GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }
    }
}

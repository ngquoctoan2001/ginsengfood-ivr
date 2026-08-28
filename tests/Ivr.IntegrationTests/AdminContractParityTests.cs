using Ivr.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0128. Prevents an OpenAPI-only admin route from surviving after its runtime endpoint is gone.
/// </summary>
public sealed class AdminContractParityTests
{
    private const string Prefix = "/v1/ivr/order-confirmation";

    [Fact]
    [Trait("TestId", "CT-API-ADMIN-PARITY-01")]
    public async Task EveryDocumentedAdminOperationMatchesAnAdminPolicyRuntimeEndpoint()
    {
        await using InternalAdminApiTestApplication app =
            await InternalAdminApiTestApplication.StartAsync(
                "Host=localhost;Database=ivr_contract_parity;Username=ivr;Password=unused");

        string[] runtime = app.Services.GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata
                .GetOrderedMetadata<IAuthorizeData>()
                .Any(data => data.Policy is AdminPolicies.Read
                    or AdminPolicies.Write
                    or AdminPolicies.Danger))
            .SelectMany(endpoint => endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!
                .HttpMethods.Select(method =>
                    string.Concat(
                        method.ToUpperInvariant(),
                        " ",
                        endpoint.RoutePattern.RawText?.TrimEnd('/'))))
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] documented = ReadDocumentedAdminOperations()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(documented, runtime);
    }

    private static IEnumerable<string> ReadDocumentedAdminOperations()
    {
        string file = Path.Combine(RepositoryRoot(),
            "specs", "api", "openapi", "ivr-order-confirmation.v1.yaml");
        string? path = null;
        string? method = null;
        foreach (string line in File.ReadLines(file))
        {
            if (line.StartsWith("  /", StringComparison.Ordinal)
                && !line.StartsWith("    ", StringComparison.Ordinal)
                && line.TrimEnd().EndsWith(':'))
            {
                path = line.Trim()[..^1];
                method = null;
                continue;
            }

            string trimmed = line.Trim();
            if (line.StartsWith("    ", StringComparison.Ordinal)
                && !line.StartsWith("      ", StringComparison.Ordinal)
                && trimmed.EndsWith(':')
                && trimmed[..^1] is "get" or "post" or "put" or "patch" or "delete")
            {
                method = trimmed[..^1].ToUpperInvariant();
                continue;
            }

            if (trimmed == "tags: [admin]" && path is not null && method is not null)
            {
                yield return $"{method} {Prefix}{path}";
            }
        }
    }

    private static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The repository root was not found.");
    }
}

using System.Net;
using Ivr.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0105 remediation. Before this guard existed, every console account route was reachable in
/// <c>IVR_EXECUTION_MODE=MOCK</c> by writing <c>X-Permissions: IVR_ACCOUNT_MANAGE</c> — no
/// password, no session, no token — because the routes named a permission but never named an
/// authentication scheme, so the policy-scheme selector fell through to the MOCK seam. That let
/// an unauthenticated caller list accounts, create accounts and reset the built-in admin's
/// password. MOCK is the default execution mode, so the console login was effectively optional.
///
/// The fix has two independent halves and this class holds one test for each, plus a structural
/// guard so a route added later cannot quietly re-open the hole.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ConsoleAccountSchemeTests(PostgresPersistenceFixture fixture)
{
    /// <summary>Console routes that must never be reachable without a real session.</summary>
    public static TheoryData<string, string> ProtectedConsoleRoutes() => new()
    {
        { "GET", "/v1/ivr/order-confirmation/auth/session" },
        { "POST", "/v1/ivr/order-confirmation/auth/sign-out" },
        { "GET", "/v1/ivr/order-confirmation/accounts" },
        { "GET", "/v1/ivr/order-confirmation/accounts/me" },
        { "GET", "/v1/ivr/order-confirmation/accounts/11111111-1111-4111-8111-111111111111" },
        { "POST", "/v1/ivr/order-confirmation/accounts" },
        { "PATCH", "/v1/ivr/order-confirmation/accounts/11111111-1111-4111-8111-111111111111" },
        {
            "POST",
            "/v1/ivr/order-confirmation/accounts/11111111-1111-4111-8111-111111111111:reset-password"
        },
        { "DELETE", "/v1/ivr/order-confirmation/accounts/11111111-1111-4111-8111-111111111111" },
        { "GET", "/v1/ivr/order-confirmation/account-roles" },
    };

    [Theory]
    [MemberData(nameof(ProtectedConsoleRoutes))]
    [Trait("TestId", "IT-ACCOUNT-SCHEME-06")]
    public async Task MockPermissionHeadersCannotReachAnyConsoleRoute(string method, string path)
    {
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);

        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Headers.Add("X-Correlation-Id", $"corr-mock-bypass-{Guid.NewGuid():N}");
        request.Headers.Add("X-Actor-Id", "admin");
        request.Headers.Add("X-Mock-Actor-Id", "admin");
        request.Headers.Add(
            "X-Permissions",
            "IVR_ACCOUNT_VIEW,IVR_ACCOUNT_MANAGE,IVR_ACCOUNT_PASSWORD_RESET,IVR_ACCOUNT_SELF_VIEW");
        request.Headers.Add("Idempotency-Key", $"mock-bypass-{Guid.NewGuid():N}");

        using HttpResponseMessage response = await app.Client.SendAsync(request);

        // 401, not 403: naming the scheme means the console handler is the only one consulted,
        // and it produces no principal at all for a caller with no bearer token.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains(
            "IVR_UNAUTHENTICATED",
            await response.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Second half of the fix, proven on its own: the MOCK seam refuses to mint the account
    /// permissions even when the route forgets to pin the scheme. The probe this drives is
    /// deliberately unpinned, so a pass here is attributable to
    /// <see cref="IvrPermissions.ConsoleSessionOnly"/> and nothing else.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-ACCOUNT-SCHEME-06")]
    public async Task MockSeamCannotMintAccountPermissionsEvenOnAnUnpinnedRoute()
    {
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/v1/ivr/order-confirmation/rbac/account-view-unpinned");
        request.Headers.Add("X-Correlation-Id", $"corr-mock-mint-{Guid.NewGuid():N}");
        request.Headers.Add("X-Actor-Id", "admin");
        request.Headers.Add("X-Mock-Actor-Id", "admin");
        request.Headers.Add("X-Permissions", "IVR_ACCOUNT_VIEW,IVR_QUEUE_VIEW");

        using HttpResponseMessage response = await app.Client.SendAsync(request);

        // Authenticated by the seam, but the account permission was never minted, so the
        // permission requirement is unmet: forbidden rather than unauthenticated.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains(
            "IVR_FORBIDDEN_CALLER",
            await response.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Structural guard. The two tests above drive the routes that exist today; this one fails
    /// when a console route is added without the pin, which is the way the hole would come back.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-ACCOUNT-SCHEME-06")]
    public async Task EveryConsoleRoutePinsTheConsoleSchemeAndOnlySignInIsAnonymous()
    {
        await using ConsoleAccountApiTestApplication app =
            await ConsoleAccountApiTestApplication.StartAsync(fixture.ConnectionString);
        var policyProvider = app.Services.GetRequiredService<IAuthorizationPolicyProvider>();
        RouteEndpoint[] endpoints = app.Services.GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => IsConsoleRoute(endpoint.RoutePattern.RawText))
            .ToArray();

        // A rename that silently emptied this set would turn the assertions below into a
        // vacuous pass, so the count is pinned to the ten routes the console actually exposes.
        Assert.Equal(10, endpoints.Length);

        foreach (RouteEndpoint endpoint in endpoints)
        {
            IAuthorizeData[] authorizeData = endpoint.Metadata.OfType<IAuthorizeData>().ToArray();
            AuthorizationPolicy combined =
                await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData)
                ?? throw new InvalidOperationException(
                    $"{endpoint.RoutePattern.RawText} has no authorization policy at all.");

            Assert.True(
                combined.AuthenticationSchemes.Count > 0,
                $"{endpoint.RoutePattern.RawText} names no authentication scheme, so the MOCK "
                + "permission seam can satisfy it.");
            Assert.Equal(
                [ConsoleSessionAuthenticationHandler.SchemeName],
                combined.AuthenticationSchemes);
            Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        }

        Endpoint signIn = Assert.Single(
            app.Services.GetRequiredService<EndpointDataSource>().Endpoints,
            endpoint => endpoint is RouteEndpoint route
                && route.RoutePattern.RawText?.EndsWith("auth/sign-in", StringComparison.Ordinal)
                    == true);
        Assert.NotNull(signIn.Metadata.GetMetadata<IAllowAnonymous>());
    }

    /// <summary>
    /// Production console routes only. The <c>/rbac/</c> probes are test scaffolding, and one of
    /// them is unpinned on purpose (see the test above).
    /// </summary>
    private static bool IsConsoleRoute(string? pattern) =>
        pattern is not null
        && !pattern.Contains("/rbac/", StringComparison.Ordinal)
        && (pattern.Contains("/accounts", StringComparison.Ordinal)
            || pattern.EndsWith("/account-roles", StringComparison.Ordinal)
            || pattern.EndsWith("/auth/session", StringComparison.Ordinal)
            || pattern.EndsWith("/auth/sign-out", StringComparison.Ordinal));
}

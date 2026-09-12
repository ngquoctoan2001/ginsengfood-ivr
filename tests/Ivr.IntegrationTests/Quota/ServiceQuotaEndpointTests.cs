using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Ivr.Api.Auth;
using Ivr.Infrastructure.Quota;

namespace Ivr.IntegrationTests.Quota;

/// <summary>
/// W-0282 / B2. The ceiling exercised through the real API pipeline, because the thing being
/// proved is a wire behaviour the published contract has promised since v1 and nothing had ever
/// produced: an HTTP <c>429</c> carrying <c>IVR_RATE_LIMITED</c>.
/// <para>
/// No database. These assertions are about middleware order and the error envelope, and a
/// container would only make them slower and less specific about what failed.
/// </para>
/// </summary>
public sealed class ServiceQuotaEndpointTests
{
    private static Dictionary<string, string?> Quota(int requestsPerWindow) =>
        new(StringComparer.Ordinal)
        {
            [$"{ServiceQuotaOptions.SectionName}:Enabled"] = "true",
            [$"{ServiceQuotaOptions.SectionName}:WindowSeconds"] = "300",
            [$"{ServiceQuotaOptions.SectionName}:RequestsPerWindow"] =
                requestsPerWindow.ToString(CultureInfo.InvariantCulture),
        };

    private static HttpRequestMessage OrderCoreRequest()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "/order-core");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            FoundationApiTestApplication.ServiceToken);
        request.Headers.Add(
            OrderCoreAllowlistMiddleware.SourceHeaderName,
            OrderCoreAllowlistOptions.SourceSystem);
        return request;
    }

    private static HttpRequestMessage DangerRequest(string actor)
    {
        HttpRequestMessage request = new(HttpMethod.Get, "/permission");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestAdminTokens.Danger);
        request.Headers.Add(AdminScopeGuard.ScopeHeaderName, AdminScopeGuard.DangerScopeValue);
        request.Headers.Add(AdminScopeGuard.ActorHeaderName, actor);
        request.Headers.Add(AdminScopeGuard.ReasonHeaderName, "W-0282 quota rehearsal");
        return request;
    }

    [Fact]
    [Trait("TestId", "IT-QUOTA-01")]
    public async Task TheContractOwnRateLimitedCodeFinallyAppearsOnTheWire()
    {
        await using FoundationApiTestApplication app = await FoundationApiTestApplication.StartAsync(
            extraConfiguration: Quota(requestsPerWindow: 2));

        for (int i = 0; i < 2; i++)
        {
            using HttpRequestMessage spend = OrderCoreRequest();
            using HttpResponseMessage allowed = await app.Client.SendAsync(spend);
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        }

        using HttpRequestMessage overBudget = OrderCoreRequest();
        using HttpResponseMessage refused = await app.Client.SendAsync(overBudget);

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        JsonObject error = JsonNode.Parse(await refused.Content.ReadAsStringAsync())!
            .AsObject()["error"]!.AsObject();
        Assert.Equal("IVR_RATE_LIMITED", error["code"]!.GetValue<string>());

        JsonObject details = error["details"]!.AsObject();
        Assert.Equal("2", details["requests_per_window"]!.GetValue<string>());
        Assert.True(
            int.Parse(
                details["retry_after_seconds"]!.GetValue<string>(),
                CultureInfo.InvariantCulture) >= 1,
            "A caller told to retry after zero seconds retries at once and is refused again.");
    }

    /// <summary>
    /// The ceiling must never shadow an authentication failure. 266 of the 460 cases in the API
    /// behaviour matrix assert a <c>401</c> or <c>403</c> for a caller with the wrong credential;
    /// a ceiling in front of authentication would rewrite every one of those into a <c>429</c>
    /// once a window was exhausted, and the matrix would be asserting the wrong contract.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-QUOTA-02")]
    public async Task AnExhaustedWindowStillAnswersAnUnauthenticatedCallerWith401()
    {
        await using FoundationApiTestApplication app = await FoundationApiTestApplication.StartAsync(
            extraConfiguration: Quota(requestsPerWindow: 1));

        using (HttpRequestMessage first = OrderCoreRequest())
        using (HttpResponseMessage spend = await app.Client.SendAsync(first))
        {
            Assert.Equal(HttpStatusCode.OK, spend.StatusCode);
        }

        using (HttpRequestMessage second = OrderCoreRequest())
        using (HttpResponseMessage exhausted = await app.Client.SendAsync(second))
        {
            Assert.Equal(HttpStatusCode.TooManyRequests, exhausted.StatusCode);
        }

        using HttpRequestMessage noCredential = new(HttpMethod.Get, "/order-core");
        noCredential.Headers.Add(
            OrderCoreAllowlistMiddleware.SourceHeaderName,
            OrderCoreAllowlistOptions.SourceSystem);
        using HttpResponseMessage unauthenticated = await app.Client.SendAsync(noCredential);

        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
    }

    /// <summary>
    /// <c>X-Actor-Id</c> is written by the caller about itself. Keying a budget on it would let one
    /// client hold an unlimited number of budgets by varying a string, so the admin surface is
    /// billed by credential tier and the actor header changes nothing about the budget.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-QUOTA-03")]
    public async Task ChangingTheSelfAssertedActorDoesNotBuyASecondBudget()
    {
        await using FoundationApiTestApplication app = await FoundationApiTestApplication.StartAsync(
            extraConfiguration: Quota(requestsPerWindow: 1));

        using (HttpRequestMessage byOne = DangerRequest("operator-one"))
        using (HttpResponseMessage first = await app.Client.SendAsync(byOne))
        {
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        }

        using HttpRequestMessage byTwo = DangerRequest("operator-two");
        using HttpResponseMessage second = await app.Client.SendAsync(byTwo);

        Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
    }

    /// <summary>
    /// The default is off, and that default is what keeps every existing suite meaning what it
    /// meant: the behaviour matrix fires 460 requests as a handful of accounts inside one window.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-QUOTA-04")]
    public async Task WithNoQuotaConfiguredNothingIsRefused()
    {
        await using FoundationApiTestApplication app =
            await FoundationApiTestApplication.StartAsync();

        for (int i = 0; i < 50; i++)
        {
            using HttpRequestMessage request = OrderCoreRequest();
            using HttpResponseMessage response = await app.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    /// <summary>
    /// A per-account override binds to the account name the middleware derives, which for the
    /// shared Order Core credential is the source system rather than a principal.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-QUOTA-05")]
    public async Task APerAccountCeilingBindsToTheAccountTheMiddlewareDerives()
    {
        string accountKey = $"{ServiceQuotaOptions.SectionName}:Accounts:"
            + $"{OrderCoreAllowlistOptions.SourceSystem}:RequestsPerWindow";
        Dictionary<string, string?> settings = new(StringComparer.Ordinal)
        {
            [$"{ServiceQuotaOptions.SectionName}:Enabled"] = "true",
            [$"{ServiceQuotaOptions.SectionName}:WindowSeconds"] = "300",
            [$"{ServiceQuotaOptions.SectionName}:RequestsPerWindow"] = "1000",
            [accountKey] = "1",
        };
        await using FoundationApiTestApplication app =
            await FoundationApiTestApplication.StartAsync(extraConfiguration: settings);

        using (HttpRequestMessage inBudget = OrderCoreRequest())
        using (HttpResponseMessage allowed = await app.Client.SendAsync(inBudget))
        {
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        }

        using HttpRequestMessage overBudget = OrderCoreRequest();
        using HttpResponseMessage refused = await app.Client.SendAsync(overBudget);

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
    }
}

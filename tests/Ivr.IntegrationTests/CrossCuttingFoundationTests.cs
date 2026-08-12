using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Api.Foundation;
using Ivr.Domain.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

public sealed class CrossCuttingFoundationTests
{
    [Fact]
    [Trait("TestId", "UT-FND-CORR-02")]
    public async Task CorrelationIdIsAcceptedGeneratedReturnedAndPropagatedOutbound()
    {
        await using FoundationApiTestApplication application =
            await FoundationApiTestApplication.StartAsync();
        using HttpRequestMessage request = new(HttpMethod.Get, "/correlation-outbound");
        request.Headers.Add("X-Correlation-Id", "corr-inbound-1");

        using HttpResponseMessage response = await application.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "corr-inbound-1",
            response.Headers.GetValues("X-Correlation-Id").Single());
        Assert.Equal("corr-inbound-1", application.CapturedCorrelationId);

        using HttpResponseMessage generated = await application.Client.GetAsync(
            "/correlation-outbound");
        string generatedId = generated.Headers.GetValues("X-Correlation-Id").Single();
        Assert.False(string.IsNullOrWhiteSpace(generatedId));
        Assert.Equal(generatedId, application.CapturedCorrelationId);

        using HttpRequestMessage unsafeRequest = new(
            HttpMethod.Get,
            "/correlation-outbound");
        unsafeRequest.Headers.Add("X-Correlation-Id", "0912341234");
        using HttpResponseMessage sanitized = await application.Client.SendAsync(unsafeRequest);
        string sanitizedId = sanitized.Headers.GetValues("X-Correlation-Id").Single();
        Assert.NotEqual("0912341234", sanitizedId);
        Assert.Equal(sanitizedId, application.CapturedCorrelationId);

        using HttpRequestMessage authoritativeRequest = new(
            HttpMethod.Get,
            "/correlation-outbound-preset");
        authoritativeRequest.Headers.Add("X-Correlation-Id", "authoritative-context");
        using HttpResponseMessage authoritative = await application.Client.SendAsync(
            authoritativeRequest);
        Assert.Equal("authoritative-context", application.CapturedCorrelationId);
    }

    [Fact]
    [Trait("TestId", "UT-FND-RBAC-03")]
    public async Task MockPermissionProviderEnforcesTheFixedPermissionCatalog()
    {
        await using FoundationApiTestApplication application =
            await FoundationApiTestApplication.StartAsync();

        using HttpResponseMessage denied = await application.Client.GetAsync("/permission");
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using JsonDocument deniedBody = JsonDocument.Parse(
            await denied.Content.ReadAsStringAsync());
        Assert.Equal(
            IvrErrorCodes.ForbiddenCaller,
            deniedBody.RootElement.GetProperty("error").GetProperty("code").GetString());

        using HttpRequestMessage allowedRequest = new(HttpMethod.Get, "/permission");
        allowedRequest.Headers.Add("X-Permissions", IvrPermissions.QueuePause);
        using HttpResponseMessage allowed = await application.Client.SendAsync(allowedRequest);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    [Fact]
    [Trait("TestId", "UT-FND-RBAC-08")]
    public async Task MockPermissionHeaderIsRejectedOutsideMockAndCannotBeRegistered()
    {
        await using FoundationApiTestApplication application =
            await FoundationApiTestApplication.StartAsync("LAB");
        using HttpRequestMessage request = new(HttpMethod.Get, "/correlation-outbound");
        request.Headers.Add("X-Permissions", IvrPermissions.QueuePause);

        using HttpResponseMessage response = await application.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using ConfigurationManager configuration = new();
        configuration["IVR_EXECUTION_MODE"] = "LAB";
        configuration[IvrApiServiceCollectionExtensions.RegisterMockPermissionProviderKey] = "true";
        ServiceCollection services = new();
        Assert.Throws<InvalidOperationException>(
            () => services.AddIvrApiFoundation(configuration));
    }

    [Fact]
    [Trait("TestId", "UT-FND-ALLOW-04")]
    public async Task OrderCoreAllowlistRequiresExactSourceAndServiceToken()
    {
        await using FoundationApiTestApplication application =
            await FoundationApiTestApplication.StartAsync();

        using HttpRequestMessage missingToken = new(HttpMethod.Get, "/order-core");
        missingToken.Headers.Add("X-Source-System", "order-core");
        using HttpResponseMessage unauthenticated = await application.Client.SendAsync(missingToken);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);

        using HttpRequestMessage wrongSource = CreateOrderCoreRequest("sales-ui");
        using HttpResponseMessage forbidden = await application.Client.SendAsync(wrongSource);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using HttpRequestMessage wrongToken = CreateOrderCoreRequest(
            "order-core",
            "wrong-local-token");
        using HttpResponseMessage invalidCredential = await application.Client.SendAsync(
            wrongToken);
        Assert.Equal(HttpStatusCode.Forbidden, invalidCredential.StatusCode);

        using HttpRequestMessage valid = CreateOrderCoreRequest("order-core");
        using HttpResponseMessage accepted = await application.Client.SendAsync(valid);
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
    }

    [Fact]
    [Trait("TestId", "UT-FND-ERR-05")]
    public async Task ErrorEnvelopeUsesStableCodesAndRedactsUnexpectedFailures()
    {
        await using FoundationApiTestApplication application =
            await FoundationApiTestApplication.StartAsync();

        using HttpResponseMessage known = await application.Client.GetAsync("/known-error");
        Assert.Equal(HttpStatusCode.Conflict, known.StatusCode);
        using JsonDocument knownBody = JsonDocument.Parse(await known.Content.ReadAsStringAsync());
        Assert.Equal(
            IvrErrorCodes.IdempotencyConflict,
            knownBody.RootElement.GetProperty("error").GetProperty("code").GetString());

        using HttpResponseMessage unexpected = await application.Client.GetAsync(
            "/unexpected-error");
        string unexpectedBody = await unexpected.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.InternalServerError, unexpected.StatusCode);
        Assert.DoesNotContain("0912341234", unexpectedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", unexpectedBody, StringComparison.Ordinal);
        using JsonDocument parsed = JsonDocument.Parse(unexpectedBody);
        JsonElement error = parsed.RootElement.GetProperty("error");
        Assert.Equal(IvrErrorCodes.InternalError, error.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            error.GetProperty("correlationId").GetString()));
    }

    private static HttpRequestMessage CreateOrderCoreRequest(
        string source,
        string token = FoundationApiTestApplication.ServiceToken)
    {
        HttpRequestMessage request = new(HttpMethod.Get, "/order-core");
        request.Headers.Add("X-Source-System", source);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token);
        return request;
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Api.Foundation;
using Ivr.Api.Middleware;
using Ivr.Domain.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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

        using HttpResponseMessage pii = await application.Client.GetAsync("/pii-error");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, pii.StatusCode);
        using JsonDocument piiBody = JsonDocument.Parse(await pii.Content.ReadAsStringAsync());
        Assert.Equal(
            IvrErrorCodes.PiiPolicyViolation,
            piiBody.RootElement.GetProperty("error").GetProperty("code").GetString());

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

    [Fact]
    [Trait("TestId", "IT-FND-ERR-12")]
    public async Task ErrorEnvelopeCatchesAuthenticationStageFailures()
    {
        await using FoundationApiTestApplication application =
            await FoundationApiTestApplication.StartAsync(throwDuringAuthentication: true);

        using HttpResponseMessage response = await application.Client.GetAsync("/permission");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("0912341234", body, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
        using JsonDocument parsed = JsonDocument.Parse(body);
        JsonElement error = parsed.RootElement.GetProperty("error");
        Assert.Equal(IvrErrorCodes.InternalError, error.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            error.GetProperty("correlationId").GetString()));
    }

    [Fact]
    [Trait("TestId", "IT-FND-ERR-13")]
    public async Task ErrorWriterAbortsWithoutRewritingAStartedResponse()
    {
        await using FoundationApiTestApplication application =
            await FoundationApiTestApplication.StartAsync();
        StartedResponseFeature responseFeature = new();
        RecordingRequestLifetimeFeature lifetimeFeature = new();
        FeatureCollection features = new();
        features.Set<IHttpResponseFeature>(responseFeature);
        features.Set<IHttpRequestLifetimeFeature>(lifetimeFeature);
        DefaultHttpContext context = new(features);
        IvrErrorResponseWriter writer = application.Services
            .GetRequiredService<IvrErrorResponseWriter>();

        await writer.WriteAsync(context, IvrErrors.InternalError());

        Assert.True(lifetimeFeature.WasAborted);
        Assert.Equal(StatusCodes.Status200OK, responseFeature.StatusCode);
        Assert.Equal(0, responseFeature.Body.Length);
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

    private sealed class RecordingRequestLifetimeFeature : IHttpRequestLifetimeFeature
    {
        public CancellationToken RequestAborted { get; set; }

        public bool WasAborted { get; private set; }

        public void Abort() => WasAborted = true;
    }

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string? ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = new MemoryStream();

        public bool HasStarted => true;

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }
    }
}

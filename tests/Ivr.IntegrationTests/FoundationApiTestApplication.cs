using Ivr.Api.Auth;
using Ivr.Api.Foundation;
using Ivr.Api.Middleware;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

internal sealed class FoundationApiTestApplication : IAsyncDisposable
{
    public const string ServiceToken = "local-foundation-test-token";

    private readonly WebApplication application;
    private readonly CaptureHandler captureHandler;

    private FoundationApiTestApplication(
        WebApplication application,
        HttpClient client,
        CaptureHandler captureHandler)
    {
        this.application = application;
        Client = client;
        this.captureHandler = captureHandler;
    }

    public HttpClient Client { get; }

    public string? CapturedCorrelationId => captureHandler.CorrelationId;

    public static async Task<FoundationApiTestApplication> StartAsync(
        string executionMode = IvrOptions.MockExecutionMode)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                EnvironmentName = "Testing",
            });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = executionMode,
                ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
                ["SIM_PROVIDER"] = "MOCK",
                ["ConnectionStrings:IvrDb"] =
                    "Host=localhost;Port=5432;Database=ivr_test;Username=ivr;Password=unused",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                [OrderCoreAllowlistOptions.TokenConfigurationKey] = ServiceToken,
            });

        builder.Services.AddIvrFoundation(builder.Configuration);
        builder.Services.AddIvrApiFoundation(builder.Configuration);
        builder.Services.AddSingleton<CaptureHandler>();
        builder.Services.AddHttpClient("capture")
            .ConfigurePrimaryHttpMessageHandler(
                provider => provider.GetRequiredService<CaptureHandler>());

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseIvrApiFoundation();

        app.MapGet("/permission", static () => Results.Ok())
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueuePause));
        app.MapGet("/order-core", static () => Results.Ok())
            .AllowAnonymous()
            .WithMetadata(new RequireOrderCoreAttribute());
        app.MapGet(
                "/correlation-outbound",
                static async (IHttpClientFactory factory, CancellationToken cancellationToken) =>
                {
                    using HttpResponseMessage response = await factory
                        .CreateClient("capture")
                        .GetAsync("https://capture.invalid", cancellationToken);
                    response.EnsureSuccessStatusCode();
                    return Results.Ok();
                })
            .AllowAnonymous();
        app.MapGet(
                "/correlation-outbound-preset",
                static async (IHttpClientFactory factory, CancellationToken cancellationToken) =>
                {
                    using HttpRequestMessage outbound = new(
                        HttpMethod.Get,
                        "https://capture.invalid");
                    outbound.Headers.Add("X-Correlation-Id", "caller-supplied-value");
                    using HttpResponseMessage response = await factory
                        .CreateClient("capture")
                        .SendAsync(outbound, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    return Results.Ok();
                })
            .AllowAnonymous();
        app.MapGet(
                "/known-error",
                (Func<IResult>)(static () => throw IvrErrors.IdempotencyConflict()))
            .AllowAnonymous();
        app.MapGet(
                "/unexpected-error",
                (Func<IResult>)(static () => throw new InvalidOperationException(
                    "customer 0912341234")))
            .AllowAnonymous();

        await app.StartAsync();
        return new FoundationApiTestApplication(
            app,
            app.GetTestClient(),
            app.Services.GetRequiredService<CaptureHandler>());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.DisposeAsync();
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public string? CorrelationId { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CorrelationId = request.Headers.TryGetValues(
                Ivr.Infrastructure.Correlation.CorrelationPropagationHandler.HeaderName,
                out IEnumerable<string>? values)
                ? values.SingleOrDefault()
                : null;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}

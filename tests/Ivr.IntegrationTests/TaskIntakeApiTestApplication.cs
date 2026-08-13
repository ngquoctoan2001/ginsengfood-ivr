using Ivr.Api.Auth;
using Ivr.Api.Foundation;
using Ivr.Api.Intake;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

internal sealed class TaskIntakeApiTestApplication : IAsyncDisposable
{
    public static readonly DateTimeOffset Now =
        new(2026, 8, 13, 6, 0, 0, TimeSpan.Zero);

    private readonly WebApplication application;

    private TaskIntakeApiTestApplication(WebApplication application, HttpClient client)
    {
        this.application = application;
        Client = client;
        Store = application.Services.GetRequiredService<InMemoryTaskIntakeStore>();
        Audit = application.Services.GetRequiredService<InMemoryAuditLogger>();
    }

    public HttpClient Client { get; }

    public InMemoryTaskIntakeStore Store { get; }

    public InMemoryAuditLogger Audit { get; }

    public static async Task<TaskIntakeApiTestApplication> StartAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
                ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
                ["SIM_PROVIDER"] = "MOCK",
                ["ConnectionStrings:IvrDb"] =
                    "Host=localhost;Database=ivr_test;Username=ivr;Password=unused",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                [OrderCoreAllowlistOptions.TokenConfigurationKey] =
                    FoundationApiTestApplication.ServiceToken,
            });
        builder.Services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
        builder.Services.AddIvrFoundation(builder.Configuration);
        builder.Services.AddIvrApiFoundation(builder.Configuration);

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseIvrApiFoundation();
        app.MapIvrTaskIntakeEndpoint();
        await app.StartAsync();
        return new TaskIntakeApiTestApplication(app, app.GetTestClient());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.DisposeAsync();
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

using Ivr.Api.Accounts;
using Ivr.Api.Admin;
using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Api.Foundation;
using Ivr.Api.Internal;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

internal sealed class ConsoleAccountApiTestApplication : IAsyncDisposable
{
    private readonly WebApplication application;

    private ConsoleAccountApiTestApplication(WebApplication application)
    {
        this.application = application;
        Client = application.GetTestClient();
    }

    public HttpClient Client { get; }

    public IServiceProvider Services => application.Services;

    public static async Task<ConsoleAccountApiTestApplication> StartAsync(
        string connectionString)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
            ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
            ["SIM_PROVIDER"] = "MOCK",
            ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
            ["ConnectionStrings:IvrDb"] = connectionString,
            [OrderCoreAllowlistOptions.TokenConfigurationKey] =
                "account-api-test-token-at-least-24-chars",
            [InternalServiceOptions.TokenConfigurationKey] =
                "console-api-internal-token-at-least-24-chars",
        });
        builder.Services.AddIvrFoundation(builder.Configuration);
        builder.Services.AddIvrApiFoundation(builder.Configuration);

        // W-0109. The script lifecycle service lives in the internal-admin service set, and a
        // minimal-API handler whose service parameter is unregistered is inferred as a body
        // parameter instead — which fails route construction on the GET, not at the call.
        builder.Services.AddIvrInternalAdminApi(builder.Configuration);

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseIvrApiFoundation();
        app.MapIvrConsoleAccountEndpoints();

        // W-0109. Script lifecycle is mapped here rather than in the admin-config harness
        // because its routes are pinned to the console session scheme, so they can only be
        // exercised by a test that actually signs in.
        app.MapIvrScriptLifecycleEndpoints();
        var probes = app.MapGroup("/v1/ivr/order-confirmation");
        probes.MapGet("/rbac/queue", static () => Results.Ok())
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueueView));
        probes.MapPost("/rbac/sim-disable", static () => Results.Ok())
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.SimDisable));
        probes.MapPost("/rbac/manual-retry", static () => Results.Ok())
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.ManualRetry));
        probes.MapPost("/rbac/queue-pause", static () => Results.Ok())
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.QueuePause));

        // W-0105. Deliberately NOT pinned to the console scheme: this probe stands in for a
        // future account route whose author forgets the pin, and lets IT-ACCOUNT-SCHEME-06
        // prove the MOCK seam still cannot mint IVR_ACCOUNT_VIEW on its own. Production console
        // routes are asserted to be pinned by the same test, which skips this /rbac/ prefix.
        probes.MapGet("/rbac/account-view-unpinned", static () => Results.Ok())
            .WithMetadata(new RequirePermissionAttribute(IvrPermissions.AccountView));
        await app.StartAsync();
        return new ConsoleAccountApiTestApplication(app);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.DisposeAsync();
    }
}

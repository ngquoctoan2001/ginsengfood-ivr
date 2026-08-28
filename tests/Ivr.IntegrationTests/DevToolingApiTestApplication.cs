using Ivr.Api.Admin;
using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Api.Foundation;
using Ivr.Api.Internal;
using Ivr.Infrastructure.Configuration;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.DevTooling;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Speech;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

/// <summary>
/// Hosts the UI-07 developer surface (W-0112).
/// <para>
/// The environment name, execution mode and real-call flag are parameters rather than constants
/// because the production case is the one worth proving, and it can only be proved by starting a
/// host configured the way production is.
/// </para>
/// </summary>
internal sealed class DevToolingApiTestApplication : IAsyncDisposable
{
    private readonly WebApplication application;

    private DevToolingApiTestApplication(WebApplication application)
    {
        this.application = application;
        Client = application.GetTestClient();
    }

    public HttpClient Client { get; }

    public IServiceProvider Services => application.Services;

    public static async Task<DevToolingApiTestApplication> StartAsync(
        string connectionString,
        string environmentName = "Testing",
        string executionMode = IvrOptions.MockExecutionMode,
        bool configureSeedDirectory = true)
    {
        // PRODUCTION_REAL only validates against TARGET_V1 + VENDOR; IvrOptionsValidator refuses
        // every other combination outright. So a production-configured host cannot be started
        // with the MOCK providers, and the test has to configure it the way production is.
        bool production = executionMode == IvrOptions.ProductionRealExecutionMode;
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = environmentName });
        builder.WebHost.UseTestServer();
        var settings = new Dictionary<string, string?>
        {
            ["IVR_EXECUTION_MODE"] = executionMode,
            ["SALES_PROVIDER"] = production ? "TARGET_V1" : "FAKE_TARGET_V1",
            ["SIM_PROVIDER"] = production ? "VENDOR" : "MOCK",
            ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",

            // FAKE_DETERMINISTIC is refused outside MOCK, so a production-configured host has to
            // name a provider it is allowed to hold. UNSELECTED is the honest one: this host
            // exists to be refused at the route, and it never speaks.
            [$"{TtsProviderOptions.SectionName}:{nameof(TtsProviderOptions.Provider)}"] =
                production ? TtsProviderOptions.UnselectedProvider : TtsProviderOptions.FakeProvider,
            ["ConnectionStrings:IvrDb"] = connectionString,
            [Ivr.Api.Auth.AdminAccessOptions.ReadTokenConfigurationKey] = TestAdminTokens.Read,
            [Ivr.Api.Auth.AdminAccessOptions.WriteTokenConfigurationKey] = TestAdminTokens.Write,
            [Ivr.Api.Auth.AdminAccessOptions.DangerTokenConfigurationKey] = TestAdminTokens.Danger,
            [OrderCoreAllowlistOptions.TokenConfigurationKey] =
                "dev-tooling-test-token-at-least-24-chars",
            [InternalServiceOptions.TokenConfigurationKey] =
                "dev-tooling-internal-token-at-least-24-chars",
        };
        if (configureSeedDirectory)
        {
            settings[$"{DevToolingOptions.SectionName}:{nameof(DevToolingOptions.SeedDirectory)}"] =
                SeedDirectory();
        }

        builder.Configuration.AddInMemoryCollection(settings);
        builder.Services.AddIvrFoundation(builder.Configuration);
        builder.Services.AddIvrEligibility(builder.Configuration);
        builder.Services.AddIvrFeatureFlags(builder.Configuration);
        builder.Services.AddIvrApiFoundation(builder.Configuration);
        builder.Services.AddIvrInternalAdminApi(builder.Configuration);

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseIvrApiFoundation();
        app.MapIvrDevToolingEndpoints();
        await app.StartAsync();
        return new DevToolingApiTestApplication(app);
    }

    /// <summary>
    /// The repository's own <c>seed/</c> folder. Deliberately the real fixtures rather than a
    /// copy: a seed loader tested against a copy proves it can read a file this test wrote, not
    /// that it can read the file the September acceptance sessions will actually run on.
    /// </summary>
    public static string SeedDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
            && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        return Path.Combine(
            directory?.FullName
                ?? throw new InvalidOperationException("The repository root was not found."),
            "seed");
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.DisposeAsync();
    }
}

using System.Collections.Frozen;
using Ivr.Api.Admin;
using Ivr.Api.Application;
using Ivr.Api.Auth;
using Ivr.Api.Foundation;
using Ivr.Api.Internal;
using Ivr.Api.Middleware;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ivr.IntegrationTests;

internal sealed class InternalAdminApiTestApplication : IAsyncDisposable
{
    public const string InternalToken = "p2-8-internal-service-test-token";
    private static readonly FrozenSet<string> AllowedRetryDestinations =
        new[] { "phone-ref-p2-8" }.ToFrozenSet(StringComparer.Ordinal);
    private readonly WebApplication application;

    private InternalAdminApiTestApplication(WebApplication application)
    {
        this.application = application;
        Client = application.GetTestClient();
    }

    public HttpClient Client { get; }

    public IServiceProvider Services => application.Services;

    public static async Task<InternalAdminApiTestApplication> StartAsync(
        string connectionString,
        bool retryKillSwitch = false,
        bool retryDestinationAllowed = true)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
            ["SALES_PROVIDER"] = FeatureFlagValues.FakeTargetV1,
            ["SIM_PROVIDER"] = FeatureFlagValues.MockSimProvider,
            ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
            ["ConnectionStrings:IvrDb"] = connectionString,
            [OrderCoreAllowlistOptions.TokenConfigurationKey] =
                FoundationApiTestApplication.ServiceToken,
            [InternalServiceOptions.TokenConfigurationKey] = InternalToken,
            ["Ivr:Scheduler:Enabled"] = "true",
            ["Ivr:Scheduler:TechnicalRetryLimit"] = "1",
        });

        builder.Services.AddIvrFoundation(builder.Configuration);
        builder.Services.AddIvrEligibility(builder.Configuration);
        builder.Services.AddIvrFeatureFlags(builder.Configuration);
        builder.Services.AddIvrApiFoundation(builder.Configuration);
        builder.Services.AddIvrInternalAdminApi(builder.Configuration);
        builder.Services.RemoveAll<InMemoryFeatureFlagStore>();
        builder.Services.RemoveAll<IFeatureFlagStore>();
        builder.Services.AddSingleton(provider =>
        {
            FeatureFlagSnapshot development = FeatureFlagSnapshot.SafeDefault(
                FeatureFlagEnvironments.Development) with
            {
                GlobalDialKillSwitch = retryKillSwitch,
                LabDestinationAllowlist = retryDestinationAllowed
                    ? AllowedRetryDestinations
                    : FrozenSet<string>.Empty,
            };
            return new InMemoryFeatureFlagStore(
                provider.GetRequiredService<IAuditLogger>(),
                FeatureFlagEnvironments.All.Select(environment =>
                    environment == FeatureFlagEnvironments.Development
                        ? development
                        : FeatureFlagSnapshot.SafeDefault(environment)));
        });
        builder.Services.AddSingleton<IFeatureFlagStore>(provider =>
            provider.GetRequiredService<InMemoryFeatureFlagStore>());
        builder.Services.RemoveAll<IEligibilityRepository>();
        builder.Services.AddSingleton<IEligibilityRepository, PostgresEligibilityRepository>();
        builder.Services.RemoveAll<IEligibilityCapacityProvider>();
        builder.Services.AddSingleton<IEligibilityCapacityProvider,
            AlwaysAvailableEligibilityCapacityProvider>();

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseIvrApiFoundation();
        app.MapIvrInternalLifecycleEndpoints();
        app.MapIvrAdminEndpoints();
        await app.StartAsync();
        return new InternalAdminApiTestApplication(app);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.DisposeAsync();
    }

    private sealed class AlwaysAvailableEligibilityCapacityProvider
        : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new EligibilityCapacitySnapshot(
                true,
                true,
                "P2-8-TEST-CAPACITY",
                1,
                0,
                0,
                0,
                null,
                "evidence://ivr/p2-8/test-capacity"));
    }
}

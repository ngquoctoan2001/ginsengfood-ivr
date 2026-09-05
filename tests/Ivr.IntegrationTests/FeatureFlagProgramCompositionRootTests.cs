using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Admin;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Ivr.IntegrationTests;

/// <summary>
/// Starts the shipping <see cref="Program"/> composition root in Development against the normal
/// PostgreSQL fixture. These tests intentionally do not rebuild the service graph by hand: the
/// defect was constructor selection in the graph that Program registered.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class FeatureFlagProgramCompositionRootTests(PostgresPersistenceFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-COMPROOT-FLAGHTTP-06")]
    public async Task DevelopmentProgramServesEveryFeatureFlagEnvironmentAndRejectsUnknownInput()
    {
        await fixture.ResetAsync();
        await using WebApplicationFactory<Program> baseline = new();
        await using WebApplicationFactory<Program> application = CreateApplication(baseline);
        using HttpClient client = application.CreateClient();

        using (HttpResponseMessage ready = await client.GetAsync("/health/ready"))
        {
            Assert.True(
                ready.StatusCode == HttpStatusCode.OK,
                await ready.Content.ReadAsStringAsync());
        }

        foreach (string environment in FeatureFlagEnvironments.All)
        {
            using HttpResponseMessage snapshotResponse = await SendReadAsync(
                client,
                $"/v1/ivr/order-confirmation/feature-flags/{environment}");
            Assert.Equal(HttpStatusCode.OK, snapshotResponse.StatusCode);
            using JsonDocument snapshot = JsonDocument.Parse(
                await snapshotResponse.Content.ReadAsStringAsync());
            Assert.True(snapshot.RootElement.GetProperty("providerReadable").GetBoolean());
            Assert.Equal(
                environment,
                snapshot.RootElement.GetProperty("snapshot").GetProperty("environment").GetString());

            using HttpResponseMessage killSwitchResponse = await SendReadAsync(
                client,
                $"/v1/ivr/order-confirmation/feature-flags/{environment}/kill-switch");
            Assert.Equal(HttpStatusCode.OK, killSwitchResponse.StatusCode);
            KillSwitchVerification killSwitch = (await killSwitchResponse.Content
                .ReadFromJsonAsync<KillSwitchVerification>())!;
            Assert.True(killSwitch.ProviderReadable);
            Assert.True(killSwitch.GlobalDialKillSwitch);
            Assert.False(killSwitch.RealCallsEnabled);
        }

        using HttpResponseMessage unknown = await SendReadAsync(
            client,
            "/v1/ivr/order-confirmation/feature-flags/Development");
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Contains(
            IvrErrorCodes.NotFound,
            await unknown.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "IT-COMPROOT-FLAGHTTP-07")]
    public async Task DevelopmentProgramKeepsTheKillSwitchOnWhenTheProviderIsUnavailable()
    {
        await using WebApplicationFactory<Program> baseline = new();
        await using WebApplicationFactory<Program> application = CreateApplication(
            baseline,
            services =>
            {
                services.RemoveAll<IFeatureFlagStore>();
                services.AddSingleton<IFeatureFlagStore>(new UnavailableFeatureFlagStore());
            });
        using HttpClient client = application.CreateClient();

        using HttpResponseMessage snapshot = await SendReadAsync(
            client,
            "/v1/ivr/order-confirmation/feature-flags/dev");
        Assert.Equal(HttpStatusCode.Conflict, snapshot.StatusCode);
        string snapshotBody = await snapshot.Content.ReadAsStringAsync();
        Assert.Contains(IvrErrorCodes.OperationalBlocked, snapshotBody, StringComparison.Ordinal);
        Assert.DoesNotContain(UnavailableFeatureFlagStore.SensitiveDetail, snapshotBody, StringComparison.Ordinal);

        using HttpResponseMessage killSwitchResponse = await SendReadAsync(
            client,
            "/v1/ivr/order-confirmation/feature-flags/dev/kill-switch");
        Assert.Equal(HttpStatusCode.OK, killSwitchResponse.StatusCode);
        KillSwitchVerification killSwitch = (await killSwitchResponse.Content
            .ReadFromJsonAsync<KillSwitchVerification>())!;
        Assert.False(killSwitch.ProviderReadable);
        Assert.True(killSwitch.GlobalDialKillSwitch);
        Assert.False(killSwitch.RealCallsEnabled);
    }

    private WebApplicationFactory<Program> CreateApplication(
        WebApplicationFactory<Program> baseline,
        Action<IServiceCollection>? configureTestServices = null) =>
        baseline.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(Environments.Development);
            builder.UseSetting("IVR_EXECUTION_MODE", IvrOptions.MockExecutionMode);
            builder.UseSetting("ConnectionStrings:IvrDb", fixture.ConnectionString);
            builder.UseSetting(
                OrderCoreAllowlistOptions.TokenConfigurationKey,
                FoundationApiTestApplication.ServiceToken);
            builder.UseSetting(
                InternalServiceOptions.TokenConfigurationKey,
                InternalAdminApiTestApplication.InternalToken);
            builder.UseSetting(AdminAccessOptions.ReadTokenConfigurationKey, TestAdminTokens.Read);
            builder.UseSetting(AdminAccessOptions.WriteTokenConfigurationKey, TestAdminTokens.Write);
            builder.UseSetting(AdminAccessOptions.DangerTokenConfigurationKey, TestAdminTokens.Danger);
            if (configureTestServices is not null)
            {
                builder.ConfigureTestServices(configureTestServices);
            }
        });

    private static async Task<HttpResponseMessage> SendReadAsync(HttpClient client, string path)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        TestAdminTokens.Authorize(request, AdminScope.Read);
        return await client.SendAsync(request);
    }

    private sealed class UnavailableFeatureFlagStore : IFeatureFlagStore
    {
        public const string SensitiveDetail = "provider-detail-must-not-escape";

        public Task<FeatureFlagSnapshot> ReadFreshAsync(
            string environment,
            CancellationToken cancellationToken = default) =>
            Task.FromException<FeatureFlagSnapshot>(new InvalidOperationException(SensitiveDetail));

        public Task<FeatureFlagSnapshot> ApplyAuditedAsync(
            FeatureFlagSnapshot expected,
            FeatureFlagSnapshot proposed,
            AuditEvent auditEvent,
            CancellationToken cancellationToken = default) =>
            Task.FromException<FeatureFlagSnapshot>(new InvalidOperationException(SensitiveDetail));
    }
}

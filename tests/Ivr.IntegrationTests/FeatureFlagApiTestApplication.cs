using Ivr.Api.Admin;
using Ivr.Api.Auth;
using Ivr.Api.Foundation;
using Ivr.Api.Middleware;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ivr.IntegrationTests;

internal sealed class FeatureFlagApiTestApplication : IAsyncDisposable
{
    private readonly WebApplication application;

    private FeatureFlagApiTestApplication(
        WebApplication application,
        HttpClient client,
        InMemoryFeatureFlagStore store,
        ToggleAuditLogger audit,
        ToggleRuntimeSafetyHealth runtimeSafety)
    {
        this.application = application;
        Client = client;
        Store = store;
        Audit = audit;
        RuntimeSafety = runtimeSafety;
    }

    public HttpClient Client { get; }

    public InMemoryFeatureFlagStore Store { get; }

    public ToggleAuditLogger Audit { get; }

    public ToggleRuntimeSafetyHealth RuntimeSafety { get; }

    public IServiceProvider Services => application.Services;

    public static async Task<FeatureFlagApiTestApplication> StartAsync(
        IEnumerable<FeatureFlagSnapshot>? seeds = null,
        bool runtimeAuthorizationApproved = true)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
                ["SALES_PROVIDER"] = FeatureFlagValues.FakeTargetV1,
                ["SIM_PROVIDER"] = FeatureFlagValues.MockSimProvider,
                [Ivr.Api.Auth.AdminAccessOptions.ReadTokenConfigurationKey] = TestAdminTokens.Read,
                [Ivr.Api.Auth.AdminAccessOptions.WriteTokenConfigurationKey] = TestAdminTokens.Write,
                [Ivr.Api.Auth.AdminAccessOptions.DangerTokenConfigurationKey] = TestAdminTokens.Danger,
                ["ConnectionStrings:IvrDb"] =
                    "Host=localhost;Database=ivr_test;Username=ivr;Password=unused",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                [OrderCoreAllowlistOptions.TokenConfigurationKey] =
                    FoundationApiTestApplication.ServiceToken,
            });

        builder.Services.AddIvrFoundation(
            builder.Configuration,
            useInMemoryTestDoubles: true);
        builder.Services.AddIvrFeatureFlags(builder.Configuration);
        builder.Services.AddIvrApiFoundation(builder.Configuration);
        builder.Services.RemoveAll<IAuditLogger>();
        builder.Services.AddSingleton<ToggleAuditLogger>();
        builder.Services.AddSingleton<IAuditLogger>(
            provider => provider.GetRequiredService<ToggleAuditLogger>());
        builder.Services.RemoveAll<InMemoryFeatureFlagStore>();
        builder.Services.RemoveAll<IFeatureFlagStore>();
        builder.Services.AddSingleton(
            provider => new InMemoryFeatureFlagStore(
                provider.GetRequiredService<IAuditLogger>(),
                seeds ?? FeatureFlagEnvironments.All.Select(
                    FeatureFlagSnapshot.SafeDefault)));
        builder.Services.AddSingleton<IFeatureFlagStore>(
            provider => provider.GetRequiredService<InMemoryFeatureFlagStore>());
        builder.Services.RemoveAll<IRuntimeGateAuthorization>();
        builder.Services.AddSingleton<IRuntimeGateAuthorization>(
            runtimeAuthorizationApproved
                ? new ApprovedRuntimeGateAuthorization()
                : new PendingRuntimeGateAuthorization());
        builder.Services.RemoveAll<IFourEyesApprovalVerifier>();
        builder.Services.AddSingleton<IFourEyesApprovalVerifier,
            ApprovedFourEyesVerifier>();
        builder.Services.RemoveAll<IRuntimeSafetyHealth>();
        builder.Services.AddSingleton<ToggleRuntimeSafetyHealth>();
        builder.Services.AddSingleton<IRuntimeSafetyHealth>(
            provider => provider.GetRequiredService<ToggleRuntimeSafetyHealth>());

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseIvrApiFoundation();
        app.MapIvrFeatureFlagEndpoints();

        await app.StartAsync();
        return new FeatureFlagApiTestApplication(
            app,
            app.GetTestClient(),
            app.Services.GetRequiredService<InMemoryFeatureFlagStore>(),
            app.Services.GetRequiredService<ToggleAuditLogger>(),
            app.Services.GetRequiredService<ToggleRuntimeSafetyHealth>());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await application.DisposeAsync();
    }

    internal sealed class ToggleAuditLogger(TimeProvider timeProvider) : IAuditLogger
    {
        private readonly InMemoryAuditLogger inner = new(timeProvider);

        public bool Available { get; set; } = true;

        public IReadOnlyCollection<AuditLogEntry> Entries => inner.Entries;

        public Task<AuditLogEntry> AppendAsync(
            AuditEvent auditEvent,
            CancellationToken cancellationToken = default) =>
            Available
                ? inner.AppendAsync(auditEvent, cancellationToken)
                : throw IvrErrors.OperationalBlocked("Audit provider is unavailable.");
    }

    internal sealed class ToggleRuntimeSafetyHealth : IRuntimeSafetyHealth
    {
        public bool AuditProviderHealthy { get; set; } = true;

        public Task<bool> IsAuditProviderHealthyAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(AuditProviderHealthy);
    }

    private sealed class ApprovedRuntimeGateAuthorization : IRuntimeGateAuthorization
    {
        public Task<bool> IsApprovedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class ApprovedFourEyesVerifier : IFourEyesApprovalVerifier
    {
        public Task<string?> VerifyAsync(
            string approvalReference,
            string proposerActorId,
            FeatureFlagSnapshot before,
            FeatureFlagSnapshot after,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(
                approvalReference == "approval-1" ? "approver-2" : null);
    }
}

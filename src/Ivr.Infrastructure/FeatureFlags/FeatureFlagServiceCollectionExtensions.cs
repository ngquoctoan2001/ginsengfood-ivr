using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ivr.Infrastructure.Persistence;

namespace Ivr.Infrastructure.FeatureFlags;

public static class FeatureFlagServiceCollectionExtensions
{
    public static IServiceCollection AddIvrFeatureFlags(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        string executionMode = configuration["IVR_EXECUTION_MODE"]
            ?? configuration[$"{IvrOptions.SectionName}:{nameof(IvrOptions.ExecutionMode)}"]
            ?? IvrOptions.MockExecutionMode;
        bool isMock = string.Equals(
            executionMode,
            IvrOptions.MockExecutionMode,
            StringComparison.OrdinalIgnoreCase);

        if (isMock)
        {
            // W-0193. Constructed by an explicit factory, not by type.
            //
            // InMemoryFeatureFlagStore has two constructors: one that seeds every environment
            // with its safe default, and one that takes the seeds as an IEnumerable. Registering
            // the type and letting the container choose picks the SECOND one, because the
            // container always selects the greediest constructor whose parameters it can resolve
            // and IEnumerable<T> always resolves - to an empty sequence when nothing is
            // registered. The store then holds no environments at all, every read throws
            // IVR_NOT_FOUND, and the platform's fail-closed fallback reports the provider as
            // unreadable. Safe, but wrong, and invisible: the console cannot read or change a
            // runtime flag in the mode every non-production deployment runs in.
            //
            // Naming the constructor is what makes the seeding a decision rather than a
            // side effect of overload resolution.
            services.TryAddSingleton(
                provider => new InMemoryFeatureFlagStore(
                    provider.GetRequiredService<IAuditLogger>()));
            services.TryAddSingleton<IFeatureFlagStore>(
                provider => provider.GetRequiredService<InMemoryFeatureFlagStore>());
            services.TryAddSingleton<IFeatureFlagCommandIdempotency,
                FeatureFlagCommandIdempotency>();
            services.TryAddSingleton<IRuntimeSafetyHealth,
                HealthyInMemoryRuntimeSafety>();
        }
        else
        {
            services.TryAddSingleton<IFeatureFlagStore, PostgresFeatureFlagStore>();
            services.TryAddSingleton<IFeatureFlagCommandIdempotency,
                PostgresFeatureFlagCommandIdempotency>();
            services.TryAddSingleton<IRuntimeSafetyHealth,
                PostgresRuntimeSafetyHealth>();
        }

        services.TryAddSingleton<FeatureFlagPlatform>();
        services.TryAddSingleton<IFeatureFlags>(
            provider => provider.GetRequiredService<FeatureFlagPlatform>());
        services.TryAddSingleton<IDynamicConfig>(
            provider => provider.GetRequiredService<FeatureFlagPlatform>());
        services.TryAddSingleton<IFeatureFlagRefresher>(
            provider => provider.GetRequiredService<FeatureFlagPlatform>());
        services.TryAddSingleton<IKillSwitch, KillSwitch>();
        services.TryAddSingleton<IDispatchGate, DispatchGate>();
        services.TryAddSingleton<IFeatureFlagAdminService, FeatureFlagAdminService>();
        // W-0195 / OD-V1-20. These three used to be Pending* classes returning a hard-coded
        // false, which was the right answer while no permission existed to move a runtime gate.
        // They now read ivr_runtime_gate_approvals, so the answer lives in a row an auditor can
        // read and revoke. A gate that cannot reach the table still answers no.
        services.TryAddSingleton<IRuntimeGateAuthorization,
            PostgresRuntimeGateAuthorization>();
        services.TryAddSingleton<IFourEyesApprovalVerifier,
            PostgresFourEyesApprovalVerifier>();
        services.TryAddSingleton<IProductionCallGate, PostgresProductionCallGate>();
        return services;
    }
}

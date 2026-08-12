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
            services.TryAddSingleton<InMemoryFeatureFlagStore>();
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
        services.TryAddSingleton<IRuntimeGateAuthorization,
            PendingRuntimeGateAuthorization>();
        services.TryAddSingleton<IFourEyesApprovalVerifier,
            PendingFourEyesApprovalVerifier>();
        services.TryAddSingleton<IProductionCallGate, PendingProductionCallGate>();
        return services;
    }
}

using Ivr.Domain.Retention;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ivr.Infrastructure.Retention;

/// <summary>
/// Registers the fail-closed P1-5 retention platform.
/// </summary>
public static class RetentionServiceCollectionExtensions
{
    /// <summary>
    /// Adds retention policy resolution, resumable execution and privacy-safe telemetry.
    /// </summary>
    public static IServiceCollection AddIvrRetention(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<RetentionOptions>()
            .Bind(configuration.GetSection(RetentionOptions.SectionName));
        services.TryAddSingleton<IRetentionPolicyProvider, ConfiguredRetentionPolicyProvider>();
        services.TryAddSingleton<IRetentionTelemetry, RetentionTelemetry>();
        services.TryAddSingleton<IRetentionJob, RetentionJob>();
        return services;
    }
}

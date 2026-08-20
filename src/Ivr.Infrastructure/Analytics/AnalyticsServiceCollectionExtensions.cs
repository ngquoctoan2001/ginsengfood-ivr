using Ivr.Domain.Retention;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ivr.Infrastructure.Analytics;

public static class AnalyticsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the P10-4 pipeline (<c>W-0055</c>).
    ///
    /// <para>The retention hook is registered <b>unconditionally</b>, while the ETL
    /// job itself is only ever run by whoever asks for it. That asymmetry is
    /// deliberate: a deployment that stops loading facts is a reporting outage, but
    /// a deployment that stops deleting them is a retention breach, and the second
    /// must not be switchable by forgetting to enable something.</para>
    /// </summary>
    public static IServiceCollection AddIvrAnalyticsPipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddOptions<AnalyticsEtlOptions>()
            .Bind(configuration.GetSection(AnalyticsEtlOptions.SectionName))
            .Validate(
                value => value.BatchSize is >= 1 and <= 50_000,
                $"{AnalyticsEtlOptions.SectionName}:BatchSize must be between 1 and 50,000.")
            .Validate(
                value => value.IntervalSeconds is >= 10 and <= 86_400,
                $"{AnalyticsEtlOptions.SectionName}:IntervalSeconds must be between 10 and 86,400.")
            .ValidateOnStart();
        services.TryAddSingleton<IAnalyticsEtlJob, AnalyticsEtlJob>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IRetentionPurgeHook, AnalyticsRetentionHook>());
        return services;
    }
}

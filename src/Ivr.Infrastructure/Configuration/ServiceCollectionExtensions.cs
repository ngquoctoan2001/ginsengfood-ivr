using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Evidence;
using Ivr.Infrastructure.Idempotency;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Channels;
using Ivr.Infrastructure.Persistence.Outbox;
using Ivr.Infrastructure.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Configuration;

/// <summary>
/// Registers the infrastructure required by the foundation phase.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIvrFoundation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        IConfigurationSection section = configuration.GetSection(IvrOptions.SectionName);
        string executionMode = GetValue(
            configuration,
            section,
            "IVR_EXECUTION_MODE",
            nameof(IvrOptions.ExecutionMode),
            IvrOptions.MockExecutionMode);

        services.AddOptions<IvrOptions>()
            .Configure(options =>
            {
                options.ExecutionMode = executionMode;
                options.SalesProvider = GetValue(
                    configuration,
                    section,
                    "SALES_PROVIDER",
                    nameof(IvrOptions.SalesProvider),
                    "FAKE_TARGET_V1");
                options.SimProvider = GetValue(
                    configuration,
                    section,
                    "SIM_PROVIDER",
                    nameof(IvrOptions.SimProvider),
                    "MOCK");
                options.ConnectionString = configuration.GetConnectionString("IvrDb")
                    ?? section[nameof(IvrOptions.ConnectionString)]
                    ?? string.Empty;
                options.RealCustomerCallAllowed = string.Equals(
                    configuration["REAL_CUSTOMER_CALL_ALLOWED"]
                        ?? section[nameof(IvrOptions.RealCustomerCallAllowed)],
                    "YES",
                    StringComparison.OrdinalIgnoreCase);
            })
            .ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<IvrOptions>, IvrOptionsValidator>());

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton<ICorrelationContext, CorrelationContext>();
        services.AddTransient<CorrelationPropagationHandler>();
        services.ConfigureHttpClientDefaults(
            clientBuilder => clientBuilder.AddHttpMessageHandler<CorrelationPropagationHandler>());

        if (string.Equals(
                executionMode,
                IvrOptions.MockExecutionMode,
                StringComparison.OrdinalIgnoreCase))
        {
            services.TryAddSingleton<InMemoryIdempotencyStore>();
            services.TryAddSingleton<IIdempotencyStore>(
                provider => provider.GetRequiredService<InMemoryIdempotencyStore>());
            services.TryAddSingleton<InMemoryAuditLogger>();
            services.TryAddSingleton<IAuditLogger>(
                provider => provider.GetRequiredService<InMemoryAuditLogger>());
            services.TryAddSingleton<InMemoryEvidenceStore>();
            services.TryAddSingleton<IEvidenceStore>(
                provider => provider.GetRequiredService<InMemoryEvidenceStore>());
        }
        else
        {
            services.TryAddSingleton<PostgresIdempotencyStore>();
            services.TryAddSingleton<IIdempotencyStore>(
                provider => provider.GetRequiredService<PostgresIdempotencyStore>());
            services.TryAddSingleton<PostgresAuditLogger>();
            services.TryAddSingleton<IAuditLogger>(
                provider => provider.GetRequiredService<PostgresAuditLogger>());
            services.TryAddSingleton<PostgresEvidenceStore>();
            services.TryAddSingleton<IEvidenceStore>(
                provider => provider.GetRequiredService<PostgresEvidenceStore>());
        }

        services.AddDbContextFactory<IvrDbContext>((serviceProvider, dbContextOptions) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<IvrOptions>>().Value;
            dbContextOptions.UseNpgsql(options.ConnectionString);
        });
        services.TryAddSingleton<FeatureFlagPersistenceSession>();
        services.TryAddSingleton<IOpaqueValueProtector, UnavailableOpaqueValueProtector>();
        services.TryAddSingleton<ICallbackOutboxRepository, CallbackOutboxRepository>();
        services.TryAddSingleton<ISimChannelLeaseRepository, SimChannelLeaseRepository>();

        return services;
    }

    private static string GetValue(
        IConfiguration configuration,
        IConfigurationSection section,
        string environmentKey,
        string sectionKey,
        string fallback) =>
        configuration[environmentKey]
            ?? section[sectionKey]
            ?? fallback;
}

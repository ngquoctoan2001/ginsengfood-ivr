using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Contracts;
using Ivr.Infrastructure.Evidence;
using Ivr.Infrastructure.Idempotency;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Outbox;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Scripts;
using Ivr.Domain.Scripts;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
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
        IConfiguration configuration,
        bool useInMemoryTestDoubles = false)
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
        services.AddOptions<ScriptContentOptions>()
            .Configure(options =>
            {
                options.ProductionTargetV1FieldsApproved = string.Equals(
                    configuration["IVR_PRODUCTION_TARGET_V1_FIELDS_APPROVED"]
                        ?? section[nameof(ScriptContentOptions.ProductionTargetV1FieldsApproved)],
                    "YES",
                    StringComparison.OrdinalIgnoreCase);
            });

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton(SpeechSummaryLimits.Create(100, 100));
        services.TryAddSingleton<TargetV1TaskMapper>();
        services.TryAddSingleton<ITaskIntakeService, TaskIntakeService>();
        services.TryAddSingleton<ICorrelationContext, CorrelationContext>();
        services.AddTransient<CorrelationPropagationHandler>();
        services.ConfigureHttpClientDefaults(
            clientBuilder => clientBuilder.AddHttpMessageHandler<CorrelationPropagationHandler>());

        if (useInMemoryTestDoubles)
        {
            if (!string.Equals(
                    executionMode,
                    IvrOptions.MockExecutionMode,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "In-memory foundation test doubles are allowed in MOCK execution only.");
            }

            services.TryAddSingleton<InMemoryIdempotencyStore>();
            services.TryAddSingleton<IIdempotencyStore>(
                provider => provider.GetRequiredService<InMemoryIdempotencyStore>());
            services.TryAddSingleton<InMemoryAuditLogger>();
            services.TryAddSingleton<IAuditLogger>(
                provider => provider.GetRequiredService<InMemoryAuditLogger>());
            services.TryAddSingleton<InMemoryEvidenceStore>();
            services.TryAddSingleton<IEvidenceStore>(
                provider => provider.GetRequiredService<InMemoryEvidenceStore>());
            services.TryAddSingleton<InMemoryScriptRegistry>();
            services.TryAddSingleton<IScriptRegistry>(
                provider => provider.GetRequiredService<InMemoryScriptRegistry>());
            services.TryAddSingleton<IScriptContentManager>(
                provider => provider.GetRequiredService<InMemoryScriptRegistry>());
            services.TryAddSingleton<IAttemptPolicyRegistry>(_ =>
                new FakeAttemptPolicyRegistry(CandidateAttemptPolicies.Create()));
            services.TryAddSingleton<InMemoryTaskIntakeStore>();
            services.TryAddSingleton<ITaskIntakeStore>(provider =>
                provider.GetRequiredService<InMemoryTaskIntakeStore>());
            services.TryAddSingleton<IEligibilityRepository>(provider =>
                provider.GetRequiredService<InMemoryTaskIntakeStore>());
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
            services.TryAddSingleton<PostgresScriptRegistry>();
            services.TryAddSingleton<IScriptRegistry>(
                provider => provider.GetRequiredService<PostgresScriptRegistry>());
            services.TryAddSingleton<IScriptContentManager>(
                provider => provider.GetRequiredService<PostgresScriptRegistry>());
            services.TryAddSingleton<IAttemptPolicyRegistry, PostgresAttemptPolicyRegistry>();
            services.TryAddSingleton<IAttemptPolicyRegistryWriter,
                PostgresAttemptPolicyRegistryWriter>();
            services.TryAddSingleton<ITaskIntakeStore, PostgresTaskIntakeStore>();
            services.TryAddSingleton<IEligibilityRepository,
                PostgresEligibilityRepository>();
        }

        services.AddDbContextFactory<IvrDbContext>((serviceProvider, dbContextOptions) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<IvrOptions>>().Value;
            dbContextOptions.UseNpgsql(options.ConnectionString);
        });
        services.TryAddSingleton<FeatureFlagPersistenceSession>();
        if (string.Equals(
                executionMode,
                IvrOptions.MockExecutionMode,
                StringComparison.OrdinalIgnoreCase))
        {
            services.TryAddSingleton<IOpaqueValueProtector, MockOnlyOpaqueValueProtector>();
        }
        else
        {
            services.TryAddSingleton<IOpaqueValueProtector, UnavailableOpaqueValueProtector>();
        }
        services.TryAddSingleton<ICallbackOutboxRepository, CallbackOutboxRepository>();
        services.TryAddSingleton<IScriptPreviewRenderer, VietnameseOrderScriptRenderer>();
        services.AddIvrScheduling(
            configuration,
            executionMode,
            string.Equals(
                executionMode,
                IvrOptions.MockExecutionMode,
                StringComparison.OrdinalIgnoreCase));

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

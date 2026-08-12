using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.Configure<IvrOptions>(options =>
        {
            var section = configuration.GetSection(IvrOptions.SectionName);

            options.ExecutionMode = GetValue(
                configuration,
                section,
                "IVR_EXECUTION_MODE",
                nameof(IvrOptions.ExecutionMode),
                "MOCK");
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
        });

        services.AddDbContext<IvrDbContext>((serviceProvider, dbContextOptions) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<IvrOptions>>().Value;
            dbContextOptions.UseNpgsql(options.ConnectionString);
        });

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

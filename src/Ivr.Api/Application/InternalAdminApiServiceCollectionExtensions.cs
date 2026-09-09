using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Api.Internal;
using Ivr.Domain.Errors;
using Ivr.Domain.Policies;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.DevTooling;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Idempotency;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Ivr.Domain.Confirmation;

namespace Ivr.Api.Application;

public static class InternalAdminApiServiceCollectionExtensions
{
    private static int ReadInt(IConfigurationSection section, string key, int fallback) =>
        int.TryParse(
            section[key],
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out int parsed)
                ? parsed
                : fallback;

    public static IServiceCollection AddIvrInternalAdminApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddOptions<InternalServiceOptions>()
            .Configure(options =>
            {
                options.ServiceToken = configuration[InternalServiceOptions.TokenConfigurationKey]
                    ?? string.Empty;
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ServiceToken),
                $"{InternalServiceOptions.TokenConfigurationKey} is required.")
            .ValidateOnStart();
        services.Configure<RouteHandlerOptions>(options =>
        {
            options.ThrowOnBadRequest = true;
        });
        // One implementation, two interfaces. The class stays whole because its fifteen methods
        // share the idempotency wrapper, the context factory and the validation helpers; splitting
        // it would duplicate those or invent a third type to hold them. What the callers see is
        // split, which is where the coupling was costing something.
        services.AddSingleton<InternalAdminApiService>();
        services.AddSingleton<IIvrLifecycleApiService>(
            provider => provider.GetRequiredService<InternalAdminApiService>());
        services.AddSingleton<IIvrAdminOperationsService>(
            provider => provider.GetRequiredService<InternalAdminApiService>());
        services.AddSingleton<IAdminReadService, AdminReadService>();
        services.AddSingleton<IAdminConfigReadService, AdminConfigReadService>();
        services.AddSingleton<IAnalyticsReadService, AnalyticsReadService>();
        services.AddSingleton<IScriptLifecycleApiService, ScriptLifecycleApiService>();

        // W-0112. Registered unconditionally; the routes are what production refuses to map.
        // Keeping the service available means NonProductionSurface is exercised by the same
        // code in every environment, rather than by a registration branch nobody runs.
        IConfigurationSection devSection = configuration.GetSection(DevToolingOptions.SectionName);
        services.AddOptions<DevToolingOptions>()
            .Configure<IHostEnvironment>((options, environment) =>
            {
                // W-0193. A relative path is resolved against the content root, not the process
                // working directory.
                //
                // The option is a filesystem path, and `dotnet run`, `dotnet test` and the
                // container image each start the process in a different directory. Anchoring on
                // the content root is what lets one committed value ("../../seed") work in all
                // three instead of working in whichever one it was last tried in.
                options.SeedDirectory = ResolveSeedDirectory(
                    devSection[nameof(DevToolingOptions.SeedDirectory)],
                    environment.ContentRootPath);
                options.ScenarioWindowSeconds = ReadInt(
                    devSection,
                    nameof(DevToolingOptions.ScenarioWindowSeconds),
                    options.ScenarioWindowSeconds);
                options.ScenarioTechnicalRetryLimit = ReadInt(
                    devSection,
                    nameof(DevToolingOptions.ScenarioTechnicalRetryLimit),
                    options.ScenarioTechnicalRetryLimit);
                options.MaximumSeedTasks = ReadInt(
                    devSection,
                    nameof(DevToolingOptions.MaximumSeedTasks),
                    options.MaximumSeedTasks);
            })
            .ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<DevToolingOptions>, DevToolingOptionsValidator>());
        services.AddSingleton<SeedCatalog>();
        services.AddSingleton<IDevToolingApiService, DevToolingApiService>();
        return services;
    }

    /// <summary>
    /// W-0193. Turns the configured seed path into one the process can actually open.
    /// <para>
    /// Empty stays empty: an unconfigured seed directory disables the developer surface, and that
    /// is the correct default outside development. An absolute path is taken as given. Only a
    /// relative path is rewritten, and it is anchored on the content root so the same committed
    /// value resolves identically however the process was started.
    /// </para>
    /// </summary>
    internal static string ResolveSeedDirectory(string? configured, string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            return string.Empty;
        }

        string trimmed = configured.Trim();
        return Path.IsPathRooted(trimmed)
            ? trimmed
            : Path.GetFullPath(Path.Combine(contentRootPath, trimmed));
    }
}

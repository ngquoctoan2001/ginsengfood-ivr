using Ivr.Contracts.Sales.CurrentGoldenHour;
using Ivr.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Callbacks;

public static class CallbackServiceCollectionExtensions
{
    public static IServiceCollection AddIvrCallbackDelivery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        IConfigurationSection section = configuration.GetSection(
            CallbackDeliveryOptions.SectionName);
        IConfigurationSection ivrSection = configuration.GetSection("Ivr");
        services.AddOptions<CallbackDeliveryOptions>()
            .Bind(section)
            .Configure(options => options.Provider = configuration["SALES_PROVIDER"]
                ?? ivrSection["SalesProvider"]
                ?? options.Provider)
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<CallbackDeliveryOptions>, CallbackDeliveryOptionsValidator>());
        services.TryAddSingleton<IServiceTokenProvider, RefreshingMockServiceTokenProvider>();
        services.TryAddSingleton<CallbackCircuitBreaker>();
        services.TryAddSingleton<ICurrentGoldenHourIdentityResolver,
            ConfiguredCurrentGoldenHourIdentityResolver>();
        services.AddHttpClient<ITargetV1CallbackTransport, TargetV1CallbackTransport>(
            (provider, client) =>
            {
                CallbackDeliveryOptions options = provider
                    .GetRequiredService<IOptions<CallbackDeliveryOptions>>().Value;
                client.BaseAddress = new Uri(options.TargetBaseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
            });
        services.AddHttpClient<ICurrentGoldenHourCallbackClient, CurrentGoldenHourCallbackClient>(
            (provider, client) =>
            {
                CallbackDeliveryOptions options = provider
                    .GetRequiredService<IOptions<CallbackDeliveryOptions>>().Value;
                client.BaseAddress = new Uri(options.CurrentGoldenHourBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
            });
        services.TryAddSingleton<ICurrentGoldenHourCallbackTransport,
            CurrentGoldenHourCallbackTransport>();
        services.TryAddSingleton<CallbackDispatcher>();
        return services;
    }
}

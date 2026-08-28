using System.Text.Json.Serialization;
using Ivr.Api.Auth;
using Ivr.Api.Health;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Auth;
using Ivr.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Foundation;

public static class IvrApiServiceCollectionExtensions
{
    public const string RegisterMockPermissionProviderKey =
        "REGISTER_MOCK_PERMISSION_PROVIDER";

    public static IServiceCollection AddIvrApiFoundation(
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
        bool registerMockProvider = configuration.GetValue<bool?>(
                RegisterMockPermissionProviderKey)
            ?? isMock;

        if (registerMockProvider && !isMock)
        {
            throw new InvalidOperationException(
                "The mock permission provider may only be registered in MOCK mode.");
        }

        // W-0122. Bound from configuration keys rather than a section so the three secrets can
        // arrive as plain environment variables, which is how every other IVR credential is
        // delivered. A tier left unset stays empty and its token never matches — fail-closed.
        services.AddOptions<AdminAccessOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.ReadToken =
                    configuration[AdminAccessOptions.ReadTokenConfigurationKey] ?? string.Empty;
                options.WriteToken =
                    configuration[AdminAccessOptions.WriteTokenConfigurationKey] ?? string.Empty;
                options.DangerToken =
                    configuration[AdminAccessOptions.DangerTokenConfigurationKey] ?? string.Empty;
            });

        // W-0122. One scheme, three policies. The console account system it replaced needed a
        // scheme selector, a session handler and a policy per permission because it authenticated
        // people; this authenticates a peer service, and Module 3 owns the operators now.
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AdminTokenAuthenticationHandler.SchemeName;
            options.DefaultChallengeScheme = AdminTokenAuthenticationHandler.SchemeName;
            options.DefaultForbidScheme = AdminTokenAuthenticationHandler.SchemeName;
        }).AddScheme<AuthenticationSchemeOptions, AdminTokenAuthenticationHandler>(
            AdminTokenAuthenticationHandler.SchemeName,
            _ => { });

        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationHandler, AdminScopeAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            foreach (AdminScope scope in Enum.GetValues<AdminScope>())
            {
                options.AddPolicy(
                    AdminPolicies.NameOf(scope),
                    policy => policy
                        .AddAuthenticationSchemes(AdminTokenAuthenticationHandler.SchemeName)
                        .RequireAuthenticatedUser()
                        .AddRequirements(new AdminScopeRequirement(scope)));
            }
        });

        services.AddSingleton<IAuthorizationMiddlewareResultHandler,
            IvrAuthorizationMiddlewareResultHandler>();
        services.AddSingleton<IvrErrorResponseWriter>();
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddOptions<OrderCoreAllowlistOptions>()
            .Configure(options =>
            {
                options.ServiceToken = configuration[
                        OrderCoreAllowlistOptions.TokenConfigurationKey]
                    ?? string.Empty;
            })
            .Configure(options =>
            {
                // W-0047 / P7-5. The value being rotated out, and the instant it stops counting.
                options.PreviousServiceToken = configuration[
                        OrderCoreAllowlistOptions.PreviousTokenConfigurationKey]
                    ?? string.Empty;
                string? retiresAt = configuration[
                    OrderCoreAllowlistOptions.PreviousTokenRetiresAtConfigurationKey];
                options.PreviousServiceTokenRetiresAt =
                    DateTimeOffset.TryParse(
                        retiresAt,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal
                            | System.Globalization.DateTimeStyles.AdjustToUniversal,
                        out DateTimeOffset parsed)
                        ? parsed
                        : null;
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ServiceToken),
                $"{OrderCoreAllowlistOptions.TokenConfigurationKey} is required.")
            .Validate(
                // A previous value with no retirement instant would be accepted until somebody
                // remembered to delete the variable, which is the rotation that never finishes.
                options => string.IsNullOrWhiteSpace(options.PreviousServiceToken)
                    || options.PreviousServiceTokenRetiresAt is not null,
                $"{OrderCoreAllowlistOptions.PreviousTokenRetiresAtConfigurationKey} is required "
                + $"whenever {OrderCoreAllowlistOptions.PreviousTokenConfigurationKey} is set.")
            .ValidateOnStart();

        services.TryAddSingleton<OrderCoreCredentialSource>();

        // W-0032 / P4-4. Service identity. Mock issuer only: ServiceIdentityOptionsValidator
        // refuses Mode=Real at startup, so no deployment can quietly claim production auth.
        services.AddOptions<ServiceIdentityOptions>()
            .Bind(configuration.GetSection(ServiceIdentityOptions.SectionName))
            .ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<ServiceIdentityOptions>, ServiceIdentityOptionsValidator>());
        services.TryAddSingleton<MockOidcIssuer>();
        services.TryAddSingleton<IServiceSigningKeySource>(
            provider => provider.GetRequiredService<MockOidcIssuer>());
        services.TryAddSingleton<IServiceJwtValidator, ServiceJwtValidator>();

        // W-0040 / P6-1. Real readiness replaces the hardcoded probe.
        services.TryAddScoped<IIvrReadinessProbe, IvrReadinessProbe>();

        return services;
    }
}

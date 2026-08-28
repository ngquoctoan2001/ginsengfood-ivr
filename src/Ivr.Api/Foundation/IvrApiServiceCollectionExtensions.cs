using System.Text.Json.Serialization;
using Ivr.Api.Auth;
using Ivr.Api.Health;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Foundation;

public static class IvrApiServiceCollectionExtensions
{
    public static IServiceCollection AddIvrApiFoundation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // W-0128. Bound from configuration keys rather than a section so the three secrets can
        // arrive as plain environment variables, which is how every other IVR credential is
        // delivered. A tier left unset stays empty and its token never matches — fail-closed.
        services.AddOptions<AdminAccessOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.ReadToken =
                    configuration[AdminAccessOptions.ReadTokenConfigurationKey] ?? string.Empty;
                options.ReadTokenPrevious = configuration[
                    AdminAccessOptions.ReadTokenPreviousConfigurationKey] ?? string.Empty;
                options.ReadTokenPreviousRetiresAt = ParseInstant(configuration[
                    AdminAccessOptions.ReadTokenPreviousRetiresAtConfigurationKey]);
                options.WriteToken =
                    configuration[AdminAccessOptions.WriteTokenConfigurationKey] ?? string.Empty;
                options.WriteTokenPrevious = configuration[
                    AdminAccessOptions.WriteTokenPreviousConfigurationKey] ?? string.Empty;
                options.WriteTokenPreviousRetiresAt = ParseInstant(configuration[
                    AdminAccessOptions.WriteTokenPreviousRetiresAtConfigurationKey]);
                options.DangerToken =
                    configuration[AdminAccessOptions.DangerTokenConfigurationKey] ?? string.Empty;
                options.DangerTokenPrevious = configuration[
                    AdminAccessOptions.DangerTokenPreviousConfigurationKey] ?? string.Empty;
                options.DangerTokenPreviousRetiresAt = ParseInstant(configuration[
                    AdminAccessOptions.DangerTokenPreviousRetiresAtConfigurationKey]);
            })
            .Validate(AdminAccessOptionsAreValid, "Admin token rotation configuration is invalid.")
            .ValidateOnStart();

        services.TryAddSingleton<AdminCredentialSource>();

        // W-0128. One scheme, three policies. The console account system it replaced needed a
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

    private static DateTimeOffset? ParseInstant(string? value) =>
        DateTimeOffset.TryParse(
            value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal
                | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out DateTimeOffset parsed)
            ? parsed
            : null;

    private static bool AdminAccessOptionsAreValid(AdminAccessOptions options)
    {
        (string Current, string Previous, DateTimeOffset? RetiresAt)[] tiers =
        [
            (options.ReadToken, options.ReadTokenPrevious, options.ReadTokenPreviousRetiresAt),
            (options.WriteToken, options.WriteTokenPrevious, options.WriteTokenPreviousRetiresAt),
            (options.DangerToken, options.DangerTokenPrevious, options.DangerTokenPreviousRetiresAt),
        ];
        string[] configured = tiers
            .SelectMany(tier => new[] { tier.Current, tier.Previous })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        return tiers.All(tier =>
                (string.IsNullOrWhiteSpace(tier.Current)
                    || tier.Current.Length >= RotatingCredentialProvider.MinimumSecretLength)
                && (string.IsNullOrWhiteSpace(tier.Previous)
                    || tier.Previous.Length >= RotatingCredentialProvider.MinimumSecretLength)
                && (string.IsNullOrWhiteSpace(tier.Previous)
                    || (!string.IsNullOrWhiteSpace(tier.Current) && tier.RetiresAt is not null)))
            && configured.Distinct(StringComparer.Ordinal).Count() == configured.Length;
    }
}

using System.Text.Json.Serialization;
using Ivr.Api.Accounts;
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

        AuthenticationBuilder authentication = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "IvrAuthenticationSelector";
            options.DefaultChallengeScheme = "IvrAuthenticationSelector";
            options.DefaultForbidScheme = "IvrAuthenticationSelector";
        });

        authentication.AddPolicyScheme(
            "IvrAuthenticationSelector",
            "IVR authentication selector",
            options => options.ForwardDefaultSelector = context =>
            {
                string authorization = context.Request.Headers.Authorization.ToString();
                if (authorization.StartsWith(
                    "Bearer ivr_session_",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return ConsoleSessionAuthenticationHandler.SchemeName;
                }

                return registerMockProvider
                    ? MockPermissionAuthenticationHandler.SchemeName
                    : FailClosedAuthenticationHandler.SchemeName;
            });
        authentication.AddScheme<AuthenticationSchemeOptions, ConsoleSessionAuthenticationHandler>(
            ConsoleSessionAuthenticationHandler.SchemeName,
            _ => { });
        authentication.AddScheme<AuthenticationSchemeOptions, FailClosedAuthenticationHandler>(
            FailClosedAuthenticationHandler.SchemeName,
            _ => { });

        if (registerMockProvider)
        {
            authentication.AddScheme<AuthenticationSchemeOptions, MockPermissionAuthenticationHandler>(
                MockPermissionAuthenticationHandler.SchemeName,
                _ => { });
        }
        services.AddAuthorization(options =>
        {
            foreach (string permission in IvrPermissions.All)
            {
                options.AddPolicy(
                    permission,
                    policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            }

            // Existing MOCK permission tests are not user-console sessions. In a real console
            // bearer session this policy narrows broad legacy QueueView reads to Admin only.
            options.AddPolicy(
                IvrRoles.ConsoleAdminPolicy,
                policy => policy.RequireAssertion(context =>
                    context.User.Identity?.AuthenticationType
                        != ConsoleSessionAuthenticationHandler.SchemeName
                    || context.User.IsInRole(IvrRoles.Admin)));

            // W-0105. Naming the scheme makes the authorization middleware authenticate the
            // request through the console handler alone. A caller presenting the MOCK
            // X-Permissions header and no bearer token therefore arrives unauthenticated
            // (401) rather than carrying whatever authority it wrote in that header.
            options.AddPolicy(
                IvrRoles.ConsoleSessionPolicy,
                policy => policy
                    .AddAuthenticationSchemes(ConsoleSessionAuthenticationHandler.SchemeName)
                    .RequireAuthenticatedUser());
        });
        services.AddSingleton<IPermissionEvaluator, ClaimsPermissionEvaluator>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler,
            IvrAuthorizationMiddlewareResultHandler>();
        services.AddSingleton<IvrErrorResponseWriter>();
        services.AddSingleton<ConsoleAccountService>();
        services.AddSingleton<ConsoleSignInRateLimiter>();
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
